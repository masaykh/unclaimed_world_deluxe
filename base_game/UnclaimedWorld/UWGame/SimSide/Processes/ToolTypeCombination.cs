using System;
using System.Collections.Generic;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Processes;

public class ToolTypeCombination : ILookUp<ToolTypeCombination, ToolTypeCombinationID>
{
	public List<Tuple<EntityType, float>> Tools;

	public float Productivity;

	private ToolTypeCombinationID id = ToolTypeCombinationID.Invalid;

	private static ToolTypeCombinationID IDCounter = ToolTypeCombinationID.First;

	public ToolTypeCombinationID ID
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

	public ToolTypeCombination()
	{
		if (!Snapshotter.IsSnapshotting)
		{
			AddToLookup();
		}
	}

	public ToolTypeCombinationID GetUniqueID()
	{
		IDCounter++;
		if (IDCounter >= ToolTypeCombinationID.Invalid)
		{
			throw new Exception("Astounding, ToolTypeCombinationID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public ToolTypeCombinationID SnapshotID(Snapshotter sn, ToolTypeCombinationID id)
	{
		return sn.DoEnum(id);
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != ToolTypeCombinationID.Invalid)
		{
			LookUp<ToolTypeCombination, ToolTypeCombinationID>.Add(ID, this);
			LookUp<ToolTypeCombination, ToolTypeCombinationID>.SetPerformSnapshot(value: false);
		}
	}

	public void RemoveIDEntry()
	{
		LookUp<ToolTypeCombination, ToolTypeCombinationID>.Remove(this);
	}

	void ILookUp<ToolTypeCombination, ToolTypeCombinationID>.ResetIDCounter()
	{
	}

	public static void ResetIDCounter()
	{
		IDCounter = ToolTypeCombinationID.First;
	}

	void ILookUp<ToolTypeCombination, ToolTypeCombinationID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUp<ToolTypeCombination, ToolTypeCombinationID>.Create();
	}

	public void SetInvalid()
	{
		id = ToolTypeCombinationID.Invalid;
	}

	public static ToolTypeCombination FindByID(ToolTypeCombinationID id)
	{
		if (id == ToolTypeCombinationID.Invalid)
		{
			return null;
		}
		return LookUp<ToolTypeCombination, ToolTypeCombinationID>.FindByID(id);
	}
}
