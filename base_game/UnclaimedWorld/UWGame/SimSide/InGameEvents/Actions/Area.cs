using System.Collections.Generic;
using System.Xml.Serialization;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.Maps;

namespace UWGame.SimSide.InGameEvents.Actions;

[XmlInclude(typeof(RectangularArea))]
[XmlInclude(typeof(Circle))]
public abstract class Area
{
	public EvalNode Offset;

	public abstract List<TilePos> ComputeArea(EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget);

	public List<TilePos> ComputeArea(EventAction action)
	{
		return ComputeArea(action.TriggeringEntity, action.TargetEntity, action.PolledEventSource, action.DynamicTarget);
	}

	protected TilePos? GetTilePos(EvalNode node, EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
	{
		PropertyResult? propertyResult = node.Evaluate(triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
		TilePos? result = null;
		if (propertyResult.HasValue && propertyResult.Value.LocationResult.HasValue)
		{
			result = MapManager.WorldPosToTilePos(propertyResult.Value.LocationResult.Value.ToVector3());
		}
		return result;
	}

	protected TilePos? AddOffset(TilePos? upperLeftTilePos, EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
	{
		if (Offset != null)
		{
			TilePos? tilePos = null;
			tilePos = GetTilePos(Offset, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
			return new TilePos(upperLeftTilePos.Value.X + tilePos.Value.X, upperLeftTilePos.Value.Y + tilePos.Value.Y);
		}
		return upperLeftTilePos;
	}
}
