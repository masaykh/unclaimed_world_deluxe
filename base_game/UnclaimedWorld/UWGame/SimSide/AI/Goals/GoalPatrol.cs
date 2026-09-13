using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Entities.Locomotors.Stances;
using UWGame.SimSide.Items;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

internal class GoalPatrol : CompositeGoal, ITopLevelGoal
{
	public enum CombatAreaMode
	{
		Patrol,
		Attack
	}

	private Vector3 location;

	private CombatAreaJob job;

	private JobID snapshotJob;

	private EntityAndRoot? weapon;

	private Dictionary<EntityAndRoot, List<ReplenishItemsForAction>> replenishItemsForTools;

	private bool hasSmallAreaToPatrol;

	private bool hasReachedArea;

	private float sampleDistance = 100f;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public double TimeSpentInTopLevelGoal { get; set; }

	public GoalPatrol(Entity owner, CombatAreaJob job, List<EntityGroupID> ownersVehicles, EntityAndRoot? weaponToUse, Dictionary<EntityAndRoot, List<ReplenishItemsForAction>> replenishItemsForTools, Vector3 locationInSearchArea)
		: base(owner)
	{
		ownersOfVehicles = ownersVehicles;
		this.job = job;
		location = locationInSearchArea;
		this.replenishItemsForTools = replenishItemsForTools;
		weapon = weaponToUse;
	}

	public GoalPatrol()
	{
	}

	protected override void Activate()
	{
		sampleDistance = 100f;
		base.Status = Status.Active;
		RemoveAllSubgoals();
		job.TakeJob(entity);
		SetLocksOnReplenishItems(job, replenishItemsForTools);
		if (job.RequiresBoldStance)
		{
			entity.Intelligence.SetBoldStance();
		}
		if (!GatherToolsOrWeapons(weapon, ownersOfVehicles, job, StorageCompartment.Haul))
		{
			base.Status = Status.Failed;
			return;
		}
		List<ItemType.TaskType> gearTasks = null;
		CompositeGoal.AddNightActivityGear(ref gearTasks);
		FindOptionalEquipmentIfNeeded(location, job, equipWeapon: false, equipFood: true, mountWeapon: true, gearTasks);
		ReplenishToolsOrWeapons(replenishItemsForTools, ownersOfVehicles, job);
		AddSubgoal(new GoalMoveToPosition(entity, location, ownersOfVehicles));
		if (job.Zone.MapArea.GetTileLocations().Count <= 4)
		{
			hasSmallAreaToPatrol = true;
		}
	}

	public override bool RequiresBoldStance()
	{
		return true;
	}

	protected void ReplenishWeapon()
	{
	}

	protected override bool ArePreconditionsOK()
	{
		if (!IsToolOrWeaponOK(weapon, out var _))
		{
			return false;
		}
		return true;
	}

	public override string GetStatus()
	{
		if (hasSmallAreaToPatrol)
		{
			return "Guarding area";
		}
		return "Patrolling area";
	}

	public override DetectionFactor GetDetectAgentsFactor(EntityType typeOfAgent, bool requiresExamineAction)
	{
		if (!requiresExamineAction)
		{
			return DetectionFactor.DetectVeryGood;
		}
		return DetectionFactor.CannotDetect;
	}

	public override DetectionFactor GetDetectResourcesFactor(ResourceType resourceType, bool requiresExamineAction)
	{
		if (!requiresExamineAction)
		{
			return DetectionFactor.DetectSome;
		}
		return DetectionFactor.CannotDetect;
	}

	public override bool IsSame(Job job)
	{
		return job == this.job;
	}

	private bool CanHoldSlotWhileVacating()
	{
		if (!hasReachedArea)
		{
			return TimeSpentInTopLevelGoal > 6.0;
		}
		return true;
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		if (!preconditionsRegulator.IsReady() || ArePreconditionsOK())
		{
			base.Status = ProcessSubgoals(elapsed);
			if (CanHoldSlotWhileVacating())
			{
				entityIntelligence.SetLastAgentOnPost(job);
			}
		}
		else
		{
			entityIntelligence.ResetLastAgentOnPost(job);
			base.Status = Status.Failed;
		}
		if (base.Status != Status.Completed)
		{
			return;
		}
		AttackAreaJob attackAreaJob = job as AttackAreaJob;
		if (!hasReachedArea)
		{
			attackAreaJob?.SetAreaReached();
			hasReachedArea = true;
		}
		else if (attackAreaJob != null && attackAreaJob.IsDurationReached() && !attackAreaJob.AreaContainsThreats())
		{
			attackAreaJob.Destroy(removeTakers: true, entity);
			base.Status = Status.Completed;
			return;
		}
		base.Status = Status.Active;
		if (entityIntelligence.Brain.ArbitrateWhileBusy(out var newGoal, useHighFrequency: true))
		{
			if (newGoal is GoalAttack)
			{
				entityIntelligence.SetLastAgentOnPost(job);
			}
			else
			{
				entityIntelligence.ResetLastAgentOnPost(job);
			}
			HandleSubstitutedGoalByArbitrator();
		}
		else if (hasSmallAreaToPatrol)
		{
			CheckToStopAndLookAround();
		}
		else
		{
			sampleDistance *= 1.5f;
			AddSubgoal(new GoalSearchArea(entity, job.Zone, ownersOfVehicles, isStealthy: false, examine: false));
		}
	}

	private void CheckToStopAndLookAround()
	{
		float timeToWaitWhenStoppedAndSearchingMean = GameData.Instance.AIConstants.TimeToWaitWhenStoppedAndSearchingMean;
		float timeToWaitWhenStoppedAndSearchingStdDev = GameData.Instance.AIConstants.TimeToWaitWhenStoppedAndSearchingStdDev;
		double num = The.Sim.GameplayRandomGenerator.RandomNormalDistribution(timeToWaitWhenStoppedAndSearchingMean, timeToWaitWhenStoppedAndSearchingStdDev);
		if (!(num > 0.5))
		{
			return;
		}
		if (entity.HasStance())
		{
			StanceType stanceType = ((!(num > 2.0) || !(The.Sim.GameplayRandomGenerator.NextDouble("GoalPatrol") > 0.4000000059604645)) ? entity.Locomotor.Stance.PickRandomStance(entity.EntityType.LocomotorType.StancesType.PatrolStancesBriefWait, entity.EntityType.LocomotorType.StancesType.DefaultStanceType) : entity.Locomotor.Stance.PickRandomStance(entity.EntityType.LocomotorType.StancesType.PatrolStancesLongerWait, entity.EntityType.LocomotorType.StancesType.DefaultStanceType));
			ChangeStance(stanceType);
			if (stanceType.AnimModifier.HasValue)
			{
				AddSubgoal(new GoalWait(entity, num, AnimAction.Scouting, stanceType.AnimModifier.Value));
			}
			else
			{
				AddSubgoal(new GoalWait(entity, num, AnimAction.Scouting));
			}
		}
		else
		{
			AddSubgoal(new GoalWait(entity, num, AnimAction.Scouting));
		}
	}

	public override bool HandleMessage(Message message)
	{
		if (!ForwardMessageToFrontMostSubgoal(message))
		{
			Message.MessageTypes messageType = message.MessageType;
			if ((uint)(messageType - 5) <= 1u)
			{
				entity.ToString().Contains("August");
				base.Status = Status.Failed;
				entityIntelligence.Memory.SetRecentlyOnPatrol(null);
				job.Abandon(entity);
				return true;
			}
			return false;
		}
		return true;
	}

	public override bool CanDropRequestedItem(Entity item)
	{
		if (item.AssignedToJob == job.ID)
		{
			return false;
		}
		return true;
	}

	public override void Deactivate()
	{
		job.Abandon(entity);
		if (weapon.HasValue)
		{
			RemoveLockOnToolOrWeapon(job.ID, weapon.Value);
		}
		RemoveLocksOnReplenishItems(job.ID, replenishItemsForTools);
		ResetThreatStance(job);
	}

	public double ScoreGoal()
	{
		ToolParams? toolParams = null;
		if (weapon.HasValue)
		{
			toolParams = new ToolParams
			{
				Tools = new List<EntityID> { weapon.Value.Entity }
			};
		}
		return ScoreJobGoal(job, toolParams);
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
		location = sn.DoVector3(location);
		if (sn.mode == Snapshotter.Mode.Save && job.ID == JobID.Invalid)
		{
			throw new Exception("Invalid jobID");
		}
		snapshotJob = sn.SnapshotID<Job, JobID>(job).Value;
		weapon = sn.DoEntityAndRootNullable(weapon);
		replenishItemsForTools = sn.DoMultiMap(replenishItemsForTools);
		TimeSpentInTopLevelGoal = sn.DoDouble(TimeSpentInTopLevelGoal);
		hasSmallAreaToPatrol = sn.DoBool(hasSmallAreaToPatrol);
		hasReachedArea = sn.DoBool(hasReachedArea);
		sn.Ignore(job);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		base.LoadPostProcess(sn);
		job = (CombatAreaJob)LookUp<Job, JobID>.FindByID(snapshotJob);
	}
}
