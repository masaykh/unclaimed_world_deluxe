using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace UW.Tools.MgfxDxToGl;

/// <summary>
/// Rewrites a DirectX-profile MGFX effect in place as an OpenGL-profile one.
///
/// What changes and what does not:
///
///   * shader code   DXBC -> GLSL, translated by MojoShader from the DX9 bytecode in the
///                   shader's Aon9 chunk. This is the same translator, and the same
///                   post-processing, that MonoGame's OpenGLShaderProfile applies - see
///                   ShaderData.CreateGLSL in mgfxc.
///   * attributes    absent on DirectX, required on OpenGL. Taken from MojoShader, which reads
///                   them out of the shader's DCL instructions.
///   * samplers      state and parameter index are kept from the DirectX shader; the uniform
///                   name and both slots are taken from MojoShader, as ShaderData.CreateGLSL
///                   does. DirectX allocates its texture and sampler registers separately and
///                   leaves the name empty; OpenGL has one combined texture unit and binds it by
///                   name. See NameSamplers and BuildSamplerMap.
///   * constant      rebuilt as the per-shader vs_/ps_uniforms_* buffers the generated GLSL
///     buffers       declares. See BuildConstantBuffer for why this can be derived from the
///                   DirectX layout without MojoShader's symbol table.
///   * parameters,   untouched. They come from the same HLSL either way, and the container
///     techniques,   model round-trips them byte for byte.
///     passes
/// </summary>
internal static class GlConverter
{
    internal sealed class Report
    {
        public List<string> Log = new();
        public List<string> Failures = new();
        public bool Success => Failures.Count == 0;
    }

    /// <summary>
    /// Matches MojoShader's uniform array declarations. The declared length is authoritative for
    /// how big the constant buffer must be: MonoGame uploads Size/16 vec4s into this array, and
    /// a mismatch would either overrun the array or leave registers unset.
    /// </summary>
    private static readonly Regex UniformArrayRegex = new(
        @"^uniform\s+(?<glsltype>vec4|ivec4|bool)\s+(?<name>(?:vs|ps)_uniforms_(?:vec4|ivec4|bool))\s*\[\s*(?<count>\d+)\s*\]\s*;",
        RegexOptions.Multiline | RegexOptions.Compiled);

    /// <summary>
    /// Converts an effect to the OpenGL profile.
    ///
    /// <paramref name="forceSm4"/> translates the DirectX 11 bytecode directly even where DX9
    /// bytecode exists. That path needs no register remapping - there is no fxc lowering in it to
    /// undo - so it is the assumption-free one, and it is the only route for a shader whose
    /// MojoShader register mapping cannot be derived. It applies to the WHOLE effect rather than
    /// per shader, because the two paths name their varyings differently and a vertex and pixel
    /// shader converted by different routes would not link.
    /// </summary>
    public static Report Convert(MgfxDocument doc, bool forceSm4 = false)
    {
        var report = new Report();

        if (doc.Profile == MgfxDocument.ProfileOpenGL)
        {
            report.Failures.Add("already an OpenGL-profile effect");
            report.Log.Add("!! already OpenGL profile; nothing to do");
            return report;
        }

        // The DirectX constant buffers are consumed to build the OpenGL ones and then replaced
        // wholesale, so keep the originals to read from while the new list is assembled.
        var dxBuffers = doc.ConstantBuffers.Select(cb => cb.Clone()).ToList();
        var glBuffers = new List<MgfxDocument.ConstantBuffer>();

        for (int i = 0; i < doc.Shaders.Count; i++)
        {
            var shader = doc.Shaders[i];
            string tag = $"[{i}] {(shader.IsVertexShader ? "vs" : "ps")}";

            byte[] dx9 = forceSm4 ? null : Dx9Bytecode.Extract(shader.ShaderCode, out string why);
            if (forceSm4) why = "forced to the direct SM4 translation";
            if (dx9 == null)
            {
                // No DX9 fallback bytecode, so MojoShader has nothing to consume. Translate the
                // DirectX 11 bytecode straight to GLSL instead - see Sm4ToGlsl for why that is
                // the only route for these, not merely a convenience.
                ConvertFromSm4(shader, doc, dxBuffers, glBuffers, tag, report);
                continue;
            }

            var translated = MojoShader.Translate(dx9, out string error);

            if (translated == null && error != null &&
                error.IndexOf("CTAB", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                // MojoShader will not take relative constant addressing without a CTAB, because
                // it cannot otherwise know how large to declare the uniform array. mgfxc
                // stripped the CTAB, but the DX11 bytecode still records the size in its
                // dcl_constantbuffer - so one can be synthesised. See CtabBuilder.
                int registers = CtabBuilder.DeclaredConstantRegisters(shader.ShaderCode);
                if (registers > 0)
                {
                    byte[] withCtab = CtabBuilder.Inject(dx9, registers, out string ctabError);
                    if (withCtab == null)
                    {
                        report.Log.Add($"   {tag} could not synthesise a CTAB: {ctabError}");
                    }
                    else
                    {
                        var retry = MojoShader.Translate(withCtab, out string retryError);
                        if (retry != null)
                        {
                            report.Log.Add($"   {tag} retried with a synthesised CTAB of " +
                                           $"{registers} float4 register(s): accepted");
                            translated = retry;
                            error = null;
                        }
                        else
                        {
                            report.Log.Add($"   {tag} synthesised CTAB rejected: {retryError}");
                        }
                    }
                }
            }

            if (translated == null)
            {
                report.Failures.Add($"{tag}: {error}");
                report.Log.Add($"!! {tag} MojoShader: {error}");
                continue;
            }

            // MojoShader reports the shader stage it actually found; if that disagrees with the
            // container the wrong bytecode was picked up, and every later index would be wrong.
            bool mojoSaysVertex = translated.ShaderType == MojoShader.ShaderType.Vertex;
            if (mojoSaysVertex != shader.IsVertexShader)
            {
                report.Failures.Add(
                    $"{tag}: container says {(shader.IsVertexShader ? "vertex" : "pixel")} shader but the " +
                    $"DX9 bytecode is a {translated.ShaderType} shader");
                report.Log.Add($"!! {tag} shader stage mismatch");
                continue;
            }

            // Keep the DirectX bytecode: BuildConstantBuffers still needs to inspect it, and
            // the next line replaces it with GLSL.
            byte[] dxbc = shader.ShaderCode;

            string glsl = PostProcess(translated.Glsl, shader.IsVertexShader);
            shader.ShaderCode = Encoding.ASCII.GetBytes(glsl);

            shader.Attributes = BuildAttributes(shader, dxbc, translated, tag, report);

            NameSamplers(shader, dxbc, dx9, translated, tag, report);

            shader.ConstantBuffers = BuildConstantBuffers(
                shader, dxbc, translated, dxBuffers, glBuffers, ref glsl, doc, tag, report);

            // BuildConstantBuffers may have padded a uniform array declaration, so the shader
            // code is written from the possibly-adjusted GLSL rather than the original.
            shader.ShaderCode = Encoding.ASCII.GetBytes(glsl);

            report.Log.Add(
                $"{tag} {translated.ShaderType.ToString().ToLowerInvariant()}_{translated.MajorVersion}_" +
                $"{translated.MinorVersion}  dx9={dx9.Length}B -> glsl={glsl.Length}B  " +
                $"instr={translated.InstructionCount} attrs={shader.Attributes.Count} " +
                $"samplers={shader.Samplers.Count} cbuffers=[{string.Join(",", shader.ConstantBuffers)}]");
        }

        if (!report.Success) return report;

        doc.ConstantBuffers.Clear();
        foreach (var cb in glBuffers) doc.ConstantBuffers.Add(cb);
        doc.Profile = MgfxDocument.ProfileOpenGL;

        report.Log.Add($"profile -> OpenGL, {glBuffers.Count} constant buffer(s) " +
                       $"(was {dxBuffers.Count} DirectX buffer(s))");
        return report;
    }

    /// <summary>
    /// The exact post-processing MonoGame's ShaderData.CreateGLSL applies to MojoShader's output.
    /// Reproduced rather than improvised, because the OpenGL backend depends on it: the ES
    /// precision block, the "#version 110" strip and the ivec4 declaration fix are all things
    /// MonoGame's own compiler emits, and a shader without them behaves differently.
    ///
    /// CRLF is deliberate - that is what MonoGame writes, and matching it keeps a converted
    /// effect byte-comparable with a freshly compiled one.
    /// </summary>
    private static string PostProcess(string glsl, bool isVertexShader)
    {
        glsl = (glsl ?? string.Empty).Replace("#version 110", "");

        string precision = isVertexShader ? "precision highp float;\r\n" : "precision mediump float;\r\n";
        glsl = "#ifdef GL_ES\r\n" + precision + "precision mediump int;\r\n#endif\r\n" + glsl;

        if (glsl.IndexOf("dFdx", StringComparison.InvariantCulture) >= 0 ||
            glsl.IndexOf("dFdy", StringComparison.InvariantCulture) >= 0)
        {
            glsl = "#extension GL_OES_standard_derivatives : enable\r\n" + glsl;
        }

        // MojoShader declares the integer uniform array as vec4; MonoGame corrects it.
        glsl = glsl.Replace("uniform vec4 ps_uniforms_ivec4", "uniform ivec4 ps_uniforms_ivec4");

        return glsl;
    }

    /// <summary>
    /// Builds the shader's attribute table: GLSL names from MojoShader, vertex semantics from
    /// the DXBC input signature.
    ///
    /// The two halves have to come from different places, and getting this wrong is silent.
    /// MonoGame's OpenGL backend binds an attribute by NAME and then resolves a vertex element
    /// through usage and index:
    ///
    ///     Attributes[i].location = GL.GetAttribLocation(program, Attributes[i].name);
    ///     int attribLocation = shader.GetAttribLocation(element.VertexElementUsage, element.UsageIndex);
    ///
    /// so the name must match the GLSL, and the usage must match the vertex declaration the game
    /// draws with. MojoShader supplies the name, but its usages are **not** the real semantics:
    /// when fxc compiles for a level_9_x profile it rewrites every input semantic to TEXCOORD,
    /// so the Aon9 declarations say TEXCOORD0..N whatever the source said. DevShape's vertex
    /// shader really takes POSITION0 and COLOR0; MojoShader reports TexCoord0 and TexCoord1.
    ///
    /// Taking usages from MojoShader therefore produces a shader whose attributes can never
    /// match a vertex declaration - GetAttribLocation(Position, 0) finds nothing, returns -1, and
    /// the position stream is simply not bound. The effect still loads, which is why a
    /// load-only check does not catch it.
    ///
    /// The ISGN chunk has the true semantics, and register numbers line up between the two
    /// (MojoShader names attributes vs_v&lt;register&gt;, and fxc preserves the register
    /// assignment through its lowering), so the register is what joins them.
    /// </summary>
    private static List<MgfxDocument.Attribute> BuildAttributes(
        MgfxDocument.Shader shader, byte[] dxbc, MojoShader.Result translated,
        string tag, Report report)
    {
        var result = new List<MgfxDocument.Attribute>();
        if (translated.Attributes.Count == 0) return result;

        var signature = DxbcSignature.Read(dxbc, "ISGN");
        if (signature.Count == 0)
        {
            report.Failures.Add(
                $"{tag}: has {translated.Attributes.Count} attribute(s) but no ISGN input " +
                "signature, so their true vertex semantics cannot be determined");
            report.Log.Add($"!! {tag} no ISGN chunk to take attribute semantics from");
            return result;
        }

        foreach (var a in translated.Attributes)
        {
            int register = RegisterFromName(a.Name);
            if (register < 0)
            {
                report.Failures.Add($"{tag}: cannot read a register number out of attribute name '{a.Name}'");
                report.Log.Add($"!! {tag} unexpected attribute name '{a.Name}'");
                continue;
            }

            var element = signature.FirstOrDefault(e => e.Register == register);
            if (element == null)
            {
                report.Failures.Add(
                    $"{tag}: attribute '{a.Name}' is at register {register}, which the input " +
                    "signature does not describe");
                report.Log.Add($"!! {tag} register {register} missing from ISGN");
                continue;
            }

            if (element.Usage < 0)
            {
                report.Failures.Add(
                    $"{tag}: input semantic '{element.SemanticName}' has no XNA VertexElementUsage");
                report.Log.Add($"!! {tag} unmappable semantic '{element.SemanticName}'");
                continue;
            }

            result.Add(new MgfxDocument.Attribute
            {
                Name = a.Name,
                Usage = (byte)element.Usage,
                Index = (byte)element.SemanticIndex,
                // The backend fills the real location in with glGetAttribLocation at link time;
                // mgfxc writes 0 here too.
                Location = 0,
            });

            // MojoShader reports MOJOSHADER_usage, the signature gives an XNA usage; comparing
            // them raw would compare two different enumerations.
            int mojoAsXna = DxbcSignature.XnaUsageFromMojoShader((int)a.Usage);
            if (mojoAsXna != element.Usage || a.Index != element.SemanticIndex)
            {
                report.Log.Add(
                    $"   {tag} {a.Name}: semantic from ISGN is " +
                    $"{DxbcSignature.VertexElementUsageName(element.Usage)}{element.SemanticIndex}, " +
                    $"not MojoShader's {DxbcSignature.VertexElementUsageName(mojoAsXna)}{a.Index} " +
                    "(fxc's level_9_x semantic rewrite)");
            }
        }

        return result;
    }

    /// <summary>Parses the register out of a MojoShader attribute name such as "vs_v3".</summary>
    private static int RegisterFromName(string name)
    {
        if (string.IsNullOrEmpty(name)) return -1;
        int i = name.LastIndexOf('v');
        if (i < 0 || i + 1 >= name.Length) return -1;
        return int.TryParse(name.Substring(i + 1), out int register) ? register : -1;
    }

    /// <summary>
    /// Rewrites each sampler for the OpenGL profile: the GLSL uniform name the backend binds the
    /// texture unit by, and both slots.
    ///
    /// All three come from MojoShader's sampler index, which is what mgfxc's own
    /// ShaderData.CreateGLSL writes (samplerName, textureSlot and samplerSlot are assigned from
    /// it verbatim). That the two slots become equal is not incidental: DirectX has separate
    /// texture and sampler register spaces and the shipped effects use them, so a sampler can sit
    /// at s4 while its texture sits at t2. OpenGL has one combined texture unit, and the backend
    /// reads the two fields for different purposes - the texture goes to Textures[textureSlot]
    /// while the sampler state goes to SamplerStates[samplerSlot] - so leaving them apart applies
    /// each texture's filtering and addressing to a different texture. Six of the shipped effects
    /// had them apart.
    ///
    /// The hard part is which MojoShader sampler each DirectX one is; see BuildSamplerMap.
    /// </summary>
    private static void NameSamplers(
        MgfxDocument.Shader shader, byte[] dxbc, byte[] dx9, MojoShader.Result translated,
        string tag, Report report)
    {
        if (shader.Samplers.Count == 0) return;

        var samplerMap = BuildSamplerMap(shader, dxbc, dx9, translated, tag, report);

        foreach (var sampler in shader.Samplers)
        {
            if (samplerMap == null || !samplerMap.TryGetValue(sampler.SamplerSlot, out int index))
            {
                report.Log.Add($"   {tag} sampler s{sampler.SamplerSlot} (param " +
                               $"{sampler.ParameterIndex}) has no DX9 counterpart; left unnamed");
                continue;
            }

            var match = translated.Samplers.First(s => s.Index == index);
            sampler.Name = match.Name;
            sampler.Type = (byte)Math.Max(0, (int)match.Type);
            sampler.TextureSlot = (byte)index;
            sampler.SamplerSlot = (byte)index;
        }
    }

    /// <summary>
    /// Maps each DirectX 11 sampler register to the DX9 one MojoShader named, by pairing the two
    /// bytecodes' texture fetches on the interpolator each reads its coordinate from.
    ///
    /// Nothing positional works here. fxc compiles the same HLSL to both profiles but allocates
    /// the sampler registers independently, so the numbers do not carry over: GUI/LCD's are
    /// {0,3,4} on DirectX and {0,1,2} on DX9. Nor does rank close the gap - OverlayEffect's are
    /// {0,1,2} on both, yet two of the three are exchanged. Nor does position in the shader: the
    /// two compilations do not even emit the fetches in the same order. What does carry over is
    /// the coordinate, because it comes from the same interpolator in both.
    ///
    /// The interpolators are matched by rank rather than by register number, since a pixel
    /// shader's DirectX input registers start after SV_Position while the DX9 texture registers
    /// start at zero. Both compilations pack the same semantics into the same interpolator in the
    /// same order, which the input signature shows directly: GUI/CRT's register 1 carries
    /// TEXCOORD0 and TEXCOORD1 in disjoint masks exactly as DX9's t0 does.
    ///
    /// Returns null rather than a partial answer when the pairing cannot be established - a
    /// coordinate the shader computed arithmetically names no interpolator, which is the case for
    /// three of water's five fetches. The caller reports that and leaves those samplers unnamed,
    /// because a mispaired sampler reads the wrong texture silently.
    /// </summary>
    private static Dictionary<int, int> BuildSamplerMap(
        MgfxDocument.Shader shader, byte[] dxbc, byte[] dx9, MojoShader.Result translated,
        string tag, Report report)
    {
        var declared = shader.Samplers.Select(s => (int)s.SamplerSlot).Distinct().OrderBy(i => i).ToList();
        var available = translated.Samplers.Select(s => s.Index).Distinct().OrderBy(i => i).ToList();

        // The unambiguous case, and much the commonest: one sampler, one candidate.
        if (declared.Count == 1 && available.Count == 1)
            return new Dictionary<int, int> { [declared[0]] = available[0] };

        var dx9Coordinates = Dx9Bytecode.SamplerCoordinates(dx9, out string dx9Error);
        if (dx9Coordinates == null)
        {
            report.Log.Add($"   {tag} cannot read the DX9 texture fetches: {dx9Error}");
            return null;
        }

        // Which DX9 texture register each DirectX input register corresponds to: its rank among
        // the input registers that carry a TEXCOORD semantic.
        var signature = DxbcSignature.Read(dxbc, "ISGN");
        var interpolators = signature
            .Where(e => e.SemanticName.StartsWith("TEXCOORD", StringComparison.OrdinalIgnoreCase))
            .Select(e => e.Register)
            .Distinct()
            .OrderBy(r => r)
            .ToList();
        if (interpolators.Count == 0)
        {
            report.Log.Add($"   {tag} input signature declares no interpolator to pair fetches on");
            return null;
        }

        // The coordinate each DirectX sampler reads. A sampler read at two different coordinates
        // could not identify a fetch, and one whose coordinate the shader computed names no
        // interpolator at all; both leave it without a key.
        var dxCoordinates = new Dictionary<int, string>();
        var unkeyed = new HashSet<int>();
        foreach (var use in Sm4Disassembler.SamplerUses(dxbc))
        {
            int rank = use.CoordRegister < 0 ? -1 : interpolators.IndexOf(use.CoordRegister);
            string coord = rank < 0
                ? null
                : Dx9Bytecode.FormatCoordinate(rank, use.CoordFirstComponent, use.CoordSecondComponent);

            if (coord == null) unkeyed.Add(use.Sampler);
            else if (dxCoordinates.TryGetValue(use.Sampler, out string already) && already != coord)
                unkeyed.Add(use.Sampler);
            else dxCoordinates[use.Sampler] = coord;
        }
        foreach (int sampler in unkeyed) dxCoordinates.Remove(sampler);

        // Group both sides by coordinate. A group has to be the same size on both sides for the
        // coordinates to have been read correctly at all, and once every sampler is keyed the
        // groups partition the two register spaces the same way.
        var dxGroups = dxCoordinates.GroupBy(kv => kv.Value)
            .ToDictionary(g => g.Key, g => g.Select(kv => kv.Key).OrderBy(i => i).ToList());
        var dx9Groups = dx9Coordinates.GroupBy(kv => kv.Value)
            .ToDictionary(g => g.Key, g => g.Select(kv => kv.Key).OrderBy(i => i).ToList());

        // Every sampler must be keyed, or a group with a missing member would silently look the
        // right size and pair the wrong way round. water computes three of its five coordinates,
        // so it lands here and keeps the register numbers it came with.
        var unkeyedDeclared = declared.Where(d => !dxCoordinates.ContainsKey(d)).ToList();
        if (unkeyedDeclared.Count > 0)
        {
            report.Log.Add($"   {tag} no interpolator identifies sampler(s) " +
                           string.Join(", ", unkeyedDeclared.Select(s => "s" + s)) +
                           "; pairing the registers by number instead, unverified");
            return IdentityMap(declared, available, tag, report);
        }

        var map = new Dictionary<int, int>();
        var assumed = new List<string>();
        foreach (var group in dxGroups)
        {
            if (!dx9Groups.TryGetValue(group.Key, out var counterparts) ||
                counterparts.Count != group.Value.Count)
            {
                report.Log.Add(
                    $"   {tag} {group.Value.Count} DirectX sampler(s) read {group.Key} but " +
                    $"{(counterparts?.Count ?? 0)} DX9 sampler(s) do; pairing the registers by " +
                    "number instead, unverified");
                return IdentityMap(declared, available, tag, report);
            }

            // Within a group of one the coordinate settles it. Within a larger group nothing
            // distinguishes the members, so they are paired in register order and said to be.
            for (int i = 0; i < group.Value.Count; i++)
            {
                map[group.Value[i]] = counterparts[i];
                if (group.Value.Count > 1)
                    assumed.Add($"s{group.Value[i]}->s{counterparts[i]}");
            }
        }

        // Verification: the pairing has to be a bijection covering every declared sampler. Had a
        // coordinate been misread, the result would almost certainly fail one of these.
        var missing = declared.Where(d => !map.ContainsKey(d)).ToList();
        if (missing.Count > 0)
        {
            report.Log.Add($"   {tag} no coordinate pairs sampler(s) " +
                           string.Join(", ", missing.Select(m => "s" + m)));
            return null;
        }
        if (map.Values.Distinct().Count() != map.Count)
        {
            report.Log.Add($"   {tag} two DirectX samplers pair with the same DX9 sampler");
            return null;
        }

        var changed = map.Where(kv => kv.Key != kv.Value).ToList();
        if (changed.Count > 0)
        {
            report.Log.Add($"   {tag} sampler registers re-allocated by fxc: " +
                           string.Join(", ", changed.Select(kv => $"s{kv.Key}->s{kv.Value}")));
        }
        if (assumed.Count > 0)
        {
            report.Log.Add($"   {tag} samplers reading the same coordinate are indistinguishable; " +
                           $"paired in register order, unverified: {string.Join(", ", assumed)}");
        }
        return map;
    }

    /// <summary>
    /// Pairs the two register spaces by number - what the converter did before the coordinates
    /// were read, and still right whenever fxc happened not to re-allocate. Used only where the
    /// coordinates cannot settle it, and always logged as unverified by the caller.
    /// </summary>
    private static Dictionary<int, int> IdentityMap(
        List<int> declared, List<int> available, string tag, Report report)
    {
        var map = new Dictionary<int, int>();
        foreach (int slot in declared)
        {
            if (available.Contains(slot)) map[slot] = slot;
            else report.Log.Add($"   {tag} s{slot} has no DX9 sampler of the same number");
        }
        return map;
    }

    /// <summary>
    /// Converts one shader by translating its DirectX 11 bytecode directly, for the shaders with
    /// no <c>Aon9</c> chunk. Mirrors what the MojoShader path does per shader: replace the code
    /// with GLSL, take the attributes, name the samplers and build the constant buffer.
    /// </summary>
    private static void ConvertFromSm4(
        MgfxDocument.Shader shader, MgfxDocument doc,
        List<MgfxDocument.ConstantBuffer> dxBuffers,
        List<MgfxDocument.ConstantBuffer> glBuffers,
        string tag, Report report)
    {
        byte[] dxbc = shader.ShaderCode;
        var translated = Sm4ToGlsl.Translate(dxbc);

        if (translated.Error != null)
        {
            report.Failures.Add($"{tag}: {translated.Error}");
            report.Log.Add($"!! {tag} SM4->GLSL: {translated.Error}");
            return;
        }

        if (translated.IsVertexShader != shader.IsVertexShader)
        {
            report.Failures.Add(
                $"{tag}: container says {(shader.IsVertexShader ? "vertex" : "pixel")} shader but " +
                "the DX11 bytecode says otherwise");
            report.Log.Add($"!! {tag} shader stage mismatch");
            return;
        }

        string glsl = translated.Glsl;
        shader.Attributes = translated.Attributes;

        // The DirectX sampler entries carry the parameter each texture comes from, which the
        // bytecode does not, so they are kept and given the translated name and a dense slot.
        foreach (var sampler in shader.Samplers)
        {
            int dense = translated.Samplers.FindIndex(
                s => s.TextureRegister == sampler.TextureSlot &&
                     s.SamplerRegister == sampler.SamplerSlot);
            if (dense < 0)
            {
                report.Log.Add(
                    $"   {tag} sampler t{sampler.TextureSlot}/s{sampler.SamplerSlot} " +
                    $"(param {sampler.ParameterIndex}) is never sampled by this shader; left unnamed");
                continue;
            }
            var match = translated.Samplers[dense];
            sampler.Name = match.Name;
            sampler.Type = (byte)match.Type;
            sampler.TextureSlot = (byte)dense;
            sampler.SamplerSlot = (byte)dense;
        }

        shader.ConstantBuffers = BuildConstantBuffers(
            shader, dxbc, null, dxBuffers, glBuffers, ref glsl, doc, tag, report,
            remapRegisters: false);

        shader.ShaderCode = Encoding.ASCII.GetBytes(glsl);

        report.Log.Add(
            $"{tag} {(shader.IsVertexShader ? "vs" : "ps")}_4_0 -> glsl={glsl.Length}B " +
            $"(direct SM4 translation) attrs={shader.Attributes.Count} " +
            $"samplers={shader.Samplers.Count} cbuffers=[{string.Join(",", shader.ConstantBuffers)}]");
    }

    /// <summary>
    /// The constant buffer for a shader translated straight from the DX11 bytecode, where the
    /// DirectX byte offsets are used as they stand.
    ///
    /// Only the array padding is shared reasoning with the remapping path - see the long comment
    /// there for why a partially-read parameter has to grow the declared array rather than be
    /// dropped. Everything the remapping path says about fxc's level_9_x lowering and its
    /// position-fixup register is irrelevant here: no DX9 lowering happened, so a shader that
    /// declares a constant buffer with no parameter behind it is simply a failure.
    /// </summary>
    private static void EmitConstantBuffer(
        MgfxDocument.Shader shader, MgfxDocument doc,
        List<MgfxDocument.ConstantBuffer> glBuffers, List<byte> result,
        List<(int ParameterIndex, int Offset)> assigned, string name, int count,
        ref string glsl, string tag, Report report)
    {
        int sizeInBytes = count * 16;
        var kept = assigned.Where(a => a.Offset < sizeInBytes).ToList();
        int dropped = assigned.Count - kept.Count;

        int needed = sizeInBytes;
        foreach (var (parameterIndex, offset) in kept)
        {
            var parameter = parameterIndex >= 0 && parameterIndex < doc.Parameters.Count
                ? doc.Parameters[parameterIndex]
                : null;
            needed = Math.Max(needed, offset + ExtentBytes(parameter));
        }
        needed = (needed + 15) / 16 * 16;

        if (needed > sizeInBytes)
        {
            int paddedCount = needed / 16;
            string from = $"uniform vec4 {name}[{count}];";
            string to = $"uniform vec4 {name}[{paddedCount}];";
            if (!glsl.Contains(from, StringComparison.Ordinal))
            {
                report.Failures.Add(
                    $"{tag}: {name} needs {needed} bytes for its parameters but the shader " +
                    $"declares {sizeInBytes}, and the declaration could not be found to pad");
                report.Log.Add($"!! {tag} could not pad {name}[{count}]");
                return;
            }
            glsl = glsl.Replace(from, to);
            report.Log.Add(
                $"   {tag} {name}[{count}] padded to [{paddedCount}]: a parameter is only " +
                "partially read by this shader, and MonoGame writes whole parameters");
            sizeInBytes = needed;
        }

        if (kept.Count == 0)
        {
            report.Failures.Add(
                $"{tag}: reads {name}[{count}] but no effect parameter maps into it");
            report.Log.Add($"!! {tag} {name}[{count}] has no parameter to bind");
            return;
        }

        var glBuffer = new MgfxDocument.ConstantBuffer
        {
            Name = name,
            SizeInBytes = (ushort)sizeInBytes,
        };
        foreach (var (parameterIndex, offset) in kept)
        {
            glBuffer.ParameterIndex.Add(parameterIndex);
            glBuffer.ParameterOffset.Add((ushort)offset);
        }

        if (dropped > 0)
        {
            report.Log.Add($"   {tag} {name}: {dropped} parameter(s) start past the array this " +
                           "shader declares and are not read by it; omitted");
        }

        int existingIndex = glBuffers.FindIndex(b => SameAs(b, glBuffer));
        if (existingIndex < 0)
        {
            existingIndex = glBuffers.Count;
            glBuffers.Add(glBuffer);
        }
        result.Add((byte)existingIndex);
    }

    /// <summary>
    /// Builds the vs_/ps_uniforms_* constant buffers the generated GLSL declares, and returns
    /// their indices for this shader.
    ///
    /// MonoGame's own OpenGL path builds these from MojoShader's symbol table, which comes from
    /// the bytecode's CTAB. mgfxc strips reflection, so the shipped effects have no CTAB and no
    /// symbols. The byte offsets already in the DirectX constant buffer are used verbatim
    /// instead, and that is correct for one specific reason: the Aon9 bytecode and the DirectX11
    /// bytecode are two outputs of the SAME fxc invocation over the same source, so the DX9
    /// fallback shader reads constants at the layout fxc chose for cb0 - components included.
    ///
    /// It is worth spelling out why comparing against mgfxc's own OpenGL output does NOT
    /// disprove this, because at first glance it looks like it does. For BloomCombine, four
    /// scalars:
    ///
    ///     shipped DirectX container      offsets 0, 4, 8, 12    size 16
    ///     mgfxc's OpenGL output          offsets 0, 16, 32, 48  size 64
    ///
    /// Those disagree, but they describe different bytecode. mgfxc compiled the HLSL afresh at
    /// ps_3_0 and its allocator gave each scalar a whole register, so its GLSL declares
    /// ps_uniforms_vec4[4]. The shipped Aon9 shader inherited fxc's cb0 packing and reads
    /// c0.x/y/z/w, so ITS translated GLSL declares ps_uniforms_vec4[1]. Each layout is right for
    /// its own shader; copying mgfxc's onto the shipped bytecode would put three of the four
    /// uniforms where nothing reads them.
    ///
    /// Where the two compilations do agree - GaussianBlur's float4-aligned arrays at 0 and 240,
    /// BloomExtract's single scalar at 0 - this produces byte-identical constant buffers AND
    /// byte-identical GLSL, which is the real confirmation that the translation path matches
    /// MonoGame's.
    ///
    /// The size comes from the array length the generated GLSL declares, not from the DirectX
    /// buffer: MonoGame uploads Size/16 vec4s into that array, so it has to match what this
    /// shader declared. Parameters beyond it are ones this shader does not read.
    /// </summary>
    private static List<byte> BuildConstantBuffers(
        MgfxDocument.Shader shader,
        byte[] dxbc,
        MojoShader.Result translated,
        List<MgfxDocument.ConstantBuffer> dxBuffers,
        List<MgfxDocument.ConstantBuffer> glBuffers,
        ref string glsl,
        MgfxDocument doc,
        string tag,
        Report report,
        bool remapRegisters = true)
    {
        var result = new List<byte>();

        if (shader.ConstantBuffers.Count > 1)
        {
            // DX11 has cb0, cb1, ...; DX9 has one flat c-register file. Concatenating in buffer
            // order is the natural reading, but no shipped effect exercises it, so it is
            // unverified - say so rather than quietly assuming.
            report.Log.Add($"   {tag} uses {shader.ConstantBuffers.Count} DirectX constant buffers; " +
                           "concatenating them in order (untested - no shipped effect does this)");
        }

        // Every (parameter, byte offset) pair this shader's DirectX buffers describe, in offset
        // order. The offsets carry over unchanged - see the remarks above.
        var assigned = new List<(int ParameterIndex, int Offset)>();
        foreach (byte dxIndex in shader.ConstantBuffers)
        {
            if (dxIndex >= dxBuffers.Count) continue;
            var cb = dxBuffers[dxIndex];

            foreach (int i in Enumerable.Range(0, cb.ParameterIndex.Count)
                         .OrderBy(i => cb.ParameterOffset[i]))
            {
                assigned.Add((cb.ParameterIndex[i], cb.ParameterOffset[i]));
            }
        }

        foreach (Match m in UniformArrayRegex.Matches(glsl))
        {
            string name = m.Groups["name"].Value;
            int count = int.Parse(m.Groups["count"].Value);

            // Only the float4 register file can be reconstructed this way. bool and int uniforms
            // live in DX9's separate b# and i# register files, which are numbered independently
            // and share no ordering with the float4 file, so there is nothing to walk. None of
            // the game's 19 effects use them - every uniform MojoShader reports is a float - so
            // this guards a future effect rather than a live limitation.
            if (!name.EndsWith("_vec4", StringComparison.Ordinal))
            {
                report.Failures.Add(
                    $"{tag}: the shader declares {name}[{count}], and DX9's bool/int register " +
                    "files cannot be reconstructed from a DirectX constant buffer layout");
                report.Log.Add($"!! {tag} unmappable uniform array {name}[{count}]");
                continue;
            }

            int sizeInBytes = count * 16;

            // fxc's level_9_x lowering does not preserve the constant layout: it RE-ALLOCATES the
            // DX9 registers densely, in ascending order of the DX11 register they came from,
            // after a register it reserves for its own half-pixel position fixup. Billboard's
            // vertex shader reads cb0 registers {0,13,14} on DirectX and declares {0,1,2,3} on
            // OpenGL - so the DirectX byte offsets have to be mapped through, not merely shifted.
            //
            // fxc's fixup register itself is left unbound. MojoShader emits the same correction
            // as its own posFixup uniform, which MonoGame sets from the viewport, so binding
            // both would apply it twice.
            // None of that applies to a shader translated straight from the DX11 bytecode: there
            // is no DX9 lowering in the path, the GLSL reads the same register the DX11 shader
            // read, and there is no inserted fixup register. The offsets are then used verbatim.
            if (!remapRegisters)
            {
                EmitConstantBuffer(shader, doc, glBuffers, result, assigned, name, count,
                    ref glsl, tag, report);
                continue;
            }

            var registerMap = BuildRegisterMap(shader, dxbc, translated, tag, report);
            if (registerMap == null) continue;      // reported; the mapping is not derivable

            var remapped = new List<(int ParameterIndex, int Offset)>();
            foreach (var (parameterIndex, offset) in assigned)
            {
                var parameter = parameterIndex >= 0 && parameterIndex < doc.Parameters.Count
                    ? doc.Parameters[parameterIndex]
                    : null;

                if (TryRemapOffset(registerMap, offset, ExtentBytes(parameter), out int mappedOffset))
                {
                    remapped.Add((parameterIndex, mappedOffset));
                }
                else
                {
                    // Either this shader does not read the parameter at all - the common case,
                    // since one parameter list serves every shader in the effect - or it spans
                    // registers the mapping does not keep contiguous. Both mean it cannot be
                    // placed here, and the array-bounds check below would have dropped the first
                    // kind anyway.
                    continue;
                }
            }
            assigned = remapped;

            // A parameter starting at or past the declared array is one this shader never reads.
            var kept = assigned.Where(a => a.Offset < sizeInBytes).ToList();
            int dropped = assigned.Count - kept.Count;

            // A parameter can START inside the array and END past it, and that is not an error to
            // drop - it is a parameter the shader partially reads. Billboard's pixel shader
            // declares ps_uniforms_vec4[4] and reads ps_c2.xyz and ps_c3.xyz, which are the first
            // two registers of the 4x4 `Rotation` at offset 32. So the parameter is genuinely
            // used, but MonoGame's ConstantBuffer.SetParameter always writes a whole matrix - 64
            // bytes - and a 64-byte buffer then throws:
            //
            //     ArgumentException: Offset and length were out of bounds for the array
            //
            // Dropping it would zero registers the shader reads; growing only the buffer would
            // make GL.Uniform4(location, Size/16, ...) upload more vec4s than the array holds,
            // which is a GL error. So the array itself is padded: the extra registers are
            // declared but never indexed, so the shader's behaviour is unchanged.
            //
            // MonoGame's own OpenGL path avoids this differently, by clamping a matrix
            // parameter's column count to the registers the symbol table says are used. That is
            // not available here - the parameter list is shared by every shader in the effect and
            // each uses a different amount of it - so padding is the equivalent that works
            // per-shader.
            int needed = sizeInBytes;
            foreach (var (parameterIndex, offset) in kept)
            {
                var parameter = parameterIndex >= 0 && parameterIndex < doc.Parameters.Count
                    ? doc.Parameters[parameterIndex]
                    : null;
                needed = Math.Max(needed, offset + ExtentBytes(parameter));
            }
            needed = (needed + 15) / 16 * 16;

            if (needed > sizeInBytes)
            {
                int paddedCount = needed / 16;
                string from = $"uniform vec4 {name}[{count}];";
                string to = $"uniform vec4 {name}[{paddedCount}];";
                if (glsl.Contains(from, StringComparison.Ordinal))
                {
                    glsl = glsl.Replace(from, to);
                    report.Log.Add(
                        $"   {tag} {name}[{count}] padded to [{paddedCount}]: a parameter is only " +
                        "partially read by this shader, and MonoGame writes whole parameters");
                    sizeInBytes = needed;
                }
                else
                {
                    report.Failures.Add(
                        $"{tag}: {name} needs {needed} bytes for its parameters but the shader " +
                        $"declares {sizeInBytes}, and the declaration could not be found to pad");
                    report.Log.Add($"!! {tag} could not pad {name}[{count}]");
                    continue;
                }
            }

            var glBuffer = new MgfxDocument.ConstantBuffer
            {
                Name = name,
                SizeInBytes = (ushort)sizeInBytes,
            };

            foreach (var (parameterIndex, offset) in kept)
            {
                glBuffer.ParameterIndex.Add(parameterIndex);
                glBuffer.ParameterOffset.Add((ushort)offset);
            }

            if (dropped > 0)
            {
                report.Log.Add($"   {tag} {name}: {dropped} parameter(s) start past the array this " +
                               "shader declares and are not read by it; omitted");
            }

            if (glBuffer.ParameterIndex.Count == 0 && count > 0 &&
                DeclaresNoConstantBuffer(dxbc))
            {
                // The DX11 shader declares no constant buffer at all, yet its DX9 fallback reads
                // one constant register. That is fxc's level_9_x position fixup: for feature
                // level 9 it emits a half-pixel correction through a constant the D3D11-on-9
                // runtime fills in. DevShape's 2D vertex shader makes this unambiguous - its
                // DX11 form is a pure passthrough (`mov o0.xyz, v0.xyz` / `mov o0.w, l(1.0)`)
                // while the Aon9 form does `v0.xy + c0.xy`.
                //
                // MojoShader already emits that correction itself, as its own posFixup uniform
                // (`gl_Position.xy += posFixup.zw * gl_Position.ww`), and MonoGame's OpenGL
                // backend sets posFixup from the viewport. So binding fxc's constant as well
                // would apply the correction twice; leaving the array unbound gives it GL's
                // default of zero, which is what makes the two paths agree.
                //
                // No constant buffer is emitted for this shader. The GLSL is untouched.
                report.Log.Add($"   {tag} {name}[{count}] left unbound: the DX11 shader declares no " +
                               "constant buffer, so this is fxc's level_9_x position fixup and " +
                               "MojoShader's own posFixup already handles it");
                continue;
            }

            if (glBuffer.ParameterIndex.Count == 0 && count > 0)
            {
                // The DX9 fallback reads a constant register that no effect parameter
                // corresponds to. This happens where fxc lifted an immediate out of the shader
                // into a constant register because SM2 cannot encode it inline: the DX11
                // bytecode needs no constant buffer at all (the container says cbuffers=[]),
                // the value lives in fxc's own level_9_x fixup data rather than in the effect,
                // and nothing here can recover it.
                //
                // Binding an all-zero buffer would compile, load and render subtly wrong, which
                // is worse than not converting - so this is a hard failure. DevShape's 2D vertex
                // shader and TimeOfDayAndLightsources' vertex shader are the two that hit it;
                // both need their HLSL recompiled rather than translated.
                report.Failures.Add(
                    $"{tag}: reads {name}[{count}] but the DirectX container attributes no parameter " +
                    "to it - fxc lifted an immediate into a constant register for the SM2 fallback, " +
                    "and that value is not in the effect. Needs HLSL, not translation");
                report.Log.Add($"!! {tag} {name}[{count}] has no parameter to bind (fxc-synthesised constant)");
                continue;
            }

            // Identical buffers are shared, exactly as EffectObject does when it de-duplicates
            // with ConstantBufferData.SameAs.
            int existingIndex = glBuffers.FindIndex(b => SameAs(b, glBuffer));
            if (existingIndex < 0)
            {
                existingIndex = glBuffers.Count;
                glBuffers.Add(glBuffer);
            }
            result.Add((byte)existingIndex);
        }

        return result;
    }

    /// <summary>
    /// True when the shader's DX11 bytecode contains no <c>dcl_constantbuffer</c>, i.e. it reads
    /// no constants at all on the DirectX path. Combined with the DX9 fallback reading one, that
    /// identifies an fxc-synthesised fixup constant rather than a real effect parameter.
    ///
    /// Returns false if the bytecode cannot be disassembled, so an unreadable shader is treated
    /// as "might genuinely need constants" and fails loudly instead of silently binding nothing.
    /// </summary>
    private static bool DeclaresNoConstantBuffer(byte[] dxbc)
    {
        var instructions = Sm4Disassembler.Read(dxbc, out _, out string error);
        if (instructions == null || error != null) return false;

        foreach (var ins in instructions)
            if (ins.Opcode == "dcl_constantbuffer")
                return false;

        return true;
    }

    /// <summary>
    /// How many float4 registers fxc's level_9_x lowering shifted the real constants up by,
    /// determined by comparing what the two bytecodes actually read rather than by counting.
    ///
    /// fxc allocates a register to its own D3D9 half-pixel position fixup when it lowers a
    /// vertex shader for feature level 9, and puts the real constants after it. DevShape's 3D
    /// vertex shader reads its matrix at c1..c4 with the fixup at c0, while the DirectX
    /// container places that matrix at offset 0 - so copying offsets across unshifted puts
    /// every uniform one register early and the shader transforms by nonsense.
    ///
    /// The previous rule inferred the shift from "the GLSL declares one register more than the
    /// DX11 dcl_constantbuffer". That conflates two unrelated things - the inserted fixup, and a
    /// shader not reading the tail of its buffer - and it cannot tell them apart. water's vertex
    /// shader declares 16 registers against a 17-register buffer, which that rule reads as
    /// "no shift" for the wrong reason.
    ///
    /// This instead takes the register indices the DX11 shader READS, offsets them, and requires
    /// the result to be contained in the set the translated shader declares. A shift is only
    /// accepted when the containment holds AND there is exactly one extra register AND register 0
    /// is one of the declared ones - which is what an inserted fixup at c0 looks like. Anything
    /// that fits neither offset is a failure rather than a guess: emitting an effect whose
    /// uniforms are silently one register out is the worst outcome available here.
    ///
    /// Relative addressing defeats the comparison, because <c>cb0[r1.x + 7]</c> contributes one
    /// index while reaching two hundred. RoundLine's instanced vertex shader is the case; there
    /// the read set is not exhaustive, so it falls back to the count and says so.
    /// </summary>
    private static Dictionary<int, int> BuildRegisterMap(
        MgfxDocument.Shader shader, byte[] dxbc, MojoShader.Result translated,
        string tag, Report report)
    {
        var dx11Reads = Sm4Disassembler.ConstantBufferReads(dxbc, out bool hasRelative);

        // Float uniforms are the float4 register file, which is the only one this maps.
        var dx9Registers = new SortedSet<int>(
            translated.Uniforms
                .Where(u => u.Type == MojoShader.UniformType.Float)
                .Select(u => u.Index));

        // Nothing to line up. DevShape's 2D vertex shader is here: the DX11 form reads no
        // constants at all, and its single DX9 register is the fixup, handled separately by
        // leaving the array unbound.
        if (dx11Reads.Count == 0 || dx9Registers.Count == 0) return new Dictionary<int, int>();

        if (hasRelative)
        {
            // cb0[r1.x + 7] contributes one index while reaching two hundred, so the read set is
            // not a basis for a mapping. RoundLine's instanced vertex shader is the case: its
            // constants are dense to begin with, so identity is the honest choice, and it is
            // recorded as unverified rather than presented as derived.
            report.Log.Add(
                $"   {tag} constant reads are relatively addressed, so the register mapping " +
                "cannot be derived; assuming the layouts match (unverified)");
            return new Dictionary<int, int>();
        }

        // fxc's fixup occupies one register and the real constants follow it, so the translated
        // shader declares exactly one more than the DX11 shader reads.
        bool hasFixup = shader.IsVertexShader
                        && dx9Registers.Count == dx11Reads.Count + 1
                        && dx9Registers.Contains(0);
        int firstReal = hasFixup ? 1 : 0;

        if (dx9Registers.Count != dx11Reads.Count + firstReal)
        {
            report.Failures.Add(
                $"{tag}: the DX11 shader reads {dx11Reads.Count} constant register(s) " +
                $"{{{string.Join(",", dx11Reads)}}} but the translated shader declares " +
                $"{dx9Registers.Count} {{{string.Join(",", dx9Registers)}}}. Those counts cannot " +
                "be reconciled, so where its uniforms belong cannot be established");
            report.Log.Add($"!! {tag} constant register counts cannot be reconciled");
            return null;
        }

        // The mapping: fxc allocates DX9 registers densely, in ascending order of the DX11
        // register they came from, after its own fixup. Verified against every shipped vertex
        // shader - DevShape {0,1,2,3}->{1,2,3,4}, CRT {0}->{1}, Billboard {0,13,14}->{1,2,3},
        // water {0..11,13,15,16}->{1..15} - which is why a plain "shift by one" only ever worked
        // for the shaders whose DX11 reads happened to be contiguous from zero.
        var map = new Dictionary<int, int>();
        int rank = 0;
        foreach (int dx11Register in dx11Reads)
            map[dx11Register] = firstReal + rank++;

        var mapped = new SortedSet<int>(map.Values);
        var expected = new SortedSet<int>(dx9Registers.Where(r => !(hasFixup && r == 0)));
        if (!mapped.SetEquals(expected))
        {
            report.Failures.Add(
                $"{tag}: the derived mapping produces registers {{{string.Join(",", mapped)}}} but " +
                $"the translated shader declares {{{string.Join(",", expected)}}} for its real " +
                "constants");
            report.Log.Add($"!! {tag} derived register mapping does not match the shader's own");
            return null;
        }

        if (map.Any(kv => kv.Key != kv.Value))
        {
            report.Log.Add(
                $"   {tag} constants re-allocated by fxc: " +
                string.Join(" ", map.OrderBy(kv => kv.Key).Select(kv => $"c{kv.Key}->c{kv.Value}")) +
                (hasFixup ? "  (position fixup at c0)" : ""));
        }
        return map;
    }

    /// <summary>
    /// Re-places a parameter from its DirectX byte offset onto the register the translated shader
    /// reads it from, preserving its position WITHIN the register so packed scalars stay put.
    ///
    /// Returns false when the parameter spans registers that do not remain contiguous under the
    /// mapping - which would mean a matrix or array split across the buffer, something a single
    /// offset cannot express.
    /// </summary>
    private static bool TryRemapOffset(
        Dictionary<int, int> map, int offset, int extent, out int newOffset)
    {
        newOffset = offset;
        if (map.Count == 0) return true;                 // identity

        int firstRegister = offset / 16;
        int lastRegister = (offset + Math.Max(extent, 1) - 1) / 16;

        if (!map.TryGetValue(firstRegister, out int mappedFirst)) return false;

        for (int r = firstRegister; r <= lastRegister; r++)
        {
            if (!map.TryGetValue(r, out int mapped)) return false;
            if (mapped != mappedFirst + (r - firstRegister)) return false;
        }

        newOffset = mappedFirst * 16 + offset % 16;
        return true;
    }

    /// <summary>
    /// How many bytes MonoGame's ConstantBuffer.SetParameter writes for a parameter, which is
    /// what decides whether it fits the buffer.
    ///
    /// Transcribed from that method and its SetData rather than reasoned about, because the
    /// rules are not what you would guess: a matrix passes its ColumnCount as SetData's *rows*
    /// and its RowCount as *columns* (they are swapped), a 1x1 always writes exactly 4 bytes
    /// whatever its type, a single-row or 4x4 value writes rows*columns*4 contiguously, and
    /// anything else writes one row per 16-byte register with only columns*4 bytes used in each.
    ///
    /// Array elements advance by whole registers, so the extent of an array is every element but
    /// the last at its register stride, plus the last element's own extent - not the stride
    /// times the count, which would over-report and drop parameters that actually fit.
    /// </summary>
    private static int ExtentBytes(MgfxDocument.Parameter p)
    {
        if (p == null) return 16;

        if (p.Elements.Count > 0)
        {
            int total = 0;
            for (int i = 0; i < p.Elements.Count - 1; i++)
                total += RegistersFor(p.Elements[i]) * 16;
            return total + ExtentBytes(p.Elements[^1]);
        }

        // EffectParameterClass.Matrix == 2.
        bool isMatrix = p.Class == 2;
        int rows = isMatrix ? p.ColumnCount : p.RowCount;
        int columns = isMatrix ? p.RowCount : p.ColumnCount;
        if (rows <= 0) rows = 1;
        if (columns <= 0) columns = 1;

        if (rows == 1 && columns == 1) return 4;
        if (rows == 1 || (rows == 4 && columns == 4)) return rows * columns * 4;
        return 16 * (rows - 1) + columns * 4;
    }

    /// <summary>
    /// The register count SetParameter returns for a parameter, which is how far the caller
    /// advances for the next array element.
    /// </summary>
    private static int RegistersFor(MgfxDocument.Parameter p)
    {
        if (p == null) return 1;
        if (p.Elements.Count > 0)
        {
            int total = 0;
            foreach (var e in p.Elements) total += RegistersFor(e);
            return total;
        }
        int registers = p.Class == 2 ? p.ColumnCount : p.RowCount;
        return registers <= 0 ? 1 : registers;
    }

    private static bool SameAs(MgfxDocument.ConstantBuffer a, MgfxDocument.ConstantBuffer b) =>
        a.Name == b.Name &&
        a.SizeInBytes == b.SizeInBytes &&
        a.ParameterIndex.SequenceEqual(b.ParameterIndex) &&
        a.ParameterOffset.SequenceEqual(b.ParameterOffset);
}
