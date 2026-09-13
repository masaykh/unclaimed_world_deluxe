using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Commands;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Planners;

internal class GoapFindPreyAction : GoapAction
{
	private FindPreyJob currentJob;

	private JobID? snapshotJob;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public GoapFindPreyAction()
	{
	}

	public GoapFindPreyAction(Allegiance allegiance, Expedition expedition)
		: base(allegiance, expedition)
	{
	}

	public override bool Init()
	{
		MapArea mapArea = ScoreAndGetHuntingJobLocation();
		if (mapArea == null)
		{
			return false;
		}
		Zone zone = null;
		foreach (EntityType preyType in allegiance.RepresentativeEntityType.IntelligenceType.PreyTypes)
		{
			HuntArea huntArea = ((zone != null) ? new HuntArea(zone.ID, giveClientFeedback: false, preyType, 1, removeAfterSuccessfulHunt: true, allegiance.SharedKnowledge.AllKnownEntities.ID) : new HuntArea(mapArea, giveClientFeedback: false, preyType, 1, removeAfterSuccessfulHunt: true, allegiance.SharedKnowledge.AllKnownEntities.ID));
			huntArea.Execute(giveClientFeedback: false);
			zone = huntArea.GetZone();
		}
		return true;
	}

	public override bool MonitorAction()
	{
		if (currentJob == null)
		{
			return false;
		}
		_ = currentJob.TakenBy.Count;
		_ = 0;
		return true;
	}

	public override void Destroy()
	{
		if (currentJob != null)
		{
			for (int i = 0; i < currentJob.TakenBy.Count; i++)
			{
			}
		}
	}

	public MapArea ScoreAndGetHuntingJobLocation()
	{
		return GoapAction.GetRandomMapArea(expedition, allegiance);
	}

	public override Snapshotter.Version DoVersion(Snapshotter sn)
	{
		base.DoVersion(sn);
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		snapshotJob = sn.SnapshotID<Job, JobID>(currentJob);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		base.LoadPostProcess(sn);
		if (snapshotJob.HasValue)
		{
			currentJob = (FindPreyJob)LookUp<Job, JobID>.FindByID(snapshotJob);
			snapshotJob = null;
		}
	}
}
