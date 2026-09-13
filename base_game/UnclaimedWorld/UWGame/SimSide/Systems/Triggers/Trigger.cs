using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.AI;
using UWGame.SimSide.Collisions;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Systems.Triggers;

public class Trigger : ISleepingUpdatable, IHasExposedProperties, ISnapshot, ILookUp<Trigger, TriggerID>
{
	public enum TriggerMovement
	{
		Static,
		Attached
	}

	public Entity Parent;

	private EntityID? snapshotParent;

	public TriggerType TriggerType;

	public float? DurationInSeconds;

	private CollideShape2D area;

	private float? range;

	private Vector2? areaDimensions;

	private Vector3? fixedLocation;

	private TimeSpan timeActivated;

	private int? timesTriggered;

	public object messageInfo;

	private double? expiryTimepointInSeconds;

	private double? timePointInSeconds;

	private double? updateInterval;

	private List<Pair<Entity, Vector2>> entitiesInRangeOfCurrentTrigger = new List<Pair<Entity, Vector2>>();

	private static Dictionary<string, GetPropertyValue> exposedPropertyValueFunctions;

	private Dictionary<string, PropertyResult> customFields;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	private TriggerID id = TriggerID.Invalid;

	private static TriggerID IDCounter;

	private Vector3 Location
	{
		get
		{
			if (Parent != null)
			{
				return Parent.PlaySiteLocation;
			}
			return fixedLocation.Value;
		}
	}

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
				LookUpSleepyUpdater<Trigger>.FindByID(SleepyUpdater)?.NotifyUpdateIntervalChanged(this);
			}
		}
	}

	public string KeyName { get; set; }

	public bool IsSnapshotted { get; set; }

	public TriggerID ID
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

	public void SetNextTimepoint(double? timepoint)
	{
		timePointInSeconds = timepoint;
	}

	void ISleepingUpdatable.CreateSleepyLookupCollection()
	{
	}

	public static void CreateSleepyLookupCollection()
	{
		LookUpSleepyUpdater<Trigger>.Create();
	}

	public Trigger(Entity attachedTo, Vector3? fixedLocation, TriggerType TriggerType, object messageInfo = null, float? range = null, Vector2? area = null)
	{
		AddToLookup();
		Parent = attachedTo;
		this.messageInfo = messageInfo;
		this.TriggerType = TriggerType;
		this.fixedLocation = fixedLocation;
		this.range = range ?? TriggerType.Range;
		areaDimensions = area ?? TriggerType.AreaDimensions;
		timeActivated = The.Sim.TotalUnPausedGameTime;
		if (DurationInSeconds.HasValue)
		{
			expiryTimepointInSeconds = UpdateTimePoints.ComputeTimePointFromInterval(DurationInSeconds.Value);
		}
		InitArea();
		RecomputeUpdateInterval();
	}

	private void InitArea()
	{
		if (areaDimensions.HasValue)
		{
			float x = areaDimensions.Value.X;
			float y = areaDimensions.Value.Y;
			float num = Location.X - x / 2f;
			float num2 = Location.Y - y / 2f;
			area = new CollideShape2D(num2, num, num2 + y, num + x);
		}
	}

	static Trigger()
	{
		exposedPropertyValueFunctions = new Dictionary<string, GetPropertyValue>();
		IDCounter = TriggerID.First;
		exposedPropertyValueFunctions.Add("triggeringsLeft", GetTriggeringsLeft);
	}

	public Trigger()
	{
	}

	public void Update(GameTime gameTime, out bool wasDestroyed)
	{
		UpdateTrigger(out wasDestroyed);
		if (!wasDestroyed)
		{
			UpdateExpiry(out wasDestroyed);
		}
	}

	public void UpdateTrigger(out bool wasDestroyed)
	{
		wasDestroyed = false;
		Predicate<Entity> filter = null;
		entitiesInRangeOfCurrentTrigger.Clear();
		if (area == null)
		{
			float value = range.Value;
			The.AgentQuadTree.GetEntitiesInRange(Location.ToVector2(), value, filter, ref entitiesInRangeOfCurrentTrigger);
		}
		else
		{
			The.AgentQuadTree.GetObjectsIntersectingBounds(area, filter, ref entitiesInRangeOfCurrentTrigger);
			_ = entitiesInRangeOfCurrentTrigger.Count;
			_ = 0;
		}
		foreach (Pair<Entity, Vector2> item in entitiesInRangeOfCurrentTrigger)
		{
			if (item.First != Parent && item.First.EntityType.IntelligenceType.HasInterestInTriggers.ContainsKey(TriggerType) && item.First.Intelligence.IsReadyToHandleTrigger(this) && (Parent == null || TriggerType.CanTriggerWhenUndetected || item.First.Intelligence.GetKnownData(Parent.EntityID, out var _) == EntityResult.SeenDirectly))
			{
				FireTrigger(item.First, out wasDestroyed);
			}
		}
	}

	private void UpdateExpiry(out bool wasDestroyed)
	{
		wasDestroyed = false;
		if (expiryTimepointInSeconds.HasValue && The.Sim.TimepointReached(expiryTimepointInSeconds.Value))
		{
			Destroy();
			wasDestroyed = true;
			expiryTimepointInSeconds = null;
		}
	}

	private void RecomputeUpdateInterval()
	{
		RecomputeUpdateInterval(out var _);
	}

	public void RecomputeUpdateInterval(out bool intervalWasChanged)
	{
		intervalWasChanged = false;
		double? currentInterval = null;
		UpdateTimePoints.GetSoonestInterval(UpdateTimePoints.ComputeIntervalFromTimepoint(expiryTimepointInSeconds), ref currentInterval);
		UpdateTimePoints.GetSoonestInterval(TriggerType.DurationBetweenTriggerUpdatesInSeconds, ref currentInterval);
		if (!Common.IsEqual(UpdateInterval, currentInterval))
		{
			UpdateInterval = currentInterval;
			intervalWasChanged = true;
		}
	}

	public void Destroy()
	{
		if (Parent != null)
		{
			Parent.DeleteTrigger(this);
		}
		else
		{
			The.Sim.TriggerSystem.DeleteTrigger(this);
		}
		RemoveIDEntry();
	}

	public void FireTrigger(Entity entity, out bool wasDestroyed)
	{
		wasDestroyed = false;
		SendMessages(entity);
		FireEvents(entity);
		if (TriggerType.MaxTimesToTriggerBeforeExpiring.HasValue)
		{
			if (!timesTriggered.HasValue)
			{
				timesTriggered = 1;
			}
			else
			{
				timesTriggered++;
			}
			if (timesTriggered >= TriggerType.MaxTimesToTriggerBeforeExpiring.Value)
			{
				Destroy();
				wasDestroyed = true;
			}
		}
	}

	public PropertyResult? GetTriggeringsLeft()
	{
		if (TriggerType.MaxTimesToTriggerBeforeExpiring.HasValue)
		{
			int num = TriggerType.MaxTimesToTriggerBeforeExpiring.Value - (timesTriggered ?? 0);
			return new PropertyResult
			{
				NumberResult = num
			};
		}
		return null;
	}

	private OwnerID? GetOwnerOfCarcass()
	{
		if (Parent.EntityType.IntelligenceType != null && Parent.Intelligence.CurrentExpedition != null)
		{
			return ((ILookUp<IOwner, OwnerID>)Parent.Intelligence.CurrentExpedition).ID;
		}
		if (Parent.OwnedBy.HasValue)
		{
			return Parent.OwnedBy;
		}
		return null;
	}

	private void SendMessages(Entity entity)
	{
		bool flag = false;
		if (TriggerType.IsPrey)
		{
			entity.SendMessage(new Message(Parent, Message.MessageTypes.PreyIsNear, this));
			flag = true;
		}
		if (TriggerType.IsEntityDied)
		{
			entity.SendMessage(new Message(Parent, Message.MessageTypes.EntityDied, this));
			flag = true;
		}
		if (!flag && TriggerType.Interest != null)
		{
			Message msg = CreateInterestMessage(Parent, this, Parent.EntityID, null, null);
			entity.SendMessage(msg);
		}
	}

	private void FireEvents(Entity detectingEntity)
	{
		if (TriggerType.ActionSetsKey != null)
		{
			ActionSets actionSets = GameData.Instance.AllActionSets[TriggerType.ActionSetsKey];
			Entity triggeringEntity = null;
			if (Parent != null)
			{
				triggeringEntity = Parent;
			}
			actionSets.Fire(triggeringEntity, detectingEntity.ID, null, out var _);
		}
	}

	public static Message CreateInterestMessage(Entity sender, Trigger trigger, EntityID? entity, Vector3? location, float? interest)
	{
		return new Message(sender, Message.MessageTypes.Interest, new Tuple<Trigger, EntityID?, Vector3?, float?>(trigger, entity, location, interest));
	}

	public PropertyResult? GetPropertyValue(string propertyKey, SharedKnowledge getterKnowledge, IHasExposedProperties parent)
	{
		PropertyResult? result = null;
		if (exposedPropertyValueFunctions.ContainsKey(propertyKey))
		{
			return exposedPropertyValueFunctions[propertyKey](this, getterKnowledge, parent);
		}
		if (customFields != null && customFields.TryGetValue(propertyKey, out var value))
		{
			result = value;
		}
		return result;
	}

	public string GetDefaultCaption(string propertyKey)
	{
		return TriggerType.Name;
	}

	public void GetDefaultKey(out string PropertyKey)
	{
		PropertyKey = null;
	}

	public void GetChildren(string keyToList, ref List<IHasExposedProperties> listToFillWithProperties, FilterCondition filter, EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget, SharedKnowledge getterKnowledge = null)
	{
	}

	public EntityID? GetEntityID()
	{
		return null;
	}

	public bool GetIsSeenDirectly()
	{
		return true;
	}

	public string GetCaption(string captionKey)
	{
		return null;
	}

	public void SetPropertyValue(string propertyKey, PropertyResult? value)
	{
		Entity.SetPropertyValue(ref customFields, propertyKey, value);
	}

	public static PropertyResult? GetTriggeringsLeft(IHasExposedProperties anObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Trigger)anObjectToGetValueFrom).GetTriggeringsLeft();
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		id = SnapshotID(sn, id);
		IDCounter = sn.DoEnum(IDCounter);
		snapshotParent = sn.SnapshotID<Entity, EntityID>(Parent);
		TriggerType = sn.DoGameData(TriggerType);
		DurationInSeconds = sn.DoFloatNullable(DurationInSeconds);
		expiryTimepointInSeconds = sn.DoDoubleNullable(expiryTimepointInSeconds);
		fixedLocation = sn.DoVector3Nullable(fixedLocation);
		messageInfo = sn.DoObject(messageInfo);
		timeActivated = sn.DoTimeSpan(timeActivated);
		timePointInSeconds = sn.DoDoubleNullable(timePointInSeconds);
		timesTriggered = sn.DoInt32Nullable(timesTriggered);
		updateInterval = sn.DoDoubleNullable(updateInterval);
		SleepyUpdater = sn.DoEnum(SleepyUpdater);
		customFields = sn.DoDictionary(customFields);
		KeyName = sn.DoString(KeyName);
		areaDimensions = sn.DoVector2Nullable(areaDimensions);
		range = sn.DoFloatNullable(range);
		sn.Ignore(entitiesInRangeOfCurrentTrigger);
		sn.Ignore(exposedPropertyValueFunctions);
		sn.Ignore(area);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		InitArea();
		Parent = Entity.FindByID(snapshotParent);
	}

	public TriggerID GetUniqueID()
	{
		IDCounter++;
		if ((ulong)IDCounter >= ulong.MaxValue)
		{
			throw new Exception("Astounding, TriggerID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public TriggerID SnapshotID(Snapshotter sn, TriggerID id)
	{
		return sn.DoEnum(id);
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != TriggerID.Invalid)
		{
			LookUp<Trigger, TriggerID>.Add(ID, this);
		}
	}

	public void SetInvalid()
	{
		id = TriggerID.Invalid;
	}

	public void RemoveIDEntry()
	{
		LookUp<Trigger, TriggerID>.Remove(this);
	}

	void ILookUp<Trigger, TriggerID>.ResetIDCounter()
	{
	}

	public static void ResetIDCounter()
	{
		IDCounter = TriggerID.First;
	}

	void ILookUp<Trigger, TriggerID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUp<Trigger, TriggerID>.Create();
	}
}
