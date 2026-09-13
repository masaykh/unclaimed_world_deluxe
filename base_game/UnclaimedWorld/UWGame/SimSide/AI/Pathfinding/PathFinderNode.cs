using System.Diagnostics;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Pathfinding;

[DebuggerDisplay("{AbsoluteX},{AbsoluteY}, Parent: ({ParentAbsoluteX},{ParentAbsoluteY}), Status: {Status}, F: {F}, G: {G}")]
public class PathFinderNode : ISnapshot
{
	public int F;

	public int G;

	public ushort AbsoluteX;

	public ushort AbsoluteY;

	public byte SectorX;

	public byte SectorY;

	public ushort ParentAbsoluteX;

	public ushort ParentAbsoluteY;

	public byte Status;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		F = sn.DoInt32(F);
		G = sn.DoInt32(G);
		SectorX = sn.DoByte(SectorX);
		SectorY = sn.DoByte(SectorY);
		AbsoluteX = sn.DoUInt16(AbsoluteX);
		AbsoluteY = sn.DoUInt16(AbsoluteY);
		ParentAbsoluteX = sn.DoUInt16(ParentAbsoluteX);
		ParentAbsoluteY = sn.DoUInt16(ParentAbsoluteY);
		Status = sn.DoByte(Status);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}
}
