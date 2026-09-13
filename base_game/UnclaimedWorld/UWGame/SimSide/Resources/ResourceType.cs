using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Resources;

[DebuggerDisplay("{KeyName}")]
public class ResourceType : IGameData, IHasCategory<ResourceCategory>, IXmlSerializable, IDetectableType
{
	public string ResourceItem;

	[XmlIgnore]
	public EntityType ResourceItemType;

	public TileResourceType TileResourceType;

	public CropType CropType;

	public float FractionOfMaximumToReplenishEachTime = 1f;

	public NormalDistribution[] DaysOfYearToReplenish;

	[XmlIgnore]
	public List<NormalDistribution> OrderedDaysOfYearToReplenish;

	public string DetectionTag;

	public Color? Color;

	public float? DetectionFlashDuration;

	public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(ResourceType))
	{
		TypeMappings = DataLoader.GetListOfTypeMappings(useEntityTypePlaceholders: true)
	};

	public string KeyName { get; set; }

	public string Name { get; set; }

	public bool DeleteRecord { get; set; }

	public ResourceCategory Category { get; set; }

	public ResourceType(string keyName)
	{
		KeyName = keyName;
	}

	public ResourceType()
	{
	}

	public bool CanReplenish()
	{
		return DaysOfYearToReplenish != null;
	}

	public double ComputeReplenishDaysFromNow(ref ushort? indexOfLastReplenishPoint)
	{
		DateAndTime.TimeDateYear currentTimeDateYear = The.Sim.DateAndTime.CurrentTimeDateYear;
		double num = (double)currentTimeDateYear.Day + currentTimeDateYear.TimeOfDay;
		bool flag = false;
		NormalDistribution normalDistribution;
		if (OrderedDaysOfYearToReplenish.Count > 1)
		{
			if (!indexOfLastReplenishPoint.HasValue)
			{
				int? num2 = null;
				for (int i = 0; i < OrderedDaysOfYearToReplenish.Count; i++)
				{
					if (OrderedDaysOfYearToReplenish[i].GetMean() * 12.0 > num)
					{
						num2 = i;
						break;
					}
				}
				if (!num2.HasValue)
				{
					flag = true;
					num2 = 0;
				}
				indexOfLastReplenishPoint = (ushort)num2.Value;
			}
			else
			{
				indexOfLastReplenishPoint++;
				if (indexOfLastReplenishPoint > DaysOfYearToReplenish.Length - 1)
				{
					indexOfLastReplenishPoint = 0;
					normalDistribution = OrderedDaysOfYearToReplenish[indexOfLastReplenishPoint.Value];
					if (normalDistribution.GetMean() < num)
					{
						flag = true;
					}
				}
			}
		}
		else
		{
			indexOfLastReplenishPoint = 0;
			normalDistribution = OrderedDaysOfYearToReplenish[indexOfLastReplenishPoint.Value];
			if (normalDistribution.GetMean() < num)
			{
				flag = true;
			}
		}
		normalDistribution = OrderedDaysOfYearToReplenish[indexOfLastReplenishPoint.Value];
		double totalDays = 12.0 * normalDistribution.GetRandomValue(The.Sim.GameplayRandomGenerator);
		int num3 = currentTimeDateYear.Year;
		if (flag)
		{
			num3++;
		}
		DateAndTime.TimeDateYear timeDateYear = new DateAndTime.TimeDateYear(totalDays);
		timeDateYear.AddTime((double)num3 * 12.0);
		if (!flag && timeDateYear.TotalDays < currentTimeDateYear.TotalDays)
		{
			timeDateYear = new DateAndTime.TimeDateYear(currentTimeDateYear.TotalDays);
		}
		return timeDateYear.TotalDays - currentTimeDateYear.TotalDays;
	}

	public int GetHarvestableItemsFromBulk(float totalHarvestableBulk)
	{
		return (int)(totalHarvestableBulk / ResourceItemType.ItemType.MaximumBulk.Value);
	}

	public void Initialize()
	{
		if (!string.IsNullOrEmpty(DetectionTag))
		{
			DataLoader.AddToTagCollection(this, DetectionTag, GameData.Instance.DetectableTypeByTag);
		}
		if (TileResourceType != null)
		{
			TileResourceType.Initialize();
		}
		if (DaysOfYearToReplenish != null)
		{
			OrderedDaysOfYearToReplenish = DaysOfYearToReplenish.OrderBy((NormalDistribution d) => d.GetMean()).ToList();
		}
	}

	public void PostLoadContentInitialize()
	{
	}

	public bool ShouldSerializeColor()
	{
		return Color.HasValue;
	}

	public void PreInitValidate(ref List<string> errors)
	{
		EntityType.ValidateRequiredValue(ref errors, "Name", !string.IsNullOrEmpty(Name));
		EntityType.ValidateRequiredValue(ref errors, "Category", Category != null);
	}

	public void PostInitValidate(ref List<string> errors)
	{
		if (OrderedDaysOfYearToReplenish == null || OrderedDaysOfYearToReplenish.Count <= 1)
		{
			return;
		}
		float? num = null;
		foreach (NormalDistribution item in OrderedDaysOfYearToReplenish)
		{
			float min = item.GetMin();
			float max = item.GetMax();
			if (num.HasValue && min < num.Value)
			{
				EntityType.CreateValidationError(ref errors, "Resource replenish date intervals must not overlap.");
			}
			num = max;
		}
	}

	public void PostDataCompleteInitialize()
	{
		ResourceItemType = GameData.Instance.AllEntityTypes[ResourceItem];
		GameData.Instance.ItemHarvestSource.Add(ResourceItemType, this);
	}

	public void PreDataCompleteValidate(ref List<string> listOfErrors)
	{
	}

	public void PostDataCompleteValidate(ref List<string> listOfErrors)
	{
	}

	public XmlSchema GetSchema()
	{
		return null;
	}

	public void ReadXml(XmlReader reader)
	{
		CustomXmlSerializer.ReadXmlDeserialize(this, reader, _proxyData);
	}

	public void WriteXml(XmlWriter writer)
	{
		CustomXmlSerializer.WriteXmlSerialize(this, writer, _proxyData);
	}
}
