using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface.Controls;
using UWGame.ClientSide.Interface.Inventory;
using UWGame.Control.Commands;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Commands;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Snapshots;
using WindowSystem;

namespace UWGame.ClientSide.Interface.HUD_Windows;

public class HUDEntityContextMenu : HUDWindow
{
	private TextButton tbSalvage;

	private TextButton tbPackingDown;

	private TextButton tbHunt;

	private TextButton tbMoveExpedition;

	private TextButton tbSetStockpile;

	private TextButton tbSetTradeOffers;

	private TextButton tbUpgrade;

	private TextButton tbDiscard;

	private TextButton tbClaim;

	private DataTypeButton btSalvageProcess;

	private Grid grdSpecialActions;

	private EntityID? entityID;

	private Expedition expedition;

	private StockpileWindow stockpileWindow;

	public UpgradeWindow UpgradeWindow;

	private const string stockpileTooltip = "Choose the types of items that can be stored in this structure";

	private const string stockpileBrokenTooltip = "The structure is broken and is unusable for stockpiling.";

	private const string offeredForTradeTooltip = "Choose the types of items that can be offered for trade in this structure";

	private const string upgradeTooltip = "Choose improvements for the structure";

	private const int maxNumberOfMissingInputsToDisplayProcessesWithout = 1;

	public HUDEntityContextMenu()
		: base(240, 220)
	{
		tbSalvage = new TextButton(gui);
		tbSalvage.Text = "BEGIN";
		tbSalvage.ToolTip = "Salvage the object: When breaking this apart, some parts will be retrieved, some will be lost. See the process tooltip for more info.";
		tbSalvage.Init(TextButton.TextButtonType.HUDSalvage);
		tbSalvage.Click += tbSalvage_Click;
		tbSalvage.ScaleWidthToFitText();
		tbPackingDown = new TextButton(gui);
		tbPackingDown.Text = "BEGIN";
		tbPackingDown.ToolTip = "Disassemble the object: All its parts will be retrieved. See the process tooltip for more info.";
		tbPackingDown.Init(TextButton.TextButtonType.HUDPackingDown);
		tbPackingDown.Click += tbSalvage_Click;
		tbPackingDown.ScaleWidthToFitText();
		tbHunt = new TextButton(gui);
		tbHunt.Text = "HUNT";
		tbHunt.ToolTip = "Hunt this animal";
		tbHunt.Init(TextButton.TextButtonType.HUDHunt);
		tbHunt.Click += tbHunt_Click;
		tbHunt.ScaleWidthToFitText();
		tbSetStockpile = new TextButton(gui);
		tbSetStockpile.Text = "STOCKPILE";
		tbSetStockpile.ToolTip = "Choose the types of items that can be stored in this structure";
		tbSetStockpile.Init(TextButton.TextButtonType.HUDStockpile);
		tbSetStockpile.Click += tbSetStockpile_Click;
		tbSetStockpile.ScaleWidthToFitText();
		tbSetTradeOffers = new TextButton(gui);
		tbSetTradeOffers.Text = "TRADE";
		tbSetTradeOffers.ToolTip = "Choose the types of items that can be offered for trade in this structure";
		tbSetTradeOffers.Init(TextButton.TextButtonType.HUDStockpile);
		tbSetTradeOffers.Click += tbSetTradeOffers_Click;
		tbSetTradeOffers.ScaleWidthToFitText();
		tbUpgrade = new TextButton(gui);
		tbUpgrade.Text = "UPGRADE";
		tbUpgrade.ToolTip = "Choose improvements for the structure";
		tbUpgrade.Init(TextButton.TextButtonType.HUDUpgrade);
		tbUpgrade.Click += tbUpgrade_Click;
		tbUpgrade.ScaleWidthToFitText();
		tbClaim = new TextButton(gui);
		tbClaim.Text = "CLAIM";
		tbClaim.Init(TextButton.TextButtonType.HUDClaim);
		tbClaim.Click += tbClaim_Click;
		tbClaim.ScaleWidthToFitText();
		tbDiscard = new TextButton(gui);
		tbDiscard.Text = "DISCARD";
		tbDiscard.ToolTip = "Choose the types of items that can be stored in this structure";
		tbDiscard.Init(TextButton.TextButtonType.HUDDiscard);
		tbDiscard.Click += tbDiscard_Click;
		tbDiscard.ScaleWidthToFitText();
		tbMoveExpedition = new TextButton(gui);
		tbMoveExpedition.Text = "MOVE CAMP";
		tbMoveExpedition.ToolTip = "Click on terrain to designate a new spot for the camp";
		tbMoveExpedition.Init(TextButton.TextButtonType.HUDStockpile);
		tbMoveExpedition.Click += tbExpedition_Click;
		tbMoveExpedition.Y = 6;
		tbMoveExpedition.X = 6;
		tbMoveExpedition.ScaleWidthToFitText();
		grdSpecialActions = new Grid(gui, ListBoxType.HUDAndLCD, Label.LabelType.HUDWindow);
		grdSpecialActions.FixedItemHeights = true;
		grdSpecialActions.RenderType = RenderType.Normal;
		grdSpecialActions.HMargin = 5;
		grdSpecialActions.VMargin = 5;
		grdSpecialActions.Font = GUIManager.LCDandHUDBodyFontPath;
		grdSpecialActions.Width = DisplayWindow.ViewPort.Width - 12;
		grdSpecialActions.ItemHeight = 26;
		grdSpecialActions.Position = new Point(0, 0);
		grdSpecialActions.CanGrowInHeight = true;
		grdSpecialActions.ScrollBarEnabled = false;
		stockpileWindow = new StockpileWindow();
		UpgradeWindow = new UpgradeWindow();
	}

	private void tbUpgrade_Click(UIComponent sender, EventArgs e)
	{
		if (entityID.HasValue && !GoalEvaluator.EntityDataResultCausesSkip(The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(entityID.Value, out var data)))
		{
			HideChildWindows();
			UpgradeWindow.ShowOnPlayfield(TileSelectionContextMenu.GetXPositionOfChildWindow(DisplayWindow), DisplayWindow.Y);
			UpgradeWindow.Fill(data.EntityID, fillUserControls: true);
		}
	}

	private void tbSetTradeOffers_Click(UIComponent sender, EventArgs e)
	{
		if (entityID.HasValue && !GoalEvaluator.EntityDataResultCausesSkip(The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(entityID.Value, out var data)))
		{
			HideChildWindows();
			stockpileWindow.ShowOnPlayfield(TileSelectionContextMenu.GetXPositionOfChildWindow(DisplayWindow), DisplayWindow.Y);
			stockpileWindow.FillFromStructure(data.EntityID, Stockpile.TypesOfStockpiles.OfferedForTrade);
		}
	}

	private void tbSetStockpile_Click(UIComponent sender, EventArgs e)
	{
		if (entityID.HasValue && !GoalEvaluator.EntityDataResultCausesSkip(The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(entityID.Value, out var data)))
		{
			HideChildWindows();
			stockpileWindow.ShowOnPlayfield(TileSelectionContextMenu.GetXPositionOfChildWindow(DisplayWindow), DisplayWindow.Y);
			stockpileWindow.FillFromStructure(data.EntityID, Stockpile.TypesOfStockpiles.Normal);
		}
	}

	private void HideChildWindows()
	{
		stockpileWindow.Hide();
		UpgradeWindow.Hide();
	}

	private void tbClaim_Click(UIComponent sender, EventArgs e)
	{
		if (!entityID.HasValue)
		{
			return;
		}
		if (!GoalEvaluator.EntityDataResultCausesSkip(The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(entityID.Value, out var data)))
		{
			if (EntityListWindow.CanBeClaimed(data))
			{
				Expedition expedition = The.InGameUI.GetExpedition();
				if (expedition != null)
				{
					Command command = new Claim(entityID.Value, The.InGameUI.UIAllegiance.ID, ((ILookUp<IOwner, OwnerID>)expedition).ID, giveClientFeedback: true);
					The.Client.Controller.StoreAndExecuteCommand(command);
				}
			}
			else
			{
				Refresh();
			}
		}
		else
		{
			HandleInvalidEntity();
		}
	}

	private void HandleInvalidEntity()
	{
		Hide();
	}

	public void OnClaimEntity()
	{
		RefreshEntityContent();
		Hide();
	}

	private void tbDiscard_Click(UIComponent sender, EventArgs e)
	{
		if (!entityID.HasValue)
		{
			return;
		}
		if (!GoalEvaluator.EntityDataResultCausesSkip(The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(entityID.Value, out var data)))
		{
			EntityListWindow.CanBeSalvagedOrDiscarded(data, out var _, out var canBeDiscardedNow);
			if (canBeDiscardedNow)
			{
				Command command = new Discard(entityID.Value, The.InGameUI.UIAllegiance.ID, giveClientFeedback: true);
				The.Client.Controller.StoreAndExecuteCommand(command);
			}
			else
			{
				Refresh();
			}
		}
		else
		{
			HandleInvalidEntity();
		}
	}

	public void OnDiscardEntity()
	{
		RefreshEntityContent();
		Hide();
	}

	private void tbExpedition_Click(UIComponent sender, EventArgs e)
	{
		The.InGameUI.InterfaceMode = InGameInterface.InterfaceState.PlaceExpeditionCenter;
		Hide();
	}

	private void tbHunt_Click(UIComponent sender, EventArgs e)
	{
		if (!entityID.HasValue)
		{
			return;
		}
		if (!GoalEvaluator.EntityDataResultCausesSkip(The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(entityID.Value, out var data)))
		{
			if (data.CanBeHunted(The.InGameUI.UIAllegiance))
			{
				Expedition expedition = The.InGameUI.GetExpedition();
				if (expedition != null)
				{
					Command command = new Hunt(entityID.Value, The.InGameUI.UIAllegiance.ID, expedition.OwnedEntities.ID, giveClientFeedback: true);
					The.Client.Controller.StoreAndExecuteCommand(command);
				}
			}
		}
		else
		{
			HandleInvalidEntity();
		}
	}

	public void OnHuntCreature()
	{
		The.InGameUI.gui.PlaySound(GUIManager.PlaceBuildingBeep);
		Hide();
	}

	public bool IsShowingEntity(EntityID entityID)
	{
		return this.entityID == entityID;
	}

	public bool IsShowingExpedition(Expedition expedition)
	{
		if (this.expedition != null)
		{
			return this.expedition == expedition;
		}
		return false;
	}

	private void tbSalvage_Click(UIComponent sender, EventArgs e)
	{
		if (entityID.HasValue)
		{
			TrySalvage(entityID.Value);
		}
	}

	public static void TrySalvage(EntityID entityID)
	{
		SharedKnowledge sharedKnowledge = The.InGameUI.UIAllegiance.SharedKnowledge;
		if (GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(entityID, out var data)))
		{
			return;
		}
		LookUpOwners.ResolveEntityOwner(data, out IOwner owner);
		if (owner == null || owner.Allegiance != The.InGameUI.UIAllegiance || !data.EntityType.CanBeSalvagedDirectly() || Salvage.SalvageJobExists(data))
		{
			return;
		}
		Command command;
		if (data.ContainedUpgrades != null)
		{
			foreach (KeyValuePair<UpgradeCategory, EntityID> containedUpgrade in data.ContainedUpgrades)
			{
				if (!GoalEvaluator.EntityDataResultCausesSkip(sharedKnowledge.GetKnownData(containedUpgrade.Value, out var data2)) && data2.EntityType.CanBeSalvagedDirectly() && !Salvage.SalvageJobExists(data2))
				{
					command = new Salvage(containedUpgrade.Value, The.InGameUI.UIAllegiance.ID, giveClientFeedback: true);
					The.Client.Controller.StoreAndExecuteCommand(command);
				}
			}
		}
		if (owner.OwnedEntities.Upgrades != null && owner.OwnedEntities.Upgrades.TryGetValue(entityID, out var value))
		{
			List<SetUpgrade> list = new List<SetUpgrade>();
			foreach (KeyValuePair<UpgradeCategory, EntityType> item2 in value)
			{
				SetUpgrade item = new SetUpgrade(entityID, The.InGameUI.UIAllegiance.ID, owner.OwnedEntities.ID, giveClientFeedback: true, item2.Key, null);
				list.Add(item);
			}
			foreach (SetUpgrade item3 in list)
			{
				The.Client.Controller.StoreAndExecuteCommand(item3);
			}
		}
		command = new Salvage(entityID, The.InGameUI.UIAllegiance.ID, giveClientFeedback: true);
		The.Client.Controller.StoreAndExecuteCommand(command);
	}

	public void OnSalvageEntity()
	{
		The.InGameUI.gui.PlaySound(GUIManager.PlaceBuildingBeep);
		Hide();
	}

	public void ShowOnPlayfield(int screenPosX, int screenPosY, bool modal, Expedition expedition)
	{
		if (RefreshExpeditionContent())
		{
			this.expedition = expedition;
			entityID = null;
			base.ShowOnPlayfield(screenPosX, screenPosY);
		}
	}

	public void ShowOnPlayfield(int screenPosX, int screenPosY, bool modal, EntityID entityID)
	{
		this.entityID = entityID;
		expedition = null;
		if (RefreshEntityContent())
		{
			base.ShowOnPlayfield(screenPosX, screenPosY);
		}
	}

	public bool RefreshExpeditionContent()
	{
		Remove(tbSalvage);
		Remove(tbPackingDown);
		Remove(tbHunt);
		Remove(tbDiscard);
		Remove(tbClaim);
		if (btSalvageProcess != null)
		{
			Remove(btSalvageProcess);
		}
		grdSpecialActions.Clear();
		Add(tbMoveExpedition);
		DisplayWindow.Height = tbMoveExpedition.Height + 12;
		return true;
	}

	private bool RefreshEntityContent()
	{
		if (!EntityHasActions(out var canBeSalvaged, out var canBeHunted, out var canBeDiscarded, out var canBeClaimed, out var hasSpecialActions, out var canSetStockpile, out var canSetTradeOffers, out var canUpgrade, out var entityData, entityID))
		{
			Remove(tbMoveExpedition);
			Remove(tbSalvage);
			Remove(tbPackingDown);
			Remove(tbHunt);
			Remove(tbDiscard);
			Remove(tbClaim);
			Remove(tbSetStockpile);
			Remove(tbSetTradeOffers);
			Remove(tbUpgrade);
			grdSpecialActions.Clear();
			return false;
		}
		int num = 0;
		Remove(tbMoveExpedition);
		if (canBeSalvaged)
		{
			int num2 = num + 6;
			ProcessType salvageProcessType = entityData.EntityType.NonLivingType.SalvageProcessType;
			CreateSalvageProcessButton(salvageProcessType, num2);
			Add(btSalvageProcess);
			TextButton textButton;
			if (salvageProcessType.IsSalvageWithoutWaste)
			{
				textButton = tbPackingDown;
				Remove(tbSalvage);
			}
			else
			{
				textButton = tbSalvage;
				Remove(tbPackingDown);
			}
			Add(textButton);
			textButton.Y = num2;
			textButton.X = btSalvageProcess.Right + 6;
			num += textButton.Height;
		}
		else
		{
			Remove(tbSalvage);
			Remove(tbPackingDown);
			if (btSalvageProcess != null)
			{
				Remove(btSalvageProcess);
			}
		}
		if (canBeHunted)
		{
			Add(tbHunt);
			tbHunt.Y = num + 6;
			tbHunt.X = 6;
			num += tbHunt.Height;
		}
		else
		{
			Remove(tbHunt);
		}
		if (canSetStockpile)
		{
			Add(tbSetStockpile);
			tbSetStockpile.Y = num + 6;
			tbSetStockpile.X = 6;
			num += tbSetStockpile.Height;
			if (Entity.IsFunctional(entityData))
			{
				tbSetStockpile.Enabled = true;
				tbSetStockpile.ToolTip = "Choose the types of items that can be stored in this structure";
			}
			else
			{
				tbSetStockpile.Enabled = false;
				tbSetStockpile.ToolTip = "The structure is broken and is unusable for stockpiling.";
			}
		}
		else
		{
			Remove(tbSetStockpile);
		}
		if (canSetTradeOffers)
		{
			Add(tbSetTradeOffers);
			tbSetTradeOffers.Y = num + 6;
			tbSetTradeOffers.X = 6;
			num += tbSetTradeOffers.Height;
			if (Entity.IsFunctional(entityData))
			{
				tbSetTradeOffers.Enabled = true;
				tbSetTradeOffers.ToolTip = "Choose the types of items that can be offered for trade in this structure";
			}
			else
			{
				tbSetTradeOffers.Enabled = false;
				tbSetTradeOffers.ToolTip = "The structure is broken and is unusable for stockpiling.";
			}
		}
		else
		{
			Remove(tbSetTradeOffers);
		}
		if (canUpgrade)
		{
			Add(tbUpgrade);
			tbUpgrade.Y = num + 6;
			tbUpgrade.X = 6;
			num += tbUpgrade.Height;
			if (Entity.IsFunctional(entityData))
			{
				tbUpgrade.Enabled = true;
				tbUpgrade.ToolTip = "UPGRADE. View or set the possible upgrades.";
			}
			else
			{
				tbUpgrade.Enabled = false;
				tbUpgrade.ToolTip = "UPGRADE. Cannot upgrade a broken structure.";
			}
		}
		else
		{
			Remove(tbUpgrade);
		}
		if (canBeDiscarded)
		{
			Add(tbDiscard);
			tbDiscard.Y = num + 6;
			tbDiscard.X = 6;
			num += tbDiscard.Height;
			if (entityData.EntityType.StructureType != null)
			{
				tbDiscard.Text = "ABANDON";
				tbDiscard.ToolTip = "ABANDON. Stop using this structure";
			}
			else
			{
				tbDiscard.Text = "DISCARD";
				tbDiscard.ToolTip = "DISCARD. Exclude this item from the colony's possessions.";
			}
			tbDiscard.ScaleWidthToFitText();
		}
		else
		{
			Remove(tbDiscard);
		}
		if (canBeClaimed)
		{
			Add(tbClaim);
			tbClaim.Y = num + 6;
			tbClaim.X = 6;
			num += tbClaim.Height;
			if (entityData.EntityType.StructureType != null)
			{
				tbClaim.Text = "CLAIM";
				tbClaim.ToolTip = "CLAIM. Start using this structure";
			}
			else
			{
				tbClaim.Text = "CLAIM";
				tbClaim.ToolTip = "CLAIM. Include this item in the colony's possessions";
			}
			tbClaim.ScaleWidthToFitText();
		}
		else
		{
			Remove(tbClaim);
		}
		PopulateSpecialActionGrid(hasSpecialActions, entityData);
		if (hasSpecialActions)
		{
			Add(grdSpecialActions);
			grdSpecialActions.Y = num + 6;
			num += grdSpecialActions.Height;
		}
		else
		{
			Remove(grdSpecialActions);
		}
		DisplayWindow.Height = num + 12;
		return true;
	}

	private void CreateSalvageProcessButton(ProcessType salvageProcess, int yPos)
	{
		if (btSalvageProcess == null || btSalvageProcess.processType != salvageProcess)
		{
			if (btSalvageProcess != null)
			{
				Remove(btSalvageProcess);
			}
			btSalvageProcess = new DataTypeButton(The.InGameUI.gui, DataSheet.InfoToShow.Production, salvageProcess, null, useUIOwner: true);
			btSalvageProcess.Init(TextButton.TextButtonType.HUDToolTipWhite);
			btSalvageProcess.IsRoot = true;
			btSalvageProcess.Text = salvageProcess.Name;
			btSalvageProcess.TextAlignment = TextButton.TextAlign.Left;
			btSalvageProcess.Width = 110;
			btSalvageProcess.X = 6;
			btSalvageProcess.Y = yPos;
		}
	}

	public void PopulateSpecialActionGrid(bool hasSpecialActions, IKnownEntityData entityData)
	{
		EntityGroup owner = LookUp<EntityGroup, EntityGroupID>.FindByID(The.InGameUI.UIOwner);
		grdSpecialActions.BeginAddingEntries();
		if (!hasSpecialActions || entityData == null || !entityData.IsCompleted())
		{
			grdSpecialActions.Clear();
		}
		else
		{
			List<ProcessType> specialActionsToDisplay = The.InGameUI.UIAllegiance.SharedKnowledge.GetSpecialActionsForDisplay(entityData);
			if (specialActionsToDisplay != null)
			{
				foreach (ProcessType item2 in specialActionsToDisplay)
				{
					if (!grdSpecialActions.TryGetEntry(item2, out var item))
					{
						item = AddItemRow(item2);
					}
					UpdateItemRow(item, entityData, item2, owner);
				}
			}
			grdSpecialActions.DeleteEntries((ProcessType e) => specialActionsToDisplay != null && specialActionsToDisplay.Contains(e));
		}
		grdSpecialActions.Sort((UIComponent p) => p.OrderByTag1, Grid.Sorting.Ascending);
		grdSpecialActions.EndAddingEntries();
	}

	private void UpdateItemRow(UIComponent itemRow, IKnownEntityData entityData, ProcessType processType, EntityGroup owner)
	{
		TextButton textButton = (TextButton)itemRow.FindChildById(UIComponent.DataControlID.CurrentOrders);
		List<Job> otherJobs = owner.OtherJobs;
		bool hasInputs;
		bool hasTools;
		int maxAmountThatCanBeProduced;
		int? noOfMissingInputTypes;
		int? noOfAvailableInputTypes;
		bool hasSkills;
		bool hasResource;
		bool hasSpecialSite;
		bool hasPolicy;
		EntityType immovableInput;
		if (SpecialAction.ActionJobExists(entityData, processType, otherJobs))
		{
			textButton.ToolTip = "This task is ongoing. Use the task panel to view or cancel it";
			textButton.Enabled = false;
		}
		else if (!The.InGameUI.UIAllegiance.SharedKnowledge.SpecialActionIsAvailable(entityData, processType))
		{
			textButton.ToolTip = "Not available at this time.";
			textButton.Enabled = false;
		}
		else if (!InventoryPanel.HasAllInputsAndToolsForProcess(processType, owner, out hasInputs, out hasTools, out maxAmountThatCanBeProduced, out noOfMissingInputTypes, out noOfAvailableInputTypes, out hasSkills, out hasResource, out hasSpecialSite, out hasPolicy, out immovableInput))
		{
			if (!hasPolicy)
			{
				textButton.ToolTip = "We need to adopt a policy first.";
			}
			else
			{
				textButton.ToolTip = "We don't have all the needed materials or tools to begin this";
			}
			textButton.Enabled = false;
		}
		else
		{
			textButton.ToolTip = "Click to begin";
			textButton.Enabled = true;
		}
	}

	protected UIComponent CreateTooltipAndActionButton(EntityType entityType, ProcessType processType, string actionLabel, EventArgs eventArgs, int menuWidth, out TextButton btAction)
	{
		UIComponent uIComponent = new UIComponent(gui);
		btAction = new TextButton(gui);
		btAction.Init(TextButton.TextButtonType.HUDToolTipWhite);
		btAction.ID = UIComponent.DataControlID.CurrentOrders;
		btAction.EventArgs = eventArgs;
		btAction.Text = actionLabel;
		btAction.TextAlignment = TextButton.TextAlign.Center;
		btAction.ScaleWidthToFitText();
		btAction.X = menuWidth - btAction.Width - 20;
		uIComponent.Add(btAction);
		DataTypeButton dataTypeButton = ((entityType == null) ? new DataTypeButton(gui, DataSheet.InfoToShow.Production, processType, null, useUIOwner: true)
		{
			Text = processType.Name
		} : new DataTypeButton(gui, DataSheet.InfoToShow.Production, entityType, null, useUIOwner: true)
		{
			Text = entityType.Name
		});
		dataTypeButton.Init(TextButton.TextButtonType.HUDToolTipWhite);
		dataTypeButton.ID = UIComponent.DataControlID.Caption;
		dataTypeButton.IsRoot = true;
		uIComponent.Add(dataTypeButton);
		dataTypeButton.TextAlignment = TextButton.TextAlign.Left;
		dataTypeButton.Width = menuWidth - btAction.Width - (menuWidth - btAction.Width - btAction.X) - 6;
		dataTypeButton.X = 2;
		dataTypeButton.SideToAnchorOn = CommonInterface.AnchorSide.Left;
		return uIComponent;
	}

	private UIComponent AddItemRow(ProcessType processType)
	{
		ActionButtonEventArgs eventArgs = new ActionButtonEventArgs(processType);
		TextButton btAction;
		UIComponent uIComponent = CreateTooltipAndActionButton(null, processType, processType.SpecialActionCaption ?? "BEGIN", eventArgs, DisplayWindow.Width, out btAction);
		btAction.Click += specialAction_Click;
		btAction.ID = UIComponent.DataControlID.CurrentOrders;
		grdSpecialActions.AddEntry(processType, uIComponent);
		uIComponent.OrderByTag1 = processType.SortOrder;
		return uIComponent;
	}

	private void specialAction_Click(UIComponent sender, EventArgs e)
	{
		if (entityID.HasValue)
		{
			ActionButtonEventArgs e2 = e as ActionButtonEventArgs;
			if (!GoalEvaluator.EntityDataResultCausesSkip(The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(entityID.Value, out var data)) && TryToCreateActionJob(data, e2.ProcessType))
			{
				The.InGameUI.gui.PlaySound(GUIManager.PlaceBuildingBeep);
			}
		}
		Refresh();
	}

	private static bool TryToCreateActionJob(IKnownEntityData entity, ProcessType processType)
	{
		EntityGroup entityGroup = LookUp<EntityGroup, EntityGroupID>.FindByID(The.InGameUI.UIOwner);
		if (entityGroup == null)
		{
			return false;
		}
		List<Job> otherJobs = entityGroup.OtherJobs;
		if (The.InGameUI.UIAllegiance.SharedKnowledge.SpecialActionIsAvailable(entity, processType) && !SpecialAction.ActionJobExists(entity, processType, otherJobs))
		{
			Command command = new SpecialAction(entity.EntityID, The.InGameUI.UIAllegiance.ID, entityGroup.ID, giveClientFeedback: true, processType.KeyName);
			The.Client.Controller.StoreAndExecuteCommand(command);
			return true;
		}
		return false;
	}

	public override void Refresh()
	{
		if (entityID.HasValue)
		{
			RefreshEntityContent();
		}
		else if (expedition != null)
		{
			RefreshExpeditionContent();
		}
		base.Refresh();
	}

	public static bool EntityHasActions(out bool canBeSalvaged, out bool canBeHunted, out bool canBeDiscarded, out bool canBeClaimed, out bool hasSpecialActions, out bool canSetStockpile, out bool canSetTradeOffers, out bool canUpgrade, out IKnownEntityData entityData, EntityID? entityID)
	{
		canBeSalvaged = false;
		canBeHunted = false;
		canBeDiscarded = false;
		hasSpecialActions = false;
		canSetStockpile = false;
		entityData = null;
		canBeClaimed = false;
		canSetTradeOffers = false;
		canUpgrade = false;
		if (entityID.HasValue && !GoalEvaluator.EntityDataResultCausesSkip(The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(entityID.Value, out entityData)))
		{
			canBeHunted = entityData.CanBeHunted(The.InGameUI.UIAllegiance);
			if (The.InGameUI.UIOwner.HasValue)
			{
				canSetStockpile = entityData.CanSetStockpileSettings(The.InGameUI.UIOwner.Value);
				canSetTradeOffers = entityData.CanSetTradeOfferSettings(The.InGameUI.UIOwner.Value);
				canUpgrade = entityData.CanBeUpgraded(The.InGameUI.UIOwner.Value);
			}
			if (The.InGameUI.UIAllegiance.SharedKnowledge.HasSpecialActionsForDisplay(entityData))
			{
				hasSpecialActions = true;
			}
			EntityListWindow.GetAllowedActions(entityData, out canBeSalvaged, out canBeDiscarded, out canBeClaimed);
		}
		return canBeHunted | canBeSalvaged | hasSpecialActions | canBeDiscarded | canBeClaimed | canSetTradeOffers | canSetStockpile | canUpgrade;
	}

	public override void Hide()
	{
		expedition = null;
		entityID = null;
		base.Hide();
	}
}
