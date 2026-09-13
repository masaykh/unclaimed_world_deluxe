using System;
using System.Collections.Generic;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;

namespace UWGame.ClientSide.Interface.Inventory;

public class SearchTextFilterSetting : ISnapshot
{
	public string SearchText;

	private HashSet<EntityType> Results;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public SearchTextFilterSetting()
	{
	}

	public SearchTextFilterSetting(string text)
	{
		SearchText = text;
	}

	public HashSet<EntityType> GetData(Predicate<EntityType> filter)
	{
		if (Results == null)
		{
			Results = GetEntityTypesMatchingString(filter);
		}
		return Results;
	}

	private HashSet<EntityType> GetEntityTypesMatchingString(Predicate<EntityType> filter)
	{
		HashSet<EntityType> hashSet = new HashSet<EntityType>();
		string value = SearchText.ToLower();
		foreach (KeyValuePair<string, EntityType> allEntityType in GameData.Instance.AllEntityTypes)
		{
			if ((filter == null || filter(allEntityType.Value)) && allEntityType.Value.Name.ToLower().Contains(value))
			{
				hashSet.Add(allEntityType.Value);
			}
		}
		return hashSet;
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		SearchText = sn.DoString(SearchText);
		sn.Ignore(Results);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}
}
