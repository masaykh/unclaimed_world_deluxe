using System;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface;
using WindowSystem;

namespace UWGame.ClientSide.Screens;

public class LoseGamePanel : Panel
{
	private Box display;

	private LCDScreen lcdScreen;

	private UIComponent lcdSurface;

	private int controlTop = 70;

	private TextArea area;

	private Grid surfaceGrid;

	public static Vector2 Dimensions = new Vector2(440f, 560f);

	public string MapDataXmlPath;

	public string Text
	{
		set
		{
			area.Text = value;
		}
	}

	public event EventHandler CancelClick;

	public LoseGamePanel(LoseGameInterface intf, Point position)
		: base(intf, "GAME END", position, Dimensions, Level.Dialogs)
	{
		FullLCDPanel.AddLCDPanelFitWindowWithBottomMargin(intf, Window, 52, new Point(16, MarginTop), out display, out lcdSurface, ref lcdScreen);
		CreateSurfaceWithScrollbar(out surfaceGrid, lcdSurface, canHaveFocus: false);
		Panel.CreateTextArea(intf, ref area, surfaceGrid);
		TextButton textButton = new TextButton(Interface.gui);
		Window.Add(textButton);
		textButton.Init(TextButton.TextButtonType.White);
		PlaceLeftButtonUnderLCD(textButton);
		textButton.Text = "DONE";
		textButton.Click += btDone_Click;
		textButton.ScaleWidthToFitText();
		AddDefaultDirt();
	}

	private void btDone_Click(UIComponent sender, EventArgs e)
	{
		if (this.CancelClick != null)
		{
			this.CancelClick(sender, e);
		}
	}

	public override void ShowDialog(bool modal)
	{
		base.ShowDialog(modal);
	}
}
