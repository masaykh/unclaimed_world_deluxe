using System;
using System.Collections.Generic;
using System.Text;

namespace UW.Tools.MgfxDxToGl;

/// <summary>
/// Parses a DXBC input/output signature chunk (ISGN / OSGN / OSG5 / PCSG).
///
/// This is what says which vertex semantic each register carries, and it is needed by both
/// routes for the three effects with no Aon9 chunk. MonoGame's OpenGL backend binds attributes
/// by NAME and then resolves a vertex element through usage+index:
///
///     Attributes[i].location = GL.GetAttribLocation(program, Attributes[i].name);
///     ...
///     int attribLocation = shader.GetAttribLocation(element.VertexElementUsage, element.UsageIndex);
///
/// so a translated shader has to declare attributes whose usage and index match the vertex
/// declaration the game actually draws with - and that mapping lives here, not in the
/// instruction stream. It is equally what someone writing the HLSL by hand needs to know.
///
/// Chunk layout:
///   uint32 elementCount
///   uint32 offsetToFirstElement   (8 for the plain forms; ignored, elements follow the header)
///   per element, 24 bytes:
///     uint32 nameOffset           (relative to the chunk's data start)
///     uint32 semanticIndex
///     uint32 systemValueType
///     uint32 componentType        (1 = uint, 2 = int, 3 = float)
///     uint32 register
///     byte   mask                 (which components exist)
///     byte   readWriteMask        (which are actually used)
///     uint16 padding
/// </summary>
internal static class DxbcSignature
{
    internal sealed class Element
    {
        public string SemanticName = "";
        public int SemanticIndex;
        public int SystemValueType;
        public int ComponentType;
        public int Register;
        public byte Mask;
        public byte ReadWriteMask;

        /// <summary>XNA VertexElementUsage for this semantic, or -1 if it has no equivalent.</summary>
        public int Usage => UsageFor(SemanticName);

        public override string ToString() =>
            $"{SemanticName}{SemanticIndex} reg={Register} mask=0x{Mask:X} rw=0x{ReadWriteMask:X}" +
            (SystemValueType != 0 ? $" sv={SystemValueType}" : "") +
            $" ctype={ComponentTypeName(ComponentType)}" +
            (Usage >= 0 ? $" usage={VertexElementUsageName(Usage)}" : " usage=(none)");
    }

    /// <summary>XNA's VertexElementUsage, which is what an MGFX attribute's usage byte holds.</summary>
    public enum XnaUsage
    {
        Position = 0, Color = 1, TextureCoordinate = 2, Normal = 3, Binormal = 4, Tangent = 5,
        BlendIndices = 6, BlendWeight = 7, Depth = 8, Fog = 9, PointSize = 10, Sample = 11,
        TessellateFactor = 12,
    }

    /// <summary>
    /// Semantic name to XNA VertexElementUsage.
    ///
    /// These are NOT MojoShader's MOJOSHADER_usage numbers, and an earlier version of this file
    /// claimed they were. The two enumerations agree only on Position and Normal: MojoShader
    /// counts POSITION, BLENDWEIGHT, BLENDINDICES, NORMAL, POINTSIZE, TEXCOORD... while XNA
    /// counts Position, Color, TextureCoordinate, Normal, Binormal, Tangent... so TEXCOORD is 5
    /// in one and 2 in the other. mgfxc converts between them explicitly
    /// (EffectObject.ToXNAVertexElementUsage) and the runtime compares against XNA's, in
    /// Shader.GetAttribLocation(VertexElementUsage, index).
    ///
    /// Getting this wrong is silent and total: VertexDeclaration looks each attribute up by
    /// usage and index, a miss returns -1, and OpenGL then feeds the shader the default
    /// (0, 0, 0, 1) for that attribute rather than failing. Every texture coordinate in the
    /// shipped set was being announced as a Tangent, so no converted shader received any
    /// interpolator at all - which is why GUI/LCD composited a single constant colour over the
    /// whole panel and DevShape's colour arrived as a constant.
    /// </summary>
    public static int UsageFor(string semanticName)
    {
        switch ((semanticName ?? "").ToUpperInvariant())
        {
            case "POSITION":
            case "SV_POSITION": return (int)XnaUsage.Position;
            case "BLENDWEIGHT": return (int)XnaUsage.BlendWeight;
            case "BLENDINDICES": return (int)XnaUsage.BlendIndices;
            case "NORMAL": return (int)XnaUsage.Normal;
            case "PSIZE": return (int)XnaUsage.PointSize;
            case "TEXCOORD": return (int)XnaUsage.TextureCoordinate;
            case "TANGENT": return (int)XnaUsage.Tangent;
            case "BINORMAL": return (int)XnaUsage.Binormal;
            case "TESSFACTOR": return (int)XnaUsage.TessellateFactor;
            case "COLOR": return (int)XnaUsage.Color;
            case "FOG": return (int)XnaUsage.Fog;
            case "DEPTH": return (int)XnaUsage.Depth;
            case "SAMPLE": return (int)XnaUsage.Sample;
            // POSITIONT has no XNA VertexElementUsage - mgfxc throws on it rather than mapping it,
            // so it is reported as unmappable instead of being given a number that means
            // something else.
            default: return -1;
        }
    }

    /// <summary>
    /// MojoShader's usage to XNA's, reproducing EffectObject.ToXNAVertexElementUsage. Used only
    /// to compare MojoShader's view of an attribute against the input signature's, which is a
    /// comparison between two different enumerations and meaningless without this.
    /// </summary>
    public static int XnaUsageFromMojoShader(int mojoShaderUsage) => mojoShaderUsage switch
    {
        0 => (int)XnaUsage.Position,
        1 => (int)XnaUsage.BlendWeight,
        2 => (int)XnaUsage.BlendIndices,
        3 => (int)XnaUsage.Normal,
        4 => (int)XnaUsage.PointSize,
        5 => (int)XnaUsage.TextureCoordinate,
        6 => (int)XnaUsage.Tangent,
        7 => (int)XnaUsage.Binormal,
        8 => (int)XnaUsage.TessellateFactor,
        10 => (int)XnaUsage.Color,
        11 => (int)XnaUsage.Fog,
        12 => (int)XnaUsage.Depth,
        13 => (int)XnaUsage.Sample,
        _ => -1,                      // 9 is POSITIONT, which XNA has no usage for
    };

    /// <summary>
    /// The register's component type. It decides what vertex-element FORMAT is legal: a shader
    /// reading BLENDINDICES as float needs a float layout, and binding a UINT format (MonoGame's
    /// Byte4) against it fails the DirectX input-layout validation with E_INVALIDARG.
    /// </summary>
    public static string ComponentTypeName(int componentType) => componentType switch
    {
        1 => "uint", 2 => "int", 3 => "float", _ => componentType.ToString(),
    };

    public static string VertexElementUsageName(int usage) =>
        System.Enum.IsDefined(typeof(XnaUsage), usage) ? ((XnaUsage)usage).ToString() : "?";

    /// <summary>Reads one signature chunk by fourcc. Returns an empty list if absent.</summary>
    public static List<Element> Read(byte[] dxbc, string fourccWanted)
    {
        var result = new List<Element>();
        if (dxbc.Length < 32 || Encoding.ASCII.GetString(dxbc, 0, 4) != "DXBC") return result;

        int chunkCount = BitConverter.ToInt32(dxbc, 28);
        for (int c = 0; c < chunkCount; c++)
        {
            int offsetPos = 32 + c * 4;
            if (offsetPos + 4 > dxbc.Length) break;
            int chunkOffset = BitConverter.ToInt32(dxbc, offsetPos);
            if (chunkOffset < 0 || chunkOffset + 8 > dxbc.Length) continue;

            string fourcc = Encoding.ASCII.GetString(dxbc, chunkOffset, 4);
            if (fourcc != fourccWanted) continue;

            int chunkSize = BitConverter.ToInt32(dxbc, chunkOffset + 4);
            int dataStart = chunkOffset + 8;
            int dataEnd = Math.Min(dataStart + chunkSize, dxbc.Length);
            if (dataStart + 8 > dataEnd) return result;

            int elementCount = (int)BitConverter.ToUInt32(dxbc, dataStart);
            int pos = dataStart + 8;

            for (int i = 0; i < elementCount && pos + 24 <= dataEnd; i++, pos += 24)
            {
                int nameOffset = (int)BitConverter.ToUInt32(dxbc, pos);
                var e = new Element
                {
                    SemanticName = ReadCString(dxbc, dataStart + nameOffset, dataEnd),
                    SemanticIndex = (int)BitConverter.ToUInt32(dxbc, pos + 4),
                    SystemValueType = (int)BitConverter.ToUInt32(dxbc, pos + 8),
                    ComponentType = (int)BitConverter.ToUInt32(dxbc, pos + 12),
                    Register = (int)BitConverter.ToUInt32(dxbc, pos + 16),
                    Mask = dxbc[pos + 20],
                    ReadWriteMask = dxbc[pos + 21],
                };
                result.Add(e);
            }
            return result;
        }
        return result;
    }

    private static string ReadCString(byte[] buf, int at, int limit)
    {
        if (at < 0 || at >= limit) return "";
        int end = at;
        while (end < limit && buf[end] != 0) end++;
        return Encoding.ASCII.GetString(buf, at, end - at);
    }
}
