using System;
using System.IO;
using System.Text;
using System.Globalization;

namespace UW.Tools.MgfxTranscode;

/// <summary>
/// A read-only parser for MGFX v10 and v11, written to mirror MonoGame's
/// Graphics/Effect/Effect.cs (ReadEffect / ReadParameters / ReadAnnotations / ReadPasses) and
/// Graphics/Shader/Shader.cs field for field.
///
/// BOTH versions, because they are both loadable and both are produced here. 3.8.5.1's Effect
/// has MGFXVersion 11 and MGFXMinVersion 10, so its runtime accepts either; the transcoder
/// writes v10 (portable back to 3.8.1), while mgfxc 3.8.5.1 - which is what compiles the
/// OpenGL-profile effects from HLSL - emits only v11. Accepting exactly one of them made the
/// whole HLSL route unusable: every `inject` failed with "Expected MGFX v10, found v11" for
/// blobs the game would have loaded perfectly well.
///
/// The single difference is in the shader block: v11 writes SourceFile and Entrypoint as two
/// length-prefixed strings after the stage flag. Nothing else moved. (Read from the shipped
/// MonoGame.Framework.dll: `if (version > 10) { SourceFile = reader.ReadString(); Entrypoint =
/// reader.ReadString(); }`.)
///
/// This exists purely to validate <see cref="MgfxRewriter"/> output without needing a GPU or a
/// live MonoGame GraphicsDevice. It is only trustworthy because it is first pointed at genuine
/// v10 files produced by MonoGame's OWN writer - the six stock effects embedded in
/// MonoGame.Framework.dll 3.8.2 (AlphaTest, Basic, DualTexture, EnvironmentMap, Skinned,
/// Sprite). If it parses those exactly, the v10 layout encoded here is correct; only then does
/// "it parses our transcoded effects exactly" mean anything.
///
/// "Exactly" means: consumes every byte, and the trailing int32 equals the MGFX signature -
/// which is precisely the completeness check MonoGame's own Effect constructor performs.
/// </summary>
internal sealed class MgfxV10Reader
{
    private static readonly int MgfxSignature = BitConverter.IsLittleEndian ? 0x5846474D : 0x4D474658;

    private readonly BinaryReader _r;

    /// <summary>Lowest container version this parses - and the lowest any 3.8.x runtime takes.</summary>
    public const int MinVersion = 10;

    /// <summary>Highest container version this parses; matches 3.8.5.1's MGFXVersion.</summary>
    public const int MaxVersion = 11;

    private readonly int _version;

    private MgfxV10Reader(BinaryReader r, int version)
    {
        _r = r;
        _version = version;
    }

    internal readonly record struct Stats(
        int Version,
        int Profile,
        int EffectKey,
        int ConstantBuffers,
        int Shaders,
        int Parameters,
        int Techniques,
        int Passes,
        int ShaderBytecodeBytes)
    {
        public override string ToString() =>
            $"v{Version} profile={Profile} cbuffers={ConstantBuffers} shaders={Shaders} " +
            $"params={Parameters} techniques={Techniques} passes={Passes} " +
            $"bytecode={ShaderBytecodeBytes}B";
    }

    private int _constantBuffers, _shaders, _parameters, _techniques, _passes, _bytecodeBytes;

    private int _parameterDepth;
    private readonly System.Collections.Generic.List<string> _parameterNames = new();
    private readonly System.Collections.Generic.Dictionary<string, string> _parameterDefaults =
        new(StringComparer.Ordinal);

    /// <summary>
    /// Where one top-level parameter's initial value sits in the blob, so it can be rewritten.
    ///
    /// Byte-addressed rather than re-serialised: the values are fixed-width and in place, so a
    /// default can be corrected without an MGFX writer and without disturbing a single other byte.
    /// </summary>
    internal readonly record struct DefaultSpan(
        string Name, byte Type, int ValueCount, int Offset, int ByteLength);

    private readonly System.Collections.Generic.List<DefaultSpan> _parameterSpans = new();

    /// <summary>The byte span of every top-level parameter's initial value.</summary>
    public static System.Collections.Generic.List<DefaultSpan> ReadDefaultSpans(byte[] blob)
    {
        using var ms = new MemoryStream(blob, writable: false);
        using var reader = new BinaryReader(ms, Encoding.UTF8);
        reader.ReadInt32();
        int version = reader.ReadByte();
        reader.ReadByte();
        reader.ReadInt32();
        var parser = new MgfxV10Reader(reader, version);
        parser.ReadBody();
        return parser._parameterSpans;
    }

    /// <summary>
    /// One parsed shader block, for <see cref="ReadShaders"/>.
    ///
    /// <paramref name="LengthFieldOffset"/> and <paramref name="BytecodeOffset"/> are absolute
    /// offsets into the blob that was parsed. They exist so the payload can be REPLACED with
    /// something of a different length: on the OpenGL profile the "bytecode" is GLSL source
    /// text, and MonoGame's shader compiler emits at least one construct no driver will accept
    /// (see <see cref="GlslFixups"/>). Nothing in MGFX stores an absolute offset, so splicing a
    /// longer or shorter payload in and rewriting its int32 length prefix is sufficient.
    /// </summary>
    internal readonly record struct ShaderBlock(
        bool IsVertexShader, byte[] Bytecode, int Samplers, int Attributes,
        int LengthFieldOffset = 0, int BytecodeOffset = 0);

    private readonly System.Collections.Generic.List<ShaderBlock> _blocks = new();

    /// <summary>
    /// Parses <paramref name="blob"/> as MGFX v10. Throws <see cref="InvalidDataException"/>
    /// unless the whole blob is consumed and the tail signature is present.
    /// </summary>
    public static Stats Validate(byte[] blob)
    {
        using var ms = new MemoryStream(blob, writable: false);
        using var reader = new BinaryReader(ms, Encoding.UTF8);
        int signature = reader.ReadInt32();
        if (signature != MgfxSignature)
            throw new InvalidDataException("Not an MGFX blob (bad header signature).");

        int version = reader.ReadByte();
        int profile = reader.ReadByte();
        int effectKey = reader.ReadInt32();

        if (version < MinVersion || version > MaxVersion)
            throw new InvalidDataException(
                $"Expected MGFX v{MinVersion} or v{MaxVersion}, found v{version}.");

        var parser = new MgfxV10Reader(reader, version);
        parser.ReadBody();

        // MonoGame's Effect ctor reads this tail and throws if it is not the signature; it is
        // how it confirms the blob was parsed correctly.
        int tail = reader.ReadInt32();
        if (tail != MgfxSignature)
            throw new InvalidDataException(
                $"Tail signature missing or wrong (0x{tail:X8}); MonoGame would reject this effect.");

        if (ms.Position != ms.Length)
            throw new InvalidDataException(
                $"Parsed {ms.Position} of {ms.Length} bytes - {ms.Length - ms.Position} trailing byte(s).");

        return new Stats(version, profile, effectKey, parser._constantBuffers, parser._shaders,
            parser._parameters, parser._techniques, parser._passes, parser._bytecodeBytes);
    }

    private void ReadBody()
    {
        _constantBuffers = _r.ReadInt32();
        for (int i = 0; i < _constantBuffers; i++)
        {
            _r.ReadString();                        // name
            _r.ReadInt16();                         // sizeInBytes
            int parameterCount = _r.ReadInt32();
            for (int j = 0; j < parameterCount; j++)
            {
                _r.ReadInt32();                     // parameter index
                _r.ReadUInt16();                    // offset
            }
        }

        _shaders = _r.ReadInt32();
        for (int i = 0; i < _shaders; i++)
            ReadShader();

        _parameters = ReadParameters();

        _techniques = _r.ReadInt32();
        for (int i = 0; i < _techniques; i++)
        {
            _r.ReadString();                        // name
            ReadAnnotations();
            int passCount = _r.ReadInt32();
            _passes += passCount;
            for (int p = 0; p < passCount; p++)
            {
                _r.ReadString();                    // name
                ReadAnnotations();
                _r.ReadInt32();                     // vertex shader index (negative = none)
                _r.ReadInt32();                     // pixel shader index  (negative = none)
                if (_r.ReadBoolean()) ReadBlendState();
                if (_r.ReadBoolean()) ReadDepthStencilState();
                if (_r.ReadBoolean()) ReadRasterizerState();
            }
        }
    }

    private int ReadParameters()
    {
        int count = _r.ReadInt32();
        _parameterDepth++;
        for (int i = 0; i < count; i++)
        {
            _r.ReadByte();                          // EffectParameterClass
            byte type = _r.ReadByte();              // EffectParameterType
            string name = _r.ReadString();          // name
            // Top level only: array elements and struct members repeat the parent's name and are
            // not what a caller means by "the parameters this effect exposes".
            if (_parameterDepth == 1)
            {
                _parameterNames.Add(name);
            }
            _r.ReadString();                        // semantic
            ReadAnnotations();
            int rowCount = _r.ReadByte();
            int columnCount = _r.ReadByte();

            int elements = ReadParameters();
            int structMembers = ReadParameters();

            if (elements != 0 || structMembers != 0)
                continue;

            int values = rowCount * columnCount;
            int defaultsOffset = (int)_r.BaseStream.Position;
            var defaults = new System.Collections.Generic.List<string>(values);
            switch (type)
            {
                case 1: // Bool
                case 2: // Int32
                    for (int v = 0; v < values; v++)
                        defaults.Add(_r.ReadInt32().ToString(CultureInfo.InvariantCulture));
                    break;
                case 3: // Single
                    for (int v = 0; v < values; v++)
                        defaults.Add(_r.ReadSingle().ToString("0.####", CultureInfo.InvariantCulture));
                    break;
                case 4: // String
                    throw new NotSupportedException("MGFX string parameters are not supported.");
                default:
                    break;
            }

            // The parameter's INITIAL VALUE, which for this game is not a detail. Several effects
            // declare uniforms with an initialiser and the game never assigns them - Billboard's
            // LightColor and AmbientColorForNormalMapping among them - so the initialiser IS the
            // lighting. An effect that keeps the parameter but loses its default multiplies every
            // sprite by zero and draws the whole world as black silhouettes.
            if (_parameterDepth == 1 && defaults.Count > 0)
            {
                _parameterDefaults[name] = string.Join(", ", defaults);
                _parameterSpans.Add(new DefaultSpan(
                    name, type, defaults.Count, defaultsOffset,
                    (int)_r.BaseStream.Position - defaultsOffset));
            }
        }
        _parameterDepth--;
        return count;
    }

    private void ReadAnnotations() => _r.ReadInt32();   // count only; no payload is stored

    private void ReadShader()
    {
        bool isVertexShader = _r.ReadBoolean();
        if (_version > 10)
        {
            _r.ReadString();                        // SourceFile (v11 only)
            _r.ReadString();                        // Entrypoint (v11 only)
        }
        int lengthFieldOffset = (int)_r.BaseStream.Position;
        int bytecodeLength = _r.ReadInt32();
        int bytecodeOffset = (int)_r.BaseStream.Position;
        _bytecodeBytes += bytecodeLength;
        byte[] bytecode = _r.ReadBytes(bytecodeLength);
        if (bytecode.Length != bytecodeLength)
            throw new InvalidDataException("Shader bytecode length runs past the end of the blob.");

        int samplerCount = _r.ReadByte();
        for (int s = 0; s < samplerCount; s++)
        {
            _r.ReadByte();                          // SamplerType
            _r.ReadByte();                          // textureSlot
            _r.ReadByte();                          // samplerSlot
            if (_r.ReadBoolean())
            {
                _r.ReadByte();                      // AddressU
                _r.ReadByte();                      // AddressV
                _r.ReadByte();                      // AddressW
                _r.ReadBytes(4);                    // BorderColor
                _r.ReadByte();                      // Filter
                _r.ReadInt32();                     // MaxAnisotropy
                _r.ReadInt32();                     // MaxMipLevel
                _r.ReadSingle();                    // MipMapLevelOfDetailBias
            }
            _r.ReadString();                        // name
            _r.ReadByte();                          // parameter index
        }

        int cbufferCount = _r.ReadByte();
        _r.ReadBytes(cbufferCount);

        int attributeCount = _r.ReadByte();
        for (int a = 0; a < attributeCount; a++)
        {
            _r.ReadString();                        // name
            _r.ReadByte();                          // VertexElementUsage
            _r.ReadByte();                          // index
            _r.ReadInt16();                         // location
        }

        _blocks.Add(new ShaderBlock(isVertexShader, bytecode, samplerCount, attributeCount,
            lengthFieldOffset, bytecodeOffset));
    }

    /// <summary>
    /// The names of the parameters an effect exposes, in declaration order.
    ///
    /// This is a compatibility surface, not a curiosity. The game reaches parameters by name and
    /// does not check - <c>effect.Parameters["WindowPosition"].SetValue(...)</c> - so a rebuilt
    /// effect that exposes FEWER parameters than the shipped one crashes with a null reference
    /// the first frame that draw runs. mgfxc drops any uniform the shader does not read, and the
    /// studio's current sources have commented-out code whose parameters the shipped binaries
    /// still carry, so a faithful rebuild is narrower than what it replaces. Comparing the two
    /// sets offline is what turns that into a build failure instead of a crash report.
    /// </summary>
    /// <summary>Parameter name -> its initial value, for the same reason as <see cref="ReadParameterNames"/>.</summary>
    public static System.Collections.Generic.Dictionary<string, string> ReadParameterDefaults(byte[] blob)
    {
        using var ms = new MemoryStream(blob, writable: false);
        using var reader = new BinaryReader(ms, Encoding.UTF8);
        reader.ReadInt32();
        int version = reader.ReadByte();
        reader.ReadByte();
        reader.ReadInt32();
        var parser = new MgfxV10Reader(reader, version);
        parser.ReadBody();
        return parser._parameterDefaults;
    }

    public static System.Collections.Generic.List<string> ReadParameterNames(byte[] blob)
    {
        using var ms = new MemoryStream(blob, writable: false);
        using var reader = new BinaryReader(ms, Encoding.UTF8);
        reader.ReadInt32();                     // signature
        int version = reader.ReadByte();
        reader.ReadByte();                      // profile
        reader.ReadInt32();                     // effect key
        var parser = new MgfxV10Reader(reader, version);
        parser.ReadBody();
        return parser._parameterNames;
    }

    /// <summary>Parses the blob and returns its shader blocks, for DXBC analysis.</summary>
    public static System.Collections.Generic.List<ShaderBlock> ReadShaders(byte[] blob)
    {
        using var ms = new MemoryStream(blob, writable: false);
        using var reader = new BinaryReader(ms, Encoding.UTF8);
        reader.ReadInt32();                     // signature
        int version = reader.ReadByte();
        reader.ReadByte();                      // profile
        reader.ReadInt32();                     // effect key
        var parser = new MgfxV10Reader(reader, version);
        parser.ReadBody();
        return parser._blocks;
    }

    private void ReadBlendState()
    {
        _r.ReadBytes(3);        // Alpha blend function / destination / source
        _r.ReadBytes(4);        // BlendFactor
        _r.ReadBytes(3);        // Color blend function / destination / source
        _r.ReadBytes(4);        // ColorWriteChannels 0..3
        _r.ReadInt32();         // MultiSampleMask
    }

    private void ReadDepthStencilState()
    {
        _r.ReadBytes(4);        // counter-clockwise stencil ops / function
        _r.ReadBoolean();       // DepthBufferEnable
        _r.ReadByte();          // DepthBufferFunction
        _r.ReadBoolean();       // DepthBufferWriteEnable
        _r.ReadInt32();         // ReferenceStencil
        _r.ReadByte();          // StencilDepthBufferFail
        _r.ReadBoolean();       // StencilEnable
        _r.ReadByte();          // StencilFail
        _r.ReadByte();          // StencilFunction
        _r.ReadInt32();         // StencilMask
        _r.ReadByte();          // StencilPass
        _r.ReadInt32();         // StencilWriteMask
        _r.ReadBoolean();       // TwoSidedStencilMode
    }

    private void ReadRasterizerState()
    {
        _r.ReadByte();          // CullMode
        _r.ReadSingle();        // DepthBias
        _r.ReadByte();          // FillMode
        _r.ReadBoolean();       // MultiSampleAntiAlias
        _r.ReadBoolean();       // ScissorTestEnable
        _r.ReadSingle();        // SlopeScaleDepthBias
    }
}
