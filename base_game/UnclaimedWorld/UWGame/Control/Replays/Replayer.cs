using System;
using System.Collections.Generic;
using GameStateManagement;
using UWGame.ClientSide.Screens;
using UWGame.Control.Input;

namespace UWGame.Control.Replays;

public class Replayer
{
	public enum ReplayingMode
	{
		InterfaceMode,
		CommandMode,
		HybridMode
	}

	public ReplayData CurrentReplay;

	private bool isActive;

	public bool IsPlaying;

	public ReplayingMode Mode;

	private List<string> randomGetMessages = new List<string>();

	private InputManager inputManager;

	private UnclaimedWorld game;

	private Controller controller;

	private int currentAIStateIndex;

	private int currentRandomGetIndex;

	private List<string> savedAIStates = new List<string>();

	public bool IsActive => isActive;

	public Replayer(InputManager inputManager, Controller controller, UnclaimedWorld game)
	{
		this.inputManager = inputManager;
		this.game = game;
		this.controller = controller;
	}

	public void Update()
	{
		if (IsPlaying)
		{
			CurrentReplay.Update(Mode);
		}
	}

	public void AdvanceFrame()
	{
		if (IsPlaying)
		{
			CurrentReplay.currentFrameIndex++;
			if (CurrentReplay.ReplayEnded())
			{
				EndReplay();
			}
		}
	}

	public void StartReplay()
	{
		inputManager.SetToReplayMode();
		CurrentReplay.currentFrameIndex = 0;
		IsPlaying = true;
	}

	public void EndReplay()
	{
		The.InGameUI.MenuDialog.QuitToMainMenu();
		inputManager.SetToDefaultMode();
		IsPlaying = false;
		isActive = false;
		CurrentReplay.Reset();
	}

	public void Initialize()
	{
	}

	public void LoadReplay(string replayFolderPath, float? timeToPause)
	{
		string replayFilePath = replayFolderPath + "\\Replay.UWRep";
		string commandFilePath = replayFolderPath + "\\Commands.xml";
		string gameParamsPath = replayFolderPath + "\\GameParams.xml";
		_ = replayFolderPath + "\\RandomCalls.UWRepRand";
		_ = replayFolderPath + "\\AIStates.UWRepStates";
		CurrentReplay = new ReplayData(controller);
		CurrentReplay.LoadReplay(replayFolderPath, replayFilePath, commandFilePath, gameParamsPath, timeToPause);
		isActive = true;
		game.GraphicsDeviceManager.PreferredBackBufferWidth = CurrentReplay.screenWidth;
		game.GraphicsDeviceManager.PreferredBackBufferHeight = CurrentReplay.screenHeight;
		game.GraphicsDeviceManager.ApplyChanges();
		LoadingScreen.StartTransitioningToGame(game.Controller, CurrentReplay.StartGameParams, loadingIsSlow: true);
	}

	public void DummyMethod(object sender, EventArgs e)
	{
	}

	public string GetCurrentSavedAIState()
	{
		return null;
	}

	public string GetCurrentSavedRandomGet()
	{
		return null;
	}
}
