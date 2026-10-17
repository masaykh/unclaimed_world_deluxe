using System;
using UWGame.ClientSide.Interface.DateAndWeatherPanel;
using UWGame.SimSide;

namespace UWGame.Mods;

/// <summary>
/// Seasons and temperature: the sun follows the season names, and the air is warm in summer, cool
/// in winter and warmer by day than by night (step 1 of "Weather and temperature"); and, behind a
/// second switch, rain and wind (step 2). What the cold does to people comes later.
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
/// RAIN AND WIND (step 2), a second switch that works with SEASONS AND TEMPERATURE on (off by
/// default). Separate because it replaces the studio's random cloud cover, and with it the
/// sunshine that dries and spoils things: the seasons can be played without it.
/// - WHAT THE STUDIO HAS. WeatherManager (Sim/Weather.cs) keeps a cloud cover and a wind. Every 2 s
///   it rolls a 1 in 50 chance of a new wind direction and a 1 in 80 chance of a new cloud cover
///   target, any value equally likely; windSpeed is 1.5 m/s and never changes. UpdateWeatherDisplay
///   puts a cloud term ("Clear" to "Overcast") and a wind term (its Beaufort list, "Calm" to
///   "Hurricane") on the date panel's WEATHER NOW line. There is no rain anywhere in the game. The
///   panel's WEATHER AHEAD line had SetWeatherAhead, which nothing called: it showed "13 C".
/// - THE SCHEDULE (<see cref="Scheduled"/>). The weather is a function of the game's random seed
///   (the gameplay RandomGenerator's RandomSeed, saved with every game and reused by replays) and
///   of the time, Year * 12 + TimeOfYear * 12 days. Nothing is drawn from the studio's random
///   stream - a draw would shift every random number the game takes after it - and so the weather
///   can be looked ahead at, which is what the forecast does. Each calendar day rolls one rain
///   spell, with a chance that follows the season: about 90% on "Mid winter", 80% on "Late winter"
///   and "Start of winter", a quarter at the equinoxes and 5% in mid summer. A winter spell is a
///   rainstorm of up to 12 hours; a summer one a shower of 2-4. The wind blows by the season,
///   1-5 m/s in summer, 5-12 m/s in winter (the cold winds between the storms), and a storm adds
///   up to 10 more; it never reaches the studio's "Storm" (24 m/s and up). Clouds gather two hours
///   before the rain and break two hours after it; between spells the sky is clear to partly cloudy.
/// - WHAT FOLLOWS (<see cref="SteerCloudCover"/>, <see cref="SteerWindSpeed"/>, from
///   WeatherManager.Update). The cloud cover and the wind speed are the schedule's. Cloud cuts the
///   studio's SunIntensity, so a rainy day is cooler and things dry and spoil less in the sun; the
///   cloud shadows (GameWorldRenderer) follow; the wind blows the cloud shadows and the smoke that
///   the studio lets the wind push (ParticleSystem, IsAffectedByWind).
/// - THE AIR (<see cref="Cooled"/>). Rain cools it by up to <see cref="RainChill"/> C and wind above
///   <see cref="ChillFromWind"/> m/s by <see cref="WindChill"/> C a m/s - a winter storm 3-5 C
///   colder - never below <see cref="FloorCelsius"/> C: "Not freezing temperatures".
/// - THE PANEL. WEATHER NOW says "Light rain", "Rain" or "Rainstorm" in place of the cloud term
///   while it rains (<see cref="SkyTerm"/>, from WeatherManager.UpdateWeatherDisplay). WEATHER AHEAD
///   shows tomorrow, midnight to midnight, from the schedule (<see cref="Tomorrow"/>): the wettest
///   it will be (or the daytime cloud), the strongest wind, and the warmest it will get.
/// - NOT HERE. No rain is drawn and none is heard: the game has no rain particle, texture or sound.
///   Its particles are puffs of the smoke, explosion, fog and pollen textures, drawn with the map
///   tile their emitter stands on (GameWorldRenderer), at one scale in both directions. Rain would
///   need a streak texture (a new binary asset) or a particle drawn stretched along its velocity,
///   and an emitter moved from tile to tile with the camera; a sound would need a new asset too.
///   What the cold does to people is step 3.
///
/// NOTHING NEW IS SAVED. The temperature is worked out each time from what a save already holds:
/// TimeOfYear and TimeOfDay (DateAndTime) and the cloud cover (WeatherManager). No tile is written,
/// so there is no per-tile cost and switching off puts the studio's 15 C back at once. No random
/// numbers are drawn; replays are unaffected beyond what the switch changes. With RAIN AND WIND on,
/// WeatherManager's own saved cloud cover, cloud target and wind speed hold the schedule's values,
/// in the fields and the format they always had; on load the schedule is worked out again from the
/// seed and the date. Switched off, the wind goes back to the studio's 1.5 m/s and the clouds
/// drift back to the studio's random targets. The sun's elevation is
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

    private static ModSetting rain;

    public static ModSetting RainSetting =>
        rain ?? (rain = ModSettings.Toggle(
            ModId, "rain", "RAIN AND WIND", defaultValue: false,
            toolTip: "Works with SEASONS AND TEMPERATURE on. Winter brings rainstorms of several hours " +
                     "with cold winds between them; spring and autumn some showers; summer is mostly " +
                     "clear. Rain and wind cool the air by up to about 5 C, never to freezing, and cloud " +
                     "keeps the sun off. The date panel says when it rains and forecasts tomorrow.",
            affectsSimulation: true));

    public static void RegisterSettings()
    {
        ModSettings.Describe(ModId, "Jerrybi",
            "Seasons and temperature: the sun follows the season names, and a subtropical year of warm summers and cool winters decides how fast things spoil outside.",
            "HUD_thumbnail_weatherAntenna");
        ModSettings.SetCategoryLabel(ModId, "WEATHER");
        _ = EnabledSetting;
        _ = RainSetting;
    }

    public static bool On => EnabledSetting.On;

    /// <summary>RAIN AND WIND, which needs SEASONS AND TEMPERATURE: the rain cools the air that switch makes.</summary>
    public static bool RainOn => On && RainSetting.On;

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

    /// <summary>The studio's wind, which never changes: WeatherManager.windSpeed = 1.5f.</summary>
    public const float StudioWindSpeed = 1.5f;

    /// <summary>Degrees (C) a full rainstorm takes off the air.</summary>
    public const float RainChill = 3f;

    /// <summary>Degrees (C) taken off the air for each m/s of wind above <see cref="ChillFromWind"/>.</summary>
    public const float WindChill = 0.12f;

    /// <summary>Wind (m/s) below which it does not cool the air.</summary>
    public const float ChillFromWind = 3f;

    /// <summary>Rain and wind never cool the air below this (C); the seasons alone never go below 5 C.</summary>
    public const float FloorCelsius = 2f;

    /// <summary>Rain below this (0-1) is not called rain.</summary>
    public const float RainThreshold = 0.05f;

    /// <summary>Wind (m/s) a full rainstorm adds.</summary>
    public const float StormWind = 10f;

    /// <summary>
    /// The strongest wind (m/s): inside the studio's "Strong gale" (20-24 in WeatherManager.windEdges),
    /// short of its "Storm"; the sky term says "Rainstorm".
    /// </summary>
    public const float MaxWind = 23.5f;

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
        DateAndTime date = The.Sim?.DateAndTime;
        if (RainOn && date != null)
        {
            Forecast ahead = Tomorrow(Seed, date.Year, date.TimeOfYear, date.TimeOfDay);
            panel.SetWeatherAhead(ahead.Sky, ahead.Wind);
            panel.SetTempAhead(Celsius(ahead.HighK));
        }
    }

    /// <summary>
    /// Called at the top of WeatherManager.Update: with RAIN AND WIND on, the schedule's cloud cover,
    /// which the studio's code then also takes as its target; null leaves the studio's.
    /// </summary>
    public static float? SteerCloudCover() => RainOn ? Now()?.Cloud : null;

    /// <summary>
    /// Called at the top of WeatherManager.Update with its wind speed: with RAIN AND WIND on, the
    /// schedule's. Off, the studio's 1.5 m/s - which every save made without the switch holds
    /// already, and which a save made with it gets back.
    /// </summary>
    public static float SteerWindSpeed(float stock) => RainOn ? Now()?.Wind ?? stock : StudioWindSpeed;

    /// <summary>
    /// Called by WeatherManager.UpdateWeatherDisplay with the studio's cloud term: while it rains,
    /// "Light rain", "Rain" or "Rainstorm" instead.
    /// </summary>
    public static string SkyTerm(string stock)
    {
        float rainNow = RainOn ? Now()?.Rain ?? 0f : 0f;
        return rainNow >= RainThreshold ? RainTerm(rainNow) : stock;
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
        float air = AirTemperature(date.TimeOfYear, date.SunElevation, weather.CloudCover);
        return RainOn ? Cooled(air, Scheduled(Seed, ScheduleDays(date.Year, date.TimeOfYear))) : air;
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

    // ---------------------------------------------------------------- rain and wind: the schedule

    /// <summary>The weather at one moment: rain 0-1, wind in m/s, cloud cover 0-1.</summary>
    public readonly struct State
    {
        public readonly float Rain;
        public readonly float Wind;
        public readonly float Cloud;

        public State(float rain, float wind, float cloud)
        {
            Rain = rain;
            Wind = wind;
            Cloud = cloud;
        }
    }

    /// <summary>Tomorrow, midnight to midnight: what the WEATHER AHEAD line shows.</summary>
    public readonly struct Forecast
    {
        public readonly string Sky;
        public readonly string Wind;
        public readonly float HighK;
        public readonly float MaxRain;
        public readonly float MaxWind;

        public Forecast(string sky, string wind, float highK, float maxRain, float maxWind)
        {
            Sky = sky;
            Wind = wind;
            HighK = highK;
            MaxRain = maxRain;
            MaxWind = maxWind;
        }
    }

    /// <summary>
    /// The game's random seed: the gameplay generator's RandomSeed, which every save holds and a
    /// replay reuses. 0 for a generator started without one.
    /// </summary>
    public static int Seed => The.Sim?.GameplayRandomGenerator?.RandomSeed ?? 0;

    /// <summary>The schedule's clock in days: (Year + TimeOfYear) * 12. The two wrap together.</summary>
    public static double ScheduleDays(int year, double timeOfYear) => (year + timeOfYear) * DateAndTime.DaysPerYear;

    private static double cachedDays = double.NaN;
    private static int cachedSeed;
    private static State cachedState;

    /// <summary>The weather now, or null with no clock.</summary>
    private static State? Now()
    {
        DateAndTime date = The.Sim?.DateAndTime;
        if (date == null)
        {
            return null;
        }
        double days = ScheduleDays(date.Year, date.TimeOfYear);
        int seed = Seed;
        if (days != cachedDays || seed != cachedSeed)
        {
            cachedState = Scheduled(seed, days);
            cachedDays = days;
            cachedSeed = seed;
        }
        return cachedState;
    }

    /// <summary>
    /// How wet the season is: 1 in the middle of "Mid winter", 0 in the middle of "Mid summer", a
    /// half at the equinoxes - step 1's season curve turned over.
    /// </summary>
    public static double Wetness(double timeOfYear) => (1.0 - Math.Cos(2.0 * Math.PI * (timeOfYear - MidSummer))) / 2.0;

    /// <summary>The chance of a rain spell on a calendar day of this wetness: 4% to 90%.</summary>
    public static double RainChance(double wetness) => 0.04 + 0.86 * wetness * wetness;

    private const double Hour = 1.0 / 24.0;

    /// <summary>The weather a seed gives at a time on the schedule's clock (<see cref="ScheduleDays"/>).</summary>
    public static State Scheduled(int seed, double days)
    {
        long day = (long)Math.Floor(days);
        float rainNow = 0f, gathering = 0f, cloudAtPeak = 0f;
        // A spell starts on its own day and may run into the next; clouds gather two hours before it.
        for (long d = day - 1; d <= day + 1; d++)
        {
            if (!Spell(seed, d, out double start, out double end, out float peak))
            {
                continue;
            }
            rainNow = Math.Max(rainNow, peak * Envelope(days, start, end, Hour));
            float gather = Envelope(days, start - 2.0 * Hour, end + 2.0 * Hour, 2.0 * Hour);
            if (gather > gathering)
            {
                gathering = gather;
                cloudAtPeak = 0.7f + 0.3f * peak;
            }
        }
        float clear = Between(DayCloud(seed, day - 1), DayCloud(seed, day), DayCloud(seed, day + 1), days - day);
        float cloud = clear + (Math.Max(clear, cloudAtPeak) - clear) * gathering;
        double block = days * 8.0; // the wind changes every three hours
        long b = (long)Math.Floor(block);
        float t = (float)(block - b);
        float wind = BlockWind(seed, b) * (1f - t) + BlockWind(seed, b + 1) * t + StormWind * rainNow;
        return new State(rainNow, Math.Min(wind, MaxWind), Common.Clamp(cloud, 0f, 1f));
    }

    /// <summary>
    /// A calendar day's rain spell, if it rolls one: when it starts and ends on the schedule's clock
    /// and how hard it rains at its height (0-1).
    /// </summary>
    public static bool Spell(int seed, long day, out double start, out double end, out float peak)
    {
        double wet = Wetness(YearFraction((day + 0.5) / DateAndTime.DaysPerYear));
        start = end = 0.0;
        peak = 0f;
        if (Uniform(seed, day, 1) >= RainChance(wet))
        {
            return false;
        }
        double hours = 2.0 + Uniform(seed, day, 3) * (2.0 + 8.0 * wet); // summer 2-4 h, winter up to 12
        start = day + Uniform(seed, day, 2);
        end = start + hours * Hour;
        peak = (float)(0.2 + 0.5 * wet + 0.3 * Uniform(seed, day, 4)); // a summer shower to a winter storm
        return true;
    }

    /// <summary>A day's cloud cover between spells: clear to partly cloudy, 0.05-0.35.</summary>
    private static float DayCloud(int seed, long day) => (float)(0.05 + 0.3 * Uniform(seed, day, 5));

    /// <summary>The wind (m/s) at the start of a three-hour block: about 1-4 in summer, 5-12 in winter.</summary>
    private static float BlockWind(int seed, long block)
    {
        double wet = Wetness(YearFraction(block / (DateAndTime.DaysPerYear * 8.0)));
        double mean = 2.5 + 6.0 * Math.Pow(wet, 1.5);
        double spread = 1.5 + 2.0 * wet;
        return (float)Math.Max(0.0, mean + spread * (2.0 * Uniform(seed, block, 6) - 1.0));
    }

    private static double YearFraction(double years) => years - Math.Floor(years);

    /// <summary>0 outside (start, end); 1 a ramp in from both ends; smooth between.</summary>
    private static float Envelope(double t, double start, double end, double ramp)
    {
        if (t <= start || t >= end)
        {
            return 0f;
        }
        double x = Math.Min(1.0, Math.Min(t - start, end - t) / ramp);
        return (float)(x * x * (3.0 - 2.0 * x));
    }

    /// <summary>A day's value, moving in a straight line from the middle of yesterday through today to the middle of tomorrow.</summary>
    private static float Between(float yesterday, float today, float tomorrow, double dayFraction)
    {
        return dayFraction < 0.5
            ? yesterday + (today - yesterday) * (float)(dayFraction + 0.5)
            : today + (tomorrow - today) * (float)(dayFraction - 0.5);
    }

    /// <summary>A number in [0, 1) from the seed, an index and a purpose (SplitMix64): the same three, the same number.</summary>
    private static double Uniform(int seed, long index, int purpose)
    {
        ulong z = Mix(Mix((ulong)(uint)seed | ((ulong)(uint)purpose << 32)) ^ (ulong)index);
        return (z >> 11) * (1.0 / (1UL << 53));
    }

    private static ulong Mix(ulong z)
    {
        z += 0x9E3779B97F4A7C15UL;
        z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
        z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
        return z ^ (z >> 31);
    }

    /// <summary>
    /// The air (K) after rain and wind: <see cref="RainChill"/> C for a full storm and
    /// <see cref="WindChill"/> C a m/s above <see cref="ChillFromWind"/>, never below
    /// <see cref="FloorCelsius"/> (and never warmer than it was).
    /// </summary>
    public static float Cooled(float air, State weather)
    {
        float chill = RainChill * weather.Rain + WindChill * Math.Max(0f, weather.Wind - ChillFromWind);
        return Math.Max(air - chill, Math.Min(air, ZeroCelsius + FloorCelsius));
    }

    /// <summary>
    /// Tomorrow, midnight to midnight by TimeOfDay, from the schedule: the wettest it gets (or the
    /// daytime cloud), the strongest wind and the warmest air, sampled every half hour.
    /// </summary>
    public static Forecast Tomorrow(int seed, int year, double timeOfYear, double timeOfDay)
    {
        const int samples = 48;
        double midnight = ScheduleDays(year, timeOfYear) + (1.0 - timeOfDay);
        float high = float.MinValue, maxRain = 0f, maxWind = 0f, cloud = 0f;
        int daylight = 0;
        for (int i = 0; i < samples; i++)
        {
            double tod = (i + 0.5) / samples;
            double days = midnight + tod;
            double toy = YearFraction(days / DateAndTime.DaysPerYear);
            State s = Scheduled(seed, days);
            float elevation = SunElevation(toy, tod);
            high = Math.Max(high, Cooled(AirTemperature(toy, elevation, s.Cloud), s));
            maxRain = Math.Max(maxRain, s.Rain);
            maxWind = Math.Max(maxWind, s.Wind);
            if (elevation > 0f)
            {
                cloud += s.Cloud;
                daylight++;
            }
        }
        string sky = maxRain >= RainThreshold ? RainTerm(maxRain) : CloudTerm(cloud / Math.Max(1, daylight));
        return new Forecast(sky, WindTerm(maxWind), high, maxRain, maxWind);
    }

    /// <summary>"Light rain" below 0.35, "Rain" below 0.7, "Rainstorm" from there.</summary>
    public static string RainTerm(float rainAmount) =>
        rainAmount < 0.35f ? UWGame.Locale.Text("Light rain")
        : rainAmount < 0.7f ? UWGame.Locale.Text("Rain")
        : UWGame.Locale.Text("Rainstorm");

    // The studio's terms and edges (WeatherManager.InitConstants) for the forecast, read the way
    // UpdateWeatherDisplay reads them: Common.GetStairStepIndex, the first edge above the value.
    // English, as on the WEATHER NOW line.
    private static readonly float[] cloudEdges = { 0.1f, 0.4f, 0.96f, 1f };
    private static readonly string[] cloudTerms = { "Clear", "Partly cloudy", "Cloudy", "Overcast" };
    private static readonly float[] windEdges = { 0.5f, 3f, 5f, 11f, 14f, 17f, 20f, 24f, 28f, 100f };
    private static readonly string[] windTerms = { "Calm", "Light breeze", "Gentle breeze", "Fresh breeze", "Strong breeze", "High wind", "Gale", "Strong gale", "Storm", "Hurricane" };

    /// <summary>The studio's cloud term for a cloud cover.</summary>
    public static string CloudTerm(float cloud) => cloudTerms[Common.GetStairStepIndex(cloud, cloudEdges)];

    /// <summary>The studio's wind term (its Beaufort list) for a wind speed in m/s.</summary>
    public static string WindTerm(float wind) => windTerms[Common.GetStairStepIndex(wind, windEdges)];
}
