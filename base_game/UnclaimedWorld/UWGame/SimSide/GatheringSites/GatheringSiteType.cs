using System;
using System.ComponentModel;
using System.Xml.Serialization;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.GatheringSites;

public class GatheringSiteType : ISnapshot, ILookUp<GatheringSiteType, GatheringSiteTypeID>
{
	public Arc arc;

	public int MaxVisitors = 99;

	public float SeatSize = 25f;

	private GatheringSiteTypeID id = GatheringSiteTypeID.Invalid;

	private static GatheringSiteTypeID IDCounter;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	[XmlIgnore]
	public GatheringSiteTypeID ID
	{
		get
		{
			return id;
		}
		set
		{
			id = value;
		}
	}

	[XmlElement("ID")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Browsable(false)]
	public long IDLong
	{
		get
		{
			return (long)ID;
		}
		set
		{
			ID = (GatheringSiteTypeID)value;
		}
	}

	public int LoadPostProcessOrder => 0;

	public bool IsSnapshotted { get; set; }

	public GatheringSiteType()
	{
		if (!Snapshotter.IsSnapshotting)
		{
			AddToLookup();
		}
	}

	public GatheringSiteTypeID GetUniqueID()
	{
		IDCounter++;
		if (IDCounter >= GatheringSiteTypeID.Invalid)
		{
			throw new Exception("Astounding, GatheringSiteTypeID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public GatheringSiteTypeID SnapshotID(Snapshotter sn, GatheringSiteTypeID id)
	{
		return sn.DoEnum(id);
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != GatheringSiteTypeID.Invalid)
		{
			LookUp<GatheringSiteType, GatheringSiteTypeID>.Add(ID, this);
		}
	}

	public void SetInvalid()
	{
		id = GatheringSiteTypeID.Invalid;
	}

	public void RemoveIDEntry()
	{
		LookUp<GatheringSiteType, GatheringSiteTypeID>.Remove(this);
	}

	void ILookUp<GatheringSiteType, GatheringSiteTypeID>.ResetIDCounter()
	{
	}

	public static void ResetIDCounter()
	{
		IDCounter = GatheringSiteTypeID.First;
	}

	void ILookUp<GatheringSiteType, GatheringSiteTypeID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUp<GatheringSiteType, GatheringSiteTypeID>.Create();
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		id = SnapshotID(sn, id);
		IDCounter = sn.DoEnum(IDCounter);
		arc = (Arc)sn.DoISnapshot(arc);
		MaxVisitors = sn.DoInt32(MaxVisitors);
		SeatSize = sn.DoFloat(SeatSize);
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
		if (arc != null)
		{
			arc.LoadPostProcess(sn);
		}
	}
}
