using System;
using System.Collections.Generic;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Entities;
using UWGame.SimSide.SimEffects;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Items;

public class Food : ISnapshot
{
	public Dictionary<FoodNutrientType, float> NutrientBulkAmounts = new Dictionary<FoodNutrientType, float>();

	public Item Parent;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public Food()
	{
	}

	public Food(Item parent)
	{
		Parent = parent;
	}

	public void UpdateNutrientAmounts(Entity parent)
	{
		NutrientBulkAmounts.Clear();
		UpdateNutrientAmounts(parent.EntityType, parent.Bulk, NutrientBulkAmounts);
	}

	public static void UpdateNutrientAmounts(EntityType foodType, float bulk, Dictionary<FoodNutrientType, float> nutrientBulkAmounts)
	{
		FoodNutrientAmount[] foodNutrientTypes = foodType.ItemType.FoodType.FoodNutrientProfile.FoodNutrientTypes;
		foreach (FoodNutrientAmount foodNutrientAmount in foodNutrientTypes)
		{
			nutrientBulkAmounts.Add(foodNutrientAmount.Nutrient, bulk * foodNutrientAmount.Amount);
		}
	}

	public void ConsumeBy(Entity consumer)
	{
		foreach (KeyValuePair<FoodNutrientType, float> nutrientBulkAmount in NutrientBulkAmounts)
		{
			if (nutrientBulkAmount.Value > 0f && consumer.BiologicalEntity.Needs.NeedsList.TryGetValue(nutrientBulkAmount.Key.KeyName, out var value))
			{
				float num = Math.Min(nutrientBulkAmount.Value, value.FoodNeed.CurrentNeededNutrientBulk);
				LogConsumeStatistics(consumer, nutrientBulkAmount.Key, nutrientBulkAmount.Value, num);
				float amount = num / value.FoodNeed.TotalNeededNutrientBulk;
				value.Satisfy(amount);
			}
		}
		List<EffectProfileType> effectTypes = Parent.Parent.EntityType.ItemType.FoodType.EffectTypes;
		if (effectTypes == null)
		{
			return;
		}
		foreach (EffectProfileType item in effectTypes)
		{
			consumer.SimEffects.Start(item);
		}
	}

	private void LogConsumeStatistics(Entity consumer, FoodNutrientType nutrientType, float nutrientValue, float uptake)
	{
		if (consumer.Intelligence.IsIndependent())
		{
			float num = nutrientValue - uptake;
			consumer.Intelligence.Allegiance.Statistics.AddNutrientEvent(nutrientType, NutrientStatistics.StatTypes.Consumed, uptake);
			if (Common.IsGreaterThan(num, 0f))
			{
				consumer.Intelligence.Allegiance.Statistics.AddNutrientEvent(nutrientType, NutrientStatistics.StatTypes.Overconsumed, num);
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
		NutrientBulkAmounts = sn.DoDictionary(NutrientBulkAmounts);
		sn.Ignore(Parent);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}
}
