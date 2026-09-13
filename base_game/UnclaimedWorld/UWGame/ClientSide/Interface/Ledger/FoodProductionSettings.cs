using UWGame.SimSide.Snapshots;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Ledger;

public class FoodProductionSettings : SheetSettings<FoodProductionSettings.SortColumns>
{
	public enum SortColumns
	{
		Name,
		Produced,
		Consumed,
		Wasted,
		EatenByCreatures,
		Disappeared
	}

	public FoodProductionSettings()
	{
		if (!Snapshotter.IsSnapshotting)
		{
			SortingSettings = new SortingSettings<SortColumns>(SortColumns.Name, Grid.Sorting.Ascending);
		}
	}
}
