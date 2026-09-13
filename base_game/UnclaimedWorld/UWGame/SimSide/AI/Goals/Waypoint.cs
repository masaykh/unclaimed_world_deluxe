using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

public class Waypoint : ISnapshot
{
	public Vector3 Location;

	public int Number;

	public bool IsLastWaypoint;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public Waypoint(int number, Vector3 location, bool isLastWaypoint = false)
	{
		Location = location;
		Number = number;
		IsLastWaypoint = isLastWaypoint;
	}

	public Waypoint()
	{
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		Location = sn.DoVector3(Location);
		Number = sn.DoInt32(Number);
		IsLastWaypoint = sn.DoBool(IsLastWaypoint);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}
}
