using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface.HUD_Windows;

namespace UWGame.Mods;

/// <summary>
/// Labels on the map that cover each other open up into a list.
///
/// THE REQUEST. tripleacoder, "HUD mod", 2026-10-04: "For the overlapping HUD labels, some of
/// which are clickable like the stockpile, I suggest the following: when the user moves his mouse
/// over a label that touches other labels, after 1 second they should be arranged in a list. If the
/// list touches other labels, those labels should also be included in the list. ... the user can
/// click on a label, or move the mouse away, in which case the stacked list disappears and the
/// labels appear as normal." Kastuk, 2026-10-09: "Want to test overlapping labels shifting into list."
///
/// HOW. The labels are the game's own marker windows (MarkerWindow: zones, camps, colonists and
/// creatures), positioned every frame by InGameInterface.UpdateMarkerWindows. After that, this
/// looks at where the pointer is. Held for <see cref="HoverSeconds"/> over a label that touches
/// another, the label and everything touching it - and everything touching those - are moved into
/// a column under the pointer, the labels themselves rather than copies, so a click on one does
/// exactly what a click on it always did. Every frame the column is laid out again, since the game
/// puts the labels back where they belong in between.
///
/// Instead of a scroll bar, a column that would run off the bottom of the map continues in a
/// second one beside it: nothing to scroll, and the mouse wheel stays the map's.
///
/// The list closes when the pointer leaves it, when a click on it is finished, or when the map
/// scrolls. Labels that only move when the map scrolls (zones, camps) are put back where they were;
/// the rest are put back by the game on the next frame. Interface only.
/// </summary>
public static partial class HudMod
{
    /// <summary>How long the pointer rests on a crowded label before the list opens.</summary>
    public const double HoverSeconds = 1.0;

    /// <summary>The space between two labels in the list, in pixels.</summary>
    public const int ListGap = 2;

    /// <summary>How far outside the list the pointer may stray before it closes, in pixels.</summary>
    public const int ListSlack = 8;

    private static MarkerWindow hoverCandidate;
    private static double hoverSince;
    private static readonly List<MarkerWindow> listed = new List<MarkerWindow>();
    private static readonly Dictionary<MarkerWindow, Point> listedFrom = new Dictionary<MarkerWindow, Point>();
    private static Point listAnchor;
    private static Vector2 listMapPosition;
    private static bool clickStarted;
    private static bool waitForLeave;

    /// <summary>
    /// Called by InGameInterface.UpdateMarkerWindows once the labels are where the game wants them,
    /// with the labels showing now.
    /// </summary>
    public static void ArrangeOverlappingLabels(ICollection<MarkerWindow> markers, GameTime time)
    {
        if (!LabelListSetting.On || time == null || The.Sim?.Controller?.InputData == null)
        {
            CloseList(restore: true);
            return;
        }
        var input = The.Sim.Controller.InputData;
        var mouse = new Point(input.mouseX, input.mouseY);
        double now = time.TotalGameTime.TotalSeconds;
        List<MarkerWindow> shown = markers.Where(m => m?.DisplayWindow != null && m.DisplayWindow.IsVisibleAndActive).ToList();

        if (listed.Count > 0)
        {
            bool scrolled = The.MapUI.MapWindowWorldPosition != listMapPosition;
            bool gone = listed.Any(m => !shown.Contains(m));
            bool clicked = clickStarted && !input.LeftButtonDown;
            clickStarted |= input.LeftButtonDown;
            if (scrolled || gone || clicked)
            {
                CloseList(restore: !scrolled);
                hoverCandidate = null;
                // After a click the pointer is still on a label - and maybe on a menu the click
                // opened. The list waits until it has been somewhere else first.
                waitForLeave = clicked;
                return;
            }
            List<Rectangle> laidOut = LayOutList(listed.Select(Bounds).ToList(), listAnchor, ListBottom());
            for (int i = 0; i < listed.Count; i++)
            {
                listed[i].DisplayWindow.Position = laidOut[i].Location;
            }
            Rectangle area = Union(laidOut);
            area.Inflate(ListSlack, ListSlack);
            if (!area.Contains(mouse))
            {
                CloseList(restore: true);
                hoverCandidate = null;
            }
            return;
        }

        // The label under the pointer, if it touches another.
        List<Rectangle> rects = shown.Select(Bounds).ToList();
        int under = -1;
        for (int i = rects.Count - 1; i >= 0; i--)
        {
            if (rects[i].Contains(mouse))
            {
                under = i;
                break;
            }
        }
        if (under < 0)
        {
            waitForLeave = false;
        }
        List<int> crowd = under >= 0 && !waitForLeave ? TouchingClosure(rects, under) : null;
        if (crowd == null || crowd.Count < 2 || input.LeftButtonDown)
        {
            hoverCandidate = null;
            return;
        }
        if (hoverCandidate != shown[under])
        {
            hoverCandidate = shown[under];
            hoverSince = now;
            return;
        }
        if (now - hoverSince < HoverSeconds)
        {
            return;
        }

        // Open: top to bottom as they were on the map, the column starting at the hovered label.
        listAnchor = rects[under].Location;
        foreach (int i in GrowIntoList(rects, crowd, listAnchor, ListBottom()))
        {
            listed.Add(shown[i]);
            listedFrom[shown[i]] = rects[i].Location;
        }
        listMapPosition = The.MapUI.MapWindowWorldPosition;
        clickStarted = false;
    }

    private static void CloseList(bool restore)
    {
        if (restore)
        {
            foreach (MarkerWindow marker in listed)
            {
                if (marker.DisplayWindow != null && listedFrom.TryGetValue(marker, out Point from))
                {
                    marker.DisplayWindow.Position = from;
                }
            }
        }
        listed.Clear();
        listedFrom.Clear();
        clickStarted = false;
    }

    private static Rectangle Bounds(MarkerWindow marker) =>
        new Rectangle(marker.DisplayWindow.X, marker.DisplayWindow.Y, marker.DisplayWindow.Width, marker.DisplayWindow.Height);

    /// <summary>The lowest a label may go: above the bottom interface, as HUDWindow.PlaceWindowInsideViewableArea keeps windows.</summary>
    private static int ListBottom() => The.Sim.Controller.DrawArea.Height - 160;

    /// <summary>
    /// The labels the list is made of, in list order: <paramref name="crowd"/>, and then every label
    /// the laid-out column itself touches, with everything touching those, until the column touches
    /// nothing new - tripleacoder: "If the list touches other labels, those labels should also be
    /// included in the list." Lining the crowd up moves it, and on the first version a label just
    /// above the new column stayed out of it (tripleacoder's screenshot, 2026-10-09: "only some of
    /// the labels are gathered").
    /// </summary>
    public static List<int> GrowIntoList(IList<Rectangle> rects, List<int> crowd, Point anchor, int bottom)
    {
        var members = new HashSet<int>(crowd);
        while (true)
        {
            List<int> ordered = members.OrderBy(i => rects[i].Y).ThenBy(i => rects[i].X).ToList();
            List<Rectangle> column = LayOutList(ordered.Select(i => rects[i]).ToList(), anchor, bottom);
            List<int> touched = Enumerable.Range(0, rects.Count)
                .Where(i => !members.Contains(i) && column.Any(c => Touch(c, rects[i])))
                .ToList();
            if (touched.Count == 0)
            {
                return ordered;
            }
            foreach (int i in touched)
            {
                members.UnionWith(TouchingClosure(rects, i));
            }
        }
    }

    /// <summary>Overlapping, or edge to edge: labels a pixel apart read as one heap.</summary>
    private static bool Touch(Rectangle a, Rectangle b)
    {
        a.Inflate(1, 1);
        return a.Intersects(b);
    }

    /// <summary>
    /// The label at <paramref name="start"/> and every label that touches it, or touches one that
    /// does, and so on - tripleacoder: "If the list touches other labels, those labels should also
    /// be included". Indexes into <paramref name="rects"/>, <paramref name="start"/> first.
    /// </summary>
    public static List<int> TouchingClosure(IList<Rectangle> rects, int start)
    {
        var found = new List<int> { start };
        var seen = new HashSet<int> { start };
        for (int next = 0; next < found.Count; next++)
        {
            Rectangle current = rects[found[next]];
            for (int i = 0; i < rects.Count; i++)
            {
                if (!seen.Contains(i) && Touch(current, rects[i]))
                {
                    seen.Add(i);
                    found.Add(i);
                }
            }
        }
        return found;
    }

    /// <summary>
    /// Where each label goes in the list: one under another from <paramref name="anchor"/>, left
    /// edges lined up, <see cref="ListGap"/> apart. A label that would end below
    /// <paramref name="bottom"/> starts a new column to the right of the widest one so far, at the
    /// same top. A list taller than the room under the anchor first moves up, as far as the top of
    /// the screen.
    /// </summary>
    public static List<Rectangle> LayOutList(IList<Rectangle> sizes, Point anchor, int bottom)
    {
        var placed = new List<Rectangle>(sizes.Count);
        int total = sizes.Sum(r => r.Height + ListGap) - ListGap;
        int top = Math.Max(0, Math.Min(anchor.Y, bottom - total));
        int x = anchor.X, y = top, columnWidth = 0;
        foreach (Rectangle size in sizes)
        {
            if (y > top && y + size.Height > bottom)
            {
                x += columnWidth + ListGap;
                y = top;
                columnWidth = 0;
            }
            placed.Add(new Rectangle(x, y, size.Width, size.Height));
            y += size.Height + ListGap;
            columnWidth = Math.Max(columnWidth, size.Width);
        }
        return placed;
    }

    private static Rectangle Union(IList<Rectangle> rects)
    {
        Rectangle all = rects[0];
        foreach (Rectangle r in rects)
        {
            all = Rectangle.Union(all, r);
        }
        return all;
    }
}
