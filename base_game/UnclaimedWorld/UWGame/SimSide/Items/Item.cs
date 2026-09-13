using System;
using System.Collections.Generic;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Items;

public class Item : Component, IIDEventSubscriber
{
	public Food Food;

	public Ammunition Ammunition;

	public bool? OKToTakeThisItemFromCarrier;

	private MethodID parentBulkChangedID;

	private EntityID? replenishes;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public EntityID? Replenishes
	{
		get
		{
			return GetRootAsItem()?.replenishes;
		}
		set
		{
			Item rootAsItem = GetRootAsItem();
			if (rootAsItem != null)
			{
				rootAsItem.replenishes = value.Value;
			}
		}
	}

	public void MergeItems(Entity entityToDestroy)
	{
		Parent.Find<NonLivingEntity>(out var c);
		entityToDestroy.Find<NonLivingEntity>(out var c2);
		if (entityToDestroy.Parts == null)
		{
			c.Condition = Common.Average(c.Condition, c2.Condition);
			c.MaxCondition = Common.Average(c.MaxCondition, c2.MaxCondition);
		}
		entityToDestroy.Destroy();
	}

	private Item GetRootAsItem()
	{
		if (Parent.GetRoot() is Entity entity && entity.Find<Item>(out var c))
		{
			return c;
		}
		return null;
	}

	public Item(Entity parent)
		: base(parent)
	{
		if (parent.EntityType.ItemType.FoodType != null)
		{
			Food = new Food(this);
		}
		if (parent.EntityType.ItemType.AmmunitionType != null)
		{
			Ammunition = new Ammunition
			{
				NoOfRounds = parent.EntityType.ItemType.AmmunitionType.MaxNoOfRounds
			};
		}
		parent.BulkChangedEvent.AddAndRegister((Action<float>)Parent_BulkChanged, (IIDEventSubscriber)this, out parentBulkChangedID);
	}

	private void Parent_BulkChanged(float oldValue)
	{
		if (Food != null)
		{
			Food.UpdateNutrientAmounts(Parent);
		}
	}

	public Item()
	{
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		Ammunition = (Ammunition)sn.DoISnapshot(Ammunition);
		Food = (Food)sn.DoISnapshot(Food);
		OKToTakeThisItemFromCarrier = sn.DoBoolNullable(OKToTakeThisItemFromCarrier);
		replenishes = sn.DoEnumNullable(replenishes);
		parentBulkChangedID = sn.DoMethodID(parentBulkChangedID);
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
		base.LoadPostProcess(sn);
		LoadPostProcessRegisterMethodIDs();
		if (Food != null)
		{
			Food.Parent = this;
			Food.LoadPostProcess(sn);
		}
		if (Ammunition != null)
		{
			Ammunition.LoadPostProcess(sn);
		}
	}

	public void LoadPostProcessRegisterMethodIDs()
	{
		ActionLookup<float>.Add(parentBulkChangedID, Parent_BulkChanged);
	}

	public void Initialize()
	{
		if (Parent.EntityType.ItemType.MaximumBulk.HasValue)
		{
			Parent.Bulk = Parent.EntityType.ItemType.MaximumBulk.Value;
		}
		if (Food != null)
		{
			Food.UpdateNutrientAmounts(Parent);
		}
	}

	public bool IsOwnedBySomebody()
	{
		return Parent.OwnedBy.HasValue;
	}

	private void AddItemToCollection(Dictionary<EntityType, List<Entity>> belongingCollection)
	{
		if (belongingCollection.ContainsKey(Parent.EntityType))
		{
			belongingCollection[Parent.EntityType].Add(Parent);
			return;
		}
		List<Entity> list = new List<Entity>();
		list.Add(Parent);
		belongingCollection.Add(Parent.EntityType, list);
	}

	public void Destroy()
	{
	}

	public bool Pickup(Entity entity, IOwner newOwner, StorageCompartment placeInCompartment)
	{
		bool flag;
		if (Parent.ContainedBy.HasValue)
		{
			Entity entity2 = Entity.FindByID(Parent.ContainedBy.Value);
			if (entity2 == null)
			{
				return false;
			}
			flag = entity2.Contains.Uncontain(Parent, destroy: false, shouldQueue: false, null, entity, placeInCompartment);
		}
		else
		{
			flag = entity.AgentStorage.AddToContain(Parent, placeInCompartment);
		}
		if (flag)
		{
			OKToTakeThisItemFromCarrier = false;
			entity.Intelligence.Allegiance.SharedKnowledge.SeeDetectableIfRelevant(Parent, testForUsesMemory: true, suppressClientFeedback: false, null, entity);
			if (newOwner != null)
			{
				Parent.ChangeOwnership(newOwner);
				HaulingJobManager.CreateHaulingJobsForAllCarriedItemsOutOfBand(entity, newOwner.OwnedEntities);
			}
			return true;
		}
		return false;
	}

	public static bool IsImmovable(float bulk)
	{
		return bulk > 1f;
	}

	private MachineBodyPart GetMachineBodyPart()
	{
		if (Parent.PartOf == null)
		{
			return null;
		}
		if (Parent.PartOf is MachineBodyPart result)
		{
			return result;
		}
		if (Parent.PartOf is Item item)
		{
			return item.GetMachineBodyPart();
		}
		return null;
	}

	public bool IsWeatherProof()
	{
		if (Parent.EntityType.NonLivingType.PartsAreWeatherProof)
		{
			return true;
		}
		if (Parent.PartOf == null)
		{
			return false;
		}
		if (Parent.PartOf is Item item)
		{
			return item.IsWeatherProof();
		}
		return false;
	}
}
