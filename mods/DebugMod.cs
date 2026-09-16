using System;
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

    private static string appliedSignature;

    private static bool reportedBadScenario;

    /// <summary>Which of the 94 prepared situations the main menu's TEST button loads.</summary>
    public static ModSetting TestScenario =>
        testScenario ?? (testScenario = ModSettings.Text(
            ModId, "testScenario", "TEST BUTTON SCENARIO", StudioTestScenario,
            toolTip: "The name of one of the debug scenarios in PlaceGameEntities - NeedsTest, " +
                     "FightTest, CookingTest, FindPreyTest, StorageTest, RobotTest and ninety " +
                     "more. An unrecognised name writes the full list to Errors.txt and uses " +
                     "the studio's default."));

    public static void RegisterSettings()
    {
        _ = TestScenario;
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

    /// <summary>Whether anything here is doing something.</summary>
    public static bool Enabled
    {
        get
        {
            if (!string.Equals(TestScenario.Value, StudioTestScenario, StringComparison.Ordinal))
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
    /// Called every client update and does nothing on almost all of them: the settings have no
    /// change notification, so the cheapest correct thing is to notice when the composite value
    /// differs from what was last written. Eleven dictionary writes on the frame a switch moves,
    /// and a string comparison on every other.
    /// </summary>
    public static void ApplyOverlays()
    {
        RegisterSettings();

        var signature = new StringBuilder(overlaySettings.Length);
        foreach (ModSetting setting in overlaySettings)
        {
            signature.Append(setting.On ? '1' : '0');
        }
        string now = signature.ToString();
        if (string.Equals(now, appliedSignature, StringComparison.Ordinal))
        {
            return;
        }
        appliedSignature = now;

        for (int i = 0; i < overlaySettings.Length; i++)
        {
            Kensei.Dev.Options.SetOption(Overlays[i][1], overlaySettings[i].On);
        }
    }
}
