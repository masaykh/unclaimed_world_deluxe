using System.Collections.Generic;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_3.Data;

public class DetectionEventHooksLoader
{
	public static List<DetectEntityTypeHook> InitDetectEntityTypeHooks()
	{
		return new List<DetectEntityTypeHook>
		{
			new DetectEntityTypeHook
			{
				KeyName = "detectCrevice",
				TypeKey = "entity:human",
				DetectedEntityKey = "terrain:crevice",
				ActionSetsKey = "detectCreviceRemark"
			},
			new DetectEntityTypeHook
			{
				KeyName = "detectBushDragon",
				TypeKey = "entity:human",
				DetectedEntityKey = "entity:bushDragon",
				ActionSetsKey = "detectBushDragon"
			}
		};
	}

	public static List<DetectResourceTypeHook> InitDetectResourceTypeHooks()
	{
		return new List<DetectResourceTypeHook>
		{
			new DetectResourceTypeHook
			{
				KeyName = "detectcommonOilTubers",
				TypeKey = "entity:human",
				DetectedResourceKey = "commonOilTubers",
				ActionSetsKey = "detectcommonOilTubersRemark"
			}
		};
	}
}
