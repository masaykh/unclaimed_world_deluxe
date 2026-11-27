using Microsoft.Xna.Framework;
using UWGame;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Entities;

namespace UnclaimedWorld.Test;

/// <summary>
/// Kastuk, "Custom Assets", 2026-11-04: the abatis wall in Clay Pit "is using too much same
/// pieces". StructureFlavour: which flavour a structure on a tile draws, and how many a type has.
/// </summary>
[TestClass]
public sealed class StructureFlavourTests
{
    private static ClientStateInfo State(params StateModifier[] conditions)
    {
        var mask = new BitMask64(typeof(StateModifier));
        foreach (StateModifier c in conditions)
        {
            mask.Set(c);
        }
        return new ClientStateInfo { Conditions = mask };
    }

    [TestMethod]
    public void ATypeDrawsAsManyFlavoursAsItsConditionsAskFor()
    {
        Assert.AreEqual(1, StructureFlavour.FlavoursIn(new RenderableType()), "no conditions: one look");
        Assert.AreEqual(1, StructureFlavour.FlavoursIn(new RenderableType { ClientStateConditions = new[] { State(StateModifier.Ordered) } }),
            "the stock abatis before this change, and every other structure");
        Assert.AreEqual(2, StructureFlavour.FlavoursIn(new RenderableType
        {
            ClientStateConditions = new[] { State(StateModifier.Ordered), State(StateModifier.Flavour2), State(StateModifier.Ordered, StateModifier.Flavour2) }
        }), "the abatis now: the default is Flavour1, abatis2 is Flavour2");
    }

    [TestMethod]
    public void AWallAlongARowMixesBothAbatis()
    {
        var row = Enumerable.Range(20, 12).Select(x => StructureFlavour.ForTile(new Point(x, 40), 2)).ToList();
        var column = Enumerable.Range(20, 12).Select(y => StructureFlavour.ForTile(new Point(40, y), 2)).ToList();
        foreach (var wall in new[] { row, column })
        {
            CollectionAssert.IsSubsetOf(wall.Distinct().ToList(), new[] { StateModifier.Flavour1, StateModifier.Flavour2 });
            Assert.IsTrue(wall.Count(f => f == StateModifier.Flavour2) is >= 3 and <= 9, string.Join(",", wall));
        }
    }

    [TestMethod]
    public void TheSameTileAlwaysGivesTheSameFlavour()
    {
        // Nothing is saved: a loaded game and the player's memory of the structure must agree.
        Assert.AreEqual(StructureFlavour.ForTile(new Point(17, 93), 2), StructureFlavour.ForTile(new Point(17, 93), 2));
        Assert.AreEqual(StateModifier.Flavour1, StructureFlavour.ForTile(new Point(17, 93), 1), "a type with one look");
    }

    [TestMethod]
    public void AFlavourAlreadySetIsKept()
    {
        var conditions = new BitMask64(typeof(StateModifier), (int)StateModifier.Ordered);
        Assert.IsFalse(StructureFlavour.HasOne(conditions));
        conditions.Set(StateModifier.Flavour3);
        Assert.IsTrue(StructureFlavour.HasOne(conditions), "a tree or tile resource set its own");
    }
}
