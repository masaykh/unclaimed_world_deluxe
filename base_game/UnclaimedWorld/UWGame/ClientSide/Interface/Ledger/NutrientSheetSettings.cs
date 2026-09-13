using UWGame.SimSide.Snapshots;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Ledger;

public class NutrientSheetSettings : SheetSettings<NutrientSheetSettings.SortColumns>
{
	public enum SortColumns
	{
		Name,
		Produced,
		Consumed,
		Overconsumed,
		Stored,
		DaysLeft
	}

	public NutrientSheetSettings()
	{
		if (!Snapshotter.IsSnapshotting)
		{
			SortingSettings = new SortingSettings<SortColumns>(SortColumns.Name, Grid.Sorting.Ascending);
		}
	}
}
