using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Collisions;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.InGameEvents.Conditions;

public class AreaCondition : ConditionValue
{
	public Rectangle Area;

	[XmlIgnore]
	private CollideShape2D area;

	[XmlIgnore]
	private List<Pair<Entity, Vector2>> membersInArea;

	public override void Initialize()
	{
		area = new CollideShape2D(Area.Top, Area.Left, Area.Bottom, Area.Right);
		membersInArea = new List<Pair<Entity, Vector2>>();
	}

	public override bool IsFulfilled(ref Entity triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
	{
		if (base.IsFulfilled(ref triggeringEntity, targetEntity, polledEventSource, dynamicTarget))
		{
			return IsFulfilled(ref triggeringEntity);
		}
		return false;
	}

	public bool IsFulfilled(ref Entity triggeringEntity)
	{
		The.AgentQuadTree.GetObjectsIntersectingBounds(area, (Entity e) => The.Sim.PlaySite.PlayerAllegiance.Members.Contains(e), ref membersInArea);
		if (membersInArea.Count > 0)
		{
			triggeringEntity = membersInArea[0].First;
			membersInArea.Clear();
			return true;
		}
		return false;
	}
}
