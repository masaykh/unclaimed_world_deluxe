using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Collisions;
using UWGame.SimSide.Entities;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_3.Data;

internal class StructureLoader
{
	public static void Init(List<EntityType> listOfEntityTypes)
	{
		float pad = 20f;
		listOfEntityTypes.Add(new EntityType("structure:signalPyre")
		{
			Name = "Signal pyre",
			SummaryDescription = "A fire that will produce a great deal of smoke, visible for miles",
			Description = "Built from a large amount of firewood covered with fresh spoak leaves. We should keep it burning as often as possible to increase chances of being found.",
			ThumbnailSmall = "HUD_thumbnail_signalPyre",
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
							AssetName = "signalPyre"
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
								AssetName = "signalPyre"
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
								AssetName = "signalPyre_construct"
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
					Pad = pad,
					PadShape = CollidePrim.Circle,
					Shapes = new CollideShape2D[1]
					{
						new CollideShape2D(new Vector2(0f, 0f), 22f)
						{
							Offset = new Vector2(0f, -2f)
						}
					}
				}
			},
			SharedSpecialActions = new Pair<string, bool>[1]
			{
				new Pair<string, bool>("lightSignalPyre", second: true)
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "ricketyConstruction",
				PartKeys = new SerializableDictionary<string, int> { { "item:spoakBranches", 1 } }
			}
		});
		listOfEntityTypes.Add(new EntityType("structure:boatWreck")
		{
			Name = "Catamaran wreck",
			ThumbnailSmall = "HUD_thumbnail_catamaran",
			SummaryDescription = "Our catamaran is capsized and damaged. Not going to sail again",
			Description = "\n NAME: Welcome Winds\n \n HOMEPORT: Noame\n \n TYPE:Catamaran\n \n BUILD YEAR:2422\n \n LENGTH: 22 m",
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
							AssetName = "catamaran",
							Offset = new Vector2(-5f, -12f)
						}
					},
					RenderAsGroundSpriteType = new RenderAsGroundSpriteType
					{
						AssetName = "catamaran_g"
					}
				}
			},
			DefaultSimState = new SimStateInfo
			{
				GeometryLayoutType = new GeometryLayoutType
				{
					Pad = pad,
					PadShape = CollidePrim.Circle,
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
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				DegradeType = "ricketyConstruction",
				SalvageProcess = "salvageBoatWreck",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:lines", 1 },
					{ "item:scrapMetal", 1 },
					{ "item:metalWire", 1 }
				}
			}
		});
	}
}
