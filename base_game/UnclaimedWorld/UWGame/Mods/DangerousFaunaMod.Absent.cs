using System.Collections.Generic;
using UWGame.SimSide.Entities;

namespace UWGame.Mods;

/// <summary>
/// The dangerous fauna mod, compiled OUT. See UnhiddenMod.Absent.cs for the pattern.
///
/// Every factor is 1 and the aggro range passes through untouched - the studio's own numbers,
/// which is what the real mod also answers until a row is changed.
/// </summary>
public static class DangerousFaunaMod
{
    /// <summary>Always false: the mod is not present in this build.</summary>
    public const bool Enabled = false;

    public const string ModId = "fauna";

    public enum Stat
    {
        Fighting,
        Damage,
        Toughness,
        Aggro,
        Speed,
        Sensor,
        AttackSpeed
    }

    public static void RegisterSettings()
    {
    }

    public static IEnumerable<string> SpeciesKeys => new string[0];

    public static ModSetting Setting(string speciesKey, Stat stat) => null;

    public static float Factor(EntityType type, Stat stat) => 1f;

    public static float HitChanceFactor(Entity attacker) => 1f;

    public static float DamageFactor(Entity attacker) => 1f;

    public static float ToughnessFactor(Entity target) => 1f;

    public static float SpeedFactor(Entity mover) => 1f;

    public static float SensorFactor(Entity sensing) => 1f;

    public static float AttackSpeedFactor(Entity attacker) => 1f;

    public static float? ScaleAggroRange(Entity entity, float? range) => range;
}
