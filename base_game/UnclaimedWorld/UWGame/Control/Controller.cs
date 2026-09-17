#define TRACE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime;
using System.Threading;
using GameStateManagement;
using InputEventSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using UWGame.Client.Audio;
using UWGame.ClientSide;
using UWGame.ClientSide.Screens;
using UWGame.Control.Commands;
using UWGame.Control.Input;
using UWGame.Control.Replays;
using UWGame.SimSide;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Scenarios;
using UWGame.SimSide.Snapshots;
using UWGame.Steam;
using WindowSystem;

namespace UWGame.Control;

public class Controller : DrawableGameComponent
{
	public enum GraphicsLevel
	{
		Low,
		High
	}

	private List<GameScreen> screens = new List<GameScreen>();

	private List<GameScreen> screensToUpdate = new List<GameScreen>();

	private InputManager inputManager;

	private IGraphicsDeviceService graphicsDeviceService;

	private ContentManager content;

	private SpriteBatch spriteBatch;

	private SpriteFont font;

	private Texture2D blankTexture;

	private Replayer replayer;

	private Recorder recorder;

	private string replayDisplayedTime;

	private bool skipRendering;

	private bool traceEnabled;

	private bool replayPaused;

	private CommandInvoker CommandInvoker;

	public RenderTarget2D ZoomRenderTarget;

	public GraphicsLevel GraphicsLevelSetting = GraphicsLevel.High;

	private InputData replayerInputData;

	private bool contentIsLoaded;

	public AudioManager AudioManager;

	public Options Options;

	public Progress Progress;

	public RandomGenerator RandomGenerator;

	public SteamManager SteamManager;

	public StatsAndAchievements StatsAndAchievements;

	private UnclaimedWorld game;

	private bool monkey = true;

	public object UpdateScreensLock = new object();

	private RasterizerState rasterizerSampleClosest = new RasterizerState
	{
		CullMode = CullMode.None,
		FillMode = FillMode.WireFrame
	};

	public float ActiveZoomFactor { get; private set; }

	/// <summary>MOD: the too-small-screen warning is reported once, not on every device reset.</summary>
	private bool warnedAboutDrawArea;

	public InputData InputData => inputManager.InputData;

	public new UnclaimedWorld Game => (UnclaimedWorld)base.Game;

	public new GraphicsDevice GraphicsDevice => base.GraphicsDevice;

	public Dimension DrawArea { get; private set; }

	public ContentManager Content => content;

	public SpriteBatch SpriteBatch => spriteBatch;

	public SpriteFont Font => font;

	public bool ScreenTraceEnabled
	{
		get
		{
			return traceEnabled;
		}
		set
		{
			traceEnabled = value;
		}
	}

	public Controller(UnclaimedWorld game)
		: base(game)
	{
		this.game = game;
		content = new UWGame.Port.UwContentManager(game.Services);
		content.RootDirectory = "Content";
		graphicsDeviceService = (IGraphicsDeviceService)game.Services.GetService(typeof(IGraphicsDeviceService));
		AudioManager = new AudioManager(null, canPlayMusic: true);
		RandomGenerator = new RandomGenerator(RandomGenerator.GeneratorType.Client);
		if (graphicsDeviceService == null)
		{
			throw new InvalidOperationException("No graphics device service.");
		}
		inputManager = new InputManager(this);
		replayer = new Replayer(inputManager, this, game);
		replayer.Initialize();
		inputManager.Initialize(replayer);
		recorder = new Recorder(this);
		recorder.Initialize();
		replayerInputData = new InputData(null);
		The.Snapshotter = new Snapshotter();
		CommandInvoker = new CommandInvoker();
		SteamManager = new SteamManager();
		StatsAndAchievements = new StatsAndAchievements(this);
		SteamManager.Initialize();
		StatsAndAchievements.Initialize();
		LoadOptionSettings();
		LoadProgress();
		// MOD: Options.RecordGame is the studio's switch and it works - it is just not reachable.
		// It lives in Options.xml with no control anywhere in the interface, so recording a
		// session meant knowing the field existed and hand-editing a file. The mod puts it in the
		// MODS menu; with the mod out this is Options.RecordGame exactly as before.
		recorder.RecordDuringPlay = UWGame.Mods.DebugMod.RecordGame(Options.RecordGame);
	}

	public void UpdateNewStateFromInputDevices()
	{
		inputManager.UpdateNewStateFromInputDevices();
	}

	public virtual void SetZoomRenderTaget()
	{
		if (ZoomIsActive())
		{
			GraphicsDevice.SetRenderTarget(ZoomRenderTarget);
		}
		else
		{
			GraphicsDevice.SetRenderTarget(null);
		}
	}

	public void EndGameSession()
	{
	}

	public void ValidateDrawAreaWidth(int minWidth)
	{
		if (DrawArea.Width < minWidth)
		{
			throw new Exception("Not enough drawing space. Increase the screen resolution, or reduce the zoom factor.");
		}
	}

	public void Destroy()
	{
		SteamManager.Destroy();
		if (ZoomRenderTarget != null)
		{
			ZoomRenderTarget.Dispose();
		}
	}

	protected override void LoadContent()
	{
		spriteBatch = new SpriteBatch(GraphicsDevice);
		font = content.Load<SpriteFont>(GUIManager.LCDandHUDBodyFontPath);
		blankTexture = content.Load<Texture2D>("MainMenu/blank");
		contentIsLoaded = true;
		if (!game.GraphicsDeviceManager.IsFullScreen)
		{
			// PORT DEVIATION 10 (see PORTING-NOTES.md). Game 1.0.4.8 added this windowed-mode
			// placement as ((Form)Control.FromHandle(Window.Handle)).Location = Point(10, 10).
			// MonoGame exposes GameWindow.Position directly and supports it on both WindowsDX
			// and DesktopGL, so no WinForms and no platform split is needed - which matters
			// because System.Windows.Forms does not exist on the GL target at all.
			UWGame.Port.PlatformDisplay.SetPosition(game.Window, new Microsoft.Xna.Framework.Point(10, 10));
		}
		PresentationParameters presentationParameters = GraphicsDevice.PresentationParameters;
		if (ZoomIsActive())
		{
			int width = (int)((float)presentationParameters.BackBufferWidth / ActiveZoomFactor);
			int height = (int)((float)presentationParameters.BackBufferHeight / ActiveZoomFactor);
			DrawArea = new Dimension(width, height);
			ZoomRenderTarget = new RenderTarget2D(GraphicsDevice, width, height, mipMap: false, presentationParameters.BackBufferFormat, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
		}
		else
		{
			DrawArea = new Dimension(presentationParameters.BackBufferWidth, presentationParameters.BackBufferHeight);
		}
		// MOD: say so when the interface will not fit, instead of letting panels be cut off at the
		// screen edge with no explanation. Once per session, and only when it is actually true.
		if (!warnedAboutDrawArea)
		{
			string tooSmall = UWGame.Mods.MagnificationMod.TooSmallWarning(DrawArea.Width, DrawArea.Height, ActiveZoomFactor);
			if (tooSmall != null)
			{
				warnedAboutDrawArea = true;
				GameStateManagement.UnclaimedWorld.LogError(tooSmall, "The screen is smaller than the interface");
			}
		}
		foreach (GameScreen screen in screens)
		{
			screen.LoadContent();
		}
		The.IngameLoadScreen.LoadContent();
	}

	private void LoadProgress()
	{
		if (File.Exists(Config.GetDataFolderPath(Config.DataType.UserSettings, "", "Progress.xml")))
		{
			try
			{
				DataLoader.DeserializeObject<Progress>("", "Progress.xml", out Progress, Config.DataType.UserSettings);
				return;
			}
			catch (IOException)
			{
				Progress = new Progress();
				return;
			}
		}
		CreateProgressFile();
	}

	private void LoadOptionSettings()
	{
		if (File.Exists(Config.GetDataFolderPath(Config.DataType.UserSettings, "", "Options.xml")))
		{
			DataLoader.DeserializeObject<Options>("", "Options.xml", out Options, Config.DataType.UserSettings);
			if (!Options.Validate())
			{
				CreateOptionsFileWithDefaultSettings();
			}
			else
			{
				Options.Write();
			}
		}
		else
		{
			CreateOptionsFileWithDefaultSettings();
		}
		ActiveZoomFactor = Options.ZoomFactor;
		// MOD: the floor was 1, which made every magnification below it unavailable rather than
		// merely unusual. Below 1 the game renders LARGER than the window and scales down, which
		// is a smaller interface and more of the world on screen.
		ActiveZoomFactor = UWGame.Mods.MagnificationMod.Clamp(ActiveZoomFactor);
		AudioManager.Init(Options, useFading: false);
	}

	private void CreateOptionsFileWithDefaultSettings()
	{
		Options = new Options();
		Options.Write();
	}

	private void CreateProgressFile()
	{
		Progress = new Progress();
		Progress.Write();
	}

	protected override void UnloadContent()
	{
		content.Unload();
		foreach (GameScreen screen in screens)
		{
			screen.UnloadContent();
		}
	}

	public void DummyEventHandler(object caller, EventArgs e)
	{
	}

	public void PauseReplay()
	{
		replayPaused = true;
	}

	public ReplayVerificationData RetrieveVerificationData()
	{
		Vector3 representativeEntityLocation = new Vector3(0f, 0f, 0f);
		Vector2 mapWindowWorldPosition = default(Vector2);
		if (The.Sim != null)
		{
			Entity representativeEntity = The.Sim.GetRepresentativeEntity();
			if (representativeEntity != null)
			{
				representativeEntityLocation = representativeEntity.PlaySiteLocation;
			}
			mapWindowWorldPosition = The.MapUI.MapWindowWorldPosition;
		}
		else
		{
			mapWindowWorldPosition.X = 0f;
			mapWindowWorldPosition.Y = 0f;
		}
		return new ReplayVerificationData
		{
			MapWindowLocation = mapWindowWorldPosition,
			RepresentativeEntityLocation = representativeEntityLocation
		};
	}

	public void Cleanup()
	{
		inputManager.Reset();
	}

	private void VerifySimAndRecordedData()
	{
		if (replayer.IsPlaying)
		{
			ReplayVerificationData replayVerificationData = RetrieveVerificationData();
			ReplayVerificationData recordedVerificationData = replayer.CurrentReplay.GetCurrentFrame().RecordedVerificationData;
			bool flag = false;
			if (!replayVerificationData.Verify(recordedVerificationData, replayer.Mode))
			{
				flag = true;
			}
			// PORT DEVIATION 20. The comparison above is the studio's and it works. What it did
			// with the answer was throw "Sim and recorded data have diverged." - no frame, no
			// count, nothing about WHAT diverged - and take the game down with it. That is a
			// message you can only act on by already knowing the answer.
			//
			// The trace writes Divergence.txt first: the frame, the number of random draws, a hash
			// of the draw-label sequence and the last sixty-four labels. Then the throw still
			// happens, because a silently wrong replay is worse than a stopped one - but now
			// there is something beside it to read.
			if (!replayer.CompareFrame(replayer.CurrentReplay.currentFrameIndex, replayVerificationData))
			{
				flag = true;
			}
			if (flag)
			{
				throw new Exception("Sim and recorded data have diverged. See Divergence.txt "
					+ "in the replay folder for the frame and the draws around it.");
			}
		}
	}

	public void SaveOrVerifyEntityAIState(string entityAIState)
	{
	}

	/// <summary>
	/// One random draw, by the label its call site passed. PORT DEVIATION 20: this was an empty
	/// method, and every RandomGenerator method in the game has been calling it - with a label
	/// naming the call site - since the studio wrote them. Recording one side and comparing the
	/// other is the whole of what makes a divergence locatable.
	/// </summary>
	public void SaveOrVerifyRandomGet(string getMessage)
	{
		if (recorder.isRecording)
		{
			recorder.SaveRandomGet(getMessage);
		}
		else if (replayer.IsPlaying)
		{
			replayer.RecordDrawForComparison(getMessage);
		}
	}

	public void StoreAndExecuteCommand(Command command)
	{
		if (recorder.isRecording)
		{
			recorder.RecordCommand(command);
		}
		CommandInvoker.Store(command);
		if (!replayer.IsPlaying || replayer.Mode == Replayer.ReplayingMode.InterfaceMode)
		{
			CommandInvoker.Execute(command);
		}
	}

	public bool SkipRendering()
	{
		return false;
	}

	public override void Update(GameTime gameTime)
	{
		if (Monitor.TryEnter(UpdateScreensLock))
		{
			try
			{
				AudioManager.Update(gameTime);
				HandleReplay(ref gameTime);
				if (!replayPaused)
				{
					inputManager.Update(gameTime, Game.IsActive);
					replayer.Update();
					screensToUpdate.Clear();
					foreach (GameScreen screen in screens)
					{
						screensToUpdate.Add(screen);
					}
					bool flag = false;
					bool coveredByOtherScreen = false;
					while (screensToUpdate.Count > 0)
					{
						GameScreen gameScreen = screensToUpdate[screensToUpdate.Count - 1];
						screensToUpdate.RemoveAt(screensToUpdate.Count - 1);
						gameScreen.Update(gameTime, flag, coveredByOtherScreen);
						if (gameScreen.ScreenState == ScreenState.TransitionOn || gameScreen.ScreenState == ScreenState.Active)
						{
							if (!flag)
							{
								gameScreen.HandleInput();
								flag = true;
							}
							if (!gameScreen.IsPopup)
							{
								coveredByOtherScreen = true;
							}
						}
					}
					if (traceEnabled)
					{
						TraceScreens();
					}
					StatsAndAchievements.Update();
					SteamManager.Update();
					VerifySimAndRecordedData();
					recorder.Update(gameTime);
					recorder.AdvanceFrame();
					replayer.AdvanceFrame();
				}
				return;
			}
			finally
			{
				Monitor.Exit(UpdateScreensLock);
			}
		}
		if (!LoadingScreenIsActive() && IngameLoadScreenIsActive())
		{
			The.IngameLoadScreen.Update(gameTime, otherScreenHasFocus: false, coveredByOtherScreen: false);
		}
	}

	private void CreateSteamMiniDump(string msg, Exception ex)
	{
	}

	private void HandleReplay(ref GameTime gameTime)
	{
		if (!replayer.IsPlaying)
		{
			return;
		}
		if (Game.IsActive)
		{
			replayerInputData.UpdateNewState(Keyboard.GetState(), Mouse.GetState());
			if (replayerInputData.IsKeyTapped(Microsoft.Xna.Framework.Input.Keys.P) || replayerInputData.IsKeyTapped(Microsoft.Xna.Framework.Input.Keys.Pause) || replayerInputData.IsKeyTapped(Microsoft.Xna.Framework.Input.Keys.Space))
			{
				replayPaused = !replayPaused;
			}
		}
		if (!replayPaused)
		{
			replayDisplayedTime = GetDisplayedReplayTime().Value.ToString();
			gameTime = replayer.CurrentReplay.GetCurrentFrame().GameTime;
		}
	}

	public double? GetDisplayedReplayTime()
	{
		if (replayer != null && replayer.CurrentReplay != null)
		{
			return replayer.CurrentReplay.GetCurrentFrame().GameTime.TotalGameTime.TotalSeconds - replayer.CurrentReplay.GetStartingSeconds();
		}
		return null;
	}

	public void SaveScenarioForReplay(StartGameParams startGameParams)
	{
		if (!replayer.IsActive && startGameParams.StartGameEditorParams == null)
		{
			recorder.SaveStartGameParams(startGameParams);
		}
	}

	public int? GetRandomSeed()
	{
		if (replayer.IsActive)
		{
			return replayer.CurrentReplay.randomSeed;
		}
		return new Random().Next();
	}

	public string GetReplayTime()
	{
		return replayDisplayedTime;
	}

	public void GameEnded()
	{
		recorder.StopRecording();
		Cleanup();
	}

	public void LoadReplay(string replayPath, float? timeToPauseReplay)
	{
		replayer.LoadReplay(replayPath, timeToPauseReplay);
	}

	public void LoadingFinished()
	{
		if (The.Sim != null && The.Sim.Mode == Sim.EngineMode.Game)
		{
			if (replayer.IsActive)
			{
				replayer.StartReplay();
			}
			else if (recorder.RecordDuringPlay)
			{
				recorder.CleanupOldRecordedFiles();
				recorder.StartRecording(The.Sim.GameplayRandomGenerator.RandomSeed);
			}
		}
	}

	private void TraceScreens()
	{
		List<string> list = new List<string>();
		foreach (GameScreen screen in screens)
		{
			list.Add(screen.GetType().Name);
		}
		Trace.WriteLine(string.Join(", ", list.ToArray()));
	}

	public override void Draw(GameTime gameTime)
	{
		SetZoomRenderTaget();
		if (ZoomIsActive())
		{
			GraphicsDevice.Clear(ClearOptions.Target, Microsoft.Xna.Framework.Color.Black, 1f, 0);
		}
		if (Monitor.TryEnter(UpdateScreensLock))
		{
			try
			{
				foreach (GameScreen screen in screens)
				{
					if (screen.ScreenState != ScreenState.Hidden)
					{
						screen.Draw(gameTime);
						if (screen is LoadingScreen)
						{
							break;
						}
					}
				}
			}
			finally
			{
				Monitor.Exit(UpdateScreensLock);
			}
		}
		else if (LoadingScreenIsActive())
		{
			The.LoadScreen.Draw(gameTime);
		}
		else if (IngameLoadScreenIsActive())
		{
			The.IngameLoadScreen.Draw(gameTime);
		}
		DrawFullscreenQuad();
	}

	private void DrawFullscreenQuad()
	{
		if (ZoomIsActive())
		{
			PresentationParameters presentationParameters = GraphicsDevice.PresentationParameters;
			GraphicsDevice.SetRenderTarget(null);
			if (ActiveZoomFactor % 1f == 0f)
			{
				spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, null, null);
			}
			else
			{
				spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
			}
			spriteBatch.Draw(ZoomRenderTarget, new Microsoft.Xna.Framework.Rectangle(0, 0, presentationParameters.BackBufferWidth, presentationParameters.BackBufferHeight), Microsoft.Xna.Framework.Color.White);
			spriteBatch.End();
		}
	}

	private bool LoadingScreenIsActive()
	{
		return !The.LoadScreen.IsLoadFinished;
	}

	private bool IngameLoadScreenIsActive()
	{
		return The.LoadScreen.IsLoadFinished;
	}

	public bool ZoomIsActive()
	{
		// MOD: any factor other than 1 needs the scaled render target, not just factors above it.
		return UWGame.Mods.MagnificationMod.ZoomIsActive(ActiveZoomFactor);
	}

	public void AddScreen(GameScreen screen, int index = -1)
	{
		screen.Controller = this;
		if (contentIsLoaded && graphicsDeviceService != null && graphicsDeviceService.GraphicsDevice != null)
		{
			screen.LoadContent();
		}
		if (index != -1)
		{
			screens.Insert(index, screen);
		}
		else
		{
			screens.Add(screen);
		}
	}

	public void RemoveFromList(GameScreen screen)
	{
		screens.Remove(screen);
		screensToUpdate.Remove(screen);
	}

	public int GetIndexOfScreen(GameScreen screen)
	{
		return screens.IndexOf(screen);
	}

	public void RemoveScreenNow(GameScreen screen, bool unloadContent = true)
	{
		if (unloadContent && graphicsDeviceService != null && graphicsDeviceService.GraphicsDevice != null)
		{
			screen.UnloadContent();
		}
		screen.Destroy();
		screens.Remove(screen);
		screensToUpdate.Remove(screen);
	}

	public GameScreen[] GetScreens()
	{
		return screens.ToArray();
	}

	public void FadeBackBufferToBlack(int alpha)
	{
		Dimension drawArea = DrawArea;
		spriteBatch.Begin();
		spriteBatch.Draw(blankTexture, new Microsoft.Xna.Framework.Rectangle(0, 0, drawArea.Width, drawArea.Height), new Microsoft.Xna.Framework.Color((byte)0, (byte)0, (byte)0, (byte)alpha));
		spriteBatch.End();
	}

	public void RecreateClientAfterLoad()
	{
		ContentManager clientContent = The.Client.Content;
		RemoveScreenNow(The.Client, unloadContent: false);
		GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
		GC.Collect();
		The.Client = new UWGame.ClientSide.Client(this, clientContent);
		AddScreen(The.Client);
		The.Client.Init();
		AudioManager.Resume();
	}
}
