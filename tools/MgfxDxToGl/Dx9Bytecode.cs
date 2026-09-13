using System;
using System.Collections.Generic;
using System.Text;

namespace UW.Tools.MgfxDxToGl;

/// <summary>
/// Pulls the DX9 (SM1-3) token stream out of a DXBC container's <c>Aon9</c> chunk.
///
/// fxc emits that chunk when a shader is compiled at a <c>level_9_x</c> profile: alongside the
/// DX11 bytecode it stores a genuine DX9 shader for the D3D9 feature-level path. 16 of the
/// game's 19 effects were built that way, which is the whole reason a bytecode-level conversion
/// to GLSL is possible at all - MojoShader consumes exactly this.
/// </summary>
internal static class Dx9Bytecode
{
    /// <summary>Returns the DX9 token stream, or null with a reason in <paramref name="why"/>.</summary>
    public static byte[] Extract(byte[] dxbc, out string why)
    {
        why = null;

        if (dxbc.Length < 32 || Encoding.ASCII.GetString(dxbc, 0, 4) != "DXBC")
        {
            why = "not a DXBC container";
            return null;
        }

        int chunkCount = BitConverter.ToInt32(dxbc, 28);
        for (int i = 0; i < chunkCount; i++)
        {
            int offsetPos = 32 + i * 4;
            if (offsetPos + 4 > dxbc.Length) break;

            int chunkOffset = BitConverter.ToInt32(dxbc, offsetPos);
            if (chunkOffset < 0 || chunkOffset + 8 > dxbc.Length) continue;
            if (Encoding.ASCII.GetString(dxbc, chunkOffset, 4) != "Aon9") continue;

            int size = BitConverter.ToInt32(dxbc, chunkOffset + 4);
            int start = chunkOffset + 8;
            int end = start + size;
            if (size < 0 || end > dxbc.Length)
            {
                why = "Aon9 chunk size runs past the end of the container";
                return null;
            }

            // The chunk opens with a small header whose layout is not publicly documented, so
            // rather than assuming an offset this scans for the DX9 version token - 0xFFFE in
            // the high word for a vertex shader, 0xFFFF for a pixel shader - and then walks the
            // token stream to its END token.
            //
            // Walking to the end is what makes this self-checking: on a coincidental byte
            // pattern the walk runs off the chunk or hits a malformed token instead of landing
            // exactly on END, and the scan simply continues past it.
            for (int pos = start; pos + 4 <= end; pos += 4)
            {
                ushort high = (ushort)(BitConverter.ToUInt32(dxbc, pos) >> 16);
                if (high != 0xFFFE && high != 0xFFFF) continue;

                int codeEnd = WalkToEnd(dxbc, pos, end);
                if (codeEnd < 0) continue;

                var code = new byte[codeEnd - pos];
                Array.Copy(dxbc, pos, code, 0, code.Length);
                return code;
            }

            why = "no well-formed DX9 token stream inside the Aon9 chunk";
            return null;
        }

        why = "no Aon9 chunk (the shader was not compiled at a level_9_x profile)";
        return null;
    }

    /// <summary>
    /// Walks a DX9 token stream from its version token to just past END, returning the exclusive
    /// end offset, or -1 if it is not well formed within <paramref name="limit"/>.
    /// </summary>
    private static int WalkToEnd(byte[] buf, int pos, int limit)
    {
        pos += 4;                      // version token
        while (pos + 4 <= limit)
        {
            uint token = BitConverter.ToUInt32(buf, pos);
            uint opcode = token & 0xFFFF;

            if (opcode == 0xFFFF) return pos + 4;      // END

            if (opcode == 0xFFFE)                      // COMMENT: length counts tokens
            {
                int commentTokens = (int)((token & 0x7FFF0000) >> 16);
                pos += 4 + commentTokens * 4;
                continue;
            }

            // Bits 24-27 hold the instruction length in tokens, excluding the opcode token.
            // SM1.x leaves it zero and there is then no reliable way to skip - but none of these
            // shaders are SM1.x, so treat zero as malformed rather than guessing.
            int length = (int)((token >> 24) & 0xF);
            if (length == 0) return -1;
            pos += 4 + length * 4;
        }
        return -1;
    }

    /// <summary>
    /// The texture coordinate each sampler is read with, keyed by DX9 sampler register.
    ///
    /// This exists to pin down which DX9 sampler register corresponds to which DirectX 11 one.
    /// fxc compiles the same HLSL to both, but it allocates the two register spaces
    /// independently and does not even emit the texture fetches in the same order, so neither
    /// the register number nor the position in the shader carries over. What does carry over is
    /// the coordinate: the fetch of the reflection map reads the same interpolator either way.
    /// Pairing the fetches up on that yields the correspondence as evidence rather than as an
    /// assumption - see GlConverter.BuildSamplerMap.
    ///
    /// A coordinate is reported as "t{register}.{components}" with the components sorted, so
    /// that the two compilations' differing swizzles compare equal. fxc is free to pack an
    /// interpolator as (z,w) for one profile and (w,z) for the other as long as the shader
    /// unpacks it to match, and it does exactly that.
    ///
    /// Only the two forms that actually occur are resolved: a fetch straight off an
    /// interpolator, and one off a temp that a <c>mov</c> filled from an interpolator. A
    /// coordinate the shader computed arithmetically has no interpolator to name and is left
    /// out, which the caller reports rather than guessing at.
    /// </summary>
    public static Dictionary<int, string> SamplerCoordinates(byte[] dx9, out string error)
    {
        error = null;
        var result = new Dictionary<int, string>();

        // Which interpolator component each temp component was last copied from, as a packed
        // (register, component) pair; -1 for anything not traceable to one.
        var provenance = new Dictionary<int, int[]>();

        int pos = 4;                                   // past the version token
        while (pos + 4 <= dx9.Length)
        {
            uint token = BitConverter.ToUInt32(dx9, pos);
            uint opcode = token & 0xFFFF;

            if (opcode == 0xFFFF) break;               // END

            if (opcode == 0xFFFE)                      // COMMENT
            {
                pos += 4 + (int)((token & 0x7FFF0000) >> 16) * 4;
                continue;
            }

            int length = (int)((token >> 24) & 0xF);
            if (length == 0)
            {
                error = $"instruction at offset {pos} declares no length";
                return null;
            }

            int paramsAt = pos + 4;
            if (paramsAt + length * 4 > dx9.Length)
            {
                error = $"instruction at offset {pos} runs past the end of the token stream";
                return null;
            }

            var operands = new uint[length];
            for (int i = 0; i < length; i++)
                operands[i] = BitConverter.ToUInt32(dx9, paramsAt + i * 4);

            if (opcode == OpcodeMov && length >= 2)
            {
                RecordCopy(provenance, operands[0], operands[1]);
            }
            else if (opcode == OpcodeTex && length >= 3 &&
                     RegisterType(operands[2]) == RegTypeSampler)
            {
                // texld's operands are destination, coordinate, sampler.
                int sampler = RegisterNumber(operands[2]);
                string coord = Coordinate(provenance, operands[1]);
                if (coord != null)
                {
                    if (result.TryGetValue(sampler, out string already) && already != coord)
                    {
                        error = $"sampler s{sampler} is read with two different coordinates " +
                                $"({already} and {coord}), so a coordinate cannot identify it";
                        return null;
                    }
                    result[sampler] = coord;
                }
            }

            pos = paramsAt + length * 4;
        }

        return result;
    }

    private const uint OpcodeMov = 1;
    private const uint OpcodeTex = 66;
    private const int RegTypeTemp = 0;
    private const int RegTypeInput = 1;
    private const int RegTypeTexture = 3;              // t# in a pixel shader
    private const int RegTypeSampler = 10;

    /// <summary>The register type, whose bits fxc splits across two fields of the token.</summary>
    private static int RegisterType(uint token) =>
        (int)(((token & 0x70000000) >> 28) | ((token & 0x00001800) >> 8));

    private static int RegisterNumber(uint token) => (int)(token & 0x7FF);

    private static int SwizzleComponent(uint token, int i) => (int)((token >> (16 + i * 2)) & 3);

    private static int WriteMask(uint token) => (int)((token >> 16) & 0xF);

    /// <summary>Notes a mov from an interpolator into a temp, per written component.</summary>
    private static void RecordCopy(Dictionary<int, int[]> provenance, uint dest, uint src)
    {
        if (RegisterType(dest) != RegTypeTemp) return;

        int destReg = RegisterNumber(dest);
        if (!provenance.TryGetValue(destReg, out int[] components))
            provenance[destReg] = components = new[] { -1, -1, -1, -1 };

        int srcType = RegisterType(src);
        bool fromInterpolator = srcType == RegTypeTexture || srcType == RegTypeInput;
        int srcReg = RegisterNumber(src);
        int mask = WriteMask(dest);

        // A write from anything else invalidates what was there: the component no longer holds
        // an interpolator, and treating a stale note as current would mispair a fetch.
        for (int i = 0; i < 4; i++)
        {
            if ((mask & (1 << i)) == 0) continue;
            components[i] = fromInterpolator ? (srcReg << 2) | SwizzleComponent(src, i) : -1;
        }
    }

    /// <summary>
    /// The interpolator and components a texld's coordinate operand names, or null when it
    /// cannot be traced back to one.
    /// </summary>
    private static string Coordinate(Dictionary<int, int[]> provenance, uint operand)
    {
        int type = RegisterType(operand);
        int register = RegisterNumber(operand);

        // A 2D fetch uses the first two components of the coordinate.
        var used = new int[2];

        if (type == RegTypeTexture || type == RegTypeInput)
        {
            for (int i = 0; i < 2; i++) used[i] = (register << 2) | SwizzleComponent(operand, i);
        }
        else if (type == RegTypeTemp)
        {
            if (!provenance.TryGetValue(register, out int[] components)) return null;
            for (int i = 0; i < 2; i++)
            {
                int from = components[SwizzleComponent(operand, i)];
                if (from < 0) return null;
                used[i] = from;
            }
        }
        else return null;

        if ((used[0] >> 2) != (used[1] >> 2)) return null;   // components of two interpolators
        return FormatCoordinate(used[0] >> 2, used[0] & 3, used[1] & 3);
    }

    /// <summary>
    /// "t1.yz" - the components sorted, so that a (z,w) and a (w,z) packing compare equal.
    /// </summary>
    public static string FormatCoordinate(int register, int first, int second)
    {
        const string names = "xyzw";
        int lo = Math.Min(first, second), hi = Math.Max(first, second);
        return "t" + register + "." + names[lo] + names[hi];
    }
}
