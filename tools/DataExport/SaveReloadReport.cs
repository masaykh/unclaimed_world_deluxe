using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using HarmonyLib;
using UWGame;
using UWGame.Control;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Vehicles;

namespace UW.Tools.DataExport;

/// <summary>
/// --save-reload=&lt;file.sav&gt;: what SAVE does to a running game, with no window.
///
/// WHY. SAVE in the game is a save followed at once by a load of the same file in the same
/// session (SaveLoadMessageBox.Update -> LoadGameDirectlyInGame -> Sim.LoadSavedGame), and that
/// load starts from the game it replaces, not from an empty one. The LOAD menu starts from an
/// emptied Sim. So a file can load from the menu and still kill SAVE: The Clay Pit, Making
/// Headway, Cudgel Hills and the two Muckroot maps all did (Kastuk, 2026-11-17), because the
/// vehicle slots of the old game were still registered when the same slots were read back.
///
/// HOW. The save is read and post-processed as --save-stuck does it, so it is a running game;
/// it is saved with Sim.DoSave, and then loaded through the game's own Sim.LoadSavedGame on its
/// thread - Snapshotter.Load with the old game still in memory. Headless stand-ins: renderables
/// are inert (PatchForHeadlessSim), Controller.RecreateClientAfterLoad builds no new Client, and
/// ScenarioData.LoadContent loads no map art. Everything else is the game's own code.
/// </summary>
internal static partial class Program
{
    private static string reloadReport;

    private static int SaveReloadReport(Snapshotter sn, Sim sim, string savePath)
    {
        int failures = 0;
        void Check(bool ok, string what)
        {
            Console.WriteLine((ok ? "  ok    " : "  FAIL  ") + what);
            if (!ok) failures++;
        }

        var renderables = FindField(typeof(UWGame.ClientSide.Client), "renderables");
        renderables.SetValue(The.Client, Activator.CreateInstance(renderables.FieldType, UWGame.SimSide.Systems.Module.Client, false));
        PatchForHeadlessSim();
        var harmony = new Harmony("uw.dataexport.reload");
        var skip = new HarmonyMethod(typeof(Program).GetMethod(nameof(SkipReturnDefault), AnyStatic));
        PatchByName(harmony, "UWGame.Control.Controller.RecreateClientAfterLoad", prefix: skip);
        PatchByName(harmony, "UWGame.SimSide.Scenarios.ScenarioData.LoadContent", prefix: skip);
        sim.LoadPostProcess(sn);
        typeof(Snapshotter).GetField("snapshotting", AnyStatic).SetValue(null, false);

        // The screens Snapshotter.Load takes the Sim out of and puts it back into, and the Client
        // it asks to rebuild itself.
        var controller = (Controller)GetField(sim, "controller");
        SetField(controller, "screens", new List<UWGame.Control.GameScreen> { The.Client, sim });
        SetField(controller, "screensToUpdate", new List<UWGame.Control.GameScreen>());
        controller.UpdateScreensLock = new object();
        SetField(The.Client, "controller", controller);

        int vehicles = Vehicles().Count();
        Console.WriteLine($"==> SAVE: {vehicles} vehicle(s) in the game ({string.Join(", ", Vehicles().Select(v => v.Parent.EntityType.KeyName).Distinct())}), {Vehicles().Sum(v => v.Slots.Count)} seat or cargo slot(s)");
        string copy = Path.Combine(Path.GetTempPath(), "uw-save-reload-" + Guid.NewGuid().ToString("N") + ".sav");
        try
        {
            SaveGameHeadless(sn, sim, copy, savePath);
            Console.WriteLine("    saved; loading it back into the running game, as SAVE does");

            Sim.LoadException = null;
            sim.LoadSavedGame(copy);
            var waited = System.Diagnostics.Stopwatch.StartNew();
            while (!Sim.LoadIsFinished && waited.Elapsed < TimeSpan.FromMinutes(5))
            {
                Thread.Sleep(50);
            }
            Exception failed = null;
            try
            {
                // The studio's own poll: joins the load thread and rethrows what it caught.
                sim.LoadSavedGame(copy);
            }
            catch (Exception ex)
            {
                failed = ex.InnerException ?? ex;
            }
            Check(Sim.LoadIsFinished, "the load finished");
            Check(failed == null, "the load did not fail" + (failed == null ? "" : ": " + failed.GetType().Name + ": " + failed.Message
                + Environment.NewLine + "        " + failed.StackTrace?.Split('\n').FirstOrDefault()?.Trim()));
            if (failed != null)
            {
                return Report(failures);
            }
            Check(The.Sim != null && The.Sim != sim, "the loaded game replaced the running one");
            Check(The.Map?.TileMap != null, "the loaded game has its map - what GameWorldRenderer.ComputeDrawingArea reads first");
            Check(Vehicles().Count() == vehicles, $"it has the same {vehicles} vehicle(s)");
            var slots = new List<PassengerOrCargoSlot>();
            LookUp<PassengerOrCargoSlot, PassengerOrCargoSlotID>.IterateMembers(slots.Add);
            Check(Vehicles().All(v => v.Slots.All(s => LookUp<PassengerOrCargoSlot, PassengerOrCargoSlotID>.FindByID(s.ID) == s)),
                  "every slot of every vehicle is the one registered under its ID");
            Check(slots.Count == Vehicles().Sum(v => v.Slots.Count),
                  $"and nothing else is: {slots.Count} registered, none left over from the game it replaced");
        }
        finally
        {
            File.Delete(copy);
        }
        return Report(failures);
    }

    private static IEnumerable<VehicleContainer> Vehicles()
    {
        var entities = (IDictionary)typeof(LookUp<Entity, EntityID>).GetField("collection", AnyStatic).GetValue(null);
        return entities.Values.Cast<Entity>().Select(e => e.Contains).OfType<VehicleContainer>();
    }

    private static int Report(int failures)
    {
        Console.WriteLine(failures == 0 ? "save reload OK" : $"save reload FAILED - {failures} check(s)");
        return failures == 0 ? 0 : 1;
    }
}
