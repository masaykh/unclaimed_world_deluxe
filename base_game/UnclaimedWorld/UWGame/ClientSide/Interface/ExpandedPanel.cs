using System;
using Microsoft.Xna.Framework;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

public class ExpandedPanel : Panel
{
	protected bool hasBeenDrawn;

	protected int TitleHeight;

	protected Window cablesWindow;

	public bool HasCRT;

	public ExpandedPanel(string topTitle)
		: this(topTitle, The.InGameUI, new Point(The.InGameUI.expandedInterfaceLeft, 18), new Vector2(The.InGameUI.expandedInterfaceWidth, The.InGameUI.rosterPanelHeight), Level.Middle, includeCables: true)
	{
	}

	public ExpandedPanel(string topTitle, CommonInterface intf, Point pos, Vector2 dimensions, Level level, bool includeCables)
		: base(intf, null, pos, dimensions, level)
	{
		Window.HasCloseButton = false;
		Window.Level = Level.Middle;
	}

	public override void Hide()
	{
		if (cablesWindow != null)
		{
			cablesWindow.Hide();
		}
		base.Hide();
	}

	public override void Show()
	{
		if (cablesWindow != null)
		{
			cablesWindow.Show();
		}
		base.Show();
	}

	protected void AddDirtOnEdges()
	{
		AddDirtOnEdges(Interface.gui, Window);
	}

	public static void AddDirtOnLeftEdge(GUIManager gui, Window Form)
	{
		Rectangle sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_left");
		Panel.AddImage(gui, Form, sourceRectangle, new Point(0, 0));
		Panel.AddImage(gui, Form, sourceRectangle, new Point(0, sourceRectangle.Height)).RenderType = RenderType.Overlay;
	}

	public static Image AddDirtOnBottomEdge(GUIManager gui, Window Form)
	{
		Rectangle sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_bottom");
		Panel.AddImage(gui, Form, sourceRectangle, new Point(0, 0));
		Image image = Panel.AddImage(gui, Form, sourceRectangle, new Point(0, Form.Height - sourceRectangle.Height));
		image.RenderType = RenderType.Overlay;
		return image;
	}

	public static void AddDirtOnEdges(GUIManager gui, Window Form)
	{
		Rectangle sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_left");
		Panel.AddImage(gui, Form, sourceRectangle, new Point(0, 0));
		Panel.AddImage(gui, Form, sourceRectangle, new Point(0, sourceRectangle.Height)).RenderType = RenderType.Overlay;
		sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_bottom");
		Panel.AddImage(gui, Form, sourceRectangle, new Point(0, Form.Height - sourceRectangle.Height));
		sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_top");
		Panel.AddImage(gui, Form, sourceRectangle, new Point(0, -9)).RenderType = RenderType.Overlay;
		Panel.AddImage(gui, Form, sourceRectangle, new Point(sourceRectangle.Width, -9)).RenderType = RenderType.Overlay;
		sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_right");
		Panel.AddImage(gui, Form, sourceRectangle, new Point(Form.Width - sourceRectangle.Width, 0)).RenderType = RenderType.Overlay;
	}

	private void Close_OnPress(object sender, EventArgs e)
	{
	}
}
