using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.Snapshots;

namespace UWGame.ClientSide.Interface;

public class EventDialogData : ISnapshot
{
	public string DisplayText;

	public string Header;

	public string DisplayImage;

	public DialogOption[] DialogOptions;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public EventDialogData(string substitutedText, string header, string displayImage, DialogOption[] dialogOptions)
	{
		DisplayText = substitutedText;
		Header = header;
		DisplayImage = displayImage;
		DialogOptions = dialogOptions;
	}

	public EventDialogData()
	{
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		DisplayText = sn.DoString(DisplayText);
		Header = sn.DoString(Header);
		DisplayImage = sn.DoString(DisplayImage);
		DialogOptions = sn.DoArray(DialogOptions);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		if (DialogOptions != null)
		{
			DialogOption[] dialogOptions = DialogOptions;
			for (int i = 0; i < dialogOptions.Length; i++)
			{
				dialogOptions[i].LoadPostProcess(sn);
			}
		}
	}
}
