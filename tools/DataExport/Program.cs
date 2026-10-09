using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using UWGame.SimSide;
using UWGame.SimSide.AllGameData;

namespace UW.Tools.DataExport;

internal static partial class Program
{
    /// <summary>--traces: print a stack trace for each table that fails, not only its name.</summary>
    private static bool printTraces;

    private static int Main(string[] args)
    {
        string target = args.FirstOrDefault(a => !a.StartsWith("--", StringComparison.Ordinal));
        bool readBack = args.Contains("--read-back");
        bool settingsSelfTest = args.Contains("--settings-selftest");
        bool disassemblyReport = args.Contains("--disassembly");
        bool scenarioReport = args.Contains("--scenarios");
        bool randomSelfTest = args.Contains("--random-selftest");
        string userScenario = args.FirstOrDefault(a => a.StartsWith("--user-scenario=", StringComparison.Ordinal))
            ?.Substring("--user-scenario=".Length);
        bool faunaSelfTest = args.Contains("--fauna-selftest");
        string fishSelfTest = args.Contains("--fish-selftest") ? "on" : args.Contains("--fish-selftest=off") ? "off" : null;
        // Resolved now, before the working directory becomes the installation.
        string saveSelfTest = args.FirstOrDefault(a => a.StartsWith("--save-selftest=", StringComparison.Ordinal))
            ?.Substring("--save-selftest=".Length);
        if (saveSelfTest != null) saveSelfTest = Path.GetFullPath(saveSelfTest);
        printTraces = args.Contains("--traces");

        // The bundled Unhidden Mod is on by default and its content hooks run inside the data
        // load, so an export made without this reflects the MODDED tables. That is usually what
        // you want - it is what the game will run - but exporting the stock tables has to be
        // possible too, both for diffing the mod's effect and for a data modder who wants the
        // studio's own numbers as a starting point. Mirrors UnclaimedWorld.exe -nomods.
        if (args.Contains("--nomods") || args.Contains("-nomods"))
        {
            UWGame.Mods.UnhiddenMod.Disable();
            UWGame.Mods.ModLoader.Disable();
            UWGame.Mods.ModSettings.UseStockValues();
        }
        Console.WriteLine("==> Unhidden Mod: " + (UWGame.Mods.UnhiddenMod.Enabled ? "ON" : "off"));

        if (target == null)
        {
            Console.Error.WriteLine("usage: dataexport <game-dir> [--read-back] [--nomods]");
            Console.Error.WriteLine();
            Console.Error.WriteLine("  Runs the game's own data export against <game-dir>, writing");
            Console.Error.WriteLine("  data/BaseData/*.xml and reading each file straight back - the same thing");
            Console.Error.WriteLine("  UnclaimedWorld.exe --export-data does, but with no window.");
            Console.Error.WriteLine();
            Console.Error.WriteLine("  --read-back  afterwards, load again in Read mode (no built-in defaults),");
            Console.Error.WriteLine("               which is what UnclaimedWorld.exe --data-from-xml does. This is");
            Console.Error.WriteLine("               the check that the exported XML is actually sufficient to run");
            Console.Error.WriteLine("               from, rather than merely well-formed.");
            Console.Error.WriteLine();
            Console.Error.WriteLine("  --nomods     export the stock tables, with the bundled Unhidden Mod off.");
            Console.Error.WriteLine("               Without this the export includes the mod's content.");
            Console.Error.WriteLine();
            Console.Error.WriteLine("  --settings-selftest");
            Console.Error.WriteLine("               write, re-read and verify user/ModSettings.xml in <game-dir>,");
            Console.Error.WriteLine("               and load nothing else. Used by build/80-verify-modloader.sh.");
            Console.Error.WriteLine();
            Console.Error.WriteLine("  --disassembly");
            Console.Error.WriteLine("               load WITHOUT exporting and list the disassembly recipes the");
            Console.Error.WriteLine("               DisassemblyMod generated, one per line, with what each gives back.");
            Console.Error.WriteLine();
            Console.Error.WriteLine("  --scenarios");
            Console.Error.WriteLine("               write the nine built-in scenarios to data/Scenarios/<name>/ and");
            Console.Error.WriteLine("               report each one separately. The game writes them too, but in one");
            Console.Error.WriteLine("               unguarded loop, so the first failure hides every scenario after it.");
            Console.Error.WriteLine();
            Console.Error.WriteLine("  --user-scenario=<folder>");
            Console.Error.WriteLine("               load user/Scenarios/<folder> the way NEW GAME does - base tables,");
            Console.Error.WriteLine("               then the scenario's own through UserDataLoader - and check that every");
            Console.Error.WriteLine("               key its scenarioData.xml names resolves, then run the validation pass.");
            Console.Error.WriteLine();
            Console.Error.WriteLine("  --fish-selftest[=off]");
            Console.Error.WriteLine("               with FishStockMod on: check fishTrapSpawningLoop gained its stock");
            Console.Error.WriteLine("               condition and action, and each trap's best-for-its-place rate. With");
            Console.Error.WriteLine("               =off: check the event is exactly the studio's.");
            Console.Error.WriteLine();
            Console.Error.WriteLine("  --fauna-selftest");
            Console.Error.WriteLine("               check that every species DangerousFaunaMod names is a creature in the");
            Console.Error.WriteLine("               table, and that a setting reaches that species and nothing else.");
            Console.Error.WriteLine();
            Console.Error.WriteLine("  --random-selftest");
            Console.Error.WriteLine("               check that a seeded random stream can be resumed by replaying");
            Console.Error.WriteLine("               draws, which is what loading a save now does. Loads nothing.");
            Console.Error.WriteLine();
            Console.Error.WriteLine("  --save-selftest=<file.sav>");
            Console.Error.WriteLine("               read a saved game with no window - its mod switches, its scenario's");
            Console.Error.WriteLine("               tables, then the whole simulation - and check the creatures a Port");
            Console.Error.WriteLine("               delivered: none waits as a passenger with nothing to ride, each");
            Console.Error.WriteLine("               belongs to the expedition that owns it, none is in a Port's stock.");
            Console.Error.WriteLine();
            Console.Error.WriteLine("  --discomfort-selftest");
            Console.Error.WriteLine("               DiscomfortMod against the tables: what stinks, which workplaces are");
            Console.Error.WriteLine("               loud, the comfort lost, and the once-a-day measurement.");
            Console.Error.WriteLine();
            Console.Error.WriteLine("  --strings-literals=FILE --strings-out=FILE");
            Console.Error.WriteLine("               write English (US).xml: the literals in FILE (from Locale.Text calls),");
            Console.Error.WriteLine("               every mod setting's label and tooltip, every item's name and description.");
            Console.Error.WriteLine("               Run by tools/build/37-make-strings.sh.");
            Console.Error.WriteLine();
            Console.Error.WriteLine("  --mapedit-selftest");
            Console.Error.WriteLine("               the Map Editor's terrain height edit on a small map built in memory,");
            Console.Error.WriteLine("               at the map's corner and away from it: it completes, and the coast");
            Console.Error.WriteLine("               keeps its subtiles in the same pattern. Loads nothing.");
            Console.Error.WriteLine();
            Console.Error.WriteLine("  --traces     print a stack trace for every table that fails to load or export.");
            return 2;
        }

        if (!Directory.Exists(target))
        {
            Console.Error.WriteLine($"FATAL: no such directory: {target}");
            return 1;
        }

        if (randomSelfTest)
        {
            return RandomSelfTest();
        }

        if (args.Contains("--mapedit-selftest"))
        {
            return MapEditSelfTest();
        }

        if (settingsSelfTest)
        {
            Directory.SetCurrentDirectory(target);
            return SettingsSelfTest();
        }

        // Config.GetDataFolderPath returns paths relative to the working directory ("data/BaseData"),
        // so the working directory IS the choice of which installation to write into. Set it
        // explicitly and report it, rather than depending on how the tool was launched.
        Directory.SetCurrentDirectory(target);
        Console.WriteLine("==> target: " + Directory.GetCurrentDirectory());

        // Load third-party mods from the target's user/Mods, exactly as the game does, and after
        // the working directory is set because that is what user/Mods resolves against.
        //
        // This is the whole reason a data mod can be developed without launching: a Harmony patch
        // on ItemLoader.Init or ProcessLoader.InitProcessTypes shows up in the exported XML, so
        // "did my patch apply, and what did it produce" is answerable in a second from a console.
        // Mod configuration, from the target's user/ModSettings.xml - after the working directory
        // is set, because that is what it resolves against. The switches shape the tables, so an
        // export made without reading them would describe a game nobody is running.
        UWGame.Mods.ModSettings.Load((message, title) => Console.WriteLine("    " + title + ": " + message));
        UWGame.Mods.PortSettings.RegisterSettings();
        UWGame.Mods.MapEdgeMod.RegisterSettings();
        UWGame.Mods.HealingMod.RegisterSettings();
        UWGame.Mods.SelfPreservationMod.RegisterSettings();
        UWGame.Mods.MagnificationMod.RegisterSettings();
        UWGame.Mods.BalancedDietMod.RegisterSettings();
        UWGame.Mods.DangerousFaunaMod.RegisterSettings();
        UWGame.Mods.FishStockMod.RegisterSettings();
        UWGame.Mods.HudMod.RegisterSettings();
        UWGame.Mods.RegrowthMod.RegisterSettings();
        UWGame.Mods.PestMod.RegisterSettings();
        UWGame.Mods.BirdHopMod.RegisterSettings();
        UWGame.Mods.HomeRaidMod.RegisterSettings();
        UWGame.Mods.ReserveMod.RegisterSettings();
        UWGame.Mods.TradeMod.RegisterSettings();
        UWGame.Mods.SafeSleepMod.RegisterSettings();
        UWGame.Mods.GatherOnDemandMod.RegisterSettings();
        UWGame.Mods.DiscomfortMod.RegisterSettings();
        UWGame.Mods.HuntingMod.RegisterSettings();
        UWGame.Mods.KeybindMod.RegisterSettings();
        UWGame.Mods.PreyFearMod.RegisterSettings();
        UWGame.Mods.ToolCareMod.RegisterSettings();
        UWGame.Mods.OwnershipMod.RegisterSettings();
        UWGame.Mods.DisassemblyMod.RegisterSettings();
        UWGame.Mods.DebugMod.RegisterSettings();
        UWGame.Mods.StateDumpMod.RegisterSettings();
        UWGame.Mods.AgentMod.RegisterSettings();
        if (UWGame.Mods.UnhiddenMod.Enabled)
        {
            UWGame.Mods.UnhiddenMod.RegisterSettings();
        }
        foreach (UWGame.Mods.ModSetting setting in UWGame.Mods.ModSettings.All)
        {
            if (!setting.IsDefault)
            {
                Console.WriteLine("==> setting: " + setting.Id + " = " + setting.Value +
                                  " (default " + setting.DefaultValue + ")");
            }
        }

        UWGame.Mods.ModLoader.LoadAll((message, title) => Console.WriteLine("    " + title + ": " + message));
        if (UWGame.Mods.ModLoader.Loaded.Count > 0 || UWGame.Mods.ModLoader.Failed.Count > 0)
        {
            Console.WriteLine($"==> mods: {UWGame.Mods.ModLoader.Loaded.Count} loaded, " +
                              $"{UWGame.Mods.ModLoader.Failed.Count} failed");
        }

        PrepareLookups();

        string stringsLiterals = args.FirstOrDefault(a => a.StartsWith("--strings-literals=", StringComparison.Ordinal))?.Substring("--strings-literals=".Length);
        string stringsOut = args.FirstOrDefault(a => a.StartsWith("--strings-out=", StringComparison.Ordinal))?.Substring("--strings-out=".Length);
        string stringsCounts = args.FirstOrDefault(a => a.StartsWith("--strings-counts=", StringComparison.Ordinal))?.Substring("--strings-counts=".Length);
        if (stringsLiterals != null && stringsOut != null)
        {
            return WriteStrings(stringsLiterals, stringsCounts, stringsOut);
        }

        if (disassemblyReport)
        {
            // NoSerialize is the mode the GAME loads in: the tables are built by the loaders and
            // handed straight to GameData, with no XML in the middle. That matters here because
            // the generated disassembly is computed from the ENTITY table, and entityTypes.xml is
            // one of the 13 that cannot serialize - in an exporting run the entity table dies
            // half-built and the report would describe a game nobody is running.
            int loadRc = Run(Sim.SerializeMode.NoSerialize, "load (no export), for the disassembly report");
            if (!ValidateDataComplete())
            {
                return 1;
            }
            DisassemblyReport();
            return loadRc;
        }

        if (userScenario != null)
        {
            return UserScenarioCheck(userScenario);
        }

        if (faunaSelfTest)
        {
            return FaunaSelfTest();
        }

        if (fishSelfTest != null)
        {
            return FishSelfTest(fishSelfTest == "on");
        }

        if (args.Contains("--diet-selftest"))
        {
            return DietSelfTest();
        }

        if (args.Contains("--regrowth-selftest"))
        {
            return RegrowthSelfTest();
        }

        if (args.Contains("--discomfort-selftest"))
        {
            return DiscomfortSelfTest();
        }

        if (args.Contains("--homeraid-selftest"))
        {
            return HomeRaidSelfTest();
        }

        if (args.Contains("--pest-selftest"))
        {
            return PestSelfTest();
        }

        if (args.Contains("--reserve-selftest"))
        {
            return ReserveSelfTest();
        }

        if (args.Contains("--trade-selftest"))
        {
            return TradeSelfTest();
        }

        if (args.Contains("--safesleep-selftest"))
        {
            return SafeSleepSelfTest();
        }

        if (args.Contains("--hunting-selftest"))
        {
            return HuntingSelfTest();
        }

        if (args.Contains("--selfpreservation-selftest"))
        {
            return SelfPreservationSelfTest();
        }

        if (args.Contains("--savetype-selftest"))
        {
            return SaveTypeSelfTest();
        }

        if (args.Contains("--toolcare-selftest"))
        {
            return ToolCareSelfTest();
        }

        if (args.Contains("--ownership-selftest"))
        {
            return OwnershipSelfTest();
        }

        if (args.Contains("--locale-selftest"))
        {
            return LocaleSelfTest();
        }

        if (args.Contains("--hud-selftest"))
        {
            return HudSelfTest();
        }

        if (args.Contains("--freshfood-selftest"))
        {
            return FreshFoodSelfTest();
        }

        if (args.Contains("--gatherondemand-selftest"))
        {
            return GatherOnDemandSelfTest();
        }

        if (saveSelfTest != null)
        {
            return SaveSelfTest(saveSelfTest);
        }

        // Procedural maps (MapGenMod). Learns from --maps-from=DIR, default this game's data/Maps.
        string mapsFrom = args.FirstOrDefault(a => a.StartsWith("--maps-from=", StringComparison.Ordinal))
            ?.Substring("--maps-from=".Length) ?? Path.Combine("data", "Maps");
        string generateMap = args.FirstOrDefault(a => a.StartsWith("--generate-map=", StringComparison.Ordinal))
            ?.Substring("--generate-map=".Length);
        if (generateMap != null || args.Contains("--mapgen-selftest"))
        {
            int loaded = Run(Sim.SerializeMode.NoSerialize, "base tables, the way the game loads them");
            if (loaded != 0) return loaded;
            if (generateMap != null)
            {
                ulong seed = ulong.TryParse(args.FirstOrDefault(a => a.StartsWith("--seed=", StringComparison.Ordinal))?.Substring(7), out ulong sd) ? sd : (ulong)Environment.TickCount64;
                int size = int.TryParse(args.FirstOrDefault(a => a.StartsWith("--size=", StringComparison.Ordinal))?.Substring(7), out int sz) ? sz : 80;
                Console.WriteLine("==> generating " + UWGame.Mods.MapGenMod.GenerateMap(mapsFrom, Path.Combine("data", "Maps"), generateMap, seed, size));
                Console.WriteLine("    open it with EDIT or TEST MAP on the main menu");
                return 0;
            }
            Console.WriteLine("==> mapgen self-test");
            string work = Path.Combine(Path.GetTempPath(), "uw-mapgen-selftest-" + Environment.ProcessId);
            try
            {
                int failures = UWGame.Mods.MapGenMod.SelfTest(mapsFrom, work, Console.WriteLine);
                Console.WriteLine(failures == 0 ? "mapgen self-test OK" : $"mapgen self-test FAILED - {failures} check(s)");
                return failures == 0 ? 0 : 1;
            }
            finally
            {
                try { Directory.Delete(work, recursive: true); } catch (IOException) { } catch (UnauthorizedAccessException) { }
            }
        }

        if (scenarioReport)
        {
            // Load the way the GAME loads and only then switch modes, which is the order that
            // makes the scenario writes the ONLY thing under test: an exporting load loses 13
            // tables on the way past, and a scenario that then failed would be impossible to tell
            // apart from a scenario that failed for its own reasons.
            int loadRc = Run(Sim.SerializeMode.NoSerialize, "load (no export), for the scenario report");
            if (loadRc != 0) return loadRc;
            Sim.CurrentSerializeMode = Sim.SerializeMode.WriteAndRead;
            return ScenarioReport();
        }

        int rc = Run(Sim.SerializeMode.WriteAndRead, "export (write, then read each file back)");
        if (rc != 0) return rc;

        Report();

        if (readBack)
        {
            Console.WriteLine();
            rc = Run(Sim.SerializeMode.Read, "read-back (from the exported XML only)");
            if (rc != 0) return rc;
            // Loading the XML is not the same as a game being able to start from it: the
            // validation pass is where bad data kills the game (CLAUDE.md, "Gates, not launches").
            if (!ValidateDataComplete()) return 1;
        }

        Console.WriteLine();
        Console.WriteLine("Done.");
        return 0;
    }

    /// <summary>
    /// Round-trips user/ModSettings.xml: register, change, write, forget, read back.
    ///
    /// This exists because the WRITE side of the settings file has no other offline witness. A
    /// player's configuration surviving is the whole promise of the file, and the two ways it
    /// could quietly break - a value that does not come back, and an entry belonging to a mod
    /// that is not installed being dropped on the next save - are both invisible until someone
    /// loses their settings. Neither needs the game to be running to check.
    ///
    /// It writes into the target installation's user/ folder, which is why the verify script
    /// gives it a throwaway one.
    /// </summary>
    private static int SettingsSelfTest()
    {
        Console.WriteLine("==> settings self-test in " + Directory.GetCurrentDirectory());
        int failures = 0;
        void Check(bool ok, string what)
        {
            Console.WriteLine((ok ? "  ok    " : "  FAIL  ") + what);
            if (!ok)
            {
                failures++;
            }
        }

        string path = UWGame.Mods.ModSettings.FilePath;
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        // A file written by hand, holding an entry nothing will claim.
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path,
            "<ModSettings>" + Environment.NewLine +
            "  <Setting id=\"someOtherMod.itsSetting\" value=\"keep me\" />" + Environment.NewLine +
            "</ModSettings>" + Environment.NewLine);

        UWGame.Mods.ModSettings.Reset();
        UWGame.Mods.ModSettings.Load((m, t) => Console.WriteLine("    " + t + ": " + m));
        UWGame.Mods.PortSettings.RegisterSettings();
        UWGame.Mods.ModSetting dateFormat = UWGame.Mods.PortSettings.SaveDateFormat;

        Check(dateFormat.Value == dateFormat.DefaultValue, "an unset setting reads as its default");

        // Self-preservation changes the studio's AI rather than fixing it, so a fresh file must leave it
        // off (Kastuk, 2026-09-27).
        UWGame.Mods.SelfPreservationMod.RegisterSettings();
        Check(!UWGame.Mods.SelfPreservationMod.Enabled, "self-preservation is off on a fresh settings file");

        dateFormat.Value = "dd-MM-yyyy HH:mm";
        Check(UWGame.Mods.ModSettings.Save((m, t) => Console.WriteLine("    " + t + ": " + m)),
              "the file was written");

        UWGame.Mods.ModSettings.Reset();
        UWGame.Mods.ModSettings.Load((m, t) => Console.WriteLine("    " + t + ": " + m));
        UWGame.Mods.PortSettings.RegisterSettings();
        Check(UWGame.Mods.PortSettings.SaveDateFormat.Value == "dd-MM-yyyy HH:mm",
              "the changed value came back");

        string written = File.ReadAllText(path);
        Check(written.Contains("someOtherMod.itsSetting") && written.Contains("keep me"),
              "an entry for a mod that is not installed survived the rewrite");

        // A value outside the allowed set falls back rather than being taken.
        UWGame.Mods.PortSettings.SaveDateFormat.Value = "not a format this build offers";
        Check(UWGame.Mods.PortSettings.SaveDateFormat.Value == dateFormat.DefaultValue,
              "an unrecognised choice falls back to the default");

        // Signature and ApplySignature are what a save is stamped with and matched against.
        // Signature and ApplySignature are what a save is stamped with and matched against.
        UWGame.Mods.ModSetting sim = UWGame.Mods.ModSettings.Toggle(
            "selftest", "content", "SELF TEST CONTENT", defaultValue: true, toolTip: null,
            affectsSimulation: true);
        Check(UWGame.Mods.ModSettings.Signature().Contains("selftest.content=true"),
              "a setting that is ON is named in the signature even though it is also the default");

        // The player's own choice, which everything below has to come back to.
        sim.Value = "false";
        Check(UWGame.Mods.ModSettings.Signature() == "", "a stock configuration signs as stock");

        UWGame.Mods.ModSettings.ApplySignature("selftest.content=true");
        Check(sim.On, "opening a save applies the content it was made with");
        Check(UWGame.Mods.ModSettings.SignatureApplied, "the applied state is remembered");

        // Scoped to the session that opened the save: the main menu puts the player's own back.
        // Without this, opening one modded save would silently change what every later NEW game is
        // played with.
        UWGame.Mods.ModSettings.RestoreAfterSaveApplied();
        Check(!sim.On, "returning to the main menu restores the setting the player had");
        Check(!UWGame.Mods.ModSettings.SignatureApplied, "and stops claiming a save's settings are in force");

        sim.Value = "true";
        UWGame.Mods.ModSettings.ApplySignature("");
        Check(!sim.On, "applying an empty signature returns to stock");
        UWGame.Mods.ModSettings.RestoreAfterSaveApplied();

        // Explain() is what the MODDED tooltip colours by. Three states, and the middle one is the
        // reason there are three: "you switched it off" is a checkbox away, "you do not have it"
        // is not, and telling a player the second when you mean the first sends them looking
        // through the options menu for something that is not in it.
        var explained = UWGame.Mods.ModSettings.Explain(
            "selftest.content=true; selftest.other=true; mod:NotInstalledMod");
        Check(explained.Count == 3, "a signature explains one line per thing it names");
        Check(explained[0].Value == UWGame.Mods.ModContentState.Present,
              "a setting this session has is green");
        Check(explained[2].Value == UWGame.Mods.ModContentState.Missing,
              "a mod this session does not have is grey");

        sim.Value = "false";
        explained = UWGame.Mods.ModSettings.Explain("selftest.content=true");
        Check(explained[0].Value == UWGame.Mods.ModContentState.Disabled,
              "a setting this build knows but has switched off is red, not grey");
        Check(UWGame.Mods.ModSettings.Explain("selftest.notregistered=true")[0].Value
                  == UWGame.Mods.ModContentState.Missing,
              "a setting from a mod that is not installed is grey, not red");

        // STABLE / TESTING: confirmed-in-play settings are listed in ModSettings; everything else tests.
        Check(UWGame.Mods.ModSettings.IsStable(UWGame.Mods.PortSettings.SaveDateFormat) && !UWGame.Mods.ModSettings.IsStable(sim),
              "SAVE DATE FORMAT is STABLE, a setting nobody confirmed is TESTING");
        Check(File.ReadAllText(path).Contains("TESTING") && File.ReadAllText(path).Contains("STABLE"),
              "ModSettings.xml notes STABLE or TESTING beside the entries");

        // A key setting (KeybindMod's KEYS section, HudMod's reveal key) holds a Keys name.
        UWGame.Mods.ModSetting keySetting = UWGame.Mods.ModSettings.Key(
            "selftest", "key", "SELF TEST KEY", Microsoft.Xna.Framework.Input.Keys.LeftAlt);
        Check(keySetting.KeyValue == Microsoft.Xna.Framework.Input.Keys.LeftAlt, "a key setting reads as its default key");
        keySetting.Value = "o";
        Check(keySetting.Value == "O" && keySetting.KeyValue == Microsoft.Xna.Framework.Input.Keys.O,
              "a key typed in any case is stored the way the enum spells it");
        keySetting.Value = "NotAKey";
        Check(keySetting.Value == "LeftAlt", "a name that is no key falls back to the default");
        if (UWGame.Mods.KeybindMod.Enabled)
        {
            var vanilla = UWGame.Mods.KeybindMod.VanillaFields();
            Check(vanilla.Count >= 12 && vanilla.All(v => !string.IsNullOrEmpty(v.Label)),
                  $"the KEYS section finds the studio's {vanilla.Count} bindings on Options, all labelled");
            Check(UWGame.Mods.KeybindMod.DisplayName("D1") == "1" && UWGame.Mods.KeybindMod.DisplayName("LeftAlt") == "LEFT ALT"
                  && UWGame.Mods.KeybindMod.DisplayName("PageUp") == "PAGE UP", "keys display as 1, LEFT ALT, PAGE UP");
            Check(!UWGame.Mods.KeybindMod.Bindable.Contains(Microsoft.Xna.Framework.Input.Keys.Escape),
                  "Escape cancels a rebinding, so it is not offered as a key");
        }

        // -nomods (ModSettings.StockOnly). It used to switch off only the Unhidden Mod and the
        // loader, and every other mod kept running from the file (Kastuk, 2026-10-05). Now each
        // mod's setting reads as stock for the session and the file keeps the player's own values.
        File.WriteAllText(path,
            "<ModSettings>" + Environment.NewLine +
            "  <Setting id=\"selftest.wanted\" value=\"true\" />" + Environment.NewLine +
            "  <Setting id=\"selftest.pick\" value=\"b\" />" + Environment.NewLine +
            "  <Setting id=\"port.selftestFormat\" value=\"dd\" />" + Environment.NewLine +
            "</ModSettings>" + Environment.NewLine);
        UWGame.Mods.ModSettings.Reset();
        UWGame.Mods.ModSettings.UseStockValues();
        UWGame.Mods.ModSettings.Load((m, t) => Console.WriteLine("    " + t + ": " + m));
        // A port setting of its own: PortSettings caches the instances registered before the Reset.
        UWGame.Mods.ModSetting portFormat = UWGame.Mods.ModSettings.Choice(UWGame.Mods.PortSettings.ModId, "selftestFormat", "FORMAT",
                                                                           new[] { "iso", "dd" }, "iso");
        UWGame.Mods.ModSetting wanted = UWGame.Mods.ModSettings.Toggle("selftest", "wanted", "WANTED", defaultValue: false, affectsSimulation: true);
        UWGame.Mods.ModSetting onByDefault = UWGame.Mods.ModSettings.Toggle("selftest", "byDefault", "BY DEFAULT", defaultValue: true, affectsSimulation: true);
        UWGame.Mods.ModSetting pick = UWGame.Mods.ModSettings.Choice("selftest", "pick", "PICK", new[] { "a", "b" }, "b",
                                                                     affectsSimulation: true, stockValue: "a");
        Check(!wanted.On, "-nomods: a mod switched on in the file reads as off");
        Check(!onByDefault.On, "-nomods: a mod on by default reads as off");
        Check(pick.Value == "a", "-nomods: a choice reads as its stock value, not the file's or the default");
        Check(portFormat.Value == "dd",
              "-nomods: the port's own settings keep the file's values - they are not a mod");
        Check(UWGame.Mods.ModSettings.Signature() == "", "-nomods: the session signs as stock");
        wanted.Value = "false";
        UWGame.Mods.ModSettings.Save((m, t) => Console.WriteLine("    " + t + ": " + m));
        written = File.ReadAllText(path);
        Check(written.Contains("id=\"selftest.wanted\" value=\"true\"") && written.Contains("id=\"selftest.pick\" value=\"b\""),
              "-nomods: saving the options keeps the player's own mod values in the file");
        Check(!written.Contains("selftest.byDefault"),
              "-nomods: a mod at its default is not written as off");

        UWGame.Mods.ModSettings.Reset();
        UWGame.Mods.ModSettings.Load((m, t) => Console.WriteLine("    " + t + ": " + m));
        wanted = UWGame.Mods.ModSettings.Toggle("selftest", "wanted", "WANTED", defaultValue: false, affectsSimulation: true);
        onByDefault = UWGame.Mods.ModSettings.Toggle("selftest", "byDefault", "BY DEFAULT", defaultValue: true, affectsSimulation: true);
        Check(wanted.On && onByDefault.On, "without -nomods the next start has the player's mods back");

        Console.WriteLine();
        if (failures == 0)
        {
            Console.WriteLine("settings self-test OK");
            return 0;
        }
        Console.WriteLine($"settings self-test FAILED - {failures} check(s)");
        return 1;
    }

    /// <summary>
    /// Creates the ID lookup collections the DATA LOADERS use, which a game gets from
    /// <c>Sim.CreateLookupCollections</c> and this tool has no Sim to get.
    ///
    /// Without this, StructureLoader.Init dies on its first gathering site: the
    /// <c>GatheringSiteType</c> constructor calls <c>AddToLookup</c>, and
    /// <c>LookUp&lt;T, Id&gt;.collection</c> is null until someone calls <c>Create()</c>. The
    /// whole entity table was lost to that one NullReferenceException - which is why
    /// entityTypes.xml has never exported, and why a mod that reads the item table produced
    /// nothing here while working perfectly in the game. That asymmetry is the thing worth
    /// removing: a tool that cannot see half the data cannot gate a change to it.
    ///
    /// Only the collections the base data load actually touches are created. The rest belong to a
    /// running simulation - entities, jobs, allegiances - and this tool never makes one.
    /// </summary>
    private static void PrepareLookups()
    {
        UWGame.SimSide.GatheringSites.GatheringSiteType.CreateLookupCollection();
        UWGame.SimSide.AI.Needs.NeedType.CreateLookupCollection();
        UWGame.SimSide.Processes.ToolTypeCombination.CreateLookupCollection();
        UWGame.SimSide.Entities.ReplenishActionType.CreateLookupCollection();
    }

    /// <summary>
    /// Runs the validation pass the GAME runs after the tables are built -
    /// <c>GameData.PostDataCompleteInitialize</c>, which every data type's
    /// <c>PostDataCompleteValidate</c> hangs off.
    ///
    /// WHY THIS IS HERE, and it is the whole reason: building the tables is not the same as
    /// surviving them. The first version of the disassembly mod built 21 perfectly good recipes
    /// and then took the game down on the next screen, in
    /// <c>ProcessType.PostDataCompleteValidate</c>, because the validator walks
    /// <c>Inputs[0].EntityType.Parts</c> for a salvage process and every item the studio gave a
    /// disassembly to happens to declare its <c>PartKeys</c>. A tool that stopped at "the tables
    /// built" reported success on a build that could not load a save. This call is where the
    /// difference shows up, and it costs a few hundred milliseconds.
    ///
    /// Returns false if it throws, because a crash here is a crash in the player's game.
    /// </summary>
    private static bool ValidateDataComplete()
    {
        Console.WriteLine("==> validating (the pass Sim.QueueGameDataAndSimInit runs)");
        try
        {
            GameData.Instance.PostDataCompleteInitialize();
            Console.WriteLine("    ok - the tables survive the game's own validation");
            return true;
        }
        catch (Exception ex)
        {
            Exception root = ex.GetBaseException();
            Console.WriteLine("    FAIL " + root.GetType().Name + ": " + root.Message);
            Console.WriteLine(root.StackTrace);
            return false;
        }
    }

    /// <summary>
    /// Checks PestMod against the tables: both pests are creatures, every nutrient profile the
    /// real items use is classified the way the mod means (meat to rats, crops and berries to
    /// quadites, meals to neither) - a profile renamed out from under it would silently stop
    /// drawing pests - and the bonus is 0 below the threshold, grows with the pile and stops at
    /// the cap.
    /// </summary>
    private static int PestSelfTest()
    {
        int rc = Run(Sim.SerializeMode.NoSerialize, "base tables, the way the game loads them");
        if (rc != 0) return rc;
        Console.WriteLine("==> pest self-test");
        int failures = 0;
        void Check(bool ok, string what)
        {
            Console.WriteLine((ok ? "  ok    " : "  FAIL  ") + what);
            if (!ok) failures++;
        }
        // RatKey is a const: null in PestMod.Absent.cs, set in mods/PestMod.cs. Whichever build this
        // is, one of the two branches is unreachable by construction, and CS0162 says so.
#pragma warning disable CS0162
        if (UWGame.Mods.PestMod.RatKey == null)
        {
            Console.WriteLine("  the mod is not in this build - nothing to check");
            return 0;
        }
#pragma warning restore CS0162
        var types = GameData.Instance.AllEntityTypes;
        Check(types.ContainsKey(UWGame.Mods.PestMod.RatKey) && types.ContainsKey(UWGame.Mods.PestMod.QuaditeKey),
              "entity:binalRat and entity:fieldQuadite are creatures in the table");
        string ProfileOf(string key) => types.TryGetValue(key, out var t) ? t.ItemType?.FoodType?.FoodNutrientProfile?.KeyName : null;
        Check(UWGame.Mods.PestMod.Classify(ProfileOf("item:smokedStreakFin")) > 0, $"smoked streak fin ({ProfileOf("item:smokedStreakFin")}) draws rats");
        Check(UWGame.Mods.PestMod.Classify(ProfileOf("item:binalRatChunk")) > 0, $"rat meat ({ProfileOf("item:binalRatChunk")}) draws rats");
        Check(UWGame.Mods.PestMod.Classify(ProfileOf("item:fingerFruit")) < 0, $"finger fruit ({ProfileOf("item:fingerFruit")}) draws field quadites");
        Check(UWGame.Mods.PestMod.Classify(ProfileOf("item:crystalBerries")) < 0, $"crystal berries ({ProfileOf("item:crystalBerries")}) draw field quadites");
        int unclassified = types.Values.Count(t => t.ItemType?.FoodType?.FoodNutrientProfile != null
            && UWGame.Mods.PestMod.Classify(t.ItemType.FoodType.FoodNutrientProfile.KeyName) == 0);
        Console.WriteLine($"  info  {unclassified} food types draw neither (meals, drinks, stimulants)");
        float threshold = UWGame.Mods.PestMod.Threshold("normal");
        Check(threshold == 10f && UWGame.Mods.PestMod.ExtraFor(9.9f, threshold) == 0, "normal: nothing below 10 bulk");
        Check(UWGame.Mods.PestMod.ExtraFor(10f, threshold) == 1 && UWGame.Mods.PestMod.ExtraFor(20f, threshold) == 3, "one at 10 bulk, three at 20");
        Check(UWGame.Mods.PestMod.ExtraFor(1000f, threshold) == UWGame.Mods.PestMod.MaxExtra, $"capped at {UWGame.Mods.PestMod.MaxExtra}");
        Console.WriteLine(failures == 0 ? "pest self-test OK" : $"pest self-test FAILED - {failures} check(s)");
        return failures == 0 ? 0 : 1;
    }

    /// <summary>
    /// FreshFoodMod's urgency and weights, on Kastuk's case: clamwich soup with 3 days left, a little
    /// further away, against pickled fish with 60 days left, the same nutrients. With the studio's
    /// weights the fish wins (its 6-day squared freshness gives the soup 0.025); with the mod's the
    /// soup must. No tables needed.
    /// </summary>
    /// <summary>
    /// GatherOnDemandMod's padlock at 0 (GatherOnDemandMod.ZoneGathersAt, read by
    /// GatherResourcesWindow.btOk_Click): with the mod off the studio's rule holds - 0 is no order -
    /// and with it on 0 keeps the zone gathering, which is the whole "only on demand" setting.
    ///
    /// Then the demand a production order makes (Kastuk, "auto harvesting": a clamwich soup order and
    /// a clam gather order at 0 - "no one is going to collect needful clams"). The studio creates no
    /// production job while an input is missing (JobManager.GetJobManagerProductionProcessesThatCanProduce),
    /// so no hauling job asks for the input either, and the gather order at 0 saw nothing to gather.
    /// Checked on a bare expedition with the real tables: the gather order's amount to produce, by
    /// the studio's own JobManager.GetAmountToProduce over GatherOnDemandMod.StockAfterDemand - the
    /// number JobManager.CreateProcessJobsToMatchOutput turns into gather jobs in the order's zones.
    /// </summary>
    private static int GatherOnDemandSelfTest()
    {
        Console.WriteLine("==> gatherondemand self-test");
        UWGame.Mods.GatherOnDemandMod.RegisterSettings();
        var setting = UWGame.Mods.ModSettings.Find("gatherondemand.enabled");
        if (setting == null)
        {
            Console.WriteLine("  the mod is not in this build - nothing to check");
            return 0;
        }
        int failures = 0;
        void Check(bool ok, string what)
        {
            Console.WriteLine((ok ? "  ok    " : "  FAIL  ") + what);
            if (!ok) failures++;
        }
        setting.Value = "false";
        Check(!UWGame.Mods.GatherOnDemandMod.ZoneGathersAt(0), "mod off: a padlock at 0 is no order, as the studio made it");
        Check(UWGame.Mods.GatherOnDemandMod.ZoneGathersAt(5), "mod off: a padlock at 5 gathers");
        setting.Value = "true";
        Check(UWGame.Mods.GatherOnDemandMod.ZoneGathersAt(0), "mod on: a padlock at 0 stays in the zone (Kastuk: the order was not kept)");
        Check(UWGame.Mods.GatherOnDemandMod.ZoneGathersAt(5), "mod on: a padlock at 5 gathers");

        // Production orders as demand. Needs the tables, and the production graph the validation pass builds.
        // A Sim, as SaveSelfTest makes one: TileResourceType reads The.Sim.Mode while the tables
        // build, which the post-load pass below needs.
        UWGame.The.Sim = new Sim { Mode = Sim.EngineMode.Game };
        typeof(Sim).GetMethod("CreateLookupCollections", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(UWGame.The.Sim, null);
        int rc = Run(Sim.SerializeMode.NoSerialize, "base tables, the way the game loads them");
        if (rc != 0) return rc;
        if (!ValidateDataComplete()) return 1;
        // The recipe index the job manager reads (ProcessYieldsThisOutput, ProcessesUsingThisInput),
        // and each fire's list of fuels (RequiresFuelType.FuelEntityTypes), are built by
        // GameData.Initialize on the loading screen, which this tool never reaches. Run its
        // content-free part, the studio's own steps in its order (InitializeWithoutContent).
        if (GameData.Instance.ProcessYieldsThisOutput.Count == 0 && !InitializeWithoutContent()) return 1;
        Console.WriteLine($"  info  recipe index: {GameData.Instance.ProcessYieldsThisOutput.Count} items made, {GameData.Instance.ProcessesUsingThisInput.Count} used");
        var types = GameData.Instance.AllEntityTypes;
        var processes = GameData.Instance.AllProcessTypes;
        bool tablesOk = types.TryGetValue("item:clamwich", out var clams) & types.TryGetValue("item:clamwichSoup", out var soup)
            & types.TryGetValue("item:spoakBranches", out var branches) & types.TryGetValue("item:spoakBranchesTrimmed", out var trimmed)
            & types.TryGetValue("item:bedFrame", out var bedFrame)
            & processes.TryGetValue("makeClamwichSoup", out var cookSoup) & processes.TryGetValue("trimSpoakBranches", out var trim)
            & processes.TryGetValue("makeBedFrame", out var makeBed);
        Check(tablesOk, "clams, clamwich soup, spoak branches, trimmed branches, bed frame and their recipes are in the tables");
        if (!tablesOk)
        {
            setting.Value = setting.DefaultValue;
            Console.WriteLine($"gatherondemand self-test FAILED - {failures} check(s)");
            return 1;
        }
        // Units of 'input' that 'amount' of 'output' takes by this recipe: whole batches, as jobs are made.
        static int Needs(UWGame.SimSide.Processes.ProcessType p, UWGame.SimSide.Entities.EntityType output, UWGame.SimSide.Entities.EntityType input, int amount)
        {
            int perBatch = p.GetOutputAmount(output) ?? 1;
            return (amount + perBatch - 1) / perBatch * p.InputsByType[input].Amount.NoOfItems.Value;
        }
        // A bare expedition: nothing in stock, no jobs, no zones. Only the orders differ per case.
        static UWGame.SimSide.Entities.EntityGroup NewOwner() => new UWGame.SimSide.Entities.EntityGroup
        {
            Parent = new UWGame.SimSide.Expeditions.Expedition(),
            ProductionOrders = new UWGame.SimSide.Expeditions.ProductionOrders(),
        };
        // What the gather order for 'type' asks JobManager to make, with nothing in stock or under way.
        static int ToGather(UWGame.SimSide.Entities.EntityGroup owner, UWGame.SimSide.Entities.EntityType type) =>
            UWGame.SimSide.Jobs.JobManager.GetAmountToProduce(owner.ProductionOrders.Orders[type],
                UWGame.Mods.GatherOnDemandMod.StockAfterDemand(owner, type, 0), 0);

        var soupOwner = NewOwner();
        soupOwner.ProductionOrders.SetStandingOrder(soup, 6);
        soupOwner.ProductionOrders.SetStandingOrder(clams, 0);
        int soupClams = Needs(cookSoup, soup, clams, 6);
        var branchOwner = NewOwner();
        branchOwner.ProductionOrders.SetStandingOrder(trimmed, 4);
        branchOwner.ProductionOrders.SetStandingOrder(branches, 0);
        int trimBranches = Needs(trim, trimmed, branches, 4);
        var directOwner = NewOwner();
        directOwner.ProductionOrders.SetDirectOrder(soup, 2);
        directOwner.ProductionOrders.SetStandingOrder(clams, 0);
        int directClams = 2 * cookSoup.InputsByType[clams].Amount.NoOfItems.Value;
        var idleOwner = NewOwner();
        idleOwner.ProductionOrders.SetStandingOrder(soup, 0);
        idleOwner.ProductionOrders.SetStandingOrder(clams, 0);
        var chainOwner = NewOwner();
        chainOwner.ProductionOrders.SetStandingOrder(bedFrame, 1);
        chainOwner.ProductionOrders.SetStandingOrder(trimmed, 0);
        chainOwner.ProductionOrders.SetStandingOrder(branches, 0);
        int bedBranches = Needs(trim, trimmed, branches, Needs(makeBed, bedFrame, trimmed, 1));

        setting.Value = "false";
        Check(ToGather(soupOwner, clams) == 0 && !UWGame.Mods.GatherOnDemandMod.HasDemand(soupOwner, clams),
              "mod off: a soup order does not move a clam order at 0, as the studio made it");
        Check(ToGather(branchOwner, branches) == 0, "mod off: a trimmed-branches order does not move a branch order at 0");
        setting.Value = "true";
        Check(ToGather(soupOwner, clams) == soupClams && UWGame.Mods.GatherOnDemandMod.HasDemand(soupOwner, clams),
              $"mod on: soup kept at 6 with no clams - the clam order at 0 gathers {soupClams} (got {ToGather(soupOwner, clams)}; Kastuk's clamwich soup)");
        Check(ToGather(branchOwner, branches) == trimBranches && UWGame.Mods.GatherOnDemandMod.HasDemand(branchOwner, branches),
              $"mod on: trimmed branches kept at 4 - the branch order at 0 gathers {trimBranches} (got {ToGather(branchOwner, branches)}; Kastuk's trimmed branches)");
        Check(ToGather(directOwner, clams) == directClams,
              $"mod on: a direct order for 2 soup jobs gathers the clams for 2 batches, {directClams} (got {ToGather(directOwner, clams)})");
        Check(ToGather(idleOwner, clams) == 0 && !UWGame.Mods.GatherOnDemandMod.HasDemand(idleOwner, clams),
              "mod on: a soup order at 0 that nothing asks for gathers no clams");
        Check(ToGather(chainOwner, branches) == bedBranches,
              $"mod on: a bed frame order pulls trimmed branches at 0, which pull {bedBranches} branches at 0 (got {ToGather(chainOwner, branches)})");

        // Fuel (Kastuk, 2026-10-04: "not works ... for gathering the firewood, when cooking tasks
        // need fuel"). Fuel is burned by a recipe's tool, not taken as an input, so nothing asked
        // for it. Any item whose first managed recipe needs a fire that burns firewood will do.
        Check(types.TryGetValue("item:firewood", out var firewood), "item:firewood is in the tables");
        UWGame.SimSide.Entities.EntityType cooked = null;
        foreach (var made in GameData.Instance.ProcessYieldsThisOutput)
        {
            var recipe = made.Value.FirstOrDefault(r => UWGame.SimSide.Jobs.JobManager.IsManagedProcess(r) && !r.IsGathering && r.InputsByType != null);
            bool burnsFirewood = recipe?.ProcessToolSet?.Tools != null && recipe.WorkOrTimeNeeded?.DaysNeeded > 0f
                && recipe.ProcessToolSet.Tools.Any(slot => slot.ToolsAndProductivity != null && slot.ToolsAndProductivity.Count > 0
                    && slot.ToolsAndProductivity.All(t => t.Item1?.ContainerType?.GetRequiresReplenishType()?.RequiresFuelType?.FuelEntityTypes?.Contains(firewood) == true));
            if (burnsFirewood)
            {
                cooked = made.Key;
                break;
            }
        }
        Check(cooked != null, $"an item is made at a fire that burns firewood ({cooked?.KeyName})");
        if (cooked != null && firewood != null)
        {
            var fireOwner = NewOwner();
            fireOwner.ProductionOrders.SetStandingOrder(cooked, 6);
            fireOwner.ProductionOrders.SetStandingOrder(firewood, 0);
            var idleFireOwner = NewOwner();
            idleFireOwner.ProductionOrders.SetStandingOrder(cooked, 0);
            idleFireOwner.ProductionOrders.SetStandingOrder(firewood, 0);
            setting.Value = "false";
            Check(ToGather(fireOwner, firewood) == 0, $"mod off: a {cooked.KeyName} order does not move a firewood order at 0");
            setting.Value = "true";
            int wood = ToGather(fireOwner, firewood);
            Check(wood > 0, $"mod on: {cooked.KeyName} kept at 6 with no fuel - the firewood order at 0 gathers {wood}");
            Check(ToGather(idleFireOwner, firewood) == 0, $"mod on: a {cooked.KeyName} order at 0 gathers no firewood");
        }

        // Planning ahead (Kastuk, "auto harvesting", 2026-10-05): BUILD for a structure on the
        // Attainable list before its materials are in, and STOP in a construction's priority list.
        var beforeMaterials = UWGame.Mods.ModSettings.Find("gatherondemand.beforeMaterials");
        var stopSetting = UWGame.Mods.ModSettings.Find("gatherondemand.stop");
        Check(beforeMaterials != null && stopSetting != null && beforeMaterials.DefaultValue == "false" && stopSetting.DefaultValue == "false",
              "PLACE STRUCTURES BEFORE THEIR MATERIALS ARE IN and STOP IN A CONSTRUCTION'S PRIORITY LIST exist, off by default");
        if (beforeMaterials != null && stopSetting != null)
        {
            var producable = new Dictionary<UWGame.SimSide.Processes.ProcessType, UWGame.ClientSide.Interface.Inventory.AttainableInfo>
            {
                { makeBed, new UWGame.ClientSide.Interface.Inventory.AttainableInfo(1) { IsProducable = true } },
            };
            var notAttainable = new Dictionary<UWGame.SimSide.Processes.ProcessType, UWGame.ClientSide.Interface.Inventory.AttainableInfo>
            {
                { makeBed, new UWGame.ClientSide.Interface.Inventory.AttainableInfo(-1) },
            };
            beforeMaterials.Value = "false";
            Check(!UWGame.Mods.GatherOnDemandMod.PlacesBeforeMaterials(producable, makeBed), "setting off: BUILD only with every material in stock, as the studio made it");
            beforeMaterials.Value = "true";
            Check(UWGame.Mods.GatherOnDemandMod.PlacesBeforeMaterials(producable, makeBed), "setting on: BUILD for a recipe the Attainable list marks producible");
            Check(!UWGame.Mods.GatherOnDemandMod.PlacesBeforeMaterials(notAttainable, makeBed) && !UWGame.Mods.GatherOnDemandMod.PlacesBeforeMaterials(null, makeBed),
                  "  but not for one it does not, nor with no Attainable entry");
            beforeMaterials.Value = beforeMaterials.DefaultValue;
            // The Attainable list is computed over NonSalvageProductionProcesses
            // (InventorySettings.StartRecomputeAttainability); building recipes must be in it, or
            // no structure could ever be marked producible.
            int buildRecipes = GameData.Instance.NonSalvageProductionProcesses.Count(r => r.GetProductionUI() == UWGame.SimSide.Processes.ProcessType.ProductionUI.Build);
            Check(buildRecipes > 0, $"the Attainable list covers construction recipes ({buildRecipes} of them)");

            // A stopped task rates 0, which every job picker drops; the others keep their rating.
            double rated = 0.8, stopped = 0.8;
            UWGame.SimSide.AI.Goals.EvaluateJob.ApplyJobPriorityModifier(UWGame.SimSide.Jobs.Priority.Normal, ref rated);
            UWGame.SimSide.AI.Goals.EvaluateJob.ApplyJobPriorityModifier(UWGame.SimSide.Jobs.Priority.Stopped, ref stopped);
            Check(rated == 0.8 && stopped == 0.0, $"STOP rates a task 0, NORMAL leaves it (got {stopped}, {rated})");
            Check(UWGame.SimSide.Jobs.Job.GetPriorityAsString(UWGame.SimSide.Jobs.Priority.Stopped) == "STOP", "the list shows it as STOP");

            // A construction site waiting for one bundle of spoak branches, which nobody has found.
            var build = (UWGame.SimSide.Jobs.ProcessJob)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(UWGame.SimSide.Jobs.ProcessJob));
            build.BuildingJob = (UWGame.SimSide.Jobs.BuildingJob)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(UWGame.SimSide.Jobs.BuildingJob));
            var workshopJob = (UWGame.SimSide.Jobs.ProcessJob)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(UWGame.SimSide.Jobs.ProcessJob));
            var haul = (UWGame.SimSide.Jobs.HaulingJobAnyItemOfType)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(UWGame.SimSide.Jobs.HaulingJobAnyItemOfType));
            haul.RequiredByProcessJob = build;
            var siteOwner = NewOwner();
            siteOwner.ProductionOrders.SetStandingOrder(branches, 0);
            siteOwner.HaulingJobsAnyItemOfType[branches] = new List<UWGame.SimSide.Jobs.HaulingJobAnyItemOfType> { haul };
            build.Priority = UWGame.SimSide.Jobs.Priority.Normal;
            Check(ToGather(siteOwner, branches) == 1, $"a site waiting for 1 bundle of branches asks for 1 (got {ToGather(siteOwner, branches)})");
            build.Priority = UWGame.SimSide.Jobs.Priority.Stopped;
            Check(ToGather(siteOwner, branches) == 0, $"  stopped, it asks for none (got {ToGather(siteOwner, branches)})");

            stopSetting.Value = "false";
            Check(!UWGame.Mods.GatherOnDemandMod.OffersStop(build), "setting off: no STOP in the list");
            stopSetting.Value = "true";
            Check(UWGame.Mods.GatherOnDemandMod.OffersStop(build) && !UWGame.Mods.GatherOnDemandMod.OffersStop(workshopJob) && !UWGame.Mods.GatherOnDemandMod.OffersStop(null),
                  "setting on: STOP for a construction, not for a workshop task");
            stopSetting.Value = stopSetting.DefaultValue;
        }
        setting.Value = setting.DefaultValue;
        Console.WriteLine(failures == 0 ? "gatherondemand self-test OK" : $"gatherondemand self-test FAILED - {failures} check(s)");
        return failures == 0 ? 0 : 1;
    }

    private static int FreshFoodSelfTest()
    {
        Console.WriteLine("==> freshfood self-test");
        UWGame.Mods.FreshFoodMod.RegisterSettings();
        if (UWGame.Mods.ModSettings.Find("freshfood.enabled") == null)
        {
            Console.WriteLine("  the mod is not in this build - nothing to check");
            return 0;
        }
        int failures = 0;
        void Check(bool ok, string what)
        {
            Console.WriteLine((ok ? "  ok    " : "  FAIL  ") + what);
            if (!ok) failures++;
        }
        Check(UWGame.Mods.FreshFoodMod.Urgency(null) == 0.0, "food that never spoils has no urgency");
        Check(UWGame.Mods.FreshFoodMod.Urgency(0) == 1.0 && UWGame.Mods.FreshFoodMod.Urgency(-2) == 1.0, "spoiling now: full urgency");
        Check(Math.Abs(UWGame.Mods.FreshFoodMod.Urgency(10) - 0.5) < 1e-9, "half-way to the 20-day horizon: 0.5");
        Check(UWGame.Mods.FreshFoodMod.Urgency(60) == 0.0, "beyond the horizon: none");
        // The studio's own numbers, copied from EvaluateEat.ComputePeopleEatScore / ScoreCondition.
        static double StudioFreshness(double days) => Math.Pow((6.0 - Math.Min(days, 6.0)) / 6.0, 2.0);
        static double StudioScore(double travel, double nutrients, double condition) => 0.45 * travel + 0.4 * nutrients + 0.1 * condition;
        double soupStudio = StudioScore(0.6, 0.7, StudioFreshness(3)), fishStudio = StudioScore(0.8, 0.7, StudioFreshness(60));
        double soupMod = UWGame.Mods.FreshFoodMod.EatScore(0.6, 0.7, UWGame.Mods.FreshFoodMod.Urgency(3), 0);
        double fishMod = UWGame.Mods.FreshFoodMod.EatScore(0.8, 0.7, UWGame.Mods.FreshFoodMod.Urgency(60), 0);
        Check(fishStudio > soupStudio, $"studio: the nearer pickled fish wins ({fishStudio:0.000} vs soup {soupStudio:0.000}) - the complaint");
        Check(soupMod > fishMod, $"mod: the soup that spoils in 3 days wins ({soupMod:0.000} vs fish {fishMod:0.000})");
        double soupFar = UWGame.Mods.FreshFoodMod.EatScore(0.2, 0.7, UWGame.Mods.FreshFoodMod.Urgency(3), 0);
        Check(soupFar < fishMod, $"but not at any distance: soup much further away loses ({soupFar:0.000})");
        Console.WriteLine(failures == 0 ? "freshfood self-test OK" : $"freshfood self-test FAILED - {failures} check(s)");
        return failures == 0 ? 0 : 1;
    }

    /// <summary>
    /// Plays key presses through HudMod.MarkersShown, the LeftAlt gesture: held shows the markers
    /// only while down; two presses within 0.8 s latch them on until the next such pair; a
    /// press alone does not change the latch, and two presses too far apart are not a pair.
    /// Then the item layers against the loaded and validated tables (HudMod.ItemGrouping).
    /// </summary>
    private static int HudSelfTest()
    {
        int loaded = Run(Sim.SerializeMode.NoSerialize, "base tables, the way the game loads them");
        if (loaded != 0) return loaded;
        // Tool.ToolEntityTypes - which items each process takes as a tool - is filled in the
        // validation pass, and the TOOLS checks below read it.
        if (!ValidateDataComplete()) return 1;
        Console.WriteLine("==> hud self-test");
        UWGame.Mods.HudMod.RegisterSettings();
        if (UWGame.Mods.ModSettings.Find("hud.revealKey") == null)
        {
            Console.WriteLine("  the mod is not in this build - nothing to check");
            return 0;
        }
        int failures = 0;
        void Check(bool ok, string what)
        {
            Console.WriteLine((ok ? "  ok    " : "  FAIL  ") + what);
            if (!ok) failures++;
        }
        bool Step(bool down, long at) => UWGame.Mods.HudMod.MarkersShown(down, at);
        UWGame.Mods.HudMod.ResetMarkerGesture();
        Check(Step(true, 1000) && !Step(false, 1300), "held: shown while down, gone when released");
        Check(Step(true, 5000) && Step(false, 5100) == false && Step(true, 5600) && Step(false, 5700),
              "two presses within 0.8 s: still shown after release");
        Check(Step(false, 8000), "latched: stays shown with no key down");
        Check(Step(true, 9000) && Step(false, 9100), "one press alone leaves the latch on");
        Check(Step(true, 9500) && !Step(false, 9600), "the next quick pair releases it");
        Check(Step(true, 20000) && !Step(false, 20100) && Step(true, 21500) && !Step(false, 21600),
              "two presses 1.5 s apart are not a pair");
        Check(Step(true, 25000) && !Step(false, 25100) && Step(true, 25900) && !Step(false, 26000),
              "two presses 0.9 s apart are not a pair (HudMod.DoublePressMilliseconds, 0.8 s)");
        Check(Step(true, 30000) && !Step(false, 30050) && Step(true, 30700) && Step(false, 30750)
              && Step(true, 30800) && Step(false, 30850),
              "a third quick press starts a new pair rather than releasing");
        UWGame.Mods.HudMod.ResetMarkerGesture();
        // ZONES (HudMod.ShowsZone): a row of its own in the overlay panel, off hides every zone but
        // the selected one, and the stock value shows them all.
        int zonesRow = Enumerable.Range(0, UWGame.Mods.HudMod.MarkerRowCount).FirstOrDefault(i => UWGame.Mods.HudMod.MarkerRowLabel(i) == "ZONES", -1);
        Check(zonesRow >= 0, "the overlay panel has a ZONES row");
        var hideZones = UWGame.Mods.ModSettings.Find("hud.hideZones");
        Check(hideZones != null && UWGame.Mods.HudMod.ShowsZone(selected: false), "zones show by default (the studio's game)");
        if (hideZones != null)
        {
            hideZones.Value = "true";
            Check(!UWGame.Mods.HudMod.ShowsZone(selected: false) && UWGame.Mods.HudMod.ShowsZone(selected: true)
                  && zonesRow >= 0 && !UWGame.Mods.HudMod.MarkerRowIsOn(zonesRow),
                  "ZONES off: zones are hidden, except the selected one");
            hideZones.Value = "false";
        }
        // Smoke on top (HudMod.EffectsOnTop / CoversEffects): off by default, and what still covers
        // it - Kastuk's "terrain things, which contans "hill" in name, and also gardtower big and
        // giant" - must be real keys, and nothing else.
        var effectsOnTop = UWGame.Mods.ModSettings.Find("hud.effectsOnTop");
        Check(effectsOnTop != null && effectsOnTop.DefaultValue == "false" && !UWGame.Mods.HudMod.EffectsOnTop,
              "SMOKE AND SPARKS DRAWN OVER EVERYTHING exists, off by default");
        int hills = 0, covering = 0;
        foreach (var t in GameData.Instance.AllEntityTypes.Values)
        {
            if (t.KeyName.StartsWith("terrain:", StringComparison.Ordinal) && t.KeyName.IndexOf("hill", StringComparison.OrdinalIgnoreCase) >= 0) hills++;
            if (UWGame.Mods.HudMod.CoversEffects(t)) covering++;
        }
        bool towersReal = GameData.Instance.AllEntityTypes.TryGetValue("terrain:utgardstowerBig", out var bigTower)
                          & GameData.Instance.AllEntityTypes.TryGetValue("terrain:utgardstowerTall", out var tallTower);
        Check(towersReal && UWGame.Mods.HudMod.CoversEffects(bigTower) && UWGame.Mods.HudMod.CoversEffects(tallTower),
              "the big and the tall rock tower cover smoke");
        Check(hills > 0 && covering == hills + 2, $"every hill covers smoke ({hills}), and nothing else does but the two towers (got {covering})");
        Check(!UWGame.Mods.HudMod.CoversEffects(GameData.Instance.AllEntityTypes["terrain:utgardstowerSmall1"])
              && !UWGame.Mods.HudMod.CoversEffects(GameData.Instance.AllEntityTypes["structure:simpleSmithy"])
              && !UWGame.Mods.HudMod.CoversEffects(null),
              "a small tower, a smithy and nothing do not");
        // Item layers (HudMod.ItemGrouping): every layer gets real items, nothing alive or built is an
        // item, each layer has a name and a colour, and the studio's own groupings are untouched.
        var types = GameData.Instance.AllEntityTypes.Values.ToList();
        var layers = new[] { UWGame.ClientSide.Interface.Overlays.EntityGrouping.Tools, UWGame.ClientSide.Interface.Overlays.EntityGrouping.Weapons,
            UWGame.ClientSide.Interface.Overlays.EntityGrouping.PreparedFood, UWGame.ClientSide.Interface.Overlays.EntityGrouping.Ingredients,
            UWGame.ClientSide.Interface.Overlays.EntityGrouping.Materials };
        foreach (var layer in layers)
        {
            int n = types.Count(t => UWGame.ClientSide.Interface.Overlays.OverlaySettings.GetGrouping(t) == layer);
            Check(n > 0 && UWGame.ClientSide.Interface.Overlays.OverlaySettings.GetName(layer) != null
                  && UWGame.ClientSide.Interface.Overlays.OverlaySettings.GetGroupingColor(layer, faded: false).HasValue,
                  $"item layer {UWGame.ClientSide.Interface.Overlays.OverlaySettings.GetName(layer)}: {n} item types, with a name and a colour");
        }
        Check(!types.Any(t => (t.StructureType != null || t.BiologicalType != null) && UWGame.Mods.HudMod.ItemGrouping(t) != null),
              "no structure or creature is put in an item layer");
        Check(types.Where(t => t.StructureType != null).All(t => UWGame.ClientSide.Interface.Overlays.OverlaySettings.GetGrouping(t) == UWGame.ClientSide.Interface.Overlays.EntityGrouping.Structures),
              "structures keep the studio's STRUCTURES grouping");
        Check(GameData.Instance.AllEntityTypes.TryGetValue("item:gunpowderRifle", out var rifleType)
              && UWGame.ClientSide.Interface.Overlays.OverlaySettings.GetGrouping(rifleType) == UWGame.ClientSide.Interface.Overlays.EntityGrouping.Weapons,
              "a gunpowder rifle is in WEAPONS");
        // TOOLS holds every item a process takes as a tool (ProcessType.ProcessToolSet -> Tool.ToolEntityTypes,
        // the studio's own definition). Kastuk found the spade missing: tools that can also be swung
        // carry a WeaponType, and that was asked first. The only ones kept out are those the studio's
        // category files under "weapons" (the spears), which go to WEAPONS, as in the stockpile.
        var toolsLayer = UWGame.ClientSide.Interface.Overlays.EntityGrouping.Tools;
        var weaponsLayer = UWGame.ClientSide.Interface.Overlays.EntityGrouping.Weapons;
        UWGame.ClientSide.Interface.Overlays.EntityGrouping? Layer(UWGame.SimSide.Entities.EntityType t) => UWGame.ClientSide.Interface.Overlays.OverlaySettings.GetGrouping(t);
        var processTools = GameData.Instance.AllProcessTypes.Values
            .Where(p => p.ProcessToolSet?.Tools != null)
            .SelectMany(p => p.ProcessToolSet.Tools).Where(a => a.Tools != null)
            .SelectMany(a => a.Tools).SelectMany(tool => tool.ToolEntityTypes)
            .Where(t => t.ItemType != null && t.StructureType == null && t.BiologicalType == null && !t.IsIntrinsic())
            .Distinct().OrderBy(t => t.KeyName, StringComparer.Ordinal).ToList();
        var outOfTools = processTools.Where(t => t.CategoryKey != "weapons" && Layer(t) != toolsLayer).ToList();
        Check(processTools.Count > 0 && outOfTools.Count == 0,
              $"every item a process uses as a tool is in TOOLS ({processTools.Count - processTools.Count(t => t.CategoryKey == "weapons")})"
              + (outOfTools.Count == 0 ? "" : " - missing: " + string.Join(", ", outOfTools.Select(t => t.KeyName))));
        var spears = processTools.Where(t => t.CategoryKey == "weapons").ToList();
        Check(spears.All(t => Layer(t) == weaponsLayer),
              $"the process tools the studio files under weapons stay in WEAPONS: {string.Join(", ", spears.Select(t => t.Name))}");
        foreach (string spade in new[] { "item:steelSpade", "item:improvisedSpade" })
        {
            Check(GameData.Instance.AllEntityTypes.TryGetValue(spade, out var spadeType) && Layer(spadeType) == toolsLayer,
                  $"{spade} is in TOOLS");
        }
        var items = types.Where(t => t.ItemType != null && UWGame.Mods.HudMod.ItemGrouping(t) != null).ToList();
        var toolsElsewhere = items.Where(t => t.CategoryKey == "tools" && Layer(t) != toolsLayer).ToList();
        Check(toolsElsewhere.Count == 0, "every item in the studio's \"tools\" category is in TOOLS"
              + (toolsElsewhere.Count == 0 ? "" : " - missing: " + string.Join(", ", toolsElsewhere.Select(t => t.KeyName))));
        var weaponsElsewhere = items.Where(t => t.CategoryKey == "weapons" && Layer(t) != weaponsLayer).ToList();
        Check(weaponsElsewhere.Count == 0, "every item in the studio's \"weapons\" category is in WEAPONS"
              + (weaponsElsewhere.Count == 0 ? "" : " - not there: " + string.Join(", ", weaponsElsewhere.Select(t => t.KeyName))));
        Console.WriteLine("  info  TOOLS: " + string.Join(", ", items.Where(t => Layer(t) == toolsLayer).Select(t => t.Name).OrderBy(n => n, StringComparer.Ordinal)));
        Console.WriteLine("  info  WEAPONS: " + string.Join(", ", items.Where(t => Layer(t) == weaponsLayer).Select(t => t.Name).OrderBy(n => n, StringComparer.Ordinal)));

        // Overlapping labels into a list (tripleacoder, "HUD mod"): which labels go in it, and where.
        var R = (Func<int, int, int, int, Microsoft.Xna.Framework.Rectangle>)((x, y, w, h) => new Microsoft.Xna.Framework.Rectangle(x, y, w, h));
        // A touches B, B touches C, C does not touch A; D is apart.
        var labels = new[] { R(100, 100, 80, 20), R(150, 110, 80, 20), R(220, 125, 80, 20), R(600, 400, 80, 20) };
        var crowd = UWGame.Mods.HudMod.TouchingClosure(labels, 0);
        Check(crowd.OrderBy(i => i).SequenceEqual(new[] { 0, 1, 2 }),
              $"the list takes the hovered label, what touches it, and what touches those - not a label apart (got {string.Join(",", crowd)})");
        Check(UWGame.Mods.HudMod.TouchingClosure(labels, 3).SequenceEqual(new[] { 3 }), "a label touching nothing makes no list");
        var column = UWGame.Mods.HudMod.LayOutList(new[] { R(0, 0, 80, 20), R(0, 0, 120, 20), R(0, 0, 60, 20) }, new Microsoft.Xna.Framework.Point(100, 100), 900);
        Check(column.Select(r => r.X).Distinct().SequenceEqual(new[] { 100 }) && column[1].Y == column[0].Bottom + UWGame.Mods.HudMod.ListGap
              && column[2].Y == column[1].Bottom + UWGame.Mods.HudMod.ListGap && column[0].Y == 100,
              "laid out one under another from the hovered label, left edges lined up");
        bool apart = true;
        for (int i = 0; i < column.Count; i++)
            for (int j = i + 1; j < column.Count; j++)
                apart &= !column[i].Intersects(column[j]);
        Check(apart, "  and none covers another");
        var tall = UWGame.Mods.HudMod.LayOutList(Enumerable.Repeat(R(0, 0, 70, 20), 50).ToList(), new Microsoft.Xna.Framework.Point(100, 300), 600);
        Check(tall.All(r => r.Bottom <= 600 && r.Y >= 0) && tall.Select(r => r.X).Distinct().Count() > 1,
              $"too many for the screen: more columns side by side, nothing below the bottom interface ({tall.Select(r => r.X).Distinct().Count()} columns)");
        var low = UWGame.Mods.HudMod.LayOutList(Enumerable.Repeat(R(0, 0, 70, 20), 5).ToList(), new Microsoft.Xna.Framework.Point(100, 590), 600);
        Check(low.All(r => r.Bottom <= 600) && low.Select(r => r.X).Distinct().Count() == 1, "near the bottom, the list moves up instead of splitting");
        Check(UWGame.Mods.HudMod.LabelListSetting.DefaultValue == "true" && !UWGame.Mods.HudMod.LabelListSetting.AffectsSimulation,
              "hud.labelList is on by default and interface only");

        Console.WriteLine(failures == 0 ? "hud self-test OK" : $"hud self-test FAILED - {failures} check(s)");
        return failures == 0 ? 0 : 1;
    }

    /// <summary>
    /// Checks the part of OwnershipMod that reads the tables: the meal recipes a household planner can
    /// order - every one yields a FoodType.IsMeal item and takes real inputs - in a fixed order, as
    /// replays need. Paying and cooking need a running game and are not checked here.
    /// </summary>
    private static int OwnershipSelfTest()
    {
        int rc = Run(Sim.SerializeMode.NoSerialize, "base tables, the way the game loads them");
        if (rc != 0) return rc;
        if (!ValidateDataComplete()) return 1;
        Console.WriteLine("==> ownership self-test");
        int failures = 0;
        void Check(bool ok, string what)
        {
            Console.WriteLine((ok ? "  ok    " : "  FAIL  ") + what);
            if (!ok) failures++;
        }
        var meals = UWGame.Mods.OwnershipMod.MealProcesses();
        Check(meals.Count > 0, $"the planner finds meal recipes ({meals.Count})");
        Check(meals.All(p => p.InputsByType != null && p.InputsByType.Count > 0 && p.InputsByType.Keys.All(k => k != null)),
              "every one takes real inputs");
        Check(meals.All(p => p.Outputs != null && p.Outputs.Any(o => o.FinalEntityTypeToCreate?.ItemType?.FoodType?.IsMeal == true)),
              "every one yields a meal");
        Check(meals.Select(p => p.KeyName).SequenceEqual(meals.Select(p => p.KeyName).OrderBy(k => k, StringComparer.Ordinal)),
              "in a fixed order (by key), so every run and replay picks the same one");
        Console.WriteLine("  info  first few: " + string.Join(", ", meals.Take(5).Select(p => p.KeyName)));
        // The planner acts through commands (tripleacoder); a command must survive the replay serializer.
        var commandSerializer = new System.Xml.Serialization.XmlSerializer(typeof(List<UWGame.Control.Commands.Command>));
        var commandWriter = new System.IO.StringWriter();
        commandSerializer.Serialize(commandWriter, new List<UWGame.Control.Commands.Command>
        {
            new UWGame.SimSide.Commands.SetHouseholdProduction((UWGame.SimSide.Entities.HouseholdID)9UL, "cookStew", "item:stew"),
            new UWGame.SimSide.Commands.GiveToHousehold((UWGame.SimSide.Entities.HouseholdID)9UL, new[] { (UWGame.SimSide.Entities.EntityID)11L, (UWGame.SimSide.Entities.EntityID)12L }),
        });
        var commandsBack = commandSerializer.Deserialize(new System.IO.StringReader(commandWriter.ToString())) as List<UWGame.Control.Commands.Command>;
        var orderBack = commandsBack?.ElementAtOrDefault(0) as UWGame.SimSide.Commands.SetHouseholdProduction;
        var giveBack = commandsBack?.ElementAtOrDefault(1) as UWGame.SimSide.Commands.GiveToHousehold;
        Check(orderBack != null && orderBack.HouseholdID == 9UL && orderBack.ProcessTypeKey == "cookStew" && orderBack.EntityTypeKey == "item:stew",
              "SetHouseholdProduction round-trips through the replay serializer");
        Check(giveBack != null && giveBack.HouseholdID == 9UL && giveBack.EntityIDs.SequenceEqual(new[] { 11L, 12L }),
              "GiveToHousehold round-trips through the replay serializer");
        Console.WriteLine(failures == 0 ? "ownership self-test OK" : $"ownership self-test FAILED - {failures} check(s)");
        return failures == 0 ? 0 : 1;
    }

    /// <summary>
    /// Checks ToolCareMod's rule for what a colonist looks after: weapons always, and items above the
    /// survival tier by their own tier or the lowest tier of whatever makes them. Prints how many of
    /// each so a wrong tier table shows up as a strange count.
    /// </summary>
    private static int ToolCareSelfTest()
    {
        int rc = Run(Sim.SerializeMode.NoSerialize, "base tables, the way the game loads them");
        if (rc != 0) return rc;
        // Tiers (EntityType/ProcessType.TierOrAreaType) are linked in the validation pass.
        if (!ValidateDataComplete()) return 1;
        Console.WriteLine("==> tool care self-test");
        int failures = 0;
        void Check(bool ok, string what)
        {
            Console.WriteLine((ok ? "  ok    " : "  FAIL  ") + what);
            if (!ok) failures++;
        }
        var types = GameData.Instance.AllEntityTypes;
        Check(types.TryGetValue("item:gunpowderRifle", out var rifle) && UWGame.Mods.ToolCareMod.IsProtected(rifle), "a gunpowder rifle is protected (a weapon)");
        var items = types.Values.Where(t => t.ItemType != null && !t.IsIntrinsic()).ToList();
        int weapons = items.Count(t => t.ItemType.WeaponType != null);
        int byTier = items.Count(t => t.ItemType.WeaponType == null && UWGame.Mods.ToolCareMod.IsProtected(t));
        Check(items.Count(t => !UWGame.Mods.ToolCareMod.IsProtected(t)) > 0, "most things are not protected - cheap tools still drop freely");
        Check(byTier > 0, $"the tier rule protects something besides weapons ({byTier} item types)");
        Check(rifle != null && UWGame.Mods.ToolCareMod.IsLookedAfter(null, rifle), "a weapon is looked after - dropped last, put back");
        var cheapTool = items.Where(t => t.ToolType != null && !UWGame.SimSide.Entities.ToolType.IsImmovable(t) && !UWGame.Mods.ToolCareMod.IsProtected(t)).OrderBy(t => t.KeyName.Contains("Knife") ? 0 : 1).FirstOrDefault();
        var lookAfter = UWGame.Mods.ToolCareMod.LookAfter;
        string keptLookAfter = lookAfter.Value;
        lookAfter.Value = "WEAPONS AND GOOD TOOLS";
        Check(cheapTool != null && !UWGame.Mods.ToolCareMod.IsLookedAfter(null, cheapTool), $"LOOK AFTER WEAPONS AND GOOD TOOLS leaves a cheap tool alone ({cheapTool?.KeyName})");
        lookAfter.Value = "ALL TOOLS AND WEAPONS";
        Check(cheapTool != null && UWGame.Mods.ToolCareMod.IsLookedAfter(null, cheapTool), "LOOK AFTER ALL TOOLS AND WEAPONS includes it");
        lookAfter.Value = keptLookAfter;

        // When to drop where they stand (Jerrybi): a fight, wounds or hunger; otherwise put it back.
        var placement = UWGame.Mods.ToolCareMod.Placement;
        var forFights = UWGame.Mods.ToolCareMod.ForFights;
        var whenHurt = UWGame.Mods.ToolCareMod.WhenHurt;
        var whenHungry = UWGame.Mods.ToolCareMod.WhenHungry;
        string[] kept = { placement.Value, forFights.Value, whenHurt.Value, whenHungry.Value };
        bool Drops(bool fight, float? health, float? food) => UWGame.Mods.ToolCareMod.DropsWhereItStands(fight, health, food);
        // Off unless asked for (Kastuk, 2026-10-05: "A fight: drop it where they stand", and also
        // "hurt drop" and "hungry drop" must be off by default").
        Check(forFights.DefaultValue == "false" && whenHurt.DefaultValue == "OFF" && whenHungry.DefaultValue == "OFF",
              "the fight, hurt and hungry drops are all OFF by default");
        placement.Value = "BACK WHERE IT CAME FROM"; forFights.Value = "true"; whenHurt.Value = "50% HEALTH"; whenHungry.Value = "WHEN HUNGRY";
        Check(!Drops(false, 1f, 1f), "healthy, fed and not fighting: the tool is put back, not dropped");
        Check(Drops(true, 1f, 1f), "setting off to a fight: dropped where they stand");
        Check(Drops(false, 0.4f, 1f) && !Drops(false, 0.6f, 1f), "HURT below 50% HEALTH: dropped at 40%, put back at 60%");
        Check(Drops(false, 1f, 0.7f) && !Drops(false, 1f, 0.8f), "WHEN HUNGRY: the game's 'has not eaten' level (70%) drops it");
        Check(!Drops(false, null, null), "no body and no needs: nothing says drop");
        whenHungry.Value = "WHEN STARVING";
        Check(!Drops(false, 1f, 0.3f) && Drops(false, 1f, 0f), "WHEN STARVING: only a food need run out drops it");
        forFights.Value = "false"; whenHurt.Value = "OFF"; whenHungry.Value = "OFF";
        Check(!Drops(true, 0.1f, 0f), "every condition OFF: put back even when fighting, hurt and starving");
        placement.Value = "NOWHERE - DROP IT";
        Check(Drops(false, 1f, 1f), "A TOOL THAT MUST GO IS TAKEN NOWHERE: always dropped where they stand, as the studio made it");
        placement.Value = kept[0]; forFights.Value = kept[1]; whenHurt.Value = kept[2]; whenHungry.Value = kept[3];
        Console.WriteLine($"  info  {items.Count} item types: {weapons} weapons, {byTier} more above the survival tier");
        Console.WriteLine(failures == 0 ? "tool care self-test OK" : $"tool care self-test FAILED - {failures} check(s)");
        return failures == 0 ? 0 : 1;
    }

    /// <summary>
    /// Checks Snapshotter.ResolveSavedType, the reason vanilla saves did not load in Deluxe 1.0 to
    /// 1.3: a save names generic types with assembly-qualified arguments ("UnclaimedWorld,
    /// Version=1.0.4.8" in the studio's saves), and Type.GetType will not bind a newer version than
    /// the one loaded. A real generic type's saved name is rewritten to the studio's version and to
    /// a future one; both must resolve to the same type.
    /// </summary>
    private static int SaveTypeSelfTest()
    {
        Console.WriteLine("==> save type-name self-test");
        int failures = 0;
        void Check(bool ok, string what)
        {
            Console.WriteLine((ok ? "  ok    " : "  FAIL  ") + what);
            if (!ok) failures++;
        }
        Type type = typeof(UWGame.SimSide.Snapshots.LookUp<UWGame.SimSide.AI.MemoryFact, UWGame.SimSide.AI.MemoryFactID>);
        string saved = type.FullName;
        Check(saved.Contains("UnclaimedWorld, Version="), "a generic type's saved name carries the assembly version: " + saved.Substring(0, Math.Min(90, saved.Length)) + "...");
        string Rewrite(string version) => System.Text.RegularExpressions.Regex.Replace(saved, @"UnclaimedWorld, Version=[0-9.]+", "UnclaimedWorld, Version=" + version);
        foreach (string version in new[] { "1.0.4.8", "9.9.9.9", "0.0.0.1" })
        {
            Type resolved = UWGame.SimSide.Snapshots.Snapshotter.ResolveSavedType(Rewrite(version));
            Check(resolved == type, $"a save naming UnclaimedWorld Version={version} resolves to the same type");
        }
        Check(UWGame.SimSide.Snapshots.Snapshotter.ResolveSavedType("UWGame.NoSuchType, UnclaimedWorld, Version=1.0.0.0") == null,
              "a type that does not exist is still null, not something else");
        Console.WriteLine(failures == 0 ? "save type-name self-test OK" : $"save type-name self-test FAILED - {failures} check(s)");
        return failures == 0 ? 0 : 1;
    }

    /// <summary>
    /// Checks HuntingMod with its prey switch as the settings file has it: on, the swarmer reaches
    /// the human Prey list - which becomes the PreyTypes the Hunt window reads - and its carcass is an item, so a hunt
    /// order has something to keep; off, the list is the studio's sixteen. The gate runs it both ways.
    /// </summary>
    private static int HuntingSelfTest()
    {
        int rc = Run(Sim.SerializeMode.NoSerialize, "base tables, the way the game loads them");
        if (rc != 0) return rc;
        Console.WriteLine("==> hunting self-test");
        int failures = 0;
        void Check(bool ok, string what)
        {
            Console.WriteLine((ok ? "  ok    " : "  FAIL  ") + what);
            if (!ok) failures++;
        }
        var types = GameData.Instance.AllEntityTypes;
        types.TryGetValue("entity:human", out var human);
        types.TryGetValue("entity:swarmer", out var swarmer);
        // Prey, not PreyTypes: IntelligenceType.PostLoadContentInitialize builds the set from these
        // strings one for one, in the game's post-load pass, which this tool does not run.
        Check(human?.IntelligenceType?.Prey != null && swarmer != null, "entity:human has a prey list and entity:swarmer exists");
        bool on = UWGame.Mods.HuntingMod.SwarmersInZones.On;
        bool listed = human?.IntelligenceType?.Prey?.Contains("entity:swarmer") == true;
        Check(listed == on, on ? "switch on: the swarmer is human prey" : "switch off: the swarmer is not human prey (the studio's list)");
        Check(swarmer?.BiologicalType?.Carcass != null && types.TryGetValue(swarmer.BiologicalType.Carcass, out var carcass) && carcass.ItemType != null,
              "the swarmer's carcass (" + swarmer?.BiologicalType?.Carcass + ") is an item type, so ProductionOrders has an order for it");
        Check(UWGame.Mods.HuntingMod.ParseFactor("x1") == 1f && UWGame.Mods.HuntingMod.ParseFactor("x1.5") == 1.5f
              && UWGame.Mods.HuntingMod.ParseFactor("nonsense") == 1f, "the chase range choices parse, and a bad value is x1");
        // Autoclaim in camp: which carcasses the policy may take, and that its switch survives the replay serializer.
        foreach (string key in new[] { "entity:binalRat", "entity:fieldQuadite", "entity:twinkler", "entity:swarmer" })
        {
            Check(types.TryGetValue(key, out var animal) && UWGame.Mods.HuntingMod.IsClaimableAnimal(animal), key + "'s carcass may be autoclaimed");
        }
        Check(human != null && !UWGame.Mods.HuntingMod.IsClaimableAnimal(human), "a person's body is never autoclaimed");
        Check(!UWGame.Mods.HuntingMod.AutoclaimsCampKills(null) && UWGame.Mods.HuntingMod.CampKillClaimant(null, default) == null,
              "with no expedition the policy is off, and nothing is claimed");
        CheckWhoOwnsTheCarcass(Check, human);
        var commandSerializer = new System.Xml.Serialization.XmlSerializer(typeof(List<UWGame.Control.Commands.Command>));
        var commandWriter = new System.IO.StringWriter();
        commandSerializer.Serialize(commandWriter, new List<UWGame.Control.Commands.Command>
        {
            new UWGame.SimSide.Commands.SetAutoclaimCampKills((UWGame.SimSide.Expeditions.ExpeditionID)7L, true),
        });
        var autoclaimBack = (commandSerializer.Deserialize(new System.IO.StringReader(commandWriter.ToString())) as List<UWGame.Control.Commands.Command>)
            ?.FirstOrDefault() as UWGame.SimSide.Commands.SetAutoclaimCampKills;
        Check(autoclaimBack != null && autoclaimBack.ExpeditionID == 7L && autoclaimBack.Claim, "SetAutoclaimCampKills round-trips through the replay serializer");
        Console.WriteLine(failures == 0 ? "hunting self-test OK" : $"hunting self-test FAILED - {failures} check(s)");
        return failures == 0 ? 0 : 1;
    }

    /// <summary>
    /// Kastuk, "Autoclaim of bodies", after c2db39e: "Now all killed animals become claimed, at far
    /// distance from camp too ... Switch in Policy is not changing anything, they become claimed
    /// with or without it."
    ///
    /// Who ends up owning a wild animal's carcass is two decisions, both checked here without a play
    /// site: the core's when the lethal blow lands (Entity.CarcassOwnerOnLethalBlow - what GoalThink
    /// gives GoalCollapse), then, for a carcass still unowned, the policy's in Entity.Kill
    /// (HuntingMod.ClaimsKillAt, the rule CampKillClaimant applies to each player expedition). The
    /// report was the first decision handing every kill by the colony's people, dogs, HOUNDs and
    /// sentries to the colony, anywhere, so the second was never asked.
    /// </summary>
    private static void CheckWhoOwnsTheCarcass(Action<bool, string> Check, UWGame.SimSide.Entities.EntityType human)
    {
        var colony = (UWGame.SimSide.Entities.OwnerID)41L;
        // The studio's own camp radius for people, in world units, around an arbitrary camp centre.
        float radius = human?.IntelligenceType?.ForageAndHuntingRadius ?? 0;
        Check(radius > 0, $"people have a forage and hunting radius ({radius})");
        var camp = new Microsoft.Xna.Framework.Vector2(4000f, 4000f);
        var inCamp = camp + new Microsoft.Xna.Framework.Vector2(radius * 0.5f, 0f);
        var farAway = camp + new Microsoft.Xna.Framework.Vector2(radius * 3f, radius);
        var ticked = new UWGame.SimSide.Expeditions.Expedition();
        UWGame.Mods.HuntingMod.SetAutoclaimCampKills(ticked, true);
        var unticked = new UWGame.SimSide.Expeditions.Expedition();
        UWGame.Mods.HuntingMod.SetAutoclaimCampKills(unticked, false);
        Check(UWGame.Mods.HuntingMod.AutoclaimsCampKills(ticked) && !UWGame.Mods.HuntingMod.AutoclaimsCampKills(unticked),
              "the policy box reads back ticked and unticked");

        // A wild animal killed outside a hunt - by a colonist in a fight, a dog, a HOUND or a sentry
        // gun: the blow carries no owner (EvaluateAttackJobs.SetGoal, "don't claim the carcass when
        // not hunting") and the animal had none. Whether it ends up claimed, and by whom.
        bool Claimed(UWGame.SimSide.Expeditions.Expedition expedition, Microsoft.Xna.Framework.Vector2 at) =>
            UWGame.SimSide.Entities.Entity.CarcassOwnerOnLethalBlow(null, null) != null
            || UWGame.Mods.HuntingMod.ClaimsKillAt(expedition, camp, radius, at);
        Check(UWGame.SimSide.Entities.Entity.CarcassOwnerOnLethalBlow(null, null) == null,
              "a kill outside a hunt is not given to the killer's side when the blow lands");
        Check(!Claimed(unticked, farAway), "the report: a kill far from camp, box unticked, stays unclaimed");
        Check(!Claimed(ticked, farAway), "the report: a kill far from camp, box ticked, stays unclaimed");
        Check(!Claimed(unticked, inCamp), "the report: a kill in camp, box unticked, stays unclaimed");
        bool on = UWGame.Mods.HuntingMod.OffersAutoclaim;
        Check(Claimed(ticked, inCamp) == on, on
            ? "AUTOCLAIM SWITCH on: a kill in camp, box ticked, is claimed"
            : "AUTOCLAIM SWITCH off: a kill in camp is not claimed even with the box ticked");
        Check(UWGame.Mods.HuntingMod.ClaimsKillAt(ticked, camp, radius, camp + new Microsoft.Xna.Framework.Vector2(radius * 1.01f, 0f)) == false,
              "just outside the radius is outside");

        // What the core still gives, policy or not: a hunt's prey to the hunters' expedition, wherever
        // it falls (GoalHunt passes the owner with the blow), and your own animal's carcass to you.
        Check(UWGame.SimSide.Entities.Entity.CarcassOwnerOnLethalBlow(colony, null) == colony,
              "a hunt's kill is the hunting expedition's, however far from camp");
        Check(UWGame.SimSide.Entities.Entity.CarcassOwnerOnLethalBlow(null, colony) == colony,
              "your own dog or livestock killed by a predator stays yours");
    }

    /// <summary>
    /// Checks SelfPreservationMod's "dogs drop vermin for real threats" against the real creatures.
    /// The decision needs three things: a colony animal, a vermin job, and a threat the colony
    /// knows of. The last two come from ThreatJobManager, which files a creature as vermin by
    /// BiologicalType.IsVermin - so the rat and the field quadite must be vermin and the whipjaw
    /// must not, or the whipjaw would be on the 4.9-priority vermin list beside the rat and there
    /// would be no "real threat" to leave the rat for. Then the decision itself, both ways: with the
    /// switch off a dog keeps its vermin whatever comes (the studio's game, and the report); with it
    /// on it leaves it - and nothing else does. The live part (GoalAttack giving the job up) needs a
    /// colony and is not reached here.
    /// </summary>
    private static int SelfPreservationSelfTest()
    {
        int rc = Run(Sim.SerializeMode.NoSerialize, "base tables, the way the game loads them");
        if (rc != 0) return rc;
        Console.WriteLine("==> self-preservation self-test");
        int failures = 0;
        void Check(bool ok, string what)
        {
            Console.WriteLine((ok ? "  ok    " : "  FAIL  ") + what);
            if (!ok) failures++;
        }
        var types = GameData.Instance.AllEntityTypes;
        bool Vermin(string key) => types.TryGetValue(key, out var t) && t.BiologicalType?.IsVermin == true;
        foreach (string key in new[] { "entity:binalRat", "entity:fieldQuadite" })
        {
            Check(types.ContainsKey(key) && Vermin(key), key + " is vermin, so its threat job is an asset threat");
        }
        foreach (string key in new[] { "entity:whipjaw", "entity:swarmer", "entity:patrician" })
        {
            Check(types.ContainsKey(key) && !Vermin(key), key + " is not vermin, so its threat job is a real threat");
        }
        Check(types.TryGetValue("entity:dog", out var dog) && dog.Person == null && dog.IntelligenceType?.CanAttack != false,
              "entity:dog is an animal that attacks - a colony animal when it is the player's");

        bool on = UWGame.Mods.SelfPreservationMod.AnimalsDropVermin.On;
        bool dogLeaves = UWGame.Mods.SelfPreservationMod.LeavesVermin(isColonyAnimal: true, jobIsVermin: true, colonyKnowsOfRealThreat: true);
        Check(dogLeaves == on, on ? "switch on: a dog chasing vermin leaves it when the colony knows of a real threat"
                                  : "switch off: a dog chasing vermin keeps it whatever comes (the studio's game)");
        Check(!UWGame.Mods.SelfPreservationMod.LeavesVermin(isColonyAnimal: true, jobIsVermin: true, colonyKnowsOfRealThreat: false),
              "with no real threat about, a dog keeps its vermin");
        Check(!UWGame.Mods.SelfPreservationMod.LeavesVermin(isColonyAnimal: true, jobIsVermin: false, colonyKnowsOfRealThreat: true),
              "a dog already on a real threat is not pulled off it");
        Check(!UWGame.Mods.SelfPreservationMod.LeavesVermin(isColonyAnimal: false, jobIsVermin: true, colonyKnowsOfRealThreat: true),
              "a person or a wild animal is never affected");
        Console.WriteLine(failures == 0 ? "self-preservation self-test OK" : $"self-preservation self-test FAILED - {failures} check(s)");
        return failures == 0 ? 0 : 1;
    }

    /// <summary>
    /// Checks SafeSleepMod's rule for which wild groups make a nest worth avoiding, against the real
    /// creatures: swarmers, whipjaws and patricians do; field quadites, rats that only forage and
    /// people who trade do not. A rule that let quadite nests count would keep colonists out of
    /// every field; one that missed swarmers would be the original report.
    /// </summary>
    private static int SafeSleepSelfTest()
    {
        int rc = Run(Sim.SerializeMode.NoSerialize, "base tables, the way the game loads them");
        if (rc != 0) return rc;
        Console.WriteLine("==> safe sleep self-test");
        int failures = 0;
        void Check(bool ok, string what)
        {
            Console.WriteLine((ok ? "  ok    " : "  FAIL  ") + what);
            if (!ok) failures++;
        }
        var types = GameData.Instance.AllEntityTypes;
        bool Dangerous(string key) => types.TryGetValue(key, out var t) && UWGame.Mods.SafeSleepMod.IsDangerous(t);
        foreach (string key in new[] { "entity:swarmer", "entity:whipjaw", "entity:patrician" })
        {
            Check(types.ContainsKey(key) && Dangerous(key), key + " makes its nest one to avoid");
        }
        foreach (string key in new[] { "entity:fieldQuadite", "entity:human" })
        {
            Check(types.ContainsKey(key) && !Dangerous(key), key + " does not");
        }
        int dangerous = types.Values.Count(t => UWGame.Mods.SafeSleepMod.IsDangerous(t));
        Console.WriteLine($"  info  {dangerous} creature types count as dangerous: " + string.Join(", ", types.Values.Where(t => UWGame.Mods.SafeSleepMod.IsDangerous(t)).Select(t => t.KeyName)));
        Console.WriteLine(failures == 0 ? "safe sleep self-test OK" : $"safe sleep self-test FAILED - {failures} check(s)");
        return failures == 0 ? 0 : 1;
    }

    /// <summary>
    /// Checks ReserveMod: a reserve set through the mod is read back from the expedition's custom
    /// fields and clears at 0, the SetReserve command survives the replay serializer (a command left
    /// out of Command's XmlInclude list breaks every replay that holds one), the slider's top is
    /// what it says, and colonists eating at the same time do not each count the same spare fish.
    /// Needs reserve.enabled on - the gate writes it.
    /// </summary>
    private static int ReserveSelfTest()
    {
        int rc = Run(Sim.SerializeMode.NoSerialize, "base tables, the way the game loads them");
        if (rc != 0) return rc;
        Console.WriteLine("==> reserve self-test");
        int failures = 0;
        void Check(bool ok, string what)
        {
            Console.WriteLine((ok ? "  ok    " : "  FAIL  ") + what);
            if (!ok) failures++;
        }
        if (!UWGame.Mods.ReserveMod.Enabled)
        {
            Console.WriteLine("  the mod is off or not in this build - nothing to check");
            return 0;
        }
        var types = GameData.Instance.AllEntityTypes;
        Check(types.TryGetValue("item:smokedStreakFin", out var fish), "item:smokedStreakFin, the trade food asked about, is in the table");
        var expedition = new UWGame.SimSide.Expeditions.Expedition();
        Check(UWGame.Mods.ReserveMod.Reserved(expedition, fish) == 0, "nothing is reserved on a new expedition");
        UWGame.Mods.ReserveMod.SetReserve(expedition, fish, 12);
        Check(UWGame.Mods.ReserveMod.Reserved(expedition, fish) == 12, "a reserve of 12 reads back as 12");
        Check(expedition.GetPropertyValue(UWGame.Mods.ReserveMod.KeyPrefix + fish.KeyName, null, null)?.NumberResult == 12f,
              "it is kept in the expedition's saved custom fields, under reserve:<item key>");
        UWGame.Mods.ReserveMod.SetReserve(expedition, fish, 0);
        Check(UWGame.Mods.ReserveMod.Reserved(expedition, fish) == 0
              && expedition.GetPropertyValue(UWGame.Mods.ReserveMod.KeyPrefix + fish.KeyName, null, null) == null,
              "a reserve of 0 removes the field");
        var serializer = new System.Xml.Serialization.XmlSerializer(typeof(List<UWGame.Control.Commands.Command>));
        var writer = new System.IO.StringWriter();
        serializer.Serialize(writer, new List<UWGame.Control.Commands.Command> { new UWGame.SimSide.Commands.SetReserve((UWGame.SimSide.Expeditions.ExpeditionID)7L, fish.KeyName, 5) });
        var back = serializer.Deserialize(new System.IO.StringReader(writer.ToString())) as List<UWGame.Control.Commands.Command>;
        var command = back?.FirstOrDefault() as UWGame.SimSide.Commands.SetReserve;
        Check(command != null && command.ExpeditionID == 7L && command.EntityTypeKey == fish.KeyName && command.Amount == 5,
              "SetReserve round-trips through the replay serializer");
        Check(UWGame.Mods.ReserveMod.SliderMax(0, 0) == 20 && UWGame.Mods.ReserveMod.SliderMax(35, 0) == 40
              && UWGame.Mods.ReserveMod.SliderMax(5, 50) == 60, "the slider tops out at 20, or the next ten above stock or reserve");

        // Mealtime (Kastuk, 2 October: reserved smoked fish eaten from the storage beside the
        // kitchen, where fresh glassy porridge was). Twelve fish, ten reserved; four colonists, none
        // starving, each starts on a bowl of porridge and tops it up with fish the way
        // GoalEat.GetAdditionalItemsToConsume does: MayTakeFood for each candidate, counting its own
        // picks. A meal's fish are claimed (SetInUseBy) until they are eaten, and stay in the count
        // until then - so the count each colonist goes by is ReserveMod.CountForMeal's.
        var shelf = Enumerable.Range(1, 12).Select(i => (UWGame.SimSide.Entities.EntityID)i).ToList();
        var claims = new Dictionary<UWGame.SimSide.Entities.EntityID, UWGame.SimSide.Entities.EntityID>();
        var eating = new HashSet<UWGame.SimSide.Entities.EntityID>();
        UWGame.SimSide.Entities.EntityID? ClaimOf(UWGame.SimSide.Entities.EntityID id) => claims.TryGetValue(id, out var by) ? by : null;
        int TopUp(UWGame.SimSide.Entities.EntityID colonist, int wanted)
        {
            int taken = 0;
            foreach (var id in shelf.Where(id => !claims.ContainsKey(id)).ToList())
            {
                if (taken == wanted || !UWGame.Mods.ReserveMod.MayTake(UWGame.Mods.ReserveMod.CountForMeal(shelf, ClaimOf, eating.Contains, colonist), 10, taken))
                {
                    continue;
                }
                claims[id] = colonist;
                taken++;
            }
            eating.Add(colonist);
            return taken;
        }
        int first = TopUp((UWGame.SimSide.Entities.EntityID)101, 3);
        Check(first == 2, $"one colonist alone takes the two above the reserve ({first})");
        for (int c = 102; c <= 104; c++)
        {
            TopUp((UWGame.SimSide.Entities.EntityID)c, 3);
        }
        int left = shelf.Count - claims.Count;
        Check(left == 10, $"four colonists eating at once leave the ten reserved ({left} left; counting the fish on other plates as stock left 4)");
        Check(UWGame.Mods.ReserveMod.CountForMeal(shelf, ClaimOf, eating.Contains, (UWGame.SimSide.Entities.EntityID)101) == 12,
              "a colonist's own claimed fish still count for it - its alreadyTaken subtracts them once");
        eating.Clear();
        Check(UWGame.Mods.ReserveMod.CountForMeal(shelf, ClaimOf, eating.Contains, null) == 12,
              "a claim by someone not eating (a haul) is still stock");

        // Starving, for the reserve's exception (EvaluateEat.IsStarvingOfEssentials). Kastuk: reserved
        // crystal wine was drunk freely. A colonist out of STIMULANTS counted as starving - the studio's
        // GetLowestFoodLevel takes every food need - and stimulants are what wine is for.
        var person = types.Values.FirstOrDefault(t => t.Person != null && t.BiologicalType != null);
        var needs = person?.BiologicalType.GetAdultNeedsAndWeight(out float _) ?? Array.Empty<UWGame.SimSide.AI.Needs.NeedType>();
        var stimulants = needs.FirstOrDefault(n => n.FoodNeedType != null && !n.FoodNeedType.IsEssential);
        Check(stimulants?.KeyName == "stimulants", $"{person?.KeyName}'s one non-essential food need is stimulants ({stimulants?.KeyName})");
        Check(types.TryGetValue("item:crystalWine", out var wine)
              && wine.ItemType.FoodType.FoodNutrientProfile.FoodNutrientTypes.Any(a => a.Amount > 0f && a.Nutrient?.KeyName == stimulants?.FoodNeedType.FoodNutrient),
              "crystal wine feeds that need");
        var fedButDry = needs.Select(n => (n, n == stimulants ? 0f : 0.5f)).ToList();
        Check(fedButDry.Min(p => p.Item2) == 0f && !UWGame.SimSide.AI.Goals.EvaluateEat.IsStarvingOfEssentials(fedButDry),
              "fed but out of stimulants: the lowest food need is 0, yet the reserve does not count it as starving");
        var hungry = needs.Select(n => (n, n.FoodNeedType != null && n.FoodNeedType.IsEssential ? 0f : 0.5f)).ToList();
        Check(UWGame.SimSide.AI.Goals.EvaluateEat.IsStarvingOfEssentials(hungry), "an essential food need at 0 is starving");
        Console.WriteLine(failures == 0 ? "reserve self-test OK" : $"reserve self-test FAILED - {failures} check(s)");
        return failures == 0 ? 0 : 1;
    }

    /// <summary>
    /// Checks TradeMod's accounting: the switch is read from the settings file, the agreed prices
    /// of an unpaid sale survive the expedition's saved custom fields exactly (decimal, in any
    /// culture) and clear, and loading pays the agreed price of what is aboard and nothing for what
    /// is left behind. Reports which way the switch is, so the gate can run it both ways.
    /// </summary>
    private static int TradeSelfTest()
    {
        int rc = Run(Sim.SerializeMode.NoSerialize, "base tables, the way the game loads them");
        if (rc != 0) return rc;
        Console.WriteLine("==> trade self-test");
        int failures = 0;
        void Check(bool ok, string what)
        {
            Console.WriteLine((ok ? "  ok    " : "  FAIL  ") + what);
            if (!ok) failures++;
        }
        Console.WriteLine(UWGame.Mods.TradeMod.PayOnPickupSetting.On
            ? "  switch on: sales are paid when the barge loads"
            : "  switch off: sales are paid when the run starts");
        Check(UWGame.Mods.TradeMod.PayOnPickupSetting.DefaultValue == "false"
              && UWGame.Mods.TradeMod.PayOnPickupSetting.AffectsSimulation,
              "trade.payOnPickup is off by default and is in the save signature");

        const string fish = "item:smokedStreakFin", wine = "item:crystalWine", nails = "item:nails";
        var agreed = new Dictionary<string, decimal> { [fish] = 12.5m, [wine] = 0.1m, [nails] = 3m };
        System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.CurrentCulture;
        string encoded;
        try
        {
            // A culture with a decimal comma: the entry must not depend on the player's locale.
            System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("de-DE");
            encoded = UWGame.Mods.TradeMod.EncodePrices(agreed);
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = culture;
        }
        Check(encoded == "item:crystalWine=0.1;item:nails=3;item:smokedStreakFin=12.5",
              "agreed prices are written in a fixed order, invariant, exact: " + encoded);
        var decoded = UWGame.Mods.TradeMod.DecodePrices(encoded);
        Check(decoded.Count == 3 && decoded[fish] == 12.5m && decoded[wine] == 0.1m && decoded[nails] == 3m,
              "and read back as the same decimals");

        var expedition = new UWGame.SimSide.Expeditions.Expedition();
        var contract = (UWGame.SimSide.Overland.Missions.Templates.ContractTemplateID)42L;
        Check(UWGame.Mods.TradeMod.GetAgreedPrices(expedition, contract) == null, "a new expedition has no unpaid sale");
        UWGame.Mods.TradeMod.SetAgreedPrices(expedition, contract, agreed);
        Check(expedition.GetPropertyValue("trade:due:42", null, null)?.StringResult == encoded,
              "an unpaid sale is kept in the expedition's saved custom fields, under trade:due:<contract id>");
        var back = UWGame.Mods.TradeMod.GetAgreedPrices(expedition, contract);
        Check(back != null && back.Count == 3 && back[fish] == 12.5m, "and read back from there");
        UWGame.Mods.TradeMod.SetAgreedPrices(expedition, contract, null);
        Check(UWGame.Mods.TradeMod.GetAgreedPrices(expedition, contract) == null
              && expedition.GetPropertyValue("trade:due:42", null, null) == null, "paying it removes the field");

        var taken = UWGame.Mods.TradeMod.Pickup.Taken;
        var left = UWGame.Mods.TradeMod.Pickup.LeftBehind;
        Check(UWGame.Mods.TradeMod.AmountDue(agreed, new[] { (fish, taken), (fish, taken), (wine, taken), (nails, taken) }) == 28.1m,
              "everything aboard: the agreed total, 2 x 12.5 + 0.1 + 3 = 28.1");
        Check(UWGame.Mods.TradeMod.AmountDue(agreed, new[] { (fish, taken), (fish, left), (wine, taken), (nails, left) }) == 12.6m,
              "half aboard: only what is aboard is paid, 12.5 + 0.1 = 12.6");
        Check(UWGame.Mods.TradeMod.AmountDue(agreed, new[] { (fish, left), (nails, left) }) == 0m,
              "nothing aboard: nothing is paid");
        Check(UWGame.Mods.TradeMod.AmountDue(agreed, new[] { ("item:notSold", taken) }) == 0m,
              "an item the sale never priced is not paid for");

        // Kastuk: the ETA "may be less exact to be more roleplayish, like 'half a day' or 'a pair
        // of hours'". Display only, so it must stay out of the save signature.
        var rough = UWGame.Mods.TradeMod.RoughEtaSetting;
        Check(rough.DefaultValue == "true" && !rough.AffectsSimulation, "trade.roughEta is on by default and not in the save signature");
        string R(double hours) => UWGame.Mods.TradeMod.RoughInterval(hours / 24.0);
        Check(R(0.5) == "within the hour" && R(2) == "a couple of hours" && R(5) == "a few hours" && R(12) == "half a day"
              && R(24) == "about a day" && R(40) == "a day or two" && R(96) == "a few days" && R(192) == "about a week" && R(300) == "over a week",
              $"rough ETAs: 0.5 h {R(0.5)}, 2 h {R(2)}, 5 h {R(5)}, 12 h {R(12)}, 1 d {R(24)}, 40 h {R(40)}, 4 d {R(96)}, 8 d {R(192)}, 12.5 d {R(300)}");
        string was = rough.Value;
        rough.Value = "false";
        Check(UWGame.Mods.TradeMod.ArrivalIn(1.25) == new DateAndTime.TimeDateYear(1.25).ToIntervalString(),
              $"switch off: the studio's interval form ({UWGame.Mods.TradeMod.ArrivalIn(1.25)})");
        rough.Value = "true";
        Check(UWGame.Mods.TradeMod.ArrivalIn(0.5) == "half a day", $"switch on: in words (0.5 days: {UWGame.Mods.TradeMod.ArrivalIn(0.5)})");
        Check(UWGame.Mods.TradeMod.ArrivalAt(new DateAndTime.TimeDateYear(3.8)) == " in the evening"
              && UWGame.Mods.TradeMod.ArrivalAt(new DateAndTime.TimeDateYear(3.05)) == " at night",
              "...and the tooltip names the part of the day, not the clock");
        rough.Value = was;

        // DebugMod's credits key, for testing purchases the colony cannot afford (Kastuk, "Dogs and
        // robots": the GOPHER). It goes through a command so that replays carry it.
        Check(UWGame.Mods.DebugMod.CreditsKeyOnSetting.DefaultValue == "false" && !UWGame.Mods.DebugMod.CreditsKeyOnSetting.AffectsSimulation,
              "debug.creditsKeyOn is off by default");
        Check(UWGame.Mods.DebugMod.CreditsKey.KeyValue == Microsoft.Xna.Framework.Input.Keys.F9 && UWGame.Mods.DebugMod.CreditsAmount == 1000m,
              $"F9 adds 1000 unless changed (got {UWGame.Mods.DebugMod.CreditsKey.KeyValue}, {UWGame.Mods.DebugMod.CreditsAmount})");
        var colony = new UWGame.SimSide.Allegiances.Allegiance { TradeCredits = 250m };
        UWGame.Mods.DebugMod.AddCredits(colony, 1000m);
        Check(colony.TradeCredits == 1250m, $"AddCredits adds to what the colony has (250 + 1000 = {colony.TradeCredits})");
        var poor = new UWGame.SimSide.Allegiances.Allegiance();
        UWGame.Mods.DebugMod.AddCredits(poor, 500m);
        Check(poor.TradeCredits == 500m, "...and to a colony that has never had credits");
        var creditsSerializer = new System.Xml.Serialization.XmlSerializer(typeof(List<UWGame.Control.Commands.Command>));
        var creditsWriter = new System.IO.StringWriter();
        creditsSerializer.Serialize(creditsWriter, new List<UWGame.Control.Commands.Command>
        {
            new UWGame.SimSide.Commands.AddCredits((UWGame.SimSide.Allegiances.AllegianceID)3L, 5000m),
        });
        var creditsBack = (creditsSerializer.Deserialize(new System.IO.StringReader(creditsWriter.ToString())) as List<UWGame.Control.Commands.Command>)
            ?.FirstOrDefault() as UWGame.SimSide.Commands.AddCredits;
        Check(creditsBack != null && creditsBack.AllegianceID == 3L && creditsBack.Amount == 5000m, "AddCredits round-trips through the replay serializer");

        Console.WriteLine(failures == 0 ? "trade self-test OK" : $"trade self-test FAILED - {failures} check(s)");
        return failures == 0 ? 0 : 1;
    }

    /// <summary>
    /// Checks RegrowthMod: every wood key it names is a resource in the table - a renamed key would
    /// silently exclude that wood - and its curve is 0.5 with nothing left, 1 from 40% up, and
    /// never goes down as more is left. The resource table only half-builds in this tool (its
    /// renderables need a client), but the keys are in it before that step fails.
    /// </summary>
    private static int RegrowthSelfTest()
    {
        int rc = Run(Sim.SerializeMode.NoSerialize, "base tables, the way the game loads them");
        if (rc != 0) return rc;
        Console.WriteLine("==> regrowth self-test");
        int failures = 0;
        void Check(bool ok, string what)
        {
            Console.WriteLine((ok ? "  ok    " : "  FAIL  ") + what);
            if (!ok) failures++;
        }
        var keys = UWGame.Mods.RegrowthMod.PlantKeys;
        if (keys.Length == 0)
        {
            Console.WriteLine("  the mod is not in this build - nothing to check");
            return 0;
        }
        foreach (string key in keys)
        {
            Check(GameData.Instance.AllResourceTypes.ContainsKey(key), key + " is a resource in the table");
        }
        Check(UWGame.Mods.RegrowthMod.Curve(0f) == 0.5f, "nothing left: x0.5");
        Check(UWGame.Mods.RegrowthMod.Curve(0.4f) == 1f && UWGame.Mods.RegrowthMod.Curve(1f) == 1f, "40% left and above: x1");
        float mid = UWGame.Mods.RegrowthMod.Curve(0.3f);
        Check(mid > 0.8f && mid < 1f, FormattableString.Invariant($"30% left: x{mid:0.##}, between"));
        bool rising = true;
        for (int i = 1; i <= 100; i++)
        {
            rising &= UWGame.Mods.RegrowthMod.Curve(i / 100f) >= UWGame.Mods.RegrowthMod.Curve((i - 1) / 100f);
        }
        Check(rising, "never goes down as more is left");
        // Kastuk, 2026-10-04: the six living-plant resources, a 3-tile zone, firewood at most -20%.
        foreach (string key in new[] { "crop:shadeleafCanes", "crop:wingweedLeaves", "crop:waterCaneStem", "crop:waterCaneLeaves", "crop:daysheenLeaves", "firegrassSod" })
        {
            Check(Array.IndexOf(keys, key) >= 0, key + " regrows slowly when gathered down");
        }
        Check(Array.IndexOf(keys, "commonOilTubers") < 0 && Array.IndexOf(keys, "spottedOilTubers") < 0, "oil tubers do not (early survival)");
        Check(UWGame.Mods.RegrowthMod.ZoneRadius == 3, "the zone is 3 tiles around the place");
        var firewoodType = GameData.Instance.AllResourceTypes["firewood"];
        var sticksType = GameData.Instance.AllResourceTypes["crop:sticks"];
        float firewoodFloor = UWGame.Mods.RegrowthMod.Floor(firewoodType), sticksFloor = UWGame.Mods.RegrowthMod.Floor(sticksType);
        Check(UWGame.Mods.RegrowthMod.Curve(0f, firewoodFloor) == 0.8f && UWGame.Mods.RegrowthMod.Curve(0.4f, firewoodFloor) == 1f,
              FormattableString.Invariant($"firewood, nothing left: x{UWGame.Mods.RegrowthMod.Curve(0f, firewoodFloor):0.##} (-20%), x1 from 40%"));
        Check(UWGame.Mods.RegrowthMod.Curve(0f, sticksFloor) == 0.5f, "sticks, nothing left: x0.5, as before");

        // The Gather window's Regrowth column: the studio's "+x.x" with the switch off; with it on
        // "+current" rounded down to a whole number, short enough for a column 50 pixels wide even
        // at 100 a year - the max lives only in the tooltip; "max." left alone.
        var overharvest = UWGame.Mods.RegrowthMod.WoodOverharvest;
        string was = overharvest.Value;
        overharvest.Value = "false";
        Check(UWGame.Mods.RegrowthMod.RegrowthLabel(12f, false) == null
              && UWGame.Mods.RegrowthMod.RegrowthToolTipNote(GameData.Instance.AllResourceTypes[keys[0]]) == null,
              "switch off: the studio's regrowth text and tooltip");
        Check(UWGame.Mods.RegrowthMod.ForecastRegrowth(null, 7f) == 7f, "switch off: the forecast is the studio's figure");
        overharvest.Value = "true";
        string label = UWGame.Mods.RegrowthMod.RegrowthLabel(12.9f, false);
        Check(label == "+12", $"switch on: 12.9 reads \"{label}\", rounded down, no max");
        label = UWGame.Mods.RegrowthMod.RegrowthLabel(4.25f, false);
        Check(label == "+4", $"switch on: 4.25 reads \"{label}\", no decimals below 10 either");
        label = UWGame.Mods.RegrowthMod.RegrowthLabel(99.99f, false);
        Check(label == "+99", $"switch on: 99.99 reads \"{label}\", a big zone still fits");
        label = UWGame.Mods.RegrowthMod.RegrowthLabel(0.4f, false);
        Check(label == "+0", $"switch on: 0.4 reads \"{label}\"");
        Check(UWGame.Mods.RegrowthMod.RegrowthLabel(0f, true) == null, "switch on: a full zone keeps the studio's \"max.\"");
        Check(UWGame.Mods.RegrowthMod.RegrowthToolTipNote(GameData.Instance.AllResourceTypes[keys[0]])?.Contains("rounded down") == true,
              "switch on: the tooltip says the column is rounded down");
        Check(UWGame.Mods.RegrowthMod.ForecastRegrowth(null, 7f) == 7f, "switch on: no tile, nothing to slow");
        overharvest.Value = was;

        // BirdHopMod, the other nature mod: its premise is that each hopper is a real creature the
        // studio made immobile but gave legs. If a data change made one mobile, the mod's branch
        // would silently stop mattering; if one lost its legs, the step would go nowhere.
        foreach (string key in UWGame.Mods.BirdHopMod.HopperKeys)
        {
            GameData.Instance.AllEntityTypes.TryGetValue(key, out var hopper);
            Check(hopper?.IntelligenceType != null && !hopper.IntelligenceType.IsMobile
                  && hopper.LocomotorType?.LeggedLocomotorType != null,
                  key + " is an immobile creature with legs (BirdHopMod)");
        }

        // HomeRaidMod: every raider must be a mobile creature that can attack, with an aggro range
        // to look for buildings in - a key that stopped matching would silently raid nothing.
        foreach (string key in UWGame.Mods.HomeRaidMod.RaiderKeys)
        {
            GameData.Instance.AllEntityTypes.TryGetValue(key, out var raider);
            Check(raider?.IntelligenceType != null && raider.IntelligenceType.IsMobile
                  && raider.IntelligenceType.CanAttack == true && (raider.IntelligenceType.AggroRange ?? 0f) > 0f,
                  key + " is a mobile creature that attacks, with an aggro range (HomeRaidMod)");
        }
        Console.WriteLine(failures == 0 ? "regrowth self-test OK" : $"regrowth self-test FAILED - {failures} check(s)");
        return failures == 0 ? 0 : 1;
    }

    /// <summary>
    /// Checks that HomeRaidMod's predators raid only what they know. tripleacoder: "RaidableBuildings
    /// should probably scan known entities, otherwise the predators become omniscient." A building
    /// is known when it is in the predator's allegiance's SharedKnowledge.AllKnownEntities - put
    /// there by the game's own seeing step (SharedKnowledge.AddToCollectionsOfKnownEntities, called
    /// by SeeDetectable when a sensor sees it) and taken out by its own forgetting
    /// (DeleteMemoryOfEntity). That memory only holds what Entity.HasInterestInEntity lets it hold,
    /// so the raiders must take an interest in homes and stores with the mod on - and only then.
    /// </summary>
    private static int HomeRaidSelfTest()
    {
        int rc = Run(Sim.SerializeMode.NoSerialize, "base tables, the way the game loads them");
        if (rc != 0) return rc;
        Console.WriteLine("==> home raid self-test");
        int failures = 0;
        void Check(bool ok, string what)
        {
            Console.WriteLine((ok ? "  ok    " : "  FAIL  ") + what);
            if (!ok) failures++;
        }
        if (UWGame.Mods.HomeRaidMod.RaiderKeys.Length == 0)
        {
            Console.WriteLine("  the mod is not in this build - nothing to check");
            return 0;
        }
        var types = GameData.Instance.AllEntityTypes;
        var home = types.Values.FirstOrDefault(t => t.StructureType != null && t.ContainerType is UWGame.SimSide.Entities.Containers.Components.HomeContainerType);
        var store = types.Values.FirstOrDefault(t => t.StructureType != null && t.ContainerType is UWGame.SimSide.Entities.Containers.Components.StorageContainerType);
        Check(home != null && store != null, $"a home ({home?.KeyName}) and a store ({store?.KeyName}) are in the tables");
        if (home == null || store == null)
        {
            Console.WriteLine($"home raid self-test FAILED - {failures} check(s)");
            return 1;
        }
        var raiders = UWGame.Mods.HomeRaidMod.RaiderKeys.Select(k => types.TryGetValue(k, out var t) ? t : null).ToList();
        Check(raiders.All(t => t != null), "every raider is in the tables");
        raiders.RemoveAll(t => t == null);

        var enabled = UWGame.Mods.HomeRaidMod.EnabledSetting;
        string was = enabled.Value;
        enabled.Value = "false";
        foreach (var raider in raiders)
        {
            Check(!UWGame.Mods.HomeRaidMod.TakesInterestIn(raider, home) && !UWGame.Mods.HomeRaidMod.TakesInterestIn(raider, store),
                  "switch off: " + raider.KeyName + " keeps the studio's memory, no added interest in buildings");
        }
        enabled.Value = "true";
        foreach (var raider in raiders)
        {
            Check(raider.SensorType != null, raider.KeyName + " has a sensor to see buildings with");
            Check(UWGame.Mods.HomeRaidMod.TakesInterestIn(raider, home) && UWGame.Mods.HomeRaidMod.TakesInterestIn(raider, store),
                  "switch on: " + raider.KeyName + " remembers the homes and stores it sees");
        }
        if (types.TryGetValue("entity:bushDragon", out var dragon))
        {
            Check(!UWGame.Mods.HomeRaidMod.TakesInterestIn(dragon, store), "...and the bush dragon, which does not raid, does not");
        }
        types.TryGetValue("entity:swarmer", out var swarmer);
        Check(raiders.Count > 0 && swarmer != null && !UWGame.Mods.HomeRaidMod.TakesInterestIn(raiders[0], swarmer),
              "...and only in buildings, not in creatures (entity:swarmer)");

        // The predator's kind's memory, as the game keeps it. The buildings are made the way a save
        // loads one - the bare Entity() with an ID and a type - because the full constructor wants a
        // running client, which this tool does not start.
        UWGame.SimSide.Entities.Entity.CreateLookupCollection();
        UWGame.SimSide.Entities.Entity Building(UWGame.SimSide.Entities.EntityType type)
        {
            var building = new UWGame.SimSide.Entities.Entity();
            building.AddToLookup();
            typeof(UWGame.SimSide.Entities.Entity).GetProperty(nameof(building.EntityType)).SetValue(building, type);
            return building;
        }
        var knowledge = new UWGame.SimSide.AI.SharedKnowledge
        {
            AllKnownEntities = new UWGame.SimSide.Entities.EntityGroup(),
            EntityLocks = new Dictionary<UWGame.SimSide.Entities.EntityID, UWGame.SimSide.AI.EntityLock>(),
            SpecialActionLocks = new Dictionary<UWGame.SimSide.Entities.EntityID, List<UWGame.SimSide.Processes.ProcessType>>(),
        };
        var seen = Building(store);
        var unseen = Building(home);
        Check(!UWGame.Mods.HomeRaidMod.Knows(knowledge.AllKnownEntities, seen) && !UWGame.Mods.HomeRaidMod.Knows(knowledge.AllKnownEntities, unseen),
              "a building the predator's kind has never perceived is not a raid target");
        knowledge.AddToCollectionsOfKnownEntities(seen);
        Check(UWGame.Mods.HomeRaidMod.Knows(knowledge.AllKnownEntities, seen),
              "one it has perceived is (SharedKnowledge.AddToCollectionsOfKnownEntities, the step SeeDetectable takes)");
        Check(!UWGame.Mods.HomeRaidMod.Knows(knowledge.AllKnownEntities, unseen), "...and perceiving one building reveals no other");
        knowledge.DeleteMemoryOfEntity(seen.ID, null, removeAllKnowledge: true);
        Check(!UWGame.Mods.HomeRaidMod.Knows(knowledge.AllKnownEntities, seen),
              "once forgotten (SharedKnowledge.DeleteMemoryOfEntity) it is not a target again");
        Check(!UWGame.Mods.HomeRaidMod.Knows(null, seen) && !UWGame.Mods.HomeRaidMod.Knows(knowledge.AllKnownEntities, null),
              "no knowledge or no building: no target");

        // Kastuk: colonists in their huts spotted every raider long before it reached the door.
        // HomeRaidMod.SensorFactorInside is a share of the sensor range for the player's people
        // inside a residence. Putting a colonist inside one needs a running Sim; checked here are
        // the setting, the cases that must stay at 1, and which containers count as a home.
        var sight = UWGame.Mods.HomeRaidMod.SightFromHome;
        Check(sight.Value == "1/3" && sight.StockValue == "ALL THE WAY",
              $"COLONISTS INSIDE A HOME SEE: 1/3 by default, all the way in stock (got {sight.Value}, {sight.StockValue})");
        Check(UWGame.Mods.HomeRaidMod.SightFraction("1/3") == 1f / 3f && UWGame.Mods.HomeRaidMod.SightFraction("1/4") == 0.25f
              && UWGame.Mods.HomeRaidMod.SightFraction("ALL THE WAY") == 1f && UWGame.Mods.HomeRaidMod.SightFraction("1/0") == 1f,
              "1/3 reads as a third; ALL THE WAY, and anything unreadable, as all of it");
        Check(UWGame.Mods.HomeRaidMod.SensorFactorInside(null) == 1f, "no entity: all the way");
        enabled.Value = "false";
        Check(UWGame.Mods.HomeRaidMod.SensorFactorInside(Building(home)) == 1f,
              "switch off: all the way, decided before the entity is looked at");
        enabled.Value = "true";
        var game = typeof(UWGame.SimSide.Entities.Entity).Assembly;
        var residence = game.GetType("UWGame.SimSide.Entities.Containers.IResidence");
        bool IsResidence(string container) =>
            residence != null && residence.IsAssignableFrom(game.GetType("UWGame.SimSide.Entities.Containers.Components." + container));
        Check(IsResidence("HomeContainer"), "a home (HomeContainer) is a residence: inside it, sight is shortened");
        Check(!IsResidence("UpgradableBuildingContainer") && !IsResidence("VehicleContainer"),
              "a workshop (UpgradableBuildingContainer) and a vehicle are not, though both hold people: full sight");
        enabled.Value = was;

        Console.WriteLine(failures == 0 ? "home raid self-test OK" : $"home raid self-test FAILED - {failures} check(s)");
        return failures == 0 ? 0 : 1;
    }

    /// <summary>
    /// Checks the balanced diet mod's preserved-food and alcohol profiles on the items themselves,
    /// with its switches as the file has them (both on by default): smoked and dried items on
    /// preserved copies with 40% of the fresh micronutrients, wine and brandy on 'alcohol' with the
    /// studio's stimulant plus a third of the crystal berries' energy, pickled food untouched - and
    /// then the validation pass, because new profiles are new data.
    /// </summary>
    private static int DietSelfTest()
    {
        int rc = Run(Sim.SerializeMode.NoSerialize, "base tables, the way the game loads them");
        if (rc != 0) return rc;
        Console.WriteLine("==> balanced diet self-test");
        int failures = 0;
        void Check(bool ok, string what)
        {
            Console.WriteLine((ok ? "  ok    " : "  FAIL  ") + what);
            if (!ok) failures++;
        }
        var items = GameData.Instance.AllEntityTypes;
        var profiles = GameData.Instance.AllFoodNutrientProfiles;
        UWGame.SimSide.Items.FoodNutrientProfile ProfileOf(string key) =>
            items.TryGetValue(key, out var t) ? t.ItemType?.FoodType?.FoodNutrientProfile : null;
        float Amount(UWGame.SimSide.Items.FoodNutrientProfile p, string nutrient) =>
            p?.FoodNutrientTypes?.Where(a => a?.Nutrient?.KeyName == nutrient).Sum(a => a.Amount) ?? 0f;

        var fresh = profiles.TryGetValue("richMeat", out var r) ? r : null;
        var smoked = ProfileOf("item:smokedStreakFin");
        Check(smoked?.KeyName == "preservedRichMeat", $"smokedStreakFin uses {smoked?.KeyName ?? "nothing"}");
        Check(fresh != null && Math.Abs(Amount(smoked, "micronutrients") - 0.4f * Amount(fresh, "micronutrients")) < 1e-6f
              && Amount(smoked, "protein") == Amount(fresh, "protein") && Amount(smoked, "foodEnergy") == Amount(fresh, "foodEnergy"),
              "...with 40% of richMeat's micronutrients and the same protein and energy");
        Check(ProfileOf("item:smokedAlabasterRay")?.KeyName == "preservedMediumMeat", "smokedAlabasterRay uses preservedMediumMeat");
        Check(ProfileOf("item:pickledCarbonTail")?.KeyName == "richMeat", "pickledCarbonTail is untouched (richMeat)");

        var wine = ProfileOf("item:crystalWine");
        var berries = profiles.TryGetValue("highEnergy", out var h) ? h : null;
        var stimulant = profiles.TryGetValue("lowStimulant", out var s) ? s : null;
        Check(wine?.KeyName == "alcohol" && ProfileOf("item:crystalBrandy")?.KeyName == "alcohol", "wine and brandy use 'alcohol'");
        Check(Math.Abs(Amount(wine, "foodEnergy") - Amount(berries, "foodEnergy") / 3f) < 1e-6f && Amount(wine, "foodEnergy") > 0f,
              FormattableString.Invariant($"...with a third of the crystal berries' energy ({Amount(wine, "foodEnergy"):0.###} of {Amount(berries, "foodEnergy"):0.###})"));
        Check(Amount(wine, "stimulants") == Amount(stimulant, "stimulants") && Amount(wine, "stimulants") > 0f, "...and the studio's stimulant");

        // Monotony: servings past the third of a dish, anywhere in the last 10 meals, over 7.
        float T(params string[] meals) => UWGame.Mods.BalancedDietMod.TirednessOf(meals);
        string[] Repeat(string dish, int n) => Enumerable.Repeat(dish, n).ToArray();
        Check(T() == 0f && T(Repeat("a", 3)) == 0f, "no memory, or a dish three times: not tired");
        Check(T(Repeat("a", 10)) == 1f, "one dish at all 10 meals: fully tired");
        Check(Math.Abs(T(Repeat("a", 5).Concat(new[] { "b", "c", "d", "e", "f" }).ToArray()) - 2f / 7f) < 1e-6f,
              "one dish five times among five others: 2/7 - counted across the memory, not in a row");
        Check(Math.Abs(T("a", "b", "a", "b", "a", "b", "a", "b", "a", "b") - 4f / 7f) < 1e-6f,
              "two dishes alternating: 4/7 - a little variation is still monotony");

        bool valid = ValidateDataComplete();
        Console.WriteLine(failures == 0 && valid ? "balanced diet self-test OK" : $"balanced diet self-test FAILED - {failures} check(s)");
        return failures == 0 && valid ? 0 : 1;
    }

    /// <summary>
    /// Checks FishStockMod against the tables the game builds.
    ///
    /// With the mod on: fishTrapSpawningLoop's spawning set must carry exactly one extra condition
    /// on fishStockAvailable and one extra SetPropertyAction on fishStock - the mod EXTENDS the
    /// studio's event, and a studio event it no longer recognises is left alone, which this would
    /// catch as a mod that silently does nothing. Every fish trap must have a fishing place in the
    /// tables, traps sharing a place must share one best rate, and the regrowth arithmetic must
    /// grow, cap, and ignore time running backwards. With it off (the default): the event must be
    /// the studio's, untouched.
    /// </summary>
    private static int FishSelfTest(bool on)
    {
        UWGame.Mods.ModSetting enabled = UWGame.Mods.ModSettings.Find("fishstock.enabled");
        if (enabled == null)
        {
            Console.WriteLine("  the mod is not in this build - nothing to check");
            return 0;
        }
        enabled.Value = on ? "true" : "false";
        int rc = Run(Sim.SerializeMode.NoSerialize, "base tables, the way the game loads them");
        if (rc != 0) return rc;

        Console.WriteLine($"==> fish stock self-test (mod {(on ? "on" : "off")})");
        int failures = 0;
        void Check(bool ok, string what)
        {
            Console.WriteLine((ok ? "  ok    " : "  FAIL  ") + what);
            if (!ok) failures++;
        }

        GameData.Instance.AllPolledEvents.TryGetValue("fishTrapSpawningLoop", out var loop);
        var spawning = loop?.ActionSets?.SetsOfActions?.FirstOrDefault(
            s => s.Actions != null && s.Actions.Any(a => a is UWGame.SimSide.InGameEvents.Actions.SpawnEntityAction));
        Check(spawning != null, "fishTrapSpawningLoop has its spawning action set");
        if (spawning == null) return 1;

        bool hasStockAction = spawning.Actions.OfType<UWGame.SimSide.InGameEvents.Actions.SetPropertyAction>().Any(a => a.PropertyKey == "fishStock");
        bool hasStockCondition = ConditionMentions(spawning.Condition, "fishStockAvailable");
        if (on)
        {
            Check(hasStockCondition, "the spawn requires fishStockAvailable >= amountOfFish");
            Check(hasStockAction, "the spawn is followed by fishStock = fishStockAvailable - amountOfFish");
            Check(ConditionMentions(spawning.Condition, "freeStorage") && ConditionMentions(spawning.Condition, "spawnChance"),
                  "...and the studio's own conditions (freeStorage, spawnChance) are still there");

            var traps = GameData.Instance.AllEntityTypes.Values.Where(UWGame.Mods.FishStockMod.IsFishTrap).ToList();
            Check(traps.Count > 0, $"{traps.Count} fish trap types in the table");
            foreach (var trap in traps)
            {
                var places = UWGame.Mods.FishStockMod.PlacesFor(trap).ToList();
                float own = UWGame.Mods.FishStockMod.CatchPerPoll(trap);
                float best = UWGame.Mods.FishStockMod.BestCatchPerPoll(trap);
                Check(places.Count > 0 && best >= own && own > 0f,
                      FormattableString.Invariant($"{trap.KeyName,-34} own {own:0.###}/poll, best {best:0.###}/poll at {string.Join(", ", places.Select(p => p.KeyName))}"));
                foreach (var place in places)
                {
                    foreach (var sibling in UWGame.Mods.FishStockMod.TrapsBuiltAt(place))
                    {
                        if (UWGame.Mods.FishStockMod.BestCatchPerPoll(sibling) != best)
                        {
                            Check(false, $"  {sibling.KeyName} shares {place.KeyName} but not its best rate");
                        }
                    }
                }
            }

            Check(UWGame.Mods.FishStockMod.Regrow(0f, 0, 100, 10f, 0.05f) == 5f, "regrowth: 100 s at 0.05/s from empty is 5");
            Check(UWGame.Mods.FishStockMod.Regrow(9f, 0, 100, 10f, 0.05f) == 10f, "regrowth: capped at the maximum");
            Check(UWGame.Mods.FishStockMod.Regrow(4f, 100, 50, 10f, 0.05f) == 4f, "regrowth: time running backwards changes nothing");
        }
        else
        {
            Check(!hasStockCondition && !hasStockAction, "with the mod off, fishTrapSpawningLoop is the studio's, untouched");
        }

        Console.WriteLine(failures == 0 ? "fish stock self-test OK" : $"fish stock self-test FAILED - {failures} check(s)");
        return failures == 0 ? 0 : 1;
    }

    /// <summary>Whether a condition tree tests the named property anywhere.</summary>
    private static bool ConditionMentions(UWGame.SimSide.InGameEvents.Conditions.Condition condition, string propertyKey)
    {
        switch (condition)
        {
            case UWGame.SimSide.InGameEvents.Conditions.ConditionFunction f:
                return ConditionMentions(f.Left, propertyKey) || ConditionMentions(f.Right, propertyKey);
            case UWGame.SimSide.InGameEvents.Conditions.CustomCondition c:
                return c.PropertyCondition?.PropertyKey == propertyKey;
            default:
                return false;
        }
    }

    /// <summary>
    /// Checks DangerousFaunaMod against the creature table the game actually builds.
    ///
    /// The mod names its species by key and scales them where combat numbers are used, so the
    /// failure it cannot report for itself is a key that no longer matches anything: the row would
    /// sit in the menu and change nothing. This loads the base tables and asserts that every key
    /// is a real creature with an intelligence, that a setting reaches exactly the species it names
    /// - not the domesticated twinkler that shares the swarmer's attacks and body, not a person -
    /// and that a value the mod does not recognise falls back to x1.
    /// </summary>
    private static int FaunaSelfTest()
    {
        int rc = Run(Sim.SerializeMode.NoSerialize, "base tables, the way the game loads them");
        if (rc != 0) return rc;

        Console.WriteLine("==> dangerous fauna self-test");
        int failures = 0;
        void Check(bool ok, string what)
        {
            Console.WriteLine((ok ? "  ok    " : "  FAIL  ") + what);
            if (!ok) failures++;
        }

        var keys = UWGame.Mods.DangerousFaunaMod.SpeciesKeys.ToList();
        if (keys.Count == 0)
        {
            Console.WriteLine("  the mod is not in this build - nothing to check");
            return 0;
        }
        var types = GameData.Instance.AllEntityTypes;
        foreach (string key in keys)
        {
            Check(types.TryGetValue(key, out var t) && t.IntelligenceType != null,
                  key + " is a creature in the table");
        }

        foreach (UWGame.Mods.DangerousFaunaMod.Stat stat in Enum.GetValues(typeof(UWGame.Mods.DangerousFaunaMod.Stat)))
        {
            Check(UWGame.Mods.DangerousFaunaMod.Factor(types["entity:swarmer"], stat) == 1f,
                  $"swarmer {stat} is x1 by default");
        }

        UWGame.Mods.ModSetting damage = UWGame.Mods.DangerousFaunaMod.Setting("entity:swarmer", UWGame.Mods.DangerousFaunaMod.Stat.Damage);
        string before = damage.Value;
        damage.Value = "x2";
        Check(UWGame.Mods.DangerousFaunaMod.Factor(types["entity:swarmer"], UWGame.Mods.DangerousFaunaMod.Stat.Damage) == 2f,
              "swarmer DAMAGE x2 reaches the swarmer");
        Check(UWGame.Mods.DangerousFaunaMod.Factor(types["entity:twinkler"], UWGame.Mods.DangerousFaunaMod.Stat.Damage) == 1f,
              "...and not the twinkler, whose attacks it shares");
        if (types.TryGetValue("entity:domesticatedTwinkler", out var pet))
        {
            Check(UWGame.Mods.DangerousFaunaMod.Factor(pet, UWGame.Mods.DangerousFaunaMod.Stat.Damage) == 1f,
                  "...and not the domesticated twinkler, which shares its body and attacks");
        }
        Check(UWGame.Mods.DangerousFaunaMod.Factor(types["entity:human"], UWGame.Mods.DangerousFaunaMod.Stat.Damage) == 1f,
              "...and not a person");
        Check(UWGame.Mods.DangerousFaunaMod.Factor(types["entity:swarmer"], UWGame.Mods.DangerousFaunaMod.Stat.Fighting) == 1f,
              "...and not the swarmer's other stats");
        damage.Value = "x7";
        Check(damage.Value == "x1" && UWGame.Mods.DangerousFaunaMod.Factor(types["entity:swarmer"], UWGame.Mods.DangerousFaunaMod.Stat.Damage) == 1f,
              "an unknown value ('x7') falls back to x1");
        damage.Value = before;

        Console.WriteLine(failures == 0 ? "dangerous fauna self-test OK" : $"dangerous fauna self-test FAILED - {failures} check(s)");
        return failures == 0 ? 0 : 1;
    }

    /// <summary>
    /// Loads one user scenario exactly as NEW GAME does and fails if it could not start.
    ///
    /// Base tables in NoSerialize (the game's mode), then the scenario's DataLoader from
    /// AllScenarioLoader.GetScenarioDataLoader - the game's own choice of loader, not one made
    /// here - driven with no stepping past failures: a table that throws is a scenario that does
    /// not load. Then every key scenarioData.xml names is looked up where the game will look it
    /// up (Sim.ExecuteStartAction, Option.GetStartActions / GetEvents), and the validation pass
    /// runs, because tables that build are not tables that survive.
    ///
    /// This is the check that a user scenario was never able to pass: UserDataLoader had no
    /// tables and no folder, so 'spawnWorld' was not in AllEventActionTypes.
    /// </summary>
    private static int UserScenarioCheck(string folder)
    {
        int rc = Run(Sim.SerializeMode.NoSerialize, "base tables, the way the game loads them");
        if (rc != 0) return rc;

        UWGame.SimSide.Scenarios.Scenario header;
        try
        {
            DataLoader.DeserializeObject<UWGame.SimSide.Scenarios.Scenario>(
                UWGame.Config.GetDataFolderPath(UWGame.Config.DataType.UserScenarios, folder, "scenario.xml"), out header);
            if (header.Name != folder)
            {
                Console.WriteLine($"    FAIL scenario.xml says <Name>{header.Name}</Name>; it must be the folder name, '{folder}'");
                return 1;
            }
            header.ScenarioData = UWGame.SimSide.AllGameData.Scenarios.AllScenarioLoader.LoadScenarioData(header);
        }
        catch (Exception ex)
        {
            Console.WriteLine("    FAIL reading the scenario header: " + ex.GetBaseException().Message);
            return 1;
        }

        DataLoader loader = UWGame.SimSide.AllGameData.Scenarios.AllScenarioLoader.GetScenarioDataLoader(header);
        Console.WriteLine($"==> scenario tables for '{folder}' through {loader.GetType().Name}");
        Sim.CurrentSerializeMode = Sim.SerializeMode.NoSerialize;
        try
        {
            for (int steps = 0; !loader.QueueInitGameData(header); steps++)
            {
                if (steps > 500)
                {
                    Console.WriteLine("    FAIL QueueInitGameData did not report completion within 500 steps.");
                    return 1;
                }
            }
        }
        catch (Exception ex)
        {
            // The outer message first: DataLoader.ReadOwnTable wraps a table that will not read
            // with the file's name, and the root exception alone does not say which file it was.
            Exception root = ex.GetBaseException();
            Console.WriteLine("    FAIL " + ex.Message);
            if (root != ex) Console.WriteLine("         " + root.GetType().Name + ": " + root.Message);
            if (printTraces) Console.WriteLine(root.StackTrace);
            return 1;
        }

        var data = header.ScenarioData;
        var missing = new List<string>();
        void Need(string what, string key, System.Collections.IDictionary table)
        {
            if (!string.IsNullOrEmpty(key) && !table.Contains(key)) missing.Add(what + " '" + key + "'");
        }
        var actions = GameData.Instance.AllEventActionTypes;
        var events = GameData.Instance.AllPolledEvents;
        Need("SpawnWorldAction", data.SpawnWorldAction, actions);
        Need("SpawnSiteAction", data.SpawnSiteAction, actions);
        foreach (string key in data.Actions ?? Array.Empty<string>()) Need("Actions", key, actions);
        foreach (string key in data.ConditionalEvents ?? Array.Empty<string>()) Need("ConditionalEvents", key, events);
        int keys = 2 + (data.Actions?.Length ?? 0) + (data.ConditionalEvents?.Length ?? 0);
        foreach (var set in data.OptionSets ?? Array.Empty<UWGame.SimSide.Scenarios.OptionSet>())
        {
            foreach (var option in set.Options ?? Array.Empty<UWGame.SimSide.Scenarios.Option>())
            {
                foreach (string key in option.ActionKeys ?? Array.Empty<string>()) Need("option " + option.Name, key, actions);
                foreach (string key in option.ConditionalEvents ?? Array.Empty<string>()) Need("option " + option.Name, key, events);
                keys += (option.ActionKeys?.Length ?? 0) + (option.ConditionalEvents?.Length ?? 0);
            }
        }
        if (missing.Count > 0)
        {
            Console.WriteLine($"    FAIL {missing.Count} of {keys} keys named by scenarioData.xml are not defined:");
            foreach (string m in missing.Take(20)) Console.WriteLine("         " + m);
            return 1;
        }
        Console.WriteLine($"    ok - all {keys} keys named by scenarioData.xml resolve");

        return ValidateDataComplete() ? 0 : 1;
    }

    /// <summary>
    /// Writes each built-in scenario separately and says which ones survive.
    ///
    /// WHY THIS IS NOT JUST RGScenarioLoader.Serialize(). That method is what the game calls, and
    /// it is one unguarded foreach over nine loaders calling WriteScenario(). DataLoader.SerializeObject
    /// has no try/catch of its own, so the FIRST scenario that throws ends the loop and every
    /// scenario after it is silently never written - which reads, from the outside, as "the export
    /// only produced two scenarios" with nothing to say why. Same loaders, one try per scenario.
    ///
    /// Reflection into the private static list because it is the only handle on the loaders
    /// individually - RGScenarioLoader exposes them only through that loop and through lookups by
    /// a name you would have to know already. Same justification as the queueState field above:
    /// not something the game should do, exactly what a diagnostic tool should.
    /// </summary>
    private static int ScenarioReport()
    {
        Console.WriteLine();
        FieldInfo field = typeof(UWGame.SimSide.AllGameData.Scenarios.RGScenarioLoader)
            .GetField("rgScenarioLoaders", BindingFlags.Static | BindingFlags.NonPublic);
        if (field == null)
        {
            Console.Error.WriteLine("FATAL: RGScenarioLoader.rgScenarioLoaders not found; the field was renamed.");
            return 1;
        }

        var loaders = (System.Collections.IEnumerable)field.GetValue(null);
        int ok = 0, failed = 0;
        foreach (UWGame.SimSide.AllGameData.Scenarios.ScenarioLoader loader in loaders)
        {
            string name = loader.FolderName;
            try
            {
                // Two halves, and only the first one is what "export the scenario" used to mean.
                // WriteScenario puts scenario.xml and scenarioData.xml down; the loader below
                // puts down the tables those files REFER to, into the same folder.
                loader.WriteScenario();

                // The header and its data are loaded separately - AllScenarioLoader keeps them
                // apart - but DataLoader.QueueInitGameData reads scenario?.ScenarioData.EnableMissions,
                // where the ?. guards the SCENARIO being null and not its ScenarioData. Hand it a
                // header with no data attached and every table in the queue dies on that one line.
                var header = loader.GetScenarioHeader();
                header.ScenarioData = loader.GetScenarioData();

                int rc = RunLoader(loader.GetDataLoader(), header, Sim.SerializeMode.WriteAndRead,
                                   "    tables for " + name);
                Console.WriteLine(rc == 0 ? $"    ok      {name}" : $"    PARTIAL {name}  (some tables failed above)");
                ok++;
            }
            catch (Exception ex)
            {
                Exception root = ex;
                while (root.InnerException != null) root = root.InnerException;
                Console.WriteLine($"    FAILED  {name}  {root.GetType().Name}: {root.Message}");
                if (printTraces)
                {
                    Console.WriteLine(root.StackTrace);
                }
                failed++;
            }
        }

        Console.WriteLine();
        Console.WriteLine($"==> scenarios: {ok} written, {failed} failed  -> data/Scenarios/<name>/");
        Console.WriteLine("    Each folder holds scenario.xml and scenarioData.xml PLUS that scenario's own");
        Console.WriteLine("    tables - its event actions, entity types, polled events and hooks.");
        Console.WriteLine();
        Console.WriteLine("    Those tables are the half that is easy to miss. scenarioData.xml names its start");
        Console.WriteLine("    action ('spawnWorld') and its Actions as KEYS, and those keys are defined in the");
        Console.WriteLine("    SCENARIO's loader, not the base one - so a scenarioData.xml copied on its own");
        Console.WriteLine("    dies in Sim.ExecuteStartAction with KeyNotFoundException on the first one.");
        return failed == 0 ? 0 : 1;
    }

    /// <summary>
    /// Prints every disassembly recipe <c>UWGame.Mods.DisassemblyMod</c> generated on this load,
    /// one line each, in the order it generated them.
    ///
    /// This is the mod's offline witness. Its recipes are computed from the item table and the
    /// production recipes rather than written down, so "what does it actually produce" is a
    /// question about a running data load and not about a source file - and answering it by
    /// launching the game would mean reading twenty tooltips. The format is deliberately flat, one
    /// line per recipe with the item, the recipe it was derived from and what it gives back, so
    /// build/80-verify-modloader.sh can assert against it with grep.
    /// </summary>
    private static void DisassemblyReport()
    {
        Console.WriteLine();
        var generated = UWGame.Mods.DisassemblyMod.Generated;
        Console.WriteLine($"==> disassembly: {generated.Count} recipe(s) generated, " +
                          $"{UWGame.Mods.DisassemblyMod.Refused.Count} refused");

        // Refusals are the interesting half when something looks missing: an item that declares
        // its parts, or a recovery that would hand back more than the recipe consumed, is skipped
        // on purpose rather than silently absent.
        foreach (string reason in UWGame.Mods.DisassemblyMod.Refused)
        {
            Console.WriteLine("    refused: " + reason);
        }

        foreach (var process in generated)
        {
            string item = process.Inputs != null && process.Inputs.Length > 0
                ? process.Inputs[0].Entity : "?";

            // creates= is the one thing in this line that is about the SIM rather than the table.
            // A salvage process does not manufacture its outputs, it hands back the parts of the
            // entity it destroyed (SimProcess.CreateOutputsFromInputs), and for a long while a
            // recipe over an item that declares no parts destroyed the item and produced NOTHING -
            // reported from a game as a flint-tipped spear that vanished leaving no materials.
            // ProcessType.SalvageOutputWillBeCreated is the sim's own predicate, asked here so a
            // table can be checked without running a colony over it.
            var outputs = new List<string>();
            var lost = new List<string>();
            if (process.Outputs != null)
            {
                foreach (var output in process.Outputs)
                {
                    outputs.Add(output.EntityTypeToCreate + " x" + (output.Amount?.NoOfItems ?? 1));
                    if (!process.SalvageOutputWillBeCreated(output))
                    {
                        lost.Add(output.EntityTypeToCreate);
                    }
                }
            }

            // Whether the item actually points AT the recipe. Generating one and failing to hang
            // it on NonLivingType.SalvageProcess would give a recipe no player can ever reach,
            // and nothing else in this report would show the difference.
            string link = "link=BROKEN";
            if (GameData.Instance.AllEntityTypes.TryGetValue(item, out var entityType)
                && entityType.NonLivingType != null
                && entityType.NonLivingType.SalvageProcess == process.KeyName)
            {
                link = "link=ok";
            }

            // Invariant, because the gate greps these numbers and a machine with a comma for a
            // decimal point would print days=0,01 and match nothing.
            string days = (process.WorkOrTimeNeeded?.DaysNeeded ?? 0f).ToString(
                "0.########", System.Globalization.CultureInfo.InvariantCulture);
            // salvage= is not decoration: a generated recipe that forgot IsSalvageProcess would be
            // registered as a way to PRODUCE its outputs (GameData.AddProcessToProductionGraph
            // sorts by that flag), so a colony ordered to make sticks could answer by taking its
            // tools apart.
            string creates = lost.Count == 0 ? "creates=ok" : "creates=NOTHING:" + string.Join(",", lost);
            Console.WriteLine($"    {process.KeyName}  from={item}  skill={process.RequiredSkill}  " +
                              $"days={days}  {link}  salvage={(process.IsSalvageProcess ? "yes" : "NO")}  " +
                              $"{creates}  out={string.Join(", ", outputs)}");
        }
    }

    /// <summary>
    /// Checks the assumption RandomGenerator's save/load resume is built on: that a System.Random
    /// stream can be fast-forwarded by drawing and discarding, and that the count to draw is of
    /// VALUES rather than of method calls.
    ///
    /// Worth a test of its own because it is an assumption about the RUNTIME, not about this game
    /// - .NET is free to change how many internal samples NextBytes consumes, and if it ever does,
    /// every save written afterwards resumes in the wrong place and nothing else would notice. A
    /// desynchronised stream does not throw; it just quietly stops being the same game.
    /// </summary>
    private static int RandomSelfTest()
    {
        int failures = 0;

        void Check(string what, bool ok)
        {
            Console.WriteLine((ok ? "  ok    " : "  FAIL  ") + what);
            if (!ok) failures++;
        }

        Console.WriteLine("==> random stream resume");

        // Next() consumes exactly one value, so N draws then one more must match a fresh
        // generator fast-forwarded N times. This is the whole mechanism in one line.
        var a = new Random(12345);
        for (int i = 0; i < 1000; i++) a.Next();
        var b = new Random(12345);
        for (int i = 0; i < 1000; i++) b.Next();
        Check("1000 draws then resume: same next value", a.Next() == b.Next());

        // The one that actually matters. SimplexNoise.CreateSeedNumbers asks for 512 bytes at a
        // time, once per Personality. If NextBytes does not consume one value per byte, the
        // fast-forward lands 511 places short of where it should, per colonist.
        var c = new Random(999);
        c.NextBytes(new byte[512]);
        int afterBytes = c.Next();

        var d = new Random(999);
        for (int i = 0; i < 512; i++) d.Next();
        int afterDraws = d.Next();
        Check("NextBytes(512) consumes 512 values, not 1", afterBytes == afterDraws);

        // Mixed traffic, in the proportions the game produces: doubles, bounded ints, a byte
        // block. Each is one value except the block.
        var e = new Random(7);
        long samples = 0;
        for (int i = 0; i < 50; i++) { e.NextDouble(); samples++; }
        for (int i = 0; i < 50; i++) { e.Next(0, 10); samples++; }
        e.NextBytes(new byte[512]); samples += 512;
        for (int i = 0; i < 50; i++) { e.Next(100); samples++; }
        int afterMixed = e.Next();

        var f = new Random(7);
        for (long i = 0; i < samples; i++) f.Next();
        Check($"mixed traffic ({samples} values) resumes exactly", afterMixed == f.Next());

        // A seeded stream must also be the same from one PROCESS to the next, or none of this
        // means anything across a save. Pinned against values taken from this runtime.
        var g = new Random(2026);
        int[] first = { g.Next(1000), g.Next(1000), g.Next(1000) };
        var h = new Random(2026);
        Check("a seed gives the same sequence twice",
              first[0] == h.Next(1000) && first[1] == h.Next(1000) && first[2] == h.Next(1000));

        Console.WriteLine();
        Console.WriteLine(failures == 0
            ? "==> the resume mechanism holds on this runtime"
            : $"==> {failures} assumption(s) BROKEN - saves will resume in the wrong place");
        return failures;
    }

    /// <summary>
    /// The Sim the tables are built under, as the game always has one by the loading screen.
    ///
    /// Without it ResourceTypes threw a NullReferenceException in every run that did not make one
    /// first - the plain export and most self-tests: TileResourceType.Initialize asks
    /// RenderableTypeMode, which reads The.Sim.Mode to choose the game's or the editor's
    /// renderable. The loop below steps past a table that throws, so those runs went on with
    /// ResourceTypes half built (every type from the first tile resource on uninitialized) and
    /// EntityTypes failing after it on a key that table should have supplied. A self-test that
    /// needs the editor's mode, or a save's Sim, makes its own first and keeps it.
    /// </summary>
    private static void EnsureSim()
    {
        if (UWGame.The.Sim != null)
        {
            return;
        }
        UWGame.The.Sim = new Sim { Mode = Sim.EngineMode.Game };
        typeof(Sim).GetMethod("CreateLookupCollections", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(UWGame.The.Sim, null);
    }

    private static int Run(Sim.SerializeMode mode, string what)
    {
        // The load screen is the data layer's only dependency on the UI, and it is null here.
        // DataLoader.UpdateProgress tolerates that (PORT DEVIATION 13).
        return RunLoader(new BaseDataLoader(), null, mode, what);
    }

    /// <summary>
    /// Drives one DataLoader's queue to completion, stepping past any table that throws.
    ///
    /// Takes the loader rather than making one because there are TWO kinds. BaseDataLoader holds
    /// the game-wide tables; every RG scenario has a second, complete DataLoader of its own
    /// (Scenario1DataLoader and its eight siblings, Config.DataType.RGScenario) carrying that
    /// scenario's entity types, event actions, polled events and hooks. The start action a
    /// scenario names - "spawnWorld" for the tutorial - is defined in the SCENARIO's loader, not
    /// the base one, which is why a scenarioData.xml copied on its own dies in
    /// Sim.ExecuteStartAction with KeyNotFoundException.
    /// </summary>
    private static int RunLoader(DataLoader loader, UWGame.SimSide.Scenarios.Scenario scenario,
                                 Sim.SerializeMode mode, string what)
    {
        Console.WriteLine("==> " + what);
        Sim.CurrentSerializeMode = mode;
        EnsureSim();

        var sw = Stopwatch.StartNew();
        int steps = 0;
        var failures = new List<(DataLoaderQueueState State, string Error)>();

        // The queueState field only advances after a table's Handle* call returns, so a table
        // that throws would be retried forever. Stepping the field past it turns one run into a
        // complete list of what does and does not export, instead of a report on the first
        // problem only. Reflection into a private field is not something the game should do -
        // it is something a diagnostic tool should, and the alternative is 80 separate runs.
        FieldInfo queueStateField = typeof(DataLoader).GetField(
            "queueState", BindingFlags.Instance | BindingFlags.NonPublic);
        if (queueStateField == null)
        {
            Console.Error.WriteLine("FATAL: DataLoader.queueState not found; the field was renamed.");
            return 1;
        }

        // QueueInitGameData advances ONE table per call and returns false while there is more to
        // do; it returns true exactly once, on the DONE step. Note the polarity - it is a
        // "may I move on?", not a "keep going". Sim.QUEUESTATE.LOAD_BASE_DATA drives it the
        // same way, one call per frame.
        //
        // The cap guards against a state machine that never terminates. It is well above the
        // ~60 real steps, so hitting it means something is wrong, not that the data grew.
        while (true)
        {
            if (++steps > 500)
            {
                Console.Error.WriteLine("FATAL: QueueInitGameData did not report completion within 500 steps.");
                return 1;
            }

            var before = (DataLoaderQueueState)queueStateField.GetValue(loader);
            try
            {
                // null here means the BASE tables only. Game 1.0.4.8 added the parameter
                // (Sim.QueueGameDataAndSimInit passes the scenario from StartGameParams, or null
                // when there is none); --scenarios passes a real one, for the per-scenario loader.
                if (loader.QueueInitGameData(scenario)) break;
            }
            catch (Exception ex)
            {
                Exception root = ex.GetBaseException();
                failures.Add((before, root.GetType().Name + ": " + root.Message));
                Console.WriteLine($"    FAIL {before}");
                Console.WriteLine($"         {root.GetType().Name}: {Truncate(root.Message, 150)}");
                if (printTraces)
                {
                    // Which table failed is the tool.s contract; WHERE it failed is what a port
                    // maintainer needs, and two missing lookup collections were found with this.
                    Console.WriteLine(root.StackTrace);
                }

                var after = (DataLoaderQueueState)queueStateField.GetValue(loader);
                if (after != before)
                {
                    // It got far enough to advance itself; nothing to force.
                    continue;
                }
                if (before == DataLoaderQueueState.DONE)
                {
                    break;
                }
                queueStateField.SetValue(loader, before + 1);
            }
        }

        Console.WriteLine($"    {steps} step(s) in {sw.ElapsedMilliseconds} ms, " +
                          $"{failures.Count} table(s) failed");

        if (failures.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine("    Tables that cannot be exported:");
            foreach (var (state, error) in failures)
                Console.WriteLine($"      {state,-32} {Truncate(error, 110)}");
        }

        // Non-zero only when nothing worked: the self-tests load the tables through here and judge
        // what they need themselves. Every table exports today, and gate 80 case 35 holds the
        // plain export and read-back to that (docs/modding.md, "What actually round-trips today").
        return steps > 1 ? 0 : 1;
    }

    private static string Truncate(string s, int max) =>
        s == null ? "" : (s.Length <= max ? s : s.Substring(0, max - 3) + "...");

    private static void Report()
    {
        string dir = Path.Combine("data", "BaseData");
        if (!Directory.Exists(dir))
        {
            Console.WriteLine("    (no data/BaseData directory was created)");
            return;
        }

        var files = Directory.GetFiles(dir, "*.xml", SearchOption.AllDirectories)
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
            .ToList();

        long total = files.Sum(f => new FileInfo(f).Length);
        Console.WriteLine($"    {files.Count} XML file(s), {total / 1024} KB in {dir}");

        // An empty or single-element file usually means the table's Init() returned nothing,
        // which is worth seeing rather than counting as success.
        var suspicious = files.Where(f => new FileInfo(f).Length < 200).ToList();
        foreach (string f in suspicious)
            Console.WriteLine($"    ? {Path.GetFileName(f)} is only {new FileInfo(f).Length} bytes");

        foreach (string f in files.OrderByDescending(f => new FileInfo(f).Length).Take(10))
            Console.WriteLine($"      {new FileInfo(f).Length,9:N0}  {Path.GetRelativePath(dir, f)}");
    }
}
