using System;
using System.Collections.Generic;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Entities;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Policies;

public class ExpeditionPolicyData
{
	public float? FractionIndependentsAllowedToSleep;

	public int? IndependentsAllowedToSleep;

	public SerializableDictionary<RatingTypes, string> CurrentTiers;

	public SerializableDictionary<string, bool> AllowAmmoUseAgainstVermin;

	public void PostDataCompleteValidate(ref List<string> listOfErrors)
	{
		if (CurrentTiers != null)
		{
			foreach (KeyValuePair<RatingTypes, string> currentTier in CurrentTiers)
			{
				EntityType.ValidateGameDataTypeExists(ref listOfErrors, currentTier.Value, GameData.Instance.AllTierTypes, out var _);
			}
			foreach (object value2 in Enum.GetValues(typeof(RatingTypes)))
			{
				if (!CurrentTiers.TryGetValue((RatingTypes)value2, out var _))
				{
					EntityType.CreateValidationError(ref listOfErrors, "Missing tier");
				}
			}
		}
		if (AllowAmmoUseAgainstVermin == null)
		{
			return;
		}
		foreach (KeyValuePair<string, bool> item in AllowAmmoUseAgainstVermin)
		{
			EntityType.ValidateGameDataTypeExists(ref listOfErrors, item.Key, GameData.Instance.AllEntityTypes, out var _);
		}
	}
}
