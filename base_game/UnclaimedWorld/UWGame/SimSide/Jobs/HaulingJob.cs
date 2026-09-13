using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Jobs;

public abstract class HaulingJob : Job
{
	public EntityGroupID ItemsToHaulGroup;

	public Vector3? ToLocation;

	public StorageTarget? ToStorage;

	public bool IsToTradeOfferStorage;

	public ProcessJob RequiredByProcessJob;

	private JobID? snapshotRequiredByProcessJob;

	private double CreatedOn;

	public OwnerID? NewOwner;

	private EntityID? item;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public EntityID? Item
	{
		get
		{
			return item;
		}
		set
		{
			if (value == item)
			{
				return;
			}
			EntityID? entityID = item;
			item = value;
			if (RequiredByProcessJob == null)
			{
				return;
			}
			if (entityID.HasValue)
			{
				Common.RemoveFromMultiList(RequiredByProcessJob.InputsBeingHauled, null, entityID.Value);
			}
			if (item.HasValue)
			{
				LookUp<EntityGroup, EntityGroupID>.FindByID(EntityGroupID).GetAllegiance().SharedKnowledge.GetKnownData(item.Value, out var data);
				if (!Common.MultiListContains(RequiredByProcessJob.InputsBeingHauled, data.EntityType, item.Value))
				{
					Common.AddToMultiList(RequiredByProcessJob.InputsBeingHauled, data.EntityType, item.Value);
				}
			}
		}
	}

	public bool IsCompleted { get; set; }

	public StorageID? GetToStorageID
	{
		get
		{
			if (ToStorage.HasValue)
			{
				return ToStorage.Value.StorageID;
			}
			return null;
		}
	}

	public EntityID? GetToStorageEntity
	{
		get
		{
			if (ToStorage.HasValue)
			{
				return ToStorage.Value.StorageEntity;
			}
			return null;
		}
	}

	public bool IsOfferedForTrade => IsToTradeOfferStorage;

	public StorageCompartment? GetToStorageCompartment
	{
		get
		{
			if (ToStorage.HasValue)
			{
				if (IsToTradeOfferStorage)
				{
					return StorageCompartment.OfferedForTrade;
				}
				return StorageCompartment.NormalStorage;
			}
			return null;
		}
	}

	public override bool RequiresBoldStance
	{
		get
		{
			if (RequiredByProcessJob != null)
			{
				return RequiredByProcessJob.ProcessType.RequiresBoldStance;
			}
			return false;
		}
	}

	public HaulingJob()
	{
	}

	public HaulingJob(Vector3? toLocation, StorageTarget? toStorage, bool isToTradeOfferStorage, EntityGroup entityGroup, OwnerID? newOwner, bool addToJobsGroupNow)
		: base(entityGroup, addToJobsGroupNow)
	{
		ToLocation = toLocation;
		ToStorage = toStorage;
		IsToTradeOfferStorage = isToTradeOfferStorage;
		NewOwner = newOwner;
		CreatedOn = The.Sim.TotalUnPausedGameTimeInSeconds;
	}

	public override void TakeJob(Entity entity)
	{
		base.TakeJob(entity);
	}

	public override Vector3? GetCircaLocation()
	{
		return null;
	}

	public bool IsStoredInTarget(IKnownEntityData entityData)
	{
		if (ToStorage.HasValue && ToStorage == entityData.StoredPermanentlyIn)
		{
			return true;
		}
		return false;
	}

	public void MarkClientAsInputNotEnroute()
	{
		if (RequiredByProcessJob == null || !item.HasValue)
		{
			return;
		}
		EntityGroup entityGroup = LookUp<EntityGroup, EntityGroupID>.FindByID(EntityGroupID);
		if (entityGroup != null)
		{
			entityGroup.GetAllegiance().SharedKnowledge.GetKnownData(item.Value, out var data);
			if (data != null)
			{
				Common.RemoveFromMultiList(RequiredByProcessJob.InputsBeingHauled, data.EntityType, item.Value);
			}
		}
	}

	public override void Destroy(bool cancelTakers, Entity entityToExcludeFromCancel = null)
	{
		JobID iD = base.ID;
		base.Destroy(cancelTakers, entityToExcludeFromCancel);
		if (!item.HasValue)
		{
			return;
		}
		EntityGroup entityGroup = LookUp<EntityGroup, EntityGroupID>.FindByID(ItemsToHaulGroup);
		if (entityGroup != null)
		{
			if (!GoalEvaluator.EntityDataResultCausesSkip(entityGroup.Parent.Allegiance.SharedKnowledge.GetKnownData(item.Value, out var data)) && data.AssignedToJob == iD)
			{
				data.AssignedToJob = null;
			}
			Entity entity = Entity.FindByID(item.Value);
			if (entity != null && entity.AssignedToJob == iD)
			{
				entity.AssignedToJob = null;
			}
		}
	}

	public bool GetToLocation(Entity agent, out Vector3? toLocation)
	{
		toLocation = null;
		if (ToLocation.HasValue)
		{
			toLocation = ToLocation.Value;
			return true;
		}
		IKnownEntityData data;
		EntityResult knownData = agent.Intelligence.Allegiance.SharedKnowledge.GetKnownData(ToStorage.Value.StorageEntity, out data);
		if (knownData == EntityResult.Remembered || knownData == EntityResult.SeenDirectly)
		{
			toLocation = data.Location;
			return true;
		}
		return false;
	}

	protected void GetToLocationAsString(StringBuilder b)
	{
		if (ToLocation.HasValue)
		{
			b.Append(MapManager.WorldPosToTile(ToLocation.Value).ToString());
			return;
		}
		Entity entity = Entity.FindByID(ToStorage.Value.StorageEntity);
		if (entity != null)
		{
			b.Append(entity.Name + " at " + entity.MapPosition.ToString());
		}
	}

	public float ScoreTimePassed()
	{
		double num = The.Sim.TotalUnPausedGameTimeInSeconds - CreatedOn - GameData.Instance.AIConstants.TimePassedForHaulingJobsToScoreHigher;
		if (num > 0.0)
		{
			num = Common.ClampTop(num, GameData.Instance.AIConstants.MaxAdditionalTimeForStarvedHaulingJobScore);
			return (float)num / GameData.Instance.AIConstants.MaxAdditionalTimeForStarvedHaulingJobScore;
		}
		return 0f;
	}

	public override Snapshotter.Version DoVersion(Snapshotter sn)
	{
		base.DoVersion(sn);
		version = sn.DoVersion((Snapshotter.Version)2u);
		return version;
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		snapshotRequiredByProcessJob = sn.SnapshotID<Job, JobID>(RequiredByProcessJob);
		ToStorage = sn.DoStorageTargetNullable(ToStorage);
		IsToTradeOfferStorage = sn.DoBool(IsToTradeOfferStorage);
		ToLocation = sn.DoVector3Nullable(ToLocation);
		NewOwner = sn.DoEnumNullable(NewOwner);
		ItemsToHaulGroup = sn.DoEnum(ItemsToHaulGroup);
		item = sn.DoEnumNullable(item);
		IsCompleted = sn.DoBool(IsCompleted);
		if (version >= (Snapshotter.Version)2u)
		{
			CreatedOn = sn.DoDouble(CreatedOn);
		}
		sn.Ignore(RequiredByProcessJob);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		base.LoadPostProcess(sn);
		if (snapshotRequiredByProcessJob.HasValue)
		{
			RequiredByProcessJob = (ProcessJob)LookUp<Job, JobID>.FindByID(snapshotRequiredByProcessJob);
		}
		if (version < (Snapshotter.Version)2u)
		{
			CreatedOn = The.Sim.TotalUnPausedGameTimeInSeconds;
		}
	}
}
