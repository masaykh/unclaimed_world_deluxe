using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame;
using UWGame.SimSide;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UW.Tools.DataExport;

/// <summary>
/// --save-stuck=&lt;file.sav&gt;: reads a saved game as --save-selftest does, then RUNS it for a
/// minute of game time with no window, and checks that every colonist who was walking when the
/// game was saved has got past the waypoint it was walking to.
///
/// WHY. "Four colonists 'stuck' on small place between two masses of land" (save 11,
/// 2026-10-18). They stood at one shallow-water subtile of a creek, each walking at the
/// 16 px/s rough-ground floor into the same waypoint, each pushed back 0.3 px an update by the
/// others (AgentCollisionResponder, Locomotor.ApplyPushVector) - more than it walked at 60 updates
/// a second. That needs the simulation running to see: the save's own fields only show four
/// colonists one step from their waypoint.
///
/// HOW. Stepping a loaded game needs Sim.LoadPostProcess, which builds renderables, and goals
/// that read them. StuckSim.cs stands in for those (see PatchForHeadlessSim); the movement,
/// collision and AI code that decides whether anyone moves is the game's own. What the stand-ins
/// cannot do: animation-timed steps (a pickup waits for its animation, and here that never
/// comes), so a colonist can end the minute bent over an item it reached. That is why the check
/// is "got past the waypoint", not "finished the job".
///
/// UW_STUCK_FPS (default 60) sets the updates a second, UW_STUCK_SECONDS (default 60) how long.
/// The jam this was written for clears at 30 and holds at 55 and above without the fix.
/// </summary>
internal static partial class Program
{
    private static string stuckReport;

    private sealed class Walker
    {
        public Entity Entity;
        public Vector3 From;
        public Vector3 To;
    }

    private static int StuckReport(Snapshotter sn, Sim sim)
    {
        var inv = CultureInfo.InvariantCulture;
        var entities = (IDictionary)typeof(LookUp<Entity, EntityID>).GetField("collection", AnyStatic).GetValue(null);

        // A real client's renderable list; PatchForHeadlessSim makes every renderable inert.
        var renderables = FindField(typeof(UWGame.ClientSide.Client), "renderables");
        renderables.SetValue(The.Client, Activator.CreateInstance(renderables.FieldType, UWGame.SimSide.Systems.Module.Client, false));
        PatchForHeadlessSim();
        sim.LoadPostProcess(sn);
        // Snapshotter.Load ends the same way; left on, goals take the loading branch of their code.
        typeof(Snapshotter).GetField("snapshotting", AnyStatic).SetValue(null, false);

        var walkers = new List<Walker>();
        foreach (Entity e in entities.Values)
        {
            if (e.Intelligence == null || !e.IsOnPlaySite() || e.Intelligence.Allegiance != sim.PlaySite.PlayerAllegiance) continue;
            if (WaypointBeingWalkedTo(e) is Vector3 to)
            {
                walkers.Add(new Walker { Entity = e, From = e.PlaySiteLocation, To = to });
            }
        }
        Console.WriteLine($"==> {walkers.Count} colonist(s) walking when the game was saved");
        foreach (Walker w in walkers)
        {
            Console.WriteLine($"    {Name(w.Entity)} at {Show(w.From)}, subtile {Subtile(w.From)}, walking to waypoint {Show(w.To)}: {GoalChain(w.Entity)}");
        }

        double seconds = double.Parse(Environment.GetEnvironmentVariable("UW_STUCK_SECONDS") ?? "60", inv);
        double fps = double.Parse(Environment.GetEnvironmentVariable("UW_STUCK_FPS") ?? "60", inv);
        Console.WriteLine($"==> running the game for {seconds.ToString(inv)} s at {fps.ToString(inv)} updates a second");
        StepSim(sim, seconds, 1.0 / fps, t =>
        {
            Console.WriteLine($"    t+{t.ToString("0", inv),-3} " + string.Join("  ", walkers.Select(w => Name(w.Entity) + Show(w.Entity.PlaySiteLocation))));
        });
        foreach (var kv in swallowed.OrderByDescending(kv => kv.Value))
        {
            Console.WriteLine($"    (headless stand-in) {kv.Value}x {kv.Key}");
        }

        var stuck = new List<Walker>();
        foreach (Walker w in walkers)
        {
            bool stillOnIt = WaypointBeingWalkedTo(w.Entity) == w.To;
            Console.WriteLine((stillOnIt ? "  FAIL  " : "  ok    ") + $"{Name(w.Entity)} " + (stillOnIt
                ? $"is still walking to {Show(w.To)} after {seconds.ToString(inv)} s, at {Show(w.Entity.PlaySiteLocation)} ({Vector3.Distance(w.Entity.PlaySiteLocation, w.From).ToString("0", inv)} px from where it was)"
                : $"got past {Show(w.To)}; now at {Show(w.Entity.PlaySiteLocation)}, {GoalChain(w.Entity)}"));
            if (stillOnIt) stuck.Add(w);
        }
        if (stuck.Count > 0)
        {
            Vector3 centre = stuck.Aggregate(Vector3.Zero, (sum, w) => sum + w.Entity.PlaySiteLocation) / stuck.Count;
            DumpFootTerrain(Subtile(centre), 8);
        }
        Console.WriteLine(walkers.Count == 0 ? "save stuck-test FAILED - nobody was walking in this save, so it checks nothing"
                          : stuck.Count == 0 ? "save stuck-test OK" : $"save stuck-test FAILED - {stuck.Count} colonist(s) stuck");
        return walkers.Count > 0 && stuck.Count == 0 ? 0 : 1;
    }

    /// <summary>
    /// The waypoint of the GoalTraverseEdgeBetweenWaypoints (internal to the game) the colonist is
    /// carrying out, down its first subgoals - or null when it is not walking one.
    /// </summary>
    private static Vector3? WaypointBeingWalkedTo(Entity e)
    {
        Goal g = e.Intelligence.Brain;
        while (g is CompositeGoal c && c.Subgoals.Count > 0)
        {
            g = c.Subgoals.Peek();
            if (g.GetType().Name == "GoalTraverseEdgeBetweenWaypoints") return (Vector3)GetField(g, "To");
        }
        return null;
    }

    private static string GoalChain(Entity e)
    {
        var names = new List<string>();
        Goal g = e.Intelligence.Brain;
        while (g is CompositeGoal c && c.Subgoals.Count > 0)
        {
            g = c.Subgoals.Peek();
            names.Add(g.GetType().Name);
        }
        return string.Join(" > ", names);
    }

    private static string Name(Entity e) => string.IsNullOrEmpty(e.Intelligence?.FirstName) ? e.EntityType.KeyName + "#" + (long)e.ID : e.Intelligence.FirstName;

    private static Point Subtile(Vector3 p) => MapManager.WorldPosToSubtile(p);

    private static string Show(Vector3 p) => $"({p.X.ToString("0", CultureInfo.InvariantCulture)},{p.Y.ToString("0", CultureInfo.InvariantCulture)})";

    /// <summary>The foot terrain costs around a subtile: '##' blocked, else the cost.</summary>
    private static void DumpFootTerrain(Point centre, int reach)
    {
        SubtileLayers foot = The.Map.TerrainCosts[SurfaceType.TransportType.Foot];
        Console.WriteLine($"==> foot terrain costs around subtile {centre} ('##' blocked)");
        Console.Write("        ");
        for (int x = centre.X - reach; x <= centre.X + reach; x++) Console.Write($"{x,4}");
        Console.WriteLine();
        for (int y = centre.Y - reach; y <= centre.Y + reach; y++)
        {
            Console.Write($"    {y,4}");
            for (int x = centre.X - reach; x <= centre.X + reach; x++)
            {
                bool on = The.Map.SubtileIsOnMap(new Point(x, y));
                int cost = on ? MapManager.GetCost(foot.GetValue(x, y)) : 0;
                Console.Write(!on ? "    " : cost == 0 ? "  ##" : $"{cost,4}");
            }
            Console.WriteLine();
        }
    }
}
