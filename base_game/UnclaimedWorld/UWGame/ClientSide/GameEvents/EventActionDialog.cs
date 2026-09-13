using UWGame.ClientSide.Interface;
using UWGame.SimSide.InGameEvents.Actions;

namespace UWGame.ClientSide.GameEvents;

public class EventActionDialog : EventActionType
{
	public string Heading;

	public DynamicText DisplayText;

	public string DisplayImage;

	public DialogOption[] DialogOptions;

	public EventActionDialog(string keyName)
		: base(keyName)
	{
	}

	public EventActionDialog()
	{
	}

	public override bool Execute(EventAction action, ref string failReason)
	{
		string substitutedText = DisplayText.GetSubstitutedText(action);
		if (DialogOptions != null)
		{
			ShowAndSaveEventDialog(DisplayImage, Heading, substitutedText, modal: true, DialogOptions);
		}
		else
		{
			ShowAndSaveEventDialog(DisplayImage, Heading, substitutedText, modal: true);
		}
		return true;
	}

	public static EventDialog ShowAndSaveEventDialog(string displayImage, string heading, string displayText, bool modal, bool showOkButton = true, string okButtonText = "OK", string okButtonTooltip = null, int? okButtonWidth = null, bool showCancelButton = false, string cancelButtonText = "CANCEL", string cancelButtonTooltip = null, int? cancelButtonWidth = null)
	{
		The.Client.EventDialogsData.Add(new EventDialogData(displayText, heading, displayImage, null));
		return ShowEventDialog(modal, showOkButton, okButtonText, okButtonTooltip, okButtonWidth, showCancelButton, cancelButtonText, cancelButtonText, cancelButtonWidth);
	}

	public static EventDialog ShowAndSaveEventDialog(string displayImage, string heading, string displayText, bool modal, DialogOption[] dialogOptions)
	{
		The.Client.EventDialogsData.Add(new EventDialogData(displayText, heading, displayImage, dialogOptions));
		return ShowEventDialog();
	}

	public static EventDialog ShowEventDialog(bool modal = true, bool showOkButton = true, string okButtonText = "OK", string okButtonTooltip = null, int? okButtonWidth = null, bool showCancelButton = false, string cancelButtonText = "CANCEL", string cancelButtonTooltip = null, int? cancelButtonWidth = null, EventDialog.Mode mode = EventDialog.Mode.NewEvent)
	{
		EventDialog eventDialog = The.InGameUI.EventDialog;
		The.Client.CurrentEventDialogIndex = The.Client.EventDialogsData.Count - 1;
		EventDialogData eventDialogData = The.Client.EventDialogsData[The.Client.CurrentEventDialogIndex];
		if (eventDialogData.DialogOptions != null)
		{
			eventDialog.ShowImageAndText(eventDialogData.DisplayImage, eventDialogData.Header, eventDialogData.DisplayText, eventDialogData.DialogOptions, mode);
		}
		else
		{
			eventDialog.ShowImageAndText(eventDialogData.DisplayImage, eventDialogData.Header, eventDialogData.DisplayText, showOkButton, okButtonText, okButtonTooltip, okButtonWidth, showCancelButton, cancelButtonText, cancelButtonTooltip, cancelButtonWidth, mode);
		}
		if (modal)
		{
			The.Client.SetModal(value: true);
		}
		eventDialog.ShowInScreenSpace(200, 200, modal);
		eventDialog.Window.CenterWindow();
		return eventDialog;
	}
}
