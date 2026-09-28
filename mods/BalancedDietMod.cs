using System;
using System.Collections.Generic;
using System.Linq;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;

namespace UWGame.Mods;

/// <summary>
/// Meat carries protein, plants carry vitamins, and only a cooked meal carries both.
///
/// THE REPORT. "Food nutrients is implemented, but some food is too universal and contain
/// everything at once, like varied fish dishes, which is quite boring. Let's make animal product
/// contain more of protein and plant products to contain mostly micronutrients." - Kastuk.
///
/// THE STOCK TABLE. Twelve profiles, each naming an amount of foodEnergy, protein and
/// micronutrients: poorMeat, mediumMeat, richMeat, meatSoup, poorVegetables, richVegetables,
/// poorStaple, richStaple, highEnergy, balanced, meal, lowWeightBalancedMeal. Raw meat already
/// carries more protein than a vegetable does - the trouble is that it carries a useful amount of
/// everything else too, so one food source feeds a colonist indefinitely and the kitchen is optional.
///
/// WHAT THIS CHANGES. The RAW families are specialised and the PREPARED ones are left exactly as
/// they are:
///
///   meat        protein x1.4   micronutrients x0.4
///   vegetables  protein x0.5   micronutrients x1.5
///   staples     protein x0.6   micronutrients x0.8
///
/// foodEnergy is untouched throughout: how far a meal goes is the studio's balance, and this is
/// about what it is made of rather than how much of it you need.
///
/// Leaving balanced, meal, lowWeightBalancedMeal and highEnergy alone is the point rather than an
/// omission. A cooked meal SHOULD be complete - that is what cooking is for - and the change is
/// only worth anything if the prepared food stays better than the sum of raw parts.
///
/// MONOTONY lowers the colony's food rating rather than what a meal gives - see the monotony
/// section below.
/// </summary>
public static class BalancedDietMod
{
    public const string ModId = "diet";

    private static ModSetting specialiseRawFood;

    /// <summary>Whether raw food families are pushed towards what they are actually good for.</summary>
    public static ModSetting SpecialiseRawFood =>
        specialiseRawFood ?? (specialiseRawFood = ModSettings.Toggle(
            ModId, "specialiseRawFood", "RAW FOOD IS SPECIALISED", defaultValue: true,
            toolTip: "Meat carries more protein and fewer vitamins, vegetables the reverse, and " +
                     "staples less of both. Cooked meals are unchanged, so a varied diet or a " +
                     "kitchen becomes worth having. Food energy is untouched.",
            affectsSimulation: true, takesEffectOnNextLoad: true));

    private static ModSetting preservedLosesVitamins;

    private static ModSetting alcoholHasEnergy;

    /// <summary>Whether smoked and dried meat and fish keep fewer micronutrients than fresh.</summary>
    public static ModSetting PreservedLosesVitamins =>
        preservedLosesVitamins ?? (preservedLosesVitamins = ModSettings.Toggle(
            ModId, "preservedLosesVitamins", "PRESERVED FOOD LOSES VITAMINS", defaultValue: true,
            toolTip: "Smoked and dried meat and fish keep 40% of the micronutrients of the fresh " +
                     "food - protein and energy stay. Pickled food keeps everything; the vinegar is " +
                     "its cost. Only food made after the change is affected.",
            affectsSimulation: true, takesEffectOnNextLoad: true));

    /// <summary>Whether wine and brandy carry some food energy beside their stimulant.</summary>
    public static ModSetting AlcoholHasEnergy =>
        alcoholHasEnergy ?? (alcoholHasEnergy = ModSettings.Toggle(
            ModId, "alcoholHasEnergy", "ALCOHOL HAS SOME FOOD ENERGY", defaultValue: true,
            toolTip: "Wine and brandy keep their stimulant and gain a third of the food energy of " +
                     "the crystal berries they are made from - a bonus, not a meal.",
            affectsSimulation: true, takesEffectOnNextLoad: true));

    private static ModSetting monotony;

    private static ModSetting monotonyPenalty;

    /// <summary>Whether eating the same dishes lowers the colony's food rating.</summary>
    public static ModSetting Monotony =>
        monotony ?? (monotony = ModSettings.Toggle(
            ModId, "monotony", "THE SAME FOOD EVERY DAY LOWERS FOOD MORALE", defaultValue: true,
            toolTip: "Each colonist remembers their last 10 meals. A dish is fine three times in " +
                     "them; every serving past that makes them a little more tired of their food, " +
                     "and the colony's FOOD rating drops with the average - the rating behind " +
                     "happiness and emigration. Meals still give their full nutrients. Drinks are exempt.",
            affectsSimulation: true));

    /// <summary>How much of the food rating a colony entirely tired of its food loses.</summary>
    public static ModSetting MonotonyPenalty =>
        monotonyPenalty ?? (monotonyPenalty = ModSettings.Choice(
            ModId, "monotonyPenalty", "MONOTONOUS DIET COSTS UP TO", new[] { "10%", "20%", "30%" }, "20%",
            toolTip: "The most the FOOD rating can lose when every colonist has eaten one dish at " +
                     "every one of their last 10 meals.",
            affectsSimulation: true));

    public static void RegisterSettings()
    {
        _ = SpecialiseRawFood;
        _ = PreservedLosesVitamins;
        _ = AlcoholHasEnergy;
        _ = Monotony;
        _ = MonotonyPenalty;
    }

    // ---- monotony ----------------------------------------------------------------------------
    //
    // Kastuk: "Moral or Food need or Comfort to be affected by same dishes day by day ... I want it
    // to be for more dynamic survival without stagnation on the same food." And after the first
    // version, which made a repeated dish fill less: "It's not that monofood will reduce nutrients
    // intake, but it must affect moral somehow", counted "within that 10 dishes memory ... as little
    // variation still can be possible".
    //
    // The game's morale is the colony ratings (FOOD, SECURITY, COMFORT on the HUD): Personality
    // turns them into happiness, and EmigrateDecider, group meetings and tier unlocks read them.
    // FOOD - "COLONY FOOD CONDITIONS" - is baseline + stockpiled food - starving - hunger deaths
    // (FoodStatistics.CombineScores). This adds one more term: MONOTONOUS DIET. A colonist's
    // tiredness is every serving of a dish beyond the third in their last 10 meals, over the 7 that
    // can be beyond it - 1 for one dish at all 10, 2/7 for one dish five times among others, 4/7 for
    // two dishes alternating. The penalty is the group's people's average times MONOTONOUS DIET
    // COSTS UP TO, taken off both the colony's rating (FoodStatisticsForAllegiance) and each
    // group's personal one (FoodStatisticsForMembers), with its own line in the rating's tooltip.
    // Meals give their full nutrients: GoalEat still asks MonotonyFactor, which now only records
    // the dish and answers 1. The studio had started on this: Meal.Variety exists, and nothing
    // constructs a Meal or reads it.
    //
    // The history is the colonist's own CustomFields entry dietRecentMeals - dish keys, newest last,
    // one per dish per meal - which the save already carries. No save-format change.

    public const string HistoryKey = "dietRecentMeals";

    /// <summary>How many meals a colonist remembers.</summary>
    public const int RememberedMeals = 10;

    /// <summary>Servings of one dish in the remembered meals that cost nothing.</summary>
    public const int FreeServings = 3;

    private static readonly Dictionary<EntityID, HashSet<string>> dishesThisMeal = new Dictionary<EntityID, HashSet<string>>();

    /// <summary>
    /// How tired of their food a colonist is, 0 to 1, from their remembered dishes: every serving
    /// of a dish past <see cref="FreeServings"/>, anywhere in the memory, over the most there can be.
    /// </summary>
    public static float TirednessOf(IEnumerable<string> remembered)
    {
        if (remembered == null)
        {
            return 0f;
        }
        int beyond = remembered.GroupBy(k => k, StringComparer.Ordinal).Sum(g => Math.Max(0, g.Count() - FreeServings));
        return Math.Min(1f, beyond / (float)(RememberedMeals - FreeServings));
    }

    /// <summary>The MONOTONOUS DIET COSTS UP TO setting as a fraction: 0.2 unless changed.</summary>
    public static float MaxMonotonyPenalty() =>
        float.TryParse(MonotonyPenalty.Value?.TrimEnd('%'), System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out float p) && p > 0f ? p / 100f : 0.2f;

    /// <summary>
    /// Called from both food ratings' ScoreRating: what MONOTONOUS DIET takes off, 0 to the
    /// setting's maximum - the average tiredness of the group's people. 0 unless switched on.
    /// </summary>
    public static float MonotonyRatingPenalty(UWGame.SimSide.Allegiances.ICanIterateEntities group)
    {
        if (!Monotony.On || group == null)
        {
            return 0f;
        }
        int people = 0;
        float tiredness = 0f;
        group.IterateMembers(delegate(Entity member)
        {
            if (member?.EntityType?.Person != null)
            {
                people++;
                tiredness += TirednessOf(History(member));
            }
        });
        return people == 0 ? 0f : MaxMonotonyPenalty() * tiredness / people;
    }

    /// <summary>The rating tooltip's MONOTONOUS DIET line, in the studio's BASELINE / STOCKPILED FOOD style.</summary>
    public static string MonotonyBreakdown(float penalty)
    {
        if (penalty <= 0f)
        {
            return "";
        }
        var text = new System.Text.StringBuilder();
        Common.AppendLine(text);
        Common.AppendLine(text, "MONOTONOUS DIET");
        text.Append("Subscore: -");
        Common.AppendLine(text, Common.PercentageToString(penalty, includePlusPrefix: false, useColoring: true));
        return text.ToString();
    }

    private static bool IsDish(EntityType food)
    {
        FoodType type = food?.ItemType?.FoodType;
        if (type?.FoodNutrientProfile == null)
        {
            return false;
        }
        bool drink = (type.FoodTags != null && Array.IndexOf(type.FoodTags, "alcoholicBeverage") >= 0)
                     || type.FoodNutrientProfile.KeyName == "lowStimulant";
        return !drink;
    }

    private static List<string> History(Entity eater)
    {
        if (eater.CustomFields != null && eater.CustomFields.TryGetValue(HistoryKey, out var stored) && !string.IsNullOrEmpty(stored.StringResult))
        {
            return stored.StringResult.Split('|').ToList();
        }
        return new List<string>();
    }

    /// <summary>
    /// Called by GoalEat.ConsumeStomachContents for each item a colonist eats. It notes the dish for
    /// RecordMeal, and logs once when a dish reaches its fourth serving in memory. It always answers
    /// 1: monotony costs morale (MonotonyRatingPenalty), not nutrients.
    /// </summary>
    public static float MonotonyFactor(Entity eater, EntityType food)
    {
        if (!Monotony.On || eater?.EntityType?.Person == null || !IsDish(food))
        {
            return 1f;
        }
        if (!dishesThisMeal.TryGetValue(eater.EntityID, out var dishes))
        {
            dishes = new HashSet<string>(StringComparer.Ordinal);
            dishesThisMeal[eater.EntityID] = dishes;
        }
        int servings = History(eater).Count(k => k == food.KeyName);
        if (dishes.Add(food.KeyName) && servings == FreeServings && eater.Intelligence?.Allegiance != null && The.Client != null)
        {
            // Once, on the first serving that fills less - so a player can see why food is going
            // faster, and which dish to vary.
            The.Client.AddLogEvent(eater.Intelligence.Allegiance, The.Client.Log.GeneralEvent, eater,
                "is tired of eating " + (food.Name ?? food.KeyName).ToLowerInvariant() + ".");
        }
        return 1f;
    }

    /// <summary>Called once a meal is eaten: its dishes join the colonist's remembered meals.</summary>
    public static void RecordMeal(Entity eater)
    {
        if (eater == null || !dishesThisMeal.TryGetValue(eater.EntityID, out var dishes))
        {
            return;
        }
        dishesThisMeal.Remove(eater.EntityID);
        if (dishes.Count == 0)
        {
            return;
        }
        List<string> history = History(eater);
        history.AddRange(dishes.OrderBy(d => d, StringComparer.Ordinal));
        if (history.Count > RememberedMeals)
        {
            history = history.Skip(history.Count - RememberedMeals).ToList();
        }
        Entity.SetPropertyValue(ref eater.CustomFields, HistoryKey,
            new UWGame.ClientSide.PropertyPresentation.PropertyResult { StringResult = string.Join("|", history) });
    }

    /// <summary>
    /// Kastuk, on the diet thread: preserved food should lose vitamins ("current rate for loss of
    /// vitamins is okay" - the 40% proposed), and "Alcohol must not replace food there, but just
    /// give some bonus, so let's it be just 1/3 of source plant's energy."
    ///
    /// Profiles are SHARED - smokedStreakFin is richMeat exactly like a fresh streak fin - so the
    /// loss cannot be a change to a profile. Instead AdjustProfiles adds copies (preservedRichMeat,
    /// preservedMediumMeat, alcohol) and AdjustItems points the preserved and alcoholic items at
    /// them. Food.NutrientBulkAmounts is saved per item, so existing stock keeps what it had.
    /// </summary>
    private const float PreservedMicronutrients = 0.4f;

    private const float AlcoholEnergyOfSource = 1f / 3f;

    /// <summary>Smoked and dried meat and fish - the items that get a preserved profile.</summary>
    private static bool IsPreserved(string itemKey) =>
        itemKey != null && (itemKey.StartsWith("item:smoked", StringComparison.Ordinal) || itemKey.StartsWith("item:dried", StringComparison.Ordinal));

    /// <summary>
    /// Called at the end of BaseDataLoader.InitEntityTypes with the whole entity list: points the
    /// preserved and alcoholic items at the profiles AdjustProfiles added.
    /// </summary>
    public static void AdjustItems(List<EntityType> types)
    {
        if (types == null)
        {
            return;
        }
        var profiles = GameData.Instance.AllFoodNutrientProfiles;
        foreach (EntityType type in types)
        {
            FoodType food = type?.ItemType?.FoodType;
            if (food?.FoodNutrientProfile == null)
            {
                continue;
            }
            if (PreservedLosesVitamins.On && IsPreserved(type.KeyName)
                && profiles.TryGetValue("preserved" + Capitalised(food.FoodNutrientProfile.KeyName), out FoodNutrientProfile preserved))
            {
                food.FoodNutrientProfile = preserved;
            }
            else if (AlcoholHasEnergy.On && food.FoodTags != null && Array.IndexOf(food.FoodTags, "alcoholicBeverage") >= 0
                && profiles.TryGetValue("alcohol", out FoodNutrientProfile alcohol))
            {
                food.FoodNutrientProfile = alcohol;
            }
        }
    }

    private static string Capitalised(string key) =>
        string.IsNullOrEmpty(key) ? key : char.ToUpperInvariant(key[0]) + key.Substring(1);

    /// <summary>A profile with every amount copied, and one nutrient scaled.</summary>
    private static FoodNutrientProfile CopyOf(FoodNutrientProfile source, string key, string scaledNutrient = null, float factor = 1f)
    {
        return new FoodNutrientProfile
        {
            KeyName = key,
            Name = source.Name,
            FoodNutrientTypes = source.FoodNutrientTypes
                .Where(a => a?.Nutrient != null)
                .Select(a => new FoodNutrientAmount
                {
                    Nutrient = a.Nutrient,
                    Amount = a.Nutrient.KeyName == scaledNutrient ? a.Amount * factor : a.Amount
                })
                .ToArray()
        };
    }

    private static void AddPreservedAndAlcoholProfiles(List<FoodNutrientProfile> profiles)
    {
        FoodNutrientProfile Find(string key) => profiles.FirstOrDefault(p => p != null && !p.DeleteRecord && p.KeyName == key);

        if (PreservedLosesVitamins.On)
        {
            foreach (string key in new[] { "richMeat", "mediumMeat" })
            {
                FoodNutrientProfile fresh = Find(key);
                if (fresh?.FoodNutrientTypes != null && Find("preserved" + Capitalised(key)) == null)
                {
                    profiles.Add(CopyOf(fresh, "preserved" + Capitalised(key), "micronutrients", PreservedMicronutrients));
                }
            }
        }

        // The stimulant from the studio's lowStimulant, plus a third of the crystal berries' energy
        // (highEnergy) - both drinks are made from them.
        FoodNutrientProfile stimulant = Find("lowStimulant");
        FoodNutrientAmount berryEnergy = Find("highEnergy")?.FoodNutrientTypes?.FirstOrDefault(a => a?.Nutrient?.KeyName == "foodEnergy");
        if (AlcoholHasEnergy.On && stimulant?.FoodNutrientTypes != null && berryEnergy != null && Find("alcohol") == null)
        {
            FoodNutrientProfile alcohol = CopyOf(stimulant, "alcohol");
            alcohol.FoodNutrientTypes = alcohol.FoodNutrientTypes
                .Where(a => a.Nutrient.KeyName != "foodEnergy")
                .Concat(new[] { new FoodNutrientAmount { Nutrient = berryEnergy.Nutrient, Amount = berryEnergy.Amount * AlcoholEnergyOfSource } })
                .ToArray();
            profiles.Add(alcohol);
        }
    }

    /// <summary>The profiles for food that comes off an animal.</summary>
    private static readonly string[] MeatProfiles = { "poorMeat", "mediumMeat", "richMeat", "meatSoup" };

    /// <summary>The profiles for food that grows.</summary>
    private static readonly string[] VegetableProfiles = { "poorVegetables", "richVegetables" };

    /// <summary>Grains and roots - bulk, not nutrition.</summary>
    private static readonly string[] StapleProfiles = { "poorStaple", "richStaple" };

    /// <summary>
    /// Called at the end of BaseDataLoader.InitFoodNutrientProfiles, with the table the game is
    /// about to use.
    ///
    /// Scales rather than replaces: every number stays a multiple of the studio's, so the
    /// relationship between poor, medium and rich survives, and so does anything they change in a
    /// later version of the game.
    /// </summary>
    public static void AdjustProfiles(List<FoodNutrientProfile> profiles)
    {
        if (profiles == null)
        {
            return;
        }

        foreach (FoodNutrientProfile profile in profiles)
        {
            if (!SpecialiseRawFood.On || profile?.FoodNutrientTypes == null || profile.DeleteRecord)
            {
                continue;
            }
            if (Contains(MeatProfiles, profile.KeyName))
            {
                Scale(profile, "protein", 1.4f);
                Scale(profile, "micronutrients", 0.4f);
            }
            else if (Contains(VegetableProfiles, profile.KeyName))
            {
                Scale(profile, "protein", 0.5f);
                Scale(profile, "micronutrients", 1.5f);
            }
            else if (Contains(StapleProfiles, profile.KeyName))
            {
                Scale(profile, "protein", 0.6f);
                Scale(profile, "micronutrients", 0.8f);
            }
        }

        // After the specialisation, so "preserved" is measured from the fresh food as it now is.
        AddPreservedAndAlcoholProfiles(profiles);
    }

    private static bool Contains(string[] keys, string key)
    {
        foreach (string candidate in keys)
        {
            if (candidate == key)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Multiplies one nutrient in one profile, if the profile carries it at all.
    ///
    /// A profile that does not list a nutrient is left alone rather than given one: "less protein"
    /// is a change to food that has protein, and inventing an amount where the studio wrote none
    /// would be a different decision wearing the same switch.
    /// </summary>
    private static void Scale(FoodNutrientProfile profile, string nutrientKey, float factor)
    {
        FoodNutrientAmount[] amounts = profile.FoodNutrientTypes;
        foreach (FoodNutrientAmount amount in amounts)
        {
            if (amount?.Nutrient != null && amount.Nutrient.KeyName == nutrientKey)
            {
                amount.Amount *= factor;
            }
        }
    }
}
