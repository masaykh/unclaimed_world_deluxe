using System.Reflection;
using System.Runtime.CompilerServices;
using UWGame;
using UWGame.SimSide;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Vehicles;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Maps;

namespace UnclaimedWorld.Test;

/// <summary>
/// A pilot told to walk (GoalArriveAsVisitor's NoVehicle move, on the way to bed) plans the walk
/// while still the skimmer's driver. The path must be planned for feet: the movement maps keep no
/// Air layer, and asking for one threw KeyNotFoundException 'Air' (Kastuk, 2026-11-14).
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class PilotWalkTransportTests
{
    private static readonly MethodInfo PathTransport =
        typeof(GoalMoveToPosition).GetMethod("PathTransport", BindingFlags.Instance | BindingFlags.NonPublic)!;

    private Entity vehicle = null!;
    private Entity pilot = null!;

    private static Entity NewEntity(long id)
    {
        var entity = (Entity)RuntimeHelpers.GetUninitializedObject(typeof(Entity));
        entity.Components = new();
        typeof(Entity).GetField("id", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(entity, (EntityID)id);
        LookUp<Entity, EntityID>.Add(entity.EntityID, entity);
        return entity;
    }

    [TestInitialize]
    public void Setup()
    {
        Entity.CreateLookupCollection();
        vehicle = NewEntity(9101);
        typeof(Entity).GetProperty("EntityType")!.SetValue(vehicle,
            new EntityType("entity:skimmer") { ContainerType = new VehicleContainerType { Transport = SurfaceType.TransportType.Air } });
        pilot = NewEntity(9102);
        pilot.DrivingVehicle = vehicle.EntityID;
    }

    [TestCleanup]
    public void Cleanup()
    {
        vehicle.RemoveIDEntry();
        pilot.RemoveIDEntry();
    }

    private SurfaceType.TransportType TransportFor(GoalMoveToPosition.VehicleUse use)
    {
        var goal = (GoalMoveToPosition)RuntimeHelpers.GetUninitializedObject(typeof(GoalMoveToPosition));
        goal.entity = pilot;
        goal.VehicleUseByGoal = use;
        return (SurfaceType.TransportType)PathTransport.Invoke(goal, null)!;
    }

    [TestMethod]
    public void PilotToldToWalkPlansForFeetNotAir()
    {
        Assert.AreEqual(SurfaceType.TransportType.Air, pilot.GetTransportType(), "still the skimmer's driver when the walk is planned");
        Assert.AreEqual(SurfaceType.TransportType.Foot, TransportFor(GoalMoveToPosition.VehicleUse.NoVehicle));
    }

    [TestMethod]
    public void AMoveThatMayUseTheVehicleKeepsTheVehiclesTransport()
    {
        Assert.AreEqual(SurfaceType.TransportType.Air, TransportFor(GoalMoveToPosition.VehicleUse.FreeUpAfterUse));
    }
}
