using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using UWGame.Control;
using UWGame.SimSide;
using UWGame.SimSide.Scenarios;

namespace UWGame.ClientSide.Screens;

public class LoadingScreen : GameScreen
{
	private enum QueueState
	{
		ADD_SIM,
		ADD_CLIENT,
		INIT_SIM,
		GAMEDATA_LOADCONTENT,
		GAMEDATA_INIT,
		INIT_CLIENT,
		RUN_SIM,
		RUN_CLIENT,
		DONE
	}

	private enum SimClientQueueState
	{
		BEGIN,
		SimConstructor,
		CLIENT_CTOR,
		DONE
	}

	private bool loadingIsSlow;

	private bool otherScreensAreGone;

	private EventHandler<EventArgs> loadEventHandler;

	private StartGameParams startGameParams;

	private Rectangle backgroundDest;

	private Texture2D backgroundTexture;

	private ContentManager content;

	private LoadingScreenInterface intf;

	public bool WaitForUser = true;

	private bool userContinued;

	private static int imageToShow = DateTime.Now.Millisecond;

	private bool startGameQueueHasFinished;

	private bool startGame;

	private QueueState queueState;

	private Thread thread;

	private bool threadStarted;

	private SimClientQueueState simClientQueueState;

	private int progress;

	private List<string> queueNamesTimes = new List<string>();

	private int prevTime = Environment.TickCount;

	private bool threadlocked;

	private string graph = "";

	private float angle;

	private double graphValue;

	public bool IsLoadFinished { get; set; }

	private LoadingScreen(Controller screenManager, bool showInterface, bool loadingIsSlow, StartGameParams startGameParams, EventHandler<EventArgs> loadNextScreen)
	{
		base.Controller = screenManager;
		base.TransitionOnTime = TimeSpan.FromSeconds(0.5);
		WaitForUser = false;
		this.loadingIsSlow = loadingIsSlow;
		loadEventHandler = loadNextScreen;
		this.startGameParams = startGameParams;
		if (loadNextScreen == null)
		{
			startGame = true;
		}
		if (showInterface)
		{
			intf = new LoadingScreenInterface(this, startGameParams, base.Controller.Game);
		}
	}

	public override void LoadContent()
	{
		if (content == null)
		{
			content = new UWGame.Port.UwContentManager(base.Controller.Game.Services);
			content.RootDirectory = "Content";
		}
		InitBackgroundImage();
		if (intf != null)
		{
			intf.LoadContent();
		}
	}

	public override void UnloadContent()
	{
		content.Unload();
		if (intf != null)
		{
			intf.UnloadContent();
		}
	}

	public static void StartTransitioningToGame(Controller controller, StartGameParams startGameParams, bool loadingIsSlow, bool showInterface = false)
	{
		GameScreen[] screens = controller.GetScreens();
		for (int i = 0; i < screens.Length; i++)
		{
			screens[i].ExitScreen();
		}
		controller.AddScreen(The.LoadScreen = new LoadingScreen(controller, showInterface, loadingIsSlow, startGameParams, null));
	}

	public static void StartTransition(Controller controller, EventHandler<EventArgs> loadNextScreen, bool loadingIsSlow, bool showInterface = false)
	{
		GameScreen[] screens = controller.GetScreens();
		for (int i = 0; i < screens.Length; i++)
		{
			screens[i].ExitScreen();
		}
		controller.AddScreen(The.LoadScreen = new LoadingScreen(controller, showInterface, loadingIsSlow, null, loadNextScreen));
	}

	public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
	{
		base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
		if (otherScreensAreGone)
		{
			if (startGame)
			{
				if (StartGameQueueMain())
				{
					base.Controller.RemoveScreenNow(this);
					startGame = false;
				}
				base.ScreenState = ScreenState.Active;
			}
			else
			{
				loadEventHandler(this, EventArgs.Empty);
				base.Controller.RemoveScreenNow(this);
			}
		}
		if (intf != null)
		{
			intf.Update(gameTime);
		}
	}

	public bool StartGameQueueMain()
	{
		if (!threadStarted)
		{
			thread = new Thread(LoadDataInThread);
			thread.IsBackground = true;
			thread.Start();
			threadStarted = true;
			IsLoadFinished = false;
		}
		if (The.LoadScreen.startGameQueueHasFinished)
		{
			if (thread != null && thread.IsAlive && Thread.CurrentThread != thread)
			{
				thread.Join();
			}
			switch (queueState)
			{
			case QueueState.ADD_SIM:
				if (The.Sim != null)
				{
					base.Controller.AddScreen(The.Sim);
				}
				The.LoadScreen.Progress("Adding Client... ", 1937);
				queueState = QueueState.ADD_CLIENT;
				break;
			case QueueState.ADD_CLIENT:
				if (The.Client != null)
				{
					if (The.Client.GraphicsDevice.GraphicsDeviceStatus == GraphicsDeviceStatus.Lost)
					{
						return false;
					}
					base.Controller.AddScreen(The.Client);
				}
				The.LoadScreen.Progress("Initializing Sim... ", 31);
				queueState = QueueState.INIT_SIM;
				break;
			case QueueState.INIT_SIM:
				if (The.Sim != null)
				{
					The.Sim.Init();
				}
				The.LoadScreen.Progress("Loading GameData content... ", 63);
				queueState = QueueState.GAMEDATA_LOADCONTENT;
				break;
			case QueueState.GAMEDATA_LOADCONTENT:
				if (The.Client != null && The.Client.GraphicsDevice.GraphicsDeviceStatus == GraphicsDeviceStatus.Lost)
				{
					return false;
				}
				if (GameData.Instance.LoadContent(base.Controller.Game, The.Client.Content))
				{
					The.LoadScreen.Progress("Init GameData... ", 141);
					queueState = QueueState.GAMEDATA_INIT;
				}
				break;
			case QueueState.GAMEDATA_INIT:
				GameData.Instance.Initialize();
				The.LoadScreen.Progress("Initializing Client... ", 265);
				queueState = QueueState.INIT_CLIENT;
				break;
			case QueueState.INIT_CLIENT:
				if (The.Client != null && The.Client.GraphicsDevice.GraphicsDeviceStatus == GraphicsDeviceStatus.Lost)
				{
					return false;
				}
				if (intf != null && PromptDialogIsDisplayed())
				{
					intf.PromptUserToContinue();
					break;
				}
				if (The.Client != null)
				{
					The.Client.Init();
				}
				The.LoadScreen.Progress("Sim.BeginRun... ", 67);
				queueState = QueueState.RUN_SIM;
				break;
			case QueueState.RUN_SIM:
				if (The.Sim == null || The.Sim.QueueBeginRun())
				{
					The.LoadScreen.Progress("Client.BeginRun... ", 2625);
					queueState = QueueState.RUN_CLIENT;
				}
				break;
			case QueueState.RUN_CLIENT:
				VerifyLoadSaveGame();
				if (The.Client != null)
				{
					if (The.Client.GraphicsDevice.GraphicsDeviceStatus == GraphicsDeviceStatus.Lost)
					{
						return false;
					}
					if (!The.Client.BeginRunWasCalled)
					{
						The.Client.BeginRun();
					}
				}
				The.LoadScreen.Progress("Loading Queue Done... ", 1);
				queueState = QueueState.DONE;
				break;
			default:
				IsLoadFinished = true;
				base.Controller.LoadingFinished();
				return true;
			}
		}
		return false;
	}

	private void VerifyLoadSaveGame()
	{
		if (Sim.LoadException != null)
		{
			throw Sim.LoadException;
		}
	}

	private bool PromptDialogIsDisplayed()
	{
		if (WaitForUser)
		{
			return !userContinued;
		}
		return false;
	}

	private void LoadDataInThread()
	{
		base.Controller.UpdateNewStateFromInputDevices();
		while (true)
		{
			try
			{
				if (!QueueSimAndClientCtors())
				{
					break;
				}
			}
			catch (Exception e)
			{
				base.Controller.Game.HandleExceptionInReleaseMode(e, isMainThread: false);
			}
		}
	}

	private bool QueueSimAndClientCtors()
	{
		bool flag = false;
		switch (simClientQueueState)
		{
		case SimClientQueueState.BEGIN:
			The.LoadScreen.Progress("Sim Constructor...", 62);
			simClientQueueState = SimClientQueueState.SimConstructor;
			break;
		case SimClientQueueState.SimConstructor:
			if (The.Sim == null)
			{
				int? randomSeed = base.Controller.GetRandomSeed();
				base.Controller.SaveScenarioForReplay(startGameParams);
				The.Sim = new Sim(base.Controller, startGameParams, randomSeed);
			}
			else
			{
				// PORT DIAGNOSTIC. A Sim that survived the last game is reused here rather than
				// rebuilt - and then it is not the scenario's world that is played, it is the
				// previous one, clock and all. A replay loaded onto a reused Sim compares its
				// first frame against a world that is already hours further on, which reads as a
				// divergence on frame 0 with the clocks far apart. Sim.ClearData nulls this in
				// Sim.Destroy, so reaching here at all means the game screen was not destroyed.
				GameStateManagement.UnclaimedWorld.LogError(
					"A Sim from the previous game was still alive when this one started, so the "
					+ "world was NOT rebuilt. Its clock reads "
					+ (The.Sim.DateAndTime == null
						? "-"
						: The.Sim.DateAndTime.CurrentTimeDateYear.TotalDays.ToString(
							"R", System.Globalization.CultureInfo.InvariantCulture))
					+ " days. Sim.Destroy did not run on the screen that was left.",
					"Stale Sim reused");
			}
			if (The.Sim.QueueGameDataAndSimInit())
			{
				The.LoadScreen.Progress("ClientConstructor...", 140);
				simClientQueueState = SimClientQueueState.CLIENT_CTOR;
			}
			break;
		case SimClientQueueState.CLIENT_CTOR:
			if (!flag && The.Client == null)
			{
				The.Client = new Client(base.Controller);
			}
			The.LoadScreen.Progress("Everything Else...", 47);
			The.LoadScreen.startGameQueueHasFinished = true;
			simClientQueueState = SimClientQueueState.DONE;
			return false;
		}
		return true;
	}

	public void OKToStartGame()
	{
		userContinued = true;
	}

	protected override float getStringEnlargementFactor()
	{
		return 1f;
	}

	public void Progress(string newStatus, int points)
	{
		int tickCount = Environment.TickCount;
		while (threadlocked)
		{
		}
		threadlocked = true;
		progress += points;
		if (queueNamesTimes.Count >= 1)
		{
			List<string> list = queueNamesTimes;
			int index = queueNamesTimes.Count - 1;
			list[index] = list[index] + " " + (tickCount - prevTime) + " msec";
		}
		queueNamesTimes.Add(newStatus);
		threadlocked = false;
		prevTime = tickCount;
	}

	public override void Draw(GameTime gameTime)
	{
		if (base.ScreenState == ScreenState.Active && base.Controller.GetScreens().Length == 1)
		{
			otherScreensAreGone = true;
		}
		if (loadingIsSlow)
		{
			base.Controller.SpriteBatch.GraphicsDevice.Clear(Color.Black);
			Color color = new Color(byte.MaxValue, byte.MaxValue, byte.MaxValue, base.TransitionAlpha);
			base.Controller.SpriteBatch.Begin();
			base.Controller.SpriteBatch.Draw(backgroundTexture, backgroundDest, color);
			base.Controller.SpriteBatch.End();
			UpdateAndDrawProgress();
			if (intf != null)
			{
				intf.Draw(gameTime);
			}
		}
	}

	public override void Destroy()
	{
		loadEventHandler = null;
		if (thread != null)
		{
			thread.Join();
			thread = null;
		}
		if (intf != null)
		{
			intf.Destroy();
			intf = null;
		}
	}

	private void UpdateAndDrawProgress()
	{
		Vector2 position = new Vector2(100f, 100f);
		while (threadlocked)
		{
		}
		threadlocked = true;
		if (graphValue < (double)progress)
		{
			graphValue += 1.0;
		}
		if (queueState < QueueState.ADD_CLIENT)
		{
			graphValue = graphValue * 0.999 + (double)progress * 0.001;
		}
		else
		{
			graphValue = graphValue * 0.7 + (double)progress * 0.3;
		}
		threadlocked = false;
		double num = 32263.0;
		double num2 = Math.Min(1.0, graphValue / num);
		double num3 = 400.0;
		int num4 = (int)(num2 * num3);
		if (graph.Length > num4)
		{
			graph = "";
		}
		while (graph.Length < num4)
		{
			graph += "|";
		}
		base.Controller.SpriteBatch.Begin();
		int num5 = base.Controller.DrawArea.Height - 200;
		Color color = Color.DarkGray;
		if (The.LoadScreen.startGameQueueHasFinished)
		{
			color = Color.Gray;
		}
		for (int i = 0; i < 3; i++)
		{
			base.Controller.SpriteBatch.DrawString(base.Controller.Font, graph, new Vector2(100 + i, num5), color, 0f, new Vector2(0f), new Vector2(1f, 2f), SpriteEffects.None, 0f);
		}
		if (Keyboard.GetState().IsKeyDown(Keys.Space))
		{
			float num6 = getStringEnlargementFactor();
			position.Y += 40f;
			while (threadlocked)
			{
			}
			threadlocked = true;
			int num7 = queueNamesTimes.Count;
			for (int j = 0; j < queueNamesTimes.Count; j++)
			{
				string text = queueNamesTimes[j] + " " + graphValue;
				if (--num7 == 0)
				{
					num6 *= 1.33f;
				}
				if (num7 < 20)
				{
					base.Controller.SpriteBatch.DrawString(base.Controller.Font, text, position, Color.Black, 0f, new Vector2(0f), num6, SpriteEffects.None, 0f);
					position.Y += 20f;
				}
			}
			threadlocked = false;
		}
		else
		{
			while (threadlocked)
			{
			}
			threadlocked = true;
			threadlocked = false;
		}
		base.Controller.SpriteBatch.End();
	}

	private void InitBackgroundImage()
	{
		if (startGameParams != null && startGameParams.StartScenarioParams != null)
		{
			backgroundTexture = content.Load<Texture2D>(startGameParams.StartScenarioParams.Scenario.ScenarioData.LoadingBackgroundImage);
		}
		else
		{
			backgroundTexture = content.Load<Texture2D>("Scenarios/Default/Scenario Screen/TitleImgSurvival_1920px");
		}
		int width = backgroundTexture.Width;
		int num = backgroundTexture.Height;
		Dimension drawArea = base.Controller.DrawArea;
		if (drawArea.Width < width)
		{
			float num2 = (float)drawArea.Width / (float)width;
			width = drawArea.Width;
			num = (int)((float)num * num2);
		}
		backgroundDest = new Rectangle((drawArea.Width - width) / 2, (drawArea.Height - num) / 3, width, num);
	}
}
