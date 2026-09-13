using UWGame.Control.Commands;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;

namespace UWGame.SimSide.Commands;

public class Examine : Command
{
	public long EntityGroupID;

	public bool GiveClientFeedback;

	public ZoneCommand ZoneCommand;

	public Priority? Priority;

	public Examine()
	{
	}

	public Examine(ZoneID zoneID, bool giveClientFeedback, EntityGroupID entityGroupID)
	{
		ZoneCommand = new ZoneCommand(zoneID);
		GiveClientFeedback = giveClientFeedback;
		EntityGroupID = (long)entityGroupID;
	}

	public Examine(MapArea mapArea, bool giveClientFeedback, EntityGroupID entityGroupID)
	{
		ZoneCommand = new ZoneCommand(mapArea);
		GiveClientFeedback = giveClientFeedback;
		EntityGroupID = (long)entityGroupID;
	}

	public override void Execute(bool giveClientFeedback)
	{
		EntityGroup entityGroupToUse;
		Zone zone = ZoneCommand.RetrieveOrCreateZone(EntityGroupID, out entityGroupToUse);
		bool flag = ExamineArea(zone, entityGroupToUse);
		if (giveClientFeedback && GiveClientFeedback && flag)
		{
			The.Client.OnForageArea(zone);
		}
	}

	private bool ExamineArea(Zone zone, EntityGroup entityGroupToUse)
	{
		zone.ExamineJob = new ScoutingJob(zone, entityGroupToUse, examine: true);
		if (Priority.HasValue)
		{
			zone.ExamineJob.Priority = Priority.Value;
		}
		return true;
	}
}
