using System.Collections.Generic;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;

namespace UWGame.SimSide.InGameEvents.Actions;

public class CreateExpeditionAction : EventActionType
{
	public ExpeditionData ExpeditionData;

	public string ExpeditionDataKey;

	public string AllegianceKey;

	public StatsData StatsData;

	public CreateExpeditionAction(string keyName)
		: base(keyName)
	{
	}

	public CreateExpeditionAction()
	{
	}

	public override bool Execute(EventAction action, ref string failReason)
	{
		ExpeditionData expeditionData = ((ExpeditionDataKey == null) ? ExpeditionData : GameData.Instance.AllExpeditionData[ExpeditionDataKey]);
		string key = AllegianceKey ?? expeditionData.AllegianceKey;
		Allegiance allegianceFromKey = The.Sim.World.GetAllegianceFromKey(key);
		if (allegianceFromKey == null)
		{
			return false;
		}
		return Expedition.CreateFromExpeditionData(expeditionData, allegianceFromKey, action, null, null, out failReason);
	}

	public override void PostDataCompleteValidate(ref List<string> listOfErrors)
	{
		if (ExpeditionData != null)
		{
			ExpeditionData.PostDataCompleteValidate(ref listOfErrors);
		}
	}

	public override void PreInitValidate(ref List<string> listOfErrors)
	{
		base.PreInitValidate(ref listOfErrors);
		if (ExpeditionData != null)
		{
			EntityType.ValidateRequiredValue(ref listOfErrors, "Allegiance key", !string.IsNullOrEmpty(ExpeditionData.AllegianceKey));
		}
		else
		{
			EntityType.ValidateGameDataTypeExists(ref listOfErrors, ExpeditionDataKey, GameData.Instance.AllExpeditionData, out var _);
		}
	}

	public override string ToString()
	{
		if (ExpeditionData != null)
		{
			return "Spawn expedition " + ExpeditionData.KeyName;
		}
		return "Spawn expedition " + ExpeditionDataKey;
	}
}
