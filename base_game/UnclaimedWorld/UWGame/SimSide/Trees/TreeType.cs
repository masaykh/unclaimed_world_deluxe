using System.Collections.Generic;
using System.Xml.Serialization;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Resources;

namespace UWGame.SimSide.Trees;

public class TreeType : IAddonType
{
	public float MatureAge;

	public float MaxAge;

	public float BulkPerSize;

	public int MaxFlavours = 1;

	public bool HasSummerWinterCycle;

	public float SizeImpact;

	public string[] Crops;

	[XmlIgnore]
	public List<ResourceType> CropTypes;

	public DefaultCrops[] DefaultCrops;

	public float MassLossPercentagePerSecond;

	public float FibrousPercentageOfTotalMass;

	public float LumberPercentageOfFiberMass;

	public float WaterNeedsPerBulk;

	public float NitrogenNeedsPerBulk;

	public float PhosphorousNeedsPerBulk;

	public float BulkGrowthSpeedInSeconds;

	public float SelfSustainmentNeedsInBulkPercentagePerSecond;

	public void Initialize()
	{
		if (DefaultCrops != null)
		{
			DefaultCrops[] defaultCrops = DefaultCrops;
			for (int i = 0; i < defaultCrops.Length; i++)
			{
				defaultCrops[i].Initialize();
			}
		}
	}

	public void PostLoadContentInitialize()
	{
		if (Crops != null)
		{
			CropTypes = new List<ResourceType>();
			string[] crops = Crops;
			foreach (string key in crops)
			{
				CropTypes.Add(GameData.Instance.AllResourceTypes[key]);
			}
		}
	}
}
