using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI.Pathfinding;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems;

namespace UWGame.SimSide.InGameEvents.Actions;

public class EventAction : ISleepingUpdatable, ISnapshot
{
	public ActionSetDataID? ParentID;

	public EventActionType EventActionType;

	public EntityID? TriggeringEntity;

	public EntityID? TargetEntity;

	public IHasExposedProperties PolledEventSource;

	private EntityID? polledEventSourceEntity;

	private ExpeditionID? polledEventSourceExpedition;

	private AllegianceID? polledEventSourceAllegiance;

	public IHasExposedProperties DynamicTarget;

	private EntityID? dynamicTargetEntity;

	private ExpeditionID? dynamicTargetExpedition;

	private AllegianceID? dynamicTargetAllegiance;

	private static HighResolutionTime timer = new HighResolutionTime();

	private double? timePointInSeconds;

	private double? updateInterval;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public double? TimePointInSeconds => timePointInSeconds;

	public SleepyUpdaterID SleepyUpdater { get; set; }

	public double? UpdateInterval
	{
		get
		{
			return updateInterval;
		}
		private set
		{
			if (!Common.IsEqual(updateInterval, value))
			{
				updateInterval = value;
				LookUpSleepyUpdater<EventAction>.FindByID(SleepyUpdater)?.NotifyUpdateIntervalChanged(this);
			}
		}
	}

	public bool IsSnapshotted { get; set; }

	public void SetNextTimepoint(double? timepoint)
	{
		timePointInSeconds = timepoint;
	}

	void ISleepingUpdatable.CreateSleepyLookupCollection()
	{
	}

	public static void CreateSleepyLookupCollection()
	{
		LookUpSleepyUpdater<EventAction>.Create();
	}

	public EventAction(EventActionType eventActionType, ActionSetData parent)
	{
		EventActionType = eventActionType;
		if (parent != null)
		{
			ParentID = parent.ID;
		}
	}

	public EventAction()
	{
	}

	public void ExecuteNowOrLater(Entity triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
	{
		TriggeringEntity = triggeringEntity?.EntityID;
		TargetEntity = targetEntity;
		PolledEventSource = polledEventSource;
		DynamicTarget = dynamicTarget;
		if (EventActionType.DelayInSeconds > 0.0)
		{
			UpdateInterval = EventActionType.DelayInSeconds;
			The.Sim.PlaySite.EventManager.AddEventForLaterExecution(this);
		}
		else
		{
			Execute();
		}
	}

	public string Execute()
	{
		timer.Start();
		string failReason = "";
		bool flag = EventActionType.Execute(this, ref failReason);
		string result = null;
		if (The.Sim != null)
		{
			StringBuilder stringBuilder = new StringBuilder(The.Sim.TotalUnPausedGameTimeInSeconds.ToString());
			stringBuilder.Append(" ");
			stringBuilder.Append(ToString());
			stringBuilder.Append((!flag) ? ("(NO EXEC)" + failReason) : "");
			result = stringBuilder.ToString();
		}
		timer.GetTime();
		_ = 0.002;
		return result;
	}

	public void Update(GameTime gameTime, out bool wasDestroyed)
	{
		UpdateDelayedFiring(out wasDestroyed);
	}

	public void RecomputeUpdateInterval(out bool intervalChanged)
	{
		intervalChanged = false;
		double? currentInterval = null;
		UpdateTimePoints.GetSoonestInterval(UpdateTimePoints.ComputeIntervalFromTimepoint(TimePointInSeconds), ref currentInterval);
		if (!Common.IsEqual(UpdateInterval, currentInterval))
		{
			UpdateInterval = currentInterval;
			intervalChanged = true;
		}
	}

	private void UpdateDelayedFiring(out bool wasDestroyed)
	{
		wasDestroyed = false;
		if (The.Sim.TimepointReached(TimePointInSeconds))
		{
			wasDestroyed = true;
			Execute();
			Destroy();
		}
	}

	private void Destroy()
	{
		The.Sim.PlaySite.EventManager.RemoveEventAction(this);
	}

	public override string ToString()
	{
		return EventActionType.ToString();
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		EventActionType = sn.DoGameData(EventActionType);
		ParentID = sn.DoEnumNullable(ParentID);
		TargetEntity = sn.DoEnumNullable(TargetEntity);
		TriggeringEntity = sn.DoEnumNullable(TriggeringEntity);
		timePointInSeconds = sn.DoDoubleNullable(timePointInSeconds);
		updateInterval = sn.DoDoubleNullable(updateInterval);
		SleepyUpdater = sn.DoEnum(SleepyUpdater);
		if (sn.mode != Snapshotter.Mode.Load)
		{
			if (PolledEventSource is Entity entity)
			{
				polledEventSourceEntity = entity.ID;
			}
			if (PolledEventSource is Expedition expedition)
			{
				polledEventSourceExpedition = expedition.ID;
			}
			if (PolledEventSource is Allegiance allegiance)
			{
				polledEventSourceAllegiance = allegiance.ID;
			}
			if (PolledEventSource is Entity entity2)
			{
				dynamicTargetEntity = entity2.ID;
			}
			if (PolledEventSource is Expedition expedition2)
			{
				dynamicTargetExpedition = expedition2.ID;
			}
			if (PolledEventSource is Allegiance allegiance2)
			{
				dynamicTargetAllegiance = allegiance2.ID;
			}
		}
		polledEventSourceEntity = sn.DoEnumNullable(polledEventSourceEntity);
		polledEventSourceExpedition = sn.DoEnumNullable(polledEventSourceExpedition);
		polledEventSourceAllegiance = sn.DoEnumNullable(polledEventSourceAllegiance);
		dynamicTargetEntity = sn.DoEnumNullable(dynamicTargetEntity);
		dynamicTargetExpedition = sn.DoEnumNullable(dynamicTargetExpedition);
		dynamicTargetAllegiance = sn.DoEnumNullable(dynamicTargetAllegiance);
		sn.Ignore(EventActionType);
		sn.Ignore(DynamicTarget);
		sn.Ignore(PolledEventSource);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		if (polledEventSourceEntity.HasValue)
		{
			PolledEventSource = Entity.FindByID(polledEventSourceEntity);
		}
		else if (polledEventSourceExpedition.HasValue)
		{
			PolledEventSource = LookUp<Expedition, ExpeditionID>.FindByID(polledEventSourceExpedition);
		}
		else if (polledEventSourceAllegiance.HasValue)
		{
			PolledEventSource = LookUp<Allegiance, AllegianceID>.FindByID(polledEventSourceAllegiance);
		}
		polledEventSourceEntity = null;
		polledEventSourceExpedition = null;
		polledEventSourceAllegiance = null;
	}
}
