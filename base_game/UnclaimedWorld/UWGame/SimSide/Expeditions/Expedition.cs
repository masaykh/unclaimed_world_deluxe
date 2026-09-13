using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.AI.Planners;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Collisions;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.GatheringSites;
using UWGame.SimSide.InGameEvents;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Policies;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems.TimeSlicing;
using UWGame.SimSide.Tiers;
using UWGame.SimSide.Trade;

namespace UWGame.SimSide.Expeditions;

[DebuggerDisplay("{KeyName}")]
public class Expedition : IHasEntityGroup, ILookUp<IHasEntityGroup, HasEntityGroupID>, IOwner, ILookUp<IOwner, OwnerID>, IHasExposedProperties, ICanIterateEntities, ILookUp<ICanIterateEntities, CanIterateEntitiesID>, ILookUp<Expedition, ExpeditionID>, ISnapshot
{
	public string Name;

	private Vector3? center;

	public ExpeditionPolicy Policy;

	private FoodExtraction foodExtraction;

	private FoodExtraction independentMemberFoodExtraction;

	public Population Population;

	public List<EntityID> Members = new List<EntityID>();

	public List<EntityID> IndependentMembers = new List<EntityID>();

	public List<EntityID> Workers = new List<EntityID>();

	public List<Household> Households = new List<Household>();

	private List<HouseholdID> snapshotHouseholds;

	public JobManager JobManager;

	private CyclableID? snapshotJobManager;

	private Dictionary<SkillType, int> membersWithSkill = new Dictionary<SkillType, int>();

	private HashSet<EntityID> membersWithUniqueSkill = new HashSet<EntityID>();

	private Regulator skillRegulator;

	public GroupStatistics Statistics;

	private EntityGroup ownerContent;

	private EntityGroupID snapshotOwnerContent;

	private Allegiance allegiance;

	private AllegianceID snapshotAllegiance;

	private PhysicalNeedsPlanner physicalNeedsPlanner;

	private GatheringSite gatheringSite;

	private GatheringSiteID? snapshotGatheringSite;

	private static Dictionary<string, GetPropertyValue> exposedPropertyValueFunctions = new Dictionary<string, GetPropertyValue>();

	private Dictionary<string, PropertyResult> customFields;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	private ExpeditionID id = ExpeditionID.Invalid;

	private static ExpeditionID IDCounter = ExpeditionID.First;

	public CanIterateEntitiesID canIterateEntitiesID;

	private HasEntityGroupID hasEntityGroupID;

	private OwnerID ownerID;

	public string KeyName { get; set; }

	public Vector3? Center
	{
		get
		{
			return center;
		}
		set
		{
			center = value;
			if (center.HasValue && Collidable != null)
			{
				Collidable.Center = center.Value.ToVector2();
			}
		}
	}

	public Collidable<Expedition> Collidable { get; set; }

	public EntityGroup OwnedEntities => ownerContent;

	public int NoOfWorkers => Workers.Count;

	public Allegiance GetAllegiance => Allegiance;

	public Vector3? Location => Center;

	public Allegiance Allegiance
	{
		get
		{
			return allegiance;
		}
		set
		{
			allegiance = value;
		}
	}

	public decimal? TradeCredits
	{
		get
		{
			return allegiance.TradeCredits;
		}
		set
		{
			allegiance.TradeCredits = value;
		}
	}

	public GatheringSite GatheringSite
	{
		get
		{
			if (gatheringSite == null)
			{
				GatheringSiteType siteType = new GatheringSiteType
				{
					arc = new Arc
					{
						Radius = 100f,
						MinAngle = -180.0,
						MaxAngle = 180.0
					},
					SeatSize = 20f,
					MaxVisitors = 20
				};
				gatheringSite = new GatheringSite(siteType, Location.Value);
			}
			return gatheringSite;
		}
	}

	public bool IsSnapshotted { get; set; }

	public ExpeditionID ID
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

	CanIterateEntitiesID ILookUp<ICanIterateEntities, CanIterateEntitiesID>.ID => canIterateEntitiesID;

	HasEntityGroupID ILookUp<IHasEntityGroup, HasEntityGroupID>.ID => hasEntityGroupID;

	OwnerID ILookUp<IOwner, OwnerID>.ID => ownerID;

	public bool IsEatable(EntityType entityType)
	{
		return foodExtraction.IsEatable(entityType);
	}

	public bool IsEatableByIndependentMembers(EntityType entityType)
	{
		return independentMemberFoodExtraction.IsEatable(entityType);
	}

	public void IterateMembers(Action<Entity> iterateFunction)
	{
		for (int num = Members.Count - 1; num >= 0; num--)
		{
			Entity entity = Entity.FindByID(Members[num]);
			if (entity != null)
			{
				iterateFunction(entity);
			}
			else
			{
				Members.RemoveAt(num);
			}
		}
	}

	public void IterateOwnedItems(Action<EntityGroup> iterateFunction)
	{
		iterateFunction(ownerContent);
	}

	public void RemoveGatheringSite()
	{
		gatheringSite = null;
	}

	public Expedition()
	{
	}

	public Expedition(Allegiance allegiance, string keyName, string name, Vector3? center)
	{
		AddToLookup();
		((ILookUp<ICanIterateEntities, CanIterateEntitiesID>)this).AddToLookup();
		((ILookUp<IHasEntityGroup, HasEntityGroupID>)this).AddToLookup();
		((ILookUp<IOwner, OwnerID>)this).AddToLookup();
		this.allegiance = allegiance;
		Name = name;
		KeyName = keyName;
		Center = center;
		ownerContent = new EntityGroup(this, manageTrade: true, manageProduction: true);
		foodExtraction = new FoodExtraction(this, ownerContent.ID);
		independentMemberFoodExtraction = new FoodExtraction(this, ownerContent.ID, includeNonIndependentMembers: false);
		Policy = new ExpeditionPolicy();
		if (allegiance.Site.IsPlaySite)
		{
			InitPlaySite();
		}
		else
		{
			InitOtherSite();
		}
		allegiance.Expeditions.Add(this);
		if (allegiance.RepresentativeEntityType.Person == null)
		{
			physicalNeedsPlanner = new PhysicalNeedsPlanner(allegiance, this);
		}
		if (allegiance.RepresentativeEntityType.PolledEvents != null && allegiance.RepresentativeEntityType.PolledEvents.TryGetValue(Scope.Expedition, out var value))
		{
			foreach (PolledEventType item in value)
			{
				if (Allegiance.Site.IsPlaySite || !item.PlaySiteOnly)
				{
					allegiance.Site.EventManager.AddPolledEvent(item.KeyName, null, ID);
				}
			}
		}
		CreateRegulators();
		CreateCollidable();
		if (Collidable != null)
		{
			allegiance.Site.PlaySite.ExpeditionRadiusQuadTree.AddCollidable(Collidable);
		}
	}

	public OwnerID GetOwnerID()
	{
		return ((ILookUp<IOwner, OwnerID>)this).ID;
	}

	private void CreateRegulators()
	{
		skillRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 0.3, "Expedition");
	}

	public static bool CreateFromExpeditionData(ExpeditionData data, Allegiance allegiance, EventAction action, float? sizeFactor, string expeditionKeyName, out string failReason)
	{
		failReason = "";
		Vector3? vector = null;
		float sizeFactor2 = data.SizeFactor ?? sizeFactor ?? 1f;
		if (data.Location != null)
		{
			PropertyResult? propertyResult = data.Location.Evaluate(action);
			if (!propertyResult.HasValue || !propertyResult.Value.LocationResult.HasValue)
			{
				failReason = "Location did not evaluate to a result";
				return false;
			}
			vector = propertyResult.Value.LocationResult.Value.ToVector3();
		}
		PricesProfile pricesProfile = null;
		if (data.PricesProfile != null)
		{
			pricesProfile = GameData.Instance.AllPricesProfiles[data.PricesProfile];
		}
		Expedition expedition = new Expedition(allegiance, expeditionKeyName ?? data.KeyName, data.Name, vector);
		if (data.TradeProfile != null)
		{
			if (allegiance.Site.IsPlaySite)
			{
				throw new Exception("Cannot create a trade manager for a playsite allegiance.");
			}
			CreateTradeManager(expedition);
			expedition.OwnedEntities.TradeManager.FillFromTradeProfile(GameData.Instance.AllTradeProfiles[data.TradeProfile], sizeFactor2, pricesProfile);
		}
		if (data.AvailableForTrade != null)
		{
			if (allegiance.Site.IsPlaySite)
			{
				throw new Exception("Cannot create a trade manager for a playsite allegiance.");
			}
			CreateTradeManager(expedition);
			if (data.AvailableForTrade != null)
			{
				expedition.OwnedEntities.TradeManager.SetTradeProperties(data.AvailableForTrade, pricesProfile);
			}
		}
		if (data.VehiclesProfile != null)
		{
			CreateTradeManager(expedition);
			expedition.OwnedEntities.TradeManager.FillFromVehiclesProfile(GameData.Instance.AllVehiclesProfiles[data.VehiclesProfile], sizeFactor2);
		}
		if (data.VehiclesForHire != null)
		{
			CreateTradeManager(expedition);
			expedition.OwnedEntities.TradeManager.SetVehiclesForHire(data.VehiclesForHire);
		}
		expedition.Policy = ExpeditionPolicy.CreateFromPolicyData(data.PolicyData);
		if (data.PopulationData != null)
		{
			expedition.Population = Population.CreateFromPopulationData(expedition, data.PopulationData);
		}
		if (expedition.Population != null)
		{
			expedition.Population.SpawnStartingPopulation();
		}
		if (data.StructuresProfile != null)
		{
			expedition.OwnedEntities.SpawnOthersiteStartingStructures(data.StructuresProfile);
		}
		if (expedition.OwnedEntities.TradeManager != null)
		{
			expedition.OwnedEntities.TradeManager.SpawnStartingTradeItems();
			expedition.OwnedEntities.TradeManager.SpawnStartingVehicles();
		}
		return true;
	}

	private static void CreateTradeManager(Expedition expedition)
	{
		if (expedition.OwnedEntities.TradeManager == null)
		{
			expedition.OwnedEntities.TradeManager = new TradeManager(expedition.OwnedEntities);
		}
	}

	public void AdoptTierPolicy(TierType tier, RatingTypes rating)
	{
		Policy.AdoptTierPolicy(tier, rating);
		float policyMinimum = Policy.GetPolicyMinimum(rating);
		foreach (EntityID independentMember in IndependentMembers)
		{
			Entity entity = Entity.FindByID(independentMember);
			if (entity != null && entity.PersonEntity != null)
			{
				entity.PersonEntity.Personality.SetPrinciplesToMinimum(rating, policyMinimum);
			}
		}
	}

	public int NoOfIndependentMembersAllowedToSleep()
	{
		return Math.Max(val2: Policy.FractionIndependentsAllowedToSleep.HasValue ? ((int)((float)IndependentMembers.Count * Policy.FractionIndependentsAllowedToSleep.Value)) : ((!Policy.IndependentsAllowedToSleep.HasValue) ? IndependentMembers.Count : Policy.IndependentsAllowedToSleep.Value), val1: Common.ClampBottom(IndependentMembers.Count / 2, 1));
	}

	public int GetNumberOfSleepingIndependents()
	{
		int num = 0;
		for (int num2 = IndependentMembers.Count - 1; num2 >= 0; num2--)
		{
			EntityID member = IndependentMembers[num2];
			Entity entity = Entity.FindByID(member);
			if (entity != null)
			{
				if (entity.Intelligence.IsSleeping())
				{
					num++;
				}
			}
			else
			{
				RemoveMemberID(member);
			}
		}
		return num;
	}

	public void InitPlaySite()
	{
		JobManager = new JobManager(OwnedEntities);
		if (allegiance.RepresentativeEntityType.IntelligenceType.CanHaul == true)
		{
			ownerContent.HaulingJobManager = new HaulingJobManager(ownerContent);
		}
		ownerContent.OtherJobManager = new OtherJobManager(ownerContent);
		if (allegiance.RepresentativeEntityType.IntelligenceType.CanHunt == true)
		{
			ownerContent.HuntingJobManager = new HuntingJobManager(ownerContent);
		}
		UpdateOperatingAreas();
	}

	public void AddHousehold(Household household)
	{
		if (!Households.Contains(household))
		{
			Households.Add(household);
		}
	}

	public void RemoveHousehold(Household household)
	{
		Households.Remove(household);
		Residence.RemoveHousehold(allegiance.SharedKnowledge, household);
	}

	private void InitOtherSite()
	{
	}

	public void GetAvailableVehicles(RouteType? routeType, bool airRoute, double distance, ref Dictionary<EntityType, List<Entity>> vehicles, int? noOfVehicles = 1)
	{
		int num = 0;
		foreach (EntityID vehicle in OwnedEntities.Vehicles)
		{
			Entity entity = Entity.FindByID(vehicle);
			if (VehicleIsAvailable(entity) && ((VehicleContainerType)entity.EntityType.ContainerType).CanUseRoute(routeType, airRoute, distance))
			{
				Common.AddToMultiList(vehicles, entity.EntityType, entity);
				num++;
				if (noOfVehicles.HasValue && num == noOfVehicles)
				{
					break;
				}
			}
		}
	}

	public bool HasAavailableVehicle()
	{
		Dictionary<EntityType, List<Entity>> vehicles = null;
		GetAvailableVehicles(null, ref vehicles, 1);
		return vehicles != null;
	}

	public void GetAvailableVehicles(EntityType type, ref Dictionary<EntityType, List<Entity>> vehicles, int? noOfVehicles = 1)
	{
		int num = 0;
		foreach (EntityID vehicle in OwnedEntities.Vehicles)
		{
			Entity entity = Entity.FindByID(vehicle);
			if (VehicleIsAvailable(entity) && (type == null || entity.EntityType == type))
			{
				Common.AddToMultiList(ref vehicles, entity.EntityType, entity);
				num++;
				if (noOfVehicles.HasValue && num == noOfVehicles)
				{
					break;
				}
			}
		}
	}

	private bool VehicleIsAvailable(Entity vehicleEntity)
	{
		if (vehicleEntity != null && vehicleEntity.IsCompleted() && Entity.IsFunctional(vehicleEntity))
		{
			if (!vehicleEntity.AssignedToJob.HasValue)
			{
				return true;
			}
			if (LookUp<Job, JobID>.FindByID(vehicleEntity.AssignedToJob) == null)
			{
				vehicleEntity.AssignedToJob = null;
				return true;
			}
			return false;
		}
		return false;
	}

	public List<IKnownEntityData> GetWorkingTerminals(SharedKnowledge sharedKnowledge, TerminalType.TypesOfTerminal? typeOfTerminal)
	{
		List<IKnownEntityData> terminals = null;
		if (typeOfTerminal.HasValue)
		{
			if (OwnedEntities.Terminals.TryGetValue(typeOfTerminal.Value, out var value))
			{
				foreach (EntityID item in value)
				{
					GetWorkingTerminal(sharedKnowledge, ref terminals, item);
				}
			}
		}
		else
		{
			foreach (KeyValuePair<TerminalType.TypesOfTerminal, List<EntityID>> terminal in OwnedEntities.Terminals)
			{
				foreach (EntityID item2 in terminal.Value)
				{
					GetWorkingTerminal(sharedKnowledge, ref terminals, item2);
				}
			}
		}
		return terminals;
	}

	private static void GetWorkingTerminal(SharedKnowledge sharedKnowledge, ref List<IKnownEntityData> terminals, EntityID structure)
	{
		if (!GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(structure, out var data)) && data != null && data.IsCompleted() && Entity.IsFunctional(data))
		{
			Common.AddToList(ref terminals, data);
		}
	}

	public List<EntityID> GetMembersReadyToEmigrate()
	{
		List<EntityID> list = null;
		foreach (EntityID member in Members)
		{
			Entity entity = Entity.FindByID(member);
			if (entity != null && entity.Intelligence.HasDesireToEmigrate(The.InGameUI.UIAllegiance))
			{
				Common.AddToList(ref list, member);
			}
		}
		return list;
	}

	public void Destroy()
	{
		JobManager.Destroy();
		ownerContent.Destroy();
		RemoveIDEntry();
		((ILookUp<ICanIterateEntities, CanIterateEntitiesID>)this).RemoveIDEntry();
		((ILookUp<IHasEntityGroup, HasEntityGroupID>)this).RemoveIDEntry();
		((ILookUp<IOwner, OwnerID>)this).RemoveIDEntry();
		if (allegiance != null && allegiance.RepresentativeEntityType.PolledEvents != null && allegiance.RepresentativeEntityType.PolledEvents.TryGetValue(Scope.Expedition, out var value))
		{
			foreach (PolledEventType item in value)
			{
				allegiance.Site.EventManager.RemovePolledEvent(item, null, ID);
			}
		}
		if (Collidable != null)
		{
			Collidable.Delete();
		}
	}

	private void UpdateOperatingAreas()
	{
		bool isPlaySite = allegiance.Site.IsPlaySite;
		if (allegiance.AllegianceType == AllegianceType.Other)
		{
			return;
		}
		for (int i = 0; i < The.Map.mapTileWidth; i++)
		{
			TerrainTile[] array = The.Map.TileMap[i];
			for (int j = 0; j < The.Map.mapTileHeight; j++)
			{
				TerrainTile terrainTile = array[j];
				if (isPlaySite)
				{
					terrainTile.OperatingAreaOf = this;
				}
				else if (terrainTile.OperatingAreaOf == this)
				{
					terrainTile.OperatingAreaOf = null;
				}
			}
		}
	}

	public void MergeExpeditions(Expedition expeditionToDissappear)
	{
	}

	public void AddMember(Entity member)
	{
		if (member.EntityType.IntelligenceType != null)
		{
			member.Intelligence.CurrentExpedition = this;
			if (member.Intelligence.IsIndependent())
			{
				IndependentMembers.Add(member.ID);
			}
			if (member.EntityType.IntelligenceType.CanDoJobs == true)
			{
				Workers.Add(member.ID);
			}
		}
		Members.Add(member.EntityID);
		UpdateWhenMembersChange();
	}

	private void UpdateWhenMembersChange()
	{
		UpdateSkills();
		Allegiance.HandleGroupMembersChanged(foodExtraction);
		Allegiance.HandleGroupMembersChanged(independentMemberFoodExtraction);
		OwnedEntities.RecomputeFoodConsumeRate();
	}

	public void RemoveMember(Entity member)
	{
		if (member.EntityType.IntelligenceType != null && member.Intelligence.CurrentExpedition == this)
		{
			member.Intelligence.CurrentExpedition = null;
		}
		RemoveMemberID(member.EntityID);
		UpdateWhenMembersChange();
	}

	public void RemoveMemberID(EntityID member)
	{
		IndependentMembers.Remove(member);
		Members.Remove(member);
		Workers.Remove(member);
		UpdateSkills();
		Allegiance.HandleGroupMembersChanged(foodExtraction);
		Allegiance.HandleGroupMembersChanged(independentMemberFoodExtraction);
	}

	public bool HasUniqueSkill(Entity agent)
	{
		if (Members.Count == 1)
		{
			return false;
		}
		return membersWithUniqueSkill.Contains(agent.ID);
	}

	public bool SkillIsUnique(SkillType skillType)
	{
		if (skillType == null)
		{
			return false;
		}
		if (membersWithSkill.TryGetValue(skillType, out var value) && value == 1)
		{
			return true;
		}
		return false;
	}

	public bool HasSkill(SkillType skillType)
	{
		if (skillType == null)
		{
			return true;
		}
		if (membersWithSkill.TryGetValue(skillType, out var value) && value > 0)
		{
			return true;
		}
		return false;
	}

	private void UpdateSkills()
	{
		foreach (SkillType item in membersWithSkill.Keys.ToList())
		{
			membersWithSkill[item] = 0;
		}
		IterateMembers(delegate(Entity e)
		{
			AddSkills(e);
		});
		membersWithUniqueSkill.Clear();
		IterateMembers(delegate(Entity e)
		{
			AddToMembersWithUniqueSkills(e);
		});
	}

	private void AddSkills(Entity entity)
	{
		foreach (KeyValuePair<SkillType, Skill> skill in entity.Intelligence.Skills)
		{
			if (skill.Value.Value > GameData.Instance.Constants.MinimumSkillValueToUse)
			{
				Common.AddToDictWithSums(membersWithSkill, skill.Key);
			}
		}
	}

	private void AddToMembersWithUniqueSkills(Entity entity)
	{
		foreach (KeyValuePair<SkillType, Skill> skill in entity.Intelligence.Skills)
		{
			if (skill.Value.Value > GameData.Instance.Constants.MinimumSkillValueToUse && SkillIsUnique(skill.Key))
			{
				membersWithUniqueSkill.Add(entity.ID);
				break;
			}
		}
	}

	public void Update(GameTime gameTime)
	{
		if (JobManager != null)
		{
			JobManager.Update(gameTime);
		}
		if (physicalNeedsPlanner != null)
		{
			physicalNeedsPlanner.Update(gameTime);
		}
		if (Statistics != null)
		{
			Statistics.Update(gameTime);
		}
		if (skillRegulator.IsReady())
		{
			UpdateSkills();
		}
		if (Population != null)
		{
			Population.Update(gameTime);
		}
		UpdateReplenishAvailableStates();
	}

	private void UpdateReplenishAvailableStates()
	{
	}

	private bool GiveFoodToHousehold(EntityType foodType, Household household)
	{
		List<EntityID> items = ownerContent.Items[foodType];
		return GiveFreeItemInList(household, items);
	}

	private static bool GiveFreeItemInList(Household household, List<EntityID> items)
	{
		return false;
	}

	private void CreateCollidable()
	{
		Vector2? position = null;
		if (Location.HasValue)
		{
			position = Location.Value.ToVector2();
			float value = allegiance.GetForageAndHuntingRadius();
			Collidable = new Collidable<Expedition>(this, position, new Vector2(value));
			Collidable.BeCircle();
		}
	}

	public static Expedition FindByID(ExpeditionID id)
	{
		return LookUp<Expedition, ExpeditionID>.FindByID(id);
	}

	public void GetChildren(string key, ref List<IHasExposedProperties> listOfChildren, FilterCondition filter, EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget, SharedKnowledge getterKnowledge = null)
	{
		bool wasFiltered = false;
		if (key == "OwnedEntities")
		{
			ownerContent.GetEntities(filter, ref listOfChildren, out wasFiltered, triggeringEntity, targetEntity, polledEventSource);
		}
		if (!wasFiltered && filter != null)
		{
			Site.FilterChildren(listOfChildren, filter, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
		}
	}

	public string GetCaption(string captionKey)
	{
		return null;
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
		return Name;
	}

	public void GetDefaultKey(out string PropertyKey)
	{
		PropertyKey = null;
	}

	public EntityID? GetEntityID()
	{
		return null;
	}

	public bool GetIsSeenDirectly()
	{
		return true;
	}

	public void SetPropertyValue(string propertyKey, PropertyResult? value)
	{
		Entity.SetPropertyValue(ref customFields, propertyKey, value);
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		IDCounter = sn.DoEnum(IDCounter);
		id = SnapshotID(sn, id);
		hasEntityGroupID = sn.DoEnum(hasEntityGroupID);
		canIterateEntitiesID = sn.DoEnum(canIterateEntitiesID);
		ownerID = sn.DoEnum(ownerID);
		customFields = sn.DoDictionary(customFields);
		snapshotAllegiance = sn.SnapshotID<Allegiance, AllegianceID>(allegiance).Value;
		Center = sn.DoVector3Nullable(Center);
		snapshotGatheringSite = sn.SnapshotID<GatheringSite, GatheringSiteID>(gatheringSite);
		foodExtraction = (FoodExtraction)sn.DoISnapshot(foodExtraction);
		independentMemberFoodExtraction = (FoodExtraction)sn.DoISnapshot(independentMemberFoodExtraction);
		if (sn.mode != Snapshotter.Mode.Load)
		{
			snapshotHouseholds = Households.Select((Household h) => h.ID).ToList();
		}
		snapshotHouseholds = sn.DoList(snapshotHouseholds);
		snapshotOwnerContent = sn.SnapshotID<EntityGroup, EntityGroupID>(ownerContent).Value;
		snapshotJobManager = sn.SnapshotID<ICyclable, CyclableID>(JobManager);
		Members = sn.DoList(Members);
		IndependentMembers = sn.DoList(IndependentMembers);
		KeyName = sn.DoString(KeyName);
		Name = sn.DoString(Name);
		physicalNeedsPlanner = (PhysicalNeedsPlanner)sn.DoISnapshot(physicalNeedsPlanner);
		Policy = (ExpeditionPolicy)sn.DoISnapshot(Policy);
		membersWithSkill = sn.DoDictionary(membersWithSkill);
		membersWithUniqueSkill = sn.DoHashSet(membersWithUniqueSkill);
		Workers = sn.DoList(Workers);
		Population = (Population)sn.DoISnapshot(Population);
		sn.Postpone(Statistics);
		sn.Ignore(JobManager);
		sn.Ignore(Households);
		sn.Ignore(exposedPropertyValueFunctions);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		allegiance = LookUp<Allegiance, AllegianceID>.FindByID(snapshotAllegiance);
		gatheringSite = LookUp<GatheringSite, GatheringSiteID>.FindByID(snapshotGatheringSite);
		Households = snapshotHouseholds.Select((HouseholdID h) => LookUp<Household, HouseholdID>.FindByID(h)).ToList();
		Households.RemoveAll((Household h) => h == null);
		ownerContent = LookUp<EntityGroup, EntityGroupID>.FindByID(snapshotOwnerContent);
		JobManager = (JobManager)LookUp<ICyclable, CyclableID>.FindByID(snapshotJobManager);
		if (physicalNeedsPlanner != null)
		{
			physicalNeedsPlanner.LoadPostProcess(sn);
		}
		if (foodExtraction != null)
		{
			foodExtraction.LoadPostProcess(sn);
		}
		if (independentMemberFoodExtraction != null)
		{
			independentMemberFoodExtraction.LoadPostProcess(sn);
		}
		if (Policy != null)
		{
			Policy.LoadPostProcess(sn);
		}
		if (Population != null)
		{
			Population.LoadPostProcess(sn);
			Population.Expedition = this;
		}
		CreateRegulators();
		CreateCollidable();
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ExpeditionID GetUniqueID()
	{
		IDCounter++;
		if (IDCounter >= ExpeditionID.Invalid)
		{
			throw new Exception("Astounding, ExpeditionID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public ExpeditionID SnapshotID(Snapshotter sn, ExpeditionID id)
	{
		return sn.DoEnum(id);
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != ExpeditionID.Invalid)
		{
			LookUp<Expedition, ExpeditionID>.Add(ID, this);
		}
	}

	public void SetInvalid()
	{
		id = ExpeditionID.Invalid;
	}

	public void RemoveIDEntry()
	{
		LookUp<Expedition, ExpeditionID>.Remove(this);
	}

	void ILookUp<Expedition, ExpeditionID>.ResetIDCounter()
	{
	}

	public static void ResetIDCounter()
	{
		IDCounter = ExpeditionID.First;
	}

	void ILookUp<Expedition, ExpeditionID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUp<Expedition, ExpeditionID>.Create();
	}

	CanIterateEntitiesID ILookUp<ICanIterateEntities, CanIterateEntitiesID>.GetUniqueID()
	{
		return HasMembers.GetUniqueID();
	}

	void ILookUp<ICanIterateEntities, CanIterateEntitiesID>.AddToLookup()
	{
		canIterateEntitiesID = ((ILookUp<ICanIterateEntities, CanIterateEntitiesID>)this).GetUniqueID();
		if (canIterateEntitiesID != CanIterateEntitiesID.Invalid)
		{
			LookUpICanIterateEntities.Add(canIterateEntitiesID, this);
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
		canIterateEntitiesID = CanIterateEntitiesID.Invalid;
	}

	void ILookUp<ICanIterateEntities, CanIterateEntitiesID>.CreateLookupCollection()
	{
	}

	HasEntityGroupID ILookUp<IHasEntityGroup, HasEntityGroupID>.GetUniqueID()
	{
		return HasEntityGroup.GetUniqueID();
	}

	void ILookUp<IHasEntityGroup, HasEntityGroupID>.AddToLookup()
	{
		hasEntityGroupID = ((ILookUp<IHasEntityGroup, HasEntityGroupID>)this).GetUniqueID();
		if (hasEntityGroupID != HasEntityGroupID.Invalid)
		{
			LookUpHasEntityGroup.Add(hasEntityGroupID, this);
		}
	}

	void ILookUp<IHasEntityGroup, HasEntityGroupID>.RemoveIDEntry()
	{
		LookUpHasEntityGroup.Remove(this);
	}

	void ILookUp<IHasEntityGroup, HasEntityGroupID>.ResetIDCounter()
	{
	}

	void ILookUp<IHasEntityGroup, HasEntityGroupID>.SetInvalid()
	{
		hasEntityGroupID = HasEntityGroupID.Invalid;
	}

	void ILookUp<IHasEntityGroup, HasEntityGroupID>.CreateLookupCollection()
	{
	}

	OwnerID ILookUp<IOwner, OwnerID>.GetUniqueID()
	{
		return Owner.GetUniqueID();
	}

	void ILookUp<IOwner, OwnerID>.AddToLookup()
	{
		ownerID = ((ILookUp<IOwner, OwnerID>)this).GetUniqueID();
		if (ownerID != OwnerID.Invalid)
		{
			LookUpOwners.Add(ownerID, this);
		}
	}

	void ILookUp<IOwner, OwnerID>.RemoveIDEntry()
	{
		LookUpOwners.Remove(this);
	}

	void ILookUp<IOwner, OwnerID>.ResetIDCounter()
	{
	}

	void ILookUp<IOwner, OwnerID>.SetInvalid()
	{
		ownerID = OwnerID.Invalid;
	}

	void ILookUp<IOwner, OwnerID>.CreateLookupCollection()
	{
	}
}
