using System;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface;
using UWGame.ClientSide.Interface.MapGUI;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;

namespace UWGame.Mods;

/// <summary>
/// THE STINK OVERLAY: where the colony stinks, coloured on the ground like the THREATS overlay.
///
/// THE REQUEST. Kastuk, "Discomfort", 2026-10-17: "I sure the visible layer of discomfort map will
/// help players to see the sources of stink at least."
///
/// HOW THE THREATS OVERLAY IS DRAWN (original_src Client/Interface/MapGUI/ThreatRender.cs). Its row
/// in the overlay panel above the minimap (HUDOverlayPanel, OverlayTypes.Threats) sets
/// OverlaySettings.OverlayTypeSettings; GameWorldRenderer.SetupInfluenceQuads then has ThreatRender
/// read the UI allegiance's Normal-stance threat map and put one tile quad per tile of every
/// on-screen sector into the influence-map vertex buffer (ÏnfluenceRender.DrawTile), coloured by
/// the stair steps MapClient.GetThreatLimits sets and faded by value / 50.
///
/// THIS. A STINK row in the same panel, beside THREATS (HUDOverlayPanel.AddStinkOverlayRow), and a
/// StinkRender - an ÏnfluenceRender like ThreatRender - writing into the same buffer, from the same
/// place (GameWorldRenderer.SetupInfluenceQuads -> SetupStinkOverlay). The values:
/// - With WASTE ON THE DISCOMFORT MAP on, the waste map itself - what the colonists route by and
///   what bedtime comfort is read from.
/// - Without it, the same drawing done on demand, only for the filth on the screen and within the
///   reach of it (a source further off cannot reach a tile on screen), into a layer of its own that
///   nothing in the simulation reads. Kept rather than drawing the real waste map regardless,
///   because that map lives with the discomfort maps and changes how colonists walk; and rather
///   than offering the overlay only with that switch, because the comfort cost is there with
///   DISCOMFORT alone. Either way the tile values are the same: (reach + 1) per filthy item at its
///   tile, one less per tile out.
/// The colour (StinkColorFor): olive where the stink is under 5 units, brown from 5 - a bed whose
/// door is there loses comfort for it (3% per 5 by default, read off the waste map; counted item by
/// item within the reach without it, which the fading overlay approximates) - fading in with the
/// stink and never quite invisible, so the reach shows.
///
/// Interface only, not saved with the game (ModSettings.xml, like HudMod's rows). Switched off, or
/// with DISCOMFORT off, nothing is drawn, nothing is scanned and the on-demand layer is let go; the
/// row is not in the panel while DISCOMFORT is off.
/// </summary>
public static partial class DiscomfortMod
{
    /// <summary>Called from HUDOverlayPanel.Populate: whether the STINK row belongs in the panel.</summary>
    public static bool StinkOverlayAvailable => Enabled;

    public static string StinkOverlayLabel => UWGame.Locale.Text("STINK");

    public static string StinkOverlayToolTip =>
        UWGame.Locale.Text("Shows where filthy items stink, fading out over the stink's reach. Brown where a bed would lose comfort to it.");

    /// <summary>From the overlay panel's checkbox; saved at once, like HudMod's rows.</summary>
    public static void SetStinkOverlay(bool shown)
    {
        StinkOverlaySetting.Value = shown ? "true" : "false";
        ModSettings.Save(GameStateManagement.UnclaimedWorld.LogError);
    }

    /// <summary>Under 5 units of stink: none of a sleeper's comfort yet.</summary>
    public static readonly Color FaintStinkColor = new Color(128, 128, 0);

    /// <summary>5 units or more: a bed here loses comfort.</summary>
    public static readonly Color StrongStinkColor = new Color(139, 90, 43);

    /// <summary>The faintest a tile with any stink is drawn, so the edge of the reach shows.</summary>
    public const float FaintestStink = 0.2f;

    /// <summary>
    /// A tile's colour for a waste-map <paramref name="value"/> drawn with <paramref name="reach"/>:
    /// transparent at 0; olive under 5 units (value / (reach + 1), as WasteUnitsAt counts them), brown
    /// from 5; premultiplied, like ThreatRender's, by units / 10 between FaintestStink and 1.
    /// </summary>
    public static Color StinkColorFor(int value, int reach)
    {
        if (value <= 0)
        {
            return Color.Transparent;
        }
        int perUnit = Math.Max(1, reach + 1);
        Color baseColor = value / perUnit >= 5 ? StrongStinkColor : FaintStinkColor;
        return baseColor * Math.Clamp(value / (float)perUnit / 10f, FaintestStink, 1f);
    }

    /// <summary>The on-demand layer while WASTE ON THE DISCOMFORT MAP is off, and the map it was drawn for.</summary>
    private static WasteMap overlayWaste;

    private static MapManager overlayWasteFor;

    /// <summary>How many times the overlay has looked for filth itself; the self-test reads it.</summary>
    public static int StinkOverlayScans { get; private set; }

    /// <summary>
    /// The layer the overlay shows over <paramref name="window"/> (tiles), and the reach it was drawn
    /// with: the drawn waste map of the player's <paramref name="personType"/> colonists, else the
    /// filth within the window and the reach of it drawn the same way. Null while not shown.
    /// </summary>
    public static TileLayer StinkOverlayLayer(EntityType personType, Rectangle window, out int reach)
    {
        reach = 0;
        if (!StinkOverlayShown)
        {
            ReleaseStinkOverlay();
            return null;
        }
        if (WasteMapOn && DrawnWasteMap(personType) is WasteMap drawn)
        {
            reach = drawn.Reach;
            return drawn.Map;
        }
        if (The.Map?.TileMap == null)
        {
            return null;
        }
        if (overlayWaste == null || overlayWasteFor != The.Map)
        {
            overlayWaste = new WasteMap { Applies = true };
            overlayWasteFor = The.Map;
        }
        int r = Reach;
        Rectangle around = window;
        around.Inflate(r, r);
        StinkOverlayScans++;
        Draw(overlayWaste, FindWaste(around), r, out _, out _);
        reach = r;
        return overlayWaste.Map;
    }

    /// <summary>Lets the on-demand layer go: switched off, nothing of it is kept.</summary>
    private static void ReleaseStinkOverlay()
    {
        overlayWaste = null;
        overlayWasteFor = null;
    }

    private static StinkRender stinkRender;

    /// <summary>
    /// Called from GameWorldRenderer.SetupInfluenceQuads after the threats: the stink on screen, a
    /// tile quad per tile with any, into the same buffer. Returns at once while not shown.
    /// </summary>
    public static void SetupStinkOverlay(VertexOverlayGroundSpriteQuad[] vertices, ref int index)
    {
        if (!StinkOverlayShown)
        {
            ReleaseStinkOverlay();
            return;
        }
        var renderer = The.Client?.Renderer;
        var allegiance = The.InGameUI?.UIAllegiance;
        if (renderer == null || allegiance == null)
        {
            return;
        }
        var window = new Rectangle(renderer.TileStartX, renderer.TileStartY,
                                   renderer.TileEndX - renderer.TileStartX + 1, renderer.TileEndY - renderer.TileStartY + 1);
        TileLayer layer = StinkOverlayLayer(allegiance.RepresentativeEntityType, window, out int reach);
        if (layer != null)
        {
            (stinkRender ??= new StinkRender()).SetupQuad(layer, reach, window, vertices, ref index);
        }
    }

    /// <summary>ThreatRender's way of drawing, for a waste layer.</summary>
    private sealed class StinkRender : ÏnfluenceRender
    {
        public StinkRender()
        {
            quad = new OverlayGroundSpriteQuad();
            sourceRectangle = The.Client.FlatSpriteSheet.GetSourceRectangle("whiteRectangle");
        }

        public void SetupQuad(TileLayer layer, int reach, Rectangle window, VertexOverlayGroundSpriteQuad[] vertices, ref int index)
        {
            int right = Math.Min(layer.Width, window.Right), bottom = Math.Min(layer.Height, window.Bottom);
            for (int y = Math.Max(0, window.Top); y < bottom; y++)
            {
                for (int x = Math.Max(0, window.Left); x < right; x++)
                {
                    byte value = layer.GetValue(x, y);
                    if (value == 0)
                    {
                        continue;
                    }
                    if (!DrawTile(x, y, index, StinkColorFor(value, reach), vertices))
                    {
                        return; // the buffer is full, as ThreatRender stops
                    }
                    index++;
                }
            }
        }
    }
}
