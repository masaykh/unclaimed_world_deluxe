using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework;
using UWGame;
using UWGame.SimSide;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UW.Tools.DataExport;

/// <summary>
/// --save-collisions=&lt;file.sav&gt;: the other side of --save-stuck. Collisions between colonists
/// still have to keep them apart, so on a loaded save's open ground, with the colonists' own AI
/// set aside for three of them:
///   1. two walk head-on into each other along one line: both arrive, and on the way their centres
///      are never closer than they would be at 30 updates a second, the studio's own feel;
///   2. one walks straight through a colleague standing at a worksite: the walker arrives, and the
///      one standing is not shoved more than half a subtile;
///   3. two stand a few pixels apart, as at neighbouring worksites: they are eased apart, not
///      pushed through each other.
/// Each scenario runs at 30, 60, 120 and 144 updates a second; the 30-a-second run is the
/// reference the others are held to.
/// </summary>
internal static partial class Program
{
    private static string collisionReport;

    /// <summary>The goal each scripted colonist runs in place of its own brain; null stands still.</summary>
    private static readonly Dictionary<Entity, Goal> scripted = new();

    private static bool RunScriptedGoal(GoalThink __instance, GameTime elapsed, ref Status __result)
    {
        Entity e = (Entity)GetField(__instance, "entity");
        if (!scripted.TryGetValue(e, out Goal g)) return true;
        __result = Status.Active;
        if (g != null && g.Status != Status.Completed && g.Status != Status.Failed)
        {
            g.Process(elapsed);
        }
        if (g == null || g.Status is Status.Completed or Status.Failed)
        {
            e.Locomotor.MoveSpeed = 0f;
        }
        return false;
    }

    private static int CollisionScenarios(Snapshotter sn, Sim sim)
    {
        var inv = CultureInfo.InvariantCulture;
        var entities = (IDictionary)typeof(LookUp<Entity, EntityID>).GetField("collection", AnyStatic).GetValue(null);
        var renderables = FindField(typeof(UWGame.ClientSide.Client), "renderables");
        renderables.SetValue(The.Client, Activator.CreateInstance(renderables.FieldType, UWGame.SimSide.Systems.Module.Client, false));
        PatchForHeadlessSim();
        new Harmony("uw.dataexport.collisions").Patch(typeof(GoalThink).GetMethod(nameof(GoalThink.ProcessThink)),
            prefix: new HarmonyMethod(typeof(Program).GetMethod(nameof(RunScriptedGoal), AnyStatic)));
        sim.LoadPostProcess(sn);
        typeof(Snapshotter).GetField("snapshotting", AnyStatic).SetValue(null, false);

        List<Entity> people = entities.Values.Cast<Entity>()
            .Where(e => e.EntityType?.KeyName == "entity:human" && e.Intelligence != null && e.IsOnPlaySite()
                        && e.Intelligence.Allegiance == sim.PlaySite.PlayerAllegiance)
            .ToList();
        if (people.Count < 2)
        {
            Console.WriteLine("save collision-test FAILED - fewer than two colonists in this save");
            return 1;
        }
        Point? lane = FindOpenLane(MapManager.WorldPosToSubtile(people[0].PlaySiteLocation), 16, 5);
        if (!lane.HasValue)
        {
            Console.WriteLine("save collision-test FAILED - no open ground near the colony");
            return 1;
        }
        float y = (lane.Value.Y + 2.5f) * MapManager.subTileSize;
        float x0 = (lane.Value.X + 1.5f) * MapManager.subTileSize, x1 = (lane.Value.X + 14.5f) * MapManager.subTileSize;
        Entity a = people[0], b = people[1];
        float radius = a.Collidable.Bounds.BoundsWidth / 2f;
        Console.WriteLine($"==> open ground: subtiles from {lane.Value} on, a lane at y {y.ToString("0", inv)} from x {x0.ToString("0", inv)} to {x1.ToString("0", inv)}; a colonist's collision radius {radius.ToString("0.#", inv)}");

        int failures = 0;
        void Check(bool ok, string what)
        {
            Console.WriteLine((ok ? "  ok    " : "  FAIL  ") + what);
            if (!ok) failures++;
        }

        // Each scenario starts from the save as loaded: reload the state of the three by position.
        var results = new Dictionary<(string, double), (float closest, float shoved, bool arrived)>();
        double[] rates = { 30, 60, 120, 144 };
        foreach (double rate in rates)
        {
            // 1. head-on
            results[("head-on", rate)] = RunScenario(sim, rate, 25, new[]
            {
                (a, new Vector3(x0, y, 0), (Vector3?)new Vector3(x1, y, 0)),
                (b, new Vector3(x1, y, 0), (Vector3?)new Vector3(x0, y, 0)),
            });
            // 2. walking through a colleague at a worksite
            results[("through a worksite", rate)] = RunScenario(sim, rate, 25, new[]
            {
                (a, new Vector3(x0, y, 0), (Vector3?)new Vector3(x1, y, 0)),
                (b, new Vector3((x0 + x1) / 2, y, 0), (Vector3?)null),
            });
            // 3. two standing nearly on top of each other
            results[("standing together", rate)] = RunScenario(sim, rate, 5, new[]
            {
                (a, new Vector3((x0 + x1) / 2 - 2, y, 0), (Vector3?)null),
                (b, new Vector3((x0 + x1) / 2 + 2, y, 0), (Vector3?)null),
            });
        }
        foreach (string name in new[] { "head-on", "through a worksite", "standing together" })
        {
            var reference = results[(name, 30)];
            foreach (double rate in rates)
            {
                var r = results[(name, rate)];
                string what = $"{name} at {rate.ToString(inv)}/s: closest {r.closest.ToString("0.0", inv)} px (30/s: {reference.closest.ToString("0.0", inv)}), "
                              + $"standing one moved {r.shoved.ToString("0.0", inv)} px, walkers arrived {r.arrived}";
                switch (name)
                {
                    case "head-on":
                        Check(r.arrived && r.closest >= reference.closest - 2f, what);
                        break;
                    case "through a worksite":
                        Check(r.arrived && r.shoved <= MapManager.subTileSize / 2f, what);
                        break;
                    default:
                        Check(r.closest > 4f - 0.01f && r.shoved < MapManager.subTileSize, what);
                        break;
                }
            }
        }
        Console.WriteLine(failures == 0 ? "save collision-test OK" : $"save collision-test FAILED - {failures} check(s)");
        return failures == 0 ? 0 : 1;
    }

    private static (float closest, float shoved, bool arrived) RunScenario(Sim sim, double rate, double seconds, (Entity who, Vector3 start, Vector3? goal)[] cast)
    {
        scripted.Clear();
        foreach (var (who, start, goal) in cast)
        {
            who.Location = start;
            who.Locomotor.MoveSpeed = 0f;
            scripted[who] = goal.HasValue
                ? new GoalMoveToPosition(who, goal.Value, new List<EntityGroupID>(), GoalMoveToPosition.VehicleUse.NoVehicle)
                : null;
        }
        float closest = float.MaxValue, shoved = 0f;
        StepSim(sim, seconds, 1.0 / rate, _ => { }, () =>
        {
            for (int i = 0; i < cast.Length; i++)
            {
                for (int j = i + 1; j < cast.Length; j++)
                {
                    closest = Math.Min(closest, Vector2.Distance(cast[i].who.PlaySiteLocation.ToVector2(), cast[j].who.PlaySiteLocation.ToVector2()));
                }
                if (!cast[i].goal.HasValue)
                {
                    shoved = Math.Max(shoved, Vector2.Distance(cast[i].who.PlaySiteLocation.ToVector2(), cast[i].start.ToVector2()));
                }
            }
        });
        bool arrived = cast.Where(c => c.goal.HasValue).All(c => Vector2.Distance(c.who.PlaySiteLocation.ToVector2(), c.goal.Value.ToVector2()) < MapManager.subTileSize);
        scripted.Clear();
        return (closest, shoved, arrived);
    }

    /// <summary>The nearest width x height block of subtiles, all walkable at the plains cost or less.</summary>
    private static Point? FindOpenLane(Point near, int width, int height)
    {
        SubtileLayers foot = The.Map.TerrainCosts[SurfaceType.TransportType.Foot];
        for (int r = 0; r < 120; r++)
        {
            for (int dy = -r; dy <= r; dy++)
            {
                for (int dx = -r; dx <= r; dx++)
                {
                    if (Math.Max(Math.Abs(dx), Math.Abs(dy)) != r) continue;
                    var corner = new Point(near.X + dx, near.Y + dy);
                    bool open = true;
                    for (int y = 0; y < height && open; y++)
                    {
                        for (int x = 0; x < width && open; x++)
                        {
                            var p = new Point(corner.X + x, corner.Y + y);
                            open = The.Map.SubtileIsOnMap(p) && MapManager.GetCost(foot.GetValue(p.X, p.Y)) is > 0 and <= 3
                                   && The.Map.GetTile(MapManager.WorldPosToTile(new Vector3(p.X * 16 + 8, p.Y * 16 + 8, 0))).EntitiesOnTile?.Any(o => o.Intelligence != null) != true;
                        }
                    }
                    if (open) return corner;
                }
            }
        }
        return null;
    }
}
