using System;
using System.Collections.Generic;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;

namespace UWGame.Mods;

/// <summary>
/// A reserve per item: an amount that colonists will not eat and workshops will not use up.
///
/// THE REQUEST. Kastuk, "reserve currently needed amount of food": "add slider to reserve
/// particular amount of materials\ingredients\prepared food (amount, which become invisible for
/// production tasks and eating actions)". The purpose, in his words: "Preserve smoked fish and
/// alcohol for trading. For now only way to reserve is to hide it in Trade stockpile at Port" -
/// which is an unhandy place when part of it is wanted back for local use. And for the control: no
/// input field, no second row of sliders - "another switcher near every producible item in
/// Production list, like standing order's Padlock (button can be just a big letter R)".
///
/// WHAT IT IS. A count, checked when a decision is made - not a set of reserved instances:
/// • Eating (EvaluateEat.GetAllFoodItems): a food type whose stock is at or below its reserve is
///   not offered - unless the colonist is starving. A reserve nobody may touch while starving
///   would be a death sentence.
/// • Production (InventoryPanel.HasInputForProcess, Structure.OwnerCanSupply): the reserve is
///   subtracted from the input's stock, the same way the studio already subtracts items promised
///   to other orders. That one check feeds both the slider's maximum and the JobManager's
///   decision to create a job, so a job that would dig into the reserve is never made.
/// • Standing orders (JobManager.GetAmountToProduce, EntityGroup.StandingOrderJobIsNeeded): the
///   reserve does not count as stock. "Keep 10" with 20 reserved means 30 on the shelf - ten to use
///   and twenty to sell.
/// • Trade is untouched: selling the reserve is what it is for.
/// Two colonists deciding in the same tick can still dip the stock one under - the price of a
/// count over instance bookkeeping, and the same imprecision the studio's own orders accept.
///
/// WHERE IT IS KEPT. In the expedition's saved custom fields (Expedition.SetPropertyValue), one
/// key per item. No new saved field, no save-format version: a save made with reserves loads in a
/// build without the mod, which simply never reads those keys.
///
/// THE CONTROL. An "R" beside each item's slider in the Production list
/// (ProductionOrderControl). Pressed, the slider edits the reserve instead of the order, tinted
/// so it cannot be mistaken for one. Released, the slider is the order again, and the R stays lit
/// while a reserve is set. The change goes through a SetReserve command, so replays carry it.
/// Off by default.
/// </summary>
public static class ReserveMod
{
    public const string ModId = "reserve";

    private static ModSetting enabled;

    public static ModSetting EnabledSetting =>
        enabled ?? (enabled = ModSettings.Toggle(
            ModId, "enabled", "RESERVE SLIDER IN THE PRODUCTION LIST", defaultValue: false,
            toolTip: "Adds an R button beside each item in the Production list. Pressed, the slider " +
                     "sets an amount colonists will not eat and workshops will not use - kept for " +
                     "trading. Starving colonists may still eat it. Reopen the Production list after " +
                     "switching this.",
            affectsSimulation: true));

    public static void RegisterSettings()
    {
        ModSettings.SetCategoryLabel(ModId, "RESERVES");
        _ = EnabledSetting;
    }

    /// <summary>Whether reserves are read at all.</summary>
    public static bool Enabled => EnabledSetting.On;

    /// <summary>The prefix of an item's key in the expedition's custom fields.</summary>
    public const string KeyPrefix = "reserve:";

    /// <summary>The largest reserve the slider offers before it grows with the stock.</summary>
    public const int MinimumSliderMax = 20;

    private static readonly Dictionary<EntityType, string> keys = new Dictionary<EntityType, string>();

    public static string KeyFor(EntityType type)
    {
        if (!keys.TryGetValue(type, out string key))
        {
            key = KeyPrefix + type.KeyName;
            keys[type] = key;
        }
        return key;
    }

    /// <summary>The reserve set for this item, or 0.</summary>
    public static int Reserved(Expedition expedition, EntityType type)
    {
        if (!Enabled || expedition == null || type == null)
        {
            return 0;
        }
        PropertyResult? value = expedition.GetPropertyValue(KeyFor(type), null, null);
        return value?.NumberResult is float n && n > 0f ? (int)n : 0;
    }

    public static int Reserved(EntityGroup owner, EntityType type) => Reserved(owner?.GetExpedition(), type);

    /// <summary>Sets (or, with 0, clears) the reserve. Called from the SetReserve command only.</summary>
    public static void SetReserve(Expedition expedition, EntityType type, int amount)
    {
        if (expedition == null || type == null)
        {
            return;
        }
        expedition.SetPropertyValue(KeyFor(type), amount > 0
            ? new PropertyResult { PropertyKeyName = KeyFor(type), NumberResult = amount }
            : (PropertyResult?)null);
    }

    /// <summary>
    /// Whether this food type is held back from a colonist choosing a meal: its stock is at or
    /// below its reserve and the colonist is not starving.
    /// </summary>
    public static bool HoldsBackFood(EntityGroup foodOwner, EntityType type, bool starving)
    {
        if (starving)
        {
            return false;
        }
        int reserve = Reserved(foodOwner, type);
        return reserve > 0 && foodOwner.CountAvailableItems(type) <= reserve;
    }

    /// <summary>The stock a standing order sees: the reserve is not there to be used.</summary>
    public static int StockAfterReserve(EntityGroup owner, EntityType type, int stock)
    {
        int reserve = Reserved(owner, type);
        return reserve > 0 ? Math.Max(0, stock - reserve) : stock;
    }

    /// <summary>The top of the reserve slider: at least <see cref="MinimumSliderMax"/>, else the next ten above what is there.</summary>
    public static int SliderMax(int stock, int reserve)
    {
        int top = Math.Max(stock, reserve);
        return top < MinimumSliderMax ? MinimumSliderMax : (top / 10 + 1) * 10;
    }
}
