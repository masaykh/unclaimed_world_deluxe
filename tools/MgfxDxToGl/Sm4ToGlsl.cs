using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace UW.Tools.MgfxDxToGl;

/// <summary>
/// Translates a DirectX 11 SM4 shader to GLSL, for the shaders that have no <c>Aon9</c> chunk
/// and so cannot go through MojoShader.
///
/// Three of the shipped effects - multiTex (19 shaders), skinFX (9) and Vehicle (10) - were
/// compiled at a plain SM4 profile rather than level_9_x, so there is no DX9 bytecode to hand to
/// MojoShader. They are also the three on the critical path for actually playing the game rather
/// than browsing menus: GameWorldRenderer.LoadContent loads multiTex, and all 207 model XNBs
/// reference skinFX.
///
/// Recovering HLSL for them and recompiling with mgfxc is not an alternative. mgfxc's OpenGL
/// profile compiles HLSL with fxc at vs_3_0/ps_3_0, and skinFX's constant buffer is 4224 bytes -
/// 264 float4 registers, with reads as high as cb0[262] - against vs_3_0's limit of 256. No
/// amount of source recovery gets past that. A GLSL uniform array has no such limit, so
/// translating the SM4 bytecode directly is the only route that works, and it needs no sources.
///
/// The output deliberately matches the dialect MojoShader emits, because that is what MonoGame's
/// OpenGL backend is known to accept here: no <c>#version</c> directive, <c>attribute</c> and
/// <c>varying</c> rather than <c>in</c>/<c>out</c>, and <c>gl_FragColor</c>. Emitting a modern
/// <c>#version</c> would give real integer operations but would fail outright on the 2.1-style
/// context this backend may create, and the existing converted effects prove this dialect links.
///
/// SM4 is a typed ISA and GLSL 1.10 has no integer bitwise operators, so integers are modelled as
/// floats and comparison results as 1.0/0.0 masks. That is sound for what fxc actually emits
/// here: every <c>and</c>/<c>or</c> in these effects combines comparison masks, every
/// <c>movc</c> is a select, and the only shifts are by a constant, which is an exact multiply.
/// Anything outside the handled set fails the translation rather than being approximated
/// silently.
/// </summary>
internal static class Sm4ToGlsl
{
    internal sealed class Sampler
    {
        public string Name = "";
        public int TextureRegister;
        public int SamplerRegister;
        /// <summary>MGFX sampler type: 0 Texture2D, 1 TextureCube, 2 Texture3D.</summary>
        public int Type;
    }

    internal sealed class Result
    {
        public string Glsl = "";
        public bool IsVertexShader;
        /// <summary>Input register to (name, usage, index), for a vertex shader's attributes.</summary>
        public List<MgfxDocument.Attribute> Attributes = new();
        public List<Sampler> Samplers = new();
        /// <summary>float4 registers the constant buffer must supply.</summary>
        public int ConstantRegisters;
        public string ConstantBufferName = "";
        public string Error;
    }

    private const string Components = "xyzw";

    public static Result Translate(byte[] dxbc)
    {
        var result = new Result();

        var instructions = Sm4Disassembler.Read(dxbc, out string model, out string decodeError);
        if (instructions == null)
        {
            result.Error = decodeError ?? "the instruction stream could not be decoded";
            return result;
        }
        if (decodeError != null)
        {
            result.Error = decodeError;
            return result;
        }

        result.IsVertexShader = model.StartsWith("vs", StringComparison.Ordinal);
        string stage = result.IsVertexShader ? "vs" : "ps";
        result.ConstantBufferName = $"{stage}_uniforms_vec4";

        var input = DxbcSignature.Read(dxbc, "ISGN");
        var output = DxbcSignature.Read(dxbc, "OSGN");

        // ---- declarations -------------------------------------------------------------------
        int temps = 0;
        int constantRegisters = 0;
        var textureDimensions = new Dictionary<int, int>();
        var samplerRegisters = new SortedSet<int>();
        var inputRegisters = new SortedSet<int>();
        var outputRegisters = new SortedSet<int>();
        var positionOutputs = new HashSet<int>();

        foreach (var ins in instructions)
        {
            switch (ins.Opcode)
            {
                case "dcl_temps":
                    if (ins.ExtraDwords.Length > 0) temps = ins.ExtraDwords[0];
                    break;

                case "dcl_constantbuffer":
                    // cb0[size]: the second index is the register count.
                    if (ins.Operands.Count > 0 && ins.Operands[0].Indices.Count >= 2)
                        constantRegisters = Math.Max(constantRegisters, (int)ins.Operands[0].Indices[1]);
                    break;

                case "dcl_resource":
                    if (ins.Operands.Count > 0 && ins.Operands[0].Indices.Count >= 1)
                        textureDimensions[(int)ins.Operands[0].Indices[0]] = ins.ResourceDimension;
                    break;

                case "dcl_sampler":
                    if (ins.Operands.Count > 0 && ins.Operands[0].Indices.Count >= 1)
                        samplerRegisters.Add((int)ins.Operands[0].Indices[0]);
                    break;

                case "dcl_input":
                case "dcl_input_ps":
                case "dcl_input_siv":
                case "dcl_input_sgv":
                case "dcl_input_ps_siv":
                case "dcl_input_ps_sgv":
                    if (ins.Operands.Count > 0 && ins.Operands[0].Type == Sm4Disassembler.OperandType.Input &&
                        ins.Operands[0].Indices.Count >= 1)
                        inputRegisters.Add((int)ins.Operands[0].Indices[0]);
                    break;

                case "dcl_output":
                case "dcl_output_siv":
                case "dcl_output_sgv":
                    if (ins.Operands.Count > 0 && ins.Operands[0].Indices.Count >= 1)
                    {
                        int reg = (int)ins.Operands[0].Indices[0];
                        outputRegisters.Add(reg);
                        // dcl_output_siv carries the system value; SV_Position is the one that
                        // has to become gl_Position rather than a varying.
                        if (ins.Opcode == "dcl_output_siv") positionOutputs.Add(reg);
                    }
                    break;
            }
        }

        result.ConstantRegisters = constantRegisters;

        // A vertex shader's SV_Position may be declared with a plain dcl_output, so fall back to
        // the output signature to find it.
        foreach (var e in output)
        {
            if (e.SemanticName.StartsWith("SV_Position", StringComparison.OrdinalIgnoreCase))
                positionOutputs.Add(e.Register);
        }

        // ---- names for inputs, outputs and samplers ------------------------------------------
        var inputNames = new Dictionary<int, string>();
        foreach (int reg in inputRegisters)
        {
            var element = input.FirstOrDefault(e => e.Register == reg);
            if (result.IsVertexShader)
            {
                if (element == null)
                {
                    result.Error = $"input register v{reg} has no entry in the input signature, " +
                                   "so its vertex semantic cannot be determined";
                    return result;
                }
                if (element.Usage < 0)
                {
                    result.Error = $"input semantic '{element.SemanticName}' has no XNA VertexElementUsage";
                    return result;
                }
                string name = $"vs_v{reg}";
                inputNames[reg] = name;
                result.Attributes.Add(new MgfxDocument.Attribute
                {
                    Name = name,
                    Usage = (byte)element.Usage,
                    Index = (byte)element.SemanticIndex,
                    Location = 0,
                });
            }
            else
            {
                // A pixel shader's SV_Position input is gl_FragCoord, not a varying.
                if (element != null &&
                    element.SemanticName.StartsWith("SV_Position", StringComparison.OrdinalIgnoreCase))
                {
                    inputNames[reg] = "gl_FragCoord";
                    continue;
                }
                if (element == null)
                {
                    result.Error = $"input register v{reg} has no entry in the input signature";
                    return result;
                }
                inputNames[reg] = VaryingForRegister(input, reg);
            }
        }

        // Outputs. A pixel shader writing one target uses gl_FragColor; more than one has to use
        // gl_FragData, and the two cannot be mixed.
        var outputNames = new Dictionary<int, string>();
        int colourTargets = result.IsVertexShader
            ? 0
            : outputRegisters.Count(r => !positionOutputs.Contains(r));
        foreach (int reg in outputRegisters)
        {
            if (positionOutputs.Contains(reg)) { outputNames[reg] = "gl_Position"; continue; }

            if (result.IsVertexShader)
            {
                string varying = VaryingForRegister(output, reg);
                if (varying == null)
                {
                    result.Error = $"output register o{reg} has no entry in the output signature";
                    return result;
                }
                outputNames[reg] = varying;
            }
            else
            {
                outputNames[reg] = colourTargets <= 1 ? "gl_FragColor" : $"gl_FragData[{reg}]";
            }
        }

        // Samplers. OpenGL has one combined texture unit, so each (texture, sampler) pair the
        // shader actually uses becomes one uniform at a dense index - the same shape
        // ShaderData.CreateGLSL produces.
        var pairs = new List<(int Texture, int Sampler)>();
        foreach (var ins in instructions)
        {
            if (!ins.Opcode.StartsWith("sample", StringComparison.Ordinal)) continue;
            if (ins.Operands.Count < 4) continue;
            var resource = ins.Operands[2];
            var sampler = ins.Operands[3];
            if (resource.Type != Sm4Disassembler.OperandType.Resource ||
                sampler.Type != Sm4Disassembler.OperandType.Sampler) continue;
            if (resource.Indices.Count < 1 || sampler.Indices.Count < 1) continue;
            var pair = ((int)resource.Indices[0], (int)sampler.Indices[0]);
            if (!pairs.Contains(pair)) pairs.Add(pair);
        }
        pairs.Sort((a, b) => a.Texture != b.Texture
            ? a.Texture.CompareTo(b.Texture)
            : a.Sampler.CompareTo(b.Sampler));

        var samplerNames = new Dictionary<(int, int), string>();
        for (int i = 0; i < pairs.Count; i++)
        {
            // D3D10_SB_RESOURCE_DIMENSION to the MGFX sampler type MonoGame reads
            // (Sampler2D 0, SamplerCube 1, SamplerVolume 2, Sampler1D 3).
            int dimension = textureDimensions.TryGetValue(pairs[i].Texture, out int d) ? d : 3;
            int type = dimension switch
            {
                2 => 3,     // TEXTURE1D
                3 => 0,     // TEXTURE2D
                5 => 2,     // TEXTURE3D
                6 => 1,     // TEXTURECUBE
                _ => -1,
            };
            if (type < 0)
            {
                result.Error = $"t{pairs[i].Texture} has resource dimension {dimension}, " +
                               "which has no MGFX sampler type";
                return result;
            }

            string name = $"{stage}_s{i}";
            samplerNames[pairs[i]] = name;
            result.Samplers.Add(new Sampler
            {
                Name = name,
                TextureRegister = pairs[i].Texture,
                SamplerRegister = pairs[i].Sampler,
                Type = type,
            });
        }

        // ---- emit ----------------------------------------------------------------------------
        var body = new StringBuilder();
        var emitter = new Emitter(result, inputNames, outputNames, samplerNames, body);

        var executable = instructions
            .Where(i => !i.Opcode.StartsWith("dcl_", StringComparison.Ordinal) &&
                        i.Opcode != "customdata" && i.Opcode != "nop")
            .ToList();

        for (int i = 0; i < executable.Count; i++)
        {
            if (!emitter.Emit(executable[i], isFinal: i == executable.Count - 1))
            {
                result.Error = emitter.Error;
                return result;
            }
        }

        var sb = new StringBuilder();
        sb.Append("#ifdef GL_ES\r\n")
          .Append(result.IsVertexShader ? "precision highp float;\r\n" : "precision mediump float;\r\n")
          .Append("precision mediump int;\r\n")
          .Append("#endif\r\n\r\n");

        if (constantRegisters > 0)
            sb.Append($"uniform vec4 {result.ConstantBufferName}[{constantRegisters}];\r\n");
        if (result.IsVertexShader)
            sb.Append("uniform vec4 posFixup;\r\n");
        foreach (var s in result.Samplers)
            sb.Append($"uniform {SamplerType(s.Type)} {s.Name};\r\n");
        for (int i = 0; i < temps; i++)
            sb.Append($"vec4 r{i};\r\n");
        foreach (int reg in inputRegisters)
        {
            if (!result.IsVertexShader)
            {
                if (inputNames[reg] == "gl_FragCoord") continue;
                sb.Append($"varying vec4 {inputNames[reg]};\r\n");
            }
            else
            {
                sb.Append($"attribute vec4 {inputNames[reg]};\r\n");
            }
        }
        if (result.IsVertexShader)
        {
            foreach (int reg in outputRegisters)
            {
                if (positionOutputs.Contains(reg)) continue;
                sb.Append($"varying vec4 {outputNames[reg]};\r\n");
            }
        }

        // The trailing ret was suppressed, so control reaches the end of the body and the fixup
        // applies there.
        emitter.EmitPositionFixup();

        sb.Append("\r\nvoid main()\r\n{\r\n");
        sb.Append(body);
        sb.Append("}\r\n");

        result.Glsl = sb.ToString();
        return result;
    }

    private static string SamplerType(int type) => type switch
    {
        1 => "samplerCube", 2 => "sampler3D", _ => "sampler2D",
    };

    /// <summary>
    /// The varying that carries an interpolator register.
    ///
    /// fxc packs several semantics into one register with disjoint masks - skinFX's o1 holds
    /// TEXCOORD0 in .xy and TEXCOORD1 in .zw - so a register maps to one varying named after the
    /// lowest semantic in it. Choosing the lowest rather than whichever the signature happens to
    /// list first is what guarantees the vertex and pixel stages pick the same name for the same
    /// register and therefore link.
    /// </summary>
    private static string VaryingForRegister(List<DxbcSignature.Element> signature, int register)
    {
        var candidates = signature.Where(e => e.Register == register)
            .OrderBy(e => e.SemanticName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(e => e.SemanticIndex)
            .ToList();
        if (candidates.Count == 0) return null;
        return VaryingName(candidates[0].SemanticName, candidates[0].SemanticIndex);
    }

    /// <summary>
    /// The varying name for a semantic. Both stages of an effect go through this, so a vertex
    /// shader's output and the pixel shader's matching input get the same name and link.
    /// </summary>
    private static string VaryingName(string semanticName, int index)
    {
        string upper = (semanticName ?? "").ToUpperInvariant();
        return upper switch
        {
            "TEXCOORD" => $"vTexCoord{index}",
            "COLOR" => $"vFrontColor{index}",
            _ => $"v{Capitalise(upper)}{index}",
        };
    }

    private static string Capitalise(string s) =>
        s.Length == 0 ? s : char.ToUpperInvariant(s[0]) + s.Substring(1).ToLowerInvariant();

    /// <summary>
    /// Emits one instruction at a time. Split out from Translate so the register/operand
    /// rendering rules live next to the instruction handling that depends on them.
    /// </summary>
    private sealed class Emitter
    {
        private readonly Result _result;
        private readonly Dictionary<int, string> _inputNames;
        private readonly Dictionary<int, string> _outputNames;
        private readonly Dictionary<(int, int), string> _samplerNames;
        private readonly StringBuilder _out;
        private int _indent = 1;

        public string Error;

        public Emitter(Result result, Dictionary<int, string> inputNames,
            Dictionary<int, string> outputNames, Dictionary<(int, int), string> samplerNames,
            StringBuilder output)
        {
            _result = result;
            _inputNames = inputNames;
            _outputNames = outputNames;
            _samplerNames = samplerNames;
            _out = output;
        }

        private void Line(string text)
        {
            _out.Append(new string('\t', Math.Max(1, _indent))).Append(text).Append("\r\n");
        }

        /// <summary>
        /// The correction MonoGame's OpenGL backend supplies posFixup for, and which MojoShader
        /// appends to its own output: y is negated when a render target is bound, because
        /// rendering to an FBO is flipped relative to the back buffer; zw is the half-pixel
        /// offset; and the last line converts DirectX's [0,w] clip depth to OpenGL's [-w,w].
        ///
        /// Emitted before an early return as well as at the end of the shader, so that a shader
        /// which returns from inside a branch still gets it.
        /// </summary>
        public void EmitPositionFixup()
        {
            if (!_result.IsVertexShader) return;
            Line("gl_Position.y = gl_Position.y * posFixup.y;");
            Line("gl_Position.xy += posFixup.zw * gl_Position.ww;");
            Line("gl_Position.z = gl_Position.z * 2.0 - gl_Position.w;");
        }

        public bool Emit(Sm4Disassembler.Instruction ins, bool isFinal)
        {
            switch (ins.Opcode)
            {
                // ---- flow control -----------------------------------------------------------
                case "ret":
                    // A trailing ret must NOT become a `return`. The vertex position fixup is
                    // appended after the body, so returning first makes it dead code - which
                    // silently dropped the DirectX-to-OpenGL depth conversion and the render
                    // target Y flip from every translated vertex shader.
                    if (isFinal) return true;
                    EmitPositionFixup();
                    Line("return;");
                    return true;

                case "if":
                {
                    if (ins.Operands.Count < 1) return Fail("if with no operand");
                    string cond = Scalar(ins.Operands[0]);
                    Line($"if ({cond} {(ins.TestNonZero ? "!=" : "==")} 0.0)");
                    Line("{");
                    _indent++;
                    return true;
                }

                case "else":
                    _indent--;
                    Line("}");
                    Line("else");
                    Line("{");
                    _indent++;
                    return true;

                case "endif":
                    _indent--;
                    Line("}");
                    return true;

                case "discard":
                {
                    if (ins.Operands.Count < 1) return Fail("discard with no operand");
                    string cond = Scalar(ins.Operands[0]);
                    Line($"if ({cond} {(ins.TestNonZero ? "!=" : "==")} 0.0) discard;");
                    return true;
                }

                // ---- texture ----------------------------------------------------------------
                case "sample":
                    return EmitSample(ins);

                // ---- per-component arithmetic -----------------------------------------------
                case "mov":
                    return EmitMove(ins);

                case "ishl":
                    return EmitShiftLeft(ins);

                case "add": return Binary(ins, "{0} + {1}");
                case "mul": return Binary(ins, "{0} * {1}");
                case "div": return Binary(ins, "{0} / {1}");
                case "min": return Binary(ins, "min({0}, {1})");
                case "max": return Binary(ins, "max({0}, {1})");
                case "and": return Binary(ins, "{0} * {1}");        // masks: AND is a product
                case "or": return Binary(ins, "max({0}, {1})");     // masks: OR is a maximum
                case "mad": return Ternary(ins, "{0} * {1} + {2}");
                case "exp": return Unary(ins, "exp2({0})");
                case "log": return Unary(ins, "log2({0})");
                case "rsq": return Unary(ins, "inversesqrt({0})");
                case "sqrt": return Unary(ins, "sqrt({0})");
                case "frc": return Unary(ins, "fract({0})");
                case "round_ne": return Unary(ins, "floor({0} + 0.5)");
                case "round_ni": return Unary(ins, "floor({0})");
                case "round_pi": return Unary(ins, "ceil({0})");
                case "round_z": return Unary(ins, "sign({0}) * floor(abs({0}))");
                case "itof":
                case "utof":
                case "ftoi":
                case "ftou":
                    // Integers are modelled as floats throughout, so a conversion is a move.
                    return EmitMove(ins);

                // ---- comparisons, which yield 1.0/0.0 masks ---------------------------------
                case "eq":
                case "ieq": return Compare(ins, "equal");
                case "ne":
                case "ine": return Compare(ins, "notEqual");
                case "lt":
                case "ilt":
                case "ult": return Compare(ins, "lessThan");
                case "ge":
                case "ige":
                case "uge": return Compare(ins, "greaterThanEqual");

                case "movc": return EmitSelect(ins);

                case "sincos": return EmitSinCos(ins);

                // ---- dot products, whose result is scalar and replicated --------------------
                case "dp2": return Dot(ins, 2);
                case "dp3": return Dot(ins, 3);
                case "dp4": return Dot(ins, 4);

                default:
                    return Fail($"opcode '{ins.Opcode}' is not translated");
            }
        }

        private bool Fail(string why)
        {
            Error = why;
            return false;
        }

        // ---- operand rendering -----------------------------------------------------------------

        /// <summary>The register part of an operand, without any swizzle or mask.</summary>
        private string Register(Sm4Disassembler.Operand op)
        {
            switch (op.Type)
            {
                case Sm4Disassembler.OperandType.Temp:
                    return "r" + op.Indices[0];
                case Sm4Disassembler.OperandType.Input:
                    return _inputNames.TryGetValue((int)op.Indices[0], out string vin)
                        ? vin : null;
                case Sm4Disassembler.OperandType.Output:
                    return _outputNames.TryGetValue((int)op.Indices[0], out string vout)
                        ? vout : null;
                case Sm4Disassembler.OperandType.ConstantBuffer:
                {
                    if (op.Indices.Count < 2) return null;
                    if (op.RelativeIndex == null)
                        return $"{_result.ConstantBufferName}[{op.Indices[1]}]";

                    // cb0[r2.y + 10] - skinFX indexes its bone-matrix palette this way. GLSL
                    // allows a non-constant index into a uniform array in a vertex shader, which
                    // is where all of these are. The index register holds an integer value that
                    // this translator carries as a float, so it is converted back explicitly.
                    if (op.RelativeIndexDimension != 1) return null;
                    string index = Scalar(op.RelativeIndex);
                    if (index == null) return null;
                    return $"{_result.ConstantBufferName}[int({index}) + {op.Indices[1]}]";
                }
                default:
                    return null;
            }
        }

        /// <summary>Which components a destination operand writes, in ascending order.</summary>
        private static List<int> DestinationComponents(Sm4Disassembler.Operand op)
        {
            var list = new List<int>();
            int mask = op.SelectionMode == 0 ? op.MaskOrSwizzle : 0xF;
            if (mask == 0) mask = 0xF;
            for (int i = 0; i < 4; i++) if ((mask & (1 << i)) != 0) list.Add(i);
            return list;
        }

        private string Destination(Sm4Disassembler.Operand op, List<int> components)
        {
            string register = Register(op);
            if (register == null) return null;
            if (components.Count == 4) return register;
            var sb = new StringBuilder(register).Append('.');
            foreach (int c in components) sb.Append(Components[c]);
            return sb.ToString();
        }

        /// <summary>
        /// A source operand narrowed to the destination's components. SM4 is component-wise and
        /// aligned by component index, so component slot c of the result reads the source's
        /// swizzle entry at position c - not the c'th written component.
        /// </summary>
        private string Source(Sm4Disassembler.Operand op, List<int> components)
        {
            if (op.Type == Sm4Disassembler.OperandType.Immediate32 ||
                op.Type == Sm4Disassembler.OperandType.Immediate64)
                return Immediate(op, components);

            string register = Register(op);
            if (register == null) return null;

            var sb = new StringBuilder();
            if (op.SelectionMode == 2)
            {
                // select-one: every component reads the same one
                sb.Append(register).Append('.').Append(Components[op.MaskOrSwizzle & 3]);
                if (components.Count > 1)
                    return Modify(op, $"vec{components.Count}({sb})");
                return Modify(op, sb.ToString());
            }

            sb.Append(register).Append('.');
            foreach (int c in components)
            {
                int source = op.SelectionMode == 1 ? (op.MaskOrSwizzle >> (c * 2)) & 3 : c;
                sb.Append(Components[source]);
            }
            return Modify(op, sb.ToString());
        }

        private static string Modify(Sm4Disassembler.Operand op, string text)
        {
            if (op.Modifier == 2 || op.Modifier == 3) text = $"abs({text})";
            if (op.Modifier == 1 || op.Modifier == 3) text = $"-({text})";
            return text;
        }

        private static string Immediate(Sm4Disassembler.Operand op, List<int> components)
        {
            float[] values = op.ImmediateFloats;
            if (values == null || values.Length == 0) return null;

            var parts = new List<string>();
            foreach (int c in components)
            {
                int index = values.Length == 1 ? 0
                    : (op.SelectionMode == 1 ? (op.MaskOrSwizzle >> (c * 2)) & 3
                    : op.SelectionMode == 2 ? op.MaskOrSwizzle & 3
                    : c);
                if (index >= values.Length) index = values.Length - 1;
                parts.Add(Literal(values[index], op.ImmediateBits?[index] ?? 0));
            }

            if (parts.Count == 1) return parts[0];
            return $"vec{parts.Count}({string.Join(", ", parts)})";
        }

        /// <summary>
        /// A float literal. An immediate carrying an integer bit pattern - which is how fxc
        /// spells a bool or an index - is emitted as the integer's value, not as the float that
        /// its bits happen to describe.
        /// </summary>
        private static string Literal(float value, uint bits)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                return ((int)bits).ToString(CultureInfo.InvariantCulture) + ".0";

            // A tiny denormal is almost certainly a small integer's bit pattern rather than a
            // real float constant.
            if (value != 0f && Math.Abs(value) < 1e-30f)
                return ((int)bits).ToString(CultureInfo.InvariantCulture) + ".0";

            string text = value.ToString("R", CultureInfo.InvariantCulture);
            if (text.IndexOf('.') < 0 && text.IndexOf('e') < 0 && text.IndexOf('E') < 0)
                text += ".0";
            return text;
        }

        /// <summary>A single component of an operand, for the scalar tests if and discard use.</summary>
        private string Scalar(Sm4Disassembler.Operand op)
        {
            if (op.Type == Sm4Disassembler.OperandType.Immediate32)
                return Immediate(op, new List<int> { 0 });

            string register = Register(op);
            if (register == null) return null;
            int component = op.SelectionMode switch
            {
                1 => op.MaskOrSwizzle & 3,
                2 => op.MaskOrSwizzle & 3,
                _ => 0,
            };
            return Modify(op, $"{register}.{Components[component]}");
        }

        private string Wrap(Sm4Disassembler.Instruction ins, string expression) =>
            ins.Saturate ? $"clamp({expression}, 0.0, 1.0)" : expression;

        // ---- instruction shapes ----------------------------------------------------------------

        private bool Assign(Sm4Disassembler.Instruction ins, string expression, List<int> components)
        {
            string dest = Destination(ins.Operands[0], components);
            if (dest == null) return Fail($"'{ins.Opcode}' has a destination this cannot render");
            if (expression == null) return Fail($"'{ins.Opcode}' has a source this cannot render");
            Line($"{dest} = {Wrap(ins, expression)};");
            return true;
        }

        private bool EmitMove(Sm4Disassembler.Instruction ins)
        {
            if (ins.Operands.Count < 2) return Fail($"'{ins.Opcode}' needs a source");
            var components = DestinationComponents(ins.Operands[0]);
            return Assign(ins, Source(ins.Operands[1], components), components);
        }

        /// <summary>
        /// ishl. With integers modelled as floats this is a multiply by a power of two, which is
        /// exact for the small indices it is used on. skinFX uses it to turn a bone index into a
        /// matrix-palette register offset (a shift by 2, i.e. four registers per matrix), so
        /// treating it as a move - which an earlier version did, misled by the disassembler
        /// printing the integer immediate 2 as 0 - silently collapsed the whole palette onto
        /// bone 0.
        /// </summary>
        private bool EmitShiftLeft(Sm4Disassembler.Instruction ins)
        {
            if (ins.Operands.Count < 3) return Fail("ishl needs two sources");
            var shift = ins.Operands[2];
            if (shift.Type != Sm4Disassembler.OperandType.Immediate32 ||
                shift.ImmediateBits == null || shift.ImmediateBits.Length == 0)
                return Fail("ishl by a non-immediate amount is not translated");

            int by = (int)shift.ImmediateBits[0];
            if (by < 0 || by > 23) return Fail($"ishl by {by} is out of the exactly-representable range");

            var components = DestinationComponents(ins.Operands[0]);
            string a = Source(ins.Operands[1], components);
            if (a == null) return Fail("ishl has a source this cannot render");
            if (by == 0) return Assign(ins, a, components);

            string factor = (1 << by).ToString(CultureInfo.InvariantCulture) + ".0";
            return Assign(ins, $"{a} * {factor}", components);
        }

        private bool Unary(Sm4Disassembler.Instruction ins, string form)
        {
            if (ins.Operands.Count < 2) return Fail($"'{ins.Opcode}' needs a source");
            var components = DestinationComponents(ins.Operands[0]);
            string a = Source(ins.Operands[1], components);
            if (a == null) return Fail($"'{ins.Opcode}' has a source this cannot render");
            return Assign(ins, string.Format(CultureInfo.InvariantCulture, form, a), components);
        }

        private bool Binary(Sm4Disassembler.Instruction ins, string form)
        {
            if (ins.Operands.Count < 3) return Fail($"'{ins.Opcode}' needs two sources");
            var components = DestinationComponents(ins.Operands[0]);
            string a = Source(ins.Operands[1], components);
            string b = Source(ins.Operands[2], components);
            if (a == null || b == null) return Fail($"'{ins.Opcode}' has a source this cannot render");
            return Assign(ins, string.Format(CultureInfo.InvariantCulture, form, a, b), components);
        }

        private bool Ternary(Sm4Disassembler.Instruction ins, string form)
        {
            if (ins.Operands.Count < 4) return Fail($"'{ins.Opcode}' needs three sources");
            var components = DestinationComponents(ins.Operands[0]);
            string a = Source(ins.Operands[1], components);
            string b = Source(ins.Operands[2], components);
            string c = Source(ins.Operands[3], components);
            if (a == null || b == null || c == null)
                return Fail($"'{ins.Opcode}' has a source this cannot render");
            return Assign(ins, string.Format(CultureInfo.InvariantCulture, form, a, b, c), components);
        }

        /// <summary>
        /// A comparison, whose SM4 result is an integer bitmask and whose GLSL stand-in is a
        /// 1.0/0.0 float. The and/or/movc/if that consume it are all written to match.
        /// </summary>
        private bool Compare(Sm4Disassembler.Instruction ins, string function)
        {
            if (ins.Operands.Count < 3) return Fail($"'{ins.Opcode}' needs two sources");
            var components = DestinationComponents(ins.Operands[0]);
            string a = Source(ins.Operands[1], components);
            string b = Source(ins.Operands[2], components);
            if (a == null || b == null) return Fail($"'{ins.Opcode}' has a source this cannot render");

            string expression = components.Count == 1
                ? $"float({a} {ScalarOperator(function)} {b})"
                : $"vec{components.Count}({function}({a}, {b}))";
            return Assign(ins, expression, components);
        }

        private static string ScalarOperator(string function) => function switch
        {
            "equal" => "==", "notEqual" => "!=", "lessThan" => "<", "greaterThanEqual" => ">=",
            _ => "==",
        };

        /// <summary>
        /// movc: per-component select on a mask. Written as a mix against a freshly recomputed
        /// 0/1 weight rather than using the mask directly, because the mask may be an integer bit
        /// pattern - a bool parameter arrives in the constant buffer as the integer 1, which a
        /// vec4 uniform reads as a denormal - and mixing by that value would be wrong.
        /// </summary>
        private bool EmitSelect(Sm4Disassembler.Instruction ins)
        {
            if (ins.Operands.Count < 4) return Fail("movc needs three sources");
            var components = DestinationComponents(ins.Operands[0]);
            string c = Source(ins.Operands[1], components);
            string a = Source(ins.Operands[2], components);
            string b = Source(ins.Operands[3], components);
            if (a == null || b == null || c == null)
                return Fail("movc has a source this cannot render");

            int n = components.Count;
            string weight = n == 1
                ? $"float({c} != 0.0)"
                : $"vec{n}(notEqual({c}, vec{n}(0.0)))";
            return Assign(ins, $"mix({b}, {a}, {weight})", components);
        }

        /// <summary>
        /// sincos writes the sine to its first destination and the cosine to its second, and
        /// either may be the null register when only one is wanted.
        /// </summary>
        private bool EmitSinCos(Sm4Disassembler.Instruction ins)
        {
            if (ins.Operands.Count < 3) return Fail("sincos needs two destinations and a source");
            var sinDest = ins.Operands[0];
            var cosDest = ins.Operands[1];
            var source = ins.Operands[2];

            if (sinDest.Type != Sm4Disassembler.OperandType.Null)
            {
                var components = DestinationComponents(sinDest);
                string a = Source(source, components);
                string dest = Destination(sinDest, components);
                if (a == null || dest == null) return Fail("sincos has an operand this cannot render");
                Line($"{dest} = {Wrap(ins, $"sin({a})")};");
            }

            if (cosDest.Type != Sm4Disassembler.OperandType.Null)
            {
                var components = DestinationComponents(cosDest);
                string a = Source(source, components);
                string dest = Destination(cosDest, components);
                if (a == null || dest == null) return Fail("sincos has an operand this cannot render");
                Line($"{dest} = {Wrap(ins, $"cos({a})")};");
            }

            return true;
        }

        private bool Dot(Sm4Disassembler.Instruction ins, int size)
        {
            if (ins.Operands.Count < 3) return Fail($"'{ins.Opcode}' needs two sources");
            var components = DestinationComponents(ins.Operands[0]);

            // A dot product reads a fixed number of components regardless of the destination
            // mask, so the sources are taken over 0..size-1 rather than over the mask.
            var read = new List<int>();
            for (int i = 0; i < size; i++) read.Add(i);
            string a = Source(ins.Operands[1], read);
            string b = Source(ins.Operands[2], read);
            if (a == null || b == null) return Fail($"'{ins.Opcode}' has a source this cannot render");

            string scalar = $"dot({a}, {b})";
            string expression = components.Count == 1 ? scalar : $"vec{components.Count}({scalar})";
            return Assign(ins, expression, components);
        }

        private bool EmitSample(Sm4Disassembler.Instruction ins)
        {
            if (ins.Operands.Count < 4) return Fail("sample needs four operands");
            var dest = ins.Operands[0];
            var coord = ins.Operands[1];
            var resource = ins.Operands[2];
            var sampler = ins.Operands[3];

            if (resource.Type != Sm4Disassembler.OperandType.Resource ||
                sampler.Type != Sm4Disassembler.OperandType.Sampler ||
                resource.Indices.Count < 1 || sampler.Indices.Count < 1)
                return Fail("sample has operands this cannot render");

            var key = ((int)resource.Indices[0], (int)sampler.Indices[0]);
            if (!_samplerNames.TryGetValue(key, out string name))
                return Fail($"sample uses t{key.Item1}/s{key.Item2}, which was not declared");

            var entry = _result.Samplers.First(s => s.Name == name);
            int coordSize = entry.Type switch { 1 => 3, 2 => 3, 3 => 1, _ => 2 };
            var read = new List<int>();
            for (int i = 0; i < coordSize; i++) read.Add(i);
            string uv = Source(coord, read);
            if (uv == null) return Fail("sample has a coordinate this cannot render");

            string function = entry.Type switch
            {
                1 => "textureCube", 2 => "texture3D", 3 => "texture1D", _ => "texture2D",
            };

            // The fetch is always four components; the destination mask then selects from it.
            var components = DestinationComponents(dest);
            string fetch = $"{function}({name}, {uv})";
            if (components.Count != 4)
            {
                var sb = new StringBuilder(".");
                foreach (int c in components)
                {
                    int source = dest.SelectionMode == 0 ? c : c;
                    sb.Append(Components[source]);
                }
                fetch = $"({fetch}){sb}";
            }
            return Assign(ins, fetch, components);
        }
    }
}
