using System;
using System.Globalization;
using System.IO;

namespace UWGame;

public static class Config
{
	public enum DataType
	{
		BaseData,
		RGScenario,
		RGMap,
		UserScenarios,
		UserMods,
		UserMaps,
		UserSettings,
		Replays,
		SaveGames,
		/// <summary>
		/// The folder holding <c>ModSettings.xml</c> - <c>user/</c> in the game folder, beside
		/// <c>user/Mods</c>. Appended at the END of the enum on purpose: these members are matched
		/// by value in a switch and adding in the middle would renumber the rest.
		/// </summary>
		UserModSettings
	}

	public const string DocumentsFolderName = "Unclaimed World";

	public const string GameParamsFileName = "GameParams.xml";

	public const string commandFileName = "Commands.xml";

	public const string replayFileName = "Replay.UWRep";

	public const string randomGetsFileName = "RandomCalls.UWRepRand";

	public const string AIStatesFileName = "AIStates.UWRepStates";

	public const string MapDataName = "MapData.xml";

	private const string dataFolder = "data/BaseData";

	private const string scenarioFolder = "data/Scenarios";

	public const string mapsFolder = "data/Maps";

	private const string userModsFolder = "user/Mods";

	private const string userFolder = "user";

	private const string userScenarioFolder = "user/Scenarios";

	private const string userMapsFolder = "user/Maps";

	private const string replaysFolder = "Replays";

	private const string saveGamesFolder = "SaveGames";

	public static CultureInfo Culture;

	static Config()
	{
		Culture = CultureInfo.InvariantCulture;
	}

	public static string GetDataFolderPath(DataType dataType, string folderName = "", string fileName = "")
	{
		string path;
		switch (dataType)
		{
		case DataType.BaseData:
			path = "data/BaseData";
			break;
		case DataType.RGScenario:
			path = "data/Scenarios";
			break;
		case DataType.RGMap:
			path = "data/Maps";
			break;
		case DataType.UserMaps:
			path = "user/Maps";
			break;
		case DataType.UserScenarios:
			path = "user/Scenarios";
			break;
		case DataType.UserMods:
			path = "user/Mods";
			break;
		case DataType.UserModSettings:
			path = "user";
			break;
		case DataType.UserSettings:
			return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Unclaimed World", folderName, fileName);
		case DataType.Replays:
			return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Unclaimed World", "Replays", folderName, fileName);
		case DataType.SaveGames:
			return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Unclaimed World", "SaveGames", folderName, fileName);
		default:
			path = "data/BaseData";
			break;
		}
		return Path.Combine(Directory.GetCurrentDirectory(), path, folderName, fileName);
	}

	public static string GetDocumentsFolderPath(string folderName = "", string fileName = "")
	{
		return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Unclaimed World", folderName, fileName);
	}
}
