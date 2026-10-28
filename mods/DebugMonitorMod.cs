using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using UWGame.SimSide;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Snapshots;

namespace UWGame.Mods;

/// <summary>
/// A live window onto the simulation, outside the game: every colonist, creature, item and
/// building, and for the one picked, its goal tree, scores, script variables and every field of
/// every component - refreshed four times a second.
///
/// WHY. tripleacoder, #dev_chat_with_claude, 2026-10-28: "For my ai work, i mostly need a way to
/// see the goal tree. It can be quite large, so there is no room in the side panel." The studio
/// had this as a WinForms dialog (Kensei.Dev.Options.OptionsForm, original_src
/// DevelopmentTools/Options.cs) that only DEBUG builds opened: Client rebuilt a text for the
/// selected entity every frame - Client.PrintEntityInfo, Client.PrintEntityGoals - and put it in a
/// label on the form. Under XNA the game window was itself a WinForms form, so that dialog shared
/// the game's thread. Under SDL it cannot, and WinForms is Windows only.
///
/// HOW. The game serves a page on localhost only (HttpListener, in .NET on every OS), and any
/// browser is the window - put it on a second monitor. Two threads, one rule each:
///
///   GAME THREAD   Tick, from Client.Update, takes a snapshot every SnapshotMs: the category
///                 counts, the list for the category being watched, and the detail of the entity
///                 being watched, serialised to one JSON string. Only this thread touches the sim.
///   SERVER THREAD answers requests with the last string, and records which category and entity
///                 the page asked for. It never touches the sim, so it cannot change or stall it.
///
/// EXPANDABLE. A category is one line in Categories and one case in CategoryOf; a detail section
/// is one method returning a Section. The page renders the three kinds a section can be - rows,
/// a tree, text - without knowing what is in them.
///
/// OFF BY DEFAULT, and nothing listens while it is off.
/// </summary>
public static class DebugMonitorMod
{
    public const string ModId = "debugmonitor";

    /// <summary>How often the game thread refreshes the snapshot, in real milliseconds.</summary>
    public const int SnapshotMs = 250;

    /// <summary>A snapshot older than this is from a game that is no longer running.</summary>
    public const int StaleMs = 3000;

    /// <summary>Most entities a list carries; the page says how many more there were.</summary>
    public const int ListCap = 3000;

    /// <summary>Most nodes one component tree carries, so a huge object cannot stall a frame.</summary>
    public const int TreeBudget = 4000;

    private static ModSetting enabled;

    private static ModSetting port;

    private static ModSetting openBrowser;

    /// <summary>The categories, in the order the page lists them.</summary>
    public static readonly (string Key, string Label)[] Categories =
    {
        ("colonists", "Colonists"),
        ("people", "Other people"),
        ("creatures", "Creatures"),
        ("structures", "Buildings"),
        ("items", "Items"),
        ("plants", "Trees and plants"),
        ("other", "Everything else"),
    };

    public static ModSetting Enabled =>
        enabled ?? (enabled = ModSettings.Toggle(
            ModId, "enabled", "DEBUG MONITOR", defaultValue: false,
            toolTip: "Shows live debug details in a browser window outside the game: every colonist, "
                   + "creature, item and building, and for the one picked its goal tree, job scores "
                   + "and every component, four times a second. Serves on this computer only. Off by "
                   + "default, and nothing listens while it is off."));

    public static ModSetting Port =>
        port ?? (port = ModSettings.Choice(
            ModId, "port", "DEBUG MONITOR PORT", new string[4] { "8765", "8766", "8767", "8768" }, "8765",
            toolTip: "The monitor is at http://localhost:<port>/ . Change it only if another program "
                   + "already uses the port."));

    public static ModSetting OpenBrowser =>
        openBrowser ?? (openBrowser = ModSettings.Toggle(
            ModId, "openBrowser", "OPEN THE MONITOR IN A BROWSER", defaultValue: true,
            toolTip: "Opens the monitor page in the default browser when a game starts with the "
                   + "monitor on. Once per run of the game."));

    public static void RegisterSettings()
    {
        ModSettings.Describe(ModId, "Jerrybi",
            "Live debug details - colonists, creatures, items, buildings, goal trees, component fields - in a browser window outside the game. For testing.",
            "HUD_thumbnail_weatherAntenna");
        ModSettings.SetCategoryLabel(ModId, "DEBUG MONITOR");
        _ = Enabled;
        _ = Port;
        _ = OpenBrowser;
    }

    // ---------------------------------------------------------------- shared between threads

    /// <summary>What the page asked for last. Written by the server thread, read by the game thread.</summary>
    private static volatile string watchedCategory = "colonists";

    private static long watchedId = -1;

    /// <summary>The last snapshot. Written by the game thread, read by the server thread.</summary>
    private static volatile string stateJson = Idle("The game has not taken a snapshot yet.");

    // ---------------------------------------------------------------- game thread

    private static readonly Stopwatch clock = Stopwatch.StartNew();

    private static long lastSnapshotMs = long.MinValue;

    private static HttpListener listener;

    private static int listeningPort;

    private static bool failed;

    private static bool browserOpened;

    /// <summary>The page's address while the server runs, otherwise null.</summary>
    public static string Url => listener == null ? null : "http://localhost:" + listeningPort + "/";

    /// <summary>
    /// Starts or stops the server to match the switch, and refreshes the snapshot on its interval.
    /// Called every client update; returns on its first lines unless the switch is on.
    /// </summary>
    public static void Tick()
    {
        if (!Enabled.On)
        {
            StopServer();
            return;
        }
        if (failed)
        {
            return;
        }
        int wanted = int.TryParse(Port.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int p) ? p : 8765;
        if (listener == null || listeningPort != wanted)
        {
            StopServer();
            if (!StartServer(wanted))
            {
                // Once, then never again this session: a port somebody else holds is not worth a
                // message per frame, and it must not be the thing that stops a game.
                failed = true;
                GameStateManagement.UnclaimedWorld.LogError(
                    "The debug monitor could not listen on port " + wanted + " and is off for this session. "
                    + "Another program may be using it; pick another DEBUG MONITOR PORT.", "Debug monitor");
                return;
            }
            if (OpenBrowser.On && !browserOpened)
            {
                browserOpened = true;
                OpenInBrowser(Url);
            }
        }
        long now = clock.ElapsedMilliseconds;
        if (now - lastSnapshotMs < SnapshotMs)
        {
            return;
        }
        Interlocked.Exchange(ref lastSnapshotMs, now);
        stateJson = Snapshot();
    }

    /// <summary>
    /// Builds the JSON the page reads. Never throws: a failure is reported in the snapshot, where
    /// the person looking at the page can see it, and the next tick tries again.
    /// </summary>
    public static string Snapshot()
    {
        try
        {
            Sim sim = The.Sim;
            if (sim == null || sim.Mode != Sim.EngineMode.Game || sim.PlaySite == null)
            {
                return Idle("No game is running. Start or load one.");
            }
            return JsonSerializer.Serialize(BuildState(sim));
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new Dictionary<string, object>
            {
                ["game"] = false,
                ["message"] = "The snapshot failed: " + ex.GetType().Name + ": " + ex.Message,
            });
        }
    }

    private static string Idle(string message)
    {
        return JsonSerializer.Serialize(new Dictionary<string, object> { ["game"] = false, ["message"] = message });
    }

    private static Dictionary<string, object> BuildState(Sim sim)
    {
        var colonists = new HashSet<Entity>();
        var allegiance = sim.PlaySite.PlayerAllegiance;
        if (allegiance != null)
        {
            foreach (Entity person in allegiance.Persons)
            {
                if (person != null)
                {
                    colonists.Add(person);
                }
            }
        }

        string category = watchedCategory;
        var counts = new Dictionary<string, int>();
        foreach (var c in Categories)
        {
            counts[c.Key] = 0;
        }
        var members = new List<Entity>();
        LookUp<Entity, EntityID>.IterateMembers(entity =>
        {
            if (entity == null)
            {
                return;
            }
            string key = CategoryOf(entity, colonists);
            counts[key]++;
            if (key == category)
            {
                members.Add(entity);
            }
        });
        members.Sort((a, b) =>
        {
            int byName = string.Compare(Label(a), Label(b), StringComparison.OrdinalIgnoreCase);
            return byName != 0 ? byName : a.EntityID.CompareTo(b.EntityID);
        });

        var list = new List<object>();
        for (int i = 0; i < members.Count && i < ListCap; i++)
        {
            list.Add(new Dictionary<string, object> { ["id"] = (long)members[i].EntityID, ["name"] = Label(members[i]) });
        }

        var categories = new List<object>();
        foreach (var c in Categories)
        {
            categories.Add(new Dictionary<string, object> { ["key"] = c.Key, ["label"] = c.Label, ["count"] = counts[c.Key] });
        }

        EntityID? selected = The.InGameUI?.SelectedEntity;
        Entity selectedEntity = Entity.FindByID(selected);
        long id = Interlocked.Read(ref watchedId);
        Entity watched = id < 0 ? null : Entity.FindByID((EntityID)id);

        return new Dictionary<string, object>
        {
            ["game"] = true,
            ["day"] = sim.DateAndTime == null ? 0.0 : Math.Round(sim.DateAndTime.CurrentTimeDateYear.TotalDays, 3),
            ["categories"] = categories,
            ["category"] = category,
            ["list"] = list,
            ["more"] = Math.Max(0, members.Count - ListCap),
            ["selected"] = selectedEntity == null ? null : (long)selectedEntity.EntityID,
            ["selectedCategory"] = selectedEntity == null ? null : CategoryOf(selectedEntity, colonists),
            ["detail"] = id < 0 ? null
                : watched == null ? new Dictionary<string, object> { ["id"] = id, ["gone"] = true }
                : Detail(watched, CategoryOf(watched, colonists)),
        };
    }

    /// <summary>Which list an entity belongs to. The first match wins.</summary>
    public static string CategoryOf(Entity entity, HashSet<Entity> colonists)
    {
        if (colonists.Contains(entity))
        {
            return "colonists";
        }
        EntityType type = entity.EntityType;
        if (type == null)
        {
            return "other";
        }
        if (type.Person != null)
        {
            return "people";
        }
        if (entity.Intelligence != null)
        {
            return "creatures";
        }
        if (type.StructureType != null)
        {
            return "structures";
        }
        if (type.ItemType != null)
        {
            return "items";
        }
        if (type.TreeType != null)
        {
            return "plants";
        }
        return "other";
    }

    private static string Label(Entity entity)
    {
        string name = Safe(() => entity.Name);
        string key = entity.EntityType?.KeyName;
        if (string.IsNullOrEmpty(name))
        {
            return key ?? "?";
        }
        return key == null || name == key ? name : name + " (" + key + ")";
    }

    // ---------------------------------------------------------------- the detail of one entity

    private static Dictionary<string, object> Detail(Entity entity, string category)
    {
        var sections = new List<object> { Overview(entity, category) };
        Intelligence intelligence = entity.Intelligence;
        if (intelligence != null)
        {
            sections.Add(GoalTree(intelligence));
            sections.Add(JobScores(intelligence));
        }
        sections.Add(ScriptVariables(entity));
        sections.Add(Components(entity));
        return new Dictionary<string, object>
        {
            ["id"] = (long)entity.EntityID,
            ["name"] = Label(entity),
            ["category"] = category,
            ["sections"] = sections,
        };
    }

    private static Dictionary<string, object> Section(string title, string kind, string field, object content)
    {
        return new Dictionary<string, object> { ["title"] = title, ["kind"] = kind, [field] = content };
    }

    /// <summary>What Client.PrintEntityInfo put at the top of the studio's dialog, as rows.</summary>
    private static Dictionary<string, object> Overview(Entity entity, string category)
    {
        var rows = new List<object>();
        void Row(string name, Func<object> value)
        {
            rows.Add(new[] { name, Safe(() => Format(value())) });
        }
        Row("ID", () => entity.EntityID);
        Row("Name", () => entity.Name);
        Row("Type", () => entity.EntityType?.KeyName);
        Row("Category", () => category);
        Row("Assigned to job", () => entity.AssignedToJob);
        Row("Contained by", () => entity.ContainedBy);
        if (entity.IsOnPlaySite())
        {
            Row("Tile", () => entity.MapPosition);
            Row("World position", () => entity.PlaySiteLocation);
        }
        if (entity.Find<BodyComponent>(out var body) && body?.Body != null)
        {
            Row("Hitpoints", () => body.Body.GlobalHitpoints.ToString("0.0", CultureInfo.InvariantCulture)
                + " of " + body.Body.MaxHitpoints.ToString("0.0", CultureInfo.InvariantCulture));
        }
        if (entity.Find<NonLivingEntity>(out var nonLiving) && nonLiving != null)
        {
            Row("Condition", () => nonLiving.Condition + " of " + nonLiving.MaxCondition);
        }
        return Section("Overview", "rows", "rows", rows);
    }

    /// <summary>
    /// The goal tree, as a tree rather than Goal.ComposeIndentedString's indented text, so the page
    /// can fold it. The first subgoal of a composite is the one being pursued.
    /// </summary>
    private static Dictionary<string, object> GoalTree(Intelligence intelligence)
    {
        var nodes = new List<object>();
        if (intelligence.Brain != null)
        {
            nodes.Add(GoalNode(intelligence.Brain, 0));
        }
        return Section("Goal tree", "tree", "nodes", nodes);
    }

    private static Dictionary<string, object> GoalNode(Goal goal, int depth)
    {
        string text = Safe(() => goal.ToString());
        string status = Safe(() => goal.GetStatus());
        var node = new Dictionary<string, object> { ["n"] = text, ["v"] = status ?? "" };
        if (goal is CompositeGoal composite && composite.Subgoals != null && composite.Subgoals.Count > 0 && depth < 32)
        {
            var children = new List<object>();
            foreach (Goal sub in composite.Subgoals)
            {
                if (sub != null)
                {
                    children.Add(GoalNode(sub, depth + 1));
                }
            }
            node["c"] = children;
        }
        return node;
    }

    /// <summary>The rest of Client.PrintEntityGoals: the current goal's score and the top-scoring jobs.</summary>
    private static Dictionary<string, object> JobScores(Intelligence intelligence)
    {
        var rows = new List<object>
        {
            new[] { "Current goal score", Format(intelligence.GetCurrentGoalUtility()) },
        };
        if (intelligence.TopScoringJobs != null)
        {
            foreach (GoalAndScore job in intelligence.TopScoringJobs)
            {
                rows.Add(new[] { job.Score.ToString("0.0000", CultureInfo.InvariantCulture), job.Goal ?? "" });
            }
        }
        return Section("Job scores", "rows", "rows", rows);
    }

    private static Dictionary<string, object> ScriptVariables(Entity entity)
    {
        var text = new StringBuilder();
        string error = Safe(() =>
        {
            entity.PrintScriptVariables(text);
            return null;
        });
        return Section("Script variables", "text", "text", error ?? text.ToString().Trim());
    }

    /// <summary>Every field of every component, the brain and the body first.</summary>
    private static Dictionary<string, object> Components(Entity entity)
    {
        var nodes = new List<object>();
        if (entity.Components != null)
        {
            var components = new List<Component>(entity.Components.Values);
            components.Sort((a, b) => Rank(a).CompareTo(Rank(b)));
            int budget = TreeBudget;
            foreach (Component component in components)
            {
                if (component != null)
                {
                    nodes.Add(Inspect(component.GetType().Name, component, 4, ref budget,
                        new HashSet<object>(ReferenceEqualityComparer.Instance)));
                }
            }
        }
        return Section("Components", "tree", "nodes", nodes);
    }

    private static int Rank(Component component)
    {
        return component switch
        {
            Intelligence => 0,
            Person => 1,
            BodyComponent => 2,
            _ => 3,
        };
    }

    // ---------------------------------------------------------------- reading any object

    /// <summary>
    /// One node of a field tree: a name, a value, and the fields under it. FIELDS, never
    /// properties - a field is read without running anything, and a property getter can create,
    /// cache or count, which a monitor must not do to the game it watches.
    ///
    /// Another entity, a game-data record or an id is a leaf with its name: following them would
    /// put the whole world under every colonist. A collection shows its count and its first 100.
    /// </summary>
    public static Dictionary<string, object> Inspect(string name, object value, int depth, ref int budget, HashSet<object> seen)
    {
        var node = new Dictionary<string, object> { ["n"] = name };
        budget--;
        if (value == null)
        {
            node["v"] = "null";
            return node;
        }
        Type type = value.GetType();
        if (IsLeaf(value, type))
        {
            node["v"] = Format(value);
            return node;
        }
        if (!type.IsValueType && !seen.Add(value))
        {
            node["v"] = TypeName(type) + " (shown above)";
            return node;
        }
        if (depth <= 0 || budget <= 0)
        {
            node["v"] = budget <= 0 ? TypeName(type) + " (not shown: too much to show)" : TypeName(type);
            return node;
        }
        var children = new List<object>();
        if (value is IDictionary dictionary)
        {
            node["v"] = TypeName(type) + ", " + dictionary.Count + " entries";
            int i = 0;
            foreach (DictionaryEntry entry in dictionary)
            {
                if (i++ >= 100 || budget <= 0)
                {
                    break;
                }
                children.Add(Inspect(Format(entry.Key), entry.Value, depth - 1, ref budget, seen));
            }
        }
        else if (value is IEnumerable sequence)
        {
            int i = 0;
            foreach (object item in sequence)
            {
                if (i < 100 && budget > 0)
                {
                    children.Add(Inspect("[" + i + "]", item, depth - 1, ref budget, seen));
                }
                i++;
            }
            node["v"] = TypeName(type) + ", " + i + " items";
        }
        else
        {
            node["v"] = TypeName(type);
            for (Type t = type; t != null && t != typeof(object); t = t.BaseType)
            {
                foreach (FieldInfo field in t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    if (budget <= 0)
                    {
                        break;
                    }
                    object fieldValue;
                    try
                    {
                        fieldValue = field.GetValue(value);
                    }
                    catch (Exception ex)
                    {
                        fieldValue = "(could not read: " + ex.GetType().Name + ")";
                    }
                    children.Add(Inspect(FieldName(field), fieldValue, depth - 1, ref budget, seen));
                }
            }
        }
        if (children.Count > 0)
        {
            node["c"] = children;
        }
        return node;
    }

    private static bool IsLeaf(object value, Type type)
    {
        return type.IsPrimitive || type.IsEnum || value is string || value is decimal
            || value is DateTime || value is TimeSpan || value is Delegate || value is Type
            || value is Entity || value is IGameData
            // a library object - a Random, a StringBuilder, a texture - is one line, not its internals
            || (!(value is IEnumerable) && type.Namespace != null
                && (type.Namespace.StartsWith("System", StringComparison.Ordinal) || type.Namespace.StartsWith("Microsoft", StringComparison.Ordinal)));
    }

    /// <summary>A type as C# writes it: List&lt;Int32&gt;, not List`1.</summary>
    private static string TypeName(Type type)
    {
        if (!type.IsGenericType)
        {
            return type.Name;
        }
        string name = type.Name;
        int tick = name.IndexOf('`');
        var args = new List<string>();
        foreach (Type arg in type.GetGenericArguments())
        {
            args.Add(TypeName(arg));
        }
        return (tick < 0 ? name : name.Substring(0, tick)) + "<" + string.Join(", ", args) + ">";
    }

    /// <summary>An auto-property's backing field reads as the property's name.</summary>
    private static string FieldName(FieldInfo field)
    {
        string name = field.Name;
        if (name.StartsWith("<", StringComparison.Ordinal))
        {
            int end = name.IndexOf('>');
            if (end > 1)
            {
                return name.Substring(1, end - 1);
            }
        }
        return name;
    }

    /// <summary>A value as one line; an entity or an entity id with the entity's name.</summary>
    private static string Format(object value)
    {
        switch (value)
        {
            case null:
                return "null";
            case EntityID id:
                return EntityText(id);
            case Entity entity:
                return EntityText(entity.EntityID);
            case IGameData data:
                return data.GetType().Name + " " + data.KeyName;
            case Delegate d:
                return "delegate " + d.Method.Name;
            case float f:
                return f.ToString("0.####", CultureInfo.InvariantCulture);
            case double d:
                return d.ToString("0.####", CultureInfo.InvariantCulture);
            case IFormattable formattable:
                return formattable.ToString(null, CultureInfo.InvariantCulture);
            default:
                return value.ToString();
        }
    }

    private static string EntityText(EntityID id)
    {
        if (id == EntityID.Invalid)
        {
            return "none";
        }
        Entity entity = Entity.FindByID(id);
        return entity == null ? (long)id + " (gone)" : (long)id + " " + Label(entity);
    }

    private static string Safe(Func<object> read)
    {
        try
        {
            object value = read();
            return value == null ? null : Format(value);
        }
        catch (Exception ex)
        {
            return "(" + ex.GetType().Name + ")";
        }
    }

    // ---------------------------------------------------------------- server thread

    /// <summary>Starts listening on localhost. False when the port cannot be had.</summary>
    public static bool StartServer(int portNumber)
    {
        var l = new HttpListener();
        l.Prefixes.Add("http://localhost:" + portNumber + "/");
        try
        {
            l.Start();
        }
        catch (Exception)
        {
            try
            {
                l.Close();
            }
            catch (Exception)
            {
            }
            return false;
        }
        listener = l;
        listeningPort = portNumber;
        new Thread(() => Serve(l)) { IsBackground = true, Name = "Debug monitor" }.Start();
        return true;
    }

    public static void StopServer()
    {
        HttpListener l = listener;
        listener = null;
        if (l == null)
        {
            return;
        }
        try
        {
            l.Close();
        }
        catch (Exception)
        {
        }
    }

    private static void Serve(HttpListener l)
    {
        while (l.IsListening)
        {
            HttpListenerContext context;
            try
            {
                context = l.GetContext();
            }
            catch (Exception)
            {
                return; // StopServer closed it
            }
            try
            {
                byte[] body = Encoding.UTF8.GetBytes(Respond(context.Request.Url.AbsolutePath, context.Request.QueryString, out string contentType, out int status));
                context.Response.StatusCode = status;
                context.Response.ContentType = contentType;
                context.Response.Headers["Cache-Control"] = "no-store";
                context.Response.ContentLength64 = body.Length;
                context.Response.OutputStream.Write(body, 0, body.Length);
            }
            catch (Exception)
            {
                // the page went away mid-answer; it asks again in a quarter of a second
            }
            finally
            {
                try
                {
                    context.Response.Close();
                }
                catch (Exception)
                {
                }
            }
        }
    }

    /// <summary>
    /// One answer: the page, or the last snapshot. Asking for the snapshot also says what to watch
    /// next - ?cat=colonists&amp;id=1234 - which the game thread reads on its next tick.
    /// </summary>
    public static string Respond(string path, NameValueCollection query, out string contentType, out int status)
    {
        status = 200;
        if (path == "/" || path == "/index.html")
        {
            contentType = "text/html; charset=utf-8";
            return DebugMonitorPage.Html;
        }
        if (path == "/api/state")
        {
            string cat = query?["cat"];
            foreach (var c in Categories)
            {
                if (c.Key == cat)
                {
                    watchedCategory = cat;
                }
            }
            string id = query?["id"];
            Interlocked.Exchange(ref watchedId,
                long.TryParse(id, NumberStyles.Integer, CultureInfo.InvariantCulture, out long n) && n >= 0 ? n : -1);
            contentType = "application/json; charset=utf-8";
            // Tick runs from the in-game client's update only. Back at the main menu nothing
            // refreshes the snapshot, and the last game must not be shown as if it were live.
            long last = Interlocked.Read(ref lastSnapshotMs);
            if (last == long.MinValue || clock.ElapsedMilliseconds - last > StaleMs)
            {
                return Idle("No game is running. Start or load one.");
            }
            return stateJson;
        }
        status = 404;
        contentType = "text/plain; charset=utf-8";
        return "Not here. The monitor is at /";
    }

    private static void OpenInBrowser(string url)
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else
            {
                Process.Start(OperatingSystem.IsMacOS() ? "open" : "xdg-open", url);
            }
        }
        catch (Exception)
        {
            // no browser to open: the address is in the setting's tooltip
        }
    }
}
