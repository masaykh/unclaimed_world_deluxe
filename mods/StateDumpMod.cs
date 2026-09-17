using System;
using System.Globalization;
using System.IO;
using System.Text;
using UWGame.SimSide;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Body;

namespace UWGame.Mods;

/// <summary>
/// Writes what the colony is doing, as text, on an interval - so a change can be checked by
/// reading a file instead of by watching a screen.
///
/// WHY. Almost every fix in this repository ends "not verified, needs play", and that is not
/// modesty: there is genuinely no way to see a job score, a hitpoint fraction or what a colonist
/// decided to do next without a person sitting in front of the game describing it. Screenshots
/// answer "does it look right"; they cannot answer "did the wounded one stay out of the fight",
/// which is the question three of the open reports actually turn on.
///
/// WHAT IT IS PART OF. The game already has most of a programmatic interface and nobody noticed,
/// because the pieces are filed under "replay":
///
///   COMMANDS IN    every player action is a Command, XML-serialisable, and ReplayData reads a
///                  Commands.xml and executes each one on the frame it names. A written-out
///                  Commands.xml IS a script of play.
///   TIMING         GameParams.xml names the scenario and map; the recorded frame times are
///                  replayed rather than re-measured.
///   EVIDENCE OUT   ReplayTrace writes a line per frame and a verdict at the end.
///
/// What was missing from that loop is state you can ASSERT on. The trace records one entity's
/// position, which proves determinism and nothing else. This is the other half: enough of the
/// colony, in a greppable format, to write a test about.
///
/// FORMAT. One block per sample, blank-line separated, fields space-separated with a leading
/// keyword - awk-able in one line, diffable between two runs, and readable without a tool:
///
///     == day 3.91 frame 14022
///     person Ana pos 118.40,74.25 hp 1.000 goal Hauling
///     person Bo  pos 96.12,101.80 hp 0.812 goal Salvaging
///     allegiance members 14 persons 6
///
/// OFF BY DEFAULT and writes nothing when off.
/// </summary>
public static class StateDumpMod
{
    public const string ModId = "statedump";

    /// <summary>Where it writes, under the game folder.</summary>
    public const string FileName = "StateDump.txt";

    private static ModSetting enabled;

    private static ModSetting everySeconds;

    private static double lastDumpDays = double.MinValue;

    private static StreamWriter writer;

    private static bool failed;

    /// <summary>Whether to write the dump at all.</summary>
    public static ModSetting Enabled =>
        enabled ?? (enabled = ModSettings.Toggle(
            ModId, "enabled", "WRITE A STATE DUMP", defaultValue: false,
            toolTip: "Writes StateDump.txt in the game folder: every colonist's position, health "
                   + "and current goal, sampled on an interval. For checking a change without "
                   + "watching the screen. Off by default and writes nothing when off."));

    /// <summary>How often to sample, in GAME hours - not real ones.</summary>
    public static ModSetting EverySeconds =>
        everySeconds ?? (everySeconds = ModSettings.Choice(
            ModId, "interval", "STATE DUMP INTERVAL", new string[4] { "1", "6", "12", "24" }, "6",
            toolTip: "Game hours between samples. Six is about four lines per colonist per day, "
                   + "which is enough to see a decision change without producing a file nobody "
                   + "can read."));

    public static void RegisterSettings()
    {
        _ = Enabled;
        _ = EverySeconds;
    }

    /// <summary>
    /// Samples the colony if enough game time has passed. Called every client update; returns on
    /// the first line unless the switch is on.
    /// </summary>
    public static void Sample()
    {
        if (!Enabled.On || failed)
        {
            return;
        }
        Sim sim = The.Sim;
        if (sim == null || sim.Mode != Sim.EngineMode.Game || sim.PlaySite == null)
        {
            return;
        }

        // GAME time, not wall clock. A dump keyed to real seconds samples wildly different
        // amounts of simulation depending on game speed, which makes two runs incomparable -
        // and comparing two runs is the entire point.
        double days = sim.DateAndTime == null ? 0.0 : sim.DateAndTime.CurrentTimeDateYear.TotalDays;
        double interval = ParseHours(EverySeconds.Value) / 24.0;
        if (lastDumpDays != double.MinValue && days - lastDumpDays < interval)
        {
            return;
        }
        lastDumpDays = days;

        try
        {
            Write(sim, days);
        }
        catch (Exception ex)
        {
            // Once, then never again: a dump that cannot be written is not worth a message per
            // frame, and it must not be the thing that stops a game.
            failed = true;
            GameStateManagement.UnclaimedWorld.LogError(
                "The state dump could not be written and has been switched off for this session: "
                + ex.GetType().Name + ": " + ex.Message, "State dump");
        }
    }

    private static double ParseHours(string value)
    {
        return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double h)
            ? h
            : 6.0;
    }

    private static void Write(Sim sim, double days)
    {
        if (writer == null)
        {
            writer = new StreamWriter(FileName, append: false) { AutoFlush = true };
            writer.WriteLine("# Unclaimed World state dump. One block per sample.");
            writer.WriteLine("# person <name> pos <x>,<y> hp <fraction> goal <what they are doing>");
            writer.WriteLine();
        }

        var sb = new StringBuilder();
        // The frame index lives on Controller.recorder, which is private, and reaching it would
        // mean a core change for a number the day already orders adequately.
        sb.Append("== day ").Append(days.ToString("0.00", CultureInfo.InvariantCulture)).AppendLine();

        var allegiance = sim.PlaySite.PlayerAllegiance;
        if (allegiance == null)
        {
            sb.AppendLine("allegiance none");
            writer.WriteLine(sb.ToString());
            return;
        }

        foreach (Entity person in allegiance.Persons)
        {
            if (person == null || !person.IsOnPlaySite())
            {
                continue;
            }
            sb.Append("person ").Append(Sanitise(person.Name));
            sb.Append(" pos ")
              .Append(person.PlaySiteLocation.X.ToString("0.00", CultureInfo.InvariantCulture))
              .Append(',')
              .Append(person.PlaySiteLocation.Y.ToString("0.00", CultureInfo.InvariantCulture));
            sb.Append(" hp ").Append(HitpointFraction(person).ToString("0.000", CultureInfo.InvariantCulture));
            sb.Append(" goal ").Append(Sanitise(CurrentGoal(person)));
            sb.AppendLine();
        }

        sb.Append("allegiance members ").Append(allegiance.Members.Count)
          .Append(" persons ").Append(allegiance.Persons.Count).AppendLine();

        writer.WriteLine(sb.ToString());
    }

    /// <summary>
    /// What this colonist is doing, by the game's own words for it.
    ///
    /// The front subgoal is the live answer - GoalThink is a CompositeGoal and its queue holds
    /// what it decided to do - and Goal.GetStatus is the studio's own description of it, the same
    /// text the interface shows. Falls back to the brain's own status when the queue is empty,
    /// which is a colonist between decisions rather than an error.
    /// </summary>
    private static string CurrentGoal(Entity person)
    {
        GoalThink brain = person.Intelligence?.Brain;
        if (brain == null)
        {
            return "none";
        }
        try
        {
            if (brain.Subgoals != null && brain.Subgoals.Count > 0)
            {
                Goal front = brain.Subgoals.Peek();
                if (front != null)
                {
                    return front.GetStatus() ?? "?";
                }
            }
            return brain.GetStatus() ?? "idle";
        }
        catch (Exception)
        {
            return "?";
        }
    }

    private static float HitpointFraction(Entity person)
    {
        if (!person.Find<BodyComponent>(out var component) || component?.Body == null)
        {
            return 1f;
        }
        Body body = component.Body;
        return body.MaxHitpoints <= 0f ? 1f : body.GlobalHitpoints / body.MaxHitpoints;
    }

    /// <summary>
    /// Keeps a value to one field. Names and goal descriptions contain spaces, and a
    /// space-separated format that lets them through is one nobody can parse.
    /// </summary>
    private static string Sanitise(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "-";
        }
        return value.Replace(' ', '_').Replace('\t', '_').Replace('\r', ' ').Replace('\n', ' ');
    }

    /// <summary>Closes the file at the end of a session.</summary>
    public static void Close()
    {
        try
        {
            writer?.Flush();
            writer?.Dispose();
        }
        catch (Exception)
        {
        }
        writer = null;
        lastDumpDays = double.MinValue;
    }
}
