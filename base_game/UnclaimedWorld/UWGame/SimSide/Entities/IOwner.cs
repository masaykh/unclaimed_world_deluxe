using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities;

public interface IOwner : ILookUp<IOwner, OwnerID>
{
	EntityGroup OwnedEntities { get; }

	Allegiance Allegiance { get; }
}
