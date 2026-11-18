using System.Reflection;
using System.Runtime.CompilerServices;
using UWGame;
using UWGame.Control.Replays;
using UWGame.SimSide;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UnclaimedWorld.Test;

/// <summary>
/// A frightened pilot in his skimmer (GoalTakeFive, GoalDoTakeFive: ValidateSafetyAndTakeAction)
/// planned his flee path for the skimmer's transport, Air, and the movement maps keep no Air layer
/// (MapManager skips it): KeyNotFoundException 'Air', the error PR #2 fixed for a walk. The exit was
/// also queued behind the path, so he would have followed it still aboard (Lars's flight audit,
/// 2026-11-17). He now gets out first, landing if he is airborne, and judges the danger again on foot.
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class PilotPanicFleeTests
{
    private const BindingFlags AnyInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private static readonly MethodInfo PerformPanicFleeing = typeof(CompositeGoal).GetMethod("PerformPanicFleeing", AnyInstance)!;
    private static readonly Type GoalTakeFive = GoalType("GoalTakeFive");
    private static readonly Type GoalExitVehicle = GoalType("GoalExitVehicle");

    private static Type GoalType(string name) => typeof(Goal).Assembly.GetType("UWGame.SimSide.AI.Goals." + name)!;

    private Entity skimmer = null!;
    private Entity pilot = null!;
    private GoalThink brain = null!;
    private Sim? oldSim;
    private MapManager? oldMap;

    private static Entity NewEntity(long id, EntityType type)
    {
        var entity = (Entity)RuntimeHelpers.GetUninitializedObject(typeof(Entity));
        entity.Components = new();
        typeof(Entity).GetField("id", AnyInstance)!.SetValue(entity, (EntityID)id);
        typeof(Entity).GetProperty("EntityType")!.SetValue(entity, type);
        LookUp<Entity, EntityID>.Add(entity.EntityID, entity);
        return entity;
    }

    private static T Blank<T>() => (T)RuntimeHelpers.GetUninitializedObject(typeof(T));

    [TestInitialize]
    public void Setup()
    {
        Entity.CreateLookupCollection();
        LookUpGoals.Create();
        // New goals make their regulators from the gameplay random stream.
        oldSim = The.Sim;
        The.Sim = Blank<Sim>();
        typeof(Sim).GetField("gameplayRandomGenerator", AnyInstance)!.SetValue(The.Sim, new RandomGenerator(1, RandomGenerator.GeneratorType.Sim));
        oldMap = The.Map;
        The.Map = Blank<MapManager>();
        The.Map.InfluenceMapTileIsFreeInfo = typeof(InfluenceMap).GetMethod("DiscomfortTileIsFree", BindingFlags.Public | BindingFlags.Static)!;

        skimmer = NewEntity(9501, new EntityType("entity:skimmer") { ContainerType = new VehicleContainerType { Transport = SurfaceType.TransportType.Air } });

        var pilotType = new EntityType("entity:pilot");
        pilot = NewEntity(9502, pilotType);
        pilot.DrivingVehicle = skimmer.EntityID;
        var locomotor = Blank<Locomotor>();
        locomotor.LeggedLocomotor = Blank<LeggedLocomotor>();
        pilot.Components[typeof(Locomotor)] = locomotor;

        // The movement maps as MapManager makes them: a layer for every transport but Air.
        var movementMap = Blank<MovementMap>();
        typeof(MovementMap).GetField("layers", AnyInstance)!.SetValue(movementMap,
            new Dictionary<SurfaceType.TransportType, SubtileLayers> { [SurfaceType.TransportType.Foot] = new SubtileLayers() });
        var playSite = Blank<PlaySiteKnowledge>();
        playSite.AllMovementMaps = new() { [ProtectionLevel.Exposed] = new() { [pilotType] = new() { [ThreatStance.Bold] = movementMap } } };
        var shared = Blank<SharedKnowledge>();
        shared.PlaySiteKnowledge = playSite;
        var allegiance = Blank<Allegiance>();
        allegiance.SharedKnowledge = shared;

        brain = new GoalThink();
        brain.AddToLookup(); // a goal with no id takes no subgoals
        var intelligence = Blank<Intelligence>();
        intelligence.Brain = brain;
        intelligence.Allegiance = allegiance;
        pilot.Components[typeof(Intelligence)] = intelligence;
    }

    [TestCleanup]
    public void Cleanup()
    {
        foreach (Goal goal in brain.Subgoals)
        {
            goal.RemoveIDEntry();
        }
        brain.RemoveIDEntry();
        skimmer.RemoveIDEntry();
        pilot.RemoveIDEntry();
        The.Sim = oldSim!;
        The.Map = oldMap!;
    }

    [TestMethod]
    public void AFrightenedPilotGetsOutBeforeHeFlees()
    {
        Assert.AreEqual(SurfaceType.TransportType.Air, pilot.GetTransportType(), "aboard the skimmer when the fright comes");

        var takingFive = (Goal)Activator.CreateInstance(GoalTakeFive)!;
        takingFive.entity = pilot;
        typeof(Goal).GetField("entityIntelligence", AnyInstance)!.SetValue(takingFive, pilot.Intelligence);
        object? keepsGoing = PerformPanicFleeing.Invoke(takingFive, new object[] { Blank<DiscomfortMap>() });

        Assert.AreEqual(false, keepsGoing, "the activity he was at stops");
        Goal[] next = brain.Subgoals.ToArray();
        Assert.AreEqual(2, next.Length);
        Assert.AreEqual(GoalExitVehicle, next[0].GetType(), "out of the skimmer first - it lands him if it is airborne");
        Assert.AreEqual(skimmer.EntityID, GoalExitVehicle.GetField("vehicle", AnyInstance)!.GetValue(next[0]));
        Assert.AreEqual(GoalTakeFive, next[1].GetType(), "then the danger is judged again, on foot, where he stands");
    }
}
