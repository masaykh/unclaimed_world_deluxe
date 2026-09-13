using System;
using System.Collections.Generic;
using System.Linq;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;

namespace UWGame.ClientSide.Interface.Inventory;

public class FilterPropertySettings : ISnapshot
{
	public Dictionary<string, FilterSetting> ActiveFilterSettings = new Dictionary<string, FilterSetting>();

	private List<FilterSettingType> snapshotActiveFilterSettings;

	private SearchTextFilterSetting SearchTextFilter;

	public bool SearchTextActive;

	private bool settingsAreDirty = true;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool SettingsAreDirty => settingsAreDirty;

	public bool IsSnapshotted { get; set; }

	public void SetSettingsNotDirty()
	{
		settingsAreDirty = false;
	}

	public bool AddFilterSetting(FilterSetting filter)
	{
		if (!ActiveFilterSettings.ContainsKey(filter.FilterSettingType.KeyName))
		{
			ActiveFilterSettings.Add(filter.FilterSettingType.KeyName, filter);
			settingsAreDirty = true;
			return true;
		}
		return false;
	}

	public void EnableSearchText(string text)
	{
		if (text == "")
		{
			SearchTextFilter = null;
			settingsAreDirty = true;
		}
		else if (SearchTextFilter == null || SearchTextFilter.SearchText != text)
		{
			SearchTextFilter = new SearchTextFilterSetting(text);
			settingsAreDirty = true;
		}
		if (!SearchTextActive)
		{
			settingsAreDirty = true;
			SearchTextActive = true;
		}
	}

	public string GetSearchText()
	{
		if (SearchTextFilter != null)
		{
			return SearchTextFilter.SearchText;
		}
		return "";
	}

	public bool DisableSearchText()
	{
		if (SearchTextActive)
		{
			SearchTextActive = false;
			settingsAreDirty = true;
		}
		return true;
	}

	public bool RemoveFilterSetting(FilterSetting filter)
	{
		if (ActiveFilterSettings.Remove(filter.FilterSettingType.KeyName))
		{
			settingsAreDirty = true;
			return true;
		}
		return false;
	}

	public void RemoveAllFilterSettings()
	{
		if (ActiveFilterSettings.Count > 0)
		{
			ActiveFilterSettings.Clear();
			settingsAreDirty = true;
		}
		if (SearchTextFilter != null || SearchTextActive)
		{
			SearchTextFilter = null;
			SearchTextActive = false;
			settingsAreDirty = true;
		}
	}

	public bool HasActiveFilters()
	{
		if (ActiveFilterSettings.Count > 0 || SearchTextActive)
		{
			return true;
		}
		return false;
	}

	public List<HashSet<EntityType>> GetFilteredEntities(Predicate<EntityType> filter)
	{
		List<HashSet<EntityType>> list = new List<HashSet<EntityType>>();
		if (ActiveFilterSettings.Count > 0)
		{
			foreach (KeyValuePair<string, FilterSetting> activeFilterSetting in ActiveFilterSettings)
			{
				list.Add(activeFilterSetting.Value.GetData(filter));
			}
		}
		if (SearchTextActive && SearchTextFilter != null)
		{
			list.Add(SearchTextFilter.GetData(filter));
		}
		return list;
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		if (sn.mode != Snapshotter.Mode.Load)
		{
			snapshotActiveFilterSettings = ActiveFilterSettings.Select((KeyValuePair<string, FilterSetting> k) => k.Value.FilterSettingType).ToList();
		}
		snapshotActiveFilterSettings = sn.DoList(snapshotActiveFilterSettings);
		SearchTextFilter = (SearchTextFilterSetting)sn.DoISnapshot(SearchTextFilter);
		SearchTextActive = sn.DoBool(SearchTextActive);
		settingsAreDirty = sn.DoBool(settingsAreDirty);
		sn.Ignore(ActiveFilterSettings);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		foreach (FilterSettingType snapshotActiveFilterSetting in snapshotActiveFilterSettings)
		{
			ActiveFilterSettings.Add(snapshotActiveFilterSetting.KeyName, new FilterSetting(snapshotActiveFilterSetting));
		}
		if (SearchTextFilter != null)
		{
			SearchTextFilter.LoadPostProcess(sn);
		}
	}
}
