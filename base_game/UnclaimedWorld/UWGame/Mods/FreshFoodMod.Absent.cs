using UWGame.SimSide.AI;

namespace UWGame.Mods;

/// <summary>
/// The eat-perishables-first mod, compiled OUT. See UnhiddenMod.Absent.cs for the pattern.
///
/// EvaluateEat keeps the studio's freshness score and weights.
/// </summary>
public static class FreshFoodMod
{
    /// <summary>Always false: the mod is not present in this build.</summary>
    public const bool Enabled = false;

    public const string ModId = "freshfood";

    public static void RegisterSettings()
    {
    }

    public static double Urgency(double? daysLeft) => 0.0;

    public static double ScoreCondition(IKnownEntityData food, double? minimumDaysLeftUntilSpoiling) => 0.0;

    public static double EatScore(double travelTimeScore, double nutrientsScore, double conditionScore, double consumeDirectlyScore) => 0.0;
}
