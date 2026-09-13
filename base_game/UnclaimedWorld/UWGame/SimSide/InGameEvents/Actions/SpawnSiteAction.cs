using System.Collections.Generic;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Overland.Locations;

namespace UWGame.SimSide.InGameEvents.Actions;

public class SpawnSiteAction : EventActionType
{
	public float? DistanceFromPlaySite;

	public float? BearingFromPlaySite;

	public string SiteDataKey;

	public SiteData SiteData;

	public SpawnSiteAction(string keyName)
		: base(keyName)
	{
	}

	public SpawnSiteAction()
	{
	}

	public override bool Execute(EventAction action, ref string failReason)
	{
		SiteData siteData = ((SiteDataKey == null) ? SiteData : GameData.Instance.AllSiteData[SiteDataKey]);
		if (siteData != null)
		{
			if (The.Sim.World.AllSites.ContainsKey(siteData.KeyName))
			{
				return false;
			}
			GeodeticCoordinate coords;
			if (DistanceFromPlaySite.HasValue && BearingFromPlaySite.HasValue)
			{
				if (The.Sim.PlaySite == null)
				{
					failReason = "Playsite not spawned yet";
					return false;
				}
				coords = DistanceCalculator.CoordFromDistance(The.Sim.PlaySite.Coords, BearingFromPlaySite.Value, DistanceFromPlaySite.Value, The.Sim.World.WorldRadius);
			}
			else
			{
				coords = siteData.Coords;
			}
			Site.CreateFromSiteData(siteData, coords);
			return true;
		}
		return false;
	}

	public void PreInitValidate(List<string> listOfErrors)
	{
	}

	public new void Initialize()
	{
	}

	public void PostInitValidate(List<string> listOfErrors)
	{
		_ = SiteData;
	}

	public void PostLoadContentValidate(List<string> listOfErrors)
	{
	}

	public override string ToString()
	{
		if (SiteDataKey != null)
		{
			return "Spawn " + SiteDataKey;
		}
		return "Spawn " + SiteData.KeyName;
	}
}
