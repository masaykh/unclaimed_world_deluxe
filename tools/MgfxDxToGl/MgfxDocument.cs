using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UW.Tools.MgfxDxToGl;

/// <summary>
/// A complete, round-trippable object model for an MGFX effect container, versions 10 and 11.
///
/// "Round-trippable" is the design requirement, not a nicety: <c>Read</c> followed by
/// <c>Write</c> at the same version must reproduce the input byte for byte. That is what makes
/// it safe to change one part (the shaders) and trust the rest was preserved rather than
/// re-derived. Program.cs exposes it as `roundtrip` and it runs over all 19 shipped effects.
///
/// Layout, from MonoGame's Effect.ReadHeader / Shader.Read and mgfxc's EffectObject.Write:
///
///   'M' 'G' 'F' 'X'        4 bytes
///   version                1 byte   (10 or 11)
///   profile                1 byte   (0 = OpenGL, 1 = DirectX_11)
///   effectKey              int32    a hash of the body; MonoGame uses it only as a cache key
///   constantBufferCount    int32, then that many constant buffers
///   shaderCount            int32, then that many shaders
///   parameters             (recursive; see ReadParameters)
///   techniqueCount         int32, then that many techniques
///   'M' 'G' 'F' 'X'        4 bytes  (MonoGame throws if this tail is missing)
///
/// The only difference between v10 and v11 is that a v11 shader block carries two extra
/// strings, sourceFile and entrypoint, right after the isVertexShader flag. Everything else is
/// identical, which is why a v10 container loads on every 3.8.2+ runtime and is what this tool
/// writes.
/// </summary>
internal sealed class MgfxDocument
{
    public const int Signature = 0x5846474D;   // 'M','G','F','X' little-endian
    public const int VersionV10 = 10;
    public const int VersionV11 = 11;

    public const byte ProfileOpenGL = 0;
    public const byte ProfileDirectX11 = 1;

    public int Version { get; set; }
    public byte Profile { get; set; }
    public int EffectKey { get; set; }

    public List<ConstantBuffer> ConstantBuffers { get; } = new();
    public List<Shader> Shaders { get; } = new();
    public List<Parameter> Parameters { get; } = new();
    public List<Technique> Techniques { get; } = new();

    // ---------------------------------------------------------------------------------------

    internal sealed class ConstantBuffer
    {
        public string Name = string.Empty;
        public ushort SizeInBytes;

        /// <summary>Index into <see cref="MgfxDocument.Parameters"/>.</summary>
        public List<int> ParameterIndex = new();

        /// <summary>Byte offset of that parameter within the buffer.</summary>
        public List<ushort> ParameterOffset = new();

        public ConstantBuffer Clone() => new()
        {
            Name = Name,
            SizeInBytes = SizeInBytes,
            ParameterIndex = new List<int>(ParameterIndex),
            ParameterOffset = new List<ushort>(ParameterOffset),
        };
    }

    internal sealed class SamplerState
    {
        public byte AddressU, AddressV, AddressW;
        public byte[] BorderColor = new byte[4];
        public byte Filter;
        public int MaxAnisotropy;
        public int MaxMipLevel;
        public float MipMapLevelOfDetailBias;
    }

    internal sealed class Sampler
    {
        public byte Type;          // SamplerType: 0 Texture2D, 1 TextureCube, 2 Texture3D, 3 Texture1D
        public byte TextureSlot;
        public byte SamplerSlot;
        public SamplerState State;  // null when the effect does not set one
        public string Name = string.Empty;
        public byte ParameterIndex;
    }

    internal sealed class Attribute
    {
        public string Name = string.Empty;
        public byte Usage;         // VertexElementUsage
        public byte Index;
        public short Location;
    }

    internal sealed class Shader
    {
        public bool IsVertexShader;

        /// <summary>v11 only. Ignored when writing v10.</summary>
        public string SourceFile = "<unknown>";

        /// <summary>v11 only. Ignored when writing v10.</summary>
        public string Entrypoint = "<unknown>";

        /// <summary>DXBC for the DirectX profile, GLSL source text for the OpenGL profile.</summary>
        public byte[] ShaderCode = Array.Empty<byte>();

        public List<Sampler> Samplers = new();

        /// <summary>Indices into <see cref="MgfxDocument.ConstantBuffers"/>.</summary>
        public List<byte> ConstantBuffers = new();

        public List<Attribute> Attributes = new();
    }

    internal sealed class Annotation
    {
        // The container stores an annotation count, and mgfxc always writes 0 - the parser it
        // pairs with never populated them. Kept as a count so a non-zero one is not silently
        // dropped.
        public Parameter Parameter;
    }

    internal sealed class Parameter
    {
        public byte Class;         // EffectParameterClass
        public byte Type;          // EffectParameterType: 1 Bool, 2 Int32, 3 Single, 4 String, ...
        public string Name = string.Empty;
        public string Semantic = string.Empty;
        public List<Parameter> Annotations = new();
        public byte RowCount;
        public byte ColumnCount;
        public List<Parameter> Elements = new();
        public List<Parameter> StructMembers = new();

        /// <summary>Raw default-value bytes; present only for a leaf numeric parameter.</summary>
        public byte[] Data;
    }

    internal sealed class Pass
    {
        public string Name = string.Empty;
        public List<Parameter> Annotations = new();
        public int VertexShaderIndex;
        public int PixelShaderIndex;
        public byte[] BlendState;          // null when absent; raw bytes, preserved verbatim
        public byte[] DepthStencilState;
        public byte[] RasterizerState;
    }

    internal sealed class Technique
    {
        public string Name = string.Empty;
        public List<Parameter> Annotations = new();
        public List<Pass> Passes = new();
    }

    // ---------------------------------------------------------------------------------------

    public static MgfxDocument Read(byte[] blob)
    {
        using var ms = new MemoryStream(blob, writable: false);
        using var r = new BinaryReader(ms, Encoding.UTF8);
        var doc = new MgfxDocument();

        if (r.ReadInt32() != Signature)
            throw new InvalidDataException("Not an MGFX blob (bad header signature).");

        doc.Version = r.ReadByte();
        doc.Profile = r.ReadByte();
        doc.EffectKey = r.ReadInt32();

        if (doc.Version != VersionV10 && doc.Version != VersionV11)
            throw new InvalidDataException(
                $"MGFX v{doc.Version} is not supported; this tool reads v10 and v11.");

        int cbufferCount = r.ReadInt32();
        for (int i = 0; i < cbufferCount; i++)
        {
            var cb = new ConstantBuffer { Name = r.ReadString(), SizeInBytes = (ushort)r.ReadInt16() };
            int paramCount = r.ReadInt32();
            for (int j = 0; j < paramCount; j++)
            {
                cb.ParameterIndex.Add(r.ReadInt32());
                cb.ParameterOffset.Add(r.ReadUInt16());
            }
            doc.ConstantBuffers.Add(cb);
        }

        int shaderCount = r.ReadInt32();
        for (int i = 0; i < shaderCount; i++)
            doc.Shaders.Add(ReadShader(r, doc.Version));

        doc.Parameters.AddRange(ReadParameters(r));

        int techniqueCount = r.ReadInt32();
        for (int i = 0; i < techniqueCount; i++)
        {
            var t = new Technique { Name = r.ReadString() };
            t.Annotations.AddRange(ReadParameters(r));
            int passCount = r.ReadInt32();
            for (int p = 0; p < passCount; p++)
            {
                var pass = new Pass { Name = r.ReadString() };
                pass.Annotations.AddRange(ReadParameters(r));
                pass.VertexShaderIndex = r.ReadInt32();
                pass.PixelShaderIndex = r.ReadInt32();
                if (r.ReadBoolean()) pass.BlendState = r.ReadBytes(18);
                if (r.ReadBoolean()) pass.DepthStencilState = r.ReadBytes(25);
                if (r.ReadBoolean()) pass.RasterizerState = r.ReadBytes(12);
                t.Passes.Add(pass);
            }
            doc.Techniques.Add(t);
        }

        if (r.ReadInt32() != Signature)
            throw new InvalidDataException("Tail signature missing; MonoGame would reject this effect.");

        if (ms.Position != ms.Length)
            throw new InvalidDataException(
                $"Parsed {ms.Position} of {ms.Length} bytes - {ms.Length - ms.Position} trailing byte(s).");

        return doc;
    }

    private static Shader ReadShader(BinaryReader r, int version)
    {
        var s = new Shader { IsVertexShader = r.ReadBoolean() };

        if (version >= VersionV11)
        {
            s.SourceFile = r.ReadString();
            s.Entrypoint = r.ReadString();
        }

        int codeLength = r.ReadInt32();
        s.ShaderCode = r.ReadBytes(codeLength);
        if (s.ShaderCode.Length != codeLength)
            throw new InvalidDataException("Shader code length runs past the end of the blob.");

        int samplerCount = r.ReadByte();
        for (int i = 0; i < samplerCount; i++)
        {
            var sampler = new Sampler
            {
                Type = r.ReadByte(),
                TextureSlot = r.ReadByte(),
                SamplerSlot = r.ReadByte(),
            };
            if (r.ReadBoolean())
            {
                sampler.State = new SamplerState
                {
                    AddressU = r.ReadByte(),
                    AddressV = r.ReadByte(),
                    AddressW = r.ReadByte(),
                    BorderColor = r.ReadBytes(4),
                    Filter = r.ReadByte(),
                    MaxAnisotropy = r.ReadInt32(),
                    MaxMipLevel = r.ReadInt32(),
                    MipMapLevelOfDetailBias = r.ReadSingle(),
                };
            }
            sampler.Name = r.ReadString();
            sampler.ParameterIndex = r.ReadByte();
            s.Samplers.Add(sampler);
        }

        int cbCount = r.ReadByte();
        for (int i = 0; i < cbCount; i++) s.ConstantBuffers.Add(r.ReadByte());

        int attrCount = r.ReadByte();
        for (int i = 0; i < attrCount; i++)
        {
            s.Attributes.Add(new Attribute
            {
                Name = r.ReadString(),
                Usage = r.ReadByte(),
                Index = r.ReadByte(),
                Location = r.ReadInt16(),
            });
        }

        return s;
    }

    private static List<Parameter> ReadParameters(BinaryReader r)
    {
        int count = r.ReadInt32();
        var list = new List<Parameter>(count);
        for (int i = 0; i < count; i++)
        {
            var p = new Parameter
            {
                Class = r.ReadByte(),
                Type = r.ReadByte(),
                Name = r.ReadString(),
                Semantic = r.ReadString(),
            };
            p.Annotations.AddRange(ReadParameters(r));
            p.RowCount = r.ReadByte();
            p.ColumnCount = r.ReadByte();
            p.Elements.AddRange(ReadParameters(r));
            p.StructMembers.AddRange(ReadParameters(r));

            // Only a leaf numeric parameter stores default data, and the writer's condition is
            // `element_count == 0 && member_count == 0 && (type - 1) <= 2`, i.e. Bool, Int32 or
            // Single. Mirroring that exactly is what keeps the round-trip exact.
            if (p.Elements.Count == 0 && p.StructMembers.Count == 0 &&
                p.Type >= 1 && p.Type <= 3)
            {
                p.Data = r.ReadBytes(p.RowCount * p.ColumnCount * 4);
            }
            list.Add(p);
        }
        return list;
    }

    // ---------------------------------------------------------------------------------------

    /// <summary>
    /// Serializes the effect. <paramref name="version"/> may be 10 or 11; v10 omits each
    /// shader's sourceFile/entrypoint strings, which is the only structural difference.
    ///
    /// <paramref name="preserveEffectKey"/> writes the key back verbatim instead of recomputing
    /// it. Needed for the byte-exact round-trip test, since the key is a hash of the body and
    /// recomputing it over an unchanged body must produce the same value anyway - so a
    /// difference there is a real signal, not noise.
    /// </summary>
    public byte[] Write(int version, bool preserveEffectKey = false)
    {
        if (version != VersionV10 && version != VersionV11)
            throw new ArgumentOutOfRangeException(nameof(version), "Only MGFX v10 and v11 can be written.");

        byte[] body;
        using (var bodyStream = new MemoryStream())
        {
            using var bw = new BinaryWriter(bodyStream, Encoding.UTF8);

            bw.Write(ConstantBuffers.Count);
            foreach (var cb in ConstantBuffers)
            {
                bw.Write(cb.Name);
                bw.Write((short)cb.SizeInBytes);
                bw.Write(cb.ParameterIndex.Count);
                for (int i = 0; i < cb.ParameterIndex.Count; i++)
                {
                    bw.Write(cb.ParameterIndex[i]);
                    bw.Write(cb.ParameterOffset[i]);
                }
            }

            bw.Write(Shaders.Count);
            foreach (var s in Shaders) WriteShader(bw, s, version);

            WriteParameters(bw, Parameters);

            bw.Write(Techniques.Count);
            foreach (var t in Techniques)
            {
                bw.Write(t.Name);
                WriteParameters(bw, t.Annotations);
                bw.Write(t.Passes.Count);
                foreach (var p in t.Passes)
                {
                    bw.Write(p.Name);
                    WriteParameters(bw, p.Annotations);
                    bw.Write(p.VertexShaderIndex);
                    bw.Write(p.PixelShaderIndex);
                    WriteOptionalState(bw, p.BlendState);
                    WriteOptionalState(bw, p.DepthStencilState);
                    WriteOptionalState(bw, p.RasterizerState);
                }
            }

            bw.Flush();
            body = bodyStream.ToArray();
        }

        int key = preserveEffectKey ? EffectKey : ComputeHash(body);

        using var outStream = new MemoryStream();
        using var w = new BinaryWriter(outStream, Encoding.UTF8);
        w.Write(new[] { 'M', 'G', 'F', 'X' });
        w.Write((byte)version);
        w.Write(Profile);
        w.Write(key);
        w.Write(body);
        w.Write(new[] { 'M', 'G', 'F', 'X' });
        w.Flush();
        return outStream.ToArray();
    }

    private static void WriteOptionalState(BinaryWriter w, byte[] state)
    {
        if (state == null)
        {
            w.Write(false);
            return;
        }
        w.Write(true);
        w.Write(state);
    }

    private static void WriteShader(BinaryWriter w, Shader s, int version)
    {
        w.Write(s.IsVertexShader);

        if (version >= VersionV11)
        {
            w.Write(s.SourceFile ?? "<unknown>");
            w.Write(s.Entrypoint ?? "<unknown>");
        }

        w.Write(s.ShaderCode.Length);
        w.Write(s.ShaderCode);

        w.Write((byte)s.Samplers.Count);
        foreach (var sampler in s.Samplers)
        {
            w.Write(sampler.Type);
            w.Write(sampler.TextureSlot);
            w.Write(sampler.SamplerSlot);
            if (sampler.State != null)
            {
                w.Write(true);
                w.Write(sampler.State.AddressU);
                w.Write(sampler.State.AddressV);
                w.Write(sampler.State.AddressW);
                w.Write(sampler.State.BorderColor);
                w.Write(sampler.State.Filter);
                w.Write(sampler.State.MaxAnisotropy);
                w.Write(sampler.State.MaxMipLevel);
                w.Write(sampler.State.MipMapLevelOfDetailBias);
            }
            else
            {
                w.Write(false);
            }
            w.Write(sampler.Name ?? string.Empty);
            w.Write(sampler.ParameterIndex);
        }

        w.Write((byte)s.ConstantBuffers.Count);
        foreach (byte cb in s.ConstantBuffers) w.Write(cb);

        w.Write((byte)s.Attributes.Count);
        foreach (var a in s.Attributes)
        {
            w.Write(a.Name ?? string.Empty);
            w.Write(a.Usage);
            w.Write(a.Index);
            w.Write(a.Location);
        }
    }

    private static void WriteParameters(BinaryWriter w, List<Parameter> parameters)
    {
        w.Write(parameters.Count);
        foreach (var p in parameters)
        {
            w.Write(p.Class);
            w.Write(p.Type);
            w.Write(p.Name ?? string.Empty);
            w.Write(p.Semantic ?? string.Empty);
            WriteParameters(w, p.Annotations);
            w.Write(p.RowCount);
            w.Write(p.ColumnCount);
            WriteParameters(w, p.Elements);
            WriteParameters(w, p.StructMembers);
            if (p.Data != null) w.Write(p.Data);
        }
    }

    /// <summary>
    /// mgfxc's EffectObject.ComputeHash over the body bytes: FNV-1a (basis 0x811C9DC5, prime
    /// 16777619) followed by an avalanche. MonoGame treats the result purely as a cache key, but
    /// reproducing the same algorithm keeps a rewritten effect indistinguishable from a freshly
    /// compiled one - and the round-trip test compares it against the shipped value, so a
    /// mistake here shows up rather than hiding.
    ///
    /// Explicitly unchecked: the FNV multiply relies on int overflow, and this must not depend
    /// on the project's CheckForOverflowUnderflow setting.
    /// </summary>
    private static int ComputeHash(byte[] body)
    {
        unchecked
        {
            int hash = -2128831035;
            foreach (byte b in body)
                hash = (hash ^ b) * 16777619;
            hash += hash << 13;
            hash ^= hash >> 7;
            hash += hash << 3;
            hash ^= hash >> 17;
            return hash + (hash << 5);
        }
    }
}
