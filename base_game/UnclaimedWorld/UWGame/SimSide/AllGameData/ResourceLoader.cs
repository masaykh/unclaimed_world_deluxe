using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Resources;

namespace UWGame.SimSide.AllGameData;

public class ResourceLoader
{
	public static List<ResourceType> Init()
	{
		List<ResourceType> list = new List<ResourceType>();
		float value = 500f;
		ResourceType resourceType = new ResourceType("firewood");
		resourceType.ResourceItem = "item:firewood";
		resourceType.Name = "Firewood";
		resourceType.Color = Color.PaleVioletRed;
		resourceType.FractionOfMaximumToReplenishEachTime = 0.5f;
		resourceType.DaysOfYearToReplenish = new NormalDistribution[2]
		{
			new NormalDistribution
			{
				Mean = 0.44999998807907104,
				StandardDeviation = 0.029999999329447746
			},
			new NormalDistribution
			{
				Mean = 0.699999988079071,
				StandardDeviation = 0.029999999329447746
			}
		};
		resourceType.DetectionFlashDuration = value;
		resourceType.Category = GameData.Instance.AllResourceCategories["rawMaterials"];
		resourceType.DetectionTag = "largeOnGround";
		resourceType.TileResourceType = new TileResourceType
		{
			MaxFlavours = 4,
			MoreSpriteLimit = 3f,
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo(),
				ClientStateConditions = new ClientStateInfo[12]
				{
					new ClientStateInfo
					{
						Conditions = new BitMask64(typeof(StateModifier), 21)
					},
					new ClientStateInfo
					{
						Conditions = new BitMask64(typeof(StateModifier), 22)
					},
					new ClientStateInfo
					{
						Conditions = new BitMask64(typeof(StateModifier), 23)
					},
					new ClientStateInfo
					{
						Conditions = new BitMask64(typeof(StateModifier), 24)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "tileresource_firewood_more_1"
						},
						Conditions = new BitMask64(typeof(StateModifier), 21, 20)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "tileresource_firewood_more_2"
						},
						Conditions = new BitMask64(typeof(StateModifier), 22, 20)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "tileresource_firewood_more_3"
						},
						Conditions = new BitMask64(typeof(StateModifier), 23, 20)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "tileresource_firewood_more_1"
						},
						Conditions = new BitMask64(typeof(StateModifier), 24, 20)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "tileresource_firewood_less_4"
						},
						Conditions = new BitMask64(typeof(StateModifier), 21, 19)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "tileresource_firewood_less_5"
						},
						Conditions = new BitMask64(typeof(StateModifier), 22, 19)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "tileresource_firewood_less_6"
						},
						Conditions = new BitMask64(typeof(StateModifier), 23, 19)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "tileresource_firewood_less_7"
						},
						Conditions = new BitMask64(typeof(StateModifier), 24, 19)
					}
				}
			}
		};
		ResourceType item = resourceType;
		list.Add(item);
		NormalDistribution[] daysOfYearToReplenish = new NormalDistribution[5]
		{
			new NormalDistribution
			{
				Mean = 0.0,
				StandardDeviation = 0.019999999552965164
			},
			new NormalDistribution
			{
				Mean = 0.20000000298023224,
				StandardDeviation = 0.019999999552965164
			},
			new NormalDistribution
			{
				Mean = 0.4000000059604645,
				StandardDeviation = 0.019999999552965164
			},
			new NormalDistribution
			{
				Mean = 0.6000000238418579,
				StandardDeviation = 0.019999999552965164
			},
			new NormalDistribution
			{
				Mean = 0.800000011920929,
				StandardDeviation = 0.019999999552965164
			}
		};
		NormalDistribution[] daysOfYearToReplenish2 = new NormalDistribution[10]
		{
			new NormalDistribution
			{
				Mean = 0.0,
				StandardDeviation = 0.009999999776482582
			},
			new NormalDistribution
			{
				Mean = 0.10000000149011612,
				StandardDeviation = 0.009999999776482582
			},
			new NormalDistribution
			{
				Mean = 0.20000000298023224,
				StandardDeviation = 0.009999999776482582
			},
			new NormalDistribution
			{
				Mean = 0.30000001192092896,
				StandardDeviation = 0.009999999776482582
			},
			new NormalDistribution
			{
				Mean = 0.4000000059604645,
				StandardDeviation = 0.009999999776482582
			},
			new NormalDistribution
			{
				Mean = 0.5,
				StandardDeviation = 0.009999999776482582
			},
			new NormalDistribution
			{
				Mean = 0.6000000238418579,
				StandardDeviation = 0.009999999776482582
			},
			new NormalDistribution
			{
				Mean = 0.699999988079071,
				StandardDeviation = 0.009999999776482582
			},
			new NormalDistribution
			{
				Mean = 0.800000011920929,
				StandardDeviation = 0.009999999776482582
			},
			new NormalDistribution
			{
				Mean = 0.8999999761581421,
				StandardDeviation = 0.009999999776482582
			}
		};
		resourceType = new ResourceType("clamwich");
		resourceType.ResourceItem = "item:clamwich";
		resourceType.Name = "Clamwich";
		resourceType.Color = Color.LimeGreen;
		resourceType.Category = GameData.Instance.AllResourceCategories["food"];
		resourceType.FractionOfMaximumToReplenishEachTime = 0.5f;
		resourceType.DaysOfYearToReplenish = daysOfYearToReplenish;
		resourceType.DetectionTag = "smallAboveGround";
		resourceType.TileResourceType = new TileResourceType
		{
			MaxFlavours = 3,
			MoreSpriteLimit = 2f,
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo(),
				ClientStateConditions = new ClientStateInfo[9]
				{
					new ClientStateInfo
					{
						Conditions = new BitMask64(typeof(StateModifier), 21)
					},
					new ClientStateInfo
					{
						Conditions = new BitMask64(typeof(StateModifier), 22)
					},
					new ClientStateInfo
					{
						Conditions = new BitMask64(typeof(StateModifier), 23)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "tileresource_clamwich_more_1"
						},
						Conditions = new BitMask64(typeof(StateModifier), 21, 20)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "tileresource_clamwich_more_2"
						},
						Conditions = new BitMask64(typeof(StateModifier), 22, 20)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "tileresource_clamwich_more_2"
						},
						Conditions = new BitMask64(typeof(StateModifier), 23, 20)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "tileresource_clamwich_less_3"
						},
						Conditions = new BitMask64(typeof(StateModifier), 21, 19)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "tileresource_clamwich_less_4"
						},
						Conditions = new BitMask64(typeof(StateModifier), 22, 19)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "tileresource_clamwich_less_5"
						},
						Conditions = new BitMask64(typeof(StateModifier), 23, 19)
					}
				}
			}
		};
		item = resourceType;
		list.Add(item);
		resourceType = new ResourceType("sulfurDeposit");
		resourceType.ResourceItem = "item:sulfurBlocks";
		resourceType.Name = "Sulfur";
		resourceType.Color = Color.Yellow;
		resourceType.Category = GameData.Instance.AllResourceCategories["rawMaterials"];
		resourceType.DetectionTag = "largeOnGround";
		resourceType.TileResourceType = new TileResourceType
		{
			MaxFlavours = 3,
			MoreSpriteLimit = 2f,
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo(),
				ClientStateConditions = new ClientStateInfo[9]
				{
					new ClientStateInfo
					{
						Conditions = new BitMask64(typeof(StateModifier), 21)
					},
					new ClientStateInfo
					{
						Conditions = new BitMask64(typeof(StateModifier), 22)
					},
					new ClientStateInfo
					{
						Conditions = new BitMask64(typeof(StateModifier), 23)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "tileresource_sulfur_more_1"
						},
						Conditions = new BitMask64(typeof(StateModifier), 21, 20)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "tileresource_sulfur_more_2"
						},
						Conditions = new BitMask64(typeof(StateModifier), 22, 20)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "tileresource_sulfur_more_3"
						},
						Conditions = new BitMask64(typeof(StateModifier), 23, 20)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "tileresource_sulfur_less_4"
						},
						Conditions = new BitMask64(typeof(StateModifier), 21, 19)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "tileresource_sulfur_less_5"
						},
						Conditions = new BitMask64(typeof(StateModifier), 22, 19)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "tileresource_sulfur_less_6"
						},
						Conditions = new BitMask64(typeof(StateModifier), 23, 19)
					}
				}
			}
		};
		item = resourceType;
		list.Add(item);
		resourceType = new ResourceType("torux");
		resourceType.ResourceItem = "item:torux";
		resourceType.Name = "Torux";
		resourceType.Color = Color.LightSteelBlue;
		resourceType.Category = GameData.Instance.AllResourceCategories["food"];
		resourceType.DetectionTag = "smallAboveGround";
		resourceType.FractionOfMaximumToReplenishEachTime = 1f;
		resourceType.DaysOfYearToReplenish = daysOfYearToReplenish;
		resourceType.TileResourceType = new TileResourceType
		{
			MaxFlavours = 2,
			MoreSpriteLimit = 2f,
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo(),
				ClientStateConditions = new ClientStateInfo[6]
				{
					new ClientStateInfo
					{
						Conditions = new BitMask64(typeof(StateModifier), 21)
					},
					new ClientStateInfo
					{
						Conditions = new BitMask64(typeof(StateModifier), 22)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "tileresource_torux_more_3"
						},
						Conditions = new BitMask64(typeof(StateModifier), 21, 20)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "tileresource_torux_more_4"
						},
						Conditions = new BitMask64(typeof(StateModifier), 22, 20)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "tileresource_torux_less_1"
						},
						Conditions = new BitMask64(typeof(StateModifier), 21, 19)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "tileresource_torux_less_2"
						},
						Conditions = new BitMask64(typeof(StateModifier), 22, 19)
					}
				}
			}
		};
		item = resourceType;
		list.Add(item);
		resourceType = new ResourceType("blackpulp");
		resourceType.ResourceItem = "item:blackpulp";
		resourceType.Name = "Blackpulp";
		resourceType.Color = Color.LightSeaGreen;
		resourceType.Category = GameData.Instance.AllResourceCategories["food"];
		resourceType.DetectionTag = "smallAboveGround";
		resourceType.FractionOfMaximumToReplenishEachTime = 1f;
		resourceType.DaysOfYearToReplenish = daysOfYearToReplenish2;
		resourceType.TileResourceType = new TileResourceType
		{
			MaxFlavours = 2,
			MoreSpriteLimit = 2f,
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo(),
				ClientStateConditions = new ClientStateInfo[6]
				{
					new ClientStateInfo
					{
						Conditions = new BitMask64(typeof(StateModifier), 21)
					},
					new ClientStateInfo
					{
						Conditions = new BitMask64(typeof(StateModifier), 22)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "tileresource_blackpulp_more_1"
						},
						Conditions = new BitMask64(typeof(StateModifier), 21, 20)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "tileresource_blackpulp_more_2"
						},
						Conditions = new BitMask64(typeof(StateModifier), 22, 20)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "tileresource_blackpulp_less_1"
						},
						Conditions = new BitMask64(typeof(StateModifier), 21, 19)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "tileresource_blackpulp_less_1"
						},
						Conditions = new BitMask64(typeof(StateModifier), 22, 19)
					}
				}
			}
		};
		item = resourceType;
		list.Add(item);
		resourceType = new ResourceType("favorbread");
		resourceType.ResourceItem = "item:favorbread";
		resourceType.Name = "Favorbread";
		resourceType.Color = Color.BlueViolet;
		resourceType.Category = GameData.Instance.AllResourceCategories["food"];
		resourceType.DetectionTag = "smallAboveGround";
		resourceType.FractionOfMaximumToReplenishEachTime = 1f;
		resourceType.DaysOfYearToReplenish = new NormalDistribution[3]
		{
			new NormalDistribution
			{
				Mean = 0.20000000298023224,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.550000011920929,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.8999999761581421,
				StandardDeviation = 0.05000000074505806
			}
		};
		resourceType.TileResourceType = new TileResourceType
		{
			MaxFlavours = 2,
			MoreSpriteLimit = 1.9f,
			RenderableType = new RenderableType
			{
				DefaultClientState = new ClientStateInfo(),
				ClientStateConditions = new ClientStateInfo[6]
				{
					new ClientStateInfo
					{
						Conditions = new BitMask64(typeof(StateModifier), 21)
					},
					new ClientStateInfo
					{
						Conditions = new BitMask64(typeof(StateModifier), 22)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "tileresource_favorbread_more_1"
						},
						Conditions = new BitMask64(typeof(StateModifier), 21, 20)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "tileresource_favorbread_more_1"
						},
						Conditions = new BitMask64(typeof(StateModifier), 22, 20)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "tileresource_favorbread_less_1"
						},
						Conditions = new BitMask64(typeof(StateModifier), 21, 19)
					},
					new ClientStateInfo
					{
						RenderAsGroundSpriteType = new RenderAsGroundSpriteType
						{
							AssetName = "tileresource_favorbread_less_2"
						},
						Conditions = new BitMask64(typeof(StateModifier), 22, 19)
					}
				}
			}
		};
		item = resourceType;
		list.Add(item);
		item = new ResourceType("podlac")
		{
			ResourceItem = "item:podlacUnrefined",
			Name = "Podlac",
			Color = Color.BurlyWood,
			DetectionFlashDuration = value,
			Category = GameData.Instance.AllResourceCategories["rawMaterials"],
			DetectionTag = "largeOnGround",
			TileResourceType = new TileResourceType
			{
				RenderableType = new RenderableType("podlac")
				{
					RenderAsIconType = new RenderAsIconType
					{
						AssetName = "charcoal",
						IconToRender = IconToRender.Ore
					}
				}
			}
		};
		list.Add(item);
		item = new ResourceType("bogOre")
		{
			ResourceItem = "item:bogOre",
			Name = "Bog ore",
			Color = Color.HotPink,
			DetectionFlashDuration = value,
			Category = GameData.Instance.AllResourceCategories["rawMaterials"],
			DetectionTag = "largeOnGround",
			TileResourceType = new TileResourceType
			{
				RenderableType = new RenderableType("bogOre")
				{
					RenderAsIconType = new RenderAsIconType
					{
						AssetName = "ore_s",
						IconToRender = IconToRender.Ore
					}
				}
			}
		};
		list.Add(item);
		item = new ResourceType("goldOre")
		{
			ResourceItem = "item:goldOre",
			Name = "Gold ore",
			Color = Color.Gold,
			DetectionFlashDuration = value,
			Category = GameData.Instance.AllResourceCategories["rawMaterials"],
			DetectionTag = "largeOnGround",
			TileResourceType = new TileResourceType
			{
				RenderableType = new RenderableType("bogOre")
				{
					RenderAsIconType = new RenderAsIconType
					{
						AssetName = "ore_s",
						IconToRender = IconToRender.Ore
					}
				}
			}
		};
		list.Add(item);
		item = new ResourceType("clay")
		{
			ResourceItem = "item:clay",
			Name = "Clay",
			Color = Color.Blue,
			DetectionFlashDuration = value,
			Category = GameData.Instance.AllResourceCategories["rawMaterials"],
			DetectionTag = "largeOnGround",
			TileResourceType = new TileResourceType
			{
				RenderableType = new RenderableType("clay")
				{
					RenderAsIconType = new RenderAsIconType
					{
						AssetName = "soil_s",
						IconToRender = IconToRender.Ore
					}
				}
			}
		};
		list.Add(item);
		item = new ResourceType("salt")
		{
			ResourceItem = "item:salt",
			Name = "Salt",
			Color = Color.White,
			DetectionFlashDuration = value,
			Category = GameData.Instance.AllResourceCategories["rawMaterials"],
			DetectionTag = "largeOnGround",
			TileResourceType = new TileResourceType
			{
				RenderableType = new RenderableType("salt")
				{
					RenderAsIconType = new RenderAsIconType
					{
						AssetName = "saltpeterPowder",
						IconToRender = IconToRender.Ore
					}
				}
			}
		};
		list.Add(item);
		item = new ResourceType("stones")
		{
			ResourceItem = "item:stones",
			Name = "Stones",
			Color = Color.LemonChiffon,
			DetectionFlashDuration = value,
			Category = GameData.Instance.AllResourceCategories["rawMaterials"],
			DetectionTag = "largeOnGround",
			TileResourceType = new TileResourceType
			{
				RenderableType = new RenderableType("stones")
				{
					RenderAsIconType = new RenderAsIconType
					{
						AssetName = "stones",
						IconToRender = IconToRender.Ore
					}
				}
			}
		};
		list.Add(item);
		item = new ResourceType("smoothSandstone")
		{
			ResourceItem = "item:smoothSandstone",
			Name = "Sharpening stones",
			Color = Color.Black,
			Category = GameData.Instance.AllResourceCategories["rawMaterials"],
			DetectionTag = "smallAboveGround",
			TileResourceType = new TileResourceType
			{
				RenderableType = new RenderableType("smoothSandstone")
				{
					RenderAsIconType = new RenderAsIconType
					{
						AssetName = "smoothSandstone",
						IconToRender = IconToRender.Ore
					}
				}
			}
		};
		list.Add(item);
		item = new ResourceType("flint")
		{
			ResourceItem = "item:flintRough",
			Name = "Flint",
			Color = Color.White,
			Category = GameData.Instance.AllResourceCategories["rawMaterials"],
			DetectionTag = "smallAboveGround",
			TileResourceType = new TileResourceType
			{
				RenderableType = new RenderableType("smoothSandstone")
				{
					RenderAsIconType = new RenderAsIconType
					{
						AssetName = "smoothSandstone",
						IconToRender = IconToRender.Ore
					}
				}
			}
		};
		list.Add(item);
		resourceType = new ResourceType("firegrassSod");
		resourceType.ResourceItem = "item:firegrassSod";
		resourceType.Name = "Firegrass sod";
		resourceType.Color = Color.Indigo;
		resourceType.Category = GameData.Instance.AllResourceCategories["rawMaterials"];
		resourceType.DetectionTag = "largeOnGround";
		resourceType.FractionOfMaximumToReplenishEachTime = 1f;
		resourceType.DaysOfYearToReplenish = new NormalDistribution[2]
		{
			new NormalDistribution
			{
				Mean = 0.30000001192092896,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.8999999761581421,
				StandardDeviation = 0.05000000074505806
			}
		};
		resourceType.TileResourceType = new TileResourceType
		{
			RenderableType = new RenderableType("firegrassSod")
			{
				RenderAsIconType = new RenderAsIconType
				{
					AssetName = "firegrassSod",
					IconToRender = IconToRender.Ore
				}
			}
		};
		item = resourceType;
		list.Add(item);
		resourceType = new ResourceType("vine");
		resourceType.ResourceItem = "item:vine";
		resourceType.Name = "Vine";
		resourceType.Color = Color.Indigo;
		resourceType.Category = GameData.Instance.AllResourceCategories["rawMaterials"];
		resourceType.DetectionTag = "largeOnGround";
		resourceType.FractionOfMaximumToReplenishEachTime = 0.7f;
		resourceType.DaysOfYearToReplenish = new NormalDistribution[3]
		{
			new NormalDistribution
			{
				Mean = 0.30000001192092896,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.6000000238418579,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.8999999761581421,
				StandardDeviation = 0.05000000074505806
			}
		};
		resourceType.TileResourceType = new TileResourceType
		{
			RenderableType = new RenderableType("vine")
			{
				RenderAsIconType = new RenderAsIconType
				{
					AssetName = "vine",
					IconToRender = IconToRender.Ore
				}
			}
		};
		item = resourceType;
		list.Add(item);
		resourceType = new ResourceType("guanoDeposit");
		resourceType.ResourceItem = "item:guano";
		resourceType.Name = "Guano deposit";
		resourceType.Color = Color.Blue;
		resourceType.Category = GameData.Instance.AllResourceCategories["rawMaterials"];
		resourceType.FractionOfMaximumToReplenishEachTime = 0.5f;
		resourceType.DaysOfYearToReplenish = new NormalDistribution[1]
		{
			new NormalDistribution
			{
				Mean = 0.25,
				StandardDeviation = 0.10000000149011612
			}
		};
		resourceType.DetectionTag = "fruitOnGroundHardToFind";
		resourceType.TileResourceType = new TileResourceType
		{
			RenderableType = new RenderableType("guanoDeposit")
			{
				RenderAsIconType = new RenderAsIconType
				{
					AssetName = "guanoDeposit",
					IconToRender = IconToRender.Ore
				}
			}
		};
		item = resourceType;
		list.Add(item);
		list.Add(new ResourceType("minnowsLive")
		{
			ResourceItem = "item:minnowsLive",
			Name = "Minnows",
			Color = Color.Azure,
			Category = GameData.Instance.AllResourceCategories["food"],
			DetectionTag = "inShallowWater",
			FractionOfMaximumToReplenishEachTime = 1f,
			DaysOfYearToReplenish = daysOfYearToReplenish2,
			TileResourceType = new TileResourceType
			{
				RenderableType = new RenderableType("minnowsLive")
				{
					RenderAsIconType = new RenderAsIconType
					{
						AssetName = "minnowsLive",
						IconToRender = IconToRender.Hook
					}
				}
			}
		});
		list.Add(new ResourceType("alabasterRay")
		{
			ResourceItem = "item:alabasterRay",
			Name = "Alabaster ray",
			Color = Color.Fuchsia,
			Category = GameData.Instance.AllResourceCategories["food"],
			DetectionTag = "inDeeperWater",
			FractionOfMaximumToReplenishEachTime = 1f,
			DaysOfYearToReplenish = daysOfYearToReplenish2,
			TileResourceType = new TileResourceType
			{
				RenderableType = new RenderableType("alabasterRay")
				{
					RenderAsIconType = new RenderAsIconType
					{
						AssetName = "alabasterRay",
						IconToRender = IconToRender.Hook
					}
				}
			}
		});
		list.Add(new ResourceType("streakFin")
		{
			ResourceItem = "item:streakFin",
			Name = "Streak fin",
			Color = Color.Gold,
			Category = GameData.Instance.AllResourceCategories["food"],
			DetectionTag = "inDeeperWater",
			FractionOfMaximumToReplenishEachTime = 1f,
			DaysOfYearToReplenish = daysOfYearToReplenish2,
			TileResourceType = new TileResourceType
			{
				RenderableType = new RenderableType("streakFin")
				{
					RenderAsIconType = new RenderAsIconType
					{
						AssetName = "streakFin",
						IconToRender = IconToRender.Hook
					}
				}
			}
		});
		list.Add(new ResourceType("carbonTail")
		{
			ResourceItem = "item:carbonTail",
			Name = "Carbon tail",
			Color = Color.DarkSeaGreen,
			Category = GameData.Instance.AllResourceCategories["food"],
			DetectionTag = "inDeeperWater",
			FractionOfMaximumToReplenishEachTime = 1f,
			DaysOfYearToReplenish = daysOfYearToReplenish2,
			TileResourceType = new TileResourceType
			{
				RenderableType = new RenderableType("carbonTail")
				{
					RenderAsIconType = new RenderAsIconType
					{
						AssetName = "carbonTail",
						IconToRender = IconToRender.Hook
					}
				}
			}
		});
		resourceType = new ResourceType("neonHornets");
		resourceType.ResourceItem = "item:neonHornetsLive";
		resourceType.Name = "Neon hornets";
		resourceType.Color = Color.DeepSkyBlue;
		resourceType.Category = GameData.Instance.AllResourceCategories["rawMaterials"];
		resourceType.DetectionTag = "smallAnimalAboveGround";
		resourceType.FractionOfMaximumToReplenishEachTime = 1f;
		resourceType.DaysOfYearToReplenish = new NormalDistribution[4]
		{
			new NormalDistribution
			{
				Mean = 0.0,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.30000001192092896,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.6000000238418579,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.8999999761581421,
				StandardDeviation = 0.05000000074505806
			}
		};
		resourceType.TileResourceType = new TileResourceType
		{
			RenderableType = new RenderableType("neonHornets")
			{
				RenderAsIconType = new RenderAsIconType
				{
					AssetName = "neonHornets",
					IconToRender = IconToRender.Bug
				}
			}
		};
		item = resourceType;
		list.Add(item);
		resourceType = new ResourceType("stinkpup");
		resourceType.ResourceItem = "item:stinkpup";
		resourceType.Name = "Stinkpup";
		resourceType.Color = Color.DarkTurquoise;
		resourceType.Category = GameData.Instance.AllResourceCategories["food"];
		resourceType.DetectionTag = "smallAnimalAboveGroundHardToSee";
		resourceType.FractionOfMaximumToReplenishEachTime = 1f;
		resourceType.DaysOfYearToReplenish = new NormalDistribution[2]
		{
			new NormalDistribution
			{
				Mean = 0.0,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.6000000238418579,
				StandardDeviation = 0.05000000074505806
			}
		};
		resourceType.TileResourceType = new TileResourceType
		{
			RenderableType = new RenderableType("stinkpup")
			{
				RenderAsIconType = new RenderAsIconType
				{
					AssetName = "stinkpup",
					IconToRender = IconToRender.Bug
				}
			}
		};
		item = resourceType;
		list.Add(item);
		resourceType = new ResourceType("phantomWeaver");
		resourceType.ResourceItem = "item:phantomWeaver";
		resourceType.Name = "Phantom weaver";
		resourceType.Color = Color.DarkSalmon;
		resourceType.Category = GameData.Instance.AllResourceCategories["food"];
		resourceType.DetectionTag = "smallAnimalAboveGroundHardToSee";
		resourceType.FractionOfMaximumToReplenishEachTime = 1f;
		resourceType.DaysOfYearToReplenish = new NormalDistribution[3]
		{
			new NormalDistribution
			{
				Mean = 0.0,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.4000000059604645,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.800000011920929,
				StandardDeviation = 0.05000000074505806
			}
		};
		resourceType.TileResourceType = new TileResourceType
		{
			RenderableType = new RenderableType("phantomWeaver")
			{
				RenderAsIconType = new RenderAsIconType
				{
					AssetName = "phantomWeaver",
					IconToRender = IconToRender.Bug
				}
			}
		};
		item = resourceType;
		list.Add(item);
		resourceType = new ResourceType("ursinix");
		resourceType.ResourceItem = "item:ursinix";
		resourceType.Name = "Ursinix";
		resourceType.Color = Color.DarkSalmon;
		resourceType.Category = GameData.Instance.AllResourceCategories["food"];
		resourceType.DetectionTag = "fruitOnGroundHardToFind";
		resourceType.FractionOfMaximumToReplenishEachTime = 1f;
		resourceType.DaysOfYearToReplenish = new NormalDistribution[3]
		{
			new NormalDistribution
			{
				Mean = 0.0,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.4000000059604645,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.800000011920929,
				StandardDeviation = 0.05000000074505806
			}
		};
		resourceType.TileResourceType = new TileResourceType
		{
			RenderableType = new RenderableType("ursinix")
			{
				RenderAsIconType = new RenderAsIconType
				{
					AssetName = "ursinix",
					IconToRender = IconToRender.Bug
				}
			}
		};
		item = resourceType;
		list.Add(item);
		resourceType = new ResourceType("webWing");
		resourceType.ResourceItem = "item:webWing";
		resourceType.Name = "Web wing";
		resourceType.Color = Color.DarkOrange;
		resourceType.Category = GameData.Instance.AllResourceCategories["food"];
		resourceType.DetectionTag = "smallAnimalAboveGround";
		resourceType.FractionOfMaximumToReplenishEachTime = 1f;
		resourceType.DaysOfYearToReplenish = new NormalDistribution[3]
		{
			new NormalDistribution
			{
				Mean = 0.0,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.4000000059604645,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.800000011920929,
				StandardDeviation = 0.05000000074505806
			}
		};
		resourceType.TileResourceType = new TileResourceType
		{
			RenderableType = new RenderableType("webWing")
			{
				RenderAsIconType = new RenderAsIconType
				{
					AssetName = "webWing",
					IconToRender = IconToRender.Bug
				}
			}
		};
		item = resourceType;
		list.Add(item);
		resourceType = new ResourceType("crestedFoiler");
		resourceType.ResourceItem = "item:crestedFoiler";
		resourceType.Name = "Crested foiler";
		resourceType.Color = Color.DarkOliveGreen;
		resourceType.Category = GameData.Instance.AllResourceCategories["food"];
		resourceType.DetectionTag = "smallAnimalAboveGround";
		resourceType.FractionOfMaximumToReplenishEachTime = 1f;
		resourceType.DaysOfYearToReplenish = new NormalDistribution[3]
		{
			new NormalDistribution
			{
				Mean = 0.0,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.4000000059604645,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.800000011920929,
				StandardDeviation = 0.05000000074505806
			}
		};
		resourceType.TileResourceType = new TileResourceType
		{
			RenderableType = new RenderableType("crestedFoiler")
			{
				RenderAsIconType = new RenderAsIconType
				{
					AssetName = "crestedFoiler",
					IconToRender = IconToRender.Bug
				}
			}
		};
		item = resourceType;
		list.Add(item);
		resourceType = new ResourceType("goldenCenobite");
		resourceType.ResourceItem = "item:goldenCenobite";
		resourceType.Name = "Golden cenobite";
		resourceType.Color = Color.DarkGreen;
		resourceType.Category = GameData.Instance.AllResourceCategories["food"];
		resourceType.DetectionTag = "smallAnimalAboveGround";
		resourceType.FractionOfMaximumToReplenishEachTime = 1f;
		resourceType.DaysOfYearToReplenish = new NormalDistribution[3]
		{
			new NormalDistribution
			{
				Mean = 0.0,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.4000000059604645,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.800000011920929,
				StandardDeviation = 0.05000000074505806
			}
		};
		resourceType.TileResourceType = new TileResourceType
		{
			RenderableType = new RenderableType("goldenCenobite")
			{
				RenderAsIconType = new RenderAsIconType
				{
					AssetName = "goldenCenobite",
					IconToRender = IconToRender.Bug
				}
			}
		};
		item = resourceType;
		list.Add(item);
		resourceType = new ResourceType("treeScuttler");
		resourceType.ResourceItem = "item:treeScuttler";
		resourceType.Name = "Tree scuttler";
		resourceType.Color = Color.Cornsilk;
		resourceType.Category = GameData.Instance.AllResourceCategories["food"];
		resourceType.DetectionTag = "smallAnimalAboveGround";
		resourceType.FractionOfMaximumToReplenishEachTime = 1f;
		resourceType.DaysOfYearToReplenish = new NormalDistribution[3]
		{
			new NormalDistribution
			{
				Mean = 0.0,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.4000000059604645,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.800000011920929,
				StandardDeviation = 0.05000000074505806
			}
		};
		resourceType.TileResourceType = new TileResourceType
		{
			RenderableType = new RenderableType("treeScuttler")
			{
				RenderAsIconType = new RenderAsIconType
				{
					AssetName = "treeScuttler",
					IconToRender = IconToRender.Bug
				}
			}
		};
		item = resourceType;
		list.Add(item);
		resourceType = new ResourceType("muckGrinder");
		resourceType.ResourceItem = "item:muckGrinder";
		resourceType.Name = "Muck grinder";
		resourceType.Color = Color.BurlyWood;
		resourceType.Category = GameData.Instance.AllResourceCategories["food"];
		resourceType.DetectionTag = "smallAnimalOnGround";
		resourceType.FractionOfMaximumToReplenishEachTime = 1f;
		resourceType.DaysOfYearToReplenish = new NormalDistribution[3]
		{
			new NormalDistribution
			{
				Mean = 0.0,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.4000000059604645,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.800000011920929,
				StandardDeviation = 0.05000000074505806
			}
		};
		resourceType.TileResourceType = new TileResourceType
		{
			RenderableType = new RenderableType("muckGrinder")
			{
				RenderAsIconType = new RenderAsIconType
				{
					AssetName = "muckGrinder",
					IconToRender = IconToRender.Bug
				}
			}
		};
		item = resourceType;
		list.Add(item);
		resourceType = new ResourceType("spriteSlug");
		resourceType.ResourceItem = "item:spriteSlug";
		resourceType.Name = "Sprite slug";
		resourceType.Color = Color.Chocolate;
		resourceType.Category = GameData.Instance.AllResourceCategories["food"];
		resourceType.DetectionTag = "smallHidden";
		resourceType.FractionOfMaximumToReplenishEachTime = 1f;
		resourceType.DaysOfYearToReplenish = new NormalDistribution[3]
		{
			new NormalDistribution
			{
				Mean = 0.0,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.4000000059604645,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.800000011920929,
				StandardDeviation = 0.05000000074505806
			}
		};
		resourceType.TileResourceType = new TileResourceType
		{
			RenderableType = new RenderableType("spriteSlug")
			{
				RenderAsIconType = new RenderAsIconType
				{
					AssetName = "spriteSlug",
					IconToRender = IconToRender.Bug
				}
			}
		};
		item = resourceType;
		list.Add(item);
		resourceType = new ResourceType("crazyDweller");
		resourceType.ResourceItem = "item:crazyDweller";
		resourceType.Name = "Crazy dweller";
		resourceType.Color = Color.Coral;
		resourceType.Category = GameData.Instance.AllResourceCategories["food"];
		resourceType.DetectionTag = "smallAnimalOnGround";
		resourceType.FractionOfMaximumToReplenishEachTime = 1f;
		resourceType.DaysOfYearToReplenish = new NormalDistribution[3]
		{
			new NormalDistribution
			{
				Mean = 0.0,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.4000000059604645,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.800000011920929,
				StandardDeviation = 0.05000000074505806
			}
		};
		resourceType.TileResourceType = new TileResourceType
		{
			RenderableType = new RenderableType("crazyDweller")
			{
				RenderAsIconType = new RenderAsIconType
				{
					AssetName = "crazyDweller",
					IconToRender = IconToRender.Bug
				}
			}
		};
		item = resourceType;
		list.Add(item);
		resourceType = new ResourceType("daggermouth");
		resourceType.ResourceItem = "item:daggermouth";
		resourceType.Name = "Daggermouth";
		resourceType.Color = Color.Crimson;
		resourceType.Category = GameData.Instance.AllResourceCategories["food"];
		resourceType.DetectionTag = "smallAnimalOnGround";
		resourceType.FractionOfMaximumToReplenishEachTime = 1f;
		resourceType.DaysOfYearToReplenish = new NormalDistribution[3]
		{
			new NormalDistribution
			{
				Mean = 0.0,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.4000000059604645,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.800000011920929,
				StandardDeviation = 0.05000000074505806
			}
		};
		resourceType.TileResourceType = new TileResourceType
		{
			RenderableType = new RenderableType("daggermouth")
			{
				RenderAsIconType = new RenderAsIconType
				{
					AssetName = "daggermouth",
					IconToRender = IconToRender.Hook
				}
			}
		};
		item = resourceType;
		list.Add(item);
		resourceType = new ResourceType("impEel");
		resourceType.ResourceItem = "item:impEel";
		resourceType.Name = "Imp eel";
		resourceType.Color = Color.Crimson;
		resourceType.Category = GameData.Instance.AllResourceCategories["food"];
		resourceType.DetectionTag = "smallAnimalBelowGround";
		resourceType.FractionOfMaximumToReplenishEachTime = 1f;
		resourceType.DaysOfYearToReplenish = new NormalDistribution[3]
		{
			new NormalDistribution
			{
				Mean = 0.0,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.4000000059604645,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.800000011920929,
				StandardDeviation = 0.05000000074505806
			}
		};
		resourceType.TileResourceType = new TileResourceType
		{
			RenderableType = new RenderableType("impEel")
			{
				RenderAsIconType = new RenderAsIconType
				{
					AssetName = "impEel",
					IconToRender = IconToRender.Hook
				}
			}
		};
		item = resourceType;
		list.Add(item);
		resourceType = new ResourceType("scampBeetle");
		resourceType.ResourceItem = "item:scampBeetle";
		resourceType.Name = "Scamp beetle";
		resourceType.Color = Color.DarkBlue;
		resourceType.Category = GameData.Instance.AllResourceCategories["food"];
		resourceType.DetectionTag = "smallAnimalOnGround";
		resourceType.FractionOfMaximumToReplenishEachTime = 1f;
		resourceType.DaysOfYearToReplenish = new NormalDistribution[3]
		{
			new NormalDistribution
			{
				Mean = 0.0,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.4000000059604645,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.800000011920929,
				StandardDeviation = 0.05000000074505806
			}
		};
		resourceType.TileResourceType = new TileResourceType
		{
			RenderableType = new RenderableType("scampBeetle")
			{
				RenderAsIconType = new RenderAsIconType
				{
					AssetName = "scampBeetle",
					IconToRender = IconToRender.Bug
				}
			}
		};
		item = resourceType;
		list.Add(item);
		resourceType = new ResourceType("scampGrub");
		resourceType.ResourceItem = "item:scampGrub";
		resourceType.Name = "Scamp grub";
		resourceType.Color = Color.AliceBlue;
		resourceType.Category = GameData.Instance.AllResourceCategories["food"];
		resourceType.DetectionTag = "smallAnimalBelowGround";
		resourceType.FractionOfMaximumToReplenishEachTime = 1f;
		resourceType.DaysOfYearToReplenish = new NormalDistribution[3]
		{
			new NormalDistribution
			{
				Mean = 0.0,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.4000000059604645,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.800000011920929,
				StandardDeviation = 0.05000000074505806
			}
		};
		resourceType.TileResourceType = new TileResourceType
		{
			RenderableType = new RenderableType("scampGrub")
			{
				RenderAsIconType = new RenderAsIconType
				{
					AssetName = "scampGrub",
					IconToRender = IconToRender.Bug
				}
			}
		};
		item = resourceType;
		list.Add(item);
		resourceType = new ResourceType("spottedOilTubers");
		resourceType.ResourceItem = "item:spottedOilTubers";
		resourceType.Name = "Spotted oil tubers";
		resourceType.Color = Color.Azure;
		resourceType.Category = GameData.Instance.AllResourceCategories["food"];
		resourceType.DetectionTag = "fruitOnGroundHardToFind";
		resourceType.FractionOfMaximumToReplenishEachTime = 1f;
		resourceType.DaysOfYearToReplenish = new NormalDistribution[3]
		{
			new NormalDistribution
			{
				Mean = 0.0,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.4000000059604645,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.800000011920929,
				StandardDeviation = 0.05000000074505806
			}
		};
		resourceType.TileResourceType = new TileResourceType
		{
			RenderableType = new RenderableType("spottedOilTubers")
			{
				RenderAsIconType = new RenderAsIconType
				{
					AssetName = "spottedOilTubers",
					IconToRender = IconToRender.Ore
				}
			}
		};
		item = resourceType;
		list.Add(item);
		resourceType = new ResourceType("commonOilTubers");
		resourceType.ResourceItem = "item:commonOilTubers";
		resourceType.Name = "Common oil tubers";
		resourceType.Color = Color.Aquamarine;
		resourceType.Category = GameData.Instance.AllResourceCategories["food"];
		resourceType.DetectionTag = "fruitOnGroundHardToFind";
		resourceType.FractionOfMaximumToReplenishEachTime = 1f;
		resourceType.DaysOfYearToReplenish = new NormalDistribution[3]
		{
			new NormalDistribution
			{
				Mean = 0.0,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.4000000059604645,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.800000011920929,
				StandardDeviation = 0.05000000074505806
			}
		};
		resourceType.TileResourceType = new TileResourceType
		{
			RenderableType = new RenderableType("commonOilTubers")
			{
				RenderAsIconType = new RenderAsIconType
				{
					AssetName = "commonOilTubers",
					IconToRender = IconToRender.Ore
				}
			}
		};
		item = resourceType;
		list.Add(item);
		resourceType = new ResourceType("hexapineLeaves");
		resourceType.ResourceItem = "item:hexapineLeaves";
		resourceType.Name = "Hexapine leaves";
		resourceType.Color = Color.LawnGreen;
		resourceType.Category = GameData.Instance.AllResourceCategories["food"];
		resourceType.DetectionTag = "fruitOnGroundHardToFind";
		resourceType.FractionOfMaximumToReplenishEachTime = 1f;
		resourceType.DaysOfYearToReplenish = new NormalDistribution[3]
		{
			new NormalDistribution
			{
				Mean = 0.0,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.4000000059604645,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.800000011920929,
				StandardDeviation = 0.05000000074505806
			}
		};
		resourceType.TileResourceType = new TileResourceType
		{
			RenderableType = new RenderableType("hexapineLeaves")
			{
				RenderAsIconType = new RenderAsIconType
				{
					AssetName = "hexapineLeaves",
					IconToRender = IconToRender.Ore
				}
			}
		};
		item = resourceType;
		list.Add(item);
		resourceType = new ResourceType("fingerFruit");
		resourceType.ResourceItem = "item:fingerFruit";
		resourceType.Name = "Finger fruit";
		resourceType.Color = Color.CadetBlue;
		resourceType.Category = GameData.Instance.AllResourceCategories["food"];
		resourceType.DetectionTag = "fruitOnGroundHardToFind";
		resourceType.FractionOfMaximumToReplenishEachTime = 1f;
		resourceType.DaysOfYearToReplenish = new NormalDistribution[3]
		{
			new NormalDistribution
			{
				Mean = 0.0,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.4000000059604645,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.800000011920929,
				StandardDeviation = 0.05000000074505806
			}
		};
		resourceType.TileResourceType = new TileResourceType
		{
			RenderableType = new RenderableType("fingerFruit")
			{
				RenderAsIconType = new RenderAsIconType
				{
					AssetName = "fingerFruit",
					IconToRender = IconToRender.Ore
				}
			}
		};
		item = resourceType;
		list.Add(item);
		resourceType = new ResourceType("glassyCreeperPods");
		resourceType.ResourceItem = "item:glassyCreeperPods";
		resourceType.Name = "Glassy creeper pods";
		resourceType.Color = Color.LavenderBlush;
		resourceType.Category = GameData.Instance.AllResourceCategories["food"];
		resourceType.DetectionTag = "fruitAboveGround";
		resourceType.FractionOfMaximumToReplenishEachTime = 1f;
		resourceType.DaysOfYearToReplenish = new NormalDistribution[3]
		{
			new NormalDistribution
			{
				Mean = 0.0,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.4000000059604645,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.800000011920929,
				StandardDeviation = 0.05000000074505806
			}
		};
		resourceType.TileResourceType = new TileResourceType
		{
			RenderableType = new RenderableType("glassyCreeperPods")
			{
				RenderAsIconType = new RenderAsIconType
				{
					AssetName = "glassyCreeperPods",
					IconToRender = IconToRender.Ore
				}
			}
		};
		item = resourceType;
		list.Add(item);
		resourceType = new ResourceType("crystalBerries");
		resourceType.ResourceItem = "item:crystalBerries";
		resourceType.Name = "Crystal berries";
		resourceType.Color = Color.LawnGreen;
		resourceType.Category = GameData.Instance.AllResourceCategories["food"];
		resourceType.DetectionTag = "fruitAboveGround";
		resourceType.FractionOfMaximumToReplenishEachTime = 1f;
		resourceType.DaysOfYearToReplenish = new NormalDistribution[3]
		{
			new NormalDistribution
			{
				Mean = 0.0,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.4000000059604645,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.800000011920929,
				StandardDeviation = 0.05000000074505806
			}
		};
		resourceType.TileResourceType = new TileResourceType
		{
			RenderableType = new RenderableType("crystalBerries")
			{
				RenderAsIconType = new RenderAsIconType
				{
					AssetName = "crystalBerries",
					IconToRender = IconToRender.Ore
				}
			}
		};
		item = resourceType;
		list.Add(item);
		resourceType = new ResourceType("crop:sticks");
		resourceType.Name = "Sticks";
		resourceType.ResourceItem = "item:sticks";
		resourceType.Category = GameData.Instance.AllResourceCategories["rawMaterials"];
		resourceType.DetectionFlashDuration = value;
		resourceType.DetectionTag = "largeAboveGround";
		resourceType.FractionOfMaximumToReplenishEachTime = 0.5f;
		resourceType.DaysOfYearToReplenish = new NormalDistribution[2]
		{
			new NormalDistribution
			{
				Mean = 0.4000000059604645,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.75,
				StandardDeviation = 0.05000000074505806
			}
		};
		resourceType.CropType = new CropType
		{
			MaxSizeShareOfWholePlant = 0.05f,
			CropItemGrowthPerDay = 0.3f,
			AgeProduction = new Vector2[5]
			{
				new Vector2(0f, 0f),
				new Vector2(0.7f, 0f),
				new Vector2(1f, 1f),
				new Vector2(8f, 1f),
				new Vector2(10f, 0.5f)
			}
		};
		ResourceType item2 = resourceType;
		list.Add(item2);
		resourceType = new ResourceType("crop:spoakBranches");
		resourceType.Name = "Spoak branches";
		resourceType.ResourceItem = "item:spoakBranches";
		resourceType.Category = GameData.Instance.AllResourceCategories["rawMaterials"];
		resourceType.DetectionFlashDuration = value;
		resourceType.DetectionTag = "largeAboveGround";
		resourceType.FractionOfMaximumToReplenishEachTime = 0.5f;
		resourceType.DaysOfYearToReplenish = new NormalDistribution[2]
		{
			new NormalDistribution
			{
				Mean = 0.4000000059604645,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.699999988079071,
				StandardDeviation = 0.05000000074505806
			}
		};
		resourceType.CropType = new CropType
		{
			MaxSizeShareOfWholePlant = 0.05f,
			CropItemGrowthPerDay = 0.3f,
			BulkLimitToShowFlag = 0f,
			TreeSpriteFlag = StateModifier.HasBranches,
			AgeProduction = new Vector2[5]
			{
				new Vector2(0f, 0f),
				new Vector2(0.7f, 0f),
				new Vector2(1f, 1f),
				new Vector2(8f, 1f),
				new Vector2(10f, 0.5f)
			}
		};
		item2 = resourceType;
		list.Add(item2);
		resourceType = new ResourceType("crop:waterCaneLeaves");
		resourceType.Name = "Water cane leaves";
		resourceType.ResourceItem = "item:waterCaneLeaves";
		resourceType.Category = GameData.Instance.AllResourceCategories["rawMaterials"];
		resourceType.DetectionTag = "largeAboveGround";
		resourceType.DetectionFlashDuration = value;
		resourceType.FractionOfMaximumToReplenishEachTime = 1f;
		resourceType.DaysOfYearToReplenish = new NormalDistribution[2]
		{
			new NormalDistribution
			{
				Mean = 0.4000000059604645,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.699999988079071,
				StandardDeviation = 0.05000000074505806
			}
		};
		resourceType.CropType = new CropType
		{
			MaxSizeShareOfWholePlant = 0.05f,
			CropItemGrowthPerDay = 0.3f,
			AgeProduction = new Vector2[5]
			{
				new Vector2(0f, 0f),
				new Vector2(0.7f, 0f),
				new Vector2(1f, 1f),
				new Vector2(8f, 1f),
				new Vector2(10f, 0.5f)
			}
		};
		item2 = resourceType;
		list.Add(item2);
		resourceType = new ResourceType("crop:waterCaneStem");
		resourceType.Name = "Water cane stem";
		resourceType.ResourceItem = "item:waterCaneStem";
		resourceType.Category = GameData.Instance.AllResourceCategories["rawMaterials"];
		resourceType.DetectionTag = "largeAboveGround";
		resourceType.FractionOfMaximumToReplenishEachTime = 1f;
		resourceType.DaysOfYearToReplenish = new NormalDistribution[2]
		{
			new NormalDistribution
			{
				Mean = 0.44999998807907104,
				StandardDeviation = 0.029999999329447746
			},
			new NormalDistribution
			{
				Mean = 0.699999988079071,
				StandardDeviation = 0.029999999329447746
			}
		};
		resourceType.CropType = new CropType
		{
			MaxSizeShareOfWholePlant = 0.05f,
			CropItemGrowthPerDay = 0.3f,
			BulkLimitToShowFlag = 0f,
			TreeSpriteFlag = StateModifier.HasBranches,
			AgeProduction = new Vector2[5]
			{
				new Vector2(0f, 0f),
				new Vector2(0.7f, 0f),
				new Vector2(1f, 1f),
				new Vector2(8f, 1f),
				new Vector2(10f, 0.5f)
			}
		};
		item2 = resourceType;
		list.Add(item2);
		resourceType = new ResourceType("crop:waterCaneSeeds");
		resourceType.Name = "Water cane seeds";
		resourceType.ResourceItem = "item:waterCaneSeeds";
		resourceType.Category = GameData.Instance.AllResourceCategories["food"];
		resourceType.DetectionTag = "largeAboveGround";
		resourceType.DaysOfYearToReplenish = new NormalDistribution[3]
		{
			new NormalDistribution
			{
				Mean = 0.0,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.4000000059604645,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.800000011920929,
				StandardDeviation = 0.05000000074505806
			}
		};
		resourceType.CropType = new CropType
		{
			MaxSizeShareOfWholePlant = 0.05f,
			CropItemGrowthPerDay = 0.3f,
			AgeProduction = new Vector2[5]
			{
				new Vector2(0f, 0f),
				new Vector2(0.7f, 0f),
				new Vector2(1f, 1f),
				new Vector2(8f, 1f),
				new Vector2(10f, 0.5f)
			}
		};
		item2 = resourceType;
		list.Add(item2);
		resourceType = new ResourceType("crop:pigFlies");
		resourceType.Name = "Pig flies";
		resourceType.ResourceItem = "item:pigFliesLive";
		resourceType.Category = GameData.Instance.AllResourceCategories["rawMaterials"];
		resourceType.DetectionTag = "smallAnimalAboveGround";
		resourceType.DaysOfYearToReplenish = new NormalDistribution[3]
		{
			new NormalDistribution
			{
				Mean = 0.30000001192092896,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.6000000238418579,
				StandardDeviation = 0.05000000074505806
			},
			new NormalDistribution
			{
				Mean = 0.8999999761581421,
				StandardDeviation = 0.05000000074505806
			}
		};
		resourceType.CropType = new CropType
		{
			MaxSizeShareOfWholePlant = 0.05f,
			CropItemGrowthPerDay = 0.3f,
			AgeProduction = new Vector2[5]
			{
				new Vector2(0f, 0f),
				new Vector2(0.7f, 0f),
				new Vector2(1f, 1f),
				new Vector2(8f, 1f),
				new Vector2(10f, 0.5f)
			}
		};
		item2 = resourceType;
		list.Add(item2);
		resourceType = new ResourceType("crop:shadeleafBowStave");
		resourceType.Name = "Shadeleaf bow stave";
		resourceType.ResourceItem = "item:shadeleafBowStave";
		resourceType.Category = GameData.Instance.AllResourceCategories["rawMaterials"];
		resourceType.DetectionTag = "aboveGroundHardToSee";
		resourceType.DaysOfYearToReplenish = new NormalDistribution[2]
		{
			new NormalDistribution
			{
				Mean = 0.5,
				StandardDeviation = 0.019999999552965164
			},
			new NormalDistribution
			{
				Mean = 0.699999988079071,
				StandardDeviation = 0.019999999552965164
			}
		};
		resourceType.CropType = new CropType
		{
			MaxSizeShareOfWholePlant = 0.05f,
			CropItemGrowthPerDay = 0.3f,
			AgeProduction = new Vector2[5]
			{
				new Vector2(0f, 0f),
				new Vector2(0.7f, 0f),
				new Vector2(1f, 1f),
				new Vector2(8f, 1f),
				new Vector2(10f, 0.5f)
			}
		};
		item2 = resourceType;
		list.Add(item2);
		resourceType = new ResourceType("crop:shadeleafCanes");
		resourceType.Name = "Shadeleaf canes";
		resourceType.ResourceItem = "item:shadeleafCanes";
		resourceType.Category = GameData.Instance.AllResourceCategories["rawMaterials"];
		resourceType.DetectionTag = "largeAboveGround";
		resourceType.DetectionFlashDuration = value;
		resourceType.DaysOfYearToReplenish = new NormalDistribution[2]
		{
			new NormalDistribution
			{
				Mean = 0.5,
				StandardDeviation = 0.019999999552965164
			},
			new NormalDistribution
			{
				Mean = 0.699999988079071,
				StandardDeviation = 0.019999999552965164
			}
		};
		resourceType.CropType = new CropType
		{
			MaxSizeShareOfWholePlant = 0.05f,
			CropItemGrowthPerDay = 0.3f,
			BulkLimitToShowFlag = 0f,
			TreeSpriteFlag = StateModifier.HasBranches,
			AgeProduction = new Vector2[5]
			{
				new Vector2(0f, 0f),
				new Vector2(0.7f, 0f),
				new Vector2(1f, 1f),
				new Vector2(8f, 1f),
				new Vector2(10f, 0.5f)
			}
		};
		item2 = resourceType;
		list.Add(item2);
		resourceType = new ResourceType("crop:shadeleafResin");
		resourceType.Name = "Shadeleaf resin";
		resourceType.ResourceItem = "item:shadeleafResin";
		resourceType.Category = GameData.Instance.AllResourceCategories["rawMaterials"];
		resourceType.DetectionTag = "smallAboveGround";
		resourceType.DaysOfYearToReplenish = new NormalDistribution[2]
		{
			new NormalDistribution
			{
				Mean = 0.5,
				StandardDeviation = 0.019999999552965164
			},
			new NormalDistribution
			{
				Mean = 0.699999988079071,
				StandardDeviation = 0.019999999552965164
			}
		};
		resourceType.CropType = new CropType
		{
			MaxSizeShareOfWholePlant = 0.05f,
			CropItemGrowthPerDay = 0.3f,
			AgeProduction = new Vector2[5]
			{
				new Vector2(0f, 0f),
				new Vector2(0.7f, 0f),
				new Vector2(1f, 1f),
				new Vector2(8f, 1f),
				new Vector2(10f, 0.5f)
			}
		};
		item2 = resourceType;
		list.Add(item2);
		resourceType = new ResourceType("crop:giantHollowBud");
		resourceType.Name = "Bud from giant hollow";
		resourceType.ResourceItem = "item:giantHollowBud";
		resourceType.Category = GameData.Instance.AllResourceCategories["rawMaterials"];
		resourceType.DetectionTag = "largeAboveGround";
		resourceType.DetectionFlashDuration = value;
		resourceType.DaysOfYearToReplenish = new NormalDistribution[1]
		{
			new NormalDistribution
			{
				Mean = 0.5,
				StandardDeviation = 0.05000000074505806
			}
		};
		resourceType.CropType = new CropType
		{
			MaxSizeShareOfWholePlant = 0.05f,
			CropItemGrowthPerDay = 0.3f,
			AgeProduction = new Vector2[5]
			{
				new Vector2(0f, 0f),
				new Vector2(0.7f, 0f),
				new Vector2(1f, 1f),
				new Vector2(8f, 1f),
				new Vector2(10f, 0.5f)
			}
		};
		item2 = resourceType;
		list.Add(item2);
		resourceType = new ResourceType("crop:daysheenLeaves");
		resourceType.Name = "Daysheen leaves";
		resourceType.ResourceItem = "item:daysheenLeaves";
		resourceType.DetectionFlashDuration = value;
		resourceType.Category = GameData.Instance.AllResourceCategories["rawMaterials"];
		resourceType.DetectionTag = "largeAboveGround";
		resourceType.DaysOfYearToReplenish = new NormalDistribution[2]
		{
			new NormalDistribution
			{
				Mean = 0.5,
				StandardDeviation = 0.019999999552965164
			},
			new NormalDistribution
			{
				Mean = 0.699999988079071,
				StandardDeviation = 0.019999999552965164
			}
		};
		resourceType.CropType = new CropType
		{
			MaxSizeShareOfWholePlant = 0.05f,
			CropItemGrowthPerDay = 0.3f,
			BulkLimitToShowFlag = 0f,
			TreeSpriteFlag = StateModifier.HasBranches,
			AgeProduction = new Vector2[5]
			{
				new Vector2(0f, 0f),
				new Vector2(0.7f, 0f),
				new Vector2(1f, 1f),
				new Vector2(8f, 1f),
				new Vector2(10f, 0.5f)
			}
		};
		item2 = resourceType;
		list.Add(item2);
		resourceType = new ResourceType("crop:marshcotSap");
		resourceType.Name = "Marshcot sap";
		resourceType.ResourceItem = "item:marshcotSap";
		resourceType.Category = GameData.Instance.AllResourceCategories["rawMaterials"];
		resourceType.DetectionTag = "aboveGroundHardToSee";
		resourceType.FractionOfMaximumToReplenishEachTime = 0.7f;
		resourceType.DaysOfYearToReplenish = daysOfYearToReplenish2;
		resourceType.CropType = new CropType
		{
			MaxSizeShareOfWholePlant = 0.05f,
			CropItemGrowthPerDay = 0.3f,
			AgeProduction = new Vector2[5]
			{
				new Vector2(0f, 0f),
				new Vector2(0.7f, 0f),
				new Vector2(1f, 1f),
				new Vector2(8f, 1f),
				new Vector2(10f, 0.5f)
			}
		};
		item2 = resourceType;
		list.Add(item2);
		resourceType = new ResourceType("crop:wingweedLeaves");
		resourceType.Name = "Wingweed leaves";
		resourceType.ResourceItem = "item:wingweedLeaves";
		resourceType.DetectionFlashDuration = value;
		resourceType.Category = GameData.Instance.AllResourceCategories["rawMaterials"];
		resourceType.DetectionTag = "largeAboveGround";
		resourceType.DaysOfYearToReplenish = new NormalDistribution[2]
		{
			new NormalDistribution
			{
				Mean = 0.5,
				StandardDeviation = 0.019999999552965164
			},
			new NormalDistribution
			{
				Mean = 0.699999988079071,
				StandardDeviation = 0.019999999552965164
			}
		};
		resourceType.CropType = new CropType
		{
			DetectionPulsingDuration = 500f,
			MaxSizeShareOfWholePlant = 0.7f,
			CropItemGrowthPerDay = 0.3f,
			BulkLimitToShowFlag = 0f,
			TreeSpriteFlag = StateModifier.HasBranches,
			AgeProduction = new Vector2[5]
			{
				new Vector2(0f, 0f),
				new Vector2(0.7f, 0f),
				new Vector2(1f, 1f),
				new Vector2(8f, 1f),
				new Vector2(10f, 0.5f)
			}
		};
		item2 = resourceType;
		list.Add(item2);
		return list;
	}
}
