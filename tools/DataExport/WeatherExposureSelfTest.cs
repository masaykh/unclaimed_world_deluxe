using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UWGame.Control.Replays;
using UWGame.SimSide;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.SimSide.Items;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.SimEffects;
using WM = UWGame.Mods.WeatherMod;

namespace UW.Tools.DataExport;

/// <summary>
/// --weather-selftest, step 3: COLD AND WET HURT.
///
/// Checked: the switch and the "Cold" effect, registered with the switch off; the exposure of a
/// night in the open, by a campfire and under a roof - the stormiest mid-winter night of thirty
/// years, the dry winter nights, the equinoxes, every summer night - and without RAIN AND WIND; the
/// real clock's night the same as the function's; the real GoalDoSleep hook's measuring, kept in the
/// colonist's CustomFields and taken off through the real Intelligence.Comfort, lapsing a day later;
/// the real EvaluateSleep.ScoreComfort scoring a hut over a campfire over the ground on a storm night,
/// and the studio's scores on a summer night or switched off; the seeded rolls for a cold, at the
/// asked rate, once a day, the same with the same seed, none in a hut or in summer and no draw then;
/// and the cold through the real Locomotor, ProcessJob and BiologicalEntity, alone and with a bad
/// stomach (the shared SicknessEffects).
///
/// NOT HERE. Whether a colonist in a running colony walks to a hut instead of lying down by the fire:
/// EvaluateSleep's path search and the goal's arbitration need a running site - that is for a game.
/// </summary>
internal static partial class Program
{
    private static void ColdAndWetSelfTest(Action<bool, string> Check, DateAndTime date, WeatherManager weather, UWGame.Mods.ModSetting enabled)
    {
        Console.WriteLine("  -- cold and wet hurt");
        var exposure = UWGame.Mods.ModSettings.Find("weather.exposure");
        var rain = UWGame.Mods.ModSettings.Find("weather.rain");
        Check(exposure != null && exposure.DefaultValue == "false" && exposure.AffectsSimulation && !exposure.TakesEffectOnNextLoad,
              "COLD AND WET HURT: off by default, a simulation setting that applies at once");
        if (exposure == null || rain == null) return;
        static string F(double v) => v.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
        static string Pct(double v) => FormattableString.Invariant($"{100.0 * v:0.#}%");

        // ------------------------------------------------------------ the "Cold" effect, registered with the switch off
        bool profile = GameData.Instance.AllEffectProfileTypes.TryGetValue(WM.ColdProfile, out EffectProfileType cold)
                       && cold.EffectTypes?.Count == 1 && cold.EffectTypes[0] is NumberEffectType n && n.Affects == AffectsNumbers.AgentComfort
                       && n.Operator == NumberEffectOperator.Add && UWGame.Common.IsEqual(n.Intensity, -0.1f) && n.DurationInDays == 1.5
                       && cold.Name == "Cold" && cold.Description != null;
        Check(profile, "\"Cold\" is in the tables though the switch was off: comfort -10% for 1.5 days - \"" + cold?.Description + "\"");
        if (!profile) return;

        // ------------------------------------------------------------ the numbers: a night, by place
        const int seed = 20261017;
        static bool IsWinter(long d) { int x = (int)(((d % 12) + 12) % 12); return x == 10 || x == 11 || x == 0; }
        static bool IsSummer(long d) { int x = (int)(((d % 12) + 12) % 12); return x >= 4 && x <= 6; }
        const double Bedtime = 0.875; // 21:00
        float Night(long day, bool withRain)
        {
            double days = day + Bedtime;
            int y = (int)Math.Floor(days / 12.0);
            return WM.NightExposure(seed, y, days / 12.0 - y, Bedtime, withRain, WM.StudioCloudCover);
        }
        float NightRain(long day) => Enumerable.Range(0, WM.NightHours).Max(h => WM.Scheduled(seed, day + Bedtime + (h + 0.5) / 24.0).Rain);
        long stormDay = -1;
        float storm = 0f, dryLo = float.MaxValue, dryHi = 0f, equinoxHi = 0f, summerHi = 0f, dryNoRainLo = float.MaxValue, dryNoRainHi = 0f;
        int dryNights = 0, equinoxNights = 0, equinoxRisky = 0;
        for (long d = 16 * 12; d < 46 * 12; d++)
        {
            int name = (int)(d % 12);
            if (IsWinter(d))
            {
                float e = Night(d, true);
                if (e > storm) { storm = e; stormDay = d; }
                if (NightRain(d) == 0f)
                {
                    dryNights++;
                    dryLo = Math.Min(dryLo, e);
                    dryHi = Math.Max(dryHi, e);
                }
                float still = Night(d, false);
                dryNoRainLo = Math.Min(dryNoRainLo, still);
                dryNoRainHi = Math.Max(dryNoRainHi, still);
            }
            else if (IsSummer(d)) summerHi = Math.Max(summerHi, Math.Max(Night(d, true), Night(d, false)));
            else if (name == 2 || name == 8)
            {
                float e = Night(d, true);
                equinoxHi = Math.Max(equinoxHi, e);
                equinoxNights++;
                if (e >= WM.ColdFromExposure) equinoxRisky++;
            }
        }
        float campfire = WM.Sheltered(storm, WM.Shelter.Campfire), roof = WM.Sheltered(storm, WM.Shelter.Roof);
        Check(storm >= 0.75f && storm <= 1f && NightRain(stormDay) >= 0.7f,
              "the stormiest mid-winter night of thirty years (day " + stormDay % 12 + " of year " + stormDay / 12 + ", rain up to " + F(NightRain(stormDay)) + "): exposure " + F(storm) + " in the open");
        Check(campfire == storm * 0.5f && roof == 0f, "  by a prepared campfire " + F(campfire) + ", in a hut " + F(roof));
        Check(dryNights > 20 && dryLo >= 0.2f && dryHi <= 0.5f, dryNights + " dry winter nights in the open: " + F(dryLo) + "-" + F(dryHi) + " (the cold and the wind)");
        Check(dryNoRainLo >= 0.25f && dryNoRainHi <= 0.4f, "  without RAIN AND WIND (dry, the studio's still air): " + F(dryNoRainLo) + "-" + F(dryNoRainHi) + " - the 5-6 C winter nights alone");
        Check(equinoxRisky < 0.2 * equinoxNights && equinoxHi < 0.5f * storm,
              "\"Spring\" and \"Mid autumn\": " + equinoxRisky + " of " + equinoxNights + " nights (the rainy ones, 11 C less the rain and wind) reach exposure " + F(WM.ColdFromExposure) + ", at most " + F(equinoxHi));
        Check(summerHi == 0f, "every summer night: " + F(summerHi) + " - nothing below 12 C");
        Check(WM.ExposureOf(273f + 12f, 1f, 20f) == 0f && WM.ExposureOf(273f + 2f, 0f, 0f) == 0.5f && WM.ExposureOf(273f + 2f, 1f, 15f) == 1f
              && UWGame.Common.IsEqual(WM.ExposureOf(273f + 7f, 0.5f, 9f), 0.5f * (0.5f + 0.15f + 0.1f)),
              "an hour: none at 12 C whatever the storm; 0.5 dry and still at 2 C; 1 at 2 C in a full storm at 15 m/s");
        Check(WM.ColdChance(storm) > 0.2f && WM.ColdChance(campfire) > 0.1f && WM.ColdChance(0.24f) == 0f && UWGame.Common.IsEqual(WM.ColdChance(1f), 0.3f),
              "the chance of a cold: " + Pct(WM.ColdChance(storm)) + " after the storm night out, " + Pct(WM.ColdChance(campfire)) + " by the fire, " +
              Pct(WM.ColdChance(dryLo)) + "-" + Pct(WM.ColdChance(dryHi)) + " on a dry winter night out, none below exposure 0.25");

        // ------------------------------------------------------------ the real clock
        var generator = typeof(Sim).GetField("gameplayRandomGenerator", BindingFlags.Instance | BindingFlags.NonPublic)!;
        // Typed Client so that a draw does not go to the replay recorder, which needs a running controller; the draws are the same.
        generator.SetValue(UWGame.The.Sim, new RandomGenerator(seed, RandomGenerator.GeneratorType.Client));
        var calls = typeof(RandomGenerator).GetField("numberOfCalls", BindingFlags.Instance | BindingFlags.NonPublic)!;
        int Draws() => (int)calls.GetValue(UWGame.The.Sim.GameplayRandomGenerator)!;
        void At(long day, double timeOfDay)
        {
            double days = day + timeOfDay;
            date.Year = (int)Math.Floor(days / 12.0);
            date.TimeOfYear = days / 12.0 - date.Year;
            date.TimeOfDay = timeOfDay;
            date.Day = (int)(day % 12) + 1;
            date.AdvanceTime(0.0);
        }
        enabled.Value = "true";
        rain.Value = "true";
        exposure.Value = "true";
        At(stormDay, Bedtime);
        float tonight = WM.TonightExposure() ?? -1f;
        Check(tonight == WM.NightExposure(seed, date.Year, date.TimeOfYear, date.TimeOfDay, true, weather.CloudCover) && Math.Abs(tonight - storm) < 1e-4f,
              "the real clock at 21:00 on that day: tonight's exposure " + F(tonight) + ", the function's to the bit");

        // ------------------------------------------------------------ the real hooks: GoalDoSleep's measuring, Intelligence.Comfort
        EntityType human = GameData.Instance.AllEntityTypes.Values.First(t => t.Person != null && t.BiologicalType != null);
        EntityType hutType = GameData.Instance.AllEntityTypes.Values.FirstOrDefault(t => t.ContainerType?.ResidenceType != null);
        EntityType fireType = GameData.Instance.AllEntityTypes.Values.FirstOrDefault(t => t.GatheringSiteType != null && t.ToolType != null);
        Check(hutType != null && fireType != null, "a residence (" + hutType?.KeyName + ") and a gathering site that is prepared (" + fireType?.KeyName + ")");
        if (hutType == null || fireType == null) return;
        Entity hut = Colonist(hutType);
        Entity fire = Colonist(fireType);
        var tool = (Tool)RuntimeHelpers.GetUninitializedObject(typeof(Tool));
        tool.Parent = fire;
        fire.Components.Add(typeof(Tool), tool);
        Entity coldFire = Colonist(fireType);
        var coldTool = (Tool)RuntimeHelpers.GetUninitializedObject(typeof(Tool));
        coldTool.Parent = coldFire;
        coldFire.Components.Add(typeof(Tool), coldTool);
        SetField(tool, "isPrepared", (bool?)true);
        SetField(coldTool, "isPrepared", (bool?)false);
        Check(WM.ShelterOf(null) == WM.Shelter.Open && WM.ShelterOf(fire) == WM.Shelter.Campfire && WM.ShelterOf(coldFire) == WM.Shelter.Open && WM.ShelterOf(hut) == WM.Shelter.Roof,
              "the studio's classes of place: the ground and an unprepared gathering site are the open, a prepared one a campfire, a residence a roof");

        Entity Sleeper()
        {
            Entity e = Colonist(human);
            var mind = (Intelligence)RuntimeHelpers.GetUninitializedObject(typeof(Intelligence));
            mind.Parent = e;
            e.Components.Add(typeof(Intelligence), mind);
            return e;
        }
        float Effects(Entity e) => e.GetEffect(AffectsNumbers.AgentComfort, 0f);
        void Forget(Entity e)
        {
            e.CustomFields = null;
            foreach (var p in e.SimEffects.EffectProfiles.Select(p => p.EffectProfileType).ToList()) e.SimEffects.Remove(p);
        }
        Entity outside = Sleeper(), byFire = Sleeper(), inHut = Sleeper();

        exposure.Value = "false";
        int before = Draws();
        UWGame.Mods.WeatherMod.OnSleepStart(outside, null);
        Check(outside.CustomFields == null && Draws() == before && outside.Intelligence.Comfort == Effects(outside),
              "switched off, the storm night out: nothing kept, nothing drawn, Intelligence.Comfort the studio's");
        exposure.Value = "true";
        UWGame.Mods.WeatherMod.OnSleepStart(outside, null);
        UWGame.Mods.WeatherMod.OnSleepStart(byFire, fire);
        before = Draws();
        UWGame.Mods.WeatherMod.OnSleepStart(inHut, hut);
        float lostOut = Effects(outside) - outside.Intelligence.Comfort, lostFire = Effects(byFire) - byFire.Intelligence.Comfort, lostHut = Effects(inHut) - inHut.Intelligence.Comfort;
        Check(UWGame.Common.IsEqual(lostOut, 0.2f * storm) && lostOut > 0.15f,
              "now, through the real Intelligence.Comfort: a storm night out costs " + Pct(lostOut) + " comfort the next day (0.2 x " + F(storm) + ")");
        Check(UWGame.Common.IsEqual(lostFire, lostOut / 2f) && lostHut == 0f && Draws() == before,
              "  by the campfire " + Pct(lostFire) + "; in the hut nothing, and nothing drawn for a cold");
        Check(outside.CustomFields.ContainsKey(WM.ExposureDayKey) && outside.CustomFields.ContainsKey(WM.ExposureKey),
              "  kept in the colonist's CustomFields, which the save carries");
        UWGame.Mods.WeatherMod.OnSleepStart(outside, hut);
        Check(UWGame.Common.IsEqual(Effects(outside) - outside.Intelligence.Comfort, lostOut), "  moved to a hut later the same day: the night out still counts (the worst of the day)");
        At(stormDay + 1, 0.5);
        Check(UWGame.Common.IsEqual(Effects(outside) - outside.Intelligence.Comfort, lostOut), "  it holds the next day");
        At(stormDay + 2, 0.5);
        Check(outside.Intelligence.Comfort == Effects(outside), "  and lapses the day after, with no sleep between");
        At(stormDay, Bedtime);
        exposure.Value = "false";
        Check(outside.Intelligence.Comfort == Effects(outside), "  switched off: none of it");
        exposure.Value = "true";

        // ------------------------------------------------------------ the real EvaluateSleep.ScoreComfort
        var evaluator = (EvaluateSleep)RuntimeHelpers.GetUninitializedObject(typeof(EvaluateSleep));
        SetField(evaluator, "entity", Sleeper());
        SetField(evaluator, "MaxSleepIncreasePerDay", GameData.Instance.Constants.SleepNeed.GainPerDayWhenSleeping * GameData.Instance.Constants.SleepNeed.MaximumSleepNeedGainFactorForPeople);
        MethodInfo scoreComfort = typeof(EvaluateSleep).GetMethod("ScoreComfort", BindingFlags.Instance | BindingFlags.NonPublic)!;
        IKnownEntityData Fact(EntityType type, bool? prepared)
        {
            var fact = (MemoryFact)RuntimeHelpers.GetUninitializedObject(typeof(MemoryFact));
            fact.EntityType = type;
            fact.IsPrepared = prepared;
            fact.Condition = 1.0;
            return fact;
        }
        IKnownEntityData hutFact = Fact(hutType, null), fireFact = Fact(fireType, true);
        double[] Scores() => new[] { (double)scoreComfort.Invoke(evaluator, new object[] { null })!, (double)scoreComfort.Invoke(evaluator, new object[] { fireFact })!, (double)scoreComfort.Invoke(evaluator, new object[] { hutFact })! };
        string Show(double[] s) => "ground " + F(s[0]) + ", campfire " + F(s[1]) + ", hut " + F(s[2]);
        exposure.Value = "false";
        double[] stock = Scores();
        exposure.Value = "true";
        double[] stormy = Scores();
        Check(stormy[2] == stock[2] && stormy[1] < stock[1] && stormy[0] < stock[0] && stormy[0] < stormy[1] && stormy[1] < stormy[2],
              "EvaluateSleep.ScoreComfort on the storm night: " + Show(stormy) + " (studio's: " + Show(stock) + ") - a hut, then a fire, then the ground");
        Check(Math.Abs(stormy[0] / stock[0] - (1.0 - 0.5 * storm)) < 1e-6, "  the ground loses half its exposure: x" + F(stormy[0] / stock[0]));
        long summerDay = 20 * 12 + 5;
        At(summerDay, Bedtime);
        Check(Scores().SequenceEqual(stock), "  a mid-summer night: the studio's scores, to the bit");
        At(stormDay, Bedtime);

        // ------------------------------------------------------------ a cold: the seeded rolls
        int Colds(Entity where, int nights)
        {
            int got = 0;
            Entity s = Sleeper();
            for (int i = 0; i < nights; i++)
            {
                Forget(s);
                UWGame.Mods.WeatherMod.OnSleepStart(s, where);
                if (WM.HasCold(s)) got++;
            }
            return got;
        }
        const int Nights = 4000;
        int outColds = Colds(null, Nights), fireColds = Colds(fire, Nights);
        double outRate = (double)outColds / Nights, fireRate = (double)fireColds / Nights;
        Check(Math.Abs(outRate - WM.ColdChance(storm)) < 0.025, "the storm night out, " + Nights + " times: " + Pct(outRate) + " caught a cold (asked " + Pct(WM.ColdChance(storm)) + ")");
        Check(Math.Abs(fireRate - WM.ColdChance(campfire)) < 0.02, "  by the campfire: " + Pct(fireRate) + " (asked " + Pct(WM.ColdChance(campfire)) + ")");
        before = Draws();
        Check(Colds(hut, 500) == 0 && Draws() == before, "  in the hut: none of 500, and nothing drawn");
        At(summerDay, Bedtime);
        before = Draws();
        Check(Colds(null, 500) == 0 && Draws() == before, "  a summer night out: none, and nothing drawn - the game's random stream is untouched");
        At(stormDay, Bedtime);
        string Pattern()
        {
            generator.SetValue(UWGame.The.Sim, new RandomGenerator(seed, RandomGenerator.GeneratorType.Client));
            Entity s = Sleeper();
            var b = new System.Text.StringBuilder();
            for (int i = 0; i < 200; i++)
            {
                Forget(s);
                UWGame.Mods.WeatherMod.OnSleepStart(s, null);
                b.Append(WM.HasCold(s) ? '1' : '0');
            }
            return b.ToString();
        }
        string p1 = Pattern(), p2 = Pattern();
        Check(p1 == p2 && p1.Contains('1') && p1.Contains('0'), "the same seed, the same colds: 200 nights alike (" + p1.Count(c => c == '1') + " colds)");
        Entity twice = Sleeper();
        before = Draws();
        UWGame.Mods.WeatherMod.OnSleepStart(twice, null);
        UWGame.Mods.WeatherMod.OnSleepStart(twice, null);
        Check(Draws() == before + 1, "lying down twice the same day: one roll");
        Entity sick = Sleeper();
        sick.SimEffects.Start(cold);
        before = Draws();
        UWGame.Mods.WeatherMod.OnSleepStart(sick, null);
        Check(Draws() == before && sick.SimEffects.EffectProfiles.Count(p => p.KeyName == WM.ColdProfile) == 1, "with a cold already: no roll, it does not stack or restart");
        rain.Value = "false";
        At(stormDay - stormDay % 12 + 11, Bedtime); // the middle of "Mid winter"'s evening, dry and still
        float stillNight = WM.TonightExposure() ?? -1f;
        Check(stillNight >= 0.3f && stillNight <= 0.4f && Colds(null, 2000) > 0,
              "without RAIN AND WIND, a \"Mid winter\" night out: exposure " + F(stillNight) + ", and colds still come (" + Pct(WM.ColdChance(stillNight)) + ")");
        rain.Value = "true";
        enabled.Value = "false";
        Check(!WM.ExposureOn && WM.ShelterScore(Sleeper(), null, 0.61) == 0.61, "COLD AND WET HURT needs SEASONS AND TEMPERATURE: with that off, nothing");
        enabled.Value = "true";

        // ------------------------------------------------------------ the cold through the studio's own code (SicknessEffects)
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
        Entity worker = Sleeper(); // no body: the studio's energy factor stays 1, so the work time shows the hook alone
        float Speed() => walker.Locomotor.CalculateSpeed(null, calculateWithTerrain: false, calculateWithBurden: false);
        float Work() => ProcessJob.CalculateWorkDurationInDays(worker, 1f, null);
        float Energy()
        {
            SetField(body, "energyLevelIsDirty", true);
            return body.EnergyLevel;
        }
        float speed0 = Speed(), work0 = Work(), energy0 = Energy();
        walker.SimEffects.Start(cold);
        worker.SimEffects.Start(cold);
        float speed1 = Speed(), work1 = Work(), energy1 = Energy();
        Check(speed0 > 0f && UWGame.Common.IsEqual(speed1, 0.9f * speed0) && UWGame.Common.IsEqual(work1, 1.2f * work0) && energy0 == 1f && energy1 == 0.75f
              && UWGame.Common.IsEqual(Effects(walker), -0.1f),
              FormattableString.Invariant($"a cold, through Locomotor.CalculateSpeed {speed0} -> {speed1}, ProcessJob.CalculateWorkDurationInDays {work0} -> {work1}, BiologicalEntity.EnergyLevel {energy0} -> {energy1}, comfort -10%"));
        EffectProfileType stomach = GameData.Instance.AllEffectProfileTypes["badStomach5"];
        walker.SimEffects.Start(stomach);
        worker.SimEffects.Start(stomach);
        float speed2 = Speed(), work2 = Work(), energy2 = Energy();
        Check(UWGame.Common.IsEqual(speed2, 0.9f * 0.8f * speed0) && UWGame.Common.IsEqual(work2, 1.2f * 1.3f * work0) && energy2 == 0.6f,
              FormattableString.Invariant($"  and a bad stomach from raw meat on top (EmergencyEatingMod, the same SicknessEffects): walks x{speed2 / speed0:0.00}, works x{work2 / work0:0.00}, energy {energy2} - multiplied, the lower cap"));
        exposure.Value = "false";
        Check(UWGame.Common.IsEqual(Speed(), speed2), "  switched off, the cold the panel shows still applies until it expires");
        Forget(walker);
        Forget(worker);
        Check(Speed() == speed0 && Work() == work0 && Energy() == energy0, "  and once it is gone, the studio's speed, work time and energy, to the bit");

        exposure.Value = exposure.DefaultValue;
        rain.Value = rain.DefaultValue;
    }
}
