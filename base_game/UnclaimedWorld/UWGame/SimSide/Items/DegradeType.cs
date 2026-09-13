using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;

namespace UWGame.SimSide.Items;

[DebuggerDisplay("{KeyName}")]
public class DegradeType : IGameData
{
	public class StorageDamageEstimation
	{
		public float Damage;

		public StorageCondition Condition;

		public static int CompareDamage(StorageDamageEstimation x, StorageDamageEstimation y)
		{
			if (x.Damage == y.Damage)
			{
				return 0;
			}
			if (y.Damage > x.Damage)
			{
				return -1;
			}
			return 1;
		}
	}

	public const float WaterBoilingPoint = 373f;

	public const float RoomTemperature = 293f;

	public const float WaterFreezingPoint = 273f;

	public const float Hot = 313f;

	public const float Refrigeration = 278f;

	public const float DeepFreeze = 255f;

	public string Description;

	public Vector2[] MoistureDamage;

	public Vector2[] TemperatureDamage;

	public Vector2[] LightDamage;

	[XmlIgnore]
	public List<StorageDamageEstimation> ConditionDamages = new List<StorageDamageEstimation>();

	[XmlIgnore]
	public List<StorageDuration> StorageDurations = new List<StorageDuration>();

	public string Name { get; set; }

	public string KeyName { get; set; }

	public bool DeleteRecord { get; set; }

	public void Initialize()
	{
		ConditionDamages.Clear();
		float interpolatedFunctionValue = Common.GetInterpolatedFunctionValue(0f, LightDamage);
		float interpolatedFunctionValue2 = Common.GetInterpolatedFunctionValue(0f, MoistureDamage);
		foreach (KeyValuePair<string, StorageCondition> allStorageCondition in GameData.Instance.AllStorageConditions)
		{
			if (allStorageCondition.Key != "exposed" && allStorageCondition.Key != "isolated" && allStorageCondition.Value.FixedTemperature.HasValue)
			{
				float damage = Common.GetInterpolatedFunctionValue(allStorageCondition.Value.FixedTemperature.Value, TemperatureDamage) + interpolatedFunctionValue + interpolatedFunctionValue2;
				ConditionDamages.Add(new StorageDamageEstimation
				{
					Condition = allStorageCondition.Value,
					Damage = damage
				});
			}
		}
		float num = Math.Max(Common.GetInterpolatedFunctionValue(309f, TemperatureDamage), Common.GetInterpolatedFunctionValue(255f, TemperatureDamage)) + Common.GetInterpolatedFunctionValue(0.8f, MoistureDamage) + Common.GetInterpolatedFunctionValue(1f, LightDamage);
		num *= 0.8f;
		ConditionDamages.Add(new StorageDamageEstimation
		{
			Condition = GameData.Instance.AllStorageConditions["exposed"],
			Damage = num
		});
		float num2 = Math.Max(Common.GetInterpolatedFunctionValue(301f, TemperatureDamage), Common.GetInterpolatedFunctionValue(271f, TemperatureDamage)) + Common.GetInterpolatedFunctionValue(0f, MoistureDamage) + Common.GetInterpolatedFunctionValue(0f, LightDamage);
		num2 *= 0.8f;
		ConditionDamages.Add(new StorageDamageEstimation
		{
			Condition = GameData.Instance.AllStorageConditions["isolated"],
			Damage = num2
		});
		ConditionDamages.Sort(StorageDamageEstimation.CompareDamage);
	}

	public void PreInitValidate(ref List<string> listOfErrors)
	{
	}

	public void PostInitValidate(ref List<string> listOfErrors)
	{
	}

	public void PostDataCompleteInitialize()
	{
		float meanAmbientTemperature = GameData.Instance.Constants.MeanAmbientTemperature;
		StorageDurationToDisplay[] storageDurationToDisplay = GameData.Instance.GUIConstants.StorageDurationToDisplay;
		foreach (StorageDurationToDisplay storageDurationToDisplay2 in storageDurationToDisplay)
		{
			StorageCondition storage = GameData.Instance.AllStorageConditions[storageDurationToDisplay2.StorageCondition];
			ComputeDuration(meanAmbientTemperature, 1f, storage, storageDurationToDisplay2);
		}
	}

	private void ComputeDuration(float outsideTemperature, float lightLevel, StorageCondition storage, StorageDurationToDisplay toDisplay)
	{
		float num = NonLivingEntity.ComputeDegradeDamage(this, storage, toDisplay.IsStorageOfWeatherProofPart, outsideTemperature, lightLevel, true, 1.0);
		float duration = ((!(num > 0f)) ? 9999f : (1f / num));
		StorageDuration storageDuration = new StorageDuration();
		storageDuration.Duration = duration;
		storageDuration.StorageDurationToDisplay = toDisplay;
		storageDuration.DegradeType = this;
		storageDuration.StorageCondition = storage;
		storageDuration.ComputeSortOrder();
		StorageDurations.Add(storageDuration);
	}

	public void PreDataCompleteValidate(ref List<string> listOfErrors)
	{
	}

	public void PostDataCompleteValidate(ref List<string> listOfErrors)
	{
	}
}
