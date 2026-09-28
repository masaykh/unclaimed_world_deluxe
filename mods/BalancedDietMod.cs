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
/// NOT DONE: morale from eating the same thing every day. That needs somewhere to remember what a
/// colonist has been eating, which is new state on the person and a save-format change, rather
/// than a different number in a table.
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

    public static void RegisterSettings()
    {
        _ = SpecialiseRawFood;
        _ = PreservedLosesVitamins;
        _ = AlcoholHasEnergy;
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
