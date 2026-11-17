using System.Collections.Generic;
using System.IO;
using UWGame.SimSide.Scenarios;

namespace UWGame.SimSide.AllGameData.Scenarios;

public class AllScenarioLoader
{
	public static List<Scenario> LoadAllScenarioHeaders()
	{
		List<Scenario> list = RGScenarioLoader.LoadAllScenarioHeaders();
		List<Scenario> collection = LoadUserScenarioHeaders();
		list.AddRange(collection);
		return list;
	}

	private static List<Scenario> LoadUserScenarioHeaders()
	{
		List<Scenario> list = new List<Scenario>();
		string[] directories = Directory.GetDirectories(Config.GetDataFolderPath(Config.DataType.UserScenarios), "*", SearchOption.TopDirectoryOnly);
		for (int i = 0; i < directories.Length; i++)
		{
			DataLoader.DeserializeObject<Scenario>(directories[i], "scenario.xml", out var objectToSerialize, Config.DataType.UserMaps);
			list.Add(objectToSerialize);
		}
		return list;
	}

	public static Scenario LoadScenarioHeader(string name, Source source)
	{
		if (source == Source.RefactoredGames)
		{
			return RGScenarioLoader.LoadScenarioHeader(name);
		}
		// PORT FIX. The studio left the user-scenario half of this as "TODO!!! for replays!!" and
		// answered null. Loading a save is the other caller (StartScenarioParams.LoadScenarioFromName,
		// from the save's header), and SAVE in the game is a save followed by a load of the same
		// file - so every game of a user scenario crashed on SAVE: the load thread died here on a
		// null Scenario, swallowed the exception, and left a half-loaded world that the next frame
		// tripped over in GameWorldRenderer.ComputeDrawingArea or FogMap.DrawMapTexture (Kastuk,
		// 2026-11-15: "all of custom scenarios got crash during saving"). A user scenario is its
		// folder in user/Scenarios, named by its <Name> - where LoadUserScenarioHeaders found it
		// and LoadScenarioData reads its scenarioData.xml. Its Source is left as the file gives it,
		// which is what NEW GAME recorded in the save and what IsSameScenario compares against.
		DataLoader.DeserializeObject<Scenario>(name, ScenarioLoader.scenarioHeaderFileName, out var header, Config.DataType.UserScenarios);
		return header;
	}

	public static ScenarioData LoadScenarioData(Scenario scenario)
	{
		ScenarioData objectToSerialize;
		if (scenario.Source == Source.RefactoredGames)
		{
			objectToSerialize = RGScenarioLoader.LoadScenarioData(scenario);
		}
		else
		{
			DataLoader.DeserializeObject<ScenarioData>(scenario.Name, "scenarioData.xml", out objectToSerialize, Config.DataType.UserScenarios);
		}
		objectToSerialize.Initialize();
		Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
		List<string> list = new List<string>();
		dictionary.Add(scenario.Name, list);
		objectToSerialize.PostInitValidate(list);
		DataLoader.DisplayAllValidationErrors(dictionary);
		return objectToSerialize;
	}

	public static DataLoader GetScenarioDataLoader(Scenario scenario)
	{
		if (scenario.Source == Source.RefactoredGames)
		{
			return RGScenarioLoader.GetScenarioLoader(scenario).GetDataLoader();
		}
		return new UserDataLoader(scenario);
	}
}
