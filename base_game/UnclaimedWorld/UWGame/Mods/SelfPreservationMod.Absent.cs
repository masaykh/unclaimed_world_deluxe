using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;

namespace UWGame.Mods;

/// <summary>
/// The self-preservation mod, compiled OUT. See UnhiddenMod.Absent.cs for why this is a stub
/// rather than an <c>#if</c> around EvaluateAttackJobs' single call site, and HealingMod.Absent.cs
/// for why every member answers what the studio's code answered.
/// </summary>
public static class SelfPreservationMod
{
    /// <summary>Always false: the mod is not present in this build.</summary>
    public const bool Enabled = false;

    /// <summary>The prefix the mod's settings would carry.</summary>
    public const string ModId = "selfpreservation";

    /// <summary>Registers nothing.</summary>
    public static void RegisterSettings()
    {
    }

    /// <summary>
    /// False - nobody declines. The studio's code had no such question in it, so the answer that
    /// leaves their behaviour alone is the one that says "this entity is not opting out".
    /// </summary>
    public static bool DeclinesThreat(Entity entity, AttackJob job, bool isAssetThreat)
    {
        return false;
    }
}
