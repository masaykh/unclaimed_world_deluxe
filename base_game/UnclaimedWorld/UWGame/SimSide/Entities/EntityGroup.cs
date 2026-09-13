using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface.Inventory;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Activities;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Combat;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.Items;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Overland.Missions.Templates;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems;
using UWGame.SimSide.Systems.TimeSlicing;
using UWGame.SimSide.Trade;

namespace UWGame.SimSide.Entities;

public class EntityGroup : ISnapshot, ILookUp<EntityGroup, EntityGroupID>
{
	public IHasEntityGroup Parent;

	private HasEntityGroupID snapshotParent;

	private Dictionary<EntityID, EntityID> allEntities = new Dictionary<EntityID, EntityID>();

	public Dictionary<EntityType, List<EntityID>> AllEntities = new Dictionary<EntityType, List<EntityID>>();

	private Dictionary<EntityType, List<EntityID>> items = new Dictionary<EntityType, List<EntityID>>();

	private List<EntityID> vehicles = new List<EntityID>();

	public Dictionary<EntityType, OwnerAmmoOfType> AmmoItems = new Dictionary<EntityType, OwnerAmmoOfType>();

	private bool foodIsDirty;

	private Dictionary<EntityType, List<EntityID>> food = new Dictionary<EntityType, List<EntityID>>();

	public Dictionary<EntityType, List<EntityID>> Structures = new Dictionary<EntityType, List<EntityID>>();

	public Dictionary<EntityType, List<EntityID>> Communicators = new Dictionary<EntityType, List<EntityID>>();

	public Dictionary<EntityType, List<EntityID>> FulfillsNeeds = new Dictionary<EntityType, List<EntityID>>();

	public Dictionary<AttackType, List<EntityID>> WeaponsByAttackType = new Dictionary<AttackType, List<EntityID>>();

	public Dictionary<EntityID, Stockpile> StructureStockpiles = new Dictionary<EntityID, Stockpile>();

	public Dictionary<EntityID, Stockpile> TerminalTradeOffers = new Dictionary<EntityID, Stockpile>();

	public Dictionary<EntityID, Dictionary<UpgradeCategory, EntityType>> Upgrades = new Dictionary<EntityID, Dictionary<UpgradeCategory, EntityType>>();

	public Dictionary<TerminalType.TypesOfTerminal, List<EntityID>> Terminals = new Dictionary<TerminalType.TypesOfTerminal, List<EntityID>>();

	public ProductionOrders ProductionOrders;

	public TradeManager TradeManager;

	public EntityGroupPolicy Policy;

	public HaulingJobManager HaulingJobManager;

	private CyclableID? snapshotHaulingJobManager;

	public OtherJobManager OtherJobManager;

	private CyclableID? snapshotOtherJobManager;

	public HuntingJobManager HuntingJobManager;

	private CyclableID? snapshotHuntingJobManager;

	private List<JobID> allJobs = new List<JobID>();

	public List<Job> HaulingJobs = new List<Job>();

	private List<JobID> snapshotHaulingJobs;

	public Dictionary<EntityType, List<HaulingJobAnyItemOfType>> HaulingJobsAnyItemOfType = new Dictionary<EntityType, List<HaulingJobAnyItemOfType>>();

	private Dictionary<EntityType, List<JobID>> snapshotHaulingJobsAnyItemOfType;

	public Dictionary<EntityID, HaulingJobSpecificItem> SpecificHaulingJobs = new Dictionary<EntityID, HaulingJobSpecificItem>();

	private Dictionary<EntityID, JobID> snapshotSpecificHaulingJobs;

	private List<Job> otherJobs = new List<Job>();

	private List<JobID> snapshotOtherJobs;

	public Dictionary<EntityType, List<ProcessJob>> ProductionJobs = new Dictionary<EntityType, List<ProcessJob>>();

	private Dictionary<EntityType, List<JobID>> snapshotProductionJobs;

	public Dictionary<EntityType, List<ProcessJob>> ManagedProductionJobs = new Dictionary<EntityType, List<ProcessJob>>();

	private Dictionary<EntityType, List<JobID>> snapshotManagedProductionJobs;

	public Dictionary<EntityType, List<ProcessJob>> ProductionJobsByInput = new Dictionary<EntityType, List<ProcessJob>>();

	private Dictionary<EntityType, List<JobID>> snapshotProductionJobsByInput;

	public List<Job> UnattendedProcessJobs = new List<Job>();

	private List<JobID> snapshotUnattendedProcessJobs;

	public Dictionary<EntityID, List<ProcessJob>> RepairJobs = new Dictionary<EntityID, List<ProcessJob>>();

	private Dictionary<EntityID, List<JobID>> snapshotRepairJobs;

	private List<Job> scoutingJobs = new List<Job>();

	private List<JobID> snapshotScoutingJobs;

	private List<Job> findPreyJobs = new List<Job>();

	private List<JobID> snapshotFindPreyJobs;

	private List<Job> patrolJobs = new List<Job>();

	private List<JobID> snapshotPatrolJobs;

	private List<Job> attackAreaJobs = new List<Job>();

	private List<JobID> snapshotAttackAreaJobs;

	private List<Job> variableMaxTakerJobs = new List<Job>();

	private List<JobID> snapshotVariableMaxTakerJobs;

	public List<Job> CheckProcessJobs = new List<Job>();

	private List<JobID> snapshotCheckProcessJobs;

	public PointQuadTree<JobID> CheckProcessJobsQuadTree;

	private List<Pair<JobID, Vector2>> snapshotCheckProcessJobsQuadTree;

	public List<Job> ThreatJobs = new List<Job>();

	public Dictionary<EntityID, Job> ThreatJobsByTarget = new Dictionary<EntityID, Job>();

	private List<JobID> snapshotThreatJobs;

	public List<Job> AssetThreatJobs = new List<Job>();

	public Dictionary<EntityID, Job> AssetThreatJobsByTarget = new Dictionary<EntityID, Job>();

	private List<JobID> snapshotAssetThreatJobs;

	public List<Zone> Zones = new List<Zone>();

	private List<ZoneID> snapshotZones;

	private List<Activity> activities = new List<Activity>();

	public Dictionary<EntityType, float> ProductionImportance = new Dictionary<EntityType, float>();

	public float? FoodProductionImportance;

	private float foodConsumeRate;

	public const float NeutralImportance = 0.5f;

	private static EntityGroupID IDCounter;

	private EntityGroupID id = EntityGroupID.Invalid;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public Dictionary<EntityType, List<EntityID>> Items => items;

	public Dictionary<EntityType, List<EntityID>> Food
	{
		get
		{
			if (foodIsDirty)
			{
				UpdateFoodItems();
				foodIsDirty = false;
			}
			return food;
		}
	}

	public List<EntityID> Vehicles => vehicles;

	public List<Job> OtherJobs => otherJobs;

	public List<Job> ScoutingJobs => scoutingJobs;

	public List<Job> FindPreyJobs => findPreyJobs;

	public List<Job> PatrolJobs => patrolJobs;

	public List<Job> AttackAreaJobs => attackAreaJobs;

	public List<Job> VariableMaxTakerJobs => variableMaxTakerJobs;

	public List<Activity> Activities
	{
		get
		{
			return activities;
		}
		set
		{
			activities = value;
		}
	}

	public EntityGroupID ID
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

	public EntityGroup(IHasEntityGroup parent, bool manageTrade, bool manageProduction)
	{
		AddToLookup();
		Parent = parent;
		Allegiance allegiance = GetAllegiance();
		if (allegiance != null)
		{
			if (manageTrade && allegiance.RepresentativeEntityType.Person != null && !allegiance.Site.IsPlaySite)
			{
				TradeManager = new TradeManager(this);
			}
			if (manageProduction && allegiance.RepresentativeEntityType.IntelligenceType.CanProduce == true)
			{
				ProductionOrders = new ProductionOrders();
			}
		}
		Policy = new EntityGroupPolicy();
		CheckProcessJobsQuadTree = new PointQuadTree<JobID>(new Vector2(The.Map.MapWorldWidth, The.Map.MapWorldHeight), 10, 7);
	}

	public EntityGroup()
	{
	}

	public void SetFoodDirty()
	{
		foodIsDirty = true;
	}

	public void Destroy()
	{
		if (HaulingJobManager != null)
		{
			HaulingJobManager.Destroy();
		}
		if (OtherJobManager != null)
		{
			OtherJobManager.Destroy();
		}
		if (HuntingJobManager != null)
		{
			HuntingJobManager.Destroy();
		}
		if (TradeManager != null)
		{
			TradeManager.Destroy();
		}
		DestroyJobs();
		LookUp<EntityGroup, EntityGroupID>.Remove(this);
	}

	public float GetImportance(EntityType mainOutput)
	{
		if (GetAllegiance().IsEatable(mainOutput))
		{
			return FoodProductionImportance ?? 0.5f;
		}
		if (ProductionImportance.TryGetValue(mainOutput, out var value))
		{
			return value;
		}
		return 0.5f;
	}

	private void DestroyJobs()
	{
		for (int num = allJobs.Count - 1; num >= 0; num--)
		{
			LookUp<Job, JobID>.FindByID(allJobs[num]).Destroy(cancelTakers: true);
		}
	}

	public void RecomputeFoodProductionImportance(Dictionary<EntityType, InventoryPanel.Availability> allAvailableItems)
	{
		int num = 0;
		float num2 = 0f;
		float intervalInDaysForComputingProductImportance = GameData.Instance.AIConstants.IntervalInDaysForComputingProductImportance;
		DateAndTime.TimeDateYear currentTimeDateYear = The.Sim.DateAndTime.CurrentTimeDateYear;
		DateAndTime.TimeDateYear timeDateYear = currentTimeDateYear;
		timeDateYear.AddTime(0f - intervalInDaysForComputingProductImportance);
		foreach (KeyValuePair<EntityType, List<EntityID>> item in food)
		{
			num += GetCurrentStockAmount(item.Key, allAvailableItems);
			num2 += ComputeEventRate(item.Key, timeDateYear, currentTimeDateYear, intervalInDaysForComputingProductImportance, ProductionStatistics.StatTypes.Produced);
		}
		double num3 = ScoreHowLongStocksWillLast(num, num2, foodConsumeRate);
		double urgencyScore = 1.0 - num3;
		double survivalScore = 1.0;
		double num4 = CalculateImportance(urgencyScore, survivalScore);
		FoodProductionImportance = (float)num4;
	}

	public void RecomputeFoodConsumeRate()
	{
		ICanIterateEntities obj = Parent as ICanIterateEntities;
		Dictionary<NeedType, float> totalNeeeds = new Dictionary<NeedType, float>();
		obj.IterateMembers(delegate(Entity e)
		{
			GetFoodNeeds(e, totalNeeeds);
		});
		int num = 0;
		EntityType genericFoodItem = GameData.Instance.AIConstants.GenericFoodItem;
		foreach (KeyValuePair<NeedType, float> item in totalNeeeds)
		{
			if (!item.Key.FoodNeedType.IsEssential)
			{
				continue;
			}
			FoodNutrientAmount foodNutrientAmount = genericFoodItem.ItemType.FoodType.FoodNutrientProfile.FoodNutrientTypes.FirstOrDefault((FoodNutrientAmount n) => n.Nutrient == item.Key.FoodNeedType.FoodNutrientType);
			if (foodNutrientAmount != null)
			{
				float num2 = foodNutrientAmount.Amount * genericFoodItem.ItemType.MaximumBulk.Value;
				int num3 = (int)(item.Value / num2);
				if (num3 > num)
				{
					num = num3;
				}
			}
		}
		foodConsumeRate = num;
	}

	private void GetFoodNeeds(Entity entity, Dictionary<NeedType, float> totalNeeeds)
	{
		if (!entity.Intelligence.IsIndependent() || entity.EntityType.BiologicalType == null)
		{
			return;
		}
		foreach (KeyValuePair<string, Need> needs in entity.BiologicalEntity.Needs.NeedsList)
		{
			if (needs.Value.FoodNeed != null)
			{
				float value = 0f;
				totalNeeeds.TryGetValue(needs.Value.NeedType, out value);
				value += needs.Value.FoodNeed.TotalNeededNutrientBulk;
				totalNeeeds[needs.Value.NeedType] = value;
			}
		}
	}

	public static int GetMaximumJobsBeforeWarning(IHasEntityGroup expedition)
	{
		return expedition.NoOfWorkers * GameData.Instance.AIConstants.JobsPerWorkerCap;
	}

	public static Vector3 GetFreeGroundLocation(IHasEntityGroup hasEntityGroup)
	{
		SubtileLayers mapCosts = The.Map.TerrainCosts[SurfaceType.TransportType.Foot];
		if (!The.Map.SubtileIsCompletelyBlocked(mapCosts, MapManager.WorldPosToSubtile(hasEntityGroup.Location.Value)))
		{
			return hasEntityGroup.Location.Value;
		}
		List<Vector2> list = UniformPoissonDiskSampler.SampleCircle(hasEntityGroup.Location.Value.ToVector2(), 60f, 16f);
		for (int i = 0; i < list.Count; i++)
		{
			Vector2 vector = list[i];
			if (!The.Map.SubtileIsCompletelyBlocked(mapCosts, MapManager.WorldPosToSubtile(vector)))
			{
				return new Vector3(vector, 0f);
			}
		}
		return new Vector3(list[0], 0f);
	}

	public OwnerID? GetOwnerID()
	{
		OwnerID? result = null;
		if (Parent is IOwner owner)
		{
			result = owner.ID;
		}
		return result;
	}

	private void UpdateFoodItems()
	{
		foreach (KeyValuePair<EntityType, List<EntityID>> item in Items)
		{
			EntityType key = item.Key;
			List<EntityID> value = item.Value;
			food.TryGetValue(key, out var value2);
			if (Parent.IsEatable(key))
			{
				if (value2 == null || value2.Count != value.Count)
				{
					if (value2 == null)
					{
						value2 = new List<EntityID>();
						food.Add(key, value2);
					}
					value2.Clear();
					value2.AddRange(value);
				}
			}
			else if (value2 != null && value2.Count > 0)
			{
				value2.Clear();
			}
		}
	}

	public bool StandingOrderJobIsNeeded(EntityType entityType)
	{
		if (ProductionOrders.Orders.TryGetValue(entityType, out var value))
		{
			int num = CountAvailableItems(entityType);
			if (value.AmountToKeepInStore.HasValue && value.AmountToKeepInStore.Value > num)
			{
				return true;
			}
		}
		return false;
	}

	public void SpawnOthersiteStartingStructures(string structureProfileKey)
	{
		foreach (KeyValuePair<string, int> startingStructure in GameData.Instance.AllStructuresProfiles[structureProfileKey].StartingStructures)
		{
			for (int i = 0; i < startingStructure.Value; i++)
			{
				Site site = Parent.Allegiance.Site;
				Entity entity = Entity.CreateAndInitEntity(GameData.Instance.AllEntityTypes[startingStructure.Key], site, null, null, Parent.Allegiance);
				entity.PlaceEntityOnOtherSite(site, null, null, new Entity.SetOwnerInfo((IOwner)Parent), null);
				entity.ComeOnline();
			}
		}
	}

	public void DeleteEntity(EntityID entityID, EntityType entityType)
	{
		allEntities.Remove(entityID);
		StructureStockpiles.Remove(entityID);
		TerminalTradeOffers.Remove(entityID);
		Upgrades.Remove(entityID);
		if (entityType != null)
		{
			Common.RemoveFromMultiList(AllEntities, entityType, entityID);
			if (IsVehicle(entityType) && Vehicles != null)
			{
				Vehicles.Remove(entityID);
			}
			if (entityType.StructureType != null)
			{
				Common.RemoveFromMultiList(Structures, entityType, entityID);
			}
			if (entityType.CommunicatorType != null)
			{
				Common.RemoveFromMultiList(Communicators, entityType, entityID);
			}
			if (entityType.TerminalType != null)
			{
				Common.RemoveFromMultiList(Terminals, entityType.TerminalType.TypeOfTerminal, entityID);
			}
			if (entityType.ItemType == null)
			{
				return;
			}
			Common.RemoveFromMultiList(Items, entityType, entityID);
			if (entityType.IsMountableWeapon())
			{
				AttackType[] attackTypes = entityType.ItemType.WeaponType.AttackTypes;
				foreach (AttackType key in attackTypes)
				{
					if (WeaponsByAttackType.TryGetValue(key, out var value))
					{
						value.Remove(entityID);
					}
				}
			}
			if (entityType.ItemType.AmmunitionType != null && AmmoItems.TryGetValue(entityType, out var value2))
			{
				value2.Items.Remove(entityID);
				value2.TotalIsDirty = true;
			}
			if (entityType.ItemType.FoodType != null)
			{
				Common.RemoveFromMultiList(Food, entityType, entityID);
			}
			return;
		}
		if (Vehicles != null)
		{
			Vehicles.Remove(entityID);
		}
		using (Dictionary<EntityType, List<EntityID>>.Enumerator enumerator = AllEntities.GetEnumerator())
		{
			while (enumerator.MoveNext() && !enumerator.Current.Value.Remove(entityID))
			{
			}
		}
		if (Items != null)
		{
			using Dictionary<EntityType, List<EntityID>>.Enumerator enumerator = Items.GetEnumerator();
			while (enumerator.MoveNext() && !enumerator.Current.Value.Remove(entityID))
			{
			}
		}
		using (Dictionary<AttackType, List<EntityID>>.Enumerator enumerator2 = WeaponsByAttackType.GetEnumerator())
		{
			while (enumerator2.MoveNext() && !enumerator2.Current.Value.Remove(entityID))
			{
			}
		}
		using (Dictionary<EntityType, List<EntityID>>.Enumerator enumerator = Structures.GetEnumerator())
		{
			while (enumerator.MoveNext() && !enumerator.Current.Value.Remove(entityID))
			{
			}
		}
		using (Dictionary<EntityType, List<EntityID>>.Enumerator enumerator = Communicators.GetEnumerator())
		{
			while (enumerator.MoveNext() && !enumerator.Current.Value.Remove(entityID))
			{
			}
		}
		foreach (KeyValuePair<EntityType, OwnerAmmoOfType> ammoItem in AmmoItems)
		{
			if (ammoItem.Value.Items.Remove(entityID))
			{
				ammoItem.Value.TotalIsDirty = true;
				break;
			}
		}
		using (Dictionary<EntityType, List<EntityID>>.Enumerator enumerator = Food.GetEnumerator())
		{
			while (enumerator.MoveNext() && !enumerator.Current.Value.Remove(entityID))
			{
			}
		}
		using Dictionary<TerminalType.TypesOfTerminal, List<EntityID>>.Enumerator enumerator4 = Terminals.GetEnumerator();
		while (enumerator4.MoveNext() && !enumerator4.Current.Value.Remove(entityID))
		{
		}
	}

	public void DeleteEntity(Entity entity)
	{
		DeleteEntity(entity.EntityID, entity.EntityType);
	}

	private bool IsVehicle(EntityType entityType)
	{
		if (entityType.ContainerType != null)
		{
			return entityType.ContainerType is VehicleContainerType;
		}
		return false;
	}

	public void AddJob(Job job)
	{
		AddOrRemoveJob(job, add: true);
	}

	public void RemoveJob(Job job)
	{
		AddOrRemoveJob(job, add: false);
	}

	public void Update(GameTime gameTime)
	{
		if (HaulingJobManager != null)
		{
			HaulingJobManager.Update(gameTime);
		}
		if (OtherJobManager != null)
		{
			OtherJobManager.Update(gameTime);
		}
		if (HuntingJobManager != null)
		{
			HuntingJobManager.Update(gameTime);
		}
		if (TradeManager != null)
		{
			TradeManager.Update(gameTime);
		}
	}

	private void AddOrRemoveJob(Job job, bool add)
	{
		if (add)
		{
			allJobs.Add(job.ID);
		}
		else
		{
			allJobs.Remove(job.ID);
		}
		if (job is IVariableMaxTakerJob)
		{
			if (add)
			{
				variableMaxTakerJobs.Add(job);
			}
			else
			{
				variableMaxTakerJobs.Remove(job);
			}
		}
		if (job is HaulingJob item)
		{
			if (add)
			{
				HaulingJobs.Add(item);
			}
			else
			{
				HaulingJobs.Remove(item);
			}
			if (job is HaulingJobAnyItemOfType haulingJobAnyItemOfType)
			{
				if (add)
				{
					Common.AddToMultiList(HaulingJobsAnyItemOfType, haulingJobAnyItemOfType.RequiredItemType, haulingJobAnyItemOfType);
				}
				else
				{
					Common.RemoveFromMultiList(HaulingJobsAnyItemOfType, haulingJobAnyItemOfType.RequiredItemType, haulingJobAnyItemOfType);
				}
			}
			if (job is HaulingJobSpecificItem haulingJobSpecificItem)
			{
				if (add)
				{
					SpecificHaulingJobs[haulingJobSpecificItem.Item.Value] = haulingJobSpecificItem;
				}
				else
				{
					SpecificHaulingJobs.Remove(haulingJobSpecificItem.Item.Value);
				}
			}
		}
		else
		{
			if (job is ProcessJob processJob)
			{
				if (processJob.RepairJob != null)
				{
					if (add)
					{
						Common.AddToMultiList(RepairJobs, processJob.RepairJob.EntityToRepair, processJob);
					}
					else
					{
						Common.RemoveFromMultiList(RepairJobs, processJob.RepairJob.EntityToRepair, processJob, removeEmptyList: true);
					}
					return;
				}
				if (processJob.OutputEntityType != null)
				{
					if (add)
					{
						Common.AddToMultiList(ProductionJobs, processJob.OutputEntityType, processJob);
						if (JobManager.IsManagedProductionJob(processJob))
						{
							Common.AddToMultiList(ManagedProductionJobs, processJob.OutputEntityType, processJob);
						}
						if (processJob.IsUnattended())
						{
							Common.AddToList(ref UnattendedProcessJobs, processJob);
						}
						UpdateImportance(processJob.OutputEntityType);
					}
					else
					{
						Common.RemoveFromMultiList(ProductionJobs, processJob.OutputEntityType, processJob);
						if (JobManager.IsManagedProductionJob(processJob))
						{
							Common.RemoveFromMultiList(ManagedProductionJobs, processJob.OutputEntityType, processJob);
						}
						if (processJob.IsUnattended())
						{
							UnattendedProcessJobs.Remove(processJob);
						}
					}
				}
				else if (add)
				{
					otherJobs.Add(processJob);
				}
				else
				{
					otherJobs.Remove(processJob);
				}
				if (processJob.ProcessType.InputsByType == null)
				{
					return;
				}
				{
					foreach (KeyValuePair<EntityType, Input> item6 in processJob.ProcessType.InputsByType)
					{
						if (add)
						{
							Common.AddToMultiList(ProductionJobsByInput, item6.Key, processJob);
						}
						else
						{
							Common.RemoveFromMultiList(ProductionJobsByInput, item6.Key, processJob);
						}
					}
					return;
				}
			}
			if (job is HuntingJob item2)
			{
				if (add)
				{
					otherJobs.Add(item2);
				}
				else
				{
					otherJobs.Remove(item2);
				}
			}
			else if (job is FindPreyJob findPreyJob)
			{
				if (add)
				{
					findPreyJobs.Add(findPreyJob);
					if (findPreyJob.Zone.ZoneHunt.CreaturesToHunt == null)
					{
						return;
					}
					{
						foreach (KeyValuePair<EntityType, int> item7 in findPreyJob.Zone.ZoneHunt.CreaturesToHunt)
						{
							if (item7.Value > 0)
							{
								UpdateImportance(item7.Key.BiologicalType.CarcassType);
							}
						}
						return;
					}
				}
				findPreyJobs.Remove(findPreyJob);
			}
			else if (job is ScoutingJob item3)
			{
				if (add)
				{
					scoutingJobs.Add(item3);
				}
				else
				{
					scoutingJobs.Remove(item3);
				}
			}
			else if (job is PatrolJob item4)
			{
				if (add)
				{
					patrolJobs.Add(item4);
				}
				else
				{
					patrolJobs.Remove(item4);
				}
			}
			else if (job is AttackAreaJob item5)
			{
				if (add)
				{
					attackAreaJobs.Add(item5);
				}
				else
				{
					attackAreaJobs.Remove(item5);
				}
			}
			else if (job is CheckProcessJob checkProcessJob)
			{
				if (add)
				{
					CheckProcessJobs.Add(checkProcessJob);
					CheckProcessJobsQuadTree.AddObject(checkProcessJob.ID, checkProcessJob.Location.ToVector2());
				}
				else
				{
					CheckProcessJobs.Remove(checkProcessJob);
					CheckProcessJobsQuadTree.RemoveObject(checkProcessJob.ID);
				}
			}
			else
			{
				if (!(job is ThreatJob threatJob))
				{
					return;
				}
				if (add)
				{
					if (threatJob.IsVermin)
					{
						AssetThreatJobs.Add(threatJob);
						AssetThreatJobsByTarget.Add(threatJob.Target.Value, threatJob);
					}
					else
					{
						ThreatJobs.Add(threatJob);
						ThreatJobsByTarget.Add(threatJob.Target.Value, threatJob);
					}
				}
				else if (threatJob.IsVermin)
				{
					AssetThreatJobs.Remove(threatJob);
					AssetThreatJobsByTarget.Remove(threatJob.Target.Value);
				}
				else
				{
					ThreatJobs.Remove(threatJob);
					ThreatJobsByTarget.Remove(threatJob.Target.Value);
				}
			}
		}
	}

	private void UpdateImportance(EntityType entityType)
	{
		if (!GetAllegiance().FoodExtraction.IsEatable(entityType))
		{
			if (!ProductionImportance.ContainsKey(entityType))
			{
				RecomputeImportance(entityType, null);
			}
		}
		else if (!FoodProductionImportance.HasValue)
		{
			RecomputeFoodProductionImportance(null);
		}
	}

	public void AddEntity(Entity entity)
	{
		if (Contains(entity))
		{
			return;
		}
		EntityType entityType = entity.EntityType;
		allEntities.Add(entity.ID, entity.ID);
		Common.AddToMultiList(AllEntities, entityType, entity.ID);
		if (IsVehicle(entityType))
		{
			Vehicles.Add(entity.EntityID);
		}
		if (entityType.ItemType != null)
		{
			if (!Items.TryGetValue(entityType, out var value))
			{
				value = new List<EntityID>();
				Items.Add(entityType, value);
			}
			value.Add(entity.EntityID);
			if (entityType.IsMountableWeapon())
			{
				AttackType[] attackTypes = entityType.ItemType.WeaponType.AttackTypes;
				foreach (AttackType key in attackTypes)
				{
					Common.AddToMultiList(WeaponsByAttackType, key, entity.EntityID);
				}
			}
			if (entityType.ItemType.AmmunitionType != null)
			{
				if (!AmmoItems.TryGetValue(entityType, out var value2))
				{
					value2 = new OwnerAmmoOfType
					{
						Items = new List<EntityID>()
					};
					AmmoItems.Add(entityType, value2);
				}
				value2.Items.Add(entity.EntityID);
				value2.TotalIsDirty = true;
			}
			if (Parent.Allegiance.Site.IsPlaySite && Parent.IsEatable(entity.EntityType))
			{
				Common.AddToMultiList(food, entity.EntityType, entity.EntityID);
			}
		}
		if (entityType.StructureType != null)
		{
			Common.AddToMultiList(Structures, entityType, entity.EntityID);
			if (entity.Contains != null && entity.Contains is IStorage)
			{
				Stockpile value3 = new Stockpile(Stockpile.TypesOfStockpiles.Normal, entity.EntityType.ContainerType.GetDefaultStorageSettings());
				StructureStockpiles.Add(entity.EntityID, value3);
			}
		}
		if (entityType.ContainerType != null && entityType.ContainerType.CanBeUpgraded)
		{
			Dictionary<UpgradeCategory, EntityType> dictionary = new Dictionary<UpgradeCategory, EntityType>();
			foreach (UpgradeCategory upgradeOption in entityType.ContainerType.GetUpgradeOptions())
			{
				dictionary.Add(upgradeOption, null);
			}
			Upgrades.Add(entity.ID, dictionary);
		}
		if (entityType.CommunicatorType != null)
		{
			Common.AddToMultiList(Communicators, entityType, entity.EntityID);
		}
		if (entityType.TerminalType != null)
		{
			Common.AddToMultiList(Terminals, entityType.TerminalType.TypeOfTerminal, entity.ID);
			if (entityType.ContainerType != null && entityType.ContainerType is TerminalContainerType)
			{
				Stockpile value4 = new Stockpile(Stockpile.TypesOfStockpiles.OfferedForTrade, null);
				TerminalTradeOffers.Add(entity.EntityID, value4);
			}
		}
	}

	public void SetAmmoDirty(EntityType ammoType)
	{
		if (AmmoItems.TryGetValue(ammoType, out var value))
		{
			value.TotalIsDirty = true;
		}
	}

	public EntityType GetOrderedUpgrade(EntityID entityID, UpgradeCategory category)
	{
		EntityType result = null;
		if (Upgrades.TryGetValue(entityID, out var value) && value.TryGetValue(category, out var value2))
		{
			result = value2;
		}
		return result;
	}

	public bool GetIsUpgrade(EntityID entityID, UpgradeCategory category, EntityType entityType)
	{
		if (Upgrades.TryGetValue(entityID, out var value) && value.TryGetValue(category, out var value2) && value2 == entityType)
		{
			return true;
		}
		return false;
	}

	public void SetUpgrade(EntityID entityID, UpgradeCategory upgradeCategory, EntityType upgradeType)
	{
		Common.AddToNestedDictionary(Upgrades, entityID, upgradeCategory, upgradeType);
	}

	public bool Contains(IKnownEntityData entity)
	{
		return allEntities.ContainsKey(entity.EntityID);
	}

	public bool Contains(Entity entity)
	{
		return allEntities.ContainsKey(entity.ID);
	}

	public void IterateEntities(Action<Entity> iterateMethod)
	{
		foreach (KeyValuePair<EntityID, EntityID> allEntity in allEntities)
		{
			Entity entity = Entity.FindByID(allEntity.Value);
			if (entity != null)
			{
				iterateMethod(entity);
			}
		}
	}

	public bool Buy(IOwner buyer, Dictionary<EntityType, List<EntityID>> order, bool buyReducedAmountsIfNeeded, decimal? maxAmountToSpend, out decimal spentAmount, out List<Entity> boughtItems)
	{
		spentAmount = default(decimal);
		boughtItems = new List<Entity>();
		Dictionary<EntityType, List<Entity>> dictionary = new Dictionary<EntityType, List<Entity>>();
		foreach (KeyValuePair<EntityType, List<EntityID>> item in order)
		{
			for (int i = 0; i < item.Value.Count; i++)
			{
				Entity entity = Entity.FindByID(item.Value[i]);
				if (entity != null)
				{
					Common.AddToMultiList(dictionary, entity.EntityType, entity);
				}
			}
		}
		List<Entity> value2;
		if (!buyReducedAmountsIfNeeded && maxAmountToSpend.HasValue)
		{
			decimal value = maxAmountToSpend.Value;
			decimal num = default(decimal);
			foreach (KeyValuePair<EntityType, List<EntityID>> item2 in order)
			{
				if (dictionary.TryGetValue(item2.Key, out value2))
				{
					if (item2.Value.Count != value2.Count)
					{
						return false;
					}
					decimal? tradePrice = BuySellActionTemplate.GetTradePrice(item2.Key, buyer.OwnedEntities, this);
					num += (decimal)item2.Value.Count * tradePrice.Value;
					if (num > value)
					{
						return false;
					}
					continue;
				}
				return false;
			}
		}
		Entity.GiveNewOwnerKnowledge giveNewOwnerKnowledge = Entity.GiveNewOwnerKnowledge.Yes;
		foreach (KeyValuePair<EntityType, List<EntityID>> item3 in order)
		{
			if (dictionary.TryGetValue(item3.Key, out value2))
			{
				decimal? tradePrice2 = BuySellActionTemplate.GetTradePrice(item3.Key, buyer.OwnedEntities, this);
				for (int j = 0; j < value2.Count; j++)
				{
					Entity entity = value2[j];
					entity.ChangeOwnership(buyer, giveNewOwnerKnowledge);
					boughtItems.Add(entity);
					spentAmount += tradePrice2.Value;
				}
				if (buyer.OwnedEntities.TradeManager != null)
				{
					buyer.OwnedEntities.TradeManager.Buy(item3.Key, boughtItems.Count);
				}
			}
		}
		MakeTradeCreditsTransaction(buyer.OwnedEntities.Parent, Parent, spentAmount);
		return true;
	}

	public int CountOutstandingJobOutput(EntityType entityType, bool countUnstarted, out int currentJobs, out float averageSpeed)
	{
		ProductionJobs.TryGetValue(entityType, out var value);
		if (value != null)
		{
			currentJobs = value.Count;
		}
		else
		{
			currentJobs = 0;
		}
		return CountJobOutput(entityType, countUnstarted, value, out averageSpeed);
	}

	private static int CountJobOutput(EntityType entityType, bool countUnstarted, List<ProcessJob> existingProductionJobs, out float averageSpeed)
	{
		int num = 0;
		averageSpeed = 0f;
		float num2 = 0f;
		int num3 = 0;
		if (existingProductionJobs != null)
		{
			for (int num4 = existingProductionJobs.Count - 1; num4 >= 0; num4--)
			{
				ProcessJob processJob = existingProductionJobs[num4];
				if (countUnstarted || (processJob.IsStarted(out var isStarted) && isStarted))
				{
					num += processJob.ProcessType.GetOutputAmount(entityType) ?? 0;
					num2 += processJob.GetProgressSpeed();
					num3++;
				}
			}
		}
		if (num3 > 0)
		{
			averageSpeed = num2 / (float)num3;
		}
		return num;
	}

	public int CountAvailableItems(EntityType entityType)
	{
		int result = 0;
		int noOfIncompleteEntities = 0;
		int noOfAvailableEntities = 0;
		int noOfAvailableEntitiesIncludingIntrinsic = 0;
		int noOfEntitiesUsedAsParts = 0;
		int noOfItemsOnOtherSite = 0;
		int noOfItemsOwnedByOthers = 0;
		List<EntityID> listOfAvailableEntities = null;
		List<EntityID> listOfUnavailableEntities = null;
		if (AllEntities.TryGetValue(entityType, out var value))
		{
			foreach (EntityID item in value)
			{
				Entity entity = Entity.FindByID(item);
				if (entity != null)
				{
					CountEntity(GetOwnerID(), entity, ref noOfIncompleteEntities, ref noOfEntitiesUsedAsParts, ref noOfItemsOnOtherSite, ref noOfItemsOwnedByOthers, ref noOfAvailableEntities, ref noOfAvailableEntitiesIncludingIntrinsic, ref listOfAvailableEntities, ref listOfUnavailableEntities);
				}
			}
			result = noOfAvailableEntities;
		}
		return result;
	}

	public static void MakeTradeCreditsTransaction(IOwner buyer, IOwner seller, decimal amount)
	{
		IHasEntityGroup parent = buyer.OwnedEntities.Parent;
		parent.TradeCredits -= (decimal?)amount;
		IHasEntityGroup parent2 = seller.OwnedEntities.Parent;
		parent2.TradeCredits += (decimal?)amount;
	}

	public static void MakeTradeCreditsTransaction(IHasEntityGroup buyer, IHasEntityGroup seller, decimal amount)
	{
		buyer.TradeCredits -= (decimal?)amount;
		seller.TradeCredits += (decimal?)amount;
	}

	public void GetEntities(FilterCondition filter, ref List<IHasExposedProperties> listToFillWithProperties, out bool wasFiltered, EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource)
	{
		if (filter is PropertyCondition { PropertyKey: "type" } propertyCondition)
		{
			if (AllEntities.TryGetValue(GameData.Instance.AllEntityTypes[propertyCondition.ConstantStringEqual], out var value))
			{
				foreach (EntityID item in value)
				{
					if (!GoalEvaluator.EntityDataResultCausesSkip(Parent.Allegiance.SharedKnowledge.GetKnownData(item, out var data)))
					{
						listToFillWithProperties.Add(data);
					}
				}
			}
			wasFiltered = true;
			return;
		}
		foreach (KeyValuePair<EntityID, EntityID> allEntity in allEntities)
		{
			if (!GoalEvaluator.EntityDataResultCausesSkip(Parent.Allegiance.SharedKnowledge.GetKnownData(allEntity.Key, out var data2)))
			{
				listToFillWithProperties.Add(data2);
			}
		}
		wasFiltered = false;
	}

	public int GetBuyAmount(EntityType itemType)
	{
		if (TradeManager != null)
		{
			return TradeManager.GetBuyAmount(itemType);
		}
		return 0;
	}

	public float? GetBuyPrice(EntityType itemType)
	{
		if (TradeManager != null)
		{
			return TradeManager.GetBuyPrice(itemType);
		}
		return null;
	}

	public float? GetSellPrice(EntityType itemType)
	{
		if (TradeManager != null)
		{
			return TradeManager.GetSellPrice(itemType);
		}
		return null;
	}

	public static void CountEntity(OwnerID? thisOwner, IKnownEntityData itemData, ref int noOfIncompleteEntities, ref int noOfEntitiesUsedAsParts, ref int noOfItemsOnOtherSite, ref int noOfItemsOwnedByOthers, ref int noOfAvailableEntities, ref int noOfAvailableEntitiesIncludingIntrinsic, ref List<EntityID> listOfAvailableEntities, ref List<EntityID> listOfUnavailableEntities)
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = true;
		if (itemData.PartOfID.HasValue)
		{
			flag3 = true;
			noOfEntitiesUsedAsParts++;
		}
		else if (!itemData.Location.HasValue)
		{
			flag2 = true;
			noOfItemsOnOtherSite++;
		}
		else if (thisOwner.HasValue && itemData.OwnedBy != thisOwner)
		{
			flag = true;
			noOfItemsOwnedByOthers++;
		}
		else if (!itemData.IsCompleted())
		{
			flag4 = false;
			noOfIncompleteEntities++;
		}
		if (flag4 && !flag2 && !flag && !flag3)
		{
			noOfAvailableEntities++;
			noOfAvailableEntitiesIncludingIntrinsic++;
			if (listOfAvailableEntities != null)
			{
				listOfAvailableEntities.Add(itemData.EntityID);
			}
			return;
		}
		if (flag3 && itemData.EntityType.IsIntrinsic())
		{
			noOfAvailableEntitiesIncludingIntrinsic++;
		}
		if (listOfUnavailableEntities != null)
		{
			listOfUnavailableEntities.Add(itemData.EntityID);
		}
	}

	public decimal? GetVehicleForHirePrice(EntityType itemType, out decimal? pricePerKilometer)
	{
		if (TradeManager != null)
		{
			return TradeManager.GetPriceToHire(itemType, out pricePerKilometer);
		}
		pricePerKilometer = null;
		return null;
	}

	public bool IsOwnedByAllegiance(Allegiance allegiance)
	{
		return GetAllegiance() == allegiance;
	}

	public Allegiance GetAllegiance()
	{
		return Parent.Allegiance;
	}

	public Expedition GetExpedition()
	{
		return Parent as Expedition;
	}

	public EntityResult GetKnownData(EntityID entityID, out IKnownEntityData data)
	{
		if (Parent.Allegiance.SharedKnowledge != null)
		{
			return Parent.Allegiance.SharedKnowledge.GetKnownData(entityID, out data);
		}
		data = Entity.FindByID(entityID);
		if (data != null)
		{
			return EntityResult.SeenDirectly;
		}
		return EntityResult.Destroyed;
	}

	public void SetNeutralImportance(EntityType entityType)
	{
		ProductionImportance[entityType] = 0.5f;
	}

	public void RecomputeImportance(EntityType entityType, Dictionary<EntityType, InventoryPanel.Availability> allAvailableItems)
	{
		double num = ScoreHowLongStocksWillLast(entityType, allAvailableItems);
		double urgencyScore = 1.0 - num;
		double survivalScore = 0.0;
		double num2 = CalculateImportance(urgencyScore, survivalScore);
		ProductionImportance[entityType] = (float)num2;
	}

	private static double CalculateImportance(double urgencyScore, double survivalScore)
	{
		return 0.75 * urgencyScore + 0.25 * survivalScore;
	}

	private double ScoreSurvivalImportance(bool isEatable)
	{
		if (isEatable)
		{
			return 1.0;
		}
		return 0.0;
	}

	private double ScoreHowLongStocksWillLast(EntityType entityType, Dictionary<EntityType, InventoryPanel.Availability> allAvailableItems)
	{
		float intervalInDaysForComputingProductImportance = GameData.Instance.AIConstants.IntervalInDaysForComputingProductImportance;
		DateAndTime.TimeDateYear currentTimeDateYear = The.Sim.DateAndTime.CurrentTimeDateYear;
		DateAndTime.TimeDateYear timeDateYear = currentTimeDateYear;
		timeDateYear.AddTime(0f - intervalInDaysForComputingProductImportance);
		float consumedRate = ComputeEventRate(entityType, timeDateYear, currentTimeDateYear, intervalInDaysForComputingProductImportance, ProductionStatistics.StatTypes.UsedAsInput);
		float productionRate = ComputeEventRate(entityType, timeDateYear, currentTimeDateYear, intervalInDaysForComputingProductImportance, ProductionStatistics.StatTypes.Produced);
		return ScoreHowLongStocksWillLast(GetCurrentStockAmount(entityType, allAvailableItems), productionRate, consumedRate);
	}

	private float ComputeEventRate(EntityType entityType, DateAndTime.TimeDateYear from, DateAndTime.TimeDateYear to, double interval, ProductionStatistics.StatTypes statType)
	{
		return (float)((double)Statistic.SumDataPoints(GetAllegiance().Statistics.ProductionStatistics.Stats[statType], entityType, from, to) / interval);
	}

	private void ComputeProductionRate(EntityType entityType, out int currentStockAmount, out int stockpiledAndProduced, out float productionRate, Dictionary<EntityType, InventoryPanel.Availability> allAvailableItems)
	{
		currentStockAmount = GetCurrentStockAmount(entityType, allAvailableItems);
		int currentJobs;
		float averageSpeed;
		int num = CountOutstandingJobOutput(entityType, countUnstarted: false, out currentJobs, out averageSpeed);
		stockpiledAndProduced = currentStockAmount + num;
		productionRate = (float)num * averageSpeed;
	}

	private int GetCurrentStockAmount(EntityType entityType, Dictionary<EntityType, InventoryPanel.Availability> allAvailableItems)
	{
		if (allAvailableItems == null || !allAvailableItems.TryGetValue(entityType, out var value))
		{
			return CountAvailableItems(entityType);
		}
		return value.NoOfAvailableItems;
	}

	private static double ScoreHowLongStocksWillLast(int currentStockAmount, float productionRate, float consumedRate)
	{
		if (productionRate > consumedRate)
		{
			return 1.0;
		}
		float num = consumedRate - productionRate;
		return Common.Clamp((float)currentStockAmount / num, 0f, 1f);
	}

	public EntityGroupID GetUniqueID()
	{
		IDCounter++;
		if (IDCounter >= EntityGroupID.Invalid)
		{
			throw new Exception("Astounding, OwnerID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public EntityGroupID SnapshotID(Snapshotter sn, EntityGroupID id)
	{
		return sn.DoEnum(id);
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != EntityGroupID.Invalid)
		{
			LookUp<EntityGroup, EntityGroupID>.Add(ID, this);
		}
	}

	public void SetInvalid()
	{
		id = EntityGroupID.Invalid;
	}

	public void RemoveIDEntry()
	{
		LookUp<EntityGroup, EntityGroupID>.Remove(this);
	}

	void ILookUp<EntityGroup, EntityGroupID>.ResetIDCounter()
	{
	}

	public static void ResetIDCounter()
	{
		IDCounter = EntityGroupID.First;
	}

	void ILookUp<EntityGroup, EntityGroupID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUp<EntityGroup, EntityGroupID>.Create();
		LookUp<EntityGroup, EntityGroupID>.SetLoadPostProcessOrder(10);
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
		allEntities = sn.DoDictionary(allEntities);
		AllEntities = sn.DoMultiMap(AllEntities);
		activities = sn.DoList(activities);
		AmmoItems = sn.DoDictionary(AmmoItems);
		food = sn.DoMultiMap(food);
		foodIsDirty = sn.DoBool(foodIsDirty);
		items = sn.DoMultiMap(items);
		Structures = sn.DoMultiMap(Structures);
		StructureStockpiles = sn.DoDictionary(StructureStockpiles);
		TerminalTradeOffers = sn.DoDictionary(TerminalTradeOffers);
		Upgrades = sn.DoNestedDictionary(Upgrades);
		Communicators = sn.DoMultiMap(Communicators);
		vehicles = sn.DoList(vehicles);
		WeaponsByAttackType = sn.DoMultiMap(WeaponsByAttackType);
		FulfillsNeeds = sn.DoMultiMap(FulfillsNeeds);
		Terminals = sn.DoMultiMap(Terminals);
		Policy = (EntityGroupPolicy)sn.DoISnapshot(Policy);
		ProductionOrders = (ProductionOrders)sn.DoISnapshot(ProductionOrders);
		foodConsumeRate = sn.DoFloat(foodConsumeRate);
		if (sn.mode != Snapshotter.Mode.Load)
		{
			snapshotZones = Zones.Select((Zone z) => z.ID).ToList();
		}
		snapshotZones = sn.DoList(snapshotZones);
		snapshotHaulingJobManager = sn.SnapshotID<ICyclable, CyclableID>(HaulingJobManager);
		snapshotOtherJobManager = sn.SnapshotID<ICyclable, CyclableID>(OtherJobManager);
		snapshotHuntingJobManager = sn.SnapshotID<ICyclable, CyclableID>(HuntingJobManager);
		allJobs = sn.DoList(allJobs);
		SnapshotJobList(sn, ref snapshotThreatJobs, ThreatJobs);
		SnapshotJobList(sn, ref snapshotAssetThreatJobs, AssetThreatJobs);
		SnapshotJobList(sn, ref snapshotHaulingJobs, HaulingJobs);
		SnapshotJobList(sn, ref snapshotOtherJobs, otherJobs);
		SnapshotJobList(sn, ref snapshotPatrolJobs, patrolJobs);
		SnapshotJobList(sn, ref snapshotAttackAreaJobs, attackAreaJobs);
		SnapshotJobList(sn, ref snapshotScoutingJobs, scoutingJobs);
		SnapshotJobList(sn, ref snapshotUnattendedProcessJobs, UnattendedProcessJobs);
		SnapshotJobList(sn, ref snapshotFindPreyJobs, findPreyJobs);
		SnapshotJobList(sn, ref snapshotCheckProcessJobs, CheckProcessJobs);
		SnapshotJobList(sn, ref snapshotVariableMaxTakerJobs, VariableMaxTakerJobs);
		TradeManager = (TradeManager)sn.DoISnapshot(TradeManager);
		if (sn.mode != Snapshotter.Mode.Load)
		{
			snapshotCheckProcessJobsQuadTree = (from p in CheckProcessJobsQuadTree.GetAllObjectsAndPositions()
				select new Pair<JobID, Vector2>(p.First, p.Second)).ToList();
			snapshotProductionJobs = new Dictionary<EntityType, List<JobID>>();
			foreach (KeyValuePair<EntityType, List<ProcessJob>> productionJob in ProductionJobs)
			{
				snapshotProductionJobs.Add(productionJob.Key, productionJob.Value.Select((ProcessJob i) => i.ID).ToList());
			}
			snapshotManagedProductionJobs = new Dictionary<EntityType, List<JobID>>();
			foreach (KeyValuePair<EntityType, List<ProcessJob>> managedProductionJob in ManagedProductionJobs)
			{
				snapshotManagedProductionJobs.Add(managedProductionJob.Key, managedProductionJob.Value.Select((ProcessJob i) => i.ID).ToList());
			}
			snapshotProductionJobsByInput = new Dictionary<EntityType, List<JobID>>();
			foreach (KeyValuePair<EntityType, List<ProcessJob>> item in ProductionJobsByInput)
			{
				snapshotProductionJobsByInput.Add(item.Key, item.Value.Select((ProcessJob i) => i.ID).ToList());
			}
			snapshotHaulingJobsAnyItemOfType = new Dictionary<EntityType, List<JobID>>();
			foreach (KeyValuePair<EntityType, List<HaulingJobAnyItemOfType>> item2 in HaulingJobsAnyItemOfType)
			{
				snapshotHaulingJobsAnyItemOfType.Add(item2.Key, item2.Value.Select((HaulingJobAnyItemOfType i) => i.ID).ToList());
			}
			snapshotRepairJobs = new Dictionary<EntityID, List<JobID>>();
			foreach (KeyValuePair<EntityID, List<ProcessJob>> repairJob in RepairJobs)
			{
				snapshotRepairJobs.Add(repairJob.Key, repairJob.Value.Select((ProcessJob i) => i.ID).ToList());
			}
			snapshotSpecificHaulingJobs = new Dictionary<EntityID, JobID>();
			foreach (KeyValuePair<EntityID, HaulingJobSpecificItem> specificHaulingJob in SpecificHaulingJobs)
			{
				snapshotSpecificHaulingJobs.Add(specificHaulingJob.Key, specificHaulingJob.Value.ID);
			}
		}
		snapshotProductionJobs = sn.DoMultiMap(snapshotProductionJobs);
		snapshotManagedProductionJobs = sn.DoMultiMap(snapshotManagedProductionJobs);
		snapshotProductionJobsByInput = sn.DoMultiMap(snapshotProductionJobsByInput);
		snapshotHaulingJobsAnyItemOfType = sn.DoMultiMap(snapshotHaulingJobsAnyItemOfType);
		snapshotRepairJobs = sn.DoMultiMap(snapshotRepairJobs);
		snapshotSpecificHaulingJobs = sn.DoDictionary(snapshotSpecificHaulingJobs);
		snapshotParent = sn.SnapshotID<IHasEntityGroup, HasEntityGroupID>(Parent).Value;
		CheckProcessJobsQuadTree = (PointQuadTree<JobID>)sn.DoISnapshot(CheckProcessJobsQuadTree);
		snapshotCheckProcessJobsQuadTree = sn.DoList(snapshotCheckProcessJobsQuadTree);
		FoodProductionImportance = sn.DoFloatNullable(FoodProductionImportance);
		ProductionImportance = sn.DoDictionary(ProductionImportance);
		sn.Ignore(ThreatJobsByTarget);
		sn.Ignore(AssetThreatJobsByTarget);
		sn.Ignore(HaulingJobManager);
		sn.Ignore(OtherJobManager);
		sn.Ignore(HuntingJobManager);
		sn.Ignore(Zones);
		sn.Ignore(ProductionJobs);
		sn.Ignore(ProductionJobsByInput);
		sn.Ignore(ManagedProductionJobs);
		sn.Ignore(HaulingJobsAnyItemOfType);
		sn.Ignore(SpecificHaulingJobs);
		sn.Ignore(RepairJobs);
		return this;
	}

	private static void SnapshotJobList(Snapshotter sn, ref List<JobID> snapshotList, List<Job> jobsList)
	{
		snapshotList = jobsList.Select((Job j) => j.ID).ToList();
		snapshotList = sn.DoList(snapshotList);
		sn.Ignore(jobsList);
	}

	private static void PostLoadJobsList(Snapshotter sn, List<JobID> snapshotList, ref List<Job> jobsList)
	{
		jobsList = snapshotList.Select((JobID j) => LookUp<Job, JobID>.FindByID(j)).ToList();
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		ThreatJobs = snapshotThreatJobs.Select((JobID j) => LookUp<Job, JobID>.FindByID(j)).ToList();
		AssetThreatJobs = snapshotAssetThreatJobs.Select((JobID j) => LookUp<Job, JobID>.FindByID(j)).ToList();
		PostLoadJobsList(sn, snapshotThreatJobs, ref ThreatJobs);
		PostLoadJobsList(sn, snapshotAssetThreatJobs, ref AssetThreatJobs);
		PostLoadJobsList(sn, snapshotScoutingJobs, ref scoutingJobs);
		PostLoadJobsList(sn, snapshotPatrolJobs, ref patrolJobs);
		PostLoadJobsList(sn, snapshotAttackAreaJobs, ref attackAreaJobs);
		PostLoadJobsList(sn, snapshotOtherJobs, ref otherJobs);
		PostLoadJobsList(sn, snapshotUnattendedProcessJobs, ref UnattendedProcessJobs);
		PostLoadJobsList(sn, snapshotFindPreyJobs, ref findPreyJobs);
		PostLoadJobsList(sn, snapshotHaulingJobs, ref HaulingJobs);
		PostLoadJobsList(sn, snapshotVariableMaxTakerJobs, ref variableMaxTakerJobs);
		foreach (ThreatJob threatJob3 in ThreatJobs)
		{
			ThreatJobsByTarget.Add(threatJob3.Target.Value, threatJob3);
		}
		foreach (ThreatJob assetThreatJob in AssetThreatJobs)
		{
			AssetThreatJobsByTarget.Add(assetThreatJob.Target.Value, assetThreatJob);
		}
		foreach (KeyValuePair<EntityType, List<JobID>> snapshotProductionJob in snapshotProductionJobs)
		{
			ProductionJobs.Add(snapshotProductionJob.Key, snapshotProductionJob.Value.Select((JobID j) => (ProcessJob)LookUp<Job, JobID>.FindByID(j)).ToList());
		}
		foreach (KeyValuePair<EntityType, List<JobID>> snapshotManagedProductionJob in snapshotManagedProductionJobs)
		{
			ManagedProductionJobs.Add(snapshotManagedProductionJob.Key, snapshotManagedProductionJob.Value.Select((JobID j) => (ProcessJob)LookUp<Job, JobID>.FindByID(j)).ToList());
		}
		foreach (KeyValuePair<EntityType, List<JobID>> item in snapshotProductionJobsByInput)
		{
			ProductionJobsByInput.Add(item.Key, item.Value.Select((JobID j) => (ProcessJob)LookUp<Job, JobID>.FindByID(j)).ToList());
		}
		foreach (KeyValuePair<EntityType, List<JobID>> item2 in snapshotHaulingJobsAnyItemOfType)
		{
			HaulingJobsAnyItemOfType.Add(item2.Key, item2.Value.Select((JobID j) => (HaulingJobAnyItemOfType)LookUp<Job, JobID>.FindByID(j)).ToList());
		}
		foreach (KeyValuePair<EntityID, JobID> snapshotSpecificHaulingJob in snapshotSpecificHaulingJobs)
		{
			SpecificHaulingJobs.Add(snapshotSpecificHaulingJob.Key, (HaulingJobSpecificItem)LookUp<Job, JobID>.FindByID(snapshotSpecificHaulingJob.Value));
		}
		foreach (KeyValuePair<EntityID, List<JobID>> snapshotRepairJob in snapshotRepairJobs)
		{
			RepairJobs.Add(snapshotRepairJob.Key, snapshotRepairJob.Value.Select((JobID j) => (ProcessJob)LookUp<Job, JobID>.FindByID(j)).ToList());
		}
		Zones = snapshotZones.Select((ZoneID z) => LookUp<Zone, ZoneID>.FindByID(z)).ToList();
		Parent = LookUpHasEntityGroup.FindByID(snapshotParent);
		if (snapshotHaulingJobManager.HasValue)
		{
			HaulingJobManager = (HaulingJobManager)LookUp<ICyclable, CyclableID>.FindByID(snapshotHaulingJobManager.Value);
		}
		if (snapshotOtherJobManager.HasValue)
		{
			OtherJobManager = (OtherJobManager)LookUp<ICyclable, CyclableID>.FindByID(snapshotOtherJobManager.Value);
		}
		if (snapshotHuntingJobManager.HasValue)
		{
			HuntingJobManager = (HuntingJobManager)LookUp<ICyclable, CyclableID>.FindByID(snapshotHuntingJobManager.Value);
		}
		if (TradeManager != null)
		{
			TradeManager.LoadPostProcess(sn);
		}
		if (StructureStockpiles != null)
		{
			foreach (KeyValuePair<EntityID, Stockpile> structureStockpile in StructureStockpiles)
			{
				structureStockpile.Value.LoadPostProcess(sn);
			}
		}
		if (TerminalTradeOffers != null)
		{
			foreach (KeyValuePair<EntityID, Stockpile> terminalTradeOffer in TerminalTradeOffers)
			{
				terminalTradeOffer.Value.LoadPostProcess(sn);
			}
		}
		if (Policy != null)
		{
			Policy.LoadPostProcess(sn);
		}
		foreach (KeyValuePair<EntityType, OwnerAmmoOfType> ammoItem in AmmoItems)
		{
			ammoItem.Value.LoadPostProcess(sn);
		}
		if (ProductionOrders != null)
		{
			ProductionOrders.LoadPostProcess(sn);
		}
		CheckProcessJobsQuadTree.SetPreLoadPostProcess(snapshotCheckProcessJobsQuadTree);
		CheckProcessJobsQuadTree.LoadPostProcess(sn);
	}
}
