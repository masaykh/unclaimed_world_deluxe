using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace UWGame.Mods;

/// <summary>
/// Every mod SELECT MODS lists (main menu -> MODDING -> SELECT MODS; tripleacoder, "Main menu",
/// 2026-10-10): what each is called, who wrote it, what it does, a picture, whether it is on, and
/// its settings.
///
/// THREE SOURCES. The mods that ship with the game describe themselves when they register
/// (<see cref="ModSettings.Describe"/>). A mod that registers settings without describing itself
/// is listed under its settings' name. And every DLL in user/Mods (<see cref="ModLoader.ModFiles"/>)
/// is listed - loaded, failed or switched off - with what a file beside it says:
///
///     user/Mods/MyMod.dll    the mod
///     user/Mods/MyMod.xml    &lt;Mod&gt;&lt;Name&gt;MY MOD&lt;/Name&gt;&lt;Author&gt;Me&lt;/Author&gt;&lt;Description&gt;...&lt;/Description&gt;&lt;/Mod&gt;
///     user/Mods/MyMod.png    its picture, about 100 x 100
///
/// all optional. A DLL whose settings use its file name as their mod id is one entry with them.
/// </summary>
public static class ModCatalog
{
    public enum ModState
    {
        /// <summary>Running.</summary>
        On,

        /// <summary>Switched off in SELECT MODS.</summary>
        Off,

        /// <summary>Off for this session: the game was started with -nomods.</summary>
        OffThisSession,

        /// <summary>A DLL that could not be loaded; <see cref="Entry.Problem"/> says why.</summary>
        Failed,

        /// <summary>Switched on or off since the game started; that takes effect at the next start.</summary>
        AtNextStart,
    }

    public sealed class Entry
    {
        /// <summary>The settings' mod id, or "dll:" and the file name for a third-party mod.</summary>
        public string Id;

        /// <summary>Shown as the entry's title - in the chosen language for the game's own.</summary>
        public string Name;

        public string Author;

        /// <summary>In the chosen language for the game's own (Locale, "(MOD DESCRIPTION)" and the id).</summary>
        public string Description;

        /// <summary>A GUI sprite name, a PNG file's full path, or null.</summary>
        public string Thumbnail;

        /// <summary>A third-party DLL's file name, or null for the game's own.</summary>
        public string FileName;

        /// <summary>Its settings, in menu order; changed in OPTIONS -> MODS.</summary>
        public List<ModSetting> Settings = new List<ModSetting>();

        public ModState State;

        /// <summary>Why a DLL failed, or null.</summary>
        public string Problem;

        /// <summary>Whether its switch can be used now. Not under -nomods, and not for a build without the loader.</summary>
        public bool CanSwitch;

        /// <summary>Whether it is switched on, as SELECT MODS shows it - which may differ from <see cref="State"/> until the next start.</summary>
        public bool SwitchedOn => ModSettings.IsModEnabled(Id);

        /// <summary>How many of its settings differ from the studio's game.</summary>
        public int Changed => Settings.Count((ModSetting s) => !s.IsStock);
    }

    /// <summary>The key of a mod's description in a language file.</summary>
    public static string DescriptionKey(string modId) => "(MOD DESCRIPTION)" + modId;

    /// <summary>
    /// The DLL files switched on or off since the game started - the switch changes what loads at
    /// the next start, and until then the entry says so.
    /// </summary>
    private static readonly HashSet<string> switchedThisSession = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>Every mod, the game's own first in their registration order, then user/Mods in load order.</summary>
    public static List<Entry> Entries()
    {
        var entries = new List<Entry>();
        var byModId = ModSettings.All
            .Where((ModSetting s) => s.ModId != PortSettings.ModId)
            .GroupBy((ModSetting s) => s.ModId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary((IGrouping<string, ModSetting> g) => g.Key, (IGrouping<string, ModSetting> g) => g.ToList(), StringComparer.OrdinalIgnoreCase);
        var dllNames = new HashSet<string>(ModLoader.ModFiles().Select(Path.GetFileNameWithoutExtension), StringComparer.OrdinalIgnoreCase);

        foreach (ModSettings.ModInfo info in ModSettings.Described)
        {
            entries.Add(new Entry
            {
                Id = info.Id,
                Name = Locale.Text(Locale.SettingGroupKey(info.Id), ModSettings.CategoryLabel(info.Id)),
                Author = info.Author,
                Description = info.Description == null ? null : Locale.Text(DescriptionKey(info.Id), info.Description),
                Thumbnail = info.Thumbnail,
                Settings = byModId.TryGetValue(info.Id, out var own) ? own : new List<ModSetting>(),
                State = BuiltInState(info.Id),
                CanSwitch = !ModSettings.StockOnly,
            });
        }
        // Settings of a mod that did not describe itself, and is not a DLL listed below.
        foreach (var group in byModId)
        {
            if (entries.Any((Entry e) => string.Equals(e.Id, group.Key, StringComparison.OrdinalIgnoreCase)) || dllNames.Contains(group.Key))
            {
                continue;
            }
            entries.Add(new Entry
            {
                Id = group.Key,
                Name = Locale.Text(Locale.SettingGroupKey(group.Key), ModSettings.CategoryLabel(group.Key)),
                Settings = group.Value,
                State = BuiltInState(group.Key),
                CanSwitch = !ModSettings.StockOnly,
            });
        }
        foreach (string path in ModLoader.ModFiles())
        {
            entries.Add(DllEntry(path, byModId));
        }
        return entries;
    }

    private static ModState BuiltInState(string modId)
    {
        if (ModSettings.StockOnly)
        {
            return ModState.OffThisSession;
        }
        // The bundled mod is off from the start of the game, so a change shows at the next.
        if (string.Equals(modId, UnhiddenMod.ModId, StringComparison.OrdinalIgnoreCase) && UnhiddenMod.Enabled != ModSettings.IsModEnabled(modId))
        {
            return ModState.AtNextStart;
        }
        return ModSettings.IsModEnabled(modId) ? ModState.On : ModState.Off;
    }

    private static Entry DllEntry(string path, Dictionary<string, List<ModSetting>> byModId)
    {
        string name = Path.GetFileNameWithoutExtension(path);
        string fileName = Path.GetFileName(path);
        var entry = new Entry
        {
            Id = ModSettings.DllModId(name),
            Name = name.ToUpperInvariant(),
            FileName = fileName,
            Settings = byModId.TryGetValue(name, out var own) ? own : new List<ModSetting>(),
            CanSwitch = ModLoader.Enabled,
        };
        string manifest = Path.ChangeExtension(path, ".xml");
        if (File.Exists(manifest))
        {
            try
            {
                XElement root = XDocument.Load(manifest).Root;
                entry.Name = ((string)root?.Element("Name"))?.Trim() is string n && n.Length > 0 ? n : entry.Name;
                entry.Author = ((string)root?.Element("Author"))?.Trim();
                entry.Description = ((string)root?.Element("Description"))?.Trim();
            }
            catch (Exception ex)
            {
                entry.Problem = Path.GetFileName(manifest) + " could not be read: " + ex.Message;
            }
        }
        string picture = Path.ChangeExtension(path, ".png");
        if (File.Exists(picture))
        {
            entry.Thumbnail = picture;
        }
        string failed = ModLoader.Failed.FirstOrDefault((string f) => f.StartsWith(fileName + " - ", StringComparison.OrdinalIgnoreCase));
        if (!ModLoader.Enabled)
        {
            entry.State = ModState.OffThisSession;
        }
        else if (switchedThisSession.Contains(entry.Id))
        {
            entry.State = ModState.AtNextStart;
        }
        else if (failed != null)
        {
            entry.State = ModState.Failed;
            entry.Problem = failed.Substring(fileName.Length + 3);
        }
        else if (ModLoader.Disabled.Contains(fileName, StringComparer.OrdinalIgnoreCase) || !ModSettings.IsModEnabled(entry.Id))
        {
            entry.State = ModState.Off;
        }
        else
        {
            entry.State = ModState.On;
        }
        return entry;
    }

    /// <summary>
    /// Switches a mod on or off (<see cref="ModSettings.SetModEnabled"/>) and saves. The game's own
    /// mods follow at once, their settings to stock or back; the bundled Unhidden Mod and a DLL at
    /// the next start of the game.
    /// </summary>
    public static void Switch(Entry entry, bool on, Action<string, string> log = null)
    {
        if (entry == null || !entry.CanSwitch)
        {
            return;
        }
        ModSettings.SetModEnabled(entry.Id, on, log);
        if (entry.FileName != null)
        {
            if (!switchedThisSession.Remove(entry.Id))
            {
                switchedThisSession.Add(entry.Id);
            }
        }
    }
}
