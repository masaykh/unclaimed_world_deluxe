using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Snapshots;

namespace UW.Tools.DataExport;

/// <summary>
/// --save-steprate=&lt;file.sav&gt;: runs a loaded game for UW_STUCK_SECONDS (default 20) of game
/// time and prints a digest of the simulation's state - every entity's position, facing, health,
/// morale and goals, the game clock and the date. Run it at several frame rates and compare.
///
/// WHY. PORT DEVIATION 22 (UWGame.Port.FixedSimStep) claims the simulation no longer depends on
/// the frame rate. With the fixed step on, the same save fed 30, 60, 120 or 144 frames a second
/// must reach the SAME state, to the bit; with it off (the studio's one update a frame) it must
/// not - which is the dependence the fixed step removes. Game speed and pause must not change it
/// either: twice the speed for half the time, or the same time with the game paused in between,
/// is the same game.
///
/// Environment:
///   UW_STUCK_FPS       drawn frames a second (default 60)
///   UW_STUCK_SECONDS   game seconds to run (default 20)
///   UW_SIM_RATE        OFF, 30 or 60 - PORT's FIXED SIMULATION RATE (default: the setting's)
///   UW_SIM_SPEED       1, 2 or 4 - the game speed; the run is that much shorter in real time
///   UW_PAUSE_FRAMES    n: after every n frames, n more frames with the game paused
///   UW_CYCLES          time-sliced AI cycles per update (default 1000, more than an update ever queues here). The game's own budget is
///                      nine milliseconds of WALL CLOCK an update (CycleManager), which no two runs
///                      spend alike; a count is what makes two runs comparable at all. "clock"
///                      keeps the wall clock, and reports how many cycles it bought.
///   UW_STATE_OUT       also write the state, one entity a line, to this file
/// </summary>
internal static partial class Program
{
    private static string stepRateReport;

    private static int StepRateReport(Snapshotter sn, Sim sim)
    {
        var inv = CultureInfo.InvariantCulture;
        var renderables = FindField(typeof(UWGame.ClientSide.Client), "renderables");
        renderables.SetValue(The.Client, Activator.CreateInstance(renderables.FieldType, UWGame.SimSide.Systems.Module.Client, false));
        PatchForHeadlessSim();
        sim.LoadPostProcess(sn);
        typeof(Snapshotter).GetField("snapshotting", AnyStatic).SetValue(null, false);

        double seconds = double.Parse(Environment.GetEnvironmentVariable("UW_STUCK_SECONDS") ?? "20", inv);
        double fps = double.Parse(Environment.GetEnvironmentVariable("UW_STUCK_FPS") ?? "60", inv);
        int speed = int.Parse(Environment.GetEnvironmentVariable("UW_SIM_SPEED") ?? "1", inv);
        int pauseFrames = int.Parse(Environment.GetEnvironmentVariable("UW_PAUSE_FRAMES") ?? "0", inv);
        string cycles = Environment.GetEnvironmentVariable("UW_CYCLES") ?? "1000";
        ApplySimulationRateFromEnvironment();
        if (cycles != "clock")
        {
            sim.CycleManager.CycleSchedule = new[] { int.Parse(cycles, inv) };
        }
        sim.SetGameSpeed(speed switch { 2 => Speeds.TwiceNormal, 4 => Speeds.FourTimesNormal, _ => Speeds.Normal });
        Speeds running = sim.Speed;
        sim.FixedStep.Reset();

        Console.WriteLine($"==> {seconds.ToString(inv)} s of game time at {fps.ToString(inv)} frames a second, speed x{speed}, "
                          + $"{DescribeSimulationRate()}, {(cycles == "clock" ? "AI work by the clock" : cycles + " AI cycles an update")}"
                          + (pauseFrames > 0 ? $", paused for {pauseFrames} frame(s) after every {pauseFrames}" : ""));

        // As StepSim, with the paused frames inserted: they are drawn frames too, and must add nothing.
        long frames = (long)Math.Round(seconds / speed * fps);
        long previousTicks = 0, rawTicks = 0, updates = 0, cyclesRun = 0, maxUpdatesInFrame = 0;
        for (long f = 0; f < frames; f++)
        {
            long ticks = (long)Math.Round((f + 1) * TimeSpan.TicksPerSecond / fps);
            RunOneFrame(sim, ref rawTicks, ticks - previousTicks, ref updates, ref cyclesRun, ref maxUpdatesInFrame);
            previousTicks = ticks;
            if (pauseFrames > 0 && (f + 1) % pauseFrames == 0)
            {
                sim.PauseGame();
                for (int p = 0; p < pauseFrames; p++)
                {
                    RunOneFrame(sim, ref rawTicks, (long)Math.Round(TimeSpan.TicksPerSecond / fps), ref updates, ref cyclesRun, ref maxUpdatesInFrame);
                }
                sim.SetGameSpeed(running);
            }
        }
        foreach (var kv in swallowed.OrderByDescending(kv => kv.Value))
        {
            Console.WriteLine($"    (headless stand-in) {kv.Value}x {kv.Key}");
        }
        Console.WriteLine($"    {updates} simulation update(s) in {frames} running frame(s), at most {maxUpdatesInFrame} in one; "
                          + $"{cyclesRun} AI cycles ({(updates == 0 ? 0 : cyclesRun / (double)updates).ToString("0.0", inv)} an update)");

        string state = DescribeState(sim);
        string outPath = Environment.GetEnvironmentVariable("UW_STATE_OUT");
        if (!string.IsNullOrEmpty(outPath))
        {
            File.WriteAllText(outPath, state);
        }
        string digest = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(state))).Substring(0, 16);
        Console.WriteLine($"    game clock {sim.TotalUnPausedGameTime.Ticks} ticks, day {sim.DateAndTime.CurrentTimeDateYear.TotalDays.ToString("R", inv)}");
        Console.WriteLine($"save step-rate state {digest}");
        return 0;
    }

    private static void RunOneFrame(Sim sim, ref long rawTicks, long elapsedTicks, ref long updates, ref long cyclesRun, ref long maxUpdatesInFrame)
    {
        rawTicks += elapsedTicks;
        var frame = new GameTime(new TimeSpan(rawTicks), new TimeSpan(elapsedTicks));
        Guard("frame", () => sim.RunFrame(sim.ScaleToGameSpeed(frame)));
        if (!sim.IsPaused)
        {
            IReadOnlyList<int> ran = sim.CycleManager.CyclesThisFrame;
            updates += ran.Count;
            cyclesRun += ran.Sum();
            maxUpdatesInFrame = Math.Max(maxUpdatesInFrame, ran.Count);
        }
    }

    /// <summary>UW_SIM_RATE, when set, is PORT's FIXED SIMULATION RATE for this run.</summary>
    private static void ApplySimulationRateFromEnvironment()
    {
        string rate = Environment.GetEnvironmentVariable("UW_SIM_RATE");
        if (!string.IsNullOrEmpty(rate))
        {
            UWGame.Mods.PortSettings.SimulationRate.Value = rate;
        }
    }

    private static string DescribeSimulationRate()
    {
        int rate = UWGame.Mods.PortSettings.SimulationStepsPerSecond;
        return rate > 0 ? $"fixed simulation step {rate} a second" : "one simulation update a frame (fixed step OFF)";
    }

    /// <summary>The simulation's state, one entity a line in ID order: floats as their exact bits.</summary>
    private static string DescribeState(Sim sim)
    {
        var inv = CultureInfo.InvariantCulture;
        var entities = (IDictionary)typeof(LookUp<Entity, EntityID>).GetField("collection", AnyStatic).GetValue(null);
        var sb = new StringBuilder();
        sb.Append("clock ").Append(sim.TotalUnPausedGameTime.Ticks.ToString(inv))
          .Append(" day ").Append(sim.DateAndTime.CurrentTimeDateYear.TotalDays.ToString("R", inv))
          .Append(" entities ").Append(entities.Count.ToString(inv)).Append('\n');
        foreach (Entity e in entities.Values.Cast<Entity>().OrderBy(e => (long)e.ID))
        {
            if (!e.IsOnPlaySite())
            {
                continue;
            }
            Vector3 p = e.PlaySiteLocation;
            Vector3 n = e.FacingNormal;
            sb.Append((long)e.ID).Append(' ').Append(Name(e))
              .Append(" at ").Append(p.X.ToString("R", inv)).Append(',').Append(p.Y.ToString("R", inv))
              .Append(" facing ").Append(n.X.ToString("R", inv)).Append(',').Append(n.Y.ToString("R", inv));
            if (e.Find<BodyComponent>(out var body) && body?.Body != null)
            {
                sb.Append(" hp ").Append(body.Body.GlobalHitpoints.ToString("R", inv));
            }
            if (e.Intelligence != null)
            {
                sb.Append(" morale ").Append(e.Intelligence.Morale.ToString("R", inv))
                  .Append(" doing ").Append(GoalChain(e));
            }
            sb.Append('\n');
        }
        return sb.ToString();
    }
}
