using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Entities.Locomotors.Stances;
using UWGame.SimSide.Processes;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_3.Data;

public class ProcessLoader
{
	public static List<ProcessType> Init()
	{
		List<ProcessType> list = new List<ProcessType>();
		SerializableDictionary<string, ChanceToTakeStance[]> kneelingOrStandingProduction = UWGame.SimSide.AllGameData.ProcessLoader.kneelingOrStandingProduction;
		SerializableDictionary<string, ChanceToTakeStance[]> kneelingOrStandingProduction2 = UWGame.SimSide.AllGameData.ProcessLoader.kneelingOrStandingProduction;
		list.Add(new ProcessType
		{
			Name = "Build rope bridge",
			KeyName = "buildRopeBridge",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "bushcraft",
			SummaryDescription = "Build a simple bridge with wire rope for crossing the gorge",
			Description = "The wire rope from the boat should be sufficient to build this.",
			PhysicalWorkFactor = 4f,
			Stances = UWGame.SimSide.AllGameData.ProcessLoader.kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:lines",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.0053333337f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Construct",
			KeyName = "constructSignalPyre",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			Stances = kneelingOrStandingProduction,
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:firewood",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = true
				},
				new Input
				{
					Entity = "item:spoakBranches",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[2]
			{
				new Output
				{
					EntityTypeToCreate = "structure:signalPyre",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:firewood",
					IsWasteProduct = true,
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			AgentActionState = AnimAction.Building
		});
		list.Add(new ProcessType
		{
			Name = "Light signal pyre",
			KeyName = "lightSignalPyre",
			RequiredSkill = "menial",
			SummaryDescription = "Light the fire and start signalling for help",
			PhysicalWorkFactor = 4f,
			Stances = UWGame.SimSide.AllGameData.ProcessLoader.kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:firewood",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = true
				}
			},
			RequiresBoldStance = false,
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.0033333334f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Salvage",
			KeyName = "salvageBoatWreck",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = kneelingOrStandingProduction2,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:boatWreck",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[4]
			{
				new Output
				{
					EntityTypeToCreate = "item:strippedCatamaran",
					IsWasteProduct = true,
					Amount = new OutputAmount
					{
						NoOfItems = 1
					},
					RelativePlacement = new Vector2(6f, -29f)
				},
				new Output
				{
					EntityTypeToCreate = "item:catamaranMast",
					IsWasteProduct = true,
					Amount = new OutputAmount
					{
						NoOfItems = 1
					},
					RelativePlacement = new Vector2(-84f, 64f)
				},
				new Output
				{
					EntityTypeToCreate = "item:lines",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:metalWire",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.0046666665f
			},
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			KeyName = "makeBakedCommonOilTubers",
			DeleteRecord = true
		});
		return list;
	}
}
