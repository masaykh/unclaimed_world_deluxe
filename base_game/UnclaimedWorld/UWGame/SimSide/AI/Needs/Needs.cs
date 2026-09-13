using System.Collections.Generic;
using System.Linq;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Items;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Needs;

public class Needs : ISnapshot
{
	public Dictionary<string, Need> NeedsList = new Dictionary<string, Need>();

	public BiologicalEntity Parent;

	public int NoOfEssentialFoodNeeds;

	public int NoOfNonEssentialFoodNeeds;

	private bool needsLengthIsDirty = true;

	private float unsatisfiedEssentialNeedsLength;

	private float unsatisfiedNonEssentialNeedsLength;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public float UnsatisfiedEssentialNeedsLength
	{
		get
		{
			if (needsLengthIsDirty)
			{
				UpdateUnsatisfiedNeedsLength();
			}
			return unsatisfiedEssentialNeedsLength;
		}
	}

	public float UnsatisfiedNonEssentialNeedsLength
	{
		get
		{
			if (needsLengthIsDirty)
			{
				UpdateUnsatisfiedNeedsLength();
			}
			return unsatisfiedNonEssentialNeedsLength;
		}
	}

	public bool IsSnapshotted { get; set; }

	public Needs()
	{
	}

	public Needs(BiologicalEntity parent)
	{
		Parent = parent;
	}

	public void SetNeedsDirty()
	{
		needsLengthIsDirty = true;
	}

	public void Initialize()
	{
		UpdateSetOfNeeds();
	}

	public void UpdateNeedsTotalBulk()
	{
		foreach (KeyValuePair<string, Need> needs in NeedsList)
		{
			if (needs.Value.FoodNeed != null)
			{
				needs.Value.FoodNeed.UpdateBulk();
			}
		}
	}

	public void UpdateSetOfNeeds()
	{
		_ = Parent.Parent.ID;
		_ = 4600;
		List<string> list = null;
		foreach (KeyValuePair<string, Need> kvp in NeedsList)
		{
			if (Parent.Parent.BiologicalEntity.AgeGroup.AgeGroupType.NeedTypes.First((NeedType n) => n.KeyName == kvp.Value.NeedType.KeyName) == null)
			{
				Common.AddToList(ref list, kvp.Value.NeedType.KeyName);
			}
		}
		if (list != null)
		{
			foreach (string item in list)
			{
				NeedsList.Remove(item);
			}
		}
		if (Parent.Parent.BiologicalEntity.AgeGroup.AgeGroupType.NeedTypes != null)
		{
			NeedType[] needTypes = Parent.Parent.BiologicalEntity.AgeGroup.AgeGroupType.NeedTypes;
			foreach (NeedType needType in needTypes)
			{
				if (NeedsList.TryGetValue(needType.KeyName, out var value))
				{
					value.DecreasePerDay = (float)Common.MoveValueToNewNormalDistribution(value.DecreasePerDay, value.NeedType.DecreasePerDay.Mean.Value, value.NeedType.DecreasePerDay.StandardDeviation.Value, needType.DecreasePerDay.Mean.Value, needType.DecreasePerDay.StandardDeviation.Value);
					value.NeedType = needType;
				}
				else
				{
					value = new Need(this, needType);
					value.DecreasePerDay = (float)needType.DecreasePerDay.GetRandomValue(The.Sim.GameplayRandomGenerator);
					NeedsList.Add(needType.KeyName, value);
				}
			}
		}
		NoOfEssentialFoodNeeds = 0;
		NoOfNonEssentialFoodNeeds = 0;
		foreach (KeyValuePair<string, Need> needs in NeedsList)
		{
			if (needs.Value.NeedType.FoodNeedType != null)
			{
				if (needs.Value.NeedType.FoodNeedType.IsEssential)
				{
					NoOfEssentialFoodNeeds++;
				}
				else
				{
					NoOfNonEssentialFoodNeeds++;
				}
			}
		}
		UpdateNeedsTotalBulk();
	}

	public bool IsEssential(FoodNutrientType nutrient)
	{
		foreach (KeyValuePair<string, Need> needs in NeedsList)
		{
			if (needs.Value.NeedType.FoodNeedType != null && needs.Value.NeedType.FoodNeedType.IsEssential && needs.Value.NeedType.FoodNeedType.FoodNutrientType == nutrient)
			{
				return true;
			}
		}
		return false;
	}

	public Dictionary<FoodNutrientType, float> GetEssentialNeedBulkAmounts()
	{
		Dictionary<FoodNutrientType, float> dictionary = new Dictionary<FoodNutrientType, float>();
		foreach (KeyValuePair<string, Need> needs in NeedsList)
		{
			if (needs.Value.NeedType.FoodNeedType != null && needs.Value.NeedType.FoodNeedType.IsEssential)
			{
				dictionary.Add(needs.Value.NeedType.FoodNeedType.FoodNutrientType, needs.Value.FoodNeed.CurrentNeededNutrientBulk);
			}
		}
		return dictionary;
	}

	private void UpdateUnsatisfiedNeedsLength()
	{
		unsatisfiedEssentialNeedsLength = 0f;
		unsatisfiedNonEssentialNeedsLength = 0f;
		foreach (KeyValuePair<string, Need> needs in NeedsList)
		{
			if (needs.Value.NeedType.FoodNeedType != null)
			{
				float num = 1f - needs.Value.CurrentLevel;
				if (needs.Value.NeedType.FoodNeedType.IsEssential)
				{
					unsatisfiedEssentialNeedsLength += num;
				}
				else
				{
					unsatisfiedNonEssentialNeedsLength += num;
				}
			}
		}
		needsLengthIsDirty = false;
	}

	public void SatisfyNeeds(double elapsedSeconds, float progressDelta, NeedSatisfaction[] needSatisfaction)
	{
		double num = elapsedSeconds * The.Sim.DateAndTime.DaysPerSecond;
		foreach (NeedSatisfaction needSatisfaction2 in needSatisfaction)
		{
			if (NeedsList.TryGetValue(needSatisfaction2.NeedType, out var value))
			{
				float amount = ((!needSatisfaction2.GainPerDay.HasValue) ? (needSatisfaction2.GainPerDay * progressDelta).Value : ((float)((double?)needSatisfaction2.GainPerDay * num).Value));
				value.Satisfy(amount);
			}
		}
	}

	public void Update(double deltaTimeInSeconds)
	{
		foreach (KeyValuePair<string, Need> needs in NeedsList)
		{
			needs.Value.Update(deltaTimeInSeconds);
		}
	}

	public void UpdateSimulation(double deltaTimeInSeconds)
	{
		foreach (KeyValuePair<string, Need> needs in NeedsList)
		{
			if (needs.Value.NeedType.PhysicalEffects == null || !needs.Value.NeedType.PhysicalEffects.UseExertionFactorToDecrease)
			{
				needs.Value.Update(deltaTimeInSeconds);
			}
		}
	}

	public void UpdateFrequentSimulation(double deltaTimeInSeconds)
	{
		foreach (KeyValuePair<string, Need> needs in NeedsList)
		{
			if (needs.Value.NeedType.PhysicalEffects != null && needs.Value.NeedType.PhysicalEffects.UseExertionFactorToDecrease)
			{
				needs.Value.Update(deltaTimeInSeconds);
			}
		}
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		NeedsList = sn.DoDictionary(NeedsList);
		NoOfEssentialFoodNeeds = sn.DoInt32(NoOfEssentialFoodNeeds);
		NoOfNonEssentialFoodNeeds = sn.DoInt32(NoOfNonEssentialFoodNeeds);
		sn.Ignore(Parent);
		sn.Ignore(needsLengthIsDirty);
		sn.Ignore(unsatisfiedEssentialNeedsLength);
		sn.Ignore(unsatisfiedNonEssentialNeedsLength);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		foreach (KeyValuePair<string, Need> needs in NeedsList)
		{
			needs.Value.Parent = this;
			needs.Value.LoadPostProcess(sn);
		}
	}
}
