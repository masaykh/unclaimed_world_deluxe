using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using UWGame;
using UWGame.Client.Particles;
using UWGame.ClientSide;
using UWGame.ClientSide.GameEvents;
using UWGame.ClientSide.Particles;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Scenarios;
using UWGame.SimSide.Systems;
using UWGame.SimSide.XmlCollections;

namespace UnclaimedWorld.Test;

/// <summary>
/// Kastuk, 2026-11-17: "Sulfur lake at Twinkler island no more smoking after loading the
/// savegame." The smoke is two of Twinkler Island's start actions (sulphurousSmoke1/2), which
/// only a new game runs, and what they make - a Renderable on a TerrainTile with a
/// ParticleEmitter - is client-only and not in a save.
///
/// This plays both halves with the real scenario and the real action and particle tables, with
/// no GraphicsDevice: a new game's start actions into one client and map, then what a load
/// leaves (a new client from RecreateClientAfterLoad, a freshly read map), then the restore
/// Snapshotter.Load now calls. It counts the emitters per particle system on each side.
/// Not run in parallel: it sets The.Client, The.Map and GameData tables.
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class ScenarioEmittersAfterLoadTests
{
    private const BindingFlags AnyInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    private Client? previousClient;
    private MapManager? previousMap;
    private Constants? previousConstants;
    private Sim? previousSim;
    private readonly List<string> addedActionKeys = new();
    private readonly List<string> addedSystemKeys = new();

    [TestInitialize]
    public void Setup()
    {
        previousClient = The.Client;
        previousMap = The.Map;
        previousSim = The.Sim;
        // Emitters time their next puff from the Sim clock; a Sim with its clock at zero is all they read.
        The.Sim = (Sim)RuntimeHelpers.GetUninitializedObject(typeof(Sim));
        previousConstants = GameData.Instance.Constants;
        // The loader reads the world radius for the scenario's SpawnWorldAction.
        GameData.Instance.Constants ??= new Constants();
        foreach (EventActionType action in UWGame.SimSide.AllGameData.Scenarios.Scenario_1.Data.EventActionLoader.Init())
        {
            if (!GameData.Instance.AllEventActionTypes.ContainsKey(action.KeyName))
            {
                GameData.Instance.AllEventActionTypes[action.KeyName] = action;
                addedActionKeys.Add(action.KeyName);
            }
        }
        foreach (ParticleSystemType type in ParticlesLoader.Init())
        {
            if (!GameData.Instance.AllParticleSystems.ContainsKey(type.KeyName))
            {
                GameData.Instance.AllParticleSystems[type.KeyName] = type;
                addedSystemKeys.Add(type.KeyName);
            }
        }
    }

    [TestCleanup]
    public void Cleanup()
    {
        The.Client = previousClient!;
        The.Map = previousMap!;
        The.Sim = previousSim!;
        GameData.Instance.Constants = previousConstants!;
        foreach (string key in addedActionKeys)
        {
            GameData.Instance.AllEventActionTypes.Remove(key);
        }
        foreach (string key in addedSystemKeys)
        {
            GameData.Instance.AllParticleSystems.Remove(key);
        }
    }

    private static void SetField(object target, string name, object value) =>
        target.GetType().GetField(name, AnyInstance)!.SetValue(target, value);

    /// <summary>A client and a map as a load leaves them: new, with no emitters on either.</summary>
    private static void FreshClientAndMap()
    {
        var client = (Client)RuntimeHelpers.GetUninitializedObject(typeof(Client));
        SetField(client, "renderables", new SleepyUpdater<Renderable>());
        client.ClientRandomGenerator = new UWGame.Control.Replays.RandomGenerator(UWGame.Control.Replays.RandomGenerator.GeneratorType.Client);
        var manager = new ParticleManager();
        foreach (ParticleSystemType type in GameData.Instance.AllParticleSystems.Values)
        {
            // ParticleSystem.Initialize loads a texture; what AddEmitter touches is set by hand.
            var system = (ParticleSystem)RuntimeHelpers.GetUninitializedObject(typeof(ParticleSystem));
            system.ParticleSystemType = type;
            system.FreeParticles = new Queue<Particle>();
            SetField(system, "AllEmitters", new SleepyUpdater<ParticleEmitter>());
            SetField(system, "allEmittersForParticleUpdates", new List<ParticleEmitter>());
            manager.ParticleSystems[type] = system;
        }
        client.ParticleManager = manager;
        The.Client = client;

        var map = (MapManager)RuntimeHelpers.GetUninitializedObject(typeof(MapManager));
        map.TileMap = new TerrainTile[160][];
        for (int x = 0; x < map.TileMap.Length; x++)
        {
            map.TileMap[x] = new TerrainTile[160];
            for (int y = 0; y < map.TileMap[x].Length; y++)
            {
                map.TileMap[x][y] = (TerrainTile)RuntimeHelpers.GetUninitializedObject(typeof(TerrainTile));
            }
        }
        The.Map = map;
    }

    private static Dictionary<string, int> EmittersBySystem()
    {
        var counts = new Dictionary<string, int>();
        foreach (KeyValuePair<ParticleSystemType, ParticleSystem> kvp in The.Client.ParticleManager.ParticleSystems)
        {
            int n = ((List<ParticleEmitter>)kvp.Value.GetType().GetField("allEmittersForParticleUpdates", AnyInstance)!.GetValue(kvp.Value)!).Count;
            if (n > 0)
            {
                counts[kvp.Key.KeyName] = n;
            }
        }
        return counts;
    }

    private static StartGameParams TwinklerIsland()
    {
        var scenario = new StartScenarioParams
        {
            ScenarioName = "Twinkler Island",
            Source = Source.RefactoredGames,
            Options = new SerializableDictionary<string, Option>(),
        };
        // What a save's header does on load (StartScenarioParams.LoadPostProcess).
        scenario.Scenario = UWGame.SimSide.AllGameData.Scenarios.AllScenarioLoader.LoadScenarioHeader(scenario.ScenarioName, scenario.Source);
        scenario.Scenario.ScenarioData = UWGame.SimSide.AllGameData.Scenarios.AllScenarioLoader.LoadScenarioData(scenario.Scenario);
        return new StartGameParams { StartScenarioParams = scenario };
    }

    /// <summary>Sim.StartGamePostLoadMapExecuteEvents for a new game, its particle actions only.</summary>
    private static void RunNewGameStartEmitters(StartGameParams start)
    {
        foreach (string key in start.StartScenarioParams.Scenario.ScenarioData.Actions)
        {
            // The base tables' actions (group meetings and so on) are not loaded here; none is an emitter.
            if (GameData.Instance.AllEventActionTypes.TryGetValue(key, out EventActionType? type) && type is ParticleEffectAction action)
            {
                try
                {
                    new EventAction(action, null).Execute();
                }
                catch (Exception) when (action.UseLocationOfEntity != null)
                {
                    // blackSmoke is placed on the wreck entity, which a table-only world has not got.
                }
            }
        }
    }

    [TestMethod]
    public void SulphurLakeSmokesAfterLoadAsInNewGame()
    {
        StartGameParams start = TwinklerIsland();

        FreshClientAndMap();
        RunNewGameStartEmitters(start);
        Dictionary<string, int> newGame = EmittersBySystem();
        Assert.AreEqual(2, newGame.GetValueOrDefault("sulphurousSmoke"), "a new game of Twinkler Island has two sulphur smokes");

        // The load: a new client, a map read from the save, and nothing in the save about either.
        FreshClientAndMap();
        Assert.AreEqual(0, EmittersBySystem().Count, "a load starts with no emitters");

        int restored = ParticleEffectAction.RestoreLastingStartEmittersAfterLoad(start);
        Dictionary<string, int> loaded = EmittersBySystem();

        Assert.AreEqual(2, loaded.GetValueOrDefault("sulphurousSmoke"), "the sulphur lake smokes again after a load");
        CollectionAssert.AreEquivalent(newGame.ToList(), loaded.ToList(), "after a load the same lasting emitters as a new game, none doubled");
        Assert.AreEqual(loaded.Values.Sum(), restored);
    }

    [TestMethod]
    public void TimedEffectsAreNotRestarted()
    {
        List<string> keys = ParticleEffectAction.LastingStartEmitters(TwinklerIsland().StartScenarioParams).Select(a => a.KeyName).ToList();

        CollectionAssert.Contains(keys, "sulphurousSmoke1");
        CollectionAssert.Contains(keys, "sulphurousSmoke2");
        // 20 seconds of smoke over the wreck at the start: long gone in any saved game.
        CollectionAssert.DoesNotContain(keys, "blackSmoke");
    }

    [TestMethod]
    public void NoScenarioRestoresNothing()
    {
        Assert.AreEqual(0, ParticleEffectAction.RestoreLastingStartEmittersAfterLoad(new StartGameParams()));
        Assert.AreEqual(0, ParticleEffectAction.RestoreLastingStartEmittersAfterLoad(null!));
    }
}
