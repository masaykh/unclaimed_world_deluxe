using System.Reflection;
using Microsoft.Xna.Framework;
using UWGame;
using UWGame.ClientSide.Interface;
using UWGame.ClientSide.Interface.HUD_Windows;
using WindowSystem;

namespace GameStateManagement;

public class MainMenuInterface : CommonInterface
{
	private int left;

	private int top;

	private new Tooltip Tooltip;

	private MainMenuPanel panel;

	private MainMenuDevPanel devPanel;

	private TimelinePanel timeline;

	public MainMenuScreen mainMenuScreen;

	public MainMenuInterface(MainMenuScreen mainMenuScreen, UnclaimedWorld game)
		: base(game)
	{
		this.mainMenuScreen = mainMenuScreen;
		left = game.Controller.DrawArea.Width / 2 - 180;
		top = game.Controller.DrawArea.Height - 240;
		Tooltip = new Tooltip(this);
	}

	public override void LoadContent()
	{
		base.LoadContent();
		panel = new MainMenuPanel(this, new Point(left, top));
		panel.Show();
		Window window = new Window(gui);
		window.X = 100;
		window.Y = 100;
		Image image = new Image(gui);
		window.Add(image);
		image.SetSkinLocation(SkinState.Normal, gui.GUISpriteSheet.GetSourceRectangle("Main_logo"));
		image.ResizeControlToFitImage();
		window.Width = image.Width;
		window.Height = image.Height;
		window.Show();
		timeline = new TimelinePanel(this);
		int d = panel.Window.Right + 280;
		d = Common.ClampTop(d, gui.ScreenWidth - 360);
		timeline.DisplayWindow.X = d;
		timeline.DisplayWindow.Y = window.Y + 100;
		Window window2 = new Window(gui);
		window2.IsMovable = false;
		Image image2 = new Image(gui);
		window2.Add(image2);
		image2.SetSkinLocation(SkinState.Normal, gui.GUISpriteSheet.GetSourceRectangle("Norden"));
		image2.ResizeControlToFitImage();
		Label label = new Label(gui);
		window2.Add(label);
		label.Init(Label.LabelType.HUDWindow);
		label.Text = "© 2012-16 Refactored Games OÜ, all rights reserved. Supported by: ";
		label.FitToText();
		window2.Width = gui.ScreenWidth;
		window2.Height = image2.Height;
		window2.X = 0;
		window2.Y = gui.ScreenHeight - window2.Height - 12;
		image2.X = window2.Width - image2.Width - 12;
		label.X = image2.X - label.Width - 12;
		label.Y = window2.Height - label.Height - 4;
		Label label2 = new Label(gui);
		window2.Add(label2);
		label2.Init(Label.LabelType.HUDWindow);
		label2.Text = UnclaimedWorld.GetVersionAsString();
		label2.FitToText();
		window2.CenterChildHorizontally(label2);
		label2.Y = window2.Height - label2.Height - 4;
		Label label3 = new Label(gui);
		window2.Add(label3);
		label3.Init(Label.LabelType.HUDWindow);
		label3.NormalColor = UIComponent.errorColor;
		label3.X = 40;
		label3.Y = window2.Height - label3.Height - 4;
		if (!mainMenuScreen.Controller.SteamManager.IsInitialized)
		{
			label3.Text = "Warning: Steam has not initialized correctly. Achievements cannot be unlocked in this session!";
			label3.FitToText();
			label3.Visible = true;
		}
		else
		{
			label3.Visible = false;
		}
		window2.Show();
		SetInterfaceCursor();
	}

	public void ShowSteamWarning()
	{
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);
		Tooltip.Update(gameTime);
	}

	public override void Destroy()
	{
		base.Destroy();
		Tooltip.Destroy();
	}

	public static int GetBuild()
	{
		return AssemblyName.GetAssemblyName(Assembly.GetExecutingAssembly().Location).Version.Build;
	}

	public static int GetRevision()
	{
		return AssemblyName.GetAssemblyName(Assembly.GetExecutingAssembly().Location).Version.Revision;
	}

	public static int GetMajor()
	{
		return AssemblyName.GetAssemblyName(Assembly.GetExecutingAssembly().Location).Version.Major;
	}

	public static int GetMinor()
	{
		return AssemblyName.GetAssemblyName(Assembly.GetExecutingAssembly().Location).Version.Minor;
	}
}
