using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Combat;
using UWGame.SimSide.Entities.Body;

namespace UWGame.SimSide.Entities;

public class DefendActionType : IXmlSerializable
{
	public BodyPartType[] DependsOn;

	private float Period;

	private float EnergyCost;

	public string AnimationKey;

	public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(AttackType))
	{
		TypeMappings = DataLoader.GetListOfTypeMappings()
	};

	public override string ToString()
	{
		return AnimationKey;
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
