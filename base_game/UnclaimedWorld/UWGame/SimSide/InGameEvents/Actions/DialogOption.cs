using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.InGameEvents.Actions;

public class DialogOption : ISnapshot
{
	public string Text;

	public string Tooltip;

	public int? ButtonWidth;

	public string ActionSet;

	public bool ActiveInArchive;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		Text = sn.DoString(Text);
		Tooltip = sn.DoString(Tooltip);
		ButtonWidth = sn.DoInt32Nullable(ButtonWidth);
		ActionSet = sn.DoString(ActionSet);
		ActiveInArchive = sn.DoBool(ActiveInArchive);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}
}
