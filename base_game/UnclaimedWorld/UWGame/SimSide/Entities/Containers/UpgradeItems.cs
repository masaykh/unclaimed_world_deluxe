using System;
using System.Collections.Generic;
using UWGame.SimSide.SimEffects;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities.Containers;

public class UpgradeItems : ISnapshot
{
	private List<EntityID> containedItems = new List<EntityID>();

	private Dictionary<UpgradeCategory, EntityID> containedUpgrades = new Dictionary<UpgradeCategory, EntityID>();

	public Entity Parent;

	private EntityID parentID;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public Dictionary<UpgradeCategory, EntityID> ContainedUpgrades => containedUpgrades;

	public bool IsSnapshotted { get; set; }

	public UpgradeItems()
	{
	}

	public UpgradeItems(Entity parent)
	{
		Parent = parent;
	}

	public void DestroyAllEntities()
	{
		for (int num = containedItems.Count - 1; num >= 0; num--)
		{
			EntityID entityID = containedItems[num];
			Entity entity = Entity.FindByID(entityID);
			if (entity != null)
			{
				if (Parent.Contains.Remove(entity))
				{
					entity.Destroy();
				}
			}
			else
			{
				containedItems.RemoveAt(num);
				RemoveUpgradeItem(entityID);
			}
		}
	}

	public bool Add(UpgradeCategory upgradeCategory, Entity entity)
	{
		if (entity.EntityType.Upgrader != null)
		{
			containedItems.Add(entity.ID);
			containedUpgrades[upgradeCategory] = entity.ID;
			if (Entity.IsFunctional(entity) && NonLivingEntity.IsCompleted(entity.Progress))
			{
				StartEffects(entity);
				SetSpriteModifier(entity.EntityType.Upgrader, set: true);
			}
			return true;
		}
		return false;
	}

	public void ApplyUpgradeEffects(Entity entity)
	{
		StartEffects(entity);
		SetSpriteModifier(entity.EntityType.Upgrader, set: true);
	}

	public void SetSpriteModifier(Upgrader upgrader, bool set)
	{
		if (upgrader.SpriteModifier.HasValue)
		{
			if (set)
			{
				Parent.SetSpriteStateFlag(upgrader.SpriteModifier.Value);
			}
			else
			{
				Parent.ClearSpriteStateFlag(upgrader.SpriteModifier.Value);
			}
		}
	}

	public void StartEffects(Entity entity)
	{
		if (entity.EntityType.Upgrader.EffectsFinal == null)
		{
			return;
		}
		foreach (EffectProfileType item in entity.EntityType.Upgrader.EffectsFinal)
		{
			Parent.SimEffects.Start(item);
		}
	}

	public void EndEffects(Entity item)
	{
		if (item.EntityType.Upgrader.EffectsFinal == null)
		{
			return;
		}
		foreach (EffectProfileType item2 in item.EntityType.Upgrader.EffectsFinal)
		{
			Parent.SimEffects.Remove(item2);
		}
	}

	public void IterateContained(Action<Entity> iterateMethod)
	{
		Garrison.IterateList(containedItems, iterateMethod);
	}

	public void GetContainedItemsList(Predicate<Entity> rule, List<Entity> items)
	{
		for (int num = containedItems.Count - 1; num >= 0; num--)
		{
			EntityID entityID = containedItems[num];
			Entity entity = Entity.FindByID(entityID);
			if (entity != null)
			{
				if (rule == null || rule(entity))
				{
					items.Add(entity);
				}
			}
			else
			{
				Remove(entityID);
			}
		}
	}

	public bool Contains(EntityID entityID)
	{
		return containedItems.Contains(entityID);
	}

	private void RemoveUpgradeItem(EntityID entityID)
	{
		UpgradeCategory upgradeCategory = GetUpgradeCategory(entityID);
		if (upgradeCategory != null)
		{
			containedUpgrades.Remove(upgradeCategory);
		}
	}

	public UpgradeCategory GetUpgradeCategory(EntityID entityID)
	{
		UpgradeCategory result = null;
		foreach (KeyValuePair<UpgradeCategory, EntityID> containedUpgrade in containedUpgrades)
		{
			if (containedUpgrade.Value == entityID)
			{
				result = containedUpgrade.Key;
				break;
			}
		}
		return result;
	}

	public bool Remove(EntityID entityID)
	{
		bool num = containedItems.Remove(entityID);
		RemoveUpgradeItem(entityID);
		if (num)
		{
			Entity entity = Entity.FindByID(entityID);
			EndEffects(entity);
			SetSpriteModifier(entity.EntityType.Upgrader, set: false);
		}
		return num;
	}

	public int NoOfItems()
	{
		return containedItems.Count;
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		containedItems = sn.DoList(containedItems);
		parentID = sn.SnapshotID<Entity, EntityID>(Parent).Value;
		containedUpgrades = sn.DoDictionary(containedUpgrades);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		Parent = Entity.FindByID(parentID);
	}
}
