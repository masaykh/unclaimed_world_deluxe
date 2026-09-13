using System.Collections.Generic;
using System.Xml.Serialization;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Overland.Missions.Templates;

public class TransportationTemplate : ISnapshot
{
	public long? HiredFromOwner;

	public List<Pair<string, int>> Vehicles;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	[XmlIgnore]
	public bool IsSnapshotted { get; set; }

	public double GetEstimatedTravelSpeed()
	{
		float num = 1000000f;
		if (Vehicles != null)
		{
			foreach (Pair<string, int> vehicle in Vehicles)
			{
				float averageOverlandTravelSpeed = ((VehicleContainerType)GameData.Instance.AllEntityTypes[vehicle.First].ContainerType).AverageOverlandTravelSpeed;
				if (averageOverlandTravelSpeed < num)
				{
					num = averageOverlandTravelSpeed;
				}
			}
			return num;
		}
		return GameData.Instance.Constants.AverageOverlandSpeedOnFoot;
	}

	public EntityType GetMainTransportation()
	{
		if (Vehicles != null && Vehicles.Count > 0)
		{
			string first = Vehicles[0].First;
			return GameData.Instance.AllEntityTypes[first];
		}
		return null;
	}

	public decimal ComputeTransportationCost(MissionTemplate parent, out decimal startFee, out decimal totalDistanceCost, out decimal costPerKilometer)
	{
		startFee = default(decimal);
		totalDistanceCost = default(decimal);
		costPerKilometer = default(decimal);
		if (HiredFromOwner.HasValue && LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)parent.Allegiance) != null)
		{
			IOwner owner = LookUpOwners.FindByID((OwnerID)HiredFromOwner.Value);
			if (owner != null)
			{
				double totalDistance = parent.GetTotalDistance();
				foreach (Pair<string, int> vehicle in Vehicles)
				{
					int second = vehicle.Second;
					decimal? pricePerKilometer;
					decimal? vehicleForHirePrice = owner.OwnedEntities.GetVehicleForHirePrice(GameData.Instance.AllEntityTypes[vehicle.First], out pricePerKilometer);
					costPerKilometer += (pricePerKilometer ?? 0m) * (decimal)second;
					decimal num = (decimal)second * (vehicleForHirePrice ?? 0m);
					decimal num2 = (decimal)second * (decimal)totalDistance * (pricePerKilometer ?? 0m);
					startFee += num;
					totalDistanceCost += num2;
				}
			}
		}
		return startFee + totalDistanceCost;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		Vehicles = sn.DoList(Vehicles);
		HiredFromOwner = sn.DoInt64Nullable(HiredFromOwner);
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
	}
}
