using System.Collections.Generic;
using UWGame.ClientSide.Interface.DateAndWeatherPanel;
using UWGame.SimSide.AI;
using UWGame.SimSide.Entities;
using UWGame.SimSide.SimEffects;

namespace UWGame.Mods;

/// <summary>
/// Seasons and temperature, compiled OUT. See PeatMod.Absent.cs for the pattern: the studio's
/// sawtooth sun, 15 C everywhere, and the date panel's fixed temperature. No "Cold" effect; nobody
/// minds where they sleep beyond what the studio scores.
/// </summary>
public static class WeatherMod
{
    public const string ModId = "weather";

    public static void RegisterSettings()
    {
    }

    /// <summary>The studio's: axialTilt * (-1 + 2 * TimeOfYear).</summary>
    public static float Declination(double timeOfYear, float stock) => stock;

    /// <summary>The studio's: the tile's own temperature, 288 K unless a test set it.</summary>
    public static float OutsideTemperature(float tileTemperature) => tileTemperature;

    /// <summary>The studio's: Constants.MeanAmbientTemperature.</summary>
    public static float ShelfLifeTemperature(float stock) => stock;

    /// <summary>The studio's: the panel keeps the temperatures and the forecast it was built with.</summary>
    public static void ShowTemperature(HUDDateAndWeather panel)
    {
    }

    /// <summary>The studio's: the cloud cover drifts to its own random targets.</summary>
    public static float? SteerCloudCover() => null;

    /// <summary>The studio's: the wind speed never changes.</summary>
    public static float SteerWindSpeed(float stock) => stock;

    /// <summary>The studio's cloud term; there is no rain.</summary>
    public static string SkyTerm(string stock) => stock;

    public static void AddEffects(List<EffectType> list)
    {
    }

    public static void AddEffectProfiles(List<EffectProfileType> list)
    {
    }

    /// <summary>The studio's: lying down outside on a cold night costs nothing.</summary>
    public static void OnSleepStart(Entity sleeper, Entity sleepingPlace)
    {
    }

    /// <summary>The studio's: comfort is the sum of the comfort effects.</summary>
    public static float ExposurePenalty(Entity colonist) => 0f;

    /// <summary>The studio's comfort score of a sleeping place.</summary>
    public static double ShelterScore(Entity sleeper, IKnownEntityData place, double stock) => stock;
}
