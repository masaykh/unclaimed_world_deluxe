using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Vehicles;

public class Passenger : ISnapshot
{
	public Vector3 Destination;

	public Entity Entity;

	public int DropoffWaypointNumber;

	public int? RendezvousWaypointNumber;

	private EntityID entityID;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public Passenger(int? RendezvousWaypointNumber, int DropoffWaypointNumber, Vector3 destination, Entity entity)
	{
		Entity = entity;
		Destination = destination;
		this.DropoffWaypointNumber = DropoffWaypointNumber;
		this.RendezvousWaypointNumber = RendezvousWaypointNumber;
	}

	public Passenger()
	{
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.VehicleSnapshotReferences);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		Destination = sn.DoVector3(Destination);
		DropoffWaypointNumber = sn.DoInt32(DropoffWaypointNumber);
		RendezvousWaypointNumber = sn.DoInt32Nullable(RendezvousWaypointNumber);
		if (sn.mode != Snapshotter.Mode.Load)
		{
			entityID = Entity.EntityID;
		}
		entityID = sn.DoEntityID(entityID);
		sn.Ignore(Entity);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		Entity = Entity.FindByID(entityID);
	}
}
