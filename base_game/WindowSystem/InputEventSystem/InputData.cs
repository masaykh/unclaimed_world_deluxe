using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace InputEventSystem;

public class InputData
{
	protected class InputKey
	{
		public Keys Key;

		public bool Pressed;

		public int Countdown;
	}

	private KeyboardState oldKeyState;

	private KeyboardState newKeyState;

	private MouseState oldMouseState;

	private MouseState newMouseState;

	private const int RepeatDelay = 500;

	private const int RepeatRate = 50;

	private List<InputKey> keys;

	private KeyDownHandler keyDown;

	private KeyboardState KeyboardState
	{
		set
		{
			oldKeyState = newKeyState;
			newKeyState = value;
		}
	}

	private MouseState MouseState
	{
		set
		{
			oldMouseState = newMouseState;
			newMouseState = value;
		}
	}

	public bool RightButtonDown => newMouseState.RightButton == ButtonState.Pressed;

	public bool LeftButtonDown => newMouseState.LeftButton == ButtonState.Pressed;

	public int mouseX => newMouseState.X;

	public int mouseY => newMouseState.Y;

	public event MouseDownHandler MouseDown;

	public event MouseUpHandler MouseUp;

	public event RequestingFocusHandler RequestingFocus;

	public event MouseMoveHandler MouseMove;

	public event MouseWheelHandler MouseWheelMove;

	private event SetMousePositionHandler mouseSet;

	public event KeyUpHandler KeyUp;

	public event KeyDownHandler KeyDown
	{
		add
		{
			keyDown = (KeyDownHandler)Delegate.Remove(keyDown, value);
			keyDown = (KeyDownHandler)Delegate.Combine(keyDown, value);
		}
		remove
		{
			keyDown = (KeyDownHandler)Delegate.Remove(keyDown, value);
		}
	}

	public void SetMousePosition(int x, int y)
	{
		this.mouseSet(x, y);
	}

	public void Reset()
	{
		this.MouseDown = null;
		this.MouseUp = null;
		this.RequestingFocus = null;
		this.MouseMove = null;
		this.KeyUp = null;
		keyDown = null;
	}

	public InputData(SetMousePositionHandler setMouseHandler)
	{
		if (setMouseHandler != null)
		{
			mouseSet += setMouseHandler.Invoke;
		}
		keys = new List<InputKey>();
		string[] names = Enum.GetNames(typeof(Keys));
		foreach (string value in names)
		{
			bool flag = false;
			foreach (InputKey key in keys)
			{
				if (key.Key == (Keys)Enum.Parse(typeof(Keys), value))
				{
					flag = true;
				}
			}
			if (!flag)
			{
				InputKey item = new InputKey
				{
					Key = (Keys)Enum.Parse(typeof(Keys), value),
					Pressed = false,
					Countdown = 500
				};
				keys.Add(item);
			}
		}
	}

	public bool WasKeyDown(Keys key)
	{
		return oldKeyState.IsKeyDown(key);
	}

	public bool IsKeyDown(Keys key)
	{
		return newKeyState.IsKeyDown(key);
	}

	public bool IsKeyUp(Keys key)
	{
		return newKeyState.IsKeyUp(key);
	}

	public bool IsKeyTapped(Keys key)
	{
		if (!oldKeyState.IsKeyDown(key))
		{
			return newKeyState.IsKeyDown(key);
		}
		return false;
	}

	public bool IsKeyReleased(Keys key)
	{
		if (oldKeyState.IsKeyDown(key))
		{
			return !newKeyState.IsKeyDown(key);
		}
		return false;
	}

	public Keys[] GetPressedKeys()
	{
		return newKeyState.GetPressedKeys();
	}

	public void UpdateNewState(KeyboardState newKeyboardState, MouseState newMouseState)
	{
		MouseState = newMouseState;
		KeyboardState = newKeyboardState;
	}

	public void UpdateKeepOldState()
	{
		oldKeyState = newKeyState;
		oldMouseState = newMouseState;
	}

	public void UpdateEvents(GameTime gameTime)
	{
		if ((newMouseState.X != oldMouseState.X || newMouseState.Y != oldMouseState.Y) && this.MouseMove != null)
		{
			MouseEventArgs e = new MouseEventArgs();
			e.State = newMouseState;
			e.Button = MouseButtons.None;
			e.Position = new Point(newMouseState.X, newMouseState.Y);
			if (this.MouseMove != null)
			{
				this.MouseMove(e);
			}
		}
		if (newMouseState.ScrollWheelValue != oldMouseState.ScrollWheelValue)
		{
			MouseEventArgs e2 = new MouseEventArgs();
			e2.State = newMouseState;
			e2.Position = new Point(newMouseState.X, newMouseState.Y);
			e2.Button = MouseButtons.None;
			if (this.RequestingFocus != null)
			{
				this.RequestingFocus(e2, TestMode.MouseWheel);
			}
			int wheelChange = newMouseState.ScrollWheelValue - oldMouseState.ScrollWheelValue;
			if (this.MouseWheelMove != null)
			{
				this.MouseWheelMove(wheelChange);
			}
		}
		if (newMouseState.LeftButton != oldMouseState.LeftButton && (this.MouseUp != null || this.MouseDown != null))
		{
			MouseEventArgs e3 = new MouseEventArgs();
			e3.State = newMouseState;
			e3.Position = new Point(newMouseState.X, newMouseState.Y);
			e3.Button = MouseButtons.Left;
			if (newMouseState.LeftButton == ButtonState.Released)
			{
				if (this.MouseUp != null)
				{
					this.MouseUp(e3);
				}
			}
			else if (this.MouseDown != null)
			{
				if (this.RequestingFocus != null)
				{
					this.RequestingFocus(e3, TestMode.Focus);
				}
				this.MouseDown(e3);
			}
		}
		if (newMouseState.RightButton != oldMouseState.RightButton && (this.MouseUp != null || this.MouseDown != null))
		{
			MouseEventArgs e4 = new MouseEventArgs();
			e4.State = newMouseState;
			e4.Position = new Point(newMouseState.X, newMouseState.Y);
			e4.Button = MouseButtons.Right;
			if (newMouseState.RightButton == ButtonState.Released)
			{
				if (this.MouseUp != null)
				{
					this.MouseUp(e4);
				}
			}
			else if (this.MouseDown != null)
			{
				if (this.RequestingFocus != null)
				{
					this.RequestingFocus(e4, TestMode.Focus);
				}
				this.MouseDown(e4);
			}
		}
		KeyEventArgs e5 = new KeyEventArgs();
		Keys[] pressedKeys = newKeyState.GetPressedKeys();
		for (int i = 0; i < pressedKeys.Length; i++)
		{
			switch (pressedKeys[i])
			{
			case Keys.LeftAlt:
			case Keys.RightAlt:
				e5.Alt = true;
				break;
			case Keys.LeftShift:
			case Keys.RightShift:
				e5.Shift = true;
				break;
			case Keys.LeftControl:
			case Keys.RightControl:
				e5.Control = true;
				break;
			}
		}
		foreach (InputKey key in keys)
		{
			if (key.Key == Keys.LeftAlt || key.Key == Keys.RightAlt || key.Key == Keys.LeftShift || key.Key == Keys.RightShift || key.Key == Keys.LeftControl || key.Key == Keys.RightControl)
			{
				continue;
			}
			bool flag = newKeyState.IsKeyDown(key.Key);
			if (flag)
			{
				key.Countdown -= gameTime.ElapsedGameTime.Milliseconds;
			}
			if (flag && !key.Pressed)
			{
				key.Pressed = true;
				e5.Key = key.Key;
				if (keyDown != null)
				{
					keyDown(e5);
				}
			}
			else if (!flag && key.Pressed)
			{
				key.Pressed = false;
				key.Countdown = 500;
				e5.Key = key.Key;
				if (this.KeyUp != null)
				{
					this.KeyUp(e5);
				}
			}
			if (key.Countdown < 0)
			{
				e5.Key = key.Key;
				if (keyDown != null)
				{
					keyDown(e5);
				}
				key.Countdown = 50;
			}
		}
	}
}
