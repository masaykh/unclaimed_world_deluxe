using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

internal class GoalConstruct : CompositeGoal, ITopLevelGoal
{
	public ProcessJob job;

	private JobID? snapshotJob;

	public List<EntityID> Tools = new List<EntityID>();

	public ToolTypeCombination ToolTypeCombination;

	private ToolTypeCombinationID? snapshotToolCombo;

	public Dictionary<EntityAndRoot, List<ReplenishItemsForAction>> replenishItemsForTools;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public double TimeSpentInTopLevelGoal { get; set; }

	public GoalConstruct(Entity entity, ProcessJob job, List<EntityGroupID> ownersOfVehicles, List<EntityID> tools, Dictionary<EntityAndRoot, List<ReplenishItemsForAction>> replenishItemsForTools, ToolTypeCombination toolTypeCombination)
		: base(entity)
	{
		this.job = job;
		base.ownersOfVehicles = ownersOfVehicles;
		Tools = tools;
		ToolTypeCombination = toolTypeCombination;
		this.replenishItemsForTools = replenishItemsForTools;
	}

	public GoalConstruct()
	{
	}

	protected override void Activate()
	{
		base.Status = Status.Active;
		RemoveAllSubgoals();
		AddSubgoal(new GoalWait(entity, GameData.Instance.AIConstants.TimeToWaitBeforeStartingGoal));
		job.TakeJob(entity);
		SetLocksOnReplenishItems(job, replenishItemsForTools);
		IKnownEntityData structureToConstruct = null;
		if (job.BuildingJob.GetWorkLocation(entity, out var workLocation, out structureToConstruct))
		{
			AddSubgoal(new GoalWait(entity, GameData.Instance.AIConstants.TimeToWaitBeforeStartingGoal));
			List<IKnownEntityData> toolsData = null;
			if (ResolveTools(Tools, ref toolsData) && GatherToolsOrWeapons(toolsData, ownersOfVehicles, job, mountAfterPickup: false, StorageCompartment.Haul))
			{
				AddSubgoal(new GoalMoveToPosition(entity, workLocation, ownersOfVehicles)
				{
					IsFinalDestination = false
				});
				PlaceStationaryToolsAtWorkSite(toolsData, workLocation);
				ReplenishToolsOrWeapons(replenishItemsForTools, ownersOfVehicles, job);
				AddSubgoal(new GoalMoveToPosition(entity, workLocation, ownersOfVehicles)
				{
					IsFinalDestination = true
				});
				AddSubgoal(new GoalTurnToFace(entity, structureToConstruct.PlaySiteLocation.ToVector2()));
				if (entity.HasStance())
				{
					ChangeStance(entity.Locomotor.Stance.PickRandomProcessStance(job.ProcessType.StanceTypes));
				}
				AddSubgoal(new GoalDoProduce(entity, job, structureToConstruct.OwnedBy, Tools, ToolTypeCombination));
			}
		}
		else
		{
			base.Status = Status.Failed;
		}
	}

	protected override bool ArePreconditionsOK()
	{
		if (!AreToolsOK(Tools))
		{
			return false;
		}
		if (!IsOutputOKAndNotCompleted(job, mustExist: true))
		{
			return false;
		}
		return true;
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		if (!preconditionsRegulator.IsReady() || ArePreconditionsOK())
		{
			base.Status = ProcessSubgoals(elapsed);
		}
		if (base.Status != Status.Completed && base.Status == Status.Failed)
		{
			job.Abandon(entity);
		}
	}

	public override string GetStatus()
	{
		return "Constructing";
	}

	public double ScoreGoal()
	{
		return ScoreJobGoal(job, new ToolParams
		{
			Tools = Tools,
			ToolProductivity = GoalProduce.GetToolCombinationProductivity(ToolTypeCombination),
			ReplenishStatus = null,
			JobDurationInDays = null
		});
	}

	public override bool IsSame(Job job)
	{
		return job == this.job;
	}

	public override void Deactivate()
	{
		if (job != null)
		{
			RemoveProcessToolLocks(job, Tools, replenishItemsForTools);
		}
	}

	public override bool HandleMessage(Message message)
	{
		if (!ForwardMessageToFrontMostSubgoal(message))
		{
			Message.MessageTypes messageType = message.MessageType;
			if ((uint)(messageType - 5) <= 1u)
			{
				base.Status = Status.Failed;
				job.Abandon(entity);
				return true;
			}
			return false;
		}
		return true;
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
		Tools = sn.DoList(Tools);
		replenishItemsForTools = sn.DoMultiMap(replenishItemsForTools);
		snapshotJob = sn.SnapshotID<Job, JobID>(job);
		snapshotToolCombo = sn.SnapshotID<ToolTypeCombination, ToolTypeCombinationID>(ToolTypeCombination);
		TimeSpentInTopLevelGoal = sn.DoDouble(TimeSpentInTopLevelGoal);
		sn.Ignore(job);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		base.LoadPostProcess(sn);
		ToolTypeCombination = LookUp<ToolTypeCombination, ToolTypeCombinationID>.FindByID(snapshotToolCombo);
		if (snapshotJob.HasValue)
		{
			job = (ProcessJob)LookUp<Job, JobID>.FindByID(snapshotJob);
		}
	}
}
