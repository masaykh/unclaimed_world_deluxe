using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using UWGame.SimSide;
using UWGame.SimSide.AllGameData;

namespace UW.Tools.DataExport;

internal static class Program
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
        UWGame.Mods.SafeSleepMod.RegisterSettings();
        UWGame.Mods.GatherOnDemandMod.RegisterSettings();
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

        if (args.Contains("--pest-selftest"))
        {
            return PestSelfTest();
        }

        if (args.Contains("--reserve-selftest"))
        {
            return ReserveSelfTest();
        }

        if (args.Contains("--safesleep-selftest"))
        {
            return SafeSleepSelfTest();
        }

        if (args.Contains("--hunting-selftest"))
        {
            return HuntingSelfTest();
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
        Console.WriteLine(failures == 0 ? "hunting self-test OK" : $"hunting self-test FAILED - {failures} check(s)");
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
    /// out of Command's XmlInclude list breaks every replay that holds one), and the slider's top is
    /// what it says. Needs reserve.enabled on - the gate writes it.
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
        Console.WriteLine(failures == 0 ? "reserve self-test OK" : $"reserve self-test FAILED - {failures} check(s)");
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
        var keys = UWGame.Mods.RegrowthMod.WoodKeys;
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

        // Non-zero only when nothing worked. A partial export is the honest current state of
        // the game's data hooks, not a tool failure - see PORTING-NOTES.md.
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
