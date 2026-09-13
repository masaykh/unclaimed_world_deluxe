using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Scenarios;

public class StartGameParams : ISnapshot
{
	public enum RGScenario
	{
		FieldsOfTauCeti,
		MuckrootMiningCamp,
		TwinklerIsland,
		MakingHeadway,
		TheClayPit,
		None
	}

	public StartScenarioParams StartScenarioParams;

	public StartDebugScenarioParams StartDebugScenarioParams;

	public StartGameEditorParams StartGameEditorParams;

	public string SavedGameToLoad;

	private RGScenario? scenario;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public override string ToString()
	{
		if (StartScenarioParams != null)
		{
			return StartScenarioParams.ToString();
		}
		if (StartDebugScenarioParams != null)
		{
			return StartDebugScenarioParams.ToString();
		}
		if (StartGameEditorParams != null)
		{
			return StartGameEditorParams.ToString();
		}
		return null;
	}

	public bool IsSameScenario(StartGameParams otherParms)
	{
		if (otherParms.StartScenarioParams != null)
		{
			if (StartScenarioParams != null)
			{
				if (StartScenarioParams.ScenarioName == otherParms.StartScenarioParams.ScenarioName && StartScenarioParams.Source == otherParms.StartScenarioParams.Source)
				{
					return true;
				}
				return false;
			}
			return false;
		}
		if (StartScenarioParams != null)
		{
			return false;
		}
		return true;
	}

	public RGScenario GetRGScenario()
	{
		if (!scenario.HasValue)
		{
			if (StartScenarioParams != null && StartScenarioParams.Scenario.Source == Source.RefactoredGames)
			{
				switch (StartScenarioParams.ScenarioName)
				{
				case "Fields of Tau Ceti - Cudgel Hills (L)":
				case "Fields of Tau Ceti - Cudgel Hills (M)":
				case "Fields of Tau Ceti - Alluvial Plain":
					scenario = RGScenario.FieldsOfTauCeti;
					break;
				case "Muckroot Mining Site":
					scenario = RGScenario.MuckrootMiningCamp;
					break;
				case "Twinkler Island":
					scenario = RGScenario.TwinklerIsland;
					break;
				case "Making Headway":
					scenario = RGScenario.MakingHeadway;
					break;
				case "The Clay Pit":
					scenario = RGScenario.TheClayPit;
					break;
				default:
					scenario = RGScenario.None;
					break;
				}
			}
			else
			{
				scenario = RGScenario.None;
			}
		}
		return scenario.Value;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		StartScenarioParams = (StartScenarioParams)sn.DoISnapshot(StartScenarioParams);
		StartDebugScenarioParams = (StartDebugScenarioParams)sn.DoISnapshot(StartDebugScenarioParams);
		sn.Ignore(SavedGameToLoad);
		sn.Ignore(StartGameEditorParams);
		sn.Ignore(scenario);
		return this;
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		if (StartScenarioParams != null)
		{
			StartScenarioParams.LoadPostProcess(sn);
		}
		if (StartDebugScenarioParams != null)
		{
			StartDebugScenarioParams.LoadPostProcess(sn);
		}
	}
}
