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

	/// <summary>PORT DEVIATION 20. The recorded draw trace this replay is compared against.</summary>
	private ReplayTrace trace;

	/// <summary>Whether this replay has already been found to differ from what was recorded.</summary>
	public bool Diverged => trace != null && trace.Diverged;

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
		// PORT DEVIATION 20. A replay that ran to the end used to say nothing at all, which made
		// "it matched" and "it was never compared" the same observable outcome. The verdict is
		// written beside the replay now, so a run that proves determinism leaves evidence of it
		// rather than merely not crashing.
		trace?.Finish();
		trace = null;
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
		// Was: this path built and THROWN AWAY with a discard, which is how the trace came to be
		// missing rather than absent by design. It is read now.
		trace = new ReplayTrace();
		trace.BeginComparing(replayFolderPath);
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

	/// <summary>One random draw during a replay, for comparison against the recording.</summary>
	public void RecordDrawForComparison(string getMessage)
	{
		trace?.Draw(getMessage);
	}

	/// <summary>
	/// Closes off a replayed frame and compares it. False means this frame did not match what was
	/// recorded; Divergence.txt beside the replay says how.

	/// <summary>
	/// Hands the studio's own verification failure to the trace, so that the one case where the
	/// two checks disagree still leaves a Divergence.txt behind, and closes the trace so a verdict
	/// is written. Called just before the replay stops.
	/// </summary>
	public void ReportStateMismatchAndFinish(int frameIndex, ReplayVerificationData recorded, ReplayVerificationData live)
	{
		trace?.ReportStateMismatch(frameIndex, recorded, live);
		trace?.Finish();
	}
	/// </summary>
	public bool CompareFrame(int frameIndex, ReplayVerificationData world)
	{
		if (trace == null || !trace.HasRecording)
		{
			return true;
		}
		return trace.EndFrame(frameIndex, world);
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
