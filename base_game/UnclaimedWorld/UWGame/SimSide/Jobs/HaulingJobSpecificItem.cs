using System.Diagnostics;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Jobs;

[DebuggerDisplay("{Item} {ToLocation} {ToStorage}")]
public class HaulingJobSpecificItem : HaulingJob
{
	public bool IsHaulJobToStorage;

	public HaulingJobSpecificItem()
	{
	}

	public HaulingJobSpecificItem(Vector3? toLocation, StorageTarget? toStorage, EntityGroup entityGroup, IKnownEntityData specificItem, OwnerID? newOwner, bool haulToStorage, bool haulToTrade, EntityGroup itemsGroup)
		: base(toLocation, toStorage, haulToTrade, entityGroup, newOwner, addToJobsGroupNow: false)
	{
		specificItem.AssignedToJob = base.ID;
		base.Item = specificItem.EntityID;
		entityGroup.AddJob(this);
		IsHaulJobToStorage = haulToStorage;
		ItemsToHaulGroup = itemsGroup.ID;
		ComputeJobType();
		SetDefaultPriority(entityGroup);
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder("Haul ");
		if (base.Item.HasValue)
		{
			Entity entity = Entity.FindByID(base.Item.Value);
			if (entity != null)
			{
				stringBuilder.Append(entity.EntityType.ItemType.Name ?? entity.EntityType.ItemType.KeyName);
				stringBuilder.Append(" To: ");
				GetToLocationAsString(stringBuilder);
				return stringBuilder.ToString();
			}
		}
		return base.ToString();
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		IsHaulJobToStorage = sn.DoBool(IsHaulJobToStorage);
		return this;
	}
}
