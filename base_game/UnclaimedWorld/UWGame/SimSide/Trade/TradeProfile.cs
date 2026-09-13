using System.Collections.Generic;
using UWGame.SimSide.Entities;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Trade;

public class TradeProfile : IGameData
{
	public string Comments;

	public string[] HighTrade;

	public string[] MediumTrade;

	public string[] LowTrade;

	public RandomTrade RandomHighTrade;

	public RandomTrade RandomMediumTrade;

	public RandomTrade RandomLowTrade;

	public string[] HighExport;

	public RandomTrade RandomHighExport;

	public string[] MediumExport;

	public RandomTrade RandomMediumExport;

	public string[] LowExport;

	public RandomTrade RandomLowExport;

	public string[] HighImport;

	public RandomTrade RandomHighImport;

	public string[] MediumImport;

	public RandomTrade RandomMediumImport;

	public string[] LowImport;

	public RandomTrade RandomLowImport;

	public SerializableDictionary<string, int> TradeGroupPriority;

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

	private void ValidateRandomTradeGroups(ref List<string> listOfErrors, RandomTrade randomTrade)
	{
		if (randomTrade != null)
		{
			string[] options = randomTrade.Options;
			foreach (string key in options)
			{
				EntityType.ValidateGameDataTypeExists(ref listOfErrors, key, GameData.Instance.AllTradeGroups);
			}
		}
	}

	private void ValidateTradeGroups(ref List<string> listOfErrors, string[] groups)
	{
		if (groups != null)
		{
			foreach (string key in groups)
			{
				EntityType.ValidateGameDataTypeExists(ref listOfErrors, key, GameData.Instance.AllTradeGroups);
			}
		}
	}

	public void PostDataCompleteValidate(ref List<string> listOfErrors)
	{
		ValidateTradeGroups(ref listOfErrors, LowTrade);
		ValidateTradeGroups(ref listOfErrors, MediumTrade);
		ValidateTradeGroups(ref listOfErrors, HighTrade);
		ValidateTradeGroups(ref listOfErrors, LowImport);
		ValidateTradeGroups(ref listOfErrors, MediumImport);
		ValidateTradeGroups(ref listOfErrors, HighImport);
		ValidateTradeGroups(ref listOfErrors, LowExport);
		ValidateTradeGroups(ref listOfErrors, MediumExport);
		ValidateTradeGroups(ref listOfErrors, HighExport);
		ValidateRandomTradeGroups(ref listOfErrors, RandomHighTrade);
		ValidateRandomTradeGroups(ref listOfErrors, RandomMediumTrade);
		ValidateRandomTradeGroups(ref listOfErrors, RandomLowTrade);
		ValidateRandomTradeGroups(ref listOfErrors, RandomHighExport);
		ValidateRandomTradeGroups(ref listOfErrors, RandomMediumExport);
		ValidateRandomTradeGroups(ref listOfErrors, RandomLowExport);
		ValidateRandomTradeGroups(ref listOfErrors, RandomHighImport);
		ValidateRandomTradeGroups(ref listOfErrors, RandomMediumImport);
		ValidateRandomTradeGroups(ref listOfErrors, RandomLowImport);
	}
}
