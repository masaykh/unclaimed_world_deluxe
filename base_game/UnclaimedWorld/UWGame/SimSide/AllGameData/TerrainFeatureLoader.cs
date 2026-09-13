using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Particles;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Collisions;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.AllGameData;

public class TerrainFeatureLoader
{
	public static void Init(List<EntityType> listOfEntityTypes)
	{
		EntityType entityType = new EntityType("tracks");
		entityType.Name = "Wheel path";
		entityType.RenderableType = new RenderableType
		{
			RenderAsConnectedGroundSpriteType = new RenderAsConnectedGroundSpriteType
			{
				AssetName = "Tracks"
			}
		};
		entityType.DirectionalLayoutType = new DirectionalLayoutType();
		entityType.TerrainType = new TerrainFeatureType
		{
			PathType = new PathType
			{
				TransportCosts = new byte[4] { 3, 4, 3, 1 },
				Rank = 2
			}
		};
		EntityType item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("footpath");
		entityType.Name = "Foot path";
		entityType.RenderableType = new RenderableType
		{
			RenderAsConnectedGroundSpriteType = new RenderAsConnectedGroundSpriteType
			{
				AssetName = "footpath"
			}
		};
		entityType.DirectionalLayoutType = new DirectionalLayoutType();
		entityType.TerrainType = new TerrainFeatureType
		{
			PathType = new PathType
			{
				TransportCosts = new byte[4] { 3, 5, 4, 1 },
				Rank = 3
			}
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("terrain:plains");
		entityType.Name = "Plains";
		entityType.TerrainType = new TerrainFeatureType
		{
			PathType = new PathType
			{
				TransportCosts = new byte[4] { 3, 5, 4, 1 },
				Rank = 3
			}
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("terrain:rockformation1");
		entityType.Name = "Rocks";
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[7]
				{
					new RenderAsBillboardType
					{
						AssetName = "rockwall1",
						Offset = new Vector2(204f, 39f) - new Vector2(120f, 72f)
					},
					new RenderAsBillboardType
					{
						AssetName = "rockwall2",
						Offset = new Vector2(68f, 103f) - new Vector2(120f, 72f)
					},
					new RenderAsBillboardType
					{
						AssetName = "rockwall3",
						Offset = new Vector2(40f, 83f) - new Vector2(120f, 72f)
					},
					new RenderAsBillboardType
					{
						AssetName = "rockwall4",
						Offset = new Vector2(136f, 103f) - new Vector2(120f, 72f)
					},
					new RenderAsBillboardType
					{
						AssetName = "rockwall5",
						Offset = new Vector2(189f, 73f) - new Vector2(120f, 72f)
					},
					new RenderAsBillboardType
					{
						AssetName = "rockwall6",
						Offset = new Vector2(226f, 56f) - new Vector2(120f, 72f)
					},
					new RenderAsBillboardType
					{
						AssetName = "rockwall7",
						Offset = new Vector2(12f, 70f) - new Vector2(120f, 72f)
					}
				},
				RenderAsGroundSpriteType = new RenderAsGroundSpriteType
				{
					AssetName = "rockwall_g"
				}
			}
		};
		entityType.TerrainType = new TerrainFeatureType();
		item = entityType;
		listOfEntityTypes.Add(item);
		listOfEntityTypes.Add(new EntityType("terrain:naturalLandTerminal")
		{
			RequiresRollToDetect = false,
			IsNeverInFogOfWar = false,
			ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always,
			UsesMemory = true,
			IsSelectable = true,
			Name = "Land passage",
			SummaryDescription = "A passage to a neighbor area which can be reached by foot. (Leads to another site)",
			Description = "The neighbor site is close enough that we can see it from here. Communication could be made with simple means such as signs and sound.",
			ThumbnailSmall = "HUD_thumbnail_path",
			UseTypeNameForDisplay = false,
			EditorRenderableType = new RenderableType
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
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 48f,
					PadShape = CollidePrim.Circle
				}
			},
			TerrainType = new TerrainFeatureType
			{
				CanBeMapEditorPlaced = true,
				IsSpecialInterestFeature = true
			},
			TerminalType = new TerminalType
			{
				TypeOfTerminal = TerminalType.TypesOfTerminal.Land
			}
		});
		listOfEntityTypes.Add(new EntityType("terrain:naturalPseudoWaterTerminal")
		{
			RequiresRollToDetect = false,
			IsNeverInFogOfWar = false,
			ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always,
			UsesMemory = true,
			IsSelectable = true,
			Name = "Water passage",
			SummaryDescription = "Water that can be crossed by a determined (or desperate) swimmer. (Leads to another site)",
			Description = "The neighbor site is close enough that we can see it from here. Communication could be made with simple means such as signs and sound.",
			ThumbnailSmall = "HUD_thumbnail_waterPassage",
			UseTypeNameForDisplay = false,
			EditorRenderableType = new RenderableType
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
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = 48f,
					PadShape = CollidePrim.Circle
				}
			},
			TerrainType = new TerrainFeatureType
			{
				CanBeMapEditorPlaced = true,
				IsSpecialInterestFeature = true
			},
			TerminalType = new TerminalType
			{
				TypeOfTerminal = TerminalType.TypesOfTerminal.Land
			}
		});
		entityType = new EntityType("terrain:shoreWavesLongA");
		entityType.Name = "S: Water, Shore waves, long A";
		entityType.IsNeverInFogOfWar = true;
		entityType.RequiresRollToDetect = false;
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				Sounds = new string[1] { "ambient/water/shoreWavesCalmLonger3A" }
			}
		};
		entityType.EditorRenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "leatherPouch"
					}
				},
				Sounds = new string[1] { "ambient/water/shoreWavesCalmLonger3A" }
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("terrain:shoreWavesLongB");
		entityType.Name = "S: Water, Shore waves, long B";
		entityType.IsNeverInFogOfWar = true;
		entityType.RequiresRollToDetect = false;
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				Sounds = new string[1] { "ambient/water/shoreWavesCalmLonger3B" }
			}
		};
		entityType.EditorRenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "leatherPouch"
					}
				},
				Sounds = new string[1] { "ambient/water/shoreWavesCalmLonger3B" }
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("terrain:shoreWavesLongLowerA");
		entityType.Name = "S: Water, Shore waves, long & lower A";
		entityType.IsNeverInFogOfWar = true;
		entityType.RequiresRollToDetect = false;
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				Sounds = new string[1] { "ambient/water/shoreWavesCalmLongerLower3A" }
			}
		};
		entityType.EditorRenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "leatherPouch"
					}
				},
				Sounds = new string[1] { "ambient/water/shoreWavesCalmLongerLower3A" }
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("terrain:shoreWavesLongLowerB");
		entityType.Name = "S: Water, Shore waves, long & lower B";
		entityType.IsNeverInFogOfWar = true;
		entityType.RequiresRollToDetect = false;
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				Sounds = new string[1] { "ambient/water/shoreWavesCalmLongerLower3B" }
			}
		};
		entityType.EditorRenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "leatherPouch"
					}
				},
				Sounds = new string[1] { "ambient/water/shoreWavesCalmLongerLower3B" }
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("terrain:shoreWavesA");
		entityType.Name = "S: Water, Shore waves A";
		entityType.IsNeverInFogOfWar = true;
		entityType.RequiresRollToDetect = false;
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				Sounds = new string[1] { "ambient/water/shoreWavesCalm2A" }
			}
		};
		entityType.EditorRenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "leatherPouch"
					}
				},
				Sounds = new string[1] { "ambient/water/shoreWavesCalm2A" }
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("terrain:shoreWavesB");
		entityType.Name = "S: Water, Shore waves B";
		entityType.IsNeverInFogOfWar = true;
		entityType.RequiresRollToDetect = false;
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				Sounds = new string[1] { "ambient/water/shoreWavesCalm2B" }
			}
		};
		entityType.EditorRenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "leatherPouch"
					}
				},
				Sounds = new string[1] { "ambient/water/shoreWavesCalm2B" }
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("terrain:shoreWavesC");
		entityType.Name = "S: Water, Shore waves";
		entityType.IsNeverInFogOfWar = true;
		entityType.RequiresRollToDetect = false;
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				Sounds = new string[1] { "ambient/water/shoreWavesCalm2C" }
			}
		};
		entityType.EditorRenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "leatherPouch"
					}
				},
				Sounds = new string[1] { "ambient/water/shoreWavesCalm2C" }
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("terrain:brook");
		entityType.Name = "S: Water, Brook";
		entityType.IsNeverInFogOfWar = false;
		entityType.RequiresRollToDetect = false;
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				Sounds = new string[1] { "ambient/water/waterBrook" }
			}
		};
		entityType.EditorRenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "leatherPouch"
					}
				},
				Sounds = new string[1] { "ambient/water/waterBrook" }
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("terrain:riverStreamA");
		entityType.Name = "S: Water, River, medium stream A";
		entityType.IsNeverInFogOfWar = true;
		entityType.RequiresRollToDetect = false;
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				Sounds = new string[1] { "ambient/water/waterRiverMediumStreamA" }
			}
		};
		entityType.EditorRenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "leatherPouch"
					}
				},
				Sounds = new string[1] { "ambient/water/waterRiverMediumStreamA" }
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("terrain:riverStreamB");
		entityType.Name = "S: Water, River, medium stream B";
		entityType.IsNeverInFogOfWar = true;
		entityType.RequiresRollToDetect = false;
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				Sounds = new string[1] { "ambient/water/waterRiverMediumStreamB" }
			}
		};
		entityType.EditorRenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "leatherPouch"
					}
				},
				Sounds = new string[1] { "ambient/water/waterRiverMediumStreamB" }
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("terrain:riverStreamC");
		entityType.Name = "S: Water, River, medium stream C";
		entityType.IsNeverInFogOfWar = true;
		entityType.RequiresRollToDetect = false;
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				Sounds = new string[1] { "ambient/water/waterRiverMediumStreamC" }
			}
		};
		entityType.EditorRenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "leatherPouch"
					}
				},
				Sounds = new string[1] { "ambient/water/waterRiverMediumStreamC" }
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("terrain:brookSmallA");
		entityType.Name = "S: Water, Brook, Small A";
		entityType.IsNeverInFogOfWar = false;
		entityType.RequiresRollToDetect = false;
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				Sounds = new string[1] { "ambient/water/waterBrookSmallA" }
			}
		};
		entityType.EditorRenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "leatherPouch"
					}
				},
				Sounds = new string[1] { "ambient/water/waterBrookSmallA" }
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("terrain:brookSmallB");
		entityType.Name = "S: Water, Brook, Small B";
		entityType.IsNeverInFogOfWar = false;
		entityType.RequiresRollToDetect = false;
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				Sounds = new string[1] { "ambient/water/waterBrookSmallB" }
			}
		};
		entityType.EditorRenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "leatherPouch"
					}
				},
				Sounds = new string[1] { "ambient/water/waterBrookSmallB" }
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("terrain:brookSmallC");
		entityType.Name = "S: Water, Brook, Small C";
		entityType.IsNeverInFogOfWar = false;
		entityType.RequiresRollToDetect = false;
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				Sounds = new string[1] { "ambient/water/waterBrookSmallC" }
			}
		};
		entityType.EditorRenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "leatherPouch"
					}
				},
				Sounds = new string[1] { "ambient/water/waterBrookSmallC" }
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("terrain:mudflatsA");
		entityType.Name = "S: Water, Mudswamp, A";
		entityType.IsNeverInFogOfWar = false;
		entityType.RequiresRollToDetect = false;
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				Sounds = new string[1] { "ambient/water/mudflats1" }
			}
		};
		entityType.EditorRenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "leatherPouch"
					}
				},
				Sounds = new string[1] { "ambient/water/mudflats1" }
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("terrain:mudflatsB");
		entityType.Name = "S: Water, Mudswamp, B";
		entityType.IsNeverInFogOfWar = false;
		entityType.RequiresRollToDetect = false;
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				Sounds = new string[1] { "ambient/water/mudflats2" }
			}
		};
		entityType.EditorRenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "leatherPouch"
					}
				},
				Sounds = new string[1] { "ambient/water/mudflats2" }
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("terrain:windMid");
		entityType.Name = "S: Wind stable, mid freq";
		entityType.IsNeverInFogOfWar = true;
		entityType.RequiresRollToDetect = false;
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				Sounds = new string[1] { "ambient/wind/windMidFreq" }
			}
		};
		entityType.EditorRenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "leatherPouch"
					}
				},
				Sounds = new string[1] { "ambient/wind/windMidFreq" }
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("terrain:windHighFreqA");
		entityType.Name = "S: Wind gusty, hi-freq";
		entityType.IsNeverInFogOfWar = true;
		entityType.RequiresRollToDetect = false;
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				Sounds = new string[1] { "ambient/wind/windHighFreq" }
			}
		};
		entityType.EditorRenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "leatherPouch"
					}
				},
				Sounds = new string[1] { "ambient/wind/windHighFreq" }
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("terrain:windHighFreqB");
		entityType.Name = "S: Wind gusty, hi-freq (cycle 3 var)";
		entityType.IsNeverInFogOfWar = true;
		entityType.RequiresRollToDetect = false;
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				Sounds = new string[3] { "ambient/wind/windHighFreqGustyLong1A", "ambient/wind/windHighFreqGustyLong1B", "ambient/wind/windHighFreqGustyLong1C" }
			}
		};
		entityType.EditorRenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "leatherPouch"
					}
				},
				Sounds = new string[3] { "ambient/wind/windHighFreqGustyLong1A", "ambient/wind/windHighFreqGustyLong1B", "ambient/wind/windHighFreqGustyLong1C" }
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("terrain:insectsMedium");
		entityType.Name = "S: Insects, medium group";
		entityType.IsNeverInFogOfWar = false;
		entityType.RequiresRollToDetect = false;
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				DelayBetweenSounds = new NormalDistribution
				{
					Min = 20f,
					Max = 22f
				},
				Sounds = new string[1] { "ambient/aliens/insectsMediumGroup" }
			}
		};
		entityType.EditorRenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "leatherPouch"
					}
				},
				Sounds = new string[1] { "ambient/aliens/insectsMediumGroup" }
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("terrain:insectsLongConstant");
		entityType.Name = "S: insect, long w break";
		entityType.IsNeverInFogOfWar = false;
		entityType.RequiresRollToDetect = false;
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				DelayBetweenSounds = new NormalDistribution
				{
					Min = 10f,
					Max = 15f
				},
				Sounds = new string[1] { "ambient/aliens/insectsLongConstantwBreak" }
			}
		};
		entityType.EditorRenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "leatherPouch"
					}
				},
				Sounds = new string[1] { "ambient/aliens/insectsLongConstantwBreak" }
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("terrain:insectFlying");
		entityType.Name = "S: Insect, flying (cycle 2 var)";
		entityType.IsNeverInFogOfWar = false;
		entityType.RequiresRollToDetect = false;
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				DelayBetweenSounds = new NormalDistribution
				{
					Min = 22f,
					Max = 36f
				},
				Sounds = new string[2] { "ambient/insects/insectFlying1A", "ambient/insects/insectFlying1B" }
			}
		};
		entityType.EditorRenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "leatherPouch"
					}
				},
				Sounds = new string[2] { "ambient/insects/insectFlying1A", "ambient/insects/insectFlying1B" }
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("terrain:insectGroup1");
		entityType.Name = "S: Insects, (cycle 3 var)";
		entityType.IsNeverInFogOfWar = false;
		entityType.RequiresRollToDetect = false;
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				DelayBetweenSounds = new NormalDistribution
				{
					Min = 2f,
					Max = 5f
				},
				Sounds = new string[3] { "ambient/insects/insectGroup1A", "ambient/insects/insectGroup1B", "ambient/insects/insectGroup1C" }
			}
		};
		entityType.EditorRenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "leatherPouch"
					}
				},
				Sounds = new string[3] { "ambient/insects/insectGroup1A", "ambient/insects/insectGroup1B", "ambient/insects/insectGroup1C" }
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("terrain:insectsLongSingle");
		entityType.Name = "S: insect, single, long";
		entityType.IsNeverInFogOfWar = false;
		entityType.RequiresRollToDetect = false;
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				DelayBetweenSounds = new NormalDistribution
				{
					Min = 12f,
					Max = 22f
				},
				Sounds = new string[1] { "ambient/aliens/insectsSingleB" }
			}
		};
		entityType.EditorRenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "leatherPouch"
					}
				},
				Sounds = new string[1] { "ambient/aliens/insectsSingleB" }
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("terrain:birdsSingle1");
		entityType.Name = "S: Bird 1, single, (cycle 3 var)";
		entityType.IsNeverInFogOfWar = false;
		entityType.RequiresRollToDetect = false;
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				DelayBetweenSounds = new NormalDistribution
				{
					Min = 10f,
					Max = 15f
				},
				Sounds = new string[3] { "ambient/birds/birdSingle1A", "ambient/birds/birdSingle1B", "ambient/birds/birdSingle1C" }
			}
		};
		entityType.EditorRenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "leatherPouch"
					}
				},
				Sounds = new string[3] { "ambient/birds/birdSingle1A", "ambient/birds/birdSingle1B", "ambient/birds/birdSingle1C" }
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("terrain:birdsSingle2");
		entityType.Name = "S: Bird 2, single, (cycle 3 var)";
		entityType.IsNeverInFogOfWar = false;
		entityType.RequiresRollToDetect = false;
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				DelayBetweenSounds = new NormalDistribution
				{
					Min = 6f,
					Max = 8f
				},
				Sounds = new string[3] { "ambient/birds/birdSingle2A", "ambient/birds/birdSingle2B", "ambient/birds/birdSingle2C" }
			}
		};
		entityType.EditorRenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "leatherPouch"
					}
				},
				Sounds = new string[3] { "ambient/birds/birdSingle2A", "ambient/birds/birdSingle2B", "ambient/birds/birdSingle2C" }
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("terrain:birdsSingle3");
		entityType.Name = "S: Bird 3, single, (cycle 2 var)";
		entityType.IsNeverInFogOfWar = false;
		entityType.RequiresRollToDetect = false;
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				DelayBetweenSounds = new NormalDistribution
				{
					Min = 12f,
					Max = 18f
				},
				Sounds = new string[2] { "ambient/birds/birdSingle3A", "ambient/birds/birdSingle3B" }
			}
		};
		entityType.EditorRenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "leatherPouch"
					}
				},
				Sounds = new string[2] { "ambient/birds/birdSingle3A", "ambient/birds/birdSingle3B" }
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("terrain:birdsSingle4");
		entityType.Name = "S: Bird 4, single, (cycle 4 var)";
		entityType.IsNeverInFogOfWar = false;
		entityType.RequiresRollToDetect = false;
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				DelayBetweenSounds = new NormalDistribution
				{
					Min = 8f,
					Max = 21f
				},
				Sounds = new string[4] { "ambient/birds/birdSingle4A", "ambient/birds/birdSingle4B", "ambient/birds/birdSingle4C", "ambient/birds/birdSingle4B" }
			}
		};
		entityType.EditorRenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "leatherPouch"
					}
				},
				Sounds = new string[4] { "ambient/birds/birdSingle4A", "ambient/birds/birdSingle4B", "ambient/birds/birdSingle4C", "ambient/birds/birdSingle4B" }
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("terrain:birdsSingle5");
		entityType.Name = "S: Bird 5, single, (cycle 5 var)";
		entityType.IsNeverInFogOfWar = false;
		entityType.RequiresRollToDetect = false;
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				DelayBetweenSounds = new NormalDistribution
				{
					Min = 14f,
					Max = 24f
				},
				Sounds = new string[5] { "ambient/birds/birdSingle5A", "ambient/birds/birdSingle5B", "ambient/birds/birdSingle5C", "ambient/birds/birdSingle5D", "ambient/birds/birdSingle5E" }
			}
		};
		entityType.EditorRenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "leatherPouch"
					}
				},
				Sounds = new string[5] { "ambient/birds/birdSingle5A", "ambient/birds/birdSingle5B", "ambient/birds/birdSingle5C", "ambient/birds/birdSingle5D", "ambient/birds/birdSingle5E" }
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("terrain:swampFrogsA");
		entityType.Name = "S: frogs A, swamp";
		entityType.IsNeverInFogOfWar = false;
		entityType.RequiresRollToDetect = false;
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				DelayBetweenSounds = new NormalDistribution
				{
					Min = 20f,
					Max = 38f
				},
				Sounds = new string[1] { "ambient/aliens/swampFrogs" }
			}
		};
		entityType.EditorRenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "leatherPouch"
					}
				},
				Sounds = new string[1] { "ambient/aliens/swampFrogs" }
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("terrain:swampFrogsB");
		entityType.Name = "S: frogs B, swamp";
		entityType.IsNeverInFogOfWar = false;
		entityType.RequiresRollToDetect = false;
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				DelayBetweenSounds = new NormalDistribution
				{
					Min = 22f,
					Max = 31f
				},
				Sounds = new string[1] { "ambient/aliens/swampFrogs2" }
			}
		};
		entityType.EditorRenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "leatherPouch"
					}
				},
				Sounds = new string[1] { "ambient/aliens/swampFrogs2" }
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("terrain:swampFrogsC");
		entityType.Name = "S: frogs C, swamp";
		entityType.IsNeverInFogOfWar = false;
		entityType.RequiresRollToDetect = false;
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				DelayBetweenSounds = new NormalDistribution
				{
					Min = 12f,
					Max = 20f
				},
				Sounds = new string[1] { "ambient/aliens/swampFrogs3" }
			}
		};
		entityType.EditorRenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "leatherPouch"
					}
				},
				Sounds = new string[1] { "ambient/aliens/swampFrogs3" }
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("terrain:wormSingle");
		entityType.Name = "S: Worm, single worm calls";
		entityType.IsNeverInFogOfWar = false;
		entityType.RequiresRollToDetect = false;
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				Sounds = new string[1] { "ambient/aliens/wormAmbienceSingle" }
			}
		};
		entityType.EditorRenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "leatherPouch"
					}
				},
				Sounds = new string[1] { "ambient/aliens/wormAmbienceSingle" }
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("terrain:rattleWooden");
		entityType.Name = "S: Rattle, wooden";
		entityType.IsNeverInFogOfWar = false;
		entityType.RequiresRollToDetect = false;
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				Sounds = new string[1] { "ambient\\aliens\\rattleWoodenAmbient" }
			}
		};
		entityType.EditorRenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "leatherPouch"
					}
				},
				Sounds = new string[1] { "ambient\\aliens\\rattleWoodenAmbient" }
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("terrain:toothCrickets");
		entityType.Name = "S: Tooth crickets";
		entityType.IsNeverInFogOfWar = false;
		entityType.RequiresRollToDetect = false;
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				Sounds = new string[1] { "ambient\\aliens\\toothCricketsAmbient" }
			}
		};
		entityType.EditorRenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "leatherPouch"
					}
				},
				Sounds = new string[1] { "ambient\\aliens\\toothCricketsAmbient" }
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("terrain:angryToyAmbient");
		entityType.Name = "S: Angry toy";
		entityType.IsNeverInFogOfWar = false;
		entityType.RequiresRollToDetect = false;
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				Sounds = new string[1] { "ambient\\aliens\\angryToyAmbient" }
			}
		};
		entityType.EditorRenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "leatherPouch"
					}
				},
				Sounds = new string[1] { "ambient\\aliens\\angryToyAmbient" }
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("terrain:croakerAmbient");
		entityType.Name = "S: Croaker";
		entityType.IsNeverInFogOfWar = false;
		entityType.RequiresRollToDetect = false;
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				Sounds = new string[1] { "ambient\\aliens\\croakerAmbient" }
			}
		};
		entityType.EditorRenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "leatherPouch"
					}
				},
				Sounds = new string[1] { "ambient\\aliens\\croakerAmbient" }
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("terrain:hummingClickClackingAmbient");
		entityType.Name = "S: Humming, ClickClacking Ambient";
		entityType.IsNeverInFogOfWar = false;
		entityType.RequiresRollToDetect = false;
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				Sounds = new string[1] { "ambient\\aliens\\hummingClickClackingAmbient" }
			}
		};
		entityType.EditorRenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "leatherPouch"
					}
				},
				Sounds = new string[1] { "ambient\\aliens\\hummingClickClackingAmbient" }
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("terrain:rattleBuzzAmbient");
		entityType.Name = "S: Rattle, Buzz Ambient";
		entityType.IsNeverInFogOfWar = false;
		entityType.RequiresRollToDetect = false;
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				Sounds = new string[1] { "ambient\\aliens\\rattleBuzzAmbient" }
			}
		};
		entityType.EditorRenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "leatherPouch"
					}
				},
				Sounds = new string[1] { "ambient\\aliens\\rattleBuzzAmbient" }
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("terrain:spacechickenAmbient");
		entityType.Name = "S: Spacechicken Ambient";
		entityType.IsNeverInFogOfWar = false;
		entityType.RequiresRollToDetect = false;
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				Sounds = new string[1] { "ambient\\aliens\\spacechickenAmbient" }
			}
		};
		entityType.EditorRenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "leatherPouch"
					}
				},
				Sounds = new string[1] { "ambient\\aliens\\spacechickenAmbient" }
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("terrain:groundFog");
		entityType.Name = "Ground fog";
		entityType.RenderableType = new RenderableType
		{
			ParticleEmitterTypes = new ParticleEmitterType[1]
			{
				new ParticleEmitterType
				{
					ParticleSystemKey = "groundFog"
				}
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = false
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("terrain:crates");
		entityType.IsNeverInFogOfWar = false;
		entityType.UsesMemory = true;
		entityType.ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always;
		entityType.Name = "Lost supplies";
		entityType.SummaryDescription = "Items that fell from the skimmer during the crash";
		entityType.Description = "We have spotted some of our items lying at the bottom of this sandstone canyon. It should be possible to retrieve them by climbing down, using suitable rope.";
		entityType.DetectionTag = "hardToSpot";
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsGroundSpriteType = new RenderAsGroundSpriteType
				{
					AssetName = "moss1_g"
				}
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = false
		};
		entityType.IsSelectable = true;
		entityType.DefaultSimState = new SimStateInfo
		{
			GeometryLayoutType = MakeRectangle(0f, 2f, 3f, 3f)
		};
		entityType.SharedSpecialActions = new Pair<string, bool>[1]
		{
			new Pair<string, bool>("retrieveCrates", second: true)
		};
		EntityType entityType2 = entityType;
		entityType2.DefaultSimState.GeometryLayoutType.Pad = 20f;
		listOfEntityTypes.Add(entityType2);
		entityType = new EntityType("terrain:unconscious");
		entityType.RequiresRollToDetect = false;
		entityType.IsNeverInFogOfWar = false;
		entityType.UsesMemory = true;
		entityType.ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always;
		entityType.Name = "Lost team member";
		entityType.SummaryDescription = "Our lost mission member is alive but motionless at the bottom of this crevice";
		entityType.Description = "He seems to be unconscious. We need to climb down and perform first aid as soon as possible.";
		entityType.DetectionTag = "kindaHardToSpot";
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsGroundSpriteType = new RenderAsGroundSpriteType
				{
					AssetName = "moss1_g"
				}
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = false,
			IsSpecialInterestFeature = true
		};
		entityType.IsSelectable = true;
		entityType.DefaultSimState = new SimStateInfo
		{
			GeometryLayoutType = MakeRectangle(0f, 2f, 3f, 3f)
		};
		entityType.SharedSpecialActions = new Pair<string, bool>[1]
		{
			new Pair<string, bool>("rescueColleague", second: true)
		};
		EntityType entityType3 = entityType;
		entityType3.DefaultSimState.GeometryLayoutType.Pad = 20f;
		listOfEntityTypes.Add(entityType3);
		entityType = new EntityType("terrain:quaditeNest");
		entityType.IsNeverInFogOfWar = false;
		entityType.UsesMemory = true;
		entityType.ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always;
		entityType.Name = "Quadite Nest";
		entityType.SummaryDescription = "Entrance to an underground nest of dangerous quadites";
		entityType.Description = "The nest lies in a cave which is likely part of a bigger underground system of so-called lava caves which have been formed from ancient volcanic activity. The cave network could hold countless individuals - this number would depend on the amount of prey available in the area.\n We would expect to see other entrances to the underground network which could hold other quadite nests.\n Approach with extreme caution.";
		entityType.ThumbnailSmall = "HUD_thumbnail_placeholder";
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsGroundSpriteType = new RenderAsGroundSpriteType
				{
					AssetName = "moss1_g"
				}
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = false,
			IsSpecialInterestFeature = true
		};
		entityType.DetectionTag = "kindaHardToSpot";
		entityType.ThreatType = new ThreatType
		{
			StrengthRating = StrengthRating.WeakerThanHumans
		};
		entityType.IsSelectable = true;
		entityType.DefaultSimState = new SimStateInfo
		{
			GeometryLayoutType = MakeRectangle(0f, 2f, 3f, 3f)
		};
		entityType.SharedSpecialActions = new Pair<string, bool>[1]
		{
			new Pair<string, bool>("useSulfurSmokeBomb", second: true)
		};
		EntityType entityType4 = entityType;
		entityType4.DefaultSimState.GeometryLayoutType.Pad = 20f;
		listOfEntityTypes.Add(entityType4);
		entityType = new EntityType("terrain:fieldQuaditeNest");
		entityType.IsNeverInFogOfWar = false;
		entityType.UsesMemory = true;
		entityType.ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always;
		entityType.Name = "Field quadite nest";
		entityType.SummaryDescription = "Entrance to an underground nest network";
		entityType.Description = "Underneath firegrass, the field quadites build a multitude of nests interconnected by underground passages.";
		entityType.ThumbnailSmall = "HUD_thumbnail_placeholder";
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsGroundSpriteType = new RenderAsGroundSpriteType
				{
					AssetName = "holeSoilSmall_g"
				}
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = false,
			IsSpecialInterestFeature = true
		};
		entityType.DetectionTag = "kindaHardToSpot";
		entityType.ThreatType = new ThreatType
		{
			StrengthRating = StrengthRating.VeryWeak
		};
		entityType.IsSelectable = false;
		entityType.SharedSpecialActions = new Pair<string, bool>[1]
		{
			new Pair<string, bool>("useVarmintBomb", second: true)
		};
		entityType.DefaultSimState = new SimStateInfo
		{
			GeometryLayoutType = MakeRectangle(0f, 2f, 3f, 3f)
		};
		EntityType entityType5 = entityType;
		entityType5.DefaultSimState.GeometryLayoutType.Pad = 20f;
		listOfEntityTypes.Add(entityType5);
		entityType = new EntityType("terrain:swarmerNest");
		entityType.IsNeverInFogOfWar = false;
		entityType.UsesMemory = true;
		entityType.ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always;
		entityType.Name = "Swarmer nest";
		entityType.SummaryDescription = "Entrance to an underground nest network";
		entityType.Description = "Underneath firegrass, the field quadites build a multitude of nests interconnected by underground passages.";
		entityType.ThumbnailSmall = "HUD_thumbnail_placeholder";
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsGroundSpriteType = new RenderAsGroundSpriteType
				{
					AssetName = "holeSoilSmall_g"
				}
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = false,
			IsSpecialInterestFeature = true
		};
		entityType.DetectionTag = "kindaHardToSpot";
		entityType.ThreatType = new ThreatType
		{
			StrengthRating = StrengthRating.VeryWeak
		};
		entityType.IsSelectable = false;
		entityType.SharedSpecialActions = new Pair<string, bool>[1]
		{
			new Pair<string, bool>("useVarmintBomb", second: true)
		};
		entityType.DefaultSimState = new SimStateInfo
		{
			GeometryLayoutType = MakeRectangle(0f, 2f, 3f, 3f)
		};
		EntityType item2 = entityType;
		entityType5.DefaultSimState.GeometryLayoutType.Pad = 20f;
		listOfEntityTypes.Add(item2);
		entityType = new EntityType("terrain:crevice");
		entityType.IsNeverInFogOfWar = false;
		entityType.UsesMemory = true;
		entityType.ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always;
		entityType.Name = "Gorge";
		entityType.SummaryDescription = "A narrow gorge.";
		entityType.Description = "We need to find a way across.";
		entityType.ThumbnailSmall = "HUD_thumbnail_placeholder";
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsGroundSpriteType = new RenderAsGroundSpriteType
				{
					AssetName = "terrainBlockerWide_g"
				}
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true
		};
		entityType.DetectionTag = "largeOnGround";
		entityType.IsSelectable = true;
		entityType.DefaultSimState = new SimStateInfo
		{
			GeometryLayoutType = MakeRectangle(0f, 0f, 150f, 44f)
		};
		entityType.SharedSpecialActions = new Pair<string, bool>[1]
		{
			new Pair<string, bool>("buildRopeBridge", second: true)
		};
		EntityType entityType6 = entityType;
		entityType6.DefaultSimState.GeometryLayoutType.Pad = 20f;
		listOfEntityTypes.Add(entityType6);
		EntityType entityType7 = new EntityType("terrain:terrainBlockerWide")
		{
			Name = "Terrain Blocker Wide",
			SummaryDescription = "for blocking passage",
			TerrainType = new TerrainFeatureType
			{
				CanBeMapEditorPlaced = false
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = MakeRectangle(0f, 0f, 150f, 44f)
			}
		};
		entityType7.DefaultSimState.GeometryLayoutType.Pad = 20f;
		listOfEntityTypes.Add(entityType7);
		EntityType item3 = new EntityType("terrain:ropeBridge")
		{
			IsNeverInFogOfWar = false,
			UsesMemory = true,
			ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always,
			Name = "Rope bridge",
			SummaryDescription = "We built it. It's safe enough to cross.",
			Description = "",
			ThumbnailSmall = "HUD_thumbnail_placeholder",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "ropeBridge_g"
					}
				}
			},
			TerrainType = new TerrainFeatureType
			{
				CanBeMapEditorPlaced = false
			},
			DetectionTag = "kindaHardToSpot",
			IsSelectable = true
		};
		listOfEntityTypes.Add(item3);
		entityType = new EntityType("terrain:catamaranWreck");
		entityType.IsNeverInFogOfWar = false;
		entityType.UsesMemory = true;
		entityType.ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.OnlyWhenSelected;
		entityType.Name = "Catamaran wreck";
		entityType.SummaryDescription = "Our catamaran is capsized and damaged. Not going to sail again";
		entityType.Description = "\n NAME: Welcome Winds\n \n HOMEPORT: Spoakdale\n \n TYPE:Catamaran\n \n BUILD YEAR:2422\n \n LENGTH: 22 m";
		entityType.ThumbnailSmall = "HUD_thumbnail_placeholder";
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "catamaran",
						Offset = new Vector2(-5f, -12f)
					}
				},
				RenderAsGroundSpriteType = new RenderAsGroundSpriteType
				{
					AssetName = "catamaran_g"
				}
			}
		};
		entityType.DefaultSimState = new SimStateInfo
		{
			GeometryLayoutType = new GeometryLayoutType
			{
				Shapes = new CollideShape2D[2]
				{
					new CollideShape2D(new Vector2(0f, 0f), 25f)
					{
						Offset = new Vector2(-5f, -12f)
					},
					new CollideShape2D(new Vector2(0f, 0f), 46f)
					{
						Offset = new Vector2(-5f, -12f)
					}
				}
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = false
		};
		entityType.DetectionTag = "kindaHardToSpot";
		entityType.IsSelectable = true;
		EntityType item4 = entityType;
		listOfEntityTypes.Add(item4);
		entityType = new EntityType("terrain:fishTrapSpotCreek");
		entityType.IsNeverInFogOfWar = false;
		entityType.UsesMemory = true;
		entityType.ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always;
		entityType.Name = "Fish weir location";
		entityType.SummaryDescription = "This spot is suitable for setting up a fish weir.";
		entityType.Description = "A fish trap made of wooden fences could catch a large number of fish when they migrate through this body of water.";
		entityType.ThumbnailSmall = "HUD_thumbnail_placeholder";
		entityType.EditorRenderableType = new RenderableType
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
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true,
			IsSpecialInterestFeature = true
		};
		entityType.DetectionTag = "inDeeperWaterFishingSpot";
		entityType.IsSelectable = true;
		entityType.SharedSpecialActions = new Pair<string, bool>[2]
		{
			new Pair<string, bool>("placeFishTrapCreekSticks", second: true),
			new Pair<string, bool>("placeFishTrapCreekNet", second: true)
		};
		EntityType item5 = entityType;
		listOfEntityTypes.Add(item5);
		entityType = new EntityType("terrain:fishTrapSpotCoast");
		entityType.IsNeverInFogOfWar = false;
		entityType.UsesMemory = true;
		entityType.ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always;
		entityType.Name = "Fish trap location (saltwater)";
		entityType.SummaryDescription = "This spot is suitable for setting up a fyke to catch the 'streak fin'";
		entityType.Description = "N/A";
		entityType.ThumbnailSmall = "HUD_thumbnail_placeholder";
		entityType.EditorRenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsGroundSpriteType = new RenderAsGroundSpriteType
				{
					AssetName = "fishTrapFyke_g"
				}
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true,
			IsSpecialInterestFeature = true
		};
		entityType.DetectionTag = "inDeeperWaterFishingSpot";
		entityType.IsSelectable = true;
		entityType.SharedSpecialActions = new Pair<string, bool>[1]
		{
			new Pair<string, bool>("placeFishTrapCoast", second: true)
		};
		EntityType item6 = entityType;
		listOfEntityTypes.Add(item6);
		entityType = new EntityType("terrain:fishTrapSpotShore");
		entityType.IsNeverInFogOfWar = false;
		entityType.UsesMemory = true;
		entityType.ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always;
		entityType.Name = "Fish trap location (freshwater)";
		entityType.SummaryDescription = "This spot is suitable for setting up a fish trap to catch the 'carbon tail'";
		entityType.Description = "N/A";
		entityType.ThumbnailSmall = "HUD_thumbnail_placeholder";
		entityType.EditorRenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsGroundSpriteType = new RenderAsGroundSpriteType
				{
					AssetName = "fishTrapCylinderSmall_g"
				}
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true,
			IsSpecialInterestFeature = true
		};
		entityType.DetectionTag = "inDeeperWaterFishingSpot";
		entityType.IsSelectable = true;
		entityType.SharedSpecialActions = new Pair<string, bool>[2]
		{
			new Pair<string, bool>("placeFishTrapShoreBasket", second: true),
			new Pair<string, bool>("placeFishTrapShoreHoopNet", second: true)
		};
		EntityType item7 = entityType;
		listOfEntityTypes.Add(item7);
		entityType = new EntityType("terrain:pierSpot");
		entityType.RequiresRollToDetect = false;
		entityType.IsNeverInFogOfWar = false;
		entityType.UsesMemory = true;
		entityType.ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always;
		entityType.Name = "Port location";
		entityType.SummaryDescription = "Location suitable for setting up a boat landing or small port";
		entityType.Description = "This location is navigable for boats and barges that come in from the sea. They can moor here if we build a landing or a small port.";
		entityType.ThumbnailSmall = "HUD_thumbnail_placeholder";
		entityType.EditorRenderableType = new RenderableType
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
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true,
			IsSpecialInterestFeature = true
		};
		entityType.IsSelectable = true;
		entityType.SharedSpecialActions = new Pair<string, bool>[3]
		{
			new Pair<string, bool>("buildSimplePort", second: true),
			new Pair<string, bool>("buildCanopyPort", second: true),
			new Pair<string, bool>("buildImprovisedLanding", second: true)
		};
		EntityType item8 = entityType;
		listOfEntityTypes.Add(item8);
		listOfEntityTypes.Add(new EntityType("terrain:clayDeposit")
		{
			IsNeverInFogOfWar = false,
			UsesMemory = true,
			ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always,
			Name = "Clay deposit",
			SummaryDescription = "A suitable place to dig a clay pit",
			Description = "This area has a large clay deposit below the surface that can be extracted if we establish a clay pit.",
			ThumbnailSmall = "HUD_thumbnail_placeholder",
			EditorRenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "clayPit_g"
					}
				}
			},
			TerrainType = new TerrainFeatureType
			{
				CanBeMapEditorPlaced = true,
				IsSpecialInterestFeature = true
			},
			DetectionTag = "farmSpotAndResourceDeposit",
			IsSelectable = true,
			SharedSpecialActions = new Pair<string, bool>[1]
			{
				new Pair<string, bool>("establishClayPit", second: true)
			}
		});
		listOfEntityTypes.Add(new EntityType("terrain:saltDeposit")
		{
			IsNeverInFogOfWar = false,
			UsesMemory = true,
			ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always,
			Name = "Salt deposit",
			SummaryDescription = "A suitable place to dig a salt mine",
			Description = "This area has a large rock salt deposit below the surface that can be extracted if we establish a salt mine.",
			ThumbnailSmall = "HUD_thumbnail_placeholder",
			EditorRenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "saltMine_g"
					}
				}
			},
			TerrainType = new TerrainFeatureType
			{
				CanBeMapEditorPlaced = true,
				IsSpecialInterestFeature = true
			},
			DetectionTag = "farmSpotAndResourceDeposit",
			IsSelectable = true,
			SharedSpecialActions = new Pair<string, bool>[1]
			{
				new Pair<string, bool>("establishSaltMine", second: true)
			}
		});
		listOfEntityTypes.Add(new EntityType("terrain:bogOreDeposit")
		{
			IsNeverInFogOfWar = false,
			UsesMemory = true,
			ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always,
			Name = "Bog ore deposit",
			SummaryDescription = "A suitable place to establish a bog ore pit",
			Description = "This area has a large bog ore deposit below the surface that can be extracted if we establish a pit.",
			ThumbnailSmall = "HUD_thumbnail_placeholder",
			EditorRenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "bogOrePit_g"
					}
				}
			},
			TerrainType = new TerrainFeatureType
			{
				CanBeMapEditorPlaced = true,
				IsSpecialInterestFeature = true
			},
			DetectionTag = "farmSpotAndResourceDeposit",
			IsSelectable = true,
			SharedSpecialActions = new Pair<string, bool>[1]
			{
				new Pair<string, bool>("establishBogOrePit", second: true)
			}
		});
		listOfEntityTypes.Add(new EntityType("terrain:rareMetalOreDeposit1")
		{
			IsNeverInFogOfWar = false,
			UsesMemory = true,
			ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always,
			Name = "Scandium deposit",
			SummaryDescription = "A site with a high concentration of scandium",
			Description = "The rare-earth metal scandium is usually difficult to mine but this area has highly concentrated scandium ores below the surface that can be extracted if we establish a simple pit.",
			ThumbnailSmall = "HUD_thumbnail_placeholder",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "terrainBlockerWide_g"
					}
				}
			},
			TerrainType = new TerrainFeatureType
			{
				CanBeMapEditorPlaced = false,
				IsSpecialInterestFeature = true
			},
			DetectionTag = "farmSpotAndResourceDeposit",
			IsSelectable = true,
			SharedSpecialActions = new Pair<string, bool>[1]
			{
				new Pair<string, bool>("establishRareMetalOrePit", second: true)
			}
		});
		listOfEntityTypes.Add(new EntityType("terrain:rareMetalOreDeposit2")
		{
			IsNeverInFogOfWar = false,
			UsesMemory = true,
			ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always,
			Name = "Terbium deposit",
			SummaryDescription = "A site with a high concentration of terbium",
			Description = "The rare-earth metal terbium is usually difficult to mine but this area has highly concentrated terbium ores below the surface that can be extracted if we establish a simple pit.",
			ThumbnailSmall = "HUD_thumbnail_placeholder",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "terrainBlockerWide_g"
					}
				}
			},
			TerrainType = new TerrainFeatureType
			{
				CanBeMapEditorPlaced = false,
				IsSpecialInterestFeature = true
			},
			DetectionTag = "farmSpotAndResourceDeposit",
			IsSelectable = true,
			SharedSpecialActions = new Pair<string, bool>[1]
			{
				new Pair<string, bool>("establishRareMetalOrePit2", second: true)
			}
		});
		listOfEntityTypes.Add(new EntityType("terrain:peatDeposit")
		{
			IsNeverInFogOfWar = false,
			UsesMemory = true,
			ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always,
			Name = "Peat deposit",
			SummaryDescription = "A suitable place to establish a peat bank where peat fuel can be cut",
			Description = "This grassy area of bog or marsh has a large deposit of peat just below the surface that can be extracted if we dig down through the firegrass sod.",
			ThumbnailSmall = "HUD_thumbnail_placeholder",
			EditorRenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "peatBank_g"
					}
				}
			},
			TerrainType = new TerrainFeatureType
			{
				CanBeMapEditorPlaced = true,
				IsSpecialInterestFeature = true
			},
			DetectionTag = "farmSpotAndResourceDeposit",
			IsSelectable = true,
			SharedSpecialActions = new Pair<string, bool>[1]
			{
				new Pair<string, bool>("establishPeatBank", second: true)
			}
		});
		entityType = new EntityType("terrain:smallPlotSpot");
		entityType.IsNeverInFogOfWar = false;
		entityType.UsesMemory = true;
		entityType.ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always;
		entityType.Name = "Small arable plot";
		entityType.SummaryDescription = "A small farm plot could be made here.";
		entityType.Description = "This area of firegrass soil is free from rocks and has the right conditions for establishing a small farm plot. First, the land would need to be tilled.";
		entityType.ThumbnailSmall = "HUD_thumbnail_placeholder";
		entityType.EditorRenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsGroundSpriteType = new RenderAsGroundSpriteType
				{
					AssetName = "plotPlowed_g"
				}
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true,
			IsSpecialInterestFeature = true
		};
		entityType.DetectionTag = "farmSpotAndResourceDeposit";
		entityType.IsSelectable = true;
		entityType.SharedSpecialActions = new Pair<string, bool>[1]
		{
			new Pair<string, bool>("establishSmallPlot", second: true)
		};
		EntityType item9 = entityType;
		listOfEntityTypes.Add(item9);
		entityType = new EntityType("terrain:largePlotSpot");
		entityType.IsNeverInFogOfWar = false;
		entityType.UsesMemory = true;
		entityType.ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always;
		entityType.Name = "Large arable plot";
		entityType.SummaryDescription = "A large farm plot could be made here.";
		entityType.Description = "This area of firegrass soil is free from rocks and has the right conditions for establishing a large farm plot. First, the land would need to be tilled.";
		entityType.ThumbnailSmall = "HUD_thumbnail_placeholder";
		entityType.EditorRenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsGroundSpriteType = new RenderAsGroundSpriteType
				{
					AssetName = "plotLargePlowed_g"
				}
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true,
			IsSpecialInterestFeature = true
		};
		entityType.DetectionTag = "farmSpotAndResourceDeposit";
		entityType.IsSelectable = true;
		entityType.SharedSpecialActions = new Pair<string, bool>[1]
		{
			new Pair<string, bool>("establishLargePlot", second: true)
		};
		EntityType item10 = entityType;
		listOfEntityTypes.Add(item10);
		AddRockTypeWithGeoLayout(listOfEntityTypes, "rockwall1", 2f, 12f, MakeRectangle(2f, 2f, 24f, 18f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "rockwall2", 2f, 30f, MakeRectangle(0f, 2f, 53f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "rockwall3", 1f, 12f, MakeRectangle(1f, 4f, 17f, 18f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "rockwall4", 2f, 30f, MakeRectangle(-1f, 2f, 35f, 18f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "rockwall5", 1f, 60f, MakeRectangle(-3f, 5f, 65f, 32f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "rockwall6", 1f, 5f, MakeRectangle(1f, 2f, 8f, 4f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "rockwall7", 1f, 2f, MakeRectangle(-2f, -3f, 8f, 4f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "rockwall8", 1.5f, 8f, MakeRectangle(-5f, 2f, 25f, 18f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "rockwall9", 2f, 30f, MakeRectangle(-3f, 2f, 27f, 18f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "ovalrocks1", 2f, 18f, MakeRectangle(-1f, 2f, 19f, 16f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "ovalrocks2", 2f, 30f, MakeRectangle(3f, -3f, 39f, 18f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "ovalrocks3", 2f, 50f, MakeRectangle(3f, 2f, 39f, 26f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "ovalrocks4", 2f, 100f, MakeRectangle(-6f, -1f, 57f, 24f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "ovalrocks5", 1.5f, 20f, MakeRectangle(-1f, 1f, 25f, 16f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "ovalrocks6", 2f, 12f, MakeRectangle(-1f, 2f, 23f, 16f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "ovalrocks7", 3f, 100f, MakeRectangle(-7f, -2f, 79f, 18f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "ovalrocks8", 2f, 30f, MakeRectangle(-3f, 2f, 39f, 18f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "ovalrocks9", 2.5f, 120f, MakeRectangle(-1f, 0f, 91f, 20f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "ovalrocks10", 1f, 10f, MakeRectangle(-1f, 2f, 19f, 4f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "ovalrocksAlgae1", 2f, 18f, MakeRectangle(-1f, 2f, 19f, 16f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "ovalrocksAlgae2", 2f, 30f, MakeRectangle(3f, -3f, 39f, 18f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "ovalrocksAlgae3", 2f, 50f, MakeRectangle(3f, 2f, 39f, 26f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "ovalrocksAlgae4", 2f, 100f, MakeRectangle(-6f, -1f, 57f, 24f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "ovalrocksAlgae5", 1.5f, 20f, MakeRectangle(-1f, 1f, 25f, 16f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "ovalrocksAlgae6", 2f, 12f, MakeRectangle(-1f, 2f, 23f, 16f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "ovalrocksAlgae7", 3f, 100f, MakeRectangle(-7f, -2f, 79f, 18f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "ovalrocksAlgae8", 2f, 30f, MakeRectangle(-3f, 2f, 39f, 18f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "ovalrocksAlgae9", 2.5f, 120f, MakeRectangle(-1f, 0f, 91f, 20f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "ovalrocksAlgae10", 1f, 10f, MakeRectangle(-1f, 2f, 19f, 4f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "limeBornholmrocks1", 1f, 50f, MakeRectangle(0f, 2f, 51f, 32f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "limeBornholmrocks2", 2.5f, 12f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "limeBornholmrocks3", 2f, 12f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "limeBornholmrocks4", 3f, 30f, MakeRectangle(0f, 2f, 65f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "limeBornholmrocks5", 1f, 20f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "limeBornholmrocks6", 1f, 26f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "limeOvalrocks1", 2f, 18f, MakeRectangle(-1f, 2f, 19f, 16f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "limeOvalrocks2", 2f, 30f, MakeRectangle(3f, -3f, 39f, 18f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "limeOvalrocks3", 2f, 50f, MakeRectangle(3f, 2f, 39f, 26f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "limeOvalrocks5", 1.5f, 20f, MakeRectangle(-1f, 1f, 25f, 16f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "limeOvalrocks6", 2f, 12f, MakeRectangle(-1f, 2f, 23f, 16f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "limeOvalrocks7", 3f, 100f, MakeRectangle(-7f, -2f, 79f, 18f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "limeOvalrocks8", 2f, 30f, MakeRectangle(-3f, 2f, 39f, 18f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "limeOvalrocks9", 2.5f, 120f, MakeRectangle(-1f, 0f, 91f, 20f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "limeDiagonalrocks2", 2.5f, 20f, MakeRectangle(0f, 2f, 49f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "limeDiagonalrocks3", 2.5f, 28f, MakeRectangle(0f, 2f, 65f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "limeDiagonalrocks4", 2f, 14f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "limeDiagonalrocks5", 1f, 10f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "limeDiagonalrocks6", 1.5f, 5f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "limeDiagonalrocks7", 3f, 100f, MakeRectangle(7f, 2f, 79f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "limeDiagonalrocks8", 1.5f, 12f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "limeDiagonalrocks9", 1.5f, 6f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "limeDiagonalrocks10", 2.5f, 18f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "limeMossrocks2", 2f, 50f, MakeRectangle(0f, 2f, 63f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "limeMossrocks4", 2f, 50f, MakeRectangle(-7f, 2f, 69f, 20f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "bornholmrocks1", 1f, 50f, MakeRectangle(0f, 2f, 51f, 32f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "bornholmrocks2", 2.5f, 12f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "bornholmrocks3", 2f, 12f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "bornholmrocks4", 3f, 30f, MakeRectangle(0f, 2f, 65f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "bornholmrocks5", 1f, 20f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "bornholmrocks6", 1f, 26f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "diagonalrocks1", 1.5f, 5f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "diagonalrocks2", 2.5f, 20f, MakeRectangle(0f, 2f, 49f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "diagonalrocks3", 2.5f, 28f, MakeRectangle(0f, 2f, 65f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "diagonalrocks4", 2f, 14f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "diagonalrocks5", 1f, 10f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "diagonalrocks6", 1.5f, 5f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "diagonalrocks7", 3f, 100f, MakeRectangle(7f, 2f, 79f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "diagonalrocks8", 1.5f, 12f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "diagonalrocks9", 1.5f, 6f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "diagonalrocks10", 2.5f, 18f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "diagonalrocks11", 2f, 6f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "diagonalrocks12", 1.5f, 20f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "legorocks1", 1f, 8f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "legorocks2", 1f, 20f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "legorocks3", 1.5f, 36f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "mossrocks1", 3f, 26f, MakeRectangle(3f, 5f, 65f, 18f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "mossrocks2", 2f, 50f, MakeRectangle(0f, 2f, 63f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "mossrocks3", 2f, 12f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "mossrocks4", 2f, 50f, MakeRectangle(-7f, 2f, 69f, 20f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "mossrocks5", 2f, 14f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "mossrocks6", 2f, 22f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "mossrocks7", 1f, 12f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "mossrocks8", 2f, 160f, MakeRectangle(0f, 4f, 105f, 30f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "pointyrocks1", 1f, 16f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "pointyrocks2", 1.5f, 28f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "pointyrocks3", 1f, 4f, MakeRectangle(0f, 0f, 17f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "pointyrocks4", 1.5f, 20f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "pointyrocks5", 1.5f, 9f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "scoobyrocks1", 1f, 14f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "scoobyrocks2", 1f, 5f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "scoobyrocks3", 1.5f, 8f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "scoobyrocks4", 1f, 12f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "slabrocks1", 1.5f, 9f, MakeRectangle(0f, 0f, 8f, 4f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "slabrocks2", 3f, 70f, MakeRectangle(0f, -6f, 71f, 4f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "slabrocks3", 1f, 5f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "slabrocks4", 1f, 42f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "slabrocks5", 2f, 28f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "slabrocks6", 1.5f, 28f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "slabrocks7", 1f, 5f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "squarerocks1", 2f, 20f, MakeRectangle(2f, -5f, 43f, 20f));
		AddRockType(listOfEntityTypes, "squarerocks2", 1f, 5f);
		AddRockType(listOfEntityTypes, "squarerocks3", 1f, 4f);
		AddRockTypeWithGeoLayout(listOfEntityTypes, "squarerocks4", 2f, 40f, MakeRectangle(0f, 2f, 51f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "squarerocks5", 2f, 30f, MakeRectangle(0f, 4f, 43f, 20f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "squarerocks6", 2f, 20f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "squarerocks7", 2f, 20f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "squarerocks8", 2f, 10f, MakeRectangle(0f, 2f, 17f, 16f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "squarerocks9", 1f, 42f, MakeRectangle(0f, 3f, 59f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "squarerocks10", 3f, 40f, MakeRectangle(0f, 2f, 67f, 18f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "squarerocks11", 2f, 44f, MakeRectangle(0f, 2f, 67f, 18f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "squarerocks12", 2f, 22f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "squarerocks13", 1.5f, 50f, MakeRectangle(5f, 2f, 63f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sulfurrock1", 1f, 50f, MakeRectangle(0f, 2f, 51f, 32f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sulfurrock2", 3f, 30f, MakeRectangle(0f, 2f, 65f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sulfurrock3", 2f, 30f, MakeRectangle(0f, 2f, 53f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sulfurrock4", 1f, 12f, MakeRectangle(1f, 4f, 17f, 18f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sulfurrock5", 3f, 40f, MakeRectangle(0f, 2f, 67f, 18f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sulfurrock6", 1f, 60f, MakeRectangle(-3f, 5f, 65f, 32f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sulfurrock7", 1.5f, 8f, MakeRectangle(-5f, 2f, 25f, 18f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sulfurrock8", 2f, 30f, MakeRectangle(-3f, 2f, 27f, 18f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sulfurrock9", 1f, 42f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sulfurrock10", 2f, 28f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sulfurrock11", 2f, 40f, MakeRectangle(0f, 2f, 51f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sulfurrock12", 2f, 30f, MakeRectangle(0f, 4f, 43f, 20f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sulfurrock13", 2f, 20f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sulfurrock14", 1f, 42f, MakeRectangle(0f, 3f, 59f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "hilljutland", 2f, 10000f, new GeometryLayoutType
		{
			Shapes = new CollideShape2D[4]
			{
				MakeRectangleShape(-48f, 99f, 266f, 36f),
				MakeRectangleShape(119f, 109f, 83f, 34f),
				MakeRectangleShape(-19f, 54f, 200f, 184f),
				MakeRectangleShape(57f, 45f, 147f, 164f)
			}
		});
		AddRockTypeWithGeoLayout(listOfEntityTypes, "hilljutlandEarth", 2f, 10000f, new GeometryLayoutType
		{
			Shapes = new CollideShape2D[4]
			{
				MakeRectangleShape(-48f, 99f, 266f, 36f),
				MakeRectangleShape(119f, 109f, 83f, 34f),
				MakeRectangleShape(-19f, 54f, 200f, 184f),
				MakeRectangleShape(57f, 45f, 147f, 164f)
			}
		});
		AddRockTypeWithGeoLayout(listOfEntityTypes, "hilljutlandMuckroot", 2f, 10000f, new GeometryLayoutType
		{
			Shapes = new CollideShape2D[4]
			{
				MakeRectangleShape(-48f, 99f, 266f, 36f),
				MakeRectangleShape(119f, 109f, 83f, 34f),
				MakeRectangleShape(-19f, 54f, 200f, 184f),
				MakeRectangleShape(57f, 45f, 147f, 164f)
			}
		});
		AddRockTypeWithGeoLayout(listOfEntityTypes, "hillfaroe", 2f, 1000f, new GeometryLayoutType
		{
			Shapes = new CollideShape2D[2]
			{
				MakeRectangleShape(-14f, 26f, 210f, 74f),
				MakeRectangleShape(87f, 34f, 59f, 38f)
			}
		});
		AddRockTypeWithGeoLayout(listOfEntityTypes, "hillfaroeEarth", 2f, 1000f, new GeometryLayoutType
		{
			Shapes = new CollideShape2D[2]
			{
				MakeRectangleShape(-14f, 26f, 210f, 74f),
				MakeRectangleShape(87f, 34f, 59f, 38f)
			}
		});
		AddRockTypeWithGeoLayout(listOfEntityTypes, "hillfaroeMuckroot", 2f, 1000f, new GeometryLayoutType
		{
			Shapes = new CollideShape2D[2]
			{
				MakeRectangleShape(-14f, 26f, 210f, 74f),
				MakeRectangleShape(87f, 34f, 59f, 38f)
			}
		});
		AddRockTypeWithGeoLayout(listOfEntityTypes, "limeHillfaroe", 2f, 1000f, new GeometryLayoutType
		{
			Shapes = new CollideShape2D[2]
			{
				MakeRectangleShape(-14f, 26f, 210f, 74f),
				MakeRectangleShape(87f, 34f, 59f, 38f)
			}
		});
		AddRockTypeWithGeoLayout(listOfEntityTypes, "hillorkney", 2f, 1000f, new GeometryLayoutType
		{
			Shapes = new CollideShape2D[4]
			{
				MakeRectangleShape(-60f, 12f, 165f, 84f),
				MakeRectangleShape(-18f, 45f, 119f, 60f),
				MakeRectangleShape(57f, 2f, 190f, 30f),
				MakeRectangleShape(73f, 19f, 81f, 84f)
			}
		});
		AddRockTypeWithGeoLayout(listOfEntityTypes, "hillorkneyEarth", 2f, 1000f, new GeometryLayoutType
		{
			Shapes = new CollideShape2D[4]
			{
				MakeRectangleShape(-60f, 12f, 165f, 84f),
				MakeRectangleShape(-18f, 45f, 119f, 60f),
				MakeRectangleShape(57f, 2f, 190f, 30f),
				MakeRectangleShape(73f, 19f, 81f, 84f)
			}
		});
		AddRockTypeWithGeoLayout(listOfEntityTypes, "hillorkneyMuckroot", 2f, 1000f, new GeometryLayoutType
		{
			Shapes = new CollideShape2D[4]
			{
				MakeRectangleShape(-60f, 12f, 165f, 84f),
				MakeRectangleShape(-18f, 45f, 119f, 60f),
				MakeRectangleShape(57f, 2f, 190f, 30f),
				MakeRectangleShape(73f, 19f, 81f, 84f)
			}
		});
		AddRockTypeWithGeoLayout(listOfEntityTypes, "limeHillorkney", 2f, 1000f, new GeometryLayoutType
		{
			Shapes = new CollideShape2D[4]
			{
				MakeRectangleShape(-60f, 12f, 165f, 84f),
				MakeRectangleShape(-18f, 45f, 119f, 60f),
				MakeRectangleShape(57f, 2f, 190f, 30f),
				MakeRectangleShape(73f, 19f, 81f, 84f)
			}
		});
		AddRockTypeWithGeoLayout(listOfEntityTypes, "hillgreece", 2f, 10000f, new GeometryLayoutType
		{
			Shapes = new CollideShape2D[4]
			{
				MakeRectangleShape(-122f, 32f, 163f, 110f),
				MakeRectangleShape(-12f, 19f, 211f, 198f),
				MakeRectangleShape(29f, 5f, 225f, 174f),
				MakeRectangleShape(150f, 15f, 163f, 86f)
			}
		});
		AddRockTypeWithGeoLayout(listOfEntityTypes, "hillgreeceEarth", 2f, 10000f, new GeometryLayoutType
		{
			Shapes = new CollideShape2D[4]
			{
				MakeRectangleShape(-122f, 32f, 163f, 110f),
				MakeRectangleShape(-12f, 19f, 211f, 198f),
				MakeRectangleShape(29f, 5f, 225f, 174f),
				MakeRectangleShape(150f, 15f, 163f, 86f)
			}
		});
		AddRockTypeWithGeoLayout(listOfEntityTypes, "hillgreeceMuckroot", 2f, 10000f, new GeometryLayoutType
		{
			Shapes = new CollideShape2D[4]
			{
				MakeRectangleShape(-122f, 32f, 163f, 110f),
				MakeRectangleShape(-12f, 19f, 211f, 198f),
				MakeRectangleShape(29f, 5f, 225f, 174f),
				MakeRectangleShape(150f, 15f, 163f, 86f)
			}
		});
		AddRockTypeWithGeoLayout(listOfEntityTypes, "hillsaltholm", 2f, 600f, new GeometryLayoutType
		{
			Shapes = new CollideShape2D[2]
			{
				MakeRectangleShape(-9f, 6f, 135f, 60f),
				MakeRectangleShape(11f, 30f, 160f, 30f)
			}
		});
		AddRockTypeWithGeoLayout(listOfEntityTypes, "hillsaltholmEarth", 2f, 600f, new GeometryLayoutType
		{
			Shapes = new CollideShape2D[2]
			{
				MakeRectangleShape(-9f, 6f, 135f, 60f),
				MakeRectangleShape(11f, 30f, 160f, 30f)
			}
		});
		AddRockTypeWithGeoLayout(listOfEntityTypes, "hillsaltholmMuckroot", 2f, 600f, new GeometryLayoutType
		{
			Shapes = new CollideShape2D[2]
			{
				MakeRectangleShape(-9f, 6f, 135f, 60f),
				MakeRectangleShape(11f, 30f, 160f, 30f)
			}
		});
		AddRockTypeWithGeoLayout(listOfEntityTypes, "limeHillsaltholm", 2f, 600f, new GeometryLayoutType
		{
			Shapes = new CollideShape2D[2]
			{
				MakeRectangleShape(-9f, 6f, 135f, 60f),
				MakeRectangleShape(11f, 30f, 160f, 30f)
			}
		});
		AddRockTypeWithGeoLayout(listOfEntityTypes, "hillshetland", 2f, 1000f, new GeometryLayoutType
		{
			Shapes = new CollideShape2D[3]
			{
				MakeRectangleShape(-11f, 41f, 213f, 94f),
				MakeRectangleShape(-1f, -15f, 163f, 60f),
				MakeCircleShape(6f, -23f, 70f)
			}
		});
		AddRockTypeWithGeoLayout(listOfEntityTypes, "hillshetlandEarth", 2f, 1000f, new GeometryLayoutType
		{
			Shapes = new CollideShape2D[3]
			{
				MakeRectangleShape(-11f, 41f, 213f, 94f),
				MakeRectangleShape(-1f, -15f, 163f, 60f),
				MakeCircleShape(6f, -23f, 70f)
			}
		});
		AddRockTypeWithGeoLayout(listOfEntityTypes, "hillshetlandMuckroot", 2f, 1000f, new GeometryLayoutType
		{
			Shapes = new CollideShape2D[3]
			{
				MakeRectangleShape(-11f, 41f, 213f, 94f),
				MakeRectangleShape(-1f, -15f, 163f, 60f),
				MakeCircleShape(6f, -23f, 70f)
			}
		});
		AddRockTypeWithGeoLayout(listOfEntityTypes, "rockLair", 2f, 100f, new GeometryLayoutType
		{
			Shapes = new CollideShape2D[2]
			{
				MakeRectangleShape(-25f, -18f, 135f, 38f),
				MakeRectangleShape(20f, -3f, 163f, 34f)
			}
		});
		AddRockGroundSpriteType(listOfEntityTypes, "rockLair_g");
		AddRockGroundSpriteType(listOfEntityTypes, "rockLairBones_g");
		AddRockGroundSpriteType(listOfEntityTypes, "bones_g");
		AddRockGroundSpriteType(listOfEntityTypes, "rockCreviceForRopeBridge_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "ropeBridge_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "rockCreviceMidSection_g", hasPointLayout: false);
		AddRockTypeWithGeoLayout(listOfEntityTypes, "rockCrevasse1", 2f, 100f, new GeometryLayoutType
		{
			Shapes = new CollideShape2D[2]
			{
				MakeRectangleShape(-9f, 0f, 139f, 48f),
				MakeRectangleShape(38f, -24f, 113f, 20f)
			}
		});
		AddRockGroundSpriteType(listOfEntityTypes, "candystalk_g", hasPointLayout: false);
		AddBillboardAnimTerrain(listOfEntityTypes, "Campfire", "campfireFast", "campfireFast");
		AddBillboardAnimTerrain(listOfEntityTypes, "Small campfire", "campfireSmall", "campfireSmall");
		AddBillboardAnimTerrain(listOfEntityTypes, "Fish circling", "fishCircling", "fishCircling");
		AddBillboardAnimTerrain(listOfEntityTypes, "Fish swarm", "fishSwarm", "fishSwarming");
		AddBillboardAnimTerrain(listOfEntityTypes, "Butterflies", "butterflies", "butterfliesSwarm");
		AddBillboardAnimTerrain(listOfEntityTypes, "Mosquito swarm", "mosquitoSwarm", "mosquitoSwarming");
		AddBillboardAnimTerrain(listOfEntityTypes, "Dragonfly", "dragonflies", "dragonflies");
		AddBillboardAnimTerrain(listOfEntityTypes, "Ground bugs", "groundBugs", "groundBugsSwarm");
		AddRockType(listOfEntityTypes, "holdenstreeFallenGrownDead1", 2f, 22f);
		AddRockType(listOfEntityTypes, "holdenstreeFallenGrownFresh1", 2f, 22f);
		AddRockType(listOfEntityTypes, "holdenstreeFallenGrownVines1", 2f, 22f);
		AddRockType(listOfEntityTypes, "holdenstreeFallenGrownNaked1", 2f, 22f);
		AddRockTypeWithGeoLayout(listOfEntityTypes, "utgardstowerBig", 1f, 280f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "utgardstowerTall", 1f, 180f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "utgardstowerSmall1", 1f, 18f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "utgardstowerSmall2", 2f, 18f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "utgardstowerSmall3", 1f, 8f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "utgardstowerSmall4", 2f, 30f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "utgardstowerSmall5", 1.5f, 26f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockGroundSpriteType(listOfEntityTypes, "utgardstower1_g");
		AddRockGroundSpriteTypeWithGeoLayout(listOfEntityTypes, "utgardstowerHole_g", MakeRectangle(0f, 2f, 31f, 22f));
		AddRockGroundSpriteTypeWithGeoLayout(listOfEntityTypes, "utgardstowerHoleWeb_g", MakeRectangle(0f, 2f, 31f, 22f));
		AddRockGroundSpriteTypeWithGeoLayout(listOfEntityTypes, "utgardstowerPondWeb_g", MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "japanesegarden", 1f, 30f, MakeRectangle(15f, 2f, 47f, 34f));
		AddRockGroundSpriteTypeWithGeoLayout(listOfEntityTypes, "japanesegarden_g", MakeRectangle(-35f, 2f, 22f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "doghouse", 1.5f, 12f, MakeRectangle(0f, 2f, 22f, 22f));
		AddRockGroundSpriteType(listOfEntityTypes, "doghouse_g", hasPointLayout: false);
		AddRockTypeWithGeoLayout(listOfEntityTypes, "clothesLine", 1.5f, 12f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "rabbitcages", 1.5f, 12f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "orchard", 2f, 12f, MakeRectangle(0f, -5f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "fenceDiagonal", 2f, 10f, new GeometryLayoutType
		{
			Shapes = new CollideShape2D[4]
			{
				MakeRectangleShape(-18f, 14f, 20f, 20f),
				MakeRectangleShape(-5f, 2f, 20f, 20f),
				MakeRectangleShape(8f, -13f, 20f, 20f),
				MakeRectangleShape(18f, -19f, 20f, 18f)
			}
		});
		AddRockTypeWithGeoLayout(listOfEntityTypes, "fenceDiagonalFlipped", 2f, 10f, new GeometryLayoutType
		{
			Shapes = new CollideShape2D[3]
			{
				MakeRectangleShape(-20f, -20f, 20f, 20f),
				MakeRectangleShape(-6f, -4f, 20f, 20f),
				MakeRectangleShape(9f, 5f, 20f, 20f)
			}
		});
		AddRockTypeWithGeoLayout(listOfEntityTypes, "fenceVertical", 2f, 10f, new GeometryLayoutType
		{
			Shapes = new CollideShape2D[1] { MakeRectangleShape(0f, 0f, 20f, 58f) }
		});
		AddRockTypeWithGeoLayout(listOfEntityTypes, "fenceHorizontal", 2f, 10f, new GeometryLayoutType
		{
			Shapes = new CollideShape2D[1] { MakeRectangleShape(0f, 0f, 108f, 20f) }
		});
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneFlat", 2f, 40f, MakeRectangle(0f, -1f, 73f, 34f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneFlatFiregrass", 2f, 40f, MakeRectangle(0f, -1f, 73f, 34f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneFlatCave", 2f, 40f, MakeRectangle(0f, -1f, 73f, 34f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneSquaretower", 1.5f, 50f, MakeRectangle(0f, -7f, 81f, 60f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneSquaretowerFiregrass", 1.5f, 50f, MakeRectangle(0f, -7f, 81f, 60f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneSquaretowerVines", 1.5f, 50f, MakeRectangle(0f, -7f, 81f, 60f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneSquaretowerGuano", 1.5f, 50f, MakeRectangle(0f, -7f, 81f, 60f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneChubbytower", 1f, 36f, MakeRectangle(0f, -4f, 55f, 44f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneChubbytowerFiregrass", 1f, 36f, MakeRectangle(0f, -4f, 55f, 44f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneCube", 1f, 20f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneCubeFiregrass", 1f, 20f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneHouse", 1f, 30f, MakeRectangle(0f, 2f, 47f, 34f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneHouseFiregrass", 1f, 30f, MakeRectangle(0f, 2f, 47f, 34f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneHouseCave", 1f, 30f, MakeRectangle(0f, 2f, 47f, 34f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneHouseGuano", 1f, 30f, MakeRectangle(0f, 2f, 47f, 34f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneNugget", 1.5f, 28f, MakeRectangle(-2f, -3f, 45f, 26f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneNuggetFiregrass", 1.5f, 28f, MakeRectangle(-2f, -3f, 45f, 26f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneNuggetGuano", 1.5f, 28f, MakeRectangle(-2f, -3f, 45f, 26f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneSlimtower", 1f, 50f, MakeRectangle(0f, -6f, 47f, 32f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneSlimtowerFiregrass", 1f, 50f, MakeRectangle(0f, -6f, 47f, 32f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneSlimtowerCave", 1f, 50f, MakeRectangle(0f, -6f, 47f, 32f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneSmall1", 1.5f, 12f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneSmall1Firegrass", 1.5f, 12f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneGuano1", 1.5f, 12f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneSmall2", 2f, 10f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneSmall2Firegrass", 2f, 10f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneSmall3", 2f, 12f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneSmall3Firegrass", 2f, 12f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneSmall4", 1f, 10f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneSmall4Firegrass", 1f, 10f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneSmall5", 1f, 5f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneSmall5Firegrass", 1f, 5f, MakeRectangle(0f, 2f, 31f, 22f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneSplittower", 2f, 110f, MakeRectangle(0f, -8f, 115f, 50f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneSplittowerFiregrass", 2f, 110f, MakeRectangle(0f, -8f, 115f, 50f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneSplittowerGuano", 2f, 110f, MakeRectangle(0f, -8f, 115f, 50f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneTerrace", 2f, 30f, MakeRectangle(4f, 2f, 51f, 32f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneTerraceFiregrass", 2f, 30f, MakeRectangle(4f, 2f, 51f, 32f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneTerraceVines", 2f, 30f, MakeRectangle(4f, 2f, 51f, 32f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneTerraceGuano", 2f, 30f, MakeRectangle(4f, 2f, 51f, 32f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneWalltower", 2f, 80f, MakeRectangle(-3f, -9f, 109f, 46f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstoneWalltowerFiregrass", 2f, 80f, MakeRectangle(-3f, -9f, 109f, 46f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstonePlateauOmelette", 1f, 800f, MakeRectangle(10f, 9f, 269f, 202f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstonePlateauPizza", 1f, 1000f, MakeRectangle(-10f, 2f, 329f, 188f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstonePlateauTortilla", 1.5f, 600f, MakeRectangle(-8f, 2f, 227f, 138f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstonePlateauCake", 1f, 700f, MakeRectangle(-16f, 17f, 255f, 124f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstonePlateauCaveVines", 1f, 700f, MakeRectangle(11f, 10f, 243f, 134f));
		AddRockTypeWithGeoLayout(listOfEntityTypes, "sandstonePlateauCakeBare", 1f, 700f, MakeRectangle(-16f, 17f, 255f, 124f));
		AddRockGroundSpriteTypeWithGeoLayout(listOfEntityTypes, "sandstoneCrevasseZigzag_g", MakeRectangle(0f, 0f, 150f, 44f));
		AddRockGroundSpriteTypeWithGeoLayout(listOfEntityTypes, "sandstoneCrevasseStraight_g", MakeRectangle(0f, -2f, 108f, 20f));
		AddRockGroundSpriteType(listOfEntityTypes, "scatterRocks1_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "scatterRocks2_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "scatterRocks3_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "scatterRocks4_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "scatterRocks5_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "scatterRocks6_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "scatterRocks7_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "scatterRocks8_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "scatterRocks9_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "scatterLimestone1_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "scatterLimestone2_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "scatterLimestone3_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "scatterLimestone4_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "scatterLimestone5_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "scatterLimestone6_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "scatterLimestone7_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "scatterLimestone8_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "scatterLimestone9_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "brambleRoots1_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "brambleRoots2_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "brambleRoots3_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "brambleRootsSmall_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "vines1_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "vines1Bare_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "vines2_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "vines2Bare_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "vines3Bare_g", hasPointLayout: false);
		AddRockGroundSpriteTypeWithGeoLayout(listOfEntityTypes, "holeSoil_g", MakeRectangle(0f, 2f, 31f, 22f));
		AddRockGroundSpriteTypeWithGeoLayout(listOfEntityTypes, "holeSoilSmall_g", MakeRectangle(0f, 2f, 31f, 22f));
		AddRockGroundSpriteType(listOfEntityTypes, "earthpatchLong_g");
		AddRockGroundSpriteTypeWithGeoLayout(listOfEntityTypes, "demonring_g", MakeRectangle(-3f, -1f, 107f, 68f));
		AddRockGroundSpriteType(listOfEntityTypes, "goodieshrub1_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "goodieshrub2_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "goodieshrub3_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "goodieshrub4_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "goodieshrub5_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "seaweedDead1_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "seaweedDead2_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "seaweedDead3_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "seaweedDead4_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "seaweedDead5_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "seaweedDead6_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "seaweedDeadPale1_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "seaweedDeadPale2_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "seaweedDeadPale3_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "seaweedDeadPale4_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "seaweedDeadPale5_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "seaweedDeadPale6_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "moss1_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "moss2_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "moss3_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "moss4_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "moss5_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "moss6_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "mossDesert1_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "mossDesert2_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "mossDesert3_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "mossDesert4_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "mossDesert5_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "mossDesert6_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "sealilies1_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "sealiliesDead1_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "daffodils1_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "daffodils2_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "thicketbare1_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "thicketbare2_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "thicketbare3_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "thicketbare4_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "thicketflower1_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "thicketflower2_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "thicketflower3_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "thicketbareDesert1_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "thicketbareDesert2_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "thicketbareDesert3_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "thicketbareDesert4_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "thicketflowerDesert1_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "thicketflowerDesert2_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "thicketflowerDesert3_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "blueShrooms1_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "blueShrooms2_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "blueShrooms3_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "waterlettuce1_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "waterlettuce2_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "waterlettuceDead1_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "pondfoil1_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "pondfoil2_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "pondfoilDead1_g", hasPointLayout: false);
		AddRockGroundSpriteTypeWithGeoLayout(listOfEntityTypes, "marshcotStem1_g", MakeRectangle(0f, 2f, 31f, 22f));
		AddRockGroundSpriteTypeWithGeoLayout(listOfEntityTypes, "marshcotStemDead1_g", MakeRectangle(0f, 2f, 31f, 22f));
		AddRockGroundSpriteType(listOfEntityTypes, "ditchHorizontal1_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "ditchVertical1_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "footpathfork_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "footpathcenter_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "footpathsouth_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "footpathDiagonal_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "footpathHorizontalLong_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "footpathHorizontalShort_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "footpathVerticalLong_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "footpathVerticalShort_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "footpathCurvy2_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "footpathCurvy3_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "footpathCurvy4_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "footpathCurvy5_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "footpathCurvyThin1_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "tilesPatch1_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "tilesStrip1_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "roadGravel_diagonal_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "roadGravel_horizontal_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "roadGravel_vertical_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "roadGravelES_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "roadGravelNE_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "roadGravelNSE_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "roadGravelNWE_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "roadGravelSNE_g", hasPointLayout: false);
		AddRockGroundSpriteType(listOfEntityTypes, "roadGravelSWE_g", hasPointLayout: false);
		AddRockGroundSpriteTypeWithGeoLayout(listOfEntityTypes, "terrainBlockerSmallAlmostInvisible_g", MakeRectangle(0f, 0f, 16f, 16f));
		AddRockGroundSpriteTypeWithGeoLayout(listOfEntityTypes, "terrainBlockerSmall_g", MakeRectangle(0f, 0f, 16f, 16f));
		AddRockGroundSpriteTypeWithGeoLayout(listOfEntityTypes, "terrainBlockerWide_g", MakeRectangle(0f, 0f, 150f, 44f));
	}

	private static EntityType AddBillboardAnimTerrain(List<EntityType> listOfEntityTypes, string name, string keyPostfix, string animationAsset = null)
	{
		EntityType entityType = new EntityType("terrain:" + keyPostfix);
		entityType.Name = name;
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AnimationAssetName = animationAsset
					}
				}
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true
		};
		EntityType entityType2 = entityType;
		listOfEntityTypes.Add(entityType2);
		return entityType2;
	}

	private static EntityType AddRockType(List<EntityType> listOfEntityTypes, string spriteName, float widthHeightRatio, float bulk, string animationAsset = null)
	{
		return AddRockType(listOfEntityTypes, "terrain:" + spriteName, spriteName, spriteName, widthHeightRatio, bulk, animationAsset);
	}

	private static EntityType AddRockType(List<EntityType> listOfEntityTypes, string keyName, string sprite, string name, float widthHeightRatio, float bulk, string animationAsset = null)
	{
		EntityType entityType = new EntityType(keyName);
		entityType.Name = name;
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true
		};
		entityType.DefaultSimState = new SimStateInfo
		{
			GeometryLayoutType = new GeometryLayoutType
			{
				Shapes = new CollideShape2D[1]
				{
					new CollideShape2D(Vector2.Zero, 15f)
				}
			}
		};
		entityType.RockType = new RockType
		{
			Bulk = bulk
		};
		EntityType entityType2 = entityType;
		if (animationAsset != null)
		{
			entityType2.RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AnimationAssetName = animationAsset,
							AspectRatio = widthHeightRatio
						}
					}
				}
			};
			entityType2.RockType.Animates = true;
		}
		else
		{
			entityType2.RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = sprite,
							AspectRatio = widthHeightRatio
						}
					}
				}
			};
		}
		listOfEntityTypes.Add(entityType2);
		return entityType2;
	}

	public static GeometryLayoutType MakeRectangle(float x, float y, float width, float height)
	{
		GeometryLayoutType geometryLayoutType = new GeometryLayoutType();
		geometryLayoutType.Shapes = new CollideShape2D[1]
		{
			new CollideShape2D((0f - height) * 0.5f, (0f - width) * 0.5f, height * 0.5f, width * 0.5f)
			{
				Offset = new Vector2(x, y)
			}
		};
		return geometryLayoutType;
	}

	private static CollideShape2D MakeRectangleShape(float x, float y, float width, float height)
	{
		return new CollideShape2D((0f - height) * 0.5f, (0f - width) * 0.5f, height * 0.5f, width * 0.5f)
		{
			Offset = new Vector2(x, y)
		};
	}

	private static CollideShape2D MakeCircleShape(float x, float y, float radius)
	{
		return new CollideShape2D(new Vector2(x, y), radius);
	}

	private static GeometryLayoutType MakeCircle(float x, float y, float radius)
	{
		GeometryLayoutType geometryLayoutType = new GeometryLayoutType();
		geometryLayoutType.Shapes = new CollideShape2D[1]
		{
			new CollideShape2D(new Vector2(x, y), radius)
		};
		return geometryLayoutType;
	}

	private static void AddRockWithGeoLayout()
	{
	}

	private static EntityType AddRockTypeWithGeoLayout(List<EntityType> listOfEntityTypes, string keyName, float widthHeightRatio, float bulk, GeometryLayoutType geoType)
	{
		EntityType entityType = AddRockTypeWithoutLayout(listOfEntityTypes, "terrain:" + keyName, keyName, keyName, widthHeightRatio, bulk);
		entityType.DefaultSimState = new SimStateInfo
		{
			GeometryLayoutType = geoType
		};
		return entityType;
	}

	private static EntityType AddRockTypeWithoutLayout(List<EntityType> listOfEntityTypes, string keyName, string sprite, string name, float widthHeightRatio, float bulk)
	{
		EntityType entityType = new EntityType(keyName);
		entityType.Name = name;
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = sprite,
						AspectRatio = widthHeightRatio
					}
				}
			}
		};
		entityType.TerrainType = new TerrainFeatureType
		{
			CanBeMapEditorPlaced = true
		};
		entityType.RockType = new RockType
		{
			Bulk = bulk
		};
		EntityType entityType2 = entityType;
		listOfEntityTypes.Add(entityType2);
		return entityType2;
	}

	private static EntityType AddRockGroundSpriteType(List<EntityType> listOfEntityTypes, string sprite, bool hasPointLayout = true)
	{
		EntityType entityType = new EntityType("terrain:" + sprite)
		{
			Name = sprite,
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = sprite
					}
				}
			},
			TerrainType = new TerrainFeatureType
			{
				CanBeMapEditorPlaced = true
			}
		};
		if (hasPointLayout)
		{
			entityType.PointLayoutType = new PointLayoutType();
		}
		listOfEntityTypes.Add(entityType);
		return entityType;
	}

	private static EntityType AddRockGroundSpriteTypeWithGeoLayout(List<EntityType> listOfEntityTypes, string sprite, GeometryLayoutType geoType)
	{
		EntityType entityType = new EntityType("terrain:" + sprite)
		{
			Name = sprite,
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = sprite
					}
				}
			},
			TerrainType = new TerrainFeatureType
			{
				CanBeMapEditorPlaced = true
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = geoType
			}
		};
		listOfEntityTypes.Add(entityType);
		return entityType;
	}
}
