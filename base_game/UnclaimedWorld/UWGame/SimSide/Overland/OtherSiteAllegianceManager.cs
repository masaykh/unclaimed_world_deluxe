using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Overland.Missions;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Overland;

public class OtherSiteAllegianceManager : ISnapshot
{
	private Mission transportWaitingToDepart;

	private MissionID? snapshotWaitingTransport;

	public Allegiance Parent;

	private AllegianceID snapshotParent;

	private Regulator missionRegulator;

	private List<EntityID> waitingImmigrants = new List<EntityID>();

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public OtherSiteAllegianceManager()
	{
	}

	public OtherSiteAllegianceManager(Allegiance allegiance)
	{
		Parent = allegiance;
		CreateRegulators();
	}

	private void CreateRegulators()
	{
		missionRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 0.1, "OtherSiteAllegianceManager");
	}

	public void Update(GameTime gameTime)
	{
	}

	private bool IsContractFulfilledForThisInterval()
	{
		return true;
	}

	public void AddEmigrantToQueue(EntityID emigrantID)
	{
		Common.AddToList(ref waitingImmigrants, emigrantID);
	}

	public int GetAmountOfWaitingEmigrants()
	{
		if (waitingImmigrants != null)
		{
			return waitingImmigrants.Count;
		}
		return 0;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		snapshotParent = sn.SnapshotID<Allegiance, AllegianceID>(Parent).Value;
		snapshotWaitingTransport = sn.SnapshotID<Mission, MissionID>(transportWaitingToDepart);
		waitingImmigrants = sn.DoList(waitingImmigrants);
		return this;
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		return version;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		Parent = LookUp<Allegiance, AllegianceID>.FindByID(snapshotParent);
		transportWaitingToDepart = LookUp<Mission, MissionID>.FindByID(snapshotWaitingTransport);
		CreateRegulators();
	}
}
