using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Communication;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.InGameEvents;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.Items;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Overland.Locations;
using UWGame.SimSide.Overland.Missions;
using UWGame.SimSide.Policies;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Scenarios;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems.TimeSlicing;
using UWGame.SimSide.Tiers;
using UWGame.Steam;

namespace UWGame.SimSide.Allegiances;

[DebuggerDisplay("{KeyName}")]
public class Allegiance : IHasExposedProperties, ICanIterateEntities, ILookUp<ICanIterateEntities, CanIterateEntitiesID>, ILookUp<Allegiance, AllegianceID>, ISnapshot, IHasEntityGroup, ILookUp<IHasEntityGroup, HasEntityGroupID>, ICommunicates
{
	public Color DebugColor;

	private Site site;

	private static Dictionary<string, GetPropertyValue> exposedPropertyValueFunctions;

	private Dictionary<string, PropertyResult> customFields;

	private SiteID snapshotSiteID;

	public AllegianceType AllegianceType;

	public HashSet<Entity> Members = new HashSet<Entity>();

	public List<Entity> MembersList = new List<Entity>();

	private List<EntityID> snapshotMembers = new List<EntityID>();

	public List<EntityID> IndependentMembers = new List<EntityID>();

	public List<Entity> Persons = new List<Entity>();

	public GroupStatistics Statistics;

	public bool PermitsImmigration;

	public List<Expedition> Expeditions = new List<Expedition>();

	private List<ExpeditionID> snapshotExpeditions = new List<ExpeditionID>();

	public AllegiancePolicy Policy = new AllegiancePolicy();

	public HashSet<AllegianceID> AllegiancesWeAreInContactWith = new HashSet<AllegianceID>();

	public List<Mission> Missions;

	private List<MissionID> snapshotMissions;

	private decimal? tradeCredits = default(decimal);

	public OtherSiteAllegianceManager OtherSiteAllegianceManager;

	public SharedKnowledge SharedKnowledge;

	public EntityType RepresentativeEntityType;

	public FoodExtraction FoodExtraction;

	public ThreatJobManager ThreatAndCombatJobManager;

	private CyclableID? snapshotThreatManager;

	public HumanActivities HumanActivities;

	public ThreatGroup ThreatGroup;

	private ThreatGroupID? snapshotThreatGroupID;

	private int? forageAndHuntingRadius;

	private AllegianceID id = AllegianceID.Invalid;

	private static AllegianceID IDCounter;

	private CanIterateEntitiesID canIterateEntitiesID;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	private HasEntityGroupID hasEntityGroupID;

	public string Name { get; private set; }

	public string KeyName { get; private set; }

	public Site Site
	{
		get
		{
			return site;
		}
		set
		{
			site = value;
		}
	}

	public GeodeticCoordinate? Coords
	{
		get
		{
			if (Site != null)
			{
				return Site.Coords;
			}
			return null;
		}
	}

	public decimal? TradeCredits
	{
		get
		{
			return tradeCredits;
		}
		set
		{
			tradeCredits = value;
		}
	}

	public Allegiance GetAllegiance => this;

	Allegiance IHasEntityGroup.Allegiance => this;

	public Vector3? Location => Vector3.Zero;

	public int NoOfWorkers => Members.Count;

	public AllegianceID ID
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

	public bool IsSnapshotted { get; set; }

	HasEntityGroupID ILookUp<IHasEntityGroup, HasEntityGroupID>.ID => hasEntityGroupID;

	public Allegiance()
	{
	}

	public Allegiance(AllegianceType allegianceType, EntityType representativeEntityType, string key = null, string name = null, bool computeAuxiliaryMaps = true, Site site = null)
	{
		AddToLookup();
		((ILookUp<ICanIterateEntities, CanIterateEntitiesID>)this).AddToLookup();
		((ILookUp<IHasEntityGroup, HasEntityGroupID>)this).AddToLookup();
		Site = site ?? The.Sim.PlaySite;
		if (key == null)
		{
			KeyName = representativeEntityType.KeyName + "Allegiance#" + ID;
		}
		else
		{
			KeyName = key;
		}
		Name = name;
		AllegianceType = allegianceType;
		RepresentativeEntityType = representativeEntityType;
		Site.Allegiances.Add(this);
		if (AllegianceType == AllegianceType.Player)
		{
			The.InGameUI.UIAllegiance = this;
		}
		ulong num = (ulong)(id + 1);
		DebugColor = new Color((byte)(num * 100 % 255), (byte)(num * 23 % 255), (byte)(num * 7 % 255));
		HumanActivities = new HumanActivities();
		SharedKnowledge = new SharedKnowledge(this);
		if (Site.IsPlaySite)
		{
			InitPlaySite(The.Sim.IsInNormalGameLoop);
		}
		OtherSiteAllegianceManager = new OtherSiteAllegianceManager(this);
		GetFirstExpedition();
		Statistics = new GroupStatistics(this, representativeEntityType);
		if (RepresentativeEntityType.PolledEvents == null || !RepresentativeEntityType.PolledEvents.TryGetValue(Scope.Allegiance, out var value))
		{
			return;
		}
		foreach (PolledEventType item in value)
		{
			if (Site.IsPlaySite || !item.PlaySiteOnly)
			{
				Site.EventManager.AddPolledEvent(item.KeyName, null, null, ID);
			}
		}
	}

	static Allegiance()
	{
		exposedPropertyValueFunctions = new Dictionary<string, GetPropertyValue>();
		IDCounter = AllegianceID.First;
		exposedPropertyValueFunctions.Add("keyName", GetKeyName);
		exposedPropertyValueFunctions.Add("name", GetName);
		exposedPropertyValueFunctions.Add("comfortRating", GetComfortRating);
		exposedPropertyValueFunctions.Add("foodRating", GetFoodRating);
		exposedPropertyValueFunctions.Add("securityRating", GetSecurityRating);
	}

	public int? GetMaxPopulationMembers()
	{
		int? num = null;
		if (Expeditions != null)
		{
			foreach (Expedition expedition in Expeditions)
			{
				if (expedition.Population != null)
				{
					if (!num.HasValue)
					{
						num = 0;
					}
					num += expedition.Population.MaxMembers;
				}
			}
		}
		return num;
	}

	public void UpdatePlaySite(GameTime gameTime)
	{
		if (AllegianceType == AllegianceType.Player && Statistics != null)
		{
			Statistics.Update(gameTime);
			Statistics.CheckAchievements();
		}
		ThreatAndCombatJobManager.Update(gameTime);
		if (HumanActivities != null)
		{
			HumanActivities.Update(gameTime);
		}
		SharedKnowledge.Update(gameTime);
	}

	private void FoodExtraction_FoodProcessesChanged()
	{
		SharedKnowledge.SetFoodDirty();
	}

	public void UpdateOffPlaySite(GameTime gameTime)
	{
		OtherSiteAllegianceManager.Update(gameTime);
	}

	public void GainContact(Allegiance otherAllegiance)
	{
		bool? canTradeAndCommunicate = otherAllegiance.RepresentativeEntityType.IntelligenceType.CanTradeAndCommunicate;
		bool flag = true;
		if (canTradeAndCommunicate == true == flag && canTradeAndCommunicate.HasValue && !AllegiancesWeAreInContactWith.Contains(otherAllegiance.ID))
		{
			AllegiancesWeAreInContactWith.Add(otherAllegiance.ID);
			LetOtherAllegianceGetKnowledgeAboutTerminals(otherAllegiance);
			LetOtherAllegianceGetKnowledgeAboutMembers(otherAllegiance);
		}
	}

	private void LetOtherAllegianceGetKnowledgeAboutMembers(Allegiance otherAllegiance)
	{
		foreach (Entity member in Members)
		{
			otherAllegiance.SharedKnowledge.SeeDetectableIfRelevant(member);
		}
	}

	public void LogProductionStatistics(IKnownEntityData entityData, IKnownProcess processData)
	{
		Statistics.AddProductionEvent(entityData.EntityType, ProductionStatistics.StatTypes.Produced, 1);
		if (processData != null)
		{
			Statistics.AddProductivityEvent(entityData.EntityType, processData);
		}
		if (!IsEatable(entityData.EntityType) || entityData.EntityType.ItemType == null || entityData.EntityType.ItemType.FoodType == null)
		{
			return;
		}
		foreach (KeyValuePair<FoodNutrientType, float> nutrientBulkAmount in entityData.NutrientBulkAmounts)
		{
			Statistics.AddNutrientEvent(nutrientBulkAmount.Key, NutrientStatistics.StatTypes.Produced, nutrientBulkAmount.Value);
		}
	}

	private void LetOtherAllegianceGetKnowledgeAboutTerminals(Allegiance otherAllegiance)
	{
		foreach (Expedition expedition in Expeditions)
		{
			foreach (KeyValuePair<TerminalType.TypesOfTerminal, List<EntityID>> terminal in expedition.OwnedEntities.Terminals)
			{
				foreach (EntityID item in terminal.Value)
				{
					if (!GoalEvaluator.EntityDataResultCausesSkip(SharedKnowledge.GetKnownData(item, out var data)) && data is Entity detectable)
					{
						otherAllegiance.SharedKnowledge.SeeDetectableIfRelevant(detectable, testForUsesMemory: true, suppressClientFeedback: false, null, null, doAssert: false);
					}
				}
			}
		}
	}

	public void LetOtherAllegiancesSeeEntity(Entity entity, bool seeEntity)
	{
		List<AllegianceID> list = null;
		foreach (AllegianceID item in AllegiancesWeAreInContactWith)
		{
			Allegiance allegiance = LookUp<Allegiance, AllegianceID>.FindByID(item);
			if (allegiance != null)
			{
				if (seeEntity)
				{
					allegiance.SharedKnowledge.SeeDetectableIfRelevant(entity);
				}
				else
				{
					allegiance.SharedKnowledge.DeleteMemoryOfEntity(entity.ID, entity.DetectableID, removeAllKnowledge: true);
				}
			}
			else
			{
				Common.AddToList(ref list, item);
			}
		}
		if (list == null)
		{
			return;
		}
		foreach (AllegianceID item2 in list)
		{
			AllegiancesWeAreInContactWith.Remove(item2);
		}
	}

	public void LoseContact(Allegiance otherAllegiance)
	{
		AllegiancesWeAreInContactWith.Remove(otherAllegiance.ID);
		foreach (Expedition expedition in Expeditions)
		{
			foreach (KeyValuePair<TerminalType.TypesOfTerminal, List<EntityID>> terminal in expedition.OwnedEntities.Terminals)
			{
				foreach (EntityID item in terminal.Value)
				{
					Entity entity = Entity.FindByID(item);
					if (entity != null)
					{
						otherAllegiance.SharedKnowledge.UnSeeEntity(entity);
					}
				}
			}
		}
		foreach (Entity member in Members)
		{
			otherAllegiance.SharedKnowledge.UnSeeEntity(member);
		}
	}

	public void Update(GameTime gameTime)
	{
		foreach (Expedition expedition in Expeditions)
		{
			expedition.Update(gameTime);
		}
		if (Missions != null)
		{
			for (int num = Missions.Count - 1; num >= 0; num--)
			{
				Missions[num].Update(gameTime);
			}
		}
		if (Site.IsPlaySite)
		{
			UpdatePlaySite(gameTime);
		}
		else
		{
			UpdateOffPlaySite(gameTime);
		}
	}

	public int GetForageAndHuntingRadius()
	{
		if (forageAndHuntingRadius.HasValue)
		{
			return forageAndHuntingRadius.Value;
		}
		return RepresentativeEntityType.IntelligenceType.ForageAndHuntingRadius;
	}

	public void AddMission(Mission mission)
	{
		Common.AddToList(ref Missions, mission);
	}

	public void RemoveMission(Mission mission)
	{
		Missions.Remove(mission);
	}

	public void InitPlaySite(bool computeAuxiliaryMaps)
	{
		if (HumanActivities == null)
		{
			HumanActivities = new HumanActivities();
		}
		FoodExtraction = new FoodExtraction(this, SharedKnowledge.AllKnownEntities.ID);
		if (computeAuxiliaryMaps && The.Map != null && The.Map.mapTileWidth > 0)
		{
			SharedKnowledge.PlaySiteKnowledge.InitAuxiliaryMaps();
		}
		ThreatAndCombatJobManager = new ThreatJobManager(this);
		ThreatGroup = ThreatGroup.GetThreatGroup(Name);
	}

	public static Allegiance CreateFromAllegianceData(AllegianceData allegianceData, Site site = null, float? sizeFactor = null, string allegianceKeyName = null, string expeditionKeyName = null)
	{
		EntityType representativeEntityType = GameData.Instance.AllEntityTypes[allegianceData.EntityType];
		if (site == null)
		{
			site = The.Sim.World.AllSites[allegianceData.Site];
		}
		Allegiance allegiance = new Allegiance(allegianceData.AllegianceType, representativeEntityType, allegianceKeyName ?? allegianceData.KeyName, allegianceData.Name ?? site.Name, computeAuxiliaryMaps: true, site);
		allegiance.forageAndHuntingRadius = allegianceData.ForageAndHuntingRadius;
		allegiance.PermitsImmigration = allegianceData.PermitsImmigration;
		if (allegiance.AllegianceType != AllegianceType.Player)
		{
			allegiance.Statistics = GroupStatistics.CreateFromStatsData(allegianceData.StatsData, allegiance, representativeEntityType);
		}
		if (allegiance.AllegianceType == AllegianceType.Player && site.IsPlaySite)
		{
			The.Sim.PlaySite.PlayerAllegiance = allegiance;
		}
		if (allegianceData.AllegianceTemplates != null)
		{
			int stairstep;
			StringChance stairStepIndex = Common.GetStairStepIndex(allegianceData.AllegianceTemplates, out stairstep, The.Sim.GameplayRandomGenerator);
			GameData.Instance.AllAllegianceTemplates[stairStepIndex.String].FillAllegiance(allegiance, sizeFactor, expeditionKeyName);
		}
		return allegiance;
	}

	public void AddMember(Entity newMember)
	{
		Members.Add(newMember);
		MembersList.Add(newMember);
		if (Site.IsPlaySite)
		{
			if (SharedKnowledge.PlaySiteKnowledge != null)
			{
				Vector2 location = (newMember.Location ?? Vector3.Zero).ToVector2();
				SharedKnowledge.PlaySiteKnowledge.AddOrUpdateKnownEntityLocation(newMember, location);
			}
			SharedKnowledge.AddMember(newMember);
		}
		if (newMember.Intelligence != null && newMember.Intelligence.IsIndependent())
		{
			IndependentMembers.Add(newMember.ID);
		}
		if (newMember.EntityType.Person != null)
		{
			Persons.Add(newMember);
			CheckAchievements();
		}
		if (Site.IsPlaySite)
		{
			HandleGroupMembersChanged(FoodExtraction);
		}
		NotifyPopulationStatistics();
		LetOtherAllegiancesSeeEntity(newMember, seeEntity: true);
	}

	private void CheckAchievements()
	{
		if (AllegianceType != AllegianceType.Player || The.Sim.StartGameParams.GetRGScenario() != StartGameParams.RGScenario.FieldsOfTauCeti)
		{
			return;
		}
		StatsAndAchievements statsAndAchievements = The.Sim.Controller.StatsAndAchievements;
		if (statsAndAchievements.IsAchievementUnlocked(AchievementID.relatives))
		{
			return;
		}
		foreach (IGrouping<string, Entity> item in from e in Persons
			group e by e.Intelligence.LastName)
		{
			if (item.Count() >= 3)
			{
				statsAndAchievements.UnlockAchievement(AchievementID.relatives);
				break;
			}
		}
	}

	private void NotifyPopulationStatistics()
	{
		if (RepresentativeEntityType.Person != null)
		{
			Statistics.NotifyPopulationChanged(Persons.Count);
		}
		else
		{
			Statistics.NotifyPopulationChanged(MembersList.Count);
		}
	}

	public static void HandleGroupMembersChanged(FoodExtraction foodExtraction)
	{
		foodExtraction.SetIsDirty();
	}

	public void RemoveMember(Entity memberToRemove, bool isDestroyed)
	{
		Members.Remove(memberToRemove);
		MembersList.Remove(memberToRemove);
		if (SharedKnowledge != null && SharedKnowledge.PlaySiteKnowledge != null)
		{
			SharedKnowledge.PlaySiteKnowledge.KnownEntityDataTree.RemoveObject(memberToRemove.EntityID);
		}
		if (memberToRemove.EntityType.Person != null)
		{
			Persons.Remove(memberToRemove);
		}
		if (memberToRemove.EntityType.IntelligenceType != null)
		{
			IndependentMembers.Remove(memberToRemove.ID);
		}
		if (Site.IsPlaySite)
		{
			HandleGroupMembersChanged(FoodExtraction);
			SharedKnowledge.RemoveMember(memberToRemove);
		}
		NotifyPopulationStatistics();
		if (memberToRemove.Intelligence.IsIndependent())
		{
			Statistics.RecordDeathOrEmigration(isDestroyed);
		}
		if (!isDestroyed)
		{
			LetOtherAllegiancesSeeEntity(memberToRemove, seeEntity: false);
		}
	}

	public void GetMembersInTierRange(RatingTypes rating, TierType tier, AgentCondition agentCanVote, out List<Entity> membersAboveRange, out List<Entity> membersBelowRange, out List<Entity> membersUnqualified, out TierType previousTier)
	{
		membersAboveRange = new List<Entity>();
		membersBelowRange = new List<Entity>();
		membersUnqualified = new List<Entity>();
		_ = tier.UpperEdge;
		TierType.GetTierBelow(Array.FindIndex(GameData.Instance.Tiers, (TierType t) => t == tier), out previousTier, out var lowerTierEdge);
		if (previousTier == null)
		{
			lowerTierEdge = -1f;
		}
		foreach (EntityID independentMember in IndependentMembers)
		{
			Entity entity = Entity.FindByID(independentMember);
			if (entity.PersonEntity == null)
			{
				continue;
			}
			if (agentCanVote.IsFulfilled(entity))
			{
				if (entity.PersonEntity.Personality.Principles[rating] > lowerTierEdge)
				{
					Common.AddToList(ref membersAboveRange, entity);
				}
				else
				{
					Common.AddToList(ref membersBelowRange, entity);
				}
			}
			else
			{
				Common.AddToList(ref membersUnqualified, entity);
			}
		}
	}

	public DateAndTime.TimeDateYear GetTimeToReachMajority(RatingTypes rating, TierType tier, int neededVotes, List<Entity> membersBelowRange)
	{
		float tierEdgeBelow = tier.GetTierEdgeBelow();
		List<Tuple<Entity, float>> list = new List<Tuple<Entity, float>>();
		foreach (Entity item in membersBelowRange)
		{
			float timeForPrincipleToReachValue = item.PersonEntity.Personality.GetTimeForPrincipleToReachValue(rating, tierEdgeBelow);
			list.Add(new Tuple<Entity, float>(item, timeForPrincipleToReachValue));
		}
		return DateAndTime.GetSecondsToIngameDays(list.OrderBy((Tuple<Entity, float> t) => t.Item2).ToList()[neededVotes - 1].Item2);
	}

	public int GetNoOfPersons(Predicate<Entity> filter)
	{
		if (filter == null)
		{
			return Persons.Count((Entity e) => e.Site == Site);
		}
		return Persons.Count((Entity e) => filter(e));
	}

	public List<Entity> GetPersons(Predicate<Entity> filter)
	{
		return Persons.Where((Entity e) => filter(e)).ToList();
	}

	public Entity GetRandomPerson(Predicate<Entity> filter)
	{
		List<Entity> persons = GetPersons(filter);
		if (persons != null && persons.Count > 0)
		{
			return Common.GetRandomListMember(persons, The.Sim.GameplayRandomGenerator);
		}
		return null;
	}

	public int GetNoOfPersons()
	{
		return Persons.Count;
	}

	public bool IsOverPopulationCap()
	{
		return IsOverPopulationCap(GetNoOfPersons());
	}

	private bool IsOverPopulationCap(int members)
	{
		return members > GameData.Instance.Constants.PopulationCap;
	}

	public bool IsWithinPopulationCap(int additionalMembers)
	{
		return !IsOverPopulationCap(GetNoOfPersons() + additionalMembers);
	}

	public bool WasRecentlyAttackedBy(Entity potentialAttacker)
	{
		EntityID? currentTarget = potentialAttacker.Intelligence.CombatInfo.Target;
		if (currentTarget.HasValue && Members.Any((Entity e) => e.EntityID == currentTarget))
		{
			return true;
		}
		return Members.Any((Entity e) => e.Intelligence.Memory.WasRecentlyHitBy(potentialAttacker.EntityID));
	}

	public float? GetMaximumAggroRange()
	{
		return Members.Max((Entity e) => e.GetAggroRange());
	}

	public bool IsOwnedByAllegiance(IKnownEntityData e)
	{
		if (e.OwnedBy.HasValue && LookUpOwners.ResolveEntityOwner(e, out IOwner owner) && owner != null && owner.Allegiance == this)
		{
			return true;
		}
		return false;
	}

	public bool CanCommunicate(CommunicationMethod method, double distance)
	{
		if (RepresentativeEntityType.CommunicatorType != null && RepresentativeEntityType.CommunicatorType.Method == method && RepresentativeEntityType.CommunicatorType.IsInRange(distance))
		{
			return true;
		}
		foreach (Expedition expedition in Expeditions)
		{
			foreach (KeyValuePair<EntityType, List<EntityID>> communicator in expedition.OwnedEntities.Communicators)
			{
				foreach (EntityID item in communicator.Value)
				{
					Entity entity = Entity.FindByID(item);
					if (entity != null && entity.EntityType.CommunicatorType.Method == method)
					{
						entity.Find<Communicator>(out var c);
						if (c.IsCommunicatorWorkingAndInRange(distance))
						{
							return true;
						}
					}
				}
			}
		}
		return false;
	}

	public bool IsUnderThreat()
	{
		return SharedKnowledge.AllKnownEntities.ThreatJobs.Count > 0;
	}

	public bool MembersHaveAIDisabled()
	{
		foreach (Entity member in Members)
		{
			if (!member.Intelligence.DisableAI)
			{
				return false;
			}
		}
		return true;
	}

	public void Destroy()
	{
		RemoveIDEntry();
		((ILookUp<ICanIterateEntities, CanIterateEntitiesID>)this).RemoveIDEntry();
		((ILookUp<IHasEntityGroup, HasEntityGroupID>)this).RemoveIDEntry();
		Site.Allegiances.Remove(this);
		if (ThreatAndCombatJobManager != null)
		{
			ThreatAndCombatJobManager.Destroy();
		}
		if (SharedKnowledge != null)
		{
			SharedKnowledge.Destroy();
		}
		foreach (Expedition expedition in Expeditions)
		{
			expedition.Destroy();
		}
		The.Sim.World.RemoveRelation(ID);
		if (RepresentativeEntityType.PolledEvents == null || !RepresentativeEntityType.PolledEvents.TryGetValue(Scope.Allegiance, out var value))
		{
			return;
		}
		foreach (PolledEventType item in value)
		{
			Site.EventManager.RemovePolledEvent(item, null, null, ID);
		}
	}

	public static void GetMembers(IHasExposedProperties presentedObject, List<IHasExposedProperties> listToFillWithProperties)
	{
		Allegiance allegiance = (Allegiance)presentedObject;
		listToFillWithProperties.AddRange(allegiance.Members);
	}

	public static void GetPersons(IHasExposedProperties presentedObject, List<IHasExposedProperties> listToFillWithProperties)
	{
		Allegiance allegiance = (Allegiance)presentedObject;
		listToFillWithProperties.AddRange(allegiance.Persons);
	}

	public static void GetRandomPersons(IHasExposedProperties presentedObject, ref List<IHasExposedProperties> listToFillWithProperties)
	{
		GetPersons(presentedObject, listToFillWithProperties);
		listToFillWithProperties = Common.Randomize(listToFillWithProperties, The.Sim.GameplayRandomGenerator);
	}

	public void GetChildren(string keyToList, ref List<IHasExposedProperties> listToFillWithProperties, FilterCondition filter, EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget, SharedKnowledge getterKnowledge = null)
	{
		switch (keyToList)
		{
		case "members":
			GetMembers(this, listToFillWithProperties);
			break;
		case "persons":
			GetPersons(this, listToFillWithProperties);
			break;
		case "randomPersons":
			GetRandomPersons(this, ref listToFillWithProperties);
			break;
		}
		if (filter != null)
		{
			Site.FilterChildren(listToFillWithProperties, filter, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
		}
	}

	public PropertyResult? GetPropertyValue(string propertyKey, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
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

	public static PropertyResult? GetKeyName(IHasExposedProperties anObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return new PropertyResult
		{
			StringResult = ((Allegiance)anObjectToGetValueFrom).KeyName
		};
	}

	public static PropertyResult? GetName(IHasExposedProperties anObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return new PropertyResult
		{
			StringResult = ((Allegiance)anObjectToGetValueFrom).Name
		};
	}

	public static PropertyResult? GetComfortRating(IHasExposedProperties anObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Allegiance)anObjectToGetValueFrom).GetRating(getterKnowledge, RatingTypes.Comfort);
	}

	public static PropertyResult? GetSecurityRating(IHasExposedProperties anObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Allegiance)anObjectToGetValueFrom).GetRating(getterKnowledge, RatingTypes.Security);
	}

	public static PropertyResult? GetFoodRating(IHasExposedProperties anObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Allegiance)anObjectToGetValueFrom).GetRating(getterKnowledge, RatingTypes.Food);
	}

	private PropertyResult? GetRating(SharedKnowledge getterKnowledge, RatingTypes statType)
	{
		return new PropertyResult
		{
			NumberResult = Statistics.GetRating(statType)
		};
	}

	public void SetPropertyValue(string propertyKey, PropertyResult? value)
	{
		Entity.SetPropertyValue(ref customFields, propertyKey, value);
	}

	public string GetCaption(string captionMethodKey)
	{
		throw new NotImplementedException();
	}

	public string GetDefaultCaption(string propertyKey)
	{
		throw new NotImplementedException();
	}

	public void GetDefaultKey(out string PropertyKey)
	{
		throw new NotImplementedException();
	}

	public EntityID? GetEntityID()
	{
		return null;
	}

	public bool GetIsSeenDirectly()
	{
		return true;
	}

	public Expedition GetExpedition(string key)
	{
		return Expeditions.FirstOrDefault((Expedition e) => e.KeyName == key);
	}

	public Expedition GetFirstExpedition()
	{
		if (Expeditions.Count > 0)
		{
			return Expeditions[0];
		}
		return null;
	}

	public void IterateMembers(Action<Entity> iterateFunction)
	{
		foreach (Entity member in Members)
		{
			iterateFunction(member);
		}
	}

	public void IterateOwnedItems(Action<EntityGroup> iterateFunction)
	{
		for (int num = Expeditions.Count - 1; num >= 0; num--)
		{
			iterateFunction(Expeditions[num].OwnedEntities);
		}
	}

	public bool IsEatable(EntityType food)
	{
		return FoodExtraction.IsEatable(food);
	}

	public AllegianceID GetUniqueID()
	{
		IDCounter++;
		if (IDCounter >= AllegianceID.Invalid)
		{
			throw new Exception("Astounding, AllegianceID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public AllegianceID SnapshotID(Snapshotter sn, AllegianceID id)
	{
		return sn.DoEnum(id);
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != AllegianceID.Invalid)
		{
			LookUp<Allegiance, AllegianceID>.Add(ID, this);
		}
	}

	public void SetInvalid()
	{
		id = AllegianceID.Invalid;
	}

	public void RemoveIDEntry()
	{
		LookUp<Allegiance, AllegianceID>.Remove(this);
	}

	void ILookUp<Allegiance, AllegianceID>.ResetIDCounter()
	{
	}

	public static void ResetIDCounter()
	{
		IDCounter = AllegianceID.First;
	}

	void ILookUp<Allegiance, AllegianceID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUp<Allegiance, AllegianceID>.Create();
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

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion((Snapshotter.Version)2u);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		IDCounter = sn.DoEnum(IDCounter);
		id = SnapshotID(sn, id);
		hasEntityGroupID = sn.DoEnum(hasEntityGroupID);
		canIterateEntitiesID = sn.DoEnum(canIterateEntitiesID);
		AllegianceType = sn.DoEnum(AllegianceType);
		DebugColor = sn.DoColor(DebugColor);
		FoodExtraction = (FoodExtraction)sn.DoISnapshot(FoodExtraction);
		HumanActivities = (HumanActivities)sn.DoISnapshot(HumanActivities);
		KeyName = sn.DoString(KeyName);
		Name = sn.DoString(Name);
		RepresentativeEntityType = sn.DoGameData(RepresentativeEntityType);
		SharedKnowledge = (SharedKnowledge)sn.DoISnapshot(SharedKnowledge);
		snapshotThreatManager = sn.SnapshotID<ICyclable, CyclableID>(ThreatAndCombatJobManager);
		OtherSiteAllegianceManager = (OtherSiteAllegianceManager)sn.DoISnapshot(OtherSiteAllegianceManager);
		Statistics = (GroupStatistics)sn.DoISnapshot(Statistics);
		tradeCredits = sn.DoDecimalNullable(tradeCredits);
		Policy = (AllegiancePolicy)sn.DoISnapshot(Policy);
		AllegiancesWeAreInContactWith = sn.DoHashSet(AllegiancesWeAreInContactWith);
		PermitsImmigration = sn.DoBool(PermitsImmigration);
		IndependentMembers = sn.DoList(IndependentMembers);
		snapshotSiteID = sn.SnapshotID<Site, SiteID>(Site).Value;
		snapshotThreatGroupID = sn.SnapshotID<ThreatGroup, ThreatGroupID>(ThreatGroup);
		if (sn.mode != Snapshotter.Mode.Load)
		{
			snapshotMembers = Members.Select((Entity c) => c.EntityID).ToList();
			snapshotExpeditions = Expeditions.Select((Expedition e) => e.ID).ToList();
			if (Missions != null)
			{
				snapshotMissions = Missions.Select((Mission c) => c.ID).ToList();
			}
		}
		snapshotMembers = sn.DoList(snapshotMembers);
		snapshotExpeditions = sn.DoList(snapshotExpeditions);
		snapshotMissions = sn.DoList(snapshotMissions);
		forageAndHuntingRadius = sn.DoInt32Nullable(forageAndHuntingRadius);
		customFields = sn.DoDictionary(customFields);
		sn.Ignore(Missions);
		sn.Ignore(Expeditions);
		sn.Ignore(ThreatAndCombatJobManager);
		sn.Ignore(MembersList);
		sn.Ignore(Members);
		sn.Ignore(exposedPropertyValueFunctions);
		sn.Ignore(Persons);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		foreach (EntityID snapshotMember in snapshotMembers)
		{
			Entity entity = Entity.FindByID(snapshotMember);
			Members.Add(entity);
			MembersList.Add(entity);
			if (entity.EntityType.Person != null)
			{
				Persons.Add(entity);
			}
		}
		Site = LookUp<Site, SiteID>.FindByID(snapshotSiteID);
		if (snapshotThreatGroupID.HasValue)
		{
			ThreatGroup = LookUp<ThreatGroup, ThreatGroupID>.FindByID(snapshotThreatGroupID.Value);
		}
		if (HumanActivities != null)
		{
			HumanActivities.LoadPostProcess(sn);
		}
		if (SharedKnowledge != null)
		{
			SharedKnowledge.LoadPostProcess(sn);
		}
		if (snapshotThreatManager.HasValue)
		{
			ThreatAndCombatJobManager = (ThreatJobManager)LookUp<ICyclable, CyclableID>.FindByID(snapshotThreatManager.Value);
		}
		if (OtherSiteAllegianceManager != null)
		{
			OtherSiteAllegianceManager.LoadPostProcess(sn);
		}
		if (FoodExtraction != null)
		{
			FoodExtraction.LoadPostProcess(sn);
		}
		Statistics.LoadPostProcess(sn);
		Expeditions = snapshotExpeditions.Select((ExpeditionID e) => Expedition.FindByID(e)).ToList();
		Policy.LoadPostProcess(sn);
		if (snapshotMissions != null)
		{
			Missions = snapshotMissions.Select((MissionID t) => LookUp<Mission, MissionID>.FindByID(t)).ToList();
			snapshotMissions = null;
		}
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
}
