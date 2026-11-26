using UWGame.Mods;

namespace UnclaimedWorld.Test;

/// <summary>
/// Kastuk, "Mod settings", 2026-11-14: "May need saveable presets of activated mods. Especially to
/// disable all mods beside debug ones to test vanilla changes." ModPresets: what a preset holds,
/// what applying one switches, and that the player's presets save beside ModSettings.xml.
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class ModPresetsTests
{
    private static ModCatalog.Entry Mod(string id, bool canSwitch = true, bool alwaysOn = false) =>
        new ModCatalog.Entry { Id = id, Name = id.ToUpperInvariant(), CanSwitch = canSwitch, AlwaysOn = alwaysOn };

    // Ids nothing has switched off, so each reads as ticked (ModCatalog.Entry.ShownOn).
    private static List<ModCatalog.Entry> Mods() => new List<ModCatalog.Entry>
    {
        Mod("port", canSwitch: false, alwaysOn: true),
        Mod("debug"),
        Mod("presettest.hud"),
        Mod("presettest.weather"),
        Mod("dll:PresetTestLocked", canSwitch: false),
    };

    [TestMethod]
    public void StockAndDebugSwitchesOffEveryModButTheDebuggingTools()
    {
        ModPresets.Preset stock = ModPresets.BuiltIns().Single(p => p.Name == ModPresets.StockAndDebugName);
        var switched = new List<(string, bool)>();
        int count = ModPresets.Apply(stock, Mods(), (e, on) => switched.Add((e.Id, on)));

        CollectionAssert.AreEqual(new List<(string, bool)> { ("presettest.hud", false), ("presettest.weather", false) }, switched,
            "debug is already on and stays; the port is not a mod; a mod that cannot be switched now is left alone");
        Assert.AreEqual(2, count);
        CollectionAssert.AreEquivalent(new[] { "debug", "debugmonitor", "statedump" }, stock.On.ToArray());
    }

    [TestMethod]
    public void APresetSwitchesOnWhatItListsAndOffEverythingElse()
    {
        var preset = new ModPresets.Preset { Name = "HUD ONLY", On = { "presettest.hud" } };
        var switched = new List<(string, bool)>();
        ModPresets.Apply(preset, Mods(), (e, on) => switched.Add((e.Id, on)));
        CollectionAssert.AreEqual(new List<(string, bool)> { ("debug", false), ("presettest.weather", false) }, switched,
            "a mod not in the preset - one installed after it was saved, too - goes off");
    }

    [TestMethod]
    public void CaptureTakesTheTickedModsOnly()
    {
        ModPresets.Preset p = ModPresets.Capture("  my test ", Mods());
        Assert.AreEqual("MY TEST", p.Name, "trimmed, upper case as the LCD panels are");
        CollectionAssert.AreEquivalent(new[] { "debug", "presettest.hud", "presettest.weather" }, p.On.ToArray(),
            "not the port, not a mod that cannot be switched");
    }

    [TestMethod]
    public void ABuiltInNameOrNoNameCannotBeSaved()
    {
        Assert.IsFalse(ModPresets.CanSaveAs(null));
        Assert.IsFalse(ModPresets.CanSaveAs("   "));
        Assert.IsFalse(ModPresets.CanSaveAs("stock + debug"), "a built-in is not replaced, whatever the case");
        Assert.IsTrue(ModPresets.CanSaveAs("Testing"));
    }

    [TestMethod]
    public void ThePlayersPresetsSaveReplaceAndDelete()
    {
        string old = Environment.CurrentDirectory;
        string dir = Path.Combine(Path.GetTempPath(), "uw-presets-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            Environment.CurrentDirectory = dir;
            Assert.AreEqual(1, ModPresets.All().Count, "no file: the built-in only");

            Assert.IsTrue(ModPresets.Save("testing", Mods()));
            Assert.IsTrue(File.Exists(ModPresets.FilePath), "written beside ModSettings.xml");
            ModPresets.Preset saved = ModPresets.All().Single(p => p.Name == "TESTING");
            CollectionAssert.AreEquivalent(new[] { "debug", "presettest.hud", "presettest.weather" }, saved.On.ToArray());

            var fewer = Mods().Where(m => m.Id != "presettest.weather").ToList();
            Assert.IsTrue(ModPresets.Save("Testing", fewer), "the same name, another case");
            Assert.AreEqual(2, ModPresets.All().Count, "replaced, not added");
            Assert.IsFalse(ModPresets.All().Single(p => p.Name == "TESTING").On.Contains("presettest.weather"));

            Assert.IsFalse(ModPresets.Save(ModPresets.StockAndDebugName, Mods()), "the built-in is refused");
            Assert.IsFalse(ModPresets.Delete(ModPresets.StockAndDebugName), "and cannot be deleted");
            Assert.IsTrue(ModPresets.Delete("testing"));
            Assert.AreEqual(1, ModPresets.All().Count);

            File.WriteAllText(ModPresets.FilePath, "<ModPresets><Preset name=");
            Assert.AreEqual(1, ModPresets.All().Count, "a broken file is ignored, not thrown");
        }
        finally
        {
            Environment.CurrentDirectory = old;
            Directory.Delete(dir, recursive: true);
        }
    }
}
