using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Combat;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems.Triggers;

namespace UWGame.SimSide.AI.Goals;

internal class GoalAttack : CompositeGoal, IIDEventSubscriber, ITopLevelGoal
{
	public JobID jobID;

	public EntityID target;

	private OwnerID? ownerOfCarcass;

	private AttackType attackType;

	private BodyPartID bodyPartToAttackID;

	private EntityAndRoot? weapon;

	private List<ReplenishItemsForAction> replenishActions;

	private MethodID? goalMove_RepathDoneEventMethodID;

	private GoalID? snapshotParentGoal;

	private GoalHunt parentGoal;

	private bool manageLocks;

	private bool isRejoicing;

	private bool chaseProgressWasMade = true;

	private float? currentPathLength;

	private const double timeBetweenChaseProgressEvaluation = 3.0;

	private Regulator chaseProgressRegulator;

	private float? previousDistanceToTarget;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public double TimeSpentInTopLevelGoal { get; set; }

	public GoalAttack(Entity owner, AttackJob job, List<EntityGroupID> ownersOfVehicles, OwnerID? ownerOfCarcass, AttackType attackType, BodyPartID bodyPartToAttackID, EntityAndRoot? weapon, List<ReplenishItemsForAction> replenish, bool manageLocks = true, GoalHunt parentGoal = null)
		: base(owner)
	{
		jobID = job.ID;
		if (job.Target.HasValue)
		{
			target = job.Target.Value;
		}
		if (parentGoal == null)
		{
			this.parentGoal = null;
			replenishActions = replenish;
		}
		else
		{
			this.parentGoal = parentGoal;
			replenishActions = null;
		}
		base.ownersOfVehicles = ownersOfVehicles;
		this.ownerOfCarcass = ownerOfCarcass;
		this.attackType = attackType;
		this.bodyPartToAttackID = bodyPartToAttackID;
		this.weapon = weapon;
		this.manageLocks = manageLocks;
	}

	public GoalAttack()
	{
	}

	public List<ReplenishItemsForAction> GetReplenishActions()
	{
		if (parentGoal != null)
		{
			return parentGoal.ReplenishActions;
		}
		return replenishActions;
	}

	protected override void Activate()
	{
		base.Status = Status.Active;
		AttackJob attackJob = (AttackJob)LookUp<Job, JobID>.FindByID(jobID);
		if (attackJob != null)
		{
			if (manageLocks)
			{
				attackJob.TakeJob(entity);
				if (!AssignWeapon(attackJob, weapon) || !SetLocksOnReplenishItems(attackJob, GetReplenishActions()))
				{
					return;
				}
			}
			entityIntelligence.CombatInfo.Target = attackJob.Target;
			if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(attackJob.Target.Value, out var data)))
			{
				return;
			}
			RemoveAllSubgoals();
			if (ArePreconditionsOK())
			{
				if (!GatherToolsOrWeapons(weapon, ownersOfVehicles, attackJob, StorageCompartment.Haul))
				{
					base.Status = Status.Failed;
					return;
				}
				ReplenishWeapon();
				if (attackType.RangeType != AttackType.RangeTypes.Melee)
				{
					ActivateRangedAttack(data);
				}
				else
				{
					ActivateMeleeAttack(data);
				}
				SetSpeedBasedOnUrgency();
				SetLastAgentOnPost();
			}
			else
			{
				base.Status = Status.Completed;
			}
		}
		else
		{
			base.Status = Status.Failed;
		}
	}

	public override void OnExit()
	{
		if (The.Sim.MeleeAttackers.TryGetValue(target, out var value))
		{
			value.Remove(entity.EntityID);
			if (value.Count == 0)
			{
				The.Sim.MeleeAttackers.Remove(target);
			}
		}
		if (entity.EntityType.IntelligenceType.IsMobile && entity.Locomotor.LeggedLocomotor != null && entity.Locomotor.LeggedLocomotor.TargetSpeed == MovementSpeeds.Run)
		{
			entity.Locomotor.LeggedLocomotor.TargetSpeed = MovementSpeeds.Normal;
		}
		base.OnExit();
	}

	public override void Deactivate()
	{
		if (entity.Name != null)
		{
			entity.Name.Contains("Lehner");
		}
		entityIntelligence.CombatInfo.Target = null;
		AttackJob attackJob = (AttackJob)LookUp<Job, JobID>.FindByID(jobID);
		if (manageLocks)
		{
			attackJob?.Abandon(entity);
			RemoveLockOnToolOrWeapon(jobID, weapon);
			RemoveLocksOnReplenishItems(jobID, GetReplenishActions());
		}
	}

	private void SetLastAgentOnPost()
	{
		CombatAreaJob lastCombatAreaJob = entityIntelligence.Memory.GetLastCombatAreaJob();
		if (lastCombatAreaJob != null)
		{
			entityIntelligence.SetLastAgentOnPost(lastCombatAreaJob);
		}
	}

	private void SetSpeedBasedOnUrgency()
	{
		if (!entity.EntityType.IntelligenceType.IsMobile || entity.Locomotor.LeggedLocomotor == null)
		{
			return;
		}
		AttackJob attackJob = (AttackJob)LookUp<Job, JobID>.FindByID(jobID);
		if (attackJob != null && attackJob is ThreatJob threatJob)
		{
			float urgency = threatJob.GetUrgency(entity);
			if (urgency < 0.5f)
			{
				entity.Locomotor.LeggedLocomotor.TargetSpeed = MovementSpeeds.Normal;
				return;
			}
			if (urgency > 0.5f)
			{
				entity.Locomotor.LeggedLocomotor.TargetSpeed = MovementSpeeds.Run;
				return;
			}
		}
		entity.Locomotor.LeggedLocomotor.TargetSpeed = MovementSpeeds.Normal;
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
		return DetectionFactor.CannotDetect;
	}

	public override bool RequiresBoldStance()
	{
		return true;
	}

	protected void ReplenishWeapon()
	{
		List<ReplenishItemsForAction> list = GetReplenishActions();
		if (list == null)
		{
			return;
		}
		AttackJob attackJob = (AttackJob)LookUp<Job, JobID>.FindByID(jobID);
		if (attackJob == null)
		{
			return;
		}
		foreach (ReplenishItemsForAction item in list)
		{
			AddSubgoal(new GoalReplenish(entity, weapon.Value, item.Items, item.Action, ownersOfVehicles, attackJob, StorageCompartment.Haul));
		}
	}

	protected void ActivateRangedAttack(IKnownEntityData targetData)
	{
		AttackJob attackJob = (AttackJob)LookUp<Job, JobID>.FindByID(jobID);
		if (attackJob != null)
		{
			bool flag;
			Vector3 rangedLocation;
			if (targetData is Entity)
			{
				flag = CombatInfo.GetRangedLocation(entity, targetData, attackType, out rangedLocation);
			}
			else
			{
				flag = false;
				rangedLocation = targetData.PlaySiteLocation;
			}
			if (!flag)
			{
				if (!entity.EntityType.IntelligenceType.IsMobile)
				{
					base.Status = Status.Failed;
					return;
				}
				AddSubgoal(new GoalMoveToPosition(entity, rangedLocation, ownersOfVehicles, GoalMoveToPosition.VehicleUse.FreeUpAfterUse, attackJob.Target, useMeleeLocationAsDestination: false, attackType));
			}
			AddSubgoal(new GoalTurnToFace(entity, null, targetData.EntityID));
			AddSubgoal(new GoalWait(entity, 1.0));
			AddSubgoal(new GoalDoAttack(entity, attackJob, ownerOfCarcass, attackType, bodyPartToAttackID, weapon, this));
			entity.Intelligence.SetBoldStance();
		}
		else
		{
			base.Status = Status.Failed;
		}
	}

	protected void ActivateMeleeAttack(IKnownEntityData targetData)
	{
		AttackJob attackJob = (AttackJob)LookUp<Job, JobID>.FindByID(jobID);
		if (attackJob != null)
		{
			if (CombatInfo.GetMeleeLocation(entity, targetData, out var meleeLocation, out var isCenterLocation))
			{
				if (Common.DistanceOctile(entity.PlaySiteLocation, targetData.PlaySiteLocation) > 48f)
				{
					AddSubgoal(new GoalWait(entity, GameData.Instance.AIConstants.TimeToWaitBeforeStartingGoal));
				}
				GoalMoveToPosition goalMoveToPosition = new GoalMoveToPosition(entity, meleeLocation.Value, ownersOfVehicles, GoalMoveToPosition.VehicleUse.FreeUpAfterUse, attackJob.Target, useMeleeLocationAsDestination: true, attackType)
				{
					IsFinalDestination = true,
					IsChasingTargetCenterLocation = isCenterLocation
				};
				goalMoveToPosition.RegisterPathDoneSubscriber(goalMove_RepathDoneEvent, this, out goalMove_RepathDoneEventMethodID);
				AddSubgoal(goalMoveToPosition);
				AddSubgoal(new GoalTurnToFace(entity, null, targetData.EntityID));
				AddSubgoal(new GoalDoAttack(entity, attackJob, ownerOfCarcass, attackType, bodyPartToAttackID, weapon, this));
				if (!The.Sim.MeleeAttackers.TryGetValue(targetData.EntityID, out var value))
				{
					value = new Dictionary<EntityID, Vector3>();
					The.Sim.MeleeAttackers.Add(targetData.EntityID, value);
				}
				value.Add(entity.EntityID, meleeLocation.Value);
				entity.Intelligence.SetBoldStance();
			}
			else
			{
				base.Status = Status.Failed;
			}
		}
		else
		{
			base.Status = Status.Failed;
		}
	}

	protected override bool ArePreconditionsOK()
	{
		if (isRejoicing)
		{
			return true;
		}
		if (!IsToolOrWeaponOK(weapon, out var weaponData))
		{
			return false;
		}
		AttackJob attackJob = (AttackJob)LookUp<Job, JobID>.FindByID(jobID);
		if (attackJob != null)
		{
			if (weaponData != null && Common.IsZero(attackJob.GetWeaponPolicyScore(entity, weaponData, attackType)))
			{
				return false;
			}
			if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(attackJob.Target.Value, out var data)))
			{
				return false;
			}
			if (data is Entity arg && !chaseProgressWasMade && !GoalDoAttack.IsInRange(entity, data, attackType) && The.Sim.GameplayRandomGenerator.NextDouble("GoalAttack") < 0.35)
			{
				if (entityIntelligence.Allegiance.AllegianceType == AllegianceType.Player)
				{
					The.Client.Log.AddLogEvent(The.Client.Log.CombatEvent, entity, $"has given up chasing {arg}.");
				}
				if (attackType.RangeType == AttackType.RangeTypes.Melee && entityIntelligence.Allegiance.HumanActivities != null)
				{
					entityIntelligence.Allegiance.HumanActivities.HasChasedHuntedCritter[data.EntityType] = true;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		if (!preconditionsRegulator.IsReady() || ArePreconditionsOK())
		{
			base.Status = ProcessSubgoals(elapsed);
		}
		else
		{
			base.Status = Status.Failed;
		}
		if (base.Status == Status.Failed)
		{
			((AttackJob)LookUp<Job, JobID>.FindByID(jobID))?.Abandon(entity);
		}
	}

	public override string GetStatus()
	{
		return "Attacking";
	}

	public double ScoreGoal()
	{
		AttackJob attackJob = (AttackJob)LookUp<Job, JobID>.FindByID(jobID);
		if (attackJob == null)
		{
			return 0.0;
		}
		AttackParams value = new AttackParams
		{
			AttackType = attackType,
			BodyPartToAttackID = bodyPartToAttackID,
			Weapon = weapon
		};
		return ScoreJobGoal(attackJob, null, value);
	}

	private void goalMove_RepathDoneEvent(float newPathLength)
	{
		if (!currentPathLength.HasValue || newPathLength < 140f)
		{
			chaseProgressWasMade = true;
		}
		else if (newPathLength > currentPathLength)
		{
			chaseProgressWasMade = false;
		}
		currentPathLength = newPathLength;
	}

	private bool ChaseProgressWasMade()
	{
		return chaseProgressWasMade;
	}

	public override bool IsSame(Job job)
	{
		AttackJob attackJob = (AttackJob)LookUp<Job, JobID>.FindByID(jobID);
		if (attackJob != null)
		{
			return job == attackJob;
		}
		return false;
	}

	public override bool CanDropRequestedItem(Entity item)
	{
		if (item.AssignedToJob == jobID)
		{
			return false;
		}
		return true;
	}

	protected override void CreateRegulators()
	{
		base.CreateRegulators();
		chaseProgressRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 1.0 / 3.0, "GoalAttackProgress");
	}

	public override bool HandleMessage(Message message)
	{
		if (entity.Name != null)
		{
			entity.Name.Contains("Lehner");
		}
		if (!ForwardMessageToFrontMostSubgoal(message))
		{
			switch (message.MessageType)
			{
			case Message.MessageTypes.CancelJobOrItemInUse:
			case Message.MessageTypes.CancelJobForAIReset:
				base.Status = Status.Failed;
				((AttackJob)LookUp<Job, JobID>.FindByID(jobID))?.Abandon(entity);
				return true;
			case Message.MessageTypes.EntityDied:
				if (personEntity != null && The.Sim.GameplayRandomGenerator.NextDouble("GoalAttack") < (double)GameData.Instance.Constants.ChanceToExultAfterWinning)
				{
					Trigger trigger = (Trigger)message.OtherInfo;
					entityIntelligence.SetTriggerCooldown(trigger, trigger.TriggerType.CooldownInTicks);
					Tuple<EntityID, EntityType> obj = (Tuple<EntityID, EntityType>)trigger.messageInfo;
					EntityID item = obj.Item1;
					_ = obj.Item2;
					AttackJob attackJob = (AttackJob)LookUp<Job, JobID>.FindByID(jobID);
					if (attackJob == null || item != attackJob.Target)
					{
						return false;
					}
					isRejoicing = true;
					Interest interest = trigger.TriggerType.Interest;
					if (interest != null)
					{
						entityIntelligence.SetNewCenterOfAttention(item, null, (float)NormalDistribution.GetRandomValue(The.Sim.GameplayRandomGenerator, interest.InterestLevelMean, interest.InterestLevelStdDeviation));
					}
					AddSubgoal(new GoalWait(entity, 0.6000000238418579 + 0.800000011920929 * The.Sim.GameplayRandomGenerator.NextDouble("GoalAttack"), scaleAnimationToFillWaitPeriod: true));
					AddSubgoal(new GoalWait(entity, 2.0, AnimAction.Idle, AnimModifier.Happy, scaleAnimationToFillWaitPeriod: true));
					AddSubgoal(new GoalWait(entity, 1.0, scaleAnimationToFillWaitPeriod: true));
					base.Status = Status.Active;
				}
				return true;
			default:
				return false;
			}
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
		target = sn.DoEnum(target);
		jobID = sn.DoEnum(jobID);
		ownerOfCarcass = sn.DoEnumNullable(ownerOfCarcass);
		previousDistanceToTarget = sn.DoFloatNullable(previousDistanceToTarget);
		attackType = sn.DoGameData(attackType);
		bodyPartToAttackID = sn.DoEnum(bodyPartToAttackID);
		weapon = sn.DoEntityAndRootNullable(weapon);
		chaseProgressWasMade = sn.DoBool(chaseProgressWasMade);
		currentPathLength = sn.DoFloatNullable(currentPathLength);
		isRejoicing = sn.DoBool(isRejoicing);
		manageLocks = sn.DoBool(manageLocks);
		TimeSpentInTopLevelGoal = sn.DoDouble(TimeSpentInTopLevelGoal);
		goalMove_RepathDoneEventMethodID = sn.DoEnumNullable(goalMove_RepathDoneEventMethodID);
		snapshotParentGoal = sn.SnapshotID<Goal, GoalID>(parentGoal);
		replenishActions = sn.DoList(replenishActions);
		if (parentGoal == null)
		{
			_ = replenishActions;
		}
		sn.Ignore(jobID);
		sn.Ignore(parentGoal);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		base.LoadPostProcess(sn);
		if (snapshotParentGoal.HasValue)
		{
			parentGoal = (GoalHunt)LookUpGoals.FindByID(snapshotParentGoal);
		}
		LoadPostProcessRegisterMethodIDs();
	}

	public void LoadPostProcessRegisterMethodIDs()
	{
		if (goalMove_RepathDoneEventMethodID.HasValue)
		{
			ActionLookup<float>.Add(goalMove_RepathDoneEventMethodID.Value, goalMove_RepathDoneEvent);
		}
	}
}
