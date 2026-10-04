using System.Collections.Generic;
using System.Diagnostics;

namespace UWGame.SimSide.Entities.Containers;

[DebuggerDisplay("{KeyName}")]
public class StorageCondition : IGameData
{
	public string Description;

	public bool RequiresPower;

	/// <summary>
	/// Maintains a constant moisture level.
	/// </summary>
	public float? FixedMoisture;

	/// <summary>
	/// Maintains a constant temperature.
	/// </summary>
	public float? FixedTemperature;

	/// <summary>
	/// Maintains a fixed light level where 0 is darkness and 1 is maximum light.
	/// </summary>
	public float? FixedLightLevel;

	/// <summary>
	/// If true, follows the ambient temperature but skews it toward room temperature.
	/// </summary>
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

	/// <summary>
	/// Skews the temperature toward room temperature.
	/// </summary>
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
