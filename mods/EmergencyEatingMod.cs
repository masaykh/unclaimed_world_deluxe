using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Items;
using UWGame.SimSide.Processes;
using UWGame.SimSide.SimEffects;

namespace UWGame.Mods;

/// <summary>
/// A starving colonist with no food left eats raw ingredients - and pays for it.
///
/// THE REQUEST. Kastuk, "SimEffects and Emergency eating", 2026-10-17:
/// 1. "Let starving colonists eat raw unprepared ingredients (in case there's no more prepared
///    food), in gradation of priority (from higher to lesser): Crystal berries, glassy creeper
///    pods, common oil tubers, raw fish, raw meat." 1.1 "Reduce Comfort relative to priority from
///    (5 for berries to 60 for meat)."
/// 2. "Give SimEffect of 'Bad stomach' after eating with gradual raising of modifier relative to
///    gradation of priority and chance to catch it (from 5% for berries to 60% for meat). ...
///    Bad Stomach will reduce movement speed by 0.2*modifier, raise processes needful time by
///    0.3*mod, reduce Comfort by 0.2*mod, make cap of energy by 0.6 of original max."
///    The 60s were lowered, and the rest added, by KASTUK'S ANSWER below.
///
/// WHY A COLONIST NEVER DID. A human eats only what its FoodItemTagsThatCanBeConsumed name -
/// cookedMeat, edibleVegi, coffee, alcoholicBeverage, with the studio's "TODO: add more tags"
/// (CreatureLoader) - because BiologicalType.InitConsumeProcesses builds a consume process for
/// those tags only, and BiologicalEntity.IsEatable, the expedition's Food list (EntityGroup.
/// UpdateFoodItems) and EvaluateEat all go by those processes. Crystal berries, glassy creeper
/// pods and common oil tubers are "inedibleVegi"; fish and meat "rawMeat" or "smallRawMeat".
/// Adding the tags to the human would make raw stock count as the colony's food everywhere (the
/// FOOD rating, FoodStatisticsForAllegiance, the nutrient filters) and be eaten whenever anyone
/// is a little hungry, so this mod does not.
///
/// HOW.
/// - Processes. BiologicalType.InitConsumeProcesses calls <see cref="AddConsumeProcesses"/> for
///   each person type: one consume process per raw ingredient, built by the studio's own code
///   (BiologicalType.CreateConsumeProcess, the body of its InitConsumeProcess) under the key
///   "&lt;person&gt;_rawConsume_&lt;item&gt;", and kept HERE, not in ConsumeProcesses. Registered
///   whether or not the mod is on: a save made mid-meal names the process.
/// - Choosing. EvaluateEat.GetAllFoodItems calls <see cref="AddRawFood"/>: a colonist near
///   starving (an essential food need at or below <see cref="NearStarvingLevel"/>) also lists the
///   expedition's raw ingredients. EvaluateEat.ScoreFoodItem scores one as eaten directly, and
///   only at the studio's starving score (AIConstants.NutrientsScoreToTriggerStarvedEating, 0.9 -
///   the studio's "if starving -> eat anything, but prefer cooked meals"); below it, 0. After the
///   scores are sorted, <see cref="OrderRawFood"/> drops every raw ingredient while any food scores
///   high enough to be chosen, and otherwise puts them in Kastuk's order: berries, pods, tubers,
///   fish, meat - the nearest of the best tier first.
/// - Eating. GoalEat.AddConsumeGoals takes the mod's process (<see cref="ConsumeProcessFor"/>).
///   Food.ConsumeBy asks <see cref="RawNutrientFactor"/> per nutrient: raw land meat feeds a third
///   of its food energy and protein, raw fish three quarters of its protein; the rest in full.
/// - After. GoalEat.ConsumeStomachContents calls <see cref="OnEaten"/> for each item BEFORE crediting
///   it (Food.ConsumeBy), then credits it at <see cref="NutritionFactor"/>. OnEaten starts the "Raw
///   food" effect for the tier (comfort -5, -12, -20, -30, -40% for 0.4, 0.65, 0.9, 1.2, 1.5 days)
///   and adds the tier's bad stomach chance (5, 10, 20, 30, 40%, times the CHANCE setting) to the
///   colonist's running chance (<see cref="AddRawMeal"/>), then rolls the running chance on the
///   game's own seeded generator (The.Sim.GameplayRandomGenerator, so a replay rolls the same) for
///   "Bad stomach": modifier 0.2, 0.4, 0.6, 0.8, 1.0 by the worst tier eaten in the running day,
///   lasting as long as that tier's "Raw food". Both are the studio's SimEffects
///   (EffectProfileType/NumberEffectType, as alcohol's comfort is), so they show in the colonist's
///   effect panel, are saved with the colonist and expire by themselves. Neither stacks: eating
///   again restarts the timer, at the worse of the running tier and the new one
///   (<see cref="StartWorst"/>).
/// - The running chance. CustomFields "rawFood:since", "rawFood:chance", "rawFood:worst" on the
///   colonist, saved with it. The first raw meal opens a window of one game day
///   (<see cref="ChanceWindowInDays"/>, on the unpaused clock the effects expire by); each raw meal
///   in it adds its chance, at most 100%; the first raw meal after it opens a new window at 0.
///   Chosen over "until midnight" so a meal at dusk and one after midnight still add up.
/// - Half nutrition. While a colonist has a bad stomach every food - raw or cooked - feeds half of
///   all its nutrients (<see cref="NutritionFactor"/>). The roll comes before the meal is credited,
///   so "the last one" - the meal that upset the stomach - already feeds half: nothing to take back.
/// - Bad stomach. The comfort part is the effect's own (-0.2 x modifier). The studio's effects can
///   not touch speed, work or energy (AffectsNumbers has none of them), so while a colonist has the
///   effect: Locomotor.CalculateSpeed x <see cref="SpeedFactor"/> (1 - 0.2m), ProcessJob.
///   CalculateWorkDurationInDays x <see cref="WorkTimeFactor"/> (1 + 0.3m), and BiologicalEntity.
///   EnergyLevel at most <see cref="EnergyCap"/> (<see cref="CapEnergy"/>). They follow the effect,
///   not the switch: what the panel shows is what applies, until it expires.
///
/// WHICH FISH. From the data: a raw meat some process needing the fishing skill catches (alabaster
/// ray, streak fin, carbon tail, daggermouth, imp eel, minnows), plus the two shellfish gathered by
/// hand on the shore (torux, clamwich - <see cref="ShellfishKeys"/>; the data does not mark them as
/// aquatic: "grasping" skill, found "smallAboveGround"). Every other rawMeat or smallRawMeat -
/// thunder chicken meat and guts, the bugs and critters, ursinix - is land meat, the worst tier.
///
/// KASTUK'S ANSWER, 2026-10-17: "I guess -60 comfort over day with already zero Food need will
/// lead to warrant escaping the location. Let it be max -40 comfort at bad stomach with raw meat,
/// for 1.5 days max. Then -5 comfort and 2/5 days for berries. Repeating of eating will not stack,
/// but reset the timer with highest of eaten bad food. Bad stomach effect will need a chance to be
/// triggered, that chances may stack during day. So 1 raw berry give just 5% of sickness chance,
/// then raw meat will give whole 40% chance at once. Taking Bad Stomach effect will halving
/// nutrition of last one and further eaten food (I'd like to introduce vomiting and such, but it's
/// quite out of style for now). Raw meat must have reduced intake of energy and protein for humans
/// for sure (like 1/3 of original value), but vitamins must stay same (actually even higher than in
/// roasted/smoked/dried meat). Raw fish then will give 3/4 of original protein. Any raw meat, which
/// including fish by tag must be included, but meat of land animals must be most dangerous."
/// Read as: the -40%/1.5 days and -5%/0.4 days are the "Raw food" comfort of meat and berries, the
/// tiers between evenly spaced; the bad stomach's own comfort stays 0.2 x modifier; "vitamins" are
/// the game's micronutrients; the window of a day is ours.
///
/// NOT CHANGED: EvaluateEat scores a raw ingredient by what it holds, not by what a colonist gets
/// from it raw - the score only decides whether the starving eat at all, and the order is the tiers.
///
/// ASSUMED, not answered by Kastuk: "cap of energy by 0.6" as at most 0.6 energy, not 0.6 of the
/// current. Off by default.
/// </summary>
public static class EmergencyEatingMod
{
    public const string ModId = "emergencyeating";

    private static ModSetting enabled;
    private static ModSetting badStomach;
    private static ModSetting chance;

    public static ModSetting EnabledSetting =>
        enabled ?? (enabled = ModSettings.Toggle(
            ModId, "enabled", "RAW FOOD WHEN STARVING", defaultValue: false,
            toolTip: "A starving colonist with no food left eats raw ingredients: crystal berries first, then " +
                     "glassy creeper pods, common oil tubers, raw fish and raw meat. Comfort drops, from 5% for " +
                     "about 10 hours (berries) to 40% for a day and a half (meat); eating again restarts it at " +
                     "the worse. Raw meat feeds a third of its energy and protein, raw fish 3/4 of its protein.",
            affectsSimulation: true));

    public static ModSetting BadStomachSetting =>
        badStomach ?? (badStomach = ModSettings.Toggle(
            ModId, "badStomach", "RAW FOOD CAN UPSET THE STOMACH", defaultValue: true,
            toolTip: "After raw food a colonist may get a bad stomach: slower walking and work, less comfort, " +
                     "energy at most 60%, and all food feeds half. Each raw meal adds its chance (5% berries ... " +
                     "40% meat) to the day's; how bad and how long rise from berries to meat.",
            affectsSimulation: true));

    public static ModSetting ChanceSetting =>
        chance ?? (chance = ModSettings.Choice(
            ModId, "chance", "CHANCE OF A BAD STOMACH (% OF 5% BERRIES ... 40% MEAT)", new[] { "50", "100", "150" }, "100",
            affectsSimulation: true));

    public static void RegisterSettings()
    {
        // SELECT MODS (main menu -> MODDING): who wrote it, what it does, and a picture.
        ModSettings.Describe(ModId, "Jerrybi",
            "A starving colonist with no food left eats raw ingredients - berries before tubers, fish before meat - at a cost in comfort, and maybe a bad stomach.",
            "HUD_thumbnail_meatDryingRack");
        ModSettings.SetCategoryLabel(ModId, "EMERGENCY EATING");
        _ = EnabledSetting;
        _ = BadStomachSetting;
        _ = ChanceSetting;
    }

    public static bool Enabled => EnabledSetting.On;

    /// <summary>Kastuk's order, best first. None: not a raw ingredient a colonist may eat.</summary>
    public enum Tier { None, CrystalBerries, GlassyCreeperPods, CommonOilTubers, RawFish, RawMeat }

    /// <summary>
    /// Raw meats from the water that no fishing process catches: shellfish gathered by hand on the
    /// shore. The rest of the fish come from the data (<see cref="IsFish"/>).
    /// </summary>
    public static readonly string[] ShellfishKeys = { "item:torux", "item:clamwich" };

    /// <summary>The skill of the processes that catch fish (fishAlabasterRay, catchingStreakFin, fishMinnows, gatherImpEel...).</summary>
    public const string FishingSkill = "fishing";

    private static readonly string[] RawMeatTags = { "rawMeat", "smallRawMeat" };

    /// <summary>Comfort lost after eating each tier (Kastuk: "max -40 comfort ... with raw meat", "-5 comfort ... for berries").</summary>
    public static readonly float[] ComfortCost = { 0f, 0.05f, 0.12f, 0.20f, 0.30f, 0.40f };

    /// <summary>
    /// How long each tier's "Raw food" and "Bad stomach" last (Kastuk: "1.5 days max", "2/5 days for berries").
    /// </summary>
    public static readonly double[] DurationInDays = { 0.0, 0.4, 0.65, 0.9, 1.2, 1.5 };

    /// <summary>The bad stomach chance each tier adds to the day's (Kastuk: "1 raw berry give just 5%", "raw meat ... whole 40%").</summary>
    public static readonly float[] StomachChance = { 0f, 0.05f, 0.10f, 0.20f, 0.30f, 0.40f };

    /// <summary>How bad a stomach each tier gives (Kastuk's "modifier", rising with the tier).</summary>
    public static readonly float[] StomachModifier = { 0f, 0.2f, 0.4f, 0.6f, 0.8f, 1.0f };

    public const float SpeedPerModifier = 0.2f;
    public const float WorkTimePerModifier = 0.3f;
    public const float ComfortPerModifier = 0.2f;

    /// <summary>The most energy a colonist with a bad stomach has (Kastuk: "cap of energy by 0.6").</summary>
    public const float EnergyCap = 0.6f;

    /// <summary>How long the running bad stomach chance adds up, from the first raw meal (see the summary).</summary>
    public const double ChanceWindowInDays = 1.0;

    /// <summary>The share of every food's nutrients a colonist with a bad stomach gets (Kastuk: "halving nutrition").</summary>
    public const float SickNutritionFactor = 0.5f;

    /// <summary>Raw land meat's food energy and protein to a colonist (Kastuk: "like 1/3 of original value").</summary>
    public const float RawMeatEnergyProteinFactor = 1f / 3f;

    /// <summary>Raw fish's protein to a colonist (Kastuk: "3/4 of original protein").</summary>
    public const float RawFishProteinFactor = 0.75f;

    public const string EnergyNutrient = "foodEnergy";
    public const string ProteinNutrient = "protein";

    public const string SinceKey = "rawFood:since";
    public const string ChanceKey = "rawFood:chance";
    public const string WorstKey = "rawFood:worst";

    /// <summary>
    /// An essential food need at or below this and a colonist looks at raw ingredients at all. Only
    /// a pre-filter: whether one is eaten is the studio's starving score (0.9), reached at about 0.2.
    /// </summary>
    public const float NearStarvingLevel = 0.3f;

    public const string RawFoodProfile = "rawFood";
    public const string BadStomachProfile = "badStomach";
    private const string ComfortEffectSuffix = "Comfort";
    public const string ProcessKeyInfix = "_rawConsume_";

    private static readonly string[] TierWhat =
    {
        "", "crystal berries", "glassy creeper pods", "common oil tubers", "raw fish", "raw meat",
    };

    /// <summary>The tier of an item; Tier.None for anything a colonist does not eat raw.</summary>
    public static Tier TierOf(EntityType type)
    {
        string[] tags = type?.ItemType?.FoodType?.FoodTags;
        if (tags == null)
        {
            return Tier.None;
        }
        switch (type.KeyName)
        {
            case "item:crystalBerries": return Tier.CrystalBerries;
            case "item:glassyCreeperPods": return Tier.GlassyCreeperPods;
            case "item:commonOilTubers": return Tier.CommonOilTubers;
        }
        if (type.SubstancesType != null || !tags.Any(t => Array.IndexOf(RawMeatTags, t) >= 0))
        {
            return Tier.None;
        }
        return IsFish(type) ? Tier.RawFish : Tier.RawMeat;
    }

    /// <summary>
    /// Whether a raw meat came out of the water: a process needing the fishing skill catches it, or
    /// it is one of the <see cref="ShellfishKeys"/>.
    /// </summary>
    public static bool IsFish(EntityType type) =>
        type != null && (Array.IndexOf(ShellfishKeys, type.KeyName) >= 0 || CaughtByFishing().Contains(type.KeyName));

    private sealed class FishingCatch
    {
        public object Processes;
        public int Count;
        public HashSet<string> Keys;
    }

    private static FishingCatch fishingCatch;

    /// <summary>What the fishing processes make, by key; built again when the process table changes.</summary>
    private static HashSet<string> CaughtByFishing()
    {
        var all = GameData.Instance?.AllProcessTypes;
        if (all == null)
        {
            return new HashSet<string>();
        }
        FishingCatch known = fishingCatch;
        if (known != null && ReferenceEquals(known.Processes, all) && known.Count == all.Count)
        {
            return known.Keys;
        }
        var keys = new HashSet<string>();
        foreach (ProcessType process in all.Values)
        {
            if (process?.RequiredSkill == FishingSkill && process.Outputs != null)
            {
                foreach (var output in process.Outputs)
                {
                    if (output?.EntityTypeToCreate != null)
                    {
                        keys.Add(output.EntityTypeToCreate);
                    }
                }
            }
        }
        fishingCatch = new FishingCatch { Processes = all, Count = all.Count, Keys = keys };
        return keys;
    }

    public static string ProfileKey(string profile, Tier tier) => profile + (int)tier;

    // ------------------------------------------------------------------ data

    /// <summary>Called at the end of BaseDataLoader.InitEffectTypes - always, so a save with one active loads.</summary>
    public static void AddEffects(List<EffectType> list)
    {
        for (Tier t = Tier.CrystalBerries; t <= Tier.RawMeat; t++)
        {
            list.Add(new NumberEffectType
            {
                KeyName = ProfileKey(RawFoodProfile, t) + ComfortEffectSuffix,
                Name = "Raw food",
                Affects = AffectsNumbers.AgentComfort,
                FormatAsPercentage = true,
                DurationInDays = DurationInDays[(int)t],
                Intensity = -ComfortCost[(int)t]
            });
            list.Add(new NumberEffectType
            {
                KeyName = ProfileKey(BadStomachProfile, t) + ComfortEffectSuffix,
                Name = "Bad stomach",
                Affects = AffectsNumbers.AgentComfort,
                FormatAsPercentage = true,
                DurationInDays = DurationInDays[(int)t],
                Intensity = -ComfortPerModifier * StomachModifier[(int)t]
            });
        }
    }

    /// <summary>Called at the end of BaseDataLoader.InitEffectProfileTypes - always, as AddEffects.</summary>
    public static void AddEffectProfiles(List<EffectProfileType> list)
    {
        for (Tier t = Tier.CrystalBerries; t <= Tier.RawMeat; t++)
        {
            string raw = ProfileKey(RawFoodProfile, t);
            list.Add(new EffectProfileType
            {
                KeyName = raw,
                Name = "Raw food",
                Description = "Had to eat " + TierWhat[(int)t] + " raw: there was no proper food left.",
                Effects = new[] { raw + ComfortEffectSuffix },
                SortOrder = 150
            });
            string stomach = ProfileKey(BadStomachProfile, t);
            float m = StomachModifier[(int)t];
            list.Add(new EffectProfileType
            {
                KeyName = stomach,
                Name = "Bad stomach",
                Description = "The " + TierWhat[(int)t] + " did not agree with the character: walks " + Percent(SpeedPerModifier * m) +
                              " slower, work takes " + Percent(WorkTimePerModifier * m) + " longer, energy is at most " + Percent(EnergyCap) +
                              ", and all food feeds half as much.",
                Effects = new[] { stomach + ComfortEffectSuffix },
                SortOrder = 160
            });
        }
    }

    private static string Percent(float fraction) =>
        Math.Round(fraction * 100f).ToString(CultureInfo.InvariantCulture) + "%";

    /// <summary>A person type's raw-food consume processes, by the item.</summary>
    private static readonly ConditionalWeakTable<BiologicalType, Dictionary<EntityType, ProcessType>> processes = new();

    /// <summary>
    /// Called at the end of BiologicalType.InitConsumeProcesses, for every creature type; makes the
    /// processes for person types only. Always, whether or not the mod is on (see the summary).
    /// </summary>
    public static void AddConsumeProcesses(EntityType parent, BiologicalType bio)
    {
        if (parent?.Person == null || bio?.ConsumeProcesses == null)
        {
            return;
        }
        var made = new Dictionary<EntityType, ProcessType>();
        foreach (EntityType food in GameData.Instance.AllEntityTypes.Values)
        {
            if (TierOf(food) != Tier.None && !bio.ConsumeProcesses.ContainsKey(food))
            {
                made[food] = bio.CreateConsumeProcess(parent, food, ProcessKeyInfix);
            }
        }
        processes.AddOrUpdate(bio, made);
    }

    /// <summary>
    /// Called from GoalEat.AddConsumeGoals: the process for eating a raw ingredient, or null for
    /// anything the eater eats the studio's way. Not switched: a meal begun is finished.
    /// </summary>
    public static ProcessType ConsumeProcessFor(Entity eater, EntityType food)
    {
        BiologicalType bio = eater?.EntityType?.BiologicalType;
        if (bio == null || food == null || !processes.TryGetValue(bio, out var byFood))
        {
            return null;
        }
        return byFood.TryGetValue(food, out ProcessType process) ? process : null;
    }

    /// <summary>Called from EvaluateEat.ScoreFoodItem: whether this is a raw ingredient to this eater.</summary>
    public static bool IsRawFood(Entity eater, EntityType food) => ConsumeProcessFor(eater, food) != null;

    // ------------------------------------------------------------------ choosing

    /// <summary>Whether an essential food need is at or below <see cref="NearStarvingLevel"/>.</summary>
    public static bool NearStarving(IEnumerable<(SimSide.AI.Needs.NeedType Type, float Level)> needs)
    {
        foreach (var (type, level) in needs)
        {
            if (type?.FoodNeedType != null && type.FoodNeedType.IsEssential && level <= NearStarvingLevel)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Called at the end of EvaluateEat.GetAllFoodItems: a colonist near starving also lists the
    /// expedition's raw ingredients it can reach, as the studio lists food (IsValidFoodItem without
    /// IsEatable).
    /// </summary>
    public static void AddRawFood(Entity eater, BiologicalEntity bio, SharedKnowledge knowledge, EntityGroup group, List<EntityID> allFoodItems)
    {
        if (!Enabled || eater?.PersonEntity == null || bio?.Needs?.NeedsList == null || group?.Items == null || allFoodItems == null || knowledge == null)
        {
            return;
        }
        if (!NearStarving(bio.Needs.NeedsList.Values.Select(n => (n.NeedType, n.CurrentLevel))))
        {
            return;
        }
        bool holds = eater.EntityType.BiologicalType.HoldsFoodWhenEating;
        List<(EntityID Id, EntityType Type)> gone = null;
        foreach (var byType in group.Items)
        {
            if (!IsRawFood(eater, byType.Key))
            {
                continue;
            }
            foreach (EntityID id in byType.Value)
            {
                List<EntityID> invalid = null;
                if (GoalEvaluator.HandleOwnerDataResult(knowledge, id, group, out IKnownEntityData data, ref invalid))
                {
                    if (GoalEvaluator.IsValidPlaysiteItem(eater, data, holds) && !allFoodItems.Contains(data.EntityID))
                    {
                        allFoodItems.Add(data.EntityID);
                    }
                }
                else if (invalid != null)
                {
                    (gone ??= new List<(EntityID, EntityType)>()).Add((id, byType.Key));
                }
            }
        }
        // The studio's other overload deletes at once; these lists are being walked, so afterwards.
        if (gone != null)
        {
            foreach (var (id, type) in gone)
            {
                group.DeleteEntity(id, type);
            }
        }
    }

    /// <summary>
    /// Called from EvaluateEat.ScoreEating once the scores are sorted. While any food that is not
    /// raw scores at least <paramref name="minimumRatingToConsider"/>, raw ingredients are dropped;
    /// otherwise those that score enough go first, best tier first and, within a tier, best score
    /// first. Anything else keeps its place after them.
    /// </summary>
    public static void OrderRawFood(Entity eater, List<Tuple<IKnownEntityData, ProcessType, double>> scores, double minimumRatingToConsider)
    {
        if (scores == null || scores.Count == 0)
        {
            return;
        }
        Tier TierOfScore(Tuple<IKnownEntityData, ProcessType, double> s) =>
            s.Item1 != null && IsRawFood(eater, s.Item1.EntityType) ? TierOf(s.Item1.EntityType) : Tier.None;
        if (!scores.Any(s => TierOfScore(s) != Tier.None))
        {
            return;
        }
        if (scores.Any(s => TierOfScore(s) == Tier.None && s.Item3 >= minimumRatingToConsider))
        {
            scores.RemoveAll(s => TierOfScore(s) != Tier.None);
            return;
        }
        var raw = scores.Where(s => TierOfScore(s) != Tier.None && s.Item3 >= minimumRatingToConsider)
                        .OrderBy(TierOfScore).ThenByDescending(s => s.Item3).ToList();
        var rest = scores.Where(s => TierOfScore(s) == Tier.None).ToList();
        scores.Clear();
        scores.AddRange(raw);
        scores.AddRange(rest);
    }

    // ------------------------------------------------------------------ after

    /// <summary>The chance of a bad stomach a tier adds, with the CHANCE setting applied; at most 1.</summary>
    public static float ChanceOf(Tier tier) =>
        Math.Min(1f, StomachChance[(int)tier] * ChoiceValue(ChanceSetting, 100) / 100f);

    /// <summary>
    /// Called from GoalEat.ConsumeStomachContents for each item eaten, before it is credited: for a
    /// raw ingredient, the tier's "Raw food" effect, its chance added to the day's, and a roll of the
    /// day's chance for "Bad stomach" at the worst tier eaten that day.
    /// </summary>
    public static void OnEaten(Entity eater, EntityType food)
    {
        if (!Enabled || eater?.SimEffects == null || The.Sim == null || !IsRawFood(eater, food))
        {
            return;
        }
        Tier tier = TierOf(food);
        if (tier == Tier.None)
        {
            return;
        }
        StartWorst(eater.SimEffects, RawFoodProfile, tier);
        float chanceToday = AddRawMeal(eater, tier, out Tier worstToday);
        if (BadStomachSetting.On && The.Sim.GameplayRandomGenerator.RandomBetween(0f, 1f) < chanceToday)
        {
            StartWorst(eater.SimEffects, BadStomachProfile, worstToday);
        }
    }

    /// <summary>
    /// Adds a raw meal's chance to the colonist's running chance and returns it (at most 1), with the
    /// worst tier eaten in the window. A meal a day or more after the window opened opens a new one.
    /// </summary>
    public static float AddRawMeal(Entity eater, Tier tier, out Tier worst)
    {
        double now = The.Sim.TotalUnPausedGameTimeInSeconds;
        float? since = Number(eater, SinceKey);
        float sum = 0f;
        worst = Tier.None;
        if (since.HasValue && now >= since.Value && now - since.Value < ChanceWindowInDays * DateAndTime.secondsPerDay)
        {
            sum = Number(eater, ChanceKey) ?? 0f;
            worst = (Tier)(int)(Number(eater, WorstKey) ?? 0f);
        }
        else
        {
            Entity.SetPropertyValue(ref eater.CustomFields, SinceKey, new PropertyResult { NumberResult = (float)now });
        }
        sum = Math.Min(1f, sum + ChanceOf(tier));
        if (tier > worst)
        {
            worst = tier;
        }
        Entity.SetPropertyValue(ref eater.CustomFields, ChanceKey, new PropertyResult { NumberResult = sum });
        Entity.SetPropertyValue(ref eater.CustomFields, WorstKey, new PropertyResult { NumberResult = (int)worst });
        return sum;
    }

    /// <summary>
    /// Starts a profile at the worse of this tier and the one active, replacing it: one at a time,
    /// and eating again restarts the timer. Returns the tier started; Tier.None if there is none.
    /// </summary>
    public static Tier StartWorst(SimEffectsComponent effects, string profile, Tier tier)
    {
        Tier active = ActiveTier(effects, profile);
        if (active > tier)
        {
            tier = active;
        }
        if (!GameData.Instance.AllEffectProfileTypes.TryGetValue(ProfileKey(profile, tier), out EffectProfileType type))
        {
            return Tier.None;
        }
        for (Tier t = Tier.CrystalBerries; t <= Tier.RawMeat; t++)
        {
            if (GameData.Instance.AllEffectProfileTypes.TryGetValue(ProfileKey(profile, t), out EffectProfileType other))
            {
                effects.Remove(other);
            }
        }
        effects.Start(type);
        return tier;
    }

    /// <summary>
    /// Called from GoalEat.ConsumeStomachContents: the share of a meal's nutrients that reaches a
    /// colonist with a bad stomach (<see cref="SickNutritionFactor"/>); 1 otherwise. Follows the effect.
    /// </summary>
    public static float NutritionFactor(Entity eater) => StomachModifierOf(eater) > 0f ? SickNutritionFactor : 1f;

    /// <summary>
    /// Called from Food.ConsumeBy per nutrient: what share of it a colonist gets from a raw
    /// ingredient - land meat a third of its food energy and protein, fish three quarters of its
    /// protein. 1 for anything else, and for anyone else (an animal eats it its own way).
    /// </summary>
    public static float RawNutrientFactor(Entity eater, EntityType food, FoodNutrientType nutrient)
    {
        if (nutrient == null || !IsRawFood(eater, food))
        {
            return 1f;
        }
        switch (TierOf(food))
        {
            case Tier.RawMeat:
                return nutrient.KeyName == EnergyNutrient || nutrient.KeyName == ProteinNutrient ? RawMeatEnergyProteinFactor : 1f;
            case Tier.RawFish:
                return nutrient.KeyName == ProteinNutrient ? RawFishProteinFactor : 1f;
            default:
                return 1f;
        }
    }

    /// <summary>The worst tier of a profile active in these effects; Tier.None if none.</summary>
    public static Tier ActiveTier(SimEffectsComponent effects, string profile)
    {
        Tier worst = Tier.None;
        if (effects?.EffectProfiles == null)
        {
            return worst;
        }
        foreach (SimEffectProfile active in effects.EffectProfiles)
        {
            string key = active?.EffectProfileType?.KeyName;
            if (key != null && key.Length == profile.Length + 1 && key.StartsWith(profile, StringComparison.Ordinal))
            {
                int t = key[key.Length - 1] - '0';
                if (t > (int)worst && t <= (int)Tier.RawMeat)
                {
                    worst = (Tier)t;
                }
            }
        }
        return worst;
    }

    /// <summary>The bad stomach modifier of a person now; 0 without one.</summary>
    public static float StomachModifierOf(Entity entity) =>
        entity?.EntityType?.Person == null ? 0f : StomachModifier[(int)ActiveTier(entity.SimEffects, BadStomachProfile)];

    /// <summary>Called from Locomotor.CalculateSpeed: 1 - 0.2 x modifier.</summary>
    public static float SpeedFactor(Entity entity) => 1f - SpeedPerModifier * StomachModifierOf(entity);

    /// <summary>Called from ProcessJob.CalculateWorkDurationInDays: 1 + 0.3 x modifier.</summary>
    public static float WorkTimeFactor(Entity worker) => 1f + WorkTimePerModifier * StomachModifierOf(worker);

    /// <summary>Called from the BiologicalEntity.EnergyLevel getter: at most <see cref="EnergyCap"/> with a bad stomach.</summary>
    public static float CapEnergy(Entity entity, float energy) =>
        StomachModifierOf(entity) > 0f ? Math.Min(energy, EnergyCap) : energy;

    private static float? Number(Entity entity, string key) =>
        entity.CustomFields != null && entity.CustomFields.TryGetValue(key, out var stored) ? stored.NumberResult : null;

    private static int ChoiceValue(ModSetting setting, int fallback) =>
        int.TryParse(setting.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) ? v : fallback;
}
