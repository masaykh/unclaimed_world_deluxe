using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.InGameEvents;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using UWGame.SimSide.Maps.MapEditor;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_4.Data;

public class PolledEventsLoader
{
	public static List<PolledEventType> Init()
	{
		List<PolledEventType> list = new List<PolledEventType>();
		PolledEventType polledEventType = new PolledEventType();
		polledEventType.KeyName = "SANDBOXNOMADMAP_timedSpawnBeginningPopulationNormal";
		polledEventType.ActionSets = new ActionSets
		{
			SetsOfActions = new ActionSetType[1]
			{
				new ActionSetType("0a4f3522-1ea4-46f1-b2e4-7a4305b5a326")
				{
					Actions = new EventActionType[59]
					{
						new SpawnEntityAction("3dd273e1-338d-42a4-8167-ba8db92c1966")
						{
							DelayInSeconds = 0.1,
							EntityData = new EntityData
							{
								EntityKey = "entity:spoakDendront",
								Name = "Demon Tree",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "demonTreeAllegiance#3"
								},
								Location = new Vector3(1933f, 264f, 0f),
								Bulk = 1.3f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						},
						new SpawnEntityAction("aa0adw2q53256-e54d-4a8d-a48a-dfb1f37600e7")
						{
							DelayInSeconds = 0.1,
							EntityData = new EntityData
							{
								EntityKey = "entity:spoakDendront",
								Name = "Demon Tree",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "demonTreeAllegiance#1"
								},
								Location = new Vector3(3189f, 2645f, 0f),
								Bulk = 1.3f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						},
						new SpawnEntityAction("aa089ba6-e54afw2525a24a48a-dfb1f37600e7")
						{
							EntityData = new EntityData
							{
								EntityKey = "entity:swampDendront",
								Name = "Swamp Demon Tree",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "swampDemonTreeAllegiance#2"
								},
								Location = new Vector3(624f, 336f, 0f),
								Bulk = 1.3f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						},
						new SpawnEntityAction("91dfzsb3a5242asf-6774-4aa8-b1f6-86a2f352d843")
						{
							DelayInSeconds = 0.1,
							EntityData = new EntityData
							{
								EntityKey = "entity:studdedThunderChicken",
								Name = "ThunderChicken(3120,2688)",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "thunderChickenAllegiance#2"
								},
								Location = new Vector3(3120f, 2688f, 0f),
								Bulk = 0.3f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						},
						new SpawnEntityAction("91dfzsb2fa242774-4aa8-bdse56-86a2f352d843")
						{
							DelayInSeconds = 0.1,
							EntityData = new EntityData
							{
								EntityKey = "entity:studdedThunderChicken",
								Name = "ThunderChicken(1584,2736)",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "thunderChickenAllegiance#2"
								},
								Location = new Vector3(1584f, 2736f, 0f),
								Bulk = 0.3f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						},
						new SpawnEntityAction("91dfz452390e2-6awfa2424-4aa8-b1f6-86a2f352d843")
						{
							DelayInSeconds = 0.1,
							EntityData = new EntityData
							{
								EntityKey = "entity:studdedThunderChicken",
								Name = "ThunderChicken(576,2928)",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "thunderChickenAllegiance#2"
								},
								Location = new Vector3(576f, 2928f, 0f),
								Bulk = 0.3f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						},
						new SpawnEntityAction("d78d8657-f25e-49eb-a465-1456b0eb3a98")
						{
							DelayInSeconds = 0.1,
							EntityData = new EntityData
							{
								EntityKey = "entity:turnip",
								Name = "Turnip1",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "turnipAllegianceNorth"
								},
								Location = new Vector3(2587f, 356f, 0f),
								Bulk = 4.2f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									RaceKey = "Pale Turnip"
								}
							}
						},
						new SpawnEntityAction("6f0be88a-eee5-42c9-8483-252947b950f1")
						{
							DelayInSeconds = 0.1,
							EntityData = new EntityData
							{
								EntityKey = "entity:turnip",
								Name = "Turnip2",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "turnipAllegianceNorth"
								},
								Location = new Vector3(2387f, 356f, 0f),
								Bulk = 4.2f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									RaceKey = "Pale Turnip"
								}
							}
						},
						new SpawnEntityAction("04961f4c-9623-4b26-8d10-b8e26ac51bd4")
						{
							DelayInSeconds = 0.1,
							EntityData = new EntityData
							{
								EntityKey = "entity:turnip",
								Name = "Turnip1",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "turnipAllegianceNorth"
								},
								Location = new Vector3(1819f, 1013f, 0f),
								Bulk = 4.2f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									RaceKey = "Copper Turnip"
								}
							}
						},
						new SpawnEntityAction("ea761587-970b-4ed0-b29e-02ebf3b61b2c")
						{
							DelayInSeconds = 0.1,
							EntityData = new EntityData
							{
								EntityKey = "entity:turnip",
								Name = "Turnip2",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "turnipAllegianceNorth"
								},
								Location = new Vector3(1619f, 1013f, 0f),
								Bulk = 4.2f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									RaceKey = "Copper Turnip"
								}
							}
						},
						new SpawnEntityAction("4dd5379e-17c0-4575-99d9-2f3728588213")
						{
							DelayInSeconds = 0.1,
							EntityData = new EntityData
							{
								EntityKey = "entity:turnip",
								Name = "Turnip4",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "turnipAllegianceNorth"
								},
								Location = new Vector3(1519f, 913f, 0f),
								Bulk = 4.2f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									RaceKey = "Copper Turnip"
								}
							}
						},
						new SpawnEntityAction("b4fawr2a42d-7d60-45d0-a0e7-e8436d24327c")
						{
							EntityData = new EntityData
							{
								EntityKey = "entity:megapod",
								Name = "Megapod1",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "slugAllegiance#1"
								},
								Location = new Vector3(2064f, 4704f, 0f),
								Bulk = 1.1f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						},
						new SpawnEntityAction("b4f9f2faw242460-45d0-a0e7-e8436d24327c")
						{
							EntityData = new EntityData
							{
								EntityKey = "entity:megapod",
								Name = "Megapod2",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "slugAllegiance#1"
								},
								Location = new Vector3(1584f, 4560f, 0f),
								Bulk = 1.1f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						},
						new SpawnEntityAction("b4f9f25d-7dfwa24245d0-a0e7-e8436d24327c")
						{
							EntityData = new EntityData
							{
								EntityKey = "entity:megapod",
								Name = "Megapod3",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "slugAllegiance#1"
								},
								Location = new Vector3(1920f, 4224f, 0f),
								Bulk = 1.1f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						},
						new SpawnEntityAction("b4f9f25d-af2453t-45d0-a0e7-e8436d24327c")
						{
							EntityData = new EntityData
							{
								EntityKey = "entity:megapod",
								Name = "Megapod4",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "slugAllegiance#1"
								},
								Location = new Vector3(2016f, 4944f, 0f),
								Bulk = 1.1f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						},
						new SpawnEntityAction("b4f9f25d-agfeafa60-45d0-a0e7-e8436d24327c")
						{
							EntityData = new EntityData
							{
								EntityKey = "entity:megapod",
								Name = "Megapod5",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "slugAllegiance#1"
								},
								Location = new Vector3(2688f, 4512f, 0f),
								Bulk = 1.1f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						},
						new SpawnEntityAction("b4f9f25d-7d60-afaws42e7-e8436d24327c")
						{
							EntityData = new EntityData
							{
								EntityKey = "entity:megapod",
								Name = "Megapod6",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "slugAllegiance#1"
								},
								Location = new Vector3(1776f, 5184f, 0f),
								Bulk = 1.1f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									CasteKey = "male"
								}
							}
						},
						new SpawnEntityAction("b4f9f25d-7wasfw24-45d0-a0e7-e8436d24327c")
						{
							EntityData = new EntityData
							{
								EntityKey = "entity:megapod",
								Name = "Megapod(1584,5088)",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "slugAllegiance#1"
								},
								Location = new Vector3(1584f, 5088f, 0f),
								Bulk = 1.1f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									CasteKey = "male"
								}
							}
						},
						new SpawnEntityAction("b4f9f25d-7d60-4awfa24252e7-e8436d24327c")
						{
							EntityData = new EntityData
							{
								EntityKey = "entity:megapod",
								Name = "Megapod(1680,5328)",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "slugAllegiance#1"
								},
								Location = new Vector3(1680f, 5328f, 0f),
								Bulk = 1.1f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									CasteKey = "male"
								}
							}
						},
						new SpawnEntityAction("b4f9ffwas24420-45d0-a0e7-e8436d24327c")
						{
							EntityData = new EntityData
							{
								EntityKey = "entity:megapod",
								Name = "Megapod(2448,4944)",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "slugAllegiance#1"
								},
								Location = new Vector3(2448f, 4944f, 0f),
								Bulk = 1.1f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									CasteKey = "male"
								}
							}
						},
						new SpawnEntityAction("b4f9f25d-fwaw353-45d0-a0e7-e8436d24327c")
						{
							EntityData = new EntityData
							{
								EntityKey = "entity:megapod",
								Name = "Megapod(2304,5472)",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "slugAllegiance#1"
								},
								Location = new Vector3(2304f, 5472f, 0f),
								Bulk = 1.1f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									CasteKey = "male"
								}
							}
						},
						new SpawnEntityAction("b4f9f25sfaa2d-7d60-45d0-a0e7-e8436d24327c")
						{
							EntityData = new EntityData
							{
								EntityKey = "entity:megapod",
								Name = "Megapod(2496,4848)",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "slugAllegiance#1"
								},
								Location = new Vector3(2496f, 4848f, 0f),
								Bulk = 1.1f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									CasteKey = "male"
								}
							}
						},
						new SpawnEntityAction("9c4bdc61-13bd-49f7-91c0-db90b9fd3b0d")
						{
							DelayInSeconds = 0.1,
							EntityData = new EntityData
							{
								EntityKey = "entity:whipjaw",
								Name = "Whipjaw",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "snatcherAllegiance#1"
								},
								Location = new Vector3(4992f, 624f, 0f),
								Bulk = 2f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						},
						new SpawnEntityAction("b1232ac8-cbf8-48d4-9973-5f4bdb043f75")
						{
							DelayInSeconds = 0.1,
							EntityData = new EntityData
							{
								EntityKey = "entity:bird",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "birdAllegiance"
								},
								Location = new Vector3(1280f, 4023f, 0f),
								Rotation = 100f,
								Bulk = 0.12f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									RaceKey = "dark"
								}
							}
						},
						new SpawnEntityAction("ffd9157e-351d-41c6-bc29-2115475cd555")
						{
							DelayInSeconds = 3.5,
							EntityData = new EntityData
							{
								EntityKey = "entity:bird",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "birdAllegiance"
								},
								Location = new Vector3(1322f, 3997f, 0f),
								Rotation = 190f,
								Bulk = 0.12f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									RaceKey = "pale"
								}
							}
						},
						new SpawnEntityAction("6e06ef9f-996e-4e69-bb23-5d725699af93")
						{
							DelayInSeconds = 0.25,
							EntityData = new EntityData
							{
								EntityKey = "entity:bird",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "birdAllegiance"
								},
								Location = new Vector3(2640f, 3312f, 0f),
								Rotation = 100f,
								Bulk = 0.12f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									RaceKey = "purple"
								}
							}
						},
						new SpawnEntityAction("0dfce0bf-9d9b-468c-9e2e-4d3c400afaa4")
						{
							DelayInSeconds = 1.5,
							EntityData = new EntityData
							{
								EntityKey = "entity:bird",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "birdAllegiance"
								},
								Location = new Vector3(2699f, 3351f, 0f),
								Rotation = 175f,
								Bulk = 0.12f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									RaceKey = "purple"
								}
							}
						},
						new SpawnEntityAction("7fc004f6-79b8-4ec3-8bdb-5b27ae7c8d1d")
						{
							DelayInSeconds = 2.75,
							EntityData = new EntityData
							{
								EntityKey = "entity:bird",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "birdAllegiance"
								},
								Location = new Vector3(2736f, 3312f, 0f),
								Rotation = 250f,
								Bulk = 0.12f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									RaceKey = "purple"
								}
							}
						},
						new SpawnEntityAction("a1cbea62-ca2d-4e01-8dd3-5581764e263a")
						{
							DelayInSeconds = 0.25,
							EntityData = new EntityData
							{
								EntityKey = "entity:bird",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "birdAllegiance"
								},
								Location = new Vector3(216f, 5972f, 0f),
								Rotation = 208f,
								Bulk = 0.12f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									RaceKey = "pale"
								}
							}
						},
						new SpawnEntityAction("8fec7376-c118-41ae-b5fc-342b34e026db")
						{
							DelayInSeconds = 1.5,
							EntityData = new EntityData
							{
								EntityKey = "entity:bird",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "birdAllegiance"
								},
								Location = new Vector3(274f, 5945f, 0f),
								Rotation = 137f,
								Bulk = 0.12f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									RaceKey = "dark"
								}
							}
						},
						new SpawnEntityAction("9c0c7900-d2d0-42a7-ad11-337931ba33d3")
						{
							DelayInSeconds = 3.5,
							EntityData = new EntityData
							{
								EntityKey = "entity:bird",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "birdAllegiance"
								},
								Location = new Vector3(359f, 5983f, 0f),
								Rotation = 316f,
								Bulk = 0.12f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									RaceKey = "yellow"
								}
							}
						},
						new SpawnEntityAction("fda48cee-e931-4690-8d71-97bec4a70d10")
						{
							DelayInSeconds = 0.2,
							EntityData = new EntityData
							{
								EntityKey = "entity:bird",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "birdAllegiance"
								},
								Location = new Vector3(4543f, 185f, 0f),
								Rotation = 120f,
								Bulk = 0.12f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									RaceKey = "red"
								}
							}
						},
						new SpawnEntityAction("7195180f-bb65-46a1-b729-888329aab2a6")
						{
							DelayInSeconds = 2.5,
							EntityData = new EntityData
							{
								EntityKey = "entity:bird",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "birdAllegiance"
								},
								Location = new Vector3(4678f, 195f, 0f),
								Rotation = 135f,
								Bulk = 0.12f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									RaceKey = "black"
								}
							}
						},
						new SpawnEntityAction("ccffadaa-05ff-4a5b-8415-d759612a56e9")
						{
							DelayInSeconds = 4.1,
							EntityData = new EntityData
							{
								EntityKey = "entity:bird",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "birdAllegiance"
								},
								Location = new Vector3(4673f, 245f, 0f),
								Rotation = 260f,
								Bulk = 0.12f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									RaceKey = "pale"
								}
							}
						},
						new SpawnEntityAction("b3cc39d1-901f-411c-93b2-ef8626fc2a57")
						{
							DelayInSeconds = 1.5,
							EntityData = new EntityData
							{
								EntityKey = "entity:bird",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "birdAllegiance"
								},
								Location = new Vector3(3761f, 5158f, 0f),
								Rotation = 135f,
								Bulk = 0.12f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									RaceKey = "dark"
								}
							}
						},
						new SpawnEntityAction("b3cc395eyujtdeu67657u7u6wc2a57")
						{
							DelayInSeconds = 0.1,
							EntityData = new EntityData
							{
								EntityKey = "entity:bird",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "birdAllegiance"
								},
								Location = new Vector3(3370f, 2199f, 0f),
								Rotation = 165f,
								Bulk = 0.12f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									RaceKey = "veryDarkGreen"
								}
							}
						},
						new SpawnEntityAction("b3ce56ueytyrutyyetu6756eu657")
						{
							DelayInSeconds = 6.1,
							EntityData = new EntityData
							{
								EntityKey = "entity:bird",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "birdAllegiance"
								},
								Location = new Vector3(3408f, 2209f, 0f),
								Rotation = 195f,
								Bulk = 0.12f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									RaceKey = "veryDarkGreen"
								}
							}
						},
						new SpawnEntityAction("b3ce56urtyeyyeteyuteyud657")
						{
							DelayInSeconds = 3.1,
							EntityData = new EntityData
							{
								EntityKey = "entity:bird",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "birdAllegiance"
								},
								Location = new Vector3(3395f, 2219f, 0f),
								Rotation = 145f,
								Bulk = 0.12f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									RaceKey = "veryDarkGreen"
								}
							}
						},
						new SpawnEntityAction("b3cc39d536777777reyueeetuytfc2a57")
						{
							DelayInSeconds = 1.0,
							EntityData = new EntityData
							{
								EntityKey = "entity:bird",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "birdAllegiance"
								},
								Location = new Vector3(3888f, 2064f, 0f),
								Rotation = 135f,
								Bulk = 0.12f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									RaceKey = "veryDarkTurqoise"
								}
							}
						},
						new SpawnEntityAction("b3cc39d536478674986r789rety7u6ui6t7ui7ytfc2a57")
						{
							DelayInSeconds = 7.3,
							EntityData = new EntityData
							{
								EntityKey = "entity:bird",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "birdAllegiance"
								},
								Location = new Vector3(3942f, 2074f, 0f),
								Rotation = 165f,
								Bulk = 0.12f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									RaceKey = "veryDarkTurqoise"
								}
							}
						},
						new SpawnEntityAction("b3cc39d55e6urtyutysrusrtyusrtyutyytfc2a57")
						{
							DelayInSeconds = 0.5,
							EntityData = new EntityData
							{
								EntityKey = "entity:bird",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "birdAllegiance"
								},
								Location = new Vector3(4224f, 2112f, 0f),
								Rotation = 183f,
								Bulk = 0.12f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									RaceKey = "veryDarkGreen"
								}
							}
						},
						new SpawnEntityAction("b3cc3946787568tdty8f768i678t8fc2a57")
						{
							DelayInSeconds = 5.5,
							EntityData = new EntityData
							{
								EntityKey = "entity:bird",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "birdAllegiance"
								},
								Location = new Vector3(4210f, 2064f, 0f),
								Rotation = 103f,
								Bulk = 0.12f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									RaceKey = "veryDarkGreen"
								}
							}
						},
						new SpawnEntityAction("b3e65u56eudt6u5eu756eu56u56et8fc2a57")
						{
							DelayInSeconds = 2.1,
							EntityData = new EntityData
							{
								EntityKey = "entity:bird",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "birdAllegiance"
								},
								Location = new Vector3(4300f, 1950f, 0f),
								Rotation = 65f,
								Bulk = 0.12f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									RaceKey = "veryDarkGreen"
								}
							}
						},
						new SpawnEntityAction("b3cc3953678ue56u5e67ue5u65ew6t8fc2a57")
						{
							DelayInSeconds = 0.1,
							EntityData = new EntityData
							{
								EntityKey = "entity:bird",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "birdAllegiance"
								},
								Location = new Vector3(4116f, 1960f, 0f),
								Rotation = 165f,
								Bulk = 0.12f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									RaceKey = "veryDarkGreen"
								}
							}
						},
						new SpawnEntityAction("b3ccd6fsaw2442autyutd6utd6ud6u6dd657")
						{
							DelayInSeconds = 6.1,
							EntityData = new EntityData
							{
								EntityKey = "entity:bird",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "birdAllegiance"
								},
								Location = new Vector3(4106f, 1955f, 0f),
								Rotation = 195f,
								Bulk = 0.12f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									RaceKey = "veryDarkGreen"
								}
							}
						},
						new SpawnEntityAction("b3ccd6utyutd6utd6udat253arfw6u6dd657")
						{
							DelayInSeconds = 3.1,
							EntityData = new EntityData
							{
								EntityKey = "entity:bird",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "birdAllegiance"
								},
								Location = new Vector3(4502f, 2020f, 0f),
								Rotation = 145f,
								Bulk = 0.12f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult,
									RaceKey = "veryDarkGreen"
								}
							}
						},
						new SpawnEntityAction("99875afta3w5cc0-30ae-4f81-82fe-06cdc739ecfe")
						{
							EntityData = new EntityData
							{
								EntityKey = "entity:binalRat",
								Name = "BinalRat(1296,1344)",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "binalRatAllegiance#2"
								},
								Location = new Vector3(1296f, 1344f, 0f),
								Bulk = 0.21f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						},
						new SpawnEntityAction("99safa32tg-30ae-4f81-82fe-06cdc739ecfe")
						{
							EntityData = new EntityData
							{
								EntityKey = "entity:binalRat",
								Name = "BinalRat(480,816)",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "binalRatAllegiance#2"
								},
								Location = new Vector3(480f, 816f, 0f),
								Bulk = 0.21f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						},
						new SpawnEntityAction("9agdfsawf0ae-4f81-82fe-06cdc739ecfe")
						{
							EntityData = new EntityData
							{
								EntityKey = "entity:binalRat",
								Name = "BinalRat(2448,96)",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "binalRatAllegiance#2"
								},
								Location = new Vector3(2448f, 96f, 0f),
								Bulk = 0.21f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						},
						new SpawnEntityAction("99875awf2a5ag-4f81-82fe-06cdc739ecfe")
						{
							EntityData = new EntityData
							{
								EntityKey = "entity:binalRat",
								Name = "BinalRat(5856,528)",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "binalRatAllegiance#1"
								},
								Location = new Vector3(5856f, 528f, 0f),
								Bulk = 0.21f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						},
						new SpawnEntityAction("99facdvd-30ae-4f81-82fe-06cdc739ecfe")
						{
							EntityData = new EntityData
							{
								EntityKey = "entity:binalRat",
								Name = "BinalRat(2304,1680)",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "binalRatAllegiance#1"
								},
								Location = new Vector3(2304f, 1680f, 0f),
								Bulk = 0.21f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						},
						new SpawnEntityAction("998wasfsfgc0-30ae-4f81-82fe-06cdc739ecfe")
						{
							EntityData = new EntityData
							{
								EntityKey = "entity:binalRat",
								Name = "BinalRat(3072,3312)",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "binalRatAllegiance#1"
								},
								Location = new Vector3(3072f, 3312f, 0f),
								Bulk = 0.21f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						},
						new SpawnEntityAction("998zxctatg-30ae-4f81-82fe-06cdc739ecfe")
						{
							EntityData = new EntityData
							{
								EntityKey = "entity:binalRat",
								Name = "BinalRat(384,2592)",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "binalRatAllegiance#1"
								},
								Location = new Vector3(384f, 2592f, 0f),
								Bulk = 0.21f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						},
						new SpawnEntityAction("f39d3dff8fwaf242413-9477-1372b18710fxdr98")
						{
							DelayInSeconds = 0.1,
							EntityData = new EntityData
							{
								EntityKey = "entity:binalRat",
								Name = "BinalRat1(6048,4800)",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "binalRatAllegiance#3"
								},
								Location = new Vector3(6048f, 4800f, 0f),
								Bulk = 0.28f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						},
						new SpawnEntityAction("f608b57c-0da242447-442d-ab93-c57fbsdrsr7f395de")
						{
							DelayInSeconds = 0.1,
							EntityData = new EntityData
							{
								EntityKey = "entity:binalRat",
								Name = "BinalRat(5280,5040)",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "binalRatAllegiance#3"
								},
								Location = new Vector3(5280f, 5040f, 0f),
								Bulk = 0.28f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						},
						new SpawnEntityAction("d913ba37-serhdc30-4fwa24242-91dd-aea79dd0a242")
						{
							DelayInSeconds = 0.1,
							EntityData = new EntityData
							{
								EntityKey = "entity:binalRat",
								Name = "BinalRat(4704,6000)",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "binalRatAllegiance#3"
								},
								Location = new Vector3(4704f, 6000f, 0f),
								Bulk = 0.28f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						},
						new SpawnEntityAction("99wasftayhyagc0-30ae-4f81-82fe-06cdc739ecfe")
						{
							EntityData = new EntityData
							{
								EntityKey = "entity:fieldQuadite",
								Name = "leafcutter(1296,1344)",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "leafcutterAllegiance#1"
								},
								Location = new Vector3(1728f, 1008f, 0f),
								Bulk = 0.2f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						},
						new SpawnEntityAction("99875c25732756dighyshbea739ecfe")
						{
							EntityData = new EntityData
							{
								EntityKey = "entity:fieldQuadite",
								Name = "leafcutter(3072,3312)",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "leafcutterAllegiance#3"
								},
								Location = new Vector3(2496f, 288f, 0f),
								Bulk = 0.21f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						},
						new SpawnEntityAction("998afs3663eaae-4f81-82fe-06cdc739ecfe")
						{
							EntityData = new EntityData
							{
								EntityKey = "entity:fieldQuadite",
								Name = "leafcutter(384,2592)",
								MemberOf = new AllegianceAndExpedition
								{
									AllegianceKey = "leafcutterAllegiance#3"
								},
								Location = new Vector3(432f, 912f, 0f),
								Bulk = 0.21f,
								BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
								{
									AgeGroup = AIAgeGroup.Adult
								}
							}
						}
					}
				}
			}
		};
		PolledEventType item = polledEventType;
		list.Add(item);
		list.Add(new PolledEventType
		{
			KeyName = "SANDBOXNOMADMAP_timedSpawnBeginningPopulationBushdragonsNormal",
			ActionSets = new ActionSets
			{
				SetsOfActions = new ActionSetType[1]
				{
					new ActionSetType("0a4f34376wrtuywtruywryuhwrhwrh26")
					{
						Actions = new EventActionType[5]
						{
							new SpawnEntityAction("b7a69ed1-8224-414f-b386-35574974654a")
							{
								DelayInSeconds = 0.1,
								EntityData = new EntityData
								{
									EntityKey = "entity:bushDragon",
									Name = "BushDragon1",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "bushDragonAllegianceSouth"
									},
									Location = new Vector3(4871f, 5609f, 0f),
									Bulk = 1f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult
									}
								}
							},
							new SpawnEntityAction("d156ad0faw2424-4dd7-af64-7e1688e699ff")
							{
								DelayInSeconds = 0.1,
								EntityData = new EntityData
								{
									EntityKey = "entity:bushDragon",
									Name = "BushDragon2",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "bushDragonAllegianceSouth"
									},
									Location = new Vector3(5022f, 5542f, 0f),
									Bulk = 1f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult
									}
								}
							},
							new SpawnEntityAction("d156ada242rwafegtefdsx7-af64-7e1688e699ff")
							{
								DelayInSeconds = 0.1,
								EntityData = new EntityData
								{
									EntityKey = "entity:bushDragon",
									Name = "BushDragon2",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "bushDragonAllegianceSouth"
									},
									Location = new Vector3(5322f, 5592f, 0f),
									Bulk = 1f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult
									}
								}
							},
							new SpawnEntityAction("e03da83f-a3c9-453f-b9fd-55b8831ba420")
							{
								DelayInSeconds = 0.1,
								EntityData = new EntityData
								{
									EntityKey = "entity:bushDragon",
									Name = "BushDragon3",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "bushDragonAllegianceSouth"
									},
									Location = new Vector3(4849f, 5152f, 0f),
									Bulk = 1f,
									BioEntity = new UWGame.SimSide.Maps.MapEditor.BiologicalEntity
									{
										AgeGroup = AIAgeGroup.Adult
									}
								}
							},
							new SpawnEntityAction("166d7be1-c292-4d74-9fde-da2a7b7afbbb")
							{
								DelayInSeconds = 0.1,
								EntityData = new EntityData
								{
									EntityKey = "entity:bushDragon",
									Name = "BushDragon4",
									MemberOf = new AllegianceAndExpedition
									{
										AllegianceKey = "bushDragonAllegianceSouth"
									},
									Location = new Vector3(5520f, 5133f, 0f),
									Bulk = 1f,
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
			KeyName = "SANDBOXNOMADMAP_continualSpawnBushDragonSouth",
			PollInterval = new ValueNode
			{
				PropertyKey = "bushDragonSpawnInterval"
			},
			StartAfterInterval = true,
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
							ConstantStringEqual = "bushDragonAllegianceSouth"
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
						PropertyKey = "maxBushDragons"
					}
				}
			},
			ActionSetsKey = "continualSpawnBushDragonSouth"
		});
		list.Add(new PolledEventType
		{
			KeyName = "SANDBOXNOMADMAP_continualSpawnTwinklerNorth",
			PollInterval = new ValueNode
			{
				PropertyKey = "twinklerSpawnInterval"
			},
			StartAfterInterval = true,
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
							ConstantStringEqual = "twinklerAllegianceNorth"
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
			ActionSetsKey = "continualSpawnTwinklerNorth"
		});
		list.Add(new PolledEventType
		{
			KeyName = "SANDBOXNOMADMAP_continualSpawnTurnipsNorth",
			PollInterval = new ValueNode
			{
				PropertyKey = "turnipSpawnInterval"
			},
			StartAfterInterval = true,
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
							ConstantStringEqual = "turnipAllegianceNorth"
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
						PropertyKey = "maxTurnips"
					}
				}
			},
			ActionSetsKey = "continualSpawnTurnipNorth"
		});
		list.Add(new PolledEventType
		{
			KeyName = "SANDBOXNOMADMAP_continualSpawnBinalRats#3",
			PollInterval = new ValueNode
			{
				PropertyKey = "binalRatSpawnInterval"
			},
			StartAfterInterval = true,
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
							ConstantStringEqual = "binalRatAllegiance#3"
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
						PropertyKey = "maxBinalRats"
					}
				}
			},
			ActionSetsKey = "continualSpawnBinalRats#3"
		});
		list.Add(new PolledEventType
		{
			KeyName = "SANDBOXNOMADMAP_continualSpawnBinalRats#2",
			PollInterval = new ValueNode
			{
				PropertyKey = "binalRatSpawnInterval"
			},
			StartAfterInterval = true,
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
							ConstantStringEqual = "binalRatAllegiance#2"
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
						PropertyKey = "maxBinalRats"
					}
				}
			},
			ActionSetsKey = "continualSpawnBinalRats#2"
		});
		list.Add(new PolledEventType
		{
			KeyName = "SANDBOXNOMADMAP_continualSpawnBinalRats#1",
			PollInterval = new ValueNode
			{
				PropertyKey = "binalRatSpawnInterval"
			},
			StartAfterInterval = true,
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
							ConstantStringEqual = "binalRatAllegiance#1"
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
						PropertyKey = "maxBinalRats"
					}
				}
			},
			ActionSetsKey = "continualSpawnBinalRats#1"
		});
		list.Add(new PolledEventType
		{
			KeyName = "SANDBOXNOMADMAP_continualSpawnBinalRatsSwamp",
			PollInterval = new ValueNode
			{
				PropertyKey = "binalRatSpawnInterval"
			},
			StartAfterInterval = true,
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
							ConstantStringEqual = "binalRatAllegianceSwamp"
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
						PropertyKey = "maxBinalRats"
					}
				}
			},
			ActionSetsKey = "continualSpawnBinalRatsSwamp"
		});
		list.Add(new PolledEventType
		{
			KeyName = "SANDBOXNOMADMAP_continualSpawnBinalRatsSwamp2",
			PollInterval = new ValueNode
			{
				PropertyKey = "binalRatSpawnInterval"
			},
			StartAfterInterval = true,
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
							ConstantStringEqual = "binalRatAllegianceSwamp"
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
						PropertyKey = "maxBinalRats"
					}
				}
			},
			ActionSetsKey = "continualSpawnBinalRatsSwamp2"
		});
		list.Add(new PolledEventType
		{
			KeyName = "SANDBOXNOMADMAP_continualSpawnThunderChickens#1",
			PollInterval = new ValueNode
			{
				PropertyKey = "thunderChickenSpawnInterval"
			},
			StartAfterInterval = true,
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
							ConstantStringEqual = "thunderChickenAllegiance#1"
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
						PropertyKey = "maxThunderChickens"
					}
				}
			},
			ActionSetsKey = "continualSpawnThunderChicken#1"
		});
		list.Add(new PolledEventType
		{
			KeyName = "SANDBOXNOMADMAP_continualSpawnThunderChickens#2",
			PollInterval = new ValueNode
			{
				PropertyKey = "thunderChickenSpawnInterval"
			},
			StartAfterInterval = true,
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
							ConstantStringEqual = "thunderChickenAllegiance#2"
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
						PropertyKey = "maxThunderChickens"
					}
				}
			},
			ActionSetsKey = "continualSpawnThunderChicken#2"
		});
		list.Add(new PolledEventType
		{
			KeyName = "SANDBOXNOMADMAP_continualSpawnThunderChickens#3",
			PollInterval = new ValueNode
			{
				PropertyKey = "thunderChickenSpawnInterval"
			},
			StartAfterInterval = true,
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
							ConstantStringEqual = "thunderChickenAllegiance#3"
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
						PropertyKey = "maxThunderChickens"
					}
				}
			},
			ActionSetsKey = "continualSpawnThunderChicken#3"
		});
		list.Add(new PolledEventType
		{
			KeyName = "SANDBOXNOMADMAP_continualSpawnSnatcher",
			PollInterval = new ValueNode
			{
				PropertyKey = "snatcherSpawnInterval"
			},
			StartAfterInterval = true,
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
							ConstantStringEqual = "snatcherAllegiance#1"
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
						PropertyKey = "maxSnatchers"
					}
				}
			},
			ActionSetsKey = "continualSpawnSnatchers#1"
		});
		list.Add(new PolledEventType
		{
			KeyName = "SANDBOXNOMADMAP_continualSpawnSnatcher#2",
			PollInterval = new ValueNode
			{
				PropertyKey = "snatcherSpawnInterval"
			},
			StartAfterInterval = true,
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
							ConstantStringEqual = "snatcherAllegiance#2"
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
						Int = 0
					}
				}
			},
			ActionSetsKey = "continualSpawnSnatchers#2"
		});
		list.Add(new PolledEventType
		{
			KeyName = "SANDBOXNOMADMAP_continualSpawnDemonTree#1",
			PollInterval = new ValueNode
			{
				PropertyKey = "demonTreeSpawnInterval"
			},
			StartAfterInterval = true,
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
							ConstantStringEqual = "demonTreeAllegiance#1"
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
						PropertyKey = "maxDemonTree"
					}
				}
			},
			ActionSetsKey = "continualdemonTree#1"
		});
		list.Add(new PolledEventType
		{
			KeyName = "SANDBOXNOMADMAP_continualSpawnDemonTree#2",
			PollInterval = new ValueNode
			{
				PropertyKey = "demonTreeSpawnInterval"
			},
			StartAfterInterval = true,
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
							ConstantStringEqual = "demonTreeAllegiance#2"
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
						PropertyKey = "maxDemonTree"
					}
				}
			},
			ActionSetsKey = "continualdemonTree#2"
		});
		list.Add(new PolledEventType
		{
			KeyName = "SANDBOXNOMADMAP_continualSpawnDemonTree#3",
			PollInterval = new ValueNode
			{
				PropertyKey = "demonTreeSpawnInterval"
			},
			StartAfterInterval = true,
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
							ConstantStringEqual = "demonTreeAllegiance#3"
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
						PropertyKey = "maxDemonTree"
					}
				}
			},
			ActionSetsKey = "continualdemonTree#3"
		});
		list.Add(new PolledEventType
		{
			KeyName = "SANDBOXNOMADMAP_continualSpawnSwampDemonTree#1",
			PollInterval = new ValueNode
			{
				PropertyKey = "demonTreeSpawnInterval"
			},
			StartAfterInterval = true,
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
							ConstantStringEqual = "swampDemonTreeAllegiance#1"
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
						PropertyKey = "maxDemonTree"
					}
				}
			},
			ActionSetsKey = "continualSwampDemonTree#1"
		});
		list.Add(new PolledEventType
		{
			KeyName = "SANDBOXNOMADMAP_continualSpawnSwampDemonTree#2",
			PollInterval = new ValueNode
			{
				PropertyKey = "demonTreeSpawnInterval"
			},
			StartAfterInterval = true,
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
							ConstantStringEqual = "swampDemonTreeAllegiance#2"
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
						PropertyKey = "maxDemonTree"
					}
				}
			},
			ActionSetsKey = "continualSwampDemonTree#2"
		});
		list.Add(new PolledEventType
		{
			KeyName = "SANDBOXNOMADMAP_continualSpawnSlugs",
			PollInterval = new ValueNode
			{
				PropertyKey = "slugSpawnInterval"
			},
			StartAfterInterval = true,
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
							ConstantStringEqual = "slugAllegiance#1"
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
						PropertyKey = "maxSlugs"
					}
				}
			},
			ActionSetsKey = "continualSpawnSlugs"
		});
		list.Add(new PolledEventType
		{
			KeyName = "SANDBOXNOMADMAP_continualSpawnLeafcutter#1",
			PollInterval = new ValueNode
			{
				PropertyKey = "leafcutterSpawnInterval"
			},
			StartAfterInterval = true,
			Condition = new ConditionFunction
			{
				Left = new CustomCondition
				{
					TargetObject = new TargetObject
					{
						GetList = new GetList
						{
							HasPropertiesListKey = "allegiances",
							FilterCondition = new PropertyCondition
							{
								PropertyKey = "keyName",
								ConstantStringEqual = "leafcutterAllegiance#1"
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
							PropertyKey = "maxLeafcutters"
						}
					}
				},
				Operator = OperatorType.And,
				Right = new CustomCondition
				{
					TargetObject = new TargetObject
					{
						TargetObjectType = TargetObjectType.Root
					},
					PropertyCondition = new PropertyCondition
					{
						PropertyKey = "nest1",
						BoolValue = true
					}
				}
			},
			ActionSetsKey = "continualSpawnLeafcutter#1"
		});
		list.Add(new PolledEventType
		{
			KeyName = "SANDBOXNOMADMAP_continualSpawnLeafcutter#2",
			PollInterval = new ValueNode
			{
				PropertyKey = "leafcutterSpawnInterval"
			},
			StartAfterInterval = true,
			Condition = new ConditionFunction
			{
				Left = new CustomCondition
				{
					TargetObject = new TargetObject
					{
						GetList = new GetList
						{
							HasPropertiesListKey = "allegiances",
							FilterCondition = new PropertyCondition
							{
								PropertyKey = "keyName",
								ConstantStringEqual = "leafcutterAllegiance#2"
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
							PropertyKey = "maxLeafcutters"
						}
					}
				},
				Operator = OperatorType.And,
				Right = new CustomCondition
				{
					TargetObject = new TargetObject
					{
						TargetObjectType = TargetObjectType.Root
					},
					PropertyCondition = new PropertyCondition
					{
						PropertyKey = "nest2",
						BoolValue = true
					}
				}
			},
			ActionSetsKey = "continualSpawnLeafcutter#2"
		});
		list.Add(new PolledEventType
		{
			KeyName = "SANDBOXNOMADMAP_continualSpawnLeafcutter#3",
			PollInterval = new ValueNode
			{
				PropertyKey = "leafcutterSpawnInterval"
			},
			StartAfterInterval = true,
			Condition = new ConditionFunction
			{
				Left = new CustomCondition
				{
					TargetObject = new TargetObject
					{
						GetList = new GetList
						{
							HasPropertiesListKey = "allegiances",
							FilterCondition = new PropertyCondition
							{
								PropertyKey = "keyName",
								ConstantStringEqual = "leafcutterAllegiance#3"
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
							PropertyKey = "maxLeafcutters"
						}
					}
				},
				Operator = OperatorType.And,
				Right = new CustomCondition
				{
					TargetObject = new TargetObject
					{
						TargetObjectType = TargetObjectType.Root
					},
					PropertyCondition = new PropertyCondition
					{
						PropertyKey = "nest3",
						BoolValue = true
					}
				}
			},
			ActionSetsKey = "continualSpawnLeafcutter#3"
		});
		list.Add(new PolledEventType
		{
			KeyName = "SANDBOXNOMADMAP_continualSpawnLeafcutter#4",
			PollInterval = new ValueNode
			{
				PropertyKey = "leafcutterSpawnInterval"
			},
			StartAfterInterval = true,
			Condition = new ConditionFunction
			{
				Left = new CustomCondition
				{
					TargetObject = new TargetObject
					{
						GetList = new GetList
						{
							HasPropertiesListKey = "allegiances",
							FilterCondition = new PropertyCondition
							{
								PropertyKey = "keyName",
								ConstantStringEqual = "leafcutterAllegiance#4"
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
							PropertyKey = "maxLeafcutters"
						}
					}
				},
				Operator = OperatorType.And,
				Right = new CustomCondition
				{
					TargetObject = new TargetObject
					{
						TargetObjectType = TargetObjectType.Root
					},
					PropertyCondition = new PropertyCondition
					{
						PropertyKey = "nest4",
						BoolValue = true
					}
				}
			},
			ActionSetsKey = "continualSpawnLeafcutter#4"
		});
		list.Add(new PolledEventType
		{
			KeyName = "SANDBOXNOMADMAP_continualSpawnLeafcutter#5",
			PollInterval = new ValueNode
			{
				PropertyKey = "leafcutterSpawnInterval"
			},
			StartAfterInterval = true,
			Condition = new ConditionFunction
			{
				Left = new CustomCondition
				{
					TargetObject = new TargetObject
					{
						GetList = new GetList
						{
							HasPropertiesListKey = "allegiances",
							FilterCondition = new PropertyCondition
							{
								PropertyKey = "keyName",
								ConstantStringEqual = "leafcutterAllegiance#5"
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
							PropertyKey = "maxLeafcutters"
						}
					}
				},
				Operator = OperatorType.And,
				Right = new CustomCondition
				{
					TargetObject = new TargetObject
					{
						TargetObjectType = TargetObjectType.Root
					},
					PropertyCondition = new PropertyCondition
					{
						PropertyKey = "nest5",
						BoolValue = true
					}
				}
			},
			ActionSetsKey = "continualSpawnLeafcutter#5"
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
							new SetPropertyAction("4e8dghjdgjdaa25taged566fwaw2266665gjdgjdg503c7")
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
							new SetPropertyAction("4e8dghjdfaw25252jd56666665gjdgjdg503c7")
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
							new SetPropertyAction("4e8dghjdgjdghzjd56666665gjdgjafawf2542dg503c7")
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
							new SetPropertyAction("4e8dghjdgjdghzjd56666665gafawf2aw5242ajdgjdg503c7")
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
							new SetPropertyAction("4e8dghjdgfwa2525a2jdghzjd56666665gjdgjdg503c7")
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
