using System.Collections.Generic;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Overland.Missions;
using UWGame.SimSide.Overland.Missions.Templates;

namespace UWGame.Mods;

/// <summary>
/// The trade mod, compiled OUT. See UnhiddenMod.Absent.cs for the pattern.
///
/// A trade run's sale is paid when the run starts, as the studio made it: the contract records
/// what EntityGroup.Buy charged, loading pays nothing more, and an abort leaves the studio's
/// refunds to themselves. An unpaid sale a modded build left in a save's expedition custom fields
/// is never read - and so never paid.
/// </summary>
public static class TradeMod
{
    /// <summary>Always false: the mod is not present in this build.</summary>
    public const bool Enabled = false;

    public const string ModId = "trade";

    public static void RegisterSettings()
    {
    }

    /// <summary>The studio's answer: the sale is paid in full, now.</summary>
    public static decimal DeferSalePayment(Mission mission, ContractTemplate contract, IOwner buyer, Expedition seller,
                                           List<Entity> soldItems, decimal paid) => paid;

    public static void CollectPayment(Contract contract, Entity vehicle, JobID missionJob)
    {
    }

    public static void ReturnUnpaidGoods(Mission mission)
    {
    }

    /// <summary>The time left in the studio's interval form, "1.25 days".</summary>
    public static string ArrivalIn(double daysLeft) => new UWGame.SimSide.DateAndTime.TimeDateYear(daysLeft).ToIntervalString();

    /// <summary>The arrival date and the part of the day.</summary>
    public static string ArrivalAt(UWGame.SimSide.DateAndTime.TimeDateYear eta) =>
        " on " + eta.ToString() + ", " + UWGame.SimSide.DateAndTime.GetTimeOfDayAsString(eta.TimeOfDay);
}
