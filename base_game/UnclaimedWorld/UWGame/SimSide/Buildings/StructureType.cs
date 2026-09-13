using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using SpriteSheetRuntime;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Processes;

namespace UWGame.SimSide.Buildings;

[XmlRoot("Structure")]
public class StructureType : IXmlSerializable
{
	public bool BuildByPlayer;

	public bool UsesAnchor;

	[XmlIgnore]
	public int WidthInTiles;

	[XmlIgnore]
	public int HeightInTiles;

	public bool IsCamp;

	public bool IsRoad;

	public bool IsAddon;

	[XmlIgnore]
	public AddonSize AddonSize;

	public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(StructureType))
	{
		TypeMappings = new List<CustomXmlSerializer.XmlTypeMappingBase>
		{
			new CustomXmlSerializer.XmlTypeMapping<EntityType, string>
			{
				GetterMethod = (EntityType t) => t?.KeyName,
				SetterMethod = (string s) => (s != null) ? GameData.Instance.AllEntityTypes[s] : null
			}
		}
	};

	public void Initialize(EntityType parent)
	{
	}

	public void MarkAnchorStructures(EntityType parent)
	{
		if (!GameData.Instance.ProcessYieldsThisOutput.TryGetValue(parent, out var value))
		{
			return;
		}
		foreach (ProcessType item in value)
		{
			if (item.UsesAnchor())
			{
				UsesAnchor = true;
				break;
			}
		}
	}

	public void PostLoadContentInitialize(EntityType parent)
	{
		_ = IsAddon;
		WidthInTiles = 1;
		HeightInTiles = 1;
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
