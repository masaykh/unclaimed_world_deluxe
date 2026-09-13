using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.HelpTopics;
using UWGame.ClientSide.Interface;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Combat;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.InGameEvents;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.Processes;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_3.Data;

public class Scenario3DataLoader : DataLoader
{
	public Scenario3DataLoader()
		: base(Config.DataType.RGScenario, 0.1f)
	{
	}

	protected override List<EventActionType> InitEventActionTypes()
	{
		return EventActionLoader.Init();
	}

	protected override List<PolledEventType> InitGlobalConditionalEvents()
	{
		return PolledEventsLoader.Init();
	}

	protected override List<AgentActionHook> InitAgentActionHooks()
	{
		return EventHooksLoader.InitAgentActionHooks();
	}

	protected override List<AttackTypeActionHook> InitAttackTypeEventHooks()
	{
		return EventHooksLoader.InitAttackTypeHooks();
	}

	protected override List<ProcessTypeActionHook> InitProcessTypeEventHooks()
	{
		return EventHooksLoader.InitProcessTypeHooks();
	}

	protected override List<DetectEntityTypeHook> InitDetectEntityTypeHooks()
	{
		return DetectionEventHooksLoader.InitDetectEntityTypeHooks();
	}

	protected override List<DetectResourceTypeHook> InitDetectResourceTypeHooks()
	{
		return DetectionEventHooksLoader.InitDetectResourceTypeHooks();
	}

	protected override GUIConstants InitGUIConstants()
	{
		return new GUIConstants
		{
			EnableFilters = false,
			EnableStandingOrders = false
		};
	}

	protected override List<ActionSets> InitActionSets()
	{
		return ActionSetsLoader.Init();
	}

	protected override List<EntityType> InitEntityTypes()
	{
		List<EntityType> list = new List<EntityType>();
		StructureLoader.Init(list);
		ItemsLoader.Init(list);
		TerrainFeatureLoader.Init(list);
		CreatureLoader.Init(list);
		return list;
	}

	protected override List<HelpTopic> InitTutorialTopics()
	{
		return TutorialLoader.Init();
	}

	protected override List<ProcessType> InitProcessTypes()
	{
		return ProcessLoader.Init();
	}

	protected override List<EntityTypePolledEvent> InitEntityPolledEvents()
	{
		return EntityPolledEventsLoader.Init();
	}

	protected override List<EntityTypeDescription> InitEntityTypeDescriptions()
	{
		return EntityTypeDescriptionLoader.Init();
	}

	protected override List<BodyLayerType> InitBodyLayerTypes()
	{
		return new List<BodyLayerType>
		{
			new BodyLayerType
			{
				KeyName = "clothesLayerBuffed",
				Name = "Survival suit",
				DamageReductionConstant = new Dictionary<string, float>
				{
					{ "bite", 4f },
					{ "sharp", 4f },
					{ "blunt", 2f },
					{ "fire", 3f },
					{ "piercing", 5f },
					{ "smallAnimalGrapple", 5f },
					{ "antiTwinkler", 5f }
				},
				DamageReductionFactor = new Dictionary<string, float>
				{
					{ "bite", 0.2f },
					{ "sharp", 0.2f },
					{ "blunt", 0.2f },
					{ "fire", 0.3f },
					{ "piercing", 0.3f },
					{ "smallAnimalGrapple", 0.3f },
					{ "antiTwinkler", 0.3f }
				}
			}
		};
	}

	protected override List<BodyType> InitBodyTypes()
	{
		List<BodyType> list = new List<BodyType>();
		string text = "humanoid";
		list.Add(new BodyType(text)
		{
			BodyPartTypes = new BodyPartType[1]
			{
				new BiologicalBodyPartType
				{
					BodyKeyName = text,
					Name = "Torso",
					ArmorLayer = "clothesLayerBuffed",
					OrganTypes = new OrganType[1]
					{
						new OrganType
						{
							Name = "Heart",
							IsVital = true
						}
					},
					ToHitProfileBack = 0.35f,
					ToHitProfileFront = 0.35f,
					ToHitProfileLeft = 0.1f,
					ToHitProfileRight = 0.1f,
					HitpointsFraction = 0.5f,
					BodyPartTypes = new BodyPartType[5]
					{
						new BiologicalBodyPartType
						{
							Name = "Head",
							BodyKeyName = text,
							OrganTypes = new OrganType[1]
							{
								new OrganType
								{
									Name = "Brain",
									IsVital = true
								}
							},
							ToHitProfileBack = 0.1f,
							ToHitProfileFront = 0.1f,
							ToHitProfileLeft = 0.1f,
							ToHitProfileRight = 0.1f,
							HitpointsFraction = 0.2f
						},
						new BiologicalBodyPartType
						{
							Name = "Left arm",
							BodyKeyName = text,
							ArmorLayer = "clothesLayerBuffed",
							ToHitProfileBack = 0.15f,
							ToHitProfileFront = 0.15f,
							ToHitProfileLeft = 0.25f,
							ToHitProfileRight = 0f,
							HitpointsFraction = 0.3f,
							Functions = new BodyPartFunction[1]
							{
								new BodyPartFunction
								{
									Function = BodyPartFunction.FunctionType.Manipulation,
									Weight = 0.5f
								}
							}
						},
						new BiologicalBodyPartType
						{
							Name = "Right arm",
							BodyKeyName = text,
							ArmorLayer = "clothesLayerBuffed",
							ToHitProfileBack = 0.15f,
							ToHitProfileFront = 0.15f,
							ToHitProfileLeft = 0f,
							ToHitProfileRight = 0.25f,
							HitpointsFraction = 0.3f,
							Functions = new BodyPartFunction[1]
							{
								new BodyPartFunction
								{
									Function = BodyPartFunction.FunctionType.Manipulation,
									Weight = 0.5f
								}
							}
						},
						new BiologicalBodyPartType
						{
							Name = "Left leg",
							BodyKeyName = text,
							ArmorLayer = "clothesLayerBuffed",
							ToHitProfileBack = 0.25f,
							ToHitProfileFront = 0.25f,
							ToHitProfileLeft = 0.3f,
							ToHitProfileRight = 0f,
							HitpointsFraction = 0.35f,
							Functions = new BodyPartFunction[1]
							{
								new BodyPartFunction
								{
									Function = BodyPartFunction.FunctionType.Locomotion,
									Weight = 0.35f
								}
							}
						},
						new BiologicalBodyPartType
						{
							Name = "Right leg",
							BodyKeyName = text,
							ArmorLayer = "clothesLayerBuffed",
							ToHitProfileBack = 0.25f,
							ToHitProfileFront = 0.25f,
							ToHitProfileLeft = 0f,
							ToHitProfileRight = 0.3f,
							HitpointsFraction = 0.35f,
							Functions = new BodyPartFunction[1]
							{
								new BodyPartFunction
								{
									Function = BodyPartFunction.FunctionType.Locomotion,
									Weight = 0.35f
								}
							}
						}
					}
				}
			}
		});
		return list;
	}

	protected override List<AttackType> InitAttackTypes()
	{
		List<AttackType> list = new List<AttackType>();
		list.Add(new AttackType
		{
			KeyName = "bushDragonSpray",
			Damage = "antiTwinkler",
			DamageMean = 0.1f,
			DamageStandardDeviation = 0f,
			ActionPointSound = GameData.Instance.AllSoundData["aliens/alienCombat/bushdragonPoisonShot"],
			ImpactSound = GameData.Instance.AllSoundData["aliens/alienCombat/poisonAlienImpact"],
			SoundAtStart = GameData.Instance.AllSoundData["aliens/alienCombat/bushdragonAttack"],
			DurationInSeconds = 1f,
			ActionPointInSeconds = 0.4f,
			AnimationStates = new AnimModifier[1] { AnimModifier.Near },
			RequiredSkill = "unarmedFighting",
			MaxRange = 100f,
			RangeType = AttackType.RangeTypes.Ray,
			BulletEffect = new BulletEffect
			{
				MuzzleDistance = 20f,
				StartColor = Color.Yellow,
				EndColor = Color.LightGreen
			}
		});
		return list;
	}
}
