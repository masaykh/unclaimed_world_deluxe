using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Overland.Missions;

public class Transportation : ISnapshot
{
	public List<EntityID> Supplies;

	public Mission mission;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public Transportation(Mission mission)
	{
		this.mission = mission;
	}

	public Transportation()
	{
	}

	public void SetParentPostLoad(Mission parent)
	{
		mission = parent;
	}

	public void Update(GameTime gameTime)
	{
	}

	public void StartMission()
	{
		if (mission.MissionTemplate.TransportationType.HiredFromOwner.HasValue)
		{
			OwnerID ownerID = (OwnerID)mission.MissionTemplate.OwnerID;
			long value = mission.MissionTemplate.TransportationType.HiredFromOwner.Value;
			IOwner buyer = LookUpOwners.FindByID(ownerID);
			IOwner seller = LookUpOwners.FindByID((OwnerID)value);
			decimal startFee;
			decimal totalDistanceCost;
			decimal costPerKilometer;
			decimal amount = mission.MissionTemplate.ComputeTransportationCost(out startFee, out totalDistanceCost, out costPerKilometer);
			EntityGroup.MakeTradeCreditsTransaction(buyer, seller, amount);
		}
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		sn.Postpone(Supplies);
		sn.Ignore(mission);
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
