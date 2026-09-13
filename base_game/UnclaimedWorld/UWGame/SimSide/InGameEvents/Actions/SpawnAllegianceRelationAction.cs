using UWGame.SimSide.Allegiances;

namespace UWGame.SimSide.InGameEvents.Actions;

public class SpawnAllegianceRelationAction : EventActionType
{
	public AllegianceRelationData AllegianceRelationData;

	public SpawnAllegianceRelationAction(string keyName)
		: base(keyName)
	{
	}

	public SpawnAllegianceRelationAction()
	{
	}

	public override bool Execute(EventAction action, ref string failReason)
	{
		if (AllegianceRelationData != null)
		{
			if (AllegianceRelation.CreateRelationFromData(AllegianceRelationData) == null)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public override string ToString()
	{
		return "Spawn relation between " + AllegianceRelationData.Allegiance1 + " and " + AllegianceRelationData.Allegiance2;
	}
}
