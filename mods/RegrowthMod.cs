using System;
using UWGame.SimSide;
using UWGame.SimSide.Resources;

namespace UWGame.Mods;

/// <summary>
/// Wood grows back more slowly where the woods around it have been cut down.
///
/// THE REQUEST. Kastuk, "Nature can be more natural", point 7: overharvesting wood should reduce
/// its respawn - "if there's no remained resource anywhere in resource zone, its -50% of the
/// spawnrate. If there's like 20-39% of resources remained, it's -15% of respawn rate. 40% of
/// resources is normal spawn rate. Must be smooth curve modifier." And, on how a zone is measured
/// when a tile respawns one unit at a time: "make a calc in moment of Replenishing day to check
/// area around each resource tile for other tiles in radius of 5 tiles, search for same resource
/// tiles, list all that collected tiles into a gathering zone and then calc overall remained
/// quantity of resources in that zone to determine respawn rate in them. If only one item is
/// respawning in tile, just make respawn decrease over zone and if there's -50%, then half of that
/// tiles in zone will not respawn that resource." Wood only for now: "firewood, sticks and
/// branches".
///
/// HOW. On the day a tile replenishes (ResourceReplenish.ReplenishResourceItems), every tile of
/// the same resource within 5 tiles of it is its zone; what the zone holds now over the most it
/// has ever held is how much is left. The amount the tile would add is multiplied by
/// <see cref="Curve"/> - x0.5 with nothing left, easing smoothly to x1 at 40% - and the fraction
/// that does not make a whole item is rolled for, on the game's own seeded generator, so a tile
/// that respawns one item has exactly that chance of doing it and a replay stays deterministic.
///
/// Off by default: it changes the wood economy of a running game. Nothing new is saved - the
/// zone is measured each time from what the tiles hold.
/// </summary>
public static class RegrowthMod
{
    public const string ModId = "regrowth";

    private static ModSetting woodOverharvest;

    public static ModSetting WoodOverharvest =>
        woodOverharvest ?? (woodOverharvest = ModSettings.Toggle(
            ModId, "woodOverharvest", "CUT-DOWN WOODS REGROW SLOWLY", defaultValue: false,
            toolTip: "Firewood, sticks and spoak branches respawn more slowly where the woods " +
                     "within 5 tiles have been gathered down: half speed with nothing left, normal " +
                     "again from 40% left.",
            affectsSimulation: true));

    public static void RegisterSettings()
    {
        _ = WoodOverharvest;
    }

    /// <summary>The resources this applies to.</summary>
    public static readonly string[] WoodKeys = { "firewood", "crop:sticks", "crop:spoakBranches" };

    /// <summary>The zone's radius in tiles.</summary>
    public const int ZoneRadius = 5;

    /// <summary>
    /// Called by ResourceReplenish.ReplenishResourceItems with the number of items the tile is
    /// about to gain; returns the number it gains instead.
    /// </summary>
    public static int AdjustReplenish(ResourceContainer tile, int amount)
    {
        if (!WoodOverharvest.On || amount <= 0 || tile?.ResourceType == null
            || Array.IndexOf(WoodKeys, tile.ResourceType.KeyName) < 0)
        {
            return amount;
        }
        float scaled = amount * Curve(ZoneFraction(tile));
        int whole = (int)Math.Floor(scaled);
        float rest = scaled - whole;
        if (rest > 0f && The.Sim.GameplayRandomGenerator.RandomBetween(0f, 1f) < rest)
        {
            whole++;
        }
        return whole;
    }

    /// <summary>
    /// The respawn multiplier for a zone with <paramref name="fractionLeft"/> of its wood: 0.5 at
    /// nothing, 1 from 0.4 up, a smoothstep between - so 20-39% left averages about x0.85, the
    /// "-15%" asked for, with no step anywhere.
    /// </summary>
    public static float Curve(float fractionLeft)
    {
        float t = Math.Max(0f, Math.Min(1f, fractionLeft / 0.4f));
        float smooth = t * t * (3f - 2f * t);
        return 0.5f + 0.5f * smooth;
    }

    /// <summary>What the tile's zone holds now over the most it has ever held; 1 if unknown.</summary>
    private static float ZoneFraction(ResourceContainer tile)
    {
        if (The.Sim?.PlaySite?.Resources == null
            || !The.Sim.PlaySite.Resources.TryGetValue(tile.ResourceType, out var sameResource))
        {
            return 1f;
        }
        var centre = tile.MapPosition;
        long now = 0, most = 0;
        for (int i = 0; i < sameResource.Count; i++)
        {
            ResourceContainer other = sameResource[i];
            if (other == null)
            {
                continue;
            }
            var at = other.MapPosition;
            if (Math.Abs(at.X - centre.X) > ZoneRadius || Math.Abs(at.Y - centre.Y) > ZoneRadius)
            {
                continue;
            }
            int held = other.NoOfHarvestableItems;
            now += held;
            most += Math.Max(held, other.Replenish?.MaxItemsEverSet ?? held);
        }
        return most > 0 ? (float)now / most : 1f;
    }
}
