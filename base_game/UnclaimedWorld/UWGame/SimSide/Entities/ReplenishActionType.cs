using System;
using System.ComponentModel;
using System.Xml.Serialization;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities;

public class ReplenishActionType : ILookUp<ReplenishActionType, ReplenishActionTypeID>
{
	public GoalReplenish.ReplenishAction ReplenishAction;

	public string AgentAction;

	[XmlIgnore]
	public ProcessType AgentActionType;

	private ReplenishActionTypeID id = ReplenishActionTypeID.Invalid;

	private static ReplenishActionTypeID IDCounter = ReplenishActionTypeID.First;

	[XmlIgnore]
	public ReplenishActionTypeID ID
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
			ID = (ReplenishActionTypeID)value;
		}
	}

	public int LoadPostProcessOrder => 0;

	public ReplenishActionType()
	{
		if (!Snapshotter.IsSnapshotting)
		{
			AddToLookup();
		}
	}

	public void PostLoadContentInitialize()
	{
		if (AgentAction != null)
		{
			AgentActionType = GameData.Instance.AllProcessTypes[AgentAction];
		}
	}

	public ReplenishActionTypeID GetUniqueID()
	{
		IDCounter++;
		if (IDCounter >= ReplenishActionTypeID.Invalid)
		{
			throw new Exception("Astounding, ReplenishActionTypeID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public ReplenishActionTypeID SnapshotID(Snapshotter sn, ReplenishActionTypeID id)
	{
		return sn.DoEnum(id);
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != ReplenishActionTypeID.Invalid)
		{
			LookUp<ReplenishActionType, ReplenishActionTypeID>.Add(ID, this);
			LookUp<ReplenishActionType, ReplenishActionTypeID>.SetPerformSnapshot(value: false);
		}
	}

	public void RemoveIDEntry()
	{
		LookUp<ReplenishActionType, ReplenishActionTypeID>.Remove(this);
	}

	void ILookUp<ReplenishActionType, ReplenishActionTypeID>.ResetIDCounter()
	{
	}

	public static void ResetIDCounter()
	{
		IDCounter = ReplenishActionTypeID.First;
	}

	void ILookUp<ReplenishActionType, ReplenishActionTypeID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUp<ReplenishActionType, ReplenishActionTypeID>.Create();
	}

	public void SetInvalid()
	{
		id = ReplenishActionTypeID.Invalid;
	}
}
