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

	private Box display;

	private LCDScreen lcdScreen;

	private UIComponent lcdSurface;

	private Grid grid;

	private string replayFileName;

	private SelectScenarioInterface selectScenarioInterface;

	private const int itemHeight = 112;

	private int itemPadding = 6;

	private const int selectButtonWidth = 80;

	public const int ThumbnailPanelWidth = 112;

	public const int DescriptionPanelWidth = 469;

	public const int SelectPanelWidth = 148;

	private const int horizPadding = 8;

	private const int vertPadding = 5;

	public event EventHandler CancelClick;

	public SelectScenarioPanel(SelectScenarioInterface intf, Point position)
		: base(intf, "SCENARIOS", position, new Vector2(800f, 620f), Level.Middle)
	{
		selectScenarioInterface = intf;
		RosterPanel.CreateRosterStyleLCDPanel(intf, Window, out display, out lcdSurface, ref lcdScreen, 55);
		grid = FullLCDPanel.AddGridWithFixedItemHeights(intf.gui, lcdSurface, 0);
		grid.ItemHeight = 112;
		grid.Selectability = Grid.SelectabilityOptions.None;
		InitButtons();
	}

	private List<UWGame.SimSide.Scenarios.Scenario> GetListOfScenarios()
	{
		// UNHIDDEN MOD: RGScenarioLoader lists only the built-in scenarios. AllScenarioLoader
		// adds the ones in user/Scenarios, which is what makes user-authored scenarios show up
		// in this picker at all.
		if (UWGame.Mods.UnhiddenMod.Enabled)
		{
			return UWGame.Mods.UnhiddenMod.AllScenarioHeaders();
		}
		return (from s in RGScenarioLoader.LoadAllScenarioHeaders()
			orderby s.SortOrder
			select s).ToList();
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
		label.Text = scenario.DisplayName ?? "(Missing)";
		Label label2 = new Label(Interface.gui);
		lCDInnerPanel2.AddContent(label2);
		label2.Init(Label.LabelType.LCDHeadingSteelGrey);
		label2.Text = "Map size: " + UWGame.SimSide.Scenarios.Scenario.GetMapSizeAsString(scenario.MapSize);
		label2.ToolTip = "The map size gives a hint about the hardware requirements for the AI to function properly. Larger maps usually have higher CPU demands.";
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
		textButton.Text = "SELECT";
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
			textButton.ToolTip = "Requires 64 bit, not available under 32 bit.";
		}
		grid.AddEntry(scenario, uIComponent);
	}

	private void tbSelect_Click(UIComponent sender, EventArgs e)
	{
		selectScenarioInterface.Screen.SelectScenario(((ScenarioEventArgs)e).Scenario);
	}

	private void Populate()
	{
		List<UWGame.SimSide.Scenarios.Scenario> listOfScenarios = GetListOfScenarios();
		grid.BeginAddingEntries();
		grid.Clear();
		foreach (UWGame.SimSide.Scenarios.Scenario item in listOfScenarios)
		{
			AddItemRow(item);
		}
		grid.EndAddingEntries();
	}

	private void btClose_Click(UIComponent sender, EventArgs e)
	{
		Window.Hide();
		if (this.CancelClick != null)
		{
			this.CancelClick(sender, e);
		}
	}

	private void InitButtons()
	{
		TextButton textButton = new TextButton(Interface.gui);
		Window.Add(textButton);
		textButton.Init(TextButton.TextButtonType.White);
		PlaceLeftButtonUnderLCD(textButton);
		textButton.Text = "MAIN";
		textButton.ScaleWidthToFitText();
		textButton.ToolTip = "Return to the main menu";
		textButton.Click += btCancel_Click;
		Rectangle sourceRectangle = Interface.gui.GUISpriteSheet.GetSourceRectangle("main_panel_dirt_center");
		Panel.AddImage(Interface.gui, Window, sourceRectangle, new Point(40, 30));
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
