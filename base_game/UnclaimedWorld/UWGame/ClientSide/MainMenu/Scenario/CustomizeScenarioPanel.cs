using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface;
using UWGame.ClientSide.Interface.LCD;
using UWGame.SimSide.Scenarios;
using UWGame.SimSide.XmlCollections;
using WindowSystem;

namespace UWGame.ClientSide.MainMenu.Scenario;

public class CustomizeScenarioPanel : Panel
{
	private class DifficultyEventArgs : EventArgs
	{
		public Difficulty Difficulty;
	}

	private class CustomDifficultyEventArgs : EventArgs
	{
		public OptionSet OptionSet;

		public string CustomDifficultyKey;
	}

	private class CustomDifficultyRandomizeEventArgs : EventArgs
	{
		public OptionSet OptionSet;
	}

	private LCDScreen lcdScreenDescription;

	private UIComponent lcdSurfaceDescription;

	private Grid descriptionSurfaceGrid;

	private Label lblHeader;

	private TextArea area;

	private Image image;

	private int descriptionBottom;

	private CRTScreen crtScreen;

	private Box displayBoxOptions;

	private LCDScreen lcdScreenOptions;

	private UIComponent lcdSurfaceOptions;

	private Grid optionsSurfaceGrid;

	private LCDInnerPanel pnCustom;

	private Grid grdOptions;

	private Label lblExtraOptions;

	private int scoreHeaderRight = 856;

	private const int minimumContentWidth = 640;

	private const int maximumContentWidth = 1024;

	private CommonInterface customizeScenarioInterface;

	private const int itemHeight = 90;

	private int itemPadding = 6;

	private CustomizeScenarioScreen screen;

	private RadioGroup mainDifficultyRadioGroup;

	private RadioButton rbCustom;

	private UIComponent errorPaddingBox;

	private Label lblError;

	private TextArea taDifficulty;

	private Image columnDivider1;

	private Image columnDivider2;

	private int selectionColumnWidth;

	private const int setTitleColumnX = 0;

	private const int difficultyColumnX = 180;

	private const int randomizeColumnX = 480;

	private const int selectionColumnX = 640;

	private const int scoreColumnX = 800;

	private const int columnSpacing = 12;

	private TextButton btStart;

	private TextButton btCancel;

	private int mainDifficultyYPos;

	private int nameColumnWidth = 80;

	private const int difficultyWidth = 180;

	private int randomizeWidth = 60;

	public event EventHandler CancelClick;

	public CustomizeScenarioPanel(CommonInterface intf, Point position, CustomizeScenarioScreen screen)
		: base(intf, "CREATE GAME", position, new Vector2(1000f, intf.gui.ScreenHeight - 180), Level.Middle)
	{
		customizeScenarioInterface = intf;
		this.screen = screen;
		selectionColumnWidth = 148;
		CreateOptionsScreen(intf);
		PopulateMainDifficulty();
		PopulateCustomOptions();
		SetPanelDimensions();
		CreateDescriptionScreens();
		PopulateDescription();
		Panel.AddDirtOnIrregularEdges(intf.gui, Window);
	}

	private void CreateOptionsScreen(CommonInterface intf)
	{
		int y = 280;
		FullLCDPanel.AddLCDPanelFitWindowWithBottomMargin(intf, Window, 52, new Point(16, y), out displayBoxOptions, out lcdSurfaceOptions, ref lcdScreenOptions);
		CreateSurfaceWithScrollbar(out optionsSurfaceGrid, lcdSurfaceOptions);
		optionsSurfaceGrid.DebugTag = "options";
		taDifficulty = new TextArea(intf.gui, ListBoxType.LCD);
		taDifficulty.RenderType = RenderType.CRTAndLCD;
		taDifficulty.Width = optionsSurfaceGrid.SurfaceWidth;
		taDifficulty.Init(Label.LabelType.LCDNormal);
		taDifficulty.CanGrowInHeight = true;
		taDifficulty.ScrollBarEnabled = false;
		taDifficulty.DebugTag = "taDifficulty";
		pnCustom = new LCDInnerPanel(intf.gui, optionsSurfaceGrid.SurfaceWidth, includeDecor: true);
		errorPaddingBox = new UIComponent(intf.gui);
		errorPaddingBox.Width = pnCustom.Panel.Width;
		errorPaddingBox.Height = 32;
		lblError = new Label(intf.gui);
		lblError.Init(Label.LabelType.LCDError);
		lblError.Width = pnCustom.Panel.Width;
		errorPaddingBox.Add(lblError);
		errorPaddingBox.CenterChildVertically(lblError);
		int y2 = 13;
		lblExtraOptions = new Label(intf.gui);
		pnCustom.Panel.Add(lblExtraOptions);
		lblExtraOptions.Init(Label.LabelType.LCDSmallHeadingBanner);
		lblExtraOptions.Text = "EXTRA OPTIONS";
		lblExtraOptions.Position = new Point(640, y2);
		grdOptions = new Grid(intf.gui, ListBoxType.LCD, Label.LabelType.LCDNormal);
		pnCustom.AddContentSetFullWidth(grdOptions);
		grdOptions.X = pnCustom.HorizontalContentPadding;
		grdOptions.Y = 30;
		grdOptions.IsOuterGrid = true;
		grdOptions.FixedItemHeights = true;
		grdOptions.ItemHeight = 40;
		SetOptionsGridWidth();
		grdOptions.HeightResize += grdOptions_HeightResize;
		grdOptions.CanGrowInHeight = true;
		grdOptions.ScrollBarEnabled = false;
		Rectangle sourceRectangle = intf.gui.GUISpriteSheet.GetSourceRectangle("main_panel_dirt_center");
		Panel.AddImage(intf.gui, Window, sourceRectangle, new Point(40, 30));
		int y3 = 6;
		columnDivider1 = new Image(intf.gui);
		columnDivider1.SetSkinLocation(SkinState.Normal, intf.gui.GUISpriteSheet.GetSourceRectangle("basic_line"));
		columnDivider1.Y = y3;
		columnDivider1.Width = 1;
		columnDivider1.ScaleImageToSizeOfControl = true;
		pnCustom.Panel.Add(columnDivider1);
		columnDivider2 = new Image(intf.gui);
		columnDivider2.SetSkinLocation(SkinState.Normal, intf.gui.GUISpriteSheet.GetSourceRectangle("basic_line"));
		columnDivider2.Y = y3;
		columnDivider2.Width = 1;
		columnDivider2.ScaleImageToSizeOfControl = true;
		pnCustom.Panel.Add(columnDivider2);
		int y4 = displayBoxOptions.Bottom + 6;
		btCancel = new TextButton(intf.gui);
		Window.Add(btCancel);
		btCancel.Init(TextButton.TextButtonType.White);
		btCancel.Position = new Point(displayBoxOptions.X - 2, y4);
		btCancel.Text = "MAIN";
		btCancel.ScaleWidthToFitText();
		btCancel.ToolTip = "Return to the main menu";
		btCancel.Click += btCancel_Click;
		btStart = new TextButton(intf.gui);
		Window.Add(btStart);
		btStart.Init(TextButton.TextButtonType.White);
		btStart.Text = "START";
		btStart.ToolTip = "Start the game";
		btStart.ScaleWidthToFitText();
		btStart.Position = new Point(displayBoxOptions.Right - btStart.Width + 3, y4);
		btStart.Click += btStart_Click;
	}

	private void SetOptionsGridWidth()
	{
		grdOptions.Width = pnCustom.Panel.Width - grdOptions.X;
	}

	private void grdOptions_HeightResize(UIComponent sender)
	{
		pnCustom.ContentHeight = grdOptions.Bottom;
		Image obj = columnDivider1;
		int height = (columnDivider2.Height = grdOptions.Height);
		obj.Height = height;
	}

	private void CreateDescriptionScreens()
	{
		lcdScreenDescription = null;
		int x = 16;
		int y = 52;
		int num = 4;
		int num2 = 280;
		int num3 = 216;
		image = new Image(Interface.gui);
		Window.Add(image);
		image.RenderType = RenderType.CRTAndLCD;
		image.ScaleImageToSizeOfControl = true;
		image.Width = num2;
		image.Height = num3;
		image.X = x;
		image.Y = y;
		StatusScreen.AddCRTPlasticFrame(Interface.gui, Window, image.Position, num2, num3, out var plasticEdge);
		crtScreen = Interface.DisplayPanelRenderer.AddCRT(image, new Point(plasticEdge.AbsolutePosition.X + num, plasticEdge.AbsolutePosition.Y + num), plasticEdge.Width - 2 * num, plasticEdge.Height - 2 * num, Window.Level, Window, ReflectionToUse.Small, isMonochrome: true);
		int right = plasticEdge.Right;
		FullLCDPanel.AddLCDPanel(Interface, Window, new Point(right, MarginTop), Window.Width - 16 - right, 224, out var display, out lcdSurfaceDescription, ref lcdScreenDescription);
		descriptionBottom = display.Bottom;
		int num4 = 12;
		CreateSurfaceWithScrollbar(out descriptionSurfaceGrid, lcdSurfaceDescription, canHaveFocus: false);
		descriptionSurfaceGrid.DebugTag = "descriptionGrid";
		descriptionSurfaceGrid.BeginAddingEntries();
		lblHeader = new Label(Interface.gui);
		descriptionSurfaceGrid.AddEntry(lblHeader, lblHeader);
		lblHeader.Init(Label.LabelType.LCDBigHeaderBanner);
		lblHeader.Y = 0;
		lblHeader.X = 0;
		lblHeader.Width = descriptionSurfaceGrid.Width;
		area = new TextArea(Interface.gui, ListBoxType.LCD);
		area.RenderType = RenderType.CRTAndLCD;
		area.Init(Label.LabelType.LCDNormal);
		area.CanGrowInHeight = true;
		descriptionSurfaceGrid.AddEntry(area, area);
		area.X = num4;
		area.Y = 55;
		area.Width = lcdSurfaceDescription.Width - 2 * num4;
		descriptionSurfaceGrid.EndAddingEntries();
	}

	private void btStart_Click(UIComponent sender, EventArgs e)
	{
		lblError.Text = "";
		SerializableDictionary<string, Option> selectedOptions;
		if (rbCustom != null && rbCustom.IsChecked)
		{
			if (GetCustomSettings(out var error, out selectedOptions))
			{
				StartGame(selectedOptions, null);
			}
			else
			{
				lblError.Text = error;
			}
		}
		else
		{
			GetMainSettings(out selectedOptions, out var mainDifficulty);
			StartGame(selectedOptions, mainDifficulty);
		}
	}

	private void StartGame(SerializableDictionary<string, Option> options, Difficulty mainDifficulty)
	{
		((CustomizeScenarioInterface)Interface).Screen.StartGame(options, mainDifficulty);
	}

	private bool GetCustomSettings(out string error, out SerializableDictionary<string, Option> selectedOptions)
	{
		error = null;
		selectedOptions = new SerializableDictionary<string, Option>();
		OptionSet[] optionSets = screen.Scenario.ScenarioData.OptionSets;
		foreach (OptionSet optionSet in optionSets)
		{
			if (grdOptions.TryGetEntry(optionSet, out var item))
			{
				OptionSet optionSet2 = (OptionSet)item.Tag1;
				ComboBox comboBox = (ComboBox)item.FindChildById(UIComponent.DataControlID.Difficulty);
				string difficultyKey = (string)comboBox.SelectedKey;
				List<Option> list = optionSet2.OptionsGroupedByDifficulty.First((IGrouping<string, Option> g) => g.Key == difficultyKey).ToList();
				if (((CheckBox)item.FindChildById(UIComponent.DataControlID.Randomize)).IsChecked)
				{
					Option randomListMember = Common.GetRandomListMember(list, screen.Controller.RandomGenerator);
					selectedOptions.Add(optionSet2.KeyName, randomListMember);
					continue;
				}
				Option option = null;
				if (list.Count > 1)
				{
					ComboBox cbOptionSelector = (ComboBox)item.FindChildById(UIComponent.DataControlID.Option);
					if (cbOptionSelector.SelectedKey == null)
					{
						error = "INPUT NEEDED - Please make a selection under the EXTRA OPTIONS column for: " + optionSet2.Name;
						return false;
					}
					option = optionSet2.Options.First((Option o) => o.KeyName == (string)cbOptionSelector.SelectedKey);
				}
				else
				{
					option = list[0];
				}
				selectedOptions.Add(optionSet2.KeyName, option);
			}
			else
			{
				selectedOptions.Add(optionSet.KeyName, optionSet.Options[0]);
			}
		}
		return true;
	}

	private void GetMainSettings(out SerializableDictionary<string, Option> selectedOptions, out Difficulty mainDifficulty)
	{
		mainDifficulty = null;
		selectedOptions = new SerializableDictionary<string, Option>();
		if (mainDifficultyRadioGroup == null)
		{
			return;
		}
		RadioButton selected = mainDifficultyRadioGroup.GetSelected();
		mainDifficulty = (Difficulty)selected.Tag1;
		OptionSet[] optionSets = screen.Scenario.ScenarioData.OptionSets;
		foreach (KeyValuePair<string, string[]> item in mainDifficulty.OptionsToUse)
		{
			_ = item.Key == "equipment";
			OptionSet optionSet = optionSets.FirstOrDefault((OptionSet o) => o.KeyName == item.Key);
			string randomOptionKey = Common.GetRandomListMember(item.Value, screen.Controller.RandomGenerator);
			Option value = optionSet.Options.FirstOrDefault((Option o) => o.KeyName == randomOptionKey);
			selectedOptions.Add(optionSet.KeyName, value);
		}
	}

	private void PopulateDescription()
	{
		string description = screen.Scenario.Description;
		string text = screen.Scenario.Image;
		string text2 = screen.Scenario.Name.ToUpper(Config.Culture);
		if (text != null)
		{
			Window.Add(image);
			image.SetSkinLocation(SkinState.Normal, Interface.gui.GUI_CRT_SpriteSheet.GetSourceRectangle(text));
		}
		else
		{
			Window.Remove(image);
		}
		image.Texture = Interface.gui.GUI_CRT_SpriteSheet.Texture;
		descriptionSurfaceGrid.BeginAddingEntries();
		lblHeader.Text = text2;
		area.Text = description;
		descriptionSurfaceGrid.EndAddingEntries();
	}

	private void PopulateMainDifficulty()
	{
		if (screen.Scenario.ScenarioData.MainDifficultySettings == null || screen.Scenario.ScenarioData.MainDifficultySettings.Length == 0)
		{
			return;
		}
		optionsSurfaceGrid.BeginAddingEntries();
		Box box = new Box(Interface.gui);
		box.SetSkinLocation(SkinState.Normal, Interface.gui.GUISpriteSheet.GetSourceRectangle("basic_header_big"));
		box.Height = 36;
		box.CornerSize = 18;
		optionsSurfaceGrid.AddEntry(box, box);
		Label label = new Label(Interface.gui);
		box.Add(label);
		label.Init(Label.LabelType.LCDNormalLight);
		label.X = 14;
		label.Text = "DIFFICULTY:";
		label.FitToText();
		box.CenterChildVertically(label);
		mainDifficultyRadioGroup = new RadioGroup(Interface.gui);
		box.Add(mainDifficultyRadioGroup);
		mainDifficultyRadioGroup.Width = optionsSurfaceGrid.Width;
		mainDifficultyRadioGroup.Height = 40;
		mainDifficultyRadioGroup.Position = new Point(label.Right + 12, mainDifficultyYPos);
		mainDifficultyRadioGroup.DebugTag = "radioGroup";
		optionsSurfaceGrid.EndAddingEntries();
		int y = 7;
		int x = 12;
		if (HasCustomOptions())
		{
			rbCustom = new RadioButton(Interface.gui);
			mainDifficultyRadioGroup.Add(rbCustom);
			rbCustom.Init(CheckBoxType.LCDRadio);
			rbCustom.X = x;
			rbCustom.Y = y;
			rbCustom.Text = "CUSTOM";
			rbCustom.Click += rbCustom_Click;
			x = rbCustom.Right + 12;
		}
		bool flag = false;
		Difficulty[] mainDifficultySettings = screen.Scenario.ScenarioData.MainDifficultySettings;
		foreach (Difficulty difficulty in mainDifficultySettings)
		{
			if (difficulty.OptionsToUse != null)
			{
				RadioButton radioButton = new RadioButton(Interface.gui);
				mainDifficultyRadioGroup.Add(radioButton);
				radioButton.Init(CheckBoxType.LCDRadio);
				radioButton.X = x;
				radioButton.Y = y;
				radioButton.Text = difficulty.Name.ToUpper(Config.Culture);
				radioButton.Tag1 = difficulty;
				radioButton.EventArgs = new DifficultyEventArgs
				{
					Difficulty = difficulty
				};
				radioButton.Click += rbDifficultySetting_Click;
				if (difficulty.IsDefault)
				{
					radioButton.IsChecked = true;
					flag = true;
				}
				x = radioButton.Right + 12;
			}
		}
		if (!flag)
		{
			((RadioButton)mainDifficultyRadioGroup.Controls[1]).IsChecked = true;
		}
		ShowDifficulty((Difficulty)mainDifficultyRadioGroup.GetSelected().Tag1);
	}

	private void ArrangeColumns()
	{
		foreach (UIComponent entry in grdOptions.Entries)
		{
			Label label = (Label)entry.FindChildById(UIComponent.DataControlID.Caption);
			label.Width = nameColumnWidth;
			ComboBox comboBox = (ComboBox)entry.FindChildById(UIComponent.DataControlID.Difficulty);
			comboBox.X = label.Right + 6;
			columnDivider1.X = comboBox.Right + grdOptions.X + 10;
			CheckBox checkBox = (CheckBox)entry.FindChildById(UIComponent.DataControlID.Randomize);
			checkBox.X = columnDivider1.Right;
			lblExtraOptions.X = checkBox.X + grdOptions.X + 4;
			ComboBox comboBox2 = (ComboBox)entry.FindChildById(UIComponent.DataControlID.Option);
			comboBox2.X = checkBox.Right + 6;
			columnDivider2.X = comboBox2.Right + grdOptions.X + 4;
		}
	}

	private void SetPanelDimensions()
	{
		int widthForContentSize = GetWidthForContentSize();
		SetContentWidth(widthForContentSize);
		SetContentHeight();
		SetLCDAndWindowDimensions();
	}

	private void SetContentHeight()
	{
		if (pnCustom.Panel.Bottom < 586)
		{
			int bottom = pnCustom.Panel.Bottom;
			optionsSurfaceGrid.Height = bottom + 72;
			lcdSurfaceOptions.Height = optionsSurfaceGrid.Height;
		}
	}

	private int GetWidthForContentSize()
	{
		int val = ((RadioButton)mainDifficultyRadioGroup.Controls.FindLast((UIComponent c) => true)).Right + 6;
		int val2 = pnCustom.HorizontalContentPadding + scoreHeaderRight + 21;
		return Common.Clamp(Math.Max(val, val2), 640, 1024);
	}

	private void SetContentWidth(int width)
	{
		grdOptions.Width = width;
		pnCustom.ContentWidth = grdOptions.Width;
		optionsSurfaceGrid.SetContentWidth(pnCustom.Panel.Width);
		lcdSurfaceOptions.Width = optionsSurfaceGrid.Width;
	}

	private void SetLCDAndWindowDimensions()
	{
		FullLCDPanel.SetFullsizeLCDScreenDimensionsFromContent(Window, displayBoxOptions, lcdScreenOptions, 16, lcdSurfaceOptions.Width, lcdSurfaceOptions.Height);
		btStart.X = displayBoxOptions.Right - btStart.Width + 3;
		Window.Height = displayBoxOptions.Bottom + 52;
		PlaceLeftButtonUnderLCD(btCancel);
		PlaceRightButtonUnderLCD(btStart);
	}

	private void rbDifficultySetting_Click(UIComponent sender, EventArgs e)
	{
		ShowDifficulty(((DifficultyEventArgs)e).Difficulty);
		RemoveCustomControls();
	}

	private void RemoveCustomControls()
	{
		optionsSurfaceGrid.BeginAddingEntries();
		optionsSurfaceGrid.TryRemoveEntry(pnCustom.Panel);
		optionsSurfaceGrid.TryRemoveEntry(errorPaddingBox);
		lblError.Text = "";
		optionsSurfaceGrid.EndAddingEntries();
	}

	private void ShowDifficulty(Difficulty difficulty)
	{
		if (!optionsSurfaceGrid.EntriesByKey.ContainsKey(taDifficulty))
		{
			optionsSurfaceGrid.AddEntry(taDifficulty, taDifficulty);
			taDifficulty.CanHaveFocus = false;
		}
		taDifficulty.Text = difficulty.Description;
	}

	private static void PopulateExtraOptionSettings(OptionSet optionSet, string customDifficultyKey, CheckBox cbRandomize, ComboBox cbOptions)
	{
		IGrouping<string, Option> grouping = optionSet.OptionsGroupedByDifficulty.First((IGrouping<string, Option> g) => g.Key == customDifficultyKey);
		if (grouping.Count() > 1)
		{
			cbRandomize.Enabled = true;
			cbRandomize.IsChecked = false;
			cbRandomize.Visible = true;
			cbOptions.Enabled = true;
			cbOptions.Visible = true;
			PopulateOptionSelector(grouping, cbOptions);
		}
		else
		{
			cbRandomize.Enabled = false;
			cbRandomize.IsChecked = false;
			cbRandomize.DebugTag = "disabledcb";
			cbRandomize.Visible = false;
			cbOptions.Enabled = false;
			cbOptions.Clear();
			cbOptions.Visible = false;
		}
	}

	private void rbCustom_Click(UIComponent sender, EventArgs e)
	{
		if (rbCustom.IsChecked)
		{
			if (!optionsSurfaceGrid.EntriesByKey.ContainsKey(pnCustom.Panel))
			{
				pnCustom.Panel.Y = mainDifficultyYPos + 12;
				optionsSurfaceGrid.BeginAddingEntries();
				optionsSurfaceGrid.TryRemoveEntry(taDifficulty);
				optionsSurfaceGrid.AddEntry(pnCustom.Panel, pnCustom.Panel);
				optionsSurfaceGrid.AddEntry(errorPaddingBox, errorPaddingBox);
				optionsSurfaceGrid.EndAddingEntries();
			}
		}
		else
		{
			RemoveCustomControls();
		}
	}

	private bool HasCustomOptions()
	{
		if (screen.Scenario.ScenarioData.CustomEnabled)
		{
			OptionSet[] optionSets = screen.Scenario.ScenarioData.OptionSets;
			for (int i = 0; i < optionSets.Length; i++)
			{
				if (optionSets[i].Options.Length > 1)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void AddOptionSetRow(OptionSet optionSet)
	{
		if (optionSet.Options.Length < 2)
		{
			return;
		}
		UIComponent uIComponent = new UIComponent(Interface.gui);
		grdOptions.AddEntry(optionSet, uIComponent);
		Label.LabelType type = (optionSet.DisplayGroup % 3) switch
		{
			0 => Label.LabelType.LCDHeadingBlue, 
			1 => Label.LabelType.LCDHeadingGreen, 
			2 => Label.LabelType.LCDHeadingRed, 
			_ => Label.LabelType.LCDHeadingBlue, 
		};
		Label label = new Label(Interface.gui);
		uIComponent.Add(label);
		label.Init(type);
		label.Text = optionSet.Name.ToUpper(Config.Culture);
		label.Position = new Point(0, 0);
		label.FitToText();
		label.ToolTip = optionSet.Description;
		uIComponent.CenterChildVertically(label);
		label.Y += 2;
		label.ID = UIComponent.DataControlID.Caption;
		nameColumnWidth = Math.Max(nameColumnWidth, label.Width);
		ComboBox comboBox = new ComboBox(Interface.gui, ListBoxType.LCDCombo, isEditable: false);
		uIComponent.Add(comboBox);
		comboBox.Init(ComboBoxTypes.LCD);
		comboBox.ID = UIComponent.DataControlID.Difficulty;
		comboBox.X = 180;
		comboBox.Width = 180;
		comboBox.Tag1 = optionSet;
		uIComponent.CenterChildVertically(comboBox);
		Difficulty[] mainDifficultySettings = screen.Scenario.ScenarioData.MainDifficultySettings;
		string text = null;
		Option firstOptionInGroup;
		foreach (IGrouping<string, Option> item in optionSet.OptionsGroupedByDifficulty)
		{
			firstOptionInGroup = item.First();
			Difficulty difficulty = mainDifficultySettings.First((Difficulty d) => d.KeyName == firstOptionInGroup.Difficulty.KeyName);
			string text2 = firstOptionInGroup.Difficulty.Name ?? difficulty.Name;
			comboBox.AddEntry(item.Key, text2.ToUpper(Config.Culture));
			if (firstOptionInGroup.Difficulty.IsDefault)
			{
				text = item.Key;
			}
		}
		if (text == null)
		{
			comboBox.SelectedIndex = 0;
			text = (string)comboBox.SelectedKey;
		}
		else
		{
			comboBox.SelectedKey = text;
		}
		comboBox.SelectionChanged += cbDifficulty_SelectionChanged;
		CheckBox checkBox = new CheckBox(Interface.gui);
		uIComponent.Add(checkBox);
		checkBox.Init(CheckBoxType.LCD);
		checkBox.X = comboBox.Right + 12;
		checkBox.Text = "RANDOMIZE";
		checkBox.IsChecked = true;
		checkBox.EventArgs = new CustomDifficultyRandomizeEventArgs
		{
			OptionSet = optionSet
		};
		checkBox.Click += cbRandomize_Click;
		checkBox.ID = UIComponent.DataControlID.Randomize;
		uIComponent.CenterChildVertically(checkBox);
		checkBox.Y++;
		checkBox.FitToText();
		randomizeWidth = checkBox.Width;
		ComboBox comboBox2 = new ComboBox(Interface.gui, ListBoxType.LCDCombo, isEditable: false);
		uIComponent.Add(comboBox2);
		comboBox2.Init(ComboBoxTypes.LCD);
		comboBox2.ID = UIComponent.DataControlID.Option;
		comboBox2.X = 640;
		uIComponent.CenterChildVertically(comboBox2);
		comboBox2.Width = selectionColumnWidth;
		PopulateExtraOptionSettings(optionSet, text, checkBox, comboBox2);
	}

	private void cbDifficulty_SelectionChanged(UIComponent sender)
	{
		ComboBox obj = sender as ComboBox;
		OptionSet optionSet = (OptionSet)obj.Tag1;
		string customDifficultyKey = (string)obj.SelectedKey;
		grdOptions.TryGetEntry(optionSet, out var item);
		CheckBox cbRandomize = (CheckBox)item.FindChildById(UIComponent.DataControlID.Randomize);
		ComboBox cbOptions = (ComboBox)item.FindChildById(UIComponent.DataControlID.Option);
		PopulateExtraOptionSettings(optionSet, customDifficultyKey, cbRandomize, cbOptions);
	}

	private static void PopulateOptionSelector(IGrouping<string, Option> optionsForSelectedDifficulty, ComboBox cbOptions)
	{
		cbOptions.Clear();
		foreach (Option item in optionsForSelectedDifficulty)
		{
			cbOptions.AddEntry(item.KeyName, item.Name);
		}
	}

	private void ShowRandomOption()
	{
	}

	private void cbRandomize_Click(UIComponent sender, EventArgs e)
	{
		CheckBox obj = sender as CheckBox;
		OptionSet optionSet = (sender.EventArgs as CustomDifficultyRandomizeEventArgs).OptionSet;
		grdOptions.TryGetEntry(optionSet, out var item);
		ComboBox comboBox = (ComboBox)item.FindChildById(UIComponent.DataControlID.Option);
		if (!obj.IsChecked)
		{
			comboBox.Enabled = true;
			if (comboBox.Count == 0)
			{
				ComboBox comboBox2 = (ComboBox)item.FindChildById(UIComponent.DataControlID.Difficulty);
				string selectedDifficultyKey = (string)comboBox2.SelectedKey;
				PopulateOptionSelector(optionSet.OptionsGroupedByDifficulty.First((IGrouping<string, Option> g) => g.Key == selectedDifficultyKey), comboBox);
			}
		}
		else
		{
			comboBox.Enabled = false;
			comboBox.Clear();
		}
	}

	private void tbSelect_Click(UIComponent sender, EventArgs e)
	{
	}

	private void Populate()
	{
	}

	private void PopulateCustomOptions()
	{
		grdOptions.BeginAddingEntries();
		grdOptions.Clear();
		OptionSet[] optionSets = screen.Scenario.ScenarioData.OptionSets;
		foreach (OptionSet optionSet in optionSets)
		{
			AddOptionSetRow(optionSet);
		}
		grdOptions.EndAddingEntries();
		ArrangeColumns();
	}

	public override void ShowDialog(bool modal)
	{
		base.ShowDialog(modal);
		PopulateMainDifficulty();
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
