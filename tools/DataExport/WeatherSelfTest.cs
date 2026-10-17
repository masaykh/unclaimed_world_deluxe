using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using UWGame.Control.Replays;
using UWGame.SimSide;
using UWGame.SimSide.AI;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Items;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Overland;

namespace UW.Tools.DataExport;

/// <summary>
/// --weather-selftest: WeatherMod, step 1 - seasons and temperature.
///
/// WHAT. Kastuk, "Weather and temperature", 2026-10-17: subtropical seasons, "Not freezing
/// temperatures, but still cold enough for sickness. Then high solar heat during usually hot
/// summer." Checked against the game's own code, stepped with no window:
/// - the sun: the real DateAndTime, advanced through a year, with the switch off has its highest
///   noon on "Mid winter" (the studio's sawtooth) and with it on on "Mid summer", lowest on "Mid
///   winter", with no jump at the new year; WeatherMod's own copy of the sun agrees with it;
/// - the air: over the same year, winter nights 5 C and days 8-14 C, summer nights 17 C and noons
///   25-35 C, never freezing, the same numbers on a second run;
/// - spoilage: the real NonLivingEntity.ComputeDegradeDamage on a one-tile map - faster in summer
///   than at the studio's 15 C, slower in winter, "isolated" storage following, a fridge not, a
///   tile the studio's DecompositionTest set keeping its own temperature;
/// - shelf-life tooltips: DegradeType.PostDataCompleteInitialize works "Outside" out for the
///   mid-summer day's average instead of 15 C.
///
/// Step 2, RAIN AND WIND (<see cref="RainAndWindSelfTest"/>):
/// - the schedule: the same seed gives the same weather to the bit, another seed other weather;
/// - the climate over ten seeds of thirty years: winter days mostly have a rain spell and nearly
///   every winter a rainstorm, with windy clear spells between; summer mostly dry; the equinoxes
///   between; the wind in the studio's Beaufort terms, light in summer, gales in winter storms,
///   never "Storm";
/// - the real WeatherManager.Update steered by it through a winter storm, without a draw from the
///   game's random stream; its WEATHER NOW line (to a stand-in interface) saying "Rainstorm"; the
///   air cooled 3-6 C and never freezing; a game loaded mid-storm the same on its first frame;
///   switching off giving back the studio's 1.5 m/s;
/// - the forecast for tomorrow against the day that then comes, through the real clock.
///
/// NOT HERE. The date panel itself: SetTemp, SetWeatherAhead and SetTempAhead need the game's
/// interface (see the mod's summary).
/// </summary>
internal static partial class Program
{
    private static int WeatherSelfTest()
    {
        UWGame.The.Sim = new Sim { Mode = Sim.EngineMode.Game };
        typeof(Sim).GetMethod("CreateLookupCollections", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(UWGame.The.Sim, null);
        int rc = Run(Sim.SerializeMode.NoSerialize, "base tables, the way the game loads them");
        if (rc != 0) return rc;
        if (!ValidateDataComplete()) return 1;
        Console.WriteLine("==> weather self-test");
        UWGame.Mods.WeatherMod.RegisterSettings();
        var enabled = UWGame.Mods.ModSettings.Find("weather.enabled");
        if (enabled == null)
        {
            Console.WriteLine("  the mod is not in this build - nothing to check");
            return 0;
        }
        int failures = 0;
        void Check(bool ok, string what)
        {
            Console.WriteLine((ok ? "  ok    " : "  FAIL  ") + what);
            if (!ok) failures++;
        }
        static string C(float kelvin) => FormattableString.Invariant($"{kelvin - 273f:0.0} C");
        static string Deg(float radians) => FormattableString.Invariant($"{MathHelper.ToDegrees(radians):0.0} deg");

        Check(enabled.DefaultValue == "false" && enabled.AffectsSimulation && !enabled.TakesEffectOnNextLoad,
              "off by default, a simulation setting that applies at once");

        // ------------------------------------------------------------ the sun, through the real DateAndTime
        var date = new DateAndTime();
        UWGame.The.Sim.DateAndTime = date;
        string[] names = (string[])typeof(DateAndTime).GetField("seasonStrings", BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null)!;
        Check(names.Length == 12 && names[5] == "Mid summer" && names[11] == "Mid winter",
              "the calendar's 12 days: \"" + names[5] + "\" is the 6th, \"" + names[11] + "\" the 12th");
        float ElevationAt(double timeOfYear, double timeOfDay)
        {
            date.TimeOfYear = timeOfYear;
            date.TimeOfDay = timeOfDay;
            date.AdvanceTime(0.0);
            return date.SunElevation;
        }
        float[] Noons() => Enumerable.Range(0, 12).Select(d => ElevationAt((d + 0.5) / 12.0, 0.5)).ToArray();
        static int ArgMax(float[] a) => Array.IndexOf(a, a.Max());
        static int ArgMin(float[] a) => Array.IndexOf(a, a.Min());

        enabled.Value = "false";
        float[] stock = Noons();
        float stockJump = Math.Abs(ElevationAt(0.9999, 0.5) - ElevationAt(0.0001, 0.5));
        Check(ArgMax(stock) == 11 && ArgMin(stock) == 0,
              "before (switch off, the studio's sawtooth): the sun is highest on \"" + names[ArgMax(stock)] + "\" and lowest on \"" + names[ArgMin(stock)] + "\"");
        Check(stockJump > 0.5f, "  and drops " + Deg(stockJump) + " at noon overnight into the new year");

        enabled.Value = "true";
        float[] noons = Noons();
        float jump = Math.Abs(ElevationAt(0.9999, 0.5) - ElevationAt(0.0001, 0.5));
        Check(ArgMax(noons) == 5 && ArgMin(noons) == 11,
              "now: highest on \"" + names[ArgMax(noons)] + "\", lowest on \"" + names[ArgMin(noons)] + "\"");
        Check(Math.Abs(noons[5] - MathHelper.ToRadians(67.5f)) < 0.01f && Math.Abs(noons[11] - MathHelper.ToRadians(22.5f)) < 0.01f,
              "  noon at " + Deg(noons[5]) + " in mid summer, " + Deg(noons[11]) + " in mid winter (latitude 45, tilt 22.5)");
        Check(Math.Abs(noons[2] - noons[8]) < 0.001f && Math.Abs(noons[2] - MathHelper.ToRadians(45f)) < 0.01f,
              "  \"" + names[2] + "\" and \"" + names[8] + "\" are the equinoxes: noon at " + Deg(noons[2]));
        Check(jump < 0.01f, "  no jump at the new year (" + Deg(jump) + ")");
        float worst = 0f;
        for (int i = 0; i < 12 * 24; i++)
        {
            double toy = (i + 0.37) / (12 * 24), tod = (i * 7 % 24 + 0.5) / 24.0;
            worst = Math.Max(worst, Math.Abs(ElevationAt(toy, tod) - UWGame.Mods.WeatherMod.SunElevation(toy, tod)));
        }
        Check(worst < 1e-4f, "  WeatherMod's own sun (for the shelf-life tooltips) is DateAndTime's to " + Deg(worst));

        // ------------------------------------------------------------ the air over a year
        var weather = new WeatherManager();
        var site = (Site)RuntimeHelpers.GetUninitializedObject(typeof(Site));
        site.PlaySite = (PlaySite)RuntimeHelpers.GetUninitializedObject(typeof(PlaySite));
        site.Entities = new ObservableList<Entity>(); // DateAndTime turns the trees to night through it
        site.PlaySite.Weather = weather;
        UWGame.The.Sim.PlaySite = site;
        Check(weather.CloudCover == UWGame.Mods.WeatherMod.StudioCloudCover, FormattableString.Invariant($"a new WeatherManager has the studio's cloud cover, {weather.CloudCover}"));

        const int stepsPerDay = 96;
        float[] Year(out float[] min, out float[] max)
        {
            date.TimeOfYear = 0.0;
            date.TimeOfDay = 0.0;
            date.AdvanceTime(0.0);
            var all = new float[12 * stepsPerDay];
            min = Enumerable.Repeat(float.MaxValue, 12).ToArray();
            max = Enumerable.Repeat(float.MinValue, 12).ToArray();
            for (int i = 0; i < all.Length; i++)
            {
                int day = Math.Min(11, (int)(date.TimeOfYear * 12));
                float t = UWGame.Mods.WeatherMod.CurrentAirTemperature() ?? 0f;
                all[i] = t;
                min[day] = Math.Min(min[day], t);
                max[day] = Math.Max(max[day], t);
                date.AdvanceTime(date.SecondsPerDay / stepsPerDay);
            }
            return all;
        }
        float[] year = Year(out float[] lo, out float[] hi);
        Check(Math.Abs(lo[11] - 278f) < 0.5f && Math.Abs(lo[5] - 290f) < 0.5f,
              "nights: " + C(lo[11]) + " in mid winter, " + C(lo[5]) + " in mid summer");
        Check(hi[11] >= 281f && hi[11] <= 287f, "a mid-winter day reaches " + C(hi[11]) + " (8-14 C asked)");
        Check(hi[5] >= 298f && hi[5] <= 308f, "a mid-summer noon reaches " + C(hi[5]) + " (25-35 C asked)");
        Check(year.Min() > 273f, "never freezing: the coldest of the year is " + C(year.Min()));
        Check(Math.Abs(hi[2] - hi[8]) < 0.2f && Math.Abs(lo[2] - lo[8]) < 0.2f && lo[2] > lo[11] && lo[2] < lo[5],
              "spring and autumn alike, between the two: " + C(lo[2]) + " to " + C(hi[2]));
        float overcast = UWGame.Mods.WeatherMod.AirTemperature(UWGame.Mods.WeatherMod.MidSummer, noons[5], 1f);
        float clear = UWGame.Mods.WeatherMod.AirTemperature(UWGame.Mods.WeatherMod.MidSummer, noons[5], 0f);
        Check(overcast < hi[5] && clear > hi[5], "cloud cools it: a mid-summer noon " + C(overcast) + " overcast, " + C(clear) + " clear");
        Check(Year(out _, out _).SequenceEqual(year), "a second year gives the same numbers, to the bit (no randomness)");
        enabled.Value = "false";
        Check(UWGame.Mods.WeatherMod.OutsideTemperature(288f) == 288f, "switch off: the studio's 15 C");
        enabled.Value = "true";

        // ------------------------------------------------------------ spoilage through ComputeDegradeDamage
        var map = (MapManager)RuntimeHelpers.GetUninitializedObject(typeof(MapManager));
        map.mapTileWidth = 1;
        map.mapTileHeight = 1;
        var tile = new TerrainTile { TilePos = new TilePos(0, 0) };
        map.TileMap = new[] { new[] { tile } };
        UWGame.The.Map = map;
        float mean = GameData.Instance.Constants.MeanAmbientTemperature;
        tile.Temperature = mean;
        var food = GameData.Instance.AllEntityTypes.Values
            .Where(t => t.ItemType?.FoodType != null && t.NonLivingType?.FinalDegradeType?.TemperatureDamage != null)
            .FirstOrDefault(t =>
            {
                var curve = t.NonLivingType.FinalDegradeType.TemperatureDamage;
                float Dmg(float k) => UWGame.Common.GetInterpolatedFunctionValue(k, curve);
                return Dmg(305f) > Dmg(288f) && Dmg(288f) > Dmg(279f);
            });
        Check(food != null, "a food whose spoilage rises with temperature: " + food?.KeyName);
        if (food == null) return 1;
        DegradeType degrade = food.NonLivingType.FinalDegradeType;
        var item = (MemoryFact)RuntimeHelpers.GetUninitializedObject(typeof(MemoryFact));
        item.EntityType = food;
        weather.SunIntensity = 0f; // light spoils too; hold it still so only the temperature differs
        float Spoil(double timeOfYear, double timeOfDay, StorageCondition storedIn = null, bool? powered = null)
        {
            date.TimeOfYear = timeOfYear;
            date.TimeOfDay = timeOfDay;
            date.AdvanceTime(0.0);
            return NonLivingEntity.ComputeDegradeDamage(item, storedIn, powered, new Point(0, 0), 1.0);
        }
        float AtTemperature(float kelvin, StorageCondition storedIn = null, bool? powered = null) =>
            NonLivingEntity.ComputeDegradeDamage(degrade, storedIn, false, kelvin, 0f, powered, 1.0);
        double summer = UWGame.Mods.WeatherMod.MidSummer, winter = summer + 0.5;

        enabled.Value = "false";
        Check(Spoil(summer, 0.5) == AtTemperature(mean) && Spoil(winter, 0.0) == AtTemperature(mean),
              "before (switch off): a day outside costs " + AtTemperature(mean).ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + " at 15 C, summer or winter");
        enabled.Value = "true";
        float summerNoon = Spoil(summer, 0.5), summerNoonAir = UWGame.Mods.WeatherMod.CurrentAirTemperature()!.Value;
        float winterNight = Spoil(winter, 0.0), winterNightAir = UWGame.Mods.WeatherMod.CurrentAirTemperature()!.Value;
        Check(summerNoon == AtTemperature(summerNoonAir) && summerNoon > AtTemperature(mean),
              "now: a mid-summer noon outside spoils it at " + C(summerNoonAir) + ": " + summerNoon.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + " a day");
        Check(winterNight == AtTemperature(winterNightAir) && winterNight < AtTemperature(mean),
              "  a mid-winter night at " + C(winterNightAir) + ": " + winterNight.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + " a day");
        var conditions = GameData.Instance.AllStorageConditions;
        StorageCondition isolated = conditions["isolated"], fridge = conditions["refrigerator"];
        Check(Spoil(summer, 0.5, isolated, false) == AtTemperature(summerNoonAir, isolated, false)
              && Spoil(summer, 0.5, isolated, false) > Spoil(winter, 0.0, isolated, false),
              "  \"isolated\" storage follows the air: " + C(isolated.GetTemperature(false, summerNoonAir)) + " inside at a mid-summer noon");
        Check(Spoil(summer, 0.5, fridge, true) == Spoil(winter, 0.0, fridge, true),
              "  a working fridge does not (" + C(fridge.GetTemperature(true, summerNoonAir)) + " all year)");
        tile.Temperature = 255f;
        Check(Spoil(summer, 0.5) == AtTemperature(255f), "  a tile set to 255 K (the studio's DecompositionTest) keeps it");
        tile.Temperature = mean;

        // ------------------------------------------------------------ shelf-life tooltips
        float Outside()
        {
            return degrade.StorageDurations.First(d => d.StorageDurationToDisplay.DisplayName == "Outside").Duration;
        }
        float stockDays = Outside();
        float tooltipAir = UWGame.Mods.WeatherMod.ShelfLifeTemperature(mean);
        Check(Math.Abs(tooltipAir - 296.5f) < 0.5f, "the tooltips' outside temperature, the mid-summer day's average: " + C(tooltipAir));
        date.TimeOfYear = summer;
        date.TimeOfDay = 0.0;
        date.AdvanceTime(0.0);
        double sum = 0.0;
        for (int i = 0; i < stepsPerDay; i++)
        {
            sum += UWGame.Mods.WeatherMod.CurrentAirTemperature()!.Value;
            date.TimeOfDay = (i + 1.0) / stepsPerDay;
            date.AdvanceTime(0.0);
        }
        Check(Math.Abs(sum / stepsPerDay - tooltipAir) < 0.2, FormattableString.Invariant($"  which is what the real day averages: {sum / stepsPerDay - 273:0.0} C"));
        degrade.StorageDurations.Clear();
        degrade.PostDataCompleteInitialize();
        float nowDays = Outside();
        Check(nowDays < stockDays,
              FormattableString.Invariant($"{food.KeyName} keeps {nowDays:0.0} days outside in the tooltip, {stockDays:0.0} at the studio's 15 C"));
        enabled.Value = "false";
        degrade.StorageDurations.Clear();
        degrade.PostDataCompleteInitialize();
        Check(Outside() == stockDays, "  and switched off, the studio's figure again");

        Check(UWGame.Mods.WeatherMod.Celsius(306.6f) == 34 && UWGame.Mods.WeatherMod.Celsius(272.6f) == 0 && UWGame.Mods.WeatherMod.Celsius(271.4f) == -2,
              "the date panel's whole degrees: 306.6 K is 34, 272.6 K is 0");

        RainAndWindSelfTest(Check, date, weather, enabled);

        enabled.Value = enabled.DefaultValue;
        Console.WriteLine(failures == 0 ? "weather self-test OK" : $"weather self-test FAILED - {failures} check(s)");
        return failures == 0 ? 0 : 1;
    }

    /// <summary>
    /// --weather-selftest, step 2: RAIN AND WIND. The schedule from a seed; a climate over many
    /// years; the real WeatherManager.Update steered by it, its WEATHER NOW line; the air it cools;
    /// the forecast against the day that then comes.
    /// </summary>
    private static void RainAndWindSelfTest(Action<bool, string> Check, DateAndTime date, WeatherManager weather, UWGame.Mods.ModSetting enabled)
    {
        Console.WriteLine("  -- rain and wind");
        var rain = UWGame.Mods.ModSettings.Find("weather.rain");
        Check(rain != null && rain.DefaultValue == "false" && rain.AffectsSimulation && !rain.TakesEffectOnNextLoad,
              "RAIN AND WIND: off by default, a simulation setting that applies at once");
        if (rain == null) return;
        enabled.Value = "true";
        static string C(float kelvin) => FormattableString.Invariant($"{kelvin - 273f:0.0} C");
        static string F(double v) => v.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
        static string Pct(double v) => FormattableString.Invariant($"{100.0 * v:0}%");

        const int seed = 20261017;
        typeof(Sim).GetField("gameplayRandomGenerator", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(UWGame.The.Sim, new RandomGenerator(seed, RandomGenerator.GeneratorType.Sim));
        Check(UWGame.Mods.WeatherMod.Seed == seed, "the schedule's seed is the game's: the gameplay generator's RandomSeed, " + UWGame.Mods.WeatherMod.Seed);

        // The real WeatherManager, stepped with the real clock. Its regulator runs on the sim's unpaused
        // time, which stands still here, so the studio's random rolls do not fire; the WEATHER NOW line
        // goes to a stand-in for the interface.
        UWGame.The.Sim.CycleManager = new UWGame.SimSide.Systems.TimeSlicing.CycleManager();
        var line = (WeatherLine)RuntimeHelpers.GetUninitializedObject(typeof(WeatherLine));
        UWGame.The.InGameUI = line;
        var frame = new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(1.0 / 60.0));
        void GoTo(int year, double timeOfYear, double timeOfDay)
        {
            date.Year = year;
            date.TimeOfYear = timeOfYear;
            date.TimeOfDay = timeOfDay;
            date.AdvanceTime(0.0);
            weather.Update(frame);
        }
        void Step(double days)
        {
            date.AdvanceTime(days * date.SecondsPerDay);
            weather.Update(frame);
        }
        double Days() => UWGame.Mods.WeatherMod.ScheduleDays(date.Year, date.TimeOfYear);
        bool IsWinter(double days) { int d = (int)(((days % 12) + 12) % 12); return d == 10 || d == 11 || d == 0; }
        bool IsSummer(double days) { int d = (int)(((days % 12) + 12) % 12); return d >= 4 && d <= 6; }

        // ------------------------------------------------------------ before: the studio's weather
        rain.Value = "false";
        double mid = UWGame.Mods.WeatherMod.MidSummer + 0.5; // the middle of "Mid winter"
        GoTo(16, mid - 0.5 / 12, 0.0);
        bool stockWind = true, stockLine = true;
        float stockCloud = weather.CloudCover;
        for (int i = 0; i < 96; i++)
        {
            Step(1.0 / 96);
            stockWind &= weather.WindSpeed == UWGame.Mods.WeatherMod.StudioWindSpeed;
            stockLine &= weather.CloudCover == stockCloud && UWGame.Mods.WeatherMod.SkyTerm("Clear") == "Clear";
        }
        Check(stockWind && stockLine, FormattableString.Invariant(
              $"before (switch off): a whole \"Mid winter\" day at the studio's {weather.WindSpeed} m/s and cloud {stockCloud}, and no rain on the line"));

        // ------------------------------------------------------------ the schedule
        rain.Value = "true";
        UWGame.Mods.WeatherMod.State[] Run(int s, double from, double years, int perDay)
        {
            var a = new UWGame.Mods.WeatherMod.State[(int)(years * 12 * perDay)];
            for (int i = 0; i < a.Length; i++) a[i] = UWGame.Mods.WeatherMod.Scheduled(s, from + (double)i / perDay);
            return a;
        }
        static bool Same(UWGame.Mods.WeatherMod.State[] a, UWGame.Mods.WeatherMod.State[] b) =>
            a.Length == b.Length && a.Zip(b).All(p => p.First.Rain == p.Second.Rain && p.First.Wind == p.Second.Wind && p.First.Cloud == p.Second.Cloud);
        var first = Run(seed, 15 * 12, 3, 96);
        Check(Same(first, Run(seed, 15 * 12, 3, 96)), "the same seed gives the same three years, every quarter hour, to the bit");
        var other = Run(seed + 1, 15 * 12, 3, 96);
        Check(!Same(first, other) && first.Zip(other).Count(p => (p.First.Rain > 0) != (p.Second.Rain > 0)) > first.Length / 20,
              "another seed gives another three years");

        // ------------------------------------------------------------ the climate: ten seeds, thirty years each
        int winterDays = 0, winterWet = 0, summerDays = 0, summerWet = 0, equinoxDays = 0, equinoxWet = 0, winters = 0, wintersWithStorm = 0;
        double winterHours = 0, winterRainHours = 0, summerHours = 0, summerRainHours = 0, winterDryWind = 0, winterDryHours = 0;
        float summerMaxWind = 0f, summerShowerWind = 0f, summerMinWind = float.MaxValue, winterMaxWind = 0f, anyMaxWind = 0f, winterDryMinWind = float.MaxValue;
        var terms = new System.Collections.Generic.HashSet<string>();
        for (int s = 0; s < 10; s++)
        {
            int sd = seed + 7919 * s;
            for (int y = 15; y < 45; y++)
            {
                bool storm = false;
                foreach (long d in new long[] { y * 12 + 10, y * 12 + 11, (y + 1) * 12 })
                {
                    storm |= UWGame.Mods.WeatherMod.Spell(sd, d, out _, out _, out float pk) && pk >= 0.7f;
                }
                winters++;
                if (storm) wintersWithStorm++;
                for (long d = y * 12; d < (y + 1) * 12; d++)
                {
                    bool wet = UWGame.Mods.WeatherMod.Spell(sd, d, out _, out _, out _);
                    int name = (int)(d % 12);
                    if (IsWinter(d)) { winterDays++; if (wet) winterWet++; }
                    else if (IsSummer(d)) { summerDays++; if (wet) summerWet++; }
                    else if (name == 2 || name == 8) { equinoxDays++; if (wet) equinoxWet++; }
                    for (int q = 0; q < 24; q++)
                    {
                        var st = UWGame.Mods.WeatherMod.Scheduled(sd, d + (q + 0.5) / 24);
                        bool raining = st.Rain >= UWGame.Mods.WeatherMod.RainThreshold;
                        terms.Add(UWGame.Mods.WeatherMod.WindTerm(st.Wind));
                        anyMaxWind = Math.Max(anyMaxWind, st.Wind);
                        if (IsWinter(d))
                        {
                            winterHours++;
                            winterMaxWind = Math.Max(winterMaxWind, st.Wind);
                            if (raining) winterRainHours++;
                            else if (st.Cloud < 0.6f) { winterDryHours++; winterDryWind += st.Wind; winterDryMinWind = Math.Min(winterDryMinWind, st.Wind); }
                        }
                        else if (IsSummer(d))
                        {
                            summerHours++;
                            if (raining) summerRainHours++;
                            else
                            {
                                summerMaxWind = Math.Max(summerMaxWind, st.Wind);
                                summerMinWind = Math.Min(summerMinWind, st.Wind);
                            }
                            summerShowerWind = Math.Max(summerShowerWind, st.Wind);
                        }
                    }
                }
            }
        }
        Check(winterWet > 0.75 * winterDays, "winter (\"Start of winter\", \"Mid winter\", \"Late winter\"): " + Pct((double)winterWet / winterDays) + " of days have a rain spell, raining " + Pct(winterRainHours / winterHours) + " of the hours");
        Check(wintersWithStorm >= 0.95 * winters, "  " + wintersWithStorm + " of " + winters + " winters bring a rainstorm (rain 0.7 and over)");
        Check(winterDryHours > 0.4 * winterHours && winterDryWind / winterDryHours >= 6.0 && winterDryMinWind >= 1.5f,
              "  between the spells, clear to partly cloudy " + Pct(winterDryHours / winterHours) + " of the hours, with wind of " + F(winterDryWind / winterDryHours) + " m/s on average (" + UWGame.Mods.WeatherMod.WindTerm((float)(winterDryWind / winterDryHours)) + ")");
        Check(summerWet < 0.12 * summerDays && summerRainHours < 0.02 * summerHours,
              "summer (\"Start of summer\" to \"Late summer\"): " + Pct((double)summerWet / summerDays) + " of days have a shower, raining " + Pct(summerRainHours / summerHours) + " of the hours");
        Check(equinoxWet > summerWet * equinoxDays / (double)summerDays && equinoxWet < winterWet * equinoxDays / (double)winterDays,
              "\"Spring\" and \"Mid autumn\" between the two: " + Pct((double)equinoxWet / equinoxDays) + " of days");

        // ------------------------------------------------------------ the wind
        Check(summerMinWind >= 0f && summerMaxWind < 5f && summerShowerWind < 11f,
              "summer wind " + F(summerMinWind) + "-" + F(summerMaxWind) + " m/s: \"" + UWGame.Mods.WeatherMod.WindTerm(summerMinWind) + "\" to \"" + UWGame.Mods.WeatherMod.WindTerm(summerMaxWind) + "\"; a shower's gusts up to " + F(summerShowerWind) + " (\"" + UWGame.Mods.WeatherMod.WindTerm(summerShowerWind) + "\")");
        Check(winterMaxWind >= 17f && anyMaxWind < 24f, "winter storms blow up to " + F(winterMaxWind) + " m/s, \"" + UWGame.Mods.WeatherMod.WindTerm(winterMaxWind) + "\"; never the studio's \"Storm\" (24) or \"Hurricane\"");
        Check(terms.Count >= 6 && !terms.Contains("Storm") && !terms.Contains("Hurricane"), "the wind uses " + terms.Count + " of the studio's Beaufort terms: " + string.Join(", ", terms));

        // ------------------------------------------------------------ the real WeatherManager, steered
        // A winter storm: the first spell of 0.7 and over from year 16 on.
        long stormDay = 16 * 12;
        double sStart = 0, sEnd = 0;
        float sPeak = 0;
        while (!(UWGame.Mods.WeatherMod.Spell(seed, stormDay, out sStart, out sEnd, out sPeak) && sPeak >= 0.7f && IsWinter(stormDay) && sEnd - sStart > 0.25)) stormDay++;
        double startDays = Math.Floor(sStart) - 1.0;
        int sy = (int)(startDays / 12);
        GoTo(sy, startDays / 12 - sy, 0.0);
        uint calls0 = (uint)(int)typeof(RandomGenerator).GetField("numberOfCalls", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(UWGame.The.Sim.GameplayRandomGenerator)!;
        bool follows = true, lineRain = false, lineDry = false;
        float maxChill = 0f, minAir = float.MaxValue, stormChill = 0f;
        int stormSamples = 0;
        double t0 = Days();
        while (Days() < sEnd + 0.5)
        {
            Step(1.0 / 96);
            var st = UWGame.Mods.WeatherMod.Scheduled(seed, Days());
            follows &= weather.CloudCover == st.Cloud && weather.WindSpeed == st.Wind;
            float air = UWGame.Mods.WeatherMod.CurrentAirTemperature()!.Value;
            float dry = UWGame.Mods.WeatherMod.AirTemperature(date.TimeOfYear, date.SunElevation, weather.CloudCover);
            maxChill = Math.Max(maxChill, dry - air);
            minAir = Math.Min(minAir, air);
            if (st.Rain >= 0.7f) { lineRain |= (line.Now ?? "").StartsWith("Rainstorm/"); stormChill += dry - air; stormSamples++; }
            if (st.Rain == 0f) lineDry |= (line.Now ?? "").Split('/')[0] is "Clear" or "Partly cloudy" or "Cloudy" or "Overcast";
        }
        uint calls1 = (uint)(int)typeof(RandomGenerator).GetField("numberOfCalls", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(UWGame.The.Sim.GameplayRandomGenerator)!;
        Check(follows, FormattableString.Invariant($"the real WeatherManager.Update takes the schedule's cloud cover and wind, every step for {Days() - t0:0.0} days around a storm on day {stormDay % 12} of year {stormDay / 12}"));
        Check(calls1 == calls0, "  without one draw from the game's random stream (" + (calls1 - calls0) + ")");
        Check(lineRain && lineDry, "  its WEATHER NOW line says \"Rainstorm\" in the storm (" + "e.g. \"" + line.Now + "\" at the end) and a cloud term when dry");
        Check(stormSamples > 0 && stormChill / stormSamples >= 3f && maxChill <= 6.5f,
              "  the storm cools the air " + F(stormChill / Math.Max(1, stormSamples)) + " C on average, at most " + F(maxChill) + " C");
        Check(minAir > 273f, "  and never to freezing: the coldest is " + C(minAir));

        // A save loaded mid-storm: a fresh WeatherManager (the studio's 0.2 and 1.5) is the schedule's on its first frame.
        GoTo(sy, (sStart + (sEnd - sStart) / 2) / 12 - sy, 0.5);
        float cloudThen = weather.CloudCover, windThen = weather.WindSpeed;
        var loaded = new WeatherManager();
        UWGame.The.Sim.PlaySite.PlaySite.Weather = loaded;
        loaded.Update(frame);
        Check(loaded.CloudCover == cloudThen && loaded.WindSpeed == windThen,
              FormattableString.Invariant($"a game loaded mid-storm has the same weather on its first frame: cloud {loaded.CloudCover:0.00}, wind {loaded.WindSpeed:0.0} m/s - nothing new saved"));
        rain.Value = "false";
        loaded.Update(frame);
        Check(loaded.WindSpeed == UWGame.Mods.WeatherMod.StudioWindSpeed, "switched off, the wind is the studio's 1.5 m/s again on the next frame");
        UWGame.The.Sim.PlaySite.PlaySite.Weather = weather;
        rain.Value = "true";

        // ------------------------------------------------------------ a year of air with rain and wind
        GoTo(17, 0.0, 0.0);
        float yearMin = float.MaxValue;
        for (int i = 0; i < 12 * 96; i++)
        {
            Step(1.0 / 96);
            float air = UWGame.Mods.WeatherMod.CurrentAirTemperature()!.Value;
            yearMin = Math.Min(yearMin, air);
        }
        Check(yearMin >= 273f + UWGame.Mods.WeatherMod.FloorCelsius - 0.01f, "a year of it never freezes: the coldest is " + C(yearMin));
        enabled.Value = "false";
        Check(UWGame.Mods.WeatherMod.SteerCloudCover() == null, "RAIN AND WIND needs SEASONS AND TEMPERATURE: with that off, the studio's clouds");
        enabled.Value = "true";

        // ------------------------------------------------------------ the forecast against the day that comes
        int days = 0, agree = 0, rainyAhead = 0;
        float worstHigh = 0f;
        string example = null;
        GoTo(15, 0.0, 0.3);
        for (int n = 0; n < 72; n++)
        {
            var ahead = UWGame.Mods.WeatherMod.Tomorrow(UWGame.Mods.WeatherMod.Seed, date.Year, date.TimeOfYear, date.TimeOfDay);
            Step(1.0 - date.TimeOfDay - 0.5 / 48); // to half a sample before midnight
            float high = float.MinValue, maxRain = 0f, maxWind = 0f;
            for (int i = 0; i < 48; i++)
            {
                Step(1.0 / 48);
                high = Math.Max(high, UWGame.Mods.WeatherMod.CurrentAirTemperature()!.Value);
                maxRain = Math.Max(maxRain, UWGame.Mods.WeatherMod.Scheduled(UWGame.Mods.WeatherMod.Seed, Days()).Rain);
                maxWind = Math.Max(maxWind, weather.WindSpeed);
            }
            days++;
            bool same = (maxRain >= UWGame.Mods.WeatherMod.RainThreshold) == (ahead.MaxRain >= UWGame.Mods.WeatherMod.RainThreshold)
                        && UWGame.Mods.WeatherMod.WindTerm(maxWind) == ahead.Wind
                        && (maxRain < UWGame.Mods.WeatherMod.RainThreshold || UWGame.Mods.WeatherMod.RainTerm(maxRain) == ahead.Sky);
            if (same) agree++;
            if (ahead.MaxRain >= UWGame.Mods.WeatherMod.RainThreshold) rainyAhead++;
            worstHigh = Math.Max(worstHigh, Math.Abs(high - ahead.HighK));
            if (example == null && ahead.MaxRain >= 0.7f) example = ahead.Sky + "/" + ahead.Wind + " " + UWGame.Mods.WeatherMod.Celsius(ahead.HighK) + " C";
            Step(1.0 - date.TimeOfDay + 0.3); // on to 0.3 of the day after: a forecast every other day
        }
        Check(agree == days && rainyAhead > 0, "the forecast for tomorrow is what tomorrow brings, " + agree + " of " + days + " days (" + rainyAhead + " forecast rain; e.g. \"" + example + "\")");
        Check(worstHigh < 0.3f, "  its high is the day's warmest to " + F(worstHigh) + " C");

        UWGame.The.InGameUI = null;
        rain.Value = rain.DefaultValue;
    }
}

/// <summary>The interface's WEATHER NOW line and nothing else, for the weather self-test.</summary>
internal sealed class WeatherLine : UWGame.ClientSide.Interface.InGameInterface
{
    public string Now;

    public WeatherLine()
        : base(null, null)
    {
    }

    public override void SetWeatherNow(string cloudCover, string wind) => Now = cloudCover + "/" + wind;
}
