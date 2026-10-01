using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using UWGame.ClientSide.Interface;
using UWGame.ClientSide.Interface.Overlays;
using UWGame.SimSide.Entities;

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

    // ---- which markers show -------------------------------------------------------------------
    //
    // Kastuk, after the activity-line fix: markers of different colonists still overlap at random,
    // "I guess only way is to hide layers of markers separately, like to hide only names. It need
    // additional buttons in bottom menu of minimap markers." InGameInterface.RefreshEntityMarkers
    // already sorts every marker into one of three kinds, so each gets a switch: NAMES (colonists
    // and creatures - an Always marker on something with a mind), LABELS (an Always marker on
    // anything else: fishing spots, arable plots, the port location) and STATUS (the ByStatus
    // icons - problems and crafting). A selected entity always shows its marker.
    //
    // Phrased as HIDE so that off is the studio's game. They are rows in the overlay panel above
    // the minimap (HUDOverlayPanel), kept in ModSettings.xml rather than the save.

    private static readonly ModSetting[] hideMarkers = new ModSetting[3];

    private static readonly string[] markerKeys = { "hideNames", "hideLabels", "hideStatus" };

    private static readonly string[] markerLabels = { "HIDE NAMES ON THE MAP", "HIDE LABELS ON THE MAP", "HIDE STATUS ICONS ON THE MAP" };

    private static readonly string[] markerRows = { "NAMES", "LABELS", "STATUS ICONS" };

    private static readonly string[] markerTips =
    {
        "Colonist and creature names on the map. A selected one always shows its name.",
        "Point-of-interest labels on the map: fishing spots, arable plots, the port location.",
        "Status icons on the map - problems and what is being made.",
    };

    private static ModSetting HideMarker(int i) =>
        hideMarkers[i] ?? (hideMarkers[i] = ModSettings.Toggle(ModId, markerKeys[i], markerLabels[i], defaultValue: false,
            toolTip: markerTips[i] + " Also a row in the overlay panel above the minimap."));

    public const int MarkerRowCount = 3;

    public static string MarkerRowLabel(int i) => markerRows[i];

    public static string MarkerRowToolTip(int i) => "Show " + markerTips[i].Substring(0, 1).ToLowerInvariant() + markerTips[i].Substring(1);

    public static bool MarkerRowIsOn(int i) => !HideMarker(i).On;

    /// <summary>From the overlay panel's checkbox: shown or not, saved at once like the developer rows.</summary>
    public static void SetMarkerRow(int i, bool shown)
    {
        HideMarker(i).Value = shown ? "false" : "true";
        ModSettings.Save(GameStateManagement.UnclaimedWorld.LogError);
    }

    /// <summary>Called from InGameInterface.RefreshEntityMarkers: whether this kind of marker may show.</summary>
    public static bool ShowsMarker(EntityType type, bool byStatus)
    {
        if (byStatus)
        {
            return !HideMarker(2).On;
        }
        return type?.IntelligenceType != null ? !HideMarker(0).On : !HideMarker(1).On;
    }

    private static ModSetting revealKey;

    /// <summary>The key held to show them. LeftAlt, as asked; rebindable in the KEYS section.</summary>
    public static ModSetting RevealKey =>
        revealKey ?? (revealKey = ModSettings.Key(
            ModId, "revealKey", "NAMES AND MARKERS (HOLD)", Keys.LeftAlt,
            toolTip: "The key held to show colonist names, status icons, labels and the zone grid, " +
                     "when HOLD LEFT ALT FOR NAMES AND MARKERS is on. Press it twice quickly to keep " +
                     "them shown, and twice again to go back. Drag-select still uses LeftAlt."));

    public static void RegisterSettings()
    {
        ModSettings.SetCategoryLabel(ModId, "HUD");
        _ = MarkersOnAltSetting;
        _ = RevealKey;
        _ = TalkPanel;
        _ = ItemLayers;
        for (int i = 0; i < MarkerRowCount; i++)
        {
            _ = HideMarker(i);
        }
    }

    /// <summary>Whether the click sites should leave the markers switch alone.</summary>
    public static bool MarkersOnAlt => MarkersOnAltSetting.On;

    // ------------------------------------------------------------------ item layers

    private static ModSetting itemLayers;

    /// <summary>
    /// Kastuk: "add layer switchers to markers menu to mark Items, to drawn their outlines and mark on
    /// minimap, just like current list of Raw materials. Full list of items may be too big, so add
    /// switchers just for categories, like Tools, Weapons, Prepared food, Materials and Ingredients."
    ///
    /// The studio's overlay menu already does this for entities by GROUPING (OverlaySettings.GetGrouping:
    /// structures, animals, colony members, places of interest): a category row with a switch, the types
    /// inside it folded away, a colour on the minimap. With this on, items get five groupings of their
    /// own (<see cref="ItemGrouping"/>), and the same rows, switches and minimap colours follow; an item
    /// drawn as a billboard is also outlined on the map while its layer is on, as trees bearing a chosen
    /// crop are (MapResourceRenderer). Interface only.
    /// </summary>
    public static ModSetting ItemLayers =>
        itemLayers ?? (itemLayers = ModSettings.Toggle(
            ModId, "itemLayers", "ITEM LAYERS IN THE MARKERS MENU", defaultValue: true,
            toolTip: "Adds TOOLS, WEAPONS, PREPARED FOOD, INGREDIENTS and MATERIALS to the markers menu " +
                     "above the minimap. Switch one on to mark those items on the minimap and outline them " +
                     "on the map."));

    /// <summary>Which item layer a type belongs to, or null (not an item, or the layers are off).</summary>
    public static EntityGrouping? ItemGrouping(EntityType type)
    {
        if (!ItemLayers.On || type?.ItemType == null || type.StructureType != null || type.BiologicalType != null || type.IsIntrinsic())
        {
            return null;
        }
        if (type.ItemType.WeaponType != null)
        {
            return EntityGrouping.Weapons;
        }
        if (type.ToolType != null)
        {
            return EntityGrouping.Tools;
        }
        if (type.ItemType.FoodType != null)
        {
            return type.ItemType.FoodType.IsMeal ? EntityGrouping.PreparedFood : EntityGrouping.Ingredients;
        }
        return EntityGrouping.Materials;
    }

    /// <summary>The menu's name for an item layer, or null for the studio's groupings.</summary>
    public static string ItemGroupingName(EntityGrouping grouping) => grouping switch
    {
        EntityGrouping.Tools => "TOOLS",
        EntityGrouping.Weapons => "WEAPONS",
        EntityGrouping.PreparedFood => "PREPARED FOOD",
        EntityGrouping.Ingredients => "INGREDIENTS",
        EntityGrouping.Materials => "MATERIALS",
        _ => null,
    };

    /// <summary>An item layer's colour on the minimap and in the menu, or null for the studio's groupings.</summary>
    public static Color? ItemGroupingColor(EntityGrouping grouping, bool faded)
    {
        Color? c = grouping switch
        {
            EntityGrouping.Tools => new Color(120, 200, 255),
            EntityGrouping.Weapons => new Color(255, 90, 90),
            EntityGrouping.PreparedFood => new Color(255, 200, 80),
            EntityGrouping.Ingredients => new Color(150, 230, 110),
            EntityGrouping.Materials => new Color(200, 170, 140),
            _ => null,
        };
        return c.HasValue && faded ? c.Value * 0.55f : c;
    }

    /// <summary>Called from MapResourceRenderer: whether this item is outlined because its layer is on.</summary>
    public static bool OutlinesItem(Entity entity) =>
        entity != null && ItemGrouping(entity.EntityType) != null
        && The.InGameUI?.OverlaySettings != null && The.InGameUI.OverlaySettings.DisplayEntityType(entity.EntityType);

    /// <summary>Two presses of the key within this long latch the markers on, or off again.</summary>
    public const long DoublePressMilliseconds = 1000;

    private static bool keyWasDown;
    private static long? lastPressAt;
    private static bool latched;

    /// <summary>
    /// Called from InGameInterface.Update every frame; returns at once unless switched on.
    ///
    /// Held, the key shows the markers while it is down. Pressed twice within a second, it latches
    /// them on until the next double press - Kastuk: with LeftAlt held, the Steam overlay's
    /// screenshot key does nothing, so a screenshot with names on needs them to stay by themselves.
    /// Wall-clock time, not game time: it is a keyboard gesture, and it works while paused.
    /// </summary>
    public static void UpdateMarkers(InGameInterface ui, bool windowIsActive)
    {
        if (!MarkersOnAlt || ui == null)
        {
            return;
        }
        Keys key = RevealKey.KeyValue == Keys.None ? Keys.LeftAlt : RevealKey.KeyValue;
        bool down = windowIsActive && Keyboard.GetState().IsKeyDown(key);
        ui.ShowOverlaysAndMarkerWindows = MarkersShown(down, System.Environment.TickCount64) && windowIsActive;
    }

    /// <summary>
    /// One frame of the gesture: whether the markers show, given whether the key is down now and
    /// the time in milliseconds. Separate from the keyboard so DataExport --hud-selftest can play
    /// presses through it.
    /// </summary>
    public static bool MarkersShown(bool keyDown, long nowMilliseconds)
    {
        if (keyDown && !keyWasDown)
        {
            if (lastPressAt.HasValue && nowMilliseconds - lastPressAt.Value <= DoublePressMilliseconds)
            {
                latched = !latched;
                lastPressAt = null;   // a third press starts a new pair
            }
            else
            {
                lastPressAt = nowMilliseconds;
            }
        }
        keyWasDown = keyDown;
        return keyDown || latched;
    }

    /// <summary>Back to nothing held and nothing latched.</summary>
    public static void ResetMarkerGesture()
    {
        keyWasDown = false;
        lastPressAt = null;
        latched = false;
    }
}
