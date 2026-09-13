using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Communication;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Overland.Locations;
using UWGame.SimSide.Overland.Missions.Templates;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Overland.Missions;

public class TravelAction : MissionAction
{
	public enum OverlandProgress
	{
		StartSite,
		BetweenSites,
		DestinationSite
	}

	public TravelActionTemplate TravelActionTemplate;

	private MissionActionTemplateID snapshotActionTemplateID;

	public MissionStop ToMissionStop;

	private MissionStopID snapshotToMissionStop;

	private GeodeticCoordinate startCoords;

	private OverlandProgress progress;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public double DistanceProgress { get; private set; }

	public OverlandProgress Progress
	{
		get
		{
			return progress;
		}
		private set
		{
			progress = value;
		}
	}

	public TravelAction()
	{
	}

	public TravelAction(Mission parent, TravelActionTemplate template)
		: base(parent)
	{
		TravelActionTemplate = template;
		if (template.ToMissionStop != null)
		{
			ToMissionStop = new MissionStop(parent, template.ToMissionStop);
		}
	}

	public TravelAction(Mission parent, MissionStop to)
		: base(parent)
	{
		ToMissionStop = to;
	}

	public DateAndTime.TimeDateYear GetETA()
	{
		double num = TravelActionTemplate.Distance - DistanceProgress;
		MissionJob missionJob = (MissionJob)LookUp<Job, JobID>.FindByID(parent.MissionJob);
		double timeInDays = num / missionJob.GetRealizedTravelSpeed();
		DateAndTime.TimeDateYear currentTimeDateYear = The.Sim.DateAndTime.CurrentTimeDateYear;
		currentTimeDateYear.AddTime(timeInDays);
		return currentTimeDateYear;
	}

	public override void StartMission()
	{
		ToMissionStop.StartMission();
	}

	public override void Destroy()
	{
		base.Destroy();
		ToMissionStop.Destroy();
	}

	private void SetCoords(GeodeticCoordinate coords)
	{
		((MissionJob)LookUp<Job, JobID>.FindByID(parent.MissionJob)).IterateVehicles(delegate(Entity e)
		{
			e.Coords = coords;
		});
	}

	private void LeaveSite()
	{
		Start();
		NotifyNextStopAllegiance();
	}

	private void NotifyNextStopAllegiance()
	{
		string text = GetETA().ToString();
		Allegiance allegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)TravelActionTemplate.ToMissionStop.TravelLocation.AllegianceID.Value);
		Allegiance allegiance2 = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)parent.MissionTemplate.Allegiance);
		if (allegiance != null && Communicates.IsInCommunicationRange(allegiance, allegiance2, out var _) && allegiance2 != null && parent.StartMissionStop.MissionStopTemplate.TravelLocation.ResolveLocation(allegiance2.SharedKnowledge, out var site, out var _, out var _, out var _))
		{
			The.Client.AddLogEvent(allegiance, The.Client.Log.GeneralEvent, null, "A transport from " + site.Name + " is on its way. ETA: " + text);
		}
	}

	public void Start()
	{
		Progress = OverlandProgress.BetweenSites;
		GeodeticCoordinate coords = parent.Coords.Value;
		startCoords = coords;
		IterateMembers(delegate(Entity e)
		{
			SetLocationOfMember(e, null, null, coords);
		}, null, null);
	}

	private void SetLocationOfMember(Entity entity, Site newSite, Vector3? playSiteLocation = null, GeodeticCoordinate? coords = null)
	{
		Site site = entity.Site;
		if (newSite == null && site != null && site.IsPlaySite)
		{
			entity.PlaceEntityOffSite(coords.Value);
			return;
		}
		entity.Site = newSite;
		entity.Coords = coords;
		entity.Location = playSiteLocation;
	}

	private void IterateMembers(Action<Entity> topLevelContainerFunction, Action<Entity> containedFunction, Action<Entity> allEntityFunction)
	{
		MissionJob missionJob = (MissionJob)LookUp<Job, JobID>.FindByID(parent.MissionJob);
		if (allEntityFunction != null)
		{
			missionJob.IterateVehicles(allEntityFunction);
			missionJob.IterateVehicleContents(allEntityFunction);
		}
		if (topLevelContainerFunction != null)
		{
			missionJob.IterateVehicles(topLevelContainerFunction);
		}
		if (containedFunction != null)
		{
			missionJob.IterateVehicleContents(containedFunction);
		}
		for (int i = 0; i < missionJob.TakenBy.Count; i++)
		{
			Entity entity = missionJob.TakenBy.Get(i);
			if (entity.ContainedBy.HasValue)
			{
				continue;
			}
			topLevelContainerFunction?.Invoke(entity);
			if (allEntityFunction != null)
			{
				allEntityFunction(entity);
			}
			if (entity.Contains == null)
			{
				continue;
			}
			if (containedFunction != null)
			{
				entity.Contains.IterateContained(delegate(Entity e)
				{
					containedFunction(e);
				});
			}
			if (allEntityFunction != null)
			{
				entity.Contains.IterateContained(delegate(Entity e)
				{
					allEntityFunction(e);
				});
			}
		}
	}

	private void SetPassengerGoal(Entity agent)
	{
		if (agent.EntityType.IntelligenceType != null)
		{
			agent.Intelligence.SetTopLevelGoal(new GoalWaitAsPassenger(agent), 1.0);
		}
	}

	private void ArriveAtSite()
	{
		Progress = OverlandProgress.DestinationSite;
		TravelLocation travelLocation = TravelActionTemplate.ToMissionStop.TravelLocation;
		SharedKnowledge sharedKnowledge = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)parent.MissionTemplate.Allegiance).SharedKnowledge;
		if (travelLocation.ResolveLocation(sharedKnowledge, out var site, out var _, out var expedition, out var _))
		{
			parent.CurrentMissionStop = ToMissionStop;
			if (site.IsPlaySite)
			{
				if (LocateLandingArea(out var dockingLocation))
				{
					TransferToPlaySite(site, dockingLocation.Value, expedition);
				}
				else
				{
					HandleFailedTravelAction();
				}
			}
			else
			{
				TransferToOtherSite(site, expedition);
			}
		}
		else
		{
			HandleFailedTravelAction();
		}
	}

	private void TransferToPlaySite(Site site, Vector3 location, Expedition expedition)
	{
		IterateMembers(delegate(Entity e)
		{
			e.TransferToPlaySite(location, null, null, expedition);
		}, delegate(Entity e)
		{
			e.TransferToPlaySite(null, null, null, expedition);
		}, null);
		((MissionJob)LookUp<Job, JobID>.FindByID(parent.MissionJob)).IterateVehicleContents(delegate(Entity e)
		{
			SetPassengerGoal(e);
		});
	}

	private void TransferToOtherSite(Site site, Expedition expedition)
	{
		IterateMembers(delegate(Entity e)
		{
			e.TransferToOtherSite(site, null, expedition);
		}, delegate(Entity e)
		{
			e.TransferToOtherSite(site, null, expedition);
		}, null);
	}

	private void HandleFailedTravelAction()
	{
		parent.Abort();
	}

	private bool LocateLandingArea(out Vector3? dockingLocation)
	{
		TravelLocation travelLocation = TravelActionTemplate.ToMissionStop.TravelLocation;
		dockingLocation = null;
		Allegiance allegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)travelLocation.AllegianceID.Value);
		if (allegiance == null)
		{
			return false;
		}
		if (TravelActionTemplate.ToMissionStop.TravelLocation.ExpeditionID.HasValue)
		{
			Expedition expedition = LookUp<Expedition, ExpeditionID>.FindByID((ExpeditionID)travelLocation.ExpeditionID.Value);
			if (expedition == null)
			{
				return false;
			}
			if (travelLocation.TerminalEntityID.HasValue)
			{
				Entity entity = Entity.FindByID((EntityID)travelLocation.TerminalEntityID.Value);
				if (entity != null)
				{
					dockingLocation = GetTerminalLocation(entity);
					return true;
				}
				return false;
			}
			Vector3? vector = LocateLandingAreaInExpedition(expedition);
		}
		else
		{
			foreach (Expedition expedition2 in allegiance.Expeditions)
			{
				Vector3? vector = LocateLandingAreaInExpedition(expedition2);
				if (vector.HasValue)
				{
					dockingLocation = vector.Value;
					return true;
				}
			}
		}
		dockingLocation = EntityGroup.GetFreeGroundLocation(allegiance.Expeditions[0]);
		return true;
	}

	private Vector3? LocateLandingAreaInExpedition(Expedition expedition)
	{
		Allegiance allegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)parent.MissionTemplate.Allegiance);
		VehicleContainerType vehicleContainerType = parent.MissionTemplate.TransportationType.GetMainTransportation().ContainerType as VehicleContainerType;
		TerminalType.TypesOfTerminal value = TerminalType.TypesOfTerminal.Land;
		if (vehicleContainerType != null)
		{
			value = vehicleContainerType.CanUseTerminal ?? TerminalType.TypesOfTerminal.Land;
		}
		List<IKnownEntityData> workingTerminals = expedition.GetWorkingTerminals(allegiance.SharedKnowledge, value);
		if (workingTerminals != null && workingTerminals.Count > 0)
		{
			return GetTerminalLocation(workingTerminals[0]);
		}
		return null;
	}

	private Vector3 GetTerminalLocation(IKnownEntityData terminal)
	{
		return terminal.PlaySiteLocation;
	}

	public override bool Update(GameTime elapsed)
	{
		base.Update(elapsed);
		if (Progress == OverlandProgress.StartSite)
		{
			LeaveSite();
		}
		double num = ((MissionJob)LookUp<Job, JobID>.FindByID(parent.MissionJob)).GetRealizedTravelSpeed() * The.Sim.DateAndTime.DaysPerSecond * elapsed.ElapsedGameTime.TotalSeconds;
		DistanceProgress += num;
		if (DistanceProgress >= TravelActionTemplate.Distance)
		{
			ArriveAtSite();
			return true;
		}
		Allegiance allegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)parent.MissionTemplate.Allegiance);
		if (allegiance != null && ToMissionStop.MissionStopTemplate.TravelLocation.ResolveLocation(allegiance.SharedKnowledge, out var site, out var _, out var _, out var _))
		{
			GeodeticCoordinate currentCoords = DistanceCalculator.GetIntermediatePoint(startCoords, site.Coords, DistanceProgress / TravelActionTemplate.Distance, TravelActionTemplate.Distance, The.Sim.World.WorldRadius);
			IterateMembers(delegate(Entity e)
			{
				SetLocationOfMember(e, null, null, currentCoords);
			}, null, null);
		}
		else
		{
			HandleFailedTravelAction();
		}
		return false;
	}

	public void SetParentPostLoad(Mission parent)
	{
		base.parent = parent;
		ToMissionStop = LookUp<MissionStop, MissionStopID>.FindByID(snapshotToMissionStop);
		ToMissionStop.SetParentPostLoad(parent);
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		DistanceProgress = sn.DoDouble(DistanceProgress);
		progress = sn.DoEnum(progress);
		startCoords = sn.DoGeodeticCoordinate(startCoords);
		snapshotToMissionStop = sn.SnapshotID<MissionStop, MissionStopID>(ToMissionStop).Value;
		if (sn.mode == Snapshotter.Mode.Save && snapshotToMissionStop == (MissionStopID)0uL)
		{
			throw new Exception();
		}
		snapshotActionTemplateID = sn.SnapshotID<MissionActionTemplate, MissionActionTemplateID>(TravelActionTemplate).Value;
		sn.Ignore(TravelActionTemplate);
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
		sn.RegisterLoadPostProcessCall(this);
		TravelActionTemplate = (TravelActionTemplate)LookUp<MissionActionTemplate, MissionActionTemplateID>.FindByID(snapshotActionTemplateID);
	}
}
