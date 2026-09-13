using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.Client.Particles;
using UWGame.ClientSide;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Collisions;
using UWGame.SimSide.Communication;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.SimSide.GatheringSites;
using UWGame.SimSide.Policies;
using UWGame.SimSide.Systems.Triggers;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.AllGameData;

public class StructureLoader
{
	public const float padRadiusShelterAndStorage = 10f;

	public static void Init(List<EntityType> listOfEntityTypes)
	{
		float comfortLevel = 0.07f;
		float comfortLevel2 = 0.09f;
		float comfortLevel3 = 0.11f;
		float comfortLevel4 = 0.13f;
		float comfortLevel5 = 0.2f;
		float comfortLevel6 = 0.3f;
		listOfEntityTypes.Add(new EntityType("structure:daysheenTipi")
		{
			Name = "Daysheen Tipi",
			SummaryDescription = "A temporary 1-person shelter, quick to construct.",
			Description = "We came up with this design inspired by the shape and properties of the daysheen leaves. Has room for at least one person",
			ThumbnailSmall = "HUD_thumbnail_daysheenTipi",
			CategoryKey = "shelter",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Comfort
			},
			ContainerType = new HomeContainerType
			{
				CanBeEnteredByTags = new string[2] { "humanTransact", "leafcutterTransact" },
				ResidenceType = new ResidenceType
				{
					LivingCapacity = 1,
					ComfortLevel = comfortLevel
				},
				StorageTags = new string[2] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" },
				ItemStorageType = new ItemStorageType("isolated", 8f),
				DefaultStorageSettings = "homeStorage",
				HasRallyPointInCourtyard = false,
				UpgradesProfile = "survivalHome1People",
				Doors = new Vector2[1]
				{
					new Vector2(8f, 7f)
				}
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "daysheenTipi",
							BaseCenter = new Vector2(36f, 29f)
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "daysheenTipi_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[2]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "daysheenTipi"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "daysheenTipi_construct"
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "daysheenTipi_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
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
						new CollideShape2D(new Vector2(0f, 0f), 9f)
						{
							Offset = new Vector2(4f, -9f)
						},
						new CollideShape2D(new Vector2(0f, 0f), 12f)
						{
							Offset = new Vector2(-12f, 0f)
						}
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "ricketyConstruction",
				SalvageProcess = "salvageDaysheenTipi",
				PartKeys = new SerializableDictionary<string, int> { { "item:daysheenLeaves", 2 } },
				Repair = "buildingRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:lean-toTarp")
		{
			Name = "Lean-to (Tarp)",
			SummaryDescription = "Simple 2-person shelter with a sloping roof, covered with a thermal tarp.",
			Description = "This design requires a number of long, straight sticks and poles. The thermal tarp is used as covering to keep out rain and wind and ensures stable temperature.",
			ThumbnailSmall = "HUD_thumbnail_leanToBigTarp",
			CategoryKey = "shelter",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Comfort
			},
			ContainerType = new HomeContainerType
			{
				CanBeEnteredByTags = new string[2] { "humanTransact", "leafcutterTransact" },
				ResidenceType = new ResidenceType
				{
					LivingCapacity = 2,
					ComfortLevel = comfortLevel
				},
				StorageTags = new string[2] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" },
				ItemStorageType = new ItemStorageType("isolated", 8f),
				DefaultStorageSettings = "homeStorage",
				HasRallyPointInCourtyard = false,
				UpgradesProfile = "survivalHome2People",
				Doors = new Vector2[1]
				{
					new Vector2(-28f, -6f)
				}
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "leanToBigTarp",
							BaseCenter = new Vector2(36f, 29f)
						}
					}
				},
				ClientStateConditions = new ClientStateInfo[2]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "leanToBigTarp"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "leanToBigTarp_construct"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					CausesCollisions = true,
					Pad = 10f,
					PadShape = CollidePrim.Circle,
					Shapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, 0f), 28f)
						{
							Offset = new Vector2(-4f, 6f)
						}
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "ricketyConstruction",
				SalvageProcess = "salvageLean-toTarp",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:sticks", 6 },
					{ "item:thermalTarp", 1 }
				},
				Repair = "buildingRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:lean-toSpoakLeaves")
		{
			Name = "Lean-to (Spoak leaves)",
			SummaryDescription = "Simple 2- person shelter with a sloping roof, covered with spoak leaves",
			Description = "This design requires a number of long, straight sticks. The structure uses the waterproof spoak leaf to provide basic protection from rain and wind and wingweed leaves for insolation. However, the leaves are prone to infestation by the scuttler bug that can end up consuming them.",
			ThumbnailSmall = "HUD_thumbnail_leanToBigSpoak",
			CategoryKey = "shelter",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Comfort
			},
			ContainerType = new HomeContainerType
			{
				CanBeEnteredByTags = new string[2] { "humanTransact", "leafcutterTransact" },
				ResidenceType = new ResidenceType
				{
					LivingCapacity = 2,
					ComfortLevel = comfortLevel
				},
				StorageTags = new string[2] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" },
				ItemStorageType = new ItemStorageType("isolated", 8f),
				DefaultStorageSettings = "homeStorage",
				HasRallyPointInCourtyard = false,
				UpgradesProfile = "survivalHome2People",
				Doors = new Vector2[1]
				{
					new Vector2(-25f, 2f)
				}
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "leanToBigSpoak",
							BaseCenter = new Vector2(36f, 29f)
						}
					}
				},
				ClientStateConditions = new ClientStateInfo[2]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "leanToBigSpoak"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "leanToBigSpoak_construct"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 10f,
					PadShape = CollidePrim.Circle,
					Shapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, 0f), 18f)
						{
							Offset = new Vector2(-4f, 6f)
						}
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "ricketyConstruction",
				SalvageProcess = "salvageLean-toSpoakLeaves",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:sticks", 4 },
					{ "item:wingweedLeaves", 2 },
					{ "item:spoakLeaves", 1 }
				},
				Repair = "buildingRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:lean-toScraps")
		{
			Name = "Lean-to (Scraps)",
			SummaryDescription = "An improvised 2-person shelter made from scrap thermoplastics panels.",
			Description = "This building will probably not last for very long, but offers temporary shelter for 2 people.",
			ThumbnailSmall = "HUD_thumbnail_leanToSmallScrap",
			CategoryKey = "shelter",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Comfort
			},
			ContainerType = new HomeContainerType
			{
				CanBeEnteredByTags = new string[2] { "humanTransact", "leafcutterTransact" },
				ResidenceType = new ResidenceType
				{
					LivingCapacity = 2,
					ComfortLevel = comfortLevel
				},
				StorageTags = new string[2] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" },
				ItemStorageType = new ItemStorageType("isolated", 8f),
				DefaultStorageSettings = "homeStorage",
				HasRallyPointInCourtyard = false,
				UpgradesProfile = "survivalHome2People",
				Doors = new Vector2[1]
				{
					new Vector2(21f, 4f)
				}
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "leanToSmallScrap",
							BaseCenter = new Vector2(36f, 29f)
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "leanToSmallScrap_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[2]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "leanToSmallScrap"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "leanToSmallScrap_construct_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 10f,
					PadShape = CollidePrim.Circle,
					Shapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, 0f), 18f)
						{
							Offset = new Vector2(0f, -6f)
						}
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "ricketyConstruction",
				SalvageProcess = "salvageLean-toScraps",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:sticks", 6 },
					{ "item:panelScraps", 1 }
				},
				Repair = "buildingRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:A-frameTarp")
		{
			Name = "A-frame (tarp)",
			SummaryDescription = "Simple 1-person shelter formed from a long backbone stick, covered with a thermal tarp.",
			Description = "Offers a small space for one person lying down. \n \nThe frame is made from a long pole which is rested against a couple of shorter sticks so that the entrance resembles an 'A'. Wingweed leaves are put in as bedding and the thermal tarp is draped on top.",
			ThumbnailSmall = "HUD_thumbnail_aFrameTarp",
			CategoryKey = "shelter",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Comfort
			},
			ContainerType = new HomeContainerType
			{
				CanBeEnteredByTags = new string[2] { "humanTransact", "leafcutterTransact" },
				ResidenceType = new ResidenceType
				{
					LivingCapacity = 1,
					ComfortLevel = comfortLevel
				},
				CanTransactWithTags = new string[1] { "humanTransact" },
				StorageTags = new string[2] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" },
				ItemStorageType = new ItemStorageType("isolated", 8f),
				DefaultStorageSettings = "homeStorage",
				HasRallyPointInCourtyard = false,
				UpgradesProfile = "survivalHome1People",
				Doors = new Vector2[1]
				{
					new Vector2(10f, 3f)
				}
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "aFrameTarp"
						}
					}
				},
				ClientStateConditions = new ClientStateInfo[2]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "aFrameTarp"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "aFrameTarp_construct"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
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
						new CollideShape2D(new Vector2(0f, 0f), 16f)
						{
							Offset = new Vector2(-2f, 3f)
						},
						new CollideShape2D(new Vector2(0f, 0f), 8f)
						{
							Offset = new Vector2(-18f, -9f)
						}
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "ricketyConstruction",
				SalvageProcess = "salvageA-frameTarp",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:sticks", 2 },
					{ "item:wingweedLeaves", 2 },
					{ "item:thermalTarp", 1 }
				},
				Repair = "buildingRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:A-frameSpoakLeaves")
		{
			Name = "A-frame (Spoak leaves)",
			SummaryDescription = "Simple 1-person shelter formed from a spoak branch, covered with leaves.",
			Description = "Offers a small space for one person lying down. \n \nThe frame is made from a branch which makes the entrance resemble an 'A'. Spoak leaves are placed to serve as a waterproof covering.",
			ThumbnailSmall = "HUD_thumbnail_aFrameSpoak",
			CategoryKey = "shelter",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Comfort
			},
			ContainerType = new HomeContainerType
			{
				CanBeEnteredByTags = new string[2] { "humanTransact", "leafcutterTransact" },
				ResidenceType = new ResidenceType
				{
					LivingCapacity = 1,
					ComfortLevel = comfortLevel
				},
				StorageTags = new string[2] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" },
				ItemStorageType = new ItemStorageType("isolated", 8f),
				DefaultStorageSettings = "homeStorage",
				HasRallyPointInCourtyard = false,
				UpgradesProfile = "survivalHome1People",
				Doors = new Vector2[1]
				{
					new Vector2(10f, 3f)
				}
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "aFrameSpoak"
						}
					}
				},
				ClientStateConditions = new ClientStateInfo[2]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "aFrameSpoak"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "aFrameSpoak_construct"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
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
						new CollideShape2D(new Vector2(0f, 0f), 16f)
						{
							Offset = new Vector2(-2f, 3f)
						},
						new CollideShape2D(new Vector2(0f, 0f), 8f)
						{
							Offset = new Vector2(-18f, -9f)
						}
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "ricketyConstruction",
				SalvageProcess = "salvageA-frameSpoakLeaves",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:spoakBranchesTrimmed", 1 },
					{ "item:spoakLeaves", 1 }
				},
				Repair = "buildingRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:A-frameScraps")
		{
			Name = "A-frame (Scraps)",
			SummaryDescription = "A crude 1-person shelter made from scrap plastic and cushions",
			Description = "Various scraps make this a somewhat comfortable place to sleep.",
			ThumbnailSmall = "HUD_thumbnail_imptent2",
			CategoryKey = "shelter",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Comfort
			},
			ContainerType = new HomeContainerType
			{
				CanBeEnteredByTags = new string[2] { "humanTransact", "leafcutterTransact" },
				ResidenceType = new ResidenceType
				{
					LivingCapacity = 1,
					ComfortLevel = comfortLevel
				},
				StorageTags = new string[2] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" },
				ItemStorageType = new ItemStorageType("isolated", 8f),
				DefaultStorageSettings = "homeStorage",
				HasRallyPointInCourtyard = false,
				UpgradesProfile = "survivalHome1People",
				Doors = new Vector2[1]
				{
					new Vector2(25f, -15f)
				}
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "imptent2",
							BaseCenter = new Vector2(36f, 29f)
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "imptent2_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[2]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "imptent2"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "imptent2_construct_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 10f,
					PadShape = CollidePrim.Circle,
					Shapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, 0f), 18f)
						{
							Offset = new Vector2(0f, 0f)
						}
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "ricketyConstruction",
				SalvageProcess = "salvageA-frameScraps",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:sticks", 4 },
					{ "item:seatCushions", 1 },
					{ "item:panelScraps", 1 }
				},
				Repair = "buildingRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:smallTent")
		{
			Name = "Small tent",
			SummaryDescription = "A small tent for 1 person.",
			Description = "The tent will offer good protection from rain and wind and is quite durable.",
			ThumbnailSmall = "HUD_thumbnail_smallTent",
			CategoryKey = "shelter",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Comfort
			},
			ContainerType = new HomeContainerType
			{
				CanBeEnteredByTags = new string[2] { "humanTransact", "leafcutterTransact" },
				ResidenceType = new ResidenceType
				{
					LivingCapacity = 1,
					ComfortLevel = comfortLevel2
				},
				CanTransactWithTags = new string[1] { "humanTransact" },
				StorageTags = new string[2] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" },
				ItemStorageType = new ItemStorageType("isolated", 8f),
				DefaultStorageSettings = "homeStorage",
				HasRallyPointInCourtyard = false,
				UpgradesProfile = "survivalHome1People",
				Doors = new Vector2[1]
				{
					new Vector2(-20f, 3f)
				}
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tent3",
							BaseCenter = new Vector2(36f, 29f)
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "tent3_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[3]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "tent3"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "tent3_construct_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "tent3"
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "tent3_g"
						},
						LightingTypes = new LightingType[1]
						{
							new LightingType
							{
								SpriteName = "common_25_tent3",
								Offset = new Point(-41, -30)
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 38)
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
						new CollideShape2D(new Vector2(0f, 0f), 13f)
						{
							Offset = new Vector2(10f, -3f)
						},
						new CollideShape2D(new Vector2(0f, 0f), 13f)
						{
							Offset = new Vector2(-7f, -1f)
						}
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "adequateConstruction",
				SalvageProcess = "salvageSmallTent",
				PartKeys = new SerializableDictionary<string, int> { { "item:smallTent", 1 } },
				Repair = "tentRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:octagonalTent")
		{
			Name = "Octagonal tent",
			SummaryDescription = "A comfortable tent for 2 people.",
			Description = "The tent will offer good protection from rain and wind and is quite durable.",
			ThumbnailSmall = "HUD_thumbnail_octagonalTent",
			CategoryKey = "shelter",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Comfort
			},
			ContainerType = new HomeContainerType
			{
				CanBeEnteredByTags = new string[2] { "humanTransact", "leafcutterTransact" },
				ResidenceType = new ResidenceType
				{
					LivingCapacity = 2,
					ComfortLevel = comfortLevel2
				},
				StorageTags = new string[2] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" },
				ItemStorageType = new ItemStorageType("isolated", 8f),
				DefaultStorageSettings = "homeStorage",
				UpgradesProfile = "survivalHome2People",
				HasRallyPointInCourtyard = false,
				Doors = new Vector2[1]
				{
					new Vector2(-19f, 10f)
				}
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tent2"
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "tent2_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[3]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "tent2"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "tent2_construct_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "tent2"
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "tent2_g"
						},
						LightingTypes = new LightingType[1]
						{
							new LightingType
							{
								SpriteName = "common_24_tent2",
								Offset = new Point(-34, -31)
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 38)
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 10f,
					PadShape = CollidePrim.Circle,
					Shapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, 0f), 21f)
						{
							Offset = new Vector2(1f, -3f)
						}
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "adequateConstruction",
				SalvageProcess = "salvageOctagonalTent",
				PartKeys = new SerializableDictionary<string, int> { { "item:octagonalTent", 1 } },
				Repair = "tentRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:domeTent")
		{
			Name = "Dome tent",
			SummaryDescription = "A comfortable tent for 2 people.",
			Description = "The tent will offer good protection from rain and wind and is quite durable.",
			ThumbnailSmall = "HUD_thumbnail_domeTent",
			CategoryKey = "shelter",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Comfort
			},
			ContainerType = new HomeContainerType
			{
				CanBeEnteredByTags = new string[2] { "humanTransact", "leafcutterTransact" },
				ResidenceType = new ResidenceType
				{
					LivingCapacity = 2,
					ComfortLevel = comfortLevel2
				},
				StorageTags = new string[2] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" },
				ItemStorageType = new ItemStorageType("isolated", 8f),
				DefaultStorageSettings = "homeStorage",
				UpgradesProfile = "survivalHome2People",
				HasRallyPointInCourtyard = false,
				Doors = new Vector2[1]
				{
					new Vector2(17f, 6f)
				}
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tent1",
							BaseCenter = new Vector2(48f, 55f)
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "tent1_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[3]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "tent1"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "tent1_construct_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "tent1",
								BaseCenter = new Vector2(48f, 55f)
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "tent1_g"
						},
						LightingTypes = new LightingType[1]
						{
							new LightingType
							{
								SpriteName = "common_23_tent1",
								Offset = new Point(-47, -52)
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 38)
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
						new CollideShape2D(new Vector2(0f, 0f), 26f)
						{
							Offset = new Vector2(0f, -3f)
						},
						new CollideShape2D(new Vector2(0f, 0f), 12f)
						{
							Offset = new Vector2(-22f, 15f)
						}
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "adequateConstruction",
				SalvageProcess = "salvageDomeTent",
				PartKeys = new SerializableDictionary<string, int> { { "item:domeTent", 1 } },
				Repair = "tentRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:domeShelterTarp")
		{
			Name = "Dome shelter (Tarp)",
			SummaryDescription = "A quite comfortable shelter for 2 people.",
			Description = "Based on a sturdy frame made from shadeleaf canes bent in arches and covered with a thermal tarp. The dome will offer good protection from rain and wind and should be reasonably durable.",
			ThumbnailSmall = "HUD_thumbnail_domeTarp",
			CategoryKey = "shelter",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Comfort
			},
			ContainerType = new HomeContainerType
			{
				CanBeEnteredByTags = new string[2] { "humanTransact", "leafcutterTransact" },
				ResidenceType = new ResidenceType
				{
					LivingCapacity = 2,
					ComfortLevel = comfortLevel2
				},
				StorageTags = new string[2] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" },
				ItemStorageType = new ItemStorageType("isolated", 8f),
				DefaultStorageSettings = "homeStorage",
				HasRallyPointInCourtyard = false,
				UpgradesProfile = "survivalHome2People",
				Doors = new Vector2[1]
				{
					new Vector2(15f, 5f)
				}
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "domeTarp",
							BaseCenter = new Vector2(53f, 56f)
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "domeTarp_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[2]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "domeTarp",
								BaseCenter = new Vector2(53f, 56f)
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "domeTarp_construct",
								BaseCenter = new Vector2(53f, 56f)
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 10f,
					PadShape = CollidePrim.Circle,
					Shapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, 0f), 25f)
						{
							Offset = new Vector2(-1f, -8f)
						}
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "adequateConstruction",
				SalvageProcess = "salvageDomeShelterTarp",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:shadeleafCanes", 4 },
					{ "item:thermalTarp", 1 }
				},
				Repair = "buildingRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:domeShelterSpoakShingles")
		{
			Name = "Dome shelter (Spoak shingles)",
			SummaryDescription = "A quite comfortable 2-person shelter, covered with durable shingles",
			Description = "Has a sufficiently sturdy frame made from shadeleaf canes bent in arches. The dome is covered with shingles made from spoak leaves, giving protection from the elements. The shingles are treated with a preservative which greatly reduces the risk of bug infestation.",
			ThumbnailSmall = "HUD_thumbnail_domeShingles",
			CategoryKey = "shelter",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Comfort
			},
			ContainerType = new HomeContainerType
			{
				CanBeEnteredByTags = new string[2] { "humanTransact", "leafcutterTransact" },
				ResidenceType = new ResidenceType
				{
					LivingCapacity = 2,
					ComfortLevel = comfortLevel3
				},
				StorageTags = new string[2] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" },
				ItemStorageType = new ItemStorageType("isolated", 8f),
				DefaultStorageSettings = "homeStorage",
				HasRallyPointInCourtyard = false,
				UpgradesProfile = "survivalHome2People",
				Doors = new Vector2[1]
				{
					new Vector2(15f, 5f)
				}
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "domeShingles",
							BaseCenter = new Vector2(53f, 56f)
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "domeShingles_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[2]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "domeShingles",
								BaseCenter = new Vector2(53f, 56f)
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "domeShingles_construct",
								BaseCenter = new Vector2(53f, 56f)
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 10f,
					PadShape = CollidePrim.Circle,
					Shapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, 0f), 25f)
						{
							Offset = new Vector2(-1f, -8f)
						}
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "adequateConstruction",
				SalvageProcess = "salvageDomeShelterSpoakShingles",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:shadeleafCanes", 4 },
					{ "item:spoakShingles", 1 }
				},
				Repair = "buildingRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:wigwamSpoakShingles")
		{
			Name = "Wigwam (Spoak shingles)",
			SummaryDescription = "A roomy and sturdy dwelling for 4 people",
			Description = "Not a simple building task in the wilderness, but the result will last for a long time. The design takes advantage of the curving shape of the spoak branches which are placed in an inter-locking pattern to form a large domed frame. Shingles from the tree's leaves make a durable cover. The leaves are treated with an anti-infestation emulsion, making the wigwam free from scuttler bugs.",
			ThumbnailSmall = "HUD_thumbnail_wigwamShingles",
			CategoryKey = "shelter",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Comfort
			},
			ContainerType = new HomeContainerType
			{
				CanBeEnteredByTags = new string[2] { "humanTransact", "leafcutterTransact" },
				ResidenceType = new ResidenceType
				{
					LivingCapacity = 4,
					ComfortLevel = comfortLevel4
				},
				StorageTags = new string[2] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" },
				ItemStorageType = new ItemStorageType("isolated", 8f),
				DefaultStorageSettings = "homeStorage",
				HasRallyPointInCourtyard = false,
				UpgradesProfile = "survivalHome4People",
				Doors = new Vector2[1]
				{
					new Vector2(21f, 19f)
				}
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "wigwamShingles"
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "wigwamShingles_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[2]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "wigwamShingles"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "wigwamShingles_construct"
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "wigwamShingles_construct_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 10f,
					PadShape = CollidePrim.Circle,
					Shapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, 0f), 29f)
						{
							Offset = new Vector2(-6f, 0f)
						}
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "sturdyConstruction",
				SalvageProcess = "salvageWigwamSpoakShingles",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:spoakBranchesTrimmed", 3 },
					{ "item:spoakShingles", 3 },
					{ "item:firegrassSod", 2 },
					{ "item:stones", 1 }
				},
				Repair = "buildingRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:storageHole")
		{
			Name = "Storage hole",
			SummaryDescription = "For storing and protecting food and ingredients",
			Description = "A hole lined with large stones. Covered with spoak leaves and some heavy rocks that will keep most animals out.",
			ThumbnailSmall = "HUD_thumbnail_storageholeYellowleaves",
			CategoryKey = "production",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Food
			},
			ContainerType = new StorageContainerType("earthCooled", 8f)
			{
				StorageTags = new string[2] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" },
				CanTransactWithTags = new string[2] { "humanTransact", "leafcutterTransact" },
				DefaultStorageSettings = "darkFoodStorage"
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "storageholeYellowleaves"
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "storageholeYellowleaves_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[2]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "storageholeYellowleaves"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "storageholeYellowleaves_construct_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 10f,
					PadShape = CollidePrim.Circle,
					Shapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, 0f), 16f)
						{
							Offset = new Vector2(0f, 3f)
						}
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "adequateConstruction",
				SalvageProcess = "salvageStorageHole",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:spoakLeaves", 1 },
					{ "item:stones", 1 }
				},
				Repair = "buildingRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:toolshed")
		{
			Name = "Toolshed",
			SummaryDescription = "A sturdy shed for storing farm equipment",
			Description = "Built from mudbricks covered in a basic white plaster. Will keep equipment in good condition when they are not in use.",
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ThumbnailSmall = "HUD_thumbnail_placeholder",
			CategoryKey = "production",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			ContainerType = new StorageContainerType("isolated", 8f)
			{
				StorageTags = new string[2] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" },
				CanTransactWithTags = new string[1] { "humanTransact" },
				DefaultStorageSettings = "farmToolshed"
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "shedskin1"
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "shed_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[2]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "shedskin1"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "shed_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 10f,
					PadShape = CollidePrim.Circle,
					Shapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, 0f), 16f)
						{
							Offset = new Vector2(0f, 3f)
						}
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "sturdyConstruction",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:waterCaneStem", 2 },
					{ "item:solidMudBrick", 3 }
				},
				Repair = "buildingRepair",
				SalvageProcess = "salvageToolshed"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:clayGranary")
		{
			Name = "Clay granary",
			SummaryDescription = "Safe storage of food and ingredients",
			Description = "To protect its contents from field quadites (and other scavengers) the granary is raised from the ground, built on pillars. The design and choice of building materials will prohibit field quadites (the most problematic pest animal) from entering.",
			ThumbnailSmall = "HUD_thumbnail_clayGranary",
			CategoryKey = "production",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			ContainerType = new StorageContainerType("isolated", 8f)
			{
				StorageTags = new string[2] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" },
				CanTransactWithTags = new string[1] { "humanTransact" },
				DefaultStorageSettings = "darkFoodStorage"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic",
				Area = RatingTypes.Food
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "clayGranary"
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "clayGranary_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[2]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "clayGranary"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "clayGranary_construct"
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "clayGranary_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 10f,
					PadShape = CollidePrim.Circle,
					Shapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, 3f), 16f)
					},
					SelectionShapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, -20f), 31f)
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "sturdyConstruction",
				SalvageProcess = "salvageClayGranary",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:spoakShingles", 1 },
					{ "item:solidMudBrick", 5 }
				},
				Repair = "buildingRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:cooledFoodCache")
		{
			Name = "Cooled food cache",
			SummaryDescription = "Cooled with the aircraft airconditioning unit",
			Description = "It should be possible to refrigerate our food by storing it in a hole together with the airconditioning unit. The hole must be lined with large stones and covered with spoak leaves and rocks to keep animals out.",
			ThumbnailSmall = "HUD_thumbnail_storageholeYellowleaves",
			CategoryKey = "production",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic",
				Area = RatingTypes.Food
			},
			ContainerType = new StorageContainerType("refrigerator", 8f)
			{
				StorageTags = new string[2] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" },
				CanTransactWithTags = new string[2] { "humanTransact", "leafcutterTransact" },
				DefaultStorageSettings = "cooledStorage"
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "storageholeYellowleaves"
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "storageholeYellowleaves_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[2]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "storageholeYellowleaves"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "storageholeYellowleaves_construct_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 10f,
					PadShape = CollidePrim.Circle,
					Shapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, 0f), 16f)
						{
							Offset = new Vector2(0f, 3f)
						}
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "adequateConstruction",
				SalvageProcess = "salvageCooledFoodCache",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:activatedFoodCoolerUnit", 1 },
					{ "item:spoakLeaves", 1 },
					{ "item:stones", 1 }
				},
				Repair = "buildingRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:abatis")
		{
			Name = "Abatis",
			SummaryDescription = "Fence made from tangled, curving branches",
			Description = "Obstacles like these might be able to block passage of some predators since spoak branches can form a dense barrier.",
			ThumbnailSmall = "HUD_thumbnail_abatis",
			CategoryKey = "defense",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Security
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "abatis1"
						}
					}
				},
				ClientStateConditions = new ClientStateInfo[1]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "abatis1"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					GridAlignedPlacement = true,
					Shapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, 0f), 7f)
						{
							Offset = new Vector2(0f, 0f)
						}
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "adequateConstruction",
				SalvageProcess = "salvageAbatis1",
				PartKeys = new SerializableDictionary<string, int> { { "item:spoakBranches", 1 } },
				Repair = "buildingRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:campfire")
		{
			Name = "Campfire",
			SummaryDescription = "The campfire needs an available supply of firewood",
			Description = "Most food requires preparation on a campfire. Some bushcraft production also requires heat from a fireplace. It is important to always have a supply of firewood in stock, else the campfire is useless.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ThumbnailSmall = "HUD_thumbnail_fireplace",
			CategoryKey = "production",
			StructureType = new StructureType
			{
				IsAddon = true,
				BuildByPlayer = true
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 10f,
					PadShape = CollidePrim.Circle,
					Shapes = new CollideShape2D[3]
					{
						new CollideShape2D(Vector2.Zero, 22f),
						new CollideShape2D(Vector2.Zero, 8f)
						{
							Offset = new Vector2(-32f, -9f)
						},
						new CollideShape2D(Vector2.Zero, 9f)
						{
							Offset = new Vector2(24f, -17f)
						}
					}
				}
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "fireplace"
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "fireplace_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[3]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "fireplace"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "fireplace_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "fireplace"
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "fireplace_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 39),
						LightingTypes = new LightingType[1]
						{
							new LightingType
							{
								SpriteName = "common_21_circularbig",
								Offset = new Point(-48, -48)
							}
						},
						ParticleEmitters = new ParticleEmitterEffect[2]
						{
							new ParticleEmitterEffect
							{
								ParticleSystemKey = "smallerSmoke"
							},
							new ParticleEmitterEffect
							{
								ParticleSystemKey = "smallFire"
							}
						}
					}
				}
			},
			ToolType = new ToolType
			{
				ToolTag = new string[1] { "fireplace" },
				Durability = 1f,
				ToolHandling = ToolHandlingType.Stationary,
				PrepareProcess = "lightFire"
			},
			ContainerType = new ReplenishContainerType
			{
				CanTransactWithTags = new string[1] { "humanTransact" },
				RequiresReplenishType = new RequiresReplenishType
				{
					ReplenishProcess = "refuelCampfire",
					RequiresFuelType = new RequiresFuelType
					{
						MaxFuel = 1f,
						FuelTypeTag = "fuelForCampfire",
						BurnRatePerDay = 7f
					}
				}
			},
			GatheringSiteType = new GatheringSiteType
			{
				arc = new Arc
				{
					Radius = 40f,
					MinAngle = -180.0,
					MaxAngle = 180.0
				},
				MaxVisitors = 30
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "ricketyConstruction",
				SalvageProcess = "salvageCampfire",
				PartKeys = new SerializableDictionary<string, int> { { "item:stones", 1 } },
				Repair = "buildingRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:fieldKitchen")
		{
			Name = "Kitchen (movable)",
			SummaryDescription = "Advanced field kitchen which uses liquid gas for cooking.",
			Description = "Specially designed for the Tau Ceti mission. Provides essential functions for food preparation in the field.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Food
			},
			ThumbnailSmall = "HUD_thumbnail_kitchenPremade",
			CategoryKey = "production",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 5f,
					PadShape = CollidePrim.Circle,
					Shapes = new CollideShape2D[3]
					{
						new CollideShape2D(Vector2.Zero, 26f),
						new CollideShape2D(Vector2.Zero, 11f)
						{
							Offset = new Vector2(-37f, 8f)
						},
						new CollideShape2D(Vector2.Zero, 11f)
						{
							Offset = new Vector2(24f, 17f)
						}
					}
				}
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "kitchenPremade"
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "kitchenPremade_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[3]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "kitchenPremade"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "kitchenPremade_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "kitchenPremade"
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "kitchenPremade_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 39),
						ParticleEmitters = new ParticleEmitterEffect[1]
						{
							new ParticleEmitterEffect
							{
								ParticleSystemKey = "foodSteam"
							}
						}
					}
				}
			},
			ToolType = new ToolType
			{
				Durability = 1f,
				ToolHandling = ToolHandlingType.Stationary,
				PrepareProcess = "cookAtStove"
			},
			ContainerType = new WorkshopContainerType
			{
				CanTransactWithTags = new string[1] { "humanTransact" },
				RequiresReplenishType = new RequiresReplenishType
				{
					RequiresFuelType = new RequiresFuelType
					{
						MaxFuel = 1f,
						FuelTypeTag = "fuelForFieldKitchen",
						BurnRatePerDay = 5f
					},
					ReplenishProcess = "refuelKitchen"
				},
				StorageTags = new string[2] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" },
				ItemStorageType = new ItemStorageType("refrigerator", 1.5f, "isolated", 2.5f),
				DefaultStorageSettings = "kitchenStorage"
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "adequateConstruction",
				SalvageProcess = "salvageFieldKitchen",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:fieldKitchenStove", 1 },
					{ "item:fieldKitchenEquipment", 1 }
				},
				Repair = "buildingRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:improvisedKitchen")
		{
			Name = "Kitchen (scraps)",
			SummaryDescription = "A work area for preparing ingredients and cooking food).",
			Description = "This workspace gives us the ability to quickly prepare and cook large amounts of food.",
			ThumbnailSmall = "HUD_thumbnail_kitchenImprovised",
			CategoryKey = "production",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic",
				Area = RatingTypes.Food
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = -10f,
					PadShape = CollidePrim.Circle,
					Shapes = new CollideShape2D[2]
					{
						new CollideShape2D(Vector2.Zero, 17f)
						{
							Offset = new Vector2(21f, -3f)
						},
						new CollideShape2D(Vector2.Zero, 25f)
						{
							Offset = new Vector2(-42f, 13f)
						}
					},
					SelectionShapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(21f, -3f), 22f)
					}
				}
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "kitchenImprovised",
							Offset = new Vector2(-35f, 7f)
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "kitchenImprovised_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[3]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "kitchenImprovised",
								Offset = new Vector2(-35f, 7f)
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "kitchenImprovised_construct",
								Offset = new Vector2(-35f, 7f)
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "kitchenImprovised_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "kitchenImprovised",
								Offset = new Vector2(-35f, 7f)
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "kitchenImprovised_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 39),
						ParticleEmitters = new ParticleEmitterEffect[2]
						{
							new ParticleEmitterEffect
							{
								ParticleSystemKey = "smallerSmoke",
								Offset = new Vector2(21f, -3f)
							},
							new ParticleEmitterEffect
							{
								ParticleSystemKey = "smallFire",
								Offset = new Vector2(21f, -3f)
							}
						}
					}
				}
			},
			ToolType = new ToolType
			{
				Durability = 1f,
				ToolHandling = ToolHandlingType.Stationary,
				PrepareProcess = "kitchenImprovisedLightFire"
			},
			ContainerType = new WorkshopContainerType
			{
				CanTransactWithTags = new string[1] { "humanTransact" },
				RequiresReplenishType = new RequiresReplenishType
				{
					ReplenishProcess = "refuelCampfire",
					RequiresFuelType = new RequiresFuelType
					{
						MaxFuel = 1f,
						FuelTypeTag = "fuelForCampfire",
						BurnRatePerDay = 5f
					}
				},
				StorageTags = new string[2] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" },
				ItemStorageType = new ItemStorageType("isolated", 1f),
				DefaultStorageSettings = "uncooledKitchenStorage"
			},
			GatheringSiteType = new GatheringSiteType
			{
				arc = new Arc
				{
					Radius = 27f,
					MinAngle = -180.0,
					MaxAngle = 180.0
				},
				MaxVisitors = 30
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "adequateConstruction",
				SalvageProcess = "salvageImprovisedKitchen",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:panelScraps", 1 },
					{ "item:sticks", 2 },
					{ "item:spoakShingles", 1 }
				},
				Repair = "buildingRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:mudBrickKitchen")
		{
			Name = "Kitchen (simple)",
			SummaryDescription = "A work area for preparing ingredients and cooking food.",
			Description = "This workspace gives the ability to efficiently prepare and cook sizable amounts of food.",
			ThumbnailSmall = "HUD_thumbnail_kitchenImprovised",
			CategoryKey = "production",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic",
				Area = RatingTypes.Food
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = -10f,
					PadShape = CollidePrim.Circle,
					Shapes = new CollideShape2D[2]
					{
						new CollideShape2D(Vector2.Zero, 17f)
						{
							Offset = new Vector2(21f, -3f)
						},
						new CollideShape2D(Vector2.Zero, 25f)
						{
							Offset = new Vector2(-42f, 13f)
						}
					},
					SelectionShapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(21f, -3f), 22f)
					}
				}
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "kitchenImprovised",
							Offset = new Vector2(-35f, 7f)
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "kitchenImprovised_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[3]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "kitchenImprovised",
								Offset = new Vector2(-35f, 7f)
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "kitchenImprovised_construct",
								Offset = new Vector2(-35f, 7f)
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "kitchenImprovised_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "kitchenImprovised",
								Offset = new Vector2(-35f, 7f)
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "kitchenImprovised_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 39),
						ParticleEmitters = new ParticleEmitterEffect[2]
						{
							new ParticleEmitterEffect
							{
								ParticleSystemKey = "smallerSmoke",
								Offset = new Vector2(21f, -3f)
							},
							new ParticleEmitterEffect
							{
								ParticleSystemKey = "smallFire",
								Offset = new Vector2(21f, -3f)
							}
						}
					}
				}
			},
			ToolType = new ToolType
			{
				Durability = 1f,
				ToolHandling = ToolHandlingType.Stationary,
				PrepareProcess = "kitchenImprovisedLightFire"
			},
			ContainerType = new WorkshopContainerType
			{
				CanTransactWithTags = new string[1] { "humanTransact" },
				RequiresReplenishType = new RequiresReplenishType
				{
					ReplenishProcess = "refuelCampfire",
					RequiresFuelType = new RequiresFuelType
					{
						MaxFuel = 1f,
						FuelTypeTag = "fuelForCampfire",
						BurnRatePerDay = 5f
					}
				},
				StorageTags = new string[2] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" },
				ItemStorageType = new ItemStorageType("isolated", 1f),
				DefaultStorageSettings = "uncooledKitchenStorage"
			},
			GatheringSiteType = new GatheringSiteType
			{
				arc = new Arc
				{
					Radius = 27f,
					MinAngle = -180.0,
					MaxAngle = 180.0
				},
				MaxVisitors = 30
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "adequateConstruction",
				SalvageProcess = "salvageMudBrickKitchen",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:sticks", 2 },
					{ "item:spoakShingles", 1 }
				},
				Repair = "buildingRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:workshopBuilding")
		{
			Name = "Workshop building",
			SummaryDescription = "A building that can house different upgrades",
			Description = "The building starts empty but can be equipped with various tools to form an efficient workplace.",
			ThumbnailSmall = "HUD_thumbnail_plasticWorkshop",
			CategoryKey = "production",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 0f,
					PadShape = CollidePrim.Circle,
					Shapes = new CollideShape2D[3]
					{
						new CollideShape2D(Vector2.Zero, 24f)
						{
							Offset = new Vector2(-15f, -7f)
						},
						new CollideShape2D(Vector2.Zero, 13f)
						{
							Offset = new Vector2(16f, -9f)
						},
						new CollideShape2D(Vector2.Zero, 13f)
						{
							Offset = new Vector2(39f, -8f)
						}
					}
				}
			},
			SimStateConditions = new SimStateInfo[4]
			{
				new SimStateInfo
				{
					Conditions = new BitMask64(typeof(StateModifier), 30),
					GeometryLayoutType = new GeometryLayoutType
					{
						Pad = 0f,
						PadShape = CollidePrim.Circle,
						Shapes = new CollideShape2D[4]
						{
							new CollideShape2D(Vector2.Zero, 17f)
							{
								Offset = new Vector2(-58f, -2f)
							},
							new CollideShape2D(Vector2.Zero, 23f)
							{
								Offset = new Vector2(-27f, -5f)
							},
							new CollideShape2D(Vector2.Zero, 33f)
							{
								Offset = new Vector2(8f, 1f)
							},
							new CollideShape2D(Vector2.Zero, 17f)
							{
								Offset = new Vector2(45f, 12f)
							}
						}
					}
				},
				new SimStateInfo
				{
					Conditions = new BitMask64(typeof(StateModifier), 31),
					GeometryLayoutType = new GeometryLayoutType
					{
						Pad = 0f,
						PadShape = CollidePrim.Circle,
						Shapes = new CollideShape2D[3]
						{
							new CollideShape2D(Vector2.Zero, 23f)
							{
								Offset = new Vector2(-16f, -6f)
							},
							new CollideShape2D(Vector2.Zero, 28f)
							{
								Offset = new Vector2(6f, -3f)
							},
							new CollideShape2D(Vector2.Zero, 22f)
							{
								Offset = new Vector2(35f, 4f)
							}
						}
					}
				},
				new SimStateInfo
				{
					Conditions = new BitMask64(typeof(StateModifier), 32),
					GeometryLayoutType = new GeometryLayoutType
					{
						Pad = 0f,
						PadShape = CollidePrim.Circle,
						Shapes = new CollideShape2D[4]
						{
							new CollideShape2D(Vector2.Zero, 21f)
							{
								Offset = new Vector2(-24f, -2f)
							},
							new CollideShape2D(Vector2.Zero, 25f)
							{
								Offset = new Vector2(2f, -5f)
							},
							new CollideShape2D(Vector2.Zero, 27f)
							{
								Offset = new Vector2(42f, 15f)
							},
							new CollideShape2D(Vector2.Zero, 14f)
							{
								Offset = new Vector2(-15f, 25f)
							}
						}
					}
				},
				new SimStateInfo
				{
					Conditions = new BitMask64(typeof(StateModifier), 33),
					GeometryLayoutType = new GeometryLayoutType
					{
						Pad = 0f,
						PadShape = CollidePrim.Circle,
						Shapes = new CollideShape2D[4]
						{
							new CollideShape2D(Vector2.Zero, 18f)
							{
								Offset = new Vector2(-62f, 2f)
							},
							new CollideShape2D(Vector2.Zero, 37f)
							{
								Offset = new Vector2(-8f, 7f)
							},
							new CollideShape2D(Vector2.Zero, 33f)
							{
								Offset = new Vector2(8f, 1f)
							},
							new CollideShape2D(Vector2.Zero, 27f)
							{
								Offset = new Vector2(25f, 1f)
							}
						}
					}
				}
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "workshopEmpty"
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "workshopEmpty_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[7]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "workshopEmpty"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "plasticWorkshop_construct"
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plasticWorkshop_construct_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "workshopTextile"
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "workshopTextile_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 30)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "workshopMachinist"
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "workshopMachinist_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 31)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "workshopCarpenter"
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "workshopCarpenter_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 32)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "workshopPolymer"
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "workshopPolymer_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 33)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "workshopPolymer"
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "workshopPolymer_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 40, 33),
						ParticleEmitters = new ParticleEmitterEffect[2]
						{
							new ParticleEmitterEffect
							{
								ParticleSystemKey = "smallestSmoke",
								Offset = new Vector2(0f, -30f)
							},
							new ParticleEmitterEffect
							{
								ParticleSystemKey = "tinyFire",
								Offset = new Vector2(4f, 13f)
							}
						}
					}
				}
			},
			ContainerType = new UpgradableBuildingContainerType
			{
				CanBeEnteredByTags = new string[1] { "humanTransact" },
				UpgradesProfile = "workshopProfile",
				StorageTags = new string[2] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" },
				ItemStorageType = new ItemStorageType("isolated", 4f),
				DefaultStorageSettings = "noStorage",
				Doors = new Vector2[1]
				{
					new Vector2(0f, 20f)
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "sturdyConstruction",
				SalvageProcess = "salvageWorkshopBuilding",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:solidMudBrick", 5 },
					{ "item:waterCaneStem", 3 },
					{ "item:stones", 1 }
				},
				Repair = "buildingRepair"
			}
		});
		ParticleEmitterEffect[] particleEmitters = new ParticleEmitterEffect[1]
		{
			new ParticleEmitterEffect
			{
				ParticleSystemKey = "smallestSmoke",
				Offset = new Vector2(5f, -54f)
			}
		};
		RenderAsBillboardType renderAsBillboardType = new RenderAsBillboardType
		{
			AssetName = "cookhouse"
		};
		RenderAsBillboardType renderAsBillboardType2 = new RenderAsBillboardType();
		renderAsBillboardType2.AssetName = "cookhouseAddition";
		renderAsBillboardType2.Offset = new Vector2(37f, -25f);
		RenderAsBillboardType renderAsBillboardType3 = renderAsBillboardType2;
		renderAsBillboardType2 = new RenderAsBillboardType();
		renderAsBillboardType2.AssetName = "cookhouseDryingShed";
		renderAsBillboardType2.Offset = new Vector2(-41f, 4f);
		RenderAsBillboardType renderAsBillboardType4 = renderAsBillboardType2;
		renderAsBillboardType2 = new RenderAsBillboardType();
		renderAsBillboardType2.AssetName = "cookhouseSmokeOven";
		renderAsBillboardType2.Offset = new Vector2(-67f, -10f);
		RenderAsBillboardType renderAsBillboardType5 = renderAsBillboardType2;
		listOfEntityTypes.Add(new EntityType("structure:cookhouse")
		{
			Name = "Cookhouse",
			SummaryDescription = "A kitchen building that can be outfitted with different cooking installations",
			Description = "The building starts empty but can have various tools installed to form an efficient workplace for making food.",
			ThumbnailSmall = "HUD_thumbnail_cookhouse",
			CategoryKey = "production",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TierOrArea = new TierOrArea
			{
				Tier = "medium",
				Area = RatingTypes.Food
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 0f,
					PadShape = CollidePrim.Circle,
					Shapes = new CollideShape2D[5]
					{
						new CollideShape2D(Vector2.Zero, 19f)
						{
							Offset = new Vector2(-42f, -17f)
						},
						new CollideShape2D(Vector2.Zero, 21f)
						{
							Offset = new Vector2(-23f, 2f)
						},
						new CollideShape2D(Vector2.Zero, 21f)
						{
							Offset = new Vector2(9f, 5f)
						},
						new CollideShape2D(Vector2.Zero, 38f)
						{
							Offset = new Vector2(36f, -21f)
						},
						new CollideShape2D(Vector2.Zero, 12f)
						{
							Offset = new Vector2(-64f, -11f)
						}
					},
					SelectionShapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, 0f), 35f)
					}
				}
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "cookhouse"
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "cookhouse_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[32]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1] { renderAsBillboardType },
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "cookhouse_construct"
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "cookhouse_construct_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1] { renderAsBillboardType },
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "cookhouse_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 34)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1] { renderAsBillboardType },
						ParticleEmitters = particleEmitters,
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "cookhouse_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 34, 39)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[2] { renderAsBillboardType, renderAsBillboardType3 },
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "cookhouse_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 37)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[2] { renderAsBillboardType, renderAsBillboardType3 },
						ParticleEmitters = particleEmitters,
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "cookhouse_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 37, 39)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[2] { renderAsBillboardType, renderAsBillboardType5 },
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "cookhouse_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 36)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[2] { renderAsBillboardType, renderAsBillboardType5 },
						ParticleEmitters = particleEmitters,
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "cookhouse_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 36, 39)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[2] { renderAsBillboardType, renderAsBillboardType4 },
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "cookhouse_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 35)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[2] { renderAsBillboardType, renderAsBillboardType4 },
						ParticleEmitters = particleEmitters,
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "cookhouse_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 35, 39)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[2] { renderAsBillboardType, renderAsBillboardType5 },
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "cookhouse_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 36, 34)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[2] { renderAsBillboardType, renderAsBillboardType5 },
						ParticleEmitters = particleEmitters,
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "cookhouse_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 36, 34, 39)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[2] { renderAsBillboardType, renderAsBillboardType3 },
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "cookhouse_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 37, 34)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[2] { renderAsBillboardType, renderAsBillboardType3 },
						ParticleEmitters = particleEmitters,
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "cookhouse_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 37, 34, 39)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[2] { renderAsBillboardType, renderAsBillboardType4 },
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "cookhouse_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 35, 34)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[2] { renderAsBillboardType, renderAsBillboardType4 },
						ParticleEmitters = particleEmitters,
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "cookhouse_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 35, 34, 39)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[3] { renderAsBillboardType, renderAsBillboardType5, renderAsBillboardType3 },
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "cookhouse_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 36, 37)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[3] { renderAsBillboardType, renderAsBillboardType5, renderAsBillboardType3 },
						ParticleEmitters = particleEmitters,
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "cookhouse_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 36, 37, 39)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[3] { renderAsBillboardType, renderAsBillboardType5, renderAsBillboardType4 },
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "cookhouse_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 36, 35)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[3] { renderAsBillboardType, renderAsBillboardType5, renderAsBillboardType4 },
						ParticleEmitters = particleEmitters,
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "cookhouse_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 36, 35, 39)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[3] { renderAsBillboardType, renderAsBillboardType3, renderAsBillboardType4 },
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "cookhouse_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 37, 35)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[3] { renderAsBillboardType, renderAsBillboardType3, renderAsBillboardType4 },
						ParticleEmitters = particleEmitters,
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "cookhouse_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 37, 35, 39)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[3] { renderAsBillboardType, renderAsBillboardType5, renderAsBillboardType4 },
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "cookhouse_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 34, 36, 35)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[3] { renderAsBillboardType, renderAsBillboardType5, renderAsBillboardType4 },
						ParticleEmitters = particleEmitters,
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "cookhouse_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 34, 36, 35, 39)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[3] { renderAsBillboardType, renderAsBillboardType5, renderAsBillboardType3 },
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "cookhouse_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 37, 36, 34)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[3] { renderAsBillboardType, renderAsBillboardType5, renderAsBillboardType3 },
						ParticleEmitters = particleEmitters,
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "cookhouse_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 37, 36, 34, 39)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[3] { renderAsBillboardType, renderAsBillboardType4, renderAsBillboardType3 },
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "cookhouse_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 37, 34, 35)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[3] { renderAsBillboardType, renderAsBillboardType4, renderAsBillboardType3 },
						ParticleEmitters = particleEmitters,
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "cookhouse_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 37, 34, 35, 39)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[4] { renderAsBillboardType, renderAsBillboardType5, renderAsBillboardType4, renderAsBillboardType3 },
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "cookhouse_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 37, 36, 35)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[4] { renderAsBillboardType, renderAsBillboardType5, renderAsBillboardType4, renderAsBillboardType3 },
						ParticleEmitters = particleEmitters,
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "cookhouse_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 37, 36, 35, 39)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[4] { renderAsBillboardType, renderAsBillboardType5, renderAsBillboardType4, renderAsBillboardType3 },
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "cookhouse_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 34, 37, 36, 35)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[4] { renderAsBillboardType, renderAsBillboardType5, renderAsBillboardType4, renderAsBillboardType3 },
						ParticleEmitters = particleEmitters,
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "cookhouse_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 34, 37, 36, 35, 39)
					}
				}
			},
			ContainerType = new UpgradableBuildingContainerType
			{
				CanBeEnteredByTags = new string[1] { "humanTransact" },
				UpgradesProfile = "cookhouseProfile",
				StorageTags = new string[2] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" },
				ItemStorageType = new ItemStorageType("isolated", 4f),
				DefaultStorageSettings = "homeStorage",
				Doors = new Vector2[1]
				{
					new Vector2(-12f, -11f)
				}
			},
			GatheringSiteType = new GatheringSiteType
			{
				arc = new Arc
				{
					Radius = 27f,
					MinAngle = -180.0,
					MaxAngle = 180.0
				},
				MaxVisitors = 30
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "sturdyConstruction",
				SalvageProcess = "salvageCookhouse",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:solidMudBrick", 5 },
					{ "item:waterCaneStem", 3 },
					{ "item:spoakBranchesTrimmed", 2 },
					{ "item:spoakShingles", 2 },
					{ "item:textile", 3 },
					{ "item:stones", 1 }
				},
				Repair = "buildingRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:still")
		{
			Name = "Still",
			SummaryDescription = "Equipment for distilling liquid mixtures",
			Description = "Primarily used for producing ethanol for beverages such as brandy. The still boils liquids and condenses the vapor and this way it could be used for separating and purifying many types of chemicals.",
			ThumbnailSmall = "HUD_thumbnail_still",
			CategoryKey = "production",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 0f,
					PadShape = CollidePrim.Circle,
					Shapes = new CollideShape2D[2]
					{
						new CollideShape2D(Vector2.Zero, 17f)
						{
							Offset = new Vector2(15f, -3f)
						},
						new CollideShape2D(Vector2.Zero, 17f)
						{
							Offset = new Vector2(-15f, -3f)
						}
					}
				}
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "still"
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "still_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[3]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "still"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "still_construct"
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "still_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "still"
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "still_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 39),
						ParticleEmitters = new ParticleEmitterEffect[2]
						{
							new ParticleEmitterEffect
							{
								ParticleSystemKey = "smallerSmoke",
								Offset = new Vector2(-21f, 7f)
							},
							new ParticleEmitterEffect
							{
								ParticleSystemKey = "smallFire",
								Offset = new Vector2(-21f, 7f)
							}
						}
					}
				}
			},
			ToolType = new ToolType
			{
				Durability = 1f,
				ToolHandling = ToolHandlingType.Stationary,
				PrepareProcess = "kitchenImprovisedLightFire"
			},
			ContainerType = new WorkshopContainerType
			{
				CanTransactWithTags = new string[1] { "humanTransact" },
				RequiresReplenishType = new RequiresReplenishType
				{
					ReplenishProcess = "refuelCampfire",
					RequiresFuelType = new RequiresFuelType
					{
						MaxFuel = 1f,
						FuelTypeTag = "fuelForCampfire",
						BurnRatePerDay = 5f
					}
				},
				StorageTags = new string[2] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" },
				ItemStorageType = new ItemStorageType("isolated", 1f),
				DefaultStorageSettings = "uncooledKitchenStorage"
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "sturdyConstruction",
				SalvageProcess = "salvageStill",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:stillComponents", 1 },
					{ "item:stones", 1 },
					{ "item:clayJar", 1 },
					{ "item:sticks", 1 }
				},
				Repair = "buildingRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:improvisedWorkbench")
		{
			Name = "Workbench (scraps)",
			SummaryDescription = "A work area for preparing materials and making items",
			Description = "Has a table for doing simple carpentry which improves efficiency and speeds up crafting of items.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ThumbnailSmall = "HUD_thumbnail_workbenchImprovised",
			CategoryKey = "production",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 7f,
					PadShape = CollidePrim.Circle,
					Shapes = new CollideShape2D[3]
					{
						new CollideShape2D(Vector2.Zero, 26f),
						new CollideShape2D(Vector2.Zero, 11f)
						{
							Offset = new Vector2(-37f, -17f)
						},
						new CollideShape2D(Vector2.Zero, 11f)
						{
							Offset = new Vector2(24f, 17f)
						}
					}
				}
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "workbenchImprovised"
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "workbenchImprovised_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[2]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "workbenchImprovised"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "workbenchImprovised_construct"
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "workbenchImprovised_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					}
				}
			},
			ToolType = new ToolType
			{
				Durability = 1f,
				ToolHandling = ToolHandlingType.Stationary
			},
			ContainerType = new WorkshopContainerType
			{
				CanTransactWithTags = new string[1] { "humanTransact" },
				StorageTags = new string[2] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" },
				ItemStorageType = new ItemStorageType("isolated", 1f),
				DefaultStorageSettings = "workbenchStorage"
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "sturdyConstruction",
				SalvageProcess = "salvageImprovisedWorkbench",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:panelScraps", 1 },
					{ "item:sticks", 3 }
				},
				Repair = "buildingRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:mudBrickWorkbench")
		{
			Name = "Workbench (simple)",
			SummaryDescription = "A work area for preparing materials and making items",
			Description = "Has a table for doing simple carpentry which improves efficiency and speeds up crafting of items.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ThumbnailSmall = "HUD_thumbnail_workbenchImprovised",
			CategoryKey = "production",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 7f,
					PadShape = CollidePrim.Circle,
					Shapes = new CollideShape2D[3]
					{
						new CollideShape2D(Vector2.Zero, 26f),
						new CollideShape2D(Vector2.Zero, 11f)
						{
							Offset = new Vector2(-37f, -17f)
						},
						new CollideShape2D(Vector2.Zero, 11f)
						{
							Offset = new Vector2(24f, 17f)
						}
					}
				}
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "workbenchImprovised"
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "workbenchImprovised_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[2]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "workbenchImprovised"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "workbenchImprovised_construct"
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "workbenchImprovised_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					}
				}
			},
			ToolType = new ToolType
			{
				Durability = 1f,
				ToolHandling = ToolHandlingType.Stationary
			},
			ContainerType = new WorkshopContainerType
			{
				CanTransactWithTags = new string[1] { "humanTransact" },
				StorageTags = new string[2] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" },
				ItemStorageType = new ItemStorageType("isolated", 1f),
				DefaultStorageSettings = "workbenchStorage"
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "sturdyConstruction",
				SalvageProcess = "salvageMudBrickWorkbench",
				PartKeys = new SerializableDictionary<string, int> { { "item:sticks", 3 } },
				Repair = "buildingRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:smokeOven")
		{
			Name = "Smoke oven",
			SummaryDescription = "Structure for smoking food",
			Description = "Smoking is an ancient way to preserve meat and fish. This structure will cold smoke the food since the fire is placed away from the food. Consists of small teepee made from sticks and sod wherein the meat is placed. A small tunnel covered with stones is dug out and a fire is lit at the end of the tunnel.",
			ThumbnailSmall = "HUD_thumbnail_smokeOven",
			CategoryKey = "production",
			StructureType = new StructureType
			{
				IsAddon = true,
				BuildByPlayer = true
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 8f,
					PadShape = CollidePrim.Circle,
					Shapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, 0f), 17f)
						{
							Offset = new Vector2(-12f, 2f)
						}
					}
				}
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Food
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "smokeOven",
							Offset = new Vector2(-12f, 0f)
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "smokeOven_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[3]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "smokeOven",
								Offset = new Vector2(-12f, 0f)
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "smokeOven_construct",
								Offset = new Vector2(-12f, 0f)
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "smokeOven",
								Offset = new Vector2(-12f, 0f)
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "smokeOven_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 39),
						ParticleEmitters = new ParticleEmitterEffect[1]
						{
							new ParticleEmitterEffect
							{
								ParticleSystemKey = "smallestSmoke"
							}
						}
					}
				}
			},
			ToolType = new ToolType
			{
				ToolTag = new string[1] { "smokeOven" },
				Durability = 0.9f,
				ToolHandling = ToolHandlingType.Stationary,
				PrepareProcess = "lightFireWithoutFlames"
			},
			ContainerType = new ToolContainerType
			{
				CanTransactWithTags = new string[1] { "humanTransact" },
				ProductionOutputStorageType = new ItemStorageType(1f)
				{
					FullStatePercentage = 0.1f,
					HalfFullStatePercentage = 0.05f
				},
				RequiresReplenishType = new RequiresReplenishType
				{
					ReplenishProcess = "refuelSmokeOven",
					RequiresFuelType = new RequiresFuelType
					{
						MaxFuel = 1f,
						FuelTypeTag = "fuelForCampfire",
						BurnRatePerDay = 5f
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "adequateConstruction",
				SalvageProcess = "salvageSmokeOven",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:sticks", 1 },
					{ "item:stones", 1 },
					{ "item:firegrassSod", 1 }
				},
				Repair = "buildingRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:sensor")
		{
			Name = "Motion sensor (deployed)",
			SummaryDescription = "Detects animals that enter its area",
			Description = "The sensor will monitor its immediate surroundings. To move the sensor, select SALVAGE",
			ThumbnailSmall = "HUD_thumbnail_sensorStructure",
			CategoryKey = "defense",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Security
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "sensorStructure"
						}
					}
				},
				ClientStateConditions = new ClientStateInfo[1]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "sensorStructure"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					}
				}
			},
			SensorType = new SensorType
			{
				Range = 420f,
				RangeAtNight = 420f,
				DetectionTypeKey = "motionSensor"
			},
			IntelligenceType = new IntelligenceType
			{
				StrengthRating = StrengthRating.None,
				IsMobile = false,
				CanAttack = false,
				CanUseWeapons = false,
				CanHunt = false,
				CanScout = false,
				CanExamine = false,
				CanPatrol = false,
				CanHaul = false,
				CanDoJobs = false,
				ServantForEntityTypeTag = "servesHumans"
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 0f,
					Shapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, 0f), 6f)
						{
							Offset = new Vector2(0f, 8f)
						}
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "advancedConstruction",
				SalvageProcess = "salvageSensor",
				PartKeys = new SerializableDictionary<string, int> { { "item:sensor", 1 } },
				Repair = "buildingRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:simplePort")
		{
			Name = "Port (simple)",
			SummaryDescription = "Tiny port which allows us to receive boats and sell goods to other settlements. Small capacity. Suited for selling food.",
			Description = "Has a pier where small boats and barges can moor and a storehouse where goods intended for sale can be placed. The clay storehouse is raised on pillars to keep a small amount of goods safe from vermin. \nThe storehouse also has a space for storage of items that are not intended for sale.",
			ThumbnailSmall = "HUD_thumbnail_simplePier",
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			CategoryKey = "miscellaneous",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TerminalType = new TerminalType
			{
				TypeOfTerminal = TerminalType.TypesOfTerminal.Pier
			},
			ContainerType = new TerminalContainerType
			{
				CanTransactWithTags = new string[1] { "humanTransact" },
				OfferedForTradeStorageType = new ItemStorageType(8f),
				StorageTags = new string[2] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" },
				ItemStorageType = new ItemStorageType(2f),
				DefaultStorageSettings = "simplePortLocalStorage"
			},
			SensorType = new SensorType
			{
				DetectionTypeKey = "communicationSensor",
				Range = 24f,
				RangeAtNight = 24f
			},
			IntelligenceType = new IntelligenceType
			{
				StrengthRating = StrengthRating.None,
				IsMobile = false,
				CanAttack = false,
				CanUseWeapons = false,
				CanHunt = false,
				CanScout = false,
				CanExamine = false,
				CanPatrol = false,
				CanHaul = false,
				CanDoJobs = false,
				ServantForEntityTypeTag = "servesHumans"
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "storeHouseClay",
							Offset = new Vector2(18f, -36f)
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "pierSimple_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[2]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "storeHouseClay",
								Offset = new Vector2(18f, -36f)
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "storeHouseClay_construct",
								Offset = new Vector2(18f, -36f)
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "pierSimple_construct_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 1f,
					SelectionShapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, 0f), 46f)
						{
							Offset = new Vector2(0f, 0f)
						}
					},
					Shapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, 0f), 20f)
						{
							Offset = new Vector2(30f, -30f)
						}
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "sturdyConstruction",
				SalvageProcess = "salvageSimplePort",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:spoakBranchesTrimmed", 3 },
					{ "item:spoakShingles", 1 },
					{ "item:solidMudBrick", 5 },
					{ "item:waterCaneStem", 3 }
				},
				Repair = "buildingRepairCustomProcess"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:canopyPort")
		{
			Name = "Port (canopy)",
			SummaryDescription = "Big port which allows us to receive boats and sell goods to other settlements. No vermin protection.",
			Description = "Has a pier where small boats and barges can moor and a large storage canopy where goods intended for sale can be placed. The goods are not protected from vermin, so this structure is NOT suited for trading food. \nThe canopy also has a space for storage of items that are not intended for sale.",
			ThumbnailSmall = "HUD_thumbnail_storageCanopy",
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			CategoryKey = "miscellaneous",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TerminalType = new TerminalType
			{
				TypeOfTerminal = TerminalType.TypesOfTerminal.Pier
			},
			ContainerType = new TerminalContainerType
			{
				CanTransactWithTags = new string[8] { "humanTransact", "robotTransact", "ratTransact", "leafcutterTransact", "chickenTransact", "snatcherTransact", "twinklerTransact", "demonTreeTransact" },
				OfferedForTradeStorageType = new ItemStorageType(80f),
				StorageTags = new string[2] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" },
				ItemStorageType = new ItemStorageType(20f),
				DefaultStorageSettings = "simplePortLocalStorage"
			},
			SensorType = new SensorType
			{
				DetectionTypeKey = "communicationSensor",
				Range = 24f,
				RangeAtNight = 24f
			},
			IntelligenceType = new IntelligenceType
			{
				StrengthRating = StrengthRating.None,
				IsMobile = false,
				CanAttack = false,
				CanUseWeapons = false,
				CanHunt = false,
				CanScout = false,
				CanExamine = false,
				CanPatrol = false,
				CanHaul = false,
				CanDoJobs = false,
				ServantForEntityTypeTag = "servesHumans"
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "storageCanopy",
							Offset = new Vector2(18f, -36f)
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "pierCanopy_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[2]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "storeHouseClay",
								Offset = new Vector2(18f, -36f)
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "pierSimple_construct_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 1f,
					SelectionShapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, 0f), 46f)
						{
							Offset = new Vector2(0f, 0f)
						}
					},
					Shapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, 0f), 32f)
						{
							Offset = new Vector2(20f, -40f)
						}
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "sturdyConstruction",
				SalvageProcess = "salvageCanopyPort",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:spoakBranchesTrimmed", 3 },
					{ "item:waterCaneStem", 3 },
					{ "item:daysheenLeaves", 2 }
				},
				Repair = "buildingRepairCustomProcess"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:largePier")
		{
			Name = "Port (large)",
			SummaryDescription = "Large port. Allows us to sell goods to other settlements",
			Description = "Has a pier where small boats and barges can moor and a storehouse with large capacity where goods intended for sale can be placed.",
			ThumbnailSmall = "HUD_thumbnail_simplePier",
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			CategoryKey = "miscellaneous",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TerminalType = new TerminalType
			{
				TypeOfTerminal = TerminalType.TypesOfTerminal.Pier
			},
			ContainerType = new TerminalContainerType
			{
				CanTransactWithTags = new string[1] { "humanTransact" },
				OfferedForTradeStorageType = new ItemStorageType(1000f),
				ItemStorageType = new ItemStorageType(2f)
			},
			SensorType = new SensorType
			{
				DetectionTypeKey = "communicationSensor",
				Range = 24f,
				RangeAtNight = 24f
			},
			IntelligenceType = new IntelligenceType
			{
				StrengthRating = StrengthRating.None,
				IsMobile = false,
				CanAttack = false,
				CanUseWeapons = false,
				CanHunt = false,
				CanScout = false,
				CanExamine = false,
				CanPatrol = false,
				CanHaul = false,
				CanDoJobs = false,
				ServantForEntityTypeTag = "servesHumans"
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "storeHouseClay",
							Offset = new Vector2(18f, -36f)
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "pierSimple_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[2]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "storeHouseClay",
								Offset = new Vector2(18f, -36f)
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "storeHouseClay_construct",
								Offset = new Vector2(18f, -36f)
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "pierSimple_construct_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 1f,
					SelectionShapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, 0f), 46f)
						{
							Offset = new Vector2(0f, 0f)
						}
					},
					Shapes = new CollideShape2D[2]
					{
						new CollideShape2D(new Vector2(0f, 0f), 20f)
						{
							Offset = new Vector2(25f, -40f)
						},
						new CollideShape2D(new Vector2(0f, 0f), 10f)
						{
							Offset = new Vector2(-4f, -40f)
						}
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "sturdyConstruction",
				SalvageProcess = "salvageSimplePort",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:spoakBranchesTrimmed", 3 },
					{ "item:spoakShingles", 1 },
					{ "item:solidMudBrick", 5 },
					{ "item:waterCaneStem", 3 }
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:landingImprovised")
		{
			Name = "Landing (improvised)",
			SummaryDescription = "Place for boats to moor transfer passengers. Goods can ONLY be received, not sold",
			Description = "This simple structure does not allow us to sell goods because it lacks a storehouse. It is only suited for receiving goods and embarking and disembarking passengers.",
			ThumbnailSmall = "HUD_thumbnail_landingImprovised",
			CategoryKey = "miscellaneous",
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TerminalType = new TerminalType
			{
				TypeOfTerminal = TerminalType.TypesOfTerminal.Pier
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "landingImprovised",
							Offset = new Vector2(-20f, -28f)
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "landingImprovised_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[2]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "landingImprovised",
								Offset = new Vector2(-20f, -28f)
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "landingImprovised_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 1f,
					SelectionShapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, 0f), 32f)
						{
							Offset = new Vector2(-6f, -6f)
						}
					},
					Shapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, 0f), 10f)
						{
							Offset = new Vector2(-20f, -30f)
						}
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "adequateConstruction",
				SalvageProcess = "salvageLandingImprovised",
				PartKeys = new SerializableDictionary<string, int> { { "item:waterCaneStem", 1 } },
				Repair = "buildingRepairCustomProcess"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:helipad")
		{
			Name = "Helipad",
			SummaryDescription = "Indicates an area for a VTOL aircraft to land",
			Description = "Any vertically landing aircraft will be looking for the big 'H' when selecting a spot to land on. This structure allows aircraft to deliver items and personnel to us, but does not make us able to sell items.",
			ThumbnailSmall = "HUD_thumbnail_helipad",
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			CategoryKey = "miscellaneous",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TerminalType = new TerminalType
			{
				TypeOfTerminal = TerminalType.TypesOfTerminal.Helipad
			},
			SensorType = new SensorType
			{
				DetectionTypeKey = "communicationSensor",
				Range = 24f,
				RangeAtNight = 24f
			},
			IntelligenceType = new IntelligenceType
			{
				StrengthRating = StrengthRating.None,
				IsMobile = false,
				CanAttack = false,
				CanUseWeapons = false,
				CanHunt = false,
				CanScout = false,
				CanExamine = false,
				CanPatrol = false,
				CanHaul = false,
				CanDoJobs = false,
				ServantForEntityTypeTag = "servesHumans"
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "helipad"
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "helipad_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[2]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "helipad"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "helipad_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 30f,
					SelectionShapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, 0f), 42f)
						{
							Offset = new Vector2(0f, 0f)
						}
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "advancedConstruction",
				SalvageProcess = "salvageHelipad",
				PartKeys = new SerializableDictionary<string, int> { { "item:stones", 1 } },
				Repair = "buildingRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:helipadBig")
		{
			Name = "Helipad (big)",
			SummaryDescription = "Storehouse and helipad for trading with VTOL aircraft",
			Description = "This landing spot has storage space for items intended for sale. This allows us to trade with aircraft from other colonies.",
			ThumbnailSmall = "HUD_thumbnail_helipadBig",
			TierOrArea = new TierOrArea
			{
				Tier = "advanced"
			},
			CategoryKey = "miscellaneous",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TerminalType = new TerminalType
			{
				TypeOfTerminal = TerminalType.TypesOfTerminal.Helipad
			},
			ContainerType = new TerminalContainerType
			{
				CanTransactWithTags = new string[1] { "humanTransact" },
				OfferedForTradeStorageType = new ItemStorageType(8f),
				StorageTags = new string[2] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" },
				ItemStorageType = new ItemStorageType(2f),
				DefaultStorageSettings = "simplePortLocalStorage"
			},
			SensorType = new SensorType
			{
				DetectionTypeKey = "communicationSensor",
				Range = 24f,
				RangeAtNight = 24f
			},
			IntelligenceType = new IntelligenceType
			{
				StrengthRating = StrengthRating.None,
				IsMobile = false,
				CanAttack = false,
				CanUseWeapons = false,
				CanHunt = false,
				CanScout = false,
				CanExamine = false,
				CanPatrol = false,
				CanHaul = false,
				CanDoJobs = false,
				ServantForEntityTypeTag = "servesHumans"
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "helipadBigStorehouse",
							Offset = new Vector2(49f, -28f)
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "helipadBig_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[2]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "helipadBigStorehouse",
								Offset = new Vector2(49f, -28f)
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "helipadBig_construct_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 1f,
					SelectionShapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, 0f), 42f)
						{
							Offset = new Vector2(-31f, 3f)
						}
					},
					Shapes = new CollideShape2D[2]
					{
						new CollideShape2D(new Vector2(0f, 0f), 26f)
						{
							Offset = new Vector2(48f, -27f)
						},
						new CollideShape2D(new Vector2(0f, 0f), 10f)
						{
							Offset = new Vector2(46f, -1f)
						}
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "advancedConstruction",
				SalvageProcess = "salvageHelipadBig",
				PartKeys = new SerializableDictionary<string, int> { { "item:structurePanels", 2 } },
				Repair = "buildingRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:heliportLarge")
		{
			Name = "Heliport",
			SummaryDescription = "Storehouse and helipad for trading with VTOL aircraft",
			Description = "This landing spot has storage space for items intended for sale. This allows us to trade with aircraft from other colonies.",
			ThumbnailSmall = "HUD_thumbnail_helipad",
			TierOrArea = new TierOrArea
			{
				Tier = "advanced"
			},
			CategoryKey = "miscellaneous",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TerminalType = new TerminalType
			{
				TypeOfTerminal = TerminalType.TypesOfTerminal.Helipad
			},
			ContainerType = new TerminalContainerType
			{
				CanTransactWithTags = new string[1] { "humanTransact" },
				OfferedForTradeStorageType = new ItemStorageType(1000f),
				StorageTags = new string[2] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" },
				ItemStorageType = new ItemStorageType(2f),
				DefaultStorageSettings = "simplePortLocalStorage"
			},
			SensorType = new SensorType
			{
				DetectionTypeKey = "communicationSensor",
				Range = 24f,
				RangeAtNight = 24f
			},
			IntelligenceType = new IntelligenceType
			{
				StrengthRating = StrengthRating.None,
				IsMobile = false,
				CanAttack = false,
				CanUseWeapons = false,
				CanHunt = false,
				CanScout = false,
				CanExamine = false,
				CanPatrol = false,
				CanHaul = false,
				CanDoJobs = false,
				ServantForEntityTypeTag = "servesHumans"
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "helipadBigStorehouse",
							Offset = new Vector2(49f, -28f)
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "helipadBig_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[2]
				{
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "helipadBig_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "helipadBig_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 1f,
					SelectionShapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, 0f), 42f)
						{
							Offset = new Vector2(-31f, 3f)
						}
					},
					Shapes = new CollideShape2D[2]
					{
						new CollideShape2D(new Vector2(0f, 0f), 26f)
						{
							Offset = new Vector2(48f, -27f)
						},
						new CollideShape2D(new Vector2(0f, 0f), 10f)
						{
							Offset = new Vector2(46f, -1f)
						}
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "advancedConstruction",
				PartKeys = new SerializableDictionary<string, int> { { "item:structurePanels", 2 } }
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:fieldLab")
		{
			Name = "Field lab (deployed)",
			SummaryDescription = "Lab set up and ready to use for analyzing specimens and make new chemical substances",
			Description = "With the lab, we can make new enzymes to treat ingredients which would otherwise be inedible to us.\n The deployed field lab can be packed down and carried by ordering SALVAGE.",
			ThumbnailSmall = "HUD_thumbnail_fieldLab",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			CategoryKey = "production",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 1f,
					PadShape = CollidePrim.Circle,
					Shapes = new CollideShape2D[2]
					{
						new CollideShape2D(Vector2.Zero, 11f)
						{
							Offset = new Vector2(-17f, -12f)
						},
						new CollideShape2D(Vector2.Zero, 11f)
						{
							Offset = new Vector2(17f, -8f)
						}
					},
					SelectionShapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, 0f), 20f)
						{
							Offset = new Vector2(0f, 0f)
						}
					}
				}
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "fieldLab"
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "fieldLab_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[2]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "fieldLab"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "fieldLab_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					}
				}
			},
			ToolType = new ToolType
			{
				Durability = 1f,
				ToolHandling = ToolHandlingType.Stationary
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "advancedConstruction",
				SalvageProcess = "salvageFieldLab",
				PartKeys = new SerializableDictionary<string, int> { { "item:fieldLabPacked", 1 } },
				Repair = "tentRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:molecularAssembler")
		{
			Name = "Molecular assembler",
			SummaryDescription = "Creates complex products with molecular precision",
			Description = "The molecular assembler works by putting together molecules of matter in a bottom-up fashion. It can create the most advanced products in a short time, with minimal waste.",
			ThumbnailSmall = "HUD_thumbnail_sensorStructure",
			CategoryKey = "production",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "molecularAssembler"
						}
					}
				},
				ClientStateConditions = new ClientStateInfo[1]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "molecularAssembler"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					}
				}
			},
			ToolType = new ToolType
			{
				Durability = 1f,
				ToolHandling = ToolHandlingType.Stationary
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 8f,
					Shapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, 0f), 11f)
						{
							Offset = new Vector2(-5f, 6f)
						}
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "advancedConstruction",
				SalvageProcess = "salvageMolecularAssembler",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:vacuumChamber", 1 },
					{ "item:assemblerCabinet", 1 },
					{ "item:assemblerCooling", 1 }
				},
				Repair = "tentRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:radioHutImprovised")
		{
			Name = "Radio hut (improvised)",
			SummaryDescription = "An improvised radio station. Used for communication with other settlements",
			Description = "A shelter for the radio, quickly constructed for urgent communication. \nMost settlements maintain communication through radio when satellite communication is no longer available. If another radio station is within reach, they can be contacted in order to arrange trade deals.",
			ThumbnailSmall = "HUD_thumbnail_radioHut",
			CategoryKey = "miscellaneous",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			CommunicatorType = new CommunicatorType
			{
				Method = CommunicationMethod.Radio,
				Range = null
			},
			SensorType = new SensorType
			{
				DetectionTypeKey = "communicationSensor",
				Range = 24f,
				RangeAtNight = 24f
			},
			IntelligenceType = new IntelligenceType
			{
				StrengthRating = StrengthRating.None,
				IsMobile = false,
				CanAttack = false,
				CanUseWeapons = false,
				CanHunt = false,
				CanScout = false,
				CanExamine = false,
				CanPatrol = false,
				CanHaul = false,
				CanDoJobs = false,
				ServantForEntityTypeTag = "servesHumans"
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "radioHut"
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "radioHut_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[2]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "radioHut"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "radioHut_construct"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 5f,
					Shapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, 0f), 20f)
						{
							Offset = new Vector2(0f, 0f)
						}
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "adequateConstruction",
				SalvageProcess = "salvageRadioHutImprovised",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:radio", 1 },
					{ "item:radioAntenna", 1 },
					{ "item:shadeleafCanes", 3 },
					{ "item:spoakLeaves", 2 }
				},
				Repair = "buildingRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:radioHut")
		{
			Name = "Radio hut",
			SummaryDescription = "A simple radio station. Used for communication with other settlements",
			Description = "A durable structure which houses the radio. \nMost settlements maintain communication through radio when satellite communication is no longer available. If another radio station is within reach, they can be contacted in order to arrange trade deals.",
			ThumbnailSmall = "HUD_thumbnail_radioHut",
			CategoryKey = "miscellaneous",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			CommunicatorType = new CommunicatorType
			{
				Method = CommunicationMethod.Radio,
				Range = null
			},
			SensorType = new SensorType
			{
				DetectionTypeKey = "communicationSensor",
				Range = 24f,
				RangeAtNight = 24f
			},
			IntelligenceType = new IntelligenceType
			{
				StrengthRating = StrengthRating.None,
				IsMobile = false,
				CanAttack = false,
				CanUseWeapons = false,
				CanHunt = false,
				CanScout = false,
				CanExamine = false,
				CanPatrol = false,
				CanHaul = false,
				CanDoJobs = false,
				ServantForEntityTypeTag = "servesHumans"
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "radioHut"
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "radioHut_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[2]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "radioHut"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "radioHut_construct"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 5f,
					Shapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, 0f), 20f)
						{
							Offset = new Vector2(0f, 0f)
						}
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "adequateConstruction",
				SalvageProcess = "salvageRadioHut",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:radio", 1 },
					{ "item:radioAntenna", 1 },
					{ "item:shadeleafCanes", 3 },
					{ "item:spoakShingles", 2 }
				},
				Repair = "buildingRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:satelliteGroundStation")
		{
			Name = "Satellite ground station (deployed)",
			SummaryDescription = "Communicates with a satellite",
			Description = "This equipment can be used to communicate with other sites on the planet via the network of satellites in orbit. When deployed, we will be able to receive messages from all other sites.",
			ThumbnailSmall = "HUD_thumbnail_satelliteDish",
			CategoryKey = "miscellaneous",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TierOrArea = new TierOrArea
			{
				Tier = "advanced"
			},
			CommunicatorType = new CommunicatorType
			{
				Method = CommunicationMethod.Satellite,
				Range = null
			},
			SensorType = new SensorType
			{
				DetectionTypeKey = "communicationSensor",
				Range = 24f,
				RangeAtNight = 24f
			},
			IntelligenceType = new IntelligenceType
			{
				StrengthRating = StrengthRating.None,
				IsMobile = false,
				CanAttack = false,
				CanUseWeapons = false,
				CanHunt = false,
				CanScout = false,
				CanExamine = false,
				CanPatrol = false,
				CanHaul = false,
				CanDoJobs = false,
				ServantForEntityTypeTag = "servesHumans"
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "satelliteDish"
						}
					}
				},
				ClientStateConditions = new ClientStateInfo[1]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "satelliteDish"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 8f,
					Shapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, 0f), 11f)
						{
							Offset = new Vector2(2f, 8f)
						}
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "advancedConstruction",
				SalvageProcess = "salvageSatelliteGroundStation",
				PartKeys = new SerializableDictionary<string, int> { { "item:satelliteGroundStation", 1 } },
				Repair = "buildingRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:weatherStation")
		{
			Name = "Weather station (deployed)",
			SummaryDescription = "Gathers data about the current weather situation",
			Description = "",
			ThumbnailSmall = "HUD_thumbnail_weatherAntenna",
			TierOrArea = new TierOrArea
			{
				Tier = "advanced"
			},
			CategoryKey = "miscellaneous",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "weatherAntenna"
						}
					}
				},
				ClientStateConditions = new ClientStateInfo[1]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "weatherAntenna"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 4f,
					Shapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, 0f), 11f)
						{
							Offset = new Vector2(2f, 27f)
						}
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "advancedConstruction",
				SalvageProcess = "salvageWeatherStation",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:weatherStationMast", 1 },
					{ "item:weatherStationSensors", 1 }
				},
				Repair = "buildingRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:turnipHut")
		{
			Name = "Turnip hut",
			SummaryDescription = "Modest hut made from a hollow turnip shell",
			Description = "The shell of a grown turnip can be used as roof and walls in a small building. It needs to be thoroughly cleaned and placed on a foundation of firegrass sod.",
			ThumbnailSmall = "HUD_thumbnail_turnipHut",
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Comfort
			},
			CategoryKey = "shelter",
			ContainerType = new HomeContainerType
			{
				CanBeEnteredByTags = new string[2] { "humanTransact", "leafcutterTransact" },
				ResidenceType = new ResidenceType
				{
					LivingCapacity = 3,
					ComfortLevel = comfortLevel4
				},
				ItemStorageType = new ItemStorageType("isolated", 8f),
				StorageTags = new string[2] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" },
				DefaultStorageSettings = "homeStorage",
				HasRallyPointInCourtyard = false,
				UpgradesProfile = "survivalHome3People",
				Doors = new Vector2[1]
				{
					new Vector2(12f, 19f)
				}
			},
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "turnipHut"
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "turnipHut_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[2]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "turnipHut"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "turnipHut"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 10f,
					PadShape = CollidePrim.Circle,
					Shapes = new CollideShape2D[3]
					{
						new CollideShape2D(new Vector2(-14f, 3f), 22f),
						new CollideShape2D(new Vector2(0f, 0f), 23f),
						new CollideShape2D(new Vector2(18f, 1f), 16f)
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "sturdyConstruction",
				SalvageProcess = "salvageTurnipHut",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:firegrassSod", 1 },
					{ "item:sticks", 2 },
					{ "item:turnipShell", 1 }
				},
				Repair = "buildingRepairCustomProcess"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:clayHut")
		{
			Name = "Clay hut",
			SummaryDescription = "Small, comfortable building for 4 occupants",
			Description = "A simple, permanent building made from mudbricks that are carefully stacked and supported by a wooden framework. After constructing the walls and the roof, a clay plaster is applied to make the building waterproof.",
			ThumbnailSmall = "HUD_thumbnail_clayPolyhedron",
			CategoryKey = "shelter",
			ContainerType = new HomeContainerType
			{
				CanBeEnteredByTags = new string[2] { "humanTransact", "leafcutterTransact" },
				ResidenceType = new ResidenceType
				{
					LivingCapacity = 4,
					ComfortLevel = comfortLevel6
				},
				ItemStorageType = new ItemStorageType("isolated", 8f),
				StorageTags = new string[2] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" },
				DefaultStorageSettings = "homeStorage",
				HasRallyPointInCourtyard = false,
				UpgradesProfile = "basicHome4People",
				Doors = new Vector2[1]
				{
					new Vector2(-25f, 15f)
				}
			},
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic",
				Area = RatingTypes.Comfort
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "clayPolyhedron"
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "clayPolyhedron_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[3]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "clayPolyhedron"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "clayPolyhedron_construct"
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "clayPolyhedron_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "clayPolyhedron"
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "clayPolyhedron_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 40),
						ParticleEmitters = new ParticleEmitterEffect[1]
						{
							new ParticleEmitterEffect
							{
								ParticleSystemKey = "smallestSmoke",
								Offset = new Vector2(0f, -30f)
							}
						}
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 5f,
					PadShape = CollidePrim.Circle,
					Shapes = new CollideShape2D[3]
					{
						new CollideShape2D(new Vector2(19f, 7f), 22f),
						new CollideShape2D(new Vector2(-14f, 2f), 22f),
						new CollideShape2D(new Vector2(4f, 0f), 22f)
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "sturdyConstruction",
				SalvageProcess = "salvageClayHut",
				Repair = "buildingRepair",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:solidMudBrick", 5 },
					{ "item:spoakBranchesTrimmed", 1 },
					{ "item:spoakShingles", 1 },
					{ "item:stones", 1 }
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:caneHut")
		{
			Name = "Cane hut",
			SummaryDescription = "Handsome dwelling made from water cane. Houses 3 people.",
			Description = "In warmer, wetter areas where water cane is abundant, this building design is a good option. The hut is raised from the ground on pillars, making it suitable for wetlands. Some clay plaster keeps the wind out but the hut will not be very comfortable in cold regions.",
			ThumbnailSmall = "HUD_thumbnail_caneHut",
			CategoryKey = "shelter",
			ContainerType = new HomeContainerType
			{
				CanBeEnteredByTags = new string[2] { "humanTransact", "leafcutterTransact" },
				ResidenceType = new ResidenceType
				{
					LivingCapacity = 3,
					ComfortLevel = comfortLevel5
				},
				ItemStorageType = new ItemStorageType("isolated", 8f),
				StorageTags = new string[2] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" },
				DefaultStorageSettings = "homeStorage",
				HasRallyPointInCourtyard = false,
				UpgradesProfile = "basicHome3People",
				Doors = new Vector2[1]
				{
					new Vector2(-27f, 8f)
				}
			},
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic",
				Area = RatingTypes.Comfort
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "caneHut"
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "caneHut_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[2]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "caneHut"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "caneHut_construct"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 10f,
					PadShape = CollidePrim.Circle,
					Shapes = new CollideShape2D[3]
					{
						new CollideShape2D(new Vector2(19f, 7f), 22f),
						new CollideShape2D(new Vector2(-17f, 6f), 25f),
						new CollideShape2D(new Vector2(4f, 5f), 25f)
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "sturdyConstruction",
				SalvageProcess = "salvageCaneHut",
				Repair = "buildingRepair",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:solidMudBrick", 1 },
					{ "item:waterCaneStem", 7 }
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:sentry")
		{
			Name = "Sentry (deployed)",
			FormalName = "TRIAAD",
			SummaryDescription = "Autonomous gun turret for area defence",
			Description = "The TRIAAD is a stationary machine gun for area defence. It has sensors and a degree of AI for operating in all conditions, and has been optimized for an alien environment containing unknown threats. Its low power consumption and deep magazine makes it able to operate unsupervised for extended periods of time.",
			ThumbnailSmall = "HUD_thumbnail_sentry",
			ThumbnailBig = "sentry",
			ContainerType = new ReplenishContainerType
			{
				CanTransactWithTags = new string[1] { "humanTransact" },
				RequiresReplenishType = new RequiresReplenishType()
			},
			LocomotorType = new LocomotorType
			{
				MaxAngularSpeed = 1.8849558f,
				RotatorType = new RotatorType
				{
					BoneKeyName = "tower_joint"
				}
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Security
			},
			CategoryKey = "defense",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			RenderableType = new RenderableType
			{
				RenderAsModelType = new RenderAsModelType
				{
					AssetName = "sentry",
					ModelScale = 2.2f,
					DefaultInfo = new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "combatIdle" }
						}
					},
					AnimConditions = new AnimConditionInfo[3]
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
						},
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "combatIdle" }
							},
							ConditionSet = new AnimConditions
							{
								Modifiers = new BitMask64(typeof(AnimModifier), 6)
							}
						},
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "shoot" }
							},
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.Attacking,
								Modifiers = new BitMask64(typeof(AnimModifier))
							}
						}
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 0f,
					Shapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, 0f), 6f)
						{
							Offset = new Vector2(0f, 9f)
						}
					}
				}
			},
			BodyType = GameData.Instance.AllBodyTypes["sentry"],
			SensorType = new SensorType
			{
				Range = 420f,
				RangeAtNight = 420f,
				DetectionTypeKey = "motionSensor"
			},
			IntelligenceType = new IntelligenceType
			{
				StrengthRating = StrengthRating.LikeHumans,
				AggroRange = 300f,
				IsMobile = false,
				CanAttack = true,
				CanUseWeapons = false,
				CanHunt = false,
				CanScout = false,
				CanExamine = false,
				CanPatrol = false,
				CanHaul = false,
				CanDoJobs = false,
				HuntsVermin = true,
				ServantForEntityTypeTag = "servesHumans",
				Skills = new SerializableDictionary<string, float> { { "shooting", 0.9f } },
				IntrinsicWeapons = new string[1] { "item:sentryGun" }
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "advancedConstruction",
				SalvageProcess = "salvageSentry",
				PartKeys = new SerializableDictionary<string, int> { { "item:sentry", 1 } }
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:sprayGunSentry")
		{
			Name = "Spray gun sentry (deployed)",
			SummaryDescription = "Autonomous turret, modified with an improvised spray gun",
			Description = "We have dismounted the machine gun and jerry-rigged a fire extinguisher gun onto the turret, making it able to shoot a poisonous liquid at approaching twinklers. For ammunition, it uses cartridges filled with bush dragon poison.",
			ThumbnailSmall = "HUD_thumbnail_sentry",
			ThumbnailBig = "sentry",
			ContainerType = new ReplenishContainerType
			{
				CanTransactWithTags = new string[1] { "humanTransact" },
				RequiresReplenishType = new RequiresReplenishType()
			},
			LocomotorType = new LocomotorType
			{
				MaxAngularSpeed = 1.8849558f,
				RotatorType = new RotatorType
				{
					BoneKeyName = "tower_joint"
				}
			},
			CategoryKey = "defense",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Security
			},
			RenderableType = new RenderableType
			{
				RenderAsModelType = new RenderAsModelType
				{
					AssetName = "sentry",
					ModelScale = 2.2f,
					DefaultInfo = new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "combatIdle" }
						}
					},
					AnimConditions = new AnimConditionInfo[3]
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
						},
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "combatIdle" }
							},
							ConditionSet = new AnimConditions
							{
								Modifiers = new BitMask64(typeof(AnimModifier), 6)
							}
						},
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "shoot" }
							},
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.Attacking,
								Modifiers = new BitMask64(typeof(AnimModifier))
							}
						}
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 0f,
					Shapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, 0f), 6f)
						{
							Offset = new Vector2(0f, 9f)
						}
					}
				}
			},
			BodyType = GameData.Instance.AllBodyTypes["sentry"],
			SensorType = new SensorType
			{
				Range = 420f,
				RangeAtNight = 420f,
				DetectionTypeKey = "motionSensor"
			},
			IntelligenceType = new IntelligenceType
			{
				StrengthRating = StrengthRating.LikeHumans,
				AggroRange = 300f,
				IsMobile = false,
				CanAttack = true,
				CanUseWeapons = false,
				CanHunt = false,
				CanScout = false,
				CanExamine = false,
				CanPatrol = false,
				CanHaul = false,
				CanDoJobs = false,
				ServantForEntityTypeTag = "servesHumans",
				Skills = new SerializableDictionary<string, float> { { "shooting", 0.9f } },
				IntrinsicWeapons = new string[1] { "item:sentrySprayGun" }
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "advancedConstruction",
				SalvageProcess = "salvageSprayGunSentry",
				PartKeys = new SerializableDictionary<string, int> { { "item:spraySentry", 1 } },
				Repair = "buildingRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:shotgunSentry")
		{
			Name = "Shotgun sentry (deployed)",
			SummaryDescription = "Autonomous turret outfitted with a shotgun",
			Description = "We have dismounted the machine gun and attached a shotgun onto the turret.",
			ThumbnailSmall = "HUD_thumbnail_sentry",
			ThumbnailBig = "sentry",
			ContainerType = new ReplenishContainerType
			{
				CanTransactWithTags = new string[1] { "humanTransact" },
				RequiresReplenishType = new RequiresReplenishType()
			},
			LocomotorType = new LocomotorType
			{
				MaxAngularSpeed = 1.8849558f,
				RotatorType = new RotatorType
				{
					BoneKeyName = "tower_joint"
				}
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Security
			},
			CategoryKey = "defense",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			RenderableType = new RenderableType
			{
				RenderAsModelType = new RenderAsModelType
				{
					AssetName = "sentry",
					ModelScale = 2.2f,
					DefaultInfo = new AnimConditionInfo
					{
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "combatIdle" }
						}
					},
					AnimConditions = new AnimConditionInfo[3]
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
						},
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "combatIdle" }
							},
							ConditionSet = new AnimConditions
							{
								Modifiers = new BitMask64(typeof(AnimModifier), 6)
							}
						},
						new AnimConditionInfo
						{
							SoundAndAnimationSet = new RandomSoundAndAnimationSet
							{
								BaseAnimations = new string[1] { "shoot" }
							},
							ConditionSet = new AnimConditions
							{
								Action = AnimAction.Attacking,
								Modifiers = new BitMask64(typeof(AnimModifier))
							}
						}
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 0f,
					Shapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, 0f), 6f)
						{
							Offset = new Vector2(0f, 9f)
						}
					}
				}
			},
			BodyType = GameData.Instance.AllBodyTypes["sentry"],
			SensorType = new SensorType
			{
				Range = 420f,
				RangeAtNight = 420f,
				DetectionTypeKey = "motionSensor"
			},
			IntelligenceType = new IntelligenceType
			{
				StrengthRating = StrengthRating.LikeHumans,
				AggroRange = 300f,
				IsMobile = false,
				CanAttack = true,
				CanUseWeapons = false,
				CanHunt = false,
				CanScout = false,
				CanExamine = false,
				CanPatrol = false,
				CanHaul = false,
				CanDoJobs = false,
				ServantForEntityTypeTag = "servesHumans",
				Skills = new SerializableDictionary<string, float> { { "shooting", 0.9f } },
				IntrinsicWeapons = new string[1] { "item:sentryShotgun" }
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "advancedConstruction",
				PartKeys = new SerializableDictionary<string, int> { { "item:shotgunSentry", 1 } },
				Repair = "buildingRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:spikeTrap")
		{
			Name = "Spring trap (deployed)",
			SummaryDescription = "4 clenching iron spikes set off by a trigger plate",
			Description = "Bait can be put in the center of the trap or the trap can be placed without bait in an area frequented by the target animal. The trap is strong enough to kill most small animals and injure larger ones.",
			ThumbnailSmall = "HUD_thumbnail_spikeTrap",
			CategoryKey = "defense",
			Triggers = new TriggerType[1] { GameData.Instance.AllTriggerTypes["spikeTrapTrigger"] },
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			ContainerType = new StorageContainerType
			{
				CanTransactWithTags = new string[7] { "ratTransact", "humanTransact", "leafcutterTransact", "chickenTransact", "snatcherTransact", "twinklerTransact", "demonTreeTransact" },
				ItemStorageType = new ItemStorageType("exposed", 0.15f),
				DefaultStorageSettings = "trapBaitStorage"
			},
			SharedSpecialActions = new Pair<string, bool>[4]
			{
				new Pair<string, bool>("changeBaitToBlackpulp", second: true),
				new Pair<string, bool>("changeBaitToGlassyCreeper", second: true),
				new Pair<string, bool>("changeBaitToRatMeat", second: true),
				new Pair<string, bool>("changeBaitToNoBait", second: true)
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "metalSpikeTrapSet_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[3]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "metalSpikeTrapSprung"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "metalSpikeTrapSprung"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "metalSpikeTrapSprung"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 7)
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 15f,
					PadShape = CollidePrim.Circle,
					SelectionShapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, 0f), 14f)
						{
							Offset = new Vector2(0f, 0f)
						}
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "sturdyConstruction",
				SalvageProcess = "salvageSpikeTrap",
				PartKeys = new SerializableDictionary<string, int> { { "item:spikeTrap", 1 } },
				Repair = "buildingRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:deadfallTrap")
		{
			Name = "Deadfall trap",
			SummaryDescription = "A heavy rock supported by a stick. Triggered when the bait is taken.",
			Description = "This simple trap has an in-built area where the bait has to be placed. When an animal tugs at the bait, the rock falls down on top of it, killing it.",
			ThumbnailSmall = "HUD_thumbnail_deadfall",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			CategoryKey = "defense",
			Triggers = new TriggerType[1] { GameData.Instance.AllTriggerTypes["smallImprovisedTrapTrigger"] },
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			ContainerType = new StorageContainerType
			{
				CanTransactWithTags = new string[4] { "ratTransact", "humanTransact", "leafcutterTransact", "chickenTransact" },
				ItemStorageType = new ItemStorageType("exposed", 0.15f),
				DefaultStorageSettings = "trapBaitStorage"
			},
			SharedSpecialActions = new Pair<string, bool>[4]
			{
				new Pair<string, bool>("changeBaitToBlackpulp", second: true),
				new Pair<string, bool>("changeBaitToGlassyCreeper", second: true),
				new Pair<string, bool>("changeBaitToRatMeat", second: true),
				new Pair<string, bool>("changeBaitToNoBait", second: true)
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "deadfallSet"
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "deadfallSet_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[3]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "deadfallSet"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "deadfallSprung"
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "deadfallSprung_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "deadfallSprung"
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "deadfallSprung_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 7)
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 15f,
					PadShape = CollidePrim.Circle,
					SelectionShapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, 0f), 14f)
						{
							Offset = new Vector2(0f, 0f)
						}
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "ricketyConstruction",
				SalvageProcess = "salvageDeadfallTrap",
				PartKeys = new SerializableDictionary<string, int> { { "item:stones", 1 } },
				Repair = "buildingRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:springSnare")
		{
			Name = "Snare",
			SummaryDescription = "Simple trap that uses a noose connected to an elastic pole, triggered by touch",
			Description = "This trap type should be set up in areas often frequented by the target animal, such as game trails or feeding grounds. Because the trap does not have an in-built bait area, any bait must be placed separate from the trap.",
			ThumbnailSmall = "HUD_thumbnail_springSnare",
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Food
			},
			CategoryKey = "defense",
			Triggers = new TriggerType[1] { GameData.Instance.AllTriggerTypes["smallImprovisedTrapTrigger"] },
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "springSnareSet"
						}
					}
				},
				ClientStateConditions = new ClientStateInfo[3]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "springSnareSet"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "springSnareSprung"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "springSnareSprung"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 7)
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 15f,
					PadShape = CollidePrim.Circle,
					SelectionShapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, 0f), 14f)
						{
							Offset = new Vector2(0f, 0f)
						}
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "ricketyConstruction",
				SalvageProcess = "salvageSpringSnare",
				PartKeys = new SerializableDictionary<string, int> { { "item:shadeleafCanes", 1 } },
				Repair = "buildingRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:landMine")
		{
			Name = "Land mine (deployed)",
			SummaryDescription = "Explosive device triggered by pressure",
			Description = "When a sufficiently heavy target steps on the trigger, it will set off a flintlock mechanism which ignites the black powder charge. The explosion will be strong enough to kill most animals in the vicinity.",
			ThumbnailSmall = "HUD_thumbnail_placeholder",
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			CategoryKey = "defense",
			Triggers = new TriggerType[1] { GameData.Instance.AllTriggerTypes["mineTrigger"] },
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "wire"
						}
					}
				},
				ClientStateConditions = new ClientStateInfo[1]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "wire"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 15f,
					PadShape = CollidePrim.Circle,
					SelectionShapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, 0f), 7f)
						{
							Offset = new Vector2(0f, 0f)
						}
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "sturdyConstruction",
				SalvageProcess = "salvageLandMine",
				PartKeys = new SerializableDictionary<string, int> { { "item:landMine", 1 } },
				Repair = "buildingRepair"
			}
		});
		StorageContainerType storageContainerType = new StorageContainerType("underWater", 0.76f);
		storageContainerType.CanTransactWithTags = new string[1] { "humanTransact" };
		storageContainerType.DefaultStorageSettings = "fishTrapCage";
		storageContainerType.AllowStockpiling = false;
		ContainerType containerType = storageContainerType;
		GeometryLayoutType geometryLayoutType = new GeometryLayoutType();
		geometryLayoutType.Pad = 22f;
		geometryLayoutType.PadShape = CollidePrim.Circle;
		geometryLayoutType.SelectionShapes = new CollideShape2D[2]
		{
			new CollideShape2D(new Vector2(0f, 0f), 13f)
			{
				Offset = new Vector2(0f, 0f)
			},
			new CollideShape2D(new Vector2(0f, 0f), 19f)
			{
				Offset = new Vector2(4f, 21f)
			}
		};
		GeometryLayoutType geometryLayoutType2 = geometryLayoutType;
		StructureType structureType = new StructureType();
		structureType.BuildByPlayer = true;
		structureType.UsesAnchor = true;
		StructureType structureType2 = structureType;
		ToolType toolType = new ToolType();
		toolType.ToolTag = new string[1] { "fishTrap" };
		toolType.IsPseudoTool = true;
		ToolType toolType2 = toolType;
		listOfEntityTypes.Add(new EntityType("structure:fishTrapCreekSticks")
		{
			Name = "Fish weir (sticks)",
			SummaryDescription = "Simple, fence-like structure that can catch a large number of migrating fish.",
			Description = "This trap is designed to catch schools of fish as they move through a narrow stream. \nWe will inspect the weir periodically and bring any fish we find back to camp.",
			ThumbnailSmall = "HUD_thumbnail_fishWeir",
			StructureType = structureType2,
			CategoryKey = "production",
			ContainerType = containerType,
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Food
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "fishTrapWeirStraight",
							Offset = new Vector2(0f, -40f)
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "fishTrapWeirStraight_g"
					}
				}
			},
			ToolType = toolType2,
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 15f,
					PadShape = CollidePrim.Circle,
					SelectionShapes = new CollideShape2D[3]
					{
						new CollideShape2D(new Vector2(0f, 0f), 24f)
						{
							Offset = new Vector2(6f, -48f)
						},
						new CollideShape2D(new Vector2(0f, 0f), 20f)
						{
							Offset = new Vector2(-6f, -25f)
						},
						new CollideShape2D(new Vector2(0f, 0f), 20f)
						{
							Offset = new Vector2(-6f, -65f)
						}
					}
				}
			},
			CustomFields = new SerializableDictionary<string, PropertyResult>
			{
				{
					"isFishTrap",
					new PropertyResult
					{
						BoolResult = true
					}
				},
				{
					"fishType",
					new PropertyResult
					{
						StringResult = "item:carbonTail"
					}
				},
				{
					"fishTypeBulk",
					new PropertyResult
					{
						NumberResult = 0.07f
					}
				},
				{
					"spawnChance",
					new PropertyResult
					{
						NumberResult = 0.04f
					}
				},
				{
					"maxNoOfFishToSpawnAtATime",
					new PropertyResult
					{
						NumberResult = 5f
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "ricketyConstruction",
				SalvageProcess = "salvageFishTrapCreekSticks",
				PartKeys = new SerializableDictionary<string, int> { { "item:sticks", 5 } },
				Repair = "buildingRepairCustomProcess"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:fishTrapCreekNet")
		{
			Name = "Fish weir (netting)",
			SummaryDescription = "Two-way weir that can catch fish migrating in both directions",
			Description = "This trap has an ingenious design which catches schools of fish travelling both up and down the stream. \nWe will inspect the weir periodically and bring any fish we find back to camp.",
			ThumbnailSmall = "HUD_thumbnail_fishWeirNet",
			StructureType = structureType2,
			CategoryKey = "production",
			ContainerType = containerType,
			TierOrArea = new TierOrArea
			{
				Tier = "basic",
				Area = RatingTypes.Food
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "fishTrapWeirNet",
							Offset = new Vector2(0f, -40f)
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "fishTrapWeirStraight_g"
					}
				}
			},
			ToolType = toolType2,
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 15f,
					PadShape = CollidePrim.Circle,
					SelectionShapes = new CollideShape2D[3]
					{
						new CollideShape2D(new Vector2(0f, 0f), 31f)
						{
							Offset = new Vector2(-6f, -48f)
						},
						new CollideShape2D(new Vector2(0f, 0f), 23f)
						{
							Offset = new Vector2(-6f, -25f)
						},
						new CollideShape2D(new Vector2(0f, 0f), 25f)
						{
							Offset = new Vector2(-6f, -65f)
						}
					}
				}
			},
			CustomFields = new SerializableDictionary<string, PropertyResult>
			{
				{
					"isFishTrap",
					new PropertyResult
					{
						BoolResult = true
					}
				},
				{
					"fishType",
					new PropertyResult
					{
						StringResult = "item:carbonTail"
					}
				},
				{
					"fishTypeBulk",
					new PropertyResult
					{
						NumberResult = 0.07f
					}
				},
				{
					"spawnChance",
					new PropertyResult
					{
						NumberResult = 0.04f
					}
				},
				{
					"maxNoOfFishToSpawnAtATime",
					new PropertyResult
					{
						NumberResult = 7f
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "adequateConstruction",
				SalvageProcess = "salvageFishTrapCreekNet",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:fishingNet", 2 },
					{ "item:sticks", 3 }
				},
				Repair = "buildingRepairCustomProcess"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:fishTrapCoast")
		{
			Name = "Fish trap - Fyke",
			SummaryDescription = "Fish trap designed for catching the 'streak fin' in its coastal habitat",
			Description = "The trap consists of a cylindrical net with wings which guide the fish toward the entrance. \nWe will check the trap periodically to collect any captures.",
			ThumbnailSmall = "HUD_thumbnail_fishTrapFyke",
			StructureType = structureType2,
			ContainerType = containerType,
			CategoryKey = "production",
			TierOrArea = new TierOrArea
			{
				Tier = "basic",
				Area = RatingTypes.Food
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "fishTrapFyke_g"
					}
				}
			},
			ToolType = toolType2,
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 15f,
					PadShape = CollidePrim.Circle,
					SelectionShapes = new CollideShape2D[3]
					{
						new CollideShape2D(new Vector2(0f, 0f), 13f)
						{
							Offset = new Vector2(0f, 0f)
						},
						new CollideShape2D(new Vector2(0f, 0f), 19f)
						{
							Offset = new Vector2(-14f, 31f)
						},
						new CollideShape2D(new Vector2(0f, 0f), 13f)
						{
							Offset = new Vector2(14f, 15f)
						}
					}
				}
			},
			CustomFields = new SerializableDictionary<string, PropertyResult>
			{
				{
					"isFishTrap",
					new PropertyResult
					{
						BoolResult = true
					}
				},
				{
					"fishType",
					new PropertyResult
					{
						StringResult = "item:streakFin"
					}
				},
				{
					"fishTypeBulk",
					new PropertyResult
					{
						NumberResult = 0.07f
					}
				},
				{
					"spawnChance",
					new PropertyResult
					{
						NumberResult = 0.05f
					}
				},
				{
					"maxNoOfFishToSpawnAtATime",
					new PropertyResult
					{
						NumberResult = 5f
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "adequateConstruction",
				SalvageProcess = "salvageFishTrapCoast",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:fishTrapHoopNet", 1 },
					{ "item:fishingNet", 1 }
				},
				Repair = "buildingRepairCustomProcess"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:fishTrapShoreBasket")
		{
			Name = "Fish trap - Basket",
			SummaryDescription = "Simple fish trap designed for catching the 'carbon tail'",
			Description = "A wicker basket which allows fish to enter through a funnel and hampers their escape. \nWe will check the trap periodically to collect any captures.",
			ThumbnailSmall = "HUD_thumbnail_fishTrapCylinder",
			StructureType = structureType2,
			CategoryKey = "production",
			ContainerType = containerType,
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Food
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "fishTrapCylinderSmall_g"
					}
				}
			},
			ToolType = toolType2,
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = geometryLayoutType2
			},
			CustomFields = new SerializableDictionary<string, PropertyResult>
			{
				{
					"isFishTrap",
					new PropertyResult
					{
						BoolResult = true
					}
				},
				{
					"fishType",
					new PropertyResult
					{
						StringResult = "item:carbonTail"
					}
				},
				{
					"fishTypeBulk",
					new PropertyResult
					{
						NumberResult = 0.07f
					}
				},
				{
					"spawnChance",
					new PropertyResult
					{
						NumberResult = 0.05f
					}
				},
				{
					"maxNoOfFishToSpawnAtATime",
					new PropertyResult
					{
						NumberResult = 3f
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "ricketyConstruction",
				SalvageProcess = "salvageFishTrapShoreBasket",
				PartKeys = new SerializableDictionary<string, int> { { "item:fishTrapBasket", 1 } },
				Repair = "buildingRepairCustomProcess"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:fishTrapShoreHoopNet")
		{
			Name = "Fish trap - Hoop net",
			SummaryDescription = "Good quality fish trap specially designed for catching the 'carbon tail'",
			Description = "A cylindrical trap that allows fish to enter from both ends but restricts their escape. Made from cotton netting and wooden hoops. \nWe will check the trap periodically to collect any captures.",
			ThumbnailSmall = "HUD_thumbnail_fishTrapHoopNet",
			StructureType = structureType2,
			CategoryKey = "production",
			ContainerType = containerType,
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Food
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "fishTrapHoopNet_g"
					}
				}
			},
			ToolType = toolType2,
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = geometryLayoutType2
			},
			CustomFields = new SerializableDictionary<string, PropertyResult>
			{
				{
					"isFishTrap",
					new PropertyResult
					{
						BoolResult = true
					}
				},
				{
					"fishType",
					new PropertyResult
					{
						StringResult = "item:carbonTail"
					}
				},
				{
					"fishTypeBulk",
					new PropertyResult
					{
						NumberResult = 0.07f
					}
				},
				{
					"spawnChance",
					new PropertyResult
					{
						NumberResult = 0.05f
					}
				},
				{
					"maxNoOfFishToSpawnAtATime",
					new PropertyResult
					{
						NumberResult = 4f
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "adequateConstruction",
				SalvageProcess = "salvageFishTrapShoreHoopNet",
				PartKeys = new SerializableDictionary<string, int> { { "item:fishTrapHoopNet", 1 } },
				Repair = "buildingRepairCustomProcess"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:smallPlot")
		{
			Name = "Small plot",
			SummaryDescription = "A small plot for farming.",
			Description = "After sowing/planting, this area can provide crops depending on the seeds used. The crops will need to be weeded periodically, so we must ensure we have tools and workforce available to tend to the plot during the growing cycle. Once the crops are ripe, we have to harvest them in time to avoid losing them.",
			ThumbnailSmall = "HUD_thumbnail_plotYellowShrubs",
			CategoryKey = "production",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic",
				Area = RatingTypes.Food
			},
			ToolType = new ToolType
			{
				IsPseudoTool = true
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "plotPlowed_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[27]
				{
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plotPlowed_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plotPlowed_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plotPlowed_g"
						},
						Conditions = new BitMask64(typeof(StateModifier))
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plotPlowed_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 21)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plotShrubYoung_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 11, 21)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plotShrubGrown_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 12, 21)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plotPlowedWeeds_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 8, 21)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plotShrubYoungWeeds_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 11, 8, 21)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plotShrubWeeds_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 12, 8, 21)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plotShrubGrownDecayed_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 18, 21)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plotShrubGrownDecayed_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 18, 8, 21)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plotPlowed_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 22)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plotShrub2Young_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 11, 22)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plotShrub2Grown_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 12, 22)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plotPlowedWeeds_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 8, 22)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plotShrub2YoungWeeds_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 11, 8, 22)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plotShrub2Weeds_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 12, 8, 22)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plotShrubGrownDecayed_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 18, 22)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plotShrubGrownDecayed_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 18, 8, 22)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plotPlowed_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 23)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plotCottonYoung_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 11, 23)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plotCottonGrown_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 12, 23)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plotPlowedWeeds_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 8, 23)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plotCottonYoungWeeds_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 11, 8, 23)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plotCottonGrownWeeds_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 12, 8, 23)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plotCottonGrownDecayed_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 18, 23)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plotCottonGrownDecayed_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 18, 8, 23)
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 50f,
					SelectionShapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(-72f, -48f), new Vector2(72f, 48f))
					}
				}
			},
			SpecialActionLocks = new Pair<string, bool>[10]
			{
				new Pair<string, bool>("useOrganicFertilizer", second: false),
				new Pair<string, bool>("useGuanoFertilizer", second: true),
				new Pair<string, bool>("stopUsingGuanoFertilizer", second: false),
				new Pair<string, bool>("stopUsingOrganicFertilizer", second: true),
				new Pair<string, bool>("growGlassyCreeperPodsInSmallPlot", second: true),
				new Pair<string, bool>("growCrystalBerriesInSmallPlot", second: true),
				new Pair<string, bool>("growCottonInSmallPlot", second: true),
				new Pair<string, bool>("stopGrowingCrystalBerriesInSmallPlot", second: false),
				new Pair<string, bool>("stopGrowingGlassyCreeperPodsInSmallPlot", second: false),
				new Pair<string, bool>("stopGrowingCottonInSmallPlot", second: false)
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "dirt",
				SalvageProcess = "salvageSmallPlot"
			},
			IsSelectable = true
		});
		listOfEntityTypes.Add(new EntityType("structure:largePlot")
		{
			Name = "Large plot",
			SummaryDescription = "A large plot for farming.",
			Description = "After sowing/planting, this area can provide crops depending on the seeds used. The crops will need to be weeded periodically, so we must ensure we have tools and workforce available to tend to the plot during the growing cycle. Once the crops are ripe, we have to harvest them in time to avoid losing them.",
			ThumbnailSmall = "HUD_thumbnail_plotYellowShrubs",
			CategoryKey = "production",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic",
				Area = RatingTypes.Food
			},
			ToolType = new ToolType
			{
				IsPseudoTool = true
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "plotLargePlowed_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[20]
				{
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plotLargePlowed_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plotLargePlowed_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plotLargePlowed_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 21)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plotLargeShrubYoung_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 11, 21)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plotLargeShrubGrown_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 12, 21)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plotLargePlowedWeeds_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 8, 21)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plotLargeShrubYoungWeeds_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 11, 8, 21)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plotLargeShrubWeeds_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 12, 8, 21)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plotLargePlowed_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 22)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plotLargeShrub2Young_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 11, 22)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plotLargeShrub2Grown_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 12, 22)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plotLargePlowedWeeds_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 8, 22)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plotLargeShrub2YoungWeeds_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 11, 8, 22)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plotLargeShrub2YoungWeeds_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 12, 8, 22)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plotLargePlowed_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 23)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plotLargeCottonYoung_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 11, 23)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plotLargeCottonGrown_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 12, 23)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plotLargePlowedWeeds_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 8, 23)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plotLargeCottonYoungWeeds_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 11, 8, 23)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "plotLargeCottonYoungWeeds_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 12, 8, 23)
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 78f,
					SelectionShapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(-120f, -72f), new Vector2(120f, 72f))
					}
				}
			},
			SpecialActionLocks = new Pair<string, bool>[10]
			{
				new Pair<string, bool>("useLargeOrganicFertilizer", second: false),
				new Pair<string, bool>("useLargeGuanoFertilizer", second: true),
				new Pair<string, bool>("stopUsingLargeGuanoFertilizer", second: false),
				new Pair<string, bool>("stopUsingLargeOrganicFertilizer", second: true),
				new Pair<string, bool>("growGlassyCreeperPodsInLargePlot", second: true),
				new Pair<string, bool>("growCrystalBerriesInLargePlot", second: true),
				new Pair<string, bool>("growCottonInLargePlot", second: true),
				new Pair<string, bool>("stopGrowingCrystalBerriesInLargePlot", second: false),
				new Pair<string, bool>("stopGrowingGlassyCreeperPodsInLargePlot", second: false),
				new Pair<string, bool>("stopGrowingCottonInLargePlot", second: false)
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "dirt",
				SalvageProcess = "salvageLargePlot"
			},
			IsSelectable = true
		});
		listOfEntityTypes.Add(new EntityType("structure:improvisedGreenhouse")
		{
			Name = "Greenhouse (primitive)",
			SummaryDescription = "Greenhouse made with primitive materials",
			Description = "The covering is made from sheets of turnip entrails which have enough translucency to trap the sun's heat. This makes us able to grow the finger fruit. We can also get a quicker harvest of ordinary crops which benefit from the higher temperature.",
			ThumbnailSmall = "HUD_thumbnail_greenhouseImprovised",
			CategoryKey = "production",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic",
				Area = RatingTypes.Food
			},
			ToolType = new ToolType
			{
				IsPseudoTool = true
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "greenhouseDomeImprovised"
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "greenhouseDomeImprovisedNoCrops_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[14]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "greenhouseDomeImprovised"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "greenhouseDomeImprovisedNoCrops_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "greenhouseDomeImprovised"
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "greenhouseDomeImprovisedNoCrops_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 21)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "greenhouseDomeImprovised"
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "greenhouseDomeImprovisedCrops_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 11, 21)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "greenhouseDomeImprovised"
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "greenhouseDomeImprovisedCrops_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 12, 21)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "greenhouseDomeImprovised"
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "greenhouseDomeImprovisedCrops_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 8, 21)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "greenhouseDomeImprovised"
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "greenhouseDomeImprovisedCrops_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 11, 8, 21)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "greenhouseDomeImprovised"
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "greenhouseDomeImprovisedCrops_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 12, 8, 21)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "greenhouseDomeImprovised"
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "greenhouseDomeImprovisedNoCrops_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 22)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "greenhouseDomeImprovised"
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "greenhouseDomeImprovisedCrops_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 11, 22)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "greenhouseDomeImprovised"
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "greenhouseDomeImprovisedCrops_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 12, 22)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "greenhouseDomeImprovised"
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "greenhouseDomeImprovisedCrops_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 8, 22)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "greenhouseDomeImprovised"
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "greenhouseDomeImprovisedCrops_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 11, 8, 22)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "greenhouseDomeImprovised"
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "greenhouseDomeImprovisedCrops_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 12, 8, 22)
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 10f,
					PadShape = CollidePrim.Circle,
					SelectionShapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, -2f), 25f)
					},
					Shapes = new CollideShape2D[2]
					{
						new CollideShape2D(new Vector2(-10f, -2f), 20f),
						new CollideShape2D(new Vector2(10f, -2f), 20f)
					}
				}
			},
			SpecialActionLocks = new Pair<string, bool>[10]
			{
				new Pair<string, bool>("useOrganicFertilizer", second: false),
				new Pair<string, bool>("useGuanoFertilizer", second: true),
				new Pair<string, bool>("stopUsingGuanoFertilizer", second: false),
				new Pair<string, bool>("stopUsingOrganicFertilizer", second: true),
				new Pair<string, bool>("growFingerFruitInGreenhouse", second: true),
				new Pair<string, bool>("growCrystalBerriesInGreenhouse", second: true),
				new Pair<string, bool>("growGlassyCreeperPodsInGreenhouse", second: true),
				new Pair<string, bool>("stopGrowingCrystalBerriesInGreenhouse", second: false),
				new Pair<string, bool>("stopGrowingGlassyCreeperPodsInGreenhouse", second: false),
				new Pair<string, bool>("stopGrowingFingerFruitInGreenhouse", second: false)
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "sturdyConstruction",
				SalvageProcess = "salvageImprovisedGreenhouse",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:improvisedGreenHouseCover", 2 },
					{ "item:shadeleafCanes", 3 }
				},
				Repair = "buildingRepair"
			},
			IsSelectable = true
		});
		listOfEntityTypes.Add(new EntityType("structure:greenhouse")
		{
			Name = "Greenhouse",
			SummaryDescription = "Greenhouse made with advanced materials",
			Description = "The greenhouse uses sheets of diamond glass left by the Ancestors. Makes us able to grow the finger fruit. Also speeds up the growth cycle of other crops which benefit from the higher temperature and protection.",
			ThumbnailSmall = "HUD_thumbnail_greenhouse",
			CategoryKey = "production",
			StructureType = new StructureType
			{
				BuildByPlayer = false
			},
			TierOrArea = new TierOrArea
			{
				Tier = "medium",
				Area = RatingTypes.Food
			},
			ToolType = new ToolType
			{
				IsPseudoTool = true
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "greenhouse2"
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "greenhouse2_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[2]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "greenhouse2"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "greenhouse2_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 10f,
					PadShape = CollidePrim.Circle,
					SelectionShapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, -2f), 25f)
					},
					Shapes = new CollideShape2D[2]
					{
						new CollideShape2D(new Vector2(-10f, -2f), 20f),
						new CollideShape2D(new Vector2(10f, -2f), 20f)
					}
				}
			},
			SpecialActionLocks = new Pair<string, bool>[10]
			{
				new Pair<string, bool>("useOrganicFertilizer", second: false),
				new Pair<string, bool>("useGuanoFertilizer", second: true),
				new Pair<string, bool>("stopUsingGuanoFertilizer", second: false),
				new Pair<string, bool>("stopUsingOrganicFertilizer", second: true),
				new Pair<string, bool>("growFingerFruitInGreenhouse", second: true),
				new Pair<string, bool>("growCrystalBerriesInGreenhouse", second: true),
				new Pair<string, bool>("growGlassyCreeperPodsInGreenhouse", second: true),
				new Pair<string, bool>("stopGrowingCrystalBerriesInGreenhouse", second: false),
				new Pair<string, bool>("stopGrowingGlassyCreeperPodsInGreenhouse", second: false),
				new Pair<string, bool>("stopGrowingFingerFruitInGreenhouse", second: false)
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "advancedConstruction",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:diamondGlass", 5 },
					{ "item:shadeleafCanes", 3 }
				},
				Repair = "buildingRepair"
			},
			IsSelectable = true
		});
		listOfEntityTypes.Add(new EntityType("structure:scarecrow")
		{
			Name = "Pest repellent (twinkler scent)",
			SummaryDescription = "Small rig that uses twinkler scent to ward off binal rats",
			Description = "Can be placed in areas that we want free from binal rats such as foodstores. The repellent effect wears off after some time at which point the repellent device has to be rebuilt using fresh twinkler pheromones.",
			ThumbnailSmall = "HUD_thumbnail_pestRepellentTwinkler",
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Security
			},
			CategoryKey = "defense",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "pestRepellentStructure"
						}
					}
				},
				ClientStateConditions = new ClientStateInfo[1]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "pestRepellentStructure"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					GridAlignedPlacement = false,
					Pad = 0f,
					Shapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, 0f), 7f)
						{
							Offset = new Vector2(0f, 0f)
						}
					}
				}
			},
			ThreatType = new ThreatType
			{
				StrengthRating = StrengthRating.LikeHumans
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "ricketyConstruction",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:twinklerPlating", 1 },
					{ "item:twinklerPheromone", 1 }
				},
				Repair = "buildingRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:kiln")
		{
			Name = "Kiln",
			SummaryDescription = "A large oven for making charcoal, bricks or other products",
			Description = "This simple kiln is made from clay. Fueled with ordinary firewood it can carbonize other batches of firewood, making charcoal. The kiln can also be used for producing mudbricks (at a faster rate than sun drying) and for firing pottery.",
			ThumbnailSmall = "HUD_thumbnail_kilnImprovised",
			CategoryKey = "production",
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			ToolType = new ToolType
			{
				ToolTag = new string[1] { "kiln" },
				Durability = 0.9f,
				ToolHandling = ToolHandlingType.Stationary,
				PrepareProcess = "kilnSmoke"
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "kilnImprovised"
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "kilnImprovised_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[3]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "kilnImprovised"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "kilnImprovised_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "kilnImprovised"
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "kilnImprovised_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 39),
						ParticleEmitters = new ParticleEmitterEffect[2]
						{
							new ParticleEmitterEffect
							{
								ParticleSystemKey = "smallestSmoke",
								Offset = new Vector2(0f, -30f)
							},
							new ParticleEmitterEffect
							{
								ParticleSystemKey = "tinyFire",
								Offset = new Vector2(10f, 10f)
							}
						}
					}
				}
			},
			ContainerType = new ToolContainerType
			{
				CanTransactWithTags = new string[1] { "humanTransact" },
				ProductionOutputStorageType = new ItemStorageType(2.5f)
				{
					FullStatePercentage = 0.1f,
					HalfFullStatePercentage = 0.05f
				},
				RequiresReplenishType = new RequiresReplenishType
				{
					ReplenishProcess = "refuelKiln",
					RequiresFuelType = new RequiresFuelType
					{
						MaxFuel = 1f,
						FuelTypeTag = "fuelForCampfire",
						BurnRatePerDay = 3f
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 10f,
					PadShape = CollidePrim.Circle,
					Shapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, -7f), 24f)
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "sturdyConstruction",
				SalvageProcess = "salvageKiln",
				PartKeys = new SerializableDictionary<string, int> { { "item:stones", 1 } },
				Repair = "buildingRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:kilnImprovisedSmall")
		{
			Name = "Oven (improvised)",
			SummaryDescription = "A small oven for firing pottery or baking",
			Description = "This small kiln is made from stones. It is not very efficient but useful in a survival situation.",
			ThumbnailSmall = "HUD_thumbnail_kilnImprovisedSmall",
			CategoryKey = "production",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			ToolType = new ToolType
			{
				ToolTag = new string[1] { "kiln" },
				Durability = 0.9f,
				ToolHandling = ToolHandlingType.Stationary,
				PrepareProcess = "kilnSmoke"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "kilnImprovisedSmall"
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "kilnImprovisedSmall_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[3]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "kilnImprovisedSmall"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "kilnImprovisedSmall_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "kilnImprovisedSmall"
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "kilnImprovisedSmall_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 39),
						ParticleEmitters = new ParticleEmitterEffect[2]
						{
							new ParticleEmitterEffect
							{
								ParticleSystemKey = "smallestSmoke",
								Offset = new Vector2(-5f, -2f)
							},
							new ParticleEmitterEffect
							{
								ParticleSystemKey = "tinyFire",
								Offset = new Vector2(-5f, 3f)
							}
						}
					}
				}
			},
			ContainerType = new ToolContainerType
			{
				CanTransactWithTags = new string[1] { "humanTransact" },
				ProductionOutputStorageType = new ItemStorageType(1f)
				{
					FullStatePercentage = 0.1f,
					HalfFullStatePercentage = 0.05f
				},
				RequiresReplenishType = new RequiresReplenishType
				{
					ReplenishProcess = "refuelKiln",
					RequiresFuelType = new RequiresFuelType
					{
						MaxFuel = 1f,
						FuelTypeTag = "fuelForCampfire",
						BurnRatePerDay = 2f
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 10f,
					PadShape = CollidePrim.Circle,
					Shapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, -7f), 17f)
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "adequateConstruction",
				SalvageProcess = "salvageKilnImprovisedSmall",
				PartKeys = new SerializableDictionary<string, int> { { "item:stones", 3 } },
				Repair = "buildingRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:goldFurnace")
		{
			Name = "Gold furnace",
			SummaryDescription = "Can melt metals such as gold which have melting points up to 1100 C / 2012 F",
			Description = "A flat structure with a chimney; the technical name is 'reverberatory furnace'. Made from tiles of clay which can withstand the intense heat required to melt gold. The metal is placed in a hearth which lays next to the firebox. Firewood is used as fuel, and a natural draft carries the flames from the firebox over to the metal. The melted metal comes out from a tap in the bottom.",
			ThumbnailSmall = "HUD_thumbnail_goldFurnace",
			CategoryKey = "production",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ToolType = new ToolType
			{
				Durability = 0.9f,
				ToolHandling = ToolHandlingType.Stationary,
				PrepareProcess = "kilnSmoke"
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "goldFurnace"
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "goldFurnace_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[3]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "goldFurnace"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "goldFurnace_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "goldFurnace"
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "goldFurnace_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 39),
						ParticleEmitters = new ParticleEmitterEffect[2]
						{
							new ParticleEmitterEffect
							{
								ParticleSystemKey = "smallestSmoke",
								Offset = new Vector2(16f, -30f)
							},
							new ParticleEmitterEffect
							{
								ParticleSystemKey = "tinyFire",
								Offset = new Vector2(-10f, 10f)
							}
						}
					}
				}
			},
			ContainerType = new ToolContainerType
			{
				CanTransactWithTags = new string[1] { "humanTransact" },
				ProductionOutputStorageType = new ItemStorageType(1.5f)
				{
					FullStatePercentage = 0.1f,
					HalfFullStatePercentage = 0.05f
				},
				RequiresReplenishType = new RequiresReplenishType
				{
					ReplenishProcess = "refuelKiln",
					RequiresFuelType = new RequiresFuelType
					{
						MaxFuel = 1f,
						FuelTypeTag = "fuelForCampfire",
						BurnRatePerDay = 3f
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
						new CollideShape2D(new Vector2(0f, 7f), 12f),
						new CollideShape2D(new Vector2(16f, -9f), 14f)
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "sturdyConstruction",
				SalvageProcess = "salvageGoldFurnace",
				PartKeys = new SerializableDictionary<string, int> { { "item:firebricks", 3 } },
				Repair = "buildingRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:improvisedSmithy")
		{
			Name = "Smithy (improvised)",
			SummaryDescription = "Simple furnace for smelting / heating iron and a rock anvil. Fuel:charcoal",
			Description = "The primitive furnace is built from clay. To reach a sufficient temperature, it requires charcoal as fuel and an air supply tool such as a bellows. After smelting the ore in the furnace, the metal is shaped on the anvil with a hammer.",
			ThumbnailSmall = "HUD_thumbnail_forgeImprovised",
			CategoryKey = "production",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			ToolType = new ToolType
			{
				ToolTag = new string[1] { "furnace" },
				Durability = 0.9f,
				ToolHandling = ToolHandlingType.Stationary,
				PrepareProcess = "forgeSmoke"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "forgeImprovised"
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "forgeImprovised_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[3]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "forgeImprovised"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "forgeImprovised_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "forgeImprovised"
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "forgeImprovised_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 39),
						ParticleEmitters = new ParticleEmitterEffect[2]
						{
							new ParticleEmitterEffect
							{
								ParticleSystemKey = "smallestSmoke",
								Offset = new Vector2(-9f, -26f)
							},
							new ParticleEmitterEffect
							{
								ParticleSystemKey = "tinyFire",
								Offset = new Vector2(-15f, 0f)
							}
						}
					}
				}
			},
			ContainerType = new WorkshopContainerType
			{
				CanTransactWithTags = new string[1] { "humanTransact" },
				ItemStorageType = new ItemStorageType("isolated", 1f),
				StorageTags = new string[2] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" },
				DefaultStorageSettings = "forgeStorage",
				RequiresReplenishType = new RequiresReplenishType
				{
					ReplenishProcess = "refuelSmithy",
					RequiresFuelType = new RequiresFuelType
					{
						MaxFuel = 1f,
						FuelTypeKeyName = "item:charcoal",
						BurnRatePerDay = 2f
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
						new CollideShape2D(new Vector2(-9f, -8f), 14f),
						new CollideShape2D(new Vector2(17f, 2f), 14f)
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "sturdyConstruction",
				SalvageProcess = "salvageImprovisedSmithy",
				Repair = "buildingRepair",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:solidMudBrick", 3 },
					{ "item:stones", 1 }
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:simpleSmithy")
		{
			Name = "Smithy (simple)",
			SummaryDescription = "Simple furnace for smelting / heating iron and an iron anvil. Fuel: charcoal",
			Description = "A step up from the improvised smithy with its rock anvil, this smithy equipped with an iron anvil can make more sophisticated iron objects. \n \nThe primitive bloomery furnace is built from clay. It requires charcoal as fuel and an air supply tool such as a bellows. After smelting iron ore in the furnace, a solid iron bloom is worked on the anvil with a hammer, removing slag until low-carbon 'wrought iron' is produced. This can be further worked into iron tools.",
			ThumbnailSmall = "HUD_thumbnail_forgeSimple",
			CategoryKey = "production",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			ToolType = new ToolType
			{
				ToolTag = new string[1] { "furnace" },
				Durability = 0.9f,
				ToolHandling = ToolHandlingType.Stationary,
				PrepareProcess = "forgeSmoke"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "forgeSimple"
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "forgeSimple_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[3]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "forgeSimple"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "forgeSimple_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "forgeSimple"
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "forgeSimple_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 39),
						ParticleEmitters = new ParticleEmitterEffect[2]
						{
							new ParticleEmitterEffect
							{
								ParticleSystemKey = "smallestSmoke",
								Offset = new Vector2(-11f, -22f)
							},
							new ParticleEmitterEffect
							{
								ParticleSystemKey = "tinyFire",
								Offset = new Vector2(-19f, 4f)
							}
						}
					}
				}
			},
			ContainerType = new WorkshopContainerType
			{
				CanTransactWithTags = new string[1] { "humanTransact" },
				ItemStorageType = new ItemStorageType("isolated", 1f),
				StorageTags = new string[2] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" },
				DefaultStorageSettings = "forgeStorage",
				RequiresReplenishType = new RequiresReplenishType
				{
					ReplenishProcess = "refuelSmithy",
					RequiresFuelType = new RequiresFuelType
					{
						MaxFuel = 1f,
						FuelTypeKeyName = "item:charcoal",
						BurnRatePerDay = 2f
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 8f,
					PadShape = CollidePrim.Circle,
					Shapes = new CollideShape2D[2]
					{
						new CollideShape2D(new Vector2(-9f, -4f), 16f),
						new CollideShape2D(new Vector2(17f, 2f), 14f)
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "sturdyConstruction",
				SalvageProcess = "salvageSimpleSmithy",
				Repair = "buildingRepair",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:solidMudBrick", 3 },
					{ "item:anvil", 1 },
					{ "item:barClamps", 1 }
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:favorbreadFarm")
		{
			Name = "Favorbread farm",
			SummaryDescription = "A pit for cultivating the favorbread vegetable (which must be ordered from the Production Panel)",
			Description = "By excavating a small garden directly underneath the dead sanctuary tree we can revive the favorbread if we provide it with the carbohydrates that the tree no longer supplies it with. We have found that the blackpulp is well suited as a substrate that will make the favorbread grow vigorously.\n Note: We have found that this method of cultivation is not possible next to a LIVING sanctuary tree because disturbing the connection between the two organisms causes a dangerous, defensive response from them.",
			ThumbnailSmall = "HUD_thumbnail_favorbreadFarm",
			CategoryKey = "production",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic",
				Area = RatingTypes.Food
			},
			ToolType = new ToolType
			{
				ToolTag = new string[1] { "favorbreadFarm" },
				Durability = 1f,
				ToolHandling = ToolHandlingType.Stationary
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "favorbreadFarm_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[3]
				{
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "favorbreadFarm_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "favorbreadFarm_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "favorbreadFarmFull_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 16)
					}
				}
			},
			ContainerType = new ToolContainerType
			{
				CanTransactWithTags = new string[1] { "humanTransact" },
				ProductionOutputStorageType = new ItemStorageType(4f)
				{
					FullStatePercentage = 0.1f
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 4f,
					PadShape = CollidePrim.Circle,
					Shapes = new CollideShape2D[3]
					{
						new CollideShape2D(new Vector2(-17f, -4f), 11f),
						new CollideShape2D(new Vector2(19f, 2f), 11f),
						new CollideShape2D(new Vector2(2f, -17f), 13f)
					},
					SelectionShapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, 0f), 24f)
						{
							Offset = new Vector2(0f, 10f)
						}
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				DegradeType = "adequateConstruction",
				SalvageProcess = "salvageFavorbreadFarm",
				Repair = "diggingRepairCustomProcess",
				PartKeys = new SerializableDictionary<string, int> { { "item:sticks", 1 } }
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:clayPit")
		{
			Name = "Clay pit",
			SummaryDescription = "A site where we can extract a large supply of clay (which must be ordered from the Production Panel)",
			Description = "The pit gives access to the rich deposits of clay beneath the surface which are otherwise hard to reach.",
			ThumbnailSmall = "HUD_thumbnail_clayPit",
			CategoryKey = "production",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ToolType = new ToolType
			{
				Durability = 1f,
				ToolHandling = ToolHandlingType.Stationary
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "clayPit_g"
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 4f,
					PadShape = CollidePrim.Circle,
					Shapes = new CollideShape2D[2]
					{
						new CollideShape2D(new Vector2(-9f, -4f), 16f),
						new CollideShape2D(new Vector2(17f, 2f), 14f)
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				DegradeType = "adequateConstruction",
				SalvageProcess = "salvageClayPit",
				Repair = "diggingRepairCustomProcess",
				PartKeys = new SerializableDictionary<string, int> { { "item:sticks", 1 } }
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:saltMine")
		{
			Name = "Salt mine",
			SummaryDescription = "A site where we can extract a large supply of salt (which must be ordered from the Production Panel)",
			Description = "The mine gives access to the rich deposits of rock salt beneath the surface which are otherwise hard to reach.",
			ThumbnailSmall = "HUD_thumbnail_saltMine",
			CategoryKey = "production",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ToolType = new ToolType
			{
				Durability = 1f,
				ToolHandling = ToolHandlingType.Stationary
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "saltMine_g"
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 4f,
					PadShape = CollidePrim.Circle,
					Shapes = new CollideShape2D[2]
					{
						new CollideShape2D(new Vector2(-9f, -4f), 19f),
						new CollideShape2D(new Vector2(17f, 2f), 14f)
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				DegradeType = "adequateConstruction",
				SalvageProcess = "salvageSaltMine",
				Repair = "diggingRepairCustomProcess",
				PartKeys = new SerializableDictionary<string, int> { { "item:sticks", 1 } }
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:bogOrePit")
		{
			Name = "Bog ore pit",
			SummaryDescription = "A site where we can extract a large supply of bog ore (which must be ordered from the Production Panel)",
			Description = "The pit gives access to the rich deposits of bog ore beneath the surface which are otherwise hard to reach.",
			ThumbnailSmall = "HUD_thumbnail_bogOrePit",
			CategoryKey = "production",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ToolType = new ToolType
			{
				Durability = 1f,
				ToolHandling = ToolHandlingType.Stationary
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "bogOrePit_g"
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 4f,
					PadShape = CollidePrim.Circle,
					Shapes = new CollideShape2D[2]
					{
						new CollideShape2D(new Vector2(-9f, -4f), 16f),
						new CollideShape2D(new Vector2(17f, 2f), 14f)
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				DegradeType = "adequateConstruction",
				SalvageProcess = "salvageBogOrePit",
				Repair = "diggingRepairCustomProcess",
				PartKeys = new SerializableDictionary<string, int> { { "item:sticks", 1 } }
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:rareMetalOrePit1")
		{
			Name = "Scandium mine",
			SummaryDescription = "A site where we can extract scandium ore (which must be ordered from the Production Panel)",
			Description = "The mine gives access to the rich deposits of scandium ore beneath the surface.",
			ThumbnailSmall = "HUD_thumbnail_saltMine",
			CategoryKey = "production",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TierOrArea = new TierOrArea
			{
				Tier = "advanced"
			},
			ToolType = new ToolType
			{
				Durability = 1f,
				ToolHandling = ToolHandlingType.Stationary
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "saltMine_g"
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 4f,
					PadShape = CollidePrim.Circle,
					Shapes = new CollideShape2D[2]
					{
						new CollideShape2D(new Vector2(-9f, -4f), 16f),
						new CollideShape2D(new Vector2(17f, 2f), 14f)
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				DegradeType = "adequateConstruction",
				SalvageProcess = "salvageRareMetalorePit1",
				Repair = "diggingRepairCustomProcess",
				PartKeys = new SerializableDictionary<string, int> { { "item:sticks", 1 } }
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:rareMetalOrePit2")
		{
			Name = "Terbium mine",
			SummaryDescription = "A site where we can extract terbium ore (which must be ordered from the Production Panel)",
			Description = "The mine gives access to the rich deposits of terbium ore beneath the surface.",
			ThumbnailSmall = "HUD_thumbnail_saltMine",
			CategoryKey = "production",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TierOrArea = new TierOrArea
			{
				Tier = "advanced"
			},
			ToolType = new ToolType
			{
				Durability = 1f,
				ToolHandling = ToolHandlingType.Stationary
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "saltMine_g"
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 4f,
					PadShape = CollidePrim.Circle,
					Shapes = new CollideShape2D[2]
					{
						new CollideShape2D(new Vector2(-9f, -4f), 16f),
						new CollideShape2D(new Vector2(17f, 2f), 14f)
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				DegradeType = "adequateConstruction",
				SalvageProcess = "salvageRareMetalorePit2",
				Repair = "diggingRepairCustomProcess",
				PartKeys = new SerializableDictionary<string, int> { { "item:sticks", 1 } }
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:peatBank")
		{
			Name = "Peat bank",
			SummaryDescription = "A site where peat can be cut (which must be ordered from the Production Panel)",
			Description = "The peat bank is a place in a bog or marsh where slabs of partially decomposed vegetation can be cut and used as fuel.",
			ThumbnailSmall = "HUD_thumbnail_peatBank",
			CategoryKey = "production",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ToolType = new ToolType
			{
				Durability = 1f,
				ToolHandling = ToolHandlingType.Stationary
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "peatBank_g"
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 4f,
					PadShape = CollidePrim.Circle,
					Shapes = new CollideShape2D[2]
					{
						new CollideShape2D(new Vector2(-9f, -4f), 16f),
						new CollideShape2D(new Vector2(17f, 2f), 14f)
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				DegradeType = "adequateConstruction",
				SalvageProcess = "salvagePeatBank",
				Repair = "plowingRepairCustomProcess",
				PartKeys = new SerializableDictionary<string, int> { { "item:sticks", 1 } }
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:firewoodStack")
		{
			Name = "Woodpile",
			SummaryDescription = "A pile where wet firewood is stacked and dried so that it can be used as fuel",
			Description = "",
			ThumbnailSmall = "HUD_thumbnail_woodPile",
			CategoryKey = "production",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ToolType = new ToolType
			{
				ToolTag = new string[1] { "woodpileTool" },
				Durability = 0.9f,
				ToolHandling = ToolHandlingType.Stationary
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "woodpile_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[4]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "woodpile"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "woodpile_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "woodpileHalfFull"
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "woodpile_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 17)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "woodpile"
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "woodpile_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 16)
					}
				}
			},
			ContainerType = new ToolContainerType
			{
				CanTransactWithTags = new string[1] { "humanTransact" },
				ProductionOutputStorageType = new ItemStorageType(6f)
				{
					FullStatePercentage = 0.8f,
					HalfFullStatePercentage = 0.15f
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 10f,
					PadShape = CollidePrim.Circle,
					Shapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, -7f), 24f)
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				DegradeType = "dirt",
				SalvageProcess = "salvageFirewoodStack",
				Repair = "buildingRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:compostPit")
		{
			Name = "Compost pit",
			SummaryDescription = "A simple pit that accelerates decomposition into organic matter",
			Description = "Branches and other plant matter such as rotten plant food will, when stored here, quickly decompose into organic matter which can then be made into compost fertilizer.",
			ThumbnailSmall = "HUD_thumbnail_compostPit",
			CategoryKey = "production",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TierOrArea = new TierOrArea
			{
				Tier = "medium"
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "compostPitEmpty_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[4]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "compostPitFull"
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "compostPitEmpty_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "compostPitEmpty_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "compostPitHalfFull_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 17)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "compostPitFull"
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "compostPitEmpty_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 16)
					}
				}
			},
			ContainerType = new StorageContainerType
			{
				CanTransactWithTags = new string[8] { "humanTransact", "robotTransact", "ratTransact", "leafcutterTransact", "chickenTransact", "snatcherTransact", "twinklerTransact", "demonTreeTransact" },
				DefaultStorageSettings = "compostBinSettings",
				StorageTags = new string[2] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" },
				ItemStorageType = new ItemStorageType("moist", 5f)
				{
					FullStatePercentage = 0.7f,
					HalfFullStatePercentage = 0.1f
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
						new CollideShape2D(new Vector2(-5f, -2f), 17f),
						new CollideShape2D(new Vector2(5f, -2f), 17f)
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				DegradeType = "sturdyConstruction",
				SalvageProcess = "salvageCompostPit",
				PartKeys = new SerializableDictionary<string, int> { { "item:stones", 1 } },
				Repair = "buildingRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:compostBin")
		{
			Name = "Compost heap",
			SummaryDescription = "A simple container that accelerates decomposition into organic matter",
			Description = "Branches and other plant matter such as rotten plant food will, when stored here, quickly decompose into organic matter which can then be made into compost fertilizer.",
			ThumbnailSmall = "HUD_thumbnail_compostHeap",
			CategoryKey = "production",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TierOrArea = new TierOrArea
			{
				Tier = "medium"
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "compostHeapEmpty"
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "compostHeap_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[4]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "compostHeapFull"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "compostHeapEmpty"
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "compostHeap_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "compostHeapHalfFull"
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "compostHeap_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 17)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "compostHeapFull"
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "compostHeap_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 16)
					}
				}
			},
			ContainerType = new StorageContainerType
			{
				CanTransactWithTags = new string[8] { "humanTransact", "robotTransact", "ratTransact", "leafcutterTransact", "chickenTransact", "snatcherTransact", "twinklerTransact", "demonTreeTransact" },
				DefaultStorageSettings = "compostBinSettings",
				StorageTags = new string[2] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" },
				ItemStorageType = new ItemStorageType("moist", 5f)
				{
					FullStatePercentage = 1f,
					HalfFullStatePercentage = 0.15f
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 10f,
					PadShape = CollidePrim.Circle,
					Shapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, -2f), 20f)
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				DegradeType = "sturdyConstruction",
				SalvageProcess = "salvageCompostBin",
				PartKeys = new SerializableDictionary<string, int> { { "item:sticks", 2 } },
				Repair = "buildingRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:meatDryingRack")
		{
			Name = "Meat drying rack",
			SummaryDescription = "A simple frame for drying meat",
			Description = "A simple solution for drying thinly cut strips of meat. Must be protected from scavengers, however. ",
			ThumbnailSmall = "HUD_thumbnail_meatDryingRack",
			CategoryKey = "production",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic",
				Area = RatingTypes.Food
			},
			ToolType = new ToolType
			{
				ToolTag = new string[1] { "meatDryingRack" },
				Durability = 0.9f,
				ToolHandling = ToolHandlingType.Stationary
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "meatDryingRackEmpty"
						}
					}
				},
				ClientStateConditions = new ClientStateInfo[4]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "meatDryingRackFull"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "meatDryingRackEmpty"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "meatDryingRackHalfFull"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 17)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "meatDryingRackFull"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 16)
					}
				}
			},
			ContainerType = new ToolContainerType
			{
				CanTransactWithTags = new string[7] { "ratTransact", "humanTransact", "leafcutterTransact", "chickenTransact", "snatcherTransact", "twinklerTransact", "demonTreeTransact" },
				ProductionOutputStorageType = new ItemStorageType(1f)
				{
					FullStatePercentage = 0.6f,
					HalfFullStatePercentage = 0.05f
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
						new CollideShape2D(new Vector2(-7f, -1f), 14f),
						new CollideShape2D(new Vector2(9f, -6f), 14f)
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "adequateConstruction",
				SalvageProcess = "salvageMeatDryingRack",
				PartKeys = new SerializableDictionary<string, int> { { "item:sticks", 3 } },
				Repair = "buildingRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:dryingShed")
		{
			Name = "Drying shed",
			SummaryDescription = "A structure for drying meat, raised from the ground",
			Description = "This shed will protect the meat from rain while letting the wind blow through. It is raised on a pillar to protect the food from scavengers.",
			ThumbnailSmall = "HUD_thumbnail_towerDryingShed",
			CategoryKey = "production",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic",
				Area = RatingTypes.Food
			},
			ToolType = new ToolType
			{
				ToolTag = new string[1] { "meatDryingRack" },
				Durability = 0.9f,
				ToolHandling = ToolHandlingType.Stationary
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "towerDryingShed"
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "towerDryingShed_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[2]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "towerDryingShed"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "clayGranary_construct"
							}
						},
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "towerDryingShed_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					}
				}
			},
			ContainerType = new ToolContainerType
			{
				CanTransactWithTags = new string[1] { "humanTransact" },
				ProductionOutputStorageType = new ItemStorageType(4f)
				{
					FullStatePercentage = 0.8f,
					HalfFullStatePercentage = 0.15f
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 10f,
					PadShape = CollidePrim.Circle,
					Shapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, 3f), 16f)
					},
					SelectionShapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, -20f), 31f)
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "sturdyConstruction",
				SalvageProcess = "salvageDryingShed",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:spoakShingles", 1 },
					{ "item:solidMudBrick", 2 },
					{ "item:shadeleafCanes", 3 }
				},
				Repair = "buildingRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:peatStack")
		{
			Name = "Peat stack",
			SummaryDescription = "Wet peat slabs are placed here to dry so that they can be used as fuel",
			Description = "",
			ThumbnailSmall = "HUD_thumbnail_peatStack",
			CategoryKey = "production",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ToolType = new ToolType
			{
				ToolTag = new string[1] { "peatStackTool" },
				Durability = 0.9f,
				ToolHandling = ToolHandlingType.Stationary
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "peatStack_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[3]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "peatStack"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "peatStack_g"
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "peatStack"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 16)
					}
				}
			},
			ContainerType = new ToolContainerType
			{
				CanTransactWithTags = new string[1] { "humanTransact" },
				ProductionOutputStorageType = new ItemStorageType(4f)
				{
					FullStatePercentage = 0.05f
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 10f,
					PadShape = CollidePrim.Circle,
					Shapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, 3f), 16f)
					},
					SelectionShapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, -20f), 31f)
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				DegradeType = "dirt",
				SalvageProcess = "salvagePeatStack",
				Repair = "buildingRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:hideRack")
		{
			Name = "Hide rack",
			SummaryDescription = "A simple frame for stretching and cleaning a hide",
			Description = "An important tool for making hide products the primitive way.",
			ThumbnailSmall = "HUD_thumbnail_hideRack",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			CategoryKey = "production",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			ToolType = new ToolType
			{
				ToolTag = new string[1] { "hideRack" },
				Durability = 0.9f,
				ToolHandling = ToolHandlingType.Stationary
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "hideRackEmpty"
						}
					}
				},
				ClientStateConditions = new ClientStateInfo[3]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "hideRack"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "hideRackEmpty"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "hideRack"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 16)
					}
				}
			},
			ContainerType = new ToolContainerType
			{
				CanTransactWithTags = new string[7] { "ratTransact", "humanTransact", "leafcutterTransact", "chickenTransact", "snatcherTransact", "twinklerTransact", "demonTreeTransact" },
				ProductionOutputStorageType = new ItemStorageType(0.08f)
				{
					FullStatePercentage = 0.1f
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 10f,
					PadShape = CollidePrim.Circle,
					Shapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, -1f), 14f)
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "adequateConstruction",
				SalvageProcess = "salvageHideRack",
				PartKeys = new SerializableDictionary<string, int> { { "item:sticks", 2 } },
				Repair = "buildingRepair"
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:rareMetalRefinery")
		{
			Name = "Refinery: Rare-earth",
			SummaryDescription = "An advanced machine for separating rare-earth metals from their ores",
			Description = "The refinery is so compact that it can be dismantled, moved and set up near the mineral deposits. It uses a series of chemical and mechanical processes such as solvent-extraction and flotation to refine rare-earth metals.",
			ThumbnailSmall = "HUD_thumbnail_refiner",
			TierOrArea = new TierOrArea
			{
				Tier = "advanced"
			},
			CategoryKey = "production",
			StructureType = new StructureType
			{
				BuildByPlayer = true
			},
			ToolType = new ToolType
			{
				ToolTag = new string[1] { "rareMetalRefinery" },
				Durability = 0.9f,
				ToolHandling = ToolHandlingType.Stationary
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "refiner"
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "refiner_g"
					}
				},
				ClientStateConditions = new ClientStateInfo[2]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "refiner"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 0)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "refiner_construct"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 1)
					}
				}
			},
			ContainerType = new ToolContainerType
			{
				CanTransactWithTags = new string[1] { "humanTransact" },
				ProductionOutputStorageType = new ItemStorageType(0.35f)
				{
					FullStatePercentage = 0.1f
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 10f,
					PadShape = CollidePrim.Circle,
					Shapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, -1f), 20f)
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "advancedConstruction",
				SalvageProcess = "salvageRareMetalRefinery",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:metalRefineryPart1", 1 },
					{ "item:metalRefineryEquipment", 1 }
				},
				Repair = "buildingRepair"
			}
		});
	}
}
