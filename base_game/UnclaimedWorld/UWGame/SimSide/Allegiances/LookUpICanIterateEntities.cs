using System.Collections.Generic;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Allegiances;

public class LookUpICanIterateEntities : ILookUpCollectible, ISnapshot
{
	private static Dictionary<CanIterateEntitiesID, AllegianceID> snapshotAllegiances = new Dictionary<CanIterateEntitiesID, AllegianceID>();

	private static Dictionary<CanIterateEntitiesID, HouseholdID> snapshotHouseholds = new Dictionary<CanIterateEntitiesID, HouseholdID>();

	private static Dictionary<CanIterateEntitiesID, ExpeditionID> snapshotExpeditions = new Dictionary<CanIterateEntitiesID, ExpeditionID>();

	private static Dictionary<CanIterateEntitiesID, EntityID> snapshotAgents = new Dictionary<CanIterateEntitiesID, EntityID>();

	private static Dictionary<CanIterateEntitiesID, ICanIterateEntities> collection;

	private static LookUpICanIterateEntities instance;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool SnapshotThis => true;

	public int LoadPostProcessOrder => 0;

	public bool IsSnapshotted { get; set; }

	public static void Create()
	{
		if (instance == null)
		{
			instance = new LookUpICanIterateEntities();
			Sim.AddLookupCollectible(typeof(ICanIterateEntities), instance);
			collection = new Dictionary<CanIterateEntitiesID, ICanIterateEntities>();
		}
	}

	public void ClearCollection()
	{
		snapshotHouseholds.Clear();
		snapshotAllegiances.Clear();
		snapshotExpeditions.Clear();
		snapshotAgents.Clear();
		collection.Clear();
	}

	public static ICanIterateEntities FindByID(CanIterateEntitiesID id)
	{
		collection.TryGetValue(id, out var value);
		return value;
	}

	public static void Remove(ICanIterateEntities hasMembers)
	{
		collection.Remove(hasMembers.ID);
		hasMembers.SetInvalid();
	}

	public static void Add(CanIterateEntitiesID hasMembersID, ICanIterateEntities canIterate)
	{
		collection.Add(hasMembersID, canIterate);
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		if (sn.mode != Snapshotter.Mode.Load)
		{
			foreach (KeyValuePair<CanIterateEntitiesID, ICanIterateEntities> item in collection)
			{
				if (item.Value is Household)
				{
					snapshotHouseholds.Add(item.Key, ((Household)item.Value).ID);
				}
				else if (item.Value is Entity)
				{
					snapshotAgents.Add(item.Key, ((Entity)item.Value).ID);
				}
				else if (item.Value is Expedition)
				{
					snapshotExpeditions.Add(item.Key, ((Expedition)item.Value).ID);
				}
				else if (item.Value is Allegiance)
				{
					snapshotAllegiances.Add(item.Key, ((Allegiance)item.Value).ID);
				}
			}
		}
		snapshotExpeditions = sn.DoDictionary(snapshotExpeditions);
		snapshotAgents = sn.DoDictionary(snapshotAgents);
		snapshotHouseholds = sn.DoDictionary(snapshotHouseholds);
		snapshotAllegiances = sn.DoDictionary(snapshotAllegiances);
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
		foreach (KeyValuePair<CanIterateEntitiesID, AllegianceID> snapshotAllegiance in snapshotAllegiances)
		{
			collection.Add(snapshotAllegiance.Key, LookUp<Allegiance, AllegianceID>.FindByID(snapshotAllegiance.Value));
		}
		foreach (KeyValuePair<CanIterateEntitiesID, ExpeditionID> snapshotExpedition in snapshotExpeditions)
		{
			collection.Add(snapshotExpedition.Key, LookUp<Expedition, ExpeditionID>.FindByID(snapshotExpedition.Value));
		}
		foreach (KeyValuePair<CanIterateEntitiesID, HouseholdID> snapshotHousehold in snapshotHouseholds)
		{
			collection.Add(snapshotHousehold.Key, LookUp<Household, HouseholdID>.FindByID(snapshotHousehold.Value));
		}
		foreach (KeyValuePair<CanIterateEntitiesID, EntityID> snapshotAgent in snapshotAgents)
		{
			collection.Add(snapshotAgent.Key, Entity.FindByID(snapshotAgent.Value));
		}
		snapshotAllegiances.Clear();
		snapshotExpeditions.Clear();
		snapshotAgents.Clear();
		snapshotHouseholds.Clear();
	}
}
