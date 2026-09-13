using UWGame.SimSide.Snapshots;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Ledger;

public class KillsSettings : SheetSettings<KillsSettings.SortColumns>
{
	public enum SortColumns
	{
		Name,
		Kills
	}

	public KillsSettings(bool dummy)
	{
		SortingSettings = new SortingSettings<SortColumns>(SortColumns.Name, Grid.Sorting.Ascending);
	}

	public KillsSettings()
	{
		if (!Snapshotter.IsSnapshotting)
		{
			SortingSettings = new SortingSettings<SortColumns>(SortColumns.Name, Grid.Sorting.Ascending);
		}
	}
}
