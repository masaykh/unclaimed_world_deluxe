using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface.DateAndWeatherPanel;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide;
using UWGame.SimSide.AI;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Processes;
using UWGame.SimSide.SimEffects;
using UWGame.SimSide.Trees;

namespace UWGame.Mods;

/// <summary>
/// Seasons and temperature: the sun follows the season names, and the air is warm in summer, cool
/// in winter and warmer by day than by night (step 1 of "Weather and temperature"); behind a
/// second switch, rain and wind (step 2); behind a third, what a cold, wet night slept out in
/// the open does to people (steps 3 and 4); and behind a fourth, what rain and wind do to things
/// drying (step 5).
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
///
/// COLD AND WET HURT (step 3), a third switch that works with SEASONS AND TEMPERATURE on, and uses the
/// rain and the wind when RAIN AND WIND is on too (off by default). Kastuk: "Not freezing
/// temperatures, but still cold enough for sickness."
/// - WHAT THE STUDIO HAS. Where a colonist sleeps sets how fast and how far the sleep need refills
///   (GoalDoSleep.ComputeSleepConditions, SleepNeed): in the open at most 0.94 at x0.8, by a
///   prepared campfire (a gathering site with IsPrepared) 0.97 at x0.9, in a residence by its
///   condition and comfort level. EvaluateSleep.ScoreComfort scores a place by that (the open 0.5
///   for its cap), so huts are preferred already. Nothing about the weather. The studio's
///   PhysicalEffects.LimitForIncreasedSickness and SicknessFactor (set per need in CreatureLoader)
///   are read nowhere: they describe sickness from an unmet NEED (hunger, thirst), not from the
///   weather, so they are left alone - a night out has no need to hang them on.
/// - EXPOSURE (<see cref="ExposureOf"/>, <see cref="NightExposure"/>). An hour in the open: cold =
///   (12 C - air) / 10, between 0 and 1 (none at 12 C and above, all of it at the 2 C floor), times
///   0.5 + 0.3 x rain (0-1) + 0.2 x wind (0 at 3 m/s, all of it at 15 m/s). The night is the next
///   <see cref="NightHours"/> hours from lying down, hour by hour, worked out from the schedule
///   (step 2) - so it is the night that comes, deterministic, with no draw. Without RAIN AND WIND it
///   is dry and still (the studio's 1.5 m/s): the cold alone. Where a colonist lies
///   (<see cref="ShelterOf"/>): in the open (the ground, or a gathering site not prepared) all of it,
///   by a prepared campfire <see cref="CampfireShelter"/> of it, in a residence or any other
///   container (a vehicle: the studio sleeps those like a house) none. With the defaults (thirty
///   years of one seed, lying down at 21:00, in the open unless said):
///     the stormiest mid-winter night   0.95, by a campfire 0.47, in a hut 0
///     a dry winter night               0.30-0.48 (the cold and the wind); 0.26-0.34 without RAIN AND WIND
///     "Spring", "Mid autumn"           mostly below 0.25; a rainy one up to about 0.3
///     every summer night               0
/// - COMFORT (<see cref="OnSleepStart"/>, <see cref="ExposurePenalty"/>, from GoalDoSleep.Activate
///   and Intelligence.Comfort, beside DiscomfortMod's). <see cref="ExposureComfortCost"/> x exposure:
///   a storm night out costs up to about 19% of the next day's comfort, by a campfire 9.5%. Kept on the
///   colonist (CustomFields "weather:exposureDay", "weather:exposure", saved with them), the worst
///   lie-down of the day; it holds until the next day's first sleep and lapses if a day passes
///   without one.
/// - A COLD (<see cref="ColdChance"/>). After a night of exposure <see cref="ColdFromExposure"/> or
///   more, once a day, a roll on the game's seeded generator (The.Sim.GameplayRandomGenerator, as
///   EmergencyEatingMod rolls a bad stomach, so a replay rolls the same) at
///   <see cref="ColdChanceAtFullExposure"/> x exposure: up to about 28% after a storm night in the
///   open, 14% by a campfire, 9-15% on a dry winter night out (8-10% without RAIN AND WIND), none in a
///   hut or in summer - and no draw then. Not while a colonist has
///   a cold already (no draw then): it does not stack or restart. "Cold" is a studio SimEffect
///   profile, registered always (<see cref="AddEffectProfiles"/>), shown in the colonist's effect
///   panel, saved with them, gone after <see cref="ColdDurationInDays"/> days: comfort -10% (its own
///   effect), and through <see cref="SicknessEffects"/>, shared with EmergencyEatingMod's bad
///   stomach, walking x0.9, work time x1.2 and energy at most 0.75. Those follow the effect, not the
///   switch.
/// - SHELTER (<see cref="ShelterScore"/>, from EvaluateSleep.ScoreComfort). The comfort score of a
///   place loses <see cref="ShelterScoreWeight"/> x its exposure tonight: on the storm night the open
///   scores 0.32 instead of 0.61, a campfire 0.50 instead of 0.66, a hut 0.92 as before. Comfort is 0.35 of a place's score and the
///   place 0.4 of the sleep goal's, so at most about 4% of the goal: it reorders the places a
///   colonist picks from - a hut, then a fire, then the ground - but hardly delays sleep itself.
/// - Off: no measuring, no roll, no penalty, the studio's sleep scores. A "Cold" already caught runs
///   its course.
///
/// WHAT KEEPS IT OFF (step 4, under the same switch). Kastuk, "Weather and temperature": "the trees
/// can be a shelter too (living spoak, not cut), especially live sanctuary tree with caps" and
/// "Explorers with precol suits surely must be more protected by default"; "Colonist equipment":
/// "New clothes need to protect against weather too ... Mackintosh to withstand the rain seasons."
/// Each of these is a <see cref="Cover"/>: a factor on each of the three shares of an hour's
/// exposure - the dry cold (the 0.5), the rain (0.3 x rain) and the wind (0.2 x wind) - so a roof
/// of leaves can keep off the rain and leave the cold. Covers multiply with each other and with
/// the place (a campfire's x0.5); a residence is still none at all (<see cref="CoverOf"/>):
/// - TREES (<see cref="TreeCover"/>, <see cref="CanopyOf"/>). The studio's trees are entities in
///   their tile's TerrainTile.TreesOnTile, with a Tree component. Living and dead are different
///   types - "tree:spoak" and the "tree:spoakdead..." ones, "tree:sanctuarytree" and
///   "tree:sanctuarytreedead..."; a felled tree is destroyed and leaves its tile's list. Only the two
///   living types count, grown (Tree.Size 0.9 or more: below it the studio draws the young sprite),
///   within <see cref="SpoakReachTiles"/> or <see cref="SanctuaryReachTiles"/> of the trunk. The
///   sanctuary tree's "caps" are its summer canopy: the studio draws it with them
///   (tree_sanctuarytree_1_grown_summer) unless the map made it InSeason.Winter, bare branches
///   (..._grown_winter); nothing in the game changes a tree's season but the map editor. The spoak's
///   own description says its rigid plates let "strong winds pass through the canopy", so it keeps
///   off more rain than wind. Several trees: the best of each share. Only the tiles within reach of
///   the sleeper are looked at (7 x 7).
///     a grown spoak                  rain x0.6, wind x0.8, cold as it is
///     a sanctuary tree with its caps rain x0.3, wind x0.5, cold x0.9
///     a sanctuary tree, bare         rain x0.8, wind x0.7, cold as it is
/// - THE PRECOL SUIT (<see cref="IsPrecolExplorer"/>). Nothing on a colonist says "suit": the white
///   suits are some of the model textures the "...HumanAncestor" races pick from, a client-side
///   choice that also gives the Muckroot research team PRECOL suits. What does say it is the
///   scenario: Twinkler Island's tables rename "entity:human" to "PRECOL explorer"
///   (EntityTypeDescription "personDescription", whose description issues every explorer a
///   SURVIVAL SUIT that shields "heat and cold" and "collects and rinses water from the wearer").
///   That table is never translated, so a person whose type the loaded scenario calls a "PRECOL
///   explorer" wears the suit: cold x0.4, rain x0.5, wind x0.5.
/// - CLOTHES (<see cref="GarmentCover"/>), worn as HideGearMod's gadgets - in the Equipment storage
///   when the colonist lies down. The studio's GoalDoSleep then drops everything a sleeper carries
///   where they lie ("just drop everything where we sleep"), so the cloak is the blanket beside
///   them. The hide cloak: cold x0.8, wind x0.7, the rain as it is. The Mackintosh (HideGearMod: a
///   cotton textile proofed with marshcot sap): cold x0.8, rain x0.3, wind x0.6. Both on, the Mackintosh.
/// The stormiest mid-winter night of the self-test, 0.95 in the open (dry cold 0.50, rain 0.25, wind
/// 0.20): by a spoak 0.81, under a sanctuary tree's caps 0.62 (bare 0.84), in a hide cloak 0.79, in a
/// Mackintosh 0.59, a PRECOL explorer 0.42 (0.27 under the caps), an explorer in a Mackintosh by a
/// lit fire under the caps 0.09 - below 0.25, so no cold. Nothing changes the studio's sleep
/// conditions; with no tree, no suit and no clothes, every number is step 3's to the bit.
/// EvaluateSleep.ScoreComfort (which now passes where the sleeper would lie) scores a spot by its
/// cover too: on that night the bare ground 0.32, a spot under the caps 0.42 (the studio's 0.61).
///
/// RAIN AND WIND DRY OR WET (step 5), a fourth switch that works with RAIN AND WIND on (off by
/// default). Kastuk, "Weather and temperature" (2026-10-18): "Wonder how rain can affect processes at
/// drying food and hides ... No, just revert slowly. Then wind without rain will speed up the drying
/// process. Way to put drying structures under trees protection." tripleacoder: "SimProcess is the
/// class that should handle physical processes like that." Separate from RAIN AND WIND so a game
/// played with the weather keeps the studio's drying until this is asked for.
/// - WHAT THE STUDIO HAS. A drying batch is a SimProcess that runs on its own once a colonist has
///   started it (WorkNeeded WorkerOnlyNeededToStart: "dries on its own", "the hide is drying on the
///   hide rack on its own") on a stationary tool, its outputs in the tool's container. Each update
///   (2 s: it creates outputs) EstimateProgressDelta adds update interval / (DaysNeeded / the tool's
///   productivity), whatever the weather. The drying ones, read from the tables (<see cref="IsDryingProcess"/>):
///   makeDriedThunderChicken, makeDriedStreakFin and makeTurnipSalami on a meat drying rack (x0.5: a
///   batch in 4.8 hours), a drying shed (x0.8, 3 h) or the cookhouse's drying shed addition (x0.9);
///   makeDryPeat on a peat stack and makeFirewood on a woodpile (x0.6, 4 h); the three
///   make...Rawhide on a hide rack. Separately, three items "can be dried in the open" by their degrade
///   type "sun drying" (wet firewood, wet mudbricks, cleaned ursinix): NonLivingEntity.Degrade wears
///   them down by temperature and sunshine, and its moisture is fixed at 0 ("disabled, for lack of UI
///   feedback").
/// - A BATCH (<see cref="DryingProgress"/>, from SimProcess.EstimateProgressDelta, after the studio's
///   progress factors and before its cap). On an open rack or stack out of doors - not one inside
///   anything - the progress is scaled by <see cref="DryingRate"/>: while it rains, BACKWARDS at
///   0.3 x the rain (0-1) x the normal rate, never below 1% (an hour of rain 0.7 takes a rack's
///   batch from 0.50 back to 0.45 instead of on to 0.71; a 12-hour rainstorm undoes about half a
///   batch); dry, faster in the wind, x(1 + 0.05 a m/s above 3), at most x1.5 (13 m/s) - a winter
///   wind of 9 m/s x1.3, the summer's 1-4 m/s hardly at all; still, dry air the studio's to the bit.
///   The drying shed and the addition (StructureLoader: "This shed will protect the meat from rain
///   while letting the wind blow through") keep the rain off - the studio's rate while it rains - and
///   take the wind. Under a living tree (step 4's <see cref="TreeCover"/>, at the structure) the rain
///   is the cover's share of it (a sanctuary tree's caps x0.3, a spoak x0.6) and the wind's speed-up
///   the cover's (x0.5, x0.8). A batch going back has no completion estimate
///   (SimProcess.ComputeEstimatedCompletionTime, which would otherwise say "now").
/// - LEFT OUT TO DRY (<see cref="SunDryingDamage"/>, from NonLivingEntity.Degrade, only for a thing in
///   no working storage): the same rate, but never below 0 - rain stops it and does not wet it
///   again, since a negative damage would raise its condition and MaxCondition, which the studio's
///   code does not expect. The estimates the haulers make (HaulingJobManager) and the tooltips are
///   left as they are.
/// - Off, or without RAIN AND WIND: the studio's, to the bit. Nothing new is saved: a batch's progress
///   is its outputs' Progress, as it always was.
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
/// COLD AND WET HURT is the exception on both counts, and needs no new save format: it keeps three
/// numbers in a colonist's CustomFields (saved as any CustomFields are) and draws one number from the
/// game's generator per colonist and day after a night out at exposure 0.25 or more; "Cold" is
/// registered whether or not the switch is on, so a save with one loads either way.
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
            "Seasons and temperature: the sun follows the season names, and a subtropical year of warm summers and cool winters decides how fast things spoil outside - and, if you like, a cold, wet night slept outside makes people ill.",
            "HUD_thumbnail_weatherAntenna");
        ModSettings.SetCategoryLabel(ModId, "WEATHER");
        _ = EnabledSetting;
        _ = RainSetting;
        _ = DryingSetting;
        _ = ExposureSetting;
    }

    public static bool On => EnabledSetting.On;

    /// <summary>RAIN AND WIND, which needs SEASONS AND TEMPERATURE: the rain cools the air that switch makes.</summary>
    public static bool RainOn => On && RainSetting.On;

    private static ModSetting drying;

    public static ModSetting DryingSetting =>
        drying ?? (drying = ModSettings.Toggle(
            ModId, "drying", "RAIN AND WIND DRY OR WET", defaultValue: false,
            toolTip: "Works with RAIN AND WIND on. Meat, fish, salami, hides, peat and firewood drying on a rack " +
                     "or a stack out in the open slowly lose their drying while it rains, and dry up to half as " +
                     "fast again in a dry wind. A drying shed keeps the rain off and lets the wind through; a " +
                     "living spoak keeps off some of the rain, a sanctuary tree with its caps most of it. Wet " +
                     "firewood, mudbricks and the like left out to sun dry stop drying in the rain and dry faster " +
                     "in the wind.",
            affectsSimulation: true));

    /// <summary>RAIN AND WIND DRY OR WET, which needs RAIN AND WIND: it is that switch's rain and wind.</summary>
    public static bool DryingOn => RainOn && DryingSetting.On;

    private static ModSetting exposure;

    public static ModSetting ExposureSetting =>
        exposure ?? (exposure = ModSettings.Toggle(
            ModId, "exposure", "COLD AND WET HURT", defaultValue: false,
            toolTip: "Works with SEASONS AND TEMPERATURE on, and with RAIN AND WIND brings in the rain and the wind. " +
                     "A colonist who sleeps out in the open on a night below about 12 C loses comfort the next " +
                     "day - up to about 19% after a winter rainstorm - and may catch a cold: slower walking and " +
                     "work, energy at most 75%, for a day and a half. A lit campfire halves it, a hut keeps it off, " +
                     "and on cold, wet nights colonists look for shelter more. A living spoak keeps off some of the " +
                     "rain and wind, a sanctuary tree with its caps most of it; PRECOL explorers' suits keep off half " +
                     "or more; a hide cloak some of the cold and wind, a Mackintosh the rain as well.",
            affectsSimulation: true));

    /// <summary>COLD AND WET HURT, which needs SEASONS AND TEMPERATURE: it is that switch's air that is cold.</summary>
    public static bool ExposureOn => On && ExposureSetting.On;

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

    // ---------------------------------------------------------------- cold and wet: tunable here

    /// <summary>Air (C) below which a night in the open chills a sleeper.</summary>
    public const float ExposureFromCelsius = 12f;

    /// <summary>Degrees below <see cref="ExposureFromCelsius"/> for the full cold: 10, so 2 C, the floor (<see cref="FloorCelsius"/>).</summary>
    public const float ExposureColdSpan = 10f;

    /// <summary>The share of the full exposure dry, still cold gives.</summary>
    public const float ExposureColdShare = 0.5f;

    /// <summary>The share a full rainstorm adds to the cold.</summary>
    public const float ExposureRainShare = 0.3f;

    /// <summary>The share wind adds to the cold, all of it at <see cref="ExposureWindSpan"/> m/s above <see cref="ChillFromWind"/>.</summary>
    public const float ExposureWindShare = 0.2f;

    /// <summary>Wind (m/s) above <see cref="ChillFromWind"/> for the whole wind share: 15 m/s, the studio's "High wind".</summary>
    public const float ExposureWindSpan = 12f;

    /// <summary>The share of the night's exposure that reaches a sleeper by a prepared campfire.</summary>
    public const float CampfireShelter = 0.5f;

    /// <summary>How long the night measured from lying down is (hours).</summary>
    public const int NightHours = 8;

    /// <summary>The comfort (a fraction, like the comfort effects) the next day loses at exposure 1.</summary>
    public const float ExposureComfortCost = 0.2f;

    /// <summary>The chance of a cold after a night at exposure 1; proportional below.</summary>
    public const float ColdChanceAtFullExposure = 0.3f;

    /// <summary>Exposure below which no cold is caught (and nothing is drawn).</summary>
    public const float ColdFromExposure = 0.25f;

    /// <summary>How long a cold lasts (days).</summary>
    public const double ColdDurationInDays = 1.5;

    /// <summary>A cold's comfort (its own effect, in the panel).</summary>
    public const float ColdComfort = 0.1f;

    /// <summary>Walking with a cold (x).</summary>
    public const float ColdSpeedFactor = 0.9f;

    /// <summary>Work time with a cold (x).</summary>
    public const float ColdWorkTimeFactor = 1.2f;

    /// <summary>The most energy a colonist with a cold has.</summary>
    public const float ColdEnergyCap = 0.75f;

    /// <summary>The share of a sleeping place's comfort score lost at exposure 1 (EvaluateSleep.ScoreComfort).</summary>
    public const float ShelterScoreWeight = 0.5f;

    public const string ColdProfile = "weatherCold";
    private const string ColdComfortEffect = ColdProfile + "Comfort";

    public const string ExposureDayKey = "weather:exposureDay";
    public const string ExposureKey = "weather:exposure";
    public const string ColdRollDayKey = "weather:coldRollDay";

    // ---------------------------------------------------------------- cold and wet: the model

    /// <summary>Where a colonist sleeps, as the weather reaches them.</summary>
    public enum Shelter { Open, Campfire, Roof }

    /// <summary>
    /// The studio's classes of sleeping place (GoalDoSleep.ComputeSleepConditions): none, or a
    /// gathering site not prepared, is the open; a prepared gathering site is a campfire; a residence
    /// - or any other container, which the studio sleeps like a house - has a roof.
    /// </summary>
    public static Shelter ShelterOf(IKnownEntityData place)
    {
        EntityType type = place?.EntityType;
        if (type == null)
        {
            return Shelter.Open;
        }
        if (type.ContainerType?.ResidenceType != null)
        {
            return Shelter.Roof;
        }
        if (type.GatheringSiteType != null)
        {
            return place.IsPrepared == true ? Shelter.Campfire : Shelter.Open;
        }
        return Shelter.Roof;
    }

    /// <summary>The share of the exposure in the open that reaches a sleeper in this shelter.</summary>
    public static float Sheltered(float exposureInTheOpen, Shelter shelter) =>
        shelter == Shelter.Open ? exposureInTheOpen
        : shelter == Shelter.Campfire ? exposureInTheOpen * CampfireShelter
        : 0f;

    /// <summary>
    /// An hour in the open (0-1) at this air (K), rain (0-1) and wind (m/s): the cold, (12 C - air) / 10
    /// between 0 and 1, times 0.5 + 0.3 x rain + 0.2 x wind (0 at 3 m/s, 1 at 15 m/s).
    /// </summary>
    public static float ExposureOf(float airKelvin, float rainAmount, float wind)
    {
        float cold = Common.Clamp((ExposureFromCelsius - (airKelvin - ZeroCelsius)) / ExposureColdSpan, 0f, 1f);
        float windy = Common.Clamp((wind - ChillFromWind) / ExposureWindSpan, 0f, 1f);
        return cold * (ExposureColdShare + ExposureRainShare * rainAmount + ExposureWindShare * windy);
    }

    /// <summary>
    /// The night ahead in the open, from a moment: the average of <see cref="NightHours"/> hours, each
    /// at its middle. With <paramref name="rainAndWind"/> the schedule's rain, wind, cloud and cooled
    /// air; without, the air under <paramref name="cloudCover"/>, dry, at the studio's still wind.
    /// </summary>
    public static float NightExposure(int seed, int year, double timeOfYear, double timeOfDay, bool rainAndWind, float cloudCover) =>
        NightOf(seed, year, timeOfYear, timeOfDay, rainAndWind, cloudCover).Open;

    /// <summary>
    /// A night ahead, as <see cref="NightExposure"/> works it out: the exposure in the open, and the
    /// three shares it is made of, each averaged over the same hours - for a <see cref="Cover"/>.
    /// </summary>
    public readonly struct Night
    {
        /// <summary>The night in the open, summed hour by hour exactly as step 3 sums it.</summary>
        public readonly float Open;
        /// <summary>The dry cold's share: cold x 0.5.</summary>
        public readonly float Cold;
        /// <summary>The rain's share: cold x 0.3 x rain.</summary>
        public readonly float Rain;
        /// <summary>The wind's share: cold x 0.2 x wind.</summary>
        public readonly float Wind;

        public Night(float open, float cold, float rain, float wind)
        {
            Open = open;
            Cold = cold;
            Rain = rain;
            Wind = wind;
        }

        /// <summary>
        /// The night under a cover: each share times its factor. With no cover, <see cref="Open"/> -
        /// step 3's own number to the bit, not the sum of the shares (which may differ in the last bit).
        /// </summary>
        public float Under(Cover cover) =>
            cover.IsNone ? Open : cover.Cold * Cold + cover.Rain * Rain + cover.Wind * Wind;
    }

    /// <summary>The night ahead from a moment (<see cref="NightExposure"/>), with its three shares.</summary>
    public static Night NightOf(int seed, int year, double timeOfYear, double timeOfDay, bool rainAndWind, float cloudCover)
    {
        double now = ScheduleDays(year, timeOfYear);
        float sum = 0f, coldSum = 0f, rainSum = 0f, windSum = 0f;
        for (int h = 0; h < NightHours; h++)
        {
            double offset = (h + 0.5) * Hour;
            double days = now + offset;
            double toy = YearFraction(days / DateAndTime.DaysPerYear);
            float elevation = SunElevation(toy, YearFraction(timeOfDay + offset));
            float air, rainNow, wind;
            if (rainAndWind)
            {
                State s = Scheduled(seed, days);
                air = Cooled(AirTemperature(toy, elevation, s.Cloud), s);
                rainNow = s.Rain;
                wind = s.Wind;
            }
            else
            {
                air = AirTemperature(toy, elevation, cloudCover);
                rainNow = 0f;
                wind = StudioWindSpeed;
            }
            sum += ExposureOf(air, rainNow, wind);
            float cold = Common.Clamp((ExposureFromCelsius - (air - ZeroCelsius)) / ExposureColdSpan, 0f, 1f);
            float windy = Common.Clamp((wind - ChillFromWind) / ExposureWindSpan, 0f, 1f);
            coldSum += cold * ExposureColdShare;
            rainSum += cold * ExposureRainShare * rainNow;
            windSum += cold * ExposureWindShare * windy;
        }
        return new Night(sum / NightHours, coldSum / NightHours, rainSum / NightHours, windSum / NightHours);
    }

    private static double cachedNightDays = double.NaN;
    private static int cachedNightSeed;
    private static bool cachedNightRain;
    private static float cachedNightCloud;
    private static Night cachedNight;

    /// <summary>The night ahead in the open from now (<see cref="NightExposure"/>), or null with no play site.</summary>
    public static float? TonightExposure() => Tonight()?.Open;

    /// <summary>
    /// The night ahead from now, with its shares (<see cref="NightOf"/>), or null with no play site.
    /// Kept for the moment: EvaluateSleep asks it for every place it scores.
    /// </summary>
    public static Night? Tonight()
    {
        WeatherManager weather = The.Sim?.PlaySite?.PlaySite?.Weather;
        DateAndTime date = The.Sim?.DateAndTime;
        if (weather == null || date == null)
        {
            return null;
        }
        double days = ScheduleDays(date.Year, date.TimeOfYear);
        int seed = Seed;
        bool rainOn = RainOn;
        float cloud = weather.CloudCover;
        if (days != cachedNightDays || seed != cachedNightSeed || rainOn != cachedNightRain || cloud != cachedNightCloud)
        {
            cachedNight = NightOf(seed, date.Year, date.TimeOfYear, date.TimeOfDay, rainOn, cloud);
            cachedNightDays = days;
            cachedNightSeed = seed;
            cachedNightRain = rainOn;
            cachedNightCloud = cloud;
        }
        return cachedNight;
    }

    // ---------------------------------------------------------------- cold and wet: what keeps it off (step 4)

    /// <summary>
    /// What keeps part of the night off a sleeper besides the place: a factor on each share of an
    /// hour's exposure (<see cref="ExposureOf"/>) - the dry cold, the rain, the wind. 1 lets it all
    /// through. Covers multiply.
    /// </summary>
    public readonly struct Cover
    {
        public readonly float Cold;
        public readonly float Rain;
        public readonly float Wind;

        public Cover(float cold, float rain, float wind)
        {
            Cold = cold;
            Rain = rain;
            Wind = wind;
        }

        /// <summary>Nothing: all of it gets through.</summary>
        public static readonly Cover None = new Cover(1f, 1f, 1f);

        public bool IsNone => Cold == 1f && Rain == 1f && Wind == 1f;

        public static Cover operator *(Cover a, Cover b) => new Cover(a.Cold * b.Cold, a.Rain * b.Rain, a.Wind * b.Wind);
    }

    /// <summary>A grown, living spoak: its rigid leaf plates keep off rain, and let "strong winds pass through".</summary>
    public static readonly Cover SpoakCover = new Cover(1f, 0.6f, 0.8f);

    /// <summary>A grown, living sanctuary tree with its caps (its summer canopy): most of the rain, half the wind.</summary>
    public static readonly Cover SanctuaryCapsCover = new Cover(0.9f, 0.3f, 0.5f);

    /// <summary>A grown, living sanctuary tree without its caps (InSeason.Winter): the trunks still break the wind.</summary>
    public static readonly Cover SanctuaryBareCover = new Cover(1f, 0.8f, 0.7f);

    /// <summary>The PRECOL survival suit: it shields "heat and cold" and "collects and rinses water from the wearer".</summary>
    public static readonly Cover PrecolSuitCover = new Cover(0.4f, 0.5f, 0.5f);

    /// <summary>HideGearMod's hide cloak: warmer and out of the wind, but it soaks through.</summary>
    public static readonly Cover CloakCover = new Cover(0.8f, 1f, 0.7f);

    /// <summary>HideGearMod's Mackintosh, cotton textile proofed with marshcot sap: the cloak's warmth, and the rain kept off.</summary>
    public static readonly Cover MackintoshCover = new Cover(0.8f, 0.3f, 0.6f);

    /// <summary>The studio's living spoak (TreeLoader).</summary>
    public const string SpoakKey = "tree:spoak";

    /// <summary>The studio's living sanctuary tree (TreeLoader); its dead ones are other keys.</summary>
    public const string SanctuaryTreeKey = "tree:sanctuarytree";

    /// <summary>
    /// How far from a spoak's trunk its canopy covers a sleeper, in tiles (48 world units): its
    /// billboard is about three tiles across.
    /// </summary>
    public const float SpoakReachTiles = 1.5f;

    /// <summary>
    /// How far from a sanctuary tree's trunk its caps cover a sleeper, in tiles. The studio blocks a
    /// circle of 45 units (about a tile) around the trunk (its GeometryLayoutType), and the caps spread
    /// some two and a half times as wide.
    /// </summary>
    public const float SanctuaryReachTiles = 2.5f;

    /// <summary>A tree this size or more is grown: the studio's Tree.GetSize draws the young sprite below it.</summary>
    public const float GrownTreeSize = 0.9f;

    /// <summary>What Twinkler Island's tables call a person (Scenario 1's EntityTypeDescription "personDescription").</summary>
    public const string PrecolExplorerName = "PRECOL explorer";

    /// <summary>
    /// Everything that keeps the night off this sleeper lying at <paramref name="at"/>: the trees over
    /// the spot, a PRECOL suit, the clothes worn. Multiplied; <see cref="Cover.None"/> with none of them.
    /// </summary>
    public static Cover CoverOf(Entity sleeper, Vector3? at)
    {
        Cover cover = at.HasValue ? TreeCover(at.Value) : Cover.None;
        if (IsPrecolExplorer(sleeper))
        {
            cover *= PrecolSuitCover;
        }
        return cover * GarmentCover(sleeper);
    }

    /// <summary>
    /// The trees over a spot: the best of each share among the grown, living spoaks and sanctuary trees
    /// within reach (<see cref="CanopyOf"/>). Looks only at the tiles within the longer reach.
    /// </summary>
    public static Cover TreeCover(Vector3 at)
    {
        MapManager map = The.Map;
        if (map?.TileMap == null)
        {
            return Cover.None;
        }
        Point centre = MapManager.WorldPosToTile(at);
        int reach = (int)Math.Ceiling(Math.Max(SpoakReachTiles, SanctuaryReachTiles));
        float cold = 1f, rainCover = 1f, wind = 1f;
        for (int x = centre.X - reach; x <= centre.X + reach; x++)
        {
            for (int y = centre.Y - reach; y <= centre.Y + reach; y++)
            {
                List<Entity> trees = map.TileIsOnMap(x, y) ? map.GetTile(x, y)?.TreesOnTile : null;
                if (trees == null)
                {
                    continue;
                }
                foreach (Entity tree in trees)
                {
                    Cover? canopy = CanopyOf(tree, at);
                    if (canopy.HasValue)
                    {
                        cold = Math.Min(cold, canopy.Value.Cold);
                        rainCover = Math.Min(rainCover, canopy.Value.Rain);
                        wind = Math.Min(wind, canopy.Value.Wind);
                    }
                }
            }
        }
        return new Cover(cold, rainCover, wind);
    }

    /// <summary>
    /// What one tree keeps off a sleeper at a spot: null unless it is a living spoak or sanctuary tree,
    /// grown, with the spot within its reach. A sanctuary tree has its caps unless the map made it winter.
    /// </summary>
    public static Cover? CanopyOf(Entity tree, Vector3 at)
    {
        string key = tree?.EntityType?.KeyName;
        float reachTiles = key == SpoakKey ? SpoakReachTiles : key == SanctuaryTreeKey ? SanctuaryReachTiles : 0f;
        if (reachTiles <= 0f || !tree.Find(out Tree living) || living.Size < GrownTreeSize)
        {
            return null;
        }
        Vector3? trunk = tree.Location;
        float reach = reachTiles * MapManager.tileSize;
        if (!trunk.HasValue || Vector2.DistanceSquared(trunk.Value.ToVector2(), at.ToVector2()) > reach * reach)
        {
            return null;
        }
        return key == SpoakKey ? SpoakCover : living.InSeason == InSeason.Winter ? SanctuaryBareCover : SanctuaryCapsCover;
    }

    /// <summary>
    /// Whether this person wears a PRECOL survival suit: the loaded scenario's tables call their type
    /// a "PRECOL explorer" (Twinkler Island). The description table is never translated.
    /// </summary>
    public static bool IsPrecolExplorer(Entity person)
    {
        EntityType type = person?.EntityType;
        GameDataCollection<EntityTypeDescription> descriptions = GameData.Instance?.AllEntityTypeDescriptions;
        if (type?.Person == null || descriptions == null)
        {
            return false;
        }
        foreach (EntityTypeDescription description in descriptions.Values)
        {
            if (description != null && description.EntityType == type.KeyName && description.EntityName == PrecolExplorerName)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>The clothes a colonist wears (HideGearMod's, in Equipment): a Mackintosh, else a hide cloak, else nothing.</summary>
    public static Cover GarmentCover(Entity colonist) =>
        HideGearMod.Wears(colonist, HideGearMod.MackintoshKey) ? MackintoshCover
        : HideGearMod.Wears(colonist, HideGearMod.CloakKey) ? CloakCover
        : Cover.None;

    /// <summary>The chance of a cold after a night at this exposure: none below <see cref="ColdFromExposure"/>.</summary>
    public static float ColdChance(float exposureTonight) =>
        exposureTonight < ColdFromExposure ? 0f : Math.Min(1f, ColdChanceAtFullExposure * exposureTonight);

    /// <summary>Today, as Year * 1000 + Day (as DiscomfortMod counts); Day goes up at midnight and back to 1 at the new year.</summary>
    private static int Today => The.Sim.DateAndTime.Year * 1000 + The.Sim.DateAndTime.Day;

    /// <summary>
    /// Whether a day counted as <see cref="Today"/> is today or yesterday - across the new year too,
    /// where yesterday is the old year's last day (12th or later).
    /// </summary>
    private static bool TodayOrYesterday(float measured)
    {
        DateAndTime date = The.Sim.DateAndTime;
        int today = Today;
        return measured == today || measured == today - 1
               || (date.Day == 1 && measured >= (date.Year - 1) * 1000 + (int)DateAndTime.DaysPerYear && measured < date.Year * 1000);
    }

    // ---------------------------------------------------------------- cold and wet: hooks

    /// <summary>
    /// Called from GoalDoSleep.Activate as a colonist lies down in <paramref name="sleepingPlace"/>
    /// (null on the ground): the night's exposure there, the worst of the day kept for the comfort;
    /// and, once a day, the roll for a cold.
    /// </summary>
    public static void OnSleepStart(Entity sleeper, Entity sleepingPlace)
    {
        if (!ExposureOn || sleeper?.EntityType?.Person == null || The.Sim?.DateAndTime == null)
        {
            return;
        }
        Night? night = Tonight();
        if (!night.HasValue)
        {
            return;
        }
        float exposed = Exposed(night.Value, sleeper, sleepingPlace, sleeper.Location);
        int today = Today;
        bool measuredToday = Number(sleeper, ExposureDayKey) == today;
        if (!measuredToday || exposed > (Number(sleeper, ExposureKey) ?? 0f))
        {
            Entity.SetPropertyValue(ref sleeper.CustomFields, ExposureDayKey, new PropertyResult { NumberResult = today });
            Entity.SetPropertyValue(ref sleeper.CustomFields, ExposureKey, new PropertyResult { NumberResult = exposed });
        }
        float chance = ColdChance(exposed);
        if (chance <= 0f || sleeper.SimEffects == null || HasCold(sleeper) || Number(sleeper, ColdRollDayKey) == today)
        {
            return;
        }
        Entity.SetPropertyValue(ref sleeper.CustomFields, ColdRollDayKey, new PropertyResult { NumberResult = today });
        if (The.Sim.GameplayRandomGenerator.RandomBetween(0f, 1f) < chance
            && GameData.Instance.AllEffectProfileTypes.TryGetValue(ColdProfile, out EffectProfileType cold))
        {
            sleeper.SimEffects.Start(cold);
        }
    }

    /// <summary>
    /// Called from the Intelligence.Comfort getter: the comfort lost to the night measured today or
    /// yesterday (<see cref="ExposureComfortCost"/> x its exposure); 0 otherwise.
    /// </summary>
    public static float ExposurePenalty(Entity colonist)
    {
        if (!ExposureOn || colonist?.CustomFields == null || The.Sim?.DateAndTime == null)
        {
            return 0f;
        }
        float? day = Number(colonist, ExposureDayKey);
        if (!day.HasValue || !TodayOrYesterday(day.Value))
        {
            return 0f;
        }
        return ExposureComfortCost * (Number(colonist, ExposureKey) ?? 0f);
    }

    /// <summary>
    /// Called from EvaluateSleep.ScoreComfort with the studio's comfort score of a sleeping place and
    /// where the sleeper would lie: for a person, less <see cref="ShelterScoreWeight"/> x the exposure
    /// there tonight, under what covers them there; the studio's otherwise.
    /// </summary>
    public static double ShelterScore(Entity sleeper, IKnownEntityData place, Vector3 at, double stock)
    {
        if (!ExposureOn || sleeper?.EntityType?.Person == null)
        {
            return stock;
        }
        Night? night = Tonight();
        return night.HasValue ? stock * (1.0 - ShelterScoreWeight * Exposed(night.Value, sleeper, place, at)) : stock;
    }

    /// <summary>
    /// The night that reaches a sleeper lying in <paramref name="place"/> (null: the ground) at
    /// <paramref name="at"/>: none under a roof; otherwise the night under their cover, then the place's share.
    /// </summary>
    public static float Exposed(Night night, Entity sleeper, IKnownEntityData place, Vector3? at)
    {
        Shelter shelter = ShelterOf(place);
        return shelter == Shelter.Roof ? 0f : Sheltered(night.Under(CoverOf(sleeper, at)), shelter);
    }

    /// <summary>Whether a colonist has a cold now.</summary>
    public static bool HasCold(Entity colonist)
    {
        if (colonist?.SimEffects?.EffectProfiles == null)
        {
            return false;
        }
        foreach (SimEffectProfile active in colonist.SimEffects.EffectProfiles)
        {
            if (active?.EffectProfileType?.KeyName == ColdProfile)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>Called at the end of BaseDataLoader.InitEffectTypes - always, so a save with a cold loads.</summary>
    public static void AddEffects(List<EffectType> list)
    {
        list.Add(new NumberEffectType
        {
            KeyName = ColdComfortEffect,
            Name = "Cold",
            Affects = AffectsNumbers.AgentComfort,
            FormatAsPercentage = true,
            DurationInDays = ColdDurationInDays,
            Intensity = -ColdComfort
        });
    }

    /// <summary>
    /// Called at the end of BaseDataLoader.InitEffectProfileTypes - always, as AddEffects - and
    /// registers the cold's walking, work and energy with <see cref="SicknessEffects"/>.
    /// </summary>
    public static void AddEffectProfiles(List<EffectProfileType> list)
    {
        list.Add(new EffectProfileType
        {
            KeyName = ColdProfile,
            Name = "Cold",
            Description = "Caught a cold sleeping out on a cold, wet night: walks " + Percent(1f - ColdSpeedFactor) + " slower, work takes " +
                          Percent(ColdWorkTimeFactor - 1f) + " longer, and energy is at most " + Percent(ColdEnergyCap) + ".",
            Effects = new[] { ColdComfortEffect },
            SortOrder = 170
        });
        SicknessEffects.Register(ColdProfile, ColdSpeedFactor, ColdWorkTimeFactor, ColdEnergyCap);
    }

    // ---------------------------------------------------------------- rain and wind dry or wet: tunable here

    /// <summary>
    /// The share of a drying process's normal rate that full rain (1) takes back: in a rain of r that
    /// gets through to it, a batch goes backwards at r times this.
    /// </summary>
    public const float RainUndoesDrying = 0.3f;

    /// <summary>Wind (m/s) above which a dry wind speeds drying.</summary>
    public const float DryingWindFrom = 3f;

    /// <summary>How much faster each m/s of wind above <see cref="DryingWindFrom"/> dries (a share of the normal rate).</summary>
    public const float DryingPerWind = 0.05f;

    /// <summary>The fastest a dry wind dries (x the normal rate): reached at 13 m/s.</summary>
    public const float MaxWindDrying = 1.5f;

    /// <summary>
    /// Rain never takes a batch's progress below this. At 0 the studio takes an output as not started
    /// (NonLivingEntity.IsStarted), which is for a batch nobody has begun.
    /// </summary>
    public const float DryingFloor = 0.01f;

    /// <summary>The studio's degrade type for things that dry where they lie (wet firewood, wet mudbricks, cleaned ursinix).</summary>
    public const string SunDryingKey = "sun drying";

    /// <summary>How a drying structure meets the weather.</summary>
    public enum DryingPlace
    {
        /// <summary>Not a drying structure, or one indoors: the studio's rate.</summary>
        None,
        /// <summary>A frame or a stack in the open: the rain and the wind reach it.</summary>
        Open,
        /// <summary>A drying shed: "will protect the meat from rain while letting the wind blow through".</summary>
        Shed
    }

    /// <summary>
    /// The studio's drying structures: the meat drying rack ("a simple frame"), the hide rack, the peat
    /// stack and the woodpile are open; the drying shed and the cookhouse's drying shed addition have a
    /// roof that keeps the rain off and let the wind through (StructureLoader: "This shed will protect
    /// the meat from rain while letting the wind blow through").
    /// </summary>
    public static DryingPlace DryingPlaceOf(EntityType type) => type?.KeyName switch
    {
        "structure:meatDryingRack" or "structure:hideRack" or "structure:peatStack" or "structure:firewoodStack" => DryingPlace.Open,
        "structure:dryingShed" or "item:dryingShedUpgrade" => DryingPlace.Shed,
        _ => DryingPlace.None
    };

    // ---------------------------------------------------------------- rain and wind dry or wet: the model

    /// <summary>
    /// Whether a process type dries on a drying structure: it runs on its own once started (not
    /// WorkerNeeded - the studio's "dries on its own", "the hide is drying on the hide rack on its own")
    /// and one of its tools is a drying structure (<see cref="DryingPlaceOf"/>).
    /// </summary>
    public static bool IsDryingProcess(ProcessType type)
    {
        if (type == null || type.WorkNeeded == WorkerNeededOptions.WorkerNeeded || type.ProcessToolSet?.Tools == null)
        {
            return false;
        }
        foreach (ToolAlternatives alternatives in type.ProcessToolSet.Tools)
        {
            foreach (UWGame.SimSide.Processes.Tool tool in alternatives?.Tools ?? Array.Empty<UWGame.SimSide.Processes.Tool>())
            {
                foreach (EntityType toolType in tool.ToolEntityTypes)
                {
                    if (DryingPlaceOf(toolType) != DryingPlace.None)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    /// <summary>
    /// The share of its normal rate something drying makes in this weather, under this cover, in the
    /// open or under a shed's roof. While it rains (<see cref="RainThreshold"/>) on it: backwards,
    /// -<see cref="RainUndoesDrying"/> x the rain that gets through. While it rains but too little gets
    /// through (a shed, or a canopy that keeps a shower off): the normal rate - the air is wet, so no
    /// wind bonus. Dry: 1 + <see cref="DryingPerWind"/> a m/s of wind above <see cref="DryingWindFrom"/>,
    /// at most <see cref="MaxWindDrying"/>, the bonus times the cover's wind factor.
    /// </summary>
    public static float DryingRate(State weather, Cover cover, DryingPlace place)
    {
        if (place == DryingPlace.None)
        {
            return 1f;
        }
        if (weather.Rain >= RainThreshold)
        {
            float wet = place == DryingPlace.Shed ? 0f : weather.Rain * cover.Rain;
            return wet >= RainThreshold ? -RainUndoesDrying * wet : 1f;
        }
        float bonus = Math.Min(MaxWindDrying - 1f, DryingPerWind * Math.Max(0f, weather.Wind - DryingWindFrom));
        return 1f + bonus * cover.Wind;
    }

    /// <summary>
    /// The drying structure a process runs on, if any: the first of its stationary tools that is one,
    /// and how the weather reaches it. An open rack or stack inside something (a building's storage) is
    /// out of the weather: null, <see cref="DryingPlace.None"/>.
    /// </summary>
    public static Entity DryingStructureOf(SimProcess process, out DryingPlace place)
    {
        place = DryingPlace.None;
        if (process?.StationaryTools == null)
        {
            return null;
        }
        foreach (EntityID id in process.StationaryTools)
        {
            Entity tool = Entity.FindByID(id);
            DryingPlace found = DryingPlaceOf(tool?.EntityType);
            if (found == DryingPlace.None)
            {
                continue;
            }
            if (found == DryingPlace.Open && tool.ContainedBy.HasValue)
            {
                return null;
            }
            place = found;
            return tool;
        }
        return null;
    }

    /// <summary>The trees over a spot, looked for only when the weather makes them matter: rain, or a wind that dries.</summary>
    private static Cover DryingCover(State weather, Vector3? at) =>
        at.HasValue && (weather.Rain >= RainThreshold || weather.Wind > DryingWindFrom) ? TreeCover(at.Value) : Cover.None;

    // ---------------------------------------------------------------- rain and wind dry or wet: hooks

    /// <summary>
    /// Called by SimProcess.EstimateProgressDelta with the process's progress and the studio's progress
    /// for this update (after its progress factors, before its cap): for a drying process
    /// (<see cref="IsDryingProcess"/>) on a drying structure out in the weather, that times
    /// <see cref="DryingRate"/> - negative in the rain, but never taking the batch below
    /// <see cref="DryingFloor"/>. The studio's otherwise, to the bit.
    /// </summary>
    public static float DryingProgress(SimProcess process, float progress, float stock)
    {
        if (!DryingOn || stock <= 0f || !IsDryingProcess(process?.ProcessType))
        {
            return stock;
        }
        Entity structure = DryingStructureOf(process, out DryingPlace place);
        State? now = Now();
        if (structure == null || !now.HasValue)
        {
            return stock;
        }
        float rate = DryingRate(now.Value, DryingCover(now.Value, structure.Location), place);
        if (rate == 1f)
        {
            return stock;
        }
        float delta = stock * rate;
        return delta >= 0f ? delta : Math.Max(delta, Math.Min(0f, DryingFloor - progress));
    }

    /// <summary>
    /// Called by NonLivingEntity.Degrade with the damage the studio worked out for an item exposed to the
    /// elements (in no working storage): for an item that sun dries (<see cref="SunDryingKey"/>) and is
    /// not weather proof, that times <see cref="DryingRate"/> where it lies - but not below 0: rain stops
    /// it drying and does not wet it again (a negative damage would raise its condition and its
    /// MaxCondition, which the studio's code does not expect). The studio's otherwise, to the bit.
    /// </summary>
    public static float SunDryingDamage(Entity item, float stock)
    {
        if (!DryingOn || stock <= 0f || item?.EntityType?.NonLivingType?.FinalDegradeType?.KeyName != SunDryingKey || item.IsWeatherProof())
        {
            return stock;
        }
        State? now = Now();
        if (!now.HasValue)
        {
            return stock;
        }
        float rate = DryingRate(now.Value, DryingCover(now.Value, item.Location), DryingPlace.Open);
        return rate == 1f ? stock : stock * Math.Max(0f, rate);
    }

    private static string Percent(float fraction) =>
        Math.Round(fraction * 100f).ToString(System.Globalization.CultureInfo.InvariantCulture) + "%";

    private static float? Number(Entity entity, string key) =>
        entity.CustomFields != null && entity.CustomFields.TryGetValue(key, out var stored) ? stored.NumberResult : null;
}
