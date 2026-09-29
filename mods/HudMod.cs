using Microsoft.Xna.Framework.Input;
using UWGame.ClientSide.Interface;

namespace UWGame.Mods;

/// <summary>
/// Names, status markers and labels on the map only while LeftAlt is held.
///
/// THE REQUEST. Kastuk, "Too much info on the screen": clicking the ground brings up the zone
/// grid, every colonist's name, status icons and the big orange point-of-interest labels, and they
/// stay until a right click. "LeftAlt key is good enough to show all that icons and names."
///
/// IT IS ONE SWITCH IN THE STUDIO'S CODE. InGameInterface.ShowOverlaysAndMarkerWindows shows or
/// hides every MarkerWindow - colonist names (ShowMarkerWindowMode.Always), status markers
/// (ByStatus), and the terrain features that carry labels (fishing spots, arable plots, the port
/// location - all Always in TerrainFeatureLoader) - and GameWorldRenderer draws the zone grid only
/// while it is on. MapClient sets it on a ground click or a drag; a right click clears it.
///
/// With this on, InGameInterface.Update sets it every frame from LeftAlt instead, and the two
/// click sites leave it alone. LeftAlt is already MapClient's limitSelectionKey for drag-select,
/// and a drag shows the markers anyway, so the two uses agree. Interface only: nothing the
/// simulation reads changes, so it is not a simulation setting.
/// </summary>
public static class HudMod
{
    public const string ModId = "hud";

    private static ModSetting markersOnAlt;

    public static ModSetting MarkersOnAltSetting =>
        markersOnAlt ?? (markersOnAlt = ModSettings.Toggle(
            ModId, "markersOnAlt", "HOLD LEFT ALT FOR NAMES AND MARKERS", defaultValue: false,
            toolTip: "Colonist names, status icons, point-of-interest labels and the zone grid show " +
                     "only while LeftAlt is held, instead of appearing on every ground click."));

    private static ModSetting talkPanel;

    public const string TalkAlways = "always";
    public const string TalkWhenSpoken = "when someone speaks";
    public const string TalkHidden = "hidden";

    /// <summary>
    /// The talk panel on the left - the frame, the CRT portrait and the lines (TalkPanel). Kastuk:
    /// "hide it fully / show only when dialogues is triggered / show it always, like now", and for
    /// the middle one, hidden again 10 seconds after the last line; lines are not copied into the
    /// bottom log in any mode. They are still recorded in Client.Log.TalkEvents.
    /// </summary>
    public static ModSetting TalkPanel =>
        talkPanel ?? (talkPanel = ModSettings.Choice(
            ModId, "talkPanel", "TALK PANEL", new[] { TalkAlways, TalkWhenSpoken, TalkHidden }, TalkAlways,
            toolTip: "The panel on the left where colonists talk: always shown (the studio's way), " +
                     "shown when someone speaks and hidden 10 seconds after the last line, or hidden."));

    /// <summary>How long the talk panel stays after the last line, in the middle mode.</summary>
    public const double TalkPanelLingerSeconds = 10.0;

    /// <summary>The talk panel mode; read by TalkPanel.Update every frame.</summary>
    public static string TalkPanelMode() => TalkPanel.Value;

    private static ModSetting revealKey;

    /// <summary>The key held to show them. LeftAlt, as asked; rebindable in the KEYS section.</summary>
    public static ModSetting RevealKey =>
        revealKey ?? (revealKey = ModSettings.Key(
            ModId, "revealKey", "NAMES AND MARKERS (HOLD)", Keys.LeftAlt,
            toolTip: "The key held to show colonist names, status icons, labels and the zone grid, " +
                     "when HOLD LEFT ALT FOR NAMES AND MARKERS is on. Drag-select still uses LeftAlt."));

    public static void RegisterSettings()
    {
        ModSettings.SetCategoryLabel(ModId, "HUD");
        _ = MarkersOnAltSetting;
        _ = RevealKey;
        _ = TalkPanel;
    }

    /// <summary>Whether the click sites should leave the markers switch alone.</summary>
    public static bool MarkersOnAlt => MarkersOnAltSetting.On;

    /// <summary>Called from InGameInterface.Update every frame; returns at once unless switched on.</summary>
    public static void UpdateMarkers(InGameInterface ui, bool windowIsActive)
    {
        if (!MarkersOnAlt || ui == null)
        {
            return;
        }
        Keys key = RevealKey.KeyValue == Keys.None ? Keys.LeftAlt : RevealKey.KeyValue;
        ui.ShowOverlaysAndMarkerWindows = windowIsActive && Keyboard.GetState().IsKeyDown(key);
    }
}
