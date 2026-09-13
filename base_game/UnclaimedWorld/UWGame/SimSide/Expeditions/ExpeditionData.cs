using System.Collections.Generic;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.Policies;
using UWGame.SimSide.Trade;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Expeditions;

public class ExpeditionData : IGameData
{
	public string AllegianceKey;

	public EvalNode Location;

	public float? SizeFactor;

	public string TradeProfile;

	public string PricesProfile;

	public string StructuresProfile;

	public string VehiclesProfile;

	public SerializableDictionary<string, TradeAmountType> AvailableForTrade;

	public SerializableDictionary<string, VehiclesForHireType> VehiclesForHire;

	public ExpeditionPolicyData PolicyData;

	public PopulationData PopulationData;

	public string KeyName { get; set; }

	public string Name { get; set; }

	public bool DeleteRecord { get; set; }

	public void PostDataCompleteValidate(ref List<string> listOfErrors)
	{
		if (AvailableForTrade != null)
		{
			foreach (KeyValuePair<string, TradeAmountType> item in AvailableForTrade)
			{
				EntityType.ValidateEntityTypeKeyExists(ref listOfErrors, item.Key);
			}
		}
		if (VehiclesForHire != null)
		{
			foreach (KeyValuePair<string, VehiclesForHireType> item2 in VehiclesForHire)
			{
				EntityType.ValidateEntityTypeKeyExists(ref listOfErrors, item2.Key);
			}
		}
		if (PricesProfile != null)
		{
			EntityType.ValidateGameDataTypeExists(ref listOfErrors, PricesProfile, GameData.Instance.AllPricesProfiles, out var _);
		}
		if (StructuresProfile != null)
		{
			EntityType.ValidateGameDataTypeExists(ref listOfErrors, StructuresProfile, GameData.Instance.AllStructuresProfiles, out var _);
		}
		if (VehiclesProfile != null)
		{
			EntityType.ValidateGameDataTypeExists(ref listOfErrors, VehiclesProfile, GameData.Instance.AllVehiclesProfiles, out var _);
			if (VehiclesForHire != null)
			{
				EntityType.CreateValidationError(ref listOfErrors, "VehiclesForHire and VehiclesProfile cannot both be specified");
			}
		}
		if (PopulationData != null)
		{
			PopulationData.PostDataCompleteValidate(ref listOfErrors);
		}
		if (PolicyData != null)
		{
			PolicyData.PostDataCompleteValidate(ref listOfErrors);
		}
	}

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
}
