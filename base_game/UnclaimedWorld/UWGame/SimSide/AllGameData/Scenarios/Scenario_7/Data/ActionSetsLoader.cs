using System.Collections.Generic;
using UWGame.Client.Particles;
using UWGame.ClientSide.GameEvents;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.InGameEvents.PropertyObjects;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_7.Data;

public class ActionSetsLoader
{
	public static List<ActionSets> Init()
	{
		List<ActionSets> list = new List<ActionSets>();
		double delayInSeconds = 2400.0;
		list.Add(new ActionSets
		{
			FireMode = ActionSetsToFire.AllValid,
			KeyName = "bigBombActivated",
			ActionTargets = new TargetObject
			{
				TargetObjectType = TargetObjectType.TargetEntity
			},
			SetsOfActions = new ActionSetType[8]
			{
				new ActionSetType("d92asad626701d-ab98-447d-9b02-7ccc426426dgsdbfbe39f5")
				{
					Actions = new EventActionType[6]
					{
						new ParticleEffectAction("s91efsffe191-d051-4sxc3ce-3535859f-24axccx284ce1504")
						{
							DelayInSeconds = 5.0,
							UseLocationOfEntity = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							DurationInSeconds = 0.4,
							ParticleEmitters = new ParticleEmitterEffect[1]
							{
								new ParticleEmitterEffect
								{
									AttachToEntity = false,
									ParticleSystemKey = "explosionSmokeCloud"
								}
							}
						},
						new ParticleEffectAction("xcc91efe1ccx91-d05xcxc1-43caae-859f-24asd284ce15-04")
						{
							DelayInSeconds = 5.0,
							UseLocationOfEntity = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							DurationInSeconds = 12.0,
							ParticleEmitters = new ParticleEmitterEffect[1]
							{
								new ParticleEmitterEffect
								{
									AttachToEntity = false,
									ParticleSystemKey = "explosionSmokeCloudLong"
								}
							}
						},
						new ParticleEffectAction("91esddfe191-dcxc051-43crereyye-859dfff-24a284ce1504")
						{
							DelayInSeconds = 5.0,
							UseLocationOfEntity = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							DurationInSeconds = 0.4,
							ParticleEmitters = new ParticleEmitterEffect[1]
							{
								new ParticleEmitterEffect
								{
									AttachToEntity = false,
									ParticleSystemKey = "explosion"
								}
							}
						},
						new ParticleEffectAction("91efsdsdawwqe191-d05rqxv1-43sdce-859f-24a284ce1504")
						{
							DelayInSeconds = 5.0,
							UseLocationOfEntity = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							},
							DurationInSeconds = 0.4,
							ParticleEmitters = new ParticleEmitterEffect[1]
							{
								new ParticleEmitterEffect
								{
									AttachToEntity = false,
									ParticleSystemKey = "mineExplosion"
								}
							}
						},
						new SoundEffectAction("91efe191-d051-43ce-859f-24ada-t35252sw-284ce1504")
						{
							DelayInSeconds = 5.0,
							KeyName = "traps/mineExplosionHardwDebris",
							Sound = "traps/mineExplosionHardwDebris"
						},
						new DestroyEntityAction("edf63fa265263ea9-egdag624e2-462edc37-8312-7ce1944a9d9e")
						{
							DelayInSeconds = 5.5,
							TargetObject = new TargetObject
							{
								TargetObjectType = TargetObjectType.TargetEntity
							}
						}
					}
				},
				new ActionSetType("101cc4ee-a07e-4c97-94e2-9c5281791412")
				{
					Condition = new CustomCondition
					{
						TargetObject = new TargetObject
						{
							TargetObjectType = TargetObjectType.TargetEntity
						},
						PropertyCondition = new PropertyCondition
						{
							PropertyKey = "name",
							ConstantStringEqual = "Field Quadite Nest 1"
						}
					},
					Actions = new EventActionType[1]
					{
						new SpawnEntityAction("f295b796-1987-405a-80c5-552b1ab5f29d")
						{
							DelayInSeconds = delayInSeconds,
							EntityDataKey = new ValueNode
							{
								String = "fieldQuaditeNest1"
							}
						}
					}
				},
				new ActionSetType("d92a70sada25251d-afdsaf32523-47d-9b02-7cccbfbe39f5")
				{
					Condition = new CustomCondition
					{
						TargetObject = new TargetObject
						{
							TargetObjectType = TargetObjectType.TargetEntity
						},
						PropertyCondition = new PropertyCondition
						{
							PropertyKey = "name",
							ConstantStringEqual = "Swarmer nest 1"
						}
					},
					Actions = new EventActionType[1]
					{
						new SpawnEntityAction("edf6saf3252533ea9-e4af3262e2-4c3sg257-8312-7ce1944a9d9e")
						{
							DelayInSeconds = delayInSeconds,
							EntityDataKey = new ValueNode
							{
								String = "swarmerNest1"
							}
						}
					}
				},
				new ActionSetType("d92a70sada25251awfasdd-afdsaf3ddwa2523-47d-9b02-7cccbfbe39f5")
				{
					Condition = new CustomCondition
					{
						TargetObject = new TargetObject
						{
							TargetObjectType = TargetObjectType.TargetEntity
						},
						PropertyCondition = new PropertyCondition
						{
							PropertyKey = "name",
							ConstantStringEqual = "Swarmer nest 2"
						}
					},
					Actions = new EventActionType[1]
					{
						new SpawnEntityAction("edf6saf3252533sadaea9-e4af3262e2-4c3sg257-8312-7ce1944a9d9e")
						{
							DelayInSeconds = delayInSeconds,
							EntityDataKey = new ValueNode
							{
								String = "swarmerNest2"
							}
						}
					}
				},
				new ActionSetType("d92a70sada252afa3f3ff51d-afdsaf32523-47d-9b02-7cccbfbe39f5")
				{
					Condition = new CustomCondition
					{
						TargetObject = new TargetObject
						{
							TargetObjectType = TargetObjectType.TargetEntity
						},
						PropertyCondition = new PropertyCondition
						{
							PropertyKey = "name",
							ConstantStringEqual = "Swarmer nest 3"
						}
					},
					Actions = new EventActionType[1]
					{
						new SpawnEntityAction("edf6saf32525egtytrtews33ea9-e4af3262e2-4c3sg257-8312-7ce1944a9d9e")
						{
							DelayInSeconds = delayInSeconds,
							EntityDataKey = new ValueNode
							{
								String = "swarmerNest3"
							}
						}
					}
				},
				new ActionSetType("d92a70sada252afa3f3ffesju6bv51d-afdsaf32523-47d-9b02-7cetccbfbe39f5")
				{
					Condition = new CustomCondition
					{
						TargetObject = new TargetObject
						{
							TargetObjectType = TargetObjectType.TargetEntity
						},
						PropertyCondition = new PropertyCondition
						{
							PropertyKey = "name",
							ConstantStringEqual = "Swarmer nest 4"
						}
					},
					Actions = new EventActionType[1]
					{
						new SpawnEntityAction("edf6saf3252hkslnbv5egtytrtews33ea9-e4af3262e2-4c3sg257-8312-7ce1944a9d9e")
						{
							DelayInSeconds = delayInSeconds,
							EntityDataKey = new ValueNode
							{
								String = "swarmerNest4"
							}
						}
					}
				},
				new ActionSetType("d92a70sada252afa3fjkiaww3ff51d-afdsaf32523-47d-9b02-7cccbfbe39f5")
				{
					Condition = new CustomCondition
					{
						TargetObject = new TargetObject
						{
							TargetObjectType = TargetObjectType.TargetEntity
						},
						PropertyCondition = new PropertyCondition
						{
							PropertyKey = "name",
							ConstantStringEqual = "Swarmer nest 5"
						}
					},
					Actions = new EventActionType[1]
					{
						new SpawnEntityAction("edf6saf32sadwaghjj525egtytrtews33ea9-e4af3262e2-4c3sg257-8312-7ce1944a9d9e")
						{
							DelayInSeconds = delayInSeconds,
							EntityDataKey = new ValueNode
							{
								String = "swarmerNest5"
							}
						}
					}
				},
				new ActionSetType("d92a70sada252afa3f53533fds3ff51d-afdsaf32523-47d-9b02-7cccbfbe39f5")
				{
					Condition = new CustomCondition
					{
						TargetObject = new TargetObject
						{
							TargetObjectType = TargetObjectType.TargetEntity
						},
						PropertyCondition = new PropertyCondition
						{
							PropertyKey = "name",
							ConstantStringEqual = "Swarmer nest 6"
						}
					},
					Actions = new EventActionType[1]
					{
						new SpawnEntityAction("edf6saf32525egtytrte51ws33ea9-e4af3262e2-4c3sg257-8312-7ce1944saavwha9d9e")
						{
							DelayInSeconds = delayInSeconds,
							EntityDataKey = new ValueNode
							{
								String = "swarmerNest6"
							}
						}
					}
				}
			}
		});
		list.Add(new ActionSets
		{
			KeyName = "showTutorialMiningScenario7",
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("645tdghjhdgkgdjkfjkdjhj82b")
				{
					Actions = new EventActionType[1]
					{
						new ShowTutorialAction("01f2dfhkjfljfhfhklfhljfhfgd7d2f30")
						{
							TutorialPageKey = "tutorialMiningScenario7"
						}
					}
				}
			}
		});
		return list;
	}
}
