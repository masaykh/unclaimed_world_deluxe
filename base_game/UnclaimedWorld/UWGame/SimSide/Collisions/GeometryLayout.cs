using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Collisions;

public class GeometryLayout : ISnapshot
{
	public Entity parent;

	private EntityID snapshotParent;

	private List<TerrainTile> touchedTiles = new List<TerrainTile>();

	private List<TerrainTileID> snapshotTouchedTiles;

	private TerrainTileID? baseCenterTile;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public GeometryLayout(Entity parent)
	{
		this.parent = parent;
	}

	public GeometryLayout()
	{
	}

	public void Place(GeoPlaceMode mode)
	{
		switch (mode)
		{
		case GeoPlaceMode.NewFeature:
			CalculateTouchedTiles();
			AddTileReferencesToEntity();
			RedrawTerrainCosts();
			break;
		case GeoPlaceMode.ShapeChanged:
		{
			RemoveTileReferencesToEntity();
			EraseFootprintAndSignalNeighborsToStampAnew(out var _);
			RedrawTerrainCosts();
			AddTileReferencesToEntity();
			break;
		}
		case GeoPlaceMode.FeatureRemoved:
		{
			EraseFootprintAndSignalNeighborsToStampAnew(out var _);
			RemoveTileReferencesToEntity();
			break;
		}
		}
	}

	private void SetAccessPointsDirty(List<Entity> neighbours)
	{
		foreach (Entity neighbour in neighbours)
		{
			neighbour.SetAccessPointDirty();
		}
	}

	private void AddTileReferencesToEntity()
	{
		if (baseCenterTile.HasValue)
		{
			LookUpSortedDictionary<TerrainTile, TerrainTileID>.FindByID(baseCenterTile.Value).RemoveCenterForGeoLayoutEntities(parent);
		}
		TerrainTile tile = The.Map.GetTile(parent.MapPosition.Value);
		tile.AddCenterForGeoLayoutEntities(parent);
		baseCenterTile = tile.ID;
		foreach (TerrainTile touchedTile in touchedTiles)
		{
			touchedTile.AddGeoLayoutEntity(parent);
		}
	}

	private void RemoveTileReferencesToEntity()
	{
		if (baseCenterTile.HasValue)
		{
			LookUpSortedDictionary<TerrainTile, TerrainTileID>.FindByID(baseCenterTile.Value).RemoveCenterForGeoLayoutEntities(parent);
		}
		foreach (TerrainTile touchedTile in touchedTiles)
		{
			touchedTile.RemoveGeoLayoutEntity(parent);
		}
	}

	public void CreateBlockedMapOverBoundsAndPadding(bool flipHorizontally, out Tuple<bool, Vector2>[][] isBlockedData, out byte[][] subTileMap)
	{
		SubtileLayers terrainCosts = The.Map.TerrainCosts[SurfaceType.TransportType.Foot];
		float sts = 16f;
		GetBoundsWithPadding(out var from, out var to);
		subTileMap = null;
		int width = (int)((to.X - from.X) / sts) + 1;
		int height = (int)((to.Y - from.Y) / sts) + 1;
		Common.InitJaggedArray(ref subTileMap, width, height);
		Tuple<bool, Vector2>[][] blockedData = null;
		Common.InitJaggedArray(ref blockedData, width, height);
		width = 0;
		height = 0;
		IterateSubtiles(delegate(Vector2 s)
		{
			float num = Math.Abs((from.X - s.X) / sts);
			float num2 = Math.Abs((from.Y - s.Y) / sts);
			Point point = MapManager.WorldPosToSubtile(s);
			MapManager.SubtileValue value = terrainCosts.GetValue(point.X, point.Y);
			blockedData[(int)num][(int)num2] = new Tuple<bool, Vector2>(!Structure.IsNotBlockedOrReserved(value), s);
		});
		isBlockedData = blockedData;
	}

	public byte[][] CreateBlockedSubtileMap(bool flipHorizontally)
	{
		return null;
	}

	private void EraseFootprintAndSignalNeighborsToStampAnew(out List<Entity> neighbours)
	{
		float num = 16f;
		Vector2 vector = new Vector2(Math.Max(num, parent.CurrentSimState.GeometryLayoutType.Pad));
		Vector2 vector2 = parent.Collidable.Bounds.BoundsUpperLeft - vector;
		Vector2 vector3 = parent.Collidable.Bounds.BoundsLowerRight + vector;
		if (vector2.X < 0f)
		{
			vector2.X = 0f;
		}
		if (vector2.Y < 0f)
		{
			vector2.Y = 0f;
		}
		vector2.X -= vector2.X % num;
		vector2.Y -= vector2.Y % num;
		if (vector3.X >= The.Map.MapWorldWidth)
		{
			vector3.X = The.Map.MapWorldWidth - 1f;
		}
		if (vector3.Y >= The.Map.MapWorldHeight)
		{
			vector3.Y = The.Map.MapWorldHeight - 1f;
		}
		for (float num2 = vector2.X; num2 < vector3.X; num2 += num)
		{
			for (float num3 = vector2.Y; num3 < vector3.Y; num3 += num)
			{
				Point point = MapManager.WorldPosToSubtile(new Vector3(num2, num3, 0f));
				The.Map.SetSubtileCostToSurfaceType(point);
				The.Map.ClearSubtileTerrainValueFlag(point, MapManager.SubtileValue.Pad | MapManager.SubtileValue.Reserved);
			}
		}
		neighbours = new List<Entity>();
		foreach (TerrainTile touchedTile in touchedTiles)
		{
			if (touchedTile.GeoLayoutEntitiesOnTile == null)
			{
				continue;
			}
			for (int num4 = touchedTile.GeoLayoutEntitiesOnTile.Count - 1; num4 >= 0; num4--)
			{
				Entity entity = Entity.FindByID(touchedTile.GeoLayoutEntitiesOnTile[num4]);
				if (entity != null)
				{
					if (entity != parent && entity.GeometryLayout != null)
					{
						neighbours.Add(entity);
					}
				}
				else
				{
					touchedTile.GeoLayoutEntitiesOnTile.RemoveAt(num4);
				}
			}
		}
		neighbours = neighbours.Distinct().ToList();
		foreach (Entity neighbour in neighbours)
		{
			neighbour.GeometryLayout.Place(GeoPlaceMode.NewFeature);
			neighbour.SetAccessPointDirty();
		}
	}

	public void Destroy()
	{
		Structure structure = parent.Structure;
		if ((structure == null || structure.State != StructureStates.BeingPlaced) && parent.Collidable != null)
		{
			ClearTerrainCosts(redraw: false);
			RemoveTileReferencesToEntity();
		}
	}

	public void ClearTerrainCosts(bool redraw)
	{
		EraseFootprintAndSignalNeighborsToStampAnew(out var _);
		if (redraw)
		{
			RedrawTerrainCosts();
		}
	}

	private void CalculateTouchedTiles()
	{
		try
		{
			float num = 48f;
			Vector2 vector = new Vector2(Math.Max(num, parent.CurrentSimState.GeometryLayoutType.Pad));
			Vector2 vector2 = parent.Collidable.Bounds.BoundsUpperLeft - vector;
			Vector2 vector3 = parent.Collidable.Bounds.BoundsLowerRight + vector;
			if (vector2.X < 0f)
			{
				vector2.X = 0f;
			}
			if (vector2.Y < 0f)
			{
				vector2.Y = 0f;
			}
			vector2.X -= vector2.X % num;
			vector2.Y -= vector2.Y % num;
			vector2.X += 1f;
			vector2.Y += 1f;
			if (vector3.X >= The.Map.MapWorldWidth)
			{
				vector3.X = The.Map.MapWorldWidth - 1f;
			}
			if (vector3.Y >= The.Map.MapWorldHeight)
			{
				vector3.Y = The.Map.MapWorldHeight - 1f;
			}
			touchedTiles.Clear();
			for (float num2 = vector2.X; num2 < vector3.X; num2 += num)
			{
				for (float num3 = vector2.Y; num3 < vector3.Y; num3 += num)
				{
					Point pos = MapManager.WorldPosToTile(new Vector2(num2, num3));
					TerrainTile tile = The.Map.GetTile(pos);
					touchedTiles.Add(tile);
				}
			}
		}
		catch (Exception ex)
		{
			string message = ex.Message;
			message = ((parent != null) ? (message + Entity.GetExceptionInformation(parent)) : (message + "PARENT NULL\n"));
			if (parent.Collidable == null)
			{
				message += "Collidable NULL \n";
			}
			else if (parent.Collidable.Bounds == null)
			{
				message += "Bounds NULL\n";
			}
			if (touchedTiles == null)
			{
				message += "touchedTiles NULL\n";
			}
			throw new Exception(message);
		}
	}

	public void GetBoundsWithPadding(out Vector2 from, out Vector2 to)
	{
		GeometryLayoutType geometryLayoutType = parent.CurrentSimState.GeometryLayoutType;
		parent.Collidable.GetBoundsWithPadding(geometryLayoutType.Pad, out from, out to);
	}

	public void IterateSubtiles(Action<Vector2> iterateMethod)
	{
		GetBoundsWithPadding(out var from, out var to);
		MapManager.IterateSubtiles(from, to, iterateMethod);
	}

	private void RedrawTerrainCosts()
	{
		GeometryLayoutType geoType = parent.CurrentSimState.GeometryLayoutType;
		float padRadiusSquared = GetPadRadiusSquared();
		bool isReservedForStructure = false;
		if (parent.EntityType.StructureType != null && !parent.Structure.ConstructionHasStarted())
		{
			isReservedForStructure = true;
		}
		IterateSubtiles(delegate(Vector2 worldPos)
		{
			bool isInPad2 = IsWithinPad(geoType, padRadiusSquared, worldPos);
			bool isWithinShapes2 = IsWithinShapes(worldPos, testIfAnyPartOfSubtileIsInBounds: false);
			StampSubtile(worldPos, 0, isWithinShapes2, isInPad2, isReservedForStructure);
		});
		if (!isReservedForStructure && parent.Contains != null && parent.Contains is IExit exit)
		{
			for (ExitDoor exitDoor = ExitDoor.Door1; exitDoor < ExitDoor.Max; exitDoor++)
			{
				Vector3 position = Vector3.Zero;
				if (exit.GetDoorPosition(ref position, exiting: false, out var _, exitDoor))
				{
					Vector3 vector = exit.GetRallyPoint(exitDoor) - position;
					vector.Length();
					float num = 0.1f;
					for (float num2 = 0f; num2 < 1f; num2 += num)
					{
						Vector3 location = position + vector * num2;
						bool isWithinShapes = IsWithinShapes(location.ToVector2(), testIfAnyPartOfSubtileIsInBounds: true);
						bool isInPad = true;
						StampSubtile(location.ToVector2(), 7, isWithinShapes, isInPad, isUnbuiltStructure: false);
					}
				}
			}
		}
		CalculateTouchedTiles();
	}

	private bool AllowEntityOnShapes(Entity entity)
	{
		if (entity != parent && entity.EntityType.TerrainType == null)
		{
			return entity.EntityType.TreeType != null;
		}
		return true;
	}

	public bool IsWithinPad(GeometryLayoutType geoType, float padRadiusSquared, Vector2 worldPos)
	{
		bool result = geoType.Pad > 0f;
		if (geoType.PadShape == CollidePrim.Circle)
		{
			result = (parent.PlaySiteLocation.ToVector2() - worldPos).LengthSquared() < padRadiusSquared;
		}
		return result;
	}

	public float GetPadRadiusSquared()
	{
		GeometryLayoutType geometryLayoutType = parent.CurrentSimState.GeometryLayoutType;
		float num = parent.Collidable.Bounds.Radius + geometryLayoutType.Pad;
		return num * num;
	}

	private void StampSubtile(Vector2 worldPos, byte cost, bool isWithinShapes, bool isInPad, bool isUnbuiltStructure)
	{
		if (isWithinShapes)
		{
			Point subtilePos = MapManager.WorldPosToSubtile(worldPos);
			if (!isUnbuiltStructure)
			{
				The.Map.SetSubtileTerrainCost(subtilePos, cost);
			}
			else
			{
				The.Map.SetSubtileTerrainValueFlag(subtilePos, MapManager.SubtileValue.Reserved);
			}
		}
		else if (isInPad)
		{
			Point subtilePos2 = MapManager.WorldPosToSubtile(worldPos);
			if (!isUnbuiltStructure)
			{
				The.Map.SetSubtileTerrainValueFlag(subtilePos2, MapManager.SubtileValue.Pad);
			}
			else
			{
				The.Map.SetSubtileTerrainValueFlag(subtilePos2, MapManager.SubtileValue.Reserved);
			}
		}
	}

	public bool IsWithinShapes(Vector2 worldPos, bool testIfAnyPartOfSubtileIsInBounds)
	{
		return parent.Collidable.IsWithinShapes(worldPos, testIfAnyPartOfSubtileIsInBounds);
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		snapshotParent = sn.SnapshotID<Entity, EntityID>(parent).Value;
		if (sn.mode != Snapshotter.Mode.Load)
		{
			snapshotTouchedTiles = touchedTiles.Select((TerrainTile t) => t.ID).ToList();
		}
		snapshotTouchedTiles = sn.DoList(snapshotTouchedTiles);
		baseCenterTile = sn.DoEnumNullable(baseCenterTile);
		sn.Ignore(touchedTiles);
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
		parent = Entity.FindByID(snapshotParent);
		touchedTiles = snapshotTouchedTiles.Select((TerrainTileID t) => LookUpSortedDictionary<TerrainTile, TerrainTileID>.FindByID(t)).ToList();
	}
}
