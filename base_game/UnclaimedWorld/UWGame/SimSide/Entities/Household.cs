using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities;

public class Household : IHasEntityGroup, ILookUp<IHasEntityGroup, HasEntityGroupID>, IOwner, ILookUp<IOwner, OwnerID>, ICanIterateEntities, ILookUp<ICanIterateEntities, CanIterateEntitiesID>, ILookUp<Household, HouseholdID>, ISnapshot
{
	private List<Entity> members = new List<Entity>();

	private List<EntityID> snapshotMembers = new List<EntityID>();

	public Entity HeadOfHousehold1;

	public Entity HeadOfHousehold2;

	private EntityID? snapshotHead1;

	private EntityID? snapshotHead2;

	public FoodExtraction FoodExtraction;

	public int TotalAssignedFood;

	public Dictionary<EntityType, int> FoodAssignedToday = new Dictionary<EntityType, int>();

	private EntityID? home;

	private EntityGroup ownedEntities;

	private EntityGroupID snapshotOwnedEntities;

	private decimal? tradeCredits = default(decimal);

	private HouseholdID id = HouseholdID.Invalid;

	private static HouseholdID IDCounter = HouseholdID.First;

	private CanIterateEntitiesID canIterateEntitiesID;

	private HasEntityGroupID hasEntityGroupID;

	private OwnerID ownerID;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public Expedition Expedition
	{
		get
		{
			if (HeadOfHousehold1 != null)
			{
				return HeadOfHousehold1.Intelligence.CurrentExpedition;
			}
			return null;
		}
	}

	public EntityID? Home
	{
		get
		{
			return home;
		}
		set
		{
			if (home != value)
			{
				home = value;
				if (Allegiance != null && !Residence.UpdateResidentsNo(Allegiance.SharedKnowledge, home))
				{
					home = null;
				}
			}
		}
	}

	public EntityGroup OwnedEntities => ownedEntities;

	public decimal? TradeCredits
	{
		get
		{
			return tradeCredits;
		}
		set
		{
			tradeCredits = value;
		}
	}

	public int NoOfMembers => members.Count;

	public int NoOfWorkers => members.Count;

	public Allegiance GetAllegiance => Allegiance;

	public Allegiance Allegiance
	{
		get
		{
			if (Expedition != null)
			{
				return Expedition.Allegiance;
			}
			return null;
		}
	}

	Vector3? IHasEntityGroup.Location => Location;

	public Vector3? Location
	{
		get
		{
			if (Home.HasValue)
			{
				IKnownEntityData knownEntityData = ResolveHome();
				if (knownEntityData != null)
				{
					return knownEntityData.Location;
				}
			}
			return HeadOfHousehold1.Location;
		}
	}

	public HouseholdID ID
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

	CanIterateEntitiesID ILookUp<ICanIterateEntities, CanIterateEntitiesID>.ID => canIterateEntitiesID;

	HasEntityGroupID ILookUp<IHasEntityGroup, HasEntityGroupID>.ID => hasEntityGroupID;

	OwnerID ILookUp<IOwner, OwnerID>.ID => ownerID;

	public bool IsSnapshotted { get; set; }

	public OwnerID? GetOwnerID()
	{
		OwnerID? result = null;
		if (this != null)
		{
			result = ((ILookUp<IOwner, OwnerID>)this).ID;
		}
		return result;
	}

	public void IterateMembers(Action<Entity> iterateFunction)
	{
		foreach (Entity member in members)
		{
			iterateFunction(member);
		}
	}

	public void IterateOwnedItems(Action<EntityGroup> iterateFunction)
	{
		iterateFunction(ownedEntities);
	}

	public bool IsEatable(EntityType entityType)
	{
		return FoodExtraction.IsEatable(entityType);
	}

	public IKnownEntityData ResolveHome()
	{
		if (Home.HasValue)
		{
			if (!GoalEvaluator.EntityDataResultCausesSkip(Allegiance.SharedKnowledge.GetKnownData(Home.Value, out var data)))
			{
				return data;
			}
			Home = null;
		}
		return null;
	}

	public bool SetHome(EntityID home)
	{
		if (Residence.AddHousehold(Allegiance.SharedKnowledge, this, home))
		{
			if (Home.HasValue)
			{
				Residence.RemoveHousehold(Allegiance.SharedKnowledge, this);
			}
			Home = home;
			return true;
		}
		return false;
	}

	private void FillNewHome()
	{
	}

	public int NoOfChildren()
	{
		int num = 0;
		foreach (Entity member in members)
		{
			if (!member.PersonEntity.IsHeadOfHousehold() && member.PersonEntity.IsChild())
			{
				num++;
			}
		}
		return num;
	}

	public Household()
	{
	}

	public Household(Expedition expedition, Entity member)
	{
		AddToLookup();
		((ILookUp<ICanIterateEntities, CanIterateEntitiesID>)this).AddToLookup();
		((ILookUp<IHasEntityGroup, HasEntityGroupID>)this).AddToLookup();
		((ILookUp<IOwner, OwnerID>)this).AddToLookup();
		ownedEntities = new EntityGroup(this, manageTrade: true, manageProduction: true);
		ownedEntities.HaulingJobManager = new HaulingJobManager(ownedEntities);
		ownedEntities.OtherJobManager = new OtherJobManager(ownedEntities);
		FoodExtraction = new FoodExtraction(this, ownedEntities.ID);
		AddMember(member);
		expedition?.AddHousehold(this);
	}

	public void Destroy(Entity lastMember)
	{
		Allegiance allegiance = lastMember.Intelligence.Allegiance;
		Expedition currentExpedition = lastMember.Intelligence.CurrentExpedition;
		List<IOwner> list = new List<IOwner>();
		if (currentExpedition != null)
		{
			list.Add(currentExpedition);
		}
		Person.DivideItems(OwnedEntities, list);
		Residence.RemoveHousehold(allegiance.SharedKnowledge, this);
		currentExpedition?.RemoveHousehold(this);
		ownedEntities.Destroy();
		RemoveIDEntry();
		((ILookUp<ICanIterateEntities, CanIterateEntitiesID>)this).RemoveIDEntry();
		((ILookUp<IHasEntityGroup, HasEntityGroupID>)this).RemoveIDEntry();
		((ILookUp<IOwner, OwnerID>)this).RemoveIDEntry();
	}

	private void FoodExtraction_FoodProcessesChanged()
	{
		ownedEntities.SetFoodDirty();
	}

	public void AddMember(Entity entity)
	{
		members.Add(entity);
		UpdateWhenMembersChanged();
	}

	private void Members_ListItemAdded(object sender)
	{
		AssignHeadsOfHousehold();
	}

	private void Members_ListItemRemoved(object sender, int indexOfRemovedItem)
	{
		AssignHeadsOfHousehold();
	}

	public bool IsEntitled()
	{
		return true;
	}

	private void AssignHeadsOfHousehold()
	{
		Entity entity = null;
		Entity headOfHousehold = null;
		float num = 0f;
		float num2 = 0f;
		foreach (Entity member in members)
		{
			float num3 = ScoreMemberAsHeadOfHousehold(member);
			if (num3 > num)
			{
				headOfHousehold = entity;
				num2 = num;
				entity = member;
				num = num3;
			}
			else if (num3 > num2)
			{
				headOfHousehold = member;
				num2 = num3;
			}
		}
		HeadOfHousehold1 = entity;
		HeadOfHousehold2 = headOfHousehold;
	}

	private float ScoreMemberAsHeadOfHousehold(Entity member)
	{
		float num = ((member.BiologicalEntity.CasteType.Reproduction != Reproduction.Male) ? 0.7f : 1f);
		float num2 = 1f;
		if (member.BiologicalEntity.AgeGroup.AgeGroupType.AIAgeGroup == AIAgeGroup.Adult)
		{
			num2 = 3f;
		}
		float num3 = num2 * Common.ClampTop(member.BiologicalEntity.AgeGroup.Age / member.BiologicalEntity.CasteType.MaxAge, 1f);
		return 0.3f * num + 0.7f * num3;
	}

	public void MergeHouseholds(Household householdToDissappear)
	{
	}

	public void SplitHouseholds(List<Entity> membersOfNewHousehold, bool splitItems, bool splitVehicles, bool splitBuildings)
	{
		if (membersOfNewHousehold.Count >= members.Count)
		{
			throw new Exception("Illegal split. Household would be destroyed...");
		}
	}

	public void RemoveMember(Entity member)
	{
		if (members.Contains(member))
		{
			members.Remove(member);
			UpdateWhenMembersChanged();
			if (members.Count == 0)
			{
				member.PersonEntity.GetHeirsAsOwners(out var _, out var owners);
				Person.DivideItems(ownedEntities, owners);
				Destroy(member);
			}
		}
	}

	private void UpdateWhenMembersChanged()
	{
		AssignHeadsOfHousehold();
		if (Home.HasValue && Allegiance != null && !Residence.UpdateResidentsNo(Allegiance.SharedKnowledge, Home))
		{
			Home = null;
		}
		Allegiance.HandleGroupMembersChanged(FoodExtraction);
		OwnedEntities.RecomputeFoodConsumeRate();
	}

	public HouseholdID GetUniqueID()
	{
		IDCounter++;
		if ((ulong)IDCounter >= ulong.MaxValue)
		{
			throw new Exception("Astounding, HouseholdID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public HouseholdID SnapshotID(Snapshotter sn, HouseholdID id)
	{
		return sn.DoEnum(id);
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != HouseholdID.Invalid)
		{
			LookUp<Household, HouseholdID>.Add(ID, this);
		}
	}

	public void SetInvalid()
	{
		id = HouseholdID.Invalid;
	}

	public void RemoveIDEntry()
	{
		LookUp<Household, HouseholdID>.Remove(this);
	}

	void ILookUp<Household, HouseholdID>.ResetIDCounter()
	{
	}

	public static void ResetIDCounter()
	{
		IDCounter = HouseholdID.First;
	}

	void ILookUp<Household, HouseholdID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUp<Household, HouseholdID>.Create();
	}

	CanIterateEntitiesID ILookUp<ICanIterateEntities, CanIterateEntitiesID>.GetUniqueID()
	{
		return HasMembers.GetUniqueID();
	}

	void ILookUp<ICanIterateEntities, CanIterateEntitiesID>.AddToLookup()
	{
		canIterateEntitiesID = ((ILookUp<ICanIterateEntities, CanIterateEntitiesID>)this).GetUniqueID();
		if (canIterateEntitiesID != CanIterateEntitiesID.Invalid)
		{
			LookUpICanIterateEntities.Add(canIterateEntitiesID, this);
		}
	}

	void ILookUp<ICanIterateEntities, CanIterateEntitiesID>.RemoveIDEntry()
	{
		LookUpICanIterateEntities.Remove(this);
	}

	void ILookUp<ICanIterateEntities, CanIterateEntitiesID>.ResetIDCounter()
	{
	}

	void ILookUp<ICanIterateEntities, CanIterateEntitiesID>.SetInvalid()
	{
		canIterateEntitiesID = CanIterateEntitiesID.Invalid;
	}

	void ILookUp<ICanIterateEntities, CanIterateEntitiesID>.CreateLookupCollection()
	{
	}

	HasEntityGroupID ILookUp<IHasEntityGroup, HasEntityGroupID>.GetUniqueID()
	{
		return HasEntityGroup.GetUniqueID();
	}

	void ILookUp<IHasEntityGroup, HasEntityGroupID>.AddToLookup()
	{
		hasEntityGroupID = ((ILookUp<IHasEntityGroup, HasEntityGroupID>)this).GetUniqueID();
		if (hasEntityGroupID != HasEntityGroupID.Invalid)
		{
			LookUpHasEntityGroup.Add(hasEntityGroupID, this);
		}
	}

	void ILookUp<IHasEntityGroup, HasEntityGroupID>.RemoveIDEntry()
	{
		LookUpHasEntityGroup.Remove(this);
	}

	void ILookUp<IHasEntityGroup, HasEntityGroupID>.ResetIDCounter()
	{
	}

	void ILookUp<IHasEntityGroup, HasEntityGroupID>.SetInvalid()
	{
		hasEntityGroupID = HasEntityGroupID.Invalid;
	}

	void ILookUp<IHasEntityGroup, HasEntityGroupID>.CreateLookupCollection()
	{
	}

	OwnerID ILookUp<IOwner, OwnerID>.GetUniqueID()
	{
		return Owner.GetUniqueID();
	}

	void ILookUp<IOwner, OwnerID>.AddToLookup()
	{
		ownerID = ((ILookUp<IOwner, OwnerID>)this).GetUniqueID();
		if (ownerID != OwnerID.Invalid)
		{
			LookUpOwners.Add(ownerID, this);
		}
	}

	void ILookUp<IOwner, OwnerID>.RemoveIDEntry()
	{
		LookUpOwners.Remove(this);
	}

	void ILookUp<IOwner, OwnerID>.ResetIDCounter()
	{
	}

	void ILookUp<IOwner, OwnerID>.SetInvalid()
	{
		ownerID = OwnerID.Invalid;
	}

	void ILookUp<IOwner, OwnerID>.CreateLookupCollection()
	{
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		id = SnapshotID(sn, id);
		IDCounter = sn.DoEnum(IDCounter);
		hasEntityGroupID = sn.DoEnum(hasEntityGroupID);
		canIterateEntitiesID = sn.DoEnum(canIterateEntitiesID);
		ownerID = sn.DoEnum(ownerID);
		FoodExtraction = (FoodExtraction)sn.DoISnapshot(FoodExtraction);
		snapshotHead1 = sn.SnapshotID<Entity, EntityID>(HeadOfHousehold1);
		snapshotHead2 = sn.SnapshotID<Entity, EntityID>(HeadOfHousehold2);
		snapshotOwnedEntities = sn.SnapshotID<EntityGroup, EntityGroupID>(ownedEntities).Value;
		Home = sn.DoEntityIDNullable(Home);
		tradeCredits = sn.DoDecimalNullable(tradeCredits);
		if (sn.mode != Snapshotter.Mode.Load)
		{
			snapshotMembers = members.Select((Entity e) => e.ID).ToList();
		}
		snapshotMembers = sn.DoList(snapshotMembers);
		sn.Ignore(FoodAssignedToday);
		sn.Ignore(TotalAssignedFood);
		sn.Ignore(members);
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
		members = snapshotMembers.Select((EntityID e) => Entity.FindByID(e)).ToList();
		HeadOfHousehold1 = Entity.FindByID(snapshotHead1);
		HeadOfHousehold2 = Entity.FindByID(snapshotHead2);
		ownedEntities = LookUp<EntityGroup, EntityGroupID>.FindByID(snapshotOwnedEntities);
		FoodExtraction.LoadPostProcess(sn);
	}
}
