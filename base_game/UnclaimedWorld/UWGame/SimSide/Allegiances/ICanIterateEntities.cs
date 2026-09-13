using System;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Allegiances;

public interface ICanIterateEntities : ILookUp<ICanIterateEntities, CanIterateEntitiesID>
{
	Allegiance GetAllegiance { get; }

	void IterateMembers(Action<Entity> iterateFunction);

	void IterateOwnedItems(Action<EntityGroup> iterateFunction);
}
