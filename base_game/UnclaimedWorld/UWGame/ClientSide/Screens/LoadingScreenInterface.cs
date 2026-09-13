using System;
using GameStateManagement;
using UWGame.ClientSide.Interface;
using UWGame.ClientSide.Screens.Loading;
using UWGame.SimSide;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Scenarios;
using WindowSystem;

namespace UWGame.ClientSide.Screens;

public class LoadingScreenInterface : CommonInterface
{
	private int loadPanelWidth = 600;

	private int totalHeight = 800;

	private int left;

	private int top;

	private EventDialog dialog;

	public LoadingScreen loadingScreen;

	private StartGameParams startGameParams;

	private HintPanel hintPanel;

	public LoadingScreenInterface(LoadingScreen screen, StartGameParams startGameParams, UnclaimedWorld game)
		: base(game)
	{
		loadingScreen = screen;
		this.startGameParams = startGameParams;
		game.Controller.ValidateDrawAreaWidth(loadPanelWidth);
		left = (game.Controller.DrawArea.Width - loadPanelWidth) / 2;
		top = (game.Controller.DrawArea.Height - totalHeight) / 2;
		hintPanel = new HintPanel(this);
		hintPanel.ShowInScreenSpace((gui.ScreenWidth - hintPanel.DisplayWindow.Width) / 2, gui.ScreenHeight - hintPanel.DisplayWindow.Height - 80);
	}

	public override void LoadContent()
	{
		base.LoadContent();
		InitDialog();
		SetInterfaceCursor();
		LoadHints();
		SelectHint();
	}

	private void LoadHints()
	{
		GameData.Instance.AllHints = DataLoader.HandleOtherDataList(BaseDataLoader.InitHints, "hints.xml");
	}

	private void SelectHint()
	{
		hintPanel.ShowHint(loadingScreen.Controller);
	}

	private void InitDialog()
	{
		string text = null;
		string heading = null;
		string imageName = null;
		if (startGameParams != null && startGameParams.StartScenarioParams != null)
		{
			text = startGameParams.StartScenarioParams.GetLoadingDialogText();
			heading = startGameParams.StartScenarioParams.Scenario.ScenarioData.LoadingDialogHeading;
			imageName = startGameParams.StartScenarioParams.Scenario.ScenarioData.LoadingDialogImage;
		}
		if (text != null)
		{
			dialog = new EventDialog(this);
			dialog.ShowImageAndText(imageName, heading, text, showOkButton: true, "OK", null, null, showCancelButton: false, "CANCEL", null, null, EventDialog.Mode.OtherDialog);
			dialog.ShowInScreenSpace(200, 200);
			dialog.Window.CenterWindow();
			dialog.Window.Close += Form_Close;
			loadingScreen.WaitForUser = true;
		}
	}

	private void Form_Close(UIComponent sender)
	{
		loadingScreen.OKToStartGame();
	}

	private void loadPanel_SaveOrLoadClick(object sender, EventArgs e)
	{
	}

	public void PromptUserToContinue()
	{
		if (dialog != null)
		{
			dialog.SetButtonText(0, "CONTINUE");
		}
	}
}
