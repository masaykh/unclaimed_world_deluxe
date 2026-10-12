namespace UWGame.Mods;

/// <summary>
/// The main menu's MAP EDITOR and TEST SCENE (in MODDING), and a test map shown whole. From the
/// community Unhidden Mod, split out of it on Kastuk's request (2026-10-11: "All stuff related to
/// interface of main menu like Show all test maps and Editor buttons can be moved to own mod
/// pack"). The settings were unhidden.mapEditorButtons and unhidden.revealTestMap;
/// ModSettings.LegacyIds reads those from an older file. Interface only.
/// </summary>
public static class MainMenuMod
{
    public const string ModId = "mainmenu";

    private static ModSetting mapEditorButtons;

    private static ModSetting revealTestMap;

    /// <summary>
    /// Whether MODDING's MAP EDITOR and TEST SCENE can be used (ModdingDialog), calling the
    /// MainMenuScreen.EditMap and TestMap methods the studio ships but never wired to a button.
    /// </summary>
    public static ModSetting MapEditorButtons =>
        mapEditorButtons ?? (mapEditorButtons = ModSettings.Toggle(
            ModId, "mapEditorButtons", "MAP EDITOR BUTTONS", defaultValue: true,
            toolTip: "Lets MODDING on the main menu open the studio's map editor, and a map " +
                     "played with no colonists on it. Takes effect the next time the main menu opens."));

    /// <summary>
    /// Whether a map played through TEST SCENE is shown whole. A test map has no colonists, so
    /// nothing ever sees any of it (Kastuk: "TEST is not really useful without visible ground").
    /// Drawing only - GameWorldRenderer.GetIsInGodMode, the studio's own switch for the editor,
    /// which the renderer, minimap and resource icons already honour; FogMap follows it.
    /// </summary>
    public static ModSetting RevealTestMap =>
        revealTestMap ?? (revealTestMap = ModSettings.Toggle(
            ModId, "revealTestMap", "SHOW ALL OF A TEST MAP", defaultValue: true,
            toolTip: "A map opened with TEST SCENE is drawn whole, as in the editor: no fog of war " +
                     "and no black unexplored ground. Ordinary games are not affected."));

    public static void RegisterSettings()
    {
        ModSettings.SetCategoryLabel(ModId, "MAIN MENU");
        ModSettings.Describe(ModId, "Community (Unhidden Mod)",
            "The map editor and test scenes from MODDING on the main menu, and a test map shown whole with no fog of war.",
            "HUD_thumbnail_path");
        _ = MapEditorButtons;
        _ = RevealTestMap;
    }

    /// <summary>Whether MODDING's MAP EDITOR and TEST SCENE are usable.</summary>
    public static bool EditorButtons => MapEditorButtons.On;

    /// <summary>True while playing a test map with <see cref="RevealTestMap"/> on.</summary>
    public static bool RevealsTestMap =>
        RevealTestMap.On
        && The.Sim != null
        && The.Sim.Mode == UWGame.SimSide.Sim.EngineMode.Game
        && The.Sim.StartGameParams?.StartGameEditorParams != null;
}
