using System;
using GameStateManagement;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface;
using UWGame.ClientSide.Interface.HUD_Windows;

namespace UWGame.ClientSide.MainMenu.Mods;

public class SelectModsInterface : CommonInterface
{
	private SelectModsPanel panel;

	public SelectModsScreen Screen;

	public new Tooltip Tooltip;

	public SelectModsInterface(SelectModsScreen loadReplayScreen, UnclaimedWorld game)
		: base(game)
	{
		Screen = loadReplayScreen;
	}

	public override void LoadContent()
	{
		base.LoadContent();
		panel = new SelectModsPanel(this, new Point(0, 0));
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
