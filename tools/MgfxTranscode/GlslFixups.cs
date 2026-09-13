using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace UW.Tools.MgfxTranscode;

/// <summary>
/// Repairs GLSL that MonoGame's shader compiler emits but no OpenGL driver will accept.
///
/// WHY THIS HAS TO EXIST. On the OpenGL profile an MGFX shader block holds GLSL SOURCE TEXT,
/// not bytecode, and that text is compiled by the DRIVER the first time the shader is used.
/// So nothing in the build can tell you it is wrong: mgfxc succeeds, the container validates,
/// the XNB loads, every parameter binds - and then the draw call silently fails. In this port
/// that presented as the world and every character missing while the interface rendered
/// perfectly, with no exception and nothing in Errors.txt.
///
/// THE BUG. mgfxc 3.8.5.1 declares a shader's boolean constants as a vec4 array, which is what
/// MonoGame's own uniform upload requires - it pushes every constant buffer with a single
/// glUniform4fv - and then emits the register as a bare alias to one of those vec4s:
///
///     uniform vec4 ps_uniforms_bool[1];
///     #define ps_b0 ps_uniforms_bool[0]
///     ...
///     if (ps_b0) {                       // error C1019: scalar Boolean expression expected
///
/// A vec4 is not a condition. Every `if (boolUniform)` in a shader is therefore uncompilable.
/// This is the second half of a bug MonoGame fixed only partly: 3.8.2-3.8.4.1 REJECTED
/// `if (boolUniform)` in a Shader Model 3 shader outright ("IF src0 must have replicate
/// swizzle"), and 3.8.5.1 stopped rejecting it without making the generated GLSL valid.
///
/// THE REPAIR is at the use site rather than in the #define: the macro keeps its vec4 value,
/// so any arithmetic use of the register is untouched, and only the boolean context is
/// corrected. The value really is a float 1.0 or 0.0 - EffectParameter stores bools in a
/// float[] and SetValue(bool) writes 1 or 0 - so comparing the x component against zero is
/// exactly the test the HLSL asked for, with no denormal or bit-pattern subtlety.
///
/// It is applied by `inject`, which every OpenGL effect passes through, and it reports what it
/// changed rather than repairing silently.
/// </summary>
internal static class GlslFixups
{
    /// <summary>
    /// `if (ps_b0)` / `if (!ps_b0)`, in any spacing mgfxc might use. Deliberately anchored on
    /// the whole condition being the register alone: a compound condition would mean MojoShader
    /// had generated something this was not written for, and it is better to leave that
    /// untouched and let <see cref="ResidualBareUse"/> report it.
    /// </summary>
    private static readonly Regex BoolCondition = new(
        @"\bif\s*\(\s*(?<not>!?)\s*(?<reg>(?:vs|ps)_b\d+)\s*\)",
        RegexOptions.Compiled);

    /// <summary>The `#define ps_b0 ps_uniforms_bool[0]` alias line, which is left alone.</summary>
    private static readonly Regex BoolDefine = new(
        @"^\s*#define\s+(?:vs|ps)_b\d+\b.*$",
        RegexOptions.Compiled | RegexOptions.Multiline);

    /// <summary>A use of a boolean register that is neither the #define nor a repaired condition.</summary>
    private static readonly Regex BareUse = new(@"\b(?:vs|ps)_b\d+\b(?!\s*\.x)", RegexOptions.Compiled);

    /// <summary>What one call changed, for reporting.</summary>
    internal readonly record struct Result(byte[] Blob, int ShadersPatched, int ConditionsFixed)
    {
        public override string ToString() =>
            ConditionsFixed == 0
                ? "no GLSL fixups needed"
                : $"{ConditionsFixed} boolean condition(s) repaired in {ShadersPatched} shader(s)";
    }

    /// <summary>
    /// Applies every fixup to an MGFX blob, returning a new blob. A blob that needs nothing is
    /// returned unchanged, byte for byte.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// If a boolean register is still used somewhere this does not understand after the repair -
    /// which would mean shipping GLSL that still will not compile.
    /// </exception>
    public static Result Apply(byte[] blob)
    {
        // Profile lives at offset 5, after the 4-byte signature and the version byte. 0 is
        // OpenGL; a DirectX blob holds real bytecode and must never be touched as text.
        if (blob.Length < 6 || blob[5] != 0)
        {
            return new Result(blob, 0, 0);
        }

        List<MgfxV10Reader.ShaderBlock> blocks = MgfxV10Reader.ReadShaders(blob);

        var replacements = new List<(int LengthFieldOffset, int Start, int Length, byte[] Payload)>();
        int conditionsFixed = 0;

        foreach (MgfxV10Reader.ShaderBlock block in blocks)
        {
            string glsl = Encoding.ASCII.GetString(block.Bytecode);
            int fixedHere = 0;

            string repaired = BoolCondition.Replace(glsl, match =>
            {
                fixedHere++;
                string reg = match.Groups["reg"].Value;
                string op = match.Groups["not"].Value == "!" ? "==" : "!=";
                return $"if ({reg}.x {op} 0.0)";
            });

            if (fixedHere == 0)
            {
                continue;
            }

            string residual = ResidualBareUse(repaired);
            if (residual != null)
            {
                throw new InvalidDataException(
                    "A boolean shader register is used in a way this fixup does not handle: " +
                    residual + ". Repairing the condition alone would leave GLSL the driver " +
                    "still rejects, so nothing was written.");
            }

            conditionsFixed += fixedHere;
            replacements.Add((block.LengthFieldOffset, block.BytecodeOffset, block.Bytecode.Length,
                Encoding.ASCII.GetBytes(repaired)));
        }

        if (replacements.Count == 0)
        {
            return new Result(blob, 0, 0);
        }

        return new Result(Splice(blob, replacements), replacements.Count, conditionsFixed);
    }

    /// <summary>
    /// The first line still naming a boolean register outside its #define and outside a
    /// component access, or null if there is none.
    /// </summary>
    private static string ResidualBareUse(string glsl)
    {
        string withoutDefines = BoolDefine.Replace(glsl, string.Empty);
        Match m = BareUse.Match(withoutDefines);
        if (!m.Success)
        {
            return null;
        }

        int lineStart = withoutDefines.LastIndexOf('\n', Math.Max(0, m.Index - 1)) + 1;
        int lineEnd = withoutDefines.IndexOf('\n', m.Index);
        if (lineEnd < 0) lineEnd = withoutDefines.Length;
        return withoutDefines[lineStart..lineEnd].Trim();
    }

    /// <summary>
    /// Rebuilds the blob with each shader's payload replaced and its int32 length prefix
    /// rewritten. Safe because MGFX stores no absolute offsets - every field is either
    /// length-prefixed or read in sequence - so everything after a replacement simply shifts.
    /// </summary>
    private static byte[] Splice(
        byte[] blob, List<(int LengthFieldOffset, int Start, int Length, byte[] Payload)> replacements)
    {
        replacements.Sort((a, b) => a.Start.CompareTo(b.Start));

        using var output = new MemoryStream(blob.Length + 256);
        int copied = 0;

        foreach ((int lengthFieldOffset, int start, int length, byte[] payload) in replacements)
        {
            // Everything up to the length prefix, verbatim.
            output.Write(blob, copied, lengthFieldOffset - copied);
            output.Write(BitConverter.GetBytes(payload.Length), 0, 4);

            // The four bytes of the old prefix are skipped, not copied: start is already past it.
            output.Write(payload, 0, payload.Length);
            copied = start + length;
        }

        output.Write(blob, copied, blob.Length - copied);
        return output.ToArray();
    }
}
