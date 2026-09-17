using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using UWGame.Control;
using UWGame.Control.Commands;
using UWGame.SimSide;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Commands;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Body;

namespace UWGame.Mods;

/// <summary>
/// Lets something that is not a person play the game, by turning the colony into files it can
/// read and a file it can write.
///
/// WHY FILES. Almost all of this already existed and was filed under "replay". Every player
/// action is a <see cref="Command"/>, all thirty-six of them registered on the base class with
/// XmlInclude and therefore XML-serialisable; Controller.StoreAndExecuteCommand is the single
/// funnel they pass through; and ReplayData already reads a Commands.xml and executes each entry
/// on the frame it names. So a written-out Commands.xml IS a script of play, and the format an
/// agent should speak is the game's own - no new schema, and no parser to keep in step with it.
///
/// What was missing is a LIVE channel and turn-taking, which is all this is.
///
///   agent/commands.txt   written once: every command the game accepts and its fields.
///   agent/observe.txt    written when it is the agent's turn. The game is paused.
///   agent/act.xml        the agent writes this. Read, executed, deleted.
///   agent/log.txt        what was executed, and why anything was refused.
///
/// TURN-TAKING IS THE POINT. Between turns the simulation is PAUSED, so however long a model
/// takes to think costs the colony nothing and changes nothing. After the actions are applied it
/// runs for a fixed number of GAME seconds and pauses again. Without that the world would run on
/// during inference and no two sessions would be comparable.
///
/// AND EVERY SESSION IS A REPLAY. The commands go through StoreAndExecuteCommand like a human's
/// clicks, so with RECORD SESSIONS FOR REPLAY on, an agent's game records to user/Replays and can
/// be played back and checked frame for frame - which is only worth anything because of the work
/// schedule recorded by CycleSchedule. A run an agent made can be proved to have happened.
///
/// OFF BY DEFAULT and touches nothing when off.
/// </summary>
public static class AgentMod
{
    public const string ModId = "agent";

    /// <summary>Where the conversation happens, under the game folder.</summary>
    public const string FolderName = "agent";

    public const string ObserveFile = "observe.txt";

    public const string ActFile = "act.xml";

    public const string CommandsFile = "commands.txt";

    public const string LogFile = "log.txt";

    private enum Phase
    {
        /// <summary>Nothing started yet, or the game was left and rejoined.</summary>
        Fresh,

        /// <summary>Paused, observation on disk, waiting for act.xml.</summary>
        AwaitingAction,

        /// <summary>Running out the turn.</summary>
        Playing
    }

    private static ModSetting enabled;

    private static ModSetting turnSeconds;

    private static Phase phase = Phase.Fresh;

    private static int turn;

    private static double turnEndsAtSeconds;

    private static bool wroteCommandList;

    private static bool failed;

    private static XmlSerializer serializer;

    /// <summary>Whether to hand the game over to an agent at all.</summary>
    public static ModSetting Enabled =>
        enabled ?? (enabled = ModSettings.Toggle(
            ModId, "enabled", "PLAY BY FILE (AGENT)", defaultValue: false,
            toolTip: "Pauses the game and writes agent/observe.txt, then waits for agent/act.xml "
                   + "- a Commands.xml fragment - executes it, and plays on for one turn. For "
                   + "driving the game from a program. Off by default and does nothing when off."));

    /// <summary>How long a turn runs, in GAME seconds.</summary>
    public static ModSetting TurnSeconds =>
        turnSeconds ?? (turnSeconds = ModSettings.Choice(
            ModId, "turnSeconds", "AGENT TURN LENGTH",
            new string[5] { "5", "15", "60", "300", "1800" }, "60",
            toolTip: "Game seconds the colony runs for after each set of actions, before it "
                   + "pauses and asks again. A day is 1600 game seconds at normal speed."));

    public static void RegisterSettings()
    {
        _ = Enabled;
        _ = TurnSeconds;
    }

    /// <summary>Whether this mod is doing anything.</summary>
    public static bool IsEnabled
    {
        get
        {
            RegisterSettings();
            return Enabled.On;
        }
    }

    /// <summary>
    /// One step of the conversation. Called every client update; returns on the first line unless
    /// the switch is on.
    /// </summary>
    public static void Tick(Controller controller)
    {
        if (!IsEnabled || failed || controller == null)
        {
            return;
        }
        Sim sim = The.Sim;
        if (sim == null || sim.Mode != Sim.EngineMode.Game || sim.PlaySite == null
            || sim.PlaySite.PlayerAllegiance == null)
        {
            return;
        }

        try
        {
            Directory.CreateDirectory(FolderName);
            WriteCommandList();

            switch (phase)
            {
                case Phase.Fresh:
                    BeginTurn(controller, sim);
                    break;

                case Phase.AwaitingAction:
                    if (TryTakeAction(controller, sim))
                    {
                        turnEndsAtSeconds = sim.TotalUnPausedGameTimeInSeconds + TurnLengthSeconds();
                        Send(controller, new Resume());
                        phase = Phase.Playing;
                    }
                    break;

                case Phase.Playing:
                    if (sim.TotalUnPausedGameTimeInSeconds >= turnEndsAtSeconds)
                    {
                        BeginTurn(controller, sim);
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            // Once, then never again. A conversation that cannot be held must not be the thing
            // that stops a game - and a message per frame would bury the reason for it.
            failed = true;
            GameStateManagement.UnclaimedWorld.LogError(
                "The agent channel has been switched off for this session: "
                + ex.GetType().Name + ": " + ex.Message, "Agent");
        }
    }

    private static double TurnLengthSeconds()
    {
        return double.TryParse(TurnSeconds.Value, NumberStyles.Float, CultureInfo.InvariantCulture,
            out double s) && s > 0.0 ? s : 60.0;
    }

    /// <summary>Pauses, writes the observation, and waits.</summary>
    private static void BeginTurn(Controller controller, Sim sim)
    {
        turn++;
        Send(controller, new Pause());
        File.WriteAllText(Path.Combine(FolderName, ObserveFile), Observe(sim));
        phase = Phase.AwaitingAction;
    }

    /// <summary>
    /// Pause and Resume go through StoreAndExecuteCommand rather than calling Sim.PauseGame
    /// directly, so that a recorded session contains them and replays the way a human's pauses do.
    /// </summary>
    private static void Send(Controller controller, Command command)
    {
        controller.StoreAndExecuteCommand(command);
    }

    // ---- reading the agent's move -------------------------------------------------------------

    /// <summary>
    /// Executes agent/act.xml if it is there and parses. Returns whether the turn may start.
    ///
    /// A file that is half-written when we look at it throws, and we simply try again next frame -
    /// the agent is not required to write atomically. A file that is complete but WRONG is a
    /// different thing: it is renamed to act.bad.xml and the reason logged, because retrying it
    /// forever would look like the game ignoring the agent.
    /// </summary>
    private static bool TryTakeAction(Controller controller, Sim sim)
    {
        string path = Path.Combine(FolderName, ActFile);
        if (!File.Exists(path))
        {
            return false;
        }

        string text;
        try
        {
            text = File.ReadAllText(path);
        }
        catch (IOException)
        {
            return false;
        }
        if (text.Trim().Length == 0)
        {
            return false;
        }

        List<Command> commands;
        try
        {
            commands = Deserialize(text);
        }
        catch (Exception ex)
        {
            Reject(path, "could not be read as a list of commands: "
                + ex.GetType().Name + ": " + (ex.InnerException?.Message ?? ex.Message));
            return false;
        }

        var log = new StringBuilder();
        log.Append("turn ").Append(turn.ToString(CultureInfo.InvariantCulture))
           .Append("  day ").Append(Days(sim).ToString("0.0000", CultureInfo.InvariantCulture))
           .AppendLine();
        int done = 0;
        foreach (Command command in commands)
        {
            if (command == null)
            {
                continue;
            }
            try
            {
                Send(controller, command);
                done++;
                log.Append("    ok      ").AppendLine(command.GetType().Name);
            }
            catch (Exception ex)
            {
                // One command failing is the agent's problem to see and correct, not a reason to
                // drop the rest of its move or to stop the game.
                log.Append("    FAILED  ").Append(command.GetType().Name).Append("  ")
                   .Append(ex.GetType().Name).Append(": ").AppendLine(ex.Message);
            }
        }
        log.Append("    ").Append(done.ToString(CultureInfo.InvariantCulture)).Append(" of ")
           .Append(commands.Count.ToString(CultureInfo.InvariantCulture))
           .AppendLine(" executed").AppendLine();
        Append(log.ToString());

        Delete(path);
        return true;
    }

    /// <summary>
    /// The game's own XmlSerializer, on the game's own list type - which is what makes act.xml and
    /// a recording's Commands.xml the same format, and means a move can be pasted out of one into
    /// the other.
    /// </summary>
    private static List<Command> Deserialize(string text)
    {
        serializer = serializer ?? new XmlSerializer(typeof(List<Command>));
        using var reader = new StringReader(text);
        return (List<Command>)serializer.Deserialize(reader) ?? new List<Command>();
    }

    private static void Reject(string path, string why)
    {
        Append("turn " + turn.ToString(CultureInfo.InvariantCulture) + "  REJECTED " + ActFile
            + Environment.NewLine + "    " + why + Environment.NewLine
            + "    kept as act.bad.xml; write a new " + ActFile + Environment.NewLine
            + Environment.NewLine);
        try
        {
            string bad = Path.Combine(FolderName, "act.bad.xml");
            File.Delete(bad);
            File.Move(path, bad);
        }
        catch (Exception)
        {
            Delete(path);
        }
    }

    private static void Delete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (Exception)
        {
        }
    }

    private static void Append(string text)
    {
        try
        {
            File.AppendAllText(Path.Combine(FolderName, LogFile), text);
        }
        catch (Exception)
        {
        }
    }

    // ---- writing what the colony looks like ---------------------------------------------------

    private static double Days(Sim sim)
    {
        return sim.DateAndTime == null ? 0.0 : sim.DateAndTime.CurrentTimeDateYear.TotalDays;
    }

    /// <summary>
    /// The colony as text.
    ///
    /// WHAT IS AND IS NOT HERE. Colonists - who they are, where they are, how hurt, and the whole
    /// goal chain that says what they decided to do - and the entities around them. Stockpile
    /// contents and the job board are NOT here yet, and a colony cannot be fed without them; they
    /// are the next thing, and worth designing once something has actually played a turn and shown
    /// what it asks for. Said plainly in the file rather than left to be discovered.
    /// </summary>
    private static string Observe(Sim sim)
    {
        var sb = new StringBuilder();
        Allegiance allegiance = sim.PlaySite.PlayerAllegiance;

        sb.Append("turn ").Append(turn.ToString(CultureInfo.InvariantCulture))
          .Append("   day ").Append(Days(sim).ToString("0.0000", CultureInfo.InvariantCulture))
          .Append("   gameSeconds ")
          .Append(sim.TotalUnPausedGameTimeInSeconds.ToString("0.0", CultureInfo.InvariantCulture))
          .Append("   turnLength ")
          .Append(TurnLengthSeconds().ToString("0", CultureInfo.InvariantCulture))
          .AppendLine();
        sb.AppendLine("# The game is PAUSED and waiting. Write " + FolderName + "/" + ActFile
            + " to act; it is a");
        sb.AppendLine("# Commands.xml fragment. " + FolderName + "/" + CommandsFile
            + " lists every command and its fields.");
        sb.AppendLine("# An empty <ArrayOfCommand/> is a legal move: do nothing, play on.");
        sb.AppendLine("# Positions are WORLD units; a tile is 48 of them. Ids are what commands take.");
        sb.AppendLine();

        sb.Append("allegiance members ")
          .Append(allegiance.Members.Count.ToString(CultureInfo.InvariantCulture))
          .Append(" persons ")
          .Append(allegiance.Persons.Count.ToString(CultureInfo.InvariantCulture))
          .AppendLine();
        sb.AppendLine();

        sb.AppendLine("# person <id> <name> at <x>,<y> tile <tx>,<ty> hp <fraction> goal <chain>");
        foreach (Entity person in allegiance.Persons)
        {
            if (person == null || !person.IsOnPlaySite())
            {
                continue;
            }
            sb.Append("person ").Append(((long)person.ID).ToString(CultureInfo.InvariantCulture))
              .Append(' ').Append(Sanitise(person.Name));
            AppendWhere(sb, person);
            sb.Append(" hp ")
              .Append(HitpointFraction(person).ToString("0.000", CultureInfo.InvariantCulture));
            sb.Append(" goal ").Append(Sanitise(GoalChain(person)));
            sb.AppendLine();
        }
        sb.AppendLine();

        AppendOthers(sb, sim, allegiance);
        return sb.ToString();
    }

    private static void AppendWhere(StringBuilder sb, Entity entity)
    {
        var at = entity.PlaySiteLocation;
        sb.Append(" at ").Append(at.X.ToString("0.0", CultureInfo.InvariantCulture))
          .Append(',').Append(at.Y.ToString("0.0", CultureInfo.InvariantCulture));
        sb.Append(" tile ").Append(((int)(at.X / 48f)).ToString(CultureInfo.InvariantCulture))
          .Append(',').Append(((int)(at.Y / 48f)).ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Everything else on the site that is not ours, nearest first and capped. Uncapped this is
    /// the whole wildlife population of a large map, which is not an observation, it is a dump.
    /// </summary>
    private static void AppendOthers(StringBuilder sb, Sim sim, Allegiance ours)
    {
        const int limit = 60;
        sb.AppendLine("# other <id> <type> at <x>,<y> tile <tx>,<ty> allegiance <kind>"
            + "   (nearest " + limit + ")");

        var mine = new List<Entity>();
        foreach (Entity person in ours.Persons)
        {
            if (person != null && person.IsOnPlaySite())
            {
                mine.Add(person);
            }
        }
        if (mine.Count == 0)
        {
            sb.AppendLine("# no colonists on site, so nothing to be near");
            return;
        }

        var found = new List<KeyValuePair<float, string>>();
        foreach (Entity entity in sim.PlaySite.Entities.GetAsList())
        {
            if (entity == null || !entity.IsOnPlaySite())
            {
                continue;
            }
            Allegiance theirs = entity.Intelligence?.Allegiance;
            if (theirs == ours)
            {
                continue;
            }
            float nearest = float.MaxValue;
            foreach (Entity person in mine)
            {
                float dx = entity.PlaySiteLocation.X - person.PlaySiteLocation.X;
                float dy = entity.PlaySiteLocation.Y - person.PlaySiteLocation.Y;
                float d2 = dx * dx + dy * dy;
                if (d2 < nearest)
                {
                    nearest = d2;
                }
            }

            var row = new StringBuilder();
            row.Append("other ").Append(((long)entity.ID).ToString(CultureInfo.InvariantCulture))
               .Append(' ').Append(Sanitise(entity.KeyName));
            AppendWhere(row, entity);
            row.Append(" allegiance ").Append(theirs?.AllegianceType.ToString() ?? "none");
            found.Add(new KeyValuePair<float, string>(nearest, row.ToString()));
        }

        found.Sort((x, y) => x.Key.CompareTo(y.Key));
        int written = 0;
        foreach (var entry in found)
        {
            if (written++ >= limit)
            {
                break;
            }
            sb.AppendLine(entry.Value);
        }
        if (found.Count > limit)
        {
            sb.Append("# ").Append((found.Count - limit).ToString(CultureInfo.InvariantCulture))
              .AppendLine(" further entities not listed");
        }
        sb.AppendLine();
        sb.AppendLine("# NOT YET REPORTED: stockpile contents, the job board, weather, buildable");
        sb.AppendLine("# structures. A colony cannot be fed from this file alone. Say what is");
        sb.AppendLine("# missing and it goes in - the format is the easy half.");
    }

    private static string GoalChain(Entity person)
    {
        GoalThink brain = person.Intelligence?.Brain;
        if (brain == null)
        {
            return "none";
        }
        try
        {
            var chain = new StringBuilder();
            Goal goal = brain;
            for (int depth = 0; depth < 16; depth++)
            {
                if (chain.Length > 0)
                {
                    chain.Append('>');
                }
                chain.Append(goal.GetType().Name);
                if (!(goal is CompositeGoal composite) || composite.Subgoals == null
                    || composite.Subgoals.Count == 0)
                {
                    break;
                }
                Goal next = composite.Subgoals.Peek();
                if (next == null)
                {
                    break;
                }
                goal = next;
            }
            return chain.ToString();
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

    private static string Sanitise(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "-";
        }
        return value.Replace(' ', '_').Replace('\t', '_').Replace('\r', ' ').Replace('\n', ' ');
    }

    // ---- the command list ---------------------------------------------------------------------

    /// <summary>
    /// Writes every command the game accepts and the fields it carries, once.
    ///
    /// Taken by REFLECTION from the XmlInclude attributes on Command, so this is not a list that
    /// can fall out of step with the game: it is the same list the serialiser itself uses. A
    /// command the studio added appears here without anyone remembering to add it.
    /// </summary>
    private static void WriteCommandList()
    {
        if (wroteCommandList)
        {
            return;
        }
        wroteCommandList = true;

        var sb = new StringBuilder();
        sb.AppendLine("# Every command this game accepts, and the fields each one carries.");
        sb.AppendLine("# Read by reflection from the XmlInclude attributes on Command, so this is");
        sb.AppendLine("# the same list its XML serialiser uses.");
        sb.AppendLine("#");
        sb.AppendLine("# " + ActFile + " is a list of these, in the format a recording's");
        sb.AppendLine("# Commands.xml uses. A move that does nothing looks like this:");
        sb.AppendLine("#");
        sb.AppendLine("#     <ArrayOfCommand />");
        sb.AppendLine("#");
        sb.AppendLine("# The surest way to get the exact shape of any command is to do it once by");
        sb.AppendLine("# hand in game with RECORD SESSIONS FOR REPLAY on, and read it out of");
        sb.AppendLine("# user/Replays/<session>/Commands.xml. That file and this one are the same");
        sb.AppendLine("# format, so a move can be pasted from one into the other.");
        sb.AppendLine();

        var names = new List<string>();
        foreach (object attribute in typeof(Command)
            .GetCustomAttributes(typeof(XmlIncludeAttribute), inherit: false))
        {
            Type type = ((XmlIncludeAttribute)attribute).Type;
            if (type == null)
            {
                continue;
            }
            var entry = new StringBuilder();
            entry.AppendLine(type.Name);
            foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (string.Equals(field.Name, "frameCalled", StringComparison.Ordinal))
                {
                    continue;
                }
                entry.Append("    ").Append(field.Name.PadRight(28))
                     .AppendLine(Readable(field.FieldType));
            }
            names.Add(entry.ToString());
        }
        names.Sort(StringComparer.OrdinalIgnoreCase);
        foreach (string entry in names)
        {
            sb.Append(entry);
        }

        try
        {
            File.WriteAllText(Path.Combine(FolderName, CommandsFile), sb.ToString());
        }
        catch (Exception)
        {
        }
    }

    private static string Readable(Type type)
    {
        Type inner = Nullable.GetUnderlyingType(type);
        if (inner != null)
        {
            return Readable(inner) + "?";
        }
        if (type.IsEnum)
        {
            return type.Name + " (" + string.Join("|", Enum.GetNames(type)) + ")";
        }
        return type.Name;
    }

    /// <summary>Forgets the conversation at the end of a session.</summary>
    public static void Reset()
    {
        phase = Phase.Fresh;
        turn = 0;
        turnEndsAtSeconds = 0.0;
        wroteCommandList = false;
    }
}
