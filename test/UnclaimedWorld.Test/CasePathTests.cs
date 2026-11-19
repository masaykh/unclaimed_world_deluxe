using System.Diagnostics;
using UWGame.Port;

namespace UnclaimedWorld.Test;

/// <summary>
/// The Linux build could not find Fonts/CRT_18pt (crt_18pt.xnb on disk) or billboard
/// (Billboard.xnb): the studio named its assets on Windows, and Linux file names are
/// case-sensitive (Kastuk, testing on Ubuntu, 2026-11-18). CasePath finds the file whatever the
/// case. Nothing here can run on Linux, so the lookup is tested against a directory lister that
/// behaves like a case-sensitive file system, against the real disk, and - where Windows allows
/// it - against a directory made case-sensitive with fsutil.
/// </summary>
[TestClass]
public sealed class CasePathTests
{
    /// <summary>A case-sensitive file system: directory -> the names in it, spelled exactly.</summary>
    private sealed class FakeDisk
    {
        public readonly Dictionary<string, List<string>> Folders = new(StringComparer.Ordinal);
        public int Listings;

        public FakeDisk Add(string path)
        {
            string parent = ".";
            foreach (string name in path.Split('/'))
            {
                if (!Folders.TryGetValue(parent, out List<string>? names))
                {
                    Folders[parent] = names = new List<string>();
                }
                if (!names.Contains(name))
                {
                    names.Add(name);
                }
                parent = parent == "." ? name : parent + "/" + name;
            }
            return this;
        }

        public string[]? List(string directory)
        {
            Listings++;
            return Folders.TryGetValue(directory, out List<string>? names) ? names.ToArray() : null;
        }
    }

    private static FakeDisk Shipped() => new FakeDisk()
        .Add("Content/Fonts/crt_18pt.xnb")
        .Add("Content/Fonts/CRTGlow.xnb")
        .Add("Content/Billboard.xnb")
        .Add("Content/Models/skimmer.xnb")
        .Add("data/BaseData/Strings/English (US).xml");

    [TestMethod]
    public void TheStudiosSpellingFindsTheShippedFile()
    {
        var disk = Shipped();
        var paths = new CasePath(disk.List);

        Assert.AreEqual("Content/Fonts/crt_18pt.xnb", paths.Find("Content/Fonts/CRT_18pt.xnb"));
        Assert.AreEqual("Content/Billboard.xnb", paths.Find("Content/billboard.xnb"));
        Assert.AreEqual("data/BaseData/Strings/English (US).xml", paths.Find("DATA/basedata/strings/english (us).XML"));
    }

    [TestMethod]
    public void BackslashesSeparateSegments()
    {
        var paths = new CasePath(Shipped().List);

        // "Models\\skimmer" is how the studio wrote it; on Linux '\' is just a character.
        Assert.AreEqual("Content/Models/skimmer.xnb", paths.Find("Content\\Models\\skimmer.xnb"));
        Assert.AreEqual("Content/Fonts/crt_18pt.xnb", paths.Find("Content/Fonts\\CRT_18pt.xnb"));
    }

    [TestMethod]
    public void AnExactPathComesBackUnchanged()
    {
        var paths = new CasePath(Shipped().List);
        string exact = "Content/Fonts/CRTGlow.xnb";

        Assert.AreSame(exact, paths.Find(exact));
    }

    [TestMethod]
    public void TheExactSpellingWinsOverAnotherCase()
    {
        // Possible on Linux: two files that differ only in case. The one asked for is the one opened.
        var disk = new FakeDisk().Add("Content/water.xnb").Add("Content/Water.xnb");
        var paths = new CasePath(disk.List);

        Assert.AreEqual("Content/Water.xnb", paths.Find("Content/Water.xnb"));
        Assert.AreEqual("Content/water.xnb", paths.Find("Content/water.xnb"));
        // Neither spelled as asked: the ordinal-first, whatever order the directory lists them in.
        Assert.AreEqual("Content/Water.xnb", paths.Find("Content/WATER.xnb"));
    }

    [TestMethod]
    public void AFileNotWrittenYetKeepsItsNameUnderTheFoldersThatExist()
    {
        var disk = new FakeDisk().Add("user/Scenarios/Clay Pit/scenario.xml");
        var paths = new CasePath(disk.List);

        Assert.AreEqual("user/Scenarios/Clay Pit/NewTable.xml", paths.Find("USER/scenarios/CLAY PIT/NewTable.xml"));
        Assert.AreEqual("user/Scenarios/My New One/scenario.xml", paths.Find("user/scenarios/My New One/scenario.xml"));
        Assert.AreEqual("nowhere/at/all.xml", paths.Find("nowhere/at/all.xml"));
    }

    [TestMethod]
    public void AFileCreatedAfterTheListingIsStillFound()
    {
        var disk = new FakeDisk().Add("user/SaveGames/first.sav");
        var paths = new CasePath(disk.List);
        Assert.AreEqual("user/SaveGames/first.sav", paths.Find("user/savegames/first.sav"));

        disk.Add("user/SaveGames/Second.sav");

        Assert.AreEqual("user/SaveGames/Second.sav", paths.Find("user/savegames/second.sav"));
    }

    [TestMethod]
    public void ADirectoryIsListedOnceWhileItsNamesMatch()
    {
        var disk = Shipped();
        var paths = new CasePath(disk.List);
        paths.Find("content/fonts/CRT_18pt.xnb");
        int first = disk.Listings;

        paths.Find("content/fonts/crtglow.xnb");
        paths.Find("content/fonts/CRT_18PT.xnb");

        Assert.AreEqual(first, disk.Listings, "a cached listing that holds the name is not re-read");
    }

    [TestMethod]
    public void RootsAreNeverLookedUp()
    {
        var disk = new FakeDisk();
        disk.Folders["/"] = new List<string> { "home" };
        disk.Folders["/home"] = new List<string> { "Game" };
        disk.Folders["C:/"] = new List<string> { "Games" };
        var paths = new CasePath(disk.List);

        Assert.AreEqual("/home/Game", paths.Find("/home/game"));
        Assert.AreEqual("C:/Games", paths.Find("C:\\games"));
    }

    [TestMethod]
    public void OnWindowsResolveChangesNothing()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Windows only");
        }
        // Even a path that does not exist: Windows opens the studio's spelling as it always did.
        string missing = Path.Combine(Path.GetTempPath(), "no-such-folder-" + Guid.NewGuid().ToString("N"), "File.xnb");
        Assert.AreSame(missing, CasePath.Resolve(missing));
    }

    [TestMethod]
    public void TheDiskListerReportsNamesAsTheyAreOnDisk()
    {
        string root = Path.Combine(Path.GetTempPath(), "uw-casepath-" + Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(Path.Combine(root, "Content", "Fonts"));
            File.WriteAllText(Path.Combine(root, "Content", "Fonts", "crt_18pt.xnb"), "");

            string found = CasePath.Disk.Find(Path.Combine(root, "content", "FONTS", "CRT_18pt.xnb"));

            Assert.AreEqual(Path.Combine(root, "Content", "Fonts", "crt_18pt.xnb").Replace('\\', '/'), found.Replace('\\', '/'));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>
    /// The real thing: a directory Windows treats case-sensitively, as Linux does. Needs the
    /// right to set the attribute (fsutil file setCaseSensitiveInfo), which an ordinary account
    /// usually lacks - inconclusive then, not failed.
    /// </summary>
    [TestMethod]
    public void ACaseSensitiveDirectoryOnWindows()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Windows only");
        }
        string root = Path.Combine(Path.GetTempPath(), "uw-casepath-cs-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            if (!MakeCaseSensitive(root))
            {
                Assert.Inconclusive("fsutil could not make " + root + " case-sensitive (needs the right to set the attribute)");
            }
            string fonts = Path.Combine(root, "Fonts");
            Directory.CreateDirectory(fonts);
            if (!MakeCaseSensitive(fonts))
            {
                Assert.Inconclusive("fsutil could not make " + fonts + " case-sensitive");
            }
            File.WriteAllText(Path.Combine(fonts, "crt_18pt.xnb"), "");
            string asked = Path.Combine(root, "fonts", "CRT_18pt.xnb");
            Assert.IsFalse(File.Exists(asked), "the directory is case-sensitive, so the studio's spelling misses");

            string found = CasePath.Disk.Find(asked);

            Assert.IsTrue(File.Exists(found), found);
            Assert.AreEqual(Path.Combine(fonts, "crt_18pt.xnb").Replace('\\', '/'), found.Replace('\\', '/'));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static bool MakeCaseSensitive(string directory)
    {
        try
        {
            using Process? fsutil = Process.Start(new ProcessStartInfo("fsutil.exe", "file setCaseSensitiveInfo \"" + directory + "\" enable")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            });
            if (fsutil == null)
            {
                return false;
            }
            // fsutil exits 0 even when it is refused, so the result is probed, not trusted.
            fsutil.StandardOutput.ReadToEnd();
            fsutil.WaitForExit();
            return Probe(directory);
        }
        catch (System.ComponentModel.Win32Exception)
        {
            return false;
        }
    }

    /// <summary>Whether a file written in lower case is missed when asked for in upper case.</summary>
    private static bool Probe(string directory)
    {
        string lower = Path.Combine(directory, "probe");
        File.WriteAllText(lower, "");
        bool sensitive = !File.Exists(Path.Combine(directory, "PROBE"));
        File.Delete(lower);
        return sensitive;
    }
}
