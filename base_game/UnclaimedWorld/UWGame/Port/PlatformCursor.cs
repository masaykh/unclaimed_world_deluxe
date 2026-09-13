using Microsoft.Xna.Framework.Input;

namespace UWGame.Port;

/// <summary>
/// Applying a mouse cursor, which is the one place the backends genuinely differ.
///
/// MonoGame has <c>Mouse.SetCursor</c>; FNA does not, because XNA 4.0 did not - and a static
/// method cannot be added to a type from another assembly, so the call site has to go through
/// something we own. That is this. See <c>Port/FnaCursorShim.cs</c> for what FNA does instead,
/// and PORT DEVIATION 9 for why the game stopped assigning <c>Form.Cursor</c>.
/// </summary>
public static class PlatformCursor
{
    /// <summary>Applies a cursor. On the FNA spike build this is currently a no-op.</summary>
    public static void Set(MouseCursor cursor)
    {
#if UW_FNA
        // The shim resolves everything to the system arrow, which SDL already shows.
        _ = cursor;
#else
        Mouse.SetCursor(cursor);
#endif
    }
}
