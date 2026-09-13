using Microsoft.Xna.Framework;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Jobs;

public class HuntingJob : AttackJob
{
	public double StrengthOfCurrentTakers;

	public double StrengthOfTarget;

	public bool IsSpecificJob;

	public ZoneID? HuntZone;

	public EntityType TargetCreatureType;

	public HuntingJob()
	{
	}

	public HuntingJob(EntityID? target, EntityType targetCreatureType, EntityGroup entityGroup, ZoneID? huntZone = null)
		: base(entityGroup, addToJobsGroupNow: false)
	{
		Target = target;
		HuntZone = huntZone;
		TargetCreatureType = targetCreatureType;
		IsSpecificJob = true;
		entityGroup.AddJob(this);
		ComputeJobType();
		SetDefaultPriority(entityGroup);
	}

	public override void GetLocation(out Point? tilePos, out EntityID? targetEntity, out ZoneID? zoneID)
	{
		tilePos = null;
		zoneID = null;
		targetEntity = Target;
	}

	public GoalEvaluator.CalculateResult ScoreThisJob(ref double rating)
	{
		rating = 1.0;
		return GoalEvaluator.CalculateResult.Done;
	}

	public override void Abandon(Entity entity, bool isDestroyingJob = false)
	{
		base.Abandon(entity, isDestroyingJob);
		if (!IsSpecificJob)
		{
			Target = null;
		}
	}

	public override string GetName()
	{
		return "Hunting";
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		IsSpecificJob = sn.DoBool(IsSpecificJob);
		StrengthOfCurrentTakers = sn.DoDouble(StrengthOfCurrentTakers);
		StrengthOfTarget = sn.DoDouble(StrengthOfTarget);
		HuntZone = sn.DoEnumNullable(HuntZone);
		TargetCreatureType = sn.DoGameData(TargetCreatureType);
		return this;
	}
}
