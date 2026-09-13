using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

internal class GoalEnter : CompositeGoal
{
	private EntityID entityToEnter;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public GoalEnter(Entity entity, EntityID entityToEnter)
		: base(entity)
	{
		this.entityToEnter = entityToEnter;
	}

	public GoalEnter()
	{
	}

	protected override void Activate()
	{
		IKnownEntityData data = null;
		EntityResult knownData = entityIntelligence.Allegiance.SharedKnowledge.GetKnownData(entityToEnter, out data);
		if (EntityResultCausesFailedGoal(knownData))
		{
			return;
		}
		if (!base.entity.GetContainedBy(out Entity container))
		{
			base.Status = Status.Failed;
		}
		else if (data is Entity { Contains: not null } entity)
		{
			if (container == data)
			{
				base.Status = Status.Completed;
				return;
			}
			Vector3 rallyPoint = entity.AccessPoint.Value;
			Vector3 position = rallyPoint;
			if (entity.Contains is IExit exit)
			{
				ExitDoor exitDoor = exit.ReserveDoorForEntryOrExit(base.entity, exiting: false);
				if (exitDoor == ExitDoor.NoneNeeded || exitDoor == ExitDoor.NoneAvailable)
				{
					exit.GetNaturalRallyPoint(ref rallyPoint);
				}
				else if (exitDoor < ExitDoor.Max)
				{
					rallyPoint = exit.GetRallyPoint(exitDoor);
				}
				exit.GetDoorPosition(ref position, exiting: false, out var _, exitDoor);
			}
			Point p = MapManager.WorldPosToSubtile(rallyPoint);
			if (!The.Map.SubtileIsCompletelyBlocked(The.Map.TerrainCosts[SurfaceType.TransportType.Foot], p) && !IsCloseEnoughForPickupAndWaypoints(rallyPoint, base.entity.Location.Value))
			{
				AddSubgoal(new GoalMoveToPosition(base.entity, rallyPoint, null, GoalMoveToPosition.VehicleUse.NoVehicle));
			}
			Point p2 = MapManager.WorldPosToSubtile(position);
			if (!The.Map.SubtileIsCompletelyBlocked(The.Map.TerrainCosts[SurfaceType.TransportType.Foot], p2))
			{
				if (!IsCloseEnoughForPickupAndWaypoints(position, base.entity.PlaySiteLocation))
				{
					AddSubgoal(new GoalMoveToPosition(base.entity, position, null, GoalMoveToPosition.VehicleUse.NoVehicle));
				}
			}
			else if (!IsCloseEnoughForPickupAndWaypoints(entity.AccessPoint.Value, base.entity.PlaySiteLocation))
			{
				AddSubgoal(new GoalMoveToPosition(base.entity, entity.AccessPoint.Value, null, GoalMoveToPosition.VehicleUse.NoVehicle));
			}
			base.Status = Status.Active;
		}
		else
		{
			base.Status = Status.Failed;
		}
	}

	private bool IsCloseEnoughForPickupAndWaypoints(Vector3 from, Vector3 to)
	{
		return Vector3.DistanceSquared(from, to) <= GameData.Instance.Constants.DistanceSquaredLimitForWaypoints;
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		base.Status = ProcessSubgoals(elapsed);
		if (base.Status == Status.Failed || base.Status != Status.Completed)
		{
			return;
		}
		IKnownEntityData data = null;
		if (entityIntelligence.Allegiance.SharedKnowledge.GetKnownData(entityToEnter, out data) != EntityResult.SeenDirectly)
		{
			base.Status = Status.Failed;
			return;
		}
		Entity entity = (Entity)data;
		if (!entity.Contains.Contains(base.entity.ID))
		{
			entity.Contains.AddToContain(base.entity);
			if (!The.Sim.DateAndTime.SunIsUp)
			{
				int num = 1;
				if (entity.Contains is IGarrison garrison)
				{
					num = garrison.GetNoOfAgentsInside();
				}
				float fractionToTurnOn = MathHelper.Clamp((float)num / (float)entity.EntityType.ContainerType.GetCapacityForIdlingPeople(), 0f, 1f);
				entity.TurnOnDesiredShareOfLights(fractionToTurnOn);
			}
		}
		base.Status = Status.Completed;
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
		entityToEnter = sn.DoEntityID(entityToEnter);
		return this;
	}
}
