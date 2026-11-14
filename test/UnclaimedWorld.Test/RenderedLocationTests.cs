using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using UWGame;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.AI;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;
using Xclna.Xna.Animation;

namespace UnclaimedWorld.Test;

[TestClass]
[DoNotParallelize]
public sealed class RenderedLocationTests
{
    private static readonly FieldInfo LocationField = typeof(GameObject).GetField("location", BindingFlags.Instance | BindingFlags.NonPublic)!;
    private static readonly FieldInfo ContainedByField = typeof(Entity).GetField("containedBy", BindingFlags.Instance | BindingFlags.NonPublic)!;
    private readonly List<Entity> entities = new();

    [TestInitialize]
    public void Initialize() => Entity.CreateLookupCollection();

    [TestCleanup]
    public void Cleanup()
    {
        foreach (var entity in entities)
        {
            entity.RemoveIDEntry();
        }
    }

    private Entity NewEntity(long id, Vector3? location)
    {
        var entity = (Entity)RuntimeHelpers.GetUninitializedObject(typeof(Entity));
        entity.Components = new();
        typeof(Entity).GetField("id", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(entity, (EntityID)id);
        LocationField.SetValue(entity, location);
        LookUp<Entity, EntityID>.Add(entity.EntityID, entity);
        entities.Add(entity);
        return entity;
    }

    private static Renderable AddModel(Entity entity, Vector3 renderedLocation, bool isDrawn, bool wasRendered)
    {
        var renderable = (Renderable)RuntimeHelpers.GetUninitializedObject(typeof(Renderable));
        renderable.IsDrawn = isDrawn;
        renderable.LerpableWasRenderedLastFrame = wasRendered;
        renderable.RenderAsModel = new RenderAsModel(entity, renderable) { Location = renderedLocation };
        entity.Renderable = renderable;
        return renderable;
    }

    private static void Contain(Entity entity, Entity container) => ContainedByField.SetValue(entity, container.EntityID);

    private static RenderAsModel AttachModel(Entity parent, Entity child, bool isHand = false)
    {
        var parentRenderable = AddModel(parent, new Vector3(98, 197, 30), isDrawn: true, wasRendered: true);
        parentRenderable.ModelWasRenderedLastFrame = true;
        var animator = (ModelAnimator)RuntimeHelpers.GetUninitializedObject(typeof(ModelAnimator));
        animator.UncorrectedWorld = Matrix.CreateTranslation(100, 200, -30);
        animator.World = Matrix.CreateTranslation(900, 900, 900);
        parentRenderable.RenderAsModel.AnimatedModel = new StiffAnimatedModel
        {
            ModelAnimator = animator,
            StandardDrawingWorldTransformation = Matrix.CreateTranslation(100, 250, -30)
        };
        var childModel = AddModel(child, Vector3.Zero, isDrawn: false, wasRendered: false).RenderAsModel;
        childModel.AttachedTo = parent;
        childModel.AttachorPoint = new AttachPoint { IsHand = isHand };
        return childModel;
    }

    [TestMethod]
    public void ContainedEntitiesUseVehicleInterpolatedPositionInsteadOfTheirStaleModels()
    {
        var vehicle = NewEntity(1, new Vector3(100, 200, 30));
        var driver = NewEntity(2, null);
        var passenger = NewEntity(3, null);
        var cargo = NewEntity(4, null);
        AddModel(vehicle, new Vector3(98, 197, 30), isDrawn: true, wasRendered: true);
        foreach (var occupant in new[] { driver, passenger, cargo })
        {
            AddModel(occupant, new Vector3(5, 6, 0), isDrawn: false, wasRendered: false);
            Contain(occupant, vehicle);
            Assert.AreEqual(new Vector3(98, 197, 30), occupant.RenderedLocation);
        }
        vehicle.Renderable.RenderAsModel.Location = new Vector3(110, 210, 30);
        Assert.AreEqual(new Vector3(110, 210, 30), driver.RenderedLocation);
        Assert.AreEqual(new Vector3(110, 210, 30), passenger.RenderedLocation);
        Assert.AreEqual(new Vector3(110, 210, 30), cargo.RenderedLocation);
    }

    [DataTestMethod]
    [DataRow(false, true)]
    [DataRow(true, false)]
    public void HiddenOrOffscreenVehicleUsesItsSimulationPosition(bool isDrawn, bool wasRendered)
    {
        var vehicle = NewEntity(1, new Vector3(100, 200, 30));
        var passenger = NewEntity(2, null);
        AddModel(vehicle, new Vector3(5, 6, 0), isDrawn, wasRendered);
        AddModel(passenger, Vector3.Zero, isDrawn: false, wasRendered: false);
        Contain(passenger, vehicle);
        Assert.AreEqual(new Vector3(100, 200, 30), passenger.RenderedLocation);
        LocationField.SetValue(vehicle, new Vector3(120, 220, 30));
        Assert.AreEqual(new Vector3(120, 220, 30), passenger.RenderedLocation);
    }

    [TestMethod]
    public void NestedContainersUseOutermostPosition()
    {
        var vehicle = NewEntity(1, new Vector3(100, 200, 30));
        var bag = NewEntity(2, null);
        var item = NewEntity(3, null);
        AddModel(bag, Vector3.Zero, isDrawn: false, wasRendered: false);
        AddModel(item, Vector3.Zero, isDrawn: false, wasRendered: false);
        Contain(bag, vehicle);
        Contain(item, bag);
        AddModel(vehicle, new Vector3(98, 197, 30), isDrawn: true, wasRendered: true);
        Assert.AreEqual(new Vector3(98, 197, 30), item.RenderedLocation);
        Assert.AreEqual(vehicle.RenderedLocation, bag.RenderedLocation);
        Assert.AreEqual(vehicle.RenderedLocation, item.RenderedLocation);
    }

    [TestMethod]
    public void DrawnOccupantUsesOwnRenderedPosition()
    {
        var vehicle = NewEntity(1, new Vector3(100, 200, 30));
        var passenger = NewEntity(2, null);
        AddModel(vehicle, new Vector3(98, 197, 30), isDrawn: true, wasRendered: true);
        AddModel(passenger, new Vector3(110, 205, 35), isDrawn: true, wasRendered: true);
        Contain(passenger, vehicle);
        Assert.AreEqual(new Vector3(110, 205, 35), passenger.RenderedLocation);
    }

    [TestMethod]
    public void AttachedOccupantUsesItsWorldPositionInsteadOfContainerPosition()
    {
        var vehicle = NewEntity(1, new Vector3(100, 200, 30));
        var passenger = NewEntity(2, null);
        Contain(passenger, vehicle);
        var attachedModel = AttachModel(vehicle, passenger);
        attachedModel.CombinedTransform = Matrix.CreateTranslation(110, 260, -35);
        Assert.AreEqual(new Vector3(110, 210, 35), passenger.RenderedLocation);

        attachedModel.CombinedTransform = Matrix.CreateTranslation(120, 270, -40);
        Assert.AreEqual(new Vector3(120, 220, 40), passenger.RenderedLocation);
    }

    [TestMethod]
    public void AttachmentWithoutCurrentParentRenderFallsBackToContainer()
    {
        var vehicle = NewEntity(1, new Vector3(100, 200, 30));
        var passenger = NewEntity(2, null);
        Contain(passenger, vehicle);
        var attachedModel = AttachModel(vehicle, passenger);
        attachedModel.CombinedTransform = Matrix.CreateTranslation(110, 260, -35);
        vehicle.Renderable.ModelWasRenderedLastFrame = false;
        vehicle.Renderable.LerpableWasRenderedLastFrame = false;
        Assert.AreEqual(new Vector3(100, 200, 30), passenger.RenderedLocation);
    }

    [TestMethod]
    public void AttachmentWaitsForTransformAfterReattachment()
    {
        var vehicle = NewEntity(1, new Vector3(100, 200, 30));
        var passenger = NewEntity(2, null);
        Contain(passenger, vehicle);
        var attachedModel = AttachModel(vehicle, passenger);
        Assert.AreEqual(vehicle.RenderedLocation, passenger.RenderedLocation);
        attachedModel.CombinedTransform = Matrix.CreateTranslation(110, 260, -35);
        Assert.AreEqual(new Vector3(110, 210, 35), passenger.RenderedLocation);
        attachedModel.AttachedTo = null;
        attachedModel.AttachedTo = vehicle;
        Assert.AreEqual(vehicle.RenderedLocation, passenger.RenderedLocation);
    }

    [TestMethod]
    public void HiddenHandAttachmentFallsBackToContainer()
    {
        var vehicle = NewEntity(1, new Vector3(100, 200, 30));
        var passenger = NewEntity(2, null);
        Contain(passenger, vehicle);
        var attachedModel = AttachModel(vehicle, passenger, isHand: true);
        attachedModel.CombinedTransform = Matrix.CreateTranslation(110, 260, -35);
        vehicle.Renderable.HideHandAttachments = true;
        Assert.AreEqual(vehicle.RenderedLocation, passenger.RenderedLocation);
    }

    [TestMethod]
    public void OccupantWithoutRenderableUsesSimulationPosition()
    {
        var vehicle = NewEntity(1, new Vector3(100, 200, 30));
        var passenger = NewEntity(2, null);
        AddModel(vehicle, new Vector3(98, 197, 30), isDrawn: true, wasRendered: true);
        Contain(passenger, vehicle);
        Assert.AreEqual(new Vector3(100, 200, 30), passenger.RenderedLocation);
    }

    [TestMethod]
    public void BoardingAndExitSwitchPositionSources()
    {
        var vehicle = NewEntity(1, new Vector3(100, 200, 30));
        var driver = NewEntity(2, new Vector3(10, 20, 0));
        AddModel(driver, new Vector3(9, 19, 0), isDrawn: true, wasRendered: true);
        AddModel(vehicle, new Vector3(98, 197, 30), isDrawn: true, wasRendered: true);
        Assert.AreEqual(new Vector3(9, 19, 0), driver.RenderedLocation);
        Contain(driver, vehicle);
        driver.Renderable.IsDrawn = false;
        Assert.AreEqual(new Vector3(98, 197, 30), driver.RenderedLocation);
        ContainedByField.SetValue(driver, null);
        LocationField.SetValue(driver, new Vector3(102, 202, 0));
        Assert.AreEqual(new Vector3(102, 202, 0), driver.RenderedLocation);
    }

    [TestMethod]
    public void HiddenIndependentEntityFallsBackToSimulationPosition()
    {
        var entity = NewEntity(1, new Vector3(100, 200, 0));
        AddModel(entity, new Vector3(5, 6, 0), isDrawn: false, wasRendered: true);
        Assert.AreEqual(new Vector3(100, 200, 0), entity.RenderedLocation);
    }

    [TestMethod]
    public void MissingContainerDoesNotDestroyOrAlterTheEntity()
    {
        var entity = NewEntity(1, null);
        AddModel(entity, Vector3.Zero, isDrawn: false, wasRendered: false);
        ContainedByField.SetValue(entity, (EntityID)99);
        Assert.ThrowsExactly<InvalidOperationException>(() => _ = entity.RenderedLocation);
        Assert.AreEqual((EntityID)99, entity.ContainedBy);
        Assert.AreSame(entity, Entity.FindByID(entity.EntityID));
    }

    [TestMethod]
    public void MemoryFactKeepsItsOwnRememberedPosition()
    {
        var memory = (MemoryFact)RuntimeHelpers.GetUninitializedObject(typeof(MemoryFact));
        var renderable = (Renderable)RuntimeHelpers.GetUninitializedObject(typeof(Renderable));
        LocationField.SetValue(memory, new Vector3(5, 6, 0));
        renderable.MemoryFact = memory;
        memory.Renderable = renderable;
        var vehicle = NewEntity(1, new Vector3(100, 200, 30));
        LocationField.SetValue(vehicle, new Vector3(120, 220, 30));
        Assert.AreEqual(new Vector3(5, 6, 0), memory.RenderedLocation);
    }
}
