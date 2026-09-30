using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using InputEventSystem;
using Kensei.Dev;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RoundLineCode;
using SpriteSheetRuntime;
using UWGame.Client.Audio;
using UWGame.Client.Interface.MapGUI;
using UWGame.ClientSide.Feedback;
using UWGame.ClientSide.Interface;
using UWGame.ClientSide.Interface.HUD_Windows;
using UWGame.ClientSide.Interface.Tasks;
using UWGame.ClientSide.Log;
using UWGame.ClientSide.Map;
using UWGame.ClientSide.Particles;
using UWGame.ClientSide.Renderables;
using UWGame.Control;
using UWGame.Control.Commands;
using UWGame.Control.Replays;
using UWGame.SimSide;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.AI.Pathfinding;
using UWGame.SimSide.AI.StrategicDecisions;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Commands;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.InGameEvents;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.IngameEvents;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems;
using WindowSystem;
using Xclna.Xna.Animation;

namespace UWGame.ClientSide;

public class Client : GameScreen
{
	public ContentManager Content;

	public SpriteBatch spriteBatch;

	public SpriteSheet FlatSpriteSheet;

	public Effect EdgeDetectEffect;

	private PrimitiveBatch primitiveBatch;

	public QuadRendererComponent quadRenderer;

	public AudioManager AudioManager;

	public FeedbackManager Feedback;

	public float FarPlane = 3000f;

	public float NearPlane = -1500f;

	public Matrix Projection;

	public Matrix PerspectiveProjection;

	public GameWorldRenderer Renderer;

	public RoundLineManager RoundLineManager;

	public Texture2D interfaceArt;

	public ParticleManager ParticleManager;

	public RandomGenerator ClientRandomGenerator;

	public List<EventDialogData> EventDialogsData = new List<EventDialogData>();

	public int CurrentEventDialogIndex;

	public bool EnableKeyboardShortcuts = true;

	public UWGame.ClientSide.Log.Log Log;

	public bool BloomEnabled = true;

	public bool EdgeDetectEnabled = true;

	private SleepyUpdater<Renderable> renderables = new SleepyUpdater<Renderable>(Module.Client);

	private int frameRate;
	public TimeSpan ElapsedTimeBetweenDraws;

	public GameTime GameTime = new GameTime();

	private InputData inputData;

	private long previousDrawElapsedTotalGameTime;

	private Regulator printAllegiancesRegulator;

	private Regulator printPerformanceRegulator;

	private Regulator printEmigrateRollsRegulator;
	private float timeToSleep;

	private float desiredSleepAmount;

	public bool MapHasMoved = true;

	public static Dictionary<string, Rectangle?[,]> AllConnectedGroundSprites = new Dictionary<string, Rectangle?[,]>();

	private Vector3 testRotation = new Vector3(0f);

	private Vector3 testTranslation = new Vector3(0f);
	public const float MaxSpeed = 180f;

	public GraphicsDevice GraphicsDevice => base.Controller.GraphicsDevice;

	public int ScreenWidth => base.Controller.DrawArea.Width;

	public int ScreenHeight => base.Controller.DrawArea.Height;

	public bool IsModal { get; private set; }

	public bool BeginRunWasCalled { get; private set; }

	public Client(Controller controller, ContentManager clientContent = null)
	{
		The.Client = this;
		ClientRandomGenerator = new RandomGenerator(RandomGenerator.GeneratorType.Client);
		printAllegiancesRegulator = new Regulator(ClientRandomGenerator, 1.0, "Client");
		printPerformanceRegulator = new Regulator(ClientRandomGenerator, 1.0, "Client");
		printEmigrateRollsRegulator = new Regulator(ClientRandomGenerator, 0.25, "Client");
		base.TransitionOffTime = TimeSpan.FromSeconds(0.5);
		base.Controller = controller;
		inputData = base.Controller.InputData;
		The.InGameUI = new InGameInterface(base.Controller.Game, GameData.Instance.GUIConstants.CustomColors);
		The.MapUI = new MapClient();
		quadRenderer = new QuadRendererComponent();
		AudioManager = new AudioManager(controller.Options, canPlayMusic: false);
		if (clientContent == null)
		{
			Content = new UWGame.Port.UwContentManager(base.Controller.Game.Services);
			Content.RootDirectory = "Content";
		}
		else
		{
			Content = clientContent;
		}
		Feedback = new FeedbackManager();
		Projection = Matrix.CreateOrthographic(base.Controller.DrawArea.Width, base.Controller.DrawArea.Height, NearPlane, FarPlane);
		float aspectRatio = (float)base.Controller.DrawArea.Width / (float)base.Controller.DrawArea.Height;
		PerspectiveProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(40f), aspectRatio, 1f, 10000f);
		ParticleManager = new ParticleManager();
		Log = new UWGame.ClientSide.Log.Log();
		Renderer = new GameWorldRenderer();
		RoundLineManager = new RoundLineManager();
	}

	public override void Init()
	{
		base.Init();
		The.InGameUI.Initialize();
		Renderer.Init();
		ParticleManager.Init();
	}

	public virtual void SetToolReplenishStatusItemOwned(Expedition expedition, EntityType toolType, bool ownsItem, float? requiredAmount)
	{
		Feedback.SetToolReplenishStatusOwnsItem(expedition, toolType, ownsItem, requiredAmount);
	}

	public virtual void SetToolReplenishStatusItemAvailable(Expedition expedition, EntityType toolType, bool itemAvailable, float? requiredAmount)
	{
		Feedback.SetToolReplenishStatusItemIsAvailable(expedition, toolType, itemAvailable, requiredAmount);
	}

	public bool GetToolReplenishStatus(Expedition expedition, EntityType toolType, out float? minimumRequiredAmount)
	{
		return Feedback.GetToolReplenishStatus(expedition, toolType, out minimumRequiredAmount);
	}

	public void GetFeedback(JobID jobID, out bool isInAccessible, out bool isBlockedByThreat, out bool isBlockedDueToBoldStanceRequired, out bool tooFarFromExpedition, out bool huntingNotFeasible, out bool areaNotCleared)
	{
		Feedback.GetFeedback(jobID, out isInAccessible, out isBlockedByThreat, out isBlockedDueToBoldStanceRequired, out tooFarFromExpedition, out huntingNotFeasible, out areaNotCleared);
	}

	public virtual void GetFeedback(EntityID entityID, out bool isInAccessible, out bool isBlockedByThreat, out bool isBlockedByBoldStance)
	{
		Feedback.GetFeedback(entityID, out isInAccessible, out isBlockedByThreat, out isBlockedByBoldStance);
	}

	public virtual bool GetIsInaccessible(JobID jobID)
	{
		return Feedback.GetIsInaccessible(jobID);
	}

	public virtual bool GetIsBlockedByThreat(JobID jobID)
	{
		return Feedback.GetIsBlockedByThreat(jobID);
	}

	public virtual void SetJobInaccessible(Job job, IHasEntityGroup ownerOfJob, bool isInaccessible)
	{
		Feedback.SetJobInaccessible(job, ownerOfJob, isInaccessible);
	}

	public virtual void SetHuntingJobNotFeasible(Job job, IHasEntityGroup ownerOfJob, bool value)
	{
		Feedback.SetHuntingJobNotFeasible(job, ownerOfJob, value);
	}

	public virtual void SetAreaNotCleared(Job job, IHasEntityGroup ownerOfJob, bool value)
	{
		Feedback.SetAreaNotCleared(job, ownerOfJob, value);
	}

	public virtual void SetJobTooFarFromExpedition(Job job, IHasEntityGroup ownerOfJob, bool value)
	{
		Feedback.SetJobTooFarFromExpedition(job, ownerOfJob, value);
	}

	public virtual void SetJobBlockedByBoldStance(Job job, IHasEntityGroup ownerOfJob, bool isBlocked)
	{
		Feedback.SetJobBlockedByBoldStance(job, ownerOfJob, isBlocked);
	}

	public virtual void SetJobBlockedByThreat(Job job, IHasEntityGroup ownerOfJob, bool isBlocked)
	{
		Feedback.SetJobBlockedByThreat(job, ownerOfJob, isBlocked);
	}

	public virtual void DestroyAccessibility(AccessibilityFeedback accessibility)
	{
		Feedback.DestroyAccessibility(accessibility);
	}

	public virtual void DestroyAccessibility(JobID jobID)
	{
		Feedback.DestroyAccessibility(jobID);
	}

	public virtual void DestroyAccessibility(EntityID entityID)
	{
		Feedback.DestroyAccessibility(entityID);
	}

	public virtual void HandleDestroyedEntity(EntityID entityID)
	{
		Feedback.HandleDestroyedEntity(entityID);
	}

	public virtual void SetEntityInaccessible(Allegiance agentAllegiance, IKnownEntityData entityData, bool isInaccessible)
	{
		Feedback.SetEntityInaccessible(agentAllegiance, entityData, isInaccessible);
	}

	public virtual void SetEntityBlockedByThreat(Allegiance agentAllegiance, IKnownEntityData entityData, bool blockedByThreat)
	{
		Feedback.SetEntityBlockedByThreat(agentAllegiance, entityData, blockedByThreat);
	}

	public void UpdateModelMatricesWithNewPositions()
	{
		Renderer.UpdateModelMatricesWithNewPositions();
	}

	public virtual void LogIsBroken(Entity entity)
	{
		if (entity.OwnedBy.HasValue && LookUpOwners.ResolveEntityOwner((IKnownEntityData)entity, out EntityGroup ownedEntities) && ownedEntities.IsOwnedByAllegiance(The.InGameUI.UIAllegiance))
		{
			Allegiance allegiance = ownedEntities.GetAllegiance();
			Point? mapPosition = entity.MapPosition;
			if (mapPosition.HasValue && The.Map.TileIsOnMap(mapPosition.Value) && The.Map.GetTile(mapPosition.Value).AllegiancesThatSeeThisTile.Contains(allegiance))
			{
				Log.AddLogEvent(The.Client.Log.EconomicEvent, entity, " is no longer functional. It has a broken part.");
			}
		}
	}

	public virtual void LogOutOfFuel(EntityType tool, float? requiredAmount)
	{
		string eventText = ((!requiredAmount.HasValue) ? $"Not enough suitable fuel in range. Production orders using {tool.Name} cannot be completed before fuel is available" : $"Not enough suitable fuel in range. Production orders using {tool.Name} cannot be completed before we have at least {requiredAmount.Value:N2} BLK of suitable fuel available");
		Log.AddLogEvent(The.Client.Log.EconomicEvent, null, eventText, UWGame.ClientSide.Log.Priority.High);
	}

	public virtual void LogKilledTarget(Entity targetAsEntity, Entity attacker)
	{
		UWGame.ClientSide.Log.Priority value = UWGame.ClientSide.Log.Priority.Normal;
		if (targetAsEntity.Intelligence.Allegiance == The.InGameUI.UIAllegiance)
		{
			value = UWGame.ClientSide.Log.Priority.High;
		}
		AddLogEvent(The.Client.Log.GeneralEvent, targetAsEntity, "was killed by " + attacker.ToLink() + ".", value);
	}

	public override void Draw(GameTime gameTime)
	{
		if (base.Controller.SkipRendering())
		{
			return;
		}
		ElapsedTimeBetweenDraws = new TimeSpan(gameTime.TotalGameTime.Ticks - previousDrawElapsedTotalGameTime);
		previousDrawElapsedTotalGameTime = gameTime.TotalGameTime.Ticks;
		if (!The.LoadScreen.IsLoadFinished || !BeginRunWasCalled)
		{
			return;
		}
		MarkPerformanceTime("before client draw", Color.Wheat);
		if (The.InGameUI == null)
		{
			throw new Exception("Where is our In Game UI, eh?");
		}
		UpdateModelMatricesWithNewPositions();
		MarkPerformanceTime("updateModelMatrices", Color.Tomato);
		if (The.Map.TileMap != null)
		{
			The.MapUI.Draw();
			MarkPerformanceTime("MapClient.Draw", Color.Thistle);
			Renderer.Draw();
			MarkPerformanceTime("Renderer.Draw", Color.Teal);
			The.InGameUI.Draw(gameTime);
			if (base.TransitionPosition > 0f)
			{
				base.Controller.FadeBackBufferToBlack(255 - base.TransitionAlpha);
			}
			MarkPerformanceTime("Client.DrawDone", Color.Turquoise);
			The.InGameUI.DrawPauseIcon(The.Sim.IsPaused);
		}
	}

	public void MarkPerformanceTime(string periodName, Color color)
	{
	}

	public void InitializeSpriteBatch()
	{
		spriteBatch = new SpriteBatch(base.Controller.GraphicsDevice);
	}

	public void PauseGame()
	{
		if (The.Sim.Mode == Sim.EngineMode.Game)
		{
			UWGame.Control.Commands.Command command = new Pause();
			base.Controller.StoreAndExecuteCommand(command);
		}
	}

	public void ResumeGame()
	{
		if (The.Sim != null && The.Sim.Mode == Sim.EngineMode.Game)
		{
			UWGame.Control.Commands.Command command = new Resume();
			base.Controller.StoreAndExecuteCommand(command);
		}
	}

	public void SetGameSpeed(Speeds speed)
	{
		if (The.Sim.Mode == Sim.EngineMode.Game)
		{
			UWGame.Control.Commands.Command command = new SetGameSpeed(speed);
			base.Controller.StoreAndExecuteCommand(command);
		}
	}

	public void OnSetSpeed(Speeds speed)
	{
		The.InGameUI.MainPanel.OnSetSpeed(speed);
	}

	public void OnPause()
	{
		The.InGameUI.MainPanel.OnPause();
		base.Controller.AudioManager.Pause();
		AudioManager.Pause();
	}

	public void OnResume()
	{
		The.InGameUI.MainPanel.OnResume();
		base.Controller.AudioManager.Resume();
		AudioManager.Resume();
	}

	public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
	{
		base.Update(gameTime, otherScreenHasFocus: false, coveredByOtherScreen: false);
		if (!The.LoadScreen.IsLoadFinished)
		{
			return;
		}
		// MOD: the developer overlays are read from Kensei.Dev.Options, which nothing in the
		// shipped game ever wrote to - Client.InitDeveloperDialog, the only thing that would
		// have, has no callers. This is what writes to it. It compares a composite of the
		// switches and returns immediately unless one moved.
		UWGame.Mods.DebugMod.ApplyOverlays();
		// MOD: samples the colony to a text file on a game-time interval, so a change can be
		// checked by reading rather than by watching. Returns on its first line when off.
		UWGame.Mods.StateDumpMod.Sample();
		// MOD: pauses the game between turns and swaps files with whatever is playing it -
		// agent/observe.txt out, agent/act.xml in. Returns on its first line when off.
		UWGame.Mods.AgentMod.Tick(base.Controller);
		bool limitFramerateWhenPaused = base.Controller.Options.LimitFramerateWhenPaused;
		if (base.IsActive)
		{
			GameTime = gameTime;
			if (!BeginRunWasCalled)
			{
				BeginRun();
			}
			The.InGameUI.Update(gameTime);
			if (The.Sim == null)
			{
				return;
			}
			The.MapUI.Update(gameTime);
			if (!The.Sim.IsPaused)
			{
				ParticleManager.Update();
				UpdateRenderables(gameTime);
				Feedback.Update(gameTime);
			}
			else
			{
				LimitFPS(limitFramerateWhenPaused);
			}
			Manager.Update();
			AudioManager.Update(gameTime);
			The.MapUI.UpdateMouseInMap();
			if (The.InGameUI.gui.IsMouseInInterface(inputData.mouseX, inputData.mouseY))
			{
				The.MapUI.TryMapScrolling();
			}
		}
		The.Client.MarkPerformanceTime("Client update", Color.CadetBlue);
	}

	private void LimitFPS(bool limitFPSWhenPaused)
	{
		if (limitFPSWhenPaused && frameRate > 0)
		{
			if (Math.Abs(frameRate - base.Controller.Options.TargetFramerateWhenPaused) > 0)
			{
				float num = 1f / (float)frameRate;
				float num2 = 1f / (float)base.Controller.Options.TargetFramerateWhenPaused;
				desiredSleepAmount = (num2 - num) * 1000f * 2f;
				float num3 = desiredSleepAmount - timeToSleep;
				if (Math.Abs(num3) > 0f)
				{
					timeToSleep += 0.04f * num3;
					timeToSleep = Common.Clamp(timeToSleep, 0f, 50f);
				}
				Thread.Sleep((int)timeToSleep);
			}
			else
			{
				timeToSleep = 0f;
			}
		}
		else
		{
			timeToSleep = 0f;
		}
	}

	public virtual void GiveDetectionFeedback(IDetectable detectable, DetectionFactor resourceDetectionFactor, Entity detectingEntity, Allegiance allegiance)
	{
		Entity entity = detectable as Entity;
		bool flag = detectable.RequiresRollToDetect();
		bool flag2 = false;
		if (resourceDetectionFactor != null)
		{
			flag2 = resourceDetectionFactor.AddLogMessageWhenDetected;
		}
		SetFlashing(detectable);
		if (detectingEntity != null && (entity != null || flag2) && flag && (entity == null || LogEntity(entity, allegiance)))
		{
			AddLogEvent(detectingEntity.Intelligence.Allegiance, The.Client.Log.GeneralEvent, detectingEntity, $"has spotted {detectable.ToLink()}");
		}
	}

	private bool LogEntity(Entity entity, Allegiance allegiance)
	{
		if (!allegiance.SharedKnowledge.PlaySiteKnowledge.SpottedAnimals.Contains(entity.EntityType) || (entity.EntityType.IntelligenceType != null && entity.EntityType.IntelligenceType.IsPredator))
		{
			return true;
		}
		return false;
	}

	private void SetFlashing(IDetectable detectable)
	{
		if (detectable is ResourceContainer resourceContainer)
		{
			resourceContainer.FlashAsDetected();
		}
		if (detectable is Entity { Renderable: not null } entity && entity.IsOnPlaySite())
		{
			entity.Renderable.SetToParentLocation();
			entity.Renderable.FlashAsDetected();
		}
	}

	public virtual void AddLogEvent(Allegiance loggingAllegiance, EventType eventType, Entity concernedEntity, string eventText, UWGame.ClientSide.Log.Priority? priority = null)
	{
		if (The.InGameUI.UIAllegiance == loggingAllegiance)
		{
			Log.AddLogEvent(eventType, concernedEntity, eventText, priority);
		}
	}

	public virtual void AddLogEvent(EventType eventType, Entity concernedEntity, string eventText, UWGame.ClientSide.Log.Priority? priority = null)
	{
		Log.AddLogEvent(eventType, concernedEntity, eventText, priority);
	}

	public virtual void AddRenderable(Renderable renderable)
	{
		renderables.Add(renderable);
	}

	public virtual void RemoveRenderable(Renderable renderable)
	{
		renderables.Remove(renderable);
	}

	private void UpdateRenderables(GameTime gameTime)
	{
		renderables.Update(gameTime);
	}

	public override void LoadContent()
	{
		FlatSpriteSheet = Content.Load<SpriteSheet>("FlatSprites");
		SetupRoadPieceSprites();
		The.InGameUI.LoadContent();
		EdgeDetectEffect = Content.Load<Effect>("EdgeDetect");
		Renderer.LoadContent();
		RoundLineManager.LoadContent(GraphicsDevice, Content);
		quadRenderer.LoadContent();
	}

	public void BeginRun()
	{
		Dimension drawArea = base.Controller.DrawArea;
		Manager.Initialise(The.Client.Content, The.Client.GraphicsDevice, 0, 0, drawArea.Width, drawArea.Height);
		The.InGameUI.SetInterfaceCursor();
		The.InGameUI.PostLoadContent();
		Renderer.PostLoadContent();
		Renderer.InitAfterMapLoad();
		BeginRunWasCalled = true;
	}

	public override void UnloadContent()
	{
		Content.Unload();
		Renderer.UnloadContent();
		The.InGameUI.UnloadContent();
	}

	public override void Destroy()
	{
		The.InGameUI.Destroy();
		The.InGameUI = null;
		The.MapUI = null;
		Kensei.Dev.Options.Destroy();
		AudioManager.Destroy();
		Renderer.Destroy();
		The.Client = null;
	}

	private static void SetupRoadPieceSprites()
	{
		foreach (KeyValuePair<string, EntityType> allEntityType in GameData.Instance.AllEntityTypes)
		{
			if (allEntityType.Value.RenderableTypeMode == null || allEntityType.Value.RenderableTypeMode.RenderAsConnectedGroundSpriteType == null)
			{
				continue;
			}
			string assetName = allEntityType.Value.RenderableTypeMode.RenderAsConnectedGroundSpriteType.AssetName;
			if (!AllConnectedGroundSprites.ContainsKey(assetName))
			{
				try
				{
					Rectangle?[,] array = new Rectangle?[8, 8];
					array[1, 2] = The.Client.FlatSpriteSheet.GetSourceRectangle($"{assetName}_E_S");
					array[0, 1] = The.Client.FlatSpriteSheet.GetSourceRectangle($"{assetName}_N_E");
					array[0, 5] = The.Client.FlatSpriteSheet.GetSourceRectangle($"{assetName}_N_SE");
					array[0, 3] = The.Client.FlatSpriteSheet.GetSourceRectangle($"{assetName}_N_W");
					array[7, 2] = The.Client.FlatSpriteSheet.GetSourceRectangle($"{assetName}_NW_S");
					array[2, 4] = The.Client.FlatSpriteSheet.GetSourceRectangle($"{assetName}_S_NE");
					array[6, 1] = The.Client.FlatSpriteSheet.GetSourceRectangle($"{assetName}_SW_E");
					array[6, 0] = The.Client.FlatSpriteSheet.GetSourceRectangle($"{assetName}_SW_N");
					array[3, 4] = The.Client.FlatSpriteSheet.GetSourceRectangle($"{assetName}_W_NE");
					array[3, 2] = The.Client.FlatSpriteSheet.GetSourceRectangle($"{assetName}_W_S");
					array[3, 5] = The.Client.FlatSpriteSheet.GetSourceRectangle($"{assetName}_W_SE");
					array[3, 3] = The.Client.FlatSpriteSheet.GetSourceRectangle($"{assetName}_W");
					array[7, 7] = The.Client.FlatSpriteSheet.GetSourceRectangle($"{assetName}_NW");
					array[0, 0] = The.Client.FlatSpriteSheet.GetSourceRectangle($"{assetName}_N");
					array[4, 4] = The.Client.FlatSpriteSheet.GetSourceRectangle($"{assetName}_NE");
					array[1, 1] = The.Client.FlatSpriteSheet.GetSourceRectangle($"{assetName}_E");
					array[5, 5] = The.Client.FlatSpriteSheet.GetSourceRectangle($"{assetName}_SE");
					array[2, 2] = The.Client.FlatSpriteSheet.GetSourceRectangle($"{assetName}_S");
					array[6, 6] = The.Client.FlatSpriteSheet.GetSourceRectangle($"{assetName}_SW");
					AllConnectedGroundSprites.Add(assetName, array);
				}
				catch (Exception ex)
				{
					throw new Exception($"Error setting up connected ground sprites '{assetName}': {ex.Message}");
				}
			}
		}
	}

	public void DrawPoint(Vector2 where, Color color)
	{
		if (primitiveBatch == null)
		{
			primitiveBatch = new PrimitiveBatch(base.Controller.DrawArea.Width, base.Controller.DrawArea.Height, base.Controller.GraphicsDevice);
		}
		primitiveBatch.Begin(PrimitiveType.LineList);
		primitiveBatch.AddVertex(where, color);
		where.X -= 1f;
		primitiveBatch.AddVertex(where, color);
		where.X += 2f;
		primitiveBatch.AddVertex(where, color);
		where.X -= 1f;
		where.Y -= 1f;
		primitiveBatch.AddVertex(where, color);
		where.Y += 2f;
		primitiveBatch.AddVertex(where, color);
		primitiveBatch.End();
	}

	public void OrientAttachedModel(string option, bool? newBool, float? newFloat)
	{
		if (!newFloat.HasValue)
		{
			return;
		}
		if (option.Contains("Rotate"))
		{
			if (option.Contains("X"))
			{
				testRotation.X = newFloat.Value;
			}
			else if (option.Contains("Y"))
			{
				testRotation.Y = newFloat.Value;
			}
			else if (option.Contains("Z"))
			{
				testRotation.Z = newFloat.Value;
			}
		}
		else if (option.Contains("Translate"))
		{
			if (option.Contains("X"))
			{
				testTranslation.X = newFloat.Value;
			}
			else if (option.Contains("Y"))
			{
				testTranslation.Y = newFloat.Value;
			}
			else if (option.Contains("Z"))
			{
				testTranslation.Z = newFloat.Value;
			}
		}
		if (The.InGameUI.SelectedEntity.HasValue)
		{
			Entity entity = Entity.FindByID(The.InGameUI.SelectedEntity.Value);
			if (entity != null)
			{
				int num = option.LastIndexOf('#');
				string attachorBoneName = option.Substring(num + 1);
				AnimatedModel.OrientAttachedModel(entity, attachorBoneName, testRotation, testTranslation);
			}
		}
	}

	public override void HandleInput()
	{
		if (!The.LoadScreen.IsLoadFinished)
		{
			return;
		}
		Options options = base.Controller.Options;
		if (OptionsDialog.HandleKeys(inputData))
		{
			return;
		}
		bool flag = false;
		if (EnableKeyboardShortcuts)
		{
			flag = inputData.IsKeyTapped(options.ToggleIngameMenu);
			// UNHIDDEN MOD: its key (O unless rebound in the KEYS section) toggles shadows. Read by
			// DateAndTime.ComputeSunAndLight.
			if (UWGame.Mods.UnhiddenMod.Enabled && inputData.IsKeyTapped(UWGame.Mods.UnhiddenMod.ShadowsKey.KeyValue))
			{
				UWGame.Mods.UnhiddenMod.ShadowsDisabled = !UWGame.Mods.UnhiddenMod.ShadowsDisabled;
			}
		}
		bool flag2 = false;
		if (flag && The.InGameUI.MenuDialog.Window.IsVisibleAndActive)
		{
			flag2 = true;
			The.InGameUI.HideInGameMenu();
		}
		if (IsModal)
		{
			return;
		}
		if (flag && !flag2 && !The.InGameUI.MenuDialog.Window.IsVisibleAndActive)
		{
			The.InGameUI.ShowInGameMenu();
		}
		if (The.Sim.Mode != Sim.EngineMode.Game || The.Sim.IsGameOver || !EnableKeyboardShortcuts || flag)
		{
			return;
		}
		if (inputData.IsKeyTapped(Keys.F2))
		{
			The.InGameUI.LogPanel.Hide();
		}
		if (inputData.IsKeyTapped(options.KeyPause1) || inputData.IsKeyTapped(options.KeyPause2) || inputData.IsKeyTapped(options.KeyPause3))
		{
			if (The.Sim.IsPaused)
			{
				ResumeGame();
			}
			else
			{
				PauseGame();
			}
		}
		else if (inputData.IsKeyTapped(options.KeyGameSpeed1))
		{
			SetGameSpeed(Speeds.Normal);
		}
		else if (inputData.IsKeyTapped(options.KeyGameSpeed2))
		{
			SetGameSpeed(Speeds.TwiceNormal);
		}
		else if (inputData.IsKeyTapped(options.KeyGameSpeed3))
		{
			SetGameSpeed(Speeds.FourTimesNormal);
		}
		else if (inputData.IsKeyTapped(options.CloseRosterPanel))
		{
			The.InGameUI.CloseRosterPanel();
		}
	}

	public virtual void StartBuild()
	{
		The.InGameUI.EntitiesBeingPlaced.Clear();
		The.InGameUI.InterfaceMode = InGameInterface.InterfaceState.None;
		The.InGameUI.gui.PlaySound(GUIManager.PlaceBuildingBeep);
	}

	public virtual void SetProduction(string entityTypeKey)
	{
		The.InGameUI.OnSetProduction(entityTypeKey);
	}

	public virtual void OnHuntCreature()
	{
		The.InGameUI.HUDActionPanel.OnHuntCreature();
	}

	public virtual void OnSalvageEntity()
	{
		The.InGameUI.HUDActionPanel.OnSalvageEntity();
	}

	public virtual void OnSpecialAction()
	{
		The.InGameUI.gui.PlaySound(GUIManager.PlaceBuildingBeep);
	}

	public virtual void OnPlaceExpedition()
	{
		The.MapUI.OnPlaceExpedition();
	}

	public virtual void OnDiscardEntity()
	{
		The.InGameUI.HUDActionPanel.OnDiscardEntity();
	}

	public virtual void OnClaimEntity()
	{
		The.InGameUI.HUDActionPanel.OnClaimEntity();
	}

	public virtual void OnScoutArea(Zone scoutArea)
	{
		The.InGameUI.ContextMenu.OnScoutArea(scoutArea);
	}

	public virtual void OnCreateStockpile(Zone zone)
	{
		The.InGameUI.ContextMenu.OnCreateStockpile(zone);
	}

	public virtual void OnForageArea(Zone forageArea)
	{
		The.InGameUI.ContextMenu.OnForageArea(forageArea);
	}

	public virtual void OnHuntArea(Zone huntArea)
	{
		The.InGameUI.ContextMenu.OnForageArea(huntArea);
	}

	public virtual void OnPatrolOrAttackArea(Zone patrolArea)
	{
		The.InGameUI.ContextMenu.OnForageArea(patrolArea);
	}

	public void SetModal(bool value)
	{
		if (value)
		{
			IsModal = true;
			base.Controller.Game.IsMouseVisible = true;
			PauseGame();
		}
		else
		{
			IsModal = false;
			ResumeGame();
		}
	}

	public virtual void SetRenderableOnMemoryFact(MemoryFact memoryFact, Entity entity, Allegiance allegiance)
	{
		if (allegiance == The.InGameUI.UIAllegiance && entity.Renderable != null)
		{
			memoryFact.Renderable = RenderableFactory.Produce(entity.Renderable, memoryFact);
		}
	}

	public virtual void AddTalk(string line, EntityID speaker, Conversation conversation)
	{
		Log.AddTalk(line, speaker, conversation);
	}

	public virtual void SpeakLine(Entity entity, float durationInSeconds, Conversation conversation, string personalityInfusedLine)
	{
		if (The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(entity.EntityID, out var _) == EntityResult.SeenDirectly)
		{
			AddTalk(personalityInfusedLine, entity.EntityID, conversation);
			The.InGameUI.ShowSpokenLine(entity, personalityInfusedLine, durationInSeconds);
		}
	}
}
