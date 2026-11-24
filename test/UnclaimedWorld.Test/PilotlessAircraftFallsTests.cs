using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using UWGame;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Vehicles;

namespace UnclaimedWorld.Test;

/// <summary>
/// A pilot who died in the air left his skimmer hovering where he died, forever: only his flight
/// goals moved it, and they end with him (issue #26). Entity.Kill now asks
/// AircraftCrash.AircraftLosingItsPilot before it swaps his carcass into the seat, and
/// AircraftCrash.Start drops the aircraft on its BallisticLocomotor, the arrows' mover.
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class PilotlessAircraftFallsTests
{
    private const BindingFlags AnyInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    private Entity skimmer = null!;
    private Entity pilot = null!;
    private VehicleContainer vehicle = null!;
    private Locomotor locomotor = null!;

    private static Entity NewEntity(long id)
    {
        var entity = (Entity)RuntimeHelpers.GetUninitializedObject(typeof(Entity));
        entity.Components = new();
        typeof(Entity).GetField("id", AnyInstance)!.SetValue(entity, (EntityID)id);
        LookUp<Entity, EntityID>.Add(entity.EntityID, entity);
        return entity;
    }

    private void PutSkimmerAt(float height)
    {
        typeof(GameObject).GetField("location", AnyInstance)!.SetValue(skimmer, (Vector3?)new Vector3(100f, 100f, height));
    }

    [TestInitialize]
    public void Setup()
    {
        Entity.CreateLookupCollection();
        skimmer = NewEntity(9601);
        typeof(Entity).GetProperty("EntityType")!.SetValue(skimmer,
            new EntityType("entity:skimmer") { ContainerType = new VehicleContainerType { Aircraft = new AircraftType() } });
        locomotor = (Locomotor)RuntimeHelpers.GetUninitializedObject(typeof(Locomotor));
        locomotor.Parent = skimmer;
        skimmer.Components[typeof(Locomotor)] = locomotor;
        PutSkimmerAt(120f);
        typeof(Entity).GetField("mapPosition", AnyInstance)!.SetValue(skimmer, (Point?)new Point(3, 3));
        vehicle = (VehicleContainer)RuntimeHelpers.GetUninitializedObject(typeof(VehicleContainer));
        vehicle.Aircraft = new Aircraft { VerticalSpeed = 40f };
        skimmer.Contains = vehicle;

        pilot = NewEntity(9602);
        pilot.DrivingVehicle = skimmer.EntityID;
        vehicle.DrivenBy = pilot;
    }

    [TestCleanup]
    public void Cleanup()
    {
        skimmer.RemoveIDEntry();
        pilot.RemoveIDEntry();
    }

    [TestMethod]
    public void APilotDyingInTheAirLeavesAnAircraftToFall()
    {
        Assert.AreSame(skimmer, AircraftCrash.AircraftLosingItsPilot(pilot));
    }

    [TestMethod]
    public void APilotDyingOnTheGroundLeavesItWhereItIs()
    {
        PutSkimmerAt(0f);
        Assert.IsNull(AircraftCrash.AircraftLosingItsPilot(pilot), "a parked skimmer has nowhere to fall");
    }

    [TestMethod]
    public void APassengerDyingDoesNotBringItDown()
    {
        pilot.DrivingVehicle = null;
        pilot.PassengerInVehicle = skimmer.EntityID;
        Assert.IsNull(AircraftCrash.AircraftLosingItsPilot(pilot));
    }

    [TestMethod]
    public void TheFallStartsWhereItWasAndNoClimbIsLeftOver()
    {
        AircraftCrash.Start(skimmer);

        Assert.IsNotNull(locomotor.BallisticLocomotor, "made for it: the skimmers' locomotor type has none");
        Assert.AreSame(locomotor, locomotor.BallisticLocomotor!.Parent);
        Assert.AreEqual(Locomotor.Mode.Ballistic, locomotor.CurrentMoveMode, "Locomotor.MoveTowardsLocation now moves it each update");
        Assert.AreEqual(new Vector3(100f, 100f, 120f), skimmer.PlaySiteLocation, "not moved, unlike Launch");
        Assert.AreEqual(0f, vehicle.Aircraft.VerticalSpeed, "the flight goals' climb speed is not the fall's");
        Assert.AreEqual(0f, locomotor.MoveSpeed);
    }

    [TestMethod]
    public void AHarderLandingDoesMoreDamageUpToALimit()
    {
        Assert.AreEqual(0f, AircraftCrash.Severity(0f), "set down, not crashed");
        Assert.AreEqual(1f, AircraftCrash.Severity(AircraftCrash.ReferenceImpactSpeed), 1e-6f);
        Assert.AreEqual(2f, AircraftCrash.Severity(10f * AircraftCrash.ReferenceImpactSpeed), "capped");
    }

    [TestMethod]
    public void AFallingAircraftHitsNobodyOnTheWayDown()
    {
        // The arrows' collision responder hits with the missile's AttackType; a falling skimmer has
        // none, and a creature under it threw NullReferenceException.
        AircraftCrash.Start(skimmer);
        var responder = (CollisionResponder)RuntimeHelpers.GetUninitializedObject(typeof(CollisionResponder));
        responder.Parent = locomotor;
        var ballistic = new BallisticCollisionResponder(responder);
        var creature = NewEntity(9603);
        try
        {
            var under = (UWGame.SimSide.Collisions.Collidable<Entity>)RuntimeHelpers.GetUninitializedObject(typeof(UWGame.SimSide.Collisions.Collidable<Entity>));
            typeof(UWGame.SimSide.Collisions.Collidable<Entity>).GetField("parent", AnyInstance)!.SetValue(under, creature);
            ballistic.HandleSingleCollision(under);
        }
        finally
        {
            creature.RemoveIDEntry();
        }
    }
}
