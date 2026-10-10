using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UWGame.SimSide;

namespace UWGame.Mods;

/// <summary>
/// Turns the studio's own developer instrumentation back on: the map overlays, and the ninety-four
/// prepared test situations behind the main menu's TEST button.
///
/// WHY IT IS WORTH HAVING. Both of the bugs found on 16 September - the self-preservation stance
/// gate that was self-fulfilling, and the animal rule that reached the whole wildlife simulation -
/// were found by reading source, twice, because there was no way to look at a job score while the
/// game ran. <c>Overlays.Jobs</c> would have shown either at a glance. tripleacoder asked "have you
/// used the debug window?" and the answer was that nobody could have: <c>Client.InitDeveloperDialog</c>
/// has zero callers, so the WinForms panel it opens was dead code in the shipped game.
///
/// THE PANEL IS GONE BUT THE PLUMBING IS NOT. <c>Kensei.Dev.Options.SetOption(name, bool)</c> writes
/// <c>s_optionsBool[name]</c> whether or not a form exists, and the ~360 call sites read that same
/// dictionary through <c>GetOption</c>. So the overlays need no UI of their own - they need
/// somebody to write to the dictionary, and the mod settings menu already is that.
///
/// TWO HALVES, AND THE SECOND IS THE ONE KASTUK ASKED FOR:
///
///   OVERLAYS        eleven switches, each one <c>SetOption</c> call.
///   TEST SCENARIOS  <c>PlaceGameEntities</c> registers 94 situations - NeedsTest, FightTest,
///                   CookingTest, FindPreyTest, StorageTest, RobotTest, BreedingTest - each with a
///                   map and a loader. The main menu's TEST button already starts one. It always
///                   started the SAME one, because GetDefaultScenario returned a hard-coded
///                   TwinklerEatTest, which is why the button looked broken: "it not load
///                   player-controlled entities and map is black". It was loading a Twinkler
///                   eating test, correctly, forever.
///
/// Everything here is OFF by default and changes nothing in a normal game.
/// </summary>
public static class DebugMod
{
    public const string ModId = "debug";

    /// <summary>What the studio's own GetDefaultScenario returned before this mod existed.</summary>
    public const string StudioTestScenario = "TwinklerEatTest";

    /// <summary>Where the list of every scenario is written, under the game folder.</summary>
    public const string ScenarioListFileName = "DebugScenarios.txt";

    /// <summary>
    /// The overlays worth a switch, as (setting key, the name Kensei.Dev knows it by, label).
    ///
    /// Not all of them: <c>PlaceGameEntities</c> and friends read several dozen option names and
    /// most are one-off rendering probes. These eleven are the ones that answer a question about
    /// the simulation rather than about a model.
    /// </summary>
    private static readonly string[][] Overlays =
    {
        new[] { "overlayJobs",        "Overlays.Jobs",                       "JOBS AND SCORES" },
        new[] { "overlayInterest",    "Overlays.Interest",                   "INTEREST" },
        new[] { "overlayRanges",      "Overlays.Ranges",                     "RANGES" },
        new[] { "overlayAllegiances", "Overlays.Allegiances",                "ALLEGIANCES" },
        new[] { "overlayMarkers",     "Overlays.Markers",                    "MARKERS" },
        new[] { "overlayPathSearch",  "Overlays.Path search",                "PATH SEARCH" },
        new[] { "overlayRegionMap",   "Overlays.Region map (Terrain/Foot)",  "REGION MAP (FOOT)" },
        new[] { "overlayCollision",   "Overlays.CollisionGeometries",        "COLLISION GEOMETRY" },
        new[] { "overlaySelection",   "Overlays.SelectionShapes",            "SELECTION SHAPES" },
        new[] { "overlayCrops",       "Overlays.Crops",                      "CROPS" },
        new[] { "overlaySensorTiles", "Overlays.Sensor tiles",               "SENSOR TILES" },
    };

    private static ModSetting[] overlaySettings;

    private static ModSetting testScenario;

    private static ModSetting recordGame;

    private static ModSetting showDevPanel;

    private static bool reportedBadScenario;

    private static bool wroteScenarioList;

    /// <summary>Which of the 94 prepared situations the main menu's TEST button loads.</summary>
    public static ModSetting TestScenario =>
        testScenario ?? (testScenario = ModSettings.Text(
            ModId, "testScenario", "TEST BUTTON SCENARIO", StudioTestScenario,
            toolTip: "The name of one of the debug scenarios in PlaceGameEntities - NeedsTest, " +
                     "FightTest, CookingTest, FindPreyTest, StorageTest, RobotTest and ninety " +
                     "more. NOTE: 86 of the 90 run on 'd Mezzomap MLo', a studio test map that " +
                     "ships with no Soil or Vegetation layer, so the GROUND IS BLACK on those - " +
                     "that is the map, not the port. MidsizeTest and mineShowcase are the two " +
                     "with real terrain. An unrecognised name writes the full list to Errors.txt " +
                     "and uses the studio's default."));

    /// <summary>
    /// Whether to record every session for replay.
    ///
    /// Options.RecordGame is the STUDIO'S switch and it works end to end - Controller reads it,
    /// Recorder opens the files, LoadingFinished starts the recording, the dev panel's LOAD REPLAY
    /// plays it back. What it never had is a control: it lives in Options.xml and nowhere else, so
    /// recording a session meant knowing the field was there and editing a file by hand. That is
    /// why a feature the studio finished went unused.
    ///
    /// Off by default, because a recording writes a file per session and most people want neither.
    /// </summary>
    public static ModSetting RecordGameSetting =>
        recordGame ?? (recordGame = ModSettings.Toggle(
            ModId, "recordGame", "RECORD SESSIONS FOR REPLAY", defaultValue: false,
            toolTip: "Writes every session to user/Replays, with the commands you gave and a "
                   + "trace of the simulation's random draws. LOAD REPLAY in the dev panel plays "
                   + "one back and reports whether it came out identical."));

    /// <summary>
    /// Whether the game should record, given what Options.xml said. The studio's value is the
    /// fallback, so with this mod out the behaviour is exactly theirs.
    /// </summary>
    public static bool RecordGame(bool studioValue)
    {
        RegisterSettings();
        return RecordGameSetting.On || studioValue;
    }

    // ---- credits for testing --------------------------------------------------------------------
    //
    // Kastuk, "Dogs and robots", 2026-10-09: "To Test GOPHER I need to buy it, but it so costly, I
    // haven't savegame with such rich colony. Need to bring debug tool to change credits quantity."
    // The studio had one on its developer panel ("Dev.Add credits", Client.AddCredits: +500). Here
    // it is a key, through the AddCredits command so that a replay carries it.

    private static ModSetting creditsKeyOn;
    private static ModSetting creditsKey;
    private static ModSetting creditsPerPress;

    public static ModSetting CreditsKeyOnSetting =>
        creditsKeyOn ?? (creditsKeyOn = ModSettings.Toggle(
            ModId, "creditsKeyOn", "KEY ADDS TRADE CREDITS", defaultValue: false,
            toolTip: "For testing. In a game, the ADD CREDITS key (F9 unless rebound in KEYS) gives "
                   + "the colony CREDITS PER PRESS more trade credits. The log says so each time."));

    public static ModSetting CreditsKey =>
        creditsKey ?? (creditsKey = ModSettings.Key(
            ModId, "creditsKey", "ADD CREDITS", Microsoft.Xna.Framework.Input.Keys.F9,
            toolTip: "Adds trade credits, when KEY ADDS TRADE CREDITS is on in DEBUG."));

    public static ModSetting CreditsPerPress =>
        creditsPerPress ?? (creditsPerPress = ModSettings.Choice(
            ModId, "creditsPerPress", "CREDITS PER PRESS", new[] { "500", "1000", "5000", "10000" }, "1000",
            toolTip: "How many trade credits one press of ADD CREDITS gives."));

    /// <summary>Asked by Client.HandleInput each frame: whether the key was the credits key, switched on.</summary>
    public static bool CreditsKeyActive => CreditsKeyOnSetting.On;

    /// <summary>The amount one press adds.</summary>
    public static decimal CreditsAmount =>
        decimal.TryParse(CreditsPerPress.Value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out decimal amount) && amount > 0m
            ? amount : 1000m;

    /// <summary>Called by the AddCredits command only.</summary>
    public static void AddCredits(UWGame.SimSide.Allegiances.Allegiance allegiance, decimal amount)
    {
        allegiance.TradeCredits = (allegiance.TradeCredits ?? 0m) + amount;
    }

    public static void RegisterSettings()
    {
        // SELECT MODS (main menu -> MODDING): who wrote it, what it does, and a picture.
        ModSettings.Describe(ModId, "Jerrybi",
            "Turns the studio's own developer tools back on: the map overlays, the dev panel with its prepared test situations, session recording for replay, and a key that adds trade credits.",
            "HUD_thumbnail_fieldLab");
        _ = TestScenario;
        _ = RecordGameSetting;
        _ = ShowMainMenuDevPanelSetting;
        _ = CreditsKeyOnSetting;
        _ = CreditsKey;
        _ = CreditsPerPress;
        WriteScenarioList();
        if (overlaySettings != null)
        {
            return;
        }
        overlaySettings = new ModSetting[Overlays.Length];
        for (int i = 0; i < Overlays.Length; i++)
        {
            overlaySettings[i] = ModSettings.Toggle(
                ModId, Overlays[i][0], "OVERLAY: " + Overlays[i][2], defaultValue: false,
                toolTip: "Draws the game's own '" + Overlays[i][1] + "' developer overlay. "
                       + "Off by default; costs nothing when off.");
        }
    }


    /// <summary>
    /// Whether to construct the studio's DEV OPTIONS panel on the main menu.
    ///
    /// THIS IS THE ONLY WAY TO REACH TWO THINGS. MainMenuDevPanel carries a TEST button wired to
    /// MainMenuScreen.StartTest - the debug SCENARIOS, which is what <see cref="TestScenario"/>
    /// chooses - and a LOAD REPLAY button wired to MainMenuScreen.LoadReplay. The panel is
    /// finished code and MainMenuInterface even declares a field to hold it; nothing ever
    /// constructed it, so neither button existed in a shipped build.
    ///
    /// Not the same as the TEST button on the main panel. That one is btTestMap_Click ->
    /// MainMenuScreen.TestMap, which opens a MAP PICKER and plays a raw map with no colonists on
    /// it. Two different features with the same word on them.
    /// </summary>
    public static ModSetting ShowMainMenuDevPanelSetting =>
        showDevPanel ?? (showDevPanel = ModSettings.Toggle(
            ModId, "devPanel", "DEV PANEL ON MAIN MENU", defaultValue: false,
            toolTip: "Shows the studio's DEV OPTIONS panel beside the main menu. Its TEST button "
                   + "starts the debug scenario chosen below, and its LOAD REPLAY button is the "
                   + "only way to play a recorded session back. Appears as soon as you close "
                   + "the options screen."));

    /// <summary>Whether the dev panel should be built, asked by the main menu.</summary>
    public static bool ShowMainMenuDevPanel
    {
        get
        {
            RegisterSettings();
            return ShowMainMenuDevPanelSetting.On;
        }
    }

    /// <summary>
    /// Whether any overlay switch is on. Separate from <see cref="Enabled"/> because the core's
    /// draw path asks this question every frame: Enabled is also true for a changed test scenario
    /// or for recording, neither of which should cost eleven dictionary lookups per frame.
    /// </summary>
    public static bool AnyOverlayOn
    {
        get
        {
            RegisterSettings();
            foreach (ModSetting setting in overlaySettings)
            {
                if (setting.On)
                {
                    return true;
                }
            }
            return false;
        }
    }

    /// <summary>
    /// Writes DebugScenarios.txt beside the game: every debug scenario, the map it loads, and
    /// whether that map has ground textures.
    ///
    /// WHY A FILE. The setting is free text and a mistyped name is indistinguishable from a
    /// working one - LogError writes to Errors.txt and nothing on screen says so, which is how a
    /// person concludes the feature is broken. Ninety names will not fit in a tooltip. So the
    /// list is simply always there, next to the executable, before anyone needs it.
    ///
    /// The soil column matters more than it looks: 86 of the 90 scenarios run on a studio test
    /// map that ships with no Soil or Vegetation folder, so the ground draws black. Knowing that
    /// in advance is the difference between a test map and a broken port.
    /// </summary>
    private static void WriteScenarioList()
    {
        if (wroteScenarioList)
        {
            return;
        }
        wroteScenarioList = true;
        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Debug scenarios, for debug.testScenario in user/ModSettings.xml or");
            sb.AppendLine("# the TEST BUTTON SCENARIO box in OPTIONS -> MODS.");
            sb.AppendLine("#");
            sb.AppendLine("# Started by the TEST button on the DEV PANEL - not the TEST button on");
            sb.AppendLine("# the main panel, which is a map picker and a different feature.");
            sb.AppendLine("#");
            sb.AppendLine("# GROUND: whether the map has a Soil folder. Without one every subtile");
            sb.AppendLine("# ends with no surface type and the ground draws black. That is the");
            sb.AppendLine("# studio's test map, not a fault in the game.");
            sb.AppendLine();
            sb.Append("scenario".PadRight(34)).Append("map".PadRight(26)).AppendLine("ground");

            var rows = new List<string>();
            foreach (var entry in PlaceGameEntities.AllScenariosAndTheirMaps())
            {
                string ground = HasGroundTextures(entry.Value) ? "yes" : "BLACK";
                rows.Add(entry.Key.ToString().PadRight(34) + entry.Value.PadRight(26) + ground);
            }
            rows.Sort(StringComparer.OrdinalIgnoreCase);
            foreach (string row in rows)
            {
                sb.AppendLine(row);
            }

            File.WriteAllText(ScenarioListFileName, sb.ToString());
        }
        catch (Exception)
        {
            // A list nobody can write is not worth failing a startup over.
        }
    }

    /// <summary>
    /// Whether a map folder carries the layer that gives the terrain a surface type. Missing is
    /// the normal state for the studio's test maps and MapLoader returns quietly for it, which is
    /// exactly why it needs saying out loud here.
    /// </summary>
    private static bool HasGroundTextures(string mapKey)
    {
        try
        {
            return Directory.Exists(Path.Combine("data", "Maps", mapKey, "Soil"));
        }
        catch (Exception)
        {
            return false;
        }
    }

    // ---- the in-game panel's view of the overlays --------------------------------------------
    //
    // The MODS options screen is the wrong place to work an overlay from: you switch one on,
    // close two dialogs, look, and go back. These let the game's own HUD overlay panel carry the
    // same switches, so they can be worked while looking at what they draw. The mod stays the
    // single source of truth - the panel writes the ModSetting, not Kensei.Dev.Options, and
    // ApplyOverlays pushes it on the next frame exactly as it does for the options screen.

    /// <summary>How many developer overlays there are. Zero with the mod absent.</summary>
    public static int OverlayCount
    {
        get
        {
            RegisterSettings();
            return overlaySettings.Length;
        }
    }

    /// <summary>The label for one, as the options screen shows it.</summary>
    public static string OverlayLabel(int index)
    {
        RegisterSettings();
        return index >= 0 && index < Overlays.Length ? Overlays[index][2] : string.Empty;
    }

    /// <summary>Its tooltip, naming the Kensei.Dev option it drives.</summary>
    public static string OverlayToolTip(int index)
    {
        RegisterSettings();
        return index >= 0 && index < Overlays.Length
            ? "Draws the game's own '" + Overlays[index][1] + "' developer overlay."
            : string.Empty;
    }

    /// <summary>Whether it is on.</summary>
    public static bool OverlayIsOn(int index)
    {
        RegisterSettings();
        return index >= 0 && index < overlaySettings.Length && overlaySettings[index].On;
    }

    /// <summary>
    /// Switches one on or off and writes ModSettings.xml, so a choice made from the HUD persists
    /// the same way one made on the options screen does.
    /// </summary>
    public static void SetOverlay(int index, bool on)
    {
        RegisterSettings();
        if (index < 0 || index >= overlaySettings.Length)
        {
            return;
        }
        overlaySettings[index].Value = on ? "true" : "false";
        ModSettings.Save(GameStateManagement.UnclaimedWorld.LogError);
    }

    /// <summary>Whether anything here is doing something.</summary>
    public static bool Enabled
    {
        get
        {
            if (!string.Equals(TestScenario.Value, StudioTestScenario, StringComparison.Ordinal)
                || RecordGameSetting.On)
            {
                return true;
            }
            RegisterSettings();
            foreach (ModSetting setting in overlaySettings)
            {
                if (setting.On)
                {
                    return true;
                }
            }
            return false;
        }
    }

    /// <summary>
    /// The debug scenario the TEST button should load, or <paramref name="fallback"/> when the
    /// setting names nothing recognisable.
    ///
    /// Takes and returns a NAME rather than the enum so that the Absent stub beside the framework
    /// does not have to know the type - and so that a scenario the studio adds or removes later
    /// changes nothing here. The caller parses it.
    /// </summary>
    public static string TestScenarioName(string fallback)
    {
        RegisterSettings();
        string wanted = (TestScenario.Value ?? string.Empty).Trim();
        if (wanted.Length == 0)
        {
            return fallback;
        }
        if (Enum.TryParse(typeof(PlaceGameEntities.DebugScenarios), wanted, ignoreCase: true, out object _))
        {
            return wanted;
        }
        ReportValidScenarioNames(wanted);
        return fallback;
    }

    /// <summary>
    /// Writes every valid scenario name to Errors.txt, once, when the setting names one that is
    /// not among them. A list of 94 in a tooltip would be unreadable and a mistyped name with no
    /// explanation is how someone concludes the feature does not work.
    /// </summary>
    private static void ReportValidScenarioNames(string wanted)
    {
        if (reportedBadScenario)
        {
            return;
        }
        reportedBadScenario = true;

        var sb = new StringBuilder();
        sb.Append("debug.testScenario is set to '").Append(wanted)
          .Append("', which is not one of the game's debug scenarios. Using ")
          .Append(StudioTestScenario).Append(" instead.").AppendLine().AppendLine();
        sb.Append("The names it accepts:").AppendLine();
        foreach (string name in Enum.GetNames(typeof(PlaceGameEntities.DebugScenarios)))
        {
            sb.Append("    ").Append(name).AppendLine();
        }
        GameStateManagement.UnclaimedWorld.LogError(sb.ToString(), "Unknown debug scenario");
    }

    /// <summary>
    /// Pushes the overlay switches into Kensei.Dev.Options, which is where the game reads them.
    ///
    /// Called every client update. It compares each switch against what the game will actually
    /// read and writes only the ones that differ - eleven dictionary lookups a frame.
    ///
    /// It used to compare against a cached copy of what it had last written, and that was wrong:
    /// <c>Client.BeginRun</c> calls <c>Kensei.Dev.Manager.Initialise</c> -> <c>Options.Initialise</c>
    /// on every new game and every load - including the reload that follows a save
    /// (<c>SaveLoadMessageBox.LoadGameDirectlyInGame</c>) - and that replaces the option
    /// dictionary with an empty one. The cache still matched the settings, so nothing was pushed
    /// back, and the overlays read as off until a switch was toggled.
    /// </summary>
    public static void ApplyOverlays()
    {
        RegisterSettings();

        for (int i = 0; i < overlaySettings.Length; i++)
        {
            bool on = overlaySettings[i].On;
            if (Kensei.Dev.Options.GetOption(Overlays[i][1]) != on)
            {
                Kensei.Dev.Options.SetOption(Overlays[i][1], on);
            }
        }
    }
}
