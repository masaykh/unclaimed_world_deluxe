using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

internal class GoalMoveInToNewHome : CompositeGoal, ITopLevelGoal
{
	private EntityID newHome;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public double TimeSpentInTopLevelGoal { get; set; }

	public GoalMoveInToNewHome(Entity owner, EntityID newHome, List<EntityGroupID> ownersOfVehicles)
		: base(owner)
	{
		this.newHome = newHome;
		base.ownersOfVehicles = ownersOfVehicles;
	}

	protected override void Activate()
	{
		_ = base.entity.ID;
		_ = 5142;
		base.Status = Status.Active;
		RemoveAllSubgoals();
		if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(newHome, out var data)))
		{
			return;
		}
		if (data is Entity entity)
		{
			if (entity.Contains is IResidence residence)
			{
				if (residence.Residence.Residents >= entity.EntityType.ContainerType.ResidenceType.LivingCapacity)
				{
					base.Status = Status.Failed;
				}
				else if (!MoveInToNewHome(personEntity, entity))
				{
					base.Status = Status.Failed;
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
		else
		{
			AddSubgoal(new GoalMoveToPosition(base.entity, ownersOfVehicles, data));
			AddSubgoal(new GoalDoMoveInToNewHome(base.entity, newHome));
		}
	}

	public static bool MoveInToNewHome(Person personEntity, Entity homeEntity)
	{
		if (Residence.HasCapacity(personEntity.Household, homeEntity.Residents.Value, homeEntity.EntityType))
		{
			return personEntity.Household.SetHome(homeEntity.ID);
		}
		return false;
	}

	public GoalMoveInToNewHome()
	{
	}

	public override string GetStatus()
	{
		return "Moving home";
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		base.Status = ProcessSubgoals(elapsed);
		if (base.Status == Status.Active && entityIntelligence.Allegiance.SharedKnowledge.GetKnownData(newHome, out var data) == EntityResult.SeenDirectly)
		{
			if (MoveInToNewHome(personEntity, (Entity)data))
			{
				base.Status = Status.Completed;
			}
			else
			{
				base.Status = Status.Failed;
			}
		}
	}

	public double ScoreGoal()
	{
		return entityIntelligence.GetCurrentGoalUtility().Value;
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		newHome = sn.DoEntityID(newHome);
		TimeSpentInTopLevelGoal = sn.DoDouble(TimeSpentInTopLevelGoal);
		return this;
	}

	public override Snapshotter.Version DoVersion(Snapshotter sn)
	{
		base.DoVersion(sn);
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}
}
