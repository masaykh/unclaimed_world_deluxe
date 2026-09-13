using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.Maps;

namespace UWGame.SimSide.InGameEvents.Actions;

public class RectangularArea : Area
{
	public EvalNode UpperLeftLocation;

	public EvalNode Width;

	public EvalNode Height;

	public override List<TilePos> ComputeArea(EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
	{
		List<TilePos> list = new List<TilePos>();
		TilePos? tilePos = GetTilePos(UpperLeftLocation, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
		tilePos = AddOffset(tilePos, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
		int? num = null;
		int? num2 = null;
		if (Width != null && Height != null)
		{
			num = (int)Math.Round(Width.Evaluate(triggeringEntity, targetEntity, polledEventSource, dynamicTarget).Value.NumberResult.Value);
			num2 = (int)Math.Round(Height.Evaluate(triggeringEntity, targetEntity, polledEventSource, dynamicTarget).Value.NumberResult.Value);
			Rectangle clampedMapAreaUsingTiles = The.Map.GetClampedMapAreaUsingTiles(tilePos.Value.ToPoint(), num.Value, num2.Value);
			for (int i = clampedMapAreaUsingTiles.X; i < clampedMapAreaUsingTiles.X + clampedMapAreaUsingTiles.Width; i++)
			{
				for (int j = clampedMapAreaUsingTiles.Y; j < clampedMapAreaUsingTiles.Y + clampedMapAreaUsingTiles.Height; j++)
				{
					list.Add(new TilePos(i, j));
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
