using System;
using System.Globalization;

namespace UWGame.Mods;

/// <summary>
/// When a structure is worn enough to repair: one setting, the same for every structure.
///
/// THE REQUEST. Kastuk, "Repairing" (2026-10-14): "There's appear so much tasks to repair Integrity.
/// Especially of the huts, when integrity is just slightly reduced, and they fix it preferably. But
/// not fixing the Clay granary, which integrity is falling apart (until I set priority High
/// manually) ... Let there's be setting in Policy to add repairing tasks at particular integrity
/// drop (select 50-60-70-80%)."
///
/// WHY THE HUTS. The studio starts a repair when a structure's integrity - or one of its parts'
/// condition - falls below a limit (NonLivingEntity.NeedsRepair, GatherPartsWithProblems), and the
/// limit is 0.9 for a residence and 0.4 for everything else (NonLivingEntity.GetConditionLimit,
/// AIConstants.ResidenceConditionToStartRepair / OtherStructureConditionToStartRepair). A hut at 89%
/// gets a job; a granary at 41% does not.
///
/// HOW. REPAIR BELOW picks one limit for all: 50%, 60%, 70% or 80%. STUDIO keeps the studio's two.
/// GetConditionLimit asks <see cref="ConditionLimit"/>, so the repair jobs (OtherJobManager.
/// CreateRepairJobIfNeeded) and the parts a repair covers (ComputeBestRepairPackage) both follow it.
/// It changes which jobs exist, so it is a simulation setting and a save records it.
/// </summary>
public static class RepairMod
{
    public const string ModId = "repair";

    public const string Studio = "STUDIO: HOMES 90%, OTHERS 40%";

    public static readonly string[] Choices = { Studio, "50%", "60%", "70%", "80%" };

    private static ModSetting repairBelow;

    public static ModSetting RepairBelow =>
        repairBelow ?? (repairBelow = ModSettings.Choice(
            ModId, "repairBelow", "REPAIR STRUCTURES BELOW", Choices, Studio,
            toolTip: "Colonists start repairing a structure when its integrity, or one of its parts' " +
                     "condition, falls below this. The studio repairs homes below 90% and everything " +
                     "else below 40%; a percentage here is used for every structure.",
            affectsSimulation: true));

    public static void RegisterSettings()
    {
        ModSettings.Describe(ModId, "Jerrybi",
            "One repair limit for every structure, instead of homes at 90% and everything else at 40%.",
            "HUD_thumbnail_workbenchImprovised");
        ModSettings.SetCategoryLabel(ModId, "REPAIR");
        _ = RepairBelow;
    }

    /// <summary>
    /// Called by NonLivingEntity.GetConditionLimit with the studio's limit for the structure;
    /// returns the one to use.
    /// </summary>
    public static float ConditionLimit(float stock) => Limit(RepairBelow.Value, stock);

    /// <summary>The limit a REPAIR BELOW choice gives: its percentage, or <paramref name="stock"/>.</summary>
    public static float Limit(string choice, float stock)
    {
        if (choice != null && choice.EndsWith("%", StringComparison.Ordinal)
            && int.TryParse(choice.TrimEnd('%'), NumberStyles.Integer, CultureInfo.InvariantCulture, out int percent))
        {
            return percent / 100f;
        }
        return stock;
    }
}
