using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Resources;

namespace UWGame.SimSide.Vegetation;

public class LowVegetationType : RenderedTerrainType
{
	public float MoveFactor;

	public Color DryTint;

	public Color LushTint;

	public bool CanGrowUnderWater;

	public string[] Crops;

	[XmlIgnore]
	public List<ResourceType> CropTypes;

	public LowVegetationType()
	{
	}

	public LowVegetationType(string keyName)
	{
		base.KeyName = keyName;
	}

	public void PostLoadContentInitialize()
	{
		RenderOrder = RenderedTerrainType.HighestOrder;
		RenderedTerrainType.HighestOrder++;
		if (Crops != null)
		{
			string[] crops = Crops;
			foreach (string key in crops)
			{
				CropTypes.Add(GameData.Instance.AllResourceTypes[key]);
			}
		}
	}
}
