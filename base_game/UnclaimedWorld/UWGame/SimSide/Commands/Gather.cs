using System;
using System.Collections.Generic;
using UWGame.Control.Commands;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Resources;

namespace UWGame.SimSide.Commands;

public class Gather : Command
{
	public int JobDifference;

	public string ResourceItemType;

	public string ProcessType;

	public string ResourceType;

	public ZoneCommand ZoneCommand;

	public bool GiveClientFeedback;

	public long EntityGroupID;

	public Priority? Priority;

	public Gather()
	{
	}

	public Gather(ZoneID zoneID, bool giveClientFeedback, int jobDifference, string resourceItemType, string processType, string resourceType, EntityGroupID entityGroupID)
	{
		ZoneCommand = new ZoneCommand(zoneID);
		JobDifference = jobDifference;
		ResourceItemType = resourceItemType;
		ProcessType = processType;
		ResourceType = resourceType;
		GiveClientFeedback = giveClientFeedback;
		EntityGroupID = (long)entityGroupID;
	}

	public Gather(MapArea mapArea, bool giveClientFeedback, int jobDifference, string resourceItemType, string processType, string resourceType, EntityGroupID entityGroupID)
	{
		ZoneCommand = new ZoneCommand(mapArea.GetTileLocations(), mapArea.StartDragTile.Value);
		JobDifference = jobDifference;
		ResourceItemType = resourceItemType;
		ProcessType = processType;
		ResourceType = resourceType;
		GiveClientFeedback = giveClientFeedback;
		EntityGroupID = (long)entityGroupID;
	}

	public override void Execute(bool giveClientFeedback)
	{
		EntityType outputType = GameData.Instance.AllEntityTypes[ResourceItemType];
		ProcessType processType = GameData.Instance.AllProcessTypes[ProcessType];
		ResourceType resourceType = GameData.Instance.AllResourceTypes[ResourceType];
		EntityGroup entityGroupToUse;
		Zone zone = ZoneCommand.RetrieveOrCreateZone(EntityGroupID, out entityGroupToUse);
		Dictionary<ResourceType, List<ProcessJob>> harvestJobs = zone.HarvestJobs;
		if (JobDifference > 0)
		{
			JobManager.AddHarvestJobs(JobDifference, entityGroupToUse, outputType, processType, resourceType, zone, Priority);
		}
		else if (JobDifference < 0)
		{
			int noOfJobsToRemove = Math.Abs(JobDifference);
			harvestJobs.TryGetValue(resourceType, out var value);
			JobManager.DestroyJobsIntelligently(value, noOfJobsToRemove);
		}
		if (GiveClientFeedback && giveClientFeedback)
		{
			The.InGameUI.ContextMenu.OnGather(zone);
		}
	}
}
