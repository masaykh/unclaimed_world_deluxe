using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UWGame.SimSide.AllGameData;

namespace UWGame.SimSide.Entities.Body;

public class MachineBodyPartType : BodyPartType, IXmlSerializable
{
	public float ManSecondsOfWorkNeeded;

	public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(MachineBodyPartType))
	{
		TypeMappings = DataLoader.GetListOfTypeMappings()
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
