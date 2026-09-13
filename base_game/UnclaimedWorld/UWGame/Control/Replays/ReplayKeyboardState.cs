using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace UWGame.Control.Replays;

public class ReplayKeyboardState
{
	public Dictionary<Keys, bool> KeyStates;

	public ReplayKeyboardState()
	{
		KeyStates = new Dictionary<Keys, bool>();
	}

	public bool IsKeyDown(Keys keyToCheck)
	{
		if (!KeyStates.TryGetValue(keyToCheck, out var value))
		{
			return false;
		}
		return value;
	}
}
