using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.GameEvents;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.InGameEvents;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Maps.MapEditor;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_2.Data;

public class PolledEventsLoader
{
	public static List<PolledEventType> Init()
	{
		List<PolledEventType> list = new List<PolledEventType>();
		list.Add(new PolledEventType
		{
			KeyName = "MUCKROOTMAP_musicTrackList",
			PollInterval = new ValueNode
			{
				Decimal = 3200f
			},
			AllowRandomTimeOffset = false,
			ActionSets = new ActionSets
			{
				SetsOfActions = new ActionSetType[1]
				{
					new ActionSetType("2452dfjdfujr6ysdt75ud6jhufgdhb83")
					{
						Actions = new EventActionType[9]
						{
							new MusicAction("faf18teyudtydurytsusrtyusr5ab")
							{
								Comments = "duration  -  5 min 2 sec = 302 sec   duration number of days = 0,189.",
								Song = "Jesper Lundager - Building a Home_320"
							},
							new MusicAction("a8yujdtjyrtydujytdjd4473aa068")
							{
								DelayInSeconds = 302.0,
								Comments = "duration -  4 min 19 sec =  259 sec . number of days: 1/1600=0,000625 *259 = 0,162",
								Song = "Jesper Lundager - Prosperous Frontier_320"
							},
							new MusicAction("a8dtyujdhgjhgdjhgdjytreewtyuw5rtyutywrutywraa068")
							{
								Comments = "duration -  3 min 19 sec  = 199 sec ...number of days: 1/1600=0,000625 *199 = 0,124",
								DelayInSeconds = 561.0,
								Song = "Jesper Lundager - Muckroot Toil_320"
							},
							new MusicAction("atyutydutyjudytjtysdurtydsu5sw65eswtywraa068")
							{
								Comments = "RelativeNoOfDays = 0.475  // dark at 0.5? (=1.0)",
								DelayInSeconds = 760.0,
								Song = "Jesper Lundager - Cetian Skies_320"
							},
							new MusicAction("addtyujdytujdtyjtydjid7eyudygjutsduyudtyu68")
							{
								Comments = "RelativeNoOfDays = 0.646   night",
								DelayInSeconds = 1034.0,
								Song = "Martin Hasseldam - Unfamiliar Starlight"
							},
							new MusicAction("adkjglkjglgkjlgjkuteyudtyutydtyudtyu4674674674tyu68")
							{
								Comments = "RelativeNoOfDays = 0.795  // morning",
								DelayInSeconds = 1272.0,
								Song = "Martin Hasseldam - Life in the Wilderness"
							},
							new MusicAction("wgjklkjlgjklgjkltwywrtytyutydtyudtyu4674674674tyu68")
							{
								Comments = "",
								DelayInSeconds = 1796.0,
								Song = "Martin Hasseldam - Settle"
							},
							new MusicAction("ygkjlkjglgjkljkuityutyutyudtyudtyu4674674674tyu68")
							{
								DelayInSeconds = 2711.0,
								Song = "Jesper Lundager - The Diamond Birds_320"
							},
							new MusicAction("kadfgfdagadfgaf67iyuiyuiyuiyuidtyudtyu4674674674tyu68")
							{
								DelayInSeconds = 2892.0,
								Song = "Martin Hasseldam - A New World (Alt3) 320kBit"
							}
						}
					}
				}
			}
		});
		list.Add(new PolledEventType
		{
			KeyName = "showEndScenarioPromptAfterBurial",
			PollInterval = new ValueNode
			{
				Decimal = 1f
			},
			Condition = new ConditionFunction
			{
				Operator = OperatorType.And,
				Left = new CustomCondition
				{
					PropertyCondition = new PropertyCondition
					{
						PropertyKey = "homebaseIsFriendly",
						BoolValue = true
					}
				},
				Right = new CustomCondition
				{
					PropertyCondition = new PropertyCondition
					{
						PropertyKey = "burialOccurred",
						BoolValue = true
					}
				}
			},
			ActionSets = new ActionSets
			{
				SetsOfActions = new ActionSetType[1]
				{
					new ActionSetType("e97919d6-83d0-4895-8764-8ed07d4391cd")
					{
						Actions = new EventActionType[2]
						{
							new SetPropertyAction("8f475e27-cb99-4411-88de-53fff91d6944")
							{
								PropertyKey = "burialOccurred",
								Value = new ValueNode
								{
									Bool = false
								}
							},
							new EventActionDialog("726ac8fc-bfe3-4b95-8457-ddc04c47cb31")
							{
								DisplayText = new DynamicText
								{
									Text = "We are welcome to abort the mission and go back. what do we choose?"
								},
								DisplayImage = "Rescue",
								DialogOptions = new DialogOption[2]
								{
									new DialogOption
									{
										Text = "CONTINUE",
										Tooltip = "No thanks, we will continue our mission."
									},
									new DialogOption
									{
										Text = "ABORT",
										Tooltip = "Yes, let's end it here.",
										ActionSet = "missionAborted"
									}
								}
							}
						}
					}
				}
			}
		});
		list.Add(new PolledEventType
		{
			KeyName = "MUCKROOTMAP_loseGame",
			UseDefaultPollInterval = true,
			AllowRandomTimeOffset = true,
			Condition = new PlayerAllegiancePersons
			{
				MaxMembers = 0
			},
			ActionSets = new ActionSets
			{
				SetsOfActions = new ActionSetType[1]
				{
					new ActionSetType("a85a34e8-9304-4240-aa11-1e661c4ac8cb")
					{
						Actions = new EventActionType[2]
						{
							new LoseGameAction("4742abae-3b47-41ca-83f7-71673f9ef428")
							{
								LoseScreenText = " \nWith the instinctive willpower of pioneers, humans strove to tame a planet whose instincts told it to resist. This duel went on for generations as hope was built, crushed and rebuilt, and lessons were repeatedly learned and forgotten. \n \nTime would tell if the human presence on Antheia was just a temporary incursion or if they were destined to dominate this biosphere just like Earth."
							},
							new MusicAction("11b0c9c0-08e9-4647-87ca-b5f774d72629")
							{
								Song = "Martin Hasseldam - Unfamiliar Starlight"
							}
						}
					}
				}
			}
		});
		list.Add(new PolledEventType
		{
			KeyName = "MUCKROOTMAP_placeNests",
			ActionSetsKey = "placeNests"
		});
		list.Add(new PolledEventType
		{
			KeyName = "MUCKROOTMAP_timedSpawnBeginningPopulationNormal",
			ActionSets = new ActionSets
			{
				SetsOfActions = new ActionSetType[1]
				{
					new ActionSetType("de53dc48-ee8b-4579-b82b-103a7ce8eb50")
					{
						Actions = new EventActionType[41]
						{
							new SpawnEntityAction("0936dda4-c34b-4ca4-986b-9b4600aa4be2")
							{
								DelayInSeconds = 0.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:patrician",
									Name = "Patrician",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "Patrician Allegiance#1"
									},
									Location = new Vector3(1848f, 1181f, 0f),
									Bulk = 5.5f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult,
										RaceKey = "Zebra patrician"
									}
								}
							},
							new SpawnEntityAction("5bf36fa1-a4cd-49cf-ad73-1bc5a37d498c")
							{
								DelayInSeconds = 0.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:patrician",
									Name = "Patrician",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "Patrician Allegiance#2"
									},
									Location = new Vector3(835f, 1748f, 0f),
									Bulk = 4.7f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult
									}
								}
							},
							new SpawnEntityAction("879f5c82-e70b-47af-bab0-58114eacc8ef")
							{
								DelayInSeconds = 0.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:patrician",
									Name = "Patrician",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "Patrician Allegiance#4"
									},
									Location = new Vector3(1152f, 2256f, 0f),
									Bulk = 4.8f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult
									}
								}
							},
							new SpawnEntityAction("61079c7f-f2b2-426b-bdd1-4222f8ffc529")
							{
								DelayInSeconds = 0.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:patrician",
									Name = "Patrician",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "Patrician Allegiance#5"
									},
									Location = new Vector3(960f, 3552f, 0f),
									Bulk = 5.5f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult
									}
								}
							},
							new SpawnEntityAction("c62aab15-5b78-4761-b2f7-e2b9299abdc9")
							{
								DelayInSeconds = 0.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:patrician",
									Name = "Patrician",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "Patrician Allegiance#6"
									},
									Location = new Vector3(5424f, 3456f, 0f),
									Bulk = 4.3f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult
									}
								}
							},
							new SpawnEntityAction("0ea2dda2-f99c-47f8-be31-47092fb17b4b")
							{
								DelayInSeconds = 0.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:patrician",
									Name = "Patrician",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "Patrician Allegiance#7"
									},
									Location = new Vector3(3984f, 2496f, 0f),
									Bulk = 4.8f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult
									}
								}
							},
							new SpawnEntityAction("cf0e002c-981f-42cb-865b-1030df3e1b10")
							{
								DelayInSeconds = 0.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:patrician",
									Name = "Patrician",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "Patrician Allegiance#7"
									},
									Location = new Vector3(720f, 2160f, 0f),
									Bulk = 4.8f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult
									}
								}
							},
							new SpawnEntityAction("8bc9b967-e062-4286-b1d0-8355dfa63d7a")
							{
								DelayInSeconds = 0.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:turnip",
									Name = "Turnip",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "TurnipAllegiance"
									},
									Location = new Vector3(3360f, 1008f, 0f),
									Bulk = 8f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult
									}
								}
							},
							new SpawnEntityAction("5be138c4-2d5c-4f0e-b204-e8a697b48e74")
							{
								DelayInSeconds = 0.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:turnip",
									Name = "Turnip",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "TurnipAllegiance"
									},
									Location = new Vector3(1440f, 96f, 0f),
									Bulk = 8f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult
									}
								}
							},
							new SpawnEntityAction("cef57d19-66d6-453e-8a1f-64f2d8d86829")
							{
								DelayInSeconds = 0.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:turnip",
									Name = "Turnip",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "TurnipAllegiance"
									},
									Location = new Vector3(3888f, 4416f, 0f),
									Bulk = 6f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult
									}
								}
							},
							new SpawnEntityAction("9a3b9865-b7e6-4399-b953-4ca9ccf85ab2")
							{
								DelayInSeconds = 0.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:turnip",
									Name = "Turnip",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "TurnipAllegiance"
									},
									Location = new Vector3(4128f, 4368f, 0f),
									Bulk = 5f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult
									}
								}
							},
							new SpawnEntityAction("1d351afd-05ae-4229-9115-eba93765044a")
							{
								DelayInSeconds = 0.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:turnip",
									Name = "Turnip",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "TurnipAllegiance"
									},
									Location = new Vector3(1488f, 1296f, 0f),
									Bulk = 5f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult
									}
								}
							},
							new SpawnEntityAction("7c982eac-10bb-4531-92e3-3725a2c0afc5")
							{
								DelayInSeconds = 0.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:bushDragon",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "Allegiance #80"
									},
									Location = new Vector3(2160f, 2400f, 0f),
									Bulk = 0.98f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult
									}
								}
							},
							new SpawnEntityAction("dc14206c-ab78-40f5-b3be-f6241e542d4e")
							{
								DelayInSeconds = 0.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:bushDragon",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "Allegiance #80"
									},
									Location = new Vector3(2400f, 2544f, 0f),
									Bulk = 1.2f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult
									}
								}
							},
							new SpawnEntityAction("7b215c2e-d556-4b80-80f5-900c0547361b")
							{
								DelayInSeconds = 0.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:bushDragon",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "Allegiance #80"
									},
									Location = new Vector3(4848f, 2448f, 0f),
									Bulk = 0.9f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult
									}
								}
							},
							new SpawnEntityAction("6363a945-3d6f-42bb-b297-1ff2dd9011d8")
							{
								DelayInSeconds = 0.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:twinkler",
									Name = "Twinkler",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "twinklerAllegiance"
									},
									Location = new Vector3(1344f, 2352f, 0f),
									Bulk = 0.5f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult,
										CasteKey = "hunter"
									}
								}
							},
							new SpawnEntityAction("ccda6e33-443e-44df-ac18-05c59618b690")
							{
								DelayInSeconds = 0.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:twinkler",
									Name = "Twinkler",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "twinklerAllegiance"
									},
									Location = new Vector3(1152f, 1296f, 0f),
									Bulk = 0.5f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult,
										CasteKey = "hunter"
									}
								}
							},
							new SpawnEntityAction("6eb20215-f892-44e4-bfc8-7c1e64c9184c")
							{
								DelayInSeconds = 0.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:twinkler",
									Name = "Twinkler",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "twinklerAllegiance"
									},
									Location = new Vector3(2976f, 1248f, 0f),
									Bulk = 0.5f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult,
										CasteKey = "hunter"
									}
								}
							},
							new SpawnEntityAction("29e65745-24f8-4323-a2e7-9b1462323a26")
							{
								DelayInSeconds = 0.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:twinkler",
									Name = "Twinkler",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "twinklerAllegiance"
									},
									Location = new Vector3(2736f, 864f, 0f),
									Bulk = 0.6f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult,
										CasteKey = "hunter"
									}
								}
							},
							new SpawnEntityAction("533125cd-7f6f-49d9-b3fa-a6c69f16a3e6")
							{
								DelayInSeconds = 0.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:twinkler",
									Name = "Twinkler",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "twinklerAllegiance"
									},
									Location = new Vector3(2688f, 288f, 0f),
									Bulk = 0.7f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult,
										CasteKey = "hunter"
									}
								}
							},
							new SpawnEntityAction("85e440dc-81ea-4720-8f7c-d7f6e33254a6")
							{
								DelayInSeconds = 0.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:twinkler",
									Name = "Twinkler",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "twinklerAllegiance"
									},
									Location = new Vector3(336f, 2074f, 0f),
									Bulk = 0.5f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult,
										CasteKey = "hunter"
									}
								}
							},
							new SpawnEntityAction("ff175b6c-cc9b-4df5-9d3f-2b46aa3574c3")
							{
								DelayInSeconds = 0.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:twinkler",
									Name = "Twinkler",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "twinklerAllegiance"
									},
									Location = new Vector3(5856f, 2352f, 0f),
									Bulk = 0.4f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult,
										CasteKey = "hunter"
									}
								}
							},
							new SpawnEntityAction("5b69f792-8982-4639-ad1f-cd1eb87b0d5e")
							{
								DelayInSeconds = 0.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:twinkler",
									Name = "Twinkler",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "twinklerAllegiance"
									},
									Location = new Vector3(5760f, 3888f, 0f),
									Bulk = 0.6f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult,
										CasteKey = "hunter"
									}
								}
							},
							new SpawnEntityAction("0a51c013-d621-463d-b674-3a5b9f98f2f3")
							{
								DelayInSeconds = 9.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:binalRat",
									Name = "BinalRat11",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "binalRatAllegianceNorthWest"
									},
									Location = new Vector3(1296f, 1344f, 0f),
									Bulk = 0.21f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult
									}
								}
							},
							new SpawnEntityAction("9fa25c93-68c4-46c6-bc27-0add6663cd55")
							{
								DelayInSeconds = 0.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:binalRat",
									Name = "BinalRat6",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "binalRatAllegianceNorthWest"
									},
									Location = new Vector3(1344f, 1344f, 0f),
									Bulk = 0.21f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult
									}
								}
							},
							new SpawnEntityAction("cc29a7b9-b66d-4336-8682-629af0e46d1f")
							{
								DelayInSeconds = 2.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:pygmyThunderChicken",
									Name = "Thunder Chicken",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "thunderChickenAllegianceNorthWest"
									},
									Location = new Vector3(432f, 2160f, 0f),
									Bulk = 0.21f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult
									}
								}
							},
							new SpawnEntityAction("c3d8d2c6-28e9-4547-92c3-482a2e351638")
							{
								DelayInSeconds = 8.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:whiteThunderChicken",
									Name = "Thunder Chicken",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "thunderChickenAllegianceNorthWest"
									},
									Location = new Vector3(1344f, 1344f, 0f),
									Bulk = 0.21f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult
									}
								}
							},
							new SpawnEntityAction("b574244f-536756375675637536-0832067cd35f")
							{
								DelayInSeconds = 1.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:whiteThunderChicken",
									Name = "Thunder Chicken Bulky",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "thunderChickenAllegianceNorthWest"
									},
									Location = new Vector3(440f, 2200f, 0f),
									Bulk = 0.3f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult,
										CasteKey = "male"
									}
								}
							},
							new SpawnEntityAction("4ac3a89tryutyudyudtyu96e3bb064")
							{
								DelayInSeconds = 1.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:bajingan",
									Name = "Thunder Chicken Thin",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "thunderChickenAllegianceNorthWest"
									},
									Location = new Vector3(450f, 2180f, 0f),
									Bulk = 0.3f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult,
										CasteKey = "male"
									}
								}
							},
							new SpawnEntityAction("e363cda2-4859-4d97-bc54-0d56c184aa5d")
							{
								DelayInSeconds = 1.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:bird",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "Allegiance #43"
									},
									Location = MapManager.TileToWorldPosVector2(new Point(14, 62)).ToVector3(),
									Rotation = 167f,
									Bulk = 0.12f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult
									}
								}
							},
							new SpawnEntityAction("c093719f-0856-4ba5-ab1b-382bd905becd")
							{
								DelayInSeconds = 1.85,
								EntityData = new EntityData
								{
									EntityKey = "entity:bird",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "Allegiance #43"
									},
									Location = MapManager.TileToWorldPosVector2(new Point(16, 64)).ToVector3(),
									Rotation = 208f,
									Bulk = 0.12f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult
									}
								}
							},
							new SpawnEntityAction("748700ca-0738-4f79-8ce8-d4deb83ecd9b")
							{
								DelayInSeconds = 2.6,
								EntityData = new EntityData
								{
									EntityKey = "entity:bird",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "Allegiance #43"
									},
									Location = MapManager.TileToWorldPosVector2(new Point(15, 63)).ToVector3(),
									Rotation = 137f,
									Bulk = 0.12f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult
									}
								}
							},
							new SpawnEntityAction("253b90c4-4846-4d59-bff1-28bf3027d101")
							{
								DelayInSeconds = 3.2,
								EntityData = new EntityData
								{
									EntityKey = "entity:bird",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "Allegiance #43"
									},
									Location = MapManager.TileToWorldPosVector2(new Point(16, 61)).ToVector3(),
									Rotation = 316f,
									Bulk = 0.12f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult
									}
								}
							},
							new SpawnEntityAction("093635673dwa2542asf555555555565673aa4be2")
							{
								DelayInSeconds = 2.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:megapod",
									Name = "Worm",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "Worm Allegiance#1"
									},
									Location = new Vector3(3892f, 2548f, 0f),
									Bulk = 6f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult
									}
								}
							},
							new SpawnEntityAction("093635673567355687ht5555565673aa4be2")
							{
								DelayInSeconds = 2.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:megapod",
									Name = "Worm",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "Worm Allegiance#1"
									},
									Location = new Vector3(4800f, 816f, 0f),
									Bulk = 6f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult
									}
								}
							},
							new SpawnEntityAction("09363rwytuhjstrjhsfgjhgfsjsfggjsyj4be2")
							{
								DelayInSeconds = 0.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:whipjaw",
									Name = "Snatcher",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "snatcherAllegiance"
									},
									Location = new Vector3(1104f, 2832f, 0f),
									Bulk = 3f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult,
										CasteKey = "female"
									}
								}
							},
							new SpawnEntityAction("093srytu656576eiu7e5iutyiujsjhfsgbe2")
							{
								DelayInSeconds = 0.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:whipjaw",
									Name = "Whipjaw",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "snatcherAllegiance"
									},
									Location = new Vector3(1200f, 3000f, 0f),
									Bulk = 3f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult,
										CasteKey = "female"
									}
								}
							},
							new SpawnEntityAction("0933r6wytuhjrjhsfstgjhgfsjsfggjsyj4be2")
							{
								DelayInSeconds = 0.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:whipjaw",
									Name = "Whipjaw",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "snatcherAllegiance"
									},
									Location = new Vector3(1150f, 2632f, 0f),
									Bulk = 3f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult,
										CasteKey = "male"
									}
								}
							},
							new SpawnEntityAction("0936356735673fwa2easd5555555565673aa4be2")
							{
								DelayInSeconds = 0.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:spoakDendront",
									Name = "Demon Tree",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "demonTreeAllegiance#1"
									},
									Location = new Vector3(4224f, 2592f, 0f),
									Bulk = 1.3f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult
									}
								}
							},
							new SpawnEntityAction("5b35677777777777777536753675367798c")
							{
								DelayInSeconds = 0.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:spoakDendront",
									Name = "Demon Tree",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "demonTreeAllegiance#2"
									},
									Location = new Vector3(2419f, 558f, 0f),
									Bulk = 1.4f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult
									}
								}
							},
							new SpawnEntityAction("5b35fdsgdhdfhgdfjdhdhdghjdhjghj7798c")
							{
								DelayInSeconds = 0.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:spoakDendront",
									Name = "Demon Tree",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "demonTreeAllegiance#3"
									},
									Location = new Vector3(1735f, 1648f, 0f),
									Bulk = 1.4f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult
									}
								}
							}
						}
					}
				}
			}
		});
		list.Add(new PolledEventType
		{
			KeyName = "MUCKROOTMAP_timedSpawnBeginningPopulationEasy",
			ActionSets = new ActionSets
			{
				SetsOfActions = new ActionSetType[1]
				{
					new ActionSetType("ee47e50e-deb0-4e3d-800c-a544639c659f")
					{
						Actions = new EventActionType[10]
						{
							new SpawnEntityAction("4a3396fd-061c-414f-b192-13b4fb05b18e")
							{
								DelayInSeconds = 0.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:bushDragon",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "Allegiance #80"
									},
									Location = new Vector3(2160f, 2400f, 0f),
									Bulk = 0.98f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult
									}
								}
							},
							new SpawnEntityAction("8b27f9c3-3de2-4331-85d6-de67f85c30a4")
							{
								DelayInSeconds = 0.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:bushDragon",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "Allegiance #80"
									},
									Location = new Vector3(2400f, 2544f, 0f),
									Bulk = 1.2f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult
									}
								}
							},
							new SpawnEntityAction("beb84854-5448-4da8-b654-bbf31bf5293c")
							{
								DelayInSeconds = 0.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:bushDragon",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "Allegiance #80"
									},
									Location = new Vector3(1632f, 2160f, 0f),
									Bulk = 0.9f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult
									}
								}
							},
							new SpawnEntityAction("0be05359-5480-4138-94b4-cc3da6569668")
							{
								DelayInSeconds = 0.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:bird",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "Allegiance #43"
									},
									Location = new Vector3(448f, 2719f, 0f),
									Rotation = 115f,
									Bulk = 0.12f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult
									}
								}
							},
							new SpawnEntityAction("6c7f4665-b922-4e3f-9fa2-11d4853b476e")
							{
								DelayInSeconds = 1.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:bird",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "Allegiance #43"
									},
									Location = new Vector3(489f, 2734f, 0f),
									Rotation = 167f,
									Bulk = 0.12f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult
									}
								}
							},
							new SpawnEntityAction("7ac057e0-bfea-4dce-86d6-a656e6ee121f")
							{
								DelayInSeconds = 1.85,
								EntityData = new EntityData
								{
									EntityKey = "entity:bird",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "Allegiance #43"
									},
									Location = new Vector3(565f, 2869f, 0f),
									Rotation = 208f,
									Bulk = 0.12f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult
									}
								}
							},
							new SpawnEntityAction("3a0e3496-09c4-4fb7-bf83-ad2308135723")
							{
								DelayInSeconds = 2.6,
								EntityData = new EntityData
								{
									EntityKey = "entity:bird",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "Allegiance #43"
									},
									Location = new Vector3(580f, 2839f, 0f),
									Rotation = 137f,
									Bulk = 0.12f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult
									}
								}
							},
							new SpawnEntityAction("59922eb7-2dd2-44d2-bf28-0699ce1167a1")
							{
								DelayInSeconds = 3.2,
								EntityData = new EntityData
								{
									EntityKey = "entity:bird",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "Allegiance #43"
									},
									Location = new Vector3(626f, 2854f, 0f),
									Rotation = 316f,
									Bulk = 0.12f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult
									}
								}
							},
							new SpawnEntityAction("c2fd62b9-1bca-4a81-9248-eee0c04a7d90")
							{
								DelayInSeconds = 2.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:bird",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "Allegiance #43"
									},
									Location = new Vector3(659f, 2908f, 0f),
									Rotation = 112f,
									Bulk = 0.12f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult
									}
								}
							},
							new SpawnEntityAction("cef097d2-1a3f-4826-8bea-19ed0cb4b04b")
							{
								DelayInSeconds = 0.0,
								EntityData = new EntityData
								{
									EntityKey = "entity:twinkler",
									Name = "twinkler3",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "twinklerAllegiance"
									},
									Location = new Vector3(1008f, 1632f, 0f),
									Bulk = 0.25f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult,
										CasteKey = "hunter"
									}
								}
							}
						}
					}
				}
			}
		});
		list.Add(new PolledEventType
		{
			KeyName = "MUCKROOTMAP_continualSpawnThunderChickenNorthWest",
			PollInterval = new ValueNode
			{
				Int = 32
			},
			StartTimePoint = new TimePoint
			{
				RelativeTimeInSeconds = new ValueNode
				{
					Decimal = 32f
				}
			},
			AllowRandomTimeOffset = true,
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
							ConstantStringEqual = "thunderChickenAllegianceNorthWest"
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
						Int = 10
					}
				}
			},
			ActionSetsKey = "continualSpawnThunderChickenNorthWest"
		});
		list.Add(new PolledEventType
		{
			KeyName = "MUCKROOTMAP_continualSpawnTwinklersNorthWestCrevice",
			PollInterval = new ValueNode
			{
				PropertyKey = "twinklerSpawnIntervalSouth"
			},
			StartAfterInterval = true,
			AllowRandomTimeOffset = true,
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
							ConstantStringEqual = "twinklerAllegiance"
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
						PropertyKey = "maxTwinklers"
					}
				}
			},
			ActionSetsKey = "continualSpawnTwinklersNorthWestCrevice"
		});
		list.Add(new PolledEventType
		{
			KeyName = "MUCKROOTMAP_continualSpawnBinalRatsNorthWest1",
			PollInterval = new ValueNode
			{
				Int = 30
			},
			StartTimePoint = new TimePoint
			{
				RelativeTimeInSeconds = new ValueNode
				{
					Int = 30
				}
			},
			AllowRandomTimeOffset = true,
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
							ConstantStringEqual = "binalRatAllegianceNorthWest1"
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
						Int = 15
					}
				}
			},
			ActionSetsKey = "continualSpawnBinalRatsNorthWest1"
		});
		list.Add(new PolledEventType
		{
			KeyName = "MUCKROOTMAP_continualSpawnBinalRatsNorthWest2",
			PollInterval = new ValueNode
			{
				Int = 30
			},
			StartTimePoint = new TimePoint
			{
				RelativeTimeInSeconds = new ValueNode
				{
					Int = 30
				}
			},
			AllowRandomTimeOffset = true,
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
							ConstantStringEqual = "binalRatAllegianceNorthWest2"
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
						Int = 15
					}
				}
			},
			ActionSetsKey = "continualSpawnBinalRatsNorthWest2"
		});
		list.Add(new PolledEventType
		{
			KeyName = "MUCKROOTMAP_continualSpawnBinalRatsNorthWest3",
			PollInterval = new ValueNode
			{
				Int = 30
			},
			StartTimePoint = new TimePoint
			{
				RelativeTimeInSeconds = new ValueNode
				{
					Int = 30
				}
			},
			AllowRandomTimeOffset = true,
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
							ConstantStringEqual = "binalRatAllegianceNorthWest2"
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
						Int = 15
					}
				}
			},
			ActionSetsKey = "continualSpawnBinalRatsNorthWest3"
		});
		return list;
	}
}
