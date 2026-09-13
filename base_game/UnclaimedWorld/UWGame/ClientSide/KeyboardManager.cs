using Microsoft.Xna.Framework.Input;

namespace UWGame.ClientSide;

public class KeyboardManager
{
	private KeyboardState oldKeyState;

	private KeyboardState newKeyState;

	public KeyboardState KeyboardState => newKeyState;

	public void Update()
	{
		oldKeyState = newKeyState;
		newKeyState = Keyboard.GetState();
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

	public bool IsKeyDown(Keys key)
	{
		return newKeyState.IsKeyDown(key);
	}
}
