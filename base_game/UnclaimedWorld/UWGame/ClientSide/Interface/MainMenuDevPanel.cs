using System;
using GameStateManagement;
using Microsoft.Xna.Framework;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

public class MainMenuDevPanel : Panel
{
	public MainMenuDevPanel(MainMenuInterface intf, Point position)
		: base(intf, UWGame.Locale.Text("DEV OPTIONS"), position, new Vector2(360f, 177f), Level.Dialogs, PanelType.MainMenu)
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
		textButton.Text = UWGame.Locale.Text("TEST SCENARIO");
		textButton.Click += btContinueGame_Click;
		textButton.ToolTip = UWGame.Locale.Text("Lists the debug scenarios, with the map each runs on, to pick one and start it.");
		textButton.Width = 106;
		TextButton textButton2 = new TextButton(Interface.gui);
		Window.Add(textButton2);
		textButton2.Init(TextButton.TextButtonType.White);
		textButton2.Position = new Point(x, 116);
		textButton2.Text = UWGame.Locale.Text("LOAD REPLAY");
		textButton2.Click += btLoadReplay_Click;
		textButton2.ToolTip = UWGame.Locale.Text("Load replay");
		textButton2.Width = 106;
	}

	private void btLoadReplay_Click(UIComponent sender, EventArgs e)
	{
		((MainMenuInterface)Interface).mainMenuScreen.LoadReplay();
	}

	private TestScenarioPanel scenarioList;

	private void btContinueGame_Click(UIComponent sender, EventArgs e)
	{
		// PORT: a list to pick from (TestScenarioPanel; tripleacoder, #general, 2026-10-24), where this
		// started whatever name was typed into TEST BUTTON SCENARIO.
		if (scenarioList == null)
		{
			scenarioList = new TestScenarioPanel(Interface, new Point(0, 0));
			scenarioList.StartClick += delegate
			{
				((MainMenuInterface)Interface).mainMenuScreen.StartTest(scenarioList.SelectedScenario.Value);
			};
		}
		scenarioList.Window.X = (Interface.Game.Controller.DrawArea.Width - scenarioList.Window.Width) / 2;
		scenarioList.Window.Y = (Interface.Game.Controller.DrawArea.Height - scenarioList.Window.Height) / 2;
		scenarioList.ShowDialog(modal: true);
	}

	public override void ShowDialog(bool modal)
	{
		base.ShowDialog(modal);
	}
}
