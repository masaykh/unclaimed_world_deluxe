using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;

namespace UWGame.SimSide.Entities.Biological;

public class ColorProbability : IXmlSerializable, IEdge
{
	public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(ColorProbability))
	{
		TypeMappings = new List<CustomXmlSerializer.XmlTypeMappingBase>
		{
			new CustomXmlSerializer.XmlTypeMapping<Vector3, string>
			{
				GetterMethod = (Vector3 t) => PersonType.Vector3ToHexString(t),
				SetterMethod = (string s) => PersonType.HexStringToVector3(s)
			}
		}
	};

	public float Edge { get; set; }

	public Vector3 Color { get; set; }

	public ColorProbability(float key, Vector3 value)
	{
		Edge = key;
		Color = value;
	}

	public ColorProbability()
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
