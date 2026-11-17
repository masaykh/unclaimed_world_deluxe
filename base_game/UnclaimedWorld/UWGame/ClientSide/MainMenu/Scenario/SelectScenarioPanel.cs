using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UWGame.ClientSide.Interface;
using UWGame.ClientSide.Interface.LCD;
using UWGame.SimSide.AllGameData.Scenarios;
using UWGame.SimSide.Scenarios;
using WindowSystem;

namespace UWGame.ClientSide.MainMenu.Scenario;

public class SelectScenarioPanel : Panel
{
	private class ScenarioEventArgs : EventArgs
	{
		public UWGame.SimSide.Scenarios.Scenario Scenario;
	}

	private LCDScreen lcdScreen;

	private UIComponent lcdSurface;

	private Grid grid;
	private SelectScenarioInterface selectScenarioInterface;

	// PORT (Kastuk, 2026-11-15: "show custom user scenarios in separated list at Start game menu to
	// not be mixed with vanilla scenarios. Like special button at bottom near Main to switch onto
	// user scenarios list."): the button beside MAIN that switches the list, and which list is
	// showing. Static so the choice survives going on to a scenario and coming back - the panel is
	// built anew each time this screen opens - and starts on the game's own list at every launch.
	private TextButton btUserScenarios;

	private static bool showUserScenarios;
	private int itemPadding = 6;
	public const int ThumbnailPanelWidth = 112;

	public const int DescriptionPanelWidth = 469;

	public const int SelectPanelWidth = 148;

	public event EventHandler CancelClick;

	public SelectScenarioPanel(SelectScenarioInterface intf, Point position)
		: base(intf, UWGame.Locale.Text("SCENARIOS"), position, new Vector2(800f, 620f), Level.Middle)
	{
		selectScenarioInterface = intf;
		RosterPanel.CreateRosterStyleLCDPanel(intf, Window, out _, out lcdSurface, ref lcdScreen, 55);
		grid = FullLCDPanel.AddGridWithFixedItemHeights(intf.gui, lcdSurface, 0);
		grid.ItemHeight = 112;
		grid.Selectability = Grid.SelectabilityOptions.None;
		InitButtons();
	}

	/// <summary>
	/// PORT: scenario headers including user-made ones. AllScenarioLoader.LoadAllScenarioHeaders
	/// enumerates user/Scenarios with Directory.GetDirectories, which throws on an installation
	/// that has never created the folder - every fresh install - so that falls back to the
	/// built-in list instead of taking out the main menu.
	/// </summary>
	public static List<UWGame.SimSide.Scenarios.Scenario> AllScenarioHeaders()
	{
		try
		{
			return (from s in AllScenarioLoader.LoadAllScenarioHeaders()
				orderby s.SortOrder
				select s).ToList();
		}
		catch (Exception ex) when (ex is System.IO.DirectoryNotFoundException || ex is System.IO.IOException || ex is UnauthorizedAccessException)
		{
			return (from s in RGScenarioLoader.LoadAllScenarioHeaders()
				orderby s.SortOrder
				select s).ToList();
		}
	}

	/// <summary>
	/// PORT: a user scenario is one read from user/Scenarios. Only RGScenarioLoader's headers go
	/// through ScenarioLoader.GetScenarioHeader, which calls SetRGSource; AllScenarioLoader's
	/// LoadUserScenarioHeaders deserializes scenario.xml and leaves Source at NotSet (Source is
	/// [XmlIgnore], so a file cannot claim to be the game's). Anything not the game's is the user's.
	/// </summary>
	public static bool IsUserScenario(UWGame.SimSide.Scenarios.Scenario scenario)
	{
		return scenario.Source != Source.RefactoredGames;
	}

	/// <summary>
	/// PORT: the part of the list one side of the GAME / USER SCENARIOS button shows, in the order
	/// it was given (AllScenarioHeaders' SortOrder), so the game's list reads exactly as it did
	/// before user scenarios were added to it.
	/// </summary>
	public static List<UWGame.SimSide.Scenarios.Scenario> ScenariosToShow(IEnumerable<UWGame.SimSide.Scenarios.Scenario> scenarios, bool userScenarios)
	{
		return scenarios.Where((UWGame.SimSide.Scenarios.Scenario s) => IsUserScenario(s) == userScenarios).ToList();
	}

	private List<UWGame.SimSide.Scenarios.Scenario> GetListOfScenarios()
	{
		// PORT (from the Unhidden Mod, now core - Kastuk, 2026-10-11): RGScenarioLoader lists only
		// the built-in scenarios. AllScenarioLoader adds the ones in user/Scenarios, which is what
		// makes user-authored scenarios show up in this picker at all.
		List<UWGame.SimSide.Scenarios.Scenario> scenarios = AllScenarioHeaders();
		// PORT: their names and descriptions in the chosen language (Locale.TranslateScenario).
		foreach (UWGame.SimSide.Scenarios.Scenario scenario in scenarios)
		{
			Locale.TranslateScenario(scenario);
		}
		return scenarios;
	}

	private void AddItemRow(UWGame.SimSide.Scenarios.Scenario scenario)
	{
		UIComponent uIComponent = new UIComponent(Interface.gui);
		LCDInnerPanel lCDInnerPanel = new LCDInnerPanel(Interface.gui, 112, includeDecor: false);
		uIComponent.Add(lCDInnerPanel.Panel);
		int num = 112 - 2 * itemPadding;
		if (!string.IsNullOrEmpty(scenario.ThumbnailImage))
		{
			Image image = new Image(Interface.gui);
			lCDInnerPanel.AddContentSetFullWidth(image);
			if (!Interface.gui.GUISpriteSheet.TryGetSourceRectangle(scenario.ThumbnailImage, out var spriteRect))
			{
				image.Texture = Interface.gui.ContentManager.Load<Texture2D>(scenario.ThumbnailImage);
			}
			else
			{
				image.SetSkinLocation(SkinState.Normal, spriteRect.Value);
				image.Texture = Interface.gui.GUISpriteSheet.Texture;
			}
			image.Position = new Point(itemPadding, itemPadding);
			image.Height = num;
			image.Width = num;
		}
		LCDInnerPanel lCDInnerPanel2 = new LCDInnerPanel(Interface.gui, 469, includeDecor: false);
		uIComponent.Add(lCDInnerPanel2.Panel);
		lCDInnerPanel2.Panel.X = lCDInnerPanel.Panel.Right - 2;
		lCDInnerPanel2.HorizontalContentPadding = 8;
		lCDInnerPanel2.VerticalContentPadding = 5;
		lCDInnerPanel2.ContentHeight = 100;
		Label label = new Label(Interface.gui);
		lCDInnerPanel2.AddContent(label);
		label.Init(Label.LabelType.LCDHeadingBlue);
		label.Text = scenario.DisplayName ?? UWGame.Locale.Text("(Missing)");
		Label label2 = new Label(Interface.gui);
		lCDInnerPanel2.AddContent(label2);
		label2.Init(Label.LabelType.LCDHeadingSteelGrey);
		label2.Text = UWGame.Locale.Text("Map size:") + " " + UWGame.SimSide.Scenarios.Scenario.GetMapSizeAsString(scenario.MapSize);
		label2.ToolTip = UWGame.Locale.Text("The map size gives a hint about the hardware requirements for the AI to function properly. Larger maps usually have higher CPU demands.");
		label2.FitToText();
		label2.X = lCDInnerPanel2.ContentWidth - label2.Width;
		TextArea textArea = new TextArea(Interface.gui, ListBoxType.LCD);
		lCDInnerPanel2.Panel.Add(textArea);
		lCDInnerPanel2.AddContentSetFullWidth(textArea);
		textArea.Init(Label.LabelType.LCDNormal);
		textArea.CanGrowInHeight = false;
		textArea.ScrollBarEnabled = false;
		textArea.Y = label.Bottom + 4;
		textArea.Height = lCDInnerPanel2.ContentHeight - textArea.Y;
		textArea.HMargin = 0;
		textArea.VMargin = 0;
		textArea.Text = scenario.SummaryDescription;
		LCDInnerPanel lCDInnerPanel3 = new LCDInnerPanel(Interface.gui, 148, includeDecor: false);
		uIComponent.Add(lCDInnerPanel3.Panel);
		lCDInnerPanel3.Panel.X = lCDInnerPanel2.Panel.Right - 2;
		TextButton textButton = new TextButton(Interface.gui);
		lCDInnerPanel3.Panel.Add(textButton);
		textButton.Init(TextButton.TextButtonType.LCD);
		textButton.Text = UWGame.Locale.Text("SELECT");
		textButton.ScaleWidthToFitText();
		textButton.X = lCDInnerPanel3.Panel.Width - textButton.Width - lCDInnerPanel3.HorizontalContentPadding;
		textButton.Y = lCDInnerPanel3.Panel.Height - textButton.Height;
		textButton.EventArgs = new ScenarioEventArgs
		{
			Scenario = scenario
		};
		textButton.Click += tbSelect_Click;
		if (!Environment.Is64BitProcess && !scenario.Allow32Bit)
		{
			textButton.Enabled = false;
			textButton.ToolTip = UWGame.Locale.Text("Requires 64 bit, not available under 32 bit.");
		}
		grid.AddEntry(scenario, uIComponent);
	}

	private void tbSelect_Click(UIComponent sender, EventArgs e)
	{
		selectScenarioInterface.Screen.SelectScenario(((ScenarioEventArgs)e).Scenario);
	}

	private void Populate()
	{
		List<UWGame.SimSide.Scenarios.Scenario> allScenarios = GetListOfScenarios();
		// PORT: the game's scenarios or the user's, never both (Kastuk, 2026-11-03: "separate user
		// and RG scenarios in the list ... not by alphabet like now"). With no user scenarios there
		// is nothing to switch to, so the button is hidden and the game's list shown - also when the
		// user list was showing and its folder has since been emptied.
		bool anyUserScenarios = allScenarios.Any(IsUserScenario);
		if (!anyUserScenarios)
		{
			showUserScenarios = false;
		}
		UpdateUserScenariosButton(anyUserScenarios);
		List<UWGame.SimSide.Scenarios.Scenario> listOfScenarios = ScenariosToShow(allScenarios, showUserScenarios);
		grid.BeginAddingEntries();
		grid.Clear();
		foreach (UWGame.SimSide.Scenarios.Scenario item in listOfScenarios)
		{
			AddItemRow(item);
		}
		grid.EndAddingEntries();
	}

	private void InitButtons()
	{
		TextButton textButton = new TextButton(Interface.gui);
		Window.Add(textButton);
		textButton.Init(TextButton.TextButtonType.White);
		PlaceLeftButtonUnderLCD(textButton);
		textButton.Text = UWGame.Locale.Text("MAIN");
		textButton.ScaleWidthToFitText();
		textButton.ToolTip = UWGame.Locale.Text("Return to the main menu");
		textButton.Click += btCancel_Click;
		// PORT: the list switch, beside MAIN and in its style (placed as AddBottomButtonInSequence
		// places a second bottom button). Its text and visibility are set by UpdateUserScenariosButton.
		btUserScenarios = new TextButton(Interface.gui);
		Window.Add(btUserScenarios);
		btUserScenarios.Init(TextButton.TextButtonType.White);
		PlaceButtonUnderLCD(btUserScenarios, textButton.Right + 5);
		btUserScenarios.Click += btUserScenarios_Click;
		Rectangle sourceRectangle = Interface.gui.GUISpriteSheet.GetSourceRectangle("main_panel_dirt_center");
		Panel.AddImage(Interface.gui, Window, sourceRectangle, new Point(40, 30));
	}

	/// <summary>
	/// PORT: the switch names the list it goes to; hidden while there are no user scenarios.
	/// </summary>
	private void UpdateUserScenariosButton(bool anyUserScenarios)
	{
		btUserScenarios.Visible = anyUserScenarios;
		if (showUserScenarios)
		{
			btUserScenarios.Text = UWGame.Locale.Text("GAME SCENARIOS");
			btUserScenarios.ToolTip = UWGame.Locale.Text("Show the scenarios that come with the game");
		}
		else
		{
			btUserScenarios.Text = UWGame.Locale.Text("USER SCENARIOS");
			btUserScenarios.ToolTip = UWGame.Locale.Text("Show your own scenarios, from the user/Scenarios folder");
		}
		btUserScenarios.ScaleWidthToFitText();
	}

	private void btUserScenarios_Click(UIComponent sender, EventArgs e)
	{
		showUserScenarios = !showUserScenarios;
		Populate();
	}

	public override void ShowDialog(bool modal)
	{
		base.ShowDialog(modal);
		Populate();
	}

	private void btCancel_Click(UIComponent sender, EventArgs e)
	{
		Window.Hide();
		if (this.CancelClick != null)
		{
			this.CancelClick(sender, e);
		}
	}

	public override void Update(GameTime elapsed)
	{
		base.Update(elapsed);
	}
}
