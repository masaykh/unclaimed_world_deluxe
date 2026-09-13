using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Combat;

namespace UWGame.SimSide.Items;

public class WeaponType : IXmlSerializable
{
	public AttackType[] AttackTypes;

	public bool? IsIntrinsic;

	[XmlIgnore]
	public string HighlyEffectiveAgainst;

	[XmlIgnore]
	public string NotEffectiveAgainst;

	public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(WeaponType))
	{
		TypeMappings = DataLoader.GetListOfTypeMappings(useEntityTypePlaceholders: true)
	};

	public float GetHighestDefenseRating()
	{
		return AttackTypes.Max((AttackType a) => a.DefenseRating ?? 0f);
	}

	public void PostLoadContentInitialize()
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
