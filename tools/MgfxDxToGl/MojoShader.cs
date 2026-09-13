using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace UW.Tools.MgfxDxToGl;

/// <summary>
/// Interop with mojoshader.dll, the DX9-bytecode-to-GLSL translator.
///
/// Struct layouts and enum values are taken from the decompiled mgfxc 3.8.5.1 MojoShader.cs,
/// which is generated against the same native library that ships in the dotnet-mgfxc tool
/// package - so this is MonoGame's own binding, not an independent guess at the ABI. The one
/// deliberate difference is that only the parts this tool reads are declared.
///
/// Getting a field type wrong here would silently shift every following field, so the pieces
/// most likely to bite are called out: <c>constant</c> in MOJOSHADER_uniform is an int and not
/// a bool, and every enum here is a C int with MOJOSHADER_*_UNKNOWN = -1 rather than 0.
/// </summary>
internal static class MojoShader
{
    public enum ShaderType { Unknown = 0, Pixel = 1, Vertex = 2, Geometry = 4 }

    public enum UniformType { Unknown = -1, Float = 0, Int = 1, Bool = 2 }

    public enum SamplerType { Unknown = -1, Sampler2D = 0, SamplerCube = 1, SamplerVolume = 2, Sampler1D = 3 }

    /// <summary>
    /// MOJOSHADER_usage. This is NOT XNA's VertexElementUsage ordering - the two agree only on
    /// Position and Normal, and TexCoord is 5 here against XNA's 2. Convert with
    /// DxbcSignature.XnaUsageFromMojoShader before putting a value anywhere MonoGame reads it.
    /// </summary>
    public enum Usage
    {
        Unknown = -1, Position = 0, BlendWeight, BlendIndices, Normal, PointSize,
        TexCoord, Tangent, Binormal, TessFactor, PositionT, Color, Fog, Depth, Sample
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Error
    {
        [MarshalAs(UnmanagedType.LPStr)] public string ErrorText;
        [MarshalAs(UnmanagedType.LPStr)] public string Filename;
        public int ErrorPosition;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Attribute
    {
        public Usage Usage;
        public int Index;
        [MarshalAs(UnmanagedType.LPStr)] public string Name;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Sampler
    {
        public SamplerType Type;
        public int Index;
        [MarshalAs(UnmanagedType.LPStr)] public string Name;
        public int TexBem;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Uniform
    {
        public UniformType Type;
        public int Index;
        public int ArrayCount;
        public int Constant;      // int, NOT bool - see the class remarks
        [MarshalAs(UnmanagedType.LPStr)] public string Name;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ParseData
    {
        public int ErrorCount;
        public IntPtr Errors;
        [MarshalAs(UnmanagedType.LPStr)] public string Profile;
        [MarshalAs(UnmanagedType.LPStr)] public string Output;
        public int OutputLength;
        public int InstructionCount;
        public ShaderType ShaderType;
        public int MajorVersion;
        public int MinorVersion;
        public int UniformCount;
        public IntPtr Uniforms;
        public int ConstantCount;
        public IntPtr Constants;
        public int SamplerCount;
        public IntPtr Samplers;
        public int AttributeCount;
        public IntPtr Attributes;
        public int OutputCount;
        public IntPtr Outputs;
        public int SwizzleCount;
        public IntPtr Swizzles;
        public int SymbolCount;
        public IntPtr Symbols;
        public IntPtr Preshader;
        public IntPtr Malloc;
        public IntPtr Free;
        public IntPtr MallocData;
    }

    [DllImport("mojoshader", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr MOJOSHADER_parse(
        [In, MarshalAs(UnmanagedType.LPStr)] string profile,
        [In] byte[] tokenbuf, int bufsize,
        IntPtr swiz, int swizcount,
        IntPtr smap, int smapcount,
        IntPtr m, IntPtr f, IntPtr d);

    // No MOJOSHADER_freeParseData here on purpose. MonoGame's binding declares it, but the
    // mojoshader.dll that ships in the dotnet-mgfxc package does not export it - the only
    // MOJOSHADER_* entry points in the binary are MOJOSHADER_parse and
    // MOJOSHADER_parseExpression - so importing it fails at the call with
    // "Unable to find an entry point named 'MOJOSHADER_freeParseData'".
    //
    // That means each parse leaks its result. mgfxc has the same behaviour and gets away with
    // it because it is a short-lived process, and so is this: converting all 19 effects parses
    // ~50 shaders totalling well under a megabyte of output. Worth knowing rather than
    // discovering, but not worth working around.

    internal sealed class Result
    {
        public string Glsl;
        public ShaderType ShaderType;
        public int MajorVersion;
        public int MinorVersion;
        public int InstructionCount;
        public List<Attribute> Attributes = new();
        public List<Sampler> Samplers = new();
        public List<Uniform> Uniforms = new();
    }

    /// <summary>
    /// Translates DX9 shader bytecode to GLSL. Returns null and sets <paramref name="error"/>
    /// when MojoShader refuses the shader - which it does, for instance, on relative constant
    /// addressing in bytecode whose CTAB has been stripped.
    /// </summary>
    public static Result Translate(byte[] dx9Bytecode, out string error)
    {
        error = null;
        IntPtr p = MOJOSHADER_parse("glsl", dx9Bytecode, dx9Bytecode.Length,
            IntPtr.Zero, 0, IntPtr.Zero, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        if (p == IntPtr.Zero)
        {
            error = "MOJOSHADER_parse returned null";
            return null;
        }

        try
        {
            var pd = Marshal.PtrToStructure<ParseData>(p);
            if (pd.ErrorCount > 0)
            {
                var errors = ReadArray<Error>(pd.Errors, pd.ErrorCount);
                error = errors.Count > 0 ? errors[0].ErrorText : "unspecified MojoShader error";
                return null;
            }

            return new Result
            {
                Glsl = pd.Output ?? string.Empty,
                ShaderType = pd.ShaderType,
                MajorVersion = pd.MajorVersion,
                MinorVersion = pd.MinorVersion,
                InstructionCount = pd.InstructionCount,
                Attributes = ReadArray<Attribute>(pd.Attributes, pd.AttributeCount),
                Samplers = ReadArray<Sampler>(pd.Samplers, pd.SamplerCount),
                Uniforms = ReadArray<Uniform>(pd.Uniforms, pd.UniformCount),
            };
        }
        catch (Exception ex)
        {
            error = ex.GetType().Name + ": " + ex.Message;
            return null;
        }
    }

    private static List<T> ReadArray<T>(IntPtr ptr, int count)
    {
        var list = new List<T>(Math.Max(count, 0));
        if (ptr == IntPtr.Zero || count <= 0) return list;
        int stride = Marshal.SizeOf<T>();
        for (int i = 0; i < count; i++)
            list.Add(Marshal.PtrToStructure<T>(ptr + i * stride));
        return list;
    }
}
