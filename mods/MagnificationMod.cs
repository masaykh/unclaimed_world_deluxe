namespace UWGame.Mods;

/// <summary>
/// Magnification below 1, and a warning when the interface will not fit.
///
/// HOW MAGNIFICATION WORKS HERE. The whole game - world and interface together - is drawn into a
/// render target of <c>backBuffer / ZoomFactor</c> and stretched to the window. Above 1 that means
/// fewer, larger pixels: everything gets bigger and coarser. <c>Controller</c> then clamps the
/// factor to a minimum of 1, so the other direction was simply unavailable.
///
/// WHAT THIS CHANGES. The floor comes down to <see cref="Minimum"/>. Below 1 the render target is
/// LARGER than the window and is scaled down, so the interface takes less of the screen and more
/// of the world is visible - which is what somebody on a large monitor asking for a smaller UI
/// actually wants. It costs fill rate: 0.5 is four times the pixels.
///
/// AND A WARNING. The interface is laid out against a minimum drawable width; below it, panels are
/// cut off rather than rearranged, which is the "it's just cut out part of screen at low enough
/// resolution" in the report. The game now says so at startup instead of leaving somebody to
/// discover a truncated dialog.
///
/// NOT DONE, and it is the larger half of the request: separating interface scale from world zoom.
/// One render target holds both, so they cannot scale independently without giving the world its
/// own target and its own camera transform. That is a rendering change rather than a setting, and
/// this mod would be the wrong place for it.
/// </summary>
public static class MagnificationMod
{
    public const string ModId = "magnification";

    /// <summary>
    /// The smallest factor allowed. 0.5 is four times the pixels of 1.0 - already a real cost on a
    /// large display - and small enough that the interface is a third of the height it was.
    /// </summary>
    public const float Minimum = 0.5f;

    /// <summary>
    /// The narrowest draw area the interface is laid out for. The widest fixed panel is the save
    /// and load dialog at 822 logical pixels; below about a thousand there is no room for it plus
    /// the margins the panels assume.
    /// </summary>
    public const int MinimumUsableWidth = 1024;

    /// <summary>The matching height, from the same panels.</summary>
    public const int MinimumUsableHeight = 720;

    private static ModSetting allowBelowOne;

    private static ModSetting warnWhenTooSmall;

    /// <summary>Whether magnification may go below 1.</summary>
    public static ModSetting AllowBelowOne =>
        allowBelowOne ?? (allowBelowOne = ModSettings.Toggle(
            ModId, "allowBelowOne", "ALLOW MAGNIFICATION BELOW 1.0", defaultValue: true,
            toolTip: "Lets the magnification slider go down to 0.5, which renders at a higher " +
                     "resolution than the window and scales down - a smaller interface and more " +
                     "of the world on screen. It costs performance: 0.5 is four times the pixels. " +
                     "Like magnification itself, switching it takes effect at the next start.",
            takesEffectOnNextLoad: true));

    /// <summary>Whether to complain when the resulting draw area is too small for the interface.</summary>
    public static ModSetting WarnWhenTooSmall =>
        warnWhenTooSmall ?? (warnWhenTooSmall = ModSettings.Toggle(
            ModId, "warnWhenTooSmall", "WARN WHEN THE INTERFACE WILL NOT FIT", defaultValue: true,
            toolTip: "Reports at startup when the resolution and magnification together leave " +
                     "less room than the interface is laid out for, instead of letting panels be " +
                     "cut off at the screen edge."));

    public static void RegisterSettings()
    {
        // SELECT MODS (main menu -> MODDING): who wrote it, what it does, and a picture.
        ModSettings.Describe(ModId, "Jerrybi",
            "Magnification below 1.0, for small screens and large monitors, and a warning when the interface will not fit.",
            "HUD_thumbnail_satelliteDish");
        _ = AllowBelowOne;
        _ = WarnWhenTooSmall;
    }

    /// <summary>
    /// The lowest magnification the OPTIONS DIALOG will accept, in percent.
    ///
    /// A THIRD PLACE THE FLOOR OF 1 LIVED, and the one that mattered most: lifting the clamp in
    /// Controller and fixing ZoomIsActive let a value below 1 WORK, but OptionsDialog.ValidateInput
    /// refused to let anyone enter one - "Magnification must be at least 1." - so the feature was
    /// reachable only by hand-editing Options.xml. Reported twice before it was found, because
    /// setting it in the file did work and that looked like the feature working.
    /// </summary>
    /// <summary>
    /// The MAGNIFICATION slider's step, in percent: 0.05 rather than the studio's 0.25. Kastuk asked
    /// for steps between 0.5 and 1.0, and a value like 0.8 set in Options.xml came back from the
    /// dialog as 0.75, because the slider snapped it.
    /// </summary>
    public static int OptionsStepSize() => 5;

    public static int OptionsFloorPercent() => OptionsFloorPercent(AllowBelowOne.On);

    /// <summary>
    /// The floor for a given state of the switch. The dialog asks with the state its checkbox
    /// SHOWS, not the one in force: Kastuk switched the mod off with 0.8 set, and from then on the
    /// dialog refused every OK ("at least 1") until the slider went back up by hand.
    /// </summary>
    public static int OptionsFloorPercent(bool allowBelowOne) => allowBelowOne ? (int)(Minimum * 100f) : 100;

    /// <summary>
    /// The switch as it stood when the magnification in force was set (Controller, at startup).
    /// Magnification only changes at a restart, so the switch must too: read live, switching the
    /// mod off mid-session made ZoomIsActive answer "no" for the 0.8 still in force, the scaled
    /// render target was dropped, and the interface came out zoomed to an unreadable size (Kastuk,
    /// with a screenshot).
    /// </summary>
    private static bool? allowBelowOneInForce;

    private static bool AllowBelowOneInForce => allowBelowOneInForce ?? AllowBelowOne.On;

    /// <summary>
    /// The magnification to actually use, given what the options file asked for.
    ///
    /// The studio's line was <c>ClampBottom(ZoomFactor, 1f)</c>; this replaces it, so with the
    /// switch off the answer is identical.
    /// </summary>
    public static float Clamp(float wanted)
    {
        allowBelowOneInForce = AllowBelowOne.On;
        if (!AllowBelowOne.On)
        {
            return Common.ClampBottom(wanted, 1f);
        }
        // A zero or negative factor would divide the back buffer by zero. The floor is a real
        // limit rather than a guard against nonsense, but it serves as both.
        return Common.Clamp(wanted, Minimum, 4f);
    }

    /// <summary>
    /// Whether the scaled render target is used at all. Was <c>ActiveZoomFactor &gt; 1f</c>, which
    /// silently disabled everything below 1 even once the clamp allowed it.
    /// </summary>
    public static bool ZoomIsActive(float activeZoomFactor)
    {
        if (!AllowBelowOneInForce)
        {
            return activeZoomFactor > 1f;
        }
        return !Common.IsZero(activeZoomFactor - 1f);
    }

    /// <summary>
    /// A sentence about the interface not fitting, or null when it does.
    ///
    /// Reported once at startup by the caller. It names both numbers because either can be the
    /// cause: a small window, or a magnification that made a large one small.
    /// </summary>
    public static string TooSmallWarning(int drawWidth, int drawHeight, float magnification)
    {
        if (!WarnWhenTooSmall.On)
        {
            return null;
        }
        if (drawWidth >= MinimumUsableWidth && drawHeight >= MinimumUsableHeight)
        {
            return null;
        }
        return Common.IsZero(magnification - 1f)
            ? string.Format(UWGame.Locale.Text("The interface is laid out for at least {0}x{1} and this session has {2}x{3}. Panels wider than that are cut off at the screen edge rather than rearranged. Raise the resolution, or lower the magnification in the options."),
                MinimumUsableWidth, MinimumUsableHeight, drawWidth, drawHeight)
            : string.Format(UWGame.Locale.Text("The interface is laid out for at least {0}x{1} and this session has {2}x{3} (magnification {4:0.00}). Panels wider than that are cut off at the screen edge rather than rearranged. Raise the resolution, or lower the magnification in the options."),
                MinimumUsableWidth, MinimumUsableHeight, drawWidth, drawHeight, magnification);
    }
}
