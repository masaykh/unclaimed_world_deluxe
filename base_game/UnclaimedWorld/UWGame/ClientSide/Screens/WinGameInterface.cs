using System;
using GameStateManagement;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface;

namespace UWGame.ClientSide.Screens;

public class WinGameInterface : CommonInterface
{
	private int loadPanelWidth = 600;

	private int totalHeight = 800;

	private int left;

	private int top;

	public WinGamePanel Panel;

	public WinGameScreen winGameScreen;

	public WinGameInterface(WinGameScreen winGameScreen, UnclaimedWorld game)
		: base(game)
	{
		this.winGameScreen = winGameScreen;
		game.Controller.ValidateDrawAreaWidth(loadPanelWidth);
		left = (game.Controller.DrawArea.Width - loadPanelWidth) / 2;
		top = (game.Controller.DrawArea.Height - totalHeight) / 2;
	}

	public override void LoadContent()
	{
		base.LoadContent();
		Panel = new WinGamePanel(this, new Point(left, top));
		Panel.CancelClick += loadPanel_CancelClick;
		Panel.Show();
		SetInterfaceCursor();
	}

	private void loadPanel_CancelClick(object sender, EventArgs e)
	{
		winGameScreen.ExitToMainMenu();
	}

	private void loadPanel_SaveOrLoadClick(object sender, EventArgs e)
	{
	}
}
