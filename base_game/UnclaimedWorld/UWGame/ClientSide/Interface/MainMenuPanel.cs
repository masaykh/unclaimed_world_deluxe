using System;
using GameStateManagement;
using Microsoft.Xna.Framework;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

public class MainMenuPanel : Panel
{
	public const int ButtonWidth = 106;

	public const int PanelHeight = 177;
	public const int ButtonTop = 76;

	private OptionsDialog optionsDialog;

	private ModdingDialog moddingDialog;

	public const int SecondButtonRowYPos = 116;

	public string MapDataXmlPath;

	public MainMenuPanel(MainMenuInterface intf, Point position)
		: base(intf, UWGame.Locale.Text("MAIN MENU"), position, new Vector2(424f, 177f), Level.Dialogs, PanelType.MainMenu)
	{
		optionsDialog = new OptionsDialog(intf);
		optionsDialog.CancelClick += optionsDialog_CancelClick;
		optionsDialog.OKClick += optionsDialog_OKClick;
		moddingDialog = new ModdingDialog(intf);
		InitButtons();
	}

	private void optionsDialog_OKClick(object sender, EventArgs e)
	{
	}

	private void optionsDialog_CancelClick(object sender, EventArgs e)
	{
	}

	private void InitButtons()
	{
		int num = 120;
		int height = 84;
		Image image = new Image(Interface.gui);
		Window.Add(image);
		Rectangle sourceRectangle = Interface.gui.GUISpriteSheet.GetSourceRectangle("darksquare");
		image.SetSkinLocation(SkinState.Normal, sourceRectangle);
		image.Position = new Point(23, 67);
		image.Width = 113;
		image.Height = height;
		image.ScaleImageToSizeOfControl = true;
		sourceRectangle = Interface.gui.GUISpriteSheet.GetSourceRectangle("main_panel_dirt_center");
		Panel.AddImage(Interface.gui, Window, sourceRectangle, new Point(40, 30));
		int num2 = 26;
		int height2 = 36;
		TextButton textButton = new TextButton(Interface.gui);
		Window.Add(textButton);
		textButton.Init(TextButton.TextButtonType.Black);
		textButton.Text = UWGame.Locale.Text("NEW GAME");
		textButton.Position = new Point(num2, image.Y + 6);
		textButton.Click += btNew_Click;
		textButton.Width = 106;
		textButton.Height = height2;
		TextButton textButton2 = new TextButton(Interface.gui);
		Window.Add(textButton2);
		textButton2.Init(TextButton.TextButtonType.Black);
		textButton2.Position = new Point(num2, textButton.Bottom + 1);
		textButton2.Text = UWGame.Locale.Text("LOAD GAME");
		textButton2.Click += btLoadGame_Click;
		textButton2.Width = 106;
		textButton2.Height = height2;
		num2 += num;
		TextButton textButton3 = new TextButton(Interface.gui);
		Window.Add(textButton3);
		textButton3.Init(TextButton.TextButtonType.White);
		textButton3.Position = new Point(num2, 116);
		textButton3.Text = UWGame.Locale.Text("OPTIONS");
		textButton3.Click += btOptions_Click;
		textButton3.Width = 106;
		num2 += 114;
		TextButton textButton4 = new TextButton(Interface.gui);
		Window.Add(textButton4);
		textButton4.Init(TextButton.TextButtonType.White);
		textButton4.Position = new Point(num2, 76);
		textButton4.Text = UWGame.Locale.Text("CREDITS");
		textButton4.Click += btCredits_Click;
		textButton4.Width = 106;
		TextButton textButton5 = new TextButton(Interface.gui);
		Window.Add(textButton5);
		textButton5.Init(TextButton.TextButtonType.White);
		textButton5.Position = new Point(num2, 116);
		textButton5.Text = UWGame.Locale.Text("EXIT");
		textButton5.Click += btExit_Click;
		textButton5.Width = 106;
		// PORT: MODDING, in the empty slot above OPTIONS where the Unhidden Mod put EDIT and TEST
		// MAP (tripleacoder, "Main menu", 2026-10-10: "the Test and Edit buttons on the main menu
		// are hard to understand ... replacing TEST and EDIT with a new button MODDING"). It opens
		// SELECT MODS, MAP EDITOR and TEST SCENE (ModdingDialog) - the editor's two still under the
		// Unhidden Mod's MAP EDITOR BUTTONS switch, greyed there when it is off. Always shown: the
		// mods' own window is part of the game, not of a mod.
		TextButton textButton6 = new TextButton(Interface.gui);
		Window.Add(textButton6);
		textButton6.Init(TextButton.TextButtonType.White);
		textButton6.Position = new Point(textButton3.X, 76);
		textButton6.Text = UWGame.Locale.Text("MODDING");
		textButton6.Click += btModding_Click;
		textButton6.Width = 106;
		textButton6.ToolTip = UWGame.Locale.Text("Select mods, open the map editor, or try a map.");
	}

	private void btModding_Click(UIComponent sender, EventArgs e)
	{
		moddingDialog.ShowDialog(modal: true);
	}

	private void btNew_Click(UIComponent sender, EventArgs e)
	{
		((MainMenuInterface)Interface).mainMenuScreen.ShowScenarios();
	}

	private void btCredits_Click(UIComponent sender, EventArgs e)
	{
		((MainMenuInterface)Interface).mainMenuScreen.ShowCredits();
	}

	private void btExit_Click(UIComponent sender, EventArgs e)
	{
		((MainMenuInterface)Interface).mainMenuScreen.Exit();
	}

	private void btOptions_Click(UIComponent sender, EventArgs e)
	{
		optionsDialog.ShowDialog(modal: true);
	}

	private void btLoadGame_Click(UIComponent sender, EventArgs e)
	{
		((MainMenuInterface)Interface).mainMenuScreen.LoadGame();
	}

	public override void ShowDialog(bool modal)
	{
		base.ShowDialog(modal);
	}
}
