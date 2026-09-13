using System.Collections.Generic;
using UWGame.SimSide.Entities.Containers;

namespace UWGame.SimSide.AllGameData;

public class UpgradeProfileLoader
{
	public static List<UpgradeProfile> Init()
	{
		List<UpgradeProfile> list = new List<UpgradeProfile>();
		list.Add(new UpgradeProfile
		{
			KeyName = "survivalHome1People",
			UpgradeCategories = new string[1] { "mats1People" }
		});
		list.Add(new UpgradeProfile
		{
			KeyName = "survivalHome2People",
			UpgradeCategories = new string[1] { "mats2People" }
		});
		list.Add(new UpgradeProfile
		{
			KeyName = "survivalHome3People",
			UpgradeCategories = new string[1] { "mats3People" }
		});
		list.Add(new UpgradeProfile
		{
			KeyName = "survivalHome4People",
			UpgradeCategories = new string[1] { "mats4People" }
		});
		list.Add(new UpgradeProfile
		{
			KeyName = "basicHome3People",
			UpgradeCategories = new string[1] { "bedsOrMats3People" }
		});
		list.Add(new UpgradeProfile
		{
			KeyName = "basicHome4People",
			UpgradeCategories = new string[2] { "bedsOrMats4People", "furniture4People" }
		});
		list.Add(new UpgradeProfile
		{
			KeyName = "workshopProfile",
			UpgradeCategories = new string[1] { "workshop" }
		});
		list.Add(new UpgradeProfile
		{
			KeyName = "cookhouseProfile",
			UpgradeCategories = new string[4] { "stove", "communityHall", "smokeOven", "dryingShed" }
		});
		return list;
	}
}
