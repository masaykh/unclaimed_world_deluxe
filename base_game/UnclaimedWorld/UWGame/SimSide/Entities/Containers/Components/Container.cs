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

/* Design idea - don't delete!
IGarrison and IStorage interfaces, which the rest of the game classes use to relate to a Container component in different ways.
[02:29:41] Mark Lorenzen: Container as IGarrison
[02:29:52] Mark Lorenzen: Container as IEquipment
[02:30:05] Mark Lorenzen: Container as IStorage
[02:31:26] Mark Lorenzen: each of these interfaces would allow other classes to enter, exit, store, remember, inhabit, transport, crew... the container, if the interface is present.
[02:31:50] Mark Lorenzen: in the case of an encampment building...
[02:32:01] Mark Lorenzen: it would need to do at least two things:
[02:32:12] Mark Lorenzen: 1) be a place to leave your stuff
[02:32:34] Mark Lorenzen: 2) be a place to go into and do homey stuff like sleep
[02:32:59] Mark Lorenzen: so write a class ContainerHome
[02:33:19] Mark Lorenzen: inherit IStorage and IGarrison
[02:33:57] Mark Lorenzen: IStorage exposes the CombinedStorage style functions.
[02:34:19] Mark Lorenzen: IGarrison would be new, exposing the functions for people entering and Exiting
[02:34:42] Mark Lorenzen: also inherit IExit, which handles the physical transitioning into and out of the building.     

other buildings might be IGarrison but not IStorage... like a watchtower
[02:35:45] Mark Lorenzen: or the other way around... a pup tent is a home but not a storage
[02:36:16] Mark Lorenzen: a vehicle would inherit ITransport and ICrew
[02:36:27] Mark Lorenzen: the crew is the driver
[02:36:40] Mark Lorenzen: passengers and items are contained via ITransport
*/

/// <summary>
/// Base containment component for the systems that hold, move, and release contained entities.
/// Different container implementations combine storage, garrison, transport, and related behaviors.
/// </summary>
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

	/// <summary>
	/// Some containers hide their contents, while others keep them visible.
	/// Open containers allow contained entities to keep their visible presence.
	/// </summary>
	public virtual bool IsOpenContainer()
	{
		return false;
	}

	/// <summary>
	/// Returns whether a specific contained entity should remain visible.
	/// </summary>
	public virtual bool IsVisible(Entity entity)
	{
		return false;
	}

	public abstract void Destroy();

	/// <summary>
	/// Everyone and everything inside, out onto the ground - the first half of Destroy, without
	/// destroying the storage or the upgrades. For a building broken open (HomeRaidMod), which
	/// stays standing, broken and repairable. Nothing by default: only containers that hold
	/// people or items have anything to throw out.
	/// </summary>
	public virtual void ThrowOutContents()
	{
	}

	/// <summary>
	/// Reset all play-site regulators when leaving the play site.
	/// Otherwise elapsed time can keep accruing while the entity is away and cause a large time delta when it returns.
	/// </summary>
	public virtual void ResetPlaySiteRegulators()
	{
	}

	/// <summary>
	/// Adds an entity to this container.
	/// This overload is the superset used by subclasses that need storage, replenish, production-output, or upgrade-specific placement.
	/// </summary>
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

	/// <summary>
	/// Specialized overload used by magazine-style containers that may produce a surplus entity.
	/// </summary>
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

	/// <summary>
	/// Central place for containment instrumentation and debug tracking.
	/// </summary>
	private void DoContainmentMonitoring(Entity entity, string action)
	{
		entity.DebugLog.Add(string.Format("Containment: Was {1} container {0}", Parent.ID, action));
		Parent.DebugLog.Add($"Containment: {entity.ID} was {action} container");
	}

	/// <summary>
	/// Subclass-specific containment logic used by <see cref="AddToContain(Entity, StorageCompartment?, StorageCondition, bool, bool, bool, bool, UpgradeCategory)"/>.
	/// </summary>
	protected abstract bool AddToContainList(Entity entity, StorageCompartment? compartment = null, StorageCondition placeInStorage = null, List<PassengerOrCargoSlot> slotsToUse = null, bool ignoreCapacity = false, bool replenish = false, bool isProductionOutput = false, UpgradeCategory upgradeCategory = null);

	/// <summary>
	/// Specialized containment hook used by magazine-style containers.
	/// </summary>
	protected virtual bool AddToContainList(Entity entity, out Entity surplusEntity, StorageCompartment? compartment = null, StorageCondition placeInStorage = null)
	{
		surplusEntity = null;
		return false;
	}

	/// <summary>
	/// Subclass-specific portion of <see cref="Remove(Entity, List{PassengerOrCargoSlot})"/>.
	/// </summary>
	protected abstract bool RemoveFromContain(Entity entity, List<PassengerOrCargoSlot> slotsToUse = null);

	/// <summary>
	/// Clears <see cref="Entity.ContainedBy"/> and lets subclasses remove the entity from their own collections.
	/// Warning: after this call the entity is in an in-between state until it is destroyed or placed elsewhere.
	/// </summary>
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

	/// <summary>
	/// Debug-only assertion helper for containment state.
	/// </summary>
	protected void ValidateContainStatus(Entity entity)
	{
	}

	/// <summary>
	/// Returns whether this container currently contains the given entity.
	/// </summary>
	public abstract bool Contains(EntityID entityID);

	/// <summary>
	/// Replaces one contained entity with another, for example when an item degrades into junk or an entity becomes a corpse.
	/// Implementations are responsible for placing the replacement in the correct compartment, even if capacity rules must be bypassed.
	/// </summary>
	public abstract void SwitchEntities(Entity entityToRemove, Entity exchangeWithEntity, bool ignoreCapacity = false);

	public virtual void NotifyBrokenContainedEntity(Entity entity)
	{
	}

	public virtual void NotifyFunctionalContainedEntity(Entity entity)
	{
	}

	public abstract List<Entity> GetContainedItemsList(Predicate<Entity> rule);

	public abstract void IterateContained(Action<Entity> iterateMethod);

	/// <summary>
	/// Atomically removes a contained entity and either destroys it or places it in another container or in the open.
	/// Exit and queue handling can be layered on top of this later.
	/// </summary>
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

	/// <summary>
	/// Call <see cref="Remove(Entity, List{PassengerOrCargoSlot})"/> first, then use this to place the entity in the world or directly into the specified storage without using exits or queuing.
	/// </summary>
	public bool EjectEntity(Entity entityToEject, StorageTarget? storageTarget = null, Entity placeInStorageEntity = null, StorageCompartment? compartment = null, StorageCondition placeInStorage = null, Vector3? placeOnGround = null, bool isOfferedForSale = false)
	{
		return EjectEntity(entityToEject, Parent, storageTarget, placeInStorageEntity, compartment, placeInStorage, placeOnGround, isOfferedForSale);
	}

	/// <summary>
	/// Handles ejection for both normal contained entities and ejected parts.
	/// </summary>
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

	/// <summary>
	/// Makes other allegiances forget entities that have moved out of communication range.
	/// If the entity is placed into a terminal container, it can be discovered again there.
	/// </summary>
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

	/// <summary>
	/// Places an ejected entity onto the parent entity's site and, when applicable, onto the play site at the chosen ground location.
	/// </summary>
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
