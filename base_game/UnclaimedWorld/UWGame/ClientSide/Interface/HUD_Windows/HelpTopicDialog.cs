using System;
using UWGame.ClientSide.HelpTopics;
using UWGame.ClientSide.Interface.Layout;
using WindowSystem;

namespace UWGame.ClientSide.Interface.HUD_Windows;

public class HelpTopicDialog : HUDWindow
{
	private const int minWidth = 420;

	private const int minHeight = 400;

	private UIComponent listSurface;

	private Grid surfaceGrid;

	private const int surfaceHeight = 354;

	private const int borderWidth = 2;

	public HelpTopicDialog(HelpTopic helpTopic)
		: base(420, 400, hasSurface: true, hasCloseButton: true, isMovable: true)
	{
		DisplayWindow.SetResizableArea(ResizeAreas.Top, isResizable: true);
		DisplayWindow.SetResizableArea(ResizeAreas.Bottom, isResizable: true);
		DisplayWindow.MinHeight = 40;
		DisplayWindow.ResizableBorderSize = 6;
		DisplayWindow.Resize += DisplayWindow_Resize;
		HideOnRightClick = false;
		CreateSurfaceWithScrollbar(out surfaceGrid, out listSurface, 354, base.TitleBarHeight, 12, canHaveFocus: false);
		DisplayWindow.Level = Level.Dialogs;
		Populate(helpTopic.FlowElements);
		SetVerticalPositions();
	}

	private void DisplayWindow_Resize(UIComponent sender)
	{
		SetVerticalPositions();
	}

	private void SetVerticalPositions()
	{
		listSurface.Height = DisplayWindow.ViewPort.Height - listSurface.Y - 10;
		surfaceGrid.Height = listSurface.Height;
	}

	private void bt_Click(UIComponent sender, EventArgs e)
	{
		Hide();
	}

	public void Populate(LayoutElement[] flowLayoutElements)
	{
		surfaceGrid.BeginAddingEntries();
		surfaceGrid.Clear();
		foreach (LayoutElement layoutElement in flowLayoutElements)
		{
			if (layoutElement.Text != null)
			{
				TextArea textArea = new TextArea(gui, ListBoxType.HUDAndLCD);
				textArea.Init(Label.LabelType.HUDWindow);
				textArea.CanGrowInHeight = true;
				surfaceGrid.AddEntry(textArea, textArea);
				textArea.Text = layoutElement.Text.Text;
			}
			else if (layoutElement.Image != null)
			{
				UIComponent uIComponent = new UIComponent(gui);
				Image image = new Image(gui);
				uIComponent.Add(image);
				image.X = 2;
				image.Y = 2;
				image.CanHaveFocus = false;
				image.SetSkinLocation(SkinState.Normal, gui.GUISpriteSheet.GetSourceRectangle(layoutElement.Image.Image));
				image.Texture = gui.GUISpriteSheet.Texture;
				image.ResizeControlToFitImage();
				Box box = new Box(gui);
				box.SetSkinLocation(0, gui.GUISpriteSheet.GetSourceRectangle("HUD_border"));
				box.CornerSize = 3;
				uIComponent.Add(box);
				box.Width = image.Width + 4;
				box.Height = image.Height + 4;
				uIComponent.Width = box.Width;
				uIComponent.Height = box.Height;
				surfaceGrid.AddEntry(image, uIComponent);
				surfaceGrid.CenterChildHorizontallyInViewport(uIComponent);
				uIComponent.X -= listSurface.X / 2;
			}
		}
		surfaceGrid.EndAddingEntries();
		foreach (UIComponent entry in surfaceGrid.Entries)
		{
			if (entry is TextArea textArea2)
			{
				textArea2.RefreshText();
			}
		}
	}

	public override void Hide()
	{
		DisplayWindow.Hide();
	}
}
