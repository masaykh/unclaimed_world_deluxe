namespace UWGame.Mods;

/// <summary>
/// The magnification mod, compiled OUT. See UnhiddenMod.Absent.cs for the pattern.
///
/// The three answers below are the studio's own expressions, taken from the lines the mod
/// replaced - Controller had <c>ClampBottom(ZoomFactor, 1f)</c> and <c>ActiveZoomFactor &gt; 1f</c>
/// written out in full, and no warning at all. A stub that returned something merely harmless
/// would change what a mod-less build does, which is the one thing it must not do.
/// </summary>
public static class MagnificationMod
{
    /// <summary>Always false: the mod is not present in this build.</summary>
    public const bool Enabled = false;

    /// <summary>The prefix the mod's settings would carry.</summary>
    public const string ModId = "magnification";

    /// <summary>Registers nothing.</summary>
    public static void RegisterSettings()
    {
    }

    /// <summary>The studio's options floor: 100%, and their exact wording for it.</summary>
    public static int OptionsFloorPercent()
    {
        return 100;
    }

    /// <summary>The studio's floor of 1, which is what their line did.</summary>
    public static float Clamp(float wanted)
    {
        return Common.ClampBottom(wanted, 1f);
    }

    /// <summary>The studio's test: only a factor ABOVE 1 used the scaled render target.</summary>
    public static bool ZoomIsActive(float activeZoomFactor)
    {
        return activeZoomFactor > 1f;
    }

    /// <summary>No warning - the studio's code said nothing about the window being too small.</summary>
    public static string TooSmallWarning(int drawWidth, int drawHeight, float magnification)
    {
        return null;
    }
}
