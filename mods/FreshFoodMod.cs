using UWGame.SimSide.AI;
using UWGame.SimSide.Entities;

namespace UWGame.Mods;

/// <summary>
/// Colonists eat what will spoil first.
///
/// THE REQUEST. Kastuk, "auto food preservation": "Whats need adjusting is foods priority, as they
/// often eat long preserved food instead of fresh perishables" - pickled fish before clamwich soup
/// or powdered crystalberries. tripleacoder: "This could be tweaked in the desirability calculation
/// in the eat evaluator."
///
/// THE STUDIO HAD THE INTENT AND WEIGHED IT AT NOTHING. EvaluateEat.ScoreCondition carries their
/// comment - "score should be higher the closer the food is to spoiling! To avoid food waste and
/// preserve rations" - but ComputePeopleEatScore gives it 0.1 against 0.45 for walking distance and
/// 0.4 for nutrients, and ScoreCondition looks only 6 days ahead, squared: soup with 3 days left
/// scores 0.25 x 0.1 = 0.025, which any difference in distance swamps. Food that does not spoil at
/// all scores a flat 0.1 - more than a perishable with over about four days left.
///
/// THIS MOD, when switched on:
/// • urgency (<see cref="Urgency"/>) looks <see cref="HorizonDays"/> ahead and is linear: 1 when it
///   spoils now, 0 at the horizon and beyond; food that never spoils has none;
/// • the eat score (<see cref="EatScore"/>) weighs it 0.25, with walking distance 0.4, nutrients 0.3
///   and eating it where it lies 0.05.
/// The STARVING score (EvaluateEat.ComputePeopleStarvingScore, nutrients 0.85) is left alone: a
/// starving colonist should eat whatever fills them. Read fresh at every evaluation, so nothing is
/// saved; it changes behaviour, so it marks saves MODDED. Off by default.
/// </summary>
public static class FreshFoodMod
{
    public const string ModId = "freshfood";

    /// <summary>How far ahead spoiling counts; the studio's 6 days made a week-old stew "fresh".</summary>
    public const double HorizonDays = 20.0;

    private static ModSetting enabled;

    public static ModSetting EnabledSetting =>
        enabled ?? (enabled = ModSettings.Toggle(
            ModId, "enabled", "EAT PERISHABLES FIRST", defaultValue: false,
            toolTip: "Colonists prefer food that will spoil soon over preserved food, and will walk a " +
                     "little further for it. Food that never spoils is eaten last. A starving " +
                     "colonist still eats whatever fills them.",
            affectsSimulation: true));

    public static void RegisterSettings()
    {
        // SELECT MODS (main menu -> MODDING): who wrote it, what it does, and a picture.
        ModSettings.Describe(ModId, "Jerrybi",
            "Colonists eat what will spoil first, instead of the freshest food in the store.",
            "HUD_thumbnail_smokeOven");
        _ = EnabledSetting;
    }

    public static bool Enabled => EnabledSetting.On;

    /// <summary>
    /// How urgently a food should be eaten: 1 when it spoils now, falling linearly to 0 at
    /// <see cref="HorizonDays"/>; 0 for food that does not spoil (null). Pure, for --freshfood-selftest.
    /// </summary>
    public static double Urgency(double? daysLeft)
    {
        if (!daysLeft.HasValue)
        {
            return 0.0;
        }
        double days = daysLeft.Value < 0 ? 0 : daysLeft.Value > HorizonDays ? HorizonDays : daysLeft.Value;
        return (HorizonDays - days) / HorizonDays;
    }

    /// <summary>EvaluateEat.ScoreCondition with this mod's urgency, honouring the caller's minimum.</summary>
    public static double ScoreCondition(IKnownEntityData food, double? minimumDaysLeftUntilSpoiling)
    {
        if (!food.Condition.HasValue)
        {
            return 0.0;
        }
        if (!food.ConditionChangeSpeed.HasValue || Common.IsZero(food.ConditionChangeSpeed.Value))
        {
            return 0.0;   // does not spoil: no urgency (the studio gave it 0.1)
        }
        double days = NonLivingEntity.GetDaysLeftUntilBreakdown(food.Condition.Value, food.ConditionChangeSpeed.Value);
        if (minimumDaysLeftUntilSpoiling.HasValue && days < minimumDaysLeftUntilSpoiling.Value)
        {
            return 0.0;
        }
        return Urgency(days);
    }

    /// <summary>EvaluateEat.ComputePeopleEatScore's weights, with urgency counting.</summary>
    public static double EatScore(double travelTimeScore, double nutrientsScore, double conditionScore, double consumeDirectlyScore) =>
        0.4 * travelTimeScore + 0.3 * nutrientsScore + 0.25 * conditionScore + 0.05 * consumeDirectlyScore;
}
