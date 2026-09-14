using Microsoft.Xna.Framework;
using UWGame.Control;

namespace UWGame.Mods;

/// <summary>
/// The map edge mod, compiled OUT. See UnhiddenMod.Absent.cs for the pattern.
///
/// This one is the clearest case for a stub: MapClient.ChangeMapWindowWorldPosition already
/// branches on <see cref="Enabled"/> and keeps the studio's clamp in the other arm, so with
/// Enabled a compile-time false the mod's arm is dead code the compiler drops - and
/// <see cref="Clamp"/> exists only so that dead arm still type-checks.
/// </summary>
public static class MapEdgeMod
{
    /// <summary>Always false: the mod is not present in this build.</summary>
    public const bool Enabled = false;

    /// <summary>The prefix the mod's settings would carry.</summary>
    public const string ModId = "mapedge";

    /// <summary>Registers nothing.</summary>
    public static void RegisterSettings()
    {
    }

    /// <summary>Unreachable - the only call to it sits behind <see cref="Enabled"/>.</summary>
    public static Vector2 Clamp(Vector2 wanted, Dimension drawArea, float mapWorldWidth, float mapWorldHeight)
    {
        return wanted;
    }
}
