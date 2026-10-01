using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;

namespace UWGame.Mods;

/// <summary>
/// The hunting mod, compiled OUT. See UnhiddenMod.Absent.cs for the pattern.
///
/// Hunters chase prey as far as the studio set, and the human prey list is the studio's. The
/// policy window has no autoclaim switch, and an animal that dies in camp unowned stays unowned;
/// a switch a modded build left in a save's expedition custom fields is never read.
/// </summary>
public static class HuntingMod
{
    /// <summary>Always false: the mod is not present in this build.</summary>
    public const bool Enabled = false;

    public const string ModId = "hunting";

    public static void RegisterSettings()
    {
    }

    public static float ChaseRangeFactor() => 1f;

    public static void AdjustCreatures(List<EntityType> types)
    {
    }

    /// <summary>Always false: the weapons page shows no autoclaim switch.</summary>
    public const bool OffersAutoclaim = false;

    public static bool AutoclaimsCampKills(Expedition expedition) => false;

    public static void SetAutoclaimCampKills(Expedition expedition, bool claim)
    {
    }

    public static IOwner CampKillClaimant(Entity dying, Vector3 location) => null;
}
