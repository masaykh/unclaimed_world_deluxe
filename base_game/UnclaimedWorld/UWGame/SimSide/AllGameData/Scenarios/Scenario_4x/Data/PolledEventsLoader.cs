using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.InGameEvents;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using UWGame.SimSide.Maps.MapEditor;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_4x.Data;

public class PolledEventsLoader
{
	public static List<PolledEventType> Init()
	{
		List<PolledEventType> list = new List<PolledEventType>();
		list.Add(new PolledEventType
		{
			KeyName = "SANDBOXNOMADMAP_migrationLesserWhipjawEast",
			PollInterval = new ValueNode
			{
				PropertyKey = "lesserWhipjawMigrationInterval"
			},
			StartTimePoint = new TimePoint
			{
				RelativeTimeInSeconds = new ValueNode
				{
					PropertyKey = "lesserWhipjawMigrationInterval"
				}
			},
			AllowRandomTimeOffset = false,
			Condition = new CustomCondition
			{
				TargetObject = new TargetObject
				{
					GetList = new GetList
					{
						HasPropertiesListKey = "allegiances",
						FilterCondition = new PropertyCondition
						{
							PropertyKey = "keyName",
							ConstantStringEqual = "lesserWhipjawAllegianceWest"
						},
						NextList = new GetList
						{
							HasPropertiesListKey = "members"
						}
					}
				},
				ListCondition = new ListCondition
				{
					CountMaximum = new ValueNode
					{
						PropertyKey = "maxLesserWhipjaw"
					}
				}
			},
			ActionSetsKey = "migrationLesserWhipjawEast"
		});
		list.Add(new PolledEventType
		{
			KeyName = "SANDBOXNOMADMAP_migrationBajinganNorth",
			PollInterval = new ValueNode
			{
				PropertyKey = "bajinganMigrationInterval"
			},
			StartTimePoint = new TimePoint
			{
				RelativeTimeInSeconds = new ValueNode
				{
					PropertyKey = "bajinganMigrationInterval"
				}
			},
			AllowRandomTimeOffset = false,
			Condition = new CustomCondition
			{
				TargetObject = new TargetObject
				{
					GetList = new GetList
					{
						HasPropertiesListKey = "allegiances",
						FilterCondition = new PropertyCondition
						{
							PropertyKey = "keyName",
							ConstantStringEqual = "bajinganAllegianceWest"
						},
						NextList = new GetList
						{
							HasPropertiesListKey = "members"
						}
					}
				},
				ListCondition = new ListCondition
				{
					CountMaximum = new ValueNode
					{
						PropertyKey = "maxBajingan"
					}
				}
			},
			ActionSetsKey = "migrationBajinganNorth"
		});
		list.Add(new PolledEventType
		{
			KeyName = "SANDBOXNOMADMAP_spawnLeafcutterNests",
			ActionSets = new ActionSets
			{
				SetsOfActions = new ActionSetType[1]
				{
					new ActionSetType("79dfgshsfghfsgzjshgfjs346x34fhjhs6e20")
					{
						Actions = new EventActionType[8]
						{
							new SpawnEntityAction("4e8dghzjdgjdghjd56666665gjdgxjdg503c1")
							{
								EntityData = new EntityData
								{
									EntityKey = "terrain:fieldQuaditeNest",
									Name = "Leafcutter nest (coord. 33;27)",
									Location = new Vector3(1584f, 1296f, 0f),
									Threat = new Threat
									{
										ThreatGroupName = "leafcutterAllegiance#1"
									}
								}
							},
							new SpawnEntityAction("4e8dghjzdgjdghjd56x666665gjdgjdg503c3")
							{
								EntityData = new EntityData
								{
									EntityKey = "terrain:fieldQuaditeNest",
									Name = "Leafcutter nest (coord. 52;5)",
									Location = new Vector3(2496f, 240f, 0f),
									Threat = new Threat
									{
										ThreatGroupName = "leafcutterAllegiance#2"
									}
								}
							},
							new SpawnEntityAction("4e8dghjdgjdghjd5xz6666665gjdgjdg503c4")
							{
								EntityData = new EntityData
								{
									EntityKey = "terrain:fieldQuaditeNest",
									Name = "Leafcutter nest (coord. 75;26)",
									Location = new Vector3(3600f, 1248f, 0f),
									Threat = new Threat
									{
										ThreatGroupName = "leafcutterAllegiance#3"
									}
								}
							},
							new SetPropertyAction("4e8dasfa3wf33afaffwsfzjd56666665gjdgjdg503c7")
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.Root
								},
								PropertyKey = "nest1",
								Value = new ValueNode
								{
									Bool = true
								}
							},
							new SetPropertyAction("4e8dghjdgjaws2524266665gjdgjdg503c7")
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.Root
								},
								PropertyKey = "nest2",
								Value = new ValueNode
								{
									Bool = true
								}
							},
							new SetPropertyAction("4e8dghjdgjdghzjd5666awsfraw253afsf6665gjdgjdg503c7")
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.Root
								},
								PropertyKey = "nest3",
								Value = new ValueNode
								{
									Bool = true
								}
							},
							new SetPropertyAction("4e8dghjdgjdghzjd56af2542a52agjdgjdg503c7")
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.Root
								},
								PropertyKey = "nest4",
								Value = new ValueNode
								{
									Bool = true
								}
							},
							new SetPropertyAction("4e8dghjdgawfzjd56fawafwafafaawfwa66665gjdgjdg503c7")
							{
								TargetObject = new TargetObject
								{
									TargetObjectType = TargetObjectType.Root
								},
								PropertyKey = "nest5",
								Value = new ValueNode
								{
									Bool = true
								}
							}
						}
					}
				}
			}
		});
		return list;
	}
}
