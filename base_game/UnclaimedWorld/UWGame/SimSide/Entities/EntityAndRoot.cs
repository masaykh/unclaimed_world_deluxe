using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;

namespace UWGame.SimSide.Entities;

public struct EntityAndRoot
{
	public EntityID Entity;

	public EntityID Root;

	public EntityAndRoot(EntityID entity, EntityID root)
	{
		Entity = entity;
		Root = root;
	}

	public static EntityID? GetEntity(EntityAndRoot? entityAndRoot)
	{
		if (entityAndRoot.HasValue)
		{
			return entityAndRoot.Value.Entity;
		}
		return null;
	}

	public bool IsValid(SharedKnowledge sharedKnowledge, out IKnownEntityData actingOnEntityData)
	{
		actingOnEntityData = null;
		if (GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(Entity, out actingOnEntityData)))
		{
			return false;
		}
		if (actingOnEntityData.RootEntityID != Root)
		{
			return false;
		}
		if (Entity != Root && GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(Root, out var _)))
		{
			return false;
		}
		return true;
	}
}
