using System.Xml.Serialization;
using UWGame.Control.Commands;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Commands;

/// <summary>
/// Orders one batch of a process for a household: the job and the hauling of its inputs are made
/// in the household's own entity group, so what is made belongs to the household.
///
/// The household counterpart of SetProduction, which addresses an expedition only - its orders and
/// its JobManager. A command because "all planners use Commands - the Commands create Jobs" (the
/// studio, PhysicalNeedsPlanner); tripleacoder asked that OwnershipMod's cooking planner follow it,
/// so the planner and a player go through the same code path. The planner executes it directly
/// with no client feedback, as GoapFindPreyAction does HuntArea.
/// </summary>
public class SetHouseholdProduction : Command
{
	public ulong HouseholdID;

	public string ProcessTypeKey;

	public string EntityTypeKey;

	public SetHouseholdProduction()
	{
	}

	public SetHouseholdProduction(UWGame.SimSide.Entities.HouseholdID householdID, string processTypeKey, string entityTypeKey)
	{
		HouseholdID = (ulong)householdID;
		ProcessTypeKey = processTypeKey;
		EntityTypeKey = entityTypeKey;
	}

	/// <summary>The job made by the last Execute, or null (for the planner's log and tests).</summary>
	[XmlIgnore]
	public ProcessJob CreatedJob { get; private set; }

	public override void Execute(bool giveClientFeedback)
	{
		CreatedJob = null;
		Household household = LookUp<Household, UWGame.SimSide.Entities.HouseholdID>.FindByID((UWGame.SimSide.Entities.HouseholdID)HouseholdID);
		if (household == null
			|| !GameData.Instance.AllProcessTypes.TryGetValue(ProcessTypeKey, out ProcessType process)
			|| !GameData.Instance.AllEntityTypes.TryGetValue(EntityTypeKey, out EntityType output))
		{
			return;
		}
		EntityGroup group = household.OwnedEntities;
		JobManager.FindProductionLocation(process, household, out var location);
		ProcessJob job = JobManager.CreateProcessJob(output, group, process, null, location);
		if (job != null && location.HasValue)
		{
			job.CreateHaulingJobsForProcessInputs(group, location.Value);
		}
		CreatedJob = job;
	}
}
