using System.Collections.Generic;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Overland.Locations;

namespace UWGame.SimSide.AllGameData;

public class SiteDataLoader
{
	public static List<SiteData> Init()
	{
		List<SiteData> list = new List<SiteData>();
		list.Add(new SiteData
		{
			Name = "Cudgel Hills",
			KeyName = "playSite",
			Description = "This varied landscape was chosen as a home by us, the founders of Castor's Homestead.",
			Coords = new GeodeticCoordinate(9.65, 72.1),
			IsPlaySite = true,
			ShowLabel = true,
			ShowTallPin = true,
			SiteMarkerOrder = 10
		});
		list.Add(new SiteData
		{
			KeyName = "randomSmallSiteDestinyRiver",
			SiteTemplates = new StringChance[3]
			{
				new StringChance
				{
					Edge = 0.33f,
					String = "smallFarmingSite"
				},
				new StringChance
				{
					Edge = 0.66f,
					String = "smallMiningSite"
				},
				new StringChance
				{
					Edge = 1f,
					String = "smallFishingSite"
				}
			},
			Coords = new GeodeticCoordinate(9.8, 71.7),
			IsPlaySite = false,
			ShowLabel = true,
			ShowTallPin = true,
			SiteMarkerOrder = 10
		});
		list.Add(new SiteData
		{
			Name = "Destiny River Delta",
			KeyName = "destinyRiverDeltaDescentEraSite",
			Description = "The big settlement that we set out from. Originally a research outpost founded by our ancestors, the first pioneers.",
			Coords = new GeodeticCoordinate(10.25, 71.25),
			IsPlaySite = false,
			ShowLabel = true,
			ShowTallPin = true,
			SiteMarkerOrder = 10
		});
		return list;
	}
}
