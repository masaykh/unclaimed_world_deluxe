using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.Client.Interface;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Collisions;
using UWGame.SimSide.Combat;
using UWGame.SimSide.Communication;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.SimSide.Entities.Locomotors.Stances;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Entities.RepairTypes;
using UWGame.SimSide.Entities.Substances;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.GatheringSites;
using UWGame.SimSide.InGameEvents;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.Items;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Overland.Locations;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Resources;
using UWGame.SimSide.SimEffects;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems;
using UWGame.SimSide.Systems.Triggers;
using UWGame.SimSide.Trees;
using UWGame.SimSide.Vehicles;
using WindowSystem;
using Xclna.Xna.Animation;

namespace UWGame.SimSide.Entities;

[DebuggerDisplay("{Name}{ID}{EntityType.Name}{MapPosition}")]
public class Entity : GameObject, IAddon, IComposite, ILookUp<IComposite, CompositeID>, ICanIterateEntities, ILookUp<ICanIterateEntities, CanIterateEntitiesID>, IKnownEntityData, IHasExposedProperties, IDetectable, ILookUp<IDetectable, DetectableID>, ISnapshot, ILookUp<Entity, EntityID>, ICommunicates, ISleepingUpdatable
{
	private delegate void GetChildrenDelegate(SharedKnowledge getterKnowledge, IHasExposedProperties hasProperties, ref List<IHasExposedProperties> listOfChildren);

	public enum StructureState
	{
		Ordered,
		Unfinished,
		Finished
	}

	public struct SetOwnerInfo
	{
		public IOwner NewOwner;

		public GiveNewOwnerKnowledge GiveNewOwnerKnowledge;

		public SetOwnerInfo(IOwner newOwner, GiveNewOwnerKnowledge giveNewOwnerKnowledge)
		{
			NewOwner = newOwner;
			GiveNewOwnerKnowledge = giveNewOwnerKnowledge;
		}

		public SetOwnerInfo(IOwner newOwner)
		{
			NewOwner = newOwner;
			GiveNewOwnerKnowledge = GiveNewOwnerKnowledge.Yes;
		}
	}

	public enum AddRandomOffset
	{
		Yes,
		No
	}

	public enum GiveNewOwnerKnowledge
	{
		Yes,
		No
	}

	private static Dictionary<string, GetPropertyValue> exposedPropertyValueFunctions;

	private static Dictionary<string, GetChildrenDelegate> getChildrenProperties;

	private BitMask64 simStateFlags;

	public SimStateInfo CurrentSimState;

	private EntityID id = EntityID.Invalid;

	private static EntityID IDCounter;

	public DebugLog DebugLog = new DebugLog();

	public OwnerID? SpawnedByOwner;

	private Renderable.SnapshotRenderable snapshotRenderable;

	private bool isInitialized;

	private Vector3? accessPoint;

	private Regulator showStatusRegulator;

	private bool tooltipEntityDataIsDirty = true;

	private EntityTypeTooltipInstanceData tooltipEntityData;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	private string name;

	public Dictionary<Type, Component> Components;

	private List<SimProcessID> processes;

	private SiteID? snapshotSiteID;

	private Site site;

	private Point? mapPosition;

	private GeodeticCoordinate? coords;

	private Dictionary<EntityType, List<EntityID>> containedEntities;

	private GatheringSiteID? snapshotGatheringSite;

	public Collidable<Entity> SelectionShape;

	public DirectionalLayout DirectionalLayout;

	public PointLayout PointLayout;

	public GeometryLayout GeometryLayout;

	private List<Trigger> attachedTriggers = new List<Trigger>();

	private List<TriggerID> snapshotTriggers;

	private Vector3 facingNormal = new Vector3(1f, 0f, 0f);

	protected float rotation;

	public bool IsDead;

	public IDActionEvent<float> BulkChangedEvent;

	private float bulk;

	private JobID? assignedToJob;

	private Dictionary<EntityGroupID, EntityID> inUseBy;

	private bool flipHorizontally;

	private EntityID? containedBy;

	public EntityID? DrivingVehicle;

	public EntityID? PassengerInVehicle;

	private bool footprintIsDirty;

	private int collidingTimeout = 3;

	private Regulator bioSystemsRegulator;

	private float avoidDetectionFactor;

	private const float constantDetectionFactor = 0.2f;

	public Dictionary<string, PropertyResult> CustomFields;

	private CompositeID compositeID;

	private DetectableID detectableID;

	private double? timePointInSeconds;

	private SleepyUpdaterID sleepyUpdater;

	private double? updateInterval;

	public CanIterateEntitiesID CanIterateEntitiesID;

	public EntityID EntityID => id;

	Vector3 IDetectable.Location => Location.Value;

	public DetectableID DetectableID => detectableID;

	public EntityID ID
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

	public static EntityID LastUsedID => IDCounter;

	public ResourceType ResourceType => null;

	public Renderable Renderable { get; set; }

	public Vector3? AccessPoint
	{
		get
		{
			if (PartOf != null)
			{
				return GetRootAsEntity().AccessPoint;
			}
			GetContainedBy(out Entity container);
			if (container != null)
			{
				return container.AccessPoint;
			}
			if ((CurrentSimState == null || CurrentSimState.GeometryLayoutType == null) && EntityType.PointLayoutType == null)
			{
				return Location;
			}
			if (!accessPoint.HasValue)
			{
				if (!Location.HasValue)
				{
					return null;
				}
				Vector3 locationToComputeFrom = location.Value;
				if (Contains != null && Contains is IExit exit)
				{
					locationToComputeFrom = exit.ComputeAccessPoint();
				}
				bool avoidReservedSubtiles = false;
				if (EntityType.StructureType != null)
				{
					avoidReservedSubtiles = true;
				}
				accessPoint = MapManager.FindUnblockedLocation(locationToComputeFrom, 200f, MapManager.ScanMethod.Fan, EntityType.AccessPointDirection, avoidReservedSubtiles, null, 40f);
			}
			return accessPoint.Value;
		}
	}

	public CasteType CasteType
	{
		get
		{
			if (EntityType.BiologicalType != null && Find<BiologicalEntity>(out var c))
			{
				return c.CasteType;
			}
			return null;
		}
	}

	public List<ProcessType> AvailableSharedSpecialActions { get; set; }

	public EntityTypeTooltipInstanceData TooltipEntityData
	{
		get
		{
			if (BiologicalEntity != null)
			{
				if (tooltipEntityDataIsDirty)
				{
					if (tooltipEntityData == null)
					{
						tooltipEntityData = new EntityTypeTooltipInstanceData();
					}
					BiologicalEntity.GenerateBiologicalEntityData(tooltipEntityData);
					tooltipEntityDataIsDirty = false;
				}
				return tooltipEntityData;
			}
			return null;
		}
	}

	public bool IsSnapshotted { get; set; }

	public DebugGoalPlan DebugGoalPlan { get; set; }

	public AllegianceID? AllegianceID
	{
		get
		{
			if (EntityType.IntelligenceType != null)
			{
				Intelligence intelligence = Intelligence;
				if (intelligence.Allegiance != null)
				{
					return intelligence.Allegiance.ID;
				}
			}
			return null;
		}
	}

	public ThreatGroup ThreatGroup
	{
		get
		{
			Threat c;
			if (EntityType.IntelligenceType != null)
			{
				Intelligence intelligence = Intelligence;
				if (intelligence != null)
				{
					return intelligence.Allegiance.ThreatGroup;
				}
			}
			else if (EntityType.ThreatType != null && Find<Threat>(out c))
			{
				return c.ThreatGroup;
			}
			return null;
		}
	}

	public string Name
	{
		get
		{
			return name;
		}
		set
		{
			if (!string.IsNullOrEmpty(name))
			{
				The.Sim.PlaySite.EntitiesByName.Remove(name);
			}
			if (!string.IsNullOrEmpty(value) && !The.Sim.PlaySite.EntitiesByName.ContainsKey(value))
			{
				The.Sim.PlaySite.EntitiesByName.Add(value, EntityID);
			}
			name = value;
		}
	}

	public EntityType EntityType { get; private set; }

	public OwnerID? OwnedBy { get; set; }

	public List<SimProcessID> Processes
	{
		get
		{
			return processes;
		}
		set
		{
			processes = value;
		}
	}

	SiteID? IKnownEntityData.Site
	{
		get
		{
			if (Site != null)
			{
				return Site.ID;
			}
			return null;
		}
	}

	public Site Site
	{
		get
		{
			return site;
		}
		set
		{
			if (value != site)
			{
				if (site != null)
				{
					site.RemoveEntity(this);
				}
				else
				{
					The.Sim.World.RemoveEntityFromBetweenSites(this);
				}
				site = value;
				if (site != null)
				{
					site.AddEntity(this);
				}
				else
				{
					The.Sim.World.AddEntityBetweenSites(this);
				}
			}
			if (Parts != null)
			{
				foreach (Entity part in Parts)
				{
					part.Site = value;
				}
			}
			if (Contains != null)
			{
				Contains.IterateContained(delegate(Entity e)
				{
					e.Site = value;
				});
			}
		}
	}

	public Vector3 PlaySiteLocation => Location.Value;

	public Point PlaySiteMapPosition => MapPosition.Value;

	public override Vector3? Location
	{
		get
		{
			return GetRootAndContainer().location;
		}
		set
		{
			if (value.HasValue)
			{
				Vector3 vector = new Vector3(The.Map.ClampWorldPosition(value.Value.ToVector2()), value.Value.Z);
				base.Location = vector;
				Vector2 value2 = vector.ToVector2();
				UpdateShapeLocation(Collidable, value2);
				UpdateShapeLocation(SelectionShape, value2);
				Point value3 = MapManager.WorldPosToTile(vector);
				MapPosition = value3;
			}
			else
			{
				base.Location = null;
				MapPosition = null;
			}
		}
	}

	public Point? MapPosition
	{
		get
		{
			return GetRootAndContainer().mapPosition;
		}
		private set
		{
			if (value != mapPosition)
			{
				Point? point = mapPosition;
				mapPosition = value;
				if (point.HasValue)
				{
					The.Map.GetTile(point.Value).RemoveEntity(this);
				}
				if (mapPosition.HasValue)
				{
					The.Map.GetTile(mapPosition.Value).AddEntity(this);
				}
				Find<Intelligence>(out var c);
				The.Map.UpdateWhoCanSeeEntityMovingBetweenTiles(this, c, point, value);
				if (c != null && IsCompleted() && c.Allegiance != null && Find<Sensor>(out var c2) && c2.IsActive && mapPosition.HasValue)
				{
					c2.UpdateTilesSeenBySensor(mapPosition.Value);
					DeprecateMemoryFactsInRadius();
				}
			}
		}
	}

	public GeodeticCoordinate? Coords
	{
		get
		{
			Entity rootAndContainer = GetRootAndContainer();
			if (rootAndContainer.Site != null)
			{
				return Site.Coords;
			}
			return rootAndContainer.coords;
		}
		set
		{
			coords = value;
		}
	}

	public Container Contains { get; set; }

	public float? CurrentMaximumSpeed
	{
		get
		{
			if (Find<Locomotor>(out var c))
			{
				return c.CurrentMaximumSpeed;
			}
			return null;
		}
	}

	public Dictionary<FoodNutrientType, float> NutrientBulkAmounts
	{
		get
		{
			if (Find<Item>(out var c))
			{
				return c.Food.NutrientBulkAmounts;
			}
			return null;
		}
	}

	public double? Condition
	{
		get
		{
			if (Find<NonLivingEntity>(out var c))
			{
				return c.Condition;
			}
			return null;
		}
	}

	public float? ConditionChangeSpeed
	{
		get
		{
			if (Find<NonLivingEntity>(out var c))
			{
				return c.ConditionChangeSpeed;
			}
			return null;
		}
	}

	public float? Integrity
	{
		get
		{
			if (Find<NonLivingEntity>(out var c))
			{
				return c.Integrity;
			}
			return null;
		}
	}

	public float? Progress
	{
		get
		{
			if (Find<NonLivingEntity>(out var c))
			{
				return c.Progress;
			}
			return null;
		}
	}

	public StanceType Stance
	{
		get
		{
			if (HasStance())
			{
				return Locomotor.Stance.CurrentStance;
			}
			return null;
		}
	}

	public float? StrengthRating
	{
		get
		{
			float num;
			if (EntityType.ThreatType != null)
			{
				num = GameData.Instance.Constants.StrengthRatings[EntityType.ThreatType.StrengthRating];
			}
			else
			{
				if (EntityType.IntelligenceType == null)
				{
					return null;
				}
				num = GameData.Instance.Constants.StrengthRatings[EntityType.IntelligenceType.StrengthRating];
			}
			if (BiologicalEntity != null)
			{
				switch (BiologicalEntity.AgeGroup.AgeGroupType.AIAgeGroup)
				{
				case AIAgeGroup.Baby:
					num = 0f;
					break;
				case AIAgeGroup.Child:
					num *= 0.5f;
					break;
				case AIAgeGroup.YoungAdult:
					num *= 0.7f;
					break;
				case AIAgeGroup.Old:
					num *= 0.8f;
					break;
				}
			}
			if (Find<BodyComponent>(out var c))
			{
				num *= (float)c.Body.FunctionalScore;
			}
			return num;
		}
	}

	public UWGame.SimSide.Entities.Body.Body Body
	{
		get
		{
			if (Find<BodyComponent>(out var c))
			{
				return c.Body;
			}
			return null;
		}
	}

	public Dictionary<SubstanceType, SubstanceAmount> SubstanceBulkAmounts
	{
		get
		{
			if (EntityType.SubstancesType != null && Find<SubstanceComponent>(out var c))
			{
				return c.BulkAmounts;
			}
			return null;
		}
	}

	public bool? IsMoving
	{
		get
		{
			if (Locomotor != null)
			{
				return Locomotor.IsMoving();
			}
			return null;
		}
	}

	public float BoundingRadius3D { get; set; }

	public Dictionary<EntityType, List<EntityID>> ContainedEntitiesByType
	{
		get
		{
			if (Contains != null)
			{
				if (containedEntities == null)
				{
					containedEntities = new Dictionary<EntityType, List<EntityID>>();
				}
				containedEntities.Clear();
				Contains.IterateContained(delegate(Entity e)
				{
					Common.AddToMultiList(containedEntities, e.EntityType, e.ID);
				});
				return containedEntities;
			}
			return null;
		}
	}

	public List<EntityID> ContainedEntities
	{
		get
		{
			if (Contains != null)
			{
				return (from e in Contains.GetContainedItemsList(null)
					select e.EntityID).ToList();
			}
			return null;
		}
	}

	public Dictionary<UpgradeCategory, EntityID> ContainedUpgrades
	{
		get
		{
			if (EntityType.ContainerType != null && EntityType.ContainerType.CanBeUpgraded && Contains is IUpgrades upgrades)
			{
				return upgrades.ContainedUpgrades;
			}
			return null;
		}
	}

	public Dictionary<EntityType, List<EntityID>> OfferedEntitiesByType
	{
		get
		{
			if (Contains != null && Contains is TerminalContainer terminalContainer)
			{
				return terminalContainer.GetOfferedItems();
			}
			return null;
		}
	}

	public Dictionary<EntityType, EntityID> IntrinsicWeapons
	{
		get
		{
			if (EntityType.IntelligenceType != null)
			{
				return Intelligence.IntrinsicWeapons;
			}
			return null;
		}
	}

	public bool HasItemStorage => StorageContainer != null;

	public IStorage StorageContainer
	{
		get
		{
			Container contains = Contains;
			if (contains != null && contains is IStorage result)
			{
				return result;
			}
			return null;
		}
	}

	public Dictionary<StorageCondition, Storage> StorageSpaces => StorageContainer?.GetStorageSpaces();

	public Dictionary<StorageCondition, Storage> TradeOffersStorageSpaces
	{
		get
		{
			if (Contains != null && Contains is TerminalContainer terminalContainer)
			{
				return terminalContainer.GetTradeOffersStorageSpaces();
			}
			return null;
		}
	}

	public float? TotalItemStorageCapacity => StorageContainer?.TotalItemStorageCapacity;

	public float? TotalStored => StorageContainer?.TotalStored;

	public bool? IsPrepared
	{
		get
		{
			if (Find<UWGame.SimSide.Items.Tool>(out var c))
			{
				return c.IsPrepared;
			}
			return null;
		}
	}

	public BiologicalEntity BiologicalEntity
	{
		get
		{
			if (Find<BiologicalEntity>(out var c))
			{
				return c;
			}
			return null;
		}
	}

	public Person PersonEntity
	{
		get
		{
			if (Find<Person>(out var c))
			{
				return c;
			}
			return null;
		}
	}

	public Item Item
	{
		get
		{
			if (Find<Item>(out var c))
			{
				return c;
			}
			return null;
		}
	}

	public Locomotor Locomotor
	{
		get
		{
			if (Find<Locomotor>(out var c))
			{
				return c;
			}
			return null;
		}
	}

	public SimEffectsComponent SimEffects
	{
		get
		{
			if (Find<SimEffectsComponent>(out var c))
			{
				return c;
			}
			return null;
		}
	}

	public Intelligence Intelligence
	{
		get
		{
			if (Find<Intelligence>(out var c))
			{
				return c;
			}
			return null;
		}
	}

	public Vehicle Vehicle
	{
		get
		{
			if (Find<Vehicle>(out var c))
			{
				return c;
			}
			return null;
		}
	}

	public Structure Structure
	{
		get
		{
			if (Find<Structure>(out var c))
			{
				return c;
			}
			return null;
		}
	}

	public GatheringSite GatheringSite { get; set; }

	public Collidable<Entity> Collidable { get; set; }

	public TerrainPath TerrainPath
	{
		get
		{
			if (Find<TerrainPath>(out var c))
			{
				return c;
			}
			return null;
		}
	}

	public Vector3 FacingNormal
	{
		get
		{
			return facingNormal;
		}
		set
		{
			facingNormal = value;
		}
	}

	public bool HasLocation
	{
		get
		{
			if (!containedBy.HasValue && !mapPosition.HasValue)
			{
				return false;
			}
			return true;
		}
	}

	public Point? TopLeftMapPosition => MapPosition;

	public float Rotation
	{
		get
		{
			return rotation;
		}
		set
		{
			rotation = value;
		}
	}

	public float FacingAngleWithRotator
	{
		get
		{
			if (Locomotor == null || Locomotor.Rotator == null)
			{
				return Rotation;
			}
			return Locomotor.Rotator.AbsoluteRotation;
		}
	}

	public NonLivingEntity NonLivingEntity
	{
		get
		{
			Find<NonLivingEntity>(out var c);
			return c;
		}
	}

	public CompositeID? PartOfID => NonLivingEntity?.PartOfID;

	public List<EntityID> PartIDs => Parts?.Select((Entity e) => e.ID).ToList();

	public IComposite PartOf
	{
		get
		{
			return NonLivingEntity?.PartOf;
		}
		set
		{
			NonLivingEntity nonLivingEntity = NonLivingEntity;
			if (nonLivingEntity != null)
			{
				nonLivingEntity.PartOf = value;
			}
		}
	}

	public EntityID? ParentEntityID => NonLivingEntity?.ParentEntityID;

	public List<Entity> Parts
	{
		get
		{
			return NonLivingEntity?.Parts;
		}
		set
		{
			NonLivingEntity nonLivingEntity = NonLivingEntity;
			if (nonLivingEntity != null)
			{
				nonLivingEntity.Parts = value;
			}
		}
	}

	public bool PartIsBroken { get; private set; }

	public EntityID RootEntityID => GetRootEntity().ID;

	public bool IsCompositeRoot
	{
		get
		{
			if (IsRoot)
			{
				return !IsLeaf;
			}
			return false;
		}
	}

	public bool IsRoot => PartOf == null;

	public bool IsLeaf => Parts == null;

	public float Bulk
	{
		get
		{
			return bulk;
		}
		set
		{
			if (Common.IsEqual(bulk, value))
			{
				return;
			}
			float num = bulk;
			bulk = value;
			BulkChangedEvent.Invoke(num);
			if (EntityType.TreeType != null)
			{
				Find<Tree>(out var c);
				c.UpdateBulk();
			}
			if (EntityType.BodyType != null)
			{
				Find<BodyComponent>(out var c2);
				c2.Body.UpdateBulk(num);
			}
			if (EntityType.BiologicalType != null)
			{
				Find<BiologicalEntity>(out var c3);
				if (c3.Needs != null)
				{
					_ = ID;
					_ = 4600;
					c3.Needs.UpdateNeedsTotalBulk();
				}
			}
		}
	}

	public AgentStorage AgentStorage
	{
		get
		{
			if (Contains != null && Contains is AgentStorage result)
			{
				return result;
			}
			return null;
		}
	}

	public JobID? AssignedToJob
	{
		get
		{
			return ((Entity)GetRoot())?.assignedToJob;
		}
		set
		{
			Entity entity = (Entity)GetRoot();
			if (entity == null)
			{
				return;
			}
			JobID? jobID = entity.assignedToJob;
			if (entity.assignedToJob != value)
			{
				if (value.HasValue)
				{
					DebugLog.Add($"AssignedToJob set: JobID {value.Value.ToString()}");
				}
				else
				{
					DebugLog.Add($"AssignedToJob cleared. Old JobID: {jobID.Value.ToString()}");
				}
			}
			entity.assignedToJob = value;
			if (jobID.HasValue)
			{
				Job job = LookUp<Job, JobID>.FindByID(jobID);
				Job job2 = LookUp<Job, JobID>.FindByID(value);
				if ((job is HaulingJob || job2 is HaulingJob) && entity.ContainedBy.HasValue && entity.GetContainedBy(out Container container) && container is AgentStorage agentStorage)
				{
					agentStorage.isHaulingIsDirty = true;
				}
			}
		}
	}

	public bool FlipHorizontally
	{
		get
		{
			return flipHorizontally;
		}
		set
		{
			flipHorizontally = value;
			if (Renderable != null)
			{
				Renderable.FlipHorizontally = value;
			}
		}
	}

	public EntityID? ContainedBy
	{
		get
		{
			return GetRootAsEntity().containedBy;
		}
		set
		{
			Entity rootAsEntity = GetRootAsEntity();
			if (this == rootAsEntity)
			{
				if (value != containedBy)
				{
					if (value.HasValue)
					{
						The.Map.UpdateWhoCanSeeEntityBeingContained(this, Intelligence, value);
					}
					containedBy = value;
					if (value.HasValue)
					{
						SetLocationPropertiesForLeaf();
					}
				}
			}
			else
			{
				rootAsEntity.ContainedBy = value;
			}
		}
	}

	public Entity OnBoard
	{
		get
		{
			if (containedBy.HasValue)
			{
				Entity entity = FindByID(containedBy.Value);
				if (entity.EntityType.ContainerType is VehicleContainerType)
				{
					return entity;
				}
			}
			return null;
		}
	}

	public bool NotOnboardDrivenVehicle
	{
		get
		{
			if (Item != null)
			{
				return OnBoard == null;
			}
			return true;
		}
	}

	public EntityID? InsideVehicle
	{
		get
		{
			if (PassengerInVehicle.HasValue)
			{
				return PassengerInVehicle;
			}
			if (DrivingVehicle.HasValue)
			{
				return DrivingVehicle;
			}
			return null;
		}
	}

	public StorageTarget? StoredPermanentlyIn
	{
		get
		{
			if (GetContainedBy(out Entity container) && container != null && container.Contains is IStorage storage && IsPermanentStorage(container.EntityType))
			{
				Storage storedIn = storage.GetStoredIn(this);
				if (storedIn != null)
				{
					return new StorageTarget(container.ID, storedIn.ID);
				}
			}
			return null;
		}
	}

	public EntityID? Replenishes
	{
		get
		{
			if (GetReplenishes(out var replenishes) && replenishes != null)
			{
				return replenishes.EntityID;
			}
			return null;
		}
	}

	public EntityID? UpgradeFor
	{
		get
		{
			if (GetUpgradesFor(out var upgrades) && upgrades != null)
			{
				return upgrades.EntityID;
			}
			return null;
		}
	}

	public List<HouseholdID> Households
	{
		get
		{
			Container contains = Contains;
			if (contains != null && contains is IResidence { Residence: not null } residence)
			{
				return residence.Residence.Households;
			}
			return null;
		}
	}

	public int? Residents
	{
		get
		{
			Container contains = Contains;
			if (contains != null && contains is IResidence { Residence: not null } residence)
			{
				return residence.Residence.Residents;
			}
			return null;
		}
		set
		{
			Container contains = Contains;
			if (contains != null && value.HasValue && contains is IResidence residence)
			{
				residence.Residence.Residents = value.Value;
			}
		}
	}

	public int? NoOfRounds
	{
		get
		{
			if (Find<Item>(out var c) && c.Ammunition != null)
			{
				return c.Ammunition.NoOfRounds;
			}
			return null;
		}
	}

	public bool FootprintIsDirty
	{
		get
		{
			return footprintIsDirty;
		}
		set
		{
			if (!footprintIsDirty)
			{
				footprintIsDirty = true;
				RecomputeUpdateInterval();
			}
		}
	}

	public double? FunctionalScore
	{
		get
		{
			if (Find<BodyComponent>(out var c))
			{
				return c.Body.FunctionalScore;
			}
			return null;
		}
	}

	public bool IsIntelligent => EntityType.IntelligenceType != null;

	public string KeyName => EntityType.KeyName;

	public Vector3 RenderedLocation
	{
		get
		{
			if (Renderable.RenderAsModel != null)
			{
				return Renderable.RenderAsModel.Location;
			}
			return Renderable.Location.Value;
		}
	}

	public float? ComfortLevel
	{
		get
		{
			if (EntityType.ContainerType != null && EntityType.ContainerType.ResidenceType != null)
			{
				return (float)Residence.GetComfortRating(this);
			}
			return null;
		}
	}

	CompositeID ILookUp<IComposite, CompositeID>.ID => compositeID;

	DetectableID ILookUp<IDetectable, DetectableID>.ID => detectableID;

	Allegiance ICanIterateEntities.GetAllegiance => Intelligence.Allegiance;

	public double? TimePointInSeconds => timePointInSeconds;

	public SleepyUpdaterID SleepyUpdater
	{
		get
		{
			return sleepyUpdater;
		}
		set
		{
			sleepyUpdater = value;
		}
	}

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
				LookUpSleepyUpdater<Entity>.FindByID(SleepyUpdater)?.NotifyUpdateIntervalChanged(this);
			}
		}
	}

	CanIterateEntitiesID ILookUp<ICanIterateEntities, CanIterateEntitiesID>.ID => CanIterateEntitiesID;

	public EntityID GetUniqueID()
	{
		IDCounter++;
		if (IDCounter >= EntityID.Invalid)
		{
			throw new Exception("Astounding, EntityID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public EntityID SnapshotID(Snapshotter sn, EntityID id)
	{
		return sn.DoEnum(id);
	}

	public void AddToLookup()
	{
		AddToLookup(hasID: false);
	}

	public void AddToLookup(bool hasID)
	{
		if (!hasID)
		{
			ID = GetUniqueID();
		}
		else
		{
			IDCounter++;
		}
		_ = ID;
		_ = 9;
		if (ID != EntityID.Invalid)
		{
			LookUp<Entity, EntityID>.Add(ID, this);
		}
	}

	public void SetInvalid()
	{
		id = EntityID.Invalid;
	}

	public void RemoveIDEntry()
	{
		LookUp<Entity, EntityID>.Remove(this);
	}

	void ILookUp<Entity, EntityID>.ResetIDCounter()
	{
	}

	public static void ResetIDCounter()
	{
		IDCounter = EntityID.First;
	}

	void ILookUp<Entity, EntityID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUp<Entity, EntityID>.Create();
	}

	public static Entity FindByID(EntityID id)
	{
		if (id == EntityID.Invalid)
		{
			return null;
		}
		return LookUp<Entity, EntityID>.FindByID(id);
	}

	public static Entity FindByID(EntityID? id)
	{
		if (!id.HasValue || id == EntityID.Invalid)
		{
			return null;
		}
		return LookUp<Entity, EntityID>.FindByID(id);
	}

	public float GetEffect(AffectsNumbers affects, float baseValue, string typeKey = null, string typeTag = null)
	{
		return SimEffects?.GetEffect(affects, baseValue, typeKey, typeTag) ?? 1f;
	}

	public bool GetEffect(AffectsFlags affects, bool baseValue, string typeKey = null, string typeTag = null, List<Tuple<string, bool>> effectComponents = null)
	{
		return SimEffects?.GetEffect(affects, baseValue, typeKey, typeTag, effectComponents) ?? baseValue;
	}

	public void SetInterestInCollidedEntity(Entity otherEntity)
	{
		if (EntityType.RenderableTypeMode.AnimatedHeadType != null)
		{
			float num = (float)The.Sim.GameplayRandomGenerator.RandomNormalDistribution(GameData.Instance.Constants.InterestLevelForCollidedEntityMean, GameData.Instance.Constants.InterestLevelForCollidedEntityStdDeviation);
			if (num > 0f)
			{
				Intelligence.SetNewCenterOfAttention(otherEntity.EntityID, null, num);
			}
		}
	}

	public void SetAccessPointDirty()
	{
		accessPoint = null;
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		IDCounter = sn.DoEnum(IDCounter);
		id = SnapshotID(sn, id);
		compositeID = sn.DoEnum(compositeID);
		detectableID = sn.DoEnum(detectableID);
		CanIterateEntitiesID = sn.DoEnum(CanIterateEntitiesID);
		EntityType = sn.DoGameData(EntityType);
		accessPoint = sn.DoVector3Nullable(accessPoint);
		avoidDetectionFactor = sn.DoFloat(avoidDetectionFactor);
		tooltipEntityDataIsDirty = sn.DoBool(tooltipEntityDataIsDirty);
		bulk = sn.DoFloat(bulk);
		collidingTimeout = sn.DoInt32(collidingTimeout);
		Components = sn.DoDictionary(Components);
		DebugLog = (DebugLog)sn.DoISnapshot(DebugLog);
		SpawnedByOwner = sn.DoEnumNullable(SpawnedByOwner);
		_ = ID;
		_ = 4528;
		containedBy = sn.DoEntityIDNullable(containedBy);
		CustomFields = sn.DoDictionary(CustomFields);
		DrivingVehicle = sn.DoEntityIDNullable(DrivingVehicle);
		isInitialized = sn.DoBool(isInitialized);
		facingNormal = sn.DoVector3(facingNormal);
		flipHorizontally = sn.DoBool(flipHorizontally);
		footprintIsDirty = sn.DoBool(footprintIsDirty);
		GeometryLayout = sn.DoISnapshot(GeometryLayout) as GeometryLayout;
		PartIsBroken = sn.DoBool(PartIsBroken);
		IsDead = sn.DoBool(IsDead);
		location = sn.DoVector3Nullable(location);
		mapPosition = sn.DoPointNullable(mapPosition);
		name = sn.DoString(name);
		PointLayout = sn.DoISnapshot(PointLayout) as PointLayout;
		rotation = sn.DoFloat(rotation);
		if (sn.mode != Snapshotter.Mode.Load && The.Client != null && Renderable != null)
		{
			snapshotRenderable = Renderable.GetFieldsToSnapshot();
		}
		snapshotRenderable = (Renderable.SnapshotRenderable)sn.DoISnapshot(snapshotRenderable);
		DirectionalLayout = (DirectionalLayout)sn.DoISnapshot(DirectionalLayout);
		BoundingRadius3D = sn.DoFloat(BoundingRadius3D);
		BulkChangedEvent = (IDActionEvent<float>)sn.DoISnapshot(BulkChangedEvent);
		tooltipEntityData = (EntityTypeTooltipInstanceData)sn.DoISnapshot(tooltipEntityData);
		AvailableSharedSpecialActions = sn.DoList(AvailableSharedSpecialActions);
		OwnedBy = sn.DoEnumNullable(OwnedBy);
		Contains = (Container)sn.DoISnapshot(Contains);
		containedEntities = sn.DoMultiMap(containedEntities);
		processes = sn.DoList(processes);
		updateInterval = sn.DoDoubleNullable(updateInterval);
		timePointInSeconds = sn.DoDoubleNullable(timePointInSeconds);
		coords = sn.DoGeodeticCoordinateNullable(coords);
		SleepyUpdater = sn.DoEnum(SleepyUpdater);
		bioSystemsRegulator = (Regulator)sn.DoISnapshot(bioSystemsRegulator);
		simStateFlags = (BitMask64)sn.DoISnapshot(simStateFlags);
		assignedToJob = sn.DoEnumNullable(assignedToJob);
		snapshotGatheringSite = sn.SnapshotID<GatheringSite, GatheringSiteID>(GatheringSite);
		snapshotSiteID = sn.SnapshotID<Site, SiteID>(site);
		inUseBy = sn.DoDictionary(inUseBy);
		if (sn.mode != Snapshotter.Mode.Load)
		{
			snapshotTriggers = attachedTriggers.Select((Trigger t) => t.ID).ToList();
		}
		snapshotTriggers = sn.DoList(snapshotTriggers);
		sn.Ignore(CurrentSimState);
		sn.Ignore(attachedTriggers);
		sn.Ignore(exposedPropertyValueFunctions);
		sn.Ignore(getChildrenProperties);
		sn.Ignore(Parts);
		sn.Ignore(Collidable);
		sn.Ignore(SelectionShape);
		sn.Ignore(DebugGoalPlan);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		foreach (KeyValuePair<Type, Component> component in Components)
		{
			component.Value.LoadPostProcess(sn);
		}
		if (Contains != null)
		{
			Contains.LoadPostProcess(sn);
		}
		if (GeometryLayout != null)
		{
			GeometryLayout.LoadPostProcess(sn);
		}
		if (PointLayout != null)
		{
			PointLayout.LoadPostProcess(sn);
		}
		attachedTriggers = snapshotTriggers.Select((TriggerID t) => LookUp<Trigger, TriggerID>.FindByID(t)).ToList();
		site = LookUp<Site, SiteID>.FindByID(snapshotSiteID);
		if (snapshotGatheringSite.HasValue && snapshotGatheringSite.HasValue)
		{
			GatheringSite = LookUp<GatheringSite, GatheringSiteID>.FindByID(snapshotGatheringSite);
		}
		showStatusRegulator = CreateShowStatusIconRegulator("Entity", EntityType);
		if (IsOnPlaySite() && EntityType.RenderableTypeMode != null)
		{
			ConstructRenderableIfNull(initialize: true);
			Renderable.SetToParentLocation();
			Renderable.UpdateAnimationConditionState();
			Renderable.ComputeMatricesForDrawing();
		}
		if (bioSystemsRegulator != null)
		{
			bioSystemsRegulator.LoadPostProcess(sn);
		}
		bool flag = footprintIsDirty;
		ReplaceSimState();
		CreateCollidable();
		footprintIsDirty = flag;
	}

	private void RemoveFromAgentQuadTree()
	{
		if (EntityType.IntelligenceType != null && The.AgentQuadTree != null)
		{
			The.AgentQuadTree.RemoveObject(this);
		}
	}

	public bool CanDefendItself()
	{
		if (EntityType.IntelligenceType.AttackTypes != null)
		{
			if (EntityType.IntelligenceType.AttackTypes.Count != 0)
			{
				return true;
			}
			return false;
		}
		return false;
	}

	public bool CanCommunicate(CommunicationMethod method, double distance)
	{
		if (EntityType.CommunicatorType != null && EntityType.CommunicatorType.Method == method)
		{
			Find<Communicator>(out var c);
			if (c.IsCommunicatorWorkingAndInRange(distance))
			{
				return true;
			}
		}
		return false;
	}

	public void AddProcess(SimProcess process)
	{
		if (!HasProcess(process))
		{
			Common.AddToList(ref processes, process.ID);
		}
	}

	public void RemoveProcess(SimProcessID id)
	{
		if (processes != null)
		{
			processes.Remove(id);
		}
	}

	public bool HasProcess(SimProcess process)
	{
		if (Processes != null)
		{
			return Processes.Contains(process.ID);
		}
		return false;
	}

	public void LogInjuryStatistics(Entity attacker)
	{
		string description = "Injured by " + attacker.ToString();
		LogInjuryDescription(description);
	}

	public void LogInjuryDescription(string description)
	{
		if (Intelligence.Statistics != null)
		{
			Intelligence.Statistics.AddViolentEvent(this, description, ViolentEventType.Injury);
		}
		Intelligence.Allegiance.Statistics.AddViolentEvent(this, description, ViolentEventType.Injury);
	}

	public float GetRepairProgress(RepairAction repairAction)
	{
		return NonLivingEntity.GetRepairProgress(repairAction);
	}

	public float CalculateSpeed(float bulk)
	{
		return Locomotor.CalculateSpeed(bulk);
	}

	public void ImpairMovement(float aMovementPartToRemove)
	{
		Locomotor.ImpairMovement(aMovementPartToRemove);
	}

	public bool IsCarrying(EntityID itemToCheckID)
	{
		return Contains.Contains(itemToCheckID);
	}

	public PassengerOrCargoSlot GetFreeDriversSlot()
	{
		if (Find<Vehicle>(out var c))
		{
			return c.GetFreeDriversSlot();
		}
		return null;
	}

	public List<PassengerOrCargoSlot> GetCargoSlotsForLoading(float bulkToLoad)
	{
		if (Find<Vehicle>(out var c))
		{
			return c.GetCargoSlotsForLoading(bulkToLoad);
		}
		return null;
	}

	public List<PassengerOrCargoSlot> GetCargoSlotsForUnloading(float bulkToLoad)
	{
		if (Find<Vehicle>(out var c))
		{
			return c.GetCargoSlotsForUnloading(bulkToLoad);
		}
		return null;
	}

	public bool IsUnassigned(SharedKnowledge sharedKnowledge)
	{
		return MemoryFact.IsUnassigned(this, sharedKnowledge);
	}

	public bool IsUnassignedToAnythingButThisJob(Job job, SharedKnowledge sharedKnowledge)
	{
		return MemoryFact.IsUnassignedToAnythingButThisJob(this, job, sharedKnowledge);
	}

	public static bool CanBeHauled(IKnownEntityData entityData)
	{
		if (!entityData.PartOfID.HasValue && GoalEvaluator.IsOnPlaySite(entityData) && entityData.IsCompleted() && !Item.IsImmovable(entityData.Bulk) && !entityData.Replenishes.HasValue && !entityData.UpgradeFor.HasValue)
		{
			return true;
		}
		return false;
	}

	public bool CanBeHauled()
	{
		return CanBeHauled(this);
	}

	public bool IsItemValidForHauling(Entity haulingAgent, Intelligence entityIntelligence, HaulingJob job)
	{
		return IsItemValidForHauling(haulingAgent, entityIntelligence, job, this);
	}

	public static bool IsItemValidForHauling(Entity haulingAgent, Intelligence entityIntelligence, HaulingJob job, IKnownEntityData itemData)
	{
		Entity entity = itemData as Entity;
		bool flag = true;
		if (entity != null)
		{
			flag = entity.Item.OKToTakeThisItemFromCarrier != false || haulingAgent.AgentStorage.Contains(entity);
		}
		Job job2 = EvaluateJob.ResolveAssignedToJob(itemData);
		if (itemData.NotOnboardDrivenVehicle && flag && itemData.CanBeHauled() && itemData.OwnedBy.HasValue && StorageHasRoomForItem(entityIntelligence.Allegiance.SharedKnowledge, job, itemData) && (job2 == null || job2 is HaulingJob) && haulingAgent.AgentStorage.ItemStorage.HasCapacityForItemWhenEmpty(itemData.Bulk) && !job.IsStoredInTarget(itemData))
		{
			return true;
		}
		return false;
	}

	public static bool StorageHasRoomForItem(SharedKnowledge sharedKnowledge, HaulingJob job, IKnownEntityData anEntityToBeHauled)
	{
		EntityID? getToStorageEntity = job.GetToStorageEntity;
		if (getToStorageEntity.HasValue && !GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(getToStorageEntity.Value, out var data)))
		{
			Storage storage = data.FindStorage(job.GetToStorageID.Value);
			if (storage != null && !storage.HasCapacityForItem(anEntityToBeHauled.Bulk))
			{
				return false;
			}
		}
		return true;
	}

	public bool HasStance()
	{
		return EntityType.HasStance();
	}

	public bool ContainsEntity(EntityID entity)
	{
		if (Contains != null)
		{
			return Contains.Contains(entity);
		}
		return false;
	}

	public Storage FindStorage(StorageID storageID)
	{
		if (Contains != null && Contains is IStorage storage)
		{
			return storage.FindStorage(storageID);
		}
		return null;
	}

	public bool IsTradeOfferStorage(StorageID storageID)
	{
		if (FindCompartment(storageID) == StorageCompartment.OfferedForTrade)
		{
			return true;
		}
		return false;
	}

	public StorageCompartment? FindCompartment(StorageID storageID)
	{
		if (Contains != null && Contains is IStorage storage)
		{
			return storage.GetCompartment(storageID);
		}
		return null;
	}

	private Entity GetRootAsEntity()
	{
		return GetRoot() as Entity;
	}

	public Entity GetRootAndContainer()
	{
		Entity rootAsEntity = GetRootAsEntity();
		GetContainedBy(out Entity container);
		if (container != null)
		{
			return container.GetRootAndContainer();
		}
		return rootAsEntity;
	}

	private void DeprecateMemoryFactsInRadius()
	{
		Point? point = MapPosition;
		if (!point.HasValue)
		{
			return;
		}
		int deprecateMemoryFactsWithinTileRadius = GameData.Instance.AIConstants.DeprecateMemoryFactsWithinTileRadius;
		int num = The.Map.ClampTileMapXPosition(point.Value.X - deprecateMemoryFactsWithinTileRadius);
		int num2 = The.Map.ClampTileMapXPosition(point.Value.X + deprecateMemoryFactsWithinTileRadius);
		int num3 = The.Map.ClampTileMapXPosition(point.Value.Y - deprecateMemoryFactsWithinTileRadius);
		int num4 = The.Map.ClampTileMapXPosition(point.Value.Y + deprecateMemoryFactsWithinTileRadius);
		for (int i = num; i <= num2; i++)
		{
			for (int j = num3; j <= num4; j++)
			{
				The.Map.TileMap[i][j].DeprecateMemoryFactsOnTile(this, TerrainTile.DeprecateDistance.Near);
			}
		}
	}

	private void UpdateShapeLocation(Collidable<Entity> shapes, Vector2 value)
	{
		if (shapes != null)
		{
			shapes.Center = value;
		}
	}

	public void SetRotationAndDir(float rotation)
	{
		Rotation = rotation;
		FacingNormal = new Vector3((float)Math.Cos(rotation), (float)Math.Sin(rotation), 0f);
	}

	public bool Find<T>(out T c) where T : Component
	{
		Type typeFromHandle = typeof(T);
		if (Components.TryGetValue(typeFromHandle, out var value))
		{
			c = (T)value;
			return true;
		}
		c = null;
		return false;
	}

	public static string GetExceptionInformation(IKnownEntityData data)
	{
		string text = "\n Whoops - fatal error. Press Ctrl-C to copy the contents of this dialog and paste the text into the forums: \n";
		if (data != null)
		{
			string text2 = data.ToString();
			bool hasValue = data.ContainedBy.HasValue;
			text = ((!(data is MemoryFact)) ? (text + "Data is Entity \n") : (text + "Data is MemoryFact \n"));
			text = ((!data.MapPosition.HasValue) ? (text + "Map Position is null.") : (text + "Map Position X:" + data.MapPosition.Value.X + " Y:" + data.MapPosition.Value.Y + "\n"));
			text = ((!data.Location.HasValue) ? (text + "Location is null.") : (text + "Location X:" + data.Location.Value.X + " Y:" + data.Location.Value.Y + "\n"));
			text = text + "Is inside container: " + hasValue + "\n";
			text = text + text2 + "\n";
			text = text + "ID: " + data.EntityID.ToString() + "\n";
			if (FindByID(data.EntityID) == null)
			{
				return text + "Entity has been removed from entity factory. Entity is null \n";
			}
			return text + "Entity still exists in entity factory. Entity is not null \n";
		}
		return "Entity/memoryFact is null \n";
	}

	public void CreateParts()
	{
		NonLivingEntity?.CreateParts();
	}

	public void SetLocationPropertiesForLeaf()
	{
		Location = null;
	}

	public void SetPart(Entity newPart)
	{
		NonLivingEntity?.SetPart(newPart);
	}

	public void RemovePart(Entity part, bool setPartOfToNull = true)
	{
		NonLivingEntity?.RemovePart(part, setPartOfToNull);
	}

	public EntityAndRoot GetAsEntityAndRoot()
	{
		Entity rootAsEntity = GetRootAsEntity();
		return new EntityAndRoot(ID, rootAsEntity.ID);
	}

	public Entity GetRootEntity()
	{
		if (PartOf == null)
		{
			return this;
		}
		return (Entity)PartOf.GetRoot();
	}

	public IComposite GetRoot()
	{
		if (PartOf == null)
		{
			return this;
		}
		return PartOf.GetRoot();
	}

	public void SetConditionDirty()
	{
		if (Find<NonLivingEntity>(out var c))
		{
			c.SetConditionDirty();
		}
	}

	public void SetBrokenPart()
	{
		bool partIsBroken = PartIsBroken;
		PartIsBroken = true;
		if (EntityType.SensorType != null)
		{
			Find<Sensor>(out var c);
			c.NotifyPartIsBroken();
		}
		if (PartOf != null)
		{
			PartOf.SetBrokenPart();
		}
		else if (!partIsBroken)
		{
			The.Client.LogIsBroken(this);
		}
		UpdateFunctionality();
	}

	public void SyncWithMemoryFact(Allegiance allegiance, MemoryFact memoryFact)
	{
		if (allegiance.AllegianceType == AllegianceType.Player)
		{
			AssignedToJob = memoryFact.AssignedToJob;
			Residents = memoryFact.Residents;
			List<HouseholdID> households = Households;
			if (households != null)
			{
				households.Clear();
				households.AddRange(memoryFact.Households);
			}
			OwnedBy = memoryFact.OwnedBy;
		}
		DebugLog.Add("Synced with memory fact.");
		DebugLog.Entries.AddRange(memoryFact.DebugLog.Entries);
	}

	public void ClearContainedBy()
	{
		if (containedBy.HasValue)
		{
			FindByID(containedBy.Value)?.Contains.Remove(this);
		}
	}

	public bool GetVehicle(out Entity vehicle)
	{
		if (!GetContainedBy(out vehicle))
		{
			return false;
		}
		if (vehicle != null)
		{
			Container contains = vehicle.Contains;
			if (contains != null && !contains.IsDriver(this))
			{
				vehicle = null;
			}
		}
		return true;
	}

	public bool GetDrivenVehicle(out Entity vehicle)
	{
		if (!GetContainedBy(out vehicle))
		{
			return false;
		}
		if (vehicle != null)
		{
			Container contains = vehicle.Contains;
			if (contains != null && !contains.IsDriver(this))
			{
				vehicle = null;
			}
		}
		return true;
	}

	public bool IsInAVehicle()
	{
		if (!GetContainedBy(out Entity container))
		{
			return false;
		}
		if (container != null)
		{
			Container contains = container.Contains;
			if (contains != null && contains.IsDriverOrPassenger(this))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsPermanentStorage(EntityType entityType)
	{
		if (entityType.IntelligenceType != null || entityType.LocomotorType != null)
		{
			if (entityType.ContainerType != null)
			{
				return entityType.ContainerType is TerminalContainerType;
			}
			return false;
		}
		return true;
	}

	public bool CarriedByAgent(out Entity carrier)
	{
		carrier = null;
		if (GetContainedBy(out Entity container))
		{
			if (container != null && container.Contains is AgentStorage)
			{
				carrier = container;
			}
			return true;
		}
		return false;
	}

	public bool StoredIn(out Storage storage)
	{
		storage = null;
		if (GetContainedBy(out Entity container))
		{
			if (container != null && container.Contains is IStorage storage2)
			{
				storage = storage2.GetStoredIn(this);
			}
			return true;
		}
		return false;
	}

	public bool GetReplenishes(out Entity replenishes)
	{
		replenishes = null;
		if (GetContainedBy(out Entity container))
		{
			if (container != null && container.Contains is IReplenishes replenishes2 && replenishes2.IsReplenishing(ID))
			{
				replenishes = container;
			}
			return true;
		}
		return false;
	}

	public bool GetUpgradesFor(out Entity upgrades)
	{
		upgrades = null;
		if (GetContainedBy(out Entity container))
		{
			if (container != null && container.Contains is IUpgrades upgrades2 && upgrades2.IsUpgrade(ID))
			{
				upgrades = container;
			}
			return true;
		}
		return false;
	}

	public bool GetContainedBy(out Entity container)
	{
		if (ContainedBy.HasValue)
		{
			container = FindByID(ContainedBy.Value);
			if (container == null)
			{
				HandleInvalidContainerEntityBug();
				return false;
			}
		}
		else
		{
			container = null;
		}
		return true;
	}

	public bool GetContainedBy(out Container container)
	{
		container = null;
		if (GetContainedBy(out Entity container2))
		{
			if (container2 != null)
			{
				container = container2.Contains;
			}
			return true;
		}
		return false;
	}

	private void HandleInvalidContainerEntityBug()
	{
		ContainedBy = null;
		Destroy();
	}

	public bool HasEnoughFuel(float neededFuel)
	{
		if (Contains != null && Contains is IHasReplenishItems { ReplenishItems: not null } hasReplenishItems && hasReplenishItems.ReplenishItems.RequiresFuel != null)
		{
			return hasReplenishItems.ReplenishItems.RequiresFuel.HasEnoughFuel(neededFuel);
		}
		return true;
	}

	public bool NeedsReload(SharedKnowledge sharedKnowledge, out IKnownEntityData itemToReload)
	{
		return NeedsReload(sharedKnowledge, this, out itemToReload);
	}

	public static bool NeedsReload(SharedKnowledge sharedKnowledge, IKnownEntityData entityData, out IKnownEntityData itemToReload)
	{
		itemToReload = null;
		if (entityData.EntityType.ContainerType != null && entityData.EntityType.ContainerType is MagazineContainerType)
		{
			int maxCapacity = ((MagazineContainerType)entityData.EntityType.ContainerType).MaxCapacity;
			int num = (int)(0.3f * (float)maxCapacity);
			int? totalAmmo = entityData.GetTotalAmmo();
			if (totalAmmo.HasValue && totalAmmo < num)
			{
				itemToReload = entityData;
				return true;
			}
			return false;
		}
		if (entityData.EntityType.IntelligenceType != null && entityData.EntityType.IntelligenceType.IntrinsicWeaponTypes != null)
		{
			foreach (KeyValuePair<EntityType, EntityID> intrinsicWeapon in entityData.IntrinsicWeapons)
			{
				if (!GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(intrinsicWeapon.Value, out var data)) && NeedsReload(sharedKnowledge, data, out itemToReload))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool NeedsRepair()
	{
		return NonLivingEntity.NeedsRepair();
	}

	public RepairPackage ComputeBestRepairPackage()
	{
		return NonLivingEntity.ComputeBestRepairPackage();
	}

	public bool HasEnoughAmmo(EntityType ammoType, int noOfRounds)
	{
		if (Contains != null && Contains is MagazineContainer magazineContainer)
		{
			return magazineContainer.HasAmmo(ammoType, noOfRounds);
		}
		return false;
	}

	public int? GetTotalAmmo()
	{
		if (Contains != null && Contains is MagazineContainer magazineContainer)
		{
			return magazineContainer.GetTotalAmmo();
		}
		return null;
	}

	public bool HasEnergyForDuration(float durationInDays)
	{
		if (Contains != null && Contains is IHasReplenishItems { ReplenishItems: not null } hasReplenishItems)
		{
			return hasReplenishItems.ReplenishItems.HasEnergyForDuration(durationInDays);
		}
		return true;
	}

	public Entity()
	{
	}

	public Entity(EntityType entityType, bool isStructureBeingPlaced = false, bool isItemBeingProduced = false)
	{
		EntityType = entityType;
		_ = EntityType.KeyName == "entity:dog";
		AddToLookup();
		((ILookUp<IComposite, CompositeID>)this).AddToLookup();
		((ILookUp<IDetectable, DetectableID>)this).AddToLookup();
		if (EntityType.IntelligenceType != null)
		{
			((ILookUp<ICanIterateEntities, CanIterateEntitiesID>)this).AddToLookup();
		}
		simStateFlags = new BitMask64(typeof(StateModifier));
		showStatusRegulator = CreateShowStatusIconRegulator("Entity", EntityType);
		Components = new Dictionary<Type, Component>();
		BulkChangedEvent = new IDActionEvent<float>();
		if (EntityType.BiologicalType != null)
		{
			Add(new BiologicalEntity(this));
		}
		if (EntityType.IntelligenceType != null)
		{
			Add(new Intelligence(this));
			if (EntityType.IntelligenceType.Skills != null)
			{
				foreach (KeyValuePair<string, float> skill in EntityType.IntelligenceType.Skills)
				{
					Intelligence.SetSkill(skill.Key, skill.Value);
				}
			}
		}
		if (EntityType.Person != null)
		{
			Add(new Person(this));
		}
		if (EntityType.ContainerType != null)
		{
			Container container = null;
			container = EntityType.ContainerType.CreateContainer(this);
			if (container != null)
			{
				Contains = container;
			}
		}
		if (EntityType.HeatingType != null)
		{
			Add(new Heating());
		}
		if (EntityType.ThreatType != null)
		{
			Add(new Threat(this));
		}
		if (EntityType.ToolType != null)
		{
			UWGame.SimSide.Items.Tool c = new UWGame.SimSide.Items.Tool(this);
			Add(c);
		}
		if (EntityType.LocomotorType != null)
		{
			Locomotor c2 = new Locomotor(this);
			Add(c2);
		}
		if (EntityType.StructureType != null)
		{
			Add(new Structure(this));
			if (isStructureBeingPlaced)
			{
				Structure.State = StructureStates.BeingPlaced;
			}
		}
		if (EntityType.TreeType != null)
		{
			if (EntityType.GatheringSiteType != null)
			{
				GatheringSite = new GatheringSite(this);
			}
			Tree c3 = new Tree(this);
			Add(c3);
		}
		if (EntityType.ItemType != null)
		{
			Add(new Item(this));
		}
		if (EntityType.TerrainType != null && EntityType.TerrainType.PathType != null)
		{
			Add(new TerrainPath(this));
		}
		if (EntityType.DirectionalLayoutType != null)
		{
			DirectionalLayout = new DirectionalLayout(this);
		}
		if (EntityType.PointLayoutType != null)
		{
			PointLayout = new PointLayout(this);
		}
		AdoptSimStateInfo(EntityType.DefaultSimState);
		if (Collidable == null)
		{
			CreateCollidable();
		}
		AvailableSharedSpecialActions = new List<ProcessType>();
		if (EntityType.SharedSpecialActionTypes != null)
		{
			foreach (ProcessType sharedSpecialActionType in EntityType.SharedSpecialActionTypes)
			{
				if (sharedSpecialActionType.SpecialActionEnabledAtStart == true)
				{
					AvailableSharedSpecialActions.Add(sharedSpecialActionType);
				}
			}
		}
		if (EntityType.GatheringSiteType != null)
		{
			GatheringSite = new GatheringSite(this);
		}
		if (EntityType.NonLivingType != null || EntityType.StructureType != null)
		{
			Add(new NonLivingEntity(this));
		}
		if (EntityType.RockType == null && EntityType.TreeType == null)
		{
			Add(new SimEffectsComponent(this));
		}
		if (EntityType.SensorType != null)
		{
			Add(new Sensor(this));
		}
		if (EntityType.SubstancesType != null)
		{
			Add(new SubstanceComponent(this));
		}
		if (EntityType.CommunicatorType != null)
		{
			Add(new Communicator(this));
		}
		if (EntityType.BodyType != null)
		{
			BodyComponent c4 = new BodyComponent(this);
			Add(c4);
		}
		if ((Structure == null || !isStructureBeingPlaced) && !isItemBeingProduced)
		{
			CreateParts();
		}
		CreateBioSystemsRegulator();
		if (EntityType.CustomFields != null)
		{
			foreach (KeyValuePair<string, PropertyResult> customField in EntityType.CustomFields)
			{
				SetPropertyValue(ref CustomFields, customField.Key, customField.Value);
			}
		}
		SetRotationAndDir((float)(The.Sim.GameplayRandomGenerator.NextDouble("Entity") * 6.2831854820251465));
		ConstructDumbRenderableIfNull();
		The.Sim.AddEntity(this);
		EntityType.EventActions.TryGetValue(EntityEventHooks.Created, out var value);
		Goal.FireEventActions(this, null, value);
	}

	public void CreateBioSystemsRegulator()
	{
		if (EntityType.BiologicalType != null)
		{
			bioSystemsRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 1.0 / (double)GameData.Instance.Constants.UpdateIntervalForBioEntity, "BiologicalEntity");
		}
	}

	public static List<ProcessType> GetSharedSpecialActionsForDisplay(IKnownEntityData entityData)
	{
		if (entityData.EntityType.SharedSpecialActionTypes != null && entityData.EntityType.SharedSpecialActionTypes.Count > 0)
		{
			List<ProcessType> list = new List<ProcessType>();
			{
				foreach (ProcessType sharedSpecialActionType in entityData.EntityType.SharedSpecialActionTypes)
				{
					if (!sharedSpecialActionType.ShowDisabledSpecialAction)
					{
						if (entityData.AvailableSharedSpecialActions.Contains(sharedSpecialActionType))
						{
							list.Add(sharedSpecialActionType);
						}
					}
					else
					{
						list.Add(sharedSpecialActionType);
					}
				}
				return list;
			}
		}
		return null;
	}

	public static Regulator CreateShowStatusIconRegulator(string source, EntityType entityType)
	{
		if (entityType.GetShowMarkerWindowMode() == EntityType.ShowMarkerWindowMode.ByStatus)
		{
			return new Regulator(The.Client.ClientRandomGenerator, 1.0, source, Regulator.Modes.Client);
		}
		return null;
	}

	public bool IsTimeToShowStatusMarkerWindow()
	{
		return showStatusRegulator.IsReady();
	}

	private void ConstructRenderableIfNull(bool initialize = false)
	{
		if (Renderable != null)
		{
			return;
		}
		if (The.Client == null)
		{
			ConstructDumbRenderableIfNull();
			return;
		}
		Renderable = RenderableFactory.Produce(this, EntityType.RenderableTypeMode, snapshotRenderable);
		bool updatePropertiesFromEntity = true;
		if (snapshotRenderable != null)
		{
			updatePropertiesFromEntity = false;
		}
		if (initialize)
		{
			Renderable.Initialize(updatePropertiesFromEntity);
		}
	}

	private void ConstructDumbRenderableIfNull()
	{
	}

	public bool ContentCanBeAccessedBy(Entity accessingEntity)
	{
		if (accessingEntity != this)
		{
			return EntityType.ContainerType.CanTransactWithContainer(accessingEntity.EntityType);
		}
		return true;
	}

	public bool HasBeenPlaced()
	{
		return mapPosition.HasValue;
	}

	public void Add(Component c)
	{
		Components.Add(c.GetType(), c);
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (PersonEntity != null)
		{
			stringBuilder.Append(Name);
			stringBuilder.Append(" ");
		}
		else
		{
			stringBuilder.Append(EntityType.Name);
		}
		return stringBuilder.ToString();
	}

	public float GetAttackRange(out float? coneWidth, out float? coneLength)
	{
		coneWidth = null;
		coneLength = null;
		Container contains = Contains;
		if (contains != null && contains is AgentStorage)
		{
			return ((AgentStorage)contains).GetMountedWeaponRange(out coneWidth, out coneLength);
		}
		if (EntityType.IntelligenceType != null && Intelligence.IntrinsicWeapons != null)
		{
			foreach (KeyValuePair<EntityType, EntityID> intrinsicWeapon in Intelligence.IntrinsicWeapons)
			{
				Entity entity = FindByID(intrinsicWeapon.Value);
				if (entity != null)
				{
					return entity.GetWeaponRange(ref coneWidth, ref coneLength);
				}
			}
		}
		return 0f;
	}

	public float GetWeaponRange(ref float? coneWidth, ref float? coneLength)
	{
		float num = 0f;
		if (EntityType.ItemType.WeaponType != null)
		{
			AttackType[] attackTypes = EntityType.ItemType.WeaponType.AttackTypes;
			foreach (AttackType attackType in attackTypes)
			{
				if (attackType.MaxRange.HasValue && attackType.MaxRange > num)
				{
					num = attackType.MaxRange.Value;
					if (attackType.AreaAttack != null && attackType.AreaAttack is ConeAttack coneAttack)
					{
						coneWidth = coneAttack.WidthInDegrees;
						coneLength = coneAttack.Length;
					}
				}
			}
		}
		return num;
	}

	public string ToLink(bool useUpperCase = false)
	{
		string text = GetDisplayName();
		if (useUpperCase)
		{
			text = text.ToUpper(Config.Culture);
		}
		return Hyperlink.ToLink(text, (long)id);
	}

	public void InitializeModelAndOnScreenFunctionality()
	{
		if (IsOnPlaySite() && EntityType.RenderableTypeMode != null)
		{
			ConstructRenderableIfNull(initialize: true);
		}
		if (Renderable != null && Renderable.RenderAsModel != null)
		{
			if (Renderable.RenderAsModel.ModelData.HasSteerableFrontWheels)
			{
				Add(new SteerableFrontWheels(this));
			}
			if (EntityType.KeyName == "entity:forestGuardian")
			{
				AttachPoint backAttachor = Renderable.RenderAsModel.ModelData.BackAttachor;
				Renderable freeAttachableRenderable = The.Client.Renderer.GetFreeAttachableRenderable("bush");
				RenderAsModel.GetAttachTransformations(freeAttachableRenderable, backAttachor, AttacheePoint.Back, null, out var attacheePoint, out var translation, out var vector);
				Renderable.AttachObject(freeAttachableRenderable.RenderAsModel, backAttachor, attacheePoint, AttacheePoint.Back, translation, vector);
			}
		}
		if (Parts == null)
		{
			return;
		}
		foreach (Entity part in Parts)
		{
			part.InitializeModelAndOnScreenFunctionality();
		}
	}

	public void Initialize(Site site, Allegiance allegiance = null, Expedition expedition = null, ThreatGroup threatGroup = null)
	{
		if (!isInitialized)
		{
			isInitialized = true;
			Site = site;
			if (Body != null)
			{
				Body.Initialize();
			}
			if (BiologicalEntity != null)
			{
				BiologicalEntity.Initialize();
			}
			if (EntityType.IntelligenceType != null)
			{
				Intelligence.Initialize(allegiance);
			}
			if (EntityType.Person != null)
			{
				PersonEntity.Household = new Household(expedition, this);
			}
			if (EntityType.ThreatType != null && Find<Threat>(out var c))
			{
				if (threatGroup == null && allegiance != null)
				{
					threatGroup = allegiance.ThreatGroup;
				}
				c.ThreatGroup = threatGroup;
			}
			if (Find<Tree>(out var c2))
			{
				c2.Initialize();
			}
			if (Find<Item>(out var c3))
			{
				c3.Initialize();
			}
			if (EntityType.Person != null)
			{
				PersonEntity.Initialize();
			}
		}
		if (Parts == null)
		{
			return;
		}
		foreach (Entity part in Parts)
		{
			part.Initialize(site, allegiance, expedition, threatGroup);
		}
	}

	public static Entity CreateAndInitEntity(EntityType entityType, Site site, List<Entity> parts = null, float? bulk = null, Allegiance allegiance = null)
	{
		Entity entity = new Entity(entityType, isStructureBeingPlaced: false, isItemBeingProduced: true);
		if (entity.NonLivingEntity != null)
		{
			entity.NonLivingEntity.SetPartsOrCreateNew(parts);
		}
		if (bulk.HasValue)
		{
			entity.Bulk = bulk.Value;
		}
		entity.Initialize(site, allegiance);
		entity.InitializeModelAndOnScreenFunctionality();
		return entity;
	}

	public void SetPosition(Vector3 location)
	{
		Point pos = MapManager.WorldPosToTile(location);
		Common.Direction value = The.Map.WorldLocationToDirectionWithinTile(location);
		SetPosition(pos, value, location);
	}

	public void SetPosition(Point pos, Common.Direction dir)
	{
		SetPosition(pos, dir, MapManager.TileAndDirectionToWorldPos(pos, dir));
	}

	public void SetPosition(Point pos, Common.Direction? dir, Vector3 location)
	{
		if (DirectionalLayout != null)
		{
			if (EntityType.DirectionalLayoutType.Fixed8DirPlacement)
			{
				Location = MapManager.GetWorldCoordsFromDirection(pos, dir.Value);
			}
			else
			{
				Location = location;
			}
			DirectionalLayout.EdgePosition = dir.Value;
		}
		else
		{
			if ((EntityType.PointLayoutType != null && EntityType.PointLayoutType.GridAlignedPlacement) || (CurrentSimState != null && CurrentSimState.GeometryLayoutType != null && CurrentSimState.GeometryLayoutType.GridAlignedPlacement))
			{
				location = MapManager.SubTileToWorldPos3(MapManager.WorldPosToSubtile(location));
			}
			Location = location;
		}
		Renderable.LocationChanged();
	}

	public bool DoDamage(float damage)
	{
		bool flag = false;
		if (IsCompositeRoot)
		{
			flag = NonLivingEntity.DoIntegrityDamage(damage);
		}
		if (Parts != null)
		{
			for (int num = Parts.Count - 1; num >= 0; num--)
			{
				flag = Parts[num].DoDamage(damage) || flag;
			}
		}
		else
		{
			flag = NonLivingEntity.DoConditionDamage(damage, null) || flag;
		}
		NonLivingEntity.ComputeConditionOfComposite();
		return flag;
	}

	public void TransferToPlaySite(Vector3? location, Entity container, StructureState? structureState, Expedition expedition)
	{
		PlaceEntityOnPlaySite(location, container, structureState, null, expedition);
		ComeOnline();
	}

	public void TransferToOtherSite(Site site, Entity container, Expedition expedition)
	{
		PlaceEntityOnOtherSite(site, container, false, null, expedition);
	}

	public void PlaceEntityOffSite(GeodeticCoordinate newCoords)
	{
		if (MapPosition.HasValue)
		{
			foreach (Allegiance item in The.Map.GetTile(MapPosition.Value).AllegiancesThatSeeThisTile)
			{
				IKnownEntityData data;
				if (Intelligence != null && Intelligence.Allegiance == item)
				{
					if (Communicates.IsInCommunicationRange(item, this, out var _, null, newCoords))
					{
						DeletePlaySiteKnowledgeOfEntity(item);
						continue;
					}
					DeletePlaySiteKnowledgeOfEntity(item);
					HandleEntityMovingOutOfCommunicationRange(item);
				}
				else if (item.SharedKnowledge.GetKnownData(ID, out data) == EntityResult.SeenDirectly)
				{
					item.SharedKnowledge.UnSeeEntity(this);
				}
			}
			if (Find<Sensor>(out var c))
			{
				c.UnseeTilesInRange();
			}
			DisableCollisions();
			RemoveFromAgentQuadTree();
			if (Contains != null)
			{
				Contains.IterateContained(delegate(Entity e)
				{
					e.RemoveFromAgentQuadTree();
					e.DeleteAttachedTriggers();
				});
			}
			DeleteAttachedTriggers();
			Site = null;
			Coords = newCoords;
			Location = null;
			if (EntityType.IntelligenceType != null && Intelligence.Allegiance.Site.IsPlaySite && OwnedBy.HasValue)
			{
				IOwner owner = LookUpOwners.FindByID(OwnedBy);
				if (owner != null && owner is Expedition newExpedition && owner.Allegiance != Intelligence.Allegiance)
				{
					ChangeExpedition(newExpedition, simulateJoinedNow: true);
				}
			}
			Renderable.RecomputeUpdateInterval(out var _);
			if (EntityType.IntelligenceType != null)
			{
				Intelligence.Brain.RemoveAllSubgoals();
			}
			ResetPlaySiteRegulators();
			if (Contains != null)
			{
				Contains.ResetPlaySiteRegulators();
			}
			foreach (KeyValuePair<Type, Component> component in Components)
			{
				component.Value.ResetPlaySiteRegulators();
			}
		}
		RecomputeUpdateIntervalOnLeafs();
	}

	private void ResetPlaySiteRegulators()
	{
		CreateBioSystemsRegulator();
	}

	private void DeletePlaySiteKnowledgeOfEntity(Allegiance allegiance)
	{
		allegiance.SharedKnowledge.DeletePlaySiteKnowledgeOfEntity(ID);
		IterateLeafs(delegate(Entity leafEntity)
		{
			allegiance.SharedKnowledge.DeletePlaySiteKnowledgeOfEntity(leafEntity.ID);
		}, allegiance);
	}

	private void IterateLeafs(Action<Entity> action, Allegiance detectingAllegiance)
	{
		if (EntityType.ContainerType != null && (detectingAllegiance == null || detectingAllegiance.SharedKnowledge.CanSeeInsideContainer(this)))
		{
			Contains.IterateContained(action);
		}
		if (Parts == null)
		{
			return;
		}
		foreach (Entity part in Parts)
		{
			action(part);
		}
	}

	public void HandleEntityMovingOutOfCommunicationRange(Allegiance allegiance)
	{
		DeleteMemoryOfEntityAndLeafs(allegiance);
	}

	private void DeleteMemoryOfEntityAndLeafs(Allegiance allegiance)
	{
		allegiance.SharedKnowledge.DeleteMemoryOfEntity(ID, DetectableID, removeAllKnowledge: true);
		IterateLeafs(delegate(Entity leafEntity)
		{
			allegiance.SharedKnowledge.DeleteMemoryOfEntity(leafEntity.ID, leafEntity.DetectableID, removeAllKnowledge: true);
		}, allegiance);
	}

	public bool PlaceEntityOnOtherSite(Site otherSite, Entity container, bool? isFinishedStructure, SetOwnerInfo? setOwnerInfo, Expedition newExpedition, StorageCompartment? compartment = null, StorageCondition placeInStorage = null, bool simulateJoinedExpeditionNow = true)
	{
		Site = otherSite;
		SetOwnerAndExpedition(setOwnerInfo, newExpedition, simulateJoinedExpeditionNow);
		if (container != null && !container.Contains.AddToContain(this, compartment, placeInStorage))
		{
			return false;
		}
		RecomputeUpdateIntervalOnLeafs();
		return true;
	}

	public void PlaceEntityOnPlaySite(Vector3 location, AddRandomOffset addRandomOffset, Entity creatorOfItem, StructureState? structureState, SetOwnerInfo? setOwnerInfo, Expedition newExpedition = null, bool isProductionOutput = false, bool assertContainment = true, UpgradeCategory upgradeCategory = null)
	{
		location = MapManager.FindFreeLocation(location, EntityType.ItemType != null, addRandomOffset, creatorOfItem);
		PlaceEntityOnPlaySite(location, null, structureState, setOwnerInfo, newExpedition, null, null, isProductionOutput, null, assertContainment, simulateJoinedExpeditionNow: true, upgradeCategory);
	}

	public bool PlaceEntityOnPlaySite(Vector3? location, Entity container, StructureState? structureState, SetOwnerInfo? setOwnerInfo, Expedition newExpedition = null, StorageCompartment? placeProductsInCompartment = null, StorageCondition placeInStorage = null, bool isProductionOutput = false, EntityID? anchorID = null, bool assertContainment = true, bool simulateJoinedExpeditionNow = true, UpgradeCategory upgradeCategory = null)
	{
		ConstructRenderableIfNull(initialize: true);
		accessPoint = null;
		if (location.HasValue)
		{
			Site = The.Sim.PlaySite;
			Point pos = MapManager.WorldPosToTile(location.Value);
			Common.Direction direction = The.Map.WorldLocationToDirectionWithinTile(location.Value);
			SetPosition(pos, direction, location.Value);
			if (DirectionalLayout != null)
			{
				DirectionalLayout.Place(direction);
			}
			else if (PointLayout != null)
			{
				PointLayout.Place();
			}
			if (GeometryLayout != null)
			{
				if (Collidable == null)
				{
					CreateCollidable();
				}
				GeometryLayout.Place(GeoPlaceMode.NewFeature);
			}
			EnableCollisions();
		}
		else if (container != null && !container.Contains.AddToContain(this, placeProductsInCompartment, placeInStorage, ignoreCapacity: false, replenish: false, isProductionOutput, assertContainment, upgradeCategory))
		{
			return false;
		}
		if (GeometryLayout != null && Structure == null && !The.Sim.AllTerrainEntities.Contains(EntityID))
		{
			The.Sim.AllTerrainEntities.Add(EntityID);
		}
		if (EntityType.BiologicalType != null)
		{
			Trigger trigger = new Trigger(this, null, GameData.Instance.AllTriggerTypes["prey"]);
			AttachTrigger(trigger);
		}
		if (IsNonHumanAnimal())
		{
			Trigger trigger2 = new Trigger(this, null, GameData.Instance.AllTriggerTypes["creature"]);
			AttachTrigger(trigger2);
		}
		if (Structure != null)
		{
			Structure.PlaceBuilding();
			if (structureState == StructureState.Ordered)
			{
				Renderable.SetOverlayFlashing(1000f);
			}
			else if (structureState != StructureState.Finished)
			{
			}
			else
			{
				Structure.ConstructionFinished(anchorID, createParts: true);
			}
		}
		if (Find<Tree>(out var c))
		{
			c.Place();
		}
		if (Renderable != null)
		{
			Renderable.SetToParentLocation();
		}
		if (Find<Locomotor>(out var c2))
		{
			c2.CurrentMoveTarget = PlaySiteLocation;
		}
		if (EntityType.GatheringSiteType != null)
		{
			GatheringSite.SetLocation(PlaySiteLocation);
		}
		SetOwnerAndExpedition(setOwnerInfo, newExpedition, simulateJoinedExpeditionNow);
		SeeEntityPlacedInContainerByAllegiances();
		SeeOwnPartsForRobots();
		RecomputeUpdateIntervalOnLeafs();
		return true;
	}

	private void RecomputeUpdateIntervalOnLeafs()
	{
		RecomputeUpdateInterval();
		if (Parts != null)
		{
			foreach (Entity part in Parts)
			{
				part.RecomputeUpdateIntervalOnLeafs();
			}
		}
		if (Contains != null)
		{
			Contains.IterateContained(delegate(Entity e)
			{
				e.RecomputeUpdateIntervalOnLeafs();
			});
		}
	}

	private void SetOwnerAndExpedition(SetOwnerInfo? setOwnerInfo, Expedition newExpedition, bool simulateJoinedExpeditionNow)
	{
		if (setOwnerInfo.HasValue)
		{
			ChangeOwnership(setOwnerInfo.Value.NewOwner, setOwnerInfo.Value.GiveNewOwnerKnowledge);
			if (setOwnerInfo.Value.NewOwner != null && EntityType.IntelligenceType != null)
			{
				newExpedition = setOwnerInfo.Value.NewOwner as Expedition;
			}
		}
		if (newExpedition != null && EntityType.IntelligenceType != null)
		{
			ChangeExpedition(newExpedition, simulateJoinedExpeditionNow);
		}
	}

	public bool GetContainerOrLocation(ref Entity container, ref Vector3? location)
	{
		if (ContainedBy.HasValue)
		{
			return GetContainedBy(out container);
		}
		location = AccessPoint;
		return true;
	}

	private void SeeOwnPartsForRobots()
	{
		if (!IsOnPlaySite() || EntityType.IntelligenceType == null || Parts == null)
		{
			return;
		}
		foreach (Entity part in Parts)
		{
			Intelligence.Allegiance.SharedKnowledge.SeeDetectable(part, suppressClientFeedback: false, null, null, doAssert: false);
		}
	}

	public bool IsOwnedByUs(IKnownEntityData food)
	{
		if (!food.OwnedBy.HasValue || EntityType.Person == null)
		{
			return false;
		}
		Person personEntity = PersonEntity;
		if (((ILookUp<IOwner, OwnerID>)personEntity).ID == food.OwnedBy)
		{
			return true;
		}
		if (((ILookUp<IOwner, OwnerID>)Intelligence.CurrentExpedition).ID == food.OwnedBy)
		{
			return true;
		}
		if (((ILookUp<IOwner, OwnerID>)personEntity.Household).ID == food.OwnedBy)
		{
			return true;
		}
		return false;
	}

	public void ComeOnline(bool isSpawning = false)
	{
		if (Find<Intelligence>(out var c))
		{
			c.ComeOnline();
		}
		if (EntityType.CommunicatorType != null && Find<Communicator>(out var c2))
		{
			c2.GainContact();
		}
		if (EntityType.ContainerType != null && EntityType.ContainerType is TerminalContainerType)
		{
			(Contains as TerminalContainer).ComeOnline();
		}
		if (GetContainedBy(out Entity container))
		{
			container?.Contains.NotifyFunctionalContainedEntity(this);
		}
		if (IsOnPlaySite())
		{
			AttachTriggers(EntityType.Triggers);
			if (Find<Sensor>(out var c3))
			{
				c3.IsActive = true;
				c3.SeeTilesInRange(MapPosition.Value);
			}
			if (EntityType.PolledEvents != null && EntityType.PolledEvents.TryGetValue(Scope.Entity, out var value))
			{
				foreach (PolledEventType item in value)
				{
					if (IsOnPlaySite() || !item.PlaySiteOnly)
					{
						Site.EventManager.AddPolledEvent(item.KeyName, EntityID);
					}
				}
			}
			if (EntityType.IntelligenceType != null && The.AgentQuadTree != null)
			{
				if (!The.AgentQuadTree.Contains(this))
				{
					The.AgentQuadTree.AddObject(this, PlaySiteLocation.ToVector2());
				}
				else
				{
					The.AgentQuadTree.UpdateObject(this, PlaySiteLocation.ToVector2());
				}
			}
		}
		EntityType.EventActions.TryGetValue(EntityEventHooks.ComeOnline, out var value2);
		Goal.FireEventActions(this, null, value2, null, isSpawning);
		RecomputeUpdateInterval();
	}

	private void SeeEntityPlacedInContainerByAllegiances()
	{
		if (!ContainedBy.HasValue)
		{
			return;
		}
		Entity entity = FindByID(containedBy.Value);
		if (!entity.MapPosition.HasValue)
		{
			return;
		}
		foreach (Allegiance item in The.Map.GetTile(entity.MapPosition.Value).AllegiancesThatSeeThisTile)
		{
			if (item.SharedKnowledge.GetKnownData(ContainedBy.Value, out var _) == EntityResult.SeenDirectly && item.SharedKnowledge.CanSeeInsideContainer(entity))
			{
				item.SharedKnowledge.SeeDetectableIfRelevant(this);
			}
		}
	}

	public bool IsNonHumanAnimal()
	{
		if (BiologicalEntity == null)
		{
			return false;
		}
		if (Intelligence == null)
		{
			return false;
		}
		if (PersonEntity != null)
		{
			return false;
		}
		return true;
	}

	private void CreateCollidable()
	{
		Vector2? position = null;
		if (Location.HasValue)
		{
			position = Location.Value.ToVector2();
		}
		if (CurrentSimState != null && CurrentSimState.GeometryLayoutType != null)
		{
			if (CurrentSimState.GeometryLayoutType.Shapes != null)
			{
				Collidable = CreateCollidableFromShapes(this, CurrentSimState.GeometryLayoutType.Shapes, position);
			}
			if (CurrentSimState.GeometryLayoutType.SelectionShapes != null)
			{
				SelectionShape = CreateCollidableFromShapes(this, CurrentSimState.GeometryLayoutType.SelectionShapes, position);
			}
		}
		else if (EntityType.CollidableType != null)
		{
			if (EntityType.CollidableType.CircleRadius.HasValue)
			{
				float value = EntityType.CollidableType.CircleRadius.Value;
				Collidable = new Collidable<Entity>(this, position, new Vector2(value));
				Collidable.BeCircle();
			}
			else if (EntityType.CollidableType.Shapes != null)
			{
				Collidable = CreateCollidableFromShapes(this, EntityType.CollidableType.Shapes, position);
			}
		}
		else if (EntityType.IntelligenceType != null)
		{
			float value2 = 8f;
			Collidable = new Collidable<Entity>(this, position, new Vector2(value2));
			Collidable.BeCircle();
		}
		if (Collidable != null)
		{
			Collidable.FlipHorizontally = FlipHorizontally;
		}
		if (SelectionShape != null)
		{
			SelectionShape.FlipHorizontally = FlipHorizontally;
		}
	}

	private static Collidable<Entity> CreateCollidableFromShapes(Entity entity, CollideShape2D[] shapes, Vector2? location)
	{
		float value = 2f * shapes[0].Radius;
		Collidable<Entity> collidable = new Collidable<Entity>(entity, location, new Vector2(value));
		foreach (CollideShape2D child in shapes)
		{
			collidable.AddChildShape(child);
		}
		return collidable;
	}

	public SurfaceType.TransportType GetTransportType()
	{
		if (DrivingVehicle.HasValue)
		{
			return ((VehicleContainerType)FindByID(DrivingVehicle.Value).EntityType.ContainerType).Transport;
		}
		return SurfaceType.TransportType.Foot;
	}

	public static float GetSelectionRadius(IKnownEntityData entityData)
	{
		if (entityData.BoundingRadius3D < 24f)
		{
			return 24f;
		}
		return entityData.BoundingRadius3D;
	}

	public static bool ContainsIntelligence(IKnownEntityData entityData, SharedKnowledge sharedKnowledge, ref List<IKnownEntityData> agentsInside)
	{
		if (entityData.ContainedEntities != null)
		{
			for (int num = entityData.ContainedEntities.Count - 1; num >= 0; num--)
			{
				EntityID entityID = entityData.ContainedEntities[num];
				if (!GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(entityID, out var data)) && data.EntityType.IntelligenceType != null)
				{
					Common.AddToList(ref agentsInside, data);
					return true;
				}
			}
		}
		return false;
	}

	public bool CanBeHunted(Allegiance byAllegiance)
	{
		return CanBeHunted(this, byAllegiance);
	}

	public static bool CanBeHunted(IKnownEntityData entity, Allegiance byAllegiance)
	{
		if (entity != null && entity.EntityType.BiologicalType != null && entity.EntityType.IntelligenceType != null && entity.AllegianceID != byAllegiance.ID)
		{
			return true;
		}
		return false;
	}

	public bool CanSetStockpileSettings(EntityGroupID byOwner)
	{
		return CanSetStockpileSettings(this, byOwner);
	}

	public static bool CanSetStockpileSettings(IKnownEntityData entityData, EntityGroupID owner)
	{
		if (entityData.HasItemStorage && entityData.EntityType.StructureType != null && entityData.EntityType.ContainerType is IHasItemStorageType { AllowsStockpiling: not false })
		{
			EntityGroup entityGroup = LookUp<EntityGroup, EntityGroupID>.FindByID(owner);
			if (entityGroup != null)
			{
				return entityGroup.Contains(entityData);
			}
		}
		return false;
	}

	public bool CanSetTradeOfferSettings(EntityGroupID byOwner)
	{
		return CanSetTradeOfferSettings(this, byOwner);
	}

	public static bool CanSetTradeOfferSettings(IKnownEntityData entityData, EntityGroupID owner)
	{
		if (entityData.EntityType.ContainerType != null && entityData.EntityType.ContainerType is TerminalContainerType)
		{
			EntityGroup entityGroup = LookUp<EntityGroup, EntityGroupID>.FindByID(owner);
			if (entityGroup != null)
			{
				return entityGroup.Contains(entityData);
			}
		}
		return false;
	}

	public bool CanBeUpgraded(EntityGroupID byOwner)
	{
		return CanBeUpgraded(this, byOwner);
	}

	public static bool CanBeUpgraded(IKnownEntityData entityData, EntityGroupID owner)
	{
		if (entityData.EntityType.ContainerType != null && entityData.EntityType.ContainerType.CanBeUpgraded)
		{
			EntityGroup entityGroup = LookUp<EntityGroup, EntityGroupID>.FindByID(owner);
			if (entityGroup != null)
			{
				return entityGroup.Contains(entityData);
			}
		}
		return false;
	}

	public void AttachTrigger(Trigger trigger)
	{
		DeleteTriggerOfType(trigger.TriggerType);
		attachedTriggers.Add(trigger);
		The.Sim.TriggerSystem.RegisterTrigger(trigger);
	}

	public void DeleteTrigger(Trigger trigger)
	{
		attachedTriggers.Remove(trigger);
		if (The.Sim != null)
		{
			The.Sim.TriggerSystem.DeleteTrigger(trigger);
		}
	}

	public void DeleteTriggerOfType(TriggerType type)
	{
		for (int num = attachedTriggers.Count - 1; num >= 0; num--)
		{
			Trigger trigger = attachedTriggers[num];
			if (trigger.TriggerType == type)
			{
				DeleteTrigger(trigger);
			}
		}
	}

	public void DeleteTriggers(TriggerType[] triggerTypes)
	{
		if (triggerTypes != null)
		{
			foreach (TriggerType type in triggerTypes)
			{
				DeleteTriggerOfType(type);
			}
		}
	}

	public string HisHerOrIts()
	{
		if (EntityType.BiologicalType != null)
		{
			if (EntityType.Person == null)
			{
				return "its";
			}
			if (BiologicalEntity.CasteType.Reproduction == Reproduction.Male)
			{
				return "his";
			}
			if (BiologicalEntity.CasteType.Reproduction == Reproduction.Female)
			{
				return "her";
			}
		}
		return "its";
	}

	public Entity Kill(OwnerID? ownerOfCarcassID, CauseOfDeath? causeOfDeathToLog = null, EntityID? killer = null)
	{
		if (EntityType.BiologicalType == null)
		{
			return null;
		}
		if (causeOfDeathToLog.HasValue)
		{
			string causeOfDeath = LogDeathMessage(causeOfDeathToLog);
			LogDeathStatistics(causeOfDeathToLog, causeOfDeath);
		}
		Entity entity = null;
		entity = CreateAndInitEntity(EntityType.BiologicalType.CarcassType, Site, null, Bulk);
		if (EntityType.Person != null)
		{
			entity.SetRotationAndDir(Common.WrapAngleBetweenZeroAndTwoPi(Rotation + (float)Math.PI));
		}
		if (entity.EntityType.RenderableTypeMode != null)
		{
			Renderable.SetToParentLocation();
			Renderable.SetAnimationActionStateFlag(AnimAction.Dying);
			Renderable.SetAnimationStateFlag(AnimModifier.Post);
			Renderable.UpdateAnimationConditionState();
			entity.Renderable = RenderableFactory.Produce(Renderable, entity);
		}
		Entity entity2 = null;
		IOwner owner = null;
		if (Intelligence.Allegiance.AllegianceType == AllegianceType.Player && Intelligence.IsIndependent())
		{
			The.Sim.PlaySite.EventManager.PlayerEntityHasDied(this, entity, causeOfDeathToLog);
		}
		if (killer.HasValue)
		{
			entity2 = FindByID(killer.Value);
		}
		owner = LookUpOwners.FindByID(ownerOfCarcassID);
		Container container;
		try
		{
			if (entity2 != null && entity2.Intelligence != null)
			{
				if (owner != null && entity2.PersonEntity != null)
				{
					entity2.Intelligence.Memory.SetRecentlyHuntedCarcass(entity.EntityID);
				}
				entity2.Intelligence.Allegiance.Statistics.AddKillEvent(EntityType);
			}
			IsDead = true;
			string key = "entityDied";
			The.Sim.TriggerSystem.RegisterTrigger(new Trigger(null, Location, GameData.Instance.AllTriggerTypes[key], new Tuple<EntityID, EntityType>(EntityID, EntityType)));
			GetContainedBy(out container);
			_ = Location;
			if (Contains != null)
			{
				Contains.IterateContained(delegate(Entity e)
				{
					Contains.Uncontain(e);
				});
			}
		}
		catch (NullReferenceException innerException)
		{
			string text = "";
			try
			{
				text = string.Concat(text, "EntityType: ", EntityType, "\n");
				text = ((entity2 == null) ? (text + "Killer: null") : (text + "Killer: " + entity2.GetDisplayName()));
				text = ((entity == null) ? (text + "Carcass: null") : (text + "Carcass: " + entity.GetDisplayName()));
			}
			catch (Exception)
			{
			}
			throw new Exception("Kill error #4, " + text, innerException);
		}
		if (container != null)
		{
			container.SwitchEntities(this, entity);
		}
		else
		{
			entity.PlaceEntityOnPlaySite(location.Value, AddRandomOffset.No, null, null, new SetOwnerInfo(owner, GiveNewOwnerKnowledge.No));
			entity.Renderable.SetToParentLocation();
			owner?.Allegiance.LogProductionStatistics(entity, null);
		}
		Destroy();
		if (owner != null)
		{
			HaulingJobManager.CreateHaulingJobsForItemOutOfBand(entity, owner.OwnedEntities);
		}
		return entity;
	}

	private string LogDeathMessage(CauseOfDeath? causeOfDeathToLog)
	{
		string text = causeOfDeathToLog.Value switch
		{
			CauseOfDeath.Starvation => "starvation", 
			CauseOfDeath.Wounds => HisHerOrIts() + " wounds", 
			_ => "unknown causes", 
		};
		The.Client.AddLogEvent(Intelligence.Allegiance, The.Client.Log.GeneralEvent, this, "has died from " + text + ".");
		return text;
	}

	private void LogDeathStatistics(CauseOfDeath? causeOfDeathToLog, string causeOfDeath)
	{
		if (Intelligence.Allegiance.Statistics == null || !GroupStatistics.GatherStatisticsForEntity(this))
		{
			return;
		}
		foreach (KeyValuePair<RatingTypes, Rating> rating in Intelligence.Allegiance.Statistics.Ratings)
		{
			if (rating.Key == RatingTypes.Security && causeOfDeathToLog.Value == CauseOfDeath.Wounds)
			{
				((SecurityStatisticsForAllegiance)rating.Value).AddViolentEvent(this, "Died from " + causeOfDeath + ".", ViolentEventType.Death);
			}
			if (rating.Key == RatingTypes.Food && causeOfDeathToLog.Value == CauseOfDeath.Starvation)
			{
				FoodStatistics obj = (FoodStatistics)rating.Value;
				Common.AddToList(value: new DataPoint<EntityID>
				{
					Value = ID,
					Time = The.Sim.DateAndTime.CurrentTimeDateYear
				}, list: ref obj.starvingDeaths);
			}
		}
	}

	private void ResetAfterEmigrating(Allegiance oldAllegiance)
	{
		if (PersonEntity != null)
		{
			PersonEntity.Personality.Attraction.Remove(oldAllegiance.ID);
		}
	}

	public void ChangeExpedition(Expedition newExpedition, bool simulateJoinedNow)
	{
		if (newExpedition == null || EntityType.IntelligenceType == null)
		{
			return;
		}
		bool flag = false;
		if (Intelligence.Allegiance != null && Intelligence.Allegiance.AllegianceType == AllegianceType.Player)
		{
			flag = true;
		}
		if (newExpedition.Allegiance.ID != AllegianceID)
		{
			ChangeAllegiance(newExpedition.Allegiance);
			if (!flag && Intelligence.Allegiance.AllegianceType == AllegianceType.Player)
			{
				EntityType.IntelligenceType.EventActions.TryGetValue(AgentActionHooks.SwitchedToPlayerAllegiance, out var value);
				Goal.FireEventActions(this, null, value);
			}
		}
		Expedition currentExpedition = Intelligence.CurrentExpedition;
		if (currentExpedition != newExpedition)
		{
			currentExpedition?.RemoveMember(this);
			newExpedition.AddMember(this);
			if (PersonEntity != null)
			{
				currentExpedition?.RemoveHousehold(PersonEntity.Household);
				newExpedition.AddHousehold(PersonEntity.Household);
			}
			if (simulateJoinedNow)
			{
				Intelligence.Memory.SetTimepointForJoiningExpedition();
			}
		}
	}

	public void ChangeAllegiance(Allegiance newAllegiance)
	{
		Allegiance allegiance = Intelligence.Allegiance;
		Intelligence.Allegiance.RemoveMember(this, isDestroyed: false);
		Intelligence.Allegiance = newAllegiance;
		Intelligence.Allegiance.AddMember(this);
		Intelligence.Statistics.ChangeAllegiance(newAllegiance);
		ResetAfterEmigrating(allegiance);
	}

	public void ChangeOwnership(IOwner newOwner, GiveNewOwnerKnowledge giveNewOwnerKnowledge = GiveNewOwnerKnowledge.Yes)
	{
		ChangeOwnership(this, newOwner, giveNewOwnerKnowledge);
	}

	public static void ChangeOwnership(IKnownEntityData entityData, IOwner newOwner, GiveNewOwnerKnowledge giveNewOwnerKnowledge = GiveNewOwnerKnowledge.Yes)
	{
		if (!entityData.PartOfID.HasValue)
		{
			LookUpOwners.ResolveEntityOwner(entityData, out IOwner owner);
			ChangeOwnershipOnParts(entityData, newOwner);
			if (giveNewOwnerKnowledge == GiveNewOwnerKnowledge.Yes)
			{
				newOwner?.Allegiance.SharedKnowledge.AddKnowledgeOfItemToNewOwner((Entity)entityData);
			}
			if (entityData.ContainedUpgrades != null)
			{
				foreach (KeyValuePair<UpgradeCategory, EntityID> containedUpgrade in entityData.ContainedUpgrades)
				{
					ChangeOwnership(GetOwnedEntityData(containedUpgrade.Value, owner, newOwner), newOwner, giveNewOwnerKnowledge);
				}
			}
			if (entityData.Households == null || newOwner != null)
			{
				return;
			}
			for (int num = entityData.Households.Count - 1; num >= 0; num--)
			{
				Household household = LookUp<Household, HouseholdID>.FindByID(entityData.Households[num]);
				if (household != null && household.Home == entityData.EntityID)
				{
					Residence.RemoveHousehold(owner.Allegiance.SharedKnowledge, household);
				}
			}
			entityData.Households.Clear();
		}
		else
		{
			LookUpOwners.ResolveEntityOwner(entityData, out IOwner owner2);
			ChangeOwnership(GetOwnedEntityData(entityData.RootEntityID, owner2, newOwner), newOwner, giveNewOwnerKnowledge);
		}
	}

	public static void ChangeOwnershipOnParts(IKnownEntityData entityData, IOwner newOwner)
	{
		if (entityData == null)
		{
			return;
		}
		LookUpOwners.ResolveEntityOwner(entityData, out IOwner owner);
		if (owner != null)
		{
			if (newOwner != owner)
			{
				owner.OwnedEntities.DeleteEntity(entityData.EntityID, entityData.EntityType);
				entityData.OwnedBy = null;
				if (newOwner != null)
				{
					Entity entity = entityData as Entity;
					newOwner.OwnedEntities.AddEntity(entity);
					entity.OwnedBy = newOwner.ID;
				}
			}
		}
		else if (newOwner != null)
		{
			Entity entity2 = entityData as Entity;
			newOwner.OwnedEntities.AddEntity(entity2);
			entity2.OwnedBy = newOwner.ID;
		}
		if (entityData.PartIDs == null)
		{
			return;
		}
		foreach (EntityID partID in entityData.PartIDs)
		{
			if (owner != null || newOwner != null)
			{
				ChangeOwnershipOnParts(GetOwnedEntityData(partID, owner, newOwner), newOwner);
			}
		}
	}

	private static IKnownEntityData GetOwnedEntityData(EntityID entityID, IOwner oldOwner, IOwner newOwner)
	{
		SharedKnowledge sharedKnowledge = ((oldOwner == null) ? newOwner.Allegiance.SharedKnowledge : oldOwner.Allegiance.SharedKnowledge);
		sharedKnowledge.GetKnownData(entityID, out var data);
		return data;
	}

	public void ClearSpriteStateFlag(StateModifier state)
	{
		Renderable.ClearSpriteStateFlag(state);
		if (Renderable.ClearSpriteStateFlag(simStateFlags, state))
		{
			ReplaceSimState();
		}
	}

	public void SetSpriteStateFlag(StateModifier state)
	{
		Renderable.SetSpriteStateFlag(state);
		if (Renderable.SetSpriteStateFlag(simStateFlags, state))
		{
			ReplaceSimState();
		}
	}

	private void ReplaceSimState()
	{
		IStateInfo bestMatch = null;
		RenderableType.FindBestStaticInfo(simStateFlags, EntityType.SimStateConditions, EntityType.DefaultSimState, out bestMatch);
		if (bestMatch != null)
		{
			AdoptSimStateInfo((SimStateInfo)bestMatch);
		}
	}

	private void AdoptSimStateInfo(SimStateInfo newInfo)
	{
		if (newInfo == null)
		{
			newInfo = EntityType.DefaultSimState;
		}
		SimStateInfo currentSimState = CurrentSimState;
		CurrentSimState = newInfo;
		if (currentSimState != CurrentSimState)
		{
			AdoptGeoLayout(CurrentSimState, currentSimState);
		}
	}

	private void AdoptGeoLayout(SimStateInfo newInfo, SimStateInfo oldInfo)
	{
		if (newInfo.GeometryLayoutType != null)
		{
			if (Collidable != null)
			{
				Collidable.Delete();
				Collidable = null;
			}
			CreateCollidable();
			if (GeometryLayout == null)
			{
				GeometryLayout = new GeometryLayout(this);
			}
			FootprintIsDirty = true;
			SetAccessPointDirty();
			return;
		}
		if (GeometryLayout != null)
		{
			GeometryLayout.Destroy();
			GeometryLayout = null;
		}
		if (Collidable != null)
		{
			Collidable.Delete();
			Collidable = null;
		}
		if (oldInfo == null || oldInfo.GeometryLayoutType != null)
		{
			SetAccessPointDirty();
		}
	}

	public static bool IsOnPlaySite(IKnownEntityData entityData)
	{
		if (entityData.Site.HasValue)
		{
			return entityData.Site == The.Sim.PlaySite.ID;
		}
		return false;
	}

	public bool IsOnPlaySite()
	{
		return IsOnPlaySite(this);
	}

	public bool IsHaulingNothingMounted()
	{
		bool result = false;
		if (AgentStorage != null)
		{
			result = !AgentStorage.MountedToolOrWeapon.HasValue && AgentStorage.IsHauling;
		}
		return result;
	}

	public bool IsAttacking()
	{
		return Intelligence.CombatInfo.Target.HasValue;
	}

	public bool IsFleeing()
	{
		if (Locomotor != null && Locomotor.LeggedLocomotor != null)
		{
			return Locomotor.LeggedLocomotor.TargetSpeed == Goal.MovementSpeeds.Run;
		}
		return false;
	}

	public bool IsAwakeAndActive()
	{
		return Intelligence.IsAwakeAndActive;
	}

	public bool CanSeeEntity(Entity entity)
	{
		Entity entity2;
		return Intelligence.GetEntitySeenDirectly(entity.EntityID, out entity2);
	}

	public void SetSneaking(bool value)
	{
		if (value)
		{
			Locomotor.LeggedLocomotor.TargetSpeed = Goal.MovementSpeeds.WalkSlowly;
			Intelligence.IsStealthy = true;
		}
		else
		{
			Locomotor.LeggedLocomotor.TargetSpeed = Goal.MovementSpeeds.Normal;
			Intelligence.IsStealthy = false;
		}
	}

	public void GetStatus(out bool isDead, out bool isUnconscious, out CauseOfDeath? causeOfDeath, out CauseOfUnconsciousness? causeOfUnconsciousness)
	{
		bool isDead2 = false;
		bool isUnconscious2 = false;
		causeOfDeath = null;
		causeOfUnconsciousness = null;
		if (Find<BodyComponent>(out var c))
		{
			c.Body.GetStatus(out isDead2, out isUnconscious2, ref causeOfDeath, ref causeOfUnconsciousness);
		}
		bool isDead3 = false;
		bool isUnconscious3 = false;
		if (Find<BiologicalEntity>(out var c2))
		{
			c2.GetStatus(out isDead3, out isUnconscious3, ref causeOfDeath, ref causeOfUnconsciousness);
		}
		isDead = isDead2 || isDead3;
		isUnconscious = isUnconscious2 || isUnconscious3;
	}

	public void EnableCollisions()
	{
		if (The.Sim.Mode == Sim.EngineMode.Game && Collidable != null && !Collidable.Enabled && CausesCollisions())
		{
			The.CollisionManager.AddCollidable(Collidable);
		}
	}

	private bool CausesCollisions()
	{
		if (CurrentSimState != null && CurrentSimState.GeometryLayoutType != null && CurrentSimState.GeometryLayoutType.CausesCollisions)
		{
			return true;
		}
		if (EntityType.TerrainType == null && EntityType.TreeType == null)
		{
			return EntityType.StructureType == null;
		}
		return false;
	}

	public void DisableCollisions()
	{
		if (Collidable != null && Collidable.Enabled)
		{
			The.CollisionManager.RemoveCollidable(Collidable);
		}
	}

	public void Update(GameTime time, out bool wasDestroyed)
	{
		bool flag = IsCompleted();
		if (name != null)
		{
			name.Contains("onlan");
		}
		if (!IsDead && flag && EntityType.IntelligenceType != null && Find<Intelligence>(out var c))
		{
			c.Update(time);
		}
		if (EntityID != EntityID.Invalid && !IsDead && IsOnPlaySite())
		{
			UpdatePlaySite(time, flag);
		}
		if (EntityID == EntityID.Invalid)
		{
			wasDestroyed = true;
		}
		else
		{
			wasDestroyed = false;
		}
	}

	private void UpdatePlaySite(GameTime time, bool isCompleted)
	{
		if (EntityType.IntelligenceType != null && The.AgentQuadTree != null)
		{
			The.AgentQuadTree.UpdateObject(this, PlaySiteLocation.ToVector2());
		}
		if (Locomotor != null)
		{
			if (collidingTimeout-- <= 0)
			{
				Locomotor.SetIsColliding(aValue: false);
				collidingTimeout = 3;
			}
			if (!IsDead && !ContainedBy.HasValue && Locomotor != null && Collidable != null && Collidable.Enabled)
			{
				Locomotor.ReactToCollisions();
			}
		}
		PlaceGeometryLayoutIfDirty(GeoPlaceMode.ShapeChanged);
		if (!IsDead)
		{
			if (isCompleted)
			{
				if (EntityType.LocomotorType != null && Find<Locomotor>(out var c))
				{
					c.UpdatePlaySite(time);
				}
				if (EntityType.SensorType != null && Find<Sensor>(out var c2))
				{
					c2.UpdatePlaySite(time);
				}
				if (EntityType.TreeType != null && Find<Tree>(out var c3))
				{
					c3.UpdatePlaySite(time);
				}
				if (EntityType.Person != null && Find<Person>(out var c4))
				{
					c4.UpdatePlaySite(time);
				}
				SimEffects?.UpdatePlaySite(time);
			}
			if (EntityType.NonLivingType != null && Find<NonLivingEntity>(out var c5))
			{
				c5.Update(time);
			}
		}
		if (EntityType.ContainerType != null && EntityType.ContainerType.GetRequiresReplenishType() != null && Contains is IHasReplenishItems { ReplenishItems: not null } hasReplenishItems)
		{
			hasReplenishItems.ReplenishItems.Update();
		}
		if (!IsDead)
		{
			UpdateSystemsThatCanCauseDeath();
		}
	}

	private void UpdateSystemsThatCanCauseDeath()
	{
		if (EntityType.BiologicalType == null || !bioSystemsRegulator.IsReadyGetTimeElapsedInSeconds(out var secondsSinceLastReady) || !Find<BiologicalEntity>(out var c))
		{
			return;
		}
		GetStatus(out var isDead, out var isUnconscious, out var causeOfDeath, out var causeOfUnconsciousness);
		c.UpdateSimulation(secondsSinceLastReady);
		Body.RegainHitpoints(secondsSinceLastReady);
		GetStatus(out var isDead2, out var isUnconscious2, out causeOfDeath, out causeOfUnconsciousness);
		bool flag = false;
		if (isDead2 || isUnconscious2)
		{
			if (!isDead && !isUnconscious)
			{
				flag = true;
			}
			else if (Intelligence.Brain.Subgoals.Count == 0)
			{
				flag = true;
			}
			else
			{
				Goal goal = Intelligence.Brain.Subgoals.Peek();
				if (!(goal is GoalCollapse) && !(goal is GoalIsDying))
				{
					flag = true;
				}
			}
		}
		if (flag)
		{
			Intelligence.Brain.RemoveAllSubgoals();
			bool flag2 = causeOfDeath == CauseOfDeath.Wounds || causeOfUnconsciousness == CauseOfUnconsciousness.Wounds;
			EntityID? killer = null;
			if (flag2)
			{
				killer = Intelligence.Memory.GetLastAttacker();
			}
			Intelligence.Brain.AddSubgoal(new GoalCollapse(this, OwnedBy, killer));
			Intelligence.Brain.AddSubgoal(new GoalIsDying(this, OwnedBy, flag2));
		}
	}

	public void SetNotDirty()
	{
		footprintIsDirty = false;
	}

	public void PlaceGeometryLayoutIfDirty(GeoPlaceMode mode)
	{
		if (footprintIsDirty && GeometryLayout != null && (Structure == null || Structure.ConstructionHasStarted()))
		{
			GeometryLayout.Place(mode);
		}
		footprintIsDirty = false;
	}

	public Vector3 ModifyNewLocationToStayOnFreeTerrain(Vector3 newLocation, Vector2 pushVector)
	{
		SubtilePos subtilePos = MapManager.WorldPosToSubtilePos(newLocation);
		SubtilePos subtilePos2 = MapManager.WorldPosToSubtilePos(PlaySiteLocation);
		if (subtilePos != subtilePos2)
		{
			SubtileLayers subtileLayers = The.Map.TerrainCosts[GetTransportType()];
			if (MapManager.IsBlocked(subtileLayers.GetValue(subtilePos)))
			{
				if (MapManager.IsBlocked(subtileLayers.GetValue(subtilePos2)))
				{
					return PlaySiteLocation;
				}
				switch (Common.GetDirection(subtilePos2.ToPoint(), subtilePos.ToPoint()))
				{
				case Common.Direction.North:
				case Common.Direction.South:
					if (pushVector.X == 0f)
					{
						return PlaySiteLocation;
					}
					pushVector.Y = 0f;
					return PlaySiteLocation + pushVector.ToVector3();
				case Common.Direction.East:
				case Common.Direction.West:
					if (pushVector.Y == 0f)
					{
						return PlaySiteLocation;
					}
					pushVector.X = 0f;
					return PlaySiteLocation + pushVector.ToVector3();
				case Common.Direction.NorthEast:
					if (!IsDirectionBlocked(subtileLayers, Common.Direction.North, subtilePos2))
					{
						pushVector.X = 0f;
						return PlaySiteLocation + pushVector.ToVector3();
					}
					if (!IsDirectionBlocked(subtileLayers, Common.Direction.East, subtilePos2))
					{
						pushVector.Y = 0f;
						return PlaySiteLocation + pushVector.ToVector3();
					}
					return PlaySiteLocation;
				case Common.Direction.NorthWest:
					if (!IsDirectionBlocked(subtileLayers, Common.Direction.North, subtilePos2))
					{
						pushVector.X = 0f;
						return PlaySiteLocation + pushVector.ToVector3();
					}
					if (!IsDirectionBlocked(subtileLayers, Common.Direction.West, subtilePos2))
					{
						pushVector.Y = 0f;
						return PlaySiteLocation + pushVector.ToVector3();
					}
					return PlaySiteLocation;
				case Common.Direction.SouthEast:
					if (!IsDirectionBlocked(subtileLayers, Common.Direction.South, subtilePos2))
					{
						pushVector.X = 0f;
						return PlaySiteLocation + pushVector.ToVector3();
					}
					if (!IsDirectionBlocked(subtileLayers, Common.Direction.East, subtilePos2))
					{
						pushVector.Y = 0f;
						return PlaySiteLocation + pushVector.ToVector3();
					}
					return PlaySiteLocation;
				case Common.Direction.SouthWest:
					if (!IsDirectionBlocked(subtileLayers, Common.Direction.South, subtilePos2))
					{
						pushVector.X = 0f;
						return PlaySiteLocation + pushVector.ToVector3();
					}
					if (!IsDirectionBlocked(subtileLayers, Common.Direction.West, subtilePos2))
					{
						pushVector.Y = 0f;
						return PlaySiteLocation + pushVector.ToVector3();
					}
					return PlaySiteLocation;
				default:
					return PlaySiteLocation;
				}
			}
		}
		return newLocation;
	}

	private bool IsDirectionBlocked(SubtileLayers terrain, Common.Direction dir, SubtilePos currentSubtile)
	{
		switch (dir)
		{
		case Common.Direction.East:
			currentSubtile.X++;
			break;
		case Common.Direction.North:
			currentSubtile.Y--;
			break;
		case Common.Direction.South:
			currentSubtile.Y++;
			break;
		case Common.Direction.West:
			currentSubtile.X--;
			break;
		default:
			return true;
		}
		return MapManager.IsBlocked(terrain.GetValue(currentSubtile));
	}

	public void AttachTriggers(TriggerType[] triggers)
	{
		if (triggers != null)
		{
			foreach (TriggerType triggerType in triggers)
			{
				Trigger trigger = new Trigger(this, null, triggerType);
				AttachTrigger(trigger);
			}
		}
	}

	public void Destroy(bool destroyParts = true, bool parentIsDestroyed = false)
	{
		_ = PersonEntity;
		EntityType.EventActions.TryGetValue(EntityEventHooks.ToBeDestroyed, out var value);
		Goal.FireEventActions(this, null, value);
		Site site = Site;
		Point? mapPos = MapPosition;
		AssignedToJob = null;
		inUseBy = null;
		if (DirectionalLayout != null)
		{
			DirectionalLayout.Destroy();
		}
		if (GeometryLayout != null)
		{
			GeometryLayout.Destroy();
		}
		if (Structure != null)
		{
			Structure.Destroy();
		}
		if (Find<Tree>(out var c))
		{
			c.Destroy();
		}
		RemoveOwnedItemIfOwnerSeesItDestroyed(mapPos);
		if (Find<Sensor>(out var c2))
		{
			c2.UnseeTilesInRange();
		}
		RemoveFromMap();
		if (Find<Item>(out var c3))
		{
			c3.Destroy();
		}
		if (PartOf != null)
		{
			if (!parentIsDestroyed)
			{
				PartOf.SetBrokenPart();
			}
			PartOf.RemovePart(this, setPartOfToNull: false);
		}
		if (Parts != null)
		{
			for (int num = Parts.Count - 1; num >= 0; num--)
			{
				Entity entity = Parts[num];
				if (destroyParts)
				{
					entity.Destroy(destroyParts: true, parentIsDestroyed: true);
				}
				else
				{
					entity.PartOf = null;
				}
			}
		}
		if (Contains != null && IsCompleted())
		{
			Contains.Destroy();
		}
		if (PartOf == null)
		{
			GetContainedBy(out Container container);
			container?.Remove(this);
		}
		if (mapPos.HasValue)
		{
			The.Map.GetTile(mapPos.Value).DeleteMemoryOfDestroyedEntity(this);
		}
		if (EntityType.Person != null)
		{
			PersonEntity.Destroy();
		}
		if (Intelligence != null)
		{
			Intelligence.Destroy();
		}
		if (Renderable != null)
		{
			Renderable.Destroy();
		}
		if (Collidable != null)
		{
			Collidable.Delete();
			Collidable = null;
		}
		if (SelectionShape != null)
		{
			SelectionShape.Delete();
			SelectionShape = null;
		}
		if (Find<Vehicle>(out var c4))
		{
			c4.Destroy();
		}
		if (EntityType.CommunicatorType != null && Find<Communicator>(out var c5))
		{
			c5.Destroy();
		}
		site.RemoveEntity(this);
		The.Client.HandleDestroyedEntity(EntityID);
		RemoveIDEntry();
		((ILookUp<IComposite, CompositeID>)this).RemoveIDEntry();
		((ILookUp<IDetectable, DetectableID>)this).RemoveIDEntry();
		if (EntityType.IntelligenceType != null)
		{
			((ILookUp<ICanIterateEntities, CanIterateEntitiesID>)this).RemoveIDEntry();
		}
		DeleteAttachedTriggers();
		RemoveFromAgentQuadTree();
		EntityType.EventActions.TryGetValue(EntityEventHooks.Destroyed, out value);
		Goal.FireEventActions(this, null, value);
		if (EntityType.IntelligenceType != null && EntityType.BiologicalType != null && site != null && site.IsPlaySite)
		{
			EntityType.IntelligenceType.EventActions.TryGetValue(AgentActionHooks.DiedOnPlaySite, out value);
			Goal.FireEventActions(this, null, value);
		}
		if (EntityType.PolledEvents != null && EntityType.PolledEvents.TryGetValue(Scope.Entity, out var value2))
		{
			foreach (PolledEventType item in value2)
			{
				Site.EventManager.RemovePolledEvent(item, EntityID);
			}
		}
		PartOf = null;
		The.Sim.RemoveEntity(this);
	}

	public static void LogProductionEvent(IKnownEntityData item, ProductionStatistics.StatTypes statType, bool testIfSeen)
	{
		LookUpOwners.ResolveEntityOwner(item, out IOwner owner);
		if (owner != null && (!testIfSeen || owner.Allegiance.SharedKnowledge.GetKnownData(item.EntityID, out var _) == EntityResult.SeenDirectly))
		{
			owner.Allegiance.Statistics.AddProductionEvent(item.EntityType, statType, 1);
		}
	}

	private void RemoveOwnedItemIfOwnerSeesItDestroyed(Point? mapPos)
	{
		Allegiance allegiance = null;
		EntityGroup entityGroup = null;
		if (PersonEntity == null && OwnedBy.HasValue)
		{
			IOwner owner = LookUpOwners.FindByID(OwnedBy);
			if (owner != null)
			{
				allegiance = owner.Allegiance;
				entityGroup = owner.OwnedEntities;
			}
		}
		if (allegiance != null && entityGroup != null && mapPos.HasValue && The.Map.GetTile(mapPos.Value).AllegiancesThatSeeThisTile.Contains(allegiance))
		{
			entityGroup.DeleteEntity(this);
		}
	}

	public void RemoveFromMap()
	{
		Point? point = MapPosition;
		if (point.HasValue && The.Map.TileIsOnMap(point.Value.X, point.Value.Y))
		{
			TerrainTile tile = The.Map.GetTile(point.Value);
			if (tile.ContainsEntity(this))
			{
				tile.RemoveEntity(this);
			}
		}
	}

	private void DeleteAttachedTriggers()
	{
		while (attachedTriggers.Count != 0)
		{
			DeleteTrigger(attachedTriggers[attachedTriggers.Count - 1]);
		}
	}

	public Entity FindPartOfType(EntityType type)
	{
		if (Parts != null)
		{
			foreach (Entity part in Parts)
			{
				if (part.EntityType == type)
				{
					return part;
				}
				Entity entity = part.FindPartOfType(type);
				if (entity != null)
				{
					return entity;
				}
			}
		}
		return null;
	}

	public bool IsCompleted()
	{
		if (Find<NonLivingEntity>(out var c))
		{
			return c.IsCompleted();
		}
		return true;
	}

	public bool RequiresRollToDetect()
	{
		if (EntityType.RequiresRollToDetect.HasValue)
		{
			return EntityType.RequiresRollToDetect.Value;
		}
		if (EntityType.ItemType == null && EntityType.StructureType == null)
		{
			return EntityType.TreeType == null;
		}
		return false;
	}

	public bool IsWeatherProof()
	{
		return Item?.IsWeatherProof() ?? false;
	}

	public bool UsesMemory(SharedKnowledge sharedKnowledge)
	{
		if (EntityType.GetUsesMemory() && IsStarted() != false)
		{
			return HasInterestInEntity(sharedKnowledge.Allegiance);
		}
		return false;
	}

	public bool HasInterestInEntity(Allegiance allegiance)
	{
		if (EntityType.IntelligenceType != null && Intelligence.Allegiance == allegiance)
		{
			return false;
		}
		EntityType representativeEntityType = allegiance.RepresentativeEntityType;
		if (representativeEntityType.Person != null)
		{
			return true;
		}
		if (EntityType.IntelligenceType != null)
		{
			return true;
		}
		if (allegiance.IsEatable(EntityType))
		{
			return true;
		}
		if (EntityType.ThreatType != null)
		{
			return true;
		}
		if (representativeEntityType.IntelligenceType.ContainerTransactValue.HasValue && EntityType.ContainerType != null)
		{
			return true;
		}
		if (representativeEntityType.IntelligenceType.CanMountToolsOrWeapons() && EntityType.ItemType != null)
		{
			return true;
		}
		return false;
	}

	public bool? IsStarted()
	{
		if (EntityType.NonLivingType != null && Find<NonLivingEntity>(out var c))
		{
			return c.IsStarted();
		}
		return null;
	}

	public bool IsInsideVehicle()
	{
		if (!PassengerInVehicle.HasValue)
		{
			return DrivingVehicle.HasValue;
		}
		return true;
	}

	public bool IsIntelligentAndNonMoving()
	{
		if (Intelligence != null && Locomotor != null)
		{
			return !Locomotor.IsMoving();
		}
		return false;
	}

	public static bool IsFunctional(IKnownEntityData data)
	{
		if (!data.PartIsBroken && (!data.Condition.HasValue || data.Condition.Value > 0.0))
		{
			if (data.FunctionalScore.HasValue)
			{
				return data.FunctionalScore > 0.0;
			}
			return true;
		}
		return false;
	}

	public bool IsVehicleValidForHauling(Entity entity, IKnownEntityData item)
	{
		if (Find<Vehicle>(out var c) && (c.DrivenBy == null || c.DrivenBy == entity) && AgentStorage != null && AgentStorage.ItemStorage.TotalCapacity >= item.Bulk && GoalEvaluator.IsOnPlaySite(this) && IsCompleted() && IsFunctional(this))
		{
			return true;
		}
		return false;
	}

	public void UpdateFunctionality()
	{
		if (!IsFunctional(this) && GetContainedBy(out Entity container))
		{
			container?.Contains.NotifyBrokenContainedEntity(this);
		}
	}

	public bool SendMessage(Message msg)
	{
		if (Find<Intelligence>(out var c) && c.Brain.SendMessage(msg))
		{
			return true;
		}
		return false;
	}

	public float GetAvoidDetectionFactor()
	{
		Intelligence intelligence = Intelligence;
		if (intelligence != null && intelligence.Brain != null)
		{
			float num = intelligence.Brain.GetStealthFactorOfActivity();
			if (intelligence.IsStealthy)
			{
				float num2 = Math.Max(intelligence.GetSkillValue(GameData.Instance.AllSkillTypes["sneaking"]), intelligence.GetSkillValue(GameData.Instance.AllSkillTypes["hunting"]));
				num = num + BiologicalEntity.ActiveStealthRating + 0.8f * num2;
			}
			num = GetEffect(AffectsNumbers.Stealth, num);
			return Common.Clamp(num, 0f, 1f);
		}
		return avoidDetectionFactor;
	}

	public float? GetChanceToIdleWalkShortDistanceAway()
	{
		if (EntityType.BiologicalType != null)
		{
			BioProperty bioProperty = BiologicalEntity.GetBioProperty("ChanceToIdleWalkShortDistanceAway");
			if (bioProperty != null)
			{
				return bioProperty.NumberValue.Value;
			}
		}
		return EntityType.IntelligenceType.ChanceToIdleWalkShortDistanceAway;
	}

	public float? GetShortIdleWalkMaxDistance()
	{
		if (EntityType.BiologicalType != null)
		{
			BioProperty bioProperty = BiologicalEntity.GetBioProperty("ShortIdleWalkMaxDistance");
			if (bioProperty != null)
			{
				return bioProperty.NumberValue.Value;
			}
		}
		return EntityType.IntelligenceType.ShortIdleWalkMaxDistance;
	}

	public float? GetShortIdleWalkMinDistance()
	{
		if (EntityType.BiologicalType != null)
		{
			BioProperty bioProperty = BiologicalEntity.GetBioProperty("ShortIdleWalkMinDistance");
			if (bioProperty != null)
			{
				return bioProperty.NumberValue.Value;
			}
		}
		return EntityType.IntelligenceType.ShortIdleWalkMinDistance;
	}

	public float? GetAggroRange()
	{
		if (EntityType.BiologicalType != null)
		{
			BioProperty bioProperty = BiologicalEntity.GetBioProperty("AggroRange");
			if (bioProperty != null)
			{
				return bioProperty.NumberValue.Value;
			}
		}
		return EntityType.IntelligenceType.AggroRange;
	}

	public float? GetAssistanceRange()
	{
		if (EntityType.BiologicalType != null)
		{
			BioProperty bioProperty = BiologicalEntity.GetBioProperty("AssistanceRange");
			if (bioProperty != null)
			{
				return bioProperty.NumberValue.Value;
			}
		}
		return EntityType.IntelligenceType.AssistanceRange;
	}

	public float GetDaySensorRange()
	{
		if (EntityType.BiologicalType != null)
		{
			BioProperty bioProperty = BiologicalEntity.GetBioProperty("SensorRange");
			if (bioProperty != null)
			{
				return bioProperty.NumberValue.Value;
			}
		}
		return EntityType.SensorType.Range;
	}

	public float GetNightSensorRange()
	{
		float? num = null;
		if (EntityType.BiologicalType != null)
		{
			BioProperty bioProperty = BiologicalEntity.GetBioProperty("SensorRangeAtNight");
			if (bioProperty != null)
			{
				num = bioProperty.NumberValue.Value;
			}
		}
		num = num ?? EntityType.SensorType.RangeAtNight;
		return new float?(GetEffect(AffectsNumbers.NightSensorRange, num.Value)).Value;
	}

	public int GetVisionRangeInTiles(float lightLevels)
	{
		return (int)(1f / 48f * GetVisionRange(lightLevels));
	}

	public float GetVisionRange(float lightLevels)
	{
		return MathHelper.Lerp(GetNightSensorRange(), GetDaySensorRange(), lightLevels);
	}

	public float GetDetectionFactor(IDetectable detectable, bool requiresExamineAction)
	{
		Intelligence intelligence = Intelligence;
		if (intelligence != null && intelligence.Brain != null)
		{
			float detectionFactorOfActivity = intelligence.Brain.GetDetectionFactorOfActivity(detectable, requiresExamineAction);
			float num = 1f;
			return detectionFactorOfActivity * num;
		}
		return 0.2f;
	}

	static Entity()
	{
		exposedPropertyValueFunctions = new Dictionary<string, GetPropertyValue>();
		getChildrenProperties = new Dictionary<string, GetChildrenDelegate>();
		IDCounter = EntityID.First;
		exposedPropertyValueFunctions.Add("hungerStatus", GetHungerStatus);
		exposedPropertyValueFunctions.Add("foodEnergyLevel", GetFoodEnergyLevel);
		exposedPropertyValueFunctions.Add("foodEnergyStatus", GetFoodEnergyStatus);
		exposedPropertyValueFunctions.Add("proteinLevel", GetProteinLevel);
		exposedPropertyValueFunctions.Add("micronutrientsLevel", GetMicronutrientsLevel);
		exposedPropertyValueFunctions.Add("stimulantsLevel", GetStimulantsLevel);
		exposedPropertyValueFunctions.Add("sleepynessLevel", GetSleepLevel);
		exposedPropertyValueFunctions.Add("sleepStatus", GetSleepStatus);
		exposedPropertyValueFunctions.Add("itemBulkForPresentation", GetItemBulkForPresentation);
		exposedPropertyValueFunctions.Add("energyLevel", GetEnergyLevel);
		exposedPropertyValueFunctions.Add("moraleLevel", GetMoraleLevel);
		exposedPropertyValueFunctions.Add("threatStance", GetThreatStance);
		exposedPropertyValueFunctions.Add("replenishStatus", GetReplenishStatus);
		exposedPropertyValueFunctions.Add("replenishStatusTooltip", GetReplenishStatusTooltip);
		exposedPropertyValueFunctions.Add("ammoStatus", GetAmmoStatus);
		exposedPropertyValueFunctions.Add("inAccessible", GetInaccessible);
		exposedPropertyValueFunctions.Add("hasThreatJob", GetHasThreatJob);
		exposedPropertyValueFunctions.Add("ConstructionProgress", GetConstructionProgress);
		exposedPropertyValueFunctions.Add("name", GetNameAsPropertyResult);
		exposedPropertyValueFunctions.Add("type", GetTypeAsPropertyResult);
		exposedPropertyValueFunctions.Add("hitpointLevel", GetHitpointsFraction);
		exposedPropertyValueFunctions.Add("ammoLevel", GetAmmoRoundsLeft);
		exposedPropertyValueFunctions.Add("itemPartCondition", GetCondition);
		exposedPropertyValueFunctions.Add("itemPartConditionTooltip", GetConditionTooltip);
		exposedPropertyValueFunctions.Add("entityIsFunctional", GetEntityIsFunctional);
		exposedPropertyValueFunctions.Add("integrity", GetIntegrity);
		exposedPropertyValueFunctions.Add("daysUntilBreakDown", GetDaysLeftUntilBreakdown);
		exposedPropertyValueFunctions.Add("tradeStorageFraction", GetTradeStorageFraction);
		exposedPropertyValueFunctions.Add("tradeStorageFractionTooltip", GetTradeStorageFractionTooltip);
		exposedPropertyValueFunctions.Add("itemStorageFraction", GetItemStorageFraction);
		exposedPropertyValueFunctions.Add("storageFractionTooltip", GetItemStorageFractionTooltip);
		exposedPropertyValueFunctions.Add("totalTradeCapacity", GetTotalTradeCapacity);
		exposedPropertyValueFunctions.Add("totalStorageCapacity", GetTotalStorageCapacity);
		exposedPropertyValueFunctions.Add("totalProductivity", GetTotalProductivity);
		exposedPropertyValueFunctions.Add("skillProductivity", GetCurrentSkillProductivity);
		exposedPropertyValueFunctions.Add("toolProductivity", GetCurrentToolProductivity);
		exposedPropertyValueFunctions.Add("energyLevelProductivity", GetEnergyLevelProductivity);
		exposedPropertyValueFunctions.Add("isAgent", IsAgent);
		exposedPropertyValueFunctions.Add("location", GetLocation);
		exposedPropertyValueFunctions.Add("allegiance", GetAllegiance);
		exposedPropertyValueFunctions.Add("expedition", GetExpedition);
		exposedPropertyValueFunctions.Add("owningExpedition", GetOwningExpedition);
		exposedPropertyValueFunctions.Add("owningAllegiance", GetOwningAllegiance);
		exposedPropertyValueFunctions.Add("skillInUseName", GetSkillInUseName);
		exposedPropertyValueFunctions.Add("toolInUseName", GetToolInUseName);
		exposedPropertyValueFunctions.Add("replenishTypeName", GetReplenishTypeName);
		exposedPropertyValueFunctions.Add("overallRating", GetOverallRating);
		exposedPropertyValueFunctions.Add("comfortRating", GetComfortRating);
		exposedPropertyValueFunctions.Add("foodRating", GetFoodRating);
		exposedPropertyValueFunctions.Add("securityRating", GetSecurityRating);
		exposedPropertyValueFunctions.Add("comfortPrinciplesAndRating", GetComfortPrinciplesAndRating);
		exposedPropertyValueFunctions.Add("foodPrinciplesAndRating", GetFoodPrinciplesAndRating);
		exposedPropertyValueFunctions.Add("securityPrinciplesAndRating", GetSecurityPrinciplesAndRating);
		exposedPropertyValueFunctions.Add("comfortRatingTooltip", GetComfortRatingTooltip);
		exposedPropertyValueFunctions.Add("foodRatingTooltip", GetFoodRatingTooltip);
		exposedPropertyValueFunctions.Add("securityRatingTooltip", GetSecurityRatingTooltip);
		exposedPropertyValueFunctions.Add("foodHappinessTooltip", GetFoodHappinessTooltip);
		exposedPropertyValueFunctions.Add("comfortHappinessTooltip", GetComfortHappinessTooltip);
		exposedPropertyValueFunctions.Add("securityHappinessTooltip", GetSecurityHappinessTooltip);
		exposedPropertyValueFunctions.Add("happiness", GetHappiness);
		exposedPropertyValueFunctions.Add("emigrateRisk", GetEmigrateRisk);
		exposedPropertyValueFunctions.Add("emigrateRiskForPlaySite", GetPlaySiteEmigrateRisk);
		exposedPropertyValueFunctions.Add("emigrateRiskTooltip", GetEmigrateRiskTooltip);
		exposedPropertyValueFunctions.Add("highestUnhappiness", GetHighestUnhappinessType);
		exposedPropertyValueFunctions.Add("emigrationTarget", GetEmigrationTarget);
		exposedPropertyValueFunctions.Add("freeStorage", GetFreeStorageSpaceAsPropertyResult);
		exposedPropertyValueFunctions.Add("EntityID", GetEntityIDasPropertyResult);
		exposedPropertyValueFunctions.Add("progress", GetProgress);
		exposedPropertyValueFunctions.Add("consumeProgress", GetConsumeProgress);
		exposedPropertyValueFunctions.Add("professionIcon", GetProfessionIcon);
		exposedPropertyValueFunctions.Add("professionDescription", GetProfessionDescription);
		exposedPropertyValueFunctions.Add("homeComfortLevel", GetHomeComfortLevel);
		exposedPropertyValueFunctions.Add("vehicleType", GetVehicleType);
		getChildrenProperties.Add("SubstanceTypes", GetSubstances);
		getChildrenProperties.Add("bodyParts", GetBodyParts);
		getChildrenProperties.Add("triggers", GetTriggers);
		getChildrenProperties.Add("itemParts", GetParts);
		getChildrenProperties.Add("home", GetHome);
		getChildrenProperties.Add("anchor", GetAnchor);
		getChildrenProperties.Add("skills", GetSkills);
		getChildrenProperties.Add("nutrition", GetNutrition);
		getChildrenProperties.Add("contained", GetContained);
		getChildrenProperties.Add("effectProfiles", GetEffectProfiles);
		getChildrenProperties.Add("residents", GetResidents);
	}

	public void PrintScriptVariables(StringBuilder description)
	{
		if (CustomFields == null)
		{
			return;
		}
		foreach (KeyValuePair<string, PropertyResult> customField in CustomFields)
		{
			description.AppendLine(customField.Key + " = " + customField.Value.ToString());
		}
	}

	public string GetCaption(string captionKey)
	{
		PropertyResult? propertyValue = GetPropertyValue(captionKey, null);
		if (propertyValue.HasValue)
		{
			return propertyValue.Value.StringResult;
		}
		return null;
	}

	private static void GetAnchor(SharedKnowledge getterKnowledge, IHasExposedProperties hasProperties, ref List<IHasExposedProperties> listToBeFilledWithParts)
	{
		((Entity)hasProperties).GetAnchor(getterKnowledge, ref listToBeFilledWithParts);
	}

	private void GetAnchor(SharedKnowledge getterKnowledge, ref List<IHasExposedProperties> listOfChildren)
	{
		if (EntityType.StructureType != null && Structure.AnchorID.HasValue)
		{
			Entity entity = FindByID(Structure.AnchorID.Value);
			if (entity != null)
			{
				listOfChildren.Add(entity);
			}
		}
	}

	private static PropertyResult? GetToolInUseName(IHasExposedProperties hasExposedProperties, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposedProperties).GetToolInUseName();
	}

	private PropertyResult? GetToolInUseName()
	{
		if (Intelligence != null && Intelligence.Brain != null)
		{
			return new PropertyResult
			{
				StringResult = Intelligence.Brain.GetToolInUseName()
			};
		}
		return null;
	}

	private static PropertyResult? GetSkillInUseName(IHasExposedProperties hasExposedProperties, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposedProperties).GetSkillInUseName();
	}

	private PropertyResult? GetSkillInUseName()
	{
		if (Intelligence != null && Intelligence.Brain != null)
		{
			return new PropertyResult
			{
				StringResult = Intelligence.Brain.GetSkillInUseName()
			};
		}
		return null;
	}

	public PropertyResult? GetPropertyValue(string propertyKey, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		PropertyResult? result = null;
		_ = propertyKey == "harvestDate";
		if (exposedPropertyValueFunctions.ContainsKey(propertyKey))
		{
			return exposedPropertyValueFunctions[propertyKey](this, getterKnowledge, parent);
		}
		if (CustomFields != null && CustomFields.TryGetValue(propertyKey, out var value))
		{
			result = value;
		}
		return result;
	}

	public void GetChildren(string key, ref List<IHasExposedProperties> listOfChildren, FilterCondition filter, EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget, SharedKnowledge getterKnowledge = null)
	{
		if (getChildrenProperties.TryGetValue(key, out var value))
		{
			value(getterKnowledge, this, ref listOfChildren);
		}
		if (filter != null)
		{
			Site.FilterChildren(listOfChildren, filter, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
		}
	}

	public void SetPropertyValue(string propertyKey, PropertyResult? value)
	{
		if (value.HasValue)
		{
			switch (propertyKey)
			{
			case "spriteFlag":
				SetSpriteFlag(value.Value);
				return;
			case "clearFlag":
				ClearSpriteFlag(value.Value);
				return;
			case "addTrigger":
				AddTrigger(value.Value);
				return;
			case "removeTrigger":
				RemoveTrigger(value.Value);
				return;
			case "enableSpecialAction":
				EnableSpecialAction(value.Value);
				return;
			case "disableSpecialAction":
				DisableSpecialAction(value.Value);
				return;
			case "toggleSecurityRatingTooltip":
				ToggleRatingTooltip(RatingTypes.Security, value.Value.BoolResult.Value);
				return;
			case "toggleComfortRatingTooltip":
				ToggleRatingTooltip(RatingTypes.Comfort, value.Value.BoolResult.Value);
				return;
			case "toggleFoodRatingTooltip":
				ToggleRatingTooltip(RatingTypes.Food, value.Value.BoolResult.Value);
				return;
			}
		}
		SetPropertyValue(ref CustomFields, propertyKey, value);
	}

	public static void SetPropertyValue(ref Dictionary<string, PropertyResult> customFields, string propertyKey, PropertyResult? value)
	{
		if (customFields == null)
		{
			customFields = new Dictionary<string, PropertyResult>();
		}
		if (!value.HasValue)
		{
			customFields.Remove(propertyKey);
		}
		else
		{
			customFields[propertyKey] = value.Value;
		}
	}

	private void AddTrigger(PropertyResult value)
	{
		Trigger trigger = new Trigger(this, null, GameData.Instance.AllTriggerTypes[value.StringResult]);
		AttachTrigger(trigger);
	}

	private void RemoveTrigger(PropertyResult value)
	{
		DeleteTriggerOfType(GameData.Instance.AllTriggerTypes[value.StringResult]);
	}

	private void SetSpriteFlag(PropertyResult value)
	{
		if (Enum.TryParse<StateModifier>(value.StringResult, out var result))
		{
			SetSpriteStateFlag(result);
		}
	}

	private void ClearSpriteFlag(PropertyResult value)
	{
		if (Enum.TryParse<StateModifier>(value.StringResult, out var result))
		{
			Renderable.ClearSpriteStateFlag(result);
		}
	}

	private void EnableSpecialAction(PropertyResult value)
	{
		if (GameData.Instance.AllProcessTypes.TryGetValue(value.StringResult, out var value2))
		{
			EnableSharedSpecialAction(value2);
		}
	}

	public void EnableSharedSpecialAction(ProcessType sharedProcessType)
	{
		if (EntityType.SharedSpecialActionTypes != null)
		{
			ProcessType processType = EntityType.SharedSpecialActionTypes.FirstOrDefault((ProcessType p) => p.OriginalProcess == sharedProcessType);
			if (processType != null && !AvailableSharedSpecialActions.Any((ProcessType p) => p.OriginalProcess == sharedProcessType))
			{
				AvailableSharedSpecialActions.Add(processType);
			}
		}
	}

	private void DisableSpecialAction(PropertyResult value)
	{
		if (GameData.Instance.AllProcessTypes.TryGetValue(value.StringResult, out var value2))
		{
			DisableSharedSpecialAction(value2.OriginalProcess);
		}
	}

	public void DisableSharedSpecialAction(ProcessType sharedProcessType)
	{
		if (EntityType.SharedSpecialActionTypes != null)
		{
			ProcessType processType = EntityType.SharedSpecialActionTypes.FirstOrDefault((ProcessType p) => p.OriginalProcess == sharedProcessType);
			if (processType != null)
			{
				AvailableSharedSpecialActions.Remove(processType);
			}
		}
	}

	public void EnableSpecialActionsUsingAnchor()
	{
		if (EntityType.SharedSpecialActionTypes == null)
		{
			return;
		}
		foreach (ProcessType sharedSpecialActionType in EntityType.SharedSpecialActionTypes)
		{
			if (sharedSpecialActionType.UsesAnchor())
			{
				EnableSharedSpecialAction(sharedSpecialActionType.OriginalProcess);
			}
		}
	}

	public void DisableSpecialActionsUsingAnchor()
	{
		if (EntityType.SharedSpecialActionTypes == null)
		{
			return;
		}
		foreach (ProcessType sharedSpecialActionType in EntityType.SharedSpecialActionTypes)
		{
			if (sharedSpecialActionType.UsesAnchor())
			{
				DisableSharedSpecialAction(sharedSpecialActionType.OriginalProcess);
			}
		}
	}

	private static void GetNutrition(SharedKnowledge getterKnowledge, IHasExposedProperties hasProperties, ref List<IHasExposedProperties> listOfChildren)
	{
		((Entity)hasProperties).GetNutrition(ref listOfChildren);
	}

	private void GetNutrition(ref List<IHasExposedProperties> listOfChildren)
	{
		if (EntityType.ItemType != null && EntityType.ItemType.FoodType != null)
		{
			FoodNutrientAmount[] foodNutrientTypes = EntityType.ItemType.FoodType.FoodNutrientProfile.FoodNutrientTypes;
			foreach (FoodNutrientAmount item in foodNutrientTypes)
			{
				listOfChildren.Add(item);
			}
		}
	}

	private static void GetSkills(SharedKnowledge getterKnowledge, IHasExposedProperties hasProperties, ref List<IHasExposedProperties> listOfChildren)
	{
		((Entity)hasProperties).GetSkills(getterKnowledge, ref listOfChildren);
	}

	private void GetSkills(SharedKnowledge getterKnowledge, ref List<IHasExposedProperties> listOfChildren)
	{
		if ((getterKnowledge != null && AllegianceID.HasValue && !RatingsAreVisible(getterKnowledge)) || EntityType.IntelligenceType == null || Intelligence.Skills == null)
		{
			return;
		}
		foreach (KeyValuePair<SkillType, Skill> skill in Intelligence.Skills)
		{
			if ((!skill.Key.SuppressDisplayForBiologicals || EntityType.BiologicalType == null) && (!skill.Key.SuppressDisplayForPersons || EntityType.Person == null))
			{
				listOfChildren.Add(skill.Value);
			}
		}
		listOfChildren = listOfChildren.OrderBy((IHasExposedProperties c) => ((Skill)c).SkillType.SortOrder).ToList();
	}

	private static void GetEffectProfiles(SharedKnowledge getterKnowledge, IHasExposedProperties hasProperties, ref List<IHasExposedProperties> listOfChildren)
	{
		((Entity)hasProperties).GetEffectProfiles(getterKnowledge, ref listOfChildren);
	}

	private void GetEffectProfiles(SharedKnowledge getterKnowledge, ref List<IHasExposedProperties> listOfChildren)
	{
		SimEffectsComponent simEffects = SimEffects;
		if (simEffects == null)
		{
			return;
		}
		foreach (SimEffectProfile effectProfile in simEffects.EffectProfiles)
		{
			listOfChildren.Add(effectProfile);
		}
	}

	private static void GetTriggers(SharedKnowledge getterKnowledge, IHasExposedProperties hasProperties, ref List<IHasExposedProperties> listOfChildren)
	{
		((Entity)hasProperties).GetTriggers(ref listOfChildren);
	}

	private void GetTriggers(ref List<IHasExposedProperties> listOfChildren)
	{
		if (attachedTriggers != null)
		{
			listOfChildren.AddRange(attachedTriggers);
		}
	}

	private static void GetBodyParts(SharedKnowledge getterKnowledge, IHasExposedProperties hasProperties, ref List<IHasExposedProperties> list)
	{
		((Entity)hasProperties).GetBodyParts(ref list);
	}

	private void GetBodyParts(ref List<IHasExposedProperties> list)
	{
		if (Body != null)
		{
			Body.GetBodyParts(ref list);
		}
	}

	private static void GetSubstances(SharedKnowledge getterKnowledge, IHasExposedProperties hasProperties, ref List<IHasExposedProperties> listOfSubstances)
	{
		((Entity)hasProperties).GetSubstances(ref listOfSubstances);
	}

	private void GetSubstances(ref List<IHasExposedProperties> listOfSubstances)
	{
		if (SubstanceBulkAmounts == null)
		{
			return;
		}
		foreach (KeyValuePair<SubstanceType, SubstanceAmount> substanceBulkAmount in SubstanceBulkAmounts)
		{
			listOfSubstances.Add(substanceBulkAmount.Value);
		}
	}

	private static void GetParts<T>(SharedKnowledge getterKnowledge, IHasExposedProperties hasProperties, ref List<T> listToBeFilledWithParts) where T : IHasExposedProperties
	{
		((Entity)hasProperties).GetParts(ref listToBeFilledWithParts);
	}

	private void GetParts<T>(ref List<T> listToBeFilledWithParts) where T : IHasExposedProperties
	{
		if (Parts == null)
		{
			return;
		}
		foreach (Entity part in Parts)
		{
			listToBeFilledWithParts.Add(part.ConvertToDesiredType<T>());
		}
	}

	private void GetInhabitants(ref List<IHasExposedProperties> listOfChildren)
	{
		if (EntityType.StructureType == null || Contains == null)
		{
			return;
		}
		List<IHasExposedProperties> listOfInhabitants = new List<IHasExposedProperties>();
		Contains.IterateContained(delegate(Entity entity)
		{
			if (entity.EntityType.BiologicalType != null)
			{
				listOfInhabitants.Add(entity.ConvertToDesiredType<IHasExposedProperties>());
			}
		});
		listOfChildren = listOfInhabitants;
	}

	private static void GetContained(SharedKnowledge getterKnowledge, IHasExposedProperties hasProperties, ref List<IHasExposedProperties> listOfChildren)
	{
		((Entity)hasProperties).GetContained(ref listOfChildren);
	}

	private void GetContained(ref List<IHasExposedProperties> listOfChildren)
	{
		if (Contains != null)
		{
			List<IHasExposedProperties> containedEntities = new List<IHasExposedProperties>();
			Contains.IterateContained(delegate(Entity entity)
			{
				containedEntities.Add(entity.ConvertToDesiredType<IHasExposedProperties>());
			});
			listOfChildren = containedEntities;
		}
	}

	public static PropertyResult? GetConditionTooltip(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return GetConditionTooltip((Entity)hasExposed);
	}

	public static PropertyResult? GetTradeStorageFractionTooltip(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetTradeStorageFractionTooltip();
	}

	private PropertyResult? GetTradeStorageFractionTooltip()
	{
		if (Contains != null && Contains is TerminalContainer { TotalTradeItemsStored: var totalTradeItemsStored, TotalTradeItemStorageCapacity: var totalTradeItemStorageCapacity } terminalContainer)
		{
			Dictionary<StorageCondition, Storage> storageSpaces = terminalContainer.GetStorageSpaces();
			return FormatStorageTooltip("Trade storage", storageSpaces, totalTradeItemsStored, totalTradeItemStorageCapacity);
		}
		return null;
	}

	public static PropertyResult? GetItemStorageFractionTooltip(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetItemStorageFractionTooltip();
	}

	private PropertyResult? GetItemStorageFractionTooltip()
	{
		if (Contains != null && Contains is IStorage storage)
		{
			Dictionary<StorageCondition, Storage> storageSpaces = storage.GetStorageSpaces();
			return FormatStorageTooltip("Item storage", storageSpaces, storage.TotalStored, storage.TotalItemStorageCapacity);
		}
		return null;
	}

	private static PropertyResult FormatStorageTooltip(string header, Dictionary<StorageCondition, Storage> condition, float stored, float capacity)
	{
		PropertyResult result = default(PropertyResult);
		StringBuilder stringBuilder = new StringBuilder();
		Common.AppendHeaderOnLightBG(stringBuilder, header);
		foreach (KeyValuePair<StorageCondition, Storage> item in condition)
		{
			if (Common.IsGreaterThan(item.Value.TotalCapacity, 0.0))
			{
				Common.AppendLine(stringBuilder, item.Key.Name);
			}
		}
		Common.AppendDivider(stringBuilder);
		Common.Append(stringBuilder, "Used percentage: ");
		double num = stored / capacity;
		Common.ValueTint value = ((num > 0.9800000190734863) ? Common.ValueTint.Negative : Common.ValueTint.Positive);
		Common.AppendPercentage(stringBuilder, num, useColoring: true, value);
		Common.AppendLine(stringBuilder);
		Common.Append(stringBuilder, "Total: ");
		Common.Append(stringBuilder, Common.ValueToDecimalString(stored, useColoring: true, value));
		Common.Append(stringBuilder, " / ");
		Common.Append(stringBuilder, Common.ValueToDecimalString(capacity, useColoring: true, value));
		Common.Append(stringBuilder, " BLK");
		result.StringResult = stringBuilder.ToString();
		return result;
	}

	public static PropertyResult? GetItemStorageFraction(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetItemStorageFraction();
	}

	private PropertyResult? GetItemStorageFraction()
	{
		if (Contains != null && Contains is IStorage storage)
		{
			return new PropertyResult
			{
				NumberResult = storage.TotalStored / storage.TotalItemStorageCapacity
			};
		}
		return null;
	}

	public static PropertyResult? GetTotalTradeCapacity(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetTotalTradeCapacity();
	}

	private PropertyResult? GetTotalTradeCapacity()
	{
		if (Contains != null && Contains is TerminalContainer terminalContainer)
		{
			return new PropertyResult
			{
				NumberResult = terminalContainer.TotalTradeItemStorageCapacity
			};
		}
		return null;
	}

	public static PropertyResult? GetTotalStorageCapacity(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetTotalStorageCapacity();
	}

	private PropertyResult? GetTotalStorageCapacity()
	{
		if (Contains != null && Contains is IStorage storage)
		{
			return new PropertyResult
			{
				NumberResult = storage.TotalItemStorageCapacity
			};
		}
		return null;
	}

	public static PropertyResult? GetTradeStorageFraction(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetTradeStorageFraction();
	}

	private PropertyResult? GetTradeStorageFraction()
	{
		if (Contains != null && Contains is TerminalContainer terminalContainer)
		{
			return new PropertyResult
			{
				NumberResult = terminalContainer.TotalTradeItemsStored / terminalContainer.TotalTradeItemStorageCapacity
			};
		}
		return null;
	}

	public static PropertyResult? GetFreeStorageSpaceAsPropertyResult(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetFreeStorageSpaceAsPropertyResult();
	}

	private PropertyResult GetFreeStorageSpaceAsPropertyResult()
	{
		return new PropertyResult
		{
			NumberResult = (TotalItemStorageCapacity - TotalStored).Value
		};
	}

	public static PropertyResult? GetEntityIDasPropertyResult(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetEntityIDasPropertyResult();
	}

	private PropertyResult GetEntityIDasPropertyResult()
	{
		return new PropertyResult
		{
			StringResult = GetEntityID().ToString()
		};
	}

	public Type ConvertToDesiredType<Type>() where Type : IHasExposedProperties
	{
		return (Type)(object)this;
	}

	public string GetDefaultCaption(string propertyKey)
	{
		return EntityType.Name;
	}

	public void GetDefaultKey(out string PropertyKey)
	{
		PropertyKey = string.Concat(EntityID);
	}

	public EntityID? GetEntityID()
	{
		return EntityID;
	}

	public bool GetIsSeenDirectly()
	{
		return true;
	}

	public string GetName()
	{
		return Name ?? EntityType.Name;
	}

	public string GetDisplayName()
	{
		return GetDisplayName(this);
	}

	public static string GetDisplayName(IKnownEntityData entityData)
	{
		if (entityData.EntityType.UseTypeNameForDisplay)
		{
			return entityData.EntityType.Name;
		}
		return entityData.Name ?? entityData.EntityType.Name;
	}

	public static PropertyResult? GetNameAsPropertyResult(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetNameAsPropertyResult();
	}

	private PropertyResult GetNameAsPropertyResult()
	{
		return new PropertyResult
		{
			StringResult = GetName()
		};
	}

	public static PropertyResult? GetTypeAsPropertyResult(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetTypeAsPropertyResult();
	}

	private PropertyResult GetTypeAsPropertyResult()
	{
		return new PropertyResult
		{
			StringResult = EntityType.KeyName
		};
	}

	public static PropertyResult? GetConstructionProgress(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetConstructionProgress(getterKnowledge);
	}

	private PropertyResult? GetConstructionProgress(SharedKnowledge getterKnowledge)
	{
		if (NonLivingEntity != null && Structure != null)
		{
			return new PropertyResult
			{
				NumberResult = NonLivingEntity.Progress
			};
		}
		return null;
	}

	public static PropertyResult? GetReplenishTypeName(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetReplenishTypeName(getterKnowledge);
	}

	private PropertyResult? GetReplenishTypeName(SharedKnowledge getterKnowledge)
	{
		if (EntityType.ContainerType != null && EntityType.ContainerType.GetRequiresReplenishType() != null && EntityType.ContainerType.GetRequiresReplenishType().RequiresFuelType != null)
		{
			return new PropertyResult
			{
				StringResult = EntityType.ContainerType.GetRequiresReplenishType().RequiresFuelType.FuelClientString
			};
		}
		return null;
	}

	public static PropertyResult? GetReplenishStatus(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return GetReplenishStatus(getterKnowledge, (IKnownEntityData)hasExposed);
	}

	public static PropertyResult? GetReplenishStatus(SharedKnowledge getterKnowledge, IKnownEntityData entityData)
	{
		if (GetReplenishStatus(getterKnowledge, entityData, out var canBeReplenished, out var _))
		{
			return new PropertyResult
			{
				NumberResult = (canBeReplenished.Value ? 1f : 0f)
			};
		}
		return null;
	}

	private static bool GetReplenishStatus(SharedKnowledge getterKnowledge, IKnownEntityData entityData, out bool? canBeReplenished, out float? requiredAmount)
	{
		canBeReplenished = null;
		requiredAmount = null;
		if (entityData.EntityType.NonLivingType != null && entityData.OwnedBy.HasValue)
		{
			LookUpOwners.ResolveEntityOwner(entityData, out IOwner owner);
			if (owner == null)
			{
				return false;
			}
			if (getterKnowledge != null && (owner == null || owner.Allegiance != getterKnowledge.Allegiance))
			{
				return false;
			}
			if (owner is Expedition expedition)
			{
				canBeReplenished = The.Client.GetToolReplenishStatus(expedition, entityData.EntityType, out requiredAmount);
				return true;
			}
			return false;
		}
		return false;
	}

	public static PropertyResult? GetReplenishStatusTooltip(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return GetReplenishStatusTooltip(getterKnowledge, (IKnownEntityData)hasExposed);
	}

	public static PropertyResult? GetReplenishStatusTooltip(SharedKnowledge getterKnowledge, IKnownEntityData entityData)
	{
		if (GetReplenishStatus(getterKnowledge, entityData, out var canBeReplenished, out var requiredAmount))
		{
			PropertyResult value = default(PropertyResult);
			if (canBeReplenished == false)
			{
				StringBuilder stringBuilder = new StringBuilder();
				Common.Append(stringBuilder, "Not enough suitable fuel is available within a certain range to complete any of the tasks.");
				if (requiredAmount.HasValue)
				{
					Common.AppendLine(stringBuilder);
					Common.Append(stringBuilder, " The minimum required fuel is: ");
					Common.AppendFormat(stringBuilder, "{0:N2} BLK", true, requiredAmount.Value);
				}
				value.StringResult = stringBuilder.ToString();
			}
			return value;
		}
		return null;
	}

	public static PropertyResult? GetInaccessible(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return MemoryFact.GetInaccessibleStatus(((Entity)hasExposed).ID);
	}

	public static PropertyResult? GetHasThreatJob(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return GetHasThreatJob(((Entity)hasExposed).ID);
	}

	public static PropertyResult? GetHasThreatJob(EntityID entityID)
	{
		PropertyResult value = default(PropertyResult);
		if (The.InGameUI.UIAllegiance.SharedKnowledge.AllKnownEntities.ThreatJobsByTarget.TryGetValue(entityID, out var _))
		{
			value.NumberResult = 1f;
		}
		else
		{
			value.NumberResult = 0f;
		}
		return value;
	}

	public static PropertyResult? GetItemBulkForPresentation(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return GetItemBulkForPresentation((Entity)hasExposed);
	}

	public static PropertyResult? GetThreatStance(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetThreatStance(getterKnowledge);
	}

	public static PropertyResult? GetMoraleLevel(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetMoraleLevel(getterKnowledge);
	}

	public static PropertyResult? GetEnergyLevel(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetEnergyLevel(getterKnowledge);
	}

	public static PropertyResult? GetOverallRating(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetOverallRating(getterKnowledge);
	}

	public static PropertyResult? GetComfortRating(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetRating(getterKnowledge, RatingTypes.Comfort);
	}

	public static PropertyResult? GetSecurityRating(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetRating(getterKnowledge, RatingTypes.Security);
	}

	public static PropertyResult? GetFoodRating(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetRating(getterKnowledge, RatingTypes.Food);
	}

	public static PropertyResult? GetComfortPrinciplesAndRating(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetPrinciplesAndRating(getterKnowledge, RatingTypes.Comfort);
	}

	public static PropertyResult? GetSecurityPrinciplesAndRating(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetPrinciplesAndRating(getterKnowledge, RatingTypes.Security);
	}

	public static PropertyResult? GetFoodPrinciplesAndRating(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetPrinciplesAndRating(getterKnowledge, RatingTypes.Food);
	}

	public static PropertyResult? GetSecurityRatingTooltip(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetRatingTooltip(getterKnowledge, RatingTypes.Security);
	}

	public static PropertyResult? GetComfortRatingTooltip(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetRatingTooltip(getterKnowledge, RatingTypes.Comfort);
	}

	public static PropertyResult? GetFoodRatingTooltip(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetRatingTooltip(getterKnowledge, RatingTypes.Food);
	}

	public static PropertyResult? GetFoodHappinessTooltip(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetHappinessTooltip(getterKnowledge, RatingTypes.Food);
	}

	public static PropertyResult? GetComfortHappinessTooltip(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetHappinessTooltip(getterKnowledge, RatingTypes.Comfort);
	}

	public static PropertyResult? GetSecurityHappinessTooltip(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetHappinessTooltip(getterKnowledge, RatingTypes.Security);
	}

	public static PropertyResult? GetEmigrateRiskTooltip(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetEmigrateRiskTooltip(getterKnowledge);
	}

	public static PropertyResult? GetHighestUnhappinessType(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetHighestUnhappinessType();
	}

	public static PropertyResult? GetEmigrationTarget(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetEmigrationTarget();
	}

	public static PropertyResult? GetHappiness(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetHappiness(getterKnowledge);
	}

	public static PropertyResult? GetEmigrateRisk(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetEmigrateRisk(getterKnowledge);
	}

	public static PropertyResult? GetPlaySiteEmigrateRisk(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetPlaySiteEmigrateRisk(getterKnowledge);
	}

	public static PropertyResult? GetItemBulkForPresentation(IKnownEntityData entityData)
	{
		if (entityData.EntityType.ItemType != null)
		{
			return new PropertyResult
			{
				NumberResult = GetBulkAsNumber(entityData.Bulk)
			};
		}
		return null;
	}

	private static float GetBulkAsNumber(float bulk)
	{
		return 100f * bulk;
	}

	public static string GetBulkAsString(float bulk)
	{
		return GetBulkAsNumber(bulk).ToString("F0");
	}

	private PropertyResult? GetThreatStance(SharedKnowledge getterKnowledge)
	{
		if (Intelligence != null)
		{
			if (getterKnowledge != null && AllegianceID.HasValue && getterKnowledge.Allegiance.ID != AllegianceID)
			{
				return null;
			}
			return new PropertyResult
			{
				NumberResult = (int)Intelligence.ThreatStance
			};
		}
		return null;
	}

	private PropertyResult? GetMoraleLevel(SharedKnowledge getterKnowledge)
	{
		if (Intelligence != null)
		{
			if (getterKnowledge != null && AllegianceID.HasValue && getterKnowledge.Allegiance.ID != AllegianceID)
			{
				return null;
			}
			return new PropertyResult
			{
				NumberResult = (int)Intelligence.Morale * 100
			};
		}
		return null;
	}

	private PropertyResult? GetOverallRating(SharedKnowledge getterKnowledge)
	{
		if (EntityType.IntelligenceType != null && Intelligence.IsIndependent())
		{
			if (getterKnowledge != null && !RatingsAreVisible(getterKnowledge))
			{
				return null;
			}
			return new PropertyResult
			{
				NumberResult = (float)Intelligence.Statistics.GetOverallRating()
			};
		}
		return null;
	}

	private PropertyResult? GetHappiness(SharedKnowledge getterKnowledge)
	{
		if (EntityType.IntelligenceType != null && Intelligence.HasHappiness())
		{
			if (getterKnowledge != null && !RatingsAreVisible(getterKnowledge))
			{
				return null;
			}
			return new PropertyResult
			{
				NumberResult = PersonEntity.Personality.Happiness
			};
		}
		return null;
	}

	private PropertyResult? GetEmigrateRisk(SharedKnowledge getterKnowledge)
	{
		if (EntityType.IntelligenceType != null && Intelligence.EmigrateDecider != null)
		{
			PropertyResult value;
			if (Intelligence.EmigrateDecider.CanEmigrateToAnyTarget())
			{
				if (getterKnowledge != null && !RatingsAreVisible(getterKnowledge))
				{
					return null;
				}
				value = new PropertyResult
				{
					NumberResult = Intelligence.EmigrateDecider.MigrationRisk
				};
			}
			else
			{
				value = new PropertyResult
				{
					NumberResult = -1f
				};
			}
			return value;
		}
		return null;
	}

	private PropertyResult? GetPlaySiteEmigrateRisk(SharedKnowledge getterKnowledge)
	{
		if (Site == null || !Site.IsPlaySite)
		{
			return null;
		}
		return GetEmigrateRisk(getterKnowledge);
	}

	private bool RatingsAreVisible(SharedKnowledge getterKnowledge)
	{
		if (getterKnowledge.Allegiance.ID == AllegianceID || (EntityType.Person != null && Site != The.Sim.PlaySite))
		{
			return true;
		}
		return false;
	}

	private PropertyResult? GetRating(SharedKnowledge getterKnowledge, RatingTypes statType)
	{
		if (EntityType.IntelligenceType != null && Intelligence.IsIndependent())
		{
			if (!RatingsAreVisible(getterKnowledge))
			{
				return null;
			}
			return new PropertyResult
			{
				NumberResult = Intelligence.Statistics.GetRating(statType)
			};
		}
		return null;
	}

	private PropertyResult? GetPrinciplesAndRating(SharedKnowledge getterKnowledge, RatingTypes statType)
	{
		if (EntityType.IntelligenceType != null && Intelligence.IsIndependent() && PersonEntity != null)
		{
			if (!RatingsAreVisible(getterKnowledge))
			{
				return null;
			}
			PropertyResult value = default(PropertyResult);
			Pair<float, float> pair = new Pair<float, float>();
			pair.First = Intelligence.Statistics.GetRating(statType);
			pair.Second = PersonEntity.Personality.Principles[statType];
			value.NumberPairResult = pair;
			return value;
		}
		return null;
	}

	private PropertyResult? GetHappinessTooltip(SharedKnowledge getterKnowledge, RatingTypes statType)
	{
		if (EntityType.IntelligenceType != null && Intelligence.IsIndependent() && PersonEntity != null)
		{
			if (!RatingsAreVisible(getterKnowledge))
			{
				return null;
			}
			PropertyResult value = default(PropertyResult);
			Rating statisticByKey = Intelligence.Statistics.GetStatisticByKey(statType);
			StringBuilder stringBuilder = new StringBuilder();
			Common.AppendLine(stringBuilder, "The bar shows the relation between the person's");
			stringBuilder.Append("PRINCIPLES (Vertical marker): ");
			stringBuilder.Append(Common.PercentageToString(PersonEntity.Personality.Principles[statType]));
			Common.AppendLine(stringBuilder);
			stringBuilder.Append(statisticByKey.GetRatingsBreakdown());
			value.StringResult = stringBuilder.ToString();
			return value;
		}
		return null;
	}

	private PropertyResult? GetRatingTooltip(SharedKnowledge getterKnowledge, RatingTypes statType)
	{
		if (EntityType.IntelligenceType != null && Intelligence.IsIndependent())
		{
			if (!RatingsAreVisible(getterKnowledge))
			{
				return null;
			}
			PropertyResult value = default(PropertyResult);
			Rating statisticByKey = Intelligence.Statistics.GetStatisticByKey(statType);
			value.StringResult = statisticByKey.GetRatingsBreakdown();
			return value;
		}
		return null;
	}

	private PropertyResult? GetEmigrateRiskTooltip(SharedKnowledge getterKnowledge)
	{
		if (EntityType.IntelligenceType != null && Intelligence.IsIndependent() && Intelligence.EmigrateDecider != null)
		{
			if (!RatingsAreVisible(getterKnowledge))
			{
				return null;
			}
			PropertyResult value = default(PropertyResult);
			if (Intelligence.EmigrateDecider.CanEmigrateToAnyTarget())
			{
				value.StringResult = Intelligence.EmigrateDecider.GetMigrateRiskTooltip();
			}
			else
			{
				value.StringResult = Intelligence.EmigrateDecider.GetCanEmigrateToTargetTooltip(null);
			}
			return value;
		}
		return null;
	}

	private PropertyResult? GetHighestUnhappinessType()
	{
		if (EntityType.IntelligenceType != null && Intelligence.IsIndependent())
		{
			return new PropertyResult
			{
				StringResult = Statistic.RatingsTypeToKey(PersonEntity.Personality.GetHighestUnhappiness())
			};
		}
		return null;
	}

	private PropertyResult? GetEmigrationTarget()
	{
		if (EntityType.IntelligenceType != null && Intelligence.CanEmigrate())
		{
			PropertyResult value = default(PropertyResult);
			string text = null;
			if (Intelligence.EmigrateDecider.PreferredMigrationTarget.HasValue)
			{
				Allegiance allegiance = LookUp<Allegiance, UWGame.SimSide.Allegiances.AllegianceID>.FindByID(Intelligence.EmigrateDecider.PreferredMigrationTarget.Value);
				if (allegiance != null)
				{
					text = allegiance.Site.Name;
					value.StringResult = text;
					return value;
				}
			}
		}
		return null;
	}

	private void ToggleRatingTooltip(RatingTypes statType, bool composeTooltip)
	{
		Intelligence.Statistics.GetStatisticByKey(statType).ToggleComposeBreakdown(composeTooltip);
	}

	private static void GetResidents(SharedKnowledge getterKnowledge, IHasExposedProperties hasProperties, ref List<IHasExposedProperties> listToBeFilledWithParts)
	{
		GetResidents((Entity)hasProperties, getterKnowledge, ref listToBeFilledWithParts);
	}

	public static void GetResidents(IKnownEntityData entityData, SharedKnowledge getterKnowledge, ref List<IHasExposedProperties> listOfChildren)
	{
		List<Entity> residents = Residence.GetResidents(entityData);
		if (residents != null)
		{
			listOfChildren.AddRange(residents);
		}
	}

	private static void GetHome(SharedKnowledge getterKnowledge, IHasExposedProperties hasProperties, ref List<IHasExposedProperties> listToBeFilledWithParts)
	{
		((Entity)hasProperties).GetHome(getterKnowledge, ref listToBeFilledWithParts);
	}

	private void GetHome(SharedKnowledge getterKnowledge, ref List<IHasExposedProperties> listOfChildren)
	{
		if (EntityType.IntelligenceType != null && (getterKnowledge == null || !AllegianceID.HasValue || getterKnowledge.Allegiance.ID == AllegianceID) && PersonEntity != null && PersonEntity.Household != null && PersonEntity.Household.Home.HasValue && getterKnowledge != null)
		{
			getterKnowledge.GetKnownData(PersonEntity.Household.Home.Value, out var data);
			if (data != null)
			{
				listOfChildren.Add(data);
			}
		}
	}

	private PropertyResult? GetEnergyLevel(SharedKnowledge getterKnowledge)
	{
		if (BiologicalEntity != null)
		{
			if (getterKnowledge != null && AllegianceID.HasValue && getterKnowledge.Allegiance.ID != AllegianceID)
			{
				return null;
			}
			return new PropertyResult
			{
				NumberResult = BiologicalEntity.EnergyLevel
			};
		}
		return null;
	}

	public static PropertyResult? GetEnergyLevelProductivity(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetEnergyLevelProductivity(getterKnowledge);
	}

	private PropertyResult? GetEnergyLevelProductivity(SharedKnowledge getterKnowledge)
	{
		if (Find<Intelligence>(out var c) && c.Brain.GetCurrentTotalProductivity().HasValue)
		{
			return GetEnergyLevel(getterKnowledge);
		}
		return null;
	}

	public static PropertyResult? GetEntityIsFunctional(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return GetEntityIsFunctional((Entity)hasExposed);
	}

	public static PropertyResult? GetIntegrity(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return GetIntegrity((Entity)hasExposed);
	}

	public static PropertyResult? GetCondition(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return GetCondition((Entity)hasExposed);
	}

	public static PropertyResult? GetDaysLeftUntilBreakdown(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return GetDaysLeftUntilBreakdown((Entity)hasExposed);
	}

	public static PropertyResult? GetEntityIsFunctional(IKnownEntityData entityData)
	{
		if (entityData.EntityType.NonLivingType != null)
		{
			PropertyResult value = default(PropertyResult);
			if (IsFunctional(entityData))
			{
				value.NumberResult = 1f;
			}
			else
			{
				value.NumberResult = 0f;
			}
			return value;
		}
		return null;
	}

	private static bool ShowCondition(IKnownEntityData entityData)
	{
		EntityType entityType = entityData.EntityType;
		if (entityType.NonLivingType != null && (entityType.NonLivingType.FinalDegradeType != null || entityType.Parts != null))
		{
			return entityData.IsStarted() == true;
		}
		return false;
	}

	public static PropertyResult? GetIntegrity(IKnownEntityData entityData)
	{
		if (ShowCondition(entityData) && entityData.Integrity.HasValue)
		{
			PropertyResult value = default(PropertyResult);
			if (entityData.EntityType.NonLivingType.FinalDegradeType != null)
			{
				value.PropertyKeyName = entityData.EntityType.NonLivingType.FinalDegradeType.KeyName;
			}
			value.NumberResult = entityData.Integrity;
			return value;
		}
		return null;
	}

	public static PropertyResult? GetDaysLeftUntilBreakdown(IKnownEntityData entityData)
	{
		if (entityData.EntityType.NonLivingType != null && entityData.ConditionChangeSpeed.HasValue)
		{
			PropertyResult value = default(PropertyResult);
			double daysLeftUntilBreakdown = NonLivingEntity.GetDaysLeftUntilBreakdown(entityData.Condition.Value, entityData.ConditionChangeSpeed.Value);
			if (daysLeftUntilBreakdown > 12.0)
			{
				return null;
			}
			value.NumberResult = (float)daysLeftUntilBreakdown;
			return value;
		}
		return null;
	}

	public static PropertyResult? GetConditionTooltip(IKnownEntityData entityData)
	{
		if (entityData.EntityType.NonLivingType != null && entityData.EntityType.NonLivingType.FinalDegradeType != null && entityData.IsStarted() == true)
		{
			PropertyResult value = default(PropertyResult);
			StringBuilder stringBuilder = new StringBuilder();
			Common.AppendHeaderOnLightBG(stringBuilder, "Condition");
			Common.AppendDivider(stringBuilder);
			if (entityData is Entity entity)
			{
				Storage storage = null;
				if (!entity.StoredIn(out storage))
				{
					return null;
				}
				StorageCondition storageCondition = ((storage == null) ? GameData.Instance.AllStorageConditions["exposed"] : storage.StorageConditions);
				Common.Append(stringBuilder, "Storage: ");
				Common.Append(stringBuilder, storageCondition.Name, tintAsValue: true);
				Common.AppendLine(stringBuilder);
			}
			Common.Append(stringBuilder, "Durability profile: ");
			Common.Append(stringBuilder, entityData.EntityType.NonLivingType.FinalDegradeType.Name, tintAsValue: true);
			Common.AppendLine(stringBuilder);
			Common.Append(stringBuilder, "Current condition: ");
			double percentage = entityData.Condition ?? 1.0;
			Common.AppendPercentage(stringBuilder, percentage, useColoring: true, Common.ValueTint.Neutral);
			if (entityData.ConditionChangeSpeed.HasValue && !Common.IsZero(entityData.ConditionChangeSpeed.Value))
			{
				double daysLeftUntilBreakdown = NonLivingEntity.GetDaysLeftUntilBreakdown(entityData.Condition.Value, entityData.ConditionChangeSpeed.Value);
				Common.AppendLine(stringBuilder);
				Common.Append(stringBuilder, "Days left: ");
				Common.Append(stringBuilder, Common.ValueToDecimalString((float)daysLeftUntilBreakdown, useColoring: true, Common.ValueTint.Neutral));
			}
			value.StringResult = stringBuilder.ToString();
			return value;
		}
		return null;
	}

	public static PropertyResult? GetCondition(IKnownEntityData entityData)
	{
		if (ShowCondition(entityData))
		{
			PropertyResult value = default(PropertyResult);
			if (entityData.EntityType.NonLivingType.FinalDegradeType != null)
			{
				value.PropertyKeyName = entityData.EntityType.NonLivingType.FinalDegradeType.KeyName;
			}
			_ = entityData.EntityID;
			_ = 6604;
			if (entityData.PartIsBroken)
			{
				value.NumberResult = 0f;
			}
			else
			{
				value.NumberResult = (float)(entityData.Condition ?? 1.0);
			}
			return value;
		}
		return null;
	}

	public static PropertyResult? GetLocation(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetLocation();
	}

	private PropertyResult? GetLocation()
	{
		if (Location.HasValue)
		{
			return new PropertyResult
			{
				LocationResult = Location.Value.ToVector2()
			};
		}
		return null;
	}

	public Allegiance GetAllegianceOrOwner()
	{
		Allegiance result = null;
		if (AllegianceID.HasValue)
		{
			result = LookUp<Allegiance, UWGame.SimSide.Allegiances.AllegianceID>.FindByID(AllegianceID.Value);
		}
		else if (PersonEntity == null && OwnedBy.HasValue)
		{
			IOwner owner = LookUpOwners.FindByID(OwnedBy);
			if (owner != null)
			{
				result = owner.Allegiance;
			}
		}
		return result;
	}

	public static PropertyResult? GetAllegiance(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetAllegiance();
	}

	private PropertyResult? GetAllegiance()
	{
		if (Intelligence != null && Intelligence.Allegiance != null)
		{
			return new PropertyResult
			{
				StringResult = Intelligence.Allegiance.KeyName
			};
		}
		return null;
	}

	public static PropertyResult? GetExpedition(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetExpedition();
	}

	private PropertyResult? GetExpedition()
	{
		if (Intelligence != null && Intelligence.CurrentExpedition != null)
		{
			return new PropertyResult
			{
				StringResult = Intelligence.CurrentExpedition.KeyName
			};
		}
		return null;
	}

	public static PropertyResult? GetOwningExpedition(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetOwningExpedition();
	}

	private PropertyResult? GetOwningExpedition()
	{
		Expedition owner = GetOwner();
		if (owner != null)
		{
			return new PropertyResult
			{
				StringResult = owner.KeyName
			};
		}
		return null;
	}

	public Expedition GetOwner()
	{
		if (PersonEntity == null && OwnedBy.HasValue)
		{
			IOwner owner = LookUpOwners.FindByID(OwnedBy);
			if (owner != null)
			{
				return owner as Expedition;
			}
		}
		return null;
	}

	public static PropertyResult? GetOwningAllegiance(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetOwningAllegiance();
	}

	private PropertyResult? GetOwningAllegiance()
	{
		if (PersonEntity == null && OwnedBy.HasValue)
		{
			IOwner owner = LookUpOwners.FindByID(OwnedBy);
			if (owner != null)
			{
				return new PropertyResult
				{
					StringResult = owner.Allegiance.KeyName
				};
			}
		}
		return null;
	}

	public static PropertyResult? GetProgress(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetProgress();
	}

	private PropertyResult? GetProgress()
	{
		if (EntityType.NonLivingType != null)
		{
			if (!IsInStomach())
			{
				Find<NonLivingEntity>(out var c);
				return new PropertyResult
				{
					NumberResult = c.Progress
				};
			}
			return null;
		}
		return null;
	}

	public static PropertyResult? GetConsumeProgress(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetConsumeProgress();
	}

	private bool IsInStomach()
	{
		if (GetContainedBy(out Container container) && container is IStorage storage)
		{
			Storage storedIn = storage.GetStoredIn(this);
			if (storedIn != null && storage.GetCompartment(storedIn.ID) == StorageCompartment.Stomach)
			{
				return true;
			}
		}
		return false;
	}

	private PropertyResult? GetConsumeProgress()
	{
		if (EntityType.NonLivingType != null)
		{
			if (IsInStomach())
			{
				Find<NonLivingEntity>(out var c);
				return new PropertyResult
				{
					NumberResult = c.Progress
				};
			}
			return null;
		}
		return null;
	}

	public static PropertyResult? GetProfessionIcon(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetProfessionIcon();
	}

	private PropertyResult? GetProfessionIcon()
	{
		if (EntityType.IntelligenceType != null)
		{
			Find<Intelligence>(out var c);
			if (c.Profession != null)
			{
				return new PropertyResult
				{
					StringResult = c.Profession.Icon
				};
			}
		}
		return null;
	}

	public static PropertyResult? GetHomeComfortLevel(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return GetHomeComfortLevel(hasExposed as IKnownEntityData);
	}

	public static PropertyResult? GetHomeComfortLevel(IKnownEntityData entityData)
	{
		if (entityData.ComfortLevel.HasValue)
		{
			return new PropertyResult
			{
				NumberResult = entityData.ComfortLevel.Value
			};
		}
		return null;
	}

	public static PropertyResult? GetVehicleType(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return GetVehicleType(hasExposed as IKnownEntityData);
	}

	public static PropertyResult? GetVehicleType(IKnownEntityData entityData)
	{
		if (entityData.EntityType.ContainerType != null)
		{
			PropertyResult value = default(PropertyResult);
			if (entityData.EntityType.ContainerType is VehicleContainerType vehicleContainerType)
			{
				value.StringResult = vehicleContainerType.VehicleType.ToString();
			}
			return value;
		}
		return null;
	}

	public static PropertyResult? GetProfessionDescription(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetProfessionDescription();
	}

	private PropertyResult? GetProfessionDescription()
	{
		if (EntityType.IntelligenceType != null)
		{
			Find<Intelligence>(out var c);
			if (c.Profession != null)
			{
				return new PropertyResult
				{
					StringResult = c.Profession.Name
				};
			}
		}
		return null;
	}

	public static PropertyResult? GetHungerStatus(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetHungerStatus(getterKnowledge);
	}

	private PropertyResult? GetHungerStatus(SharedKnowledge getterKnowledge)
	{
		float? num = null;
		if (EntityType.BiologicalType != null)
		{
			bool flag = true;
			if (getterKnowledge != null && getterKnowledge != Intelligence.Allegiance.SharedKnowledge)
			{
				flag = false;
			}
			int num2 = 0;
			float num3 = 0f;
			foreach (KeyValuePair<string, Need> needs in BiologicalEntity.Needs.NeedsList)
			{
				if ((flag || needs.Value.NeedType.LevelVisibleToOtherAllegiances) && needs.Value.NeedType.FoodNeedType != null && needs.Value.NeedType.FoodNeedType.IsEssential)
				{
					if (needs.Value.PhysicalNeed != null && needs.Value.PhysicalNeed.DaysAtZero > 0f && needs.Value.NeedType.PhysicalEffects.DaysAtZeroCausingDeath.HasValue)
					{
						float val = num ?? 1f;
						float starvedToDeathFraction = needs.Value.PhysicalNeed.GetStarvedToDeathFraction();
						num = Math.Min(val, 0.5f * starvedToDeathFraction);
					}
					else
					{
						num3 += needs.Value.CurrentLevel;
					}
					num2++;
				}
			}
			if (!num.HasValue)
			{
				num = 0.5f * ((num2 <= 0) ? new float?(num3) : new float?(num3 / (float)num2)) + 0.5f;
			}
			return new PropertyResult
			{
				NumberResult = num.Value
			};
		}
		return null;
	}

	public static PropertyResult? GetProteinLevel(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetProteinLevel(getterKnowledge);
	}

	public static PropertyResult? GetMicronutrientsLevel(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetMicronutrientsLevel(getterKnowledge);
	}

	public static PropertyResult? GetFoodEnergyLevel(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetFoodEnergyLevel(getterKnowledge);
	}

	public static PropertyResult? GetStimulantsLevel(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetStimulantsLevel(getterKnowledge);
	}

	private PropertyResult? GetFoodEnergyLevel(SharedKnowledge getterKnowledge)
	{
		return GetNeedLevelResult("foodEnergy", getterKnowledge);
	}

	public static PropertyResult? GetFoodEnergyStatus(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetFoodEnergyStatus(getterKnowledge);
	}

	private PropertyResult? GetFoodEnergyStatus(SharedKnowledge getterKnowledge)
	{
		return GetNeedStatusResult("foodEnergy", getterKnowledge);
	}

	private PropertyResult? GetProteinLevel(SharedKnowledge getterKnowledge)
	{
		return GetNeedLevelResult("protein", getterKnowledge);
	}

	private PropertyResult? GetMicronutrientsLevel(SharedKnowledge getterKnowledge)
	{
		return GetNeedLevelResult("micronutrients", getterKnowledge);
	}

	private PropertyResult? GetStimulantsLevel(SharedKnowledge getterKnowledge)
	{
		return GetNeedLevelResult("stimulants", getterKnowledge);
	}

	public static PropertyResult? GetSleepLevel(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetSleepLevel(getterKnowledge);
	}

	private PropertyResult? GetSleepLevel(SharedKnowledge getterKnowledge)
	{
		return GetNeedLevelResult("sleep", getterKnowledge);
	}

	public static PropertyResult? GetSleepStatus(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetSleepStatus(getterKnowledge);
	}

	private PropertyResult? GetSleepStatus(SharedKnowledge getterKnowledge)
	{
		return GetNeedStatusResult("sleep", getterKnowledge);
	}

	private PropertyResult? GetNeedLevelResult(string needToGetResultFrom, SharedKnowledge getterKnowledge)
	{
		Need need = GetNeed(needToGetResultFrom);
		if (need == null)
		{
			return null;
		}
		if (getterKnowledge != null && getterKnowledge.Allegiance != Intelligence.Allegiance && !need.NeedType.LevelVisibleToOtherAllegiances)
		{
			return null;
		}
		return new PropertyResult
		{
			NumberResult = need.CurrentLevel
		};
	}

	private PropertyResult? GetNeedStatusResult(string needToGetResultFrom, SharedKnowledge getterKnowledge)
	{
		Need need = GetNeed(needToGetResultFrom);
		if (need == null)
		{
			return null;
		}
		if (getterKnowledge != null && getterKnowledge.Allegiance != Intelligence.Allegiance && !need.NeedType.LevelVisibleToOtherAllegiances)
		{
			return null;
		}
		return new PropertyResult
		{
			NumberResult = need.GetWeightedStatus()
		};
	}

	private Need GetNeed(string needKey)
	{
		if (EntityType.BiologicalType != null)
		{
			if (BiologicalEntity.Needs.NeedsList.TryGetValue(needKey, out var value))
			{
				return value;
			}
			return null;
		}
		return null;
	}

	public static PropertyResult? GetHitpointsFraction(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return GetHitpointsFractionValue(((Entity)hasExposed).Body);
	}

	public static PropertyResult? GetHitpointsFractionValue(UWGame.SimSide.Entities.Body.Body body)
	{
		if (body != null)
		{
			return new PropertyResult
			{
				NumberResult = body.GetModifiedHitpointsForPresentation()
			};
		}
		return null;
	}

	public static PropertyResult? GetAmmoStatus(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetAmmoStatus(getterKnowledge);
	}

	private PropertyResult? GetAmmoStatus(SharedKnowledge getterKnowledge)
	{
		int? noOfRounds = null;
		if (!GetAmmoRoundsLeft(getterKnowledge, ref noOfRounds, checkAmmoItems: false))
		{
			return null;
		}
		if (noOfRounds.HasValue)
		{
			PropertyResult value = default(PropertyResult);
			if (EntityType.ItemType != null && EntityType.ItemType.WeaponType != null && EntityType.ItemType.WeaponType.IsIntrinsic == true)
			{
				value.NumberResult = 1f;
			}
			else if (noOfRounds.Value == 0)
			{
				value.NumberResult = 0.2f;
			}
			else
			{
				value.NumberResult = 1f;
			}
			return value;
		}
		return null;
	}

	public static PropertyResult? GetAmmoRoundsLeft(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetAmmoRoundsLeft(getterKnowledge);
	}

	private PropertyResult? GetAmmoRoundsLeft(SharedKnowledge getterKnowledge)
	{
		int? noOfRounds = null;
		if (!GetAmmoRoundsLeft(getterKnowledge, ref noOfRounds))
		{
			return null;
		}
		if (noOfRounds.HasValue)
		{
			return new PropertyResult
			{
				StringResult = noOfRounds.Value.ToString()
			};
		}
		return null;
	}

	private bool GetAmmoRoundsLeft(SharedKnowledge getterKnowledge, ref int? noOfRounds, bool checkAmmoItems = true)
	{
		if (Item != null)
		{
			if (getterKnowledge != null && AllegianceID.HasValue && AllegianceID != getterKnowledge.Allegiance.ID)
			{
				return false;
			}
			if (Item.Ammunition != null && checkAmmoItems)
			{
				noOfRounds = Item.Ammunition.NoOfRounds;
			}
			else
			{
				noOfRounds = GetWeaponRounds(this);
			}
		}
		else if (EntityType.IntelligenceType != null && EntityType.IntelligenceType.IntrinsicWeaponTypes != null)
		{
			int num = -1;
			foreach (KeyValuePair<EntityType, EntityID> intrinsicWeapon in IntrinsicWeapons)
			{
				Entity entity = FindByID(intrinsicWeapon.Value);
				if (entity != null)
				{
					noOfRounds = GetWeaponRounds(entity);
					if (noOfRounds.HasValue)
					{
						num = Common.Max(num, noOfRounds.Value);
					}
				}
			}
			if (num > -1)
			{
				noOfRounds = num;
			}
		}
		return true;
	}

	private static int? GetWeaponRounds(Entity weapon)
	{
		int? result = null;
		if (weapon.EntityType.ContainerType != null && weapon.EntityType.ContainerType is MagazineContainerType)
		{
			MagazineContainer magazineContainer = weapon.Contains as MagazineContainer;
			result = magazineContainer.GetTotalAmmo();
		}
		return result;
	}

	public static PropertyResult? IsAgent(IHasExposedProperties hasProperties, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return new PropertyResult
		{
			BoolResult = (((Entity)hasProperties).EntityType.IntelligenceType != null)
		};
	}

	public static PropertyResult? GetTotalProductivity(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetTotalProductivity(getterKnowledge);
	}

	private PropertyResult? GetTotalProductivity(SharedKnowledge getterKnowledge)
	{
		if (Find<Intelligence>(out var c) && c.Brain != null)
		{
			if (getterKnowledge != null && getterKnowledge.Allegiance != c.Allegiance)
			{
				return null;
			}
			float? currentTotalProductivity = c.Brain.GetCurrentTotalProductivity();
			if (currentTotalProductivity.HasValue)
			{
				return new PropertyResult
				{
					NumberResult = currentTotalProductivity
				};
			}
		}
		return null;
	}

	private static PropertyResult? GetCurrentSkillProductivity(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetCurrentSkillProductivity(getterKnowledge);
	}

	private PropertyResult? GetCurrentSkillProductivity(SharedKnowledge getterKnowledge)
	{
		if (Find<Intelligence>(out var c) && c.Brain != null)
		{
			if (getterKnowledge != null && !RatingsAreVisible(getterKnowledge))
			{
				return null;
			}
			float? skillProductivity = c.Brain.GetSkillProductivity();
			if (skillProductivity.HasValue)
			{
				return new PropertyResult
				{
					NumberResult = skillProductivity
				};
			}
		}
		return null;
	}

	private static PropertyResult? GetCurrentToolProductivity(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Entity)hasExposed).GetCurrentToolProductivity(getterKnowledge);
	}

	private PropertyResult? GetCurrentToolProductivity(SharedKnowledge getterKnowledge)
	{
		if (Find<Intelligence>(out var c) && c.Brain != null)
		{
			if (getterKnowledge != null && getterKnowledge.Allegiance != c.Allegiance)
			{
				return null;
			}
			float? toolProductivity = c.Brain.GetToolProductivity();
			if (toolProductivity.HasValue)
			{
				return new PropertyResult
				{
					NumberResult = toolProductivity
				};
			}
		}
		return null;
	}

	CompositeID ILookUp<IComposite, CompositeID>.GetUniqueID()
	{
		return Composite.GetUniqueID();
	}

	void ILookUp<IComposite, CompositeID>.AddToLookup()
	{
		compositeID = ((ILookUp<IComposite, CompositeID>)this).GetUniqueID();
		if (compositeID != CompositeID.Invalid)
		{
			LookUpIComposites.Add(compositeID, this);
		}
	}

	void ILookUp<IComposite, CompositeID>.RemoveIDEntry()
	{
		LookUpIComposites.Remove(this);
	}

	void ILookUp<IComposite, CompositeID>.ResetIDCounter()
	{
	}

	void ILookUp<IComposite, CompositeID>.SetInvalid()
	{
		compositeID = CompositeID.Invalid;
	}

	void ILookUp<IComposite, CompositeID>.CreateLookupCollection()
	{
	}

	DetectableID ILookUp<IDetectable, DetectableID>.GetUniqueID()
	{
		return Detectable.GetUniqueID();
	}

	void ILookUp<IDetectable, DetectableID>.AddToLookup()
	{
		detectableID = ((ILookUp<IDetectable, DetectableID>)this).GetUniqueID();
		if (detectableID != DetectableID.Invalid)
		{
			LookUpIDetectables.Add(detectableID, this);
		}
	}

	void ILookUp<IDetectable, DetectableID>.RemoveIDEntry()
	{
		LookUpIDetectables.Remove(this);
	}

	void ILookUp<IDetectable, DetectableID>.ResetIDCounter()
	{
	}

	void ILookUp<IDetectable, DetectableID>.SetInvalid()
	{
		detectableID = DetectableID.Invalid;
	}

	void ILookUp<IDetectable, DetectableID>.CreateLookupCollection()
	{
	}

	public void IterateMembers(Action<Entity> iterateFunction)
	{
		iterateFunction(this);
	}

	public void IterateOwnedItems(Action<EntityGroup> iterateFunction)
	{
		if (PersonEntity != null)
		{
			iterateFunction(PersonEntity.OwnedEntities);
		}
	}

	public void RecomputeUpdateInterval()
	{
		RecomputeUpdateInterval(out var _);
	}

	public void RecomputeUpdateInterval(out bool intervalChanged)
	{
		intervalChanged = false;
		double? currentInterval = null;
		if (!IsDead && ID != EntityID.Invalid)
		{
			foreach (KeyValuePair<Type, Component> component in Components)
			{
				UpdateTimePoints.GetSoonestInterval(component.Value.GetUpdateInterval(), ref currentInterval);
			}
			if (IsOnPlaySite())
			{
				if (!IsDead && IsCompleted())
				{
					if (EntityType.ContainerType != null)
					{
						UpdateTimePoints.GetSoonestInterval(Contains.GetUpdateInterval(), ref currentInterval);
					}
					if (EntityType.BiologicalType != null)
					{
						UpdateTimePoints.GetSoonestInterval(GameData.Instance.Constants.UpdateIntervalForBioEntity, ref currentInterval);
					}
				}
				if (footprintIsDirty && GeometryLayout != null && (Structure == null || Structure.ConstructionHasStarted()))
				{
					UpdateTimePoints.GetSoonestInterval(0.0, ref currentInterval);
				}
			}
		}
		if (!Common.IsEqual(UpdateInterval, currentInterval))
		{
			UpdateInterval = currentInterval;
			intervalChanged = true;
		}
	}

	public virtual void TurnOnDesiredShareOfLights(float fractionToTurnOn)
	{
		SetSpriteStateFlag(StateModifier.LightIsOn);
	}

	public virtual void TurnOffTheLight()
	{
		ClearSpriteStateFlag(StateModifier.LightIsOn);
	}

	public void SetNextTimepoint(double? timepoint)
	{
		timePointInSeconds = timepoint;
	}

	void ISleepingUpdatable.CreateSleepyLookupCollection()
	{
	}

	public static void CreateSleepyLookupCollection()
	{
		LookUpSleepyUpdater<Entity>.Create();
	}

	CanIterateEntitiesID ILookUp<ICanIterateEntities, CanIterateEntitiesID>.GetUniqueID()
	{
		return HasMembers.GetUniqueID();
	}

	void ILookUp<ICanIterateEntities, CanIterateEntitiesID>.AddToLookup()
	{
		CanIterateEntitiesID = ((ILookUp<ICanIterateEntities, CanIterateEntitiesID>)this).GetUniqueID();
		if (CanIterateEntitiesID != CanIterateEntitiesID.Invalid)
		{
			LookUpICanIterateEntities.Add(CanIterateEntitiesID, this);
		}
	}

	void ILookUp<ICanIterateEntities, CanIterateEntitiesID>.RemoveIDEntry()
	{
		LookUpICanIterateEntities.Remove(this);
	}

	void ILookUp<ICanIterateEntities, CanIterateEntitiesID>.ResetIDCounter()
	{
	}

	void ILookUp<ICanIterateEntities, CanIterateEntitiesID>.SetInvalid()
	{
		CanIterateEntitiesID = CanIterateEntitiesID.Invalid;
	}

	void ILookUp<ICanIterateEntities, CanIterateEntitiesID>.CreateLookupCollection()
	{
	}
}
