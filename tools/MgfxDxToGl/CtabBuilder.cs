using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UW.Tools.MgfxDxToGl;

/// <summary>
/// Synthesises a D3DX constant table (CTAB) and injects it into DX9 shader bytecode.
///
/// Why: MojoShader refuses relative constant addressing without a CTAB - *"relative addressing
/// unsupported without a CTAB"* - because it has no way to know how large the array being
/// indexed is, and therefore how large to declare `vs_uniforms_vec4[]`. mgfxc strips reflection,
/// so the shipped effects have no CTAB. RoundLine's vertex shader is the one that needs it: it
/// is an instanced line renderer reading `cb0[r1.x + 7]` into a 200-element `instanceData`
/// array, which the DX11 disassembly confirms (`dcl_constantbuffer cb0[207]`).
///
/// What is synthesised is deliberately NOT a faithful reconstruction. A DX9 CTAB describes each
/// constant by whole register index and count, with no way to say "this float lives in
/// component .y of register 4" - yet that is exactly what this bytecode does, because it
/// inherited fxc's DX11 cbuffer packing (RoundLine's `time` and `lineRadius` share register 4).
/// So a per-parameter table cannot be expressed.
///
/// It does not need to be. MojoShader uses the CTAB for two things: bounding relative
/// addressing, and populating its symbol list. The symbols are metadata this tool never reads -
/// constant buffers are built from the container's own offsets - so one entry describing the
/// whole register file as a single float4 array is sufficient and honest about what it is.
/// </summary>
internal static class CtabBuilder
{
    // D3DXPARAMETER_CLASS
    private const ushort ClassVector = 1;
    // D3DXPARAMETER_TYPE
    private const ushort TypeFloat = 3;
    // D3DXREGISTER_SET
    private const ushort RegisterSetFloat4 = 2;

    private const int HeaderSize = 28;
    private const int ConstantInfoSize = 20;
    private const int TypeInfoSize = 16;

    /// <summary>
    /// Returns <paramref name="dx9"/> with a CTAB comment inserted after its version token,
    /// declaring a single float4 array of <paramref name="registerCount"/> registers starting at
    /// c0.
    /// </summary>
    public static byte[] Inject(byte[] dx9, int registerCount, out string error)
    {
        error = null;

        if (dx9 == null || dx9.Length < 8)
        {
            error = "DX9 bytecode too short to carry a comment";
            return null;
        }
        if (registerCount <= 0 || registerCount > 8192)
        {
            error = $"implausible register count {registerCount}";
            return null;
        }

        uint versionToken = BitConverter.ToUInt32(dx9, 0);
        ushort high = (ushort)(versionToken >> 16);
        if (high != 0xFFFE && high != 0xFFFF)
        {
            error = "first token is not a DX9 version token";
            return null;
        }

        byte[] ctab = BuildTable(versionToken, registerCount);

        // A comment token's payload length is counted in dwords and must be exact, so the
        // payload is padded to a dword boundary. Payload = 'CTAB' fourcc + the table.
        int payloadBytes = 4 + ctab.Length;
        int padded = (payloadBytes + 3) & ~3;
        int payloadDwords = padded / 4;
        if (payloadDwords > 0x7FFF)
        {
            error = "constant table too large for a single comment token";
            return null;
        }

        using var ms = new MemoryStream();
        using var w = new BinaryWriter(ms);

        w.Write(versionToken);
        w.Write((uint)(0xFFFE | (payloadDwords << 16)));
        w.Write(Encoding.ASCII.GetBytes("CTAB"));
        w.Write(ctab);
        for (int i = payloadBytes; i < padded; i++) w.Write((byte)0);
        w.Write(dx9, 4, dx9.Length - 4);

        w.Flush();
        return ms.ToArray();
    }

    private static byte[] BuildTable(uint versionToken, int registerCount)
    {
        // Layout, with every offset relative to the start of this table (which is where
        // MojoShader's parser takes its base from):
        //
        //   0   CTHeader          28 bytes
        //   28  CTInfo[1]         20 bytes
        //   48  CTType            16 bytes
        //   64  strings
        int constantInfoOffset = HeaderSize;
        int typeInfoOffset = constantInfoOffset + ConstantInfoSize;
        int stringsOffset = typeInfoOffset + TypeInfoSize;

        byte[] creator = Encoding.ASCII.GetBytes("UW MgfxDxToGl synthesised CTAB\0");
        byte[] target = Encoding.ASCII.GetBytes(TargetName(versionToken) + "\0");
        byte[] name = Encoding.ASCII.GetBytes("c\0");

        int creatorOffset = stringsOffset;
        int targetOffset = creatorOffset + creator.Length;
        int nameOffset = targetOffset + target.Length;

        using var ms = new MemoryStream();
        using var w = new BinaryWriter(ms);

        // CTHeader
        w.Write((uint)HeaderSize);
        w.Write((uint)creatorOffset);
        w.Write(versionToken);
        w.Write((uint)1);                    // one constant
        w.Write((uint)constantInfoOffset);
        w.Write((uint)0);                    // flags
        w.Write((uint)targetOffset);

        // CTInfo
        w.Write((uint)nameOffset);
        w.Write(RegisterSetFloat4);
        w.Write((ushort)0);                  // register index
        w.Write((ushort)registerCount);
        w.Write((ushort)0);                  // reserved
        w.Write((uint)typeInfoOffset);
        w.Write((uint)0);                    // no default value

        // CTType: an array of registerCount float4s.
        w.Write(ClassVector);
        w.Write(TypeFloat);
        w.Write((ushort)1);                  // rows
        w.Write((ushort)4);                  // columns
        w.Write((ushort)registerCount);      // elements
        w.Write((ushort)0);                  // struct members
        w.Write((uint)0);                    // struct member info

        w.Write(creator);
        w.Write(target);
        w.Write(name);

        w.Flush();
        return ms.ToArray();
    }

    private static string TargetName(uint versionToken)
    {
        bool vertex = (versionToken >> 16) == 0xFFFE;
        int major = (int)((versionToken >> 8) & 0xFF);
        int minor = (int)(versionToken & 0xFF);
        return $"{(vertex ? "vs" : "ps")}_{major}_{minor}";
    }

    /// <summary>
    /// The number of float4 constant registers a shader's DX11 bytecode declares, from
    /// <c>dcl_constantbuffer cbN[count]</c>. This is the authority on how large the array must
    /// be: it is what fxc itself recorded, rather than anything inferred from the container.
    /// Returns 0 if there is no such declaration.
    /// </summary>
    public static int DeclaredConstantRegisters(byte[] dxbc)
    {
        var instructions = Sm4Disassembler.Read(dxbc, out _, out string error);
        if (instructions == null || error != null) return 0;

        int max = 0;
        foreach (var ins in instructions)
        {
            if (ins.Opcode != "dcl_constantbuffer") continue;
            // The operand is cbN[count]: index 0 is the buffer number, index 1 the size.
            var op = ins.Operands.Count > 0 ? ins.Operands[0] : null;
            if (op == null || op.Indices.Count < 2) continue;
            max = Math.Max(max, (int)op.Indices[1]);
        }
        return max;
    }
}
