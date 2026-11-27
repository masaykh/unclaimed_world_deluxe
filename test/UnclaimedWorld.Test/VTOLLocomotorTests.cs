using System.Reflection;
using System.Runtime.CompilerServices;
using UWGame;
using UWGame.Control.Replays;
using UWGame.SimSide;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Vehicles;

namespace UnclaimedWorld.Test;

[TestClass]
[DoNotParallelize]
public sealed class VTOLLocomotorTests
{
    private const BindingFlags AnyInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private Sim? previousSim;
    private Entity pilot = null!;
    private Entity aircraft = null!;

    [TestInitialize]
    public void Initialize()
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

        previousSim = The.Sim;
        The.Sim = (Sim)RuntimeHelpers.GetUninitializedObject(typeof(Sim));
        typeof(Sim).GetField("gameplayRandomGenerator", AnyInstance)!.SetValue(The.Sim, new RandomGenerator(1, RandomGenerator.GeneratorType.Client));
        Entity.CreateLookupCollection();
        pilot = NewEntity(90001, new EntityType("entity:testpilot")
        {
            LocomotorType = new LocomotorType
            {
                LeggedLocomotorType = new LeggedLocomotorType { WalkSlowSpeed = 10f, WalkNormalSpeed = 20f, WalkFastSpeed = 30f }
            }
        });
        aircraft = NewEntity(90002, new EntityType("entity:testaircraft")
        {
            LocomotorType = new LocomotorType { VTOLLocomotorType = new VTOLLocomotorType { MaxAirSpeed = 180f } },
            ContainerType = new VehicleContainerType { Transport = SurfaceType.TransportType.Air }
        });
    }

    [TestCleanup]
    public void Cleanup()
    {
        pilot?.RemoveIDEntry();
        aircraft?.RemoveIDEntry();
        The.Sim = previousSim!;
    }

    private static Entity NewEntity(long id, EntityType type)
    {
        var entity = (Entity)RuntimeHelpers.GetUninitializedObject(typeof(Entity));
        entity.Components = new();
        typeof(Entity).GetField("id", AnyInstance)!.SetValue(entity, (EntityID)id);
        typeof(Entity).GetProperty("EntityType")!.SetValue(entity, type);
        LookUp<Entity, EntityID>.Add(entity.EntityID, entity);
        entity.Components[typeof(Locomotor)] = new Locomotor(entity);
        return entity;
    }

    [TestMethod]
    public void BoardingUsesAircraftSpeedAndDoesNotLowerTheTravelScore()
    {
        const float distance = 1800f;
        double beforeTime = GoalEvaluator.GetEvaluatorTimeCostOfDistance(pilot, distance);
        double beforeScore = GoalEvaluator.GetTravelScoreFromDistance(pilot, distance);
        var container = (VehicleContainer)RuntimeHelpers.GetUninitializedObject(typeof(VehicleContainer));
        container.Parent = aircraft;
        container.DrivenBy = pilot;
        aircraft.Contains = container;
        typeof(Entity).GetField("containedBy", AnyInstance)!.SetValue(pilot, (EntityID?)aircraft.EntityID);
        pilot.DrivingVehicle = aircraft.EntityID;

        double afterTime = GoalEvaluator.GetEvaluatorTimeCostOfDistance(pilot, distance);
        double afterScore = GoalEvaluator.GetTravelScoreFromDistance(pilot, distance);

        Assert.AreEqual(180f, aircraft.Locomotor.CurrentMaximumSpeedForEvaluator);
        Assert.AreEqual(distance / 20d * PlainsType.Instance.MovementFactor(SurfaceType.TransportType.Foot, SurfaceType.TerrainFeatures.None), beforeTime, 0.0001);
        Assert.AreEqual(distance / 180d * PlainsType.Instance.MovementFactor(SurfaceType.TransportType.Air, SurfaceType.TerrainFeatures.None), afterTime, 0.0001);
        Assert.IsTrue(afterScore > beforeScore);
    }

    [TestMethod]
    public void UsualTravelSpeedSurvivesTransientModesAndRespectsDisability()
    {
        Assert.IsInstanceOfType<VTOLLocomotor>(aircraft.TravelLocomotor);
        Assert.IsNull(aircraft.Locomotor.LeggedLocomotor);
        Assert.AreEqual(180f, aircraft.Locomotor.CurrentMaximumSpeedForEvaluator);
        Assert.AreEqual(180f, aircraft.Locomotor.CalculateSpeed(500f));
        aircraft.Locomotor.CurrentMoveMode = Locomotor.Mode.Ballistic;
        Assert.AreEqual(180f, aircraft.Locomotor.CurrentMaximumSpeedForEvaluator);
        aircraft.Locomotor.CurrentMoveMode = Locomotor.Mode.None;
        aircraft.Locomotor.ImpairMovement(0.25f);
        Assert.AreEqual(135f, aircraft.Locomotor.CurrentMaximumSpeedForEvaluator);
        Assert.AreEqual(180f, aircraft.CalculateLoadedVehicleSpeed(500f));
    }

    [TestMethod]
    public void LeggedEvaluatorSpeedIgnoresTargetSpeedAndKeepsDisability()
    {
        var locomotor = pilot.Locomotor;
        Assert.IsInstanceOfType<LeggedLocomotor>(pilot.TravelLocomotor);
        locomotor.LeggedLocomotor.TargetSpeed = Goal.MovementSpeeds.WalkSlowly;
        Assert.AreEqual(10f, locomotor.GetTargetSpeed());
        Assert.AreEqual(20f, locomotor.CurrentMaximumSpeedForEvaluator);
        locomotor.CurrentMoveMode = Locomotor.Mode.Ballistic;
        locomotor.ImpairMovement(0.25f);
        Assert.AreEqual(15f, locomotor.CurrentMaximumSpeedForEvaluator);
    }

    [TestMethod]
    public void SnapshotRestoresVTOLParentAndSpeed()
    {
        aircraft.Locomotor.VTOLLocomotor.SteeringType = SteeringType.Pivot;
        aircraft.Locomotor.VTOLLocomotor.VerticalSpeed = -42f;
        using var stream = new MemoryStream();
        var snapshotter = new Snapshotter { mode = Snapshotter.Mode.Save };
        typeof(Snapshotter).GetField("m_writer", AnyInstance)!.SetValue(snapshotter, new BinaryWriter(stream));
        snapshotter.DoISnapshot(aircraft.Locomotor, verifyField: false);
        stream.Position = 0;
        snapshotter.mode = Snapshotter.Mode.Load;
        typeof(Snapshotter).GetField("m_reader", AnyInstance)!.SetValue(snapshotter, new BinaryReader(stream));
        bool wasSnapshotting = Snapshotter.IsSnapshotting;
        try
        {
            Snapshotter.IsSnapshotting = true;
            var restored = (Locomotor)snapshotter.DoISnapshot<Locomotor>(null!, verifyField: false);
            restored.LoadPostProcess(snapshotter);

            Assert.AreSame(aircraft, restored.Parent);
            Assert.AreSame(restored, restored.VTOLLocomotor.Parent);
            Assert.AreEqual(180f, restored.CurrentMaximumSpeedForEvaluator);
            Assert.AreEqual(SteeringType.Pivot, restored.VTOLLocomotor.SteeringType);
            Assert.AreEqual(-42f, restored.VTOLLocomotor.VerticalSpeed);
        }
        finally
        {
            Snapshotter.IsSnapshotting = wasSnapshotting;
        }
    }

    [TestMethod]
    public void TakeoffLandingEstimateTracksVerticalParameters()
    {
        var type = new VTOLLocomotorType { CruiseAltitude = 200f, MaxVerticalMoveSpeed = 100f };
        Assert.AreEqual(3f, type.EstimatedTakeOffLandingTime, 0.0001f);

        type.CruiseAltitude = 300f;
        Assert.AreEqual(4.5f, type.EstimatedTakeOffLandingTime, 0.0001f);
        type.MaxVerticalMoveSpeed = 50f;
        Assert.AreEqual(9f, type.EstimatedTakeOffLandingTime, 0.0001f);
        type.MaxVerticalMoveSpeed = 0f;
        Assert.IsTrue(float.IsPositiveInfinity(type.EstimatedTakeOffLandingTime));
        type.MaxVerticalMoveSpeed = -1f;
        Assert.IsTrue(float.IsPositiveInfinity(type.EstimatedTakeOffLandingTime));
        type.CruiseAltitude = 0f;
        Assert.AreEqual(0f, type.EstimatedTakeOffLandingTime);
    }

    [TestMethod]
    public void AircraftDefinitionsUseVTOLSpeedWithoutPlaceholderLegs()
    {
        var types = new List<EntityType>();
        bool addedBody = !GameData.Instance.AllBodyTypes.ContainsKey("mulevehicle");
        if (addedBody)
            GameData.Instance.AllBodyTypes["mulevehicle"] = new BodyType("mulevehicle");
        try
        {
            typeof(BaseDataLoader).GetMethod("InitMiscEntityTypes", BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, new object[] { types });
            foreach (string key in new[] { "entity:skimmer", "entity:harpy" })
            {
                var type = types.Single(t => t.KeyName == key);
                Assert.IsNull(type.LocomotorType.LeggedLocomotorType);
                Assert.AreEqual(180f, type.LocomotorType.VTOLLocomotorType.MaxAirSpeed);
                Assert.AreEqual(90f, type.LocomotorType.VTOLLocomotorType.MaxVerticalMoveSpeed);
                Assert.AreEqual(200f, type.LocomotorType.VTOLLocomotorType.CruiseAltitude);
                Assert.AreEqual(10f / 3f, type.LocomotorType.VTOLLocomotorType.EstimatedTakeOffLandingTime, 0.0001f);
            }
        }
        finally
        {
            if (addedBody)
                GameData.Instance.AllBodyTypes.Remove("mulevehicle");
        }
    }
}
