using System;
using GameStateManagement;
using Microsoft.Xna.Framework;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

/// <summary>
/// PORT: the main menu's MODDING window. tripleacoder, "Main menu", 2026-10-10: "the Test and Edit
/// buttons on the main menu are hard to understand ... replacing TEST and EDIT with a new button
/// MODDING. MODDING should open a small window with the 3 buttons - SELECT MODS, MAP EDITOR, TEST
/// SCENE."
///
/// MAP EDITOR and TEST SCENE are the studio's own MainMenuScreen.EditMap and TestMap, which the
/// Unhidden Mod gave buttons to; they stay under its MAP EDITOR BUTTONS switch (Kastuk's), shown
/// greyed with the reason when it is off.
/// </summary>
public class ModdingDialog : Panel
{
	private LCDScreen lcdScreen;

	private UIComponent lcdSurface;

	private readonly MainMenuInterface menu;

	public ModdingDialog(MainMenuInterface intf)
		: base(intf, UWGame.Locale.Text("MODDING"), new Point(420, 200), new Vector2(260f, 250f), Level.Menu, PanelType.RegularEdges)
	{
		menu = intf;
		TextButton btClose = new TextButton(Interface.gui);
		Window.Add(btClose);
		btClose.Init(TextButton.TextButtonType.White);
		btClose.Text = UWGame.Locale.Text("CLOSE");
		btClose.ScaleWidthToFitText();
		PlaceRightButtonUnderLCD(btClose);
		btClose.Click += delegate
		{
			Hide();
		};
		FullLCDPanel.AddLCDPanelFitWindowWithBottomMargin(Interface, Window, 52, Panel.RosterMargin, out _, out lcdSurface, ref lcdScreen);

		bool editorButtons = UWGame.Mods.UnhiddenMod.Enabled && UWGame.Mods.UnhiddenMod.MapEditorButtons.On;
		string editorOff = UWGame.Mods.UnhiddenMod.Enabled
			? UWGame.Locale.Text("Switched off: OPTIONS -> MODS -> UNHIDDEN MOD -> MAP EDITOR BUTTONS.")
			: UWGame.Locale.Text("Comes with the Unhidden Mod, which is switched off: SELECT MODS.");
		int y = 12;
		y = AddButton(UWGame.Locale.Text("SELECT MODS"), UWGame.Locale.Text("Every mod: what it does, who made it, and a switch for each."), true, null, y, delegate
		{
			Hide();
			menu.mainMenuScreen.ShowSelectMods();
		});
		y = AddButton(UWGame.Locale.Text("MAP EDITOR"), UWGame.Locale.Text("Open a map in the editor: terrain, height, soil, vegetation, assets and resources."), editorButtons, editorOff, y, delegate
		{
			Hide();
			menu.mainMenuScreen.EditMap();
		});
		AddButton(UWGame.Locale.Text("TEST SCENE"), UWGame.Locale.Text("Pick a map and walk around it: no colonists, nothing at stake."), editorButtons, editorOff, y, delegate
		{
			Hide();
			menu.mainMenuScreen.TestMap();
		});
		AddDirtOnStraightEdges();
		Hide();
	}

	private int AddButton(string text, string toolTip, bool enabled, string whyNot, int y, ClickHandler click)
	{
		TextButton button = new TextButton(Interface.gui);
		lcdSurface.Add(button);
		button.Init(TextButton.TextButtonType.LCD);
		button.Text = text;
		button.Width = lcdSurface.Width - 24;
		button.X = 12;
		button.Y = y;
		button.Enabled = enabled;
		button.ToolTip = enabled ? toolTip : toolTip + " " + whyNot;
		button.Click += click;
		return button.Bottom + 10;
	}
}
