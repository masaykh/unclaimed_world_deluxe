using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Trees;

namespace UWGame.SimSide.Resources;

public class Crop : ResourceContainer
{
	public IHasCrops Parent;

	private HasCropsID snapshotParent;

	private Entity parentEntity;

	private EntityID? snapshotParentEntity;

	public float TotalBulk;

	public float TotalBulkOfRipeItems;

	private List<IResourceItem> ripeCropItems = new List<IResourceItem>();

	private List<ResourceItemID> snapshotRipeCropItems;

	private List<IResourceItem> cropItems = new List<IResourceItem>();

	private List<ResourceItemID> snapshotCropItems;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public override Entity ParentEntity => parentEntity;

	public override EntityType EntityType => null;

	public override bool IsIntelligent => false;

	public override Vector3 AccessPoint => Parent.AccessPoint;

	public override List<IResourceItem> ResourceItems => ripeCropItems;

	public override Point MapPosition => Parent.MapPosition;

	public override float TotalHarvestableBulk => TotalBulkOfRipeItems;

	public override int NoOfHarvestableItems => ripeCropItems.Count;

	public override Renderable Renderable
	{
		get
		{
			if (ParentEntity != null)
			{
				return ParentEntity.Renderable;
			}
			return null;
		}
	}

	public override Vector3 Location => Parent.Location;

	public override IResourceItem FindHarvestableItem()
	{
		return GetRipeItem();
	}

	public Crop()
	{
	}

	public Crop(IHasCrops parent, ResourceType cropType)
		: base(cropType)
	{
		((ILookUp<IDetectable, DetectableID>)this).AddToLookup();
		Parent = parent;
		if (parent is Tree tree)
		{
			parentEntity = tree.Parent;
		}
	}

	public override void Destroy()
	{
		base.Destroy();
		((ILookUp<IDetectable, DetectableID>)this).RemoveIDEntry();
	}

	public override bool IsDestroyed(SharedKnowledge knowledge)
	{
		if (ParentEntity != null)
		{
			if (GoalEvaluator.EntityDataResultCausesSkip(knowledge.GetKnownData(ParentEntity.EntityID, out var _)))
			{
				return true;
			}
			return false;
		}
		if (base.ID != ResourceID.Invalid)
		{
			return LookUp<ResourceContainer, ResourceID>.FindByID(base.ID) == null;
		}
		return true;
	}

	public void GetGrowthNeeds(double deltaTimeInSeconds, out float water, out float nitrogen, out float phosphorous, float age, float plantBulk)
	{
		_ = resourceType.CropType.MaxSizeShareOfWholePlant;
		Common.GetInterpolatedFunctionValue(age, resourceType.CropType.AgeProduction);
		_ = The.Sim.DateAndTime.DaysPerSecond;
		water = 0f;
		nitrogen = 0f;
		phosphorous = 0f;
	}

	public void GrowAndRipen(float availableForCropGrowth, double deltaTimeInSeconds)
	{
		double deltaDays = The.Sim.DateAndTime.DaysPerSecond * deltaTimeInSeconds;
		float num = -1f;
		float newCropItemBuds = resourceType.CropType.MaxSizeShareOfWholePlant * num;
		foreach (CropItem cropItem in cropItems)
		{
			if (!cropItem.IsFullyGrown)
			{
				cropItem.Grow(ref availableForCropGrowth, deltaDays);
			}
			else if (!cropItem.IsRipe() && resourceType.CropType.RipeSpeed.HasValue)
			{
				cropItem.Ripen(deltaDays);
			}
		}
		UpdateBulkAndSprites(NoOfHarvestableItems);
		SetNewCropItemBuds(newCropItemBuds);
	}

	protected override void UpdateBulkAndSprites(int noOfItems)
	{
		base.UpdateBulkAndSprites(noOfItems);
		_ = TotalBulk;
		_ = TotalBulkOfRipeItems;
		TotalBulk = 0f;
		TotalBulkOfRipeItems = 0f;
		ripeCropItems.Clear();
		foreach (CropItem cropItem in cropItems)
		{
			TotalBulk += cropItem.Bulk;
			if (cropItem.IsRipe())
			{
				TotalBulkOfRipeItems += cropItem.Bulk;
				ripeCropItems.Add(cropItem);
			}
		}
		if (ParentEntity != null && resourceType.CropType.TreeSpriteFlag.HasValue)
		{
			ParentEntity.Renderable.SetOrClearSpriteStateFlag(TotalBulk > resourceType.CropType.BulkLimitToShowFlag, resourceType.CropType.TreeSpriteFlag.Value);
			if (resourceType.CropType.RipeSpeed.HasValue)
			{
				ParentEntity.Renderable.SetOrClearSpriteStateFlag(NoOfHarvestableItems > 0, StateModifier.Ripe);
			}
		}
	}

	public void SetRandomCrops(float bulkFactor, float mean, float standardDeviation)
	{
		bulkFactor = Common.ClampTop(bulkFactor, 1f);
		float num = Common.ClampBottom((float)The.Sim.GameplayRandomGenerator.RandomNormalDistribution(mean, standardDeviation), 0f) * bulkFactor;
		bool hasValue = base.ResourceType.CropType.RipeSpeed.HasValue;
		int num2 = (int)Math.Round(num, MidpointRounding.ToEven);
		if (hasValue)
		{
			num2 *= 2;
		}
		SetResourceItems(num2);
	}

	public override void SetResourceItems(int noOfItems)
	{
		int count = cropItems.Count;
		if (noOfItems > count)
		{
			AddResourceItems(noOfItems - count);
		}
		else if (noOfItems < count)
		{
			int amounttoRemove = count - noOfItems;
			RemoveResourceItems(amounttoRemove);
		}
	}

	public override void RemoveResourceItems(int amounttoRemove)
	{
		while (amounttoRemove > 0 && cropItems.Count > 0)
		{
			cropItems[cropItems.Count - 1].Destroy();
			cropItems.RemoveAt(cropItems.Count - 1);
			amounttoRemove--;
		}
		UpdateBulkAndSprites(cropItems.Count);
	}

	public override void AddResourceItems(int noOfItemsToAdd)
	{
		for (int i = 0; i < noOfItemsToAdd; i++)
		{
			CropItem cropItem = new CropItem(this);
			cropItems.Add(cropItem);
			cropItem.Bulk = base.ResourceType.ResourceItemType.ItemType.MaximumBulk.Value;
			cropItem.IsFullyGrown = true;
			cropItem.Ripeness = 1f;
		}
		UpdateBulkAndSprites(cropItems.Count);
	}

	private void SetNewCropItemBuds(float maxCropBulk)
	{
		maxCropBulk *= 1.5f;
		float num = maxCropBulk - (float)cropItems.Count * base.ResourceType.ResourceItemType.ItemType.MaximumBulk.Value;
		if (num > 0f)
		{
			int num2 = Math.Max(1, (int)(num / base.ResourceType.ResourceItemType.ItemType.MaximumBulk.Value));
			for (int i = 0; i < num2; i++)
			{
				cropItems.Add(new CropItem(this));
			}
		}
	}

	public override bool GatherResource(IResourceItem cropItem)
	{
		if (cropItem != null && ripeCropItems.Contains(cropItem))
		{
			cropItems.Remove(cropItem);
			UpdateBulkAndSprites(cropItems.Count);
			return true;
		}
		return false;
	}

	public CropItem GetRipeItem()
	{
		return (CropItem)ripeCropItems[0];
	}

	public Tree GetTree()
	{
		return Parent as Tree;
	}

	public override bool GetClosestAccessibleHarvestLocation(SubtileLayers movemap, Vector3 fromLocation, out Vector3? closestLocation)
	{
		Vector3 accessPoint = Parent.AccessPoint;
		if (The.Map.GetClosestAccessiblePoint(movemap, fromLocation, accessPoint, stayInsideTile: false, out var closestSubtile))
		{
			closestLocation = MapManager.SubTileToWorldPos3(closestSubtile.Value);
			return true;
		}
		closestLocation = null;
		return false;
	}

	public override bool RequiresRollToDetect()
	{
		return true;
	}

	public override bool UsesMemory(SharedKnowledge sharedKnowledge)
	{
		return true;
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		TotalBulk = sn.DoFloat(TotalBulk);
		TotalBulkOfRipeItems = sn.DoFloat(TotalBulkOfRipeItems);
		snapshotParentEntity = sn.SnapshotID<Entity, EntityID>(parentEntity);
		snapshotParent = sn.SnapshotID<IHasCrops, HasCropsID>(Parent).Value;
		snapshotCropItems = cropItems.Select((IResourceItem r) => r.ID).ToList();
		snapshotCropItems = sn.DoList(snapshotCropItems);
		snapshotRipeCropItems = ripeCropItems.Select((IResourceItem r) => r.ID).ToList();
		snapshotRipeCropItems = sn.DoList(snapshotRipeCropItems);
		sn.Ignore(ripeCropItems);
		sn.Ignore(cropItems);
		return this;
	}

	public override Snapshotter.Version DoVersion(Snapshotter sn)
	{
		base.DoVersion(sn);
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		base.LoadPostProcess(sn);
		Parent = LookUpIHasCrops.FindByID(snapshotParent);
		parentEntity = Entity.FindByID(snapshotParentEntity);
		cropItems = snapshotCropItems.Select((ResourceItemID r) => LookUp<IResourceItem, ResourceItemID>.FindByID(r)).ToList();
		ripeCropItems = snapshotRipeCropItems.Select((ResourceItemID r) => LookUp<IResourceItem, ResourceItemID>.FindByID(r)).ToList();
	}
}
