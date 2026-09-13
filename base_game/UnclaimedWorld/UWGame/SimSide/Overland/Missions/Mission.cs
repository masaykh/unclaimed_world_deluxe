using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Communication;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Overland.Locations;
using UWGame.SimSide.Overland.Missions.Templates;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Overland.Missions;

public class Mission : ISnapshot, ILookUp<Mission, MissionID>, ICommunicates
{
	public JobID MissionJob;

	public MissionTemplate MissionTemplate;

	private MissionTemplateID snapshotMissionTemplate;

	public MissionStop StartMissionStop;

	private MissionStopID snapshotStartMissionStop;

	public Transportation Transportation;

	public List<Contract> Contracts = new List<Contract>();

	public MissionStop CurrentMissionStop;

	private MissionStopID snapshotCurrentMissionStop;

	private GeodeticCoordinate? tempCoords;

	private bool tempCanCommunicate;

	private MissionID id = MissionID.Invalid;

	private static MissionID IDCounter = MissionID.First;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public TravelLocation? CurrentLocation
	{
		get
		{
			if (CurrentMissionStop.TravelAction != null && CurrentMissionStop.TravelAction.Progress == TravelAction.OverlandProgress.BetweenSites)
			{
				return null;
			}
			return CurrentMissionStop.MissionStopTemplate.TravelLocation;
		}
	}

	public Site Site
	{
		get
		{
			if (CurrentLocation.HasValue)
			{
				SharedKnowledge sharedKnowledge = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)MissionTemplate.Allegiance).SharedKnowledge;
				if (CurrentLocation.Value.ResolveLocation(sharedKnowledge, out var site, out var _, out var _, out var _))
				{
					return site;
				}
			}
			return null;
		}
	}

	public GeodeticCoordinate? Coords
	{
		get
		{
			Site site = Site;
			if (site != null)
			{
				return site.Coords;
			}
			MissionJob missionJob = (MissionJob)LookUp<Job, JobID>.FindByID(MissionJob);
			tempCoords = null;
			if (missionJob.Vehicles != null)
			{
				missionJob.IterateVehicles(delegate(Entity e)
				{
					GetCoords(e);
				});
			}
			if (!tempCoords.HasValue)
			{
				missionJob.TakenBy.Iterate(delegate(Entity e)
				{
					GetCoords(e);
				});
			}
			return tempCoords;
		}
	}

	public MissionID ID
	{
		get
		{
			return id;
		}
		private set
		{
			id = value;
		}
	}

	public int LoadPostProcessOrder => 0;

	public bool IsSnapshotted { get; set; }

	private void CreateRegulators()
	{
	}

	public string GetMissionMarker()
	{
		string text = "hiker_map_icon";
		if (MissionTemplate.TransportationType.Vehicles != null && MissionTemplate.TransportationType.Vehicles != null && MissionTemplate.TransportationType.Vehicles.Count > 0)
		{
			text = GameData.Instance.AllEntityTypes[MissionTemplate.TransportationType.Vehicles[0].First].WorldMapIcon ?? text;
		}
		return text;
	}

	private void GetCoords(Entity entity)
	{
		GeodeticCoordinate? coords = entity.Coords;
		if (coords.HasValue)
		{
			tempCoords = coords;
		}
	}

	public string GetTooltip()
	{
		StringBuilder stringBuilder = new StringBuilder();
		EntityType mainTransportation = GetMainTransportation();
		stringBuilder.Append("Transportation: ");
		if (mainTransportation != null)
		{
			Common.Append(stringBuilder, mainTransportation.Name);
		}
		else
		{
			Common.Append(stringBuilder, "On foot");
		}
		return stringBuilder.ToString();
	}

	public bool CanCommunicate(CommunicationMethod method, double distance)
	{
		Allegiance allegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)MissionTemplate.Allegiance);
		if (allegiance != null && CurrentLocation.HasValue && CurrentLocation.Value.ResolveLocation(allegiance.SharedKnowledge, out var _, out var allegiance2, out var _, out var _) && allegiance2 != null && allegiance2.CanCommunicate(method, distance))
		{
			return true;
		}
		MissionJob missionJob = (MissionJob)LookUp<Job, JobID>.FindByID(MissionJob);
		tempCanCommunicate = false;
		missionJob.IterateVehicles(delegate(Entity e)
		{
			CanCommunicate(e, method, distance);
		});
		if (tempCanCommunicate)
		{
			return true;
		}
		for (int num = 0; num < missionJob.TakenBy.Count; num++)
		{
			if (missionJob.TakenBy.Get(num).CanCommunicate(method, distance))
			{
				return true;
			}
		}
		return false;
	}

	private void CanCommunicate(Entity entity, CommunicationMethod method, double distance)
	{
		if (entity.CanCommunicate(method, distance))
		{
			tempCanCommunicate = true;
		}
	}

	public Mission(EntityGroup entityGroup, MissionTemplate missionType, Dictionary<EntityType, List<EntityID>> vehicles)
	{
		MissionTemplate = missionType;
		AddToLookup();
		StartMissionStop = new MissionStop(this, missionType.StartMissionStopTemplate);
		Transportation = new Transportation(this);
		Allegiance allegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)missionType.Allegiance);
		MissionJob missionJob = new MissionJob(entityGroup, vehicles);
		MissionJob = missionJob.ID;
		CurrentMissionStop = StartMissionStop;
		allegiance.AddMission(this);
	}

	public Mission()
	{
	}

	public void StartMission()
	{
		Transportation.StartMission();
		StartMissionStop.StartMission();
	}

	public bool IsOnPlaySite()
	{
		return GetVehicleOrAgentToLoad().IsOnPlaySite();
	}

	public void Destroy()
	{
		RemoveIDEntry();
		LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)MissionTemplate.Allegiance).RemoveMission(this);
		LookUp<Job, JobID>.FindByID(MissionJob).Destroy(cancelTakers: true);
	}

	public void Abort()
	{
		RefundOrders();
		FireAbortEvents();
		RemoveAllStopsAndActionsBeforeReturnAction();
	}

	private void RefundOrders()
	{
		MissionJob job = (MissionJob)LookUp<Job, JobID>.FindByID(MissionJob);
		if (!job.ResolveOwner(out var owner))
		{
			return;
		}
		Entity vehicleOrAgentToLoad = GetVehicleOrAgentToLoad();
		List<Entity> carriedCargo = vehicleOrAgentToLoad.Contains.GetContainedItemsList((Entity e) => e.AssignedToJob == job.ID);
		foreach (Contract contract in Contracts)
		{
			if (LookUpOwners.FindByID((OwnerID)contract.ContractTemplate.BuyerID) != owner.Parent && contract.TransferredEntities.All((EntityID e) => carriedCargo.Contains(Entity.FindByID(e))))
			{
				contract.Revert();
			}
		}
	}

	private void FireAbortEvents()
	{
		if (LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)MissionTemplate.Allegiance).AllegianceType == AllegianceType.Player)
		{
			GameData.Instance.AllegianceEvents.TryGetValue(AllegianceEvents.TransportToPlayerAborted, out var value);
			Goal.FireEventActions(null, null, value);
		}
	}

	private void RemoveAllStopsAndActionsBeforeReturnAction()
	{
		if (CurrentMissionStop.Actions.Count > 0)
		{
			CurrentMissionStop.Actions.Clear();
			CurrentMissionStop.MissionStopTemplate.Actions.Clear();
		}
		MissionStop end = CurrentMissionStop.GetEnd();
		if (end != CurrentMissionStop)
		{
			end.MissionStopTemplate.Actions.Clear();
			end.MissionStopTemplate.Actions.Enqueue(new UnloadActionTemplate(end.MissionStopTemplate, allowDeleting: false));
			end.MissionStopTemplate.Actions.Enqueue(new DisembarkActionTemplate(end.MissionStopTemplate, allowDeleting: false));
			CurrentMissionStop.MissionStopTemplate.TravelAction = new TravelActionTemplate(CurrentMissionStop.MissionStopTemplate, end.MissionStopTemplate);
			CurrentMissionStop.TravelAction = new TravelAction(this, CurrentMissionStop.MissionStopTemplate.TravelAction);
			CurrentMissionStop.MissionStopTemplate.RecalculateNumbers();
			CurrentMissionStop.TravelAction.Start();
		}
	}

	private void RecalculateNumbers()
	{
		StartMissionStop.MissionStopTemplate.SetNumber(0);
	}

	public EntityType GetMainTransportation()
	{
		if (MissionTemplate.TransportationType != null)
		{
			return MissionTemplate.TransportationType.GetMainTransportation();
		}
		return null;
	}

	public bool GetNextStopAndETA(out TravelLocation? nextStop, out DateAndTime.TimeDateYear? eta)
	{
		nextStop = null;
		eta = null;
		if (CurrentMissionStop.TravelAction != null)
		{
			nextStop = CurrentMissionStop.TravelAction.ToMissionStop.MissionStopTemplate.TravelLocation;
			eta = CurrentMissionStop.TravelAction.GetETA();
			return true;
		}
		return false;
	}

	public Entity GetVehicleToLoad()
	{
		MissionJob missionJob = (MissionJob)LookUp<Job, JobID>.FindByID(MissionJob);
		if (missionJob.Vehicles != null && missionJob.Vehicles.Count > 0)
		{
			return Entity.FindByID(missionJob.Vehicles.First().Value[0]);
		}
		return null;
	}

	public Entity GetVehicleOrAgentToLoad()
	{
		MissionJob missionJob = (MissionJob)LookUp<Job, JobID>.FindByID(MissionJob);
		if (missionJob.Vehicles.Count > 0)
		{
			return Entity.FindByID(missionJob.Vehicles.First().Value[0]);
		}
		return missionJob.TakenBy.Get(0);
	}

	public void Update(GameTime gameTime)
	{
		CurrentMissionStop.Update(gameTime);
		Transportation.Update(gameTime);
		if (CurrentMissionStop.Actions.Count == 0 && CurrentMissionStop.TravelAction == null)
		{
			Destroy();
		}
	}

	public MissionID GetUniqueID()
	{
		IDCounter++;
		if ((ulong)IDCounter >= ulong.MaxValue)
		{
			throw new Exception("Astounding, MissionID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != MissionID.Invalid)
		{
			LookUp<Mission, MissionID>.Add(ID, this);
		}
	}

	public void SetInvalid()
	{
		id = MissionID.Invalid;
	}

	public void RemoveIDEntry()
	{
		LookUp<Mission, MissionID>.Remove(this);
	}

	void ILookUp<Mission, MissionID>.ResetIDCounter()
	{
	}

	public static void ResetIDCounter()
	{
		IDCounter = MissionID.First;
	}

	void ILookUp<Mission, MissionID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUp<Mission, MissionID>.Create();
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		id = sn.DoEnum(id);
		IDCounter = sn.DoEnum(IDCounter);
		MissionJob = sn.DoEnum(MissionJob);
		snapshotStartMissionStop = sn.SnapshotID<MissionStop, MissionStopID>(StartMissionStop).Value;
		snapshotCurrentMissionStop = sn.SnapshotID<MissionStop, MissionStopID>(CurrentMissionStop).Value;
		snapshotMissionTemplate = sn.SnapshotID<MissionTemplate, MissionTemplateID>(MissionTemplate).Value;
		Contracts = sn.DoList(Contracts);
		Transportation = (Transportation)sn.DoISnapshot(Transportation);
		sn.Ignore(tempCoords);
		sn.Ignore(tempCanCommunicate);
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
		StartMissionStop = LookUp<MissionStop, MissionStopID>.FindByID(snapshotStartMissionStop);
		CurrentMissionStop = LookUp<MissionStop, MissionStopID>.FindByID(snapshotCurrentMissionStop);
		MissionTemplate = LookUp<MissionTemplate, MissionTemplateID>.FindByID(snapshotMissionTemplate);
		StartMissionStop.SetParentPostLoad(this);
		CurrentMissionStop.SetParentPostLoad(this);
		if (Transportation != null)
		{
			Transportation.LoadPostProcess(sn);
			Transportation.SetParentPostLoad(this);
		}
		foreach (Contract contract in Contracts)
		{
			contract.LoadPostProcess(sn);
		}
	}
}
