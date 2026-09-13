using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities;
using UWGame.SimSide.SimEffects;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Buildings;

public class Residence : ISnapshot
{
	private Entity parent;

	private EntityID snapshotParent;

	public List<HouseholdID> Households = new List<HouseholdID>();

	public int Residents;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public Residence()
	{
	}

	public Residence(Entity parent)
	{
		this.parent = parent;
	}

	public static float GetSleepNeedGainFactor(double condition, float comfortLevel)
	{
		float f = (float)(condition * (double)comfortLevel);
		f = Common.ClampBottom(f, 0.4f);
		return MathHelper.Lerp(GameData.Instance.Constants.SleepNeed.GainFactorForPeopleSleepingInOpen, GameData.Instance.Constants.SleepNeed.MaximumSleepNeedGainFactorForPeople, f);
	}

	public static bool HasCapacity(Household household, int residents, EntityType residenceType)
	{
		if (residents + household.NoOfMembers > residenceType.ContainerType.ResidenceType.LivingCapacity)
		{
			return false;
		}
		return true;
	}

	public static bool RemoveHousehold(SharedKnowledge sharedKnowledge, Household household)
	{
		if (household.Home.HasValue)
		{
			if (GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(household.Home.Value, out var data)))
			{
				household.Home = null;
				return false;
			}
			data.Households.Remove(household.ID);
			UpdateResidentsNo(data);
			household.Home = null;
		}
		return true;
	}

	public static bool AddHousehold(SharedKnowledge sharedKnowledge, Household household, EntityID? home)
	{
		if (home.HasValue)
		{
			if (GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(home.Value, out var data)))
			{
				home = null;
				return false;
			}
			data.Households.Add(household.ID);
			UpdateResidentsNo(data);
		}
		return true;
	}

	public static List<Entity> GetResidents(IKnownEntityData entityData)
	{
		List<Entity> residents = null;
		if (entityData.Households != null)
		{
			foreach (HouseholdID household in entityData.Households)
			{
				LookUp<Household, HouseholdID>.FindByID(household)?.IterateMembers(delegate(Entity e)
				{
					Common.AddToList(ref residents, e);
				});
			}
		}
		return residents;
	}

	public static void UpdateResidentsNo(IKnownEntityData homeData)
	{
		homeData.Residents = 0;
		for (int num = homeData.Households.Count - 1; num >= 0; num--)
		{
			Household household = LookUp<Household, HouseholdID>.FindByID(homeData.Households[num]);
			if (household != null)
			{
				homeData.Residents += household.NoOfMembers;
			}
			else
			{
				homeData.Households.RemoveAt(num);
			}
		}
	}

	public static bool UpdateResidentsNo(SharedKnowledge sharedKnowledge, EntityID? home)
	{
		if (home.HasValue)
		{
			if (GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(home.Value, out var data)))
			{
				home = null;
				return false;
			}
			UpdateResidentsNo(data);
		}
		return true;
	}

	public static double GetComfortRating(Entity homeData)
	{
		float comfortLevel = homeData.EntityType.ContainerType.ResidenceType.ComfortLevel;
		comfortLevel = homeData.GetEffect(AffectsNumbers.OfferedComfort, comfortLevel);
		double num = homeData.Condition ?? 1.0;
		if (num > (double)GameData.Instance.AIConstants.Ratings.Comfort.MinimumHomeConditionToConsiderPerfect)
		{
			num = 1.0;
		}
		else
		{
			num = MathHelper.Lerp(0f, 1f, (float)num / GameData.Instance.AIConstants.Ratings.Comfort.MinimumHomeConditionToConsiderPerfect);
			num = Common.Clamp(num, 0.0, 1.0);
		}
		return (double)comfortLevel * num;
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		snapshotParent = sn.SnapshotID<Entity, EntityID>(parent).Value;
		Households = sn.DoList(Households);
		Residents = sn.DoInt32(Residents);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		parent = Entity.FindByID(snapshotParent);
	}
}
