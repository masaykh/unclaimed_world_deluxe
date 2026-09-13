using UWGame.Control.Commands;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Resources;

namespace UWGame.SimSide.Commands;

public class SetStandingOrderGatherInZone : Command
{
	public string ResourceType;

	public ZoneCommand ZoneCommand;

	public bool Enable;

	public bool GiveClientFeedback;

	public long EntityGroupID;

	public Priority? Priority;

	public SetStandingOrderGatherInZone()
	{
	}

	public SetStandingOrderGatherInZone(ZoneID zoneID, bool giveClientFeedback, string resourceType, bool enable, EntityGroupID entityGroupID)
	{
		ZoneCommand = new ZoneCommand(zoneID);
		ResourceType = resourceType;
		GiveClientFeedback = giveClientFeedback;
		Enable = enable;
		EntityGroupID = (long)entityGroupID;
	}

	public SetStandingOrderGatherInZone(MapArea mapArea, bool giveClientFeedback, string resourceType, bool enable, EntityGroupID entityGroupID)
	{
		ZoneCommand = new ZoneCommand(mapArea.GetTileLocations(), mapArea.StartDragTile.Value);
		ResourceType = resourceType;
		GiveClientFeedback = giveClientFeedback;
		Enable = enable;
		EntityGroupID = (long)entityGroupID;
	}

	public override void Execute(bool giveClientFeedback)
	{
		ResourceType item = GameData.Instance.AllResourceTypes[ResourceType];
		EntityGroup entityGroupToUse;
		Zone zone = ZoneCommand.RetrieveOrCreateZone(EntityGroupID, out entityGroupToUse);
		if (Enable)
		{
			zone.AllowStandingOrderHarvest.Add(item);
		}
		else
		{
			zone.AllowStandingOrderHarvest.Remove(item);
		}
		if (GiveClientFeedback && giveClientFeedback)
		{
			The.InGameUI.ContextMenu.OnSetStandingGatherOrder(zone);
		}
	}
}
