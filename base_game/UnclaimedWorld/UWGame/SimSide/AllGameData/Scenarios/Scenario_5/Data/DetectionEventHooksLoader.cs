using System.Collections.Generic;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_5.Data;

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
				KeyName = "detectSwampDemonTreeHook",
				TypeKey = "entity:human",
				DetectedEntityKey = "entity:swampDendront",
				ActionSetsKey = "detectSwampDemonTreeTalk"
			},
			new DetectEntityTypeHook
			{
				KeyName = "detectWhipjawHook",
				TypeKey = "entity:human",
				DetectedEntityKey = "entity:whipjaw",
				ActionSetsKey = "detectWhipjawTalk"
			},
			new DetectEntityTypeHook
			{
				KeyName = "detectTwinklerHook",
				TypeKey = "entity:human",
				DetectedEntityKey = "entity:twinkler",
				ActionSetsKey = "detectTwinklerTalk"
			},
			new DetectEntityTypeHook
			{
				KeyName = "detectPygmyThunderChicken",
				TypeKey = "entity:human",
				DetectedEntityKey = "entity:pygmyThunderChicken",
				ActionSetsKey = "detectPygmyThunderChicken"
			}
		};
	}

	public static List<DetectResourceTypeHook> InitDetectResourceTypeHooks()
	{
		return new List<DetectResourceTypeHook>();
	}
}
