using System.Collections.Generic;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_2.Data;

public class DetectionEventHooksLoader
{
	public static List<DetectEntityTypeHook> InitDetectEntityTypeHooks()
	{
		return new List<DetectEntityTypeHook>
		{
			new DetectEntityTypeHook
			{
				KeyName = "detectDemonTreeHook",
				TypeKey = "entity:human",
				DetectedEntityKey = "entity:spoakDendront",
				ActionSetsKey = "detectDemonTreeTalk"
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
			}
		};
	}

	public static List<DetectResourceTypeHook> InitDetectResourceTypeHooks()
	{
		return new List<DetectResourceTypeHook>();
	}
}
