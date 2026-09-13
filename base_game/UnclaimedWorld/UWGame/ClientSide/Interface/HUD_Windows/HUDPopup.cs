using Microsoft.Xna.Framework;
using WindowSystem;

namespace UWGame.ClientSide.Interface.HUD_Windows;

public abstract class HUDPopup
{
	public Window DisplayWindow;

	protected int windowHeight = 80;

	protected int windowWidth = 200;

	protected GUIManager gui;

	protected Game game;

	public HUDPopup(int width, int height)
	{
		windowWidth = width;
		windowHeight = height;
		gui = The.InGameUI.gui;
		game = The.Sim.Controller.Game;
		DisplayWindow = new Window(gui);
		DisplayWindow.Skin = gui.GUISpriteSheet.GetSourceRectangle("HUD_window_base");
		DisplayWindow.Opacity = 0.75f;
		DisplayWindow.CornerSize = 47;
		DisplayWindow.Margin = 0;
		DisplayWindow.Level = Level.Bottom;
		DisplayWindow.Resizable = false;
		DisplayWindow.IsMovable = true;
		DisplayWindow.Position = new Point(0, The.Client.Controller.DrawArea.Height - windowHeight);
		DisplayWindow.WindowSize = new Vector2(windowWidth, windowHeight);
		DisplayWindow.HasCloseButton = true;
		DisplayWindow.HasCRTOrLCDComponents = false;
		DisplayWindow.HasOverlayComponents = false;
		DisplayWindow.Hide();
	}

	protected void Add(UIComponent control)
	{
		DisplayWindow.Add(control);
	}
}
