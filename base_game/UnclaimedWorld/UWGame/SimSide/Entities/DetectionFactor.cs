using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UWGame.SimSide.AllGameData;

namespace UWGame.SimSide.Entities;

public class DetectionFactor : IXmlSerializable
{
	public string TypeKey;

	public string TypeTag;

	[XmlIgnore]
	public List<IDetectableType> DetectableTypes = new List<IDetectableType>();

	public float? DistanceToAlwaysDetect;

	public bool RequiresExamineAction;

	public bool AddLogMessageWhenDetected = true;

	public float Value;

	public SkillType SkillToUse;

	public float? InterestLevelForSpottedResourceMean;

	public float? InterestLevelForSpottedResourceStdDeviation;

	public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(DetectionFactor))
	{
		TypeMappings = DataLoader.GetListOfTypeMappings()
	};

	public bool ShouldSerializeDistanceToAlwaysDetect()
	{
		return DistanceToAlwaysDetect.HasValue;
	}

	public void PostLoadContentInitialize(ref List<string> errors)
	{
		_ = TypeTag == "inDeeperWaterFishingSpot";
		if (!string.IsNullOrEmpty(TypeTag))
		{
			DetectableTypes = GameData.Instance.DetectableTypeByTag[TypeTag];
		}
		if (!string.IsNullOrEmpty(TypeKey))
		{
			IDetectableType detectableType = null;
			EntityType value2;
			if (GameData.Instance.AllResourceTypes.TryGetValue(TypeKey, out var value))
			{
				detectableType = value;
			}
			else if (GameData.Instance.AllEntityTypes.TryGetValue(TypeKey, out value2))
			{
				detectableType = value2;
			}
			else
			{
				EntityType.CreateValidationError(ref errors, $"TypeKey {TypeKey} not found.");
			}
			if (detectableType != null && !DetectableTypes.Contains(detectableType))
			{
				DetectableTypes.Add(detectableType);
			}
		}
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
