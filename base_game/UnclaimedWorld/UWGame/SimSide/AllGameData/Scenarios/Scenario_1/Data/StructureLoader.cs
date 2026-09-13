using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Collisions;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_1.Data;

internal class StructureLoader
{
	public static void Init(List<EntityType> listOfEntityTypes)
	{
		listOfEntityTypes.Add(new EntityType("structure:skimmerHull")
		{
			Name = "Aircraft wreck (hull)",
			ThumbnailSmall = "HUD_thumbnail_skimmerHull",
			SummaryDescription = "Heavily damaged fuselage of a Skimmer aircraft",
			Description = "//MODEL//\n SK-140 'Skimmer' aircraft deployed in the PRECOL mission for planet exploration. Designed for carrying equipment and personnel on research trips.\n \n SPEED: 360 km/h\n RANGE: 450 km\n PAYLOAD:400 kg\n PROPULSION:\n Ducted-fan tiltrotors\n Superconducting power cells\n --------------------------------\n //DAMAGE ASSESSMENT//\n Aircraft is non-functional after crash-landing. Cause of crash: Failure of rotor #1 due to impact with attacking creatures (quadites) during take-off. At crash-landing, fuselage suffered additional damage as did the navigation and communication instruments\n REPAIRABILITY: We lack the necessary tools to restore any of the aircraft functions to operating condition",
			CategoryKey = "miscellaneous",
			StructureType = new StructureType
			{
				BuildByPlayer = false
			},
			ContainerType = new StorageContainerType("isolated", 8f)
			{
				CanTransactWithTags = new string[1] { "humanTransact" }
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "skimmerHull",
							BaseCenter = new Vector2(0f, 0f),
							Offset = new Vector2(0f, 0f)
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "skimmerHull_g"
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 10f,
					PadShape = CollidePrim.Circle,
					Shapes = new CollideShape2D[2]
					{
						new CollideShape2D(new Vector2(0f, 0f), 25f)
						{
							Offset = new Vector2(4f, -6f)
						},
						new CollideShape2D(new Vector2(0f, 0f), 26f)
						{
							Offset = new Vector2(44f, -22f)
						}
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "equipment",
				SalvageProcess = "salvageSkimmerHull",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:scrapMetal", 3 },
					{ "item:seatCushions", 3 },
					{ "item:inactivatedFoodCoolerUnit", 1 },
					{ "item:textile", 1 }
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:skimmerTail")
		{
			Name = "Aircraft wreck (tail)",
			SummaryDescription = "Broken off tail section of a Skimmer aircraft",
			Description = "//MODEL//\n SK-140 'Skimmer' aircraft deployed in the PRECOL mission for planet exploration. Designed for carrying equipment and personnel on research trips.\n \n SPEED: 360 km/h\n RANGE: 450 km\n PAYLOAD:400 kg\n PROPULSION:\n Ducted-fan tiltrotors\n Superconducting power cells\n --------------------------------\n //DAMAGE ASSESSMENT//\n Aircraft is non-functional after crash-landing. Cause of crash: Failure of rotor #1 due to impact with attacking creatures (quadites) during take-off. At crash-landing, fuselage suffered additional damage as did the navigation and communication instruments\n REPAIRABILITY: We lack the necessary tools to restore any of the aircraft functions to operating condition",
			ThumbnailSmall = "HUD_thumbnail_skimmerTail",
			CategoryKey = "miscellaneous",
			StructureType = new StructureType(),
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "skimmerTail"
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "skimmerTail_g"
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 12f,
					PadShape = CollidePrim.Circle,
					Shapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(2f, -1f), 25f)
						{
							Offset = new Vector2(-4f, 8f)
						}
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "equipment",
				SalvageProcess = "salvageSkimmerTail",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:scrapMetal", 2 },
					{ "item:panelScraps", 1 }
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:skimmerEngineSide")
		{
			Name = "Aircraft wreck (rotor #2)",
			SummaryDescription = "Damaged rotor of the Skimmer aircraft",
			Description = "//MODEL//\n SK-140 'Skimmer' aircraft deployed in the PRECOL mission for planet exploration. Designed for carrying equipment and personnel on research trips.\n \n SPEED: 360 km/h\n RANGE: 450 km\n PAYLOAD:400 kg\n PROPULSION:\n Ducted-fan tiltrotors\n Superconducting power cells\n --------------------------------\n //DAMAGE ASSESSMENT//\n Aircraft is non-functional after crash-landing. Cause of crash: Failure of rotor #1 due to impact with attacking creatures (quadites) during take-off. At crash-landing, fuselage suffered additional damage as did the navigation and communication instruments\n REPAIRABILITY: We lack the necessary tools to restore any of the aircraft functions to operating condition",
			ThumbnailSmall = "HUD_thumbnail_skimmerEngine",
			CategoryKey = "miscellaneous",
			StructureType = new StructureType
			{
				BuildByPlayer = false
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "skimmerEngineSideBuried"
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "skimmerEngineSideBuried_g"
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 14f,
					PadShape = CollidePrim.Circle,
					Shapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(2f, -1f), 20f)
						{
							Offset = new Vector2(0f, -6f)
						}
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "equipment",
				SalvageProcess = "salvageSkimmerEngineSide",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:scrapMetal", 1 },
					{ "item:propellerDome", 1 },
					{ "item:superconductingWire", 2 }
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:skimmerEngineTop")
		{
			Name = "Aircraft wreck (rotor #1)",
			SummaryDescription = "Damaged rotor of the Skimmer aircraft",
			Description = "//MODEL//\n SK-140 'Skimmer' aircraft deployed in the PRECOL mission for planet exploration. Designed for carrying equipment and personnel on research trips.\n \n SPEED: 360 km/h\n RANGE: 450 km\n PAYLOAD:400 kg\n PROPULSION:\n Ducted-fan tiltrotors\n Superconducting power cells\n --------------------------------\n //DAMAGE ASSESSMENT//\n Aircraft is non-functional after crash-landing. Cause of crash: Failure of rotor #1 due to impact with attacking creatures (quadites) during take-off. At crash-landing, fuselage suffered additional damage as did the navigation and communication instruments\n REPAIRABILITY: We lack the necessary tools to restore any of the aircraft functions to operating condition",
			ThumbnailSmall = "HUD_thumbnail_skimmerEngine",
			CategoryKey = "miscellaneous",
			StructureType = new StructureType
			{
				BuildByPlayer = false
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "skimmerEngineTop"
						}
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 20f,
					PadShape = CollidePrim.Circle,
					Shapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(2f, -1f), 20f)
						{
							Offset = new Vector2(0f, -6f)
						}
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "equipment",
				SalvageProcess = "salvageSkimmerEngineTop",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:scrapMetal", 1 },
					{ "item:propellerDome", 1 },
					{ "item:superconductingWire", 2 }
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:signalPyre")
		{
			DeleteRecord = true
		});
	}
}
