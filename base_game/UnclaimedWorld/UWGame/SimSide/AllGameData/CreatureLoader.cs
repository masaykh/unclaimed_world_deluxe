using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.SimSide.Entities.Locomotors.Stances;
using UWGame.SimSide.XmlCollections;
using Xclna.Xna.Animation;

namespace UWGame.SimSide.AllGameData;

public class CreatureLoader
{
	public const int sensorRangeHuman = 350;

	public const int sensorRangeHumanNight = 200;

	public const float humanMaxRegainLimit = 0.5f;

	public const float humanFractionOfMaxHitpointsGainedPerDay = 0.3f;

	public static void Init(List<EntityType> listOfEntityTypes)
	{
		CreateHumanNeeds(out var babySleepNeed, out var childSleepNeed, out var youngAdultSleepNeed, out var adultSleepNeed, out var oldSleepNeed, out var humanFoodNeed, out var humanProteinNeed, out var humanMicronutrientsNeed, out var humanStimulantsNeed);
		EntityType entityType = new EntityType("meshtest");
		entityType.Name = "Mesh test";
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "meshtest",
				ModelScale = 3.5f
			}
		};
		EntityType item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("entity:skinnedtest");
		entityType.Name = "Skinned test";
		entityType.ThumbnailSmall = "HUD_thumbnail_diamondBird";
		entityType.IsFlyer = true;
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				ModelScale = 4f,
				AssetName = "skinnedtest",
				AnimConditions = new AnimConditionInfo[1]
				{
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "idle" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Idle
						}
					}
				}
			}
		};
		entityType.LocomotorType = new LocomotorType
		{
			MaxAngularSpeed = (float)Math.PI / 4f,
			FourSidedSymmetry = true,
			MeleeRadius = 20f,
			LeggedLocomotorType = new LeggedLocomotorType
			{
				TerrainNegateFactor = 1f,
				WalkSlowSpeed = 10f,
				WalkNormalSpeed = 25f,
				WalkFastSpeed = 24f
			},
			CollisionResponderType = new CollisionResponderType
			{
				AgentCollisionResponderType = new AgentCollisionResponderType()
			}
		};
		entityType.SensorType = new SensorType
		{
			Range = 200f,
			RangeAtNight = 150f,
			DetectionTypeKey = "defaultDetection"
		};
		entityType.BodyType = GameData.Instance.AllBodyTypes["bird"];
		EntityType entityType2 = entityType;
		entityType2.IntelligenceType = new IntelligenceType
		{
			IsMobile = true,
			StrengthRating = StrengthRating.WeakerThanHumans,
			AggroRange = 0f,
			MembersScoutingFraction = 0f
		};
		entityType2.BiologicalType = new BiologicalType
		{
			OxygenAndMuscleEnergyIncreaseRatePerDay = 10f,
			TimeToConsumeFullMealInDays = 0.0125f,
			StomachSizeFractionOfEntityBulk = 0.1f,
			StomachContentsDecreaseRatePerDay = 2f,
			ActiveStealthRating = 0.3f,
			MaxRegainLimit = 0.5f,
			FractionOfMaxHitpointsGainedPerDay = 0.3f,
			Carcass = "item:birdCarcass",
			RaceTypes = new RaceType[1]
			{
				new RaceType
				{
					KeyName = "pale",
					Name = "Pale race",
					PortraitSkinType = "Pale",
					PrimaryColor = "F7F4C5".ToColorVector3(),
					ModelBasicTextureName = "SkinnedTestTexture",
					Edge = 0.3f
				}
			},
			Castes = new List<CasteType>
			{
				new CasteType
				{
					KeyName = "male",
					Reproduction = Reproduction.Male,
					Edge = 0.51f,
					HeightMean = 0.5f,
					HeightStandardDeviation = 0.02f,
					WeightMean = 12f,
					WeightStandardDeviation = 0.15f,
					AgeGroupTypes = new List<AgeGroupType>
					{
						new AgeGroupType
						{
							Name = "Baby",
							AIAgeGroup = AIAgeGroup.Baby,
							CanReproduce = false,
							Edge = 1.5f,
							HeightTargetModifier = 0.05f,
							WeightTargetModifier = 0.03f
						},
						new AgeGroupType
						{
							Name = "Child",
							AIAgeGroup = AIAgeGroup.Child,
							CanReproduce = false,
							Edge = 11f,
							HeightTargetModifier = 0.6f,
							WeightTargetModifier = 0.5f
						},
						new AgeGroupType
						{
							Name = "Young adult",
							AIAgeGroup = AIAgeGroup.YoungAdult,
							CanReproduce = false,
							Edge = 16f,
							HeightTargetModifier = 0.97f,
							WeightTargetModifier = 0.92f
						},
						new AgeGroupType
						{
							Name = "Adult",
							AIAgeGroup = AIAgeGroup.Adult,
							CanReproduce = true,
							Edge = 72f,
							HeightTargetModifier = 1f,
							WeightTargetModifier = 1f
						},
						new AgeGroupType
						{
							Name = "Old",
							AIAgeGroup = AIAgeGroup.Old,
							CanReproduce = true,
							Edge = 200f,
							HeightTargetModifier = 0.9f,
							WeightTargetModifier = 1f
						}
					}
				},
				new CasteType
				{
					KeyName = "female",
					Reproduction = Reproduction.Female,
					Edge = 1f,
					HeightMean = 0.5f,
					HeightStandardDeviation = 0.02f,
					WeightMean = 12f,
					WeightStandardDeviation = 0.15f,
					AgeGroupTypes = new List<AgeGroupType>
					{
						new AgeGroupType
						{
							Name = "Baby",
							AIAgeGroup = AIAgeGroup.Baby,
							CanReproduce = false,
							Edge = 1.5f,
							HeightTargetModifier = 0.05f,
							WeightTargetModifier = 0.03f
						},
						new AgeGroupType
						{
							Name = "Child",
							AIAgeGroup = AIAgeGroup.Child,
							CanReproduce = false,
							Edge = 11f,
							HeightTargetModifier = 0.6f,
							WeightTargetModifier = 0.5f
						},
						new AgeGroupType
						{
							Name = "Young adult",
							AIAgeGroup = AIAgeGroup.YoungAdult,
							CanReproduce = false,
							Edge = 16f,
							HeightTargetModifier = 0.97f,
							WeightTargetModifier = 0.92f
						},
						new AgeGroupType
						{
							Name = "Adult",
							AIAgeGroup = AIAgeGroup.Adult,
							CanReproduce = true,
							Edge = 72f,
							HeightTargetModifier = 1f,
							WeightTargetModifier = 1f
						},
						new AgeGroupType
						{
							Name = "Old",
							AIAgeGroup = AIAgeGroup.Old,
							CanReproduce = false,
							Edge = 200f,
							HeightTargetModifier = 0.9f,
							WeightTargetModifier = 1f
						}
					}
				}
			}
		};
		listOfEntityTypes.Add(entityType2);
		NeedType needType = new NeedType();
		needType.KeyName = "foodEnergy";
		needType.FoodNeedType = new FoodNeedType
		{
			FoodNutrient = "foodEnergy",
			RequiredNutrientsAsFractionOfEntityBulk = 0.03f
		};
		needType.DecreasePerDay = new NormalDistribution
		{
			Mean = 2.0,
			StandardDeviation = 0.03999999910593033
		};
		needType.LimitForDecreasedEnergy = 0.05f;
		needType.DecreasedEnergyWeight = 0.08f;
		needType.PhysicalEffects = new PhysicalEffects
		{
			DaysAtZeroCausingCollapse = 3f,
			DaysAtZeroCausingDeath = 3.5f,
			DaysAtZeroDecreaseFactor = 1f,
			LimitForReducedGrowth = 0.1f,
			LimitForIncreasedSickness = 0.05f,
			UseExertionFactorToDecrease = false
		};
		NeedType needType2 = needType;
		entityType = new EntityType("entity:patrician");
		entityType.Name = "Patrician";
		entityType.ThumbnailSmall = "HUD_thumbnail_patrician";
		entityType.SummaryDescription = "Predator/scavenger";
		entityType.Description = "\n FEEDING CLASSIFICATION: Carnivore. Eats fish, slugs, smaller animals\n \n HEIGHT: Up to 3 m\n \n ANATOMY\n Vertical, octahedron-shaped body protected by exoskeleton. Its four legs are arranged symmetrically as are the sensory organs on the top.\n Its regal posture, 'crown' and territorial behaviour made researchers name the animal for the ancient Roman land holders.\n \n BEHAVIOR\n Highly territorial animal which jealously protects its hunting grounds. Kills by stabbing with a venomous spear that extends from its legs. With the spear, digestive enzymes are then pumped into the prey and the liquified tissues are sucked out.\n \n SURVIVAL GUIDE NOTES\n The animal will attack us if we enter its territory, but it moves rather slowly.";
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "patrician",
				GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]> { 
				{
					"normal",
					new GaitAnimationBracket[1]
					{
						new GaitAnimationBracket
						{
							MinimumSpeed = 0f,
							MaximumSpeed = 65f,
							StrideLength = 6f,
							StrideDuration = 0.44f,
							AnimationKey = "gaitWalk"
						}
					}
				} },
				DefaultInfo = new AnimConditionInfo
				{
					SoundAndAnimationSet = new RandomSoundAndAnimationSet
					{
						BaseAnimations = new string[1] { "idle" }
					}
				},
				DefaultStances = new AnimConditionInfo[2]
				{
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "idle" }
						}
					},
					new AnimConditionInfo
					{
						ConditionSet = new AnimConditions
						{
							Modifiers = new BitMask64(typeof(AnimModifier), 6)
						},
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "combatIdle" }
						}
					}
				},
				AnimConditions = new AnimConditionInfo[10]
				{
					new AnimConditionInfo
					{
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Moving
						},
						GaitSetKey = "normal"
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "idle" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Idle
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "eat" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Eating
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "combatIdle" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Idle,
							Modifiers = new BitMask64(typeof(AnimModifier), 6)
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "attackLowRight" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Attacking,
							Modifiers = new BitMask64(typeof(AnimModifier), 21, 1)
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "attackHighDouble" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Attacking,
							Modifiers = new BitMask64(typeof(AnimModifier), 20, 25)
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "hit" },
							Sounds = new string[1] { "aliens/alienCombat/flutter_Mat43_v2" }
						},
						Playback = Playback.Manual,
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Recoiling
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "dying" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "collapse" },
							Sounds = new string[1] { "aliens/alienCombat/strumming_Mat25_v1" }
						},
						Looping = Looping.No,
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying,
							Modifiers = new BitMask64(typeof(AnimModifier), 12)
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "dead" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying,
							Modifiers = new BitMask64(typeof(AnimModifier), 13)
						}
					}
				}
			}
		};
		entityType.LocomotorType = new LocomotorType
		{
			MaxAngularSpeed = (float)Math.PI / 4f,
			FourSidedSymmetry = true,
			MeleeRadius = 22f,
			LeggedLocomotorType = new LeggedLocomotorType
			{
				TerrainNegateFactor = 0.5f,
				WalkSlowSpeed = 10f,
				WalkNormalSpeed = 18f,
				WalkFastSpeed = 24f
			},
			CollisionResponderType = new CollisionResponderType
			{
				AgentCollisionResponderType = new AgentCollisionResponderType()
			}
		};
		entityType.SensorType = new SensorType
		{
			Range = 450f,
			RangeAtNight = 300f,
			DetectionTypeKey = "defaultDetection"
		};
		entityType.BodyType = GameData.Instance.AllBodyTypes["patrician"];
		entityType.ContainerType = new AgentStorageType
		{
			ItemStorageType = new ItemStorageType(0.8f),
			StomachStorageType = new ItemStorageType(0.5f)
		};
		EntityType entityType3 = entityType;
		entityType3.IntelligenceType = new IntelligenceType
		{
			IsMobile = true,
			CanAttack = true,
			CanUseWeapons = false,
			CanHunt = true,
			CanScout = true,
			CanExamine = true,
			CanPatrol = true,
			CanHaul = false,
			IsPredator = true,
			WillAttackNonThreatsNearby = true,
			StrengthRating = StrengthRating.LikeHumans,
			InterestInTriggerTypes = new string[2] { "mineTrigger", "spikeTrapTrigger" },
			Courage = 0.5f,
			MemoryInDays = 3f,
			Boldness = 0.8f,
			AggroRange = 300f,
			ContainerTransactTag = "patricianTransact",
			ChanceToRestAfterMeleeAttack = 0.3,
			MinRestTimeAfterAttackingInSeconds = 0.5f,
			MaxRestTimeAfterAttackingInSeconds = 1.2f,
			Prey = new string[5] { "entity:mudWorm", "entity:whiteThunderChicken", "entity:binalRat", "entity:bajingan", "entity:pygmyThunderChicken" },
			Attacks = new string[2] { "patricianHighDouble", "patricianLowRight" },
			Skills = new SerializableDictionary<string, float> { { "unarmedFighting", 0.4f } }
		};
		entityType3.BiologicalType = new BiologicalType
		{
			OrderKey = "patricianOrder",
			OxygenAndMuscleEnergyIncreaseRatePerDay = 10f,
			TimeToConsumeFullMealInDays = 0.005f,
			StomachSizeFractionOfEntityBulk = 0.15f,
			StomachContentsDecreaseRatePerDay = 2f,
			FoodItemTagsThatCanBeConsumed = new string[6] { "cookedMeat", "rawMeat", "smallRawMeat", "spoiledMeal", "inedibleMeat", "rottenMeat" },
			ExtractionProcessTypes = new string[12]
			{
				"extractMudWormMeat", "extractLeafCutterMeat", "extractThunderChickenMeat", "extractBinalRatMeat", "extractTwinklerMeat", "extractBushDragonMeat", "extractSwampDemonTreeMeat", "extractDemonTreeMeat", "extractMegapodMeat", "extractWhipjawMeat",
				"extractSpikePlantMeat", "extractForestGuardianMeat"
			},
			IsTerritorial = true,
			MaxRegainLimit = 0.5f,
			FractionOfMaxHitpointsGainedPerDay = 0.3f,
			ActiveStealthRating = 0.1f,
			Carcass = "item:patricianCarcass",
			RaceTypes = new RaceType[7]
			{
				new RaceType
				{
					Name = "Steppe patrician",
					PortraitSkinType = "Pale",
					PrimaryColor = "F7F4C5".ToColorVector3(),
					ModelBasicTextureName = "PatricianPaleTexture",
					Edge = 0.3f,
					ModelScale = 3f
				},
				new RaceType
				{
					Name = "Undocumented dark patrician",
					PortraitSkinType = "Dark",
					PrimaryColor = "564920".ToColorVector3(),
					ModelBasicTextureName = "PatricianBrownTexture",
					Edge = 0.6f,
					ModelScale = 3f
				},
				new RaceType
				{
					Name = "Zebra patrician",
					PortraitSkinType = "Yellow",
					PrimaryColor = "564920".ToColorVector3(),
					ModelBasicTextureName = "PatricianZebraTexture",
					Edge = 0.7f,
					ModelScale = 3f
				},
				new RaceType
				{
					Name = "Wasp patrician",
					PortraitSkinType = "Red",
					PrimaryColor = "564920".ToColorVector3(),
					ModelBasicTextureName = "PatricianWaspTexture",
					Edge = 0.8f,
					ModelScale = 3f
				},
				new RaceType
				{
					Name = "White patrician",
					PortraitSkinType = "White",
					PrimaryColor = "564920".ToColorVector3(),
					ModelBasicTextureName = "PatricianWhiteTexture",
					Edge = 0.85f,
					ModelScale = 3f
				},
				new RaceType
				{
					Name = "Undocumented purple patrician",
					PortraitSkinType = "Purple",
					PrimaryColor = "564920".ToColorVector3(),
					ModelBasicTextureName = "PatricianPurpleTexture",
					Edge = 0.87f,
					ModelScale = 3f
				},
				new RaceType
				{
					Name = "Black patrician",
					PortraitSkinType = "Black",
					PrimaryColor = "3F3411".ToColorVector3(),
					ModelBasicTextureName = "PatricianBlackTexture",
					Edge = 0.9f,
					ModelScale = 3f
				}
			},
			Castes = new List<CasteType>
			{
				new CasteType
				{
					KeyName = "male",
					Reproduction = Reproduction.Male,
					Edge = 0.51f,
					HeightMean = 2f,
					HeightStandardDeviation = 0.08f,
					WeightMean = 110f,
					WeightStandardDeviation = 0.15f,
					AgeGroupTypes = new List<AgeGroupType>
					{
						new AgeGroupType
						{
							Name = "Baby",
							AIAgeGroup = AIAgeGroup.Baby,
							CanReproduce = false,
							Edge = 1f,
							HeightTargetModifier = 0.05f,
							WeightTargetModifier = 0.03f,
							ModelScaleFraction = 0.3f,
							NeedTypes = new NeedType[1] { needType2 }
						},
						new AgeGroupType
						{
							Name = "Child",
							AIAgeGroup = AIAgeGroup.Child,
							CanReproduce = false,
							Edge = 2f,
							HeightTargetModifier = 0.6f,
							WeightTargetModifier = 0.5f,
							ModelScaleFraction = 0.6f,
							NeedTypes = new NeedType[1] { needType2 }
						},
						new AgeGroupType
						{
							Name = "Young adult",
							AIAgeGroup = AIAgeGroup.YoungAdult,
							CanReproduce = false,
							Edge = 8f,
							HeightTargetModifier = 0.97f,
							WeightTargetModifier = 0.92f,
							ModelScaleFraction = 0.85f,
							NeedTypes = new NeedType[1] { needType2 }
						},
						new AgeGroupType
						{
							Name = "Adult",
							AIAgeGroup = AIAgeGroup.Adult,
							CanReproduce = true,
							Edge = 20f,
							HeightTargetModifier = 1f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f,
							NeedTypes = new NeedType[1] { needType2 }
						},
						new AgeGroupType
						{
							Name = "Old",
							AIAgeGroup = AIAgeGroup.Old,
							CanReproduce = true,
							Edge = 22f,
							HeightTargetModifier = 0.9f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f,
							NeedTypes = new NeedType[1] { needType2 }
						}
					}
				},
				new CasteType
				{
					KeyName = "female",
					Reproduction = Reproduction.Female,
					Edge = 1f,
					HeightMean = 1.9f,
					HeightStandardDeviation = 0.05f,
					WeightMean = 100f,
					WeightStandardDeviation = 0.1f,
					AgeGroupTypes = new List<AgeGroupType>
					{
						new AgeGroupType
						{
							Name = "Baby",
							AIAgeGroup = AIAgeGroup.Baby,
							CanReproduce = false,
							Edge = 1f,
							HeightTargetModifier = 0.05f,
							WeightTargetModifier = 0.03f,
							ModelScaleFraction = 0.3f,
							NeedTypes = new NeedType[1] { needType2 }
						},
						new AgeGroupType
						{
							Name = "Child",
							AIAgeGroup = AIAgeGroup.Child,
							CanReproduce = false,
							Edge = 2f,
							HeightTargetModifier = 0.6f,
							WeightTargetModifier = 0.5f,
							ModelScaleFraction = 0.6f,
							NeedTypes = new NeedType[1] { needType2 }
						},
						new AgeGroupType
						{
							Name = "Young adult",
							AIAgeGroup = AIAgeGroup.YoungAdult,
							CanReproduce = false,
							Edge = 8f,
							HeightTargetModifier = 0.97f,
							WeightTargetModifier = 0.92f,
							ModelScaleFraction = 0.85f,
							NeedTypes = new NeedType[1] { needType2 }
						},
						new AgeGroupType
						{
							Name = "Adult",
							AIAgeGroup = AIAgeGroup.Adult,
							CanReproduce = true,
							Edge = 20f,
							HeightTargetModifier = 1f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f,
							NeedTypes = new NeedType[1] { needType2 }
						},
						new AgeGroupType
						{
							Name = "Old",
							AIAgeGroup = AIAgeGroup.Old,
							CanReproduce = false,
							Edge = 22f,
							HeightTargetModifier = 0.9f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f,
							NeedTypes = new NeedType[1] { needType2 }
						}
					}
				}
			}
		};
		listOfEntityTypes.Add(entityType3);
		needType = new NeedType();
		needType.KeyName = "foodEnergy";
		needType.FoodNeedType = new FoodNeedType
		{
			FoodNutrient = "foodEnergy",
			RequiredNutrientsAsFractionOfEntityBulk = 0.03f
		};
		needType.DecreasePerDay = new NormalDistribution
		{
			Mean = 2.0,
			StandardDeviation = 0.03999999910593033
		};
		needType.LimitForDecreasedEnergy = 0.05f;
		needType.DecreasedEnergyWeight = 0.08f;
		needType.PhysicalEffects = new PhysicalEffects
		{
			DaysAtZeroCausingCollapse = 3f,
			DaysAtZeroCausingDeath = 3.5f,
			DaysAtZeroDecreaseFactor = 1f,
			LimitForReducedGrowth = 0.1f,
			LimitForIncreasedSickness = 0.05f,
			UseExertionFactorToDecrease = false
		};
		NeedType needType3 = needType;
		entityType = new EntityType("entity:whipjaw");
		entityType.Name = "Great whipjaw";
		entityType.ThumbnailSmall = "HUD_thumbnail_snatcher";
		entityType.SummaryDescription = "Dangerous predator/scavenger";
		entityType.Description = "\n FEEDING CLASSIFICATION: Carnivore: Eats carrion and small animals.\n \n LENGTH: Up to 2.5 m\n \n ANATOMY\n We named the whipjaw for the strong arms mounted on its head: During feeding or combat, the animal uncoils these limbs with a powerful motion. The two outermost arms are lined with razor like teeth (for cutting) while the inner arm is studded with small hooks for grabbing and tearing. In combination, they function more or less like a knife and fork cutting off a piece of steak. The middle arm will bring pieces of flesh to the mouth which is situated at the top the head.\n The female is further equipped with two sharp horns and a strong keratin armor making the whipjaw cow almost unassailable.\n \n BEHAVIOR\n The animal makes up for its slow speed with highly aggressive behavior and will most often win when competing with other scavengers for a carcass.\n \n SURVIVAL GUIDE NOTES\n The animal is not afraid of humans and we should keep a distance.";
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				ModelScale = 2.1f,
				AssetName = "snatcher",
				GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]> { 
				{
					"normal",
					new GaitAnimationBracket[1]
					{
						new GaitAnimationBracket
						{
							MinimumSpeed = 0f,
							MaximumSpeed = 65f,
							StrideLength = 6f,
							StrideDuration = 0.44f,
							AnimationKey = "walk"
						}
					}
				} },
				DefaultInfo = new AnimConditionInfo
				{
					SoundAndAnimationSet = new RandomSoundAndAnimationSet
					{
						BaseAnimations = new string[1] { "idle" }
					}
				},
				DefaultStances = new AnimConditionInfo[2]
				{
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "idle" }
						}
					},
					new AnimConditionInfo
					{
						ConditionSet = new AnimConditions
						{
							Modifiers = new BitMask64(typeof(AnimModifier), 6)
						},
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "combatIdle" }
						}
					}
				},
				AnimConditions = new AnimConditionInfo[10]
				{
					new AnimConditionInfo
					{
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Moving
						},
						GaitSetKey = "normal"
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "idle" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Idle
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "combatIdle" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Idle,
							Modifiers = new BitMask64(typeof(AnimModifier), 6)
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "eat" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Eating
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "attack1" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Attacking,
							Modifiers = new BitMask64(typeof(AnimModifier), 21, 1)
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "attack2" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Attacking,
							Modifiers = new BitMask64(typeof(AnimModifier), 20, 25)
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "hit" },
							Sounds = new string[1] { "aliens/alienCombat/snatcherHit" }
						},
						Playback = Playback.Manual,
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Recoiling
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "idle" }
						},
						Playback = Playback.Manual,
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Recoiling
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "collapse" },
							Sounds = new string[1] { "aliens/alienCombat/snatcherDeath" }
						},
						Looping = Looping.No,
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying,
							Modifiers = new BitMask64(typeof(AnimModifier), 12)
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "dead" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying,
							Modifiers = new BitMask64(typeof(AnimModifier), 13)
						}
					}
				}
			}
		};
		entityType.LocomotorType = new LocomotorType
		{
			MaxAngularSpeed = (float)Math.PI / 4f,
			FourSidedSymmetry = true,
			MeleeRadius = 22f,
			LeggedLocomotorType = new LeggedLocomotorType
			{
				TerrainNegateFactor = 0.5f,
				WalkSlowSpeed = 14f,
				WalkNormalSpeed = 22f,
				WalkFastSpeed = 28f
			},
			CollisionResponderType = new CollisionResponderType
			{
				AgentCollisionResponderType = new AgentCollisionResponderType()
			}
		};
		entityType.SensorType = new SensorType
		{
			Range = 350f,
			RangeAtNight = 200f,
			DetectionTypeKey = "defaultDetection"
		};
		entityType.BodyType = GameData.Instance.AllBodyTypes["snatcher"];
		entityType.ContainerType = new AgentStorageType
		{
			ItemStorageType = new ItemStorageType(0.8f),
			StomachStorageType = new ItemStorageType(0.5f)
		};
		EntityType entityType4 = entityType;
		entityType4.IntelligenceType = new IntelligenceType
		{
			IsMobile = true,
			CanAttack = true,
			CanUseWeapons = false,
			CanHunt = true,
			CanScout = true,
			CanExamine = true,
			CanPatrol = true,
			CanHaul = false,
			IsPredator = true,
			WillAttackNonThreatsNearby = true,
			StrengthRating = StrengthRating.LikeHumans,
			InterestInTriggerTypes = new string[2] { "mineTrigger", "spikeTrapTrigger" },
			Courage = 0.25f,
			MemoryInDays = 3f,
			Boldness = 0.4f,
			AggroRange = 200f,
			ContainerTransactTag = "snatcherTransact",
			ChanceToRestAfterMeleeAttack = 0.9,
			MinRestTimeAfterAttackingInSeconds = 0.4f,
			MaxRestTimeAfterAttackingInSeconds = 1.2f,
			Prey = new string[6] { "entity:mudWorm", "entity:binalRat", "entity:whiteThunderChicken", "entity:pygmyThunderChicken", "entity:bajingan", "entity:human" },
			Attacks = new string[2] { "snatcherGrabAttack", "snatcherFastAttack" },
			Skills = new SerializableDictionary<string, float> { { "unarmedFighting", 0.3f } }
		};
		entityType4.BiologicalType = new BiologicalType
		{
			OrderKey = "carnufexOrder",
			OxygenAndMuscleEnergyIncreaseRatePerDay = 10f,
			TimeToConsumeFullMealInDays = 0.005f,
			StomachSizeFractionOfEntityBulk = 0.1f,
			StomachContentsDecreaseRatePerDay = 2f,
			FoodItemTagsThatCanBeConsumed = new string[6] { "cookedMeat", "rawMeat", "smallRawMeat", "spoiledMeal", "inedibleMeat", "rottenMeat" },
			ExtractionProcessTypes = new string[12]
			{
				"extractMudWormMeat", "extractPatricianMeat", "extractLeafCutterMeat", "extractThunderChickenMeat", "extractBinalRatMeat", "extractTwinklerMeat", "extractBushDragonMeat", "extractSwampDemonTreeMeat", "extractDemonTreeMeat", "extractMegapodMeat",
				"extractSpikePlantMeat", "extractForestGuardianMeat"
			},
			IsTerritorial = true,
			MaxRegainLimit = 0.5f,
			FractionOfMaxHitpointsGainedPerDay = 0.3f,
			ActiveStealthRating = 0.1f,
			Carcass = "item:whipjawCarcass",
			Castes = new List<CasteType>
			{
				new CasteType
				{
					KeyName = "male",
					Reproduction = Reproduction.Male,
					Edge = 0.51f,
					HeightMean = 2f,
					HeightStandardDeviation = 0.08f,
					WeightMean = 110f,
					WeightStandardDeviation = 0.15f,
					ModelBasicTextureName = "SnatcherArmoredTexture4",
					ModelName = "snatcherArmored",
					AgeGroupTypes = new List<AgeGroupType>
					{
						new AgeGroupType
						{
							Name = "Baby",
							AIAgeGroup = AIAgeGroup.Baby,
							CanReproduce = false,
							Edge = 1f,
							HeightTargetModifier = 0.05f,
							WeightTargetModifier = 0.03f,
							ModelScaleFraction = 0.3f,
							NeedTypes = new NeedType[1] { needType3 }
						},
						new AgeGroupType
						{
							Name = "Child",
							AIAgeGroup = AIAgeGroup.Child,
							CanReproduce = false,
							Edge = 3f,
							HeightTargetModifier = 0.6f,
							WeightTargetModifier = 0.5f,
							ModelScaleFraction = 0.6f,
							NeedTypes = new NeedType[1] { needType3 }
						},
						new AgeGroupType
						{
							Name = "Young adult",
							AIAgeGroup = AIAgeGroup.YoungAdult,
							CanReproduce = false,
							Edge = 6f,
							HeightTargetModifier = 0.97f,
							WeightTargetModifier = 0.92f,
							ModelScaleFraction = 0.85f,
							NeedTypes = new NeedType[1] { needType3 }
						},
						new AgeGroupType
						{
							Name = "Adult",
							AIAgeGroup = AIAgeGroup.Adult,
							CanReproduce = true,
							Edge = 36f,
							HeightTargetModifier = 1f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f,
							NeedTypes = new NeedType[1] { needType3 }
						},
						new AgeGroupType
						{
							Name = "Old",
							AIAgeGroup = AIAgeGroup.Old,
							CanReproduce = true,
							Edge = 40f,
							HeightTargetModifier = 0.9f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f,
							NeedTypes = new NeedType[1] { needType3 }
						}
					}
				},
				new CasteType
				{
					KeyName = "female",
					Reproduction = Reproduction.Female,
					Edge = 1f,
					HeightMean = 1.9f,
					HeightStandardDeviation = 0.05f,
					WeightMean = 100f,
					WeightStandardDeviation = 0.1f,
					ModelBasicTextureName = "SnatcherFemaleTexture",
					AgeGroupTypes = new List<AgeGroupType>
					{
						new AgeGroupType
						{
							Name = "Baby",
							AIAgeGroup = AIAgeGroup.Baby,
							CanReproduce = false,
							Edge = 1f,
							HeightTargetModifier = 0.05f,
							WeightTargetModifier = 0.03f,
							ModelScaleFraction = 0.3f,
							NeedTypes = new NeedType[1] { needType3 }
						},
						new AgeGroupType
						{
							Name = "Child",
							AIAgeGroup = AIAgeGroup.Child,
							CanReproduce = false,
							Edge = 4f,
							HeightTargetModifier = 0.6f,
							WeightTargetModifier = 0.5f,
							ModelScaleFraction = 0.6f,
							NeedTypes = new NeedType[1] { needType3 }
						},
						new AgeGroupType
						{
							Name = "Young adult",
							AIAgeGroup = AIAgeGroup.YoungAdult,
							CanReproduce = false,
							Edge = 6f,
							HeightTargetModifier = 0.97f,
							WeightTargetModifier = 0.92f,
							ModelScaleFraction = 0.85f,
							NeedTypes = new NeedType[1] { needType3 }
						},
						new AgeGroupType
						{
							Name = "Adult",
							AIAgeGroup = AIAgeGroup.Adult,
							CanReproduce = true,
							Edge = 42f,
							HeightTargetModifier = 1f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f,
							NeedTypes = new NeedType[1] { needType3 }
						},
						new AgeGroupType
						{
							Name = "Old",
							AIAgeGroup = AIAgeGroup.Old,
							CanReproduce = false,
							Edge = 48f,
							HeightTargetModifier = 0.9f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f,
							NeedTypes = new NeedType[1] { needType3 }
						}
					}
				}
			}
		};
		listOfEntityTypes.Add(entityType4);
		entityType = new EntityType("entity:lesserWhipjaw");
		entityType.Name = "Lesser whipjaw";
		entityType.ThumbnailSmall = "HUD_thumbnail_snatcher";
		entityType.SummaryDescription = "Small scavenger. Not dangerous but can be overwhelming in large numbers.";
		entityType.Description = "\n FEEDING CLASSIFICATION: Carnivore: Eats carrion and small animals.\n \n LENGTH: About 30 cm\n \n ANATOMY\n We named the whipjaw for the strong arms mounted on its head: During feeding or combat, the animal uncoils these limbs with a powerful motion. The two outermost arms are lined with razor like teeth (for cutting) while the inner arm is studded with small hooks for grabbing and tearing. In combination, they function more or less like a knife and fork cutting off a piece of steak. The middle arm will bring pieces of flesh to the mouth which is situated at the top the head.\n \n BEHAVIOR Will periodically migrate in large numbers along rivers and streams in search for food. \n \n SURVIVAL GUIDE NOTES\n The animal is not afraid of humans but only poses a threat to our food stockpiles.";
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				ModelScale = 0.8f,
				AssetName = "snatcher",
				ModelBasicTextureName = "SnatcherVariantTexture5",
				GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]> { 
				{
					"normal",
					new GaitAnimationBracket[1]
					{
						new GaitAnimationBracket
						{
							MinimumSpeed = 0f,
							MaximumSpeed = 77f,
							StrideLength = 5f,
							StrideDuration = 0.44f,
							AnimationKey = "walk"
						}
					}
				} },
				DefaultInfo = new AnimConditionInfo
				{
					SoundAndAnimationSet = new RandomSoundAndAnimationSet
					{
						BaseAnimations = new string[1] { "idle" }
					}
				},
				DefaultStances = new AnimConditionInfo[2]
				{
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "idle" }
						}
					},
					new AnimConditionInfo
					{
						ConditionSet = new AnimConditions
						{
							Modifiers = new BitMask64(typeof(AnimModifier), 6)
						},
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "combatIdle" }
						}
					}
				},
				AnimConditions = new AnimConditionInfo[10]
				{
					new AnimConditionInfo
					{
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Moving
						},
						GaitSetKey = "normal"
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "idle" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Idle
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "combatIdle" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Idle,
							Modifiers = new BitMask64(typeof(AnimModifier), 6)
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "eat" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Eating
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "attack1" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Attacking,
							Modifiers = new BitMask64(typeof(AnimModifier), 21, 1)
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "attack2" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Attacking,
							Modifiers = new BitMask64(typeof(AnimModifier), 20, 25)
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "hit" },
							Sounds = new string[1] { "aliens/alienCombat/snatcherHit" }
						},
						Playback = Playback.Manual,
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Recoiling
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "idle" }
						},
						Playback = Playback.Manual,
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Recoiling
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "collapse" },
							Sounds = new string[1] { "aliens/alienCombat/snatcherDeath" }
						},
						Looping = Looping.No,
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying,
							Modifiers = new BitMask64(typeof(AnimModifier), 12)
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "dead" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying,
							Modifiers = new BitMask64(typeof(AnimModifier), 13)
						}
					}
				}
			}
		};
		entityType.LocomotorType = new LocomotorType
		{
			MaxAngularSpeed = (float)Math.PI / 4f,
			FourSidedSymmetry = true,
			MeleeRadius = 22f,
			LeggedLocomotorType = new LeggedLocomotorType
			{
				TerrainNegateFactor = 0.1f,
				WalkSlowSpeed = 15f,
				WalkNormalSpeed = 43f,
				WalkFastSpeed = 64f
			},
			CollisionResponderType = new CollisionResponderType
			{
				AgentCollisionResponderType = new AgentCollisionResponderType()
			}
		};
		entityType.SensorType = new SensorType
		{
			Range = 450f,
			RangeAtNight = 300f,
			DetectionTypeKey = "defaultDetection"
		};
		entityType.BodyType = GameData.Instance.AllBodyTypes["snatcher"];
		entityType.ContainerType = new AgentStorageType
		{
			ItemStorageType = new ItemStorageType(0.8f),
			StomachStorageType = new ItemStorageType(0.5f)
		};
		entityType4 = entityType;
		entityType4.IntelligenceType = new IntelligenceType
		{
			IsMobile = true,
			CanAttack = false,
			CanUseWeapons = false,
			CanHunt = false,
			CanScout = true,
			CanExamine = true,
			CanPatrol = false,
			CanHaul = false,
			IsPredator = false,
			StrengthRating = StrengthRating.None,
			InterestInTriggerTypes = new string[4] { "mineTrigger", "smallImprovisedTrapTrigger", "spikeTrapTrigger", "animalMigrateTrigger" },
			Courage = 0f,
			MemoryInDays = 0.2f,
			Boldness = 1.49f,
			AggroRange = 0f,
			ContainerTransactTag = "ratTransact"
		};
		entityType4.BiologicalType = new BiologicalType
		{
			OrderKey = "carnufexOrder",
			OxygenAndMuscleEnergyIncreaseRatePerDay = 10f,
			FoodItemTagsThatCanBeConsumed = new string[6] { "cookedMeat", "rawMeat", "smallRawMeat", "spoiledMeal", "inedibleMeat", "rottenMeat" },
			ExtractionProcessTypes = new string[12]
			{
				"extractMudWormMeat", "extractPatricianMeat", "extractLeafCutterMeat", "extractThunderChickenMeat", "extractBinalRatMeat", "extractTwinklerMeat", "extractBushDragonMeat", "extractSwampDemonTreeMeat", "extractDemonTreeMeat", "extractMegapodMeat",
				"extractSpikePlantMeat", "extractForestGuardianMeat"
			},
			TimeToConsumeFullMealInDays = 0.0125f,
			StomachSizeFractionOfEntityBulk = 0.2f,
			StomachContentsDecreaseRatePerDay = 3f,
			ActiveStealthRating = 0.1f,
			IsVermin = true,
			MaxRegainLimit = 0.5f,
			FractionOfMaxHitpointsGainedPerDay = 0.3f,
			Carcass = "item:whipjawCarcass",
			Castes = new List<CasteType>
			{
				new CasteType
				{
					KeyName = "male",
					Reproduction = Reproduction.Male,
					Edge = 0.51f,
					HeightMean = 0.5f,
					HeightStandardDeviation = 0.02f,
					WeightMean = 30f,
					WeightStandardDeviation = 3f,
					AgeGroupTypes = new List<AgeGroupType>
					{
						new AgeGroupType
						{
							Name = "Chick",
							AIAgeGroup = AIAgeGroup.Baby,
							CanReproduce = false,
							Edge = 0.5f,
							HeightTargetModifier = 0.05f,
							WeightTargetModifier = 0.03f,
							ModelScaleFraction = 0.5f,
							NeedTypes = new NeedType[1] { needType3 }
						},
						new AgeGroupType
						{
							Name = "Chick",
							AIAgeGroup = AIAgeGroup.Child,
							CanReproduce = false,
							Edge = 2f,
							HeightTargetModifier = 0.6f,
							WeightTargetModifier = 0.5f,
							ModelScaleFraction = 0.6f,
							NeedTypes = new NeedType[1] { needType3 }
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.YoungAdult,
							CanReproduce = false,
							Edge = 4f,
							HeightTargetModifier = 0.97f,
							WeightTargetModifier = 0.92f,
							ModelScaleFraction = 0.85f,
							NeedTypes = new NeedType[1] { needType3 }
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.Adult,
							CanReproduce = true,
							Edge = 20f,
							HeightTargetModifier = 1f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f,
							NeedTypes = new NeedType[1] { needType3 }
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.Old,
							CanReproduce = true,
							Edge = 22f,
							HeightTargetModifier = 0.9f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f,
							NeedTypes = new NeedType[1] { needType3 }
						}
					}
				},
				new CasteType
				{
					KeyName = "female",
					Reproduction = Reproduction.Female,
					Edge = 1f,
					HeightMean = 0.45f,
					HeightStandardDeviation = 0.02f,
					WeightMean = 28f,
					WeightStandardDeviation = 2f,
					AgeGroupTypes = new List<AgeGroupType>
					{
						new AgeGroupType
						{
							Name = "Chick",
							AIAgeGroup = AIAgeGroup.Baby,
							CanReproduce = false,
							Edge = 0.5f,
							HeightTargetModifier = 0.05f,
							WeightTargetModifier = 0.03f,
							ModelScaleFraction = 0.3f,
							NeedTypes = new NeedType[1] { needType3 }
						},
						new AgeGroupType
						{
							Name = "Chick",
							AIAgeGroup = AIAgeGroup.Child,
							CanReproduce = false,
							Edge = 2f,
							HeightTargetModifier = 0.6f,
							WeightTargetModifier = 0.5f,
							ModelScaleFraction = 0.6f,
							NeedTypes = new NeedType[1] { needType3 }
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.YoungAdult,
							CanReproduce = false,
							Edge = 4f,
							HeightTargetModifier = 0.97f,
							WeightTargetModifier = 0.92f,
							ModelScaleFraction = 0.85f,
							NeedTypes = new NeedType[1] { needType3 }
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.Adult,
							CanReproduce = true,
							Edge = 20f,
							HeightTargetModifier = 1f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f,
							NeedTypes = new NeedType[1] { needType3 }
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.Old,
							CanReproduce = false,
							Edge = 22f,
							HeightTargetModifier = 0.9f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f,
							NeedTypes = new NeedType[1] { needType3 }
						}
					}
				}
			}
		};
		listOfEntityTypes.Add(entityType4);
		entityType = new EntityType("entity:spikePlant");
		entityType.Name = "Ursinix";
		entityType.ThumbnailSmall = "HUD_thumbnail_ursinix";
		entityType.SummaryDescription = "Ambush predator";
		entityType.Description = "";
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				ModelScale = 1.8f,
				AssetName = "spikePlant",
				ModelBasicTextureName = "SpikePlantTexture",
				GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]> { 
				{
					"normal",
					new GaitAnimationBracket[1]
					{
						new GaitAnimationBracket
						{
							MinimumSpeed = 0f,
							MaximumSpeed = 0f,
							StrideLength = 0.005f,
							StrideDuration = 0.44f,
							AnimationKey = "idle"
						}
					}
				} },
				AnimConditions = new AnimConditionInfo[8]
				{
					new AnimConditionInfo
					{
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Moving
						},
						GaitSetKey = "normal"
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "idle" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Idle
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "combatIdle" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Idle,
							Modifiers = new BitMask64(typeof(AnimModifier), 6)
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "attack" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Attacking,
							Modifiers = new BitMask64(typeof(AnimModifier), 21)
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "hit" },
							Sounds = new string[1] { "aliens/rattleWoodenShort" }
						},
						Playback = Playback.Manual,
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Recoiling
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "dead" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "collapse" },
							Sounds = new string[1] { "aliens/rattleWooden" }
						},
						Looping = Looping.No,
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying,
							Modifiers = new BitMask64(typeof(AnimModifier), 12)
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "dead" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying,
							Modifiers = new BitMask64(typeof(AnimModifier), 13)
						}
					}
				}
			}
		};
		entityType.LocomotorType = new LocomotorType
		{
			MaxAngularSpeed = (float)Math.PI / 4f,
			FourSidedSymmetry = false,
			MeleeRadius = 1f,
			LeggedLocomotorType = new LeggedLocomotorType
			{
				TerrainNegateFactor = 0.5f,
				WalkSlowSpeed = 0f,
				WalkNormalSpeed = 0f,
				WalkFastSpeed = 0f
			},
			CollisionResponderType = new CollisionResponderType
			{
				AgentCollisionResponderType = new AgentCollisionResponderType()
			}
		};
		entityType.SensorType = new SensorType
		{
			Range = 450f,
			RangeAtNight = 300f,
			DetectionTypeKey = "defaultDetection"
		};
		entityType.BodyType = GameData.Instance.AllBodyTypes["spikePlant"];
		entityType.ContainerType = new AgentStorageType
		{
			CanTransactWithTags = new string[4] { "ratTransact", "humanTransact", "leafcutterTransact", "chickenTransact" },
			ItemStorageType = new ItemStorageType(0.5f),
			StomachStorageType = new ItemStorageType(0.07f)
		};
		EntityType entityType5 = entityType;
		entityType5.IntelligenceType = new IntelligenceType
		{
			IsMobile = true,
			CanAttack = true,
			CanUseWeapons = false,
			CanHunt = true,
			CanScout = false,
			CanExamine = false,
			CanPatrol = false,
			CanHaul = false,
			IsPredator = true,
			WillAttackNonThreatsNearby = false,
			StrengthRating = StrengthRating.None,
			InterestInTriggerTypes = new string[1] { "mineTrigger" },
			Courage = 1f,
			MemoryInDays = 3f,
			Boldness = 1f,
			AggroRange = 300f,
			ChanceToRestAfterMeleeAttack = 0.3,
			MinRestTimeAfterAttackingInSeconds = 0.4f,
			MaxRestTimeAfterAttackingInSeconds = 1.2f,
			Prey = new string[5] { "entity:mudWorm", "entity:binalRat", "entity:whiteThunderChicken", "entity:pygmyThunderChicken", "entity:human" },
			Attacks = new string[1] { "spikePlantAttack" },
			Skills = new SerializableDictionary<string, float> { { "unarmedFighting", 1f } }
		};
		entityType5.BiologicalType = new BiologicalType
		{
			OrderKey = "spikePlantOrder",
			OxygenAndMuscleEnergyIncreaseRatePerDay = 10f,
			TimeToConsumeFullMealInDays = 0.0125f,
			StomachSizeFractionOfEntityBulk = 0.15f,
			StomachContentsDecreaseRatePerDay = 2f,
			IsTerritorial = true,
			MaxRegainLimit = 0.5f,
			FractionOfMaxHitpointsGainedPerDay = 0.3f,
			ActiveStealthRating = 1f,
			Carcass = "item:spikePlantCarcass",
			Castes = new List<CasteType>
			{
				new CasteType
				{
					KeyName = "male",
					Reproduction = Reproduction.Male,
					Edge = 0.51f,
					HeightMean = 2f,
					HeightStandardDeviation = 0.08f,
					WeightMean = 110f,
					WeightStandardDeviation = 0.15f,
					AgeGroupTypes = new List<AgeGroupType>
					{
						new AgeGroupType
						{
							Name = "Baby",
							AIAgeGroup = AIAgeGroup.Baby,
							CanReproduce = false,
							Edge = 1.5f,
							HeightTargetModifier = 0.05f,
							WeightTargetModifier = 0.03f,
							ModelScaleFraction = 0.3f
						},
						new AgeGroupType
						{
							Name = "Child",
							AIAgeGroup = AIAgeGroup.Child,
							CanReproduce = false,
							Edge = 11f,
							HeightTargetModifier = 0.6f,
							WeightTargetModifier = 0.5f,
							ModelScaleFraction = 0.6f
						},
						new AgeGroupType
						{
							Name = "Young adult",
							AIAgeGroup = AIAgeGroup.YoungAdult,
							CanReproduce = false,
							Edge = 16f,
							HeightTargetModifier = 0.97f,
							WeightTargetModifier = 0.92f,
							ModelScaleFraction = 0.85f
						},
						new AgeGroupType
						{
							Name = "Adult",
							AIAgeGroup = AIAgeGroup.Adult,
							CanReproduce = true,
							Edge = 52f,
							HeightTargetModifier = 1f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f
						},
						new AgeGroupType
						{
							Name = "Old",
							AIAgeGroup = AIAgeGroup.Old,
							CanReproduce = true,
							Edge = 60f,
							HeightTargetModifier = 0.9f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f
						}
					}
				},
				new CasteType
				{
					KeyName = "female",
					Reproduction = Reproduction.Female,
					Edge = 1f,
					HeightMean = 1.9f,
					HeightStandardDeviation = 0.05f,
					WeightMean = 100f,
					WeightStandardDeviation = 0.1f,
					AgeGroupTypes = new List<AgeGroupType>
					{
						new AgeGroupType
						{
							Name = "Baby",
							AIAgeGroup = AIAgeGroup.Baby,
							CanReproduce = false,
							Edge = 1.5f,
							HeightTargetModifier = 0.05f,
							WeightTargetModifier = 0.03f,
							ModelScaleFraction = 0.3f
						},
						new AgeGroupType
						{
							Name = "Child",
							AIAgeGroup = AIAgeGroup.Child,
							CanReproduce = false,
							Edge = 11f,
							HeightTargetModifier = 0.6f,
							WeightTargetModifier = 0.5f,
							ModelScaleFraction = 0.6f
						},
						new AgeGroupType
						{
							Name = "Young adult",
							AIAgeGroup = AIAgeGroup.YoungAdult,
							CanReproduce = false,
							Edge = 16f,
							HeightTargetModifier = 0.97f,
							WeightTargetModifier = 0.92f,
							ModelScaleFraction = 0.85f
						},
						new AgeGroupType
						{
							Name = "Adult",
							AIAgeGroup = AIAgeGroup.Adult,
							CanReproduce = true,
							Edge = 52f,
							HeightTargetModifier = 1f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f
						},
						new AgeGroupType
						{
							Name = "Old",
							AIAgeGroup = AIAgeGroup.Old,
							CanReproduce = false,
							Edge = 60f,
							HeightTargetModifier = 0.9f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f
						}
					}
				}
			}
		};
		listOfEntityTypes.Add(entityType5);
		needType = new NeedType();
		needType.KeyName = "foodEnergy";
		needType.FoodNeedType = new FoodNeedType
		{
			FoodNutrient = "foodEnergy",
			RequiredNutrientsAsFractionOfEntityBulk = 0.04f
		};
		needType.DecreasePerDay = new NormalDistribution
		{
			Mean = 2.0,
			StandardDeviation = 0.03999999910593033
		};
		needType.LimitForDecreasedEnergy = 0.05f;
		needType.DecreasedEnergyWeight = 0.08f;
		needType.PhysicalEffects = new PhysicalEffects
		{
			DaysAtZeroCausingCollapse = 4f,
			DaysAtZeroCausingDeath = 4f,
			DaysAtZeroDecreaseFactor = 1f,
			LimitForReducedGrowth = 0.1f,
			LimitForIncreasedSickness = 0.05f,
			UseExertionFactorToDecrease = false
		};
		NeedType needType4 = needType;
		entityType = new EntityType("entity:megapod");
		entityType.Name = "Megapod";
		entityType.ThumbnailSmall = "HUD_thumbnail_worm";
		entityType.SummaryDescription = "Large slug-like animal";
		entityType.Description = "\n FEEDING CLASSIFICATION: Herbivore.\n \n SIZE: up to 600 cm\n \n ANATOMY\n Specialized for an amphibious lifestyle: On land, moves by rhythmic contractions of its 'foot' (for which we named the animal), while in water, it is aided by a powerful tail for swimming. Formidable tusks extend from the jawbones.\n \n BEHAVIOR\n The animal lives in swamps where it divides its time between family life above water and foraging below the waterline. Diet consists of underwater plants that it uproots with its strong forward-pointing tusks which are also used in fights for dominance with its own species and in defending against predators.\n \n THREAT LEVEL\n The animal will defend its territory but its land speed is slow enough for intruders to be able to escape without getting harmed.";
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				ModelScale = 2.5f,
				AssetName = "worm",
				ModelBasicTextureName = "WormTexture",
				GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]> { 
				{
					"normal",
					new GaitAnimationBracket[1]
					{
						new GaitAnimationBracket
						{
							MinimumSpeed = 20f,
							MaximumSpeed = 35f,
							StrideLength = 22f,
							StrideDuration = 1.4f,
							AnimationKey = "walk"
						}
					}
				} },
				DefaultInfo = new AnimConditionInfo
				{
					SoundAndAnimationSet = new RandomSoundAndAnimationSet
					{
						BaseAnimations = new string[1] { "idle" }
					}
				},
				DefaultStances = new AnimConditionInfo[2]
				{
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "idle" }
						}
					},
					new AnimConditionInfo
					{
						ConditionSet = new AnimConditions
						{
							Modifiers = new BitMask64(typeof(AnimModifier), 6)
						},
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "combatIdle" }
						}
					}
				},
				AnimConditions = new AnimConditionInfo[9]
				{
					new AnimConditionInfo
					{
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Moving
						},
						GaitSetKey = "normal"
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "idle" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Idle
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "eat" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Eating
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "combatIdle" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Idle,
							Modifiers = new BitMask64(typeof(AnimModifier), 6)
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "attack" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Attacking
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "hit" },
							Sounds = new string[1] { "aliens/alienCombat/wormHitShort" }
						},
						Playback = Playback.Manual,
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Recoiling
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "dead" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "collapse" },
							Sounds = new string[1] { "aliens/alienCombat/wormDeathShort" }
						},
						Looping = Looping.No,
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying,
							Modifiers = new BitMask64(typeof(AnimModifier), 12)
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "dead" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying,
							Modifiers = new BitMask64(typeof(AnimModifier), 13)
						}
					}
				}
			}
		};
		entityType.LocomotorType = new LocomotorType
		{
			MaxAngularSpeed = (float)Math.PI / 4f,
			FourSidedSymmetry = false,
			MeleeRadius = 32f,
			LeggedLocomotorType = new LeggedLocomotorType
			{
				TerrainNegateFactor = 0.1f,
				WalkSlowSpeed = 5f,
				WalkNormalSpeed = 6f,
				WalkFastSpeed = 8f
			},
			CollisionResponderType = new CollisionResponderType
			{
				AgentCollisionResponderType = new AgentCollisionResponderType()
			}
		};
		entityType.SensorType = new SensorType
		{
			Range = 450f,
			RangeAtNight = 300f,
			DetectionTypeKey = "defaultDetection"
		};
		entityType.BodyType = GameData.Instance.AllBodyTypes["worm"];
		entityType.ContainerType = new AgentStorageType
		{
			ItemStorageType = new ItemStorageType(0.8f),
			StomachStorageType = new ItemStorageType(0.5f)
		};
		EntityType entityType6 = entityType;
		entityType6.IntelligenceType = new IntelligenceType
		{
			IsMobile = true,
			CanAttack = true,
			CanUseWeapons = false,
			CanHunt = true,
			CanScout = true,
			CanExamine = true,
			CanPatrol = true,
			CanHaul = false,
			IsPredator = true,
			WillAttackNonThreatsNearby = true,
			StrengthRating = StrengthRating.LikeHumans,
			InterestInTriggerTypes = new string[2] { "mineTrigger", "spikeTrapTrigger" },
			Courage = 0.3f,
			MemoryInDays = 3f,
			Boldness = 0.8f,
			AggroRange = 250f,
			ContainerTransactTag = "wormTransact",
			ChanceToRestAfterMeleeAttack = 0.4,
			MinRestTimeAfterAttackingInSeconds = 0.5f,
			MaxRestTimeAfterAttackingInSeconds = 1f,
			Prey = new string[6] { "entity:mudWorm", "entity:binalRat", "entity:whiteThunderChicken", "entity:bajingan", "entity:pygmyThunderChicken", "entity:human" },
			Attacks = new string[1] { "wormAttack" },
			Skills = new SerializableDictionary<string, float> { { "unarmedFighting", 0.3f } }
		};
		entityType6.BiologicalType = new BiologicalType
		{
			OrderKey = "wormOrder",
			OxygenAndMuscleEnergyIncreaseRatePerDay = 7.5f,
			TimeToConsumeFullMealInDays = 0.005f,
			StomachSizeFractionOfEntityBulk = 0.25f,
			StomachContentsDecreaseRatePerDay = 2f,
			FoodItemTagsThatCanBeConsumed = new string[6] { "cookedMeat", "rawMeat", "smallRawMeat", "spoiledMeal", "inedibleMeat", "rottenMeat" },
			ExtractionProcessTypes = new string[12]
			{
				"extractMudWormMeat", "extractPatricianMeat", "extractLeafCutterMeat", "extractThunderChickenMeat", "extractBinalRatMeat", "extractTwinklerMeat", "extractBushDragonMeat", "extractSwampDemonTreeMeat", "extractDemonTreeMeat", "extractWhipjawMeat",
				"extractSpikePlantMeat", "extractForestGuardianMeat"
			},
			IsTerritorial = true,
			MaxRegainLimit = 0.5f,
			FractionOfMaxHitpointsGainedPerDay = 0.3f,
			ActiveStealthRating = 1f,
			Carcass = "item:megapodCarcass",
			Castes = new List<CasteType>
			{
				new CasteType
				{
					KeyName = "male",
					Reproduction = Reproduction.Male,
					Edge = 0.51f,
					HeightMean = 2f,
					HeightStandardDeviation = 0.08f,
					WeightMean = 110f,
					WeightStandardDeviation = 0.15f,
					AgeGroupTypes = new List<AgeGroupType>
					{
						new AgeGroupType
						{
							Name = "Baby",
							AIAgeGroup = AIAgeGroup.Baby,
							CanReproduce = false,
							Edge = 0.5f,
							HeightTargetModifier = 0.05f,
							WeightTargetModifier = 0.25f,
							ModelName = "wormThin",
							ModelBasicTextureName = "WormThinTexture",
							ModelScaleFraction = 0.4f,
							NeedTypes = new NeedType[1] { needType4 }
						},
						new AgeGroupType
						{
							Name = "Child",
							AIAgeGroup = AIAgeGroup.Child,
							CanReproduce = false,
							Edge = 2f,
							HeightTargetModifier = 0.6f,
							WeightTargetModifier = 0.5f,
							ModelName = "wormThin",
							ModelBasicTextureName = "WormThinTexture",
							ModelScaleFraction = 0.75f,
							NeedTypes = new NeedType[1] { needType4 }
						},
						new AgeGroupType
						{
							Name = "Young adult",
							AIAgeGroup = AIAgeGroup.YoungAdult,
							CanReproduce = false,
							Edge = 4f,
							HeightTargetModifier = 0.97f,
							WeightTargetModifier = 0.75f,
							ModelScaleFraction = 0.85f,
							NeedTypes = new NeedType[1] { needType4 }
						},
						new AgeGroupType
						{
							Name = "Adult",
							AIAgeGroup = AIAgeGroup.Adult,
							CanReproduce = true,
							Edge = 19f,
							HeightTargetModifier = 1f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f,
							NeedTypes = new NeedType[1] { needType4 }
						},
						new AgeGroupType
						{
							Name = "Old",
							AIAgeGroup = AIAgeGroup.Old,
							CanReproduce = true,
							Edge = 20f,
							HeightTargetModifier = 0.9f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f,
							NeedTypes = new NeedType[1] { needType4 }
						}
					}
				},
				new CasteType
				{
					KeyName = "female",
					Reproduction = Reproduction.Female,
					Edge = 1f,
					HeightMean = 1.9f,
					HeightStandardDeviation = 0.05f,
					WeightMean = 100f,
					WeightStandardDeviation = 0.1f,
					AgeGroupTypes = new List<AgeGroupType>
					{
						new AgeGroupType
						{
							Name = "Baby",
							AIAgeGroup = AIAgeGroup.Baby,
							CanReproduce = false,
							Edge = 0.5f,
							HeightTargetModifier = 0.05f,
							WeightTargetModifier = 0.03f,
							ModelName = "wormThin",
							ModelBasicTextureName = "WormThinTexture",
							ModelScaleFraction = 0.4f,
							NeedTypes = new NeedType[1] { needType4 }
						},
						new AgeGroupType
						{
							Name = "Child",
							AIAgeGroup = AIAgeGroup.Child,
							CanReproduce = false,
							Edge = 2f,
							HeightTargetModifier = 0.6f,
							WeightTargetModifier = 0.5f,
							ModelName = "wormThin",
							ModelBasicTextureName = "WormThinTexture",
							ModelScaleFraction = 0.75f,
							NeedTypes = new NeedType[1] { needType4 }
						},
						new AgeGroupType
						{
							Name = "Young adult",
							AIAgeGroup = AIAgeGroup.YoungAdult,
							CanReproduce = false,
							Edge = 4f,
							HeightTargetModifier = 0.97f,
							WeightTargetModifier = 0.92f,
							ModelName = "wormThin",
							ModelBasicTextureName = "WormThinTexture",
							ModelScaleFraction = 0.85f,
							NeedTypes = new NeedType[1] { needType4 }
						},
						new AgeGroupType
						{
							Name = "Adult",
							AIAgeGroup = AIAgeGroup.Adult,
							CanReproduce = true,
							Edge = 19f,
							HeightTargetModifier = 1f,
							WeightTargetModifier = 1f,
							ModelName = "wormThin",
							ModelBasicTextureName = "WormThinTexture",
							ModelScaleFraction = 0.9f,
							NeedTypes = new NeedType[1] { needType4 }
						},
						new AgeGroupType
						{
							Name = "Old",
							AIAgeGroup = AIAgeGroup.Old,
							CanReproduce = false,
							Edge = 20f,
							HeightTargetModifier = 0.9f,
							WeightTargetModifier = 1f,
							ModelName = "wormThin",
							ModelBasicTextureName = "WormThinTexture",
							ModelScaleFraction = 0.9f,
							NeedTypes = new NeedType[1] { needType4 }
						}
					}
				}
			}
		};
		listOfEntityTypes.Add(entityType6);
		entityType = new EntityType("entity:turnip");
		entityType.Name = "Turnip";
		entityType.ThumbnailSmall = "HUD_thumbnail_turnip";
		entityType.SummaryDescription = "Plated grass eater";
		entityType.Description = "\n FEEDING CLASSIFICATION: Herbivore\n \n HEIGHT: up to 3m\n \n ANATOMY\n This bulky animal belongs to the widespread class of four-part symmetric animals. From the basic blueprint it has developed a pyramidal shape topped by a cluster of sensory organs.\n The combination of heavy plating and a sensitive warning system seems to protect the creature from most predators. It also appears to have a defensive mechanism against brushfires.\n \n BEHAVIOR\n The grazer processes vast amounts of plant material as it roams the grasslands.\n When the order was first discovered, only the egg stage of the creature's life cycle was known and the creature was thus named for the egg's appearance: The egg resembles a turnip, as it slowly emerges from the ground.\n \n SURVIVAL GUIDE NOTES\n The creature appears docile and will attempt to stay out of trouble";
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "turnip",
				GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]> { 
				{
					"normal",
					new GaitAnimationBracket[1]
					{
						new GaitAnimationBracket
						{
							MinimumSpeed = 10f,
							MaximumSpeed = 22f,
							StrideLength = 12f,
							StrideDuration = 0.8f,
							AnimationKey = "gaitWalk"
						}
					}
				} },
				AnimConditions = new AnimConditionInfo[2]
				{
					new AnimConditionInfo
					{
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Moving
						},
						GaitSetKey = "normal"
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "tentaclesLooking" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Idle
						}
					}
				}
			}
		};
		entityType.LocomotorType = new LocomotorType
		{
			MaxAngularSpeed = (float)Math.PI / 4f,
			FourSidedSymmetry = true,
			MeleeRadius = 20f,
			LeggedLocomotorType = new LeggedLocomotorType
			{
				TerrainNegateFactor = 0.8f,
				WalkSlowSpeed = 5f,
				WalkNormalSpeed = 7f,
				WalkFastSpeed = 12f
			},
			CollisionResponderType = new CollisionResponderType
			{
				AgentCollisionResponderType = new AgentCollisionResponderType()
			}
		};
		entityType.SensorType = new SensorType
		{
			Range = 350f,
			RangeAtNight = 200f,
			DetectionTypeKey = "defaultDetection"
		};
		entityType.BodyType = GameData.Instance.AllBodyTypes["turnip"];
		EntityType entityType7 = entityType;
		entityType7.IntelligenceType = new IntelligenceType
		{
			IsMobile = true,
			CanAttack = false,
			CanUseWeapons = false,
			CanHunt = false,
			CanScout = true,
			CanExamine = true,
			CanPatrol = true,
			CanHaul = false,
			IsPredator = false,
			StrengthRating = StrengthRating.VeryWeak,
			InterestInTriggerTypes = new string[2] { "mineTrigger", "spikeTrapTrigger" },
			MemoryInDays = 0.002f,
			AggroRange = 0f
		};
		entityType7.BiologicalType = new BiologicalType
		{
			OxygenAndMuscleEnergyIncreaseRatePerDay = 5f,
			TimeToConsumeFullMealInDays = 0.0125f,
			StomachSizeFractionOfEntityBulk = 0.3f,
			StomachContentsDecreaseRatePerDay = 2f,
			ActiveStealthRating = 0f,
			IsTerritorial = false,
			ResilienceMean = 1f,
			ResilienceStandardDeviation = 0.08f,
			MaxRegainLimit = 0.5f,
			FractionOfMaxHitpointsGainedPerDay = 0.3f,
			Carcass = "item:turnipCarcass",
			RaceTypes = new RaceType[2]
			{
				new RaceType
				{
					Name = "Pale Turnip",
					PortraitSkinType = "Pale",
					PrimaryColor = "F7F4C5".ToColorVector3(),
					ModelBasicTextureName = "TurnipPaleTexture",
					Edge = 0.3f,
					ModelScale = 2.5f
				},
				new RaceType
				{
					Name = "Copper Turnip",
					PortraitSkinType = "Dark",
					PrimaryColor = "F7F4C5".ToColorVector3(),
					ModelBasicTextureName = "TurnipDarkTexture",
					Edge = 0.4f,
					ModelScale = 2.5f
				}
			},
			Castes = new List<CasteType>
			{
				new CasteType
				{
					KeyName = "male",
					Reproduction = Reproduction.Male,
					Edge = 0.51f,
					HeightMean = 3.2f,
					HeightStandardDeviation = 0.1f,
					WeightMean = 490f,
					WeightStandardDeviation = 15f,
					AgeGroupTypes = new List<AgeGroupType>
					{
						new AgeGroupType
						{
							Name = "Baby",
							AIAgeGroup = AIAgeGroup.Baby,
							CanReproduce = false,
							Edge = 1.5f,
							HeightTargetModifier = 0.05f,
							WeightTargetModifier = 0.03f,
							ModelScaleFraction = 0.3f
						},
						new AgeGroupType
						{
							Name = "Child",
							AIAgeGroup = AIAgeGroup.Child,
							CanReproduce = false,
							Edge = 4f,
							HeightTargetModifier = 0.6f,
							WeightTargetModifier = 0.5f,
							ModelScaleFraction = 0.6f
						},
						new AgeGroupType
						{
							Name = "Young adult",
							AIAgeGroup = AIAgeGroup.YoungAdult,
							CanReproduce = false,
							Edge = 16f,
							HeightTargetModifier = 0.97f,
							WeightTargetModifier = 0.92f,
							ModelScaleFraction = 0.85f
						},
						new AgeGroupType
						{
							Name = "Adult",
							AIAgeGroup = AIAgeGroup.Adult,
							CanReproduce = true,
							Edge = 72f,
							HeightTargetModifier = 1f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f
						},
						new AgeGroupType
						{
							Name = "Old",
							AIAgeGroup = AIAgeGroup.Old,
							CanReproduce = true,
							Edge = 80f,
							HeightTargetModifier = 0.9f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f
						}
					}
				},
				new CasteType
				{
					KeyName = "female",
					Reproduction = Reproduction.Female,
					Edge = 1f,
					HeightMean = 2.9f,
					HeightStandardDeviation = 0.1f,
					WeightMean = 390f,
					WeightStandardDeviation = 10f,
					AgeGroupTypes = new List<AgeGroupType>
					{
						new AgeGroupType
						{
							Name = "Baby",
							AIAgeGroup = AIAgeGroup.Baby,
							CanReproduce = false,
							Edge = 1.5f,
							HeightTargetModifier = 0.05f,
							WeightTargetModifier = 0.03f,
							ModelScaleFraction = 0.3f
						},
						new AgeGroupType
						{
							Name = "Child",
							AIAgeGroup = AIAgeGroup.Child,
							CanReproduce = false,
							Edge = 4f,
							HeightTargetModifier = 0.6f,
							WeightTargetModifier = 0.5f,
							ModelScaleFraction = 0.6f
						},
						new AgeGroupType
						{
							Name = "Young adult",
							AIAgeGroup = AIAgeGroup.YoungAdult,
							CanReproduce = false,
							Edge = 16f,
							HeightTargetModifier = 0.97f,
							WeightTargetModifier = 0.92f,
							ModelScaleFraction = 0.85f
						},
						new AgeGroupType
						{
							Name = "Adult",
							AIAgeGroup = AIAgeGroup.Adult,
							CanReproduce = true,
							Edge = 72f,
							HeightTargetModifier = 1f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f
						},
						new AgeGroupType
						{
							Name = "Old",
							AIAgeGroup = AIAgeGroup.Old,
							CanReproduce = false,
							Edge = 80f,
							HeightTargetModifier = 0.9f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f
						}
					}
				}
			}
		};
		listOfEntityTypes.Add(entityType7);
		entityType = new EntityType("entity:bird");
		entityType.Name = "Diamond bird";
		entityType.ThumbnailSmall = "HUD_thumbnail_diamondBird";
		entityType.IsFlyer = true;
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "bird",
				AnimConditions = new AnimConditionInfo[2]
				{
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "walk" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Moving
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "idle" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Idle
						}
					}
				}
			}
		};
		entityType.LocomotorType = new LocomotorType
		{
			MaxAngularSpeed = (float)Math.PI / 4f,
			FourSidedSymmetry = true,
			MeleeRadius = 20f,
			LeggedLocomotorType = new LeggedLocomotorType
			{
				TerrainNegateFactor = 1f,
				WalkSlowSpeed = 10f,
				WalkNormalSpeed = 25f,
				WalkFastSpeed = 24f
			},
			CollisionResponderType = new CollisionResponderType
			{
				AgentCollisionResponderType = new AgentCollisionResponderType()
			}
		};
		entityType.SensorType = new SensorType
		{
			Range = 100f,
			RangeAtNight = 50f,
			DetectionTypeKey = "defaultDetection"
		};
		entityType.BodyType = GameData.Instance.AllBodyTypes["bird"];
		EntityType entityType8 = entityType;
		entityType8.IntelligenceType = new IntelligenceType
		{
			IsMobile = false,
			StrengthRating = StrengthRating.VeryWeak,
			AggroRange = 0f,
			MembersScoutingFraction = 0f,
			ForageAndHuntingRadius = 0
		};
		entityType8.BiologicalType = new BiologicalType
		{
			OxygenAndMuscleEnergyIncreaseRatePerDay = 10f,
			TimeToConsumeFullMealInDays = 0.0125f,
			StomachSizeFractionOfEntityBulk = 0.1f,
			StomachContentsDecreaseRatePerDay = 2f,
			ActiveStealthRating = 0.3f,
			MaxRegainLimit = 0.5f,
			FractionOfMaxHitpointsGainedPerDay = 0.3f,
			Carcass = "item:birdCarcass",
			RaceTypes = new RaceType[8]
			{
				new RaceType
				{
					KeyName = "pale",
					Name = "Pale race",
					PortraitSkinType = "Pale",
					PrimaryColor = "F7F4C5".ToColorVector3(),
					ModelBasicTextureName = "BirdPaleTexture",
					Edge = 0.3f,
					ModelScale = 2.1f
				},
				new RaceType
				{
					KeyName = "dark",
					Name = "Dark race",
					PortraitSkinType = "Dark",
					PrimaryColor = "564920".ToColorVector3(),
					ModelBasicTextureName = "BirdDarkTexture",
					Edge = 0.6f,
					ModelScale = 2.1f
				},
				new RaceType
				{
					KeyName = "yellow",
					Name = "Yellow race",
					PortraitSkinType = "Yellow",
					PrimaryColor = "564920".ToColorVector3(),
					ModelBasicTextureName = "BirdYellowTexture",
					Edge = 0.6f,
					ModelScale = 2.1f
				},
				new RaceType
				{
					KeyName = "red",
					Name = "Red race",
					PortraitSkinType = "Red",
					PrimaryColor = "564920".ToColorVector3(),
					ModelBasicTextureName = "BirdRedTexture",
					Edge = 0.6f,
					ModelScale = 2.1f
				},
				new RaceType
				{
					KeyName = "black",
					Name = "Black race",
					PortraitSkinType = "Black",
					PrimaryColor = "3F3411".ToColorVector3(),
					ModelBasicTextureName = "BirdBlackTexture",
					Edge = 0.9f,
					ModelScale = 2.1f
				},
				new RaceType
				{
					KeyName = "purple",
					Name = "Purple race",
					PortraitSkinType = "Purple",
					PrimaryColor = "3F3411".ToColorVector3(),
					ModelBasicTextureName = "BirdPurpleTexture",
					Edge = 0.9f,
					ModelScale = 2.1f
				},
				new RaceType
				{
					KeyName = "veryDarkTurqoise",
					Name = "Cave race",
					PortraitSkinType = "Very Dark Turqoise",
					PrimaryColor = "3F3411".ToColorVector3(),
					ModelBasicTextureName = "BirdVeryDarkTurqoiseTexture",
					Edge = 0.9f,
					ModelScale = 0.8f
				},
				new RaceType
				{
					KeyName = "veryDarkGreen",
					Name = "Cave race",
					PortraitSkinType = "Very Dark Green",
					PrimaryColor = "3F3411".ToColorVector3(),
					ModelBasicTextureName = "BirdVeryDarkGreenTexture",
					Edge = 0.9f,
					ModelScale = 1.1f
				}
			},
			Castes = new List<CasteType>
			{
				new CasteType
				{
					KeyName = "male",
					Reproduction = Reproduction.Male,
					Edge = 0.51f,
					HeightMean = 0.5f,
					HeightStandardDeviation = 0.02f,
					WeightMean = 12f,
					WeightStandardDeviation = 0.15f,
					AgeGroupTypes = new List<AgeGroupType>
					{
						new AgeGroupType
						{
							Name = "Baby",
							AIAgeGroup = AIAgeGroup.Baby,
							CanReproduce = false,
							Edge = 0.5f,
							HeightTargetModifier = 0.05f,
							WeightTargetModifier = 0.03f,
							ModelScaleFraction = 0.75f
						},
						new AgeGroupType
						{
							Name = "Child",
							AIAgeGroup = AIAgeGroup.Child,
							CanReproduce = false,
							Edge = 1f,
							HeightTargetModifier = 0.6f,
							WeightTargetModifier = 0.5f,
							ModelScaleFraction = 0.82f
						},
						new AgeGroupType
						{
							Name = "Young adult",
							AIAgeGroup = AIAgeGroup.YoungAdult,
							CanReproduce = false,
							Edge = 3f,
							HeightTargetModifier = 0.97f,
							WeightTargetModifier = 0.92f,
							ModelScaleFraction = 0.9f
						},
						new AgeGroupType
						{
							Name = "Adult",
							AIAgeGroup = AIAgeGroup.Adult,
							CanReproduce = true,
							Edge = 18f,
							HeightTargetModifier = 1f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f
						},
						new AgeGroupType
						{
							Name = "Old",
							AIAgeGroup = AIAgeGroup.Old,
							CanReproduce = true,
							Edge = 20f,
							HeightTargetModifier = 0.9f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f
						}
					}
				},
				new CasteType
				{
					KeyName = "female",
					Reproduction = Reproduction.Female,
					Edge = 1f,
					HeightMean = 0.5f,
					HeightStandardDeviation = 0.02f,
					WeightMean = 12f,
					WeightStandardDeviation = 0.15f,
					AgeGroupTypes = new List<AgeGroupType>
					{
						new AgeGroupType
						{
							Name = "Baby",
							AIAgeGroup = AIAgeGroup.Baby,
							CanReproduce = false,
							Edge = 0.5f,
							HeightTargetModifier = 0.05f,
							WeightTargetModifier = 0.03f,
							ModelScaleFraction = 0.75f
						},
						new AgeGroupType
						{
							Name = "Child",
							AIAgeGroup = AIAgeGroup.Child,
							CanReproduce = false,
							Edge = 1f,
							HeightTargetModifier = 0.6f,
							WeightTargetModifier = 0.5f,
							ModelScaleFraction = 0.82f
						},
						new AgeGroupType
						{
							Name = "Young adult",
							AIAgeGroup = AIAgeGroup.YoungAdult,
							CanReproduce = false,
							Edge = 3f,
							HeightTargetModifier = 0.97f,
							WeightTargetModifier = 0.92f,
							ModelScaleFraction = 0.9f
						},
						new AgeGroupType
						{
							Name = "Adult",
							AIAgeGroup = AIAgeGroup.Adult,
							CanReproduce = true,
							Edge = 19f,
							HeightTargetModifier = 1f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f
						},
						new AgeGroupType
						{
							Name = "Old",
							AIAgeGroup = AIAgeGroup.Old,
							CanReproduce = false,
							Edge = 22f,
							HeightTargetModifier = 0.9f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f
						}
					}
				}
			}
		};
		listOfEntityTypes.Add(entityType8);
		needType = new NeedType();
		needType.KeyName = "protein";
		needType.FoodNeedType = new FoodNeedType
		{
			FoodNutrient = "protein",
			RequiredNutrientsAsFractionOfEntityBulk = 0.03f
		};
		needType.DecreasePerDay = new NormalDistribution
		{
			Mean = 3.0,
			StandardDeviation = 0.019999999552965164
		};
		needType.LimitForDecreasedEnergy = 0.05f;
		needType.DecreasedEnergyWeight = 0.08f;
		needType.PhysicalEffects = new PhysicalEffects
		{
			DaysAtZeroCausingCollapse = 2f,
			DaysAtZeroCausingDeath = 2.1f,
			DaysAtZeroDecreaseFactor = 1f,
			LimitForReducedGrowth = 0.1f,
			LimitForIncreasedSickness = 0.05f,
			UseExertionFactorToDecrease = false
		};
		NeedType needType5 = needType;
		BodyType bodyType = GameData.Instance.AllBodyTypes["twinkler"];
		entityType = new EntityType("entity:twinkler");
		entityType.Name = "Twinkler";
		entityType.ThumbnailSmall = "HUD_thumbnail_twinkler";
		entityType.SummaryDescription = "Dangerous carnivore";
		entityType.Description = "\n FEEDING CLASSIFICATION: Carnivorous predator\n \n HEIGHT: Up to 1 m\n \n ANATOMY\n Species of quadites named for their nightly display of blinking light signals, believed to be used for communication with other members of the hive.\n Mouth opening is situated on the bottom of the body, the digestive canals extending radially from the center. On the top are bioluminescent communication organs as well as the sensory receptors.\n Covered in a strong exoskeleton. All four legs are capable of stabbing and injecting venom.\n \n BEHAVIOR\n As is the case with the other Quadite species, the Twinkler quadite lives in colonies. We have reports of colonies that hold up to a hundred individuals.\n They emerge from their burrows to hunt in packs, behaving much like wolves. Primary food source is the thunder chicken, but their diet appears to be varied.\n \n SENSES\n We believe they locate prey through vision and smell. Senses are highly developed, especially the eyes, which are able to perceive multispectral images as well as polarized light, an ability which probably aids the animal with hitting its prey.\n Infrared vision  allows them to hunt very efficiently at night while maintaining constant communication.\n \n SURVIVAL GUIDE NOTES\n Threat factor ranges from Medium to High. Humans are sometimes seen as prey and the speed and numbers of twinklers make them able to overrun us. Their hard shell and sharp talons present a demanding challenge in close combat and it is therefore recommended that they be dispatched from a distance with ranged weapons.\n \n COMPETITORS\n Twinklers are apex predators (meaning at the top of their food chain) and in some territories compete with the Black Patrician, another apex predator.";
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "twinkler",
				GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]> { 
				{
					"normal",
					new GaitAnimationBracket[1]
					{
						new GaitAnimationBracket
						{
							MinimumSpeed = 35f,
							MaximumSpeed = 77f,
							StrideLength = 27f,
							StrideDuration = 0.9f,
							AnimationKey = "gaitWalk"
						}
					}
				} },
				AnimConditions = new AnimConditionInfo[7]
				{
					new AnimConditionInfo
					{
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Moving
						},
						GaitSetKey = "normal"
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "idle" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Idle
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "attackLowRight" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Attacking,
							Modifiers = new BitMask64(typeof(AnimModifier), 21, 1)
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "attackHighRight" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Attacking,
							Modifiers = new BitMask64(typeof(AnimModifier), 20, 1)
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[2] { "hit", "hit" },
							Sounds = new string[2] { "aliens/hummingClickClacking", "aliens/hummingClickClacking" }
						},
						Playback = Playback.Manual,
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Recoiling
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "dying" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "dead" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying,
							Modifiers = new BitMask64(typeof(AnimModifier), 13)
						}
					}
				}
			}
		};
		entityType.LocomotorType = new LocomotorType
		{
			MaxAngularSpeed = (float)Math.PI / 4f,
			FourSidedSymmetry = true,
			MeleeRadius = 12f,
			LeggedLocomotorType = new LeggedLocomotorType
			{
				TerrainNegateFactor = 0.3f,
				WalkSlowSpeed = 20f,
				WalkNormalSpeed = 22f,
				WalkFastSpeed = 24f
			},
			CollisionResponderType = new CollisionResponderType
			{
				AgentCollisionResponderType = new AgentCollisionResponderType()
			}
		};
		entityType.SensorType = new SensorType
		{
			Range = 1000f,
			RangeAtNight = 1000f,
			DetectionTypeKey = "defaultDetection"
		};
		entityType.BodyType = bodyType;
		entityType.ContainerType = new AgentStorageType
		{
			ItemStorageType = new ItemStorageType(0.5f),
			StomachStorageType = new ItemStorageType(0.07f)
		};
		EntityType entityType9 = entityType;
		entityType9.IntelligenceType = new IntelligenceType
		{
			MembersScoutingFraction = 1f,
			IsMobile = true,
			CanAttack = true,
			CanUseWeapons = false,
			CanHunt = true,
			CanScout = true,
			CanExamine = true,
			CanPatrol = true,
			CanHaul = false,
			IsPredator = true,
			WillAttackNonThreatsNearby = true,
			ContainerTransactTag = "twinklerTransact",
			InterestInTriggerTypes = new string[3] { "mineTrigger", "spikeTrapTrigger", "smallImprovisedTrapTrigger" },
			StrengthRating = StrengthRating.WeakerThanHumans,
			Boldness = 0.25f,
			Courage = 0.05f,
			MemoryInDays = 2f,
			Prey = new string[5] { "entity:mudWorm", "entity:binalRat", "entity:whiteThunderChicken", "entity:pygmyThunderChicken", "entity:human" },
			Attacks = new string[2] { "twinklerLowRight", "twinklerHighRight" },
			Skills = new SerializableDictionary<string, float> { { "unarmedFighting", 0.2f } },
			AggroRange = 450f,
			AssistanceRange = 600f
		};
		entityType9.BiologicalType = new BiologicalType
		{
			OrderKey = "quaditeOrder",
			OxygenAndMuscleEnergyIncreaseRatePerDay = 12f,
			FoodItemTagsThatCanBeConsumed = new string[6] { "cookedMeat", "rawMeat", "smallRawMeat", "spoiledMeal", "inedibleMeat", "rottenMeat" },
			ExtractionProcessTypes = new string[13]
			{
				"extractMudWormMeat", "extractPatricianMeat", "extractLeafCutterMeat", "extractThunderChickenMeat", "extractBinalRatMeat", "extractTwinklerMeat", "extractBushDragonMeat", "extractSwampDemonTreeMeat", "extractDemonTreeMeat", "extractMegapodMeat",
				"extractWhipjawMeat", "extractSpikePlantMeat", "extractForestGuardianMeat"
			},
			TimeToConsumeFullMealInDays = 0.005f,
			StomachSizeFractionOfEntityBulk = 0.3f,
			StomachContentsDecreaseRatePerDay = 3f,
			ActiveStealthRating = 0.2f,
			MaxRegainLimit = 0.5f,
			FractionOfMaxHitpointsGainedPerDay = 0.3f,
			Carcass = "item:quaditeCarcass",
			ResilienceMean = 7f,
			ResilienceStandardDeviation = 0.08f,
			BioPropertyTypes = new BioPropertyType[4]
			{
				new BioPropertyType
				{
					KeyName = "SensorRange",
					InterpolateSetting = BioPropertyType.Interpolate.DefaultPriority
				},
				new BioPropertyType
				{
					KeyName = "SensorRangeAtNight",
					InterpolateSetting = BioPropertyType.Interpolate.DefaultPriority
				},
				new BioPropertyType
				{
					KeyName = "AggroRange",
					InterpolateSetting = BioPropertyType.Interpolate.DefaultPriority
				},
				new BioPropertyType
				{
					KeyName = "AssistanceRange",
					InterpolateSetting = BioPropertyType.Interpolate.DefaultPriority
				}
			},
			Castes = new List<CasteType>
			{
				new CasteType
				{
					KeyName = "hunter",
					PrimaryColor = "F7F4C5".ToColorVector3(),
					ModelBasicTextureName = "QuaditeYellowTexture",
					Edge = 0.3f,
					ModelScale = 2.5f,
					ModelName = "twinkler",
					BioProperties = new SerializableDictionary<string, BioProperty>
					{
						{
							"SensorRange",
							new BioProperty
							{
								NumberValue = 1000f
							}
						},
						{
							"SensorRangeAtNight",
							new BioProperty
							{
								NumberValue = 1000f
							}
						},
						{
							"AggroRange",
							new BioProperty
							{
								NumberValue = 950f
							}
						},
						{
							"AssistanceRange",
							new BioProperty
							{
								NumberValue = 1200f
							}
						}
					},
					Reproduction = Reproduction.None,
					HeightMean = 0.6f,
					HeightStandardDeviation = 0.02f,
					WeightMean = 30f,
					WeightStandardDeviation = 3f,
					AgeGroupTypes = new List<AgeGroupType>
					{
						new AgeGroupType
						{
							Name = "Hatchling",
							AIAgeGroup = AIAgeGroup.Baby,
							CanReproduce = false,
							Edge = 1.5f,
							HeightTargetModifier = 0.05f,
							WeightTargetModifier = 0.03f,
							ModelScaleFraction = 0.2f
						},
						new AgeGroupType
						{
							Name = "Hatchling",
							AIAgeGroup = AIAgeGroup.Child,
							CanReproduce = false,
							Edge = 11f,
							HeightTargetModifier = 0.6f,
							WeightTargetModifier = 0.5f,
							ModelScaleFraction = 0.6f
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.YoungAdult,
							CanReproduce = false,
							Edge = 16f,
							HeightTargetModifier = 0.97f,
							WeightTargetModifier = 0.92f,
							ModelScaleFraction = 0.9f,
							NeedTypes = new NeedType[1] { needType5 }
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.Adult,
							CanReproduce = true,
							Edge = 72f,
							HeightTargetModifier = 1f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f,
							NeedTypes = new NeedType[1] { needType5 }
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.Old,
							CanReproduce = true,
							Edge = 200f,
							HeightTargetModifier = 0.9f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 0.95f,
							NeedTypes = new NeedType[1] { needType5 }
						}
					}
				},
				new CasteType
				{
					KeyName = "guard",
					Reproduction = Reproduction.None,
					HeightMean = 0.7f,
					HeightStandardDeviation = 0.02f,
					WeightMean = 25f,
					WeightStandardDeviation = 2f,
					PrimaryColor = "564920".ToColorVector3(),
					ModelBasicTextureName = "QuaditeRedTexture",
					Edge = 0.6f,
					ModelScale = 2.1f,
					BioProperties = new SerializableDictionary<string, BioProperty>
					{
						{
							"SensorRange",
							new BioProperty
							{
								NumberValue = 576f
							}
						},
						{
							"SensorRangeAtNight",
							new BioProperty
							{
								NumberValue = 576f
							}
						},
						{
							"AggroRange",
							new BioProperty
							{
								NumberValue = 210f
							}
						},
						{
							"AssistanceRange",
							new BioProperty
							{
								NumberValue = 300f
							}
						}
					},
					AgeGroupTypes = new List<AgeGroupType>
					{
						new AgeGroupType
						{
							Name = "Hatchling",
							AIAgeGroup = AIAgeGroup.Baby,
							CanReproduce = false,
							Edge = 1.5f,
							HeightTargetModifier = 0.05f,
							WeightTargetModifier = 0.03f
						},
						new AgeGroupType
						{
							Name = "Hatchling",
							AIAgeGroup = AIAgeGroup.Child,
							CanReproduce = false,
							Edge = 11f,
							HeightTargetModifier = 0.6f,
							WeightTargetModifier = 0.5f
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.YoungAdult,
							CanReproduce = false,
							Edge = 16f,
							HeightTargetModifier = 0.97f,
							WeightTargetModifier = 0.92f,
							NeedTypes = new NeedType[1] { needType5 }
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.Adult,
							CanReproduce = true,
							Edge = 72f,
							HeightTargetModifier = 1f,
							WeightTargetModifier = 1f,
							NeedTypes = new NeedType[1] { needType5 }
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.Old,
							CanReproduce = false,
							Edge = 200f,
							HeightTargetModifier = 0.9f,
							WeightTargetModifier = 1f,
							NeedTypes = new NeedType[1] { needType5 }
						}
					}
				}
			}
		};
		listOfEntityTypes.Add(entityType9);
		bodyType = GameData.Instance.AllBodyTypes["twinkler"];
		entityType = new EntityType("entity:swarmer");
		entityType.Name = "Swarmer";
		entityType.ThumbnailSmall = "HUD_thumbnail_twinkler";
		entityType.SummaryDescription = "One of the most dangerous quadite species, the swarmer is extremely fast and aggressive.";
		entityType.Description = "\n FEEDING CLASSIFICATION: Carnivore.\n \n SIZE: 60-100 cm\n \n ANATOMY\n A fast and agile quadite protected by a strong, spiked exoskeleton. Armed with sharp front limbs which have high penetration power. \n \n BEHAVIOR\n This predator lives in underground colonies near the muckroot biome and anyone that enters its territory will soon find themselves attacked by a group of swarmers.\n \n THREAT LEVEL\n Very high.\n \n NOTES\n In its habitat, extreme caution is advised - sentry turrets and sensors should be set up to secure any kind of camp.";
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "twinklerThinSpiky",
				GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]> { 
				{
					"normal",
					new GaitAnimationBracket[1]
					{
						new GaitAnimationBracket
						{
							MinimumSpeed = 65f,
							MaximumSpeed = 150f,
							StrideLength = 21f,
							StrideDuration = 0.9f,
							AnimationKey = "gaitWalk"
						}
					}
				} },
				AnimConditions = new AnimConditionInfo[7]
				{
					new AnimConditionInfo
					{
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Moving
						},
						GaitSetKey = "normal"
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "idle" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Idle
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "attackLowRight" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Attacking,
							Modifiers = new BitMask64(typeof(AnimModifier), 21, 1)
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "attackHighRight" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Attacking,
							Modifiers = new BitMask64(typeof(AnimModifier), 20, 1)
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "hit" },
							Sounds = new string[1] { "aliens/hummingClickClacking" }
						},
						Playback = Playback.Manual,
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Recoiling
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "dying" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "dead" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying,
							Modifiers = new BitMask64(typeof(AnimModifier), 13)
						}
					}
				}
			}
		};
		entityType.LocomotorType = new LocomotorType
		{
			MaxAngularSpeed = (float)Math.PI / 2f,
			FourSidedSymmetry = true,
			MeleeRadius = 12f,
			LeggedLocomotorType = new LeggedLocomotorType
			{
				TerrainNegateFactor = 0.3f,
				WalkSlowSpeed = 50f,
				WalkNormalSpeed = 70f,
				WalkFastSpeed = 100f
			},
			CollisionResponderType = new CollisionResponderType
			{
				AgentCollisionResponderType = new AgentCollisionResponderType()
			}
		};
		entityType.SensorType = new SensorType
		{
			Range = 1000f,
			RangeAtNight = 1000f,
			DetectionTypeKey = "defaultDetection"
		};
		entityType.BodyType = bodyType;
		entityType.ContainerType = new AgentStorageType
		{
			ItemStorageType = new ItemStorageType(0.5f),
			StomachStorageType = new ItemStorageType(0.07f)
		};
		EntityType entityType10 = entityType;
		entityType10.IntelligenceType = new IntelligenceType
		{
			MembersScoutingFraction = 1f,
			IsMobile = true,
			CanAttack = true,
			CanUseWeapons = false,
			CanHunt = true,
			CanScout = true,
			CanExamine = true,
			CanPatrol = true,
			CanHaul = false,
			IsPredator = true,
			WillAttackNonThreatsNearby = true,
			ContainerTransactTag = "twinklerTransact",
			InterestInTriggerTypes = new string[3] { "mineTrigger", "spikeTrapTrigger", "smallImprovisedTrapTrigger" },
			StrengthRating = StrengthRating.LikeHumans,
			Boldness = 0.25f,
			Courage = 0.05f,
			MemoryInDays = 2f,
			Prey = new string[6] { "entity:mudWorm", "entity:binalRat", "entity:whiteThunderChicken", "entity:pygmyThunderChicken", "entity:bajingan", "entity:human" },
			Attacks = new string[2] { "twinklerLowRight", "twinklerHighRight" },
			Skills = new SerializableDictionary<string, float> { { "unarmedFighting", 0.2f } },
			AggroRange = 450f,
			AssistanceRange = 600f
		};
		entityType10.BiologicalType = new BiologicalType
		{
			OrderKey = "quaditeOrder",
			OxygenAndMuscleEnergyIncreaseRatePerDay = 12f,
			FoodItemTagsThatCanBeConsumed = new string[6] { "cookedMeat", "rawMeat", "smallRawMeat", "spoiledMeal", "inedibleMeat", "rottenMeat" },
			ExtractionProcessTypes = new string[12]
			{
				"extractMudWormMeat", "extractPatricianMeat", "extractThunderChickenMeat", "extractBinalRatMeat", "extractTwinklerMeat", "extractBushDragonMeat", "extractSwampDemonTreeMeat", "extractDemonTreeMeat", "extractMegapodMeat", "extractWhipjawMeat",
				"extractSpikePlantMeat", "extractForestGuardianMeat"
			},
			TimeToConsumeFullMealInDays = 0.005f,
			StomachSizeFractionOfEntityBulk = 0.3f,
			StomachContentsDecreaseRatePerDay = 3f,
			ActiveStealthRating = 0.2f,
			MaxRegainLimit = 0.5f,
			FractionOfMaxHitpointsGainedPerDay = 0.3f,
			Carcass = "item:swarmerCarcass",
			ResilienceMean = 7f,
			ResilienceStandardDeviation = 0.08f,
			BioPropertyTypes = new BioPropertyType[4]
			{
				new BioPropertyType
				{
					KeyName = "SensorRange",
					InterpolateSetting = BioPropertyType.Interpolate.DefaultPriority
				},
				new BioPropertyType
				{
					KeyName = "SensorRangeAtNight",
					InterpolateSetting = BioPropertyType.Interpolate.DefaultPriority
				},
				new BioPropertyType
				{
					KeyName = "AggroRange",
					InterpolateSetting = BioPropertyType.Interpolate.DefaultPriority
				},
				new BioPropertyType
				{
					KeyName = "AssistanceRange",
					InterpolateSetting = BioPropertyType.Interpolate.DefaultPriority
				}
			},
			Castes = new List<CasteType>
			{
				new CasteType
				{
					KeyName = "guard",
					Reproduction = Reproduction.None,
					HeightMean = 0.7f,
					HeightStandardDeviation = 0.02f,
					WeightMean = 25f,
					WeightStandardDeviation = 2f,
					PrimaryColor = "564920".ToColorVector3(),
					ModelBasicTextureName = "QuaditeThinSpikyTexture",
					Edge = 0.6f,
					ModelScale = 2.1f,
					BioProperties = new SerializableDictionary<string, BioProperty>
					{
						{
							"SensorRange",
							new BioProperty
							{
								NumberValue = 576f
							}
						},
						{
							"SensorRangeAtNight",
							new BioProperty
							{
								NumberValue = 576f
							}
						},
						{
							"AggroRange",
							new BioProperty
							{
								NumberValue = 210f
							}
						},
						{
							"AssistanceRange",
							new BioProperty
							{
								NumberValue = 300f
							}
						}
					},
					AgeGroupTypes = new List<AgeGroupType>
					{
						new AgeGroupType
						{
							Name = "Hatchling",
							AIAgeGroup = AIAgeGroup.Baby,
							CanReproduce = false,
							Edge = 1.5f,
							HeightTargetModifier = 0.05f,
							WeightTargetModifier = 0.03f
						},
						new AgeGroupType
						{
							Name = "Hatchling",
							AIAgeGroup = AIAgeGroup.Child,
							CanReproduce = false,
							Edge = 11f,
							HeightTargetModifier = 0.6f,
							WeightTargetModifier = 0.5f
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.YoungAdult,
							CanReproduce = false,
							Edge = 16f,
							HeightTargetModifier = 0.97f,
							WeightTargetModifier = 0.92f,
							NeedTypes = new NeedType[1] { needType5 }
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.Adult,
							CanReproduce = true,
							Edge = 72f,
							HeightTargetModifier = 1f,
							WeightTargetModifier = 1f,
							NeedTypes = new NeedType[1] { needType5 }
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.Old,
							CanReproduce = false,
							Edge = 200f,
							HeightTargetModifier = 0.9f,
							WeightTargetModifier = 1f,
							NeedTypes = new NeedType[1] { needType5 }
						}
					}
				}
			}
		};
		listOfEntityTypes.Add(entityType10);
		needType = new NeedType();
		needType.KeyName = "foodEnergy";
		needType.FoodNeedType = new FoodNeedType
		{
			FoodNutrient = "foodEnergy",
			RequiredNutrientsAsFractionOfEntityBulk = 0.1f
		};
		needType.DecreasePerDay = new NormalDistribution
		{
			Mean = 1.0,
			StandardDeviation = 0.019999999552965164
		};
		needType.LimitForDecreasedEnergy = 0.1f;
		needType.DecreasedEnergyWeight = 0.6f;
		needType.PhysicalEffects = new PhysicalEffects
		{
			DaysAtZeroCausingCollapse = 4f,
			DaysAtZeroCausingDeath = 4f,
			DaysAtZeroDecreaseFactor = 1f,
			UseExertionFactorToDecrease = true
		};
		NeedType needType6 = needType;
		bodyType = GameData.Instance.AllBodyTypes["twinkler"];
		entityType = new EntityType("entity:fieldQuadite");
		entityType.Name = "Field quadite";
		entityType.ThumbnailSmall = "HUD_thumbnail_twinklerWhiteStar";
		entityType.SummaryDescription = "Ravenous plant eater. Vermin.";
		entityType.Description = "\n FEEDING CLASSIFICATION: Herbivore.\n \n SIZE: 10-40 cm\n \n ANATOMY\n A smaller quadite species specialized in collecting plant food for its colony. Equipped with sharp scissor-like appendages that are able to cut through tough plant material such as spoak leaves.\n \n BEHAVIOR\n Lives in underground colonies built underneath the firegrass plains. The tunneling is done by a builder caste which is rarely seen. The worker caste, however, is easy to notice as it scours the landscape in a constant search for nutrient-rich plant parts.\n \n THREAT LEVEL\n None. Will rather flee than defend itself if threatened.\n \n NOTES\n In its habitat, vegetable food stores have to be protected thoroughly. The field quadite is able to gain entry to any building made from spoak leaves or other soft material.";
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				ModelScale = 1.25f,
				AssetName = "twinklerThin",
				GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]> { 
				{
					"normal",
					new GaitAnimationBracket[1]
					{
						new GaitAnimationBracket
						{
							MinimumSpeed = 35f,
							MaximumSpeed = 77f,
							StrideLength = 14f,
							StrideDuration = 0.9f,
							AnimationKey = "gaitWalk"
						}
					}
				} },
				AnimConditions = new AnimConditionInfo[7]
				{
					new AnimConditionInfo
					{
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Moving
						},
						GaitSetKey = "normal"
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "idle" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Idle
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "attackLowRight" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Attacking,
							Modifiers = new BitMask64(typeof(AnimModifier), 21, 1)
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "attackHighRight" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Attacking,
							Modifiers = new BitMask64(typeof(AnimModifier), 20, 1)
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "hit" },
							Sounds = new string[1] { "aliens/hummingClickClacking" }
						},
						Playback = Playback.Manual,
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Recoiling
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "dying" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "dead" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying,
							Modifiers = new BitMask64(typeof(AnimModifier), 13)
						}
					}
				}
			}
		};
		entityType.LocomotorType = new LocomotorType
		{
			MaxAngularSpeed = (float)Math.PI / 4f,
			FourSidedSymmetry = true,
			MeleeRadius = 12f,
			LeggedLocomotorType = new LeggedLocomotorType
			{
				TerrainNegateFactor = 0.3f,
				WalkSlowSpeed = 25f,
				WalkNormalSpeed = 35f,
				WalkFastSpeed = 50f
			},
			CollisionResponderType = new CollisionResponderType
			{
				AgentCollisionResponderType = new AgentCollisionResponderType()
			}
		};
		entityType.SensorType = new SensorType
		{
			Range = 360f,
			RangeAtNight = 360f,
			DetectionTypeKey = "defaultDetection"
		};
		entityType.BodyType = bodyType;
		entityType.ContainerType = new AgentStorageType
		{
			ItemStorageType = new ItemStorageType(0.5f),
			StomachStorageType = new ItemStorageType(0.07f)
		};
		EntityType entityType11 = entityType;
		entityType11.IntelligenceType = new IntelligenceType
		{
			MembersScoutingFraction = 1f,
			IsMobile = true,
			CanAttack = false,
			CanUseWeapons = false,
			CanHunt = false,
			CanScout = true,
			CanExamine = true,
			CanPatrol = true,
			CanHaul = false,
			IsPredator = false,
			ContainerTransactTag = "leafcutterTransact",
			InterestInTriggerTypes = new string[3] { "mineTrigger", "spikeTrapTrigger", "smallImprovisedTrapTrigger" },
			StrengthRating = StrengthRating.None,
			Courage = 0f,
			MemoryInDays = 3f,
			Boldness = 1.49f,
			AggroRange = 0f,
			AssistanceRange = 600f
		};
		entityType11.BiologicalType = new BiologicalType
		{
			OrderKey = "quaditeOrder",
			OxygenAndMuscleEnergyIncreaseRatePerDay = 12f,
			FoodItemTagsThatCanBeConsumed = new string[2] { "edibleVegi", "inedibleVegi" },
			TimeToConsumeFullMealInDays = 0.005f,
			StomachSizeFractionOfEntityBulk = 0.45f,
			StomachContentsDecreaseRatePerDay = 3f,
			ActiveStealthRating = 0.2f,
			MaxRegainLimit = 0.5f,
			FractionOfMaxHitpointsGainedPerDay = 0.3f,
			Carcass = "item:leafcutterCarcass",
			ResilienceMean = 7f,
			ResilienceStandardDeviation = 0.08f,
			IsVermin = true,
			BioPropertyTypes = new BioPropertyType[5]
			{
				new BioPropertyType
				{
					KeyName = "SensorRange",
					InterpolateSetting = BioPropertyType.Interpolate.DefaultPriority
				},
				new BioPropertyType
				{
					KeyName = "SensorRangeAtNight",
					InterpolateSetting = BioPropertyType.Interpolate.DefaultPriority
				},
				new BioPropertyType
				{
					KeyName = "AggroRange",
					InterpolateSetting = BioPropertyType.Interpolate.DefaultPriority
				},
				new BioPropertyType
				{
					KeyName = "AssistanceRange",
					InterpolateSetting = BioPropertyType.Interpolate.DefaultPriority
				},
				new BioPropertyType
				{
					KeyName = "IsVermin",
					InterpolateSetting = BioPropertyType.Interpolate.DefaultPriority
				}
			},
			Castes = new List<CasteType>
			{
				new CasteType
				{
					KeyName = "Scavenger",
					PrimaryColor = "F7F4C5".ToColorVector3(),
					ModelBasicTextureName = "QuaditeThinTexture",
					Edge = 0.3f,
					ModelName = "twinklerThin",
					BioProperties = new SerializableDictionary<string, BioProperty>
					{
						{
							"SensorRange",
							new BioProperty
							{
								NumberValue = 360f
							}
						},
						{
							"SensorRangeAtNight",
							new BioProperty
							{
								NumberValue = 360f
							}
						},
						{
							"AggroRange",
							new BioProperty
							{
								NumberValue = 0f
							}
						},
						{
							"AssistanceRange",
							new BioProperty
							{
								NumberValue = 1200f
							}
						},
						{
							"IsVermin",
							new BioProperty
							{
								BoolValue = true
							}
						}
					},
					Reproduction = Reproduction.None,
					HeightMean = 0.6f,
					HeightStandardDeviation = 0.02f,
					WeightMean = 20f,
					WeightStandardDeviation = 3f,
					AgeGroupTypes = new List<AgeGroupType>
					{
						new AgeGroupType
						{
							Name = "Hatchling",
							AIAgeGroup = AIAgeGroup.Baby,
							CanReproduce = false,
							Edge = 0.4f,
							HeightTargetModifier = 0.05f,
							WeightTargetModifier = 0.03f,
							ModelScaleFraction = 0.2f
						},
						new AgeGroupType
						{
							Name = "Hatchling",
							AIAgeGroup = AIAgeGroup.Child,
							CanReproduce = false,
							Edge = 0.7f,
							HeightTargetModifier = 0.6f,
							WeightTargetModifier = 0.5f,
							ModelScaleFraction = 0.6f
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.YoungAdult,
							CanReproduce = false,
							Edge = 0.9f,
							HeightTargetModifier = 0.97f,
							WeightTargetModifier = 0.92f,
							ModelScaleFraction = 0.9f,
							NeedTypes = new NeedType[1] { needType6 }
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.Adult,
							CanReproduce = true,
							Edge = 3.8f,
							HeightTargetModifier = 1f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f,
							NeedTypes = new NeedType[1] { needType6 }
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.Old,
							CanReproduce = true,
							Edge = 4f,
							HeightTargetModifier = 0.9f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 0.95f,
							NeedTypes = new NeedType[1] { needType6 }
						}
					}
				}
			}
		};
		listOfEntityTypes.Add(entityType11);
		needType = new NeedType();
		needType.KeyName = "foodEnergy";
		needType.FoodNeedType = new FoodNeedType
		{
			FoodNutrient = "foodEnergy",
			RequiredNutrientsAsFractionOfEntityBulk = 0.1f
		};
		needType.DecreasePerDay = new NormalDistribution
		{
			Mean = 2.0,
			StandardDeviation = 0.019999999552965164
		};
		needType.LimitForDecreasedEnergy = 0.1f;
		needType.DecreasedEnergyWeight = 0.6f;
		needType.PhysicalEffects = new PhysicalEffects
		{
			DaysAtZeroCausingCollapse = 4f,
			DaysAtZeroCausingDeath = 4f,
			DaysAtZeroDecreaseFactor = 1f,
			UseExertionFactorToDecrease = true
		};
		NeedType needType7 = needType;
		entityType = new EntityType("entity:binalRat");
		entityType.Name = "Binal rat";
		entityType.ThumbnailSmall = "HUD_thumbnail_binalRat";
		entityType.SummaryDescription = "Small, two-legged omnivore/scavenger. Vermin.";
		entityType.Description = "\n FEEDING CLASSIFICATION: Its flexible diet means it can survive almost anywhere.\n \n HEIGHT: 10 to 25 cm\n \n ANATOMY\n This order shares some characteristics with the thunder chickens. A common feature is retractable tentacles around the mouth used for feeding. They also have in common an antenna on top of the head for detecting dangers.\n \n BEHAVIOR\n Pervasive, very adaptive order of animals.\n \n ENEMIES\n Preyed on by quadites.\n \n SURVIVAL GUIDE NOTES\n They are not very fearful of humans and will be attracted by any food or waste lying exposed. Should be controlled as a pest and not hunted as a source of food (their habit of eating plants that are poisonous to humans makes their flesh inedible to us.)";
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				ModelScale = 0.5f,
				AssetName = "thunderchicken",
				ModelBasicTextureName = "BinalRatBrownTexture",
				GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]> { 
				{
					"normal",
					new GaitAnimationBracket[1]
					{
						new GaitAnimationBracket
						{
							MinimumSpeed = 0f,
							MaximumSpeed = 77f,
							StrideLength = 11f,
							StrideDuration = 0.4f,
							AnimationKey = "gaitWalk"
						}
					}
				} },
				AnimConditions = new AnimConditionInfo[5]
				{
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							AdditionalAnimations1 = new string[1] { "hidden" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Moving
						},
						GaitSetKey = "normal"
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "idle" },
							AdditionalAnimations1 = new string[1] { "look" },
							AdditionalAnimations2 = new string[1] { "eat" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Idle
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "idle" },
							AdditionalAnimations2 = new string[1] { "eat" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Eating
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "dying" },
							Sounds = new string[1] { "aliens/binalRatDie" }
						},
						Looping = Looping.No,
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "dead" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying,
							Modifiers = new BitMask64(typeof(AnimModifier), 13)
						}
					}
				}
			}
		};
		entityType.LocomotorType = new LocomotorType
		{
			MaxAngularSpeed = 7.853982f,
			MeleeRadius = 10f,
			LeggedLocomotorType = new LeggedLocomotorType
			{
				TerrainNegateFactor = 0.01f,
				WalkSlowSpeed = 8f,
				WalkNormalSpeed = 50f,
				WalkFastSpeed = 74f
			},
			CollisionResponderType = new CollisionResponderType
			{
				AgentCollisionResponderType = new AgentCollisionResponderType()
			}
		};
		entityType.SensorType = new SensorType
		{
			Range = 360f,
			RangeAtNight = 360f,
			DetectionTypeKey = "defaultDetection"
		};
		entityType.BodyType = GameData.Instance.AllBodyTypes["thunderchicken"];
		entityType.ContainerType = new AgentStorageType
		{
			ItemStorageType = new ItemStorageType(0.5f),
			StomachStorageType = new ItemStorageType(0.07f)
		};
		EntityType entityType12 = entityType;
		entityType12.IntelligenceType = new IntelligenceType
		{
			IsMobile = true,
			CanAttack = false,
			CanUseWeapons = false,
			CanHunt = false,
			CanScout = true,
			CanExamine = true,
			CanPatrol = false,
			CanHaul = false,
			IsPredator = false,
			StrengthRating = StrengthRating.None,
			InterestInTriggerTypes = new string[2] { "smallImprovisedTrapTrigger", "spikeTrapTrigger" },
			ContainerTransactTag = "ratTransact",
			Courage = 0f,
			MemoryInDays = 3f,
			Boldness = 1.49f,
			AggroRange = 0f
		};
		entityType12.BiologicalType = new BiologicalType
		{
			OrderKey = "binalRatOrder",
			OxygenAndMuscleEnergyIncreaseRatePerDay = 10f,
			FoodItemTagsThatCanBeConsumed = new string[7] { "cookedMeat", "rawMeat", "smallRawMeat", "rottenMeat", "inedibleMeat", "edibleVegi", "inedibleVegi" },
			ExtractionProcessTypes = new string[13]
			{
				"extractMudWormMeat", "extractPatricianMeat", "extractLeafCutterMeat", "extractThunderChickenMeat", "extractTwinklerMeat", "extractBushDragonMeat", "extractTurnipMeat", "extractSwampDemonTreeMeat", "extractDemonTreeMeat", "extractMegapodMeat",
				"extractWhipjawMeat", "extractSpikePlantMeat", "extractForestGuardianMeat"
			},
			TimeToConsumeFullMealInDays = 0.0125f,
			StomachSizeFractionOfEntityBulk = 0.2f,
			StomachContentsDecreaseRatePerDay = 3f,
			ActiveStealthRating = 0.1f,
			MaxRegainLimit = 0.5f,
			FractionOfMaxHitpointsGainedPerDay = 0.3f,
			Carcass = "item:binalRatCarcass",
			IsVermin = true,
			Castes = new List<CasteType>
			{
				new CasteType
				{
					KeyName = "male",
					Reproduction = Reproduction.Male,
					Edge = 0.51f,
					HeightMean = 0.25f,
					HeightStandardDeviation = 0.02f,
					WeightMean = 15f,
					WeightStandardDeviation = 1f,
					AgeGroupTypes = new List<AgeGroupType>
					{
						new AgeGroupType
						{
							Name = "Chick",
							AIAgeGroup = AIAgeGroup.Baby,
							CanReproduce = false,
							Edge = 0.3f,
							HeightTargetModifier = 0.05f,
							WeightTargetModifier = 0.03f,
							ModelScaleFraction = 0.5f,
							NeedTypes = new NeedType[1] { needType7 }
						},
						new AgeGroupType
						{
							Name = "Chick",
							AIAgeGroup = AIAgeGroup.Child,
							CanReproduce = false,
							Edge = 0.6f,
							HeightTargetModifier = 0.6f,
							WeightTargetModifier = 0.5f,
							ModelScaleFraction = 0.75f,
							NeedTypes = new NeedType[1] { needType7 }
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.YoungAdult,
							CanReproduce = false,
							Edge = 1f,
							HeightTargetModifier = 0.97f,
							WeightTargetModifier = 0.92f,
							ModelScaleFraction = 0.9f,
							NeedTypes = new NeedType[1] { needType7 }
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.Adult,
							CanReproduce = true,
							Edge = 5f,
							HeightTargetModifier = 1f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f,
							NeedTypes = new NeedType[1] { needType7 }
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.Old,
							CanReproduce = true,
							Edge = 6f,
							HeightTargetModifier = 0.9f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f,
							NeedTypes = new NeedType[1] { needType7 }
						}
					}
				},
				new CasteType
				{
					KeyName = "female",
					Reproduction = Reproduction.Female,
					Edge = 1f,
					HeightMean = 0.25f,
					HeightStandardDeviation = 0.02f,
					WeightMean = 14f,
					WeightStandardDeviation = 1f,
					AgeGroupTypes = new List<AgeGroupType>
					{
						new AgeGroupType
						{
							Name = "Chick",
							AIAgeGroup = AIAgeGroup.Baby,
							CanReproduce = false,
							Edge = 0.3f,
							HeightTargetModifier = 0.05f,
							WeightTargetModifier = 0.03f,
							ModelScaleFraction = 0.5f,
							NeedTypes = new NeedType[1] { needType7 }
						},
						new AgeGroupType
						{
							Name = "Chick",
							AIAgeGroup = AIAgeGroup.Child,
							CanReproduce = false,
							Edge = 0.6f,
							HeightTargetModifier = 0.6f,
							WeightTargetModifier = 0.5f,
							ModelScaleFraction = 0.75f,
							NeedTypes = new NeedType[1] { needType7 }
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.YoungAdult,
							CanReproduce = false,
							Edge = 1f,
							HeightTargetModifier = 0.97f,
							WeightTargetModifier = 0.92f,
							ModelScaleFraction = 0.9f,
							NeedTypes = new NeedType[1] { needType7 }
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.Adult,
							CanReproduce = true,
							Edge = 5f,
							HeightTargetModifier = 1f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f,
							NeedTypes = new NeedType[1] { needType7 }
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.Old,
							CanReproduce = false,
							Edge = 6f,
							HeightTargetModifier = 0.9f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f,
							NeedTypes = new NeedType[1] { needType7 }
						}
					}
				}
			}
		};
		listOfEntityTypes.Add(entityType12);
		needType = new NeedType();
		needType.KeyName = "foodEnergy";
		needType.FoodNeedType = new FoodNeedType
		{
			FoodNutrient = "foodEnergy",
			RequiredNutrientsAsFractionOfEntityBulk = 0.1f
		};
		needType.DecreasePerDay = new NormalDistribution
		{
			Mean = 1.0,
			StandardDeviation = 0.019999999552965164
		};
		needType.LimitForDecreasedEnergy = 0.1f;
		needType.DecreasedEnergyWeight = 0.6f;
		needType.PhysicalEffects = new PhysicalEffects
		{
			DaysAtZeroCausingCollapse = 4f,
			DaysAtZeroCausingDeath = 4f,
			DaysAtZeroDecreaseFactor = 1f,
			UseExertionFactorToDecrease = true
		};
		entityType = new EntityType("entity:mudWorm");
		entityType.Name = "Mud worm";
		entityType.ThumbnailSmall = "HUD_thumbnail_worm";
		entityType.SummaryDescription = "Small worm-like scavenger. Not dangerous.";
		entityType.Description = "It makes its home in soft, moist soil such as river banks and wetlands. Has a scavenger life style and emerges from its tunnels to feed on whatever decomposing flesh or plant matter it can find.";
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				ModelScale = 0.4f,
				AssetName = "worm",
				ModelBasicTextureName = "WormSimpleTexture",
				GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]> { 
				{
					"normal",
					new GaitAnimationBracket[1]
					{
						new GaitAnimationBracket
						{
							MinimumSpeed = 20f,
							MaximumSpeed = 35f,
							StrideLength = 22f,
							StrideDuration = 1.4f,
							AnimationKey = "walk"
						}
					}
				} },
				DefaultInfo = new AnimConditionInfo
				{
					SoundAndAnimationSet = new RandomSoundAndAnimationSet
					{
						BaseAnimations = new string[1] { "idle" }
					}
				},
				DefaultStances = new AnimConditionInfo[2]
				{
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "idle" }
						}
					},
					new AnimConditionInfo
					{
						ConditionSet = new AnimConditions
						{
							Modifiers = new BitMask64(typeof(AnimModifier), 6)
						},
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "combatIdle" }
						}
					}
				},
				AnimConditions = new AnimConditionInfo[9]
				{
					new AnimConditionInfo
					{
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Moving
						},
						GaitSetKey = "normal"
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "idle" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Idle
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "eat" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Eating
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "combatIdle" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Idle,
							Modifiers = new BitMask64(typeof(AnimModifier), 6)
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "attack" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Attacking
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "hit" },
							Sounds = new string[1] { "aliens/alienCombat/wormHitShort" }
						},
						Playback = Playback.Manual,
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Recoiling
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "dead" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "collapse" },
							Sounds = new string[1] { "aliens/alienCombat/wormDeathShort" }
						},
						Looping = Looping.No,
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying,
							Modifiers = new BitMask64(typeof(AnimModifier), 12)
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "dead" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying,
							Modifiers = new BitMask64(typeof(AnimModifier), 13)
						}
					}
				}
			}
		};
		entityType.LocomotorType = new LocomotorType
		{
			MaxAngularSpeed = (float)Math.PI / 4f,
			FourSidedSymmetry = false,
			MeleeRadius = 32f,
			LeggedLocomotorType = new LeggedLocomotorType
			{
				TerrainNegateFactor = 0.1f,
				WalkSlowSpeed = 3f,
				WalkNormalSpeed = 4f,
				WalkFastSpeed = 6f
			},
			CollisionResponderType = new CollisionResponderType
			{
				AgentCollisionResponderType = new AgentCollisionResponderType()
			}
		};
		entityType.SensorType = new SensorType
		{
			Range = 370f,
			RangeAtNight = 220f,
			DetectionTypeKey = "defaultDetection"
		};
		entityType.BodyType = bodyType;
		entityType.ContainerType = new AgentStorageType
		{
			ItemStorageType = new ItemStorageType(0.5f),
			StomachStorageType = new ItemStorageType(0.07f)
		};
		EntityType entityType13 = entityType;
		entityType13.IntelligenceType = new IntelligenceType
		{
			IsMobile = true,
			CanAttack = false,
			CanUseWeapons = false,
			CanHunt = false,
			CanScout = true,
			CanExamine = true,
			CanPatrol = true,
			CanHaul = false,
			IsPredator = false,
			ContainerTransactTag = "ratTransact",
			InterestInTriggerTypes = new string[3] { "mineTrigger", "spikeTrapTrigger", "smallImprovisedTrapTrigger" },
			StrengthRating = StrengthRating.None,
			Courage = 0f,
			MemoryInDays = 3f,
			Boldness = 1.49f
		};
		entityType13.BiologicalType = new BiologicalType
		{
			OrderKey = "wormOrder",
			OxygenAndMuscleEnergyIncreaseRatePerDay = 12f,
			TimeToConsumeFullMealInDays = 0.005f,
			StomachSizeFractionOfEntityBulk = 0.45f,
			StomachContentsDecreaseRatePerDay = 3f,
			FoodItemTagsThatCanBeConsumed = new string[8] { "edibleVegi", "inedibleVegi", "cookedMeat", "rawMeat", "smallRawMeat", "spoiledMeal", "inedibleMeat", "rottenMeat" },
			ExtractionProcessTypes = new string[11]
			{
				"extractPatricianMeat", "extractLeafCutterMeat", "extractThunderChickenMeat", "extractBinalRatMeat", "extractTwinklerMeat", "extractBushDragonMeat", "extractSwampDemonTreeMeat", "extractDemonTreeMeat", "extractWhipjawMeat", "extractSpikePlantMeat",
				"extractForestGuardianMeat"
			},
			IsTerritorial = false,
			MaxRegainLimit = 0.5f,
			FractionOfMaxHitpointsGainedPerDay = 0.3f,
			ActiveStealthRating = 0.2f,
			Carcass = "item:mudWormCarcass",
			ResilienceMean = 1f,
			ResilienceStandardDeviation = 0f,
			IsVermin = true,
			Castes = new List<CasteType>
			{
				new CasteType
				{
					KeyName = "Worker",
					Reproduction = Reproduction.Male,
					Edge = 1f,
					HeightMean = 0.3f,
					HeightStandardDeviation = 0.08f,
					WeightMean = 20f,
					WeightStandardDeviation = 3f,
					AgeGroupTypes = new List<AgeGroupType>
					{
						new AgeGroupType
						{
							Name = "Hatchling",
							AIAgeGroup = AIAgeGroup.Baby,
							CanReproduce = false,
							Edge = 0.5f,
							HeightTargetModifier = 0.7f,
							WeightTargetModifier = 0.7f,
							ModelScaleFraction = 0.7f
						},
						new AgeGroupType
						{
							Name = "Hatchling",
							AIAgeGroup = AIAgeGroup.Child,
							CanReproduce = false,
							Edge = 0.8f,
							HeightTargetModifier = 0.8f,
							WeightTargetModifier = 0.8f,
							ModelScaleFraction = 0.8f
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.YoungAdult,
							CanReproduce = false,
							Edge = 1f,
							HeightTargetModifier = 0.9f,
							WeightTargetModifier = 0.9f,
							ModelScaleFraction = 0.9f,
							NeedTypes = new NeedType[1] { needType6 }
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.Adult,
							CanReproduce = true,
							Edge = 3.5f,
							HeightTargetModifier = 1f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.Old,
							CanReproduce = true,
							Edge = 4f,
							HeightTargetModifier = 1f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f
						}
					}
				}
			}
		};
		listOfEntityTypes.Add(entityType13);
		needType = new NeedType();
		needType.KeyName = "foodEnergy";
		needType.FoodNeedType = new FoodNeedType
		{
			FoodNutrient = "foodEnergy",
			RequiredNutrientsAsFractionOfEntityBulk = 0.1f
		};
		needType.DecreasePerDay = new NormalDistribution
		{
			Mean = 2.0,
			StandardDeviation = 0.019999999552965164
		};
		needType.LimitForDecreasedEnergy = 0.1f;
		needType.DecreasedEnergyWeight = 0.6f;
		needType.PhysicalEffects = new PhysicalEffects
		{
			DaysAtZeroCausingCollapse = 4f,
			DaysAtZeroCausingDeath = 4f,
			DaysAtZeroDecreaseFactor = 1f,
			UseExertionFactorToDecrease = true
		};
		entityType = new EntityType("entity:bushDragon");
		entityType.Name = "Northern bush dragon";
		entityType.ThumbnailSmall = "HUD_thumbnail_bushDragon";
		entityType.SummaryDescription = "Omnivorous herd animal";
		entityType.Description = "\n FEEDING CLASSIFICATION: Omnivore. Eats low vegetation, small animals, carrion.\n \n HEIGHT: Up to 1.5 m (wings excluded)\n \n ANATOMY\n Waddling, 3-legged animal which has developed wings, not for flight but for display purposes. Likely used to dissuade predators and possibly in mating behaviour. The creature has a defensive weapon in the form of a chemical spray.\n \n BEHAVIOR\n If approached, bush dragons will defend themselves much in the manner of the terran skunk. Against the quadites the spray seems to be particularly effective, causing incapacitation and even death.\n \n SURVIVAL GUIDE NOTES\n The slow-moving creatures protect their herd, but if distance is observed they do not attack. Their defensive chemical is quite toxic to humans and caution is advised.";
		entityType.DetectionTag = "huge";
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "bushdragon",
				GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]> { 
				{
					"normal",
					new GaitAnimationBracket[1]
					{
						new GaitAnimationBracket
						{
							MinimumSpeed = 20f,
							MaximumSpeed = 35f,
							StrideLength = 11f,
							StrideDuration = 1.1f,
							AnimationKey = "gaitWalk"
						}
					}
				} },
				DefaultInfo = new AnimConditionInfo
				{
					SoundAndAnimationSet = new RandomSoundAndAnimationSet
					{
						BaseAnimations = new string[1] { "idle" }
					}
				},
				DefaultStances = new AnimConditionInfo[2]
				{
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "idle" }
						}
					},
					new AnimConditionInfo
					{
						ConditionSet = new AnimConditions
						{
							Modifiers = new BitMask64(typeof(AnimModifier), 6)
						},
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "combatIdle" }
						}
					}
				},
				AnimConditions = new AnimConditionInfo[6]
				{
					new AnimConditionInfo
					{
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Moving
						},
						GaitSetKey = "normal"
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "idle" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Idle
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "attackNormal" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Attacking,
							Modifiers = new BitMask64(typeof(AnimModifier), 18)
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "hit" },
							Sounds = new string[1] { "aliens/bushdragonHit" }
						},
						Playback = Playback.Manual,
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Recoiling
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "dying" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "dead" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying,
							Modifiers = new BitMask64(typeof(AnimModifier), 13)
						}
					}
				}
			}
		};
		entityType.LocomotorType = new LocomotorType
		{
			MaxAngularSpeed = (float)Math.PI / 4f,
			FourSidedSymmetry = false,
			MeleeRadius = 16f,
			LeggedLocomotorType = new LeggedLocomotorType
			{
				TerrainNegateFactor = 0.6f,
				WalkSlowSpeed = 8f,
				WalkNormalSpeed = 9f,
				WalkFastSpeed = 11f
			},
			CollisionResponderType = new CollisionResponderType
			{
				AgentCollisionResponderType = new AgentCollisionResponderType()
			}
		};
		entityType.SensorType = new SensorType
		{
			Range = 350f,
			RangeAtNight = 200f,
			DetectionTypeKey = "defaultDetection"
		};
		entityType.BodyType = GameData.Instance.AllBodyTypes["bushdragon"];
		entityType.ContainerType = new AgentStorageType
		{
			ItemStorageType = new ItemStorageType(0.8f),
			StomachStorageType = new ItemStorageType(0.07f)
		};
		EntityType entityType14 = entityType;
		entityType14.IntelligenceType = new IntelligenceType
		{
			MembersScoutingFraction = 1f,
			IsMobile = true,
			CanAttack = true,
			CanUseWeapons = false,
			CanHunt = true,
			CanScout = true,
			CanExamine = true,
			CanPatrol = true,
			CanHaul = false,
			StrengthRating = StrengthRating.LikeHumans,
			InterestInTriggerTypes = new string[2] { "mineTrigger", "spikeTrapTrigger" },
			Courage = 0.5f,
			MemoryInDays = 0.2f,
			Boldness = 0.25f,
			AggroRange = 110f,
			ChanceToRestAfterMeleeAttack = 0.3,
			MinRestTimeAfterAttackingInSeconds = 0.5f,
			MaxRestTimeAfterAttackingInSeconds = 1.2f,
			Prey = new string[5] { "entity:mudWorm", "entity:binalRat", "entity:whiteThunderChicken", "entity:pygmyThunderChicken", "entity:bajingan" },
			Skills = new SerializableDictionary<string, float> { { "unarmedFighting", 0.6f } },
			Attacks = new string[1] { "bushDragonSpray" }
		};
		entityType14.BiologicalType = new BiologicalType
		{
			OxygenAndMuscleEnergyIncreaseRatePerDay = 8f,
			TimeToConsumeFullMealInDays = 0.0125f,
			FoodItemTagsThatCanBeConsumed = new string[3] { "inedibleVegi", "edibleVegi", "spoiledMeal" },
			StomachSizeFractionOfEntityBulk = 0.2f,
			StomachContentsDecreaseRatePerDay = 3f,
			ActiveStealthRating = 0.1f,
			OrderKey = "bushDragonOrder",
			IsTerritorial = true,
			MaxRegainLimit = 0.5f,
			FractionOfMaxHitpointsGainedPerDay = 0.3f,
			Carcass = "item:bushDragonCarcass",
			RaceTypes = new RaceType[1]
			{
				new RaceType
				{
					Name = "Northern Bush Dragon",
					PortraitSkinType = "Pale",
					PrimaryColor = "F7F4C5".ToColorVector3(),
					ModelBasicTextureName = "BushdragonPaleTexture",
					Edge = 0.3f,
					ModelScale = 0.8f
				}
			},
			Castes = new List<CasteType>
			{
				new CasteType
				{
					KeyName = "male",
					Reproduction = Reproduction.Male,
					Edge = 0.51f,
					HeightMean = 2f,
					HeightStandardDeviation = 0.08f,
					WeightMean = 100f,
					WeightStandardDeviation = 0.15f,
					AgeGroupTypes = new List<AgeGroupType>
					{
						new AgeGroupType
						{
							Name = "Baby",
							AIAgeGroup = AIAgeGroup.Baby,
							CanReproduce = false,
							Edge = 0.5f,
							HeightTargetModifier = 0.5f,
							WeightTargetModifier = 0.03f,
							ModelScaleFraction = 0.3f
						},
						new AgeGroupType
						{
							Name = "Child",
							AIAgeGroup = AIAgeGroup.Child,
							CanReproduce = false,
							Edge = 1f,
							HeightTargetModifier = 0.8f,
							WeightTargetModifier = 0.5f,
							ModelScaleFraction = 0.6f
						},
						new AgeGroupType
						{
							Name = "Young adult",
							AIAgeGroup = AIAgeGroup.YoungAdult,
							CanReproduce = false,
							Edge = 4f,
							HeightTargetModifier = 0.97f,
							WeightTargetModifier = 0.92f,
							ModelScaleFraction = 0.85f
						},
						new AgeGroupType
						{
							Name = "Adult",
							AIAgeGroup = AIAgeGroup.Adult,
							CanReproduce = true,
							Edge = 28f,
							HeightTargetModifier = 1f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f
						},
						new AgeGroupType
						{
							Name = "Old",
							AIAgeGroup = AIAgeGroup.Old,
							CanReproduce = true,
							Edge = 30f,
							HeightTargetModifier = 0.9f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f
						}
					}
				},
				new CasteType
				{
					KeyName = "female",
					Reproduction = Reproduction.Female,
					Edge = 1f,
					HeightMean = 1.9f,
					HeightStandardDeviation = 0.05f,
					WeightMean = 90f,
					WeightStandardDeviation = 0.1f,
					AgeGroupTypes = new List<AgeGroupType>
					{
						new AgeGroupType
						{
							Name = "Baby",
							AIAgeGroup = AIAgeGroup.Baby,
							CanReproduce = false,
							Edge = 0.5f,
							HeightTargetModifier = 0.05f,
							WeightTargetModifier = 0.03f,
							ModelScaleFraction = 0.3f
						},
						new AgeGroupType
						{
							Name = "Child",
							AIAgeGroup = AIAgeGroup.Child,
							CanReproduce = false,
							Edge = 1f,
							HeightTargetModifier = 0.6f,
							WeightTargetModifier = 0.5f,
							ModelScaleFraction = 0.6f
						},
						new AgeGroupType
						{
							Name = "Young adult",
							AIAgeGroup = AIAgeGroup.YoungAdult,
							CanReproduce = false,
							Edge = 4f,
							HeightTargetModifier = 0.97f,
							WeightTargetModifier = 0.92f,
							ModelScaleFraction = 0.85f
						},
						new AgeGroupType
						{
							Name = "Adult",
							AIAgeGroup = AIAgeGroup.Adult,
							CanReproduce = true,
							Edge = 28f,
							HeightTargetModifier = 1f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f
						},
						new AgeGroupType
						{
							Name = "Old",
							AIAgeGroup = AIAgeGroup.Old,
							CanReproduce = false,
							Edge = 30f,
							HeightTargetModifier = 0.9f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f
						}
					}
				}
			}
		};
		listOfEntityTypes.Add(entityType14);
		needType = new NeedType();
		needType.KeyName = "foodEnergy";
		needType.FoodNeedType = new FoodNeedType
		{
			FoodNutrient = "foodEnergy",
			RequiredNutrientsAsFractionOfEntityBulk = 0.03f
		};
		needType.DecreasePerDay = new NormalDistribution
		{
			Mean = 3.0,
			StandardDeviation = 0.019999999552965164
		};
		needType.LimitForDecreasedEnergy = 0.05f;
		needType.DecreasedEnergyWeight = 0.08f;
		needType.PhysicalEffects = new PhysicalEffects
		{
			DaysAtZeroCausingCollapse = 2f,
			DaysAtZeroCausingDeath = 2.1f,
			DaysAtZeroDecreaseFactor = 1f,
			LimitForReducedGrowth = 0.1f,
			LimitForIncreasedSickness = 0.05f,
			UseExertionFactorToDecrease = false
		};
		NeedType needType8 = needType;
		entityType = new EntityType("entity:spoakDendront");
		entityType.Name = "Spoak dendront";
		entityType.ThumbnailSmall = "HUD_thumbnail_demonTree";
		entityType.SummaryDescription = "Ambush predator";
		entityType.Description = "\n FEEDING CLASSIFICATION: Ambush predator.\n \n HEIGHT: up to 250 cm\n \n ANATOMY\n Like other dendronts, this species has specialized in camouflage and mimicry of the surrounding vegetation, in this case spoak trees. The order shares some similarities with the bush dragons, notably, its 3-legged anatomy and the large wing-like appendages.\n \n BEHAVIOR\n The animal is able to blend in with spoak trees, even matching the way the wind moves the leaves. When prey (typically thunder chicken) is near, the dendront seizes it with a quick thrust of its sharp limbs. If the spoak dendront is revealed by competitors (or humans), it will try to scare them away by spreading out its 'leaves'. This menacing pose made some colonists name the animal 'Tree demon' though the name has not been widely adopted.\n \n THREAT LEVEL\n High. The animal is very hard to notice until too late, so caution is recommended in the animal's habitat.";
		entityType.DetectionTag = "wellHiddenAnimal";
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				ModelScale = 3.8f,
				AssetName = "demonTree",
				ModelBasicTextureName = "DemonTreeTexture",
				GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]> { 
				{
					"normal",
					new GaitAnimationBracket[1]
					{
						new GaitAnimationBracket
						{
							MinimumSpeed = 0f,
							MaximumSpeed = 5f,
							StrideLength = 1f,
							StrideDuration = 0.44f,
							AnimationKey = "walk"
						}
					}
				} },
				AnimConditions = new AnimConditionInfo[9]
				{
					new AnimConditionInfo
					{
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Moving
						},
						GaitSetKey = "normal"
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "idle" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Idle
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "eat" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Eating
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "combatIdle" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Idle,
							Modifiers = new BitMask64(typeof(AnimModifier), 6)
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "attack" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Attacking
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "hit" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Recoiling
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "dead" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "collapse" },
							Sounds = new string[1] { "aliens/rattleWooden" }
						},
						Looping = Looping.No,
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying,
							Modifiers = new BitMask64(typeof(AnimModifier), 12)
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "dead" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying,
							Modifiers = new BitMask64(typeof(AnimModifier), 13)
						}
					}
				}
			}
		};
		entityType.LocomotorType = new LocomotorType
		{
			MaxAngularSpeed = (float)Math.PI * 4f,
			FourSidedSymmetry = false,
			MeleeRadius = 22f,
			LeggedLocomotorType = new LeggedLocomotorType
			{
				TerrainNegateFactor = 0.5f,
				WalkSlowSpeed = 4f,
				WalkNormalSpeed = 8f,
				WalkFastSpeed = 12f
			},
			CollisionResponderType = new CollisionResponderType
			{
				AgentCollisionResponderType = new AgentCollisionResponderType()
			}
		};
		entityType.SensorType = new SensorType
		{
			Range = 225f,
			RangeAtNight = 150f,
			DetectionTypeKey = "defaultDetection"
		};
		entityType.BodyType = GameData.Instance.AllBodyTypes["demonTree"];
		entityType.ContainerType = new AgentStorageType
		{
			ItemStorageType = new ItemStorageType(0.8f),
			StomachStorageType = new ItemStorageType(0.07f)
		};
		EntityType entityType15 = entityType;
		entityType15.IntelligenceType = new IntelligenceType
		{
			IsMobile = true,
			CanAttack = true,
			CanUseWeapons = false,
			CanHunt = true,
			CanScout = true,
			CanExamine = true,
			CanPatrol = true,
			CanHaul = false,
			IsPredator = true,
			WillAttackNonThreatsNearby = true,
			StrengthRating = StrengthRating.LikeHumans,
			InterestInTriggerTypes = new string[2] { "mineTrigger", "spikeTrapTrigger" },
			Courage = 0.5f,
			MemoryInDays = 3f,
			Boldness = 0.8f,
			AggroRange = 125f,
			ContainerTransactTag = "demonTreeTransact",
			ChanceToRestAfterMeleeAttack = 0.5,
			MinRestTimeAfterAttackingInSeconds = 0.5f,
			MaxRestTimeAfterAttackingInSeconds = 1f,
			Prey = new string[5] { "entity:mudWorm", "entity:binalRat", "entity:whiteThunderChicken", "entity:pygmyThunderChicken", "entity:bajingan" },
			Attacks = new string[1] { "demonTreeAttack" },
			Skills = new SerializableDictionary<string, float> { { "unarmedFighting", 0.9f } }
		};
		entityType15.BiologicalType = new BiologicalType
		{
			OrderKey = "demonTreeOrder",
			OxygenAndMuscleEnergyIncreaseRatePerDay = 10f,
			TimeToConsumeFullMealInDays = 0.0025f,
			FoodItemTagsThatCanBeConsumed = new string[6] { "cookedMeat", "rawMeat", "smallRawMeat", "spoiledMeal", "inedibleMeat", "rottenMeat" },
			ExtractionProcessTypes = new string[1] { "extractThunderChickenMeat" },
			StomachSizeFractionOfEntityBulk = 0.15f,
			StomachContentsDecreaseRatePerDay = 0.25f,
			IsTerritorial = true,
			MaxRegainLimit = 0.5f,
			FractionOfMaxHitpointsGainedPerDay = 0.3f,
			ActiveStealthRating = 1f,
			Carcass = "item:demontreeCarcass",
			Castes = new List<CasteType>
			{
				new CasteType
				{
					KeyName = "male",
					Reproduction = Reproduction.Male,
					Edge = 0.51f,
					HeightMean = 2f,
					HeightStandardDeviation = 0.08f,
					WeightMean = 110f,
					WeightStandardDeviation = 0.15f,
					AgeGroupTypes = new List<AgeGroupType>
					{
						new AgeGroupType
						{
							Name = "Baby",
							AIAgeGroup = AIAgeGroup.Baby,
							CanReproduce = false,
							Edge = 0.5f,
							HeightTargetModifier = 0.05f,
							WeightTargetModifier = 0.03f,
							ModelScaleFraction = 0.3f,
							NeedTypes = new NeedType[1] { needType8 }
						},
						new AgeGroupType
						{
							Name = "Child",
							AIAgeGroup = AIAgeGroup.Child,
							CanReproduce = false,
							Edge = 1f,
							HeightTargetModifier = 0.6f,
							WeightTargetModifier = 0.5f,
							ModelScaleFraction = 0.6f,
							NeedTypes = new NeedType[1] { needType8 }
						},
						new AgeGroupType
						{
							Name = "Young adult",
							AIAgeGroup = AIAgeGroup.YoungAdult,
							CanReproduce = false,
							Edge = 4f,
							HeightTargetModifier = 0.97f,
							WeightTargetModifier = 0.92f,
							ModelScaleFraction = 0.85f,
							NeedTypes = new NeedType[1] { needType8 }
						},
						new AgeGroupType
						{
							Name = "Adult",
							AIAgeGroup = AIAgeGroup.Adult,
							CanReproduce = true,
							Edge = 24f,
							HeightTargetModifier = 1f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f,
							NeedTypes = new NeedType[1] { needType8 }
						},
						new AgeGroupType
						{
							Name = "Old",
							AIAgeGroup = AIAgeGroup.Old,
							CanReproduce = true,
							Edge = 25f,
							HeightTargetModifier = 0.9f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f,
							NeedTypes = new NeedType[1] { needType8 }
						}
					}
				},
				new CasteType
				{
					KeyName = "female",
					Reproduction = Reproduction.Female,
					Edge = 1f,
					HeightMean = 1.9f,
					HeightStandardDeviation = 0.05f,
					WeightMean = 100f,
					WeightStandardDeviation = 0.1f,
					AgeGroupTypes = new List<AgeGroupType>
					{
						new AgeGroupType
						{
							Name = "Baby",
							AIAgeGroup = AIAgeGroup.Baby,
							CanReproduce = false,
							Edge = 0.5f,
							HeightTargetModifier = 0.05f,
							WeightTargetModifier = 0.03f,
							ModelScaleFraction = 0.3f,
							NeedTypes = new NeedType[1] { needType8 }
						},
						new AgeGroupType
						{
							Name = "Child",
							AIAgeGroup = AIAgeGroup.Child,
							CanReproduce = false,
							Edge = 1f,
							HeightTargetModifier = 0.6f,
							WeightTargetModifier = 0.5f,
							ModelScaleFraction = 0.6f,
							NeedTypes = new NeedType[1] { needType8 }
						},
						new AgeGroupType
						{
							Name = "Young adult",
							AIAgeGroup = AIAgeGroup.YoungAdult,
							CanReproduce = false,
							Edge = 4f,
							HeightTargetModifier = 0.97f,
							WeightTargetModifier = 0.92f,
							ModelScaleFraction = 0.85f,
							NeedTypes = new NeedType[1] { needType8 }
						},
						new AgeGroupType
						{
							Name = "Adult",
							AIAgeGroup = AIAgeGroup.Adult,
							CanReproduce = true,
							Edge = 24f,
							HeightTargetModifier = 1f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f,
							NeedTypes = new NeedType[1] { needType8 }
						},
						new AgeGroupType
						{
							Name = "Old",
							AIAgeGroup = AIAgeGroup.Old,
							CanReproduce = false,
							Edge = 25f,
							HeightTargetModifier = 0.9f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f,
							NeedTypes = new NeedType[1] { needType8 }
						}
					}
				}
			}
		};
		listOfEntityTypes.Add(entityType15);
		needType = new NeedType();
		needType.KeyName = "foodEnergy";
		needType.FoodNeedType = new FoodNeedType
		{
			FoodNutrient = "foodEnergy",
			RequiredNutrientsAsFractionOfEntityBulk = 0.03f
		};
		needType.DecreasePerDay = new NormalDistribution
		{
			Mean = 3.0,
			StandardDeviation = 0.019999999552965164
		};
		needType.LimitForDecreasedEnergy = 0.05f;
		needType.DecreasedEnergyWeight = 0.08f;
		needType.PhysicalEffects = new PhysicalEffects
		{
			DaysAtZeroCausingCollapse = 2f,
			DaysAtZeroCausingDeath = 3f,
			DaysAtZeroDecreaseFactor = 1f,
			LimitForReducedGrowth = 0.1f,
			LimitForIncreasedSickness = 0.05f,
			UseExertionFactorToDecrease = false
		};
		needType8 = needType;
		entityType = new EntityType("entity:swampDendront");
		entityType.Name = "Swamp dendront";
		entityType.ThumbnailSmall = "HUD_thumbnail_demonTreeSwamp";
		entityType.SummaryDescription = "Ambush predator";
		entityType.Description = "\n FEEDING CLASSIFICATION: Ambush predator.\n \n HEIGHT: up to 250 cm\n \n ANATOMY\n Amphibious species of dendronts which is adapted for the swamp. The dendront order shares some similarities with the bush dragons, notably, its 3-legged anatomy and the flexible appendages which in this species look like vines and branches.\n \n BEHAVIOR\n Will slither around the swamp until prey is within striking distance, at which point the dendront is usually able to kill it with a single strike.\n \n THREAT LEVEL\n High. The animal is very hard to notice until too late, so caution is recommended in the animal's habitat.";
		entityType.DetectionTag = "wellHiddenAnimal";
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				ModelScale = 3.8f,
				AssetName = "demonTreeMoss",
				ModelBasicTextureName = "DemonTreeMossTexture1",
				GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]> { 
				{
					"normal",
					new GaitAnimationBracket[1]
					{
						new GaitAnimationBracket
						{
							MinimumSpeed = 0f,
							MaximumSpeed = 5f,
							StrideLength = 1f,
							StrideDuration = 0.44f,
							AnimationKey = "walk"
						}
					}
				} },
				AnimConditions = new AnimConditionInfo[9]
				{
					new AnimConditionInfo
					{
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Moving
						},
						GaitSetKey = "normal"
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "idle" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Idle
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "eat" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Eating
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "combatIdle" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Idle,
							Modifiers = new BitMask64(typeof(AnimModifier), 6)
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "attack" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Attacking
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "hit" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Recoiling
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "dead" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "collapse" },
							Sounds = new string[1] { "aliens/rattleWooden" }
						},
						Looping = Looping.No,
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying,
							Modifiers = new BitMask64(typeof(AnimModifier), 12)
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "dead" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying,
							Modifiers = new BitMask64(typeof(AnimModifier), 13)
						}
					}
				}
			}
		};
		entityType.LocomotorType = new LocomotorType
		{
			MaxAngularSpeed = (float)Math.PI * 4f,
			FourSidedSymmetry = false,
			MeleeRadius = 22f,
			LeggedLocomotorType = new LeggedLocomotorType
			{
				TerrainNegateFactor = 0.5f,
				WalkSlowSpeed = 4f,
				WalkNormalSpeed = 8f,
				WalkFastSpeed = 12f
			},
			CollisionResponderType = new CollisionResponderType
			{
				AgentCollisionResponderType = new AgentCollisionResponderType()
			}
		};
		entityType.SensorType = new SensorType
		{
			Range = 225f,
			RangeAtNight = 150f,
			DetectionTypeKey = "defaultDetection"
		};
		entityType.BodyType = GameData.Instance.AllBodyTypes["demonTree"];
		entityType.ContainerType = new AgentStorageType
		{
			ItemStorageType = new ItemStorageType(0.8f),
			StomachStorageType = new ItemStorageType(0.07f)
		};
		entityType15 = entityType;
		entityType15.IntelligenceType = new IntelligenceType
		{
			IsMobile = true,
			CanAttack = true,
			CanUseWeapons = false,
			CanHunt = true,
			CanScout = true,
			CanExamine = true,
			CanPatrol = true,
			CanHaul = false,
			IsPredator = true,
			WillAttackNonThreatsNearby = true,
			StrengthRating = StrengthRating.LikeHumans,
			InterestInTriggerTypes = new string[2] { "mineTrigger", "spikeTrapTrigger" },
			Courage = 0.5f,
			MemoryInDays = 3f,
			Boldness = 0.8f,
			AggroRange = 125f,
			ContainerTransactTag = "demonTreeTransact",
			ChanceToRestAfterMeleeAttack = 0.5,
			MinRestTimeAfterAttackingInSeconds = 0.5f,
			MaxRestTimeAfterAttackingInSeconds = 1f,
			Prey = new string[5] { "entity:mudWorm", "entity:binalRat", "entity:whiteThunderChicken", "entity:pygmyThunderChicken", "entity:bajingan" },
			Attacks = new string[1] { "demonTreeAttack" },
			Skills = new SerializableDictionary<string, float> { { "unarmedFighting", 0.9f } }
		};
		entityType15.BiologicalType = new BiologicalType
		{
			OrderKey = "demonTreeOrder",
			OxygenAndMuscleEnergyIncreaseRatePerDay = 10f,
			TimeToConsumeFullMealInDays = 0.0025f,
			FoodItemTagsThatCanBeConsumed = new string[6] { "cookedMeat", "rawMeat", "smallRawMeat", "spoiledMeal", "inedibleMeat", "rottenMeat" },
			ExtractionProcessTypes = new string[1] { "extractThunderChickenMeat" },
			StomachSizeFractionOfEntityBulk = 0.15f,
			StomachContentsDecreaseRatePerDay = 0.25f,
			IsTerritorial = true,
			MaxRegainLimit = 0.5f,
			FractionOfMaxHitpointsGainedPerDay = 0.3f,
			ActiveStealthRating = 1f,
			Carcass = "item:swampDemonTreeCarcass",
			Castes = new List<CasteType>
			{
				new CasteType
				{
					KeyName = "male",
					Reproduction = Reproduction.Male,
					Edge = 0.51f,
					HeightMean = 2f,
					HeightStandardDeviation = 0.08f,
					WeightMean = 110f,
					WeightStandardDeviation = 0.15f,
					AgeGroupTypes = new List<AgeGroupType>
					{
						new AgeGroupType
						{
							Name = "Baby",
							AIAgeGroup = AIAgeGroup.Baby,
							CanReproduce = false,
							Edge = 0.5f,
							HeightTargetModifier = 0.05f,
							WeightTargetModifier = 0.03f,
							ModelScaleFraction = 0.3f,
							NeedTypes = new NeedType[1] { needType8 }
						},
						new AgeGroupType
						{
							Name = "Child",
							AIAgeGroup = AIAgeGroup.Child,
							CanReproduce = false,
							Edge = 1f,
							HeightTargetModifier = 0.6f,
							WeightTargetModifier = 0.5f,
							ModelScaleFraction = 0.6f,
							NeedTypes = new NeedType[1] { needType8 }
						},
						new AgeGroupType
						{
							Name = "Young adult",
							AIAgeGroup = AIAgeGroup.YoungAdult,
							CanReproduce = false,
							Edge = 5f,
							HeightTargetModifier = 0.97f,
							WeightTargetModifier = 0.92f,
							ModelScaleFraction = 0.85f,
							NeedTypes = new NeedType[1] { needType8 }
						},
						new AgeGroupType
						{
							Name = "Adult",
							AIAgeGroup = AIAgeGroup.Adult,
							CanReproduce = true,
							Edge = 28f,
							HeightTargetModifier = 1f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f,
							NeedTypes = new NeedType[1] { needType8 }
						},
						new AgeGroupType
						{
							Name = "Old",
							AIAgeGroup = AIAgeGroup.Old,
							CanReproduce = true,
							Edge = 30f,
							HeightTargetModifier = 0.9f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f,
							NeedTypes = new NeedType[1] { needType8 }
						}
					}
				},
				new CasteType
				{
					KeyName = "female",
					Reproduction = Reproduction.Female,
					Edge = 1f,
					HeightMean = 1.9f,
					HeightStandardDeviation = 0.05f,
					WeightMean = 100f,
					WeightStandardDeviation = 0.1f,
					AgeGroupTypes = new List<AgeGroupType>
					{
						new AgeGroupType
						{
							Name = "Baby",
							AIAgeGroup = AIAgeGroup.Baby,
							CanReproduce = false,
							Edge = 0.5f,
							HeightTargetModifier = 0.05f,
							WeightTargetModifier = 0.03f,
							ModelScaleFraction = 0.3f,
							NeedTypes = new NeedType[1] { needType8 }
						},
						new AgeGroupType
						{
							Name = "Child",
							AIAgeGroup = AIAgeGroup.Child,
							CanReproduce = false,
							Edge = 1f,
							HeightTargetModifier = 0.6f,
							WeightTargetModifier = 0.5f,
							ModelScaleFraction = 0.6f,
							NeedTypes = new NeedType[1] { needType8 }
						},
						new AgeGroupType
						{
							Name = "Young adult",
							AIAgeGroup = AIAgeGroup.YoungAdult,
							CanReproduce = false,
							Edge = 5f,
							HeightTargetModifier = 0.97f,
							WeightTargetModifier = 0.92f,
							ModelScaleFraction = 0.85f,
							NeedTypes = new NeedType[1] { needType8 }
						},
						new AgeGroupType
						{
							Name = "Adult",
							AIAgeGroup = AIAgeGroup.Adult,
							CanReproduce = true,
							Edge = 28f,
							HeightTargetModifier = 1f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f,
							NeedTypes = new NeedType[1] { needType8 }
						},
						new AgeGroupType
						{
							Name = "Old",
							AIAgeGroup = AIAgeGroup.Old,
							CanReproduce = false,
							Edge = 30f,
							HeightTargetModifier = 0.9f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f,
							NeedTypes = new NeedType[1] { needType8 }
						}
					}
				}
			}
		};
		listOfEntityTypes.Add(entityType15);
		entityType = new EntityType("entity:forestGuardian");
		entityType.Name = "Forest guardian";
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "forestguardian",
				GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]> { 
				{
					"normal",
					new GaitAnimationBracket[1]
					{
						new GaitAnimationBracket
						{
							MinimumSpeed = 5f,
							MaximumSpeed = 20f,
							StrideLength = 22.2f,
							StrideDuration = 1f,
							AnimationKey = "gaitWalk"
						}
					}
				} },
				AnimConditions = new AnimConditionInfo[6]
				{
					new AnimConditionInfo
					{
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Moving
						},
						GaitSetKey = "normal"
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "idle" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Idle
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "eat" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Eating
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "collapse" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying,
							Modifiers = new BitMask64(typeof(AnimModifier), 12)
						},
						Looping = Looping.No
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "dead" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "dead" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying,
							Modifiers = new BitMask64(typeof(AnimModifier), 13)
						}
					}
				}
			}
		};
		entityType.LocomotorType = new LocomotorType
		{
			MaxAngularSpeed = (float)Math.PI * 2f / 25f,
			FourSidedSymmetry = true,
			MeleeRadius = 22f,
			LeggedLocomotorType = new LeggedLocomotorType
			{
				TerrainNegateFactor = 0.9f,
				WalkSlowSpeed = 1.71f,
				WalkNormalSpeed = 1.71f,
				WalkFastSpeed = 1.71f
			},
			CollisionResponderType = new CollisionResponderType
			{
				AgentCollisionResponderType = new AgentCollisionResponderType()
			}
		};
		entityType.SensorType = new SensorType
		{
			Range = 350f,
			RangeAtNight = 200f,
			DetectionTypeKey = "defaultDetection"
		};
		entityType.BodyType = GameData.Instance.AllBodyTypes["forestguardian"];
		EntityType entityType16 = entityType;
		entityType16.IntelligenceType = new IntelligenceType
		{
			IsMobile = true,
			CanAttack = false,
			CanUseWeapons = false,
			CanHunt = false,
			CanScout = true,
			CanExamine = true,
			CanPatrol = false,
			CanHaul = false,
			Boldness = 0.4f,
			MemoryInDays = 1f,
			StrengthRating = StrengthRating.WeakerThanHumans,
			InterestInTriggerTypes = new string[2] { "mineTrigger", "spikeTrapTrigger" },
			AggroRange = 0f
		};
		entityType16.BiologicalType = new BiologicalType
		{
			OrderKey = "forestGuardianOrder",
			OxygenAndMuscleEnergyIncreaseRatePerDay = 4f,
			TimeToConsumeFullMealInDays = 0.0125f,
			StomachSizeFractionOfEntityBulk = 0.3f,
			StomachContentsDecreaseRatePerDay = 3f,
			ActiveStealthRating = 0.1f,
			MaxRegainLimit = 0.5f,
			FractionOfMaxHitpointsGainedPerDay = 0.3f,
			Carcass = "item:forestGuardianCarcass",
			RaceTypes = new RaceType[3]
			{
				new RaceType
				{
					KeyName = "pale",
					Name = "Pale race",
					PortraitSkinType = "Pale",
					PrimaryColor = "F7F4C5".ToColorVector3(),
					ModelBasicTextureName = "BushbackPaleTexture",
					Edge = 0.3f,
					ModelScale = 2.3f
				},
				new RaceType
				{
					KeyName = "dark",
					Name = "Dark race",
					PortraitSkinType = "Dark",
					PrimaryColor = "564920".ToColorVector3(),
					ModelBasicTextureName = "BushbackDarkTexture",
					Edge = 0.6f,
					ModelScale = 2.3f
				},
				new RaceType
				{
					KeyName = "black",
					Name = "",
					PortraitSkinType = "Black",
					PrimaryColor = "3F3411".ToColorVector3(),
					Edge = 0.9f,
					ModelScale = 2.3f
				}
			},
			Castes = new List<CasteType>
			{
				new CasteType
				{
					KeyName = "male",
					Reproduction = Reproduction.Male,
					Edge = 0.51f,
					HeightMean = 2f,
					HeightStandardDeviation = 0.08f,
					WeightMean = 100f,
					WeightStandardDeviation = 0.15f,
					AgeGroupTypes = new List<AgeGroupType>
					{
						new AgeGroupType
						{
							Name = "Baby",
							AIAgeGroup = AIAgeGroup.Baby,
							CanReproduce = false,
							Edge = 1.5f,
							HeightTargetModifier = 0.05f,
							WeightTargetModifier = 0.03f,
							ModelScaleFraction = 0.3f
						},
						new AgeGroupType
						{
							Name = "Child",
							AIAgeGroup = AIAgeGroup.Child,
							CanReproduce = false,
							Edge = 11f,
							HeightTargetModifier = 0.6f,
							WeightTargetModifier = 0.5f,
							ModelScaleFraction = 0.6f
						},
						new AgeGroupType
						{
							Name = "Young adult",
							AIAgeGroup = AIAgeGroup.YoungAdult,
							CanReproduce = false,
							Edge = 16f,
							HeightTargetModifier = 0.97f,
							WeightTargetModifier = 0.92f,
							ModelScaleFraction = 0.85f
						},
						new AgeGroupType
						{
							Name = "Adult",
							AIAgeGroup = AIAgeGroup.Adult,
							CanReproduce = true,
							Edge = 72f,
							HeightTargetModifier = 1f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f
						},
						new AgeGroupType
						{
							Name = "Old",
							AIAgeGroup = AIAgeGroup.Old,
							CanReproduce = true,
							Edge = 200f,
							HeightTargetModifier = 0.9f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f
						}
					}
				},
				new CasteType
				{
					KeyName = "female",
					Reproduction = Reproduction.Female,
					Edge = 1f,
					HeightMean = 1.9f,
					HeightStandardDeviation = 0.05f,
					WeightMean = 90f,
					WeightStandardDeviation = 0.1f,
					AgeGroupTypes = new List<AgeGroupType>
					{
						new AgeGroupType
						{
							Name = "Baby",
							AIAgeGroup = AIAgeGroup.Baby,
							CanReproduce = false,
							Edge = 1.5f,
							HeightTargetModifier = 0.05f,
							WeightTargetModifier = 0.03f,
							ModelScaleFraction = 0.3f
						},
						new AgeGroupType
						{
							Name = "Child",
							AIAgeGroup = AIAgeGroup.Child,
							CanReproduce = false,
							Edge = 11f,
							HeightTargetModifier = 0.6f,
							WeightTargetModifier = 0.5f,
							ModelScaleFraction = 0.6f
						},
						new AgeGroupType
						{
							Name = "Young adult",
							AIAgeGroup = AIAgeGroup.YoungAdult,
							CanReproduce = false,
							Edge = 16f,
							HeightTargetModifier = 0.97f,
							WeightTargetModifier = 0.92f,
							ModelScaleFraction = 0.85f
						},
						new AgeGroupType
						{
							Name = "Adult",
							AIAgeGroup = AIAgeGroup.Adult,
							CanReproduce = true,
							Edge = 72f,
							HeightTargetModifier = 1f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f
						},
						new AgeGroupType
						{
							Name = "Old",
							AIAgeGroup = AIAgeGroup.Old,
							CanReproduce = false,
							Edge = 200f,
							HeightTargetModifier = 0.9f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f
						}
					}
				}
			}
		};
		listOfEntityTypes.Add(entityType16);
		needType = new NeedType();
		needType.KeyName = "foodEnergy";
		needType.FoodNeedType = new FoodNeedType
		{
			FoodNutrient = "foodEnergy",
			RequiredNutrientsAsFractionOfEntityBulk = 0.1f
		};
		needType.DecreasePerDay = new NormalDistribution
		{
			Mean = 2.0,
			StandardDeviation = 0.019999999552965164
		};
		needType.LimitForDecreasedEnergy = 0.1f;
		needType.DecreasedEnergyWeight = 0.6f;
		needType.PhysicalEffects = new PhysicalEffects
		{
			DaysAtZeroCausingCollapse = 10000f,
			DaysAtZeroCausingDeath = 10000f,
			DaysAtZeroDecreaseFactor = 1f,
			UseExertionFactorToDecrease = true
		};
		NeedType needType9 = needType;
		entityType = new EntityType("entity:whiteThunderChicken");
		entityType.Name = "White thunder chicken";
		entityType.ThumbnailSmall = "HUD_thumbnail_whiteThunderChicken";
		entityType.SummaryDescription = "Quick-running animal";
		entityType.Description = "\n FEEDING CLASSIFICATION: Omnivore. Prefers to eat smaller bugs.\n \nHEIGHT: 60 cm\n \nANATOMY\n Like all thunder chickens, the white feeds using retractable tentacles around its mouth while an antenna on top of its head detects dangers. \n \nBEHAVIOR\n  When threatened it will quickly run away, sometimes emitting a loud sound. Occasionally solitary, mostly it forms small herds with other species of thunder chicken.\n \nENEMIES\n Hunted by twinkler quadites.\n \nSURVIVAL GUIDE NOTES\n The thunder chickens pose no threat to us and the flesh is edible: Tasty and a good source of protein.";
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				ModelScale = 1.5f,
				AssetName = "thunderchicken",
				ModelBasicTextureName = "ThunderchickenPaleTexture",
				GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]> { 
				{
					"normal",
					new GaitAnimationBracket[1]
					{
						new GaitAnimationBracket
						{
							MinimumSpeed = 0f,
							MaximumSpeed = 77f,
							StrideLength = 11f,
							StrideDuration = 0.4f,
							AnimationKey = "gaitWalk"
						}
					}
				} },
				AnimConditions = new AnimConditionInfo[5]
				{
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							AdditionalAnimations1 = new string[1] { "hidden" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Moving
						},
						GaitSetKey = "normal"
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "idle" },
							AdditionalAnimations1 = new string[1] { "look" },
							AdditionalAnimations2 = new string[1] { "eat" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Idle
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "idle" },
							AdditionalAnimations2 = new string[1] { "eat" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Eating
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "dying" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "dead" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying,
							Modifiers = new BitMask64(typeof(AnimModifier), 13)
						}
					}
				}
			}
		};
		entityType.LocomotorType = new LocomotorType
		{
			MaxAngularSpeed = 4.712389f,
			MeleeRadius = 10f,
			LeggedLocomotorType = new LeggedLocomotorType
			{
				TerrainNegateFactor = 0.5f,
				WalkSlowSpeed = 20f,
				WalkNormalSpeed = 43f,
				WalkFastSpeed = 64f
			},
			CollisionResponderType = new CollisionResponderType
			{
				AgentCollisionResponderType = new AgentCollisionResponderType()
			}
		};
		entityType.SensorType = new SensorType
		{
			Range = 354f,
			RangeAtNight = 200f,
			DetectionTypeKey = "defaultDetection"
		};
		entityType.BodyType = GameData.Instance.AllBodyTypes["thunderchicken"];
		entityType.ContainerType = new AgentStorageType
		{
			ItemStorageType = new ItemStorageType(0.5f),
			StomachStorageType = new ItemStorageType(0.07f)
		};
		EntityType entityType17 = entityType;
		entityType17.IntelligenceType = new IntelligenceType
		{
			ForageAndHuntingRadius = 250,
			IsMobile = true,
			CanAttack = false,
			CanUseWeapons = false,
			CanHunt = false,
			CanScout = true,
			CanExamine = true,
			CanPatrol = false,
			CanHaul = false,
			IsPredator = false,
			ContainerTransactTag = "chickenTransact",
			StrengthRating = StrengthRating.WeakerThanHumans,
			InterestInTriggerTypes = new string[3] { "mineTrigger", "smallImprovisedTrapTrigger", "spikeTrapTrigger" },
			Courage = 0f,
			MemoryInDays = 0.05f,
			Boldness = 1f,
			AggroRange = 0f
		};
		entityType17.BiologicalType = new BiologicalType
		{
			OrderKey = "thunderChickenOrder",
			OxygenAndMuscleEnergyIncreaseRatePerDay = 10f,
			FoodItemTagsThatCanBeConsumed = new string[3] { "inedibleVegi", "edibleVegi", "spoiledMeal" },
			TimeToConsumeFullMealInDays = 0.0125f,
			StomachSizeFractionOfEntityBulk = 0.2f,
			StomachContentsDecreaseRatePerDay = 3f,
			ActiveStealthRating = 0.1f,
			MaxRegainLimit = 0.5f,
			FractionOfMaxHitpointsGainedPerDay = 0.3f,
			Carcass = "item:thunderChickenCarcass",
			Castes = new List<CasteType>
			{
				new CasteType
				{
					KeyName = "male",
					Reproduction = Reproduction.Male,
					Edge = 0.51f,
					HeightMean = 0.5f,
					HeightStandardDeviation = 0.02f,
					WeightMean = 30f,
					WeightStandardDeviation = 3f,
					AgeGroupTypes = new List<AgeGroupType>
					{
						new AgeGroupType
						{
							Name = "Chick",
							AIAgeGroup = AIAgeGroup.Baby,
							CanReproduce = false,
							Edge = 0.5f,
							HeightTargetModifier = 0.05f,
							WeightTargetModifier = 0.03f,
							ModelScaleFraction = 0.3f,
							NeedTypes = new NeedType[1] { needType9 }
						},
						new AgeGroupType
						{
							Name = "Chick",
							AIAgeGroup = AIAgeGroup.Child,
							CanReproduce = false,
							Edge = 1f,
							HeightTargetModifier = 0.6f,
							WeightTargetModifier = 0.5f,
							ModelScaleFraction = 0.6f,
							NeedTypes = new NeedType[1] { needType9 }
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.YoungAdult,
							CanReproduce = false,
							Edge = 2f,
							HeightTargetModifier = 0.97f,
							WeightTargetModifier = 0.92f,
							ModelScaleFraction = 0.85f,
							NeedTypes = new NeedType[1] { needType9 }
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.Adult,
							CanReproduce = true,
							Edge = 9f,
							HeightTargetModifier = 1f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f,
							NeedTypes = new NeedType[1] { needType9 }
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.Old,
							CanReproduce = true,
							Edge = 10f,
							HeightTargetModifier = 0.9f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f,
							NeedTypes = new NeedType[1] { needType9 }
						}
					}
				},
				new CasteType
				{
					KeyName = "female",
					Reproduction = Reproduction.Female,
					Edge = 1f,
					HeightMean = 0.45f,
					HeightStandardDeviation = 0.02f,
					WeightMean = 28f,
					WeightStandardDeviation = 2f,
					AgeGroupTypes = new List<AgeGroupType>
					{
						new AgeGroupType
						{
							Name = "Chick",
							AIAgeGroup = AIAgeGroup.Baby,
							CanReproduce = false,
							Edge = 0.5f,
							HeightTargetModifier = 0.05f,
							WeightTargetModifier = 0.03f,
							ModelScaleFraction = 0.3f,
							NeedTypes = new NeedType[1] { needType9 }
						},
						new AgeGroupType
						{
							Name = "Chick",
							AIAgeGroup = AIAgeGroup.Child,
							CanReproduce = false,
							Edge = 1f,
							HeightTargetModifier = 0.6f,
							WeightTargetModifier = 0.5f,
							ModelScaleFraction = 0.6f,
							NeedTypes = new NeedType[1] { needType9 }
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.YoungAdult,
							CanReproduce = false,
							Edge = 2f,
							HeightTargetModifier = 0.97f,
							WeightTargetModifier = 0.92f,
							ModelScaleFraction = 0.85f,
							NeedTypes = new NeedType[1] { needType9 }
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.Adult,
							CanReproduce = true,
							Edge = 10f,
							HeightTargetModifier = 1f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f,
							NeedTypes = new NeedType[1] { needType9 }
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.Old,
							CanReproduce = false,
							Edge = 11f,
							HeightTargetModifier = 0.9f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f,
							NeedTypes = new NeedType[1] { needType9 }
						}
					}
				}
			}
		};
		listOfEntityTypes.Add(entityType17);
		entityType = new EntityType("entity:pygmyThunderChicken");
		entityType.Name = "Pygmy Thunder chicken";
		entityType.ThumbnailSmall = "HUD_thumbnail_thunderChicken";
		entityType.SummaryDescription = "Quick-running animal";
		entityType.Description = "\n FEEDING CLASSIFICATION: Herbivore.\n \n HEIGHT: up to 40 cm\n \n ANATOMY\n Smaller species of thunder chicken. Like all thunder chickens, feeds using retractable tentacles around its mouth while an antenna on top of its head detects dangers. \n \n BEHAVIOR\n  Quick to flee. Forms small herds with other species of thunder chicken.\n \n THREAT LEVEL\n None.\n \n ENEMIES\n Numerous.\n \n NOTES\n Has been determined edible.";
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				ModelScale = 1.25f,
				AssetName = "thunderchicken",
				ModelBasicTextureName = "ThunderchickenDarkTexture",
				GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]> { 
				{
					"normal",
					new GaitAnimationBracket[1]
					{
						new GaitAnimationBracket
						{
							MinimumSpeed = 0f,
							MaximumSpeed = 77f,
							StrideLength = 11f,
							StrideDuration = 0.4f,
							AnimationKey = "gaitWalk"
						}
					}
				} },
				AnimConditions = new AnimConditionInfo[5]
				{
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							AdditionalAnimations1 = new string[1] { "hidden" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Moving
						},
						GaitSetKey = "normal"
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "idle" },
							AdditionalAnimations1 = new string[1] { "look" },
							AdditionalAnimations2 = new string[1] { "eat" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Idle
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "idle" },
							AdditionalAnimations2 = new string[1] { "eat" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Eating
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "dying" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "dead" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying,
							Modifiers = new BitMask64(typeof(AnimModifier), 13)
						}
					}
				}
			}
		};
		entityType.LocomotorType = new LocomotorType
		{
			MaxAngularSpeed = 4.712389f,
			MeleeRadius = 10f,
			LeggedLocomotorType = new LeggedLocomotorType
			{
				TerrainNegateFactor = 0.7f,
				WalkSlowSpeed = 30f,
				WalkNormalSpeed = 43f,
				WalkFastSpeed = 64f
			},
			CollisionResponderType = new CollisionResponderType
			{
				AgentCollisionResponderType = new AgentCollisionResponderType()
			}
		};
		entityType.SensorType = new SensorType
		{
			Range = 354f,
			RangeAtNight = 200f,
			DetectionTypeKey = "defaultDetection"
		};
		entityType.BodyType = GameData.Instance.AllBodyTypes["thunderchicken"];
		entityType.ContainerType = new AgentStorageType
		{
			ItemStorageType = new ItemStorageType(0.5f),
			StomachStorageType = new ItemStorageType(0.07f)
		};
		entityType17 = entityType;
		entityType17.IntelligenceType = new IntelligenceType
		{
			IsMobile = true,
			CanAttack = false,
			CanUseWeapons = false,
			CanHunt = false,
			CanScout = true,
			CanExamine = true,
			CanPatrol = false,
			CanHaul = false,
			IsPredator = false,
			ContainerTransactTag = "chickenTransact",
			StrengthRating = StrengthRating.WeakerThanHumans,
			InterestInTriggerTypes = new string[3] { "mineTrigger", "smallImprovisedTrapTrigger", "spikeTrapTrigger" },
			Courage = 0f,
			MemoryInDays = 0.05f,
			Boldness = 1f,
			AggroRange = 0f
		};
		entityType17.BiologicalType = new BiologicalType
		{
			OxygenAndMuscleEnergyIncreaseRatePerDay = 10f,
			TimeToConsumeFullMealInDays = 0.0125f,
			StomachSizeFractionOfEntityBulk = 0.2f,
			StomachContentsDecreaseRatePerDay = 3f,
			ActiveStealthRating = 0.1f,
			MaxRegainLimit = 0.5f,
			FractionOfMaxHitpointsGainedPerDay = 0.3f,
			Carcass = "item:thunderChickenCarcass",
			Castes = new List<CasteType>
			{
				new CasteType
				{
					KeyName = "male",
					Reproduction = Reproduction.Male,
					Edge = 0.51f,
					HeightMean = 0.5f,
					HeightStandardDeviation = 0.02f,
					WeightMean = 30f,
					WeightStandardDeviation = 3f,
					AgeGroupTypes = new List<AgeGroupType>
					{
						new AgeGroupType
						{
							Name = "Chick",
							AIAgeGroup = AIAgeGroup.Baby,
							CanReproduce = false,
							Edge = 0.5f,
							HeightTargetModifier = 0.05f,
							WeightTargetModifier = 0.03f,
							ModelScaleFraction = 0.5f,
							NeedTypes = new NeedType[1] { needType9 }
						},
						new AgeGroupType
						{
							Name = "Chick",
							AIAgeGroup = AIAgeGroup.Child,
							CanReproduce = false,
							Edge = 0.8f,
							HeightTargetModifier = 0.6f,
							WeightTargetModifier = 0.5f,
							ModelScaleFraction = 0.6f,
							NeedTypes = new NeedType[1] { needType9 }
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.YoungAdult,
							CanReproduce = false,
							Edge = 2f,
							HeightTargetModifier = 0.97f,
							WeightTargetModifier = 0.92f,
							ModelScaleFraction = 0.85f,
							NeedTypes = new NeedType[1] { needType9 }
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.Adult,
							CanReproduce = true,
							Edge = 8f,
							HeightTargetModifier = 1f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f,
							NeedTypes = new NeedType[1] { needType9 }
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.Old,
							CanReproduce = true,
							Edge = 9f,
							HeightTargetModifier = 0.9f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f,
							NeedTypes = new NeedType[1] { needType9 }
						}
					}
				},
				new CasteType
				{
					KeyName = "female",
					Reproduction = Reproduction.Female,
					Edge = 1f,
					HeightMean = 0.45f,
					HeightStandardDeviation = 0.02f,
					WeightMean = 28f,
					WeightStandardDeviation = 2f,
					AgeGroupTypes = new List<AgeGroupType>
					{
						new AgeGroupType
						{
							Name = "Chick",
							AIAgeGroup = AIAgeGroup.Baby,
							CanReproduce = false,
							Edge = 0.5f,
							HeightTargetModifier = 0.05f,
							WeightTargetModifier = 0.03f,
							ModelScaleFraction = 0.3f,
							NeedTypes = new NeedType[1] { needType9 }
						},
						new AgeGroupType
						{
							Name = "Chick",
							AIAgeGroup = AIAgeGroup.Child,
							CanReproduce = false,
							Edge = 0.8f,
							HeightTargetModifier = 0.6f,
							WeightTargetModifier = 0.5f,
							ModelScaleFraction = 0.6f,
							NeedTypes = new NeedType[1] { needType9 }
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.YoungAdult,
							CanReproduce = false,
							Edge = 2f,
							HeightTargetModifier = 0.97f,
							WeightTargetModifier = 0.92f,
							ModelScaleFraction = 0.85f,
							NeedTypes = new NeedType[1] { needType9 }
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.Adult,
							CanReproduce = true,
							Edge = 8f,
							HeightTargetModifier = 1f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f,
							NeedTypes = new NeedType[1] { needType9 }
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.Old,
							CanReproduce = false,
							Edge = 9f,
							HeightTargetModifier = 0.9f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f,
							NeedTypes = new NeedType[1] { needType9 }
						}
					}
				}
			}
		};
		listOfEntityTypes.Add(entityType17);
		entityType = new EntityType("entity:bajingan");
		entityType.Name = "Bajingan";
		entityType.ThumbnailSmall = "HUD_thumbnail_thunderChickenThin";
		entityType.SummaryDescription = "Opportunistic scavenger (vermin). Is edible.";
		entityType.Description = "\n FEEDING CLASSIFICATION: Omnivore.\n \n HEIGHT: up to 40 cm\n \n ANATOMY\n Lithe species of thunder chicken. Feeds using its mouth (instead of tentacles), which makes it able to feed at a much quicker rate than other thunder chickens. \n \n BEHAVIOR\n  Despite its size, this scavenger has a voracious appetite no doubt owing to its very high metabolism. \n \n THREAT LEVEL\n None.\n \n SURVIVAL GUIDE NOTES\n Keep food supplies hidden if camping in this animal's habitat.";
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				ModelScale = 1.25f,
				AssetName = "thunderChickenThin",
				ModelBasicTextureName = "ThunderchickenThinTexture1",
				GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]> { 
				{
					"normal",
					new GaitAnimationBracket[1]
					{
						new GaitAnimationBracket
						{
							MinimumSpeed = 0f,
							MaximumSpeed = 77f,
							StrideLength = 11f,
							StrideDuration = 0.4f,
							AnimationKey = "gaitWalk"
						}
					}
				} },
				AnimConditions = new AnimConditionInfo[5]
				{
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							AdditionalAnimations1 = new string[1] { "hidden" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Moving
						},
						GaitSetKey = "normal"
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "idle" },
							AdditionalAnimations1 = new string[1] { "look" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Idle
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "eatNoTentacle" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Eating
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "dying" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "dead" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying,
							Modifiers = new BitMask64(typeof(AnimModifier), 13)
						}
					}
				}
			}
		};
		entityType.LocomotorType = new LocomotorType
		{
			MaxAngularSpeed = 4.712389f,
			MeleeRadius = 10f,
			LeggedLocomotorType = new LeggedLocomotorType
			{
				TerrainNegateFactor = 0.7f,
				WalkSlowSpeed = 30f,
				WalkNormalSpeed = 43f,
				WalkFastSpeed = 64f
			},
			CollisionResponderType = new CollisionResponderType
			{
				AgentCollisionResponderType = new AgentCollisionResponderType()
			}
		};
		entityType.SensorType = new SensorType
		{
			Range = 354f,
			RangeAtNight = 200f,
			DetectionTypeKey = "defaultDetection"
		};
		entityType.BodyType = GameData.Instance.AllBodyTypes["thunderchicken"];
		entityType.ContainerType = new AgentStorageType
		{
			ItemStorageType = new ItemStorageType(0.5f),
			StomachStorageType = new ItemStorageType(0.07f)
		};
		entityType17 = entityType;
		entityType17.IntelligenceType = new IntelligenceType
		{
			IsMobile = true,
			CanAttack = false,
			CanUseWeapons = false,
			CanHunt = false,
			CanScout = true,
			CanExamine = true,
			CanPatrol = false,
			CanHaul = false,
			ContainerTransactTag = "chickenTransact",
			StrengthRating = StrengthRating.None,
			InterestInTriggerTypes = new string[4] { "mineTrigger", "smallImprovisedTrapTrigger", "spikeTrapTrigger", "animalMigrateTrigger" },
			Courage = 0f,
			IsPredator = false,
			MemoryInDays = 0.3f,
			Boldness = 1.49f,
			AggroRange = 0f
		};
		entityType17.BiologicalType = new BiologicalType
		{
			OxygenAndMuscleEnergyIncreaseRatePerDay = 10f,
			FoodItemTagsThatCanBeConsumed = new string[7] { "cookedMeat", "rawMeat", "smallRawMeat", "rottenMeat", "inedibleMeat", "edibleVegi", "inedibleVegi" },
			ExtractionProcessTypes = new string[13]
			{
				"extractMudWormMeat", "extractPatricianMeat", "extractLeafCutterMeat", "extractThunderChickenMeat", "extractBinalRatMeat", "extractTwinklerMeat", "extractBushDragonMeat", "extractSwampDemonTreeMeat", "extractDemonTreeMeat", "extractMegapodMeat",
				"extractWhipjawMeat", "extractSpikePlantMeat", "extractForestGuardianMeat"
			},
			TimeToConsumeFullMealInDays = 0.0125f,
			StomachSizeFractionOfEntityBulk = 0.2f,
			StomachContentsDecreaseRatePerDay = 3f,
			ActiveStealthRating = 0.1f,
			IsVermin = true,
			MaxRegainLimit = 0.5f,
			FractionOfMaxHitpointsGainedPerDay = 0.3f,
			Carcass = "item:thunderChickenCarcass",
			Castes = new List<CasteType>
			{
				new CasteType
				{
					KeyName = "male",
					Reproduction = Reproduction.Male,
					Edge = 0.51f,
					HeightMean = 0.5f,
					HeightStandardDeviation = 0.02f,
					WeightMean = 30f,
					WeightStandardDeviation = 3f,
					AgeGroupTypes = new List<AgeGroupType>
					{
						new AgeGroupType
						{
							Name = "Chick",
							AIAgeGroup = AIAgeGroup.Baby,
							CanReproduce = false,
							Edge = 0.5f,
							HeightTargetModifier = 0.05f,
							WeightTargetModifier = 0.03f,
							ModelScaleFraction = 0.5f,
							NeedTypes = new NeedType[1] { needType9 }
						},
						new AgeGroupType
						{
							Name = "Chick",
							AIAgeGroup = AIAgeGroup.Child,
							CanReproduce = false,
							Edge = 1f,
							HeightTargetModifier = 0.6f,
							WeightTargetModifier = 0.5f,
							ModelScaleFraction = 0.6f,
							NeedTypes = new NeedType[1] { needType9 }
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.YoungAdult,
							CanReproduce = false,
							Edge = 4f,
							HeightTargetModifier = 0.97f,
							WeightTargetModifier = 0.92f,
							ModelScaleFraction = 0.85f,
							NeedTypes = new NeedType[1] { needType9 }
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.Adult,
							CanReproduce = true,
							Edge = 12f,
							HeightTargetModifier = 1f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f,
							NeedTypes = new NeedType[1] { needType9 }
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.Old,
							CanReproduce = true,
							Edge = 14f,
							HeightTargetModifier = 0.9f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f,
							NeedTypes = new NeedType[1] { needType9 }
						}
					}
				},
				new CasteType
				{
					KeyName = "female",
					Reproduction = Reproduction.Female,
					Edge = 1f,
					HeightMean = 0.45f,
					HeightStandardDeviation = 0.02f,
					WeightMean = 28f,
					WeightStandardDeviation = 2f,
					AgeGroupTypes = new List<AgeGroupType>
					{
						new AgeGroupType
						{
							Name = "Chick",
							AIAgeGroup = AIAgeGroup.Baby,
							CanReproduce = false,
							Edge = 0.5f,
							HeightTargetModifier = 0.05f,
							WeightTargetModifier = 0.03f,
							ModelScaleFraction = 0.3f,
							NeedTypes = new NeedType[1] { needType9 }
						},
						new AgeGroupType
						{
							Name = "Chick",
							AIAgeGroup = AIAgeGroup.Child,
							CanReproduce = false,
							Edge = 1f,
							HeightTargetModifier = 0.6f,
							WeightTargetModifier = 0.5f,
							ModelScaleFraction = 0.6f,
							NeedTypes = new NeedType[1] { needType9 }
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.YoungAdult,
							CanReproduce = false,
							Edge = 4f,
							HeightTargetModifier = 0.97f,
							WeightTargetModifier = 0.92f,
							ModelScaleFraction = 0.85f,
							NeedTypes = new NeedType[1] { needType9 }
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.Adult,
							CanReproduce = true,
							Edge = 12f,
							HeightTargetModifier = 1f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f,
							NeedTypes = new NeedType[1] { needType9 }
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.Old,
							CanReproduce = false,
							Edge = 14f,
							HeightTargetModifier = 0.9f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f,
							NeedTypes = new NeedType[1] { needType9 }
						}
					}
				}
			}
		};
		listOfEntityTypes.Add(entityType17);
		entityType = new EntityType("entity:studdedThunderChicken");
		entityType.Name = "Studded thunder chicken";
		entityType.ThumbnailSmall = "HUD_thumbnail_thunderChickenBulky";
		entityType.SummaryDescription = "Lightly armored herbivore";
		entityType.Description = "\n FEEDING CLASSIFICATION: Herbivore.\n \n HEIGHT: up to 70 cm\n \n ANATOMY\n Species of thunder chicken with a sturdier build and some armor. Feeds using retractable tentacles around its mouth.\n \nBEHAVIOR\n  Despite its protective armor, it is no less timid than other thunder chickens. Lives a mostly solitary life.\n \n THREAT LEVEL\n None.\n \n ENEMIES\n Preyed on by spoak dendronts and several other species.\n \n NOTES\n Edible by humans.";
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				ModelScale = 1.25f,
				AssetName = "thunderChickenBulky",
				ModelBasicTextureName = "ThunderchickenBulkyTexture1",
				GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]> { 
				{
					"normal",
					new GaitAnimationBracket[1]
					{
						new GaitAnimationBracket
						{
							MinimumSpeed = 0f,
							MaximumSpeed = 77f,
							StrideLength = 11f,
							StrideDuration = 0.4f,
							AnimationKey = "gaitWalk"
						}
					}
				} },
				AnimConditions = new AnimConditionInfo[6]
				{
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							AdditionalAnimations1 = new string[1] { "hidden" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Moving
						},
						GaitSetKey = "normal"
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "idle" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Idle
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "idle" },
							AdditionalAnimations2 = new string[1] { "eat" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Eating
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "dying" },
							Sounds = new string[1] { "aliens/spacechicken" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying,
							Modifiers = new BitMask64(typeof(AnimModifier), 12)
						},
						Looping = Looping.No
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "dying" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "dead" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying,
							Modifiers = new BitMask64(typeof(AnimModifier), 13)
						}
					}
				}
			}
		};
		entityType.LocomotorType = new LocomotorType
		{
			MaxAngularSpeed = 4.712389f,
			MeleeRadius = 10f,
			LeggedLocomotorType = new LeggedLocomotorType
			{
				TerrainNegateFactor = 0.7f,
				WalkSlowSpeed = 30f,
				WalkNormalSpeed = 43f,
				WalkFastSpeed = 64f
			},
			CollisionResponderType = new CollisionResponderType
			{
				AgentCollisionResponderType = new AgentCollisionResponderType()
			}
		};
		entityType.SensorType = new SensorType
		{
			Range = 354f,
			RangeAtNight = 200f,
			DetectionTypeKey = "defaultDetection"
		};
		entityType.BodyType = GameData.Instance.AllBodyTypes["thunderchicken"];
		entityType.ContainerType = new AgentStorageType
		{
			ItemStorageType = new ItemStorageType(0.5f),
			StomachStorageType = new ItemStorageType(0.07f)
		};
		entityType17 = entityType;
		entityType17.IntelligenceType = new IntelligenceType
		{
			IsMobile = true,
			CanAttack = false,
			CanUseWeapons = false,
			CanHunt = false,
			CanScout = true,
			CanExamine = true,
			CanPatrol = false,
			CanHaul = false,
			IsPredator = false,
			ContainerTransactTag = "chickenTransact",
			StrengthRating = StrengthRating.WeakerThanHumans,
			InterestInTriggerTypes = new string[3] { "mineTrigger", "smallImprovisedTrapTrigger", "spikeTrapTrigger" },
			Courage = 0f,
			MemoryInDays = 0.2f,
			Boldness = 1f,
			AggroRange = 0f
		};
		entityType17.BiologicalType = new BiologicalType
		{
			OxygenAndMuscleEnergyIncreaseRatePerDay = 10f,
			TimeToConsumeFullMealInDays = 0.0125f,
			StomachSizeFractionOfEntityBulk = 0.2f,
			StomachContentsDecreaseRatePerDay = 3f,
			ActiveStealthRating = 0.1f,
			MaxRegainLimit = 0.5f,
			FractionOfMaxHitpointsGainedPerDay = 0.3f,
			Carcass = "item:thunderChickenCarcass",
			Castes = new List<CasteType>
			{
				new CasteType
				{
					KeyName = "male",
					Reproduction = Reproduction.Male,
					Edge = 0.51f,
					HeightMean = 0.5f,
					HeightStandardDeviation = 0.02f,
					WeightMean = 30f,
					WeightStandardDeviation = 3f,
					AgeGroupTypes = new List<AgeGroupType>
					{
						new AgeGroupType
						{
							Name = "Chick",
							AIAgeGroup = AIAgeGroup.Baby,
							CanReproduce = false,
							Edge = 0.5f,
							HeightTargetModifier = 0.05f,
							WeightTargetModifier = 0.03f,
							ModelScaleFraction = 0.5f,
							NeedTypes = new NeedType[1] { needType9 }
						},
						new AgeGroupType
						{
							Name = "Chick",
							AIAgeGroup = AIAgeGroup.Child,
							CanReproduce = false,
							Edge = 1f,
							HeightTargetModifier = 0.6f,
							WeightTargetModifier = 0.5f,
							ModelScaleFraction = 0.6f,
							NeedTypes = new NeedType[1] { needType9 }
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.YoungAdult,
							CanReproduce = false,
							Edge = 4f,
							HeightTargetModifier = 0.97f,
							WeightTargetModifier = 0.92f,
							ModelScaleFraction = 0.85f,
							NeedTypes = new NeedType[1] { needType9 }
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.Adult,
							CanReproduce = true,
							Edge = 12f,
							HeightTargetModifier = 1f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f,
							NeedTypes = new NeedType[1] { needType9 }
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.Old,
							CanReproduce = true,
							Edge = 14f,
							HeightTargetModifier = 0.9f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f,
							NeedTypes = new NeedType[1] { needType9 }
						}
					}
				},
				new CasteType
				{
					KeyName = "female",
					Reproduction = Reproduction.Female,
					Edge = 1f,
					HeightMean = 0.45f,
					HeightStandardDeviation = 0.02f,
					WeightMean = 28f,
					WeightStandardDeviation = 2f,
					AgeGroupTypes = new List<AgeGroupType>
					{
						new AgeGroupType
						{
							Name = "Chick",
							AIAgeGroup = AIAgeGroup.Baby,
							CanReproduce = false,
							Edge = 0.5f,
							HeightTargetModifier = 0.05f,
							WeightTargetModifier = 0.03f,
							ModelScaleFraction = 0.3f,
							NeedTypes = new NeedType[1] { needType9 }
						},
						new AgeGroupType
						{
							Name = "Chick",
							AIAgeGroup = AIAgeGroup.Child,
							CanReproduce = false,
							Edge = 1f,
							HeightTargetModifier = 0.6f,
							WeightTargetModifier = 0.5f,
							ModelScaleFraction = 0.6f,
							NeedTypes = new NeedType[1] { needType9 }
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.YoungAdult,
							CanReproduce = false,
							Edge = 4f,
							HeightTargetModifier = 0.97f,
							WeightTargetModifier = 0.92f,
							ModelScaleFraction = 0.85f,
							NeedTypes = new NeedType[1] { needType9 }
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.Adult,
							CanReproduce = true,
							Edge = 12f,
							HeightTargetModifier = 1f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f,
							NeedTypes = new NeedType[1] { needType9 }
						},
						new AgeGroupType
						{
							Name = "Grown",
							AIAgeGroup = AIAgeGroup.Old,
							CanReproduce = false,
							Edge = 14f,
							HeightTargetModifier = 0.9f,
							WeightTargetModifier = 1f,
							ModelScaleFraction = 1f,
							NeedTypes = new NeedType[1] { needType9 }
						}
					}
				}
			}
		};
		listOfEntityTypes.Add(entityType17);
		bodyType = GameData.Instance.AllBodyTypes["twinkler"];
		listOfEntityTypes.Add(new EntityType("entity:domesticatedTwinkler")
		{
			Name = "Domesticated Twinkler",
			ThumbnailSmall = "HUD_thumbnail_twinkler",
			SummaryDescription = "Domesticated animal",
			Description = "",
			RenderableType = new RenderableType
			{
				RenderAsModelType = new RenderAsModelType
				{
					ModelScale = 2.5f,
					AssetName = "twinkler",
					GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]> { 
					{
						"normal",
						new GaitAnimationBracket[1]
						{
							new GaitAnimationBracket
							{
								MinimumSpeed = 35f,
								MaximumSpeed = 77f,
								StrideLength = 27f,
								StrideDuration = 0.9f,
								AnimationKey = "gaitWalk"
							}
						}
					} },
					AnimConditions = new AnimConditionInfo[7]
					{
						new AnimConditionInfo
						{
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.Moving
							},
							GaitSetKey = "normal"
						},
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "idle" }
							},
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.Idle
							}
						},
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "attackLowRight" }
							},
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.Attacking,
								Modifiers = new BitMask64(typeof(AnimModifier), 21, 1)
							}
						},
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "attackHighRight" }
							},
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.Attacking,
								Modifiers = new BitMask64(typeof(AnimModifier), 20, 1)
							}
						},
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "hit" },
								Sounds = new string[1] { "aliens/hummingClickClacking" }
							},
							Playback = Playback.Manual,
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.Recoiling
							}
						},
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "dying" }
							},
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.Dying
							}
						},
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "dead" }
							},
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.Dying,
								Modifiers = new BitMask64(typeof(AnimModifier), 13)
							}
						}
					}
				}
			},
			LocomotorType = new LocomotorType
			{
				MaxAngularSpeed = (float)Math.PI / 4f,
				FourSidedSymmetry = true,
				MeleeRadius = 12f,
				LeggedLocomotorType = new LeggedLocomotorType
				{
					TerrainNegateFactor = 0.3f,
					WalkSlowSpeed = 20f,
					WalkNormalSpeed = 22f,
					WalkFastSpeed = 24f
				},
				CollisionResponderType = new CollisionResponderType
				{
					AgentCollisionResponderType = new AgentCollisionResponderType()
				}
			},
			SensorType = new SensorType
			{
				Range = 350f,
				RangeAtNight = 350f,
				DetectionTypeKey = "defaultDetection"
			},
			BodyType = bodyType,
			ContainerType = new AgentStorageType
			{
				ItemStorageType = new ItemStorageType(0.5f),
				StomachStorageType = new ItemStorageType(0.07f)
			},
			IntelligenceType = new IntelligenceType
			{
				MembersScoutingFraction = 1f,
				IsMobile = true,
				CanAttack = true,
				CanUseWeapons = false,
				CanHunt = false,
				CanScout = true,
				CanExamine = true,
				CanPatrol = true,
				CanHaul = false,
				IsPredator = true,
				ContainerTransactTag = "twinklerTransact",
				StrengthRating = StrengthRating.WeakerThanHumans,
				Boldness = 0.25f,
				Courage = 0.05f,
				MemoryInDays = 2f,
				Attacks = new string[2] { "twinklerLowRight", "twinklerHighRight" },
				Skills = new SerializableDictionary<string, float> { { "unarmedFighting", 0.2f } },
				AggroRange = 450f,
				AssistanceRange = 600f
			},
			BiologicalType = new BiologicalType
			{
				OrderKey = "quaditeOrder",
				OxygenAndMuscleEnergyIncreaseRatePerDay = 12f,
				FoodItemTagsThatCanBeConsumed = new string[6] { "cookedMeat", "rawMeat", "smallRawMeat", "spoiledMeal", "inedibleMeat", "rottenMeat" },
				ExtractionProcessTypes = new string[12]
				{
					"extractMudWormMeat", "extractPatricianMeat", "extractLeafCutterMeat", "extractThunderChickenMeat", "extractBinalRatMeat", "extractBushDragonMeat", "extractSwampDemonTreeMeat", "extractDemonTreeMeat", "extractMegapodMeat", "extractWhipjawMeat",
					"extractSpikePlantMeat", "extractForestGuardianMeat"
				},
				TimeToConsumeFullMealInDays = 0.005f,
				StomachSizeFractionOfEntityBulk = 0.3f,
				StomachContentsDecreaseRatePerDay = 3f,
				ActiveStealthRating = 0.2f,
				MaxRegainLimit = 0.5f,
				FractionOfMaxHitpointsGainedPerDay = 0.3f,
				Carcass = "item:quaditeCarcass",
				ResilienceMean = 7f,
				ResilienceStandardDeviation = 0.08f,
				BioPropertyTypes = new BioPropertyType[4]
				{
					new BioPropertyType
					{
						KeyName = "SensorRange",
						InterpolateSetting = BioPropertyType.Interpolate.DefaultPriority
					},
					new BioPropertyType
					{
						KeyName = "SensorRangeAtNight",
						InterpolateSetting = BioPropertyType.Interpolate.DefaultPriority
					},
					new BioPropertyType
					{
						KeyName = "AggroRange",
						InterpolateSetting = BioPropertyType.Interpolate.DefaultPriority
					},
					new BioPropertyType
					{
						KeyName = "AssistanceRange",
						InterpolateSetting = BioPropertyType.Interpolate.DefaultPriority
					}
				},
				Castes = new List<CasteType>
				{
					new CasteType
					{
						KeyName = "male",
						Reproduction = Reproduction.Male,
						HeightMean = 0.7f,
						HeightStandardDeviation = 0.02f,
						WeightMean = 25f,
						WeightStandardDeviation = 2f,
						PrimaryColor = "564920".ToColorVector3(),
						ModelBasicTextureName = "QuaditeRedTexture",
						Edge = 0.6f,
						ModelScale = 2.1f,
						AgeGroupTypes = new List<AgeGroupType>
						{
							new AgeGroupType
							{
								Name = "Puppy",
								AIAgeGroup = AIAgeGroup.Baby,
								CanReproduce = false,
								Edge = 0.5f,
								HeightTargetModifier = 0.05f,
								WeightTargetModifier = 0.03f
							},
							new AgeGroupType
							{
								Name = "Puppy",
								AIAgeGroup = AIAgeGroup.Child,
								CanReproduce = false,
								Edge = 1f,
								HeightTargetModifier = 0.1f,
								WeightTargetModifier = 0.1f
							},
							new AgeGroupType
							{
								Name = "Young",
								AIAgeGroup = AIAgeGroup.YoungAdult,
								CanReproduce = true,
								Edge = 4f,
								HeightTargetModifier = 0.8f,
								WeightTargetModifier = 0.8f
							},
							new AgeGroupType
							{
								Name = "Grown",
								AIAgeGroup = AIAgeGroup.Adult,
								CanReproduce = true,
								Edge = 14f,
								HeightTargetModifier = 1f,
								WeightTargetModifier = 1f,
								NeedTypes = new NeedType[1] { needType5 }
							},
							new AgeGroupType
							{
								Name = "Old",
								AIAgeGroup = AIAgeGroup.Old,
								CanReproduce = false,
								Edge = 20f,
								HeightTargetModifier = 0.9f,
								WeightTargetModifier = 1f,
								NeedTypes = new NeedType[1] { needType5 }
							}
						}
					},
					new CasteType
					{
						KeyName = "female",
						Reproduction = Reproduction.Female,
						HeightMean = 0.7f,
						HeightStandardDeviation = 0.02f,
						WeightMean = 25f,
						WeightStandardDeviation = 2f,
						PrimaryColor = "564920".ToColorVector3(),
						ModelBasicTextureName = "QuaditeRedTexture",
						Edge = 0.6f,
						ModelScale = 2.1f,
						AgeGroupTypes = new List<AgeGroupType>
						{
							new AgeGroupType
							{
								Name = "Puppy",
								AIAgeGroup = AIAgeGroup.Baby,
								CanReproduce = false,
								Edge = 0.5f,
								HeightTargetModifier = 0.05f,
								WeightTargetModifier = 0.03f
							},
							new AgeGroupType
							{
								Name = "Puppy",
								AIAgeGroup = AIAgeGroup.Child,
								CanReproduce = false,
								Edge = 1f,
								HeightTargetModifier = 0.1f,
								WeightTargetModifier = 0.1f
							},
							new AgeGroupType
							{
								Name = "Young",
								AIAgeGroup = AIAgeGroup.YoungAdult,
								CanReproduce = true,
								Edge = 4f,
								HeightTargetModifier = 0.8f,
								WeightTargetModifier = 0.8f
							},
							new AgeGroupType
							{
								Name = "Grown",
								AIAgeGroup = AIAgeGroup.Adult,
								CanReproduce = true,
								Edge = 14f,
								HeightTargetModifier = 1f,
								WeightTargetModifier = 1f,
								NeedTypes = new NeedType[1] { needType5 }
							},
							new AgeGroupType
							{
								Name = "Old",
								AIAgeGroup = AIAgeGroup.Old,
								CanReproduce = false,
								Edge = 20f,
								HeightTargetModifier = 0.9f,
								WeightTargetModifier = 1f,
								NeedTypes = new NeedType[1] { needType5 }
							}
						}
					}
				}
			}
		});
		float num = 0.1f;
		needType = new NeedType();
		needType.KeyName = "foodEnergy";
		needType.FoodNeedType = new FoodNeedType
		{
			FoodNutrient = "foodEnergy",
			RequiredNutrientsAsFractionOfEntityBulk = 0.6f * num / 1f
		};
		needType.DecreasePerDay = new NormalDistribution
		{
			Mean = 1.0,
			StandardDeviation = 0.019999999552965164
		};
		needType.LimitForDecreasedEnergy = 0.15f;
		needType.DecreasedEnergyWeight = 0.6f;
		needType.PhysicalEffects = new PhysicalEffects
		{
			DaysAtZeroCausingCollapse = 2f,
			DaysAtZeroCausingDeath = 2.05f,
			DaysAtZeroDecreaseFactor = 1f,
			UseExertionFactorToDecrease = true
		};
		NeedType needType10 = needType;
		needType = new NeedType();
		needType.KeyName = "sleep";
		needType.SleepNeedType = new SleepNeedType();
		needType.DecreasePerDay = new NormalDistribution
		{
			Mean = 1.0,
			StandardDeviation = 0.014999999664723873
		};
		needType.LimitForDecreasedEnergy = 0.4f;
		needType.DecreasedEnergyWeight = 0.4f;
		needType.PhysicalEffects = new PhysicalEffects
		{
			DaysAtZeroCausingDeath = 2f,
			DaysAtZeroDecreaseFactor = 2f
		};
		NeedType needType11 = needType;
		bodyType = GameData.Instance.AllBodyTypes["dog"];
		listOfEntityTypes.Add(new EntityType("entity:dog")
		{
			Name = "Dog",
			ThumbnailSmall = "HUD_thumbnail_dog",
			SummaryDescription = "Tau Shepherd dog breed.",
			Description = "\n The German Shepherd was a popular choice as the basis for a dog breed adapted to the conditions on planet Antheia. Created as a result of some genetic engineering and some breeding. The Tau shepherd is trained to keep pest animals away and will also attack animals that are a threat to its owners.",
			UseTypeNameForDisplay = false,
			CategoryKey = "animals",
			RenderableType = new RenderableType
			{
				RenderAsModelType = new RenderAsModelType
				{
					ModelScale = 1.78f,
					AssetName = "dog",
					GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]> { 
					{
						"normal",
						new GaitAnimationBracket[3]
						{
							new GaitAnimationBracket
							{
								MinimumSpeed = 1f,
								MaximumSpeed = 57f,
								StrideLength = 18f,
								StrideDuration = 0.96f,
								AnimationKey = "gaitWalk"
							},
							new GaitAnimationBracket
							{
								MinimumSpeed = 54f,
								MaximumSpeed = 96f,
								StrideLength = 32f,
								StrideDuration = 0.96f,
								AnimationKey = "gaitTrot"
							},
							new GaitAnimationBracket
							{
								MinimumSpeed = 92f,
								MaximumSpeed = 180f,
								StrideLength = 46f,
								StrideDuration = 0.96f,
								AnimationKey = "gaitGallop"
							}
						}
					} },
					DefaultInfo = new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "idle" }
						}
					},
					DefaultStances = new AnimConditionInfo[2]
					{
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "idle" }
							}
						},
						new AnimConditionInfo
						{
							ConditionSet = new AnimConditions
							{
								Modifiers = new BitMask64(typeof(AnimModifier), 17)
							},
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "lying" }
							}
						}
					},
					AnimConditions = new AnimConditionInfo[12]
					{
						new AnimConditionInfo
						{
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.Moving
							},
							GaitSetKey = "normal"
						},
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "lyingToStand" }
							},
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.ChangingStance,
								Modifiers = new BitMask64(typeof(AnimModifier), 17)
							},
							Looping = Looping.No
						},
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "standToLying" }
							},
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.ChangingStance,
								Modifiers = new BitMask64(typeof(AnimModifier), 17, 9)
							},
							Looping = Looping.No
						},
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[2] { "idle", "idleHowl" }
							},
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.Idle
							}
						},
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[2] { "lying", "sleep" }
							},
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.Idle,
								Modifiers = new BitMask64(typeof(AnimModifier), 17)
							},
							Looping = Looping.Yes
						},
						new AnimConditionInfo
						{
							ConditionSet = new AnimConditions
							{
								Modifiers = new BitMask64(typeof(AnimModifier), 6)
							},
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "combatIdle" }
							}
						},
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[6] { "attack", "attack", "attack", "attack", "attack", "attack" },
								Sounds = new string[6] { "domesticated/dog/dogAttackSnarl1", "domesticated/dog/dogAttackSnarl2", "", "", "", "" }
							},
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.Attacking,
								Modifiers = new BitMask64(typeof(AnimModifier))
							}
						},
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "hit" },
								Sounds = new string[1] { "domesticated/dog/dogHitBark1" }
							},
							Playback = Playback.Manual,
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.Recoiling
							}
						},
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "collapse" },
								Sounds = new string[1] { "domesticated/dog/dogHitWhimper2" }
							},
							Looping = Looping.No,
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.Dying,
								Modifiers = new BitMask64(typeof(AnimModifier), 12)
							}
						},
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "dead" }
							},
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.Dying,
								Modifiers = new BitMask64(typeof(AnimModifier), 13)
							}
						},
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "eat" }
							},
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.Eating
							},
							Looping = Looping.Yes
						},
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "sleep" }
							},
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.Sleeping,
								Modifiers = new BitMask64(typeof(AnimModifier), 17)
							},
							Looping = Looping.Yes
						}
					}
				}
			},
			LocomotorType = new LocomotorType
			{
				MaxAngularSpeed = (float)Math.PI * 3f,
				FourSidedSymmetry = false,
				MeleeRadius = 12f,
				CanRun = true,
				Stances = "dog",
				LeggedLocomotorType = new LeggedLocomotorType
				{
					TerrainNegateFactor = 0.3f,
					WalkSlowSpeed = 35f,
					WalkNormalSpeed = 76f,
					WalkFastSpeed = 80f,
					RunSpeed = 95f
				},
				CollisionResponderType = new CollisionResponderType
				{
					AgentCollisionResponderType = new AgentCollisionResponderType()
				}
			},
			SensorType = new SensorType
			{
				Range = 350f,
				RangeAtNight = 350f,
				DetectionTypeKey = "human"
			},
			BodyType = bodyType,
			ContainerType = new AgentStorageType
			{
				ItemStorageType = new ItemStorageType(0.5f),
				StomachStorageType = new ItemStorageType(0.07f)
			},
			IntelligenceType = new IntelligenceType
			{
				MembersScoutingFraction = 1f,
				IsMobile = true,
				AllowEscapeFromTinyAreas = true,
				CanAttack = true,
				CanUseWeapons = false,
				CanHunt = false,
				CanScout = false,
				CanExamine = false,
				CanPatrol = false,
				CanHaul = false,
				HuntsVermin = true,
				IsPredator = true,
				ContainerTransactTag = "dogTransact",
				ServantForEntityTypeTag = "servesHumans",
				StrengthRating = StrengthRating.WeakerThanHumans,
				Boldness = 0.4f,
				Courage = 0.05f,
				MemoryInDays = 2f,
				ChanceToIdleWalkShortDistanceAway = 0.3f,
				ShortIdleWalkMinDistance = 40f,
				ShortIdleWalkMaxDistance = 160f,
				Attacks = new string[1] { "dogBiting" },
				Skills = new SerializableDictionary<string, float> { { "unarmedFighting", 0.3f } },
				AggroRange = 450f,
				AssistanceRange = 600f
			},
			BiologicalType = new BiologicalType
			{
				OrderKey = "carnivoraOrder",
				OxygenAndMuscleEnergyIncreaseRatePerDay = 12f,
				FoodItemTagsThatCanBeConsumed = new string[6] { "cookedMeat", "rawMeat", "smallRawMeat", "spoiledMeal", "inedibleMeat", "rottenMeat" },
				ExtractionProcessTypes = new string[13]
				{
					"extractMudWormMeat", "extractPatricianMeat", "extractLeafCutterMeat", "extractThunderChickenMeat", "extractBinalRatMeat", "extractTwinklerMeat", "extractBushDragonMeat", "extractSwampDemonTreeMeat", "extractDemonTreeMeat", "extractMegapodMeat",
					"extractWhipjawMeat", "extractSpikePlantMeat", "extractForestGuardianMeat"
				},
				TimeToConsumeFullMealInDays = 0.005f,
				StomachSizeFractionOfEntityBulk = 0.1f,
				StomachContentsDecreaseRatePerDay = 3f,
				ActiveStealthRating = 0.2f,
				EatingStances = new ChanceToTakeStance[1]
				{
					new ChanceToTakeStance
					{
						Stance = "standing"
					}
				},
				MaxRegainLimit = 0.5f,
				FractionOfMaxHitpointsGainedPerDay = 0.3f,
				Carcass = "item:dogCarcass",
				ResilienceMean = 5.5f,
				ResilienceStandardDeviation = 0.08f,
				BioPropertyTypes = new BioPropertyType[4]
				{
					new BioPropertyType
					{
						KeyName = "SensorRange",
						InterpolateSetting = BioPropertyType.Interpolate.DefaultPriority
					},
					new BioPropertyType
					{
						KeyName = "SensorRangeAtNight",
						InterpolateSetting = BioPropertyType.Interpolate.DefaultPriority
					},
					new BioPropertyType
					{
						KeyName = "AggroRange",
						InterpolateSetting = BioPropertyType.Interpolate.DefaultPriority
					},
					new BioPropertyType
					{
						KeyName = "AssistanceRange",
						InterpolateSetting = BioPropertyType.Interpolate.DefaultPriority
					}
				},
				Castes = new List<CasteType>
				{
					new CasteType
					{
						KeyName = "male",
						Reproduction = Reproduction.Male,
						HeightMean = 0.7f,
						HeightStandardDeviation = 0.02f,
						WeightMean = 40f,
						WeightStandardDeviation = 2f,
						PrimaryColor = "564920".ToColorVector3(),
						ModelBasicTextureName = "DogGermanShepherdTexture",
						Edge = 0.6f,
						AgeGroupTypes = new List<AgeGroupType>
						{
							new AgeGroupType
							{
								Name = "Puppy",
								AIAgeGroup = AIAgeGroup.Baby,
								CanReproduce = false,
								Edge = 0.5f,
								HeightTargetModifier = 0.05f,
								WeightTargetModifier = 0.03f,
								ModelScaleFraction = 0.7f
							},
							new AgeGroupType
							{
								Name = "Puppy",
								AIAgeGroup = AIAgeGroup.Child,
								CanReproduce = false,
								Edge = 1f,
								HeightTargetModifier = 0.1f,
								WeightTargetModifier = 0.1f,
								ModelScaleFraction = 0.9f
							},
							new AgeGroupType
							{
								Name = "Young",
								AIAgeGroup = AIAgeGroup.YoungAdult,
								CanReproduce = true,
								Edge = 4f,
								HeightTargetModifier = 0.8f,
								WeightTargetModifier = 0.8f,
								NeedTypes = new NeedType[2] { needType10, needType11 }
							},
							new AgeGroupType
							{
								Name = "Grown",
								AIAgeGroup = AIAgeGroup.Adult,
								CanReproduce = true,
								Edge = 14f,
								HeightTargetModifier = 1f,
								WeightTargetModifier = 1f,
								NeedTypes = new NeedType[2] { needType10, needType11 }
							},
							new AgeGroupType
							{
								Name = "Old",
								AIAgeGroup = AIAgeGroup.Old,
								CanReproduce = false,
								Edge = 20f,
								HeightTargetModifier = 0.9f,
								WeightTargetModifier = 1f,
								NeedTypes = new NeedType[2] { needType10, needType11 }
							}
						}
					},
					new CasteType
					{
						KeyName = "female",
						Reproduction = Reproduction.Female,
						HeightMean = 0.7f,
						HeightStandardDeviation = 0.02f,
						WeightMean = 38f,
						WeightStandardDeviation = 2f,
						PrimaryColor = "564920".ToColorVector3(),
						ModelBasicTextureName = "DogGermanShepherdTexture",
						Edge = 0.6f,
						AgeGroupTypes = new List<AgeGroupType>
						{
							new AgeGroupType
							{
								Name = "Puppy",
								AIAgeGroup = AIAgeGroup.Baby,
								CanReproduce = false,
								Edge = 0.5f,
								HeightTargetModifier = 0.05f,
								WeightTargetModifier = 0.03f,
								ModelScaleFraction = 0.7f
							},
							new AgeGroupType
							{
								Name = "Puppy",
								AIAgeGroup = AIAgeGroup.Child,
								CanReproduce = false,
								Edge = 1f,
								HeightTargetModifier = 0.1f,
								WeightTargetModifier = 0.1f,
								ModelScaleFraction = 0.9f
							},
							new AgeGroupType
							{
								Name = "Young",
								AIAgeGroup = AIAgeGroup.YoungAdult,
								CanReproduce = true,
								Edge = 4f,
								HeightTargetModifier = 0.8f,
								WeightTargetModifier = 0.8f,
								NeedTypes = new NeedType[2] { needType10, needType11 }
							},
							new AgeGroupType
							{
								Name = "Grown",
								AIAgeGroup = AIAgeGroup.Adult,
								CanReproduce = true,
								Edge = 14f,
								HeightTargetModifier = 1f,
								WeightTargetModifier = 1f,
								NeedTypes = new NeedType[2] { needType10, needType11 }
							},
							new AgeGroupType
							{
								Name = "Old",
								AIAgeGroup = AIAgeGroup.Old,
								CanReproduce = false,
								Edge = 20f,
								HeightTargetModifier = 0.9f,
								WeightTargetModifier = 1f,
								NeedTypes = new NeedType[2] { needType10, needType11 }
							}
						}
					}
				}
			}
		});
		AnimConditionInfo animConditionInfo = new AnimConditionInfo();
		animConditionInfo.SoundAndAnimationSet = new RandomSoundAndAnimationSet
		{
			BaseAnimations = new string[1] { "fold" },
			Sounds = new string[1] { "robotServoArms2" }
		};
		animConditionInfo.ConditionSet = new AnimConditions
		{
			Action = AnimAction.ChangingStance,
			Modifiers = new BitMask64(typeof(AnimModifier), 47)
		};
		animConditionInfo.Looping = Looping.No;
		AnimConditionInfo animConditionInfo2 = animConditionInfo;
		animConditionInfo = new AnimConditionInfo();
		animConditionInfo.SoundAndAnimationSet = new RandomSoundAndAnimationSet
		{
			BaseAnimations = new string[1] { "unfold" },
			Sounds = new string[1] { "robotServoArms2" }
		};
		animConditionInfo.ConditionSet = new AnimConditions
		{
			Action = AnimAction.ChangingStance,
			Modifiers = new BitMask64(typeof(AnimModifier), 47, 9)
		};
		animConditionInfo.Looping = Looping.No;
		AnimConditionInfo animConditionInfo3 = animConditionInfo;
		animConditionInfo = new AnimConditionInfo();
		animConditionInfo.SoundAndAnimationSet = new RandomSoundAndAnimationSet
		{
			BaseAnimations = new string[1] { "farming" }
		};
		animConditionInfo.ConditionSet = new AnimConditions
		{
			Action = AnimAction.Tilling
		};
		animConditionInfo.Looping = Looping.Yes;
		AnimConditionInfo animConditionInfo4 = animConditionInfo;
		listOfEntityTypes.Add(new EntityType("entity:robotSmall")
		{
			Name = "FLEA",
			ThumbnailSmall = "HUD_thumbnail_smallRobot",
			SummaryDescription = "Small robot equipped for weeding",
			Description = "In this configuration, the FLEA is able to neutralize most forms of weeds. The arm mounted tool contains a cutting device, a pesticide dispenser and a laser. It is able to power itself by using vegetation as a fuel source for its engine and for recharging its batteries. Its parts are simple and easily replaceable which ensures a very long service life.",
			CategoryKey = "robots",
			RenderableType = new RenderableType
			{
				RenderAsModelType = new RenderAsModelType
				{
					ModelScale = 1.5f,
					AssetName = "robotLight",
					ModelBasicTextureName = "RobotTexture1",
					GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]> { 
					{
						"normal",
						new GaitAnimationBracket[1]
						{
							new GaitAnimationBracket
							{
								MinimumSpeed = 0f,
								MaximumSpeed = 77f,
								StrideLength = 11f,
								StrideDuration = 0.4f,
								AnimationKey = "idle"
							}
						}
					} },
					AnimConditions = new AnimConditionInfo[6]
					{
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet(),
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.Moving
							},
							GaitSetKey = "normal"
						},
						animConditionInfo2,
						animConditionInfo3,
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "idle" }
							},
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.Idle
							}
						},
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "scan" }
							},
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.Scouting
							}
						},
						animConditionInfo4
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartKeys = new SerializableDictionary<string, int> { { "item:weedingRobotTool", 1 } }
			},
			LocomotorType = new LocomotorType
			{
				MaxAngularSpeed = 4.712389f,
				MeleeRadius = 10f,
				Stances = "robot",
				LeggedLocomotorType = new LeggedLocomotorType
				{
					TerrainNegateFactor = 0.5f,
					WalkSlowSpeed = 20f,
					WalkNormalSpeed = 33f,
					WalkFastSpeed = 64f
				},
				CollisionResponderType = new CollisionResponderType
				{
					AgentCollisionResponderType = new AgentCollisionResponderType()
				}
			},
			SensorType = new SensorType
			{
				Range = 350f,
				RangeAtNight = 350f,
				DetectionTypeKey = "human"
			},
			BodyType = GameData.Instance.AllBodyTypes["robotBody"],
			IntelligenceType = new IntelligenceType
			{
				RespectsOwnership = true,
				CanAttack = false,
				CanUseWeapons = false,
				CanProduce = true,
				CanHaul = false,
				CanHunt = false,
				CanPatrol = false,
				CanScout = false,
				CanExamine = false,
				CanPanic = false,
				ServantForEntityTypeTag = "servesHumans",
				IsMobile = true,
				StrengthRating = StrengthRating.WeakerThanHumans,
				Courage = 1f,
				MemoryInDays = 0.1f,
				Boldness = 1f,
				ChanceToIdleWalkShortDistanceAway = 0.1f,
				ShortIdleWalkMaxDistance = 120f,
				ShortIdleWalkMinDistance = 48f,
				IntrinsicTools = new string[1] { "item:weedingRobotTool" },
				Skills = new SerializableDictionary<string, float> { { "weeding", 0.8f } }
			}
		});
		listOfEntityTypes.Add(new EntityType("entity:guardRobot")
		{
			Name = "HOUND",
			ThumbnailSmall = "HUD_thumbnail_smallRobot",
			SummaryDescription = "Small robot equipped for guarding",
			Description = "In this configuration, the HOUND is able to neutralize most forms of animals. The arm mounted tool contains a laser.",
			CategoryKey = "robots",
			RenderableType = new RenderableType
			{
				RenderAsModelType = new RenderAsModelType
				{
					ModelScale = 1.2f,
					AssetName = "robotLight",
					ModelBasicTextureName = "RobotTexture1",
					GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]> { 
					{
						"normal",
						new GaitAnimationBracket[1]
						{
							new GaitAnimationBracket
							{
								MinimumSpeed = 0f,
								MaximumSpeed = 77f,
								StrideLength = 11f,
								StrideDuration = 0.4f,
								AnimationKey = "idle"
							}
						}
					} },
					AnimConditions = new AnimConditionInfo[6]
					{
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet(),
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.Moving
							},
							GaitSetKey = "normal"
						},
						animConditionInfo2,
						animConditionInfo3,
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "idle" }
							},
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.Idle
							}
						},
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "scan" }
							},
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.Scouting
							}
						},
						animConditionInfo4
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartKeys = new SerializableDictionary<string, int> { { "item:sentryLaserGun", 1 } }
			},
			LocomotorType = new LocomotorType
			{
				MaxAngularSpeed = 4.712389f,
				MeleeRadius = 10f,
				Stances = "robot",
				LeggedLocomotorType = new LeggedLocomotorType
				{
					TerrainNegateFactor = 0.5f,
					WalkSlowSpeed = 20f,
					WalkNormalSpeed = 33f,
					WalkFastSpeed = 64f
				},
				CollisionResponderType = new CollisionResponderType
				{
					AgentCollisionResponderType = new AgentCollisionResponderType()
				}
			},
			SensorType = new SensorType
			{
				Range = 350f,
				RangeAtNight = 350f,
				DetectionTypeKey = "human"
			},
			BodyType = GameData.Instance.AllBodyTypes["robotBody"],
			IntelligenceType = new IntelligenceType
			{
				RespectsOwnership = true,
				CanAttack = true,
				CanUseWeapons = false,
				CanProduce = true,
				CanHaul = false,
				CanHunt = false,
				CanPatrol = true,
				CanScout = true,
				CanExamine = false,
				CanPanic = false,
				ServantForEntityTypeTag = "servesHumans",
				IsMobile = true,
				StrengthRating = StrengthRating.WeakerThanHumans,
				Courage = 1f,
				MemoryInDays = 0.1f,
				Boldness = 1f,
				ChanceToIdleWalkShortDistanceAway = 0.1f,
				ShortIdleWalkMaxDistance = 120f,
				ShortIdleWalkMinDistance = 48f,
				IntrinsicWeapons = new string[1] { "item:sentryLaserGun" },
				Skills = new SerializableDictionary<string, float> { { "shooting", 0.8f } },
				Attacks = new string[1] { "sentryLaserGunShot" },
				AggroRange = 250f,
				AssistanceRange = 400f
			}
		});
		listOfEntityTypes.Add(new EntityType("entity:haulingRobot")
		{
			Name = "GOPHER",
			SummaryDescription = "Sturdy farm robot able to harvest crops and transport items",
			Description = "This robot vehicle was designed by the Tau Ceti planners with survivability in mind. It is able to power itself by using vegetation as a fuel source for its engine and for recharging its batteries. Its parts are simple and easily replaceable which ensures a very long service life.",
			ThumbnailSmall = "HUD_thumbnail_haulingRobot",
			CategoryKey = "robots",
			RenderableType = new RenderableType
			{
				BoxHandlingWhenHauling = new BoxHandlingWhenHauling
				{
					BoxHandling = BoxHandlingWhenHauling.BoxHandlingType.AlwaysOnBack,
					UseHeavyBackpack = false,
					ShowBoxInHand = AttacheePoint.LeftHand,
					AttachorWhenBoxIsInHand = "leftHand"
				},
				RenderAsModelType = new RenderAsModelType
				{
					ModelScale = 1.45f,
					AssetName = "robotHeavy",
					ModelBasicTextureName = "RobotTexture1",
					GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]> { 
					{
						"normal",
						new GaitAnimationBracket[1]
						{
							new GaitAnimationBracket
							{
								MinimumSpeed = 0f,
								MaximumSpeed = 77f,
								StrideLength = 11f,
								StrideDuration = 0.4f,
								AnimationKey = "idle"
							}
						}
					} },
					DefaultInfo = new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "idleUnfolded" }
						}
					},
					DefaultStances = new AnimConditionInfo[2]
					{
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "idle" }
							},
							ConditionSet = new AnimConditions
							{
								Modifiers = new BitMask64(typeof(AnimModifier), 47)
							}
						},
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "idleUnfolded" }
							}
						}
					},
					AnimConditions = new AnimConditionInfo[18]
					{
						new AnimConditionInfo
						{
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.Moving
							},
							GaitSetKey = "normal"
						},
						new AnimConditionInfo
						{
							GaitSetKey = "normal",
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.Hauling
							},
							Forbiddens = new BitMask64(typeof(AnimModifier), 30)
						},
						new AnimConditionInfo
						{
							GaitSetKey = "normal",
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.Hauling,
								Modifiers = new BitMask64(typeof(AnimModifier), 19)
							},
							Forbiddens = new BitMask64(typeof(AnimModifier), 30)
						},
						new AnimConditionInfo
						{
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.Hauling,
								Modifiers = new BitMask64(typeof(AnimModifier), 30)
							},
							GaitSetKey = "normal"
						},
						new AnimConditionInfo
						{
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.Hauling,
								Modifiers = new BitMask64(typeof(AnimModifier), 30, 19)
							},
							GaitSetKey = "normal"
						},
						animConditionInfo2,
						animConditionInfo3,
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "idle" }
							},
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.Idle
							}
						},
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "idleUnfolded" }
							},
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.Idle
							},
							Looping = Looping.Yes
						},
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "pickup" }
							},
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.PickingUp
							},
							Looping = Looping.No,
							StartingPoint = StartingPoint.FromBeginning
						},
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "pickup" }
							},
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.PickingUp,
								Modifiers = new BitMask64(typeof(AnimModifier), 26)
							},
							Looping = Looping.No,
							StartingPoint = StartingPoint.FromBeginning
						},
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "pickup" }
							},
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.PickingUp,
								Modifiers = new BitMask64(typeof(AnimModifier), 26, 13)
							},
							Looping = Looping.No,
							StartingPoint = StartingPoint.Specified,
							StartingPointInSeconds = 0.4f
						},
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "pickup" }
							},
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.PickingUp,
								Modifiers = new BitMask64(typeof(AnimModifier), 13)
							},
							Forbiddens = new BitMask64(typeof(AnimModifier), 26),
							Looping = Looping.No,
							StartingPoint = StartingPoint.Specified,
							StartingPointInSeconds = 0.4f
						},
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "drop" }
							},
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.Dropping
							},
							Looping = Looping.No,
							StartingPoint = StartingPoint.FromBeginning
						},
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "drop" }
							},
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.Dropping,
								Modifiers = new BitMask64(typeof(AnimModifier), 29)
							},
							Looping = Looping.No,
							StartingPoint = StartingPoint.FromBeginning
						},
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "drop" }
							},
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.Dropping,
								Modifiers = new BitMask64(typeof(AnimModifier), 26)
							},
							Looping = Looping.No,
							StartingPoint = StartingPoint.FromBeginning
						},
						animConditionInfo4,
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "farming" }
							},
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.Harvesting
							},
							Looping = Looping.Yes
						}
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartKeys = new SerializableDictionary<string, int> { { "item:weedingRobotTool", 1 } }
			},
			LocomotorType = new LocomotorType
			{
				MaxAngularSpeed = 4.712389f,
				MeleeRadius = 10f,
				Stances = "robot",
				LeggedLocomotorType = new LeggedLocomotorType
				{
					TerrainNegateFactor = 0.5f,
					WalkSlowSpeed = 14f,
					WalkNormalSpeed = 30f,
					WalkFastSpeed = 42f,
					HaulSpeed = 26f
				},
				CollisionResponderType = new CollisionResponderType
				{
					AgentCollisionResponderType = new AgentCollisionResponderType()
				}
			},
			SensorType = new SensorType
			{
				Range = 347f,
				RangeAtNight = 347f,
				DetectionTypeKey = "human"
			},
			BodyType = GameData.Instance.AllBodyTypes["robotBody"],
			ContainerType = new AgentStorageType
			{
				ItemStorageType = new ItemStorageType(1f)
			},
			IntelligenceType = new IntelligenceType
			{
				RespectsOwnership = true,
				CanAttack = false,
				CanUseWeapons = false,
				CanHaul = true,
				CanProduce = true,
				CanHunt = false,
				CanPatrol = false,
				CanScout = false,
				CanExamine = false,
				CanPanic = false,
				ContainerTransactTag = "robotTransact",
				ServantForEntityTypeTag = "servesHumans",
				IsMobile = true,
				StrengthRating = StrengthRating.WeakerThanHumans,
				Courage = 1f,
				MemoryInDays = 0.1f,
				Boldness = 1f,
				PickupLightDuration = 1.4f,
				PickupLightActionPointDuration = 0.4f,
				PickupHeavyDuration = 1.4f,
				PickupHeavyActionPointDuration = 0.4f,
				SwitchLightToLightDuration = 1.4f,
				SwitchLightToLightActionPointDuration = 0.4f,
				DropLightDuration = 1.68f,
				DropLightActionPointDuration = 1.4f,
				DropHeavyDuration = 1.68f,
				DropHeavyActionPointDuration = 1.4f,
				DropLightToLightActionPointDuration = 1.4f,
				DropLightToLightDuration = 1.68f,
				ChanceToIdleWalkShortDistanceAway = 0.1f,
				ShortIdleWalkMaxDistance = 120f,
				ShortIdleWalkMinDistance = 48f,
				IntrinsicTools = new string[1] { "item:weedingRobotTool" },
				Skills = new SerializableDictionary<string, float>
				{
					{ "grasping", 0.6f },
					{ "fruitPicking", 0.6f },
					{ "weeding", 0.8f }
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("entity:diggingRobot")
		{
			Name = "MOLE",
			SummaryDescription = "Sturdy mining robot able to mine ores and transport items",
			Description = "This robot vehicle was designed by the Tau Ceti planners with survivability in mind. It is able to power itself by using vegetation as a fuel source for its engine and for recharging its batteries. Its parts are simple and easily replaceable which ensures a very long service life.",
			ThumbnailSmall = "HUD_thumbnail_haulingRobot",
			CategoryKey = "robots",
			RenderableType = new RenderableType
			{
				BoxHandlingWhenHauling = new BoxHandlingWhenHauling
				{
					BoxHandling = BoxHandlingWhenHauling.BoxHandlingType.AlwaysOnBack,
					UseHeavyBackpack = false,
					ShowBoxInHand = AttacheePoint.LeftHand,
					AttachorWhenBoxIsInHand = "leftHand"
				},
				RenderAsModelType = new RenderAsModelType
				{
					ModelScale = 1.5f,
					AssetName = "robotHeavy",
					ModelBasicTextureName = "RobotTexture2",
					GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]> { 
					{
						"normal",
						new GaitAnimationBracket[1]
						{
							new GaitAnimationBracket
							{
								MinimumSpeed = 0f,
								MaximumSpeed = 77f,
								StrideLength = 11f,
								StrideDuration = 0.4f,
								AnimationKey = "idle"
							}
						}
					} },
					DefaultInfo = new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "idleUnfolded" }
						}
					},
					DefaultStances = new AnimConditionInfo[2]
					{
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "idle" }
							},
							ConditionSet = new AnimConditions
							{
								Modifiers = new BitMask64(typeof(AnimModifier), 47)
							}
						},
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "idleUnfolded" }
							}
						}
					},
					AnimConditions = new AnimConditionInfo[17]
					{
						new AnimConditionInfo
						{
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.Moving
							},
							GaitSetKey = "normal"
						},
						new AnimConditionInfo
						{
							GaitSetKey = "normal",
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.Hauling
							},
							Forbiddens = new BitMask64(typeof(AnimModifier), 30)
						},
						new AnimConditionInfo
						{
							GaitSetKey = "normal",
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.Hauling,
								Modifiers = new BitMask64(typeof(AnimModifier), 19)
							},
							Forbiddens = new BitMask64(typeof(AnimModifier), 30)
						},
						new AnimConditionInfo
						{
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.Hauling,
								Modifiers = new BitMask64(typeof(AnimModifier), 30)
							},
							GaitSetKey = "normal"
						},
						new AnimConditionInfo
						{
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.Hauling,
								Modifiers = new BitMask64(typeof(AnimModifier), 30, 19)
							},
							GaitSetKey = "normal"
						},
						animConditionInfo2,
						animConditionInfo3,
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "idle" }
							},
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.Idle
							}
						},
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "idleUnfolded" }
							},
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.Idle
							},
							Looping = Looping.Yes
						},
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "pickup" }
							},
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.PickingUp
							},
							Looping = Looping.No,
							StartingPoint = StartingPoint.FromBeginning
						},
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "pickup" }
							},
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.PickingUp,
								Modifiers = new BitMask64(typeof(AnimModifier), 26)
							},
							Looping = Looping.No,
							StartingPoint = StartingPoint.FromBeginning
						},
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "pickup" }
							},
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.PickingUp,
								Modifiers = new BitMask64(typeof(AnimModifier), 26, 13)
							},
							Looping = Looping.No,
							StartingPoint = StartingPoint.Specified,
							StartingPointInSeconds = 0.4f
						},
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "pickup" }
							},
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.PickingUp,
								Modifiers = new BitMask64(typeof(AnimModifier), 13)
							},
							Forbiddens = new BitMask64(typeof(AnimModifier), 26),
							Looping = Looping.No,
							StartingPoint = StartingPoint.Specified,
							StartingPointInSeconds = 0.4f
						},
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "drop" }
							},
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.Dropping
							},
							Looping = Looping.No,
							StartingPoint = StartingPoint.FromBeginning
						},
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "drop" }
							},
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.Dropping,
								Modifiers = new BitMask64(typeof(AnimModifier), 29)
							},
							Looping = Looping.No,
							StartingPoint = StartingPoint.FromBeginning
						},
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "drop" }
							},
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.Dropping,
								Modifiers = new BitMask64(typeof(AnimModifier), 26)
							},
							Looping = Looping.No,
							StartingPoint = StartingPoint.FromBeginning
						},
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "farming" }
							},
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.Digging
							},
							Looping = Looping.Yes
						}
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartKeys = new SerializableDictionary<string, int> { { "item:diggingRobotTool", 1 } }
			},
			LocomotorType = new LocomotorType
			{
				MaxAngularSpeed = 4.712389f,
				MeleeRadius = 10f,
				Stances = "robot",
				LeggedLocomotorType = new LeggedLocomotorType
				{
					TerrainNegateFactor = 0.5f,
					WalkSlowSpeed = 14f,
					WalkNormalSpeed = 30f,
					WalkFastSpeed = 42f,
					HaulSpeed = 26f
				},
				CollisionResponderType = new CollisionResponderType
				{
					AgentCollisionResponderType = new AgentCollisionResponderType()
				}
			},
			SensorType = new SensorType
			{
				Range = 347f,
				RangeAtNight = 347f,
				DetectionTypeKey = "human"
			},
			BodyType = GameData.Instance.AllBodyTypes["robotBody"],
			ContainerType = new AgentStorageType
			{
				ItemStorageType = new ItemStorageType(1.3f)
			},
			IntelligenceType = new IntelligenceType
			{
				RespectsOwnership = true,
				CanAttack = false,
				CanUseWeapons = false,
				CanHaul = true,
				CanProduce = true,
				CanHunt = false,
				CanPatrol = false,
				CanScout = false,
				CanExamine = false,
				CanPanic = false,
				ContainerTransactTag = "robotTransact",
				ServantForEntityTypeTag = "servesHumans",
				IsMobile = true,
				StrengthRating = StrengthRating.WeakerThanHumans,
				Courage = 1f,
				MemoryInDays = 0.1f,
				Boldness = 1f,
				PickupLightDuration = 1.4f,
				PickupLightActionPointDuration = 0.4f,
				PickupHeavyDuration = 1.4f,
				PickupHeavyActionPointDuration = 0.4f,
				SwitchLightToLightDuration = 1.4f,
				SwitchLightToLightActionPointDuration = 0.4f,
				DropLightDuration = 1.68f,
				DropLightActionPointDuration = 1.4f,
				DropHeavyDuration = 1.68f,
				DropHeavyActionPointDuration = 1.4f,
				DropLightToLightActionPointDuration = 1.4f,
				DropLightToLightDuration = 1.68f,
				ChanceToIdleWalkShortDistanceAway = 0.1f,
				ShortIdleWalkMaxDistance = 120f,
				ShortIdleWalkMinDistance = 48f,
				IntrinsicTools = new string[1] { "item:diggingRobotTool" },
				Skills = new SerializableDictionary<string, float>
				{
					{ "grasping", 0.6f },
					{ "fruitPicking", 0.6f },
					{ "weeding", 0.8f }
				}
			}
		});
		bodyType = GameData.Instance.AllBodyTypes["humanoid"];
		entityType = new EntityType("entity:human");
		entityType.ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always;
		entityType.Name = "Human";
		entityType.SummaryDescription = "";
		entityType.Description = "";
		entityType.UseTypeNameForDisplay = false;
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "man",
				GaitAnimations = new XmlDictionary<string, GaitAnimationBracket[]>
				{
					{
						"normal",
						new GaitAnimationBracket[5]
						{
							new GaitAnimationBracket
							{
								MinimumSpeed = 0f,
								MaximumSpeed = 16f,
								StrideLength = 9f,
								StrideDuration = 0.96f,
								AnimationKey = "gaitSlowWalk"
							},
							new GaitAnimationBracket
							{
								MinimumSpeed = 14f,
								MaximumSpeed = 37f,
								StrideLength = 18f,
								StrideDuration = 0.96f,
								AnimationKey = "gaitStroll"
							},
							new GaitAnimationBracket
							{
								MinimumSpeed = 35f,
								MaximumSpeed = 67f,
								StrideLength = 26f,
								StrideDuration = 0.96f,
								AnimationKey = "gaitWalk"
							},
							new GaitAnimationBracket
							{
								MinimumSpeed = 64f,
								MaximumSpeed = 106f,
								StrideLength = 32f,
								StrideDuration = 0.96f,
								AnimationKey = "gaitJog"
							},
							new GaitAnimationBracket
							{
								MinimumSpeed = 102f,
								MaximumSpeed = 180f,
								StrideLength = 46f,
								StrideDuration = 0.96f,
								AnimationKey = "gaitRun"
							}
						}
					},
					{
						"haulHeavy",
						new GaitAnimationBracket[3]
						{
							new GaitAnimationBracket
							{
								MinimumSpeed = 0f,
								MaximumSpeed = 40f,
								StrideLength = 16f,
								StrideDuration = 0.96f,
								AnimationKey = "haulHeavy"
							},
							new GaitAnimationBracket
							{
								MinimumSpeed = 38f,
								MaximumSpeed = 106f,
								StrideLength = 32f,
								StrideDuration = 0.96f,
								AnimationKey = "gaitJog"
							},
							new GaitAnimationBracket
							{
								MinimumSpeed = 102f,
								MaximumSpeed = 180f,
								StrideLength = 46f,
								StrideDuration = 0.96f,
								AnimationKey = "gaitRun"
							}
						}
					},
					{
						"haulHeavyFar",
						new GaitAnimationBracket[3]
						{
							new GaitAnimationBracket
							{
								MinimumSpeed = 0f,
								MaximumSpeed = 40f,
								StrideLength = 22f,
								StrideDuration = 0.96f,
								AnimationKey = "haulHeavyBack"
							},
							new GaitAnimationBracket
							{
								MinimumSpeed = 38f,
								MaximumSpeed = 106f,
								StrideLength = 32f,
								StrideDuration = 0.96f,
								AnimationKey = "gaitJog"
							},
							new GaitAnimationBracket
							{
								MinimumSpeed = 102f,
								MaximumSpeed = 180f,
								StrideLength = 46f,
								StrideDuration = 0.96f,
								AnimationKey = "gaitRun"
							}
						}
					},
					{
						"sneak",
						new GaitAnimationBracket[4]
						{
							new GaitAnimationBracket
							{
								MinimumSpeed = 0f,
								MaximumSpeed = 16f,
								StrideLength = 9f,
								StrideDuration = 0.96f,
								AnimationKey = "gaitSlowWalk"
							},
							new GaitAnimationBracket
							{
								MinimumSpeed = 14f,
								MaximumSpeed = 67f,
								StrideLength = 24f,
								StrideDuration = 0.96f,
								AnimationKey = "gaitSneak"
							},
							new GaitAnimationBracket
							{
								MinimumSpeed = 64f,
								MaximumSpeed = 106f,
								StrideLength = 32f,
								StrideDuration = 0.96f,
								AnimationKey = "gaitJog"
							},
							new GaitAnimationBracket
							{
								MinimumSpeed = 102f,
								MaximumSpeed = 180f,
								StrideLength = 46f,
								StrideDuration = 0.96f,
								AnimationKey = "gaitRun"
							}
						}
					},
					{
						"wounded",
						new GaitAnimationBracket[4]
						{
							new GaitAnimationBracket
							{
								MinimumSpeed = 0f,
								MaximumSpeed = 16f,
								StrideLength = 9f,
								StrideDuration = 0.96f,
								AnimationKey = "gaitSlowWalk"
							},
							new GaitAnimationBracket
							{
								MinimumSpeed = 14f,
								MaximumSpeed = 67f,
								StrideLength = 18f,
								StrideDuration = 0.96f,
								AnimationKey = "gaitLimpWalk"
							},
							new GaitAnimationBracket
							{
								MinimumSpeed = 64f,
								MaximumSpeed = 106f,
								StrideLength = 32f,
								StrideDuration = 0.96f,
								AnimationKey = "gaitJog"
							},
							new GaitAnimationBracket
							{
								MinimumSpeed = 102f,
								MaximumSpeed = 180f,
								StrideLength = 46f,
								StrideDuration = 0.96f,
								AnimationKey = "gaitRun"
							}
						}
					},
					{
						"fatigued",
						new GaitAnimationBracket[4]
						{
							new GaitAnimationBracket
							{
								MinimumSpeed = 0f,
								MaximumSpeed = 16f,
								StrideLength = 9f,
								StrideDuration = 0.96f,
								AnimationKey = "gaitSlowWalk"
							},
							new GaitAnimationBracket
							{
								MinimumSpeed = 14f,
								MaximumSpeed = 67f,
								StrideLength = 20f,
								StrideDuration = 0.96f,
								AnimationKey = "gaitFatiguedWalk"
							},
							new GaitAnimationBracket
							{
								MinimumSpeed = 64f,
								MaximumSpeed = 106f,
								StrideLength = 32f,
								StrideDuration = 0.96f,
								AnimationKey = "gaitJog"
							},
							new GaitAnimationBracket
							{
								MinimumSpeed = 102f,
								MaximumSpeed = 180f,
								StrideLength = 46f,
								StrideDuration = 0.96f,
								AnimationKey = "gaitRun"
							}
						}
					}
				},
				DefaultInfo = new AnimConditionInfo
				{
					SoundAndAnimationSet = new RandomSoundAndAnimationSet
					{
						BaseAnimations = new string[1] { "idle" }
					}
				},
				DefaultStances = new AnimConditionInfo[4]
				{
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "idle" }
						}
					},
					new AnimConditionInfo
					{
						ConditionSet = new AnimConditions
						{
							Modifiers = new BitMask64(typeof(AnimModifier), 15)
						},
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "kneel" }
						}
					},
					new AnimConditionInfo
					{
						ConditionSet = new AnimConditions
						{
							Modifiers = new BitMask64(typeof(AnimModifier), 16)
						},
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "sittingIdle" }
						}
					},
					new AnimConditionInfo
					{
						ConditionSet = new AnimConditions
						{
							Modifiers = new BitMask64(typeof(AnimModifier), 17)
						},
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "sleep" }
						}
					}
				},
				AnimConditions = new AnimConditionInfo[116]
				{
					new AnimConditionInfo
					{
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Moving
						},
						GaitSetKey = "normal"
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							AdditionalAnimations1 = new string[1] { "haulLight" }
						},
						GaitSetKey = "normal",
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Hauling
						},
						Forbiddens = new BitMask64(typeof(AnimModifier), 30)
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							AdditionalAnimations1 = new string[1] { "haulLight" }
						},
						GaitSetKey = "normal",
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Hauling,
							Modifiers = new BitMask64(typeof(AnimModifier), 19)
						},
						Forbiddens = new BitMask64(typeof(AnimModifier), 30)
					},
					new AnimConditionInfo
					{
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Hauling,
							Modifiers = new BitMask64(typeof(AnimModifier), 30)
						},
						GaitSetKey = "haulHeavy"
					},
					new AnimConditionInfo
					{
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Hauling,
							Modifiers = new BitMask64(typeof(AnimModifier), 30, 19)
						},
						GaitSetKey = "haulHeavyFar"
					},
					new AnimConditionInfo
					{
						GaitSetKey = "fatigued",
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Moving,
							Modifiers = new BitMask64(typeof(AnimModifier), 8)
						}
					},
					new AnimConditionInfo
					{
						GaitSetKey = "wounded",
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Moving,
							Modifiers = new BitMask64(typeof(AnimModifier), 7)
						}
					},
					new AnimConditionInfo
					{
						GaitSetKey = "sneak",
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Moving,
							Modifiers = new BitMask64(typeof(AnimModifier), 42)
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "pickupLight" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.PickingUp
						},
						Looping = Looping.No,
						StartingPoint = StartingPoint.FromBeginning
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "pickupHeavy" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.PickingUp,
							Modifiers = new BitMask64(typeof(AnimModifier), 26)
						},
						Looping = Looping.No,
						StartingPoint = StartingPoint.FromBeginning
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "pickupMounted" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.PickingUp,
							Modifiers = new BitMask64(typeof(AnimModifier), 27)
						},
						Looping = Looping.No,
						StartingPoint = StartingPoint.FromBeginning
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "pickupEquipped" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.PickingUp,
							Modifiers = new BitMask64(typeof(AnimModifier), 28)
						},
						Looping = Looping.No,
						StartingPoint = StartingPoint.FromBeginning
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "pickupMountedEquipped" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.PickingUp,
							Modifiers = new BitMask64(typeof(AnimModifier), 27, 28)
						},
						Looping = Looping.No,
						StartingPoint = StartingPoint.FromBeginning
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "pickupHeavy" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.PickingUp,
							Modifiers = new BitMask64(typeof(AnimModifier), 29)
						},
						Looping = Looping.No,
						StartingPoint = StartingPoint.FromBeginning
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "pickupHeavy" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.PickingUp,
							Modifiers = new BitMask64(typeof(AnimModifier), 26, 13)
						},
						Looping = Looping.No,
						StartingPoint = StartingPoint.Specified,
						StartingPointInSeconds = 0.32f
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "pickupLight" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.PickingUp,
							Modifiers = new BitMask64(typeof(AnimModifier), 13)
						},
						Forbiddens = new BitMask64(typeof(AnimModifier), 26),
						Looping = Looping.No,
						StartingPoint = StartingPoint.Specified,
						StartingPointInSeconds = 0.56f
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "dropLight" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dropping
						},
						Looping = Looping.No,
						StartingPoint = StartingPoint.FromBeginning
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "dropLightSame" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dropping,
							Modifiers = new BitMask64(typeof(AnimModifier), 29)
						},
						Looping = Looping.No,
						StartingPoint = StartingPoint.FromBeginning
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "dropHeavy" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dropping,
							Modifiers = new BitMask64(typeof(AnimModifier), 26)
						},
						Looping = Looping.No,
						StartingPoint = StartingPoint.FromBeginning
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "dropMounted" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dropping,
							Modifiers = new BitMask64(typeof(AnimModifier), 27)
						},
						Looping = Looping.No,
						StartingPoint = StartingPoint.FromBeginning
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "dropMountedSame" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dropping,
							Modifiers = new BitMask64(typeof(AnimModifier), 27, 29)
						},
						Looping = Looping.No,
						StartingPoint = StartingPoint.FromBeginning
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "dropEquipped" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dropping,
							Modifiers = new BitMask64(typeof(AnimModifier), 28)
						},
						Looping = Looping.No,
						StartingPoint = StartingPoint.FromBeginning
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "kneelToIdle" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.ChangingStance,
							Modifiers = new BitMask64(typeof(AnimModifier), 15)
						},
						Forbiddens = new BitMask64(typeof(AnimModifier), 16),
						Looping = Looping.No
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "sittingToIdle" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.ChangingStance,
							Modifiers = new BitMask64(typeof(AnimModifier), 16)
						},
						Forbiddens = new BitMask64(typeof(AnimModifier), 15),
						Looping = Looping.No
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "sleepToIdle" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.ChangingStance,
							Modifiers = new BitMask64(typeof(AnimModifier), 17)
						},
						Looping = Looping.No
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "idleToKneel" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.ChangingStance,
							Modifiers = new BitMask64(typeof(AnimModifier), 15, 9)
						},
						Forbiddens = new BitMask64(typeof(AnimModifier), 16),
						Looping = Looping.No
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "idleToSleep" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.ChangingStance,
							Modifiers = new BitMask64(typeof(AnimModifier), 17, 9)
						},
						Looping = Looping.No
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "idleToSitting" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.ChangingStance,
							Modifiers = new BitMask64(typeof(AnimModifier), 16, 9)
						},
						Forbiddens = new BitMask64(typeof(AnimModifier), 15),
						Looping = Looping.No
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "kneelToSitting" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.ChangingStance,
							Modifiers = new BitMask64(typeof(AnimModifier), 16, 15, 9)
						},
						Looping = Looping.No
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[5] { "kneel", "kneelCollectSoil", "kneelExamineSoil", "kneelTablet", "kneelWaterDevice" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Idle,
							Modifiers = new BitMask64(typeof(AnimModifier), 15)
						},
						Looping = Looping.Yes
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "kneelTablet" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Idle,
							Modifiers = new BitMask64(typeof(AnimModifier), 15)
						},
						Looping = Looping.Yes,
						TemporaryRenderablesToAttach = new AnimConditionInfo.TemporaryAttachable[1]
						{
							new AnimConditionInfo.TemporaryAttachable
							{
								RenderableTypeKey = "tablet",
								AttachorTag = "leftHand"
							}
						},
						AttachPoints = new AnimConditionInfo.AttachPointData[1]
						{
							new AnimConditionInfo.AttachPointData
							{
								AttacheePoint = AttacheePoint.RightHand,
								RenderableTypeKey = "tablet",
								Translation = new Vector3(1.601f, 0.026f, 0.184f),
								Rotation = new Vector3(-67.559f, 167.717f, -33.543f)
							}
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[5] { "idle", "idleLong", "idleHandsOnHips", "idleStretchesNeck", "idleWipesNose" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Idle
						},
						Forbiddens = new BitMask64(typeof(AnimModifier), 21)
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[2] { "idleTalkShort", "idleTalkArgue" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Idle,
							Modifiers = new BitMask64(typeof(AnimModifier), 14)
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "exult" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Idle,
							Modifiers = new BitMask64(typeof(AnimModifier), 4)
						},
						Looping = Looping.No
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[3] { "sittingIdle", "sittingAttentive", "sittingMend" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Idle,
							Modifiers = new BitMask64(typeof(AnimModifier), 16)
						},
						Looping = Looping.Yes
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "kneel" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Idle,
							Modifiers = new BitMask64(typeof(AnimModifier), 15, 5)
						},
						Looping = Looping.Yes
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[5] { "kneel", "kneelShieldEyes", "kneelExamineSoil", "kneelCollectSoil", "kneelTablet" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Scouting,
							Modifiers = new BitMask64(typeof(AnimModifier), 15)
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "scout" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Scouting
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "sleep" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Sleeping,
							Modifiers = new BitMask64(typeof(AnimModifier), 17)
						},
						Looping = Looping.Yes
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "mend" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Reloading,
							Modifiers = new BitMask64(typeof(AnimModifier), 15)
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "mend" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Mending,
							Modifiers = new BitMask64(typeof(AnimModifier), 15)
						},
						Looping = Looping.Yes
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[2] { "mend", "mend" },
							Sounds = new string[2] { "activities/crafting/craftingGeneral1", "activities/crafting/craftingGeneral2" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Mending,
							Modifiers = new BitMask64(typeof(AnimModifier), 15, 44)
						},
						Looping = Looping.Yes
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[5] { "constructPull", "idleSweat", "idleStretchesNeck", "chopLow", "idleWipesNose" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Building
						},
						Looping = Looping.Yes
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[5] { "constructPull", "idleSweat", "idleStretchesNeck", "chopLow", "idleWipesNose" },
							Sounds = new string[5] { "activities/salvage/salvagePullMetal", "", "", "activities/gather/gatherChopLow", "" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Building,
							Modifiers = new BitMask64(typeof(AnimModifier), 45)
						},
						Looping = Looping.Yes
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[5] { "constructPull", "idleSweat", "idleStretchesNeck", "chopLow", "idleWipesNose" },
							Sounds = new string[5] { "activities/building/buildingElectronic1", "", "", "activities/building/buildingElectronic2", "" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Building,
							Modifiers = new BitMask64(typeof(AnimModifier), 46)
						},
						Looping = Looping.Yes
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[5] { "constructPull", "idleSweat", "idleStretchesNeck", "chopLow", "idleWipesNose" },
							Sounds = new string[5] { "activities/building/buildingImprovisedKneelWaterDevice", "", "", "activities/gather/gatherChopLow", "" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Building,
							Modifiers = new BitMask64(typeof(AnimModifier), 44)
						},
						Looping = Looping.Yes
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[5] { "constructPull", "idleSweat", "idleStretchesNeck", "chopLow", "idleWipesNose" },
							Sounds = new string[5] { "activities/building/buildingTarpKneelWaterDevice", "", "", "activities/building/buildingTarpKneelDig", "" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Building,
							Modifiers = new BitMask64(typeof(AnimModifier), 43)
						},
						Looping = Looping.Yes
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[5] { "constructPull", "idleSweat", "idleStretchesNeck", "chopLow", "idleWipesNose" },
							Sounds = new string[5] { "activities/building/buildingTarpImprovisedKneelWaterDevice", "", "", "activities/building/buildingTarpImprovisedMend", "" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Building,
							Modifiers = new BitMask64(typeof(AnimModifier), 43, 44)
						},
						Looping = Looping.Yes
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[3] { "mend", "kneelDig", "kneelWaterDevice" },
							Sounds = new string[3] { "activities/building/buildingImprovisedMend", "activities/building/buildingImprovisedKneelDig", "activities/building/buildingImprovisedKneelWaterDevice" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Building,
							Modifiers = new BitMask64(typeof(AnimModifier), 44, 15)
						},
						Looping = Looping.Yes
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[3] { "mend", "kneelDig", "kneelWaterDevice" },
							Sounds = new string[3] { "activities/building/buildingTarpImprovisedMend", "activities/building/buildingTarpImprovisedKneelDig", "activities/building/buildingTarpImprovisedKneelWaterDevice" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Building,
							Modifiers = new BitMask64(typeof(AnimModifier), 44, 43, 15)
						},
						Looping = Looping.Yes
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[3] { "mend", "kneelDig", "kneelWaterDevice" },
							Sounds = new string[3] { "activities/building/buildingTarpMend", "activities/building/buildingTarpKneelDig", "activities/building/buildingTarpKneelWaterDevice" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Building,
							Modifiers = new BitMask64(typeof(AnimModifier), 43, 15)
						},
						Looping = Looping.Yes
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[3] { "mend", "kneelDig", "kneelWaterDevice" },
							Sounds = new string[3] { "activities/building/buildingElectronic1", "activities/building/buildingElectronic2", "activities/building/buildingElectronic1" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Building,
							Modifiers = new BitMask64(typeof(AnimModifier), 46, 15)
						},
						Looping = Looping.Yes
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[3] { "mend", "kneelDig", "kneelWaterDevice" },
							Sounds = new string[3] { "activities/building/buildingMetalMend", "activities/building/buildingMetalKneelDig", "activities/building/buildingMetalKneelWaterDevice" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Building,
							Modifiers = new BitMask64(typeof(AnimModifier), 45, 15)
						},
						Looping = Looping.Yes
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[5] { "constructPull", "cutLow", "idleSweat", "idleStretchesNeck", "idleWipesNose" },
							Sounds = new string[5] { "", "activities/building/buildingSteel", "", "", "" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Building,
							Modifiers = new BitMask64(typeof(AnimModifier), 34)
						},
						Looping = Looping.Yes
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[2] { "mend", "kneelDig" },
							Sounds = new string[2] { "activities/building/buildingSteel", "" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Building,
							Modifiers = new BitMask64(typeof(AnimModifier), 34, 15)
						},
						Looping = Looping.Yes
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[3] { "shovel", "shovel", "gather" },
							Sounds = new string[3] { "activities/farming/farmingHoe1A", "activities/farming/farmingHoe1B", "activities/gather/gatherHandGather" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Building,
							Modifiers = new BitMask64(typeof(AnimModifier), 41, 44)
						},
						Looping = Looping.Yes
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[4] { "constructPull", "chopLow", "idleSweat", "idleStretchesNeck" },
							Sounds = new string[4] { "activities/building/buildingImprovisedKneelWaterDevice", "activities/gather/gatherChopLow", "", "" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Building,
							Modifiers = new BitMask64(typeof(AnimModifier), 35, 21)
						},
						AttachPoints = new AnimConditionInfo.AttachPointData[1]
						{
							new AnimConditionInfo.AttachPointData
							{
								AttacheePoint = AttacheePoint.RightHand,
								RenderableTypeKey = "hammer",
								Translation = new Vector3(-1.942f, 0.157f, 0.052f),
								Rotation = new Vector3(112.913f, -180f, -110.079f)
							}
						},
						Looping = Looping.Yes
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "chopLow" },
							Sounds = new string[1] { "activities/building/buildingHammer" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Building,
							Modifiers = new BitMask64(typeof(AnimModifier), 35, 21)
						},
						AttachPoints = new AnimConditionInfo.AttachPointData[1]
						{
							new AnimConditionInfo.AttachPointData
							{
								AttacheePoint = AttacheePoint.RightHand,
								RenderableTypeKey = "hammer",
								Translation = new Vector3(-1.942f, 0.157f, 0.052f),
								Rotation = new Vector3(112.913f, -180f, -110.079f)
							}
						},
						Looping = Looping.Yes
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[2] { "hammering", "standingMend" },
							Sounds = new string[2] { "activities/building/buildingHammer", "" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Mending,
							Modifiers = new BitMask64(typeof(AnimModifier), 35, 45)
						},
						AttachPoints = new AnimConditionInfo.AttachPointData[1]
						{
							new AnimConditionInfo.AttachPointData
							{
								AttacheePoint = AttacheePoint.RightHand,
								RenderableTypeKey = "hammer",
								Translation = new Vector3(-1.942f, 0.157f, 0.052f),
								Rotation = new Vector3(112.913f, -180f, -110.079f)
							}
						},
						Looping = Looping.Yes
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[4] { "constructPull", "chopLow", "idleWipesNose", "gather" },
							Sounds = new string[4] { "activities/butcher/butcherConstructPull", "activities/butcher/butcherChopLow", "activities/butcher/butcherSharpen", "activities/butcher/butcherGather" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Butchering,
							Modifiers = new BitMask64(typeof(AnimModifier), 36)
						},
						Looping = Looping.Yes
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[4] { "constructPull", "cutLow", "idleWipesNose", "gather" },
							Sounds = new string[4] { "activities/butcher/butcherConstructPull", "activities/butcher/butcherCutLow", "activities/butcher/butcherSharpen", "activities/butcher/butcherGather" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Butchering,
							Modifiers = new BitMask64(typeof(AnimModifier), 34)
						},
						Looping = Looping.Yes
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[6] { "constructPull", "cutLow", "idleSweat", "attackStompRight", "constructPull", "cutLow" },
							Sounds = new string[6] { "activities/salvage/salvagePullMetal", "activities/salvage/salvageMetalCutLow", "", "activities/salvage/salvageBreakMetal2", "activities/salvage/salvagePullMetal", "activities/salvage/salvageMetalCutLow" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Salvaging
						},
						Looping = Looping.Yes
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[4] { "useHoe", "idleSweat", "gather", "useHoe" },
							Sounds = new string[4] { "activities/farming/farmingHoe1A", "", "activities/gather/gatherHandGather", "activities/farming/farmingHoe1B" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Tilling,
							Modifiers = new BitMask64(typeof(AnimModifier), 40)
						},
						AttachPoints = new AnimConditionInfo.AttachPointData[1]
						{
							new AnimConditionInfo.AttachPointData
							{
								AttacheePoint = AttacheePoint.RightHand,
								RenderableTypeKey = "farmingHoe",
								Translation = new Vector3(1.916f, 4.331f, -0.026f),
								Rotation = new Vector3(92.12601f, -168.661f, 167.717f)
							}
						},
						Looping = Looping.Yes
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[4] { "shovel", "idleWipesNose", "gather", "shovel" },
							Sounds = new string[4] { "activities/farming/farmingHoe1A", "", "activities/gather/gatherHandGather", "activities/farming/farmingHoe1B" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Tilling,
							Modifiers = new BitMask64(typeof(AnimModifier), 41)
						},
						AttachPoints = new AnimConditionInfo.AttachPointData[1]
						{
							new AnimConditionInfo.AttachPointData
							{
								AttacheePoint = AttacheePoint.RightHand,
								RenderableTypeKey = "shovel",
								Translation = new Vector3(2.467f, 2.362f, -0.367f),
								Rotation = new Vector3(92.12601f, -168.661f, 162.992f)
							}
						},
						Looping = Looping.Yes
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "eat" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Eating
						},
						Looping = Looping.Yes
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[3] { "gather", "kneelDig", "kneelExamineSoil" },
							Sounds = new string[3] { "activities/gather/gatherHandGather", "activities/gather/gatherHandKneelDig", "" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Harvesting,
							Modifiers = new BitMask64(typeof(AnimModifier), 15)
						},
						Looping = Looping.Yes,
						Forbiddens = new BitMask64(typeof(AnimModifier), 36, 39, 32, 31, 33)
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "chopLow" },
							Sounds = new string[1] { "activities/gather/gatherChopLow" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Harvesting,
							Modifiers = new BitMask64(typeof(AnimModifier), 36, 37, 21)
						},
						AttachPoints = new AnimConditionInfo.AttachPointData[2]
						{
							new AnimConditionInfo.AttachPointData
							{
								AttacheePoint = AttacheePoint.RightHand,
								RenderableTypeKey = "machete",
								Translation = new Vector3(0.236f, -0.236f, -0.026f),
								Rotation = new Vector3(139.37f, -177.165f, -102.52f)
							},
							new AnimConditionInfo.AttachPointData
							{
								AttacheePoint = AttacheePoint.RightHand,
								RenderableTypeKey = "axe",
								Translation = new Vector3(-1.942f, 0.157f, 0.052f),
								Rotation = new Vector3(139.37f, -145.039f, -83.622f)
							}
						},
						Looping = Looping.Yes
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[2] { "fishingSpearIdle", "fishingSpearThrustMiss" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Fishing,
							Modifiers = new BitMask64(typeof(AnimModifier), 31)
						},
						AttachPoints = new AnimConditionInfo.AttachPointData[1]
						{
							new AnimConditionInfo.AttachPointData
							{
								AttacheePoint = AttacheePoint.RightHand,
								RenderableTypeKey = "spear",
								Translation = new Vector3(0.499f, -1.234f, 0.079f),
								Rotation = new Vector3(86.457f, 1.417f, -4.252f)
							}
						},
						Looping = Looping.Yes
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "fishingHook" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Fishing
						},
						Looping = Looping.Yes
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[3] { "shovel", "shovel", "gather" },
							Sounds = new string[3] { "activities/farming/farmingHoe1A", "activities/farming/farmingHoe1B", "activities/gather/gatherHandGather" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Digging,
							Modifiers = new BitMask64(typeof(AnimModifier), 41)
						},
						AttachPoints = new AnimConditionInfo.AttachPointData[1]
						{
							new AnimConditionInfo.AttachPointData
							{
								AttacheePoint = AttacheePoint.RightHand,
								RenderableTypeKey = "shovel",
								Translation = new Vector3(2.467f, 2.362f, -0.367f),
								Rotation = new Vector3(92.12601f, -168.661f, 162.992f)
							}
						},
						Looping = Looping.Yes
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[3] { "pickaxe", "pickaxe", "gather" },
							Sounds = new string[3] { "activities/farming/farmingHoe1A", "activities/farming/farmingHoe1B", "activities/gather/gatherHandGather" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Digging,
							Modifiers = new BitMask64(typeof(AnimModifier), 38)
						},
						AttachPoints = new AnimConditionInfo.AttachPointData[1]
						{
							new AnimConditionInfo.AttachPointData
							{
								AttacheePoint = AttacheePoint.RightHand,
								RenderableTypeKey = "pickaxe",
								Translation = new Vector3(-1.627f, -2.677f, -0.367f),
								Rotation = new Vector3(139.37f, -177.165f, -102.52f)
							}
						},
						Looping = Looping.Yes
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[4] { "gather", "kneelDig", "kneelExamineSoil", "idleSweat" },
							Sounds = new string[4] { "activities/gather/gatherHandGather", "activities/gather/gatherHandKneelDig", "", "" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Digging,
							Modifiers = new BitMask64(typeof(AnimModifier), 15)
						},
						Looping = Looping.Yes,
						Forbiddens = new BitMask64(typeof(AnimModifier), 36, 39, 32, 31, 33)
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "cutLow" },
							Sounds = new string[1] { "activities/gather/gatherCutLow" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Harvesting,
							Modifiers = new BitMask64(typeof(AnimModifier), 34, 21)
						},
						Looping = Looping.Yes
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "gatherStand" },
							Sounds = new string[1] { "activities/gather/gatherHandGatherStand" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Harvesting
						},
						Looping = Looping.Yes
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "gatherStand" },
							Sounds = new string[1] { "activities/gather/gatherHandGatherStand" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Harvesting,
							Modifiers = new BitMask64(typeof(AnimModifier), 21)
						},
						Looping = Looping.Yes
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "combatIdleUnarmed" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Idle,
							Modifiers = new BitMask64(typeof(AnimModifier), 6)
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "combatHit" }
						},
						Playback = Playback.Manual,
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Recoiling,
							Modifiers = new BitMask64(typeof(AnimModifier), 6)
						},
						Looping = Looping.No
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "combatHit" }
						},
						Playback = Playback.Manual,
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Recoiling
						},
						Looping = Looping.No
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "combatCollapse" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying,
							Modifiers = new BitMask64(typeof(AnimModifier), 12)
						},
						Looping = Looping.No
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "combatCollapse" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying,
							Modifiers = new BitMask64(typeof(AnimModifier), 12, 7)
						},
						Looping = Looping.No
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "combatCollapse" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying,
							Modifiers = new BitMask64(typeof(AnimModifier), 12, 6)
						},
						Looping = Looping.No
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "combatCollapse" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying,
							Modifiers = new BitMask64(typeof(AnimModifier), 12, 6, 7)
						},
						Looping = Looping.No
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "dying" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying
						},
						Looping = Looping.No
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "dying" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying,
							Modifiers = new BitMask64(typeof(AnimModifier), 7)
						},
						Looping = Looping.No
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "dead" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying,
							Modifiers = new BitMask64(typeof(AnimModifier), 13)
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "dead" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Dying,
							Modifiers = new BitMask64(typeof(AnimModifier), 13, 7)
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "attackPunchRightMiss" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Attacking,
							Modifiers = new BitMask64(typeof(AnimModifier), 24)
						},
						Forbiddens = new BitMask64(typeof(AnimModifier), 32, 39, 33),
						Looping = Looping.No
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "attackStompRight" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Attacking,
							Modifiers = new BitMask64(typeof(AnimModifier), 21, 1, 18, 25)
						},
						Looping = Looping.No
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "attackSnapkickRight" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Attacking,
							Modifiers = new BitMask64(typeof(AnimModifier), 21, 1, 18)
						},
						Looping = Looping.No
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "attackPunchLowRight" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Attacking,
							Modifiers = new BitMask64(typeof(AnimModifier), 1, 18)
						},
						Looping = Looping.No
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "attackPunchHighRight" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Attacking,
							Modifiers = new BitMask64(typeof(AnimModifier), 20, 1, 18)
						},
						Looping = Looping.No
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "attackClubHigh" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Attacking,
							Modifiers = new BitMask64(typeof(AnimModifier), 34, 1, 18)
						},
						Looping = Looping.No
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "attackClubHigh" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Attacking,
							Modifiers = new BitMask64(typeof(AnimModifier), 35, 1, 18)
						},
						Looping = Looping.No,
						AttachPoints = new AnimConditionInfo.AttachPointData[1]
						{
							new AnimConditionInfo.AttachPointData
							{
								AttacheePoint = AttacheePoint.RightHand,
								RenderableTypeKey = "hammer",
								Translation = new Vector3(-1.942f, 0.157f, -0.367f),
								Rotation = new Vector3(108.189f, -161.102f, -95.906f)
							}
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "attackClubHigh" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Attacking,
							Modifiers = new BitMask64(typeof(AnimModifier), 37, 1, 18)
						},
						Looping = Looping.No,
						AttachPoints = new AnimConditionInfo.AttachPointData[1]
						{
							new AnimConditionInfo.AttachPointData
							{
								AttacheePoint = AttacheePoint.RightHand,
								RenderableTypeKey = "axe",
								Translation = new Vector3(-1.942f, 0.157f, -0.367f),
								Rotation = new Vector3(108.189f, -161.102f, -95.906f)
							}
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "attackClubHigh" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Attacking,
							Modifiers = new BitMask64(typeof(AnimModifier), 36, 1, 18)
						},
						Looping = Looping.No,
						AttachPoints = new AnimConditionInfo.AttachPointData[1]
						{
							new AnimConditionInfo.AttachPointData
							{
								AttacheePoint = AttacheePoint.RightHand,
								RenderableTypeKey = "machete",
								Translation = new Vector3(0.236f, -0.236f, -0.026f),
								Rotation = new Vector3(139.37f, -177.165f, -102.52f)
							}
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "attackPickaxe" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Attacking,
							Modifiers = new BitMask64(typeof(AnimModifier), 38, 1, 18)
						},
						Looping = Looping.No,
						AttachPoints = new AnimConditionInfo.AttachPointData[1]
						{
							new AnimConditionInfo.AttachPointData
							{
								AttacheePoint = AttacheePoint.RightHand,
								RenderableTypeKey = "pickaxe",
								Translation = new Vector3(-2.467f, -4.462f, -0.026f),
								Rotation = new Vector3(86.457f, -177.165f, -103.465f)
							}
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "combatIdleRifle" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Idle,
							Modifiers = new BitMask64(typeof(AnimModifier), 32, 6)
						},
						AttachPoints = new AnimConditionInfo.AttachPointData[1]
						{
							new AnimConditionInfo.AttachPointData
							{
								AttacheePoint = AttacheePoint.RightHand,
								RenderableTypeKey = "rifle",
								Translation = new Vector3(1.129f, 0.814f, 0.709f),
								Rotation = new Vector3(-7.087f, 37.323f, 9.921f)
							}
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "idle" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Idle,
							Modifiers = new BitMask64(typeof(AnimModifier), 32)
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "attackHoeMid" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Attacking,
							Modifiers = new BitMask64(typeof(AnimModifier), 32, 18)
						},
						Looping = Looping.No,
						AttachPoints = new AnimConditionInfo.AttachPointData[1]
						{
							new AnimConditionInfo.AttachPointData
							{
								AttacheePoint = AttacheePoint.RightHand,
								RenderableTypeKey = "rifle",
								Translation = new Vector3(0.9710001f, 5.066f, 1.601f),
								Rotation = new Vector3(-109.134f, 61.89f, 157.323f)
							}
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "combatRifleAim" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Attacking,
							Modifiers = new BitMask64(typeof(AnimModifier), 32, 19)
						},
						Looping = Looping.No,
						AttachPoints = new AnimConditionInfo.AttachPointData[1]
						{
							new AnimConditionInfo.AttachPointData
							{
								AttacheePoint = AttacheePoint.RightHand,
								RenderableTypeKey = "rifle",
								Translation = new Vector3(1.129f, 0.814f, 0.709f),
								Rotation = new Vector3(-7.087f, 37.323f, 9.921f)
							}
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "combatRifleAim" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Attacking,
							Modifiers = new BitMask64(typeof(AnimModifier), 32, 19, 24)
						},
						Looping = Looping.No,
						AttachPoints = new AnimConditionInfo.AttachPointData[1]
						{
							new AnimConditionInfo.AttachPointData
							{
								AttacheePoint = AttacheePoint.RightHand,
								RenderableTypeKey = "rifle",
								Translation = new Vector3(1.129f, 0.814f, 0.709f),
								Rotation = new Vector3(-7.087f, 37.323f, 9.921f)
							}
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "combatRifleReload" },
							Sounds = new string[1] { "activities/weapons/coilrifleReload" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Reloading,
							Modifiers = new BitMask64(typeof(AnimModifier), 32)
						},
						AttachPoints = new AnimConditionInfo.AttachPointData[1]
						{
							new AnimConditionInfo.AttachPointData
							{
								AttacheePoint = AttacheePoint.RightHand,
								RenderableTypeKey = "rifle",
								Translation = new Vector3(1.129f, 0.814f, 0.709f),
								Rotation = new Vector3(-7.087f, 37.323f, 9.921f)
							}
						},
						Looping = Looping.No
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "combatIdleRifle" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Idle,
							Modifiers = new BitMask64(typeof(AnimModifier), 33, 6)
						},
						AttachPoints = new AnimConditionInfo.AttachPointData[1]
						{
							new AnimConditionInfo.AttachPointData
							{
								AttacheePoint = AttacheePoint.RightHand,
								RenderableTypeKey = "watergun",
								Translation = new Vector3(1.129f, 0.814f, 0.709f),
								Rotation = new Vector3(-7.087f, 37.323f, 9.921f)
							}
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "idle" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Idle,
							Modifiers = new BitMask64(typeof(AnimModifier), 33)
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "combatRifleAimHip" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Attacking,
							Modifiers = new BitMask64(typeof(AnimModifier), 33, 19, 21)
						},
						Looping = Looping.No,
						AttachPoints = new AnimConditionInfo.AttachPointData[1]
						{
							new AnimConditionInfo.AttachPointData
							{
								AttacheePoint = AttacheePoint.RightHand,
								RenderableTypeKey = "watergun",
								Translation = new Vector3(1.129f, 0.814f, 0.709f),
								Rotation = new Vector3(-7.087f, 37.323f, 9.921f)
							}
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "combatWatergunReload" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Reloading,
							Modifiers = new BitMask64(typeof(AnimModifier), 33)
						},
						Playback = Playback.Manual,
						Looping = Looping.No,
						AttachPoints = new AnimConditionInfo.AttachPointData[1]
						{
							new AnimConditionInfo.AttachPointData
							{
								AttacheePoint = AttacheePoint.RightHand,
								RenderableTypeKey = "watergun",
								Translation = new Vector3(1.129f, 0.814f, 0.709f),
								Rotation = new Vector3(-7.087f, 37.323f, 9.921f)
							}
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "combatIdleBow" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Idle,
							Modifiers = new BitMask64(typeof(AnimModifier), 39, 6)
						},
						AttachPoints = new AnimConditionInfo.AttachPointData[1]
						{
							new AnimConditionInfo.AttachPointData
							{
								AttacheePoint = AttacheePoint.RightHand,
								RenderableTypeKey = "bow",
								Translation = new Vector3(0.341f, 0.184f, -0.026f),
								Rotation = new Vector3(-55.276f, 180f, 75.118f)
							}
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "idle" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Idle,
							Modifiers = new BitMask64(typeof(AnimModifier), 39)
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "combatBowAim" },
							Sounds = new string[1] { "activities/weapons/bow/bowAim2A" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Attacking,
							Modifiers = new BitMask64(typeof(AnimModifier), 39, 19)
						},
						Looping = Looping.No,
						AttachPoints = new AnimConditionInfo.AttachPointData[1]
						{
							new AnimConditionInfo.AttachPointData
							{
								AttacheePoint = AttacheePoint.RightHand,
								RenderableTypeKey = "bow",
								Translation = new Vector3(0.341f, 0.184f, -0.026f),
								Rotation = new Vector3(-55.276f, 180f, 75.118f)
							}
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "combatBowReload" },
							Sounds = new string[1] { "activities/weapons/bow/bowReload2A" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Reloading,
							Modifiers = new BitMask64(typeof(AnimModifier), 39)
						},
						Playback = Playback.Manual,
						Looping = Looping.No,
						AttachPoints = new AnimConditionInfo.AttachPointData[1]
						{
							new AnimConditionInfo.AttachPointData
							{
								AttacheePoint = AttacheePoint.RightHand,
								RenderableTypeKey = "bow",
								Translation = new Vector3(0.341f, 0.184f, -0.026f),
								Rotation = new Vector3(-55.276f, 180f, 75.118f)
							}
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "combatIdleSpear" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Idle,
							Modifiers = new BitMask64(typeof(AnimModifier), 6, 31)
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "idle" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Idle,
							Modifiers = new BitMask64(typeof(AnimModifier), 31)
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "attackSpearMid" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Attacking,
							Modifiers = new BitMask64(typeof(AnimModifier), 31, 18)
						},
						Looping = Looping.No,
						AttachPoints = new AnimConditionInfo.AttachPointData[1]
						{
							new AnimConditionInfo.AttachPointData
							{
								AttacheePoint = AttacheePoint.RightHand,
								RenderableTypeKey = "spear",
								Translation = new Vector3(0.814f, 6.85f, 1.391f),
								Rotation = new Vector3(-77.008f, 36.378f, 4.252f)
							}
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "attackHoeMid" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Attacking,
							Modifiers = new BitMask64(typeof(AnimModifier), 40, 18)
						},
						Looping = Looping.No,
						AttachPoints = new AnimConditionInfo.AttachPointData[1]
						{
							new AnimConditionInfo.AttachPointData
							{
								AttacheePoint = AttacheePoint.RightHand,
								RenderableTypeKey = "farmingHoe",
								Translation = new Vector3(0.9710001f, 5.066f, 1.601f),
								Rotation = new Vector3(66.61401f, 61.89f, 157.323f)
							}
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "attackHoeMid" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Attacking,
							Modifiers = new BitMask64(typeof(AnimModifier), 41, 18)
						},
						Looping = Looping.No,
						AttachPoints = new AnimConditionInfo.AttachPointData[1]
						{
							new AnimConditionInfo.AttachPointData
							{
								AttacheePoint = AttacheePoint.RightHand,
								RenderableTypeKey = "shovel",
								Translation = new Vector3(0.9710001f, 5.066f, 1.601f),
								Rotation = new Vector3(66.61401f, 61.89f, 157.323f)
							}
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "combatSpearThrow" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Attacking,
							Modifiers = new BitMask64(typeof(AnimModifier), 31, 19)
						},
						Looping = Looping.No,
						AttachPoints = new AnimConditionInfo.AttachPointData[1]
						{
							new AnimConditionInfo.AttachPointData
							{
								AttacheePoint = AttacheePoint.RightHand,
								RenderableTypeKey = "spear",
								Translation = new Vector3(0.499f, -1.234f, 0.079f),
								Rotation = new Vector3(86.457f, 1.417f, -4.252f)
							}
						}
					}
				}
			},
			AnimatedHeadType = new AnimatedHeadType
			{
				SpineBones = new string[5] { "SpineB", "SpineC", "SpineD", "Neck", "Head" },
				TurnToLookLerpFactor = 0.06f,
				PitchForward = 0.12f
			},
			BoxHandlingWhenHauling = new BoxHandlingWhenHauling
			{
				BoxHandling = BoxHandlingWhenHauling.BoxHandlingType.OnlyOnBackWhenHeavyAndHaulingFar,
				UseHeavyBackpack = true,
				ShowBoxInHand = AttacheePoint.RightHand,
				AttachorWhenBoxIsInHand = "rightHand"
			}
		};
		entityType.LocomotorType = new LocomotorType
		{
			MaxAngularSpeed = 7.853982f,
			MeleeRadius = 12f,
			CanRun = true,
			Stances = "humanoid",
			LeggedLocomotorType = new LeggedLocomotorType
			{
				TerrainNegateFactor = 0.1f,
				WalkSlowSpeed = 25f,
				HaulSpeed = 40f,
				WalkNormalSpeed = 66f,
				WalkFastSpeed = 70f,
				RunSpeed = 88f
			},
			CollisionResponderType = new CollisionResponderType
			{
				AgentCollisionResponderType = new AgentCollisionResponderType()
			}
		};
		entityType.SensorType = new SensorType
		{
			Range = 350f,
			RangeAtNight = 200f,
			DetectionTypeKey = "human"
		};
		entityType.ContainerType = new AgentStorageType
		{
			ItemStorageType = new ItemStorageType(1f),
			EquipmentStorageType = new ItemStorageType(0.5f),
			StomachStorageType = new ItemStorageType(0.07f)
		};
		entityType.BodyType = bodyType;
		EntityType entityType18 = entityType;
		entityType18.IntelligenceType = new IntelligenceType
		{
			IsMobile = true,
			RespectsOwnership = true,
			AllowEscapeFromTinyAreas = true,
			CanAttack = true,
			CanTradeAndCommunicate = true,
			CanProduce = true,
			CanDoJobs = true,
			CanSpeak = true,
			CanUseWeapons = true,
			CanCheckProgress = true,
			CanUseGadgets = true,
			CanEmigrate = true,
			CanMountTools = true,
			CanReplenish = true,
			CanHaul = true,
			CanPatrol = true,
			CanHunt = true,
			CanScout = true,
			CanExamine = true,
			IsPredator = true,
			OtherAgentsNearExpeditionCenterAreConsideredThreats = true,
			IdleChanceToTalk = 0.2f,
			ContainerTransactTag = "humanTransact",
			HasServantsTags = new string[1] { "servesHumans" },
			Prey = new string[16]
			{
				"entity:mudWorm", "entity:binalRat", "entity:whiteThunderChicken", "entity:pygmyThunderChicken", "entity:studdedThunderChicken", "entity:bajingan", "entity:twinkler", "entity:fieldQuadite", "entity:turnip", "entity:patrician",
				"entity:megapod", "entity:whipjaw", "entity:lesserWhipjaw", "entity:spoakDendront", "entity:swampDendront", "entity:bushDragon"
			},
			Attacks = new string[4] { "personPunchHighRight", "personPunchLowRight", "personSnapkickRight", "personStompRight" },
			InterestInTriggerTypes = new string[2] { "entityDied", "creature" },
			AggroRange = 160f,
			ChanceToRestAfterMeleeAttack = 0.6,
			ChanceToRestAfterRangedAttack = 0.6,
			MinRestTimeAfterAttackingInSeconds = 0.5f,
			MaxRestTimeAfterAttackingInSeconds = 1.2f,
			ChanceToIdleWalkShortDistanceAway = 0.2f,
			ShortIdleWalkMaxDistance = 120f,
			ShortIdleWalkMinDistance = 48f,
			Skills = new SerializableDictionary<string, float>
			{
				{ "fruitPicking", 1f },
				{ "grasping", 1f }
			},
			DropLightDuration = 1.2f,
			DropLightActionPointDuration = 0.64f,
			DropHeavyDuration = 0.92f,
			DropHeavyActionPointDuration = 0.44f,
			pickupMountedEquippedDuration = 1.72f,
			pickupMountedEquippedActionPointDuration = 0.48f,
			pickupEquipDuration = 1.72f,
			pickupEquipActionPointDuration = 0.48f,
			PickupLightDuration = 1.2f,
			PickupLightActionPointDuration = 0.56f,
			PickupHeavyDuration = 0.68f,
			PickupHeavyActionPointDuration = 0.32f,
			pickupMountDuration = 0.96f,
			pickupMountActionPointDuration = 0.48f,
			SwitchLightToLightDuration = 2.4f,
			SwitchLightToLightActionPointDuration = 1.2f,
			dropEquippedDuration = 1.32f,
			dropEquippedActionPointDuration = 0.8f,
			dropMountedDuration = 0.96f,
			dropMountedActionPointDuration = 0.36f,
			DropLightToLightActionPointDuration = 0.6f,
			DropLightToLightDuration = 1.24f,
			dropMountToMountActionPointDuration = 0.5f,
			dropMountToMountDuration = 1f
		};
		entityType18.BiologicalType = new BiologicalType
		{
			SpeciesPlural = "humans",
			HoldsFoodWhenEating = true,
			EatingStances = new ChanceToTakeStance[1]
			{
				new ChanceToTakeStance
				{
					Stance = "sitting"
				}
			},
			OxygenAndMuscleEnergyIncreaseRatePerDay = 10f,
			FoodItemTagsThatCanBeConsumed = new string[4] { "cookedMeat", "edibleVegi", "coffee", "alcoholicBeverage" },
			TimeToConsumeFullMealInDays = 0.0065f,
			StomachSizeFractionOfEntityBulk = 0.225f,
			StomachContentsDecreaseRatePerDay = 2f,
			ActiveStealthRating = 0f,
			MaxRegainLimit = 0.5f,
			FractionOfMaxHitpointsGainedPerDay = 0.3f,
			TimeOfDayToGoToSleep = 0.95,
			ModelBasicTextureName = "ManGrey2Texture",
			Carcass = "item:body",
			RaceTypes = new RaceType[30]
			{
				new RaceType
				{
					KeyName = "white1",
					Name = "White",
					PortraitSkinType = "Celtic",
					PrimaryColor = new Vector3(0.9803922f, 83f / 85f, 0.9686275f),
					Edge = 0.2f,
					SecondaryColorProbabilityEdges = new List<ColorProbability>
					{
						new ColorProbability(0.1f, new Vector3(0.9333333f, 46f / 51f, 0.8588235f)),
						new ColorProbability(0.5f, new Vector3(0.8941177f, 0.8313726f, 0.7019608f)),
						new ColorProbability(0.9f, new Vector3(0.772549f, 0.5254902f, 0.2627451f)),
						new ColorProbability(1f, new Vector3(0.7843137f, 2f / 3f, 0.5333334f))
					}
				},
				new RaceType
				{
					KeyName = "white2",
					Name = "White",
					PortraitSkinType = "Light European",
					PrimaryColor = new Vector3(81f / 85f, 0.9176471f, 0.8980392f),
					Edge = 0.5f,
					SecondaryColorProbabilityEdges = new List<ColorProbability>
					{
						new ColorProbability(0.1f, new Vector3(0.9333333f, 46f / 51f, 0.8588235f)),
						new ColorProbability(0.2f, new Vector3(0.8941177f, 0.8313726f, 0.7019608f)),
						new ColorProbability(0.3f, new Vector3(0.772549f, 0.5254902f, 0.2627451f)),
						new ColorProbability(0.5f, new Vector3(0.7843137f, 2f / 3f, 0.5333334f)),
						new ColorProbability(0.6f, new Vector3(0.654902f, 0.5019608f, 0.3803922f)),
						new ColorProbability(0.7f, new Vector3(0.5137255f, 0.4078431f, 0.3254902f)),
						new ColorProbability(0.8f, new Vector3(33f / 85f, 0.2352941f, 0.2078431f)),
						new ColorProbability(0.9f, new Vector3(0.2509804f, 16f / 85f, 16f / 85f)),
						new ColorProbability(1f, new Vector3(0.09019608f, 7f / 85f, 8f / 85f))
					}
				},
				new RaceType
				{
					KeyName = "white3",
					Name = "White",
					PortraitSkinType = "Average Caucasian",
					PrimaryColor = new Vector3(0.9960784f, 82f / 85f, 0.8823529f),
					Edge = 0.7f,
					SecondaryColorProbabilityEdges = new List<ColorProbability>
					{
						new ColorProbability(0.1f, new Vector3(0.7843137f, 2f / 3f, 0.5333334f)),
						new ColorProbability(0.2f, new Vector3(0.654902f, 0.5019608f, 0.3803922f)),
						new ColorProbability(0.7f, new Vector3(0.5137255f, 0.4078431f, 0.3254902f)),
						new ColorProbability(0.8f, new Vector3(33f / 85f, 0.2352941f, 0.2078431f)),
						new ColorProbability(0.9f, new Vector3(0.2509804f, 16f / 85f, 16f / 85f)),
						new ColorProbability(1f, new Vector3(0.09019608f, 7f / 85f, 8f / 85f))
					}
				},
				new RaceType
				{
					KeyName = "hispanic",
					Name = "Hispanic",
					PortraitSkinType = "Olive skin",
					PrimaryColor = new Vector3(0.9215686f, 0.8392157f, 0.6235294f),
					Edge = 0.8f,
					SecondaryColorProbabilityEdges = new List<ColorProbability>
					{
						new ColorProbability(0.1f, new Vector3(33f / 85f, 0.2352941f, 0.2078431f)),
						new ColorProbability(0.7f, new Vector3(0.2509804f, 16f / 85f, 16f / 85f)),
						new ColorProbability(1f, new Vector3(0.09019608f, 7f / 85f, 8f / 85f))
					}
				},
				new RaceType
				{
					KeyName = "black1",
					Name = "Black",
					PortraitSkinType = "Dark",
					PrimaryColor = new Vector3(52f / 85f, 0.4196078f, 0.2627451f),
					Edge = 0.85f,
					SecondaryColorProbabilityEdges = new List<ColorProbability>
					{
						new ColorProbability(0.2f, new Vector3(33f / 85f, 0.2352941f, 0.2078431f)),
						new ColorProbability(0.7f, new Vector3(0.2509804f, 16f / 85f, 16f / 85f)),
						new ColorProbability(1f, new Vector3(0.09019608f, 7f / 85f, 8f / 85f))
					}
				},
				new RaceType
				{
					KeyName = "black2",
					Name = "Black",
					PortraitSkinType = "Black",
					PrimaryColor = new Vector3(0.3411765f, 0.1960784f, 0.1607843f),
					Edge = 0.9f,
					SecondaryColorProbabilityEdges = new List<ColorProbability>
					{
						new ColorProbability(0.4f, new Vector3(0.2509804f, 16f / 85f, 16f / 85f)),
						new ColorProbability(1f, new Vector3(0.09019608f, 7f / 85f, 8f / 85f))
					}
				},
				new RaceType
				{
					KeyName = "asian",
					Name = "Asian",
					PortraitSkinType = "East Asian",
					PrimaryColor = new Vector3(81f / 85f, 0.9176471f, 0.8980392f),
					Edge = 1f,
					SecondaryColorProbabilityEdges = new List<ColorProbability>
					{
						new ColorProbability(0.1f, new Vector3(0.2509804f, 16f / 85f, 16f / 85f)),
						new ColorProbability(1f, new Vector3(0.09019608f, 7f / 85f, 8f / 85f))
					}
				},
				new RaceType
				{
					KeyName = "whiteHumanDescendant",
					Name = "White Human Descendant",
					PortraitSkinType = "whitePortrait",
					ModelBasicTextureNames = new string[12]
					{
						"ManBlueBrownClothesBrownHairTexture", "ManOrangeGreyClothesYellowHairTexture", "ManCurryClothesRedHairTexture", "ManTurquoiseDarkClothesBlondHairTexture", "ManBurgundyClothesWhiteHairTexture", "ManDarkBlueBeigeClothesBrownHairTexture", "ManOrangeDarkGreyClothesYellowHairTexture", "ManDarkBrownClothesRedHairTexture", "ManBlueGreyClothesYellowHairTexture", "ManOrangeGreyClothesRedHairTexture",
						"ManBrownGreyClothesBlondHairTexture", "ManSandyDarkClothesWhiteHairTexture"
					},
					PrimaryColor = new Vector3(52f / 85f, 0.4196078f, 0.2627451f),
					Edge = 1f,
					SecondaryColorProbabilityEdges = new List<ColorProbability>
					{
						new ColorProbability(1f, new Vector3(0.2509804f, 16f / 85f, 16f / 85f))
					}
				},
				new RaceType
				{
					KeyName = "asianHumanDescendant",
					Name = "Asian Human Descendant",
					PortraitSkinType = "asianPortrait",
					ModelBasicTextureNames = new string[4] { "ManSandyClothesBlackHairTexture", "ManCurryGreyClothesBrownSkinTexture", "ManOchreClothesBlackHairTexture", "ManTurquoiseDarkClothesBlackHairTexture" },
					PrimaryColor = new Vector3(52f / 85f, 0.4196078f, 0.2627451f),
					Edge = 1f,
					SecondaryColorProbabilityEdges = new List<ColorProbability>
					{
						new ColorProbability(1f, new Vector3(0.2509804f, 16f / 85f, 16f / 85f))
					}
				},
				new RaceType
				{
					KeyName = "hispanicHumanDescendant",
					Name = "Hispanic Human Descendant",
					PortraitSkinType = "hispanicPortrait",
					ModelBasicTextureNames = new string[7] { "ManBlueBrownClothesBrownHairTexture", "ManDarkRedClothesDarkSkinBrownHairTexture", "ManSandyClothesBlackHairTexture", "ManDarkBlueBeigeClothesBrownHairTexture", "ManCurryGreyClothesBrownSkinTexture", "ManOchreClothesBlackHairTexture", "ManTurquoiseDarkClothesBlackHairTexture" },
					PrimaryColor = new Vector3(52f / 85f, 0.4196078f, 0.2627451f),
					Edge = 1f,
					SecondaryColorProbabilityEdges = new List<ColorProbability>
					{
						new ColorProbability(1f, new Vector3(0.2509804f, 16f / 85f, 16f / 85f))
					}
				},
				new RaceType
				{
					KeyName = "blackHumanDescendant",
					Name = "Black Human Descendant",
					PortraitSkinType = "blackPortrait",
					ModelBasicTextureNames = new string[5] { "ManDarkRedClothesDarkSkinBrownHairTexture", "ManBrownGreyClothesDarkSkinTexture", "ManBrownBeigeClothesDarkSkinTexture", "ManSandyClothesDarkSkinTexture", "ManCurryGreyClothesBrownSkinTexture" },
					PrimaryColor = new Vector3(52f / 85f, 0.4196078f, 0.2627451f),
					Edge = 1f,
					SecondaryColorProbabilityEdges = new List<ColorProbability>
					{
						new ColorProbability(1f, new Vector3(0.2509804f, 16f / 85f, 16f / 85f))
					}
				},
				new RaceType
				{
					KeyName = "whiteHumanAncestor",
					Name = "White Human Ancestor",
					PortraitSkinType = "whitePortrait",
					ModelBasicTextureNames = new string[17]
					{
						"ManBlue1Texture", "ManRed1Texture", "ManRed2Texture", "ManGreen1Texture", "ManGreen2Texture", "ManGrey1Texture", "ManGrey2Texture", "ManGrey3Texture", "ManBlueSolid1Texture", "ManGreenSolid1Texture",
						"ManGreySolid1Texture", "ManGreySolid2Texture", "ManGreyClothes1Texture", "ManWhitePantsClothes1Texture", "ManWhiteBlueClothes1Texture", "ManBlueGreyClothesYellowHairTexture", "ManGreenBlueClothes1Texture"
					},
					PrimaryColor = new Vector3(52f / 85f, 0.4196078f, 0.2627451f),
					Edge = 1f,
					SecondaryColorProbabilityEdges = new List<ColorProbability>
					{
						new ColorProbability(1f, new Vector3(0.2509804f, 16f / 85f, 16f / 85f))
					}
				},
				new RaceType
				{
					KeyName = "asianHumanAncestor",
					Name = "Asian Human Ancestor",
					PortraitSkinType = "asianPortrait",
					ModelBasicTextureNames = new string[5] { "ManRed1Texture", "ManGreen1Texture", "ManGreySolid2Texture", "ManWhitePantsClothes1Texture", "ManTurquoiseDarkClothesBlackHairTexture" },
					PrimaryColor = new Vector3(52f / 85f, 0.4196078f, 0.2627451f),
					Edge = 1f,
					SecondaryColorProbabilityEdges = new List<ColorProbability>
					{
						new ColorProbability(1f, new Vector3(0.2509804f, 16f / 85f, 16f / 85f))
					}
				},
				new RaceType
				{
					KeyName = "hispanicHumanAncestor",
					Name = "Hispanic Human Ancestor",
					PortraitSkinType = "hispanicPortrait",
					ModelBasicTextureNames = new string[10] { "ManGreen2Texture", "ManGrey2Texture", "ManGrey3Texture", "ManRed1Texture", "ManGreen1Texture", "ManGreenSolid1Texture", "ManGreySolid2Texture", "ManWhitePantsClothes1Texture", "ManDarkBlueBeigeClothesBrownHairTexture", "ManTurquoiseDarkClothesBlackHairTexture" },
					PrimaryColor = new Vector3(52f / 85f, 0.4196078f, 0.2627451f),
					Edge = 1f,
					SecondaryColorProbabilityEdges = new List<ColorProbability>
					{
						new ColorProbability(1f, new Vector3(0.2509804f, 16f / 85f, 16f / 85f))
					}
				},
				new RaceType
				{
					KeyName = "blackHumanAncestor",
					Name = "Black Human Ancestor",
					PortraitSkinType = "blackPortrait",
					ModelBasicTextureNames = new string[2] { "ManBlue2Texture", "ManGreenGreyClothes1Texture" },
					PrimaryColor = new Vector3(52f / 85f, 0.4196078f, 0.2627451f),
					Edge = 1f,
					SecondaryColorProbabilityEdges = new List<ColorProbability>
					{
						new ColorProbability(1f, new Vector3(0.2509804f, 16f / 85f, 16f / 85f))
					}
				},
				new RaceType
				{
					KeyName = "sandyClothesBlackHair",
					Name = "Asian",
					PortraitSkinType = "East Asian",
					ModelBasicTextureName = "ManSandyClothesBlackHairTexture",
					PrimaryColor = new Vector3(52f / 85f, 0.4196078f, 0.2627451f),
					Edge = 1f,
					SecondaryColorProbabilityEdges = new List<ColorProbability>
					{
						new ColorProbability(1f, new Vector3(0.2509804f, 16f / 85f, 16f / 85f))
					}
				},
				new RaceType
				{
					KeyName = "burgundyClothesWhiteHair",
					Name = "White",
					PortraitSkinType = "White",
					ModelBasicTextureName = "ManBurgundyClothesWhiteHairTexture",
					PrimaryColor = new Vector3(52f / 85f, 0.4196078f, 0.2627451f),
					Edge = 1f,
					SecondaryColorProbabilityEdges = new List<ColorProbability>
					{
						new ColorProbability(1f, new Vector3(0.2509804f, 16f / 85f, 16f / 85f))
					}
				},
				new RaceType
				{
					KeyName = "darkBlueBeigeClothesBrownHair",
					Name = "Hispanic",
					PortraitSkinType = "White",
					ModelBasicTextureName = "ManDarkBlueBeigeClothesBrownHairTexture",
					PrimaryColor = new Vector3(52f / 85f, 0.4196078f, 0.2627451f),
					Edge = 1f,
					SecondaryColorProbabilityEdges = new List<ColorProbability>
					{
						new ColorProbability(1f, new Vector3(0.2509804f, 16f / 85f, 16f / 85f))
					}
				},
				new RaceType
				{
					KeyName = "orangeDarkGreyClothesYellowHair",
					Name = "White",
					PortraitSkinType = "White",
					ModelBasicTextureName = "ManOrangeDarkGreyClothesYellowHairTexture",
					PrimaryColor = new Vector3(52f / 85f, 0.4196078f, 0.2627451f),
					Edge = 1f,
					SecondaryColorProbabilityEdges = new List<ColorProbability>
					{
						new ColorProbability(1f, new Vector3(0.2509804f, 16f / 85f, 16f / 85f))
					}
				},
				new RaceType
				{
					KeyName = "darkBrownClothesRedHair",
					Name = "White",
					PortraitSkinType = "White",
					ModelBasicTextureName = "ManDarkBrownClothesRedHairTexture",
					PrimaryColor = new Vector3(52f / 85f, 0.4196078f, 0.2627451f),
					Edge = 1f,
					SecondaryColorProbabilityEdges = new List<ColorProbability>
					{
						new ColorProbability(1f, new Vector3(0.2509804f, 16f / 85f, 16f / 85f))
					}
				},
				new RaceType
				{
					KeyName = "brownBeigeClothesDarkSkin",
					Name = "Black",
					PortraitSkinType = "Black",
					ModelBasicTextureName = "ManBrownBeigeClothesDarkSkinTexture",
					PrimaryColor = new Vector3(52f / 85f, 0.4196078f, 0.2627451f),
					Edge = 1f,
					SecondaryColorProbabilityEdges = new List<ColorProbability>
					{
						new ColorProbability(1f, new Vector3(0.2509804f, 16f / 85f, 16f / 85f))
					}
				},
				new RaceType
				{
					KeyName = "sandyClothesDarkSkin",
					Name = "Black",
					PortraitSkinType = "Black",
					ModelBasicTextureName = "ManSandyClothesDarkSkinTexture",
					PrimaryColor = new Vector3(52f / 85f, 0.4196078f, 0.2627451f),
					Edge = 1f,
					SecondaryColorProbabilityEdges = new List<ColorProbability>
					{
						new ColorProbability(1f, new Vector3(0.2509804f, 16f / 85f, 16f / 85f))
					}
				},
				new RaceType
				{
					KeyName = "blueGreyClothesYellowHair",
					Name = "White",
					PortraitSkinType = "White",
					ModelBasicTextureName = "ManBlueGreyClothesYellowHairTexture",
					PrimaryColor = new Vector3(52f / 85f, 0.4196078f, 0.2627451f),
					Edge = 1f,
					SecondaryColorProbabilityEdges = new List<ColorProbability>
					{
						new ColorProbability(1f, new Vector3(0.2509804f, 16f / 85f, 16f / 85f))
					}
				},
				new RaceType
				{
					KeyName = "orangeGreyClothesRedHair",
					Name = "White",
					PortraitSkinType = "White",
					ModelBasicTextureName = "ManOrangeGreyClothesRedHairTexture",
					PrimaryColor = new Vector3(52f / 85f, 0.4196078f, 0.2627451f),
					Edge = 1f,
					SecondaryColorProbabilityEdges = new List<ColorProbability>
					{
						new ColorProbability(1f, new Vector3(0.2509804f, 16f / 85f, 16f / 85f))
					}
				},
				new RaceType
				{
					KeyName = "curryGreyClothesBrownSkin",
					Name = "Hispanic",
					PortraitSkinType = "Dark",
					ModelBasicTextureName = "ManCurryGreyClothesBrownSkinTexture",
					PrimaryColor = new Vector3(52f / 85f, 0.4196078f, 0.2627451f),
					Edge = 1f,
					SecondaryColorProbabilityEdges = new List<ColorProbability>
					{
						new ColorProbability(1f, new Vector3(0.2509804f, 16f / 85f, 16f / 85f))
					}
				},
				new RaceType
				{
					KeyName = "ochreClothesBlackHair",
					Name = "Asian",
					PortraitSkinType = "Dark",
					ModelBasicTextureName = "ManOchreClothesBlackHairTexture",
					PrimaryColor = new Vector3(52f / 85f, 0.4196078f, 0.2627451f),
					Edge = 1f,
					SecondaryColorProbabilityEdges = new List<ColorProbability>
					{
						new ColorProbability(1f, new Vector3(0.2509804f, 16f / 85f, 16f / 85f))
					}
				},
				new RaceType
				{
					KeyName = "brownGreyClothesBlondHair",
					Name = "White",
					PortraitSkinType = "White",
					ModelBasicTextureName = "ManBrownGreyClothesBlondHairTexture",
					PrimaryColor = new Vector3(52f / 85f, 0.4196078f, 0.2627451f),
					Edge = 1f,
					SecondaryColorProbabilityEdges = new List<ColorProbability>
					{
						new ColorProbability(1f, new Vector3(0.2509804f, 16f / 85f, 16f / 85f))
					}
				},
				new RaceType
				{
					KeyName = "turquoiseDarkClothesBlackHair",
					Name = "Asian",
					PortraitSkinType = "East Asian",
					ModelBasicTextureName = "ManTurquoiseDarkClothesBlackHairTexture",
					PrimaryColor = new Vector3(52f / 85f, 0.4196078f, 0.2627451f),
					Edge = 1f,
					SecondaryColorProbabilityEdges = new List<ColorProbability>
					{
						new ColorProbability(1f, new Vector3(0.2509804f, 16f / 85f, 16f / 85f))
					}
				},
				new RaceType
				{
					KeyName = "sandyDarkClothesWhiteHair",
					Name = "White",
					PortraitSkinType = "White",
					ModelBasicTextureName = "ManSandyDarkClothesWhiteHairTexture",
					PrimaryColor = new Vector3(52f / 85f, 0.4196078f, 0.2627451f),
					Edge = 1f,
					SecondaryColorProbabilityEdges = new List<ColorProbability>
					{
						new ColorProbability(1f, new Vector3(0.2509804f, 16f / 85f, 16f / 85f))
					}
				},
				new RaceType
				{
					KeyName = "colorReplaceClothes",
					Name = "ManColorReplaceClothes",
					PortraitSkinType = "White",
					ModelBasicTextureName = "ManColorReplaceTexture",
					PrimaryColor = new Vector3(52f / 85f, 0.4196078f, 0.2627451f),
					Edge = 1f,
					SecondaryColorProbabilityEdges = new List<ColorProbability>
					{
						new ColorProbability(1f, new Vector3(0.2509804f, 16f / 85f, 16f / 85f))
					}
				}
			},
			Castes = new List<CasteType>
			{
				new CasteType
				{
					KeyName = "male",
					Reproduction = Reproduction.Male,
					Edge = 0.51f,
					HeightMean = 1.8f,
					HeightStandardDeviation = 0.08f,
					WeightMean = 80f,
					WeightStandardDeviation = 0.15f,
					ModelName = "man",
					ModelScale = 2f,
					AgeGroupTypes = new List<AgeGroupType>
					{
						new AgeGroupType
						{
							Name = "Baby",
							AIAgeGroup = AIAgeGroup.Baby,
							CanReproduce = false,
							Edge = 1.5f,
							HeightTargetModifier = 0.05f,
							WeightTargetModifier = 0.03f,
							NeedTypes = new NeedType[4] { babySleepNeed, humanFoodNeed, humanProteinNeed, humanMicronutrientsNeed }
						},
						new AgeGroupType
						{
							Name = "Child",
							AIAgeGroup = AIAgeGroup.Child,
							CanReproduce = false,
							Edge = 11f,
							HeightTargetModifier = 0.6f,
							WeightTargetModifier = 0.5f,
							NeedTypes = new NeedType[4] { childSleepNeed, humanFoodNeed, humanProteinNeed, humanMicronutrientsNeed }
						},
						new AgeGroupType
						{
							Name = "Young adult",
							AIAgeGroup = AIAgeGroup.YoungAdult,
							CanReproduce = false,
							Edge = 16f,
							HeightTargetModifier = 0.97f,
							WeightTargetModifier = 0.92f,
							NeedTypes = new NeedType[4] { youngAdultSleepNeed, humanFoodNeed, humanProteinNeed, humanMicronutrientsNeed }
						},
						new AgeGroupType
						{
							Name = "Adult",
							AIAgeGroup = AIAgeGroup.Adult,
							CanReproduce = true,
							Edge = 72f,
							HeightTargetModifier = 1f,
							WeightTargetModifier = 1f,
							NeedTypes = new NeedType[5] { adultSleepNeed, humanFoodNeed, humanProteinNeed, humanMicronutrientsNeed, humanStimulantsNeed }
						},
						new AgeGroupType
						{
							Name = "Old",
							AIAgeGroup = AIAgeGroup.Old,
							CanReproduce = true,
							Edge = 200f,
							HeightTargetModifier = 0.9f,
							WeightTargetModifier = 1f,
							NeedTypes = new NeedType[5] { oldSleepNeed, humanFoodNeed, humanProteinNeed, humanMicronutrientsNeed, humanStimulantsNeed }
						}
					}
				},
				new CasteType
				{
					KeyName = "female",
					Reproduction = Reproduction.Female,
					Edge = 1f,
					HeightMean = 1.68f,
					HeightStandardDeviation = 0.05f,
					WeightMean = 68f,
					WeightStandardDeviation = 0.1f,
					ModelName = "woman",
					ModelScale = 2f,
					AgeGroupTypes = new List<AgeGroupType>
					{
						new AgeGroupType
						{
							Name = "Baby",
							AIAgeGroup = AIAgeGroup.Baby,
							CanReproduce = false,
							Edge = 1.5f,
							HeightTargetModifier = 0.05f,
							WeightTargetModifier = 0.03f,
							NeedTypes = new NeedType[4] { babySleepNeed, humanFoodNeed, humanProteinNeed, humanMicronutrientsNeed }
						},
						new AgeGroupType
						{
							Name = "Child",
							AIAgeGroup = AIAgeGroup.Child,
							CanReproduce = false,
							Edge = 11f,
							HeightTargetModifier = 0.6f,
							WeightTargetModifier = 0.5f,
							NeedTypes = new NeedType[4] { childSleepNeed, humanFoodNeed, humanProteinNeed, humanMicronutrientsNeed }
						},
						new AgeGroupType
						{
							Name = "Young adult",
							AIAgeGroup = AIAgeGroup.YoungAdult,
							CanReproduce = false,
							Edge = 16f,
							HeightTargetModifier = 0.97f,
							WeightTargetModifier = 0.92f,
							NeedTypes = new NeedType[4] { youngAdultSleepNeed, humanFoodNeed, humanProteinNeed, humanMicronutrientsNeed }
						},
						new AgeGroupType
						{
							Name = "Adult",
							AIAgeGroup = AIAgeGroup.Adult,
							CanReproduce = true,
							Edge = 72f,
							HeightTargetModifier = 1f,
							WeightTargetModifier = 1f,
							NeedTypes = new NeedType[5] { adultSleepNeed, humanFoodNeed, humanProteinNeed, humanMicronutrientsNeed, humanStimulantsNeed }
						},
						new AgeGroupType
						{
							Name = "Old",
							AIAgeGroup = AIAgeGroup.Old,
							CanReproduce = false,
							Edge = 200f,
							HeightTargetModifier = 0.9f,
							WeightTargetModifier = 1f,
							NeedTypes = new NeedType[5] { oldSleepNeed, humanFoodNeed, humanProteinNeed, humanMicronutrientsNeed, humanStimulantsNeed }
						}
					}
				}
			}
		};
		entityType18.Person = new PersonType
		{
			ShirtColors = new List<Vector3>
			{
				new Vector3(0.4980392f, 0.4352941f, 0.3411765f),
				new Vector3(0.6196079f, 0.4705882f, 0.2509804f),
				new Vector3(0.3568628f, 0.6196079f, 0.3843137f),
				new Vector3(0.509804f, 0.6784314f, 0.4941176f),
				new Vector3(0.2862745f, 0.6196079f, 1f),
				new Vector3(0.3176471f, 47f / 85f, 0.6784314f),
				new Vector3(0.7372549f, 0.2588235f, 0.2509804f),
				new Vector3(66f / 85f, 66f / 85f, 66f / 85f),
				new Vector3(0.4784314f, 0.4784314f, 0.4784314f),
				new Vector3(0.3372549f, 0.3372549f, 0.3372549f),
				new Vector3(33f / 85f, 33f / 85f, 33f / 85f)
			},
			PantsColors = new List<Vector3>
			{
				new Vector3(0.4392157f, 31f / 85f, 0.2705882f),
				new Vector3(32f / 51f, 0.5372549f, 0.427451f),
				new Vector3(0.3803922f, 0.509804f, 0.3843137f),
				new Vector3(0.2588235f, 0.4352941f, 0.6470588f),
				new Vector3(0.4901961f, 0.7215686f, 0.8784314f),
				new Vector3(0.4980392f, 0.4980392f, 0.4980392f),
				new Vector3(32f / 51f, 32f / 51f, 32f / 51f)
			}
		};
		listOfEntityTypes.Add(entityType18);
	}

	public static void CreateHumanNeeds(out NeedType babySleepNeed, out NeedType childSleepNeed, out NeedType youngAdultSleepNeed, out NeedType adultSleepNeed, out NeedType oldSleepNeed, out NeedType humanFoodNeed, out NeedType humanProteinNeed, out NeedType humanMicronutrientsNeed, out NeedType humanStimulantsNeed)
	{
		babySleepNeed = new NeedType
		{
			KeyName = "sleep",
			SleepNeedType = new SleepNeedType(),
			DecreasePerDay = new NormalDistribution
			{
				Mean = 0.699999988079071,
				StandardDeviation = 0.029999999329447746
			},
			LimitForDecreasedEnergy = 0.2f,
			DecreasedEnergyWeight = 0.4f
		};
		childSleepNeed = new NeedType
		{
			KeyName = "sleep",
			SleepNeedType = new SleepNeedType(),
			DecreasePerDay = new NormalDistribution
			{
				Mean = 0.4000000059604645,
				StandardDeviation = 0.019999999552965164
			},
			LimitForDecreasedEnergy = 0.2f,
			DecreasedEnergyWeight = 0.4f
		};
		youngAdultSleepNeed = new NeedType
		{
			KeyName = "sleep",
			SleepNeedType = new SleepNeedType(),
			DecreasePerDay = new NormalDistribution
			{
				Mean = 0.3799999952316284,
				StandardDeviation = 0.014999999664723873
			},
			LimitForDecreasedEnergy = 0.2f,
			DecreasedEnergyWeight = 0.4f
		};
		adultSleepNeed = new NeedType
		{
			KeyName = "sleep",
			SleepNeedType = new SleepNeedType(),
			DecreasePerDay = new NormalDistribution
			{
				Mean = 1.0,
				StandardDeviation = 0.014999999664723873
			},
			LimitForDecreasedEnergy = 0.4f,
			DecreasedEnergyWeight = 0.4f,
			PhysicalEffects = new PhysicalEffects
			{
				DaysAtZeroCausingDeath = 2f,
				DaysAtZeroDecreaseFactor = 2f
			}
		};
		oldSleepNeed = new NeedType
		{
			KeyName = "sleep",
			SleepNeedType = new SleepNeedType(),
			DecreasePerDay = new NormalDistribution
			{
				Mean = 0.4000000059604645,
				StandardDeviation = 0.019999999552965164
			},
			LimitForDecreasedEnergy = 0.2f,
			DecreasedEnergyWeight = 0.4f
		};
		float num = 0.125f;
		humanFoodNeed = new NeedType
		{
			KeyName = "foodEnergy",
			FoodNeedType = new FoodNeedType
			{
				FoodNutrient = "foodEnergy",
				RequiredNutrientsAsFractionOfEntityBulk = 0.6f * num / 1f
			},
			DecreasePerDay = new NormalDistribution
			{
				Mean = 1.0,
				StandardDeviation = 0.019999999552965164
			},
			LimitForDecreasedEnergy = 0.15f,
			DecreasedEnergyWeight = 0.6f,
			PhysicalEffects = new PhysicalEffects
			{
				DaysAtZeroCausingCollapse = 2f,
				DaysAtZeroCausingDeath = 2.05f,
				DaysAtZeroDecreaseFactor = 1f,
				UseExertionFactorToDecrease = true
			}
		};
		humanProteinNeed = new NeedType
		{
			KeyName = "protein",
			FoodNeedType = new FoodNeedType
			{
				FoodNutrient = "protein",
				RequiredNutrientsAsFractionOfEntityBulk = 0.006f
			},
			DecreasePerDay = new NormalDistribution
			{
				Mean = 1.0,
				StandardDeviation = 0.019999999552965164
			},
			LimitForDecreasedEnergy = 0.05f,
			DecreasedEnergyWeight = 0.08f,
			PhysicalEffects = new PhysicalEffects
			{
				LimitForReducedGrowth = 0.1f,
				LimitForIncreasedSickness = 0.05f,
				UseExertionFactorToDecrease = false
			}
		};
		humanMicronutrientsNeed = new NeedType
		{
			KeyName = "micronutrients",
			FoodNeedType = new FoodNeedType
			{
				FoodNutrient = "micronutrients",
				RequiredNutrientsAsFractionOfEntityBulk = 0.0003f
			},
			DecreasePerDay = new NormalDistribution
			{
				Mean = 1.0,
				StandardDeviation = 0.019999999552965164
			},
			LimitForDecreasedEnergy = 0.05f,
			DecreasedEnergyWeight = 0.08f,
			PhysicalEffects = new PhysicalEffects
			{
				LimitForReducedGrowth = 0.1f,
				LimitForIncreasedSickness = 0.05f,
				UseExertionFactorToDecrease = false
			}
		};
		humanStimulantsNeed = new NeedType
		{
			KeyName = "stimulants",
			FoodNeedType = new FoodNeedType
			{
				FoodNutrient = "stimulants",
				RequiredNutrientsAsFractionOfEntityBulk = 0.0003f,
				IsEssential = false
			},
			DecreasePerDay = new NormalDistribution
			{
				Mean = 1.0,
				StandardDeviation = 0.019999999552965164
			},
			LimitForDecreasedEnergy = 0f,
			DecreasedEnergyWeight = 0f
		};
	}
}
