using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI.Pathfinding;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.InGameEvents;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.InGameEvents.SpecialEvents;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems;
using UWGame.SimSide.Systems.TimeSlicing;

namespace UWGame.SimSide.IngameEvents;

public class EventManager : ICyclable, ILookUp<ICyclable, CyclableID>, ISnapshot
{
	private enum Phase
	{
		GlobalConditions,
		EventActions,
		PlayerEntityDeaths,
		GroupMeetings
	}

	private Phase phase;

	private Regulator regulator;

	private HighResolutionTime timer;

	private const int eventActionsPerCycle = 5;

	private SleepyUpdater<PolledEvent> polledEvents = new SleepyUpdater<PolledEvent>(Module.Sim, staggerUpdates: true);

	private List<PolledEvent> snapshotPolledEvents;

	private SleepyUpdater<EventAction> eventActions = new SleepyUpdater<EventAction>(Module.Sim);

	private List<EventAction> snapshotEventActions;

	private Dictionary<ActionSetType, int> actionSetFirings = new Dictionary<ActionSetType, int>();

	private UnhappinessGroupMeetingEvent unhappinessGroupMeetingEvent;

	private Funeral funeral;

	public static double totalComputationAllInstancesInSeconds;

	private CyclableID id = CyclableID.Invalid;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsPaused { get; set; }

	public double StartedOnTimeInSeconds { get; set; }

	public double TotalComputationAllInstancesInSeconds
	{
		get
		{
			return totalComputationAllInstancesInSeconds;
		}
		set
		{
			totalComputationAllInstancesInSeconds = value;
		}
	}

	public double ComputationTimeSpentInSeconds { get; set; }

	public double? UpdateInterval => 0.08;

	public CyclableID ID
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

	public bool UnregisterBeforeSnapshot => false;

	public bool IsSnapshotted { get; set; }

	public EventManager()
	{
	}

	public EventManager(Site site)
	{
		if (!Snapshotter.IsSnapshotting)
		{
			AddToLookup();
			CreateRegulators();
			if (site.IsPlaySite)
			{
				unhappinessGroupMeetingEvent = new UnhappinessGroupMeetingEvent();
				funeral = new Funeral();
			}
		}
	}

	private void CreateRegulators()
	{
		regulator = new Regulator(The.Sim.GameplayRandomGenerator, 1.0 / UpdateInterval.Value, "EventManager");
		timer = new HighResolutionTime();
	}

	public void RemovePolledEvent(PolledEventType type, EntityID? sourceEntity = null, ExpeditionID? sourceExpedition = null, AllegianceID? sourceAllegiance = null)
	{
		PolledEvent polledEvent = GetEvent(type, sourceEntity, sourceExpedition, sourceAllegiance);
		if (polledEvent != null)
		{
			RemovePolledEvent(polledEvent);
		}
	}

	private PolledEvent GetEvent(PolledEventType type, EntityID? sourceEntity = null, ExpeditionID? sourceExpedition = null, AllegianceID? sourceAllegiance = null)
	{
		PolledEvent result = null;
		if (sourceEntity.HasValue)
		{
			result = polledEvents.GetItem((PolledEvent p) => p.PolledEventType == type && p.SourceEntity == sourceEntity);
		}
		else if (sourceExpedition.HasValue)
		{
			result = polledEvents.GetItem((PolledEvent p) => p.PolledEventType == type && p.SourceExpedition == sourceExpedition);
		}
		else if (sourceAllegiance.HasValue)
		{
			result = polledEvents.GetItem((PolledEvent p) => p.PolledEventType == type && p.SourceAllegiance == sourceAllegiance);
		}
		return result;
	}

	public PolledEvent AddPolledEvent(string eventKey, EntityID? sourceEntity = null, ExpeditionID? sourceExpedition = null, AllegianceID? sourceAllegiance = null)
	{
		PolledEventType type = GameData.Instance.AllPolledEvents[eventKey];
		if (GetEvent(type, sourceEntity, sourceExpedition, sourceAllegiance) == null)
		{
			PolledEvent polledEvent = new PolledEvent(type, sourceEntity, sourceExpedition);
			AddPolledEventToSleepyUpdater(polledEvent);
			return polledEvent;
		}
		return null;
	}

	private void AddPolledEventToSleepyUpdater(PolledEvent globalCondition, bool keepExistingTimepoint = false)
	{
		bool value = false;
		if (globalCondition.PolledEventType.AllowRandomTimeOffset)
		{
			value = true;
		}
		polledEvents.Add(globalCondition, value, keepExistingTimepoint);
	}

	public bool IsExpended(ActionSetType actionSet)
	{
		if (actionSet.MaxFirings.HasValue)
		{
			if (!actionSetFirings.TryGetValue(actionSet, out var value))
			{
				value = 0;
			}
			if (value >= actionSet.MaxFirings.Value)
			{
				return true;
			}
		}
		return false;
	}

	public void IncreaseFirings(ActionSetType actionSet)
	{
		if (actionSetFirings.TryGetValue(actionSet, out var value))
		{
			value++;
			actionSetFirings[actionSet] = value;
		}
		else
		{
			value = 1;
			actionSetFirings.Add(actionSet, value);
		}
	}

	public void RemoveEventAction(EventAction eventAction)
	{
		eventActions.Remove(eventAction);
	}

	public void RemovePolledEvent(PolledEvent polledEvent)
	{
		polledEvents.Remove(polledEvent);
	}

	public void Update(GameTime gameTime)
	{
		if (!The.Sim.CycleManager.IsRegistered(this))
		{
			double millisecondsSinceLastReady = 0.0;
			if (regulator.IsReady(ref millisecondsSinceLastReady))
			{
				phase = Phase.GlobalConditions;
				The.Sim.CycleManager.Register(this, CycleManager.Priority.Medium);
			}
		}
	}

	public void AddEventForLaterExecution(EventAction eventAction)
	{
		eventActions.Add(eventAction);
	}

	public void ClearEvents()
	{
		polledEvents = new SleepyUpdater<PolledEvent>(Module.Sim, staggerUpdates: true);
	}

	public void PlayerEntityHasDied(Entity deadEntity, Entity carcassEntity, CauseOfDeath? causeOfDeath)
	{
		if (funeral != null)
		{
			funeral.PlayerEntityHasDied(deadEntity, carcassEntity, causeOfDeath);
		}
	}

	public void GetCurrentEvents(StringBuilder description)
	{
		description.AppendLine("Global (polled) conditions:");
		polledEvents.IterateItems(delegate(PolledEvent g)
		{
			description.AppendLine(g.TimePointInSeconds.Value + ": " + g.PolledEventType.KeyName);
		});
		description.AppendLine();
		description.AppendLine("Event actions:");
		eventActions.IterateItems(delegate(EventAction e)
		{
			description.AppendLine(e.TimePointInSeconds.Value + ": " + e.EventActionType.ToString());
		});
	}

	public CyclableID GetUniqueID()
	{
		return Cyclable.GetUniqueID();
	}

	public CyclableID SnapshotID(Snapshotter sn, CyclableID id)
	{
		return sn.DoEnum(id);
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != CyclableID.Invalid)
		{
			LookUp<ICyclable, CyclableID>.Add(ID, this);
		}
	}

	public void RemoveIDEntry()
	{
		LookUp<ICyclable, CyclableID>.Remove(this);
	}

	public void SetInvalid()
	{
		id = CyclableID.Invalid;
	}

	public void ResetIDCounter()
	{
	}

	void ILookUp<ICyclable, CyclableID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUp<ICyclable, CyclableID>.Create();
	}

	public void PrintInfo(StringBuilder text)
	{
		text.Append($"EventManager {ID}:");
	}

	public bool CycleOnce()
	{
		bool result = false;
		_ = phase;
		timer.Start();
		switch (phase)
		{
		case Phase.GlobalConditions:
			polledEvents.Update(The.Sim.GameTime);
			if (The.Sim == null)
			{
				result = true;
			}
			else
			{
				phase = Phase.EventActions;
			}
			break;
		case Phase.EventActions:
			eventActions.Update(The.Sim.GameTime);
			phase = Phase.PlayerEntityDeaths;
			break;
		case Phase.PlayerEntityDeaths:
			if (funeral != null)
			{
				funeral.Update();
			}
			phase = Phase.GroupMeetings;
			break;
		case Phase.GroupMeetings:
			if (unhappinessGroupMeetingEvent != null)
			{
				unhappinessGroupMeetingEvent.Update();
			}
			phase = Phase.GlobalConditions;
			result = true;
			break;
		default:
			result = true;
			break;
		}
		timer.GetTime();
		_ = 0.002;
		return result;
	}

	public bool PlayerAllegianceIsSleepingOrCollapsed()
	{
		return The.Sim.PlaySite.PlayerAllegiance.Members.All((Entity e) => !e.IsAwakeAndActive());
	}

	public bool PlayerAllegianceIsAttacking()
	{
		return The.Sim.PlaySite.PlayerAllegiance.Members.Any((Entity e) => e.IsAttacking());
	}

	public bool PlayerAllegianceIsUnderThreat()
	{
		return The.Sim.PlaySite.PlayerAllegiance.IsUnderThreat();
	}

	public void PrintGlobalConditions(StringBuilder description)
	{
		description.AppendLine("sleeping or collapsed: " + PlayerAllegianceIsSleepingOrCollapsed());
		description.AppendLine("attacking: " + PlayerAllegianceIsAttacking());
		description.AppendLine("under threat: " + PlayerAllegianceIsUnderThreat());
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		id = SnapshotID(sn, id);
		IsPaused = sn.DoBool(IsPaused);
		phase = sn.DoEnum(phase);
		actionSetFirings = sn.DoDictionary(actionSetFirings);
		unhappinessGroupMeetingEvent = (UnhappinessGroupMeetingEvent)sn.DoISnapshot(unhappinessGroupMeetingEvent);
		funeral = (Funeral)sn.DoISnapshot(funeral);
		if (sn.mode != Snapshotter.Mode.Load)
		{
			snapshotEventActions = new List<EventAction>();
			snapshotPolledEvents = new List<PolledEvent>();
			eventActions.IterateItems(delegate(EventAction e)
			{
				snapshotEventActions.Add(e);
			});
			polledEvents.IterateItems(delegate(PolledEvent e)
			{
				snapshotPolledEvents.Add(e);
			});
		}
		snapshotEventActions = sn.DoList(snapshotEventActions);
		snapshotPolledEvents = sn.DoList(snapshotPolledEvents);
		sn.Ignore(totalComputationAllInstancesInSeconds);
		sn.Ignore(ComputationTimeSpentInSeconds);
		sn.Ignore(StartedOnTimeInSeconds);
		sn.Ignore(regulator);
		sn.Ignore(polledEvents);
		sn.Ignore(eventActions);
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
		foreach (PolledEvent snapshotPolledEvent in snapshotPolledEvents)
		{
			snapshotPolledEvent.LoadPostProcess(sn);
			AddPolledEventToSleepyUpdater(snapshotPolledEvent, keepExistingTimepoint: true);
		}
		snapshotPolledEvents.Clear();
		foreach (EventAction snapshotEventAction in snapshotEventActions)
		{
			eventActions.Add(snapshotEventAction, null, keepExistingTimepoint: true);
		}
		snapshotEventActions.Clear();
		if (unhappinessGroupMeetingEvent != null)
		{
			unhappinessGroupMeetingEvent.LoadPostProcess(sn);
		}
		if (funeral != null)
		{
			funeral.LoadPostProcess(sn);
		}
		CreateRegulators();
	}
}
