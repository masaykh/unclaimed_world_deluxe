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
		return null;
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
		return new UserDataLoader();
	}
}
