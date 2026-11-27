using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace UWGame.Mods;

/// <summary>
/// NEW: named sets of switched-on mods for SELECT MODS (Kastuk, "Mod settings", 2026-11-14: "May
/// need saveable presets of activated mods. Especially to disable all mods beside debug ones to
/// test vanilla changes").
///
/// A preset is the list of mods whose switch is ON. Applying it switches every other switchable
/// mod off, so a mod installed after the preset was saved stays off under it. It holds switches
/// only, not the mods' settings: those stay as the player left them, kept in ModSettings.xml while
/// a mod is off, as switching by hand does (ModSettings.SetModEnabled).
///
/// Applying goes through <see cref="ModCatalog.Switch"/>, one mod at a time, so everything a click
/// on a switch does - settings to stock, saving, a DLL waiting for the next start - happens the
/// same way. The port's own entry is not a mod and is never touched.
///
/// Kept in a file of its own beside ModSettings.xml, which ModSettings.Save rebuilds from scratch.
/// </summary>
public static class ModPresets
{
    public const string FileName = "ModPresets.xml";

    /// <summary>The built-in preset: the studio's game with only the debugging tools on.</summary>
    public const string StockAndDebugName = "STOCK + DEBUG";

    /// <summary>The mods <see cref="StockAndDebugName"/> keeps on: DebugMod, DebugMonitorMod, StateDumpMod.</summary>
    public static readonly string[] DebugModIds = { "debug", "debugmonitor", "statedump" };

    public sealed class Preset
    {
        /// <summary>Upper case, as the LCD panels are; unique, ignoring case.</summary>
        public string Name;

        /// <summary>The ids of the mods switched on (ModCatalog.Entry.Id).</summary>
        public HashSet<string> On = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Shipped with the game: not in the file, and cannot be overwritten or deleted.</summary>
        public bool BuiltIn;
    }

    /// <summary>Where the file is, or would be: beside ModSettings.xml.</summary>
    public static string FilePath =>
        Config.GetDataFolderPath(Config.DataType.UserModSettings, "", FileName);

    /// <summary>The built-in presets, then the player's in file order. A missing or broken file gives the built-ins.</summary>
    public static List<Preset> All(Action<string, string> log = null)
    {
        List<Preset> all = BuiltIns();
        foreach (Preset p in ReadFile(log))
        {
            if (!all.Any((Preset b) => SameName(b.Name, p.Name)))
            {
                all.Add(p);
            }
        }
        return all;
    }

    public static List<Preset> BuiltIns()
    {
        return new List<Preset>
        {
            new Preset { Name = StockAndDebugName, On = new HashSet<string>(DebugModIds, StringComparer.OrdinalIgnoreCase), BuiltIn = true },
        };
    }

    /// <summary>A name as it is stored: trimmed and upper case. Null when nothing is left.</summary>
    public static string NormalizeName(string name)
    {
        string n = name?.Trim().ToUpperInvariant();
        return string.IsNullOrEmpty(n) ? null : n;
    }

    /// <summary>Whether a preset can be saved under this name: not empty, and not a built-in's.</summary>
    public static bool CanSaveAs(string name)
    {
        string n = NormalizeName(name);
        return n != null && !BuiltIns().Any((Preset b) => SameName(b.Name, n));
    }

    /// <summary>Whether a mod's switch is part of a preset: a mod, not the port, and switchable now.</summary>
    public static bool Counts(ModCatalog.Entry entry) => entry != null && !entry.AlwaysOn && entry.CanSwitch;

    /// <summary>The mods ticked in SELECT MODS now, as a preset.</summary>
    public static Preset Capture(string name, IEnumerable<ModCatalog.Entry> entries)
    {
        Preset p = new Preset { Name = NormalizeName(name) };
        foreach (ModCatalog.Entry e in entries.Where(Counts).Where((ModCatalog.Entry e) => e.ShownOn))
        {
            p.On.Add(e.Id);
        }
        return p;
    }

    /// <summary>
    /// Switches every mod to what <paramref name="preset"/> says, through <paramref name="switchMod"/>
    /// (ModCatalog.Switch in the game). Only a mod whose ticked state differs is switched. Returns
    /// how many were.
    /// </summary>
    public static int Apply(Preset preset, IEnumerable<ModCatalog.Entry> entries, Action<ModCatalog.Entry, bool> switchMod)
    {
        if (preset == null)
        {
            return 0;
        }
        int switched = 0;
        foreach (ModCatalog.Entry e in entries.Where(Counts).ToList())
        {
            bool want = preset.On.Contains(e.Id);
            if (want != e.ShownOn)
            {
                switchMod(e, want);
                switched++;
            }
        }
        return switched;
    }

    /// <summary>Applies <paramref name="preset"/> as clicks on the switches would.</summary>
    public static int Apply(Preset preset, IEnumerable<ModCatalog.Entry> entries, Action<string, string> log = null)
    {
        return Apply(preset, entries, (ModCatalog.Entry e, bool on) => ModCatalog.Switch(e, on, log));
    }

    /// <summary>
    /// Saves the mods ticked now under <paramref name="name"/>, replacing a preset of that name.
    /// False, with nothing written, for a name <see cref="CanSaveAs"/> refuses or a file that cannot be written.
    /// </summary>
    public static bool Save(string name, IEnumerable<ModCatalog.Entry> entries, Action<string, string> log = null)
    {
        if (!CanSaveAs(name))
        {
            return false;
        }
        Preset p = Capture(name, entries);
        List<Preset> own = ReadFile(log);
        int at = own.FindIndex((Preset o) => SameName(o.Name, p.Name));
        if (at >= 0)
        {
            own[at] = p;
        }
        else
        {
            own.Add(p);
        }
        return WriteFile(own, log);
    }

    /// <summary>Deletes the player's preset of this name. False for a built-in, or a name that is not there.</summary>
    public static bool Delete(string name, Action<string, string> log = null)
    {
        List<Preset> own = ReadFile(log);
        if (own.RemoveAll((Preset o) => SameName(o.Name, name)) == 0)
        {
            return false;
        }
        return WriteFile(own, log);
    }

    private static bool SameName(string a, string b) => NormalizeName(a) != null && string.Equals(NormalizeName(a), NormalizeName(b), StringComparison.Ordinal);

    /// <summary>
    /// The player's presets. Missing is normal; a broken file is reported and read as empty, as
    /// ModSettings treats its own.
    /// </summary>
    private static List<Preset> ReadFile(Action<string, string> log)
    {
        List<Preset> presets = new List<Preset>();
        try
        {
            string path = FilePath;
            if (!File.Exists(path))
            {
                return presets;
            }
            XDocument doc = XDocument.Load(path);
            foreach (XElement e in doc.Root?.Elements("Preset") ?? Array.Empty<XElement>())
            {
                string name = NormalizeName((string)e.Attribute("name"));
                if (name == null || presets.Any((Preset p) => SameName(p.Name, name)))
                {
                    continue;
                }
                Preset preset = new Preset { Name = name };
                foreach (XElement on in e.Elements("On"))
                {
                    string id = ((string)on.Attribute("id"))?.Trim();
                    if (!string.IsNullOrEmpty(id))
                    {
                        preset.On.Add(id);
                    }
                }
                presets.Add(preset);
            }
        }
        catch (Exception ex)
        {
            log?.Invoke(FileName + " could not be read and is being ignored (" + ex.Message + ").", "Mod presets");
            presets.Clear();
        }
        return presets;
    }

    private static bool WriteFile(List<Preset> presets, Action<string, string> log)
    {
        string path = null;
        try
        {
            path = FilePath;
            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            XElement root = new XElement("ModPresets");
            root.Add(new XComment(
                " Presets for MODDING -> SELECT MODS. Each lists the mods switched ON; applying one switches" +
                " every other mod off. A preset does not hold the mods' settings. Hand-editable. "));
            foreach (Preset p in presets)
            {
                root.Add(new XElement("Preset", new XAttribute("name", p.Name),
                    p.On.OrderBy((string id) => id, StringComparer.OrdinalIgnoreCase)
                        .Select((string id) => new XElement("On", new XAttribute("id", id)))));
            }
            XmlWriterSettings xws = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                Encoding = new UTF8Encoding(false),
                NewLineChars = "\r\n"
            };
            using (XmlWriter w = XmlWriter.Create(path, xws))
            {
                new XDocument(root).Save(w);
            }
            return true;
        }
        catch (Exception ex)
        {
            log?.Invoke("Could not write " + (path ?? FileName) + ": " + ex.Message, "Mod presets");
            return false;
        }
    }
}
