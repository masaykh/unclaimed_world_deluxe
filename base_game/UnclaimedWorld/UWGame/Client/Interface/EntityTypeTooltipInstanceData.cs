using UWGame.SimSide.Snapshots;

namespace UWGame.Client.Interface;

public class EntityTypeTooltipInstanceData : ISnapshot
{
	public string Description;

	public string RaceTypeDescription;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public EntityTypeTooltipInstanceData()
	{
	}

	public EntityTypeTooltipInstanceData(EntityTypeTooltipInstanceData instanceToCopyFrom)
	{
		Description = instanceToCopyFrom.Description;
		RaceTypeDescription = instanceToCopyFrom.RaceTypeDescription;
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		Description = sn.DoString(Description);
		RaceTypeDescription = sn.DoString(RaceTypeDescription);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}
}
