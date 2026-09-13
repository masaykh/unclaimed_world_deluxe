using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.AI;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.Overland.Locations;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Overland;

public class World : ISnapshot, IHasExposedProperties
{
	public Dictionary<string, Site> AllSites = new Dictionary<string, Site>();

	private List<SiteID> snapshotSites;

	private HashSet<EntityID> OffSiteEntities = new HashSet<EntityID>();

	public double WorldRadius;

	public float ViewLatitudeStart;

	public float ViewLatitudeEnd;

	public float ViewLongitudeStart;

	public float ViewLongitudeEnd;

	public Dictionary<AllegianceID, List<AllegianceRelation>> Relations = new Dictionary<AllegianceID, List<AllegianceRelation>>();

	private Dictionary<AllegianceID, List<AllegianceRelationID>> snapshotRelations = new Dictionary<AllegianceID, List<AllegianceRelationID>>();

	public Dictionary<RouteType, Dictionary<SiteID, Dictionary<SiteID, RouteID>>> AllRoutes = new Dictionary<RouteType, Dictionary<SiteID, Dictionary<SiteID, RouteID>>>();

	public EntityID? LastSpawnedEntity;

	private const string keyName = "world";

	private Dictionary<string, PropertyResult> customFields;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public string KeyName => "world";

	public bool IsSnapshotted { get; set; }

	public static World CreateFromWorldData(WorldData worldData)
	{
		return new World
		{
			WorldRadius = worldData.WorldRadius,
			ViewLatitudeStart = worldData.ViewLatitudeStart,
			ViewLatitudeEnd = worldData.ViewLatitudeEnd,
			ViewLongitudeStart = worldData.ViewLongitudeStart,
			ViewLongitudeEnd = worldData.ViewLongitudeEnd
		};
	}

	public Allegiance GetAllegianceFromKey(string key)
	{
		foreach (Site value in AllSites.Values)
		{
			foreach (Allegiance allegiance in value.Allegiances)
			{
				if (allegiance.KeyName == key)
				{
					return allegiance;
				}
			}
		}
		return null;
	}

	public AllegianceRelation GetRelationBetweenAllegianceIDs(AllegianceID id1, AllegianceID id2)
	{
		if (Relations.TryGetValue(id1, out var value))
		{
			return value.FirstOrDefault((AllegianceRelation a) => a.AllegianceA.ID == id2 || a.AllegianceB.ID == id2);
		}
		return null;
	}

	public void RemoveRelation(AllegianceID allegianceID)
	{
		if (!The.Sim.World.Relations.TryGetValue(allegianceID, out var value))
		{
			return;
		}
		for (int num = value.Count - 1; num >= 0; num--)
		{
			AllegianceRelation allegianceRelation = value[num];
			value.RemoveAt(num);
			if (The.Sim.World.Relations.TryGetValue(allegianceRelation.AllegianceB.ID, out var value2))
			{
				value2.Remove(allegianceRelation);
			}
			allegianceRelation.Destroy();
		}
	}

	public void AddEntityBetweenSites(Entity entity)
	{
		OffSiteEntities.Add(entity.ID);
	}

	public void RemoveEntityFromBetweenSites(Entity entity)
	{
		OffSiteEntities.Remove(entity.ID);
	}

	public Site GetPlaySite()
	{
		foreach (Site value in AllSites.Values)
		{
			if (value.IsPlaySite)
			{
				return value;
			}
		}
		return null;
	}

	public void Update(GameTime gameTime)
	{
		foreach (KeyValuePair<string, Site> allSite in AllSites)
		{
			allSite.Value.Update(gameTime);
		}
	}

	public double GetAirDistance(GeodeticCoordinate coords1, GeodeticCoordinate coords2)
	{
		return DistanceCalculator.Haversine(coords1, coords2, WorldRadius);
	}

	public List<Tuple<Route, double>> GetRoutesAndDistances(Site fromSite, Site toSite)
	{
		List<Tuple<Route, double>> list = new List<Tuple<Route, double>>();
		double airDistance = The.Sim.World.GetAirDistance(fromSite.Coords, toSite.Coords);
		list.Add(new Tuple<Route, double>(null, airDistance));
		foreach (RouteType value4 in Enum.GetValues(typeof(RouteType)))
		{
			if (The.Sim.World.AllRoutes.TryGetValue(value4, out var value) && value.TryGetValue(fromSite.ID, out var value2) && value2.TryGetValue(toSite.ID, out var value3))
			{
				Route route = LookUp<Route, RouteID>.FindByID(value3);
				list.Add(new Tuple<Route, double>(route, route.Length));
			}
		}
		return list;
	}

	public void GetChildren(string keyToList, ref List<IHasExposedProperties> resultList, FilterCondition filter, EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget, SharedKnowledge getterKnowledge = null)
	{
		bool wasFiltered = false;
		if (keyToList == "sites")
		{
			GetSites(this, filter, ref resultList, out wasFiltered, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
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

	public PropertyResult? GetPropertyValue(string propertyKey, SharedKnowledge getterKnowledge = null, IHasExposedProperties parent = null)
	{
		return null;
	}

	public string GetDefaultCaption(string propertyKey)
	{
		return "World";
	}

	public void GetDefaultKey(out string propertyKey)
	{
		propertyKey = "world";
	}

	public string GetCaption(string captionKey)
	{
		return null;
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

	public static void GetSites(IHasExposedProperties source, FilterCondition filter, ref List<IHasExposedProperties> listToFillWithProperties, out bool wasFiltered, EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
	{
		World world = (World)source;
		if (filter is PropertyCondition { PropertyKey: "name" } propertyCondition)
		{
			if (world.AllSites.TryGetValue(propertyCondition.ConstantStringEqual, out var value))
			{
				listToFillWithProperties.Add(value);
			}
			wasFiltered = true;
		}
		else
		{
			listToFillWithProperties.AddRange(world.AllSites.Values.ToList());
			wasFiltered = false;
		}
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		if (sn.mode != Snapshotter.Mode.Load)
		{
			snapshotSites = new List<SiteID>();
			foreach (KeyValuePair<string, Site> allSite in AllSites)
			{
				snapshotSites.Add(allSite.Value.ID);
			}
			snapshotRelations = new Dictionary<AllegianceID, List<AllegianceRelationID>>();
			foreach (KeyValuePair<AllegianceID, List<AllegianceRelation>> relation in Relations)
			{
				snapshotRelations.Add(relation.Key, relation.Value.Select((AllegianceRelation r) => r.ID).ToList());
			}
		}
		snapshotRelations = sn.DoMultiMap(snapshotRelations);
		snapshotSites = sn.DoList(snapshotSites);
		WorldRadius = sn.DoDouble(WorldRadius);
		LastSpawnedEntity = sn.DoEnumNullable(LastSpawnedEntity);
		OffSiteEntities = sn.DoHashSet(OffSiteEntities);
		ViewLatitudeStart = sn.DoFloat(ViewLatitudeStart);
		ViewLatitudeEnd = sn.DoFloat(ViewLatitudeEnd);
		ViewLongitudeStart = sn.DoFloat(ViewLongitudeStart);
		ViewLongitudeEnd = sn.DoFloat(ViewLongitudeEnd);
		customFields = sn.DoDictionary(customFields);
		AllRoutes = sn.DoDoubleNestedDictionary(AllRoutes);
		sn.Ignore(Relations);
		sn.Ignore(AllSites);
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
		AllSites.Clear();
		foreach (SiteID snapshotSite in snapshotSites)
		{
			Site site = LookUp<Site, SiteID>.FindByID(snapshotSite);
			AllSites.Add(site.KeyName, site);
		}
		foreach (KeyValuePair<AllegianceID, List<AllegianceRelationID>> snapshotRelation in snapshotRelations)
		{
			Relations.Add(snapshotRelation.Key, snapshotRelation.Value.Select((AllegianceRelationID r) => LookUp<AllegianceRelation, AllegianceRelationID>.FindByID(r)).ToList());
		}
		snapshotRelations = null;
	}
}
