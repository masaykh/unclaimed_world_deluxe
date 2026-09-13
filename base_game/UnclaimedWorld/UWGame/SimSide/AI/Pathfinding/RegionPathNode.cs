using System.Diagnostics;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Pathfinding;

[DebuggerDisplay("{RegionCenterInSubtiles}")]
public class RegionPathNode : ISnapshot
{
	public Point RegionCenterInSubtiles;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public RegionPathNode()
	{
	}

	public RegionPathNode(RegionPathFinderNodeAStar pathNode)
	{
		RegionCenterInSubtiles = MapManager.WorldPosToSubtile(pathNode.CenterLocation);
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		RegionCenterInSubtiles = sn.DoPoint(RegionCenterInSubtiles);
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
