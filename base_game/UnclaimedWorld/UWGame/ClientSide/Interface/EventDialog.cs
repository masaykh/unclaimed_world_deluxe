using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.SimSide;
using UWGame.SimSide.InGameEvents.Actions;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

public class EventDialog : Panel
{
	public enum Mode
	{
		NewEvent,
		OtherDialog
	}

	public class ButtonEventArgs : EventArgs
	{
		public string Key;

		public int Index;
	}

	private TextArea area;

	private Image image;

	private const int width = 786;

	private const int height = 308;

	private Box display;

	private Box crtPlasticEdge;

	private LCDScreen lcdScreen;

	private UIComponent lcdSurface;

	private CRTScreen crtScreen;

	private Grid surfaceGrid;

	private const int titleHeight = 30;

	private DialogOption[] dialogOptions;

	private const int maxNoOfButtons = 4;

	private TextButton[] buttons = new TextButton[4];

	private List<TextButton> buttonsOnForm = new List<TextButton>();

	private Mode mode;

	private const int leftButtonXPos = 451;

	private const int rightButtonEdge = 756;

	public string Text
	{
		set
		{
			area.Text = value;
		}
	}

	public string Image
	{
		set
		{
			image.SetSkinLocation(SkinState.Normal, Interface.gui.GUISpriteSheet.GetSourceRectangle(value));
			image.ResizeControlToFitImage();
		}
	}

	public event Action<string, int> ButtonClicked;

	public EventDialog(CommonInterface intf)
		: base(intf, null, Point.Zero, new Vector2(786f, 308f), Level.EventDialog, PanelType.EventDialog)
	{
		FullLCDPanel.AddLCDPanel(intf, Window, new Point(14, 46), 465, 215, out display, out lcdSurface, ref lcdScreen);
		CreateSurfaceWithScrollbar(out surfaceGrid, lcdSurface, canHaveFocus: false);
		area = new TextArea(intf.gui, ListBoxType.LCD);
		area.RenderType = RenderType.CRTAndLCD;
		area.Init(Label.LabelType.LCDNormal);
		area.CanGrowInHeight = true;
		surfaceGrid.AddEntry(area, area);
		area.X = 6;
		area.Y = 45;
		area.Width = surfaceGrid.SurfaceWidth - 12;
		Window.Level = Level.EventDialog;
		int crtWidth = 279;
		int crtHeight = 216;
		image = new Image(intf.gui);
		image.RenderType = RenderType.CRTAndLCD;
		image.ScaleImageToSizeOfControl = true;
		image.Width = crtWidth;
		image.Height = crtHeight;
		image.X = 496;
		image.Y = 33;
		StatusScreen.AddCRTPlasticFrame(intf.gui, Window, new Point(image.X, image.Y), crtWidth, crtHeight, out crtPlasticEdge);
		crtScreen = intf.DisplayPanelRenderer.AddCRT(image, new Point(image.AbsolutePosition.X, image.AbsolutePosition.Y), crtWidth, crtHeight, Window.Level, Window, ReflectionToUse.Small, isMonochrome: true);
		for (int i = 0; i < 4; i++)
		{
			TextButton textButton = new TextButton(intf.gui);
			textButton.Init(TextButton.TextButtonType.White);
			textButton.Y = crtPlasticEdge.Bottom + 9;
			buttons[i] = textButton;
			textButton.Click += button_Click;
			textButton.EventArgs = new ButtonEventArgs
			{
				Key = null,
				Index = i
			};
		}
		Panel.AddDirtOnIrregularTopEdge(Interface.gui, Window);
		AddDirtOnBottomEdge(Interface.gui, Window);
	}

	private void AddDirtOnBottomEdge(GUIManager gui, Window Form)
	{
		Rectangle sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_bottom");
		Panel.AddImage(gui, Form, sourceRectangle, new Point(445, Form.Height - sourceRectangle.Height)).RenderType = RenderType.Overlay;
	}

	private bool IsNewestEvent()
	{
		if (The.Client.CurrentEventDialogIndex == The.Client.EventDialogsData.Count - 1)
		{
			return true;
		}
		return false;
	}

	private void button_Click(UIComponent sender, EventArgs e)
	{
		ButtonEventArgs e2 = e as ButtonEventArgs;
		if (this.ButtonClicked != null)
		{
			this.ButtonClicked(e2.Key, e2.Index);
		}
		if (dialogOptions != null)
		{
			DialogOption dialogOption = dialogOptions[e2.Index];
			if (GameData.Instance.AllActionSets.TryGetValue(dialogOption.ActionSet, out var value))
			{
				value.Fire(null, null, null, out var _);
			}
		}
		Hide();
	}

	private static void AddButton(Window Form, List<TextButton> buttonsOnForm, TextButton okButton)
	{
		Form.Add(okButton);
		buttonsOnForm.Add(okButton);
	}

	public static void ArrangeButtons(List<TextButton> buttonsOnForm, TextButton[] buttons, int leftXPos, int rightEdge)
	{
		int count = buttonsOnForm.Count;
		if (count <= 0)
		{
			return;
		}
		TextButton textButton = buttonsOnForm[0];
		textButton.X = leftXPos;
		if (count <= 1)
		{
			return;
		}
		TextButton textButton2 = buttons[count - 1];
		textButton2.X = rightEdge - textButton2.Width;
		if (count > 2)
		{
			int num = buttonsOnForm.Sum((TextButton bt) => bt.Width);
			int num2 = (textButton2.Right - textButton.X - num) / (count - 1);
			int right = buttons[0].Right;
			for (int num3 = 1; num3 < buttonsOnForm.Count - 1; num3++)
			{
				TextButton textButton3 = buttonsOnForm[num3];
				textButton3.X = right + num2;
				right = textButton3.Right;
			}
		}
	}

	public void SetButtonText(int index, string text)
	{
		buttons[index].Text = text;
		buttons[index].ScaleWidthToFitText();
	}

	public void ShowImageAndText(string imageName, string heading, string text, bool showOkButton = true, string okButtonText = "OK", string okButtonTooltip = null, int? okButtonWidth = null, bool showCancelButton = false, string cancelButtonText = "CANCEL", string cancelButtonTooltip = null, int? cancelButtonWidth = null, Mode mode = Mode.NewEvent)
	{
		FillImageAndText(imageName, heading, text);
		dialogOptions = null;
		this.mode = mode;
		RemoveButtons();
		if (showOkButton)
		{
			TextButton button = buttons[0];
			SetupButton(Window, buttonsOnForm, okButtonText, okButtonTooltip, okButtonWidth, button);
		}
		if (showCancelButton)
		{
			TextButton button2 = buttons[1];
			SetupButton(Window, buttonsOnForm, cancelButtonText, cancelButtonTooltip, cancelButtonWidth, button2);
		}
		ArrangeButtons(buttonsOnForm, buttons, 451, 756);
	}

	public void ShowImageAndText(string imageName, string heading, string text, DialogOption[] dialogButtons, Mode mode = Mode.NewEvent)
	{
		FillImageAndText(imageName, heading, text);
		dialogOptions = dialogButtons;
		this.mode = mode;
		if (this.ButtonClicked != null)
		{
			Delegate[] invocationList = this.ButtonClicked.GetInvocationList();
			foreach (Delegate obj in invocationList)
			{
				ButtonClicked -= (Action<string, int>)obj;
			}
		}
		SetupButtons(Window, dialogButtons, buttonsOnForm, buttons);
		ArrangeButtons(buttonsOnForm, buttons, 451, 756);
	}

	public static void SetupButtons(Window Form, DialogOption[] dialogButtons, List<TextButton> buttonsOnForm, TextButton[] buttons)
	{
		buttonsOnForm.Clear();
		for (int i = 0; i < dialogButtons.Length; i++)
		{
			DialogOption dialogOption = dialogButtons[i];
			TextButton textButton = buttons[i];
			textButton.Text = dialogOption.Text;
			textButton.ToolTip = dialogOption.Tooltip;
			if (dialogOption.ButtonWidth.HasValue)
			{
				textButton.Width = dialogOption.ButtonWidth.Value;
			}
			else
			{
				textButton.ScaleWidthToFitText();
			}
			Form.Add(textButton);
			buttonsOnForm.Add(textButton);
		}
		for (int j = dialogButtons.Length; j < buttons.Length; j++)
		{
			Form.Remove(buttons[j]);
		}
	}

	private void FillImageAndText(string imageName, string heading, string text)
	{
		if (imageName != null)
		{
			Window.Add(image);
			image.SetSkinLocation(SkinState.Normal, Interface.gui.GUI_CRT_SpriteSheet.GetSourceRectangle(imageName));
		}
		else
		{
			Window.Add(image);
		}
		image.Texture = Interface.gui.GUI_CRT_SpriteSheet.Texture;
		surfaceGrid.BeginAddingEntries();
		lblTitle.Text = (heading ?? "").ToUpper(Config.Culture);
		lblTitle.FitToText();
		area.Text = text;
		surfaceGrid.EndAddingEntries();
	}

	public static void SetupButton(Window form, List<TextButton> buttonsOnForm, string text, string tooltip, int? width, TextButton button)
	{
		AddButton(form, buttonsOnForm, button);
		if (text != null)
		{
			button.Text = text;
		}
		button.ToolTip = tooltip;
		if (width.HasValue)
		{
			button.Width = width.Value;
		}
		else
		{
			button.ScaleWidthToFitText();
		}
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

	public override void Hide()
	{
		base.Hide();
	}
}
