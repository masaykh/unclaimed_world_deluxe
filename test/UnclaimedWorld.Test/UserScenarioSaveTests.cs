using System.Xml.Serialization;
using UWGame;
using UWGame.SimSide.AllGameData.Scenarios;
using UWGame.SimSide.Scenarios;

namespace UnclaimedWorld.Test;

/// <summary>
/// SAVE is a save followed by a load of the same file, and the load finds the save's scenario by
/// name through AllScenarioLoader.LoadScenarioHeader - which answered null for a user scenario,
/// so every game of one crashed on SAVE (Kastuk, 2026-11-15). The whole header round trip, with
/// real tables, is case 1 of tools/build/84-verify-user-scenarios.sh; this is the lookup alone.
/// </summary>
[TestClass]
[DoNotParallelize] // Config resolves user/Scenarios against the current directory.
public sealed class UserScenarioSaveTests
{
    private string root = null!;
    private string oldDirectory = null!;

    [TestInitialize]
    public void Setup()
    {
        oldDirectory = Environment.CurrentDirectory;
        root = Path.Combine(Path.GetTempPath(), "uw-userscen-" + Guid.NewGuid().ToString("N"));
        string folder = Path.Combine(root, "user", "Scenarios", "My Island");
        Directory.CreateDirectory(folder);
        using (var writer = new StreamWriter(Path.Combine(folder, ScenarioLoader.scenarioHeaderFileName)))
        {
            new XmlSerializer(typeof(Scenario)).Serialize(writer, new Scenario { Name = "My Island", MapKey = "fx DemoIsland" });
        }
        Environment.CurrentDirectory = root;
    }

    [TestCleanup]
    public void Cleanup()
    {
        Environment.CurrentDirectory = oldDirectory;
        Directory.Delete(root, recursive: true);
    }

    [TestMethod]
    public void ASavedUserScenarioIsFoundAgainByName()
    {
        // The Source NEW GAME records for a user scenario: the header file does not carry one.
        Scenario? found = AllScenarioLoader.LoadScenarioHeader("My Island", Source.NotSet);
        Assert.IsNotNull(found, "the load of a save of this scenario has nothing to read its tables from");
        Assert.AreEqual("My Island", found.Name);
        Assert.AreEqual("fx DemoIsland", found.MapKey);
        Assert.AreEqual(Source.NotSet, found.Source, "the Source IsSameScenario compares a save against");
    }
}
