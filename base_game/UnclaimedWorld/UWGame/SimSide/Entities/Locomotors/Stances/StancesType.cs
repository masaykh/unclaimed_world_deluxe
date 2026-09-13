using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UWGame.ClientSide.Renderables;

namespace UWGame.SimSide.Entities.Locomotors.Stances;

public class StancesType : IGameData
{
	public class StanceChangeDuration
	{
		public string FromStance;

		[XmlIgnore]
		public StanceType FromStanceType;

		public string ToStance;

		[XmlIgnore]
		public StanceType ToStanceType;

		public double Duration;

		public void Initialize()
		{
			FromStanceType = GameData.Instance.AllStanceTypes[FromStance];
			ToStanceType = GameData.Instance.AllStanceTypes[ToStance];
		}
	}

	public string[] Stances;

	[XmlIgnore]
	public List<StanceType> StanceTypes;

	public ChanceToTakeStance[] IdleStancesNearHome;

	public ChanceToTakeStance[] IdleStancesAwayFromHome;

	public ChanceToTakeStance[] PatrolStancesLongerWait;

	public ChanceToTakeStance[] PatrolStancesBriefWait;

	public ChanceToTakeStance[] SleepStances;

	public ChanceToTakeStance[] SearchStancesLongerWait;

	public ChanceToTakeStance[] SearchStancesBriefWait;

	public string DefaultStance;

	[XmlIgnore]
	public StanceType DefaultStanceType;

	public string DefaultStanceWhenWorking;

	[XmlIgnore]
	public StanceType DefaultStanceTypeWhenWorking;

	public string MovingStance;

	[XmlIgnore]
	public StanceType MovingStanceType;

	public string IncapacitatedStance;

	[XmlIgnore]
	public StanceType IncapacitatedStanceType;

	public StanceChangeDuration[] StanceChangeDurations;

	[XmlIgnore]
	public Dictionary<StanceType, Dictionary<StanceType, double>> StanceChangeDurationsMapping;

	public float IdleChanceToSitFactor;

	public float IdleChanceToKneelFactor;

	public float IdleChanceToStandFactor;

	public float IdleChanceToLayDownFactor;

	public float IdleRemainInCurrentSitOrStandStanceAddend;

	public string KeyName { get; set; }

	public string Name { get; set; }

	public bool DeleteRecord { get; set; }

	public void PreInitValidate(ref List<string> errors)
	{
	}

	public void Initialize()
	{
		string[] stances = Stances;
		foreach (string key in stances)
		{
			Common.AddToList(ref StanceTypes, GameData.Instance.AllStanceTypes[key]);
		}
		InitStances(IdleStancesNearHome);
		InitStances(IdleStancesAwayFromHome);
		InitStances(PatrolStancesBriefWait);
		InitStances(PatrolStancesLongerWait);
		InitStances(SearchStancesBriefWait);
		InitStances(SearchStancesLongerWait);
		InitStances(SleepStances);
		DefaultStanceTypeWhenWorking = GameData.Instance.AllStanceTypes[DefaultStanceWhenWorking];
		DefaultStanceType = GameData.Instance.AllStanceTypes[DefaultStance];
		MovingStanceType = GameData.Instance.AllStanceTypes[MovingStance];
		IncapacitatedStanceType = GameData.Instance.AllStanceTypes[IncapacitatedStance];
		if (StanceChangeDurations == null)
		{
			return;
		}
		StanceChangeDurationsMapping = new Dictionary<StanceType, Dictionary<StanceType, double>>();
		StanceChangeDuration[] stanceChangeDurations = StanceChangeDurations;
		for (int i = 0; i < stanceChangeDurations.Length; i++)
		{
			stanceChangeDurations[i].Initialize();
		}
		stanceChangeDurations = StanceChangeDurations;
		foreach (StanceChangeDuration stanceChangeDuration in stanceChangeDurations)
		{
			if (StanceChangeDurationsMapping.TryGetValue(stanceChangeDuration.FromStanceType, out var value))
			{
				continue;
			}
			value = new Dictionary<StanceType, double>();
			Common.AddToDictionary(ref StanceChangeDurationsMapping, stanceChangeDuration.FromStanceType, value);
			StanceChangeDuration[] stanceChangeDurations2 = StanceChangeDurations;
			foreach (StanceChangeDuration stanceChangeDuration2 in stanceChangeDurations2)
			{
				if (stanceChangeDuration2.FromStance == stanceChangeDuration.FromStance && !value.ContainsKey(stanceChangeDuration2.ToStanceType))
				{
					value.Add(stanceChangeDuration2.ToStanceType, stanceChangeDuration2.Duration);
				}
			}
		}
	}

	private void InitStances(ChanceToTakeStance[] stances)
	{
		if (stances != null)
		{
			for (int i = 0; i < stances.Length; i++)
			{
				stances[i].Initialize();
			}
		}
	}

	public void PostInitValidate(ref List<string> errors)
	{
		if (!StanceTypes.Exists((StanceType s) => !s.AnimModifier.HasValue))
		{
			EntityType.CreateValidationError(ref errors, "There must be one default stance in Stances that does not define an AnimModifier flag.");
		}
	}

	public bool IsDefaultStance(AnimConditions conditionSet)
	{
		if (conditionSet == null || conditionSet.Modifiers == null)
		{
			return true;
		}
		return !StanceTypes.Exists((StanceType s) => s.AnimModifier.HasValue && conditionSet.Modifiers.Test(s.AnimModifier.Value));
	}

	public AnimModifier? GetEndStance(AnimConditionInfo info)
	{
		if (info.ConditionSet.Modifiers == null)
		{
			return null;
		}
		BitMask64 modifiers = info.ConditionSet.Modifiers;
		List<StanceType> list = StanceTypes.FindAll((StanceType s) => s.AnimModifier.HasValue && info.ConditionSet.Modifiers.Test(s.AnimModifier.Value));
		if (list == null)
		{
			return null;
		}
		int minNumber = list.Min((StanceType s) => s.Number);
		int maxNumber = list.Max((StanceType s) => s.Number);
		if (modifiers.Test(AnimModifier.Reverse))
		{
			return list.First((StanceType s) => s.Number == minNumber).AnimModifier;
		}
		if (list.Count > 1)
		{
			return list.First((StanceType s) => s.Number == maxNumber).AnimModifier;
		}
		return null;
	}

	public void PreDataCompleteValidate(ref List<string> listOfErrors)
	{
	}

	public void PostDataCompleteInitialize()
	{
	}

	public void PostDataCompleteValidate(ref List<string> listOfErrors)
	{
	}
}
