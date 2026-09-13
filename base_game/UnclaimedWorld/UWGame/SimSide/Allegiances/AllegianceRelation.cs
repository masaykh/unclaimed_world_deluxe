using System;
using System.Collections.Generic;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Allegiances;

public class AllegianceRelation : ISnapshot, ILookUp<AllegianceRelation, AllegianceRelationID>
{
	public Allegiance AllegianceA;

	private AllegianceID snapshotA;

	public Allegiance AllegianceB;

	private AllegianceID snapshotB;

	public float Relation;

	public bool AcceptImigration;

	public bool AllowTrade;

	public bool AllowPassage;

	private AllegianceRelationID id = AllegianceRelationID.Invalid;

	private static AllegianceRelationID IDCounter = AllegianceRelationID.First;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public AllegianceRelationID ID
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

	public AllegianceRelation()
	{
		if (!Snapshotter.IsSnapshotting)
		{
			AddToLookup();
		}
	}

	public static AllegianceRelation CreateRelationFromData(AllegianceRelationData allegianceRelationData)
	{
		AllegianceRelation allegianceRelation = new AllegianceRelation
		{
			AllegianceA = The.Sim.World.GetAllegianceFromKey(allegianceRelationData.Allegiance1),
			AllegianceB = The.Sim.World.GetAllegianceFromKey(allegianceRelationData.Allegiance2),
			Relation = allegianceRelationData.Relation
		};
		if (allegianceRelation.AllegianceA == null || allegianceRelation.AllegianceB == null)
		{
			return null;
		}
		if (The.Sim.World.GetRelationBetweenAllegianceIDs(allegianceRelation.AllegianceA.ID, allegianceRelation.AllegianceB.ID) != null)
		{
			return null;
		}
		List<AllegianceRelation> value = new List<AllegianceRelation>();
		The.Sim.World.Relations.TryGetValue(allegianceRelation.AllegianceA.ID, out value);
		if (value == null)
		{
			value = new List<AllegianceRelation>();
		}
		value.Add(allegianceRelation);
		The.Sim.World.Relations.Add(allegianceRelation.AllegianceA.ID, value);
		The.Sim.World.Relations.TryGetValue(allegianceRelation.AllegianceB.ID, out value);
		if (value == null)
		{
			value = new List<AllegianceRelation>();
		}
		value.Add(allegianceRelation);
		The.Sim.World.Relations.Add(allegianceRelation.AllegianceB.ID, value);
		return allegianceRelation;
	}

	public void Destroy()
	{
		RemoveIDEntry();
	}

	public AllegianceRelationID GetUniqueID()
	{
		IDCounter++;
		if ((ulong)IDCounter >= ulong.MaxValue)
		{
			throw new Exception("Astounding, AllegianceRelationID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public AllegianceRelationID SnapshotID(Snapshotter sn, AllegianceRelationID id)
	{
		return sn.DoEnum(id);
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != AllegianceRelationID.Invalid)
		{
			LookUp<AllegianceRelation, AllegianceRelationID>.Add(ID, this);
		}
	}

	public void SetInvalid()
	{
		id = AllegianceRelationID.Invalid;
	}

	public void RemoveIDEntry()
	{
		LookUp<AllegianceRelation, AllegianceRelationID>.Remove(this);
	}

	void ILookUp<AllegianceRelation, AllegianceRelationID>.ResetIDCounter()
	{
	}

	public static void ResetIDCounter()
	{
		IDCounter = AllegianceRelationID.First;
	}

	void ILookUp<AllegianceRelation, AllegianceRelationID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUp<AllegianceRelation, AllegianceRelationID>.Create();
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		id = SnapshotID(sn, id);
		IDCounter = sn.DoEnum(IDCounter);
		snapshotA = sn.SnapshotID<Allegiance, AllegianceID>(AllegianceA).Value;
		snapshotB = sn.SnapshotID<Allegiance, AllegianceID>(AllegianceB).Value;
		Relation = sn.DoFloat(Relation);
		AcceptImigration = sn.DoBool(AcceptImigration);
		AllowPassage = sn.DoBool(AllowPassage);
		AllowTrade = sn.DoBool(AllowTrade);
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
		AllegianceA = LookUp<Allegiance, AllegianceID>.FindByID(snapshotA);
		AllegianceB = LookUp<Allegiance, AllegianceID>.FindByID(snapshotB);
	}
}
