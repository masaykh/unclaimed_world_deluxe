using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

internal class GoalEmigrate : CompositeGoal, ITopLevelGoal
{
	private AllegianceID newAllegiance;

	private ExpeditionID newExpedition;

	private SiteID newSite;

	private EntityID terminalToLeaveFrom;

	private RouteID? routeID;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public double TimeSpentInTopLevelGoal { get; set; }

	public GoalEmigrate(Entity owner, SiteID newSite, AllegianceID newAllegiance, ExpeditionID newExpedition, EntityID terminalToLeaveFrom, RouteID? routeToUse, List<EntityGroupID> ownersOfVehicles)
		: base(owner)
	{
		this.newSite = newSite;
		this.newAllegiance = newAllegiance;
		this.newExpedition = newExpedition;
		this.terminalToLeaveFrom = terminalToLeaveFrom;
		routeID = routeToUse;
		base.ownersOfVehicles = ownersOfVehicles;
	}

	public GoalEmigrate()
	{
	}

	protected override bool ArePreconditionsOK()
	{
		if (EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(terminalToLeaveFrom, out var _)))
		{
			return false;
		}
		return true;
	}

	protected override void Activate()
	{
		base.Status = Status.Active;
		RemoveAllSubgoals();
		if (!EntityResultCausesFailedGoal(entityIntelligence.GetKnownData(terminalToLeaveFrom, out var data)))
		{
			DropAllCarriedItems();
			AddSubgoal(new GoalMoveToPosition(entity, ownersOfVehicles, data));
			AddSubgoal(new GoalDoEmigrate(entity, newSite, newAllegiance, newExpedition, routeID));
			AddSubgoal(new GoalWait(entity, 4.0));
			entity.EntityType.IntelligenceType.EventActions.TryGetValue(AgentActionHooks.StartsToLeaveAllegiance, out var value);
			Goal.FireEventActions(entity, null, value);
		}
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		if (!preconditionsRegulator.IsReady() || ArePreconditionsOK())
		{
			base.Status = ProcessSubgoals(elapsed);
		}
	}

	public override string GetStatus()
	{
		return "Leaving site";
	}

	public double ScoreGoal()
	{
		return entityIntelligence.GetCurrentGoalUtility().Value;
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		newAllegiance = sn.DoEnum(newAllegiance);
		newSite = sn.DoEnum(newSite);
		newExpedition = sn.DoEnum(newExpedition);
		terminalToLeaveFrom = sn.DoEnum(terminalToLeaveFrom);
		routeID = sn.DoEnumNullable(routeID);
		TimeSpentInTopLevelGoal = sn.DoDouble(TimeSpentInTopLevelGoal);
		return this;
	}

	public override Snapshotter.Version DoVersion(Snapshotter sn)
	{
		base.DoVersion(sn);
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}
}
