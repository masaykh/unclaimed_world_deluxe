using System.Reflection;
using System.Runtime.InteropServices;
using UWGame.SimSide.AI.Pathfinding;

namespace UnclaimedWorld.Test;

/// <summary>
/// HighResolutionTime read Kernel32.dll's performance counter through P/Invoke, and NEW GAME on
/// Linux died with DllNotFoundException 'Kernel32.dll' (GitHub issue #1).
/// </summary>
[TestClass]
public sealed class HighResolutionTimeTests
{
    [TestMethod]
    public void MeasuresElapsedSecondsWithoutAWindowsLibrary()
    {
        var timer = new HighResolutionTime();
        timer.Start();
        Thread.Sleep(30);
        double seconds = timer.GetTime();
        Assert.IsTrue(seconds >= 0.02 && seconds < 5, $"measured {seconds} s for a 30 ms sleep");
    }

    [TestMethod]
    public void TheGameAssemblyImportsNothingFromKernel32()
    {
        var imports = typeof(HighResolutionTime).Assembly.GetTypes()
            .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            .Select(m => (method: m, import: m.GetCustomAttribute<DllImportAttribute>()))
            .Where(x => x.import != null && x.import.Value.StartsWith("kernel32", StringComparison.OrdinalIgnoreCase))
            .Select(x => $"{x.method.DeclaringType!.FullName}.{x.method.Name}")
            .ToList();
        Assert.AreEqual(0, imports.Count, "Windows-only imports: " + string.Join(", ", imports));
    }
}
