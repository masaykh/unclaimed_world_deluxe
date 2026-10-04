using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using UWGame;
using UWGame.SimSide;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Snapshots;

namespace UW.Tools.DataExport;

/// <summary>
/// --save-selftest=&lt;file.sav&gt;: reads a saved game with no window, and checks what a Port
/// delivered in it.
///
/// WHY. A bought dog "appear at dock and still stand mindlessly" twice, after two fixes that
/// each built and were each shipped as "not verified in a game". Both read the code correctly
/// and drew the wrong conclusion from it, and the evidence that would have said so - the save
/// the tester sent - could not be opened without starting the game. This opens it: the dog at
/// the dock was the colony's member and owned by it, and its only goal was a
/// passenger wait with nothing to ride.
///
/// HOW FAR IT GOES. Exactly as far as the save's own bytes: the header, the mod switches it was
/// made with (ModSettings.ApplySignature, as the load panel does), its scenario's tables, then
/// the whole simulation through the game's own Snapshotter. It stops before
/// Sim.LoadPostProcess, which builds renderables from loaded models - there is no GraphicsDevice
/// here - so what it checks are the saved fields themselves, plus one tick of the goal that
/// matters, which needs nothing from the client. The steps that stand in for the game's
/// startup are each named where they happen.
/// </summary>
internal static partial class Program
{
    private const BindingFlags AnyInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    private const BindingFlags AnyStatic = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    private static int SaveSelfTest(string savePath)
    {
        if (!File.Exists(savePath))
        {
            Console.Error.WriteLine("FATAL: no such save: " + savePath);
            return 1;
        }
        Console.WriteLine("==> save: " + savePath);

        // The parts of a running Sim the loaders and the reader touch. TileResourceType reads
        // The.Sim.Mode while the tables build; CreateLookupCollections is what the Sim's real
        // constructor does, and every collection in the save is read into one of them.
        The.Sim = new Sim { Mode = Sim.EngineMode.Game };
        typeof(Sim).GetMethod("CreateLookupCollections", AnyInstance).Invoke(The.Sim, null);
        var sn = new Snapshotter();
        The.Snapshotter = sn;

        SnapshotHeader header;
        using (OpenSave(sn, savePath))
        {
            header = (SnapshotHeader)sn.DoISnapshot<SnapshotHeader>(null);
        }
        // Snapshotter.LoadHeader ends the same way. Left on, the tables would build as if a save
        // were filling them in: NeedType, for one, only registers itself when one is not.
        typeof(Snapshotter).GetField("snapshotting", AnyStatic).SetValue(null, false);
        Console.WriteLine("    made with: " + (string.IsNullOrEmpty(header.Mods) ? "(no mod switches)" : header.Mods));
        UWGame.Mods.ModSettings.ApplySignature(header.Mods);
        header.LoadPostProcess(sn);
        UWGame.SimSide.Scenarios.Scenario scenario = header.StartGameParams?.StartScenarioParams?.Scenario;

        // The tables, as Sim.QueueGameDataAndSimInit builds them: base, then the scenario's own
        // loader, which is where a scenario's action sets live.
        int rc = RunLoader(new BaseDataLoader(), scenario, Sim.SerializeMode.NoSerialize, "base tables, with the save's switches");
        if (rc != 0) return rc;
        if (scenario != null)
        {
            DataLoader scenarioLoader = UWGame.SimSide.AllGameData.Scenarios.AllScenarioLoader.GetScenarioDataLoader(scenario);
            if (scenarioLoader != null)
            {
                rc = RunLoader(scenarioLoader, scenario, Sim.SerializeMode.NoSerialize, "the save's scenario tables");
                if (rc != 0) return rc;
            }
        }
        if (!ValidateDataComplete())
        {
            return 1;
        }
        if (!InitializeWithoutContent())
        {
            return 1;
        }

        // Once any combination exists the game stops snapshotting this table
        // (ToolTypeCombination.AddToLookup), and every played game has one. Nothing here creates
        // one, so say so, or the reader takes the next table for this one's contents.
        LookUp<UWGame.SimSide.Processes.ToolTypeCombination, UWGame.SimSide.Processes.ToolTypeCombinationID>.SetPerformSnapshot(value: false);

        // InventorySettings, which a save carries, draws a regulator from the client's random
        // stream in its constructor. Nothing else of the client is reached while reading.
        The.Client = (UWGame.ClientSide.Client)RuntimeHelpers.GetUninitializedObject(typeof(UWGame.ClientSide.Client));
        The.Client.ClientRandomGenerator = new UWGame.Control.Replays.RandomGenerator(UWGame.Control.Replays.RandomGenerator.GeneratorType.Client);

        Sim sim;
        Console.WriteLine("==> reading the simulation");
        using (OpenSave(sn, savePath))
        {
            sn.DoISnapshot<SnapshotHeader>(null);
            sim = (Sim)sn.DoISnapshot(null, typeof(Sim));
        }
        The.Sim = sim;
        // Every random draw reports to the replay controller; a controller that is neither
        // recording nor replaying is the normal state, and is all a goal tick needs.
        var controller = (UWGame.Control.Controller)RuntimeHelpers.GetUninitializedObject(typeof(UWGame.Control.Controller));
        SetField(controller, "recorder", RuntimeHelpers.GetUninitializedObject(typeof(UWGame.Control.Replays.Recorder)));
        SetField(controller, "replayer", RuntimeHelpers.GetUninitializedObject(typeof(UWGame.Control.Replays.Replayer)));
        SetField(sim, "controller", controller);
        sn.mode = Snapshotter.Mode.Load;

        return CheckDeliveredCreatures(sn, sim);
    }

    private static int CheckDeliveredCreatures(Snapshotter sn, Sim sim)
    {
        Console.WriteLine("==> creatures a Port delivers (dogs, robots)");
        int failures = 0;
        void Check(bool ok, string what)
        {
            Console.WriteLine((ok ? "  ok    " : "  FAIL  ") + what);
            if (!ok) failures++;
        }

        var playSite = (SiteID)GetField(sim, "snapshotPlaySite");
        var entities = (IDictionary)typeof(LookUp<Entity, EntityID>).GetField("collection", AnyStatic).GetValue(null);
        int creatures = 0;
        int released = 0;
        Entity aStuckOne = null;
        foreach (Entity e in entities.Values)
        {
            IntelligenceType intelligenceType = e.EntityType?.IntelligenceType;
            if (intelligenceType == null || e.Intelligence == null) continue;
            if (!Equals(GetField(e, "snapshotSiteID"), (SiteID?)playSite)) continue;
            string who = e.EntityType.KeyName + "#" + (long)e.ID + (string.IsNullOrEmpty(e.Intelligence.FirstName) ? "" : " " + e.Intelligence.FirstName);

            // 1. Nothing waits as a passenger with nothing to ride. The studio's wait ends only on
            //    a GetOff message, and cargo unloaded at a dock never gets one.
            Goal top = TopLevelGoal(e);
            if (top != null && top.GetType().Name == "GoalWaitAsPassenger"
                && !e.ContainedBy.HasValue && !e.PassengerInVehicle.HasValue)
            {
                top.LoadPostProcess(sn);
                Status after = top.Process(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.1)));
                Check(after == Status.Completed,
                      who + " was waiting as a passenger, aboard nothing; after one tick the wait is " + after);
                released++;
                aStuckOne = e;
            }

            // 2. and 3. are about what a Port sells: dogs and robots, the servants.
            if (intelligenceType.ServantForEntityTypeTag == null || e.EntityType.StructureType != null) continue;
            creatures++;
            object memberOf = GetField(e.Intelligence, "snapshotExpedition");
            Expedition expedition = memberOf is ExpeditionID id ? LookUp<Expedition, ExpeditionID>.FindByID(id) : null;
            OwnerID? expeditionAsOwner = expedition == null ? null : ((ILookUp<IOwner, OwnerID>)expedition).ID;
            if (e.OwnedBy.HasValue && expeditionAsOwner.HasValue)
            {
                Check(e.OwnedBy == expeditionAsOwner,
                      who + " is a member of the expedition that owns it (owner " + (long)e.OwnedBy.Value + ", member of " + (long)expeditionAsOwner.Value + ")");
                Check(Equals(GetField(e.Intelligence, "snapshotAllegianceID"), GetField(expedition, "snapshotAllegiance")),
                      who + " thinks for its expedition's allegiance");
            }
            Entity container = e.ContainedBy.HasValue ? (Entity)entities[e.ContainedBy.Value] : null;
            Check(!(container?.Contains is TerminalContainer),
                  who + (container == null ? " is not in anything" : " is in " + container.EntityType.KeyName + ", not a Port's stock"));
        }

        // The control: the same rule must not end a real ride. A passenger seated in a vehicle
        // keeps waiting - otherwise this would be "every wait ends", not "a wait with nothing to
        // ride ends".
        if (aStuckOne != null)
        {
            Type waitType = typeof(Goal).Assembly.GetType("UWGame.SimSide.AI.Goals.GoalWaitAsPassenger", throwOnError: true);
            EntityID? seat = aStuckOne.PassengerInVehicle;
            aStuckOne.PassengerInVehicle = aStuckOne.ID;
            var riding = (Goal)Activator.CreateInstance(waitType, aStuckOne);
            Status rider = riding.Process(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.1)));
            aStuckOne.PassengerInVehicle = seat;
            Check(rider == Status.Active, "control: the same creature, seated in a vehicle, keeps waiting (" + rider + ")");
        }

        Console.WriteLine($"    {creatures} dog(s)/robot(s) on the play site, {released} waiting with nothing to ride");
        Check(creatures > 0, "the save has a dog or a robot on the play site to check");
        Console.WriteLine(failures == 0 ? "save self-test OK" : $"save self-test FAILED - {failures} check(s)");
        return failures == 0 ? 0 : 1;
    }

    private static Goal TopLevelGoal(Entity e)
    {
        object brainID = GetField(e.Intelligence, "snapshotBrain");
        Goal brain = brainID is GoalID id ? LookUpGoals.FindByID(id) : null;
        if (brain is not CompositeGoal) return null;
        var queued = (IEnumerable<GoalID>)GetField(brain, "snapshotSubgoals");
        GoalID? first = queued.Cast<GoalID?>().FirstOrDefault();
        return first.HasValue ? LookUpGoals.FindByID(first.Value) : null;
    }

    /// <summary>
    /// The half of GameData.Initialize that does not need loaded content. The game runs it from
    /// the loading screen after GameData.LoadContent; it is where, among much else, each
    /// creature's consume recipes are generated - and a save names those by key.
    ///
    /// Init2DAnims is the one step that reads a sprite sheet, and only for source rectangles.
    /// A sheet whose every sprite is an empty rectangle lets it run: the names it asks for are
    /// learned from its own KeyNotFoundException, so nothing here has to list them.
    /// </summary>
    private static bool InitializeWithoutContent()
    {
        Console.WriteLine("==> the post-load pass, without content (GameData.Initialize, minus the art)");
        GameData data = GameData.Instance;
        var sheet = new SpriteSheetRuntime.ExtendedSpriteSheet
        {
            spriteRectangles = new List<Rectangle> { Rectangle.Empty },
            spriteNames = new Dictionary<string, int>(),
        };
        data.BillboardSpriteSheet = sheet;
        var errors = new Dictionary<string, List<string>>();
        SetField(data, "allPostLoadContentValidationErrors", errors);
        MethodInfo init2DAnims = typeof(GameData).GetMethod("Init2DAnims", AnyInstance);
        for (int attempt = 0; ; attempt++)
        {
            try
            {
                init2DAnims.Invoke(data, null);
                break;
            }
            catch (TargetInvocationException ex) when (ex.InnerException is KeyNotFoundException missing && attempt < 10000)
            {
                Match name = Regex.Match(missing.Message, "named '(.*)'");
                if (!name.Success) throw;
                sheet.spriteNames[name.Groups[1].Value] = 0;
                data.Animation2Ds.Clear();
            }
        }
        // The rest of GameData.Initialize, in its order.
        foreach (string step in new[]
                 {
                     "SetupEventHooks", "SetupOtherEvents", "PostLoadContentInitialize", "CreateProcessGraph",
                     "ValidateProcessTypeGraph", "CreateRepairProcesses", "CreateSpecialActionProcesses",
                     "MarkAnchorStructures", "ExtractNestedGameData", "OverrideEntityTypeDescriptions",
                     "InitContainerTransactHints", "InitOtherLists", "InitMetaConstants", "InitDamageScoreTables",
                     "InitWeaponEffectiveness",
                 })
        {
            MethodInfo method = typeof(GameData).GetMethod(step, AnyInstance);
            if (method == null)
            {
                Console.WriteLine("    FAIL GameData." + step + " no longer exists - this list follows GameData.Initialize");
                return false;
            }
            object[] parameters = method.GetParameters().Length == 0 ? null : new object[method.GetParameters().Length];
            if (step == "CreateRepairProcesses") parameters[0] = errors;
            try
            {
                method.Invoke(data, parameters);
            }
            catch (TargetInvocationException ex)
            {
                Exception root = ex.GetBaseException();
                Console.WriteLine("    FAIL GameData." + step + ": " + root.GetType().Name + ": " + root.Message);
                return false;
            }
        }
        Console.WriteLine("    ok");
        return true;
    }

    /// <summary>
    /// The reader Snapshotter.Load would set up, without the screen swap around it. IsSnapshotting
    /// has to be true while reading: PlaySite's constructor, for one, builds a fresh world unless
    /// it is told a save is filling it in.
    /// </summary>
    private static BinaryReader OpenSave(Snapshotter sn, string path)
    {
        var reader = new BinaryReader(new BufferedStream(new GZipStream(File.OpenRead(path), CompressionMode.Decompress), 65536));
        SetField(sn, "m_reader", reader);
        sn.mode = Snapshotter.Mode.Load;
        typeof(Snapshotter).GetField("snapshotting", AnyStatic).SetValue(null, true);
        return reader;
    }

    private static FieldInfo FindField(Type type, string name)
    {
        for (Type t = type; t != null; t = t.BaseType)
        {
            FieldInfo field = t.GetField(name, AnyInstance | BindingFlags.DeclaredOnly);
            if (field != null) return field;
        }
        throw new MissingFieldException(type.Name, name);
    }

    private static object GetField(object instance, string name) => FindField(instance.GetType(), name).GetValue(instance);

    private static void SetField(object instance, string name, object value) => FindField(instance.GetType(), name).SetValue(instance, value);
}
