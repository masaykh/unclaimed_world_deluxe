using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Buildings;

public class DirectionalLayout : ISnapshot
{
	public Common.Direction EdgePosition;

	public int SubtileIndex;

	public Point EdgeSubtile;

	public Point CenterSubtile;

	private Entity parent;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		CenterSubtile = sn.DoPoint(CenterSubtile);
		EdgePosition = sn.DoEnum(EdgePosition);
		EdgeSubtile = sn.DoPoint(EdgeSubtile);
		SubtileIndex = sn.DoInt32(SubtileIndex);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}

	public DirectionalLayout()
	{
	}

	public DirectionalLayout(Entity parent)
	{
		this.parent = parent;
	}

	public void Place(Common.Direction edgePos)
	{
		EdgePosition = edgePos;
		Point edgeSubtile = MapManager.DirectionToRelativeSubtile(edgePos);
		SubtileIndex = MapManager.GetLocalSubtileIndex(edgeSubtile.X, edgeSubtile.Y);
		Point point = MapManager.TileEdgeToSubtile(parent.MapPosition.Value);
		EdgeSubtile = edgeSubtile;
		EdgeSubtile.X += point.X;
		EdgeSubtile.Y += point.Y;
		CenterSubtile = new Point(point.X + 1, point.Y + 1);
		if (parent.EntityType.StructureType != null && parent.EntityType.StructureType.IsRoad)
		{
			The.Map.TileMap[parent.MapPosition.Value.X][parent.MapPosition.Value.Y].AddRoad(parent, EdgePosition);
		}
		else
		{
			The.Map.TileMap[parent.MapPosition.Value.X][parent.MapPosition.Value.Y].AddEdgeStructure(parent);
		}
		RedrawTerrainCosts();
	}

	public byte[][] CreateBlockedSubtileMap()
	{
		byte[][] map = null;
		Common.InitJaggedArray(ref map, 3, 3);
		map[1][1] = byte.MaxValue;
		Point point = MapManager.DirectionToRelativeSubtile(EdgePosition);
		map[point.X][point.Y] = byte.MaxValue;
		return map;
	}

	public bool[][] CreateIsBlockedMap()
	{
		bool[][] map = null;
		Common.InitJaggedArray(ref map, 3, 3);
		map[1][1] = true;
		Point point = MapManager.DirectionToRelativeSubtile(EdgePosition);
		map[point.X][point.Y] = true;
		return map;
	}

	public void Destroy()
	{
		The.Map.GetTile(parent.MapPosition.Value).RemoveEdgeStructure(parent);
		ClearTerrainCosts(redraw: true);
	}

	public void ClearTerrainCosts(bool redraw)
	{
		The.Map.SetSubtileCostToSurfaceType(EdgeSubtile);
		The.Map.SetSubtileCostToSurfaceType(CenterSubtile);
	}

	public void RedrawTerrainCosts()
	{
		Structure structure = parent.Structure;
		if (structure != null)
		{
			if (parent.TerrainPath != null || !structure.ConstructionHasStarted() || !parent.EntityType.DirectionalLayoutType.IsObstacle)
			{
				return;
			}
			{
				foreach (SurfaceType.TransportType item in MapManager.MapTransportTypeArray)
				{
					SetCost(item, 0);
				}
				return;
			}
		}
		if (!parent.EntityType.DirectionalLayoutType.IsObstacle)
		{
			return;
		}
		foreach (SurfaceType.TransportType item2 in MapManager.MapTransportTypeArray)
		{
			SetCost(item2, 0);
		}
	}

	private void SetCost(SurfaceType.TransportType transport, byte cost)
	{
		The.Map.SetSubtileCost(EdgeSubtile, transport, cost);
		The.Map.SetSubtileCost(CenterSubtile, transport, cost);
	}

	public void RedrawRoadCost()
	{
		PathType pathType = parent.EntityType.TerrainType.PathType;
		foreach (SurfaceType.TransportType item in MapManager.MapTransportTypeArray)
		{
			byte cost = pathType.TransportCosts[(uint)item];
			SetCost(item, cost);
		}
	}
}
