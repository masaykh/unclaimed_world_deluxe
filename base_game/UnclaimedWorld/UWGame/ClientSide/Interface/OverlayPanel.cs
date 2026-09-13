using System;
using InputEventSystem;
using Microsoft.Xna.Framework;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

public class OverlayPanel
{
	public Window DisplayWindow;

	protected InGameInterface intf = The.InGameUI;

	private const int height = 53;

	public ImageButton btOverlay;

	public ImageButton btResource;

	public ImageButton btAccessMinimap;

	private InputData frameInput;

	public OverlayPanel(int xPos)
	{
		CreateButtonWindow(intf.gui, xPos, "scanpanel", 22, out DisplayWindow);
		frameInput = The.Client.Controller.InputData;
		int num = 10;
		int y = 16;
		btAccessMinimap = new ImageButton(intf.gui);
		btAccessMinimap.Init(ImageButtonType.Minimap);
		btAccessMinimap.Position = new Point(num, y);
		DisplayWindow.Add(btAccessMinimap);
		btAccessMinimap.Click += btAccessMinimap_Click;
		btAccessMinimap.ToolTip = "Toggle the mini-map on/off";
		btOverlay = new ImageButton(intf.gui);
		btOverlay.Init(ImageButtonType.ResourceSelectionArrow);
		btOverlay.Position = new Point(num + btAccessMinimap.Width, y);
		DisplayWindow.Add(btOverlay);
		btOverlay.Click += tbOverlay_Click;
		btOverlay.ToolTip = "Select what to display on the mini-map and the terrain view";
		btResource = new ImageButton(intf.gui);
		btResource.Init(ImageButtonType.ScanButton);
		btResource.Position = new Point(num + 2 * btAccessMinimap.Width, y);
		DisplayWindow.Add(btResource);
		btResource.Click += resourceTypeRadioButton_click;
		btResource.ToolTip = "Activate the SCAN button to highlight resources in the terrain view";
		btResource.IsChecked = The.InGameUI.OverlaySettings.ShowOverlaysOnGameArea;
		intf.HUDOverlayPanel.Refresh();
	}

	public static void CreateButtonWindow(GUIManager gui, int xPos, string sprite, int cornerSize, out Window buttonWindow)
	{
		buttonWindow = new Window(gui);
		buttonWindow.Position = new Point(xPos, The.Client.Controller.DrawArea.Height - 53);
		buttonWindow.WindowSize = new Vector2(124f, 53f);
		buttonWindow.Level = Level.Bottom;
		buttonWindow.IsMovable = false;
		buttonWindow.Resizable = false;
		buttonWindow.Margin = 0;
		buttonWindow.HasCloseButton = false;
		buttonWindow.Skin = gui.GUISpriteSheet.GetSourceRectangle(sprite);
		buttonWindow.CornerSize = cornerSize;
		buttonWindow.Show();
	}

	private void btAccessMinimap_Click(UIComponent sender, EventArgs e)
	{
		if (The.InGameUI.Minimap.DisplayWindow.IsVisibleAndActive)
		{
			The.InGameUI.Minimap.Hide();
		}
		else
		{
			The.InGameUI.Minimap.Show();
		}
	}

	private void resourceTypeRadioButton_click(UIComponent sender, EventArgs e)
	{
		The.InGameUI.OverlaySettings.ShowOverlaysOnGameArea = ((ImageButton)sender).IsChecked;
	}

	private void tbOverlay_MouseOut(MouseEventArgs args)
	{
		The.InGameUI.HUDOverlayPanel.DisplayWindow.CheckCoordinates(frameInput.mouseX, frameInput.mouseY);
	}

	private void tbOverlay_Click(UIComponent sender, EventArgs e)
	{
		if (!intf.HUDOverlayPanel.DisplayWindow.IsVisibleAndActive)
		{
			_ = (int)Common.Clamp(0.22 * (double)The.MapUI.mapWindowWidth, 200.0, 280.0);
			_ = 280;
			intf.HUDOverlayPanel.ShowInScreenSpace(sender.AbsolutePosition.X - 45, sender.AbsolutePosition.Y - intf.HUDOverlayPanel.DisplayWindow.Height - 7);
			intf.HUDOverlayPanel.Refresh();
		}
		else
		{
			intf.HUDOverlayPanel.Hide();
		}
	}
}
