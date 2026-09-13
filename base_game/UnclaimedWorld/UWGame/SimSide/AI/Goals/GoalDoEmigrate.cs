using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Overland.Missions;
using UWGame.SimSide.Overland.Missions.Templates;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

public class GoalDoEmigrate : Goal
{
	private AllegianceID newAllegiance;

	private ExpeditionID newExpedition;

	private SiteID newSite;

	private RouteID? routeID;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public GoalDoEmigrate(Entity owner, SiteID newSite, AllegianceID newAllegiance, ExpeditionID newExpedition, RouteID? routeID)
		: base(owner)
	{
		this.newSite = newSite;
		this.newAllegiance = newAllegiance;
		this.newExpedition = newExpedition;
		this.routeID = routeID;
	}

	public GoalDoEmigrate()
	{
	}

	protected override void Activate()
	{
		base.Status = Status.Active;
		entity.EntityType.IntelligenceType.EventActions.TryGetValue(AgentActionHooks.LeavesSiteForNewAllegiance, out var value);
		Goal.FireEventActions(entity, null, value);
		StartMission();
		base.Status = Status.Completed;
	}

	public override bool IsSame(Job job)
	{
		return false;
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
	}

	private void StartMission()
	{
		OwnerID iD = ((ILookUp<IOwner, OwnerID>)entityIntelligence.CurrentExpedition).ID;
		MissionTemplate missionTemplate = new MissionTemplate(entityIntelligence.Allegiance.ID, iD, createID: false);
		missionTemplate.TransportationType = new TransportationTemplate();
		TravelLocation travelLocation = new TravelLocation(entityIntelligence.Allegiance, (long)entityIntelligence.CurrentExpedition.ID, null);
		MissionStopTemplate missionStopTemplate = new MissionStopTemplate(createID: false);
		missionStopTemplate.TravelLocation = travelLocation;
		missionTemplate.StartMissionStopTemplate = missionStopTemplate;
		TravelLocation travelLocation2 = new TravelLocation((long)newSite, (long)newAllegiance, (long)newExpedition, null);
		MissionStopTemplate missionStopTemplate2 = new MissionStopTemplate(createID: false);
		missionStopTemplate2.TravelLocation = travelLocation2;
		TravelActionTemplate travelActionTemplate = new TravelActionTemplate(missionTemplate.StartMissionStopTemplate, missionStopTemplate2);
		missionTemplate.StartMissionStopTemplate.TravelAction = travelActionTemplate;
		Route route = null;
		if (routeID.HasValue)
		{
			route = LookUp<Route, RouteID>.FindByID(routeID);
			travelActionTemplate.SetRoute(route, useAirRoute: false);
		}
		else
		{
			travelActionTemplate.SetRoute(null, useAirRoute: true);
		}
		List<EntityID> list = new List<EntityID>();
		list.Add(entity.ID);
		missionStopTemplate.Actions.Enqueue(new EmbarkActionTemplate(missionStopTemplate, list, allowDeleting: false));
		missionStopTemplate2.Actions.Enqueue(new UnloadActionTemplate(missionStopTemplate2, allowDeleting: false));
		missionStopTemplate2.Actions.Enqueue(new DisembarkActionTemplate(missionStopTemplate2, allowDeleting: false));
		missionTemplate.AssignIDs();
		Mission mission = new Mission(entityIntelligence.CurrentExpedition.OwnedEntities, missionTemplate, null);
		LookUp<Job, JobID>.FindByID(mission.MissionJob).TakeJob(entity);
		mission.StartMission();
		entityIntelligence.Memory.SetEmigrateDecision(null, entity);
	}

	public override Snapshotter.Version DoVersion(Snapshotter sn)
	{
		base.DoVersion(sn);
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		newAllegiance = sn.DoEnum(newAllegiance);
		newSite = sn.DoEnum(newSite);
		newExpedition = sn.DoEnum(newExpedition);
		routeID = sn.DoEnumNullable(routeID);
		return this;
	}
}
