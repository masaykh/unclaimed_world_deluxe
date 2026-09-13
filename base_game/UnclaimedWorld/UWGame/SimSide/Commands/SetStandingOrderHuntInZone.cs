using UWGame.Control.Commands;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;

namespace UWGame.SimSide.Commands;

public class SetStandingOrderHuntInZone : Command
{
	public string EntityType;

	public ZoneCommand ZoneCommand;

	public bool Enable;

	public bool GiveClientFeedback;

	public long EntityGroupID;

	public Priority? Priority;

	public SetStandingOrderHuntInZone()
	{
	}

	public SetStandingOrderHuntInZone(ZoneID zoneID, bool giveClientFeedback, string entityType, bool enable, EntityGroupID entityGroupID)
	{
		ZoneCommand = new ZoneCommand(zoneID);
		EntityType = entityType;
		GiveClientFeedback = giveClientFeedback;
		Enable = enable;
		EntityGroupID = (long)entityGroupID;
	}

	public SetStandingOrderHuntInZone(MapArea mapArea, bool giveClientFeedback, string entityType, bool enable, EntityGroupID entityGroupID)
	{
		ZoneCommand = new ZoneCommand(mapArea.GetTileLocations(), mapArea.StartDragTile.Value);
		EntityType = entityType;
		GiveClientFeedback = giveClientFeedback;
		Enable = enable;
		EntityGroupID = (long)entityGroupID;
	}

	public override void Execute(bool giveClientFeedback)
	{
		EntityType item = GameData.Instance.AllEntityTypes[EntityType];
		EntityGroup entityGroupToUse;
		Zone zone = ZoneCommand.RetrieveOrCreateZone(EntityGroupID, out entityGroupToUse);
		if (Enable)
		{
			zone.ZoneHunt.AllowStandingOrderHunt.Add(item);
		}
		else
		{
			zone.ZoneHunt.AllowStandingOrderHunt.Remove(item);
		}
		if (GiveClientFeedback && giveClientFeedback)
		{
			The.InGameUI.ContextMenu.OnSetStandingGatherOrder(zone);
		}
	}
}
