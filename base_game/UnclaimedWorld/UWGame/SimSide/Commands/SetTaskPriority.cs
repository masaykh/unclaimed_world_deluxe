using UWGame.Control.Commands;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Commands;

public class SetTaskPriority : Command
{
	public long jobID;

	public Priority jobPriority;

	public SetTaskPriority()
	{
	}

	public SetTaskPriority(JobID jobID, Priority jobPriority)
	{
		this.jobID = (long)jobID;
		this.jobPriority = jobPriority;
	}

	public override void Execute(bool giveClientFeedback)
	{
		Job job = LookUp<Job, JobID>.FindByID((JobID)jobID);
		if (job == null)
		{
			return;
		}
		job.Priority = jobPriority;
		PassPriorityToUpgradeRemovals(job, jobPriority);
		if (!(job is ProcessJob { HarvestJob: not null } processJob))
		{
			return;
		}
		Zone zone = processJob.HarvestJob.Zone;
		if (zone == null || !zone.HasHarvestJobs() || !processJob.HarvestJob.Zone.HarvestJobs.TryGetValue(processJob.HarvestJob.ResourceType, out var value))
		{
			return;
		}
		foreach (ProcessJob item in value)
		{
			item.Priority = jobPriority;
		}
	}

	/// <summary>
	/// PORT FIX: a workshop's removal is rated 0 while it still holds upgrades (ProcessJob's "don't
	/// allow salvage before all upgrades are done"), so it waits for the upgrades' own removal jobs -
	/// which were made at Normal priority. Kastuk set the workshop's removal to High and nobody came
	/// until every other task was Low. The priority now goes to those removals too, as it already
	/// goes to a zone's harvest jobs just above.
	/// </summary>
	public static void PassPriorityToUpgradeRemovals(Job job, Priority priority)
	{
		if (!(job is ProcessJob { SalvageJob: not null } salvage) || !salvage.GetImmovableInput(out EntityID? input, out var _) || !input.HasValue)
		{
			return;
		}
		Entity host = Entity.FindByID(input.Value);
		if (host?.ContainedUpgrades == null)
		{
			return;
		}
		foreach (EntityID upgradeID in host.ContainedUpgrades.Values)
		{
			Entity upgrade = Entity.FindByID(upgradeID);
			ProcessJob upgradeRemoval = (upgrade != null) ? Salvage.FindSalvageJob(upgrade) : null;
			if (upgradeRemoval != null)
			{
				upgradeRemoval.Priority = priority;
			}
		}
	}
}
