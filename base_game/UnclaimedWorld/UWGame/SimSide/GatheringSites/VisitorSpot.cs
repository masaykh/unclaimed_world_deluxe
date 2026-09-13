using System;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.GatheringSites;

public class VisitorSpot : IComparable<VisitorSpot>, ISnapshot
{
	public EntityID EntityID;

	public Vector2 WorldLocation;

	public double DistanceSquared;

	public bool Arrived;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public VisitorSpot(EntityID id, Vector2 pos, bool arrivedIn = false)
	{
		EntityID = id;
		WorldLocation = pos;
		DistanceSquared = 0.0;
		Arrived = arrivedIn;
	}

	public VisitorSpot()
	{
	}

	public int CompareTo(VisitorSpot other)
	{
		if (other == null)
		{
			return 1;
		}
		return DistanceSquared.CompareTo(other.DistanceSquared);
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		EntityID = sn.DoEntityID(EntityID);
		WorldLocation = sn.DoVector2(WorldLocation);
		DistanceSquared = sn.DoDouble(DistanceSquared);
		Arrived = sn.DoBool(Arrived);
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
