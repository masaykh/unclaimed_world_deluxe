using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using UWGame.SimSide.Maps;

namespace UWGame.SimSide.InGameEvents.Actions;

public class ExploreAction : EventActionType
{
	public bool ExploreWholeMap;

	public DetectMode DetectMode;

	public float RadiusStart;

	public float? RadiusEnd;

	public Vector2 OffsetLocationStart;

	public Vector2? OffsetLocationEnd;

	public EvalNode DynamicLocationStart;

	public EvalNode DynamicLocationEnd;

	public TargetObject EntityToExploreWith;

	public ExploreAction(string keyName)
		: base(keyName)
	{
	}

	public ExploreAction()
	{
	}

	public override bool Execute(EventAction action, ref string failReason)
	{
		Entity byEntity = null;
		if (EntityToExploreWith != null)
		{
			List<IHasExposedProperties> result = EntityToExploreWith.GetResult(action);
			if (result.Count == 0)
			{
				return false;
			}
			byEntity = (Entity)result[0];
		}
		if (ExploreWholeMap)
		{
			The.Sim.ExploreShroud(byEntity, DetectMode);
		}
		else
		{
			Vector2 exploreLocationStart = OffsetLocationStart;
			if (!AddDynamicOffset(action, DynamicLocationStart, ref failReason, ref exploreLocationStart))
			{
				return false;
			}
			if (DynamicLocationEnd != null || OffsetLocationEnd.HasValue)
			{
				Vector2 exploreLocationStart2 = OffsetLocationEnd ?? Vector2.Zero;
				if (!AddDynamicOffset(action, DynamicLocationEnd, ref failReason, ref exploreLocationStart2))
				{
					return false;
				}
				The.Sim.ExploreShroud(new WorldLocation(exploreLocationStart.ToVector3()), new WorldLocation(exploreLocationStart2.ToVector3()), RadiusStart, RadiusEnd ?? RadiusStart, byEntity, DetectMode);
			}
			else
			{
				The.Sim.ExploreCircularSpot(byEntity, new WorldLocation(exploreLocationStart.ToVector3()), DetectMode, RadiusStart);
			}
		}
		return true;
	}

	public override void PreInitValidate(ref List<string> errors)
	{
		base.PreInitValidate(ref errors);
		if (DetectMode != DetectMode.NoEntityDetection)
		{
			EntityType.ValidateRequiredValue(ref errors, "EntityToExploreWith when DetectMode is not None", EntityToExploreWith != null);
		}
	}

	public override string ToString()
	{
		return "ExploreAction " + OffsetLocationStart.ToString() + ", whole map: " + ExploreWholeMap;
	}

	private static bool AddDynamicOffset(EventAction action, EvalNode dynamicLocation, ref string failReason, ref Vector2 exploreLocationStart)
	{
		if (dynamicLocation != null)
		{
			PropertyResult? propertyResult = dynamicLocation.Evaluate(action);
			if (!propertyResult.HasValue || !propertyResult.Value.LocationResult.HasValue)
			{
				failReason = "Location did not evaluate to a result";
				return false;
			}
			Vector2? vector = propertyResult.Value.LocationResult.Value;
			if (vector.HasValue)
			{
				exploreLocationStart += vector.Value;
			}
		}
		return true;
	}
}
