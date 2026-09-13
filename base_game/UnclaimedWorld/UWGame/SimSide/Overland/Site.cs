using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.AI;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Collisions;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.InGameEvents.SpecialEvents;
using UWGame.SimSide.IngameEvents;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Overland.Locations;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems.TimeSlicing;

namespace UWGame.SimSide.Overland;

public class Site : IHasExposedProperties, ISnapshot, ILookUp<Site, SiteID>
{
	private static Dictionary<string, GetPropertyValue> exposedPropertyValueFunctions;

	public string Name;

	private string keyName;

	public string Description;

	private bool isPlaySite;

	public bool ShowLabel = true;

	public bool ShowTallPin = true;

	public int SiteMarkerOrder;

	public GeodeticCoordinate Coords;

	public List<Allegiance> Allegiances = new List<Allegiance>();

	private List<AllegianceID> snapshotAllegiances;

	public Allegiance PlayerAllegiance;

	private AllegianceID? snapshotPlayerAllegiance;

	public List<ThreatGroup> ThreatGroups = new List<ThreatGroup>();

	private List<ThreatGroupID> snapshotThreatGroups;

	public HashSet<EntityID> AllEntities = new HashSet<EntityID>();

	public ObservableList<Entity> Entities = new ObservableList<Entity>();

	private List<EntityID> snapshotEntities;

	public Dictionary<string, EntityID> EntitiesByName = new Dictionary<string, EntityID>();

	public Dictionary<string, List<EntityID>> EntitiesByType = new Dictionary<string, List<EntityID>>();

	public Dictionary<ResourceType, ObservableList<ResourceContainer>> Resources = new Dictionary<ResourceType, ObservableList<ResourceContainer>>();

	private Dictionary<ResourceType, List<ResourceID>> snapshotResources;

	public HashSet<ResourceType> EditorResources = new HashSet<ResourceType>();

	public PlaySite PlaySite;

	public EventManager EventManager;

	private CyclableID eventManagerID;

	private static char[] separator;

	private Dictionary<string, PropertyResult> customFields;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	private SiteID id = SiteID.Invalid;

	private static SiteID IDCounter;

	public bool IsPlaySite
	{
		get
		{
			return isPlaySite;
		}
		set
		{
			isPlaySite = value;
			if (isPlaySite)
			{
				The.Sim.PlaySite = this;
			}
		}
	}

	public string KeyName => keyName;

	public bool IsSnapshotted { get; set; }

	public SiteID ID
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

	public Site(string keyName, bool isPlaySite = false)
	{
		this.keyName = keyName;
		AddToLookup();
		The.Sim.World.AllSites.Add(keyName, this);
		IsPlaySite = isPlaySite;
		EventManager = new EventManager(this);
		eventManagerID = EventManager.ID;
		if (isPlaySite)
		{
			PlaySite = new PlaySite();
			Allegiances = The.Sim.PlaySite.Allegiances;
		}
	}

	public Site()
	{
	}

	static Site()
	{
		exposedPropertyValueFunctions = new Dictionary<string, GetPropertyValue>();
		separator = new char[1] { ':' };
		IDCounter = SiteID.First;
		exposedPropertyValueFunctions.Add("getJournalHeaderNames", GetJournalHeaderNames);
		exposedPropertyValueFunctions.Add("getJournalDate", GetJournalDate);
		exposedPropertyValueFunctions.Add("getDate", GetDate);
		exposedPropertyValueFunctions.Add("getLocation", GetLocation);
		exposedPropertyValueFunctions.Add("getTime", GetElapsedTime);
		exposedPropertyValueFunctions.Add("getRandomSimNumber", GetRandomSimNumber);
		exposedPropertyValueFunctions.Add("getRandomClientNumber", GetRandomClientNumber);
	}

	public static Site CreateFromSiteData(SiteData siteData, GeodeticCoordinate coords)
	{
		Site site = new Site(siteData.KeyName, siteData.IsPlaySite)
		{
			Name = siteData.Name,
			keyName = siteData.KeyName,
			Description = siteData.Description,
			Coords = coords,
			ShowLabel = siteData.ShowLabel,
			ShowTallPin = siteData.ShowTallPin,
			SiteMarkerOrder = siteData.SiteMarkerOrder
		};
		if (siteData.SiteTemplates != null)
		{
			int stairstep;
			StringChance stairStepIndex = Common.GetStairStepIndex(siteData.SiteTemplates, out stairstep, The.Sim.GameplayRandomGenerator);
			GameData.Instance.AllSiteTemplates[stairStepIndex.String].FillSite(site, siteData.AllegianceKeyName, siteData.ExpeditionKeyName);
		}
		return site;
	}

	public Expedition GetFirstPlayerExpedition()
	{
		if (PlayerAllegiance != null)
		{
			if (PlayerAllegiance.Expeditions.Count > 0)
			{
				return PlayerAllegiance.Expeditions[0];
			}
			return null;
		}
		return null;
	}

	public bool PlaceAllGeometry(ref int geoLayoutEntityProgress, int entitiesPerCycle)
	{
		if (isPlaySite)
		{
			int num = Math.Min(geoLayoutEntityProgress + entitiesPerCycle, Entities.Count);
			for (int i = geoLayoutEntityProgress; i < num; i++)
			{
				Entities[i].PlaceGeometryLayoutIfDirty(GeoPlaceMode.ShapeChanged);
			}
			if (num == Entities.Count)
			{
				geoLayoutEntityProgress = 0;
				return true;
			}
			geoLayoutEntityProgress = num;
			return false;
		}
		return true;
	}

	public void InitAuxiliaryMaps()
	{
		if (!isPlaySite)
		{
			return;
		}
		foreach (Allegiance allegiance in Allegiances)
		{
			allegiance.SharedKnowledge.PlaySiteKnowledge.InitAuxiliaryMaps();
		}
	}

	public void Update(GameTime gameTime)
	{
		foreach (Allegiance allegiance in Allegiances)
		{
			allegiance.Update(gameTime);
		}
		EventManager.Update(gameTime);
		if (PlaySite != null)
		{
			PlaySite.Update(gameTime);
		}
	}

	public void RemoveEntity(Entity entity)
	{
		AllEntities.Remove(entity.ID);
		Entities.Remove(entity);
		if (EntitiesByType.TryGetValue(entity.EntityType.KeyName, out var value))
		{
			value.Remove(entity.EntityID);
		}
		if (!string.IsNullOrEmpty(entity.Name))
		{
			EntitiesByName.Remove(entity.Name);
		}
	}

	public void AddEntity(Entity entity)
	{
		if (!AllEntities.Contains(entity.ID))
		{
			AllEntities.Add(entity.ID);
			Entities.Add(entity);
			if (!string.IsNullOrEmpty(entity.Name) && !EntitiesByName.ContainsKey(entity.Name))
			{
				EntitiesByName.Add(entity.Name, entity.EntityID);
			}
			Common.AddToMultiList(EntitiesByType, entity.EntityType.KeyName, entity.EntityID);
		}
	}

	public void AddResourceContainer(ResourceContainer resourceContainer)
	{
		if (!Resources.TryGetValue(resourceContainer.ResourceType, out var value))
		{
			value = new ObservableList<ResourceContainer>();
			Resources.Add(resourceContainer.ResourceType, value);
		}
		value.Add(resourceContainer);
	}

	public string GetCaption(string captionKey)
	{
		return null;
	}

	public static PropertyResult? GetJournalHeaderNames(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Site)hasExposed).GetJournalHeaderNames();
	}

	public PropertyResult? GetJournalHeaderNames()
	{
		List<Entity> list = new List<Entity>(PlayerAllegiance.MembersList);
		AgentCondition canParticipate = UnhappinessGroupMeetingEvent.GetMeetingParticipantConditions();
		list = list.FindAll((Entity e) => e.PersonEntity != null && e.Site == The.Sim.PlaySite && canParticipate.IsFulfilled(e));
		PropertyResult value = default(PropertyResult);
		if (list.Count > 0)
		{
			Entity randomPerson = Common.GetRandomListMember(list, The.Sim.GameplayRandomGenerator);
			string text = Common.ListToCommaSeparatedString(list.FindAll((Entity e) => e != randomPerson), (Entity e) => SubstituteValue.FormatEntity(e));
			string text2 = $"RECORDED BY: {SubstituteValue.FormatEntity(randomPerson)}";
			if (!string.IsNullOrEmpty(text))
			{
				text2 += $" \nPRESENT: {text}";
			}
			value.StringResult = text2;
		}
		return value;
	}

	public static void GetEntities(IHasExposedProperties source, FilterCondition filter, bool onlyFinished, ref List<IHasExposedProperties> listToFillWithProperties, out bool wasFiltered, EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
	{
		Site site = (Site)source;
		if (filter is PropertyCondition propertyCondition)
		{
			if (propertyCondition.PropertyKey == "name")
			{
				if (site.EntitiesByName.TryGetValue(propertyCondition.ConstantStringEqual, out var value))
				{
					Entity entity = Entity.FindByID(value);
					if (entity != null)
					{
						listToFillWithProperties.Add(entity);
					}
				}
				wasFiltered = true;
			}
			else if (propertyCondition.PropertyKey == "ID")
			{
				string stringCompareValue = propertyCondition.GetStringCompareValue(triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
				if (stringCompareValue != null)
				{
					Entity entity2 = Entity.FindByID((EntityID)long.Parse(stringCompareValue));
					if (entity2 != null)
					{
						listToFillWithProperties.Add(entity2);
					}
				}
				wasFiltered = true;
			}
			else if (propertyCondition.PropertyKey == "type")
			{
				if (site.EntitiesByType.TryGetValue(propertyCondition.ConstantStringEqual, out var value2))
				{
					foreach (EntityID item in value2)
					{
						Entity entity3 = Entity.FindByID(item);
						if (entity3 != null)
						{
							listToFillWithProperties.Add(entity3);
						}
					}
				}
				wasFiltered = true;
			}
			else
			{
				listToFillWithProperties.AddRange(site.Entities.GetAsList());
				wasFiltered = false;
			}
		}
		else
		{
			listToFillWithProperties.AddRange(site.Entities.GetAsList());
			wasFiltered = false;
		}
		if (onlyFinished)
		{
			listToFillWithProperties.RemoveAll((IHasExposedProperties i) => !((Entity)i).IsCompleted());
		}
	}

	public static void GetAllegiances(IHasExposedProperties source, ref List<IHasExposedProperties> listToFillWithProperties)
	{
		Site site = (Site)source;
		listToFillWithProperties.AddRange(site.Allegiances);
	}

	public void GetChildren(string keyToList, ref List<IHasExposedProperties> resultList, FilterCondition filter, EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget, SharedKnowledge getterKnowledge = null)
	{
		bool wasFiltered = false;
		switch (keyToList)
		{
		case "entities":
			GetEntities(this, filter, onlyFinished: false, ref resultList, out wasFiltered, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
			break;
		case "finishedEntities":
			GetEntities(this, filter, onlyFinished: true, ref resultList, out wasFiltered, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
			break;
		case "allegiances":
			GetAllegiances(this, ref resultList);
			break;
		}
		if (!wasFiltered && filter != null)
		{
			FilterChildren(resultList, filter, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
		}
	}

	public static void FilterChildren(List<IHasExposedProperties> resultList, FilterCondition filter, EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
	{
		resultList.RemoveAll((IHasExposedProperties h) => !filter.IsFulfilled(h, triggeringEntity, targetEntity, polledEventSource, dynamicTarget));
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

	public string GetDefaultCaption(string propertyKey)
	{
		return Name;
	}

	public void GetDefaultKey(out string propertyKey)
	{
		propertyKey = keyName;
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

	public static PropertyResult? GetLocation(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Site)hasExposed).GetLocation();
	}

	private PropertyResult? GetLocation()
	{
		return new PropertyResult
		{
			StringResult = Coords.ToString()
		};
	}

	public static PropertyResult? GetJournalDate(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Site)hasExposed).GetJournalDate();
	}

	private PropertyResult? GetJournalDate()
	{
		return new PropertyResult
		{
			StringResult = The.Sim.DateAndTime.CurrentTimeDateYear.GetDateForJournal()
		};
	}

	public static PropertyResult? GetDate(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Site)hasExposed).GetDate();
	}

	private PropertyResult? GetDate()
	{
		return new PropertyResult
		{
			DateResult = The.Sim.DateAndTime.CurrentTimeDateYear
		};
	}

	public static PropertyResult? GetElapsedTime(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Site)hasExposed).GetElapsedTime();
	}

	private PropertyResult? GetElapsedTime()
	{
		return new PropertyResult
		{
			NumberResult = (float)The.Sim.TotalUnPausedGameTimeInSeconds
		};
	}

	public static PropertyResult? GetRandomSimNumber(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Site)hasExposed).GetRandomSimNumber();
	}

	private PropertyResult? GetRandomSimNumber()
	{
		return new PropertyResult
		{
			NumberResult = (float)The.Sim.GameplayRandomGenerator.NextDouble("getRandomSimNumber")
		};
	}

	public static PropertyResult? GetRandomClientNumber(IHasExposedProperties hasExposed, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Site)hasExposed).GetRandomClientNumber();
	}

	private PropertyResult? GetRandomClientNumber()
	{
		if (The.Client != null)
		{
			return new PropertyResult
			{
				NumberResult = (float)The.Client.ClientRandomGenerator.NextDouble("getRandomClientNumber")
			};
		}
		return new PropertyResult
		{
			NumberResult = 0f
		};
	}

	public void PrintGlobalScriptVariables(StringBuilder description)
	{
		if (customFields == null)
		{
			return;
		}
		foreach (KeyValuePair<string, PropertyResult> customField in customFields)
		{
			description.AppendLine(customField.Key + " = " + customField.Value.ToString());
		}
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		id = SnapshotID(sn, id);
		IDCounter = sn.DoEnum(IDCounter);
		Name = sn.DoString(Name);
		keyName = sn.DoString(keyName);
		Description = sn.DoString(Description);
		if (sn.mode != Snapshotter.Mode.Load)
		{
			snapshotAllegiances = Allegiances.Select((Allegiance a) => a.ID).ToList();
			snapshotEntities = (from e in Entities.GetAsList()
				select e.EntityID).ToList();
			snapshotResources = new Dictionary<ResourceType, List<ResourceID>>();
			foreach (KeyValuePair<ResourceType, ObservableList<ResourceContainer>> resource in Resources)
			{
				snapshotResources.Add(resource.Key, (from r in resource.Value.GetAsList()
					select r.ID).ToList());
				resource.Value.Clear();
			}
			snapshotThreatGroups = ThreatGroups.Select((ThreatGroup t) => t.ID).ToList();
		}
		PlaySite = (PlaySite)sn.DoISnapshot(PlaySite);
		snapshotAllegiances = sn.DoList(snapshotAllegiances);
		customFields = sn.DoDictionary(customFields);
		snapshotPlayerAllegiance = sn.SnapshotID<Allegiance, AllegianceID>(PlayerAllegiance);
		Entities.Clear();
		Entities = (ObservableList<Entity>)sn.DoISnapshot(Entities);
		AllEntities = sn.DoHashSet(AllEntities);
		EntitiesByName = sn.DoDictionary(EntitiesByName);
		EntitiesByType = sn.DoMultiMap(EntitiesByType);
		snapshotEntities = sn.DoList(snapshotEntities);
		snapshotThreatGroups = sn.DoList(snapshotThreatGroups);
		Resources = sn.DoDictionary(Resources);
		snapshotResources = sn.DoMultiMap(snapshotResources);
		isPlaySite = sn.DoBool(isPlaySite);
		Coords = sn.DoGeodeticCoordinate(Coords);
		ShowLabel = sn.DoBool(ShowLabel);
		ShowTallPin = sn.DoBool(ShowTallPin);
		SiteMarkerOrder = sn.DoInt32(SiteMarkerOrder);
		eventManagerID = sn.DoEnum(eventManagerID);
		sn.Ignore(separator);
		sn.Ignore(EventManager);
		sn.Ignore(PlayerAllegiance);
		sn.Ignore(Allegiances);
		sn.Ignore(ThreatGroups);
		sn.Ignore(exposedPropertyValueFunctions);
		sn.Ignore(EditorResources);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		Entities.AddRange(snapshotEntities.Select((EntityID e) => Entity.FindByID(e)));
		Entities.LoadPostProcess(sn);
		if (snapshotResources != null)
		{
			foreach (KeyValuePair<ResourceType, List<ResourceID>> snapshotResource in snapshotResources)
			{
				Resources[snapshotResource.Key].AddRange(snapshotResource.Value.Select((ResourceID r) => LookUp<ResourceContainer, ResourceID>.FindByID(r)).ToList());
			}
			snapshotResources = null;
		}
		foreach (KeyValuePair<ResourceType, ObservableList<ResourceContainer>> resource in Resources)
		{
			resource.Value.LoadPostProcess(sn);
		}
		PlayerAllegiance = LookUp<Allegiance, AllegianceID>.FindByID(snapshotPlayerAllegiance);
		Allegiances = snapshotAllegiances.Select((AllegianceID a) => LookUp<Allegiance, AllegianceID>.FindByID(a)).ToList();
		if (snapshotThreatGroups != null)
		{
			ThreatGroups = snapshotThreatGroups.Select((ThreatGroupID t) => LookUp<ThreatGroup, ThreatGroupID>.FindByID(t)).ToList();
		}
		if (PlaySite != null)
		{
			PlaySite.LoadPostProcess(sn);
		}
		EventManager = (EventManager)LookUp<ICyclable, CyclableID>.FindByID(eventManagerID);
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public SiteID GetUniqueID()
	{
		IDCounter++;
		if (IDCounter >= SiteID.Invalid)
		{
			throw new Exception("Astounding, SiteID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public SiteID SnapshotID(Snapshotter sn, SiteID id)
	{
		return sn.DoEnum(id);
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != SiteID.Invalid)
		{
			LookUp<Site, SiteID>.Add(ID, this);
		}
	}

	public void SetInvalid()
	{
		id = SiteID.Invalid;
	}

	public void RemoveIDEntry()
	{
		LookUp<Site, SiteID>.Remove(this);
	}

	void ILookUp<Site, SiteID>.ResetIDCounter()
	{
	}

	public static void ResetIDCounter()
	{
		IDCounter = SiteID.First;
	}

	void ILookUp<Site, SiteID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUp<Site, SiteID>.Create();
	}
}
