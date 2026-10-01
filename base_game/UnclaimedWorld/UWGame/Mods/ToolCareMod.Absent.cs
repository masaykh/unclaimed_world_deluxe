using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;

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

    public static bool IsLookedAfter(Entity carrier, EntityType type) => false;

    public static bool DropsWhereItStands(Entity carrier) => true;

    public static void NoteTakenFrom(Entity carrier, Entity item)
    {
    }

    public static bool PlaceToTakeItTo(Entity carrier, Entity item, out Vector3? location, out StorageTarget? storage, out string where)
    {
        location = null;
        storage = null;
        where = null;
        return false;
    }

    public static void LogTakenBack(Entity carrier, Entity item, string where)
    {
    }
}
