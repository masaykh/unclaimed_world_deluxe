using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface.BuyAndSell;
using UWGame.ClientSide.Interface.Missions;
using UWGame.ClientSide.Interface.Personnel;
using UWGame.SimSide;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Communication;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Overland.Missions.Templates;
using UWGame.SimSide.Policies;
using UWGame.SimSide.Snapshots;
using WindowSystem;

namespace UWGame.ClientSide.Interface.World_map;

public class SiteWindow : UIComponent
{
	private Grid grdOuter;

	private Site site;

	private EntityGroupID? otherPartyID;

	private TextArea taDescription;

	private Label lblHeader;

	private int titleY = 8;

	private int sideMargin = 16;

	private Label windowHeading;

	private GUIManager gui;

	private int captionX = 16;

	private int terminalX = 143;

	private MovableArea movableArea;

	private TravelLocation? dialogSourceLocation;

	private const string noCommTooltip = "No communication with this allegiance";

	public event Action<TravelLocation> TerminalSelected;

	public event Action ChildDialogDisplayed;

	public event Action ChildDialogClosed;

	private void OnStartAnimating(UIComponent sender)
	{
		base.IsAnimating = true;
	}

	private void OnEndAnimating(UIComponent sender)
	{
		base.IsAnimating = false;
	}

	public SiteWindow(GUIManager gui, int width, int height)
		: base(gui)
	{
		this.gui = gui;
		Box box = new Box(gui);
		box.CornerSize = 20;
		box.SetSkinLocation(SkinState.Normal, gui.GUISpriteSheet.GetSourceRectangle("HUD_window_base"));
		box.Width = width;
		box.Height = height;
		Add(box);
		Width = width;
		Height = height;
		windowHeading = new Label(gui);
		Add(windowHeading);
		windowHeading.Init(Label.LabelType.HUDWindowHeader);
		windowHeading.X = sideMargin;
		windowHeading.Y = titleY;
		windowHeading.Text = "";
		windowHeading.FitToText();
		windowHeading.Visible = true;
		ImageButton imageButton = new ImageButton(gui);
		Add(imageButton);
		imageButton.Init(ImageButtonType.HUDClose);
		imageButton.X = Width - 25;
		imageButton.Y = titleY;
		imageButton.Visible = true;
		imageButton.Click += closeButton_Click;
		imageButton.ToolTip = "Close this window";
		imageButton.CheckedMode = CheckedModes.CannotBeChecked;
		imageButton.ZOrder = 1f;
		taDescription = new TextArea(gui, ListBoxType.HUDAndLCD);
		taDescription.RenderType = RenderType.CRTAndLCD;
		taDescription.Init(Label.LabelType.HUDWindow);
		taDescription.Width = width - 24;
		Add(taDescription);
		taDescription.CanGrowInHeight = false;
		taDescription.ScrollBarEnabled = true;
		taDescription.Y = windowHeading.Bottom + 6;
		taDescription.X = sideMargin;
		taDescription.Height = 38;
		movableArea = new MovableArea(gui);
		base.Add(movableArea);
		movableArea.StartMoving += OnStartAnimating;
		movableArea.EndMoving += OnEndAnimating;
		movableArea.ZOrder = 0.1f;
		movableArea.Width = Width;
		movableArea.Height = Height;
		CreateGridHeader();
		grdOuter = new Grid(gui, ListBoxType.HUDAndLCD, Label.LabelType.HUDWindow);
		grdOuter.IsOuterGrid = true;
		grdOuter.Y = lblHeader.Bottom + 8;
		grdOuter.FixedItemHeights = true;
		grdOuter.Width = width;
		grdOuter.ItemHeight = 22;
		grdOuter.Font = GUIManager.LCDandHUDBodyFontPath;
		grdOuter.Height = 85;
		grdOuter.Visible = true;
		grdOuter.ZOrder = 1f;
		grdOuter.CanGrowInHeight = false;
		grdOuter.ScrollBar.ZOrder = 1f;
		grdOuter.ScrollBar.X = width - 20;
		Add(grdOuter);
	}

	private void CreateGridHeader()
	{
		int y = taDescription.Bottom + 3;
		lblHeader = new Label(gui);
		Add(lblHeader);
		lblHeader.Init(Label.LabelType.HUDWindow);
		lblHeader.Text = "EXPEDITION";
		lblHeader.FitToText();
		lblHeader.X = sideMargin;
		lblHeader.Y = y;
		lblHeader.Visible = true;
		lblHeader = new Label(gui);
		Add(lblHeader);
		lblHeader.Init(Label.LabelType.HUDWindow);
		lblHeader.Text = "TERMINAL";
		lblHeader.FitToText();
		lblHeader.X = terminalX;
		lblHeader.Y = y;
		lblHeader.Visible = true;
	}

	private void closeButton_Click(UIComponent sender, EventArgs e)
	{
		Hide();
	}

	public void Fill(Site site, EntityGroupID? otherPartyID)
	{
		this.site = site;
		this.otherPartyID = otherPartyID;
		windowHeading.Text = "SITE: " + site.Name;
		taDescription.Text = site.Description;
	}

	private void Populate()
	{
		grdOuter.Y = lblHeader.Bottom + 8;
		grdOuter.BeginAddingEntries();
		List<object> list = null;
		SharedKnowledge sharedKnowledge = The.InGameUI.UIAllegiance.SharedKnowledge;
		foreach (Allegiance allegiance3 in site.Allegiances)
		{
			if (allegiance3.RepresentativeEntityType.Person == null)
			{
				continue;
			}
			foreach (Expedition expedition2 in allegiance3.Expeditions)
			{
				foreach (KeyValuePair<TerminalType.TypesOfTerminal, List<EntityID>> terminal in expedition2.OwnedEntities.Terminals)
				{
					for (int num = terminal.Value.Count - 1; num >= 0; num--)
					{
						EntityID entityID = terminal.Value[num];
						string key = TravelLocation.GetKey(site.ID, allegiance3.ID, expedition2.ID, entityID);
						if (GoalEvaluator.HandleOwnerDataResult(sharedKnowledge, entityID, sharedKnowledge.AllKnownEntities, out var entityData))
						{
							AddUpdateRow(key, allegiance3, expedition2, entityData);
						}
					}
				}
			}
		}
		foreach (KeyValuePair<object, UIComponent> item in grdOuter.EntriesByKey)
		{
			TravelLocation travelLocation = (TravelLocation)item.Value.FindChildById(DataControlID.Selector).Tag1;
			if (LookUp<Site, SiteID>.FindByID((SiteID)travelLocation.SiteID) == null)
			{
				Common.AddToList(ref list, item.Key);
				continue;
			}
			if (travelLocation.AllegianceID.HasValue)
			{
				Allegiance allegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)travelLocation.AllegianceID.Value);
				if (allegiance == null || !site.Allegiances.Contains(allegiance))
				{
					Common.AddToList(ref list, item.Key);
					continue;
				}
			}
			if (travelLocation.ExpeditionID.HasValue)
			{
				Allegiance allegiance2 = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)travelLocation.AllegianceID.Value);
				Expedition expedition = LookUp<Expedition, ExpeditionID>.FindByID((ExpeditionID)travelLocation.ExpeditionID.Value);
				if (expedition == null || allegiance2 == null || !allegiance2.Expeditions.Contains(expedition))
				{
					Common.AddToList(ref list, item.Key);
					continue;
				}
			}
			if (travelLocation.TerminalEntityID.HasValue)
			{
				LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)travelLocation.AllegianceID.Value);
				LookUp<Expedition, ExpeditionID>.FindByID((ExpeditionID)travelLocation.ExpeditionID.Value);
				if (!GoalEvaluator.HandleOwnerDataResult(sharedKnowledge, (EntityID)travelLocation.TerminalEntityID.Value, sharedKnowledge.AllKnownEntities, out var _))
				{
					list.Add(item.Key);
				}
			}
		}
		if (list != null)
		{
			foreach (object item2 in list)
			{
				grdOuter.RemoveEntry(item2);
			}
		}
		grdOuter.Sort((UIComponent r) => r.OrderByTag1, Grid.Sorting.Ascending, (UIComponent r) => r.OrderByTag2, Grid.Sorting.Ascending);
		string value = "";
		foreach (UIComponent entry in grdOuter.Entries)
		{
			string text = (entry.FindChildById(DataControlID.Caption) as Label).Text;
			if (text.Equals(value))
			{
				(entry.FindChildById(DataControlID.Caption) as Label).Text = "";
			}
			value = text;
		}
		grdOuter.EndAddingEntries();
	}

	private void AddUpdateRow(string travelLocationKey, Allegiance allegiance, Expedition expedition, IKnownEntityData terminal)
	{
		if (!grdOuter.TryGetEntry(travelLocationKey, out var item))
		{
			item = AddItemRow(travelLocationKey, site, allegiance, expedition, terminal);
		}
		UpdateRow(item, allegiance);
	}

	public new void Refresh()
	{
		Populate();
	}

	private UIComponent AddItemRow(string key, Site site, Allegiance allegiance, Expedition expedition, IKnownEntityData terminal)
	{
		UIComponent uIComponent = new UIComponent(gui);
		TravelLocation travelLocation = new TravelLocation(allegiance, (long)expedition.ID, (long?)terminal?.EntityID);
		grdOuter.ItemHeight = 22;
		Label label = new Label(gui);
		label.Init(Label.LabelType.HUDWindow);
		label.ID = DataControlID.Caption;
		label.X = captionX;
		label.Visible = true;
		label.MaxWidth = 123;
		uIComponent.Add(label);
		Label label2 = new Label(gui);
		label2.Init(Label.LabelType.HUDWindow);
		label2.X = terminalX;
		label2.Visible = true;
		label2.ID = DataControlID.Transport;
		uIComponent.Add(label2);
		int num = 308;
		TextButton textButton = new TextButton(gui);
		textButton.Init(TextButton.TextButtonType.HUD);
		textButton.Visible = true;
		textButton.Text = "SELECT";
		textButton.Tag1 = travelLocation;
		textButton.ZOrder = 1f;
		textButton.ScaleWidthToFitText();
		textButton.X = num - textButton.Width;
		textButton.ID = DataControlID.Selector;
		uIComponent.Add(textButton);
		textButton.Click += tbSelect_Click;
		ImageButton imageButton = new ImageButton(gui);
		imageButton.Init(ImageButtonType.HUDPrices);
		imageButton.ID = DataControlID.WillingToBuy;
		imageButton.X = num;
		uIComponent.Add(imageButton);
		imageButton.Click += tbToBuy_Click;
		imageButton.Tag1 = travelLocation;
		imageButton.NormalColor = GameData.Instance.GUIConstants.SellingButtonTint;
		ImageButton imageButton2 = new ImageButton(gui);
		imageButton2.Init(ImageButtonType.HUDPrices);
		imageButton2.ID = DataControlID.GoodsForSale;
		imageButton2.X = imageButton.Right;
		uIComponent.Add(imageButton2);
		imageButton2.Click += tbForSale_Click;
		imageButton2.Tag1 = travelLocation;
		imageButton2.NormalColor = GameData.Instance.GUIConstants.BuyingButtonTint;
		ImageButton imageButton3 = new ImageButton(gui);
		imageButton3.Init(ImageButtonType.HUDPeople);
		imageButton3.ID = DataControlID.Personnel;
		imageButton3.X = imageButton2.Right;
		uIComponent.Add(imageButton3);
		imageButton3.Click += tbPeople_Click;
		imageButton3.Tag1 = travelLocation;
		grdOuter.AddEntry(key, uIComponent);
		return uIComponent;
	}

	private void tbForSale_Click(UIComponent sender, EventArgs e)
	{
		if (((TravelLocation)sender.Tag1).ResolveLocation(The.InGameUI.UIAllegiance.SharedKnowledge, out var _, out var _, out var expedition, out var _))
		{
			EntityGroup buyer = LookUp<EntityGroup, EntityGroupID>.FindByID(otherPartyID);
			EntityGroup ownedEntities = expedition.OwnedEntities;
			BuySellPanel.BuySellDialogMode mode = BuySellPanel.BuySellDialogMode.ViewBuyAtNPC;
			ShowDialog(sender, mode, buyer, ownedEntities);
		}
	}

	private void tbToBuy_Click(UIComponent sender, EventArgs e)
	{
		if (((TravelLocation)sender.Tag1).ResolveLocation(The.InGameUI.UIAllegiance.SharedKnowledge, out var _, out var _, out var expedition, out var _))
		{
			EntityGroup ownedEntities = expedition.OwnedEntities;
			EntityGroup seller = null;
			BuySellPanel.BuySellDialogMode mode = BuySellPanel.BuySellDialogMode.ViewSellAtNPC;
			ShowDialog(sender, mode, ownedEntities, seller);
		}
	}

	private void ShowDialog(UIComponent sender, BuySellPanel.BuySellDialogMode mode, EntityGroup buyer, EntityGroup seller)
	{
		TravelLocation value = (TravelLocation)sender.Tag1;
		dialogSourceLocation = value;
		if (this.ChildDialogDisplayed != null)
		{
			this.ChildDialogDisplayed();
		}
		BuySellPanel buySellDialog = The.InGameUI.BuySellDialog;
		buySellDialog.CancelClick += BuySellDialog_CancelClick;
		buySellDialog.Window.Close += ChildDialogWindow_Close;
		The.InGameUI.FillAndShowBuySellDialog(sender.AbsolutePosition, mode, null, CanTrade, seller, buyer, GetItemsForSale);
	}

	private void UpdateRow(UIComponent item, Allegiance allegiance)
	{
		UIComponent uIComponent = item.FindChildById(DataControlID.Selector);
		TextButton textButton = (TextButton)uIComponent;
		TravelLocation travelLocation = (TravelLocation)textButton.Tag1;
		uIComponent = item.FindChildById(DataControlID.Caption);
		if (uIComponent != null)
		{
			Label label = (Label)uIComponent;
			label.Text = allegiance.Name;
			if (label.TextWidth > label.MaxWidth)
			{
				label.ToolTip = allegiance.Name;
			}
			else
			{
				label.ToolTip = "";
			}
		}
		uIComponent = item.FindChildById(DataControlID.Transport);
		if (uIComponent != null)
		{
			Label label2 = (Label)uIComponent;
			if (!travelLocation.TerminalEntityID.HasValue)
			{
				label2.Text = "No terminal";
			}
			else
			{
				Entity entity = Entity.FindByID((EntityID)travelLocation.TerminalEntityID.Value);
				label2.Text = entity.GetDisplayName();
			}
		}
		ImageButton imageButton = (ImageButton)item.FindChildById(DataControlID.WillingToBuy);
		ImageButton imageButton2 = (ImageButton)item.FindChildById(DataControlID.GoodsForSale);
		ImageButton imageButton3 = (ImageButton)item.FindChildById(DataControlID.Personnel);
		CommunicationMethod? workingMethod;
		bool flag = Communicates.IsInCommunicationRange(The.InGameUI.UIAllegiance, allegiance, out workingMethod);
		if (flag)
		{
			imageButton2.Enabled = true;
			imageButton2.ToolTip = "View prices and goods for sale at this location";
			imageButton.Enabled = true;
			imageButton.ToolTip = "View prices for goods this location is willing to buy";
			imageButton3.Enabled = true;
			imageButton3.ToolTip = "See people at this location who are interested in migrating";
		}
		else
		{
			imageButton2.Enabled = false;
			imageButton2.ToolTip = "No communication with this allegiance";
			imageButton.Enabled = false;
			imageButton.ToolTip = "No communication with this allegiance";
			imageButton3.Enabled = false;
			imageButton3.ToolTip = "No communication with this allegiance";
		}
		if (allegiance == The.InGameUI.UIAllegiance)
		{
			imageButton.Visible = false;
		}
		else
		{
			imageButton.Visible = true;
		}
		if (this.TerminalSelected != null)
		{
			textButton.Visible = true;
			if (The.InGameUI.CreateMissionPanel.CanSelectLocation(travelLocation, out var isAlreadySelected, out var wrongTerminalType))
			{
				if (flag)
				{
					if (The.InGameUI.CreateMissionPanel.HasTransportationIfStart(travelLocation))
					{
						textButton.Enabled = true;
						textButton.ToolTip = "Select this location";
					}
					else
					{
						textButton.Enabled = false;
						textButton.ToolTip = "This cannot be selected as a starting location since there is no transportation currently available from this site.";
					}
				}
				else
				{
					textButton.Enabled = false;
					textButton.ToolTip = "No communication with this allegiance";
				}
			}
			else
			{
				textButton.Enabled = false;
				if (isAlreadySelected)
				{
					textButton.ToolTip = "This location is already selected";
				}
				else if (wrongTerminalType)
				{
					allegiance.SharedKnowledge.GetKnownData((EntityID)travelLocation.TerminalEntityID.Value, out var data);
					if (data != null)
					{
						textButton.ToolTip = TravelActionTemplate.GetWrongTerminalError(data);
					}
				}
				else
				{
					textButton.ToolTip = "";
				}
			}
		}
		else
		{
			textButton.Visible = false;
		}
		item.OrderByTag1 = Expedition.FindByID((ExpeditionID)travelLocation.ExpeditionID.Value).Name;
		item.OrderByTag2 = ((!travelLocation.TerminalEntityID.HasValue) ? "" : Entity.FindByID((EntityID)travelLocation.TerminalEntityID.Value).Name);
	}

	private void tbPeople_Click(UIComponent sender, EventArgs e)
	{
		TravelLocation value = (TravelLocation)sender.Tag1;
		dialogSourceLocation = value;
		ShowPersonnelDialog(sender.AbsolutePosition);
		if (this.ChildDialogDisplayed != null)
		{
			this.ChildDialogDisplayed();
		}
	}

	private void ShowPersonnelDialog(Point absolutePosition)
	{
		PersonnelDialog personnelDialog = The.InGameUI.PersonnelDialog;
		personnelDialog.CancelClick += PersonnelDialog_CancelClick;
		personnelDialog.Window.Close += ChildDialogWindow_Close;
		personnelDialog.FillAndShow(GetPeople, absolutePosition, showSelectors: false, showMigrateRisk: false);
	}

	private void ChildDialogWindow_Close(UIComponent sender)
	{
		((Window)sender).Close -= ChildDialogWindow_Close;
		if (this.ChildDialogClosed != null)
		{
			this.ChildDialogClosed();
		}
	}

	private void ShowBuySellDialog(Point absolutePosition, CargoActionTypes selectedAction, Dictionary<EntityType, List<EntityID>> currentOrders, Allegiance allegiance, Expedition expedition, EntityGroup buyer)
	{
	}

	private bool CanTrade(EntityType entityType, out TierOrAreaType tierPolicy)
	{
		tierPolicy = null;
		return true;
	}

	private List<IKnownEntityData> GetPeople()
	{
		return CreateMissionPanel.GetPeople(dialogSourceLocation);
	}

	private List<EntityID> GetItemsForSale(EntityType type, out bool sourceIsInvalid)
	{
		return CreateMissionPanel.GetItemsForSale(type, dialogSourceLocation.Value, null, out sourceIsInvalid);
	}

	private void BuySellDialog_CancelClick(object sender, EventArgs e)
	{
		ResetAfterBuySellDialog();
	}

	private void PersonnelDialog_CancelClick(object sender, EventArgs e)
	{
		ResetAfterPersonnelDialog();
	}

	public void ResetAfterBuySellDialog()
	{
		The.InGameUI.BuySellDialog.CancelClick -= BuySellDialog_CancelClick;
		dialogSourceLocation = null;
	}

	public void ResetAfterPersonnelDialog()
	{
		The.InGameUI.PersonnelDialog.CancelClick -= PersonnelDialog_CancelClick;
		dialogSourceLocation = null;
	}

	private void tbSelect_Click(UIComponent sender, EventArgs e)
	{
		if (this.TerminalSelected != null)
		{
			this.TerminalSelected((TravelLocation)sender.Tag1);
		}
	}

	public void Hide()
	{
		Visible = false;
		base.Y = 1000;
	}

	public void Show()
	{
		Visible = true;
		Populate();
	}

	private static void AddTravelLocation(ComboBox cb, ref bool hasAddedSiteName, ref bool hasAddedAllegianceName, ref bool hasAddedExpeditionName, Site site, Allegiance allegiance, Expedition expedition, Entity terminal, TravelLocation? excludeLocation)
	{
		if (!excludeLocation.HasValue || excludeLocation.Value.AllegianceID.Value != (long)allegiance.ID || excludeLocation.Value.ExpeditionID.Value != (long)expedition.ID || ((terminal != null || excludeLocation.Value.TerminalEntityID.HasValue) && excludeLocation.Value.TerminalEntityID != (long?)terminal.ID))
		{
			long? terminalEntityID = null;
			if (terminal != null)
			{
				terminalEntityID = (long)terminal.ID;
			}
			TravelLocation travelLocation = new TravelLocation(allegiance, (long)expedition.ID, terminalEntityID);
			string text = "";
			cb.AddEntry(travelLocation, text);
		}
	}
}
