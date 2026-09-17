#define TRACE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using GameStateManagement;
using Kensei.Dev;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Feedback;
using UWGame.ClientSide.Interface;
using UWGame.ClientSide.Interface.BuyAndSell;
using UWGame.ClientSide.Interface.Inventory;
using UWGame.ClientSide.Interface.Ledger;
using UWGame.ClientSide.Interface.Overlays;
using UWGame.ClientSide.Interface.Tasks;
using UWGame.ClientSide.Log;
using UWGame.ClientSide.Particles;
using UWGame.ClientSide.Renderables;
using UWGame.Control;
using UWGame.Control.Replays;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.AI.Pathfinding;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.AllGameData.Scenarios;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Collisions;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Entities.Substances;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.GatheringSites;
using UWGame.SimSide.InGameEvents;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.Items;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Overland.Locations;
using UWGame.SimSide.Overland.Missions;
using UWGame.SimSide.Overland.Missions.Templates;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Scenarios;
using UWGame.SimSide.SimEffects;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems;
using UWGame.SimSide.Systems.TimeSlicing;
using UWGame.SimSide.Systems.Triggers;
using UWGame.SimSide.Trees;
using UWGame.SimSide.Vegetation;
using UWGame.SimSide.Vehicles;

namespace UWGame.SimSide;

public class Sim : GameScreen, ISnapshot
{
	public enum SerializeMode
	{
		WriteAndRead,
		NoSerialize,
		Read
	}

	public enum PersonSex
	{
		Male,
		Female
	}

	private enum SeasonPrefix : ulong
	{
		Early,
		Mid,
		Late
	}

	private enum Seasons : ulong
	{
		Spring,
		Summer,
		Autumn,
		Winter
	}

	public enum DayPhases : ulong
	{
		Work,
		Leisure,
		Sleep
	}

	public enum EngineMode
	{
		Game,
		Edit
	}

	private enum QUEUESTATE
	{
		BEGIN,
		LOAD_BASE_DATA,
		LOAD_SCENARIO_DATA,
		LOAD_MAP_DATA,
		LOAD_MOD_DATA,
		Initialize,
		PostDataCompleteInitialize,
		CREATE_META_DATA,
		INIT_CONSTANTS,
		EVERYTHING_ELSE,
		DONE,
		WRITESCENARIOS
	}

	public class FileOpenException : Exception
	{
		public FileOpenException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}

	private enum LoadSaveGameProgressFlag
	{
		NotStarted,
		Ongoing,
		Done
	}

	private class LoadSavedGameParams
	{
		public string LoadFilePath;
	}

	public enum StartGameMode
	{
		Edit,
		DebugNewGame,
		DebugLoadSaved,
		ScenarioNewGame,
		ScenarioLoadSaved
	}

	public enum WaitingFor
	{
		Regions,
		Path
	}

	private Speeds speed = Speeds.Normal;

	public bool IsGameOver;

	public static SerializeMode CurrentSerializeMode = SerializeMode.NoSerialize;

	public TriggerSystem TriggerSystem;

	public Dictionary<EntityID, Tuple<WaitingFor, double>> WaitingAgents = new Dictionary<EntityID, Tuple<WaitingFor, double>>();

	public DateAndTime DateAndTime;

	public const int HasNoLimitValue = -1;

	public List<EntityID> AllStructures = new List<EntityID>();

	public List<EntityID> AllTerrainEntities = new List<EntityID>();

	public List<string> DebugAttackLog = new List<string>();

	public Dictionary<ResourceType, Tuple<NoiseParams, SimplexNoise>> ResourceNoiseSeeds;

	public Site PlaySite;

	private SiteID snapshotPlaySite;

	public World World;

	public Dictionary<ProcessJob, List<GoalDoProduceAtomic>> ProductionJobsRequiringEnergy = new Dictionary<ProcessJob, List<GoalDoProduceAtomic>>();

	public CycleManager CycleManager;

	private SleepyUpdater<Entity> entities;

	private List<EntityID> snapshotEntities;

	private static Collections CollectionOfCollections = new Collections();

	public const double SleepPhaseStarts = 0.923;

	public const double WorkPhaseStarts = 0.23;

	public const double LeisurePhaseStarts = 0.615;

	public Dictionary<EntityID, Dictionary<EntityID, Vector3>> MeleeAttackers = new Dictionary<EntityID, Dictionary<EntityID, Vector3>>();

	public EngineMode Mode;

	public StartGameParams StartGameParams;

	public StartGameMode startGameMode;

	private List<EntityID> snapshotCollisionManagerList;

	private List<Pair<EntityID, Vector2>> snapshotAgentQuadTree;

	public const int FoodStockSize = 2;

	private Regulator performanceRegulator;

	public GameTime GameTime = new GameTime();

	private TimeSpan totalUnPausedGameTime;

	private TimeSpan elapsedTime = TimeSpan.Zero;

	private int entityCounter;

	private RandomGenerator gameplayRandomGenerator;

	private Vector2? snapshotMapWindowWorldPosition;

	private List<EventDialogData> snapshotEvents;

	private OverlaySettings snapshotOverlaySettings;

	private InventorySettings snapshotInventorySettings;

	private TaskSettings snapshotTaskSettings;

	private BuySellPanelSettings snapshotBuySettings;

	private BuySellPanelSettings snapshotSellSettings;

	private FoodProductionSettings snapshotFoodProductionSettings;

	private ProductionSettings snapshotProductionSettings;

	private KillsSettings snapshotKillsSettings;

	private NutrientSheetSettings snapshotNutrientSheetSettings;

	private BaseDataLoader baseDataLoader;

	private DataLoader scenarioDataLoader;

	private QUEUESTATE queueState;

	private Thread loadSaveGameThread;

	private LoadSaveGameProgressFlag loadSaveGameProgressFlag;

	public static bool LoadIsFinished = false;

	public static Exception LoadException = null;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public int beginRunProgress;

	private int geoLayoutEntityProgress;

	private int placeGeoLayoutProgress;

	private const int entitiesPerCycle = 100;

	public const int ProgressAfterSaveLoad = 4;

	private List<string> startGameActionLog = new List<string>();

	private List<string> startGamePopulationSpawnLog = new List<string>();

	private List<EventActionType> sortedStartGameEventActions;

	private int startGameEventActionsProgress;

	public bool IsInNormalGameLoop;

	public Speeds Speed => speed;

	public bool IsPaused => speed == Speeds.Pause;

	private float GameSpeed => speed switch
	{
		Speeds.Normal => 1f, 
		Speeds.TwiceNormal => 2f, 
		Speeds.FourTimesNormal => 4f, 
		_ => 1f, 
	};

	public TimeSpan TotalUnPausedGameTime
	{
		get
		{
			return totalUnPausedGameTime;
		}
		private set
		{
			totalUnPausedGameTime = value;
			TotalUnPausedGameTimeInSeconds = totalUnPausedGameTime.TotalSeconds;
		}
	}

	public double TotalUnPausedGameTimeInSeconds { get; private set; }

	public RandomGenerator GameplayRandomGenerator => gameplayRandomGenerator;

	public bool IsSnapshotted { get; set; }

	public static void AddLookupCollectible(Type t, ILookUpCollectible collection)
	{
		if (!CollectionOfCollections.ContainsKey(t))
		{
			CollectionOfCollections.Add(t, collection);
		}
	}

	public static void RemoveLookupCollectible(Type t)
	{
		CollectionOfCollections.Remove(t);
	}

	public Sim(Controller screenManager, StartGameParams startGameParams, int? randomSeed)
	{
		base.Controller = screenManager;
		StartGameParams = startGameParams;
		base.TransitionOnTime = TimeSpan.FromSeconds(1.5);
		base.TransitionOffTime = TimeSpan.FromSeconds(0.5);
		The.Sim = this;
		if (randomSeed.HasValue)
		{
			gameplayRandomGenerator = new RandomGenerator(randomSeed.Value, RandomGenerator.GeneratorType.Sim);
		}
		else
		{
			gameplayRandomGenerator = new RandomGenerator(RandomGenerator.GeneratorType.Sim);
		}
		CreateLookupCollections();
	}

	private void CreateRegulators()
	{
		performanceRegulator = new Regulator(gameplayRandomGenerator, 0.3, "Sim");
	}

	public bool QueueGameDataAndSimInit()
	{
		Scenario scenario = StartGameParams?.StartScenarioParams?.Scenario;
		switch (queueState)
		{
		case QUEUESTATE.BEGIN:
			baseDataLoader = new BaseDataLoader();
			if (scenario != null)
			{
				scenarioDataLoader = AllScenarioLoader.GetScenarioDataLoader(scenario);
			}
			queueState = QUEUESTATE.LOAD_BASE_DATA;
			break;
		case QUEUESTATE.LOAD_BASE_DATA:
			if (baseDataLoader.QueueInitGameData(scenario))
			{
				The.LoadScreen.Progress("Load scenario data...", 200);
				queueState = QUEUESTATE.LOAD_SCENARIO_DATA;
			}
			break;
		case QUEUESTATE.LOAD_SCENARIO_DATA:
		{
			bool flag = false;
			if (scenarioDataLoader != null)
			{
				if (scenarioDataLoader.QueueInitGameData(scenario))
				{
					flag = true;
				}
			}
			else
			{
				flag = true;
			}
			if (flag)
			{
				The.LoadScreen.Progress("PostDataCompleteInitialize...", 1015);
				queueState = QUEUESTATE.PostDataCompleteInitialize;
			}
			break;
		}
		case QUEUESTATE.PostDataCompleteInitialize:
			GameData.Instance.PostDataCompleteInitialize();
			The.LoadScreen.Progress("Replace data placeholders...", 1015);
			if (CurrentSerializeMode == SerializeMode.WriteAndRead)
			{
				The.LoadScreen.Progress("Write scenarios...", 3015);
				queueState = QUEUESTATE.WRITESCENARIOS;
			}
			else
			{
				The.LoadScreen.Progress("More SimInit stuff...", 50);
				queueState = QUEUESTATE.EVERYTHING_ELSE;
			}
			break;
		case QUEUESTATE.WRITESCENARIOS:
			if (CurrentSerializeMode == SerializeMode.WriteAndRead)
			{
				RGScenarioLoader.Serialize();
			}
			The.LoadScreen.Progress("More SimInit stuff...", 50);
			queueState = QUEUESTATE.EVERYTHING_ELSE;
			break;
		case QUEUESTATE.EVERYTHING_ELSE:
			DateAndTime = new DateAndTime();
			TriggerSystem = new TriggerSystem();
			Locale.Init();
			The.Map = new MapManager();
			CycleManager = new CycleManager();
			InitEntityUpdater();
			CreateRegulators();
			queueState = QUEUESTATE.DONE;
			break;
		case QUEUESTATE.DONE:
			queueState = QUEUESTATE.BEGIN;
			return true;
		}
		return false;
	}

	public override void Init()
	{
		base.Init();
		The.Map.Init();
	}

	public void DoSave(string fullFilePath)
	{
		SnapshotHeader header = new SnapshotHeader(StartGameParams);
		FileStream stream;
		try
		{
			stream = File.Open(fullFilePath, FileMode.Create);
		}
		catch (Exception ex)
		{
			throw new FileOpenException(ex.Message, ex);
		}
		using BinaryWriter writer = new BinaryWriter(new BufferedStream(new GZipStream(stream, CompressionMode.Compress), 65536));
		The.Snapshotter.Save(writer, header, verifyCompleteness: true, snapShotHeader: true, snapShotBody: true);
	}

	public bool LoadSavedGame(string fullFilePath)
	{
		if (loadSaveGameProgressFlag == LoadSaveGameProgressFlag.NotStarted)
		{
			loadSaveGameThread = new Thread(LoadSavedGameInThread);
			loadSaveGameThread.IsBackground = true;
			loadSaveGameProgressFlag = LoadSaveGameProgressFlag.Ongoing;
			LoadIsFinished = false;
			loadSaveGameThread.Priority = ThreadPriority.AboveNormal;
			loadSaveGameThread.Start(new LoadSavedGameParams
			{
				LoadFilePath = fullFilePath
			});
		}
		else if (LoadIsFinished)
		{
			if (loadSaveGameThread != null && loadSaveGameThread.IsAlive && Thread.CurrentThread != loadSaveGameThread)
			{
				loadSaveGameThread.Join();
			}
			loadSaveGameProgressFlag = LoadSaveGameProgressFlag.Done;
			if (LoadException != null)
			{
				throw LoadException;
			}
		}
		return false;
	}

	private static void LoadSavedGameInThread(object parms)
	{
		string loadFilePath = (parms as LoadSavedGameParams).LoadFilePath;
		try
		{
			using BinaryReader reader = new BinaryReader(new BufferedStream(new GZipStream(File.Open(loadFilePath, FileMode.Open), CompressionMode.Decompress), 65536));
			The.Snapshotter.Load(reader);
		}
		catch (Exception innerException)
		{
			LoadIsFinished = true;
			LoadException = new UWException("This Save game failed to load. This can be because the file is out of date. " + Environment.NewLine + Environment.NewLine + "SOLUTION: If your current version of the game is different than the version number on the save file, you can revert to an earlier version of the game. Steam makes it possible to select a different branch/version of the game, and on the Unclaimed World forum on Steam there are instructions on how to do this. " + Environment.NewLine + Environment.NewLine + "If this does not work, you can make a post in the Unclaimed World forum on Steam and paste the error report." + Environment.NewLine + Environment.NewLine, innerException)
			{
				IncludePasteInstructions = false
			};
		}
	}

	public void FreeMemoryBeforeLoad()
	{
		ClearData(clearBaseData: false);
	}

	private void ClearData(bool clearBaseData)
	{
		foreach (ILookUpCollectible value in CollectionOfCollections.Values)
		{
			if (clearBaseData || value.SnapshotThis)
			{
				value.ClearCollection();
			}
		}
		Cyclable.ResetIDCounterNoInvoke();
		ResourceItem.ResetIDCounterNoInvoke();
		Composite.ResetIDCounterNoInvoke();
		HasMembers.ResetIDCounterNoInvoke();
		Detectable.ResetIDCounterNoInvoke();
		MethodCounter.ResetIDCounterNoInvoke();
		IMapCounter.ResetIDCounterNoInvoke();
		Owner.ResetIDCounterNoInvoke();
		HasEntityGroup.ResetIDCounterNoInvoke();
		HasCrops.ResetIDCounterNoInvoke();
		Event.ResetOtherIDCounter();
		TalkEvent.ResetOtherIDCounter();
		ItemStorage.ResetIDCounterNoInvoke();
		GoalDoProduceAtomic.ClearPool();
		GoalTraverseEdgeBetweenWaypointsAtomic.ClearPool();
		GoalDoTakeFiveAtomic.ClearPool();
		BodyPart.ResetBodyPartCounter();
		The.AgentQuadTree = null;
		The.CollisionManager = null;
		The.Map = null;
		The.Sim = null;
	}

	public override void Destroy()
	{
		base.Destroy();
		ClearData(clearBaseData: true);
		GameData.UnloadAllData();
		Options.RemoveAllOptions();
	}

	public void DestroyAllEntities()
	{
		for (int num = The.Sim.PlaySite.Entities.Count - 1; num > 0; num--)
		{
			The.Sim.PlaySite.Entities[num].Destroy(destroyParts: false);
		}
	}

	public override void ExitScreen()
	{
		base.ExitScreen();
		base.Controller.GameEnded();
	}

	public string GetDifficultyKey()
	{
		return The.Sim.StartGameParams.StartScenarioParams?.MainDifficultyKey;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		if (sn.mode != Snapshotter.Mode.Load)
		{
			if (The.Client != null)
			{
				snapshotMapWindowWorldPosition = The.MapUI.MapWindowWorldPosition;
				snapshotEvents = The.Client.EventDialogsData;
				if (The.InGameUI != null)
				{
					snapshotOverlaySettings = The.InGameUI.OverlaySettings;
					snapshotInventorySettings = The.InGameUI.InventorySettings;
					snapshotBuySettings = The.InGameUI.BuySettings;
					snapshotSellSettings = The.InGameUI.SellSettings;
					snapshotTaskSettings = The.InGameUI.TaskSettings;
					snapshotFoodProductionSettings = The.InGameUI.FoodProductionSettings;
					snapshotKillsSettings = The.InGameUI.KillsSettings;
					snapshotProductionSettings = The.InGameUI.ProductionSettings;
					snapshotNutrientSheetSettings = The.InGameUI.NutrientSheetSettings;
				}
				else
				{
					snapshotOverlaySettings = null;
					snapshotInventorySettings = null;
					snapshotBuySettings = null;
					snapshotSellSettings = null;
					snapshotTaskSettings = null;
					snapshotFoodProductionSettings = null;
					snapshotProductionSettings = null;
					snapshotNutrientSheetSettings = null;
				}
			}
			else
			{
				snapshotMapWindowWorldPosition = null;
				snapshotEvents = null;
				snapshotOverlaySettings = null;
				snapshotInventorySettings = null;
			}
		}
		snapshotMapWindowWorldPosition = sn.DoVector2Nullable(snapshotMapWindowWorldPosition);
		snapshotEvents = sn.DoList(snapshotEvents);
		snapshotOverlaySettings = (OverlaySettings)sn.DoISnapshot(snapshotOverlaySettings);
		snapshotInventorySettings = (InventorySettings)sn.DoISnapshot(snapshotInventorySettings);
		snapshotBuySettings = (BuySellPanelSettings)sn.DoISnapshot(snapshotBuySettings);
		snapshotSellSettings = (BuySellPanelSettings)sn.DoISnapshot(snapshotSellSettings);
		snapshotTaskSettings = (TaskSettings)sn.DoISnapshot(snapshotTaskSettings);
		snapshotFoodProductionSettings = (FoodProductionSettings)sn.DoISnapshot(snapshotFoodProductionSettings);
		snapshotProductionSettings = (ProductionSettings)sn.DoISnapshot(snapshotProductionSettings);
		snapshotKillsSettings = (KillsSettings)sn.DoISnapshot(snapshotKillsSettings);
		snapshotNutrientSheetSettings = (NutrientSheetSettings)sn.DoISnapshot(snapshotNutrientSheetSettings);
		if (sn.mode != Snapshotter.Mode.Load)
		{
			snapshotEntities = new List<EntityID>();
			entities.IterateItems(delegate(Entity e)
			{
				snapshotEntities.Add(e.ID);
			});
		}
		snapshotEntities = sn.DoList(snapshotEntities);
		CollectionOfCollections = (Collections)sn.DoISnapshot(CollectionOfCollections);
		AllStructures = sn.DoList(AllStructures);
		AllTerrainEntities = sn.DoList(AllTerrainEntities);
		elapsedTime = sn.DoTimeSpan(elapsedTime);
		entityCounter = sn.DoInt32(entityCounter);
		MeleeAttackers = sn.DoNestedDictionary(MeleeAttackers);
		Mode = sn.DoEnum(Mode);
		ProductionJobsRequiringEnergy = sn.DoMultiMap(ProductionJobsRequiringEnergy);
		startGameActionLog = sn.DoList(startGameActionLog);
		startGamePopulationSpawnLog = sn.DoList(startGamePopulationSpawnLog);
		IsGameOver = sn.DoBool(IsGameOver);
		TotalUnPausedGameTime = sn.DoTimeSpan(TotalUnPausedGameTime);
		TotalUnPausedGameTimeInSeconds = sn.DoDouble(TotalUnPausedGameTimeInSeconds);
		World = (World)sn.DoISnapshot(World);
		DateAndTime = (DateAndTime)sn.DoISnapshot(DateAndTime);
		Cyclable.DoSnapshot(sn);
		Composite.DoSnapshot(sn);
		ResourceItem.DoSnapshot(sn);
		HasMembers.DoSnapshot(sn);
		Detectable.DoSnapshot(sn);
		MethodCounter.DoSnapshot(sn);
		IMapCounter.DoSnapshot(sn);
		HasEntityGroup.DoSnapshot(sn);
		Owner.DoSnapshot(sn);
		HasCrops.DoSnapshot(sn);
		gameplayRandomGenerator = (RandomGenerator)sn.DoISnapshot(gameplayRandomGenerator);
		The.Map = (MapManager)sn.DoISnapshot(The.Map);
		if (sn.mode != Snapshotter.Mode.Load)
		{
			snapshotCollisionManagerList = (from e in The.CollisionManager.GetAllObjects()
				select e.ID).ToList();
			snapshotAgentQuadTree = (from p in The.AgentQuadTree.GetAllObjectsAndPositions()
				select new Pair<EntityID, Vector2>(p.First.ID, p.Second)).ToList();
		}
		The.CollisionManager = (CollisionManager<Entity>)sn.DoISnapshot(The.CollisionManager);
		snapshotCollisionManagerList = sn.DoList(snapshotCollisionManagerList);
		The.AgentQuadTree = (PointQuadTree<Entity>)sn.DoISnapshot(The.AgentQuadTree);
		snapshotAgentQuadTree = sn.DoList(snapshotAgentQuadTree);
		CycleManager = (CycleManager)sn.DoISnapshot(CycleManager);
		snapshotPlaySite = sn.SnapshotID<Site, SiteID>(PlaySite).Value;
		TriggerSystem = (TriggerSystem)sn.DoISnapshot(TriggerSystem);
		sn.Ignore(WaitingAgents);
		sn.Ignore(GameTime);
		sn.Ignore(baseDataLoader);
		sn.Ignore(queueState);
		sn.Ignore(scenarioDataLoader);
		sn.Ignore(StartGameParams);
		sn.Ignore(ResourceNoiseSeeds);
		sn.Ignore(IsPaused);
		sn.Ignore(DebugAttackLog);
		sn.Ignore(CurrentSerializeMode);
		sn.Ignore(base.Controller);
		sn.Ignore(IsInNormalGameLoop);
		sn.Ignore(beginRunProgress);
		sn.Ignore(entities);
		sn.Ignore(speed);
		sn.Ignore(sortedStartGameEventActions);
		sn.Ignore(startGameEventActionsProgress);
		sn.Ignore(LoadException);
		sn.Ignore(loadSaveGameThread);
		sn.Ignore(loadSaveGameProgressFlag);
		sn.Ignore(startGameMode);
		sn.Ignore(placeGeoLayoutProgress);
		sn.Ignore(geoLayoutEntityProgress);
		IgnoreSnapshotFields(sn);
		stopwatch.Stop();
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		CycleManager.LoadPostProcess(sn);
		TriggerSystem.LoadPostProcess(sn);
		World.LoadPostProcess(sn);
		DateAndTime.LoadPostProcess(sn);
		PlaySite = LookUp<Site, SiteID>.FindByID(snapshotPlaySite);
		List<Pair<Entity, Vector2>> preLoadPostProcess = snapshotAgentQuadTree.Select((Pair<EntityID, Vector2> t) => new Pair<Entity, Vector2>(Entity.FindByID(t.First), t.Second)).ToList();
		The.AgentQuadTree.SetPreLoadPostProcess(preLoadPostProcess);
		The.AgentQuadTree.LoadPostProcess(sn);
		The.Map.LoadPostProcess(sn);
		CollectionOfCollections.LoadPostProcess(sn);
		List<Collidable<Entity>> preLoadPostProcess2 = snapshotCollisionManagerList.Select((EntityID t) => Entity.FindByID(t).Collidable).ToList();
		The.CollisionManager.SetPreLoadPostProcess(preLoadPostProcess2);
		The.CollisionManager.LoadPostProcess(sn);
		if (snapshotOverlaySettings != null)
		{
			snapshotOverlaySettings.LoadPostProcess(sn);
		}
		if (snapshotInventorySettings != null)
		{
			snapshotInventorySettings.LoadPostProcess(sn);
		}
		if (snapshotBuySettings != null)
		{
			snapshotBuySettings.LoadPostProcess(sn);
		}
		if (snapshotSellSettings != null)
		{
			snapshotSellSettings.LoadPostProcess(sn);
		}
		InitEntityUpdater();
		if (snapshotEntities != null)
		{
			foreach (EntityID snapshotEntity in snapshotEntities)
			{
				entities.Add(LookUp<Entity, EntityID>.FindByID(snapshotEntity), null, keepExistingTimepoint: true);
			}
			snapshotEntities.Clear();
		}
		CreateRegulators();
		RecreateClientAfterInGameSave(sn);
		speed = Speeds.Normal;
	}

	public void RecreateClientAfterInGameSave(Snapshotter sn)
	{
		if (snapshotMapWindowWorldPosition.HasValue)
		{
			The.MapUI.MapWindowWorldPosition = snapshotMapWindowWorldPosition.Value;
		}
		if (snapshotEvents != null)
		{
			foreach (EventDialogData snapshotEvent in snapshotEvents)
			{
				snapshotEvent.LoadPostProcess(sn);
			}
			The.Client.EventDialogsData = snapshotEvents;
		}
		The.InGameUI.OverlaySettings = snapshotOverlaySettings;
		The.InGameUI.InventorySettings = snapshotInventorySettings;
		The.InGameUI.BuySettings = snapshotBuySettings;
		The.InGameUI.SellSettings = snapshotSellSettings;
		The.InGameUI.TaskSettings = snapshotTaskSettings;
		The.InGameUI.FoodProductionSettings = snapshotFoodProductionSettings;
		The.InGameUI.ProductionSettings = snapshotProductionSettings;
		The.InGameUI.KillsSettings = snapshotKillsSettings;
		The.InGameUI.NutrientSheetSettings = snapshotNutrientSheetSettings;
		The.InGameUI.OverlaySettings.LoadPostProcess(sn);
		The.InGameUI.InventorySettings.LoadPostProcess(sn);
		The.InGameUI.BuySettings.LoadPostProcess(sn);
		The.InGameUI.SellSettings.LoadPostProcess(sn);
		The.InGameUI.TaskSettings.LoadPostProcess(sn);
		The.InGameUI.FoodProductionSettings.LoadPostProcess(sn);
		The.InGameUI.ProductionSettings.LoadPostProcess(sn);
		The.InGameUI.KillsSettings.LoadPostProcess(sn);
		The.InGameUI.NutrientSheetSettings.LoadPostProcess(sn);
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public Sim()
	{
	}

	private void Entities_ListMemberRemoved(object sender, int indexOfRemovedMember)
	{
		ObservableList<Entity>.UpdateCounterWhenItemIsRemoved(ref entityCounter, indexOfRemovedMember);
	}

	public bool TimepointReached(long TimepointInTicks)
	{
		return TotalUnPausedGameTime.Ticks >= TimepointInTicks;
	}

	public bool TimepointReached(double TimepointInSeconds)
	{
		return Common.IsGreaterThanOrEqual(TotalUnPausedGameTimeInSeconds, TimepointInSeconds);
	}

	public bool TimepointReached(double? TimepointInSeconds)
	{
		if (TimepointInSeconds.HasValue)
		{
			return Common.IsGreaterThanOrEqual(TotalUnPausedGameTimeInSeconds, TimepointInSeconds.Value);
		}
		return false;
	}

	public bool TimepointReached(double? lastTimepointInSeconds, double firstTimepoint, double minimumTimeInSeconds)
	{
		if (!lastTimepointInSeconds.HasValue)
		{
			if (Common.IsGreaterThanOrEqual(TotalUnPausedGameTimeInSeconds, firstTimepoint))
			{
				return true;
			}
			return false;
		}
		return Common.IsGreaterThanOrEqual(TotalUnPausedGameTimeInSeconds, lastTimepointInSeconds.Value + minimumTimeInSeconds);
	}

	public double GetTimePhaseProgress(DayPhases phase)
	{
		if (phase <= DayPhases.Sleep)
		{
			switch (phase)
			{
			case DayPhases.Work:
				if (DateAndTime.TimeOfDay > 0.23 && DateAndTime.TimeOfDay < 0.615)
				{
					return (DateAndTime.TimeOfDay - 0.23) / 0.385;
				}
				return 0.0;
			case DayPhases.Leisure:
				if (DateAndTime.TimeOfDay > 0.615 && DateAndTime.TimeOfDay < 0.923)
				{
					return (DateAndTime.TimeOfDay - 0.615) / 0.30800000000000005;
				}
				return 0.0;
			case DayPhases.Sleep:
			{
				double num = 0.07699999999999996;
				if (DateAndTime.TimeOfDay > 0.923)
				{
					return (DateAndTime.TimeOfDay - 0.923) / (num + 0.23);
				}
				if (DateAndTime.TimeOfDay < 0.23)
				{
					return (num + DateAndTime.TimeOfDay) / (num + 0.23);
				}
				return 0.0;
			}
			}
		}
		return 0.0;
	}

	public void SetGameSpeed(Speeds speed)
	{
		this.speed = speed;
	}

	public void PauseGame()
	{
		speed = Speeds.Pause;
		Trace.WriteLine("PauseGame()");
		Trace.WriteLine(Environment.StackTrace);
	}

	public void ResumeGame()
	{
		speed = Speeds.Normal;
	}

	public void PostLoadMap()
	{
		The.Map.InitPostLoadMap();
		if (Mode == EngineMode.Game)
		{
			foreach (Allegiance allegiance in PlaySite.Allegiances)
			{
				allegiance.SharedKnowledge.PlaySiteKnowledge.InitAuxiliaryMaps();
			}
		}
		if (The.InGameUI != null && The.InGameUI.Minimap != null)
		{
			The.InGameUI.Minimap.CreateMap();
		}
		if (The.InGameUI != null && The.InGameUI.FogMap != null)
		{
			The.InGameUI.FogMap.CreateMap();
		}
	}

	private void AdvancePlaceGeoLayout(int progress)
	{
		The.LoadScreen.Progress("Sim PlaceGeoLayout stage " + (placeGeoLayoutProgress + 1), progress);
		placeGeoLayoutProgress++;
	}

	private void Advance(int progress)
	{
		The.LoadScreen.Progress("Sim BeginRun stage " + (beginRunProgress + 1), progress);
		beginRunProgress++;
	}

	public bool QueueBeginRun()
	{
		switch (beginRunProgress)
		{
		case 0:
			GameData.Instance.PostLoadContentValidate();
			Advance(60);
			return false;
		case 1:
			StartGamePreLoadMap();
			Advance(10);
			return false;
		case 2:
			InitRenderablesForEditorOrGame();
			Advance(10);
			return false;
		case 3:
			if (StartGameLoadMapOrSavedGame())
			{
				Advance(10);
				return false;
			}
			return false;
		case 4:
			StartGamePostLoadMap();
			Advance(10);
			return false;
		case 5:
			if (StartGamePostLoadMapPrepareEventActions())
			{
				Advance(10);
				return false;
			}
			return false;
		case 6:
			if (StartGamePostLoadMapExecuteEvents())
			{
				Advance(10);
				return false;
			}
			return false;
		case 7:
			if (StartGamePostLoadMapRegisterEvents())
			{
				Advance(10);
				return false;
			}
			return false;
		case 8:
			if (PlaySite.PlaceAllGeometry(ref geoLayoutEntityProgress, 100))
			{
				Advance(67);
				return false;
			}
			AdvancePlaceGeoLayout(50);
			return false;
		case 9:
			PlaySite.InitAuxiliaryMaps();
			beginRunProgress = 0;
			return true;
		default:
			beginRunProgress++;
			return false;
		}
	}

	private void InitRenderablesForEditorOrGame()
	{
		foreach (KeyValuePair<string, EntityType> allEntityType in GameData.Instance.AllEntityTypes)
		{
			allEntityType.Value.InitRenderableTypeMode();
		}
	}

	private void CreateLookupCollections()
	{
		Allegiance.CreateLookupCollection();
		AllegianceRelation.CreateLookupCollection();
		Household.CreateLookupCollection();
		Expedition.CreateLookupCollection();
		Terrain.CreateLookupCollection();
		TerrainTile.CreateLookupCollection();
		SubtileLayer.CreateLookupCollection();
		SubtileLayers.CreateLookupCollection();
		Goal.CreateLookupCollection();
		SimProcess.CreateLookupCollection();
		ProcessMemory.CreateLookupCollection();
		SimEffect.CreateLookupCollection();
		SubstancePool.CreateLookupCollection();
		Tree.CreateLookupCollection();
		DiscomfortMap.CreateLookupCollection();
		Site.CreateLookupCollection();
		Route.CreateLookupCollection();
		PassengerOrCargoSlot.CreateLookupCollection();
		Trigger.CreateLookupCollection();
		TileResourceItem.CreateLookupCollection();
		GatheringSite.CreateLookupCollection();
		GatheringSiteType.CreateLookupCollection();
		Mission.CreateLookupCollection();
		MissionStop.CreateLookupCollection();
		MissionStopTemplate.CreateLookupCollection();
		MissionTemplate.CreateLookupCollection();
		MissionActionTemplate.CreateLookupCollection();
		ContractTemplate.CreateLookupCollection();
		ReplenishActionType.CreateLookupCollection();
		ActionSetData.CreateLookupCollection();
		Conversation.CreateLookupCollection();
		ThreatGroup.CreateLookupCollection();
		ToolTypeCombination.CreateLookupCollection();
		LowVegetation.CreateLookupCollection();
		ResourceContainer.CreateLookupCollection();
		Job.CreateLookupCollection();
		NeedType.CreateLookupCollection();
		EntityGroup.CreateLookupCollection();
		Zone.CreateLookupCollection();
		Entity.CreateLookupCollection();
		MemoryFact.CreateLookupCollection();
		InfluenceMap.CreateLookupCollection();
		RegionSearchRequest.CreateLookupCollection();
		RegionPath.CreateLookupCollection();
		ActionLookup<IKnownProcess>.Create();
		ActionLookup<float>.Create();
		ActionLookup<List<EntityID>>.Create();
		ActionLookup<SimProcess>.Create();
		ActionLookup<int>.Create();
		ActionLookup.Create();
		Entity.CreateSleepyLookupCollection();
		Renderable.CreateSleepyLookupCollection();
		ParticleEmitter.CreateSleepyLookupCollection();
		Trigger.CreateSleepyLookupCollection();
		PolledEvent.CreateSleepyLookupCollection();
		SimProcess.CreateSleepyLookupCollection();
		EventAction.CreateSleepyLookupCollection();
		AccessibilityFeedback.CreateSleepyLookupCollection();
		ReplenishFeedback.CreateSleepyLookupCollection();
		TileResourceContainer.CreateSleepyLookupCollection();
		LookUp<ICyclable, CyclableID>.Create();
		LookUpICanIterateEntities.Create();
		LookUpHasEntityGroup.Create();
		LookUpIComposites.Create();
		LookUpIHasCrops.Create();
		LookUpOwners.Create();
		LookUpIDetectables.Create();
	}

	private bool StartGamePreLoadMap()
	{
		IsInNormalGameLoop = false;
		startGameMode = GetStartGameMode();
		switch (startGameMode)
		{
		case StartGameMode.Edit:
			Mode = StartGameParams.StartGameEditorParams.EngineMode;
			The.Sim.World = World.CreateFromWorldData(new WorldData
			{
				WorldRadius = GameData.Instance.Constants.DefaultWorldRadius
			});
			new Site("editorSite", isPlaySite: true).PlayerAllegiance = new Allegiance(AllegianceType.Player, GameData.Instance.AllEntityTypes["entity:human"], "editorAllegiance");
			The.Map.StartLoadMap(new DirectoryInfo(StartGameParams.StartGameEditorParams.MapToLoadPath));
			break;
		case StartGameMode.DebugNewGame:
		{
			StartDebugScenarioParams startDebugScenarioParams = StartGameParams.StartDebugScenarioParams;
			The.Sim.World = World.CreateFromWorldData(new WorldData
			{
				WorldRadius = GameData.Instance.Constants.DefaultWorldRadius
			});
			new Site("debugSite", isPlaySite: true)
			{
				Coords = new GeodeticCoordinate(10.0, 60.0),
				Name = "Debug Site"
			};
			The.Map.StartLoadMap(startDebugScenarioParams.MapKey);
			break;
		}
		case StartGameMode.ScenarioNewGame:
		{
			StartScenarioParams startScenarioParams = StartGameParams.StartScenarioParams;
			DateAndTime.ResetTimeOfYearAndTimeOfDay(startScenarioParams.Scenario.TimeDateYear);
			ExecuteStartAction(startScenarioParams.Scenario.ScenarioData.SpawnWorldAction);
			ExecuteStartAction(startScenarioParams.Scenario.ScenarioData.SpawnSiteAction);
			The.Map.StartLoadMap(startScenarioParams.Scenario.MapKey);
			break;
		}
		}
		return true;
	}

	private StartGameMode GetStartGameMode()
	{
		try
		{
			if (StartGameParams.StartGameEditorParams != null)
			{
				return StartGameMode.Edit;
			}
			if (StartGameParams.StartDebugScenarioParams != null)
			{
				if (string.IsNullOrEmpty(StartGameParams.SavedGameToLoad))
				{
					return StartGameMode.DebugNewGame;
				}
				return StartGameMode.DebugLoadSaved;
			}
			if (StartGameParams.StartScenarioParams != null)
			{
				if (string.IsNullOrEmpty(StartGameParams.SavedGameToLoad))
				{
					return StartGameMode.ScenarioNewGame;
				}
				return StartGameMode.ScenarioLoadSaved;
			}
		}
		catch (Exception innerException)
		{
			string text = "";
			if (this == null)
			{
				text = "Sim is null";
			}
			else if (StartGameParams == null)
			{
				text = "StartGameParams is null";
			}
			else
			{
				if (StartGameParams.StartDebugScenarioParams == null)
				{
					text = "StartDebugScenarioParams is null";
				}
				if (StartGameParams.StartGameEditorParams == null)
				{
					text = "StartGameEditorParams is null";
				}
				if (StartGameParams.StartScenarioParams == null)
				{
					text = "StartScenarioParams is null";
				}
				if (StartGameParams.SavedGameToLoad == null)
				{
					text = "SavedGameToLoad is null";
				}
			}
			throw new Exception("GetStartGameMode " + text, innerException);
		}
		return StartGameMode.Edit;
	}

	private bool StartGamePostLoadMap()
	{
		switch (startGameMode)
		{
		case StartGameMode.Edit:
			PostLoadMap();
			if (Mode != EngineMode.Edit && PlaySite != null)
			{
				new Expedition(The.Sim.PlaySite.PlayerAllegiance, "Start", "Start", MapManager.TileToWorldPos(new Point(15, 5)));
			}
			break;
		case StartGameMode.DebugNewGame:
		{
			PostLoadMap();
			The.Sim.World.GetPlaySite().PlayerAllegiance = new Allegiance(AllegianceType.Player, GameData.Instance.AllEntityTypes["entity:human"], "debugAllegiance");
			StartDebugScenarioParams startDebugScenarioParams = StartGameParams.StartDebugScenarioParams;
			if (startDebugScenarioParams.ScenarioKey != PlaceGameEntities.DebugScenarios.None)
			{
				PlaceGameEntities.BeginRun(startDebugScenarioParams.ScenarioKey);
			}
			break;
		}
		case StartGameMode.ScenarioNewGame:
			PostLoadMap();
			break;
		}
		ReportWhatStarted();
		return true;
	}

	/// <summary>
	/// PORT DIAGNOSTIC. One line in Errors.txt naming what was actually started.
	///
	/// Three different starts land in a playable game - a scenario, a debug scenario, and the map
	/// editor's TEST MAP - and from a screenshot they are indistinguishable. A day was spent on
	/// "the test scenarios are empty" that was TEST MAP loading a raw map with no colonists on it,
	/// correctly, every time. The report is four facts and settles it without a screenshot.
	/// </summary>
	private void ReportWhatStarted()
	{
		try
		{
			int persons = 0;
			int members = 0;
			var allegiance = PlaySite?.PlayerAllegiance;
			if (allegiance != null)
			{
				persons = allegiance.Persons?.Count ?? 0;
				members = allegiance.Members?.Count ?? 0;
			}
			string scenario = StartGameParams?.StartDebugScenarioParams == null
				? "-"
				: StartGameParams.StartDebugScenarioParams.ScenarioKey.ToString();
			string map = StartGameParams?.StartDebugScenarioParams?.MapKey
				?? StartGameParams?.StartScenarioParams?.Scenario?.MapKey
				?? "-";

			GameStateManagement.UnclaimedWorld.LogError(
				"    start mode       " + startGameMode + System.Environment.NewLine
				+ "    debug scenario   " + scenario + System.Environment.NewLine
				+ "    map              " + map + System.Environment.NewLine
				+ "    colonists placed " + persons + "   (allegiance members " + members + ")"
				+ System.Environment.NewLine + System.Environment.NewLine
				+ "Edit means the map editor or the TEST MAP button: a raw map, no colonists."
				+ System.Environment.NewLine
				+ "DebugNewGame means the dev panel's TEST SCENARIO."
				+ System.Environment.NewLine
				+ "ScenarioNewGame means NEW GAME.",
				"Game started");
		}
		catch (System.Exception)
		{
			// A note about what started must never be what stops it.
		}
	}

	private bool StartGamePostLoadMapPrepareEventActions()
	{
		StartGameMode startGameMode = this.startGameMode;
		if (startGameMode == StartGameMode.ScenarioNewGame)
		{
			StartScenarioParams startScenarioParams = StartGameParams.StartScenarioParams;
			List<EventActionType> list = new List<EventActionType>();
			if (startScenarioParams.Scenario.ScenarioData.Actions != null)
			{
				string[] actions = startScenarioParams.Scenario.ScenarioData.Actions;
				foreach (string key in actions)
				{
					EventActionType item = GameData.Instance.AllEventActionTypes[key];
					list.Add(item);
				}
			}
			foreach (KeyValuePair<string, Option> option in startScenarioParams.Options)
			{
				list.AddRange(option.Value.GetStartActions());
			}
			sortedStartGameEventActions = list.OrderBy((EventActionType e) => e.DelayInSeconds).ToList();
		}
		return true;
	}

	private bool StartGamePostLoadMapExecuteEvents()
	{
		if (sortedStartGameEventActions != null)
		{
			int d = startGameEventActionsProgress + 15;
			d = Common.ClampTop(d, sortedStartGameEventActions.Count);
			for (int i = startGameEventActionsProgress; i < d; i++)
			{
				new EventAction(sortedStartGameEventActions[i], null).Execute();
			}
			if (d == sortedStartGameEventActions.Count)
			{
				sortedStartGameEventActions = null;
				startGameEventActionsProgress = 0;
				return true;
			}
			startGameEventActionsProgress = d;
			return false;
		}
		return true;
	}

	private bool StartGamePostLoadMapRegisterEvents()
	{
		StartGameMode startGameMode = this.startGameMode;
		if (startGameMode == StartGameMode.ScenarioNewGame)
		{
			StartScenarioParams startScenarioParams = StartGameParams.StartScenarioParams;
			startScenarioParams.Scenario.RegisterEvents();
			foreach (KeyValuePair<string, Option> option in startScenarioParams.Options)
			{
				if (option.Value.ConditionalEvents != null)
				{
					string[] conditionalEvents = option.Value.ConditionalEvents;
					foreach (string eventKey in conditionalEvents)
					{
						PlaySite.EventManager.AddPolledEvent(eventKey);
					}
				}
			}
		}
		return true;
	}

	private bool StartGameLoadMapOrSavedGame()
	{
		IsInNormalGameLoop = false;
		switch (startGameMode)
		{
		case StartGameMode.Edit:
			_ = StartGameParams.StartGameEditorParams;
			return The.Map.LoadMapQueued();
		case StartGameMode.DebugNewGame:
			_ = StartGameParams.StartDebugScenarioParams;
			return The.Map.LoadMapQueued();
		case StartGameMode.DebugLoadSaved:
			return LoadSavedGame(StartGameParams.SavedGameToLoad);
		case StartGameMode.ScenarioNewGame:
			_ = StartGameParams.StartScenarioParams;
			return The.Map.LoadMapQueued();
		case StartGameMode.ScenarioLoadSaved:
			return LoadSavedGame(StartGameParams.SavedGameToLoad);
		default:
			return true;
		}
	}

	private void ExecuteStartAction(string actionKey)
	{
		new EventAction(GameData.Instance.AllEventActionTypes[actionKey], null).Execute();
	}

	public void AddStartLogMessage(string message)
	{
		startGameActionLog.Add(message);
	}

	public void AddStartPopulationSpawnMessage(string message)
	{
		startGamePopulationSpawnLog.Add(message);
	}

	public List<string> GetStartGameLog()
	{
		return startGameActionLog;
	}

	public List<string> GetStartPopulationSpawnLog()
	{
		return startGamePopulationSpawnLog;
	}

	public void InitializeBioEntityToPlace(PersonSex sex, float? age, AIAgeGroup? ageGroup, Allegiance allegiance, Entity entity)
	{
		entity.BiologicalEntity.CasteType = entity.EntityType.BiologicalType.Castes[(sex == PersonSex.Female) ? 1 : 0];
		if (allegiance == null && Mode == EngineMode.Game)
		{
			allegiance = new Allegiance(AllegianceType.Other, entity.EntityType);
		}
		entity.BiologicalEntity.SetAgePreInit(age, ageGroup);
		entity.Initialize(PlaySite, allegiance);
		entity.InitializeModelAndOnScreenFunctionality();
	}

	public void ExploreShroud(TilePos from, TilePos to, int startRadiusInTiles, int endRadiusInTiles, Entity byEntity)
	{
		WorldLocation fromLocation = MapManager.TilePosToWorldLocation(from);
		WorldLocation toLocation = MapManager.TilePosToWorldLocation(to);
		float startRadiusInPixels = startRadiusInTiles * 48;
		float endRadiusInPixels = endRadiusInTiles * 48;
		ExploreShroud(fromLocation, toLocation, startRadiusInPixels, endRadiusInPixels, byEntity, DetectMode.RollToDetectHiddenEntities);
	}

	public void ExploreShroud(Entity byEntity, DetectMode detectMode)
	{
		TerrainTile[][] tileMap = The.Map.TileMap;
		Sensor c = null;
		SharedKnowledge sharedKnowledge = null;
		if (byEntity != null)
		{
			byEntity.Find<Sensor>(out c);
			sharedKnowledge = byEntity.Intelligence.Allegiance.SharedKnowledge;
		}
		List<IDetectable> allDetectables = new List<IDetectable>();
		for (int i = 0; i < The.Map.mapTileWidth; i++)
		{
			TerrainTile[] array = tileMap[i];
			for (int j = 0; j < The.Map.mapTileHeight; j++)
			{
				TerrainTile tile = array[j];
				ExploreTile(byEntity, sharedKnowledge, detectMode, c, tile, allDetectables);
			}
		}
	}

	public void ExploreShroud(WorldLocation fromLocation, WorldLocation toLocation, float startRadiusInPixels, float endRadiusInPixels, Entity byEntity, DetectMode rollToDetect)
	{
		int num = (int)(startRadiusInPixels / 48f);
		int num2 = (int)(endRadiusInPixels / 48f);
		Vector2 vector = (toLocation - fromLocation).ToVector3().ToVector2();
		float num3 = vector.Length();
		vector.Normalize();
		float num4 = 0f;
		float num5 = 30f;
		WorldLocation worldLocation = fromLocation;
		for (; num4 < num3; num4 += num5)
		{
			worldLocation = new WorldLocation(fromLocation.X + num4 * vector.X, fromLocation.Y + num4 * vector.Y, 0f);
			int radiusInTiles = (int)MathHelper.Lerp(num, num2, num4 / num3);
			ExploreCircularSpot(byEntity, worldLocation, rollToDetect, radiusInTiles);
		}
	}

	public void ExploreCircularSpot(Entity byEntity, WorldLocation location, DetectMode rollToDetect, float radiusInPixels)
	{
		int radiusInTiles = (int)(radiusInPixels / 48f);
		The.Sim.ExploreCircularSpot(byEntity, location, rollToDetect, radiusInTiles);
	}

	public void ExploreCircularSpot(Entity byEntity, WorldLocation location, DetectMode rollToDetect, int radiusInTiles)
	{
		byEntity.Find<Sensor>(out var c);
		TerrainTile[][] tileMap = The.Map.TileMap;
		int jaggedArrayWidth = Common.GetJaggedArrayWidth(tileMap);
		int jaggedArrayHeight = Common.GetJaggedArrayHeight(tileMap);
		TilePos p = MapManager.WorldPosToTilePos(location);
		List<IDetectable> allDetectables = new List<IDetectable>();
		int num = Math.Max(0, p.X - radiusInTiles);
		int num2 = Math.Max(0, p.Y - radiusInTiles);
		int num3 = Math.Min(jaggedArrayWidth - 1, p.X + radiusInTiles);
		int num4 = Math.Min(jaggedArrayHeight - 1, p.Y + radiusInTiles);
		SharedKnowledge sharedKnowledge = null;
		if (byEntity != null)
		{
			sharedKnowledge = byEntity.Intelligence.Allegiance.SharedKnowledge;
		}
		for (int i = num; i <= num3; i++)
		{
			TerrainTile[] array = tileMap[i];
			for (int j = num2; j <= num4; j++)
			{
				if (Common.DistanceOctile(p, new TilePos(i, j)) <= (float)radiusInTiles)
				{
					TerrainTile tile = array[j];
					ExploreTile(byEntity, sharedKnowledge, rollToDetect, c, tile, allDetectables);
				}
			}
		}
	}

	private static void ExploreTile(Entity byEntity, SharedKnowledge sharedKnowledge, DetectMode detectMode, Sensor sensor, TerrainTile tile, List<IDetectable> allDetectables)
	{
		if (tile.HasEverBeenSeenByPlayer)
		{
			return;
		}
		tile.HasEverBeenSeenByPlayer = true;
		if (detectMode != DetectMode.NoEntityDetection && sensor != null)
		{
			allDetectables.Clear();
			tile.GetDetectablesOnTile(allDetectables);
			if (allDetectables.Count > 0)
			{
				bool rollToDetectHiddenEntities = detectMode == DetectMode.RollToDetectHiddenEntities;
				sensor.RollToDetect(byEntity, sharedKnowledge, allDetectables, suppressClientFeedbackAndEvents: true, detectAllWhichHasMinimumRange: true, unseeAfterDetecting: true, rollToDetectHiddenEntities, doAssert: false);
			}
		}
	}

	public override void LoadContent()
	{
	}

	public void AddWaitingAgent(Entity entity, WaitingFor waitingFor)
	{
	}

	private void CoordsTest()
	{
		GeodeticCoordinate value = new GeodeticCoordinate(50.0, 0.0);
		_ = DistanceCalculator.GetBearing(end: new GeodeticCoordinate(60.0, 0.0), start: value) / Math.PI;
	}

	private void InitEntityUpdater()
	{
		entities = new SleepyUpdater<Entity>(Module.Sim, staggerUpdates: true);
	}

	public void AddEntity(Entity entity)
	{
		entities.Add(entity);
	}

	public void RemoveEntity(Entity entity)
	{
		entities.Remove(entity);
	}

	public void AdvanceTime(double seconds)
	{
		TotalUnPausedGameTime = TotalUnPausedGameTime.Add(new TimeSpan(0, 0, 0, 0, (int)(seconds * 1000.0)));
		DateAndTime.AdvanceTime(seconds);
		foreach (Entity @as in PlaySite.Entities.GetAsList())
		{
			@as.CreateBioSystemsRegulator();
		}
	}

	public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
	{
		if (GameTime == null)
		{
			gameTime = new GameTime(new TimeSpan((long)(GameSpeed * (float)gameTime.TotalGameTime.Ticks)), new TimeSpan((long)(GameSpeed * (float)gameTime.ElapsedGameTime.Ticks)));
		}
		else
		{
			TimeSpan timeSpan = new TimeSpan((long)(GameSpeed * (float)gameTime.ElapsedGameTime.Ticks));
			TimeSpan totalGameTime = GameTime.TotalGameTime.Add(timeSpan);
			gameTime = new GameTime(totalGameTime, timeSpan);
		}
		base.Update(gameTime, otherScreenHasFocus: false, coveredByOtherScreen: false);
		if (!base.IsActive || !The.LoadScreen.IsLoadFinished)
		{
			return;
		}
		IsInNormalGameLoop = true;
		GameTime = gameTime;
		if (IsPaused)
		{
			return;
		}
		TotalUnPausedGameTime = TotalUnPausedGameTime.Add(new TimeSpan(gameTime.ElapsedGameTime.Ticks));
		Trace.WriteLine("Sim.Update #2 " + TotalUnPausedGameTimeInSeconds);
		if (Mode == EngineMode.Game)
		{
			The.Client.MarkPerformanceTime("before sim update", Color.Yellow);
			DateAndTime.Update(gameTime);
			TriggerSystem.Update(gameTime);
			World.Update(gameTime);
			The.Client.MarkPerformanceTime("World.Update", Color.Blue);
			entities.GetItem((Entity e) => e.EntityID == (EntityID)14180L);
			entities.Update(gameTime);
			The.Client.MarkPerformanceTime("Entities.Update", Color.Beige);
			LookUp<EntityGroup, EntityGroupID>.IterateMembers(delegate(EntityGroup g)
			{
				g.Update(gameTime);
			});
			The.Client.MarkPerformanceTime("HaulingJobManager.Update", Color.Purple);
			CycleManager.Update();
			if (IsGameOver)
			{
				IsInNormalGameLoop = true;
				return;
			}
			The.Client.MarkPerformanceTime("CycleManager.Update", Color.Red);
			CheckPerformance();
		}
		The.Map.Update(gameTime);
	}

	private void CheckPerformance()
	{
		if (performanceRegulator.IsReady() && (float)WaitingAgents.Count >= GameData.Instance.Constants.NumberOfAgentsWatingToTriggerAlert)
		{
			double num = The.Sim.WaitingAgents.Average((KeyValuePair<EntityID, Tuple<WaitingFor, double>> a) => TotalUnPausedGameTimeInSeconds - a.Value.Item2);
			if (num > (double)GameData.Instance.Constants.AverageAgentWatingTimeToTriggerAlert && base.Controller.Options.ShowPerformanceWarning)
			{
				The.Client.AddLogEvent(The.Client.Log.GeneralEvent, null, $"Warning: Slowdown detected. Average agent waiting time: {num:N1}s", UWGame.ClientSide.Log.Priority.High);
			}
		}
	}

	public Entity GetRepresentativeEntity()
	{
		if (PlaySite.PlayerAllegiance.Persons.Count != 0)
		{
			return PlaySite.PlayerAllegiance.Persons.FirstOrDefault((Entity p) => p.IsOnPlaySite());
		}
		List<PointTreeDweller<Entity>> pointObjectList = null;
		The.AgentQuadTree.GetAllObjects(ref pointObjectList);
		if (pointObjectList != null && pointObjectList.Count > 0)
		{
			return pointObjectList[0].ObjectAndPosition.First;
		}
		return null;
	}

	public override void Draw(GameTime gameTime)
	{
		_ = The.LoadScreen.IsLoadFinished;
	}
}
