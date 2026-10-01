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
/// THE FORECAST. With the switch on, the Gather window's Regrowth column reads "current/max" a
/// year: max with every place at full rate (the studio's figure), current with the slowdown each
/// place would get today (<see cref="ForecastRegrowth"/>), so a cut-down wood shows what it costs
/// before a replenish day comes round.
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
    /// Called by MapArea.AddToResourceSum, once per tile, with the studio's yearly regrowth for it
    /// (ResourceContainer.GetCurrentRegrowth); returns what that tile is expected to regrow with
    /// its zone as cut down as it is now - the same x0.5..x1 that AdjustReplenish applies on the
    /// day it replenishes. The Gather window's Regrowth column (GatherResourcesWindow.UpdateRow) and
    /// the zone side panel add these up. Kastuk: "calculate forecast regrowth right at zone window
    /// near resources in Gathering (max regrowth/current regrowth). Current regrowth will show
    /// expected regrowth value with current status of harvested tiles." Reads only; draws no random
    /// number, so replays are unaffected.
    /// </summary>
    public static float ForecastRegrowth(ResourceContainer tile, float yearlyRegrowth)
    {
        if (!WoodOverharvest.On || yearlyRegrowth <= 0f || tile?.ResourceType == null
            || Array.IndexOf(WoodKeys, tile.ResourceType.KeyName) < 0)
        {
            return yearlyRegrowth;
        }
        return yearlyRegrowth * Curve(ZoneFraction(tile));
    }

    /// <summary>
    /// The Regrowth column's text with the mod on: "current/max" a year, e.g. "12/20" - current from
    /// <see cref="ForecastRegrowth"/>, max with every tile at full rate. Null leaves the studio's
    /// "+current" (mod off) or "max." (the zone has all it can hold). One decimal below 10, whole
    /// numbers above, because the column is narrow: it starts at 290 in a 364-wide window.
    /// </summary>
    public static string RegrowthLabel(float current, float maximum, bool maximumReached)
    {
        if (!WoodOverharvest.On || maximumReached)
        {
            return null;
        }
        return Amount(current) + "/" + Amount(maximum);
    }

    /// <summary>The line the Regrowth tooltip gains with the mod on; null with it off.</summary>
    public static string RegrowthToolTipNote(ResourceType type)
    {
        if (!WoodOverharvest.On || type == null)
        {
            return null;
        }
        return Array.IndexOf(WoodKeys, type.KeyName) >= 0
            ? "Shown as current/max. Current counts the woods within 5 tiles of each place: where they are cut down, it regrows down to half as fast (CUT-DOWN WOODS REGROW SLOWLY)."
            : "Shown as current/max. This resource is not slowed by cutting it down.";
    }

    private static string Amount(float value)
    {
        return value < 10f ? value.ToString("0.#") : value.ToString("0");
    }

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
