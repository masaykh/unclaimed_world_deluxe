using System.Collections.Generic;
using UWGame.SimSide.Entities.Containers;

namespace UWGame.SimSide.AllGameData;

public class UpgradeCategoryLoader
{
	public static List<UpgradeCategory> Init()
	{
		List<UpgradeCategory> list = new List<UpgradeCategory>();
		string description = "Upgrading the mats will increase the comfort level for the residents.";
		string name = "Mats";
		int sortOrder = 10;
		string description2 = "Upgrading the beds will increase the comfort level for the residents.";
		string name2 = "Beds";
		list.Add(new UpgradeCategory
		{
			KeyName = "mats1People",
			Name = name,
			Description = description,
			SortOrder = sortOrder
		});
		list.Add(new UpgradeCategory
		{
			KeyName = "mats2People",
			Name = name,
			Description = description,
			SortOrder = sortOrder
		});
		list.Add(new UpgradeCategory
		{
			KeyName = "mats3People",
			Name = name,
			Description = description,
			SortOrder = sortOrder
		});
		list.Add(new UpgradeCategory
		{
			KeyName = "mats4People",
			Name = name,
			Description = description,
			SortOrder = sortOrder
		});
		int sortOrder2 = 10;
		list.Add(new UpgradeCategory
		{
			KeyName = "bedsOrMats3People",
			Name = name2,
			Description = description2,
			SortOrder = sortOrder2
		});
		list.Add(new UpgradeCategory
		{
			KeyName = "bedsOrMats4People",
			Name = name2,
			Description = description2,
			SortOrder = sortOrder2
		});
		list.Add(new UpgradeCategory
		{
			KeyName = "furniture4People",
			Name = "Furniture",
			Description = "",
			SortOrder = 13
		});
		list.Add(new UpgradeCategory
		{
			KeyName = "stove",
			Name = "Stove",
			Description = "Installing a stove makes it more convenient to cook food",
			SortOrder = 15
		});
		list.Add(new UpgradeCategory
		{
			KeyName = "communityHall",
			Name = "Community hall",
			Description = "",
			SortOrder = 20
		});
		list.Add(new UpgradeCategory
		{
			KeyName = "smokeOven",
			Name = "Smoke oven",
			Description = "",
			SortOrder = 25
		});
		list.Add(new UpgradeCategory
		{
			KeyName = "dryingShed",
			Name = "Drying shed",
			Description = "",
			SortOrder = 30
		});
		list.Add(new UpgradeCategory
		{
			KeyName = "workshop",
			Name = "Workshop",
			Description = "",
			SortOrder = 40
		});
		return list;
	}
}
