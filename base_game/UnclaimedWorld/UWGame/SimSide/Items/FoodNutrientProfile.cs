using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UWGame.SimSide.AllGameData;

namespace UWGame.SimSide.Items;

public class FoodNutrientProfile : IGameData, IXmlSerializable
{
	public FoodNutrientAmount[] FoodNutrientTypes;

	public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(FoodNutrientProfile))
	{
		TypeMappings = DataLoader.GetListOfTypeMappings()
	};

	public string KeyName { get; set; }

	public string Name { get; set; }

	public bool DeleteRecord { get; set; }

	public void Initialize()
	{
	}

	public void PreInitValidate(ref List<string> listOfErrors)
	{
	}

	public void PostInitValidate(ref List<string> listOfErrors)
	{
	}

	public void PostDataCompleteInitialize()
	{
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
