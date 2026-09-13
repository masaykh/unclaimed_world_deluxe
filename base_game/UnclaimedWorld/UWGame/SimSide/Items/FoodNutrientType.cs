using System.Collections.Generic;
using System.Xml.Serialization;

namespace UWGame.SimSide.Items;

public class FoodNutrientType : IGameData
{
	[XmlIgnore]
	public bool SatisfiesComfort;

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

	public void PreDataCompleteValidate(ref List<string> listOfErrors)
	{
	}

	public void PostDataCompleteInitialize()
	{
	}

	public void PostDataCompleteValidate(ref List<string> listOfErrors)
	{
	}

	public FoodNutrientType(string keyName)
	{
		KeyName = keyName;
	}

	public FoodNutrientType()
	{
	}

	public override string ToString()
	{
		return Name;
	}
}
