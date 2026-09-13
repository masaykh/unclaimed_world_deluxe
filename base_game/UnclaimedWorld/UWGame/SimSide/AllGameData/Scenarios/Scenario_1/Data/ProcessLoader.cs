using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Processes;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_1.Data;

public class ProcessLoader
{
	public static List<ProcessType> Init()
	{
		List<ProcessType> list = new List<ProcessType>();
		float value = 6f;
		list.Add(new ProcessType
		{
			Name = "Salvage",
			KeyName = "salvageSkimmerHull",
			RequiredSkill = "menial",
			PhysicalWorkFactor = value,
			IsSalvageProcess = true,
			Stances = UWGame.SimSide.AllGameData.ProcessLoader.GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:skimmerHull",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[6]
			{
				new Output
				{
					EntityTypeToCreate = "item:strippedHull",
					IsWasteProduct = true,
					Amount = new OutputAmount
					{
						NoOfItems = 1
					},
					RelativePlacement = new Vector2(11f, -10f)
				},
				new Output
				{
					EntityTypeToCreate = "item:panelScraps",
					IsWasteProduct = true,
					Amount = new OutputAmount
					{
						NoOfItems = 2
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:textile",
					IsWasteProduct = false,
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:scrapMetal",
					IsWasteProduct = false,
					Amount = new OutputAmount
					{
						NoOfItems = 3
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:seatCushions",
					IsWasteProduct = false,
					Amount = new OutputAmount
					{
						NoOfItems = 3
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:inactivatedFoodCoolerUnit",
					IsWasteProduct = false,
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 75f
			},
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Salvage",
			KeyName = "salvageSkimmerTail",
			RequiredSkill = "menial",
			PhysicalWorkFactor = value,
			IsSalvageProcess = true,
			Stances = UWGame.SimSide.AllGameData.ProcessLoader.GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:skimmerTail",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[3]
			{
				new Output
				{
					EntityTypeToCreate = "item:strippedTail",
					IsWasteProduct = true,
					Amount = new OutputAmount
					{
						NoOfItems = 1
					},
					RelativePlacement = new Vector2(-7f, 15f)
				},
				new Output
				{
					EntityTypeToCreate = "item:panelScraps",
					IsWasteProduct = false,
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:scrapMetal",
					IsWasteProduct = false,
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 75f
			},
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Salvage",
			KeyName = "salvageSkimmerEngineSide",
			RequiredSkill = "menial",
			PhysicalWorkFactor = value,
			IsSalvageProcess = true,
			Stances = UWGame.SimSide.AllGameData.ProcessLoader.GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:skimmerEngineSide",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[3]
			{
				new Output
				{
					EntityTypeToCreate = "item:strippedEngineSide",
					IsWasteProduct = true,
					Amount = new OutputAmount
					{
						NoOfItems = 1
					},
					RelativePlacement = new Vector2(0f, -2f)
				},
				new Output
				{
					EntityTypeToCreate = "item:superconductingWire",
					IsWasteProduct = false,
					Amount = new OutputAmount
					{
						NoOfItems = 2
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:propellerDome",
					IsWasteProduct = false,
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 75f
			},
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Salvage",
			KeyName = "salvageSkimmerEngineTop",
			RequiredSkill = "menial",
			PhysicalWorkFactor = value,
			IsSalvageProcess = true,
			Stances = UWGame.SimSide.AllGameData.ProcessLoader.GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:skimmerEngineTop",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[3]
			{
				new Output
				{
					EntityTypeToCreate = "item:strippedEngineTop",
					IsWasteProduct = true,
					Amount = new OutputAmount
					{
						NoOfItems = 1
					},
					RelativePlacement = new Vector2(-2f, -1f)
				},
				new Output
				{
					EntityTypeToCreate = "item:superconductingWire",
					IsWasteProduct = false,
					Amount = new OutputAmount
					{
						NoOfItems = 2
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:propellerDome",
					IsWasteProduct = false,
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 75f
			},
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeImprovisedCookingPot",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 2.2f,
			Stances = UWGame.SimSide.AllGameData.ProcessLoader.GetToolMakingStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:propellerDome",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:improvisedCookingPot",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.0033333334f
			},
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		return list;
	}
}
