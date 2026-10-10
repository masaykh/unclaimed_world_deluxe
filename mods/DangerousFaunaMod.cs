using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using UWGame.SimSide.Entities;

namespace UWGame.Mods;

/// <summary>
/// How dangerous each dangerous animal is: a multiplier per species for how often it hits, how
/// hard, from how far it comes for you, and how much it takes to bring down.
///
/// THE REQUEST. Kastuk, after the melee investigation showed swarmers are harmless by the studio's
/// own numbers (unarmedFighting 0.2; the twinkler's attacks, average 8 piercing, against a suit
/// that takes 30% and then 5 more - about 0.6 damage a hit): "Need to be separated mod with sliders
/// or field of modifiers for mentioned parameters of aggressive fauna ... twinkler, swarmer,
/// patrician, that one four-legged one and poison leafy one." Every wild animal with an
/// AggroRange in CreatureLoader is listed, so an extra row at x1 costs nothing - and, at his later
/// request, the player's own fighters: the dog and the guard robot (HOUND).
///
/// WHY AT RUNTIME AND NOT IN THE TABLES. The obvious version scales the creature table at load,
/// and it would be wrong three ways. The swarmer uses the twinkler's attacks (twinklerLowRight,
/// twinklerHighRight) and the twinkler's body type, and so does the player's domesticated
/// twinkler - scaling those entries changes all three. And aggro range is a per-caste BioProperty
/// COPIED INTO EACH CREATURE when it spawns, so animals already in a save would keep the old
/// value. A multiplier applied where the number is USED is per species, reaches creatures that
/// already exist, and changes nothing in the data:
///
///   fighting   GoalDoAttack.ComputeChanceToHit   x attacker's factor
///   damage     AttackType.HitTarget              x attacker's factor, into the energy factor -
///                                                 so BEFORE armour, as a stronger blow would be
///   toughness  AttackType.HitTarget              / target's factor, AFTER armour
///   aggro      Entity.GetAggroRange              x factor
///   speed      Locomotor.CalculateSpeed          x factor, every gait
///   sensor     Entity.GetDay/NightSensorRange    x factor
///   attack     GoalDoAttack timings              / factor, and RenderAsModel plays the attack
///   speed                                        animation x factor so it still meets the hit
///
/// Every value defaults to x1, which is the studio's game exactly: the mod does nothing until a
/// row is changed. Settings affect the simulation, so they go into a save's settings signature.
/// </summary>
public static class DangerousFaunaMod
{
    public const string ModId = "fauna";

    /// <summary>The four things each species can be scaled on, in the order the menu lists them.</summary>
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

    private static readonly string[] StatKeys = { "fighting", "damage", "toughness", "aggro", "speed", "sensor", "attackspeed" };

    private static readonly string[] StatLabels = { "HIT CHANCE", "DAMAGE", "TOUGHNESS", "AGGRO RANGE", "MOVE SPEED", "SENSOR RANGE", "ATTACK SPEED" };

    private static readonly string[] StatToolTips =
    {
        "How often its attacks land. Multiplies its chance to hit (GoalDoAttack.ComputeChanceToHit).",
        "How hard it hits, before armour - armour still takes its share, so a small increase can " +
        "turn a harmless blow into a wound (AttackType.HitTarget).",
        "How much it takes to bring down. Damage it receives is divided by this, after armour.",
        "How far away it notices you and comes for you (Entity.GetAggroRange).",
        "How fast it moves, in every gait - walking, running, chasing (Locomotor.CalculateSpeed). " +
        "Its walk animation speeds up to match on its own.",
        "How far it sees and hears, day and night (Entity.GetDaySensorRange / GetNightSensorRange).",
        "How fast it attacks: wind-up, swing, miss and the rest after, with the attack animation " +
        "played at the same speed so it still lines up with the hit (GoalDoAttack, RenderAsModel)."
    };

    /// <summary>Choices as shown and stored; "x1" is the studio's number.</summary>
    private static readonly string[] Multipliers = { "x0.25", "x0.5", "x0.75", "x1", "x1.5", "x2", "x3", "x5" };

    private const string Stock = "x1";

    /// <summary>Entity type key, and the name the menu shows.</summary>
    private static readonly string[,] SpeciesTable =
    {
        { "entity:twinkler", "TWINKLER" },
        { "entity:swarmer", "SWARMER" },
        { "entity:patrician", "PATRICIAN" },
        { "entity:whipjaw", "GREAT WHIPJAW" },
        { "entity:megapod", "MEGAPOD" },
        { "entity:spikePlant", "URSINIX" },
        { "entity:bushDragon", "NORTHERN BUSH DRAGON" },
        { "entity:spoakDendront", "SPOAK DENDRONT" },
        { "entity:swampDendront", "SWAMP DENDRONT" },
        { "entity:dog", "DOG" },
        { "entity:guardRobot", "GUARD ROBOT (HOUND)" }
    };

    /// <summary>One species' four settings, with the last parsed value of each.</summary>
    private sealed class Species
    {
        public string Key;

        public ModSetting[] Settings = new ModSetting[StatKeys.Length];

        private readonly string[] parsedFrom = new string[StatKeys.Length];

        private readonly float[] parsed = new float[StatKeys.Length];

        public float Factor(Stat stat)
        {
            int i = (int)stat;
            string v = Settings[i].Value;
            // Parsed again only when the stored string changes, which is a reference comparison
            // on every other call - these are asked for on hot paths (aggro range is read for
            // every creature evaluating every threat).
            if (!ReferenceEquals(v, parsedFrom[i]))
            {
                parsed[i] = Parse(v);
                parsedFrom[i] = v;
            }
            return parsed[i];
        }
    }

    private static Dictionary<string, Species> byKey;

    private static readonly Species None = new Species();

    /// <summary>
    /// Species by EntityType reference, so a hot path hashes an object rather than a string. Weak,
    /// because the data tables are rebuilt on every load and the old types must be let go.
    /// </summary>
    private static readonly ConditionalWeakTable<EntityType, Species> byType = new ConditionalWeakTable<EntityType, Species>();

    public static void RegisterSettings()
    {
        // SELECT MODS (main menu -> MODDING): who wrote it, what it does, and a picture.
        ModSettings.Describe(ModId, "Jerrybi",
            "Tune how dangerous each dangerous animal is: a multiplier per species for how often it hits, how hard, how far away it notices you, how fast it moves and how much it takes to bring down.",
            "HUD_thumbnail_ursinix");
        ModSettings.SetCategoryLabel(ModId, "DANGEROUS FAUNA");
        if (byKey != null)
        {
            return;
        }
        var table = new Dictionary<string, Species>(StringComparer.Ordinal);
        for (int s = 0; s < SpeciesTable.GetLength(0); s++)
        {
            var species = new Species { Key = SpeciesTable[s, 0] };
            string shortKey = species.Key.Substring("entity:".Length);
            for (int i = 0; i < StatKeys.Length; i++)
            {
                species.Settings[i] = ModSettings.Choice(
                    ModId, shortKey + "." + StatKeys[i], SpeciesTable[s, 1] + " " + StatLabels[i],
                    Multipliers, Stock, toolTip: StatToolTips[i] + " x1 is the studio's number.",
                    affectsSimulation: true);
            }
            table[species.Key] = species;
        }
        byKey = table;
    }

    /// <summary>The species keys this mod scales - for the self-test in tools/DataExport.</summary>
    public static IEnumerable<string> SpeciesKeys
    {
        get
        {
            for (int s = 0; s < SpeciesTable.GetLength(0); s++)
            {
                yield return SpeciesTable[s, 0];
            }
        }
    }

    /// <summary>The setting behind one species' stat, or null for a species not listed.</summary>
    public static ModSetting Setting(string speciesKey, Stat stat)
    {
        RegisterSettings();
        return byKey.TryGetValue(speciesKey ?? "", out Species species) ? species.Settings[(int)stat] : null;
    }

    /// <summary>The multiplier for one stat of one entity type; 1 for anything not listed.</summary>
    public static float Factor(EntityType type, Stat stat)
    {
        if (type == null)
        {
            return 1f;
        }
        RegisterSettings();
        Species species = byType.GetValue(type, t => byKey.TryGetValue(t.KeyName ?? "", out Species found) ? found : None);
        return species == None ? 1f : species.Factor(stat);
    }

    // The four call sites. Each is one multiplication at the place the studio computes the number.

    public static float HitChanceFactor(Entity attacker) => Factor(attacker?.EntityType, Stat.Fighting);

    public static float DamageFactor(Entity attacker) => Factor(attacker?.EntityType, Stat.Damage);

    public static float ToughnessFactor(Entity target) => Factor(target?.EntityType, Stat.Toughness);

    public static float SpeedFactor(Entity mover) => Factor(mover?.EntityType, Stat.Speed);

    public static float SensorFactor(Entity sensing) => Factor(sensing?.EntityType, Stat.Sensor);

    public static float AttackSpeedFactor(Entity attacker) => Factor(attacker?.EntityType, Stat.AttackSpeed);

    public static float? ScaleAggroRange(Entity entity, float? range)
    {
        return range.HasValue ? range.Value * Factor(entity?.EntityType, Stat.Aggro) : range;
    }

    /// <summary>"x1.5" -> 1.5. Anything unreadable is x1: a hand-edited file must not change the game.</summary>
    private static float Parse(string value)
    {
        if (value != null && value.Length > 1 && (value[0] == 'x' || value[0] == 'X')
            && float.TryParse(value.Substring(1), NumberStyles.Float, CultureInfo.InvariantCulture, out float f)
            && f > 0f)
        {
            return f;
        }
        return 1f;
    }
}
