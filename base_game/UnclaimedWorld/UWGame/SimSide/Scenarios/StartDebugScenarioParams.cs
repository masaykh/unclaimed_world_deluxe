using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Scenarios;

public class StartDebugScenarioParams : ISnapshot
{
	public PlaceGameEntities.DebugScenarios ScenarioKey;

	public string MapKey;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public override string ToString()
	{
		return ScenarioKey.ToString();
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		ScenarioKey = sn.DoEnum(ScenarioKey);
		MapKey = sn.DoString(MapKey);
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
	}
}
