using System.Collections.Generic;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Commands;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using UWGame.SimSide.Maps.MapEditor;

namespace UWGame.SimSide.InGameEvents.Actions;

public class SpawnUpgradeAction : EventActionType
{
	public TargetObject UpgradeTargetObject;

	public string UpgradeTargetEntityName;

	public AllegianceAndExpedition OwnedBy;

	public string UpgradeCategoryKey;

	public string EntityTypeKey;

	public override bool Execute(EventAction action, ref string failReason)
	{
		Entity entity = null;
		if (!EventActionType.GetEntity(UpgradeTargetEntityName, UpgradeTargetObject, action, out entity, ref failReason))
		{
			return false;
		}
		Allegiance allegiance;
		Expedition expedition;
		if (OwnedBy != null)
		{
			if (!OwnedBy.Resolve(action, out allegiance, out expedition, ref failReason))
			{
				failReason = "Failed to resolve owner";
				return false;
			}
		}
		else
		{
			expedition = entity.GetOwner();
			if (expedition == null)
			{
				failReason = "Upgrade target is not owned by an expedition";
				return false;
			}
			allegiance = expedition.Allegiance;
		}
		new SetUpgrade(entity.ID, allegiance.ID, expedition.OwnedEntities.ID, giveClientFeedback: false, GameData.Instance.AllUpgradeCategories[UpgradeCategoryKey], EntityTypeKey).Execute(giveClientFeedback: false);
		return true;
	}

	public override void PostDataCompleteValidate(ref List<string> listOfErrors)
	{
		EntityType.ValidateGameDataTypeExists(ref listOfErrors, UpgradeCategoryKey, GameData.Instance.AllUpgradeCategories, out var _);
		EntityType.ValidateGameDataTypeExists(ref listOfErrors, EntityTypeKey, GameData.Instance.AllEntityTypes, out var _);
	}

	public override string ToString()
	{
		return "Set upgrade " + UpgradeCategoryKey + " to " + EntityTypeKey;
	}
}
