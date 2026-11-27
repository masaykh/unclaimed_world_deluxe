using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Vehicles;

/// <summary>
/// This class is now mainly used to control animation. Not a client class though.
/// </summary>
public class Aircraft : ISnapshot
{
	public float RightDuctFanAngle;

	public float LeftDuctFanAngle;

	public float PropellerSpeed;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.VehicleSnapshotReferences);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		RightDuctFanAngle = sn.DoFloat(RightDuctFanAngle);
		LeftDuctFanAngle = sn.DoFloat(LeftDuctFanAngle);
		PropellerSpeed = sn.DoFloat(PropellerSpeed);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}
}
