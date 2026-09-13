using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.AI.Activities;

public abstract class Formation
{
	public Dictionary<Entity, Vector2> FormationPositions = new Dictionary<Entity, Vector2>();

	public Dictionary<Entity, Vector2> FormationPositionDrifts = new Dictionary<Entity, Vector2>();

	public abstract int MaxMembers { get; }

	public abstract Vector2 ComputePosition();

	public virtual void LeaveFormation(Entity entity)
	{
		if (FormationPositions.ContainsKey(entity))
		{
			FormationPositions.Remove(entity);
		}
		if (FormationPositionDrifts.ContainsKey(entity))
		{
			FormationPositionDrifts.Remove(entity);
		}
	}
}
