using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Pathfinding;

public class RegionPathFinderNodeAStar : ISnapshot
{
	public float G;

	public ushort Color;

	public float F;

	public Vector2 CenterLocation;

	public ushort Parent;

	public byte Status;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		G = sn.DoFloat(G);
		F = sn.DoFloat(F);
		Color = sn.DoUInt16(Color);
		Parent = sn.DoUInt16(Parent);
		Status = sn.DoByte(Status);
		CenterLocation = sn.DoVector2(CenterLocation);
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
