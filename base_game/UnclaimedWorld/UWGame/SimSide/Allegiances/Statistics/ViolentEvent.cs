using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Allegiances.Statistics;

public class ViolentEvent : ISnapshot
{
	public ViolentEventType EventType;

	public EntityID Victim;

	public string Name;

	public string Description;

	public DateAndTime.TimeDateYear Time;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		EventType = sn.DoEnum(EventType);
		Victim = sn.DoEnum(Victim);
		Name = sn.DoString(Name);
		Description = sn.DoString(Description);
		Time = sn.DoTimeDateYear(Time);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}
}
