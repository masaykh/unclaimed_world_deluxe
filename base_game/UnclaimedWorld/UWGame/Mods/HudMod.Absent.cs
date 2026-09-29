using UWGame.ClientSide.Interface;

namespace UWGame.Mods;

/// <summary>
/// The HUD mod, compiled OUT. See UnhiddenMod.Absent.cs for the pattern.
///
/// The markers switch is left to the studio's click handling: a ground click shows, a right
/// click hides.
/// </summary>
public static class HudMod
{
    /// <summary>Always false: the mod is not present in this build.</summary>
    public const bool Enabled = false;

    public const string ModId = "hud";

    public static void RegisterSettings()
    {
    }

    public static bool MarkersOnAlt => false;

    public const int MarkerRowCount = 0;

    public static string MarkerRowLabel(int i) => null;

    public static string MarkerRowToolTip(int i) => null;

    public static bool MarkerRowIsOn(int i) => true;

    public static void SetMarkerRow(int i, bool shown)
    {
    }

    /// <summary>Every marker the studio shows.</summary>
    public static bool ShowsMarker(UWGame.SimSide.Entities.EntityType type, bool byStatus) => true;

    public const string TalkAlways = "always";
    public const string TalkWhenSpoken = "when someone speaks";
    public const string TalkHidden = "hidden";

    public const double TalkPanelLingerSeconds = 10.0;

    /// <summary>The studio's talk panel: always there.</summary>
    public static string TalkPanelMode() => TalkAlways;

    public static void UpdateMarkers(InGameInterface ui, bool windowIsActive)
    {
    }
}
