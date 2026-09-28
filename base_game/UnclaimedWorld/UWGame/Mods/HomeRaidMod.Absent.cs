using UWGame.SimSide;

namespace UWGame.Mods;

/// <summary>
/// The home raid mod, compiled OUT. See UnhiddenMod.Absent.cs for the pattern.
///
/// No animal ever targets a building: sleepers and stores are as safe as the studio made them.
/// </summary>
public static class HomeRaidMod
{
    /// <summary>Always false: the mod is not present in this build.</summary>
    public const bool Enabled = false;

    public const string ModId = "homeraid";

    public static readonly string[] RaiderKeys = new string[0];

    public static void RegisterSettings()
    {
    }

    public static void Update(Sim sim)
    {
    }

    public static float ParseSeconds(string value) => 60f;
}
