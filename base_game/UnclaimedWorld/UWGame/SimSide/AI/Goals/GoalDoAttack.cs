using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Log;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Combat;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems.Triggers;

namespace UWGame.SimSide.AI.Goals;

internal class GoalDoAttack : CompositeGoal
{
	public AttackJob job;

	private JobID? snapshotJob;

	private OwnerID? ownerOfCarcass;

	private GoalID snapshotParentGoal;

	private GoalAttack parentGoal;

	private AttackType attackType;

	private BodyPartID bodyPartToAttackID;

	private EntityAndRoot? weapon;

	private bool willHitTarget;

	private bool hasCompletedFirstPhase;

	private bool hasCompletedRestPhase;

	private bool targetDied;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public GoalDoAttack(Entity entity, AttackJob job, OwnerID? ownerOfCarcass, AttackType attackType, BodyPartID bodyPartToAttackID, EntityAndRoot? weapon, GoalAttack parentGoal)
		: base(entity)
	{
		this.job = job;
		this.parentGoal = parentGoal;
		this.ownerOfCarcass = ownerOfCarcass;
		this.attackType = attackType;
		this.bodyPartToAttackID = bodyPartToAttackID;
		this.weapon = weapon;
		_ = weapon.HasValue;
	}

	public GoalDoAttack()
	{
	}

	protected override void Activate()
	{
		if (weapon.HasValue && EntityIsNotSeenDirectly(weapon.Value.Entity, out var _))
		{
			return;
		}
		if (ArePreconditionsOK(out var targetEntity, out var bodyPart))
		{
			if (AreWeStandingInAStack(targetEntity))
			{
				base.Status = Status.Failed;
				return;
			}
			base.Status = Status.Active;
			BodyPart.AttackDirection attackDirection = GetAttackDirection(base.entity, targetEntity.PlaySiteLocation, targetEntity.Rotation);
			entityIntelligence.Memory.SetLastAttackTarget(targetEntity.EntityID);
			if (base.entity.AgentStorage != null)
			{
				base.entity.AgentStorage.MountedToolOrWeapon = EntityAndRoot.GetEntity(weapon);
			}
			BodyPart hitBodyPart = null;
			if (RollToHit(base.entity, targetEntity, attackType, bodyPart, attackDirection, out hitBodyPart))
			{
				willHitTarget = true;
				SetFirstPartAnimationState();
			}
			else
			{
				willHitTarget = false;
				float num = attackType.MissDurationInSeconds ?? GetAttackTypeDurationOrDefault();
				AddSubgoal(new GoalWait(base.entity, num, AnimAction.Attacking, AnimModifier.Fail, scaleAnimationToFillWaitPeriod: true));
			}
			attackType.StartStartEffects(targetEntity, base.entity);
		}
		else
		{
			base.Status = Status.Failed;
		}
	}

	private float GetMissDuration()
	{
		return 0.72f;
	}

	private void SetFirstPartAnimationState()
	{
		float attackTypeActionPointOrDefault = GetAttackTypeActionPointOrDefault();
		AddSubgoal(new GoalWait(entity, attackTypeActionPointOrDefault, AnimAction.Attacking, attackType.AnimationStatesList, scaleAnimationToFillWaitPeriod: false, GoalWait.OnExitFlagAction.Leave));
	}

	private float GetAttackTypeActionPointOrDefault()
	{
		if (attackType.ActionPointInSeconds > 0f)
		{
			return attackType.ActionPointInSeconds;
		}
		return 0.5f;
	}

	private float GetAttackTypeDurationOrDefault()
	{
		if (attackType.DurationInSeconds > 0f)
		{
			return attackType.DurationInSeconds;
		}
		return 1f;
	}

	private void WakeUpNearbyAllies()
	{
		if (!(job is ThreatJob { IsVermin: false }))
		{
			return;
		}
		List<Pair<Entity, Vector2>> resultsList = new List<Pair<Entity, Vector2>>();
		The.AgentQuadTree.GetEntitiesInRange(entity.PlaySiteLocation.ToVector2(), GameData.Instance.AIConstants.Combat.CombatWakeUpRange, (Entity e) => e != entity && e.Intelligence.Allegiance == entityIntelligence.Allegiance && e.Intelligence.IsSleeping(), ref resultsList);
		foreach (Pair<Entity, Vector2> item in resultsList)
		{
			item.First.SendMessage(new Message(Message.MessageTypes.WakeUpCombatAlert));
		}
	}

	private void SetDetectedByAllWatchers()
	{
		TerrainTile tile = The.Map.GetTile(entity.MapPosition.Value);
		if (tile.EntitiesThatSeeThisTile == null)
		{
			return;
		}
		foreach (Entity item in tile.EntitiesThatSeeThisTile)
		{
			if (item != entity)
			{
				try
				{
					item.SendMessage(new Message
					{
						MessageType = Message.MessageTypes.AlertToPresence,
						Sender = entity
					});
				}
				catch (Exception ex)
				{
					throw new Exception(ex.Message + Entity.GetExceptionInformation(item));
				}
			}
		}
	}

	public override void Deactivate()
	{
		ClearAttackAnimFlags();
	}

	private void ClearAttackAnimFlags()
	{
		AnimModifier[] array = null;
		if (base.entity.AgentStorage != null)
		{
			EntityID? mountedToolOrWeapon = base.entity.AgentStorage.MountedToolOrWeapon;
			if (mountedToolOrWeapon.HasValue)
			{
				Entity entity = Entity.FindByID(mountedToolOrWeapon.Value);
				if (entity != null)
				{
					array = entity.EntityType.ItemType.AnimStatesWhenAttached;
				}
			}
		}
		base.entity.Renderable.ClearAnimationActionStateFlag(AnimAction.Attacking);
		AnimModifier[] animationStates = attackType.AnimationStates;
		foreach (AnimModifier state in animationStates)
		{
			if (array == null || !Array.Exists(array, (AnimModifier f) => f == state))
			{
				base.entity.Renderable.ClearAnimationStateFlag(state);
			}
		}
	}

	private bool AreWeStandingInAStack(Entity targetEntity)
	{
		if (!targetEntity.Locomotor.IsMoving())
		{
			if (targetEntity.Intelligence.CombatInfo.Target == entity.EntityID)
			{
				if (MapManager.WorldPosToSubtile(entity.PlaySiteLocation) == MapManager.WorldPosToSubtile(targetEntity.PlaySiteLocation))
				{
					return true;
				}
				return false;
			}
			return MapManager.IsStandingOnNonMovingEntity(entity);
		}
		return false;
	}

	protected bool ArePreconditionsOK(out Entity targetEntity, out BodyPart bodyPart)
	{
		bodyPart = null;
		if (EntityIsNotSeenDirectly(job.Target.Value, out targetEntity))
		{
			return false;
		}
		bool result = IsInRange(entity, targetEntity, attackType);
		bodyPart = targetEntity.Body.FindBodyPart(bodyPartToAttackID);
		return result;
	}

	public static bool IsInRange(Entity entity, IKnownEntityData targetData, AttackType attackType)
	{
		if (attackType.RangeType == AttackType.RangeTypes.Melee)
		{
			if (!(targetData is Entity target))
			{
				return false;
			}
			return IsInStrikingPosition(entity, target);
		}
		if ((double)(entity.PlaySiteLocation - targetData.PlaySiteLocation).Length() < (double?)(attackType.MaxRange + 15f))
		{
			return true;
		}
		return false;
	}

	public static bool IsInStrikingPosition(Entity attacker, Entity target)
	{
		float rotationOfClosestCorner;
		int cornerNo;
		if (AttackJob.IsCorrectMeleeDistanceRoundedToSubtiles(attacker, target))
		{
			return GoalTurnToFace.IsFacing(attacker, target.PlaySiteLocation.ToVector2(), out rotationOfClosestCorner, out cornerNo, 0.3f);
		}
		return false;
	}

	private void DamageWeaponCondition(bool hasHit)
	{
		if (!weapon.HasValue || !attackType.ConditionDamageMean.HasValue)
		{
			return;
		}
		Entity entity = Entity.FindByID(weapon.Value.Entity);
		if (entity != null)
		{
			float num = (float)The.Sim.GameplayRandomGenerator.RandomNormalDistribution(attackType.ConditionDamageMean.Value, attackType.ConditionDamageStandardDeviation.Value);
			if (attackType.RangeType == AttackType.RangeTypes.Melee && !hasHit)
			{
				num *= 0.3f;
			}
			if (num > 0f && entity.DoDamage(num))
			{
				The.Client.AddLogEvent(entityIntelligence.Allegiance, The.Client.Log.CombatEvent, base.entity, string.Concat("A ", entity.EntityType.Name.ToLower(Config.Culture), " broke while ", base.entity, " was using it."));
			}
		}
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		if (ProcessSubgoals(elapsed) == Status.Completed)
		{
			if (!ValidateSafetyAndTakeAction(null))
			{
				return;
			}
			if (!hasCompletedFirstPhase)
			{
				hasCompletedFirstPhase = true;
				Entity entity = Entity.FindByID(job.Target.Value);
				if (entity == null)
				{
					base.Status = Status.Failed;
					return;
				}
				The.Client.AddLogEvent(The.Client.Log.CombatEvent, base.entity, $"is attacking {entity.ToLink()}!", UWGame.ClientSide.Log.Priority.High);
				if (targetDied)
				{
					base.Status = Status.Completed;
					return;
				}
				double num = 0.0;
				base.Status = Status.Active;
				if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(job.Target.Value, out var _)))
				{
					return;
				}
				bool flag = false;
				if (!HandleWeaponAndAmmo(entity))
				{
					return;
				}
				if (willHitTarget && attackType.RangeType != AttackType.RangeTypes.Ballistic)
				{
					flag = attackType.HitTargets(base.entity, job, ownerOfCarcass, bodyPartToAttackID, entity);
					if (flag)
					{
						DestroyJobAndRemoveLocks(ref job, null, null, parentGoal.GetReplenishActions(), weapon);
					}
					num = GetAttackTypeDurationOrDefault() - GetAttackTypeActionPointOrDefault();
				}
				if (!willHitTarget)
				{
					FireEventActionsWhenMissed(entity);
				}
				DamageWeaponCondition(willHitTarget);
				attackType.StartActionPointEffects(entity, base.entity, willHitTarget);
				if (flag)
				{
					base.Status = Status.Completed;
				}
				else if (num > 0.0 && !flag)
				{
					AddSubgoal(new GoalWait(base.entity, num));
					WakeUpNearbyAllies();
				}
			}
			else if (!hasCompletedRestPhase)
			{
				hasCompletedRestPhase = true;
				if (!SetRestPeriodAfterAttack())
				{
					base.Status = Status.Completed;
				}
			}
			else
			{
				base.Status = Status.Completed;
			}
		}
		SetDetectedByAllWatchers();
	}

	private void FireEventActionsWhenMissed(Entity targetAsEntity)
	{
		attackType.FireEventActions(job, targetAsEntity, entity, AgentActionHooks.MissedAnAttackOnAnEnemy, AgentActionHooks.MissedAnAttackOnPrey);
	}

	private bool SetRestPeriodAfterAttack()
	{
		double? num = null;
		num = (attackType.ChanceToRest.HasValue ? new double?(attackType.ChanceToRest.Value) : ((attackType.RangeType != AttackType.RangeTypes.Melee) ? entity.EntityType.IntelligenceType.ChanceToRestAfterRangedAttack : entity.EntityType.IntelligenceType.ChanceToRestAfterMeleeAttack));
		if (num.HasValue)
		{
			float? num2;
			float? num3;
			if (attackType.RestTimeMean.HasValue)
			{
				num2 = attackType.RestTimeMean.Value;
				num3 = attackType.RestTimeStandardDeviation.Value;
			}
			else
			{
				num2 = entity.EntityType.IntelligenceType.RestTimeAfterAttackingMean;
				num3 = entity.EntityType.IntelligenceType.RestTimeAfterAttackingStandardDeviation;
			}
			if (num2.HasValue && num3.HasValue && The.Sim.GameplayRandomGenerator.NextDouble(null) < num.Value)
			{
				double value = The.Sim.GameplayRandomGenerator.RandomNormalDistribution(num2.Value, num3.Value);
				AddSubgoal(new GoalWait(entity, value, AnimAction.Idle, AnimModifier.Bold));
				ClearAttackAnimFlags();
				return true;
			}
		}
		return false;
	}

	private bool HandleWeaponAndAmmo(Entity target)
	{
		if (weapon.HasValue)
		{
			Entity entity = Entity.FindByID(weapon.Value.Entity);
			if (entity == null)
			{
				base.Status = Status.Failed;
				return false;
			}
			if (!SpendAmmo(entity))
			{
				base.Status = Status.Failed;
				return false;
			}
			if (attackType.RangeType == AttackType.RangeTypes.Ballistic)
			{
				base.entity.AgentStorage.Uncontain(entity);
				if (entity.Locomotor != null)
				{
					entity.Locomotor.StartMoving(Locomotor.Mode.Ballistic, target.PlaySiteLocation, 380f, 0f, base.entity.EntityID, entityIntelligence.Allegiance, attackType, ownerOfCarcass, job);
				}
			}
		}
		return true;
	}

	private bool SpendAmmo(Entity weaponEntity)
	{
		if (attackType.UsesAmmo != null)
		{
			int num = ((MagazineContainer)weaponEntity.Contains).SpendAmmo(attackType.UsesAmmoType, attackType.RoundsToSpend.Value, weaponEntity.GetOwner().OwnedEntities);
			if (num == 0)
			{
				return false;
			}
			if (num < attackType.RoundsToSpend.Value)
			{
				_ = num / attackType.RoundsToSpend.Value;
			}
		}
		return true;
	}

	public static float GetEnergyLevelFactorOnDamage(Entity entity, AttackType attackType)
	{
		if (attackType.RangeType == AttackType.RangeTypes.Melee)
		{
			return GetEnergyLevelFactorOnDamageForMelee(entity);
		}
		return 1f;
	}

	public static float GetEnergyLevelFactorOnDamageForMelee(Entity entity)
	{
		float result = 1f;
		if (entity.Find<BiologicalEntity>(out var c))
		{
			result = MathHelper.Lerp(GameData.Instance.Constants.ZeroEnergyMeleeDamageFactor, 1f, c.EnergyLevel);
		}
		return result;
	}

	public override float GetExertionLevel()
	{
		if (attackType.RangeType == AttackType.RangeTypes.Melee)
		{
			return GameData.Instance.Constants.PhysicalWork.MeleeFighting;
		}
		return GameData.Instance.Constants.PhysicalWork.RangedFighting;
	}

	public override StealthFactor GetStealthFactor()
	{
		if (attackType.RangeType == AttackType.RangeTypes.Melee)
		{
			return StealthFactor.ExtremelyBad;
		}
		return StealthFactor.NotGood;
	}

	public static BodyPart.AttackDirection GetAttackDirection(Entity attacker, Vector3 targetLocation, float targetRotation)
	{
		Vector3 vector = targetLocation - attacker.PlaySiteLocation;
		vector.Normalize();
		vector *= -1f;
		float num = Common.VectorToAngle(vector);
		num -= targetRotation;
		if (num < 0f)
		{
			num += (float)Math.PI * 2f;
		}
		if (num > (float)Math.PI / 4f && num < (float)Math.PI * 3f / 4f)
		{
			return BodyPart.AttackDirection.Right;
		}
		if (num > (float)Math.PI * 3f / 4f && num < 3.926991f)
		{
			return BodyPart.AttackDirection.Back;
		}
		if (num > 3.926991f && num < 5.497787f)
		{
			return BodyPart.AttackDirection.Left;
		}
		return BodyPart.AttackDirection.Front;
	}

	public static bool RollToHit(Entity entity, Entity target, AttackType attackType, BodyPart bodyPart, BodyPart.AttackDirection direction, out BodyPart hitBodyPart)
	{
		hitBodyPart = bodyPart;
		float num = ComputeChanceToHit(entity, target, bodyPart, direction, attackType);
		float num2 = The.Sim.GameplayRandomGenerator.RandomBetween(0f, 1f);
		if (num2 < num)
		{
			return true;
		}
		if (num2 < num + 0.1f)
		{
			hitBodyPart = bodyPart.Body.GetRandomBodyPartToHit(direction);
			return true;
		}
		return false;
	}

	public static float ComputeChanceToHit(Entity entity, IKnownEntityData target, BodyPartID bodyPartID, BodyPart.AttackDirection direction, AttackType attackType)
	{
		BodyPart bodyPartToHit = target.Body.FindBodyPart(bodyPartID);
		return ComputeChanceToHit(entity, target, bodyPartToHit, direction, attackType);
	}

	public static float ComputeChanceToHit(Entity attacker, IKnownEntityData target, BodyPart bodyPartToHit, BodyPart.AttackDirection direction, AttackType attackType)
	{
		float num = 1f;
		float num2 = 1f;
		float num3 = 0f;
		float num4 = 1f;
		float num5 = 1f;
		if (attacker != null)
		{
			num2 = bodyPartToHit.GetToHitModifier(attacker.Bulk, direction);
			if (attackType.RequiredSkillType != null)
			{
				num = attacker.Intelligence.GetSkillValue(attackType.RequiredSkillType);
			}
			if (target is Entity entity && entity.Intelligence.CombatInfo.Target != attacker.EntityID)
			{
				num3 = GameData.Instance.Constants.MeleeToHitBonusOnDistractedTarget;
			}
			if (attacker.Find<BiologicalEntity>(out var c))
			{
				num4 = MathHelper.Lerp(GameData.Instance.Constants.ZeroEnergyToHitFactor, 1f, c.EnergyLevel);
			}
			if (attackType.RangeType != AttackType.RangeTypes.Melee && Common.DistanceOctile(attacker.Location.Value, target.Location.Value) < GameData.Instance.Constants.CloseDistanceForRangedAttack)
			{
				num5 = GameData.Instance.Constants.CloseDistanceRangedAttackToHitFactor;
			}
		}
		float num6 = 0f;
		if (target.Stance != null && target.Stance.IsProne == true)
		{
			num6 = GameData.Instance.Constants.MeleeToHitBonusOnProneTarget;
		}
		return num5 * attackType.AccuracyFactor * num4 * num2 * (num + num3 + num6);
	}

	public override bool HandleMessage(Message message)
	{
		if (!ForwardMessageToFrontMostSubgoal(message))
		{
			switch (message.MessageType)
			{
			case Message.MessageTypes.Hit:
				if (!hasCompletedFirstPhase && The.Sim.GameplayRandomGenerator.NextDouble("GoalDoAttack") < 0.6)
				{
					return true;
				}
				return false;
			case Message.MessageTypes.EntityDied:
			{
				Tuple<EntityID, EntityType> obj = (Tuple<EntityID, EntityType>)((Trigger)message.OtherInfo).messageInfo;
				EntityID item = obj.Item1;
				_ = obj.Item2;
				if (job != null)
				{
					if (item == job.Target)
					{
						targetDied = true;
					}
				}
				else
				{
					targetDied = true;
				}
				return false;
			}
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
		snapshotJob = sn.SnapshotID<Job, JobID>(job);
		ownerOfCarcass = sn.DoEnumNullable(ownerOfCarcass);
		attackType = sn.DoGameData(attackType);
		bodyPartToAttackID = sn.DoEnum(bodyPartToAttackID);
		weapon = sn.DoEntityAndRootNullable(weapon);
		willHitTarget = sn.DoBool(willHitTarget);
		hasCompletedFirstPhase = sn.DoBool(hasCompletedFirstPhase);
		hasCompletedRestPhase = sn.DoBool(hasCompletedRestPhase);
		targetDied = sn.DoBool(targetDied);
		snapshotParentGoal = sn.SnapshotID<Goal, GoalID>(parentGoal).Value;
		sn.Ignore(job);
		sn.Ignore(parentGoal);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		base.LoadPostProcess(sn);
		if (snapshotJob.HasValue)
		{
			job = (AttackJob)LookUp<Job, JobID>.FindByID(snapshotJob);
		}
		parentGoal = (GoalAttack)LookUpGoals.FindByID(snapshotParentGoal);
	}
}
