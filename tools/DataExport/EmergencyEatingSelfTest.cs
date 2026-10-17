using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UWGame.SimSide;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Processes;
using UWGame.SimSide.SimEffects;

namespace UW.Tools.DataExport;

/// <summary>
/// --emergency-eating-selftest: EmergencyEatingMod against the real, validated tables.
///
/// WHAT. Kastuk, "SimEffects and Emergency eating", 2026-10-17: a starving colonist with no food
/// left eats raw ingredients in the order crystal berries, glassy creeper pods, common oil tubers,
/// raw fish, raw meat; "Bad stomach" with a modifier rising by tier slows walking (0.2m) and work
/// (0.3m), takes comfort (0.2m) and caps energy at 0.6. Kastuk's answer, same day: comfort -5% for
/// 0.4 days (berries) to -40% for 1.5 days (meat), not stacking but restarting at the worse; bad
/// stomach chances of 5..40% adding up over a day; a bad stomach halves the nutrition of the meal
/// that brought it and of every meal after; raw land meat a third of its energy and protein, raw
/// fish three quarters of its protein; land meat the worst tier.
///
/// Checked: every key the mod names is in the tables and in the tier it should be; each person type
/// has a consume process per raw ingredient, built the studio's way, registered, and NOT among its
/// ConsumeProcesses (so the FOOD rating and the food lists are untouched - BiologicalType.IsEatable
/// still says no); each tier feeds a nutrient a colonist needs; the effects are registered with the
/// switch off, with Kastuk's numbers and durations; the real OrderRawFood the evaluator calls keeps raw food out
/// while food will do and puts berries before meat when not; and the real OnEaten, on a colonist
/// with the game's effect component and the seeded generator, starts the effects, restarts the timer
/// at the worst, adds the day's chances and starts again a day later, rolls about the asked share of
/// bad stomachs at the worst tier of the day, and the speed, work, energy and comfort hooks follow;
/// the real Food.ConsumeBy feeds raw meat and fish the asked share, and half of anything with a bad
/// stomach; and GoalEat.ConsumeStomachContents rolls before it credits the meal (by its IL).
///
/// NOT HERE. A colony: EvaluateEat's path search and GoalEat's walking and eating need a running
/// site, so whether a starving colonist actually walks to the berries is for a game.
/// </summary>
internal static partial class Program
{
    private static int EmergencyEatingSelfTest()
    {
        UWGame.The.Sim = new Sim { Mode = Sim.EngineMode.Game };
        typeof(Sim).GetMethod("CreateLookupCollections", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(UWGame.The.Sim, null);
        int rc = Run(Sim.SerializeMode.NoSerialize, "base tables, the way the game loads them");
        if (rc != 0) return rc;
        if (!ValidateDataComplete()) return 1;
        // Where the game generates each creature's consume processes - and the mod its own.
        if (!InitializeWithoutContent()) return 1;
        Console.WriteLine("==> emergency eating self-test");
        System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture; // the numbers below
        UWGame.Mods.EmergencyEatingMod.RegisterSettings();
        var enabled = UWGame.Mods.ModSettings.Find("emergencyeating.enabled");
        if (enabled == null)
        {
            Console.WriteLine("  the mod is not in this build - nothing to check");
            return 0;
        }
        var stomach = UWGame.Mods.ModSettings.Find("emergencyeating.badStomach");
        var chance = UWGame.Mods.ModSettings.Find("emergencyeating.chance");
        int failures = 0;
        void Check(bool ok, string what)
        {
            Console.WriteLine((ok ? "  ok    " : "  FAIL  ") + what);
            if (!ok) failures++;
        }
        Check(enabled.DefaultValue == "false" && enabled.AffectsSimulation && !enabled.TakesEffectOnNextLoad,
              "off by default, a simulation setting that applies at once");
        Check(stomach != null && stomach.DefaultValue == "true" && chance != null && chance.DefaultValue == "100",
              "a bad stomach is on, at the asked chance, by default");
        Check(!enabled.On, "  and the tables below were loaded with it off");

        var types = GameData.Instance.AllEntityTypes;
        EntityType Type(string key) => types.TryGetValue(key, out var t) ? t : null;
        UWGame.Mods.EmergencyEatingMod.Tier TierOf(string key) => UWGame.Mods.EmergencyEatingMod.TierOf(Type(key));
        var T = new
        {
            None = UWGame.Mods.EmergencyEatingMod.Tier.None,
            Berries = UWGame.Mods.EmergencyEatingMod.Tier.CrystalBerries,
            Pods = UWGame.Mods.EmergencyEatingMod.Tier.GlassyCreeperPods,
            Tubers = UWGame.Mods.EmergencyEatingMod.Tier.CommonOilTubers,
            Fish = UWGame.Mods.EmergencyEatingMod.Tier.RawFish,
            Meat = UWGame.Mods.EmergencyEatingMod.Tier.RawMeat,
        };

        // The tiers, against the tables.
        Check(TierOf("item:crystalBerries") == T.Berries && TierOf("item:glassyCreeperPods") == T.Pods && TierOf("item:commonOilTubers") == T.Tubers,
              "crystal berries, glassy creeper pods, common oil tubers: tiers 1, 2, 3");
        foreach (string key in new[] { "item:crystalBerries", "item:glassyCreeperPods", "item:commonOilTubers" })
        {
            Check(Type(key)?.ItemType?.FoodType?.FoodTags?.Contains("inedibleVegi") == true, "  " + key + " is inedibleVegi, which a colonist does not eat");
        }
        foreach (string key in new[] { "item:alabasterRay", "item:streakFin", "item:carbonTail", "item:daggermouth", "item:impEel", "item:minnowsLive", "item:torux", "item:clamwich" })
        {
            Check(TierOf(key) == T.Fish, key + " is raw fish (tier 4)");
        }
        var fishedFromData = GameData.Instance.AllProcessTypes.Values.Where(p => p.RequiredSkill == "fishing" && p.Outputs != null)
            .SelectMany(p => p.Outputs).Select(o => o.EntityTypeToCreate)
            .Where(k => k != null && Type(k)?.ItemType?.FoodType?.FoodTags?.Any(g => g == "rawMeat" || g == "smallRawMeat") == true)
            .Distinct().OrderBy(k => k).ToList();
        Check(fishedFromData.Count >= 6 && fishedFromData.All(k => TierOf(k) == T.Fish) && fishedFromData.All(k => Array.IndexOf(UWGame.Mods.EmergencyEatingMod.ShellfishKeys, k) < 0),
              "the fish come from the data - every raw meat a fishing process catches, " + fishedFromData.Count + ": " + string.Join(", ", fishedFromData));
        var rawMeatTagged = types.Values.Where(t => t.SubstancesType == null && t.ItemType?.FoodType?.FoodTags?.Any(g => g == "rawMeat" || g == "smallRawMeat") == true).ToList();
        Check(rawMeatTagged.Count > 0 && rawMeatTagged.All(t => { var k = UWGame.Mods.EmergencyEatingMod.TierOf(t); return k == T.Fish || k == T.Meat; }),
              "every rawMeat or smallRawMeat item (" + rawMeatTagged.Count + ") is raw fish or raw meat");
        var meat = types.Values.Where(t => UWGame.Mods.EmergencyEatingMod.TierOf(t) == T.Meat).Select(t => t.KeyName).OrderBy(k => k).ToList();
        Check(meat.Contains("item:thunderChickenMeat") && meat.Contains("item:ursinix") && meat.Count >= 8,
              "raw meat (tier 5), " + meat.Count + ": " + string.Join(", ", meat));
        Check(meat.All(k => Type(k).ItemType.FoodType.FoodTags.Any(g => g == "rawMeat" || g == "smallRawMeat") && Type(k).SubstancesType == null),
              "  every one is rawMeat or smallRawMeat, and none a carcass to butcher");
        foreach (string key in new[] { "item:roastedStreakFin", "item:powderedCrystalBerries", "item:bakedCommonOilTubers", "item:spottedOilTubers",
                                       "item:rottenVegetables", "item:binalRatChunk", "item:hardtack", "item:firewood" })
        {
            Check(Type(key) != null && TierOf(key) == T.None, key + " is not raw food to the mod");
        }

        // The processes, per person type.
        var people = types.Values.Where(t => t.Person != null && t.BiologicalType != null).ToList();
        Check(people.Count > 0, "person types: " + string.Join(", ", people.Select(p => p.KeyName)));
        var raw = types.Values.Where(t => UWGame.Mods.EmergencyEatingMod.TierOf(t) != T.None).ToList();
        foreach (EntityType person in people)
        {
            var bio = person.BiologicalType;
            var fake = Colonist(person);
            int made = raw.Count(r => UWGame.Mods.EmergencyEatingMod.ConsumeProcessFor(fake, r) != null);
            Check(made == raw.Count, $"{person.KeyName}: a raw-food process for each of the {raw.Count} raw ingredients ({made})");
            bool studioShape = raw.All(r =>
            {
                ProcessType p = UWGame.Mods.EmergencyEatingMod.ConsumeProcessFor(fake, r);
                return p != null && p.IsConsumeProcess && !p.UseWorkerEnergyAsProductionFactor
                       && p.KeyName == person.KeyName + UWGame.Mods.EmergencyEatingMod.ProcessKeyInfix + r.KeyName
                       && GameData.Instance.AllProcessTypes.TryGetValue(p.KeyName, out var registered) && registered == p
                       && p.Inputs.Length == 1 && p.Inputs[0].Entity == r.KeyName && p.Outputs.Length == 1 && p.Outputs[0].EntityTypeToCreate == r.KeyName
                       && p.WorkOrTimeNeeded.DaysNeeded == bio.TimeToConsumeFullMealInDays;
            });
            Check(studioShape, "  each built as the studio's consume process (one in, the same out, a meal's time) and registered by key");
            Check(raw.All(r => !bio.ConsumeProcesses.ContainsKey(r) && !bio.IsEatable(r)),
                  "  none is among its ConsumeProcesses: IsEatable still says no, so the FOOD rating and food lists are the studio's");
            Check(UWGame.Mods.EmergencyEatingMod.ConsumeProcessFor(fake, Type("item:roastedStreakFin")) == null && bio.ConsumeProcesses.ContainsKey(Type("item:roastedStreakFin")),
                  "  cooked fish keeps the studio's own process");
            var needs = bio.GetAdultNeedsAndWeight(out _)?.Where(n => n.FoodNeedType != null).Select(n => n.FoodNeedType.FoodNutrientType).ToList();
            foreach (var tier in new[] { T.Berries, T.Pods, T.Tubers, T.Fish, T.Meat })
            {
                var feeds = raw.Where(r => UWGame.Mods.EmergencyEatingMod.TierOf(r) == tier)
                               .All(r => r.ItemType.FoodType.FoodNutrientProfile.FoodNutrientTypes.Any(n => needs != null && needs.Contains(n.Nutrient) && n.Amount > 0f));
                Check(feeds, $"  tier {(int)tier} ({tier}) feeds a nutrient a colonist needs");
            }
        }
        var animal = types.Values.FirstOrDefault(t => t.Person == null && t.BiologicalType?.ConsumeProcesses != null);
        Check(animal != null && raw.All(r => UWGame.Mods.EmergencyEatingMod.ConsumeProcessFor(Colonist(animal), r) == null),
              "an animal (" + animal?.KeyName + ") gets none: it eats what its own tags say");

        // The effects, registered with the switch off.
        float[] comfort = { 0f, 0.05f, 0.12f, 0.20f, 0.30f, 0.40f };
        double[] days = { 0.0, 0.4, 0.65, 0.9, 1.2, 1.5 };
        float[] chances = { 0f, 0.05f, 0.10f, 0.20f, 0.30f, 0.40f };
        float[] modifier = { 0f, 0.2f, 0.4f, 0.6f, 0.8f, 1.0f };
        for (int t = 1; t <= 5; t++)
        {
            bool Comfort(string profile, float intensity) =>
                GameData.Instance.AllEffectProfileTypes.TryGetValue(profile + t, out var p) && p.EffectTypes?.Count == 1
                && p.EffectTypes[0] is NumberEffectType n && n.Affects == AffectsNumbers.AgentComfort && n.Operator == NumberEffectOperator.Add
                && UWGame.Common.IsEqual(n.Intensity, intensity) && n.DurationInDays == days[t] && p.Name != null && p.Description != null;
            Check(Comfort("rawFood", -comfort[t]), $"tier {t}: \"Raw food\" takes {comfort[t]:P0} comfort for {days[t]} days");
            Check(Comfort("badStomach", -0.2f * modifier[t]), $"tier {t}: \"Bad stomach\" (modifier {modifier[t]}) takes {0.2f * modifier[t]:P0} comfort for {days[t]} days, chance {chances[t]:P0}");
            Check(UWGame.Common.IsEqual(UWGame.Mods.EmergencyEatingMod.ChanceOf((UWGame.Mods.EmergencyEatingMod.Tier)t), chances[t]), "  at the asked chance");
        }
        chance.Value = "150";
        Check(UWGame.Common.IsEqual(UWGame.Mods.EmergencyEatingMod.ChanceOf(T.Meat), 0.6f) && UWGame.Common.IsEqual(UWGame.Mods.EmergencyEatingMod.ChanceOf(T.Berries), 0.075f),
              "CHANCE 150%: meat 60%, berries 7.5%");
        chance.Value = chance.DefaultValue;

        // Choosing: the real OrderRawFood EvaluateEat.ScoreEating calls.
        EntityType human = people[0];
        Entity colonist = Colonist(human);
        IKnownEntityData Fact(string key)
        {
            var fact = (MemoryFact)RuntimeHelpers.GetUninitializedObject(typeof(MemoryFact));
            fact.EntityType = Type(key);
            return fact;
        }
        List<Tuple<IKnownEntityData, ProcessType, double>> Scores(params (string key, double score)[] s) =>
            s.Select(x => Tuple.Create(Fact(x.key), (ProcessType)null, x.score)).ToList();
        string Keys(List<Tuple<IKnownEntityData, ProcessType, double>> l) => string.Join(", ", l.Select(x => x.Item1.EntityType.KeyName.Substring(5)));
        double min = 0.5;
        var withFood = Scores(("item:crystalBerries", 0.95), ("item:hardtack", 0.8), ("item:thunderChickenMeat", 0.9));
        UWGame.Mods.EmergencyEatingMod.OrderRawFood(colonist, withFood, min);
        Check(Keys(withFood) == "hardtack", "hardtack scores enough: raw food is dropped, though it scored higher (" + Keys(withFood) + ")");
        var noFood = Scores(("item:thunderChickenMeat", 0.95), ("item:streakFin", 0.9), ("item:crystalBerries", 0.7), ("item:commonOilTubers", 0.8), ("item:glassyCreeperPods", 0.4), ("item:crystalBerries", 0.75));
        UWGame.Mods.EmergencyEatingMod.OrderRawFood(colonist, noFood, min);
        Check(Keys(noFood) == "crystalBerries, crystalBerries, commonOilTubers, streakFin, thunderChickenMeat",
              "no food: berries (the better score first), tubers, fish, meat; pods scoring too little are dropped (" + Keys(noFood) + ")");
        var weakFood = Scores(("item:hardtack", 0.3), ("item:impEel", 0.92), ("item:ursinix", 0.94));
        UWGame.Mods.EmergencyEatingMod.OrderRawFood(colonist, weakFood, min);
        Check(Keys(weakFood) == "impEel, ursinix, hardtack", "food that scores too little does not hold raw food back: fish before meat (" + Keys(weakFood) + ")");
        var cookedOnly = Scores(("item:hardtack", 0.8), ("item:roastedStreakFin", 0.9));
        UWGame.Mods.EmergencyEatingMod.OrderRawFood(colonist, cookedOnly, min);
        Check(Keys(cookedOnly) == "hardtack, roastedStreakFin", "no raw food: the list is the studio's, untouched");
        NeedType[] humanNeeds = human.BiologicalType.GetAdultNeedsAndWeight(out _);
        NeedType essential = humanNeeds.First(n => n.FoodNeedType != null && n.FoodNeedType.IsEssential);
        NeedType stimulant = humanNeeds.FirstOrDefault(n => n.FoodNeedType != null && !n.FoodNeedType.IsEssential);
        Check(UWGame.Mods.EmergencyEatingMod.NearStarving(new[] { (essential, 0.2f) }) && !UWGame.Mods.EmergencyEatingMod.NearStarving(new[] { (essential, 0.5f) })
              && (stimulant == null || !UWGame.Mods.EmergencyEatingMod.NearStarving(new[] { (essential, 0.6f), (stimulant, 0f) })),
              "only a colonist near starving of an essential need (" + essential.KeyName + ") looks at raw food; no drink is not hunger");

        // After: the real OnEaten, on a colonist with the game's effect component and the seeded generator.
        // A seeded generator in the Sim's slot. Typed Client only so that it does not report each draw
        // to the replay recorder, which needs a running controller; the draws are the same.
        typeof(Sim).GetField("gameplayRandomGenerator", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(UWGame.The.Sim, new UWGame.Control.Replays.RandomGenerator(4242, UWGame.Control.Replays.RandomGenerator.GeneratorType.Client));
        SimEffectsComponent effects = colonist.SimEffects;
        UWGame.Mods.EmergencyEatingMod.Tier Active(string profile) => UWGame.Mods.EmergencyEatingMod.ActiveTier(effects, profile);
        void ClearAll()
        {
            foreach (var p in effects.EffectProfiles.Select(p => p.EffectProfileType).ToList()) effects.Remove(p);
        }
        UWGame.Mods.EmergencyEatingMod.OnEaten(colonist, Type("item:thunderChickenMeat"));
        Check(Active("rawFood") == T.None, "switched off: eating raw meat starts nothing");
        enabled.Value = "true";
        UWGame.Mods.EmergencyEatingMod.OnEaten(colonist, Type("item:hardtack"));
        Check(effects.EffectProfiles.Count == 0, "hardtack is not raw food: nothing");
        var clock = typeof(Sim).GetProperty("TotalUnPausedGameTimeInSeconds")!;
        double secondsPerDay = DateAndTime.secondsPerDay;
        void At(double day) => clock.SetValue(UWGame.The.Sim, day * secondsPerDay);
        SimEffect First(string profile) => effects.EffectProfiles.First(p => p.KeyName.StartsWith(profile)).Effects[0];
        double Started(string profile) => First(profile).StartedOn / secondsPerDay;
        double Expires(string profile) => First(profile).ExpiresOn!.Value / secondsPerDay;
        void Forget() => colonist.CustomFields = null;
        At(10.0);
        UWGame.Mods.EmergencyEatingMod.OnEaten(colonist, Type("item:streakFin"));
        Check(Active("rawFood") == T.Fish && UWGame.Common.IsEqual(colonist.GetEffect(AffectsNumbers.AgentComfort, 0f) - StomachComfort(colonist), -0.30f)
              && Math.Abs(Expires("rawFood") - 11.2) < 1e-6,
              "raw fish: \"Raw food\" tier 4, comfort -30% for 1.2 days");
        At(10.5);
        UWGame.Mods.EmergencyEatingMod.OnEaten(colonist, Type("item:crystalBerries"));
        Check(Active("rawFood") == T.Fish && effects.EffectProfiles.Count(p => p.KeyName.StartsWith("rawFood")) == 1
              && Math.Abs(Started("rawFood") - 10.5) < 1e-6 && Math.Abs(Expires("rawFood") - 11.7) < 1e-6,
              FormattableString.Invariant($"berries half a day after fish: no stacking - the fish's -30% restarts (ends day {Expires("rawFood")})"));
        At(11.0);
        UWGame.Mods.EmergencyEatingMod.OnEaten(colonist, Type("item:ursinix"));
        Check(Active("rawFood") == T.Meat && effects.EffectProfiles.Count(p => p.KeyName.StartsWith("rawFood")) == 1 && Math.Abs(Expires("rawFood") - 12.5) < 1e-6
              && UWGame.Common.IsEqual(colonist.GetEffect(AffectsNumbers.AgentComfort, 0f) - StomachComfort(colonist), -0.40f),
              "meat after fish: meat replaces it, -40% for 1.5 days - the most it gets");

        // The day's chance, adding up.
        Forget();
        At(20.0);
        float c1 = UWGame.Mods.EmergencyEatingMod.AddRawMeal(colonist, T.Berries, out var w1);
        At(20.3);
        float c2 = UWGame.Mods.EmergencyEatingMod.AddRawMeal(colonist, T.Berries, out var w2);
        At(20.6);
        float c3 = UWGame.Mods.EmergencyEatingMod.AddRawMeal(colonist, T.Meat, out var w3);
        At(20.9);
        float c4 = UWGame.Mods.EmergencyEatingMod.AddRawMeal(colonist, T.Pods, out var w4);
        Check(UWGame.Common.IsEqual(c1, 0.05f) && UWGame.Common.IsEqual(c2, 0.10f) && UWGame.Common.IsEqual(c3, 0.50f) && UWGame.Common.IsEqual(c4, 0.60f)
              && w1 == T.Berries && w2 == T.Berries && w3 == T.Meat && w4 == T.Meat,
              FormattableString.Invariant($"berries, berries, meat, pods within a day: chance {c1}, {c2}, {c3}, {c4}; the worst stays meat"));
        At(21.0);
        float c5 = UWGame.Mods.EmergencyEatingMod.AddRawMeal(colonist, T.Berries, out var w5);
        Check(UWGame.Common.IsEqual(c5, 0.05f) && w5 == T.Berries, FormattableString.Invariant($"a day after the first raw meal the count starts again: berries {c5}"));
        At(21.1);
        UWGame.Mods.EmergencyEatingMod.AddRawMeal(colonist, T.Meat, out _);
        UWGame.Mods.EmergencyEatingMod.AddRawMeal(colonist, T.Meat, out _);
        float c6 = UWGame.Mods.EmergencyEatingMod.AddRawMeal(colonist, T.Meat, out _);
        Check(UWGame.Common.IsEqual(c6, 1f), "at most 100%");
        Check(colonist.CustomFields != null && colonist.CustomFields.ContainsKey("rawFood:since") && colonist.CustomFields.ContainsKey("rawFood:chance")
              && colonist.CustomFields.ContainsKey("rawFood:worst"),
              "kept in the colonist's CustomFields, which the save carries");
        // A sure roll: 95% already, and berries take it to 100% - the bad stomach is the day's worst (meat), not the berries'.
        ClearAll();
        Forget();
        At(30.0);
        UWGame.Mods.EmergencyEatingMod.AddRawMeal(colonist, T.Meat, out _);
        Entity.SetPropertyValue(ref colonist.CustomFields, "rawFood:chance", new UWGame.ClientSide.PropertyPresentation.PropertyResult { NumberResult = 0.95f });
        UWGame.Mods.EmergencyEatingMod.OnEaten(colonist, Type("item:crystalBerries"));
        Check(Active("badStomach") == T.Meat && Math.Abs(Expires("badStomach") - 31.5) < 1e-6,
              "berries on a 95% day after meat: a bad stomach for sure, at meat's strength, for 1.5 days");
        At(30.5);
        UWGame.Mods.EmergencyEatingMod.OnEaten(colonist, Type("item:crystalBerries"));
        Check(Active("badStomach") == T.Meat && effects.EffectProfiles.Count(p => p.KeyName.StartsWith("badStomach")) == 1 && Math.Abs(Expires("badStomach") - 32.0) < 1e-6,
              "berries again while sick: one bad stomach, still meat's, its timer restarted");
        int Rolls(string key, int n)
        {
            int got = 0;
            for (int i = 0; i < n; i++)
            {
                ClearAll();
                Forget();
                UWGame.Mods.EmergencyEatingMod.OnEaten(colonist, Type(key));
                if (Active("badStomach") != T.None) got++;
            }
            return got;
        }
        int meatRolls = Rolls("item:thunderChickenMeat", 1000), berryRolls = Rolls("item:crystalBerries", 1000);
        Check(meatRolls > 340 && meatRolls < 460, $"raw meat 1000 times, a fresh day each: {meatRolls} bad stomachs (asked: 40%)");
        Check(berryRolls > 25 && berryRolls < 80, $"crystal berries 1000 times, a fresh day each: {berryRolls} bad stomachs (asked: 5%)");
        int twice = 0;
        for (int i = 0; i < 1000; i++)
        {
            ClearAll();
            Forget();
            UWGame.Mods.EmergencyEatingMod.OnEaten(colonist, Type("item:streakFin"));
            UWGame.Mods.EmergencyEatingMod.OnEaten(colonist, Type("item:streakFin"));
            if (Active("badStomach") != T.None) twice++;
        }
        Check(twice > 660 && twice < 780,
              $"raw fish twice in a day, 1000 times: {twice} bad stomachs (rolls at 30%, then 60%: 1 - 0.7 x 0.4 = 72%; 51% if they did not add up)");
        stomach.Value = "false";
        Check(Rolls("item:thunderChickenMeat", 200) == 0, "RAW FOOD CAN UPSET THE STOMACH off: never");
        stomach.Value = stomach.DefaultValue;

        // The bad stomach's hooks, at each tier.
        ClearAll();
        Check(UWGame.Mods.SicknessEffects.SpeedFactor(colonist) == 1f && UWGame.Mods.SicknessEffects.WorkTimeFactor(colonist) == 1f
              && UWGame.Mods.SicknessEffects.CapEnergy(colonist, 0.95f) == 0.95f, "no bad stomach: speed, work time and energy are the studio's");
        for (int t = 1; t <= 5; t++)
        {
            ClearAll();
            UWGame.Mods.EmergencyEatingMod.StartWorst(effects, "badStomach", (UWGame.Mods.EmergencyEatingMod.Tier)t);
            float m = modifier[t];
            Check(UWGame.Common.IsEqual(UWGame.Mods.SicknessEffects.SpeedFactor(colonist), 1f - 0.2f * m)
                  && UWGame.Common.IsEqual(UWGame.Mods.SicknessEffects.WorkTimeFactor(colonist), 1f + 0.3f * m)
                  && UWGame.Common.IsEqual(colonist.GetEffect(AffectsNumbers.AgentComfort, 0f), -0.2f * m)
                  && UWGame.Mods.SicknessEffects.CapEnergy(colonist, 0.95f) == 0.6f && UWGame.Mods.SicknessEffects.CapEnergy(colonist, 0.4f) == 0.4f,
                  $"bad stomach {t} (m={m}): walks x{1f - 0.2f * m}, work takes x{1f + 0.3f * m}, comfort {-0.2f * m:P0}, energy at most 60%");
        }
        UWGame.Mods.EmergencyEatingMod.StartWorst(effects, "badStomach", T.Berries);
        Check(Active("badStomach") == T.Meat, "a lesser bad stomach does not replace a worse one: the worse restarts");
        enabled.Value = "false";
        Check(UWGame.Common.IsEqual(UWGame.Mods.SicknessEffects.SpeedFactor(colonist), 0.8f),
              "switched off mid-day: the bad stomach the panel shows still applies until it expires");
        enabled.Value = enabled.DefaultValue;
        Check(UWGame.Mods.SicknessEffects.SpeedFactor(Colonist(animal)) == 1f, "an animal is never slowed by it");

        // Through the studio's own code, where the hooks are.
        Entity walker = Colonist(human);
        var legs = (Locomotor)RuntimeHelpers.GetUninitializedObject(typeof(Locomotor));
        legs.Parent = walker;
        SetField(legs, "moveAbility", 1f);
        walker.Components.Add(typeof(Locomotor), legs);
        var body = (BiologicalEntity)RuntimeHelpers.GetUninitializedObject(typeof(BiologicalEntity));
        body.Parent = walker;
        body.Needs = (Needs)RuntimeHelpers.GetUninitializedObject(typeof(Needs));
        body.Needs.NeedsList = new Dictionary<string, Need>();
        walker.Components.Add(typeof(BiologicalEntity), body);
        Entity worker = Colonist(human); // no body: the studio's energy factor stays 1, so the work time shows the hook alone
        var mind = (Intelligence)RuntimeHelpers.GetUninitializedObject(typeof(Intelligence));
        mind.Parent = worker;
        worker.Components.Add(typeof(Intelligence), mind);
        float Speed() => walker.Locomotor.CalculateSpeed(null, calculateWithTerrain: false, calculateWithBurden: false);
        float Work() => ProcessJob.CalculateWorkDurationInDays(worker, 1f, null);
        float Energy()
        {
            SetField(body, "energyLevelIsDirty", true);
            return body.EnergyLevel;
        }
        float speed0 = Speed(), work0 = Work(), energy0 = Energy();
        UWGame.Mods.EmergencyEatingMod.StartWorst(walker.SimEffects, "badStomach", T.Meat);
        UWGame.Mods.EmergencyEatingMod.StartWorst(worker.SimEffects, "badStomach", T.Meat);
        float speed1 = Speed(), work1 = Work(), energy1 = Energy();
        Check(speed0 > 0f && UWGame.Common.IsEqual(speed1, 0.8f * speed0),
              FormattableString.Invariant($"Locomotor.CalculateSpeed: {speed0} -> {speed1} with a bad stomach from meat"));
        Check(work0 > 0f && UWGame.Common.IsEqual(work1, 1.3f * work0),
              FormattableString.Invariant($"ProcessJob.CalculateWorkDurationInDays: {work0} -> {work1} days"));
        Check(energy0 == 1f && energy1 == 0.6f, FormattableString.Invariant($"BiologicalEntity.EnergyLevel: {energy0} -> {energy1}"));

        // Nutrition: the real Food.ConsumeBy, with the factor GoalEat.ConsumeStomachContents passes.
        Entity eater = Colonist(human);
        var eaterBody = (BiologicalEntity)RuntimeHelpers.GetUninitializedObject(typeof(BiologicalEntity));
        eaterBody.Parent = eater;
        eaterBody.Needs = (Needs)RuntimeHelpers.GetUninitializedObject(typeof(Needs));
        eaterBody.Needs.NeedsList = new Dictionary<string, Need>();
        eater.Components.Add(typeof(BiologicalEntity), eaterBody);
        var eaterMind = (Intelligence)RuntimeHelpers.GetUninitializedObject(typeof(Intelligence));
        eaterMind.Parent = eater;
        eaterMind.Allegiance = (UWGame.SimSide.Allegiances.Allegiance)RuntimeHelpers.GetUninitializedObject(typeof(UWGame.SimSide.Allegiances.Allegiance));
        eaterMind.Allegiance.Statistics = (UWGame.SimSide.Allegiances.Statistics.GroupStatistics)RuntimeHelpers.GetUninitializedObject(typeof(UWGame.SimSide.Allegiances.Statistics.GroupStatistics));
        eater.Components.Add(typeof(Intelligence), eaterMind);
        const float plenty = 1000f; // so no need fills up and every nutrient's uptake shows
        foreach (NeedType n in humanNeeds.Where(n => n.FoodNeedType?.FoodNutrientType != null))
        {
            var need = new Need(eaterBody.Needs, n);
            need.FoodNeed.TotalNeededNutrientBulk = plenty;
            eaterBody.Needs.NeedsList[n.FoodNeedType.FoodNutrientType.KeyName] = need;
        }
        // What each nutrient of a bulk of 1 gave, as a share of what it holds; NaN where it holds none.
        Dictionary<string, float> Feed(string key, float factor)
        {
            foreach (Need need in eaterBody.Needs.NeedsList.Values) need.CurrentLevel = 0f;
            var item = (UWGame.SimSide.Items.Item)RuntimeHelpers.GetUninitializedObject(typeof(UWGame.SimSide.Items.Item));
            item.Parent = Colonist(Type(key));
            var food = new UWGame.SimSide.Items.Food(item);
            UWGame.SimSide.Items.Food.UpdateNutrientAmounts(Type(key), 1f, food.NutrientBulkAmounts);
            food.ConsumeBy(eater, factor);
            return eaterBody.Needs.NeedsList.ToDictionary(kv => kv.Key, kv =>
            {
                float held = food.NutrientBulkAmounts.Where(a => a.Key.KeyName == kv.Key).Sum(a => a.Value);
                return held > 0f ? kv.Value.CurrentLevel * plenty / held : float.NaN;
            });
        }
        bool Gives(Dictionary<string, float> got, string nutrient, float share) => !float.IsNaN(got[nutrient]) && Math.Abs(got[nutrient] - share) < 1e-3f;
        string Shares(Dictionary<string, float> got) => string.Join(", ", got.Select(kv => FormattableString.Invariant($"{kv.Key} {kv.Value:0.###}")));
        ClearAll();
        foreach (var e in eater.SimEffects.EffectProfiles.Select(p => p.EffectProfileType).ToList()) eater.SimEffects.Remove(e);
        Check(UWGame.Mods.EmergencyEatingMod.NutritionFactor(eater) == 1f, "no bad stomach: food feeds in full");
        var rawMeat = Feed("item:thunderChickenMeat", UWGame.Mods.EmergencyEatingMod.NutritionFactor(eater));
        Check(Gives(rawMeat, "foodEnergy", 1f / 3f) && Gives(rawMeat, "protein", 1f / 3f) && Gives(rawMeat, "micronutrients", 1f),
              "Food.ConsumeBy, raw thunder chicken meat: a third of its energy and protein, all its micronutrients (" + Shares(rawMeat) + ")");
        var rawFish = Feed("item:streakFin", 1f);
        Check(Gives(rawFish, "protein", 0.75f) && rawFish.Where(kv => kv.Key != "protein" && !float.IsNaN(kv.Value)).All(kv => Math.Abs(kv.Value - 1f) < 1e-3f) && !float.IsNaN(rawFish["foodEnergy"]),
              "  raw streak fin: three quarters of its protein, the rest in full (" + Shares(rawFish) + ")");
        var berries = Feed("item:crystalBerries", 1f);
        Check(berries.Values.Where(v => !float.IsNaN(v)).All(v => Math.Abs(v - 1f) < 1e-3f) && berries.Values.Any(v => !float.IsNaN(v)),
              "  crystal berries: in full (" + Shares(berries) + ")");
        var cooked = Feed("item:roastedStreakFin", 1f);
        Check(cooked.Values.Where(v => !float.IsNaN(v)).All(v => Math.Abs(v - 1f) < 1e-3f), "  roasted streak fin, not raw: in full (" + Shares(cooked) + ")");
        var protein = GameData.Instance.AllFoodNutrientTypes.Values.First(n => n.KeyName == "protein");
        Check(UWGame.Mods.EmergencyEatingMod.RawNutrientFactor(Colonist(animal), Type("item:thunderChickenMeat"), protein) == 1f,
              "  an animal eating raw meat gets all of it: the factor is the colonist's raw-eating only");
        UWGame.Mods.EmergencyEatingMod.StartWorst(eater.SimEffects, "badStomach", T.Berries);
        Check(UWGame.Mods.EmergencyEatingMod.NutritionFactor(eater) == 0.5f, "a bad stomach, even the mildest: food feeds half");
        var sickHardtack = Feed("item:hardtack", UWGame.Mods.EmergencyEatingMod.NutritionFactor(eater));
        Check(sickHardtack.Values.Where(v => !float.IsNaN(v)).All(v => Math.Abs(v - 0.5f) < 1e-3f) && sickHardtack.Values.Any(v => !float.IsNaN(v)),
              "  hardtack with a bad stomach: half of every nutrient (" + Shares(sickHardtack) + ")");
        var sickMeat = Feed("item:thunderChickenMeat", UWGame.Mods.EmergencyEatingMod.NutritionFactor(eater));
        Check(Gives(sickMeat, "foodEnergy", 1f / 6f) && Gives(sickMeat, "micronutrients", 0.5f),
              "  raw meat with a bad stomach: a sixth of its energy, half its micronutrients (" + Shares(sickMeat) + ")");
        // "Halving nutrition of last one": the meal that upsets the stomach must be credited after the roll.
        var eatCalls = StomachCalls();
        int onEaten = eatCalls.FindIndex(c => c == "EmergencyEatingMod.OnEaten"), consume = eatCalls.FindIndex(c => c == "Food.ConsumeBy");
        int factor = eatCalls.FindIndex(c => c == "EmergencyEatingMod.NutritionFactor");
        Check(onEaten >= 0 && consume > onEaten && factor > onEaten && factor < consume,
              "GoalEat.ConsumeStomachContents rolls (OnEaten), then credits the meal (Food.ConsumeBy) at NutritionFactor - the upsetting meal feeds half too ("
              + string.Join(" -> ", eatCalls) + ")");

        Console.WriteLine(failures == 0 ? "emergency eating self-test OK" : $"emergency eating self-test FAILED - {failures} check(s)");
        return failures == 0 ? 0 : 1;
    }

    /// <summary>The comfort of any "Bad stomach" in a colonist's effects (it is rolled, so excluded where only "Raw food" is checked).</summary>
    private static float StomachComfort(Entity colonist) =>
        colonist.SimEffects.EffectProfiles.Where(p => p.KeyName.StartsWith("badStomach")).SelectMany(p => p.Effects).Sum(e => e.GetValue());

    /// <summary>
    /// The mod's and the food's calls in GoalEat.ConsumeStomachContents (its per-item delegate), in
    /// IL order: EmergencyEatingMod.* and Food.ConsumeBy, as "Type.Method".
    /// </summary>
    private static List<string> StomachCalls()
    {
        var calls = new List<(Type Holder, int At, string Name)>();
        Type goalEat = typeof(UWGame.SimSide.Items.Food).Assembly.GetType("UWGame.SimSide.AI.Goals.GoalEat", throwOnError: true)!; // internal
        var holders = new[] { goalEat }.Concat(goalEat.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public));
        foreach (Type holder in holders)
        {
            foreach (MethodInfo m in holder.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            {
                byte[] il = m.GetMethodBody()?.GetILAsByteArray();
                if (il == null) continue;
                var found = new List<(Type, int, string)>();
                for (int i = 0; i + 4 < il.Length; i++)
                {
                    if (il[i] != 0x28 && il[i] != 0x6F) continue; // call, callvirt
                    MethodBase target;
                    try { target = m.Module.ResolveMethod(BitConverter.ToInt32(il, i + 1)); }
                    catch (Exception) { continue; }
                    string name = target.DeclaringType?.Name + "." + target.Name;
                    if (name.StartsWith("EmergencyEatingMod.") || name == "Food.ConsumeBy") found.Add((holder, i, name));
                }
                if (found.Any(f => f.Item3 == "Food.ConsumeBy")) calls.AddRange(found);
            }
        }
        return calls.OrderBy(c => c.At).Select(c => c.Name).ToList();
    }

    /// <summary>An entity of a type, with the game's effect component and nothing else - enough for the mod's hooks.</summary>
    private static Entity Colonist(EntityType type)
    {
        var entity = (Entity)RuntimeHelpers.GetUninitializedObject(typeof(Entity));
        typeof(Entity).GetProperty("EntityType")!.SetValue(entity, type);
        typeof(Entity).GetField("id", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(entity, EntityID.Invalid);
        entity.Components = new Dictionary<Type, Component>();
        var effects = (SimEffectsComponent)RuntimeHelpers.GetUninitializedObject(typeof(SimEffectsComponent));
        effects.Parent = entity;
        effects.EffectProfiles = new List<SimEffectProfile>();
        effects.NumberEffects = new Dictionary<AffectsNumbers, List<SimEffect>>();
        effects.FlagEffects = new Dictionary<AffectsFlags, List<SimEffect>>();
        entity.Components.Add(typeof(SimEffectsComponent), effects);
        return entity;
    }
}
