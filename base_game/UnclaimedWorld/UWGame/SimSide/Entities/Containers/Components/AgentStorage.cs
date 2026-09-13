using System;
using System.Collections.Generic;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.SimEffects;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Vehicles;
using Xclna.Xna.Animation;

namespace UWGame.SimSide.Entities.Containers.Components;

public class AgentStorage : Container, IStorage, IIDEventSubscriber
{
	public enum BurdenState
	{
		None,
		Mounted,
		Equipped,
		HaulLight,
		HaulHeavy
	}

	public ItemStorage ItemStorage;

	public ItemStorage Equipment;

	public ItemStorage Stomach;

	private MethodID? parentBulkChangedID;

	private EntityID? mountedToolOrWeapon;

	public bool isHaulingIsDirty = true;

	private bool isHauling;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public EntityID? MountedToolOrWeapon
	{
		get
		{
			return mountedToolOrWeapon;
		}
		set
		{
			if (!Parent.EntityType.IntelligenceType.CanMountToolsOrWeapons() || mountedToolOrWeapon == value || (value.HasValue && !Contains(value.Value)))
			{
				return;
			}
			Entity entity = null;
			if (value.HasValue)
			{
				entity = Entity.FindByID(value.Value);
			}
			Entity entity2 = null;
			if (mountedToolOrWeapon.HasValue)
			{
				entity2 = Entity.FindByID(mountedToolOrWeapon.Value);
			}
			if (Parent.Renderable != null)
			{
				if (entity2 != null)
				{
					Parent.Renderable.ClearAnimationStateFlags(entity2.EntityType.ItemType.AnimStatesWhenAttached);
					Parent.Renderable.RemoveAttachedMountedRenderables(entity2.EntityType.ItemType.AttachedObjectRenderableType);
				}
				if (entity != null)
				{
					if (entity.EntityType.ItemType.AttachedObjectRenderableType != null)
					{
						Parent.Renderable.AttachPooledObjectIfPossible(entity.EntityType.ItemType.AttachedObjectRenderableType, entity.EntityType.ItemType.AttachorTagToMountOn, AttacheePoint.RightHand, saveToSnapshot: true);
					}
					Parent.Renderable.SetAnimationStateFlags(entity.EntityType.ItemType.AnimStatesWhenAttached);
				}
			}
			mountedToolOrWeapon = value;
			if (entity2 != null)
			{
				EndEffects(entity2);
			}
			if (entity != null)
			{
				StartEffects(entity);
			}
		}
	}

	public bool IsHauling
	{
		get
		{
			if (isHaulingIsDirty)
			{
				isHauling = RecomputeIsHauling();
				isHaulingIsDirty = false;
			}
			return isHauling;
		}
	}

	public float TotalStored => ItemStorage.TotalStored;

	public float TotalItemStorageCapacity => ItemStorage.TotalCapacity;

	public AgentStorage()
	{
	}

	public AgentStorage(Entity parent)
		: base(parent)
	{
		AgentStorageType agentStorageType = (AgentStorageType)parent.EntityType.ContainerType;
		ItemStorage = new ItemStorage(parent, hasFixedCapacity: true, agentStorageType.ItemStorageType);
		if (agentStorageType.EquipmentStorageType != null)
		{
			Equipment = new ItemStorage(parent, hasFixedCapacity: true, agentStorageType.EquipmentStorageType);
		}
		if (agentStorageType.StomachStorageType != null)
		{
			Stomach = new ItemStorage(parent, hasFixedCapacity: false, agentStorageType.StomachStorageType);
			parent.BulkChangedEvent.AddAndRegister((Action<float>)parent_BulkChanged, (IIDEventSubscriber)this, out parentBulkChangedID);
		}
	}

	public Storage FindStorage(StorageID storageID)
	{
		Storage storage = ItemStorage.FindStorage(storageID);
		if (storage == null)
		{
			storage = Equipment.FindStorage(storageID);
		}
		if (storage == null)
		{
			storage = Stomach.FindStorage(storageID);
		}
		return storage;
	}

	public void NotifyStomachFractionChanged()
	{
		Stomach.TotalCapacity = Parent.BiologicalEntity.GetTotalStomachCapacity();
		Stomach.StorageSpaces[GameData.Instance.AllStorageConditions["isolated"]].TotalCapacity = Stomach.TotalCapacity;
	}

	private void parent_BulkChanged(float oldValue)
	{
		NotifyStomachFractionChanged();
	}

	public static BurdenState GetHaulBurdenState(float newStoredPercentage)
	{
		if (newStoredPercentage > GameData.Instance.Constants.StoredPercentageMeansHeavyHaul)
		{
			return BurdenState.HaulHeavy;
		}
		return BurdenState.HaulLight;
	}

	public bool RecomputeIsHauling(IKnownEntityData exceptItem = null, IKnownEntityData withItem = null)
	{
		isHauling = false;
		float num = TotalStored;
		if (exceptItem != null)
		{
			num -= exceptItem.Bulk;
		}
		else if (withItem != null)
		{
			num += withItem.Bulk;
		}
		if (GetHaulingPercentageOfCapacity(num) < GameData.Instance.Constants.StoredPercentageMeansHauling)
		{
			return isHauling;
		}
		for (int num2 = ItemStorage.StoredItems.Count - 1; num2 >= 0; num2--)
		{
			EntityID entityID = ItemStorage.StoredItems[num2];
			if (exceptItem == null || entityID != exceptItem.EntityID)
			{
				Entity entity = Entity.FindByID(entityID);
				if (entity != null)
				{
					if (ItemTriggersHauling(entity))
					{
						isHauling = true;
						break;
					}
				}
				else
				{
					ItemStorage.RemoveOutdatedItem(entityID);
				}
			}
		}
		if (withItem != null && ItemTriggersHauling(withItem))
		{
			isHauling = true;
		}
		return isHauling;
	}

	private bool ItemTriggersHauling(IKnownEntityData item)
	{
		Job job = EvaluateJob.ResolveAssignedToJob(item);
		if (job != null && (job is HaulingJob || (job is ProcessJob && !item.EntityType.IsMountable())) && job.TakenBy.Contains(Parent))
		{
			return true;
		}
		return false;
	}

	public StorageCompartment GetCompartment(StorageID storageID)
	{
		if (ItemStorage != null && ItemStorage.HasStorage(storageID))
		{
			return StorageCompartment.Haul;
		}
		if (Equipment != null && Equipment.HasStorage(storageID))
		{
			return StorageCompartment.Equipment;
		}
		return StorageCompartment.Stomach;
	}

	public BurdenState GetCurrentBurdenState()
	{
		if (IsHauling && !MountedToolOrWeapon.HasValue)
		{
			return GetHaulBurdenState(GetHaulingPercentageOfCapacity());
		}
		if (MountedToolOrWeapon.HasValue)
		{
			return BurdenState.Mounted;
		}
		if (Equipment != null && Equipment.TotalStored > 0f)
		{
			return BurdenState.Equipped;
		}
		return BurdenState.None;
	}

	public float GetMountedWeaponRange(out float? coneWidth, out float? coneLength)
	{
		coneWidth = null;
		coneLength = null;
		if (MountedToolOrWeapon.HasValue)
		{
			Entity entity = Entity.FindByID(MountedToolOrWeapon.Value);
			if (entity != null)
			{
				return entity.GetWeaponRange(ref coneWidth, ref coneLength);
			}
		}
		return 0f;
	}

	public void IterateContained(StorageCompartment compartment, Action<Entity> del)
	{
		switch (compartment)
		{
		case StorageCompartment.Haul:
			ItemStorage.IterateContained(del);
			break;
		case StorageCompartment.Equipment:
			Equipment.IterateContained(del);
			break;
		case StorageCompartment.Stomach:
			Stomach.IterateContained(del);
			break;
		}
	}

	public override void IterateContained(Action<Entity> del)
	{
		ItemStorage.IterateContained(del);
		if (Equipment != null)
		{
			Equipment.IterateContained(del);
		}
		if (Stomach != null)
		{
			Stomach.IterateContained(del);
		}
	}

	public void IterateContainedBreakOnTrue(IterateBoolMethod iterateMethod)
	{
		if (!ItemStorage.IterateContainedBreakOnTrue(iterateMethod))
		{
			if (Equipment != null)
			{
				Equipment.IterateContainedBreakOnTrue(iterateMethod);
			}
			if (Stomach != null)
			{
				Stomach.IterateContainedBreakOnTrue(iterateMethod);
			}
		}
	}

	public override List<Entity> GetContainedItemsList(Predicate<Entity> rule)
	{
		List<Entity> list = new List<Entity>();
		ItemStorage.GetContainedItemsList(rule, list);
		if (Equipment != null)
		{
			Equipment.GetContainedItemsList(rule, list);
		}
		if (Stomach != null)
		{
			Stomach.GetContainedItemsList(rule, list);
		}
		return list;
	}

	public Storage GetStoredIn(Entity entity)
	{
		Storage storedIn = ItemStorage.GetStoredIn(entity.EntityID);
		if (storedIn == null && Equipment != null)
		{
			storedIn = Equipment.GetStoredIn(entity.EntityID);
		}
		if (storedIn == null && Stomach != null)
		{
			storedIn = Stomach.GetStoredIn(entity.EntityID);
		}
		return storedIn;
	}

	public Dictionary<StorageCondition, Storage> GetStorageSpaces()
	{
		return ItemStorage.StorageSpaces;
	}

	public float GetHaulingPercentageOfCapacity(float stored)
	{
		return stored / ItemStorage.TotalCapacity;
	}

	public float GetHaulingPercentageOfCapacity()
	{
		return GetHaulingPercentageOfCapacity(ItemStorage.TotalStored);
	}

	public ItemStorage GetCompartment(StorageCompartment compartment)
	{
		return compartment switch
		{
			StorageCompartment.Equipment => Equipment, 
			StorageCompartment.Stomach => Stomach, 
			_ => ItemStorage, 
		};
	}

	public bool Contains(Entity item)
	{
		if (!ItemStorage.Contains(item) && (Equipment == null || !Equipment.Contains(item)) && (Stomach == null || !Stomach.Contains(item)))
		{
			return MountedToolOrWeapon == item.EntityID;
		}
		return true;
	}

	public override bool Contains(EntityID item)
	{
		if (!ItemStorage.Contains(item) && (Equipment == null || !Equipment.Contains(item)) && (Stomach == null || !Stomach.Contains(item)))
		{
			return MountedToolOrWeapon == item;
		}
		return true;
	}

	public override void Destroy()
	{
		ItemStorage.UncontainAllEntities();
		ItemStorage.Destroy();
		if (Equipment != null)
		{
			Equipment.UncontainAllEntities();
			Equipment.Destroy();
		}
		if (Stomach != null)
		{
			Stomach.UncontainAllEntities();
			Stomach.Destroy();
		}
	}

	protected override bool RemoveFromContain(Entity entity, List<PassengerOrCargoSlot> slots)
	{
		_ = entity.ID;
		_ = 18;
		bool flag = false;
		flag = ItemStorage.Remove(entity, removeFromChildStorage: true);
		if (!flag && Equipment != null)
		{
			flag = Equipment.Remove(entity, removeFromChildStorage: true);
			if (flag)
			{
				EndEffects(entity);
			}
		}
		if (!flag && Stomach != null)
		{
			flag = Stomach.Remove(entity, removeFromChildStorage: true);
		}
		if (MountedToolOrWeapon == entity.EntityID)
		{
			MountedToolOrWeapon = null;
		}
		if (flag)
		{
			if (Parent.Locomotor != null)
			{
				Parent.Locomotor.CurrentMaximumSpeedIsDirty = true;
				Parent.Locomotor.CurrentMaximumSpeedNotAffectedByTerrainIsDirty = true;
			}
			isHaulingIsDirty = true;
			HandleAttachedBoxAnimation();
		}
		return true;
	}

	private void HandleAttachedBoxAnimation()
	{
		BurdenState currentBurdenState = GetCurrentBurdenState();
		Parent.Renderable.UpdateBurdenState(currentBurdenState);
	}

	protected override bool AddToContainList(Entity entity, StorageCompartment? compartment = null, StorageCondition placeInStorage = null, List<PassengerOrCargoSlot> slotsToUse = null, bool ignoreCapacity = false, bool replenish = false, bool isProductionOutput = false, UpgradeCategory upgradeCategory = null)
	{
		_ = entity.ID;
		_ = 18;
		if (!compartment.HasValue || compartment.Value == StorageCompartment.Haul)
		{
			bool num = ItemStorage.Add(entity, placeInStorage, ignoreCapacity);
			if (num)
			{
				isHaulingIsDirty = true;
				if (Parent.Locomotor != null)
				{
					Parent.Locomotor.CurrentMaximumSpeedIsDirty = true;
					Parent.Locomotor.CurrentMaximumSpeedNotAffectedByTerrainIsDirty = true;
				}
			}
			return num;
		}
		if (compartment.Value == StorageCompartment.Equipment)
		{
			if (Equipment.Add(entity, placeInStorage, ignoreCapacity))
			{
				StartEffects(entity);
				return true;
			}
			return false;
		}
		if (Stomach != null && compartment.Value == StorageCompartment.Stomach)
		{
			if (Stomach.Add(entity, placeInStorage, ignoreCapacity))
			{
				Parent.BiologicalEntity.AddToStomachContents(entity.Bulk);
				return true;
			}
			return false;
		}
		return false;
	}

	private void StartEffects(Entity item)
	{
		if (item.EntityType.ItemType.FinalEffectsWhenEquipped == null)
		{
			return;
		}
		foreach (EffectProfileType item2 in item.EntityType.ItemType.FinalEffectsWhenEquipped)
		{
			Parent.SimEffects.Start(item2);
		}
	}

	private void EndEffects(Entity item)
	{
		if (item.EntityType.ItemType.FinalEffectsWhenEquipped == null)
		{
			return;
		}
		foreach (EffectProfileType item2 in item.EntityType.ItemType.FinalEffectsWhenEquipped)
		{
			Parent.SimEffects.Remove(item2);
		}
	}

	public void EndEffectsFromEquippedItem(Entity entity)
	{
		if (Equipment != null && Equipment.Contains(entity.EntityID))
		{
			EndEffects(entity);
		}
	}

	public override void NotifyBrokenContainedEntity(Entity entity)
	{
		EndEffectsFromEquippedItem(entity);
	}

	public override void SwitchEntities(Entity itemToRemove, Entity exchangeWithItem, bool ignoreCapacity = false)
	{
		if (MountedToolOrWeapon == itemToRemove.EntityID)
		{
			MountedToolOrWeapon = null;
		}
		if (Equipment != null && Equipment.Contains(itemToRemove.EntityID))
		{
			StorageCondition storageConditions = Equipment.GetStorageConditions(itemToRemove.EntityID);
			if (Remove(itemToRemove))
			{
				EndEffects(itemToRemove);
				AddToContain(exchangeWithItem, StorageCompartment.Equipment, storageConditions, ignoreCapacity: true);
			}
		}
		else if (ItemStorage.Contains(itemToRemove.EntityID))
		{
			StorageCondition storageConditions2 = ItemStorage.GetStorageConditions(itemToRemove.EntityID);
			if (Remove(itemToRemove))
			{
				AddToContain(exchangeWithItem, StorageCompartment.Haul, storageConditions2, ignoreCapacity: true);
			}
		}
		else if (Stomach != null && Stomach.Contains(itemToRemove.EntityID))
		{
			StorageCondition storageConditions3 = Stomach.GetStorageConditions(itemToRemove.EntityID);
			if (Remove(itemToRemove))
			{
				AddToContain(exchangeWithItem, StorageCompartment.Stomach, storageConditions3, ignoreCapacity: true);
			}
		}
	}

	public void MoveCarriedItemToCompartment(Entity item, StorageCompartment toCompartment)
	{
		if (toCompartment == StorageCompartment.Haul)
		{
			if (Equipment.Contains(item))
			{
				Equipment.Remove(item, removeFromChildStorage: true);
			}
			if (!ItemStorage.Contains(item))
			{
				ItemStorage.Add(item, null);
			}
		}
		else
		{
			if (ItemStorage.Contains(item))
			{
				ItemStorage.Remove(item, removeFromChildStorage: true);
			}
			if (!Equipment.Contains(item))
			{
				Equipment.Add(item, null);
			}
		}
	}

	public float GetFreeStomachCapacity()
	{
		if (Stomach != null)
		{
			return (1f - Parent.BiologicalEntity.StomachContents) * Stomach.TotalCapacity;
		}
		return 0f;
	}

	public override Snapshotter.Version DoVersion(Snapshotter sn)
	{
		base.DoVersion(sn);
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		Equipment = (ItemStorage)sn.DoISnapshot(Equipment);
		isHauling = sn.DoBool(isHauling);
		isHaulingIsDirty = sn.DoBool(isHaulingIsDirty);
		ItemStorage = (ItemStorage)sn.DoISnapshot(ItemStorage);
		mountedToolOrWeapon = sn.DoEntityIDNullable(mountedToolOrWeapon);
		parentBulkChangedID = sn.DoMethodIDNullable(parentBulkChangedID);
		Stomach = (ItemStorage)sn.DoISnapshot(Stomach);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		base.LoadPostProcess(sn);
		LoadPostProcessRegisterMethodIDs();
		ItemStorage.LoadPostProcess(sn);
		if (Stomach != null)
		{
			Stomach.LoadPostProcess(sn);
		}
		if (Equipment != null)
		{
			Equipment.LoadPostProcess(sn);
		}
	}

	public void LoadPostProcessRegisterMethodIDs()
	{
		if (parentBulkChangedID.HasValue)
		{
			ActionLookup<float>.Add(parentBulkChangedID.Value, parent_BulkChanged);
		}
	}
}
