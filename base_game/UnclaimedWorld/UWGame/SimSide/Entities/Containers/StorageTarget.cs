using System.Diagnostics;

namespace UWGame.SimSide.Entities.Containers;

[DebuggerDisplay("StorageEntity: {StorageEntity}, {StorageID}")]
public struct StorageTarget
{
	private readonly EntityID storageEntity;

	private readonly StorageID storageID;

	public EntityID StorageEntity => storageEntity;

	/// <summary>
	/// Used together with the container entity to resolve the target compartment and storage conditions.
	/// </summary>
	public StorageID StorageID => storageID;

	public StorageTarget(EntityID storageEntity, StorageID storageID)
	{
		this.storageID = storageID;
		this.storageEntity = storageEntity;
	}

	public override bool Equals(object obj)
	{
		if (obj is StorageTarget)
		{
			return this == (StorageTarget)obj;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return storageID.GetHashCode() ^ storageEntity.GetHashCode();
	}

	public static bool operator ==(StorageTarget x, StorageTarget y)
	{
		if (x.StorageEntity == y.StorageEntity)
		{
			return x.StorageID == y.StorageID;
		}
		return false;
	}

	public static bool operator !=(StorageTarget x, StorageTarget y)
	{
		return !(x == y);
	}
}
