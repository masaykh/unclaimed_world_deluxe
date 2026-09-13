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

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_3.Data;

internal class CreatureLoader
{
	public const int sensorRangeHuman = 350;

	public const int sensorRangeHumanNight = 200;

	public const float humanMaxRegainLimit = 0.5f;

	public const float humanFractionOfMaxHitpointsGainedPerDay = 0.3f;

	public static void Init(List<EntityType> listOfEntityTypes)
	{
		CreateHumanNeeds(out var humanFoodNeed, out var humanProteinNeed, out var humanMicronutrientsNeed, out var humanStimulantsNeed);
		BodyType bodyType = GameData.Instance.AllBodyTypes["humanoid"];
		EntityType entityType = new EntityType("entity:human");
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
				DefaultStances = new AnimConditionInfo[3]
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
					}
				},
				AnimConditions = new AnimConditionInfo[103]
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
							BaseAnimations = new string[1] { "idleTalkShort" }
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
						},
						Playback = Playback.Manual
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
						Looping = Looping.Yes,
						Forbiddens = new BitMask64(typeof(AnimModifier), 36, 39, 32, 31, 33)
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
							Sounds = new string[5] { "activities/building/buildingSteel", "activities/building/buildingSteel", "", "", "" }
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
							BaseAnimations = new string[4] { "constructPull", "chopLow", "idleSweat", "idleStretchesNeck" },
							Sounds = new string[4] { "", "activities/building/buildingHammer", "", "" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Building,
							Modifiers = new BitMask64(typeof(AnimModifier), 35)
						},
						Looping = Looping.Yes
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "mend" },
							Sounds = new string[1] { "activities/building/buildingHammer" }
						},
						ConditionSet = new AnimConditions
						{
							Action = AnimAction.Building,
							Modifiers = new BitMask64(typeof(AnimModifier), 35, 15)
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
							BaseAnimations = new string[4] { "useHoe", "idleWipesNose", "gather", "useHoe" },
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
							Modifiers = new BitMask64(typeof(AnimModifier), 36, 21)
						},
						AttachPoints = new AnimConditionInfo.AttachPointData[1]
						{
							new AnimConditionInfo.AttachPointData
							{
								AttacheePoint = AttacheePoint.RightHand,
								RenderableTypeKey = "machete",
								Translation = new Vector3(0.236f, -0.236f, -0.026f),
								Rotation = new Vector3(139.37f, -177.165f, -102.52f)
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
							Action = AnimAction.Harvesting,
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
							BaseAnimations = new string[1] { "attackClubHigh" },
							Sounds = new string[1] { "melee/STAB2_24 - 4 Stabs With Blood" }
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
						Playback = Playback.Manual,
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
							BaseAnimations = new string[1] { "combatBowAim" }
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
							BaseAnimations = new string[1] { "combatBowReload" }
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
								Translation = new Vector3(-1.076f, -0.079f, -0.814f),
								Rotation = new Vector3(92.126f, 60.945f, -94.96f)
							}
						}
					},
					new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "attackClubHigh" },
							Sounds = new string[1] { "melee/STAB2_24 - 4 Stabs With Blood" }
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
		EntityType entityType2 = entityType;
		entityType2.IntelligenceType = new IntelligenceType
		{
			IsMobile = true,
			RespectsOwnership = true,
			AllowEscapeFromTinyAreas = true,
			CanAttack = true,
			CanSpeak = true,
			CanTradeAndCommunicate = true,
			CanProduce = true,
			CanDoJobs = true,
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
			Prey = new string[4] { "entity:mudWorm", "entity:binalRat", "entity:whiteThunderChicken", "entity:bushDragon" },
			Attacks = new string[4] { "personPunchHighRight", "personPunchLowRight", "personSnapkickRight", "personStompRight" },
			Skills = new SerializableDictionary<string, float>
			{
				{ "fruitPicking", 1f },
				{ "grasping", 1f }
			},
			InterestInTriggerTypes = new string[2] { "entityDied", "creature" },
			AggroRange = 160f,
			ChanceToRestAfterMeleeAttack = 0.6,
			ChanceToRestAfterRangedAttack = 0.6,
			MinRestTimeAfterAttackingInSeconds = 0.5f,
			MaxRestTimeAfterAttackingInSeconds = 1.2f,
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
		entityType2.BiologicalType = new BiologicalType
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
			StomachSizeFractionOfEntityBulk = 0.45f,
			StomachContentsDecreaseRatePerDay = 2f,
			ActiveStealthRating = 0f,
			MaxRegainLimit = 0.5f,
			FractionOfMaxHitpointsGainedPerDay = 0.3f,
			TimeOfDayToGoToSleep = 0.35,
			Carcass = "item:body",
			RaceTypes = new RaceType[25]
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
					KeyName = "blue1",
					Name = "ManBlue1",
					PortraitSkinType = "White",
					ModelBasicTextureName = "ManBlue1Texture",
					PrimaryColor = new Vector3(0.9960784f, 82f / 85f, 0.8823529f),
					Edge = 1f,
					SecondaryColorProbabilityEdges = new List<ColorProbability>
					{
						new ColorProbability(1f, new Vector3(0.2509804f, 16f / 85f, 16f / 85f))
					}
				},
				new RaceType
				{
					KeyName = "blue2",
					Name = "ManBlue2",
					PortraitSkinType = "Dark",
					ModelBasicTextureName = "ManBlue2Texture",
					PrimaryColor = new Vector3(52f / 85f, 0.4196078f, 0.2627451f),
					Edge = 1f,
					SecondaryColorProbabilityEdges = new List<ColorProbability>
					{
						new ColorProbability(1f, new Vector3(0.2509804f, 16f / 85f, 16f / 85f))
					}
				},
				new RaceType
				{
					KeyName = "red1",
					Name = "ManRed1",
					PortraitSkinType = "East Asian",
					ModelBasicTextureName = "ManRed1Texture",
					PrimaryColor = new Vector3(52f / 85f, 0.4196078f, 0.2627451f),
					Edge = 1f,
					SecondaryColorProbabilityEdges = new List<ColorProbability>
					{
						new ColorProbability(1f, new Vector3(0.2509804f, 16f / 85f, 16f / 85f))
					}
				},
				new RaceType
				{
					KeyName = "red2",
					Name = "ManRed2",
					PortraitSkinType = "White",
					ModelBasicTextureName = "ManRed2Texture",
					PrimaryColor = new Vector3(52f / 85f, 0.4196078f, 0.2627451f),
					Edge = 1f,
					SecondaryColorProbabilityEdges = new List<ColorProbability>
					{
						new ColorProbability(1f, new Vector3(0.2509804f, 16f / 85f, 16f / 85f))
					}
				},
				new RaceType
				{
					KeyName = "green1",
					Name = "ManGreen1",
					PortraitSkinType = "White",
					ModelBasicTextureName = "ManGreen1Texture",
					PrimaryColor = new Vector3(52f / 85f, 0.4196078f, 0.2627451f),
					Edge = 1f,
					SecondaryColorProbabilityEdges = new List<ColorProbability>
					{
						new ColorProbability(1f, new Vector3(0.2509804f, 16f / 85f, 16f / 85f))
					}
				},
				new RaceType
				{
					KeyName = "green2",
					Name = "ManGreen2",
					PortraitSkinType = "White",
					ModelBasicTextureName = "ManGreen2Texture",
					PrimaryColor = new Vector3(52f / 85f, 0.4196078f, 0.2627451f),
					Edge = 1f,
					SecondaryColorProbabilityEdges = new List<ColorProbability>
					{
						new ColorProbability(1f, new Vector3(0.2509804f, 16f / 85f, 16f / 85f))
					}
				},
				new RaceType
				{
					KeyName = "grey1",
					Name = "ManGrey1",
					PortraitSkinType = "White",
					ModelBasicTextureName = "ManGrey1Texture",
					PrimaryColor = new Vector3(52f / 85f, 0.4196078f, 0.2627451f),
					Edge = 1f,
					SecondaryColorProbabilityEdges = new List<ColorProbability>
					{
						new ColorProbability(1f, new Vector3(0.2509804f, 16f / 85f, 16f / 85f))
					}
				},
				new RaceType
				{
					KeyName = "grey2",
					Name = "ManGrey2",
					PortraitSkinType = "White",
					ModelBasicTextureName = "ManGrey2Texture",
					PrimaryColor = new Vector3(52f / 85f, 0.4196078f, 0.2627451f),
					Edge = 1f,
					SecondaryColorProbabilityEdges = new List<ColorProbability>
					{
						new ColorProbability(1f, new Vector3(0.2509804f, 16f / 85f, 16f / 85f))
					}
				},
				new RaceType
				{
					KeyName = "grey3",
					Name = "ManGrey3",
					PortraitSkinType = "White",
					ModelBasicTextureName = "ManGrey3Texture",
					PrimaryColor = new Vector3(52f / 85f, 0.4196078f, 0.2627451f),
					Edge = 1f,
					SecondaryColorProbabilityEdges = new List<ColorProbability>
					{
						new ColorProbability(1f, new Vector3(0.2509804f, 16f / 85f, 16f / 85f))
					}
				},
				new RaceType
				{
					KeyName = "greySolid1",
					Name = "ManGreySolid",
					PortraitSkinType = "White",
					ModelBasicTextureName = "ManGreySolid1Texture",
					PrimaryColor = new Vector3(52f / 85f, 0.4196078f, 0.2627451f),
					Edge = 1f,
					SecondaryColorProbabilityEdges = new List<ColorProbability>
					{
						new ColorProbability(1f, new Vector3(0.2509804f, 16f / 85f, 16f / 85f))
					}
				},
				new RaceType
				{
					KeyName = "greenSolid1",
					Name = "ManGreenSolid",
					PortraitSkinType = "White",
					ModelBasicTextureName = "ManGreenSolid1Texture",
					PrimaryColor = new Vector3(52f / 85f, 0.4196078f, 0.2627451f),
					Edge = 1f,
					SecondaryColorProbabilityEdges = new List<ColorProbability>
					{
						new ColorProbability(1f, new Vector3(0.2509804f, 16f / 85f, 16f / 85f))
					}
				},
				new RaceType
				{
					KeyName = "blueSolid1",
					Name = "ManBlueSolid",
					PortraitSkinType = "White",
					ModelBasicTextureName = "ManBlueSolid1Texture",
					PrimaryColor = new Vector3(52f / 85f, 0.4196078f, 0.2627451f),
					Edge = 1f,
					SecondaryColorProbabilityEdges = new List<ColorProbability>
					{
						new ColorProbability(1f, new Vector3(0.2509804f, 16f / 85f, 16f / 85f))
					}
				},
				new RaceType
				{
					KeyName = "blueBrownClothes1",
					Name = "ManBlueBrownClothes1",
					PortraitSkinType = "White",
					ModelBasicTextureName = "ManBlueBrownClothes1Texture",
					PrimaryColor = new Vector3(52f / 85f, 0.4196078f, 0.2627451f),
					Edge = 1f,
					SecondaryColorProbabilityEdges = new List<ColorProbability>
					{
						new ColorProbability(1f, new Vector3(0.2509804f, 16f / 85f, 16f / 85f))
					}
				},
				new RaceType
				{
					KeyName = "greenGreyClothes1",
					Name = "ManGreenGreyClothes1",
					PortraitSkinType = "Black",
					ModelBasicTextureName = "ManGreenGreyClothes1Texture",
					PrimaryColor = new Vector3(52f / 85f, 0.4196078f, 0.2627451f),
					Edge = 1f,
					SecondaryColorProbabilityEdges = new List<ColorProbability>
					{
						new ColorProbability(1f, new Vector3(0.2509804f, 16f / 85f, 16f / 85f))
					}
				},
				new RaceType
				{
					KeyName = "greyClothes1",
					Name = "ManGreyClothes1",
					PortraitSkinType = "White",
					ModelBasicTextureName = "ManGreyClothes1Texture",
					PrimaryColor = new Vector3(52f / 85f, 0.4196078f, 0.2627451f),
					Edge = 1f,
					SecondaryColorProbabilityEdges = new List<ColorProbability>
					{
						new ColorProbability(1f, new Vector3(0.2509804f, 16f / 85f, 16f / 85f))
					}
				},
				new RaceType
				{
					KeyName = "whitePantsClothes1",
					Name = "ManWhitePantsClothes1",
					PortraitSkinType = "East Asian",
					ModelBasicTextureName = "ManWhitePantsClothes1Texture",
					PrimaryColor = new Vector3(52f / 85f, 0.4196078f, 0.2627451f),
					Edge = 1f,
					SecondaryColorProbabilityEdges = new List<ColorProbability>
					{
						new ColorProbability(1f, new Vector3(0.2509804f, 16f / 85f, 16f / 85f))
					}
				},
				new RaceType
				{
					KeyName = "greenBlueClothes1",
					Name = "ManGreenBlueClothes1",
					PortraitSkinType = "White",
					ModelBasicTextureName = "ManGreenBlueClothes1Texture",
					PrimaryColor = new Vector3(52f / 85f, 0.4196078f, 0.2627451f),
					Edge = 1f,
					SecondaryColorProbabilityEdges = new List<ColorProbability>
					{
						new ColorProbability(1f, new Vector3(0.2509804f, 16f / 85f, 16f / 85f))
					}
				},
				new RaceType
				{
					KeyName = "whiteBlueClothes1",
					Name = "ManWhiteBlueClothes1",
					PortraitSkinType = "White",
					ModelBasicTextureName = "ManWhiteBlueClothes1Texture",
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
							NeedTypes = new NeedType[3] { humanFoodNeed, humanProteinNeed, humanMicronutrientsNeed }
						},
						new AgeGroupType
						{
							Name = "Child",
							AIAgeGroup = AIAgeGroup.Child,
							CanReproduce = false,
							Edge = 11f,
							HeightTargetModifier = 0.6f,
							WeightTargetModifier = 0.5f,
							NeedTypes = new NeedType[3] { humanFoodNeed, humanProteinNeed, humanMicronutrientsNeed }
						},
						new AgeGroupType
						{
							Name = "Young adult",
							AIAgeGroup = AIAgeGroup.YoungAdult,
							CanReproduce = false,
							Edge = 16f,
							HeightTargetModifier = 0.97f,
							WeightTargetModifier = 0.92f,
							NeedTypes = new NeedType[3] { humanFoodNeed, humanProteinNeed, humanMicronutrientsNeed }
						},
						new AgeGroupType
						{
							Name = "Adult",
							AIAgeGroup = AIAgeGroup.Adult,
							CanReproduce = true,
							Edge = 72f,
							HeightTargetModifier = 1f,
							WeightTargetModifier = 1f,
							NeedTypes = new NeedType[4] { humanFoodNeed, humanProteinNeed, humanMicronutrientsNeed, humanStimulantsNeed }
						},
						new AgeGroupType
						{
							Name = "Old",
							AIAgeGroup = AIAgeGroup.Old,
							CanReproduce = true,
							Edge = 200f,
							HeightTargetModifier = 0.9f,
							WeightTargetModifier = 1f,
							NeedTypes = new NeedType[4] { humanFoodNeed, humanProteinNeed, humanMicronutrientsNeed, humanStimulantsNeed }
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
							NeedTypes = new NeedType[3] { humanFoodNeed, humanProteinNeed, humanMicronutrientsNeed }
						},
						new AgeGroupType
						{
							Name = "Child",
							AIAgeGroup = AIAgeGroup.Child,
							CanReproduce = false,
							Edge = 11f,
							HeightTargetModifier = 0.6f,
							WeightTargetModifier = 0.5f,
							NeedTypes = new NeedType[3] { humanFoodNeed, humanProteinNeed, humanMicronutrientsNeed }
						},
						new AgeGroupType
						{
							Name = "Young adult",
							AIAgeGroup = AIAgeGroup.YoungAdult,
							CanReproduce = false,
							Edge = 16f,
							HeightTargetModifier = 0.97f,
							WeightTargetModifier = 0.92f,
							NeedTypes = new NeedType[3] { humanFoodNeed, humanProteinNeed, humanMicronutrientsNeed }
						},
						new AgeGroupType
						{
							Name = "Adult",
							AIAgeGroup = AIAgeGroup.Adult,
							CanReproduce = true,
							Edge = 72f,
							HeightTargetModifier = 1f,
							WeightTargetModifier = 1f,
							NeedTypes = new NeedType[4] { humanFoodNeed, humanProteinNeed, humanMicronutrientsNeed, humanStimulantsNeed }
						},
						new AgeGroupType
						{
							Name = "Old",
							AIAgeGroup = AIAgeGroup.Old,
							CanReproduce = false,
							Edge = 200f,
							HeightTargetModifier = 0.9f,
							WeightTargetModifier = 1f,
							NeedTypes = new NeedType[4] { humanFoodNeed, humanProteinNeed, humanMicronutrientsNeed, humanStimulantsNeed }
						}
					}
				}
			}
		};
		entityType2.Person = new PersonType
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
		listOfEntityTypes.Add(entityType2);
		NeedType needType = new NeedType
		{
			KeyName = "foodEnergy",
			FoodNeedType = new FoodNeedType
			{
				FoodNutrient = "foodEnergy",
				RequiredNutrientsAsFractionOfEntityBulk = 0.1f
			},
			DecreasePerDay = new NormalDistribution
			{
				Mean = 2.0,
				StandardDeviation = 0.019999999552965164
			},
			LimitForDecreasedEnergy = 0.1f,
			DecreasedEnergyWeight = 0.6f,
			PhysicalEffects = new PhysicalEffects
			{
				DaysAtZeroCausingCollapse = 4f,
				DaysAtZeroCausingDeath = 4f,
				DaysAtZeroDecreaseFactor = 1f,
				UseExertionFactorToDecrease = true
			}
		};
		entityType = new EntityType("entity:bushDragon");
		entityType.Name = "Northern bush dragon";
		entityType.ThumbnailSmall = "HUD_thumbnail_bushDragon";
		entityType.SummaryDescription = "Omnivorous herd animal";
		entityType.Description = "\n FEEDING CLASSIFICATION: Omnivore. Eats low vegetation, small animals, carrion.\n \n HEIGHT: Up to 1.5 m (wings excluded)\n \n ANATOMY\n Waddling, 3-legged animal which has developed wings, not for flight but for display purposes. Likely used to dissuade predators and possibly in mating behaviour. The creature has a defensive weapon in the form of a chemical spray.\n \n BEHAVIOR\n If approached, bush dragons will defend themselves much in the manner of the terran skunk. Against the quadites the spray seems to be particularly effective, causing incapacitation and even death.\n \n SURVIVAL GUIDE NOTES\n The slow-moving creatures protect their herd, but if distance is observed they do not attack. The toxicity to humans of their defensive chemical is yet to be determined, but caution is advised.";
		entityType.DetectionTag = "huge";
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				ModelScale = 0.8f,
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
		EntityType entityType3 = entityType;
		entityType3.IntelligenceType = new IntelligenceType
		{
			ForageAndHuntingRadius = 290,
			MembersScoutingFraction = 1f,
			IsMobile = true,
			CanAttack = true,
			CanUseWeapons = false,
			CanHunt = false,
			CanScout = true,
			CanExamine = true,
			CanPatrol = true,
			CanHaul = false,
			StrengthRating = StrengthRating.LikeHumans,
			Courage = 0.5f,
			MemoryInDays = 3f,
			Boldness = 0.25f,
			AggroRange = 200f,
			Skills = new SerializableDictionary<string, float> { { "unarmedFighting", 0.6f } },
			Attacks = new string[1] { "bushDragonSpray" }
		};
		entityType3.BiologicalType = new BiologicalType
		{
			OxygenAndMuscleEnergyIncreaseRatePerDay = 8f,
			TimeToConsumeFullMealInDays = 0.05f,
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
					Name = "Grey Bush Dragon",
					PortraitSkinType = "Dark",
					PrimaryColor = "F7F4C5".ToColorVector3(),
					ModelBasicTextureName = "BushdragonDarkTexture",
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
					WeightMean = 140f,
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
							NeedTypes = new NeedType[1] { needType }
						},
						new AgeGroupType
						{
							Name = "Child",
							AIAgeGroup = AIAgeGroup.Child,
							CanReproduce = false,
							Edge = 11f,
							HeightTargetModifier = 0.6f,
							WeightTargetModifier = 0.5f,
							NeedTypes = new NeedType[1] { needType }
						},
						new AgeGroupType
						{
							Name = "Young adult",
							AIAgeGroup = AIAgeGroup.YoungAdult,
							CanReproduce = false,
							Edge = 16f,
							HeightTargetModifier = 0.97f,
							WeightTargetModifier = 0.92f,
							NeedTypes = new NeedType[1] { needType }
						},
						new AgeGroupType
						{
							Name = "Adult",
							AIAgeGroup = AIAgeGroup.Adult,
							CanReproduce = true,
							Edge = 72f,
							HeightTargetModifier = 1f,
							WeightTargetModifier = 1f,
							NeedTypes = new NeedType[1] { needType }
						},
						new AgeGroupType
						{
							Name = "Old",
							AIAgeGroup = AIAgeGroup.Old,
							CanReproduce = true,
							Edge = 200f,
							HeightTargetModifier = 0.9f,
							WeightTargetModifier = 1f,
							NeedTypes = new NeedType[1] { needType }
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
					WeightMean = 140f,
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
							NeedTypes = new NeedType[1] { needType }
						},
						new AgeGroupType
						{
							Name = "Child",
							AIAgeGroup = AIAgeGroup.Child,
							CanReproduce = false,
							Edge = 11f,
							HeightTargetModifier = 0.6f,
							WeightTargetModifier = 0.5f,
							NeedTypes = new NeedType[1] { needType }
						},
						new AgeGroupType
						{
							Name = "Young adult",
							AIAgeGroup = AIAgeGroup.YoungAdult,
							CanReproduce = false,
							Edge = 16f,
							HeightTargetModifier = 0.97f,
							WeightTargetModifier = 0.92f,
							NeedTypes = new NeedType[1] { needType }
						},
						new AgeGroupType
						{
							Name = "Adult",
							AIAgeGroup = AIAgeGroup.Adult,
							CanReproduce = true,
							Edge = 72f,
							HeightTargetModifier = 1f,
							WeightTargetModifier = 1f,
							NeedTypes = new NeedType[1] { needType }
						},
						new AgeGroupType
						{
							Name = "Old",
							AIAgeGroup = AIAgeGroup.Old,
							CanReproduce = false,
							Edge = 200f,
							HeightTargetModifier = 0.9f,
							WeightTargetModifier = 1f,
							NeedTypes = new NeedType[1] { needType }
						}
					}
				}
			}
		};
		listOfEntityTypes.Add(entityType3);
	}

	public static void CreateHumanNeeds(out NeedType humanFoodNeed, out NeedType humanProteinNeed, out NeedType humanMicronutrientsNeed, out NeedType humanStimulantsNeed)
	{
		float num = 0.25f;
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
				Mean = 0.0,
				StandardDeviation = 0.0
			},
			LimitForDecreasedEnergy = 0.15f,
			DecreasedEnergyWeight = 0.6f,
			PhysicalEffects = new PhysicalEffects
			{
				DaysAtZeroCausingCollapse = 2f,
				DaysAtZeroCausingDeath = 2.05f,
				DaysAtZeroDecreaseFactor = 1f,
				UseExertionFactorToDecrease = false
			}
		};
		humanProteinNeed = new NeedType
		{
			KeyName = "protein",
			FoodNeedType = new FoodNeedType
			{
				FoodNutrient = "protein",
				RequiredNutrientsAsFractionOfEntityBulk = 0.012f
			},
			DecreasePerDay = new NormalDistribution
			{
				Mean = 0.0,
				StandardDeviation = 0.0
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
				RequiredNutrientsAsFractionOfEntityBulk = 0.0006f
			},
			DecreasePerDay = new NormalDistribution
			{
				Mean = 0.0,
				StandardDeviation = 0.0
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
				IsEssential = false,
				RequiredNutrientsAsFractionOfEntityBulk = 0.0006f
			},
			DecreasePerDay = new NormalDistribution
			{
				Mean = 0.0,
				StandardDeviation = 0.0
			},
			LimitForDecreasedEnergy = 0f,
			DecreasedEnergyWeight = 0f
		};
	}
}
