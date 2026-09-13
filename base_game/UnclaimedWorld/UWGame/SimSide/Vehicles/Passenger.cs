using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Vehicles;

public class Passenger
{
	public Vector3 Destination;

	public Entity Entity;

	public int DropoffWaypointNumber;

	public int? RendezvousWaypointNumber;

	public Passenger(int? RendezvousWaypointNumber, int DropoffWaypointNumber, Vector3 destination, Entity entity)
	{
		Entity = entity;
		Destination = destination;
		this.DropoffWaypointNumber = DropoffWaypointNumber;
		this.RendezvousWaypointNumber = RendezvousWaypointNumber;
	}
}
