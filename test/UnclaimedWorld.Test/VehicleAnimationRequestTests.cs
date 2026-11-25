using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using UWGame;
using UWGame.ClientSide.Renderables;
using UWGame.Control.Replays;
using UWGame.SimSide;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Vehicles;

namespace UnclaimedWorld.Test;

[TestClass]
[DoNotParallelize]
public sealed class VehicleAnimationRequestTests
{
    private const BindingFlags AnyInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private Entity skimmer = null!;
    private Entity pilot = null!;
    private Sim? oldSim;

    private sealed class ScalarRenderable : Renderable
    {
        public double? Scalar;

        public ScalarRenderable() : base((Entity)null!, (RenderableType)null!) { }

        public override void UpdateMainAnimationTimeScalar(double scalar) => Scalar = scalar;
    }

    private static Entity NewEntity(long id)
    {
        var entity = (Entity)RuntimeHelpers.GetUninitializedObject(typeof(Entity));
        entity.Components = new();
        typeof(Entity).GetField("id", AnyInstance)!.SetValue(entity, (EntityID)id);
        entity.Renderable = (Renderable)RuntimeHelpers.GetUninitializedObject(typeof(Renderable));
        entity.Renderable.AnimConditions = new AnimConditions();
        LookUp<Entity, EntityID>.Add(entity.EntityID, entity);
        return entity;
    }

    [TestInitialize]
    public void Setup()
    {
        Entity.CreateLookupCollection();
        LookUpGoals.Create();
        oldSim = The.Sim;
        The.Sim = (Sim)RuntimeHelpers.GetUninitializedObject(typeof(Sim));
        typeof(Sim).GetField("gameplayRandomGenerator", AnyInstance)!.SetValue(The.Sim, new RandomGenerator(1, RandomGenerator.GeneratorType.Sim));
        skimmer = NewEntity(9501);
        typeof(Entity).GetProperty("EntityType")!.SetValue(skimmer,
            new EntityType("entity:skimmer")
            {
                LocomotorType = new LocomotorType { VTOLLocomotorType = new VTOLLocomotorType { CruiseAltitude = 120f } },
                ContainerType = new VehicleContainerType { Aircraft = new AircraftType() }
            });
        skimmer.Components[typeof(Locomotor)] = (Locomotor)RuntimeHelpers.GetUninitializedObject(typeof(Locomotor));
        typeof(GameObject).GetField("location", AnyInstance)!.SetValue(skimmer, (Vector3?)new Vector3(100f, 100f, 0f));
        var vehicle = (VehicleContainer)RuntimeHelpers.GetUninitializedObject(typeof(VehicleContainer));
        vehicle.Aircraft = new Aircraft();
        skimmer.Contains = vehicle;
        pilot = NewEntity(9502);
        typeof(Entity).GetField("containedBy", AnyInstance)!.SetValue(pilot, (EntityID?)skimmer.EntityID);
        vehicle.DrivenBy = pilot;
    }

    [TestCleanup]
    public void Cleanup()
    {
        skimmer.RemoveIDEntry();
        pilot.RemoveIDEntry();
        The.Sim = oldSim!;
    }

    private static GameTime Seconds(double seconds) => new(TimeSpan.Zero, TimeSpan.FromSeconds(seconds));

    private GoalTakeoff StartTakeoff()
    {
        var goal = new GoalTakeoff(pilot);
        goal.EnterIfNew();
        goal.Process(Seconds(0));
        return goal;
    }

    [TestMethod]
    public void TakeoffRequestsAnimationsOnVehicleAndWaitsOnPilot()
    {
        var goal = StartTakeoff();
        Assert.AreEqual(4, skimmer.Renderable.ParallelAnimationConditions.Count);
        Assert.AreEqual(0, pilot.Renderable.ParallelAnimationConditions.Count);
        var wait = (GoalWait)goal.Subgoals.Peek();
        Assert.AreSame(pilot, wait.entity);
        Assert.AreEqual(3d, wait.MaxTimeToWait);
        goal.Process(Seconds(2));
        Assert.AreEqual(4, skimmer.Renderable.ParallelAnimationConditions.Count);
        Assert.AreEqual(1d, wait.TimeLeftInSeconds);
        goal.Terminate();
        Assert.AreEqual(0, skimmer.Renderable.ParallelAnimationConditions.Count);
    }

    [TestMethod]
    public void FailedWaitClearsOnlyTakeoffsRequests()
    {
        var existing = new AnimConditions { Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Pre, (int)AnimModifier.Right, (int)AnimModifier.Slow) };
        skimmer.Renderable.AddParallelAnimationCondition(existing);
        var goal = StartTakeoff();
        Assert.AreEqual(5, skimmer.Renderable.ParallelAnimationConditions.Count);
        goal.Subgoals.Peek().Status = Status.Failed;
        goal.Process(Seconds(0));
        Assert.AreEqual(Status.Failed, goal.Status);
        Assert.AreEqual(1, skimmer.Renderable.ParallelAnimationConditions.Count);
        Assert.AreEqual(existing.Modifiers.Bits, skimmer.Renderable.ParallelAnimationConditions[0].Modifiers.Bits);
    }

    [TestMethod]
    public void ActiveTakeoffSnapshotRetainsOwnershipWithoutDuplicatingRequests()
    {
        var goal = StartTakeoff();
        goal.Process(Seconds(1));
        using var stream = new MemoryStream();
        var writer = new Snapshotter { mode = Snapshotter.Mode.Save };
        typeof(Snapshotter).GetField("m_writer", AnyInstance)!.SetValue(writer, new BinaryWriter(stream));
        goal.DoVersion(writer);
        goal.DoSnapshot(writer);
        stream.Position = 0;
        var reader = new Snapshotter { mode = Snapshotter.Mode.Load };
        typeof(Snapshotter).GetField("m_reader", AnyInstance)!.SetValue(reader, new BinaryReader(stream));
        var loaded = new GoalTakeoff();
        loaded.DoVersion(reader);
        loaded.DoSnapshot(reader);
        loaded.LoadPostProcess(reader);
        Assert.AreEqual(stream.Length, stream.Position);
        Assert.AreEqual(4, skimmer.Renderable.ParallelAnimationConditions.Count);
        Assert.AreEqual(2d, ((GoalWait)loaded.Subgoals.Peek()).TimeLeftInSeconds);
        loaded.Terminate();
        Assert.AreEqual(0, skimmer.Renderable.ParallelAnimationConditions.Count);
    }

    [TestMethod]
    public void TargetedWaitBeginsAfterPreviousSubgoal()
    {
        var parent = new GoalTakeoff(pilot);
        var previous = new GoalWait(pilot, 1d);
        var condition = new AnimConditions { Action = AnimAction.Containing, Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Pre, (int)AnimModifier.Crew) };
        var wait = new GoalWait(pilot, new List<AnimConditions> { condition }, 3d, animationTargetEntity: skimmer.EntityID);
        parent.AddSubgoal(previous);
        parent.AddSubgoal(wait);
        Assert.AreEqual(0, skimmer.Renderable.ParallelAnimationConditions.Count);
        previous.Status = Status.Completed;
        parent.ProcessSubgoals(Seconds(0));
        Assert.AreEqual(1, skimmer.Renderable.ParallelAnimationConditions.Count);
        parent.Terminate();
        Assert.AreEqual(0, skimmer.Renderable.ParallelAnimationConditions.Count);
    }

    [TestMethod]
    public void AnimationFlagsAndScalingUseTheSpecifiedTarget()
    {
        var renderable = (ScalarRenderable)RuntimeHelpers.GetUninitializedObject(typeof(ScalarRenderable));
        renderable.AnimConditions = new AnimConditions();
        skimmer.Renderable = renderable;
        var wait = new GoalWait(pilot, 4d, AnimAction.Containing, AnimModifier.Crew,
            scaleAnimationToFillWaitPeriod: true, animationTargetEntity: skimmer);
        wait.EnterIfNew();
        Assert.AreEqual(AnimAction.Containing, renderable.AnimConditions.Action);
        Assert.IsTrue(renderable.AnimConditions.Modifiers.Test(AnimModifier.Crew));
        Assert.IsNull(pilot.Renderable.AnimConditions.Action);
        Assert.IsFalse(pilot.Renderable.AnimConditions.Modifiers.Test(AnimModifier.Crew));
        wait.Process(Seconds(2));
        Assert.AreEqual(0.5d, renderable.Scalar);
        wait.Process(Seconds(2));
        Assert.AreEqual(Status.Completed, wait.Status);
        Assert.IsNull(renderable.AnimConditions.Action);
        Assert.IsFalse(renderable.AnimConditions.Modifiers.Test(AnimModifier.Crew));
    }

    [TestMethod]
    public void OmittedTargetKeepsOwnerAnimationBehavior()
    {
        var wait = new GoalWait(pilot, 3d, AnimAction.Eating, AnimModifier.Left);
        wait.EnterIfNew();
        Assert.AreEqual(AnimAction.Eating, pilot.Renderable.AnimConditions.Action);
        Assert.IsTrue(pilot.Renderable.AnimConditions.Modifiers.Test(AnimModifier.Left));
        wait.Terminate();
        Assert.IsNull(pilot.Renderable.AnimConditions.Action);
        Assert.IsFalse(pilot.Renderable.AnimConditions.Modifiers.Test(AnimModifier.Left));
    }

    [TestMethod]
    public void MissingTargetFailsWithoutPublishingOrClearingOtherRequests()
    {
        var condition = new AnimConditions { Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Left) };
        skimmer.Renderable.AddParallelAnimationCondition(condition);
        var wait = new GoalWait(pilot, new List<AnimConditions> { condition }, 3d, animationTargetEntity: skimmer.EntityID);
        skimmer.RemoveIDEntry();
        try
        {
            wait.EnterIfNew();
            Assert.AreEqual(Status.Failed, wait.Status);
            Assert.AreEqual(Status.Failed, wait.Process(Seconds(1)));
            Assert.AreEqual(0, pilot.Renderable.ParallelAnimationConditions.Count);
        }
        finally
        {
            LookUp<Entity, EntityID>.Add(skimmer.EntityID, skimmer);
        }
        wait.Terminate();
        Assert.AreEqual(1, skimmer.Renderable.ParallelAnimationConditions.Count);
    }

    [TestMethod]
    public void TargetRemovedDuringWaitFailsWithoutFallingBackToOwner()
    {
        var condition = new AnimConditions { Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Right) };
        var wait = new GoalWait(pilot, new List<AnimConditions> { condition }, 3d, animationTargetEntity: skimmer);
        wait.EnterIfNew();
        wait.Process(Seconds(1));
        skimmer.RemoveIDEntry();
        try
        {
            Assert.AreEqual(Status.Failed, wait.Process(Seconds(1)));
            Assert.AreEqual(0, pilot.Renderable.ParallelAnimationConditions.Count);
        }
        finally
        {
            LookUp<Entity, EntityID>.Add(skimmer.EntityID, skimmer);
        }
    }

    [TestMethod]
    public void TargetedWaitSnapshotRestoresTargetAndCleansUpAfterLoad()
    {
        var condition = new AnimConditions { Modifiers = new BitMask64(typeof(AnimModifier), (int)AnimModifier.Left) };
        var wait = new GoalWait(pilot, new List<AnimConditions> { condition }, 3d, animationTargetEntity: skimmer);
        wait.EnterIfNew();
        wait.Process(Seconds(1));

        using var stream = new MemoryStream();
        var writer = new Snapshotter { mode = Snapshotter.Mode.Save };
        typeof(Snapshotter).GetField("m_writer", AnyInstance)!.SetValue(writer, new BinaryWriter(stream));
        wait.DoVersion(writer);
        wait.DoSnapshot(writer);
        writer.DoISnapshot(new Renderable.SnapshotRenderable(skimmer.Renderable), verifyField: false);

        stream.Position = 0;
        var reader = new Snapshotter { mode = Snapshotter.Mode.Load };
        typeof(Snapshotter).GetField("m_reader", AnyInstance)!.SetValue(reader, new BinaryReader(stream));
        var restoredWait = new GoalWait();
        var wasSnapshotting = Snapshotter.IsSnapshotting;
        try
        {
            Snapshotter.IsSnapshotting = true;
            restoredWait.DoVersion(reader);
            restoredWait.DoSnapshot(reader);
            var snapshot = (Renderable.SnapshotRenderable)reader.DoISnapshot<Renderable.SnapshotRenderable>(null!, verifyField: false);
            var restoredRenderable = (Renderable)RuntimeHelpers.GetUninitializedObject(typeof(Renderable));
            snapshot.LoadRenderableWithSnapshotData(restoredRenderable);
            skimmer.Renderable = restoredRenderable;
        }
        finally
        {
            Snapshotter.IsSnapshotting = wasSnapshotting;
        }
        restoredWait.LoadPostProcess(reader);
        Assert.AreEqual(stream.Length, stream.Position);
        Assert.AreEqual(1, skimmer.Renderable.ParallelAnimationConditions.Count);
        Assert.AreEqual(2d, restoredWait.TimeLeftInSeconds);
        restoredWait.Process(Seconds(2));
        Assert.AreEqual(Status.Completed, restoredWait.Status);
        Assert.AreEqual(0, skimmer.Renderable.ParallelAnimationConditions.Count);
    }
}
