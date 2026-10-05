using UWGame.SimSide;

namespace UnclaimedWorld.Test
{
    [TestClass]
    [DoNotParallelize]
    public sealed class SimulationRegressionTests
    {
        [TestMethod]
        public void EmptyMap_AdvancesRequestedSimulationTime()
        {
            // TODO: the game cannot run headlessly yet. Make a ClientStub and a RenderableStub...
            /*
            using var game = SimulationTestHost.Create(
                PlaceGameEntities.DebugScenarios.EmptyMap,
                randomSeed: 12345);

            game.RunForSimulationSeconds(10);

            Assert.AreEqual(10.0, game.Sim.TotalUnPausedGameTimeInSeconds, 0.001);
            */
        }
    }
}
