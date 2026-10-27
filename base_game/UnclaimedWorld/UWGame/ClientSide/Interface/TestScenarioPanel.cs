using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.SimSide;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

/// <summary>
/// PORT: the dev panel's TEST SCENARIO list. tripleacoder, #general, 2026-10-24: "to start a test
/// scenario, it is a bit too fiddly. First you have to open the mod options and enter the name in
/// a textbox ... It would be better if this button showed a list of test scenarios."
///
/// Every registered debug scenario (PlaceGameEntities.AllScenariosAndTheirMaps) with the map it
/// runs on, by name, in an LCD grid like the studio's LOAD MAP (MapEditorSaveLoadPanel). It opens
/// on the last one started; START starts the selected one and remembers it (DebugMod's TEST BUTTON
/// SCENARIO, so the setting is what was picked rather than something to type).
/// </summary>
public class TestScenarioPanel : Panel
{
	/// <summary>The map nearly all of them run on: the studio's test map, with no ground layers.</summary>
	public const string BlackGroundMap = "d Mezzomap MLo";

	private const int MapColumnX = 230;

	private LCDScreen lcdScreen;

	private UIComponent lcdSurface;

	private Grid grid;

	private TextArea taMessages;

	/// <summary>The scenario to start, once START is clicked.</summary>
	public PlaceGameEntities.DebugScenarios? SelectedScenario;

	public event EventHandler StartClick;

	public event EventHandler CancelClick;

	public TestScenarioPanel(CommonInterface intf, Point position)
		: base(intf, UWGame.Locale.Text("TEST SCENARIO"), position, new Vector2(440f, 560f), Level.Dialogs)
	{
		FullLCDPanel.AddLCDPanelFitWindowWithBottomMargin(intf, Window, 80, new Point(16, MarginTop), out _, out lcdSurface, ref lcdScreen);
		taMessages = new TextArea(intf.gui, ListBoxType.LCD);
		taMessages.RenderType = RenderType.CRTAndLCD;
		lcdSurface.Add(taMessages);
		taMessages.Init(Label.LabelType.LCDNormal);
		taMessages.Width = lcdSurface.Width;
		taMessages.CanGrowInHeight = false;
		taMessages.ScrollBarEnabled = false;
		taMessages.Height = 62;
		taMessages.Text = string.Format(UWGame.Locale.Text("Pick a debug scenario. Those on '{0}' have black ground: that studio test map has no soil or vegetation layer."), BlackGroundMap);
		grid = FullLCDPanel.AddGridWithFixedItemHeights(intf.gui, lcdSurface, taMessages.Bottom + 6);
		TextButton textButton = new TextButton(Interface.gui);
		Window.Add(textButton);
		textButton.Init(TextButton.TextButtonType.White);
		textButton.Text = UWGame.Locale.Text("START");
		textButton.ToolTip = UWGame.Locale.Text("Starts the selected scenario.");
		textButton.Click += btStart_Click;
		textButton.ScaleWidthToFitText();
		PlaceRightButtonUnderLCD(textButton);
		TextButton textButton2 = new TextButton(Interface.gui);
		Window.Add(textButton2);
		textButton2.Init(TextButton.TextButtonType.White);
		textButton2.Text = UWGame.Locale.Text("CANCEL");
		textButton2.ToolTip = UWGame.Locale.Text("Cancels and closes the dialog.");
		textButton2.ScaleWidthToFitText();
		textButton2.Click += btCancel_Click;
		PlaceLeftButtonUnderLCD(textButton2);
		AddDefaultDirt();
	}

	/// <summary>Every registered scenario and its map, by name.</summary>
	public static List<KeyValuePair<PlaceGameEntities.DebugScenarios, string>> Scenarios() =>
		PlaceGameEntities.AllScenariosAndTheirMaps()
			.OrderBy((KeyValuePair<PlaceGameEntities.DebugScenarios, string> s) => s.Key.ToString(), StringComparer.OrdinalIgnoreCase)
			.ToList();

	private void PopulateList()
	{
		List<KeyValuePair<PlaceGameEntities.DebugScenarios, string>> scenarios = Scenarios();
		grid.BeginAddingEntries();
		grid.Clear();
		foreach (KeyValuePair<PlaceGameEntities.DebugScenarios, string> scenario in scenarios)
		{
			grid.AddEntry(scenario.Key, scenario.Key.ToString(), MapColumnX, scenario.Value);
		}
		grid.EndAddingEntries();
		int current = scenarios.FindIndex((KeyValuePair<PlaceGameEntities.DebugScenarios, string> s) => s.Key == PlaceGameEntities.DefaultScenario);
		if (current >= 0)
		{
			grid.SelectedIndex = current;
			grid.ScrollToIndex(current);
		}
	}

	public override void ShowDialog(bool modal)
	{
		base.ShowDialog(modal);
		PopulateList();
	}

	private void btStart_Click(UIComponent sender, EventArgs e)
	{
		if (!grid.GetSelectedKey(out object key))
		{
			return;
		}
		SelectedScenario = (PlaceGameEntities.DebugScenarios)key;
		UWGame.Mods.DebugMod.RememberTestScenario(SelectedScenario.Value.ToString());
		Hide();
		StartClick?.Invoke(sender, e);
	}

	private void btCancel_Click(UIComponent sender, EventArgs e)
	{
		Hide();
		CancelClick?.Invoke(sender, e);
	}
}
