using System;
using UWGame.ClientSide.Interface.DateAndWeatherPanel;
using UWGame.SimSide;

namespace UWGame.Mods;

/// <summary>
/// Seasons and temperature: the sun follows the season names, and the air is warm in summer, cool
/// in winter and warmer by day than by night. Step 1 of "Weather and temperature"; rain, wind and
/// what the cold does to people come later.
///
/// THE REQUEST. Kastuk, "Weather and temperature" (2026-10-17): "rainstorms during winter time with
/// cold winds between them. Not freezing temperatures, but still cold enough for sickness. Then
/// high solar heat during usually hot summer. Can take references from subtropic seasons."
///
/// WHAT THE STUDIO HAS (original_src/UnclaimedWorld).
/// - The calendar (Sim/DateAndTime.cs) names twelve days a year (DaysPerYear = 12), "Late winter"
///   to "Mid winter", by (int)(TimeOfYear * 12). The sun's declination is
///   axialTilt * (-1 + 2 * TimeOfYear), commented "goes from -axialTilt in the winter to +axialTilt
///   in summer" - but TimeOfYear runs 0 to 1 and wraps, so it is a SAWTOOTH: the sun is highest on
///   the last day, "Mid winter", lowest the next morning, "Late winter", and "Mid summer" is near
///   the equinox. The names and the sun disagree.
/// - Air temperature is a constant. TerrainTile.Temperature is "15 C - replace this!", set from
///   Constants.MeanAmbientTemperature (288 K); DegradeType.TemperatureDamage says "currently (2015)
///   no temp variation OUTSIDE in the game. weather temp is hardcoded to 288 Kelvin = 15 C".
///   WeatherManager.Update computes globalTemperature = 273 + 5 + 20 * SunIntensity ("replace
///   this... temp goes from 5 to 20 deg. C.") and its CycleOnce would have written it into every
///   tile, but that loop is commented out: "disabled for testing temperature override on tile
///   levels" - PlaceGameEntities.DecompositionTest sets tiles to 255/278/284/294 K, the freezer,
///   fridge, earth-cooled and air-con temperatures, to watch food decompose at each.
/// - The only thing that reads a tile's temperature is spoilage: NonLivingEntity.
///   ComputeDegradeDamage, which passes it through the item's storage (StorageCondition.
///   GetTemperature: a fixed temperature, or "isolated" = ambient - 0.4 * (ambient - 293)).
///
/// WHAT THIS CHANGES, behind SEASONS AND TEMPERATURE (off by default):
/// 1. The sun (<see cref="Declination"/>, from DateAndTime.ComputeSunAndLight): axialTilt *
///    cos(2 pi (TimeOfYear - 5.5/12)) - highest in the middle of the "Mid summer" day, lowest in the
///    middle of "Mid winter", level at "Spring" and "Mid autumn", and no jump at the new year.
///    The same tilt (pi/8) and latitude (pi/4): noon at 67.5 degrees in summer, 22.5 in winter.
/// 2. The air (<see cref="AirTemperature"/>): a night temperature that follows the season, plus
///    the studio's own day term, 20 degrees times the sunshine - (1 - 0.5 * cloud cover) *
///    sin(sun elevation), WeatherManager's SunIntensity. With the default cloud cover (0.2):
///      mid winter: 5 C at night, ~12 C at noon (9 C overcast)
///      equinox:   11 C at night, ~24 C at noon
///      mid summer: 17 C at night, ~34 C at noon (26 C overcast, 35.5 C clear)
///    The numbers are <see cref="NightCelsius"/>, <see cref="NightSeasonalSwing"/> and
///    <see cref="DaySwing"/>; change them there.
/// 3. Spoilage (<see cref="OutsideTemperature"/>, from ComputeDegradeDamage): a tile at the studio's
///    288 K is at the air temperature instead. A tile somebody set to anything else - the studio's
///    DecompositionTest - keeps it. Storage follows on its own, because storage temperatures are
///    computed from the ambient one; a fridge or a freezer is unaffected.
/// 4. Shelf-life tooltips (<see cref="ShelfLifeTemperature"/>, from DegradeType.
///    PostDataCompleteInitialize): "Outside" and "Inside" are worked out for the mid-summer day's
///    average air temperature (~23.5 C) instead of 15 C - the shortest a thing keeps in the year.
///    They are computed when the game's tables are built, so they follow the switch from the next
///    game started or loaded.
/// 5. The HUD (<see cref="ShowTemperature"/>, from InGameInterface.SetTimeAndDate): the date panel's
///    WEATHER NOW line shows the air temperature through the studio's HUDDateAndWeather.SetTemp,
///    which nothing called (the panel showed a fixed "17 C"). The line is in the panel's lower half;
///    drag its bottom edge down to see it.
///
/// NOTHING NEW IS SAVED. The temperature is worked out each time from what a save already holds:
/// TimeOfYear and TimeOfDay (DateAndTime) and the cloud cover (WeatherManager). No tile is written,
/// so there is no per-tile cost and switching off puts the studio's 15 C back at once. No random
/// numbers are drawn; replays are unaffected beyond what the switch changes. The sun's elevation is
/// used rather than SunIntensity because the port's shadow toggle (PortSettings.ShadowsDisabled)
/// forces SunIsUp off and with it SunIntensity; that is a display option and must not cool the air.
/// </summary>
public static class WeatherMod
{
    public const string ModId = "weather";

    private static ModSetting enabled;

    public static ModSetting EnabledSetting =>
        enabled ?? (enabled = ModSettings.Toggle(
            ModId, "enabled", "SEASONS AND TEMPERATURE", defaultValue: false,
            toolTip: "The sun is highest in mid summer and lowest in mid winter, as the season names say. " +
                     "The air follows a warm subtropical year: about 5 C on a mid-winter night to 12 C at noon, " +
                     "17 C on a mid-summer night to 34 C at noon, warmer in sunshine than under cloud. Food " +
                     "and materials outside, and in storage that is not cooled, spoil faster in the heat and " +
                     "slower in the cold. The date panel shows the temperature.",
            affectsSimulation: true));

    public static void RegisterSettings()
    {
        ModSettings.Describe(ModId, "Jerrybi",
            "Seasons and temperature: the sun follows the season names, and a subtropical year of warm summers and cool winters decides how fast things spoil outside.",
            "HUD_thumbnail_weatherAntenna");
        ModSettings.SetCategoryLabel(ModId, "WEATHER");
        _ = EnabledSetting;
    }

    public static bool On => EnabledSetting.On;

    // ---------------------------------------------------------------- the climate, tunable here

    /// <summary>The studio's zero Celsius: 288 K is its 15 C (Constants.MeanAmbientTemperature).</summary>
    public const float ZeroCelsius = 273f;

    /// <summary>The night temperature halfway through the year (C), at "Spring" and "Mid autumn".</summary>
    public const float NightCelsius = 11f;

    /// <summary>How far the night temperature rises in mid summer and falls in mid winter (C): 5 C to 17 C.</summary>
    public const float NightSeasonalSwing = 6f;

    /// <summary>
    /// Degrees added by full sunshine (C). The studio's: globalTemperature = 273 + 5 + 20 * SunIntensity.
    /// Full sunshine is SunIntensity 1 - the sun overhead with no cloud, which this latitude never sees.
    /// </summary>
    public const float DaySwing = 20f;

    /// <summary>
    /// The middle of the "Mid summer" day as a TimeOfYear: the sixth of twelve names (index 5),
    /// "Late winter" to "Mid winter", and (int)(TimeOfYear * 12) picks the name.
    /// </summary>
    public const double MidSummer = 5.5 / 12.0;

    /// <summary>DateAndTime's axialTilt, "tilt of the planet": MathHelper.Pi / 8.</summary>
    public const double AxialTilt = Math.PI / 8.0;

    /// <summary>DateAndTime's latitude: MathHelper.PiOver4, "equator = 0f, north pole = Pi/2".</summary>
    public const double Latitude = Math.PI / 4.0;

    /// <summary>The studio's starting cloud cover, "set cloud cover here" (WeatherManager.cloudCover).</summary>
    public const float StudioCloudCover = 0.2f;

    // ---------------------------------------------------------------- hooks

    /// <summary>
    /// Called by DateAndTime.ComputeSunAndLight with the studio's declination; with the switch on,
    /// one that peaks in the middle of "Mid summer" and bottoms out in the middle of "Mid winter".
    /// </summary>
    public static float Declination(double timeOfYear, float stock) =>
        On ? SeasonalDeclination(timeOfYear) : stock;

    /// <summary>
    /// Called by NonLivingEntity.ComputeDegradeDamage with the tile's temperature: the air
    /// temperature for a tile at the studio's 15 C, the tile's own for one set to anything else.
    /// </summary>
    public static float OutsideTemperature(float tileTemperature)
    {
        if (!On || tileTemperature != GameData.Instance.Constants.MeanAmbientTemperature)
        {
            return tileTemperature;
        }
        float? air = CurrentAirTemperature();
        return air ?? tileTemperature;
    }

    /// <summary>
    /// Called by DegradeType.PostDataCompleteInitialize with the temperature the shelf-life tooltips
    /// are worked out for (the studio's 15 C); with the switch on, the mid-summer day's average.
    /// </summary>
    public static float ShelfLifeTemperature(float stock) => On ? DailyMeanTemperature(MidSummer, StudioCloudCover) : stock;

    /// <summary>Called by InGameInterface.SetTimeAndDate: the current air temperature on the date panel.</summary>
    public static void ShowTemperature(HUDDateAndWeather panel)
    {
        if (!On || panel == null)
        {
            return;
        }
        float? air = CurrentAirTemperature();
        if (air.HasValue)
        {
            panel.SetTemp(Celsius(air.Value));
        }
    }

    // ---------------------------------------------------------------- the model

    /// <summary>The air temperature now (K), or null with no play site.</summary>
    public static float? CurrentAirTemperature()
    {
        WeatherManager weather = The.Sim?.PlaySite?.PlaySite?.Weather;
        DateAndTime date = The.Sim?.DateAndTime;
        if (weather == null || date == null)
        {
            return null;
        }
        return AirTemperature(date.TimeOfYear, date.SunElevation, weather.CloudCover);
    }

    /// <summary>
    /// The air temperature (K) at a time of year, a sun elevation (radians) and a cloud cover (0-1):
    /// the season's night temperature plus <see cref="DaySwing"/> times the studio's SunIntensity.
    /// </summary>
    public static float AirTemperature(double timeOfYear, float sunElevation, float cloudCover)
    {
        double season = Math.Cos(2.0 * Math.PI * (timeOfYear - MidSummer)); // 1 mid summer, -1 mid winter
        float sunshine = Common.Clamp((1f - 0.5f * cloudCover) * (float)Math.Sin(sunElevation), 0f, 1f);
        return ZeroCelsius + NightCelsius + NightSeasonalSwing * (float)season + DaySwing * sunshine;
    }

    /// <summary>The declination (radians) with the switch on: +AxialTilt mid summer, -AxialTilt mid winter.</summary>
    public static float SeasonalDeclination(double timeOfYear) =>
        (float)(AxialTilt * Math.Cos(2.0 * Math.PI * (timeOfYear - MidSummer)));

    /// <summary>
    /// The sun's elevation (radians) as DateAndTime.ComputeSunAndLight works it out, with the
    /// switch on: hour angle (TimeOfDay - 0.5) * 2 pi, <see cref="SeasonalDeclination"/>, <see cref="Latitude"/>.
    /// </summary>
    public static float SunElevation(double timeOfYear, double timeOfDay)
    {
        double hourAngle = (timeOfDay - 0.5) * 2.0 * Math.PI;
        double declination = SeasonalDeclination(timeOfYear);
        double sin = Math.Cos(hourAngle) * Math.Cos(declination) * Math.Cos(Latitude) + Math.Sin(declination) * Math.Sin(Latitude);
        return (float)Math.Asin(sin);
    }

    /// <summary>The average air temperature (K) over a day at a time of year and a steady cloud cover.</summary>
    public static float DailyMeanTemperature(double timeOfYear, float cloudCover)
    {
        const int samples = 96;
        double sum = 0.0;
        for (int i = 0; i < samples; i++)
        {
            sum += AirTemperature(timeOfYear, SunElevation(timeOfYear, (i + 0.5) / samples), cloudCover);
        }
        return (float)(sum / samples);
    }

    /// <summary>Kelvin to whole degrees Celsius, as the date panel shows them.</summary>
    public static int Celsius(float kelvin) => (int)Math.Round(kelvin - ZeroCelsius, MidpointRounding.AwayFromZero);
}
