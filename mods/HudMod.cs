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

    private static ModSetting revealKey;

    /// <summary>The key held to show them. LeftAlt, as asked; rebindable in the KEYS section.</summary>
    public static ModSetting RevealKey =>
        revealKey ?? (revealKey = ModSettings.Key(
            ModId, "revealKey", "SHOW NAMES AND MARKERS (HOLD)", Keys.LeftAlt,
            toolTip: "The key held to show colonist names, status icons, labels and the zone grid, " +
                     "when HOLD LEFT ALT FOR NAMES AND MARKERS is on. Drag-select still uses LeftAlt."));

    public static void RegisterSettings()
    {
        ModSettings.SetCategoryLabel(ModId, "HUD");
        _ = MarkersOnAltSetting;
        _ = RevealKey;
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
