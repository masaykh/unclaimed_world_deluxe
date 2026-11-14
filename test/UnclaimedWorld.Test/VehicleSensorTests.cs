using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using UWGame;
using UWGame.SimSide;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Constants;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Vehicles;

namespace UnclaimedWorld.Test;

[TestClass]
[DoNotParallelize]
public sealed class VehicleSensorTests
{
    private static readonly FieldInfo MapPositionField = typeof(Entity).GetField("mapPosition", BindingFlags.Instance | BindingFlags.NonPublic)!;
    private static readonly PropertyInfo MapPositionProperty = typeof(Entity).GetProperty("MapPosition")!;
    private static readonly FieldInfo ContainedBy = typeof(Entity).GetField("containedBy", BindingFlags.Instance | BindingFlags.NonPublic)!;
    private static readonly FieldInfo LightLevel = typeof(UWGame.SimSide.DateAndTime).GetField("lightLevel", BindingFlags.Instance | BindingFlags.NonPublic)!;
    private Sim previousSim = null!;
    private MapManager previousMap = null!;
    private AIConstants previousAIConstants = null!;
    private Entity vehicle = null!;
    private Entity pilot = null!;
    private Sensor sensor = null!;

    [TestInitialize]
    public void Initialize()
    {
        previousSim = The.Sim;
        previousMap = The.Map;
        previousAIConstants = GameData.Instance.AIConstants;
        GameData.Instance.AIConstants = new AIConstants();
        Entity.CreateLookupCollection();
        The.Sim = (Sim)RuntimeHelpers.GetUninitializedObject(typeof(Sim));
        The.Sim.DateAndTime = (UWGame.SimSide.DateAndTime)RuntimeHelpers.GetUninitializedObject(typeof(UWGame.SimSide.DateAndTime));
        LightLevel.SetValue(The.Sim.DateAndTime, 1f);
        The.Map = (MapManager)RuntimeHelpers.GetUninitializedObject(typeof(MapManager));
        The.Map.mapTileWidth = 20;
        The.Map.mapTileHeight = 20;
        The.Map.TileMap = Enumerable.Range(0, 20)
            .Select(x => Enumerable.Range(0, 20).Select(y => new TerrainTile { TilePos = new TilePos(x, y) }).ToArray())
            .ToArray();

        vehicle = NewEntity(1);
        pilot = NewEntity(2);
        var vehicleType = new VehicleContainerType
        {
            PassengerOrCargoSlotTypes = new[] { new PassengerOrCargoSlotType { PassengerSlotType = new PassengerSlotType() } }
        };
        vehicleType.Initialize();
        vehicleType.ItemStorageType = new ItemStorageType();
        vehicleType.PassengerOrCargoSlotTypes = Array.Empty<PassengerOrCargoSlotType>();
        typeof(Entity).GetProperty("EntityType")!.SetValue(vehicle, new EntityType("entity:skimmer") { ContainerType = vehicleType });
        var vehicleContainer = new VehicleContainer(vehicle);
        vehicle.Contains = vehicleContainer;
        var garrison = typeof(VehicleContainer).GetField("garrison", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(vehicleContainer)!;
        garrison.GetType().GetMethod("Add")!.Invoke(garrison, new object[] { pilot.EntityID });
        typeof(Entity).GetProperty("EntityType")!.SetValue(pilot, new EntityType("entity:human")
        {
            SensorType = new SensorType { Range = 96f, RangeAtNight = 96f }
        });
        var allegiance = new Allegiance { SharedKnowledge = new SharedKnowledge() };
        var intelligence = (Intelligence)RuntimeHelpers.GetUninitializedObject(typeof(Intelligence));
        intelligence.Parent = pilot;
        intelligence.Allegiance = allegiance;
        pilot.Components.Add(typeof(Intelligence), intelligence);
        var vehicleIntelligence = (Intelligence)RuntimeHelpers.GetUninitializedObject(typeof(Intelligence));
        vehicleIntelligence.Parent = vehicle;
        vehicleIntelligence.Allegiance = allegiance;
        vehicle.Components.Add(typeof(Intelligence), vehicleIntelligence);
        sensor = new Sensor { Parent = pilot, IsActive = true };
        pilot.Components.Add(typeof(Sensor), sensor);
        ContainedBy.SetValue(pilot, vehicle.EntityID);
        MapPositionField.SetValue(vehicle, new Point(5, 5));
        sensor.SeeTilesInRange(pilot.MapPosition!.Value);
    }

    [TestCleanup]
    public void Cleanup()
    {
        vehicle.RemoveIDEntry();
        pilot.RemoveIDEntry();
        The.Sim = previousSim;
        The.Map = previousMap;
        GameData.Instance.AIConstants = previousAIConstants;
    }

    private static Entity NewEntity(long id)
    {
        var entity = (Entity)RuntimeHelpers.GetUninitializedObject(typeof(Entity));
        entity.Components = new();
        typeof(Entity).GetField("id", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(entity, (EntityID)id);
        LookUp<Entity, EntityID>.Add(entity.EntityID, entity);
        return entity;
    }

    private void MoveVehicle(Point position) => MapPositionProperty.SetValue(vehicle, position);

    private void UpdateSensor() => sensor.UpdatePlaySite(new GameTime());

    [TestMethod]
    public void PilotVisionFollowsMovingVehicleAndForgetsOldTiles()
    {
        var oldTile = The.Map.TileMap[3][5];
        var newTile = The.Map.TileMap[8][5];
        Assert.IsTrue(oldTile.EntitiesThatSeeThisTile.Contains(pilot));
        Assert.IsFalse(newTile.EntitiesThatSeeThisTile.Contains(pilot));

        MoveVehicle(new Point(6, 5));

        Assert.IsFalse(oldTile.EntitiesThatSeeThisTile.Contains(pilot));
        Assert.IsTrue(newTile.EntitiesThatSeeThisTile.Contains(pilot));
        Assert.IsTrue(sensor.tilesCurrentlySeen.Contains(newTile));
        Assert.IsFalse(sensor.tilesCurrentlySeen.Contains(oldTile));
    }

    [TestMethod]
    public void SensorRangeChangesStillRefreshStationaryPilotVision()
    {
        var outerTile = The.Map.TileMap[7][5];
        Assert.IsTrue(outerTile.EntitiesThatSeeThisTile.Contains(pilot));

        pilot.EntityType.SensorType.Range = 48f;
        UpdateSensor();
        Assert.IsFalse(outerTile.EntitiesThatSeeThisTile.Contains(pilot));

        pilot.EntityType.SensorType.Range = 96f;
        UpdateSensor();
        Assert.IsTrue(outerTile.EntitiesThatSeeThisTile.Contains(pilot));
    }

    [TestMethod]
    public void InactivePilotSensorIsNotRefreshedByVehicleMovement()
    {
        sensor.IsActive = false;
        var before = new HashSet<TerrainTile>(sensor.tilesCurrentlySeen);

        MoveVehicle(new Point(6, 5));

        Assert.IsTrue(before.SetEquals(sensor.tilesCurrentlySeen));
    }
}
