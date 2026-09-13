using System.Collections.Generic;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Jobs;

public class ProductionPlan
{
	public Dictionary<EntityType, int> ProductionOrderChanges;
}
