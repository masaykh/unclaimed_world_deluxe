using System;
using System.Collections.Generic;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.AI;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities.Body;

public class Body : IHasBodyParts, ISnapshot
{
	public struct BodyPartChance : IScore, IEdge
	{
		public BodyPart BodyPart;

		public float Score { get; set; }

		public float Edge { get; set; }
	}

	public MemoryFact ParentMemoryFact;

	private MemoryFactID? snapshotParentMemoryFact;

	public Entity Parent;

	private EntityID? snapshotParentEntity;

	public float MaxHitpoints;

	public double FunctionalScore = 1.0;

	private float globalHitpoints;

	private float lowestHitpointsFraction;

	private List<BodyPart> bodyParts = new List<BodyPart>();

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public List<BodyPart> BodyParts
	{
		get
		{
			return bodyParts;
		}
		set
		{
			bodyParts = value;
		}
	}

	private float HitpointsFractionLeft => GlobalHitpoints / MaxHitpoints;

	public float GlobalHitpoints
	{
		get
		{
			return globalHitpoints;
		}
		set
		{
			if (value != globalHitpoints)
			{
				globalHitpoints = value;
				lowestHitpointsFraction = Math.Min(HitpointsFractionLeft, lowestHitpointsFraction);
				UpdateRenderableHitpoints();
				if (Parent != null && Parent.EntityType.IntelligenceType != null)
				{
					Parent.Intelligence.SetPanicLevelDirty();
				}
			}
		}
	}

	public bool IsSnapshotted { get; set; }

	public Body(Entity parent)
	{
		Parent = parent;
		lowestHitpointsFraction = 1f;
	}

	public Body()
	{
	}

	public Body(Body original)
	{
		MaxHitpoints = original.MaxHitpoints;
		FunctionalScore = original.FunctionalScore;
		globalHitpoints = original.globalHitpoints;
		CopyBodyParts(bodyParts, original.bodyParts);
	}

	private void CopyBodyParts(List<BodyPart> copiedBodyParts, List<BodyPart> originalBodyParts)
	{
		foreach (BodyPart originalBodyPart in originalBodyParts)
		{
			BodyPart bodyPart = ((!(originalBodyPart is BiologicalBodyPart)) ? ((BodyPart)new MachineBodyPart(originalBodyPart)) : ((BodyPart)new BiologicalBodyPart(originalBodyPart)));
			bodyPart.Body = this;
			bodyParts.Add(bodyPart);
			if (originalBodyPart.BodyParts != null)
			{
				bodyPart.BodyParts = new List<BodyPart>();
				CopyBodyParts(bodyPart.BodyParts, originalBodyPart.BodyParts);
			}
		}
	}

	public float GetHurtVitalBodyPartFactorForMorale()
	{
		float currentFactor = 1f;
		foreach (BodyPart bodyPart in bodyParts)
		{
			bodyPart.GetHurtVitalBodyPartFactor(ref currentFactor);
		}
		return currentFactor;
	}

	public float GetModifiedHitpointsForPresentation()
	{
		float totalHitpoints = 0f;
		float totalMaxHitpoints = 0f;
		foreach (BodyPart bodyPart in bodyParts)
		{
			bodyPart.GetModifiedHitpointsForPresentation(ref totalHitpoints, ref totalMaxHitpoints);
		}
		return Common.Clamp(totalHitpoints / totalMaxHitpoints, 0f, 1f);
	}

	public void UpdateRenderable()
	{
		UpdateRenderableHitpoints();
	}

	private void UpdateRenderableHitpoints()
	{
		if (Parent.Renderable != null)
		{
			if (HitpointsFractionLeft < 0.5f)
			{
				Parent.Renderable.SetAnimationStateFlag(AnimModifier.Damaged);
			}
			else
			{
				Parent.Renderable.ClearAnimationStateFlag(AnimModifier.Damaged);
			}
		}
	}

	public void RegainHitpoints(double deltaTimeInSeconds)
	{
		if (Parent.Name != null)
		{
			Parent.Name.Contains("Conlan");
		}
		if (GlobalHitpoints < MaxHitpoints)
		{
			BiologicalEntity biologicalEntity = Parent.BiologicalEntity;
			float num = lowestHitpointsFraction + (1f - lowestHitpointsFraction) * biologicalEntity.MaxRegainLimit;
			// MOD: a wound may heal all the way rather than stopping halfway back to full.
			num = UWGame.Mods.HealingMod.RecoveryCeiling(num);
			if (HitpointsFractionLeft < num)
			{
				// MOD: and how fast depends on food, sleep and morale, not muscle energy alone.
				float healingFactor = UWGame.Mods.HealingMod.RateFactor(Parent);
				float val = (float)((double)(healingFactor * Common.ClampBottom(Parent.BiologicalEntity.EnergyLevel, 0.5f) * biologicalEntity.FractionOfMaxHitpointsGainedPerDay * MaxHitpoints) * deltaTimeInSeconds / The.Sim.DateAndTime.SecondsPerDay);
				val = Math.Min(val, MaxHitpoints - GlobalHitpoints);
				RegainInBodyParts(val);
				GlobalHitpoints += val;
			}
		}
	}

	private void RegainInBodyParts(float totalRegainAmount)
	{
		foreach (BodyPart bodyPart in BodyParts)
		{
			bodyPart.Regain(totalRegainAmount);
		}
	}

	public BodyPart GetRandomBodyPartToHit(BodyPart.AttackDirection attackDirection)
	{
		List<BodyPartChance> list = new List<BodyPartChance>();
		foreach (BodyPart bodyPart in BodyParts)
		{
			bodyPart.GatherBodyParts(list, attackDirection);
		}
		Common.BuildEdgesFromBucketSizes(list, doSort: true, out var totalScore);
		int stairstep;
		return Common.GetStairStepIndex(list, out stairstep, The.Sim.GameplayRandomGenerator, totalScore).BodyPart;
	}

	public void UpdateBulk(float oldBulk)
	{
		if (!Parent.IsDead)
		{
			if (GlobalHitpoints == 0f)
			{
				UpdateHitpoints();
			}
			else
			{
				UpdateHitpointsWithNewBulk(oldBulk);
			}
		}
	}

	public void ChangeMaxHitpoints(float newMaxHitPointsValue)
	{
		MaxHitpoints = newMaxHitPointsValue;
		UpdateBodyHitpoints();
	}

	public void Initialize()
	{
		if (Parent.EntityType.BodyType.Bulk.HasValue)
		{
			Parent.Bulk = Parent.EntityType.BodyType.Bulk.Value;
		}
	}

	private void UpdateBodyHitpoints()
	{
		GlobalHitpoints = MaxHitpoints;
		UpdateBodyPartHitpoints();
	}

	private void UpdateHitpoints()
	{
		float? resilience = null;
		if (Parent.BiologicalEntity != null)
		{
			resilience = Parent.BiologicalEntity.Resilience;
		}
		MaxHitpoints = ComputeHitpoints(Parent.EntityType, Parent.Bulk, resilience);
		UpdateBodyHitpoints();
	}

	public static float ComputeHitpoints(EntityType entityType, float bulk, float? resilience)
	{
		if (entityType.BodyType.Hitpoints.HasValue)
		{
			return entityType.BodyType.Hitpoints.Value;
		}
		return ComputeHitpoints(bulk, resilience.Value);
	}

	public void UpdateBodyPartHitpoints()
	{
		foreach (BodyPart bodyPart in BodyParts)
		{
			bodyPart.InitializeHitpoints();
		}
	}

	private bool IsBodyPartFatallyDamaged()
	{
		foreach (BodyPart bodyPart in BodyParts)
		{
			if (bodyPart.IsBodyPartDamageFatal())
			{
				return true;
			}
		}
		return false;
	}

	private bool IsBodyPartCausingCollapse()
	{
		foreach (BodyPart bodyPart in BodyParts)
		{
			if (bodyPart.IsBodyPartDamageCausingCollapse())
			{
				return true;
			}
		}
		return false;
	}

	public bool IsDead()
	{
		if (!(GlobalHitpoints <= 0f))
		{
			return IsBodyPartFatallyDamaged();
		}
		return true;
	}

	public void GetStatus(out bool isDead, out bool isUnconscious, ref CauseOfDeath? causeOfDeath, ref CauseOfUnconsciousness? causeOfUnconsciousness)
	{
		isDead = IsDead();
		isUnconscious = false;
		if (!isDead)
		{
			isUnconscious = GlobalHitpoints <= GameData.Instance.Constants.FractionOfHitpointsCausingCollapse * MaxHitpoints;
			if (!isUnconscious)
			{
				isUnconscious = IsBodyPartCausingCollapse();
			}
		}
		if (isDead)
		{
			causeOfDeath = CauseOfDeath.Wounds;
		}
		if (isUnconscious)
		{
			causeOfUnconsciousness = CauseOfUnconsciousness.Wounds;
		}
	}

	private static float ComputeHitpoints(float bulk, float resilience)
	{
		return 100f * resilience * bulk;
	}

	public void GetBodyParts<Type>(ref List<Type> listToFillWithProperties) where Type : IHasExposedProperties
	{
		if (BodyParts == null)
		{
			return;
		}
		foreach (BodyPart bodyPart in BodyParts)
		{
			bodyPart.GetList(ref listToFillWithProperties);
		}
	}

	private void UpdateHitpointsWithNewBulk(float oldBulk)
	{
		if (GlobalHitpoints == MaxHitpoints)
		{
			UpdateHitpoints();
		}
		else
		{
			if (Parent.EntityType.BodyType.Hitpoints.HasValue)
			{
				return;
			}
			float resilience = 1f;
			if (Parent.Find<BiologicalEntity>(out var c))
			{
				resilience = c.Resilience;
			}
			MaxHitpoints = ComputeHitpoints(Parent.Bulk, resilience);
			float bulkPercentageIncrease = GetBulkPercentageIncrease(oldBulk, Parent.Bulk);
			GlobalHitpoints = (1f + bulkPercentageIncrease) * GlobalHitpoints;
			foreach (BodyPart bodyPart in BodyParts)
			{
				bodyPart.UpdateHitpointsWithNewBulk(bulkPercentageIncrease);
			}
		}
	}

	public static float GetBulkPercentageIncrease(float oldBulk, float currentBulk)
	{
		return (currentBulk - oldBulk) / oldBulk;
	}

	public float GetHitpointsFractionUntilUnconsciousness()
	{
		return GetHitpointsUntilUnonsciousness() / GetMaxHitpointsUntilUnconsciousness();
	}

	private float GetMaxHitpointsUntilUnconsciousness()
	{
		return MaxHitpoints * (1f - GameData.Instance.Constants.FractionOfHitpointsCausingCollapse);
	}

	private float GetHitpointsUntilUnonsciousness()
	{
		float num = MaxHitpoints - globalHitpoints;
		return GetMaxHitpointsUntilUnconsciousness() - num;
	}

	public BodyPart FindBodyPartOfType(BodyPartType bodyPartType)
	{
		foreach (BodyPart bodyPart2 in BodyParts)
		{
			BodyPart bodyPart = bodyPart2.FindFirstMatchingBodyPart((BodyPart p) => p.BodyPartType == bodyPartType);
			if (bodyPart != null)
			{
				return bodyPart;
			}
		}
		return null;
	}

	public BodyPart FindBodyPart(BodyPartID bodyPartID)
	{
		foreach (BodyPart bodyPart2 in BodyParts)
		{
			BodyPart bodyPart = bodyPart2.FindFirstMatchingBodyPart((BodyPart p) => p.BodyPartID == bodyPartID);
			if (bodyPart != null)
			{
				return bodyPart;
			}
		}
		return null;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		bodyParts = sn.DoList(bodyParts);
		FunctionalScore = sn.DoDouble(FunctionalScore);
		globalHitpoints = sn.DoFloat(globalHitpoints);
		lowestHitpointsFraction = sn.DoFloat(lowestHitpointsFraction);
		MaxHitpoints = sn.DoFloat(MaxHitpoints);
		snapshotParentMemoryFact = sn.SnapshotID<MemoryFact, MemoryFactID>(ParentMemoryFact);
		snapshotParentEntity = sn.SnapshotID<Entity, EntityID>(Parent);
		sn.Ignore(Parent);
		sn.Ignore(ParentMemoryFact);
		return this;
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		if (snapshotParentMemoryFact.HasValue)
		{
			ParentMemoryFact = LookUp<MemoryFact, MemoryFactID>.FindByID(snapshotParentMemoryFact.Value);
		}
		snapshotParentMemoryFact = null;
		Parent = Entity.FindByID(snapshotParentEntity);
		foreach (BodyPart bodyPart in bodyParts)
		{
			bodyPart.LoadPostProcess(sn);
		}
	}
}
