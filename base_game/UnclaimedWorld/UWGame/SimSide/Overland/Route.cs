using System;
using System.Collections.Generic;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Overland;

public class Route : ISnapshot, ILookUp<Route, RouteID>
{
	public Site Site1;

	private SiteID snapshotSite1;

	public Site Site2;

	private SiteID snapshotSite2;

	public float Length;

	public RouteType RouteType;

	private RouteID id = RouteID.Invalid;

	private static RouteID IDCounter = RouteID.First;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public RouteID ID
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

	public Route(Site fromSite, Site toSite, RouteType type, float length)
	{
		AddToLookup();
		Site1 = fromSite;
		Site2 = toSite;
		RouteType = type;
		Length = length;
		if (!The.Sim.World.AllRoutes.TryGetValue(RouteType, out var value))
		{
			value = new Dictionary<SiteID, Dictionary<SiteID, RouteID>>();
			The.Sim.World.AllRoutes.Add(RouteType, value);
		}
		Common.AddToNestedDictionary(value, Site1.ID, Site2.ID, ID);
	}

	public Route()
	{
		if (!Snapshotter.IsSnapshotting)
		{
			AddToLookup();
		}
	}

	public static void CreateFromRouteData(RouteData routeData)
	{
		if (The.Sim.World.AllSites.TryGetValue(routeData.FromSite, out var value) && The.Sim.World.AllSites.TryGetValue(routeData.ToSite, out var value2))
		{
			new Route(value, value2, routeData.RouteType, routeData.Length);
			new Route(value2, value, routeData.RouteType, routeData.Length);
		}
	}

	public RouteID GetUniqueID()
	{
		IDCounter++;
		if (IDCounter >= RouteID.Invalid)
		{
			throw new Exception("Astounding, RouteID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public RouteID SnapshotID(Snapshotter sn, RouteID id)
	{
		return sn.DoEnum(id);
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != RouteID.Invalid)
		{
			LookUp<Route, RouteID>.Add(ID, this);
		}
	}

	public void SetInvalid()
	{
		id = RouteID.Invalid;
	}

	public void RemoveIDEntry()
	{
		LookUp<Route, RouteID>.Remove(this);
	}

	void ILookUp<Route, RouteID>.ResetIDCounter()
	{
	}

	public static void ResetIDCounter()
	{
		IDCounter = RouteID.First;
	}

	void ILookUp<Route, RouteID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUp<Route, RouteID>.Create();
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		id = sn.DoEnum(id);
		IDCounter = sn.DoEnum(IDCounter);
		snapshotSite1 = sn.SnapshotID<Site, SiteID>(Site1).Value;
		snapshotSite2 = sn.SnapshotID<Site, SiteID>(Site2).Value;
		Length = sn.DoFloat(Length);
		RouteType = sn.DoEnum(RouteType);
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
		Site1 = LookUp<Site, SiteID>.FindByID(snapshotSite1);
		Site2 = LookUp<Site, SiteID>.FindByID(snapshotSite2);
	}
}
