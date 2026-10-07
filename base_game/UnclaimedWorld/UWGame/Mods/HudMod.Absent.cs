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

    /// <summary>Every zone, as the studio shows them.</summary>
    public static bool ShowsZone(bool selected) => true;

    public const string TalkAlways = "always";
    public const string TalkWhenSpoken = "when someone speaks";

    public const double TalkPanelLingerSeconds = 10.0;

    /// <summary>The studio's talk panel: always there.</summary>
    public static string TalkPanelMode() => TalkAlways;

    public static void UpdateMarkers(InGameInterface ui, bool windowIsActive)
    {
    }

    /// <summary>No item layers: the studio's four groupings only.</summary>
    public static UWGame.ClientSide.Interface.Overlays.EntityGrouping? ItemGrouping(UWGame.SimSide.Entities.EntityType type) => null;

    public static string ItemGroupingName(UWGame.ClientSide.Interface.Overlays.EntityGrouping grouping) => null;

    public static Microsoft.Xna.Framework.Color? ItemGroupingColor(UWGame.ClientSide.Interface.Overlays.EntityGrouping grouping, bool faded) => null;

    public static bool OutlinesItem(UWGame.SimSide.Entities.Entity entity) => false;

    /// <summary>The studio's order: each object's smoke and sparks drawn in its own row.</summary>
    public static bool EffectsOnTop => false;

    public static bool CoversEffects(UWGame.SimSide.Entities.EntityType type) => false;

    /// <summary>The studio's order: rows stay in the order they were first added.</summary>
    public static bool SortsMarkerLists => false;

    /// <summary>No gesture: shown while held, as far as anything asks.</summary>
    public static bool MarkersShown(bool keyDown, long nowMilliseconds) => keyDown;

    public static void ResetMarkerGesture()
    {
    }
}
