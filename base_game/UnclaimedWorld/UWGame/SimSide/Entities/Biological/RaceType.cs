using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Entities.Biological;

public class RaceType : IXmlSerializable, IEdge
{
	public string KeyName;

	public string Name;

	public string ModelName;

	public string ModelBasicTextureName;

	public string[] ModelBasicTextureNames;

	public float? ModelScale;

	public string Description;

	public string PortraitSkinType;

	public Vector3? PrimaryColor;

	public List<ColorProbability> SecondaryColorProbabilityEdges;

	public Vector3? TertiaryColor;

	public Vector3? QuaternaryColor;

	public float? Size;

	public SerializableDictionary<string, BioProperty> BioProperties;

	public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(RaceType))
	{
		TypeMappings = new List<CustomXmlSerializer.XmlTypeMappingBase>
		{
			new CustomXmlSerializer.XmlTypeMapping<Vector3?, string>
			{
				GetterMethod = (Vector3? t) => t.HasValue ? PersonType.Vector3ToHexString(t.Value) : null,
				SetterMethod = (string s) => (s != null) ? new Vector3?(PersonType.HexStringToVector3(s)) : ((Vector3?)null)
			}
		}
	};

	[XmlElement("ProbabilityEdge")]
	public float Edge { get; set; }

	public void Initialize(int raceNo)
	{
		if (string.IsNullOrEmpty(Name))
		{
			Name = "Race #" + raceNo;
		}
	}

	public bool GetBioPropertyValue(BioPropertyType propertyKey, out BioProperty property)
	{
		return BiologicalEntity.GetBioPropertyValue(propertyKey, BioProperties, out property);
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
