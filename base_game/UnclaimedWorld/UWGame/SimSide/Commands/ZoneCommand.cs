using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Commands;

public class ZoneCommand
{
	public long? ZoneID;

	public List<TilePos> CoveredArea;

	public Point StartDragTilePosition;

	public ZoneCommand()
	{
	}

	public ZoneCommand(ZoneID zoneID)
	{
		ZoneID = (long)zoneID;
	}

	public ZoneCommand(List<TilePos> coveredArea, Point startDragTilePosition)
	{
		CoveredArea = coveredArea;
		StartDragTilePosition = startDragTilePosition;
	}

	public ZoneCommand(MapArea mapArea)
		: this(mapArea.GetTileLocations(), mapArea.StartDragTile.Value)
	{
	}

	public Zone RetrieveOrCreateZone(long entityGroupID, out EntityGroup entityGroupToUse)
	{
		entityGroupToUse = LookUp<EntityGroup, EntityGroupID>.FindByID((EntityGroupID)entityGroupID);
		Zone zone = null;
		if (ZoneID.HasValue)
		{
			zone = LookUp<Zone, UWGame.SimSide.Maps.ZoneID>.FindByID((ZoneID)ZoneID.Value);
			if (zone == null)
			{
				throw new Exception("Zone not found");
			}
		}
		else
		{
			MapArea mapArea = new MapArea();
			foreach (TilePos item in CoveredArea)
			{
				mapArea.Add(The.Map.GetTile(item.X, item.Y));
			}
			mapArea.StartDragTile = StartDragTilePosition;
			zone = new Zone(entityGroupToUse, mapArea);
		}
		return zone;
	}
}
