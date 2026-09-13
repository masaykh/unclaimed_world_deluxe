using System.Collections.Generic;
using System.Diagnostics;

namespace UWGame.SimSide.Entities.Containers;

[DebuggerDisplay("{KeyName}")]
public class StorageCondition : IGameData
{
	public string Description;

	public bool RequiresPower;

	public float? FixedMoisture;

	public float? FixedTemperature;

	public float? FixedLightLevel;

	public bool IsolatedTemperature;

	public string Name { get; set; }

	public string KeyName { get; set; }

	public bool DeleteRecord { get; set; }

	public float GetTemperature(bool isPowered, float ambientTemperature)
	{
		if (RequiresPower && !isPowered)
		{
			return ambientTemperature;
		}
		if (IsolatedTemperature)
		{
			return ComputeIsolatedTemperature(ambientTemperature);
		}
		return FixedTemperature ?? ambientTemperature;
	}

	public static float ComputeIsolatedTemperature(float ambientTemperature)
	{
		float num = 293f;
		return ambientTemperature - 0.4f * (ambientTemperature - num);
	}

	public void Initialize()
	{
	}

	public void PreInitValidate(ref List<string> listOfErrors)
	{
		if (IsolatedTemperature && FixedTemperature.HasValue)
		{
			EntityType.CreateValidationError(ref listOfErrors, "Cannot specify both IsolatedTemperature and FixedTemperature.");
		}
	}

	public void PreDataCompleteValidate(ref List<string> listOfErrors)
	{
	}

	public void PostInitValidate(ref List<string> listOfErrors)
	{
	}

	public void PostDataCompleteInitialize()
	{
	}

	public void PostDataCompleteValidate(ref List<string> listOfErrors)
	{
	}
}
