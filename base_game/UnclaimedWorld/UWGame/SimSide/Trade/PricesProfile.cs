using System.Collections.Generic;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Trade;

public class PricesProfile : IGameData
{
	public string Comments;

	public SerializableDictionary<string, float> Prices;

	public string KeyName { get; set; }

	public string Name { get; set; }

	public bool DeleteRecord { get; set; }

	public void PreInitValidate(ref List<string> errors)
	{
	}

	public void Initialize()
	{
	}

	public void PostInitValidate(ref List<string> errors)
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
}
