using System;
using GameStateManagement;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface;
using UWGame.ClientSide.Interface.HUD_Windows;

namespace UWGame.ClientSide.MainMenu.Scenario;

public class SelectScenarioInterface : CommonInterface
{
	private SelectScenarioPanel panel;

	public SelectScenarioScreen Screen;

	public new Tooltip Tooltip;

	public SelectScenarioInterface(SelectScenarioScreen loadReplayScreen, UnclaimedWorld game)
		: base(game)
	{
		Screen = loadReplayScreen;
	}

	public override void LoadContent()
	{
		base.LoadContent();
		panel = new SelectScenarioPanel(this, new Point(0, 0));
		panel.Window.CenterWindow();
		panel.CancelClick += loadPanel_CancelClick;
		panel.ShowDialog(modal: true);
		Tooltip = new Tooltip(this);
		SetInterfaceCursor();
	}

	private void loadPanel_CancelClick(object sender, EventArgs e)
	{
		Screen.ExitToMainMenu();
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);
		Tooltip.Update(gameTime);
	}

	public override void Destroy()
	{
		base.Destroy();
		Tooltip.Destroy();
	}
}
