using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace UWGame.Control.Replays;

public class ReplayFrame
{
	public GameTime GameTime;

	public KeyboardState KeyboardState;

	public MouseState MouseState;

	public ReplayVerificationData RecordedVerificationData = new ReplayVerificationData();

	private int replayFormatVersion;

	private static Dictionary<Keys, bool> keyStates;

	private List<Keys> changedKeys = new List<Keys>();

	static ReplayFrame()
	{
		keyStates = new Dictionary<Keys, bool>();
		foreach (Keys value in Enum.GetValues(typeof(Keys)))
		{
			keyStates.Add(value, value: false);
		}
	}

	public ReplayFrame()
	{
	}

	public ReplayFrame(GameTime gameTime, int mouseX, int mouseY, bool leftButtonDown, bool rightButtonDown, List<Keys> changedKeys)
	{
		GameTime = gameTime;
		InitMouseState(mouseX, mouseY, leftButtonDown, rightButtonDown);
		this.changedKeys = changedKeys;
	}

	public void LoadFrame(BinaryFileReader replayReader)
	{
		LoadTime(replayReader);
		LoadMouseState(replayReader);
		LoadKeyboardState(replayReader);
		RecordedVerificationData.Load(replayReader);
	}

	private void LoadKeyboardState(BinaryFileReader replayReader)
	{
		List<Keys> list = new List<Keys>();
		while (replayReader.ReadBool())
		{
			Keys key = (Keys)replayReader.ReadInt();
			keyStates[key] = !keyStates[key];
		}
		foreach (KeyValuePair<Keys, bool> keyState in keyStates)
		{
			if (keyState.Value)
			{
				list.Add(keyState.Key);
			}
		}
		KeyboardState = new KeyboardState(list.ToArray());
	}

	private void LoadMouseState(BinaryFileReader replayReader)
	{
		int mouseX = replayReader.ReadInt();
		int mouseY = replayReader.ReadInt();
		bool leftButtonDown = replayReader.ReadBool();
		bool rightButtonDown = replayReader.ReadBool();
		InitMouseState(mouseX, mouseY, leftButtonDown, rightButtonDown);
	}

	public void InitMouseState(int mouseX, int mouseY, bool leftButtonDown, bool rightButtonDown)
	{
		int scrollWheel = 0;
		ButtonState middleButton = ButtonState.Released;
		ButtonState xButton = ButtonState.Released;
		ButtonState xButton2 = ButtonState.Released;
		ButtonState leftButton = (leftButtonDown ? ButtonState.Pressed : ButtonState.Released);
		ButtonState rightButton = (rightButtonDown ? ButtonState.Pressed : ButtonState.Released);
		MouseState = new MouseState(mouseX, mouseY, scrollWheel, leftButton, middleButton, rightButton, xButton, xButton2);
	}

	private void LoadTime(BinaryFileReader replayReader)
	{
		TimeSpan totalGameTime = new TimeSpan(replayReader.ReadLong());
		TimeSpan elapsedGameTime = new TimeSpan(replayReader.ReadLong());
		GameTime = new GameTime(totalGameTime, elapsedGameTime);
	}
}
