using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using UWGame;
using UWGame.SimSide;

namespace UW.Tools.DataExport;

/// <summary>
/// --strings-literals=FILE --strings-out=FILE: writes English (US).xml, the template translators
/// copy (translations/README.md). Run by tools/build/37-make-strings.sh, which collects FILE - the
/// literals passed to Locale.Text in the code, one per line as written in C#.
///
/// Every entry the game can ask Locale for, and nothing else: the interface text from FILE, keyed
/// "(GUI)" and the English as in the studio's own file; every registered mod setting's label and
/// tooltip ("(SETTING)", "(SETTING TIP)" and the id) and each category's heading ("(SETTING
/// GROUP)" and the mod id); every item's name and description ("(ITEM)", "(ITEM DESCRIPTION)" and
/// its KeyName). Sorted by key, so a change to the code is a small change to the file.
/// </summary>
internal static partial class Program
{
    private static int WriteStrings(string literalsPath, string countsPath, string outPath)
    {
        int rc = Run(Sim.SerializeMode.NoSerialize, "base tables, the way the game loads them");
        if (rc != 0) return rc;

        var entries = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (string line in File.ReadAllLines(literalsPath))
        {
            if (line.Length == 0) continue;
            string english = UnescapeCSharp(line);
            entries["(GUI)" + english] = english;
        }
        // Locale.Count's two forms, "one" TAB "other": the value is "one|other", the forms in order.
        if (countsPath != null)
        {
            foreach (string line in File.ReadAllLines(countsPath))
            {
                string[] pair = line.Split('\t');
                if (pair.Length != 2) continue;
                entries[Locale.CountKey(UnescapeCSharp(pair[1]))] = UnescapeCSharp(pair[0]) + "|" + UnescapeCSharp(pair[1]);
            }
        }
        var modIds = new SortedSet<string>(StringComparer.Ordinal);
        foreach (UWGame.Mods.ModSetting setting in UWGame.Mods.ModSettings.All)
        {
            entries[Locale.SettingKey(setting.Id)] = setting.Label;
            if (!string.IsNullOrEmpty(setting.ToolTip))
            {
                entries[Locale.SettingTipKey(setting.Id)] = setting.ToolTip;
            }
            foreach (string choice in setting.Choices ?? new string[0])
            {
                if (setting.ChoiceIsText(choice))
                {
                    entries[Locale.SettingChoiceKey(setting.Id, choice)] = choice;
                }
            }
            modIds.Add(setting.ModId);
        }
        foreach (string modId in modIds)
        {
            entries[Locale.SettingGroupKey(modId)] = UWGame.Mods.ModSettings.CategoryLabel(modId);
        }
        // The same list the game translates from (Locale.DataTexts) - the base game's tables, then
        // each built-in scenario's.
        foreach (Locale.DataText text in Locale.DataTexts(GameData.Instance))
        {
            entries[text.Key] = text.English;
        }
        var texts = ScenarioEntries(text => entries[text.Key] = text.English);
        if (texts == null)
        {
            return 1;
        }
        foreach (var pair in texts)
        {
            entries[pair.Key] = pair.Value;
        }

        var list = entries.Select(e => new UWGame.String { Key = e.Key, Value = e.Value }).ToList();
        var ns = new XmlSerializerNamespaces();
        ns.Add("", "");
        var settings = new XmlWriterSettings { Indent = true, IndentChars = "  ", NewLineChars = "\r\n", Encoding = new UTF8Encoding(false) };
        using (XmlWriter writer = XmlWriter.Create(outPath, settings))
        {
            new XmlSerializer(typeof(List<UWGame.String>)).Serialize(writer, list, ns);
        }
        // The file key of each entry, in the same order (Locale.ShortKey). 37-strings.pl group
        // writes the template with these, after checking its own copy of the rule gives the same.
        // Two entries may never share one: a translation could not tell them apart.
        var shared = list.GroupBy(e => Locale.ShortKey(e.Key)).Where(g => g.Count() > 1).ToList();
        if (shared.Count > 0)
        {
            Console.Error.WriteLine("FATAL: these entries would share a key in the language files (Locale.ShortKey):");
            foreach (var g in shared)
            {
                Console.Error.WriteLine("  " + g.Key + ": " + string.Join(" | ", g.Select(e => e.Key)));
            }
            return 1;
        }
        File.WriteAllLines(outPath + ".keys", list.Select(e => Locale.ShortKey(e.Key)), new UTF8Encoding(false));
        Console.WriteLine($"==> strings: {list.Count} entries ({list.Count(e => e.Key.StartsWith("(GUI)", StringComparison.Ordinal))} interface, "
                          + $"{list.Count(e => e.Key.StartsWith("(SETTING", StringComparison.Ordinal))} settings, "
                          + $"{list.Count - list.Count(e => e.Key.StartsWith("(GUI)", StringComparison.Ordinal) || e.Key.StartsWith("(SETTING", StringComparison.Ordinal))} data) -> {outPath}");
        return 0;
    }

    /// <summary>
    /// The built-in scenarios' own text, as template entries - with the base game's tables loaded
    /// when called. A scenario's text goes under the shared key when the base game has none and
    /// every scenario that has it agrees; under "key@scenario" where it differs
    /// (Locale.TranslatedData). <paramref name="headerText"/> gets what the New Game screens show
    /// of each (Locale.TranslateScenario). Null, and the reason written, when a scenario does not
    /// load completely.
    /// </summary>
    private static SortedDictionary<string, string> ScenarioEntries(Action<Locale.DataText> headerText)
    {
        var baseTexts = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (Locale.DataText text in Locale.DataTexts(GameData.Instance))
        {
            baseTexts[text.Key] = text.English;
        }
        var scenarioTexts = new SortedDictionary<string, SortedDictionary<string, string>>(StringComparer.Ordinal);
        foreach (var loader in BuiltInScenarios())
        {
            var header = loader.GetScenarioHeader();
            header.ScenarioData = loader.GetScenarioData();
            foreach (Locale.DataText text in Locale.ScenarioTexts(header))
            {
                headerText?.Invoke(text);
            }
            if (!LoadScenarioTables(loader))
            {
                Console.Error.WriteLine($"FATAL: scenario {header.Name} did not load completely, so its text cannot be listed.");
                return null;
            }
            foreach (Locale.DataText text in Locale.DataTexts(GameData.Instance))
            {
                if (baseTexts.TryGetValue(text.Key, out string same) && same == text.English)
                {
                    continue;
                }
                if (!scenarioTexts.TryGetValue(text.Key, out var byScenario))
                {
                    scenarioTexts[text.Key] = byScenario = new SortedDictionary<string, string>(StringComparer.Ordinal);
                }
                byScenario[header.Name] = text.English;
            }
        }
        var entries = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in scenarioTexts)
        {
            if (!baseTexts.ContainsKey(pair.Key) && pair.Value.Values.Distinct(StringComparer.Ordinal).Count() == 1)
            {
                entries[pair.Key] = pair.Value.Values.First();
                continue;
            }
            foreach (var byScenario in pair.Value)
            {
                entries[pair.Key + "@" + byScenario.Key] = byScenario.Value;
            }
        }
        return entries;
    }

    /// <summary>
    /// --locale-selftest: a language file the way a translator writes one, in the target's
    /// data/BaseData/Strings, chosen in PORT -> LANGUAGE. Interface text, a mod setting's label and
    /// an item's name come back translated; what the file leaves out, and everything with English
    /// chosen, comes back as the code's English; a broken file is English, not a crash.
    /// </summary>
    private static int LocaleSelfTest()
    {
        Console.WriteLine("==> locale self-test");
        int failures = 0;
        void Check(bool ok, string what)
        {
            Console.WriteLine((ok ? "  ok    " : "  FAIL  ") + what);
            if (!ok) failures++;
        }
        // The keys a language file uses (Locale.ShortKey; Kastuk and tripleacoder, 2026-10-09).
        string Key(string k) => Locale.ShortKey(k);
        Check(Key("(ITEM DESCRIPTION)item:acetylene") == "ItemDescriptionItemAcetylene" && Key("(GUI)SAVE GAME") == "GuiSAVEGAME"
              && Key("(SETTING)hud.labelList") == "SettingHudLabelList",
              $"a key is the area's words and the rest's, run together ({Key("(ITEM DESCRIPTION)item:acetylene")}, {Key("(GUI)SAVE GAME")})");
        Check(Key("(GUI)Close") == "GuiClose" && Key("(GUI)CLOSE") == "GuiCLOSE", "the text's case is kept, so CLOSE and Close are two keys");
        Check(System.Text.RegularExpressions.Regex.IsMatch(Key("(GUI)HEALTH:"), "^GuiHEALTH_[0-9a-f]{12}$") && Key("(GUI)HEALTH:") != Key("(GUI)HEALTH")
              && Key("(GUI)Subscore: +") != Key("(GUI)Subscore: -"),
              $"text with punctuation gets a hash, so HEALTH: and HEALTH stay two ({Key("(GUI)HEALTH:")})");
        string longKey = Key("(GUI)Content changes apply at the next game start and not before that!");
        Check(longKey.Length == Locale.ShortKeyLength + 13 && longKey.StartsWith("GuiContentChangesApplyAtTheNextGameStartAndNotBefo_", StringComparison.Ordinal),
              $"a long one is cut at {Locale.ShortKeyLength} and given a hash ({longKey})");
        Check(Key(Key("(GUI)HEALTH:")) != null && Locale.IsShortKey(Key("(GUI)HEALTH:")) && !Locale.IsShortKey("(GUI)HEALTH"),
              "a short key is letters, digits and _ only - a file's old \"(GUI)...\" keys are told apart and converted");

        string folder = Config.GetDataFolderPath(Config.DataType.BaseData, "Strings");
        Directory.CreateDirectory(folder);
        File.WriteAllText(Path.Combine(folder, "Test.xml"),
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<ArrayOfString>\r\n" +
            "  <String><Key>(GUI)SAVE GAME</Key><Value>СОХРАНИТЬ ИГРУ</Value></String>\r\n" +
            // The same entry again in the short form: one key, the last wins. The rest of the file
            // is in the old form, which must keep working for files written before.
            $"  <String><Key>{Key("(GUI)SAVE GAME")}</Key><Value>СОХРАНИТЬ</Value></String>\r\n" +
            "  <String><Key>(GUI)Illegal width entered. {0} is maximum.</Key><Value>Ширина не больше {0}.</Value></String>\r\n" +
            "  <String><Key>(SETTING)hud.labelList</Key><Value>СПИСОК МЕТОК</Value></String>\r\n" +
            "  <String><Key>(ITEM)item:acetylene</Key><Value>Ацетилен</Value></String>\r\n" +
            "  <String><Key>(PROCESS)activateSnare</Key><Value>Перезарядка ловушки</Value></String>\r\n" +
            "  <String><Key>(SOIL)soil:clay</Key><Value>Глина</Value></String>\r\n" +
            "  <String><Key>(COUNT){0} CHANGED</Key><Value>{0} ИЗМЕНЕНИЕ|{0} ИЗМЕНЕНИЯ|{0} ИЗМЕНЕНИЙ</Value></String>\r\n" +
            "  <String><Key>(SETTING CHOICE)toolcare.lookAfter=WEAPONS AND GOOD TOOLS</Key><Value>ОРУЖИЕ И ХОРОШИЕ ИНСТРУМЕНТЫ</Value></String>\r\n" +
            "  <String><Key>(GUI)Illegal height entered. {0} is maximum.</Key><Value>Высота не больше {1}.</Value></String>\r\n" +
            "</ArrayOfString>\r\n", new UTF8Encoding(false));
        File.WriteAllText(Path.Combine(folder, "Broken.xml"), "<ArrayOfString><String><Key>", new UTF8Encoding(false));

        // As the options menu does on opening: the files were written after the settings registered.
        UWGame.Mods.PortSettings.RefreshLanguageChoices();
        UWGame.Mods.ModSetting language = UWGame.Mods.PortSettings.Language;
        Check(language.Choices.Contains("Test") && language.Choices[0] == Locale.InvariantCulture,
              "LANGUAGE lists English (US) first and every file in Strings (" + string.Join(", ", language.Choices) + ")");
        Check(Locale.Text("SAVE GAME") == "SAVE GAME", "English chosen: the code's English");

        language.Value = "Test";
        Check(Locale.Text("SAVE GAME") == "СОХРАНИТЬ", $"Test chosen: translated, the last of a key written twice ({Locale.Text("SAVE GAME")})");
        Check(Locale.Text("NEW GAME") == "NEW GAME", "a key the file leaves out stays English");
        Check(string.Format(Locale.Text("Illegal width entered. {0} is maximum."), 1920) == "Ширина не больше 1920.", "a format string keeps its {0}");
        Check(UWGame.Mods.HudMod.LabelListSetting.DisplayLabel == "СПИСОК МЕТОК"
              && UWGame.Mods.HudMod.LabelListSetting.Label == "OVERLAPPING LABELS OPEN INTO A LIST",
              "a mod setting's label is shown translated, and kept in English for the settings file");
        string counts = string.Join(", ", new[] { 1, 3, 5, 11, 21, 22 }.Select(n => Locale.Count(n, "{0} CHANGED", "{0} CHANGED")));
        Check(counts == "1 ИЗМЕНЕНИЕ, 3 ИЗМЕНЕНИЯ, 5 ИЗМЕНЕНИЙ, 11 ИЗМЕНЕНИЙ, 21 ИЗМЕНЕНИЕ, 22 ИЗМЕНЕНИЯ",
              $"a count takes the language's form for its number - three forms, East Slavic ({counts})");
        var lookAfter = UWGame.Mods.ModSettings.Find("toolcare.lookAfter");
        Check(lookAfter == null || lookAfter.DisplayChoice("WEAPONS AND GOOD TOOLS") == "ОРУЖИЕ И ХОРОШИЕ ИНСТРУМЕНТЫ"
              && lookAfter.Choices.Contains("WEAPONS AND GOOD TOOLS"),
              "a dropdown shows its choice translated, and keeps the English value to store");
        Check(UWGame.Mods.ModSettings.Find("hud.labelList") != null && UWGame.Mods.PortSettings.Language.DisplayChoice("Test") == "Test",
              "a language's name is shown as it is, never looked up");
        Check(string.Format(Locale.Text("Illegal height entered. {0} is maximum."), 1080) == "Illegal height entered. 1080 is maximum.",
              "a translation with a {1} the code does not pass is not used - English, instead of a crash in string.Format");

        // Scenarios. A text two scenarios word differently is two keys, "key@scenario" each. One
        // scenario's translation lands in that scenario only; in the other the English stays - even
        // with the plain key translated too, because that is not the English the template holds.
        // The key is taken from the entries the template is written from, so the test follows the data.
        if (Run(Sim.SerializeMode.NoSerialize, "base tables, the way the game loads them") != 0) return 1;
        var variants = (ScenarioEntries(null) ?? new SortedDictionary<string, string>()).Keys
            .Where(k => k.Contains("@"))
            .GroupBy(k => k.Substring(0, k.LastIndexOf('@')), k => k.Substring(k.LastIndexOf('@') + 1))
            .FirstOrDefault(g => g.Count() >= 2);
        Check(variants != null, "the template has a text two scenarios word differently" + (variants == null ? "" : $" ({variants.Key})"));
        if (variants == null) return 1;
        string shared = variants.Key, first = variants.ElementAt(0), second = variants.ElementAt(1);
        bool LoadScenario(string name)
        {
            var loader = BuiltInScenarios().FirstOrDefault(l => l.GetScenarioHeader().Name == name);
            return loader != null && LoadScenarioTables(loader);
        }
        string Now(string key) => Locale.DataTexts(GameData.Instance).Where(t => t.Key == key).Select(t => t.English).FirstOrDefault();

        language.Value = Locale.InvariantCulture;
        if (!LoadScenario(first)) { Check(false, "scenario " + first + " loads completely"); return 1; }
        string talk = Locale.DataTexts(GameData.Instance).Select(t => t.Key).FirstOrDefault(k => k.StartsWith("(TALKACTION", StringComparison.Ordinal));
        File.WriteAllText(Path.Combine(folder, "Scenes.xml"),
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<ArrayOfString>\r\n" +
            $"  <String><Key>{System.Security.SecurityElement.Escape(shared)}</Key><Value>ОБЩЕЕ</Value></String>\r\n" +
            $"  <String><Key>{System.Security.SecurityElement.Escape(shared + "@" + first)}</Key><Value>СВОЁ</Value></String>\r\n" +
            $"  <String><Key>{System.Security.SecurityElement.Escape(talk)}</Key><Value>РЕПЛИКА</Value></String>\r\n" +
            "  <String><Key>(HELP)introduction</Key><Value>0: Введение</Value></String>\r\n" +
            $"  <String><Key>{System.Security.SecurityElement.Escape("(SCENARIO DISPLAYNAME)" + first)}</Key><Value>СЦЕНАРИЙ</Value></String>\r\n" +
            "</ArrayOfString>\r\n", new UTF8Encoding(false));
        UWGame.Mods.PortSettings.RefreshLanguageChoices();
        language.Value = "Scenes";
        Locale.TranslateData(GameData.Instance);
        Check(Now(shared) == "СВОЁ", $"in {first}, its own translation of its own wording of {shared}");
        Check(talk != null && Now(talk) == "РЕПЛИКА", $"a colonist's spoken line, inline in an action set, translates ({talk})");
        Check(GameData.Instance.AllHelpTopics.TryGetValue("introduction", out var intro) && intro.Name == "0: Введение", "a help topic's title translates");
        var header = BuiltInScenarios().First(l => l.GetScenarioHeader().Name == first).GetScenarioHeader();
        Locale.TranslateScenario(header);
        Check(header.DisplayName == "СЦЕНАРИЙ" && header.Name == first, "a scenario's shown name translates on the New Game screen, and its Name - its identity - does not");

        language.Value = Locale.InvariantCulture;
        if (!LoadScenario(second)) { Check(false, "scenario " + second + " loads completely"); return 1; }
        string secondEnglish = Now(shared);
        language.Value = "Scenes";
        Locale.TranslateData(GameData.Instance);
        Check(secondEnglish != null && Now(shared) == secondEnglish,
              $"in {second}, which words it differently and has no translation, the English stays - not another scenario's, not the plain key's");

        GameData.UnloadAllData();
        language.Value = "Test";

        int rc = Run(Sim.SerializeMode.NoSerialize, "base tables, the way the game loads them");
        if (rc != 0) return rc;
        if (!ValidateDataComplete()) return 1;
        Check(GameData.Instance.AllEntityTypes["item:acetylene"].Name == "Ацетилен", "an item's name is translated when the tables are completed");
        Check(GameData.Instance.AllProcessTypes["activateSnare"].Name == "Перезарядка ловушки", "...and a recipe's, from another table (Locale.DataTexts)");
        Check(!GameData.Instance.AllSoilComponentTypes.TryGetValue("soil:clay", out var clay) || clay.Name == "Глина",
              "...and a soil's, whose Name is a field hiding the base property");
        Check(Locale.DataTexts(GameData.Instance).All(t => !t.Key.StartsWith("(TERRAIN", StringComparison.Ordinal)),
              "terrain features, named for their assets, are not in the template");
        Check(GameData.Instance.AllEntityTypes["item:advancedCookingPot"].Name != null
              && !GameData.Instance.AllEntityTypes["item:advancedCookingPot"].Name.StartsWith("(ITEM)", StringComparison.Ordinal),
              "an item the file leaves out keeps its English name");

        // The simulation cannot tell which language is chosen. Every number, flag and enum in all
        // the tables is the same after every data text is translated as before - and nothing in the
        // simulation compares a name with English (37-make-strings.sh refuses that).
        GameData.UnloadAllData();
        rc = Run(Sim.SerializeMode.NoSerialize, "base tables again, for the fingerprint");
        if (rc != 0) return rc;
        string before = Fingerprint(GameData.Instance);
        List<Locale.DataText> texts = Locale.DataTexts(GameData.Instance);
        var everything = new StringBuilder("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<ArrayOfString>\r\n");
        foreach (Locale.DataText text in texts)
        {
            everything.Append("<String><Key>").Append(System.Security.SecurityElement.Escape(text.Key)).Append("</Key><Value>")
                      .Append(System.Security.SecurityElement.Escape("Ψ" + text.English)).Append("</Value></String>\r\n");
        }
        everything.Append("</ArrayOfString>\r\n");
        File.WriteAllText(Path.Combine(folder, "Everything.xml"), everything.ToString(), new UTF8Encoding(false));
        UWGame.Mods.PortSettings.RefreshLanguageChoices();
        language.Value = "Everything";
        Locale.TranslateData(GameData.Instance);
        int translated = Locale.DataTexts(GameData.Instance).Count(t => t.English.StartsWith("Ψ", StringComparison.Ordinal));
        Check(translated == texts.Count, $"every data text translates: {translated} of {texts.Count}");
        string after = Fingerprint(GameData.Instance);
        Check(before == after, $"and nothing else in the tables changes - {before.Length} characters of numbers, flags and enums, identical"
              + (before == after ? "" : $" (first difference at {Enumerable.Range(0, Math.Min(before.Length, after.Length)).FirstOrDefault(i => before[i] != after[i])})"));

        language.Value = "Broken";
        Check(Locale.Text("SAVE GAME") == "SAVE GAME", "a file that cannot be read: English, no crash");
        language.Value = Locale.InvariantCulture;
        Check(Locale.Text("SAVE GAME") == "SAVE GAME", "back to English");

        Console.WriteLine(failures == 0 ? "locale self-test OK" : $"locale self-test FAILED - {failures} check(s)");
        return failures == 0 ? 0 : 1;
    }

    /// <summary>
    /// Every public number, flag and enum in the tables, as text - what the simulation could read:
    /// every entry, and everything inside it, walked as Locale.DataTexts walks it (a spoken line's
    /// Duration, a recipe's thresholds). Strings are left out: they are what translation changes,
    /// and the simulation does not compare them (37-make-strings.sh).
    /// </summary>
    private static string Fingerprint(GameData data)
    {
        var sb = new StringBuilder();
        var seen = new HashSet<object>(ReferenceEqualityComparer.Instance);
        var members = new Dictionary<Type, MemberInfo[]>();
        foreach (var collection in data.AllGameDataCollections.OrderBy(c => c.Key.FullName, StringComparer.Ordinal))
        {
            if (!(collection.Value is System.Collections.IDictionary entries)) continue;
            foreach (string key in entries.Keys.Cast<string>().OrderBy(k => k, StringComparer.Ordinal))
            {
                sb.Append(collection.Key.Name).Append('/').Append(key).Append(':');
                Walk(entries[key], 0);
                sb.Append('\n');
            }
        }
        return sb.ToString();

        void Walk(object o, int depth)
        {
            if (o == null || depth > 12) return;
            Type t = o.GetType();
            if (IsPlain(t))
            {
                sb.Append(Convert.ToString(o, System.Globalization.CultureInfo.InvariantCulture)).Append(';');
                return;
            }
            if (o is string || !seen.Add(o)) return;
            if (o is System.Collections.IDictionary d)
            {
                foreach (object k in d.Keys.Cast<object>().OrderBy(k => Convert.ToString(k, System.Globalization.CultureInfo.InvariantCulture), StringComparer.Ordinal))
                {
                    Walk(d[k], depth + 1);
                }
                return;
            }
            if (o is System.Collections.IEnumerable list)
            {
                foreach (object item in list) Walk(item, depth + 1);
                return;
            }
            if (t.Namespace == null || !t.Namespace.StartsWith("UWGame", StringComparison.Ordinal)) return;
            if (!members.TryGetValue(t, out MemberInfo[] ms))
            {
                members[t] = ms = t.GetFields(BindingFlags.Public | BindingFlags.Instance).Cast<MemberInfo>()
                    .Concat(t.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead && p.GetIndexParameters().Length == 0))
                    .OrderBy(m => m.Name, StringComparer.Ordinal).ToArray();
            }
            foreach (MemberInfo m in ms)
            {
                object v;
                try { v = m is FieldInfo f ? f.GetValue(o) : ((PropertyInfo)m).GetValue(o); } catch (Exception) { continue; }
                sb.Append(m.Name).Append('=');
                Walk(v, depth + 1);
            }
        }

        static bool IsPlain(Type type)
        {
            Type u = Nullable.GetUnderlyingType(type) ?? type;
            return u.IsPrimitive || u.IsEnum || u == typeof(decimal);
        }
    }

    /// <summary>The built-in scenarios, in the game's order (RGScenarioLoader).</summary>
    private static List<UWGame.SimSide.AllGameData.Scenarios.ScenarioLoader> BuiltInScenarios()
    {
        FieldInfo field = typeof(UWGame.SimSide.AllGameData.Scenarios.RGScenarioLoader)
            .GetField("rgScenarioLoaders", BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("RGScenarioLoader.rgScenarioLoaders not found; the field was renamed.");
        return ((System.Collections.IEnumerable)field.GetValue(null)).Cast<UWGame.SimSide.AllGameData.Scenarios.ScenarioLoader>().ToList();
    }

    /// <summary>
    /// A scenario's tables, loaded as the game loads them (Sim.QueueGameDataAndSimInit): the old
    /// tables gone, then the base loader and then the scenario's, both told the scenario - a
    /// scenario's loaders read the base tables. False if any table did not build.
    /// </summary>
    private static bool LoadScenarioTables(UWGame.SimSide.AllGameData.Scenarios.ScenarioLoader loader)
    {
        GameData.UnloadAllData();
        var header = loader.GetScenarioHeader();
        header.ScenarioData = loader.GetScenarioData();
        if (RunLoader(new UWGame.SimSide.AllGameData.BaseDataLoader(), header, Sim.SerializeMode.NoSerialize, "base tables for " + header.Name) != 0 || LastRunFailures > 0)
        {
            return false;
        }
        return RunLoader(loader.GetDataLoader(), header, Sim.SerializeMode.NoSerialize, "tables of " + header.Name) == 0 && LastRunFailures == 0;
    }

    /// <summary>A C# regular string literal's contents, as the compiler reads them.</summary>
    internal static string UnescapeCSharp(string literal)
    {
        var sb = new StringBuilder(literal.Length);
        for (int i = 0; i < literal.Length; i++)
        {
            char c = literal[i];
            if (c != '\\' || i + 1 >= literal.Length)
            {
                sb.Append(c);
                continue;
            }
            char n = literal[++i];
            switch (n)
            {
                case 'n': sb.Append('\n'); break;
                case 'r': sb.Append('\r'); break;
                case 't': sb.Append('\t'); break;
                case '0': sb.Append('\0'); break;
                case 'u' when i + 4 < literal.Length:
                    sb.Append((char)Convert.ToInt32(literal.Substring(i + 1, 4), 16));
                    i += 4;
                    break;
                default: sb.Append(n); break;
            }
        }
        return sb.ToString();
    }
}
