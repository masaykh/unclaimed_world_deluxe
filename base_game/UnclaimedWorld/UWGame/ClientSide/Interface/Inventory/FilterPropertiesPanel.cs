using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Inventory;

public class FilterPropertiesPanel : UIComponent
{
	private ComboBox cbFilter;

	private HorizontalList hzlFilters;

	private Grid grdHrzFilterContainer;

	private ImageButton ibRemoveAllFilters;

	private TextBox tbSearch;

	private ImageButton ibSearch;

	private const string addFilterPromptKey = "ADDFILTER";

	private FilterPropertySettings FilterPropertySettings;

	public event Action FiltersChanged;

	public FilterPropertiesPanel(GUIManager gui, bool makeRoomForExpandButton)
		: base(gui)
	{
		int y = 0;
		if (makeRoomForExpandButton)
		{
			y = 32;
			Height = 100;
		}
		else
		{
			Height = 72;
		}
		cbFilter = new ComboBox(gui, ListBoxType.LCDCombo, isEditable: false);
		Add(cbFilter);
		cbFilter.Init(ComboBoxTypes.LCD);
		cbFilter.X = 4;
		cbFilter.Y = y;
		cbFilter.Width = 185;
		PopulateFilterSettingsCombo();
		cbFilter.SelectedIndex = 0;
		cbFilter.SelectionChanged += cbFilter_SelectionChanged;
		cbFilter.ToolTip = "Select filter";
		cbFilter.DebugTag = "cbFilter";
		grdHrzFilterContainer = new Grid(gui, ListBoxType.LCD, Label.LabelType.LCDNormal);
		grdHrzFilterContainer.FixedItemHeights = false;
		grdHrzFilterContainer.RenderType = RenderType.Normal;
		Add(grdHrzFilterContainer);
		grdHrzFilterContainer.Font = GUIManager.LCDandHUDBodyFontPath;
		grdHrzFilterContainer.Width = 278;
		grdHrzFilterContainer.Height = 75;
		grdHrzFilterContainer.Position = new Point(222, 0);
		grdHrzFilterContainer.CanHaveFocus = true;
		grdHrzFilterContainer.ScrollBarEnabled = true;
		grdHrzFilterContainer.CanGrowInHeight = false;
		grdHrzFilterContainer.DebugTag = "filterGrid";
		grdHrzFilterContainer.BeginAddingEntries();
		hzlFilters = new HorizontalList(gui);
		hzlFilters.CenterItemsVertically = true;
		grdHrzFilterContainer.AddEntry(hzlFilters, hzlFilters);
		hzlFilters.HorizontalSpacing = 6;
		hzlFilters.Y = 5;
		hzlFilters.MaxWidth = grdHrzFilterContainer.Width - 10;
		hzlFilters.X = 0;
		hzlFilters.RefreshEntries();
		grdHrzFilterContainer.EndAddingEntries();
		ibRemoveAllFilters = new ImageButton(gui);
		Add(ibRemoveAllFilters);
		ibRemoveAllFilters.InitWithIcon(ImageButtonType.LCD, "HUD_icon_trash", hasCheckedState: false);
		ibRemoveAllFilters.Click += tbRemoveAllFilters_Click;
		ibRemoveAllFilters.ToolTip = "Remove all filters";
		ibRemoveAllFilters.X = 190;
		ibRemoveAllFilters.Y = 0;
		ibRemoveAllFilters.Height = 30;
		ibRemoveAllFilters.Width = 30;
		ibRemoveAllFilters.Visible = false;
		ibRemoveAllFilters.SetIconTint(GameData.Instance.GUIConstants.sidePanelTextColor);
		ibRemoveAllFilters.RecalculateIconPosition();
		tbSearch = new TextBox(gui);
		tbSearch.Init(TextBox.TextBoxType.LCD);
		tbSearch.IsEditable = true;
		Add(tbSearch);
		tbSearch.Width = 142;
		tbSearch.X = cbFilter.X;
		tbSearch.Y = cbFilter.Bottom;
		tbSearch.GetFocus += tbSearch_GetFocus;
		tbSearch.LoseFocus += tbSearch_LoseFocus;
		ibSearch = new ImageButton(gui);
		Add(ibSearch);
		ibSearch.InitWithIcon(ImageButtonType.LCD, "HUD_icon_search", hasCheckedState: true);
		ibSearch.Height = 30;
		ibSearch.Width = 30;
		ibSearch.CheckedMode = CheckedModes.SwitchCheckedStateOnClick;
		ibSearch.X = tbSearch.Right;
		ibSearch.Y = tbSearch.Y - 4;
		ibSearch.Click += ibSearch_Click;
		ibSearch.SetIconTint(GameData.Instance.GUIConstants.sidePanelTextColor);
		UpdateSearchFilterButton();
	}

	private void tbSearch_LoseFocus()
	{
		The.Client.EnableKeyboardShortcuts = true;
	}

	private void tbSearch_GetFocus()
	{
		The.Client.EnableKeyboardShortcuts = false;
	}

	private void ibSearch_Click(UIComponent sender, EventArgs e)
	{
		if (ibSearch.IsChecked)
		{
			FilterPropertySettings.EnableSearchText(tbSearch.Text);
		}
		else
		{
			FilterPropertySettings.DisableSearchText();
		}
		UpdateSearchFilterButton();
		UpdateRemoveAllFiltersButton();
		FireFiltersChangedEvent();
	}

	private void UpdateSearchFilterButton()
	{
		if (ibSearch.IsChecked)
		{
			ibSearch.ToolTip = "Click to deactivate the text filter";
			tbSearch.Enabled = false;
			tbSearch.ToolTip = $"Search term: '{tbSearch.Text}' The search text cannot be changed when the search button is active.";
		}
		else
		{
			ibSearch.ToolTip = "Click to activate the text filter using the text in the search field";
			tbSearch.Enabled = true;
			tbSearch.ToolTip = "";
		}
	}

	public void Fill(FilterPropertySettings settings)
	{
		FilterPropertySettings = settings;
		Populate();
	}

	private void Populate()
	{
		hzlFilters.Clear();
		foreach (KeyValuePair<string, FilterSetting> activeFilterSetting in FilterPropertySettings.ActiveFilterSettings)
		{
			AddFilterSettingToHzlList(activeFilterSetting.Value);
		}
		ibSearch.IsChecked = FilterPropertySettings.SearchTextActive;
		tbSearch.Text = FilterPropertySettings.GetSearchText();
		UpdateSearchFilterButton();
		UpdateRemoveAllFiltersButton();
	}

	private void tbRemoveAllFilters_Click(UIComponent sender, EventArgs e)
	{
		FilterPropertySettings.RemoveAllFilterSettings();
		FilterPropertySettings.DisableSearchText();
		Populate();
		FireFiltersChangedEvent();
	}

	private void cbFilter_SelectionChanged(UIComponent sender)
	{
		ComboBox comboBox = (ComboBox)sender;
		if (!comboBox.SelectedKey.Equals("ADDFILTER"))
		{
			FilterSetting filter = (FilterSetting)comboBox.SelectedKey;
			if (FilterPropertySettings.AddFilterSetting(filter))
			{
				AddFilterSettingToHzlList(filter);
			}
			comboBox.SelectedIndex = 0;
			hzlFilters.Sort(Grid.Sorting.Ascending, useFirstTag: true);
			hzlFilters.RefreshEntries();
			UpdateRemoveAllFiltersButton();
			FireFiltersChangedEvent();
		}
	}

	private void UpdateRemoveAllFiltersButton()
	{
		if (FilterPropertySettings.ActiveFilterSettings.Count > 0 || FilterPropertySettings.SearchTextActive)
		{
			ibRemoveAllFilters.Visible = true;
		}
		else
		{
			ibRemoveAllFilters.Visible = false;
		}
	}

	private void FireFiltersChangedEvent()
	{
		if (this.FiltersChanged != null)
		{
			this.FiltersChanged();
		}
	}

	private void AddFilterSettingToHzlList(FilterSetting filter)
	{
		string displayString = filter.GetDisplayString();
		TextButton textButton = InventoryPanel.CreateTextButton(guiManager, 190, 5, 0, displayString + "  X ", filter_Click);
		textButton.ScaleToFitText();
		textButton.Height = 30;
		textButton.ID = DataControlID.Filter;
		textButton.Tag1 = filter;
		textButton.ToolTip = "Click to remove";
		textButton.OrderByTag1 = (float)textButton.Width;
		hzlFilters.AddEntry(filter, textButton);
	}

	private void PopulateFilterSettingsCombo()
	{
		Enum.GetValues(typeof(StaticFilterSettings));
		cbFilter.AddEntry("ADDFILTER", "Add Filter:");
		string[] productionFilterSettings = GameData.Instance.GUIConstants.ProductionFilterSettings;
		foreach (string key in productionFilterSettings)
		{
			FilterSetting filterSetting = new FilterSetting(GameData.Instance.AllFilterSettingTypes[key]);
			cbFilter.AddEntry(filterSetting, filterSetting.GetDisplayString());
		}
	}

	private void filter_Click(UIComponent sender, EventArgs e)
	{
		FilterSetting filterSetting = (FilterSetting)((TextButton)sender).Tag1;
		hzlFilters.RemoveEntry(filterSetting);
		FilterPropertySettings.RemoveFilterSetting(filterSetting);
		hzlFilters.Sort(Grid.Sorting.Ascending, useFirstTag: true);
		hzlFilters.RefreshEntries();
		UpdateRemoveAllFiltersButton();
		FireFiltersChangedEvent();
	}
}
