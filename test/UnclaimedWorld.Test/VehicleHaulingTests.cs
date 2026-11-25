using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Vehicles;

namespace UnclaimedWorld.Test;

[TestClass]
[DoNotParallelize]
public sealed class VehicleHaulingTests
{
    private static readonly MethodInfo CanUseVehicle = typeof(EvaluateHaulingJobs).GetMethod(
        "CanUseVehicleForHauling", BindingFlags.NonPublic | BindingFlags.Static)!;

    [ClassInitialize]
    public static void Initialize(TestContext context)
    {
        if (!GameData.Instance.AllEntityTypes.ContainsKey("entity:human"))
        {
            GameData.Instance.AllEntityTypes["entity:human"] = new EntityType
            {
                LocomotorType = new LocomotorType
                {
                    LeggedLocomotorType = new LeggedLocomotorType { WalkNormalSpeed = 20f }
                }
            };
        }
    }

    private static IKnownEntityData Known(Func<MethodInfo, object?[]?, object?> handler)
    {
        var proxy = DispatchProxy.Create<IKnownEntityData, KnownDataProxy>();
        ((KnownDataProxy)(object)proxy).Handler = handler;
        return proxy;
    }

    private static PassengerOrCargoSlot CargoSlot(float capacity, float used)
    {
        var slot = new PassengerOrCargoSlot
        {
            PassengerOrCargoSlotType = new PassengerOrCargoSlotType { CargoSlotType = new CargoSlotType(capacity) }
        };
        slot.CargoSlot = new CargoSlot(slot) { BulkCarried = used };
        return slot;
    }

    [TestMethod]
    public void MissingPassengerAttachmentPointsAreOptional()
    {
        var model = new UWGame.ClientSide.ModelData();
        Assert.IsNull(model.GetAttachPointFromKeyName("Cargo"));
        Assert.IsNull(model.GetAttachPointFromTag("Cargo"));
    }

    [TestMethod]
    public void VehicleUsesItsOwnCargoStorageForHaulingEligibility()
    {
        PassengerOrCargoSlot.CreateLookupCollection();
        if (!GameData.Instance.AllStorageConditions.ContainsKey("isolated"))
            GameData.Instance.AllStorageConditions["isolated"] = new StorageCondition { KeyName = "isolated" };
        var type = new VehicleContainerType(8);
        type.Initialize();
        var vehicle = (Entity)RuntimeHelpers.GetUninitializedObject(typeof(Entity));
        vehicle.Components = new();
        typeof(Entity).GetProperty("EntityType")!.SetValue(vehicle, new EntityType { ContainerType = type });
        typeof(UWGame.GameObject).GetField("location", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(vehicle, (Vector3?)Vector3.Zero);
        vehicle.Contains = new VehicleContainer(vehicle);
        var item = Known((method, _) => method.Name == "get_Bulk"
            ? 3f : throw new InvalidOperationException(method.Name));

        Assert.IsNull(vehicle.AgentStorage);
        Assert.IsTrue(vehicle.IsVehicleValidForHauling(null!, item));
    }

    [TestMethod]
    public void VehicleAccountsForCargoWhenLoadingAndUnloading()
    {
        Entity.CreateLookupCollection();
        PassengerOrCargoSlot.CreateLookupCollection();
        if (!GameData.Instance.AllStorageConditions.ContainsKey("isolated"))
            GameData.Instance.AllStorageConditions["isolated"] = new StorageCondition { KeyName = "isolated" };

        var type = new VehicleContainerType(8)
        {
            PassengerOrCargoSlotTypes = new[]
            {
                new PassengerOrCargoSlotType { CargoSlotType = new CargoSlotType(4) },
                new PassengerOrCargoSlotType { CargoSlotType = new CargoSlotType(4) }
            }
        };
        type.Initialize();
        var parent = (Entity)RuntimeHelpers.GetUninitializedObject(typeof(Entity));
        parent.Components = new();
        typeof(Entity).GetProperty("EntityType")!.SetValue(parent, new EntityType { ContainerType = type });
        typeof(Entity).GetField("id", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(parent, (EntityID)100010);
        var item = (Entity)RuntimeHelpers.GetUninitializedObject(typeof(Entity));
        item.Components = new();
        typeof(Entity).GetProperty("EntityType")!.SetValue(item, new EntityType());
        typeof(Entity).GetField("id", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(item, (EntityID)100011);
        typeof(Entity).GetField("bulk", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(item, 5f);
        LookUp<Entity, EntityID>.Add(parent.EntityID, parent);
        LookUp<Entity, EntityID>.Add(item.EntityID, item);
        var vehicle = new VehicleContainer(parent);
        parent.Contains = vehicle;
        var previousMap = UWGame.The.Map;
        UWGame.The.Map = (MapManager)RuntimeHelpers.GetUninitializedObject(typeof(MapManager));
        var add = typeof(VehicleContainer).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single(method => method.Name == "AddToContainList" && method.GetParameters().Length == 8);
        var remove = typeof(VehicleContainer).GetMethod("RemoveFromContain", BindingFlags.Instance | BindingFlags.NonPublic)!;
        try
        {
            vehicle.Slots[0].CargoSlot.TargetedByHauler = parent;
            Assert.AreEqual(1, vehicle.GetCargoSlotsForLoading(1).Count);
            Assert.AreSame(vehicle.Slots[1], vehicle.GetCargoSlotsForLoading(1)[0]);
            vehicle.Slots[0].CargoSlot.TargetedByHauler = null;
            var slots = vehicle.GetCargoSlotsForLoading(item.Bulk);
            Assert.IsTrue((bool)add.Invoke(vehicle, new object?[] { item, null, null, slots, false, false, false, null })!);
            Assert.AreEqual(4f, vehicle.Slots[0].CargoSlot.BulkCarried);
            Assert.AreEqual(1f, vehicle.Slots[1].CargoSlot.BulkCarried);
            Assert.AreEqual(5f, vehicle.TotalStored);

            Assert.IsTrue((bool)remove.Invoke(vehicle, new object?[] { item, null })!);
            Assert.AreEqual(0f, vehicle.Slots[0].CargoSlot.BulkCarried);
            Assert.AreEqual(0f, vehicle.Slots[1].CargoSlot.BulkCarried);
            Assert.AreEqual(0f, vehicle.TotalStored);
        }
        finally
        {
            UWGame.The.Map = previousMap;
            foreach (var slot in vehicle.Slots) slot.Destroy();
            parent.RemoveIDEntry();
            item.RemoveIDEntry();
        }
    }

    [TestMethod]
    public void VehicleMustHaveEnoughAvailableCargoSlotCapacity()
    {
        var item = Known((method, _) => method.Name switch
        {
            "get_Bulk" => 3f,
            "get_EntityID" => (EntityID)42,
            _ => throw new InvalidOperationException(method.Name)
        });
        var slots = new List<PassengerOrCargoSlot> { CargoSlot(4, 2) };
        float stored = 0;
        var vehicle = Known((method, _) => method.Name switch
        {
            "IsVehicleValidForHauling" => true,
            "ContainsEntity" => false,
            "get_TotalItemStorageCapacity" => (float?)8f,
            "get_TotalStored" => (float?)stored,
            "GetCargoSlotsForLoading" => slots,
            _ => throw new InvalidOperationException(method.Name)
        });

        Assert.IsFalse((bool)CanUseVehicle.Invoke(null, new object?[] { null, vehicle, item })!);
        slots.Add(CargoSlot(2, 0));
        Assert.IsTrue((bool)CanUseVehicle.Invoke(null, new object?[] { null, vehicle, item })!);
        stored = 6;
        Assert.IsFalse((bool)CanUseVehicle.Invoke(null, new object?[] { null, vehicle, item })!);
    }

    [TestMethod]
    public void LoadedVehicleSpeedUsesTheTravelLocomotor()
    {
        var vehicle = (Entity)RuntimeHelpers.GetUninitializedObject(typeof(Entity));
        vehicle.Components = new();
        typeof(Entity).GetProperty("EntityType")!.SetValue(vehicle, new EntityType
        {
            LocomotorType = new LocomotorType { VTOLLocomotorType = new VTOLLocomotorType { MaxAirSpeed = 180f } },
            ContainerType = new VehicleContainerType { Transport = SurfaceType.TransportType.OffRoad }
        });
        vehicle.Components[typeof(Locomotor)] = new Locomotor(vehicle);

        Assert.AreEqual(180f, vehicle.CalculateLoadedVehicleSpeed(5f));
    }

    [TestMethod]
    public void AirTimeIncludesBothFlightLegsAndLandingOverhead()
    {
        var type = new EntityType
        {
            LocomotorType = new LocomotorType
            {
                VTOLLocomotorType = new VTOLLocomotorType { MaxAirSpeed = 10, MaxVerticalMoveSpeed = 10, CruiseAltitude = 20 }
            },
            ContainerType = new VehicleContainerType
            {
                Aircraft = new AircraftType()
            }
        };
        var vehicle = Known((method, _) => method.Name == "get_EntityType"
            ? type : throw new InvalidOperationException(method.Name));

        double time = EvaluateHaulingJobs.EstimateAirTime(vehicle, new Vector3(30, 0, 0), Vector3.Zero, 10, new Vector3(10, 0, 0));
        Assert.AreEqual(9d, time, 0.0001);
        Assert.AreEqual(9d, EvaluateHaulingJobs.EstimateAirTime(vehicle, new Vector3(30, 0, 0), Vector3.Zero), 0.0001);
    }
}

public class KnownDataProxy : DispatchProxy
{
    public Func<MethodInfo, object?[]?, object?> Handler { get; set; } = null!;

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) => Handler(targetMethod!, args);
}
