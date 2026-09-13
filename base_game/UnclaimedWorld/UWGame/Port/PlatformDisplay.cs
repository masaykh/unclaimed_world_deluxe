using Microsoft.Xna.Framework;

namespace UWGame.Port;

/// <summary>
/// The two window/display controls that do not exist in the same shape on every backend.
///
/// Both are MonoGame API surface that XNA 4.0 never had, so FNA - which reimplements XNA rather
/// than extending it - either renames them or does not have them:
///
///   IsBorderless       -> FNA calls it IsBorderlessEXT. FNA suffixes every deliberate deviation
///                         from XNA with EXT, which is how it keeps "what XNA did" legible.
///   HardwareModeSwitch -> FNA has no equivalent. Fullscreen is always desktop-resolution; there
///                         is no exclusive mode switch to turn off.
/// </summary>
public static class PlatformDisplay
{
    /// <summary>Borderless window. PORT DEVIATION 10 - was Form.FormBorderStyle.</summary>
    public static void SetBorderless(GameWindow window, bool borderless)
    {
        if (window == null)
        {
            return;
        }
#if UW_FNA
        window.IsBorderlessEXT = borderless;
#else
        window.IsBorderless = borderless;
#endif
    }

    /// <summary>
    /// Moves the window's top-left corner. PORT DEVIATION 10 - was
    /// <c>((Form)Control.FromHandle(Window.Handle)).Location</c>.
    ///
    /// A no-op on FNA: <c>GameWindow</c> there exposes only <c>ClientBounds</c>, with no setter.
    /// The window lands wherever SDL puts it, which is centred on the primary display - an
    /// acceptable difference for a diagnostic placement that exists to keep the window clear of
    /// the taskbar.
    /// </summary>
    public static void SetPosition(GameWindow window, Point position)
    {
        if (window == null)
        {
            return;
        }
#if UW_FNA
        _ = position;
#else
        window.Position = position;
#endif
    }

    /// <summary>
    /// Whether fullscreen may change the display mode. A no-op on FNA, which has no exclusive
    /// fullscreen to switch into.
    ///
    /// Worth noting rather than hiding: this is the option suspected of causing the stale-frame
    /// screenshot behaviour, because an exclusive-fullscreen window bypasses DWM composition and
    /// capture tools then read a stale surface. If that is right, FNA not having it is a fix
    /// rather than a loss. See todo.md.
    /// </summary>
    public static void SetHardwareModeSwitch(GraphicsDeviceManager graphics, bool hardwareModeSwitch)
    {
        if (graphics == null)
        {
            return;
        }
#if UW_FNA
        _ = hardwareModeSwitch;
#else
        graphics.HardwareModeSwitch = hardwareModeSwitch;
#endif
    }
}
