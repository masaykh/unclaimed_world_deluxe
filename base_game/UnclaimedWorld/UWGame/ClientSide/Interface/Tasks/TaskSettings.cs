using UWGame.SimSide.Snapshots;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Tasks;

public class TaskSettings : ISnapshot
{
	public enum SortColumns
	{
		Priority,
		Completion,
		TaskType
	}

	public SortingSettings<SortColumns> SortingSettings;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public TaskSettings()
	{
		if (!Snapshotter.IsSnapshotting)
		{
			SortingSettings = new SortingSettings<SortColumns>(SortColumns.TaskType, Grid.Sorting.Ascending);
		}
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		SortingSettings = (SortingSettings<SortColumns>)sn.DoISnapshot(SortingSettings);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		SortingSettings.LoadPostProcess(sn);
	}
}
