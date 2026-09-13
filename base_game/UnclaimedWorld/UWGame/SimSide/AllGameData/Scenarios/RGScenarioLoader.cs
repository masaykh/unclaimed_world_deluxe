using System.Collections.Generic;
using System.Linq;
using UWGame.SimSide.AllGameData.Scenarios.Scenario_1;
using UWGame.SimSide.AllGameData.Scenarios.Scenario_2;
using UWGame.SimSide.AllGameData.Scenarios.Scenario_3;
using UWGame.SimSide.AllGameData.Scenarios.Scenario_4;
using UWGame.SimSide.AllGameData.Scenarios.Scenario_4x;
using UWGame.SimSide.AllGameData.Scenarios.Scenario_4y;
using UWGame.SimSide.AllGameData.Scenarios.Scenario_5;
using UWGame.SimSide.AllGameData.Scenarios.Scenario_6;
using UWGame.SimSide.AllGameData.Scenarios.Scenario_7;
using UWGame.SimSide.Scenarios;

namespace UWGame.SimSide.AllGameData.Scenarios;

public class RGScenarioLoader
{
	private static List<ScenarioLoader> rgScenarioLoaders;

	static RGScenarioLoader()
	{
		rgScenarioLoaders = new List<ScenarioLoader>();
		rgScenarioLoaders.Add(new Scenario1Loader());
		rgScenarioLoaders.Add(new Scenario2Loader());
		rgScenarioLoaders.Add(new Scenario3Loader());
		rgScenarioLoaders.Add(new Scenario4Loader());
		rgScenarioLoaders.Add(new Scenario5Loader());
		rgScenarioLoaders.Add(new Scenario4xLoader());
		rgScenarioLoaders.Add(new Scenario6Loader());
		rgScenarioLoaders.Add(new Scenario4yLoader());
		rgScenarioLoaders.Add(new Scenario7Loader());
	}

	public static void Serialize()
	{
		foreach (ScenarioLoader rgScenarioLoader in rgScenarioLoaders)
		{
			rgScenarioLoader.WriteScenario();
		}
	}

	public static List<Scenario> LoadAllScenarioHeaders()
	{
		List<Scenario> list = new List<Scenario>();
		foreach (ScenarioLoader rgScenarioLoader in rgScenarioLoaders)
		{
			Scenario scenarioHeader = rgScenarioLoader.GetScenarioHeader();
			if (!scenarioHeader.IsInDevelopment)
			{
				list.Add(scenarioHeader);
			}
		}
		return list;
	}

	public static Scenario LoadScenarioHeader(string name)
	{
		return rgScenarioLoaders.FirstOrDefault((ScenarioLoader l) => l.FolderName == name).GetScenarioHeader();
	}

	public static ScenarioLoader GetScenarioLoader(Scenario scenario)
	{
		return rgScenarioLoaders.FirstOrDefault((ScenarioLoader l) => l.FolderName == scenario.Name);
	}

	public static ScenarioData LoadScenarioData(Scenario scenario)
	{
		return GetScenarioLoader(scenario)?.GetScenarioData();
	}
}
