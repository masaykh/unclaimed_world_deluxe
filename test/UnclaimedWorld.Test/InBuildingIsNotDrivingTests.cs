using System.Reflection;
using System.Runtime.CompilerServices;
using UWGame;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Snapshots;

namespace UnclaimedWorld.Test;

/// <summary>
/// A colonist inside a building is not driving it. Entity.GetDrivenVehicle and GetVehicle let a
/// container that is not ICrew through, so a colonist resting in a hut "drove" the hut and
/// GoalEvaluator.GetEvaluatorTimeCostOfDistance read the hut's null Locomotor (Kastuk, 2026-11-14,
/// on maps with no vehicles).
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class InBuildingIsNotDrivingTests
{
    private static readonly FieldInfo ContainedBy = typeof(Entity).GetField("containedBy", BindingFlags.Instance | BindingFlags.NonPublic)!;

    private Entity hut = null!;
    private Entity colonist = null!;

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
        hut = NewEntity(9201);
        hut.Contains = (Container)RuntimeHelpers.GetUninitializedObject(typeof(UpgradableBuildingContainer));
        colonist = NewEntity(9202);
        ContainedBy.SetValue(colonist, hut.EntityID);
    }

    [TestCleanup]
    public void Cleanup()
    {
        hut.RemoveIDEntry();
        colonist.RemoveIDEntry();
    }

    [TestMethod]
    public void AColonistInAHutDrivesNothing()
    {
        Assert.IsTrue(colonist.GetDrivenVehicle(out var driven));
        Assert.IsNull(driven, "a hut is not a vehicle");
        Assert.IsTrue(colonist.GetVehicle(out var vehicle));
        Assert.IsNull(vehicle, "a hut is not a vehicle");
    }
}
