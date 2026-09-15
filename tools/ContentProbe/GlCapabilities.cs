#if UW_GL
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace UW.Tools.ContentProbe;

/// <summary>
/// What the OpenGL driver under this process actually is, and the two limits a shader can
/// exceed.
///
/// WHY THIS EXISTS. An effect that loads here and fails on someone else's machine is a driver
/// difference, and MonoGame's exception for it says "Failed to compile vertex shader
/// &lt;unknown&gt;. See &lt;unknown&gt;." - the GLSL info log, which is the only thing that would
/// say WHY, is discarded before it reaches the caller. Reported from an Intel HD Graphics
/// machine on a 2016 driver, where every animated model took the game down and that message was
/// the whole of the evidence.
///
/// So: print what the driver is and what it allows, next to the pass/fail list. Two competing
/// explanations for that report, and one run of this separates them:
///
///   UNIFORM SPACE   skinFX's vertex shader declares vec4[262] = 1048 float components. Intel
///                   Gen6 typically gives 1024. If MAX_VERTEX_UNIFORM_COMPONENTS is below 1048,
///                   that is the cause and no lowering pass will help.
///   GLSL DIALECT    ShadowDusk warns (SD0403) that the emitted GL for that same shader contains
///                   an unsigned integer type and a non-square matrix - GLSL 1.30+ constructs
///                   that strict front ends reject. If the limits are ample, that is the cause.
///
/// P/Invoke rather than MonoGame's bindings because MonoGame.OpenGL.GL is internal. glGetString
/// and glGetIntegerv are both OpenGL 1.1 core, so they are exported directly by the platform's
/// GL library and need no extension loader - which is the whole reason these two calls are
/// usable this way and a GL 2.0+ entry point would not be.
/// </summary>
internal static class GlCapabilities
{
    private const int GL_VENDOR = 0x1F00;
    private const int GL_RENDERER = 0x1F01;
    private const int GL_VERSION = 0x1F02;
    private const int GL_SHADING_LANGUAGE_VERSION = 0x8B8C;

    private const int GL_MAX_VERTEX_UNIFORM_COMPONENTS = 0x8B4A;
    private const int GL_MAX_FRAGMENT_UNIFORM_COMPONENTS = 0x8B49;
    private const int GL_MAX_VERTEX_ATTRIBS = 0x8869;
    private const int GL_MAX_TEXTURE_IMAGE_UNITS = 0x8872;
    private const int GL_MAX_VERTEX_TEXTURE_IMAGE_UNITS = 0x8B4C;

    /// <summary>The largest uniform array any effect in this game declares, in vec4.</summary>
    private const int SkinFxVertexVec4 = 262;

    [DllImport("opengl32.dll", EntryPoint = "glGetString")]
    private static extern IntPtr GlGetStringWin(int name);

    [DllImport("opengl32.dll", EntryPoint = "glGetIntegerv")]
    private static extern void GlGetIntegervWin(int name, out int value);

    [DllImport("libGL.so.1", EntryPoint = "glGetString")]
    private static extern IntPtr GlGetStringUnix(int name);

    [DllImport("libGL.so.1", EntryPoint = "glGetIntegerv")]
    private static extern void GlGetIntegervUnix(int name, out int value);

    private static bool windows = OperatingSystem.IsWindows();

    private static string Str(int name)
    {
        IntPtr p = windows ? GlGetStringWin(name) : GlGetStringUnix(name);
        return p == IntPtr.Zero ? "?" : Marshal.PtrToStringAnsi(p);
    }

    private static int Int(int name)
    {
        if (windows) { GlGetIntegervWin(name, out int w); return w; }
        GlGetIntegervUnix(name, out int u);
        return u;
    }

    /// <summary>
    /// Prints the report, or one line saying why it could not. Never throws: a diagnostic that
    /// takes the diagnosis down with it is worse than no diagnostic - and P/Invoking into a GL
    /// library by name is exactly the kind of thing that fails on a platform nobody tried.
    /// </summary>
    public static void Report()
    {
        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("OpenGL driver:");
            sb.AppendLine($"    vendor      {Str(GL_VENDOR)}");
            sb.AppendLine($"    renderer    {Str(GL_RENDERER)}");
            sb.AppendLine($"    version     {Str(GL_VERSION)}");
            sb.AppendLine($"    GLSL        {Str(GL_SHADING_LANGUAGE_VERSION)}");

            int vsComponents = Int(GL_MAX_VERTEX_UNIFORM_COMPONENTS);
            int psComponents = Int(GL_MAX_FRAGMENT_UNIFORM_COMPONENTS);
            int needed = SkinFxVertexVec4 * 4;

            sb.AppendLine("  limits:");
            sb.AppendLine($"    MAX_VERTEX_UNIFORM_COMPONENTS    {vsComponents,6}   "
                          + $"(skinFX's vertex shader needs {needed})");
            sb.AppendLine($"    MAX_FRAGMENT_UNIFORM_COMPONENTS  {psComponents,6}");
            sb.AppendLine($"    MAX_VERTEX_ATTRIBS               {Int(GL_MAX_VERTEX_ATTRIBS),6}");
            sb.AppendLine($"    MAX_TEXTURE_IMAGE_UNITS          {Int(GL_MAX_TEXTURE_IMAGE_UNITS),6}");
            sb.AppendLine($"    MAX_VERTEX_TEXTURE_IMAGE_UNITS   {Int(GL_MAX_VERTEX_TEXTURE_IMAGE_UNITS),6}");

            // The verdict, said out loud, because the numbers alone need someone to remember
            // which way round the comparison goes.
            if (vsComponents > 0 && vsComponents < needed)
            {
                sb.AppendLine();
                sb.AppendLine($"  !! skinFX CANNOT FIT: it needs {needed} vertex uniform components and this");
                sb.AppendLine($"     driver allows {vsComponents}. Every animated model will fail to draw.");
            }
            else if (vsComponents >= needed)
            {
                sb.AppendLine($"    -> uniform space is sufficient; an effect failure here is the GLSL dialect,");
                sb.AppendLine($"       not the limits (see SD0403 in the effect build log).");
            }

            Console.Write(sb.ToString());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"OpenGL driver: could not be queried ({ex.GetType().Name}: {ex.Message})");
        }
    }
}
#endif
