using Microsoft.Xna.Framework;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Trees;

namespace UWGame.SimSide.Entities;

public class PointLayout : ISnapshot
{
	private Entity parent;

	private EntityID snapshotParent;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public PointLayout(Entity parent)
	{
		this.parent = parent;
	}

	public PointLayout()
	{
	}

	public void RedrawTerrainCosts()
	{
		if (parent.Find<Tree>(out var c))
		{
			c.RedrawTerrainCosts();
			return;
		}
		Structure structure = parent.Structure;
		if (structure != null)
		{
			if (structure.ConstructionHasStarted())
			{
				The.Map.SetSubtileTerrainCost(parent.Location.Value, 0);
			}
		}
		else if (parent.EntityType.PointLayoutType.IsBlocking)
		{
			The.Map.SetSubtileTerrainCost(parent.Location.Value, 0);
			if (parent.EntityType.RockType != null)
			{
				The.Map.SetSubtileTerrainCost(parent.Location.Value, 2);
			}
		}
	}

	public bool[][] CreateIsBlockedMap()
	{
		bool[][] map = null;
		Common.InitJaggedArray(ref map, 3, 3);
		Point point = MapManager.WorldPosToRelativeSubtile(parent.PlaySiteLocation);
		map[point.X][point.Y] = true;
		return map;
	}

	public byte[][] CreateBlockedSubtileMap()
	{
		byte[][] map = null;
		Common.InitJaggedArray(ref map, 3, 3);
		Point point = MapManager.WorldPosToRelativeSubtile(parent.PlaySiteLocation);
		map[point.X][point.Y] = byte.MaxValue;
		return map;
	}

	public void Place()
	{
		The.Map.GetTile(parent.MapPosition.Value).AddEntity(parent);
		RedrawTerrainCosts();
	}

	public void ClearTerrainCosts(bool redraw)
	{
		The.Map.SetSubtileTerrainCostToSurfaceType(parent.PlaySiteLocation);
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		snapshotParent = sn.SnapshotID<Entity, EntityID>(parent).Value;
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		parent = Entity.FindByID(snapshotParent);
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}
}
