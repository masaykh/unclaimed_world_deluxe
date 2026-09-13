using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Combat;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

internal class GoalHunt : CompositeGoal, ITopLevelGoal
{
	private HuntingJob job;

	private JobID snapshotJob;

	private OwnerID? ownerOfCarcass;

	private AttackType attackType;

	private BodyPartID bodyPartToAttackID;

	private EntityAndRoot? weapon;

	public List<ReplenishItemsForAction> ReplenishActions;

	private Regulator setSneakingRegulator;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public double TimeSpentInTopLevelGoal { get; set; }

	public GoalHunt(Entity entity, HuntingJob job, List<EntityGroupID> ownersOfVehicles, OwnerID? ownerOfCarcass, AttackType attackType, BodyPartID bodyPartToAttackID, EntityAndRoot? weapon, List<ReplenishItemsForAction> replenish)
		: base(entity)
	{
		this.job = job;
		base.ownersOfVehicles = ownersOfVehicles;
		this.ownerOfCarcass = ownerOfCarcass;
		this.attackType = attackType;
		this.bodyPartToAttackID = bodyPartToAttackID;
		this.weapon = weapon;
		ReplenishActions = replenish;
	}

	public GoalHunt()
	{
	}

	protected override void Activate()
	{
		base.Status = Status.Active;
		RemoveAllSubgoals();
		job.TakeJob(entity);
		if (AssignWeapon(job, weapon) && SetLocksOnReplenishItems(job, ReplenishActions))
		{
			AddSubgoal(new GoalAttack(entity, job, ownersOfVehicles, ownerOfCarcass, attackType, bodyPartToAttackID, weapon, ReplenishActions, manageLocks: false, this));
		}
	}

	protected override bool ArePreconditionsOK()
	{
		if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(job.Target.Value, out var data)))
		{
			return false;
		}
		if (IsBeyondHuntingLimits(entity, data) && The.Sim.GameplayRandomGenerator.NextDouble("GoalHunt") < 0.2)
		{
			return false;
		}
		return true;
	}

	public override bool RequiresBoldStance()
	{
		return true;
	}

	public static bool IsBeyondHuntingLimits(Entity entity, IKnownEntityData target)
	{
		if (The.Map.IsHuntingZone(entity.Intelligence.Allegiance, entity.MapPosition.Value))
		{
			return false;
		}
		if (target != null)
		{
			return TargetIsTooFarFromExpedition(entity, target.PlaySiteLocation);
		}
		return false;
	}

	public static bool TargetIsTooFarFromExpedition(Entity entity, Vector3 targetLocation, float addToDistance = 0f)
	{
		Vector3 value = entity.Intelligence.CurrentExpedition.Center.Value;
		if (Common.DistanceOctile(targetLocation, value) + addToDistance > GameData.Instance.AIConstants.MaximumDistanceFromExpeditionToChasePrey)
		{
			return true;
		}
		return false;
	}

	public override bool CanDropRequestedItem(Entity item)
	{
		if (job != null && item.AssignedToJob == job.ID)
		{
			return false;
		}
		return true;
	}

	public override string GetStatus()
	{
		return "Hunting";
	}

	public double ScoreGoal()
	{
		AttackParams value = new AttackParams
		{
			AttackType = attackType,
			BodyPartToAttackID = bodyPartToAttackID,
			Weapon = weapon
		};
		return ScoreJobGoal(job, null, value);
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

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		if (!preconditionsRegulator.IsReady() || ArePreconditionsOK())
		{
			if (setSneakingRegulator.IsReady() && !SetSneaking())
			{
				return;
			}
			base.Status = ProcessSubgoals(elapsed);
		}
		else
		{
			base.Status = Status.Failed;
		}
		if (base.Status == Status.Completed)
		{
			if (!AttackTargetAgain())
			{
				MarkSuccesfulZoneHunt();
				base.Status = Status.Completed;
			}
			else
			{
				base.Status = Status.Active;
			}
		}
		else
		{
			_ = base.Status;
			_ = 3;
		}
	}

	private void MarkSuccesfulZoneHunt()
	{
		if (job.HuntZone.HasValue)
		{
			Zone zone = LookUp<Zone, ZoneID>.FindByID(job.HuntZone.Value);
			zone?.ZoneHunt.NotifySuccessfulHunt(job.TargetCreatureType, zone);
		}
	}

	protected override void CreateRegulators()
	{
		base.CreateRegulators();
		setSneakingRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 1.0, "GoalHuntSneaking");
	}

	private bool AttackTargetAgain()
	{
		if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(job.Target.Value, out var data)))
		{
			return false;
		}
		if (data.Body.IsDead())
		{
			return false;
		}
		List<WeaponInstanceCombo> list = new List<WeaponInstanceCombo>();
		EvaluateAttackJobs.GetAllWeaponInstanceCombosWithCarriedWeapons(entity, job, new WeaponInstanceComboJobData
		{
			Job = job
		}, data, list);
		for (int i = 0; i < list.Count; i++)
		{
			WeaponInstanceCombo weaponInstanceCombo = list[i];
			weaponInstanceCombo.AttackData.JobData.AttackDirection = GoalDoAttack.GetAttackDirection(entity, data.PlaySiteLocation, data.Rotation);
			BodyPart bodyPart = data.Body.FindBodyPart(weaponInstanceCombo.AttackData.bodyPartID);
			weaponInstanceCombo.AttackData.EstimatedDamageScore = GameData.Instance.AttackScoresAgainstBodyParts[weaponInstanceCombo.AttackData.AttackType][bodyPart.BodyPartType];
			double num = AttackJob.ScoreAttackDamageAndChanceToHit(entity, weaponInstanceCombo, data, 1f, weaponInstanceCombo.AttackData.EstimatedDamageScore);
			weaponInstanceCombo.Score = (float)num;
			if (weaponInstanceCombo.Weapon != null && weapon.HasValue && weaponInstanceCombo.Weapon.EntityID == weapon.Value.Entity)
			{
				weaponInstanceCombo.Score *= 2f;
			}
		}
		list.RemoveAll((WeaponInstanceCombo c) => (double)c.Score == 0.0);
		if (list.Count > 0)
		{
			list.Sort((WeaponInstanceCombo a, WeaponInstanceCombo b) => b.Score.CompareTo(a.Score));
			double topScore = list[0].Score;
			list.RemoveAll((WeaponInstanceCombo c) => (double)c.Score < 0.6 * topScore);
			Common.BuildEdgesFromBucketSizes(list, doSort: true, out var totalScore);
			if (totalScore > 0f)
			{
				Common.GetStairStepIndex(list, out var stairstep, The.Sim.GameplayRandomGenerator);
				WeaponInstanceCombo selectedCombo = list[stairstep];
				List<ReplenishItemsForAction> list2 = null;
				if (selectedCombo.Weapon != null)
				{
					bool flag = true;
					if (selectedCombo.AttackData.AttackType.RoundsToSpend.HasValue)
					{
						flag = selectedCombo.Weapon.HasEnoughAmmo(selectedCombo.AttackData.AttackType.UsesAmmoType, selectedCombo.AttackData.AttackType.RoundsToSpend.Value);
					}
					if (!flag)
					{
						List<Entity> containedItemsList = entity.Contains.GetContainedItemsList((Entity entity) => entity.EntityType.ItemType != null && entity.EntityType.ItemType.AmmunitionType != null && entity.EntityType == selectedCombo.AttackData.AttackType.UsesAmmoType);
						if (containedItemsList != null && containedItemsList.Count > 0)
						{
							List<EntityID> list3 = new List<EntityID>();
							int num2 = 0;
							foreach (Entity item in containedItemsList)
							{
								num2 += item.Item.Ammunition.NoOfRounds;
								list3.Add(item.EntityID);
								if (num2 >= selectedCombo.AttackData.AttackType.RoundsToSpend.Value)
								{
									break;
								}
							}
							if (num2 < selectedCombo.AttackData.AttackType.RoundsToSpend.Value)
							{
								return false;
							}
							ProcessType action = selectedCombo.Weapon.EntityType.ContainerType.GetReplenishProcesses()[selectedCombo.AttackData.AttackType.UsesAmmoType];
							list2 = new List<ReplenishItemsForAction>();
							list2.Add(new ReplenishItemsForAction(action, list3));
						}
					}
				}
				ReplenishActions = list2;
				AddSubgoal(new GoalAttack(entity, job, ownersOfVehicles, ownerOfCarcass, selectedCombo.AttackData.AttackType, selectedCombo.AttackData.bodyPartID, (selectedCombo.Weapon != null) ? new EntityAndRoot?(selectedCombo.Weapon.GetAsEntityAndRoot()) : ((EntityAndRoot?)null), list2, manageLocks: false, this));
				return true;
			}
		}
		return false;
	}

	private bool SetSneaking()
	{
		if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(job.Target.Value, out var data)))
		{
			return false;
		}
		bool flag = false;
		if (data is Entity entity && entity.CanSeeEntity(base.entity))
		{
			flag = true;
		}
		if (flag)
		{
			if (entityIntelligence.IsStealthy)
			{
				base.entity.SetSneaking(value: false);
			}
		}
		else if (OtherActiveAllegianceMembersAreStandingNearby(base.entity, entityIntelligence, 90f))
		{
			base.entity.SetSneaking(value: false);
		}
		else
		{
			float num = Common.DistanceOctile(base.entity.PlaySiteLocation, data.PlaySiteLocation);
			float range = data.EntityType.SensorType.Range;
			if (num < range + 100f)
			{
				base.entity.SetSneaking(value: true);
			}
			else if (num > range + 190f)
			{
				base.entity.SetSneaking(value: false);
			}
		}
		return true;
	}

	public static bool OtherActiveAllegianceMembersAreStandingNearby(Entity entity, Intelligence entityIntelligence, float radius)
	{
		List<Pair<Entity, Vector2>> resultsList = null;
		The.AgentQuadTree.GetEntitiesInRange(entity.PlaySiteLocation.ToVector2(), 100f, (Entity e) => e != entity && e.Intelligence.Allegiance == entityIntelligence.Allegiance && !e.Intelligence.IsStealthy && e.Intelligence.IsAwakeAndActive, ref resultsList);
		if (resultsList != null && resultsList.Count > 0)
		{
			return true;
		}
		return false;
	}

	public override bool IsSame(Job job)
	{
		return job == this.job;
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

	public override void Deactivate()
	{
		entity.SetSneaking(value: false);
		job.Abandon(entity);
		RemoveLockOnToolOrWeapon(job.ID, weapon);
		RemoveLocksOnReplenishItems(job.ID, ReplenishActions);
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
		snapshotJob = sn.SnapshotID<Job, JobID>(job).Value;
		ownerOfCarcass = sn.DoEnumNullable(ownerOfCarcass);
		attackType = sn.DoGameData(attackType);
		bodyPartToAttackID = sn.DoEnum(bodyPartToAttackID);
		weapon = sn.DoEntityAndRootNullable(weapon);
		ReplenishActions = sn.DoList(ReplenishActions);
		TimeSpentInTopLevelGoal = sn.DoDouble(TimeSpentInTopLevelGoal);
		sn.Ignore(job);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		base.LoadPostProcess(sn);
		job = (HuntingJob)LookUp<Job, JobID>.FindByID(snapshotJob);
	}
}
