using System;
using System.Collections.Generic;
using System.Text;

namespace UW.Tools.MgfxDxToGl;

/// <summary>
/// Minimal reader for the SM4/SM5 instruction stream in a DXBC container's SHDR/SHEX chunk.
///
/// This exists to answer a scoping question with facts rather than impressions: the three
/// effects with no Aon9 chunk (multiTex, skinFX, Vehicle - 38 shaders) can only reach GLSL by
/// translating their DX11 bytecode, and whether that is a weekend or a month depends entirely
/// on how large and how exotic the opcode set actually is. So this walks the stream and counts
/// opcodes; it does not decode operands or translate anything.
///
/// Format, from the DXBC/SM4 encoding: the chunk opens with a version token and a
/// length-in-dwords, then a flat instruction stream. Each instruction's first dword carries the
/// opcode in bits 0-10 and its length in dwords in bits 24-30. Bit 31 marks an extended opcode
/// token, which chains. Declarations (DCL_*) live in the same stream as ordinary instructions.
/// </summary>
internal static class Sm4Bytecode
{
    /// <summary>
    /// D3D10/11 opcode names, indexed by opcode number. Only the range these shaders can
    /// actually contain is filled in; anything past it prints as its raw number, which is
    /// exactly the signal wanted - an unnamed opcode is one worth looking at.
    /// </summary>
    private static readonly string[] OpcodeNames =
    {
        "ADD", "AND", "BREAK", "BREAKC", "CALL", "CALLC", "CASE", "CONTINUE", "CONTINUEC",
        "CUT", "DEFAULT", "DERIV_RTX", "DERIV_RTY", "DISCARD", "DIV", "DP2", "DP3", "DP4",
        "ELSE", "EMIT", "EMITTHENCUT", "ENDIF", "ENDLOOP", "ENDSWITCH", "EQ", "EXP", "FRC",
        "FTOI", "FTOU", "GE", "IADD", "IF", "IEQ", "IGE", "ILT", "IMAD", "IMAX", "IMIN",
        "IMUL", "INE", "INEG", "ISHL", "ISHR", "ITOF", "LABEL", "LD", "LD_MS", "LOG", "LOOP",
        "LT", "MAD", "MIN", "MAX", "CUSTOMDATA", "MOV", "MOVC", "MUL", "NE", "NOP", "NOT",
        "OR", "RESINFO", "RET", "RETC", "ROUND_NE", "ROUND_NI", "ROUND_PI", "ROUND_Z", "RSQ",
        "SAMPLE", "SAMPLE_C", "SAMPLE_C_LZ", "SAMPLE_L", "SAMPLE_D", "SAMPLE_B", "SQRT",
        "SWITCH", "SINCOS", "UDIV", "ULT", "UGE", "UMUL", "UMAD", "UMAX", "UMIN", "USHR",
        "UTOF", "XOR", "DCL_RESOURCE", "DCL_CONSTANT_BUFFER", "DCL_SAMPLER",
        "DCL_INDEX_RANGE", "DCL_GS_OUTPUT_PRIMITIVE_TOPOLOGY", "DCL_GS_INPUT_PRIMITIVE",
        "DCL_MAX_OUTPUT_VERTEX_COUNT", "DCL_INPUT", "DCL_INPUT_SGV", "DCL_INPUT_SIV",
        "DCL_INPUT_PS", "DCL_INPUT_PS_SGV", "DCL_INPUT_PS_SIV", "DCL_OUTPUT",
        "DCL_OUTPUT_SGV", "DCL_OUTPUT_SIV", "DCL_TEMPS", "DCL_INDEXABLE_TEMP",
        "DCL_GLOBAL_FLAGS",
    };

    internal sealed class Info
    {
        public string Model = "?";
        public int InstructionCount;
        public int TempCount;
        public bool HasDynamicFlowControl;
        public bool HasIndexing;
        public Dictionary<string, int> Opcodes = new(StringComparer.Ordinal);
        public string Error;
    }

    public static Info Read(byte[] dxbc)
    {
        var info = new Info();

        if (dxbc.Length < 32 || Encoding.ASCII.GetString(dxbc, 0, 4) != "DXBC")
        {
            info.Error = "not a DXBC container";
            return info;
        }

        int chunkCount = BitConverter.ToInt32(dxbc, 28);
        for (int i = 0; i < chunkCount; i++)
        {
            int offsetPos = 32 + i * 4;
            if (offsetPos + 4 > dxbc.Length) break;
            int chunkOffset = BitConverter.ToInt32(dxbc, offsetPos);
            if (chunkOffset < 0 || chunkOffset + 8 > dxbc.Length) continue;

            string fourcc = Encoding.ASCII.GetString(dxbc, chunkOffset, 4);
            if (fourcc != "SHDR" && fourcc != "SHEX") continue;

            int chunkSize = BitConverter.ToInt32(dxbc, chunkOffset + 4);
            int start = chunkOffset + 8;
            int end = Math.Min(start + chunkSize, dxbc.Length);
            if (start + 8 > end) { info.Error = "SHDR chunk too short"; return info; }

            uint version = BitConverter.ToUInt32(dxbc, start);
            int minor = (int)(version & 0xF);
            int major = (int)((version >> 4) & 0xF);
            uint programType = version >> 16;
            string stage = programType switch
            {
                0 => "ps", 1 => "vs", 2 => "gs", 3 => "hs", 4 => "ds", 5 => "cs", _ => "?",
            };
            info.Model = $"{stage}_{major}_{minor}";

            // dword[1] is the total chunk length in dwords, including these two tokens.
            int lengthInDwords = (int)BitConverter.ToUInt32(dxbc, start + 4);
            int streamEnd = Math.Min(start + lengthInDwords * 4, end);

            int pos = start + 8;
            while (pos + 4 <= streamEnd)
            {
                uint token = BitConverter.ToUInt32(dxbc, pos);
                int opcode = (int)(token & 0x7FF);
                int lengthDwords = (int)((token >> 24) & 0x7F);

                // CUSTOMDATA carries its own dword length in the following token and does not
                // use the length field, so treating it like a normal instruction would desync
                // the whole walk.
                if (opcode == 53)
                {
                    if (pos + 8 > streamEnd) break;
                    int customLength = (int)BitConverter.ToUInt32(dxbc, pos + 4);
                    if (customLength <= 0) break;
                    pos += customLength * 4;
                    Bump(info, "CUSTOMDATA");
                    continue;
                }

                if (lengthDwords <= 0) { info.Error = $"zero-length instruction at byte {pos}"; break; }

                string name = opcode >= 0 && opcode < OpcodeNames.Length
                    ? OpcodeNames[opcode]
                    : $"OPCODE_{opcode}";
                Bump(info, name);

                if (!name.StartsWith("DCL_", StringComparison.Ordinal)) info.InstructionCount++;

                switch (name)
                {
                    case "DCL_TEMPS":
                        if (pos + 8 <= streamEnd) info.TempCount = (int)BitConverter.ToUInt32(dxbc, pos + 4);
                        break;
                    case "IF":
                    case "BREAKC":
                    case "CONTINUEC":
                    case "SWITCH":
                    case "LOOP":
                        info.HasDynamicFlowControl = true;
                        break;
                    case "DCL_INDEX_RANGE":
                    case "DCL_INDEXABLE_TEMP":
                        info.HasIndexing = true;
                        break;
                }

                pos += lengthDwords * 4;
            }
            return info;
        }

        info.Error = "no SHDR/SHEX chunk";
        return info;
    }

    private static void Bump(Info info, string name)
    {
        info.Opcodes.TryGetValue(name, out int n);
        info.Opcodes[name] = n + 1;
    }
}
