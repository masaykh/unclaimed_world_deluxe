using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
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
/// NOT HERE. The date panel: SetTemp needs the game's interface (see the mod's summary).
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

        enabled.Value = enabled.DefaultValue;
        Console.WriteLine(failures == 0 ? "weather self-test OK" : $"weather self-test FAILED - {failures} check(s)");
        return failures == 0 ? 0 : 1;
    }
}
