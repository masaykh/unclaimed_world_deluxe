using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework.Input;

namespace UWGame.Mods;

/// <summary>
/// The key bindings mod, compiled OUT. See UnhiddenMod.Absent.cs for the pattern.
///
/// The options menu has no KEYS section: the studio's keys are edited in Options.xml, and a mod's
/// key, if one is built in, shows in its own category as a text box holding the key's name.
/// </summary>
public static class KeybindMod
{
    /// <summary>Always false: the mod is not present in this build.</summary>
    public const bool Enabled = false;

    public const string ModId = "keys";

    public static void RegisterSettings()
    {
    }

    public static List<(FieldInfo Field, string Label)> VanillaFields() => new List<(FieldInfo, string)>();

    public static readonly Keys[] Bindable = new Keys[0];

    public static string DisplayName(string keyName) => keyName ?? "-";
}
