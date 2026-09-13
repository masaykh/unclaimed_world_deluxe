using System;
using System.IO;
using System.IO.Compression;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UWGame.Client.MainMenu.LoadSavedGame;
using UWGame.ClientSide.Screens;
using UWGame.Control;
using UWGame.SimSide.Snapshots;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

public class InGameMenuDialog : Panel
{
	private OptionsDialog optionsDialog;

	private SaveLoadGamePanel saveDialog;

	private SaveLoadGamePanel loadDialog;

	public InGameMenuDialog()
		: base(The.InGameUI, "MENU", new Point(400, 280), new Vector2(260f, 290f), Level.Menu, PanelType.RegularEdges)
	{
		int num = 180;
		int x = 16 + (Window.Width - 32 - num) / 2;
		int num2 = 6;
		TextButton textButton = new TextButton(Interface.gui);
		Window.Add(textButton);
		textButton.Init(TextButton.TextButtonType.White);
		textButton.Position = new Point(x, MarginTop + 5);
		textButton.Text = "RESUME GAME";
		textButton.Click += btContinueGame_Click;
		textButton.Width = num;
		TextButton textButton2 = new TextButton(Interface.gui);
		Window.Add(textButton2);
		textButton2.Init(TextButton.TextButtonType.White);
		textButton2.Position = new Point(x, textButton.Bottom + num2);
		textButton2.Text = "QUIT TO TITLE SCREEN";
		textButton2.Click += btQuitToMainMenu_Click;
		textButton2.Width = num;
		TextButton textButton3 = new TextButton(Interface.gui);
		Window.Add(textButton3);
		textButton3.Init(TextButton.TextButtonType.White);
		textButton3.Position = new Point(x, textButton2.Bottom + num2);
		textButton3.Text = "QUIT TO DESKTOP";
		textButton3.Click += btQuitToDesktop_Click;
		textButton3.Width = num;
		TextButton textButton4 = new TextButton(Interface.gui);
		Window.Add(textButton4);
		textButton4.Init(TextButton.TextButtonType.White);
		textButton4.Position = new Point(x, textButton3.Bottom + num2);
		textButton4.Text = "OPTIONS";
		textButton4.Click += btOptions_Click;
		textButton4.Width = num;
		TextButton textButton5 = new TextButton(Interface.gui);
		Window.Add(textButton5);
		textButton5.Init(TextButton.TextButtonType.White);
		textButton5.Position = new Point(x, textButton4.Bottom + num2 + num2);
		textButton5.Text = "SAVE GAME";
		textButton5.Click += btSaveGame_Click;
		textButton5.Width = num;
		TextButton textButton6 = new TextButton(Interface.gui);
		Window.Add(textButton6);
		textButton6.Init(TextButton.TextButtonType.White);
		textButton6.Position = new Point(x, textButton5.Bottom + num2);
		textButton6.Text = "LOAD GAME";
		textButton6.Click += btLoadGame_Click;
		textButton6.Width = num;
		AddDefaultDirt();
		Window.Height = textButton6.Bottom + 6 + MarginBottom;
		optionsDialog = new OptionsDialog(The.InGameUI);
		optionsDialog.CancelClick += optionsDialog_CancelClick;
		optionsDialog.OKClick += optionsDialog_OKClick;
		saveDialog = new SaveLoadGamePanel(SaveLoadGamePanel.SaveOrLoad.Save, Interface, new Point(300, 0));
		saveDialog.SaveOrLoadClick += saveDialog_SaveClick;
		saveDialog.CancelClick += saveloadDialog_CancelClick;
		loadDialog = new SaveLoadGamePanel(SaveLoadGamePanel.SaveOrLoad.Load, Interface, Point.Zero);
		loadDialog.SaveOrLoadClick += loadDialog_LoadClick;
		loadDialog.CancelClick += saveloadDialog_CancelClick;
	}

	private void optionsDialog_OKClick(object sender, EventArgs e)
	{
		HandleChildWindowClose();
	}

	private void optionsDialog_CancelClick(object sender, EventArgs e)
	{
		HandleChildWindowClose();
	}

	private void saveloadDialog_CancelClick(object sender, EventArgs e)
	{
		HandleChildWindowClose();
	}

	private static void HandleChildWindowClose()
	{
		The.InGameUI.ShowInGameMenu();
	}

	private void loadDialog_LoadClick(object sender, EventArgs e)
	{
		LoadSavedGameWithPossibleRedirect(loadDialog.SelectedSaveGamePath);
	}

	private void LoadSavedGameWithPossibleRedirect(string savegamePath)
	{
		SnapshotHeader snapshotHeader;
		using (BinaryReader reader = new BinaryReader(new BufferedStream(new GZipStream(File.Open(savegamePath, FileMode.Open), CompressionMode.Decompress), 65536)))
		{
			snapshotHeader = The.Snapshotter.LoadHeader(reader);
		}
		// The direct path keeps the data tables that are already in memory - which is what makes it
		// fast, and what makes it wrong when the save needs different ones. The tables were built
		// once when this session started; a save that names a modded recipe cannot be resolved
		// against tables built without it. So a difference in modded content redirects through the
		// loading screen exactly the way a different scenario does, and that path rebuilds them.
		bool sameMods = string.Equals(snapshotHeader.Mods ?? "",
			UWGame.Mods.ModSettings.EffectiveSignature, StringComparison.Ordinal);
		if (The.Sim.StartGameParams.IsSameScenario(snapshotHeader.StartGameParams) && sameMods)
		{
			SaveLoadMessageBox.LoadGameDirectlyInGame(savegamePath, isLoadAfterSave: false);
		}
		else
		{
			LoadSavedGameScreen.LoadSavedGameWithLoadingScreen(The.Sim.Controller, savegamePath, snapshotHeader);
		}
	}

	private void saveDialog_SaveClick(object sender, EventArgs e)
	{
		The.InGameUI.SaveLoadMessageBox.StartSave(saveDialog.SelectedSaveGamePath);
	}

	public override void DrawContent(Window sender, SpriteBatch formSpriteBatch)
	{
		base.DrawContent(sender, formSpriteBatch);
	}

	private void InitButtons()
	{
	}

	private void btQuitToDesktop_Click(UIComponent sender, EventArgs e)
	{
		The.Sim.Controller.Game.Exit();
	}

	private void btOptions_Click(UIComponent sender, EventArgs e)
	{
		The.InGameUI.HideInGameMenu();
		optionsDialog.ShowDialog(modal: true);
	}

	private void btSaveGame_Click(UIComponent sender, EventArgs e)
	{
		The.InGameUI.HideInGameMenu();
		saveDialog.ShowDialog(modal: true);
	}

	private void btLoadGame_Click(UIComponent sender, EventArgs e)
	{
		The.InGameUI.HideInGameMenu();
		loadDialog.ShowDialog(modal: true);
	}

	private void btQuitToMainMenu_Click(UIComponent sender, EventArgs e)
	{
		QuitToMainMenu();
	}

	public void QuitToMainMenu()
	{
		LoadingScreen.StartTransition(The.Sim.Controller, LoadMainMenuScreen, loadingIsSlow: false);
	}

	private void LoadMainMenuScreen(object sender, EventArgs e)
	{
		LoadingScreen loadingScreen = sender as LoadingScreen;
		GameScreen[] screens = loadingScreen.Controller.GetScreens();
		for (int i = 0; i < screens.Length; i++)
		{
			screens[i].ExitScreen();
		}
		loadingScreen.Controller.AddScreen(new BackgroundScreen(BackgroundScreen.Background.Normal));
		loadingScreen.Controller.AddScreen(new MainMenuScreen(loadingScreen.Controller.Game));
	}

	private void btContinueGame_Click(UIComponent sender, EventArgs e)
	{
		The.InGameUI.HideInGameMenu();
	}

	public override void ShowDialog(bool modal)
	{
		base.ShowDialog(modal);
	}
}
