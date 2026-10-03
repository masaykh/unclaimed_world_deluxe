using System;
using System.Collections.Generic;
using System.Linq;
using UWGame.ClientSide.Interface.Controls;
using UWGame.ClientSide.Interface.HUD_Windows;
using UWGame.ClientSide.Interface.Inventory;
using UWGame.Control.Commands;
using UWGame.SimSide.Commands;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Policy;

public class WeaponsPage : TabPagePanel
{
	private Grid grid;

	private Expedition expedition;
	private int gridHeaderY = 40;

	/// <summary>HuntingMod's policy switch; null when the mod does not offer it.</summary>
	private CheckBox cbAutoclaim;

	private string autoclaimTooltip = "Animals that die within the camp's hunting radius with nobody to claim them - killed by our people, dogs or sentries outside a hunt, by wildlife, or starved - become ours and are hauled in, as if claimed by hand. Bodies of people are never claimed this way.";

	private string availableTooltip = "The amount of ammunition available";

	private string useVerminTooltip = "Select whether we allow this ammunition type to be used against vermin, or if it can only be used against threats and other targets.";

	public WeaponsPage(TabControl parent)
		: base(parent)
	{
		GUIManager gUIManager = parent.guiManager;
		parent.AddTabPage(this, "WEAPONS POLICY", "Set/view weapon policies");
		Label label = new Label(guiManager);
		label.Init(Label.LabelType.LCDBigHeaderBanner);
		Add(label);
		label.X = 0;
		label.Y = 0;
		label.Text = "AMMO USAGE";
		label.ToolTip = "Specify what the ammunition may be used for";
		label.Width = Width;
		if (UWGame.Mods.HuntingMod.OffersAutoclaim)
		{
			cbAutoclaim = new CheckBox(gUIManager);
			Add(cbAutoclaim);
			cbAutoclaim.Text = "CLAIM ANIMALS THAT DIE IN CAMP";
			cbAutoclaim.Init(CheckBoxType.LCD);
			cbAutoclaim.SwitchStateOnClick = true;
			cbAutoclaim.X = 22;
			cbAutoclaim.Y = gridHeaderY;
			cbAutoclaim.FitToText();
			cbAutoclaim.ToolTip = autoclaimTooltip;
			cbAutoclaim.Click += cbAutoclaim_Click;
			gridHeaderY += 30;
		}
		grid = new Grid(gUIManager, ListBoxType.HUDAndLCD, Label.LabelType.HUDWindow);
		grid.IsOuterGrid = true;
		grid.X = 0;
		grid.Y = gridHeaderY + 26;
		grid.FixedItemHeights = true;
		grid.Width = Width;
		grid.ScrollBarEnabled = true;
		grid.ItemHeight = 27;
		grid.CanGrowInHeight = true;
		grid.Font = GUIManager.LCDandHUDBodyFontPath;
		Add(grid);
		CreateGridHeader();
		SetHeight();
	}

	private void SetHeight()
	{
		Height = grid.Bottom + 6;
	}

	private void CreateGridHeader()
	{
		Label label = new Label(guiManager);
		Add(label);
		label.Init(Label.LabelType.LCDHeadingBrown);
		label.Text = "Type";
		label.ToolTip = "The ammunition type";
		label.FitToText();
		label.X = 22;
		label.Y = gridHeaderY;
		Label label2 = new Label(guiManager);
		Add(label2);
		label2.Init(Label.LabelType.LCDHeadingBlue);
		label2.Text = "Owned";
		label2.ToolTip = availableTooltip;
		label2.FitToText();
		label2.X = 200;
		label2.Y = gridHeaderY;
		Label label3 = new Label(guiManager);
		Add(label3);
		label3.Init(Label.LabelType.LCDHeadingRed);
		label3.ToolTip = useVerminTooltip;
		label3.Text = "Use against vermin";
		label3.FitToText();
		label3.X = 300;
		label3.Y = gridHeaderY;
	}

	private static void SetUseAgainstVermin(Expedition expedition, bool newValue, EntityType entityType)
	{
		if (expedition.Policy.GetAllowAmmoForVermin(entityType) != newValue)
		{
			Command command = new AllowAmmoForVermin(expedition.ID, entityType, newValue, giveClientFeedback: true);
			The.Client.Controller.StoreAndExecuteCommand(command);
		}
	}

	private UIComponent AddRow(EntityType entityType, OwnerAmmoOfType ownedAmmo, EntityGroup owner)
	{
		UIComponent uIComponent = new UIComponent(guiManager);
		grid.AddEntry(entityType, uIComponent);
		uIComponent.OrderByTag1 = entityType.PluralName;
		InventoryPanel.AddEntityTypeIcon(entityType, uIComponent, 10);
		DataTypeButton dataTypeButton = new DataTypeButton(guiManager, DataSheet.InfoToShow.Data, entityType, owner.ID, useUIOwner: false);
		dataTypeButton.Init(TextButton.TextButtonType.LCDToolTipBlack);
		dataTypeButton.ID = DataControlID.Caption;
		dataTypeButton.IsRoot = true;
		dataTypeButton.Text = entityType.PluralName;
		uIComponent.Add(dataTypeButton);
		dataTypeButton.TextAlignment = TextButton.TextAlign.Left;
		dataTypeButton.Width = 160;
		dataTypeButton.X = 22;
		dataTypeButton.DebugTag = "entityTypeButton";
		Label label = new Label(guiManager);
		label.Init(Label.LabelType.LCDNormal);
		uIComponent.Add(label);
		label.X = 200;
		label.ID = DataControlID.Available;
		label.ToolTip = availableTooltip;
		label.Text = "0";
		label.FitToText();
		uIComponent.CenterChildVertically(label);
		CheckBox checkBox = new CheckBox(guiManager);
		uIComponent.Add(checkBox);
		checkBox.Init(CheckBoxType.LCDNoLabel);
		checkBox.SwitchStateOnClick = true;
		checkBox.X = 300;
		checkBox.ID = DataControlID.Selector;
		checkBox.ToolTip = useVerminTooltip;
		checkBox.Click += cbAllowVermin_Click;
		checkBox.Tag1 = entityType;
		uIComponent.OrderByTag2 = entityType.PluralName;
		return uIComponent;
	}

	private void cbAllowVermin_Click(UIComponent sender, EventArgs e)
	{
		CheckBox checkBox = sender as CheckBox;
		EntityType entityType = (EntityType)checkBox.Tag1;
		SetUseAgainstVermin(expedition, checkBox.IsChecked, entityType);
	}

	private void cbAutoclaim_Click(UIComponent sender, EventArgs e)
	{
		if (expedition != null && UWGame.Mods.HuntingMod.AutoclaimsCampKills(expedition) != cbAutoclaim.IsChecked)
		{
			The.Client.Controller.StoreAndExecuteCommand(new SetAutoclaimCampKills(expedition.ID, cbAutoclaim.IsChecked));
		}
	}

	public override void Refresh()
	{
		expedition = The.InGameUI.GetExpedition();
		if (cbAutoclaim != null)
		{
			cbAutoclaim.IsChecked = UWGame.Mods.HuntingMod.AutoclaimsCampKills(expedition);
		}
		Populate();
	}

	private void Populate()
	{
		UIComponent item = null;
		EntityGroup ownedEntities = expedition.OwnedEntities;
		if (ownedEntities == null)
		{
			grid.Clear();
			return;
		}
		Dictionary<EntityType, OwnerAmmoOfType> data = GetAmmoToDisplay(ownedEntities);
		grid.BeginAddingEntries();
		foreach (KeyValuePair<EntityType, OwnerAmmoOfType> item2 in data)
		{
			if (!grid.TryGetEntry(item2.Key, out item))
			{
				item = AddRow(item2.Key, item2.Value, ownedEntities);
			}
			UpdateRow(item, item2.Key, item2.Value, ownedEntities);
		}
		grid.DeleteEntries((EntityType e) => data.Any((KeyValuePair<EntityType, OwnerAmmoOfType> t) => t.Key == e));
		grid.Sort((UIComponent i) => i.OrderByTag1, Grid.Sorting.Ascending);
		grid.EndAddingEntries();
		SetHeight();
	}

	private Dictionary<EntityType, OwnerAmmoOfType> GetAmmoToDisplay(EntityGroup owner)
	{
		return owner.AmmoItems;
	}

	private void UpdateRow(UIComponent item, EntityType entityType, OwnerAmmoOfType ownedAmmo, EntityGroup owner)
	{
		int noOfIncompleteEntities;
		int noOfEntitiesUsedAsParts;
		int noOfItemsOnOtherSite;
		int noOfItemsOwnedByOthers;
		int noOfAvailableItemsIncludingIntrinsic;
		List<EntityID> listOfAllEntities;
		List<EntityID> listOfAvailableEntities;
		List<EntityID> listOfUnavailableEntities;
		int noOfAvailableEntities = InventoryPanel.GetNoOfAvailableEntities(owner.AllEntities, owner, entityType, out noOfIncompleteEntities, out noOfEntitiesUsedAsParts, out noOfItemsOnOtherSite, out noOfItemsOwnedByOthers, out noOfAvailableItemsIncludingIntrinsic, out listOfAllEntities, out listOfAvailableEntities, out listOfUnavailableEntities);
		((DataTypeButton)item.FindChildById(DataControlID.Caption)).SetAvailableStatusColor(noOfAvailableEntities > 0);
		item.FindChildById<Label>(DataControlID.Available, out var child, firstLevelOnly: false);
		child.Text = ownedAmmo.TotalRounds.ToString();
		child.FitToText();
		item.FindChildById<CheckBox>(DataControlID.Selector, out var child2, firstLevelOnly: false);
		child2.IsChecked = expedition.Policy.GetAllowAmmoForVermin(entityType);
	}
}
