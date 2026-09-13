using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface.Controls;
using UWGame.ClientSide.Interface.Overlays;
using UWGame.SimSide;
using UWGame.SimSide.AI;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Resources;
using WindowSystem;

namespace UWGame.ClientSide.Interface.HUD_Windows;

public class HUDOverlayPanel : HUDWindow
{
	private List<Grid> childGridsThatWereChanged = new List<Grid>();

	private const int quantityX = 10;

	private const int captionX = 25;

	private const int buildX = 120;

	private Grid outerGrid;

	private Grid grdSingleItems;

	private Grid categoryGrid;

	public HUDOverlayPanel()
		: base(244, 350)
	{
		UIComponent uIComponent = new UIComponent(gui);
		Add(uIComponent);
		uIComponent.X = 7;
		uIComponent.Y = 10;
		uIComponent.Width = DisplayWindow.ViewPort.Width - 2 * uIComponent.X - 2;
		uIComponent.Height = DisplayWindow.Height - 2 * uIComponent.Y - 5;
		grdSingleItems = new Grid(gui, ListBoxType.HUDAndLCD, Label.LabelType.HUDWindow);
		grdSingleItems.IsOuterGrid = false;
		grdSingleItems.X = 6;
		grdSingleItems.FixedItemHeights = true;
		grdSingleItems.Width = uIComponent.Width;
		grdSingleItems.ScrollBarEnabled = false;
		grdSingleItems.ItemHeight = 22;
		grdSingleItems.CanGrowInHeight = true;
		grdSingleItems.Font = GUIManager.LCDandHUDBodyFontPath;
		categoryGrid = new Grid(gui, ListBoxType.HUDAndLCD, Label.LabelType.HUDWindow);
		categoryGrid.FixedItemHeights = false;
		categoryGrid.CanGrowInHeight = true;
		categoryGrid.ScrollBarEnabled = false;
		categoryGrid.RenderType = RenderType.Normal;
		categoryGrid.HMargin = 0;
		categoryGrid.VMargin = 0;
		categoryGrid.Font = GUIManager.LCDandHUDBodyFontPath;
		categoryGrid.Width = uIComponent.Width;
		categoryGrid.ItemHeight = 26;
		categoryGrid.Position = new Point(0, 0);
		outerGrid = HUDWindow.CreateOuterGridForCollapsableLists(The.InGameUI.gui, uIComponent);
		outerGrid.IsOuterGrid = true;
		outerGrid.ScrollBarEnabled = true;
		outerGrid.BeginAddingEntries();
		outerGrid.AddEntry(grdSingleItems, grdSingleItems);
		outerGrid.AddEntry(categoryGrid, categoryGrid);
		outerGrid.EndAddingEntries();
	}

	public override void Refresh()
	{
		base.Refresh();
		Populate();
	}

	public override void Hide()
	{
		base.Hide();
		The.InGameUI.OverlayPanel.btOverlay.IsChecked = false;
	}

	public void Populate()
	{
		SharedKnowledge sharedKnowledge = The.InGameUI.UIAllegiance.SharedKnowledge;
		if (The.Sim.Mode == Sim.EngineMode.Game)
		{
			PopulateSingleItemGrid();
		}
		else
		{
			PopulateSingleItemGridEditor();
		}
		childGridsThatWereChanged.Clear();
		categoryGrid.BeginAddingEntries();
		PopulateResources(sharedKnowledge, "Show resource");
		PopulateStructureTypes(sharedKnowledge, "Show structure");
		PopulateEntityTypes(sharedKnowledge, "");
		foreach (Grid item in childGridsThatWereChanged)
		{
			item.EndAddingEntries();
		}
		categoryGrid.EndAddingEntries();
	}

	private void PopulateSingleItemGrid()
	{
		grdSingleItems.BeginAddingEntries();
		foreach (object value2 in Enum.GetValues(typeof(OverlayTypes)))
		{
			if (!grdSingleItems.EntriesByKey.TryGetValue(value2, out var value))
			{
				OverlayTypes overlayTypes = (OverlayTypes)value2;
				value = AddSingleItemRow(overlayTypes, OverlaySettings.GetTextFromOverlayType(overlayTypes), null, OverlaySettings.GetTooltipFromOverlayType(overlayTypes), OverlaySettings.GetIconFromOverlayType(overlayTypes), OverlaySettings.GetColorFromOverlayType(overlayTypes), cbSelectOverlayType_Click);
			}
			UpdateSingleItemRow(value);
		}
		grdSingleItems.EndAddingEntries();
	}

	private void PopulateSingleItemGridEditor()
	{
		grdSingleItems.BeginAddingEntries();
		foreach (object value2 in Enum.GetValues(typeof(EditorOverlayTypes)))
		{
			if (!grdSingleItems.EntriesByKey.TryGetValue(value2, out var value))
			{
				EditorOverlayTypes editorOverlayTypes = (EditorOverlayTypes)value2;
				value = AddSingleItemRow(editorOverlayTypes, OverlaySettings.GetTextFromOverlayType(editorOverlayTypes), null, OverlaySettings.GetTooltipFromOverlayType(editorOverlayTypes), OverlaySettings.GetIconFromOverlayType(editorOverlayTypes), OverlaySettings.GetColorFromOverlayType(editorOverlayTypes), cbSelectEditorOverlayType_Click);
			}
			UpdateSingleItemRowEditor(value);
		}
		grdSingleItems.EndAddingEntries();
	}

	private void PopulateStructureTypes(SharedKnowledge sharedKnowledge, string checkboxTooltip)
	{
		Grid grdGrouping = null;
		EntityGrouping entityGrouping = EntityGrouping.Structures;
		foreach (KeyValuePair<EntityType, List<EntityID>> structure in sharedKnowledge.AllKnownEntities.Structures)
		{
			if (structure.Value.Count > 0)
			{
				AddAndUpdateEntityGroupingAndRow(structure.Key, checkboxTooltip, out grdGrouping, entityGrouping);
			}
			if (sharedKnowledge.AllKnownEntities.AllEntities.TryGetValue(structure.Key, out var value) && value.Count != 0)
			{
				continue;
			}
			if (grdGrouping == null && categoryGrid.TryGetEntry(entityGrouping, out var item))
			{
				grdGrouping = (Grid)(item as CollapsablePanel).ExpandedPanel.Controls[0];
				grdGrouping.BeginAddingEntries();
				childGridsThatWereChanged.Add(grdGrouping);
			}
			if (grdGrouping == null)
			{
				continue;
			}
			grdGrouping.TryRemoveEntry(structure.Key);
			if (grdGrouping.Entries.Count == 0)
			{
				categoryGrid.TryRemoveEntry(entityGrouping);
				if (grdGrouping != null)
				{
					childGridsThatWereChanged.Remove(grdGrouping);
				}
			}
		}
	}

	private void PopulateEntityTypes(SharedKnowledge sharedKnowledge, string checkboxTooltip)
	{
		Grid grdGrouping = null;
		foreach (KeyValuePair<EntityType, List<EntityID>> allEntity in sharedKnowledge.AllKnownEntities.AllEntities)
		{
			EntityGrouping? grouping = OverlaySettings.GetGrouping(allEntity.Key);
			if (!grouping.HasValue || grouping == EntityGrouping.Structures)
			{
				continue;
			}
			if (allEntity.Value.Count > 0 && grouping.HasValue)
			{
				AddAndUpdateEntityGroupingAndRow(allEntity.Key, checkboxTooltip, out grdGrouping, grouping.Value);
			}
			if (grouping == EntityGrouping.Structures || !sharedKnowledge.AllKnownEntities.AllEntities.TryGetValue(allEntity.Key, out var value) || value.Count != 0)
			{
				continue;
			}
			if (categoryGrid.TryGetEntry(grouping, out var item))
			{
				grdGrouping = (Grid)(item as CollapsablePanel).ExpandedPanel.Controls[0];
				grdGrouping.BeginAddingEntries();
				childGridsThatWereChanged.Add(grdGrouping);
			}
			if (grdGrouping == null)
			{
				continue;
			}
			grdGrouping.TryRemoveEntry(allEntity.Key);
			if (grdGrouping.Entries.Count == 0)
			{
				categoryGrid.TryRemoveEntry(grouping);
				if (grdGrouping != null)
				{
					childGridsThatWereChanged.Remove(grdGrouping);
				}
			}
		}
	}

	private void PopulateResources(SharedKnowledge sharedKnowledge, string checkboxTooltip)
	{
		if (The.Sim.Mode == Sim.EngineMode.Game)
		{
			foreach (KeyValuePair<ResourceType, HashSet<ResourceID>> allKnownResourceContainer in sharedKnowledge.PlaySiteKnowledge.AllKnownResourceContainers)
			{
				ResourceType key = allKnownResourceContainer.Key;
				int count = allKnownResourceContainer.Value.Count;
				HandleResource(checkboxTooltip, key, count, (ResourceType r) => ShouldDisplayResourceInGame(sharedKnowledge, r));
			}
			return;
		}
		foreach (ResourceType editorResource in The.Sim.PlaySite.EditorResources)
		{
			int noOfResourceContainers = 1;
			HandleResource(checkboxTooltip, editorResource, noOfResourceContainers, ShouldDisplayResourceInEditor);
		}
	}

	private bool ShouldDisplayResourceInGame(SharedKnowledge sharedKnowledge, ResourceType resourceType)
	{
		if (sharedKnowledge.PlaySiteKnowledge.AllKnownResourceContainers.TryGetValue(resourceType, out var value))
		{
			return value.Count > 0;
		}
		return false;
	}

	private bool ShouldDisplayResourceInEditor(ResourceType resourceType)
	{
		return The.Sim.PlaySite.EditorResources.Contains(resourceType);
	}

	private void HandleResource(string checkboxTooltip, ResourceType resourceType, int noOfResourceContainers, Predicate<ResourceType> resourceExists)
	{
		Grid grid = null;
		ResourceCategory category = resourceType.Category;
		UIComponent item;
		if (noOfResourceContainers > 0)
		{
			CollapsablePanel cpCategory = null;
			grid = null;
			_ = resourceType.ResourceItemType;
			if (grid == null)
			{
				if (categoryGrid.TryGetEntry(category, out item))
				{
					cpCategory = item as CollapsablePanel;
					grid = (Grid)cpCategory.ExpandedPanel.Controls[0];
				}
				else
				{
					AddResourceCategoryRow(ref cpCategory, ref grid, category);
					UpdateCategoryRow(cpCategory, The.InGameUI.OverlaySettings.ResourceCategoriesToDisplay[category]);
					grid.BeginAddingEntries();
					childGridsThatWereChanged.Add(grid);
				}
			}
			if (!grid.TryGetEntry(resourceType, out var item2))
			{
				item2 = AddResourceTypeRow(grid, resourceType, category, checkboxTooltip);
			}
			bool selected = The.InGameUI.OverlaySettings.DisplayResourceType(resourceType);
			UpdateResourceTypeRow(item2, selected, cpCategory);
		}
		if (resourceExists(resourceType))
		{
			return;
		}
		if (grid == null && categoryGrid.TryGetEntry(category, out item))
		{
			CollapsablePanel cpCategory = item as CollapsablePanel;
			grid = (Grid)cpCategory.ExpandedPanel.Controls[0];
			grid.BeginAddingEntries();
			childGridsThatWereChanged.Add(grid);
		}
		if (grid == null)
		{
			return;
		}
		grid.TryRemoveEntry(resourceType);
		if (grid.Entries.Count == 0)
		{
			categoryGrid.TryRemoveEntry(category);
			if (grid != null)
			{
				childGridsThatWereChanged.Remove(grid);
			}
		}
	}

	private void AddAndUpdateEntityGroupingAndRow(EntityType entityType, string checkboxTooltip, out Grid grdGrouping, EntityGrouping grouping)
	{
		CollapsablePanel cpGrouping = null;
		grdGrouping = null;
		if (grdGrouping == null)
		{
			if (categoryGrid.TryGetEntry(grouping, out var item))
			{
				cpGrouping = item as CollapsablePanel;
				grdGrouping = (Grid)cpGrouping.ExpandedPanel.Controls[0];
			}
			else
			{
				AddEntityGroupingRow(ref cpGrouping, ref grdGrouping, grouping, OverlaySettings.GetName(grouping));
				UpdateCategoryRow(cpGrouping, The.InGameUI.OverlaySettings.EntityTypeGroupingsToDisplay[grouping]);
				grdGrouping.BeginAddingEntries();
				childGridsThatWereChanged.Add(grdGrouping);
			}
		}
		if (!grdGrouping.TryGetEntry(entityType, out var item2))
		{
			if (checkboxTooltip == "")
			{
				if (grouping.Equals(EntityGrouping.Animals))
				{
					checkboxTooltip = "Show animal";
				}
				else if (grouping.Equals(EntityGrouping.Interest))
				{
					checkboxTooltip = "Show places of interest";
				}
			}
			item2 = AddEntityTypeRow(grdGrouping, entityType, grouping, checkboxTooltip);
		}
		UpdateEntityTypeRow(item2, The.InGameUI.OverlaySettings.EntityTypesToDisplay[entityType], cpGrouping);
	}

	private int GetCollapsablePanelWidth()
	{
		return categoryGrid.Width - 16;
	}

	private void CreateCategoryRow(ref CollapsablePanel cp, ref Grid grdChild, object key, string title, Color color, ClickHandler clickHandler)
	{
		cp = new CollapsablePanel(gui, CollapsablePanel.PanelType.HUDSmall);
		cp.CollapsedHeight = categoryGrid.ItemHeight;
		cp.Init();
		cp.Title = title;
		cp.TitleSummaryRightAlignXPos = 10;
		cp.TitlePositionX = 10;
		cp.ToolTip = "Click to see more detailed options";
		cp.CollapsablePanelRightPadding = 1;
		categoryGrid.AddEntry(key, cp);
		grdChild = new Grid(gui, ListBoxType.HUDAndLCD, Label.LabelType.HUDWindow);
		grdChild.FixedItemHeights = true;
		grdChild.Width = 210;
		grdChild.VMargin = 4;
		grdChild.ScrollBarEnabled = false;
		grdChild.ItemHeight = 25;
		grdChild.CanGrowInHeight = true;
		grdChild.Font = GUIManager.LCDandHUDBodyFontPath;
		grdChild.IsOuterGrid = false;
		grdChild.DebugTag = "grdChild";
		cp.AddContent(grdChild);
		if (key is ResourceCategory)
		{
			Image image = new Image(gui);
			image.ToolTip = "When activating the SCAN button, these resources will also be visible in the terrain view.";
			image.SetSkinLocation(SkinState.Normal, gui.GUISpriteSheet.GetSourceRectangle("HUD_icon_scan"));
			image.ResizeControlToFitImage();
			image.X = 10;
			image.ID = UIComponent.DataControlID.StatusIcon;
			image.Visible = true;
			cp.CenterOnHeader(image);
			cp.TitlePositionX = image.Right + 6 - 2;
			cp.Add(image);
		}
		CheckBox checkBox = new CheckBox(gui);
		checkBox.Init(CheckBoxType.HUDCheckBox);
		checkBox.ToolTip = "Select all / Deselect all";
		checkBox.ID = UIComponent.DataControlID.Selector;
		checkBox.X = 188;
		checkBox.Y = 4;
		checkBox.Visible = true;
		checkBox.IsChecked = false;
		checkBox.Tag1 = key;
		checkBox.Click += clickHandler;
		checkBox.SetNormalColor(color);
		cp.Add(checkBox);
		checkBox.DebugTag = "cbOverlaySelect";
		Image image2 = new Image(gui);
		image2.ToolTip = "Some items in the category have overriding settings.";
		image2.SetSkinLocation(SkinState.Normal, gui.GUISpriteSheet.GetSourceRectangle("HUD_exclamationmark_parenthesis"));
		image2.ResizeControlToFitImage();
		image2.X = checkBox.X - 20;
		image2.ID = UIComponent.DataControlID.HasOverridingItemIcon;
		image2.Visible = false;
		image2.Color = color;
		cp.CenterOnHeader(image2);
		cp.Add(image2);
	}

	private void AddResourceCategoryRow(ref CollapsablePanel cpCategory, ref Grid categoryGrid, ResourceCategory resourceCategory)
	{
		Color value = resourceCategory.Color.Value;
		CreateCategoryRow(ref cpCategory, ref categoryGrid, resourceCategory, resourceCategory.Name, value, cbSelectDeselectResourceCategory_Click);
	}

	private void AddEntityGroupingRow(ref CollapsablePanel cpGrouping, ref Grid groupingGrid, EntityGrouping entityGrouping, string title)
	{
		Color value = OverlaySettings.GetGroupingColor(entityGrouping, faded: false).Value;
		CreateCategoryRow(ref cpGrouping, ref groupingGrid, entityGrouping, title, value, cbSelectDeselectEntityGrouping_Click);
	}

	private UIComponent AddResourceTypeRow(Grid grid, ResourceType resourceType, object resourceCategory, string checkboxTooltip)
	{
		ResourceTypeButtonEventArgs eventArgs = new ResourceTypeButtonEventArgs(resourceType);
		UIComponent uIComponent = CreateEntityTypeRow(resourceType, null, checkboxTooltip, eventArgs, DisplayWindow.Width, resourceCategory);
		grid.AddEntry(resourceType, uIComponent);
		return uIComponent;
	}

	private UIComponent AddEntityTypeRow(Grid grid, EntityType entityType, EntityGrouping grouping, string checkboxTooltip)
	{
		ItemTypeButtonEventArgs eventArgs = new ItemTypeButtonEventArgs(entityType);
		UIComponent uIComponent = CreateEntityTypeRow(null, entityType, checkboxTooltip, eventArgs, DisplayWindow.Width, grouping);
		grid.AddEntry(entityType, uIComponent);
		return uIComponent;
	}

	private void UpdateEntityTypeRow(UIComponent itemRow, bool selected, CollapsablePanel cpCategory)
	{
		CheckBox obj = (CheckBox)itemRow.FindChildById(UIComponent.DataControlID.Selector);
		EntityGrouping value = OverlaySettings.GetGrouping((EntityType)obj.Tag1).Value;
		obj.IsChecked = selected;
		UpdateOverridingSettingIcon(cpCategory, value);
	}

	private void UpdateResourceTypeRow(UIComponent itemRow, bool selected, CollapsablePanel cpCategory)
	{
		CheckBox obj = (CheckBox)itemRow.FindChildById(UIComponent.DataControlID.Selector);
		ResourceCategory category = ((ResourceType)obj.Tag1).Category;
		obj.IsChecked = selected;
		UpdateOverridingSettingIcon(cpCategory, category);
	}

	private void UpdateCategoryRow(CollapsablePanel cpCategory, bool selected)
	{
		CheckBox checkBox = (CheckBox)cpCategory.FindChildById(UIComponent.DataControlID.Selector);
		checkBox.IsChecked = selected;
		UpdateOverridingSettingIcon(cpCategory, checkBox.Tag1);
	}

	private void UpdateSingleItemRowEditor(UIComponent row)
	{
		row.FindChildById<CheckBox>(UIComponent.DataControlID.Selector, out var child, firstLevelOnly: false);
		child.IsChecked = The.InGameUI.OverlaySettings.EditorOverlayTypeSettings[(EditorOverlayTypes)row.Tag1];
	}

	private void UpdateSingleItemRow(UIComponent row)
	{
		row.FindChildById<CheckBox>(UIComponent.DataControlID.Selector, out var child, firstLevelOnly: false);
		child.IsChecked = The.InGameUI.OverlaySettings.OverlayTypeSettings[(OverlayTypes)row.Tag1];
	}

	private UIComponent AddSingleItemRow(object key, string text, string captionTooltip, string checkboxTooltip, string spriteName, Color? color, ClickHandler clickHandler)
	{
		UIComponent uIComponent = new UIComponent(gui);
		grdSingleItems.AddEntry(key, uIComponent);
		if (spriteName != null)
		{
			Rectangle sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle(spriteName);
			Image image = new Image(gui);
			uIComponent.Add(image);
			image.Texture = gui.GUISpriteSheet.Texture;
			image.SetSkinLocation(SkinState.Normal, sourceRectangle);
			image.ResizeControlToFitImage();
			image.X = 2;
			uIComponent.AlignVertically(image);
		}
		Label label = new Label(gui);
		uIComponent.Add(label);
		label.Init(Label.LabelType.HUDWindow);
		label.X = 25;
		label.Text = text;
		label.FitToText();
		uIComponent.AlignVertically(label);
		label.ToolTip = captionTooltip;
		CheckBox checkBox = new CheckBox(gui);
		checkBox.Init(CheckBoxType.HUDCheckBox);
		checkBox.ID = UIComponent.DataControlID.Selector;
		checkBox.ToolTip = checkboxTooltip;
		uIComponent.AlignVertically(checkBox);
		checkBox.X = 182;
		checkBox.ToolTip = checkboxTooltip;
		checkBox.IsChecked = false;
		checkBox.Tag1 = key;
		checkBox.Click += clickHandler;
		checkBox.SetNormalColor(color);
		uIComponent.Add(checkBox);
		return uIComponent;
	}

	private UIComponent CreateEntityTypeRow(ResourceType resourceType, EntityType entityType, string tooltip, EventArgs eventArgs, int menuWidth, object key)
	{
		UIComponent uIComponent = new UIComponent(gui);
		EntityType entityType2 = entityType ?? resourceType.ResourceItemType;
		IconInfo iconInfo;
		Rectangle iconSprite = entityType2.GetIconSprite(out iconInfo);
		Image image = new Image(gui);
		uIComponent.Add(image);
		image.Texture = gui.GUISpriteSheet.Texture;
		image.ResizeControlToFitImage();
		image.X = 2;
		image.SetSkinLocation(SkinState.Normal, iconSprite);
		DataTypeButton dataTypeButton = new DataTypeButton(gui, DataSheet.InfoToShow.Production, entityType2, null, useUIOwner: true);
		dataTypeButton.Text = entityType2.Name;
		dataTypeButton.Init(TextButton.TextButtonType.HUDToolTipWhite);
		dataTypeButton.ID = UIComponent.DataControlID.Caption;
		dataTypeButton.IsRoot = true;
		dataTypeButton.TextAlignment = TextButton.TextAlign.Left;
		dataTypeButton.X = 24;
		dataTypeButton.Width = 160;
		CheckBox checkBox = new CheckBox(gui);
		checkBox.Init(CheckBoxType.HUDCheckBox);
		checkBox.ID = UIComponent.DataControlID.Selector;
		checkBox.ToolTip = tooltip;
		checkBox.EventArgs = eventArgs;
		checkBox.Y = image.Y + 3;
		checkBox.Height = 1;
		checkBox.X = dataTypeButton.X + dataTypeButton.Width;
		checkBox.IsChecked = false;
		Color? color = null;
		if (resourceType != null)
		{
			checkBox.Tag1 = resourceType;
			checkBox.Click += cbSelectResourceType_Click;
			color = resourceType.Category.Color;
		}
		else
		{
			checkBox.Tag1 = entityType;
			checkBox.Click += cbSelectEntityType_Click;
			color = OverlaySettings.GetGroupingColorFromEntityType(entityType);
		}
		checkBox.SetNormalColor(color);
		uIComponent.Add(checkBox);
		uIComponent.Add(dataTypeButton);
		return uIComponent;
	}

	private static void SelectDeselectEntityTypesInGrouping(CollapsablePanel cpGrouping, bool selectAll)
	{
		Grid obj = (Grid)cpGrouping.ExpandedPanel.Controls[0];
		The.InGameUI.OverlaySettings.EntityTypeGroupingsToDisplay[(EntityGrouping)cpGrouping.Tag1] = selectAll;
		foreach (UIComponent entry in obj.Entries)
		{
			EntityType entityType = (EntityType)entry.Tag1;
			((CheckBox)entry.FindChildById(UIComponent.DataControlID.Selector)).IsChecked = selectAll;
			SelectDeselectEntityType(selectAll, entityType);
		}
	}

	private static void SelectDeselectResourceTypes(CollapsablePanel cpCategory, bool selectAll)
	{
		Grid obj = (Grid)cpCategory.ExpandedPanel.Controls[0];
		The.InGameUI.OverlaySettings.ResourceCategoriesToDisplay[(ResourceCategory)cpCategory.Tag1] = selectAll;
		foreach (UIComponent entry in obj.Entries)
		{
			ResourceType resourceType = (ResourceType)entry.Tag1;
			((CheckBox)entry.FindChildById(UIComponent.DataControlID.Selector)).IsChecked = selectAll;
			SelectDeselectResourceType(selectAll, resourceType);
		}
	}

	private static void SelectDeselectResourceType(bool select, ResourceType resourceType)
	{
		The.InGameUI.OverlaySettings.ResourceTypesToDisplay[resourceType] = select;
	}

	private static void SelectDeselectEntityType(bool select, EntityType entityType)
	{
		The.InGameUI.OverlaySettings.EntityTypesToDisplay[entityType] = select;
	}

	private void cbSelectDeselectEntityGrouping_Click(UIComponent sender, EventArgs e)
	{
		CheckBox checkBox = sender as CheckBox;
		CollapsablePanel collapsablePanel = (CollapsablePanel)checkBox.Parent;
		SelectDeselectEntityTypesInGrouping(collapsablePanel, checkBox.IsChecked);
		UpdateOverridingSettingIcon(collapsablePanel, checkBox.Tag1);
		The.InGameUI.Minimap.SetSettingsDirty();
	}

	private void cbSelectDeselectResourceCategory_Click(UIComponent sender, EventArgs e)
	{
		CheckBox checkBox = sender as CheckBox;
		CollapsablePanel cpCategory = (CollapsablePanel)checkBox.Parent;
		SelectDeselectResourceTypes(cpCategory, checkBox.IsChecked);
		UpdateOverridingSettingIcon(cpCategory, checkBox.Tag1);
		The.InGameUI.Minimap.SetSettingsDirty();
	}

	private void cbSelectResourceType_Click(UIComponent sender, EventArgs e)
	{
		CheckBox obj = (CheckBox)sender;
		ResourceType resourceType = (ResourceType)obj.Parent.Tag1;
		ResourceCategory category = resourceType.Category;
		CollapsablePanel cpCategory = (CollapsablePanel)categoryGrid.EntriesByKey[category];
		SelectDeselectResourceType(obj.IsChecked, resourceType);
		UpdateOverridingSettingIcon(cpCategory, category);
		The.InGameUI.Minimap.SetSettingsDirty();
	}

	private void cbSelectEntityType_Click(UIComponent sender, EventArgs e)
	{
		CheckBox obj = (CheckBox)sender;
		EntityType entityType = (EntityType)obj.Tag1;
		EntityGrouping value = OverlaySettings.GetGrouping(entityType).Value;
		CollapsablePanel cpCategory = (CollapsablePanel)categoryGrid.EntriesByKey[value];
		SelectDeselectEntityType(obj.IsChecked, entityType);
		UpdateOverridingSettingIcon(cpCategory, value);
		The.InGameUI.Minimap.SetSettingsDirty();
	}

	private void cbSelectEditorOverlayType_Click(UIComponent sender, EventArgs e)
	{
		CheckBox checkBox = (CheckBox)sender;
		EditorOverlayTypes editorOverlayTypes = (EditorOverlayTypes)sender.Tag1;
		The.InGameUI.OverlaySettings.EditorOverlayTypeSettings[editorOverlayTypes] = checkBox.IsChecked;
		if (editorOverlayTypes == EditorOverlayTypes.TerrainDivision && checkBox.IsChecked)
		{
			The.Client.Renderer.terrainSlicedMap.RedrawMap(The.Client.Renderer.DiffuseMSRenderTarget);
		}
	}

	private void cbSelectOverlayType_Click(UIComponent sender, EventArgs e)
	{
		CheckBox checkBox = (CheckBox)sender;
		OverlayTypes overlayTypes = (OverlayTypes)sender.Tag1;
		The.InGameUI.OverlaySettings.OverlayTypeSettings[overlayTypes] = checkBox.IsChecked;
		if (overlayTypes == OverlayTypes.ColonyMembers)
		{
			The.InGameUI.OverlaySettings.EntityTypeGroupingsToDisplay[EntityGrouping.ColonyMembers] = checkBox.IsChecked;
			The.InGameUI.Minimap.SetSettingsDirty();
		}
	}

	private bool OneOrMoreItemsInCategoryAreDifferent(CollapsablePanel cp, object category, out bool hiddenItemsAreDifferent)
	{
		hiddenItemsAreDifferent = false;
		CheckBox cBox = (CheckBox)cp.FindChildById(UIComponent.DataControlID.Selector);
		if (((Grid)cp.ExpandedPanel.Controls[0]).Entries.Exists((UIComponent c) => ((CheckBox)c.FindChildById(UIComponent.DataControlID.Selector)).IsChecked != cBox.IsChecked))
		{
			return true;
		}
		if (category is ResourceCategory category2)
		{
			if (The.InGameUI.OverlaySettings.ResourceCategoryHasDifferentItemSetting(cBox.IsChecked, category2))
			{
				hiddenItemsAreDifferent = true;
				return true;
			}
		}
		else if (The.InGameUI.OverlaySettings.EntityCategoryHasDifferentItemSetting(cBox.IsChecked, (EntityGrouping)category))
		{
			hiddenItemsAreDifferent = true;
			return true;
		}
		return false;
	}

	private void UpdateOverridingSettingIcon(CollapsablePanel cpCategory, object category)
	{
		Image image = (Image)cpCategory.FindChildById(UIComponent.DataControlID.HasOverridingItemIcon);
		if (OneOrMoreItemsInCategoryAreDifferent(cpCategory, category, out var hiddenItemsAreDifferent))
		{
			image.Visible = true;
			if (hiddenItemsAreDifferent)
			{
				image.ToolTip = "Some hidden item types have overriding settings that are different.";
			}
			else
			{
				image.ToolTip = "Some item types have overriding settings that are different.";
			}
		}
		else
		{
			image.Visible = false;
		}
	}
}
