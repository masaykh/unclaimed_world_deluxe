using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Overland;

namespace UWGame.SimSide.InGameEvents.Actions;

public class SpawnAllegianceAction : EventActionType
{
	public ExpeditionData ExpeditionData;

	public string AllegianceDataKey;

	public AllegianceData AllegianceData;

	public string Site;

	public SpawnAllegianceAction(string keyName)
		: base(keyName)
	{
	}

	public SpawnAllegianceAction()
	{
	}

	public override bool Execute(EventAction action, ref string failReason)
	{
		AllegianceData allegianceData = ((AllegianceDataKey == null) ? AllegianceData : GameData.Instance.AllAllegianceData[AllegianceDataKey]);
		if (The.Sim.World.GetAllegianceFromKey(allegianceData.KeyName) != null)
		{
			failReason = "Allegiance already exists.";
			return false;
		}
		if (allegianceData != null)
		{
			Site site = null;
			if (Site != null)
			{
				site = The.Sim.World.AllSites[Site];
			}
			Allegiance allegiance = Allegiance.CreateFromAllegianceData(allegianceData, site);
			if (allegiance == null)
			{
				failReason = "Failed to create allegiance.";
				return false;
			}
			if (ExpeditionData != null)
			{
				return Expedition.CreateFromExpeditionData(ExpeditionData, allegiance, action, null, null, out failReason);
			}
			return true;
		}
		return false;
	}

	public override string ToString()
	{
		if (AllegianceData != null)
		{
			if (AllegianceData.Site != null)
			{
				return "Spawn " + AllegianceData.KeyName + " in " + AllegianceData.Site;
			}
			return "Spawn " + AllegianceData.KeyName;
		}
		return "Spawn " + AllegianceDataKey;
	}
}
