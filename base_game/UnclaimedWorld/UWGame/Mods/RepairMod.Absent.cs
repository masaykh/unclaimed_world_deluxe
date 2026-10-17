namespace UWGame.Mods;

/// <summary>
/// The repair mod, compiled OUT. See PeatMod.Absent.cs for the pattern: the studio's limits, homes
/// at 90% and everything else at 40%.
/// </summary>
public static class RepairMod
{
    public const string ModId = "repair";

    public static void RegisterSettings()
    {
    }

    public static float ConditionLimit(float stock) => stock;

    public static float Limit(string choice, float stock) => stock;
}
