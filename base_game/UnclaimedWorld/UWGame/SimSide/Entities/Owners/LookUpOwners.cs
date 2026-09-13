using System.Collections.Generic;
using UWGame.SimSide.AI;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities.Owners;

public class LookUpOwners : ILookUpCollectible, ISnapshot
{
	private static Dictionary<OwnerID, EntityID> snapshotEntities = new Dictionary<OwnerID, EntityID>();

	private static Dictionary<OwnerID, ExpeditionID> snapshotExpeditions = new Dictionary<OwnerID, ExpeditionID>();

	private static Dictionary<OwnerID, HouseholdID> snapshotHouseholds = new Dictionary<OwnerID, HouseholdID>();

	private static Dictionary<OwnerID, IOwner> collection;

	private static LookUpOwners instance;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool SnapshotThis => true;

	public int LoadPostProcessOrder => 0;

	public bool PerformSnapshot => true;

	public bool IsSnapshotted { get; set; }

	public static void Create()
	{
		if (instance == null)
		{
			instance = new LookUpOwners();
			Sim.AddLookupCollectible(typeof(IOwner), instance);
			collection = new Dictionary<OwnerID, IOwner>();
		}
	}

	public void ClearCollection()
	{
		snapshotExpeditions.Clear();
		snapshotEntities.Clear();
		snapshotHouseholds.Clear();
		collection.Clear();
	}

	public static IOwner FindByID(OwnerID? id)
	{
		if (!id.HasValue)
		{
			return null;
		}
		collection.TryGetValue(id.Value, out var value);
		return value;
	}

	public static void Remove(IOwner owner)
	{
		collection.Remove(owner.ID);
		owner.SetInvalid();
	}

	public static void Add(OwnerID ownerID, IOwner owner)
	{
		collection.Add(ownerID, owner);
	}

	public static bool ResolveEntityOwner(IKnownEntityData entityData, out IOwner owner)
	{
		owner = null;
		if (entityData.OwnedBy.HasValue)
		{
			owner = FindByID(entityData.OwnedBy);
			if (owner == null)
			{
				entityData.OwnedBy = null;
				return false;
			}
		}
		return true;
	}

	public static bool ResolveEntityOwner(IKnownEntityData entityData, out EntityGroup ownedEntities)
	{
		ownedEntities = null;
		if (entityData.OwnedBy.HasValue)
		{
			IOwner owner = FindByID(entityData.OwnedBy);
			if (owner == null)
			{
				entityData.OwnedBy = null;
				return false;
			}
			ownedEntities = owner.OwnedEntities;
		}
		return true;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		if (sn.mode != Snapshotter.Mode.Load)
		{
			foreach (KeyValuePair<OwnerID, IOwner> item in collection)
			{
				if (item.Value is Household)
				{
					snapshotHouseholds.Add(item.Key, ((Household)item.Value).ID);
				}
				else if (item.Value is Person)
				{
					snapshotEntities.Add(item.Key, ((Person)item.Value).Parent.ID);
				}
				else if (item.Value is Expedition)
				{
					snapshotExpeditions.Add(item.Key, ((Expedition)item.Value).ID);
				}
			}
		}
		snapshotExpeditions = sn.DoDictionary(snapshotExpeditions);
		snapshotEntities = sn.DoDictionary(snapshotEntities);
		snapshotHouseholds = sn.DoDictionary(snapshotHouseholds);
		sn.Ignore(collection);
		sn.Ignore(instance);
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
		collection.Clear();
		foreach (KeyValuePair<OwnerID, ExpeditionID> snapshotExpedition in snapshotExpeditions)
		{
			collection.Add(snapshotExpedition.Key, LookUp<Expedition, ExpeditionID>.FindByID(snapshotExpedition.Value));
		}
		foreach (KeyValuePair<OwnerID, HouseholdID> snapshotHousehold in snapshotHouseholds)
		{
			collection.Add(snapshotHousehold.Key, LookUp<Household, HouseholdID>.FindByID(snapshotHousehold.Value));
		}
		foreach (KeyValuePair<OwnerID, EntityID> snapshotEntity in snapshotEntities)
		{
			collection.Add(snapshotEntity.Key, Entity.FindByID(snapshotEntity.Value).PersonEntity);
		}
		snapshotExpeditions.Clear();
		snapshotEntities.Clear();
		snapshotHouseholds.Clear();
	}
}
