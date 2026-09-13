using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Vegetation;

namespace UWGame.SimSide.Soil;

public class SoilComponentType : RenderedTerrainType
{
	public new string Name;

	public float MoveFactor;

	public RenderAsRocksType RenderAsRocksType;

	public bool ScaleDisplayAmountAsWithRocks;

	private Color dryTint;

	private Color wetTint;

	[XmlIgnore]
	public Vector4 WetTintAsVector;

	[XmlIgnore]
	public Vector4 DryTintAsVector;

	public Color DryTint
	{
		get
		{
			return dryTint;
		}
		set
		{
			dryTint = value;
			DryTintAsVector = dryTint.ToVector4();
			DryTintAsVector.W = 1f;
		}
	}

	public Color WetTint
	{
		get
		{
			return wetTint;
		}
		set
		{
			wetTint = value;
			WetTintAsVector = wetTint.ToVector4();
			WetTintAsVector.W = 1f;
		}
	}

	public SoilComponentType()
	{
		WetTint = new Color(240, 240, 240, 255);
		DryTint = new Color(255, 255, 255, 255);
	}

	public SoilComponentType(string keyName)
		: this()
	{
		base.KeyName = keyName;
	}

	public void PostLoadContentInitialize()
	{
		if (RenderAsRocksType == null)
		{
			RenderOrder = RenderedTerrainType.HighestOrder;
			RenderedTerrainType.HighestOrder++;
		}
		else
		{
			RenderOrder = RenderedTerrainType.HighestOrder + 100;
			RenderedTerrainType.HighestOrder++;
		}
	}
}
