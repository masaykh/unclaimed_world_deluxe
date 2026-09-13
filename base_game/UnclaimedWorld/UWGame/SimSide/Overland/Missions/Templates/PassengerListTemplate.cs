using System.Collections.Generic;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Overland.Missions.Templates;

public class PassengerListTemplate : ISnapshot
{
	public TravelLocation StartingLocation;

	public List<long> Passengers;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		Passengers = sn.DoList(Passengers);
		StartingLocation = sn.DoTravelLocation(StartingLocation);
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
