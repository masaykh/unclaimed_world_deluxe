using System.Collections.Generic;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_1.Data;

public class DetectionEventHooksLoader
{
	public static List<DetectEntityTypeHook> InitDetectEntityTypeHooks()
	{
		return new List<DetectEntityTypeHook>
		{
			new DetectEntityTypeHook
			{
				KeyName = "detectThunderChickenCarcass",
				TypeKey = "entity:human",
				DetectedEntityKey = "item:thunderChickenCarcass",
				ActionSetsKey = "detectThunderChickenCarcass"
			},
			new DetectEntityTypeHook
			{
				KeyName = "detectQuaditeCarcass",
				TypeKey = "entity:human",
				DetectedEntityKey = "item:quaditeCarcass",
				ActionSetsKey = "detectQuaditeCarcass"
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
			},
			new DetectEntityTypeHook
			{
				KeyName = "detectTwinkler",
				TypeKey = "entity:human",
				DetectedEntityKey = "entity:twinkler",
				ActionSetsKey = "detectTwinkler"
			},
			new DetectEntityTypeHook
			{
				KeyName = "detectBushDragonRemark",
				TypeKey = "entity:human",
				DetectedEntityKey = "entity:bushDragon",
				ActionSetsKey = "detectBushDragonRemark"
			},
			new DetectEntityTypeHook
			{
				KeyName = "detectBushDragonSetDelay",
				TypeKey = "entity:human",
				DetectedEntityKey = "entity:bushDragon",
				ActionSetsKey = "detectBushDragonSetDelay"
			},
			new DetectEntityTypeHook
			{
				KeyName = "detectQuaditeNestRemark",
				TypeKey = "entity:human",
				DetectedEntityKey = "terrain:quaditeNest",
				ActionSetsKey = "detectQuaditeNestRemark"
			},
			new DetectEntityTypeHook
			{
				KeyName = "detectQuaditeNestSetProperty",
				TypeKey = "entity:human",
				DetectedEntityKey = "terrain:quaditeNest",
				ActionSetsKey = "detectQuaditeNestSetProperty"
			},
			new DetectEntityTypeHook
			{
				KeyName = "detectCratesRemark",
				TypeKey = "entity:human",
				DetectedEntityKey = "terrain:crates",
				ActionSetsKey = "detectCratesRemark"
			},
			new DetectEntityTypeHook
			{
				KeyName = "detectCrystalBerries",
				TypeKey = "entity:human",
				DetectedEntityKey = "item:crystalBerries",
				ActionSetsKey = "talkCrystalBerries"
			},
			new DetectEntityTypeHook
			{
				KeyName = "detectSkimmerTailRemark",
				TypeKey = "entity:human",
				DetectedEntityKey = "structure:skimmerTail",
				ActionSetsKey = "detectSkimmerTailRemark"
			},
			new DetectEntityTypeHook
			{
				KeyName = "detectFishTrapCoastRemark",
				TypeKey = "entity:human",
				DetectedEntityKey = "terrain:fishTrapSpotCoast",
				ActionSetsKey = "detectFishTrapCoastRemark"
			},
			new DetectEntityTypeHook
			{
				KeyName = "detectFishTrapShoreRemark",
				TypeKey = "entity:human",
				DetectedEntityKey = "terrain:fishTrapSpotShore",
				ActionSetsKey = "detectFishTrapShoreRemark"
			}
		};
	}

	public static List<DetectResourceTypeHook> InitDetectResourceTypeHooks()
	{
		return new List<DetectResourceTypeHook>
		{
			new DetectResourceTypeHook
			{
				KeyName = "detectShadeleafCanes",
				TypeKey = "entity:human",
				DetectedResourceKey = "crop:shadeleafCanes",
				ActionSetsKey = "detectShadeleafCanes"
			},
			new DetectResourceTypeHook
			{
				KeyName = "detectShadeleafBowStave",
				TypeKey = "entity:human",
				DetectedResourceKey = "crop:shadeleafBowStave",
				ActionSetsKey = "detectShadeleafBowStave"
			},
			new DetectResourceTypeHook
			{
				KeyName = "detectShadeleafResin",
				TypeKey = "entity:human",
				DetectedResourceKey = "crop:shadeleafResin",
				ActionSetsKey = "detectShadeleafResin"
			},
			new DetectResourceTypeHook
			{
				KeyName = "detectSulfur",
				TypeKey = "entity:human",
				DetectedResourceKey = "sulfurDeposit",
				ActionSetsKey = "detectSulfur"
			}
		};
	}
}
