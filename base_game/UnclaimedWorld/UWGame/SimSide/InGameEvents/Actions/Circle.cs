using System;
using System.Collections.Generic;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.Maps;

namespace UWGame.SimSide.InGameEvents.Actions;

public class Circle : Area
{
	public EvalNode CenterLocation;

	public EvalNode Radius;

	public override List<TilePos> ComputeArea(EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
	{
		List<TilePos> list = new List<TilePos>();
		TilePos? tilePos = GetTilePos(CenterLocation, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
		tilePos = AddOffset(tilePos, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
		int? num = null;
		if (Radius != null)
		{
			num = (int)Math.Round(Radius.Evaluate(triggeringEntity, targetEntity, polledEventSource, dynamicTarget).Value.NumberResult.Value);
			MapManager.GetClampedMapAreaUsingTiles(tilePos.Value, num.Value, out var minX, out var maxX, out var minY, out var maxY);
			for (int i = minX; i < maxX; i++)
			{
				for (int j = minY; j < maxY; j++)
				{
					if (Common.DistanceOctile(tilePos.Value, new TilePos(i, j)) <= (float?)num)
					{
						list.Add(new TilePos(i, j));
					}
				}
			}
		}
		else if (The.Map.TileIsOnMap(tilePos.Value.ToPoint()))
		{
			list.Add(tilePos.Value);
		}
		return list;
	}
}
