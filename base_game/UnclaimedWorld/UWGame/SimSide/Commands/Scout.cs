using UWGame.Control.Commands;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;

namespace UWGame.SimSide.Commands;

public class Scout : Command
{
	public long EntityGroupID;

	public bool GiveClientFeedback;

	public Priority? Priority;

	public ZoneCommand ZoneCommand;

	public Scout()
	{
	}

	public Scout(ZoneID zoneID, bool giveClientFeedback, EntityGroupID entityGroupID)
	{
		ZoneCommand = new ZoneCommand(zoneID);
		GiveClientFeedback = giveClientFeedback;
		EntityGroupID = (long)entityGroupID;
	}

	public Scout(MapArea mapArea, bool giveClientFeedback, EntityGroupID entityGroupID)
	{
		ZoneCommand = new ZoneCommand(mapArea.GetTileLocations(), mapArea.StartDragTile.Value);
		GiveClientFeedback = giveClientFeedback;
		EntityGroupID = (long)entityGroupID;
	}

	public override void Execute(bool giveClientFeedback)
	{
		EntityGroup entityGroupToUse;
		Zone zone = ZoneCommand.RetrieveOrCreateZone(EntityGroupID, out entityGroupToUse);
		bool flag = ScoutArea(zone, entityGroupToUse);
		if (giveClientFeedback && GiveClientFeedback && flag)
		{
			The.Client.OnScoutArea(zone);
		}
	}

	private bool ScoutArea(Zone zoneToScout, EntityGroup entityGroupToUse)
	{
		zoneToScout.ScoutingJob = new ScoutingJob(zoneToScout, entityGroupToUse, examine: false);
		if (Priority.HasValue)
		{
			zoneToScout.ScoutingJob.Priority = Priority.Value;
		}
		return true;
	}
}
