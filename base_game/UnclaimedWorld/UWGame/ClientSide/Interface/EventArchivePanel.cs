using System;
using System.Collections.Generic;
using UWGame.SimSide;
using UWGame.SimSide.InGameEvents.Actions;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

public class EventArchivePanel : RosterPanel
{
	private Grid surfaceGrid;

	private TextArea area;

	private DialogOption[] dialogOptions;

	private const int maxNoOfButtons = 4;

	private TextButton[] buttons = new TextButton[4];

	private ImageButton previousButton;

	private ImageButton nextButton;

	private List<TextButton> buttonsOnForm = new List<TextButton>();

	private const int itemHeight = 36;

	private const int horizPadding = 6;

	private const int vertPadding = 4;

	private const int leftButtonXPos = 20;

	private const int rightButtonEdge = 312;

	public EventArchivePanel()
		: base("EVENT ARCHIVE", 487, 486)
	{
		HasStatusCRT = true;
		CreateSurfaceWithScrollbar(out surfaceGrid, lcdSurface, canHaveFocus: false);
		area = new TextArea(Interface.gui, ListBoxType.LCD);
		area.RenderType = RenderType.CRTAndLCD;
		area.Init(Label.LabelType.LCDNormal);
		area.CanGrowInHeight = true;
		surfaceGrid.AddEntry(area, area);
		area.X = 6;
		area.Y = 45;
		area.Width = surfaceGrid.SurfaceWidth - 12;
		previousButton = new ImageButton(Interface.gui);
		previousButton.InitWithIcon(ImageButtonType.White, "arrowblack_left", hasCheckedState: false);
		previousButton.ToolTip = "Previous event";
		previousButton.Click += previousButton_Click;
		Window.Add(previousButton);
		PlaceButtonUnderLCD(previousButton, 370);
		nextButton = new ImageButton(Interface.gui);
		nextButton.InitWithIcon(ImageButtonType.White, "arrowblack_right", hasCheckedState: false);
		nextButton.ToolTip = "Next event";
		nextButton.Click += nextButton_Click;
		Window.Add(nextButton);
		PlaceButtonUnderLCD(nextButton, previousButton.Right + 4);
		for (int i = 0; i < 4; i++)
		{
			TextButton textButton = new TextButton(Interface.gui);
			textButton.Init(TextButton.TextButtonType.White);
			buttons[i] = textButton;
			textButton.Click += button_Click;
			textButton.EventArgs = new EventDialog.ButtonEventArgs
			{
				Key = null,
				Index = i
			};
			PlaceButtonUnderLCD(textButton);
		}
		InitStatusContentPanel();
	}

	private void InitStatusContentPanel()
	{
		statusContent = The.InGameUI.StatusScreen.GetNewSurfaceContent();
		InitStatusImage(statusContent.Width, statusContent.Height);
		statusContent.Remove(pnBillboards);
	}

	private void nextButton_Click(UIComponent sender, EventArgs e)
	{
		if (The.Client.EventDialogsData.Count > 0)
		{
			int num = Common.Min(The.Client.EventDialogsData.Count - 1, The.Client.CurrentEventDialogIndex + 1);
			if (num != The.Client.CurrentEventDialogIndex)
			{
				DisplayNextOrPreviousEvent(num);
			}
		}
	}

	private void btClose_Click(UIComponent sender, EventArgs e)
	{
		Hide();
	}

	private void previousButton_Click(UIComponent sender, EventArgs e)
	{
		if (The.Client.EventDialogsData.Count > 0)
		{
			int num = Common.Max(0, The.Client.CurrentEventDialogIndex - 1);
			if (num != The.Client.CurrentEventDialogIndex)
			{
				DisplayNextOrPreviousEvent(num);
			}
		}
	}

	private void DisplayNextOrPreviousEvent(int newIndex)
	{
		The.Client.CurrentEventDialogIndex = newIndex;
		EventDialogData eventDialogData = The.Client.EventDialogsData[The.Client.CurrentEventDialogIndex];
		if (eventDialogData.DialogOptions != null)
		{
			ShowImageAndText(eventDialogData.DisplayImage, eventDialogData.Header, eventDialogData.DisplayText, eventDialogData.DialogOptions);
		}
		else
		{
			ShowImageAndText(eventDialogData.DisplayImage, eventDialogData.Header, eventDialogData.DisplayText);
		}
	}

	private void ShowImageAndText(string imageName, string heading, string text, bool showOkButton = true, string okButtonText = "OK", string okButtonTooltip = null, int? okButtonWidth = null, bool showCancelButton = false, string cancelButtonText = "CANCEL", string cancelButtonTooltip = null, int? cancelButtonWidth = null)
	{
		FillImageAndText(imageName, heading, text);
		dialogOptions = null;
		RemoveButtons();
		if (showOkButton)
		{
			TextButton button = buttons[0];
			EventDialog.SetupButton(Window, buttonsOnForm, okButtonText, okButtonTooltip, okButtonWidth, button);
		}
		if (showCancelButton)
		{
			TextButton button2 = buttons[1];
			EventDialog.SetupButton(Window, buttonsOnForm, cancelButtonText, cancelButtonTooltip, cancelButtonWidth, button2);
		}
		EventDialog.ArrangeButtons(buttonsOnForm, buttons, 20, 312);
	}

	private void ShowImageAndText(string imageName, string heading, string text, DialogOption[] dialogButtons)
	{
		FillImageAndText(imageName, heading, text);
		dialogOptions = dialogButtons;
		EventDialog.SetupButtons(Window, dialogButtons, buttonsOnForm, buttons);
		EventDialog.ArrangeButtons(buttonsOnForm, buttons, 20, 312);
	}

	private void FillImageAndText(string imageName, string heading, string text)
	{
		if (imageName != null)
		{
			imStatusBackground.SetSkinLocation(SkinState.Normal, Interface.gui.GUI_CRT_SpriteSheet.GetSourceRectangle(imageName));
		}
		else
		{
			statusContent.Remove(imStatusBackground);
		}
		imStatusBackground.Texture = Interface.gui.GUI_CRT_SpriteSheet.Texture;
		lblTitle.Text = (heading ?? "").ToUpper(Config.Culture);
		lblTitle.FitToText();
		surfaceGrid.BeginAddingEntries();
		area.Text = text;
		surfaceGrid.EndAddingEntries();
	}

	private void RemoveButtons()
	{
		buttonsOnForm.Clear();
		TextButton[] array = buttons;
		foreach (TextButton control in array)
		{
			Window.Remove(control);
		}
	}

	public override void Show()
	{
		if (The.Client.EventDialogsData.Count > 0)
		{
			The.Client.CurrentEventDialogIndex = The.Client.EventDialogsData.Count - 1;
			EventDialogData eventDialogData = The.Client.EventDialogsData[The.Client.CurrentEventDialogIndex];
			if (eventDialogData.DialogOptions != null)
			{
				ShowImageAndText(eventDialogData.DisplayImage, eventDialogData.Header, eventDialogData.DisplayText, eventDialogData.DialogOptions);
			}
			else
			{
				ShowImageAndText(eventDialogData.DisplayImage, eventDialogData.Header, eventDialogData.DisplayText);
			}
		}
		base.Show();
	}

	private void button_Click(UIComponent sender, EventArgs e)
	{
		EventDialog.ButtonEventArgs e2 = e as EventDialog.ButtonEventArgs;
		if (dialogOptions != null)
		{
			DialogOption dialogOption = dialogOptions[e2.Index];
			if (dialogOption.ActiveInArchive && GameData.Instance.AllActionSets.TryGetValue(dialogOption.ActionSet, out var value))
			{
				value.Fire(null, null, null, out var _);
			}
		}
		The.InGameUI.CloseRosterPanel();
	}
}
