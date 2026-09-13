using System.Collections.Generic;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.AI.Goals;

public struct ToolParams
{
	public List<EntityID> Tools;

	public EntityID? ImmovableTool;

	public float? ToolProductivity;

	public Dictionary<EntityType, ReplenishStatus> ReplenishStatus;

	public float? JobDurationInDays;
}
