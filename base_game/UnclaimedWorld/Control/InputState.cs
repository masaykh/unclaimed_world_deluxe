using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Control;

public class InputState
{
	public KeyboardState CurrentKeyboardState;

	public GamePadState CurrentGamePadState;

	public KeyboardState LastKeyboardState;

	public GamePadState LastGamePadState;

	public void Update()
	{
		LastKeyboardState = CurrentKeyboardState;
		LastGamePadState = CurrentGamePadState;
		CurrentKeyboardState = Keyboard.GetState();
		CurrentGamePadState = GamePad.GetState(PlayerIndex.One);
	}

	private bool IsNewKeyPress(Keys key)
	{
		if (CurrentKeyboardState.IsKeyDown(key))
		{
			return LastKeyboardState.IsKeyUp(key);
		}
		return false;
	}
}
