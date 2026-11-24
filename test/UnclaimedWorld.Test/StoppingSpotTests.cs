using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using UWGame;
using UWGame.Control.Replays;
using UWGame.SimSide;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UnclaimedWorld.Test;

/// <summary>
/// Issue #33: one place decides where a vehicle lands or parks (StoppingSpot), reserves it for the
/// driver's side (SharedKnowledge.StopLocks) and looks at it again on the way in. These are the
/// rules that need no map: when two spots clash, when a reservation no longer counts, how often a
/// flight looks again, which helipads a Travel flight considers - and that the new state saves.
/// What needs a running game - the movement map, discomfort, known vehicles - is listed in the PR
/// for checking in game.
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class StoppingSpotTests
{
    private const BindingFlags AnyInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    private static Vector3 AtTile(int x, int y) => MapManager.TileToWorldPos(new Point(x, y));

    // GoalTakeFive is internal; PilotFliesHomeTests reaches it the same way.
    private static readonly MethodInfo Choose = typeof(Goal).Assembly.GetType("UWGame.SimSide.AI.Goals.GoalTakeFive")!
        .GetMethod("ChooseTerminalTile", BindingFlags.Static | BindingFlags.Public)!;

    private static Point? ChooseTerminalTile(IList<Vector3> terminals, Vector3 from, Func<Point, bool> taken) =>
        (Point?)Choose.Invoke(null, new object[] { terminals, from, taken });

    [TestMethod]
    public void TwoVehiclesClashWhenNearerThanTheirRadiiTogether()
    {
        // A lock covers the footprint, not a tile: a 40 px skimmer beside a 30 px one needs 70 px.
        Vector3 a = new Vector3(100, 100, 0);
        Assert.IsTrue(StoppingSpot.Overlaps(a, 40, new Vector3(169, 100, 0), 30));
        Assert.IsFalse(StoppingSpot.Overlaps(a, 40, new Vector3(171, 100, 0), 30));
        Assert.IsTrue(StoppingSpot.Overlaps(a, 40, new Vector3(100, 100, 500), 30), "height does not count - a spot is on the ground");
    }

    [TestMethod]
    public void AReservationCountsWhileSomebodyIsComingOrTheVehicleIsThere()
    {
        Vector3 spot = AtTile(10, 10);
        Assert.IsFalse(StoppingSpot.IsStale(holderHasDriver: true, AtTile(40, 40), spot), "a pilot on his way");
        Assert.IsFalse(StoppingSpot.IsStale(holderHasDriver: false, spot, spot), "parked on it, nobody in it");
        Assert.IsTrue(StoppingSpot.IsStale(holderHasDriver: false, AtTile(40, 40), spot),
            "no driver and somewhere else: the pilot died or got out, and nobody is coming");
    }

    [TestMethod]
    public void AFlightLooksAgainMoreOftenAsItCloses()
    {
        Assert.AreEqual(StoppingSpot.RecheckFarSeconds, StoppingSpot.RecheckInterval(StoppingSpot.RecheckNearDistance + 1));
        Assert.AreEqual(StoppingSpot.RecheckNearSeconds, StoppingSpot.RecheckInterval(StoppingSpot.RecheckNearDistance - 1));
    }

    [TestMethod]
    public void OnTheWaySomewhereOnlyAHelipadNearTheDestinationCounts()
    {
        var pads = new List<Vector3> { AtTile(10 + StoppingSpot.TravelTerminalTiles + 5, 10), AtTile(12, 10), AtTile(10, 10 + StoppingSpot.TravelTerminalTiles) };
        CollectionAssert.AreEqual(new List<Vector3> { pads[1], pads[2] }, StoppingSpot.TerminalsWithin(pads, AtTile(10, 10), StoppingSpot.TravelTerminalTiles),
            "the far pad is left out, the others keep their order");
        CollectionAssert.AreEqual(pads, StoppingSpot.TerminalsWithin(pads, AtTile(10, 10), null), "going home, every pad counts");
    }

    [TestMethod]
    public void TravelTakesTheFreeHelipadNearestTheDestination()
    {
        // StoppingSpot.ChooseTerminal: TerminalsWithin, then GoalTakeFive.ChooseTerminalTile from the destination.
        var pads = new List<Vector3> { AtTile(14, 10), AtTile(11, 10), AtTile(30, 30) };
        List<Vector3> near = StoppingSpot.TerminalsWithin(pads, AtTile(10, 10), StoppingSpot.TravelTerminalTiles);
        Assert.AreEqual(new Point(11, 10), ChooseTerminalTile(near, AtTile(10, 10), _ => false));
        Assert.AreEqual(new Point(14, 10), ChooseTerminalTile(near, AtTile(10, 10), tile => tile == new Point(11, 10)),
            "the nearer one is reserved, so the next");
        Assert.IsNull(ChooseTerminalTile(near, AtTile(10, 10), _ => true), "none free: open ground near the destination");
    }

    // ---- saving ----

    private Entity pilot = null!;
    private Sim? oldSim;

    [TestInitialize]
    public void Setup()
    {
        Entity.CreateLookupCollection();
        LookUpGoals.Create();
        pilot = (Entity)RuntimeHelpers.GetUninitializedObject(typeof(Entity));
        pilot.Components = new();
        typeof(Entity).GetField("id", AnyInstance)!.SetValue(pilot, (EntityID)9401L);
        LookUp<Entity, EntityID>.Add(pilot.EntityID, pilot);
        oldSim = The.Sim;
        The.Sim = (Sim)RuntimeHelpers.GetUninitializedObject(typeof(Sim));
        typeof(Sim).GetField("gameplayRandomGenerator", AnyInstance)!.SetValue(The.Sim, new RandomGenerator(1, RandomGenerator.GeneratorType.Sim));
    }

    [TestCleanup]
    public void Cleanup()
    {
        pilot.RemoveIDEntry();
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

    private GoalFlyToPosition SaveAndLoad(GoalFlyToPosition goal)
    {
        goal.entity = pilot;
        using var stream = new MemoryStream();
        Snapshotter writer = NewSnapshotter(Snapshotter.Mode.Save, stream);
        goal.DoVersion(writer);
        goal.DoSnapshot(writer);

        stream.Position = 0;
        var loaded = new GoalFlyToPosition();
        Snapshotter reader = NewSnapshotter(Snapshotter.Mode.Load, stream);
        loaded.DoVersion(reader);
        loaded.DoSnapshot(reader);
        Assert.AreEqual(stream.Length, stream.Position, "the load reads exactly what the save wrote");
        return loaded;
    }

    private static object? Field(GoalFlyToPosition goal, string name) => typeof(GoalFlyToPosition).GetField(name, AnyInstance)!.GetValue(goal);

    [TestMethod]
    public void AFlightToAReservedSpotSavesWhatItReChecksFor()
    {
        GoalFlyToPosition loaded = SaveAndLoad(new GoalFlyToPosition(pilot, new Point(5, 6), StopPurpose.ReturnHome, new Point(2, 3)));
        Assert.AreEqual(new Point(5, 6), loaded.Destination);
        Assert.AreEqual(StopPurpose.ReturnHome, Field(loaded, "stopPurpose"));
        Assert.AreEqual(new Point(2, 3), Field(loaded, "stopNear"), "a new spot is looked for near the same place");
    }

    [TestMethod]
    public void AStudioFlightSavesWithoutAPurposeAndIsNotReChecked()
    {
        GoalFlyToPosition loaded = SaveAndLoad(new GoalFlyToPosition(pilot, new Point(5, 6)));
        Assert.IsNull(Field(loaded, "stopPurpose"));
    }

    [TestMethod]
    public void TheLockTableSaves()
    {
        // SharedKnowledge.DoSnapshot writes StopLocks with DoDictionary.
        var locks = new Dictionary<EntityID, Vector3> { [(EntityID)7L] = AtTile(3, 4), [(EntityID)9L] = new Vector3(100.5f, 200.25f, 0) };
        using var stream = new MemoryStream();
        NewSnapshotter(Snapshotter.Mode.Save, stream).DoDictionary(locks);
        stream.Position = 0;
        Dictionary<EntityID, Vector3> loaded = NewSnapshotter(Snapshotter.Mode.Load, stream).DoDictionary(new Dictionary<EntityID, Vector3>());
        CollectionAssert.AreEquivalent(locks, loaded);
    }
}
