using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Overland.Missions;
using UWGame.SimSide.Overland.Missions.Templates;

namespace UWGame.Mods;

/// <summary>
/// Goods the colony sells on a trade run are paid for when the barge loads them, not when the run
/// starts.
///
/// THE REQUEST. Kastuk, "Trading": "Hardcore-related: in vanilla, when I sell something at
/// trading, credits come immediately, before the barge comes to take the sold goods. Let there be
/// a mod option that the credit transaction happens only when the coming barge takes the goods."
///
/// WHAT THE STUDIO DOES. A run's sale is a BuySellAction at the colony's stop. When the run starts,
/// BuySellAction.StartMission makes the whole transaction at once if the colony is in
/// communication with the stop - "// if in comm range, transfer ownership now. otherwise we will do
/// it on arrival." - and selling at your own stop always is. EntityGroup.Buy changes the goods'
/// owner and moves the credits in one go; the barge arrives later and BuySellAction.LoadItems puts
/// the goods aboard.
///
/// WHAT THIS CHANGES, with the switch on, for a sale by the colony running the trade run:
/// • When the run starts, the goods still change owner - they are promised to the buyer and nobody
///   at home uses them, as before - but the credits are handed straight back
///   (DeferSalePayment), and the price agreed for each kind of item is written down.
/// • When the barge loads (CollectPayment, after BuySellAction.LoadItems), the buyer pays the
///   agreed price for each item that is actually aboard. An item that no longer exists - spoiled,
///   burned - is not paid for. One that exists but did not get aboard (a full barge) is given back
///   to the colony, and the buyer's demand for it is restored, as Contract.Revert does.
/// • If the run is aborted before the barge loads (Mission.Abort, ReturnUnpaidGoods), no credits
///   have moved, so every promised item is simply given back. After loading, a sale is paid and the
///   studio's own refund rules apply unchanged.
/// Buying is untouched: the request is about the colony's sales.
///
/// WHERE IT IS KEPT. The agreed prices, one entry per sale, in the selling expedition's saved
/// custom fields (Expedition.SetPropertyValue) under trade:due:&lt;contract id&gt; - no new saved
/// field and no save-format version, as ReserveMod and HuntingMod keep theirs. The entry is what
/// says "not paid yet", not the switch: a sale made with the switch on is paid on loading even if
/// the switch is turned off meanwhile, and one made with it off is never paid twice. A save with an
/// unpaid sale loaded in a build without the mod is never paid for it - that build cannot read the
/// entry - so finish such runs before moving a save to one.
///
/// DETERMINISM. Everything runs in the simulation - StartMission from the CreateMission command,
/// loading and aborting from Mission.Update - and reads nothing but simulation state, so a replay
/// pays the same amounts at the same moments. The switch affects saves and replays (it is in their
/// signature). Off by default.
/// </summary>
public static class TradeMod
{
    public const string ModId = "trade";

    private static ModSetting payOnPickup;

    public static ModSetting PayOnPickupSetting =>
        payOnPickup ?? (payOnPickup = ModSettings.Toggle(
            ModId, "payOnPickup", "SOLD GOODS ARE PAID FOR WHEN THE BARGE LOADS THEM", defaultValue: false,
            toolTip: "Hardcore. A trade run's sale pays nothing when the run starts: the buyer pays " +
                     "when the barge takes the goods aboard, and only for what it takes. Goods it " +
                     "leaves behind, or that an aborted run never collects, are yours again.",
            affectsSimulation: true));

    private static ModSetting roughEta;

    public static ModSetting RoughEtaSetting =>
        roughEta ?? (roughEta = ModSettings.Toggle(
            ModId, "roughEta", "MISSIONS SAY ROUGHLY WHEN THEY ARRIVE", defaultValue: true,
            toolTip: "The ETA in the missions list reads \"half a day\" or \"a day or two\" instead " +
                     "of \"0.48 days\", and its tooltip names the part of the day, not the clock."));

    public static void RegisterSettings()
    {
        ModSettings.SetCategoryLabel(ModId, "TRADE");
        _ = PayOnPickupSetting;
        _ = RoughEtaSetting;
    }

    // ---- the ETA in the missions list --------------------------------------------------------
    //
    // Kastuk, "Trading", 2026-10-04, on the ETA column: "It may be less exact to be more
    // roleplayish, like 'half a day' or 'a pair of hours'." MissionsPanel.UpdateRow asks here for
    // the words. Display only: nothing in the simulation reads them.

    /// <summary>How long until arrival, for the ETA column.</summary>
    public static string ArrivalIn(double daysLeft)
    {
        return RoughEtaSetting.On ? RoughInterval(daysLeft) : new DateAndTime.TimeDateYear(daysLeft).ToIntervalString();
    }

    /// <summary>When it arrives, for the ETA's tooltip, after "arrival at &lt;site&gt;".</summary>
    public static string ArrivalAt(DateAndTime.TimeDateYear eta)
    {
        string partOfDay = DateAndTime.GetTimeOfDayAsString(eta.TimeOfDay);
        if (!RoughEtaSetting.On)
        {
            return " on " + eta.ToString() + ", " + partOfDay;
        }
        return partOfDay switch
        {
            "Night" => " at night",
            "Noon" => " around noon",
            _ => " in the " + partOfDay.ToLowerInvariant()
        };
    }

    /// <summary>A game day is 24 hours. The bands are wide on purpose: it is a guess, not a timetable.</summary>
    public static string RoughInterval(double daysLeft)
    {
        double hours = daysLeft * 24.0;
        if (hours < 1.0) return "within the hour";
        if (hours < 3.0) return "a couple of hours";
        if (hours < 8.0) return "a few hours";
        if (hours < 16.0) return "half a day";
        if (hours < 30.0) return "about a day";
        if (daysLeft < 2.5) return "a day or two";
        if (daysLeft < 6.0) return "a few days";
        if (daysLeft < 10.0) return "about a week";
        return "over a week";
    }

    /// <summary>The prefix of a sale's entry in the selling expedition's custom fields.</summary>
    public const string KeyPrefix = "trade:due:";

    public static string KeyFor(ContractTemplateID contract) =>
        KeyPrefix + ((long)contract).ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Called from BuySellAction.MakeTransaction once EntityGroup.Buy has moved goods and credits.
    /// Returns what the contract records as paid: <paramref name="paid"/>, or 0 when the payment is
    /// deferred to loading - in which case the credits have been handed back here.
    /// </summary>
    public static decimal DeferSalePayment(Mission mission, ContractTemplate contract, IOwner buyer, Expedition seller,
                                           List<Entity> soldItems, decimal paid)
    {
        if (!PayOnPickupSetting.On || buyer == null || seller == null || soldItems == null || soldItems.Count == 0
            || LookUpOwners.FindByID((OwnerID)contract.SellerID) != seller || !IsColonySale(mission, buyer, seller))
        {
            return paid;
        }
        // The prices EntityGroup.Buy has just charged, item kind by item kind: the same call it makes,
        // and Buy changes demand, not prices (TradeManager.Buy lowers AmountToBuy only).
        Dictionary<string, decimal> agreed = new Dictionary<string, decimal>(StringComparer.Ordinal);
        foreach (EntityType type in soldItems.Select(e => e.EntityType).Distinct())
        {
            agreed[type.KeyName] = BuySellActionTemplate.GetTradePrice(type, buyer.OwnedEntities, seller.OwnedEntities) ?? 0m;
        }
        // Only a sale whose prices add up to what was charged is deferred, so loading can never pay
        // a different sum from the one the studio's sale would have.
        if (AmountDue(agreed, soldItems.Select(e => (e.EntityType.KeyName, Pickup.Taken))) != paid)
        {
            return paid;
        }
        EntityGroup.MakeTradeCreditsTransaction(seller.OwnedEntities.Parent, buyer.OwnedEntities.Parent, paid);
        SetAgreedPrices(seller, contract.ID, agreed);
        return 0m;
    }

    /// <summary>
    /// A sale by the colony that runs the trade run, to someone else. Other groups' runs, and the
    /// colony's purchases, are left as the studio made them.
    /// </summary>
    private static bool IsColonySale(Mission mission, IOwner buyer, Expedition seller)
    {
        Allegiance sellerAllegiance = seller.Allegiance;
        return sellerAllegiance != null
            && sellerAllegiance.AllegianceType == AllegianceType.Player
            && (long)sellerAllegiance.ID == mission.MissionTemplate.Allegiance
            && buyer.Allegiance != sellerAllegiance;
    }

    /// <summary>
    /// Called from BuySellAction.LoadItems once the goods have been put aboard. If the sale is
    /// unpaid, the buyer pays the agreed price for what is aboard; what is not aboard but still
    /// exists goes back to the colony.
    /// </summary>
    public static void CollectPayment(Contract contract, Entity vehicle, JobID missionJob)
    {
        if (contract == null || !TryGetUnpaidSale(contract, out Expedition seller, out IOwner buyer, out Dictionary<string, decimal> agreed))
        {
            return;
        }
        List<EntityID> taken = new List<EntityID>();
        List<(string, Pickup)> goods = new List<(string, Pickup)>();
        foreach (EntityID id in contract.TransferredEntities ?? new List<EntityID>())
        {
            Entity entity = Entity.FindByID(id);
            if (entity == null)
            {
                continue; // gone: nothing to pay for and nothing to give back
            }
            if (vehicle != null && entity.ContainedBy == vehicle.ID)
            {
                taken.Add(id);
                goods.Add((entity.EntityType.KeyName, Pickup.Taken));
            }
            else
            {
                GiveBack(entity, seller, buyer, missionJob);
                goods.Add((entity.EntityType.KeyName, Pickup.LeftBehind));
            }
        }
        decimal due = AmountDue(agreed, goods);
        EntityGroup.MakeTradeCreditsTransaction(buyer.OwnedEntities.Parent, seller.OwnedEntities.Parent, due);
        contract.PaidAmount = due;
        contract.TransferredEntities = taken;
        SetAgreedPrices(seller, contract.ContractTemplate.ID, null);
    }

    /// <summary>
    /// Called from Mission.Abort, before the studio's refunds: every sale still unpaid gives all its
    /// goods back, and records that nothing changed hands, so RefundOrders finds nothing to revert.
    /// </summary>
    public static void ReturnUnpaidGoods(Mission mission)
    {
        foreach (Contract contract in mission.Contracts)
        {
            if (!TryGetUnpaidSale(contract, out Expedition seller, out IOwner buyer, out _))
            {
                continue;
            }
            foreach (EntityID id in contract.TransferredEntities ?? new List<EntityID>())
            {
                Entity entity = Entity.FindByID(id);
                if (entity != null)
                {
                    GiveBack(entity, seller, buyer, mission.MissionJob);
                }
            }
            contract.TransferredEntities = new List<EntityID>();
            SetAgreedPrices(seller, contract.ContractTemplate.ID, null);
        }
    }

    /// <summary>The colony owns the item again, the buyer wants it again, and no run claims it.</summary>
    private static void GiveBack(Entity entity, Expedition seller, IOwner buyer, JobID missionJob)
    {
        entity.ChangeOwnership(seller);
        buyer.OwnedEntities.TradeManager?.Buy(entity.EntityType, -1);
        if (entity.AssignedToJob == missionJob)
        {
            entity.AssignedToJob = null;
        }
    }

    private static bool TryGetUnpaidSale(Contract contract, out Expedition seller, out IOwner buyer, out Dictionary<string, decimal> agreed)
    {
        seller = null;
        buyer = null;
        agreed = null;
        if (contract.ContractTemplate == null)
        {
            return false;
        }
        seller = LookUpOwners.FindByID((OwnerID)contract.ContractTemplate.SellerID) as Expedition;
        buyer = LookUpOwners.FindByID((OwnerID)contract.ContractTemplate.BuyerID);
        agreed = GetAgreedPrices(seller, contract.ContractTemplate.ID);
        return agreed != null && buyer != null;
    }

    /// <summary>The agreed prices of an unpaid sale, by item key, or null if the sale is not unpaid.</summary>
    public static Dictionary<string, decimal> GetAgreedPrices(Expedition seller, ContractTemplateID contract)
    {
        string text = seller?.GetPropertyValue(KeyFor(contract), null, null)?.StringResult;
        return text == null ? null : DecodePrices(text);
    }

    /// <summary>Writes (or, with null, clears) an unpaid sale's agreed prices.</summary>
    public static void SetAgreedPrices(Expedition seller, ContractTemplateID contract, IReadOnlyDictionary<string, decimal> agreed)
    {
        string key = KeyFor(contract);
        seller?.SetPropertyValue(key, agreed == null
            ? (PropertyResult?)null
            : new PropertyResult { PropertyKeyName = key, StringResult = EncodePrices(agreed) });
    }

    /// <summary>"key=price;key=price", ordered by key, prices invariant and exact.</summary>
    public static string EncodePrices(IReadOnlyDictionary<string, decimal> prices) =>
        string.Join(";", prices.OrderBy(p => p.Key, StringComparer.Ordinal)
                               .Select(p => p.Key + "=" + p.Value.ToString(CultureInfo.InvariantCulture)));

    public static Dictionary<string, decimal> DecodePrices(string text)
    {
        Dictionary<string, decimal> prices = new Dictionary<string, decimal>(StringComparer.Ordinal);
        foreach (string part in text.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            int at = part.LastIndexOf('=');
            if (at > 0 && decimal.TryParse(part.Substring(at + 1), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal price))
            {
                prices[part.Substring(0, at)] = price;
            }
        }
        return prices;
    }

    /// <summary>What became of one sold item by the time the barge loaded.</summary>
    public enum Pickup
    {
        Taken,
        LeftBehind
    }

    /// <summary>The agreed price of every item taken aboard; nothing for what was left behind.</summary>
    public static decimal AmountDue(IReadOnlyDictionary<string, decimal> agreed, IEnumerable<(string TypeKey, Pickup State)> goods)
    {
        decimal due = 0m;
        foreach ((string typeKey, Pickup state) in goods)
        {
            if (state == Pickup.Taken && agreed.TryGetValue(typeKey, out decimal price))
            {
                due += price;
            }
        }
        return due;
    }
}
