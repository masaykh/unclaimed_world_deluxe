using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

internal class GoalArriveAsVisitor : CompositeGoal
{
	private EntityID? entityToArriveAt;

	private ExpeditionID? expeditionToArriveAt;

	private Vector3 seat = Vector3.Zero;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public GoalArriveAsVisitor(Entity entity, EntityID? newPlace, List<EntityGroupID> ownersOfVehicles)
		: base(entity)
	{
		entityToArriveAt = newPlace;
		base.ownersOfVehicles = ownersOfVehicles;
	}

	public GoalArriveAsVisitor(Entity entity, ExpeditionID? newPlace, List<EntityGroupID> ownersOfVehicles)
		: base(entity)
	{
		expeditionToArriveAt = newPlace;
		base.ownersOfVehicles = ownersOfVehicles;
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
		entityToArriveAt = sn.DoEntityIDNullable(entityToArriveAt);
		expeditionToArriveAt = sn.DoEnumNullable(expeditionToArriveAt);
		seat = sn.DoVector3(seat);
		return this;
	}

	public GoalArriveAsVisitor()
	{
	}

	protected override void Activate()
	{
		base.Status = Status.Active;
		RemoveAllSubgoals();
		IKnownEntityData data = null;
		Vector2? vector;
		if (entityToArriveAt.HasValue)
		{
			if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(entityToArriveAt.Value, out data)))
			{
				return;
			}
			seat = data.AccessPoint.Value;
			vector = data.GatheringSite.AddVisitor(ref entity);
		}
		else
		{
			Expedition expedition = Expedition.FindByID(expeditionToArriveAt.Value);
			seat = expedition.Center.Value;
			vector = expedition.GatheringSite.AddVisitor(ref entity);
		}
		if (vector.HasValue)
		{
			seat.X = vector.Value.X;
			seat.Y = vector.Value.Y;
		}
		AddSubgoal(new GoalMoveToPosition(entity, seat, null, GoalMoveToPosition.VehicleUse.NoVehicle));
		if (data != null)
		{
			AddSubgoal(new GoalTurnToFace(entity, data.PlaySiteLocation.ToVector2()));
		}
		else
		{
			AddSubgoal(new GoalTurnToFace(entity, Expedition.FindByID(expeditionToArriveAt.Value).Location.Value.ToVector2()));
		}
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		base.Status = ProcessSubgoals(elapsed);
		if (base.Status == Status.Active)
		{
			return;
		}
		if (base.Status == Status.Completed)
		{
			if (entityToArriveAt.HasValue)
			{
				entityIntelligence.GetKnownData(entityToArriveAt.Value, out var data);
				data?.GatheringSite.OnDockReached(ref entity);
			}
			else
			{
				Expedition.FindByID(expeditionToArriveAt.Value).GatheringSite.OnDockReached(ref entity);
			}
		}
		else
		{
			_ = base.Status;
			_ = 3;
		}
	}

	public override void OnExit()
	{
		base.OnExit();
	}
}
