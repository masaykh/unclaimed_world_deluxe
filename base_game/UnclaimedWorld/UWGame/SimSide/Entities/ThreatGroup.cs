using System;
using System.Linq;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities;

public class ThreatGroup : ISnapshot, ILookUp<ThreatGroup, ThreatGroupID>
{
	public string Name;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	private ThreatGroupID id = ThreatGroupID.Invalid;

	private static ThreatGroupID IDCounter = ThreatGroupID.First;

	public bool IsSnapshotted { get; set; }

	public ThreatGroupID ID
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

	public ThreatGroup(string threatGroupName)
	{
		AddToLookup();
		Name = threatGroupName;
		if (!string.IsNullOrEmpty(Name))
		{
			The.Sim.PlaySite.ThreatGroups.Add(this);
		}
	}

	public ThreatGroup()
	{
	}

	public static ThreatGroup GetThreatGroup(string threatGroupName)
	{
		ThreatGroup threatGroup;
		if (!string.IsNullOrEmpty(threatGroupName))
		{
			threatGroup = The.Sim.PlaySite.ThreatGroups.FirstOrDefault((ThreatGroup t) => t.Name == threatGroupName);
			if (threatGroup == null)
			{
				threatGroup = new ThreatGroup(threatGroupName);
			}
		}
		else
		{
			threatGroup = new ThreatGroup(null);
		}
		return threatGroup;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		id = sn.DoEnum(id);
		IDCounter = sn.DoEnum(IDCounter);
		Name = sn.DoString(Name);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ThreatGroupID GetUniqueID()
	{
		IDCounter++;
		if (IDCounter >= ThreatGroupID.Invalid)
		{
			throw new Exception("Astounding, ThreatGroupID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public ThreatGroupID SnapshotID(Snapshotter sn, ThreatGroupID id)
	{
		return sn.DoEnum(id);
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != ThreatGroupID.Invalid)
		{
			LookUp<ThreatGroup, ThreatGroupID>.Add(ID, this);
		}
	}

	public void SetInvalid()
	{
		id = ThreatGroupID.Invalid;
	}

	public void RemoveIDEntry()
	{
		LookUp<ThreatGroup, ThreatGroupID>.Remove(this);
	}

	void ILookUp<ThreatGroup, ThreatGroupID>.ResetIDCounter()
	{
	}

	public static void ResetIDCounter()
	{
		IDCounter = ThreatGroupID.First;
	}

	void ILookUp<ThreatGroup, ThreatGroupID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUp<ThreatGroup, ThreatGroupID>.Create();
	}
}
