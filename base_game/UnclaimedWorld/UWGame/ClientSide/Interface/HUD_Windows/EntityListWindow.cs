using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Commands;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Maps;
using WindowSystem;

namespace UWGame.ClientSide.Interface.HUD_Windows;

public class EntityListWindow : HUDWindow
{
	private enum OwnerAction
	{
		Discard,
		Claim
	}

	private Grid grdEntities;

	private EntityType entityType;

	private List<EntityID> listOfEntities;

	private int gridHeaderY = 9;

	private Label windowHeading;

	private static int WindowHeight = 20;

	private string hyperLinkToolTip = "LMB: Select the link target.\n RMB: Center on target.\n Double click: Select and center.";

	private int gridY;

	private const int hzlStatusWidth = 60;

	private static int locationX = 15;

	private static int statusX = locationX + 110 + 6;

	private static int salvageX = statusX + 60 + 9 - 4;

	private static int discardClaimX = salvageX + 21 + 9;

	private static int captionX = discardClaimX + 20 + 9;

	private static int spaceOnLeftAndBetween = 9;

	private const string selectTooltip = "Click to select";

	private const string selectTooltipOffsite = "Cannot be selected. The item is off site.";

	public EntityListWindow()
		: base(596, 180, hasSurface: true, hasCloseButton: true, isMovable: true, "HUD_window_base", hideWhenMouseExits: false, Level.EntityTypeInfo)
	{
		DisplayWindow.SetResizableArea(ResizeAreas.Top, isResizable: true);
		DisplayWindow.SetResizableArea(ResizeAreas.Bottom, isResizable: true);
		DisplayWindow.MinHeight = 60;
		DisplayWindow.ResizableBorderSize = 6;
		DisplayWindow.Resize += DisplayWindow_Resize;
		windowHeading = new Label(gui);
		Add(windowHeading);
		windowHeading.Init(Label.LabelType.HUDWindowHeader);
		windowHeading.X = locationX;
		windowHeading.Y = gridHeaderY - 1;
		windowHeading.ID = UIComponent.DataControlID.Name;
		windowHeading.FitToText();
		CreateGridHeader();
		grdEntities = new Grid(gui, ListBoxType.HUDAndLCD, Label.LabelType.HUDWindow);
		grdEntities.IsOuterGrid = true;
		grdEntities.ScrollBarEnabled = true;
		grdEntities.Y = gridY;
		grdEntities.FixedItemHeights = true;
		grdEntities.Width = DisplayWindow.Width - 18 + 9;
		grdEntities.ItemHeight = 22;
		grdEntities.CanGrowInHeight = false;
		grdEntities.Font = GUIManager.LCDandHUDBodyFontPath;
		grdEntities.Height = DisplayWindow.Height - grdEntities.Y - 10;
		Add(grdEntities);
		WorldPosition = null;
		SetVerticalPositions();
	}

	private void DisplayWindow_Resize(UIComponent sender)
	{
		SetVerticalPositions();
	}

	private void SetVerticalPositions()
	{
		grdEntities.Height = DisplayWindow.Height - grdEntities.Y - 10;
	}

	private void CreateGridHeader()
	{
		Label label = new Label(gui);
		label.Init(Label.LabelType.HUDWindow);
		label.ID = UIComponent.DataControlID.Location;
		label.Text = "LOCATION";
		label.FitToText();
		label.X = locationX;
		label.Y = windowHeading.Bottom + 3;
		Add(label);
		label = new Label(gui);
		label.Init(Label.LabelType.HUDWindow);
		label.ID = UIComponent.DataControlID.Status;
		label.Text = "STATUS";
		label.FitToText();
		label.X = statusX;
		label.Y = windowHeading.Bottom + 3;
		Add(label);
		gridY = label.Bottom + 8;
	}

	public void SetDataSource(EntityType entityType, List<EntityID> listOfEntities)
	{
		this.entityType = entityType;
		this.listOfEntities = listOfEntities;
	}

	private void FillHeader()
	{
		string text = "";
		if (entityType.ItemType != null)
		{
			text = "ITEM: ";
		}
		text += entityType.Name;
		windowHeading.Text = text;
	}

	public override void Refresh()
	{
		Populate();
	}

	public override void ShowOnPlayfield(int screenPosX, int screenPosY, bool avoidRightInterfaceArea = false)
	{
		base.ShowOnPlayfield(screenPosX, screenPosY, avoidRightInterfaceArea);
		DisplayWindow.BringToTop();
		WorldPosition = null;
		Fill();
	}

	public void PopulateAndShowOnPlayfield(UIComponent spawnButton, EntityType entityType, EntityGroup owner)
	{
		SetDataSource(entityType, The.InGameUI.GetExpedition().OwnedEntities.AllEntities[entityType]);
		int screenPosX = spawnButton.AbsolutePosition.X + 26;
		int y = spawnButton.AbsolutePosition.Y;
		ShowOnPlayfield(screenPosX, y, avoidRightInterfaceArea: false);
	}

	public void OpenNextToStockButton(UIComponent itemButton)
	{
		ShowOnPlayfield(itemButton.AbsolutePosition.X - DisplayWindow.Width - 2, itemButton.AbsolutePosition.Y - gridY, avoidRightInterfaceArea: false);
	}

	private void Populate()
	{
		grdEntities.BeginAddingEntries();
		UIComponent item = null;
		List<EntityID> invalidEntities = null;
		if (listOfEntities == null || listOfEntities.Count == 0)
		{
			grdEntities.Clear();
			return;
		}
		for (int num = listOfEntities.Count - 1; num >= 0; num--)
		{
			EntityID entityID = listOfEntities[num];
			if (GoalEvaluator.EntityDataResultCausesSkip(The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(entityID, out var data)))
			{
				if (invalidEntities == null)
				{
					invalidEntities = new List<EntityID>();
				}
				invalidEntities.Add(entityID);
			}
			else
			{
				GetAllowedActions(data, out var canBeSalvagedNow, out var canBeDiscardedNow, out var canBeClaimed);
				if (!grdEntities.TryGetEntry(entityID, out item))
				{
					AddItemControls(entityID, data, canBeSalvagedNow, canBeDiscardedNow, canBeClaimed, out item);
				}
				Entity.FindByID((EntityID)66L);
				UpdateItemControls(data, item, canBeSalvagedNow, canBeDiscardedNow, canBeClaimed);
			}
		}
		grdEntities.DeleteEntries((EntityID e) => listOfEntities.Contains(e) && (invalidEntities == null || !invalidEntities.Contains(e)));
		grdEntities.Sort((UIComponent r) => r.OrderByTag1, Grid.Sorting.Descending, (UIComponent r) => r.OrderByTag2, Grid.Sorting.Descending);
		grdEntities.EndAddingEntries();
	}

	private void AddItemControls(EntityID key, IKnownEntityData entityData, bool canBeSalvagedNow, bool canBeDiscarded, bool canBeClaimed, out UIComponent item)
	{
		bool num = entityData.EntityType.CanBeSalvagedDirectly();
		int x = locationX;
		item = new UIComponent(gui);
		Hyperlink control = new Hyperlink(gui, RenderType.Normal)
		{
			ID = UIComponent.DataControlID.Location,
			X = x,
			Tag1 = key,
			NormalColor = Color.White,
			DebugTag = "hlLocation" + grdEntities.Count
		};
		item.Add(control);
		x = statusX;
		HorizontalList control2 = new HorizontalList(The.InGameUI.gui, 7)
		{
			CenterItemsVertically = true,
			ID = UIComponent.DataControlID.StatusIcon,
			HorizontalSpacing = -2,
			X = x,
			Visible = true,
			Height = WindowHeight,
			MinHeight = WindowHeight,
			MaxHeight = WindowHeight,
			Width = 60
		};
		item.Add(control2);
		x = salvageX;
		if (num)
		{
			ImageButton imageButton = new ImageButton(gui);
			imageButton.Init(ImageButtonType.HUDSalvage);
			imageButton.Enabled = canBeSalvagedNow;
			imageButton.ID = UIComponent.DataControlID.SalvageAction;
			imageButton.X = x;
			imageButton.EventArgs = new EntityButtonEventArgs(key);
			imageButton.ToolTip = "Salvage the object: When breaking this apart, some parts will be retrieved, some will be lost. See the process tooltip for more info.";
			imageButton.Click += salvage_Click;
			item.Add(imageButton);
			imageButton.Y--;
			imageButton.MaxHeight = imageButton.Height - 2;
			ImageButton imageButton2 = new ImageButton(gui);
			imageButton2.Init(ImageButtonType.HUDPackDown);
			imageButton2.Enabled = canBeSalvagedNow;
			imageButton2.ID = UIComponent.DataControlID.PackingDownAction;
			imageButton2.X = x;
			imageButton2.EventArgs = new EntityButtonEventArgs(key);
			imageButton2.ToolTip = "Disassemble the object: All its parts will be retrieved. See the process tooltip for more info.";
			imageButton2.Click += salvage_Click;
			item.Add(imageButton2);
			imageButton2.Y--;
			imageButton2.MaxHeight = imageButton2.Height - 2;
			x = discardClaimX;
		}
		ImageButton imageButton3 = new ImageButton(gui);
		imageButton3.Init(canBeClaimed ? ImageButtonType.HUDClaim : ImageButtonType.HUDDiscard);
		imageButton3.ID = UIComponent.DataControlID.DiscardClaim;
		imageButton3.X = x;
		imageButton3.ToolTip = (canBeClaimed ? "Claim this object which is not owned by anyone" : "Discard the object");
		imageButton3.EventArgs = new EntityButtonEventArgs(key);
		imageButton3.Click += owner_Click;
		imageButton3.MaxHeight = imageButton3.Height - 2;
		imageButton3.DebugTag = "ClaimButton" + grdEntities.Count;
		item.Add(imageButton3);
		imageButton3.Y--;
		x = ((x == salvageX) ? discardClaimX : captionX);
		ItemTypeButtonEventArgs eventArgs = new ItemTypeButtonEventArgs(entityType);
		TextButton textButton = new TextButton(gui);
		textButton.Text = "SELECT";
		textButton.ID = UIComponent.DataControlID.Selector;
		textButton.Init(TextButton.TextButtonType.HUD);
		textButton.Tag1 = entityData.EntityID;
		textButton.X = x;
		textButton.Y = imageButton3.Y;
		textButton.ScaleToFitText();
		textButton.Height += 2;
		textButton.Click += btName_Click;
		textButton.Visible = true;
		textButton.EventArgs = eventArgs;
		item.Add(textButton);
		grdEntities.AddEntry(key, item);
	}

	private void btName_Click(UIComponent sender, EventArgs e)
	{
		EntityID? entityID = sender.Tag1 as EntityID?;
		if (entityID.HasValue)
		{
			The.InGameUI.SelectEntity(entityID.Value);
		}
	}

	private void UpdateIconList(IKnownEntityData entityWithStatus, HorizontalList hzlStatus)
	{
		IHasExposedProperties hasExposedProperties = null;
		hasExposedProperties = entityWithStatus;
		foreach (PresentationTypeCategory finalPresentationTypeCategory in GameData.Instance.CustomStatusIconData.FinalPresentationTypeCategories)
		{
			int? numberOfItems = null;
			PresentationTypeCategoryProcessor.DisplayCategory(finalPresentationTypeCategory, hasExposedProperties, hzlStatus, ref numberOfItems);
		}
		hzlStatus.Width = ((hzlStatus.Width < 60) ? 60 : hzlStatus.Width);
	}

	private static void UpdateDiscardClaimButton(ImageButton bt, bool canBeDiscarded, bool canBeClaimed)
	{
		if (canBeDiscarded || canBeClaimed)
		{
			if (canBeDiscarded)
			{
				bt.Init(ImageButtonType.HUDDiscard);
				bt.Tag1 = OwnerAction.Discard;
				bt.ToolTip = "Discard the item";
			}
			else
			{
				bt.Init(ImageButtonType.HUDClaim);
				bt.Tag1 = OwnerAction.Claim;
				bt.ToolTip = "Claim the item not owned by anyone";
			}
			bt.Visible = true;
		}
		else
		{
			bt.Visible = false;
		}
	}

	private void UpdateItemControls(IKnownEntityData entityData, UIComponent item, bool canBeSalvagedNow, bool canBeDiscarded, bool canBeClaimed)
	{
		GetLocation(entityData, out var partOrContainerData, out var stockpile);
		Hyperlink hyperlink = (Hyperlink)item.FindChildById(UIComponent.DataControlID.Location);
		hyperlink.ToolTip = hyperLinkToolTip;
		if (partOrContainerData != null)
		{
			hyperlink.MaxWidth = 110;
			hyperlink.TargetEntityID = (uint)partOrContainerData.EntityID;
			if (InGameInterface.CanSelectEntity(partOrContainerData))
			{
				hyperlink.Enabled = true;
				if (entityData.ParentEntityID.HasValue)
				{
					hyperlink.ToolTip = "*" + partOrContainerData.GetDisplayName() + " \n*:part of this item. \n \n" + hyperlink.ToolTip;
					hyperlink.Text = "*" + partOrContainerData.GetDisplayName();
				}
				else
				{
					hyperlink.ToolTip = hyperLinkToolTip;
					hyperlink.Text = partOrContainerData.GetDisplayName();
				}
			}
			else
			{
				hyperlink.Text = partOrContainerData.GetDisplayName();
				hyperlink.Enabled = false;
				hyperlink.ToolTip = "Cannot be selected. The item is off site.";
			}
		}
		else
		{
			if (stockpile != null)
			{
				hyperlink.Text = "Stockpile " + stockpile.GetDisplayName();
			}
			else
			{
				hyperlink.Text = entityData.MapPosition.ToString();
			}
			hyperlink.TargetEntityID = (uint)entityData.EntityID;
			hyperlink.TargetMapPosition = entityData.MapPosition;
		}
		HorizontalList horizontalList = item.FindChildById(UIComponent.DataControlID.StatusIcon) as HorizontalList;
		UpdateIconList(entityData, horizontalList);
		int num = ((horizontalList.Width == 60) ? 60 : (horizontalList.Width - 60));
		ImageButton imageButton = item.FindChildById(UIComponent.DataControlID.SalvageAction) as ImageButton;
		ImageButton imageButton2 = item.FindChildById(UIComponent.DataControlID.PackingDownAction) as ImageButton;
		if (imageButton != null)
		{
			ImageButton imageButton3;
			if (entityData.EntityType.NonLivingType.SalvageProcessType.IsSalvageWithoutWaste)
			{
				imageButton3 = imageButton2;
				imageButton.Visible = false;
			}
			else
			{
				imageButton3 = imageButton;
				imageButton2.Visible = false;
			}
			imageButton3.Visible = true;
			if (!Salvage.SalvageJobExists(entityData))
			{
				imageButton3.Enabled = canBeSalvagedNow;
			}
			else
			{
				imageButton3.Enabled = false;
			}
			imageButton3.X = ((num == 60) ? salvageX : (salvageX + num));
		}
		ImageButton imageButton4 = (ImageButton)item.FindChildById(UIComponent.DataControlID.DiscardClaim);
		UpdateDiscardClaimButton(imageButton4, canBeDiscarded, canBeClaimed);
		TextButton textButton = (TextButton)item.FindChildById(UIComponent.DataControlID.Selector);
		if (entityData.IsCompleted())
		{
			textButton.LabelColor = Color.White;
		}
		else
		{
			textButton.LabelColor = Color.Orange;
		}
		if (InGameInterface.CanSelectEntity(entityData))
		{
			textButton.Enabled = true;
			textButton.ToolTip = "Click to select";
		}
		else
		{
			textButton.Enabled = false;
			textButton.ToolTip = "Cannot be selected. The item is off site.";
		}
		if (imageButton != null)
		{
			if (num == 60)
			{
				imageButton4.X = discardClaimX;
				textButton.X = captionX;
			}
			else
			{
				imageButton4.X = discardClaimX + num;
				textButton.X = captionX + num;
			}
		}
		else if (num == 60)
		{
			imageButton4.X = salvageX;
			textButton.X = discardClaimX - 1;
		}
		else
		{
			imageButton4.X = salvageX + num;
			textButton.X = discardClaimX + num - 1;
		}
		item.OrderByTag1 = (entityData.IsCompleted() ? 1 : 0);
		item.OrderByTag2 = hyperlink.Text;
		ResizeWindow(textButton.Right + spaceOnLeftAndBetween);
	}

	private void Fill()
	{
		FillHeader();
		Populate();
	}

	private void owner_Click(UIComponent sender, EventArgs e)
	{
		EntityID entity = (e as EntityButtonEventArgs).Entity;
		IKnownEntityData data;
		EntityResult knownData = The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(entity, out data);
		if (!LookUpOwners.ResolveEntityOwner(data, out EntityGroup ownedEntities))
		{
			return;
		}
		if (ownedEntities != null && (knownData == EntityResult.Destroyed || knownData == EntityResult.EntityStatusIsNowUnknown))
		{
			ownedEntities.DeleteEntity(entity, entityType);
			data.OwnedBy = null;
			return;
		}
		if ((OwnerAction)(sender as ImageButton).Tag1 == OwnerAction.Discard)
		{
			if (ownedEntities != null)
			{
				ownedEntities.DeleteEntity(entity, entityType);
				data.OwnedBy = null;
			}
		}
		else if (data is Entity entity2)
		{
			Expedition firstPlayerExpedition = The.Sim.PlaySite.GetFirstPlayerExpedition();
			entity2.ChangeOwnership(firstPlayerExpedition);
		}
		Populate();
	}

	private void salvage_Click(UIComponent sender, EventArgs e)
	{
		EntityID entity = (e as EntityButtonEventArgs).Entity;
		ImageButton imageButton = sender as ImageButton;
		if (!Salvage.SalvageJobExists(Entity.FindByID(entity)))
		{
			HUDEntityContextMenu.TrySalvage(entity);
			imageButton.Enabled = false;
		}
	}

	private void GetLocation(IKnownEntityData entityData, out IKnownEntityData partOrContainerData, out Zone stockpile)
	{
		stockpile = null;
		partOrContainerData = null;
		EntityID entityID = entityData.EntityID;
		EntityID? containedBy = entityData.ContainedBy;
		List<EntityID> list = null;
		if (entityData.ParentEntityID.HasValue)
		{
			if (GoalEvaluator.EntityDataResultCausesSkip(The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(entityData.ParentEntityID.Value, out partOrContainerData)))
			{
				if (list == null)
				{
					list = new List<EntityID>();
				}
				list.Add(entityID);
			}
		}
		else if (containedBy.HasValue)
		{
			if (GoalEvaluator.EntityDataResultCausesSkip(The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(containedBy.Value, out partOrContainerData)))
			{
				if (list == null)
				{
					list = new List<EntityID>();
				}
				list.Add(entityID);
			}
		}
		else
		{
			partOrContainerData = null;
			stockpile = The.Map.GetTile(entityData.MapPosition.Value).GetStockpileZone(The.InGameUI.UIAllegiance);
		}
	}

	private void ResizeWindow(int width)
	{
		int num = 15;
		int right = DisplayWindow.Right;
		width += num;
		grdEntities.ScrollBar.X = width - num - 6;
		DisplayWindow.Width = width;
		DisplayWindow.X = right - width;
	}

	public static void GetAllowedActions(IKnownEntityData entityData, out bool canBeSalvagedNow, out bool canBeDiscardedNow, out bool canBeClaimed)
	{
		CanBeSalvagedOrDiscarded(entityData, out canBeSalvagedNow, out canBeDiscardedNow);
		canBeClaimed = CanBeClaimed(entityData);
	}

	public static void CanBeSalvagedOrDiscarded(IKnownEntityData entityData, out bool canBeSalvagedNow, out bool canBeDiscardedNow)
	{
		canBeDiscardedNow = false;
		canBeSalvagedNow = false;
		if (entityData.OwnedBy.HasValue && The.InGameUI.UIAllegiance.HumanActivities != null && LookUpOwners.ResolveEntityOwner(entityData, out EntityGroup ownedEntities) && ownedEntities != null && ownedEntities.IsOwnedByAllegiance(The.InGameUI.UIAllegiance))
		{
			if (entityData.UpgradeFor.HasValue)
			{
				canBeDiscardedNow = false;
			}
			else
			{
				canBeDiscardedNow = true;
			}
			if (entityData.EntityType.CanBeSalvagedDirectly() && !Salvage.SalvageJobExists(entityData))
			{
				canBeSalvagedNow = true;
			}
		}
	}

	public static bool CanBeClaimed(IKnownEntityData entityData)
	{
		if (!entityData.OwnedBy.HasValue && entityData is Entity)
		{
			if (entityData.UpgradeFor.HasValue)
			{
				return false;
			}
			if (((Entity)entityData).NonLivingEntity != null)
			{
				return true;
			}
			return false;
		}
		return false;
	}
}
