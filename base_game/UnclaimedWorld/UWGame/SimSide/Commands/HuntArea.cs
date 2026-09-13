using UWGame.Control.Commands;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;

namespace UWGame.SimSide.Commands;

public class HuntArea : Command
{
	public string EntityType;

	public int Amount;

	public long EntityGroupID;

	public bool GiveClientFeedback;

	public ZoneCommand ZoneCommand;

	public bool RemoveAfterFirstSuccessfulHunt;

	public Priority? Priority;

	private Zone zone;

	public HuntArea()
	{
	}

	public HuntArea(ZoneID zoneID, bool giveClientFeedback, EntityType creatureToHunt, int amount, bool removeAfterSuccessfulHunt, EntityGroupID entityGroupID)
	{
		ZoneCommand = new ZoneCommand(zoneID);
		Amount = amount;
		RemoveAfterFirstSuccessfulHunt = removeAfterSuccessfulHunt;
		EntityType = creatureToHunt.KeyName;
		GiveClientFeedback = giveClientFeedback;
		EntityGroupID = (long)entityGroupID;
	}

	public HuntArea(MapArea mapArea, bool giveClientFeedback, EntityType creatureToHunt, int amount, bool removeAfterSuccessfulHunt, EntityGroupID entityGroupID)
	{
		ZoneCommand = new ZoneCommand(mapArea.GetTileLocations(), mapArea.StartDragTile.Value);
		Amount = amount;
		RemoveAfterFirstSuccessfulHunt = removeAfterSuccessfulHunt;
		EntityType = creatureToHunt.KeyName;
		GiveClientFeedback = giveClientFeedback;
		EntityGroupID = (long)entityGroupID;
	}

	public override void Execute(bool giveClientFeedback)
	{
		zone = ZoneCommand.RetrieveOrCreateZone(EntityGroupID, out var entityGroupToUse);
		bool flag = DoHuntArea(zone, entityGroupToUse);
		if (giveClientFeedback && GiveClientFeedback && flag)
		{
			The.Client.OnHuntArea(zone);
		}
	}

	public Zone GetZone()
	{
		return zone;
	}

	private bool DoHuntArea(Zone zoneToHuntIn, EntityGroup entityGroupToUse)
	{
		EntityType key = GameData.Instance.AllEntityTypes[EntityType];
		zoneToHuntIn.ZoneHunt.CreaturesToHunt[key] = Amount;
		return true;
	}
}
