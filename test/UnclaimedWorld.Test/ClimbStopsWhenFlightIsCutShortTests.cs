using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using UWGame;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Vehicles;

namespace UnclaimedWorld.Test;

/// <summary>
/// Only a finished takeoff or landing zeroed the aircraft's climb speed. A task cancelled during
/// the climb left it climbing, and the landing that followed started from that upward speed with
/// 30 px/s per second to turn it round: about a second more of climbing (Lars's flight audit, 2026-11-17).
/// GoalLand, and GoalTakeoff for an aircraft already in the air, now start from a standstill.
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class ClimbStopsWhenFlightIsCutShortTests
{
    private const BindingFlags AnyInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private static readonly MethodInfo Activate = typeof(Goal).GetMethod("Activate", AnyInstance)!;

    private const float ClimbSpeed = 40f;

    private Entity skimmer = null!;
    private Entity pilot = null!;
    private Aircraft aircraft = null!;

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
        skimmer = NewEntity(9401);
        typeof(Entity).GetProperty("EntityType")!.SetValue(skimmer,
            new EntityType("entity:skimmer") { ContainerType = new VehicleContainerType { Aircraft = new AircraftType() } });
        skimmer.Components[typeof(Locomotor)] = (Locomotor)RuntimeHelpers.GetUninitializedObject(typeof(Locomotor));
        // Cut short half way up, still climbing.
        typeof(GameObject).GetField("location", AnyInstance)!.SetValue(skimmer, (Vector3?)new Vector3(100f, 100f, 120f));
        typeof(Entity).GetField("mapPosition", AnyInstance)!.SetValue(skimmer, (Point?)new Point(3, 3));
        aircraft = new Aircraft { VerticalSpeed = ClimbSpeed };
        var vehicle = (VehicleContainer)RuntimeHelpers.GetUninitializedObject(typeof(VehicleContainer));
        vehicle.Aircraft = aircraft;
        skimmer.Contains = vehicle;

        pilot = NewEntity(9402);
        typeof(Entity).GetField("containedBy", AnyInstance)!.SetValue(pilot, (EntityID?)skimmer.EntityID);
        vehicle.DrivenBy = pilot;
    }

    [TestCleanup]
    public void Cleanup()
    {
        skimmer.RemoveIDEntry();
        pilot.RemoveIDEntry();
    }

    private void Start(Goal goal)
    {
        goal.entity = pilot;
        Activate.Invoke(goal, null);
        Assert.IsTrue(goal.isActive(), "the pilot drives the skimmer, so the goal starts");
    }

    [TestMethod]
    public void ALandingAfterACutShortClimbStartsFromAStandstill()
    {
        Start(new GoalLand());
        Assert.AreEqual(0f, aircraft.VerticalSpeed, "not still climbing at the speed the cancelled takeoff left");
        Assert.AreEqual(120f, skimmer.PlaySiteLocation.Z, "a speed is reset, the skimmer does not move");
    }

    [TestMethod]
    public void ATakeoffAlreadyInTheAirStartsFromAStandstill()
    {
        aircraft.VerticalSpeed = -ClimbSpeed;
        Start(new GoalTakeoff());
        Assert.AreEqual(0f, aircraft.VerticalSpeed, "not still descending at the speed the cancelled landing left");
        Assert.AreEqual(120f, skimmer.PlaySiteLocation.Z);
    }
}
