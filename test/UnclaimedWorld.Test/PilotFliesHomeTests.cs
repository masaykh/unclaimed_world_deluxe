using System.Reflection;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Maps;

namespace UnclaimedWorld.Test;

/// <summary>
/// A pilot whose job ends while he is in the air flies home and lands there, rather than hovering
/// until he is put out on whatever tile is nearest (issue #25). Home is the nearest helipad no other
/// vehicle stands on (Lars: "Return to base should prioritize helipads") -
/// GoalTakeFive.ChooseTerminalTile.
/// </summary>
[TestClass]
public sealed class PilotFliesHomeTests
{
    private static readonly MethodInfo Choose = typeof(Goal).Assembly.GetType("UWGame.SimSide.AI.Goals.GoalTakeFive")!
        .GetMethod("ChooseTerminalTile", BindingFlags.Static | BindingFlags.Public)!;

    private static Point? ChooseTerminalTile(IList<Vector3> terminals, Vector3 from, Func<Point, bool> taken) =>
        (Point?)Choose.Invoke(null, new object[] { terminals, from, taken });

    private static Vector3 AtTile(int x, int y) => MapManager.TileToWorldPos(new Point(x, y));

    private static bool NoneTaken(Point _) => false;

    [TestMethod]
    public void HeLandsOnTheNearestHelipad()
    {
        var pads = new List<Vector3> { AtTile(40, 40), AtTile(12, 10), AtTile(60, 5) };
        Assert.AreEqual(new Point(12, 10), ChooseTerminalTile(pads, AtTile(10, 10), NoneTaken));
    }

    [TestMethod]
    public void AHelipadWithAnotherVehicleOnItIsPassedOver()
    {
        var pads = new List<Vector3> { AtTile(12, 10), AtTile(40, 40) };
        Assert.AreEqual(new Point(40, 40), ChooseTerminalTile(pads, AtTile(10, 10), tile => tile == new Point(12, 10)),
            "the near pad is taken, so the far one");
    }

    [TestMethod]
    public void WithEveryHelipadTakenThereIsNoneToChoose()
    {
        var pads = new List<Vector3> { AtTile(12, 10), AtTile(40, 40) };
        Assert.IsNull(ChooseTerminalTile(pads, AtTile(10, 10), _ => true), "GoalTakeFive.FindHome then lands him by the expedition's centre");
        Assert.IsNull(ChooseTerminalTile(new List<Vector3>(), AtTile(10, 10), NoneTaken), "no helipad built");
    }

    [TestMethod]
    public void BetweenTwoEquallyNearHelipadsTheChoiceIsTheSameEveryTime()
    {
        // A replay must land the same way: the first of equals, in the expedition's order.
        var pads = new List<Vector3> { AtTile(14, 10), AtTile(6, 10) };
        Assert.AreEqual(new Point(14, 10), ChooseTerminalTile(pads, AtTile(10, 10), NoneTaken));
        pads.Reverse();
        Assert.AreEqual(new Point(6, 10), ChooseTerminalTile(pads, AtTile(10, 10), NoneTaken));
    }
}
