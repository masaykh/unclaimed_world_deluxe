using System.Collections.Generic;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities;

public class LookUpHasEntityGroup : ILookUpCollectible, ISnapshot
{
	private static Dictionary<HasEntityGroupID, EntityID> snapshotEntities = new Dictionary<HasEntityGroupID, EntityID>();

	private static Dictionary<HasEntityGroupID, ExpeditionID> snapshotExpeditions = new Dictionary<HasEntityGroupID, ExpeditionID>();

	private static Dictionary<HasEntityGroupID, HouseholdID> snapshotHouseholds = new Dictionary<HasEntityGroupID, HouseholdID>();

	private static Dictionary<HasEntityGroupID, AllegianceID> snapshotAllegiances = new Dictionary<HasEntityGroupID, AllegianceID>();

	private static Dictionary<HasEntityGroupID, IHasEntityGroup> collection;

	private static LookUpHasEntityGroup instance;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool SnapshotThis => true;

	public int LoadPostProcessOrder => 0;

	public bool IsSnapshotted { get; set; }

	public static void Create()
	{
		if (instance == null)
		{
			instance = new LookUpHasEntityGroup();
			Sim.AddLookupCollectible(typeof(IHasEntityGroup), instance);
			collection = new Dictionary<HasEntityGroupID, IHasEntityGroup>();
		}
	}

	public void ClearCollection()
	{
		snapshotExpeditions.Clear();
		snapshotEntities.Clear();
		snapshotHouseholds.Clear();
		snapshotAllegiances.Clear();
		collection.Clear();
	}

	public static IHasEntityGroup FindByID(HasEntityGroupID? id)
	{
		if (!id.HasValue)
		{
			return null;
		}
		collection.TryGetValue(id.Value, out var value);
		return value;
	}

	public static void Add(HasEntityGroupID ownerID, IHasEntityGroup owner)
	{
		collection.Add(ownerID, owner);
	}

	public static void Remove(IHasEntityGroup owner)
	{
		collection.Remove(owner.ID);
		owner.SetInvalid();
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		if (sn.mode != Snapshotter.Mode.Load)
		{
			foreach (KeyValuePair<HasEntityGroupID, IHasEntityGroup> item in collection)
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
				else if (item.Value is Allegiance)
				{
					snapshotAllegiances.Add(item.Key, ((Allegiance)item.Value).ID);
				}
			}
		}
		snapshotExpeditions = sn.DoDictionary(snapshotExpeditions);
		snapshotEntities = sn.DoDictionary(snapshotEntities);
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
		foreach (KeyValuePair<HasEntityGroupID, ExpeditionID> snapshotExpedition in snapshotExpeditions)
		{
			collection.Add(snapshotExpedition.Key, LookUp<Expedition, ExpeditionID>.FindByID(snapshotExpedition.Value));
		}
		foreach (KeyValuePair<HasEntityGroupID, HouseholdID> snapshotHousehold in snapshotHouseholds)
		{
			collection.Add(snapshotHousehold.Key, LookUp<Household, HouseholdID>.FindByID(snapshotHousehold.Value));
		}
		foreach (KeyValuePair<HasEntityGroupID, EntityID> snapshotEntity in snapshotEntities)
		{
			collection.Add(snapshotEntity.Key, Entity.FindByID(snapshotEntity.Value).PersonEntity);
		}
		foreach (KeyValuePair<HasEntityGroupID, AllegianceID> snapshotAllegiance in snapshotAllegiances)
		{
			collection.Add(snapshotAllegiance.Key, LookUp<Allegiance, AllegianceID>.FindByID(snapshotAllegiance.Value));
		}
		snapshotExpeditions.Clear();
		snapshotEntities.Clear();
		snapshotHouseholds.Clear();
		snapshotAllegiances.Clear();
	}
}
