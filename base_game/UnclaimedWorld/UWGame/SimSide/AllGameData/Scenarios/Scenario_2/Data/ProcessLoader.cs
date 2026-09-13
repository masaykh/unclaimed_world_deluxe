using System.Collections.Generic;
using UWGame.SimSide.Processes;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_2.Data;

public class ProcessLoader
{
	public static List<ProcessType> Init()
	{
		return new List<ProcessType>
		{
			new ProcessType
			{
				KeyName = "makeImprovisedCookingPot",
				DeleteRecord = true
			},
			new ProcessType
			{
				KeyName = "salvageSkimmerHull",
				DeleteRecord = true
			},
			new ProcessType
			{
				KeyName = "salvageSkimmerTail",
				DeleteRecord = true
			},
			new ProcessType
			{
				KeyName = "salvageSkimmerEngineSide",
				DeleteRecord = true
			},
			new ProcessType
			{
				KeyName = "salvageSkimmerEngineTop",
				DeleteRecord = true
			},
			new ProcessType
			{
				KeyName = "makeVarmintBomb",
				DeleteRecord = true
			},
			new ProcessType
			{
				KeyName = "makeLandMine",
				DeleteRecord = true
			},
			new ProcessType
			{
				KeyName = "makeIronSpear",
				DeleteRecord = true
			},
			new ProcessType
			{
				KeyName = "makeSteelKnife",
				DeleteRecord = true
			},
			new ProcessType
			{
				KeyName = "makeIronArrow",
				DeleteRecord = true
			},
			new ProcessType
			{
				KeyName = "makeIronHoe",
				DeleteRecord = true
			},
			new ProcessType
			{
				KeyName = "makeBrickMold",
				DeleteRecord = true
			},
			new ProcessType
			{
				KeyName = "makeBellows",
				DeleteRecord = true
			},
			new ProcessType
			{
				KeyName = "makeHammer",
				DeleteRecord = true
			},
			new ProcessType
			{
				KeyName = "makeSulfurPowder",
				DeleteRecord = true
			},
			new ProcessType
			{
				KeyName = "makeBlackPowderRifleAmmo",
				DeleteRecord = true
			},
			new ProcessType
			{
				KeyName = "makeCharcoal",
				DeleteRecord = true
			},
			new ProcessType
			{
				KeyName = "makeVat",
				DeleteRecord = true
			},
			new ProcessType
			{
				KeyName = "buildRopeBridge",
				DeleteRecord = true
			},
			new ProcessType
			{
				KeyName = "makeStoneHammer",
				DeleteRecord = true
			}
		};
	}
}
