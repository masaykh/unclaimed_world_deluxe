using System.Reflection;
using System.Runtime.CompilerServices;
using UWGame;
using UWGame.Control.Replays;
using UWGame.SimSide;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;

namespace UnclaimedWorld.Test;

/// <summary>
/// GoalMoveToPosition queues GoalTakeoff, GoalFlyToPosition and GoalLand together, and every goal is
/// saved, active or not. Each finds its aircraft only in Activate, so saving while the pilot was
/// still walking to the skimmer, waiting or climbing threw NullReferenceException in DoSnapshot
/// (Lars's flight audit, 2026-11-17). A queued goal saves no aircraft, loads none, and finds it
/// when it starts.
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class QueuedFlightGoalSaveTests
{
    private const BindingFlags AnyInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    private Entity pilot = null!;
    private Entity entityZero = null!;
    private Sim? oldSim;

    private static Entity NewEntity(long id)
    {
        var entity = (Entity)RuntimeHelpers.GetUninitializedObject(typeof(Entity));
        entity.Components = new();
        typeof(Entity).GetField("id", AnyInstance)!.SetValue(entity, (EntityID)id);
        LookUp<Entity, EntityID>.Add(entity.EntityID, entity);
        return entity;
    }

    [TestInitialize]
    public void Setup()
    {
        Entity.CreateLookupCollection();
        LookUpGoals.Create();
        pilot = NewEntity(9301);
        // EntityID 0 is a real id (EntityID.First): a goal that saved the default would load this.
        entityZero = NewEntity((long)EntityID.First);
        // Goal.LoadPostProcess makes its regulators from the gameplay random stream.
        oldSim = The.Sim;
        The.Sim = (Sim)RuntimeHelpers.GetUninitializedObject(typeof(Sim));
        typeof(Sim).GetField("gameplayRandomGenerator", AnyInstance)!.SetValue(The.Sim, new RandomGenerator(1, RandomGenerator.GeneratorType.Sim));
    }

    [TestCleanup]
    public void Cleanup()
    {
        pilot.RemoveIDEntry();
        entityZero.RemoveIDEntry();
        The.Sim = oldSim!;
    }

    private static Snapshotter NewSnapshotter(Snapshotter.Mode mode, Stream stream)
    {
        var sn = new Snapshotter { mode = mode };
        if (mode == Snapshotter.Mode.Save)
        {
            typeof(Snapshotter).GetField("m_writer", AnyInstance)!.SetValue(sn, new BinaryWriter(stream));
        }
        else
        {
            typeof(Snapshotter).GetField("m_reader", AnyInstance)!.SetValue(sn, new BinaryReader(stream));
        }
        return sn;
    }

    private void SaveAndLoadQueued<T>() where T : Goal, new()
    {
        var queued = new T { entity = pilot };
        Assert.IsNull(typeof(T).GetField("Aircraft")!.GetValue(queued), "a queued goal has not looked for its aircraft yet");

        using var stream = new MemoryStream();
        queued.DoSnapshot(NewSnapshotter(Snapshotter.Mode.Save, stream));

        stream.Position = 0;
        var loaded = new T();
        var reader = NewSnapshotter(Snapshotter.Mode.Load, stream);
        loaded.DoSnapshot(reader);
        Assert.AreEqual(stream.Length, stream.Position, "the load reads exactly what the save wrote");
        Assert.AreEqual(EntityID.Invalid, typeof(T).GetField("aircraftID", AnyInstance)!.GetValue(loaded), "no aircraft was saved");

        loaded.LoadPostProcess(reader);
        Assert.AreSame(pilot, loaded.entity);
        Assert.IsNull(typeof(T).GetField("Aircraft")!.GetValue(loaded), "not entity 0 - the goal finds its aircraft when it starts");
        Assert.IsTrue(loaded.isInactive());
    }

    [TestMethod]
    public void AQueuedTakeoffSavesAndLoads() => SaveAndLoadQueued<GoalTakeoff>();

    [TestMethod]
    public void AQueuedFlightSavesAndLoads() => SaveAndLoadQueued<GoalFlyToPosition>();

    [TestMethod]
    public void AQueuedLandingSavesAndLoads() => SaveAndLoadQueued<GoalLand>();
}
