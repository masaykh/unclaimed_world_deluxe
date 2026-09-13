using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Commands;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Planners;

internal class GoapScoutAction : GoapAction
{
	private ScoutingJob currentJob;

	private JobID? snapshotJob;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public GoapScoutAction()
	{
	}

	public GoapScoutAction(Allegiance allegiance, Expedition expedition)
		: base(allegiance, expedition)
	{
	}

	public override bool Init()
	{
		MapArea mapArea = ScoreAndGetScoutingJobLocation();
		if (mapArea == null)
		{
			return false;
		}
		new Scout(mapArea, giveClientFeedback: false, allegiance.SharedKnowledge.AllKnownEntities.ID).Execute(giveClientFeedback: false);
		int count = allegiance.SharedKnowledge.AllKnownEntities.ScoutingJobs.Count;
		currentJob = allegiance.SharedKnowledge.AllKnownEntities.ScoutingJobs[count - 1] as ScoutingJob;
		return true;
	}

	public override bool MonitorAction()
	{
		if (currentJob == null)
		{
			return false;
		}
		if (currentJob.TakenBy.Count > 0)
		{
			return true;
		}
		if (!allegiance.SharedKnowledge.AllKnownEntities.ScoutingJobs.Contains(currentJob))
		{
			return false;
		}
		return true;
	}

	public override void Destroy()
	{
		if (currentJob != null)
		{
			currentJob.Destroy(removeTakers: true);
		}
	}

	public MapArea ScoreAndGetScoutingJobLocation()
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
		sn.Ignore(currentJob);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		base.LoadPostProcess(sn);
		if (snapshotJob.HasValue)
		{
			currentJob = (ScoutingJob)LookUp<Job, JobID>.FindByID(snapshotJob);
			snapshotJob = null;
		}
	}
}
