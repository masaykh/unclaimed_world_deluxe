#define TRACE
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using UWGame.Port;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UWGame;
using UWGame.ClientSide.Screens;
using UWGame.Control;

namespace GameStateManagement;

public class UnclaimedWorld : Game
{
	public GraphicsDeviceManager GraphicsDeviceManager;

	public Controller Controller;

	private const int maxFSRetries = 5;

	public static readonly Point MaxScreenDimensions = new Point(4096, 4096);

	private static readonly string dialogInstructions = "Whoops - fatal error. Press Ctrl-C to copy the contents of this dialog and paste the text into the forums: " + Environment.NewLine + Environment.NewLine;

	public Point GetScreenResolution()
	{
		return new Point(GraphicsDeviceManager.PreferredBackBufferWidth, GraphicsDeviceManager.PreferredBackBufferHeight);
	}

	public UnclaimedWorld()
	{
		Thread.CurrentThread.CurrentCulture = Config.Culture;
		GraphicsDeviceManager = new GraphicsDeviceManager(this);
		GraphicsDeviceManager.PreparingDeviceSettings += OnPreparingDeviceSettings;
		GraphicsDeviceManager.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
		GraphicsDeviceManager.PreferMultiSampling = true;
		base.IsFixedTimeStep = false;
		try
		{
			Controller = new Controller(this);
			Controller.ScreenTraceEnabled = false;
			base.Components.Add(Controller);
			GraphicsDeviceManager.SynchronizeWithVerticalRetrace = Controller.Options.SynchronizeWithVerticalRetrace;
			Controller.AddScreen(new BackgroundScreen(BackgroundScreen.Background.Normal));
			MainMenuScreen screen = new MainMenuScreen(this);
			Controller.AddScreen(screen);
			The.IngameLoadScreen = new IngameLoadGameScreen(this);
			InitTracing(enableTracing: false);
		}
		catch (Exception e)
		{
			HandleExceptionInReleaseMode(e, isMainThread: true);
		}
	}

	private void InitTracing(bool enableTracing)
	{
		if (enableTracing)
		{
			TextWriterTraceListener textWriterTraceListener = new TextWriterTraceListener("trace.log");
			Trace.Listeners.Add(textWriterTraceListener);
			Trace.AutoFlush = true;
			textWriterTraceListener.Filter = new EventTypeFilter(SourceLevels.All);
			Trace.WriteLine("InitTracing");
		}
	}

	private void OnPreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
	{
		e.GraphicsDeviceInformation.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
		e.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = 4;
		e.GraphicsDeviceInformation.GraphicsProfile = GraphicsProfile.HiDef;
	}

	protected override void Initialize()
	{
		// PORT DEVIATION 18 (see PORTING-NOTES.md). Was `base.Content.RootDirectory = "Content";`
		// on MonoGame's built-in ContentManager.
		//
		// Game.Content is replaced with UwContentManager so that EVERY load goes through the
		// port's manager, which prefers port-content\ over Content\. It has to be this one:
		// game.Content is what BloomComponent and DisplayPanelRenderer load their effects with,
		// so leaving it as the stock manager would mean the converted effects were ignored on
		// exactly the assets that need them - and that is the crash path,
		// DisplayPanelRenderer.LoadContent -> "This MGFX effect is for an older release".
		//
		// The other five construction sites already used UwContentManager (deviation 14); this
		// is the sixth and last, and the only one the game creates for itself.
		base.Content = new UWGame.Port.UwContentManager(Services)
		{
			RootDirectory = "Content"
		};
		_ = GraphicsAdapter.DefaultAdapter.SupportedDisplayModes;
		bool flag = false;
		int num = 0;
		do
		{
			try
			{
				int width;
				int height;
				if (!Controller.Options.FullScreen || Controller.Options.HardwareModeSwitch)
				{
					width = Controller.Options.ResolutionWidth;
					height = Controller.Options.ResolutionHeight;
				}
				else
				{
					width = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
					height = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
				}
				SetScreenResolution(Controller.Options.FullScreen, Controller.Options.Borderless, Controller.Options.HardwareModeSwitch, width, height);
				GraphicsDeviceManager.ApplyChanges();
				flag = false;
			}
			catch (Exception ex)
			{
				if (!ex.Message.Contains("DXGI_ERROR_NOT_CURRENTLY_AVAILABLE"))
				{
					continue;
				}
				// PORT DEVIATION 10: was Control.FromHandle(...) cast to a WinForms Form purely to
				// report its focus chain. See UWGame/Port/PlatformWindow.cs.
				string text = PlatformWindow.DescribeFocus(base.Window.Handle, base.IsActive);
				if (num == 5)
				{
					UWGame.Port.PlatformDisplay.SetHardwareModeSwitch(GraphicsDeviceManager, false);
					string title = "The game failed to start in fullscreen mode (DXGI_ERROR_NOT_CURRENTLY_AVAILABLE).";
					LogError(text + "Attempting to start without resolution mode switch.", title);
				}
				else
				{
					if (num >= 5)
					{
						string text3 = PlatformWindow.DescribeFocus(base.Window.Handle, base.IsActive);
						string text4 = text3;
						text3 = text4 + "The game failed to start in fullscreen mode (DXGI_ERROR_NOT_CURRENTLY_AVAILABLE). This may be because the game window is out of focus." + Environment.NewLine + "You can try again, or you can switch to windowed mode instead, by changing the FullScreen property to False in Options.xml (this file can be found in the Documents/Unclaimed World folder.)" + Environment.NewLine + Environment.NewLine + "If you need more help, press Ctrl-C to copy this text and then paste it into the Steam forum.";
						UWException ex2 = new UWException(text3, ex);
						ex2.IncludePasteInstructions = false;
						throw ex2;
					}
					string title2 = "DXGI_ERROR_NOT_CURRENTLY_AVAILABLE, retry: " + num + ", v. " + GetVersionAsString();
					LogError(text + GetErrorMessage(ex), title2);
					Thread.Sleep(100);
					PlatformWindow.BringToFront(base.Window.Handle);
				}
				num++;
				flag = true;
			}
		}
		while (flag);
		base.IsMouseVisible = true;

		// The studio never set this, so the title bar showed whatever MonoGame defaulted to from
		// the assembly. Named explicitly now, because this build is not their game and should not
		// present itself as though it were - see license.md section 3, which permits this name
		// and forbids anything implying an official release.
		base.Window.Title = GameName;

		base.Initialize();

#if UW_GL
		// PORT DEVIATION 19. The first point at which there IS a GL context to ask, and still
		// before anything loads an effect. A driver too small for the character shader is fatal
		// and unfixable (see GlCapabilityCheck), so say so here rather than let it crash on the
		// first animated model with a message that names neither the shader nor the reason.
		string glProblem = UWGame.Port.GlCapabilityCheck.Check();
		if (glProblem != null)
		{
			LogError(glProblem, "This graphics driver cannot run the OpenGL build");
			UWGame.Port.PlatformWindow.ShowErrorDialog(IntPtr.Zero, glProblem,
				"This graphics driver cannot run the OpenGL build");
			Exit();
		}
#endif
	}

	private void SetWindowSize(int width, int height)
	{
		int num = Common.ClampTop(width, MaxScreenDimensions.X);
		int num2 = Common.ClampTop(height, MaxScreenDimensions.Y);
		if (GraphicsDeviceManager.PreferredBackBufferWidth != num || GraphicsDeviceManager.PreferredBackBufferHeight != num2)
		{
			GraphicsDeviceManager.PreferredBackBufferWidth = num;
			GraphicsDeviceManager.PreferredBackBufferHeight = num2;
		}
	}

	public void SetScreenResolution(bool useFullscreen, bool borderLess, bool hardwareModeSwitch, int width, int height)
	{
		if (useFullscreen)
		{
			SetWindowSize(width, height);
			UWGame.Port.PlatformDisplay.SetHardwareModeSwitch(GraphicsDeviceManager, hardwareModeSwitch);
			if (!GraphicsDeviceManager.IsFullScreen)
			{
				GraphicsDeviceManager.ToggleFullScreen();
			}
			return;
		}
		SetWindowSize(width - 30, height - 50);
		// PORT DEVIATION 10: was Control.FromHandle(handle).FindForm().FormBorderStyle =
		// FormBorderStyle.None / Fixed3D. MonoGame exposes this directly and supports it on both
		// WindowsDX and DesktopGL, so this needs no platform split at all.
		UWGame.Port.PlatformDisplay.SetBorderless(base.Window, borderLess);
		if (GraphicsDeviceManager.IsFullScreen)
		{
			GraphicsDeviceManager.ToggleFullScreen();
		}
	}

	/// <summary>
	/// PORT FIX. Closes a recording when the window is closed.
	///
	/// Recorder.StopRecording had exactly one caller, Controller.GameEnded, which runs on RETURN
	/// TO MAIN MENU and on QUIT from inside the game. Closing the window - alt+F4, the X, a
	/// shutdown - reached none of it, so the draw trace's last buffer was never flushed: up to
	/// six hundred frames of it lost, and the final line liable to be cut in half.
	///
	/// Replay.UWRep and Commands.xml were never at risk; both are flushed as they are written.
	/// It is the trace that was, and a truncated trace is worse than a missing one, because it
	/// reads as a divergence at the point where the file stops.
	/// </summary>
	protected override void OnExiting(object sender, ExitingEventArgs args)
	{
		try
		{
			Controller?.GameEnded();
		}
		catch (Exception)
		{
			// Nothing here may stop the game from closing.
		}
		base.OnExiting(sender, args);
	}

	protected override void Draw(GameTime gameTime)
	{
		GraphicsDeviceManager.GraphicsDevice.Clear(Color.Black);
		base.Draw(gameTime);
	}

	public void HandleExceptionInReleaseMode(Exception e, bool isMainThread)
	{
		// PORT DEVIATION 6 (see PORTING-NOTES.md).
		// Two changes, both diagnostics-only - the crash reporter used to destroy its own
		// crash report:
		//
		//  (a) SetScreenResolution is now guarded. It reads Window.Handle, and if the game is
		//      already tearing down, WinFormsGameForm is disposed and Control.CreateHandle
		//      throws ObjectDisposedException. That escaped HandleExceptionInReleaseMode
		//      entirely, so the ORIGINAL exception `e` was never logged or shown - the process
		//      died reporting the reporter's failure instead. Observed for real on .NET 8.
		//  (b) LogError is now called BEFORE the MessageBox instead of after. The box is modal,
		//      so on an unattended run (or when it cannot be shown) nothing was ever written to
		//      Errors.txt. Logging first means the report survives regardless.
		//
		// Neither changes what the player sees when the dialog does appear.
		string title = GetTitle(e);
		string text = GetErrorMessage(e);
		LogError(text, title);

		if (isMainThread)
		{
			try
			{
				SetScreenResolution(useFullscreen: false, borderLess: false, hardwareModeSwitch: false, 1024, 768);
			}
			catch (Exception resizeFailure)
			{
				LogError("Could not restore a windowed resolution before reporting the error above: "
					+ resizeFailure.Message, title);
			}
		}
		if (isMainThread)
		{
			try
			{
				string pasteInstructions = GetPasteInstructions(e);
				PlatformWindow.ShowErrorDialog(base.Window.Handle, pasteInstructions + text, title);
			}
			catch (Exception ex)
			{
				text = text + Environment.NewLine + "----------------";
				text += Environment.NewLine;
				text = text + "MessageBox failed: " + ex.Message;
				text += Environment.NewLine;
				text += ex.StackTrace;
				// The original report is already in Errors.txt; record why the dialog failed too.
				LogError(text, title);
			}
		}
		throw e;
	}

	private static string GetPasteInstructions(Exception e)
	{
		if (e is UWException ex)
		{
			if (ex.IncludePasteInstructions)
			{
				return dialogInstructions;
			}
			return "";
		}
		return dialogInstructions;
	}

	private static string GetTitle(Exception e)
	{
		string text = "Fatal error " + GetVersionAsString();
		if (e is UWException ex)
		{
			return ex.Title ?? text;
		}
		return text;
	}

	private static string GetErrorMessage(Exception e)
	{
		int num = -1;
		StackTrace stackTrace = new StackTrace(e, fNeedFileInfo: true);
		if (stackTrace != null)
		{
			StackFrame frame = stackTrace.GetFrame(0);
			if (frame != null)
			{
				num = frame.GetFileLineNumber();
			}
		}
		string text = e.Message + Environment.NewLine + "Line: " + num + Environment.NewLine + e.StackTrace;
		if (e.InnerException != null)
		{
			text += Environment.NewLine;
			text += e.InnerException.Message;
			text += Environment.NewLine;
			text += e.InnerException.StackTrace;
		}
		return text;
	}

	public static void LogError(string message, string title)
	{
		string path = "Errors.txt";
		try
		{
			using StreamWriter streamWriter = new StreamWriter(path, append: true);
			streamWriter.WriteLine("---------------------------------------");
			streamWriter.WriteLine(title);
			streamWriter.WriteLine("Date :" + DateTime.Now.ToString());
			streamWriter.Write(message);
			streamWriter.Write(Environment.NewLine);
			streamWriter.Write(Environment.NewLine);
		}
		catch (Exception)
		{
		}
	}

	public static Version GetVersion()
	{
		return AssemblyName.GetAssemblyName(Assembly.GetExecutingAssembly().Location).Version;
	}

	/// <summary>
	/// The version as a player should read it: "1.0", not "1.0.0.0".
	/// </summary>
	/// <remarks>
	/// Version.ToString() prints every component the assembly declares, so a plain 1.0 release
	/// reads "1.0.0.0" in the fatal-error title and on the save/load screen. Trimming to
	/// major.minor is cosmetic and stops there - GetVersion() still returns the real Version, and
	/// SnapshotHeader still writes THAT into saves, so nothing that parses a version sees this.
	/// </remarks>
	public static string GetVersionAsString()
	{
		Version v = GetVersion();
		return v.Build > 0 || v.Revision > 0
			? v.ToString()
			: $"{v.Major}.{v.Minor}";
	}

	/// <summary>The name shown on the window and anywhere the game names itself.</summary>
	public const string GameName = "Unclaimed World Deluxe";
}
