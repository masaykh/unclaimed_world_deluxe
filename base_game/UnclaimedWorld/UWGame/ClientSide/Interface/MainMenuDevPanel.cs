using System;
using GameStateManagement;
using Microsoft.Xna.Framework;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

public class MainMenuDevPanel : Panel
{
	private int controlTop = 70;

	public MainMenuDevPanel(MainMenuInterface intf, Point position)
		: base(intf, "DEV OPTIONS", position, new Vector2(360f, 177f), Level.Dialogs, PanelType.MainMenu)
	{
		InitButtons();
	}

	private void InitButtons()
	{
		Rectangle sourceRectangle = Interface.gui.GUISpriteSheet.GetSourceRectangle("main_panel_dirt_center");
		Panel.AddImage(Interface.gui, Window, sourceRectangle, new Point(40, 30));
		int y = 76;
		int x = 42;
		TextButton textButton = new TextButton(Interface.gui);
		Window.Add(textButton);
		textButton.Init(TextButton.TextButtonType.White);
		textButton.Position = new Point(x, y);
		// DEBUG MOD: named apart from the main panel's TEST MAP. Two buttons said TEST, one
		// started a debug SCENARIO and the other opened a MAP PICKER, and a person testing had
		// no way to tell which was which - so the scenario setting looked inert for a day.
		textButton.Text = "TEST SCENARIO";
		textButton.Click += btContinueGame_Click;
		textButton.ToolTip = "Starts the debug scenario named by TEST BUTTON SCENARIO in the mod settings. DebugScenarios.txt beside the game lists all ninety.";
		textButton.Width = 106;
		TextButton textButton2 = new TextButton(Interface.gui);
		Window.Add(textButton2);
		textButton2.Init(TextButton.TextButtonType.White);
		textButton2.Position = new Point(x, 116);
		textButton2.Text = "LOAD REPLAY";
		textButton2.Click += btLoadReplay_Click;
		textButton2.ToolTip = "Load replay";
		textButton2.Width = 106;
	}

	private void btLoadReplay_Click(UIComponent sender, EventArgs e)
	{
		((MainMenuInterface)Interface).mainMenuScreen.LoadReplay();
	}

	private void btContinueGame_Click(UIComponent sender, EventArgs e)
	{
		((MainMenuInterface)Interface).mainMenuScreen.StartTest();
	}

	public override void ShowDialog(bool modal)
	{
		base.ShowDialog(modal);
	}
}
