using Microsoft.Xna.Framework;
using UWGame.ClientSide.Map;

namespace UnclaimedWorld.Test
{
    /// <summary>
    /// Kastuk, 2026-10-23: "big terrains like hillgreece will appear out of nothing in explorating
    /// area, when colonist come close enough to center of this sprite." A rock or hill in the shroud
    /// is drawn once any tile within twice its collider has been seen
    /// (GameWorldRenderer.AnyTileSeenAround). Tiles are 48 px.
    /// </summary>
    [TestClass]
    public sealed class TerrainPopInTests
    {
        // A collider over tiles 10..11 (480..575 px); twice it reaches tiles 9..12.
        private static readonly Vector2 From = new Vector2(480, 480);
        private static readonly Vector2 To = new Vector2(575, 575);

        private static bool SeenOnly(int seenX, int seenY) =>
            GameWorldRenderer.AnyTileSeenAround(From, To, 100, 100, (x, y) => x == seenX && y == seenY);

        [TestMethod]
        public void ATileUnderTheCollider_ShowsIt()
        {
            Assert.IsTrue(SeenOnly(10, 11));
        }

        [TestMethod]
        public void ATileWithinTwiceTheCollider_ShowsIt_BeforeTheCentreIsSeen()
        {
            Assert.IsTrue(SeenOnly(9, 9), "the corner of the doubled area");
            Assert.IsTrue(SeenOnly(12, 12), "the far corner");
        }

        [TestMethod]
        public void ATileBeyondIt_DoesNot()
        {
            Assert.IsFalse(SeenOnly(13, 10));
            Assert.IsFalse(SeenOnly(8, 10));
            Assert.IsFalse(GameWorldRenderer.AnyTileSeenAround(From, To, 100, 100, (x, y) => false), "nothing seen");
        }

        [TestMethod]
        public void AtTheMapEdge_TheAreaIsClipped()
        {
            int calls = 0;
            bool any = GameWorldRenderer.AnyTileSeenAround(new Vector2(0, 0), new Vector2(95, 95), 2, 2, (x, y) =>
            {
                calls++;
                Assert.IsTrue(x >= 0 && x < 2 && y >= 0 && y < 2, $"tile {x},{y} is off the map");
                return false;
            });
            Assert.IsFalse(any);
            Assert.AreEqual(4, calls, "every tile of the 2 x 2 map, and no other");
        }
    }
}
