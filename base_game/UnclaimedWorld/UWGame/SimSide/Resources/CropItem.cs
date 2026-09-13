using System;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Resources;

public class CropItem : IResourceItem, ILookUp<IResourceItem, ResourceItemID>, ISnapshot
{
	public float Ripeness;

	public float Bulk;

	public float GrowthSpeedFactor;

	public Crop crop;

	private ResourceID snapshotCrop;

	public bool IsFullyGrown;

	private JobID? snapshotJob;

	private ResourceItemID id = ResourceItemID.Invalid;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public ResourceContainer Container => crop;

	public ProcessJob AssignedToJob { get; set; }

	public ResourceItemID ID
	{
		get
		{
			return id;
		}
		private set
		{
			id = value;
		}
	}

	public int LoadPostProcessOrder => 0;

	public bool IsSnapshotted { get; set; }

	public CropItem()
	{
	}

	public CropItem(Crop crop)
	{
		AddToLookup();
		GrowthSpeedFactor = 1.25f - 0.25f * (float)(The.Sim.GameplayRandomGenerator.NextDouble("CropItem") + The.Sim.GameplayRandomGenerator.NextDouble("CropItem"));
		this.crop = crop;
	}

	public bool Grow(ref float availableForGrowth, double deltaDays)
	{
		float val = (float)((double)GrowthSpeedFactor * deltaDays * (double)crop.ResourceType.CropType.CropItemGrowthPerDay);
		val = Math.Min(availableForGrowth, val);
		val = Math.Min(crop.ResourceType.ResourceItemType.ItemType.MaximumBulk.Value - Bulk, val);
		if (val > 0f)
		{
			Bulk += val;
			availableForGrowth -= val;
			if (Bulk >= crop.ResourceType.ResourceItemType.ItemType.MaximumBulk.Value)
			{
				IsFullyGrown = true;
				Bulk = crop.ResourceType.ResourceItemType.ItemType.MaximumBulk.Value;
			}
			return true;
		}
		return false;
	}

	public void Ripen(double deltaDays)
	{
		Ripeness += (float)(deltaDays * (double)crop.ResourceType.CropType.RipeSpeed.Value);
		Ripeness = Math.Min(1f, Ripeness);
	}

	public void UpdateSimulation(double deltaTimeInSeconds)
	{
	}

	public bool IsRipe()
	{
		if (crop.ResourceType.CropType.RipeSpeed.HasValue)
		{
			return Ripeness >= 1f;
		}
		return true;
	}

	public void Destroy()
	{
		RemoveIDEntry();
	}

	public ResourceItemID GetUniqueID()
	{
		return ResourceItem.GetUniqueID();
	}

	public ResourceItemID SnapshotID(Snapshotter sn, ResourceItemID id)
	{
		return sn.DoEnum(id);
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != ResourceItemID.Invalid)
		{
			LookUp<IResourceItem, ResourceItemID>.Add(ID, this);
		}
	}

	public void RemoveIDEntry()
	{
		LookUp<IResourceItem, ResourceItemID>.Remove(this);
	}

	public void SetInvalid()
	{
		id = ResourceItemID.Invalid;
	}

	public void ResetIDCounter()
	{
	}

	void ILookUp<IResourceItem, ResourceItemID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUp<IResourceItem, ResourceItemID>.Create();
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		id = sn.DoEnum(id);
		snapshotJob = sn.SnapshotID<Job, JobID>(AssignedToJob);
		Bulk = sn.DoFloat(Bulk);
		snapshotCrop = sn.SnapshotID<ResourceContainer, ResourceID>(crop).Value;
		GrowthSpeedFactor = sn.DoFloat(GrowthSpeedFactor);
		IsFullyGrown = sn.DoBool(IsFullyGrown);
		Ripeness = sn.DoFloat(Ripeness);
		sn.Ignore(crop);
		sn.Ignore(AssignedToJob);
		return this;
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		crop = (Crop)LookUp<ResourceContainer, ResourceID>.FindByID(snapshotCrop);
		if (snapshotJob.HasValue)
		{
			AssignedToJob = (ProcessJob)LookUp<Job, JobID>.FindByID(snapshotJob);
		}
	}
}
