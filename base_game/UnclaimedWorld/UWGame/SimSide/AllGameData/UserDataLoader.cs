using UWGame.SimSide.Scenarios;

namespace UWGame.SimSide.AllGameData;

/// <summary>
/// For user scenarios: every table comes from the scenario's own folder under user/Scenarios,
/// laid out the way tools/DataExport --scenarios writes one. See DataLoader.OnlyDeserializes.
/// </summary>
public class UserDataLoader : DataLoader
{
	public UserDataLoader(Scenario scenario)
		: base(Config.DataType.UserScenarios, 0.1f)
	{
		// The folder is the scenario's Name - the same key AllScenarioLoader.LoadScenarioData
		// reads scenarioData.xml by - so <Name> in scenario.xml must match the folder name.
		// Left empty, it read user/Scenarios/ itself.
		base.FolderName = scenario.Name;
	}

	protected override bool OnlyDeserializes => true;
}
