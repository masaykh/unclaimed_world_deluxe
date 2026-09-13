using System;
using UWGame.SimSide.AI;
using UWGame.SimSide.Combat;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Jobs;

public class ThreatJob : AttackJob
{
	public double ThreatRating;

	private bool isVermin;

	public EntityID? ClosestMemberOfAllegiance;

	private int maxTakers;

	public bool IsVermin
	{
		get
		{
			return isVermin;
		}
		set
		{
			if (value == isVermin)
			{
				return;
			}
			if (ResolveOwner(out var owner))
			{
				if (isVermin)
				{
					owner.AssetThreatJobs.Remove(this);
					owner.AssetThreatJobsByTarget.Remove(Target.Value);
					owner.ThreatJobs.Add(this);
					owner.ThreatJobsByTarget.Add(Target.Value, this);
				}
				else
				{
					owner.ThreatJobs.Remove(this);
					owner.ThreatJobsByTarget.Remove(Target.Value);
					owner.AssetThreatJobs.Add(this);
					owner.AssetThreatJobsByTarget.Add(Target.Value, this);
				}
			}
			isVermin = value;
		}
	}

	public override int MaxJobPositions => maxTakers;

	public ThreatJob()
	{
	}

	public ThreatJob(Entity entity, EntityGroup entityGroup, bool isAssetThreatOnly)
		: base(entityGroup, addToJobsGroupNow: false)
	{
		Target = entity.EntityID;
		isVermin = isAssetThreatOnly;
		entityGroup.AddJob(this);
		if (IsVermin)
		{
			maxTakers = GameData.Instance.AIConstants.Combat.MaxTakersForVerminThreatJob;
		}
		else
		{
			float num = entity.StrengthRating.Value / GameData.Instance.Constants.StrengthRatings[entityGroup.GetAllegiance().RepresentativeEntityType.IntelligenceType.StrengthRating];
			float f = GameData.Instance.AIConstants.Combat.JobTakersPerStrengthRatio * num;
			maxTakers = (int)Math.Round(Common.Clamp(f, 2f, GameData.Instance.AIConstants.Combat.MaxTakersForThreatJob));
		}
		ComputeJobType();
		SetDefaultPriority(entityGroup);
	}

	public override double GetWeaponPolicyScore(Entity entity, IKnownEntityData weapon, AttackType attackType)
	{
		double result = 1.0;
		if (IsVermin)
		{
			Expedition currentExpedition = entity.Intelligence.CurrentExpedition;
			if (currentExpedition != null && currentExpedition.Policy != null)
			{
				result = currentExpedition.Policy.GetWeaponPolicyScore(attackVermin: true, weapon.EntityType, attackType);
			}
		}
		return result;
	}

	public float GetUrgency(Entity entity)
	{
		if (ClosestMemberOfAllegiance.HasValue)
		{
			if (ClosestMemberOfAllegiance == entity.EntityID)
			{
				return 0.4f;
			}
			return 1f;
		}
		return 0.5f;
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		ClosestMemberOfAllegiance = sn.DoEnumNullable(ClosestMemberOfAllegiance);
		ThreatRating = sn.DoDouble(ThreatRating);
		IsVermin = sn.DoBool(IsVermin);
		maxTakers = sn.DoInt32(maxTakers);
		return this;
	}
}
