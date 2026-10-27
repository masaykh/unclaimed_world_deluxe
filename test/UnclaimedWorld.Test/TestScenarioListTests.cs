using UWGame.ClientSide.Interface;
using UWGame.SimSide;

namespace UnclaimedWorld.Test
{
    /// <summary>
    /// tripleacoder, #general, 2026-10-24: "It would be better if this button showed a list of test
    /// scenarios." What TestScenarioPanel lists, and that a pick starts that scenario on its map.
    /// </summary>
    [TestClass]
    public sealed class TestScenarioListTests
    {
        [TestMethod]
        public void TheList_HasEveryRegisteredScenario_ByName_WithItsMap()
        {
            var listed = TestScenarioPanel.Scenarios();
            var registered = PlaceGameEntities.AllScenariosAndTheirMaps().ToList();
            Assert.IsGreaterThan(90, listed.Count, "the studio's ninety-odd scenarios");
            CollectionAssert.AreEquivalent(registered, listed, "every registered scenario, and nothing else");
            CollectionAssert.AreEqual(listed.Select(s => s.Key.ToString()).OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList(),
                listed.Select(s => s.Key.ToString()).ToList(), "by name");
            Assert.IsTrue(listed.All(s => !string.IsNullOrWhiteSpace(s.Value)), "each with its map");
            Assert.IsTrue(listed.Any(s => s.Key == PlaceGameEntities.DefaultScenario), "the one it opens on is in it");
        }

        [TestMethod]
        public void APick_StartsThatScenario_OnItsMap()
        {
            foreach (var scenario in TestScenarioPanel.Scenarios())
            {
                var start = PlaceGameEntities.GetNewScenarioParams(scenario.Key);
                Assert.AreEqual(scenario.Key, start.ScenarioKey);
                Assert.AreEqual(scenario.Value, start.MapKey);
            }
        }
    }
}
