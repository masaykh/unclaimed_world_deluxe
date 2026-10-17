using UWGame.ClientSide.Interface.DateAndWeatherPanel;

namespace UWGame.Mods;

/// <summary>
/// Seasons and temperature, compiled OUT. See PeatMod.Absent.cs for the pattern: the studio's
/// sawtooth sun, 15 C everywhere, and the date panel's fixed temperature.
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

    /// <summary>The studio's: the panel keeps the temperature it was built with.</summary>
    public static void ShowTemperature(HUDDateAndWeather panel)
    {
    }
}
