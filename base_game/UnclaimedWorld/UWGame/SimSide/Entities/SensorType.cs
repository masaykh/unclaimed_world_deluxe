using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UWGame.SimSide.AllGameData;

namespace UWGame.SimSide.Entities;

public class SensorType : IXmlSerializable
{
	public float Range = 480f;

	public float RangeAtNight = 200f;

	public float PowerNeeds;

	public string DetectionTypeKey;

	public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(SensorType))
	{
		TypeMappings = DataLoader.GetListOfTypeMappings(useEntityTypePlaceholders: true)
	};

	[XmlIgnore]
	public DetectionType DetectionType { get; private set; }

	public void Initialize()
	{
	}

	public void PostInitValidate(ref List<string> listOfErrors)
	{
	}

	public void PostLoadContentInitialize()
	{
		if (!string.IsNullOrEmpty(DetectionTypeKey))
		{
			DetectionType = GameData.Instance.AllDetectionTypes[DetectionTypeKey];
		}
		if (DetectionType == null)
		{
			DetectionType = new DetectionType();
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
