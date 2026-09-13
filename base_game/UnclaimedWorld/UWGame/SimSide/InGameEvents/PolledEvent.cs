using System.Diagnostics;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems;

namespace UWGame.SimSide.InGameEvents;

[DebuggerDisplay("{PolledEventType.KeyName} time:{timePointInSeconds}, interval:{updateInterval}")]
public class PolledEvent : ISleepingUpdatable, ISnapshot
{
	public PolledEventType PolledEventType;

	public EntityID? SourceEntity;

	public ExpeditionID? SourceExpedition;

	public AllegianceID? SourceAllegiance;

	private bool hasUpdatedOnce;

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
				LookUpSleepyUpdater<PolledEvent>.FindByID(SleepyUpdater)?.NotifyUpdateIntervalChanged(this);
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
		LookUpSleepyUpdater<PolledEvent>.Create();
	}

	public PolledEvent(PolledEventType type, EntityID? sourceEntity = null, ExpeditionID? sourceExpedition = null, AllegianceID? sourceAllegiance = null)
	{
		PolledEventType = type;
		SourceEntity = sourceEntity;
		SourceExpedition = sourceExpedition;
		SourceAllegiance = sourceAllegiance;
		bool intervalChanged = false;
		RecomputeUpdateInterval(out intervalChanged);
	}

	public PolledEvent()
	{
	}

	private void Destroy()
	{
		The.Sim.PlaySite.EventManager.RemovePolledEvent(this);
	}

	public void Update(GameTime gameTime, out bool wasDestroyed)
	{
		wasDestroyed = false;
		Entity triggeringEntity = null;
		IHasExposedProperties polledEventSource = null;
		if (SourceEntity.HasValue)
		{
			polledEventSource = Entity.FindByID(SourceEntity.Value);
		}
		else if (SourceExpedition.HasValue)
		{
			polledEventSource = Expedition.FindByID(SourceExpedition.Value);
		}
		else if (SourceAllegiance.HasValue)
		{
			polledEventSource = LookUp<Allegiance, AllegianceID>.FindByID(SourceAllegiance.Value);
		}
		if (PolledEventType.Condition == null || PolledEventType.Condition.IsFulfilled(ref triggeringEntity, null, polledEventSource, null))
		{
			PolledEventType.Fire(triggeringEntity, polledEventSource, out wasDestroyed);
		}
		if (PolledEventType.PollInterval == null && !PolledEventType.UseDefaultPollInterval)
		{
			wasDestroyed = true;
		}
		if (wasDestroyed && The.Sim != null)
		{
			Destroy();
		}
	}

	public override string ToString()
	{
		return PolledEventType.KeyName;
	}

	public void RecomputeUpdateInterval(out bool intervalChanged)
	{
		intervalChanged = false;
		double? currentInterval = null;
		if (!hasUpdatedOnce)
		{
			if (PolledEventType.StartTimePoint != null)
			{
				UpdateTimePoints.GetSoonestInterval(PolledEventType.StartTimePoint.GetUpdateInterval(), ref currentInterval);
			}
			else if (PolledEventType.StartAfterInterval)
			{
				UpdateTimePoints.GetSoonestInterval(PolledEventType.GetPollIntervalUpdateInterval(), ref currentInterval);
			}
			if (!currentInterval.HasValue)
			{
				currentInterval = 0.0;
			}
			hasUpdatedOnce = true;
		}
		else
		{
			UpdateTimePoints.GetSoonestInterval(PolledEventType.GetPollIntervalUpdateInterval(), ref currentInterval);
			if (!currentInterval.HasValue)
			{
				currentInterval = 4.0;
			}
		}
		if (!Common.IsEqual(UpdateInterval, currentInterval))
		{
			UpdateInterval = currentInterval;
			intervalChanged = true;
		}
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		PolledEventType = sn.DoGameData(PolledEventType);
		timePointInSeconds = sn.DoDoubleNullable(timePointInSeconds);
		updateInterval = sn.DoDoubleNullable(updateInterval);
		SleepyUpdater = sn.DoEnum(SleepyUpdater);
		hasUpdatedOnce = sn.DoBool(hasUpdatedOnce);
		SourceEntity = sn.DoEnumNullable(SourceEntity);
		SourceExpedition = sn.DoEnumNullable(SourceExpedition);
		SourceAllegiance = sn.DoEnumNullable(SourceAllegiance);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}
}
