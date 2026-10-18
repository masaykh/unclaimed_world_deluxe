using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;

namespace UWGame.Mods;

/// <summary>
/// The discomfort mod, compiled OUT. See PeatMod.Absent.cs for the pattern.
///
/// Comfort is the studio's sum of comfort effects; nothing near a sleeping place changes it. The
/// discomfort maps are the sum of their threat maps, as the studio left them.
/// </summary>
public static class DiscomfortMod
{
    /// <summary>Always false: the mod is not present in this build.</summary>
    public const bool Enabled = false;

    public const string ModId = "discomfort";

    public static void RegisterSettings()
    {
    }

    public static void OnSleepStart(Entity sleeper, Entity sleepingPlace)
    {
    }

    public static float Penalty(Entity colonist) => 0f;

    public static void AddWasteSectors(DiscomfortMap map, HashSet<Point> changed, HashSet<Point> childRemoved)
    {
    }

    public static void AddWaste(DiscomfortMap map, TileSector own)
    {
    }

    public static bool WasteKeepsSector(DiscomfortMap map, int sectorX, int sectorY) => false;

    /// <summary>No STINK row in the overlay panel.</summary>
    public static bool StinkOverlayAvailable => false;

    public static bool StinkOverlayShown => false;

    public static string StinkOverlayLabel => null;

    public static string StinkOverlayToolTip => null;

    public static readonly Color StrongStinkColor = Color.Transparent;

    public static void SetStinkOverlay(bool shown)
    {
    }

    /// <summary>Nothing drawn beside the studio's overlays.</summary>
    public static void SetupStinkOverlay(UWGame.ClientSide.Interface.VertexOverlayGroundSpriteQuad[] vertices, ref int index)
    {
    }
}
