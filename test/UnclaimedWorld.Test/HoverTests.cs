using System.Reflection;
using InputEventSystem;
using Microsoft.Xna.Framework;
using WindowSystem;

namespace UnclaimedWorld.Test
{
    /// <summary>
    /// The stuck double-arrow cursor (Kastuk, "Small interface issue", 2026-10-12: "I just got stuck
    /// of the doublearrow cursor on that moment, when move a cursor up from the Turnip gut sheet
    /// window"). A window's resize edges sit at ZOrder 0.3 and its close button at 0.4
    /// (Window.CreateResizableAreas / Init). UIComponent.CheckMouseStatus skips every child below the
    /// highest one already hit - so an edge the pointer leaves straight onto a higher sibling was never
    /// told, and kept its hover and its resize cursor.
    /// </summary>
    [TestClass]
    public sealed class HoverTests
    {
        private static readonly MethodInfo CheckMouseStatus =
            typeof(GUIManager).GetMethod("CheckMouseStatus", BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(MouseEventArgs) }, null)
            ?? throw new InvalidOperationException("GUIManager.CheckMouseStatus(MouseEventArgs) not found");

        /// <summary>A GUIManager without its constructor, which loads fonts: only what the hover code reads.</summary>
        private static GUIManager NewGui()
        {
            var gui = (GUIManager)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(GUIManager));
            var controls = new Dictionary<Level, List<UIComponent>>();
            foreach (Level level in Enum.GetValues(typeof(Level)))
            {
                controls[level] = new List<UIComponent>();
            }
            typeof(GUIManager).GetField("controls", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(gui, controls);
            typeof(GUIManager).GetProperty("ScreenWidth")!.SetValue(gui, 800);
            typeof(GUIManager).GetProperty("ScreenHeight")!.SetValue(gui, 600);
            return gui;
        }

        private static void MoveTo(GUIManager gui, int x, int y) =>
            CheckMouseStatus.Invoke(gui, new object[] { new MouseEventArgs { Position = new Point(x, y) } });

        private static bool Hovered(UIComponent c) =>
            (bool)typeof(UIComponent).GetProperty("IsMouseOver", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(c)!;

        [TestMethod]
        public void ResizeEdge_LeftOntoAHigherSibling_GivesTheCursorBack()
        {
            var gui = NewGui();
            var window = new UIComponent(gui) { X = 0, Y = 0, Width = 200, Height = 200 };
            // The close button first in the list, above the edge, as a window has them.
            var close = new UIComponent(gui) { X = 180, Y = 0, Width = 20, Height = 10, ZOrder = 0.4f };
            var topEdge = new ResizableArea(gui) { X = 0, Y = 0, Width = 200, Height = 10, ZOrder = 0.3f, ResizeArea = ResizeAreas.Top };
            window.Add(close);
            window.Add(topEdge);
            gui.Add(window);

            MoveTo(gui, 50, 5);
            Assert.IsTrue(Hovered(topEdge), "on the top edge");
            Assert.AreEqual(MouseSprites.ResizingNS, gui.MouseSprite, "the resize cursor shows on the edge");

            MoveTo(gui, 190, 5);
            Assert.IsFalse(Hovered(topEdge), "the edge lets go when the pointer moves onto the close button above it");
            Assert.AreEqual(MouseSprites.Normal, gui.MouseSprite, "and the cursor is the normal one again");
        }

        [TestMethod]
        public void LowerSibling_UnderTheHit_IsNotHovered()
        {
            var gui = NewGui();
            var window = new UIComponent(gui) { X = 0, Y = 0, Width = 200, Height = 200 };
            var high = new UIComponent(gui) { X = 0, Y = 0, Width = 100, Height = 100, ZOrder = 0.4f };
            var low = new UIComponent(gui) { X = 0, Y = 0, Width = 100, Height = 100, ZOrder = 0.1f };
            window.Add(high);
            window.Add(low);
            gui.Add(window);

            MoveTo(gui, 50, 50);
            Assert.IsTrue(Hovered(high), "the higher of two overlapping siblings gets the hover");
            Assert.IsFalse(Hovered(low), "the lower one, covered, does not - as before");
        }
    }
}
