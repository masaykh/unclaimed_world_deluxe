using System;
using GameStateManagement;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface;

namespace UWGame.ClientSide.Screens;

public class LoseGameInterface : CommonInterface
{
	private int loadPanelWidth = 600;

	private int totalHeight = 800;

	private int left;

	private int top;

	public LoseGamePanel Panel;

	public LoseGameScreen loseGameScreen;

	public LoseGameInterface(LoseGameScreen loseGameScreen, UnclaimedWorld game)
		: base(game)
	{
		this.loseGameScreen = loseGameScreen;
		game.Controller.ValidateDrawAreaWidth(loadPanelWidth);
		left = (game.Controller.DrawArea.Width - loadPanelWidth) / 2;
		top = (game.Controller.DrawArea.Height - totalHeight) / 2;
		SetInterfaceCursor();
	}

	public override void LoadContent()
	{
		base.LoadContent();
		Panel = new LoseGamePanel(this, new Point(left, top));
		Panel.CancelClick += loadPanel_CancelClick;
		Panel.Show();
		SetInterfaceCursor();
	}

	private void loadPanel_CancelClick(object sender, EventArgs e)
	{
		loseGameScreen.ExitToMainMenu();
	}
}
