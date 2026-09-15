#if UW_GL
using System;
using System.Runtime.InteropServices;

namespace UWGame.Port;

/// <summary>
/// PORT DEVIATION 19. Reports, in words, when this machine's OpenGL driver cannot hold the
/// game's shaders - instead of letting it crash several screens later with a message that names
/// nothing.
///
/// WHAT IT IS FOR. Reported from an Intel HD Graphics machine on a 2016 driver: the menu worked,
/// and loading a map died the instant the first animated model drew, with
///
///     Failed to compile vertex shader &lt;unknown&gt;. See &lt;unknown&gt;.
///        at Microsoft.Xna.Framework.Graphics.Shader.GetShaderHandle()
///        ...
///        at UWGame.ClientSide.Map.GameWorldRenderer.DrawShadows(Boolean drawModels)
///
/// Both &lt;unknown&gt;s are MonoGame's: the GLSL info log, the only thing that says WHY, never
/// reaches the exception. There is nothing in that report for a player to act on, and nothing in
/// it for us either - it took a purpose-built probe to find out that the driver allows
/// GL_MAX_VERTEX_UNIFORM_COMPONENTS = 512.
///
/// WHY 512 IS FATAL AND NOT MERELY TIGHT. 512 components is 128 vec4, and it is the OpenGL 2.0
/// specification's MINIMUM - the least a conforming driver may offer. skinFX's vertex shader
/// needs 262 vec4, of which the Animation library's MatrixPalette[56] is 224 on its own. The bone
/// palette alone is 1.75x the entire budget. No packing helps: the tightest possible layout of
/// that shader is still 243 vec4, and a palette small enough to fit would hold 22 bones where the
/// game's models use 39 to 58.
///
/// So this is not a bug to fix in the shader compiler, and the check does not pretend otherwise.
/// It names the DirectX build, which runs the studio's own compiled shaders through Direct3D 11 -
/// where the same machine's constant-buffer budget is 4096 vec4, because Intel's D3D driver for
/// that hardware is far better than its OpenGL one.
///
/// CONSERVATIVE, like its sibling EffectVersionCheck: anything it cannot establish is treated as
/// fine. A guard that stops the game on its own uncertainty is worse than the crash it replaces -
/// and the query itself is P/Invoke into whatever GL library is loaded, which is exactly the kind
/// of thing that fails somewhere nobody tried.
/// </summary>
public static class GlCapabilityCheck
{
    private const int GL_RENDERER = 0x1F01;
    private const int GL_VERSION = 0x1F02;
    private const int GL_MAX_VERTEX_UNIFORM_COMPONENTS = 0x8B4A;

    /// <summary>
    /// What skinFX's vertex shader declares, in float components: 262 vec4.
    ///
    /// Hard-coded rather than read from the effect, because the effect cannot be read before the
    /// content manager exists and this has to run before the first draw. It changes only when the
    /// shader or the compiler does, and tools/ContentProbe prints the real figure on any machine
    /// - so a drift shows up there rather than being silently wrong here.
    /// </summary>
    private const int ComponentsNeeded = 262 * 4;

    [DllImport("opengl32.dll", EntryPoint = "glGetString")]
    private static extern IntPtr GlGetStringWin(int name);

    [DllImport("opengl32.dll", EntryPoint = "glGetIntegerv")]
    private static extern void GlGetIntegervWin(int name, out int value);

    [DllImport("libGL.so.1", EntryPoint = "glGetString")]
    private static extern IntPtr GlGetStringUnix(int name);

    [DllImport("libGL.so.1", EntryPoint = "glGetIntegerv")]
    private static extern void GlGetIntegervUnix(int name, out int value);

    private static string Str(int name)
    {
        IntPtr p = OperatingSystem.IsWindows() ? GlGetStringWin(name) : GlGetStringUnix(name);
        return p == IntPtr.Zero ? "unknown" : (Marshal.PtrToStringAnsi(p) ?? "unknown");
    }

    private static int Int(int name)
    {
        if (OperatingSystem.IsWindows()) { GlGetIntegervWin(name, out int w); return w; }
        GlGetIntegervUnix(name, out int u);
        return u;
    }

    /// <summary>
    /// Returns null when the game may proceed, otherwise the message to show and log.
    ///
    /// Must be called with a current GL context - i.e. after the GraphicsDevice exists - and
    /// before anything loads an effect.
    /// </summary>
    public static string Check()
    {
        int available;
        string renderer;
        string version;
        try
        {
            available = Int(GL_MAX_VERTEX_UNIFORM_COMPONENTS);
            renderer = Str(GL_RENDERER);
            version = Str(GL_VERSION);
        }
        catch (Exception)
        {
            // Could not ask. Say nothing and let the game run: it may well be fine, and being
            // wrong in this direction costs a crash we already have rather than a game that
            // refuses to start on a machine that would have worked.
            return null;
        }

        // Zero means the query failed or there is no context yet - not a tiny budget.
        if (available <= 0 || available >= ComponentsNeeded)
        {
            return null;
        }

        return "This computer's OpenGL driver is too small for the game's shaders, so the "
             + "OpenGL build cannot run here.\n\n"
             + $"    graphics driver   {renderer}\n"
             + $"    OpenGL version    {version}\n"
             + $"    vertex uniforms   {available} components; the character shader needs {ComponentsNeeded}\n\n"
             + "This is a limit of the driver, not a setting, and nothing in the game can work "
             + "around it: the bone palette an animated character needs is larger than everything "
             + "this driver allows a vertex shader to hold.\n\n"
             + "USE THE DIRECTX BUILD INSTEAD - the archive whose name ends in -win-x64-dx. It is "
             + "the same game and runs the studio's own compiled shaders through Direct3D, which "
             + "this computer supports properly.\n\n"
             + "Without it the game would start, show the menu, and fail the moment a map loaded "
             + "with \"Failed to compile vertex shader\", which is why it is stopping now instead.";
    }
}
#endif
