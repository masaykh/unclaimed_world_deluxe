using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace UW.Tools.MgfxDxToGl;

/// <summary>
/// Decodes the SM4/SM5 instruction stream into operands, and prints it.
///
/// Two jobs. The immediate one is recovering constant VALUES that exist only in the DX11
/// bytecode: where fxc lifted an immediate into a constant register for the SM2 fallback, the
/// DX11 shader still encodes it inline, so reading it here turns an unbindable uniform into a
/// compile-time constant (see GlConverter's handling of DevShape and
/// TimeOfDayAndLightsources). The longer-term one is that operand decoding is the hard half of
/// an SM4-to-GLSL translator, which is the only route left for the three effects with no Aon9
/// chunk at all.
///
/// Operand encoding, from the DXBC/SM4 spec:
///
///   bits 0-1    number of components (0 = one, 1 = four, 2 = N)
///   bits 2-3    component selection (0 = mask, 1 = swizzle, 2 = select-one)
///   bits 4-11   the mask / swizzle / selection itself
///   bits 12-19  operand type (temp, input, output, immediate, sampler, resource, cbuffer, ...)
///   bits 20-21  index dimension (how many index operands follow)
///   bits 22-30  how each of up to three indices is represented
///   bit  31     an extended operand token follows (carries neg/abs modifiers)
/// </summary>
internal static class Sm4Disassembler
{
    public enum OperandType
    {
        Temp = 0, Input = 1, Output = 2, IndexableTemp = 3, Immediate32 = 4, Immediate64 = 5,
        Sampler = 6, Resource = 7, ConstantBuffer = 8, ImmediateConstantBuffer = 9,
        Label = 10, InputPrimitiveId = 11, OutputDepth = 12, Null = 13,
    }

    internal sealed class Operand
    {
        public OperandType Type;
        public int NumComponents;          // 0 = one, 1 = four, 2 = N
        public int SelectionMode;          // 0 = mask, 1 = swizzle, 2 = select-one
        public int MaskOrSwizzle;
        public List<long> Indices = new();
        /// <summary>Set for an Immediate32 operand: the literal dwords, as floats and as raw bits.</summary>
        public float[] ImmediateFloats;
        public uint[] ImmediateBits;
        public int Modifier;               // 0 none, 1 neg, 2 abs, 3 abs+neg

        /// <summary>The register operand added to <see cref="Indices"/>, for relative addressing.</summary>
        public Operand RelativeIndex;

        /// <summary>Which of the indices <see cref="RelativeIndex"/> applies to.</summary>
        public int RelativeIndexDimension = -1;

        public override string ToString()
        {
            if (Type == OperandType.Immediate32 && ImmediateFloats != null)
            {
                var parts = new string[ImmediateFloats.Length];
                for (int i = 0; i < ImmediateFloats.Length; i++)
                    parts[i] = Fmt(ImmediateFloats[i], ImmediateBits[i]);
                return "(" + string.Join(", ", parts) + ")";
            }

            var sb = new StringBuilder();
            if (Modifier == 1 || Modifier == 3) sb.Append('-');
            if (Modifier == 2 || Modifier == 3) sb.Append("|");
            sb.Append(Type switch
            {
                OperandType.Temp => "r",
                OperandType.Input => "v",
                OperandType.Output => "o",
                OperandType.IndexableTemp => "x",
                OperandType.Sampler => "s",
                OperandType.Resource => "t",
                OperandType.ConstantBuffer => "cb",
                OperandType.OutputDepth => "oDepth",
                OperandType.Null => "null",
                _ => Type.ToString(),
            });
            for (int i = 0; i < Indices.Count; i++)
            {
                string rel = (RelativeIndexDimension == i && RelativeIndex != null)
                    ? RelativeIndex + " + "
                    : "";
                string text = rel + Indices[i].ToString(CultureInfo.InvariantCulture);
                sb.Append(i == 0 && rel.Length == 0 ? text : "[" + text + "]");
            }
            sb.Append(Suffix());
            if (Modifier == 2 || Modifier == 3) sb.Append("|");
            return sb.ToString();
        }

        private static string Fmt(float f, uint bits)
        {
            // SM4 is typed, and an integer operand's bits reinterpreted as a float is a tiny
            // denormal. Printing that through a float format rounds it to "0", which hid a
            // `ishl r1, v3, l(2)` - the matrix-palette stride - as `ishl r1, v3, (0)` and made
            // the shift look like a no-op move. Integer-looking operands are shown as integers.
            if (float.IsNaN(f) || float.IsInfinity(f)) return "0x" + bits.ToString("X8");
            if (bits != 0 && Math.Abs(f) < 1e-30f)
                return "i" + ((int)bits).ToString(CultureInfo.InvariantCulture);
            if (f == Math.Floor(f) && Math.Abs(f) < 1e7f) return ((long)f).ToString(CultureInfo.InvariantCulture);
            return f.ToString("0.######", CultureInfo.InvariantCulture);
        }

        private string Suffix()
        {
            const string comps = "xyzw";
            switch (SelectionMode)
            {
                case 0:                                  // write mask
                    if (MaskOrSwizzle == 0 || MaskOrSwizzle == 0xF) return "";
                    var mask = new StringBuilder(".");
                    for (int i = 0; i < 4; i++)
                        if ((MaskOrSwizzle & (1 << i)) != 0) mask.Append(comps[i]);
                    return mask.ToString();
                case 1:                                  // swizzle, two bits per component
                    var sw = new StringBuilder();
                    for (int i = 0; i < 4; i++) sw.Append(comps[(MaskOrSwizzle >> (i * 2)) & 3]);
                    return sw.ToString() == "xyzw" ? "" : "." + sw;
                case 2:                                  // select a single component
                    return "." + comps[MaskOrSwizzle & 3];
                default:
                    return "";
            }
        }
    }

    internal sealed class Instruction
    {
        public string Opcode = "";
        public int OpcodeNumber;
        public bool Saturate;

        /// <summary>
        /// The raw opcode token. Several fields live in its opcode-specific bits and are only
        /// needed by the GLSL emitter: bit 18 is the test polarity that separates if_nz from
        /// if_z (and discard_nz from discard_z), and bits 11-15 are a dcl_resource's dimension.
        /// Kept whole rather than decoded into a growing set of fields.
        /// </summary>
        public uint Token;

        /// <summary>if_nz / discard_nz rather than the _z form. Meaningless for other opcodes.</summary>
        public bool TestNonZero => ((Token >> 18) & 1) != 0;

        /// <summary>A dcl_resource's D3D10_SB_RESOURCE_DIMENSION. Meaningless for other opcodes.</summary>
        public int ResourceDimension => (int)((Token >> 11) & 0x1F);
        public List<Operand> Operands = new();
        public int[] ExtraDwords = Array.Empty<int>();   // DCL_TEMPS and friends

        /// <summary>Whether this opcode's test-polarity bit means anything.</summary>
        private bool HasTest =>
            Opcode == "if" || Opcode == "discard" || Opcode == "breakc" ||
            Opcode == "continuec" || Opcode == "retc";

        public override string ToString() =>
            Opcode + (Saturate ? "_sat" : "") +
            // Printed, because an inverted branch is invisible otherwise and reads as a
            // perfectly plausible shader.
            (HasTest ? (TestNonZero ? "_nz" : "_z") : "") +
            (Operands.Count > 0 ? " " + string.Join(", ", Operands) : "") +
            (ExtraDwords.Length > 0 ? " " + string.Join(", ", ExtraDwords) : "");
    }

    /// <summary>
    /// The SHDR/SHEX instruction stream minus its two header dwords, or null if there is none.
    /// For diagnosing a decode that desynced: the disassembly above is an interpretation, this
    /// is the input it was built from.
    /// </summary>
    public static byte[] RawStream(byte[] dxbc)
    {
        if (dxbc.Length < 32 || Encoding.ASCII.GetString(dxbc, 0, 4) != "DXBC") return null;

        int chunkCount = BitConverter.ToInt32(dxbc, 28);
        for (int c = 0; c < chunkCount; c++)
        {
            int offsetPos = 32 + c * 4;
            if (offsetPos + 4 > dxbc.Length) break;
            int chunkOffset = BitConverter.ToInt32(dxbc, offsetPos);
            if (chunkOffset < 0 || chunkOffset + 8 > dxbc.Length) continue;

            string fourcc = Encoding.ASCII.GetString(dxbc, chunkOffset, 4);
            if (fourcc != "SHDR" && fourcc != "SHEX") continue;

            int chunkSize = BitConverter.ToInt32(dxbc, chunkOffset + 4);
            int start = chunkOffset + 8;
            int end = Math.Min(start + chunkSize, dxbc.Length);
            if (start + 8 > end) return null;

            int lengthInDwords = (int)BitConverter.ToUInt32(dxbc, start + 4);
            int streamEnd = Math.Min(start + lengthInDwords * 4, end);
            int from = start + 8;
            if (streamEnd <= from) return Array.Empty<byte>();

            var raw = new byte[streamEnd - from];
            Array.Copy(dxbc, from, raw, 0, raw.Length);
            return raw;
        }
        return null;
    }

    /// <summary>
    /// The cb0 register indices the shader actually READS, excluding the
    /// <c>dcl_constantbuffer</c> declaration itself.
    ///
    /// This is the evidence half of working out fxc's level_9_x register shift: comparing the
    /// registers the DX11 shader reads against the ones the translated DX9 shader declares
    /// says how far the constants moved, instead of inferring it from a count that conflates
    /// the inserted fixup register with a shader simply not reading its whole buffer.
    ///
    /// <paramref name="hasRelative"/> reports whether any constant read is relatively addressed.
    /// The set is then incomplete by construction - <c>cb0[r1.x + 7]</c> names one register but
    /// reaches 200 - so a caller must not treat it as exhaustive.
    /// </summary>
    public static SortedSet<int> ConstantBufferReads(byte[] dxbc, out bool hasRelative)
    {
        hasRelative = false;
        var reads = new SortedSet<int>();

        var instructions = Read(dxbc, out _, out string error);
        if (instructions == null || error != null) return reads;

        foreach (var ins in instructions)
        {
            if (ins.Opcode == "dcl_constantbuffer") continue;

            foreach (var op in ins.Operands)
            {
                if (op.Type != OperandType.ConstantBuffer) continue;
                // cbN[index]: the first index is the buffer number, the second the register.
                if (op.Indices.Count < 2) continue;
                if (op.RelativeIndex != null) hasRelative = true;
                reads.Add((int)op.Indices[1]);
            }
        }
        return reads;
    }

    /// <summary>One texture fetch: which sampler and texture it uses, and off which coordinate.</summary>
    internal sealed class SamplerUse
    {
        public int Sampler;
        public int Texture;
        /// <summary>The input register the coordinate came from, or -1 if it was computed.</summary>
        public int CoordRegister = -1;
        public int CoordFirstComponent;
        public int CoordSecondComponent;
    }

    /// <summary>
    /// Every texture fetch in the shader, with the interpolator its coordinate was read from.
    ///
    /// This is the DirectX 11 half of pairing the two register spaces up by coordinate; see
    /// Dx9Bytecode.SamplerCoordinates for why a coordinate is the thing that carries over
    /// between the two compilations, and GlConverter.BuildSamplerMap for the pairing itself.
    ///
    /// A coordinate the shader computed rather than read straight off an interpolator is
    /// reported with CoordRegister -1 instead of being guessed at.
    /// </summary>
    public static List<SamplerUse> SamplerUses(byte[] dxbc)
    {
        var uses = new List<SamplerUse>();

        var instructions = Read(dxbc, out _, out string error);
        if (instructions == null || error != null) return uses;

        foreach (var ins in instructions)
        {
            if (!ins.Opcode.StartsWith("sample", StringComparison.Ordinal)) continue;

            // sample dest, coord, resource, sampler. sampleinfo and samplepos share the prefix
            // but not the shape, and are filtered out by the operand count and types below.
            if (ins.Operands.Count < 4) continue;

            var coord = ins.Operands[1];
            var resource = ins.Operands[2];
            var sampler = ins.Operands[3];
            if (resource.Type != OperandType.Resource || sampler.Type != OperandType.Sampler) continue;
            if (resource.Indices.Count < 1 || sampler.Indices.Count < 1) continue;

            var use = new SamplerUse
            {
                Sampler = (int)sampler.Indices[0],
                Texture = (int)resource.Indices[0],
            };

            // SelectionMode 1 is a swizzle, two bits per component; a 2D fetch uses the first two.
            if (coord.Type == OperandType.Input && coord.Indices.Count >= 1 &&
                coord.RelativeIndex == null && coord.SelectionMode == 1)
            {
                use.CoordRegister = (int)coord.Indices[0];
                use.CoordFirstComponent = coord.MaskOrSwizzle & 3;
                use.CoordSecondComponent = (coord.MaskOrSwizzle >> 2) & 3;
            }

            uses.Add(use);
        }
        return uses;
    }

    /// <summary>Decodes the SHDR/SHEX instruction stream. Returns null with a reason if it cannot.</summary>
    public static List<Instruction> Read(byte[] dxbc, out string model, out string error)
    {
        model = "?";
        error = null;

        if (dxbc.Length < 32 || Encoding.ASCII.GetString(dxbc, 0, 4) != "DXBC")
        {
            error = "not a DXBC container";
            return null;
        }

        int chunkCount = BitConverter.ToInt32(dxbc, 28);
        for (int c = 0; c < chunkCount; c++)
        {
            int offsetPos = 32 + c * 4;
            if (offsetPos + 4 > dxbc.Length) break;
            int chunkOffset = BitConverter.ToInt32(dxbc, offsetPos);
            if (chunkOffset < 0 || chunkOffset + 8 > dxbc.Length) continue;

            string fourcc = Encoding.ASCII.GetString(dxbc, chunkOffset, 4);
            if (fourcc != "SHDR" && fourcc != "SHEX") continue;

            int chunkSize = BitConverter.ToInt32(dxbc, chunkOffset + 4);
            int start = chunkOffset + 8;
            int end = Math.Min(start + chunkSize, dxbc.Length);
            if (start + 8 > end) { error = "SHDR chunk too short"; return null; }

            uint version = BitConverter.ToUInt32(dxbc, start);
            uint programType = version >> 16;
            string stage = programType switch
            {
                0 => "ps", 1 => "vs", 2 => "gs", 3 => "hs", 4 => "ds", 5 => "cs", _ => "?",
            };
            model = $"{stage}_{(version >> 4) & 0xF}_{version & 0xF}";

            int lengthInDwords = (int)BitConverter.ToUInt32(dxbc, start + 4);
            int streamEnd = Math.Min(start + lengthInDwords * 4, end);

            var result = new List<Instruction>();
            int pos = start + 8;

            while (pos + 4 <= streamEnd)
            {
                int instructionStart = pos;
                uint token = BitConverter.ToUInt32(dxbc, pos);
                int opcode = (int)(token & 0x7FF);
                int lengthDwords = (int)((token >> 24) & 0x7F);

                var ins = new Instruction
                {
                    OpcodeNumber = opcode,
                    Opcode = Sm4Opcodes.Name(opcode),
                    Saturate = ((token >> 13) & 1) != 0,
                    Token = token,
                };

                if (opcode == Sm4Opcodes.CustomData)
                {
                    if (pos + 8 > streamEnd) break;
                    int customLength = (int)BitConverter.ToUInt32(dxbc, pos + 4);
                    if (customLength <= 0) break;
                    result.Add(ins);
                    pos += customLength * 4;
                    continue;
                }

                if (lengthDwords <= 0) { error = $"zero-length instruction at byte {pos}"; break; }
                int insEnd = Math.Min(instructionStart + lengthDwords * 4, streamEnd);
                pos += 4;

                // Skip any extended opcode tokens; none of the shaders here carry information in
                // them that changes the decode (sample offsets, resource dims).
                while (((token >> 31) & 1) != 0 && pos + 4 <= insEnd)
                {
                    token = BitConverter.ToUInt32(dxbc, pos);
                    pos += 4;
                }

                if (Sm4Opcodes.IsDeclarationWithRawDwords(ins.Opcode))
                {
                    var extra = new List<int>();
                    while (pos + 4 <= insEnd) { extra.Add((int)BitConverter.ToUInt32(dxbc, pos)); pos += 4; }
                    ins.ExtraDwords = extra.ToArray();
                    result.Add(ins);
                    pos = insEnd;
                    continue;
                }

                // The *_siv / *_sgv declarations are an operand followed by a bare
                // system-value-name dword. Decoding that dword as an operand produced a
                // phantom "r" register in the listing.
                int trailingDwords = Sm4Opcodes.TrailingDwordCount(ins.Opcode);

                while (pos + 4 <= insEnd - trailingDwords * 4)
                {
                    var operand = ReadOperand(dxbc, ref pos, insEnd, out string operandError);
                    if (operand == null) { error = operandError; break; }
                    ins.Operands.Add(operand);
                }

                if (error == null && trailingDwords > 0)
                {
                    var extra = new List<int>();
                    while (pos + 4 <= insEnd) { extra.Add((int)BitConverter.ToUInt32(dxbc, pos)); pos += 4; }
                    ins.ExtraDwords = extra.ToArray();
                }

                result.Add(ins);
                pos = insEnd;
                if (error != null) break;
            }

            return result;
        }

        error = "no SHDR/SHEX chunk";
        return null;
    }

    private static Operand ReadOperand(byte[] buf, ref int pos, int limit, out string error)
    {
        error = null;
        if (pos + 4 > limit) { error = "operand runs past the instruction"; return null; }

        uint token = BitConverter.ToUInt32(buf, pos);
        pos += 4;

        var op = new Operand
        {
            NumComponents = (int)(token & 0x3),
            SelectionMode = (int)((token >> 2) & 0x3),
            MaskOrSwizzle = (int)((token >> 4) & 0xFF),
            Type = (OperandType)((token >> 12) & 0xFF),
        };
        int indexDimension = (int)((token >> 20) & 0x3);

        if (((token >> 31) & 1) != 0)
        {
            if (pos + 4 > limit) { error = "extended operand token runs past the instruction"; return null; }
            uint ext = BitConverter.ToUInt32(buf, pos);
            pos += 4;
            if ((ext & 0x3F) == 1) op.Modifier = (int)((ext >> 6) & 0xFF);
        }

        if (op.Type == OperandType.Immediate32)
        {
            // Component count is an enum, not a count: 0 = zero components, 1 = one,
            // 2 = four, 3 = N. Verified against a real `mov o0.w, l(1.0)`, whose immediate
            // operand token 0x00004001 declares 1 component and is followed by exactly one
            // dword (0x3F800000). Reading four there desynced the whole stream.
            int count = op.NumComponents == 2 ? 4 : 1;
            op.ImmediateFloats = new float[count];
            op.ImmediateBits = new uint[count];
            for (int i = 0; i < count; i++)
            {
                if (pos + 4 > limit) { error = "immediate runs past the instruction"; return null; }
                uint bits = BitConverter.ToUInt32(buf, pos);
                pos += 4;
                op.ImmediateBits[i] = bits;
                op.ImmediateFloats[i] = BitConverter.Int32BitsToSingle((int)bits);
            }
            return op;
        }

        for (int d = 0; d < indexDimension; d++)
        {
            int representation = (int)((token >> (22 + d * 3)) & 0x7);
            switch (representation)
            {
                case 0:   // immediate32
                    if (pos + 4 > limit) { error = "index runs past the instruction"; return null; }
                    op.Indices.Add(BitConverter.ToUInt32(buf, pos));
                    pos += 4;
                    break;
                case 1:   // immediate64
                    if (pos + 8 > limit) { error = "64-bit index runs past the instruction"; return null; }
                    op.Indices.Add((long)BitConverter.ToUInt64(buf, pos));
                    pos += 8;
                    break;
                case 2:   // relative: an operand of its own
                {
                    var rel = ReadOperand(buf, ref pos, limit, out error);
                    if (rel == null) return null;
                    op.Indices.Add(0);
                    op.RelativeIndex = rel;
                    op.RelativeIndexDimension = d;
                    break;
                }
                case 3:   // immediate32 + relative
                {
                    if (pos + 4 > limit) { error = "index runs past the instruction"; return null; }
                    long baseIndex = BitConverter.ToUInt32(buf, pos);
                    pos += 4;
                    var rel = ReadOperand(buf, ref pos, limit, out error);
                    if (rel == null) return null;
                    op.Indices.Add(baseIndex);
                    // Keeping this matters: dropping it made RoundLine's instanced line shader
                    // print as cb0[7] when it is really cb0[7 + r1.x], which reads as a shader
                    // that ignores its instance data.
                    op.RelativeIndex = rel;
                    op.RelativeIndexDimension = d;
                    break;
                }
                default:
                    error = $"unsupported index representation {representation}";
                    return null;
            }
        }

        return op;
    }
}

internal static class Sm4Opcodes
{
    public const int CustomData = 53;

    private static readonly string[] Names =
    {
        "add", "and", "break", "breakc", "call", "callc", "case", "continue", "continuec",
        "cut", "default", "deriv_rtx", "deriv_rty", "discard", "div", "dp2", "dp3", "dp4",
        "else", "emit", "emitthencut", "endif", "endloop", "endswitch", "eq", "exp", "frc",
        "ftoi", "ftou", "ge", "iadd", "if", "ieq", "ige", "ilt", "imad", "imax", "imin",
        "imul", "ine", "ineg", "ishl", "ishr", "itof", "label", "ld", "ld_ms", "log", "loop",
        "lt", "mad", "min", "max", "customdata", "mov", "movc", "mul", "ne", "nop", "not",
        "or", "resinfo", "ret", "retc", "round_ne", "round_ni", "round_pi", "round_z", "rsq",
        "sample", "sample_c", "sample_c_lz", "sample_l", "sample_d", "sample_b", "sqrt",
        "switch", "sincos", "udiv", "ult", "uge", "umul", "umad", "umax", "umin", "ushr",
        "utof", "xor", "dcl_resource", "dcl_constantbuffer", "dcl_sampler", "dcl_indexrange",
        "dcl_outputtopology", "dcl_inputprimitive", "dcl_maxout", "dcl_input", "dcl_input_sgv",
        "dcl_input_siv", "dcl_input_ps", "dcl_input_ps_sgv", "dcl_input_ps_siv", "dcl_output",
        "dcl_output_sgv", "dcl_output_siv", "dcl_temps", "dcl_indexableTemp",
        "dcl_globalFlags",
    };

    public static string Name(int opcode) =>
        opcode >= 0 && opcode < Names.Length ? Names[opcode] : $"opcode_{opcode}";

    /// <summary>
    /// Declarations whose payload is raw dwords rather than operands. Everything else in the
    /// dcl_* family does encode real operands (dcl_input v0.xy, dcl_resource t0, ...), so only
    /// these must skip operand decoding.
    ///
    /// Matched on the NAME rather than a hardcoded opcode number, because the number is this
    /// file's own array index and hardcoding it twice is how the two drift apart.
    /// </summary>
    public static bool IsDeclarationWithRawDwords(string name) =>
        name == "dcl_temps" || name == "dcl_globalFlags" || name == "dcl_maxout";

    /// <summary>
    /// How many bare dwords trail an instruction's operands. The *_siv / *_sgv declarations
    /// carry a system-value name this way; everything else here carries none.
    /// </summary>
    public static int TrailingDwordCount(string name) =>
        name.EndsWith("_siv", StringComparison.Ordinal) ||
        name.EndsWith("_sgv", StringComparison.Ordinal) ? 1 : 0;
}
