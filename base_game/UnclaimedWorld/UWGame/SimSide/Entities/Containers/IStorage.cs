using System.Collections.Generic;
using UWGame.SimSide.Entities.Containers.Components;

namespace UWGame.SimSide.Entities.Containers;

public interface IStorage
{
	float TotalItemStorageCapacity { get; }

	float TotalStored { get; }

	Storage GetStoredIn(Entity entity);

	Dictionary<StorageCondition, Storage> GetStorageSpaces();

	StorageCompartment GetCompartment(StorageID storageID);

	Storage FindStorage(StorageID storageID);
}
