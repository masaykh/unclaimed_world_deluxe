using UWGame.SimSide.Snapshots;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Ledger;

public class ProductionSettings : SheetSettings<ProductionSettings.SortColumns>
{
	public enum SortColumns
	{
		Name,
		Produced,
		Consumed,
		Wasted,
		UsedInProduction,
		Disappeared,
		Productivity
	}

	public ProductionSettings()
	{
		if (!Snapshotter.IsSnapshotting)
		{
			SortingSettings = new SortingSettings<SortColumns>(SortColumns.Name, Grid.Sorting.Ascending);
		}
	}
}
