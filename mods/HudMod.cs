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
public static partial class HudMod
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
    /// the middle one, hidden again 10 seconds after the last speaker's portrait has gone (Kastuk,
    /// after the first cut counted from the line itself: "may be too fast"); lines are not copied
    /// into the bottom log in any mode. They are still recorded in Client.Log.TalkEvents.
    /// </summary>
    public static ModSetting TalkPanel =>
        talkPanel ?? (talkPanel = ModSettings.Choice(
            ModId, "talkPanel", "TALK PANEL", new[] { TalkAlways, TalkWhenSpoken, TalkHidden }, TalkAlways,
            toolTip: "The panel on the left where colonists talk: always shown (the studio's way), " +
                     "shown when someone speaks and hidden 10 seconds after the last speaker's portrait " +
                     "has gone, or hidden."));

    /// <summary>How long the talk panel stays after the last speaker's portrait has gone, in the middle mode.</summary>
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
    // Kastuk, 2026-10-06: "Add layer of markers to hide Zones separately too." ZONES hides a zone's
    // area on the ground (GameWorldRenderer.SetupInterfaceOnMapQuads) and its label
    // (InGameInterface.RefreshZoneMarkers). The studio had thought of it there:
    // "if (The.InGameUI.ShowOverlaysAndMarkerWindows) // || The.InGameUI.Minimap.ShowZones".
    // A selected zone always shows, so the one being worked on cannot vanish.
    //
    // Phrased as HIDE so that off is the studio's game. They are rows in the overlay panel above
    // the minimap (HUDOverlayPanel), kept in ModSettings.xml rather than the save.

    private static readonly ModSetting[] hideMarkers = new ModSetting[4];

    private static readonly string[] markerKeys = { "hideNames", "hideLabels", "hideStatus", "hideZones" };

    private static readonly string[] markerLabels = { "HIDE NAMES ON THE MAP", "HIDE LABELS ON THE MAP", "HIDE STATUS ICONS ON THE MAP", "HIDE ZONES ON THE MAP" };

    private static readonly string[] markerTips =
    {
        "Colonist and creature names on the map. A selected one always shows its name.",
        "Point-of-interest labels on the map: fishing spots, arable plots, the port location.",
        "Status icons on the map - problems and what is being made.",
        "Zones on the map - their area and their label. A selected zone always shows.",
    };

    private static ModSetting HideMarker(int i) =>
        hideMarkers[i] ?? (hideMarkers[i] = ModSettings.Toggle(ModId, markerKeys[i], markerLabels[i], defaultValue: false,
            toolTip: markerTips[i] + " Also a row in the overlay panel above the minimap."));

    public const int MarkerRowCount = 4;

    public static string MarkerRowLabel(int i) => i switch
    {
        0 => UWGame.Locale.Text("NAMES"),
        1 => UWGame.Locale.Text("LABELS"),
        2 => UWGame.Locale.Text("STATUS ICONS"),
        _ => UWGame.Locale.Text("ZONES"),
    };

    public static string MarkerRowToolTip(int i) => i switch
    {
        0 => UWGame.Locale.Text("Show colonist and creature names on the map. A selected one always shows its name."),
        1 => UWGame.Locale.Text("Show point-of-interest labels on the map: fishing spots, arable plots, the port location."),
        2 => UWGame.Locale.Text("Show status icons on the map - problems and what is being made."),
        _ => UWGame.Locale.Text("Show zones on the map - their area and their label. A selected zone always shows."),
    };

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

    /// <summary>
    /// Called from GameWorldRenderer.SetupInterfaceOnMapQuads and InGameInterface.RefreshZoneMarkers:
    /// whether a zone's area and label show. A selected zone always does.
    /// </summary>
    public static bool ShowsZone(bool selected) => selected || !HideMarker(3).On;

    private static ModSetting revealKey;

    /// <summary>The key held to show them. LeftAlt, as asked; rebindable in the KEYS section.</summary>
    public static ModSetting RevealKey =>
        revealKey ?? (revealKey = ModSettings.Key(
            ModId, "revealKey", "NAMES AND MARKERS (HOLD)", Keys.LeftAlt,
            toolTip: "The key held to show colonist names, status icons, labels and the zone grid, " +
                     "when HOLD LEFT ALT FOR NAMES AND MARKERS is on. Press it twice quickly to keep " +
                     "them shown, and twice again to go back. Drag-select still uses LeftAlt."));

    private static ModSetting labelList;

    /// <summary>Overlapping labels open into a list - see HudModLabelList.cs.</summary>
    public static ModSetting LabelListSetting =>
        labelList ?? (labelList = ModSettings.Toggle(
            ModId, "labelList", "OVERLAPPING LABELS OPEN INTO A LIST", defaultValue: true,
            toolTip: "Rest the pointer for a second on a label that other labels cover, and they " +
                     "all line up in a list under it, each one clickable. Move away to close it."));

    public static void RegisterSettings()
    {
        ModSettings.SetCategoryLabel(ModId, "HUD");
        _ = MarkersOnAltSetting;
        _ = RevealKey;
        _ = TalkPanel;
        _ = ItemLayers;
        _ = EffectsOnTopSetting;
        _ = LabelListSetting;
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
                     "on the map. The lists in the markers menu are sorted by name."));

    /// <summary>
    /// Which item layer a type belongs to, or null (not an item, or the layers are off).
    ///
    /// Tools before weapons. Kastuk: "tools like Spade is not shown in markers Tools list". Every
    /// hand tool that can also be swung in a fight carries a WeaponType - spades, hoes, pickaxes,
    /// hand axes, knives, machetes, the hammer: 15 types - and asking WeaponType first put them all
    /// in WEAPONS. What makes an item a tool in the studio's data is a ToolType: a process can only
    /// name an item as its tool if it has one (Tool.PostInitValidate refuses anything else).
    ///
    /// The one exception is the studio's own category. Spears are ToolTypes too (gathering from
    /// an ursinix takes one), but EntityCategory "weapons" files them with the guns, and the
    /// stockpile and trade windows list them there; the markers menu agrees with those windows.
    /// The same category puts the sentry items, which have no WeaponType, in WEAPONS rather than
    /// MATERIALS, and "tools" puts the fire extinguisher, which has no ToolType, in TOOLS.
    /// </summary>
    public static EntityGrouping? ItemGrouping(EntityType type)
    {
        if (!ItemLayers.On || type?.ItemType == null || type.StructureType != null || type.BiologicalType != null || type.IsIntrinsic())
        {
            return null;
        }
        if (type.CategoryKey == WeaponsCategory)
        {
            return EntityGrouping.Weapons;
        }
        if (type.ToolType != null || type.CategoryKey == ToolsCategory)
        {
            return EntityGrouping.Tools;
        }
        if (type.ItemType.WeaponType != null)
        {
            return EntityGrouping.Weapons;
        }
        if (type.ItemType.FoodType != null)
        {
            return type.ItemType.FoodType.IsMeal ? EntityGrouping.PreparedFood : EntityGrouping.Ingredients;
        }
        return EntityGrouping.Materials;
    }

    /// <summary>The studio's EntityCategory keys for the two item categories the layers follow outright.</summary>
    private const string WeaponsCategory = "weapons";

    private const string ToolsCategory = "tools";

    /// <summary>
    /// Called from HUDOverlayPanel.Populate: whether the rows inside each category of the markers
    /// menu are put in alphabetical order. Kastuk: "Need alphabet sort for the list, just like in
    /// stockpiles and trade." It goes with the item layers, whose lists are the long ones.
    /// </summary>
    public static bool SortsMarkerLists => ItemLayers.On;

    /// <summary>The menu's name for an item layer, or null for the studio's groupings.</summary>
    public static string ItemGroupingName(EntityGrouping grouping) => grouping switch
    {
        EntityGrouping.Tools => UWGame.Locale.Text("TOOLS"),
        EntityGrouping.Weapons => UWGame.Locale.Text("WEAPONS"),
        EntityGrouping.PreparedFood => UWGame.Locale.Text("PREPARED FOOD"),
        EntityGrouping.Ingredients => UWGame.Locale.Text("INGREDIENTS"),
        EntityGrouping.Materials => UWGame.Locale.Text("MATERIALS"),
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

    // ------------------------------------------------------------------ smoke and sparks on top

    private static ModSetting effectsOnTop;

    /// <summary>
    /// Kastuk, 2026-10-05: "Old bug with emitters effects is layered behind all other things, so
    /// smoke of production structures and sulphur material sources is drawn under structures,
    /// colonists and plants. Let it be drawn over everything, except high cliffs (all terrain
    /// things, which contans "hill" in name, and also gardtower big and giant) and diamond birds at
    /// flying animation mode." GameWorldRenderer.DrawSortedObjectsMain draws the map in rows, back
    /// to front, and each object's particles straight after it (Renderable.Draw), so anything in a
    /// nearer row covered them. With this on the particles are drawn after the rows, except that a
    /// hill or a big tower (<see cref="CoversEffects"/>) in a nearer row still goes over them.
    /// Diamond birds do not fly in the game (BirdHopMod: flight "waits for flight animations"), so
    /// they are not an exception. Interface only.
    /// </summary>
    public static ModSetting EffectsOnTopSetting =>
        effectsOnTop ?? (effectsOnTop = ModSettings.Toggle(
            ModId, "effectsOnTop", "SMOKE AND SPARKS DRAWN OVER EVERYTHING", defaultValue: false,
            toolTip: "Smoke from workshops, fires and sulphur sources is drawn over structures, " +
                     "colonists and plants instead of behind whatever stands in front of it. Hills " +
                     "and the big rock towers still cover it."));

    /// <summary>Called from GameWorldRenderer.DrawSortedObjectsMain every frame.</summary>
    public static bool EffectsOnTop => EffectsOnTopSetting.On;

    /// <summary>The big rock towers that still stand in front of smoke: terrain:utgardstowerBig and terrain:utgardstowerTall.</summary>
    private static readonly string[] CoveringTowers = { "terrain:utgardstowerBig", "terrain:utgardstowerTall" };

    /// <summary>Whether this stands in front of smoke drawn on top: a hill (any terrain with "hill" in its key) or a big tower.</summary>
    public static bool CoversEffects(EntityType type)
    {
        string key = type?.KeyName;
        if (key == null || !key.StartsWith("terrain:", System.StringComparison.Ordinal))
        {
            return false;
        }
        return key.IndexOf("hill", System.StringComparison.OrdinalIgnoreCase) >= 0 || System.Array.IndexOf(CoveringTowers, key) >= 0;
    }

    /// <summary>Called from MapResourceRenderer: whether this item is outlined because its layer is on.</summary>
    public static bool OutlinesItem(Entity entity) =>
        entity != null && ItemGrouping(entity.EntityType) != null
        && The.InGameUI?.OverlaySettings != null && The.InGameUI.OverlaySettings.DisplayEntityType(entity.EntityType);

    /// <summary>Two presses of the key within this long latch the markers on, or off again. 0.8 s (Kastuk, 2026-10-06; was 1 s).</summary>
    public const long DoublePressMilliseconds = 800;

    private static bool keyWasDown;
    private static long? lastPressAt;
    private static bool latched;

    /// <summary>
    /// Called from InGameInterface.Update every frame; returns at once unless switched on.
    ///
    /// Held, the key shows the markers while it is down. Pressed twice within 0.8 s, it latches
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
