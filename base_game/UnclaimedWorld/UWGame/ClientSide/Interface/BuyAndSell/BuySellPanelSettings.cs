using System.Collections.Generic;
using System.Linq;
using UWGame.ClientSide.Interface.Inventory;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;
using WindowSystem;

namespace UWGame.ClientSide.Interface.BuyAndSell;

public class BuySellPanelSettings : ISnapshot
{
	public enum SortColumns
	{
		Name,
		Amount,
		Price,
		Bulk,
		OfferDemand
	}

	public FilterPropertySettings FilterPropertySettings;

	public SortingSettings<SortColumns> SortingSettings;

	public const SortColumns DefaultSortColumn = SortColumns.Amount;

	public const Grid.Sorting DefaultSortOrder = Grid.Sorting.Descending;

	public ViewType viewType = ViewType.List;

	public bool IsExpanded;

	private HashSet<EntityType> baseData;

	private HashSet<EntityType> staticData;

	private bool settingsAreDirty = true;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public BuySellPanelSettings()
	{
		if (!Snapshotter.IsSnapshotting)
		{
			FilterPropertySettings = new FilterPropertySettings();
			SortingSettings = new SortingSettings<SortColumns>(SortColumns.Amount, Grid.Sorting.Descending);
		}
	}

	public HashSet<EntityType> GetData()
	{
		if (FilterPropertySettings.SettingsAreDirty)
		{
			HashSet<EntityType> filteredEntities = GetFilteredEntities();
			FilterPropertySettings.SetSettingsNotDirty();
			staticData = filteredEntities;
		}
		return staticData;
	}

	public static bool FilterTradeItems(EntityType entityType)
	{
		entityType.KeyName.Contains("hauling");
		if (entityType.TerrainType == null && entityType.TreeType == null && entityType.StructureType == null && entityType.Category != null)
		{
			return true;
		}
		return false;
	}

	public static HashSet<EntityType> GetBaseData(ref HashSet<EntityType> cachedBaseData)
	{
		if (cachedBaseData == null)
		{
			cachedBaseData = new HashSet<EntityType>();
			foreach (KeyValuePair<string, EntityType> allEntityType in GameData.Instance.AllEntityTypes)
			{
				if (FilterTradeItems(allEntityType.Value))
				{
					cachedBaseData.Add(allEntityType.Value);
				}
			}
		}
		return cachedBaseData;
	}

	private HashSet<EntityType> GetFilteredEntities()
	{
		HashSet<EntityType> result = new HashSet<EntityType>();
		if (FilterPropertySettings.HasActiveFilters())
		{
			staticData = new HashSet<EntityType>();
			List<HashSet<EntityType>> filteredEntities = FilterPropertySettings.GetFilteredEntities(FilterTradeItems);
			if (filteredEntities.Count > 0)
			{
				result = filteredEntities.Aggregate((HashSet<EntityType> previousList, HashSet<EntityType> nextList) => previousList.Union(nextList).ToHashSet());
			}
		}
		else
		{
			result = GetBaseData(ref baseData);
		}
		return result;
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		IsExpanded = sn.DoBool(IsExpanded);
		SortingSettings = (SortingSettings<SortColumns>)sn.DoISnapshot(SortingSettings);
		FilterPropertySettings = (FilterPropertySettings)sn.DoISnapshot(FilterPropertySettings);
		viewType = sn.DoEnum(viewType);
		settingsAreDirty = sn.DoBool(settingsAreDirty);
		staticData = sn.DoHashSet(staticData);
		sn.Ignore(baseData);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		SortingSettings.LoadPostProcess(sn);
		FilterPropertySettings.LoadPostProcess(sn);
	}
}
