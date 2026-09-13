using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace UWGame.SimSide.AI.Activities;

public class LeisureWalkActivity : GroupMoveActivity
{
	public LeisureWalkActivity(List<Activity> addToList, Vector3 destination)
		: base(addToList, destination)
	{
	}
}
