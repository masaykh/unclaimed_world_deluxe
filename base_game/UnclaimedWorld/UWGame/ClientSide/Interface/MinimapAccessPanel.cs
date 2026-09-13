using System;
using InputEventSystem;
using Microsoft.Xna.Framework;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

public class MinimapAccessPanel
{
	public Window DisplayWindow;

	protected InGameInterface intf = The.InGameUI;

	private const int height = 55;

	public ImageButton btAccessMinimap;

	private InputData frameInput;

	public MinimapAccessPanel(int screenX)
	{
	}

	private void btAccessMinimap_Click(UIComponent sender, EventArgs e)
	{
		if (The.InGameUI.Minimap.DisplayWindow.Visible)
		{
			The.InGameUI.Minimap.Hide();
		}
		else
		{
			The.InGameUI.Minimap.Show();
		}
	}

	private void CreateButtonWindow(GUIManager gui, int xPos, string sprite, int cornerSize, out Window buttonWindow)
	{
		buttonWindow = new Window(gui);
		buttonWindow.Position = new Point(xPos, The.Client.Controller.DrawArea.Height - 55);
		buttonWindow.WindowSize = new Vector2(38f, 55f);
		buttonWindow.Level = Level.Bottom;
		buttonWindow.IsMovable = false;
		buttonWindow.Resizable = false;
		buttonWindow.Margin = 0;
		buttonWindow.HasCloseButton = false;
		buttonWindow.Skin = gui.GUISpriteSheet.GetSourceRectangle(sprite);
		buttonWindow.CornerSize = cornerSize;
		buttonWindow.Show();
	}

	private void tbOverlay_MouseOut(MouseEventArgs args)
	{
		if (!The.InGameUI.HUDOverlayPanel.DisplayWindow.CheckCoordinates(frameInput.mouseX, frameInput.mouseY))
		{
			The.InGameUI.HUDOverlayPanel.Hide();
		}
	}

	private void tbOverlay_Click(UIComponent sender, EventArgs e)
	{
		if (!intf.HUDOverlayPanel.DisplayWindow.Visible)
		{
			intf.HUDOverlayPanel.ShowInScreenSpace(sender.AbsolutePosition.X, sender.AbsolutePosition.Y - intf.HUDOverlayPanel.DisplayWindow.Height);
		}
		else
		{
			intf.HUDOverlayPanel.Hide();
		}
	}
}
