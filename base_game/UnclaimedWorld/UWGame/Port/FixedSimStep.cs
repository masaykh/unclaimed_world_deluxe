using System;

namespace UWGame.Port;

/// <summary>
/// PORT DEVIATION 22. The simulation advances in fixed slices of game time, whatever the frame
/// rate; drawing, input and the interface still run once per frame.
///
/// WHY. The studio turned XNA's fixed time step off ("LARS: I get atrocious framerates without
/// these settings!", UnclaimedWorld.cs) and kept vsync, so Sim.Update ran once per monitor
/// refresh - 60, 120, 144 times a second - with ElapsedGameTime the length of that frame. Most of
/// the simulation integrates that elapsed time correctly. Some of it does not: a constant applied
/// per update, a counter of updates used as a timer, a chance rolled every update. Those run
/// twice as fast on a 120 Hz monitor as on a 60 Hz one. Four colonists jammed in a creek for good
/// (Locomotor.ApplyPushVector, save 11) is the one that was seen.
///
/// Running the simulation in steps of exactly 1/<see cref="StepsPerSecond"/> s makes every one
/// of those constants mean the same thing on every machine, without finding and rescaling each.
/// XNA's own IsFixedTimeStep is not used because it fixes DRAWING too, which is what the studio
/// switched off: here Draw still runs at the monitor's rate.
///
/// THE RATE. 60 a second by default, because that is the studio's own figure: AIConstants'
/// TimeAllocatedInSecondsForTimeSlicedSystems is documented as "1 frame, or update, takes
/// 0.016 s", and ProcessJob estimates a worker's progress with CalculateProgressDelta(0.0167, ...).
/// At 60 a 60 Hz player's game is the one the studio shipped, step for step.
///
/// THE CAP. A frame runs at most <see cref="MaxStepsPerFrame"/> steps. Time beyond that is
/// dropped, not owed: on a machine that cannot keep up (or after a long hitch, such as the
/// first frame after loading) the game runs slower rather than spiralling into ever longer
/// frames. The studio's own answer to a long frame was one long step.
///
/// This class is the arithmetic only - no game state - so it can be tested on its own. The
/// accumulator is not saved: a loaded game starts with none.
/// </summary>
public sealed class FixedSimStep
{
    /// <summary>The most steps one frame runs. At 60 a second and four times speed a frame at 30 fps needs 8.</summary>
    public const int MaxStepsPerFrame = 8;

    private long accumulatedTicks;

    /// <summary>Steps per second of game time; 0 is off (one variable step per frame, the studio's way).</summary>
    public int StepsPerSecond { get; private set; }

    /// <summary>The length of one step. Whole ticks, so the game clock never accumulates rounding.</summary>
    public TimeSpan Step { get; private set; }

    /// <summary>Whether the simulation is stepped at a fixed rate.</summary>
    public bool IsOn => StepsPerSecond > 0;

    /// <summary>Game time owed to the simulation and not yet stepped: less than one step after a frame.</summary>
    public TimeSpan Pending => new TimeSpan(accumulatedTicks);

    /// <summary>Game time dropped by the cap since the last <see cref="Reset"/>.</summary>
    public TimeSpan Dropped { get; private set; }

    public FixedSimStep(int stepsPerSecond)
    {
        SetRate(stepsPerSecond);
    }

    /// <summary>Changes the rate. What is pending is kept if it is still less than a step.</summary>
    public void SetRate(int stepsPerSecond)
    {
        if (stepsPerSecond == StepsPerSecond)
        {
            return;
        }
        StepsPerSecond = Math.Max(0, stepsPerSecond);
        Step = IsOn ? new TimeSpan(TimeSpan.TicksPerSecond / StepsPerSecond) : TimeSpan.Zero;
        accumulatedTicks = 0;
    }

    /// <summary>Forgets pending time - at a load, and when a recording or a replay starts.</summary>
    public void Reset()
    {
        accumulatedTicks = 0;
        Dropped = TimeSpan.Zero;
    }

    /// <summary>
    /// Adds one frame's game time (already scaled by the game speed) and returns how many steps to
    /// run now. Only meaningful when <see cref="IsOn"/>.
    /// </summary>
    public int Advance(TimeSpan gameTimeThisFrame)
    {
        if (!IsOn || gameTimeThisFrame <= TimeSpan.Zero)
        {
            return 0;
        }
        accumulatedTicks += gameTimeThisFrame.Ticks;
        long steps = accumulatedTicks / Step.Ticks;
        if (steps > MaxStepsPerFrame)
        {
            long excess = (steps - MaxStepsPerFrame) * Step.Ticks;
            Dropped += new TimeSpan(excess);
            accumulatedTicks -= excess;
            steps = MaxStepsPerFrame;
        }
        accumulatedTicks -= steps * Step.Ticks;
        return (int)steps;
    }
}
