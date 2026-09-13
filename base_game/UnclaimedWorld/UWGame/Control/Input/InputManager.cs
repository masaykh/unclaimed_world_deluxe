using System;
using InputEventSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using UWGame.Control.Replays;

namespace UWGame.Control.Input;

public class InputManager
{
	public InputData InputData;

	private Replayer replayer;

	private bool isReplaying;

	private Controller controller;

	public void Update(GameTime gameTime, bool gameHasFocus)
	{
		if (!isReplaying || replayer.Mode == Replayer.ReplayingMode.CommandMode)
		{
			if (gameHasFocus)
			{
				UpdateNewStateFromInputDevices();
			}
			else
			{
				InputData.UpdateKeepOldState();
			}
		}
		else
		{
			ReplayFrame currentFrame = replayer.CurrentReplay.GetCurrentFrame();
			InputData.UpdateNewState(currentFrame.KeyboardState, currentFrame.MouseState);
		}
		InputData.UpdateEvents(gameTime);
	}

	public InputManager(Controller controller)
	{
		this.controller = controller;
	}

	public void UpdateNewStateFromInputDevices()
	{
		InputData.UpdateNewState(Keyboard.GetState(), GetZoomedMouseState(controller));
	}

	public static MouseState GetZoomedMouseState(Controller controller)
	{
		MouseState state = Mouse.GetState();
		if (controller.ZoomIsActive())
		{
			int x = (int)Math.Round((float)state.X / controller.ActiveZoomFactor);
			int y = (int)Math.Round((float)state.Y / controller.ActiveZoomFactor);
			return new MouseState(x, y, state.ScrollWheelValue, state.LeftButton, state.MiddleButton, state.RightButton, state.XButton1, state.XButton2);
		}
		return state;
	}

	public void OnMouseSetPos(int x, int y)
	{
		if (!isReplaying)
		{
			if (controller.ZoomIsActive())
			{
				x = (int)Math.Round((float)x * controller.ActiveZoomFactor);
				y = (int)Math.Round((float)y * controller.ActiveZoomFactor);
			}
			Mouse.SetPosition(x, y);
		}
	}

	public void Reset()
	{
		InputData.Reset();
	}

	public void Initialize(Replayer replayer)
	{
		this.replayer = replayer;
		InputData = new InputData(OnMouseSetPos);
		SetToDefaultMode();
	}

	public void SetToReplayMode()
	{
		isReplaying = true;
	}

	public void SetToDefaultMode()
	{
		isReplaying = false;
	}
}
