using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework;
using UWGame;
using UWGame.SimSide;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UW.Tools.DataExport;

/// <summary>
/// --save-scout=&lt;file.sav&gt;: SCOUT and EXAMINE end. Loads a save as --save-stuck does, orders a
/// SCOUT and an EXAMINE zone beside the colony through the game's own commands
/// (UWGame.SimSide.Commands.Scout and Examine, as the zone menu's buttons issue them), runs the
/// game with no window and follows every sweep of the two orders: who started it (GoalSearchArea
/// under GoalScouting), how many points it had, how many it visited, and how it ended.
///
/// WHY. "Currently SCOUT and EXAMINE commands are endless" (2026-10-19). The studio kept a sweep's
/// points on GoalSearchArea, so every interruption - a meal, sleep, nightfall, a better job -
/// threw the sweep away and the next one sampled the zone again from nothing. The port keeps the
/// sweep on the ScoutingJob (ScoutingJob.SweepPointsLeft); this checks that it does what that
/// promises:
///
///   - each order's zone is sampled ONCE (UniformPoissonDiskSampler.SampleRectangle, counted),
///     whoever sweeps it and however often they are interrupted
///   - the run interrupts a sweeper twice per order (its goals are dropped, as GoalThink drops them
///     when a better goal wins), and the next sweep starts with what was left, not a fresh zone
///   - a second colonist is put on each order while the first is on it, and they share the one
///     sweep: no point is held by two sweeps at once
///   - a point nobody can reach, added to each order's sweep, is dropped and does not restart it
///   - both orders END: the job destroyed, and the zone's ScoutingJob / ExamineJob cleared
///
/// Saves: UW_SCOUT_SAVE_OUT=&lt;file&gt; writes the game through Sim.DoSave once both sweeps are under
/// way (after UW_SCOUT_SAVE_AFTER seconds, default 120), prints what is left of each and stops.
/// Run on that file, this resumes the saved orders instead of ordering new ones and prints what it
/// read back - gate 80 case 32e compares the two - then checks the rest as above. Orders in a save
/// from an older build are listed with the sweep they read back: none.
///
/// Environment: UW_SCOUT_SECONDS game seconds at most (default 7200; the run stops when both
/// orders end), UW_SCOUT_TILES the zones' side (default 20), UW_SCOUT_CYCLES AI cycles an update
/// (default 300; the game's own budget is wall-clock time, which makes no two runs alike),
/// UW_SCOUT_FORCE=0 no forced interruption, second colonist or unreachable point - the game as it plays.
///
/// The sweep fields are read by name, so this also runs against a build without them - the
/// studio's - and reports what that build does: it fails.
/// </summary>
internal static partial class Program
{
    private static string scoutReport;

    private sealed class Sweep
    {
        public Entity Entity;
        public ScoutingJob Job;
        public int PointsAtStart;
        public int Visited;
        public string End;
        public double Started;
        public double Ended;
        public string NextGoal;
        public bool FromSave;
    }

    private sealed class Order
    {
        public ScoutingJob Job;
        public Zone Zone;
        public string Name;
        public int Samplings;
        public int PointsSampled = -1;
        public int Interruptions;
        public bool SecondPut;
        public bool Unreachable;
        public Vector2 UnreachablePoint;
        public double? Destroyed;
        public readonly HashSet<Entity> Sweepers = new();
        public int MostSweepingAtOnce;
        public int LeftAfterInterruption = -1;
        public int LeftAtNextSweep = -1;
        public bool OthersStillOn;
    }

    private static readonly List<Sweep> sweeps = new();
    private static readonly Dictionary<object, Sweep> sweepOfGoal = new(ReferenceEqualityComparer.Instance);
    private static readonly List<Order> orders = new();
    private static readonly List<string> scoutEvents = new();
    private static readonly List<string> doubleHeld = new();
    private static double scoutNow;
    private static bool forceEvents;
    private static Order sampling;
    private static readonly Dictionary<ScoutingJob, double?> olderOrders = new();

    private static List<Vector2> PointsLeft(ScoutingJob job) => typeof(ScoutingJob).GetField("SweepPointsLeft")?.GetValue(job) as List<Vector2>;

    private static int PointsVisited(ScoutingJob job) => typeof(ScoutingJob).GetField("SweepPointsVisited")?.GetValue(job) is int n ? n : -1;

    private static string Describe(ScoutingJob job) => PointsLeft(job) is List<Vector2> left ? $"{left.Count} point(s) left, {PointsVisited(job)} visited" : "no sweep of its own";

    private static int ScoutSweepReport(Snapshotter sn, Sim sim)
    {
        var inv = CultureInfo.InvariantCulture;
        var renderables = FindField(typeof(UWGame.ClientSide.Client), "renderables");
        renderables.SetValue(The.Client, Activator.CreateInstance(renderables.FieldType, UWGame.SimSide.Systems.Module.Client, false));
        PatchForHeadlessSim();
        sim.LoadPostProcess(sn);
        typeof(Snapshotter).GetField("snapshotting", AnyStatic).SetValue(null, false);
        PatchScoutingForReport();
        sim.CycleManager.CycleSchedule = new[] { int.Parse(Environment.GetEnvironmentVariable("UW_SCOUT_CYCLES") ?? "300", inv) };

        // Goals reach an entity's renderable directly (GoalWait.OnEnter sets its animation flags), and
        // with no window LoadPostProcess leaves most without one; an inert one lets their goals run
        // instead of throwing every update - which would also stop every sweep at its first pause.
        foreach (Entity e in ((IDictionary)typeof(LookUp<Entity, EntityID>).GetField("collection", AnyStatic).GetValue(null)).Values)
        {
            if (e.Renderable == null && e.IsOnPlaySite())
            {
                e.Renderable = (UWGame.ClientSide.Renderables.Renderable)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(UWGame.ClientSide.Renderables.Renderable));
            }
        }

        List<Entity> colonists = sim.PlaySite.PlayerAllegiance.Members
            .Where(e => e.Intelligence != null && e.IsOnPlaySite() && e.PersonEntity != null).OrderBy(e => (long)e.ID).ToList();
        Console.WriteLine($"==> {colonists.Count} colonist(s); day {sim.DateAndTime.CurrentTimeDateYear.TotalDays.ToString("0.000", inv)}, a day is {sim.DateAndTime.SecondsPerDay.ToString("0", inv)} s");
        if (colonists.Count < 2)
        {
            Console.WriteLine("save scout-test FAILED - it needs two colonists");
            return 1;
        }
        EntityGroup group = colonists[0].Intelligence.CurrentExpedition.OwnedEntities;

        // The orders the save holds. Ours, written by UW_SCOUT_SAVE_OUT, are resumed.
        var held = group.ScoutingJobs.OfType<ScoutingJob>().Where(j => j.Zone?.MapArea != null).ToList();
        foreach (ScoutingJob j in held)
        {
            Console.WriteLine($"    the save holds {(j.Examine ? "an EXAMINE" : "a SCOUT")} order on zone {(long)j.Zone.ID}: {Describe(j)}");
        }
        bool resumed = held.Count(j => PointsLeft(j) != null) >= 2;
        foreach (ScoutingJob j in held.Where(j => PointsLeft(j) == null)) olderOrders[j] = null;
        if (resumed)
        {
            foreach (ScoutingJob j in held.Where(j => PointsLeft(j) != null))
            {
                orders.Add(new Order { Job = j, Zone = j.Zone, Name = j.Examine ? "EXAMINE" : "SCOUT", PointsSampled = PointsLeft(j).Count + PointsVisited(j) });
                Console.WriteLine($"resumed {(j.Examine ? "EXAMINE" : "SCOUT")}: {PointsLeft(j).Count} left, {PointsVisited(j)} visited");
            }
            // Sweeps under way when the game was saved: their GoalSearchArea activated before it.
            foreach (Order o in orders)
            {
                foreach (var (e, g) in ActiveSearches(colonists, o.Job))
                {
                    var s = new Sweep { Entity = e, Job = o.Job, PointsAtStart = CountOf(g, "locationsStillToVisit"), FromSave = true };
                    sweeps.Add(s);
                    sweepOfGoal[g] = s;
                    o.Sweepers.Add(e);
                }
            }
        }
        else
        {
            Vector3 centre = colonists.Aggregate(Vector3.Zero, (s, e) => s + e.PlaySiteLocation) / colonists.Count;
            int size = int.Parse(Environment.GetEnvironmentVariable("UW_SCOUT_TILES") ?? "20", inv);
            Point at = MapManager.WorldPosToTile(centre);
            new UWGame.SimSide.Commands.Scout(WalkableSquare(new Point(at.X - size - 2, at.Y - size / 2), size), giveClientFeedback: false, group.ID).Execute(giveClientFeedback: false);
            new UWGame.SimSide.Commands.Examine(WalkableSquare(new Point(at.X + 3, at.Y - size / 2), size), giveClientFeedback: false, group.ID).Execute(giveClientFeedback: false);
            foreach (ScoutingJob j in group.ScoutingJobs.OfType<ScoutingJob>().Except(held))
            {
                orders.Add(new Order { Job = j, Zone = j.Zone, Name = j.Examine ? "EXAMINE" : "SCOUT" });
            }
        }
        foreach (Order o in orders)
        {
            Console.WriteLine($"    {o.Name} zone {(long)o.Zone.ID}: {o.Zone.MapArea.GetTileLocations().Count} tiles in {o.Zone.MapArea.GetBoundingBoxInTiles()}");
        }
        if (orders.Count != 2 || orders.Count(o => o.Job.Examine) != 1)
        {
            Console.WriteLine("save scout-test FAILED - the SCOUT and EXAMINE commands did not give one order each");
            return 1;
        }

        forceEvents = Environment.GetEnvironmentVariable("UW_SCOUT_FORCE") != "0";
        double seconds = double.Parse(Environment.GetEnvironmentVariable("UW_SCOUT_SECONDS") ?? "7200", inv);
        string saveOut = Environment.GetEnvironmentVariable("UW_SCOUT_SAVE_OUT");
        double saveAfter = double.Parse(Environment.GetEnvironmentVariable("UW_SCOUT_SAVE_AFTER") ?? "120", inv);
        double start = sim.TotalUnPausedGameTimeInSeconds;
        Console.WriteLine($"==> running at most {seconds.ToString(inv)} s of game time" + (saveOut == null ? "" : $", saving once both sweeps are under way after {saveAfter.ToString(inv)} s"));
        bool saved = false;
        const double fps = 30;
        for (long f = 0; f < (long)(seconds * fps) && !saved && orders.Any(o => o.Destroyed == null); f++)
        {
            if (f % (long)(600 * fps) == 0)
            {
                Console.WriteLine($"    t+{scoutNow,-5:0} {sim.DateAndTime.TimeOfDay.ToString("0.00", inv)} of the day: " + string.Join(", ", orders.Select(o =>
                    $"{o.Name} {(o.Destroyed != null ? "ended" : Describe(o.Job) + $", taken by {o.Job.TakenBy.Count}")}"))
                    + "; doing " + string.Join(", ", colonists.Select(c => Name(c) + " " + (c.Intelligence.Brain.Subgoals.Count > 0 ? c.Intelligence.Brain.Subgoals.Peek().GetType().Name : "nothing"))));
            }
            long ticks = (long)Math.Round((f + 1) * TimeSpan.TicksPerSecond / fps), previous = (long)Math.Round(f * TimeSpan.TicksPerSecond / fps);
            var frame = new GameTime(new TimeSpan(ticks), new TimeSpan(ticks - previous));
            Guard("frame", () => sim.RunFrame(sim.ScaleToGameSpeed(frame)));
            scoutNow = sim.TotalUnPausedGameTimeInSeconds - start;
            if (Environment.GetEnvironmentVariable("UW_SCOUT_DEBUG") != null && f % (long)(30 * fps) == 0)
            {
                Console.WriteLine($"  dbg t+{scoutNow:0} " + string.Join(" | ", colonists.Select(c => Name(c) + (c.IsDead ? " DEAD" : "") + " " + GoalChain(c) + " "
                    + string.Join(",", c.BiologicalEntity?.Needs?.NeedsList.Select(kv => kv.Key + "=" + kv.Value.CurrentLevel.ToString("0.00", inv)) ?? Array.Empty<string>()))));
            }
            AfterScoutFrame(colonists);
            if (saveOut != null && scoutNow >= saveAfter && orders.All(o => PointsLeft(o.Job) is { Count: > 0 } && PointsVisited(o.Job) > 0))
            {
                SaveGameHeadless(sn, sim, saveOut);
                foreach (Order o in orders)
                {
                    Console.WriteLine($"saved {o.Name}: {PointsLeft(o.Job).Count} left, {PointsVisited(o.Job)} visited");
                }
                saved = true;
            }
        }
        if (saveOut != null)
        {
            Console.WriteLine(saved ? $"==> saved mid-sweep at t+{scoutNow:0} to {saveOut}" : "save scout-test FAILED - the sweeps never both got under way, nothing saved");
            return saved ? 0 : 1;
        }

        foreach (var kv in swallowed.OrderByDescending(kv => kv.Value).Take(8))
        {
            Console.WriteLine($"    (headless stand-in) {kv.Value}x {kv.Key}");
        }
        foreach (string e in scoutEvents) Console.WriteLine("    " + e);
        foreach (var (j, ended) in olderOrders)
        {
            // An order from an older build reads back with no sweep of its own; a sweep under way in that
            // save goes on with its own points, as the studio's did, and the next one starts the order's.
            Console.WriteLine($"==> the save's {(j.Examine ? "EXAMINE" : "SCOUT")} order on zone {(long)j.Zone.ID}, read with no sweep of its own: "
                              + (ended is double t ? $"ENDED at t+{t:0}" : "still ordered: " + Describe(j)));
        }
        foreach (Order o in orders)
        {
            List<Sweep> mine = sweeps.Where(s => s.Job == o.Job).ToList();
            Console.WriteLine($"==> {o.Name}: {o.PointsSampled} point(s) sampled, {o.Samplings} sampling(s); {mine.Count} sweep(s) by {o.Sweepers.Count} colonist(s), "
                              + $"{mine.Sum(s => s.Visited)} visit(s) counted by the sweeps; {(o.Destroyed is double d ? $"ENDED at t+{d:0}" : "still ordered: " + Describe(o.Job))}");
            foreach (Sweep s in mine)
            {
                Console.WriteLine($"    t {s.Started,6:0} - {(s.End == null ? "" : s.Ended.ToString("0", inv)),6}  {Name(s.Entity),-10} {s.PointsAtStart,4} point(s){(s.FromSave ? " (under way in the save)" : "")}, "
                                  + $"visited {s.Visited,3}, ended {s.End ?? "(running)"}{(s.NextGoal == null ? "" : ", then " + s.NextGoal)}");
            }
        }

        int failures = 0;
        void Check(bool ok, string what)
        {
            Console.WriteLine((ok ? "  ok    " : "  FAIL  ") + what);
            if (!ok) failures++;
        }
        foreach (Order o in orders)
        {
            Check(o.Samplings <= (resumed ? 0 : 1) && PointsLeft(o.Job) != null,
                $"{o.Name}: the zone is sampled once for the order, not once a sweep ({o.Samplings} sampling(s){(resumed ? " after the load" : "")}, the order keeps its points: {PointsLeft(o.Job) != null})");
            if (!resumed && forceEvents)
            {
                Check(o.Interruptions == 2 && (o.LeftAtNextSweep >= 0 ? o.LeftAtNextSweep <= o.LeftAfterInterruption : o.OthersStillOn),
                    $"{o.Name}: after {o.Interruptions} interruption(s), the sweep went on with what was left ({o.LeftAfterInterruption} left when last interrupted, "
                    + (o.LeftAtNextSweep >= 0 ? $"{o.LeftAtNextSweep} at the next sweep)" : o.OthersStillOn ? "and the other colonist's sweep went on)" : "and no sweep came after)"));
                Check(o.SecondPut && o.Sweepers.Count >= 2,
                    $"{o.Name}: a second colonist put on it sweeps the same points ({o.Sweepers.Count} colonist(s) swept it, at most {o.MostSweepingAtOnce} at once)");
            }
            if (forceEvents) Check(o.Unreachable && PointsLeft(o.Job)?.Contains(o.UnreachablePoint) == false,
                $"{o.Name}: a point nobody can reach was dropped from the sweep");
            Check(o.Destroyed != null && o.Job.ID == JobID.Invalid && (o.Job.Examine ? o.Zone.ExamineJob : o.Zone.ScoutingJob) == null,
                $"{o.Name}: the order ended - the job destroyed and the zone's {(o.Job.Examine ? "ExamineJob" : "ScoutingJob")} cleared"
                + (o.Destroyed is double t ? $", after {t:0} s ({(t / sim.DateAndTime.SecondsPerDay).ToString("0.00", inv)} of a day)" : ""));
        }
        Check(doubleHeld.Count == 0, "no point was held by two sweeps at once, or held and still in the order's list" + (doubleHeld.Count == 0 ? "" : ": " + doubleHeld[0]));
        Console.WriteLine(failures == 0 ? "save scout-test OK" : $"save scout-test FAILED - {failures} check(s)");
        return failures == 0 ? 0 : 1;
    }

    /// <summary>Between updates: note how sweeps ended, check the shared list, and interrupt or add sweepers on cue.</summary>
    private static void AfterScoutFrame(List<Entity> colonists)
    {
        foreach (Sweep s in sweeps.Where(s => s.End != null && s.NextGoal == null))
        {
            var brain = s.Entity.Intelligence.Brain;
            Goal top = brain.Subgoals.Count > 0 ? brain.Subgoals.Peek() : null;
            if (top != null && top.GetType().Name != "GoalScouting") s.NextGoal = top.GetType().Name;
        }
        foreach (Order o in orders.Where(o => o.Destroyed == null))
        {
            List<Vector2> left = PointsLeft(o.Job);
            List<(Entity Entity, Goal Goal)> active = ActiveSearches(colonists, o.Job);
            o.MostSweepingAtOnce = Math.Max(o.MostSweepingAtOnce, active.Count);

            // No point held twice, and none held and still in the list.
            var heldPoints = new List<Vector2>();
            foreach (var (e, g) in active)
            {
                if (GetFieldOrNull(g, "holdsPoint") is true)
                {
                    var p = (Vector2)GetField(g, "closestLocation");
                    if (heldPoints.Contains(p) || left?.Contains(p) == true)
                    {
                        doubleHeld.Add($"{o.Name} point {p} at t+{scoutNow:0}, by {Name(e)}");
                    }
                    heldPoints.Add(p);
                }
            }

            if (active.Count == 0 || !forceEvents) continue;
            // The points the first sweeper is working from: the order's, or (the studio's) its own.
            Goal sweeping = active[0].Goal;
            var points = left ?? (List<Vector2>)GetFieldOrNull(sweeping, "locationsStillToVisit");
            int visitedHere = CountOf(sweeping, "numberOfVisitedLocations");
            if (points == null) continue;
            // A point nobody can reach, once the sweep is under way.
            if (!o.Unreachable && visitedHere >= 1)
            {
                o.UnreachablePoint = BlockedPointNear(o.Zone);
                points.Insert(0, o.UnreachablePoint);
                o.Unreachable = true;
                scoutEvents.Add($"t {scoutNow,6:0} {o.Name}: added a point nobody can reach, {o.UnreachablePoint}");
            }
            // Interruptions: the sweeper's goals dropped mid-walk, as GoalThink drops them when another goal wins.
            if (o.Interruptions < 2 && visitedHere >= 3 && ((CompositeGoal)sweeping).Subgoals.Count > 0)
            {
                Entity e = active[0].Entity;
                int leftBefore = points.Count;
                e.Intelligence.Brain.RemoveAllSubgoals();
                o.Interruptions++;
                // The order's list has the point being walked to back by now; the studio's sweep is gone with its list.
                o.LeftAfterInterruption = left?.Count ?? leftBefore;
                o.LeftAtNextSweep = -1;
                o.OthersStillOn = active.Count > 1;
                scoutEvents.Add($"t {scoutNow,6:0} {o.Name}: {Name(e)} interrupted mid-walk, {o.LeftAfterInterruption} point(s) left");
            }
            // A second colonist, once the first has been interrupted, set on the order by its own evaluator.
            else if (!o.SecondPut && o.Interruptions >= 1 && active.Count == 1)
            {
                Entity first = active[0].Entity;
                Entity second = colonists.FirstOrDefault(c => c != first && JobOfTopGoal(c) == null && c.Intelligence.Brain.Subgoals.Count > 0);
                if (second != null && PutOnOrder(second, o))
                {
                    o.SecondPut = true;
                    scoutEvents.Add($"t {scoutNow,6:0} {o.Name}: {Name(second)} put on it beside {Name(first)}");
                }
            }
        }
    }

    private static List<(Entity, Goal)> ActiveSearches(List<Entity> colonists, ScoutingJob job)
    {
        var found = new List<(Entity, Goal)>();
        foreach (Entity e in colonists)
        {
            if (JobOfTopGoal(e) != job) continue;
            for (Goal g = e.Intelligence.Brain; g is CompositeGoal c && c.Subgoals.Count > 0;)
            {
                g = c.Subgoals.Peek();
                if (g.GetType().Name == "GoalSearchArea" && g.Status == Status.Active)
                {
                    found.Add((e, g));
                    break;
                }
            }
        }
        return found;
    }

    /// <summary>
    /// The colonist's own EvaluateScoutingJobs, told the order is its best, sets its goal - what
    /// GoalThink.Arbitrate does when that evaluator wins - without cancelling whoever is on it.
    /// </summary>
    private static bool PutOnOrder(Entity colonist, Order o)
    {
        var evaluators = (IEnumerable)GetField(colonist.Intelligence.Brain, "evaluators");
        GoalEvaluator evaluator = evaluators.Cast<GoalEvaluator>().FirstOrDefault(ev => ev.GetType().Name == "EvaluateScoutingJobs");
        if (evaluator == null) return false;
        SetField(evaluator, "mostDesirableJob", o.Job);
        SetField(evaluator, "locationInScoutingArea", (Vector3?)MapManager.TileToWorldPos(o.Zone.MapArea.StartDragTile.Value));
        SetField(evaluator, "bestScore", 1.0);
        colonist.Intelligence.Brain.RemoveAllSubgoals();
        return evaluator.SetGoal();
    }

    /// <summary>A world position near the zone that nobody walks: the nearest subtile the foot layer blocks.</summary>
    private static Vector2 BlockedPointNear(Zone zone)
    {
        SubtileLayers foot = The.Map.TerrainCosts[SurfaceType.TransportType.Foot];
        Point c = MapManager.WorldPosToSubtile(zone.MapArea.GetCenter().Value);
        for (int r = 0; r < 300; r++)
        {
            for (int y = c.Y - r; y <= c.Y + r; y++)
            {
                for (int x = c.X - r; x <= c.X + r; x++)
                {
                    if ((Math.Abs(x - c.X) == r || Math.Abs(y - c.Y) == r) && The.Map.SubtileIsOnMap(new Point(x, y))
                        && MapManager.GetCost(foot.GetValue(x, y)) == 0)
                    {
                        return new Vector2(x * MapManager.subTileSize + MapManager.subTileSizeOver2, y * MapManager.subTileSize + MapManager.subTileSizeOver2);
                    }
                }
            }
        }
        throw new InvalidOperationException("no blocked subtile near the zone");
    }

    /// <summary>Sim.DoSave, with the StartGameParams the game keeps from the save's header when it loads one.</summary>
    private static void SaveGameHeadless(Snapshotter sn, Sim sim, string path)
    {
        if (sim.StartGameParams == null)
        {
            SnapshotHeader header;
            using (OpenSave(sn, Path.GetFullPath(scoutReport)))
            {
                header = (SnapshotHeader)sn.DoISnapshot<SnapshotHeader>(null);
            }
            typeof(Snapshotter).GetField("snapshotting", AnyStatic).SetValue(null, false);
            // As SaveSelfTest does it: with no client, the scenario's options are resolved without loading its art.
            UWGame.ClientSide.Client client = The.Client;
            The.Client = null;
            header.LoadPostProcess(sn);
            The.Client = client;
            sim.StartGameParams = header.StartGameParams;
        }
        // With no client the Sim writes the interface's settings it read from the save (Sim.DoSnapshot),
        // rather than asking a map window and an interface there are none of here.
        UWGame.ClientSide.Client headless = The.Client;
        The.Client = null;
        try
        {
            sim.DoSave(path);
        }
        finally
        {
            The.Client = headless;
        }
    }

    /// <summary>A square of tiles from <paramref name="corner"/>: the tiles on the map whose centre a person can walk.</summary>
    private static MapArea WalkableSquare(Point corner, int size)
    {
        SubtileLayers foot = The.Map.TerrainCosts[SurfaceType.TransportType.Foot];
        var area = new MapArea();
        for (int y = corner.Y; y < corner.Y + size; y++)
        {
            for (int x = corner.X; x < corner.X + size; x++)
            {
                if (!The.Map.TileIsOnMap(x, y)) continue;
                Point sub = MapManager.WorldPosToSubtile(MapManager.TileToWorldPos(new Point(x, y)));
                if (MapManager.GetCost(foot.GetValue(sub.X, sub.Y)) == 0) continue;
                area.Add(The.Map.GetTile(x, y));
                area.StartDragTile ??= new Point(x, y);
            }
        }
        return area;
    }

    private static void PatchScoutingForReport()
    {
        var harmony = new Harmony("uw.dataexport.scout");
        Type search = typeof(Sim).Assembly.GetType("UWGame.SimSide.AI.Goals.GoalSearchArea", throwOnError: true);
        harmony.Patch(search.GetMethod("Activate", AnyInstance),
            prefix: new HarmonyMethod(typeof(Program).GetMethod(nameof(SearchActivating), AnyStatic)),
            postfix: new HarmonyMethod(typeof(Program).GetMethod(nameof(SearchActivated), AnyStatic)));
        harmony.Patch(typeof(CompositeGoal).GetMethod(nameof(CompositeGoal.Terminate)), prefix: new HarmonyMethod(typeof(Program).GetMethod(nameof(GoalTerminating), AnyStatic)));
        harmony.Patch(typeof(ScoutingJob).GetMethod(nameof(ScoutingJob.Destroy)), prefix: new HarmonyMethod(typeof(Program).GetMethod(nameof(ScoutingJobDestroyed), AnyStatic)));
        // An event's music cue reaches the audio manager there is none of here. Its throw ended the
        // whole time-sliced AI update (CycleManager) each time, so no colonist ever chose a new goal.
        harmony.Patch(typeof(UWGame.ClientSide.GameEvents.MusicAction).GetMethod(nameof(UWGame.ClientSide.GameEvents.MusicAction.Execute)),
            prefix: new HarmonyMethod(typeof(Program).GetMethod(nameof(MusicPlayed), AnyStatic)));
        // Each reads a sprite or tints one there is none of here (a tile resource's renderable, an item's
        // billboard), and its throw ended the caller - a colonist's whole update, or the whole AI update.
        // Swallowed at the method, they answer "no" and the rest goes on.
        foreach (string name in new[] { "UWGame.SimSide.AI.Goals.EvaluateJob.IsGatherJobAccessible", "UWGame.SimSide.Jobs.HaulingJobManager.TestItemToStackWith",
                                        "UWGame.SimSide.Jobs.HaulingJobManager.FindSimilarItemToStackWithInTile" })
        {
            PatchByName(harmony, name, finalizer: new HarmonyMethod(typeof(Program).GetMethod(nameof(SwallowException), AnyStatic)));
        }
        foreach (MethodInfo m in typeof(UniformPoissonDiskSampler).GetMethods().Where(m => m.Name == nameof(UniformPoissonDiskSampler.SampleRectangle)))
        {
            harmony.Patch(m, postfix: new HarmonyMethod(typeof(Program).GetMethod(nameof(Sampled), AnyStatic)));
        }
    }

    private static ScoutingJob JobOfTopGoal(Entity e)
    {
        var brain = e?.Intelligence?.Brain;
        if (brain == null || brain.Subgoals.Count == 0) return null;
        Goal top = brain.Subgoals.Peek();
        return top.GetType().Name == "GoalScouting" ? GetField(top, "job") as ScoutingJob : null;
    }

    private static object GetFieldOrNull(object instance, string name)
    {
        for (Type t = instance.GetType(); t != null; t = t.BaseType)
        {
            FieldInfo field = t.GetField(name, AnyInstance | BindingFlags.DeclaredOnly);
            if (field != null) return field.GetValue(instance);
        }
        return null;
    }

    private static int CountOf(object goal, string field) => GetFieldOrNull(goal, field) switch { ICollection c => c.Count, int i => i, _ => -1 };

    private static void SearchActivating(object __instance)
    {
        ScoutingJob job = JobOfTopGoal(((Goal)__instance).entity);
        sampling = orders.FirstOrDefault(o => o.Job == job);
    }

    private static bool MusicPlayed(ref bool __result)
    {
        __result = true;
        return false;
    }

    private static void Sampled()
    {
        if (sampling != null) sampling.Samplings++;
    }

    private static void SearchActivated(object __instance)
    {
        Order o = sampling;
        sampling = null;
        if (o == null) return;
        Entity e = ((Goal)__instance).entity;
        int points = CountOf(__instance, "locationsStillToVisit");
        if (o.PointsSampled < 0) o.PointsSampled = points;
        if (o.Interruptions > 0 && o.LeftAtNextSweep < 0) o.LeftAtNextSweep = points;
        o.Sweepers.Add(e);
        var s = new Sweep { Entity = e, Job = o.Job, PointsAtStart = points, Started = scoutNow };
        sweeps.Add(s);
        sweepOfGoal[__instance] = s;
    }

    private static void GoalTerminating(object __instance)
    {
        if (__instance.GetType().Name == "GoalScouting") ScoutingTerminating(__instance);
        else if (__instance.GetType().Name == "GoalSearchArea") SearchTerminating(__instance);
    }

    private static void SearchTerminating(object __instance)
    {
        if (!sweepOfGoal.TryGetValue(__instance, out Sweep s) || s.End != null) return;
        s.Visited = CountOf(__instance, "numberOfVisitedLocations");
        int left = CountOf(__instance, "locationsStillToVisit");
        s.End = ((Goal)__instance).Status switch
        {
            Status.Completed => "COMPLETED",
            Status.Failed => left == 0 && s.Visited == 0 ? "FAILED (no point reachable)" : $"FAILED (a move failed, {left} left)",
            _ => $"interrupted ({left} left)",
        };
        s.Ended = scoutNow;
    }

    private static void ScoutingTerminating(object __instance)
    {
        var goal = (Goal)__instance;
        if (GetField(__instance, "job") is ScoutingJob job && orders.Any(o => o.Job == job) && goal.Status == Status.Failed
            && !sweeps.Any(s => s.Entity == goal.entity && s.Job == job && s.End == null))
        {
            scoutEvents.Add($"t {scoutNow,6:0} {Name(goal.entity)}: GoalScouting FAILED before its search (the walk to the zone's edge)");
        }
    }

    private static void ScoutingJobDestroyed(ScoutingJob __instance)
    {
        if (olderOrders.ContainsKey(__instance) && olderOrders[__instance] == null) olderOrders[__instance] = scoutNow;
        Order o = orders.FirstOrDefault(o => o.Job == __instance);
        if (o != null && o.Destroyed == null)
        {
            o.Destroyed = scoutNow;
            scoutEvents.Add($"t {scoutNow,6:0} {o.Name}: the job is destroyed");
        }
    }
}
