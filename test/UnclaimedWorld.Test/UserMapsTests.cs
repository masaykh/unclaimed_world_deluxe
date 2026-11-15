using UWGame;
using UWGame.ClientSide.Interface;
using UWGame.SimSide.Maps;

namespace UnclaimedWorld.Test;

/// <summary>
/// The Map Editor saved into data/Maps, which every update and release build replaces; it saves into
/// user/Maps now (Kastuk, 2026-11-14). A user map of the same name takes the game map's place.
/// </summary>
[TestClass]
public sealed class UserMapsTests
{
    private string root = null!;

    [TestInitialize]
    public void Setup()
    {
        root = Path.Combine(Path.GetTempPath(), "uw-usermaps-" + Guid.NewGuid().ToString("N"));
        Map("data", "Clay Pit");
        Map("data", "Twinkler");
        Map("user", "Twinkler");
        Map("user", "My Island");
        Directory.CreateDirectory(Path.Combine(root, "data", "Not A Map"));
    }

    [TestCleanup]
    public void Cleanup() => Directory.Delete(root, recursive: true);

    private void Map(string folder, string name)
    {
        string dir = Path.Combine(root, folder, name);
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, Config.MapDataName), "<MapData />");
    }

    private string Game => Path.Combine(root, "data");
    private string User => Path.Combine(root, "user");

    [TestMethod]
    public void AUserMapTakesTheGameMapsPlaceAndNewOnesFollow()
    {
        var merged = MapEditorSaveLoadPanel.MergeMapFolders(
            MapEditorSaveLoadPanel.GetListOfMapFolders(Game), MapEditorSaveLoadPanel.GetListOfMapFolders(User));
        var names = merged.Select(d => d.Name).OrderBy(n => n).ToList();
        CollectionAssert.AreEqual(new[] { "Clay Pit", "My Island", "Twinkler" }, names, "one entry a name; a folder without MapData.xml is not a map");
        Assert.AreEqual(User, merged.Single(d => d.Name == "Twinkler").Parent!.FullName.TrimEnd(Path.DirectorySeparatorChar));
        Assert.AreEqual(Game, merged.Single(d => d.Name == "Clay Pit").Parent!.FullName.TrimEnd(Path.DirectorySeparatorChar));
    }

    [TestMethod]
    public void LookupByNamePrefersTheUserCopy()
    {
        Assert.AreEqual(User, MapManager.ChooseMapsFolder("Twinkler", Game, User));
        Assert.AreEqual(Game, MapManager.ChooseMapsFolder("Clay Pit", Game, User));
    }

    [TestMethod]
    public void NoUserMapsFolderYetIsNotAnError()
    {
        Assert.AreEqual(0, MapEditorSaveLoadPanel.GetListOfMapFolders(Path.Combine(root, "nowhere")).Count);
    }
}
