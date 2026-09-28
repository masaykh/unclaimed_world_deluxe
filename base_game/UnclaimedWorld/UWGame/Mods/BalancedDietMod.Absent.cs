using System.Collections.Generic;
using UWGame.SimSide.Items;

namespace UWGame.Mods;

/// <summary>
/// The balanced diet mod, compiled OUT. See UnhiddenMod.Absent.cs for the pattern.
/// </summary>
public static class BalancedDietMod
{
    /// <summary>Always false: the mod is not present in this build.</summary>
    public const bool Enabled = false;

    /// <summary>The prefix the mod's settings would carry.</summary>
    public const string ModId = "diet";

    /// <summary>Registers nothing.</summary>
    public static void RegisterSettings()
    {
    }

    /// <summary>Every meal fills as the studio made it.</summary>
    public static float MonotonyFactor(UWGame.SimSide.Entities.Entity eater, UWGame.SimSide.Entities.EntityType food) => 1f;

    public static void RecordMeal(UWGame.SimSide.Entities.Entity eater)
    {
    }

    public static float MonotonyFactorFor(int servingsRemembered) => 1f;

    /// <summary>Leaves every item on the profile the studio gave it.</summary>
    public static void AdjustItems(List<UWGame.SimSide.Entities.EntityType> types)
    {
    }

    /// <summary>
    /// Leaves the studio's nutrient profiles exactly as the table declared them. The real mod
    /// edits the list in place, so doing nothing here is the whole of "not installed".
    /// </summary>
    public static void AdjustProfiles(List<FoodNutrientProfile> profiles)
    {
    }
}
