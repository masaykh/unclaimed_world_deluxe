using UWGame.Control.Commands;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Commands;

public class SetUpgrade : Command
{
	public long EntityGroup;

	public long EntityID;

	public long AllegianceID;

	public string UpgradeCategory;

	public string UpgradeEntityType;

	public bool GiveClientFeedback;

	public Priority? Priority;

	public SetUpgrade()
	{
	}

	public SetUpgrade(EntityID entityID, AllegianceID allegianceID, EntityGroupID entityGroupID, bool giveClientFeedback, UpgradeCategory category, string upgradeTypeKey)
	{
		EntityID = (long)entityID;
		EntityGroup = (long)entityGroupID;
		AllegianceID = (long)allegianceID;
		GiveClientFeedback = giveClientFeedback;
		UpgradeCategory = category.KeyName;
		UpgradeEntityType = upgradeTypeKey;
	}

	public override void Execute(bool giveClientFeedback)
	{
		bool flag = DoSetUpgrade();
		if (giveClientFeedback && GiveClientFeedback && flag)
		{
			The.Client.OnSpecialAction();
		}
	}

	private bool DoSetUpgrade()
	{
		LookUp<Allegiance, UWGame.SimSide.Allegiances.AllegianceID>.FindByID((AllegianceID)AllegianceID);
		EntityGroup entityGroup = LookUp<UWGame.SimSide.Entities.EntityGroup, EntityGroupID>.FindByID((EntityGroupID)EntityGroup);
		EntityType upgradeType = null;
		if (UpgradeEntityType != null)
		{
			upgradeType = GameData.Instance.AllEntityTypes[UpgradeEntityType];
		}
		entityGroup.SetUpgrade((EntityID)EntityID, GameData.Instance.AllUpgradeCategories[UpgradeCategory], upgradeType);
		return true;
	}
}
