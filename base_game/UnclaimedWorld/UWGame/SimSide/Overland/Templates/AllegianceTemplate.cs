using System.Collections.Generic;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Maps.MapEditor;

namespace UWGame.SimSide.Overland.Templates;

public class AllegianceTemplate : IGameData
{
	public StringChanceSet[] Expeditions;

	public string KeyName { get; set; }

	public string Name { get; set; }

	public bool DeleteRecord { get; set; }

	public void FillAllegiance(Allegiance allegiance, float? sizeFactor, string expeditionKeyName = null)
	{
		if (Expeditions != null)
		{
			StringChanceSet[] expeditions = Expeditions;
			for (int i = 0; i < expeditions.Length; i++)
			{
				int stairstep;
				StringChance stairStepIndex = Common.GetStairStepIndex(expeditions[i].Chances, out stairstep, The.Sim.GameplayRandomGenerator);
				Expedition.CreateFromExpeditionData(GameData.Instance.AllExpeditionData[stairStepIndex.String], allegiance, null, sizeFactor, expeditionKeyName, out var _);
			}
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

	public void PostDataCompleteValidate(ref List<string> listOfErrors)
	{
	}
}
