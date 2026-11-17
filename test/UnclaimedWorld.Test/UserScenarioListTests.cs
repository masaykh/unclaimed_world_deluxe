using UWGame.ClientSide.MainMenu.Scenario;
using UWGame.SimSide.Scenarios;

namespace UnclaimedWorld.Test
{
    /// <summary>
    /// Kastuk, 2026-11-15: "show custom user scenarios in separated list at Start game menu to not be
    /// mixed with vanilla scenarios. Like special button at bottom near Main to switch onto user
    /// scenarios list." What each side of SelectScenarioPanel's switch lists.
    /// </summary>
    [TestClass]
    public sealed class UserScenarioListTests
    {
        private static Scenario Game(string name, int sortOrder)
        {
            var s = new Scenario { Name = name, SortOrder = sortOrder };
            s.SetRGSource();
            return s;
        }

        // What AllScenarioLoader.LoadUserScenarioHeaders makes: a deserialized header, Source never set.
        private static Scenario User(string name, int sortOrder) => new Scenario { Name = name, SortOrder = sortOrder };

        private static List<Scenario> Mixed() => new List<Scenario>
        {
            Game("Tutorial", 0), User("My valley", 0), Game("Free Play", 1), User("Aardvark", 5), Game("Siege", 2),
        };

        [TestMethod]
        public void TheGameList_IsTheGamesScenarios_InTheOrderGiven()
        {
            var shown = SelectScenarioPanel.ScenariosToShow(Mixed(), userScenarios: false);
            CollectionAssert.AreEqual(new[] { "Tutorial", "Free Play", "Siege" }, shown.Select(s => s.Name).ToList());
        }

        [TestMethod]
        public void TheUserList_IsTheUsersScenarios_InTheOrderGiven()
        {
            var shown = SelectScenarioPanel.ScenariosToShow(Mixed(), userScenarios: true);
            CollectionAssert.AreEqual(new[] { "My valley", "Aardvark" }, shown.Select(s => s.Name).ToList());
        }

        [TestMethod]
        public void TheTwoLists_SplitEveryScenario_ExactlyOnce()
        {
            var all = Mixed();
            var game = SelectScenarioPanel.ScenariosToShow(all, userScenarios: false);
            var user = SelectScenarioPanel.ScenariosToShow(all, userScenarios: true);
            Assert.AreEqual(all.Count, game.Count + user.Count);
            CollectionAssert.AreEquivalent(all, game.Concat(user).ToList());
        }

        [TestMethod]
        public void WithNoUserScenarios_TheUserListIsEmpty_AndTheGameListIsAll()
        {
            var all = new List<Scenario> { Game("Tutorial", 0), Game("Free Play", 1) };
            Assert.IsFalse(all.Any(SelectScenarioPanel.IsUserScenario), "the switch is hidden");
            Assert.IsEmpty(SelectScenarioPanel.ScenariosToShow(all, userScenarios: true));
            CollectionAssert.AreEqual(all, SelectScenarioPanel.ScenariosToShow(all, userScenarios: false));
        }

        [TestMethod]
        public void TheShippedScenarios_AreAllTheGames()
        {
            // RGScenarioLoader's headers are what the game list had before user scenarios joined it.
            var shipped = UWGame.SimSide.AllGameData.Scenarios.RGScenarioLoader.LoadAllScenarioHeaders();
            Assert.IsNotEmpty(shipped);
            Assert.IsFalse(shipped.Any(SelectScenarioPanel.IsUserScenario));
        }
    }
}
