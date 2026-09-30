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
