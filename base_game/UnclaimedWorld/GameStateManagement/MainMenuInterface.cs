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

	/// <summary>
	/// <summary>
	/// DEBUG MOD: the studio's own DEV OPTIONS panel, constructed.
	///
	/// MainMenuDevPanel is finished work - a TEST button and a LOAD REPLAY button, both wired to
	/// MainMenuScreen.StartTest and MainMenuScreen.LoadReplay - and the field to hold it is
	/// declared at the top of this class. Nothing ever assigned it. LOAD REPLAY is the ONLY way
	/// into the replay system: MainMenuPanel has a btLoadReplay_Click handler too and nothing
	/// subscribes to that either, so in a shipped build a recorded session could not be played
	/// back at all.
	///
	/// Built from Update rather than LoadContent, and that is the whole point of it being here:
	/// LoadContent runs once when the menu screen is created, so a switch flipped on the options
	/// screen did nothing until the menu was built again - which took starting a game and coming
	/// back out. From here it appears as soon as the options screen is closed.
	///
	/// Left of the main panel, clamped so it stays on screen on a narrow window.
	/// </summary>
	private void BuildDevPanelIfWanted()
	{
		if (devPanel != null || !UWGame.Mods.DebugMod.ShowMainMenuDevPanel)
		{
			return;
		}
		devPanel = new MainMenuDevPanel(this, new Point(System.Math.Max(8, left - 372), top));
		devPanel.Show();
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);
		BuildDevPanelIfWanted();
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
