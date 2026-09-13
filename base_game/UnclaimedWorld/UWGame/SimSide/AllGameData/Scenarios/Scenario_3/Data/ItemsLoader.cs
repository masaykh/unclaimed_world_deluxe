using System.Collections.Generic;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Items;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_3.Data;

internal class ItemsLoader
{
	public static void Init(List<EntityType> listOfEntityTypes)
	{
		listOfEntityTypes.Add(new EntityType("item:lines")
		{
			Name = "Wire rope",
			SummaryDescription = "Galvanized steel ropes",
			Description = "Strong ropes typically used for sail boat rigging.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.07f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry"
			},
			Category = GameData.Instance.AllEntityCategories["rawMaterials"],
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "wireRope"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:catamaranStart")
		{
			Name = "Catamaran",
			SummaryDescription = "The catamaran hull",
			Icon = "typeIcon_strippedCatamaran",
			ItemType = new ItemType
			{
				MaximumBulk = 5f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry"
			},
			Category = GameData.Instance.AllEntityCategories["rawMaterials"],
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "catamaranItem"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:catamaranMastSail")
		{
			Name = "Catamaran mast",
			SummaryDescription = "The mast from the catamaran",
			Icon = "typeIcon_catamaranMast",
			ItemType = new ItemType
			{
				MaximumBulk = 5f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry"
			},
			Category = GameData.Instance.AllEntityCategories["rawMaterials"],
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "catamaranMastSail"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:strippedCatamaran")
		{
			Name = "Stripped catamaran",
			SummaryDescription = "We've stripped the catamaran for any useful materials",
			Icon = "typeIcon_strippedCatamaran",
			ItemType = new ItemType
			{
				MaximumBulk = 5f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry"
			},
			Category = GameData.Instance.AllEntityCategories["rawMaterials"],
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "strippedCatamaran"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:catamaranMast")
		{
			Name = "Catamaran mast",
			SummaryDescription = "The mast from the catamaran",
			Icon = "typeIcon_catamaranMast",
			ItemType = new ItemType
			{
				MaximumBulk = 5f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry"
			},
			Category = GameData.Instance.AllEntityCategories["rawMaterials"],
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "catamaranMast"
						}
					}
				}
			}
		});
		ItemLoader.CreateDiamondKnife(listOfEntityTypes).ItemType.TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>
		{
			{
				ItemType.TaskType.LongerJourneys,
				ItemType.AppropriateLevel.None
			},
			{
				ItemType.TaskType.UnspecifiedHunting,
				ItemType.AppropriateLevel.None
			},
			{
				ItemType.TaskType.PatrolOrAttack,
				ItemType.AppropriateLevel.None
			},
			{
				ItemType.TaskType.Scouting,
				ItemType.AppropriateLevel.None
			},
			{
				ItemType.TaskType.Hauling,
				ItemType.AppropriateLevel.None
			}
		};
	}
}
