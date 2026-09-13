using System;
using UWGame.ClientSide.GameEvents;
using UWGame.ClientSide.Screens;
using UWGame.Control;
using WindowSystem;

namespace UWGame.SimSide.InGameEvents.Actions;

public class LoseGameAction : EventActionType, IGameData
{
	public DynamicText ModalDialogText;

	public string ModalDialogImage;

	public string LoseScreenText;

	private Controller screenManager;

	public LoseGameAction(string keyName)
		: base(keyName)
	{
	}

	public LoseGameAction()
	{
	}

	public override bool Execute(EventAction action, ref string failReason)
	{
		screenManager = The.Sim.Controller;
		The.Client.PauseGame();
		The.Sim.IsGameOver = true;
		if (The.Client != null)
		{
			if (ModalDialogText != null && !string.IsNullOrEmpty(ModalDialogText.Text))
			{
				EventActionDialog.ShowAndSaveEventDialog(ModalDialogImage, "", ModalDialogText.GetSubstitutedText(action), modal: true).Window.Close += DisplayWindow_Close;
			}
			else
			{
				GoToLoseGameScreen();
			}
		}
		else
		{
			GoToLoseGameScreen();
		}
		return true;
	}

	private void DisplayWindow_Close(UIComponent sender)
	{
		GoToLoseGameScreen();
	}

	private void GoToLoseGameScreen()
	{
		LoadingScreen.StartTransition(screenManager, LoadLoseGameScreen, loadingIsSlow: false);
	}

	private void LoadLoseGameScreen(object sender, EventArgs e)
	{
		LoadLoseGameScreen();
	}

	private void LoadLoseGameScreen()
	{
		GameScreen[] screens = screenManager.GetScreens();
		for (int i = 0; i < screens.Length; i++)
		{
			screens[i].ExitScreen();
		}
		screenManager.AddScreen(new BackgroundScreen(BackgroundScreen.Background.BlueTint));
		screenManager.AddScreen(new LoseGameScreen(screenManager, LoseScreenText));
	}
}
