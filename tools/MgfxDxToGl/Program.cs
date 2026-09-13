using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UW.Tools.MgfxTranscode;

namespace UW.Tools.MgfxDxToGl;

internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length < 1)
        {
            Usage();
            return 2;
        }

        string command = args[0];
        string[] rest = args.Skip(1).ToArray();

        try
        {
            return command switch
            {
                "roundtrip" => RoundTrip(rest),
                "dump" => Dump(rest),
                "translate" => Translate(rest),
                "sm4" => Sm4(rest),
                "disasm" => Disasm(rest),
                "signatures" => Signatures(rest),
                "regshift" => RegShift(rest),
                "convert" => Convert(rest),
                _ => Usage(),
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("FATAL: " + ex.GetBaseException().Message);
            return 1;
        }
    }

    private static int Usage()
    {
        Console.Error.WriteLine("mgfxdxtogl - convert DirectX-profile MGFX effects to the OpenGL profile");
        Console.Error.WriteLine();
        Console.Error.WriteLine("  roundtrip <path>...");
        Console.Error.WriteLine("      Read each effect and write it back at its own version, then compare bytes.");
        Console.Error.WriteLine("      Proves the container model is complete before anything relies on it to");
        Console.Error.WriteLine("      preserve the parts it does not touch. Read-only.");
        Console.Error.WriteLine();
        Console.Error.WriteLine("  dump <path>... [--glsl] [--params]");
        Console.Error.WriteLine("      Print an effect's constant buffers, shaders, samplers, attributes and");
        Console.Error.WriteLine("      techniques. --glsl prints shader source for OpenGL-profile effects.");
        Console.Error.WriteLine();
        Console.Error.WriteLine("  convert <in.xnb> <out.xnb>");
        Console.Error.WriteLine("      Translate a DirectX-profile effect to the OpenGL profile via the DX9");
        Console.Error.WriteLine("      bytecode in each shader's Aon9 chunk, and write it as MGFX v10.");
        Console.Error.WriteLine();
        Console.Error.WriteLine("  Paths may be .xnb (the effect is taken from the container) or a bare .mgfxo.");
        return 2;
    }

    // -------------------------------------------------------------------------------------------

    /// <summary>Loads an effect blob from either an XNB container or a bare .mgfxo file.</summary>
    private static byte[] LoadBlob(string path)
    {
        if (path.EndsWith(".xnb", StringComparison.OrdinalIgnoreCase))
            return XnbFile.Read(path).ReadEffectBlob();
        return File.ReadAllBytes(path);
    }

    private static IEnumerable<string> Expand(IEnumerable<string> paths)
    {
        foreach (string p in paths)
        {
            if (p.StartsWith("--", StringComparison.Ordinal)) continue;
            if (Directory.Exists(p))
            {
                foreach (string f in Directory.EnumerateFiles(p, "*.xnb", SearchOption.AllDirectories))
                    yield return f;
            }
            else
            {
                yield return p;
            }
        }
    }

    private static int RoundTrip(string[] args)
    {
        int identical = 0, differed = 0, skipped = 0;

        foreach (string path in Expand(args))
        {
            byte[] blob;
            try
            {
                blob = LoadBlob(path);
            }
            catch (InvalidDataException)
            {
                skipped++;   // not an effect; `dump`/`roundtrip` over a whole Content tree hits many
                continue;
            }

            string name = Path.GetFileName(path);
            try
            {
                var doc = MgfxDocument.Read(blob);

                // Preserve the key, because it is not part of what the model has to get right.
                // These effects were transcoded from MGFX v8 by MgfxTranscode, which carries the
                // original key across rather than recomputing it, so the shipped value is not the
                // hash of this v10 body. Recomputing is checked separately below, against the one
                // input where the key IS the body's hash: mgfxc's own output.
                byte[] again = doc.Write(doc.Version, preserveEffectKey: true);

                if (again.Length == blob.Length && again.AsSpan().SequenceEqual(blob))
                {
                    byte[] rehashed = doc.Write(doc.Version);
                    int freshKey = BitConverter.ToInt32(rehashed, 6);
                    string keyNote = freshKey == doc.EffectKey
                        ? "key matches"
                        : $"key 0x{doc.EffectKey:X8}, recomputed 0x{freshKey:X8}";
                    Console.WriteLine($"  OK   {name,-32} v{doc.Version} profile={doc.Profile} " +
                                      $"{blob.Length,7} B identical ({keyNote})");
                    identical++;
                }
                else
                {
                    Console.WriteLine($"  DIFF {name,-32} v{doc.Version} profile={doc.Profile} " +
                                      $"{blob.Length} B in, {again.Length} B out");
                    Console.WriteLine("       " + FirstDifference(blob, again));
                    differed++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  FAIL {name,-32} {ex.GetBaseException().Message}");
                differed++;
            }
        }

        Console.WriteLine();
        Console.WriteLine($"{identical} byte-identical, {differed} not, {skipped} non-effect file(s) skipped.");
        return differed == 0 ? 0 : 1;
    }

    private static string FirstDifference(byte[] a, byte[] b)
    {
        int n = Math.Min(a.Length, b.Length);
        for (int i = 0; i < n; i++)
            if (a[i] != b[i])
                return $"first difference at byte {i}: 0x{a[i]:X2} vs 0x{b[i]:X2}";
        return $"identical for the first {n} bytes; lengths differ";
    }

    // -------------------------------------------------------------------------------------------

    private static int Dump(string[] args)
    {
        bool showGlsl = args.Contains("--glsl");
        bool showParams = args.Contains("--params");

        foreach (string path in Expand(args))
        {
            byte[] blob;
            try { blob = LoadBlob(path); }
            catch (InvalidDataException) { continue; }

            var doc = MgfxDocument.Read(blob);
            Console.WriteLine($"=== {Path.GetFileName(path)}  MGFX v{doc.Version} " +
                              $"profile={(doc.Profile == MgfxDocument.ProfileOpenGL ? "OpenGL" : "DirectX_11")} " +
                              $"key=0x{doc.EffectKey:X8} ===");

            Console.WriteLine($"  constant buffers: {doc.ConstantBuffers.Count}");
            for (int i = 0; i < doc.ConstantBuffers.Count; i++)
            {
                var cb = doc.ConstantBuffers[i];
                Console.WriteLine($"    [{i}] \"{cb.Name}\" size={cb.SizeInBytes} params={cb.ParameterIndex.Count}");
                for (int j = 0; j < cb.ParameterIndex.Count; j++)
                {
                    int pi = cb.ParameterIndex[j];
                    ushort off = cb.ParameterOffset[j];
                    string pname = pi >= 0 && pi < doc.Parameters.Count ? doc.Parameters[pi].Name : "?";
                    var p = pi >= 0 && pi < doc.Parameters.Count ? doc.Parameters[pi] : null;
                    string shape = p == null ? "" : $" {p.RowCount}x{p.ColumnCount}" +
                                                    (p.Elements.Count > 0 ? $"[{p.Elements.Count}]" : "");
                    // reg = offset/16 is the DX9 constant register the OpenGL profile would use.
                    Console.WriteLine($"          param[{pi}] offset={off,-5} (reg {off / 16,-3}) {pname}{shape}");
                }
            }

            Console.WriteLine($"  shaders: {doc.Shaders.Count}");
            for (int i = 0; i < doc.Shaders.Count; i++)
            {
                var s = doc.Shaders[i];
                Console.WriteLine($"    [{i}] {(s.IsVertexShader ? "vs" : "ps")} code={s.ShaderCode.Length,6} B " +
                                  $"samplers={s.Samplers.Count} cbuffers=[{string.Join(",", s.ConstantBuffers)}] " +
                                  $"attrs={s.Attributes.Count}");
                foreach (var sm in s.Samplers)
                    Console.WriteLine($"          sampler type={sm.Type} tex={sm.TextureSlot} smp={sm.SamplerSlot} " +
                                      $"param={sm.ParameterIndex} state={(sm.State != null ? "yes" : "no")} \"{sm.Name}\"");
                foreach (var a in s.Attributes)
                    Console.WriteLine($"          attr usage={a.Usage} index={a.Index} loc={a.Location} \"{a.Name}\"");
                if (showGlsl && doc.Profile == MgfxDocument.ProfileOpenGL)
                {
                    Console.WriteLine("          ---- GLSL ----");
                    foreach (string line in Encoding.ASCII.GetString(s.ShaderCode).Split('\n'))
                        Console.WriteLine("          " + line.TrimEnd('\r'));
                }
            }

            if (showParams)
            {
                Console.WriteLine($"  parameters: {doc.Parameters.Count}");
                for (int i = 0; i < doc.Parameters.Count; i++)
                {
                    var p = doc.Parameters[i];
                    Console.WriteLine($"    [{i}] class={p.Class} type={p.Type} {p.RowCount}x{p.ColumnCount}" +
                                      (p.Elements.Count > 0 ? $"[{p.Elements.Count}]" : "") +
                                      (p.StructMembers.Count > 0 ? $" members={p.StructMembers.Count}" : "") +
                                      $" \"{p.Name}\"" +
                                      (string.IsNullOrEmpty(p.Semantic) ? "" : $" : {p.Semantic}"));
                }
            }

            Console.WriteLine($"  techniques: {doc.Techniques.Count}");
            foreach (var t in doc.Techniques)
            {
                Console.WriteLine($"    \"{t.Name}\" passes={t.Passes.Count}");
                foreach (var p in t.Passes)
                    Console.WriteLine($"      \"{p.Name}\" vs={p.VertexShaderIndex} ps={p.PixelShaderIndex}" +
                                      (p.BlendState != null ? " blend" : "") +
                                      (p.DepthStencilState != null ? " depth" : "") +
                                      (p.RasterizerState != null ? " raster" : ""));
            }
            Console.WriteLine();
        }
        return 0;
    }

    // -------------------------------------------------------------------------------------------

    /// <summary>
    /// Translates each shader of a DirectX-profile effect and prints what MojoShader made of it,
    /// without building a container. This is the diagnostic for a conversion that fails or looks
    /// wrong: it shows the GLSL, the uniform arrays it declares and the samplers and attributes
    /// it found, none of which are visible once `convert` has refused.
    /// </summary>
    private static int Translate(string[] args)
    {
        bool showGlsl = args.Contains("--glsl");
        int shaderFilter = -1;
        int idx = Array.IndexOf(args, "--shader");
        if (idx >= 0 && idx + 1 < args.Length) int.TryParse(args[idx + 1], out shaderFilter);

        foreach (string path in Expand(args))
        {
            byte[] blob;
            try { blob = LoadBlob(path); }
            catch (InvalidDataException) { continue; }

            var doc = MgfxDocument.Read(blob);
            Console.WriteLine($"=== {Path.GetFileName(path)} (profile " +
                              $"{(doc.Profile == MgfxDocument.ProfileOpenGL ? "OpenGL" : "DirectX_11")}) ===");

            for (int i = 0; i < doc.Shaders.Count; i++)
            {
                if (shaderFilter >= 0 && i != shaderFilter) continue;
                var s = doc.Shaders[i];
                string tag = $"[{i}] {(s.IsVertexShader ? "vs" : "ps")}";

                byte[] dx9 = Dx9Bytecode.Extract(s.ShaderCode, out string why);
                if (dx9 == null) { Console.WriteLine($"  {tag} no DX9 bytecode: {why}"); continue; }

                var t = MojoShader.Translate(dx9, out string error);
                if (t == null) { Console.WriteLine($"  {tag} MojoShader: {error}"); continue; }

                Console.WriteLine($"  {tag} {t.ShaderType}_{t.MajorVersion}_{t.MinorVersion} " +
                                  $"instr={t.InstructionCount} dxCbuffers=[{string.Join(",", s.ConstantBuffers)}]");
                foreach (var u in t.Uniforms)
                    Console.WriteLine($"        uniform {u.Type} index={u.Index} arrayCount={u.ArrayCount} " +
                                      $"constant={u.Constant} \"{u.Name}\"");
                foreach (var sm in t.Samplers)
                    Console.WriteLine($"        sampler {sm.Type} index={sm.Index} \"{sm.Name}\"");
                foreach (var a in t.Attributes)
                    Console.WriteLine($"        attr {a.Usage}{a.Index} \"{a.Name}\"");

                string glsl = t.Glsl ?? string.Empty;
                foreach (System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(
                             glsl, @"^uniform[^\n]*;", System.Text.RegularExpressions.RegexOptions.Multiline))
                    Console.WriteLine($"        decl: {m.Value.Trim()}");

                if (showGlsl)
                    foreach (string line in glsl.Split('\n'))
                        Console.WriteLine("        " + line.TrimEnd('\r'));
            }
            Console.WriteLine();
        }
        return 0;
    }

    /// <summary>
    /// Reports the SM4 opcode makeup of each shader, aggregated across everything given. This is
    /// the scoping measurement for the effects with no Aon9 chunk: their only route to GLSL is
    /// translating DX11 bytecode, and the size of that job is decided by how many distinct
    /// opcodes appear and whether any are exotic.
    /// </summary>
    private static int Sm4(string[] args)
    {
        var total = new SortedDictionary<string, int>(StringComparer.Ordinal);
        int shaders = 0, withFlow = 0, withIndexing = 0, maxInstr = 0, maxTemps = 0;

        foreach (string path in Expand(args))
        {
            byte[] blob;
            try { blob = LoadBlob(path); }
            catch (InvalidDataException) { continue; }

            MgfxDocument doc;
            try { doc = MgfxDocument.Read(blob); }
            catch (InvalidDataException) { continue; }

            if (doc.Profile != MgfxDocument.ProfileDirectX11) continue;

            Console.WriteLine($"=== {Path.GetFileName(path)} ===");
            for (int i = 0; i < doc.Shaders.Count; i++)
            {
                var s = doc.Shaders[i];
                var info = Sm4Bytecode.Read(s.ShaderCode);
                bool hasAon9 = Dx9Bytecode.Extract(s.ShaderCode, out _) != null;

                Console.WriteLine($"  [{i}] {info.Model,-6} instr={info.InstructionCount,-4} " +
                                  $"temps={info.TempCount,-3} " +
                                  $"{(info.HasDynamicFlowControl ? "flow " : "     ")}" +
                                  $"{(info.HasIndexing ? "index " : "      ")}" +
                                  $"{(hasAon9 ? "hasAon9" : "NO-Aon9")}" +
                                  (info.Error == null ? "" : $"  ({info.Error})"));

                shaders++;
                if (info.HasDynamicFlowControl) withFlow++;
                if (info.HasIndexing) withIndexing++;
                maxInstr = Math.Max(maxInstr, info.InstructionCount);
                maxTemps = Math.Max(maxTemps, info.TempCount);
                foreach (var kv in info.Opcodes)
                {
                    total.TryGetValue(kv.Key, out int n);
                    total[kv.Key] = n + kv.Value;
                }
            }
        }

        Console.WriteLine();
        Console.WriteLine($"{shaders} shader(s); {withFlow} with dynamic flow control, " +
                          $"{withIndexing} with indexing; largest {maxInstr} instructions, " +
                          $"{maxTemps} temps.");
        Console.WriteLine($"{total.Count} distinct opcode(s):");
        foreach (var kv in total)
            Console.WriteLine($"    {kv.Key,-36} {kv.Value}");
        return 0;
    }

    /// <summary>
    /// Disassembles a shader's DX11 bytecode. Used to read values that exist only there - most
    /// importantly the immediates fxc inlines in the DX11 shader but lifts into a constant
    /// register for the SM2 fallback.
    /// </summary>
    private static int Disasm(string[] args)
    {
        int shaderFilter = -1;
        int idx = Array.IndexOf(args, "--shader");
        if (idx >= 0 && idx + 1 < args.Length) int.TryParse(args[idx + 1], out shaderFilter);

        foreach (string path in Expand(args))
        {
            byte[] blob;
            try { blob = LoadBlob(path); }
            catch (InvalidDataException) { continue; }

            MgfxDocument doc;
            try { doc = MgfxDocument.Read(blob); }
            catch (InvalidDataException) { continue; }

            Console.WriteLine($"=== {Path.GetFileName(path)} ===");
            for (int i = 0; i < doc.Shaders.Count; i++)
            {
                if (shaderFilter >= 0 && i != shaderFilter) continue;
                var s = doc.Shaders[i];

                var instructions = Sm4Disassembler.Read(s.ShaderCode, out string model, out string error);
                Console.WriteLine($"  [{i}] {model}" + (error == null ? "" : $"  ({error})"));

                if (args.Contains("--raw"))
                {
                    // The instruction stream as dwords, with the opcode token's fields broken
                    // out. This is what settles a decoder desync: the printed instructions are
                    // an interpretation, these are the bytes.
                    byte[] shdr = Sm4Disassembler.RawStream(s.ShaderCode);
                    if (shdr != null)
                    {
                        for (int b = 0; b + 4 <= shdr.Length; b += 4)
                        {
                            uint dw = BitConverter.ToUInt32(shdr, b);
                            Console.WriteLine($"        {b / 4,4}: 0x{dw:X8}   " +
                                              $"op={dw & 0x7FF,-4} len={(dw >> 24) & 0x7F,-3} " +
                                              $"ext={(dw >> 31) & 1}   f={BitConverter.Int32BitsToSingle((int)dw):g}");
                        }
                    }
                }

                if (instructions == null) continue;
                foreach (var ins in instructions)
                    Console.WriteLine("        " + ins);
            }
            Console.WriteLine();
        }
        return 0;
    }

    /// <summary>
    /// Prints each shader's DXBC input and output signatures, and - where the shader also has an
    /// Aon9 chunk - cross-checks the input signature against the attributes MojoShader derives
    /// independently from the DX9 bytecode.
    ///
    /// That cross-check is the point. The signature chunks are the only source for what vertex
    /// semantics a shader with no Aon9 expects, and there is no way to verify a reading of them
    /// on exactly those shaders. But on the 16 effects that DO have Aon9 there are two
    /// independent derivations of the same fact, so agreement there is real evidence the parser
    /// is right before it is trusted on multiTex, skinFX and Vehicle.
    /// </summary>
    private static int Signatures(string[] args)
    {
        int agree = 0, disagree = 0, unchecked_ = 0;

        foreach (string path in Expand(args))
        {
            byte[] blob;
            try { blob = LoadBlob(path); }
            catch (InvalidDataException) { continue; }

            MgfxDocument doc;
            try { doc = MgfxDocument.Read(blob); }
            catch (InvalidDataException) { continue; }
            if (doc.Profile != MgfxDocument.ProfileDirectX11) continue;

            Console.WriteLine($"=== {Path.GetFileName(path)} ===");
            for (int i = 0; i < doc.Shaders.Count; i++)
            {
                var s = doc.Shaders[i];
                var inputs = DxbcSignature.Read(s.ShaderCode, "ISGN");
                var outputs = DxbcSignature.Read(s.ShaderCode, "OSGN");
                if (outputs.Count == 0) outputs = DxbcSignature.Read(s.ShaderCode, "OSG5");

                Console.WriteLine($"  [{i}] {(s.IsVertexShader ? "vs" : "ps")}  " +
                                  $"{inputs.Count} input(s), {outputs.Count} output(s)");
                foreach (var e in inputs) Console.WriteLine($"        in   {e}");
                foreach (var e in outputs) Console.WriteLine($"        out  {e}");

                // Cross-check against MojoShader, but only for a vertex shader: a pixel shader's
                // inputs are varyings, which MojoShader reports as inputs rather than attributes.
                if (!s.IsVertexShader) { continue; }

                byte[] dx9 = Dx9Bytecode.Extract(s.ShaderCode, out _);
                if (dx9 == null) { unchecked_++; continue; }
                var t = MojoShader.Translate(dx9, out _);
                if (t == null) { unchecked_++; continue; }

                // Compare the (usage, index) sets. MojoShader drops attributes the DX9 bytecode
                // does not read, so the signature is allowed to be a superset.
                var fromMojo = new SortedSet<string>(
                    t.Attributes.ConvertAll(a => $"{(int)a.Usage}:{a.Index}"));
                var fromSignature = new SortedSet<string>();
                foreach (var e in inputs)
                    if (e.Usage >= 0) fromSignature.Add($"{e.Usage}:{e.SemanticIndex}");

                bool ok = fromMojo.IsSubsetOf(fromSignature);
                Console.WriteLine($"        check: MojoShader [{string.Join(" ", fromMojo)}] " +
                                  (ok ? "is a subset of" : "DOES NOT MATCH") +
                                  $" signature [{string.Join(" ", fromSignature)}]");
                if (ok) agree++; else disagree++;
            }
            Console.WriteLine();
        }

        Console.WriteLine($"signature/MojoShader cross-check: {agree} agree, {disagree} disagree, " +
                          $"{unchecked_} vertex shader(s) with no Aon9 to check against.");
        return disagree == 0 ? 0 : 1;
    }

    /// <summary>
    /// Reports, per shader, how many float4 constant registers the translated GLSL declares
    /// against how many the DX11 bytecode's dcl_constantbuffer declares.
    ///
    /// The difference matters because fxc's level_9_x lowering does not preserve the constant
    /// layout: for a vertex shader it allocates a register to its own half-pixel position fixup
    /// and shifts the real constants up. DevShape's 3D vertex shader reads `renderMatrix` at
    /// c1..c4 with the fixup at c0, while the DirectX container places renderMatrix at offset 0.
    /// Copying DirectX offsets across therefore puts every uniform one register early.
    ///
    /// So this measures the shift on every shader rather than assuming it is always one, and
    /// always at the front.
    /// </summary>
    private static int RegShift(string[] args)
    {
        Console.WriteLine($"{"effect",-28} {"shader",-7} {"glsl",5} {"dx11",5} {"shift",6}");

        var histogram = new SortedDictionary<int, int>();

        foreach (string path in Expand(args))
        {
            byte[] blob;
            try { blob = LoadBlob(path); }
            catch (InvalidDataException) { continue; }

            MgfxDocument doc;
            try { doc = MgfxDocument.Read(blob); }
            catch (InvalidDataException) { continue; }
            if (doc.Profile != MgfxDocument.ProfileDirectX11) continue;

            string effect = Path.GetFileNameWithoutExtension(path);

            for (int i = 0; i < doc.Shaders.Count; i++)
            {
                var s = doc.Shaders[i];
                byte[] dx9 = Dx9Bytecode.Extract(s.ShaderCode, out _);
                if (dx9 == null) continue;

                var t = MojoShader.Translate(dx9, out _);
                if (t == null)
                {
                    int registers = CtabBuilder.DeclaredConstantRegisters(s.ShaderCode);
                    if (registers <= 0) continue;
                    byte[] withCtab = CtabBuilder.Inject(dx9, registers, out _);
                    if (withCtab == null) continue;
                    t = MojoShader.Translate(withCtab, out _);
                    if (t == null) continue;
                }

                int glslRegisters = DeclaredGlslRegisters(t.Glsl, s.IsVertexShader);
                int dx11Registers = CtabBuilder.DeclaredConstantRegisters(s.ShaderCode);
                int shift = glslRegisters - dx11Registers;

                Console.WriteLine($"{effect,-28} {"[" + i + "]" + (s.IsVertexShader ? "vs" : "ps"),-7} " +
                                  $"{glslRegisters,5} {dx11Registers,5} {shift,6}");

                string key = s.IsVertexShader ? "vs" : "ps";
                histogram.TryGetValue(shift, out int n);
                histogram[shift] = n + 1;
            }
        }

        Console.WriteLine();
        Console.WriteLine("shift histogram (glsl registers - dx11 registers):");
        foreach (var kv in histogram)
            Console.WriteLine($"  {kv.Key,+3}  {kv.Value} shader(s)");
        return 0;
    }

    private static int DeclaredGlslRegisters(string glsl, bool isVertexShader)
    {
        string name = isVertexShader ? "vs_uniforms_vec4" : "ps_uniforms_vec4";
        var m = System.Text.RegularExpressions.Regex.Match(
            glsl ?? "", @"uniform\s+vec4\s+" + name + @"\s*\[\s*(\d+)\s*\]");
        return m.Success ? int.Parse(m.Groups[1].Value) : 0;
    }

    private static int Convert(string[] args)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("usage: mgfxdxtogl convert <in.xnb> <out.xnb>");
            return 2;
        }

        bool forceSm4 = args.Contains("--sm4");
        var paths = args.Where(a => !a.StartsWith("--", StringComparison.Ordinal)).ToArray();
        string inPath = paths[0], outPath = paths[1];
        var xnb = XnbFile.Read(inPath);
        var doc = MgfxDocument.Read(xnb.ReadEffectBlob());

        var report = GlConverter.Convert(doc, forceSm4);
        foreach (string line in report.Log) Console.WriteLine("  " + line);

        if (!report.Success)
        {
            Console.Error.WriteLine($"  !! {Path.GetFileName(inPath)}: not converted " +
                                    $"({report.Failures.Count} of {doc.Shaders.Count} shader(s) could not be translated)");
            // The reasons, not just the count. A failure here is usually the interesting output
            // of a run - it says what about the shader defeated the conversion.
            foreach (string failure in report.Failures)
                Console.Error.WriteLine("     " + failure);
            return 1;
        }

        byte[] blob = doc.Write(MgfxDocument.VersionV10);
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outPath)));
        xnb.WriteWithEffectBlob(outPath, blob);

        Console.WriteLine($"  -> {Path.GetFileName(outPath)}  {blob.Length} B, MGFX v10 profile=OpenGL");
        return 0;
    }
}
