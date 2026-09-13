using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Combat;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Items;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Vehicles;

namespace UWGame.SimSide.Entities.Containers.Components;

public abstract class Container : ISnapshot
{
	public delegate void IterateMethod(Entity thisEntity);

	public delegate bool IterateBoolMethod(Entity thisEntity);

	public Entity Parent;

	private EntityID parentID;

	private Snapshotter.Version version;

	public bool IsSnapshotted { get; set; }

	protected Container()
	{
	}

	public Container(Entity parent)
	{
		Parent = parent;
	}

	public virtual bool isGarrison()
	{
		return false;
	}

	public virtual bool IsOpenContainer()
	{
		return false;
	}

	public virtual bool IsVisible(Entity entity)
	{
		return false;
	}

	public virtual void ReactToTransformChange()
	{
	}

	public virtual bool OnEntityWantsToEnterOrExit(Entity entity, EnterExitType wants)
	{
		return false;
	}

	public virtual bool hasentitysWantingToEnterOrExit()
	{
		return false;
	}

	public virtual void onContaining(Entity entity, bool wasSelected)
	{
	}

	public virtual void onRemoving(Entity entity)
	{
	}

	public virtual void onPassengerDamage(Entity entity, UWGame.SimSide.Entities.Body.Body body, AttackType attack)
	{
	}

	public virtual void orderAllPassengersToExit()
	{
	}

	public virtual void orderOnePassengersToExit(Entity passenger)
	{
	}

	public abstract void Destroy();

	public virtual void ResetPlaySiteRegulators()
	{
	}

	public bool AddToContain(Entity entity, StorageCompartment? compartment = null, StorageCondition placeInStorage = null, bool ignoreCapacity = false, bool replenish = false, bool isProductionOutput = false, bool assertContainment = true, UpgradeCategory upgradeCategory = null)
	{
		ValidateContainStatus(entity);
		bool num = AddToContainList(entity, compartment, placeInStorage, null, ignoreCapacity, replenish, isProductionOutput, upgradeCategory);
		if (num)
		{
			entity.ContainedBy = Parent.EntityID;
			DoContainmentMonitoring(entity, "added to");
			if (entity.Renderable != null)
			{
				entity.Renderable.UpdateIsDrawnStatus();
			}
			UpdateRenderFlags();
			entity.DisableCollisions();
		}
		if (assertContainment)
		{
			ValidateContainStatus(entity);
		}
		return num;
	}

	public bool AddToContain(Entity entity, out Entity surplusEntity, StorageCompartment? compartment = null, StorageCondition placeInStorage = null)
	{
		bool num = AddToContainList(entity, out surplusEntity, compartment, placeInStorage);
		if (num)
		{
			entity.ContainedBy = Parent.EntityID;
			DoContainmentMonitoring(entity, "added to");
			UpdateRenderFlags();
			entity.DisableCollisions();
		}
		ValidateContainStatus(entity);
		return num;
	}

	public virtual double? GetUpdateInterval()
	{
		if (this is IHasReplenishItems { ReplenishItems: not null })
		{
			return GameData.Instance.Constants.UpdateIntervalForEntityComponents;
		}
		return null;
	}

	private void UpdateRenderFlags()
	{
		float? num = null;
		if (this is IStorage storage)
		{
			num = storage.TotalStored / storage.TotalItemStorageCapacity;
		}
		else if (this is IHoldsProductionOutput { TotalStoredOutput: not null, TotalStoredOutput: var totalStoredOutput, TotalOutputCapacity: var totalOutputCapacity })
		{
			num = totalStoredOutput / totalOutputCapacity;
		}
		if (!num.HasValue)
		{
			return;
		}
		float num2 = Parent.EntityType.ContainerType.FullStatePercentage ?? 0.9f;
		float num3 = Parent.EntityType.ContainerType.HalfFullStatePercentage ?? 0.4f;
		if (Parent.Renderable != null)
		{
			if (num > num2)
			{
				Parent.SetSpriteStateFlag(StateModifier.Full);
				return;
			}
			if (num > num3)
			{
				Parent.SetSpriteStateFlag(StateModifier.HalfFull);
				return;
			}
			Parent.ClearSpriteStateFlag(StateModifier.HalfFull);
			Parent.ClearSpriteStateFlag(StateModifier.Full);
		}
	}

	private void DoContainmentMonitoring(Entity entity, string action)
	{
		entity.DebugLog.Add(string.Format("Containment: Was {1} container {0}", Parent.ID, action));
		Parent.DebugLog.Add($"Containment: {entity.ID} was {action} container");
	}

	protected abstract bool AddToContainList(Entity entity, StorageCompartment? compartment = null, StorageCondition placeInStorage = null, List<PassengerOrCargoSlot> slotsToUse = null, bool ignoreCapacity = false, bool replenish = false, bool isProductionOutput = false, UpgradeCategory upgradeCategory = null);

	protected virtual bool AddToContainList(Entity entity, out Entity surplusEntity, StorageCompartment? compartment = null, StorageCondition placeInStorage = null)
	{
		surplusEntity = null;
		return false;
	}

	protected abstract bool RemoveFromContain(Entity entity, List<PassengerOrCargoSlot> slotsToUse = null);

	public bool Remove(Entity entity, List<PassengerOrCargoSlot> slotsToUse = null)
	{
		ValidateContainStatus(entity);
		if (entity.ContainedBy == Parent.EntityID || (!entity.ContainedBy.HasValue && entity.PartOfID.HasValue))
		{
			entity.ContainedBy = null;
			DoContainmentMonitoring(entity, "removed from");
			Item item = entity.Item;
			if (item != null)
			{
				item.OKToTakeThisItemFromCarrier = null;
			}
			if (entity.Renderable != null)
			{
				entity.Renderable.IsDrawn = true;
			}
			entity.EnableCollisions();
			RemoveFromContain(entity, slotsToUse);
			UpdateRenderFlags();
			HandleAllegiancesInCommRangeRemoveEntity(entity);
			ValidateContainStatus(entity);
			return true;
		}
		return false;
	}

	protected void ValidateContainStatus(Entity entity)
	{
	}

	public abstract bool Contains(EntityID entityID);

	public abstract void SwitchEntities(Entity entityToRemove, Entity exchangeWithEntity, bool ignoreCapacity = false);

	public virtual ContainedEntityStatus getContainedStatusForEntity(Entity queryentity)
	{
		return ContainedEntityStatus.None;
	}

	public virtual bool isPassengerAllowedToFire()
	{
		return false;
	}

	public virtual bool isOccupantBlockedByContainer(Entity entity, Entity containedTarget)
	{
		return false;
	}

	public virtual bool isDisplayedInUI()
	{
		return false;
	}

	public virtual EntityID getClosestRiderToPosition(Vector3 position)
	{
		return EntityID.Invalid;
	}

	public virtual void enableEnter(bool bEnable)
	{
	}

	public virtual void restoreDefaultOpenness()
	{
	}

	public virtual void NotifyBrokenContainedEntity(Entity entity)
	{
	}

	public virtual void NotifyFunctionalContainedEntity(Entity entity)
	{
	}

	public abstract List<Entity> GetContainedItemsList(Predicate<Entity> rule);

	public abstract void IterateContained(Action<Entity> iterateMethod);

	public bool Uncontain(Entity entity, bool destroy = false, bool shouldQueue = false, StorageTarget? storageTarget = null, Entity placeInStorageEntity = null, StorageCompartment? compartment = null, StorageCondition placeInStorage = null, List<PassengerOrCargoSlot> slotsToUse = null, Vector3? placeOnGround = null)
	{
		ValidateContainStatus(entity);
		if (Remove(entity, slotsToUse))
		{
			bool flag = false;
			if (destroy)
			{
				entity.Destroy();
			}
			else if (!(shouldQueue && flag))
			{
				return EjectEntity(entity, storageTarget, placeInStorageEntity, compartment, placeInStorage, placeOnGround);
			}
			ValidateContainStatus(entity);
			return true;
		}
		ValidateContainStatus(entity);
		return false;
	}

	public bool EjectEntity(Entity entityToEject, StorageTarget? storageTarget = null, Entity placeInStorageEntity = null, StorageCompartment? compartment = null, StorageCondition placeInStorage = null, Vector3? placeOnGround = null, bool isOfferedForSale = false)
	{
		return EjectEntity(entityToEject, Parent, storageTarget, placeInStorageEntity, compartment, placeInStorage, placeOnGround, isOfferedForSale);
	}

	public static bool EjectEntity(Entity entityToEject, Entity parentOrPartOfEntity, StorageTarget? storageTarget = null, Entity placeInStorageEntity = null, StorageCompartment? compartment = null, StorageCondition placeInStorage = null, Vector3? placeOnGround = null, bool isOfferedForSale = false)
	{
		Entity container = placeInStorageEntity;
		StorageCompartment? compartment2 = compartment;
		StorageCondition placeInStorage2 = placeInStorage;
		if (storageTarget.HasValue)
		{
			container = Entity.FindByID(storageTarget.Value.StorageEntity);
			if (container != null)
			{
				IStorage storage = container.Contains as IStorage;
				Storage storage2 = storage.FindStorage(storageTarget.Value.StorageID);
				compartment2 = storage.GetCompartment(storageTarget.Value.StorageID);
				placeInStorage2 = storage2.StorageConditions;
			}
		}
		else if (placeInStorageEntity == null && !parentOrPartOfEntity.GetContainedBy(out container))
		{
			return false;
		}
		if (container != null)
		{
			if (!container.Contains.AddToContain(entityToEject, compartment2, placeInStorage2))
			{
				PlaceOnGround(entityToEject, parentOrPartOfEntity, placeOnGround);
				return false;
			}
		}
		else
		{
			PlaceOnGround(entityToEject, parentOrPartOfEntity, placeOnGround);
		}
		return true;
	}

	private void HandleAllegiancesInCommRangeRemoveEntity(Entity entityToEject)
	{
		if (Parent.EntityType.CommunicatorType == null)
		{
			return;
		}
		Allegiance allegianceOrOwner = Parent.GetAllegianceOrOwner();
		if (allegianceOrOwner == null)
		{
			return;
		}
		foreach (AllegianceID item in allegianceOrOwner.AllegiancesWeAreInContactWith)
		{
			Allegiance allegiance = LookUp<Allegiance, AllegianceID>.FindByID(item);
			if (allegiance != null && allegiance.Site != allegianceOrOwner.Site)
			{
				entityToEject.HandleEntityMovingOutOfCommunicationRange(allegiance);
			}
		}
	}

	private void PlaceOnGround(Entity entityToBePlaced, Vector3? placeOnGround)
	{
		PlaceOnGround(entityToBePlaced, Parent, placeOnGround);
	}

	private static void PlaceOnGround(Entity entityToBePlaced, Entity parentOrPartOfEntity, Vector3? placeOnGround)
	{
		entityToBePlaced.Site = parentOrPartOfEntity.Site;
		if (entityToBePlaced.IsOnPlaySite())
		{
			Vector3 location = placeOnGround ?? parentOrPartOfEntity.AccessPoint.Value;
			entityToBePlaced.PlaceEntityOnPlaySite(location, Entity.AddRandomOffset.Yes, null, null, null);
			if (entityToBePlaced.Renderable != null)
			{
				entityToBePlaced.Renderable.SetToParentLocation();
			}
		}
	}

	public virtual bool IsDriver(Entity entity)
	{
		return false;
	}

	public virtual bool IsPassenger(Entity entity)
	{
		return false;
	}

	public virtual bool IsDriverOrPassenger(Entity entity)
	{
		return false;
	}

	public static void ComputeSumsOfItems(Entity e, Dictionary<EntityType, int> containedEntities)
	{
		if (e.EntityType.NonLivingType != null)
		{
			int value = 0;
			if (containedEntities.TryGetValue(e.EntityType, out value))
			{
				value++;
				containedEntities[e.EntityType] = value;
			}
			else
			{
				containedEntities[e.EntityType] = 1;
			}
		}
	}

	public virtual ISnapshot DoSnapshot(Snapshotter sn)
	{
		parentID = sn.SnapshotID<Entity, EntityID>(Parent).Value;
		return this;
	}

	public virtual void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		Parent = Entity.FindByID(parentID);
	}

	public virtual Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}
}
