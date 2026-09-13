using System;
using GameStateManagement;
using Microsoft.Xna.Framework;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

public class MainMenuPanel : Panel
{
	public const int ButtonWidth = 106;

	public const int PanelHeight = 177;

	private const int blotTop = 67;

	private const int blotX = 23;

	public const int ButtonTop = 76;

	private OptionsDialog optionsDialog;

	public const int SecondButtonRowYPos = 116;

	public string MapDataXmlPath;

	public MainMenuPanel(MainMenuInterface intf, Point position)
		: base(intf, "MAIN MENU", position, new Vector2(424f, 177f), Level.Dialogs, PanelType.MainMenu)
	{
		optionsDialog = new OptionsDialog(intf);
		optionsDialog.CancelClick += optionsDialog_CancelClick;
		optionsDialog.OKClick += optionsDialog_OKClick;
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
		textButton.Text = "NEW GAME";
		textButton.Position = new Point(num2, image.Y + 6);
		textButton.Click += btNew_Click;
		textButton.Width = 106;
		textButton.Height = height2;
		TextButton textButton2 = new TextButton(Interface.gui);
		Window.Add(textButton2);
		textButton2.Init(TextButton.TextButtonType.Black);
		textButton2.Position = new Point(num2, textButton.Bottom + 1);
		textButton2.Text = "LOAD GAME";
		textButton2.Click += btLoadGame_Click;
		textButton2.Width = 106;
		textButton2.Height = height2;
		num2 += num;
		TextButton textButton3 = new TextButton(Interface.gui);
		Window.Add(textButton3);
		textButton3.Init(TextButton.TextButtonType.White);
		textButton3.Position = new Point(num2, 116);
		textButton3.Text = "OPTIONS";
		textButton3.Click += btOptions_Click;
		textButton3.Width = 106;
		num2 += 114;
		TextButton textButton4 = new TextButton(Interface.gui);
		Window.Add(textButton4);
		textButton4.Init(TextButton.TextButtonType.White);
		textButton4.Position = new Point(num2, 76);
		textButton4.Text = "CREDITS";
		textButton4.Click += btCredits_Click;
		textButton4.Width = 106;
		TextButton textButton5 = new TextButton(Interface.gui);
		Window.Add(textButton5);
		textButton5.Init(TextButton.TextButtonType.White);
		textButton5.Position = new Point(num2, 116);
		textButton5.Text = "EXIT";
		textButton5.Click += btExit_Click;
		textButton5.Width = 106;
		// UNHIDDEN MOD: EDIT and TEST reach the map editor.
		//
		// btEditMap_Click and btTestMap_Click below are the STUDIO'S OWN handlers, already
		// present and already calling MainMenuScreen.EditMap / TestMap - nothing was subscribed
		// to them. The whole feature was built and then left without a button, which is what
		// makes this mod's name apt.
		//
		// The coordinates are the patch's own: they fill the empty slot at y=76 directly above
		// OPTIONS, two 59px buttons where one 106px one would go. Not num2, which by this point
		// has advanced to the CREDITS/EXIT column and would stack them under CREDITS.
		if (UWGame.Mods.UnhiddenMod.Enabled)
		{
			TextButton textButton6 = new TextButton(Interface.gui);
			Window.Add(textButton6);
			textButton6.Init(TextButton.TextButtonType.White);
			textButton6.Position = new Point(142, 76);
			textButton6.Text = "EDIT";
			textButton6.Click += btEditMap_Click;
			textButton6.Width = 59;
			TextButton textButton7 = new TextButton(Interface.gui);
			Window.Add(textButton7);
			textButton7.Init(TextButton.TextButtonType.White);
			textButton7.Position = new Point(198, 76);
			textButton7.Text = "TEST";
			textButton7.Click += btTestMap_Click;
			textButton7.Width = 59;
		}
	}

	private void btNew_Click(UIComponent sender, EventArgs e)
	{
		((MainMenuInterface)Interface).mainMenuScreen.ShowScenarios();
	}

	private void btCredits_Click(UIComponent sender, EventArgs e)
	{
		((MainMenuInterface)Interface).mainMenuScreen.ShowCredits();
	}

	private void btLoad_Click(UIComponent sender, EventArgs e)
	{
	}

	private void btExit_Click(UIComponent sender, EventArgs e)
	{
		((MainMenuInterface)Interface).mainMenuScreen.Exit();
	}

	private void btIntro_Click(UIComponent sender, EventArgs e)
	{
		((MainMenuInterface)Interface).mainMenuScreen.ShowIntro();
	}

	private void btOptions_Click(UIComponent sender, EventArgs e)
	{
		optionsDialog.ShowDialog(modal: true);
	}

	private void btTestMap_Click(UIComponent sender, EventArgs e)
	{
		((MainMenuInterface)Interface).mainMenuScreen.TestMap();
	}

	private void btEditMap_Click(UIComponent sender, EventArgs e)
	{
		((MainMenuInterface)Interface).mainMenuScreen.EditMap();
	}

	private void btLoadReplay_Click(UIComponent sender, EventArgs e)
	{
		((MainMenuInterface)Interface).mainMenuScreen.LoadReplay();
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
