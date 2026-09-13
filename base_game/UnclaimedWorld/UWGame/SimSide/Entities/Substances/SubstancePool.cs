using System;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities.Substances;

public class SubstancePool : ILookUp<SubstancePool, SubstancePoolID>
{
	public SubstanceType SubstanceType;

	private SubstancePoolID id = SubstancePoolID.Invalid;

	private static SubstancePoolID IDCounter;

	public SubstancePoolID ID
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

	public void RequestSubstance()
	{
	}

	public void Consume(float amount)
	{
	}

	public SubstancePoolID GetUniqueID()
	{
		IDCounter++;
		if (IDCounter >= SubstancePoolID.Invalid)
		{
			throw new Exception("Astounding, SubstancePoolID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public SubstancePoolID SnapshotID(Snapshotter sn, SubstancePoolID id)
	{
		return sn.DoEnum(id);
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != SubstancePoolID.Invalid)
		{
			LookUp<SubstancePool, SubstancePoolID>.Add(ID, this);
		}
	}

	public void SetInvalid()
	{
		id = SubstancePoolID.Invalid;
	}

	public void RemoveIDEntry()
	{
		LookUp<SubstancePool, SubstancePoolID>.Remove(this);
	}

	void ILookUp<SubstancePool, SubstancePoolID>.ResetIDCounter()
	{
	}

	public static void ResetIDCounter()
	{
		IDCounter = SubstancePoolID.First;
	}

	void ILookUp<SubstancePool, SubstancePoolID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUp<SubstancePool, SubstancePoolID>.Create();
	}
}
