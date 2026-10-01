using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;

namespace UWGame.Mods;

/// <summary>
/// The tool care mod, compiled OUT. See UnhiddenMod.Absent.cs for the pattern.
///
/// A colonist whose job needs its hands drops whatever the job doesn't use, where it stands, as the
/// studio made it.
/// </summary>
public static class ToolCareMod
{
    /// <summary>Always false: the mod is not present in this build.</summary>
    public const bool Enabled = false;

    public const string ModId = "toolcare";

    public static void RegisterSettings()
    {
    }

    public static bool IsCarefulCarrier(Entity carrier) => false;

    public static bool IsProtected(EntityType type) => false;

    public static bool KeptWhenMakingRoom(Entity carrier, EntityType type) => false;

    public static bool IsOutOfCamp(Entity carrier) => false;

    public static Vector3? DropSpotInCamp(Entity carrier) => null;

    public static void LogCarriedHome(Entity carrier, Entity item)
    {
    }
}
