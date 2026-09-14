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
            Console.Error.WriteLine("  --traces     print a stack trace for every table that fails to load or export.");
            return 2;
        }

        if (!Directory.Exists(target))
        {
            Console.Error.WriteLine($"FATAL: no such directory: {target}");
            return 1;
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
        UWGame.Mods.DisassemblyMod.RegisterSettings();
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
