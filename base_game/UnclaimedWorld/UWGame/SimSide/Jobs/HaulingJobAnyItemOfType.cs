using System.Diagnostics;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Jobs;

[DebuggerDisplay("{RequiredItemType.KeyName} {ToLocation} {ToStorage}")]
public class HaulingJobAnyItemOfType : HaulingJob, ISnapshot
{
	public EntityType RequiredItemType;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public HaulingJobAnyItemOfType(Vector3 toLocation, EntityGroup entityGroup, EntityType requiredItemType, EntityGroupID ownerOfItemsToHaul, OwnerID? newOwner)
		: base(toLocation, null, isToTradeOfferStorage: false, entityGroup, newOwner, addToJobsGroupNow: false)
	{
		RequiredItemType = requiredItemType;
		entityGroup.AddJob(this);
		ItemsToHaulGroup = ownerOfItemsToHaul;
		ComputeJobType();
		SetDefaultPriority(entityGroup);
	}

	public HaulingJobAnyItemOfType()
	{
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder("Haul ");
		if (base.Item.HasValue)
		{
			stringBuilder.Append(base.Item.ToString());
		}
		else
		{
			stringBuilder.Append(RequiredItemType.Name);
		}
		stringBuilder.Append(" To: ");
		GetToLocationAsString(stringBuilder);
		return stringBuilder.ToString();
	}

	public override void Destroy(bool cancelTakers, Entity entityToExcludeFromCancel = null)
	{
		if (RequiredByProcessJob != null)
		{
			RequiredByProcessJob.AddLog(string.Concat("Destroyed HaulingJobAnyItemOfType: ", RequiredItemType.KeyName, ", JobID: ", base.ID, " , ItemID: ", base.Item.HasValue ? base.Item.Value.ToString() : ""));
		}
		base.Destroy(cancelTakers, entityToExcludeFromCancel);
	}

	public new Snapshotter.Version DoVersion(Snapshotter sn)
	{
		base.DoVersion(sn);
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		RequiredItemType = sn.DoGameData(RequiredItemType);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		base.LoadPostProcess(sn);
	}
}
