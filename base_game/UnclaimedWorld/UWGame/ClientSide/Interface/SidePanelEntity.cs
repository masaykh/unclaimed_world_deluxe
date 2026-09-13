using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface.Controls;
using UWGame.ClientSide.Interface.HUD_Windows;
using UWGame.ClientSide.Interface.Inventory;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

public class SidePanelEntity : RosterPanel
{
	public Grid outerGrid;

	private CollapsablePanel cpCarrying;

	private Grid grdCarrying;

	private List<CategoryPanelAndData> customPanels = new List<CategoryPanelAndData>();

	private Label lblStatusInfo1;

	private Label lblStatusInfo2;

	private Label lblStatusInfo3;

	private Label lblStatusInfo4;

	private DataTypeButton selectedEntityTypeDataButton;

	private const int containsPanelSortOrder = 17;

	public SidePanelEntity()
		: base(The.InGameUI.sidePanelHeight, isInfoPanel: true)
	{
		InitStatusContentPanel();
		outerGrid = RosterPanel.CreateOuterGridForCollapsableLists(The.InGameUI.gui, lcdSurface);
		int value = 20;
		new UIComponent(The.InGameUI.gui);
		RosterPanel.AddCollapsablePanelAndGrid(The.InGameUI.gui, outerGrid, "ITEMS", value, out cpCarrying, out grdCarrying, Grid.SelectabilityOptions.None, 17);
		AddPresentationPanel(outerGrid, customPanels, GameData.Instance.CustomSidePanelData);
	}

	public static void AddPresentationPanel(Grid outerGrid, List<CategoryPanelAndData> customPanels, CustomDataPresentation presentationToUse)
	{
		foreach (PresentationTypeCategory finalPresentationTypeCategory in presentationToUse.FinalPresentationTypeCategories)
		{
			RosterPanel.AddCollapsablePanelAndGridNotFixed(The.InGameUI.gui, outerGrid, finalPresentationTypeCategory.Name, out var panel, out var grid, Grid.SelectabilityOptions.None);
			customPanels.Add(new CategoryPanelAndData(panel, grid, finalPresentationTypeCategory));
			panel.IsExpanded = finalPresentationTypeCategory.StartsAsExpanded;
			panel.OrderByTag1 = finalPresentationTypeCategory.PanelSortOrder;
		}
	}

	private void InitStatusContentPanel()
	{
		statusContent = The.InGameUI.StatusScreen.GetNewSurfaceContent();
		statusContent.DebugTag = "entityStatus";
		_ = The.Sim.Controller.Game;
		GUIManager gui = The.InGameUI.gui;
		imStatusBackground = new Image(gui);
		statusContent.Add(imStatusBackground);
		InitBillboardPanel();
		Rectangle sourceRectangle = gui.GUI_CRT_SpriteSheet.GetSourceRectangle("human_b_m_adult_1");
		imStatusBackground.Texture = gui.GUI_CRT_SpriteSheet.Texture;
		imStatusBackground.SetSkinLocation(SkinState.Normal, sourceRectangle);
		imStatusBackground.Position = new Point(statusContent.Width - sourceRectangle.Width, 2);
		imStatusBackground.ScaleImageToSizeOfControl = false;
		imStatusBackground.ResizeControlToFitImage();
		imStatusBackground.RenderType = RenderType.CRTAndLCD;
		imStatusBackground.Alpha = 1f;
		RosterPanel.InitStatusCRTHeader(gui, statusContent, 8, out lblStatusHeading, out crtUnderline);
		lblStatusInfo1 = new Label(gui);
		statusContent.Add(lblStatusInfo1);
		lblStatusInfo1.Position = new Point(8, 110);
		lblStatusInfo1.Text = "MALE";
		lblStatusInfo1.Init(Label.LabelType.CRTSmall);
		lblStatusInfo1.Width = 200;
		lblStatusInfo2 = new Label(gui);
		statusContent.Add(lblStatusInfo2);
		lblStatusInfo2.Position = new Point(8, 130);
		lblStatusInfo2.Text = "AGE: 34";
		lblStatusInfo2.Init(Label.LabelType.CRTSmall);
		lblStatusInfo2.Width = 200;
		lblStatusInfo3 = new Label(gui);
		statusContent.Add(lblStatusInfo3);
		lblStatusInfo3.Position = new Point(8, 165);
		lblStatusInfo3.Text = "MANUAL LABORER";
		lblStatusInfo3.Init(Label.LabelType.CRTSmall);
		lblStatusInfo3.NormalColor = Color.PowderBlue;
		lblStatusInfo3.Width = 200;
		lblStatusInfo4 = new Label(gui);
		statusContent.Add(lblStatusInfo4);
		lblStatusInfo4.Position = new Point(8, 185);
		lblStatusInfo4.Text = "WALKING";
		lblStatusInfo4.Init(Label.LabelType.CRTSmall);
		lblStatusInfo4.NormalColor = Color.PowderBlue;
		lblStatusInfo4.Width = 200;
	}

	public override void Show()
	{
		The.InGameUI.StatusScreen.ShowCenterButton(show: true);
		base.Show();
	}

	public override void Hide()
	{
		The.InGameUI.StatusScreen.ShowCenterButton(show: false);
		base.Hide();
	}

	private void ShowBackgroundImage()
	{
		statusContent.Insert(imStatusBackground, 1);
	}

	private void RefreshStatusScreen()
	{
		lblStatusInfo1.Text = "";
		lblStatusInfo2.Text = "";
		lblStatusInfo3.Text = "";
		lblStatusInfo4.Text = "";
		lblStatusHeading.Text = "";
		lblStatusHeading.DebugTag = "missingHeader";
		statusContent.Remove(imStatusBackground);
		statusContent.Remove(pnBillboards);
		RefreshEntityStatus();
		The.InGameUI.StatusScreen.Refresh();
	}

	protected void RefreshEntityStatus()
	{
		if (!intface.SelectedEntity.HasValue)
		{
			return;
		}
		The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(The.InGameUI.SelectedEntity.Value, out var data);
		Entity entity = data as Entity;
		if (data == null)
		{
			return;
		}
		RefreshEntityName(data);
		if (entity != null)
		{
			if (data.EntityType.Person != null)
			{
				RefreshPersonSpecificData(data, entity);
			}
			else if (entity.Structure != null)
			{
				RefreshStructureData(entity);
			}
			if (entity.Intelligence != null && entity.Intelligence.Brain != null)
			{
				RefreshTask(entity);
			}
		}
		else
		{
			RefreshMemoryFactExplanation();
		}
		if (data.EntityType.Person != null && entity != null)
		{
			RefreshPortraitImage(entity);
		}
		else if (data.EntityType.RenderableTypeMode != null && data.EntityType.RenderableTypeMode.DefaultClientState != null && data.EntityType.RenderableTypeMode.DefaultClientState.RenderAsBillboardType != null)
		{
			ShowBillboardPanel();
			RosterPanel.CreateAndPlaceBillboards(intface.gui, pnBillboards, data.EntityType, statusBillboardPanelCenter, 130f, doScaling: true);
		}
		else if (!string.IsNullOrEmpty(data.EntityType.ThumbnailBig))
		{
			ShowBackgroundImage();
			Rectangle sourceRectangle = intface.gui.GUI_CRT_SpriteSheet.GetSourceRectangle(data.EntityType.ThumbnailBig);
			imStatusBackground.Texture = The.InGameUI.gui.GUI_CRT_SpriteSheet.Texture;
			imStatusBackground.SetSkinLocation(SkinState.Normal, sourceRectangle);
			imStatusBackground.Position = new Point(statusContent.Width - sourceRectangle.Width, 2);
			imStatusBackground.ScaleImageToSizeOfControl = false;
			imStatusBackground.ResizeControlToFitImage();
			imStatusBackground.Alpha = 1f;
		}
	}

	public void RefreshPortraitImage(Entity entity)
	{
		ShowBackgroundImage();
		Rectangle portraitForStatusDisplay = entity.PersonEntity.GetPortraitForStatusDisplay(The.InGameUI.gui);
		imStatusBackground.Texture = Interface.gui.GUI_CRT_SpriteSheet.Texture;
		imStatusBackground.SetSkinLocation(SkinState.Normal, portraitForStatusDisplay);
		imStatusBackground.Position = new Point(statusContent.Width - portraitForStatusDisplay.Width, 2);
		imStatusBackground.ScaleImageToSizeOfControl = false;
		imStatusBackground.ResizeControlToFitImage();
		imStatusBackground.Alpha = 1f;
	}

	private void RefreshMemoryFactExplanation()
	{
		string arg = "Last seen here";
		arg = $"-{arg}-";
		lblStatusInfo4.Text = arg;
	}

	private void RefreshTask(Entity entity)
	{
		string text = entity.Intelligence.Brain.GetStatus().ToUpper(Config.Culture);
		if (text != "")
		{
			text = $"-{text}-";
		}
		lblStatusInfo3.Text = text;
	}

	private void RefreshStructureData(Entity entity)
	{
		lblStatusInfo4.Text = entity.Structure.GetStateDescription();
		if (lblStatusInfo4.Text != "")
		{
			lblStatusInfo4.Text = $"-{lblStatusInfo4.Text}-";
		}
	}

	private void RefreshPersonSpecificData(IKnownEntityData data, Entity entity)
	{
		SetHeaderText(data.GetDisplayName().ToUpper(Config.Culture));
		lblStatusInfo1.Text = entity.BiologicalEntity.CasteType.Reproduction.ToString().ToUpper(Config.Culture);
		lblStatusInfo2.Text = "AGE: " + (int)entity.BiologicalEntity.AgeGroup.Age;
		float value = entity.Intelligence.Brain.GetExertionLevelOfActivity() / GameData.Instance.Constants.PhysicalWork.MaxPhysicalWorkCost;
		string valueTerm = GameData.Instance.AllPresentationTypes["energyUsePresentation"].GetValueTerm(value, null, "");
		Label label = lblStatusInfo4;
		label.Text = label.Text + "EXERTION LEVEL: " + valueTerm;
	}

	private void RefreshEntityName(IKnownEntityData data)
	{
		SetHeaderText(data.GetDisplayName().ToUpper(Config.Culture));
	}

	public override void Refresh()
	{
		RefreshStatusScreen();
		IKnownEntityData data;
		if (!intface.SelectedEntity.HasValue)
		{
			data = null;
		}
		else
		{
			The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(intface.SelectedEntity.Value, out data);
		}
		Populate(data);
	}

	public void Populate(IKnownEntityData entityData)
	{
		outerGrid.BeginAddingEntries();
		PopulateContains(entityData);
		PopulateCustomPanels(entityData, customPanels, outerGrid);
		RefreshEntityTypeButton(entityData);
		outerGrid.Sort((UIComponent u) => u.OrderByTag1, Grid.Sorting.Ascending);
		outerGrid.EndAddingEntries();
	}

	private void RefreshEntityTypeButton(IKnownEntityData entityData)
	{
		if (entityData != null)
		{
			UpdateEntityTypeInfo(outerGrid, ref selectedEntityTypeDataButton, entityData);
		}
		else if (selectedEntityTypeDataButton != null)
		{
			outerGrid.TryRemoveEntry(selectedEntityTypeDataButton);
		}
	}

	private bool EntityCanBeSalvaged(IKnownEntityData entity)
	{
		return entity?.EntityType.CanBeSalvagedDirectly() ?? false;
	}

	private void ShowNextOrPrevious(int currentIndex)
	{
		int count = The.Sim.PlaySite.PlayerAllegiance.Persons.Count;
		if (count > 0)
		{
			if (currentIndex > count - 1)
			{
				currentIndex -= count;
			}
			else if (currentIndex < 0)
			{
				currentIndex += count;
			}
			Entity entity = The.Sim.PlaySite.PlayerAllegiance.Persons[currentIndex];
			if (!entity.IsDead && entity.IsOnPlaySite())
			{
				intface.SelectEntity(entity);
			}
		}
	}

	private void PopulateContains(IKnownEntityData entityData)
	{
		if (RosterPanel.AddPanelIfNotPresent(entityData != null && entityData.ContainedEntitiesByType != null, outerGrid, cpCarrying))
		{
			PopulateContainedItems(entityData);
		}
		else
		{
			RosterPanel.AddPanelIfNotPresent(addPanel: false, outerGrid, cpCarrying);
		}
	}

	public static void PopulateCustomPanels(IKnownEntityData entityData, List<CategoryPanelAndData> customPanels, Grid outerGrid)
	{
		bool flag = true;
		IHasExposedProperties hasExposedProperties = entityData as Entity;
		if (hasExposedProperties == null)
		{
			hasExposedProperties = entityData as MemoryFact;
			if (hasExposedProperties == null)
			{
				flag = false;
			}
		}
		foreach (CategoryPanelAndData customPanel in customPanels)
		{
			int? numberOfItems = null;
			bool flag2 = false;
			if (flag)
			{
				_ = customPanel.Category.Name == "SKILLS";
				flag2 = PresentationTypeCategoryProcessor.DisplayCategory(customPanel.Category, hasExposedProperties, customPanel.Grid, ref numberOfItems);
				customPanel.Panel.Summary = "";
				if (customPanel.Category.SetCountAsSummary)
				{
					if (customPanel.Grid.Count > 0)
					{
						customPanel.Panel.Summary = numberOfItems.ToString();
					}
				}
				else if (numberOfItems.HasValue)
				{
					customPanel.Panel.Summary = numberOfItems.ToString();
				}
			}
			try
			{
				RosterPanel.AddPanelIfNotPresent(flag2 && flag, outerGrid, customPanel.Panel);
			}
			catch (NullReferenceException)
			{
				string text = "Custom panel error #1 \n";
				text = ((customPanel == null) ? (text + ", customPanel: null") : ((customPanel.Category == null) ? (text + ", customPanel.Category: null") : (text + "customPanel.Category name: " + customPanel.Category.Name)));
				text = text + ", panelHasData: " + flag2;
				text = text + ", entityExists: " + flag;
				text = ((outerGrid != null) ? (text + ", outerGrid: not null") : (text + ", outerGrid: null"));
				throw new Exception(text);
			}
		}
	}

	private void PopulateContainedItems(IKnownEntityData entityData)
	{
		if (entityData.EntityType.BiologicalType != null)
		{
			if (cpCarrying.Title != "CARRYING")
			{
				cpCarrying.Title = "CARRYING";
			}
		}
		else if (cpCarrying.Title != "CONTAINS")
		{
			cpCarrying.Title = "CONTAINS";
		}
		if (!RosterPanel.AddPanelIfNotPresent(entityData.ContainedEntitiesByType.Count > 0, outerGrid, cpCarrying))
		{
			return;
		}
		int num = 0;
		grdCarrying.BeginAddingEntries();
		OwnerID? uIOwnerID = The.InGameUI.GetUIOwnerID();
		foreach (KeyValuePair<EntityType, List<EntityID>> item2 in entityData.ContainedEntitiesByType)
		{
			_ = entityData.TotalStored.HasValue;
			EntityType key = item2.Key;
			if (key.ItemType != null)
			{
				num += item2.Value.Count;
				if (!grdCarrying.TryGetEntry(key, out var item))
				{
					item = SidePanelMapArea.AddItemRow(grdCarrying, key, null, useCurrentUIOwner: true, tbItems_Click);
				}
				List<EntityID> value = item2.Value;
				SidePanelMapArea.UpdateItemRow(uIOwnerID, item, key, value);
			}
		}
		grdCarrying.DeleteEntries((object e) => !(e is EntityType) || entityData.ContainedEntitiesByType.ContainsKey((EntityType)e));
		grdCarrying.DeleteEntries((object e) => !(e is EntityCategory) || SidePanelMapArea.CategoryIsRepresented((EntityCategory)e, entityData.ContainedEntitiesByType));
		grdCarrying.EndAddingEntries();
		cpCarrying.Summary = num.ToString();
	}

	private static void UpdateEntityTypeInfo(Grid outerGrid, ref DataTypeButton btEntityType, IKnownEntityData entityData)
	{
		EntityType entityType = entityData.EntityType;
		EntityID entityID = entityData.EntityID;
		if (btEntityType == null)
		{
			btEntityType = new DataTypeButton(The.InGameUI.gui, DataSheet.InfoToShow.Data, entityType, null, useUIOwner: true, entityID);
		}
		else
		{
			btEntityType.FillEntityType(DataSheet.InfoToShow.Data, entityType, null, useUIOwner: true, entityType.Name, entityID);
		}
		Image image;
		if (!outerGrid.TryGetEntry(btEntityType, out var _))
		{
			UIComponent uIComponent = new UIComponent(The.InGameUI.gui);
			image = InventoryPanel.AddEntityTypeIcon(entityType, uIComponent);
			image.ID = UIComponent.DataControlID.StatusIcon;
			image.X = 2;
			image.Y = -3;
			btEntityType.Init(TextButton.TextButtonType.LCDToolTipBlack);
			btEntityType.ID = UIComponent.DataControlID.Caption;
			btEntityType.IsRoot = true;
			btEntityType.Text = entityType.Name;
			btEntityType.TextAlignment = TextButton.TextAlign.Left;
			btEntityType.Width = 160;
			btEntityType.X = image.Width + 5;
			btEntityType.DebugTag = "sidePanelTypeInfo";
			uIComponent.Add(btEntityType);
			outerGrid.AddEntry(btEntityType, uIComponent, 0);
		}
		else
		{
			image = (Image)outerGrid.FindChildById(UIComponent.DataControlID.StatusIcon);
			image.Texture = The.InGameUI.gui.GUISpriteSheet.Texture;
			IconInfo iconInfo;
			Rectangle iconSprite = entityType.GetIconSprite(out iconInfo);
			image.SetSkinLocation(SkinState.Normal, iconSprite);
			image.ResizeControlToFitImage();
		}
		if (entityData.EntityType.StructureType != null || entityData.EntityType.Person != null || entityData.EntityType.BiologicalType != null || (entityData.EntityType.TerrainType != null && entityData.EntityType.TerrainType.IsSpecialInterestFeature))
		{
			image.Color = GameData.Instance.GUIConstants.sidePanelTextColor;
		}
		else
		{
			image.Color = null;
		}
	}

	private void tbItems_Click(UIComponent sender, EventArgs e)
	{
		StockButton stockButton = sender as StockButton;
		The.InGameUI.EntityListWindow.SetDataSource(stockButton.EntityType, stockButton.EntityList);
		The.InGameUI.EntityListWindow.OpenNextToStockButton(sender);
	}

	private void previous_Click(UIComponent sender, EventArgs e)
	{
		if (intface.SelectedEntity.HasValue)
		{
			Entity entity = Entity.FindByID(intface.SelectedEntity.Value);
			if (entity != null)
			{
				int num = The.InGameUI.UIAllegiance.MembersList.IndexOf(entity);
				num = ((num != -1) ? (num - 1) : 0);
				ShowNextOrPrevious(num);
			}
		}
	}

	private void next_Click(UIComponent sender, EventArgs e)
	{
		if (intface.SelectedEntity.HasValue)
		{
			Entity entity = Entity.FindByID(intface.SelectedEntity.Value);
			if (entity != null)
			{
				int num = The.InGameUI.UIAllegiance.MembersList.IndexOf(entity);
				num = ((num != -1) ? (num + 1) : 0);
				ShowNextOrPrevious(num);
			}
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

	public List<EntityID> GetListOfItems(EntityType entityType)
	{
		if (!intface.SelectedEntity.HasValue)
		{
			return null;
		}
		IKnownEntityData data;
		if (Entity.FindByID(intface.SelectedEntity.Value).ContainedBy.HasValue)
		{
			The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(Entity.FindByID(intface.SelectedEntity.Value).ContainedBy.Value, out data);
		}
		else
		{
			The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(intface.SelectedEntity.Value, out data);
		}
		if (data == null)
		{
			return null;
		}
		List<EntityID> list = new List<EntityID>();
		Dictionary<EntityType, List<EntityID>> containedEntitiesByType = data.ContainedEntitiesByType;
		if (containedEntitiesByType != null)
		{
			foreach (KeyValuePair<EntityType, List<EntityID>> item in containedEntitiesByType)
			{
				if (item.Key != entityType)
				{
					continue;
				}
				for (int num = containedEntitiesByType[entityType].Count - 1; num >= 0; num--)
				{
					EntityID entityID = containedEntitiesByType[entityType][num];
					if (!GoalEvaluator.EntityDataResultCausesSkip(The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(entityID, out var data2)) && data2.EntityType == entityType)
					{
						list.Add(data2.EntityID);
					}
				}
			}
		}
		return list;
	}
}
