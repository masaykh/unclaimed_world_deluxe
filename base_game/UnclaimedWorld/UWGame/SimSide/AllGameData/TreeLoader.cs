using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.Client.Particles;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Collisions;
using UWGame.SimSide.Entities;
using UWGame.SimSide.GatheringSites;
using UWGame.SimSide.Trees;

namespace UWGame.SimSide.AllGameData;

public class TreeLoader
{
	public static void Init(List<EntityType> listOfEntityTypes)
	{
		string description = "\n BIOLOGY OVERVIEW\n The tree has a symbiotic relationship with the blue 'favorbread' which grows in close proximity. The tree will provide the favorbread with carbohydrates and in return, the favorbread collects mineral nutrients for the tree and seems to also protect the tree against diseases and pests.\n \n SURVIVAL GUIDE NOTES\n The favorbread is edible to humans without any preparation and is a cherished source of nutrition for anyone staying in the wilderness.";
		string description2 = "\n BIOLOGY OVERVIEW\n The tree has a symbiotic relationship with the blue 'favorbread' which grows in close proximity. The tree will provide the favorbread with carbohydrates and in return, the favorbread collects mineral nutrients for the tree and seems to also protect the tree against diseases and pests.\n \n SURVIVAL GUIDE NOTES\n The favorbread is edible and can be cultivated in a relatively simple way by excavating a pit next to a DEAD sanctuary tree. When we provide the favorbread with a source of carbohydrate, such as blackpulp, we can 'revive' the plant and make it grow fruits again.\n \n Note: This method of cultivation is not possible next to a LIVING sanctuary tree because disturbing the connection between the two organisms causes a dangerous, defensive response.";
		float bendyness = 0.8f;
		EntityType entityType = new EntityType("tree:shadeleaf");
		entityType.Name = "Shadeleaf";
		entityType.SummaryDescription = "Small deciduous tree that bends in the wind.";
		entityType.Description = "\n \n SURVIVAL GUIDE NOTES\n Though shadeleaf may be fit for smaller creatures attempting to shade themselves, its leaves are too thin and brittle to be of much use in crafting. However, the shadeleaf's thin, pliant limbs are particularly useful in small-scale shelter construction. Shadeleaf cane is also suitable when manufacturing arrow shafts, as its flexibility is ideal when accounting for accuracy.";
		entityType.ThumbnailSmall = "HUD_thumbnail_shadeleaf";
		entityType.TreeType = new TreeType
		{
			BulkPerSize = 4f,
			MatureAge = 2f,
			MaxAge = 12f,
			SizeImpact = 0.6f,
			MaxFlavours = 4,
			FibrousPercentageOfTotalMass = 0f,
			LumberPercentageOfFiberMass = 0f,
			Crops = new string[3] { "crop:shadeleafCanes", "crop:shadeleafBowStave", "crop:sticks" },
			DefaultCrops = new DefaultCrops[3]
			{
				new DefaultCrops
				{
					KeyName = "crop:shadeleafCanes",
					MinItemsForFullGrownPlant = 0,
					MaxItemsForFullGrownPlant = 1
				},
				new DefaultCrops
				{
					KeyName = "crop:shadeleafBowStave",
					MinItemsForFullGrownPlant = 0,
					MaxItemsForFullGrownPlant = 1
				},
				new DefaultCrops
				{
					KeyName = "crop:sticks",
					MinItemsForFullGrownPlant = 0,
					MaxItemsForFullGrownPlant = 1
				}
			}
		};
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "tree_shadeleaf_1_grown_summer"
					}
				}
			},
			ClientStateConditions = new ClientStateInfo[40]
			{
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_shadeleaf_1_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 10)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_shadeleaf_2_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 10)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_shadeleaf_3_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23, 10)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_shadeleaf_4_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 24, 10)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_shadeleaf_1_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 10, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_shadeleaf_2_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 10, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_shadeleaf_3_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23, 10, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_shadeleaf_4_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 24, 10, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_shadeleaf_1_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_shadeleaf_2_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_shadeleaf_3_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_shadeleaf_4_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 24)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_shadeleaf_1_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_shadeleaf_2_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_shadeleaf_3_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_shadeleaf_4_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 24, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_shadeleaf_1_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_shadeleaf_2_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_shadeleaf_3_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_shadeleaf_4_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 24, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_shadeleaf_1_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 15, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_shadeleaf_2_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 15, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_shadeleaf_3_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23, 15, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_shadeleaf_4_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 24, 15, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_shadeleafdead_1_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 18)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_shadeleafdead_2_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 18)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_shadeleafdead_1_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23, 18)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_shadeleafdead_2_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 24, 18)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_shadeleafdead_1_young",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 15, 18)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_shadeleafdead_2_young",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 15, 18)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_shadeleafdead_1_young",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23, 15, 18)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_shadeleafdead_2_young",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 24, 15, 18)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_shadeleafdead_1_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 18, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_shadeleafdead_2_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 18, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_shadeleafdead_1_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23, 18, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_shadeleafdead_2_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 24, 18, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_shadeleafdead_1_young",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 15, 18, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_shadeleafdead_2_young",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 15, 18, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_shadeleafdead_1_young",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23, 15, 18, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_shadeleafdead_2_young",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 24, 15, 18, 14)
				}
			}
		};
		entityType.PointLayoutType = new PointLayoutType();
		EntityType item = entityType;
		listOfEntityTypes.Add(item);
		bendyness = 0.8f;
		entityType = new EntityType("tree:paleshadeleaf");
		entityType.Name = "Pale shadeleaf";
		entityType.SummaryDescription = "Small deciduous tree that bends in the wind.";
		entityType.Description = "\n \n SURVIVAL GUIDE NOTES\n Though shadeleaf may be fit for smaller creatures attempting to shade themselves, its leaves are too thin and brittle to be of much use in crafting. However, the shadeleaf's thin, pliant limbs are particularly useful in small-scale shelter construction. Shadeleaf cane is also suitable when manufacturing arrow shafts, as its flexibility is ideal when accounting for accuracy.";
		entityType.ThumbnailSmall = "HUD_thumbnail_shadeleaf";
		entityType.TreeType = new TreeType
		{
			BulkPerSize = 4f,
			MatureAge = 2f,
			MaxAge = 12f,
			SizeImpact = 0.6f,
			MaxFlavours = 4,
			FibrousPercentageOfTotalMass = 0f,
			LumberPercentageOfFiberMass = 0f,
			Crops = new string[3] { "crop:shadeleafCanes", "crop:shadeleafBowStave", "crop:sticks" },
			DefaultCrops = new DefaultCrops[3]
			{
				new DefaultCrops
				{
					KeyName = "crop:shadeleafCanes",
					MinItemsForFullGrownPlant = 0,
					MaxItemsForFullGrownPlant = 1
				},
				new DefaultCrops
				{
					KeyName = "crop:shadeleafBowStave",
					MinItemsForFullGrownPlant = 0,
					MaxItemsForFullGrownPlant = 1
				},
				new DefaultCrops
				{
					KeyName = "crop:sticks",
					MinItemsForFullGrownPlant = 0,
					MaxItemsForFullGrownPlant = 1
				}
			}
		};
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "tree_shadeleaf_1_grown_summer"
					}
				}
			},
			ClientStateConditions = new ClientStateInfo[40]
			{
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_paleshadeleaf_1_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 10)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_paleshadeleaf_2_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 10)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_paleshadeleaf_3_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23, 10)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_paleshadeleaf_4_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 24, 10)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_paleshadeleaf_1_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 10, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_paleshadeleaf_2_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 10, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_paleshadeleaf_3_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23, 10, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_paleshadeleaf_4_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 24, 10, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_paleshadeleaf_1_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_paleshadeleaf_2_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_paleshadeleaf_3_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_paleshadeleaf_4_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 24)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_paleshadeleaf_1_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_paleshadeleaf_2_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_paleshadeleaf_3_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_paleshadeleaf_4_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 24, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_paleshadeleaf_1_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_paleshadeleaf_2_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_paleshadeleaf_3_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_paleshadeleaf_4_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 24, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_paleshadeleaf_1_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 15, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_paleshadeleaf_2_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 15, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_paleshadeleaf_3_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23, 15, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_paleshadeleaf_4_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 24, 15, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_paleshadeleafdead_1_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 18)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_paleshadeleafdead_2_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 18)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_paleshadeleafdead_1_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23, 18)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_paleshadeleafdead_2_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 24, 18)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_paleshadeleafdead_1_young",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 15, 18)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_paleshadeleafdead_2_young",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 15, 18)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_paleshadeleafdead_1_young",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23, 15, 18)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_paleshadeleafdead_2_young",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 24, 15, 18)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_paleshadeleafdead_1_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 18, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_paleshadeleafdead_2_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 18, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_paleshadeleafdead_1_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23, 18, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_paleshadeleafdead_2_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 24, 18, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_paleshadeleafdead_1_young",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 15, 18, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_paleshadeleafdead_2_young",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 15, 18, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_paleshadeleafdead_1_young",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23, 15, 18, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_paleshadeleafdead_2_young",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 24, 15, 18, 14)
				}
			}
		};
		entityType.PointLayoutType = new PointLayoutType();
		item = entityType;
		listOfEntityTypes.Add(item);
		bendyness = 0.7f;
		entityType = new EntityType("tree:shadeleafdead");
		entityType.Name = "Shadeleaf - dead";
		entityType.SummaryDescription = "Dead shadeleaf tree. Supplies a useful resin.";
		entityType.Description = "\n \n SURVIVAL GUIDE NOTES\n A beneficial resin can be found on dead shadeleaf trees: When attacked by scuttler bugs, the shadelaf tree produces and secretes a large amount of resin to ward off the bugs and protect other, nearby shadeleaf trees. We can harvest this resin and make use of its repellent effect by applying it to plant material thus protecting them against infestation. The resin also has adhesive properties and can be turned into a glue.";
		entityType.TreeType = new TreeType
		{
			BulkPerSize = 4f,
			MatureAge = 2f,
			MaxAge = 12f,
			SizeImpact = 0.6f,
			FibrousPercentageOfTotalMass = 0f,
			LumberPercentageOfFiberMass = 0f,
			Crops = new string[2] { "crop:sticks", "crop:shadeleafResin" },
			DefaultCrops = new DefaultCrops[2]
			{
				new DefaultCrops
				{
					KeyName = "crop:sticks",
					MinItemsForFullGrownPlant = 0,
					MaxItemsForFullGrownPlant = 1
				},
				new DefaultCrops
				{
					KeyName = "crop:shadeleafResin",
					MinItemsForFullGrownPlant = 0,
					MaxItemsForFullGrownPlant = 1
				}
			}
		};
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "tree_shadeleafdead_1_grown",
						Bendyness = bendyness
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
							AssetName = "tree_shadeleafdead_1_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_shadeleafdead_2_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22)
				}
			}
		};
		entityType.PointLayoutType = new PointLayoutType();
		item = entityType;
		listOfEntityTypes.Add(item);
		bendyness = 0.6f;
		entityType = new EntityType("tree:daysheen");
		entityType.Name = "Daysheen";
		entityType.SummaryDescription = "Giant non-flowering plant with glossy, cone-shaped leaves.";
		entityType.Description = "\n BIOLOGY OVERVIEW\n Daysheen is a massive clump-forming perennial with stiff, glossy leaves that form outward into a conical shape. Their glossy outer-coating may serve as a possible surface for glare reflection.\n \n SURVIVAL GUIDE NOTES\n The sheer size and structure of the daysheen's leaves, as well as their reflective capabilities, are substantial factors when considering shelter construction. ";
		entityType.ThumbnailSmall = "HUD_thumbnail_daysheen";
		entityType.TreeType = new TreeType
		{
			BulkPerSize = 16f,
			MatureAge = 8f,
			MaxAge = 20f,
			MaxFlavours = 6,
			SizeImpact = 2f,
			FibrousPercentageOfTotalMass = 0f,
			LumberPercentageOfFiberMass = 0f,
			Crops = new string[1] { "crop:daysheenLeaves" },
			DefaultCrops = new DefaultCrops[1]
			{
				new DefaultCrops
				{
					KeyName = "crop:daysheenLeaves",
					MinItemsForFullGrownPlant = 1,
					MaxItemsForFullGrownPlant = 2
				}
			}
		};
		entityType.GatheringSiteType = new GatheringSiteType
		{
			arc = new Arc
			{
				Radius = 5f,
				MinAngle = -180.0,
				MaxAngle = 180.0
			},
			SeatSize = 10f,
			MaxVisitors = 4
		};
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "tree_daysheen_1_grown_day"
					}
				}
			},
			ClientStateConditions = new ClientStateInfo[40]
			{
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_daysheen_1_grown_day",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 10)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_daysheen_2_grown_day",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 10)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_daysheen_1_grown_day",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23, 10)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_daysheen_2_grown_day",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 24, 10)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_daysheen_1_grown_day",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 25, 10)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_daysheen_2_grown_day",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 26, 10)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_daysheen_1_grown_day",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 10, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_daysheen_2_grown_day",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 10, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_daysheen_1_grown_day",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23, 10, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_daysheen_2_grown_day",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 24, 10, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_daysheen_1_grown_day",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 25, 10, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_daysheen_2_grown_day",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 26, 10, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_daysheen_1_grown_cut",
							Bendyness = 0.2f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_daysheen_2_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_daysheen_1_grown_cut",
							Bendyness = 0.2f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_daysheen_2_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 24)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_daysheen_1_grown_cut",
							Bendyness = 0.2f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 25)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_daysheen_2_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 26)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_daysheen_1_grown_cut",
							Bendyness = 0.2f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_daysheen_2_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_daysheen_1_grown_cut",
							Bendyness = 0.2f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_daysheen_2_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 24, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_daysheen_1_grown_cut",
							Bendyness = 0.2f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 25, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_daysheen_2_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 26, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_daysheen_1_young_day",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_daysheen_2_young_day",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_daysheen_3_young_day",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_daysheen_4_young_day",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 24, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_daysheen_5_young_day",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 25, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_daysheen_6_young_day",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 26, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_daysheen_1_young_night",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 15, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_daysheen_1_young_night",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 15, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_daysheen_1_young_night",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23, 15, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_daysheen_2_young_night",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 24, 15, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_daysheen_2_young_night",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 25, 15, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_daysheen_2_young_night",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 26, 15, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_daysheendead_1_grown",
							Bendyness = 0.3f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 18)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_daysheendead_1_young",
							Bendyness = 0.3f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 18, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_daysheendead_1_grown",
							Bendyness = 0.3f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 18, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_daysheendead_1_young",
							Bendyness = 0.3f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 18, 15, 14)
				}
			}
		};
		entityType.DefaultSimState = new SimStateInfo
		{
			GeometryLayoutType = new GeometryLayoutType
			{
				Shapes = new CollideShape2D[1]
				{
					new CollideShape2D(Vector2.Zero, 5f)
					{
						Offset = new Vector2(-10f, -20f)
					}
				}
			}
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		bendyness = 0.6f;
		entityType = new EntityType("tree:daysheendead");
		entityType.Name = "Daysheen - dead";
		entityType.SummaryDescription = "Giant non-flowering plant with glossy, cone-shaped leaves.";
		entityType.Description = "\n BIOLOGY OVERVIEW\n Daysheen is a massive clump-forming perennial with stiff, glossy leaves that form outward into a conical shape. Their glossy outer-coating may serve as a possible surface for glare reflection.\n \n SURVIVAL GUIDE NOTES\n No use has been found for the dead daysheen.";
		entityType.TreeType = new TreeType
		{
			BulkPerSize = 16f,
			MatureAge = 8f,
			MaxAge = 20f,
			SizeImpact = 2f,
			FibrousPercentageOfTotalMass = 0f,
			LumberPercentageOfFiberMass = 0f
		};
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "tree_daysheendead_1_grown",
						Bendyness = bendyness
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
							AssetName = "tree_daysheendead_1_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_daysheendead_1_young",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 15)
				}
			}
		};
		entityType.DefaultSimState = new SimStateInfo
		{
			GeometryLayoutType = new GeometryLayoutType
			{
				Shapes = new CollideShape2D[1]
				{
					new CollideShape2D(Vector2.Zero, 5f)
					{
						Offset = new Vector2(-10f, -20f)
					}
				}
			}
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		bendyness = 0.1f;
		entityType = new EntityType("tree:gianthollow");
		entityType.Name = "Giant hollow";
		entityType.SummaryDescription = "Hollow plant formed from large triangular scales";
		entityType.Description = "\n BIOLOGY OVERVIEW\n These massive cavernous plants are native to arid, sunny landscapes. The giant hollows are home to many small animals, which offer the plant protection from predators in exchange for shelter and shade. The plant is able to uptake unusually large amounts of sulfate and thrives in areas with high sulfur concentrations.\n \n SURVIVAL GUIDE NOTES\n The buds of this plant are tough and have the size of a bowl. They could find use as an improvised food container.";
		entityType.ThumbnailSmall = "HUD_thumbnail_greatHollow";
		entityType.TreeType = new TreeType
		{
			BulkPerSize = 40f,
			MatureAge = 22f,
			MaxAge = 400f,
			SizeImpact = 5f,
			FibrousPercentageOfTotalMass = 1f,
			LumberPercentageOfFiberMass = 0f,
			MaxFlavours = 3,
			Crops = new string[2] { "crop:sticks", "crop:giantHollowBud" },
			DefaultCrops = new DefaultCrops[2]
			{
				new DefaultCrops
				{
					KeyName = "crop:sticks",
					MinItemsForFullGrownPlant = 0,
					MaxItemsForFullGrownPlant = 1
				},
				new DefaultCrops
				{
					KeyName = "crop:giantHollowBud",
					MinItemsForFullGrownPlant = 0,
					MaxItemsForFullGrownPlant = 1
				}
			}
		};
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "tree_gianthollow_1_grown",
						Bendyness = bendyness
					}
				}
			},
			ClientStateConditions = new ClientStateInfo[6]
			{
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_gianthollow_1_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_gianthollow_2_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_gianthollow_3_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_gianthollow_1_young",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_gianthollow_1_young",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_gianthollow_3_young",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23, 15)
				}
			}
		};
		entityType.DefaultSimState = new SimStateInfo
		{
			GeometryLayoutType = new GeometryLayoutType
			{
				Shapes = new CollideShape2D[1]
				{
					new CollideShape2D(Vector2.Zero, 10f)
					{
						Offset = new Vector2(-10f, -20f)
					}
				}
			}
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		bendyness = 0.1f;
		entityType = new EntityType("tree:starSnare");
		entityType.Name = "Star snare";
		entityType.SummaryDescription = "Carnivorous plant that attracts flying prey at night by emitting light";
		entityType.Description = "The tentacles crowning the top can lure and ensnare flyers such as diamond birds so they fall into the digestive fluid of the plant's hollow interior where they dissolve.";
		entityType.ThumbnailSmall = "HUD_thumbnail_starsnare";
		entityType.TreeType = new TreeType
		{
			BulkPerSize = 40f,
			MatureAge = 22f,
			MaxAge = 200f,
			SizeImpact = 5f,
			FibrousPercentageOfTotalMass = 1f,
			LumberPercentageOfFiberMass = 0f,
			MaxFlavours = 3
		};
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "tree_starsnare_1_grown_day",
						Bendyness = bendyness
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
							AssetName = "tree_starsnare_1_grown_day",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_starsnare_1_young_day",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_starsnare_1_grown_night",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_starsnare_1_young_night",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 15, 14)
				}
			}
		};
		entityType.DefaultSimState = new SimStateInfo
		{
			GeometryLayoutType = new GeometryLayoutType
			{
				Shapes = new CollideShape2D[1]
				{
					new CollideShape2D(Vector2.Zero, 15f)
					{
						Offset = new Vector2(0f, -8f)
					}
				}
			}
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		bendyness = 0.2f;
		entityType = new EntityType("tree:brambletiny");
		entityType.Name = "Iron bramble - small";
		entityType.SummaryDescription = "Tangled bushes that form a dense barrier.";
		entityType.Description = "\n BIOLOGY OVERVIEW\n Iron bramble is a collection of several small non-fruit-bearing, prickly shrubs that compete amongst one another for resources in temperate environments. The individual species do not have any natural weapons to combat each other leading the shrubs to enact a type of mutualistic relationship where each type of thorn from each different plant protects the whole group from separate dangers. Many small creatures use this variability of protection to their advantage when making nests in the bramble.";
		entityType.ThumbnailSmall = "HUD_thumbnail_bramble";
		entityType.TreeType = new TreeType
		{
			BulkPerSize = 10f,
			MatureAge = 6f,
			MaxAge = 40f,
			SizeImpact = 1f,
			FibrousPercentageOfTotalMass = 0.2f,
			LumberPercentageOfFiberMass = 0f
		};
		entityType.GatheringSiteType = new GatheringSiteType
		{
			arc = new Arc
			{
				Radius = 8f,
				MinAngle = -180.0,
				MaxAngle = 180.0
			},
			SeatSize = 1f,
			MaxVisitors = 2
		};
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "tree_brambletiny_1_grown",
						Bendyness = bendyness
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
							AssetName = "tree_brambletiny_1_young",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 15)
				}
			}
		};
		entityType.DefaultSimState = new SimStateInfo
		{
			GeometryLayoutType = new GeometryLayoutType
			{
				Shapes = new CollideShape2D[1]
				{
					new CollideShape2D(Vector2.Zero, 14f)
					{
						Offset = new Vector2(-1f, -4f)
					}
				}
			}
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		bendyness = 0.2f;
		entityType = new EntityType("tree:bramblesmall");
		entityType.Name = "Iron bramble - larger";
		entityType.SummaryDescription = "Tangled bushes that form a dense barrier.";
		entityType.Description = "\n BIOLOGY OVERVIEW\n Iron bramble is a collection of several small non-fruit-bearing, prickly shrubs that compete amongst one another for resources in temperate environments. The individual species do not have any natural weapons to combat each other leading the shrubs to enact a type of mutualistic relationship where each type of thorn from each different plant protects the whole group from separate dangers. Many small creatures use this variability of protection to their advantage when making nests in the bramble.";
		entityType.TreeType = new TreeType
		{
			BulkPerSize = 10f,
			MatureAge = 6f,
			MaxAge = 40f,
			SizeImpact = 1f,
			FibrousPercentageOfTotalMass = 0.2f,
			LumberPercentageOfFiberMass = 0f
		};
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "tree_bramblesmall_1_grown",
						Bendyness = bendyness
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
							AssetName = "tree_bramblesmall_1_young",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 15)
				}
			}
		};
		entityType.DefaultSimState = new SimStateInfo
		{
			GeometryLayoutType = new GeometryLayoutType
			{
				Shapes = new CollideShape2D[1]
				{
					new CollideShape2D(Vector2.Zero, 19f)
					{
						Offset = new Vector2(0f, -8f)
					}
				}
			}
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		bendyness = 0.2f;
		entityType = new EntityType("tree:bramblemedium");
		entityType.Name = "Iron bramble - big";
		entityType.SummaryDescription = "Tangled bushes that form a dense barrier.";
		entityType.Description = "\n BIOLOGY OVERVIEW\n Iron bramble is a collection of several small non-fruit-bearing, prickly shrubs that compete amongst one another for resources in temperate environments. The individual species do not have any natural weapons to combat each other leading the shrubs to enact a type of mutualistic relationship where each type of thorn from each different plant protects the whole group from separate dangers. Many small creatures use this variability of protection to their advantage when making nests in the bramble.";
		entityType.TreeType = new TreeType
		{
			BulkPerSize = 10f,
			MatureAge = 6f,
			MaxAge = 40f,
			SizeImpact = 1f,
			FibrousPercentageOfTotalMass = 0.2f,
			LumberPercentageOfFiberMass = 0f
		};
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "tree_bramblemedium_1_grown",
						Bendyness = bendyness
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
							AssetName = "tree_bramblemedium_1_young",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 15)
				}
			}
		};
		entityType.DefaultSimState = new SimStateInfo
		{
			GeometryLayoutType = new GeometryLayoutType
			{
				Shapes = new CollideShape2D[1]
				{
					new CollideShape2D(Vector2.Zero, 23f)
					{
						Offset = new Vector2(0f, -16f)
					}
				}
			}
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		bendyness = 1.5f;
		entityType = new EntityType("tree:spoak");
		entityType.Name = "Spoak";
		entityType.SummaryDescription = "Hardwood tree with stiff leaves.";
		entityType.Description = "\n BIOLOGY OVERVIEW\n The wood's properties and thick trunk that spreads into spiraling branches led us to the name 'spiral oak', later condensed to the portmanteau: 'spoak'. Its leaves are large, rigid plates arranged in a pattern that allows for maximal solar energy to be collected, while at the same time letting strong winds pass through the canopy without toppling the tree.\n \n SURVIVAL GUIDE NOTES\n Both leaves and branches are useful in construction.";
		entityType.ThumbnailSmall = "HUD_thumbnail_spoak";
		entityType.TreeType = new TreeType
		{
			BulkPerSize = 10f,
			MatureAge = 6f,
			MaxAge = 40f,
			SizeImpact = 1f,
			MaxFlavours = 5,
			FibrousPercentageOfTotalMass = 1f,
			LumberPercentageOfFiberMass = 0.5f,
			Crops = new string[2] { "crop:spoakBranches", "crop:sticks" },
			DefaultCrops = new DefaultCrops[2]
			{
				new DefaultCrops
				{
					KeyName = "crop:spoakBranches",
					MinItemsForFullGrownPlant = 0,
					MaxItemsForFullGrownPlant = 1
				},
				new DefaultCrops
				{
					KeyName = "crop:sticks",
					MinItemsForFullGrownPlant = 0,
					MaxItemsForFullGrownPlant = 1
				}
			}
		};
		entityType.GatheringSiteType = new GatheringSiteType
		{
			arc = new Arc
			{
				Radius = 5f,
				MinAngle = -180.0,
				MaxAngle = 180.0
			},
			SeatSize = 10f,
			MaxVisitors = 4
		};
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "tree_holdenstree_1_grown_summer_nofruit",
						Bendyness = bendyness
					}
				}
			},
			ClientStateConditions = new ClientStateInfo[25]
			{
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_holdenstree_1_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_holdenstree_2_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_holdenstree_3_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_holdenstree_4_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 24, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_holdenstree_5_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 25, 15)
				},
				new ClientStateInfo
				{
					ParticleEmitters = new ParticleEmitterEffect[1]
					{
						new ParticleEmitterEffect
						{
							ParticleSystemKey = "pollen"
						}
					},
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_holdenstree_1_grown_summer_nofruit",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 10)
				},
				new ClientStateInfo
				{
					ParticleEmitters = new ParticleEmitterEffect[1]
					{
						new ParticleEmitterEffect
						{
							ParticleSystemKey = "pollen"
						}
					},
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_holdenstree_2_grown_summer_nofruit",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 10)
				},
				new ClientStateInfo
				{
					ParticleEmitters = new ParticleEmitterEffect[1]
					{
						new ParticleEmitterEffect
						{
							ParticleSystemKey = "pollen"
						}
					},
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_holdenstree_3_grown_summer_nofruit",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23, 10)
				},
				new ClientStateInfo
				{
					ParticleEmitters = new ParticleEmitterEffect[1]
					{
						new ParticleEmitterEffect
						{
							ParticleSystemKey = "pollen"
						}
					},
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_holdenstree_4_grown_summer_nofruit",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 24, 10)
				},
				new ClientStateInfo
				{
					ParticleEmitters = new ParticleEmitterEffect[1]
					{
						new ParticleEmitterEffect
						{
							ParticleSystemKey = "pollen"
						}
					},
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_holdenstree_5_grown_summer_nofruit",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 25, 10)
				},
				new ClientStateInfo
				{
					ParticleEmitters = new ParticleEmitterEffect[1]
					{
						new ParticleEmitterEffect
						{
							ParticleSystemKey = "pollen"
						}
					},
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_holdenstree_1_grown_summer_nofruit",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 10, 14)
				},
				new ClientStateInfo
				{
					ParticleEmitters = new ParticleEmitterEffect[1]
					{
						new ParticleEmitterEffect
						{
							ParticleSystemKey = "pollen"
						}
					},
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_holdenstree_2_grown_summer_nofruit",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 10, 14)
				},
				new ClientStateInfo
				{
					ParticleEmitters = new ParticleEmitterEffect[1]
					{
						new ParticleEmitterEffect
						{
							ParticleSystemKey = "pollen"
						}
					},
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_holdenstree_3_grown_summer_nofruit",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23, 10, 14)
				},
				new ClientStateInfo
				{
					ParticleEmitters = new ParticleEmitterEffect[1]
					{
						new ParticleEmitterEffect
						{
							ParticleSystemKey = "pollen"
						}
					},
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_holdenstree_4_grown_summer_nofruit",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 24, 10, 14)
				},
				new ClientStateInfo
				{
					ParticleEmitters = new ParticleEmitterEffect[1]
					{
						new ParticleEmitterEffect
						{
							ParticleSystemKey = "pollen"
						}
					},
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_holdenstree_5_grown_summer_nofruit",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 25, 10, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_holdenstree_1_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_holdenstree_2_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_holdenstree_3_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_holdenstree_4_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 24)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_holdenstree_5_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 25)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_holdenstree_1_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_holdenstree_2_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_holdenstree_3_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_holdenstree_4_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 24, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_holdenstree_5_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 25, 14)
				}
			}
		};
		entityType.PointLayoutType = new PointLayoutType();
		item = entityType;
		listOfEntityTypes.Add(item);
		bendyness = 1.5f;
		entityType = new EntityType("tree:spoakdeadleaves");
		entityType.Name = "Spoak - dead leaves";
		entityType.SummaryDescription = "A dying spoak tree.";
		entityType.Description = "\n BIOLOGY OVERVIEW\n The wood's properties and the thick trunk that spreads into spiralling branches made us name this tree 'spiral oak', later contracted to the portmanteau 'spoak'. Its leaves are large, rigid plates that seem to be arranged in a pattern that allows maximum solar energy to be collected while at the same time letting strong winds pass through the canopy without toppling the tree.";
		entityType.TreeType = new TreeType
		{
			BulkPerSize = 10f,
			MatureAge = 6f,
			MaxAge = 40f,
			SizeImpact = 1f,
			FibrousPercentageOfTotalMass = 0.2f,
			LumberPercentageOfFiberMass = 0f,
			Crops = new string[2] { "crop:spoakBranches", "crop:sticks" },
			DefaultCrops = new DefaultCrops[2]
			{
				new DefaultCrops
				{
					KeyName = "crop:spoakBranches",
					MinItemsForFullGrownPlant = 0,
					MaxItemsForFullGrownPlant = 1
				},
				new DefaultCrops
				{
					KeyName = "crop:sticks",
					MinItemsForFullGrownPlant = 0,
					MaxItemsForFullGrownPlant = 1
				}
			}
		};
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "tree_holdenstreedeadleaves_1_grown",
						Bendyness = bendyness
					}
				}
			}
		};
		entityType.PointLayoutType = new PointLayoutType();
		item = entityType;
		listOfEntityTypes.Add(item);
		bendyness = 0.2f;
		entityType = new EntityType("tree:spoakdeadnaked");
		entityType.Name = "Spoak - dead, naked";
		entityType.SummaryDescription = "A dead spoak tree.";
		entityType.Description = "\n BIOLOGY OVERVIEW\n The wood's properties and the thick trunk that spreads into spiralling branches made us name this tree 'spiral oak', later contracted to the portmanteau 'spoak'.";
		entityType.TreeType = new TreeType
		{
			BulkPerSize = 10f,
			MatureAge = 6f,
			MaxAge = 40f,
			SizeImpact = 1f,
			FibrousPercentageOfTotalMass = 0.2f,
			LumberPercentageOfFiberMass = 0f,
			Crops = new string[2] { "crop:spoakBranches", "crop:sticks" },
			DefaultCrops = new DefaultCrops[1]
			{
				new DefaultCrops
				{
					KeyName = "crop:sticks",
					MinItemsForFullGrownPlant = 0,
					MaxItemsForFullGrownPlant = 1
				}
			}
		};
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "tree_holdenstreedeadnaked_1_grown",
						Bendyness = bendyness
					}
				}
			}
		};
		entityType.PointLayoutType = new PointLayoutType();
		item = entityType;
		listOfEntityTypes.Add(item);
		bendyness = 0.4f;
		entityType = new EntityType("tree:spoakdeadvines");
		entityType.Name = "Spoak - dead, overgrown";
		entityType.SummaryDescription = "A dead spoak tree, overgrown with weeds.";
		entityType.Description = "\n BIOLOGY OVERVIEW\n The wood's properties and the thick trunk that spreads into spiralling branches made us name this tree 'spiral oak', later contracted to the portmanteau 'spoak'.";
		entityType.TreeType = new TreeType
		{
			BulkPerSize = 10f,
			MatureAge = 6f,
			MaxAge = 40f,
			SizeImpact = 1f,
			FibrousPercentageOfTotalMass = 0.2f,
			LumberPercentageOfFiberMass = 0f,
			Crops = new string[2] { "crop:spoakBranches", "crop:sticks" },
			DefaultCrops = new DefaultCrops[2]
			{
				new DefaultCrops
				{
					KeyName = "crop:spoakBranches",
					MinItemsForFullGrownPlant = 0,
					MaxItemsForFullGrownPlant = 1
				},
				new DefaultCrops
				{
					KeyName = "crop:sticks",
					MinItemsForFullGrownPlant = 0,
					MaxItemsForFullGrownPlant = 1
				}
			}
		};
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "tree_holdenstreedeadvines_1_grown",
						Bendyness = bendyness
					}
				}
			}
		};
		entityType.PointLayoutType = new PointLayoutType();
		item = entityType;
		listOfEntityTypes.Add(item);
		bendyness = 0.5f;
		listOfEntityTypes.Add(new EntityType("tree:copperfern")
		{
			Name = "Copperfern",
			SummaryDescription = "A bush often found on sandy soil",
			Description = "\n BIOLOGY OVERVIEW\n Though the copperfern's leaves take on the appearance of those of a fern, the plant more closely resembles a shrub. Microscopic aphid-like insects inhabit the bush, releasing a sweet, irresistible substance similar to honeydew. This substance attracts various other insects for feeding and nesting, including the pig fly.",
			ThumbnailSmall = "HUD_thumbnail_coppperfern",
			TreeType = new TreeType
			{
				BulkPerSize = 10f,
				MatureAge = 6f,
				MaxAge = 40f,
				SizeImpact = 1f,
				FibrousPercentageOfTotalMass = 0.2f,
				LumberPercentageOfFiberMass = 0f,
				MaxFlavours = 3,
				Crops = new string[1] { "crop:pigFlies" },
				DefaultCrops = new DefaultCrops[1]
				{
					new DefaultCrops
					{
						KeyName = "crop:pigFlies",
						MinItemsForFullGrownPlant = 0,
						MaxItemsForFullGrownPlant = 2
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
							AssetName = "tree_copperfern_1_grown_summer",
							Bendyness = bendyness
						}
					}
				},
				ClientStateConditions = new ClientStateInfo[9]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "tree_copperfern_1_grown_summer",
								Bendyness = bendyness
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 21)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "tree_copperfern_2_grown_summer",
								Bendyness = bendyness
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 22)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "tree_copperfern_3_grown_summer",
								Bendyness = bendyness
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 23)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "tree_copperfern_1_young_summer",
								Bendyness = bendyness
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 21, 15)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "tree_copperfern_2_young_summer",
								Bendyness = bendyness
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 22, 15)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "tree_copperfern_2_young_summer",
								Bendyness = bendyness
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 23, 15)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "tree_copperferndead_1_grown",
								Bendyness = bendyness
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 21, 18)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "tree_copperferndead_1_grown",
								Bendyness = bendyness
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 22, 18)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "tree_copperferndead_1_grown",
								Bendyness = bendyness
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 23, 18)
					}
				}
			},
			PointLayoutType = new PointLayoutType()
		});
		bendyness = 0.4f;
		listOfEntityTypes.Add(new EntityType("tree:copperferndead")
		{
			Name = "Copperfern - dead",
			SummaryDescription = "A bush often found on sandy soil",
			Description = "\n BIOLOGY OVERVIEW\n Though the copperfern's leaves take on the appearance of those of a fern, the plant more closely resembles a shrub. Microscopic aphid-like insects inhabit the bush, releasing a sweet, irresistible substance similar to honeydew. This substance attracts various other insects for feeding and nesting, including the pig fly.",
			TreeType = new TreeType
			{
				BulkPerSize = 10f,
				MatureAge = 6f,
				MaxAge = 40f,
				SizeImpact = 1f,
				FibrousPercentageOfTotalMass = 0.2f,
				LumberPercentageOfFiberMass = 0f
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_copperferndead_1_grown",
							Bendyness = bendyness
						}
					}
				}
			},
			PointLayoutType = new PointLayoutType()
		});
		bendyness = 0.5f;
		listOfEntityTypes.Add(new EntityType("tree:palecopperfern")
		{
			Name = "Pale copperfern",
			SummaryDescription = "A bush often found on sandy soil",
			Description = "\n BIOLOGY OVERVIEW\n Though the copperfern's leaves take on the appearance of those of a fern, the plant more closely resembles a shrub. Microscopic aphid-like insects inhabit the bush, releasing a sweet, irresistible substance similar to honeydew. This substance attracts various other insects for feeding and nesting, including the pig fly.",
			ThumbnailSmall = "HUD_thumbnail_coppperfern",
			TreeType = new TreeType
			{
				BulkPerSize = 10f,
				MatureAge = 6f,
				MaxAge = 40f,
				SizeImpact = 1f,
				FibrousPercentageOfTotalMass = 0.2f,
				LumberPercentageOfFiberMass = 0f,
				MaxFlavours = 3,
				Crops = new string[1] { "crop:pigFlies" },
				DefaultCrops = new DefaultCrops[1]
				{
					new DefaultCrops
					{
						KeyName = "crop:pigFlies",
						MinItemsForFullGrownPlant = 0,
						MaxItemsForFullGrownPlant = 2
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
							AssetName = "tree_palecopperfern_1_grown_summer",
							Bendyness = bendyness
						}
					}
				},
				ClientStateConditions = new ClientStateInfo[9]
				{
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "tree_palecopperfern_1_grown_summer",
								Bendyness = bendyness
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 21)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "tree_palecopperfern_2_grown_summer",
								Bendyness = bendyness
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 22)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "tree_palecopperfern_3_grown_summer",
								Bendyness = bendyness
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 23)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "tree_palecopperfern_1_young_summer",
								Bendyness = bendyness
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 21, 15)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "tree_palecopperfern_2_young_summer",
								Bendyness = bendyness
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 22, 15)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "tree_palecopperfern_2_young_summer",
								Bendyness = bendyness
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 23, 15)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "tree_palecopperferndead_1_grown",
								Bendyness = bendyness
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 21, 18)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "tree_palecopperferndead_1_grown",
								Bendyness = bendyness
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 22, 18)
					},
					new ClientStateInfo
					{
						RenderAsBillboardType = new RenderAsBillboardType[1]
						{
							new RenderAsBillboardType
							{
								AssetName = "tree_palecopperferndead_1_grown",
								Bendyness = bendyness
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 23, 18)
					}
				}
			},
			PointLayoutType = new PointLayoutType()
		});
		bendyness = 0.4f;
		listOfEntityTypes.Add(new EntityType("tree:palecopperferndead")
		{
			Name = "Pale copperfern - dead",
			SummaryDescription = "A bush often found on sandy soil",
			Description = "\n BIOLOGY OVERVIEW\n Though the copperfern's leaves take on the appearance of those of a fern, the plant more closely resembles a shrub. Microscopic aphid-like insects inhabit the bush, releasing a sweet, irresistible substance similar to honeydew. This substance attracts various other insects for feeding and nesting, including the pig fly.",
			TreeType = new TreeType
			{
				BulkPerSize = 10f,
				MatureAge = 6f,
				MaxAge = 40f,
				SizeImpact = 1f,
				FibrousPercentageOfTotalMass = 0.2f,
				LumberPercentageOfFiberMass = 0f
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palecopperferndead_1_grown",
							Bendyness = bendyness
						}
					}
				}
			},
			PointLayoutType = new PointLayoutType()
		});
		bendyness = 0.2f;
		entityType = new EntityType("tree:rhubarb");
		entityType.Name = "Clearleaf tree";
		entityType.SummaryDescription = "Odd plant whose leaves are shaped as transparent disks";
		entityType.Description = "These knobby plant structures grow on top of muckroot - the grey/blue mossy carpet found in certain areas. Like other plants which grow on muckroot, the clearleaf is in symbiosis with the muckroot which provides it with nutrients.";
		entityType.ThumbnailSmall = "HUD_thumbnail_rhubarb";
		entityType.TreeType = new TreeType
		{
			BulkPerSize = 10f,
			MatureAge = 6f,
			MaxAge = 40f,
			SizeImpact = 1f,
			MaxFlavours = 3,
			FibrousPercentageOfTotalMass = 0.2f,
			LumberPercentageOfFiberMass = 0f
		};
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "tree_rhubarb_1_grown_summer",
						Bendyness = bendyness
					}
				}
			},
			ClientStateConditions = new ClientStateInfo[6]
			{
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_rhubarb_1_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_rhubarb_2_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_rhubarb_3_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_rhubarb_1_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_rhubarb_2_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_rhubarb_3_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23, 15)
				}
			}
		};
		entityType.PointLayoutType = new PointLayoutType();
		item = entityType;
		listOfEntityTypes.Add(item);
		bendyness = 0.2f;
		entityType = new EntityType("tree:desertrhubarb");
		entityType.Name = "Desert clearleaf tree";
		entityType.SummaryDescription = "Odd plant whose leaves are shaped as transparent disks";
		entityType.Description = "This variant of the clearleaf tree has adapted to grow in arid environments by evolving an ability to conserve water. There is still much research to be done before we understand the details.";
		entityType.ThumbnailSmall = "HUD_thumbnail_rhubarb";
		entityType.TreeType = new TreeType
		{
			BulkPerSize = 10f,
			MatureAge = 6f,
			MaxAge = 40f,
			SizeImpact = 1f,
			MaxFlavours = 3,
			FibrousPercentageOfTotalMass = 0.2f,
			LumberPercentageOfFiberMass = 0f
		};
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "tree_desertrhubarb_1_grown_summer",
						Bendyness = bendyness
					}
				}
			},
			ClientStateConditions = new ClientStateInfo[6]
			{
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_desertrhubarb_1_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_desertrhubarb_2_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_desertrhubarb_3_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_desertrhubarb_1_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_desertrhubarb_2_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_desertrhubarb_3_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23, 15)
				}
			}
		};
		entityType.PointLayoutType = new PointLayoutType();
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("tree:sanctuarytree");
		entityType.Name = "Sanctuary tree";
		entityType.SummaryDescription = "Colossal tree that grows in the firegrass biome";
		entityType.Description = description;
		entityType.ThumbnailSmall = "HUD_thumbnail_sanctuarytree";
		entityType.TreeType = new TreeType
		{
			BulkPerSize = 10f,
			MatureAge = 6f,
			MaxAge = 40f,
			SizeImpact = 1f,
			HasSummerWinterCycle = true,
			FibrousPercentageOfTotalMass = 0.2f,
			LumberPercentageOfFiberMass = 0f,
			Crops = new string[1] { "crop:sticks" },
			DefaultCrops = new DefaultCrops[1]
			{
				new DefaultCrops
				{
					KeyName = "crop:sticks",
					MinItemsForFullGrownPlant = 1,
					MaxItemsForFullGrownPlant = 2
				}
			}
		};
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "tree_sanctuarytree_1_grown_summer"
					}
				}
			},
			ClientStateConditions = new ClientStateInfo[6]
			{
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_sanctuarytree_1_grown_summer"
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_sanctuarytree_1_young_summer"
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_sanctuarytree_1_grown_winter"
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 13)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_sanctuarytree_1_young_winter"
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 15, 13)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_sanctuarytreedead_1_grown"
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 18)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_sanctuarytreedeadvines_1_grown"
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 18, 8)
				}
			}
		};
		entityType.DefaultSimState = new SimStateInfo
		{
			GeometryLayoutType = new GeometryLayoutType
			{
				Shapes = new CollideShape2D[1]
				{
					new CollideShape2D(Vector2.Zero, 45f)
					{
						Offset = new Vector2(-10f, -20f)
					}
				}
			}
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("tree:sanctuarytreedead");
		entityType.Name = "Sanctuary tree - dead";
		entityType.SummaryDescription = "The remnants of a colossal tree that inhabits the firegrass biome";
		entityType.Description = description2;
		entityType.ThumbnailSmall = "HUD_thumbnail_placeholder";
		entityType.IsSelectable = true;
		entityType.UsesMemory = true;
		entityType.ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always;
		entityType.SharedSpecialActions = new Pair<string, bool>[1]
		{
			new Pair<string, bool>("buildFavorbreadFarm", second: true)
		};
		entityType.TreeType = new TreeType
		{
			BulkPerSize = 10f,
			MatureAge = 6f,
			MaxAge = 40f,
			SizeImpact = 1f,
			FibrousPercentageOfTotalMass = 0.2f,
			LumberPercentageOfFiberMass = 0f,
			Crops = new string[1] { "crop:sticks" },
			DefaultCrops = new DefaultCrops[1]
			{
				new DefaultCrops
				{
					KeyName = "crop:sticks",
					MinItemsForFullGrownPlant = 1,
					MaxItemsForFullGrownPlant = 2
				}
			}
		};
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "tree_sanctuarytreedead_1_grown"
					}
				}
			}
		};
		entityType.DefaultSimState = new SimStateInfo
		{
			GeometryLayoutType = new GeometryLayoutType
			{
				Shapes = new CollideShape2D[1]
				{
					new CollideShape2D(Vector2.Zero, 45f)
					{
						Offset = new Vector2(-10f, -20f)
					}
				}
			}
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("tree:sanctuarytreedeadvines");
		entityType.Name = "Sanctuary tree - dead";
		entityType.SummaryDescription = "The remnants of a colossal tree that inhabits the firegrass biome";
		entityType.Description = description2;
		entityType.ThumbnailSmall = "HUD_thumbnail_placeholder";
		entityType.IsSelectable = true;
		entityType.UsesMemory = true;
		entityType.ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always;
		entityType.SharedSpecialActions = new Pair<string, bool>[1]
		{
			new Pair<string, bool>("buildFavorbreadFarm", second: true)
		};
		entityType.TreeType = new TreeType
		{
			BulkPerSize = 10f,
			MatureAge = 6f,
			MaxAge = 40f,
			SizeImpact = 1f,
			FibrousPercentageOfTotalMass = 0.2f,
			LumberPercentageOfFiberMass = 0f,
			Crops = new string[1] { "crop:sticks" },
			DefaultCrops = new DefaultCrops[1]
			{
				new DefaultCrops
				{
					KeyName = "crop:sticks",
					MinItemsForFullGrownPlant = 1,
					MaxItemsForFullGrownPlant = 2
				}
			}
		};
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "tree_sanctuarytreedeadvines_1_grown"
					}
				}
			}
		};
		entityType.DefaultSimState = new SimStateInfo
		{
			GeometryLayoutType = new GeometryLayoutType
			{
				Shapes = new CollideShape2D[1]
				{
					new CollideShape2D(Vector2.Zero, 45f)
					{
						Offset = new Vector2(-10f, -20f)
					}
				}
			}
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("tree:greentub");
		entityType.Name = "Greentub";
		entityType.SummaryDescription = "Cylindrical, spungy growths that thrive on muckroot";
		entityType.Description = "These barrel shaped plants live on top of 'muckroot' - the blue crust of moss which covers rivers and streams. Like all plants that live there, the greentubs are symbiotes - they do not have roots and muckroot provides them with nutrients.";
		entityType.ThumbnailSmall = "HUD_thumbnail_greentub";
		entityType.TreeType = new TreeType
		{
			BulkPerSize = 10f,
			MatureAge = 6f,
			MaxAge = 40f,
			SizeImpact = 1f,
			MaxFlavours = 3,
			FibrousPercentageOfTotalMass = 0.2f,
			LumberPercentageOfFiberMass = 0f
		};
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "tree_greentub_1_grown_summer"
					}
				}
			},
			ClientStateConditions = new ClientStateInfo[6]
			{
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_greentub_1_grown_summer"
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_greentub_2_grown_summer"
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_greentub_3_grown_summer"
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_greentub_1_young_summer"
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_greentub_2_young_summer"
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_greentub_3_young_summer"
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23, 15)
				}
			}
		};
		entityType.PointLayoutType = new PointLayoutType();
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("tree:goblinsprouts");
		entityType.Name = "Goblin sprouts";
		entityType.SummaryDescription = "Spherical, spungy growth. Ubiquitous on muckroot";
		entityType.Description = "These are among the various plants that live on top of 'muckroot' - the blue crust of moss which covers rivers and streams. Like all plants that live there, the goblin sprouts are symbiotes - they do not have roots and muckroot provides them with nutrients.";
		entityType.ThumbnailSmall = "HUD_thumbnail_goblinsprouts";
		entityType.TreeType = new TreeType
		{
			BulkPerSize = 10f,
			MatureAge = 6f,
			MaxAge = 40f,
			SizeImpact = 1f,
			MaxFlavours = 2,
			FibrousPercentageOfTotalMass = 0.2f,
			LumberPercentageOfFiberMass = 0f
		};
		entityType.GatheringSiteType = new GatheringSiteType
		{
			arc = new Arc
			{
				Radius = 5f,
				MinAngle = -180.0,
				MaxAngle = 180.0
			},
			SeatSize = 5f,
			MaxVisitors = 2
		};
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "tree_goblinsprouts_1_grown_summer"
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
							AssetName = "tree_goblinsprouts_1_grown_summer"
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_goblinsprouts_2_grown_summer"
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_goblinsprouts_1_young_summer"
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_goblinsprouts_2_young_summer"
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 15)
				}
			}
		};
		entityType.PointLayoutType = new PointLayoutType();
		item = entityType;
		listOfEntityTypes.Add(item);
		bendyness = 0.1f;
		entityType = new EntityType("tree:marshcotflower");
		entityType.Name = "Marshcot flower";
		entityType.SummaryDescription = "Giant flower with petals that resemble satin pillows";
		entityType.Description = "\n NOTES\n We have found that the sap of the marshcot can be used for making rubber.\n \n BIOLOGY OVERVIEW\n The flower of the marshcot can reach the size of a car. The marshcot grows in brackish wetlands and swamps where its thick tangled stems can cover large areas above and under water.";
		entityType.ThumbnailSmall = "HUD_thumbnail_marshcot";
		entityType.TreeType = new TreeType
		{
			BulkPerSize = 10f,
			MatureAge = 6f,
			MaxAge = 40f,
			SizeImpact = 1f,
			MaxFlavours = 2,
			FibrousPercentageOfTotalMass = 0.2f,
			LumberPercentageOfFiberMass = 0f,
			Crops = new string[1] { "crop:marshcotSap" },
			DefaultCrops = new DefaultCrops[1]
			{
				new DefaultCrops
				{
					KeyName = "crop:marshcotSap",
					MinItemsForFullGrownPlant = 1,
					MaxItemsForFullGrownPlant = 2
				}
			}
		};
		entityType.GatheringSiteType = new GatheringSiteType
		{
			arc = new Arc
			{
				Radius = 5f,
				MinAngle = -180.0,
				MaxAngle = 180.0
			},
			SeatSize = 5f,
			MaxVisitors = 2
		};
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "tree_marshcotflower_1_grown",
						Bendyness = bendyness
					}
				}
			},
			ClientStateConditions = new ClientStateInfo[6]
			{
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_marshcotflower_1_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_marshcotflower_2_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_marshcotflower_1_young",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_marshcotflower_1_young",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_marshcotflowerdead_1_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 18)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_marshcotflowerdead_1_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 18)
				}
			}
		};
		entityType.PointLayoutType = new PointLayoutType();
		item = entityType;
		listOfEntityTypes.Add(item);
		bendyness = 0.1f;
		entityType = new EntityType("tree:marshcotflowerdead");
		entityType.Name = "Marshcot flower - dead";
		entityType.SummaryDescription = "The flower of a dead marshcot plant";
		entityType.Description = "\n NOTES\n We have found that the sap of the marshcot can be used for making rubber.\n \n BIOLOGY OVERVIEW\n The flower of the marshcot can reach the size of a car. The marshcot grows in brackish wetlands and swamps where its thick tangled stems can cover large areas above and under water.";
		entityType.TreeType = new TreeType
		{
			BulkPerSize = 10f,
			MatureAge = 6f,
			MaxAge = 40f,
			SizeImpact = 1f,
			FibrousPercentageOfTotalMass = 0.2f,
			LumberPercentageOfFiberMass = 0f
		};
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "tree_marshcotflowerdead_1_grown",
						Bendyness = bendyness
					}
				}
			}
		};
		entityType.PointLayoutType = new PointLayoutType();
		item = entityType;
		listOfEntityTypes.Add(item);
		bendyness = 0.4f;
		entityType = new EntityType("tree:marshcotleaf");
		entityType.Name = "Marshcot leaf";
		entityType.SummaryDescription = "Large, stiff leaf on the marshcot plant";
		entityType.Description = "\n NOTES\n We have found that the sap of the marshcot can be used for making rubber.\n \n BIOLOGY OVERVIEW\n The marshcot grows in brackish wetlands and swamps where its thick tangled stems can cover large areas above and under water.";
		entityType.ThumbnailSmall = "HUD_thumbnail_marshcot";
		entityType.TreeType = new TreeType
		{
			BulkPerSize = 10f,
			MatureAge = 6f,
			MaxAge = 40f,
			MaxFlavours = 3,
			SizeImpact = 1f,
			FibrousPercentageOfTotalMass = 0.2f,
			LumberPercentageOfFiberMass = 0f,
			Crops = new string[1] { "crop:marshcotSap" },
			DefaultCrops = new DefaultCrops[1]
			{
				new DefaultCrops
				{
					KeyName = "crop:marshcotSap",
					MinItemsForFullGrownPlant = 1,
					MaxItemsForFullGrownPlant = 1
				}
			}
		};
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "tree_marshcotleaf_1_grown",
						Bendyness = bendyness
					}
				}
			},
			ClientStateConditions = new ClientStateInfo[5]
			{
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_marshcotleaf_1_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_marshcotleaf_2_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_marshcotleaf_3_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_marshcotleafdead_1_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 18)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_marshcotleafdead_2_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 18)
				}
			}
		};
		entityType.PointLayoutType = new PointLayoutType();
		item = entityType;
		listOfEntityTypes.Add(item);
		bendyness = 0.3f;
		entityType = new EntityType("tree:marshcotleafdead");
		entityType.Name = "Marshcot leaf - dead";
		entityType.SummaryDescription = "Leaf on a dead marshcot plant";
		entityType.Description = "\n NOTES\n We have found that the sap of the marshcot can be used for making rubber.\n \n BIOLOGY OVERVIEW\n The marshcot grows in brackish wetlands and swamps where its thick tangled stems can cover large areas above and under water.";
		entityType.ThumbnailSmall = "HUD_thumbnail_marshcot";
		entityType.TreeType = new TreeType
		{
			BulkPerSize = 10f,
			MatureAge = 6f,
			MaxAge = 40f,
			MaxFlavours = 2,
			SizeImpact = 1f,
			FibrousPercentageOfTotalMass = 0.2f,
			LumberPercentageOfFiberMass = 0f
		};
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "tree_marshcotleafdead_1_grown",
						Bendyness = bendyness
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
							AssetName = "tree_marshcotleafdead_1_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_marshcotleafdead_2_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22)
				}
			}
		};
		entityType.PointLayoutType = new PointLayoutType();
		item = entityType;
		listOfEntityTypes.Add(item);
		bendyness = 0.4f;
		entityType = new EntityType("tree:thistle");
		entityType.Name = "Thistle tree";
		entityType.ThumbnailSmall = "HUD_thumbnail_thistle";
		entityType.TreeType = new TreeType
		{
			BulkPerSize = 10f,
			MatureAge = 6f,
			MaxAge = 40f,
			SizeImpact = 1f,
			FibrousPercentageOfTotalMass = 0.2f,
			LumberPercentageOfFiberMass = 0f
		};
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "tree_thistle_1_grown",
						Bendyness = bendyness
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
							AssetName = "tree_thistle_1_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_thistle_1_young",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_thistledead_1_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 18)
				}
			}
		};
		entityType.PointLayoutType = new PointLayoutType();
		item = entityType;
		listOfEntityTypes.Add(item);
		bendyness = 0.4f;
		entityType = new EntityType("tree:thistledead");
		entityType.Name = "Thistle tree - dead";
		entityType.ThumbnailSmall = "HUD_thumbnail_thistle";
		entityType.TreeType = new TreeType
		{
			BulkPerSize = 10f,
			MatureAge = 6f,
			MaxAge = 40f,
			SizeImpact = 1f,
			FibrousPercentageOfTotalMass = 0.2f,
			LumberPercentageOfFiberMass = 0f
		};
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "tree_thistledead_1_grown",
						Bendyness = bendyness
					}
				}
			}
		};
		entityType.PointLayoutType = new PointLayoutType();
		item = entityType;
		listOfEntityTypes.Add(item);
		bendyness = 0.7f;
		entityType = new EntityType("tree:candystalk");
		entityType.Name = "Geo stalk";
		entityType.SummaryDescription = "Strange, crystalline plant life that grows from rock crevices";
		entityType.Description = "Based on its habitat, it seems that its crystalline structure is grown from an, as yet, unknown biological composition, derived from geode crystals which are high in minerals.";
		entityType.ThumbnailSmall = "HUD_thumbnail_candystalk";
		entityType.TreeType = new TreeType
		{
			BulkPerSize = 10f,
			MatureAge = 6f,
			MaxAge = 40f,
			SizeImpact = 1f,
			FibrousPercentageOfTotalMass = 0.2f,
			LumberPercentageOfFiberMass = 0f
		};
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "tree_candystalk_1_grown_summer",
						Bendyness = bendyness
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
							AssetName = "tree_candystalk_1_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_candystalk_1_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 15)
				}
			}
		};
		entityType.PointLayoutType = new PointLayoutType();
		item = entityType;
		listOfEntityTypes.Add(item);
		bendyness = 0.5f;
		entityType = new EntityType("tree:wingweed");
		entityType.Name = "Wingweed";
		entityType.SummaryDescription = "Herbaceous plant with large, soft leaves. Common on firegrass";
		entityType.Description = "\n BIOLOGY OVERVIEW\n Wingweed often grows in temperate, drier climes, which makes its presence amongst firegrass almost a constant; however the plant has an excellent plasticity and can be found in almost any habitat. Due to their proximity to the ground, the species generally have larger leaves with which to absorb sunlight.\n \n SURVIVAL GUIDE NOTES\n The leaves can serve as a temporary insulation as they are quite proficient at retaining heat.";
		entityType.ThumbnailSmall = "HUD_thumbnail_wingweed";
		entityType.TreeType = new TreeType
		{
			BulkPerSize = 1f,
			MatureAge = 5f,
			MaxAge = 10f,
			SizeImpact = 0.6f,
			MaxFlavours = 7,
			FibrousPercentageOfTotalMass = 0f,
			LumberPercentageOfFiberMass = 0f,
			Crops = new string[1] { "crop:wingweedLeaves" },
			DefaultCrops = new DefaultCrops[1]
			{
				new DefaultCrops
				{
					KeyName = "crop:wingweedLeaves",
					MinItemsForFullGrownPlant = 0,
					MaxItemsForFullGrownPlant = 1
				}
			}
		};
		entityType.GatheringSiteType = new GatheringSiteType
		{
			arc = new Arc
			{
				Radius = 3f,
				MinAngle = -180.0,
				MaxAngle = 180.0
			},
			SeatSize = 5f,
			MaxVisitors = 2
		};
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "tree_wingweed_1_grown_summer",
						Bendyness = bendyness
					}
				}
			},
			ClientStateConditions = new ClientStateInfo[56]
			{
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweed_1_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 10)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweed_2_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 10)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweed_3_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23, 10)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweed_4_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 24, 10)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweed_5_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 25, 10)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweed_6_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 26, 10)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweed_7_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 27, 10)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweed_1_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 10, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweed_2_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 10, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweed_3_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23, 10, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweed_4_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 24, 10, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweed_5_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 25, 10, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweed_6_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 26, 10, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweed_7_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 27, 10, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweed_1_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweed_2_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweed_3_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweed_4_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 24)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweed_5_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 25)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweed_6_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 26)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweed_7_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 27)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweed_1_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweed_2_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweed_3_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweed_4_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 24, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweed_5_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 25, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweed_6_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 26, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweed_7_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 27, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweed_1_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweed_2_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweed_3_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweed_4_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 24, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweed_5_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 25, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweed_6_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 26, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweed_7_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 27, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweed_1_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 15, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweed_2_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 15, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweed_3_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23, 15, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweed_4_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 24, 15, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweed_5_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 25, 15, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweed_6_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 26, 15, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweed_7_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 27, 15, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweeddead_1_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 18)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweeddead_1_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 18)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweeddead_1_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23, 18)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweeddead_1_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 24, 18)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweeddead_2_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 25, 18)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweeddead_2_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 26, 18)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweeddead_2_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 27, 18)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweeddead_1_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 18, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweeddead_1_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 18, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweeddead_1_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23, 18, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweeddead_1_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 24, 18, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweeddead_2_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 25, 18, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweeddead_2_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 26, 18, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_wingweeddead_2_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 27, 18, 14)
				}
			}
		};
		entityType.PointLayoutType = new PointLayoutType
		{
			IsBlocking = false
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		bendyness = 0.5f;
		entityType = new EntityType("tree:desertWingweed");
		entityType.Name = "Desert wingweed";
		entityType.SummaryDescription = "Herbaceous plant with large, soft leaves. This variant is common on dry soil";
		entityType.Description = "\n BIOLOGY OVERVIEW\n Wingweed often grows in temperate, drier climes, which makes its presence amongst firegrass almost a constant; however the plant has an excellent plasticity and the desert variant can be found on very dry soil. Due to their proximity to the ground, the species generally have larger leaves with which to absorb sunlight.\n \n SURVIVAL GUIDE NOTES\n The leaves can serve as a temporary insulation as they are quite proficient at retaining heat.";
		entityType.ThumbnailSmall = "HUD_thumbnail_wingweed";
		entityType.TreeType = new TreeType
		{
			BulkPerSize = 1f,
			MatureAge = 5f,
			MaxAge = 10f,
			SizeImpact = 0.6f,
			MaxFlavours = 7,
			FibrousPercentageOfTotalMass = 0f,
			LumberPercentageOfFiberMass = 0f,
			Crops = new string[1] { "crop:wingweedLeaves" },
			DefaultCrops = new DefaultCrops[1]
			{
				new DefaultCrops
				{
					KeyName = "crop:wingweedLeaves",
					MinItemsForFullGrownPlant = 0,
					MaxItemsForFullGrownPlant = 1
				}
			}
		};
		entityType.GatheringSiteType = new GatheringSiteType
		{
			arc = new Arc
			{
				Radius = 3f,
				MinAngle = -180.0,
				MaxAngle = 180.0
			},
			SeatSize = 5f,
			MaxVisitors = 2
		};
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "tree_palewingweed_1_grown_summer",
						Bendyness = bendyness
					}
				}
			},
			ClientStateConditions = new ClientStateInfo[56]
			{
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweed_1_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 10)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweed_2_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 10)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweed_3_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23, 10)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweed_4_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 24, 10)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweed_5_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 25, 10)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweed_6_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 26, 10)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweed_7_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 27, 10)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweed_1_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 10, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweed_2_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 10, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweed_3_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23, 10, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweed_4_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 24, 10, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweed_5_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 25, 10, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweed_6_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 26, 10, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweed_7_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 27, 10, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweed_1_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweed_2_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweed_3_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweed_4_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 24)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweed_5_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 25)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweed_6_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 26)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweed_7_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 27)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweed_1_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweed_2_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweed_3_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweed_4_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 24, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweed_5_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 25, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweed_6_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 26, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweed_7_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 27, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweed_1_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweed_2_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweed_3_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweed_4_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 24, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweed_5_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 25, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweed_6_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 26, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweed_7_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 27, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweed_1_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 15, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweed_2_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 15, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweed_3_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23, 15, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweed_4_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 24, 15, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweed_5_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 25, 15, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweed_6_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 26, 15, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweed_7_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 27, 15, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweeddead_1_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 18)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweeddead_1_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 18)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweeddead_1_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23, 18)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweeddead_1_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 24, 18)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweeddead_2_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 25, 18)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweeddead_2_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 26, 18)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweeddead_2_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 27, 18)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweeddead_1_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 18, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweeddead_1_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 22, 18, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweeddead_1_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 23, 18, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweeddead_1_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 24, 18, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweeddead_2_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 25, 18, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweeddead_2_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 26, 18, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_palewingweeddead_2_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 27, 18, 14)
				}
			}
		};
		entityType.PointLayoutType = new PointLayoutType
		{
			IsBlocking = false
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		bendyness = 0.5f;
		entityType = new EntityType("tree:wingweeddead");
		entityType.Name = "Wingweed - dead";
		entityType.SummaryDescription = "Herbaceous plant with large, soft leaves. Common on firegrass";
		entityType.Description = "\n BIOLOGY OVERVIEW\n Wingweed often grows in temperate, drier climes, which makes its presence amongst firegrass almost a constant; however the plant has an excellent plasticity and can be found in almost any habitat. Due to their proximity to the ground, the species generally have larger leaves with which to absorb sunlight.\n \n SURVIVAL GUIDE NOTES\n The leaves can serve as a temporary insulation as they are quite proficient at retaining heat.";
		entityType.ThumbnailSmall = "HUD_thumbnail_wingweed";
		entityType.TreeType = new TreeType
		{
			BulkPerSize = 1f,
			MatureAge = 1f,
			MaxAge = 10f,
			SizeImpact = 0.6f,
			FibrousPercentageOfTotalMass = 0f,
			LumberPercentageOfFiberMass = 0f
		};
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "tree_wingweeddead_1_grown",
						Bendyness = bendyness
					}
				}
			}
		};
		entityType.PointLayoutType = new PointLayoutType();
		item = entityType;
		listOfEntityTypes.Add(item);
		bendyness = 0.5f;
		entityType = new EntityType("tree:desertwingweeddead");
		entityType.Name = "Desert wingweed - dead";
		entityType.SummaryDescription = "Herbaceous plant with large, soft leaves. This variant is common on dry soil";
		entityType.Description = "\n BIOLOGY OVERVIEW\n Wingweed often grows in temperate, drier climes, which makes its presence amongst firegrass almost a constant; however the plant has an excellent plasticity and the desert variant can be found on very dry soil. Due to their proximity to the ground, the species generally have larger leaves with which to absorb sunlight.\n \n SURVIVAL GUIDE NOTES\n The leaves can serve as a temporary insulation as they are quite proficient at retaining heat.";
		entityType.ThumbnailSmall = "HUD_thumbnail_wingweed";
		entityType.TreeType = new TreeType
		{
			BulkPerSize = 1f,
			MatureAge = 1f,
			MaxAge = 10f,
			SizeImpact = 0.6f,
			FibrousPercentageOfTotalMass = 0f,
			LumberPercentageOfFiberMass = 0f
		};
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "tree_palewingweeddead_1_grown",
						Bendyness = bendyness
					}
				}
			}
		};
		entityType.PointLayoutType = new PointLayoutType();
		item = entityType;
		listOfEntityTypes.Add(item);
		bendyness = 0.3f;
		entityType = new EntityType("tree:riveraxle");
		entityType.Name = "Water cane";
		entityType.SummaryDescription = "Bamboo-like plant often found in riverbeds";
		entityType.Description = "\n BIOLOGY OVERVIEW\n The water cane is a non-flowering perennial evergreen which grows in wet, nutrient-rich soil.\n \n SURVIVAL GUIDE NOTES\n  Its versatility in crafting is highly prized by us, as the hollow stems have exorbitant strength. Not only are the water cane's stems beneficial, but its seeds can be harvested for human consumption and its small, stiff leaves can be used as fletching.";
		entityType.ThumbnailSmall = "HUD_thumbnail_riveraxle";
		entityType.TreeType = new TreeType
		{
			BulkPerSize = 1f,
			MatureAge = 5f,
			MaxAge = 10f,
			SizeImpact = 0.6f,
			FibrousPercentageOfTotalMass = 0f,
			LumberPercentageOfFiberMass = 0f,
			Crops = new string[3] { "crop:waterCaneLeaves", "crop:sticks", "crop:waterCaneStem" },
			DefaultCrops = new DefaultCrops[3]
			{
				new DefaultCrops
				{
					KeyName = "crop:waterCaneLeaves",
					MinItemsForFullGrownPlant = 0,
					MaxItemsForFullGrownPlant = 1
				},
				new DefaultCrops
				{
					KeyName = "crop:sticks",
					MinItemsForFullGrownPlant = 0,
					MaxItemsForFullGrownPlant = 1
				},
				new DefaultCrops
				{
					KeyName = "crop:waterCaneStem",
					MinItemsForFullGrownPlant = 0,
					MaxItemsForFullGrownPlant = 1
				}
			}
		};
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "tree_riveraxle_1_grown_summer",
						Bendyness = bendyness
					}
				}
			},
			ClientStateConditions = new ClientStateInfo[8]
			{
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_riveraxle_1_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 10)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_riveraxle_1_grown_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 10, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_riveraxle_1_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_riveraxle_1_grown_cut",
							Bendyness = 0.1f
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_riveraxle_1_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 15)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_riveraxle_1_young_summer",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 15, 14)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_riveraxledead_1_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 18)
				},
				new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tree_riveraxledead_1_grown",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 21, 18, 14)
				}
			}
		};
		entityType.PointLayoutType = new PointLayoutType();
		item = entityType;
		listOfEntityTypes.Add(item);
		bendyness = 0.3f;
		entityType = new EntityType("tree:riveraxledead");
		entityType.Name = "Water cane - dead";
		entityType.SummaryDescription = "Bamboo-like plant often found in riverbeds";
		entityType.Description = "\n BIOLOGY OVERVIEW\n The water cane is a non-flowering perennial evergreen which grows in wet, nutrient-rich soil.\n \n SURVIVAL GUIDE NOTES\n  Its versatility in crafting is highly prized by us, as the hollow stems have exorbitant strength. Not only are the water cane's stems beneficial, but its seeds can be harvested for human consumption and its small, stiff leaves can be used as fletching.";
		entityType.ThumbnailSmall = "HUD_thumbnail_riveraxle";
		entityType.TreeType = new TreeType
		{
			BulkPerSize = 1f,
			MatureAge = 5f,
			MaxAge = 10f,
			SizeImpact = 0.6f,
			FibrousPercentageOfTotalMass = 0f,
			LumberPercentageOfFiberMass = 0f,
			Crops = new string[3] { "crop:sticks", "crop:waterCaneStem", "crop:waterCaneSeeds" },
			DefaultCrops = new DefaultCrops[3]
			{
				new DefaultCrops
				{
					KeyName = "crop:sticks",
					MinItemsForFullGrownPlant = 0,
					MaxItemsForFullGrownPlant = 1
				},
				new DefaultCrops
				{
					KeyName = "crop:waterCaneStem",
					MinItemsForFullGrownPlant = 0,
					MaxItemsForFullGrownPlant = 1
				},
				new DefaultCrops
				{
					KeyName = "crop:waterCaneSeeds",
					MinItemsForFullGrownPlant = 0,
					MaxItemsForFullGrownPlant = 1
				}
			}
		};
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "tree_riveraxledead_1_grown",
						Bendyness = bendyness
					}
				}
			}
		};
		entityType.PointLayoutType = new PointLayoutType();
		item = entityType;
		listOfEntityTypes.Add(item);
		entityType = new EntityType("tree:desertbrambletiny");
		entityType.Name = "Desert bramble - small";
		entityType.SummaryDescription = "Tangled bushes that form a dense barrier.";
		entityType.Description = "\n BIOLOGY OVERVIEW\n Desert bramble is a collection of several small non-fruit-bearing, prickly shrubs that compete amongst one another for resources in temperate environments. The individual species do not have any natural weapons to combat each other leading the shrubs to enact a type of mutualistic relationship where each type of thorn from each different plant protects the whole group from separate dangers. Many small creatures use this variability of protection to their advantage when making nests in the bramble.";
		entityType.ThumbnailSmall = "HUD_thumbnail_bramble";
		entityType.TreeType = new TreeType
		{
			BulkPerSize = 10f,
			MatureAge = 6f,
			MaxAge = 40f,
			SizeImpact = 1f,
			FibrousPercentageOfTotalMass = 0.2f,
			LumberPercentageOfFiberMass = 0f
		};
		entityType.GatheringSiteType = new GatheringSiteType
		{
			arc = new Arc
			{
				Radius = 8f,
				MinAngle = -180.0,
				MaxAngle = 180.0
			},
			SeatSize = 1f,
			MaxVisitors = 2
		};
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "tree_palebrambletiny_1_grown",
						Bendyness = bendyness
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
							AssetName = "tree_palebrambletiny_1_young",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 15)
				}
			}
		};
		entityType.DefaultSimState = new SimStateInfo
		{
			GeometryLayoutType = new GeometryLayoutType
			{
				Shapes = new CollideShape2D[1]
				{
					new CollideShape2D(Vector2.Zero, 14f)
					{
						Offset = new Vector2(-1f, -4f)
					}
				}
			}
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		bendyness = 0.2f;
		entityType = new EntityType("tree:desertbramblesmall");
		entityType.Name = "Desert bramble - larger";
		entityType.SummaryDescription = "Tangled bushes that form a dense barrier.";
		entityType.Description = "\n BIOLOGY OVERVIEW\n Desert bramble is a collection of several small non-fruit-bearing, prickly shrubs that compete amongst one another for resources in temperate environments. The individual species do not have any natural weapons to combat each other leading the shrubs to enact a type of mutualistic relationship where each type of thorn from each different plant protects the whole group from separate dangers. Many small creatures use this variability of protection to their advantage when making nests in the bramble.";
		entityType.TreeType = new TreeType
		{
			BulkPerSize = 10f,
			MatureAge = 6f,
			MaxAge = 40f,
			SizeImpact = 1f,
			FibrousPercentageOfTotalMass = 0.2f,
			LumberPercentageOfFiberMass = 0f
		};
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "tree_palebramblesmall_1_grown",
						Bendyness = bendyness
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
							AssetName = "tree_palebramblesmall_1_young",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 15)
				}
			}
		};
		entityType.DefaultSimState = new SimStateInfo
		{
			GeometryLayoutType = new GeometryLayoutType
			{
				Shapes = new CollideShape2D[1]
				{
					new CollideShape2D(Vector2.Zero, 19f)
					{
						Offset = new Vector2(0f, -8f)
					}
				}
			}
		};
		item = entityType;
		listOfEntityTypes.Add(item);
		bendyness = 0.2f;
		entityType = new EntityType("tree:desertbramblemedium");
		entityType.Name = "Desert bramble - big";
		entityType.SummaryDescription = "Tangled bushes that form a dense barrier.";
		entityType.Description = "\n BIOLOGY OVERVIEW\n Desert bramble is a collection of several small non-fruit-bearing, prickly shrubs that compete amongst one another for resources in temperate environments. The individual species do not have any natural weapons to combat each other leading the shrubs to enact a type of mutualistic relationship where each type of thorn from each different plant protects the whole group from separate dangers. Many small creatures use this variability of protection to their advantage when making nests in the bramble.";
		entityType.TreeType = new TreeType
		{
			BulkPerSize = 10f,
			MatureAge = 6f,
			MaxAge = 40f,
			SizeImpact = 1f,
			FibrousPercentageOfTotalMass = 0.2f,
			LumberPercentageOfFiberMass = 0f
		};
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "tree_palebramblemedium_1_grown",
						Bendyness = bendyness
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
							AssetName = "tree_palebramblemedium_1_young",
							Bendyness = bendyness
						}
					},
					Conditions = new BitMask64(typeof(StateModifier), 15)
				}
			}
		};
		entityType.DefaultSimState = new SimStateInfo
		{
			GeometryLayoutType = new GeometryLayoutType
			{
				Shapes = new CollideShape2D[1]
				{
					new CollideShape2D(Vector2.Zero, 23f)
					{
						Offset = new Vector2(0f, -16f)
					}
				}
			}
		};
		item = entityType;
		listOfEntityTypes.Add(item);
	}
}
