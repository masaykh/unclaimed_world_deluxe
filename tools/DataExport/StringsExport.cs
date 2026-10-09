using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        // The same list the game translates from (Locale.DataTexts).
        foreach (Locale.DataText text in Locale.DataTexts(GameData.Instance))
        {
            entries[text.Key] = text.English;
        }

        var list = entries.Select(e => new UWGame.String { Key = e.Key, Value = e.Value }).ToList();
        var ns = new XmlSerializerNamespaces();
        ns.Add("", "");
        var settings = new XmlWriterSettings { Indent = true, IndentChars = "  ", NewLineChars = "\r\n", Encoding = new UTF8Encoding(false) };
        using (XmlWriter writer = XmlWriter.Create(outPath, settings))
        {
            new XmlSerializer(typeof(List<UWGame.String>)).Serialize(writer, list, ns);
        }
        Console.WriteLine($"==> strings: {list.Count} entries ({list.Count(e => e.Key.StartsWith("(GUI)", StringComparison.Ordinal))} interface, "
                          + $"{list.Count(e => e.Key.StartsWith("(SETTING", StringComparison.Ordinal))} settings, "
                          + $"{list.Count - list.Count(e => e.Key.StartsWith("(GUI)", StringComparison.Ordinal) || e.Key.StartsWith("(SETTING", StringComparison.Ordinal))} data) -> {outPath}");
        return 0;
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
        string folder = Config.GetDataFolderPath(Config.DataType.BaseData, "Strings");
        Directory.CreateDirectory(folder);
        File.WriteAllText(Path.Combine(folder, "Test.xml"),
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<ArrayOfString>\r\n" +
            "  <String><Key>(GUI)SAVE GAME</Key><Value>СОХРАНИТЬ ИГРУ</Value></String>\r\n" +
            "  <String><Key>(GUI)SAVE GAME</Key><Value>СОХРАНИТЬ</Value></String>\r\n" +
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
    /// Every public number, flag and enum of every entry in every table, as text - what the
    /// simulation could read. Strings are left out: they are what translation changes, and the
    /// simulation does not compare them (37-make-strings.sh).
    /// </summary>
    private static string Fingerprint(GameData data)
    {
        var sb = new StringBuilder();
        foreach (var collection in data.AllGameDataCollections.OrderBy(c => c.Key.FullName, StringComparer.Ordinal))
        {
            if (!(collection.Value is System.Collections.IDictionary entries)) continue;
            foreach (string key in entries.Keys.Cast<string>().OrderBy(k => k, StringComparer.Ordinal))
            {
                object entry = entries[key];
                Type t = entry.GetType();
                sb.Append(collection.Key.Name).Append('/').Append(key).Append(':');
                foreach (var f in t.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).OrderBy(f => f.Name, StringComparer.Ordinal))
                {
                    if (IsPlain(f.FieldType)) sb.Append(f.Name).Append('=').Append(Convert.ToString(f.GetValue(entry), System.Globalization.CultureInfo.InvariantCulture)).Append(';');
                }
                foreach (var p in t.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).OrderBy(p => p.Name, StringComparer.Ordinal))
                {
                    if (!IsPlain(p.PropertyType) || p.GetIndexParameters().Length > 0 || !p.CanRead) continue;
                    object v;
                    try { v = p.GetValue(entry); } catch (Exception) { v = "<throws>"; }
                    sb.Append(p.Name).Append('=').Append(Convert.ToString(v, System.Globalization.CultureInfo.InvariantCulture)).Append(';');
                }
                sb.Append('\n');
            }
        }
        return sb.ToString();

        static bool IsPlain(Type type)
        {
            Type u = Nullable.GetUnderlyingType(type) ?? type;
            return u.IsPrimitive || u.IsEnum || u == typeof(decimal);
        }
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
