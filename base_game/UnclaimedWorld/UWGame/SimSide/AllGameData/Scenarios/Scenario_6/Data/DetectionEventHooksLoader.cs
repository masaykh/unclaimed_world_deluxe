using System.Collections.Generic;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_6.Data;

public class DetectionEventHooksLoader
{
	public static List<DetectEntityTypeHook> InitDetectEntityTypeHooks()
	{
		return new List<DetectEntityTypeHook>
		{
			new DetectEntityTypeHook
			{
				KeyName = "detectWhiteThunderChicken",
				TypeKey = "entity:human",
				DetectedEntityKey = "entity:whiteThunderChicken",
				ActionSetsKey = "detectWhiteThunderChicken"
			},
			new DetectEntityTypeHook
			{
				KeyName = "detectPygmyThunderChicken",
				TypeKey = "entity:human",
				DetectedEntityKey = "entity:pygmyThunderChicken",
				ActionSetsKey = "detectPygmyThunderChicken"
			},
			new DetectEntityTypeHook
			{
				KeyName = "detectPatricianHook",
				TypeKey = "entity:human",
				DetectedEntityKey = "entity:patrician",
				ActionSetsKey = "detectPatrician"
			},
			new DetectEntityTypeHook
			{
				KeyName = "detectMudWormHook",
				TypeKey = "entity:human",
				DetectedEntityKey = "entity:mudWorm",
				ActionSetsKey = "detectMudWorm"
			}
		};
	}

	public static List<DetectResourceTypeHook> InitDetectResourceTypeHooks()
	{
		return new List<DetectResourceTypeHook>();
	}
}
