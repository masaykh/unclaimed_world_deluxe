using System;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Overland.Locations;

namespace UWGame.SimSide.Communication;

public class Communicates
{
	public static bool IsInCommunicationRange(ICommunicates from, ICommunicates to, out CommunicationMethod? workingMethod, Site toSite, GeodeticCoordinate? toCoords)
	{
		workingMethod = null;
		if (from.Site != null && from.Site == toSite)
		{
			workingMethod = CommunicationMethod.Direct;
			return true;
		}
		if (!from.Coords.HasValue || !from.Coords.HasValue)
		{
			return false;
		}
		double airDistance = The.Sim.World.GetAirDistance(toCoords.Value, from.Coords.Value);
		if (airDistance < (double)GameData.Instance.Constants.VisualCommunicationRangeInKms)
		{
			workingMethod = CommunicationMethod.Visual;
			return true;
		}
		workingMethod = null;
		foreach (CommunicationMethod value in Enum.GetValues(typeof(CommunicationMethod)))
		{
			if (from.CanCommunicate(value, airDistance) && to.CanCommunicate(value, airDistance))
			{
				workingMethod = value;
				return true;
			}
		}
		return false;
	}

	public static bool IsInCommunicationRange(ICommunicates from, ICommunicates to, out CommunicationMethod? workingMethod)
	{
		return IsInCommunicationRange(from, to, out workingMethod, to.Site, to.Coords);
	}
}
