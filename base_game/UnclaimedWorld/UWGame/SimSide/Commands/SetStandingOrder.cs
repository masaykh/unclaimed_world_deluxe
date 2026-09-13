using UWGame.Control.Commands;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Commands;

public class SetStandingOrder : Command
{
	public long ExpeditionID;

	public string EntityTypeKey;

	public int NewCount;

	public bool GiveClientFeedback;

	public SetStandingOrder()
	{
	}

	public SetStandingOrder(ExpeditionID expeditionID, string entityTypeKey, int newCount, bool giveClientFeedback)
	{
		EntityTypeKey = entityTypeKey;
		NewCount = newCount;
		ExpeditionID = (long)expeditionID;
		GiveClientFeedback = giveClientFeedback;
	}

	public override void Execute(bool giveClientFeedback)
	{
		bool flag = DoSetStandingOrder();
		if (giveClientFeedback && GiveClientFeedback && flag)
		{
			The.Client.SetProduction(EntityTypeKey);
		}
	}

	private bool DoSetStandingOrder()
	{
		Expedition expedition = LookUp<Expedition, UWGame.SimSide.Expeditions.ExpeditionID>.FindByID((ExpeditionID)ExpeditionID);
		EntityType entityType = GameData.Instance.AllEntityTypes[EntityTypeKey];
		expedition.OwnedEntities.ProductionOrders.SetStandingOrder(entityType, NewCount);
		return true;
	}
}
