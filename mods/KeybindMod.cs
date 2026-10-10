using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework.Input;
using UWGame.ClientSide;

namespace UWGame.Mods;

/// <summary>
/// A KEYS section in the options menu: the studio's key bindings and the mods' keys, rebound by
/// clicking a key and pressing the new one.
///
/// THE REQUEST. Kastuk, "Magnification setting": "it's time to bring Keybinding menu into
/// Settings, with adding current vanilla keys, part of which is hidden in Options.xml, and also add
/// new keys from the mods into it." And: "Keybindings menu can be parallel mod ... just visual
/// config like mod settings."
///
/// THE STUDIO'S KEYS are public Keys fields on Options - KeyScrollLeft ... KeyGameSpeed3,
/// ToggleIngameMenu, CloseRosterPanel - which Options.Write saves to Options.xml and nothing in the
/// game lets you change. They are found by reflection (<see cref="VanillaFields"/>), so a field the
/// studio adds later appears without an edit here, under its field name until
/// <see cref="Labels"/> gives it a better one. Their home stays Options.xml: the section reads the
/// fields and writes them back on OK, and a hand-edited file still works.
///
/// THE MODS' KEYS are ModSettings of kind Key (ModSettings.Key), kept in ModSettings.xml like any
/// other setting: HUD's reveal key (LeftAlt) and UNHIDDEN's shadow toggle (O). A mod that adds a
/// key registers it and it shows up here.
///
/// IN THE DIALOG (OptionsDialog.BuildKeysCategory). Click a key and press the new one; Escape
/// cancels, so Escape itself cannot be bound. A key used twice turns red. Nothing is applied until
/// OK, like every other control there. Interface only; nothing the simulation reads.
/// </summary>
public static class KeybindMod
{
    public const string ModId = "keys";

    /// <summary>The section is shown whenever the mod is built in.</summary>
    public static bool Enabled => true;

    public static void RegisterSettings()
    {
        // SELECT MODS (main menu -> MODDING): who wrote it, what it does, and a picture.
        ModSettings.Describe(ModId, "Jerrybi",
            "A KEYS section in the options menu: the studio's key bindings and the mods' keys, rebound by clicking a key and pressing the new one.",
            "HUD_thumbnail_radioHut");
    }

    /// <summary>Readable names for the studio's fields, in the order the section lists them.</summary>
    public static (string Field, string Label)[] Labels => new[]
    {
        ("KeyScrollUp", UWGame.Locale.Text("SCROLL UP")),
        ("KeyScrollDown", UWGame.Locale.Text("SCROLL DOWN")),
        ("KeyScrollLeft", UWGame.Locale.Text("SCROLL LEFT")),
        ("KeyScrollRight", UWGame.Locale.Text("SCROLL RIGHT")),
        ("KeyPause1", UWGame.Locale.Text("PAUSE")),
        ("KeyPause2", UWGame.Locale.Text("PAUSE (2ND KEY)")),
        ("KeyPause3", UWGame.Locale.Text("PAUSE (3RD KEY)")),
        ("KeyGameSpeed1", UWGame.Locale.Text("NORMAL SPEED")),
        ("KeyGameSpeed2", UWGame.Locale.Text("DOUBLE SPEED")),
        ("KeyGameSpeed3", UWGame.Locale.Text("FOUR TIMES SPEED")),
        ("ToggleIngameMenu", UWGame.Locale.Text("GAME MENU")),
        ("CloseRosterPanel", UWGame.Locale.Text("CLOSE ROSTER PANEL")),
    };

    /// <summary>Every public Keys field on Options, labelled ones first in their order.</summary>
    public static List<(FieldInfo Field, string Label)> VanillaFields()
    {
        var fields = typeof(Options).GetFields(BindingFlags.Public | BindingFlags.Instance)
            .Where(f => f.FieldType == typeof(Keys)).ToList();
        var result = new List<(FieldInfo, string)>();
        foreach (var (name, label) in Labels)
        {
            FieldInfo f = fields.FirstOrDefault(x => x.Name == name);
            if (f != null)
            {
                result.Add((f, label));
                fields.Remove(f);
            }
        }
        foreach (FieldInfo f in fields)
        {
            result.Add((f, f.Name.ToUpperInvariant()));
        }
        return result;
    }

    /// <summary>Every key a player can bind: all of them but None and Escape, which cancels.</summary>
    public static readonly Keys[] Bindable = ((Keys[])Enum.GetValues(typeof(Keys)))
        .Where(k => k != Keys.None && k != Keys.Escape).Distinct().ToArray();

    /// <summary>A key as the button shows it: D1 as 1, LeftAlt as LEFT ALT.</summary>
    public static string DisplayName(string keyName)
    {
        if (string.IsNullOrEmpty(keyName))
        {
            return "-";
        }
        if (keyName.Length == 2 && keyName[0] == 'D' && char.IsDigit(keyName[1]))
        {
            return keyName.Substring(1);
        }
        var text = new System.Text.StringBuilder();
        for (int i = 0; i < keyName.Length; i++)
        {
            if (i > 0 && char.IsUpper(keyName[i]) && !char.IsUpper(keyName[i - 1]))
            {
                text.Append(' ');
            }
            text.Append(char.ToUpperInvariant(keyName[i]));
        }
        return text.ToString();
    }
}
