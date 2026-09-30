using System;
using System.Collections.Generic;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;

namespace UWGame.Mods;

/// <summary>
/// Food left lying about draws pests: rats to meat, field quadites to crops and vegetables.
///
/// THE REQUEST. Kastuk, "Nature can be more natural", point 6: "A lot of exposed food without
/// protection measures will multiply pests gradually, who can come in group and eat out fields and
/// unprotected food in minutes like a swarm of the locusts." And on which pest: "Based on pile of
/// exposed food. If it mostly animal products, spawn rats. If it mostly plants, spawn field
/// quadites."
///
/// HOW - THROUGH THE STUDIO'S OWN SPAWNING. Rats and field quadites already arrive through a
/// wild expedition's Population (Population.Update): a ceiling (MaxMembers) and a growth rate, with
/// spawn points the scenario gives them. This mod raises that ceiling and speeds that growth while
/// food lies exposed, so a swarm builds over game hours, arrives the way those animals always do,
/// and - since rats and quadites already eat stockpiled food and crops - goes for the food by
/// itself. When the food is put away, the bonus goes and the population drifts back.
///
/// "EXPOSED" is player-owned food on the play site that is not inside anything: lying on the
/// ground or in a stockpile zone. Food in a crate, a storehouse or someone's pack is protected.
/// Whether it is animal or plant food is read from its nutrient profile (meat profiles, including
/// the diet mod's preserved ones, against vegetables, staples and berries); prepared meals count as
/// neither. The larger of the two decides which pest the pile draws.
///
/// The bonus is added where Population.Update computes what to fill, never written into the
/// population, so a save carries nothing of this mod and switching it off leaves no trace. Measured
/// once a game hour.
///
/// WHERE IT WORKS: scenarios whose rat and field-quadite groups have a Population - Fields of Tau
/// Ceti, Making Headway, The Clay Pit, Muckroot Mining Site. Twinkler Island and the Muckroot
/// Research Station place fixed groups that never grow, and there it does nothing.
/// </summary>
public static class PestMod
{
    public const string ModId = "pests";

    private static ModSetting enabled;
    private static ModSetting sensitivity;

    public static ModSetting Enabled =>
        enabled ?? (enabled = ModSettings.Toggle(
            ModId, "enabled", "EXPOSED FOOD DRAWS PESTS", defaultValue: false,
            toolTip: "Food lying on the ground or in stockpile zones - not in a crate, storehouse or " +
                     "pack - swells the local rat population (meat) or field quadites (crops and " +
                     "vegetables), until they come in numbers for it.",
            affectsSimulation: true));

    public static ModSetting Sensitivity =>
        sensitivity ?? (sensitivity = ModSettings.Choice(
            ModId, "sensitivity", "PEST SENSITIVITY", new[] { "low", "normal", "high" }, "normal",
            toolTip: "How much exposed food it takes: low 20, normal 10, high 5 units of bulk before " +
                     "the first extra pest, then one more for every half as much again.",
            affectsSimulation: true));

    public static void RegisterSettings()
    {
        _ = Enabled;
        _ = Sensitivity;
    }

    public const string RatKey = "entity:binalRat";
    public const string QuaditeKey = "entity:fieldQuadite";

    /// <summary>
    /// The most extra members one population is given. Most maps have three rat groups (their
    /// studio caps are 3-5, growing ~10 a day), so this is up to 18 more rats at the worst.
    /// </summary>
    public const int MaxExtra = 6;

    private static readonly HashSet<string> AnimalProfiles = new HashSet<string>(StringComparer.Ordinal)
    {
        "poorMeat", "mediumMeat", "richMeat", "meatSoup", "preservedRichMeat", "preservedMediumMeat"
    };

    private static readonly HashSet<string> PlantProfiles = new HashSet<string>(StringComparer.Ordinal)
    {
        "poorVegetables", "richVegetables", "poorStaple", "richStaple", "highEnergy"
    };

    /// <summary>+1 for animal food, -1 for plant food, 0 for anything else (meals, drinks).</summary>
    public static int Classify(string profileKey) =>
        profileKey == null ? 0 : AnimalProfiles.Contains(profileKey) ? 1 : PlantProfiles.Contains(profileKey) ? -1 : 0;

    /// <summary>The threshold in bulk for a sensitivity: low 20, normal 10, high 5.</summary>
    public static float Threshold(string sensitivityValue) =>
        sensitivityValue == "low" ? 20f : sensitivityValue == "high" ? 5f : 10f;

    /// <summary>Extra members for this much exposed bulk: one at the threshold, one more per half-threshold, capped.</summary>
    public static int ExtraFor(float exposedBulk, float threshold)
    {
        if (exposedBulk < threshold || threshold <= 0f) return 0;
        return Math.Min(MaxExtra, 1 + (int)((exposedBulk - threshold) / (threshold / 2f)));
    }

    private static double measuredAt = double.MinValue;
    private static float animalBulk;
    private static float plantBulk;

    /// <summary>
    /// Called from Population.Update: how many members to add to this population's ceiling. 0 for
    /// every population but the rats' and field quadites', and for those unless their food is out.
    /// </summary>
    public static int ExtraMembers(Expedition population)
    {
        string key = population?.Allegiance?.RepresentativeEntityType?.KeyName;
        if (!Enabled.On || (key != RatKey && key != QuaditeKey))
        {
            return 0;
        }
        Measure();
        float threshold = Threshold(Sensitivity.Value);
        if (key == RatKey)
        {
            return animalBulk >= plantBulk ? ExtraFor(animalBulk, threshold) : 0;
        }
        return plantBulk > animalBulk ? ExtraFor(plantBulk, threshold) : 0;
    }

    /// <summary>Growth speeds up with the bonus, so a swarm gathers in hours rather than weeks.</summary>
    public static float GrowthFactor(int extra) => extra > 0 ? 1f + extra / 6f : 1f;

    /// <summary>Totals the player's exposed food, at most once a game hour.</summary>
    private static void Measure()
    {
        Sim sim = The.Sim;
        if (sim?.PlaySite?.PlayerAllegiance == null)
        {
            return;
        }
        double now = sim.TotalUnPausedGameTimeInSeconds;
        if (now >= measuredAt && now - measuredAt < DateAndTime.secondsPerDay / 24.0)
        {
            return;
        }
        measuredAt = now;
        animalBulk = 0f;
        plantBulk = 0f;
        foreach (Expedition expedition in sim.PlaySite.PlayerAllegiance.Expeditions)
        {
            var food = expedition?.OwnedEntities?.Food;
            if (food == null) continue;
            foreach (var byType in food)
            {
                int kind = Classify(byType.Key?.ItemType?.FoodType?.FoodNutrientProfile?.KeyName);
                if (kind == 0) continue;
                foreach (EntityID id in byType.Value)
                {
                    Entity item = Entity.FindByID(id);
                    if (item == null || item.ContainedBy.HasValue || !item.IsOnPlaySite()) continue;
                    if (kind > 0) animalBulk += item.Bulk; else plantBulk += item.Bulk;
                }
            }
        }
    }
}
