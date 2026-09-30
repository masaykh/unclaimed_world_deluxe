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
	public delegate void ButtonClick(object sender);

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

	private int frameCounter;

	public TimeSpan ElapsedTimeBetweenDraws;

	private TimeSpan frameRateElapsedTime = TimeSpan.Zero;

	public GameTime GameTime = new GameTime();

	private InputData inputData;

	private long previousDrawElapsedTotalGameTime;

	private StringBuilder description = new StringBuilder("");

	private Regulator printAllegiancesRegulator;

	private Regulator printPerformanceRegulator;

	private Regulator printEmigrateRollsRegulator;

	private int jobWidth = 10;

	private int outputWidth = 14;

	private int impWidth = 7;

	private int scoreWidth = 7;

	private int scoreNoToolsWidth = 7;

	private int activeWidth = 8;

	private int takerScoreWidth = 8;

	private float timeToSleep;

	private float desiredSleepAmount;

	private int frameCount;

	public bool MapHasMoved = true;

	public static Dictionary<string, Rectangle?[,]> AllConnectedGroundSprites = new Dictionary<string, Rectangle?[,]>();

	private Vector3 testRotation = new Vector3(0f);

	private Vector3 testTranslation = new Vector3(0f);

	private Dictionary<string, Tuple<string, AttachPoint, AttacheePoint>> attacheeOptions;

	private Dictionary<string, AttachPoint> attachorOptions = new Dictionary<string, AttachPoint>();

	private Dictionary<string, string> animationOptions = new Dictionary<string, string>();

	private Dictionary<string, MovementMap> overlayOptions = new Dictionary<string, MovementMap>();

	private Dictionary<string, RegionMap> regionMapOverlayOptions = new Dictionary<string, RegionMap>();

	private Dictionary<string, ThreatMap> threatMapOverlayOptions = new Dictionary<string, ThreatMap>();

	private Dictionary<string, DiscomfortMap> discomfortMapOverlayOptions = new Dictionary<string, DiscomfortMap>();

	private Dictionary<string, ResourceType> resourceTypeOverlayOptions = new Dictionary<string, ResourceType>();

	public const float MaxSpeed = 180f;

	public GraphicsDevice GraphicsDevice => base.Controller.GraphicsDevice;

	public int ScreenWidth => base.Controller.DrawArea.Width;

	public int ScreenHeight => base.Controller.DrawArea.Height;

	public bool IsModal { get; private set; }

	public bool BeginRunWasCalled { get; private set; }

	public static bool isHeadless()
	{
		return The.Client == null;
	}

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

	private void PrintSounds()
	{
		Kensei.Dev.Options.SetSoundsText(AudioManager.PrintSoundsForDebug());
	}

	private void PrintPerformance()
	{
		if (printPerformanceRegulator.IsReady())
		{
			StringBuilder stringBuilder = new StringBuilder();
			The.Sim.CycleManager.PrintPerformance(stringBuilder);
			Kensei.Dev.Options.SetPerformanceText(stringBuilder.ToString());
		}
	}

	private void PrintJobs()
	{
		StringBuilder stringBuilder = new StringBuilder();
		EntityGroup entityGroup = LookUp<EntityGroup, EntityGroupID>.FindByID(The.InGameUI.UIOwner);
		if (entityGroup != null)
		{
			stringBuilder.Append("Job".PadRight(jobWidth));
			stringBuilder.Append("Output".PadRight(outputWidth));
			stringBuilder.Append("Imp.".PadRight(impWidth));
			stringBuilder.Append("Sco.".PadRight(scoreWidth));
			stringBuilder.Append("No tls".PadRight(scoreNoToolsWidth));
			stringBuilder.Append("Active".PadRight(activeWidth));
			stringBuilder.Append("Taker sc.".PadRight(takerScoreWidth));
			stringBuilder.AppendLine();
			List<ProcessJob> list = new List<ProcessJob>();
			foreach (KeyValuePair<EntityType, List<ProcessJob>> productionJob in entityGroup.ProductionJobs)
			{
				foreach (ProcessJob item in productionJob.Value)
				{
					list.Add(item);
				}
			}
			IOrderedEnumerable<ProcessJob> orderedEnumerable = list.OrderByDescending((ProcessJob j) => j.DebugScore);
			stringBuilder.AppendLine("PRODUCTION/GATHER");
			foreach (ProcessJob item2 in orderedEnumerable)
			{
				AppendProcessJob(stringBuilder, entityGroup, item2);
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("HAULING");
			foreach (HaulingJob item3 in entityGroup.HaulingJobs.OrderByDescending((Job j) => j.DebugScore))
			{
				HaulingJobAnyItemOfType haulingJobAnyItemOfType = item3 as HaulingJobAnyItemOfType;
				string value = ((haulingJobAnyItemOfType == null) ? "Haul" : "Haul any");
				stringBuilder.Append(value.Truncate(jobWidth).PadRight(jobWidth));
				string value2 = "";
				if (item3.Item.HasValue)
				{
					The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(item3.Item.Value, out var data);
					if (data != null)
					{
						value2 = data.EntityType.Name;
					}
				}
				else if (haulingJobAnyItemOfType != null)
				{
					value2 = haulingJobAnyItemOfType.RequiredItemType.Name;
				}
				stringBuilder.Append(value2.Truncate(outputWidth - 1).PadRight(outputWidth));
				stringBuilder.Append(EvaluateHaulingJobs.ScoreJobMaterialUrgency(item3, entityGroup).ToString("N2").PadRight(impWidth));
				stringBuilder.Append(item3.DebugScore.ToString("N3").PadRight(scoreWidth));
				stringBuilder.Append(item3.DebugScoreNoTools.ToString("N3").PadRight(scoreNoToolsWidth));
				stringBuilder.Append((item3.TakenBy.Count > 0).ToString().PadRight(activeWidth));
				AppendTakerScore(stringBuilder, takerScoreWidth, item3);
				stringBuilder.AppendLine();
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("OTHER");
			foreach (Job item4 in entityGroup.OtherJobs.OrderByDescending((Job j) => j.DebugScore))
			{
				AppendJob(stringBuilder, item4);
			}
			List<ProcessJob> list2 = new List<ProcessJob>();
			foreach (KeyValuePair<EntityID, List<ProcessJob>> repairJob in entityGroup.RepairJobs)
			{
				foreach (ProcessJob item5 in repairJob.Value)
				{
					list2.Add(item5);
				}
			}
			if (list2.Count > 0)
			{
				IOrderedEnumerable<ProcessJob> orderedEnumerable2 = list2.OrderByDescending((ProcessJob j) => j.DebugScore);
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("REPAIR");
				foreach (ProcessJob item6 in orderedEnumerable2)
				{
					AppendProcessJob(stringBuilder, entityGroup, item6);
				}
			}
			if (entityGroup.FindPreyJobs.Count > 0)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("FIND PREY");
				foreach (FindPreyJob item7 in entityGroup.FindPreyJobs.OrderByDescending((Job j) => j.DebugScore))
				{
					double value3 = item7.ScorePreyImportance(entityGroup);
					AppendJob(stringBuilder, item7, value3);
				}
			}
			if (entityGroup.ScoutingJobs.Count > 0)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("SCOUT");
				foreach (Job item8 in entityGroup.ScoutingJobs.OrderByDescending((Job j) => j.DebugScore))
				{
					AppendJob(stringBuilder, item8);
				}
			}
			if (entityGroup.PatrolJobs.Count > 0)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("PATROL");
				foreach (Job item9 in entityGroup.PatrolJobs.OrderByDescending((Job j) => j.DebugScore))
				{
					AppendJob(stringBuilder, item9);
				}
			}
			if (entityGroup.CheckProcessJobs.Count > 0)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("CHECK PROCESS");
				foreach (Job item10 in entityGroup.CheckProcessJobs.OrderByDescending((Job j) => j.DebugScore))
				{
					AppendJob(stringBuilder, item10);
				}
			}
			if (entityGroup.AttackAreaJobs.Count > 0)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("ATTACK");
				foreach (Job item11 in entityGroup.AttackAreaJobs.OrderByDescending((Job j) => j.DebugScore))
				{
					AppendJob(stringBuilder, item11);
				}
			}
		}
		Kensei.Dev.Options.SetJobsText(stringBuilder.ToString());
	}

	private void AppendJob(StringBuilder description, Job job, double? importance = null)
	{
		string name = job.GetName();
		description.Append(name.Truncate(jobWidth).PadRight(jobWidth));
		description.Append("".PadRight(outputWidth));
		if (importance.HasValue)
		{
			description.Append(importance.Value.ToString("N2").PadRight(impWidth));
		}
		else
		{
			description.Append("".PadRight(impWidth));
		}
		description.Append(job.DebugScore.ToString("N3").PadRight(scoreWidth));
		description.Append(job.DebugScoreNoTools.ToString("N3").PadRight(scoreNoToolsWidth));
		description.Append((job.TakenBy.Count > 0).ToString().PadRight(activeWidth));
		AppendTakerScore(description, takerScoreWidth, job);
		description.AppendLine();
	}

	private void AppendProcessJob(StringBuilder description, EntityGroup owner, ProcessJob job)
	{
		description.Append(job.GetName().Truncate(jobWidth).PadRight(jobWidth));
		string value = ((!job.ProcessType.HasOutput) ? job.ProcessType.Name : job.ProcessType.Outputs[0].FinalEntityTypeToCreate.Name);
		description.Append(value.Truncate(outputWidth - 1).PadRight(outputWidth));
		description.Append(job.GetImportance(owner).ToString("N2").PadRight(impWidth));
		description.Append(job.DebugScore.ToString("N3").PadRight(scoreWidth));
		description.Append(job.DebugScoreNoTools.ToString("N3").PadRight(scoreNoToolsWidth));
		JobsPanel.GetProgressTint(job, out var isActive);
		description.Append(isActive.ToString().PadRight(activeWidth));
		AppendTakerScore(description, takerScoreWidth, job);
		description.AppendLine();
	}

	private static void AppendTakerScore(StringBuilder description, int takerScoreWidth, Job job)
	{
		double? takerScore = job.GetTakerScore();
		if (takerScore.HasValue)
		{
			description.Append(takerScore.Value.ToString("N3").PadRight(takerScoreWidth));
		}
	}

	private void ResetPerformanceCounters(string option, bool? newBool, float? newFloat)
	{
		EventManager.totalComputationAllInstancesInSeconds = 0.0;
		MovementMap.totalComputationAllInstancesInSeconds = 0.0;
		PathPlanner.totalComputationAllInstancesInSeconds = 0.0;
		RegionSearchPlanner.totalComputationAllInstancesInSeconds = 0.0;
		HaulingJobManager.totalComputationAllInstancesInSeconds = 0.0;
		RegionMap.totalComputationAllInstancesInSeconds = 0.0;
	}

	private static void PrintGlobalScriptVariables(StringBuilder description)
	{
		description.AppendLine("Global (site) variables:");
		The.Sim.PlaySite.PrintGlobalScriptVariables(description);
	}

	private static void PrintGlobalConditions(StringBuilder description)
	{
		description.AppendLine();
		description.AppendLine("Allegiance conditions:");
		The.Sim.PlaySite.EventManager.PrintGlobalConditions(description);
	}

	private static void PrintEventsInfo(StringBuilder description)
	{
		description.AppendLine();
		description.AppendLine("Time: " + The.Sim.TotalUnPausedGameTimeInSeconds);
		The.Sim.PlaySite.EventManager.GetCurrentEvents(description);
	}

	private static void PrintAnimStates(StringBuilder states, Entity entity, Renderable renderable)
	{
		states.Append("\nActual AnimationStateFlags:\n");
		states.Append(entity.Renderable.AnimConditions.ToString());
		if (renderable.RenderAsModel.SelectedAnimInfo != null)
		{
			states.Append("\nBest Match Conditions: ");
			if (entity.Renderable.AnimConditions.Equals(renderable.RenderAsModel.SelectedAnimInfo.ConditionSet))
			{
				states.Append("PERFECT");
			}
			states.Append("\n");
			if (renderable.RenderAsModel.SelectedAnimInfo.ConditionSet != null)
			{
				states.Append(renderable.RenderAsModel.SelectedAnimInfo.ConditionSet.ToString());
			}
			if (renderable.RenderAsModel.SelectedAnimInfo.Forbiddens != null && renderable.RenderAsModel.SelectedAnimInfo.Forbiddens.Any())
			{
				states.Append("\nBest Match Forbiddens:\n     ");
				states.Append(renderable.RenderAsModel.SelectedAnimInfo.Forbiddens.StateNames);
			}
			states.Append("\n");
			renderable.RenderAsModel.AppendAnimDebugInfo(states);
		}
		else
		{
			states.Append("\n\n   MATCH FAILED -- curAnimInfo is null -- this is bad.\n\n");
		}
	}

	private static void PrintEntityGoals(Intelligence intelligenceComponent, StringBuilder goals)
	{
		goals.Append("\n");
		goals.Append(intelligenceComponent.Brain.ComposeIndentedString(""));
		goals.Append("Current goal score: ");
		goals.Append(intelligenceComponent.GetCurrentGoalUtility());
		goals.Append("\n");
		goals.Append("Score | Goal");
		goals.Append("\n");
		for (int i = 0; i < intelligenceComponent.TopScoringJobs.Count; i++)
		{
			goals.Append(intelligenceComponent.TopScoringJobs[i].Score.ToString("{0.0000}"));
			goals.Append(" ");
			goals.Append(intelligenceComponent.TopScoringJobs[i].Goal);
			goals.Append("\n");
		}
		goals.Append("\n");
	}

	private static void PrintEntityInfo(StringBuilder description, Entity entity)
	{
		if (entity.PersonEntity != null)
		{
			description.Append(entity.Name);
			description.Append(", Household: ");
			description.Append(entity.PersonEntity.Household.ID.ToString());
			description.Append(", Home: ");
			description.Append(entity.PersonEntity.Household.Home.HasValue ? entity.PersonEntity.Household.Home.Value.ToString() : "None");
			description.Append("\r\n\r\n");
		}
		description.Append("ID: ");
		description.Append(entity.EntityID);
		description.Append("\n");
		description.Append("Name: ");
		description.Append(entity.Name);
		description.Append("\n");
		description.Append("Assigned to job: ");
		description.Append(entity.AssignedToJob);
		description.Append("\n");
		EntityID? inUseBy = The.InGameUI.UIAllegiance.SharedKnowledge.GetInUseBy(entity.ID);
		if (inUseBy.HasValue)
		{
			description.Append("In use by: ");
			description.Append(inUseBy.Value);
			description.Append("\n");
		}
		if (entity.ContainedBy.HasValue)
		{
			description.Append("Contained by: ");
			description.Append(entity.ContainedBy.Value);
			description.Append("\n");
		}
		if (entity.IsOnPlaySite())
		{
			description.Append("Tile Position: ");
			description.Append(entity.MapPosition.Value);
			description.Append("\n");
			description.Append("World Position: ");
			description.Append(entity.PlaySiteLocation.X);
			description.Append(", ");
			description.Append(entity.PlaySiteLocation.Y);
			description.Append(", ");
			description.Append(entity.PlaySiteLocation.Z);
			Point p = MapManager.WorldPosToSubtile(entity.PlaySiteLocation);
			if (The.Map.SubtileIsCompletelyBlocked(The.Map.TerrainCosts[SurfaceType.TransportType.Foot], p))
			{
				description.Append("\n");
				description.Append("\n");
				description.Append("Subtile " + p.ToString() + " is blocked!");
				description.Append("\n");
				description.Append("\n");
			}
			description.Append("Screen Position: ");
			Vector2 vector = The.MapUI.WorldPosToScreen(entity.PlaySiteLocation);
			description.Append(vector.X);
			description.Append(", ");
			description.Append(vector.Y);
			description.Append("\n");
		}
		description.Append("\n");
		description.Append("Time of day: ");
		description.Append(The.Sim.DateAndTime.TimeOfDay);
		description.Append(", Time of year: ");
		description.Append(The.Sim.DateAndTime.TimeOfYear);
		description.Append("\n");
		if (entity.Find<NonLivingEntity>(out var c))
		{
			description.Append("\n");
			description.Append("Condition: ");
			description.Append(c.Condition);
			description.Append("\n");
			description.Append("Max condition: ");
			description.Append(c.MaxCondition);
			description.Append("\n");
		}
		if (entity.Intelligence != null && entity.Intelligence.Brain != null)
		{
			description.Append("Detect agents factor: ");
			description.Append(entity.Intelligence.Brain.GetDetectAgentsFactor(null, requiresExamineAction: false));
			description.Append("\n");
			description.Append("Detect resources factor: ");
			description.Append(entity.Intelligence.Brain.GetDetectResourcesFactor(null, requiresExamineAction: false));
			description.Append("\n");
			if (entity.BiologicalEntity != null)
			{
				description.Append("Exertion level: ");
				description.Append(entity.Intelligence.Brain.GetExertionLevelOfActivity());
				description.Append("\n");
				description.Append("Stealth factor: ");
				description.Append(entity.Intelligence.Brain.GetStealthFactor());
				description.Append("\n");
			}
		}
		if (entity.BiologicalEntity == null)
		{
			return;
		}
		description.Append("Energy level: ");
		description.Append(entity.BiologicalEntity.EnergyLevel);
		description.Append("\n");
		description.Append("Stomach content: ");
		description.Append(entity.BiologicalEntity.StomachContents);
		description.Append("\n");
		foreach (KeyValuePair<string, Need> needs in entity.BiologicalEntity.Needs.NeedsList)
		{
			description.Append(needs.Value.NeedType.KeyName);
			if (needs.Value.NeedType.FoodNeedType != null)
			{
				description.Append("(" + needs.Value.NeedType.FoodNeedType.FoodNutrient + ")");
			}
			description.Append(": " + needs.Value.CurrentLevel);
			if (needs.Value.PhysicalNeed != null && needs.Value.PhysicalNeed.DaysAtZero > 0f)
			{
				description.Append(", days at zero: ");
				description.Append(needs.Value.PhysicalNeed.DaysAtZero);
			}
			description.Append("\n");
		}
		description.Append("\n");
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
			frameCount++;
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

	private void SetDebugAttachorOptions(Entity entity)
	{
		attachorOptions.Clear();
		if (entity.Renderable.RenderAsModel.ModelData.RightHandAttachor != null)
		{
			SetDebugAttachorOption(entity.Renderable.RenderAsModel.ModelData.RightHandAttachor);
		}
		if (entity.Renderable.RenderAsModel.ModelData.LeftHandAttachor != null)
		{
			SetDebugAttachorOption(entity.Renderable.RenderAsModel.ModelData.LeftHandAttachor);
		}
		if (entity.Renderable.RenderAsModel.ModelData.BackAttachor != null)
		{
			SetDebugAttachorOption(entity.Renderable.RenderAsModel.ModelData.BackAttachor);
		}
		if (entity.Renderable.RenderAsModel.ModelData.HelmetAttachor != null)
		{
			SetDebugAttachorOption(entity.Renderable.RenderAsModel.ModelData.HelmetAttachor);
		}
	}

	private string SetDebugAttachorOption(AttachPoint attachor)
	{
		string text = "Attach.Attachor " + attachor.BoneName;
		attachorOptions.Add(text, attachor);
		Kensei.Dev.Options.SetOption(text, boolToSet: false);
		Kensei.Dev.Options.SetOptionCallback(text, TestAttachor);
		return text;
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

	private void SetDebugAttacheeOptions(RenderableType renderableType)
	{
		string assetName = renderableType.RenderAsModelType.AssetName;
		AttachPoint backAttachee = GameData.Instance.AllModels[assetName].BackAttachee;
		if (backAttachee != null)
		{
			SetDebugAttacheeOption(renderableType.KeyName, backAttachee, AttacheePoint.Back);
		}
		backAttachee = GameData.Instance.AllModels[assetName].RightHandAttachee;
		if (backAttachee != null)
		{
			SetDebugAttacheeOption(renderableType.KeyName, backAttachee, AttacheePoint.RightHand);
		}
		backAttachee = GameData.Instance.AllModels[assetName].LeftHandAttachee;
		if (backAttachee != null)
		{
			SetDebugAttacheeOption(renderableType.KeyName, backAttachee, AttacheePoint.LeftHand);
		}
		backAttachee = GameData.Instance.AllModels[assetName].BottomAttachee;
		if (backAttachee != null)
		{
			SetDebugAttacheeOption(renderableType.KeyName, backAttachee, AttacheePoint.Bottom);
		}
	}

	private string SetDebugAttacheeOption(string renderableTypeKey, AttachPoint attacheePoint, AttacheePoint attacheePointName)
	{
		string text = "Attach." + renderableTypeKey + attacheePoint.BoneName;
		Kensei.Dev.Options.SetOption(text, boolToSet: false);
		Kensei.Dev.Options.SetOptionCallback(text, TestAttachee);
		attacheeOptions.Add(text, new Tuple<string, AttachPoint, AttacheePoint>(renderableTypeKey, attacheePoint, attacheePointName));
		return text;
	}

	private void TestAttachee(string option, bool? newBool, float? newFloat)
	{
		if (!newBool.HasValue || !The.InGameUI.SelectedEntity.HasValue)
		{
			return;
		}
		Entity entity = Entity.FindByID(The.InGameUI.SelectedEntity.Value);
		if (entity == null || newBool != true)
		{
			return;
		}
		Tuple<string, AttachPoint, AttacheePoint> tuple = attacheeOptions[option];
		string item = tuple.Item1;
		_ = tuple.Item2.BoneName;
		AttachPoint item2 = attacheeOptions[option].Item2;
		AttacheePoint item3 = tuple.Item3;
		AnimConditionInfo animCondition = FindMatchingAnimCondition(entity, entity.Renderable.RenderAsModel.GetCurrentMainAnimation(), attacheeOptions[option].Item1, item3);
		foreach (KeyValuePair<string, AttachPoint> attachorOption in attachorOptions)
		{
			if (Kensei.Dev.Options.GetOption(attachorOption.Key))
			{
				AttachPoint value = attachorOption.Value;
				Renderable freeAttachableRenderable = Renderer.GetFreeAttachableRenderable(item);
				RenderAsModel.GetAttachTransformations(freeAttachableRenderable, value, item3, animCondition, out var _, out var translation, out var rotation);
				entity.Renderable.AttachObject(freeAttachableRenderable.RenderAsModel, value, item2, null, translation, rotation);
				break;
			}
		}
	}

	private void TestAttachor(string option, bool? newBool, float? newFloat)
	{
		if (!The.InGameUI.SelectedEntity.HasValue)
		{
			return;
		}
		Entity entity = Entity.FindByID(The.InGameUI.SelectedEntity.Value);
		if (entity == null)
		{
			return;
		}
		AttachPoint attachPoint = attachorOptions[option];
		_ = attachPoint.BoneName;
		if (newBool == true)
		{
			foreach (KeyValuePair<string, AttachPoint> attachorOption in attachorOptions)
			{
				if (attachorOption.Key != option)
				{
					Kensei.Dev.Options.SetOption(attachorOption.Key, boolToSet: false);
					entity.Renderable.RemoveAndRetireAllAttachedModels(attachorOptions[attachorOption.Key], null);
				}
			}
			Kensei.Dev.Options.RemoveOptionsStartingWith("Attach.Rotate");
			Kensei.Dev.Options.RemoveOptionsStartingWith("Attach.Translate");
			{
				foreach (KeyValuePair<string, Tuple<string, AttachPoint, AttacheePoint>> attacheeOption in attacheeOptions)
				{
					if (Kensei.Dev.Options.GetOption(attacheeOption.Key))
					{
						AttachPoint item = attacheeOption.Value.Item2;
						AttacheePoint item2 = attacheeOption.Value.Item3;
						AnimConditionInfo animConditionInfo = FindMatchingAnimCondition(entity, entity.Renderable.RenderAsModel.GetCurrentMainAnimation(), attacheeOption.Value.Item1, item2);
						Renderable freeAttachableRenderable = Renderer.GetFreeAttachableRenderable(attacheeOption.Value.Item1);
						RenderAsModel.GetAttachTransformations(freeAttachableRenderable, attachPoint, item2, animConditionInfo, out var attacheePoint, out var translation, out var rotation, out var appliedRotationTransform, out var appliedTranslationTransform);
						entity.Renderable.AttachObject(freeAttachableRenderable.RenderAsModel, attachPoint, item, null, translation, rotation);
						testRotation = Common.WrapVectorBetweenMinusNAndN(rotation, 180f);
						string text = "";
						string text2 = "#";
						string text3 = "";
						text = ", " + text2 + attachPoint.BoneName;
						switch (appliedRotationTransform)
						{
						case RenderAsModel.AppliedAttachableTransforms.Attachee:
							text3 = ", Source: " + attacheePoint.BoneName;
							break;
						case RenderAsModel.AppliedAttachableTransforms.Attachor:
							text3 = ", Source: Attachor/default";
							break;
						case RenderAsModel.AppliedAttachableTransforms.Animation:
							text3 = ", Source: Anim condition " + animConditionInfo.ConditionSet.ToString();
							break;
						}
						string optionName = "Attach.Rotate X" + text3 + text;
						Kensei.Dev.Options.SetOption(optionName, testRotation.X, -180f, 180f);
						Kensei.Dev.Options.SetOptionCallback(optionName, OrientAttachedModel);
						string optionName2 = "Attach.Rotate Y" + text3 + text;
						Kensei.Dev.Options.SetOption(optionName2, testRotation.Y, -180f, 180f);
						Kensei.Dev.Options.SetOptionCallback(optionName2, OrientAttachedModel);
						string optionName3 = "Attach.Rotate Z" + text3 + text;
						Kensei.Dev.Options.SetOption(optionName3, testRotation.Z, -180f, 180f);
						Kensei.Dev.Options.SetOptionCallback(optionName3, OrientAttachedModel);
						testTranslation = translation;
						testTranslation = Common.WrapVectorBetweenMinusNAndN(testTranslation, 10f);
						text3 = "";
						switch (appliedTranslationTransform)
						{
						case RenderAsModel.AppliedAttachableTransforms.Attachee:
							text3 = ", Source: " + attacheePoint.BoneName;
							break;
						case RenderAsModel.AppliedAttachableTransforms.Attachor:
							text3 = ", Source: Attachor/default";
							break;
						case RenderAsModel.AppliedAttachableTransforms.Animation:
							text3 = ", Source: Anim condition " + animConditionInfo.ConditionSet.ToString();
							break;
						}
						string optionName4 = "Attach.Translate X" + text3 + text;
						Kensei.Dev.Options.SetOption(optionName4, testTranslation.X, -20f, 20f);
						Kensei.Dev.Options.SetOptionCallback(optionName4, OrientAttachedModel);
						string optionName5 = "Attach.Translate Y" + text3 + text;
						Kensei.Dev.Options.SetOption(optionName5, testTranslation.Y, -20f, 20f);
						Kensei.Dev.Options.SetOptionCallback(optionName5, OrientAttachedModel);
						string optionName6 = "Attach.Translate Z" + text3 + text;
						Kensei.Dev.Options.SetOption(optionName6, testTranslation.Z, -20f, 20f);
						Kensei.Dev.Options.SetOptionCallback(optionName6, OrientAttachedModel);
						break;
					}
				}
				return;
			}
		}
		Kensei.Dev.Options.RemoveOptionsStartingWith("Attach.Rotate");
		Kensei.Dev.Options.RemoveOptionsStartingWith("Attach.Translate");
		entity.Renderable.RemoveAndRetireAllAttachedModels(attachPoint, null);
	}

	private AnimConditionInfo FindMatchingAnimCondition(Entity entity, string animKey, string attachedRenderableTypeKey, AttacheePoint? attacheePoint)
	{
		if (entity.EntityType.RenderableTypeMode.RenderAsModelType.AnimConditions != null)
		{
			AnimConditionInfo[] animConditions = entity.EntityType.RenderableTypeMode.RenderAsModelType.AnimConditions;
			foreach (AnimConditionInfo animConditionInfo in animConditions)
			{
				if (animConditionInfo.AttachPoints == null || animConditionInfo.SoundAndAnimationSet == null || animConditionInfo.SoundAndAnimationSet.BaseAnimations == null || !animConditionInfo.SoundAndAnimationSet.BaseAnimations.Contains(animKey))
				{
					continue;
				}
				AnimConditionInfo.AttachPointData[] attachPoints = animConditionInfo.AttachPoints;
				foreach (AnimConditionInfo.AttachPointData attachPointData in attachPoints)
				{
					AttacheePoint? attacheePoint2 = attachPointData.AttacheePoint;
					AttacheePoint? attacheePoint3 = attacheePoint;
					if (attacheePoint2.GetValueOrDefault() == attacheePoint3.GetValueOrDefault() && attacheePoint2.HasValue == attacheePoint3.HasValue && attachPointData.RenderableTypeKey == attachedRenderableTypeKey)
					{
						return animConditionInfo;
					}
				}
			}
		}
		return null;
	}

	private void TestAnims(string option, bool? newBool, float? newFloat)
	{
		if (!The.InGameUI.SelectedEntity.HasValue)
		{
			return;
		}
		Entity entity = Entity.FindByID(The.InGameUI.SelectedEntity.Value);
		if (entity == null || !entity.Intelligence.DisableAI || entity.Renderable == null || entity.Renderable.RenderAsModel == null)
		{
			return;
		}
		if (newBool == true)
		{
			Looping looping = Looping.Yes;
			if (option.StartsWith("Anim.Test anim 1"))
			{
				looping = ((!Kensei.Dev.Options.GetOption("Anim.Test anim 1: Looping")) ? Looping.No : Looping.Yes);
			}
			else if (option.StartsWith("Anim.Test anim 2"))
			{
				looping = ((!Kensei.Dev.Options.GetOption("Anim.Test anim 2: Looping")) ? Looping.No : Looping.Yes);
			}
			else if (option.StartsWith("Anim.Test anim 3"))
			{
				looping = ((!Kensei.Dev.Options.GetOption("Anim.Test anim 3: Looping")) ? Looping.No : Looping.Yes);
			}
			entity.Renderable.RenderAsModel.StartMainAnimation(animationOptions[option], Playback.Forwards, StartingPoint.FromBeginning, BlendMode.NoBlending, 1f, looping);
		}
		else
		{
			entity.Renderable.RenderAsModel.StopMainAnimation();
		}
	}

	private void MovemapClick(string option, bool? newBool, float? newFloat)
	{
		MovementMap item = overlayOptions[option];
		bool? flag = newBool;
		bool flag2 = true;
		if (flag == true == flag2 && flag.HasValue && !The.MapUI.Overlays.Contains(item))
		{
			The.MapUI.Overlays.Add(item);
		}
		else
		{
			The.MapUI.Overlays.Remove(item);
		}
	}

	private void RegionMapClick(string option, bool? newBool, float? newFloat)
	{
		RegionMap item = regionMapOverlayOptions[option];
		bool? flag = newBool;
		bool flag2 = true;
		if (flag == true == flag2 && flag.HasValue && !The.MapUI.Overlays.Contains(item))
		{
			The.MapUI.Overlays.Add(item);
		}
		else
		{
			The.MapUI.Overlays.Remove(item);
		}
	}

	private void ThreatMapClick(string option, bool? newBool, float? newFloat)
	{
		ThreatMap item = threatMapOverlayOptions[option];
		bool? flag = newBool;
		bool flag2 = true;
		if (flag == true == flag2 && flag.HasValue && !The.MapUI.Overlays.Contains(item))
		{
			The.MapUI.Overlays.Add(item);
		}
		else
		{
			The.MapUI.Overlays.Remove(item);
		}
	}

	private void DiscomfortMapClick(string option, bool? newBool, float? newFloat)
	{
		DiscomfortMap item = discomfortMapOverlayOptions[option];
		bool? flag = newBool;
		bool flag2 = true;
		if (flag == true == flag2 && flag.HasValue && !The.MapUI.Overlays.Contains(item))
		{
			The.MapUI.Overlays.Add(item);
		}
		else
		{
			The.MapUI.Overlays.Remove(item);
		}
	}

	private void TestSwarmers(string option, bool? newBool, float? newFloat)
	{
		Allegiance allegiance = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:swarmer"]);
		Expedition firstExpedition = The.InGameUI.UIAllegiance.GetFirstExpedition();
		for (int i = 0; i < 10; i++)
		{
			for (int j = 0; j < 5; j++)
			{
				PlaceGameEntities.ImmobilizeEntity(PlaceGameEntities.PlaceAnimal("entity:swarmer", Reproduction.Male, MapManager.WorldPosToTile(new Vector3(firstExpedition.Location.Value.X + (float)i * 5f, firstExpedition.Location.Value.Y + (float)j * 5f, 0f)), 20f, allegiance));
			}
		}
	}

	private void TestTwinkler(string option, bool? newBool, float? newFloat)
	{
		Allegiance allegiance = new Allegiance(AllegianceType.Other, GameData.Instance.AllEntityTypes["entity:twinkler"]);
		Expedition firstExpedition = The.InGameUI.UIAllegiance.GetFirstExpedition();
		PlaceGameEntities.ImmobilizeEntity(PlaceGameEntities.PlaceAnimal("entity:twinkler", Reproduction.Male, MapManager.WorldPosToTile(new Vector3(firstExpedition.Location.Value.X, firstExpedition.Location.Value.Y, 0f)), 20f, allegiance));
	}

	private void TestAdvanceTime(string option, bool? newBool, float? newFloat)
	{
		The.Sim.AdvanceTime(DateAndTime.secondsPerDay);
	}

	private void TestSetGunsProduced(string option, bool? newBool, float? newFloat)
	{
		Allegiance uIAllegiance = The.InGameUI.UIAllegiance;
		EntityType entityType = GameData.Instance.AllEntityTypes["item:gunpowderRifle"];
		uIAllegiance.Statistics.AddProductionEvent(entityType, ProductionStatistics.StatTypes.Produced, 10);
		EntityType entityType2 = GameData.Instance.AllEntityTypes["item:blackPowderRifleAmmo"];
		uIAllegiance.Statistics.AddProductionEvent(entityType2, ProductionStatistics.StatTypes.Produced, 20);
	}

	private void TestSetHidesProduced(string option, bool? newBool, float? newFloat)
	{
		Allegiance uIAllegiance = The.InGameUI.UIAllegiance;
		EntityType entityType = GameData.Instance.AllEntityTypes["item:whipjawTannedHide"];
		uIAllegiance.Statistics.AddProductionEvent(entityType, ProductionStatistics.StatTypes.Produced, 40);
		entityType = GameData.Instance.AllEntityTypes["item:megapodTannedHide"];
		uIAllegiance.Statistics.AddProductionEvent(entityType, ProductionStatistics.StatTypes.Produced, 20);
		entityType = GameData.Instance.AllEntityTypes["item:thunderChickenTannedHide"];
		uIAllegiance.Statistics.AddProductionEvent(entityType, ProductionStatistics.StatTypes.Produced, 50);
	}

	private void TestRatingsAndPopulation(string option, bool? newBool, float? newFloat)
	{
		SpawnFood();
		SpawnGunsAndAmmo();
		SpawnUpgradedHuts();
		SpawnComfortItems();
		for (int i = 0; i < 20; i++)
		{
			SpawnNewMember();
		}
	}

	private void TestFood(string option, bool? newBool, float? newFloat)
	{
		SpawnFood();
	}

	private static void SpawnFood()
	{
		Expedition firstExpedition = The.InGameUI.UIAllegiance.GetFirstExpedition();
		for (int i = 0; i < 100; i++)
		{
			PlaceGameEntities.AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:smokedCarbonTail"]), firstExpedition.Location.Value);
		}
	}

	private void TestCoilRiflesAndAmmo(string option, bool? newBool, float? newFloat)
	{
		SpawnGunsAndAmmo();
	}

	private static void SpawnGunsAndAmmo()
	{
		Expedition firstExpedition = The.InGameUI.UIAllegiance.GetFirstExpedition();
		for (int i = 0; i < 20; i++)
		{
			PlaceGameEntities.AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:coilRifle"]), firstExpedition.Location.Value);
			Entity item = new Entity(GameData.Instance.AllEntityTypes["item:coilRifleAmmo"]);
			PlaceGameEntities.AddColonyItem(item, firstExpedition.Location.Value);
			PlaceGameEntities.AddColonyItem(item, firstExpedition.Location.Value);
		}
	}

	private void TestBushDragonCarcass(string option, bool? newBool, float? newFloat)
	{
		SpawnBushDragonCarcass();
	}

	private void TestUpgradedHuts(string option, bool? newBool, float? newFloat)
	{
		SpawnUpgradedHuts();
	}

	private static void SpawnComfortItems()
	{
		Expedition firstExpedition = The.InGameUI.UIAllegiance.GetFirstExpedition();
		for (int i = 0; i < 20; i++)
		{
			PlaceGameEntities.AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:crystalBrandy"]), firstExpedition.Location.Value);
		}
	}

	private static void SpawnBushDragonCarcass()
	{
		Expedition firstExpedition = The.InGameUI.UIAllegiance.GetFirstExpedition();
		PlaceGameEntities.AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:bushDragonCarcass"])
		{
			Bulk = 1f
		}, firstExpedition.Location.Value);
	}

	private static void SpawnUpgradedHuts()
	{
		Expedition firstExpedition = The.InGameUI.UIAllegiance.GetFirstExpedition();
		Point point = MapManager.WorldPosToTile(firstExpedition.Location.Value);
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				Entity entity = PlaceGameEntities.AddFinishedStructure("structure:clayHut", new Point(point.X + i * 2 + 2, point.Y + j * 2 + 2), firstExpedition);
				UpgradeCategory upgradeCategory = GameData.Instance.AllUpgradeCategories["furniture4People"];
				PlaceGameEntities.AddColonyItemUpgrade("item:furniture4People", entity, upgradeCategory);
				firstExpedition.OwnedEntities.SetUpgrade(entity.EntityID, upgradeCategory, GameData.Instance.AllEntityTypes["item:furniture4People"]);
				upgradeCategory = GameData.Instance.AllUpgradeCategories["bedsOrMats4People"];
				PlaceGameEntities.AddColonyItemUpgrade("item:beds4People", entity, upgradeCategory);
				firstExpedition.OwnedEntities.SetUpgrade(entity.EntityID, upgradeCategory, GameData.Instance.AllEntityTypes["item:beds4People"]);
			}
		}
	}

	private void TestMember(string option, bool? newBool, float? newFloat)
	{
		SpawnNewMember();
	}

	private static void SpawnNewMember()
	{
		Expedition firstExpedition = The.InGameUI.UIAllegiance.GetFirstExpedition();
		PlaceGameEntities.GetBob(MapManager.WorldPosToTile(firstExpedition.Location.Value), firstExpedition);
	}

	private void TestSpraySentries(string option, bool? newBool, float? newFloat)
	{
		Expedition firstExpedition = The.InGameUI.UIAllegiance.GetFirstExpedition();
		for (int i = 0; i < 5; i++)
		{
			Vector3 location = firstExpedition.Location.Value + new Vector3((float)i * 48f - 48f, -48f, 0f);
			SpawnSentry(firstExpedition, location);
		}
		for (int j = 0; j < 5; j++)
		{
			Vector3 location2 = firstExpedition.Location.Value + new Vector3(-48f, (float)j * 48f - 48f, 0f);
			SpawnSentry(firstExpedition, location2);
		}
		for (int k = 0; k < 20; k++)
		{
			PlaceGameEntities.AddColonyItem(new Entity(GameData.Instance.AllEntityTypes["item:bushDragonCartridge"]), firstExpedition.Location.Value);
		}
	}

	private static void TestSpawnComfortItems(string option, bool? newBool, float? newFloat)
	{
		SpawnComfortItems();
	}

	private static Vector3 SpawnSentry(Expedition exp, Vector3 location)
	{
		Entity entity = PlaceGameEntities.AddFinishedStructure("structure:sprayGunSentry", null, exp, flipHorizontally: false, location);
		Entity entity2 = new Entity(GameData.Instance.AllEntityTypes["item:bushDragonCartridge"]);
		PlaceGameEntities.AddColonyItem(entity2, new Vector3(50f, 50f, 0f));
		entity.Parts[0].Parts.FirstOrDefault((Entity p) => p.EntityType.KeyName == "item:sentrySprayGun").Contains.AddToContain(entity2, out var _);
		return location;
	}

	private void PopulateAllegiances()
	{
		if (!printAllegiancesRegulator.IsReady())
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Allegiance allegiance in The.Sim.PlaySite.Allegiances)
		{
			string text = allegiance.Name ?? allegiance.KeyName ?? allegiance.RepresentativeEntityType.PluralName;
			stringBuilder.Append(text + ": " + allegiance.Members.Count);
			int? maxPopulationMembers = allegiance.GetMaxPopulationMembers();
			if (maxPopulationMembers.HasValue)
			{
				stringBuilder.Append("/" + maxPopulationMembers.Value);
			}
			stringBuilder.AppendLine();
		}
		Kensei.Dev.Options.SetAllegiancesText(stringBuilder.ToString());
	}

	private void PopulateTestEvents(List<ActionSets> actions, List<PolledEventType> polled)
	{
		Kensei.Dev.Options.PopulateTestEvents(actions);
		Kensei.Dev.Options.PopulatePolledTestEvents(polled);
	}

	public void ScaleSelectedRenderable(string option, bool? newBool, float? newFloat)
	{
		if (!newFloat.HasValue || !The.InGameUI.SelectedEntity.HasValue)
		{
			return;
		}
		Entity entity = Entity.FindByID(The.InGameUI.SelectedEntity.Value);
		if (entity == null)
		{
			return;
		}
		Renderable renderable = entity.Renderable;
		if (renderable != null)
		{
			RenderAsModel renderAsModel = renderable.RenderAsModel;
			if (renderAsModel != null)
			{
				renderAsModel.FinalModelScale = newFloat.Value;
			}
		}
	}

	public void SetGaitMinimumSpeed(string option, bool? newBool, float? newFloat)
	{
		if (newFloat.HasValue && The.InGameUI.SelectedEntity.HasValue && Entity.FindByID(The.InGameUI.SelectedEntity.Value) != null)
		{
			option.Split(new string[1] { " - " }, StringSplitOptions.RemoveEmptyEntries);
		}
	}

	public void YawSelectedRenderable(string option, bool? newBool, float? newFloat)
	{
		if (newFloat.HasValue && The.InGameUI.SelectedEntity.HasValue)
		{
			Entity entity = Entity.FindByID(The.InGameUI.SelectedEntity.Value);
			if (entity != null)
			{
				entity.SetRotationAndDir(newFloat.Value);
				entity.Renderable.SetToParentLocation();
			}
		}
	}

	private void DeveloperDialogSelectedEntityChangedEvent(EntityID? oldEntityID, EntityID? newEntity)
	{
		Kensei.Dev.Options.RemoveOptionsStartingWith("Attach.Attachor");
		Kensei.Dev.Options.RemoveOptionsStartingWith("Attach.Scale");
		Kensei.Dev.Options.RemoveOptionsStartingWith("Attach.Rotate");
		Kensei.Dev.Options.RemoveOptionsStartingWith("Attach.Translate");
		Kensei.Dev.Options.RemoveOptionsStartingWith("Attach.Pivot");
		string text = "Gait tuning.";
		string text2 = "Gait tuning.Entity speed";
		Kensei.Dev.Options.RemoveOptionsStartingWith(text);
		Kensei.Dev.Options.RemoveOptionsStartingWith(text2);
		regionMapOverlayOptions.Clear();
		overlayOptions.Clear();
		discomfortMapOverlayOptions.Clear();
		threatMapOverlayOptions.Clear();
		Kensei.Dev.Options.RemoveOptionsStartingWith("Overlays.Move map ");
		Kensei.Dev.Options.RemoveOptionsStartingWith("Overlays.Region map ");
		Kensei.Dev.Options.RemoveOptionsStartingWith("Overlays.Threat map ");
		Kensei.Dev.Options.RemoveOptionsStartingWith("Overlays.Discomfort map ");
		Kensei.Dev.Options.RemoveOptionsStartingWith("Anim tuning.Idle");
		The.MapUI.Overlays.Clear();
		if (oldEntityID.HasValue)
		{
			Entity entity = Entity.FindByID(oldEntityID.Value);
			if (entity != null && entity.Renderable.RenderAsModel != null && entity.Locomotor != null)
			{
				entity.Locomotor.MaximumSpeedDebugOnly = null;
			}
		}
		if (The.InGameUI.SelectedEntity.HasValue)
		{
			Entity entity2 = Entity.FindByID(The.InGameUI.SelectedEntity.Value);
			if (entity2 == null || entity2.Renderable == null)
			{
				return;
			}
			if (entity2.Renderable.RenderAsModel != null && entity2.Locomotor != null)
			{
				Kensei.Dev.Options.SetOption("Attach.Scale Selected Renderable", entity2.Renderable.RenderAsModel.FinalModelScale, 0.1f, 16f);
				Kensei.Dev.Options.SetOptionCallback("Attach.Scale Selected Renderable", ScaleSelectedRenderable);
				float val = Common.WrapAngleBetweenMinusPiAndPi(entity2.Rotation);
				Kensei.Dev.Options.SetOption("Attach.Pivot Selected Renderable", val, -(float)Math.PI, (float)Math.PI);
				Kensei.Dev.Options.SetOptionCallback("Attach.Pivot Selected Renderable", YawSelectedRenderable);
				SetDebugAttachorOptions(entity2);
				foreach (KeyValuePair<string, AttachPoint> attachorOption in attachorOptions)
				{
					foreach (KeyValuePair<string, Tuple<string, AttachPoint, AttacheePoint>> attacheeOption in attacheeOptions)
					{
						if (AnimatedModel.HasAttachedModel(entity2, attacheeOption.Value.Item1, attachorOption.Value.BoneName, attacheeOption.Value.Item2.BoneName))
						{
							Kensei.Dev.Options.SetOption(attachorOption.Key, boolToSet: true);
						}
					}
				}
				if (entity2.Locomotor.LeggedLocomotor != null)
				{
					_ = entity2.EntityType.LocomotorType.LeggedLocomotorType;
				}
			}
			if (entity2.Intelligence != null)
			{
				Kensei.Dev.Options.SetOption("Anim.Disable AI", entity2.Intelligence.DisableAI);
				SetDebugOverlayOptions(entity2);
			}
			else
			{
				Kensei.Dev.Options.RemoveOptionsStartingWith("Anim.Disable AI");
			}
			if (entity2.Renderable.RenderAsModel != null && entity2.Locomotor != null)
			{
				Kensei.Dev.Options.SetOption("Anim.Rotate slowly", entity2.Locomotor.RotateSlowly);
			}
			else
			{
				Kensei.Dev.Options.RemoveOptionsStartingWith("Anim.Rotate slowly");
			}
			if (entity2.Renderable.RenderAsModel != null && entity2.Locomotor != null && entity2.Locomotor.LeggedLocomotor != null)
			{
				Kensei.Dev.Options.SetOption("Anim.Wander", entity2.Locomotor.LeggedLocomotor.TestWander);
			}
			else
			{
				Kensei.Dev.Options.RemoveOptionsStartingWith("Anim.Wander");
			}
			if (entity2.Renderable.RenderAsModel != null && entity2.Locomotor != null)
			{
				Kensei.Dev.Options.SetOption(text2, entity2.Locomotor.MaximumSpeedDebugOnly.HasValue ? entity2.Locomotor.MaximumSpeedDebugOnly.Value : 5f, 5f, 180f);
				Kensei.Dev.Options.SetOptionCallback(text2, delegate(string s, bool? b, float? f)
				{
					entity2.Locomotor.MaximumSpeedDebugOnly = f.Value;
				});
				if (entity2.EntityType.RenderableTypeMode.RenderAsModelType.GaitAnimations != null)
				{
					foreach (KeyValuePair<string, GaitAnimationBracket[]> gaitAnimation in entity2.EntityType.RenderableTypeMode.RenderAsModelType.GaitAnimations)
					{
						GaitAnimationBracket[] value = gaitAnimation.Value;
						foreach (GaitAnimationBracket gaitAnimationBracket in value)
						{
							GaitAnimationBracket thisAnim = gaitAnimationBracket;
							string optionName = text + " - " + gaitAnimation.Key + " - " + gaitAnimationBracket.AnimationKey + " - minimum speed";
							Kensei.Dev.Options.SetOption(optionName, gaitAnimationBracket.MinimumSpeed, 0f, 180f);
							Kensei.Dev.Options.SetOptionCallback(optionName, delegate(string s, bool? b, float? f)
							{
								thisAnim.MinimumSpeed = f.Value;
							});
							string optionName2 = text + " - " + gaitAnimation.Key + " - " + gaitAnimationBracket.AnimationKey + " - maximum speed";
							Kensei.Dev.Options.SetOption(optionName2, Common.ClampTop(gaitAnimationBracket.MaximumSpeed, 180f), 0f, 180f);
							Kensei.Dev.Options.SetOptionCallback(optionName2, delegate(string s, bool? b, float? f)
							{
								thisAnim.MaximumSpeed = f.Value;
							});
						}
					}
				}
			}
		}
		string text3 = "Anim.Test anim 1: ";
		string text4 = "Anim.Test anim 2: ";
		string text5 = "Anim.Test anim 3: ";
		animationOptions.Clear();
		Kensei.Dev.Options.RemoveOptionsStartingWith(text3);
		Kensei.Dev.Options.RemoveOptionsStartingWith(text4);
		Kensei.Dev.Options.RemoveOptionsStartingWith(text5);
		if (The.InGameUI.SelectedEntity.HasValue)
		{
			Entity entity3 = Entity.FindByID(The.InGameUI.SelectedEntity.Value);
			if (entity3 != null && entity3.Renderable.RenderAsModel != null)
			{
				SetDebugAnimOptions(entity3, text3, text4, text5);
			}
		}
	}

	private void SetDebugOverlayOptions(Entity entity)
	{
		if (The.Sim.Mode != Sim.EngineMode.Game || entity.Intelligence.Allegiance == null || entity.Intelligence.Allegiance.SharedKnowledge.PlaySiteKnowledge.AllMovementMaps == null)
		{
			return;
		}
		foreach (KeyValuePair<ProtectionLevel, Dictionary<EntityType, Dictionary<ThreatStance, MovementMap>>> allMovementMap in entity.Intelligence.Allegiance.SharedKnowledge.PlaySiteKnowledge.AllMovementMaps)
		{
			foreach (KeyValuePair<EntityType, Dictionary<ThreatStance, MovementMap>> item in allMovementMap.Value)
			{
				foreach (KeyValuePair<ThreatStance, MovementMap> item2 in item.Value)
				{
					string text = "Overlays.Move map " + item2.Value.IDName;
					overlayOptions.Add(text, item2.Value);
					Kensei.Dev.Options.SetOption(text, boolToSet: false);
					Kensei.Dev.Options.SetOptionCallback(text, MovemapClick);
				}
			}
		}
		foreach (KeyValuePair<ProtectionLevel, Dictionary<EntityType, Dictionary<ThreatStance, MovementMap>>> allMovementMap2 in entity.Intelligence.Allegiance.SharedKnowledge.PlaySiteKnowledge.AllMovementMaps)
		{
			foreach (KeyValuePair<EntityType, Dictionary<ThreatStance, MovementMap>> item3 in allMovementMap2.Value)
			{
				foreach (KeyValuePair<ThreatStance, MovementMap> item4 in item3.Value)
				{
					SetDebugOverlayRegionMapOption(item4, SurfaceType.TransportType.Foot);
					SetDebugOverlayRegionMapOption(item4, SurfaceType.TransportType.OffRoad);
				}
			}
		}
		foreach (KeyValuePair<ProtectionLevel, Dictionary<EntityType, Dictionary<ThreatStance, MovementMap>>> allMovementMap3 in entity.Intelligence.Allegiance.SharedKnowledge.PlaySiteKnowledge.AllMovementMaps)
		{
			foreach (KeyValuePair<EntityType, Dictionary<ThreatStance, MovementMap>> item5 in allMovementMap3.Value)
			{
				foreach (KeyValuePair<ThreatStance, MovementMap> item6 in item5.Value)
				{
					DiscomfortMap discomfortMap = item6.Value.GetDiscomfortMap();
					string text2 = "Overlays.Discomfort map " + discomfortMap.IDName;
					discomfortMapOverlayOptions.Add(text2, discomfortMap);
					Kensei.Dev.Options.SetOption(text2, boolToSet: false);
					Kensei.Dev.Options.SetOptionCallback(text2, DiscomfortMapClick);
				}
			}
		}
		foreach (KeyValuePair<EntityType, Dictionary<ThreatStance, ThreatMap>> threatMap in entity.Intelligence.Allegiance.SharedKnowledge.PlaySiteKnowledge.ThreatMaps)
		{
			foreach (KeyValuePair<ThreatStance, ThreatMap> item7 in threatMap.Value)
			{
				string text3 = "Overlays.Threat map " + item7.Value.IDName;
				threatMapOverlayOptions.Add(text3, item7.Value);
				Kensei.Dev.Options.SetOption(text3, boolToSet: false);
				Kensei.Dev.Options.SetOptionCallback(text3, ThreatMapClick);
			}
		}
	}

	private void SetDebugOverlayRegionMapOption(KeyValuePair<ThreatStance, MovementMap> kvp3, SurfaceType.TransportType transport)
	{
		if (kvp3.Value.Layers.TryGetValue(transport, out var value))
		{
			string text = "Overlays.Region map " + value.RegionMap.IDName;
			regionMapOverlayOptions.Add(text, value.RegionMap);
			Kensei.Dev.Options.SetOption(text, boolToSet: false);
			Kensei.Dev.Options.SetOptionCallback(text, RegionMapClick);
		}
	}

	private void SetDebugAnimOptions(Entity entity, string animLabel1, string animLabel2, string animLabel3)
	{
		Kensei.Dev.Options.SetOption(animLabel1 + "Looping", boolToSet: true);
		foreach (KeyValuePair<string, AnimationController> animationController in entity.Renderable.RenderAsModel.AnimatedModel.ModelAnimator.AnimationControllers)
		{
			string text = animLabel1 + animationController.Key;
			animationOptions.Add(text, animationController.Key);
			Kensei.Dev.Options.SetOption(text, boolToSet: false);
			Kensei.Dev.Options.SetOptionCallback(text, TestAnims);
		}
		Kensei.Dev.Options.SetOption(animLabel2 + "Looping", boolToSet: true);
		foreach (KeyValuePair<string, AnimationController> animationController2 in entity.Renderable.RenderAsModel.AnimatedModel.ModelAnimator.AnimationControllers)
		{
			string text = animLabel2 + animationController2.Key;
			animationOptions.Add(text, animationController2.Key);
			Kensei.Dev.Options.SetOption(text, boolToSet: false);
			Kensei.Dev.Options.SetOptionCallback(text, TestAnims);
		}
		Kensei.Dev.Options.SetOption(animLabel3 + "Looping", boolToSet: true);
		foreach (KeyValuePair<string, AnimationController> animationController3 in entity.Renderable.RenderAsModel.AnimatedModel.ModelAnimator.AnimationControllers)
		{
			string text = animLabel3 + animationController3.Key;
			animationOptions.Add(text, animationController3.Key);
			Kensei.Dev.Options.SetOption(text, boolToSet: false);
			Kensei.Dev.Options.SetOptionCallback(text, TestAnims);
		}
	}

	public void BuildClicked(object sender)
	{
	}

	private void PlaceThreat_OnPress(string option, bool? newBool, float? newFloat)
	{
		The.InGameUI.InterfaceMode = InGameInterface.InterfaceState.Threat;
	}

	private void Resource_OnPress(string option, bool? newBool, float? newFloat)
	{
		ResourceType item = resourceTypeOverlayOptions[option];
		bool? flag = newBool;
		bool flag2 = true;
		if (flag == true == flag2 && flag.HasValue && !The.MapUI.Overlays.Contains(item))
		{
			The.MapUI.ResourceOverlays.Add(item);
		}
		else
		{
			The.MapUI.ResourceOverlays.Remove(item);
		}
	}

	private void UnblockTile_OnPress(string option, bool? newBool, float? newFloat)
	{
		The.InGameUI.InterfaceMode = InGameInterface.InterfaceState.UnblockSubtile;
	}

	private void BlockTile_OnPress(string option, bool? newBool, float? newFloat)
	{
		The.InGameUI.InterfaceMode = InGameInterface.InterfaceState.BlockSubtile;
		The.InGameUI.SelectedTiles.IterateArea(delegate(TerrainTile selTile)
		{
			if (selTile != null)
			{
				Vector2 vector = MapManager.SubTileToWorldPos(MapManager.TileEdgeToSubtile(selTile.TilePos).ToPoint());
				Vector2 to = vector + new Vector2(32f, 32f);
				MapManager.IterateSubtiles(vector, to, delegate(Vector2 s)
				{
					MapLoader.CreateAndPlaceEntityFromEntityData(new EntityData
					{
						EntityKey = "structure:abatis",
						Location = s.ToVector3()
					}, out var _);
				});
			}
		});
	}

	private void SetEmigrateDecision(Entity man)
	{
		if (man.Intelligence.EmigrateDecider.PreferredMigrationTarget.HasValue)
		{
			man.Intelligence.Memory.SetEmigrateDecision(man.Intelligence.EmigrateDecider.PreferredMigrationTarget.Value, man);
		}
	}

	private void StarveNearDeath_OnPress(string option, bool? newBool, float? newFloat)
	{
		Entity selectedAgentOrFirstPerson = GetSelectedAgentOrFirstPerson();
		if (selectedAgentOrFirstPerson != null)
		{
			SetStarving(selectedAgentOrFirstPerson, nearDeath: true);
		}
	}

	private void Starve_OnPress(string option, bool? newBool, float? newFloat)
	{
		Entity selectedAgentOrFirstPerson = GetSelectedAgentOrFirstPerson();
		if (selectedAgentOrFirstPerson != null)
		{
			SetStarving(selectedAgentOrFirstPerson);
		}
	}

	private void AddCredits(string option, bool? newBool, float? newFloat)
	{
		Allegiance uIAllegiance = The.InGameUI.UIAllegiance;
		uIAllegiance.TradeCredits += (decimal?)500;
	}

	private void IntegrityDamage_OnPress(string option, bool? newBool, float? newFloat)
	{
		if (The.InGameUI.SelectedEntity.HasValue)
		{
			Entity entity = Entity.FindByID(The.InGameUI.SelectedEntity.Value);
			if (entity != null && entity.EntityType.NonLivingType != null)
			{
				entity.NonLivingEntity.DoIntegrityDamage(0.6f);
			}
		}
	}

	private void AddLogMessage_OnPress(string option, bool? newBool, float? newFloat)
	{
		The.Client.Log.AddLogEvent(The.Client.Log.EconomicEvent, null, "Test log event");
	}

	private void Damage_OnPress(string option, bool? newBool, float? newFloat)
	{
		if (The.InGameUI.SelectedEntity.HasValue)
		{
			Entity entity = Entity.FindByID(The.InGameUI.SelectedEntity.Value);
			if (entity != null && entity.EntityType.NonLivingType != null)
			{
				entity.DoDamage(0.6f);
			}
		}
	}

	private void Immobilize_OnPress(string option, bool? newBool, float? newFloat)
	{
		Entity selectedAgentOrFirstPerson = GetSelectedAgentOrFirstPerson();
		if (selectedAgentOrFirstPerson != null && selectedAgentOrFirstPerson.Locomotor != null)
		{
			selectedAgentOrFirstPerson.Locomotor.ToggleImmobilize();
		}
	}

	private void RaiseComfortPrinciples(string option, bool? newBool, float? newFloat)
	{
		Entity selectedAgentOrFirstPerson = GetSelectedAgentOrFirstPerson();
		if (selectedAgentOrFirstPerson != null && selectedAgentOrFirstPerson.PersonEntity != null)
		{
			selectedAgentOrFirstPerson.PersonEntity.Personality.Principles[RatingTypes.Comfort] += 0.1f;
		}
	}

	private void RaiseFoodPrinciples(string option, bool? newBool, float? newFloat)
	{
		Entity selectedAgentOrFirstPerson = GetSelectedAgentOrFirstPerson();
		if (selectedAgentOrFirstPerson != null && selectedAgentOrFirstPerson.PersonEntity != null)
		{
			selectedAgentOrFirstPerson.PersonEntity.Personality.Principles[RatingTypes.Food] += 0.1f;
		}
	}

	private void RaiseSecurityPrinciples(string option, bool? newBool, float? newFloat)
	{
		Entity selectedAgentOrFirstPerson = GetSelectedAgentOrFirstPerson();
		if (selectedAgentOrFirstPerson != null && selectedAgentOrFirstPerson.PersonEntity != null)
		{
			selectedAgentOrFirstPerson.PersonEntity.Personality.Principles[RatingTypes.Security] += 0.1f;
		}
	}

	private void Injure_OnPress(string option, bool? newBool, float? newFloat)
	{
		Entity selectedAgentOrFirstPerson = GetSelectedAgentOrFirstPerson();
		if (selectedAgentOrFirstPerson != null)
		{
			selectedAgentOrFirstPerson.Find<BodyComponent>(out var _);
			selectedAgentOrFirstPerson.LogInjuryDescription("Test injury");
		}
	}

	private void Sleep_OnPress(string option, bool? newBool, float? newFloat)
	{
		Entity selectedAgentOrFirstPerson = GetSelectedAgentOrFirstPerson();
		if (selectedAgentOrFirstPerson != null)
		{
			selectedAgentOrFirstPerson.BiologicalEntity.Needs.NeedsList["sleep"].CurrentLevel = 0f;
			selectedAgentOrFirstPerson.BiologicalEntity.Needs.NeedsList["sleep"].PhysicalNeed.DaysAtZero = 1f;
		}
	}

	private void Emigrate_OnPress(string option, bool? newBool, float? newFloat)
	{
		Entity selectedAgentOrFirstPerson = GetSelectedAgentOrFirstPerson();
		if (selectedAgentOrFirstPerson != null)
		{
			selectedAgentOrFirstPerson.Intelligence.Memory.ResetTimepointForJoiningExpedition();
			SetEmigrateDecision(selectedAgentOrFirstPerson);
		}
	}

	private static Entity GetSelectedAgentOrFirstPerson()
	{
		Entity entity = null;
		if (The.InGameUI.SelectedEntity.HasValue)
		{
			Entity entity2 = Entity.FindByID(The.InGameUI.SelectedEntity.Value);
			if (entity2.Intelligence != null)
			{
				entity = entity2;
			}
		}
		if (entity == null && The.Sim.PlaySite.PlayerAllegiance.Persons.Count > 0)
		{
			entity = The.Sim.PlaySite.PlayerAllegiance.Persons[0];
		}
		return entity;
	}

	private static void SetStarving(Entity man, bool nearDeath = false)
	{
		if (man.BiologicalEntity.Needs.NeedsList.TryGetValue("protein", out var value))
		{
			value.CurrentLevel = 0f;
			if (nearDeath)
			{
				value.PhysicalNeed.DaysAtZero = 2f;
			}
		}
		if (man.BiologicalEntity.Needs.NeedsList.TryGetValue("foodEnergy", out value))
		{
			value.CurrentLevel = 0f;
			if (nearDeath)
			{
				value.PhysicalNeed.DaysAtZero = 2f;
			}
		}
		if (man.BiologicalEntity.Needs.NeedsList.TryGetValue("micronutrients", out value))
		{
			value.CurrentLevel = 0f;
			if (nearDeath)
			{
				value.PhysicalNeed.DaysAtZero = 2f;
			}
		}
		man.BiologicalEntity.AddToStomachContents(-1f);
	}

	private void MergeHouseholds_OnPress(string option, bool? newBool, float? newFloat)
	{
		if (newBool.HasValue && The.Sim.PlaySite.PlayerAllegiance.Persons.Count > 1 && The.Sim.PlaySite.GetFirstPlayerExpedition().Households.Count > 1)
		{
			The.Sim.PlaySite.GetFirstPlayerExpedition().Households[0].MergeHouseholds(The.Sim.PlaySite.GetFirstPlayerExpedition().Households[The.Sim.PlaySite.GetFirstPlayerExpedition().Households.Count - 1]);
		}
	}

	private void ClearTile_OnPress(string option, bool? newBool, float? newFloat)
	{
		if (The.InGameUI.SelectedEntity.HasValue)
		{
			Entity entity = Entity.FindByID(The.InGameUI.SelectedEntity.Value);
			if (entity != null)
			{
				entity.Destroy();
				The.InGameUI.SelectEntity(null);
			}
		}
		else
		{
			The.InGameUI.SelectedTiles.IterateArea(delegate(TerrainTile tile)
			{
				The.Map.ClearTile(tile);
			});
		}
	}

	private void CancelEntityJob_OnPress(string option, bool? newBool, float? newFloat)
	{
		if (The.InGameUI.SelectedEntity.HasValue)
		{
			Entity.FindByID(The.InGameUI.SelectedEntity.Value)?.SendMessage(new Message(null, Message.MessageTypes.CancelJobOrItemInUse, Message.CancelJobKeepVehicle.KeepVehicle));
		}
	}

	private void Kill_OnPress(string option, bool? newBool, float? newFloat)
	{
		if (!The.InGameUI.SelectedEntity.HasValue)
		{
			return;
		}
		Entity entity = Entity.FindByID(The.InGameUI.SelectedEntity.Value);
		if (entity != null)
		{
			if (entity.EntityType.BiologicalType != null)
			{
				entity.Kill(null);
			}
			else
			{
				entity.Destroy();
			}
		}
	}

	private void GodMode_OnPress(string option, bool? newBool, float? newFloat)
	{
		if (newBool == false)
		{
			The.InGameUI.FogMap.DisplayWindow.Show();
		}
		else
		{
			The.InGameUI.FogMap.DisplayWindow.Hide();
		}
	}

	private void FogOfWar_OnPress(string option, bool? newBool, float? newFloat)
	{
		if (newBool == true)
		{
			The.InGameUI.FogMap.DisplayWindow.Show();
		}
		else
		{
			The.InGameUI.FogMap.DisplayWindow.Hide();
		}
	}

	private void Wander_OnPress(string option, bool? newBool, float? newFloat)
	{
		if (!The.InGameUI.SelectedEntity.HasValue)
		{
			return;
		}
		Entity entity = Entity.FindByID(The.InGameUI.SelectedEntity.Value);
		if (entity != null && entity.Renderable.RenderAsModel != null && entity.Locomotor.LeggedLocomotor != null)
		{
			if (newBool == true)
			{
				entity.SendMessage(new Message(null, Message.MessageTypes.CancelJobOrItemInUse, Message.CancelJobKeepVehicle.KeepVehicle));
			}
			entity.Locomotor.LeggedLocomotor.TestWander = newBool.Value;
		}
	}

	private void SetUIToSelectedEntityAllegiance(string option, bool? newBool, float? newFloat)
	{
		if (newBool == true)
		{
			The.InGameUI.SetUIAllegianceToSelectedEntity();
		}
		else
		{
			The.InGameUI.ResetUIAllegiance();
		}
	}

	private void LaunchSelected(string option, bool? newBool, float? newFloat)
	{
		if (newBool == true)
		{
			The.InGameUI.InterfaceMode = InGameInterface.InterfaceState.Launch;
		}
		else
		{
			The.InGameUI.InterfaceMode = InGameInterface.InterfaceState.None;
		}
	}

	private void DisableAI_OnPress(string option, bool? newBool, float? newFloat)
	{
		if (!The.InGameUI.SelectedEntity.HasValue)
		{
			return;
		}
		Entity entity = Entity.FindByID(The.InGameUI.SelectedEntity.Value);
		if (entity != null && entity.Intelligence != null)
		{
			if (newBool == true)
			{
				entity.SendMessage(new Message(null, Message.MessageTypes.CancelJobOrItemInUse, Message.CancelJobKeepVehicle.KeepVehicle));
			}
			entity.Intelligence.DisableAI = newBool.Value;
		}
	}

	private void RotateSlowly_OnPress(string option, bool? newBool, float? newFloat)
	{
		if (The.InGameUI.SelectedEntity.HasValue)
		{
			Entity entity = Entity.FindByID(The.InGameUI.SelectedEntity.Value);
			if (entity != null && entity.Locomotor != null)
			{
				entity.Locomotor.RotateSlowly = newBool.Value;
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

	public void UpdateFrameRate()
	{
		frameRateElapsedTime += The.Sim.GameTime.ElapsedGameTime;
		if (frameRateElapsedTime > TimeSpan.FromSeconds(1.0))
		{
			frameRateElapsedTime -= TimeSpan.FromSeconds(1.0);
			frameRate = frameCounter;
			frameCounter = 0;
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
