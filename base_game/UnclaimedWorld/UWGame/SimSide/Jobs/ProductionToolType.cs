using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Jobs;

public class ProductionToolType : IXmlSerializable
{
	public EntityType ToolItem;

	public float ProductivityBonus;

	public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(ProductionToolType))
	{
		TypeMappings = new List<CustomXmlSerializer.XmlTypeMappingBase>
		{
			new CustomXmlSerializer.XmlTypeMapping<EntityType, string>
			{
				GetterMethod = (EntityType t) => t?.KeyName,
				SetterMethod = (string s) => (s != null) ? GameData.Instance.AllItemTypes[s] : null
			}
		}
	};

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
