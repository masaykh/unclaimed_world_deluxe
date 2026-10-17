using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface;
using UWGame.ClientSide.Interface.HUD_Windows;
using UWGame.SimSide.Maps;

namespace UWGame.Mods;

/// <summary>
/// A placed zone lights up in yellow while the cursor is on its label, or while its window is open.
///
/// THE REQUEST. Kastuk, "HUD mod", 2026-10-12: "Zones: when cursor is over symbol\button of already
/// placed zone, or when it pressed and open the window of already placed zone, highlight that zone
/// area with yellow color, just like at fresh drawn zone."
///
/// HOW. A fresh selection is the studio's InGameInterface.SelectedTiles, whose MapAreaRender is
/// yellow; a placed zone's is turquoise (MapAreaRender's constructor), and GameWorldRenderer draws
/// the zones only while the markers are shown. So the zone to light up has its MapAreaRender.Color
/// set to yellow, and back again when it is no longer the one; and it is drawn by itself when the
/// zone grid is not being drawn. Which zone: the one whose label (MarkerWindow) the cursor is on,
/// else InGameInterface.SelectedZone while its windows are up - the zone side panel
/// (SidePanelMapArea) or the actions menu (TileSelectionContextMenu) that the label's arrow opens.
/// SelectedZone alone is not enough: the studio keeps it after those windows close.
///
/// Interface only: nothing the simulation reads changes.
/// </summary>
public static partial class HudMod
{
    /// <summary>The label the cursor is on, while it is a zone's.</summary>
    private static MarkerWindow hoveredMarker;

    /// <summary>The zone drawn yellow now, and the colour it had before.</summary>
    private static Zone litZone;
    private static Color litZoneColor;

    /// <summary>Called by MarkerWindow's MouseOver and MouseOut (and Reset, which empties it).</summary>
    public static void HoverMarker(MarkerWindow marker, bool over)
    {
        if (over)
        {
            hoveredMarker = marker;
        }
        else if (hoveredMarker == marker)
        {
            hoveredMarker = null;
        }
    }

    /// <summary>The zone to light up now, or null.</summary>
    public static Zone ZoneToLight(InGameInterface ui)
    {
        if (!HudMod.ZoneHighlightSetting.On || ui == null)
        {
            return null;
        }
        if (hoveredMarker?.Zone != null && hoveredMarker.DisplayWindow.IsVisibleAndActive)
        {
            return hoveredMarker.Zone;
        }
        Zone selected = ui.SelectedZone;
        if (selected != null
            && (ui.RosterIsDisplayed(ui.SidePanelMapArea)
                || (ui.ContextMenu != null && ui.ContextMenu.DisplayWindow.IsVisibleAndActive)))
        {
            return selected;
        }
        return null;
    }

    /// <summary>
    /// Called by GameWorldRenderer.SetupInterfaceOnMapQuads before it draws the zone grid: colours
    /// the zone to light up, and draws it here when the grid will not.
    /// </summary>
    public static void SetupZoneHighlight(InGameInterface ui, VertexOverlayGroundSpriteQuad[] vertices, ref int index)
    {
        Zone want = ZoneToLight(ui);
        if (want != litZone)
        {
            if (litZone?.MapArea?.MapAreaRender != null)
            {
                litZone.MapArea.MapAreaRender.Color = litZoneColor;
            }
            litZone = want?.MapArea?.MapAreaRender != null ? want : null;
            if (litZone != null)
            {
                litZoneColor = litZone.MapArea.MapAreaRender.Color;
                litZone.MapArea.MapAreaRender.Color = Color.Yellow;
            }
        }
        if (litZone == null)
        {
            return;
        }
        bool gridDrawsIt = ui.ShowOverlaysAndMarkerWindows && ui.UIExpedition.HasValue && ShowsZone(litZone == ui.SelectedZone);
        if (!gridDrawsIt)
        {
            litZone.MapArea.MapAreaRender.SetupQuad(vertices, ref index);
        }
    }
}
