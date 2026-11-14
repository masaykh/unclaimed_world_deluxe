using System.Reflection;
using System.Runtime.CompilerServices;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Vehicles;

namespace UnclaimedWorld.Test;

[TestClass]
[DoNotParallelize]
public sealed class VehicleContainmentTests
{
    private static readonly MethodInfo Add = typeof(VehicleContainer).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
        .Single(method => method.Name == "AddToContainList" && method.GetParameters().Length == 10);
    private static readonly MethodInfo Remove = typeof(VehicleContainer).GetMethod("RemoveFromContain", BindingFlags.Instance | BindingFlags.NonPublic)!;

    private static Entity NewEntity(long id)
    {
        var entity = (Entity)RuntimeHelpers.GetUninitializedObject(typeof(Entity));
        entity.Components = new();
        typeof(Entity).GetField("id", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(entity, (EntityID)id);
        return entity;
    }

    private static VehicleContainer NewVehicle(bool hasSeats = true)
    {
        PassengerOrCargoSlot.CreateLookupCollection();
        var type = new VehicleContainerType
        {
            PassengerOrCargoSlotTypes = hasSeats
                ? new[] { new PassengerOrCargoSlotType { PassengerSlotType = new PassengerSlotType { IsDriversSeat = true } } }
                : Array.Empty<PassengerOrCargoSlotType>()
        };
        type.Initialize();
        type.ItemStorageType = new ItemStorageType();
        var parent = NewEntity(100);
        typeof(Entity).GetProperty("EntityType")!.SetValue(parent, new EntityType { ContainerType = type });
        var vehicle = new VehicleContainer(parent);
        parent.Contains = vehicle;
        foreach (var slot in vehicle.Slots)
        {
            slot.Destroy();
        }
        vehicle.Slots.Clear();
        return vehicle;
    }

    private static PassengerOrCargoSlot NewSeat(bool driverSeat = false, float cargoCapacity = 0)
    {
        var seat = new PassengerOrCargoSlot
        {
            PassengerSlot = new PassengerSlot(),
            DriversSlot = driverSeat ? new DriversSlot() : null,
            PassengerOrCargoSlotType = new PassengerOrCargoSlotType
            {
                PassengerSlotType = new PassengerSlotType { IsDriversSeat = driverSeat },
                CargoSlotType = cargoCapacity > 0 ? new CargoSlotType(cargoCapacity) : null
            }
        };
        if (cargoCapacity > 0)
        {
            seat.CargoSlot = new CargoSlot(seat);
        }
        return seat;
    }

    private static bool Board(VehicleContainer vehicle, Entity entity, PassengerOrCargoSlot seat, bool asDriver = false) =>
        (bool)Add.Invoke(vehicle, new object?[] { entity, null, null, null, false, false, false, null, seat, asDriver })!;

    private static bool Unboard(VehicleContainer vehicle, Entity entity) =>
        (bool)Remove.Invoke(vehicle, new object?[] { entity, null })!;

    [TestMethod]
    public void PassengerInDriversSeatIsNotTheDriverAndRemovalFreesCapacity()
    {
        var vehicle = NewVehicle();
        var passenger = NewEntity(101);
        var seat = NewSeat(driverSeat: true, cargoCapacity: 5);
        vehicle.Slots.Add(seat);
        seat.PassengerSlot.TargetedBy = passenger;

        Assert.IsTrue(Board(vehicle, passenger, seat));
        Assert.AreSame(passenger, seat.PassengerSlot.Passenger);
        Assert.IsNull(seat.PassengerSlot.TargetedBy);
        Assert.IsNull(vehicle.DrivenBy);
        Assert.IsNull(seat.DriversSlot.Driver);
        Assert.AreEqual(vehicle.Parent.EntityID, passenger.PassengerInVehicle);
        Assert.AreEqual(1, vehicle.GetNoOfAgentsInside());
        Assert.AreEqual(5f, vehicle.CargoCapacityTakenUpByPassengers);

        Assert.IsTrue(Unboard(vehicle, passenger));
        Assert.IsNull(passenger.PassengerInVehicle);
        Assert.IsNull(seat.PassengerSlot.Passenger);
        Assert.AreEqual(0, vehicle.Passengers.Count);
        Assert.AreEqual(0, vehicle.GetNoOfAgentsInside());
        Assert.AreEqual(0f, vehicle.CargoCapacityTakenUpByPassengers);
    }

    [TestMethod]
    public void DriverBoardsAndLeavesThroughConstructorCreatedGarrison()
    {
        var vehicle = NewVehicle();
        var driver = NewEntity(102);
        var seat = NewSeat(driverSeat: true);
        vehicle.Slots.Add(seat);
        seat.DriversSlot.TargetedBy = driver;

        Assert.IsTrue(Board(vehicle, driver, seat, asDriver: true));
        Assert.AreSame(driver, vehicle.DrivenBy);
        Assert.AreSame(driver, seat.DriversSlot.Driver);
        Assert.IsNull(seat.DriversSlot.TargetedBy);
        Assert.AreEqual(vehicle.Parent.EntityID, driver.DrivingVehicle);
        Assert.AreEqual(1, vehicle.GetNoOfAgentsInside());

        Assert.IsTrue(Unboard(vehicle, driver));
        Assert.IsNull(vehicle.DrivenBy);
        Assert.IsNull(seat.DriversSlot.Driver);
        Assert.IsNull(driver.DrivingVehicle);
        Assert.AreEqual(0, vehicle.GetNoOfAgentsInside());
    }

    [TestMethod]
    public void VehicleWithoutSeatsDoesNotCreateGarrisonDuringBoarding()
    {
        var vehicle = NewVehicle(hasSeats: false);
        var seat = NewSeat(driverSeat: true);
        vehicle.Slots.Add(seat);

        Assert.IsFalse(Board(vehicle, NewEntity(105), seat, asDriver: true));
        Assert.IsNull(typeof(VehicleContainer).GetField("garrison", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(vehicle));
        Assert.IsNull(vehicle.DrivenBy);
    }

    [TestMethod]
    public void UnavailableOrForeignSeatsAreRejectedWithoutChangingOccupants()
    {
        var vehicle = NewVehicle();
        var passenger = NewEntity(103);
        var other = NewEntity(104);
        var seat = NewSeat(driverSeat: true, cargoCapacity: 5);
        vehicle.Slots.Add(seat);

        Assert.IsFalse(Board(vehicle, passenger, NewSeat()));
        Assert.IsFalse(Board(vehicle, passenger, NewSeat(), asDriver: true));
        seat.PassengerSlot.TargetedBy = other;
        Assert.IsFalse(Board(vehicle, passenger, seat));
        seat.PassengerSlot.TargetedBy = null;
        seat.CargoSlot.BulkCarried = 1;
        Assert.IsFalse(Board(vehicle, passenger, seat));
        seat.CargoSlot.BulkCarried = 0;
        Assert.IsTrue(Board(vehicle, passenger, seat));
        Assert.IsFalse(Board(vehicle, other, seat, asDriver: true));
        Assert.AreSame(passenger, seat.PassengerSlot.Passenger);
        Assert.IsNull(vehicle.DrivenBy);
        Assert.AreEqual(1, vehicle.GetNoOfAgentsInside());
    }

    [TestMethod]
    public void DriverSeatOccupiedByPassengerOrCargoIsNotAvailable()
    {
        var vehicle = NewVehicle();
        var seat = NewSeat(driverSeat: true, cargoCapacity: 5);
        seat.PassengerOrCargoSlotType.PassengerSlotType.Comfort = 1;
        vehicle.Slots.Add(seat);
        Assert.AreSame(seat, vehicle.GetFreeDriversSlot());

        seat.PassengerSlot.Passenger = NewEntity(106);
        Assert.IsNull(vehicle.GetFreeDriversSlot());
        seat.PassengerSlot.Passenger = null;
        seat.CargoSlot.BulkCarried = 1;
        Assert.IsNull(vehicle.GetFreeDriversSlot());
        seat.CargoSlot.BulkCarried = 0;
        seat.CargoSlot.TargetedByHauler = NewEntity(107);
        Assert.IsNull(vehicle.GetFreeDriversSlot());
    }

    [TestMethod]
    public void SeatSnapshotRestoresIdentityOccupantReservationAndCargo()
    {
        Entity.CreateLookupCollection();
        PassengerOrCargoSlot.CreateLookupCollection();
        var vehicle = NewVehicle();
        var passenger = NewEntity(100001);
        var target = NewEntity(100002);
        vehicle.Parent = NewEntity(100003);
        vehicle.Parent.Contains = vehicle;
        var type = new PassengerOrCargoSlotType
        {
            PassengerSlotType = new PassengerSlotType { IsDriversSeat = true },
            CargoSlotType = new CargoSlotType(9)
        };
        typeof(Entity).GetProperty("EntityType")!.SetValue(vehicle.Parent,
            new EntityType { ContainerType = new VehicleContainerType { PassengerOrCargoSlotTypes = new[] { type } } });
        LookUp<Entity, EntityID>.Add(vehicle.Parent.EntityID, vehicle.Parent);
        LookUp<Entity, EntityID>.Add(passenger.EntityID, passenger);
        LookUp<Entity, EntityID>.Add(target.EntityID, target);
        PassengerOrCargoSlot? loaded = null;
        try
        {
            var seat = new PassengerOrCargoSlot(vehicle)
            {
                PassengerOrCargoSlotType = type,
                PassengerSlot = new PassengerSlot { Passenger = passenger, TargetedBy = target },
                DriversSlot = new DriversSlot { TargetedBy = target }
            };
            seat.CargoSlot = new CargoSlot(seat) { BulkCarried = 3, TargetedByHauler = target };
            vehicle.Slots.Add(seat);
            var id = seat.ID;
            using var stream = new MemoryStream();
            var snapshotter = new Snapshotter { mode = Snapshotter.Mode.Save };
            typeof(Snapshotter).GetField("m_writer", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(snapshotter, new BinaryWriter(stream));
            snapshotter.DoISnapshot(seat, verifyField: false);
            seat.RemoveIDEntry();
            stream.Position = 0;
            snapshotter.mode = Snapshotter.Mode.Load;
            typeof(Snapshotter).GetField("m_reader", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(snapshotter, new BinaryReader(stream));
            loaded = (PassengerOrCargoSlot)snapshotter.DoISnapshot<PassengerOrCargoSlot>(null!, verifyField: false);
            vehicle.Slots[0] = loaded;
            loaded.LoadPostProcess(snapshotter);

            Assert.AreEqual(id, loaded.ID);
            Assert.AreSame(loaded, LookUp<PassengerOrCargoSlot, PassengerOrCargoSlotID>.FindByID(id));
            Assert.AreSame(type, loaded.PassengerOrCargoSlotType);
            Assert.AreSame(passenger, loaded.PassengerSlot.Passenger);
            Assert.AreSame(target, loaded.PassengerSlot.TargetedBy);
            Assert.AreSame(target, loaded.DriversSlot.TargetedBy);
            Assert.AreEqual(3f, loaded.CargoSlot.BulkCarried);
            Assert.AreSame(target, loaded.CargoSlot.TargetedByHauler);
        }
        finally
        {
            loaded?.RemoveIDEntry();
            vehicle.Parent.RemoveIDEntry();
            passenger.RemoveIDEntry();
            target.RemoveIDEntry();
        }
    }
}
