using System.Reflection;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI.Goals;

namespace UnclaimedWorld.Test;

/// <summary>
/// A pilot who gets out of an airborne skimmer lands it first, on ground he can stand on
/// (GoalExitVehicle.FindLandingTile). Before, he stepped out at the exit point in mid-air or onto
/// the river (Kastuk, 2026-11-14).
/// </summary>
[TestClass]
public sealed class AircraftLandingTileTests
{
    private static readonly MethodInfo Find = typeof(Goal).Assembly.GetType("UWGame.SimSide.AI.Goals.GoalExitVehicle")!.GetMethod("FindLandingTile", BindingFlags.Static | BindingFlags.Public)!;

    private static Point? FindLandingTile(Point from, Func<Point, bool> canStandOn, int radius) =>
        (Point?)Find.Invoke(null, new object[] { from, canStandOn, radius });

    // A river: columns 4 to 8 are water, everything else is ground.
    private static bool NotRiver(Point p) => p.X < 4 || p.X > 8;

    [TestMethod]
    public void OverGroundItLandsWhereItIs()
    {
        Assert.AreEqual(new Point(2, 5), FindLandingTile(new Point(2, 5), NotRiver, 12));
    }

    [TestMethod]
    public void OverTheRiverItLandsOnTheNearerBank()
    {
        Assert.AreEqual(new Point(3, 5), FindLandingTile(new Point(5, 5), NotRiver, 12), "two tiles to the west bank, four to the east");
        Assert.AreEqual(new Point(9, 5), FindLandingTile(new Point(7, 5), NotRiver, 12));
    }

    [TestMethod]
    public void InTheMiddleOfTheRiverTheChoiceIsTheSameEveryTime()
    {
        // Both banks are three tiles away; scan order decides, so a replay lands the same way.
        Point? first = FindLandingTile(new Point(6, 5), NotRiver, 12);
        Assert.AreEqual(new Point(3, 5), first);
        Assert.AreEqual(first, FindLandingTile(new Point(6, 5), NotRiver, 12));
    }

    [TestMethod]
    public void TheStraightLineNeighbourBeatsTheDiagonalInTheSameRing()
    {
        Point? landing = FindLandingTile(new Point(10, 10), p => p == new Point(12, 12) || p == new Point(12, 10), 12);
        Assert.AreEqual(new Point(12, 10), landing);
    }

    [TestMethod]
    public void NoGroundInReachGivesNothing()
    {
        Assert.IsNull(FindLandingTile(new Point(6, 5), _ => false, 3));
    }
}
