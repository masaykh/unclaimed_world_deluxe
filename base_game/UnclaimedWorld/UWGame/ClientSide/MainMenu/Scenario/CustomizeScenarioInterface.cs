using System;
using GameStateManagement;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface;
using UWGame.ClientSide.Interface.HUD_Windows;

namespace UWGame.ClientSide.MainMenu.Scenario;

internal class CustomizeScenarioInterface : CommonInterface
{
	private CustomizeScenarioPanel panel;

	public CustomizeScenarioScreen Screen;

	public new Tooltip Tooltip;

	public CustomizeScenarioInterface(CustomizeScenarioScreen screen, UnclaimedWorld game)
		: base(game)
	{
		Screen = screen;
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

	public override void LoadContent()
	{
		base.LoadContent();
		panel = new CustomizeScenarioPanel(this, new Point(0, 0), Screen);
		panel.Window.CenterWindow();
		panel.CancelClick += loadPanel_CancelClick;
		panel.Show();
		SetInterfaceCursor();
		Tooltip = new Tooltip(this);
	}

	private void loadPanel_CancelClick(object sender, EventArgs e)
	{
		Screen.ExitToMainMenu();
	}
}
