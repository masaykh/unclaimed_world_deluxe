using System.Collections.Generic;
using System.Xml.Serialization;
using UWGame.SimSide.AI;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Overland.Locations;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Overland.Missions.Templates;

public class TravelActionTemplate : MissionActionTemplate
{
	public long? Route;

	public bool? UsesAirRoute;

	public MissionStopTemplate ToMissionStop;

	private MissionStopTemplateID snapshotToMissionStop;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public override string Name => TemplateName;

	public static string TemplateName => "Travel";

	public override ActionTypes ActionType => ActionTypes.Travel;

	[XmlIgnore]
	public double Distance { get; private set; }

	[XmlIgnore]
	public double? Bearing { get; private set; }

	public TravelActionTemplate(MissionStopTemplate missionStopTemplate, MissionStopTemplate to)
		: base(missionStopTemplate, allowDeleting: false)
	{
		ToMissionStop = to;
	}

	public TravelActionTemplate()
	{
	}

	public void SetRoute(Route route, bool useAirRoute)
	{
		UsesAirRoute = null;
		Route = null;
		if (route != null)
		{
			Distance = route.Length;
			Route = (long)route.ID;
		}
		else if (useAirRoute)
		{
			UsesAirRoute = true;
			Site site = LookUp<Site, SiteID>.FindByID((SiteID)MissionStopTemplate.TravelLocation.SiteID);
			Site site2 = LookUp<Site, SiteID>.FindByID((SiteID)ToMissionStop.TravelLocation.SiteID);
			Distance = The.Sim.World.GetAirDistance(site.Coords, site2.Coords);
			Bearing = DistanceCalculator.GetBearing(site.Coords, site2.Coords);
		}
	}

	public override void AssignIDs()
	{
		base.AssignIDs();
		ToMissionStop.AssignIDs();
	}

	private bool ValidateRoute(VehicleContainerType vehicle, ref List<string> errors)
	{
		return ValidateRoute(Route, UsesAirRoute, Distance, vehicle, ref errors);
	}

	public static bool ValidateRoute(long? routeID, bool? usesAirRoute, double distance, VehicleContainerType vehicle, ref List<string> errors)
	{
		bool flag = true;
		if (usesAirRoute == true || routeID.HasValue)
		{
			RouteType? routeType = null;
			bool isAirRoute = false;
			if (routeID.HasValue)
			{
				Route route = LookUp<UWGame.SimSide.Overland.Route, RouteID>.FindByID((RouteID)routeID.Value);
				routeType = route.RouteType;
			}
			else
			{
				isAirRoute = true;
			}
			if (!vehicle.CanUseRoute(routeType, isAirRoute, distance))
			{
				flag = false;
			}
		}
		else
		{
			flag = false;
		}
		if (!flag)
		{
			Common.AddToList(ref errors, "No valid route exists.");
			return false;
		}
		return true;
	}

	public override bool Validate(MissionTemplate parent, ref bool hasMeaning, ref List<string> errors)
	{
		EntityType mainTransportation = parent.TransportationType.GetMainTransportation();
		if (mainTransportation != null)
		{
			VehicleContainerType vehicle = mainTransportation.ContainerType as VehicleContainerType;
			if (!ValidateRoute(vehicle, ref errors))
			{
				return false;
			}
			return CanUseTerminal(parent, ToMissionStop.TravelLocation, vehicle, ref errors);
		}
		Common.AddToList(ref errors, "No transportation selected.");
		return false;
	}

	public static bool CanUseTerminal(MissionTemplate parent, TravelLocation travelLocation, VehicleContainerType vehicle, ref List<string> errors)
	{
		TerminalType.TypesOfTerminal? canUseTerminal = vehicle.CanUseTerminal;
		if (canUseTerminal.HasValue)
		{
			Allegiance allegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)parent.Allegiance);
			if (travelLocation.ResolveLocation(allegiance.SharedKnowledge, out var _, out var _, out var _, out var terminalData) && canUseTerminal != terminalData.EntityType.TerminalType.TypeOfTerminal)
			{
				Common.AddToList(ref errors, GetWrongTerminalError(terminalData));
				return false;
			}
		}
		return true;
	}

	public static string GetWrongTerminalError(IKnownEntityData terminal)
	{
		return terminal.EntityType.Name + " is the wrong terminal type for this vehicle.";
	}

	public DateAndTime.TimeDateYear GetTravelTime(MissionTemplate parent)
	{
		double estimatedTravelSpeed = parent.TransportationType.GetEstimatedTravelSpeed();
		double totalDays = Distance / estimatedTravelSpeed;
		return new DateAndTime.TimeDateYear(totalDays);
	}

	public override float ComputeTotalCargoBulk()
	{
		return ToMissionStop.ComputeTotalCargoBulk();
	}

	public override decimal ComputeTotalCost(MissionTemplate parent, out decimal boughtItemsCost, out decimal soldItemsCost)
	{
		return 0m + ToMissionStop.ComputeTotalCost(parent, out boughtItemsCost, out soldItemsCost);
	}

	public override MissionAction CreateMissionAction(Mission mission)
	{
		return new TravelAction(mission, this);
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		Route = sn.DoInt64Nullable(Route);
		UsesAirRoute = sn.DoBoolNullable(UsesAirRoute);
		snapshotToMissionStop = sn.SnapshotID<MissionStopTemplate, MissionStopTemplateID>(ToMissionStop).Value;
		Distance = sn.DoDouble(Distance);
		Bearing = sn.DoDoubleNullable(Bearing);
		return this;
	}

	public override Snapshotter.Version DoVersion(Snapshotter sn)
	{
		base.DoVersion(sn);
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		base.LoadPostProcess(sn);
		sn.RegisterLoadPostProcessCall(this);
		ToMissionStop = LookUp<MissionStopTemplate, MissionStopTemplateID>.FindByID(snapshotToMissionStop);
	}
}
