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
    private static int WriteStrings(string literalsPath, string outPath)
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
        var modIds = new SortedSet<string>(StringComparer.Ordinal);
        foreach (UWGame.Mods.ModSetting setting in UWGame.Mods.ModSettings.All)
        {
            entries[Locale.SettingKey(setting.Id)] = setting.Label;
            if (!string.IsNullOrEmpty(setting.ToolTip))
            {
                entries[Locale.SettingTipKey(setting.Id)] = setting.ToolTip;
            }
            modIds.Add(setting.ModId);
        }
        foreach (string modId in modIds)
        {
            entries[Locale.SettingGroupKey(modId)] = UWGame.Mods.ModSettings.CategoryLabel(modId);
        }
        foreach (var type in GameData.Instance.AllEntityTypes.Values.Where(t => t.ItemType != null))
        {
            entries[Locale.ItemNameKey(type.KeyName)] = type.Name;
            if (!string.IsNullOrEmpty(type.Description))
            {
                entries[Locale.ItemDescriptionKey(type.KeyName)] = type.Description;
            }
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
                          + $"{list.Count(e => e.Key.StartsWith("(ITEM", StringComparison.Ordinal))} items) -> {outPath}");
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

        int rc = Run(Sim.SerializeMode.NoSerialize, "base tables, the way the game loads them");
        if (rc != 0) return rc;
        if (!ValidateDataComplete()) return 1;
        Check(GameData.Instance.AllEntityTypes["item:acetylene"].Name == "Ацетилен", "an item's name is translated when the tables are completed");
        Check(GameData.Instance.AllEntityTypes["item:advancedCookingPot"].Name != null
              && !GameData.Instance.AllEntityTypes["item:advancedCookingPot"].Name.StartsWith("(ITEM)", StringComparison.Ordinal),
              "an item the file leaves out keeps its English name");

        language.Value = "Broken";
        Check(Locale.Text("SAVE GAME") == "SAVE GAME", "a file that cannot be read: English, no crash");
        language.Value = Locale.InvariantCulture;
        Check(Locale.Text("SAVE GAME") == "SAVE GAME", "back to English");

        Console.WriteLine(failures == 0 ? "locale self-test OK" : $"locale self-test FAILED - {failures} check(s)");
        return failures == 0 ? 0 : 1;
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
