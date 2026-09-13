using System;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface;
using WindowSystem;

namespace UWGame.ClientSide.Screens;

public class WinGamePanel : Panel
{
	private Box display;

	private LCDScreen lcdScreen;

	private UIComponent lcdSurface;

	private TextArea area;

	private Grid surfaceGrid;

	private int controlTop = 70;

	public string MapDataXmlPath;

	public string Text
	{
		set
		{
			area.Text = value;
		}
	}

	public event EventHandler CancelClick;

	public WinGamePanel(WinGameInterface intf, Point position)
		: base(intf, "GAME WON", position, LoseGamePanel.Dimensions, Level.Dialogs)
	{
		FullLCDPanel.AddLCDPanelFitWindowWithBottomMargin(intf, Window, 52, new Point(16, MarginTop), out display, out lcdSurface, ref lcdScreen);
		CreateSurfaceWithScrollbar(out surfaceGrid, lcdSurface, canHaveFocus: false);
		Panel.CreateTextArea(intf, ref area, surfaceGrid);
		int num = 12;
		area = new TextArea(intf.gui, ListBoxType.LCD);
		area.Init(Label.LabelType.LCDNormal);
		area.CanGrowInHeight = true;
		surfaceGrid.AddEntry(area, area);
		area.X = num;
		area.Y = 45;
		area.Width = lcdSurface.Width - 2 * num;
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
