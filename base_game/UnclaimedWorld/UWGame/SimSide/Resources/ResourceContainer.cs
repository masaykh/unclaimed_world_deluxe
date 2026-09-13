using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.AI;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems;

namespace UWGame.SimSide.Resources;

public abstract class ResourceContainer : ILookUp<ResourceContainer, ResourceID>, ISnapshot, IDetectable, ILookUp<IDetectable, DetectableID>
{
	protected ResourceType resourceType;

	private ResourceReplenish resourceReplenish;

	private DetectableID detectableID;

	private ResourceID id = ResourceID.Invalid;

	private static ResourceID IDCounter = ResourceID.First;

	private Snapshotter.Version version;

	public ResourceType ResourceType => resourceType;

	public DetectableID DetectableID => detectableID;

	public abstract Renderable Renderable { get; }

	public abstract float TotalHarvestableBulk { get; }

	public abstract Point MapPosition { get; }

	public abstract Vector3 Location { get; }

	public abstract Vector3 AccessPoint { get; }

	public abstract EntityType EntityType { get; }

	public abstract bool IsIntelligent { get; }

	public abstract Entity ParentEntity { get; }

	public abstract int NoOfHarvestableItems { get; }

	public abstract List<IResourceItem> ResourceItems { get; }

	DetectableID ILookUp<IDetectable, DetectableID>.ID => detectableID;

	public ResourceID ID
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

	public ResourceContainer()
	{
	}

	public ResourceContainer(ResourceType resourceType)
	{
		AddToLookup();
		this.resourceType = resourceType;
		if (resourceType.CanReplenish())
		{
			resourceReplenish = new ResourceReplenish();
		}
		SetNextReplenishTimepoint();
	}

	private void SetNextReplenishTimepoint()
	{
		if (resourceReplenish != null)
		{
			resourceReplenish.SetNextReplenishTimepoint(this);
		}
	}

	public float GetMaximumRegrowth()
	{
		if (resourceReplenish != null)
		{
			return resourceReplenish.GetMaximumReplenishRate(this);
		}
		return 0f;
	}

	public float GetCurrentRegrowth(out bool maximumReached)
	{
		if (resourceReplenish != null)
		{
			return resourceReplenish.GetCurrentReplenishRate(this, out maximumReached);
		}
		maximumReached = true;
		return 0f;
	}

	public abstract bool RequiresRollToDetect();

	public abstract bool UsesMemory(SharedKnowledge sharedKnowledge);

	public abstract bool IsDestroyed(SharedKnowledge knowledge);

	public abstract bool GetClosestAccessibleHarvestLocation(SubtileLayers movemap, Vector3 fromLocation, out Vector3? closestLocation);

	public abstract bool GatherResource(IResourceItem resourceItem);

	protected virtual void UpdateBulkAndSprites(int noOfItems)
	{
		if (resourceReplenish != null)
		{
			resourceReplenish.UpdateMaxItemsEverSet(noOfItems);
		}
	}

	public abstract void SetResourceItems(int noOfItems);

	public virtual double? GetUpdateInterval()
	{
		double? currentInterval = null;
		if (resourceReplenish != null)
		{
			UpdateTimePoints.GetSoonestInterval(resourceReplenish.GetUpdateInterval(), ref currentInterval);
		}
		return currentInterval;
	}

	public virtual void RemoveResourceItems(int amounttoRemove)
	{
	}

	public virtual void AddResourceItems(int noOfItemsToAdd)
	{
	}

	public virtual void Update(GameTime gameTime)
	{
		if (resourceReplenish != null)
		{
			resourceReplenish.Update(gameTime, this);
		}
	}

	public abstract IResourceItem FindHarvestableItem();

	public override string ToString()
	{
		return resourceType.Name;
	}

	public string ToLink(bool useUpperCase = false)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("§C");
		stringBuilder.Append(id.ToString());
		stringBuilder.Append("¤");
		if (useUpperCase)
		{
			stringBuilder.Append(ToString().ToUpper(Config.Culture));
		}
		else
		{
			stringBuilder.Append(ToString());
		}
		stringBuilder.Append("§");
		return stringBuilder.ToString();
	}

	public void FlashAsDetected()
	{
		if (Renderable != null)
		{
			float duration = ResourceType.DetectionFlashDuration ?? GameData.Instance.Constants.FlashDuration;
			Renderable.SetResourceContainerColorFlashing(GameData.Instance.Constants.FlashingColorWhenDetected, duration);
		}
	}

	public void FlashWhenClicked()
	{
		if (Renderable != null)
		{
			float flashDuration = GameData.Instance.Constants.FlashDuration;
			Renderable.SetResourceContainerColorFlashing(GameData.Instance.Constants.FlashingColorWhenClicked, flashDuration);
		}
	}

	public virtual void Destroy()
	{
		RemoveIDEntry();
	}

	DetectableID ILookUp<IDetectable, DetectableID>.GetUniqueID()
	{
		return Detectable.GetUniqueID();
	}

	void ILookUp<IDetectable, DetectableID>.AddToLookup()
	{
		detectableID = ((ILookUp<IDetectable, DetectableID>)this).GetUniqueID();
		if (detectableID != DetectableID.Invalid)
		{
			LookUpIDetectables.Add(detectableID, this);
		}
	}

	void ILookUp<IDetectable, DetectableID>.RemoveIDEntry()
	{
		LookUpIDetectables.Remove(this);
	}

	void ILookUp<IDetectable, DetectableID>.ResetIDCounter()
	{
	}

	void ILookUp<IDetectable, DetectableID>.SetInvalid()
	{
		detectableID = DetectableID.Invalid;
	}

	void ILookUp<IDetectable, DetectableID>.CreateLookupCollection()
	{
	}

	public ResourceID GetUniqueID()
	{
		IDCounter++;
		if (IDCounter >= ResourceID.Invalid)
		{
			throw new Exception("Astounding, ResourceID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public ResourceID SnapshotID(Snapshotter sn, ResourceID id)
	{
		return sn.DoEnum(id);
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != ResourceID.Invalid)
		{
			LookUp<ResourceContainer, ResourceID>.Add(ID, this);
		}
	}

	public void SetInvalid()
	{
		id = ResourceID.Invalid;
	}

	public void RemoveIDEntry()
	{
		LookUp<ResourceContainer, ResourceID>.Remove(this);
	}

	void ILookUp<ResourceContainer, ResourceID>.ResetIDCounter()
	{
	}

	public static void ResetIDCounter()
	{
		IDCounter = ResourceID.First;
	}

	void ILookUp<ResourceContainer, ResourceID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUp<ResourceContainer, ResourceID>.Create();
	}

	public virtual ISnapshot DoSnapshot(Snapshotter sn)
	{
		id = SnapshotID(sn, id);
		IDCounter = sn.DoEnum(IDCounter);
		detectableID = sn.DoEnum(detectableID);
		resourceType = sn.DoGameData(resourceType);
		resourceReplenish = (ResourceReplenish)sn.DoISnapshot(resourceReplenish);
		return this;
	}

	public virtual void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		if (resourceReplenish != null)
		{
			resourceReplenish.LoadPostProcess(sn);
		}
	}

	public virtual Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}
}
