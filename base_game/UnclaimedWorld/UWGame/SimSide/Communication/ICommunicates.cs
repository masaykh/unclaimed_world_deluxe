using UWGame.SimSide.Overland;
using UWGame.SimSide.Overland.Locations;

namespace UWGame.SimSide.Communication;

public interface ICommunicates
{
	Site Site { get; }

	GeodeticCoordinate? Coords { get; }

	bool CanCommunicate(CommunicationMethod method, double distance);
}
