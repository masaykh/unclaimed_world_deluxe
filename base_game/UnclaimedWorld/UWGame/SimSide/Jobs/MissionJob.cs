using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Jobs;

public class MissionJob : Job
{
	public Dictionary<EntityType, List<EntityID>> Vehicles;

	public MissionJob()
	{
	}

	public MissionJob(EntityGroup entityGroup, Dictionary<EntityType, List<EntityID>> vehicles)
		: base(entityGroup)
	{
		Vehicles = vehicles;
		if (Vehicles != null)
		{
			foreach (KeyValuePair<EntityType, List<EntityID>> vehicle in Vehicles)
			{
				foreach (EntityID item in vehicle.Value)
				{
					entityGroup.GetKnownData(item, out var data);
					if (data != null)
					{
						data.AssignedToJob = base.ID;
					}
					else
					{
						Destroy(cancelTakers: true);
					}
				}
			}
		}
		ComputeJobType();
		SetDefaultPriority(entityGroup);
	}

	public override Vector3? GetCircaLocation()
	{
		return null;
	}

	public double GetRealizedTravelSpeed()
	{
		float num = 1000000f;
		if (Vehicles != null)
		{
			foreach (KeyValuePair<EntityType, List<EntityID>> vehicle in Vehicles)
			{
				foreach (EntityID item in vehicle.Value)
				{
					Entity entity = Entity.FindByID(item);
					if (entity != null)
					{
						float averageOverlandTravelSpeed = ((VehicleContainerType)entity.EntityType.ContainerType).AverageOverlandTravelSpeed;
						if (averageOverlandTravelSpeed < num)
						{
							num = averageOverlandTravelSpeed;
						}
					}
				}
			}
			return num;
		}
		return GameData.Instance.Constants.AverageOverlandSpeedOnFoot;
	}

	public void IterateVehicles(Action<Entity> function)
	{
		if (Vehicles == null)
		{
			return;
		}
		foreach (KeyValuePair<EntityType, List<EntityID>> vehicle in Vehicles)
		{
			foreach (EntityID item in vehicle.Value)
			{
				Entity entity = Entity.FindByID(item);
				if (entity != null)
				{
					function(entity);
				}
			}
		}
	}

	public void IterateVehicleContents(Action<Entity> function)
	{
		if (Vehicles == null)
		{
			return;
		}
		foreach (KeyValuePair<EntityType, List<EntityID>> vehicle in Vehicles)
		{
			foreach (EntityID item in vehicle.Value)
			{
				Entity.FindByID(item)?.Contains.IterateContained(delegate(Entity e)
				{
					function(e);
				});
			}
		}
	}

	public override void Destroy(bool cancelTakers, Entity entityToExclude = null)
	{
		JobID iD = base.ID;
		base.Destroy(cancelTakers, entityToExclude);
		if (Vehicles == null)
		{
			return;
		}
		if (ResolveOwner(out var owner))
		{
			foreach (KeyValuePair<EntityType, List<EntityID>> vehicle in Vehicles)
			{
				foreach (EntityID item in vehicle.Value)
				{
					owner.GetKnownData(item, out var data);
					if (data != null && data.AssignedToJob == iD)
					{
						data.AssignedToJob = null;
					}
				}
			}
		}
		Vehicles.Clear();
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		Vehicles = sn.DoMultiMap(Vehicles);
		return this;
	}
}
