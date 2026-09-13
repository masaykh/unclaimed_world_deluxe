using System;
using System.Collections.Generic;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Entities.Locomotors.Stances;
using UWGame.SimSide.Policies;
using UWGame.SimSide.Processes;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.AllGameData;

public class ProcessLoader
{
	public const float sittingLightWork = 2f;

	public const float walkingLightWork = 3f;

	public const float moderateWork = 4f;

	public const float moderateHardWork = 6f;

	public const float hardWork = 8f;

	public const float assembleTool = 2.2f;

	public const string synthesizingName = "Synthesizing";

	public const string cookingName = "Cooking";

	public const string reloadSentryName = "Reload sentry";

	public const string toolmakingName = "Producing";

	public const string producingName = "Producing";

	public const string upgradingName = "Upgrading";

	public const string salvagingName = "Salvage";

	public const string removingName = "Removing";

	public const string clearAwayName = "Clear away";

	public const string settingUpName = "Setting up";

	public const string constructingStructureName = "Constructing";

	public const string packingDownName = "Pack down";

	public const string disassembleName = "Disassemble";

	public const string harvestingName = "Harvesting";

	public const string growingName = "Growing";

	public const string gatheringName = "Gathering";

	public const string catchingName = "Catching";

	public const string fishingName = "Fishing";

	public const string huntingName = "Hunting";

	public const string miningName = "Extracting";

	public const string extractingName = "Eating";

	public const string primaryExtraction = "Extracting deposit";

	public const string cultivatingName = "Cultivating";

	public const string salvageWithLossSummary = "When breaking this object apart, some parts will be retrieved and some may be lost.";

	public const string disassembleWithoutLossSummary = "This object can be disassembled without losing any parts.";

	public const string packingDownWithoutLossSummary = "This object can be packed down and set up repeatedly without losing any parts.";

	public const string clearAwaySummary = "Remove all traces of this structure.";

	private const float baseDaysOfWorkNeeded = 1f / 3f;

	public const float timeToPrepareTool = 0.002f;

	public const float timeToReloadInSeconds = 1.92f;

	public const float timeToReloadFireExtinguisherInSeconds = 1.36f;

	public const float timeToReloadImprovisedBowInSeconds = 1.42f;

	public const float timeForInstantCraftingTask = 0.00033333336f;

	public const float timeForTinyCraftingTask = 0.0033333334f;

	public const float timeForSmallCraftingTask = 1f / 150f;

	public const float timeForMediumCraftingTask = 1f / 75f;

	public const float timeForSmithingBigTool = 0.02f;

	public const float timeForBigCraftingTask = 1f / 30f;

	public const float timeForExtraBigCraftingTask = 0.050000004f;

	public const float timeToCreatePit = 1f / 75f;

	public const float timeReconditioningSurvivalPart = 0.01f;

	public const float timeForShortStandaloneTask = 1f / 30f;

	public const float timeForLongStandaloneTask = 71f / (226f * (float)Math.PI);

	public const float timeForVeryLongStandaloneTask = 0.3f;

	public const float timeForMolecularAssembly = 1f / 75f;

	public const float timeForMolecularPlateAssembly = 1f / 30f;

	public const float timeForTinyHarvestingTask = 0.0033333334f;

	public const float timeForSmallHarvestingTask = 1f / 150f;

	public const float timeForMediumHarvestingTask = 1f / 75f;

	public const float fishing = 2.4f;

	public const float constructionExertion = 4f;

	public const float timeForSmallPrimitiveShelter = 1f / 120f;

	public const float timeForMediumPrimitiveShelter = 0.011666667f;

	public const float timeForLargePrimitiveShelter = 0.023333333f;

	public const float timeToDigHole = 1f / 150f;

	public const float timeToBuildAbatis = 0.0033333334f;

	public const float timeToBuildSmallPlot = 0.023333333f;

	public const float timeToBuildLargePlot = 0.050000004f;

	public const float salvageStructure = 4f;

	public const float timeForReplenishing = 0.0023333335f;

	public static SerializableDictionary<string, ChanceToTakeStance[]> standingProduction = new SerializableDictionary<string, ChanceToTakeStance[]>
	{
		{
			"humanoid",
			new ChanceToTakeStance[1]
			{
				new ChanceToTakeStance
				{
					Stance = "standing"
				}
			}
		},
		{
			"robot",
			new ChanceToTakeStance[1]
			{
				new ChanceToTakeStance
				{
					Stance = "active"
				}
			}
		}
	};

	public static SerializableDictionary<string, ChanceToTakeStance[]> sittingProduction = new SerializableDictionary<string, ChanceToTakeStance[]>
	{
		{
			"humanoid",
			new ChanceToTakeStance[1]
			{
				new ChanceToTakeStance
				{
					Stance = "sitting"
				}
			}
		},
		{
			"robot",
			new ChanceToTakeStance[1]
			{
				new ChanceToTakeStance
				{
					Stance = "active"
				}
			}
		}
	};

	public static SerializableDictionary<string, ChanceToTakeStance[]> kneelingOrStandingProduction = new SerializableDictionary<string, ChanceToTakeStance[]>
	{
		{
			"humanoid",
			new ChanceToTakeStance[2]
			{
				new ChanceToTakeStance
				{
					Stance = "kneeling",
					AddedChanceToRemainInStance = 0.1f
				},
				new ChanceToTakeStance
				{
					Stance = "standing",
					AddedChanceToRemainInStance = 0.1f
				}
			}
		},
		{
			"robot",
			new ChanceToTakeStance[1]
			{
				new ChanceToTakeStance
				{
					Stance = "active"
				}
			}
		}
	};

	public static SerializableDictionary<string, ChanceToTakeStance[]> kneelingProduction = new SerializableDictionary<string, ChanceToTakeStance[]>
	{
		{
			"humanoid",
			new ChanceToTakeStance[1]
			{
				new ChanceToTakeStance
				{
					Stance = "kneeling"
				}
			}
		},
		{
			"robot",
			new ChanceToTakeStance[1]
			{
				new ChanceToTakeStance
				{
					Stance = "active"
				}
			}
		}
	};

	public static SerializableDictionary<string, ChanceToTakeStance[]> GetSmithingStance()
	{
		return standingProduction;
	}

	public static SerializableDictionary<string, ChanceToTakeStance[]> GetToolMakingStances()
	{
		return kneelingProduction;
	}

	public static SerializableDictionary<string, ChanceToTakeStance[]> GetSalvageStructureStances()
	{
		return kneelingOrStandingProduction;
	}

	public static List<ProcessType> InitProcessTypes()
	{
		List<ProcessType> list = new List<ProcessType>();
		list.Add(new ProcessType
		{
			KeyName = "lightFireFuelBurning",
			Name = "Lighting fire",
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.002f
			},
			PreparedToolModifier = StateModifier.BurningFuel,
			AgentActionState = AnimAction.Mending,
			Stances = kneelingProduction
		});
		list.Add(new ProcessType
		{
			KeyName = "lightFire",
			Name = "Lighting fire",
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.002f
			},
			AgentActionState = AnimAction.Mending,
			Stances = kneelingProduction
		});
		list.Add(new ProcessType
		{
			KeyName = "kitchenImprovisedLightFire",
			Name = "Lighting fire",
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.002f
			},
			AgentActionState = AnimAction.Mending,
			Stances = kneelingProduction
		});
		list.Add(new ProcessType
		{
			KeyName = "lightFireWithoutFlames",
			Name = "Lighting fire",
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.002f
			},
			AgentActionState = AnimAction.Mending,
			Stances = kneelingProduction
		});
		list.Add(new ProcessType
		{
			KeyName = "forgeSmoke",
			Name = "Lighting fire",
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.002f
			},
			AgentActionState = AnimAction.Mending,
			Stances = kneelingProduction
		});
		list.Add(new ProcessType
		{
			KeyName = "kilnSmoke",
			Name = "Lighting fire",
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.002f
			},
			AgentActionState = AnimAction.Mending,
			Stances = kneelingProduction
		});
		list.Add(new ProcessType
		{
			KeyName = "cookAtStove",
			Name = "Cooking at stove",
			JobTypeKey = "cookingJobType",
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.002f
			},
			AgentActionState = AnimAction.Mending,
			Stances = kneelingProduction
		});
		list.Add(new ProcessType
		{
			KeyName = "reloadBoltActionRifle",
			Name = "Reloading",
			WorkOrTimeNeeded = new WorkOrTime
			{
				TimeInSecondsNeeded = 1.92f
			},
			Stances = standingProduction,
			AgentActionState = AnimAction.Reloading,
			ReplenishAction = GoalReplenish.ReplenishAction.Reload
		});
		list.Add(new ProcessType
		{
			KeyName = "reloadGunpowderRifle",
			Name = "Reloading",
			WorkOrTimeNeeded = new WorkOrTime
			{
				TimeInSecondsNeeded = 2.8799999f
			},
			Stances = standingProduction,
			AgentActionState = AnimAction.Reloading,
			ReplenishAction = GoalReplenish.ReplenishAction.Reload
		});
		list.Add(new ProcessType
		{
			KeyName = "reloadCoilRifleAmmo",
			Name = "Reloading",
			WorkOrTimeNeeded = new WorkOrTime
			{
				TimeInSecondsNeeded = 1.92f
			},
			Stances = standingProduction,
			AgentActionState = AnimAction.Reloading,
			ReplenishAction = GoalReplenish.ReplenishAction.Reload
		});
		list.Add(new ProcessType
		{
			KeyName = "reloadBlunderbuss",
			Name = "Reloading",
			WorkOrTimeNeeded = new WorkOrTime
			{
				TimeInSecondsNeeded = 2.8799999f
			},
			Stances = standingProduction,
			AgentActionState = AnimAction.Reloading,
			ReplenishAction = GoalReplenish.ReplenishAction.Reload
		});
		list.Add(new ProcessType
		{
			KeyName = "reloadShotgun",
			Name = "Reloading",
			WorkOrTimeNeeded = new WorkOrTime
			{
				TimeInSecondsNeeded = 1.92f
			},
			Stances = standingProduction,
			AgentActionState = AnimAction.Reloading,
			ReplenishAction = GoalReplenish.ReplenishAction.Reload
		});
		list.Add(new ProcessType
		{
			KeyName = "reloadFireExtinguisher",
			Name = "Reloading",
			WorkOrTimeNeeded = new WorkOrTime
			{
				TimeInSecondsNeeded = 1.36f
			},
			Stances = standingProduction,
			AgentActionState = AnimAction.Reloading,
			ReplenishAction = GoalReplenish.ReplenishAction.Reload
		});
		list.Add(new ProcessType
		{
			KeyName = "reloadImprovisedBow",
			Name = "Reloading",
			WorkOrTimeNeeded = new WorkOrTime
			{
				TimeInSecondsNeeded = 1.42f
			},
			Stances = standingProduction,
			AgentActionState = AnimAction.Reloading,
			ReplenishAction = GoalReplenish.ReplenishAction.Reload
		});
		list.Add(new ProcessType
		{
			KeyName = "refuelCampfire",
			Name = "Rekindling",
			WorkOrTimeNeeded = new WorkOrTime
			{
				TimeInSecondsNeeded = 3f
			},
			Stances = kneelingProduction,
			AgentActionState = AnimAction.Mending,
			ReplenishAction = GoalReplenish.ReplenishAction.Refuel
		});
		list.Add(new ProcessType
		{
			KeyName = "refuelSmokeOven",
			Name = "Rekindling",
			WorkOrTimeNeeded = new WorkOrTime
			{
				TimeInSecondsNeeded = 5f
			},
			Stances = kneelingProduction,
			AgentActionState = AnimAction.Mending,
			ReplenishAction = GoalReplenish.ReplenishAction.Refuel
		});
		list.Add(new ProcessType
		{
			KeyName = "refuelSmithy",
			Name = "Refuelling",
			WorkOrTimeNeeded = new WorkOrTime
			{
				TimeInSecondsNeeded = 3f
			},
			Stances = kneelingProduction,
			AgentActionState = AnimAction.Mending,
			ReplenishAction = GoalReplenish.ReplenishAction.Refuel
		});
		list.Add(new ProcessType
		{
			KeyName = "refuelKiln",
			Name = "Refuelling",
			WorkOrTimeNeeded = new WorkOrTime
			{
				TimeInSecondsNeeded = 3f
			},
			Stances = kneelingProduction,
			AgentActionState = AnimAction.Mending,
			ReplenishAction = GoalReplenish.ReplenishAction.Refuel
		});
		list.Add(new ProcessType
		{
			KeyName = "refuelKitchen",
			Name = "Refuelling",
			WorkOrTimeNeeded = new WorkOrTime
			{
				TimeInSecondsNeeded = 3f
			},
			Stances = kneelingProduction,
			AgentActionState = AnimAction.Mending,
			ReplenishAction = GoalReplenish.ReplenishAction.Refuel
		});
		list.Add(new ProcessType
		{
			Name = "Reload sentry",
			KeyName = "reloadSentryGun",
			JobTypeKey = "sentryReloadJobType",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.0023333335f
			}
		});
		float value = 6f;
		list.Add(new ProcessType
		{
			Name = "Butchering",
			KeyName = "butcherMegapod",
			JobTypeKey = "butcheringJobType",
			RequiredSkill = "butchering",
			PhysicalWorkFactor = value,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:megapodCarcass",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1,
						Substances = new string[4] { "meat", "guts", "hide", "bones" }
					}
				}
			},
			Outputs = new Output[2]
			{
				new Output
				{
					EntityTypeToCreate = "item:megapodGreenHide",
					Amount = new OutputAmount
					{
						Bulk = new Bulk
						{
							InputSubstance = "hide"
						}
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:megapodBrain",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			ProcessToolSetKey = "toolSetButcherFlesh",
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f,
				MultiplyByBulk = true
			},
			AgentActionState = AnimAction.Butchering,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Butchering",
			KeyName = "butcherWhipjaw",
			JobTypeKey = "butcheringJobType",
			RequiredSkill = "butchering",
			PhysicalWorkFactor = value,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:whipjawCarcass",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1,
						Substances = new string[4] { "meat", "guts", "hide", "bones" }
					}
				}
			},
			Outputs = new Output[2]
			{
				new Output
				{
					EntityTypeToCreate = "item:whipjawGreenHide",
					Amount = new OutputAmount
					{
						Bulk = new Bulk
						{
							InputSubstance = "hide"
						}
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:whipjawBrain",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			ProcessToolSetKey = "toolSetButcherFlesh",
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f,
				MultiplyByBulk = true
			},
			AgentActionState = AnimAction.Butchering,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Butchering",
			KeyName = "butcherTurnip",
			JobTypeKey = "butcheringJobType",
			RequiredSkill = "butchering",
			PhysicalWorkFactor = value,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:turnipCarcass",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1,
						Substances = new string[4] { "meat", "guts", "shell", "hide" }
					}
				}
			},
			Outputs = new Output[4]
			{
				new Output
				{
					EntityTypeToCreate = "item:turnipMeat",
					Amount = new OutputAmount
					{
						Bulk = new Bulk
						{
							InputSubstance = "meat"
						}
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:turnipShell",
					Amount = new OutputAmount
					{
						Bulk = new Bulk
						{
							InputSubstance = "shell"
						}
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:turnipGuts",
					Amount = new OutputAmount
					{
						Bulk = new Bulk
						{
							InputSubstance = "guts"
						}
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:turnipBrain",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			ProcessToolSetKey = "toolSetButcherTurnip",
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f,
				MultiplyByBulk = true
			},
			AgentActionState = AnimAction.Butchering,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Butchering",
			KeyName = "butcherThunderChicken",
			JobTypeKey = "butcheringJobType",
			RequiredSkill = "butchering",
			PhysicalWorkFactor = value,
			Stances = standingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:thunderChickenCarcass",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1,
						Substances = new string[4] { "meat", "guts", "hide", "bones" }
					}
				}
			},
			Outputs = new Output[4]
			{
				new Output
				{
					EntityTypeToCreate = "item:thunderChickenMeat",
					Amount = new OutputAmount
					{
						Bulk = new Bulk
						{
							InputSubstance = "meat"
						}
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:thunderChickenGreenHide",
					Amount = new OutputAmount
					{
						Bulk = new Bulk
						{
							InputSubstance = "hide"
						}
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:thunderChickenGuts",
					Amount = new OutputAmount
					{
						Bulk = new Bulk
						{
							InputSubstance = "guts"
						}
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:thunderChickenBrain",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			ProcessToolSetKey = "toolSetButcherFlesh",
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			AgentActionState = AnimAction.Butchering,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Butchering",
			KeyName = "butcherTwinkler",
			JobTypeKey = "butcheringJobType",
			RequiredSkill = "butchering",
			PhysicalWorkFactor = value,
			Stances = standingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:quaditeCarcass",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1,
						Substances = new string[3] { "meat", "plating", "guts" }
					}
				}
			},
			Outputs = new Output[2]
			{
				new Output
				{
					EntityTypeToCreate = "item:twinklerMeat",
					Amount = new OutputAmount
					{
						Bulk = new Bulk
						{
							InputSubstance = "meat"
						}
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:twinklerPlating",
					Amount = new OutputAmount
					{
						Bulk = new Bulk
						{
							InputSubstance = "plating"
						}
					}
				}
			},
			ProcessToolSetKey = "toolSetButcherFlesh",
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 75f
			},
			AgentActionState = AnimAction.Butchering,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Butchering",
			KeyName = "butcherBushDragon",
			JobTypeKey = "butcheringJobType",
			RequiredSkill = "butchering",
			PhysicalWorkFactor = value,
			Stances = standingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:bushDragonCarcass",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[2]
			{
				new Output
				{
					EntityTypeToCreate = "item:bushDragonPoisonGlands",
					Amount = new OutputAmount
					{
						NoOfItems = 1,
						Bulk = new Bulk
						{
							FractionOfInputBulk = 0.05f
						}
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:bushDragonHarvestedCarcass",
					IsWasteProduct = true,
					Amount = new OutputAmount
					{
						NoOfItems = 1,
						Bulk = new Bulk
						{
							FractionOfInputBulk = 0.95f
						}
					}
				}
			},
			ProcessToolSetKey = "toolSetButcherFlesh",
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 75f
			},
			AgentActionState = AnimAction.Butchering,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Butchering",
			KeyName = "butcherBinalRat",
			JobTypeKey = "butcheringJobType",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 2f,
			Stances = standingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:binalRatCarcass",
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
					EntityTypeToCreate = "item:binalRatChunk",
					Amount = new OutputAmount
					{
						NoOfItems = 2,
						Bulk = new Bulk
						{
							FractionOfInputBulk = 0.9f
						}
					}
				}
			},
			ProcessToolSetKey = "toolSetButcherFlesh",
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.0033333334f
			},
			AgentActionState = AnimAction.Butchering,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Cooking",
			KeyName = "makeBlackzpacho",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "cooking",
			PhysicalWorkFactor = 2f,
			Stances = kneelingProduction,
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:blackpulp",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 2
					}
				},
				new Input
				{
					Entity = "item:blackpulpEnzyme",
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
					EntityTypeToCreate = "item:blackzpacho",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					},
					ToolContainerTagsToPlaceIn = new string[1] { "cookingPot" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.0033333334f
			},
			ProcessToolSetKey = "toolSetMakeBlendedFood"
		});
		list.Add(new ProcessType
		{
			Name = "Cooking",
			KeyName = "makeTurnipRoast",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "cooking",
			PhysicalWorkFactor = 2f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:turnipMeat",
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
					EntityTypeToCreate = "item:turnipRoast",
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
			ProcessToolSetKey = "toolSetFireplace"
		});
		list.Add(new ProcessType
		{
			Name = "Cooking",
			KeyName = "makeTurnipRawSausage",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "cooking",
			PhysicalWorkFactor = 2f,
			Stances = standingProduction,
			WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart,
			Inputs = new Input[3]
			{
				new Input
				{
					Entity = "item:turnipMeat",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 3
					}
				},
				new Input
				{
					Entity = "item:turnipGuts",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:crystalBerries",
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
					EntityTypeToCreate = "item:turnipRawSausage",
					Amount = new OutputAmount
					{
						NoOfItems = 5
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			ProcessToolSetKey = "toolSetKitchen"
		});
		list.Add(new ProcessType
		{
			Name = "Cooking",
			KeyName = "makeTurnipSalami",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "cooking",
			PhysicalWorkFactor = 2f,
			Stances = standingProduction,
			WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:turnipRawSausage",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 5
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:turnipSalami",
					Amount = new OutputAmount
					{
						NoOfItems = 5
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 71f / (226f * (float)Math.PI)
			},
			ProcessToolSetKey = "toolSetSalamiDrying"
		});
		list.Add(new ProcessType
		{
			Name = "Cooking",
			KeyName = "makeTurnipFriedSausage",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "cooking",
			PhysicalWorkFactor = 2f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:turnipRawSausage",
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
					EntityTypeToCreate = "item:turnipFriedSausage",
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
			ProcessToolSetKey = "toolSetFireplace"
		});
		list.Add(new ProcessType
		{
			Name = "Cooking",
			KeyName = "makeRoastedStreakFin",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "cooking",
			PhysicalWorkFactor = 2f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:streakFin",
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
					EntityTypeToCreate = "item:roastedStreakFin",
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
			ProcessToolSetKey = "toolSetFireplace"
		});
		list.Add(new ProcessType
		{
			Name = "Cooking",
			KeyName = "makeRoastedCarbonTail",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "cooking",
			PhysicalWorkFactor = 2f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:carbonTail",
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
					EntityTypeToCreate = "item:roastedCarbonTail",
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
			ProcessToolSetKey = "toolSetFireplace"
		});
		list.Add(new ProcessType
		{
			Name = "Cooking",
			KeyName = "makeRoastedAlabasterRay",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "cooking",
			PhysicalWorkFactor = 2f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:alabasterRay",
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
					EntityTypeToCreate = "item:roastedAlabasterRay",
					Amount = new OutputAmount
					{
						NoOfItems = 2
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			ProcessToolSetKey = "toolSetFireplace"
		});
		list.Add(new ProcessType
		{
			Name = "Cooking",
			KeyName = "makeAlabasterStew",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "cooking",
			PhysicalWorkFactor = 2f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:alabasterRay",
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
					EntityTypeToCreate = "item:alabasterStew",
					Amount = new OutputAmount
					{
						NoOfItems = 2
					},
					ToolContainerTagsToPlaceIn = new string[1] { "cookingPot" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			ProcessToolSetKey = "toolSetMakeStew"
		});
		list.Add(new ProcessType
		{
			Name = "Cooking",
			KeyName = "makeGrilledUrsinix",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "cooking",
			PhysicalWorkFactor = 2f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:driedUrsinix",
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
					EntityTypeToCreate = "item:grilledUrsinix",
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
			ProcessToolSetKey = "toolSetFireplace"
		});
		list.Add(new ProcessType
		{
			Name = "Cooking",
			KeyName = "makeThunderChickenStew",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "cooking",
			PhysicalWorkFactor = 2f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:thunderChickenGuts",
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
					EntityTypeToCreate = "item:thunderChickenStew",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					},
					ToolContainerTypesToPlaceIn = new string[1] { "item:advancedCookingPot" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			ProcessToolSetKey = "toolSetMakeStew"
		});
		list.Add(new ProcessType
		{
			Name = "Cooking",
			KeyName = "makeThunderChickenSkewers",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "cooking",
			PhysicalWorkFactor = 2f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:thunderChickenMeat",
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
					EntityTypeToCreate = "item:thunderChickenSkewers",
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
			ProcessToolSetKey = "toolSetFireplace"
		});
		list.Add(new ProcessType
		{
			Name = "Cooking",
			KeyName = "makeRoastedPhantomWeaver",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "cooking",
			PhysicalWorkFactor = 2f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:phantomWeaver",
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
					EntityTypeToCreate = "item:roastedPhantomWeaver",
					Amount = new OutputAmount
					{
						NoOfItems = 2
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			ProcessToolSetKey = "toolSetFireplace"
		});
		list.Add(new ProcessType
		{
			Name = "Cooking",
			KeyName = "makeRoastedWebWing",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "cooking",
			PhysicalWorkFactor = 2f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:webWing",
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
					EntityTypeToCreate = "item:roastedWebWing",
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
			ProcessToolSetKey = "toolSetFireplace"
		});
		list.Add(new ProcessType
		{
			Name = "Cooking",
			KeyName = "makeRoastedCrestedFoiler",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "cooking",
			PhysicalWorkFactor = 2f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:crestedFoiler",
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
					EntityTypeToCreate = "item:roastedCrestedFoiler",
					Amount = new OutputAmount
					{
						NoOfItems = 2
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			ProcessToolSetKey = "toolSetFireplace"
		});
		list.Add(new ProcessType
		{
			Name = "Cooking",
			KeyName = "makeRoastedGoldenCenobite",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "cooking",
			PhysicalWorkFactor = 2f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:goldenCenobite",
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
					EntityTypeToCreate = "item:roastedGoldenCenobite",
					Amount = new OutputAmount
					{
						NoOfItems = 2
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			ProcessToolSetKey = "toolSetFireplace"
		});
		list.Add(new ProcessType
		{
			Name = "Cooking",
			KeyName = "makeRoastedMuckGrinder",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "cooking",
			PhysicalWorkFactor = 2f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:muckGrinder",
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
					EntityTypeToCreate = "item:roastedMuckGrinder",
					Amount = new OutputAmount
					{
						NoOfItems = 2
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			ProcessToolSetKey = "toolSetFireplace"
		});
		list.Add(new ProcessType
		{
			Name = "Cooking",
			KeyName = "makeRoastedCrazyDweller",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "cooking",
			PhysicalWorkFactor = 2f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:crazyDweller",
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
					EntityTypeToCreate = "item:roastedCrazyDweller",
					Amount = new OutputAmount
					{
						NoOfItems = 2
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			ProcessToolSetKey = "toolSetFireplace"
		});
		list.Add(new ProcessType
		{
			Name = "Cooking",
			KeyName = "makeRoastedDaggermouth",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "cooking",
			PhysicalWorkFactor = 2f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:daggermouth",
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
					EntityTypeToCreate = "item:roastedDaggermouth",
					Amount = new OutputAmount
					{
						NoOfItems = 3
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			ProcessToolSetKey = "toolSetFireplace"
		});
		list.Add(new ProcessType
		{
			Name = "Cooking",
			KeyName = "makeRoastedImpEel",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "cooking",
			PhysicalWorkFactor = 2f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:impEel",
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
					EntityTypeToCreate = "item:roastedImpEel",
					Amount = new OutputAmount
					{
						NoOfItems = 3
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			ProcessToolSetKey = "toolSetFireplace"
		});
		list.Add(new ProcessType
		{
			Name = "Cooking",
			KeyName = "makeMinnowSoup",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "cooking",
			PhysicalWorkFactor = 2f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:minnowsLive",
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
					EntityTypeToCreate = "item:minnowSoup",
					Amount = new OutputAmount
					{
						NoOfItems = 2
					},
					ToolContainerTagsToPlaceIn = new string[1] { "cookingPot" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			ProcessToolSetKey = "toolSetMakeStew"
		});
		list.Add(new ProcessType
		{
			Name = "Cooking",
			KeyName = "makeWaterCanePorridge",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "cooking",
			PhysicalWorkFactor = 2f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:waterCaneSeeds",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 2
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:waterCanePorridge",
					Amount = new OutputAmount
					{
						NoOfItems = 2
					},
					ToolContainerTagsToPlaceIn = new string[1] { "cookingPot" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			ProcessToolSetKey = "toolSetMakeStew"
		});
		list.Add(new ProcessType
		{
			Name = "Cooking",
			KeyName = "smokeThunderChicken",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "cooking",
			PhysicalWorkFactor = 2f,
			Stances = kneelingProduction,
			WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:thunderChickenMeat",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 6
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:smokedThunderChicken",
					Amount = new OutputAmount
					{
						NoOfItems = 6
					},
					ToolContainerTagsToPlaceIn = new string[1] { "smokeOven" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			ProcessToolSetKey = "toolSetSmokeOven"
		});
		list.Add(new ProcessType
		{
			Name = "Cooking",
			KeyName = "smokeAlabasterRay",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "cooking",
			PhysicalWorkFactor = 2f,
			Stances = kneelingProduction,
			WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:alabasterRay",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 6
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:smokedAlabasterRay",
					Amount = new OutputAmount
					{
						NoOfItems = 6
					},
					ToolContainerTagsToPlaceIn = new string[1] { "smokeOven" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			ProcessToolSetKey = "toolSetSmokeOven"
		});
		list.Add(new ProcessType
		{
			Name = "Cooking",
			KeyName = "smokeStreakFin",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "cooking",
			PhysicalWorkFactor = 2f,
			Stances = kneelingProduction,
			WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:streakFin",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 6
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:smokedStreakFin",
					Amount = new OutputAmount
					{
						NoOfItems = 6
					},
					ToolContainerTagsToPlaceIn = new string[1] { "smokeOven" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			ProcessToolSetKey = "toolSetSmokeOven"
		});
		list.Add(new ProcessType
		{
			Name = "Cooking",
			KeyName = "smokeCarbonTail",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "cooking",
			PhysicalWorkFactor = 2f,
			Stances = kneelingProduction,
			WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:carbonTail",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 6
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:smokedCarbonTail",
					Amount = new OutputAmount
					{
						NoOfItems = 6
					},
					ToolContainerTagsToPlaceIn = new string[1] { "smokeOven" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			ProcessToolSetKey = "toolSetSmokeOven"
		});
		list.Add(new ProcessType
		{
			Name = "Cooking",
			KeyName = "smokeTurnip",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "cooking",
			PhysicalWorkFactor = 2f,
			Stances = kneelingProduction,
			WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:turnipMeat",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 6
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:smokedTurnip",
					Amount = new OutputAmount
					{
						NoOfItems = 6
					},
					ToolContainerTagsToPlaceIn = new string[1] { "smokeOven" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			ProcessToolSetKey = "toolSetSmokeOven"
		});
		list.Add(new ProcessType
		{
			Name = "Cooking",
			KeyName = "makeHardtack",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "cooking",
			PhysicalWorkFactor = 4f,
			WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:glassyCreeperPods",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 6
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:hardtack",
					Amount = new OutputAmount
					{
						NoOfItems = 6
					},
					ToolContainerTagsToPlaceIn = new string[1] { "kiln" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 75f
			},
			ProcessToolSetKey = "toolSetOven",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Cooking",
			KeyName = "makeDriedThunderChicken",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 2f,
			Stances = standingProduction,
			WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart,
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:thunderChickenMeat",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 6
					}
				},
				new Input
				{
					Entity = "item:salt",
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
					EntityTypeToCreate = "item:driedThunderChicken",
					Amount = new OutputAmount
					{
						NoOfItems = 6
					},
					ToolContainerTagsToPlaceIn = new string[1] { "meatDryingRack" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 71f / (226f * (float)Math.PI)
			},
			ProcessToolSetKey = "toolSetMeatDrying",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Cooking",
			KeyName = "makeDriedStreakFin",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 2f,
			Stances = standingProduction,
			WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart,
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:streakFin",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 10
					}
				},
				new Input
				{
					Entity = "item:salt",
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
					EntityTypeToCreate = "item:driedSaltedStreakFin",
					Amount = new OutputAmount
					{
						NoOfItems = 10
					},
					ToolContainerTagsToPlaceIn = new string[1] { "meatDryingRack" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 71f / (226f * (float)Math.PI)
			},
			ProcessToolSetKey = "toolSetMeatDrying",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Cooking",
			KeyName = "makeDesalinatedStreakFin",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "cooking",
			PhysicalWorkFactor = 2f,
			WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:driedSaltedStreakFin",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 5
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:desalinatedStreakFin",
					Amount = new OutputAmount
					{
						NoOfItems = 5
					},
					ToolContainerTagsToPlaceIn = new string[1] { "liquidContainerNoHeat" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			ProcessToolSetKey = "toolSetJarNoHeating",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Cooking",
			KeyName = "makeHexapineSalad",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "cooking",
			PhysicalWorkFactor = 2f,
			Stances = kneelingProduction,
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:hexapineLeaves",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 2
					}
				},
				new Input
				{
					Entity = "item:hexapineLeavesEnzyme",
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
					EntityTypeToCreate = "item:hexapineSalad",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					},
					ToolContainerTagsToPlaceIn = new string[1] { "cookingPot" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.0033333334f
			},
			ProcessToolSetKey = "toolSetMakeBlendedFood"
		});
		list.Add(new ProcessType
		{
			Name = "Cooking",
			KeyName = "makeFingerPot",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "cooking",
			PhysicalWorkFactor = 2f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:fingerFruit",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 2
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:fingerPot",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					},
					ToolContainerTagsToPlaceIn = new string[1] { "cookingPot" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.0033333334f
			},
			ProcessToolSetKey = "toolSetMakeStew"
		});
		list.Add(new ProcessType
		{
			Name = "Cooking",
			KeyName = "makeFermentedFingerFruit",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "cooking",
			PhysicalWorkFactor = 2f,
			WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart,
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:fingerFruit",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 2
					}
				},
				new Input
				{
					Entity = "item:salt",
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
					EntityTypeToCreate = "item:fermentedFingerFruit",
					Amount = new OutputAmount
					{
						NoOfItems = 2
					},
					ToolContainerTagsToPlaceIn = new string[1] { "liquidContainerNoHeat" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 71f / (226f * (float)Math.PI)
			},
			ProcessToolSetKey = "toolSetFermentWithKitchenAndJar",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Cooking",
			KeyName = "makePickledCarbonTail",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "cooking",
			PhysicalWorkFactor = 2f,
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:carbonTail",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 6
					}
				},
				new Input
				{
					Entity = "item:vinegar",
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
					EntityTypeToCreate = "item:pickledCarbonTail",
					Amount = new OutputAmount
					{
						NoOfItems = 6
					},
					ToolContainerTagsToPlaceIn = new string[1] { "liquidContainerNoHeat" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			ProcessToolSetKey = "toolSetFermentWithKitchenAndJar",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Cooking",
			KeyName = "makePickledAlabasterRay",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "cooking",
			PhysicalWorkFactor = 2f,
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:alabasterRay",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 2
					}
				},
				new Input
				{
					Entity = "item:vinegar",
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
					EntityTypeToCreate = "item:pickledAlabasterRay",
					Amount = new OutputAmount
					{
						NoOfItems = 3
					},
					ToolContainerTagsToPlaceIn = new string[1] { "liquidContainerNoHeat" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			ProcessToolSetKey = "toolSetFermentWithKitchenAndJar",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Cooking",
			KeyName = "makeVinegar",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "cooking",
			PhysicalWorkFactor = 2f,
			WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:crystalBerries",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 2
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:vinegar",
					Amount = new OutputAmount
					{
						NoOfItems = 2
					},
					ToolContainerTagsToPlaceIn = new string[1] { "liquidContainerNoHeat" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 71f / (226f * (float)Math.PI)
			},
			ProcessToolSetKey = "toolSetMakeVinegar",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeCrystalWine",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "cooking",
			PhysicalWorkFactor = 2f,
			WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:crystalBerries",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 2
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:crystalWine",
					Amount = new OutputAmount
					{
						NoOfItems = 2
					},
					ToolContainerTagsToPlaceIn = new string[1] { "liquidContainerNoHeat" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 71f / (226f * (float)Math.PI)
			},
			ProcessToolSetKey = "toolSetFermentWithKitchenAndJar",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeCrystalBrandy",
			RequiredSkill = "cooking",
			JobTypeKey = "cookingJobType",
			PhysicalWorkFactor = 2f,
			WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:crystalWine",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 4
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:crystalBrandy",
					Amount = new OutputAmount
					{
						NoOfItems = 2
					},
					ToolContainerTagsToPlaceIn = new string[1] { "liquidContainerNoHeat" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			ProcessToolSetKey = "toolSetMakeBrandy"
		});
		list.Add(new ProcessType
		{
			Name = "Cooking",
			KeyName = "makeClamwichSoup",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "cooking",
			PhysicalWorkFactor = 2f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:clamwich",
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
					EntityTypeToCreate = "item:clamwichSoup",
					Amount = new OutputAmount
					{
						NoOfItems = 2
					},
					ToolContainerTagsToPlaceIn = new string[1] { "cookingPot" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			ProcessToolSetKey = "toolSetMakeStew"
		});
		list.Add(new ProcessType
		{
			Name = "Cooking",
			KeyName = "makeBakedTorux",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "cooking",
			PhysicalWorkFactor = 2f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:torux",
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
					EntityTypeToCreate = "item:bakedTorux",
					Amount = new OutputAmount
					{
						NoOfItems = 2
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			ProcessToolSetKey = "toolSetFireplace"
		});
		list.Add(new ProcessType
		{
			Name = "Cooking",
			KeyName = "makeGrubGrub",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "cooking",
			PhysicalWorkFactor = 2f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:scampGrub",
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
					EntityTypeToCreate = "item:grubGrub",
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
			ProcessToolSetKey = "toolSetFireplace"
		});
		list.Add(new ProcessType
		{
			Name = "Cooking",
			KeyName = "makeBakedCommonOilTubers",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "cooking",
			PhysicalWorkFactor = 2f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:commonOilTubers",
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
					EntityTypeToCreate = "item:bakedCommonOilTubers",
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
			ProcessToolSetKey = "toolSetFireplace"
		});
		list.Add(new ProcessType
		{
			Name = "Cooking",
			KeyName = "makeMashedCommonOilTubers",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "cooking",
			PhysicalWorkFactor = 2f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:commonOilTubers",
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
					EntityTypeToCreate = "item:mashedCommonOilTubers",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					},
					ToolContainerTagsToPlaceIn = new string[1] { "cookingPot" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.0033333334f
			},
			ProcessToolSetKey = "toolSetMakeStew"
		});
		list.Add(new ProcessType
		{
			Name = "Cooking",
			KeyName = "makeGlassyPorridge",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "cooking",
			PhysicalWorkFactor = 2f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:glassyCreeperPods",
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
					EntityTypeToCreate = "item:glassyPorridge",
					Amount = new OutputAmount
					{
						NoOfItems = 2
					},
					ToolContainerTagsToPlaceIn = new string[1] { "cookingPot" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			ProcessToolSetKey = "toolSetMakeStew"
		});
		list.Add(new ProcessType
		{
			Name = "Cooking",
			KeyName = "makePowderedCrystalBerries",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "cooking",
			PhysicalWorkFactor = 2f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:crystalBerries",
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
					EntityTypeToCreate = "item:powderedCrystalBerries",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					},
					ToolContainerTagsToPlaceIn = new string[1] { "cookingPot" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.0033333334f
			},
			ProcessToolSetKey = "toolSetMakeBlendedFood"
		});
		list.Add(new ProcessType
		{
			Name = "Cooking",
			KeyName = "makeSimCoffee",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "cooking",
			PhysicalWorkFactor = 2f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:simCoffeeBeans",
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
					EntityTypeToCreate = "item:simCoffee",
					Amount = new OutputAmount
					{
						NoOfItems = 4
					},
					ToolContainerTagsToPlaceIn = new string[1] { "cookingPot" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			ProcessToolSetKey = "toolSetMakeStew"
		});
		list.Add(new ProcessType
		{
			Name = "Synthesizing",
			KeyName = "makeHexapineLeavesEnzyme",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "biology",
			PhysicalWorkFactor = 2f,
			Stances = sittingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:hexapineLeaves",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[2]
			{
				new Output
				{
					EntityTypeToCreate = "item:hexapineLeavesEnzyme",
					Amount = new OutputAmount
					{
						NoOfItems = 4
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:hexapineLeaves",
					IsWasteProduct = true,
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
			ProcessToolSetKey = "toolSetMakeEnzyme"
		});
		list.Add(new ProcessType
		{
			Name = "Synthesizing",
			KeyName = "makeBlackpulpEnzyme",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "biology",
			PhysicalWorkFactor = 2f,
			Stances = sittingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:blackpulp",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[2]
			{
				new Output
				{
					EntityTypeToCreate = "item:blackpulpEnzyme",
					Amount = new OutputAmount
					{
						NoOfItems = 4
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:blackpulp",
					IsWasteProduct = true,
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
			ProcessToolSetKey = "toolSetMakeEnzyme"
		});
		list.Add(new ProcessType
		{
			Name = "Synthesizing",
			KeyName = "makeSpottedOilTuberEnzyme",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "biology",
			PhysicalWorkFactor = 2f,
			Stances = sittingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:spottedOilTubers",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[2]
			{
				new Output
				{
					EntityTypeToCreate = "item:spottedOilTuberEnzyme",
					Amount = new OutputAmount
					{
						NoOfItems = 4
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:spottedOilTubers",
					IsWasteProduct = true,
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
			ProcessToolSetKey = "toolSetMakeEnzyme"
		});
		list.Add(new ProcessType
		{
			Name = "Cooking",
			KeyName = "makeMashedSpottedOilTubers",
			JobTypeKey = "cookingJobType",
			RequiredSkill = "cooking",
			PhysicalWorkFactor = 2f,
			Stances = kneelingProduction,
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:spottedOilTubers",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 2
					}
				},
				new Input
				{
					Entity = "item:spottedOilTuberEnzyme",
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
					EntityTypeToCreate = "item:mashedSpottedOilTubers",
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
			ProcessToolSetKey = "toolSetMakeStew"
		});
		list.Add(new ProcessType
		{
			Name = "Make twinkler pheromone",
			KeyName = "makeTwinklerPheromone",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "biology",
			PhysicalWorkFactor = 2f,
			Stances = sittingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:twinklerMeat",
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
					EntityTypeToCreate = "item:twinklerPheromone",
					Amount = new OutputAmount
					{
						NoOfItems = 2
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.0033333334f
			},
			ProcessToolSetKey = "toolSetMakeEnzyme"
		});
		float value2 = 6f;
		SerializableDictionary<string, ChanceToTakeStance[]> stances = kneelingOrStandingProduction;
		list.Add(new ProcessType
		{
			Name = "Salvage",
			SummaryDescription = "When breaking this object apart, some parts will be retrieved and some may be lost.",
			KeyName = "salvagePeatStack",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:peatStack",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.00033333336f
			},
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Salvage",
			SummaryDescription = "When breaking this object apart, some parts will be retrieved and some may be lost.",
			KeyName = "salvageFirewoodStack",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:firewoodStack",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.00033333336f
			},
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Salvage",
			SummaryDescription = "When breaking this object apart, some parts will be retrieved and some may be lost.",
			KeyName = "salvageCompostPit",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			Stances = kneelingOrStandingProduction,
			IsSalvageProcess = true,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:compostPit",
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
					EntityTypeToCreate = "item:stones",
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
			AgentActionState = AnimAction.Digging
		});
		list.Add(new ProcessType
		{
			Name = "Salvage",
			SummaryDescription = "When breaking this object apart, some parts will be retrieved and some may be lost.",
			KeyName = "salvageCompostBin",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:compostBin",
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
					EntityTypeToCreate = "item:sticks",
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
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Salvage",
			SummaryDescription = "When breaking this object apart, some parts will be retrieved and some may be lost.",
			KeyName = "salvageMeatDryingRack",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:meatDryingRack",
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
					EntityTypeToCreate = "item:sticks",
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
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Salvage",
			SummaryDescription = "When breaking this object apart, some parts will be retrieved and some may be lost.",
			KeyName = "salvageHideRack",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:hideRack",
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
					EntityTypeToCreate = "item:sticks",
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
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Pack down",
			SummaryDescription = "This object can be packed down and set up repeatedly without losing any parts.",
			KeyName = "salvageRareMetalRefinery",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:rareMetalRefinery",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[2]
			{
				new Output
				{
					EntityTypeToCreate = "item:metalRefineryEquipment",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:metalRefineryPart1",
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
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Salvage",
			SummaryDescription = "When breaking this object apart, some parts will be retrieved and some may be lost.",
			KeyName = "salvageImprovisedGreenhouse",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:improvisedGreenhouse",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[2]
			{
				new Output
				{
					EntityTypeToCreate = "item:shadeleafCanes",
					Amount = new OutputAmount
					{
						NoOfItems = 2
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:improvisedGreenHouseCover",
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
			SummaryDescription = "When breaking this object apart, some parts will be retrieved and some may be lost.",
			KeyName = "salvageSimpleSmithy",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:simpleSmithy",
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
					EntityTypeToCreate = "item:solidMudBrick",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:anvil",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:barClamps",
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
			SummaryDescription = "When breaking this object apart, some parts will be retrieved and some may be lost.",
			KeyName = "salvageSpikeTrap",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:spikeTrap",
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
					EntityTypeToCreate = "item:spikeTrap",
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
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Salvage",
			SummaryDescription = "When breaking this object apart, some parts will be retrieved and some may be lost.",
			KeyName = "salvageDryingShed",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:dryingShed",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[2]
			{
				new Output
				{
					EntityTypeToCreate = "item:solidMudBrick",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:shadeleafCanes",
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
			SummaryDescription = "When breaking this object apart, some parts will be retrieved and some may be lost.",
			KeyName = "salvageToolshed",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:toolshed",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[2]
			{
				new Output
				{
					EntityTypeToCreate = "item:solidMudBrick",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:waterCaneStem",
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
			SummaryDescription = "When breaking this object apart, some parts will be retrieved and some may be lost.",
			KeyName = "salvageClayGranary",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:clayGranary",
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
					EntityTypeToCreate = "item:solidMudBrick",
					Amount = new OutputAmount
					{
						NoOfItems = 2
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
			SummaryDescription = "When breaking this object apart, some parts will be retrieved and some may be lost.",
			KeyName = "salvageCaneHut",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:caneHut",
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
					EntityTypeToCreate = "item:waterCaneStem",
					Amount = new OutputAmount
					{
						NoOfItems = 3
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
			SummaryDescription = "When breaking this object apart, some parts will be retrieved and some may be lost.",
			KeyName = "salvageClayHut",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:clayHut",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[2]
			{
				new Output
				{
					EntityTypeToCreate = "item:solidMudBrick",
					Amount = new OutputAmount
					{
						NoOfItems = 2
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:spoakBranchesTrimmed",
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
			SummaryDescription = "When breaking this object apart, some parts will be retrieved and some may be lost.",
			KeyName = "salvageTurnipHut",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:turnipHut",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[2]
			{
				new Output
				{
					EntityTypeToCreate = "item:turnipShell",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:sticks",
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
			SummaryDescription = "When breaking this object apart, some parts will be retrieved and some may be lost.",
			KeyName = "salvageImprovisedSmithy",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:improvisedSmithy",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[2]
			{
				new Output
				{
					EntityTypeToCreate = "item:solidMudBrick",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:stones",
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
			SummaryDescription = "When breaking this object apart, some parts will be retrieved and some may be lost.",
			KeyName = "salvageKiln",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:kiln",
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
					EntityTypeToCreate = "item:stones",
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
			SummaryDescription = "When breaking this object apart, some parts will be retrieved and some may be lost.",
			KeyName = "salvageKilnImprovisedSmall",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:kilnImprovisedSmall",
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
					EntityTypeToCreate = "item:stones",
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
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Salvage",
			SummaryDescription = "When breaking this object apart, some parts will be retrieved and some may be lost.",
			KeyName = "salvageGoldFurnace",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:goldFurnace",
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
					EntityTypeToCreate = "item:firebricks",
					Amount = new OutputAmount
					{
						NoOfItems = 2
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
			Name = "Removing",
			KeyName = "salvagePolymerWorkshopUpgrade",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:polymerWorkshopUpgrade",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[2]
			{
				new Output
				{
					EntityTypeToCreate = "item:extrusionMachineComponents",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:solidMudBrick",
					Amount = new OutputAmount
					{
						NoOfItems = 3
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
			Name = "Removing",
			KeyName = "salvageCarpenterWorkshopUpgrade",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:carpenterWorkshopUpgrade",
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
					EntityTypeToCreate = "item:solidMudBrick",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:barClamps",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:sticks",
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
			Name = "Removing",
			KeyName = "salvageTextileWorkshopUpgrade",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:textileWorkshopUpgrade",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[2]
			{
				new Output
				{
					EntityTypeToCreate = "item:loomComponents",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:waterCaneStem",
					Amount = new OutputAmount
					{
						NoOfItems = 4
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
			Name = "Removing",
			KeyName = "salvageMetalLatheShopHumanPoweredUpgrade",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:metalLatheShopHumanPoweredUpgrade",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[2]
			{
				new Output
				{
					EntityTypeToCreate = "item:metalLatheComponents",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:humanPowerUnit",
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
			Name = "Removing",
			KeyName = "salvageWorkshopBuilding",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:workshopBuilding",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[2]
			{
				new Output
				{
					EntityTypeToCreate = "item:solidMudBrick",
					Amount = new OutputAmount
					{
						NoOfItems = 2
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:waterCaneStem",
					Amount = new OutputAmount
					{
						NoOfItems = 2
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
			Name = "Removing",
			KeyName = "salvageCookhouse",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:cookhouse",
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
					EntityTypeToCreate = "item:solidMudBrick",
					Amount = new OutputAmount
					{
						NoOfItems = 2
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:waterCaneStem",
					Amount = new OutputAmount
					{
						NoOfItems = 2
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:textile",
					Amount = new OutputAmount
					{
						NoOfItems = 2
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:spoakShingles",
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
			SummaryDescription = "When breaking this object apart, some parts will be retrieved and some may be lost.",
			KeyName = "salvageStill",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:still",
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
					EntityTypeToCreate = "item:stillComponents",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:stones",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:clayJar",
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
			SummaryDescription = "When breaking this object apart, some parts will be retrieved and some may be lost.",
			KeyName = "salvageRadioHutImprovised",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:radioHutImprovised",
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
					EntityTypeToCreate = "item:radio",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:radioAntenna",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:shadeleafCanes",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:spoakLeaves",
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
			SummaryDescription = "When breaking this object apart, some parts will be retrieved and some may be lost.",
			KeyName = "salvageRadioHut",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:radioHut",
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
					EntityTypeToCreate = "item:radio",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:radioAntenna",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:shadeleafCanes",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:spoakShingles",
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
			Name = "Disassemble",
			SummaryDescription = "Disassemble",
			KeyName = "salvageDeadfallTrap",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:deadfallTrap",
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
					EntityTypeToCreate = "item:stones",
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
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Disassemble",
			SummaryDescription = "Disassemble",
			KeyName = "salvageSpringSnare",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:springSnare",
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
					EntityTypeToCreate = "item:shadeleafCanes",
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
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Disassemble",
			SummaryDescription = "Disassemble",
			KeyName = "salvageLandMine",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:landMine",
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
					EntityTypeToCreate = "item:landMine",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Pack down",
			SummaryDescription = "This object can be packed down and set up repeatedly without losing any parts.",
			KeyName = "salvageSensor",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 2f,
			IsSalvageProcess = true,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:sensor",
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
					EntityTypeToCreate = "item:sensor",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.00033333336f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Pack down",
			SummaryDescription = "This object can be packed down and set up repeatedly without losing any parts.",
			KeyName = "salvageSentry",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 2f,
			IsSalvageProcess = true,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:sentry",
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
					EntityTypeToCreate = "item:sentry",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.00033333336f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Pack down",
			SummaryDescription = "This object can be packed down and set up repeatedly without losing any parts.",
			KeyName = "salvageSprayGunSentry",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 2f,
			IsSalvageProcess = true,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:sprayGunSentry",
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
					EntityTypeToCreate = "item:spraySentry",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.00033333336f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Salvage",
			SummaryDescription = "Salvage",
			KeyName = "salvageScarecrow",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 2f,
			IsSalvageProcess = true,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:scarecrow",
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
					EntityTypeToCreate = "item:twinklerPlating",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.00033333336f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Salvage",
			SummaryDescription = "Salvage",
			KeyName = "salvageSimplePort",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:simplePort",
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
					EntityTypeToCreate = "item:waterCaneStem",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:spoakBranchesTrimmed",
					Amount = new OutputAmount
					{
						NoOfItems = 2
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:spoakShingles",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:solidMudBrick",
					Amount = new OutputAmount
					{
						NoOfItems = 3
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
			SummaryDescription = "Salvage",
			KeyName = "salvageCanopyPort",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:canopyPort",
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
					EntityTypeToCreate = "item:daysheenLeaves",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:waterCaneStem",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:spoakBranchesTrimmed",
					Amount = new OutputAmount
					{
						NoOfItems = 2
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
			SummaryDescription = "Salvage",
			KeyName = "salvageLandingImprovised",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:landingImprovised",
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
					EntityTypeToCreate = "item:waterCaneStem",
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
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Disassemble",
			SummaryDescription = "This object can be disassembled without losing any parts.",
			KeyName = "salvageHelipadBig",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = stances,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:helipadBig",
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
					EntityTypeToCreate = "item:structurePanels",
					Amount = new OutputAmount
					{
						NoOfItems = 2
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			AgentActionState = AnimAction.Salvaging
		});
		list.Add(new ProcessType
		{
			Name = "Disassemble",
			SummaryDescription = "This object can be disassembled without losing any parts.",
			KeyName = "salvageHelipad",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:helipad",
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
					EntityTypeToCreate = "item:stones",
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
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Pack down",
			SummaryDescription = "This object can be packed down and set up repeatedly without losing any parts.",
			KeyName = "salvageSatelliteGroundStation",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 2f,
			IsSalvageProcess = true,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:satelliteGroundStation",
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
					EntityTypeToCreate = "item:satelliteGroundStation",
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
			AgentActionState = AnimAction.Salvaging
		});
		list.Add(new ProcessType
		{
			Name = "Pack down",
			SummaryDescription = "This object can be packed down and set up repeatedly without losing any parts.",
			KeyName = "salvageWeatherStation",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = stances,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:weatherStation",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[2]
			{
				new Output
				{
					EntityTypeToCreate = "item:weatherStationMast",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:weatherStationSensors",
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
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Pack down",
			SummaryDescription = "This object can be packed down and set up repeatedly without losing any parts.",
			KeyName = "salvageMolecularAssembler",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = stances,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:molecularAssembler",
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
					EntityTypeToCreate = "item:vacuumChamber",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:assemblerCabinet",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:assemblerCooling",
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
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Pack down",
			SummaryDescription = "This object can be packed down and set up repeatedly without losing any parts.",
			KeyName = "salvageSmallTent",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = stances,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:smallTent",
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
					EntityTypeToCreate = "item:smallTent",
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
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Pack down",
			SummaryDescription = "This object can be packed down and set up repeatedly without losing any parts.",
			KeyName = "salvageOctagonalTent",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = stances,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:octagonalTent",
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
					EntityTypeToCreate = "item:octagonalTent",
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
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Pack down",
			SummaryDescription = "This object can be packed down and set up repeatedly without losing any parts.",
			KeyName = "salvageDomeTent",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = stances,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:domeTent",
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
					EntityTypeToCreate = "item:domeTent",
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
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Disassemble",
			SummaryDescription = "This object can be disassembled without losing any parts.",
			KeyName = "salvageStorageHole",
			RequiredSkill = "menial",
			PhysicalWorkFactor = value2,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:storageHole",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[2]
			{
				new Output
				{
					EntityTypeToCreate = "item:spoakLeaves",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:stones",
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
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Disassemble",
			SummaryDescription = "This object can be disassembled without losing any parts.",
			KeyName = "salvageCooledFoodCache",
			RequiredSkill = "menial",
			PhysicalWorkFactor = value2,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:cooledFoodCache",
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
					EntityTypeToCreate = "item:spoakLeaves",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:stones",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:inactivatedFoodCoolerUnit",
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
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Disassemble",
			SummaryDescription = "This object can be disassembled without losing any parts.",
			KeyName = "salvageSmokeOven",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:smokeOven",
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
					EntityTypeToCreate = "item:sticks",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:stones",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:firegrassSod",
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
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Disassemble",
			SummaryDescription = "This object can be disassembled without losing any parts.",
			KeyName = "salvageCampfire",
			PhysicalWorkFactor = 4f,
			RequiredSkill = "menial",
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:campfire",
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
					EntityTypeToCreate = "item:stones",
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
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Disassemble",
			SummaryDescription = "This object can be disassembled without losing any parts.",
			KeyName = "salvageFieldKitchen",
			PhysicalWorkFactor = 4f,
			RequiredSkill = "menial",
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:fieldKitchen",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[2]
			{
				new Output
				{
					EntityTypeToCreate = "item:fieldKitchenStove",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:fieldKitchenEquipment",
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
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Pack down",
			SummaryDescription = "This object can be packed down and set up repeatedly without losing any parts.",
			KeyName = "salvageFieldLab",
			PhysicalWorkFactor = 2f,
			RequiredSkill = "menial",
			IsSalvageProcess = true,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:fieldLab",
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
					EntityTypeToCreate = "item:fieldLabPacked",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Salvage",
			SummaryDescription = "When breaking this object apart, some parts will be retrieved and some may be lost.",
			KeyName = "salvageA-frameTarp",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:A-frameTarp",
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
					EntityTypeToCreate = "item:sticks",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:wingweedLeaves",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:thermalTarp",
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
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Salvage",
			SummaryDescription = "When breaking this object apart, some parts will be retrieved and some may be lost.",
			KeyName = "salvageA-frameSpoakLeaves",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:A-frameSpoakLeaves",
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
					EntityTypeToCreate = "item:spoakBranchesTrimmed",
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
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Salvage",
			SummaryDescription = "When breaking this object apart, some parts will be retrieved and some may be lost.",
			KeyName = "salvageA-frameScraps",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:A-frameScraps",
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
					EntityTypeToCreate = "item:sticks",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:seatCushions",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:panelScraps",
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
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Salvage",
			SummaryDescription = "When breaking this object apart, some parts will be retrieved and some may be lost.",
			KeyName = "salvageLean-toTarp",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:lean-toTarp",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[2]
			{
				new Output
				{
					EntityTypeToCreate = "item:sticks",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:thermalTarp",
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
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Salvage",
			SummaryDescription = "When breaking this object apart, some parts will be retrieved and some may be lost.",
			KeyName = "salvageLean-toSpoakLeaves",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:lean-toSpoakLeaves",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[2]
			{
				new Output
				{
					EntityTypeToCreate = "item:sticks",
					Amount = new OutputAmount
					{
						NoOfItems = 2
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:spoakLeaves",
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
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Salvage",
			SummaryDescription = "When breaking this object apart, some parts will be retrieved and some may be lost.",
			KeyName = "salvageLean-toScraps",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:lean-toScraps",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[2]
			{
				new Output
				{
					EntityTypeToCreate = "item:sticks",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:panelScraps",
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
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Salvage",
			SummaryDescription = "When breaking this object apart, some parts will be retrieved and some may be lost.",
			KeyName = "salvageDaysheenTipi",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:daysheenTipi",
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
					EntityTypeToCreate = "item:daysheenLeaves",
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
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Salvage",
			SummaryDescription = "When breaking this object apart, some parts will be retrieved and some may be lost.",
			KeyName = "salvageDomeShelterTarp",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:domeShelterTarp",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[2]
			{
				new Output
				{
					EntityTypeToCreate = "item:thermalTarp",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:shadeleafCanes",
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
			SummaryDescription = "When breaking this object apart, some parts will be retrieved and some may be lost.",
			KeyName = "salvageDomeShelterSpoakShingles",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:domeShelterSpoakShingles",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[2]
			{
				new Output
				{
					EntityTypeToCreate = "item:spoakShingles",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:shadeleafCanes",
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
			SummaryDescription = "When breaking this object apart, some parts will be retrieved and some may be lost.",
			KeyName = "salvageWigwamSpoakShingles",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:wigwamSpoakShingles",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[2]
			{
				new Output
				{
					EntityTypeToCreate = "item:spoakShingles",
					Amount = new OutputAmount
					{
						NoOfItems = 3
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:spoakBranchesTrimmed",
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
			SummaryDescription = "When breaking this object apart, some parts will be retrieved and some may be lost.",
			KeyName = "salvageImprovisedKitchen",
			PhysicalWorkFactor = 4f,
			RequiredSkill = "menial",
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:improvisedKitchen",
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
					EntityTypeToCreate = "item:panelScraps",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:sticks",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:spoakShingles",
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
			SummaryDescription = "When breaking this object apart, some parts will be retrieved and some may be lost.",
			KeyName = "salvageMudBrickKitchen",
			PhysicalWorkFactor = 4f,
			RequiredSkill = "menial",
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:mudBrickKitchen",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[2]
			{
				new Output
				{
					EntityTypeToCreate = "item:sticks",
					Amount = new OutputAmount
					{
						NoOfItems = 2
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:spoakShingles",
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
			SummaryDescription = "When breaking this object apart, some parts will be retrieved and some may be lost.",
			KeyName = "salvageImprovisedWorkbench",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:improvisedWorkbench",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[2]
			{
				new Output
				{
					EntityTypeToCreate = "item:panelScraps",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:sticks",
					Amount = new OutputAmount
					{
						NoOfItems = 2
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
			SummaryDescription = "When breaking this object apart, some parts will be retrieved and some may be lost.",
			KeyName = "salvageMudBrickWorkbench",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:mudBrickWorkbench",
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
					EntityTypeToCreate = "item:sticks",
					Amount = new OutputAmount
					{
						NoOfItems = 2
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
			SummaryDescription = "When breaking this object apart, some parts will be retrieved and some may be lost.",
			KeyName = "salvageAbatis1",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:abatis",
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
					EntityTypeToCreate = "item:spoakBranches",
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
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Clear away",
			SummaryDescription = "Remove all traces of this structure.",
			KeyName = "salvageFavorbreadFarm",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:favorbreadFarm",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Clear away",
			SummaryDescription = "Remove all traces of this structure.",
			KeyName = "salvageClayPit",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:clayPit",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Clear away",
			SummaryDescription = "Remove all traces of this structure.",
			KeyName = "salvageSaltMine",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:saltMine",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Clear away",
			SummaryDescription = "Remove all traces of this structure.",
			KeyName = "salvageBogOrePit",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:bogOrePit",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Clear away",
			SummaryDescription = "Remove all traces of this structure.",
			KeyName = "salvageRareMetalorePit1",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:rareMetalOrePit1",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Clear away",
			SummaryDescription = "Remove all traces of this structure.",
			KeyName = "salvageRareMetalorePit2",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:rareMetalOrePit2",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Clear away",
			SummaryDescription = "Remove all traces of this structure.",
			KeyName = "salvagePeatBank",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:peatBank",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Salvage",
			SummaryDescription = "When breaking this object apart, some parts will be retrieved and some may be lost.",
			KeyName = "salvageFishTrapCreekSticks",
			PhysicalWorkFactor = 4f,
			RequiredSkill = "menial",
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:fishTrapCreekSticks",
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
					EntityTypeToCreate = "item:sticks",
					Amount = new OutputAmount
					{
						NoOfItems = 2
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
			SummaryDescription = "When breaking this object apart, some parts will be retrieved and some may be lost.",
			KeyName = "salvageFishTrapCreekNet",
			PhysicalWorkFactor = 4f,
			RequiredSkill = "menial",
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:fishTrapCreekNet",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[2]
			{
				new Output
				{
					EntityTypeToCreate = "item:fishingNet",
					Amount = new OutputAmount
					{
						NoOfItems = 2
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:sticks",
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
			Name = "Pack down",
			SummaryDescription = "This object can be packed down and set up repeatedly without losing any parts.",
			KeyName = "salvageFishTrapCoast",
			PhysicalWorkFactor = 4f,
			RequiredSkill = "menial",
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:fishTrapCoast",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[2]
			{
				new Output
				{
					EntityTypeToCreate = "item:fishTrapHoopNet",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:fishingNet",
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
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Pack down",
			SummaryDescription = "This object can be packed down and set up repeatedly without losing any parts.",
			KeyName = "salvageFishTrapShoreBasket",
			PhysicalWorkFactor = 4f,
			RequiredSkill = "menial",
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:fishTrapShoreBasket",
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
					EntityTypeToCreate = "item:fishTrapBasket",
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
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Pack down",
			SummaryDescription = "This object can be packed down and set up repeatedly without losing any parts.",
			KeyName = "salvageFishTrapShoreHoopNet",
			PhysicalWorkFactor = 4f,
			RequiredSkill = "menial",
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:fishTrapShoreHoopNet",
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
					EntityTypeToCreate = "item:fishTrapHoopNet",
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
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Clear away",
			SummaryDescription = "Remove all traces of this structure.",
			KeyName = "salvageSmallPlot",
			PhysicalWorkFactor = 4f,
			RequiredSkill = "menial",
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:smallPlot",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null,
			ProcessToolSetKey = "toolSetUnPlowingTools"
		});
		list.Add(new ProcessType
		{
			Name = "Clear away",
			SummaryDescription = "Remove all traces of this structure.",
			KeyName = "salvageLargePlot",
			PhysicalWorkFactor = 4f,
			RequiredSkill = "menial",
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "structure:largePlot",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null,
			ProcessToolSetKey = "toolSetUnPlowingTools"
		});
		list.Add(new ProcessType
		{
			Name = "Use blackpulp bait",
			KeyName = "changeBaitToBlackpulp",
			JobTypeKey = "checkTrapsJobType",
			RequiredSkill = "menial",
			SummaryDescription = "This type of food will be brought to the trap when inspected",
			Description = "Even with no bait option chosen, we will make sure to inspect the trap regularly, and, in case it has been sprung, reset it.",
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:blackpulp",
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
					EntityTypeToCreate = "item:blackpulp",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					},
					IsWasteProduct = true
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.00033333336f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Use glassy creeper bait",
			KeyName = "changeBaitToGlassyCreeper",
			JobTypeKey = "checkTrapsJobType",
			RequiredSkill = "menial",
			SummaryDescription = "This type of food will be brought to the trap when inspected",
			Description = "Even with no bait option chosen, we will make sure to inspect the trap regularly, and, in case it has been sprung, reset it.",
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:glassyCreeperPods",
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
					EntityTypeToCreate = "item:glassyCreeperPods",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					},
					IsWasteProduct = true
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.00033333336f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Use rat meat bait",
			KeyName = "changeBaitToRatMeat",
			JobTypeKey = "checkTrapsJobType",
			RequiredSkill = "menial",
			SummaryDescription = "This type of food will be brought to the trap when inspected",
			Description = "Even with no bait option chosen, we will make sure to inspect the trap regularly, and, in case it has been sprung, reset it.",
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:binalRatChunk",
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
					EntityTypeToCreate = "item:binalRatChunk",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					},
					IsWasteProduct = true
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.00033333336f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Don't use bait",
			KeyName = "changeBaitToNoBait",
			JobTypeKey = "checkTrapsJobType",
			RequiredSkill = "menial",
			SummaryDescription = "No food will be brought to the trap when inspected",
			Description = "Even with no bait option chosen, we will make sure to inspect the trap regularly, and, in case it has been sprung, reset it.",
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.00033333336f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Reactivating trap",
			KeyName = "activateTrapWithBlackpulp",
			JobTypeKey = "checkTrapsJobType",
			RequiredSkill = "menial",
			SummaryDescription = "",
			Description = "",
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:blackpulp",
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
					EntityTypeToCreate = "item:blackpulp",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					},
					IsWasteProduct = true
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.0033333334f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Reactivating trap",
			KeyName = "activateTrapWithGlassyCreeper",
			JobTypeKey = "checkTrapsJobType",
			RequiredSkill = "menial",
			SummaryDescription = "",
			Description = "",
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:glassyCreeperPods",
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
					EntityTypeToCreate = "item:glassyCreeperPods",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					},
					IsWasteProduct = true
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.0033333334f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Reactivating trap",
			KeyName = "activateTrapWithRatMeat",
			JobTypeKey = "checkTrapsJobType",
			RequiredSkill = "menial",
			SummaryDescription = "",
			Description = "",
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:binalRatChunk",
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
					EntityTypeToCreate = "item:binalRatChunk",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					},
					IsWasteProduct = true
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.0033333334f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Kill animal",
			KeyName = "killTrappedAnimal",
			RequiredSkill = "menial",
			SummaryDescription = "",
			Description = "",
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.00033333336f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Reactivating trap",
			KeyName = "activateSnare",
			JobTypeKey = "checkTrapsJobType",
			RequiredSkill = "menial",
			SummaryDescription = "",
			Description = "",
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.0033333334f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Fumigating nest",
			KeyName = "useSulfurSmokeBomb",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			SummaryDescription = "It's being suggested that we fumigate the twinkler nest using some sort of smoke bomb.",
			Description = "If we carry out this plan, we need to take extreme caution because any disturbance of the nest is likely to trigger a defense response from the twinklers.",
			RequiresBoldStance = true,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:sulfurSmokeBomb",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Blowing up nest",
			KeyName = "useVarmintBomb",
			SummaryDescription = "Place and detonate explosives in the nest entrance",
			Description = "With a strong enough bomb, it is possible to collapse the dirt walls at the entrance of this quadite nest. However, the colony will likely rebuild the entrance eventually.",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			RequiresBoldStance = true,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:varmintBomb",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.0033333334f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Digging up rat nest",
			KeyName = "digUpRatNest",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			SummaryDescription = "summary wip",
			Description = "Description wip",
			RequiresBoldStance = true,
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Retrieving supplies",
			KeyName = "retrieveCrates",
			RequiredSkill = "bushcraft",
			SummaryDescription = "We should be able to climb down and get the supplies with the use of some climbing gear",
			Description = "The supplies are lying at the bottom of a slot canyon where water has carved a deep crevice in the sandstone.",
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			RequiresBoldStance = true,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:vine",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.0033333334f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Rescuing team member",
			KeyName = "rescueColleague",
			RequiredSkill = "bushcraft",
			SummaryDescription = "Our lost mission member is alive but motionless at the bottom of this crevice",
			Description = "He seems to be unconscious. We need to climb down and perform first aid as soon as possible. Some kind of harness is needed to get him up.",
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			RequiresBoldStance = true,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:vine",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Cleaning shell",
			KeyName = "cleanTurnipShell",
			SummaryDescription = "N/A",
			Description = "N/A",
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			AgentActionState = AnimAction.Harvesting
		});
		list.Add(new ProcessType
		{
			Name = "Build fish weir (sticks)",
			KeyName = "placeFishTrapCreekSticks",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "bushcraft",
			SummaryDescription = "Build a fish weir from sticks",
			Description = "A fish weir made of wooden fences could catch a large number of fish when they migrate through this body of water.",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:sticks",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 5
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:fishTrapCreekSticks",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			AgentActionState = AnimAction.Building,
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Build fish weir (netting)",
			KeyName = "placeFishTrapCreekNet",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "fishing",
			SummaryDescription = "Build a fish weir from netting",
			Description = "A two-way fish weir made of netting could catch a large number of fish when they migrate both ways through this body of water.",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:fishingNet",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 2
					}
				},
				new Input
				{
					Entity = "item:sticks",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 3
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:fishTrapCreekNet",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			DisablesSharedActionProcesses = new string[1] { "placeFishTrapCreekSticks" },
			AgentActionState = AnimAction.Building,
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Build fish trap (fyke)",
			KeyName = "placeFishTrapCoast",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "fishing",
			SummaryDescription = "Build a 'fyke' fish trap",
			Description = "This body of water is a typical habitat of the 'streak fin' fish and it would be a suitable place for setting up a fish trap specially designed to catch this species.",
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:fishTrapHoopNet",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:fishingNet",
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
					EntityTypeToCreate = "structure:fishTrapCoast",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Place fish trap (basket)",
			KeyName = "placeFishTrapShoreBasket",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "bushcraft",
			SummaryDescription = "Place 'basket' fish trap",
			Description = "This body of water is a typical habitat of the 'carbon tail' fish and it would be a good place for setting up a fish trap specially designed to catch this species.",
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:fishTrapBasket",
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
					EntityTypeToCreate = "structure:fishTrapShoreBasket",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Place fish trap (hoop net)",
			KeyName = "placeFishTrapShoreHoopNet",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "fishing",
			SummaryDescription = "Place 'hoop net' fish trap",
			Description = "This body of water is a typical habitat of the 'carbon tail' fish and it would be a good place for setting up a fish trap specially designed to catch this species.",
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:fishTrapHoopNet",
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
					EntityTypeToCreate = "structure:fishTrapShoreHoopNet",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Checking fish trap",
			KeyName = "checkFishTrap",
			JobTypeKey = "checkTrapsJobType",
			UserCanCancel = false,
			UserCannotCancelReason = "Checking cannot be cancelled. Abandon or salvage the fish trap to stop using it",
			SummaryDescription = "Check trap for fish.",
			Description = "We should regularly check if the trap has caught any fish",
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.00033333336f
			},
			RequiredSkill = "menial"
		});
		list.Add(new ProcessType
		{
			Name = "Build simple port",
			KeyName = "buildSimplePort",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "construction",
			SummaryDescription = "Build a simple port which allows us to receive boats and sell goods to other settlements. Small capacity. Suited for selling food.",
			Description = "Has a pier where small boats and barges can moor and a storehouse where goods intended for sale can be placed. The clay storehouse is raised on pillars to keep a small amount of goods safe from vermin. \nThe storehouse also has a space for storage of items that are not intended for sale.",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[4]
			{
				new Input
				{
					Entity = "item:spoakBranchesTrimmed",
					Amount = new InputAmount
					{
						NoOfItems = 3
					},
					IsConsumed = true
				},
				new Input
				{
					Entity = "item:spoakShingles",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = true
				},
				new Input
				{
					Entity = "item:solidMudBrick",
					Amount = new InputAmount
					{
						NoOfItems = 5
					},
					IsConsumed = true
				},
				new Input
				{
					Entity = "item:waterCaneStem",
					Amount = new InputAmount
					{
						NoOfItems = 3
					},
					IsConsumed = true
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:simplePort",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.023333333f
			},
			ProcessToolSetKey = "toolSetCordage",
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Build canopy port",
			KeyName = "buildCanopyPort",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "construction",
			SummaryDescription = "Build a port which allows us to receive boats and sell goods to other settlements. Large capacity. No vermin protection.",
			Description = "Has a pier where small boats and barges can moor and a large storage canopy where goods intended for sale can be placed. The goods are not protected from vermin, so this structure is NOT suited for trading food. \nThe canopy also has a space for storage of items that are not intended for sale.",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[3]
			{
				new Input
				{
					Entity = "item:daysheenLeaves",
					Amount = new InputAmount
					{
						NoOfItems = 2
					},
					IsConsumed = true
				},
				new Input
				{
					Entity = "item:spoakBranchesTrimmed",
					Amount = new InputAmount
					{
						NoOfItems = 3
					},
					IsConsumed = true
				},
				new Input
				{
					Entity = "item:waterCaneStem",
					Amount = new InputAmount
					{
						NoOfItems = 3
					},
					IsConsumed = true
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:canopyPort",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.011666667f
			},
			ProcessToolSetKey = "toolSetCordage",
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Build landing",
			KeyName = "buildImprovisedLanding",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "construction",
			SummaryDescription = "Build an improvised landing for boats to moor. Can transfer passengers. Goods can ONLY be received, not sold.",
			Description = "This simple structure does not allow us to sell goods because it lacks a storehouse. It is only suited for receiving goods and embarking and disembarking passengers.",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:waterCaneStem",
					Amount = new InputAmount
					{
						NoOfItems = 2
					},
					IsConsumed = true
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:landingImprovised",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 120f
			},
			ProcessToolSetKey = "toolSetCordage",
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Build favorbread farm",
			KeyName = "buildFavorbreadFarm",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "bushcraft",
			SummaryDescription = "Dig a pit where we can cultivate the favorbread vegetable",
			Description = "By excavating a small garden directly underneath the dead sanctuary tree we can revive the favorbread if we provide it with the carbohydrates that the tree no longer supplies it with. We have found that the blackpulp is well suited as a nutrient that will make the favorbread grow vigorously.\n Note: We have found that this method of cultivation is not possible next to a LIVING sanctuary tree because disturbing the connection between the two organisms elicits a dangerous, defensive response from them.",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:sticks",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = true
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:favorbreadFarm",
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
			ProcessToolSetKey = "toolSetDiggingConstruction",
			AgentActionState = AnimAction.Digging
		});
		list.Add(new ProcessType
		{
			Name = "Build clay pit",
			KeyName = "establishClayPit",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "construction",
			SummaryDescription = "Establish a clay pit from which we can extract large amounts of clay",
			Description = "By digging an extraction site here, we will have access to the rich deposits of clay beneath the surface which are otherwise hard to reach. (When the clay pit is established, clay can then be ordered from the Production Manager)",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:sticks",
					IsConsumed = false,
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
					EntityTypeToCreate = "structure:clayPit",
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
			ProcessToolSetKey = "toolSetDiggingConstruction",
			AgentActionState = AnimAction.Digging
		});
		list.Add(new ProcessType
		{
			Name = "Build salt mine",
			KeyName = "establishSaltMine",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "construction",
			SummaryDescription = "Establish a salt mine from which we can extract large amounts of salt",
			Description = "By digging an extraction site here, we will have access to the rich deposits of salt beneath the surface which are otherwise hard to reach. (When the salt mine is established, salt can then be ordered from the Production Manager)",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:sticks",
					IsConsumed = false,
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
					EntityTypeToCreate = "structure:saltMine",
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
			ProcessToolSetKey = "toolSetDiggingConstruction",
			AgentActionState = AnimAction.Digging
		});
		list.Add(new ProcessType
		{
			Name = "Build bog ore pit",
			KeyName = "establishBogOrePit",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "construction",
			SummaryDescription = "Establish a pit from which we can extract large amounts of bog ore",
			Description = "By digging an extraction site here, we will have access to the rich deposits of bog ore beneath the surface which are otherwise hard to reach. (When the bog ore pit is established, bog ore can then be ordered from the Production Manager)",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:sticks",
					IsConsumed = false,
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
					EntityTypeToCreate = "structure:bogOrePit",
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
			ProcessToolSetKey = "toolSetDiggingConstruction",
			AgentActionState = AnimAction.Digging
		});
		list.Add(new ProcessType
		{
			Name = "Build scandium mine",
			KeyName = "establishRareMetalOrePit",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "construction",
			SummaryDescription = "Establish a pit from which we can extract scandium ore",
			Description = "By digging an extraction site here, we will have access to the rich deposits of scandium ore beneath the surface. (When the scandium mine is established, scandium ore can then be ordered from the Production Manager)",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:sticks",
					IsConsumed = false,
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
					EntityTypeToCreate = "structure:rareMetalOrePit1",
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
			ProcessToolSetKey = "toolSetDiggingConstruction",
			AgentActionState = AnimAction.Digging
		});
		list.Add(new ProcessType
		{
			Name = "Build terbium mine",
			KeyName = "establishRareMetalOrePit2",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "construction",
			SummaryDescription = "Establish a pit from which we can extract terbium ore",
			Description = "By digging an extraction site here, we will have access to the rich deposits of terbium ore beneath the surface. (When the terbium mine is established, terbium can then be ordered from the Production Manager)",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:sticks",
					IsConsumed = false,
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
					EntityTypeToCreate = "structure:rareMetalOrePit2",
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
			ProcessToolSetKey = "toolSetDiggingConstruction",
			AgentActionState = AnimAction.Digging
		});
		list.Add(new ProcessType
		{
			Name = "Build peat bank",
			KeyName = "establishPeatBank",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "menial",
			SummaryDescription = "Establish a digging site where we can cut peat",
			Description = "This area is well suited for making a trench where we can cut peat. (When the peat bank is established, peat can then be ordered from the Production Manager)",
			PhysicalWorkFactor = 4f,
			Stances = standingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:sticks",
					IsConsumed = false,
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
					EntityTypeToCreate = "structure:peatBank",
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
			ProcessToolSetKey = "toolSetPlowingTools",
			AgentActionState = AnimAction.Tilling
		});
		string userCannotCancelReason = "Fertilizing cannot be cancelled. To stop using fertilizer, use the 'Stop fertilizing' action on the farm plot or greenhouse";
		string userCannotCancelReason2 = "Weeding cannot be cancelled. To stop growing crops, use the 'Stop growing' action on the farm plot or greenhouse";
		string userCannotCancelReason3 = "Planting cannot be cancelled. To stop growing crops, use the 'Stop growing' action on the farm plot or greenhouse";
		list.Add(new ProcessType
		{
			Name = "Establish farm plot",
			KeyName = "establishSmallPlot",
			JobTypeKey = "constructionJobType",
			SummaryDescription = "Establish a small farm plot",
			Description = "At this location we could establish a small farm plot and grow our own crops.",
			PhysicalWorkFactor = 4f,
			Stances = standingProduction,
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.023333333f
			},
			ProcessToolSetKey = "toolSetPlowingTools",
			AgentActionState = AnimAction.Tilling,
			RequiredSkill = "farming",
			TierOrArea = new TierOrArea
			{
				Tier = "basic",
				Area = RatingTypes.Food
			}
		});
		list.Add(new ProcessType
		{
			Name = "Establish farm plot",
			KeyName = "establishLargePlot",
			JobTypeKey = "constructionJobType",
			SummaryDescription = "Establish a large farm plot",
			Description = "At this location we could establish a large farm plot and grow our own crops.",
			PhysicalWorkFactor = 4f,
			Stances = standingProduction,
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.050000004f
			},
			ProcessToolSetKey = "toolSetPlowingTools",
			AgentActionState = AnimAction.Tilling,
			RequiredSkill = "farming",
			TierOrArea = new TierOrArea
			{
				Tier = "basic",
				Area = RatingTypes.Food
			}
		});
		string name = "Plant cotton";
		string summaryDescription = "Till the soil and plant cotton";
		string description = "This plant should give a decent yield provided the farm plot is weeded regularly during the growth period. Once the cotton plant is fully grown the cotton should be picked without delay, as the plant and crops will wither in freezing temperatures.";
		list.Add(new ProcessType
		{
			Name = "Sow Glassy creeper pods",
			KeyName = "plantGlassyCreeperPodsInSmallPlot",
			JobTypeKey = "sowingAndHarvestingJobType",
			UserCannotCancelReason = userCannotCancelReason3,
			UserCanCancel = false,
			SummaryDescription = "Till the soil and sow Glassy creeper",
			Description = "This plant should give a decent yield provided the farm plot is weeded regularly during the growth period. Once the Glassy creeper plant is fully grown the pods should be harvested without delay, as the plant and crops will wither quickly. A new batch of glassy creeper plants can be started immediately after harvesting the previous one, by tilling and sowing again.",
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:glassyCreeperPods",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			ProcessToolSetKey = "toolSetSowingWeedingFertilizingFarmPlot",
			AgentActionState = AnimAction.Tilling,
			RequiredSkill = "farming"
		});
		list.Add(new ProcessType
		{
			Name = "Sow Crystal berries",
			KeyName = "plantCrystalBerriesInSmallPlot",
			JobTypeKey = "sowingAndHarvestingJobType",
			UserCannotCancelReason = userCannotCancelReason3,
			UserCanCancel = false,
			SummaryDescription = "Till the soil and sow Crystal berries",
			Description = "This plant should give a decent yield provided the farm plot is weeded regularly during the growth period. Once the Crystal berry shrub is fully grown the berries should be harvested without delay, as the plant and crops will wither quickly. A new batch of crystal berries can be started immediately after harvesting the previous one, by tilling and sowing again.",
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:crystalBerries",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			ProcessToolSetKey = "toolSetSowingWeedingFertilizingFarmPlot",
			AgentActionState = AnimAction.Tilling,
			RequiredSkill = "farming"
		});
		list.Add(new ProcessType
		{
			Name = name,
			KeyName = "plantCottonInSmallPlot",
			JobTypeKey = "sowingAndHarvestingJobType",
			UserCannotCancelReason = userCannotCancelReason3,
			UserCanCancel = false,
			SummaryDescription = summaryDescription,
			Description = description,
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:cotton",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			ProcessToolSetKey = "toolSetSowingWeedingFertilizingFarmPlot",
			AgentActionState = AnimAction.Tilling,
			RequiredSkill = "farming"
		});
		list.Add(new ProcessType
		{
			Name = "Sow Glassy creeper pods",
			KeyName = "plantGlassyCreeperPodsInLargePlot",
			JobTypeKey = "sowingAndHarvestingJobType",
			UserCannotCancelReason = userCannotCancelReason3,
			UserCanCancel = false,
			SummaryDescription = "Till the soil and sow glassy creeper",
			Description = "This plant should give a decent yield provided the farm plot is weeded regularly during the growth period. Once the Glassy creeper plant is fully grown the pods should be harvested without delay, as the plant and crops will wither quickly. A new batch of glassy creeper plants can be started immediately after harvesting the previous one, by tilling and sowing again.",
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:glassyCreeperPods",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 2
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 75f
			},
			ProcessToolSetKey = "toolSetSowingWeedingFertilizingFarmPlot",
			AgentActionState = AnimAction.Tilling,
			RequiredSkill = "farming"
		});
		list.Add(new ProcessType
		{
			Name = "Sow Crystal berries",
			KeyName = "plantCrystalBerriesInLargePlot",
			JobTypeKey = "sowingAndHarvestingJobType",
			UserCannotCancelReason = userCannotCancelReason3,
			UserCanCancel = false,
			SummaryDescription = "Till the soil and sow crystal berries",
			Description = "This plant should give a decent yield provided the farm plot is weeded regularly during the growth period. Once the Crystal berry shrub is fully grown the berries should be harvested without delay, as the plant and crops will wither quickly. A new batch of crystal berries can be started immediately after harvesting the previous one, by tilling and sowing again.",
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:crystalBerries",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 2
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 75f
			},
			ProcessToolSetKey = "toolSetSowingWeedingFertilizingFarmPlot",
			AgentActionState = AnimAction.Tilling,
			RequiredSkill = "farming"
		});
		list.Add(new ProcessType
		{
			Name = name,
			KeyName = "plantCottonInLargePlot",
			JobTypeKey = "sowingAndHarvestingJobType",
			UserCannotCancelReason = userCannotCancelReason3,
			UserCanCancel = false,
			SummaryDescription = summaryDescription,
			Description = description,
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:cotton",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 2
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 75f
			},
			ProcessToolSetKey = "toolSetSowingWeedingFertilizingFarmPlot",
			AgentActionState = AnimAction.Tilling,
			RequiredSkill = "farming"
		});
		list.Add(new ProcessType
		{
			Name = "Sow Finger fruits",
			KeyName = "plantFingerFruitInGreenhouse",
			JobTypeKey = "sowingAndHarvestingJobType",
			UserCannotCancelReason = userCannotCancelReason3,
			UserCanCancel = false,
			SummaryDescription = "Sow finger fruit",
			Description = "The finger fruit plant requires a greenhouse and should give a decent yield provided the plot is weeded regularly during the growth period. Once the finger fruits are grown they should be harvested without delay, as the plant and crops will wither quickly. A new batch of finger fruit can be started immediately after harvesting the previous one, by tilling and sowing again.",
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:fingerFruit",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			RequiredSkill = "farming",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Sow Glassy creeper pods",
			KeyName = "plantGlassyCreeperPodsInGreenhouse",
			JobTypeKey = "sowingAndHarvestingJobType",
			UserCannotCancelReason = userCannotCancelReason3,
			UserCanCancel = false,
			SummaryDescription = "Sow Glassy creeper",
			Description = "This plant should give a decent yield provided the plot is weeded regularly during the growth period. Once the Glassy creeper plant is fully grown the pods should be harvested without delay, as the plant and crops will wither quickly. A new batch of glassy creeper plants can be started immediately after harvesting the previous one, by tilling and sowing again.",
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:glassyCreeperPods",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			RequiredSkill = "farming",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Sow Crystal berries",
			KeyName = "plantCrystalBerriesInGreenhouse",
			JobTypeKey = "sowingAndHarvestingJobType",
			UserCannotCancelReason = userCannotCancelReason3,
			UserCanCancel = false,
			SummaryDescription = "Sow Crystal berries",
			Description = "This plant should give a decent yield provided the plot is weeded regularly during the growth period. Once the Crystal berry shrub is fully grown the berries should be harvested without delay, as the plant and crops will wither quickly. A new batch of crystal berries can be started immediately after harvesting the previous one, by tilling and sowing again.",
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:crystalBerries",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			RequiredSkill = "farming",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Weeding plot",
			KeyName = "weedPlot",
			JobTypeKey = "weedingAndFertilizingJobType",
			UserCannotCancelReason = userCannotCancelReason2,
			UserCanCancel = false,
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			RequiredSkill = "weeding",
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 75f
			},
			ProcessToolSetKey = "toolSetSowingWeedingFertilizingFarmPlot",
			AgentActionState = AnimAction.Tilling
		});
		list.Add(new ProcessType
		{
			Name = "Weeding greenhouse",
			KeyName = "weedGreenhouse",
			JobTypeKey = "weedingAndFertilizingJobType",
			UserCannotCancelReason = userCannotCancelReason2,
			UserCanCancel = false,
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			RequiredSkill = "weeding",
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 75f
			},
			ProcessToolSetKey = "toolSetGreenhouseHarvest",
			AgentActionState = AnimAction.Harvesting,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Low }
		});
		list.Add(new ProcessType
		{
			Name = "Weeding plot",
			KeyName = "weedLargePlot",
			JobTypeKey = "weedingAndFertilizingJobType",
			UserCannotCancelReason = userCannotCancelReason2,
			UserCanCancel = false,
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			RequiredSkill = "weeding",
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			ProcessToolSetKey = "toolSetSowingWeedingFertilizingFarmPlot",
			AgentActionState = AnimAction.Tilling
		});
		string text = "To ensure a high crop yield, we will fertilize the plot whenever the soil quality drops as long as we have the selected fertilizer available.";
		list.Add(new ProcessType
		{
			Name = "Fertilize plot",
			KeyName = "organicFertilizePlot",
			JobTypeKey = "weedingAndFertilizingJobType",
			UserCannotCancelReason = userCannotCancelReason,
			UserCanCancel = false,
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			RequiredSkill = "farming",
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:organicFertilizer",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 4
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 75f
			},
			ProcessToolSetKey = "toolSetSowingWeedingFertilizingFarmPlot",
			AgentActionState = AnimAction.Tilling
		});
		list.Add(new ProcessType
		{
			Name = "Fertilize plot",
			KeyName = "organicFertilizeLargePlot",
			JobTypeKey = "weedingAndFertilizingJobType",
			UserCannotCancelReason = userCannotCancelReason,
			UserCanCancel = false,
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			RequiredSkill = "farming",
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:organicFertilizer",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 8
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			ProcessToolSetKey = "toolSetSowingWeedingFertilizingFarmPlot",
			AgentActionState = AnimAction.Tilling
		});
		list.Add(new ProcessType
		{
			Name = "Fertilize plot",
			KeyName = "guanoFertilizePlot",
			JobTypeKey = "weedingAndFertilizingJobType",
			UserCannotCancelReason = userCannotCancelReason,
			UserCanCancel = false,
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			RequiredSkill = "farming",
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:guanoFertilizer",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 75f
			},
			ProcessToolSetKey = "toolSetSowingWeedingFertilizingFarmPlot",
			AgentActionState = AnimAction.Tilling
		});
		list.Add(new ProcessType
		{
			Name = "Fertilize plot",
			KeyName = "guanoFertilizeLargePlot",
			JobTypeKey = "weedingAndFertilizingJobType",
			UserCannotCancelReason = userCannotCancelReason,
			UserCanCancel = false,
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			RequiredSkill = "farming",
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:guanoFertilizer",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 2
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			ProcessToolSetKey = "toolSetSowingWeedingFertilizingFarmPlot",
			AgentActionState = AnimAction.Tilling
		});
		int sortOrder = 110;
		int sortOrder2 = 120;
		list.Add(new ProcessType
		{
			Name = "Use compost fertilizer",
			KeyName = "useOrganicFertilizer",
			JobTypeKey = "weedingAndFertilizingJobType",
			UserCanCancel = false,
			SummaryDescription = "The type of fertilizer to use once the plot needs fertilizing",
			Description = "We will need to use 4 bags of compost for fertilizing this small farm plot when the soil is low on nutrients." + text,
			PhysicalWorkFactor = 0f,
			WorkNeeded = WorkerNeededOptions.StartRemotely,
			IsMetaAction = true,
			SortOrder = sortOrder,
			DisablesSpecialActionLockProcesses = new string[2] { "useOrganicFertilizer", "stopUsingGuanoFertilizer" },
			EnablesSpecialActionLockProcesses = new string[2] { "stopUsingOrganicFertilizer", "useGuanoFertilizer" },
			Stances = kneelingProduction,
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0f
			},
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Use compost fertilizer",
			KeyName = "useLargeOrganicFertilizer",
			JobTypeKey = "weedingAndFertilizingJobType",
			UserCanCancel = false,
			SummaryDescription = "The type of fertilizer to use once the plot needs fertilizing",
			Description = "We will need to use 8 bags of compost for fertilizing this large farm plot when the soil is low on nutrients" + text,
			PhysicalWorkFactor = 0f,
			WorkNeeded = WorkerNeededOptions.StartRemotely,
			IsMetaAction = true,
			SortOrder = sortOrder,
			DisablesSpecialActionLockProcesses = new string[2] { "useLargeOrganicFertilizer", "stopUsingLargeGuanoFertilizer" },
			EnablesSpecialActionLockProcesses = new string[2] { "stopUsingLargeOrganicFertilizer", "useLargeGuanoFertilizer" },
			Stances = kneelingProduction,
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0f
			},
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Use guano fertilizer",
			KeyName = "useGuanoFertilizer",
			JobTypeKey = "weedingAndFertilizingJobType",
			UserCanCancel = false,
			SummaryDescription = "The type of fertilizer to use once the plot needs fertilizing",
			Description = "We will need to use 1 bag of guano fertilizer for fertilizing this small farm plot when the soil is low on nutrients" + text,
			PhysicalWorkFactor = 0f,
			WorkNeeded = WorkerNeededOptions.StartRemotely,
			IsMetaAction = true,
			SortOrder = sortOrder2,
			DisablesSpecialActionLockProcesses = new string[2] { "useGuanoFertilizer", "stopUsingOrganicFertilizer" },
			EnablesSpecialActionLockProcesses = new string[2] { "stopUsingGuanoFertilizer", "useOrganicFertilizer" },
			Stances = kneelingProduction,
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0f
			},
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Use guano fertilizer",
			KeyName = "useLargeGuanoFertilizer",
			JobTypeKey = "weedingAndFertilizingJobType",
			UserCanCancel = false,
			SummaryDescription = "The type of fertilizer to use once the plot needs fertilizing",
			Description = "We will need to use 2 bags of guano fertilizer for fertilizing this large farm plot when the soil is low on nutrients" + text,
			PhysicalWorkFactor = 0f,
			WorkNeeded = WorkerNeededOptions.StartRemotely,
			IsMetaAction = true,
			SortOrder = 100,
			DisablesSpecialActionLockProcesses = new string[2] { "useLargeGuanoFertilizer", "stopUsingLargeOrganicFertilizer" },
			EnablesSpecialActionLockProcesses = new string[2] { "stopUsingLargeGuanoFertilizer", "useLargeOrganicFertilizer" },
			Stances = kneelingProduction,
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0f
			},
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		string name2 = "Stop using guano fertilizer";
		string name3 = "Stop using compost fertilizer";
		string specialActionCaption = "SELECT";
		list.Add(new ProcessType
		{
			Name = name2,
			KeyName = "stopUsingLargeGuanoFertilizer",
			JobTypeKey = "weedingAndFertilizingJobType",
			UserCanCancel = false,
			SpecialActionCaption = specialActionCaption,
			SummaryDescription = "",
			Description = "",
			PhysicalWorkFactor = 0f,
			WorkNeeded = WorkerNeededOptions.StartRemotely,
			IsMetaAction = true,
			SortOrder = sortOrder2,
			Stances = kneelingProduction,
			EnablesSpecialActionLockProcesses = new string[1] { "useLargeGuanoFertilizer" },
			DisablesSpecialActionLockProcesses = new string[1] { "stopUsingLargeGuanoFertilizer" },
			ShowDisabledSpecialAction = false,
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0f
			},
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = name3,
			KeyName = "stopUsingLargeOrganicFertilizer",
			JobTypeKey = "weedingAndFertilizingJobType",
			UserCanCancel = false,
			SpecialActionCaption = specialActionCaption,
			SummaryDescription = "",
			Description = "",
			PhysicalWorkFactor = 0f,
			WorkNeeded = WorkerNeededOptions.StartRemotely,
			IsMetaAction = true,
			SortOrder = sortOrder,
			Stances = kneelingProduction,
			EnablesSpecialActionLockProcesses = new string[1] { "useLargeOrganicFertilizer" },
			DisablesSpecialActionLockProcesses = new string[1] { "stopUsingLargeOrganicFertilizer" },
			ShowDisabledSpecialAction = false,
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0f
			},
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = name2,
			KeyName = "stopUsingGuanoFertilizer",
			JobTypeKey = "weedingAndFertilizingJobType",
			UserCanCancel = false,
			SpecialActionCaption = specialActionCaption,
			SummaryDescription = "",
			Description = "",
			PhysicalWorkFactor = 0f,
			WorkNeeded = WorkerNeededOptions.StartRemotely,
			IsMetaAction = true,
			SortOrder = sortOrder2,
			Stances = kneelingProduction,
			EnablesSpecialActionLockProcesses = new string[1] { "useGuanoFertilizer" },
			DisablesSpecialActionLockProcesses = new string[1] { "stopUsingGuanoFertilizer" },
			ShowDisabledSpecialAction = false,
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0f
			},
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = name3,
			KeyName = "stopUsingOrganicFertilizer",
			JobTypeKey = "weedingAndFertilizingJobType",
			UserCanCancel = false,
			SpecialActionCaption = specialActionCaption,
			SummaryDescription = "",
			Description = "",
			PhysicalWorkFactor = 0f,
			WorkNeeded = WorkerNeededOptions.StartRemotely,
			IsMetaAction = true,
			SortOrder = sortOrder,
			Stances = kneelingProduction,
			EnablesSpecialActionLockProcesses = new string[1] { "useOrganicFertilizer" },
			DisablesSpecialActionLockProcesses = new string[1] { "stopUsingOrganicFertilizer" },
			ShowDisabledSpecialAction = false,
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0f
			},
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		string name4 = "Stop growing glassy creeper pods";
		string name5 = "Stop growing crystal berries";
		string name6 = "Stop growing cotton";
		string name7 = "Stop growing finger fruit";
		string summaryDescription2 = "Stops tending the current crops. NOTE: to replant with different crop, first clear away the farm plot";
		string description2 = "After stopping, we can resume tending the current crops at any point. But if we want to re-plant with a different crop immediately, we have to clear away the plot, then re-establish the plot, then plant the new crop.";
		string name8 = "Grow glassy creeper pods";
		string text2 = "The crops will be planted, tended and harvested in a continuous cycle until an order is given to stop.";
		string name9 = "Grow crystal berries";
		string text3 = "The crops will be planted, tended and harvested in a continuous cycle until an order is given to stop.";
		string name10 = "Grow cotton";
		string text4 = "The crops will be planted, tended and harvested in a continuous cycle until an order is given to stop.";
		string name11 = "Grow finger fruit";
		string text5 = "The finger fruit plant requires a greenhouse and should give a decent yield provided the plot is weeded regularly during the growth period. The crops will be planted, tended and harvested in a continuous cycle until an order is given to stop.";
		int sortOrder3 = 5;
		int sortOrder4 = 15;
		string specialActionCaption2 = "SELECT";
		list.Add(new ProcessType
		{
			Name = name9,
			KeyName = "growCrystalBerriesInLargePlot",
			JobTypeKey = "sowingAndHarvestingJobType",
			UserCanCancel = false,
			SummaryDescription = "Use this plot for growing crystal berries. 2 crystal berries are needed for sowing.",
			Description = "We need 2 crystal berry items to start growing on this large plot. " + text3,
			PhysicalWorkFactor = 0f,
			WorkNeeded = WorkerNeededOptions.StartRemotely,
			IsMetaAction = true,
			SortOrder = sortOrder3,
			Stances = kneelingProduction,
			DisablesSpecialActionLockProcesses = new string[3] { "growCrystalBerriesInLargePlot", "stopGrowingGlassyCreeperPodsInLargePlot", "stopGrowingCottonInLargePlot" },
			EnablesSpecialActionLockProcesses = new string[3] { "stopGrowingCrystalBerriesInLargePlot", "growGlassyCreeperPodsInLargePlot", "growCottonInLargePlot" },
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0f
			},
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = name5,
			KeyName = "stopGrowingCrystalBerriesInLargePlot",
			JobTypeKey = "sowingAndHarvestingJobType",
			UserCanCancel = false,
			SpecialActionCaption = specialActionCaption2,
			SummaryDescription = summaryDescription2,
			Description = description2,
			PhysicalWorkFactor = 0f,
			WorkNeeded = WorkerNeededOptions.StartRemotely,
			IsMetaAction = true,
			SortOrder = sortOrder3,
			Stances = kneelingProduction,
			EnablesSpecialActionLockProcesses = new string[1] { "growCrystalBerriesInLargePlot" },
			DisablesSpecialActionLockProcesses = new string[1] { "stopGrowingCrystalBerriesInLargePlot" },
			ShowDisabledSpecialAction = false,
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0f
			},
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = name8,
			KeyName = "growGlassyCreeperPodsInLargePlot",
			JobTypeKey = "sowingAndHarvestingJobType",
			UserCanCancel = false,
			SummaryDescription = "Use this plot for growing glassy creeper pods. 2 glassy pods are needed for sowing.",
			Description = "We need 2 glassy creeper pods to start growing on this large plot. " + text2,
			PhysicalWorkFactor = 0f,
			WorkNeeded = WorkerNeededOptions.StartRemotely,
			IsMetaAction = true,
			SortOrder = sortOrder4,
			Stances = kneelingProduction,
			DisablesSpecialActionLockProcesses = new string[3] { "growGlassyCreeperPodsInLargePlot", "stopGrowingCrystalBerriesInLargePlot", "stopGrowingCottonInLargePlot" },
			EnablesSpecialActionLockProcesses = new string[3] { "stopGrowingGlassyCreeperPodsInLargePlot", "growCrystalBerriesInLargePlot", "growCottonInLargePlot" },
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0f
			},
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = name4,
			KeyName = "stopGrowingGlassyCreeperPodsInLargePlot",
			JobTypeKey = "sowingAndHarvestingJobType",
			UserCanCancel = false,
			SpecialActionCaption = specialActionCaption2,
			SummaryDescription = summaryDescription2,
			Description = description2,
			PhysicalWorkFactor = 0f,
			Stances = kneelingProduction,
			EnablesSpecialActionLockProcesses = new string[1] { "growGlassyCreeperPodsInLargePlot" },
			DisablesSpecialActionLockProcesses = new string[1] { "stopGrowingGlassyCreeperPodsInLargePlot" },
			WorkNeeded = WorkerNeededOptions.StartRemotely,
			IsMetaAction = true,
			SortOrder = sortOrder4,
			ShowDisabledSpecialAction = false,
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0f
			},
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = name10,
			KeyName = "growCottonInLargePlot",
			JobTypeKey = "sowingAndHarvestingJobType",
			UserCanCancel = false,
			SummaryDescription = "Use this plot for growing cotton. 2 cotton are needed for sowing.",
			Description = "We need 2 cotton items to start growing on this large plot. " + text4,
			PhysicalWorkFactor = 0f,
			WorkNeeded = WorkerNeededOptions.StartRemotely,
			IsMetaAction = true,
			SortOrder = 20,
			Stances = kneelingProduction,
			DisablesSpecialActionLockProcesses = new string[3] { "growCottonInLargePlot", "stopGrowingCrystalBerriesInLargePlot", "stopGrowingGlassyCreeperPodsInLargePlot" },
			EnablesSpecialActionLockProcesses = new string[3] { "stopGrowingCottonInLargePlot", "growCrystalBerriesInLargePlot", "growGlassyCreeperPodsInLargePlot" },
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0f
			},
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = name6,
			KeyName = "stopGrowingCottonInLargePlot",
			JobTypeKey = "sowingAndHarvestingJobType",
			UserCanCancel = false,
			SpecialActionCaption = specialActionCaption2,
			SummaryDescription = summaryDescription2,
			Description = description2,
			PhysicalWorkFactor = 0f,
			WorkNeeded = WorkerNeededOptions.StartRemotely,
			IsMetaAction = true,
			SortOrder = 20,
			Stances = kneelingProduction,
			EnablesSpecialActionLockProcesses = new string[1] { "growCottonInLargePlot" },
			DisablesSpecialActionLockProcesses = new string[1] { "stopGrowingCottonInLargePlot" },
			ShowDisabledSpecialAction = false,
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0f
			},
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Grow crystal berries",
			KeyName = "growCrystalBerriesInSmallPlot",
			JobTypeKey = "sowingAndHarvestingJobType",
			UserCanCancel = false,
			SummaryDescription = "Use this plot for growing crystal berries. 1 crystal berry is needed for sowing.",
			Description = "We need 1 crystal berry item to start growing on this small plot. " + text3,
			PhysicalWorkFactor = 0f,
			WorkNeeded = WorkerNeededOptions.StartRemotely,
			IsMetaAction = true,
			SortOrder = sortOrder3,
			Stances = kneelingProduction,
			DisablesSpecialActionLockProcesses = new string[3] { "growCrystalBerriesInSmallPlot", "stopGrowingCottonInSmallPlot", "stopGrowingGlassyCreeperPodsInSmallPlot" },
			EnablesSpecialActionLockProcesses = new string[3] { "stopGrowingCrystalBerriesInSmallPlot", "growGlassyCreeperPodsInSmallPlot", "growCottonInSmallPlot" },
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0f
			},
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = name5,
			KeyName = "stopGrowingCrystalBerriesInSmallPlot",
			JobTypeKey = "sowingAndHarvestingJobType",
			UserCanCancel = false,
			SpecialActionCaption = specialActionCaption2,
			SummaryDescription = summaryDescription2,
			Description = description2,
			PhysicalWorkFactor = 0f,
			WorkNeeded = WorkerNeededOptions.StartRemotely,
			IsMetaAction = true,
			SortOrder = sortOrder3,
			Stances = kneelingProduction,
			EnablesSpecialActionLockProcesses = new string[1] { "growCrystalBerriesInSmallPlot" },
			DisablesSpecialActionLockProcesses = new string[1] { "stopGrowingCrystalBerriesInSmallPlot" },
			ShowDisabledSpecialAction = false,
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0f
			},
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = name8,
			KeyName = "growGlassyCreeperPodsInSmallPlot",
			JobTypeKey = "sowingAndHarvestingJobType",
			UserCanCancel = false,
			SummaryDescription = "Use this plot for growing glassy creeper pods. 1 glassy creeper is needed for sowing.",
			Description = "We need 1 glassy creeper pod to start growing on this small plot. " + text2,
			PhysicalWorkFactor = 0f,
			WorkNeeded = WorkerNeededOptions.StartRemotely,
			IsMetaAction = true,
			SortOrder = sortOrder4,
			Stances = kneelingProduction,
			DisablesSpecialActionLockProcesses = new string[3] { "growGlassyCreeperPodsInSmallPlot", "stopGrowingCottonInSmallPlot", "stopGrowingCrystalBerriesInSmallPlot" },
			EnablesSpecialActionLockProcesses = new string[3] { "stopGrowingGlassyCreeperPodsInSmallPlot", "growCrystalBerriesInSmallPlot", "growCottonInSmallPlot" },
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0f
			},
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = name4,
			KeyName = "stopGrowingGlassyCreeperPodsInSmallPlot",
			JobTypeKey = "sowingAndHarvestingJobType",
			UserCanCancel = false,
			SpecialActionCaption = specialActionCaption2,
			SummaryDescription = summaryDescription2,
			Description = description2,
			PhysicalWorkFactor = 0f,
			WorkNeeded = WorkerNeededOptions.StartRemotely,
			IsMetaAction = true,
			SortOrder = sortOrder4,
			Stances = kneelingProduction,
			EnablesSpecialActionLockProcesses = new string[1] { "growGlassyCreeperPodsInSmallPlot" },
			DisablesSpecialActionLockProcesses = new string[1] { "stopGrowingGlassyCreeperPodsInSmallPlot" },
			ShowDisabledSpecialAction = false,
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0f
			},
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = name10,
			KeyName = "growCottonInSmallPlot",
			JobTypeKey = "sowingAndHarvestingJobType",
			UserCanCancel = false,
			SummaryDescription = "Use this plot for growing cotton. 1 cotton is needed for sowing.",
			Description = "We need 1 cotton item to start growing on this small plot. " + text4,
			PhysicalWorkFactor = 0f,
			WorkNeeded = WorkerNeededOptions.StartRemotely,
			IsMetaAction = true,
			SortOrder = 20,
			Stances = kneelingProduction,
			DisablesSpecialActionLockProcesses = new string[3] { "growCottonInSmallPlot", "stopGrowingGlassyCreeperPodsInSmallPlot", "stopGrowingCrystalBerriesInSmallPlot" },
			EnablesSpecialActionLockProcesses = new string[3] { "stopGrowingCottonInSmallPlot", "growCrystalBerriesInSmallPlot", "growGlassyCreeperPodsInSmallPlot" },
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0f
			},
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = name6,
			KeyName = "stopGrowingCottonInSmallPlot",
			JobTypeKey = "sowingAndHarvestingJobType",
			UserCanCancel = false,
			SpecialActionCaption = specialActionCaption2,
			SummaryDescription = summaryDescription2,
			Description = description2,
			PhysicalWorkFactor = 0f,
			WorkNeeded = WorkerNeededOptions.StartRemotely,
			IsMetaAction = true,
			SortOrder = 20,
			Stances = kneelingProduction,
			EnablesSpecialActionLockProcesses = new string[1] { "growCottonInSmallPlot" },
			DisablesSpecialActionLockProcesses = new string[1] { "stopGrowingCottonInSmallPlot" },
			ShowDisabledSpecialAction = false,
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0f
			},
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Grow crystal berries",
			KeyName = "growCrystalBerriesInGreenhouse",
			JobTypeKey = "sowingAndHarvestingJobType",
			UserCanCancel = false,
			SummaryDescription = "Use this greenhouse for growing crystal berries. 1 crystal berry is needed for sowing.",
			Description = "We need 1 crystal berry item to start growing. " + text3,
			PhysicalWorkFactor = 0f,
			WorkNeeded = WorkerNeededOptions.StartRemotely,
			IsMetaAction = true,
			SortOrder = sortOrder3,
			Stances = kneelingProduction,
			DisablesSpecialActionLockProcesses = new string[3] { "growCrystalBerriesInGreenhouse", "stopGrowingGlassyCreeperPodsInGreenhouse", "stopGrowingFingerFruitInGreenhouse" },
			EnablesSpecialActionLockProcesses = new string[3] { "stopGrowingCrystalBerriesInGreenhouse", "growGlassyCreeperPodsInGreenhouse", "growFingerFruitInGreenhouse" },
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0f
			},
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = name5,
			KeyName = "stopGrowingCrystalBerriesInGreenhouse",
			JobTypeKey = "sowingAndHarvestingJobType",
			UserCanCancel = false,
			SpecialActionCaption = specialActionCaption2,
			SummaryDescription = summaryDescription2,
			Description = description2,
			PhysicalWorkFactor = 0f,
			WorkNeeded = WorkerNeededOptions.StartRemotely,
			IsMetaAction = true,
			SortOrder = sortOrder3,
			Stances = kneelingProduction,
			EnablesSpecialActionLockProcesses = new string[1] { "growCrystalBerriesInGreenhouse" },
			DisablesSpecialActionLockProcesses = new string[1] { "stopGrowingCrystalBerriesInGreenhouse" },
			ShowDisabledSpecialAction = false,
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0f
			},
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = name8,
			KeyName = "growGlassyCreeperPodsInGreenhouse",
			JobTypeKey = "sowingAndHarvestingJobType",
			UserCanCancel = false,
			SummaryDescription = "Use this greenhouse for growing glassy creepers. 1 glassy creeper pod is needed for sowing.",
			Description = "We need 1 glassy creeper pod to start growing. " + text2,
			PhysicalWorkFactor = 0f,
			WorkNeeded = WorkerNeededOptions.StartRemotely,
			IsMetaAction = true,
			SortOrder = sortOrder4,
			Stances = kneelingProduction,
			DisablesSpecialActionLockProcesses = new string[3] { "growGlassyCreeperPodsInGreenhouse", "stopGrowingCrystalBerriesInGreenhouse", "stopGrowingFingerFruitInGreenhouse" },
			EnablesSpecialActionLockProcesses = new string[3] { "stopGrowingGlassyCreeperPodsInGreenhouse", "growCrystalBerriesInGreenhouse", "growFingerFruitInGreenhouse" },
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0f
			},
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = name4,
			KeyName = "stopGrowingGlassyCreeperPodsInGreenhouse",
			JobTypeKey = "sowingAndHarvestingJobType",
			UserCanCancel = false,
			SpecialActionCaption = specialActionCaption2,
			SummaryDescription = summaryDescription2,
			Description = description2,
			PhysicalWorkFactor = 0f,
			WorkNeeded = WorkerNeededOptions.StartRemotely,
			IsMetaAction = true,
			SortOrder = sortOrder4,
			Stances = kneelingProduction,
			EnablesSpecialActionLockProcesses = new string[1] { "growGlassyCreeperPodsInGreenhouse" },
			DisablesSpecialActionLockProcesses = new string[1] { "stopGrowingGlassyCreeperPodsInGreenhouse" },
			ShowDisabledSpecialAction = false,
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0f
			},
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = name11,
			KeyName = "growFingerFruitInGreenhouse",
			JobTypeKey = "sowingAndHarvestingJobType",
			UserCanCancel = false,
			SummaryDescription = "Use this greenhouse for growing finger fruit. 1 finger fruit is needed for sowing.",
			Description = "We need 1 finger fruit item to start growing this. " + text5,
			PhysicalWorkFactor = 0f,
			WorkNeeded = WorkerNeededOptions.StartRemotely,
			IsMetaAction = true,
			SortOrder = 25,
			Stances = kneelingProduction,
			DisablesSpecialActionLockProcesses = new string[3] { "growFingerFruitInGreenhouse", "stopGrowingGlassyCreeperPodsInGreenhouse", "stopGrowingCrystalBerriesInGreenhouse" },
			EnablesSpecialActionLockProcesses = new string[3] { "stopGrowingFingerFruitInGreenhouse", "growCrystalBerriesInGreenhouse", "growGlassyCreeperPodsInGreenhouse" },
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0f
			},
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = name7,
			KeyName = "stopGrowingFingerFruitInGreenhouse",
			JobTypeKey = "sowingAndHarvestingJobType",
			UserCanCancel = false,
			SpecialActionCaption = specialActionCaption2,
			SummaryDescription = summaryDescription2,
			Description = description2,
			PhysicalWorkFactor = 0f,
			WorkNeeded = WorkerNeededOptions.StartRemotely,
			IsMetaAction = true,
			SortOrder = 25,
			Stances = kneelingProduction,
			EnablesSpecialActionLockProcesses = new string[1] { "growFingerFruitInGreenhouse" },
			DisablesSpecialActionLockProcesses = new string[1] { "stopGrowingFingerFruitInGreenhouse" },
			ShowDisabledSpecialAction = false,
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0f
			},
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Harvest crops",
			KeyName = "harvestCrops",
			JobTypeKey = "sowingAndHarvestingJobType",
			UserCanCancel = false,
			UserCannotCancelReason = "Harvesting cannot be cancelled. To stop growing crops, use the 'Stop growing' action on the farm plot.",
			SummaryDescription = "Crops can be harvested when they are fully grown. If left too long, they will wither",
			Description = "The window for harvesting varies depending on the crop type. Some crops will decay quickly while some can be left on the plant for longer.",
			RequiredSkill = "farming",
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			ProcessToolSetKey = "toolSetHarvestFieldCrops",
			AgentActionState = AnimAction.Harvesting
		});
		list.Add(new ProcessType
		{
			Name = "Harvest crops",
			KeyName = "harvestGreenhouseCrops",
			JobTypeKey = "sowingAndHarvestingJobType",
			UserCanCancel = false,
			UserCannotCancelReason = "Harvesting cannot be cancelled. To stop growing crops, use the 'Stop growing' action on the greenhouse.",
			SummaryDescription = "Crops can be harvested when they are fully grown. If left too long, they will wither",
			Description = "The window for harvesting varies, depending on the crop type. Some crops will decay quickly while some can be left on the plant for longer.",
			RequiredSkill = "farming",
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			ProcessToolSetKey = "toolSetGreenhouseHarvest",
			AgentActionState = AnimAction.Harvesting
		});
		list.Add(new ProcessType
		{
			Name = "Harvest crops",
			KeyName = "harvestLargeCrops",
			JobTypeKey = "sowingAndHarvestingJobType",
			UserCanCancel = false,
			UserCannotCancelReason = "Harvesting cannot be cancelled. To stop growing crops, use the 'Stop growing' action on the farm plot.",
			SummaryDescription = "Crops can be harvested when they are fully grown. If left too long, they will wither",
			Description = "The window for harvesting varies, depending on the crop type. Some crops will decay quickly while some can be left on the plant for longer.",
			RequiredSkill = "farming",
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 75f
			},
			ProcessToolSetKey = "toolSetHarvestFieldCrops",
			AgentActionState = AnimAction.Harvesting
		});
		list.Add(new ProcessType
		{
			Name = "Removing",
			KeyName = "salvageSimpleStove",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:simpleStoveUpgrade",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[2]
			{
				new Output
				{
					EntityTypeToCreate = "item:stones",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:solidMudBrick",
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
			AgentActionState = AnimAction.Mending,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Removing",
			KeyName = "salvageCommunityHall",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:communityHallUpgrade",
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
					EntityTypeToCreate = "item:spoakShingles",
					Amount = new OutputAmount
					{
						NoOfItems = 2
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:solidMudBrick",
					Amount = new OutputAmount
					{
						NoOfItems = 2
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:spoakBranchesTrimmed",
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
			Name = "Removing",
			KeyName = "salvageSmokeOvenUpgrade",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:smokeOvenUpgrade",
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
					EntityTypeToCreate = "item:solidMudBrick",
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
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Removing",
			KeyName = "salvageDryingShedUpgrade",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:dryingShedUpgrade",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[2]
			{
				new Output
				{
					EntityTypeToCreate = "item:solidMudBrick",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:shadeleafCanes",
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
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		SerializableDictionary<string, ChanceToTakeStance[]> salvageStructureStances = GetSalvageStructureStances();
		float num = 0.00033333336f;
		AnimAction agentActionState = AnimAction.Salvaging;
		list.Add(new ProcessType
		{
			Name = "Removing",
			KeyName = "salvageWingweedMats1People",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = salvageStructureStances,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:wingweedMats1People",
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
					EntityTypeToCreate = "item:wingweedMat",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = num
			},
			AgentActionState = agentActionState,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Removing",
			KeyName = "salvageWingweedMats2People",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = salvageStructureStances,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:wingweedMats2People",
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
					EntityTypeToCreate = "item:wingweedMat",
					Amount = new OutputAmount
					{
						NoOfItems = 2
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = num * 1.5f
			},
			AgentActionState = agentActionState,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Removing",
			KeyName = "salvageWingweedMats3People",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = salvageStructureStances,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:wingweedMats3People",
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
					EntityTypeToCreate = "item:wingweedMat",
					Amount = new OutputAmount
					{
						NoOfItems = 3
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = num * 2f
			},
			AgentActionState = agentActionState,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Removing",
			KeyName = "salvageWingweedMats4People",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = salvageStructureStances,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:wingweedMats4People",
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
					EntityTypeToCreate = "item:wingweedMat",
					Amount = new OutputAmount
					{
						NoOfItems = 4
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = num * 2.5f
			},
			AgentActionState = agentActionState,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Removing",
			KeyName = "salvageBeds3People",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:beds3People",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[2]
			{
				new Output
				{
					EntityTypeToCreate = "item:bedFrame",
					Amount = new OutputAmount
					{
						NoOfItems = 3
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:textile",
					Amount = new OutputAmount
					{
						NoOfItems = 3
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Removing",
			KeyName = "salvageBeds4People",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:beds4People",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[2]
			{
				new Output
				{
					EntityTypeToCreate = "item:bedFrame",
					Amount = new OutputAmount
					{
						NoOfItems = 4
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:textile",
					Amount = new OutputAmount
					{
						NoOfItems = 4
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Removing",
			KeyName = "salvageFurniture4People",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			IsSalvageProcess = true,
			Stances = GetSalvageStructureStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:furniture4People",
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
					EntityTypeToCreate = "item:furniture",
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
			AgentActionState = AnimAction.Salvaging,
			AgentAnimationStates = null
		});
		SerializableDictionary<string, ChanceToTakeStance[]> stances2 = kneelingProduction;
		float value3 = 2f;
		list.Add(new ProcessType
		{
			Name = "Disassemble",
			SummaryDescription = "This object can be disassembled without losing any parts.",
			KeyName = "salvageFireSuppressantCartridge",
			RequiredSkill = "menial",
			PhysicalWorkFactor = value3,
			IsSalvageProcess = true,
			Stances = stances2,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:fireSuppressantCartridge",
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
					EntityTypeToCreate = "item:emptyCartridge",
					IsWasteProduct = false,
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.00033333336f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Salvage",
			KeyName = "salvageFieldLabPacked",
			RequiredSkill = "menial",
			PhysicalWorkFactor = value3,
			IsSalvageProcess = true,
			Stances = stances2,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:fieldLabPacked",
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
					EntityTypeToCreate = "item:labComponents",
					IsWasteProduct = false,
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Disassemble",
			SummaryDescription = "This object can be disassembled without losing any parts.",
			KeyName = "salvageMusket",
			RequiredSkill = "smithing",
			PhysicalWorkFactor = value3,
			IsSalvageProcess = true,
			Stances = stances2,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:musket",
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
					EntityTypeToCreate = "item:gunBarrelSmoothLong",
					IsWasteProduct = false,
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:gunStock",
					IsWasteProduct = false,
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:flintlockMechanism",
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
			}
		});
		list.Add(new ProcessType
		{
			Name = "Disassemble",
			SummaryDescription = "This object can be disassembled without losing any parts.",
			KeyName = "salvageMusketoon",
			RequiredSkill = "smithing",
			PhysicalWorkFactor = value3,
			IsSalvageProcess = true,
			Stances = stances2,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:musketoon",
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
					EntityTypeToCreate = "item:gunBarrelSmoothShort",
					IsWasteProduct = false,
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:gunStock",
					IsWasteProduct = false,
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:flintlockMechanism",
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
			}
		});
		list.Add(new ProcessType
		{
			Name = "Disassemble",
			SummaryDescription = "This object can be disassembled without losing any parts.",
			KeyName = "salvageGunpowderRifle",
			RequiredSkill = "smithing",
			PhysicalWorkFactor = value3,
			IsSalvageProcess = true,
			Stances = stances2,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:gunpowderRifle",
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
					EntityTypeToCreate = "item:gunBarrelRifled",
					IsWasteProduct = false,
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:gunStock",
					IsWasteProduct = false,
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:flintlockMechanism",
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
			}
		});
		list.Add(new ProcessType
		{
			Name = "Disassemble",
			SummaryDescription = "This object can be disassembled without losing any parts.",
			KeyName = "salvageBoltActionRifle",
			RequiredSkill = "smithing",
			PhysicalWorkFactor = value3,
			IsSalvageProcess = true,
			Stances = stances2,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:boltActionRifle",
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
					EntityTypeToCreate = "item:gunBarrelRifled",
					IsWasteProduct = false,
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:gunStock",
					IsWasteProduct = false,
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:boltActionMechanism",
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
			}
		});
		list.Add(new ProcessType
		{
			Name = "Disassemble",
			SummaryDescription = "This object can be disassembled without losing any parts.",
			KeyName = "salvageSentryItem",
			RequiredSkill = "mechanics",
			PhysicalWorkFactor = value3,
			IsSalvageProcess = true,
			Stances = stances2,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:sentry",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[2]
			{
				new Output
				{
					EntityTypeToCreate = "item:sentryGun",
					IsWasteProduct = false,
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:sentryWeaponMount",
					IsWasteProduct = false,
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Salvage",
			SummaryDescription = "When breaking this object apart, some parts will be retrieved and some may be lost.",
			KeyName = "salvageSpraySentryItem",
			RequiredSkill = "mechanics",
			PhysicalWorkFactor = value3,
			IsSalvageProcess = true,
			Stances = stances2,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:spraySentry",
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
					EntityTypeToCreate = "item:sentryWeaponMount",
					IsWasteProduct = false,
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Salvage",
			SummaryDescription = "When breaking this object apart, some parts will be retrieved and some may be lost.",
			KeyName = "salvageShotgunSentryItem",
			RequiredSkill = "mechanics",
			PhysicalWorkFactor = value3,
			IsSalvageProcess = true,
			Stances = stances2,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:shotgunSentry",
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
					EntityTypeToCreate = "item:sentryWeaponMount",
					IsWasteProduct = false,
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Disassemble",
			SummaryDescription = "This object can be disassembled without losing any parts.",
			KeyName = "salvageShotgun",
			RequiredSkill = "mechanics",
			PhysicalWorkFactor = value3,
			IsSalvageProcess = true,
			Stances = stances2,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:shotgun",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[2]
			{
				new Output
				{
					EntityTypeToCreate = "item:gunBarrelSmoothShort",
					IsWasteProduct = false,
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:gunStock",
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
			}
		});
		list.Add(new ProcessType
		{
			Name = "Disassemble",
			SummaryDescription = "This object can be disassembled without losing any parts.",
			KeyName = "salvageBlackPowderRifleAmmo",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = value3,
			IsSalvageProcess = true,
			Stances = stances2,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:blackPowderRifleAmmo",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[2]
			{
				new Output
				{
					EntityTypeToCreate = "item:goldBullet",
					IsWasteProduct = false,
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:blackPowder",
					IsWasteProduct = false,
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.00033333336f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Disassemble",
			SummaryDescription = "This object can be disassembled without losing any parts.",
			KeyName = "salvageBlackPowderShotAmmo",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = value3,
			IsSalvageProcess = true,
			Stances = stances2,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:blackPowderShotAmmo",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[2]
			{
				new Output
				{
					EntityTypeToCreate = "item:blunderbussBalls",
					IsWasteProduct = false,
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:blackPowder",
					IsWasteProduct = false,
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.00033333336f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Disassemble",
			SummaryDescription = "This object can be disassembled without losing any parts.",
			KeyName = "salvageKnifeSpear",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = value3,
			IsSalvageProcess = true,
			Stances = stances2,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:advancedKnifeSpear",
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
					EntityTypeToCreate = "item:advancedKnife",
					IsWasteProduct = false,
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Disassemble",
			SummaryDescription = "This object can be disassembled without losing any parts.",
			KeyName = "salvageBlacksmithsToolbox",
			RequiredSkill = "menial",
			PhysicalWorkFactor = value3,
			IsSalvageProcess = true,
			Stances = stances2,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:blacksmithsToolbox",
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
					EntityTypeToCreate = "item:hammer",
					IsWasteProduct = false,
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:file",
					IsWasteProduct = false,
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:tongs",
					IsWasteProduct = false,
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.00033333336f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Disassemble",
			SummaryDescription = "This object can be disassembled without losing any parts.",
			KeyName = "salvageMetalworkersToolbox",
			RequiredSkill = "menial",
			PhysicalWorkFactor = value3,
			IsSalvageProcess = true,
			Stances = stances2,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:metalWorkersToolbox",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[5]
			{
				new Output
				{
					EntityTypeToCreate = "item:hammer",
					IsWasteProduct = false,
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:file",
					IsWasteProduct = false,
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:tongs",
					IsWasteProduct = false,
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:handDrill",
					IsWasteProduct = false,
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:hacksaw",
					IsWasteProduct = false,
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.00033333336f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Disassemble",
			SummaryDescription = "This object can be disassembled without losing any parts.",
			KeyName = "salvageCarpentersToolbox",
			RequiredSkill = "menial",
			PhysicalWorkFactor = value3,
			IsSalvageProcess = true,
			Stances = stances2,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:carpentersToolbox",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[5]
			{
				new Output
				{
					EntityTypeToCreate = "item:hammer",
					IsWasteProduct = false,
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:file",
					IsWasteProduct = false,
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:steelHandAxe",
					IsWasteProduct = false,
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:handDrill",
					IsWasteProduct = false,
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:bowSaw",
					IsWasteProduct = false,
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.00033333336f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Upgrading",
			KeyName = "upgradePolymerWorkshop",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "mechanics",
			PhysicalWorkFactor = 4f,
			Stances = kneelingOrStandingProduction,
			Inputs = new Input[3]
			{
				new Input
				{
					Entity = "item:extrusionMachineComponents",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:wroughtIron",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:solidMudBrick",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 3
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:polymerWorkshopUpgrade",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			AgentActionState = AnimAction.Building,
			ProcessToolSetKey = "toolSetAssembleMetalMachine"
		});
		list.Add(new ProcessType
		{
			Name = "Upgrading",
			KeyName = "upgradeMetalLatheShopHumanPowered",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "mechanics",
			PhysicalWorkFactor = 4f,
			Stances = kneelingOrStandingProduction,
			Inputs = new Input[5]
			{
				new Input
				{
					Entity = "item:metalLatheComponents",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:humanPowerUnit",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:blisterSteel",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:wroughtIron",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:thunderChickenTannedHide",
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
					EntityTypeToCreate = "item:metalLatheShopHumanPoweredUpgrade",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			ProcessToolSetKey = "toolSetAssembleMetalMachine",
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Upgrading",
			KeyName = "upgradeCarpenterWorkshop",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "construction",
			PhysicalWorkFactor = 4f,
			Stances = kneelingOrStandingProduction,
			Inputs = new Input[4]
			{
				new Input
				{
					Entity = "item:barClamps",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:solidMudBrick",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 2
					}
				},
				new Input
				{
					Entity = "item:sticks",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 2
					}
				},
				new Input
				{
					Entity = "item:shadeleafCanes",
					IsConsumed = false,
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
					EntityTypeToCreate = "item:carpenterWorkshopUpgrade",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			AgentActionState = AnimAction.Building,
			ProcessToolSetKey = "toolSetCordage"
		});
		list.Add(new ProcessType
		{
			Name = "Upgrading",
			KeyName = "upgradeTextileWorkshop",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "construction",
			PhysicalWorkFactor = 4f,
			Stances = kneelingOrStandingProduction,
			Inputs = new Input[3]
			{
				new Input
				{
					Entity = "item:loomComponents",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:waterCaneStem",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 4
					}
				},
				new Input
				{
					Entity = "item:thunderChickenTannedHide",
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
					EntityTypeToCreate = "item:textileWorkshopUpgrade",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			AgentActionState = AnimAction.Building,
			ProcessToolSetKey = "toolSetCordage"
		});
		list.Add(new ProcessType
		{
			Name = "Upgrading",
			KeyName = "upgradeSimpleStove",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "construction",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:stones",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:solidMudBrick",
					IsConsumed = false,
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
					EntityTypeToCreate = "item:simpleStoveUpgrade",
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
			AgentActionState = AnimAction.Building,
			ProcessToolSetKey = "toolSetSmallDiggingConstruction",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Upgrading",
			KeyName = "upgradeCommunityHall",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "construction",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[3]
			{
				new Input
				{
					Entity = "item:solidMudBrick",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 4
					}
				},
				new Input
				{
					Entity = "item:spoakBranchesTrimmed",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 2
					}
				},
				new Input
				{
					Entity = "item:spoakShingles",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 3
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:communityHallUpgrade",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			AgentActionState = AnimAction.Building,
			ProcessToolSetKey = "toolSetDiggingConstruction",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Upgrading",
			KeyName = "upgradeSmokeOven",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "construction",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:solidMudBrick",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 2
					}
				},
				new Input
				{
					Entity = "item:stones",
					IsConsumed = false,
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
					EntityTypeToCreate = "item:smokeOvenUpgrade",
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
			AgentActionState = AnimAction.Building,
			ProcessToolSetKey = "toolSetSmallDiggingConstruction",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Upgrading",
			KeyName = "upgradeDryingShed",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "construction",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[3]
			{
				new Input
				{
					Entity = "item:solidMudBrick",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:spoakShingles",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:shadeleafCanes",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 2
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:dryingShedUpgrade",
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
			AgentActionState = AnimAction.Building,
			ProcessToolSetKey = "toolSetSmallDiggingConstruction",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		SerializableDictionary<string, ChanceToTakeStance[]> stances3 = kneelingProduction;
		AnimAction agentActionState2 = AnimAction.Mending;
		float num2 = 0.004f;
		list.Add(new ProcessType
		{
			Name = "Upgrading",
			KeyName = "upgradeWingweedMats1People",
			JobTypeKey = "constructionJobType",
			RequiredSkill = null,
			PhysicalWorkFactor = 4f,
			Stances = stances3,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:wingweedMat",
					IsConsumed = false,
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
					EntityTypeToCreate = "item:wingweedMats1People",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f * num2
			},
			AgentActionState = agentActionState2
		});
		list.Add(new ProcessType
		{
			Name = "Upgrading",
			KeyName = "upgradeWingweedMats2People",
			JobTypeKey = "constructionJobType",
			RequiredSkill = null,
			PhysicalWorkFactor = 4f,
			Stances = stances3,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:wingweedMat",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 2
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:wingweedMats2People",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1.5f * num2
			},
			AgentActionState = agentActionState2
		});
		list.Add(new ProcessType
		{
			Name = "Upgrading",
			KeyName = "upgradeWingweedMats3People",
			JobTypeKey = "constructionJobType",
			RequiredSkill = null,
			PhysicalWorkFactor = 4f,
			Stances = stances3,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:wingweedMat",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 3
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:wingweedMats3People",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 2f * num2
			},
			AgentActionState = agentActionState2
		});
		list.Add(new ProcessType
		{
			Name = "Upgrading",
			KeyName = "upgradeWingweedMats4People",
			JobTypeKey = "constructionJobType",
			RequiredSkill = null,
			PhysicalWorkFactor = 4f,
			Stances = stances3,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:wingweedMat",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 4
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:wingweedMats4People",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 2.5f * num2
			},
			AgentActionState = agentActionState2
		});
		SerializableDictionary<string, ChanceToTakeStance[]> stances4 = kneelingProduction;
		AnimAction agentActionState3 = AnimAction.Mending;
		float num3 = 1f / 150f;
		list.Add(new ProcessType
		{
			Name = "Upgrading",
			KeyName = "upgradeBeds3People",
			JobTypeKey = "constructionJobType",
			RequiredSkill = null,
			PhysicalWorkFactor = 4f,
			Stances = stances4,
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:bedFrame",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 3
					}
				},
				new Input
				{
					Entity = "item:textile",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 3
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:beds3People",
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
			AgentActionState = agentActionState3
		});
		list.Add(new ProcessType
		{
			Name = "Upgrading",
			KeyName = "upgradeBeds4People",
			JobTypeKey = "constructionJobType",
			RequiredSkill = null,
			PhysicalWorkFactor = 4f,
			Stances = stances4,
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:bedFrame",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 4
					}
				},
				new Input
				{
					Entity = "item:textile",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 4
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:beds4People",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 2.5f * num3
			},
			AgentActionState = agentActionState3
		});
		list.Add(new ProcessType
		{
			Name = "Upgrading",
			KeyName = "upgradeFurniture4People",
			JobTypeKey = "constructionJobType",
			RequiredSkill = null,
			PhysicalWorkFactor = 4f,
			Stances = stances4,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:furniture",
					IsConsumed = false,
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
					EntityTypeToCreate = "item:furniture4People",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f * num3
			},
			AgentActionState = agentActionState3
		});
		float value4 = 2.2f;
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeCleanTurnipGuts",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 2.2f,
			Stances = GetToolMakingStances(),
			WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart,
			Inputs = new Input[3]
			{
				new Input
				{
					Entity = "item:turnipGuts",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 10
					}
				},
				new Input
				{
					Entity = "item:turnipBrain",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:salt",
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
					EntityTypeToCreate = "item:improvisedGreenHouseCover",
					Amount = new OutputAmount
					{
						NoOfItems = 10
					},
					ToolContainerTagsToPlaceIn = new string[1] { "liquidContainerNoHeat" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			ProcessToolSetKey = "toolSetDissolveInedibleMatter",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeThunderChickenRawhide",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 2.2f,
			Stances = GetToolMakingStances(),
			WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:thunderChickenGreenHide",
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
					EntityTypeToCreate = "item:thunderChickenRawhide",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					},
					ToolContainerTagsToPlaceIn = new string[1] { "hideRack" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			ProcessToolSetKey = "toolSetCleanHide",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeThunderChickenTannedHide",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 2.2f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:thunderChickenRawhide",
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
					EntityTypeToCreate = "item:thunderChickenTannedHide",
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
			ProcessToolSetKey = "toolSetFireplace",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeWhipjawRawhide",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 2.2f,
			Stances = GetToolMakingStances(),
			WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:whipjawGreenHide",
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
					EntityTypeToCreate = "item:whipjawRawhide",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					},
					ToolContainerTagsToPlaceIn = new string[1] { "hideRack" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			ProcessToolSetKey = "toolSetCleanHide",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeWhipjawTannedHide",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 2.2f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:whipjawRawhide",
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
					EntityTypeToCreate = "item:whipjawTannedHide",
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
			ProcessToolSetKey = "toolSetFireplace",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeMegapodRawhide",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 2.2f,
			Stances = GetToolMakingStances(),
			WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:megapodGreenHide",
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
					EntityTypeToCreate = "item:megapodRawhide",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					},
					ToolContainerTagsToPlaceIn = new string[1] { "hideRack" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			ProcessToolSetKey = "toolSetCleanHide",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeMegapodTannedHide",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 2.2f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:megapodRawhide",
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
					EntityTypeToCreate = "item:megapodTannedHide",
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
			ProcessToolSetKey = "toolSetFireplace",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeOrganicFertilizer",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "farming",
			PhysicalWorkFactor = 2.2f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:organicMatter",
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
					EntityTypeToCreate = "item:organicFertilizer",
					Amount = new OutputAmount
					{
						Bulk = new Bulk
						{
							FractionOfInputBulk = 1f
						}
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f,
				MultiplyByBulk = true
			},
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeLandMine",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 2.2f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[3]
			{
				new Input
				{
					Entity = "item:blackPowder",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:flintlockMechanism",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:waterCaneStem",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 2
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:landMine",
					Amount = new OutputAmount
					{
						NoOfItems = 4
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			ProcessToolSetKey = "toolSetBluntTool",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeVarmintBomb",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 2.2f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:blackPowder",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:waterCaneStem",
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
					EntityTypeToCreate = "item:varmintBomb",
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
			ProcessToolSetKey = "toolSetBluntTool",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeBlackPowderRifleAmmo",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 2.2f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:blackPowder",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:goldBullet",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 4
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:blackPowderRifleAmmo",
					Amount = new OutputAmount
					{
						NoOfItems = 4
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.00033333336f
			},
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeBlackPowderShotAmmo",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 2.2f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:blackPowder",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:blunderbussBalls",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 4
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:blackPowderShotAmmo",
					Amount = new OutputAmount
					{
						NoOfItems = 4
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.00033333336f
			},
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeGoldBullet",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "chemistry",
			PhysicalWorkFactor = 2.2f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:gold",
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
					EntityTypeToCreate = "item:goldBullet",
					Amount = new OutputAmount
					{
						NoOfItems = 4
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			ProcessToolSetKey = "toolSetBulletCasting",
			AgentActionState = AnimAction.Mending,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Metal }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeSandMold",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "chemistry",
			PhysicalWorkFactor = 4f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:sticks",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:clay",
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
					EntityTypeToCreate = "item:sandMold",
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
			ProcessToolSetKey = "toolSetShapenSmallWood",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeGoldPot",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "chemistry",
			PhysicalWorkFactor = 8f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:gold",
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
					EntityTypeToCreate = "item:goldPot",
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
			ProcessToolSetKey = "toolSetPrimitiveCasting",
			AgentActionState = AnimAction.Mending,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeMetalLatheComponents",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "mechanics",
			PhysicalWorkFactor = 8f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:gold",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 2
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:metalLatheComponents",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			ProcessToolSetKey = "toolSetSimpleCasting",
			AgentActionState = AnimAction.Mending,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeExtrusionMachineComponents",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "mechanics",
			PhysicalWorkFactor = 8f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:gold",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 2
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:extrusionMachineComponents",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			ProcessToolSetKey = "toolSetSimpleCasting",
			AgentActionState = AnimAction.Mending,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeGoldSheet",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "chemistry",
			PhysicalWorkFactor = 8f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:gold",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 2
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:goldSheet",
					Amount = new OutputAmount
					{
						NoOfItems = 2
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			ProcessToolSetKey = "toolSetSimpleCasting",
			AgentActionState = AnimAction.Mending,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeStillComponents",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "smithing",
			PhysicalWorkFactor = 8f,
			Stances = GetSmithingStance(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:goldSheet",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 2
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:stillComponents",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			ProcessToolSetKey = "toolSetSimpleForging",
			AgentActionState = AnimAction.Mending,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Metal }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "humanPowerUnitComponents",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "mechanics",
			PhysicalWorkFactor = 6f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:gold",
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
					EntityTypeToCreate = "item:humanPowerUnitComponents",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			ProcessToolSetKey = "toolSetSimpleCasting",
			AgentActionState = AnimAction.Mending,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeHumanPowerUnit",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "mechanics",
			PhysicalWorkFactor = 4f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:sticks",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 2
					}
				},
				new Input
				{
					Entity = "item:humanPowerUnitComponents",
					IsConsumed = false,
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
					EntityTypeToCreate = "item:humanPowerUnit",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			ProcessToolSetKey = "toolSetAssembleMetalMachine",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeBlackPowder",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 2.2f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[3]
			{
				new Input
				{
					Entity = "item:saltpeter",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:charcoal",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:sulfurPowder",
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
					EntityTypeToCreate = "item:blackPowder",
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
			ProcessToolSetKey = "toolSetMixingBlackPowder",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeTappingBucket",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 2.2f,
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:clayJar",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:sticks",
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
					EntityTypeToCreate = "item:tappingBucket",
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
			ProcessToolSetKey = "toolSetShapenSmallWood",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Disassemble",
			KeyName = "salvageTappingBucket",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 2f,
			IsSalvageProcess = true,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:tappingBucket",
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
					EntityTypeToCreate = "item:clayJar",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.00033333336f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makePlasticTappingBucket",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 2.2f,
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:improvisedPlasticJar",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:sticks",
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
					EntityTypeToCreate = "item:plasticTappingBucket",
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
			ProcessToolSetKey = "toolSetShapenSmallWood",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Disassemble",
			KeyName = "salvagePlasticTappingBucket",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 2f,
			IsSalvageProcess = true,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:plasticTappingBucket",
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
					EntityTypeToCreate = "item:improvisedPlasticJar",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.00033333336f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeVat",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 4f,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:spoakShingles",
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
					EntityTypeToCreate = "item:vat",
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
			ProcessToolSetKey = "toolSetCombineLightImprovisedObjects",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeSaltpeterSolution",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 4f,
			WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:guano",
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
					EntityTypeToCreate = "item:saltpeterSolution",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					},
					ToolContainerTagsToPlaceIn = new string[1] { "liquidContainerNoHeat" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 71f / (226f * (float)Math.PI)
			},
			ProcessToolSetKey = "toolSetDissolveInedibleMatter",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeSaltpeter",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 4f,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:saltpeterSolution",
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
					EntityTypeToCreate = "item:saltpeter",
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
			ProcessToolSetKey = "toolSetMakeStew",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeGuanoFertilizer",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "farming",
			PhysicalWorkFactor = 4f,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:guano",
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
					EntityTypeToCreate = "item:guanoFertilizer",
					Amount = new OutputAmount
					{
						NoOfItems = 2
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeSulfurPowder",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 4f,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:sulfurBlocks",
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
					EntityTypeToCreate = "item:sulfurPowder",
					Amount = new OutputAmount
					{
						NoOfItems = 3
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.0033333334f
			},
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeFlintlockMechanism",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "smithing",
			PhysicalWorkFactor = 4f,
			Stances = GetSmithingStance(),
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:wroughtIron",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:flintRough",
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
					EntityTypeToCreate = "item:flintlockMechanism",
					Amount = new OutputAmount
					{
						NoOfItems = 2
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 75f
			},
			ProcessToolSetKey = "toolSetHarderImprovisedForging",
			AgentActionState = AnimAction.Mending,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Metal }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeBlunderbussBalls",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "smithing",
			PhysicalWorkFactor = 4f,
			Stances = GetSmithingStance(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:gold",
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
					EntityTypeToCreate = "item:blunderbussBalls",
					Amount = new OutputAmount
					{
						NoOfItems = 4
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 75f
			},
			ProcessToolSetKey = "toolSetHarderImprovisedForging",
			AgentActionState = AnimAction.Mending,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Metal }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeSpikeTrap",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "smithing",
			PhysicalWorkFactor = 4f,
			Stances = GetSmithingStance(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:wroughtIron",
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
					EntityTypeToCreate = "item:spikeTrap",
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
			ProcessToolSetKey = "toolSetHarderImprovisedForging",
			AgentActionState = AnimAction.Mending,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Metal }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeBellows",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 4f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:thunderChickenTannedHide",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:sticks",
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
					EntityTypeToCreate = "item:bellows",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			ProcessToolSetKey = "toolSetCombineLightImprovisedObjects",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeRawhideString",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 4f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:thunderChickenRawhide",
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
					EntityTypeToCreate = "item:rawhideString",
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
			ProcessToolSetKey = "toolSetButcherFlesh",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeBlowpipe",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 4f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:waterCaneStem",
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
					EntityTypeToCreate = "item:blowpipe",
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
			ProcessToolSetKey = "toolSetShapenSmallWood",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeHammer",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "smithing",
			PhysicalWorkFactor = 8f,
			Stances = GetSmithingStance(),
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:wroughtIron",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:sticks",
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
					EntityTypeToCreate = "item:hammer",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.02f
			},
			ProcessToolSetKey = "toolSetImprovisedForging",
			AgentActionState = AnimAction.Mending,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Metal }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeStoneHammer",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 2.2f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:stones",
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
					EntityTypeToCreate = "item:stoneHammer",
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
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeGunStock",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 4f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:sticks",
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
					EntityTypeToCreate = "item:gunStock",
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
			ProcessToolSetKey = "toolSetShapenSmallWood",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeGunBarrelUnbored",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "smithing",
			PhysicalWorkFactor = 8f,
			Stances = GetSmithingStance(),
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:wroughtIron",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:blisterSteel",
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
					EntityTypeToCreate = "item:gunBarrelUnbored",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			ProcessToolSetKey = "toolSetSimpleForging",
			AgentActionState = AnimAction.Mending,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Metal }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeGunBarrelSmoothLong",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "smithing",
			PhysicalWorkFactor = 4f,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:gunBarrelUnbored",
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
					EntityTypeToCreate = "item:gunBarrelSmoothLong",
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
			ProcessToolSetKey = "toolSetBarrelBoring",
			AgentActionState = AnimAction.Mending,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeGunBarrelSmoothShort",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "smithing",
			PhysicalWorkFactor = 4f,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:gunBarrelSmoothLong",
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
					EntityTypeToCreate = "item:gunBarrelSmoothShort",
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
			ProcessToolSetKey = "toolSetBarrelBoring",
			AgentActionState = AnimAction.Mending,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeGunBarrelRifled",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "smithing",
			PhysicalWorkFactor = 4f,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:gunBarrelSmoothLong",
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
					EntityTypeToCreate = "item:gunBarrelRifled",
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
			ProcessToolSetKey = "toolSetMetalLathe",
			AgentActionState = AnimAction.Mending,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeGunpowderRifle",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "smithing",
			Stances = GetSmithingStance(),
			PhysicalWorkFactor = 6f,
			Inputs = new Input[3]
			{
				new Input
				{
					Entity = "item:gunBarrelRifled",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:gunStock",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:flintlockMechanism",
					IsConsumed = false,
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
					EntityTypeToCreate = "item:gunpowderRifle",
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
			ProcessToolSetKey = "toolSetSimpleForging",
			AgentActionState = AnimAction.Mending,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Metal }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeMusketoon",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "smithing",
			Stances = GetSmithingStance(),
			PhysicalWorkFactor = 6f,
			Inputs = new Input[3]
			{
				new Input
				{
					Entity = "item:gunBarrelSmoothShort",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:gunStock",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:flintlockMechanism",
					IsConsumed = false,
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
					EntityTypeToCreate = "item:musketoon",
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
			ProcessToolSetKey = "toolSetSimpleForging",
			AgentActionState = AnimAction.Mending,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Metal }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeMusket",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "smithing",
			Stances = GetSmithingStance(),
			PhysicalWorkFactor = 6f,
			Inputs = new Input[3]
			{
				new Input
				{
					Entity = "item:gunBarrelSmoothLong",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:gunStock",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:flintlockMechanism",
					IsConsumed = false,
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
					EntityTypeToCreate = "item:musket",
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
			ProcessToolSetKey = "toolSetSimpleForging",
			AgentActionState = AnimAction.Mending,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Metal }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeAnvil",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "smithing",
			Stances = GetSmithingStance(),
			PhysicalWorkFactor = 8f,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:wroughtIron",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 5
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:anvil",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			ProcessToolSetKey = "toolSetImprovisedForging",
			AgentActionState = AnimAction.Mending,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Metal }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeSteelHoe",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "smithing",
			Stances = GetSmithingStance(),
			PhysicalWorkFactor = 8f,
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:blisterSteel",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:waterCaneStem",
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
					EntityTypeToCreate = "item:steelHoe",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.02f
			},
			ProcessToolSetKey = "toolSetSimpleForging",
			AgentActionState = AnimAction.Mending,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Metal }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeIronHooks",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "smithing",
			Stances = GetSmithingStance(),
			PhysicalWorkFactor = 4f,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:wroughtIron",
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
					EntityTypeToCreate = "item:ironHooks",
					Amount = new OutputAmount
					{
						NoOfItems = 5
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 75f
			},
			ProcessToolSetKey = "toolSetImprovisedForging",
			AgentActionState = AnimAction.Mending,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Metal }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeSteelKnife",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "smithing",
			Stances = GetSmithingStance(),
			PhysicalWorkFactor = 8f,
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:blisterSteel",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:sticks",
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
					EntityTypeToCreate = "item:steelKnife",
					Amount = new OutputAmount
					{
						NoOfItems = 2
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 75f
			},
			ProcessToolSetKey = "toolSetSimpleForging",
			AgentActionState = AnimAction.Mending,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Metal }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeTinnerSnips",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "smithing",
			Stances = GetSmithingStance(),
			PhysicalWorkFactor = 8f,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:blisterSteel",
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
					EntityTypeToCreate = "item:tinnerSnips",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.02f
			},
			ProcessToolSetKey = "toolSetSimpleForging",
			AgentActionState = AnimAction.Mending,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Metal }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeTongs",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "smithing",
			Stances = GetSmithingStance(),
			PhysicalWorkFactor = 8f,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:wroughtIron",
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
					EntityTypeToCreate = "item:tongs",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.02f
			},
			ProcessToolSetKey = "toolSetImprovisedForging",
			AgentActionState = AnimAction.Mending,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Metal }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeFile",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "smithing",
			Stances = GetSmithingStance(),
			PhysicalWorkFactor = 8f,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:wroughtIron",
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
					EntityTypeToCreate = "item:file",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.02f
			},
			ProcessToolSetKey = "toolSetImprovisedForging",
			AgentActionState = AnimAction.Mending,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Metal }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeBarClamps",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "smithing",
			PhysicalWorkFactor = 8f,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:wroughtIron",
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
					EntityTypeToCreate = "item:barClamps",
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
			ProcessToolSetKey = "toolSetHarderImprovisedForging",
			AgentActionState = AnimAction.Mending,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Metal }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeHandDrill",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "smithing",
			Stances = GetSmithingStance(),
			PhysicalWorkFactor = 8f,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:blisterSteel",
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
					EntityTypeToCreate = "item:handDrill",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			ProcessToolSetKey = "toolSetSimpleForging",
			AgentActionState = AnimAction.Mending,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Metal }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeHacksaw",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "smithing",
			Stances = GetSmithingStance(),
			PhysicalWorkFactor = 8f,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:blisterSteel",
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
					EntityTypeToCreate = "item:hacksaw",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.02f
			},
			ProcessToolSetKey = "toolSetSimpleForging",
			AgentActionState = AnimAction.Mending,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Metal }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeBowSaw",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "smithing",
			Stances = GetSmithingStance(),
			PhysicalWorkFactor = 8f,
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:blisterSteel",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:waterCaneStem",
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
					EntityTypeToCreate = "item:bowSaw",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.02f
			},
			ProcessToolSetKey = "toolSetSimpleForging",
			AgentActionState = AnimAction.Mending,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Metal }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeBlacksmithsToolbox",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 2.2f,
			Inputs = new Input[3]
			{
				new Input
				{
					Entity = "item:file",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:tongs",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:hammer",
					IsConsumed = false,
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
					EntityTypeToCreate = "item:blacksmithsToolbox",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.00033333336f
			},
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeMetalworkersToolbox",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 2.2f,
			Inputs = new Input[3]
			{
				new Input
				{
					Entity = "item:blacksmithsToolbox",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:handDrill",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:hacksaw",
					IsConsumed = false,
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
					EntityTypeToCreate = "item:metalWorkersToolbox",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.00033333336f
			},
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeCarpentersToolbox",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 2.2f,
			Inputs = new Input[5]
			{
				new Input
				{
					Entity = "item:file",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:handDrill",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:hammer",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:steelHandAxe",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:bowSaw",
					IsConsumed = false,
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
					EntityTypeToCreate = "item:carpentersToolbox",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.00033333336f
			},
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeSteelMachete",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "smithing",
			Stances = GetSmithingStance(),
			PhysicalWorkFactor = 8f,
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:blisterSteel",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:sticks",
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
					EntityTypeToCreate = "item:steelMachete",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.02f
			},
			ProcessToolSetKey = "toolSetSimpleForging",
			AgentActionState = AnimAction.Mending,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Metal }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeImprovisedAxe",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = value4,
			Stances = GetToolMakingStances(),
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:scrapMetal",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:waterCaneStem",
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
					EntityTypeToCreate = "item:improvisedHandAxe",
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
			ProcessToolSetKey = "toolSetMakeImprovisedTool",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeSteelHandAxe",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "smithing",
			Stances = GetSmithingStance(),
			PhysicalWorkFactor = 8f,
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:blisterSteel",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:waterCaneStem",
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
					EntityTypeToCreate = "item:steelHandAxe",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.02f
			},
			ProcessToolSetKey = "toolSetSimpleForging",
			AgentActionState = AnimAction.Mending,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Metal }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeImprovisedPickaxe",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 2.2f,
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:scrapMetal",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:waterCaneStem",
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
					EntityTypeToCreate = "item:improvisedPickaxe",
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
			ProcessToolSetKey = "toolSetSharpenBladeAndtoolSetAttachWoodAndMetal",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeSteelPickaxe",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "smithing",
			Stances = GetSmithingStance(),
			PhysicalWorkFactor = 8f,
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:blisterSteel",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:waterCaneStem",
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
					EntityTypeToCreate = "item:steelPickaxe",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.02f
			},
			ProcessToolSetKey = "toolSetImprovisedForging",
			AgentActionState = AnimAction.Mending,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Metal }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeImprovisedSpade",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = value4,
			Stances = GetToolMakingStances(),
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:scrapMetal",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:waterCaneStem",
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
					EntityTypeToCreate = "item:improvisedSpade",
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
			ProcessToolSetKey = "toolSetMakeImprovisedTool",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeSteelSpade",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "smithing",
			Stances = GetSmithingStance(),
			PhysicalWorkFactor = 8f,
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:blisterSteel",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:waterCaneStem",
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
					EntityTypeToCreate = "item:steelSpade",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.02f
			},
			ProcessToolSetKey = "toolSetSimpleForging",
			AgentActionState = AnimAction.Mending,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Metal }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeTurnipCracker",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "smithing",
			Stances = GetSmithingStance(),
			PhysicalWorkFactor = 8f,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:wroughtIron",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 2
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:turnipCracker",
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
			ProcessToolSetKey = "toolSetHarderImprovisedForging",
			AgentActionState = AnimAction.Mending,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Metal }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeIronSpear",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "smithing",
			PhysicalWorkFactor = 2.2f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:waterCaneStem",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:wroughtIron",
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
					EntityTypeToCreate = "item:ironSpear",
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
			ProcessToolSetKey = "toolSetHarderImprovisedForging",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeIronArrow",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "smithing",
			PhysicalWorkFactor = 2.2f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[3]
			{
				new Input
				{
					Entity = "item:improvisedArrowShaftBundle",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:waterCaneLeaves",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:wroughtIron",
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
					EntityTypeToCreate = "item:ironArrow",
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
			ProcessToolSetKey = "toolSetHarderImprovisedForging",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeRoughBloomIron",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "smithing",
			Stances = GetSmithingStance(),
			PhysicalWorkFactor = 8f,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:bogOre",
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
					EntityTypeToCreate = "item:roughBloomIron",
					Amount = new OutputAmount
					{
						NoOfItems = 5
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			ProcessToolSetKey = "toolSetImprovisedForging",
			AgentActionState = AnimAction.Mending,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Metal }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeGold",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "chemistry",
			PhysicalWorkFactor = 8f,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:goldOre",
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
					EntityTypeToCreate = "item:gold",
					Amount = new OutputAmount
					{
						NoOfItems = 5
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f,
				MultiplyByBulk = true
			},
			ProcessToolSetKey = "toolSetSimpleCasting",
			AgentActionState = AnimAction.Mending,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Metal }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeWroughtIron",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "smithing",
			Stances = GetSmithingStance(),
			PhysicalWorkFactor = 8f,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:roughBloomIron",
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
					EntityTypeToCreate = "item:wroughtIron",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f,
				MultiplyByBulk = true
			},
			ProcessToolSetKey = "toolSetImprovisedForging",
			AgentActionState = AnimAction.Mending,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Metal }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeBlisterSteel",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "smithing",
			Stances = GetSmithingStance(),
			PhysicalWorkFactor = 8f,
			WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:wroughtIron",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 4
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:blisterSteel",
					Amount = new OutputAmount
					{
						NoOfItems = 4
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 71f / (226f * (float)Math.PI),
				MultiplyByBulk = true
			},
			ProcessToolSetKey = "toolSetKilnBigAndSmall",
			AgentActionState = AnimAction.Mending,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Metal }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeRareMetal",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 2.2f,
			Stances = GetToolMakingStances(),
			WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:scandiumOre",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 3
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:scandium",
					Amount = new OutputAmount
					{
						NoOfItems = 3
					},
					ToolContainerTagsToPlaceIn = new string[1] { "rareMetalRefinery" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			ProcessToolSetKey = "toolSetRefineRareMetal",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeRareMetal2",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 2.2f,
			Stances = GetToolMakingStances(),
			WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:terbiumOre",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 3
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:terbium",
					Amount = new OutputAmount
					{
						NoOfItems = 3
					},
					ToolContainerTagsToPlaceIn = new string[1] { "rareMetalRefinery" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			ProcessToolSetKey = "toolSetRefineRareMetal",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeCharcoal",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 4f,
			WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart,
			Stances = GetToolMakingStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:firewood",
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
					EntityTypeToCreate = "item:charcoal",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					},
					ToolContainerTagsToPlaceIn = new string[1] { "kiln" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			ProcessToolSetKey = "toolSetKilnBigAndSmall",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeDryPeat",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			Stances = standingProduction,
			WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:wetPeat",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 10
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:dryPeat",
					Amount = new OutputAmount
					{
						NoOfItems = 10
					},
					ToolContainerTagsToPlaceIn = new string[1] { "peatStackTool" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 71f / (226f * (float)Math.PI)
			},
			ProcessToolSetKey = "toolSetDryingPeat",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeFirewood",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			Stances = standingProduction,
			WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:wetFirewood",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 10
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:firewood",
					Amount = new OutputAmount
					{
						NoOfItems = 10
					},
					ToolContainerTagsToPlaceIn = new string[1] { "woodpileTool" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 71f / (226f * (float)Math.PI)
			},
			ProcessToolSetKey = "toolSetDryingFirewood",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeImprovisedPlasticJar",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 2f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:panelScraps",
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
					EntityTypeToCreate = "item:improvisedPlasticJar",
					Amount = new OutputAmount
					{
						NoOfItems = 2
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			ProcessToolSetKey = "toolSetShapenSmallWood",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeUnfiredClayJar",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 2f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:clay",
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
					EntityTypeToCreate = "item:unfiredClayJar",
					Amount = new OutputAmount
					{
						NoOfItems = 2
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			ProcessToolSetKey = "toolSetShapePottery",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeUnfiredBulletMold",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "chemistry",
			PhysicalWorkFactor = 2f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:clay",
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
					EntityTypeToCreate = "item:unfiredBulletMold",
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
			ProcessToolSetKey = "toolSetShapePottery",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeBulletMold",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "chemistry",
			PhysicalWorkFactor = 4f,
			WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart,
			Stances = GetToolMakingStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:unfiredBulletMold",
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
					EntityTypeToCreate = "item:bulletMold",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					},
					ToolContainerTagsToPlaceIn = new string[1] { "kiln" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			ProcessToolSetKey = "toolSetKilnBigAndSmall",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeUnfinishedFirebricks",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "chemistry",
			PhysicalWorkFactor = 2f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:clay",
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
					EntityTypeToCreate = "item:unfinishedFirebricks",
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
			ProcessToolSetKey = "toolSetShapePottery",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeFirebricks",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "chemistry",
			PhysicalWorkFactor = 4f,
			WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart,
			Stances = GetToolMakingStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:unfinishedFirebricks",
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
					EntityTypeToCreate = "item:firebricks",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					},
					ToolContainerTagsToPlaceIn = new string[1] { "kiln" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			ProcessToolSetKey = "toolSetKilnBig",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeGlazedClayJar",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 4f,
			WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart,
			Stances = GetToolMakingStances(),
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:salt",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:unfiredClayJar",
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
					EntityTypeToCreate = "item:clayJar",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					},
					ToolContainerTagsToPlaceIn = new string[1] { "kiln" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			ProcessToolSetKey = "toolSetKilnBig",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeUnfiredClayPot",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 2f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:clay",
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
					EntityTypeToCreate = "item:unfiredClayPot",
					Amount = new OutputAmount
					{
						NoOfItems = 2
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			ProcessToolSetKey = "toolSetShapePottery",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeClayPotUnglazed",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 4f,
			WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart,
			Stances = GetToolMakingStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:unfiredClayPot",
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
					EntityTypeToCreate = "item:clayPotUnglazed",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					},
					ToolContainerTagsToPlaceIn = new string[1] { "kiln" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			ProcessToolSetKey = "toolSetKilnBigAndSmall",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Cultivating",
			KeyName = "makeFavorbread",
			JobTypeKey = "sowingAndHarvestingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 2f,
			Stances = kneelingProduction,
			WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:blackpulp",
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
					EntityTypeToCreate = "item:favorbread",
					Amount = new OutputAmount
					{
						NoOfItems = 8
					},
					ToolContainerTagsToPlaceIn = new string[1] { "favorbreadFarm" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.3f
			},
			ProcessToolSetKey = "toolSetFavorbreadFarm",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Extracting deposit",
			KeyName = "makeClayFromPit",
			JobTypeKey = "extractFromDepositJobType",
			RequiredSkill = "menial",
			MoveOutputToWorkerWhenCompleted = false,
			PhysicalWorkFactor = 6f,
			Stances = kneelingOrStandingProduction,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:clay",
					Amount = new OutputAmount
					{
						NoOfItems = 4
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 2f / 75f
			},
			AgentActionState = AnimAction.Digging,
			ProcessToolSetKey = "toolSetClayPit"
		});
		list.Add(new ProcessType
		{
			Name = "Extracting deposit",
			KeyName = "makeSaltFromMine",
			JobTypeKey = "extractFromDepositJobType",
			RequiredSkill = "menial",
			MoveOutputToWorkerWhenCompleted = false,
			PhysicalWorkFactor = 6f,
			Stances = kneelingOrStandingProduction,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:salt",
					Amount = new OutputAmount
					{
						NoOfItems = 4
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 2f / 75f
			},
			AgentActionState = AnimAction.Digging,
			ProcessToolSetKey = "toolSetSaltMine"
		});
		list.Add(new ProcessType
		{
			Name = "Extracting deposit",
			KeyName = "makeBogOreFromPit",
			JobTypeKey = "extractFromDepositJobType",
			MoveOutputToWorkerWhenCompleted = false,
			PhysicalWorkFactor = 6f,
			Stances = kneelingOrStandingProduction,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:bogOre",
					Amount = new OutputAmount
					{
						NoOfItems = 4
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 2f / 75f
			},
			AgentActionState = AnimAction.Digging,
			ProcessToolSetKey = "toolSetBogOrePit"
		});
		list.Add(new ProcessType
		{
			Name = "Extracting deposit",
			KeyName = "makeRareMetalOreFromPit",
			JobTypeKey = "extractFromDepositJobType",
			MoveOutputToWorkerWhenCompleted = false,
			PhysicalWorkFactor = 6f,
			Stances = kneelingOrStandingProduction,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:scandiumOre",
					Amount = new OutputAmount
					{
						NoOfItems = 4
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 2f / 75f
			},
			AgentActionState = AnimAction.Digging,
			ProcessToolSetKey = "toolSetRareMetalOrePit"
		});
		list.Add(new ProcessType
		{
			Name = "Extracting deposit",
			KeyName = "makeRareMetalOreFromPit2",
			JobTypeKey = "extractFromDepositJobType",
			MoveOutputToWorkerWhenCompleted = false,
			PhysicalWorkFactor = 6f,
			Stances = kneelingOrStandingProduction,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:terbiumOre",
					Amount = new OutputAmount
					{
						NoOfItems = 4
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 2f / 75f
			},
			AgentActionState = AnimAction.Digging,
			ProcessToolSetKey = "toolSetRareMetalOrePit2"
		});
		list.Add(new ProcessType
		{
			Name = "Extracting deposit",
			KeyName = "makeWetPeatFromBank",
			JobTypeKey = "extractFromDepositJobType",
			RequiredSkill = "menial",
			MoveOutputToWorkerWhenCompleted = false,
			PhysicalWorkFactor = 6f,
			Stances = standingProduction,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:wetPeat",
					Amount = new OutputAmount
					{
						NoOfItems = 4
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 2f / 75f
			},
			AgentActionState = AnimAction.Tilling,
			ProcessToolSetKey = "toolSetPeatBank"
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeSolidMudBrick",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "construction",
			PhysicalWorkFactor = 4f,
			WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart,
			Stances = GetToolMakingStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:wetMudBrick",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 3
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:solidMudBrick",
					Amount = new OutputAmount
					{
						NoOfItems = 3
					},
					ToolContainerTagsToPlaceIn = new string[1] { "kiln" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			ProcessToolSetKey = "toolSetKilnBig",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "mudBrickMolding",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:clay",
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
					EntityTypeToCreate = "item:wetMudBrick",
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
			ProcessToolSetKey = "toolSetMold",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeBrickMold",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "construction",
			PhysicalWorkFactor = 4f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:sticks",
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
					EntityTypeToCreate = "item:brickMold",
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
			ProcessToolSetKey = "toolSetShapenSmallWood",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Chopping",
			KeyName = "chopFirewood",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:spoakBranches",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[2]
			{
				new Output
				{
					EntityTypeToCreate = "item:wetFirewood",
					Amount = new OutputAmount
					{
						NoOfItems = 2
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:spoakLeaves",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					},
					IsWasteProduct = true
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.0033333334f
			},
			ProcessToolSetKey = "toolSetChopToughWood",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Trimming",
			KeyName = "trimSpoakBranches",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:spoakBranches",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[2]
			{
				new Output
				{
					EntityTypeToCreate = "item:spoakBranchesTrimmed",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:spoakLeaves",
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
			ProcessToolSetKey = "toolSetChopWeakWood",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeWoodenCookingPot",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = value4,
			Stances = GetToolMakingStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:giantHollowBud",
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
					EntityTypeToCreate = "item:woodenCookingPot",
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
			ProcessToolSetKey = "toolSetShapenSmallWood",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeImprovisedKnife",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = value4,
			Stances = GetToolMakingStances(),
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:scrapMetal",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:sticks",
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
					EntityTypeToCreate = "item:improvisedKnife",
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
			ProcessToolSetKey = "toolSetMakeImprovisedTool",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Cooking",
			KeyName = "makeSulfurSmokeBomb",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 2f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:sulfurBlocks",
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
					EntityTypeToCreate = "item:sulfurSmokeBomb",
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
			ProcessToolSetKey = "toolSetMakeSulfurSmokeBomb",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeImprovisedTrowel",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = value4,
			Stances = GetToolMakingStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:sticks",
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
					EntityTypeToCreate = "item:improvisedTrowel",
					Amount = new OutputAmount
					{
						NoOfItems = 2
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeBluntKnife",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = value4,
			Stances = GetToolMakingStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:sticks",
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
					EntityTypeToCreate = "item:bluntKnife",
					Amount = new OutputAmount
					{
						NoOfItems = 2
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeFlintKnife",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = value4,
			Stances = GetToolMakingStances(),
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:flintRough",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:sticks",
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
					EntityTypeToCreate = "item:flintKnife",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			ProcessToolSetKey = "toolSetShapenSmallWood",
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 75f
			},
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeStrongBugNet",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 2.2f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:sticks",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:textile",
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
					EntityTypeToCreate = "item:strongBugNet",
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
			ProcessToolSetKey = "toolSetAttachWoodAndMetal",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeImprovisedBowLimb",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = value4,
			Stances = GetToolMakingStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:shadeleafBowStave",
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
					EntityTypeToCreate = "item:improvisedBowLimb",
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
			ProcessToolSetKey = "toolSetShapenSmallWood",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeImprovisedBasicSpear",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = value4,
			Stances = GetToolMakingStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:waterCaneStem",
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
					EntityTypeToCreate = "item:improvisedBasicSpear",
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
			ProcessToolSetKey = "toolSetShapenSmallWood",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeKnifeSpear",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 2.2f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:waterCaneStem",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:advancedKnife",
					IsConsumed = false,
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
					EntityTypeToCreate = "item:advancedKnifeSpear",
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
			ProcessToolSetKey = "toolSetAttachWoodAndMetal",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeImprovisedGoodSpear",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 2.2f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:waterCaneStem",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:scrapMetal",
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
					EntityTypeToCreate = "item:improvisedGoodSpear",
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
			ProcessToolSetKey = "toolSetShapenSimpleSmallMetalAndtoolSetAttachWoodAndMetal",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeFarmingHoe",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 2.2f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:waterCaneStem",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:scrapMetal",
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
					EntityTypeToCreate = "item:farmingHoe",
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
			ProcessToolSetKey = "toolSetShapenSimpleSmallMetalAndtoolSetAttachWoodAndMetal",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeImprovisedFlintSpear",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 2.2f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:waterCaneStem",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:flintRough",
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
					EntityTypeToCreate = "item:improvisedFlintSpear",
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
			ProcessToolSetKey = "toolSetAttachWoodAndMetal",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "assembleImprovisedBow",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 2.2f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:improvisedBowLimb",
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
					EntityTypeToCreate = "item:improvisedBow",
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
			ProcessToolSetKey = "toolSetLightString",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeImprovisedArrowShaftBundle",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 2.2f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:shadeleafCanes",
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
					EntityTypeToCreate = "item:improvisedArrowShaftBundle",
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
			ProcessToolSetKey = "toolSetShapenSmallWoodImprovisedWithFire",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeImprovisedBasicArrow",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = value4,
			Stances = GetToolMakingStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:improvisedArrowShaftBundle",
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
					EntityTypeToCreate = "item:improvisedBasicArrow",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.00033333336f
			},
			ProcessToolSetKey = "toolSetShapenSmallWood",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeImprovisedChitinousArrow",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 2.2f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[3]
			{
				new Input
				{
					Entity = "item:improvisedArrowShaftBundle",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:waterCaneLeaves",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:twinklerPlating",
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
					EntityTypeToCreate = "item:improvisedChitinousArrow",
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
			ProcessToolSetKey = "toolSetAttachLightObjectsAndShapeSmallWood",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeImprovisedMetalArrow",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 2.2f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[3]
			{
				new Input
				{
					Entity = "item:improvisedArrowShaftBundle",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:waterCaneLeaves",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:scrapMetal",
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
					EntityTypeToCreate = "item:improvisedMetalArrow",
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
			ProcessToolSetKey = "toolSetShapenSimpleSmallMetalAndCombineLightImprovisedObjects",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeImprovisedMetalHooks",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = value4,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:scrapMetal",
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
					EntityTypeToCreate = "item:improvisedMetalHooks",
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
			ProcessToolSetKey = "toolSetShapenSimpleSmallMetal",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeWoodenHooks",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = value4,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:sticks",
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
					EntityTypeToCreate = "item:woodenHooks",
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
			ProcessToolSetKey = "toolSetShapenSmallWood",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeFieldLabPacked",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "mechanics",
			PhysicalWorkFactor = 2.2f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:labComponents",
					IsConsumed = false,
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
					EntityTypeToCreate = "item:fieldLabPacked",
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
			ProcessToolSetKey = "toolSetShapenSimpleSmallMetal",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeImprovedFireExtinguisher",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "mechanics",
			PhysicalWorkFactor = 2.2f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:basicFireExtinguisher",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:labComponents",
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
					EntityTypeToCreate = "item:improvedFireExtinguisher",
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
			ProcessToolSetKey = "toolSetShapenSimpleSmallMetal",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeSentryItem",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "mechanics",
			PhysicalWorkFactor = 2.2f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:sentryGun",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:sentryWeaponMount",
					IsConsumed = false,
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
					EntityTypeToCreate = "item:sentry",
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
			ProcessToolSetKey = "toolSetShapenSimpleSmallMetal",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeSprayGunSentry",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "mechanics",
			PhysicalWorkFactor = 2.2f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[3]
			{
				new Input
				{
					Entity = "item:basicFireExtinguisher",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:labComponents",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:sentryWeaponMount",
					IsConsumed = false,
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
					EntityTypeToCreate = "item:spraySentry",
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
			ProcessToolSetKey = "toolSetShapenSimpleSmallMetal",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeShotgunSentry",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "mechanics",
			PhysicalWorkFactor = 2.2f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[3]
			{
				new Input
				{
					Entity = "item:gunBarrelSmoothShort",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:scrapMetal",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:sentryWeaponMount",
					IsConsumed = false,
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
					EntityTypeToCreate = "item:shotgunSentry",
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
			ProcessToolSetKey = "toolSetShapenSimpleSmallMetal",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "cleanUrsinix",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = value,
			Stances = standingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:ursinix",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[2]
			{
				new Output
				{
					EntityTypeToCreate = "item:cleanedUrsinix",
					Amount = new OutputAmount
					{
						NoOfItems = 3
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:ursinixVenomGland",
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
			ProcessToolSetKey = "toolSetButcherFlesh",
			AgentActionState = AnimAction.Butchering,
			AgentAnimationStates = null
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeBushDragonCartridge",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 2.2f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:bushDragonPoisonGlands",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:emptyCartridge",
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
					EntityTypeToCreate = "item:bushDragonCartridge",
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
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeSpoakShingles",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = value4,
			Stances = GetToolMakingStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:spoakLeaves",
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
					EntityTypeToCreate = "item:spoakShingles",
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
			ProcessToolSetKey = "toolSetMakeSpoakShingles",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeWingweedMats",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 2.2f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:wingweedLeaves",
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
					EntityTypeToCreate = "item:wingweedMat",
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
			ProcessToolSetKey = "toolSetMakeSpoakShingles",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeTextile",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "weaving",
			PhysicalWorkFactor = 4f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:cotton",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 5
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:textile",
					Amount = new OutputAmount
					{
						NoOfItems = 5
					},
					ToolContainerTypesToPlaceIn = new string[1] { "item:textileWorkshopUpgrade" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 75f
			},
			ProcessToolSetKey = "toolSetMakeTextile"
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeCottonString",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "weaving",
			PhysicalWorkFactor = 4f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:cotton",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 3
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:cottonString",
					Amount = new OutputAmount
					{
						NoOfItems = 3
					},
					ToolContainerTypesToPlaceIn = new string[1] { "item:textileWorkshopUpgrade" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 75f
			},
			ProcessToolSetKey = "toolSetMakeTextile"
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeBedFrame",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "carpentry",
			PhysicalWorkFactor = 4f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:spoakBranchesTrimmed",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 2
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:bedFrame",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					},
					ToolContainerTypesToPlaceIn = new string[1] { "item:carpenterWorkshopUpgrade" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 75f
			},
			ProcessToolSetKey = "toolSetCarpentry"
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeFurniture",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "carpentry",
			PhysicalWorkFactor = 4f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:spoakBranchesTrimmed",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 3
					}
				},
				new Input
				{
					Entity = "item:thunderChickenTannedHide",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 2
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:furniture",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					},
					ToolContainerTypesToPlaceIn = new string[1] { "item:carpenterWorkshopUpgrade" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 75f
			},
			ProcessToolSetKey = "toolSetCarpentry"
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeLoomComponents",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "carpentry",
			PhysicalWorkFactor = 4f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:waterCaneStem",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 3
					}
				},
				new Input
				{
					Entity = "item:spoakBranchesTrimmed",
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
					EntityTypeToCreate = "item:loomComponents",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					},
					ToolContainerTypesToPlaceIn = new string[1] { "item:carpenterWorkshopUpgrade" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			ProcessToolSetKey = "toolSetCarpentry"
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeGaskets",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "chemistry",
			PhysicalWorkFactor = 4f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:marshcotSap",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 5
					}
				},
				new Input
				{
					Entity = "item:sulfurPowder",
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
					EntityTypeToCreate = "item:gaskets",
					Amount = new OutputAmount
					{
						NoOfItems = 5
					},
					ToolContainerTypesToPlaceIn = new string[1] { "item:polymerWorkshopUpgrade" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 75f
			},
			ProcessToolSetKey = "toolSetMakeRubber"
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeFishTrapBasket",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 2.2f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:shadeleafCanes",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 3
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:fishTrapBasket",
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
			ProcessToolSetKey = "toolSetCordage"
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeFishTrapHoopNet",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "fishing",
			PhysicalWorkFactor = 4f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:spoakBranchesTrimmed",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:fishingNet",
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
					EntityTypeToCreate = "item:fishTrapHoopNet",
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
			ProcessToolSetKey = "toolSetShapenSmallWoodImprovisedWithFire"
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeFishingNet",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "fishing",
			PhysicalWorkFactor = 2.2f,
			Stances = GetToolMakingStances(),
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:cottonString",
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
					EntityTypeToCreate = "item:fishingNet",
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
			ProcessToolSetKey = "toolSetMakeSpoakShingles"
		});
		float value5 = 1.8f;
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeMolecularKnife",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "menial",
			PhysicalWorkFactor = value5,
			Stances = standingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:acetylene",
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
					EntityTypeToCreate = "item:advancedKnife",
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
			ProcessToolSetKey = "toolSetUseAssemblerPlateA"
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeMolecularMachete",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "menial",
			PhysicalWorkFactor = value5,
			Stances = standingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:acetylene",
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
					EntityTypeToCreate = "item:advancedMachete",
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
			ProcessToolSetKey = "toolSetUseAssemblerPlateA"
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeMolecularCookingPot",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "menial",
			PhysicalWorkFactor = value5,
			Stances = standingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:acetylene",
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
					EntityTypeToCreate = "item:advancedCookingPot",
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
			ProcessToolSetKey = "toolSetUseAssemblerPlateA"
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeRifleAmmo",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "menial",
			PhysicalWorkFactor = value5,
			Stances = standingProduction,
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:acetylene",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:ironCanister",
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
					EntityTypeToCreate = "item:coilRifleAmmo",
					Amount = new OutputAmount
					{
						NoOfItems = 4
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 75f
			},
			ProcessToolSetKey = "toolSetUseAssemblerPlateB"
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeAssemblerPlateA",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "menial",
			PhysicalWorkFactor = value5,
			Stances = standingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:acetylene",
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
					EntityTypeToCreate = "item:assemblerPlateA",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			ProcessToolSetKey = "toolSetUseMasterAssemblerPlateA"
		});
		list.Add(new ProcessType
		{
			Name = "Producing",
			KeyName = "makeMasterAssemblerPlateA",
			JobTypeKey = "craftingJobType",
			RequiredSkill = "menial",
			PhysicalWorkFactor = value5,
			Stances = standingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:acetylene",
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
					EntityTypeToCreate = "item:masterAssemblerPlateA",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			ProcessToolSetKey = "toolSetUseMasterAssemblerPlateA"
		});
		list.Add(new ProcessType
		{
			Name = "Hunting",
			KeyName = "huntingBinalRat",
			RequiredSkill = "hunting",
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "entity:binalRat",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			IsPseudoProcess = true,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:binalRatCarcass",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.03f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Hunting",
			KeyName = "huntingTurnip",
			RequiredSkill = "hunting",
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "entity:turnip",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			IsPseudoProcess = true,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:turnipCarcass",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.03f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Hunting",
			KeyName = "huntingPygmyThunderChicken",
			RequiredSkill = "hunting",
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "entity:pygmyThunderChicken",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			IsPseudoProcess = true,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:thunderChickenCarcass",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.03f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Hunting",
			KeyName = "huntingWhiteThunderChicken",
			RequiredSkill = "hunting",
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "entity:whiteThunderChicken",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			IsPseudoProcess = true,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:thunderChickenCarcass",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.03f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Hunting",
			KeyName = "huntingBulkyThunderChicken",
			RequiredSkill = "hunting",
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "entity:studdedThunderChicken",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			IsPseudoProcess = true,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:thunderChickenCarcass",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.03f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Hunting",
			KeyName = "huntingThinThunderChicken",
			RequiredSkill = "hunting",
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "entity:bajingan",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			IsPseudoProcess = true,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:thunderChickenCarcass",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.03f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Hunting",
			KeyName = "huntingTwinkler",
			RequiredSkill = "hunting",
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "entity:twinkler",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			IsPseudoProcess = true,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:quaditeCarcass",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.03f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Hunting",
			KeyName = "huntingLeafcutter",
			RequiredSkill = "hunting",
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "entity:fieldQuadite",
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
					EntityTypeToCreate = "item:leafcutterCarcass",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.03f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Hunting",
			KeyName = "huntingBushDragon",
			RequiredSkill = "hunting",
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "entity:bushDragon",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			IsPseudoProcess = true,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:bushDragonCarcass",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.03f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Hunting",
			KeyName = "huntingPatrician",
			RequiredSkill = "hunting",
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "entity:patrician",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			IsPseudoProcess = true,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:patricianCarcass",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.03f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Hunting",
			KeyName = "huntingForestGuardian",
			RequiredSkill = "hunting",
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "entity:forestGuardian",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			IsPseudoProcess = true,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:forestGuardianCarcass",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.03f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Hunting",
			KeyName = "huntingWhipjaw",
			RequiredSkill = "hunting",
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "entity:whipjaw",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			IsPseudoProcess = true,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:whipjawCarcass",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.03f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Hunting",
			KeyName = "huntingLesserWhipjaw",
			RequiredSkill = "hunting",
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "entity:lesserWhipjaw",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			IsPseudoProcess = true,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:whipjawCarcass",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.03f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Hunting",
			KeyName = "huntingDemonTree",
			RequiredSkill = "hunting",
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "entity:spoakDendront",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			IsPseudoProcess = true,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:demontreeCarcass",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.03f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Hunting",
			KeyName = "huntingSwampDemonTree",
			RequiredSkill = "hunting",
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "entity:swampDendront",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			IsPseudoProcess = true,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:swampDemonTreeCarcass",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.03f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Hunting",
			KeyName = "huntingMegapod",
			RequiredSkill = "hunting",
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "entity:megapod",
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			IsPseudoProcess = true,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:megapodCarcass",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.03f
			}
		});
		list.Add(new ProcessType
		{
			Name = "Growing",
			KeyName = "growingGlassyCreeperPodsInFarmPlot",
			RequiredSkill = "farming",
			IsPseudoProcess = true,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:glassyCreeperPods",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			ProcessToolSetKey = "toolSetPseudoFarmplot",
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:glassyCreeperPods",
					Amount = new OutputAmount
					{
						NoOfItems = 5
					}
				}
			}
		});
		list.Add(new ProcessType
		{
			Name = "Growing",
			KeyName = "growingCrystalBerriesInFarmPlot",
			RequiredSkill = "farming",
			IsPseudoProcess = true,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:crystalBerries",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			ProcessToolSetKey = "toolSetPseudoFarmplot",
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:crystalBerries",
					Amount = new OutputAmount
					{
						NoOfItems = 5
					}
				}
			}
		});
		list.Add(new ProcessType
		{
			Name = "Growing",
			KeyName = "growingCottonInFarmPlot",
			RequiredSkill = "farming",
			IsPseudoProcess = true,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:cotton",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			ProcessToolSetKey = "toolSetPseudoFarmplot",
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:cotton",
					Amount = new OutputAmount
					{
						NoOfItems = 5
					}
				}
			}
		});
		list.Add(new ProcessType
		{
			Name = "Growing",
			KeyName = "growingGlassyCreeperPodsInGreenhouse",
			RequiredSkill = "farming",
			IsPseudoProcess = true,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:glassyCreeperPods",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			ProcessToolSetKey = "toolSetPseudoGreenhouse",
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:glassyCreeperPods",
					Amount = new OutputAmount
					{
						NoOfItems = 5
					}
				}
			}
		});
		list.Add(new ProcessType
		{
			Name = "Growing",
			KeyName = "growingCrystalBerriesInGreenhouse",
			RequiredSkill = "farming",
			IsPseudoProcess = true,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:crystalBerries",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			ProcessToolSetKey = "toolSetPseudoGreenhouse",
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:crystalBerries",
					Amount = new OutputAmount
					{
						NoOfItems = 5
					}
				}
			}
		});
		list.Add(new ProcessType
		{
			Name = "Growing",
			KeyName = "growingFingerFruitInGreenhouse",
			RequiredSkill = "farming",
			IsPseudoProcess = true,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:fingerFruit",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			ProcessToolSetKey = "toolSetPseudoGreenhouse",
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:fingerFruit",
					Amount = new OutputAmount
					{
						NoOfItems = 5
					}
				}
			}
		});
		list.Add(new ProcessType
		{
			Name = "Fishing",
			KeyName = "catchingCarbonTail",
			RequiredSkill = "fishing",
			IsPseudoProcess = true,
			ProcessToolSetKey = "toolSetCarbonTailTraps",
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:carbonTail",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			}
		});
		list.Add(new ProcessType
		{
			Name = "Fishing",
			KeyName = "catchingStreakFin",
			RequiredSkill = "fishing",
			IsPseudoProcess = true,
			ProcessToolSetKey = "toolSetStreakFinTraps",
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:streakFin",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			}
		});
		float value6 = 2.4f;
		list.Add(new ProcessType
		{
			Name = "Harvesting",
			KeyName = "harvestBlackpulp",
			RequiredSkill = "menial",
			JobTypeKey = "gatherFoodJobType",
			IsGathering = true,
			MoveOutputToWorkerWhenCompleted = true,
			PhysicalWorkFactor = value6,
			Stances = kneelingProduction,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:blackpulp",
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
			AgentActionState = AnimAction.Harvesting
		});
		list.Add(new ProcessType
		{
			Name = "Harvesting",
			KeyName = "harvestFavorbread",
			RequiredSkill = "menial",
			JobTypeKey = "gatherFoodJobType",
			IsGathering = true,
			MoveOutputToWorkerWhenCompleted = true,
			PhysicalWorkFactor = value6,
			Stances = kneelingProduction,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:favorbread",
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
			AgentActionState = AnimAction.Harvesting
		});
		CreateHarvestSpoakBranches(list);
		list.Add(new ProcessType
		{
			Name = "Harvesting",
			KeyName = "harvestWaterCaneLeaves",
			RequiredSkill = "menial",
			IsGathering = true,
			JobTypeKey = "gatherMaterialsJobType",
			MoveOutputToWorkerWhenCompleted = true,
			PhysicalWorkFactor = value6,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "tree:riveraxle",
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
					EntityTypeToCreate = "item:waterCaneLeaves",
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
			ProcessToolSetKey = "toolSetHarvestSmallBranches",
			AgentActionState = AnimAction.Harvesting,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Low }
		});
		list.Add(new ProcessType
		{
			Name = "Harvesting",
			KeyName = "harvestWaterCaneStem",
			RequiredSkill = "menial",
			IsGathering = true,
			JobTypeKey = "gatherMaterialsJobType",
			MoveOutputToWorkerWhenCompleted = false,
			PhysicalWorkFactor = 6f,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "tree:riveraxle",
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
					EntityTypeToCreate = "item:waterCaneStem",
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
			ProcessToolSetKey = "toolSetChopWeakWood",
			AgentActionState = AnimAction.Harvesting,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Low }
		});
		list.Add(new ProcessType
		{
			Name = "Gathering",
			KeyName = "harvestWaterCaneSeeds",
			RequiredSkill = "menial",
			IsGathering = true,
			JobTypeKey = "gatherFoodJobType",
			MoveOutputToWorkerWhenCompleted = true,
			PhysicalWorkFactor = value6,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "tree:riveraxle",
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
					EntityTypeToCreate = "item:waterCaneSeeds",
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
			AgentActionState = AnimAction.Harvesting
		});
		list.Add(new ProcessType
		{
			Name = "Gathering",
			KeyName = "harvestShadeleafBowStave",
			RequiredSkill = "menial",
			IsGathering = true,
			JobTypeKey = "gatherMaterialsJobType",
			MoveOutputToWorkerWhenCompleted = true,
			PhysicalWorkFactor = value6,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "tree:shadeleaf",
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
					EntityTypeToCreate = "item:shadeleafBowStave",
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
			ProcessToolSetKey = "toolSetChopWeakWood",
			AgentActionState = AnimAction.Harvesting,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Low }
		});
		list.Add(new ProcessType
		{
			Name = "Harvesting",
			KeyName = "harvestShadeleafCanes",
			RequiredSkill = "menial",
			MoveOutputToWorkerWhenCompleted = false,
			IsGathering = true,
			JobTypeKey = "gatherMaterialsJobType",
			PhysicalWorkFactor = 6f,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "tree:shadeleaf",
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
					EntityTypeToCreate = "item:shadeleafCanes",
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
			ProcessToolSetKey = "toolSetChopWeakWood",
			AgentActionState = AnimAction.Harvesting,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Low }
		});
		list.Add(new ProcessType
		{
			Name = "Harvesting",
			KeyName = "harvestShadeleafResin",
			RequiredSkill = "menial",
			MoveOutputToWorkerWhenCompleted = true,
			IsGathering = true,
			JobTypeKey = "gatherMaterialsJobType",
			PhysicalWorkFactor = 2f,
			Stances = kneelingOrStandingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "tree:shadeleaf",
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
					EntityTypeToCreate = "item:shadeleafResin",
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
			AgentActionState = AnimAction.Harvesting,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Low }
		});
		list.Add(new ProcessType
		{
			Name = "Gathering",
			KeyName = "harvestGiantHollowBud",
			RequiredSkill = "menial",
			IsGathering = true,
			JobTypeKey = "gatherMaterialsJobType",
			MoveOutputToWorkerWhenCompleted = true,
			PhysicalWorkFactor = value6,
			Stances = kneelingOrStandingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "tree:gianthollow",
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
					EntityTypeToCreate = "item:giantHollowBud",
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
			AgentActionState = AnimAction.Harvesting,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Low }
		});
		list.Add(new ProcessType
		{
			Name = "Gathering",
			KeyName = "harvestWingweedLeaves",
			RequiredSkill = "menial",
			IsGathering = true,
			JobTypeKey = "gatherMaterialsJobType",
			MoveOutputToWorkerWhenCompleted = true,
			PhysicalWorkFactor = 4f,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "tree:wingweed",
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
					EntityTypeToCreate = "item:wingweedLeaves",
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
			ProcessToolSetKey = "toolSetHarvestSmallBranches",
			AgentActionState = AnimAction.Harvesting,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Low }
		});
		list.Add(new ProcessType
		{
			Name = "Harvesting",
			KeyName = "harvestDaysheenLeaves",
			RequiredSkill = "menial",
			IsGathering = true,
			JobTypeKey = "gatherMaterialsJobType",
			MoveOutputToWorkerWhenCompleted = false,
			PhysicalWorkFactor = 4f,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "tree:daysheen",
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
					EntityTypeToCreate = "item:daysheenLeaves",
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
			ProcessToolSetKey = "toolSetChopWeakWood",
			AgentActionState = AnimAction.Harvesting,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Low }
		});
		list.Add(new ProcessType
		{
			Name = "Gathering",
			KeyName = "harvestVines",
			RequiredSkill = "menial",
			IsGathering = true,
			JobTypeKey = "gatherMaterialsJobType",
			MoveOutputToWorkerWhenCompleted = true,
			PhysicalWorkFactor = 4f,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:vine",
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
			ProcessToolSetKey = "toolSetChopWeakWood",
			AgentActionState = AnimAction.Harvesting,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Low }
		});
		list.Add(new ProcessType
		{
			Name = "Harvesting",
			KeyName = "harvestMarshcotSapMarshcotFlower",
			RequiredSkill = "bushcraft",
			IsGathering = true,
			WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart,
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "tree:marshcotflower",
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
					EntityTypeToCreate = "item:marshcotSap",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					},
					ToolContainerTagsToPlaceIn = new string[1] { "liquidContainerNoHeat" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			ProcessToolSetKey = "toolSetHarvestSap",
			AgentActionState = AnimAction.Harvesting
		});
		list.Add(new ProcessType
		{
			Name = "Harvesting",
			KeyName = "harvestMarshcotSapMarshcotLeaf",
			RequiredSkill = "bushcraft",
			IsGathering = true,
			WorkNeeded = WorkerNeededOptions.WorkerOnlyNeededToStart,
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "tree:marshcotflower",
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
					EntityTypeToCreate = "item:marshcotSap",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					},
					ToolContainerTagsToPlaceIn = new string[1] { "liquidContainerNoHeat" }
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			ProcessToolSetKey = "toolSetHarvestSap",
			AgentActionState = AnimAction.Harvesting
		});
		list.Add(new ProcessType
		{
			Name = "Gathering",
			KeyName = "gatherUrsinix",
			PhysicalWorkFactor = 2.4f,
			RequiredSkill = "bushcraft",
			IsGathering = true,
			JobTypeKey = "gatherFoodJobType",
			MoveOutputToWorkerWhenCompleted = true,
			Stances = kneelingProduction,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:ursinix",
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
			ProcessToolSetKey = "toolSetFishAlabasterRay",
			AgentActionState = AnimAction.Fishing
		});
		list.Add(new ProcessType
		{
			Name = "Catching",
			KeyName = "catchNeonHornets",
			PhysicalWorkFactor = 2.4f,
			RequiredSkill = "bushcraft",
			IsGathering = true,
			JobTypeKey = "gatherMaterialsJobType",
			MoveOutputToWorkerWhenCompleted = true,
			Stances = kneelingOrStandingProduction,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:neonHornetsLive",
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
			ProcessToolSetKey = "toolSetStrongBugNet",
			AgentActionState = AnimAction.Harvesting
		});
		list.Add(new ProcessType
		{
			Name = "Catching",
			KeyName = "catchPigFlies",
			PhysicalWorkFactor = 2.4f,
			RequiredSkill = "bushcraft",
			IsGathering = true,
			JobTypeKey = "gatherMaterialsJobType",
			MoveOutputToWorkerWhenCompleted = true,
			Stances = kneelingOrStandingProduction,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:pigFliesLive",
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
			ProcessToolSetKey = "toolSetStrongBugNet",
			AgentActionState = AnimAction.Harvesting
		});
		list.Add(new ProcessType
		{
			Name = "Catching",
			KeyName = "catchPhantomWeaver",
			PhysicalWorkFactor = 2.4f,
			RequiredSkill = "bushcraft",
			IsGathering = true,
			JobTypeKey = "gatherFoodJobType",
			MoveOutputToWorkerWhenCompleted = true,
			Stances = kneelingOrStandingProduction,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:phantomWeaver",
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
			ProcessToolSetKey = "toolSetStrongBugNet",
			AgentActionState = AnimAction.Harvesting,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Low }
		});
		list.Add(new ProcessType
		{
			Name = "Catching",
			KeyName = "catchWebWing",
			PhysicalWorkFactor = 2.4f,
			RequiredSkill = "bushcraft",
			IsGathering = true,
			JobTypeKey = "gatherFoodJobType",
			MoveOutputToWorkerWhenCompleted = true,
			Stances = kneelingOrStandingProduction,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:webWing",
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
			ProcessToolSetKey = "toolSetStrongBugNet",
			AgentActionState = AnimAction.Harvesting
		});
		list.Add(new ProcessType
		{
			Name = "Fishing",
			KeyName = "fishMinnows",
			RequiredSkill = "fishing",
			IsGathering = true,
			JobTypeKey = "gatherFoodJobType",
			MoveOutputToWorkerWhenCompleted = true,
			PhysicalWorkFactor = 2.4f,
			Stances = kneelingProduction,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:minnowsLive",
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
			ProcessToolSetKey = "toolSetFishMinnows",
			AgentActionState = AnimAction.Harvesting,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Low }
		});
		list.Add(new ProcessType
		{
			Name = "Fishing",
			KeyName = "fishAlabasterRay",
			RequiredSkill = "fishing",
			IsGathering = true,
			JobTypeKey = "gatherFoodJobType",
			MoveOutputToWorkerWhenCompleted = true,
			PhysicalWorkFactor = 2.4f,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:alabasterRay",
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
			ProcessToolSetKey = "toolSetFishAlabasterRay",
			AgentActionState = AnimAction.Fishing
		});
		list.Add(new ProcessType
		{
			Name = "Fishing",
			KeyName = "fishDaggermouth",
			RequiredSkill = "fishing",
			IsGathering = true,
			JobTypeKey = "gatherFoodJobType",
			MoveOutputToWorkerWhenCompleted = true,
			PhysicalWorkFactor = 2.4f,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:daggermouth",
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
			AgentActionState = AnimAction.Fishing,
			ProcessToolSetKey = "toolSetFishAlabasterRay"
		});
		list.Add(new ProcessType
		{
			Name = "Fishing",
			KeyName = "fishStreakFin",
			RequiredSkill = "fishing",
			IsGathering = true,
			JobTypeKey = "gatherFoodJobType",
			MoveOutputToWorkerWhenCompleted = true,
			PhysicalWorkFactor = 2.4f,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:streakFin",
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
			ProcessToolSetKey = "toolSetFishStreakFin",
			AgentActionState = AnimAction.Fishing
		});
		list.Add(new ProcessType
		{
			Name = "Fishing",
			KeyName = "fishCarbonTail",
			RequiredSkill = "fishing",
			IsGathering = true,
			JobTypeKey = "gatherFoodJobType",
			MoveOutputToWorkerWhenCompleted = true,
			PhysicalWorkFactor = 2.4f,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:carbonTail",
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
			ProcessToolSetKey = "toolSetFishCarbonTail",
			AgentActionState = AnimAction.Fishing
		});
		list.Add(new ProcessType
		{
			Name = "Gathering",
			KeyName = "gatherFirewood",
			RequiredSkill = "grasping",
			IsGathering = true,
			JobTypeKey = "gatherFuelJobType",
			MoveOutputToWorkerWhenCompleted = true,
			PhysicalWorkFactor = 3f,
			Stances = kneelingProduction,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:firewood",
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
			AgentActionState = AnimAction.Harvesting
		});
		list.Add(new ProcessType
		{
			Name = "Harvesting",
			KeyName = "gatherHexapineLeaves",
			RequiredSkill = "grasping",
			IsGathering = true,
			JobTypeKey = "gatherFoodJobType",
			MoveOutputToWorkerWhenCompleted = true,
			PhysicalWorkFactor = 3f,
			Stances = kneelingProduction,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:hexapineLeaves",
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
			AgentActionState = AnimAction.Harvesting
		});
		list.Add(new ProcessType
		{
			Name = "Gathering",
			KeyName = "gatherSticks",
			RequiredSkill = "grasping",
			IsGathering = true,
			JobTypeKey = "gatherMaterialsJobType",
			MoveOutputToWorkerWhenCompleted = true,
			PhysicalWorkFactor = 3f,
			Stances = kneelingProduction,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:sticks",
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
			AgentActionState = AnimAction.Harvesting
		});
		list.Add(new ProcessType
		{
			Name = "Extracting",
			KeyName = "gatherGoldOre",
			RequiredSkill = "menial",
			IsGathering = true,
			JobTypeKey = "gatherMaterialsJobType",
			PhysicalWorkFactor = 6f,
			Stances = kneelingOrStandingProduction,
			MoveOutputToWorkerWhenCompleted = false,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:goldOre",
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
			AgentActionState = AnimAction.Digging,
			ProcessToolSetKey = "toolSetDiggingSoil"
		});
		list.Add(new ProcessType
		{
			Name = "Extracting",
			KeyName = "gatherBogOre",
			RequiredSkill = "menial",
			IsGathering = true,
			JobTypeKey = "gatherMaterialsJobType",
			MoveOutputToWorkerWhenCompleted = false,
			PhysicalWorkFactor = 6f,
			Stances = kneelingOrStandingProduction,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:bogOre",
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
			AgentActionState = AnimAction.Digging,
			ProcessToolSetKey = "toolSetDiggingSoil"
		});
		list.Add(new ProcessType
		{
			Name = "Extracting",
			KeyName = "gatherPodlac",
			RequiredSkill = "menial",
			IsGathering = true,
			JobTypeKey = "gatherMaterialsJobType",
			PhysicalWorkFactor = 6f,
			Stances = kneelingOrStandingProduction,
			MoveOutputToWorkerWhenCompleted = false,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:podlacUnrefined",
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
			AgentActionState = AnimAction.Digging,
			ProcessToolSetKey = "toolSetDiggingSoil"
		});
		list.Add(new ProcessType
		{
			Name = "Gathering",
			KeyName = "gatherGuano",
			RequiredSkill = "menial",
			IsGathering = true,
			JobTypeKey = "gatherMaterialsJobType",
			MoveOutputToWorkerWhenCompleted = false,
			PhysicalWorkFactor = 6f,
			Stances = kneelingOrStandingProduction,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:guano",
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
			AgentActionState = AnimAction.Digging,
			ProcessToolSetKey = "toolSetDiggingSoil"
		});
		list.Add(new ProcessType
		{
			Name = "Gathering",
			KeyName = "gatherClay",
			RequiredSkill = "menial",
			IsGathering = true,
			JobTypeKey = "gatherMaterialsJobType",
			MoveOutputToWorkerWhenCompleted = false,
			PhysicalWorkFactor = 6f,
			Stances = kneelingOrStandingProduction,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:clay",
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
			AgentActionState = AnimAction.Digging,
			ProcessToolSetKey = "toolSetDiggingClay"
		});
		list.Add(new ProcessType
		{
			Name = "Gathering",
			KeyName = "gatherSalt",
			RequiredSkill = "menial",
			IsGathering = true,
			JobTypeKey = "gatherMaterialsJobType",
			MoveOutputToWorkerWhenCompleted = false,
			PhysicalWorkFactor = 6f,
			Stances = kneelingOrStandingProduction,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:salt",
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
			AgentActionState = AnimAction.Digging,
			ProcessToolSetKey = "toolSetDiggingSalt"
		});
		list.Add(new ProcessType
		{
			Name = "Gathering",
			KeyName = "gatherFiregrassSod",
			RequiredSkill = "menial",
			IsGathering = true,
			JobTypeKey = "gatherMaterialsJobType",
			MoveOutputToWorkerWhenCompleted = false,
			PhysicalWorkFactor = 6f,
			Stances = kneelingOrStandingProduction,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:firegrassSod",
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
			AgentActionState = AnimAction.Digging,
			ProcessToolSetKey = "toolSetDiggingSoil"
		});
		list.Add(new ProcessType
		{
			Name = "Gathering",
			KeyName = "gatherSulfurBlocks",
			RequiredSkill = "grasping",
			IsGathering = true,
			JobTypeKey = "gatherMaterialsJobType",
			MoveOutputToWorkerWhenCompleted = false,
			PhysicalWorkFactor = 6f,
			Stances = kneelingOrStandingProduction,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:sulfurBlocks",
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
			AgentActionState = AnimAction.Digging,
			ProcessToolSetKey = "toolSetDiggingSoil"
		});
		list.Add(new ProcessType
		{
			Name = "Gathering",
			KeyName = "gatherStones",
			RequiredSkill = "grasping",
			IsGathering = true,
			JobTypeKey = "gatherMaterialsJobType",
			MoveOutputToWorkerWhenCompleted = true,
			PhysicalWorkFactor = 6f,
			Stances = kneelingProduction,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:stones",
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
			AgentActionState = AnimAction.Harvesting
		});
		list.Add(new ProcessType
		{
			Name = "Gathering",
			KeyName = "gatherFlint",
			RequiredSkill = "grasping",
			IsGathering = true,
			JobTypeKey = "gatherMaterialsJobType",
			MoveOutputToWorkerWhenCompleted = true,
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:flintRough",
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
			AgentActionState = AnimAction.Harvesting
		});
		list.Add(new ProcessType
		{
			Name = "Gathering",
			KeyName = "gatherSmoothSandstone",
			RequiredSkill = "grasping",
			IsGathering = true,
			JobTypeKey = "gatherMaterialsJobType",
			MoveOutputToWorkerWhenCompleted = true,
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:smoothSandstone",
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
			AgentActionState = AnimAction.Harvesting
		});
		list.Add(new ProcessType
		{
			Name = "Gathering",
			KeyName = "gatherTorux",
			RequiredSkill = "grasping",
			IsGathering = true,
			JobTypeKey = "gatherFoodJobType",
			MoveOutputToWorkerWhenCompleted = true,
			PhysicalWorkFactor = 3f,
			Stances = kneelingProduction,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:torux",
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
			AgentActionState = AnimAction.Harvesting
		});
		list.Add(new ProcessType
		{
			Name = "Gathering",
			KeyName = "gatherClamwich",
			RequiredSkill = "grasping",
			IsGathering = true,
			JobTypeKey = "gatherFoodJobType",
			MoveOutputToWorkerWhenCompleted = true,
			PhysicalWorkFactor = 3f,
			Stances = kneelingProduction,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:clamwich",
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
			AgentActionState = AnimAction.Harvesting
		});
		list.Add(new ProcessType
		{
			Name = "Catching",
			KeyName = "gatherCrestedFoiler",
			RequiredSkill = "bushcraft",
			IsGathering = true,
			JobTypeKey = "gatherFoodJobType",
			MoveOutputToWorkerWhenCompleted = true,
			PhysicalWorkFactor = 3f,
			Stances = kneelingOrStandingProduction,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:crestedFoiler",
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
			ProcessToolSetKey = "toolSetChopWeakWood",
			AgentActionState = AnimAction.Harvesting
		});
		list.Add(new ProcessType
		{
			Name = "Catching",
			KeyName = "gatherGoldenCenobite",
			RequiredSkill = "bushcraft",
			IsGathering = true,
			JobTypeKey = "gatherFoodJobType",
			MoveOutputToWorkerWhenCompleted = true,
			PhysicalWorkFactor = 3f,
			Stances = kneelingProduction,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:goldenCenobite",
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
			AgentActionState = AnimAction.Harvesting,
			ProcessToolSetKey = "toolSetChopWeakWood"
		});
		list.Add(new ProcessType
		{
			Name = "Catching",
			KeyName = "gatherTreeScuttler",
			RequiredSkill = "bushcraft",
			IsGathering = true,
			JobTypeKey = "gatherFoodJobType",
			MoveOutputToWorkerWhenCompleted = true,
			PhysicalWorkFactor = 3f,
			Stances = kneelingOrStandingProduction,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:treeScuttler",
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
			AgentActionState = AnimAction.Harvesting
		});
		list.Add(new ProcessType
		{
			Name = "Catching",
			KeyName = "gatherMuckGrinder",
			RequiredSkill = "bushcraft",
			IsGathering = true,
			JobTypeKey = "gatherFoodJobType",
			MoveOutputToWorkerWhenCompleted = true,
			PhysicalWorkFactor = 3f,
			Stances = kneelingProduction,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:muckGrinder",
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
			AgentActionState = AnimAction.Harvesting
		});
		list.Add(new ProcessType
		{
			Name = "Gathering",
			KeyName = "gatherSpriteSlug",
			RequiredSkill = "bushcraft",
			IsGathering = true,
			JobTypeKey = "gatherFoodJobType",
			MoveOutputToWorkerWhenCompleted = true,
			PhysicalWorkFactor = 3f,
			Stances = kneelingOrStandingProduction,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:spriteSlug",
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
			AgentActionState = AnimAction.Harvesting,
			ProcessToolSetKey = "toolSetChopWeakWood"
		});
		list.Add(new ProcessType
		{
			Name = "Catching",
			KeyName = "gatherCrazyDweller",
			RequiredSkill = "bushcraft",
			IsGathering = true,
			JobTypeKey = "gatherFoodJobType",
			MoveOutputToWorkerWhenCompleted = true,
			PhysicalWorkFactor = 3f,
			Stances = kneelingOrStandingProduction,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:crazyDweller",
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
			AgentActionState = AnimAction.Harvesting,
			ProcessToolSetKey = "toolSetChopWeakWood"
		});
		list.Add(new ProcessType
		{
			Name = "Gathering",
			KeyName = "gatherScampBeetle",
			RequiredSkill = "bushcraft",
			IsGathering = true,
			JobTypeKey = "gatherFoodJobType",
			MoveOutputToWorkerWhenCompleted = true,
			PhysicalWorkFactor = 3f,
			Stances = kneelingProduction,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:scampBeetle",
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
			AgentActionState = AnimAction.Harvesting,
			ProcessToolSetKey = "toolSetChopWeakWood"
		});
		list.Add(new ProcessType
		{
			Name = "Gathering",
			KeyName = "gatherScampGrub",
			RequiredSkill = "bushcraft",
			IsGathering = true,
			JobTypeKey = "gatherFoodJobType",
			MoveOutputToWorkerWhenCompleted = true,
			PhysicalWorkFactor = 3f,
			Stances = kneelingProduction,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:scampGrub",
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
			AgentActionState = AnimAction.Harvesting
		});
		list.Add(new ProcessType
		{
			Name = "Catching",
			KeyName = "gatherImpEel",
			RequiredSkill = "fishing",
			IsGathering = true,
			JobTypeKey = "gatherFoodJobType",
			MoveOutputToWorkerWhenCompleted = true,
			PhysicalWorkFactor = 3f,
			Stances = kneelingProduction,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:impEel",
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
			ProcessToolSetKey = "toolSetChopWeakWood",
			AgentActionState = AnimAction.Harvesting
		});
		list.Add(new ProcessType
		{
			Name = "Gathering",
			KeyName = "gatherCommonOilTubers",
			RequiredSkill = "bushcraft",
			MoveOutputToWorkerWhenCompleted = true,
			IsGathering = true,
			JobTypeKey = "gatherFoodJobType",
			PhysicalWorkFactor = 3f,
			Stances = kneelingProduction,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:commonOilTubers",
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
			AgentActionState = AnimAction.Harvesting
		});
		list.Add(new ProcessType
		{
			Name = "Gathering",
			KeyName = "gatherSpottedOilTubers",
			RequiredSkill = "bushcraft",
			MoveOutputToWorkerWhenCompleted = true,
			IsGathering = true,
			JobTypeKey = "gatherFoodJobType",
			PhysicalWorkFactor = 3f,
			Stances = kneelingProduction,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:spottedOilTubers",
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
			AgentActionState = AnimAction.Harvesting
		});
		list.Add(new ProcessType
		{
			Name = "Gathering",
			KeyName = "gatherGlassyCreeperPods",
			RequiredSkill = "fruitPicking",
			MoveOutputToWorkerWhenCompleted = true,
			IsGathering = true,
			JobTypeKey = "gatherFoodJobType",
			PhysicalWorkFactor = 3f,
			Stances = kneelingProduction,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:glassyCreeperPods",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			ProcessToolSetKey = "toolSetChopWeakWood",
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			AgentActionState = AnimAction.Harvesting
		});
		list.Add(new ProcessType
		{
			Name = "Gathering",
			KeyName = "gatherCrystalBerries",
			RequiredSkill = "fruitPicking",
			JobTypeKey = "gatherFoodJobType",
			MoveOutputToWorkerWhenCompleted = true,
			IsGathering = true,
			PhysicalWorkFactor = 3f,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:crystalBerries",
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
			AgentActionState = AnimAction.Harvesting
		});
		list.Add(new ProcessType
		{
			Name = "Gathering",
			KeyName = "gatherFingerFruit",
			RequiredSkill = "fruitPicking",
			JobTypeKey = "gatherFoodJobType",
			MoveOutputToWorkerWhenCompleted = true,
			IsGathering = true,
			PhysicalWorkFactor = 3f,
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:fingerFruit",
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
			AgentActionState = AnimAction.Harvesting
		});
		list.Add(new ProcessType
		{
			Name = "Constructing",
			KeyName = "constructImprovisedGreenhouse",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "construction",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:improvisedGreenHouseCover",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 2
					}
				},
				new Input
				{
					Entity = "item:shadeleafCanes",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 3
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:improvisedGreenhouse",
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
			ProcessToolSetKey = "toolSetDiggingConstruction",
			AgentActionState = AnimAction.Building
		});
		list.Add(new ProcessType
		{
			Name = "Constructing",
			KeyName = "constructGreenhouse",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "construction",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:diamondGlass",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 5
					}
				},
				new Input
				{
					Entity = "item:shadeleafCanes",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 3
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:greenhouse",
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
			ProcessToolSetKey = "toolSetDiggingConstruction",
			AgentActionState = AnimAction.Building
		});
		list.Add(new ProcessType
		{
			Name = "Constructing",
			KeyName = "constructMeatDryingRack",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:sticks",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 3
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:meatDryingRack",
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
			AgentActionState = AnimAction.Building,
			ProcessToolSetKey = "toolSetCordage",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Constructing",
			KeyName = "constructHideRack",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:sticks",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 2
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:hideRack",
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
			AgentActionState = AnimAction.Building,
			ProcessToolSetKey = "toolSetCordage",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Constructing",
			KeyName = "constructRareMetalRefinery",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:metalRefineryEquipment",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				},
				new Input
				{
					Entity = "item:metalRefineryPart1",
					IsConsumed = false,
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
					EntityTypeToCreate = "structure:rareMetalRefinery",
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
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Constructing",
			KeyName = "constructFirewoodStack",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:sticks",
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
					EntityTypeToCreate = "structure:firewoodStack",
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
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Constructing",
			KeyName = "constructPeatStack",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:sticks",
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
					EntityTypeToCreate = "structure:peatStack",
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
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Constructing",
			KeyName = "constructCompostPit",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 6f,
			Stances = kneelingOrStandingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:stones",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 1
					}
				}
			},
			Outputs = new Output[2]
			{
				new Output
				{
					EntityTypeToCreate = "structure:compostPit",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:soil",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					},
					IsWasteProduct = true
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 75f
			},
			AgentActionState = AnimAction.Digging,
			ProcessToolSetKey = "toolSetDiggingSoil"
		});
		list.Add(new ProcessType
		{
			Name = "Constructing",
			KeyName = "constructCompostBin",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:sticks",
					IsConsumed = false,
					Amount = new InputAmount
					{
						NoOfItems = 2
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:compostBin",
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
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Constructing",
			KeyName = "constructDryingShed",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "construction",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[3]
			{
				new Input
				{
					Entity = "item:spoakShingles",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:solidMudBrick",
					Amount = new InputAmount
					{
						NoOfItems = 2
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:shadeleafCanes",
					Amount = new InputAmount
					{
						NoOfItems = 3
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:dryingShed",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			AgentActionState = AnimAction.Building,
			ProcessToolSetKey = "toolSetDiggingConstruction",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Constructing",
			KeyName = "constructToolshed",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "construction",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:solidMudBrick",
					Amount = new InputAmount
					{
						NoOfItems = 3
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:waterCaneStem",
					Amount = new InputAmount
					{
						NoOfItems = 2
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:toolshed",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			AgentActionState = AnimAction.Building,
			ProcessToolSetKey = "toolSetDiggingConstruction",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Constructing",
			KeyName = "constructClayGranary",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "construction",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:spoakShingles",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:solidMudBrick",
					Amount = new InputAmount
					{
						NoOfItems = 5
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:clayGranary",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			AgentActionState = AnimAction.Building,
			ProcessToolSetKey = "toolSetDiggingConstruction",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Constructing",
			KeyName = "constructCaneHut",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "construction",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:waterCaneStem",
					Amount = new InputAmount
					{
						NoOfItems = 7
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:solidMudBrick",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:caneHut",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			AgentActionState = AnimAction.Building,
			ProcessToolSetKey = "toolSetCordage",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Constructing",
			KeyName = "constructClayHut",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "construction",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[4]
			{
				new Input
				{
					Entity = "item:solidMudBrick",
					Amount = new InputAmount
					{
						NoOfItems = 5
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:spoakBranchesTrimmed",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:spoakShingles",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:stones",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:clayHut",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 30f
			},
			ProcessToolSetKey = "toolSetDiggingConstruction",
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = " Build turnip hut",
			KeyName = "constructTurnipHut",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "construction",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[3]
			{
				new Input
				{
					Entity = "item:turnipShell",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:sticks",
					Amount = new InputAmount
					{
						NoOfItems = 2
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:firegrassSod",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:turnipHut",
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
			ProcessToolSetKey = "toolSetCordage",
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Constructing",
			KeyName = "constructImprovisedSmithy",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "construction",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:solidMudBrick",
					Amount = new InputAmount
					{
						NoOfItems = 3
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:stones",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:improvisedSmithy",
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
			ProcessToolSetKey = "toolSetSmallDiggingConstruction",
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Constructing",
			KeyName = "constructSimpleSmithy",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "construction",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[3]
			{
				new Input
				{
					Entity = "item:solidMudBrick",
					Amount = new InputAmount
					{
						NoOfItems = 3
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:anvil",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:barClamps",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:simpleSmithy",
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
			ProcessToolSetKey = "toolSetSmallDiggingConstruction",
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Constructing",
			KeyName = "constructKiln",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "construction",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:clay",
					Amount = new InputAmount
					{
						NoOfItems = 4
					},
					IsConsumed = true
				},
				new Input
				{
					Entity = "item:stones",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:kiln",
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
			ProcessToolSetKey = "toolSetDiggingConstruction",
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Constructing",
			KeyName = "constructGoldFurnace",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "construction",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:firebricks",
					Amount = new InputAmount
					{
						NoOfItems = 3
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:clay",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = true
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:goldFurnace",
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
			ProcessToolSetKey = "toolSetDiggingConstruction",
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Constructing",
			KeyName = "constructKilnImprovisedSmall",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:stones",
					Amount = new InputAmount
					{
						NoOfItems = 3
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:kilnImprovisedSmall",
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
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Setting up",
			KeyName = "constructSpikeTrap",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:spikeTrap",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:spikeTrap",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.00033333336f
			},
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Constructing",
			KeyName = "constructDeadfallTrap",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:stones",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:deadfallTrap",
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
		list.Add(new ProcessType
		{
			Name = "Constructing",
			KeyName = "constructSpringSnare",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:shadeleafCanes",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:springSnare",
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
			ProcessToolSetKey = "toolSetCordage",
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Setting up",
			KeyName = "constructLandMine",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:landMine",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:landMine",
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
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Constructing",
			KeyName = "constructHelipad",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "construction",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:paint",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = true
				},
				new Input
				{
					Entity = "item:stones",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:helipad",
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
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Constructing",
			KeyName = "constructHelipadBig",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "construction",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:structurePanels",
					Amount = new InputAmount
					{
						NoOfItems = 2
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:helipadBig",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.011666667f
			},
			ProcessToolSetKey = "toolSetShapenSmallWood",
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Setting up",
			KeyName = "constructSatelliteGroundStation",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "construction",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:satelliteGroundStation",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:satelliteGroundStation",
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
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Electronic }
		});
		list.Add(new ProcessType
		{
			Name = "Setting up",
			KeyName = "constructSmallTent",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:smallTent",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:smallTent",
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
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Tarp }
		});
		list.Add(new ProcessType
		{
			Name = "Setting up",
			KeyName = "constructOctagonalTent",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:octagonalTent",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:octagonalTent",
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
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Metal }
		});
		list.Add(new ProcessType
		{
			Name = "Setting up",
			KeyName = "constructDomeTent",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:domeTent",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:domeTent",
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
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Metal }
		});
		list.Add(new ProcessType
		{
			Name = "Constructing",
			KeyName = "constructA-frameTarp",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[3]
			{
				new Input
				{
					Entity = "item:sticks",
					Amount = new InputAmount
					{
						NoOfItems = 2
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:wingweedLeaves",
					Amount = new InputAmount
					{
						NoOfItems = 2
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:thermalTarp",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:A-frameTarp",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 120f
			},
			ProcessToolSetKey = "toolSetCordage",
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[2]
			{
				AnimModifier.Improvised,
				AnimModifier.Tarp
			}
		});
		list.Add(new ProcessType
		{
			Name = "Constructing",
			KeyName = "constructA-frameSpoakLeaves",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:spoakBranchesTrimmed",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:spoakLeaves",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:A-frameSpoakLeaves",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 120f
			},
			ProcessToolSetKey = "toolSetCordage",
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Constructing",
			KeyName = "constructA-frameScraps",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[3]
			{
				new Input
				{
					Entity = "item:sticks",
					Amount = new InputAmount
					{
						NoOfItems = 4
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:seatCushions",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:panelScraps",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:A-frameScraps",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 120f
			},
			ProcessToolSetKey = "toolSetCordage",
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Constructing",
			KeyName = "constructDaysheenTipi",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:daysheenLeaves",
					Amount = new InputAmount
					{
						NoOfItems = 2
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:daysheenTipi",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.0075000003f
			},
			ProcessToolSetKey = "toolSetCordage",
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Constructing",
			KeyName = "constructLean-toTarp",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:sticks",
					Amount = new InputAmount
					{
						NoOfItems = 6
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:thermalTarp",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:lean-toTarp",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.010000001f
			},
			ProcessToolSetKey = "toolSetCordage",
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[2]
			{
				AnimModifier.Improvised,
				AnimModifier.Tarp
			}
		});
		list.Add(new ProcessType
		{
			Name = "Constructing",
			KeyName = "constructLean-toSpoakLeaves",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[3]
			{
				new Input
				{
					Entity = "item:sticks",
					Amount = new InputAmount
					{
						NoOfItems = 4
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:wingweedLeaves",
					Amount = new InputAmount
					{
						NoOfItems = 2
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:spoakLeaves",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:lean-toSpoakLeaves",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.010000001f
			},
			ProcessToolSetKey = "toolSetCordage",
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Constructing",
			KeyName = "constructLean-toScraps",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:sticks",
					Amount = new InputAmount
					{
						NoOfItems = 6
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:panelScraps",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:lean-toScraps",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.010000001f
			},
			ProcessToolSetKey = "toolSetCordage",
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Constructing",
			KeyName = "constructDomeShelterTarp",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:shadeleafCanes",
					Amount = new InputAmount
					{
						NoOfItems = 4
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:thermalTarp",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:domeShelterTarp",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.014f
			},
			ProcessToolSetKey = "toolSetCordage",
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[2]
			{
				AnimModifier.Improvised,
				AnimModifier.Tarp
			}
		});
		list.Add(new ProcessType
		{
			Name = "Constructing",
			KeyName = "constructDomeShelterSpoakShingles",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:shadeleafCanes",
					Amount = new InputAmount
					{
						NoOfItems = 4
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:spoakShingles",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:domeShelterSpoakShingles",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.011666667f
			},
			ProcessToolSetKey = "toolSetCordage",
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Constructing",
			KeyName = "constructRadioHutImprovised",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "construction",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[4]
			{
				new Input
				{
					Entity = "item:shadeleafCanes",
					Amount = new InputAmount
					{
						NoOfItems = 3
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:spoakLeaves",
					Amount = new InputAmount
					{
						NoOfItems = 2
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:radio",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:radioAntenna",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:radioHutImprovised",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.011666667f
			},
			ProcessToolSetKey = "toolSetCordage",
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Constructing",
			KeyName = "constructRadioHut",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "construction",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[4]
			{
				new Input
				{
					Entity = "item:shadeleafCanes",
					Amount = new InputAmount
					{
						NoOfItems = 3
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:spoakShingles",
					Amount = new InputAmount
					{
						NoOfItems = 2
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:radio",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:radioAntenna",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:radioHut",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.011666667f
			},
			ProcessToolSetKey = "toolSetCordage",
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Constructing",
			KeyName = "constructWigwamSpoakShingles",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[4]
			{
				new Input
				{
					Entity = "item:spoakBranchesTrimmed",
					Amount = new InputAmount
					{
						NoOfItems = 3
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:spoakShingles",
					Amount = new InputAmount
					{
						NoOfItems = 3
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:firegrassSod",
					Amount = new InputAmount
					{
						NoOfItems = 2
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:stones",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:wigwamSpoakShingles",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.023333333f
			},
			ProcessToolSetKey = "toolSetCordage",
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Constructing",
			KeyName = "constructCampfire",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			Stances = stances,
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
					Entity = "item:stones",
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
					EntityTypeToCreate = "structure:campfire",
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
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Setting up",
			KeyName = "constructFieldKitchen",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:fieldKitchenStove",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:fieldKitchenEquipment",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:fieldKitchen",
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
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Metal }
		});
		list.Add(new ProcessType
		{
			Name = "Setting up",
			KeyName = "constructFieldLab",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 2f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:fieldLabPacked",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:fieldLab",
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
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Metal }
		});
		list.Add(new ProcessType
		{
			Name = "Setting up",
			KeyName = "constructImprovisedKitchen",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[4]
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
					Entity = "item:panelScraps",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:sticks",
					Amount = new InputAmount
					{
						NoOfItems = 2
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:spoakShingles",
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
					EntityTypeToCreate = "structure:improvisedKitchen",
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
				DaysNeeded = 0.011666667f
			},
			ProcessToolSetKey = "toolSetCordage",
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Constructing",
			KeyName = "constructMudbrickKitchen",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[4]
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
					Entity = "item:clay",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = true
				},
				new Input
				{
					Entity = "item:sticks",
					Amount = new InputAmount
					{
						NoOfItems = 2
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:spoakShingles",
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
					EntityTypeToCreate = "structure:mudBrickKitchen",
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
				DaysNeeded = 0.011666667f
			},
			ProcessToolSetKey = "toolSetCordage",
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Constructing",
			KeyName = "constructImprovisedWorkbench",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:panelScraps",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:sticks",
					Amount = new InputAmount
					{
						NoOfItems = 3
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:improvisedWorkbench",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.011666667f
			},
			ProcessToolSetKey = "toolSetCordage",
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Constructing",
			KeyName = "constructMudbrickWorkbench",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:clay",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = true
				},
				new Input
				{
					Entity = "item:sticks",
					Amount = new InputAmount
					{
						NoOfItems = 3
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:mudBrickWorkbench",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.011666667f
			},
			ProcessToolSetKey = "toolSetCordage",
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Constructing",
			KeyName = "constructWorkshopBuilding",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "construction",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[3]
			{
				new Input
				{
					Entity = "item:solidMudBrick",
					Amount = new InputAmount
					{
						NoOfItems = 5
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:waterCaneStem",
					Amount = new InputAmount
					{
						NoOfItems = 3
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:stones",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:workshopBuilding",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.023333333f
			},
			ProcessToolSetKey = "toolSetCordage",
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Constructing",
			KeyName = "constructCookhouse",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "construction",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[6]
			{
				new Input
				{
					Entity = "item:solidMudBrick",
					Amount = new InputAmount
					{
						NoOfItems = 5
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:waterCaneStem",
					Amount = new InputAmount
					{
						NoOfItems = 3
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:spoakBranchesTrimmed",
					Amount = new InputAmount
					{
						NoOfItems = 2
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:spoakShingles",
					Amount = new InputAmount
					{
						NoOfItems = 2
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:textile",
					Amount = new InputAmount
					{
						NoOfItems = 3
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:stones",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:cookhouse",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.023333333f
			},
			ProcessToolSetKey = "toolSetCordage",
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Constructing",
			KeyName = "constructStill",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "construction",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[4]
			{
				new Input
				{
					Entity = "item:stillComponents",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:stones",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:clayJar",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:sticks",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:still",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.011666667f
			},
			ProcessToolSetKey = "toolSetCordage",
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Setting up",
			KeyName = "constructSensor",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 2f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:sensor",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:sensor",
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
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Electronic }
		});
		list.Add(new ProcessType
		{
			Name = "Setting up",
			KeyName = "constructSentry",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 2f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:sentry",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:sentry",
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
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Electronic }
		});
		list.Add(new ProcessType
		{
			Name = "Setting up",
			KeyName = "constructSprayGunSentry",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 2f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:spraySentry",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:sprayGunSentry",
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
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Electronic }
		});
		list.Add(new ProcessType
		{
			Name = "Setting up",
			KeyName = "constructShotgunSentry",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 2f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:shotgunSentry",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:shotgunSentry",
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
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Electronic }
		});
		list.Add(new ProcessType
		{
			Name = "Setting up",
			KeyName = "constructWeatherStation",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:weatherStationMast",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:weatherStationSensors",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:weatherStation",
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
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Electronic }
		});
		list.Add(new ProcessType
		{
			Name = "Setting up",
			KeyName = "constructMolecularAssembler",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "menial",
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			Inputs = new Input[3]
			{
				new Input
				{
					Entity = "item:vacuumChamber",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:assemblerCabinet",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:assemblerCooling",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:molecularAssembler",
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
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Metal }
		});
		list.Add(new ProcessType
		{
			Name = "Constructing",
			KeyName = "constructStorageHole",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 6f,
			Stances = stances,
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:stones",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:spoakLeaves",
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
					EntityTypeToCreate = "structure:storageHole",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:soil",
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
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Constructing",
			KeyName = "constructCooledFoodCache",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 6f,
			Stances = stances,
			Inputs = new Input[3]
			{
				new Input
				{
					Entity = "item:stones",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:spoakLeaves",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:inactivatedFoodCoolerUnit",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = true
				}
			},
			Outputs = new Output[2]
			{
				new Output
				{
					EntityTypeToCreate = "structure:cooledFoodCache",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				},
				new Output
				{
					EntityTypeToCreate = "item:soil",
					IsWasteProduct = true,
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.0073333336f
			},
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Constructing",
			KeyName = "constructSmokeOven",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[3]
			{
				new Input
				{
					Entity = "item:sticks",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:stones",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:firegrassSod",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:smokeOven",
					Amount = new OutputAmount
					{
						NoOfItems = 1
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 120f
			},
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Constructing",
			KeyName = "constructAbatis1",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[1]
			{
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
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:abatis",
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
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = "Constructing",
			KeyName = "constructScarecrow",
			JobTypeKey = "constructionJobType",
			RequiredSkill = "bushcraft",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			Inputs = new Input[2]
			{
				new Input
				{
					Entity = "item:twinklerPlating",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				},
				new Input
				{
					Entity = "item:twinklerPheromone",
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					IsConsumed = false
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "structure:scarecrow",
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
			AgentActionState = AnimAction.Building,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		string name12 = "Doing maintenance";
		list.Add(new ProcessType
		{
			Name = name12,
			SummaryDescription = string.Format("Repairing {0} to improve their condition.", "trimmed spoak branches"),
			KeyName = "reconditionSpokBranchesTrimmedPart",
			RequiredSkill = "construction",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.01f
			},
			RepairAction = RepairAction.PartsCondition,
			AgentActionState = AnimAction.Mending,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = name12,
			SummaryDescription = string.Format("Repairing {0} to improve their condition.", "spoak leaves"),
			KeyName = "reconditionSpoakLeaves",
			RequiredSkill = "construction",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.01f
			},
			RepairAction = RepairAction.PartsCondition,
			AgentActionState = AnimAction.Mending,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = name12,
			SummaryDescription = string.Format("Repairing {0} to improve their condition.", "daysheen leaves"),
			KeyName = "reconditionDaysheenLeaves",
			RequiredSkill = "construction",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.01f
			},
			RepairAction = RepairAction.PartsCondition,
			AgentActionState = AnimAction.Mending,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = name12,
			SummaryDescription = string.Format("Repairing {0} to improve their condition.", "sticks"),
			KeyName = "reconditionSticks",
			RequiredSkill = "construction",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.01f
			},
			RepairAction = RepairAction.PartsCondition,
			AgentActionState = AnimAction.Mending,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = name12,
			SummaryDescription = "Fixing the integrity of the structure so it does not fall apart",
			KeyName = "repairPrimitiveIntegrity",
			RequiredSkill = "construction",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 1f / 150f
			},
			RepairAction = RepairAction.Integrity,
			ProcessToolSetKey = "toolSetCordage",
			AgentActionState = AnimAction.Mending,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = name12,
			SummaryDescription = "Repairing a part to improve its condition.",
			KeyName = "repairPrimitiveCondition",
			RequiredSkill = "construction",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.01f
			},
			RepairAction = RepairAction.PartsCondition,
			AgentActionState = AnimAction.Mending,
			AgentAnimationStates = new AnimModifier[1] { AnimModifier.Improvised }
		});
		list.Add(new ProcessType
		{
			Name = name12,
			SummaryDescription = "Fixing the integrity of the structure so it does not fall apart",
			KeyName = "repairPrimitiveIntegrityWithDigging",
			RequiredSkill = "construction",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.009333333f
			},
			RepairAction = RepairAction.Integrity,
			ProcessToolSetKey = "toolSetDiggingConstruction",
			AgentActionState = AnimAction.Digging
		});
		list.Add(new ProcessType
		{
			Name = name12,
			SummaryDescription = "Fixing the integrity of the structure so it does not fall apart",
			KeyName = "repairPrimitiveIntegrityWithPlowing",
			RequiredSkill = "construction",
			PhysicalWorkFactor = 4f,
			Stances = stances,
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = 0.009333333f
			},
			RepairAction = RepairAction.Integrity,
			ProcessToolSetKey = "toolSetPlowingTools",
			AgentActionState = AnimAction.Digging
		});
		float value7 = 0.001f;
		list.Add(new ProcessType
		{
			Name = "Eating",
			KeyName = "extractThunderChickenMeat",
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:thunderChickenCarcass",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1,
						Substances = new string[1] { "meat" }
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:thunderChickenShred",
					Amount = new OutputAmount
					{
						Bulk = new Bulk
						{
							InputSubstance = "meat"
						}
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = value7
			}
		});
		list.Add(new ProcessType
		{
			Name = "Eating",
			KeyName = "extractBinalRatMeat",
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:binalRatCarcass",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1,
						Substances = new string[1] { "meat" }
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:binalRatShred",
					Amount = new OutputAmount
					{
						Bulk = new Bulk
						{
							InputSubstance = "meat"
						}
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = value7
			}
		});
		list.Add(new ProcessType
		{
			Name = "Eating",
			KeyName = "extractTwinklerMeat",
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:quaditeCarcass",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1,
						Substances = new string[1] { "meat" }
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:twinklerShred",
					Amount = new OutputAmount
					{
						Bulk = new Bulk
						{
							InputSubstance = "meat"
						}
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = value7
			}
		});
		list.Add(new ProcessType
		{
			Name = "Eating",
			KeyName = "extractDemonTreeMeat",
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:demontreeCarcass",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1,
						Substances = new string[1] { "meat" }
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:demonTreeShred",
					Amount = new OutputAmount
					{
						Bulk = new Bulk
						{
							InputSubstance = "meat"
						}
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = value7
			}
		});
		list.Add(new ProcessType
		{
			Name = "Eating",
			KeyName = "extractSwampDemonTreeMeat",
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:swampDemonTreeCarcass",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1,
						Substances = new string[1] { "meat" }
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:swampDemonTreeShred",
					Amount = new OutputAmount
					{
						Bulk = new Bulk
						{
							InputSubstance = "meat"
						}
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = value7
			}
		});
		list.Add(new ProcessType
		{
			Name = "Eating",
			KeyName = "extractPatricianMeat",
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:patricianCarcass",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1,
						Substances = new string[1] { "meat" }
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:patricianShred",
					Amount = new OutputAmount
					{
						Bulk = new Bulk
						{
							InputSubstance = "meat"
						}
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = value7
			}
		});
		list.Add(new ProcessType
		{
			Name = "Eating",
			KeyName = "extractBushDragonMeat",
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:bushDragonCarcass",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1,
						Substances = new string[1] { "meat" }
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:bushDragonShred",
					Amount = new OutputAmount
					{
						Bulk = new Bulk
						{
							InputSubstance = "meat"
						}
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = value7
			}
		});
		list.Add(new ProcessType
		{
			Name = "Eating",
			KeyName = "extractTurnipMeat",
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:turnipCarcass",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1,
						Substances = new string[1] { "meat" }
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:turnipShred",
					Amount = new OutputAmount
					{
						Bulk = new Bulk
						{
							InputSubstance = "meat"
						}
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = value7
			}
		});
		list.Add(new ProcessType
		{
			Name = "Eating",
			KeyName = "extractMegapodMeat",
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:megapodCarcass",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1,
						Substances = new string[1] { "meat" }
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:megapodShred",
					Amount = new OutputAmount
					{
						Bulk = new Bulk
						{
							InputSubstance = "meat"
						}
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = value7
			}
		});
		list.Add(new ProcessType
		{
			Name = "Eating",
			KeyName = "extractWhipjawMeat",
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:whipjawCarcass",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1,
						Substances = new string[1] { "meat" }
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:whipjawShred",
					Amount = new OutputAmount
					{
						Bulk = new Bulk
						{
							InputSubstance = "meat"
						}
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = value7
			}
		});
		list.Add(new ProcessType
		{
			Name = "Eating",
			KeyName = "extractSpikePlantMeat",
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:spikePlantCarcass",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1,
						Substances = new string[1] { "meat" }
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:spikePlantShred",
					Amount = new OutputAmount
					{
						Bulk = new Bulk
						{
							InputSubstance = "meat"
						}
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = value7
			}
		});
		list.Add(new ProcessType
		{
			Name = "Eating",
			KeyName = "extractForestGuardianMeat",
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:forestGuardianCarcass",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1,
						Substances = new string[1] { "meat" }
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:forestguardianShred",
					Amount = new OutputAmount
					{
						Bulk = new Bulk
						{
							InputSubstance = "meat"
						}
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = value7
			}
		});
		list.Add(new ProcessType
		{
			Name = "Eating",
			KeyName = "extractLeafCutterMeat",
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:leafcutterCarcass",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1,
						Substances = new string[1] { "meat" }
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:leafcutterShred",
					Amount = new OutputAmount
					{
						Bulk = new Bulk
						{
							InputSubstance = "meat"
						}
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = value7
			}
		});
		list.Add(new ProcessType
		{
			Name = "Eating",
			KeyName = "extractMudWormMeat",
			PhysicalWorkFactor = 4f,
			Stances = kneelingProduction,
			Inputs = new Input[1]
			{
				new Input
				{
					Entity = "item:mudWormCarcass",
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1,
						Substances = new string[1] { "meat" }
					}
				}
			},
			Outputs = new Output[1]
			{
				new Output
				{
					EntityTypeToCreate = "item:mudWormShred",
					Amount = new OutputAmount
					{
						Bulk = new Bulk
						{
							InputSubstance = "meat"
						}
					}
				}
			},
			WorkOrTimeNeeded = new WorkOrTime
			{
				DaysNeeded = value7
			}
		});
		return list;
	}

	public static ProcessType CreateHarvestSpoakBranches(List<ProcessType> listOfProcessTypes)
	{
		ProcessType processType = new ProcessType();
		processType.Name = "Harvesting";
		processType.KeyName = "harvestSpoakBranches";
		processType.RequiredSkill = "menial";
		processType.JobTypeKey = "gatherMaterialsJobType";
		processType.IsGathering = true;
		processType.MoveOutputToWorkerWhenCompleted = false;
		processType.PhysicalWorkFactor = 6f;
		processType.Stances = standingProduction;
		processType.Inputs = new Input[1]
		{
			new Input
			{
				Entity = "tree:spoak",
				Amount = new InputAmount
				{
					NoOfItems = 1
				}
			}
		};
		processType.Outputs = new Output[1]
		{
			new Output
			{
				EntityTypeToCreate = "item:spoakBranches",
				Amount = new OutputAmount
				{
					NoOfItems = 1
				}
			}
		};
		processType.WorkOrTimeNeeded = new WorkOrTime
		{
			DaysNeeded = 0.0033333334f
		};
		processType.ProcessToolSetKey = "toolSetChopToughWood";
		processType.AgentActionState = AnimAction.Harvesting;
		processType.AgentAnimationStates = new AnimModifier[1] { AnimModifier.Low };
		ProcessType processType2 = processType;
		listOfProcessTypes.Add(processType2);
		return processType2;
	}
}
