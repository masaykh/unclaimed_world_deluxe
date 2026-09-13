using System.Collections.Generic;
using UWGame.ClientSide;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Combat;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.SimSide.Entities.Substances;
using UWGame.SimSide.Items;
using UWGame.SimSide.Policies;
using UWGame.SimSide.XmlCollections;
using Xclna.Xna.Animation;

namespace UWGame.SimSide.AllGameData;

public class ItemLoader
{
	public const float toolDurabilityBrittle = 0f;

	public const float toolDurabilityAverage = 0.5f;

	public const float toolDurabilityDurable = 0.9f;

	public const float toolDurabilityUnbreakable = 1f;

	public const float assemblerPlateDurability = 0.8f;

	public static void Init(List<EntityType> listOfEntityTypes)
	{
		listOfEntityTypes.Add(new EntityType("item:meshTest")
		{
			Name = "Mesh test",
			SummaryDescription = "test the mesh",
			ItemType = new ItemType
			{
				MaximumBulk = 0.15f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				RenderAsModelType = new RenderAsModelType
				{
					AssetName = "meshtest",
					ModelScale = 10f
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:astroRation")
		{
			Name = "PRECOL Ration",
			SummaryDescription = "Meal. Consists of a variety of synthetic food items",
			Description = "Can be stored under any conditions and will keep almost indefinitely.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["lowWeightBalancedMeal"],
					IsMeal = true,
					FoodTags = new string[1] { "cookedMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				DegradeType = "dirt",
				Repairability = 0f,
				DegradesTo = "item:spoiledMeal"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "advanced"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "mealMeatstapleveg"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:blackpulp")
		{
			Name = "Blackpulp",
			SummaryDescription = "Inside the tough, rubbery shell of these fruits is a tar-like substance",
			Description = "Contains toxins that make it inedible to humans without some advanced processing",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richVegetables"],
					FoodTags = new string[1] { "inedibleVegi" }
				}
			},
			NonLivingType = new NonLivingType
			{
				DegradeType = "perishable",
				Repairability = 0f,
				DegradesTo = "item:rottenVegetables"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "blackpulp"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:blackzpacho")
		{
			Name = "Blackzpacho",
			SummaryDescription = "Cold soup made from blackpulp",
			Description = "The Tau Ceti variation of the Earth's gazpacho. To make this, the blackpulp must be processed with a specially engineered enzyme (that we can produce in a field lab).",
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richVegetables"],
					IsMeal = true,
					FoodTags = new string[1] { "edibleVegi" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "cooked food",
				DegradesTo = "item:spoiledMeal"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "mealBlackzpacho"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:minnowSoup")
		{
			Name = "Minnow soup",
			SummaryDescription = "Soup made from small fish",
			Description = "Lacking a larger catch, minnows can be used in a soup. The result varies, depending on the vegetables and spices used.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["meatSoup"],
					IsMeal = true,
					FoodTags = new string[1] { "cookedMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "cooked food",
				DegradesTo = "item:spoiledMeal"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "mealBeige"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:favorbread")
		{
			Name = "Favorbread",
			SummaryDescription = "A fleshy, nutritious vegetable which is edible to humans without preparation",
			Description = "A nourishing staple which can be found at the foot of the sanctuary tree. On dead sanctuary trees, the favorbread naturally ceases to grow but in these locations we have found a way to revive it with a simple method of cultivation: By excavating a small garden directly underneath the dead sanctuary tree and providing the carbohydrates that the tree no longer supplies it with.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richStaple"],
					FoodTags = new string[1] { "edibleVegi" }
				}
			},
			NonLivingType = new NonLivingType
			{
				DegradeType = "perishable",
				Repairability = 0f,
				DegradesTo = "item:rottenVegetables"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "favorbread"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:fingerFruit")
		{
			Name = "Finger fruit",
			SummaryDescription = "A nutritious vegetable resembling a small sausage. Requires cooking",
			Description = "We can cultivate the finger fruit plant in a greenhouse.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richVegetables"]
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "perishable, no freeze",
				DegradesTo = "item:rottenVegetables"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "vegetables"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:fingerPot")
		{
			Name = "Finger pot",
			SummaryDescription = "Vegetable dish made from fresh finger fruit",
			Description = "N/A",
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richVegetables"],
					IsMeal = true,
					FoodTags = new string[1] { "edibleVegi" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "cooked food",
				DegradesTo = "item:spoiledMeal"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "mealMeatstapleveg"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:fermentedFingerFruit")
		{
			Name = "Fermented finger fruit",
			SummaryDescription = "Finger fruit which has fermented in brine, increasing its lifespan",
			Description = "Will keep for a long time. The taste is a unique combination of salty, sweet and sour with distinctive flowery aromas.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f,
				RequiredStorageTags = new string[1] { "storageTagLiquidContainerClosedNoHeat" },
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richVegetables"],
					IsMeal = true,
					FoodTags = new string[1] { "edibleVegi" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "pickledFood",
				DegradesTo = "item:spoiledMeal"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "mealMeatstapleveg"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:driedBeef")
		{
			Name = "Beef jerky",
			SummaryDescription = "Beef jerky stays fresh for several months",
			Description = "Meat, trimmed of fat, salted and dried to prevent spoilage.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richMeat"],
					IsMeal = true,
					FoodTags = new string[1] { "cookedMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "meatSmoked"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:waterCaneSeeds")
		{
			Name = "Water cane seeds",
			SummaryDescription = "Found on the dead water cane",
			Description = "The seeds have some nutritional value and can even be eaten raw although thorough cooking is recommended.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["poorStaple"],
					FoodTags = new string[1] { "edibleVegi" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry",
				DegradesTo = "item:rottenStaple"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "staple"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:waterCanePorridge")
		{
			Name = "Water cane porridge",
			SummaryDescription = "A modest bush dish which provides much needed calories",
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["poorStaple"],
					IsMeal = true,
					FoodTags = new string[1] { "edibleVegi" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "cooked food",
				DegradesTo = "item:spoiledMeal"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "mealBlackzpacho"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:hexapineLeaves")
		{
			Name = "Hexapine leaves",
			SummaryDescription = "The leaves from the hexapine need enzyme treatment to become edible.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.125f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richVegetables"],
					FoodTags = new string[1] { "inedibleVegi" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "perishable, no freeze",
				DegradesTo = "item:rottenVegetables"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "vegetables"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:hexapineSalad")
		{
			Name = "Hexapine slaw",
			SummaryDescription = "Vegetable dish made from enzyme-treated hexapine leaves.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richVegetables"],
					IsMeal = true,
					FoodTags = new string[1] { "edibleVegi" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "cooked food",
				DegradesTo = "item:spoiledMeal"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "mealMeatstapleveg"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:turnipMeat")
		{
			Name = "Turnip meat",
			SummaryDescription = "The meat is hard to extract from the turnip's shell.",
			Description = "Has a characteristic blue tinge and a flavor that takes some getting used to.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.07f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					FoodTags = new string[1] { "inedibleMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "raw meat",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "meatraw"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:turnipBrain")
		{
			Name = "Turnip brain",
			SummaryDescription = "A rather small brain for such a large animal",
			Description = "The brain contains oils that can be used as a primitive tanning agent for hide treatment.",
			ToolType = new ToolType
			{
				Durability = 0.2f,
				ToolHandling = ToolHandlingType.HandTool
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.15f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					FoodTags = new string[1] { "inedibleMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "raw meat",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "gutsyellow"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:turnipGuts")
		{
			Name = "Turnip guts",
			SummaryDescription = "The vast guts of a large grass eater",
			Description = "These intestines can find use in cooking and possibly as a natural material.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					FoodTags = new string[1] { "inedibleMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "raw meat",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "gutspink"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:turnipTentacles")
		{
			Name = "Turnip tentacles",
			ItemType = new ItemType
			{
				MaximumBulk = 0.15f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					FoodTags = new string[1] { "inedibleMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "raw seafood",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tentacles"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:turnipRoast")
		{
			Name = "Turnip roast",
			SummaryDescription = "Big, soft pieces of roasted meat with a characteristic blue tinge",
			Description = "N/A",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					IsMeal = true,
					FoodTags = new string[1] { "cookedMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "cooked food",
				DegradesTo = "item:spoiledMeal"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "mealMeatstapleveg"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:smokedTurnip")
		{
			Name = "Smoked turnip meat",
			SummaryDescription = "Pieces of smoked meat from the turnip animal. Will keep for some time",
			Description = "N/A",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					IsMeal = true,
					FoodTags = new string[1] { "cookedMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "somewhatPreserved",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "meatSmoked"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:turnipRawSausage")
		{
			Name = "Turnip raw sausage",
			SummaryDescription = "Fermented sausages made from turnip meat. Can be cooked or made into salami",
			Description = "Yeast-like bacteria have fermented these sausages. They can now either be dried, making long-lasting salami or they can be cooked for immediate consumption. The sausages have a limited shelf life in the current condition.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					FoodTags = new string[1] { "inedibleMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "raw meat",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "meatraw"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:turnipSalami")
		{
			Name = "Turnip salami",
			SummaryDescription = "A tasty, ready to eat salami which can keep for months",
			Description = "This meat product is the result of careful craftsmanship. It is made from fermented sausages that are hung to dry for a long time in very specific humidity and temperature conditions.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					IsMeal = true,
					FoodTags = new string[1] { "cookedMeat" }
				}
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic",
				Area = RatingTypes.Food
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry",
				DegradesTo = "item:spoiledMeal"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "mealMeatstapleveg"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:turnipFriedSausage")
		{
			Name = "Turnip fried sausage",
			SummaryDescription = "Juicy sausages made from turnip meat. They have a characteristic blue tinge",
			Description = "N/A",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					IsMeal = true,
					FoodTags = new string[1] { "cookedMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "cooked food",
				DegradesTo = "item:spoiledMeal"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "mealMeatstapleveg"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:twinklerGuts")
		{
			Name = "Quadite innards",
			SummaryDescription = "N/A",
			Description = "N/A",
			ItemType = new ItemType
			{
				MaximumBulk = 0.15f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					FoodTags = new string[1] { "inedibleMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "raw seafood",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "waste",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "gutspink"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:twinklerMeat")
		{
			Name = "Quadite flesh",
			SummaryDescription = "Riddled with scamp larvae (which in time become edible)",
			Description = "\n BIOLOGY OVERVIEW\n The Scamp larvae start their life as parasites inside the quadite's digestive canals. After the animal dies of unrelated causes, the larvae will quickly devour its flesh, growing in size and ending the feeding frenzy when the biggest grub devours its siblings.\n \nSURVIVAL GUIDE NOTES\n The scamp larvae secrete harmful substances designed to ward off other carrion-eaters, but once their feeding ends, the single scamp that remains can be cooked and eaten by humans. This requires that the meat be stored for a while, closely monitored so that the grub can be caught before it transitions into its next stage, the Scamp beetle. (The beetle is not to be eaten, even after cooking.)",
			ItemType = new ItemType
			{
				MaximumBulk = 0.08f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					FoodTags = new string[1] { "inedibleMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "raw seafood",
				DegradesTo = "item:scampGrub"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "gutspink"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:twinklerShred")
		{
			Name = "Quadite shreds",
			SummaryDescription = "Shreds of quadite flesh, torn off the carcass by scavengers",
			Description = "These pieces of flesh have been tainted by enzymes from the digestive fluids of wild animals. They will decompose and rot quickly, are not fit for human consumption and have little use to humans.",
			ItemType = new ItemType
			{
				HasNoMaximumBulk = true,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					FoodTags = new string[1] { "inedibleMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "fleshShreds",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "fleshShred"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:binalRatShred")
		{
			Name = "Binal rat shreds",
			SummaryDescription = "Small remnants of a binal rat, torn off by animals that feed on the carcass",
			Description = "These pieces of flesh have been tainted by enzymes from the digestive fluids of wild animals. They will decompose and rot quickly, are not fit for human consumption and have little use to humans.",
			ItemType = new ItemType
			{
				HasNoMaximumBulk = true,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					FoodTags = new string[1] { "inedibleMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "fleshShreds",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "fleshShred"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:thunderChickenShred")
		{
			Name = "Thunder chicken shreds",
			SummaryDescription = "Scraps of flesh torn off by animals feeding on the carcass",
			Description = "These pieces of flesh have been tainted by enzymes from the digestive fluids of wild animals. They will decompose and rot quickly, are not fit for human consumption and have little use to humans.",
			ItemType = new ItemType
			{
				HasNoMaximumBulk = true,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					FoodTags = new string[1] { "inedibleMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "fleshShreds",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "fleshShred"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:patricianShred")
		{
			Name = "Patrician shreds",
			SummaryDescription = "Scraps of flesh torn off by animals feeding on the carcass",
			Description = "These pieces of flesh have been tainted by enzymes from the digestive fluids of wild animals. They will decompose and rot quickly, are not fit for human consumption and have little use to humans.",
			ItemType = new ItemType
			{
				HasNoMaximumBulk = true,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					FoodTags = new string[1] { "inedibleMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "fleshShreds",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "fleshShred"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:bushDragonShred")
		{
			Name = "Bush dragon shreds",
			SummaryDescription = "Scraps of flesh torn off by animals feeding on the carcass",
			Description = "These pieces of flesh have been tainted by enzymes from the digestive fluids of wild animals. They will decompose and rot quickly, are not fit for human consumption and have little use to humans.",
			ItemType = new ItemType
			{
				HasNoMaximumBulk = true,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					FoodTags = new string[1] { "inedibleMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "fleshShreds",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "fleshShred"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:turnipShred")
		{
			Name = "Turnip shreds",
			SummaryDescription = "Scraps of flesh torn off by animals feeding on the carcass",
			Description = "These pieces of flesh have been tainted by enzymes from the digestive fluids of wild animals. They will decompose and rot quickly, are not fit for human consumption and have little use to humans.",
			ItemType = new ItemType
			{
				HasNoMaximumBulk = true,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					FoodTags = new string[1] { "inedibleMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "fleshShreds",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "fleshShred"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:demonTreeShred")
		{
			Name = "Dendront shreds",
			SummaryDescription = "Scraps of flesh torn off by animals feeding on the carcass",
			Description = "These pieces of flesh have been tainted by enzymes from the digestive fluids of wild animals. They will decompose and rot quickly, are not fit for human consumption and have little use to humans.",
			ItemType = new ItemType
			{
				HasNoMaximumBulk = true,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					FoodTags = new string[1] { "inedibleMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "fleshShreds",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "fleshShred"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:swampDemonTreeShred")
		{
			Name = "Swamp tree demon shreds",
			SummaryDescription = "Scraps of flesh torn off by animals feeding on the carcass",
			Description = "These pieces of flesh have been tainted by enzymes from the digestive fluids of wild animals. They will decompose and rot quickly, are not fit for human consumption and have little use to humans.",
			ItemType = new ItemType
			{
				HasNoMaximumBulk = true,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					FoodTags = new string[1] { "inedibleMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "fleshShreds",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "fleshShred"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:megapodShred")
		{
			Name = "Megapod shreds",
			SummaryDescription = "Scraps of flesh torn off by animals feeding on the carcass",
			Description = "These pieces of flesh have been tainted by enzymes from the digestive fluids of wild animals. They will decompose and rot quickly, are not fit for human consumption and have little use to humans.",
			ItemType = new ItemType
			{
				HasNoMaximumBulk = true,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					FoodTags = new string[1] { "inedibleMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "fleshShreds",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "fleshShred"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:whipjawShred")
		{
			Name = "Whipjaw shreds",
			SummaryDescription = "Scraps of flesh torn off by animals feeding on the carcass",
			Description = "These pieces of flesh have been tainted by enzymes from the digestive fluids of wild animals. They will decompose and rot quickly, are not fit for human consumption and have little use to humans.",
			ItemType = new ItemType
			{
				HasNoMaximumBulk = true,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					FoodTags = new string[1] { "inedibleMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "fleshShreds",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "fleshShred"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:spikePlantShred")
		{
			Name = "Spike plant shreds",
			SummaryDescription = "Scraps of flesh torn off by animals feeding on the carcass",
			Description = "These pieces of flesh have been tainted by enzymes from the digestive fluids of wild animals. They will decompose and rot quickly, are not fit for human consumption and have little use to humans.",
			ItemType = new ItemType
			{
				HasNoMaximumBulk = true,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					FoodTags = new string[1] { "inedibleMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "fleshShreds",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "fleshShred"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:forestguardianShred")
		{
			Name = "Forest guardian shreds",
			SummaryDescription = "Scraps of flesh torn off by animals feeding on the carcass",
			Description = "These pieces of flesh have been tainted by enzymes from the digestive fluids of wild animals. They will decompose and rot quickly, are not fit for human consumption and have little use to humans.",
			ItemType = new ItemType
			{
				HasNoMaximumBulk = true,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					FoodTags = new string[1] { "inedibleMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "fleshShreds",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "fleshShred"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:thinThunderChickenShred")
		{
			Name = "Bajingan shreds",
			SummaryDescription = "Scraps of flesh torn off by animals feeding on the carcass",
			Description = "These pieces of flesh have been tainted by enzymes from the digestive fluids of wild animals. They will decompose and rot quickly, are not fit for human consumption and have little use to humans.",
			ItemType = new ItemType
			{
				HasNoMaximumBulk = true,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					FoodTags = new string[1] { "inedibleMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "fleshShreds",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "fleshShred"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:leafcutterShred")
		{
			Name = "Field quadite shreds",
			SummaryDescription = "Scraps of flesh torn off by animals feeding on the carcass",
			Description = "These pieces of flesh have been tainted by enzymes from the digestive fluids of wild animals. They will decompose and rot quickly, are not fit for human consumption and have little use to humans.",
			ItemType = new ItemType
			{
				HasNoMaximumBulk = true,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					FoodTags = new string[1] { "inedibleMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "fleshShreds",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "fleshShred"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:mudWormShred")
		{
			Name = "Mud worm shreds",
			SummaryDescription = "Scraps of flesh torn off by animals feeding on the carcass",
			Description = "These pieces of flesh have been tainted by enzymes from the digestive fluids of wild animals. They will decompose and rot quickly, are not fit for human consumption and have little use to humans.",
			ItemType = new ItemType
			{
				HasNoMaximumBulk = true,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					FoodTags = new string[1] { "inedibleMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "fleshShreds",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "fleshShred"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:binalRatChunk")
		{
			Name = "Binal rat flesh chunk",
			SummaryDescription = "A cut up binal rat",
			Description = "Can be used as bait, may even attract other binal rats. Binal rat meat is inedible by humans due to the high amount of toxins.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.5f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					FoodTags = new string[1] { "inedibleMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "raw meat",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "fleshShred"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:thunderChickenMeat")
		{
			Name = "Thunder chicken meat",
			SummaryDescription = "Various cuts from the planet's most tasty animal",
			Description = "Surprisingly dark meat.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.07f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					FoodTags = new string[1] { "rawMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "raw meat",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "meatraw"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:thunderChickenGuts")
		{
			Name = "Thunder chicken guts",
			SummaryDescription = "Offal to go in a stew.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					FoodTags = new string[1] { "rawMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "raw seafood",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "gutsyellow"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:thunderChickenStew")
		{
			Name = "Thunder chicken stew",
			SummaryDescription = "A modest campfire meal cooked in a pot",
			Description = "Main ingredient is thunder chicken offal. The cook adds whatever spices are at hand.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["meatSoup"],
					IsMeal = true,
					FoodTags = new string[1] { "cookedMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "cooked food",
				DegradesTo = "item:spoiledMeal"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "mealMeatstapleveg"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:thunderChickenSkewers")
		{
			Name = "Chicken skewers",
			SummaryDescription = "Pieces of thunder chicken meat put on spits and roasted on the campfire",
			Description = "The cook mixes the meat pieces with whatever vegetables are at hand. Provides a good deal of recommended daily intake of protein and nutrients.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					IsMeal = true,
					FoodTags = new string[1] { "cookedMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "cooked food",
				DegradesTo = "item:spoiledMeal"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "mealMeatstapleveg"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:smokedThunderChicken")
		{
			Name = "Smoked thunder chicken",
			SummaryDescription = "Smoked meat that keeps for some days.",
			Description = "This meat has been preserved by smoking, extending its shelf life when stored at room temperature. But it will not keep indefinitely.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					IsMeal = true,
					FoodTags = new string[1] { "cookedMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "somewhatPreserved",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "meatSmoked"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:driedThunderChicken")
		{
			Name = "Dried thunder chicken",
			SummaryDescription = "Dried meat that keeps for several months.",
			Description = "This meat has been preserved by drying and salting and will not spoil easily.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					IsMeal = true,
					FoodTags = new string[1] { "cookedMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "meatSmoked"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:clamwich")
		{
			Name = "Clamwich",
			SummaryDescription = "Mollusc found on mudflats",
			Description = "\n SURVIVAL GUIDE NOTES\n The clamwich offers some nutritional value but has harmful toxins and needs to be carefully cleaned before cooking. It's a time consuming process and a well-organized kitchen area is recommended.\n \n BIOLOGY OVERVIEW\n Through sheer evolutionary chance, the clamwich shares many characteristics of Earth-based clams, specifically the Northern Quahog. The clamwich feeds on plankton and other organic particles extracted from the water. Can be found where tides and rivers deposit muddy sediments.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					FoodTags = new string[1] { "smallRawMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "raw seafood",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "clamwichPile"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:clamwichSoup")
		{
			Name = "Clamwich soup",
			SummaryDescription = "Soup made of clamwich",
			Description = "Requires some boiling.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["meatSoup"],
					IsMeal = true,
					FoodTags = new string[1] { "cookedMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "cooked food",
				DegradesTo = "item:spoiledMeal"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "mealBlackzpacho"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:torux")
		{
			Name = "Torux",
			SummaryDescription = "Donut shaped mollusc found on beaches",
			Description = "\n BIOLOGY OVERVIEW\n The peculiar shellfish found on saltwater shores are the second stage of the torux life cycle. \n When it washes up on the shore it anchors itself to the seabed, converting to a stationary life similar to a mussel.\n \nSURVIVAL GUIDE NOTES\n Edible after cooking.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					FoodTags = new string[1] { "smallRawMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "raw seafood",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "toruxPile"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:bakedTorux")
		{
			Name = "Baked torux",
			SummaryDescription = "Baked in their shells among the coals",
			Description = "The taste takes some getting used to, but it serves as a modest source of protein.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					IsMeal = true,
					FoodTags = new string[1] { "cookedMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "cooked food",
				DegradesTo = "item:spoiledMeal"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "toruxPile"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:alabasterRay")
		{
			Name = "Alabaster ray",
			SummaryDescription = "Aquatic, plated creature",
			Description = "\n BIOLOGY OVERVIEW\n Spends its life on the seabed, its flat body weighed down by heavy bony armor. It can be spotted in shallow coastal waters feeding on crustaceans which it dislodges and crushes with its strong jaws. It is reminiscent of the prehistoric plated fish that once were widespread in Earth's oceans.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					FoodTags = new string[1] { "rawMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "raw seafood",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "meatraw"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:roastedAlabasterRay")
		{
			Name = "Roasted alabaster ray",
			SummaryDescription = "Pieces of alabaster meat roasted on long skewers",
			Description = "It takes some time to separate the flesh from the shell, but it is well worth the effort.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					IsMeal = true,
					FoodTags = new string[1] { "cookedMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "cooked food",
				DegradesTo = "item:spoiledMeal"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "mealMeatstapleveg"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:alabasterStew")
		{
			Name = "Alabaster stew",
			SummaryDescription = "Alabaster ray left to simmer in a pot",
			Description = "Tender and juicy alabaster meat slowly heated in a pot with whatever spices and vegetables the cook chooses.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["meatSoup"],
					IsMeal = true,
					FoodTags = new string[1] { "cookedMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "cooked food",
				DegradesTo = "item:spoiledMeal"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "mealMeatstapleveg"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:pickledAlabasterRay")
		{
			Name = "Pickled alabaster ray",
			SummaryDescription = "Fried fish pickled in vinegar. Keeps for several months",
			Description = "This fish has been preserved by pickling (covered in vinegar) and will not spoil easily.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				RequiredStorageTags = new string[1] { "storageTagLiquidContainerClosedNoHeat" },
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					IsMeal = true,
					FoodTags = new string[1] { "cookedMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "pickledFood",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "meatSmoked"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:smokedAlabasterRay")
		{
			Name = "Smoked alabaster ray",
			SummaryDescription = "Smoked fish that keeps for some days",
			Description = "This fish has been preserved by smoking, extending its shelf life when stored at room temperature. But it will not keep indefinitely.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					IsMeal = true,
					FoodTags = new string[1] { "cookedMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "somewhatPreserved",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "meatSmoked"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:streakFin")
		{
			Name = "Streak fin",
			SummaryDescription = "Fast-moving, saltwater predator. Catches flying insects.",
			Description = "Has some nutritional value. Can be caught with pig fly bait.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.07f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richMeat"],
					FoodTags = new string[1] { "rawMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "raw seafood",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tentacles"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:roastedStreakFin")
		{
			Name = "Roasted streak fin",
			SummaryDescription = "Rather tangy tasting roasted fish.",
			Description = "The taste is best drowned out by campfire smoke and generous use of spices.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richMeat"],
					IsMeal = true,
					FoodTags = new string[1] { "cookedMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "cooked food",
				DegradesTo = "item:spoiledMeal"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "mealMeatstapleveg"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:smokedStreakFin")
		{
			Name = "Smoked streak fin",
			SummaryDescription = "Smoked fish that keeps for some days",
			Description = "This fish has been preserved by smoking, extending its shelf life when stored at room temperature. But it will not keep indefinitely.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richMeat"],
					IsMeal = true,
					FoodTags = new string[1] { "cookedMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "somewhatPreserved",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "meatSmoked"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:driedSaltedStreakFin")
		{
			Name = "Dried, salted streak fin",
			SummaryDescription = "Fish that has been salted and dried. Can keep for many months",
			Description = "Streak fin shares similarities with Earth's lean whitefish such as cod and is well suited for this preservation method. Thoroughly dried and salted it can be stored at room temperature for a long time. Inedible to humans and animals before the salt has been removed by soaking in water.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richMeat"],
					FoodTags = new string[1] { "inedibleIngredient" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "meatSmoked"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:desalinatedStreakFin")
		{
			Name = "Soaked streak fin",
			SummaryDescription = "Salted fish that has been soaked for some time and is ready to eat",
			Description = "Dried, salted streakfin requires that the salt be removed before it can be eaten. This is done by soaking it in water for some time.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richMeat"],
					IsMeal = true,
					FoodTags = new string[1] { "cookedMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "cooked food",
				DegradesTo = "item:spoiledMeal"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "mealMeatstapleveg"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:carbonTail")
		{
			Name = "Carbon tail",
			SummaryDescription = "Powerful swimmer that lurks among the water weeds",
			Description = "\n BIOLOGY OVERVIEW\n The creature feeds by ambushing passing fishes.\n \nSURVIVAL GUIDE NOTES\n It can be caught with patience and appropriate bait. A meal can then be prepared from it.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.07f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richMeat"],
					FoodTags = new string[1] { "rawMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "raw seafood",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tentaclesPurple"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:roastedCarbonTail")
		{
			Name = "Roasted carbon tail",
			SummaryDescription = "Carbon tail roasted in the coals",
			Description = "Very appetizing appearance and smell.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richMeat"],
					IsMeal = true,
					FoodTags = new string[1] { "cookedMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "cooked food",
				DegradesTo = "item:spoiledMeal"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "mealBlackzpacho"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:smokedCarbonTail")
		{
			Name = "Smoked carbon tail",
			SummaryDescription = "Smoked fish that keeps for some days",
			Description = "This fish has been preserved by smoking, extending its shelf life when stored at room temperature. But it will not keep indefinitely.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richMeat"],
					IsMeal = true,
					FoodTags = new string[1] { "cookedMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "somewhatPreserved",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "meatSmoked"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:pickledCarbonTail")
		{
			Name = "Pickled carbon tail",
			SummaryDescription = "Fried fish pickled in vinegar. Keeps for several months",
			Description = "This fish has been preserved by pickling and will not spoil easily.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				RequiredStorageTags = new string[1] { "storageTagLiquidContainerClosedNoHeat" },
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richMeat"],
					IsMeal = true,
					FoodTags = new string[1] { "cookedMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "pickledFood",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "meatSmoked"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:phantomWeaver")
		{
			Name = "Phantom weaver",
			SummaryDescription = "Small creature. Completely unknown.",
			Description = "This peculiar animal may or may not be fit to eat. There is abundant room for further research.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					FoodTags = new string[1] { "rawMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "raw seafood",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "meatraw"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:roastedPhantomWeaver")
		{
			Name = "Roasted phantom weaver",
			SummaryDescription = "Phantom weaver roasted on a skewer",
			Description = "Provides some amount of protein",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					IsMeal = true,
					FoodTags = new string[1] { "cookedMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "cooked food",
				DegradesTo = "item:spoiledMeal"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "mealBlackzpacho"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:webWing")
		{
			Name = "Web wing",
			SummaryDescription = "Elusive flyer. Virtually unknown.",
			Description = "\n BIOLOGY OVERVIEW\n The web wing's lifecycle has four stages, like many insectoid arthropods, consisting of an egg stage, a larva stage, a pupa stage, and an adult stage. With many predators taking advantage of the animal's vulnerability during its first three stages of life, actual sightings of a winged adult are rare. The web wing's name comes from the spiderweb pattern that adorns the creature's fragile, butterfly-like wings.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					FoodTags = new string[1] { "smallRawMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "raw seafood",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "meatraw"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:roastedWebWing")
		{
			Name = "Roasted web wing",
			SummaryDescription = "Web wing roasted on a skewer",
			Description = "Provides some amount of protein",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					IsMeal = true,
					FoodTags = new string[1] { "cookedMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "cooked food",
				DegradesTo = "item:spoiledMeal"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "mealBlackzpacho"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:crestedFoiler")
		{
			Name = "Crested foiler",
			SummaryDescription = "Small, beautifully colored herbivore.",
			Description = "\n BIOLOGY OVERVIEW\n Somewhere between avian and insectoid, the crested foiler is highly recognizable by the small feathery tuft located on the top of its head. Both males and females are brightly colored and armored, suggesting that the foiler's appearance is more likely to be a type of defensive camouflage or aposematism rather than for mate attraction. The small foiler is too heavy for long-term flight, though it has many other defensive mechanisms to make up for this deficit: a sharp-hooked beak, a set of clawed wings, and a piercing shriek which can disorient its predators.\n \nSURVIVAL GUIDE NOTES\n The crested foiler doesn't seem to be a good candidate as a food source as its body doesn't produce a significant amount of meat.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					FoodTags = new string[1] { "smallRawMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "raw seafood",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "meatraw"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:roastedCrestedFoiler")
		{
			Name = "Roasted foiler",
			SummaryDescription = "Crested foiler roasted on a skewer",
			Description = "Provides some amount of protein",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					IsMeal = true,
					FoodTags = new string[1] { "cookedMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "cooked food",
				DegradesTo = "item:spoiledMeal"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "mealBlackzpacho"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:goldenCenobite")
		{
			Name = "Golden cenobite",
			SummaryDescription = "Small reptilian predator",
			Description = "\n BIOLOGY OVERVIEW\n The elusive cenobite often lives a hermitic life, separating itself from other individuals of its own species save for the purpose of mating. The creature can be found in two colors, black and the more common golden shade. The golden cenobite appears to be less violent in nature compared to its cousin, but its venom is equally, if not more, potent.\n \nSURVIVAL GUIDE NOTES\n Though the cenobite's fangs don't appear to be well-suited to pierce human skin, anyone handling it should take caution, especially during food preparation.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					FoodTags = new string[1] { "smallRawMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "raw seafood",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "smallCarcass"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:roastedGoldenCenobite")
		{
			Name = "Roasted cenobite",
			SummaryDescription = "Golden cenobite roasted on a skewer",
			Description = "Provides some amount of protein",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					IsMeal = true,
					FoodTags = new string[1] { "cookedMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "cooked food",
				DegradesTo = "item:spoiledMeal"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "mealBlackzpacho"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:treeScuttler")
		{
			Name = "Tree scuttler",
			SummaryDescription = "An especially large, voracious species of scuttler bug",
			Description = "BIOLOGY OVERVIEW\n Resembling an arachnid, the tree scuttler prefers to make its home in the leaves of the spoak tree, but doesn't discriminate against other leafy trees and shrubs. Its nests are built of a thin silky material. The creature has an extremely elevated metabolism and one scuttler can consume all the leaves of a single spoak in only a month; a swarm of scuttlers are capable of clearing a single tree in a matter of days. \n \nSURVIVAL GUIDE NOTES\n It takes just one of these creatures to make short work of an improvised shelter unless the building materials have been treated with a suitable repellant.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					FoodTags = new string[1] { "inedibleMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "raw seafood",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "smallCarcass"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:stinkpup")
		{
			Name = "Stinkpup",
			SummaryDescription = "Small, mostly nocturnal animal",
			Description = "Scampers about in the undergrowth hunting for smaller prey. When threatened, releases a pungent smell as a deterrent. The creature's movements are highly erratic and it is capable of jumping short distances.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.07f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					FoodTags = new string[1] { "inedibleMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "raw seafood",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "smallCarcass"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:muckGrinder")
		{
			Name = "Muck grinder",
			SummaryDescription = "Grotesque creature, truly alien in appearance.",
			Description = "\n BIOLOGY OVERVIEW\n The muck grinder's physical structure and behavior are unlike anything previously encountered. It appears to have some sort of tongue-like appendage used for filtering through decayed matter within great reserves of mud. The creature then grinds down the matter with a set of sharp teeth formed around the inside of its cylindrical body. The tongue may also serve to make the grinder locomotive.\n \nSURVIVAL GUIDE NOTES\n Its general appearance tends to evoke revulsion in humans, though with some nimble bladework and application of heat it could be edible.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					FoodTags = new string[1] { "rawMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "raw seafood",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "smallCarcass"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:roastedMuckGrinder")
		{
			Name = "Roasted muck grinder",
			SummaryDescription = "Wholly unappetizing dish",
			Description = "Taste, appearance and smell are uniformly bad.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					IsMeal = true,
					FoodTags = new string[1] { "cookedMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "cooked food",
				DegradesTo = "item:spoiledMeal"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "mealBlackzpacho"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:spriteSlug")
		{
			Name = "Sprite slug",
			SummaryDescription = "Small, slug-like creature covered in iridescent spikes. Contains a stimulant, eaten raw.",
			Description = "\n BIOLOGY OVERVIEW\n Flashes with colour to attract mates.\n \nSURVIVAL GUIDE NOTES\n Can be eaten raw if great care is taken not to touch its sharp spikes. It contains chemical compounds that have a stimulating effect on humans.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["lowStimulant"],
					IsMeal = true,
					FoodTags = new string[1] { "cookedMeat" }
				}
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Comfort
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "perishable, no freeze",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tentacles"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:crazyDweller")
		{
			Name = "Crazy dweller",
			SummaryDescription = "Lives in crevices between rocks and large trees",
			Description = "\n BIOLOGY OVERVIEW\n Must occasionally emerge from its hiding hole. On those occasions it uses erratic movement and behavior to confuse and evade its many predators.\n \nSURVIVAL GUIDE NOTES\n Hard to catch but will make a tasty meal after cooking",
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					FoodTags = new string[1] { "smallRawMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "raw seafood",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "smallCarcass"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:roastedCrazyDweller")
		{
			Name = "Roasted crazy dweller",
			SummaryDescription = "Rather delicious.",
			Description = "Catching and cooking this creature is worth the effort.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					IsMeal = true,
					FoodTags = new string[1] { "cookedMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "cooked food",
				DegradesTo = "item:spoiledMeal"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "mealBlackzpacho"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:daggermouth")
		{
			Name = "Daggermouth",
			SummaryDescription = "Long-bodied marine predator with sharp teeth",
			Description = "\n BIOLOGY OVERVIEW\n Thin, long and vicious, this predator's diet consists mostly of smaller fish, though it has been known to attack land-based critters that stray too close to its home.\n \nSURVIVAL GUIDE NOTES\n The daggermouth is not a fish to handle haphazardly. Its razor-sharp teeth can pierce flesh easily and can make short work of netting. Collection of the daggermouth should only be undertaken through spear-fishing. With the right cooking it can become a nutritious meal.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.15f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					FoodTags = new string[1] { "rawMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "raw seafood",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "smallCarcass"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:roastedDaggermouth")
		{
			Name = "Cooked daggermouth",
			SummaryDescription = "Cooked directly in the coals",
			Description = "After cooking, its tough hide is cut open and the insides can be scooped out.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					IsMeal = true,
					FoodTags = new string[1] { "cookedMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "cooked food",
				DegradesTo = "item:spoiledMeal"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "mealBlackzpacho"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:impEel")
		{
			Name = "Imp eel",
			SummaryDescription = "Elusive, long-bodied swimmer",
			Description = "\n BIOLOGY OVERVIEW\n Creature that swims at great speed inside the muckroot canal system, hunting smaller fishes. Its enemy is the patrician which is able to detect its presence from outside the muckroot tube walls. The patrician will stand in wait at carefully chosen choke points, then stab the imp eel through the wall, injecting it with a powerful dissolvant and sucking up its insides.\n \nSURVIVAL GUIDE NOTES\n If any attempt is made to catch this creature, attention is advised. A patrician might appear to protect its hunting grounds.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.15f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					FoodTags = new string[1] { "rawMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "raw seafood",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tentacles"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:roastedImpEel")
		{
			Name = "Roasted imp eel",
			SummaryDescription = "Roasted on a spit. Rather savory",
			Description = "Provides some amount of protein",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					IsMeal = true,
					FoodTags = new string[1] { "cookedMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "cooked food",
				DegradesTo = "item:spoiledMeal"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "mealBlackzpacho"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:scampBeetle")
		{
			Name = "Scamp beetle",
			SummaryDescription = "Winged bug the size of a hand",
			Description = "\n BIOLOGY OVERVIEW\n Little is known about this stage of the scamp's life cycle. The previous stage was a grub emerged from a quadite carcass.\n \nSURVIVAL GUIDE NOTES\n The adult scamp is an aggressive beetle that can inflict a nasty bite.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.06f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					FoodTags = new string[1] { "smallRawMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "raw meat"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "smallCarcass"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:scampGrub")
		{
			Name = "Scamp grub",
			SummaryDescription = "Larvae stage of the scamp beetle",
			Description = "\n BIOLOGY OVERVIEW\n Parasite found in some quadite species. Once the quadite dies, the parasites grow in size as they quickly devour the quadite's flesh, after which they turn on each other. A single grub survives the feeding frenzy. It will then transition into a scamp beetle. This stage in its life cycle is less understood.\n \nSURVIVAL GUIDE NOTES\n After cooking, the scamp grub is an excellent source of protein.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.06f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					FoodTags = new string[1] { "smallRawMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "raw seafood",
				DegradesTo = "item:scampBeetle"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "smallCarcass"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:grubGrub")
		{
			Name = "Grub grub",
			SummaryDescription = "Scamp grub roasted on a skewer",
			Description = "A good source of protein in the wilderness.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					IsMeal = true,
					FoodTags = new string[1] { "cookedMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "cooked food",
				DegradesTo = "item:spoiledMeal"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "mealBlackzpacho"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:ursinix")
		{
			Name = "Ursinix",
			SummaryDescription = "Spherically shaped animal that lies in ambush, half buried.",
			Description = "ANATOMY: Many legs on a spiked, round body give the ursinix an appearance somewhere between the terran spider and sea urchin. Has the size of a watermelon. \n \nBEHAVIOR/HABITAT: Most species are found in coastal areas. Burrows in soft, wet sediments where it lies in ambush, its body half covered. Attracts prey by extending a multicolor and vibrating lure into the air. Its ability to imitate many sounds and movements makes it able to attract a diverse range of small animals, although the binal rat is probably its favorite prey. Once the prey is near, the ursinix quickly impales it with a barbed spike, injecting a paralyzing venom which acts within seconds. The prey is then dragged into the gaping maw of the ursinix.\n \nSURVIVAL GUIDE NOTES\n We have found the ursinix to be a valuable food source, but preparing it for cooking is a rather elaborate process. It is recommended to kill the animal with a spear, out of reach of its venomous spike. It must then be dug out and the poison gland carefully removed from its back. Then, cut the ursinix in half alongside the mouth, remove stomach contents and set the two halves out to dry under the sun. This last step is very important, as the UV rays will break down harmful toxins.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.17f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richMeat"],
					FoodTags = new string[1] { "rawMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "raw seafood",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "gutspink"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:cleanedUrsinix")
		{
			Name = "Ursinix (cleaned)",
			SummaryDescription = "Ursinix with poison gland and stomach contents removed",
			Description = "Ursinix, cut in two halves and cleaned. Before it can be used in cooking, it needs to be dried in the sun. UV rays will break down the toxins in its flesh.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richMeat"],
					FoodTags = new string[1] { "rawMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "sun drying",
				DegradesTo = "item:driedUrsinix"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "gutspink"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:driedUrsinix")
		{
			Name = "Ursinix (dried)",
			SummaryDescription = "Ursinix ready for cooking",
			Description = "With poison gland removed and toxins broken down by sun drying, the ursinix is ready to be cooked.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richMeat"],
					FoodTags = new string[1] { "rawMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "brainsYellow"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:grilledUrsinix")
		{
			Name = "Grilled Ursinix",
			SummaryDescription = "Excellent nutritional value and sweet crab-like taste.",
			Description = "",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richMeat"],
					IsMeal = true,
					FoodTags = new string[1] { "cookedMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "cooked food",
				DegradesTo = "item:spoiledMeal"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "mealMeatstapleveg"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:commonOilTubers")
		{
			Name = "Common oil tubers",
			SummaryDescription = "Potato-like vegetable with high fat content.",
			Description = "\n SURVIVAL GUIDE NOTES\n A valuable source of nutrition that grows in the firegrass biome. Hard to spot but usually found underneath firegrass. Only requires cooking to be made edible.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richStaple"],
					FoodTags = new string[1] { "inedibleVegi" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry",
				DegradesTo = "item:rottenStaple"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "staple"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:spottedOilTubers")
		{
			Name = "Spotted oil tubers",
			SummaryDescription = "This variant of the oil tuber is inedible by humans until we process it with a special enzyme",
			Description = "\n SURVIVAL GUIDE NOTES\n Grows in the firegrass biome. Hard to spot but usually found underneath firegrass. Before we can eat it, it requires processing with a specially engineered enzyme. Such an enzyme could be made with a standard field lab.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richStaple"],
					FoodTags = new string[1] { "inedibleVegi" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry",
				DegradesTo = "item:rottenStaple"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "staple"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:bakedCommonOilTubers")
		{
			Name = "Baked common oil tubers",
			SummaryDescription = "Very nourishing. Cooked among the coals.",
			Description = "The taste is described as a cross between hazelnuts and mushrooms",
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richStaple"],
					IsMeal = true,
					FoodTags = new string[1] { "edibleVegi" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "cooked food",
				DegradesTo = "item:spoiledMeal"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "mealBeige"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:mashedCommonOilTubers")
		{
			Name = "Mashed oil tubers",
			SummaryDescription = "A single portion of spuds boiled and mashed in a pot.",
			Description = "The cook adds whichever spices are available. This recipe leaves the tubers soft and tasty.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richStaple"],
					IsMeal = true,
					FoodTags = new string[1] { "edibleVegi" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "cooked food",
				DegradesTo = "item:spoiledMeal"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "mealBeige"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:mashedSpottedOilTubers")
		{
			Name = "Spotted oil mash",
			SummaryDescription = "A single portion of spuds boiled and mashed in a pot.",
			Description = "The spotted oil tubers have first been processed with a special enzyme to make them edible. After that, the cook adds whichever spices are available. This recipe leaves the tubers soft and tasty.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["richStaple"],
					IsMeal = true,
					FoodTags = new string[1] { "edibleVegi" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "cooked food",
				DegradesTo = "item:spoiledMeal"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "mealBeige"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:glassyCreeperPods")
		{
			Name = "Glassy creeper pods",
			SummaryDescription = "Seeds of the Glassy creeper plant",
			Description = "These transparent, pea-like seeds are a modest source of nutrition when cooked. They should be fairly easy to grow as crops on a suitable patch of soil because the glassy creeper will vigorously spread on uncontested ground",
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["poorStaple"],
					FoodTags = new string[1] { "inedibleVegi" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "perishable",
				DegradesTo = "item:rottenStaple"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "vegetables"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:glassyPorridge")
		{
			Name = "Glassy porridge",
			SummaryDescription = "Oddly transparent dish made from creeper pods.",
			Description = "To realize its potential, this rather insipid dish needs to be accompanied by vegetables or meats of the cook's choosing.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["poorStaple"],
					IsMeal = true,
					FoodTags = new string[1] { "edibleVegi" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "cooked food",
				DegradesTo = "item:spoiledMeal"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "mealBeige"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:hardtack")
		{
			Name = "Hardtack",
			SummaryDescription = "Very hard crackers that can keep for years",
			Description = "All moisture has been baked from this bread, resulting in a very long shelf life if stored dry. They consist of flour made from glassy creeper pods mixed with water",
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["poorStaple"],
					IsMeal = true,
					FoodTags = new string[1] { "edibleVegi" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry",
				DegradesTo = "item:spoiledMeal"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "staple"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:crystalBerries")
		{
			Name = "Crystal berries",
			SummaryDescription = "Small, hollow fruits with a crisp transparent shell",
			Description = "Very high energy content. Should not be eaten directly because of the sharp pieces from its shell. The crystal shrub is a fairly robust plant and the berries could be grown as crops on a suitable plot of land.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["highEnergy"],
					FoodTags = new string[1] { "inedibleVegi" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "perishable",
				DegradesTo = "item:rottenVegetables"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "vegetables"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:powderedCrystalBerries")
		{
			Name = "Powdered crystal berries",
			SummaryDescription = "Extremely sweet white powder",
			Description = "By grounding the crystal berries we get a white powder many times sweeter than sugar.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["highEnergy"],
					IsMeal = true,
					FoodTags = new string[1] { "edibleVegi" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "perishable",
				DegradesTo = "item:organicMatter"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "mealBlackzpacho"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:minnowsLive")
		{
			Name = "Minnows",
			SummaryDescription = "Small fish",
			Description = "Could be used in a soup",
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["mediumMeat"],
					FoodTags = new string[1] { "smallRawMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "raw seafood",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "brainsPink"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:rottenMeat")
		{
			Name = "Rotten meat",
			SummaryDescription = "Meat that is decomposing and inedible by humans.",
			Description = "Depending on conditions, it will continue to decompose into organic matter",
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["poorMeat"],
					FoodTags = new string[1] { "rottenMeat" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "decomposedFood",
				DegradesTo = "item:organicMatter"
			},
			CategoryKey = "waste",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "mushSmallReddish"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:rottenVegetables")
		{
			Name = "Rotten vegetables",
			SummaryDescription = "Vegetables that are decomposing and inedible by humans.",
			Description = "Depending on conditions, it will continue to decompose into organic matter.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["poorVegetables"],
					FoodTags = new string[1] { "inedibleVegi" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "decomposedFood",
				DegradesTo = "item:organicMatter"
			},
			CategoryKey = "waste",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "mushSmallGreenish"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:rottenStaple")
		{
			Name = "Moldy staple food",
			SummaryDescription = "Staple vegetable food that is decomposing and inedible by humans.",
			Description = "Staples (such as roots, tubers, seeds and grains) normally keep for some time but this is past its expiry date. Depending on conditions, it will continue to decompose into organic matter.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["poorVegetables"],
					FoodTags = new string[1] { "inedibleVegi" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "decomposedFood",
				DegradesTo = "item:organicMatter"
			},
			CategoryKey = "waste",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "mushSmallYellowish"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:spoiledMeal")
		{
			Name = "Spoiled meal",
			SummaryDescription = "The disgusting remnants of a meal that was never eaten.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.15f,
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["lowWeightBalancedMeal"],
					FoodTags = new string[1] { "spoiledMeal" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "decomposedFood",
				DegradesTo = "item:organicMatter"
			},
			CategoryKey = "waste",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "mealSpoiled"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:crystalWine")
		{
			Name = "Crystal wine",
			SummaryDescription = "Fruit wine made from fermented crystal berries. Could be distilled into brandy",
			Description = "Around 8% alcoholic content. Despite its elegant name, this is a rather plain beverage. Its main advantage is that it's easy to make once you have the necessary yeast.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f,
				RequiredStorageTags = new string[1] { "storageTagLiquidContainerClosedNoHeat" },
				FoodType = new FoodType
				{
					IsMeal = true,
					IsDrunk = true,
					Effects = new string[1] { "cheapAlcohol" },
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["lowStimulant"],
					FoodTags = new string[1] { "alcoholicBeverage" }
				}
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic",
				Area = RatingTypes.Comfort
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "pickledFood",
				DegradesTo = "item:rottenStaple"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "vegetables"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:crystalBrandy")
		{
			Name = "Crystal brandy",
			SummaryDescription = "High-proof (45%) spirit made from crystal berries",
			Description = "A strong alcoholic beverage made by distillation of crystal wine.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				RequiredStorageTags = new string[1] { "storageTagLiquidContainerClosedNoHeat" },
				FoodType = new FoodType
				{
					IsMeal = true,
					IsDrunk = true,
					Effects = new string[1] { "improvedAlcohol" },
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["lowStimulant"],
					FoodTags = new string[1] { "alcoholicBeverage" }
				}
			},
			TierOrArea = new TierOrArea
			{
				Tier = "medium",
				Area = RatingTypes.Comfort
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "pickledFood",
				DegradesTo = "item:spoiledMeal"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "greyPouch"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:simCoffee")
		{
			Name = "Hot sim coffee",
			SummaryDescription = "Hot coffee made from simulated beans.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				FoodType = new FoodType
				{
					IsMeal = true,
					IsDrunk = true,
					Effects = new string[1] { "caffeine" },
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["lowStimulant"],
					FoodTags = new string[1] { "coffee" }
				}
			},
			TierOrArea = new TierOrArea
			{
				Tier = "advanced",
				Area = RatingTypes.Comfort
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "hotFood",
				DegradesTo = "item:simCoffeeCold"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "greyPouch"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:simCoffeeCold")
		{
			Name = "Cold sim coffee",
			SummaryDescription = "Coffee that has gone cold. Made from simulated beans.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				FoodType = new FoodType
				{
					IsMeal = true,
					IsDrunk = true,
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["lowStimulant"],
					FoodTags = new string[1] { "coffee" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "cooked food",
				DegradesTo = "item:spoiledMeal"
			},
			CategoryKey = "preparedFood",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "greyPouch"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:simCoffeeBeans")
		{
			Name = "Sim coffee beans",
			SummaryDescription = "Coffee beans that were chemically produced rather than grown. Not as good as the real thing.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f
			},
			TierOrArea = new TierOrArea
			{
				Tier = "advanced",
				Area = RatingTypes.Comfort
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry",
				DegradesTo = "item:organicMatter"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "greyPouch"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:spottedOilTuberEnzyme")
		{
			Name = "Spotted oil tuber enzyme",
			SummaryDescription = "Enzyme for making spotted oil tuber edible",
			Description = "This enzyme can be made using our field lab. First, a sample of the spotted oil tuber is analyzed and then we can synthesize a batch of enzymes to add to the oil tubers during the cooking process.\n Note that the enzyme needs to be used for cooking immediately since it quickly degrades and becomes useless.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.01f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "enzyme",
				DegradesTo = "item:degradedEnzyme"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "greyPouch"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:blackpulpEnzyme")
		{
			Name = "Blackpulp enzyme",
			SummaryDescription = "Enzyme for making the blackpulp fruit edible",
			Description = "This enzyme can be made using our field lab. First, a sample of the blackpulp is analyzed and then we can synthesize a batch of enzymes to add to the blackpulp during the cooking process.\n Note that the enzyme needs to be used for cooking immediately since it quickly degrades and becomes useless.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.01f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "enzyme",
				DegradesTo = "item:degradedEnzyme"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "greyPouch"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:hexapineLeavesEnzyme")
		{
			Name = "Hexapine enzyme",
			SummaryDescription = "Enzyme for making the hexapine leaves edible",
			Description = "This enzyme can be made using our field lab. First, a sample of the hexapine leaf is analyzed and then we can synthesize a batch of enzymes to add to the hexapine leaves during the cooking process.\n Be aware that the enzyme needs to be used for cooking immediately since it quickly degrades and becomes useless.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.01f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "enzyme",
				DegradesTo = "item:degradedEnzyme"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "greyPouch"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:degradedEnzyme")
		{
			Name = "Degraded enzyme",
			SummaryDescription = "The enzyme is past its expiry date and is no longer of use.",
			Description = "Enzymes need to be used for cooking immediately since they quickly degrade and becomes useless.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.01f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "decomposedFood",
				DegradesTo = "item:organicMatter"
			},
			CategoryKey = "waste",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "mushSmallGreyish"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:twinklerPheromone")
		{
			Name = "Twinkler pheromone",
			SummaryDescription = "Pheromone extracted from twinkler, to be used in a rat repellent contraption",
			Description = "From a dead twinkler, we can extract a chemical compound which the twinkler uses to signal with. Using the field lab, we can enhance its properties, making it sufficiently strong to ward off binal rats (and other animals) when placed in a special contraption.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.01f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "perishable",
				DegradesTo = "item:degradedChemical"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "greyPouch"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:degradedChemical")
		{
			Name = "Degraded chemical",
			SummaryDescription = "This chemical compound is past its expiry date and is no longer of use.",
			Description = "Some chemicals have a limited shelf life and some need to be stored under very specific conditions.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.01f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "decomposedFood",
				DegradesTo = "item:organicMatter"
			},
			CategoryKey = "waste",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "mushSmallGreyish"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:blackPowder")
		{
			Name = "Black powder",
			SummaryDescription = "Simple chemical explosive",
			Description = "Because it is simple to make, this ancient type of explosive can be useful when no other options are available.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.15f
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic",
				Area = RatingTypes.Security
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "wetDecay",
				DegradesTo = "item:decayedBlackPowder"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "blackPowder"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:decayedBlackPowder")
		{
			Name = "Decayed black powder",
			SummaryDescription = "No longer useful as an explosive",
			Description = "This blackpowder has decayed because it was not stored in dry conditions.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.15f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "dirt"
			},
			CategoryKey = "waste",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "blackPowder"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:goldBullet")
		{
			Name = "Bullets (rifle projectiles)",
			SummaryDescription = "Rifled gold bullets. Part of the ammunition for a rifled firearm",
			Description = "Must be put in bag and carried together with a pouch of black powder. Gold is plentiful and can be used as a substitute for lead projectiles because it has an even higher density and malleability. The bullet will fit snugly in the grooves of a rifled barrel, making it spin during flight which increases accuracy. When making these bullets, precision is needed and they must be cast using a special bullet mold.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f
			},
			TierOrArea = new TierOrArea
			{
				Tier = "medium",
				Area = RatingTypes.Security
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "dirt"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "canister"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:blunderbussBalls")
		{
			Name = "Ball shot (gold)",
			SummaryDescription = "Uneven gold projectiles. Part of the ammunition for a musket/musketoon",
			Description = "Can only be used with smooth bore firearms. Is put in a bag and carried together with a pouch of black powder. Because little precision is needed for manufacturing this ammunition, they can be made from gold nuggets that are simply heated and hammered in a smithy, without casting.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic",
				Area = RatingTypes.Security
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "dirt"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "canister"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:saltpeter")
		{
			Name = "Saltpeter powder",
			SummaryDescription = "Saltpeter refined into a fine white powder",
			Description = "The mineral can be used as a component in black powder.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.25f
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "dirt"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "saltpeterPowder"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:gunBarrelUnbored")
		{
			Name = "Gun barrel (unbored)",
			SummaryDescription = "Unfinished gun barrel made from wrought iron and steel",
			Description = "A long, irregular tube which needs to undergo boring before it can be used in a firearm.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.07f
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic",
				Area = RatingTypes.Security
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "gunBarrel"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:gunBarrelSmoothLong")
		{
			Name = "Gun barrel (smooth, long)",
			SummaryDescription = "For use in a smooth bore firearm or modified further for use in other gun types.",
			Description = "Can be used in a musket. Can also be rifled for use in a rifle. If shortened and with a larger caliber bored, it can be used in a shotgun-type weapon.",
			TierOrArea = new TierOrArea
			{
				Tier = "basic",
				Area = RatingTypes.Security
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.07f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "gunBarrel"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:gunBarrelSmoothShort")
		{
			Name = "Gun barrel (smooth, short)",
			SummaryDescription = "For use in a short, smooth bore firearm such as a shotgun",
			Description = "The short barrel is useful for hunting in bush where a long barrel can be impractical.",
			TierOrArea = new TierOrArea
			{
				Tier = "basic",
				Area = RatingTypes.Security
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.07f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "gunBarrel"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:gunBarrelRifled")
		{
			Name = "Gun barrel (rifled)",
			SummaryDescription = "Gun barrel for use in a rifled firearm",
			Description = "The grooves inside the barrel will cause a rifle bullet to spin, stabilizing its flight and greatly increase its accuracy and range compared to a non-rifled barrel.",
			TierOrArea = new TierOrArea
			{
				Tier = "medium",
				Area = RatingTypes.Security
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.07f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "gunBarrel"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:gunStock")
		{
			Name = "Gun stock",
			SummaryDescription = "The part of a firearm which is held against the shoulder",
			Description = "Versatile enough that it can be used for different gun types.",
			TierOrArea = new TierOrArea
			{
				Tier = "basic",
				Area = RatingTypes.Security
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.04f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "bushcraftComponentsSmall"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:anvil")
		{
			Name = "Anvil",
			SummaryDescription = "Iron tool used in metalworking as a surface for hammering",
			Description = "The anvil itself is made by forging together billets of wrought iron.",
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.9f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "dirt"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "blackBox"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:barClamps")
		{
			Name = "Bar clamps (simple)",
			SummaryDescription = "Iron clamps for securing an object so that it can be worked on",
			Description = "When a threaded rod is unavailable for making a vise, these tools can be used for holding an item in place: Curved and straight iron rods that work together with a notched bar.",
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.14f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "ironTools"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:metalLatheComponents")
		{
			Name = "Metal lathe components",
			SummaryDescription = "The metal parts of a lathe such as the frame, bed, saddle, wheels, gears, screws etc",
			Description = "Carefully handmade from cast gold - all parts fit together perfectly after some modifications are made.",
			TierOrArea = new TierOrArea
			{
				Tier = "medium"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.5f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "castComponents"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:humanPowerUnitComponents")
		{
			Name = "Human power unit components",
			SummaryDescription = "The metal parts for the human power unit. Pedals, cranks, chain drive",
			Description = "Primarily made from cast gold.",
			TierOrArea = new TierOrArea
			{
				Tier = "medium"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.75f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "castComponents"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:humanPowerUnit")
		{
			Name = "Human power unit",
			SummaryDescription = "Pedal device which enables the worker to power a machine tool using his leg muscles",
			Description = "Shares some similarities with a stationary bicycle: Consists of pedals, cranks and a chain drive - mounted on a sturdy frame.",
			TierOrArea = new TierOrArea
			{
				Tier = "medium"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.75f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:humanPowerUnitComponents", 1 },
					{ "item:sticks", 2 }
				}
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "humanPowerUnit"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:extrusionMachineComponents")
		{
			Name = "Extrusion machine components",
			SummaryDescription = "The metal parts for an extrusion machine.",
			Description = "Primarily made from cast gold. All parts fit together perfectly after some modifications are made.",
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 1f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "castComponents"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:loomComponents")
		{
			Name = "Loom components",
			SummaryDescription = "Wooden parts for a loom.",
			Description = "Careful carpentry has produced these components which can be assembled into a loom.",
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 1f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "bushcraftComponents"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:goldSheet")
		{
			Name = "Gold sheet",
			SummaryDescription = "Thin plates of gold which can be shaped into tubes, pipes and other objects",
			Description = "Sharing some of the properties of copper sheet. A skilled blacksmith or metalworker can bend, hammer or cut this metal in countless ways.",
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "dirt"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "castComponents"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:stillComponents")
		{
			Name = "Still components",
			SummaryDescription = "Boiler and pipes made from gold plate: Main components of a still",
			Description = "Carefully handmade from cast gold - a still can easily be put together from these parts.",
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.9f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "castComponents"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:gaskets")
		{
			Name = "Rubber parts",
			SummaryDescription = "Rubber components for use in machines and engines",
			Description = "Tough, resistant products such as gaskets used for sealing a machine. Manufactured from marshcot sap which is a natural polymer superior to the natural rubber from Earth. The sap is first cleaned, then coagulated with a mild acid and left to dry. Then, it is heated and shaped while mixed with sulfur which causes the rubber to toughen (vulcanize).",
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "leatherPouch"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:bogOre")
		{
			Name = "Bog ore",
			SummaryDescription = "Lumps of iron ore, easily harvested. Found in bogs, near basalt rocks.",
			Description = "The ore appears as small lumps in bogs where water flows from nearby iron-rich mountains. The deposits are easy to harvest and can serve as a resource for small-scale metalworking",
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.75f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "dirt"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "ironOre"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:scandiumOre")
		{
			Name = "Scandium ore",
			SummaryDescription = "These minerals have an unusually high concentration of the chemical element scandium",
			Description = "The high concentration of scandium in this ore makes it possible to refine the metal on site when using an advanced refiner installation.",
			TierOrArea = new TierOrArea
			{
				Tier = "advanced"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.75f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "dirt"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "soilWhite"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:terbiumOre")
		{
			Name = "Terbium ore",
			SummaryDescription = "The chemical element terbium is present in high amounts in these minerals",
			Description = "The high concentration of terbium makes it easy to refine the metal when using an advanced refiner installation.",
			TierOrArea = new TierOrArea
			{
				Tier = "advanced"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.75f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "dirt"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "soilGrey"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:goldOre")
		{
			Name = "Gold ore",
			SummaryDescription = "Gold nuggets are common on this planet and are easily harvested",
			Description = "Big lumps can often be found in loose mountain soil and sediments. They have a high purity but usually require smelting to remove impurities before the gold can be cast into objects. However, the nuggets can also be shaped into simple items through forging at lower temperatures.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.75f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "dirt"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "goldOre"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:gold")
		{
			Name = "Gold",
			SummaryDescription = "Rods of gold",
			Description = "Gold is very common on Antheia so its physical properties can be used for any purpose without fretting about price: It is very malleable, has high density and corrosion resistance. This gold has been smelted into rods of high purity and can be used to cast high quality objects.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "dirt"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "goldSticks"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:roughBloomIron")
		{
			Name = "Bloom iron",
			SummaryDescription = "Clump of iron and slag, a product of primitive iron smelting",
			Description = "An early step in primitive metalworking, this requires further work by the blacksmith before the iron is pure enough to be used.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "dirt"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "ironBloom"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:wroughtIron")
		{
			Name = "Wrought iron",
			SummaryDescription = "Iron rods that have been shaped on the anvil by a blacksmith",
			Description = "These iron products are ready to be shaped into their final form such as simple iron tools.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "dirt"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "ironRods"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:scandium")
		{
			Name = "Scandium (refined)",
			SummaryDescription = "Soft, silvery metal with a yellow shade. Only used for high tech products",
			Description = "Scandium is called a rare-earth metal. This chemical element is required in the construction of the electromagnetic shields known as Project CANOPY.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "dirt"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "advanced"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "lightOrangePowder"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:terbium")
		{
			Name = "Terbium (refined)",
			SummaryDescription = "Silvery-white metal which is very soft. Only used for high tech products",
			Description = "Terbium is called a rare-earth metal. This chemical element is required in the construction of the electromagnetic shields known as Project CANOPY.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "dirt"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "advanced"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "lightGreyBluePowder"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:blisterSteel")
		{
			Name = "Blister steel",
			SummaryDescription = "Steel rods made from wrought iron using a carburization process",
			Description = "Named for the characteristic small lumps that comes from the carburization method, where the wrought iron is heat treated together with charcoal in a kiln for some time.  This increases the carbon content of the iron, creating blister steel. It can then be further worked with a hammer to produce steel products.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "dirt"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "steelRods"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:guano")
		{
			Name = "Guano pile",
			SummaryDescription = "Excrements from animals and birds that live in caves or arid areas",
			Description = "Because the manure has not been subjected to rain, it has a high content of phosphate, nitrogen and potassium which makes it useful as a fertilizer. Can also be used for extracting chemical compounds such as saltpeter",
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.5f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "dirt"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "soilGrey"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:saltpeterSolution")
		{
			Name = "Saltpeter solution",
			SummaryDescription = "Saltpeter which has been fully dissolved in water",
			Description = "The saltpeter is ready to be refined by boiling.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.6f,
				RequiredStorageTags = new string[1] { "storageTagLiquidContainerNoHeat" }
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "dirt"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "mush"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:marshcotSap")
		{
			Name = "Marshcot sap",
			SummaryDescription = "Fluid extracted from marshcot plants. Can be turned into rubber",
			Description = "This emulsion has many of the same qualities found in natural rubber from Earth.",
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.06f,
				RequiredStorageTags = new string[1] { "storageTagLiquidContainerNoHeat" }
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "mush"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:sulfurPowder")
		{
			Name = "Sulfur powder",
			SummaryDescription = "Small pile of refined sulfur",
			Description = "N/A",
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.25f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "dirt"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "sulfurPowder"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:clay")
		{
			Name = "Clay",
			SummaryDescription = "Fine-grained soil material useful for ceramics and simple construction",
			Description = "N/A",
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.75f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "dirt"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "soil_s"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:wetMudBrick")
		{
			Name = "Mudbricks (wet)",
			SummaryDescription = "The mudbricks are still wet. They dry slowly on their own OR quickly in a kiln",
			Description = " The bricks are a simple and cheap construction material for buildings. Can either be dried slowly in the sun or quickly in a kiln and can then be used for construction. Made of clay, sand and some plant material.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.75f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "sun drying",
				DegradesTo = "item:solidMudBrick"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "mudBricksWet"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:solidMudBrick")
		{
			Name = "Mudbricks",
			SummaryDescription = "Brick made of clay, sand and some plant material. They have dried and are ready for use in construction.",
			Description = "The bricks are a simple and cheap construction material for buildings.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.75f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "dirt",
				DegradesTo = "item:clay"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "mudBricksDry"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:unfinishedFirebricks")
		{
			Name = "Firebricks (unfinished)",
			SummaryDescription = "Unfinished bricks made of a special mix of clay, designed to withstand temperatures up to 1100 C",
			Description = "When the bricks have been fired in a kiln they can be used for structures that must withstand the high heat involved in melting metals such as gold. The material consists of a carefully selected mix of silicon sand and kaolin clay.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.75f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "dirt",
				DegradesTo = "item:clay"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "mudBricksWet"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:firebricks")
		{
			Name = "Firebricks",
			SummaryDescription = "Bricks made of a special mix of clay, designed to withstand temperatures up to 1100 C",
			Description = "Used for structures that must withstand the high heat involved in melting metals such as gold. The material consists of a carefully selected mix of silicon sand and kaolin clay.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.75f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "dirt",
				DegradesTo = "item:clay"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "mudBricksDry"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:salt")
		{
			Name = "Salt pile",
			SummaryDescription = "Salt is vital for nutrition and many other purposes",
			Description = "Salt is useful for preserving food, since most of the bacteria that cause decomposition of organic matter are sensitive to high salt concentrations, just like on Earth.",
			TierOrArea = new TierOrArea
			{
				Tier = "basic",
				Area = RatingTypes.Food
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.125f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "dirt"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "saltpeterPowder"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:paint")
		{
			Name = "Paint",
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.3f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "stored dry"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "drum"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:acetylene")
		{
			Name = "Acetylene canister",
			SummaryDescription = "Hydrocarbon input material for the molecular assembler",
			Description = "This simple hydrogen and carbon molecule is the main input material for creating diamondoid structures using the molecular assembler.",
			TierOrArea = new TierOrArea
			{
				Tier = "advanced"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.15f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "canister"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:ironCanister")
		{
			Name = "Canister (iron)",
			SummaryDescription = "Iron input material for the molecular assembler",
			Description = "Iron is needed for some products made by the molecular assembler.",
			TierOrArea = new TierOrArea
			{
				Tier = "advanced"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.15f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "canister"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:plastCrete")
		{
			Name = "PlastCrete",
			SummaryDescription = "Binder for a concrete-like building material.",
			Description = "Combines with water and aggregate to form a building material that is strong and light. Contains specially designed particles that combine into a strong lattice, reinforcing the structure.",
			TierOrArea = new TierOrArea
			{
				Tier = "advanced"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.5f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "cement"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:diamondGlass")
		{
			Name = "Diamond glass",
			SummaryDescription = "Extremely durable and lightweight transparent material",
			Description = "Made from carbon with a molecular assembler.",
			TierOrArea = new TierOrArea
			{
				Tier = "advanced"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.5f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "neverDegrades"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "scrapMetal"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:strippedHull")
		{
			Name = "Stripped aircraft hull",
			SummaryDescription = "We've stripped the hull part of the aircraft wreck for any useful materials",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			Icon = "typeIcon_strippedHull",
			ItemType = new ItemType
			{
				MaximumBulk = 5f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "strippedHull"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:strippedTail")
		{
			Name = "Stripped aircraft tail",
			SummaryDescription = "We've stripped the tail part of the aircraft wreck for any useful materials",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			Icon = "typeIcon_strippedTail",
			ItemType = new ItemType
			{
				MaximumBulk = 4f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "strippedTail"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:strippedEngineSide")
		{
			Name = "Stripped aircraft rotor",
			SummaryDescription = "We've stripped this aircraft rotor for any useful materials",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			Icon = "typeIcon_strippedEngineSide",
			ItemType = new ItemType
			{
				MaximumBulk = 3f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "strippedEngineSide"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:strippedEngineTop")
		{
			Name = "Stripped aircraft rotor",
			SummaryDescription = "We've stripped this aircraft rotor for any useful materials",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			Icon = "typeIcon_strippedEngineTop",
			ItemType = new ItemType
			{
				MaximumBulk = 3f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "strippedEngineTop"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:propellerDome")
		{
			Name = "Prop spinner",
			SummaryDescription = "An aluminum dome from the Skimmer's propeller hub",
			Description = "A camp member has suggested using this component as a cooking pot.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.15f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0.8f,
				DegradeType = "stored dry"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "scrapMetal"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:organicMatter")
		{
			Name = "Organic matter",
			SummaryDescription = "Decomposed plant or animal material",
			Description = "Organic compounds that have come from the remains of plants and animals and their waste products.\n It will continue to decompose and eventually enter the soil as humus.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ItemType = new ItemType
			{
				HasNoMaximumBulk = true
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "biowaste"
			},
			CategoryKey = "waste",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "mushSmallGreyish"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:organicFertilizer")
		{
			Name = "Compost fertilizer",
			SummaryDescription = "Organic fertilizer for a farm plot",
			Description = "Made from decomposed plant and animal matter",
			TierOrArea = new TierOrArea
			{
				Tier = "medium"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "biowaste"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "cement"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:guanoFertilizer")
		{
			Name = "Guano fertilizer",
			SummaryDescription = "Organic fertilizer for a farm plot",
			Description = "Made from natural guano deposits, this is a more concentrated fertilizer than compost.",
			TierOrArea = new TierOrArea
			{
				Tier = "medium"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.25f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "dirt"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "cement"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:firegrassSod")
		{
			Name = "Firegrass sod",
			SummaryDescription = "Pieces of firegrass turf",
			Description = "Firegrass grows thick with entangled roots. It is possible to cut pieces and use them as bricks for simple structures.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.75f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "dirt",
				DegradesTo = "item:organicMatter"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "firegrassSod"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:sulfurBlocks")
		{
			Name = "Sulfur blocks",
			SummaryDescription = "Pieces of sulfur crystals",
			Description = "Gathered from vulcanic deposits. This sulfur has a relatively high purity but could be refined.",
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.75f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "dirt"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "sulfurOre"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:sulfurSmokeBomb")
		{
			Name = "Sulfur smoke bomb",
			SummaryDescription = "Will burn and release noxious gas",
			Description = "This smoke bomb will release large amounts of sulfur dioxide which prevents respiration in most organisms. Only effective in a closed space. Might be able to clear out a quadite nest...let's see what happens.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.5f
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic",
				Area = RatingTypes.Security
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "cement"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:varmintBomb")
		{
			Name = "Varmint bomb",
			SummaryDescription = "Black powder bomb useful for destroying animal tunnels",
			Description = "Has sufficent explosive power to collapse the entrance area of an underground nest",
			ItemType = new ItemType
			{
				MaximumBulk = 0.08f
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic",
				Area = RatingTypes.Security
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "canister"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:scrapMetal")
		{
			Name = "Scrap metal",
			SummaryDescription = "Pieces of aluminum and carbon from aircraft hull",
			Description = "Broken-off pieces from a scrapped aircraft / vehicle. Mostly consist of a hard and strong carbon structure covered with a thin layer of aluminum.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.9f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "dirt"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "scrapMetal"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:panelScraps")
		{
			Name = "Panel scraps",
			SummaryDescription = "Thermoplastics/composite panels from aircraft/vehicle interior",
			Description = "Pieces of interior panels, ceiling and floor stripped from the inside of an aircraft or vehicle. Made of composite materials and thermoplastics.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.75f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "scrapMetal"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:inactivatedFoodCoolerUnit")
		{
			Name = "Air condition unit (inactivated)",
			SummaryDescription = "Air condition unit salvaged from an aircraft",
			Description = "Still works. We could place it in a food cache to keep temperature at 5 degrees Celsius. Would have a battery life of 2-5 months depending on environment.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.15f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "battery"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:activatedFoodCoolerUnit")
		{
			Name = "Air condition unit (activated)",
			SummaryDescription = "Air condition unit salvaged from an aircraft",
			Description = "The device is activated and will keep the surrounding temperature at 5 degrees Celsius. Will run out of battery in 2-5 months depending on environment.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.15f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "adequateConstruction"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "battery"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:seatCushions")
		{
			Name = "Seat cushions",
			SummaryDescription = "Cushions from aircraft/vehicle seats",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.75f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "cloth"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:textile")
		{
			Name = "Textile",
			SummaryDescription = "Piece of woven fabric",
			Description = "A sturdy versatile fabric suited for outdoor and indoor use.",
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.08f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "cloth"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:fiber")
		{
			Name = "Fiber",
			SummaryDescription = "Natural or synthetic fiber material",
			Description = "This material can be spun and woven into textile",
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.08f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "cloth"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:cotton")
		{
			Name = "Cotton",
			SummaryDescription = "Natural fiber material which grows in a ball around the cottonseed",
			Description = "The cotton plant was brought from Earth but later modified for a new climate by the first pioneers. The cotton fibers can be spun and woven into textile or string. The seeds are used for growing the next crop.",
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.2f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "cotton"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:sticks")
		{
			Name = "Sticks",
			SummaryDescription = "Various treelimbs and pieces of wood",
			Description = "This wood is useful for constructing simple tools and shelter.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.3f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "firewood"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:flintRough")
		{
			Name = "Flint",
			SummaryDescription = "A pile of flint of various sizes and shapes",
			Description = "Flint is a stone found near chalk or limestone which is easy to shape for use in simple tools",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.15f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "dirt"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "flint"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:smallTent")
		{
			Name = "Small tent",
			SummaryDescription = "Tent that offers good protection from wind and weather",
			Description = "Part of the Tau Ceti survival kit. Its smart fabric that can stabilize temperature makes the tent comfortable in both warm and cold conditions.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.25f
			},
			TierOrArea = new TierOrArea
			{
				Tier = "advanced",
				Area = RatingTypes.Comfort
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tarp"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:octagonalTent")
		{
			Name = "Octagonal tent",
			SummaryDescription = "Tent that offers good protection from wind and weather",
			Description = "Part of the Tau Ceti survival kit. Its smart fabric that can stabilize temperature makes the tent comfortable in both warm and cold conditions.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.5f
			},
			TierOrArea = new TierOrArea
			{
				Tier = "advanced",
				Area = RatingTypes.Comfort
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tarp"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:domeTent")
		{
			Name = "Dome tent",
			SummaryDescription = "Tent that offers good protection from wind and weather",
			Description = "Part of the Tau Ceti survival kit. Its smart fabric that can stabilize temperature makes the tent comfortable in both warm and cold conditions.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.5f
			},
			TierOrArea = new TierOrArea
			{
				Tier = "advanced",
				Area = RatingTypes.Comfort
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tarp"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:thermalTarp")
		{
			Name = "Thermal tarp",
			SummaryDescription = "Sophisticated textile that provides protection from wind and weather",
			Description = "Part of the Tau Ceti survival kit. Primarily intended for use in improvised shelters. Its smart fabric that can stabilize temperature makes the tarp useful in both warm and cold conditions.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.15f
			},
			TierOrArea = new TierOrArea
			{
				Tier = "advanced",
				Area = RatingTypes.Comfort
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tarp"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:fishTrapBasket")
		{
			Name = "Basket fish trap",
			SummaryDescription = "A simple portable fish trap designed for catching the 'carbon tail'",
			Description = "This bottle shaped wicker trap can catch the fish species 'carbon tail' if placed in a suitable spot near its fresh water habitat.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.55f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "fishTrapCylinderSmall"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:fishTrapHoopNet")
		{
			Name = "Hoop net",
			SummaryDescription = "An good quality fish trap made from hoops and netting",
			Description = "This cylindrical trap is efficient at catching the fish species 'carbon tail' if placed in a suitable spot near its fresh water habitat.",
			TierOrArea = new TierOrArea
			{
				Tier = "basic",
				Area = RatingTypes.Food
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.65f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "fishTrapHoopNetItem"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:fishingNet")
		{
			Name = "Fishing net",
			SummaryDescription = "A cotton mesh used for fish traps. Treated with a resin",
			Description = "An experienced fisher can weave a good quality cotton net with a few simple tools. To protect the net from rotting it is treated with a resin.",
			TierOrArea = new TierOrArea
			{
				Tier = "basic",
				Area = RatingTypes.Food
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.3f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "fishingNet"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:daysheenLeaves")
		{
			Name = "Daysheen leaves",
			SummaryDescription = "Stiff, shiny, brightly coloured leaves",
			Description = "The leaves naturally form a cone. We can take advantage of this property when building a shelter of such a shape.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.3f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "proneToInfestation",
				DegradesTo = "item:infestedLeavesRemains"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "daysheenLeaves"
						}
					}
				}
			}
		});
		EntityType entityType = new EntityType("item:charcoal")
		{
			Name = "Charcoal",
			SummaryDescription = "A traditional fuel type made from heat treated organic matter",
			Description = "Charcoal can provide a more intense heat than firewood and is often required for primitive metalworking."
		};
		EntityType entityType2 = entityType;
		ItemType itemType = new ItemType
		{
			MaximumBulk = 0.3f
		};
		ItemType itemType2 = itemType;
		FuelType fuelType = new FuelType();
		itemType2.FuelType = fuelType;
		entityType2.ItemType = itemType;
		entityType.NonLivingType = new NonLivingType
		{
			Repairability = 0f,
			DegradeType = "stored dry"
		};
		entityType.TierOrArea = new TierOrArea
		{
			Tier = "basic"
		};
		entityType.CategoryKey = "rawMaterials";
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "charcoal"
					}
				}
			}
		};
		listOfEntityTypes.Add(entityType);
		listOfEntityTypes.Add(new EntityType("item:firewood")
		{
			Name = "Firewood",
			SummaryDescription = "Kindling for a fire",
			Description = "Dry plant matter such as twigs, leaves and branches. A basic fuel type required for many types of production.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.3f,
				FuelType = new FuelType
				{
					FuelTags = new string[2] { "fuelForCampfire", "fuelForFieldKitchen" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "firewood"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:wetFirewood")
		{
			Name = "Firewood (wet)",
			SummaryDescription = "Fresh wood that needs to dry before being used as fuel",
			Description = "Can be placed in a woodpile to accelerate drying.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.3f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "sun drying",
				DegradesTo = "item:firewood"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "firewood"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:dryPeat")
		{
			Name = "Peat",
			SummaryDescription = "Fuel. Dry slabs of partially decayed plant matter",
			Description = "A type of fuel which can be extracted in bogs and marshes, locations where trees and therefore firewood is scarce. Its properties are quite similar to firewood.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.3f,
				FuelType = new FuelType
				{
					FuelTags = new string[1] { "fuelForCampfire" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "peatDry"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:wetPeat")
		{
			Name = "Peat (wet)",
			SummaryDescription = "Wet slabs of partially decayed plant matter. After drying, used as fuel",
			Description = "When this has been dried, its properties are quite similar to firewood. Peat can be extracted in locations where trees and firewood is scarce. ",
			ItemType = new ItemType
			{
				MaximumBulk = 0.3f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "dirt"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "peatWet"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:waterCaneStem")
		{
			Name = "Water cane stems",
			SummaryDescription = "Stiff, bamboo-like tubes",
			Description = "Can be useful for making various items that depend on these properties.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.25f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "pole"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:waterCaneLeaves")
		{
			Name = "Water cane leaves",
			SummaryDescription = "Tiny, stiff leaves useful as arrow fletchings",
			Description = "This feather-like material can be carefully applied to the ends of arrows to stabilize their flight.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.02f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "wingweedLeaves"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:giantHollowBud")
		{
			Name = "Bowl-shaped flower bud",
			SummaryDescription = "A young outgrowth from the 'giant hollow' plant",
			Description = "The bud has the size of a bowl and is protected by tough scales. Its size and toughness could make it useful as an improvised food container. It simply needs to be hollowed out.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.2f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "woodenPot"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:shadeleafCanes")
		{
			Name = "Shadeleaf canes",
			SummaryDescription = "Bendy, tough saplings",
			Description = "The elastic limbs of a Shadeleaf tree are useful for construction of dome-shaped shelters and tools that need flexibility.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.2f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "roughCanes"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:shadeleafBowStave")
		{
			Name = "Shadeleaf bow stave",
			SummaryDescription = "A branch well-suited for making a bow",
			Description = "This branch has the necessary properties for making a bow.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "bowStave"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:wingweedLeaves")
		{
			Name = "Wingweed leaves",
			SummaryDescription = "Large soft leaves",
			Description = "Can be used for insulating shelters. However, they are prone to infestation by 'scuttlers' (a type of bug that inhabits the firegrass biome).",
			ItemType = new ItemType
			{
				MaximumBulk = 0.3f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "proneToInfestation",
				DegradesTo = "item:infestedLeavesRemains"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "wingweedLeaves"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:wingweedMat")
		{
			Name = "Wingweed mat",
			SummaryDescription = "Leaf mat for insulating shelters",
			Description = "With some patience, wingweed leaves can be made into mats which will increase the comfort of a shelter. The leaves are treated with a preservative which increases their resistance from bug infestation.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.5f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Comfort
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "wingweedLeaves"
						}
					}
				}
			}
		});
		string text = "mats";
		string description = "The mats increase the comfort of a shelter. The leaves are treated with a preservative which increases their resistance from bug infestation.";
		listOfEntityTypes.Add(new EntityType("item:wingweedMats1People")
		{
			Name = "Wingweed mats (1)",
			SummaryDescription = "Upgrade: 1 leaf mat for shelters",
			Description = description,
			ItemType = new ItemType
			{
				MaximumBulk = 0.5f
			},
			Category = GameData.Instance.AllEntityCategories["upgrades"],
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry",
				SalvageProcess = "salvageWingweedMats1People",
				PartKeys = new SerializableDictionary<string, int> { { "item:wingweedMat", 1 } },
				Repair = "buildingRepair"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Comfort
			},
			Upgrader = new Upgrader
			{
				UpgradeCategories = new string[1] { "mats1People" },
				Effects = new string[1] { text },
				SpriteModifier = StateModifier.Upgrade1
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "wingweedLeaves"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:wingweedMats2People")
		{
			Name = "Wingweed mats (2)",
			SummaryDescription = "Upgrade: 2 leaf mats for shelters",
			Description = description,
			ItemType = new ItemType
			{
				MaximumBulk = 1f
			},
			Category = GameData.Instance.AllEntityCategories["upgrades"],
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry",
				SalvageProcess = "salvageWingweedMats2People",
				PartKeys = new SerializableDictionary<string, int> { { "item:wingweedMat", 2 } },
				Repair = "buildingRepair"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Comfort
			},
			Upgrader = new Upgrader
			{
				UpgradeCategories = new string[1] { "mats2People" },
				Effects = new string[1] { text },
				SpriteModifier = StateModifier.Upgrade1
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "wingweedLeaves"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:wingweedMats3People")
		{
			Name = "Wingweed mats (3)",
			SummaryDescription = "Upgrade: 3 leaf mats for shelters",
			Description = description,
			ItemType = new ItemType
			{
				MaximumBulk = 1.5f
			},
			Category = GameData.Instance.AllEntityCategories["upgrades"],
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry",
				SalvageProcess = "salvageWingweedMats3People",
				PartKeys = new SerializableDictionary<string, int> { { "item:wingweedMat", 3 } },
				Repair = "buildingRepair"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Comfort
			},
			Upgrader = new Upgrader
			{
				UpgradeCategories = new string[2] { "mats3People", "bedsOrMats3People" },
				Effects = new string[1] { text },
				SpriteModifier = StateModifier.Upgrade1
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "wingweedLeaves"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:wingweedMats4People")
		{
			Name = "Wingweed mats (4)",
			SummaryDescription = "Upgrade: 4 leaf mats for shelters",
			Description = description,
			ItemType = new ItemType
			{
				MaximumBulk = 2f
			},
			Category = GameData.Instance.AllEntityCategories["upgrades"],
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry",
				SalvageProcess = "salvageWingweedMats4People",
				PartKeys = new SerializableDictionary<string, int> { { "item:wingweedMat", 4 } },
				Repair = "buildingRepair"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Comfort
			},
			Upgrader = new Upgrader
			{
				UpgradeCategories = new string[2] { "mats4People", "bedsOrMats4People" },
				Effects = new string[1] { text },
				SpriteModifier = StateModifier.Upgrade1
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "wingweedLeaves"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:bedFrame")
		{
			Name = "Bed frame",
			SummaryDescription = "Sturdy wooden bed frame, made from naturally shaped materials",
			Description = "This bed frame was made by exploiting the curves in the spoak tree branches.",
			ItemType = new ItemType
			{
				MaximumBulk = 1f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "equipment"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "medium",
				Area = RatingTypes.Comfort
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "furniture"
						}
					}
				}
			}
		});
		string[] effects = new string[1] { "simpleBed" };
		string description2 = "These beds will provide increased comfort to the residents.";
		listOfEntityTypes.Add(new EntityType("item:beds3People")
		{
			Name = "Beds (3)",
			SummaryDescription = "Upgrade: 3 simple wooden beds for a home",
			Description = description2,
			ThumbnailSmall = "HUD_thumbnail_placeholder",
			ItemType = new ItemType
			{
				MaximumBulk = 4f
			},
			Category = GameData.Instance.AllEntityCategories["upgrades"],
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "equipment",
				SalvageProcess = "salvageBeds3People",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:bedFrame", 3 },
					{ "item:textile", 3 }
				},
				Repair = "buildingRepair"
			},
			Upgrader = new Upgrader
			{
				UpgradeCategories = new string[1] { "bedsOrMats3People" },
				Effects = effects,
				SpriteModifier = StateModifier.Upgrade2
			},
			TierOrArea = new TierOrArea
			{
				Tier = "medium",
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
							AssetName = "furniture"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:beds4People")
		{
			Name = "Beds (4)",
			SummaryDescription = "Upgrade: 4 simple wooden beds for a home",
			Description = description2,
			ThumbnailSmall = "HUD_thumbnail_placeholder",
			ItemType = new ItemType
			{
				MaximumBulk = 4f
			},
			Category = GameData.Instance.AllEntityCategories["upgrades"],
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "equipment",
				SalvageProcess = "salvageBeds4People",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:bedFrame", 4 },
					{ "item:textile", 4 }
				},
				Repair = "buildingRepair"
			},
			Upgrader = new Upgrader
			{
				UpgradeCategories = new string[1] { "bedsOrMats4People" },
				Effects = effects,
				SpriteModifier = StateModifier.Upgrade2
			},
			TierOrArea = new TierOrArea
			{
				Tier = "medium",
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
							AssetName = "furniture"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:furniture")
		{
			Name = "Furniture",
			SummaryDescription = "Compact and functional table, chairs and shelves",
			Description = "This furniture is beautifully made from the best spoak wood.",
			ItemType = new ItemType
			{
				MaximumBulk = 1f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "equipment"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "medium",
				Area = RatingTypes.Comfort
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "furniture"
						}
					}
				}
			}
		});
		string[] effects2 = new string[1] { "furnitureEffect" };
		listOfEntityTypes.Add(new EntityType("item:furniture4People")
		{
			Name = "Furniture (4)",
			SummaryDescription = "Upgrade: Furniture for a small hut with 4 people",
			Description = "Table, chairs and shelves which will provide increased comfort in a small hut",
			ThumbnailSmall = "HUD_thumbnail_placeholder",
			ItemType = new ItemType
			{
				MaximumBulk = 1f
			},
			Category = GameData.Instance.AllEntityCategories["upgrades"],
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "equipment",
				SalvageProcess = "salvageFurniture4People",
				PartKeys = new SerializableDictionary<string, int> { { "item:furniture", 1 } },
				Repair = "buildingRepair"
			},
			Upgrader = new Upgrader
			{
				UpgradeCategories = new string[1] { "furniture4People" },
				Effects = effects2,
				SpriteModifier = StateModifier.Upgrade2
			},
			TierOrArea = new TierOrArea
			{
				Tier = "medium",
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
							AssetName = "furniture"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:textileWorkshopUpgrade")
		{
			Name = "Textile workshop",
			SummaryDescription = "Upgrade: Simple machines for spinning fibers and weaving",
			Description = "Equipped with a spinning wheel which spins thread from natural fibers. Also has a wooden loom which is used to weave cloth from thread.",
			ThumbnailSmall = "HUD_thumbnail_workshopTextile",
			ItemType = new ItemType
			{
				MaximumBulk = 6f
			},
			Category = GameData.Instance.AllEntityCategories["upgrades"],
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "sturdyConstruction",
				SalvageProcess = "salvageTextileWorkshopUpgrade",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:loomComponents", 1 },
					{ "item:waterCaneStem", 4 }
				},
				Repair = "buildingRepair"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			Upgrader = new Upgrader
			{
				UpgradeCategories = new string[1] { "workshop" },
				SpriteModifier = StateModifier.Upgrade3,
				StorageSettings = "textileWorkshopStorage"
			},
			ToolType = new ToolType
			{
				Durability = 1f,
				ToolHandling = ToolHandlingType.Stationary
			},
			ContainerType = new ToolContainerType
			{
				CanTransactWithTags = new string[1] { "humanTransact" },
				ProductionOutputStorageType = new ItemStorageType(2f)
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "boxes"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:metalLatheShopHumanPoweredUpgrade")
		{
			Name = "Machine shop (human powered)",
			SummaryDescription = "Upgrade: A work area for turning and making cylindrical metal shapes",
			Description = "The workshop is built around a lathe: An essential tool for making precision-shaped metal items. The tool is powered by the operator's muscles through a simple gearbox.",
			ThumbnailSmall = "HUD_thumbnail_workshopMachinist",
			ItemType = new ItemType
			{
				MaximumBulk = 6f
			},
			Category = GameData.Instance.AllEntityCategories["upgrades"],
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "sturdyConstruction",
				SalvageProcess = "salvageMetalLatheShopHumanPoweredUpgrade",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:metalLatheComponents", 1 },
					{ "item:humanPowerUnit", 1 }
				},
				Repair = "buildingRepair"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "medium"
			},
			Upgrader = new Upgrader
			{
				UpgradeCategories = new string[1] { "workshop" },
				SpriteModifier = StateModifier.Upgrade4,
				StorageSettings = "metalWorkshopStorage"
			},
			ToolType = new ToolType
			{
				Durability = 1f,
				ToolHandling = ToolHandlingType.Stationary
			},
			ContainerType = new ToolContainerType
			{
				CanTransactWithTags = new string[1] { "humanTransact" },
				ProductionOutputStorageType = new ItemStorageType(2f),
				StorageTags = new string[2] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" }
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "boxes"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:carpenterWorkshopUpgrade")
		{
			Name = "Carpenter's workshop",
			SummaryDescription = "Upgrade: A well-equipped woodworking shop",
			Description = "The workshop is equipped with tools for shaping the wood types that are common here, such as the curving spoak branches and the long straight tubes of the watercane. \nImportant assets are the foot powered spring pole lathe and the workbench.",
			ThumbnailSmall = "HUD_thumbnail_workshopCarpenter",
			ItemType = new ItemType
			{
				MaximumBulk = 2f
			},
			Category = GameData.Instance.AllEntityCategories["upgrades"],
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "sturdyConstruction",
				SalvageProcess = "salvageCarpenterWorkshopUpgrade",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:sticks", 2 },
					{ "item:solidMudBrick", 2 },
					{ "item:barClamps", 1 },
					{ "item:shadeleafCanes", 1 }
				},
				Repair = "buildingRepair"
			},
			Upgrader = new Upgrader
			{
				UpgradeCategories = new string[1] { "workshop" },
				SpriteModifier = StateModifier.Upgrade5,
				StorageSettings = "carpentersWorkshopStorage"
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
			ContainerType = new ToolContainerType
			{
				CanTransactWithTags = new string[1] { "humanTransact" },
				ProductionOutputStorageType = new ItemStorageType(2f)
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "boxes"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:polymerWorkshopUpgrade")
		{
			Name = "Polymer workshop",
			SummaryDescription = "Upgrade: For making plastic and rubber. Extrusion machine, vats and heating apparatus connected to a fireplace",
			Description = "The extrusion machine works by pressing plastic or rubber feedstock through a heated die, thereby forming long, continuous products (tubes and sheets). When using a mould, the machine can also make smaller items.",
			ThumbnailSmall = "HUD_thumbnail_workshopPolymer",
			ItemType = new ItemType
			{
				MaximumBulk = 6f
			},
			Category = GameData.Instance.AllEntityCategories["upgrades"],
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "sturdyConstruction",
				SalvageProcess = "salvagePolymerWorkshopUpgrade",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:extrusionMachineComponents", 1 },
					{ "item:solidMudBrick", 3 }
				},
				Repair = "buildingRepair"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			Upgrader = new Upgrader
			{
				UpgradeCategories = new string[1] { "workshop" },
				SpriteModifier = StateModifier.Upgrade6,
				StorageSettings = "polymerWorkshopStorage"
			},
			ToolType = new ToolType
			{
				Durability = 1f,
				ToolHandling = ToolHandlingType.Stationary,
				PrepareProcess = "lightFireFuelBurning"
			},
			ContainerType = new ToolContainerType
			{
				CanTransactWithTags = new string[1] { "humanTransact" },
				ProductionOutputStorageType = new ItemStorageType(2f),
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
				StorageTags = new string[2] { "storageTagLiquidContainerClosedNoHeat", "storageTagLiquidContainerNoHeat" }
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "boxes"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:simpleStoveUpgrade")
		{
			Name = "Clay stove",
			SummaryDescription = "Upgrade: Large stove and oven made from clay and stones",
			Description = "This stove is moderately effective at cooking and baking big quantities of food.",
			ItemType = new ItemType
			{
				MaximumBulk = 5f
			},
			Category = GameData.Instance.AllEntityCategories["upgrades"],
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "sturdyConstruction",
				SalvageProcess = "salvageSimpleStove",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:solidMudBrick", 1 },
					{ "item:stones", 1 }
				},
				Repair = "buildingRepair"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "medium",
				Area = RatingTypes.Food
			},
			Upgrader = new Upgrader
			{
				UpgradeCategories = new string[1] { "stove" },
				SpriteModifier = StateModifier.UpgradeStove
			},
			ToolType = new ToolType
			{
				ToolHandling = ToolHandlingType.Stationary,
				Durability = 1f,
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
						BurnRatePerDay = 5f
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
							AssetName = "boxes"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:smokeOvenUpgrade")
		{
			Name = "Smoke oven addition",
			SummaryDescription = "Upgrade: Smoke oven built in addition to the cookhouse",
			Description = "A convenient and efficient structure for smoking large quantities of fish and meat.",
			ItemType = new ItemType
			{
				MaximumBulk = 7f
			},
			Category = GameData.Instance.AllEntityCategories["upgrades"],
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "sturdyConstruction",
				SalvageProcess = "salvageSmokeOvenUpgrade",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:solidMudBrick", 2 },
					{ "item:stones", 1 }
				},
				Repair = "buildingRepair"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "medium",
				Area = RatingTypes.Food
			},
			Upgrader = new Upgrader
			{
				UpgradeCategories = new string[1] { "smokeOven" },
				SpriteModifier = StateModifier.UpgradeCookhouseSmokeOven
			},
			ToolType = new ToolType
			{
				ToolHandling = ToolHandlingType.Stationary,
				Durability = 1f,
				PrepareProcess = "lightFire"
			},
			ContainerType = new ReplenishContainerType
			{
				CanTransactWithTags = new string[1] { "humanTransact" },
				RequiresReplenishType = new RequiresReplenishType
				{
					ReplenishProcess = "refuelSmokeOven",
					RequiresFuelType = new RequiresFuelType
					{
						MaxFuel = 1f,
						FuelTypeTag = "fuelForCampfire",
						BurnRatePerDay = 4f
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
							AssetName = "boxes"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:dryingShedUpgrade")
		{
			Name = "Drying shed addition",
			SummaryDescription = "Upgrade: Drying shed built in addition to the cookhouse",
			Description = "A convenient and efficient structure for drying fish and meat.",
			ItemType = new ItemType
			{
				MaximumBulk = 6f
			},
			Category = GameData.Instance.AllEntityCategories["upgrades"],
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "sturdyConstruction",
				SalvageProcess = "salvageDryingShedUpgrade",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:solidMudBrick", 1 },
					{ "item:spoakShingles", 1 },
					{ "item:shadeleafCanes", 2 }
				},
				Repair = "buildingRepair"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "medium",
				Area = RatingTypes.Food
			},
			Upgrader = new Upgrader
			{
				UpgradeCategories = new string[1] { "dryingShed" },
				SpriteModifier = StateModifier.UpgradeCookhouseDryingShed
			},
			ToolType = new ToolType
			{
				ToolHandling = ToolHandlingType.Stationary,
				Durability = 1f
			},
			ContainerType = new ToolContainerType
			{
				CanTransactWithTags = new string[1] { "humanTransact" },
				ProductionOutputStorageType = new ItemStorageType(4f)
			},
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "boxes"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:communityHallUpgrade")
		{
			Name = "Community hall",
			SummaryDescription = "Upgrade: Community building addition to the cookhouse",
			Description = "NOTE: the functionality of this building is work-in-progress!",
			ThumbnailSmall = "HUD_thumbnail_placeholder",
			ItemType = new ItemType
			{
				MaximumBulk = 40f
			},
			Category = GameData.Instance.AllEntityCategories["upgrades"],
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "sturdyConstruction",
				SalvageProcess = "salvageCommunityHall",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:solidMudBrick", 4 },
					{ "item:spoakBranchesTrimmed", 2 },
					{ "item:spoakShingles", 3 }
				},
				Repair = "buildingRepair"
			},
			Upgrader = new Upgrader
			{
				UpgradeCategories = new string[1] { "communityHall" },
				SpriteModifier = StateModifier.UpgradeCookhouseCommunityHall
			},
			TierOrArea = new TierOrArea
			{
				Tier = "medium",
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
							AssetName = "boxes"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:spoakLeaves")
		{
			Name = "Spoak leaves",
			SummaryDescription = "Leaves that are large, rigid plates",
			Description = "The stiff leaves can offer protection against rain and wind when placed as overlapping tiles. But, after a time, they will attract the scuttler bug. It is therefore recommended to treat the plant material with a repellant if it is being used for long-term shelter.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.3f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "proneToInfestation",
				DegradesTo = "item:infestedLeavesRemains"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "spoakLeaves"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:infestedLeavesRemains")
		{
			Name = "Infested leaf remains",
			SummaryDescription = "The leaves have been eaten up by scuttler bugs and are no longer useful",
			Description = "After a time, spoak leaves, wingweed leaves and daysheen will attract the scuttler bug which will eat the plant material. It is therefore recommended to treat the plant material with a repellant if it is being used for long-term shelter.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.2f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "biowaste",
				DegradesTo = "item:organicMatter"
			},
			CategoryKey = "waste",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "biowaste"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:spoakBranches")
		{
			Name = "Spoak branches",
			SummaryDescription = "Hard, spiralling branches with large, stiff leaves",
			Description = "These branches should be trimmed, because in this condition they are only useful for the crudest construction tasks.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.5f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "branches"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:spoakBranchesTrimmed")
		{
			Name = "Spoak branches - trimmed",
			SummaryDescription = "Spoak branches free of leaves and cut to size",
			Description = "The hard, durable material is useful for building curving structures, but shaping the wood is difficult work.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.5f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "spoakWood"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:spoakShingles")
		{
			Name = "Spoak shingles",
			SummaryDescription = "Spoak leaf shingles for structures",
			Description = "When made into shingles, the stiff leaves are an excellent cover for a shelter. The leaves are treated with a preservative which increases their resistance from bug infestation.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.2f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "spoakLeaves"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:planks")
		{
			Name = "Planks",
			ItemType = new ItemType
			{
				MaximumBulk = 0.75f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0.5f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "planks"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:stones")
		{
			Name = "Stones",
			Description = "Stones of various size, type and shape",
			SummaryDescription = "Ordinary stones that could be used for simple construction",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.75f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "dirt"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "stones"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:soil")
		{
			Name = "Soil",
			SummaryDescription = "An ordinary pile of soil with no special characteristics",
			Description = "N/A",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.75f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "dirt"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "soil_s"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:podlacUnrefined")
		{
			Name = "Podlac (unrefined)",
			Description = "Hard resin secreted by swamp insects",
			SummaryDescription = "",
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.75f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "dirt"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "charcoal"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:turnipShell")
		{
			Name = "Turnip shell",
			SummaryDescription = "The shell of a turnip animal is exceptionally tough and thick",
			Description = "The shell is sometimes used as a primitive hut. The location of the building is usually right where the animal died, owing to the great weight of the shell which makes it difficult to move.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ItemType = new ItemType
			{
				HasNoMaximumBulk = true
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry"
			},
			CategoryKey = "rawMaterials",
			SharedSpecialActions = new Pair<string, bool>[1]
			{
				new Pair<string, bool>("constructTurnipHut", second: true)
			},
			ShowMarkerWindowSetting = EntityType.ShowMarkerWindowMode.Always,
			IsSelectable = true,
			RenderableType = new RenderableType
			{
				RenderAsModelType = new RenderAsModelType
				{
					AssetName = "turnip",
					ModelScale = 2.5f,
					ModelBasicTextureName = "TurnipDarkTexture",
					DefaultInfo = new AnimConditionInfo
					{
						Looping = Looping.No,
						StartingPoint = StartingPoint.Specified,
						StartingPointInSeconds = 0.5f,
						SpeedFactor = 0f,
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "hide" }
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:twinklerPlating")
		{
			Name = "Quadite plating",
			SummaryDescription = "Pieces of chitinous shell from quadites.",
			Description = "N/A",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.08f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "chitinousshells"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:improvisedGreenHouseCover")
		{
			Name = "Turnip gut sheet",
			SummaryDescription = "Large, translucent sheet for use as a simple window",
			Description = "Because of its translucency it can be used as a primitive substitute for transparent plastic, though not as durable. It shares some characteristics with the so-called goldbeater's skin, made from cow intestines, which was used on Earth for primitive balloons.",
			TierOrArea = new TierOrArea
			{
				Tier = "basic",
				Area = RatingTypes.Food
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tarpBeige"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:megapodGreenHide")
		{
			Name = "Megapod fresh hide",
			SummaryDescription = "Recently removed hide of a megapod",
			Description = "The hide needs to have flesh scraps and membranes removed before it spoils. A primitive way to preserve a hide is to treat it with the animal's own brain.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.08f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "raw meat",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "hidePink"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:megapodRawhide")
		{
			Name = "Megapod rawhide",
			SummaryDescription = "Animal skin. Stiff, but can bend to some extent",
			Description = "Can be made into simple hide products or it can be further processed (tanned) into a soft, tanned hide or leather.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.08f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry",
				DegradesTo = "item:organicMatter"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "hideBeige"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:megapodTannedHide")
		{
			Name = "Megapod tanned hide",
			SummaryDescription = "Soft, tanned hide which has increased durability",
			Description = "The hide has been softened by a tanning treatment.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.08f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry",
				DegradesTo = "item:organicMatter"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tarpBeige"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:megapodBrain")
		{
			Name = "Megapod brain",
			SummaryDescription = "A brain extracted from a megapod",
			Description = "The brain contains oils that can be used as a primitive tanning agent for hide treatment.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ToolType = new ToolType
			{
				Durability = 0.2f,
				ToolHandling = ToolHandlingType.HandTool
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "raw meat",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "brainsPink"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:whipjawGreenHide")
		{
			Name = "Whipjaw fresh hide",
			SummaryDescription = "Recently removed hide of a whipjaw",
			Description = "The hide needs to have flesh scraps and membranes removed before it spoils. A primitive way to preserve a hide is to treat it with the animal's own brain.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.08f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "raw meat",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "hidePink"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:whipjawRawhide")
		{
			Name = "Whipjaw rawhide",
			SummaryDescription = "Animal skin. Stiff, but can bend to some extent",
			Description = "Can be made into simple hide products or it can be further processed (tanned) into a soft, tanned hide or leather.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.08f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry",
				DegradesTo = "item:organicMatter"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "hideBeige"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:whipjawTannedHide")
		{
			Name = "Whipjaw tanned hide",
			SummaryDescription = "Soft, tanned hide which has increased durability",
			Description = "The hide has been softened by a tanning treatment.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.08f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry",
				DegradesTo = "item:organicMatter"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tarpBeige"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:whipjawBrain")
		{
			Name = "Whipjaw brain",
			SummaryDescription = "A brain extracted from a whipjaw",
			Description = "The brain contains oils that can be used as a primitive tanning agent for hide treatment.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ToolType = new ToolType
			{
				Durability = 0.2f,
				ToolHandling = ToolHandlingType.HandTool
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "raw meat",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "brainsPink"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:thunderChickenGreenHide")
		{
			Name = "Thunder chicken fresh hide",
			SummaryDescription = "Recently removed hide of a thunder chicken",
			Description = "The hide needs to have flesh scraps and membranes removed before it spoils. A primitive way to preserve a hide is to treat it with the animal's own brain.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.08f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "raw meat",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "hidePink"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:thunderChickenRawhide")
		{
			Name = "Thunder chicken rawhide",
			SummaryDescription = "Animal skin. Stiff, but can bend to some extent",
			Description = "Can be made into simple hide products or it can be further processed (tanned) into a soft, tanned hide or leather.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.08f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry",
				DegradesTo = "item:organicMatter"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "hideBeige"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:thunderChickenTannedHide")
		{
			Name = "Thunder chicken tanned hide",
			SummaryDescription = "Soft, tanned hide which has increased durability",
			Description = "The hide has been softened by a tanning treatment.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.08f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry",
				DegradesTo = "item:organicMatter"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tarpBeige"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:thunderChickenBrain")
		{
			Name = "Thunder chicken brain",
			SummaryDescription = "The small brain has been extracted from the animal's back",
			Description = "The brain contains oils that can be used as a primitive tanning agent for hide treatment.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ToolType = new ToolType
			{
				Durability = 0.2f,
				ToolHandling = ToolHandlingType.HandTool
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "raw meat",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "brainsPink"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:thunderChickenBones")
		{
			Name = "Thunder chicken bones",
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry",
				DegradesTo = "item:organicMatter"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "boxes"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:bushDragonMeat")
		{
			Name = "Bush dragon meat",
			SummaryDescription = "This meat is inedible by humans",
			Description = "N/A",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.3f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "raw meat",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "meatraw"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:bushDragonPoisonGlands")
		{
			Name = "Bush dragon poison glands",
			SummaryDescription = "The noxious fluid inside these glands could be of use to us",
			Description = "When care is taken, extracting the poison is a fairly simple matter.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.03f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "raw meat",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "gutsyellow"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:bushDragonWings")
		{
			Name = "Bush dragon wings",
			ItemType = new ItemType
			{
				HasNoMaximumBulk = true
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "raw meat",
				DegradesTo = "item:rottenMeat"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "meatraw"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:bushDragonBones")
		{
			Name = "Bush dragon bones",
			ItemType = new ItemType
			{
				MaximumBulk = 0.3f
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "stored dry"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "meatraw"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:improvisedBowLimb")
		{
			Name = "Improvised bow limb",
			SummaryDescription = "Made from a specially suited piece of wood",
			Description = "Carefully shaped with a knife.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "equipment"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Security
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "bowStave"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:improvisedArrowShaftBundle")
		{
			Name = "Arrow shafts",
			SummaryDescription = "Made from shadeleaf canes",
			Description = "Carefully shaped with a knife and heat from a burning campfire.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.04f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "equipment"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Security
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "arrowShafts"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:basicFireExtinguisher")
		{
			Name = "Fire extinguisher",
			SummaryDescription = "Uses cartridges. Has short range.",
			Description = "Designed to control aircraft fires, this extinguisher has moderate discharge power to avoid dispersing burning material. \n A camp member suggests that we could improve its range and output if we had components such as a pump and various tubes.",
			TierOrArea = new TierOrArea
			{
				Tier = "advanced",
				Area = RatingTypes.Security
			},
			ItemType = new ItemType
			{
				HauledItemValue = ItemType.HauledItemValues.MostValuable,
				MaximumBulk = 0.12f,
				AttachedObjectRenderableType = "watergun",
				AttachorTagToMountOn = "rightHand",
				AttachesToBodyPart = "Right arm",
				AnimStatesWhenAttached = new AnimModifier[1] { AnimModifier.Watergun }
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "gadgets"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:fireSuppressantCartridge")
		{
			Name = "Fire suppressant cartridge",
			SummaryDescription = "Ammunition for a fire extinguisher",
			Description = "Used with a fire extinguisher to fight typical aircraft fires. Consists of a CO2 propellant and a fire suppressing compound. To combat other types of fires, the suppressant can be unloaded and exchanged with another kind of compound. \nA camp member has brought forward the idea to 'salvage' the cartridge, and filling it with a toxic compound to defend against quadites.",
			TierOrArea = new TierOrArea
			{
				Tier = "advanced",
				Area = RatingTypes.Security
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment",
				SalvageProcess = "salvageFireSuppressantCartridge",
				PartKeys = new SerializableDictionary<string, int> { { "item:emptyCartridge", 1 } }
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "bottle"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:emptyCartridge")
		{
			Name = "Empty cartridge",
			SummaryDescription = "Empty cartridge for a fire extinguisher",
			Description = "Consists of a CO2 propellant and an empty compound chamber. After being filled, the cartridge can be used in a fire extinguisher as 'ammunition'. \nSome camp members have suggested filling it with bush dragon poison to defend against quadites.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Security
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.03f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "bottle"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:flintlockMechanism")
		{
			Name = "Flintlock mechanism",
			SummaryDescription = "Ignition mechanism of 18th century design used for blackpowder firearms",
			Description = "A piece of flint held by a metal hammer. When triggered, the flint will hit a steel plate and make a spark which can ignite a blackpowder charge.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.04f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "equipment"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic",
				Area = RatingTypes.Security
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "greyPouch"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:boltActionMechanism")
		{
			Name = "Bolt-action mechanism",
			SummaryDescription = "Simple firearm mechanism for firing metal cartridges",
			Description = "Components that allow the spent cartridge case to be ejected and a new one to be placed in the breech.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.04f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "equipment"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "medium",
				Area = RatingTypes.Security
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "greyPouch"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:landMine")
		{
			Name = "Land mine",
			SummaryDescription = "Explosive device triggered by pressure",
			Description = "Simple land mine made from blackpowder and a flintlock trigger mechanism. Must be deployed as a structure.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.12f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "equipment"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic",
				Area = RatingTypes.Security
			},
			CategoryKey = "rawMaterials",
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
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:unfiredClayJar")
		{
			Name = "Clay jar (unfired)",
			SummaryDescription = "Half finished clay jar",
			Description = "This piece of pottery is dry but needs to be fired and glazed in a kiln before it can be of use.",
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.07f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "clayPotBeige"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:unfiredClayPot")
		{
			Name = "Clay pot (unfired)",
			SummaryDescription = "Half finished clay pot",
			Description = "This piece of pottery is dry but needs to be fired in a kiln before it can be of use.",
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.07f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "clayPotSmallBeige"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:unfiredBulletMold")
		{
			Name = "Bullet mold (unfired)",
			SummaryDescription = "Half finished bullet mold",
			Description = "This piece of ceramics is dry but needs to be fired in a kiln before it can be of use.",
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.07f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "brickMold"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:liquidGas")
		{
			Name = "Liquid gas",
			SummaryDescription = "Flammable gas for heating, cooking or running certain engines.",
			Description = "The gas is pressurized and in liquid form.",
			TierOrArea = new TierOrArea
			{
				Tier = "advanced"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.5f,
				FuelType = new FuelType
				{
					FuelTags = new string[1] { "fuelForFieldKitchen" }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "drum"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:coilRifle")
		{
			Name = "Coil rifle",
			SummaryDescription = "Electromagnetic gun.",
			Description = "Part of the standard issue equipment for the Tau Ceti Program. Range, precision and power is efficient against all types of native wildlife. The gun works by accelerating the (ferromagnetic) projectile through a series of electromagnetic coils.",
			ItemType = new ItemType
			{
				HauledItemValue = ItemType.HauledItemValues.MostValuable,
				MaximumBulk = 0.08f,
				AttachedObjectRenderableType = "rifle",
				AttachorTagToMountOn = "rightHand",
				AttachesToBodyPart = "Right arm",
				AnimStatesWhenAttached = new AnimModifier[1] { AnimModifier.Rifle },
				WeaponType = new WeaponType
				{
					AttackTypes = new AttackType[1] { GameData.Instance.AllAttackTypes["shootCoilRifle"] }
				},
				TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>
				{
					{
						ItemType.TaskType.LongerJourneys,
						ItemType.AppropriateLevel.Best
					},
					{
						ItemType.TaskType.PatrolOrAttack,
						ItemType.AppropriateLevel.Best
					},
					{
						ItemType.TaskType.UnspecifiedHunting,
						ItemType.AppropriateLevel.Best
					},
					{
						ItemType.TaskType.Scouting,
						ItemType.AppropriateLevel.Minor
					},
					{
						ItemType.TaskType.Hauling,
						ItemType.AppropriateLevel.Minor
					}
				}
			},
			TierOrArea = new TierOrArea
			{
				Tier = "advanced",
				Area = RatingTypes.Security
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			ContainerType = new MagazineContainerType
			{
				CanTransactWithTags = new string[1] { "humanTransact" },
				MaxCapacity = 15,
				UsesAmmoTypeKeyName = "item:coilRifleAmmo",
				ReplenishProcess = "reloadCoilRifleAmmo"
			},
			CategoryKey = "weapons",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "guns"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:boltActionRifle")
		{
			Name = "Bolt-action rifle",
			SummaryDescription = "For hunting and defense.",
			Description = "A rugged rifle which is loaded with metal cartridge ammunition by using a manually operated bolt mechanism.",
			ItemType = new ItemType
			{
				HauledItemValue = ItemType.HauledItemValues.MostValuable,
				MaximumBulk = 0.1f,
				AttachedObjectRenderableType = "rifle",
				AttachorTagToMountOn = "rightHand",
				AttachesToBodyPart = "Right arm",
				AnimStatesWhenAttached = new AnimModifier[1] { AnimModifier.Rifle },
				WeaponType = new WeaponType
				{
					AttackTypes = new AttackType[1] { GameData.Instance.AllAttackTypes["shootCorditeRifledBullet"] }
				},
				TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>
				{
					{
						ItemType.TaskType.LongerJourneys,
						ItemType.AppropriateLevel.Best
					},
					{
						ItemType.TaskType.PatrolOrAttack,
						ItemType.AppropriateLevel.Best
					},
					{
						ItemType.TaskType.UnspecifiedHunting,
						ItemType.AppropriateLevel.Best
					},
					{
						ItemType.TaskType.Scouting,
						ItemType.AppropriateLevel.Minor
					},
					{
						ItemType.TaskType.Hauling,
						ItemType.AppropriateLevel.Minor
					}
				}
			},
			TierOrArea = new TierOrArea
			{
				Tier = "medium",
				Area = RatingTypes.Security
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment",
				SalvageProcess = "salvageBoltActionRifle",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:gunBarrelRifled", 1 },
					{ "item:gunStock", 1 },
					{ "item:boltActionMechanism", 1 }
				}
			},
			ContainerType = new MagazineContainerType
			{
				CanTransactWithTags = new string[1] { "humanTransact" },
				MaxCapacity = 10,
				UsesAmmoTypeKeyName = "item:corditeAmmo",
				ReplenishProcess = "reloadBoltActionRifle"
			},
			CategoryKey = "weapons",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "guns"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:gunpowderRifle")
		{
			Name = "Black powder rifle",
			SummaryDescription = "Breech-loading flintlock rifle.",
			Description = "Has high precision and rate of fire when taken into account its low tech manufacturing process. The design is somewhat similar to a Ferguson rifle of the 18th century: The weapon is loaded from the breech with black powder and a rifle bullet. Must be reloaded often.",
			ItemType = new ItemType
			{
				HauledItemValue = ItemType.HauledItemValues.MostValuable,
				MaximumBulk = 0.14f,
				AttachedObjectRenderableType = "rifle",
				AttachorTagToMountOn = "rightHand",
				AttachesToBodyPart = "Right arm",
				AnimStatesWhenAttached = new AnimModifier[1] { AnimModifier.Rifle },
				WeaponType = new WeaponType
				{
					AttackTypes = new AttackType[1] { GameData.Instance.AllAttackTypes["shootGunpowderRifle"] }
				},
				TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>
				{
					{
						ItemType.TaskType.LongerJourneys,
						ItemType.AppropriateLevel.Best
					},
					{
						ItemType.TaskType.PatrolOrAttack,
						ItemType.AppropriateLevel.Best
					},
					{
						ItemType.TaskType.UnspecifiedHunting,
						ItemType.AppropriateLevel.Best
					},
					{
						ItemType.TaskType.Scouting,
						ItemType.AppropriateLevel.Minor
					},
					{
						ItemType.TaskType.Hauling,
						ItemType.AppropriateLevel.Minor
					}
				}
			},
			TierOrArea = new TierOrArea
			{
				Tier = "medium",
				Area = RatingTypes.Security
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment",
				SalvageProcess = "salvageGunpowderRifle",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:gunBarrelRifled", 1 },
					{ "item:gunStock", 1 },
					{ "item:flintlockMechanism", 1 }
				}
			},
			ContainerType = new MagazineContainerType
			{
				CanTransactWithTags = new string[1] { "humanTransact" },
				MaxCapacity = 3,
				UsesAmmoTypeKeyName = "item:blackPowderRifleAmmo",
				ReplenishProcess = "reloadGunpowderRifle"
			},
			CategoryKey = "weapons",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "guns"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:musket")
		{
			Name = "Musket",
			SummaryDescription = "Muzzleloading, flintlock smooth-bore.",
			Description = "A simple firearm that fires smooth gold balls using black powder. Has limited range and accuracy and must be reloaded often.",
			ItemType = new ItemType
			{
				HauledItemValue = ItemType.HauledItemValues.MostValuable,
				MaximumBulk = 0.14f,
				AttachedObjectRenderableType = "rifle",
				AttachorTagToMountOn = "rightHand",
				AttachesToBodyPart = "Right arm",
				AnimStatesWhenAttached = new AnimModifier[1] { AnimModifier.Rifle },
				WeaponType = new WeaponType
				{
					AttackTypes = new AttackType[2]
					{
						GameData.Instance.AllAttackTypes["shootUnrifledBullet"],
						GameData.Instance.AllAttackTypes["hitWithRifleButt"]
					}
				},
				TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>
				{
					{
						ItemType.TaskType.LongerJourneys,
						ItemType.AppropriateLevel.Best
					},
					{
						ItemType.TaskType.PatrolOrAttack,
						ItemType.AppropriateLevel.Best
					},
					{
						ItemType.TaskType.UnspecifiedHunting,
						ItemType.AppropriateLevel.Best
					},
					{
						ItemType.TaskType.Scouting,
						ItemType.AppropriateLevel.Minor
					},
					{
						ItemType.TaskType.Hauling,
						ItemType.AppropriateLevel.Minor
					}
				}
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic",
				Area = RatingTypes.Security
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment",
				SalvageProcess = "salvageMusket",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:gunBarrelSmoothLong", 1 },
					{ "item:gunStock", 1 },
					{ "item:flintlockMechanism", 1 }
				}
			},
			ContainerType = new MagazineContainerType
			{
				CanTransactWithTags = new string[1] { "humanTransact" },
				MaxCapacity = 3,
				UsesAmmoTypeKeyName = "item:blackPowderShotAmmo",
				ReplenishProcess = "reloadBlunderbuss"
			},
			CategoryKey = "weapons",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "guns"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:musketoon")
		{
			Name = "Musketoon",
			SummaryDescription = "Muzzleloading flintlock shotgun.",
			Description = "A simple firearm that fires a number of small projectiles in a single blast. Basically a shorter musket with bigger bore which increases the spread, giving better accuracy but shorter range. Useful for vermin hunting. Must be reloaded often.",
			ItemType = new ItemType
			{
				HauledItemValue = ItemType.HauledItemValues.MostValuable,
				MaximumBulk = 0.1f,
				AttachedObjectRenderableType = "rifle",
				AttachorTagToMountOn = "rightHand",
				AttachesToBodyPart = "Right arm",
				AnimStatesWhenAttached = new AnimModifier[1] { AnimModifier.Rifle },
				WeaponType = new WeaponType
				{
					AttackTypes = new AttackType[2]
					{
						GameData.Instance.AllAttackTypes["shootBlunderbuss"],
						GameData.Instance.AllAttackTypes["hitWithRifleButt"]
					}
				},
				TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>
				{
					{
						ItemType.TaskType.LongerJourneys,
						ItemType.AppropriateLevel.Best
					},
					{
						ItemType.TaskType.PatrolOrAttack,
						ItemType.AppropriateLevel.Best
					},
					{
						ItemType.TaskType.UnspecifiedHunting,
						ItemType.AppropriateLevel.Best
					},
					{
						ItemType.TaskType.Scouting,
						ItemType.AppropriateLevel.Minor
					},
					{
						ItemType.TaskType.Hauling,
						ItemType.AppropriateLevel.Minor
					}
				}
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic",
				Area = RatingTypes.Security
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment",
				SalvageProcess = "salvageMusketoon",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:gunBarrelSmoothShort", 1 },
					{ "item:gunStock", 1 },
					{ "item:flintlockMechanism", 1 }
				}
			},
			ContainerType = new MagazineContainerType
			{
				CanTransactWithTags = new string[1] { "humanTransact" },
				MaxCapacity = 3,
				UsesAmmoTypeKeyName = "item:blackPowderShotAmmo",
				ReplenishProcess = "reloadBlunderbuss"
			},
			CategoryKey = "weapons",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "guns"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:shotgun")
		{
			Name = "Pump-action shotgun",
			SummaryDescription = "Rugged and very effective shotgun.",
			Description = "Designed for the Tau Ceti mission with the intention of having easily replaceable parts. A smoothbore firearm which uses cordite-propelled cartridges to fire a number of pellets in each blast. The handgrip is pumped between each shot to replace the cartridge in the chamber.",
			ItemType = new ItemType
			{
				HauledItemValue = ItemType.HauledItemValues.MostValuable,
				MaximumBulk = 0.08f,
				AttachedObjectRenderableType = "rifle",
				AttachorTagToMountOn = "rightHand",
				AttachesToBodyPart = "Right arm",
				AnimStatesWhenAttached = new AnimModifier[1] { AnimModifier.Rifle },
				WeaponType = new WeaponType
				{
					AttackTypes = new AttackType[2]
					{
						GameData.Instance.AllAttackTypes["shootShotgun"],
						GameData.Instance.AllAttackTypes["hitWithRifleButt"]
					}
				},
				TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>
				{
					{
						ItemType.TaskType.LongerJourneys,
						ItemType.AppropriateLevel.Best
					},
					{
						ItemType.TaskType.PatrolOrAttack,
						ItemType.AppropriateLevel.Best
					},
					{
						ItemType.TaskType.UnspecifiedHunting,
						ItemType.AppropriateLevel.Best
					},
					{
						ItemType.TaskType.Scouting,
						ItemType.AppropriateLevel.Minor
					},
					{
						ItemType.TaskType.Hauling,
						ItemType.AppropriateLevel.Minor
					}
				}
			},
			TierOrArea = new TierOrArea
			{
				Tier = "advanced",
				Area = RatingTypes.Security
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment",
				SalvageProcess = "salvageShotgun",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:gunBarrelSmoothShort", 1 },
					{ "item:gunStock", 1 }
				}
			},
			ContainerType = new MagazineContainerType
			{
				CanTransactWithTags = new string[1] { "humanTransact" },
				MaxCapacity = 10,
				UsesAmmoTypeKeyName = "item:shotgunAmmo",
				ReplenishProcess = "reloadShotgun"
			},
			CategoryKey = "weapons",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "guns"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:improvedFireExtinguisher")
		{
			Name = "Spray weapon",
			SummaryDescription = "Improvised weapon. Can spray a powerful discharge of toxic liquid",
			Description = "This modified fire extinguisher allows for powerful discharges of a toxic liquid over long range and will be effective against twinklers.",
			ItemType = new ItemType
			{
				HauledItemValue = ItemType.HauledItemValues.MostValuable,
				MaximumBulk = 0.13f,
				AttachedObjectRenderableType = "watergun",
				AttachorTagToMountOn = "rightHand",
				AttachesToBodyPart = "Right arm",
				AnimStatesWhenAttached = new AnimModifier[1] { AnimModifier.Watergun },
				TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>
				{
					{
						ItemType.TaskType.LongerJourneys,
						ItemType.AppropriateLevel.Best
					},
					{
						ItemType.TaskType.PatrolOrAttack,
						ItemType.AppropriateLevel.Best
					},
					{
						ItemType.TaskType.UnspecifiedHunting,
						ItemType.AppropriateLevel.None
					},
					{
						ItemType.TaskType.Scouting,
						ItemType.AppropriateLevel.Minor
					},
					{
						ItemType.TaskType.Hauling,
						ItemType.AppropriateLevel.None
					}
				},
				WeaponType = new WeaponType
				{
					AttackTypes = new AttackType[1] { GameData.Instance.AllAttackTypes["shootImprovedFireExtinguisherBushDragonPoison"] }
				}
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Security
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			ContainerType = new MagazineContainerType
			{
				CanTransactWithTags = new string[1] { "humanTransact" },
				MaxCapacity = 10,
				UsesAmmoTag = "fireExtinguisherAmmo",
				ReplenishProcess = "reloadFireExtinguisher"
			},
			CategoryKey = "weapons",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "gadgets"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:sentry")
		{
			Name = "Sentry (item)",
			SummaryDescription = "Autonomous gun turret for area defence",
			Description = "The TRIAAD is a stationary machine gun for area defence. It has sensors and a degree of AI for operating in all conditions, and has been optimized for an alien environment containing unknown threats. Its low power consumption and deep magazine makes it able to operate unsupervised for extended periods of time.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.83f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				PartsAreWeatherProof = true,
				DegradeType = "equipment",
				SalvageProcess = "salvageSentryItem",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:sentryGun", 1 },
					{ "item:sentryWeaponMount", 1 }
				}
			},
			TierOrArea = new TierOrArea
			{
				Tier = "advanced",
				Area = RatingTypes.Security
			},
			CategoryKey = "weapons",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "guns"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:spraySentry")
		{
			Name = "Spray gun sentry (item)",
			SummaryDescription = "Autonomous turret, modified with an improvised spray gun",
			Description = "We have dismounted the machine gun and jerry-rigged a fire extinguisher gun onto the turret, making it able to, hopefully, shoot a poisonous liquid at approaching twinklers.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.83f
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Security
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				PartsAreWeatherProof = true,
				DegradeType = "equipment",
				SalvageProcess = "salvageSpraySentryItem",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:sentrySprayGun", 1 },
					{ "item:sentryWeaponMount", 1 }
				}
			},
			CategoryKey = "weapons",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "guns"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:shotgunSentry")
		{
			Name = "Shotgun sentry (item)",
			SummaryDescription = "Autonomous turret outfitted with a shotgun. Made with improvised methods.",
			Description = "We have dismounted the machine gun and attached a shotgun onto the turret.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.83f
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Security
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				PartsAreWeatherProof = true,
				DegradeType = "equipment",
				SalvageProcess = "salvageShotgunSentryItem",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:sentryShotgun", 1 },
					{ "item:sentryWeaponMount", 1 }
				}
			},
			CategoryKey = "weapons",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "guns"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:improvisedBow")
		{
			Name = "Bow (improvised)",
			SummaryDescription = "Simple bow made from wood. Ammunition: Arrows",
			Description = "This bow offers some advantage over hand weapons, particularly when sneaking up on faster prey.",
			ItemType = new ItemType
			{
				HauledItemValue = ItemType.HauledItemValues.MostValuable,
				MaximumBulk = 0.07f,
				AttachedObjectRenderableType = "bow",
				AttachorTagToMountOn = "leftHand",
				AttachesToBodyPart = "Left arm",
				AnimStatesWhenAttached = new AnimModifier[1] { AnimModifier.Bow },
				WeaponType = new WeaponType
				{
					AttackTypes = new AttackType[4]
					{
						GameData.Instance.AllAttackTypes["shootImprovisedBasicArrow"],
						GameData.Instance.AllAttackTypes["shootImprovisedChitinousArrow"],
						GameData.Instance.AllAttackTypes["shootImprovisedMetalArrow"],
						GameData.Instance.AllAttackTypes["shootIronArrow"]
					}
				},
				TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>
				{
					{
						ItemType.TaskType.LongerJourneys,
						ItemType.AppropriateLevel.Normal
					},
					{
						ItemType.TaskType.UnspecifiedHunting,
						ItemType.AppropriateLevel.Best
					},
					{
						ItemType.TaskType.PatrolOrAttack,
						ItemType.AppropriateLevel.Normal
					},
					{
						ItemType.TaskType.Scouting,
						ItemType.AppropriateLevel.Minor
					},
					{
						ItemType.TaskType.Hauling,
						ItemType.AppropriateLevel.Minor
					}
				}
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Security
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "improvisedEquipment"
			},
			ContainerType = new MagazineContainerType
			{
				CanTransactWithTags = new string[1] { "humanTransact" },
				MaxCapacity = 5,
				UsesAmmoTag = "arrow",
				ReplenishProcess = "reloadImprovisedBow"
			},
			CategoryKey = "weapons",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "bow"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:improvisedBasicSpear")
		{
			Name = "Spear (crude)",
			SummaryDescription = "Long, sharpened pole. Serves as both tool and weapon.",
			Description = "A crude weapon that doubles as a tool for some fishing and gathering of small prey.",
			Icon = "spear",
			ToolType = new ToolType
			{
				Durability = 0.9f,
				ToolHandling = ToolHandlingType.HandTool
			},
			ItemType = new ItemType
			{
				HauledItemValue = ItemType.HauledItemValues.MostValuable,
				MaximumBulk = 0.1f,
				AttachedObjectRenderableType = "spear",
				AttachorTagToMountOn = "rightHand",
				AttachesToBodyPart = "Right arm",
				AnimStatesWhenAttached = new AnimModifier[1] { AnimModifier.Spear },
				WeaponType = new WeaponType
				{
					AttackTypes = new AttackType[1] { GameData.Instance.AllAttackTypes["personCrudeSpearThrustMid"] }
				}
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Security
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "weapons",
			RenderableType = new RenderableType
			{
				RenderAsModelType = new RenderAsModelType
				{
					AssetName = "spear",
					ModelScale = 2f
				}
			}
		});
		entityType = new EntityType("item:ironSpear")
		{
			Name = "Spear (iron-tipped)",
			SummaryDescription = "An handmade spear outfitted with a sharp spearhead made from wrought iron. Serves as both tool and weapon.",
			Description = "A reliable weapon that doubles as a tool for some fishing and gathering of small prey.",
			Icon = "spear",
			ToolType = new ToolType
			{
				Durability = 0.9f,
				ToolHandling = ToolHandlingType.HandTool
			}
		};
		EntityType entityType3 = entityType;
		LocomotorType locomotorType = new LocomotorType();
		LocomotorType locomotorType2 = locomotorType;
		BallisticLocomotorType ballisticLocomotorType = new BallisticLocomotorType();
		locomotorType2.BallisticLocomotorType = ballisticLocomotorType;
		LocomotorType locomotorType3 = locomotorType;
		CollisionResponderType collisionResponderType = new CollisionResponderType();
		CollisionResponderType collisionResponderType2 = collisionResponderType;
		BallisticResponderType ballisticResponderType = new BallisticResponderType();
		collisionResponderType2.BallisticResponderType = ballisticResponderType;
		locomotorType3.CollisionResponderType = collisionResponderType;
		entityType3.LocomotorType = locomotorType;
		entityType.TierOrArea = new TierOrArea
		{
			Tier = "basic",
			Area = RatingTypes.Security
		};
		entityType.ItemType = new ItemType
		{
			HauledItemValue = ItemType.HauledItemValues.MostValuable,
			MaximumBulk = 0.1f,
			AttachedObjectRenderableType = "spear",
			AttachorTagToMountOn = "rightHand",
			AttachesToBodyPart = "Right arm",
			AnimStatesWhenAttached = new AnimModifier[1] { AnimModifier.Spear },
			TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>
			{
				{
					ItemType.TaskType.LongerJourneys,
					ItemType.AppropriateLevel.Normal
				},
				{
					ItemType.TaskType.PatrolOrAttack,
					ItemType.AppropriateLevel.Normal
				},
				{
					ItemType.TaskType.UnspecifiedHunting,
					ItemType.AppropriateLevel.Normal
				},
				{
					ItemType.TaskType.Scouting,
					ItemType.AppropriateLevel.Minor
				},
				{
					ItemType.TaskType.Hauling,
					ItemType.AppropriateLevel.Minor
				}
			},
			WeaponType = new WeaponType
			{
				AttackTypes = new AttackType[1] { GameData.Instance.AllAttackTypes["personMetalSpearThrustMid"] }
			}
		};
		entityType.NonLivingType = new NonLivingType
		{
			Repairability = 1f,
			DegradeType = "equipment"
		};
		entityType.CategoryKey = "weapons";
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "spear",
				ModelScale = 2f
			}
		};
		listOfEntityTypes.Add(entityType);
		entityType = new EntityType("item:improvisedGoodSpear")
		{
			Name = "Spear (scrap metal)",
			SummaryDescription = "An improvised spear outfitted with a sharp metal head. Serves as both a tool and a weapon.",
			Description = "A simple weapon that doubles as a tool for some fishing and gathering of small prey.",
			Icon = "spear",
			ToolType = new ToolType
			{
				Durability = 0.9f,
				ToolHandling = ToolHandlingType.HandTool
			}
		};
		EntityType entityType4 = entityType;
		locomotorType = new LocomotorType();
		LocomotorType locomotorType4 = locomotorType;
		ballisticLocomotorType = new BallisticLocomotorType();
		locomotorType4.BallisticLocomotorType = ballisticLocomotorType;
		LocomotorType locomotorType5 = locomotorType;
		collisionResponderType = new CollisionResponderType();
		CollisionResponderType collisionResponderType3 = collisionResponderType;
		ballisticResponderType = new BallisticResponderType();
		collisionResponderType3.BallisticResponderType = ballisticResponderType;
		locomotorType5.CollisionResponderType = collisionResponderType;
		entityType4.LocomotorType = locomotorType;
		entityType.TierOrArea = new TierOrArea
		{
			Tier = "survival",
			Area = RatingTypes.Security
		};
		entityType.ItemType = new ItemType
		{
			HauledItemValue = ItemType.HauledItemValues.MostValuable,
			MaximumBulk = 0.1f,
			AttachedObjectRenderableType = "spear",
			AttachorTagToMountOn = "rightHand",
			AttachesToBodyPart = "Right arm",
			AnimStatesWhenAttached = new AnimModifier[1] { AnimModifier.Spear },
			TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>
			{
				{
					ItemType.TaskType.LongerJourneys,
					ItemType.AppropriateLevel.Normal
				},
				{
					ItemType.TaskType.PatrolOrAttack,
					ItemType.AppropriateLevel.Normal
				},
				{
					ItemType.TaskType.UnspecifiedHunting,
					ItemType.AppropriateLevel.Normal
				},
				{
					ItemType.TaskType.Scouting,
					ItemType.AppropriateLevel.Minor
				},
				{
					ItemType.TaskType.Hauling,
					ItemType.AppropriateLevel.Minor
				}
			},
			WeaponType = new WeaponType
			{
				AttackTypes = new AttackType[1] { GameData.Instance.AllAttackTypes["personMetalSpearThrustMid"] }
			}
		};
		entityType.NonLivingType = new NonLivingType
		{
			Repairability = 1f,
			DegradeType = "equipment"
		};
		entityType.CategoryKey = "weapons";
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "spear",
				ModelScale = 2f
			}
		};
		listOfEntityTypes.Add(entityType);
		entityType = new EntityType("item:advancedKnifeSpear")
		{
			Name = "Knife spear (improvised)",
			SummaryDescription = "A knife attached to a long shaft (Retrieve the knife by salvaging the spear). Serves as both tool and weapon",
			Description = "This improvised weapon is an efficient way to extend the knife's reach for use in hunting, defense and fishing.",
			Icon = "spear",
			ToolType = new ToolType
			{
				Durability = 0.9f,
				ToolHandling = ToolHandlingType.HandTool
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Security
			}
		};
		EntityType entityType5 = entityType;
		locomotorType = new LocomotorType();
		LocomotorType locomotorType6 = locomotorType;
		ballisticLocomotorType = new BallisticLocomotorType();
		locomotorType6.BallisticLocomotorType = ballisticLocomotorType;
		LocomotorType locomotorType7 = locomotorType;
		collisionResponderType = new CollisionResponderType();
		CollisionResponderType collisionResponderType4 = collisionResponderType;
		ballisticResponderType = new BallisticResponderType();
		collisionResponderType4.BallisticResponderType = ballisticResponderType;
		locomotorType7.CollisionResponderType = collisionResponderType;
		entityType5.LocomotorType = locomotorType;
		entityType.ItemType = new ItemType
		{
			HauledItemValue = ItemType.HauledItemValues.MostValuable,
			MaximumBulk = 0.1f,
			AttachedObjectRenderableType = "spear",
			AttachorTagToMountOn = "rightHand",
			AttachesToBodyPart = "Right arm",
			AnimStatesWhenAttached = new AnimModifier[1] { AnimModifier.Spear },
			TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>
			{
				{
					ItemType.TaskType.LongerJourneys,
					ItemType.AppropriateLevel.Normal
				},
				{
					ItemType.TaskType.PatrolOrAttack,
					ItemType.AppropriateLevel.Normal
				},
				{
					ItemType.TaskType.UnspecifiedHunting,
					ItemType.AppropriateLevel.Normal
				},
				{
					ItemType.TaskType.Scouting,
					ItemType.AppropriateLevel.Minor
				},
				{
					ItemType.TaskType.Hauling,
					ItemType.AppropriateLevel.Minor
				}
			},
			WeaponType = new WeaponType
			{
				AttackTypes = new AttackType[1] { GameData.Instance.AllAttackTypes["personMetalSpearThrustMid"] }
			}
		};
		entityType.NonLivingType = new NonLivingType
		{
			Repairability = 1f,
			DegradeType = "equipment",
			SalvageProcess = "salvageKnifeSpear",
			PartKeys = new SerializableDictionary<string, int> { { "item:advancedKnife", 1 } }
		};
		entityType.CategoryKey = "weapons";
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "spear",
				ModelScale = 2f
			}
		};
		listOfEntityTypes.Add(entityType);
		entityType = new EntityType("item:improvisedFlintSpear")
		{
			Name = "Spear (flint-tipped)",
			SummaryDescription = "An improvised spear tipped with a sharp piece of flint. Serves as both tool and weapon",
			Description = "A simple weapon that doubles as a tool for some fishing and gathering of small prey.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Security
			},
			Icon = "spear",
			ToolType = new ToolType
			{
				Durability = 0.9f,
				ToolHandling = ToolHandlingType.HandTool
			}
		};
		EntityType entityType6 = entityType;
		locomotorType = new LocomotorType();
		LocomotorType locomotorType8 = locomotorType;
		ballisticLocomotorType = new BallisticLocomotorType();
		locomotorType8.BallisticLocomotorType = ballisticLocomotorType;
		LocomotorType locomotorType9 = locomotorType;
		collisionResponderType = new CollisionResponderType();
		CollisionResponderType collisionResponderType5 = collisionResponderType;
		ballisticResponderType = new BallisticResponderType();
		collisionResponderType5.BallisticResponderType = ballisticResponderType;
		locomotorType9.CollisionResponderType = collisionResponderType;
		entityType6.LocomotorType = locomotorType;
		entityType.ItemType = new ItemType
		{
			HauledItemValue = ItemType.HauledItemValues.MostValuable,
			MaximumBulk = 0.1f,
			AttachedObjectRenderableType = "spear",
			AttachorTagToMountOn = "rightHand",
			AttachesToBodyPart = "Right arm",
			AnimStatesWhenAttached = new AnimModifier[1] { AnimModifier.Spear },
			TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>
			{
				{
					ItemType.TaskType.LongerJourneys,
					ItemType.AppropriateLevel.Normal
				},
				{
					ItemType.TaskType.PatrolOrAttack,
					ItemType.AppropriateLevel.Normal
				},
				{
					ItemType.TaskType.UnspecifiedHunting,
					ItemType.AppropriateLevel.Normal
				},
				{
					ItemType.TaskType.Scouting,
					ItemType.AppropriateLevel.Minor
				},
				{
					ItemType.TaskType.Hauling,
					ItemType.AppropriateLevel.Minor
				}
			},
			WeaponType = new WeaponType
			{
				AttackTypes = new AttackType[1] { GameData.Instance.AllAttackTypes["personFlintSpearThrustMid"] }
			}
		};
		entityType.NonLivingType = new NonLivingType
		{
			Repairability = 1f,
			DegradeType = "equipment"
		};
		entityType.CategoryKey = "weapons";
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "spear",
				ModelScale = 2f
			}
		};
		listOfEntityTypes.Add(entityType);
		listOfEntityTypes.Add(new EntityType("item:bushDragonCartridge")
		{
			Name = "Bush dragon cartridge",
			SummaryDescription = "A fire extinguisher cartridge with bush dragon poison.",
			Description = "The cartridge can be used as ammo for a spray weapon, a modified fire extinguisher. Contains a toxic bush dragon agent and a CO2-propellant.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Security
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				AmmunitionType = new AmmunitionType
				{
					MaxNoOfRounds = 20,
					AmmoTags = new string[1] { "fireExtinguisherAmmo" }
				},
				HauledItemValue = ItemType.HauledItemValues.MostValuable
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "ammunition",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "bottle"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:coilRifleAmmo")
		{
			Name = "Coil rifle ammo clip",
			SummaryDescription = "Ammunition for a coil gun",
			Description = "Solid projectiles made from a ferromagnetic steel-alloy",
			ItemType = new ItemType
			{
				HauledItemValue = ItemType.HauledItemValues.MostValuable,
				MaximumBulk = 0.03f,
				AmmunitionType = new AmmunitionType
				{
					MaxNoOfRounds = 30
				}
			},
			TierOrArea = new TierOrArea
			{
				Tier = "advanced",
				Area = RatingTypes.Security
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "ammunition",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "greyPouch"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:sentryGunAmmo")
		{
			Name = "Sentry gun ammo",
			SummaryDescription = "Ammunition for the sentry robot",
			Description = "Cartridges, each containing chemical propellant and a bullet",
			ItemType = new ItemType
			{
				HauledItemValue = ItemType.HauledItemValues.MostValuable,
				MaximumBulk = 0.06f,
				AmmunitionType = new AmmunitionType
				{
					MaxNoOfRounds = 100
				}
			},
			TierOrArea = new TierOrArea
			{
				Tier = "advanced",
				Area = RatingTypes.Security
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "ammunition",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "boxes"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:corditeAmmo")
		{
			Name = "Bolt-action rifle ammo",
			SummaryDescription = "Ammunition clip for a bolt-action rifle",
			Description = "Several cartridges with cordite propelled projectiles.",
			ItemType = new ItemType
			{
				HauledItemValue = ItemType.HauledItemValues.MostValuable,
				MaximumBulk = 0.03f,
				AmmunitionType = new AmmunitionType
				{
					MaxNoOfRounds = 20
				}
			},
			TierOrArea = new TierOrArea
			{
				Tier = "medium",
				Area = RatingTypes.Security
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "ammunition",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "greyPouch"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:blackPowderRifleAmmo")
		{
			Name = "Black powder rifle ammo",
			SummaryDescription = "Ammunition for a black powder rifle",
			Description = "A pouch with black powder and a small bag with rifled gold bullets. The high density and ductility of gold makes for an advantageous projectile material.",
			ItemType = new ItemType
			{
				HauledItemValue = ItemType.HauledItemValues.MostValuable,
				MaximumBulk = 0.05f,
				AmmunitionType = new AmmunitionType
				{
					MaxNoOfRounds = 6
				}
			},
			TierOrArea = new TierOrArea
			{
				Tier = "medium",
				Area = RatingTypes.Security
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "improvisedEquipment",
				SalvageProcess = "salvageBlackPowderRifleAmmo",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:goldBullet", 1 },
					{ "item:blackPowder", 1 }
				}
			},
			CategoryKey = "ammunition",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "greyPouch"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:shotgunAmmo")
		{
			Name = "Shotgun shells",
			SummaryDescription = "Ammunition cartridges for a shotgun",
			Description = "Plastic cases containing primer, powder charge and tungsten pellets",
			ItemType = new ItemType
			{
				HauledItemValue = ItemType.HauledItemValues.MostValuable,
				MaximumBulk = 0.03f,
				AmmunitionType = new AmmunitionType
				{
					MaxNoOfRounds = 20
				}
			},
			TierOrArea = new TierOrArea
			{
				Tier = "advanced",
				Area = RatingTypes.Security
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "ammunition",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "greyPouch"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:blackPowderShotAmmo")
		{
			Name = "Musket/musketoon ammo",
			SummaryDescription = "Ammunition for a black powder smooth bore firearm such as a musket or musketoon",
			Description = "A pouch with black powder and a small bag with gold balls to be used in a non-rifled firearm. The high density and ductility of gold makes for an advantageous projectile material.",
			ItemType = new ItemType
			{
				HauledItemValue = ItemType.HauledItemValues.MostValuable,
				MaximumBulk = 0.1f,
				AmmunitionType = new AmmunitionType
				{
					MaxNoOfRounds = 6
				}
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic",
				Area = RatingTypes.Security
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "improvisedEquipment",
				SalvageProcess = "salvageBlackPowderShotAmmo",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:blunderbussBalls", 1 },
					{ "item:blackPowder", 1 }
				}
			},
			CategoryKey = "ammunition",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "greyPouch"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:improvisedBasicArrow")
		{
			Name = "Arrows (crude)",
			SummaryDescription = "Bunch of simple arrows without arrowheads or fletchings",
			Description = "These arrows, although basically sharpened sticks, still offer a chance to hunt prey that cannot be caught by hand.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.04f,
				AmmunitionType = new AmmunitionType
				{
					MaxNoOfRounds = 10,
					AmmoTags = new string[1] { "arrow" }
				},
				HauledItemValue = ItemType.HauledItemValues.MostValuable
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "improvisedEquipment"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Security
			},
			CategoryKey = "ammunition",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "arrowShafts"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:improvisedChitinousArrow")
		{
			Name = "Arrows (chitin)",
			SummaryDescription = "Bunch of arrows fitted with sharp chitin heads and fletchings",
			Description = "We can use animal materials to make a more deadly arrow.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				AmmunitionType = new AmmunitionType
				{
					MaxNoOfRounds = 10,
					AmmoTags = new string[1] { "arrow" }
				},
				HauledItemValue = ItemType.HauledItemValues.MostValuable
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "improvisedEquipment",
				PartKeys = new SerializableDictionary<string, int> { { "item:improvisedArrowShaftBundle", 1 } }
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Security
			},
			CategoryKey = "ammunition",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "arrowShafts"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:improvisedMetalArrow")
		{
			Name = "Arrows (scrap metal)",
			SummaryDescription = "Bunch of arrows with fletchings and sharp heads made of metal",
			Description = "Pieces of scrap metal made into arrowheads and used on arrows.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				AmmunitionType = new AmmunitionType
				{
					MaxNoOfRounds = 10,
					AmmoTags = new string[1] { "arrow" }
				},
				HauledItemValue = ItemType.HauledItemValues.MostValuable
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "improvisedEquipment",
				PartKeys = new SerializableDictionary<string, int> { { "item:improvisedArrowShaftBundle", 1 } }
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Security
			},
			CategoryKey = "ammunition",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "arrowShafts"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:ironArrow")
		{
			Name = "Arrows (iron)",
			SummaryDescription = "Bunch of arrows with fletchings and sharp heads made of iron",
			Description = "Reliable arrows outfitted with sharp arrow heads made from wrought iron",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				AmmunitionType = new AmmunitionType
				{
					MaxNoOfRounds = 10,
					AmmoTags = new string[1] { "arrow" }
				},
				HauledItemValue = ItemType.HauledItemValues.MostValuable
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic",
				Area = RatingTypes.Security
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "improvisedEquipment",
				PartKeys = new SerializableDictionary<string, int> { { "item:improvisedArrowShaftBundle", 1 } }
			},
			CategoryKey = "ammunition",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "arrowShafts"
						}
					}
				}
			}
		});
		entityType = new EntityType("item:turnipCarcass")
		{
			Name = "Turnip carcass",
			SummaryDescription = "Carcass from the Turnip creature",
			Icon = "HUD_icon_carcass"
		};
		EntityType entityType7 = entityType;
		itemType = new ItemType
		{
			HasNoMaximumBulk = true
		};
		ItemType itemType3 = itemType;
		CarcassType carcassType = new CarcassType();
		itemType3.CarcassType = carcassType;
		entityType7.ItemType = itemType;
		entityType.NonLivingType = new NonLivingType
		{
			Repairability = 0f,
			DegradeType = "carcassDecomposing",
			DegradesTo = "item:organicMatter"
		};
		entityType.CategoryKey = "bodies";
		entityType.SubstancesType = new SubstancesType
		{
			SubstanceFractions = new SerializableDictionary<string, float>
			{
				{ "meat", 0.35f },
				{ "shell", 0.35f },
				{ "guts", 0.25f },
				{ "hide", 0.05f }
			}
		};
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "turnip",
				ModelScale = 2.5f,
				ModelBasicTextureName = "TurnipDarkTexture",
				DefaultInfo = new AnimConditionInfo
				{
					Looping = Looping.No,
					StartingPoint = StartingPoint.Specified,
					StartingPointInSeconds = 0.5f,
					SpeedFactor = 0f,
					SoundAndAnimationSet = new RandomSoundAndAnimationSet
					{
						BaseAnimations = new string[1] { "hide" }
					}
				}
			}
		};
		listOfEntityTypes.Add(entityType);
		entityType = new EntityType("item:body")
		{
			Name = "Human body",
			SummaryDescription = "A dead person's remains"
		};
		EntityType entityType8 = entityType;
		itemType = new ItemType
		{
			HasNoMaximumBulk = true
		};
		ItemType itemType4 = itemType;
		carcassType = new CarcassType();
		itemType4.CarcassType = carcassType;
		entityType8.ItemType = itemType;
		entityType.NonLivingType = new NonLivingType
		{
			Repairability = 0f,
			DegradeType = "carcassDecomposing",
			DegradesTo = "item:organicMatter"
		};
		entityType.CategoryKey = "bodies";
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "man",
				ModelScale = 2f,
				ModelBasicTextureName = "ManGrey1Texture",
				DefaultInfo = new AnimConditionInfo
				{
					Looping = Looping.No,
					SpeedFactor = 0f,
					SoundAndAnimationSet = new RandomSoundAndAnimationSet
					{
						BaseAnimations = new string[1] { "dead" }
					}
				}
			}
		};
		listOfEntityTypes.Add(entityType);
		entityType = new EntityType("item:quaditeCarcass")
		{
			Name = "Quadite carcass",
			SummaryDescription = "A dead quadite",
			Icon = "HUD_icon_carcass",
			Description = "The quadite carcass is quickly reduced to a husk, as its own parasites feed on their dead host."
		};
		EntityType entityType9 = entityType;
		itemType = new ItemType
		{
			HasNoMaximumBulk = true
		};
		ItemType itemType5 = itemType;
		carcassType = new CarcassType();
		itemType5.CarcassType = carcassType;
		entityType9.ItemType = itemType;
		entityType.NonLivingType = new NonLivingType
		{
			Repairability = 0f,
			DegradeType = "raw meat",
			DegradesTo = "item:twinklerPlating"
		};
		entityType.CategoryKey = "bodies";
		entityType.SubstancesType = new SubstancesType
		{
			SubstanceFractions = new SerializableDictionary<string, float>
			{
				{ "meat", 0.3f },
				{ "guts", 0.2f },
				{ "plating", 0.5f }
			}
		};
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "twinkler",
				ModelScale = 2.5f,
				ModelBasicTextureName = "QuaditeRedTexture",
				DefaultInfo = new AnimConditionInfo
				{
					Looping = Looping.No,
					SpeedFactor = 0f,
					SoundAndAnimationSet = new RandomSoundAndAnimationSet
					{
						BaseAnimations = new string[1] { "dead" }
					}
				}
			}
		};
		listOfEntityTypes.Add(entityType);
		entityType = new EntityType("item:swarmerCarcass")
		{
			Name = "Swarmer carcass",
			SummaryDescription = "A dead swarmer",
			Icon = "HUD_icon_carcass",
			Description = "The swarmer carcass is quickly reduced to a husk, as its own parasites feed on their dead host."
		};
		EntityType entityType10 = entityType;
		itemType = new ItemType
		{
			HasNoMaximumBulk = true
		};
		ItemType itemType6 = itemType;
		carcassType = new CarcassType();
		itemType6.CarcassType = carcassType;
		entityType10.ItemType = itemType;
		entityType.NonLivingType = new NonLivingType
		{
			Repairability = 0f,
			DegradeType = "raw meat",
			DegradesTo = "item:twinklerPlating"
		};
		entityType.CategoryKey = "bodies";
		entityType.SubstancesType = new SubstancesType
		{
			SubstanceFractions = new SerializableDictionary<string, float>
			{
				{ "meat", 0.3f },
				{ "guts", 0.2f },
				{ "plating", 0.5f }
			}
		};
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "twinkler",
				ModelScale = 2.5f,
				ModelBasicTextureName = "QuaditeRedTexture",
				DefaultInfo = new AnimConditionInfo
				{
					Looping = Looping.No,
					SpeedFactor = 0f,
					SoundAndAnimationSet = new RandomSoundAndAnimationSet
					{
						BaseAnimations = new string[1] { "dead" }
					}
				}
			}
		};
		listOfEntityTypes.Add(entityType);
		entityType = new EntityType("item:leafcutterCarcass")
		{
			Name = "Field quadite carcass",
			SummaryDescription = "A dead field quadite",
			Icon = "HUD_icon_carcass"
		};
		EntityType entityType11 = entityType;
		itemType = new ItemType
		{
			HasNoMaximumBulk = true
		};
		ItemType itemType7 = itemType;
		carcassType = new CarcassType();
		itemType7.CarcassType = carcassType;
		entityType11.ItemType = itemType;
		entityType.NonLivingType = new NonLivingType
		{
			Repairability = 0f,
			DegradeType = "carcassDecomposing",
			DegradesTo = "item:organicMatter"
		};
		entityType.CategoryKey = "bodies";
		entityType.SubstancesType = new SubstancesType
		{
			SubstanceFractions = new SerializableDictionary<string, float>
			{
				{ "meat", 0.3f },
				{ "guts", 0.2f },
				{ "plating", 0.5f }
			}
		};
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "twinklerThin",
				ModelScale = 0.1f,
				ModelBasicTextureName = "QuaditeThinTexture",
				DefaultInfo = new AnimConditionInfo
				{
					Looping = Looping.No,
					SpeedFactor = 0f,
					SoundAndAnimationSet = new RandomSoundAndAnimationSet
					{
						BaseAnimations = new string[1] { "dead" }
					}
				}
			}
		};
		listOfEntityTypes.Add(entityType);
		entityType = new EntityType("item:patricianCarcass")
		{
			Name = "Patrician carcass",
			SummaryDescription = "A dead patrician",
			Icon = "HUD_icon_carcass"
		};
		EntityType entityType12 = entityType;
		itemType = new ItemType
		{
			HasNoMaximumBulk = true
		};
		ItemType itemType8 = itemType;
		carcassType = new CarcassType();
		itemType8.CarcassType = carcassType;
		entityType12.ItemType = itemType;
		entityType.NonLivingType = new NonLivingType
		{
			Repairability = 0f,
			DegradeType = "carcassDecomposing",
			DegradesTo = "item:organicMatter"
		};
		entityType.CategoryKey = "bodies";
		entityType.SubstancesType = new SubstancesType
		{
			SubstanceFractions = new SerializableDictionary<string, float>
			{
				{ "meat", 0.5f },
				{ "bones", 0.2f },
				{ "guts", 0.2f },
				{ "hide", 0.1f }
			}
		};
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "patrician",
				ModelScale = 3f,
				DefaultInfo = new AnimConditionInfo
				{
					Looping = Looping.No,
					SpeedFactor = 0f,
					SoundAndAnimationSet = new RandomSoundAndAnimationSet
					{
						BaseAnimations = new string[1] { "dead" }
					}
				}
			}
		};
		listOfEntityTypes.Add(entityType);
		entityType = new EntityType("item:whipjawCarcass")
		{
			Name = "Whipjaw carcass",
			SummaryDescription = "A dead field whipjaw",
			Icon = "HUD_icon_carcass"
		};
		EntityType entityType13 = entityType;
		itemType = new ItemType
		{
			HasNoMaximumBulk = true
		};
		ItemType itemType9 = itemType;
		carcassType = new CarcassType();
		itemType9.CarcassType = carcassType;
		entityType13.ItemType = itemType;
		entityType.NonLivingType = new NonLivingType
		{
			Repairability = 0f,
			DegradeType = "carcassDecomposing",
			DegradesTo = "item:organicMatter"
		};
		entityType.CategoryKey = "bodies";
		entityType.SubstancesType = new SubstancesType
		{
			SubstanceFractions = new SerializableDictionary<string, float>
			{
				{ "meat", 0.5f },
				{ "bones", 0.2f },
				{ "guts", 0.2f },
				{ "hide", 0.1f }
			}
		};
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "snatcher",
				ModelScale = 3f,
				DefaultInfo = new AnimConditionInfo
				{
					Looping = Looping.No,
					SpeedFactor = 0f,
					SoundAndAnimationSet = new RandomSoundAndAnimationSet
					{
						BaseAnimations = new string[1] { "dead" }
					}
				}
			}
		};
		listOfEntityTypes.Add(entityType);
		entityType = new EntityType("item:mudWormCarcass")
		{
			Name = "Mud worm carcass",
			SummaryDescription = "A dead mud worm",
			Icon = "HUD_icon_carcass"
		};
		EntityType entityType14 = entityType;
		itemType = new ItemType
		{
			HasNoMaximumBulk = true
		};
		ItemType itemType10 = itemType;
		carcassType = new CarcassType();
		itemType10.CarcassType = carcassType;
		entityType14.ItemType = itemType;
		entityType.NonLivingType = new NonLivingType
		{
			Repairability = 0f,
			DegradeType = "carcassDecomposing",
			DegradesTo = "item:organicMatter"
		};
		entityType.CategoryKey = "bodies";
		entityType.SubstancesType = new SubstancesType
		{
			SubstanceFractions = new SerializableDictionary<string, float>
			{
				{ "meat", 0.9f },
				{ "guts", 0.1f }
			}
		};
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "worm",
				ModelScale = 0.4f,
				DefaultInfo = new AnimConditionInfo
				{
					Looping = Looping.No,
					SpeedFactor = 0f,
					SoundAndAnimationSet = new RandomSoundAndAnimationSet
					{
						BaseAnimations = new string[1] { "dead" }
					}
				}
			}
		};
		listOfEntityTypes.Add(entityType);
		entityType = new EntityType("item:demontreeCarcass")
		{
			Name = "Dendront carcass",
			SummaryDescription = "A dead dendront",
			Icon = "HUD_icon_carcass"
		};
		EntityType entityType15 = entityType;
		itemType = new ItemType
		{
			HasNoMaximumBulk = true
		};
		ItemType itemType11 = itemType;
		carcassType = new CarcassType();
		itemType11.CarcassType = carcassType;
		entityType15.ItemType = itemType;
		entityType.NonLivingType = new NonLivingType
		{
			Repairability = 0f,
			DegradeType = "carcassDecomposing",
			DegradesTo = "item:organicMatter"
		};
		entityType.CategoryKey = "bodies";
		entityType.SubstancesType = new SubstancesType
		{
			SubstanceFractions = new SerializableDictionary<string, float>
			{
				{ "meat", 0.5f },
				{ "bones", 0.2f },
				{ "guts", 0.2f },
				{ "hide", 0.1f }
			}
		};
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "demonTree",
				ModelScale = 3.8f,
				DefaultInfo = new AnimConditionInfo
				{
					Looping = Looping.No,
					SpeedFactor = 0f,
					SoundAndAnimationSet = new RandomSoundAndAnimationSet
					{
						BaseAnimations = new string[1] { "dead" }
					}
				}
			}
		};
		listOfEntityTypes.Add(entityType);
		entityType = new EntityType("item:swampDemonTreeCarcass")
		{
			Name = "Swamp dendront carcass",
			SummaryDescription = "A dead swamp dendront",
			Icon = "HUD_icon_carcass"
		};
		EntityType entityType16 = entityType;
		itemType = new ItemType
		{
			HasNoMaximumBulk = true
		};
		ItemType itemType12 = itemType;
		carcassType = new CarcassType();
		itemType12.CarcassType = carcassType;
		entityType16.ItemType = itemType;
		entityType.NonLivingType = new NonLivingType
		{
			Repairability = 0f,
			DegradeType = "carcassDecomposing",
			DegradesTo = "item:organicMatter"
		};
		entityType.CategoryKey = "bodies";
		entityType.SubstancesType = new SubstancesType
		{
			SubstanceFractions = new SerializableDictionary<string, float>
			{
				{ "meat", 0.5f },
				{ "bones", 0.2f },
				{ "guts", 0.2f },
				{ "hide", 0.1f }
			}
		};
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "demonTreeMoss",
				ModelBasicTextureName = "DemonTreeMossTexture1",
				ModelScale = 3.8f,
				DefaultInfo = new AnimConditionInfo
				{
					Looping = Looping.No,
					SpeedFactor = 0f,
					SoundAndAnimationSet = new RandomSoundAndAnimationSet
					{
						BaseAnimations = new string[1] { "dead" }
					}
				}
			}
		};
		listOfEntityTypes.Add(entityType);
		entityType = new EntityType("item:spikePlantCarcass")
		{
			Name = "Ursinix carcass",
			SummaryDescription = "A dead ursinix",
			Icon = "HUD_icon_carcass"
		};
		EntityType entityType17 = entityType;
		itemType = new ItemType
		{
			HasNoMaximumBulk = true
		};
		ItemType itemType13 = itemType;
		carcassType = new CarcassType();
		itemType13.CarcassType = carcassType;
		entityType17.ItemType = itemType;
		entityType.NonLivingType = new NonLivingType
		{
			Repairability = 0f,
			DegradeType = "carcassDecomposing",
			DegradesTo = "item:organicMatter"
		};
		entityType.CategoryKey = "bodies";
		entityType.SubstancesType = new SubstancesType
		{
			SubstanceFractions = new SerializableDictionary<string, float>
			{
				{ "meat", 0.5f },
				{ "bones", 0.2f },
				{ "guts", 0.2f },
				{ "hide", 0.1f }
			}
		};
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "spikePlant",
				ModelScale = 1.8f,
				DefaultInfo = new AnimConditionInfo
				{
					Looping = Looping.No,
					SpeedFactor = 0f,
					SoundAndAnimationSet = new RandomSoundAndAnimationSet
					{
						BaseAnimations = new string[1] { "dead" }
					}
				}
			}
		};
		listOfEntityTypes.Add(entityType);
		entityType = new EntityType("item:megapodCarcass")
		{
			Name = "Megapod carcass",
			SummaryDescription = "A dead megapod",
			Icon = "HUD_icon_carcass"
		};
		EntityType entityType18 = entityType;
		itemType = new ItemType
		{
			HasNoMaximumBulk = true
		};
		ItemType itemType14 = itemType;
		carcassType = new CarcassType();
		itemType14.CarcassType = carcassType;
		entityType18.ItemType = itemType;
		entityType.NonLivingType = new NonLivingType
		{
			Repairability = 0f,
			DegradeType = "carcassDecomposing",
			DegradesTo = "item:organicMatter"
		};
		entityType.CategoryKey = "bodies";
		entityType.SubstancesType = new SubstancesType
		{
			SubstanceFractions = new SerializableDictionary<string, float>
			{
				{ "meat", 0.5f },
				{ "bones", 0.2f },
				{ "guts", 0.2f },
				{ "hide", 0.1f }
			}
		};
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "worm",
				ModelScale = 1.8f,
				DefaultInfo = new AnimConditionInfo
				{
					Looping = Looping.No,
					SpeedFactor = 0f,
					SoundAndAnimationSet = new RandomSoundAndAnimationSet
					{
						BaseAnimations = new string[1] { "dead" }
					}
				}
			}
		};
		listOfEntityTypes.Add(entityType);
		entityType = new EntityType("item:forestGuardianCarcass")
		{
			Name = "Forest guardian carcass",
			SummaryDescription = "A dead forest guardian",
			Icon = "HUD_icon_carcass"
		};
		EntityType entityType19 = entityType;
		itemType = new ItemType
		{
			HasNoMaximumBulk = true
		};
		ItemType itemType15 = itemType;
		carcassType = new CarcassType();
		itemType15.CarcassType = carcassType;
		entityType19.ItemType = itemType;
		entityType.NonLivingType = new NonLivingType
		{
			Repairability = 0f,
			DegradeType = "carcassDecomposing",
			DegradesTo = "item:organicMatter"
		};
		entityType.CategoryKey = "bodies";
		entityType.SubstancesType = new SubstancesType
		{
			SubstanceFractions = new SerializableDictionary<string, float>
			{
				{ "meat", 0.5f },
				{ "bones", 0.2f },
				{ "guts", 0.2f },
				{ "hide", 0.1f }
			}
		};
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "forestguardian",
				ModelScale = 2.3f,
				DefaultInfo = new AnimConditionInfo
				{
					Looping = Looping.No,
					SpeedFactor = 0f,
					SoundAndAnimationSet = new RandomSoundAndAnimationSet
					{
						BaseAnimations = new string[1] { "dead" }
					}
				}
			}
		};
		listOfEntityTypes.Add(entityType);
		entityType = new EntityType("item:thunderChickenCarcass")
		{
			Name = "Thunder chicken carcass",
			SummaryDescription = "A dead thunder chicken",
			Description = "A valued source of food for humans and animals alike.",
			Icon = "HUD_icon_carcass"
		};
		EntityType entityType20 = entityType;
		itemType = new ItemType
		{
			HasNoMaximumBulk = true
		};
		ItemType itemType16 = itemType;
		carcassType = new CarcassType();
		itemType16.CarcassType = carcassType;
		entityType20.ItemType = itemType;
		entityType.NonLivingType = new NonLivingType
		{
			Repairability = 0f,
			DegradeType = "carcassDecomposing",
			DegradesTo = "item:organicMatter"
		};
		entityType.CategoryKey = "bodies";
		entityType.SubstancesType = new SubstancesType
		{
			SubstanceFractions = new SerializableDictionary<string, float>
			{
				{ "meat", 0.5f },
				{ "bones", 0.2f },
				{ "guts", 0.2f },
				{ "hide", 0.1f }
			}
		};
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "thunderchicken",
				ModelScale = 1.25f,
				ModelBasicTextureName = "ThunderchickenDarkTexture",
				DefaultInfo = new AnimConditionInfo
				{
					Looping = Looping.No,
					SpeedFactor = 0f,
					SoundAndAnimationSet = new RandomSoundAndAnimationSet
					{
						BaseAnimations = new string[1] { "dead" }
					}
				}
			}
		};
		listOfEntityTypes.Add(entityType);
		entityType = new EntityType("item:binalRatCarcass")
		{
			Name = "Binal rat carcass",
			SummaryDescription = "A dead binal rat",
			Icon = "HUD_icon_carcass",
			Description = "The meat of this animal is inedible by humans."
		};
		EntityType entityType21 = entityType;
		itemType = new ItemType
		{
			HasNoMaximumBulk = true
		};
		ItemType itemType17 = itemType;
		carcassType = new CarcassType();
		itemType17.CarcassType = carcassType;
		entityType21.ItemType = itemType;
		entityType.NonLivingType = new NonLivingType
		{
			Repairability = 0f,
			DegradeType = "raw meat",
			DegradesTo = "item:organicMatter"
		};
		entityType.CategoryKey = "bodies";
		entityType.SubstancesType = new SubstancesType
		{
			SubstanceFractions = new SerializableDictionary<string, float>
			{
				{ "meat", 0.5f },
				{ "bones", 0.2f },
				{ "guts", 0.2f },
				{ "hide", 0.1f }
			}
		};
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "thunderchicken",
				ModelScale = 1.5f,
				ModelBasicTextureName = "BinalRatBrownTexture",
				DefaultInfo = new AnimConditionInfo
				{
					Looping = Looping.No,
					SpeedFactor = 0f,
					SoundAndAnimationSet = new RandomSoundAndAnimationSet
					{
						BaseAnimations = new string[1] { "dead" }
					}
				}
			}
		};
		listOfEntityTypes.Add(entityType);
		entityType = new EntityType("item:dogCarcass")
		{
			Name = "Dog carcass",
			SummaryDescription = "A dead dog",
			Description = "N/A",
			Icon = "HUD_icon_carcass"
		};
		EntityType entityType22 = entityType;
		itemType = new ItemType
		{
			HasNoMaximumBulk = true
		};
		ItemType itemType18 = itemType;
		carcassType = new CarcassType();
		itemType18.CarcassType = carcassType;
		entityType22.ItemType = itemType;
		entityType.NonLivingType = new NonLivingType
		{
			Repairability = 0f,
			DegradeType = "carcassDecomposing",
			DegradesTo = "item:organicMatter"
		};
		entityType.CategoryKey = "bodies";
		entityType.SubstancesType = new SubstancesType
		{
			SubstanceFractions = new SerializableDictionary<string, float>
			{
				{ "meat", 0.5f },
				{ "bones", 0.2f },
				{ "guts", 0.2f },
				{ "hide", 0.1f }
			}
		};
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "dog",
				ModelScale = 1.5f,
				ModelBasicTextureName = "DogGermanShepherdTexture",
				DefaultInfo = new AnimConditionInfo
				{
					Looping = Looping.No,
					SpeedFactor = 0f,
					SoundAndAnimationSet = new RandomSoundAndAnimationSet
					{
						BaseAnimations = new string[1] { "dead" }
					}
				}
			}
		};
		listOfEntityTypes.Add(entityType);
		entityType = new EntityType("item:bushDragonCarcass")
		{
			Name = "Bush dragon carcass",
			SummaryDescription = "A dead bush dragon.",
			Description = "Inedible by humans but could find other uses.",
			Icon = "HUD_icon_carcass"
		};
		EntityType entityType23 = entityType;
		itemType = new ItemType
		{
			HasNoMaximumBulk = true
		};
		ItemType itemType19 = itemType;
		carcassType = new CarcassType();
		itemType19.CarcassType = carcassType;
		entityType23.ItemType = itemType;
		entityType.NonLivingType = new NonLivingType
		{
			Repairability = 0f,
			DegradeType = "carcassDecomposing",
			DegradesTo = "item:organicMatter"
		};
		entityType.CategoryKey = "bodies";
		entityType.SubstancesType = new SubstancesType
		{
			SubstanceFractions = new SerializableDictionary<string, float>
			{
				{ "meat", 0.5f },
				{ "bones", 0.2f },
				{ "guts", 0.2f },
				{ "hide", 0.1f }
			}
		};
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "bushdragon",
				ModelScale = 0.8f,
				ModelBasicTextureName = "BushdragonPaleTexture",
				DefaultInfo = new AnimConditionInfo
				{
					Looping = Looping.No,
					SpeedFactor = 0f,
					SoundAndAnimationSet = new RandomSoundAndAnimationSet
					{
						BaseAnimations = new string[1] { "dead" }
					}
				}
			}
		};
		listOfEntityTypes.Add(entityType);
		listOfEntityTypes.Add(new EntityType("item:bushDragonHarvestedCarcass")
		{
			Name = "Harvested bush dragon carcass",
			SummaryDescription = "A dead bush dragon whose organs of value to us have been extracted",
			Description = "The rest of this carcass has little use to us.",
			Icon = "HUD_icon_carcass",
			ItemType = new ItemType
			{
				HasNoMaximumBulk = true
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "carcassDecomposing",
				DegradesTo = "item:organicMatter"
			},
			CategoryKey = "bodies",
			SubstancesType = new SubstancesType
			{
				SubstanceFractions = new SerializableDictionary<string, float>
				{
					{ "meat", 0.5f },
					{ "bones", 0.2f },
					{ "guts", 0.2f },
					{ "hide", 0.1f }
				}
			},
			RenderableType = new RenderableType
			{
				RenderAsModelType = new RenderAsModelType
				{
					AssetName = "bushdragon",
					ModelScale = 0.8f,
					ModelBasicTextureName = "BushdragonPaleTexture",
					DefaultInfo = new AnimConditionInfo
					{
						Looping = Looping.No,
						SpeedFactor = 0f,
						SoundAndAnimationSet = new RandomSoundAndAnimationSet
						{
							BaseAnimations = new string[1] { "dead" }
						}
					}
				}
			}
		});
		entityType = new EntityType("item:birdCarcass")
		{
			Name = "Diamond bird carcass",
			SummaryDescription = "A dead diamond bird",
			Icon = "HUD_icon_carcass"
		};
		EntityType entityType24 = entityType;
		itemType = new ItemType
		{
			HasNoMaximumBulk = true
		};
		ItemType itemType20 = itemType;
		carcassType = new CarcassType();
		itemType20.CarcassType = carcassType;
		entityType24.ItemType = itemType;
		entityType.NonLivingType = new NonLivingType
		{
			Repairability = 0f,
			DegradeType = "carcassDecomposing",
			DegradesTo = "item:organicMatter"
		};
		entityType.CategoryKey = "bodies";
		entityType.RenderableType = new RenderableType
		{
			RenderAsModelType = new RenderAsModelType
			{
				AssetName = "bird",
				ModelScale = 2.1f,
				ModelBasicTextureName = "BirdYellowTexture"
			}
		};
		listOfEntityTypes.Add(entityType);
		listOfEntityTypes.Add(new EntityType("item:bellows")
		{
			Name = "Bellows",
			SummaryDescription = "Simple hand tool for blowing air. Used for ventilating a furnace",
			Description = "Consists of a flexible bag between two rigid boards. When the bag is compressed, air blows through the nozzle.",
			ToolType = new ToolType
			{
				Durability = 1f,
				ToolHandling = ToolHandlingType.HandTool,
				ToolTag = new string[1] { "blowTool" }
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.08f
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "bellows"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:blowpipe")
		{
			Name = "Blowpipe",
			SummaryDescription = "Hollow pipe for blowing air. Useful as simple ventilation in a low temperature furnace",
			Description = "A very basic form of furnace ventilation that depends on the lung capacity of the person manning it.",
			ToolType = new ToolType
			{
				Durability = 1f,
				ToolHandling = ToolHandlingType.HandTool,
				ToolTag = new string[1] { "blowTool" }
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "pole"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:turnipCracker")
		{
			Name = "Turnip cracker",
			SummaryDescription = "Jack-like item used for splitting a turnip's shell",
			Description = "Made of wrought iron. Consists of two heavy jaws connected to a notched lifting post. The jaws expand when a lever is turned in the same way that an old wagon jack functions. This makes it possible to split the sturdy shell of a turnip animal.",
			ToolType = new ToolType
			{
				Durability = 0.9f,
				ToolHandling = ToolHandlingType.HandTool
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.2f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "ironTools"
						}
					}
				}
			}
		});
		CreateDiamondKnife(listOfEntityTypes);
		listOfEntityTypes.Add(new EntityType("item:improvisedKnife")
		{
			Name = "Knife (scrap metal)",
			SummaryDescription = "A makeshift knife. Blade made from scrap metal.",
			Description = "",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ToolType = new ToolType
			{
				Durability = 0.9f,
				ToolHandling = ToolHandlingType.HandTool,
				ToolTag = new string[2] { "knife", "butcherFlesh" }
			},
			ItemType = new ItemType
			{
				HauledItemValue = ItemType.HauledItemValues.MostValuable,
				MaximumBulk = 0.05f,
				AttachedObjectRenderableType = "knife",
				AttachorTagToMountOn = "rightHand",
				AttachesToBodyPart = "Right arm",
				AnimStatesWhenAttached = new AnimModifier[1] { AnimModifier.Knife },
				WeaponType = new WeaponType
				{
					AttackTypes = new AttackType[1] { GameData.Instance.AllAttackTypes["improvisedKnifeHack"] }
				},
				TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>
				{
					{
						ItemType.TaskType.LongerJourneys,
						ItemType.AppropriateLevel.Normal
					},
					{
						ItemType.TaskType.UnspecifiedHunting,
						ItemType.AppropriateLevel.Minor
					},
					{
						ItemType.TaskType.PatrolOrAttack,
						ItemType.AppropriateLevel.Minor
					},
					{
						ItemType.TaskType.Scouting,
						ItemType.AppropriateLevel.Minor
					},
					{
						ItemType.TaskType.Hauling,
						ItemType.AppropriateLevel.Minor
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "knife"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:flintKnife")
		{
			Name = "Knife (flint)",
			SummaryDescription = "A primitive knife made from knapped flint",
			Description = "The stone blade has been painstakingly flaked by hand, then joined with a wooden handle to form a sharp but fragile cutting tool.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ToolType = new ToolType
			{
				Durability = 0.9f,
				ToolHandling = ToolHandlingType.HandTool,
				ToolTag = new string[2] { "knife", "butcherFlesh" }
			},
			ItemType = new ItemType
			{
				HauledItemValue = ItemType.HauledItemValues.MostValuable,
				MaximumBulk = 0.05f,
				AttachedObjectRenderableType = "knife",
				AttachorTagToMountOn = "rightHand",
				AttachesToBodyPart = "Right arm",
				AnimStatesWhenAttached = new AnimModifier[1] { AnimModifier.Knife },
				WeaponType = new WeaponType
				{
					AttackTypes = new AttackType[1] { GameData.Instance.AllAttackTypes["improvisedKnifeHack"] }
				},
				TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>
				{
					{
						ItemType.TaskType.LongerJourneys,
						ItemType.AppropriateLevel.Normal
					},
					{
						ItemType.TaskType.UnspecifiedHunting,
						ItemType.AppropriateLevel.Minor
					},
					{
						ItemType.TaskType.PatrolOrAttack,
						ItemType.AppropriateLevel.Minor
					},
					{
						ItemType.TaskType.Scouting,
						ItemType.AppropriateLevel.Minor
					},
					{
						ItemType.TaskType.Hauling,
						ItemType.AppropriateLevel.Minor
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "knife"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:steelKnife")
		{
			Name = "Knife (steel)",
			SummaryDescription = "A durable, handmade steel knife",
			Description = "Made with excellent craftsmanship.",
			ToolType = new ToolType
			{
				Durability = 0.9f,
				ToolHandling = ToolHandlingType.HandTool,
				ToolTag = new string[2] { "knife", "butcherFlesh" }
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ItemType = new ItemType
			{
				HauledItemValue = ItemType.HauledItemValues.MostValuable,
				MaximumBulk = 0.05f,
				AttachedObjectRenderableType = "knife",
				AttachorTagToMountOn = "rightHand",
				AttachesToBodyPart = "Right arm",
				AnimStatesWhenAttached = new AnimModifier[1] { AnimModifier.Knife },
				WeaponType = new WeaponType
				{
					AttackTypes = new AttackType[1] { GameData.Instance.AllAttackTypes["improvisedKnifeHack"] }
				},
				TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>
				{
					{
						ItemType.TaskType.LongerJourneys,
						ItemType.AppropriateLevel.Normal
					},
					{
						ItemType.TaskType.UnspecifiedHunting,
						ItemType.AppropriateLevel.Minor
					},
					{
						ItemType.TaskType.PatrolOrAttack,
						ItemType.AppropriateLevel.Minor
					},
					{
						ItemType.TaskType.Scouting,
						ItemType.AppropriateLevel.Minor
					},
					{
						ItemType.TaskType.Hauling,
						ItemType.AppropriateLevel.Minor
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment",
				DegradesTo = "item:blisterSteel"
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "knife"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:advancedMachete")
		{
			Name = "Machete (diamondoid carbon)",
			SummaryDescription = "Long cleaving/cutting tool. Exceptional quality",
			Description = "Developed for the Tau Ceti Program. Very versatile but is not well-suited for precision cutting because of its size.",
			ToolType = new ToolType
			{
				Durability = 0.9f,
				ToolHandling = ToolHandlingType.HandTool,
				ToolTag = new string[2] { "butcherFlesh", "cutThinShell" }
			},
			TierOrArea = new TierOrArea
			{
				Tier = "advanced"
			},
			ItemType = new ItemType
			{
				HauledItemValue = ItemType.HauledItemValues.MostValuable,
				MaximumBulk = 0.1f,
				AnimStatesWhenAttached = new AnimModifier[1] { AnimModifier.Machete },
				AttachorTagToMountOn = "rightHand",
				AttachesToBodyPart = "Right arm",
				AttachedObjectRenderableType = "machete",
				WeaponType = new WeaponType
				{
					AttackTypes = new AttackType[1] { GameData.Instance.AllAttackTypes["personHack"] }
				},
				TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>
				{
					{
						ItemType.TaskType.LongerJourneys,
						ItemType.AppropriateLevel.Minor
					},
					{
						ItemType.TaskType.UnspecifiedHunting,
						ItemType.AppropriateLevel.Normal
					},
					{
						ItemType.TaskType.PatrolOrAttack,
						ItemType.AppropriateLevel.Normal
					},
					{
						ItemType.TaskType.Scouting,
						ItemType.AppropriateLevel.Minor
					},
					{
						ItemType.TaskType.Hauling,
						ItemType.AppropriateLevel.Minor
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "machete"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:steelMachete")
		{
			Name = "Machete (steel)",
			SummaryDescription = "Long cleaving/cutting tool made of steel",
			Description = "Suited for cutting down soft types of vegetation",
			ToolType = new ToolType
			{
				Durability = 0.9f,
				ToolHandling = ToolHandlingType.HandTool,
				ToolTag = new string[2] { "butcherFlesh", "cutThinShell" }
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ItemType = new ItemType
			{
				HauledItemValue = ItemType.HauledItemValues.MostValuable,
				MaximumBulk = 0.12f,
				AnimStatesWhenAttached = new AnimModifier[1] { AnimModifier.Machete },
				AttachorTagToMountOn = "rightHand",
				AttachesToBodyPart = "Right arm",
				AttachedObjectRenderableType = "machete",
				WeaponType = new WeaponType
				{
					AttackTypes = new AttackType[1] { GameData.Instance.AllAttackTypes["personHack"] }
				},
				TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>
				{
					{
						ItemType.TaskType.LongerJourneys,
						ItemType.AppropriateLevel.Minor
					},
					{
						ItemType.TaskType.UnspecifiedHunting,
						ItemType.AppropriateLevel.Normal
					},
					{
						ItemType.TaskType.PatrolOrAttack,
						ItemType.AppropriateLevel.Normal
					},
					{
						ItemType.TaskType.Scouting,
						ItemType.AppropriateLevel.Minor
					},
					{
						ItemType.TaskType.Hauling,
						ItemType.AppropriateLevel.Minor
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment",
				DegradesTo = "item:blisterSteel"
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "machete"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:improvisedHandAxe")
		{
			Name = "Hand axe (scrap metal)",
			SummaryDescription = "Improvised small axe useful as a weapon and for chopping wood",
			Description = "Made from scrap metal.",
			ToolType = new ToolType
			{
				Durability = 0.9f,
				ToolHandling = ToolHandlingType.HandTool,
				ToolTag = new string[3] { "shapenSmallWood", "butcherFlesh", "cutThinShell" }
			},
			ItemType = new ItemType
			{
				HauledItemValue = ItemType.HauledItemValues.MostValuable,
				MaximumBulk = 0.1f,
				AttachedObjectRenderableType = "axe",
				AttachorTagToMountOn = "rightHand",
				AttachesToBodyPart = "Right arm",
				AnimStatesWhenAttached = new AnimModifier[1] { AnimModifier.Axe },
				WeaponType = new WeaponType
				{
					AttackTypes = new AttackType[1] { GameData.Instance.AllAttackTypes["personHack"] }
				},
				TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>
				{
					{
						ItemType.TaskType.LongerJourneys,
						ItemType.AppropriateLevel.Minor
					},
					{
						ItemType.TaskType.UnspecifiedHunting,
						ItemType.AppropriateLevel.Normal
					},
					{
						ItemType.TaskType.PatrolOrAttack,
						ItemType.AppropriateLevel.Normal
					},
					{
						ItemType.TaskType.Scouting,
						ItemType.AppropriateLevel.Minor
					},
					{
						ItemType.TaskType.Hauling,
						ItemType.AppropriateLevel.Minor
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "axe"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:steelHandAxe")
		{
			Name = "Hand axe (steel)",
			SummaryDescription = "Small axe, very useful for chopping hard wood",
			Description = "Made from steel, has excellent durability and sharpness",
			ToolType = new ToolType
			{
				Durability = 0.9f,
				ToolHandling = ToolHandlingType.HandTool,
				ToolTag = new string[3] { "shapenSmallWood", "butcherFlesh", "cutThinShell" }
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ItemType = new ItemType
			{
				HauledItemValue = ItemType.HauledItemValues.MostValuable,
				MaximumBulk = 0.1f,
				AttachedObjectRenderableType = "axe",
				AttachorTagToMountOn = "rightHand",
				AttachesToBodyPart = "Right arm",
				AnimStatesWhenAttached = new AnimModifier[1] { AnimModifier.Axe },
				WeaponType = new WeaponType
				{
					AttackTypes = new AttackType[1] { GameData.Instance.AllAttackTypes["personHack"] }
				},
				TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>
				{
					{
						ItemType.TaskType.LongerJourneys,
						ItemType.AppropriateLevel.Minor
					},
					{
						ItemType.TaskType.UnspecifiedHunting,
						ItemType.AppropriateLevel.Normal
					},
					{
						ItemType.TaskType.PatrolOrAttack,
						ItemType.AppropriateLevel.Normal
					},
					{
						ItemType.TaskType.Scouting,
						ItemType.AppropriateLevel.Minor
					},
					{
						ItemType.TaskType.Hauling,
						ItemType.AppropriateLevel.Minor
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment",
				DegradesTo = "item:blisterSteel"
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "axe"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:improvisedPickaxe")
		{
			Name = "Pickaxe (scrap metal)",
			SummaryDescription = "Improvised hand tool with a pointed head",
			Description = "Made from scrap metal, this tool is useful for loosening hard soil types and as a weapon, especially against animals with a hard shell.",
			ToolType = new ToolType
			{
				Durability = 0.9f,
				ToolHandling = ToolHandlingType.HandTool
			},
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ItemType = new ItemType
			{
				HauledItemValue = ItemType.HauledItemValues.MostValuable,
				MaximumBulk = 0.13f,
				AttachedObjectRenderableType = "pickaxe",
				AttachorTagToMountOn = "rightHand",
				AttachesToBodyPart = "Right arm",
				AnimStatesWhenAttached = new AnimModifier[1] { AnimModifier.Pickaxe },
				WeaponType = new WeaponType
				{
					AttackTypes = new AttackType[1] { GameData.Instance.AllAttackTypes["personHack"] }
				},
				TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>
				{
					{
						ItemType.TaskType.LongerJourneys,
						ItemType.AppropriateLevel.Minor
					},
					{
						ItemType.TaskType.UnspecifiedHunting,
						ItemType.AppropriateLevel.Normal
					},
					{
						ItemType.TaskType.PatrolOrAttack,
						ItemType.AppropriateLevel.Normal
					},
					{
						ItemType.TaskType.Scouting,
						ItemType.AppropriateLevel.Minor
					},
					{
						ItemType.TaskType.Hauling,
						ItemType.AppropriateLevel.Minor
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment",
				DegradesTo = "item:wroughtIron"
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "pickaxe"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:steelPickaxe")
		{
			Name = "Pickaxe (steel)",
			SummaryDescription = "Hand tool with a pointed head",
			Description = "Made from steel, this tool is useful for loosening hard soil types and as a weapon, especially against animals with a hard shell.",
			ToolType = new ToolType
			{
				Durability = 0.9f,
				ToolHandling = ToolHandlingType.HandTool
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ItemType = new ItemType
			{
				HauledItemValue = ItemType.HauledItemValues.MostValuable,
				MaximumBulk = 0.13f,
				AttachedObjectRenderableType = "pickaxe",
				AttachorTagToMountOn = "rightHand",
				AttachesToBodyPart = "Right arm",
				AnimStatesWhenAttached = new AnimModifier[1] { AnimModifier.Pickaxe },
				WeaponType = new WeaponType
				{
					AttackTypes = new AttackType[1] { GameData.Instance.AllAttackTypes["personHack"] }
				},
				TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>
				{
					{
						ItemType.TaskType.LongerJourneys,
						ItemType.AppropriateLevel.Minor
					},
					{
						ItemType.TaskType.UnspecifiedHunting,
						ItemType.AppropriateLevel.Normal
					},
					{
						ItemType.TaskType.PatrolOrAttack,
						ItemType.AppropriateLevel.Normal
					},
					{
						ItemType.TaskType.Scouting,
						ItemType.AppropriateLevel.Minor
					},
					{
						ItemType.TaskType.Hauling,
						ItemType.AppropriateLevel.Minor
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment",
				DegradesTo = "item:blisterSteel"
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "pickaxe"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:hammer")
		{
			Name = "Hammer (iron)",
			SummaryDescription = "A good quality hammer useful for smithing",
			Description = "N/A",
			ToolType = new ToolType
			{
				Durability = 0.9f,
				ToolHandling = ToolHandlingType.HandTool,
				ToolTag = new string[1] { "bluntTool" }
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ItemType = new ItemType
			{
				HauledItemValue = ItemType.HauledItemValues.MostValuable,
				MaximumBulk = 0.08f,
				AttachedObjectRenderableType = "hammer",
				AttachorTagToMountOn = "rightHand",
				AttachesToBodyPart = "Right arm",
				AnimStatesWhenAttached = new AnimModifier[1] { AnimModifier.Hammer },
				WeaponType = new WeaponType
				{
					AttackTypes = new AttackType[1] { GameData.Instance.AllAttackTypes["personHammerBlow"] }
				},
				TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>
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
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment",
				DegradesTo = "item:wroughtIron"
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "hammer"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:stoneHammer")
		{
			Name = "Hammer (stone)",
			SummaryDescription = "A very crude hammer, consisting of a large rock",
			Description = "Only useful for the most rudimentary work.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ToolType = new ToolType
			{
				Durability = 1f,
				ToolHandling = ToolHandlingType.HandTool,
				ToolTag = new string[1] { "bluntTool" }
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.25f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "dirt"
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "greyPouch"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:file")
		{
			Name = "File",
			SummaryDescription = "Hand tool for shaping metal and wood. Cuts away small amounts of material",
			Description = "A tool essential for blacksmithing",
			ToolType = new ToolType
			{
				Durability = 1f,
				ToolHandling = ToolHandlingType.HandTool
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.07f
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "ironTools"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:tongs")
		{
			Name = "Tongs",
			SummaryDescription = "Hand tool for gripping heavy and hot objects",
			Description = "These iron tongs are essential for blacksmithing.",
			ToolType = new ToolType
			{
				Durability = 1f,
				ToolHandling = ToolHandlingType.HandTool
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.07f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "ironTools"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:handDrill")
		{
			Name = "Hand drill",
			SummaryDescription = "Hand tool for boring and drilling in metal and wood",
			Description = "",
			ToolType = new ToolType
			{
				Durability = 1f,
				ToolHandling = ToolHandlingType.HandTool
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.07f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "ironTools"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:hacksaw")
		{
			Name = "Hacksaw",
			SummaryDescription = "Hand tool for sawing metal",
			Description = "",
			ToolType = new ToolType
			{
				Durability = 1f,
				ToolHandling = ToolHandlingType.HandTool
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.07f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "ironTools"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:bowSaw")
		{
			Name = "Bow saw",
			SummaryDescription = "Hand tool for sawing wood",
			Description = "",
			ToolType = new ToolType
			{
				Durability = 1f,
				ToolHandling = ToolHandlingType.HandTool
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.07f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "ironTools"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:blacksmithsToolbox")
		{
			Name = "Blacksmith's toolset",
			SummaryDescription = "A basic collection of tools used by the blacksmith",
			Description = "The toolbox contains a hammer, file and tongs",
			ToolType = new ToolType
			{
				Durability = 0.9f,
				ToolHandling = ToolHandlingType.HandTool
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ItemType = new ItemType
			{
				HauledItemValue = ItemType.HauledItemValues.MostValuable,
				MaximumBulk = 0.16f,
				AttachedObjectRenderableType = "hammer",
				AttachorTagToMountOn = "rightHand",
				AttachesToBodyPart = "Right arm",
				AnimStatesWhenAttached = new AnimModifier[1] { AnimModifier.Hammer }
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment",
				SalvageProcess = "salvageBlacksmithsToolbox",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:hammer", 1 },
					{ "item:file", 1 },
					{ "item:tongs", 1 }
				}
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "ironTools"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:metalWorkersToolbox")
		{
			Name = "Metal worker's toolset",
			SummaryDescription = "A versatile collection of tools used by the metalworker and the blacksmith",
			Description = "The toolbox contains a hammer, file, tongs, hand drill and hacksaw",
			ToolType = new ToolType
			{
				Durability = 0.9f,
				ToolHandling = ToolHandlingType.HandTool
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ItemType = new ItemType
			{
				HauledItemValue = ItemType.HauledItemValues.MostValuable,
				MaximumBulk = 0.16f,
				AttachedObjectRenderableType = "hammer",
				AttachorTagToMountOn = "rightHand",
				AttachesToBodyPart = "Right arm",
				AnimStatesWhenAttached = new AnimModifier[1] { AnimModifier.Hammer }
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment",
				SalvageProcess = "salvageMetalworkersToolbox",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:hammer", 1 },
					{ "item:file", 1 },
					{ "item:tongs", 1 },
					{ "item:handDrill", 1 },
					{ "item:hacksaw", 1 }
				}
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "ironTools"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:carpentersToolbox")
		{
			Name = "Carpenter's toolset",
			SummaryDescription = "A versatile collection of tools used by the carpenter",
			Description = "The toolbox contains a hammer, file, axe, hand drill and bow saw",
			ToolType = new ToolType
			{
				Durability = 0.9f,
				ToolHandling = ToolHandlingType.HandTool
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ItemType = new ItemType
			{
				HauledItemValue = ItemType.HauledItemValues.MostValuable,
				MaximumBulk = 0.16f,
				AttachedObjectRenderableType = "axe",
				AttachorTagToMountOn = "rightHand",
				AttachesToBodyPart = "Right arm",
				AnimStatesWhenAttached = new AnimModifier[1] { AnimModifier.Axe }
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment",
				SalvageProcess = "salvageCarpentersToolbox",
				PartKeys = new SerializableDictionary<string, int>
				{
					{ "item:hammer", 1 },
					{ "item:file", 1 },
					{ "item:steelHandAxe", 1 },
					{ "item:handDrill", 1 },
					{ "item:bowSaw", 1 }
				}
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "ironTools"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:farmingHoe")
		{
			Name = "Hoe (scrap metal)",
			SummaryDescription = "Improvised farming tool for weeding and tilling soil",
			Description = "Made from scrap metal. Sufficient for establishing a small farm plot",
			Icon = "hoe",
			ToolType = new ToolType
			{
				Durability = 1f,
				ToolHandling = ToolHandlingType.HandTool,
				ToolTag = new string[3] { "plowingTools", "unPlowingTools", "weedingTools" }
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ItemType = new ItemType
			{
				HauledItemValue = ItemType.HauledItemValues.MostValuable,
				MaximumBulk = 0.1f,
				AnimStatesWhenAttached = new AnimModifier[1] { AnimModifier.Hoe },
				AttachorTagToMountOn = "rightHand",
				AttachesToBodyPart = "Right arm",
				AttachedObjectRenderableType = "farmingHoe",
				TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>
				{
					{
						ItemType.TaskType.LongerJourneys,
						ItemType.AppropriateLevel.None
					},
					{
						ItemType.TaskType.PatrolOrAttack,
						ItemType.AppropriateLevel.Minor
					},
					{
						ItemType.TaskType.UnspecifiedHunting,
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
				},
				WeaponType = new WeaponType
				{
					AttackTypes = new AttackType[1] { GameData.Instance.AllAttackTypes["personHoeHack"] }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				RenderAsModelType = new RenderAsModelType
				{
					AssetName = "farmingHoe",
					ModelScale = 2f
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:steelHoe")
		{
			Name = "Hoe (steel)",
			SummaryDescription = "Farming tool for weeding and tilling soil",
			Description = "Sufficient for establishing a small farm plot.",
			Icon = "hoe",
			ToolType = new ToolType
			{
				Durability = 1f,
				ToolHandling = ToolHandlingType.HandTool,
				ToolTag = new string[3] { "plowingTools", "unPlowingTools", "weedingTools" }
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ItemType = new ItemType
			{
				HauledItemValue = ItemType.HauledItemValues.MostValuable,
				MaximumBulk = 0.1f,
				AnimStatesWhenAttached = new AnimModifier[1] { AnimModifier.Hoe },
				AttachorTagToMountOn = "rightHand",
				AttachesToBodyPart = "Right arm",
				AttachedObjectRenderableType = "farmingHoe",
				TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>
				{
					{
						ItemType.TaskType.LongerJourneys,
						ItemType.AppropriateLevel.None
					},
					{
						ItemType.TaskType.PatrolOrAttack,
						ItemType.AppropriateLevel.Minor
					},
					{
						ItemType.TaskType.UnspecifiedHunting,
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
				},
				WeaponType = new WeaponType
				{
					AttackTypes = new AttackType[1] { GameData.Instance.AllAttackTypes["personHoeHack"] }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment",
				DegradesTo = "item:blisterSteel"
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				RenderAsModelType = new RenderAsModelType
				{
					AssetName = "farmingHoe",
					ModelScale = 2f
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:improvisedSpade")
		{
			Name = "Spade (scrap metal)",
			SummaryDescription = "Spade made from improvised materials",
			Icon = "shovel",
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ToolType = new ToolType
			{
				Durability = 0.9f,
				ToolHandling = ToolHandlingType.HandTool,
				ToolTag = new string[3] { "plowingTools", "unPlowingTools", "diggingSoil" }
			},
			ItemType = new ItemType
			{
				HauledItemValue = ItemType.HauledItemValues.MostValuable,
				MaximumBulk = 0.1f,
				AttachedObjectRenderableType = "shovel",
				AttachorTagToMountOn = "rightHand",
				AttachesToBodyPart = "Right arm",
				AnimStatesWhenAttached = new AnimModifier[1] { AnimModifier.Shovel },
				TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>
				{
					{
						ItemType.TaskType.LongerJourneys,
						ItemType.AppropriateLevel.None
					},
					{
						ItemType.TaskType.PatrolOrAttack,
						ItemType.AppropriateLevel.None
					},
					{
						ItemType.TaskType.UnspecifiedHunting,
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
				},
				WeaponType = new WeaponType
				{
					AttackTypes = new AttackType[1] { GameData.Instance.AllAttackTypes["personShovelHack"] }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				RenderAsModelType = new RenderAsModelType
				{
					AssetName = "shovel",
					ModelScale = 2f
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:steelSpade")
		{
			Name = "Spade (steel)",
			SummaryDescription = "Spade with a steel blade",
			Description = "Very useful for digging. Has excellent durability and sharpness.",
			Icon = "shovel",
			ToolType = new ToolType
			{
				Durability = 0.9f,
				ToolHandling = ToolHandlingType.HandTool,
				ToolTag = new string[3] { "plowingTools", "unPlowingTools", "diggingSoil" }
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ItemType = new ItemType
			{
				HauledItemValue = ItemType.HauledItemValues.MostValuable,
				MaximumBulk = 0.125f,
				AttachedObjectRenderableType = "shovel",
				AttachorTagToMountOn = "rightHand",
				AttachesToBodyPart = "Right arm",
				AnimStatesWhenAttached = new AnimModifier[1] { AnimModifier.Shovel },
				TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>
				{
					{
						ItemType.TaskType.LongerJourneys,
						ItemType.AppropriateLevel.None
					},
					{
						ItemType.TaskType.PatrolOrAttack,
						ItemType.AppropriateLevel.None
					},
					{
						ItemType.TaskType.UnspecifiedHunting,
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
				},
				WeaponType = new WeaponType
				{
					AttackTypes = new AttackType[1] { GameData.Instance.AllAttackTypes["personShovelHack"] }
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment",
				DegradesTo = "item:blisterSteel"
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				RenderAsModelType = new RenderAsModelType
				{
					AssetName = "shovel",
					ModelScale = 2f
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:improvisedTrowel")
		{
			Name = "Trowel (improvised)",
			SummaryDescription = "Very simple digging tool, consisting of a flat stick",
			Description = "When lacking a spade, this stick can be used for loosening hard soil before moving it by hand",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ToolType = new ToolType
			{
				Durability = 1f,
				ToolHandling = ToolHandlingType.HandTool,
				ToolTag = new string[1] { "diggingSoil" }
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.07f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "bushcraftComponentsSmall"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:bluntKnife")
		{
			Name = "Blunt knife",
			SummaryDescription = "Wooden tool for scraping and shaping soft materials",
			Description = "Useful for shaping pottery and scraping animal hides, for example.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ToolType = new ToolType
			{
				Durability = 1f,
				ToolHandling = ToolHandlingType.HandTool,
				ToolTag = new string[1] { "bluntKnife" }
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.07f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "bushcraftComponentsSmall"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:vinegar")
		{
			Name = "Vinegar",
			SummaryDescription = "A mild, edible acid useful for preservation of food",
			Description = "Like on Earth, many naturally occuring bacteria here can produce vinegar by fermentation of fruit. The vinegar can be used for conserving food (pickling). When making more vinegar, the production is greatly sped up by using a preexisting batch of vinegar (bacterial culture) to kickstart the process.",
			ToolType = new ToolType
			{
				Durability = 0.2f,
				ToolHandling = ToolHandlingType.HandTool
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.125f,
				RequiredStorageTags = new string[1] { "storageTagLiquidContainerClosedNoHeat" },
				FoodType = new FoodType
				{
					FoodNutrientProfile = GameData.Instance.AllFoodNutrientProfiles["poorVegetables"],
					FoodTags = new string[1] { "inedibleIngredient" }
				}
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic",
				Area = RatingTypes.Food
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "pickledFood",
				DegradesTo = "item:organicMatter"
			},
			CategoryKey = "ingredients",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "vegetables"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:shadeleafResin")
		{
			Name = "Shadeleaf resin",
			SummaryDescription = "A resinous gum which works a glue and protects materials from bug infestation.",
			Description = "Sticky, colourless gum that turns to a transparent mass when the essential oils have been allowed to evaporate. Can be used for joining light objects. Also has a repellent effect on scuttler bugs and can be applied to plant material to protect against infestation",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ToolType = new ToolType
			{
				Durability = 0.9f,
				ToolHandling = ToolHandlingType.HandTool,
				ToolTag = new string[2] { "improvisedGlue", "combineLightImprovisedObjects" }
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.07f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "improvisedEquipment"
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "leatherPouch"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:advancedString")
		{
			Name = "String (high-tech)",
			SummaryDescription = "Ultra strong and lightweight string",
			Description = "Among the Tau Ceti Mission standard equipment. Its multifilament construction offers extreme power and durability and high tensile strength.",
			TierOrArea = new TierOrArea
			{
				Tier = "advanced"
			},
			ToolType = new ToolType
			{
				Durability = 1f,
				ToolHandling = ToolHandlingType.HandTool,
				ToolTag = new string[2] { "lightString", "cordage" }
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.04f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "equipment"
			},
			CategoryKey = "tools",
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
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:metalWire")
		{
			Name = "Metal wire",
			SummaryDescription = "A small roll of metal wire",
			Description = "Wraps around and stays in place - should be useful for all kinds of small construction tasks",
			TierOrArea = new TierOrArea
			{
				Tier = "medium"
			},
			ToolType = new ToolType
			{
				Durability = 0.9f,
				ToolHandling = ToolHandlingType.HandTool,
				ToolTag = new string[1] { "lightString" }
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.04f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "equipment"
			},
			CategoryKey = "tools",
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
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:rawhideString")
		{
			Name = "String (rawhide)",
			SummaryDescription = "Crude string made from rawhide",
			Description = "Adequate for simple construction tasks. Stiff when dry.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ToolType = new ToolType
			{
				Durability = 1f,
				ToolHandling = ToolHandlingType.HandTool,
				ToolTag = new string[2] { "lightString", "cordage" }
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "improvisedEquipment"
			},
			CategoryKey = "tools",
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
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:cottonString")
		{
			Name = "String (cotton)",
			SummaryDescription = "Sturdy string made from cotton",
			Description = "Can be used both as a tool for small crafting tasks and as material for a fishing net.",
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ToolType = new ToolType
			{
				Durability = 1f,
				ToolHandling = ToolHandlingType.HandTool,
				ToolTag = new string[2] { "lightString", "cordage" }
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "equipment"
			},
			CategoryKey = "tools",
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
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:vine")
		{
			Name = "Vine",
			SummaryDescription = "Rope-like stems of a climbing plant",
			Description = "This vine is reasonably durable and strong and makes a natural cordage that can be used in shelter construction.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ToolType = new ToolType
			{
				Durability = 0.2f,
				ToolHandling = ToolHandlingType.HandTool,
				ToolTag = new string[1] { "cordage" }
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.07f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "equipment"
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "bushcraftComponentsSmall"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:superconductingWire")
		{
			Name = "Superconducting wire",
			SummaryDescription = "Pieces of electrical wiring with very low resistance.",
			Description = "Although designed for electric power transmission, this wire has enough ductility and tensile strength that it may find use in simple construction tasks.",
			ToolType = new ToolType
			{
				Durability = 0.9f,
				ToolHandling = ToolHandlingType.HandTool,
				ToolTag = new string[2] { "lightString", "cordage" }
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.04f
			},
			TierOrArea = new TierOrArea
			{
				Tier = "advanced"
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "equipment"
			},
			CategoryKey = "tools",
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
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:brickMold")
		{
			Name = "Brick mold",
			SummaryDescription = "A simple wooden mold used for shaping rectangular mudbricks",
			Description = "A very simple tool, basically a frame made from wood",
			ToolType = new ToolType
			{
				Durability = 1f,
				ToolHandling = ToolHandlingType.HandTool,
				ToolTag = new string[1] { "mold" }
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.06f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "brickMold"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:bulletMold")
		{
			Name = "Bullet mold",
			SummaryDescription = "A simple ceramic mold used for casting bullets",
			Description = "The ceramic is made from specially selected clay and can withstand the temperature of melted gold. The hot, molten metal is poured into the holes in the mold and allowed to cool. This method produces bullets of a sufficient quality.",
			ToolType = new ToolType
			{
				Durability = 1f,
				ToolHandling = ToolHandlingType.HandTool
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.09f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "greyPouch"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:sandMold")
		{
			Name = "Sand mold",
			SummaryDescription = "Mold used for sand casting metal",
			Description = "A wooden frame (also called a flask) filled with a temperature resistant molding sand (greensand). The flask is used together with a casting pattern to form a mold into which the hot metal is poured.",
			ToolType = new ToolType
			{
				Durability = 1f,
				ToolHandling = ToolHandlingType.HandTool,
				ToolTag = new string[1] { "mold" }
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.09f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "improvisedEquipment"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "brickMold"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:ursinixVenomGland")
		{
			Name = "Ursinix venom gland",
			SummaryDescription = "This animal organ contains a large amount of strong neurotoxin",
			Description = "The poison contained in this tissue might find use for us, but it should be handled with great care. This rapid-acting poison could be used when fishing for small fish by throwing it in the water.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Food
			},
			ToolType = new ToolType
			{
				Durability = 0.9f,
				ToolHandling = ToolHandlingType.HandTool
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "perishable",
				DegradesTo = "item:organicMatter"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "leatherPouch"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:neonHornetsLive")
		{
			Name = "Live neon hornets",
			SummaryDescription = "Flying insects that emit a strong glow",
			Description = "\n BIOLOGY OVERVIEW\n Neon hornets are aggressive, hive-building insects with thoracic bioluminescence.\n \nSURVIVAL GUIDE NOTES\n Their bioluminescence resembles that of the sparkscale, a small fish species that is the primary prey of the carbon tail. Hence, the neon hornet might be useful as bait to catch the carbon tail.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Food
			},
			ToolType = new ToolType
			{
				Durability = 0.9f,
				ToolHandling = ToolHandlingType.HandTool
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "raw seafood",
				DegradesTo = "item:neonHornetsDead"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "leatherPouch"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:neonHornetsDead")
		{
			Name = "Dead neon hornets",
			SummaryDescription = "These dead insects no longer glow",
			Description = "Can be used as bait for fishing. We've found that the fish 'carbon tail' has a preference for neon hornets",
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Food
			},
			ToolType = new ToolType
			{
				Durability = 0.9f,
				ToolHandling = ToolHandlingType.HandTool
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.03f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "perishable",
				DegradesTo = "item:organicMatter"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "leatherPouch"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:pigFliesLive")
		{
			Name = "Live pig flies",
			SummaryDescription = "Flying insect often found on copperfern",
			Description = "The pig fly is named for the unusual snout-like sense organ it uses to find honeydew amongst the copperferns it frequently inhabits. Its quick, dance-like movement above water surfaces attracts the streak fin, which feeds on flying insects.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Food
			},
			ToolType = new ToolType
			{
				Durability = 0.9f,
				ToolHandling = ToolHandlingType.HandTool
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "raw seafood",
				DegradesTo = "item:pigFliesDead"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "leatherPouch"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:pigFliesDead")
		{
			Name = "Dead pig flies",
			SummaryDescription = "Can be used as fishing bait",
			Description = "We've found that the fish streak fin has a preference for pig flies",
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Food
			},
			ToolType = new ToolType
			{
				Durability = 0.9f,
				ToolHandling = ToolHandlingType.HandTool
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.03f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "perishable",
				DegradesTo = "item:organicMatter"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "leatherPouch"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:exaGlue")
		{
			Name = "Exa glue",
			SummaryDescription = "Ultra strong adhesive",
			Description = "Issued for the Tau Ceti Mission to aid in smaller construction tasks. Will bind almost all surfaces.",
			TierOrArea = new TierOrArea
			{
				Tier = "advanced"
			},
			ToolType = new ToolType
			{
				Durability = 0.9f,
				ToolHandling = ToolHandlingType.HandTool,
				ToolTag = new string[1] { "combineLightImprovisedObjects" }
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.02f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "greyPouch"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:smoothSandstone")
		{
			Name = "Sharpening stones",
			SummaryDescription = "Smooth sedimentary rocks suited for sharpening metal",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ToolType = new ToolType
			{
				Durability = 0.9f,
				ToolHandling = ToolHandlingType.HandTool,
				ToolTag = new string[1] { "sharpenBlade" }
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "bushcraftComponentsSmall"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:metalCutter")
		{
			Name = "Metal cutter",
			SummaryDescription = "A tool like this would come in handy",
			ToolType = new ToolType
			{
				Durability = 0.9f,
				ToolHandling = ToolHandlingType.Stationary,
				ToolTag = new string[1] { "cutMetal" }
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.3f
			},
			TierOrArea = new TierOrArea
			{
				Tier = "advanced"
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "powertool"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:tinnerSnips")
		{
			Name = "Tinner snips",
			SummaryDescription = "Simple, steel hand tool for cutting metal",
			Description = "A type of scissors with long handles and short strong blades, able to cut thin metal sheets and small metal objects.",
			ToolType = new ToolType
			{
				Durability = 0.9f,
				ToolHandling = ToolHandlingType.HandTool,
				ToolTag = new string[1] { "cutMetal" }
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "ironTools"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:advancedSnips")
		{
			Name = "Metal shears",
			SummaryDescription = "Advanced hand tool for cutting metal",
			Description = "Part of the Tau Ceti Mission equipment, this tool can cut both soft and tough metal types.",
			ToolType = new ToolType
			{
				Durability = 0.9f,
				ToolHandling = ToolHandlingType.HandTool,
				ToolTag = new string[1] { "cutMetal" }
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.07f
			},
			TierOrArea = new TierOrArea
			{
				Tier = "advanced"
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "knife"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:sentryLaserGun")
		{
			Name = "Stationary machine gun component",
			SummaryDescription = "small laser for the sentry robot",
			Description = "The weapon component of the sentry turret",
			ItemType = new ItemType
			{
				HauledItemValue = ItemType.HauledItemValues.MostValuable,
				MaximumBulk = 0.2f,
				TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>
				{
					{
						ItemType.TaskType.LongerJourneys,
						ItemType.AppropriateLevel.Best
					},
					{
						ItemType.TaskType.PatrolOrAttack,
						ItemType.AppropriateLevel.Best
					},
					{
						ItemType.TaskType.UnspecifiedHunting,
						ItemType.AppropriateLevel.None
					},
					{
						ItemType.TaskType.Scouting,
						ItemType.AppropriateLevel.Best
					},
					{
						ItemType.TaskType.Hauling,
						ItemType.AppropriateLevel.None
					}
				},
				WeaponType = new WeaponType
				{
					AttackTypes = new AttackType[1] { GameData.Instance.AllAttackTypes["sentryLaserGunShot"] },
					IsIntrinsic = true
				}
			},
			TierOrArea = new TierOrArea
			{
				Tier = "advanced",
				Area = RatingTypes.Security
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "equipment"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:sentryGun")
		{
			Name = "Stationary machine gun component",
			SummaryDescription = "Machinegun for the sentry robot",
			Description = "The weapon component of the sentry turret",
			ItemType = new ItemType
			{
				HauledItemValue = ItemType.HauledItemValues.MostValuable,
				MaximumBulk = 0.2f,
				TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>
				{
					{
						ItemType.TaskType.LongerJourneys,
						ItemType.AppropriateLevel.Best
					},
					{
						ItemType.TaskType.PatrolOrAttack,
						ItemType.AppropriateLevel.Best
					},
					{
						ItemType.TaskType.UnspecifiedHunting,
						ItemType.AppropriateLevel.None
					},
					{
						ItemType.TaskType.Scouting,
						ItemType.AppropriateLevel.Best
					},
					{
						ItemType.TaskType.Hauling,
						ItemType.AppropriateLevel.None
					}
				},
				WeaponType = new WeaponType
				{
					AttackTypes = new AttackType[1] { GameData.Instance.AllAttackTypes["sentryGunBurst"] },
					IsIntrinsic = true
				}
			},
			TierOrArea = new TierOrArea
			{
				Tier = "advanced",
				Area = RatingTypes.Security
			},
			ContainerType = new MagazineContainerType
			{
				CanTransactWithTags = new string[1] { "humanTransact" },
				MaxCapacity = 100,
				UsesAmmoTypeKeyName = "item:sentryGunAmmo",
				ReplenishProcess = "reloadSentryGun"
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "equipment"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:sentrySprayGun")
		{
			Name = "Stationary spray gun component",
			SummaryDescription = "Improvised spray gun for the sentry turret",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			Description = "With some clever engineering, this can be fitted onto the sentry robot",
			ItemType = new ItemType
			{
				HauledItemValue = ItemType.HauledItemValues.MostValuable,
				MaximumBulk = 0.2f,
				WeaponType = new WeaponType
				{
					AttackTypes = new AttackType[1] { GameData.Instance.AllAttackTypes["shootImprovedFireExtinguisherBushDragonPoison"] },
					IsIntrinsic = true
				}
			},
			ContainerType = new MagazineContainerType
			{
				CanTransactWithTags = new string[1] { "humanTransact" },
				MaxCapacity = 10,
				UsesAmmoTag = "fireExtinguisherAmmo",
				ReplenishProcess = "reloadSentryGun"
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "equipment"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:sentryShotgun")
		{
			Name = "Mountable shotgun",
			SummaryDescription = "Shotgun, modified for the sentry turret",
			Description = "With some minor adjustments, this weapon can be fitted onto the sentry robot",
			TierOrArea = new TierOrArea
			{
				Tier = "advanced"
			},
			ItemType = new ItemType
			{
				HauledItemValue = ItemType.HauledItemValues.MostValuable,
				MaximumBulk = 0.2f,
				WeaponType = new WeaponType
				{
					AttackTypes = new AttackType[1] { GameData.Instance.AllAttackTypes["shootShotgun"] },
					IsIntrinsic = true
				}
			},
			ContainerType = new MagazineContainerType
			{
				CanTransactWithTags = new string[1] { "humanTransact" },
				MaxCapacity = 8,
				UsesAmmoTypeKeyName = "item:shotgunAmmo",
				ReplenishProcess = "reloadSentryGun"
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "equipment"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:weedingRobotTool")
		{
			Name = "GOPHER robot tool system",
			SummaryDescription = "Robot mounted tools for harvesting and weeding",
			Description = "Agricultural robot tool suite for harvesting crops, weeding and controlling pests",
			ToolType = new ToolType
			{
				Durability = 1f,
				ToolHandling = ToolHandlingType.Intrinsic,
				ToolTag = new string[1] { "weedingTools" }
			},
			TierOrArea = new TierOrArea
			{
				Tier = "advanced"
			},
			ItemType = new ItemType
			{
				HauledItemValue = ItemType.HauledItemValues.MostValuable,
				MaximumBulk = 0.2f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "equipment"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:diggingRobotTool")
		{
			Name = "Digging system",
			SummaryDescription = "Robot mounted system for digging",
			Description = "Digging robot tool suite",
			ToolType = new ToolType
			{
				Durability = 1f,
				ToolHandling = ToolHandlingType.Intrinsic,
				ToolTag = new string[1] { "miningTools" }
			},
			TierOrArea = new TierOrArea
			{
				Tier = "advanced"
			},
			ItemType = new ItemType
			{
				HauledItemValue = ItemType.HauledItemValues.MostValuable,
				MaximumBulk = 0.2f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "equipment"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:advancedCookingPot")
		{
			Name = "Cooking pot (titanium)",
			SummaryDescription = "Light weight metal pot",
			Description = "Made of titanium, this cooking tool is an essential part of the standard issue Tau Ceti equipment.",
			ToolType = new ToolType
			{
				ToolHandling = ToolHandlingType.Stationary,
				Durability = 0.5f,
				ToolTag = new string[1] { "cookingPot" }
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.09f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "advanced",
				Area = RatingTypes.Food
			},
			ContainerType = new ToolContainerType
			{
				CanTransactWithTags = new string[8] { "humanTransact", "robotTransact", "ratTransact", "leafcutterTransact", "chickenTransact", "snatcherTransact", "twinklerTransact", "demonTreeTransact" },
				ProductionOutputStorageType = new ItemStorageType(0.2f)
				{
					FullStatePercentage = 0.1f
				}
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "pot"
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
								AssetName = "potFull"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 16)
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:improvisedCookingPot")
		{
			Name = "Cooking pot (scrap metal)",
			SummaryDescription = "Metal pot made from scrap metal",
			Description = "An improvised metal container for cooking.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ToolType = new ToolType
			{
				ToolTag = new string[1] { "cookingPot" },
				ToolHandling = ToolHandlingType.Stationary,
				Durability = 0.5f
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0.7f,
				DegradeType = "equipment"
			},
			ContainerType = new ToolContainerType
			{
				CanTransactWithTags = new string[8] { "humanTransact", "robotTransact", "ratTransact", "leafcutterTransact", "chickenTransact", "snatcherTransact", "twinklerTransact", "demonTreeTransact" },
				ProductionOutputStorageType = new ItemStorageType(0.2f)
				{
					FullStatePercentage = 0.1f
				}
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "pot"
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
								AssetName = "potFull"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 16)
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:goldPot")
		{
			Name = "Cooking pot (gold)",
			SummaryDescription = "Cooking pot made from cast gold",
			Description = "Gold is plentiful on our planet and can be used for mundane items such as this. It's a heavy, good quality cooking pot which benefits from the special properties of gold.",
			ToolType = new ToolType
			{
				ToolHandling = ToolHandlingType.Stationary,
				Durability = 0.5f,
				ToolTag = new string[1] { "cookingPot" }
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.14f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ContainerType = new ToolContainerType
			{
				CanTransactWithTags = new string[8] { "humanTransact", "robotTransact", "ratTransact", "leafcutterTransact", "chickenTransact", "snatcherTransact", "twinklerTransact", "demonTreeTransact" },
				ProductionOutputStorageType = new ItemStorageType(0.2f)
				{
					FullStatePercentage = 0.1f
				}
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "potGoldenEmpty"
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
								AssetName = "potGoldenFull"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 16)
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:woodenCookingPot")
		{
			Name = "Cooking pot (wooden)",
			SummaryDescription = "Cooking pot made from plant materials. Less efficient than a metal pot.",
			Description = "This wooden vessel will burn if placed directly on a fire. Instead, food can be cooked in the pot by gradually placing hot stones in the pot together with water and ingredients. Cooking food this way requires a steady supply of stones heated in a nearby fire.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ToolType = new ToolType
			{
				ToolTag = new string[1] { "cookingPot" },
				ToolHandling = ToolHandlingType.Stationary,
				Durability = 0.5f
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0.7f,
				DegradeType = "equipment"
			},
			ContainerType = new ToolContainerType
			{
				CanTransactWithTags = new string[8] { "humanTransact", "robotTransact", "ratTransact", "leafcutterTransact", "chickenTransact", "snatcherTransact", "twinklerTransact", "demonTreeTransact" },
				ProductionOutputStorageType = new ItemStorageType(0.2f)
				{
					FullStatePercentage = 0.1f
				}
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "woodenPot"
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
								AssetName = "woodenPotFull"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 16)
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:clayPotUnglazed")
		{
			Name = "Cooking pot (unglazed clay)",
			SummaryDescription = "A small clay pot for cooking",
			Description = "This clay item has low quality but can be used for cooking if care is taken during use. Since it has not been glazed it is not ideal for storing liquids.",
			ToolType = new ToolType
			{
				ToolTag = new string[2] { "cookingPot", "liquidContainerNoHeat" },
				ToolHandling = ToolHandlingType.Stationary,
				Durability = 0.5f
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0.7f,
				DegradeType = "equipment"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ContainerType = new ToolContainerType
			{
				CanTransactWithTags = new string[8] { "humanTransact", "robotTransact", "ratTransact", "leafcutterTransact", "chickenTransact", "snatcherTransact", "twinklerTransact", "demonTreeTransact" },
				ProductionOutputStorageType = new ItemStorageType(0.2f)
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "clayPotSmall"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:vat")
		{
			Name = "Vat",
			SummaryDescription = "A simple container for storing liquid",
			Description = "Made from improvised materials.",
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ToolType = new ToolType
			{
				ToolTag = new string[1] { "liquidContainerNoHeat" },
				ToolHandling = ToolHandlingType.Stationary,
				Durability = 0.5f
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0.7f,
				DegradeType = "equipment"
			},
			ContainerType = new ToolContainerType
			{
				StorageTags = new string[1] { "storageTagLiquidContainerNoHeat" },
				CanTransactWithTags = new string[8] { "humanTransact", "robotTransact", "ratTransact", "leafcutterTransact", "chickenTransact", "snatcherTransact", "twinklerTransact", "demonTreeTransact" },
				ProductionOutputStorageType = new ItemStorageType(0.8f)
				{
					FullStatePercentage = 0.1f
				}
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "vatSpoakEmpty"
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
								AssetName = "vatSpoakFullFine"
							}
						},
						Conditions = new BitMask64(typeof(StateModifier), 16)
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:tappingBucket")
		{
			Name = "Tapping bucket",
			SummaryDescription = "A container and a spout for tapping liquid from a tree",
			Description = "Made from a clay jar which can be retrieved by salvaging this item.",
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ToolType = new ToolType
			{
				ToolTag = new string[1] { "liquidContainerNoHeat" },
				ToolHandling = ToolHandlingType.Stationary,
				Durability = 0.5f
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0.7f,
				DegradeType = "equipment",
				SalvageProcess = "salvageTappingBucket",
				PartKeys = new SerializableDictionary<string, int> { { "item:clayJar", 1 } }
			},
			ContainerType = new ToolContainerType
			{
				StorageTags = new string[1] { "storageTagLiquidContainerNoHeat" },
				CanTransactWithTags = new string[8] { "humanTransact", "robotTransact", "ratTransact", "leafcutterTransact", "chickenTransact", "snatcherTransact", "twinklerTransact", "demonTreeTransact" },
				ProductionOutputStorageType = new ItemStorageType(0.4f)
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "clayPot"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:plasticTappingBucket")
		{
			Name = "Tapping bucket (plastic)",
			SummaryDescription = "A plastic container and a spout for tapping liquid from a tree",
			Description = "Made from a plastic jar which can be retrieved by salvaging this item.",
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ToolType = new ToolType
			{
				ToolTag = new string[1] { "liquidContainerNoHeat" },
				ToolHandling = ToolHandlingType.Stationary,
				Durability = 0.5f
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0.7f,
				DegradeType = "equipment",
				SalvageProcess = "salvagePlasticTappingBucket",
				PartKeys = new SerializableDictionary<string, int> { { "item:improvisedPlasticJar", 1 } }
			},
			ContainerType = new ToolContainerType
			{
				StorageTags = new string[1] { "storageTagLiquidContainerNoHeat" },
				CanTransactWithTags = new string[8] { "humanTransact", "robotTransact", "ratTransact", "leafcutterTransact", "chickenTransact", "snatcherTransact", "twinklerTransact", "demonTreeTransact" },
				ProductionOutputStorageType = new ItemStorageType(0.4f)
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "canister"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:clayJar")
		{
			Name = "Clay jar",
			SummaryDescription = "A closed container for storing food and liquid",
			Description = "This clay item has been glazed by firing it in the kiln together with a handful of salt, making it waterproof.",
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			ToolType = new ToolType
			{
				ToolTag = new string[1] { "liquidContainerNoHeat" },
				ToolHandling = ToolHandlingType.Stationary,
				Durability = 0.5f
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0.7f,
				DegradeType = "equipment"
			},
			ContainerType = new ToolContainerType
			{
				StorageTags = new string[1] { "storageTagLiquidContainerClosedNoHeat" },
				CanTransactWithTags = new string[1] { "humanTransact" },
				ProductionOutputStorageType = new ItemStorageType(0.4f)
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "clayPot"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:improvisedPlasticJar")
		{
			Name = "Plastic jar (improvised)",
			SummaryDescription = "A closed container for storing food and liquid, made from plastic scraps",
			Description = "N/A",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ToolType = new ToolType
			{
				ToolTag = new string[1] { "liquidContainerNoHeat" },
				ToolHandling = ToolHandlingType.Stationary,
				Durability = 0.5f
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0.7f,
				DegradeType = "equipment"
			},
			ContainerType = new ToolContainerType
			{
				StorageTags = new string[1] { "storageTagLiquidContainerClosedNoHeat" },
				CanTransactWithTags = new string[1] { "humanTransact" },
				ProductionOutputStorageType = new ItemStorageType(0.4f)
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "canister"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:strongBugNet")
		{
			Name = "Bug net",
			SummaryDescription = "Used for catching small flyers",
			Description = "The net consists of permeable textile and a long handle.",
			TierOrArea = new TierOrArea
			{
				Tier = "survival"
			},
			ToolType = new ToolType
			{
				Durability = 0.4f,
				ToolHandling = ToolHandlingType.HandTool
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.08f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "bushcraftComponentsSmall"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:thornHooks")
		{
			Name = "Fishing hooks (thorn)",
			SummaryDescription = "Used for fishing together with bait and string",
			Description = "Sharp, curving thorns made into fishing hooks",
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Food
			},
			ToolType = new ToolType
			{
				Durability = 0.4f,
				ToolHandling = ToolHandlingType.HandTool,
				ToolTag = new string[1] { "fishingHook" }
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.02f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "leatherPouch"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:improvisedMetalHooks")
		{
			Name = "Fishing hooks (scrap metal)",
			SummaryDescription = "Used for fishing together with bait and string",
			Description = "Pieces of metal made into fishing hooks",
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Food
			},
			ToolType = new ToolType
			{
				Durability = 0.4f,
				ToolHandling = ToolHandlingType.HandTool,
				ToolTag = new string[1] { "fishingHook" }
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.02f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "leatherPouch"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:woodenHooks")
		{
			Name = "Fishing hooks (wooden)",
			SummaryDescription = "Used for fishing together with bait and string",
			Description = "Pieces of hard wood sharpened into fishing hooks",
			TierOrArea = new TierOrArea
			{
				Tier = "survival",
				Area = RatingTypes.Food
			},
			ToolType = new ToolType
			{
				Durability = 0.4f,
				ToolHandling = ToolHandlingType.HandTool,
				ToolTag = new string[1] { "fishingHook" }
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.02f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "leatherPouch"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:ironHooks")
		{
			Name = "Fishing hooks (iron)",
			SummaryDescription = "Used for fishing together with bait and string",
			Description = "Various fishing hooks made from wrought iron",
			TierOrArea = new TierOrArea
			{
				Tier = "basic",
				Area = RatingTypes.Food
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.02f
			},
			ToolType = new ToolType
			{
				Durability = 0.4f,
				ToolHandling = ToolHandlingType.HandTool,
				ToolTag = new string[1] { "fishingHook" }
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "leatherPouch"
						}
					}
				}
			}
		});
		float value = 0.1f;
		listOfEntityTypes.Add(new EntityType("item:assemblerPlateA")
		{
			Name = "Assembler plate #A",
			SummaryDescription = "Production plate for the molecular assembler",
			Description = "Produces simple items with a diamondoid, stiff structure like: knife, machete and axe. \nLike all the assembler plates, it is vulnerable to background radiation and should be stored in a shielded container.",
			TierOrArea = new TierOrArea
			{
				Tier = "advanced"
			},
			ToolType = new ToolType
			{
				ToolHandling = ToolHandlingType.Stationary,
				Durability = 0.8f
			},
			ItemType = new ItemType
			{
				MaximumBulk = value
			},
			NonLivingType = new NonLivingType
			{
				DegradeType = "equipment"
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "nanoplate"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:masterAssemblerPlateA")
		{
			Name = "Master assembler plate #A",
			SummaryDescription = "Replicating plate for the molecular assembler",
			Description = "Produces plate #A as well as copies of itself. Because of the high complexity of the product, the process takes longer than ordinary assembly. \nThe plate is vulnerable to background radiation and should be stored in a shielded container. \nIt is always a good idea to keep a few spares of Master plates.",
			TierOrArea = new TierOrArea
			{
				Tier = "advanced"
			},
			ToolType = new ToolType
			{
				ToolHandling = ToolHandlingType.Stationary,
				Durability = 0.8f
			},
			ItemType = new ItemType
			{
				MaximumBulk = value
			},
			NonLivingType = new NonLivingType
			{
				DegradeType = "equipment"
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "nanoplate"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:assemblerPlateB")
		{
			Name = "Assembler plate #B",
			SummaryDescription = "Production plate for the molecular assembler",
			Description = "Produces iron based products such as ammunition for the coil gun. \nLike all the assembler plates, it is vulnerable to background radiation and should be stored in a shielded container.",
			TierOrArea = new TierOrArea
			{
				Tier = "advanced"
			},
			ToolType = new ToolType
			{
				ToolHandling = ToolHandlingType.Stationary,
				Durability = 0.8f
			},
			ItemType = new ItemType
			{
				MaximumBulk = value
			},
			NonLivingType = new NonLivingType
			{
				DegradeType = "equipment"
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "nanoplate"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:masterAssemblerPlateB")
		{
			Name = "Master assembler plate #B",
			SummaryDescription = "Replicating plate for the molecular assembler",
			Description = "Produces plate #B as well as copies of itself. Because of the high complexity of the product, the process takes longer than ordinary assembly. \nThe plate is vulnerable to background radiation and should be stored in a shielded container. \nIt is always a good idea to keep a few spares of Master plates.",
			TierOrArea = new TierOrArea
			{
				Tier = "advanced"
			},
			ToolType = new ToolType
			{
				ToolHandling = ToolHandlingType.Stationary,
				Durability = 0.8f
			},
			ItemType = new ItemType
			{
				MaximumBulk = value
			},
			NonLivingType = new NonLivingType
			{
				DegradeType = "equipment"
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "nanoplate"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:cloak")
		{
			Name = "Cloak",
			SummaryDescription = "Clothing fabric made of metamaterials that bend light.",
			Description = "The cloak makes the wearer extremely hard to see. This makes it useful for hunting.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.07f,
				EffectsWhenEquipped = new string[1] { "cloaking" },
				TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>
				{
					{
						ItemType.TaskType.LongerJourneys,
						ItemType.AppropriateLevel.None
					},
					{
						ItemType.TaskType.UnspecifiedHunting,
						ItemType.AppropriateLevel.Best
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
				}
			},
			TierOrArea = new TierOrArea
			{
				Tier = "advanced",
				Area = RatingTypes.Security
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "equipment"
			},
			CategoryKey = "equipment",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "tarp"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:nightVisionGoggles")
		{
			Name = "Night vision goggles",
			SummaryDescription = "Goggles that improve night vision.",
			Description = "Visible light and infrared radiation is amplified, giving the wearer the ability to see at night.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				EffectsWhenEquipped = new string[1] { "nightVision" },
				TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>
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
					},
					{
						ItemType.TaskType.NightActivities,
						ItemType.AppropriateLevel.Best
					}
				}
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "equipment"
			},
			TierOrArea = new TierOrArea
			{
				Tier = "advanced",
				Area = RatingTypes.Security
			},
			CategoryKey = "equipment",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "greyPouch"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:groundScanner")
		{
			Name = "Ground scanner",
			SummaryDescription = "Device that aids in detecting hidden resources and objects",
			Description = "A sensor suite analyzes the ground surface to show its composition and reveal any objects of interest",
			ItemType = new ItemType
			{
				MaximumBulk = 0.05f,
				EffectsWhenEquipped = new string[1] { "groundScanner" },
				UseGearAtAnyDistanceFromExpedition = true,
				TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel> { 
				{
					ItemType.TaskType.Examining,
					ItemType.AppropriateLevel.Best
				} }
			},
			TierOrArea = new TierOrArea
			{
				Tier = "advanced"
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "equipment"
			},
			CategoryKey = "equipment",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "greyPouch"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:spikeTrap")
		{
			Name = "Spring trap (item)",
			SummaryDescription = "Must be set up using the BUILD button",
			Description = "4 clenching iron spikes held back by a powerful spring. Set off by a trigger plate in the middle. This is a robust design which will kill smaller animals and maim bigger ones.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.1f
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				PartsAreWeatherProof = true,
				DegradeType = "equipment",
				PartKeys = new SerializableDictionary<string, int> { { "item:wroughtIron", 1 } }
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "metalSpikeTrapItem"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:sensor")
		{
			Name = "Motion sensor (item)",
			SummaryDescription = "The sensor needs to be set up as a structure",
			Description = "Once set up, the sensor will monitor its surroundings, allowing us to keep an eye on that area. Developed for the Tau Ceti mission. It is primarily powered by solar cells but will function day and night.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.07f
			},
			TierOrArea = new TierOrArea
			{
				Tier = "advanced",
				Area = RatingTypes.Security
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				PartsAreWeatherProof = true,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "sensorItem"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:fieldLabPacked")
		{
			Name = "Field lab (item)",
			SummaryDescription = "The field lab must be set up using the BUILD button",
			Description = "The field lab packed down for transport. Deploying the field lab makes us able to analyze samples and synthesize new drugs.\n It's being suggested that we instead salvage it and use its components for building a chemical liquid weapon.",
			TierOrArea = new TierOrArea
			{
				Tier = "advanced",
				Area = RatingTypes.Food
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.8f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				PartsAreWeatherProof = false,
				DegradeType = "equipment",
				SalvageProcess = "salvageFieldLabPacked",
				PartKeys = new SerializableDictionary<string, int> { { "item:labComponents", 1 } }
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "boxes"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:labComponents")
		{
			Name = "Lab components",
			SummaryDescription = "Pump, tubes and small containers taken from the field lab",
			Description = "These parts can be used for enhancing a fire extinguisher, making it able to project chemical liquid to defend against quadites.",
			TierOrArea = new TierOrArea
			{
				Tier = "advanced"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.6f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				PartsAreWeatherProof = false,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "labComponents"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:sentryWeaponMount")
		{
			Name = "Sentry weapon mount",
			SummaryDescription = "Rotating, automatically-aimed gun mount that must be outfitted with a weapon",
			Description = "With some engineering skill and suitable components, a weapon can be attached here.",
			TierOrArea = new TierOrArea
			{
				Tier = "advanced",
				Area = RatingTypes.Security
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.6f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				PartsAreWeatherProof = false,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "equipment"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:structurePanels")
		{
			Name = "Structure panels",
			SummaryDescription = "Light weight, insulating, synthetic panels for simple, durable structures",
			Description = "Versatile building material which can be made into many types of structures using simple tools.",
			TierOrArea = new TierOrArea
			{
				Tier = "advanced"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.6f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				PartsAreWeatherProof = false,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "scrapMetal"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:fieldKitchenStove")
		{
			Name = "Field kitchen stove",
			SummaryDescription = "The field kitchen needs to be set up using the BUILD button",
			Description = "The stove for a field kitchen. Deploying the field kitchen creates a place for preparing food in relative comfort.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.35f
			},
			TierOrArea = new TierOrArea
			{
				Tier = "advanced",
				Area = RatingTypes.Food
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				PartsAreWeatherProof = false,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "boxes"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:fieldKitchenEquipment")
		{
			Name = "Field kitchen equipment",
			SummaryDescription = "The field kitchen needs to be set up using the BUILD button",
			Description = "Equipment and structure parts for a field kitchen. Deploying the field kitchen creates a place for preparing food in relative comfort.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.7f
			},
			TierOrArea = new TierOrArea
			{
				Tier = "advanced",
				Area = RatingTypes.Food
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				PartsAreWeatherProof = false,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "boxes"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:metalRefineryPart1")
		{
			Name = "Refinery tanks",
			SummaryDescription = "Large metal tanks which are components for the Refinery structure",
			Description = "The tanks are part of the refinery installation which can separate rare-earth metals. They will be containing the chemical mixtures used in the processes. The refinery is highly compact and can be dismantled, moved and set up as needed.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.35f
			},
			TierOrArea = new TierOrArea
			{
				Tier = "advanced"
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				PartsAreWeatherProof = false,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "boxes"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:metalRefineryEquipment")
		{
			Name = "Refinery frame",
			SummaryDescription = "The frame for the Refinery structure",
			Description = "This component is part of the refinery installation which can separate rare-earth metals. The refinery is highly compact and can be dismantled, moved and set up as needed.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.7f
			},
			TierOrArea = new TierOrArea
			{
				Tier = "advanced"
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				PartsAreWeatherProof = false,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "boxes"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:weatherStationMast")
		{
			Name = "Weather station mast",
			SummaryDescription = "The mast for various weather station sensors",
			Description = "When deployed, the weather station will gather accurate data about the current weather. This can be used for making forecasts.",
			TierOrArea = new TierOrArea
			{
				Tier = "advanced"
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.85f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				PartsAreWeatherProof = true,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "equipment"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:radio")
		{
			Name = "Radio",
			SummaryDescription = "Parts of a radio station which is used to communicate with other settlements",
			Description = "Most settlements maintain communication through radio when satellite communication is unavailable. If another radio station is within reach, they can be contacted in order to arrange trade deals.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.18f
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "boxes"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:radioAntenna")
		{
			Name = "Radio antenna",
			SummaryDescription = "Antenna for a small radio station",
			Description = "Most settlements maintain communication through radio when satellite communication is unavailable. If another radio station is within reach, they can be contacted in order to arrange trade deals.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.7f
			},
			TierOrArea = new TierOrArea
			{
				Tier = "basic"
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "boxes"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:weatherStationSensors")
		{
			Name = "Weather station sensors",
			SummaryDescription = "Sensors for monitoring the weather",
			Description = "When deployed, the weather station will gather accurate data about the current weather. This can be used for making forecasts.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.5f
			},
			TierOrArea = new TierOrArea
			{
				Tier = "advanced"
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "equipment"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:satelliteGroundStation")
		{
			Name = "Satellite ground station",
			SummaryDescription = "Enables satellite communication",
			Description = "When deployed, the satellite ground station will enable communication with other sites using the satellites in orbit.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.5f
			},
			TierOrArea = new TierOrArea
			{
				Tier = "advanced"
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "equipment"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:vacuumChamber")
		{
			Name = "Vacuum chamber",
			SummaryDescription = "The vacuum chamber for the molecular assembler",
			Description = "This chamber perfectly isolates the product from the environment while it is being produced by the assembler.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.5f
			},
			TierOrArea = new TierOrArea
			{
				Tier = "advanced"
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "equipment"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:assemblerCabinet")
		{
			Name = "Assembler cabinet",
			SummaryDescription = "The cabinet for the molecular assembler",
			Description = "This cabinet houses the molecular assembler. It has a port for materials.",
			ItemType = new ItemType
			{
				MaximumBulk = 1f
			},
			TierOrArea = new TierOrArea
			{
				Tier = "advanced"
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "equipment"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:assemblerCooling")
		{
			Name = "Assembler cooling system",
			SummaryDescription = "The cooling system for the molecular assembler",
			Description = "This is the cooling system for the molecular assembler, it is needed to remove the heat generated in the process.",
			ItemType = new ItemType
			{
				MaximumBulk = 0.5f
			},
			TierOrArea = new TierOrArea
			{
				Tier = "advanced"
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "equipment"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:none")
		{
			Name = "none",
			ItemType = new ItemType
			{
				MaximumBulk = 0.3f,
				Abbreviation = "Too"
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			ToolType = new ToolType
			{
				ToolHandling = ToolHandlingType.HandTool,
				Durability = 1f
			},
			CategoryKey = "tools",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "boxes"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:skimmerMotor")
		{
			Name = "Skimmer motor",
			ItemType = new ItemType
			{
				MaximumBulk = 1f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "boxes"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:skimmerRotor")
		{
			Name = "Rotor",
			ItemType = new ItemType
			{
				MaximumBulk = 1f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "boxes"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:skimmerSeat")
		{
			Name = "Seat",
			ItemType = new ItemType
			{
				MaximumBulk = 1f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "boxes"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:skimmerCanopy")
		{
			Name = "Canopy",
			ItemType = new ItemType
			{
				MaximumBulk = 1f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "boxes"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:skimmerHull")
		{
			Name = "Hull",
			ItemType = new ItemType
			{
				MaximumBulk = 6f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "boxes"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:skimmerWing")
		{
			Name = "Wing",
			ItemType = new ItemType
			{
				MaximumBulk = 4f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "boxes"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:skimmerLandingGear")
		{
			Name = "Landing gear",
			ItemType = new ItemType
			{
				MaximumBulk = 4f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "boxes"
						}
					}
				}
			}
		});
		entityType = new EntityType("item:metalParts");
		entityType.Name = "Metal parts";
		entityType.ItemType = new ItemType
		{
			Abbreviation = "Met",
			MaximumBulk = 0.1f
		};
		entityType.NonLivingType = new NonLivingType
		{
			Repairability = 1f,
			DegradeType = "equipment"
		};
		entityType.CategoryKey = "rawMaterials";
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "metal"
					}
				}
			}
		};
		EntityType item = entityType;
		listOfEntityTypes.Add(item);
		listOfEntityTypes.Add(new EntityType("item:cement")
		{
			Name = "Cement",
			ItemType = new ItemType
			{
				Abbreviation = "Cem",
				MaximumBulk = 0.15f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "cement"
						}
					}
				}
			}
		});
		listOfEntityTypes.Add(new EntityType("item:muleVehicleBody")
		{
			Name = "Mule vehicle body",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "boxes"
						}
					}
				}
			},
			ItemType = new ItemType
			{
				MaximumBulk = 5f
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				Repairability = 0.8f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials"
		});
		listOfEntityTypes.Add(new EntityType("item:wheelMotor")
		{
			Name = "Wheel motor",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "boxes"
						}
					}
				}
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.2f
			},
			NonLivingType = new NonLivingType
			{
				Repairability = 0.8f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials"
		});
		listOfEntityTypes.Add(new EntityType("item:suspension")
		{
			Name = "Suspension",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "boxes"
						}
					}
				}
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.2f
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				Repairability = 1f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials"
		});
		listOfEntityTypes.Add(new EntityType("item:vehicleSeat")
		{
			Name = "Seat",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "boxes"
						}
					}
				}
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.6f
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				Repairability = 0.4f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials"
		});
		listOfEntityTypes.Add(new EntityType("item:controlPanel")
		{
			Name = "Control panel",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "boxes"
						}
					}
				}
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.6f
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				Repairability = 0.4f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials"
		});
		listOfEntityTypes.Add(new EntityType("item:wheel")
		{
			Name = "Wheel",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "boxes"
						}
					}
				}
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.4f
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				Repairability = 1f,
				DegradeType = "equipment",
				PartKeys = new SerializableDictionary<string, int> { { "item:metalParts", 3 } }
			},
			CategoryKey = "rawMaterials"
		});
		listOfEntityTypes.Add(new EntityType("item:tyre")
		{
			Name = "Tyre",
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo
				{
					RenderAsBillboardType = new RenderAsBillboardType[1]
					{
						new RenderAsBillboardType
						{
							AssetName = "boxes"
						}
					}
				}
			},
			ItemType = new ItemType
			{
				MaximumBulk = 0.4f
			},
			NonLivingType = new NonLivingType
			{
				PartsAreWeatherProof = true,
				Repairability = 0.3f,
				DegradeType = "equipment"
			},
			CategoryKey = "rawMaterials"
		});
	}

	public static EntityType CreateDiamondKnife(List<EntityType> listOfEntityTypes)
	{
		EntityType entityType = new EntityType("item:advancedKnife");
		entityType.Name = "Knife (diamondoid carbon)";
		entityType.SummaryDescription = "All-purpose survival knife made from diamondoid carbon";
		entityType.Description = "The knife is made from carbon atoms arranged in a crystal structure similar to diamond, giving it exceptional hardness and durability. This is a product of a molecular assembler.";
		entityType.ToolType = new ToolType
		{
			Durability = 0.9f,
			ToolHandling = ToolHandlingType.HandTool,
			ToolTag = new string[3] { "knife", "butcherFlesh", "cutThinShell" }
		};
		entityType.TierOrArea = new TierOrArea
		{
			Tier = "advanced"
		};
		entityType.ItemType = new ItemType
		{
			HauledItemValue = ItemType.HauledItemValues.MostValuable,
			MaximumBulk = 0.05f,
			AttachedObjectRenderableType = "knife",
			AttachorTagToMountOn = "rightHand",
			AttachesToBodyPart = "Right arm",
			AnimStatesWhenAttached = new AnimModifier[1] { AnimModifier.Knife },
			WeaponType = new WeaponType
			{
				AttackTypes = new AttackType[1] { GameData.Instance.AllAttackTypes["knifeHack"] }
			},
			TaskAppropriateLevels = new SerializableDictionary<ItemType.TaskType, ItemType.AppropriateLevel>
			{
				{
					ItemType.TaskType.LongerJourneys,
					ItemType.AppropriateLevel.Normal
				},
				{
					ItemType.TaskType.UnspecifiedHunting,
					ItemType.AppropriateLevel.Minor
				},
				{
					ItemType.TaskType.PatrolOrAttack,
					ItemType.AppropriateLevel.Minor
				},
				{
					ItemType.TaskType.Scouting,
					ItemType.AppropriateLevel.Minor
				},
				{
					ItemType.TaskType.Hauling,
					ItemType.AppropriateLevel.Minor
				}
			}
		};
		entityType.NonLivingType = new NonLivingType
		{
			Repairability = 1f,
			DegradeType = "equipment"
		};
		entityType.CategoryKey = "tools";
		entityType.RenderableType = new RenderableType
		{
			DefaultClientState = new ClientStateInfo
			{
				RenderAsBillboardType = new RenderAsBillboardType[1]
				{
					new RenderAsBillboardType
					{
						AssetName = "knife"
					}
				}
			}
		};
		EntityType entityType2 = entityType;
		listOfEntityTypes.Add(entityType2);
		return entityType2;
	}
}
