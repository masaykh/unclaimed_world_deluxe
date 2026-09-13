using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface.Controls;
using UWGame.ClientSide.Interface.HUD_Windows;
using UWGame.SimSide;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Resources;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

public class SidePanelMapArea : RosterPanel
{
	private FullLCDPanel.SetCollapsedSummary SetSummaryDelegate;

	private Grid outerGrid;

	private CollapsablePanel cpPersons;

	private CollapsablePanel cpRobots;

	private CollapsablePanel cpAnimals;

	private CollapsablePanel cpResources;

	private CollapsablePanel cpItems;

	private CollapsablePanel cpTrees;

	private CollapsablePanel cpStructures;

	private CollapsablePanel cpTerrain;

	private Grid grdPersons;

	private Grid grdRobots;

	private Grid grdAnimals;

	private Grid grdResources;

	private Grid grdItems;

	private Grid grdTrees;

	private Grid grdStructures;

	private Grid grdTerrain;

	public const int GridMargin = 2;

	private const bool useUIOWner = true;

	private Dictionary<ResourceType, MapArea.ResourcesAndJobs> data = new Dictionary<ResourceType, MapArea.ResourcesAndJobs>();

	private Dictionary<EntityType, int> allItems = new Dictionary<EntityType, int>();

	private List<IKnownEntityData> entities = new List<IKnownEntityData>();

	private Dictionary<EntityType, List<EntityID>> allItemByType = new Dictionary<EntityType, List<EntityID>>();

	private EntityType entityType;

	public SidePanelMapArea()
		: base(The.InGameUI.sidePanelHeight, isInfoPanel: true)
	{
		SetSummaryDelegate = SetSummaryAsTotal;
		InitStatusContentPanel();
		int value = 20;
		outerGrid = RosterPanel.CreateOuterGridForCollapsableLists(The.InGameUI.gui, lcdSurface);
		RosterPanel.AddCollapsablePanelAndGrid(The.InGameUI.gui, outerGrid, "PERSONS", value, out cpPersons, out grdPersons, Grid.SelectabilityOptions.None);
		RosterPanel.AddCollapsablePanelAndGrid(The.InGameUI.gui, outerGrid, "ROBOTS", value, out cpRobots, out grdRobots, Grid.SelectabilityOptions.None, 5);
		RosterPanel.AddCollapsablePanelAndGrid(The.InGameUI.gui, outerGrid, "ANIMALS", value, out cpAnimals, out grdAnimals, Grid.SelectabilityOptions.None, 10);
		RosterPanel.AddCollapsablePanelAndGrid(The.InGameUI.gui, outerGrid, "STRUCTURES", value, out cpStructures, out grdStructures, Grid.SelectabilityOptions.None, 15);
		RosterPanel.AddCollapsablePanelAndGrid(The.InGameUI.gui, outerGrid, "ITEMS", value, out cpItems, out grdItems, Grid.SelectabilityOptions.None, 20);
		RosterPanel.AddCollapsablePanelAndGrid(The.InGameUI.gui, outerGrid, "RESOURCES", value, out cpResources, out grdResources, Grid.SelectabilityOptions.None, 25);
		RosterPanel.AddCollapsablePanelAndGrid(The.InGameUI.gui, outerGrid, "PLANTS", value, out cpTrees, out grdTrees, Grid.SelectabilityOptions.None, 35);
		if (The.Sim.Mode == Sim.EngineMode.Edit)
		{
			RosterPanel.AddCollapsablePanelAndGrid(The.InGameUI.gui, outerGrid, "TERRAIN", value, out cpTerrain, out grdTerrain, Grid.SelectabilityOptions.None, 45);
		}
	}

	private void InitStatusContentPanel()
	{
		statusContent = The.InGameUI.StatusScreen.GetNewSurfaceContent();
		_ = The.Sim.Controller.Game;
		GUIManager gui = The.InGameUI.gui;
		imStatusBackground = new Image(gui);
		statusContent.Add(imStatusBackground);
		RosterPanel.InitStatusCRTHeader(gui, statusContent, 8, out lblStatusHeading, out crtUnderline);
	}

	private void RefreshStatusScreen()
	{
		lblStatusHeading.Text = "";
		if (TileSelectionContextMenu.GetMapArea() != null)
		{
			SetHeaderText("LAND");
		}
	}

	private void ShowBackgroundImage()
	{
		statusContent.Insert(imStatusBackground, 1);
	}

	public override void Show()
	{
		base.Show();
	}

	public override void Hide()
	{
		base.Hide();
	}

	public override void Refresh()
	{
		RefreshStatusScreen();
		Populate();
	}

	private void Populate()
	{
		outerGrid.BeginAddingEntries();
		PopulatePersons();
		PopulateRobots();
		PopulateAnimals();
		PopulateItems();
		PopulateStructures();
		PopulateResources();
		PopulateTrees();
		if (The.Sim.Mode == Sim.EngineMode.Edit)
		{
			PopulateTerrain();
		}
		outerGrid.Sort((UIComponent u) => u.OrderByTag1, Grid.Sorting.Ascending);
		outerGrid.EndAddingEntries();
	}

	private void PopulateResources()
	{
		data.Clear();
		TileSelectionContextMenu.GetMapArea().GetSumOfAllResourcesInArea(The.InGameUI.UIAllegiance.SharedKnowledge, data);
		int num = 0;
		grdResources.BeginAddingEntries();
		if (RosterPanel.AddPanelIfNotPresent(data.Count > 0, outerGrid, cpResources))
		{
			foreach (KeyValuePair<ResourceType, MapArea.ResourcesAndJobs> datum in data)
			{
				int numberOfResources = datum.Value.NumberOfResources;
				num += numberOfResources;
				if (grdResources.TryGetEntry(datum.Key, out var item))
				{
					UIComponent uIComponent = item.FindChildById(UIComponent.DataControlID.Stock);
					if (uIComponent != null)
					{
						((Label)uIComponent).Text = numberOfResources.ToString();
					}
				}
				else
				{
					AddResourceRow(datum.Key, numberOfResources);
				}
			}
		}
		cpResources.Summary = num.ToString();
		grdResources.DeleteEntries((ResourceType e) => data.ContainsKey(e));
		grdResources.EndAddingEntries();
	}

	private void PopulateAnimals()
	{
		MapArea mapArea = TileSelectionContextMenu.GetMapArea();
		entities.Clear();
		mapArea.GetEntitiesInArea(entities, null, (IKnownEntityData e) => e.EntityType.Person == null && e.EntityType.IntelligenceType != null && e.EntityType.BiologicalType != null, The.InGameUI.UIAllegiance);
		grdAnimals.BeginAddingEntries();
		if (RosterPanel.AddPanelIfNotPresent(entities.Count > 0, outerGrid, cpAnimals))
		{
			foreach (IKnownEntityData entity in entities)
			{
				if (!grdAnimals.EntriesByKey.ContainsKey(entity.EntityID))
				{
					grdAnimals.AddHyperLinkEntry(entity.EntityID, entity.EntityType.Name, (uint)entity.EntityID, 6);
				}
			}
			cpAnimals.Summary = entities.Count.ToString();
		}
		grdAnimals.DeleteEntries((EntityID e) => entities.Exists((IKnownEntityData knownEntityData) => knownEntityData.EntityID == e));
		grdAnimals.EndAddingEntries();
	}

	private void PopulateStructures()
	{
		MapArea mapArea = TileSelectionContextMenu.GetMapArea();
		entities.Clear();
		mapArea.GetEntitiesInArea(entities, null, (IKnownEntityData e) => e.EntityType.StructureType != null, The.InGameUI.UIAllegiance);
		grdStructures.BeginAddingEntries();
		if (RosterPanel.AddPanelIfNotPresent(entities.Count > 0, outerGrid, cpStructures))
		{
			foreach (IKnownEntityData entity in entities)
			{
				if (!grdStructures.EntriesByKey.ContainsKey(entity.EntityID))
				{
					grdStructures.AddHyperLinkEntry(entity.EntityID, entity.EntityType.Name, (uint)entity.EntityID, 6);
				}
			}
			cpStructures.Summary = entities.Count.ToString();
		}
		grdStructures.DeleteEntries((EntityID e) => entities.Exists((IKnownEntityData knownEntityData) => knownEntityData.EntityID == e));
		grdStructures.EndAddingEntries();
	}

	private void PopulateItems()
	{
		MapArea mapArea = TileSelectionContextMenu.GetMapArea();
		allItemByType.Clear();
		mapArea.GetNoOfItemsInAreaByType(allItemByType, (IKnownEntityData e) => e.EntityType.ItemType != null, The.InGameUI.UIAllegiance);
		if (!RosterPanel.AddPanelIfNotPresent(allItemByType.Count > 0, outerGrid, cpItems))
		{
			return;
		}
		OwnerID? uIOwnerID = The.InGameUI.GetUIOwnerID();
		int num = 0;
		grdItems.BeginAddingEntries();
		foreach (KeyValuePair<EntityType, List<EntityID>> item2 in allItemByType)
		{
			EntityType key = item2.Key;
			if (key.ItemType != null)
			{
				num += item2.Value.Count;
				if (!grdItems.TryGetEntry(key, out var item))
				{
					AddItemRow(grdItems, key, null, useCurrentUIOwner: true, tbItems_Click);
					grdItems.TryGetEntry(key, out item);
				}
				List<EntityID> listOfItems = GetListOfItems(key);
				UpdateItemRow(uIOwnerID, item, key, listOfItems);
			}
			grdItems.DeleteEntries((object e) => !(e is EntityType) || allItemByType.ContainsKey((EntityType)e));
			grdItems.DeleteEntries((object e) => !(e is EntityCategory) || CategoryIsRepresented((EntityCategory)e, allItemByType));
			grdItems.EndAddingEntries();
			cpItems.Summary = num.ToString();
		}
	}

	public static bool CategoryIsRepresented(EntityCategory category, Dictionary<EntityType, List<EntityID>> allEntities)
	{
		foreach (KeyValuePair<EntityType, List<EntityID>> allEntity in allEntities)
		{
			if (allEntity.Key.Category == category && allEntity.Value.Count > 0)
			{
				return true;
			}
		}
		return false;
	}

	public static UIComponent AddItemRow(Grid grdItems, EntityType entityType, EntityGroup owner, bool useCurrentUIOwner, Action<UIComponent, EventArgs> clickMethodToSeeListOfItems)
	{
		UIComponent uIComponent = new UIComponent(The.InGameUI.gui);
		_ = entityType.Name == "Firewood";
		DataTypeButton dataTypeButton = new DataTypeButton(The.InGameUI.gui, DataSheet.InfoToShow.Data, entityType, GoalEvaluator.GetOwnerID(owner), useCurrentUIOwner);
		dataTypeButton.Init(TextButton.TextButtonType.LCDToolTipBlack);
		dataTypeButton.ID = UIComponent.DataControlID.Caption;
		dataTypeButton.IsRoot = true;
		uIComponent.Add(dataTypeButton);
		dataTypeButton.TextAlignment = TextButton.TextAlign.Left;
		dataTypeButton.Width = 160;
		dataTypeButton.X = 2;
		StockButton stockButton = new StockButton(The.InGameUI.gui, entityType);
		uIComponent.Add(stockButton);
		stockButton.ScaleWidthToFitText();
		stockButton.Position = new Point(dataTypeButton.Right + 6, 0);
		stockButton.Click += clickMethodToSeeListOfItems.Invoke;
		stockButton.ID = UIComponent.DataControlID.Stock;
		InsertItemAndCategory(grdItems, entityType, entityType.Category, entityType.Category.Name.ToUpper(Config.Culture), null, uIComponent, null, CreateCategoryHeaderRow);
		return uIComponent;
	}

	public static void InsertItemAndCategory(Grid grid, EntityType entityType, object categoryRowKey, string categoryName, int? categorySortOrder, UIComponent item, Func<Grid, int, int> findIndexOfNextCategoryRow, Func<string, int?, UIComponent> createCategoryHeaderRow)
	{
		if (grid.TryGetEntry(categoryRowKey, out var item2))
		{
			int num = grid.Entries.IndexOf(item2);
			while (true)
			{
				num++;
				if (num >= grid.Entries.Count)
				{
					break;
				}
				if (grid.Entries[num].Tag1 is EntityType entityType2)
				{
					if (string.CompareOrdinal(entityType2.Name, entityType.Name) > 0)
					{
						grid.AddEntry(entityType, item, num);
						return;
					}
					continue;
				}
				grid.AddEntry(entityType, item, num);
				return;
			}
			grid.AddEntry(entityType, item);
			return;
		}
		UIComponent item3 = createCategoryHeaderRow(categoryName, null);
		int num2;
		if (findIndexOfNextCategoryRow != null && categorySortOrder.HasValue)
		{
			num2 = findIndexOfNextCategoryRow(grid, categorySortOrder.Value);
			if (num2 == grid.Entries.Count)
			{
				grid.AddEntry(categoryRowKey, item3);
				return;
			}
			grid.AddEntry(categoryRowKey, item3, num2);
		}
		else
		{
			grid.AddEntry(categoryRowKey, item3);
			num2 = grid.Entries.Count - 1;
		}
		grid.AddEntry(entityType, item, num2 + 1);
	}

	private static UIComponent AddEntityTypeRow(Grid grid, EntityType entityType, int amount)
	{
		UIComponent uIComponent = new UIComponent(The.InGameUI.gui);
		DataTypeButton dataTypeButton = new DataTypeButton(The.InGameUI.gui, DataSheet.InfoToShow.Data, entityType, null, useUIOwner: true);
		dataTypeButton.Init(TextButton.TextButtonType.LCDToolTipBlack);
		dataTypeButton.ID = UIComponent.DataControlID.Caption;
		dataTypeButton.IsRoot = true;
		dataTypeButton.Text = entityType.Name;
		uIComponent.Add(dataTypeButton);
		dataTypeButton.TextAlignment = TextButton.TextAlign.Left;
		dataTypeButton.Width = 160;
		dataTypeButton.X = 2;
		Label label = new Label(The.InGameUI.gui);
		uIComponent.Add(label);
		label.Init(Label.LabelType.LCDNormal);
		label.Text = amount.ToString();
		label.Position = new Point(dataTypeButton.Right + 6, 0);
		label.ID = UIComponent.DataControlID.Stock;
		grid.AddEntry(entityType, uIComponent);
		return uIComponent;
	}

	private void PopulateTrees()
	{
		MapArea mapArea = TileSelectionContextMenu.GetMapArea();
		allItems.Clear();
		mapArea.GetNoOfEntitiesInArea(allItems, (IKnownEntityData e) => e.EntityType.TreeType != null, The.InGameUI.UIAllegiance);
		if (!RosterPanel.AddPanelIfNotPresent(allItems.Count > 0, outerGrid, cpTrees))
		{
			return;
		}
		int num = 0;
		grdTrees.BeginAddingEntries();
		foreach (KeyValuePair<EntityType, int> allItem in allItems)
		{
			EntityType key = allItem.Key;
			num += allItem.Value;
			if (!grdTrees.TryGetEntry(key, out var item))
			{
				AddEntityTypeRow(grdTrees, key, allItem.Value);
				grdTrees.TryGetEntry(key, out item);
			}
			UpdateEntityTypeRow(item, key, allItem.Value);
		}
		grdTrees.DeleteEntries((object e) => !(e is EntityType) || allItems.ContainsKey((EntityType)e));
		grdTrees.EndAddingEntries();
		cpTrees.Summary = num.ToString();
	}

	private void PopulateTerrain()
	{
		MapArea mapArea = TileSelectionContextMenu.GetMapArea();
		allItems.Clear();
		mapArea.GetNoOfEntitiesInArea(allItems, (IKnownEntityData e) => e.EntityType.TerrainType != null, The.InGameUI.UIAllegiance);
		if (!RosterPanel.AddPanelIfNotPresent(allItems.Count > 0, outerGrid, cpTerrain))
		{
			return;
		}
		int num = 0;
		grdTerrain.BeginAddingEntries();
		foreach (KeyValuePair<EntityType, int> allItem in allItems)
		{
			EntityType key = allItem.Key;
			num += allItem.Value;
			if (!grdTerrain.TryGetEntry(key, out var item))
			{
				item = AddEntityTypeRow(grdTerrain, key, allItem.Value);
			}
			UpdateEntityTypeRow(item, key, allItem.Value);
		}
		grdTerrain.DeleteEntries((object e) => !(e is EntityType) || allItems.ContainsKey((EntityType)e));
		grdTerrain.EndAddingEntries();
		cpTerrain.Summary = num.ToString();
	}

	private void AddResourceRow(ResourceType resourceType, int amount)
	{
		UIComponent uIComponent = new UIComponent(The.InGameUI.gui);
		Label label = new Label(Interface.gui);
		uIComponent.Add(label);
		label.Init(Label.LabelType.LCDNormal);
		label.Text = resourceType.Name;
		label.X = 2;
		label.ID = UIComponent.DataControlID.Caption;
		Label label2 = new Label(Interface.gui);
		uIComponent.Add(label2);
		label2.Init(Label.LabelType.LCDNormal);
		label2.Text = amount.ToString();
		label2.Position = new Point(label.Right + 6, 0);
		label2.ID = UIComponent.DataControlID.Stock;
		grdResources.AddEntry(resourceType, uIComponent);
	}

	private static UIComponent CreateCategoryHeaderRow(string categoryName, int? total)
	{
		UIComponent uIComponent = new UIComponent(The.InGameUI.gui);
		Label label = new Label(The.InGameUI.gui);
		uIComponent.Add(label);
		label.Init(Label.LabelType.LCDNormalDark);
		label.Y = 5;
		label.X = 7;
		label.Text = categoryName;
		label.FitToText();
		return uIComponent;
	}

	public static void UpdateItemRow(OwnerID? thisOwner, UIComponent itemRow, EntityType entityType, List<EntityID> itemsOfType)
	{
		int noOfIncompleteEntities = 0;
		int noOfAvailableEntities = 0;
		int noOfAvailableEntitiesIncludingIntrinsic = 0;
		int noOfEntitiesUsedAsParts = 0;
		int noOfItemsOnOtherSite = 0;
		int noOfItemsOwnedByOthers = 0;
		List<EntityID> listOfAvailableEntities = null;
		List<EntityID> listOfUnavailableEntities = null;
		foreach (EntityID item in itemsOfType)
		{
			Entity entity = Entity.FindByID(item);
			if (entity != null)
			{
				EntityGroup.CountEntity(thisOwner, entity, ref noOfIncompleteEntities, ref noOfEntitiesUsedAsParts, ref noOfItemsOnOtherSite, ref noOfItemsOwnedByOthers, ref noOfAvailableEntities, ref noOfAvailableEntitiesIncludingIntrinsic, ref listOfAvailableEntities, ref listOfUnavailableEntities);
			}
		}
		((StockButton)itemRow.FindChildById(UIComponent.DataControlID.Stock)).UpdateStockButton(noOfAvailableEntities, noOfIncompleteEntities, noOfEntitiesUsedAsParts, noOfItemsOnOtherSite, noOfItemsOwnedByOthers, itemsOfType, hideIfZero: false);
	}

	private static void UpdateEntityTypeRow(UIComponent itemRow, EntityType entityType, int amount)
	{
		((Label)itemRow.FindChildById(UIComponent.DataControlID.Stock)).Text = amount.ToString();
	}

	private void tbItems_Click(UIComponent sender, EventArgs e)
	{
		StockButton stockButton = sender as StockButton;
		The.InGameUI.EntityListWindow.SetDataSource(stockButton.EntityType, stockButton.EntityList);
		The.InGameUI.EntityListWindow.OpenNextToStockButton(sender);
	}

	private static List<EntityID> GetListOfItems(EntityType entityType)
	{
		MapArea mapArea = TileSelectionContextMenu.GetMapArea();
		_ = The.InGameUI.UIAllegiance.SharedKnowledge;
		List<EntityID> list = new List<EntityID>();
		mapArea.GetEntitiesInArea(null, list, (IKnownEntityData e) => e.EntityType == entityType, The.InGameUI.UIAllegiance);
		return list;
	}

	private void PopulatePersons()
	{
		MapArea mapArea = TileSelectionContextMenu.GetMapArea();
		entities.Clear();
		mapArea.GetEntitiesInArea(entities, null, (IKnownEntityData e) => e.EntityType.Person != null, The.InGameUI.UIAllegiance);
		grdPersons.BeginAddingEntries();
		if (RosterPanel.AddPanelIfNotPresent(entities.Count > 0, outerGrid, cpPersons))
		{
			foreach (IKnownEntityData entity in entities)
			{
				if (!grdPersons.EntriesByKey.ContainsKey(entity.EntityID))
				{
					grdPersons.AddHyperLinkEntry(entity.EntityID, entity.Name, (uint)entity.EntityID, 6);
				}
			}
			cpPersons.Summary = entities.Count.ToString();
		}
		grdPersons.DeleteEntries((EntityID e) => entities.Exists((IKnownEntityData knownEntityData) => knownEntityData.EntityID == e));
		grdPersons.EndAddingEntries();
	}

	private void PopulateRobots()
	{
		MapArea mapArea = TileSelectionContextMenu.GetMapArea();
		entities.Clear();
		mapArea.GetEntitiesInArea(entities, null, (IKnownEntityData e) => e.EntityType.IntelligenceType != null && e.EntityType.StructureType == null && e.EntityType.BiologicalType == null, The.InGameUI.UIAllegiance);
		grdRobots.BeginAddingEntries();
		if (RosterPanel.AddPanelIfNotPresent(entities.Count > 0, outerGrid, cpRobots))
		{
			foreach (IKnownEntityData entity in entities)
			{
				if (!grdRobots.EntriesByKey.ContainsKey(entity.EntityID))
				{
					grdRobots.AddHyperLinkEntry(entity.EntityID, entity.GetDisplayName(), (uint)entity.EntityID, 6);
				}
			}
			cpRobots.Summary = entities.Count.ToString();
		}
		grdRobots.DeleteEntries((EntityID e) => entities.Exists((IKnownEntityData knownEntityData) => knownEntityData.EntityID == e));
		grdRobots.EndAddingEntries();
	}

	private void AddEntityHyperlink(Entity entity, Grid grid)
	{
		if (entity.PersonEntity != null)
		{
			grid.AddHyperLinkEntry(null, entity.Name, (uint)entity.EntityID, 6);
		}
		else
		{
			grid.AddHyperLinkEntry(null, entity.EntityType.Name, (uint)entity.EntityID, 6);
		}
	}

	public static void SetSummaryAsTotal(CollapsablePanel cpCategory, object o)
	{
		string text = o.ToString();
		string summary = cpCategory.Summary;
		if (summary == "")
		{
			cpCategory.Summary = text;
		}
		else
		{
			cpCategory.Summary = (int.Parse(summary) + int.Parse(text)).ToString();
		}
	}
}
