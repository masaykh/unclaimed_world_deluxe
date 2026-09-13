using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Needs;

public class NeedType : ILookUp<NeedType, NeedTypeID>
{
	public string KeyName;

	public FoodNeedType FoodNeedType;

	public SleepNeedType SleepNeedType;

	public ProcessNeedType ProcessNeedType;

	public PhysicalEffects PhysicalEffects;

	public NormalDistribution DecreasePerDay;

	public float LimitForDecreasedEnergy;

	public float DecreasedEnergyWeight;

	public bool LevelVisibleToOtherAllegiances = true;

	[XmlIgnore]
	public float NormalizedDecreasedEnergyWeight;

	private NeedTypeID id = NeedTypeID.Invalid;

	private static NeedTypeID IDCounter;

	[XmlIgnore]
	public NeedTypeID ID
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
			ID = (NeedTypeID)value;
		}
	}

	public int LoadPostProcessOrder => 0;

	public NeedType()
	{
		if (!Snapshotter.IsSnapshotting)
		{
			AddToLookup();
		}
	}

	public override string ToString()
	{
		if (FoodNeedType != null && FoodNeedType.FoodNutrientType != null)
		{
			return FoodNeedType.FoodNutrientType.Name;
		}
		return KeyName.ToString();
	}

	public void Initialize()
	{
	}

	public void PostDataCompleteInitialize()
	{
		if (FoodNeedType != null)
		{
			FoodNeedType.PostDataCompleteInitialize();
		}
	}

	public void Validate(ref List<string> errors)
	{
		EntityType.ValidateRequiredValue(ref errors, "DecreasePerDay", DecreasePerDay != null);
		if (DecreasePerDay != null)
		{
			EntityType.ValidateRequiredValue(ref errors, "DecreasePerDay.Mean", DecreasePerDay.Mean.HasValue);
			EntityType.ValidateRequiredValue(ref errors, "DecreasePerDay.StandardDeviation", DecreasePerDay.StandardDeviation.HasValue);
		}
	}

	public bool GivesComfortEffects()
	{
		if (FoodNeedType != null && FoodNeedType.FoodNutrientType.SatisfiesComfort)
		{
			return true;
		}
		return false;
	}

	public float? GetEnergyFactor(float currentLevel)
	{
		if (LimitForDecreasedEnergy > 0f)
		{
			float num = 1f;
			if (currentLevel < LimitForDecreasedEnergy)
			{
				float num2 = LimitForDecreasedEnergy - currentLevel;
				num = 1f - num2 / LimitForDecreasedEnergy;
				num = Common.Clamp(num, 0f, 1f);
			}
			return num * NormalizedDecreasedEnergyWeight;
		}
		return null;
	}

	public NeedTypeID GetUniqueID()
	{
		IDCounter++;
		if (IDCounter >= NeedTypeID.Invalid)
		{
			throw new Exception("Astounding, NeedTypeID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public NeedTypeID SnapshotID(Snapshotter sn, NeedTypeID id)
	{
		return sn.DoEnum(id);
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != NeedTypeID.Invalid)
		{
			LookUp<NeedType, NeedTypeID>.Add(ID, this);
		}
		LookUp<NeedType, NeedTypeID>.SetPerformSnapshot(value: false);
	}

	public void SetInvalid()
	{
		id = NeedTypeID.Invalid;
	}

	public void RemoveIDEntry()
	{
		LookUp<NeedType, NeedTypeID>.Remove(this);
	}

	void ILookUp<NeedType, NeedTypeID>.ResetIDCounter()
	{
	}

	public static void ResetIDCounter()
	{
		IDCounter = NeedTypeID.First;
	}

	void ILookUp<NeedType, NeedTypeID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUp<NeedType, NeedTypeID>.Create();
	}
}
