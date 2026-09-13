using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Kensei.Dev;

public static class Command
{
	public delegate void CommandFunction(string[] arguments);

	private struct CommandEntry
	{
		internal readonly CommandFunction m_function;

		internal readonly string m_help;

		internal CommandEntry(CommandFunction commandFunction, string help)
		{
			m_function = commandFunction;
			m_help = help;
		}
	}

	// PORT DEVIATION 8 (see PORTING-NOTES.md).
	// The 133-line `private class CommandForm : Form` that used to live here is replaced by
	// this inert stub, and DoModelessDialog below no longer constructs it.
	//
	// Unlike the Options panel, this one WAS reachable in the shipped game, and badly so:
	// Manager.Update runs every frame from Client.cs, calls Command.Update(keyboard), and
	// s_activateKey is Keys.OemPipe - so pressing "\" in a retail build opened a Win32
	// console window over the game. Worse, s_lastKeyboard is assigned once and never updated,
	// so the edge-detect `IsKeyDown(key) && !s_lastKeyboard.IsKeyDown(key)` is permanently
	// true: HOLDING "\" disposed and re-constructed a Form every single frame. And the window
	// was useless anyway - Command.Initialise, which registers help/cls/exit/collectGarbage,
	// has zero callers, so every command answered "Error: unknown command".
	//
	// Removing it fixes that retail defect and takes the last WinForms dependency out of
	// Kensei.Dev. The public API is unchanged, so no call site moves.
	private sealed class CommandForm
	{
		internal void Print(string message) { }

		internal void SetInput(string input) { }

		internal void Cls() { }

		internal void Show() { }

		internal void Dispose() { }
	}

	private class InsensitiveComparer : IEqualityComparer<string>
	{
		private CaseInsensitiveComparer comparer = new CaseInsensitiveComparer();

		public int GetHashCode(string str)
		{
			return str.ToLowerInvariant().GetHashCode();
		}

		public bool Equals(string lhs, string rhs)
		{
			return comparer.Compare(lhs, rhs) == 0;
		}
	}

	private static Dictionary<string, CommandEntry> s_commands = new Dictionary<string, CommandEntry>(new InsensitiveComparer());

	private static bool s_active;

	private static Microsoft.Xna.Framework.Input.Keys s_activateKey = Microsoft.Xna.Framework.Input.Keys.OemPipe;

	private static CommandForm s_form;

	private static KeyboardState s_lastKeyboard = default(KeyboardState);

	private static string s_currentCommandLine;

	private static List<string> s_output = new List<string>();

	private static List<string> s_commandHistory = new List<string>();

	private static int s_doskey = -1;

	private static bool s_doskeyActive;

	private static Microsoft.Xna.Framework.Color s_backgroundColour = Microsoft.Xna.Framework.Color.Crimson;

	private static Microsoft.Xna.Framework.Color s_activeTextColour = Microsoft.Xna.Framework.Color.White;

	private static Microsoft.Xna.Framework.Color s_inactiveTextColour = Microsoft.Xna.Framework.Color.Yellow;

	private static float s_screenProportion = 0.5f;

	public static bool Active
	{
		get
		{
			return s_active;
		}
		private set
		{
			s_active = value;
		}
	}

	public static Microsoft.Xna.Framework.Input.Keys ActivationKey
	{
		get
		{
			return s_activateKey;
		}
		set
		{
			s_activateKey = value;
		}
	}

	public static Microsoft.Xna.Framework.Color BackgroundColour
	{
		get
		{
			return s_backgroundColour;
		}
		set
		{
			s_backgroundColour = value;
		}
	}

	public static Microsoft.Xna.Framework.Color ActiveTextColour
	{
		get
		{
			return s_activeTextColour;
		}
		set
		{
			s_activeTextColour = value;
		}
	}

	public static Microsoft.Xna.Framework.Color InactiveTextColour
	{
		get
		{
			return s_inactiveTextColour;
		}
		set
		{
			s_inactiveTextColour = value;
		}
	}

	public static float ScreenProportion
	{
		get
		{
			return s_screenProportion;
		}
		set
		{
			s_screenProportion = MathHelper.Clamp(value, 0f, 1f);
		}
	}

	internal static void Initialise()
	{
		AddCommand("help", HelpDelegate, "\"help\" to see a list of commands, or \"help <command>\" to see help for that command.");
		AddCommand("cls", ClsDelegate, "Clears the console output.");
		AddCommand("clearHistory", ClearDoskeyDelegate, "Clears the command history.");
		AddCommand("exit", ExitDelegate, "Closes the command console and returns to game (can also press Esc).");
		AddCommand("collectGarbage", CollectGarbageDelegate, "Forcibly collects garbage.");
		AddCommand("command", DialogBoxDelegate, "Brings up a dialog box offering more powerful access to the command prompt system.");
	}

	internal static void Update(KeyboardState keyboard)
	{
		if (keyboard.IsKeyDown(s_activateKey) && !s_lastKeyboard.IsKeyDown(s_activateKey))
		{
			DoModelessDialog();
		}
	}

	internal static void PreDraw(float screenWidth, float screenHeight)
	{
		if (Active)
		{
			float y = screenHeight * ScreenProportion;
			Shape.Box(Vector2.Zero, new Vector2(screenWidth, y), BackgroundColour, solid: true);
		}
	}

	internal static void Draw(float screenWidth, float screenHeight)
	{
		if (!Active)
		{
			return;
		}
		Microsoft.Xna.Framework.Rectangle rectangle = new Microsoft.Xna.Framework.Rectangle(0, 0, (int)screenWidth, (int)(screenHeight * ScreenProportion));
		float num = Manager.SpriteFont.MeasureString("> " + s_currentCommandLine).Y;
		int num2;
		for (num2 = s_output.Count - 1; num2 >= 0; num2--)
		{
			num += Manager.SpriteFont.MeasureString(s_output[num2]).Y;
			if (num >= (float)rectangle.Height)
			{
				num2++;
				break;
			}
		}
		Vector2 position = new Vector2(rectangle.Left, rectangle.Top);
		Manager.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
		for (num2 = (int)MathHelper.Max(num2, 0); num2 >= 0 && num2 < s_output.Count; num2++)
		{
			Manager.SpriteBatch.DrawString(Manager.SpriteFont, s_output[num2], position, InactiveTextColour);
			position.Y += Manager.SpriteFont.MeasureString(s_output[num2]).Y;
		}
		Manager.SpriteBatch.DrawString(Manager.SpriteFont, "> " + s_currentCommandLine, position, ActiveTextColour);
		Manager.SpriteBatch.End();
	}

	public static void Print(string message)
	{
		s_output.Add(message);
		if (s_form != null)
		{
			s_form.Print(message);
		}
	}

	public static bool AddCommand(string commandName, CommandFunction commandFunction, string helpText)
	{
		if (!commandName.Contains(" "))
		{
			if (!s_commands.ContainsKey(commandName))
			{
				try
				{
					s_commands.Add(commandName, new CommandEntry(commandFunction, helpText));
					return true;
				}
				catch (ArgumentException)
				{
					Print("Error: couldn't register command \"" + commandName + "\".");
				}
			}
			else
			{
				Print("Error: command \"" + commandName + "\" already registered.");
			}
		}
		else
		{
			Print("Error: command \"" + commandName + "\" contains spaces.");
		}
		return false;
	}

	private static void ProcessInput(KeyboardState keyboard)
	{
		Microsoft.Xna.Framework.Input.Keys[] pressedKeys = keyboard.GetPressedKeys();
		foreach (Microsoft.Xna.Framework.Input.Keys keys in pressedKeys)
		{
			if (s_lastKeyboard.IsKeyDown(keys))
			{
				continue;
			}
			if (IsSpecialKey(keys))
			{
				switch (keys)
				{
				case Microsoft.Xna.Framework.Input.Keys.Escape:
					s_currentCommandLine = "";
					break;
				case Microsoft.Xna.Framework.Input.Keys.Enter:
					ProcessCurrentCommand();
					break;
				case Microsoft.Xna.Framework.Input.Keys.Up:
					DoskeyDecrement();
					break;
				case Microsoft.Xna.Framework.Input.Keys.Down:
					DoskeyIncrement();
					break;
				case Microsoft.Xna.Framework.Input.Keys.Back:
					if (s_currentCommandLine.Length > 0)
					{
						s_currentCommandLine = s_currentCommandLine.Remove(s_currentCommandLine.Length - 1, 1);
					}
					break;
				case Microsoft.Xna.Framework.Input.Keys.NumPad0:
				case Microsoft.Xna.Framework.Input.Keys.NumPad1:
				case Microsoft.Xna.Framework.Input.Keys.NumPad2:
				case Microsoft.Xna.Framework.Input.Keys.NumPad3:
				case Microsoft.Xna.Framework.Input.Keys.NumPad4:
				case Microsoft.Xna.Framework.Input.Keys.NumPad5:
				case Microsoft.Xna.Framework.Input.Keys.NumPad6:
				case Microsoft.Xna.Framework.Input.Keys.NumPad7:
				case Microsoft.Xna.Framework.Input.Keys.NumPad8:
				case Microsoft.Xna.Framework.Input.Keys.NumPad9:
					s_currentCommandLine += KeyToChar(keys, keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift) || keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightShift));
					break;
				}
			}
			else
			{
				s_currentCommandLine += KeyToChar(keys, keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift) || keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightShift));
			}
		}
	}

	private static void ProcessCurrentCommand()
	{
		ProcessCommand(s_currentCommandLine);
		s_currentCommandLine = "";
	}

	private static void ProcessCommand(string command)
	{
		command = command.Trim();
		string[] array = command.Split();
		if (array.Length != 0 && command.Length > 0)
		{
			if (s_commandHistory.Count == 0 || command != s_commandHistory[s_commandHistory.Count - 1])
			{
				s_commandHistory.Add(command);
			}
			if (!s_doskeyActive)
			{
				s_doskey = -1;
			}
			Print("> " + command);
			if (s_commands.TryGetValue(array[0], out var value))
			{
				value.m_function(array);
			}
			else
			{
				Print("Error: unknown command \"" + array[0] + "\".");
			}
			s_doskeyActive = false;
		}
	}

	private static void DoskeyIncrement()
	{
		if (s_doskey != -1)
		{
			s_doskey++;
			s_doskey = (int)MathHelper.Min(s_commandHistory.Count - 1, s_doskey);
		}
		if (s_doskey >= 0 && s_doskey < s_commandHistory.Count)
		{
			s_currentCommandLine = s_commandHistory[s_doskey];
			if (s_form != null)
			{
				s_form.SetInput(s_currentCommandLine);
			}
		}
		s_doskeyActive = true;
	}

	private static void DoskeyDecrement()
	{
		if (s_doskey == -1)
		{
			s_doskey = s_commandHistory.Count - 1;
		}
		else if (s_doskeyActive)
		{
			s_doskey = (int)MathHelper.Max(s_doskey - 1, 0);
		}
		if (s_doskey >= 0 && s_doskey < s_commandHistory.Count)
		{
			s_currentCommandLine = s_commandHistory[s_doskey];
			if (s_form != null)
			{
				s_form.SetInput(s_currentCommandLine);
			}
		}
		s_doskeyActive = true;
	}

	public static char KeyToChar(Microsoft.Xna.Framework.Input.Keys key, bool shiftPressed)
	{
		switch (key)
		{
		case Microsoft.Xna.Framework.Input.Keys.A:
			if (!shiftPressed)
			{
				return 'a';
			}
			return 'A';
		case Microsoft.Xna.Framework.Input.Keys.B:
			if (!shiftPressed)
			{
				return 'b';
			}
			return 'B';
		case Microsoft.Xna.Framework.Input.Keys.C:
			if (!shiftPressed)
			{
				return 'c';
			}
			return 'C';
		case Microsoft.Xna.Framework.Input.Keys.D:
			if (!shiftPressed)
			{
				return 'd';
			}
			return 'D';
		case Microsoft.Xna.Framework.Input.Keys.E:
			if (!shiftPressed)
			{
				return 'e';
			}
			return 'E';
		case Microsoft.Xna.Framework.Input.Keys.F:
			if (!shiftPressed)
			{
				return 'f';
			}
			return 'F';
		case Microsoft.Xna.Framework.Input.Keys.G:
			if (!shiftPressed)
			{
				return 'g';
			}
			return 'G';
		case Microsoft.Xna.Framework.Input.Keys.H:
			if (!shiftPressed)
			{
				return 'h';
			}
			return 'H';
		case Microsoft.Xna.Framework.Input.Keys.I:
			if (!shiftPressed)
			{
				return 'i';
			}
			return 'I';
		case Microsoft.Xna.Framework.Input.Keys.J:
			if (!shiftPressed)
			{
				return 'j';
			}
			return 'J';
		case Microsoft.Xna.Framework.Input.Keys.K:
			if (!shiftPressed)
			{
				return 'k';
			}
			return 'K';
		case Microsoft.Xna.Framework.Input.Keys.L:
			if (!shiftPressed)
			{
				return 'l';
			}
			return 'L';
		case Microsoft.Xna.Framework.Input.Keys.M:
			if (!shiftPressed)
			{
				return 'm';
			}
			return 'M';
		case Microsoft.Xna.Framework.Input.Keys.N:
			if (!shiftPressed)
			{
				return 'n';
			}
			return 'N';
		case Microsoft.Xna.Framework.Input.Keys.O:
			if (!shiftPressed)
			{
				return 'o';
			}
			return 'O';
		case Microsoft.Xna.Framework.Input.Keys.P:
			if (!shiftPressed)
			{
				return 'p';
			}
			return 'P';
		case Microsoft.Xna.Framework.Input.Keys.Q:
			if (!shiftPressed)
			{
				return 'q';
			}
			return 'Q';
		case Microsoft.Xna.Framework.Input.Keys.R:
			if (!shiftPressed)
			{
				return 'r';
			}
			return 'R';
		case Microsoft.Xna.Framework.Input.Keys.S:
			if (!shiftPressed)
			{
				return 's';
			}
			return 'S';
		case Microsoft.Xna.Framework.Input.Keys.T:
			if (!shiftPressed)
			{
				return 't';
			}
			return 'T';
		case Microsoft.Xna.Framework.Input.Keys.U:
			if (!shiftPressed)
			{
				return 'u';
			}
			return 'U';
		case Microsoft.Xna.Framework.Input.Keys.V:
			if (!shiftPressed)
			{
				return 'v';
			}
			return 'V';
		case Microsoft.Xna.Framework.Input.Keys.W:
			if (!shiftPressed)
			{
				return 'w';
			}
			return 'W';
		case Microsoft.Xna.Framework.Input.Keys.X:
			if (!shiftPressed)
			{
				return 'x';
			}
			return 'X';
		case Microsoft.Xna.Framework.Input.Keys.Y:
			if (!shiftPressed)
			{
				return 'y';
			}
			return 'Y';
		case Microsoft.Xna.Framework.Input.Keys.Z:
			if (!shiftPressed)
			{
				return 'z';
			}
			return 'Z';
		case Microsoft.Xna.Framework.Input.Keys.D0:
			return '0';
		case Microsoft.Xna.Framework.Input.Keys.D1:
			return '1';
		case Microsoft.Xna.Framework.Input.Keys.D2:
			return '2';
		case Microsoft.Xna.Framework.Input.Keys.D3:
			return '3';
		case Microsoft.Xna.Framework.Input.Keys.D4:
			return '4';
		case Microsoft.Xna.Framework.Input.Keys.D5:
			return '5';
		case Microsoft.Xna.Framework.Input.Keys.D6:
			return '6';
		case Microsoft.Xna.Framework.Input.Keys.D7:
			return '7';
		case Microsoft.Xna.Framework.Input.Keys.D8:
			return '8';
		case Microsoft.Xna.Framework.Input.Keys.D9:
			return '9';
		case Microsoft.Xna.Framework.Input.Keys.NumPad0:
			return '0';
		case Microsoft.Xna.Framework.Input.Keys.NumPad1:
			return '1';
		case Microsoft.Xna.Framework.Input.Keys.NumPad2:
			return '2';
		case Microsoft.Xna.Framework.Input.Keys.NumPad3:
			return '3';
		case Microsoft.Xna.Framework.Input.Keys.NumPad4:
			return '4';
		case Microsoft.Xna.Framework.Input.Keys.NumPad5:
			return '5';
		case Microsoft.Xna.Framework.Input.Keys.NumPad6:
			return '6';
		case Microsoft.Xna.Framework.Input.Keys.NumPad7:
			return '7';
		case Microsoft.Xna.Framework.Input.Keys.NumPad8:
			return '8';
		case Microsoft.Xna.Framework.Input.Keys.NumPad9:
			return '9';
		default:
			return ' ';
		}
	}

	public static bool IsSpecialKey(Microsoft.Xna.Framework.Input.Keys key)
	{
		if (key == Microsoft.Xna.Framework.Input.Keys.Escape || key == Microsoft.Xna.Framework.Input.Keys.Enter || key == Microsoft.Xna.Framework.Input.Keys.Up || key == Microsoft.Xna.Framework.Input.Keys.Down || key == Microsoft.Xna.Framework.Input.Keys.Back)
		{
			return true;
		}
		return false;
	}

	private static void HelpDelegate(string[] commandArguments)
	{
		if (commandArguments.Length > 1)
		{
			if (s_commands.TryGetValue(commandArguments[1], out var value))
			{
				Print(commandArguments[1] + ": " + value.m_help);
			}
			else
			{
				Print("Error: unknown command \"" + commandArguments[1] + "\".");
			}
			return;
		}
		Print("Type a command, then press Enter, or press Esc to exit.\nUse \"help <command>\" to get help for a specific command.\nCommands available:");
		List<string> list = new List<string>();
		foreach (string key in s_commands.Keys)
		{
			list.Add(key);
		}
		list.Sort();
		foreach (string item in list)
		{
			Print("    " + item);
		}
	}

	private static void ClsDelegate(string[] commandArguments)
	{
		s_output.Clear();
		if (s_form != null)
		{
			s_form.Cls();
		}
	}

	private static void ClearDoskeyDelegate(string[] commandArguments)
	{
		s_commandHistory.Clear();
		s_doskeyActive = false;
		s_doskey = -1;
	}

	private static void ExitDelegate(string[] commandArguments)
	{
		s_active = false;
		if (s_form != null)
		{
			s_form.Dispose();
			s_form = null;
		}
	}

	private static void CollectGarbageDelegate(string[] commandArguments)
	{
		GC.Collect();
	}

	private static void DialogBoxDelegate(string[] commandArguments)
	{
		DoModelessDialog();
	}

	// Was: dispose any existing CommandForm, construct a new one, seed it with help output and
	// Show() it. Now a no-op so s_form stays null - see PORT DEVIATION 8. This is what the
	// "\" key and the "command" console command used to reach.
	public static void DoModelessDialog()
	{
	}
}
