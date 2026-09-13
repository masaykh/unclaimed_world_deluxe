using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface.Controls;
using UWGame.ClientSide.Interface.HUD_Windows;
using UWGame.ClientSide.Interface.Inventory;
using UWGame.ClientSide.Interface.LCD;
using UWGame.SimSide;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Overland.Missions.Templates;
using UWGame.SimSide.Policies;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Trade;
using WindowSystem;

namespace UWGame.ClientSide.Interface.BuyAndSell;

public class BuySellPanel : Panel
{
	public enum BuySellDialogMode
	{
		ViewSellAtNPC,
		ViewBuyAtNPC,
		ActionSellAtPlayer,
		ActionBuyAtNPC
	}

	public delegate V Func<T, U, V>(T input, out U output);

	public delegate bool CanTradeDelegate(EntityType entityType, out TierOrAreaType unavailablePolicy);

	private BuySellDialogMode mode;

	private Grid grdCategoryView;

	private Grid grdListView;

	private LCDInnerPanel lcdMessagePanel;

	private LCDScreen lcdScreen;

	private UIComponent lcdSurface;

	private Box display;

	private int itemTypeIconColumnX = 40;

	private int captionX = 70;

	private int quantityX = 235;

	private int sliderX = 242;

	private int offerDemandX = 384;

	private int priceX = 443;

	private int bulkX = 512;

	private Dictionary<EntityType, InventoryPanel.Availability> allAvailableItems = new Dictionary<EntityType, InventoryPanel.Availability>();

	private ImageButton ibCategory;

	private ImageButton ibList;

	private Color unattainableColor = GameData.Instance.GUIConstants.UnattainableColor;

	private Icon backgroundTint;

	private Dictionary<EntityType, List<EntityID>> currentOrders;

	private BuySellPanelSettings settings;

	private EntityGroupID? ownerOfItemsID;

	private EntityGroupID? buyerID;

	private Func<EntityType, bool, List<EntityID>> getItems;

	private CanTradeDelegate CanTrade;

	private UIComponent sortingButtonsContainer;

	private FillableBar sliderBeingDragged;

	private const int gridTopMargin = 50;

	private const int gridBottomMargin = 50;

	private Label lblNoWaresNote;

	private Label lblTotalItemCost;

	private Label lblTotalBulk;

	private FilterPropertiesPanel filterPropertiesPanel;

	private LCDInnerPanel filterPanelContainer;

	private LCDInnerPanel totalsPanel;

	private SortingButtons<BuySellPanelSettings.SortColumns> sortingButtons;

	private TextButton btOK;

	private TextButton btCancel;

	private TextButton tbExpand;

	private Dictionary<EntityType, EntityType> presentItemTypes = new Dictionary<EntityType, EntityType>();

	private List<Grid> categoryGrids = new List<Grid>();

	private CargoActionTypes CargoActionType
	{
		get
		{
			switch (mode)
			{
			case BuySellDialogMode.ViewBuyAtNPC:
			case BuySellDialogMode.ActionBuyAtNPC:
				return CargoActionTypes.Buy;
			case BuySellDialogMode.ViewSellAtNPC:
			case BuySellDialogMode.ActionSellAtPlayer:
				return CargoActionTypes.Sell;
			default:
				return CargoActionTypes.Buy;
			}
		}
	}

	private bool AllowOrders
	{
		get
		{
			if (mode == BuySellDialogMode.ActionBuyAtNPC || mode == BuySellDialogMode.ActionSellAtPlayer)
			{
				return true;
			}
			return false;
		}
	}

	public Dictionary<EntityType, List<EntityID>> Orders => currentOrders;

	public event EventHandler OKClick;

	public event EventHandler CancelClick;

	public BuySellPanel(CommonInterface intf, Point position)
		: base(intf, "", position, new Vector2(614f, 580f), Level.StackedDialogs)
	{
		RosterPanel.CreateRosterStyleLCDPanel(intf, Window, out display, out lcdSurface, ref lcdScreen, 52, null, addTopEdgeDirt: true, addBottomEdgeDirt: false);
		filterPanelContainer = new LCDInnerPanel(Interface.gui, lcdSurface.Width, includeDecor: false);
		filterPanelContainer.HorizontalContentPadding = 0;
		filterPanelContainer.VerticalContentPadding = 5;
		filterPanelContainer.ContentHeight = 150;
		lcdSurface.Add(filterPanelContainer.Panel);
		filterPropertiesPanel = new FilterPropertiesPanel(Interface.gui, makeRoomForExpandButton: true);
		filterPropertiesPanel.FiltersChanged += filterPropertiesPanel_FiltersChanged;
		filterPanelContainer.AddContentSetFullWidth(filterPropertiesPanel);
		filterPropertiesPanel.Y = 2;
		tbExpand = new TextButton(Interface.gui);
		lcdSurface.Add(tbExpand);
		tbExpand.Init(TextButton.TextButtonType.LCD);
		tbExpand.Text = "MORE";
		tbExpand.X = 2;
		tbExpand.Y = 2;
		tbExpand.Click += Expand_Click;
		tbExpand.ScaleToFitText();
		tbExpand.Height = 30;
		sortingButtonsContainer = new UIComponent(Interface.gui);
		lcdSurface.Add(sortingButtonsContainer);
		sortingButtonsContainer.Width = lcdSurface.Width;
		sortingButtonsContainer.Height = 50;
		sortingButtonsContainer.Position = new Point(0, 160);
		sortingButtons = new SortingButtons<BuySellPanelSettings.SortColumns>(Interface.gui);
		sortingButtons.SortClicked += sortingButtons_SortClicked;
		sortingButtons.Position = new Point(56, 0);
		sortingButtons.Width = lcdSurface.Width - sortingButtons.X;
		sortingButtonsContainer.Add(sortingButtons);
		totalsPanel = new LCDInnerPanel(Interface.gui, lcdSurface.Width, includeDecor: false);
		totalsPanel.HorizontalContentPadding = 0;
		totalsPanel.VerticalContentPadding = 5;
		totalsPanel.ContentHeight = 26;
		lcdSurface.Add(totalsPanel.Panel);
		totalsPanel.Panel.Y = lcdSurface.Height - totalsPanel.Height;
		totalsPanel.TintPanel(Color.Cornsilk);
		grdCategoryView = new Grid(Interface.gui, ListBoxType.LCD, Label.LabelType.LCDNormal);
		grdCategoryView.FixedItemHeights = false;
		grdCategoryView.RenderType = RenderType.CRTAndLCD;
		lcdSurface.Add(grdCategoryView);
		grdCategoryView.Font = GUIManager.LCDandHUDBodyFontPath;
		grdCategoryView.Width = lcdSurface.Width;
		grdCategoryView.Height = lcdSurface.Height - 50 - 50 - 30;
		grdCategoryView.ItemHeight = 26;
		grdCategoryView.Position = new Point(0, 50);
		grdListView = new Grid(Interface.gui, ListBoxType.LCD, Label.LabelType.LCDNormal);
		grdListView.FixedItemHeights = true;
		grdListView.RenderType = RenderType.CRTAndLCD;
		lcdSurface.Add(grdListView);
		grdListView.Font = GUIManager.LCDandHUDBodyFontPath;
		grdListView.Width = lcdSurface.Width;
		grdListView.Height = lcdSurface.Height - 50 - 50 - 30;
		grdListView.ItemHeight = 26;
		grdListView.Position = new Point(0, 50);
		lblNoWaresNote = new Label(Interface.gui);
		lblNoWaresNote.Init(Label.LabelType.LCDNormal);
		lcdSurface.Add(lblNoWaresNote);
		lblNoWaresNote.Y = 50;
		lblNoWaresNote.X = 6;
		lblNoWaresNote.Visible = false;
		Label label = new Label(Interface.gui);
		totalsPanel.AddContent(label);
		label.Init(Label.LabelType.LCDSmallHeadingBanner);
		label.Text = "TOTAL:";
		label.Y = 8;
		label.X = 280;
		label.FitToText();
		lblTotalBulk = new Label(Interface.gui);
		totalsPanel.AddContent(lblTotalBulk);
		lblTotalBulk.Init(Label.LabelType.LCDNormal);
		lblTotalBulk.X = bulkX;
		lblTotalBulk.Y = label.Y;
		lblTotalItemCost = new Label(Interface.gui);
		totalsPanel.AddContent(lblTotalItemCost);
		lblTotalItemCost.Init(Label.LabelType.LCDNormal);
		lblTotalItemCost.AlignRight(priceX);
		lblTotalItemCost.Y = lblTotalBulk.Y;
		Panel.AddImage(Interface.gui, totalsPanel.Panel, "lcd_icon_credits_oneCoin", new Point(lblTotalItemCost.Right + 6, lblTotalItemCost.Y + 2)).SetSkinLocation(SkinState.Normal, null, UIComponent.LCDNormal, UIComponent.LCDNormal);
		btOK = AddLowerButton("OK", "Accepts the order and closes the dialog.", Align.Left);
		btOK.Click += btOK_Click;
		btCancel = AddLowerButton("CANCEL", "Cancels and closes the dialog.", Align.Right);
		btCancel.Click += btCancel_Click;
		CreateGridHeaderButtons();
		RosterPanel.AddTintedBackground(ref backgroundTint, display);
	}

	private void LoadUserSettings()
	{
		filterPropertiesPanel.Fill(settings.FilterPropertySettings);
		sortingButtons.Fill(settings.SortingSettings);
		SetView(settings.viewType);
		ExpandOrCollapseTopPanel(settings.IsExpanded);
	}

	private void Expand_Click(UIComponent sender, EventArgs e)
	{
		bool expand = !settings.IsExpanded;
		ExpandOrCollapseTopPanel(expand);
	}

	private void sortingButtons_SortClicked()
	{
		Populate();
	}

	private void filterPropertiesPanel_FiltersChanged()
	{
		Refresh();
	}

	private void ExpandOrCollapseTopPanel(bool expand)
	{
		if (expand)
		{
			SetMinimizedOrExpandedContentProperties(85, visible: true, "LESS", "Hide search options");
		}
		else
		{
			SetMinimizedOrExpandedContentProperties(26, visible: false, "MORE", "Display search options");
		}
		settings.IsExpanded = expand;
	}

	private void SetMinimizedOrExpandedContentProperties(int panelheight, bool visible, string text, string btToolTip)
	{
		tbExpand.ToolTip = btToolTip;
		tbExpand.Text = text;
		UpdateGridYPosition(panelheight);
		if (visible)
		{
			filterPanelContainer.AddContentSetFullWidth(filterPropertiesPanel);
		}
		else
		{
			filterPanelContainer.RemoveContent(filterPropertiesPanel);
		}
	}

	private void CreateGridHeaderButtons()
	{
		ibList = new ImageButton(Interface.gui);
		ibList.Width = 30;
		sortingButtonsContainer.Add(ibList);
		ibList.Width = 30;
		ibList.InitWithIcon(ImageButtonType.LCD, "basic_icon_list", hasCheckedState: true);
		ibList.Click += tbListView_Click;
		ibList.Y = 0;
		ibList.X = 0;
		ibList.ToolTip = "List view";
		ibList.Width = 30;
		ibList.Height = 30;
		ibList.RecalculateIconPosition();
		ibCategory = new ImageButton(Interface.gui);
		ibCategory.Width = 30;
		sortingButtonsContainer.Add(ibCategory);
		ibCategory.Width = 30;
		ibCategory.InitWithIcon(ImageButtonType.LCD, "basic_icon_category", hasCheckedState: true);
		ibCategory.Click += tbCategoryView_Click;
		ibCategory.ToolTip = "Category view";
		ibCategory.Y = 0;
		ibCategory.X = 30;
		ibCategory.IsChecked = true;
		ibCategory.Width = 30;
		ibCategory.Height = 30;
		ibCategory.RecalculateIconPosition();
		sortingButtons.CreateTextButton(0, 185, "NAME", BuySellPanelSettings.SortColumns.Name);
		sortingButtons.CreateTextButton(179, 99, "AMOUNT", BuySellPanelSettings.SortColumns.Amount);
		sortingButtons.CreateTextButton(277, 62, "", BuySellPanelSettings.SortColumns.OfferDemand, "OFFER/DEMAND");
		sortingButtons.CreateTextButton(336, 62, "", BuySellPanelSettings.SortColumns.Price, "PRICE");
		sortingButtons.CreateTextButton(397, 90, "BULK", BuySellPanelSettings.SortColumns.Bulk);
	}

	private void UpdateGridYPosition(int filterPanelHeight)
	{
		filterPanelContainer.ContentHeight = filterPanelHeight;
		int y = filterPanelContainer.Panel.Bottom + 2;
		sortingButtonsContainer.Y = y;
		grdListView.Y = sortingButtonsContainer.Bottom - 15;
		grdCategoryView.Y = sortingButtonsContainer.Bottom - 15;
		int height = lcdSurface.Height - grdCategoryView.Y - totalsPanel.Height;
		grdCategoryView.Height = height;
		grdListView.Height = height;
	}

	private void SetView(ViewType viewTypeToSet)
	{
		settings.viewType = viewTypeToSet;
		if (settings.viewType == ViewType.List)
		{
			lcdSurface.Remove(grdCategoryView);
			lcdSurface.Add(grdListView);
			ibCategory.IsChecked = false;
			ibList.IsChecked = true;
		}
		else
		{
			lcdSurface.Remove(grdListView);
			lcdSurface.Add(grdCategoryView);
			ibCategory.IsChecked = true;
			ibList.IsChecked = false;
		}
	}

	private void tbListView_Click(UIComponent sender, EventArgs e)
	{
		SetView(ViewType.List);
		Populate();
	}

	private void tbCategoryView_Click(UIComponent sender, EventArgs e)
	{
		SetView(ViewType.Categories);
		Populate();
	}

	private void btOK_Click(UIComponent sender, EventArgs e)
	{
		new List<Grid>();
		if (!ResolveEntityGroup(out var ownerOfItems, out var _, out var buyer))
		{
			return;
		}
		bool isPlayerOwnedLocation = GetIsPlayerOwnedLocation(ownerOfItems);
		if (currentOrders != null)
		{
			foreach (KeyValuePair<EntityType, List<EntityID>> currentOrder in currentOrders)
			{
				decimal? agreedPrice = GetAgreedPrice(ownerOfItems, buyer, currentOrder.Key);
				if (GetTradeItems(ownerOfItems, buyer, isPlayerOwnedLocation, currentOrder.Key, out var items, agreedPrice, out var _, out var _, out var itemSourceIsInvalid))
				{
					ValidateAndCountOrders(currentOrder.Key, items);
				}
				else if (itemSourceIsInvalid)
				{
					return;
				}
			}
			List<EntityType> list = null;
			foreach (KeyValuePair<EntityType, List<EntityID>> currentOrder2 in currentOrders)
			{
				if (currentOrder2.Value.Count == 0)
				{
					Common.AddToList(ref list, currentOrder2.Key);
				}
			}
			if (list != null)
			{
				foreach (EntityType item in list)
				{
					currentOrders.Remove(item);
				}
			}
		}
		if (this.OKClick != null)
		{
			this.OKClick(this, null);
		}
		Hide();
	}

	public static string GetHeading(BuySellDialogMode mode, EntityGroup buyer, EntityGroup siteOwner)
	{
		string result = "";
		switch (mode)
		{
		case BuySellDialogMode.ViewSellAtNPC:
			result = "Willing to buy at ";
			result += buyer.GetAllegiance().Name;
			break;
		case BuySellDialogMode.ViewBuyAtNPC:
			result = "For sale at ";
			result += siteOwner.GetAllegiance().Name;
			break;
		case BuySellDialogMode.ActionSellAtPlayer:
			result = "Sell at ";
			result += siteOwner.GetAllegiance().Name;
			break;
		case BuySellDialogMode.ActionBuyAtNPC:
			result = "For sale at ";
			result += siteOwner.GetAllegiance().Name;
			break;
		}
		return result;
	}

	public void Fill(BuySellDialogMode mode, Func<EntityType, bool, List<EntityID>> getItems, CanTradeDelegate canTrade, EntityGroupID? ownerID, EntityGroupID? buyerID, Dictionary<EntityType, List<EntityID>> currentOrders, BuySellPanelSettings settings)
	{
		this.mode = mode;
		this.getItems = getItems;
		CanTrade = canTrade;
		ownerOfItemsID = ownerID;
		this.buyerID = buyerID;
		this.currentOrders = currentOrders;
		if (AllowOrders)
		{
			lcdSurface.Add(totalsPanel.Panel);
			btOK.Visible = true;
		}
		else
		{
			lcdSurface.Remove(totalsPanel.Panel);
			btOK.Visible = false;
		}
		this.settings = settings;
		LoadUserSettings();
		if (ResolveEntityGroup(out var ownerOfItems, out var _, out var buyer))
		{
			bool isPlayerOwnedLocation = GetIsPlayerOwnedLocation(ownerOfItems);
			string heading = GetHeading(mode, buyer, ownerOfItems);
			lblTitle.Text = heading;
			if (CargoActionType == CargoActionTypes.Sell)
			{
				sortingButtons.EnableButton(BuySellPanelSettings.SortColumns.OfferDemand, isPlayerOwnedLocation);
				Color sellingTint = GameData.Instance.GUIConstants.SellingTint;
				sellingTint.A = 160;
				RosterPanel.SetBackgroundTint(sellingTint, backgroundTint);
			}
			else if (CargoActionType == CargoActionTypes.Buy)
			{
				sortingButtons.EnableButton(BuySellPanelSettings.SortColumns.OfferDemand, enable: false);
				Color buyingTint = GameData.Instance.GUIConstants.BuyingTint;
				buyingTint.A = 160;
				RosterPanel.SetBackgroundTint(buyingTint, backgroundTint);
			}
			else
			{
				sortingButtons.EnableButton(BuySellPanelSettings.SortColumns.OfferDemand, enable: false);
				RosterPanel.SetBackgroundTint(null, backgroundTint);
			}
		}
	}

	public override void Refresh()
	{
		Populate();
		base.Refresh();
	}

	private void HandleDestroyedEntityGroupOrDataSource()
	{
		The.InGameUI.MessageBox.ShowMessage("The terminal no longer exists. It is not possible to continue browsing the items.");
		The.InGameUI.MessageBox.OKClick += MessageBoxDestroyedMission_OKClick;
	}

	private void MessageBoxDestroyedMission_OKClick(object sender, EventArgs e)
	{
		The.InGameUI.MessageBox.OKClick -= MessageBoxDestroyedMission_OKClick;
		grdCategoryView.Clear();
		grdListView.Clear();
		CancelDialog();
	}

	private bool GetData(out HashSet<EntityType> data)
	{
		data = settings.GetData();
		return true;
	}

	private bool ResolveEntityGroup(out EntityGroup ownerOfItems, out Expedition ownerOfItemsExpedition, out EntityGroup buyer)
	{
		ownerOfItems = null;
		ownerOfItemsExpedition = null;
		buyer = null;
		if (mode != BuySellDialogMode.ViewSellAtNPC)
		{
			ownerOfItems = LookUp<EntityGroup, EntityGroupID>.FindByID(ownerOfItemsID);
			if (ownerOfItems == null)
			{
				HandleDestroyedEntityGroupOrDataSource();
				return false;
			}
			ownerOfItemsExpedition = ownerOfItems.Parent as Expedition;
		}
		if (buyerID.HasValue)
		{
			buyer = LookUp<EntityGroup, EntityGroupID>.FindByID(buyerID);
			if (buyer == null)
			{
				HandleDestroyedEntityGroupOrDataSource();
				return false;
			}
		}
		return true;
	}

	private void PopulateWithCategories(EntityGroup ownerOfItems, Expedition expedition, EntityGroup npcBuyer)
	{
		if (!GetData(out var data))
		{
			return;
		}
		Grid categoryGrid = null;
		presentItemTypes.Clear();
		categoryGrids.Clear();
		allAvailableItems.Clear();
		_ = grdCategoryView.Entries.Count;
		grdCategoryView.BeginAddingEntries();
		bool isPlayerOwnedLocation = GetIsPlayerOwnedLocation(ownerOfItems);
		foreach (EntityType item3 in data)
		{
			EntityCategory category = item3.Category;
			object category2 = item3.Category;
			decimal? agreedPrice = GetAgreedPrice(ownerOfItems, npcBuyer, item3);
			CollapsablePanel cpCategory = null;
			if (grdCategoryView.TryGetEntry(category2, out var item))
			{
				cpCategory = item as CollapsablePanel;
				categoryGrid = (Grid)cpCategory.ExpandedPanel.Controls[0];
				if (!categoryGrids.Contains(categoryGrid))
				{
					categoryGrid.BeginAddingEntries();
					categoryGrids.Add(categoryGrid);
				}
			}
			bool itemSourceIsInvalid = false;
			List<EntityID> items;
			int available;
			int demandedItems;
			if (cpCategory == null)
			{
				if (!GetTradeItems(ownerOfItems, npcBuyer, isPlayerOwnedLocation, item3, out items, agreedPrice, out available, out demandedItems, out itemSourceIsInvalid))
				{
					if (itemSourceIsInvalid)
					{
						return;
					}
					continue;
				}
				AddCategoryRow(ref cpCategory, ref categoryGrid, category);
				categoryGrid.BeginAddingEntries();
				categoryGrids.Add(categoryGrid);
			}
			if (GetTradeItems(ownerOfItems, npcBuyer, isPlayerOwnedLocation, item3, out items, agreedPrice, out available, out demandedItems, out itemSourceIsInvalid))
			{
				presentItemTypes.Add(item3, item3);
				if (!categoryGrid.TryGetEntry(item3, out var item2))
				{
					item2 = AddItemRow(categoryGrid, item3, ownerOfItems, useCurrentUIOwner: true, isInCategoryPanel: true);
				}
				UpdateItemRow(item2, item3, ownerOfItems, isPlayerOwnedLocation, items, available, demandedItems, agreedPrice, isInCategoryPanel: true);
			}
			else if (itemSourceIsInvalid)
			{
				return;
			}
		}
		FullLCDPanel.Cleanup<EntityType, int, EntityCategory>(grdCategoryView, null, presentItemTypes, null);
		Grid.Sorting sortOrder = GetSortOrder();
		InventoryPanel.DoCategorySorting(grdCategoryView, categoryGrids, sortOrder);
		grdCategoryView.EndAddingEntries();
	}

	private decimal? GetAgreedPrice(EntityGroup owner, EntityGroup buyer, EntityType entityType)
	{
		return BuySellActionTemplate.GetTradePrice(entityType, buyer, owner);
	}

	private void PopulateWithList(EntityGroup ownerOfItems, Expedition expeditionOwner, EntityGroup npcBuyer)
	{
		if (!GetData(out var data))
		{
			return;
		}
		presentItemTypes.Clear();
		bool isPlayerOwnedLocation = GetIsPlayerOwnedLocation(ownerOfItems);
		grdListView.BeginAddingEntries();
		decimal? num = null;
		foreach (EntityType item2 in data)
		{
			item2.KeyName.Contains("hauling");
			num = GetAgreedPrice(ownerOfItems, npcBuyer, item2);
			bool itemSourceIsInvalid = false;
			if (!GetTradeItems(ownerOfItems, npcBuyer, isPlayerOwnedLocation, item2, out var items, num, out var available, out var demandedItems, out itemSourceIsInvalid))
			{
				if (itemSourceIsInvalid)
				{
					return;
				}
				continue;
			}
			presentItemTypes.Add(item2, item2);
			if (!grdListView.TryGetEntry(item2, out var item))
			{
				item = AddItemRow(grdListView, item2, ownerOfItems, useCurrentUIOwner: true, isInCategoryPanel: false);
			}
			UpdateItemRow(item, item2, ownerOfItems, isPlayerOwnedLocation, items, available, demandedItems, num, isInCategoryPanel: false);
		}
		grdListView.DeleteEntries((EntityType j) => presentItemTypes.ContainsKey(j));
		Grid.Sorting sortOrder = GetSortOrder();
		grdListView.Sort((UIComponent i) => i.OrderByTag1, sortOrder);
		grdListView.EndAddingEntries();
	}

	private Grid.Sorting GetSortOrder()
	{
		Grid.Sorting result = Grid.Sorting.Ascending;
		if (settings != null)
		{
			result = settings.SortingSettings.SortOrder;
		}
		return result;
	}

	private void Populate()
	{
		if (!ResolveEntityGroup(out var ownerOfItems, out var ownerOfItemsExpedition, out var buyer))
		{
			return;
		}
		switch (settings.viewType)
		{
		case ViewType.Categories:
			PopulateWithCategories(ownerOfItems, ownerOfItemsExpedition, buyer);
			if (grdCategoryView.Count > 0 && grdListView.Count == 0)
			{
				PopulateWithList(ownerOfItems, ownerOfItemsExpedition, buyer);
			}
			break;
		case ViewType.List:
			PopulateWithList(ownerOfItems, ownerOfItemsExpedition, buyer);
			if (grdListView.Count > 0 && grdCategoryView.Count == 0)
			{
				PopulateWithCategories(ownerOfItems, ownerOfItemsExpedition, buyer);
			}
			break;
		}
		if (((settings.viewType == ViewType.Categories && grdCategoryView.Entries.Count == 0) || (settings.viewType == ViewType.List && grdListView.Entries.Count == 0)) && !settings.FilterPropertySettings.HasActiveFilters())
		{
			lblNoWaresNote.ToolTip = "";
			if (CargoActionType == CargoActionTypes.Buy)
			{
				if (GetIsPlayerOwnedLocation(ownerOfItems))
				{
					if (settings.FilterPropertySettings.HasActiveFilters())
					{
						lblNoWaresNote.Text = "We are not offering anything for trade matching the filters.";
					}
					else
					{
						lblNoWaresNote.Text = "We are not offering anything for trade.";
						lblNoWaresNote.ToolTip = "Items have to be placed inside a terminal and be included in the Trade settings to appear here.";
					}
				}
				else
				{
					lblNoWaresNote.Text = "Nothing is available to buy.";
				}
			}
			else
			{
				lblNoWaresNote.Text = "Nothing can be sold here.";
			}
			lblNoWaresNote.FitToText();
			lblNoWaresNote.Y = sortingButtonsContainer.Bottom + 6;
			lblNoWaresNote.Visible = true;
		}
		else
		{
			lblNoWaresNote.Visible = false;
		}
		UpdateTotals(ownerOfItems, buyer);
	}

	private int ValidateAndCountOrders(EntityType entityType, List<EntityID> availableItems)
	{
		int result = 0;
		if (currentOrders.TryGetValue(entityType, out var value))
		{
			for (int num = value.Count - 1; num >= 0; num--)
			{
				EntityID item = value[num];
				if (availableItems == null || !availableItems.Contains(item))
				{
					value.RemoveAt(num);
				}
			}
			result = value.Count;
		}
		return result;
	}

	private bool GetTradeItems(EntityGroup itemsOwner, EntityGroup npcBuyer, bool isPlayerOwned, EntityType entityType, out List<EntityID> items, decimal? price, out int available, out int demandedItems, out bool itemSourceIsInvalid)
	{
		itemSourceIsInvalid = false;
		items = null;
		available = 0;
		demandedItems = 0;
		if (CargoActionType == CargoActionTypes.Buy)
		{
			if (!isPlayerOwned && !price.HasValue)
			{
				return false;
			}
			items = getItems(entityType, out itemSourceIsInvalid);
			if (itemSourceIsInvalid)
			{
				HandleDestroyedEntityGroupOrDataSource();
				itemSourceIsInvalid = true;
				available = 0;
				return false;
			}
			if (items == null)
			{
				if (!price.HasValue && isPlayerOwned)
				{
					return false;
				}
				available = 0;
			}
			else
			{
				available = items.Count;
			}
		}
		else if (CargoActionType == CargoActionTypes.Sell)
		{
			if (isPlayerOwned)
			{
				items = getItems(entityType, out itemSourceIsInvalid);
				if (itemSourceIsInvalid)
				{
					HandleDestroyedEntityGroupOrDataSource();
					itemSourceIsInvalid = true;
					available = 0;
					return false;
				}
				if (!price.HasValue)
				{
					if (items != null)
					{
						available = 0;
						return true;
					}
					return false;
				}
				demandedItems = npcBuyer.GetBuyAmount(entityType);
				if (items != null)
				{
					available = Math.Min(items.Count, demandedItems);
				}
				else
				{
					available = 0;
				}
			}
			else
			{
				if (!price.HasValue)
				{
					items = null;
					available = 0;
					return false;
				}
				items = null;
				available = npcBuyer.GetBuyAmount(entityType);
			}
		}
		return true;
	}

	public static int GetItemColumn(int headerXPos, bool isInCategoryPanel)
	{
		int num = headerXPos;
		if (isInCategoryPanel)
		{
			num -= 7;
		}
		return num;
	}

	private UIComponent AddItemRow(Grid categoryGrid, EntityType entityType, EntityGroup owner, bool useCurrentUIOwner, bool isInCategoryPanel)
	{
		UIComponent uIComponent = new UIComponent(Interface.gui);
		categoryGrid.AddEntry(entityType, uIComponent);
		InventoryPanel.AddEntityTypeIcon(entityType, uIComponent, itemTypeIconColumnX);
		ItemTypeButtonEventArgs eventArgs = new ItemTypeButtonEventArgs(entityType);
		DataTypeButton dataTypeButton = new DataTypeButton(Interface.gui, DataSheet.InfoToShow.Data, entityType, GoalEvaluator.GetOwnerID(owner), useCurrentUIOwner);
		int itemColumn = GetItemColumn(captionX, isInCategoryPanel);
		dataTypeButton.Init(TextButton.TextButtonType.LCDToolTipBlack);
		dataTypeButton.ID = UIComponent.DataControlID.Caption;
		dataTypeButton.IsRoot = true;
		dataTypeButton.Text = entityType.PluralName;
		uIComponent.Add(dataTypeButton);
		dataTypeButton.TextAlignment = TextButton.TextAlign.Left;
		dataTypeButton.Width = 170;
		dataTypeButton.X = itemColumn;
		HorizontalList horizontalList = new HorizontalList(Interface.gui);
		uIComponent.Add(horizontalList);
		horizontalList.ID = UIComponent.DataControlID.NotAttainableIcons;
		horizontalList.X = GetItemColumn(sliderX, isInCategoryPanel);
		horizontalList.Height = 21;
		horizontalList.Y = -2;
		FillableBar fillableBar = new FillableBar(Interface.gui, FillableBar.FillableBarType.LCDSlider, canGrow: false, includeButtons: true, GameData.Instance.GUIConstants.TimeBetweenSliderButtonIncrements, GameData.Instance.GUIConstants.SliderButtonDelay);
		uIComponent.Add(fillableBar);
		fillableBar.ID = UIComponent.DataControlID.CurrentOrders;
		fillableBar.Width = 105;
		fillableBar.X = GetItemColumn(sliderX, isInCategoryPanel);
		fillableBar.Y = 1;
		fillableBar.Tag1 = entityType;
		fillableBar.EventArgs = eventArgs;
		fillableBar.SliderMouseUp += fillableBar_SliderMouseUp;
		fillableBar.SliderMouseDown += fillableBar_SliderMouseDown;
		Label label = new Label(Interface.gui);
		uIComponent.Add(label);
		label.Init(Label.LabelType.LCDNormal);
		label.X = GetItemColumn(sliderX + 65, isInCategoryPanel);
		label.ID = UIComponent.DataControlID.Amount;
		label = new Label(Interface.gui);
		uIComponent.Add(label);
		label.Init(Label.LabelType.LCDNormal);
		label.X = GetItemColumn(offerDemandX, isInCategoryPanel);
		label.ID = UIComponent.DataControlID.OfferDemand;
		label.TooltipExpires = false;
		label.TooltipWidth = 240;
		label = new Label(Interface.gui);
		uIComponent.Add(label);
		label.Init(Label.LabelType.LCDNormal);
		label.X = GetItemColumn(priceX, isInCategoryPanel);
		label.ID = UIComponent.DataControlID.Price;
		label = new Label(Interface.gui);
		uIComponent.Add(label);
		label.Init(Label.LabelType.LCDNormal);
		label.X = GetItemColumn(bulkX, isInCategoryPanel);
		label.ID = UIComponent.DataControlID.Bulk;
		uIComponent.OrderByTag1 = entityType.PluralName;
		return uIComponent;
	}

	private void AddCategoryRow(ref CollapsablePanel cpCategory, ref Grid categoryGrid, EntityCategory entityCategory)
	{
		cpCategory = new CollapsablePanel(Interface.gui, CollapsablePanel.PanelType.DropDownBig);
		cpCategory.HeadingYPos = 4;
		cpCategory.CollapsedHeight = grdCategoryView.ItemHeight;
		grdCategoryView.AddEntry(entityCategory, cpCategory);
		cpCategory.OrderByTag1 = entityCategory.SortOrder;
		cpCategory.Init(InventoryPanel.GetPanelSubType(entityCategory));
		cpCategory.Title = entityCategory.Name;
		cpCategory.Width = grdCategoryView.Width;
		categoryGrid = new Grid(Interface.gui, ListBoxType.LCD, Label.LabelType.LCDNormal);
		categoryGrid.DebugTag = "categoryGrid";
		categoryGrid.FixedItemHeights = true;
		categoryGrid.Width = cpCategory.Width;
		cpCategory.AddContent(categoryGrid);
		categoryGrid.ScrollBarEnabled = false;
		categoryGrid.ItemHeight = 26;
		categoryGrid.CanGrowInHeight = true;
		categoryGrid.Font = GUIManager.LCDandHUDBodyFontPath;
		categoryGrid.IsOuterGrid = false;
	}

	private void UpdateItemRow(UIComponent itemRow, EntityType entityType, EntityGroup owner, bool isOwnedByUI, List<EntityID> items, int noOfAvailableItems, int demandedItems, decimal? price, bool isInCategoryPanel)
	{
		int num = 0;
		if (currentOrders != null)
		{
			num = ValidateAndCountOrders(entityType, items);
		}
		int num2 = 0;
		if (items != null)
		{
			num2 = items.Count;
		}
		UIComponent uIComponent = itemRow.FindChildById(UIComponent.DataControlID.Amount);
		HorizontalList horizontalList = itemRow.FindChildById(UIComponent.DataControlID.NotAttainableIcons) as HorizontalList;
		FillableBar fillableBar = itemRow.FindChildById(UIComponent.DataControlID.CurrentOrders) as FillableBar;
		bool flag = false;
		bool isPlayerOwnedLocation = GetIsPlayerOwnedLocation(owner);
		if (!AllowOrders)
		{
			fillableBar.Visible = false;
			uIComponent.Visible = true;
			Label label = (Label)uIComponent;
			label.Text = noOfAvailableItems.ToString();
			label.FitToText();
			if (CargoActionType == CargoActionTypes.Sell)
			{
				label.ToolTip = "Current demand: The amount of goods this site is willing to buy right now. This number may gradually rise.";
			}
			else if (CargoActionType == CargoActionTypes.Buy)
			{
				if (isPlayerOwnedLocation)
				{
					label.ToolTip = "For sale: The amount of goods we can trade right now. Items have to be inside a terminal and included in the Trade settings to appear here.";
				}
				else
				{
					label.ToolTip = "For sale: The amount of goods this site has for sale right now. This number may gradually rise.";
				}
			}
			else
			{
				label.ToolTip = null;
			}
		}
		else
		{
			uIComponent.Visible = false;
			if (CanTrade(entityType, out var unavailablePolicy))
			{
				fillableBar.Visible = true;
				if (sliderBeingDragged != fillableBar)
				{
					if (fillableBar.MaxValue != noOfAvailableItems)
					{
						fillableBar.MaxValue = noOfAvailableItems;
						flag = true;
					}
					if (fillableBar.Value != num)
					{
						fillableBar.Value = num;
						flag = true;
					}
					if (flag)
					{
						fillableBar.UpdateSliderPosition();
					}
					fillableBar.Visible = true;
					if (noOfAvailableItems > 0)
					{
						fillableBar.Enabled = true;
						fillableBar.ToolTip = "Drag slider to specify amount to order.";
					}
					else
					{
						if (fillableBar.Enabled)
						{
							fillableBar.UpdateSliderPosition();
							fillableBar.Enabled = false;
						}
						if (CargoActionType == CargoActionTypes.Buy)
						{
							fillableBar.ToolTip = "This item is currently not available to buy.";
						}
						else if (demandedItems == 0)
						{
							fillableBar.ToolTip = "There is no demand for this item.";
						}
						else
						{
							fillableBar.ToolTip = "There are no items stored and offered for trade in the terminal building.";
						}
					}
				}
			}
			else
			{
				fillableBar.Visible = false;
				horizontalList.Visible = true;
				PopulateNotAttainableIcons(horizontalList, unavailablePolicy);
			}
		}
		((DataTypeButton)itemRow.FindChildById(UIComponent.DataControlID.Caption)).SetAvailableStatusColor(noOfAvailableItems > 0);
		Label label2 = (Label)itemRow.FindChildById(UIComponent.DataControlID.OfferDemand);
		if (CargoActionType == CargoActionTypes.Sell && isOwnedByUI)
		{
			label2.Text = num2 + "|" + demandedItems;
			StringBuilder stringBuilder = new StringBuilder();
			Common.AppendLine(stringBuilder, "Offer | Demand");
			Common.AppendDivider(stringBuilder);
			Common.Append(stringBuilder, "Amount we are offering for sale: ");
			Common.Append(stringBuilder, num2.ToString(), tintAsValue: true);
			Common.AppendLine(stringBuilder);
			Common.Append(stringBuilder, "Max. amount buyer wants: ");
			Common.Append(stringBuilder, demandedItems.ToString(), tintAsValue: true);
			Common.AppendLine(stringBuilder);
			Common.AppendLine(stringBuilder);
			Common.Append(stringBuilder, "Max. amount we can sell: ");
			Common.Append(stringBuilder, noOfAvailableItems.ToString(), tintAsValue: true);
			label2.ToolTip = stringBuilder.ToString();
		}
		else
		{
			label2.Text = "";
			label2.ToolTip = "";
		}
		label2.FitToText();
		label2.AlignRight(GetItemColumn(offerDemandX, isInCategoryPanel));
		UIComponent uIComponent2 = itemRow.FindChildById(UIComponent.DataControlID.Price);
		if (uIComponent2 != null)
		{
			Label label3 = uIComponent2 as Label;
			label3.Text = Common.GetPriceAsString(price);
			label3.FitToText();
			label3.AlignRight(GetItemColumn(priceX, isInCategoryPanel));
			if (isOwnedByUI)
			{
				label3.ToolTip = "The price we can sell 1 of these items for";
			}
			else if (CargoActionType == CargoActionTypes.Sell)
			{
				label3.ToolTip = "The price we can sell 1 of these items for";
			}
			else
			{
				label3.ToolTip = "The price we can buy 1 of these items for";
			}
		}
		float? bulkOfTradeItem = TradeManager.GetBulkOfTradeItem(entityType);
		UIComponent uIComponent3 = itemRow.FindChildById(UIComponent.DataControlID.Bulk);
		if (uIComponent3 != null)
		{
			Label obj = uIComponent3 as Label;
			obj.Text = (bulkOfTradeItem.HasValue ? Entity.GetBulkAsString(bulkOfTradeItem.Value) : "");
			obj.FitToText();
			obj.AlignRight(GetItemColumn(bulkX, isInCategoryPanel));
		}
		switch (settings.SortingSettings.SortedBy)
		{
		case BuySellPanelSettings.SortColumns.Name:
			itemRow.OrderByTag1 = (itemRow.Tag1 as EntityType).PluralName;
			break;
		case BuySellPanelSettings.SortColumns.Amount:
			itemRow.OrderByTag1 = 100 * noOfAvailableItems + num;
			break;
		case BuySellPanelSettings.SortColumns.Bulk:
			itemRow.OrderByTag1 = bulkOfTradeItem ?? 0f;
			break;
		case BuySellPanelSettings.SortColumns.Price:
			itemRow.OrderByTag1 = price;
			break;
		case BuySellPanelSettings.SortColumns.OfferDemand:
			itemRow.OrderByTag1 = 100 * num2 + demandedItems;
			break;
		}
	}

	private void PopulateNotAttainableIcons(HorizontalList list, TierOrAreaType unavailablePolicy)
	{
		list.BeginAddingEntries();
		string text = "";
		if (unavailablePolicy != null)
		{
			UIComponent uIComponent = ProductionOrderControl.AddOrGetIcon(unavailablePolicy, list, unattainableColor);
			text = "Not available. The following policy needs to be enacted first: " + unavailablePolicy.ToString();
			uIComponent.ToolTip = text;
		}
		else
		{
			list.TryRemoveEntry(IconKeys.NoPolicy);
		}
		list.EndAddingEntries();
	}

	public override void Hide()
	{
		base.Hide();
		sliderBeingDragged = null;
	}

	private void fillableBar_SliderMouseDown(object sender, EventArgs e)
	{
		SetUserChangedSliderState((FillableBar)sender);
	}

	private void SetUserChangedSliderState(FillableBar control)
	{
		sliderBeingDragged = control;
	}

	private bool GetIsPlayerOwnedLocation(EntityGroup owner)
	{
		if (owner != null && owner.GetAllegiance() == The.InGameUI.UIAllegiance)
		{
			return true;
		}
		return false;
	}

	private void fillableBar_SliderMouseUp(object sender, EventArgs e)
	{
		sliderBeingDragged = null;
		if (!GetData(out var data) || !ResolveEntityGroup(out var ownerOfItems, out var _, out var buyer))
		{
			return;
		}
		FillableBar obj = sender as FillableBar;
		EntityType entityType = (EntityType)obj.Tag1;
		int num = 0;
		int value = obj.Value;
		if (currentOrders == null)
		{
			currentOrders = new Dictionary<EntityType, List<EntityID>>();
		}
		bool isPlayerOwnedLocation = GetIsPlayerOwnedLocation(ownerOfItems);
		if (data.Contains(entityType))
		{
			decimal? agreedPrice = GetAgreedPrice(ownerOfItems, buyer, entityType);
			if (GetTradeItems(ownerOfItems, buyer, isPlayerOwnedLocation, entityType, out var items, agreedPrice, out var _, out var _, out var itemSourceIsInvalid))
			{
				num = ValidateAndCountOrders(entityType, items);
				if (!currentOrders.TryGetValue(entityType, out var value2))
				{
					value2 = new List<EntityID>();
					currentOrders.Add(entityType, value2);
				}
				int i = value - num;
				if (i > 0)
				{
					foreach (EntityID item in items)
					{
						if (!value2.Contains(item))
						{
							value2.Add(item);
							i--;
							if (i == 0)
							{
								break;
							}
						}
					}
				}
				else if (i < 0)
				{
					for (; i != 0; i++)
					{
						if (value2.Count <= 0)
						{
							break;
						}
						value2.RemoveAt(0);
					}
				}
			}
			else if (itemSourceIsInvalid)
			{
				return;
			}
		}
		UpdateTotals(ownerOfItems, buyer);
	}

	private void UpdateTotals(EntityGroup owner, EntityGroup npcBuyer)
	{
		float num = 0f;
		decimal amount = default(decimal);
		if (currentOrders != null)
		{
			foreach (KeyValuePair<EntityType, List<EntityID>> currentOrder in currentOrders)
			{
				if (currentOrder.Value.Count <= 0)
				{
					continue;
				}
				CargoActionTypes cargoActionType = CargoActionType;
				if ((uint)(cargoActionType - 2) <= 1u)
				{
					float? bulkOfTradeItem = TradeManager.GetBulkOfTradeItem(currentOrder.Key);
					if (bulkOfTradeItem.HasValue)
					{
						num += bulkOfTradeItem.Value * (float)currentOrder.Value.Count;
					}
					decimal? agreedPrice = GetAgreedPrice(owner, npcBuyer, currentOrder.Key);
					if (agreedPrice.HasValue)
					{
						amount += agreedPrice.Value * (decimal)currentOrder.Value.Count;
					}
				}
			}
		}
		lblTotalBulk.Text = Entity.GetBulkAsString(num);
		lblTotalBulk.FitToText();
		lblTotalBulk.AlignRight(bulkX);
		lblTotalBulk.Text += " BLK";
		lblTotalItemCost.Text = Common.MoneyAsString(amount);
		lblTotalItemCost.AlignRight(priceX);
	}

	private void btCancel_Click(UIComponent sender, EventArgs e)
	{
		CancelDialog();
	}

	private void CancelDialog()
	{
		Window.Hide();
		if (this.CancelClick != null)
		{
			this.CancelClick(this, null);
		}
	}
}
