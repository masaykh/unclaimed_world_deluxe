using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InputEventSystem;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface.BuyAndSell;
using UWGame.ClientSide.Interface.Controls;
using UWGame.ClientSide.Interface.HUD_Windows;
using UWGame.ClientSide.Interface.LCD;
using UWGame.ClientSide.Interface.Personnel;
using UWGame.ClientSide.Interface.World_map;
using UWGame.Control.Commands;
using UWGame.SimSide;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Commands;
using UWGame.SimSide.Communication;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Overland;
using UWGame.SimSide.Overland.Missions.Templates;
using UWGame.SimSide.Policies;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.XmlCollections;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Missions;

public class CreateMissionPanel : RosterPanel
{
	private enum Mode
	{
		Edit,
		Create
	}

	public enum WorldMapDialogSource
	{
		Start,
		End
	}

	private class TransportationType
	{
		public EntityType VehicleType;
	}

	private Grid surfaceGrid;

	public TravelLocation? start;

	public TravelLocation? destination;

	private ComboBox cbTransportation;

	private Label lblTransportNotesOrCost;

	private ErrorAndMessagePanel errorAndMessagePanel;

	private Label lblDestination;

	private Label lblStartingLocation;

	private Image iconHomeStart;

	private Image iconHomeDestination;

	private ActionPicker ActionPicker;

	private Label lblTotalMissionCost;

	private Label lblTotalCargoBulk;

	private Label lblAvailableCargoBulk;

	private Label lblSupplyNeedsHeader;

	private Label lblCostHeader;

	private Label lblSupplyNeeds;

	private Label lblCost;

	private Label lblTotalTradingCredits;

	private TextButton tbTransportMore;

	private int labelXPos;

	private int controlXPos = 118;

	private int controlWidth = 200;

	private int labelWidth = 145;

	private int travelPointControlWidth = 430;

	private LCDInnerPanel selectionPanel;

	private const Label.LabelType topLeftCaptionLabelStyle = Label.LabelType.LCDHeadingSteelGrey;

	private const Label.LabelType totalCaptionLabelStyle = Label.LabelType.LCDSmallHeadingBanner;

	private MissionTemplate missionTemplate;

	private ExclamationMarkInACircle exclamationMarkStart;

	private ExclamationMarkInACircle exclamationMarkDestination;

	private ExclamationMarkInACircle exclamationTransport;

	private const int innerPanelVerticalPadding = 8;

	private const int staticBottomHeight = 60;

	private Point worldMapDialogPosition;

	private ImageButton actionPickerSourceButton;

	private MissionStopTemplate dialogSourceLocation;

	private CargoActionTypes? dialogSourceAction;

	public WorldMapDialogSource worldMapDialogSource;

	private MissionActionTemplate dialogSourceActionTemplate;

	private const int totalColumnX = 478;

	private const int totalCaptionColumnX = 370;

	private const int collapsedPanelContentHeight = 119;

	private bool transportIsExpanded;

	private const int collapsedTravelActionHeight = 30;

	private const int goodsRowHeight = 26;

	private const string noCommTooltip = "To hire transports from another allegiance, we need to establish communication first. A ground satellite station is a good option.";

	private const string separator = " | ";

	private static int separatorLength = " | ".Length;

	private MissionTemplate Mission
	{
		get
		{
			if (missionTemplate == null)
			{
				OwnerID iD = ((ILookUp<IOwner, OwnerID>)LookUp<Expedition, ExpeditionID>.FindByID(The.InGameUI.UIExpedition.Value)).ID;
				missionTemplate = new MissionTemplate(The.InGameUI.UIAllegiance.ID, iD, createID: false);
			}
			return missionTemplate;
		}
		set
		{
			missionTemplate = value;
		}
	}

	public MissionTemplate getMissionTemplate()
	{
		return missionTemplate;
	}

	public CreateMissionPanel()
		: base("CREATE NEW RUN", 620, needBottomMarginForButtons: true)
	{
		AddSelectionPanel();
		CreateSurfaceWithScrollbar(out surfaceGrid, lcdSurface, canHaveFocus: true, 184);
		lcdSurface.DebugTag = "createMissionSurface";
		surfaceGrid.DebugTag = "grdLocations";
		errorAndMessagePanel = new ErrorAndMessagePanel(lcdSurface);
		UpdateMissionStopGridYPosAndHeight();
		worldMapDialogPosition = new Point(Window.X - 75, Window.Y + 75);
		ActionPicker = new ActionPicker(Interface.gui);
		ActionPicker.ActionSelected += ActionPicker_ActionSelected;
		ActionPicker.MouseOut += ActionPicker_MouseOut;
		ActionPicker.InvalidLocation += ActionPicker_InvalidLocation;
		lcdSurface.Add(ActionPicker);
		HideActionPicker();
		AddLowerButton("START RUN", "Click to start this run", Align.Left).Click += btStartRun_Click;
		AddLowerButton("CANCEL RUN", "Click to cancel this run", Align.Right).Click += btCancelRun_Click;
	}

	private void ActionPicker_InvalidLocation()
	{
		HideActionPicker();
		Revalidate();
	}

	private void btCancelRun_Click(UIComponent sender, EventArgs e)
	{
		OnCancel();
		surfaceGrid.Clear();
		Hide();
		The.InGameUI.ChangeRosterPanel(The.InGameUI.MissionsPanel);
	}

	private void UpdateMissionStopGridYPosAndHeight()
	{
		surfaceGrid.Y = selectionPanel.Panel.Bottom + 6;
		surfaceGrid.Height = lcdSurface.Height - surfaceGrid.Y - errorAndMessagePanel.ContentHeight - 13;
	}

	private void ActionPicker_MouseOut(UIComponent sender, MouseEventArgs args)
	{
		HideActionPicker();
		actionPickerSourceButton = null;
	}

	private void UpdateTotalCost()
	{
		if (missionTemplate != null)
		{
			EntityGroup entityGroup = LookUp<EntityGroup, EntityGroupID>.FindByID(The.InGameUI.UIOwner.Value);
			lblTotalCargoBulk.Text = Entity.GetBulkAsString(missionTemplate.ComputeTotalCargoBulk());
			lblTotalCargoBulk.ToolTip = "Amount of cargo bulk packed into the vehicle.";
			lblAvailableCargoBulk.Text = "/ " + Entity.GetBulkAsString(missionTemplate.GetTotalCargoCapacity());
			lblAvailableCargoBulk.X = lblTotalCargoBulk.Right;
			lblAvailableCargoBulk.ToolTip = "Total amount of cargo bulk that the vehicle can transport.";
			lblTotalMissionCost.Visible = true;
			decimal num = missionTemplate.ComputeTotalCost(out var transportCost, out var boughtItemsCost, out var soldItemsCost);
			lblTotalMissionCost.Text = Common.GetPriceAsString(num);
			ColorLabelByValue(lblTotalMissionCost, num);
			StringBuilder stringBuilder = new StringBuilder();
			Common.AppendLine(stringBuilder, "The total cost of the mission.");
			Common.AppendDivider(stringBuilder);
			Common.Append(stringBuilder, "Transport cost: ");
			Common.Append(stringBuilder, Common.GetPriceAsString(transportCost, useColoring: true, Common.ValueTint.Negative));
			Common.AppendLine(stringBuilder);
			if (!Common.IsZero(boughtItemsCost))
			{
				Common.Append(stringBuilder, "Cost of bought items: ");
				Common.Append(stringBuilder, Common.GetPriceAsString(boughtItemsCost, useColoring: true, Common.ValueTint.Negative));
				Common.AppendLine(stringBuilder);
			}
			if (!Common.IsZero(soldItemsCost))
			{
				Common.Append(stringBuilder, "Price of sold items: ");
				Common.Append(stringBuilder, Common.GetPriceAsString(soldItemsCost, useColoring: true, Common.ValueTint.Positive));
				Common.AppendLine(stringBuilder);
			}
			Common.AppendLine(stringBuilder);
			if (Common.IsGreaterThan(num, 0m))
			{
				Common.Append(stringBuilder, "Total cost: ");
				Common.Append(stringBuilder, Common.GetPriceAsString(num, useColoring: true, Common.ValueTint.Negative));
			}
			else
			{
				Common.Append(stringBuilder, "Total earned: ");
				Common.Append(stringBuilder, Common.GetPriceAsString(num, useColoring: true, Common.ValueTint.Positive));
			}
			lblTotalMissionCost.ToolTip = stringBuilder.ToString();
			lblTotalTradingCredits.Text = " / " + Common.MoneyAsString(entityGroup.Parent.TradeCredits.Value, abbreviate: true);
			lblTotalTradingCredits.ToolTip = "Total amount of available Credits.";
			lblTotalTradingCredits.X = lblTotalMissionCost.Right;
			if (missionTemplate.StartMissionStopTemplate != null && missionTemplate.StartMissionStopTemplate.TravelAction != null && missionTemplate.TransportationType != null)
			{
				missionTemplate.GetTotalDistance();
				num = missionTemplate.ComputeTransportationCost(out var _, out var _, out var costPerKilometer);
				lblCost.Text = Common.GetPriceAsString(costPerKilometer) + "/ KM";
				lblCost.ToolTip = "The cost of transportation / km";
			}
		}
	}

	private void ColorLabelByValue(Label lbl, decimal total)
	{
		if (Common.IsZero(total))
		{
			lbl.NormalColor = lbl.GetNormalColorForType();
		}
		else if (total < 0m)
		{
			lbl.NormalColor = GameData.Instance.GUIConstants.PositiveColor;
		}
		else
		{
			lbl.NormalColor = GameData.Instance.GUIConstants.NegativeColor;
		}
	}

	private void ShowError(string error, string errorTooltip = null, FieldError? errorField = null)
	{
		errorAndMessagePanel.ShowError(error, errorTooltip);
		if (errorField.HasValue)
		{
			string text = error;
			if (errorTooltip != null)
			{
				text = text + " \n" + errorTooltip;
			}
			switch (errorField.Value)
			{
			case FieldError.Start:
				ShowLocationError(text, exclamationMarkStart, lblStartingLocation, iconHomeStart);
				break;
			case FieldError.Destination:
				ShowLocationError(text, exclamationMarkDestination, lblDestination, iconHomeDestination);
				break;
			case FieldError.Transport:
				lblTransportNotesOrCost.Visible = false;
				exclamationTransport.Visible = true;
				exclamationTransport.ToolTip = text;
				break;
			}
		}
	}

	private static void ShowLocationError(string tooltip, ExclamationMarkInACircle exclamation, Label location, Image homeIcon)
	{
		exclamation.Visible = true;
		exclamation.ToolTip = tooltip;
		if (homeIcon.Visible)
		{
			exclamation.X = homeIcon.Right;
		}
		else if (location.Visible)
		{
			exclamation.X = location.Right + 4;
		}
		else
		{
			exclamation.X = location.X;
		}
	}

	private void Revalidate()
	{
		if (errorAndMessagePanel.IsShowingError)
		{
			ClearErrors();
			List<string> errors = null;
			FieldError? errorFieldCode = null;
			if (!ValidateMissionTemplate(ref errors, ref errorFieldCode) && errors != null && errors.Count > 0)
			{
				ShowErrors(errors, errorFieldCode);
			}
		}
	}

	private void ClearErrors()
	{
		errorAndMessagePanel.Clear();
		lblTransportNotesOrCost.Visible = true;
		exclamationMarkDestination.Visible = false;
		exclamationMarkStart.Visible = false;
		exclamationTransport.Visible = false;
	}

	private void btStartRun_Click(UIComponent sender, EventArgs e)
	{
		ClearErrors();
		List<string> errors = null;
		FieldError? errorFieldCode = null;
		if (ValidateMissionTemplate(ref errors, ref errorFieldCode))
		{
			Mission.StartMissionStopTemplate.TravelLocation.ResolveLocation(The.InGameUI.UIAllegiance.SharedKnowledge, out var _, out var _, out var expedition, out var _);
			Dictionary<EntityType, List<Entity>> vehicles = null;
			if (!GetVehiclesToAssign(expedition, ref vehicles))
			{
				ShowError("No vehicle available.", null, FieldError.Transport);
				return;
			}
			if (VehiclesAreHired(expedition) && !CanCommunicateWithExpedition(expedition))
			{
				ShowError("No communication with Start allegiance.", "To hire transports from another allegiance, we need to establish communication first. A ground satellite station is a good option.", FieldError.Start);
				return;
			}
			if (!CanCommunicateWithExpedition(LookUp<Expedition, ExpeditionID>.FindByID((ExpeditionID)destination.Value.ExpeditionID.Value)))
			{
				ShowError("No communication with Destination allegiance.", "To hire transports from another allegiance, we need to establish communication first. A ground satellite station is a good option.", FieldError.Destination);
				return;
			}
			Command command = new CreateMissionTemplate(missionTemplate);
			The.Client.Controller.StoreAndExecuteCommand(command);
			Command command2 = new CreateMission(missionTemplate.ID, expedition.OwnedEntities.ID, vehicles);
			The.Client.Controller.StoreAndExecuteCommand(command2);
			Hide();
			The.InGameUI.RosterAccessPanel.ShowMissions();
		}
		else if (errors != null && errors.Count > 0)
		{
			ShowErrors(errors, errorFieldCode);
		}
	}

	private void ShowErrors(List<string> errors, FieldError? errorField)
	{
		ShowError(errors[0], null, errorField);
	}

	private bool GetVehiclesToAssign(Expedition fromExpedition, ref Dictionary<EntityType, List<Entity>> vehicles)
	{
		vehicles = new Dictionary<EntityType, List<Entity>>();
		if (Mission.TransportationType != null && Mission.TransportationType.Vehicles != null)
		{
			foreach (Pair<string, int> vehicle in Mission.TransportationType.Vehicles)
			{
				fromExpedition.GetAvailableVehicles(GameData.Instance.AllEntityTypes[vehicle.First], ref vehicles, vehicle.Second);
				if (vehicles.Count == 0)
				{
					return false;
				}
			}
		}
		return true;
	}

	private bool ValidateMissionTemplate(ref List<string> errors, ref FieldError? errorFieldCode)
	{
		if (Mission.StartMissionStopTemplate != null && !Mission.ValidateMissionStops())
		{
			HandleDestroyedMissionStop();
			return false;
		}
		return Mission.Validate(ref errors, ref errorFieldCode);
	}

	private void HandleDestroyedMissionStop()
	{
		The.InGameUI.MessageBox.ShowMessage("One of the travel locations no longer exists. It is not possible to continue editing the mission.");
		The.InGameUI.MessageBox.OKClick += MessageBoxDestroyedMission_OKClick;
	}

	private void MessageBoxDestroyedMission_OKClick(object sender, EventArgs e)
	{
		The.InGameUI.MessageBox.OKClick -= MessageBoxDestroyedMission_OKClick;
		DestroyMission();
		Hide();
	}

	private void btNewAction_Click(UIComponent sender)
	{
	}

	private void btAddAction_Click(UIComponent sender, EventArgs e)
	{
		actionPickerSourceButton = (ImageButton)sender;
		ShowActionPicker();
		ActionPicker.Fill(missionTemplate, actionPickerSourceButton.Tag1 as MissionStopTemplate);
		ActionPicker.Show();
		int xPosToCenterAbout = sender.AbsolutePosition.X - display.AbsolutePosition.X;
		int num = sender.AbsolutePosition.Y - display.AbsolutePosition.Y;
		num = sender.AbsolutePosition.Y;
		ActionPicker.CenterThisHorizontally(xPosToCenterAbout);
		ActionPicker.CenterThisVertically(num);
		InGameInterface.PlaceWindowInsideViewableArea(ActionPicker, lcdSurface);
	}

	private void ShowActionPicker()
	{
		ActionPicker.Visible = true;
	}

	private void ActionPicker_ActionSelected(ActionTypes selectedAction)
	{
		// PORT: read the button BEFORE hiding the picker, and check it.
		//
		// The order was the other way round, and hiding the picker is exactly what can clear this
		// field: ActionPicker_MouseOut sets actionPickerSourceButton to null, and the pointer is
		// over the picker at the moment it is hidden - the click that got us here happened inside
		// it. So the next line dereferenced null and picking a trade action crashed the game.
		// Reported by Kastuk, on a mission to Zenig Station.
		//
		// The port's own hover release no longer raises MouseOut, which is what made this
		// reachable every time; this makes it unreachable at all, including from a genuine pointer
		// movement in the same frame - which is what the studio's ordering was always exposed to.
		UIComponent sourceButton = actionPickerSourceButton;
		actionPickerSourceButton = null;
		HideActionPicker();
		if (sourceButton == null)
		{
			return;
		}
		MissionStopTemplate missionStopTemplate = sourceButton.Tag1 as MissionStopTemplate;
		Point absolutePosition = sourceButton.AbsolutePosition;
		if (missionStopTemplate == null)
		{
			return;
		}
		if (!missionStopTemplate.TravelLocation.ResolveLocation(The.InGameUI.UIAllegiance.SharedKnowledge, out var _, out var allegiance, out var expedition, out var _))
		{
			HandleDestroyedMissionStop();
			return;
		}
		switch (selectedAction)
		{
		case ActionTypes.Buy:
		{
			EntityGroup playerBuyer = GetPlayerBuyer();
			BuyOrLoadFromLocation(absolutePosition, CargoActionTypes.Buy, missionStopTemplate, expedition, playerBuyer);
			break;
		}
		case ActionTypes.Load:
			BuyOrLoadFromLocation(absolutePosition, CargoActionTypes.Load, missionStopTemplate, expedition, null);
			break;
		case ActionTypes.Sell:
		{
			EntityGroup nPCBuyer = GetNPCBuyer();
			BuyOrLoadFromLocation(absolutePosition, CargoActionTypes.Sell, missionStopTemplate, expedition, nPCBuyer);
			break;
		}
		case ActionTypes.Embark:
			EmbarkFromLocation(absolutePosition, missionStopTemplate, allegiance, expedition);
			break;
		}
		UpdateTotalCost();
	}

	private void HideActionPicker()
	{
		ActionPicker.Visible = false;
		ActionPicker.X = 1000;
	}

	private void BuyOrLoadFromLocation(Point dialogSourceAbsolutePosition, CargoActionTypes selectedAction, MissionStopTemplate location, Expedition ownerOfItemsExpedition, EntityGroup buyer)
	{
		dialogSourceAction = selectedAction;
		dialogSourceLocation = location;
		ShowBuySellDialog(dialogSourceAbsolutePosition, selectedAction, null, ownerOfItemsExpedition, buyer);
	}

	private void EmbarkFromLocation(Point dialogSourceAbsolutePosition, MissionStopTemplate location, Allegiance allegiance, Expedition expedition)
	{
		dialogSourceLocation = location;
		ShowPersonnelDialog(dialogSourceAbsolutePosition);
	}

	private void ShowPersonnelDialog(Point dialogSourceAbsolutePosition)
	{
		ShowModalOverlay();
		PersonnelDialog personnelDialog = The.InGameUI.PersonnelDialog;
		personnelDialog.OKClick += personnelDialog_OKClick;
		personnelDialog.CancelClick += personnelDialog_CancelClick;
		personnelDialog.FillAndShow(GetPeople, dialogSourceAbsolutePosition, showSelectors: true, showMigrateRisk: false);
	}

	private void personnelDialog_CancelClick(object sender, EventArgs e)
	{
		ResetAfterPersonnelDialog();
	}

	private void personnelDialog_OKClick(object sender, EventArgs e)
	{
		List<EntityID> selectedEntities = The.InGameUI.PersonnelDialog.GetSelectedEntities();
		MissionActionTemplate missionActionTemplate = null;
		bool flag = false;
		if (dialogSourceActionTemplate == null)
		{
			flag = true;
		}
		if (dialogSourceLocation.TravelLocation.ResolveLocation(The.InGameUI.UIAllegiance.SharedKnowledge, out var _, out var _, out var _, out var _))
		{
			if (flag)
			{
				if (selectedEntities != null && selectedEntities.Count > 0)
				{
					missionActionTemplate = new EmbarkActionTemplate(dialogSourceLocation, selectedEntities, allowDeleting: true);
				}
			}
			else
			{
				EmbarkActionTemplate embarkActionTemplate = dialogSourceActionTemplate as EmbarkActionTemplate;
				if (selectedEntities != null && selectedEntities.Count > 0)
				{
					embarkActionTemplate.PassengerListTemplate.Passengers = selectedEntities.Select((EntityID p) => (long)p).ToList();
				}
				else
				{
					embarkActionTemplate.MissionStopTemplate.RemoveAction(embarkActionTemplate);
				}
			}
			if (missionActionTemplate != null)
			{
				Mission.AddAction(missionActionTemplate, dialogSourceLocation);
			}
			RepopulateAfterAddedAction();
			ResetAfterPersonnelDialog();
		}
		else
		{
			ResetAfterPersonnelDialog();
			HandleDestroyedMissionStop();
		}
	}

	private void ShowWorldMapDialog(Point absolutePosition)
	{
		ShowModalOverlay();
		WorldMapDialog worldMapDialog = The.InGameUI.WorldMapDialog;
		worldMapDialog.OKClick += WorldMapDialog_OKClick;
		worldMapDialog.CancelClick += WorldMapDialog_CancelClick;
		worldMapDialog.SelectedTravelLocation = null;
		EntityGroup playerBuyer = GetPlayerBuyer();
		EntityGroupID? buyer = null;
		if (playerBuyer != null)
		{
			buyer = playerBuyer.ID;
		}
		worldMapDialog.Fill(buyer);
		worldMapDialog.ShowInScreenSpace(worldMapDialogPosition.X, worldMapDialogPosition.Y);
	}

	private void ShowBuySellDialog(Point absolutePosition, CargoActionTypes selectedAction, Dictionary<EntityType, List<EntityID>> currentOrders, Expedition ownerOfItemsExpedition, EntityGroup buyer)
	{
		ShowModalOverlay();
		BuySellPanel buySellDialog = The.InGameUI.BuySellDialog;
		buySellDialog.OKClick += BuySellDialog_OKClick;
		buySellDialog.CancelClick += BuySellDialog_CancelClick;
		BuySellPanel.BuySellDialogMode mode = ((selectedAction != CargoActionTypes.Buy) ? BuySellPanel.BuySellDialogMode.ActionSellAtPlayer : BuySellPanel.BuySellDialogMode.ActionBuyAtNPC);
		The.InGameUI.FillAndShowBuySellDialog(absolutePosition, mode, currentOrders, CanTrade, ownerOfItemsExpedition.OwnedEntities, buyer, GetItemsForSale);
	}

	private List<EntityID> GetItemsForSale(EntityType type, out bool sourceIsInvalid)
	{
		Expedition homeExpedition = GetHomeExpedition();
		if (dialogSourceLocation == null)
		{
			sourceIsInvalid = true;
			return null;
		}
		return GetItemsForSale(type, dialogSourceLocation.TravelLocation, homeExpedition, out sourceIsInvalid);
	}

	public static List<EntityID> GetItemsForSale(EntityType type, TravelLocation travelLocation, Expedition buyingExpedition, out bool sourceIsInvalid)
	{
		sourceIsInvalid = false;
		new Dictionary<EntityType, List<EntityID>>();
		SharedKnowledge sharedKnowledge = The.InGameUI.UIAllegiance.SharedKnowledge;
		if (travelLocation.ResolveLocation(sharedKnowledge, out var _, out var _, out var expedition, out var terminalData))
		{
			if (terminalData != null)
			{
				OwnerID iD = ((ILookUp<IOwner, OwnerID>)expedition).ID;
				if (terminalData.OfferedEntitiesByType != null && terminalData.OfferedEntitiesByType.TryGetValue(type, out var value))
				{
					return GetValidItemsForSale(terminalData, iD, sharedKnowledge, value, buyingExpedition);
				}
				return null;
			}
			sourceIsInvalid = true;
			return null;
		}
		sourceIsInvalid = true;
		return null;
	}

	private static List<EntityID> GetValidItemsForSale(IKnownEntityData fromTerminal, OwnerID sellerID, SharedKnowledge sharedKnowledge, List<EntityID> items, Expedition buyingExpedition)
	{
		List<EntityID> list = null;
		for (int num = items.Count - 1; num >= 0; num--)
		{
			EntityID entityID = items[num];
			if (BuySellActionTemplate.ItemIsValidForSale(sharedKnowledge, entityID, fromTerminal, sellerID, buyingExpedition))
			{
				Common.AddToList(ref list, entityID);
			}
		}
		return list;
	}

	private List<IKnownEntityData> GetPeople()
	{
		TravelLocation? travelLocation = null;
		if (dialogSourceLocation != null)
		{
			travelLocation = dialogSourceLocation.TravelLocation;
		}
		return GetPeople(travelLocation);
	}

	public static List<IKnownEntityData> GetPeople(TravelLocation? travelLocation)
	{
		List<IKnownEntityData> list = null;
		if (travelLocation.HasValue && travelLocation.Value.ResolveLocation(The.InGameUI.UIAllegiance.SharedKnowledge, out var _, out var _, out var expedition, out var _))
		{
			if (expedition != null)
			{
				foreach (EntityID independentMember in expedition.IndependentMembers)
				{
					if (!GoalEvaluator.EntityDataResultCausesSkip(The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(independentMember, out var data)))
					{
						Common.AddToList(ref list, data);
					}
				}
				return list;
			}
			return null;
		}
		return null;
	}

	private void WorldMapDialog_OKClick(object sender, EventArgs e)
	{
		ResetAfterWorldMapDialog();
		if (start.HasValue && destination.HasValue)
		{
			surfaceGrid.Clear();
			if (worldMapDialogSource == WorldMapDialogSource.Start)
			{
				start = The.InGameUI.WorldMapDialog.SelectedTravelLocation;
			}
			else
			{
				destination = The.InGameUI.WorldMapDialog.SelectedTravelLocation;
			}
			if (!PopulateTransportation())
			{
				return;
			}
			CreateMissionStartingLocation();
			CreateMissionDestinationAndReturnDestination();
		}
		else if (worldMapDialogSource == WorldMapDialogSource.Start)
		{
			start = The.InGameUI.WorldMapDialog.SelectedTravelLocation;
			if (!PopulateTransportation())
			{
				return;
			}
			CreateMissionStartingLocation();
		}
		else
		{
			destination = The.InGameUI.WorldMapDialog.SelectedTravelLocation;
			CreateMissionDestinationAndReturnDestination();
		}
		ShowHomeIcon();
		UpdateTotalCost();
		Revalidate();
	}

	private bool CanTrade(EntityType entityType, out TierOrAreaType tierPolicy)
	{
		return InGameInterface.CanTrade(GetHomeExpedition(), entityType, out tierPolicy);
	}

	private Expedition GetHomeExpedition()
	{
		if (start.Value.AllegianceID.Value == (long)The.InGameUI.UIAllegiance.ID)
		{
			return LookUp<Expedition, ExpeditionID>.FindByID((ExpeditionID)start.Value.ExpeditionID.Value);
		}
		if (destination.HasValue && destination.Value.AllegianceID.Value == (long)The.InGameUI.UIAllegiance.ID)
		{
			return LookUp<Expedition, ExpeditionID>.FindByID((ExpeditionID)destination.Value.ExpeditionID.Value);
		}
		return null;
	}

	private void ShowHomeIcon()
	{
		if (start.Value.AllegianceID.Value == (long)The.InGameUI.UIAllegiance.ID)
		{
			iconHomeStart.Visible = true;
			iconHomeDestination.Visible = false;
			iconHomeStart.X = lblStartingLocation.Right;
		}
		else if (destination.HasValue && destination.Value.AllegianceID.Value == (long)The.InGameUI.UIAllegiance.ID)
		{
			iconHomeDestination.Visible = true;
			iconHomeStart.Visible = false;
			iconHomeDestination.X = lblDestination.Right;
		}
		else
		{
			iconHomeStart.Visible = false;
			iconHomeDestination.Visible = false;
		}
	}

	private void CreateMissionStartingLocation()
	{
		surfaceGrid.BeginAddingEntries();
		CreateStartingLocation();
		PopulateMissionLocations();
		surfaceGrid.EndAddingEntries();
	}

	private void CreateMissionDestinationAndReturnDestination()
	{
		CreateMissionDestination();
		CreateReturnDestination();
		PopulateMissionLocations();
		PopulateTransportation();
		Mission.SelectRoutes();
	}

	private void WorldMapDialog_CancelClick(object sender, EventArgs e)
	{
		ResetAfterWorldMapDialog();
	}

	private void BuySellDialog_CancelClick(object sender, EventArgs e)
	{
		ResetAfterBuySellDialog();
	}

	private void BuySellDialog_OKClick(object sender, EventArgs e)
	{
		BuySellPanel buySellDialog = The.InGameUI.BuySellDialog;
		if (buySellDialog.Orders != null)
		{
			MissionActionTemplate missionActionTemplate = null;
			bool isCreating = false;
			if (dialogSourceActionTemplate == null)
			{
				isCreating = true;
			}
			switch (dialogSourceAction)
			{
			case CargoActionTypes.Buy:
			{
				Expedition buyingExpedition = LookUp<Expedition, ExpeditionID>.FindByID(The.InGameUI.UIExpedition);
				if (dialogSourceLocation.TravelLocation.ResolveLocation(The.InGameUI.UIAllegiance.SharedKnowledge, out var _, out var _, out var expedition2, out var _))
				{
					missionActionTemplate = CreateOrModifyBuySellActions(buySellDialog, missionActionTemplate, isCreating, buyingExpedition, expedition2);
					break;
				}
				ResetAfterBuySellDialog();
				HandleDestroyedMissionStop();
				return;
			}
			case CargoActionTypes.Sell:
			{
				Expedition expedition = null;
				Expedition sellingExpedition = null;
				if (!dialogSourceLocation.TravelLocation.ResolveLocation(The.InGameUI.UIAllegiance.SharedKnowledge, out var _, out var _, out sellingExpedition, out var _))
				{
					ResetAfterBuySellDialog();
					HandleDestroyedMissionStop();
					return;
				}
				expedition = Mission.GetExpeditionMatchingPredicate((Expedition exp) => exp != sellingExpedition);
				if (expedition != null)
				{
					missionActionTemplate = CreateOrModifyBuySellActions(buySellDialog, missionActionTemplate, isCreating, expedition, sellingExpedition);
					break;
				}
				ResetAfterBuySellDialog();
				return;
			}
			case CargoActionTypes.Load:
				missionActionTemplate = new LoadActionTemplate(dialogSourceLocation, buySellDialog.Orders, allowDeleting: true);
				break;
			}
			if (missionActionTemplate != null)
			{
				Mission.AddAction(missionActionTemplate, dialogSourceLocation);
			}
		}
		RepopulateAfterAddedAction();
		ResetAfterBuySellDialog();
	}

	private MissionActionTemplate CreateOrModifyBuySellActions(BuySellPanel dialog, MissionActionTemplate newAction, bool isCreating, Expedition buyingExpedition, Expedition sellingExpedition)
	{
		if (isCreating)
		{
			if (dialog.Orders != null && dialog.Orders.Count > 0)
			{
				newAction = new BuySellActionTemplate(Mission, dialogSourceLocation, dialog.Orders, buyReducedAmountsIfNeeded: true, null, ((ILookUp<IOwner, OwnerID>)buyingExpedition).ID, ((ILookUp<IOwner, OwnerID>)sellingExpedition).ID, allowDeleting: true);
			}
		}
		else
		{
			BuySellActionTemplate buySellActionTemplate = dialogSourceActionTemplate as BuySellActionTemplate;
			if (dialog.Orders != null)
			{
				if (dialog.Orders.Any((KeyValuePair<EntityType, List<EntityID>> k) => k.Value.Count > 0))
				{
					buySellActionTemplate.ContractTemplate.Entities = new SerializableDictionary<string, List<long>>(dialog.Orders.ToDictionary((KeyValuePair<EntityType, List<EntityID>> k) => k.Key.KeyName, (KeyValuePair<EntityType, List<EntityID>> k) => k.Value.Select((EntityID e) => (long)e).ToList()));
				}
				else
				{
					buySellActionTemplate.MissionStopTemplate.RemoveAction(buySellActionTemplate);
				}
			}
		}
		return newAction;
	}

	private void RepopulateAfterAddedAction()
	{
		surfaceGrid.TryGetEntry(dialogSourceLocation, out var _);
		PopulateMissionLocations();
		UpdateTotalCost();
		Revalidate();
	}

	private void ResetAfterWorldMapDialog()
	{
		WorldMapDialog worldMapDialog = The.InGameUI.WorldMapDialog;
		worldMapDialog.OKClick -= WorldMapDialog_OKClick;
		worldMapDialog.CancelClick -= WorldMapDialog_CancelClick;
		RemoveModalOverlay();
	}

	private void ResetAfterBuySellDialog()
	{
		BuySellPanel buySellDialog = The.InGameUI.BuySellDialog;
		buySellDialog.OKClick -= BuySellDialog_OKClick;
		buySellDialog.CancelClick -= BuySellDialog_CancelClick;
		dialogSourceLocation = null;
		dialogSourceAction = null;
		dialogSourceActionTemplate = null;
		RemoveModalOverlay();
	}

	private void ResetAfterPersonnelDialog()
	{
		PersonnelDialog personnelDialog = The.InGameUI.PersonnelDialog;
		personnelDialog.OKClick -= personnelDialog_OKClick;
		personnelDialog.CancelClick -= personnelDialog_CancelClick;
		dialogSourceLocation = null;
		dialogSourceAction = null;
		dialogSourceActionTemplate = null;
		RemoveModalOverlay();
	}

	private void AddSelectionPanel()
	{
		int width = 109;
		selectionPanel = new LCDInnerPanel(Interface.gui, lcdSurface.Width, includeDecor: true, 1f);
		lcdSurface.Add(selectionPanel.Panel);
		selectionPanel.ContentHeight = 119;
		selectionPanel.Panel.Y = 0;
		selectionPanel.VerticalContentPadding = 8;
		Label label = new Label(Interface.gui);
		selectionPanel.AddContent(label);
		label.Init(Label.LabelType.LCDHeadingSteelGrey);
		label.Text = "START:";
		label.Width = width;
		ImageButton imageButton = new ImageButton(Interface.gui);
		imageButton.InitWithIcon(ImageButtonType.LCD, "globe_icon", hasCheckedState: false);
		selectionPanel.AddContent(imageButton);
		imageButton.Width = 35;
		imageButton.Height = 44;
		imageButton.ToolTip = "Select the starting point for the mission";
		imageButton.X = controlXPos;
		label.AlignVertically(imageButton);
		imageButton.Click += btStart_Click;
		imageButton.RecalculateIconPosition();
		label = new Label(Interface.gui);
		selectionPanel.AddContent(label);
		label.Init(Label.LabelType.LCDHeadingSteelGrey);
		label.Text = "DESTINATION:";
		label.Y = imageButton.Bottom + 6;
		label.Width = width;
		ImageButton imageButton2 = new ImageButton(Interface.gui);
		imageButton2.InitWithIcon(ImageButtonType.LCD, "globe_icon", hasCheckedState: false);
		selectionPanel.AddContent(imageButton2);
		imageButton2.Width = 35;
		imageButton2.Height = 44;
		imageButton2.ToolTip = "Select the destination point for the mission";
		imageButton2.X = controlXPos;
		label.AlignVertically(imageButton2);
		imageButton2.Click += btDestination_Click;
		imageButton2.RecalculateIconPosition();
		label = new Label(Interface.gui);
		selectionPanel.AddContent(label);
		label.Init(Label.LabelType.LCDHeadingSteelGrey);
		label.Text = "TRANSPORT:";
		label.Y = imageButton2.Bottom + 12;
		label.Width = width;
		cbTransportation = new ComboBox(Interface.gui, ListBoxType.LCDCombo, isEditable: false);
		selectionPanel.AddContent(cbTransportation);
		cbTransportation.Init(ComboBoxTypes.LCD);
		cbTransportation.X = controlXPos;
		label.AlignVertically(cbTransportation);
		cbTransportation.Width = controlWidth;
		cbTransportation.SelectionChanged += cbTransportation_SelectionChanged;
		tbTransportMore = new TextButton(Interface.gui);
		tbTransportMore.Init(TextButton.TextButtonType.LCD);
		selectionPanel.AddContent(tbTransportMore);
		tbTransportMore.Text = "MORE";
		tbTransportMore.Width = 70;
		tbTransportMore.X = cbTransportation.Right + 2;
		label.AlignVertically(tbTransportMore);
		tbTransportMore.Click += tbTransportMore_Click;
		lblTransportNotesOrCost = new Label(Window.guiManager);
		selectionPanel.AddContent(lblTransportNotesOrCost);
		lblTransportNotesOrCost.Init(Label.LabelType.LCDNormal);
		lblTransportNotesOrCost.X = tbTransportMore.Right + 5;
		lblTransportNotesOrCost.Y = cbTransportation.Y + 6;
		lblTransportNotesOrCost.Visible = true;
		lblTransportNotesOrCost.TooltipExpires = false;
		exclamationTransport = new ExclamationMarkInACircle(Window.guiManager);
		selectionPanel.AddContent(exclamationTransport);
		exclamationTransport.X = tbTransportMore.Right + 5;
		exclamationTransport.Y = cbTransportation.Y + 6;
		exclamationTransport.Visible = false;
		Image image = new Image(Interface.gui);
		selectionPanel.AddContent(image);
		image.SetSkinLocation(SkinState.Normal, Interface.gui.GUISpriteSheet.GetSourceRectangle("basic_line"));
		image.X = 5;
		image.Y = cbTransportation.Y - 7;
		image.Width = 561;
		image.Height = 2;
		image.ScaleImageToSizeOfControl = true;
		lblSupplyNeedsHeader = new Label(Interface.gui);
		lblSupplyNeedsHeader.Init(Label.LabelType.LCDHeadingSteelGrey);
		selectionPanel.AddContent(lblSupplyNeedsHeader);
		lblSupplyNeedsHeader.Text = "SUPPLY NEEDS";
		lblSupplyNeedsHeader.Width = labelWidth;
		lblSupplyNeedsHeader.X = 160;
		lblSupplyNeedsHeader.Y = cbTransportation.Bottom + 6;
		lblSupplyNeeds = new Label(Interface.gui);
		lblSupplyNeeds.Init(Label.LabelType.LCDNormal);
		selectionPanel.AddContent(lblSupplyNeeds);
		lblSupplyNeeds.Text = "N/A";
		lblSupplyNeeds.Width = labelWidth;
		lblSupplyNeeds.X = lblSupplyNeedsHeader.X;
		lblSupplyNeeds.Y = lblSupplyNeedsHeader.Bottom + 2;
		lblCostHeader = new Label(Interface.gui);
		lblCostHeader.Init(Label.LabelType.LCDHeadingSteelGrey);
		selectionPanel.AddContent(lblCostHeader);
		lblCostHeader.Width = labelWidth;
		lblCostHeader.X = lblSupplyNeedsHeader.Right + 14;
		lblCostHeader.Y = cbTransportation.Bottom + 6;
		lblCostHeader.Text = "COST / KM";
		lblCost = new Label(Interface.gui);
		lblCost.Init(Label.LabelType.LCDNormal);
		selectionPanel.AddContent(lblCost);
		lblCost.Width = labelWidth;
		lblCost.X = lblCostHeader.X;
		lblCost.Y = lblCostHeader.Bottom + 2;
		int num = 9;
		lblStartingLocation = new Label(Window.guiManager);
		selectionPanel.AddContent(lblStartingLocation);
		lblStartingLocation.Init(Label.LabelType.LCDHeadingBlue);
		lblStartingLocation.X = 161;
		lblStartingLocation.Y = num;
		lblStartingLocation.Visible = false;
		iconHomeStart = CreateHomeIcon();
		iconHomeStart.Y = lblStartingLocation.Y;
		exclamationMarkStart = new ExclamationMarkInACircle(Window.guiManager);
		selectionPanel.AddContent(exclamationMarkStart);
		exclamationMarkStart.X = 161;
		exclamationMarkStart.Y = num + 3;
		exclamationMarkStart.Visible = false;
		lblDestination = new Label(Window.guiManager);
		selectionPanel.AddContent(lblDestination);
		lblDestination.Init(Label.LabelType.LCDHeadingBrown);
		lblDestination.X = 161;
		lblDestination.Y = imageButton.Bottom + 6;
		lblDestination.Visible = false;
		exclamationMarkDestination = new ExclamationMarkInACircle(Window.guiManager);
		selectionPanel.AddContent(exclamationMarkDestination);
		exclamationMarkDestination.X = 161;
		exclamationMarkDestination.Y = imageButton.Bottom + 6 + 4;
		exclamationMarkDestination.Visible = false;
		iconHomeDestination = CreateHomeIcon();
		iconHomeDestination.Y = lblDestination.Y;
		Label label2 = new Label(Window.guiManager);
		lcdSurface.Add(label2);
		label2.Init(Label.LabelType.LCDSmallHeadingBanner);
		label2.Y = num;
		label2.X = 370;
		label2.Text = "TOTAL CARGO:";
		label2.ToolTip = "Total bulk of the cargo";
		lblTotalCargoBulk = new Label(Window.guiManager);
		lcdSurface.Add(lblTotalCargoBulk);
		lblTotalCargoBulk.Init(Label.LabelType.LCDNormal);
		lblTotalCargoBulk.Y = label2.Y;
		lblTotalCargoBulk.X = 478;
		lblAvailableCargoBulk = new Label(Window.guiManager);
		lcdSurface.Add(lblAvailableCargoBulk);
		lblAvailableCargoBulk.Init(Label.LabelType.LCDNormal);
		lblAvailableCargoBulk.Y = label2.Y;
		lblAvailableCargoBulk.X = lblTotalCargoBulk.Right + 6;
		Label label3 = new Label(Window.guiManager);
		lcdSurface.Add(label3);
		label3.Init(Label.LabelType.LCDSmallHeadingBanner);
		label3.Y = imageButton.Bottom + 6;
		label3.X = 370;
		label3.Text = "TOTAL COST:";
		label3.ToolTip = "Total expenses (in trade credits) for starting this run. A negative number here means we will earn credits.";
		lblTotalMissionCost = new Label(Window.guiManager);
		lcdSurface.Add(lblTotalMissionCost);
		lblTotalMissionCost.Init(Label.LabelType.LCDNormal);
		lblTotalMissionCost.Y = label3.Y;
		lblTotalMissionCost.X = 478;
		lblTotalMissionCost.Height = label3.Height;
		lblTotalMissionCost.Visible = false;
		lblTotalMissionCost.TooltipExpires = false;
		lblTotalTradingCredits = new Label(Window.guiManager);
		lcdSurface.Add(lblTotalTradingCredits);
		lblTotalTradingCredits.Init(Label.LabelType.LCDNormal);
		lblTotalTradingCredits.Y = label3.Y;
		lblTotalTradingCredits.X = lblTotalMissionCost.Right + 6;
		Image image2 = new Image(Interface.gui);
		selectionPanel.AddContent(image2);
		image2.SetSkinLocation(SkinState.Normal, Interface.gui.GUISpriteSheet.GetSourceRectangle("basic_line"));
		image2.X = label3.X - 5;
		image2.Y = 5;
		image2.Width = 2;
		image2.Height = 72;
		image2.ScaleImageToSizeOfControl = true;
	}

	private Image CreateHomeIcon()
	{
		Image image = new Image(Window.guiManager);
		image.SetSkinLocation(SkinState.Normal, Window.guiManager.GUISpriteSheet.GetSourceRectangle("HUD_icon_structure"), Color.Green, Color.Green);
		image.ResizeControlToFitImage();
		image.ToolTip = "This is where we are";
		image.Visible = false;
		lcdSurface.Add(image);
		return image;
	}

	private void btStart_Click(UIComponent sender, EventArgs e)
	{
		worldMapDialogSource = WorldMapDialogSource.Start;
		ShowWorldMapDialog(sender.AbsolutePosition);
	}

	private void btDestination_Click(UIComponent sender, EventArgs e)
	{
		if (Mission.StartMissionStopTemplate != null)
		{
			worldMapDialogSource = WorldMapDialogSource.End;
			ShowWorldMapDialog(sender.AbsolutePosition);
		}
		else
		{
			ShowError("Select a starting point first.", null, FieldError.Start);
		}
	}

	private void tbTransportMore_Click(UIComponent sender, EventArgs e)
	{
		if (transportIsExpanded)
		{
			transportIsExpanded = false;
			tbTransportMore.Text = "MORE";
			selectionPanel.ContentHeight = 115;
		}
		else
		{
			transportIsExpanded = true;
			tbTransportMore.Text = "LESS";
			selectionPanel.ContentHeight = 162;
		}
		UpdateMissionStopGridYPosAndHeight();
	}

	private void cbTransportation_SelectionChanged(UIComponent sender)
	{
		if (!start.Value.ResolveLocation(The.InGameUI.UIAllegiance.SharedKnowledge, out var _, out var _, out var expedition, out var _))
		{
			HandleDestroyedMissionStop();
			return;
		}
		Mission.TransportationType = new TransportationTemplate();
		EntityType vehicleType = ((TransportationType)cbTransportation.SelectedKey).VehicleType;
		if (vehicleType != null)
		{
			Mission.TransportationType.Vehicles = new List<Pair<string, int>>
			{
				new Pair<string, int>(vehicleType.KeyName, 1)
			};
		}
		if (!Mission.SelectRoutes())
		{
			HandleDestroyedMissionStop();
			return;
		}
		if (VehiclesAreHired(expedition))
		{
			Mission.TransportationType.HiredFromOwner = (long)((ILookUp<IOwner, OwnerID>)expedition).ID;
		}
		else
		{
			Mission.TransportationType.HiredFromOwner = null;
		}
		EntityGroup ownedEntities = LookUp<Expedition, ExpeditionID>.FindByID((ExpeditionID)destination.Value.ExpeditionID.Value).OwnedEntities;
		if (selectionPanel.Panel.FindChildById(UIComponent.DataControlID.Name) != null)
		{
			selectionPanel.Panel.Remove(selectionPanel.Panel.FindChildById(UIComponent.DataControlID.Name));
		}
		DataTypeButton dataTypeButton = new DataTypeButton(Interface.gui, DataSheet.InfoToShow.Production, vehicleType, GoalEvaluator.GetOwnerID(ownedEntities), useUIOwner: false);
		dataTypeButton.Init(TextButton.TextButtonType.LCDToolTipBlack);
		dataTypeButton.ID = UIComponent.DataControlID.Name;
		selectionPanel.AddContent(dataTypeButton);
		dataTypeButton.IsRoot = true;
		dataTypeButton.TextAlignment = TextButton.TextAlign.Left;
		dataTypeButton.X = 10;
		dataTypeButton.Width = lblSupplyNeedsHeader.X - dataTypeButton.X - 5;
		dataTypeButton.Y = lblSupplyNeedsHeader.Y + 2;
		OutputTransportNotes(expedition);
		UpdateTotalCost();
		PopulateMissionLocations();
		Revalidate();
	}

	private bool VehiclesAreHired(Expedition fromExpedition)
	{
		if (fromExpedition != null)
		{
			return fromExpedition.Allegiance != The.InGameUI.UIAllegiance;
		}
		return false;
	}

	private void CreateStartingLocation()
	{
		if (Mission.StartMissionStopTemplate != null)
		{
			Mission.StartMissionStopTemplate.Destroy();
			Mission.StartMissionStopTemplate = null;
		}
		TravelLocation? travelLocation = start;
		Mission.StartMissionStopTemplate = new MissionStopTemplate(createID: false);
		Mission.StartMissionStopTemplate.TravelLocation = travelLocation.Value;
	}

	private void CreateMissionDestination()
	{
		if (Mission.StartMissionStopTemplate != null && Mission.StartMissionStopTemplate.TravelAction != null)
		{
			Mission.StartMissionStopTemplate.TravelAction.Destroy();
			Mission.StartMissionStopTemplate.TravelAction = null;
		}
		if (destination.HasValue)
		{
			MissionStopTemplate missionStopTemplate = new MissionStopTemplate(createID: false);
			missionStopTemplate.TravelLocation = destination.Value;
			missionStopTemplate.Actions.Enqueue(new UnloadActionTemplate(missionStopTemplate, allowDeleting: false));
			missionStopTemplate.Actions.Enqueue(new DisembarkActionTemplate(missionStopTemplate, allowDeleting: false));
			Mission.AddMissionLocation(missionStopTemplate);
		}
	}

	private TravelLocation? GetReturnDestination()
	{
		if (Mission.StartMissionStopTemplate != null && Mission.StartMissionStopTemplate.TravelAction != null)
		{
			return Mission.StartMissionStopTemplate.TravelAction.ToMissionStop.TravelAction.ToMissionStop.TravelLocation;
		}
		return null;
	}

	private void CreateReturnDestination()
	{
		if (Mission.StartMissionStopTemplate != null && Mission.StartMissionStopTemplate.TravelAction != null)
		{
			if (Mission.StartMissionStopTemplate.TravelAction.ToMissionStop.TravelAction != null)
			{
				Mission.StartMissionStopTemplate.TravelAction.ToMissionStop.TravelAction.Destroy();
			}
			if (start.HasValue)
			{
				MissionStopTemplate missionStopTemplate = new MissionStopTemplate(createID: false);
				missionStopTemplate.TravelLocation = new TravelLocation(start.Value);
				missionStopTemplate.IsLocked = true;
				missionStopTemplate.Actions.Enqueue(new UnloadActionTemplate(missionStopTemplate, allowDeleting: false));
				missionStopTemplate.Actions.Enqueue(new DisembarkActionTemplate(missionStopTemplate, allowDeleting: false));
				Mission.AddMissionLocation(missionStopTemplate);
			}
		}
	}

	private UIComponent AddTravelActionRow(TravelActionTemplate travelAction)
	{
		UIComponent uIComponent = new UIComponent(Interface.gui);
		surfaceGrid.AddEntry(travelAction, uIComponent);
		uIComponent.OrderByTag1 = travelAction.MissionStopTemplate.Number * 2 + 1;
		uIComponent.Height = 30;
		TextButton textButton = new TextButton(Interface.gui);
		textButton.Init(TextButton.TextButtonType.LCD);
		uIComponent.Add(textButton);
		textButton.CheckedMode = CheckedModes.SwitchCheckedStateOnClick;
		textButton.Text = "SHOW";
		textButton.Width = 70;
		textButton.X = 99;
		textButton.Y = 2;
		textButton.Click += tbTravelActionMore_Click;
		textButton.Tag1 = travelAction;
		textButton.ID = UIComponent.DataControlID.Expand;
		Image image = new Image(Interface.gui);
		uIComponent.Add(image);
		image.SetSkinLocation(SkinState.Normal, Interface.gui.GUISpriteSheet.GetSourceRectangle("mission_arrow_small"));
		image.ResizeControlToFitImage();
		image.ID = UIComponent.DataControlID.SmallArrow;
		image.X = 13;
		Image image2 = new Image(Interface.gui);
		uIComponent.Add(image2);
		image2.Y = 6;
		image2.X = -2;
		image2.SetSkinLocation(SkinState.Normal, Interface.gui.GUISpriteSheet.GetSourceRectangle("mission_arrow_big"));
		image2.ResizeControlToFitImage();
		image2.Visible = false;
		image2.ID = UIComponent.DataControlID.BigArrow;
		int width = 176;
		int num = -2;
		int x = 190;
		int y = 43;
		Label label = new Label(Interface.gui);
		uIComponent.Add(label);
		label.Init(Label.LabelType.LCDSmallHeadingBanner);
		label.Text = "DISTANCE:";
		label.X = x;
		label.Y = 8;
		label.Width = width;
		label.Visible = false;
		label.ID = UIComponent.DataControlID.DistanceCaption;
		Label label2 = new Label(Interface.gui);
		uIComponent.Add(label2);
		label2.Init(Label.LabelType.LCDNormal);
		label2.X = label.X + 2;
		label2.Y = label.Bottom + num;
		label2.Width = width;
		label2.Visible = true;
		label2.ID = UIComponent.DataControlID.Distance;
		Label label3 = new Label(Interface.gui);
		uIComponent.Add(label3);
		label3.Init(Label.LabelType.LCDSmallHeadingBanner);
		label3.Text = "CAPACITY:";
		label3.X = label.Right + 5;
		label3.Y = 8;
		label3.Width = width;
		label3.Visible = false;
		label3.ID = UIComponent.DataControlID.CapacityCaption;
		Label label4 = new Label(Interface.gui);
		uIComponent.Add(label4);
		label4.Init(Label.LabelType.LCDNormal);
		label4.X = label3.X + 2;
		label4.Y = label3.Bottom + num;
		label4.Width = width;
		label4.Visible = true;
		label4.ID = UIComponent.DataControlID.Capacity;
		Label label5 = new Label(Interface.gui);
		uIComponent.Add(label5);
		label5.Init(Label.LabelType.LCDSmallHeadingBanner);
		label5.Text = "TRAVEL TIME:";
		label5.X = x;
		label5.Y = y;
		label5.Width = width;
		label5.Visible = true;
		label5.ID = UIComponent.DataControlID.TravelTimeCaption;
		Label label6 = new Label(Interface.gui);
		uIComponent.Add(label6);
		label6.Init(Label.LabelType.LCDNormal);
		label6.X = label5.X + 2;
		label6.Y = label5.Bottom + num;
		label6.Width = width;
		label6.Visible = true;
		label6.ID = UIComponent.DataControlID.TravelTime;
		Label label7 = new Label(Interface.gui);
		uIComponent.Add(label7);
		label7.Init(Label.LabelType.LCDSmallHeadingBanner);
		label7.Text = "FUEL CONSUMPTION:";
		label7.X = label.Right + 5;
		label7.Y = y;
		label7.Width = width;
		label7.Visible = true;
		label7.ID = UIComponent.DataControlID.FuelConsumptionCaption;
		Label label8 = new Label(Interface.gui);
		uIComponent.Add(label8);
		label8.Init(Label.LabelType.LCDNormal);
		label8.X = label3.X;
		label8.Y = label5.Bottom + num;
		label8.Width = width;
		label8.Visible = true;
		label8.Text = "N/A";
		label8.ID = UIComponent.DataControlID.FuelConsumption;
		return uIComponent;
	}

	private void tbTravelActionMore_Click(UIComponent sender, EventArgs e)
	{
		TextButton textButton = sender as TextButton;
		surfaceGrid.TryGetEntry(sender.Tag1, out var item);
		if (textButton.IsChecked)
		{
			item.Height = 87;
		}
		else
		{
			textButton.Text = "SHOW";
			item.Height = 30;
		}
		UpdateTravelActionRow(item, (TravelActionTemplate)sender.Tag1);
	}

	private UIComponent AddMissionLocationRow(MissionStopTemplate missionLocation)
	{
		Label heading = null;
		Label subheading = null;
		TravelLocation travelLocation = missionLocation.TravelLocation;
		UIComponent result = AddMissionLocationPanel(missionLocation, ref heading, ref subheading);
		if (!travelLocation.ResolveLocation(The.InGameUI.UIAllegiance.SharedKnowledge, out var site, out var allegiance, out var expedition, out var _))
		{
			HandleDestroyedMissionStop();
			return result;
		}
		heading.Init((travelLocation.ExpeditionID == start.Value.ExpeditionID) ? Label.LabelType.LCDHeadingBlue : Label.LabelType.LCDHeadingBrown);
		heading.Text = site.Name.ToUpper();
		heading.FitToText();
		subheading.Init(Label.LabelType.LCDNormal);
		subheading.X = heading.Right + 5;
		subheading.Text = allegiance.Name + ", " + expedition.Name;
		subheading.FitToText();
		PopulateLocationActions(missionLocation);
		return result;
	}

	private void UpdateTravelActionRow(UIComponent itemRow, TravelActionTemplate travelAction)
	{
		Image image = (Image)itemRow.FindChildById(UIComponent.DataControlID.SmallArrow);
		Image image2 = (Image)itemRow.FindChildById(UIComponent.DataControlID.BigArrow);
		Label label = (Label)itemRow.FindChildById(UIComponent.DataControlID.DistanceCaption);
		Label label2 = (Label)itemRow.FindChildById(UIComponent.DataControlID.Distance);
		Label label3 = (Label)itemRow.FindChildById(UIComponent.DataControlID.CapacityCaption);
		Label label4 = (Label)itemRow.FindChildById(UIComponent.DataControlID.Capacity);
		TextButton textButton = (TextButton)itemRow.FindChildById(UIComponent.DataControlID.Expand);
		if (textButton.IsChecked)
		{
			image.Visible = false;
			image2.Visible = true;
			label.Visible = true;
			label2.Visible = true;
			label3.Visible = true;
			label4.Visible = true;
			label2.Text = Units.GetKilometersAsString(travelAction.Distance);
			Label label5 = (Label)itemRow.FindChildById(UIComponent.DataControlID.TravelTime);
			if (missionTemplate.TransportationType != null)
			{
				label5.Text = travelAction.GetTravelTime(missionTemplate).ToIntervalString();
				PassengerListTemplate passengerListTemplate = missionTemplate.GetPassengerListTemplate();
				if (passengerListTemplate == null)
				{
					label4.Text = Entity.GetBulkAsString(missionTemplate.ComputeTotalCargoBulk()) + " BULK";
				}
				else
				{
					label4.Text = passengerListTemplate.Passengers.Count() + " Passengers | " + Entity.GetBulkAsString(missionTemplate.ComputeTotalCargoBulk()) + " BULK";
				}
			}
			else
			{
				label5.Text = "UNKNOWN";
			}
			textButton.Text = "HIDE";
			textButton.ToolTip = "Collapse this part";
		}
		else
		{
			image.Visible = true;
			image2.Visible = false;
			label.Visible = false;
			label2.Visible = false;
			label3.Visible = false;
			label4.Visible = false;
			textButton.Text = "SHOW";
			textButton.ToolTip = "Expand to show more information";
		}
	}

	private void UpdateMissionLocationRow(UIComponent itemRow, MissionStopTemplate missionLocation)
	{
		itemRow.OrderByTag1 = missionLocation.Number * 2;
		Label label = (Label)itemRow.FindChildById(UIComponent.DataControlID.ErrorsAndMessages);
		if (Mission.TransportationType == null && !missionLocation.IsLocked)
		{
			label.Visible = true;
			label.Text = "First select a transportation, then add actions to each stop.";
			label.FitToText();
		}
		else
		{
			label.Visible = false;
		}
		PopulateLocationActions(missionLocation);
	}

	private void PopulateMissionLocations()
	{
		surfaceGrid.BeginAddingEntries();
		PopulateMissionLocation(missionTemplate.StartMissionStopTemplate);
		surfaceGrid.Sort((UIComponent i) => (int)(i.OrderByTag1 ?? ((object)1000)), Grid.Sorting.Ascending);
		surfaceGrid.DeleteEntriesWithNullableKey((MissionStopTemplate m) => m == null || missionTemplate.ContainsLocation(m));
		surfaceGrid.EndAddingEntries();
	}

	private void PopulateTravelAction(TravelActionTemplate travelAction)
	{
		if (!surfaceGrid.TryGetEntry(travelAction, out var item))
		{
			item = AddTravelActionRow(travelAction);
		}
		UpdateTravelActionRow(item, travelAction);
	}

	private void PopulateMissionLocation(MissionStopTemplate missionLocation)
	{
		if (!surfaceGrid.TryGetEntry(missionLocation, out var item))
		{
			item = AddMissionLocationRow(missionLocation);
		}
		UpdateMissionLocationRow(item, missionLocation);
		if (missionLocation.TravelAction != null)
		{
			PopulateTravelAction(missionLocation.TravelAction);
			PopulateMissionLocation(missionLocation.TravelAction.ToMissionStop);
		}
		if (start.HasValue)
		{
			lblStartingLocation.Visible = true;
			exclamationMarkStart.Visible = false;
			lblStartingLocation.Text = The.InGameUI.WorldMapDialog.worldMap.GetSiteName((SiteID)start.Value.SiteID);
		}
		if (destination.HasValue)
		{
			lblDestination.Visible = true;
			exclamationMarkDestination.Visible = false;
			lblDestination.Text = The.InGameUI.WorldMapDialog.worldMap.GetSiteName((SiteID)destination.Value.SiteID);
		}
	}

	private void PopulateLocationActions(MissionStopTemplate location)
	{
		HorizontalList horizontalList = (HorizontalList)surfaceGrid.EntriesByKey[location].FindChildById(UIComponent.DataControlID.CurrentOrders);
		Queue<MissionActionTemplate> actions = Mission.GetActionsAtLocation(location);
		bool flag = !location.IsLocked;
		horizontalList.BeginAddingEntries();
		UIComponent entry;
		ImageButton btAction;
		if (actions != null)
		{
			foreach (MissionActionTemplate item in actions)
			{
				int actionType = (int)item.ActionType;
				if (!horizontalList.TryGetEntry(actionType, out entry))
				{
					ClickHandler deleteAction = null;
					if (item.AllowDeleting)
					{
						deleteAction = ibDelete_Click;
					}
					entry = AddAction(horizontalList, actionType, item, item.ActionType, "Click to edit", flag, deleteAction, Interface.gui, out btAction);
					btAction.Click += tbAction_Click;
				}
				UpdateAction(entry, item, flag);
			}
		}
		int newButtonKey = -1;
		if (horizontalList.TryGetEntry(newButtonKey, out entry))
		{
			horizontalList.TryRemoveEntry(newButtonKey);
		}
		if (flag)
		{
			if (entry == null)
			{
				AddCreateActionButton(horizontalList, newButtonKey, location, enabled: true, out btAction);
			}
			else
			{
				horizontalList.AddEntry(newButtonKey, entry);
			}
		}
		horizontalList.DeleteEntries((int r) => r == newButtonKey || actions.FirstOrDefault((MissionActionTemplate a) => a.ActionType == (ActionTypes)r) != null);
		horizontalList.EndAddingEntries();
	}

	private void AddCreateActionButton(HorizontalList hlActions, object key, MissionStopTemplate missionStopTemplate, bool enabled, out ImageButton btAction)
	{
		UIComponent uIComponent = new UIComponent(Interface.gui);
		hlActions.AddEntry(key, uIComponent);
		btAction = new ImageButton(Interface.gui);
		uIComponent.Add(btAction);
		btAction.Init(ImageButtonType.AddAction);
		btAction.Tag1 = missionStopTemplate;
		btAction.Enabled = enabled;
		btAction.ScaleImageToSizeOfControl = false;
		btAction.Click += btAddAction_Click;
		uIComponent.Width = btAction.Width;
		uIComponent.Height = btAction.Height;
		Label label = new Label(Interface.gui);
		uIComponent.Add(label);
		label.Init(Label.LabelType.LCDNormal);
		label.Text = "ADD ACTION";
		label.X = 16;
		label.Y = 14;
	}

	public static UIComponent AddAction(HorizontalList hlActions, object key, object tag, ActionTypes action, string tooltip, bool enabled, ClickHandler deleteAction, GUIManager gui, out ImageButton btAction)
	{
		UIComponent uIComponent = new UIComponent(gui);
		uIComponent.DebugTag = action.ToString();
		hlActions.AddEntry(key, uIComponent);
		btAction = new ImageButton(gui);
		uIComponent.Add(btAction);
		btAction.Init(GetActionButtonType(action));
		btAction.Tag1 = tag;
		btAction.ID = UIComponent.DataControlID.Action;
		btAction.Enabled = enabled;
		btAction.ScaleImageToSizeOfControl = false;
		btAction.ToolTip = tooltip;
		btAction.DebugTag = action.ToString() + "Button";
		uIComponent.Width = btAction.Width;
		uIComponent.Height = btAction.Height;
		Label label = new Label(gui);
		uIComponent.Add(label);
		label.Init(Label.LabelType.LCDNormal);
		label.Text = MissionActionTemplate.GetName(action).ToUpper(Config.Culture);
		label.X = 16;
		label.Y = 14;
		if (deleteAction != null)
		{
			Image image = new Image(gui);
			uIComponent.Add(image);
			image.SetSkinLocation(SkinState.Normal, gui.GUISpriteSheet.GetSourceRectangle("HUD_icon_trash"));
			image.SetSkinLocation(SkinState.Hover, gui.GUISpriteSheet.GetSourceRectangle("HUD_icon_trash"), Color.Gray, Color.Gray);
			image.ToolTip = "Delete this action";
			image.X = 101;
			image.Y = 14;
			image.Color = GameData.Instance.GUIConstants.sidePanelTextColor;
			image.ResizeControlToFitImage();
			image.Click += deleteAction;
		}
		Grid grid = new Grid(gui, ListBoxType.LCD, Label.LabelType.LCDNormal);
		uIComponent.Add(grid);
		grid.FixedItemHeights = true;
		grid.ItemHeight = 16;
		grid.Width = 95;
		grid.Height = 48;
		grid.CanGrowInHeight = false;
		grid.ScrollBarEnabled = false;
		grid.X = btAction.X + 16;
		grid.Y = 29;
		grid.ID = UIComponent.DataControlID.Passengers;
		grid.CanHaveFocus = false;
		grid.DebugTag = "passengerGrid";
		HorizontalList horizontalList = new HorizontalList(gui);
		uIComponent.Add(horizontalList);
		horizontalList.HorizontalSpacing = 6;
		horizontalList.Y = 31;
		horizontalList.Color = new Color(30, 82, 92);
		horizontalList.DebugTag = "goodsList";
		horizontalList.MaxWidth = 94;
		horizontalList.MaxHeight = 48;
		horizontalList.X = btAction.X + 12;
		horizontalList.ID = UIComponent.DataControlID.Goods;
		horizontalList.CanHaveFocus = false;
		return uIComponent;
	}

	private static CargoActionTypes? GetCargoActionType(ActionTypes action)
	{
		return action switch
		{
			ActionTypes.Buy => CargoActionTypes.Buy, 
			ActionTypes.Sell => CargoActionTypes.Sell, 
			_ => null, 
		};
	}

	private static ImageButtonType GetActionButtonType(ActionTypes action)
	{
		return action switch
		{
			ActionTypes.Buy => ImageButtonType.BuyAction, 
			ActionTypes.Sell => ImageButtonType.SellAction, 
			ActionTypes.Load => ImageButtonType.LoadAction, 
			ActionTypes.Unload => ImageButtonType.UnloadAction, 
			ActionTypes.Embark => ImageButtonType.EmbarkAction, 
			ActionTypes.Disembark => ImageButtonType.DisembarkAction, 
			_ => ImageButtonType.BuyAction, 
		};
	}

	private void UpdateAction(UIComponent item, MissionActionTemplate action, bool enabled)
	{
		((ImageButton)item.FindChildById(UIComponent.DataControlID.Action)).Enabled = true;
		SerializableDictionary<string, List<long>> orders = null;
		BuySellActionTemplate buySellActionTemplate = action as BuySellActionTemplate;
		LoadActionTemplate loadActionTemplate = action as LoadActionTemplate;
		if (buySellActionTemplate != null)
		{
			orders = buySellActionTemplate.ContractTemplate.Entities;
		}
		else if (loadActionTemplate != null)
		{
			orders = loadActionTemplate.Orders;
		}
		UpdateGoodsIcons(item, orders);
		List<long> passengers = null;
		if (action is EmbarkActionTemplate embarkActionTemplate)
		{
			passengers = embarkActionTemplate.PassengerListTemplate.Passengers;
		}
		UpdatePassengers(item, passengers);
	}

	private void UpdatePassengers(UIComponent item, List<long> passengers)
	{
		Grid grid = (Grid)item.FindChildById(UIComponent.DataControlID.Passengers);
		grid.BeginAddingEntries();
		if (passengers != null)
		{
			foreach (long passenger in passengers)
			{
				Entity entity = Entity.FindByID((EntityID)passenger);
				if (!grid.TryGetEntry(entity.ID, out var _))
				{
					grid.AddEntry(entity.ID, entity.Name);
				}
			}
			grid.DeleteEntries((EntityID r) => passengers.Contains((long)r));
		}
		else
		{
			grid.Clear();
		}
		grid.EndAddingEntries();
	}

	private void UpdateGoodsIcons(UIComponent item, SerializableDictionary<string, List<long>> orders)
	{
		HorizontalList horizontalList = (HorizontalList)item.FindChildById(UIComponent.DataControlID.Goods);
		horizontalList.BeginAddingEntries();
		horizontalList.ToolTip = "";
		if (orders != null)
		{
			foreach (KeyValuePair<string, List<long>> order in orders)
			{
				EntityType entityType = GameData.Instance.AllEntityTypes[order.Key];
				if (!horizontalList.TryGetEntry(entityType, out var _) && order.Value.Count > 0)
				{
					IconInfo iconInfo;
					Rectangle iconSprite = entityType.GetIconSprite(out iconInfo);
					iconInfo?.GetYPosAdjustment(iconSprite.Height);
					Image image = new Image(Interface.gui);
					image.SetSkinLocation(SkinState.Normal, iconSprite);
					image.Texture = Interface.gui.GUISpriteSheet.Texture;
					image.ResizeControlToFitImage();
					image.OrderByTag1 = (float)image.Width;
					image.CanHaveFocus = false;
					UIComponent uIComponent = new UIComponent(Interface.gui);
					uIComponent.Width = iconSprite.Width;
					uIComponent.Height = 26;
					uIComponent.Add(image);
					uIComponent.CenterChildVertically(image, iconInfo?.CenterYPos);
					uIComponent.CanHaveFocus = false;
					horizontalList.AddEntry(entityType, uIComponent);
				}
			}
			horizontalList.DeleteEntries((EntityType r) => orders.ContainsKey(r.KeyName) && orders[r.KeyName].Count > 0);
		}
		else
		{
			horizontalList.Clear();
		}
		horizontalList.EndAddingEntries();
	}

	private void ibDelete_Click(UIComponent sender, EventArgs e)
	{
		MissionActionTemplate missionActionTemplate = sender.Parent.FindChildById(UIComponent.DataControlID.Action).Tag1 as MissionActionTemplate;
		MissionStopTemplate missionStopTemplate = missionActionTemplate.MissionStopTemplate;
		missionStopTemplate.RemoveAction(missionActionTemplate);
		surfaceGrid.TryGetEntry(missionStopTemplate, out var item);
		UpdateMissionLocationRow(item, missionStopTemplate);
		PopulateLocationActions(missionStopTemplate);
		UpdateTotalCost();
		Revalidate();
	}

	private EntityGroup GetPlayerBuyer()
	{
		if (Mission.GetOwner(The.InGameUI.UIAllegiance.SharedKnowledge, (EntityGroup e) => e.GetAllegiance() == The.InGameUI.UIAllegiance, out var otherOwner))
		{
			return otherOwner;
		}
		HandleDestroyedMissionStop();
		return null;
	}

	private EntityGroup GetNPCBuyer()
	{
		if (Mission.GetOwner(The.InGameUI.UIAllegiance.SharedKnowledge, (EntityGroup e) => e.GetAllegiance() != The.InGameUI.UIAllegiance, out var otherOwner))
		{
			return otherOwner;
		}
		HandleDestroyedMissionStop();
		return null;
	}

	private void tbAction_Click(UIComponent sender, EventArgs e)
	{
		MissionActionTemplate missionActionTemplate = sender.Tag1 as MissionActionTemplate;
		MissionStopTemplate missionStopTemplate = missionActionTemplate.MissionStopTemplate;
		if (!missionStopTemplate.TravelLocation.ResolveLocation(The.InGameUI.UIAllegiance.SharedKnowledge, out var _, out var _, out var expedition, out var _))
		{
			HandleDestroyedMissionStop();
			return;
		}
		switch (missionActionTemplate.ActionType)
		{
		case ActionTypes.Buy:
			EditBuySellAction(CargoActionTypes.Buy, sender, missionActionTemplate, missionStopTemplate, expedition, null);
			break;
		case ActionTypes.Sell:
		{
			EntityGroup nPCBuyer = GetNPCBuyer();
			EditBuySellAction(CargoActionTypes.Sell, sender, missionActionTemplate, missionStopTemplate, expedition, nPCBuyer);
			break;
		}
		case ActionTypes.Embark:
			dialogSourceLocation = missionStopTemplate;
			dialogSourceActionTemplate = missionActionTemplate;
			ShowPersonnelDialog(sender.AbsolutePosition);
			break;
		}
	}

	private void EditBuySellAction(CargoActionTypes cargoAction, UIComponent sender, MissionActionTemplate action, MissionStopTemplate missionStop, Expedition expedition, EntityGroup buyer)
	{
		BuySellActionTemplate buySellActionTemplate = action as BuySellActionTemplate;
		dialogSourceAction = cargoAction;
		dialogSourceLocation = missionStop;
		dialogSourceActionTemplate = action;
		ShowBuySellDialog(sender.AbsolutePosition, cargoAction, buySellActionTemplate.ContractTemplate.Entities.ToDictionary((KeyValuePair<string, List<long>> k) => GameData.Instance.AllEntityTypes[k.Key], (KeyValuePair<string, List<long>> k) => k.Value.Select((long e) => (EntityID)e).ToList()), expedition, buyer);
	}

	public override void Refresh()
	{
		base.Refresh();
		if (The.InGameUI.WorldMapDialog.Window.IsVisibleAndActive)
		{
			The.InGameUI.WorldMapDialog.Refresh();
		}
		if (The.InGameUI.PersonnelDialog.Window.IsVisibleAndActive)
		{
			The.InGameUI.PersonnelDialog.Refresh();
		}
		if (The.InGameUI.BuySellDialog.Window.IsVisibleAndActive)
		{
			The.InGameUI.BuySellDialog.Refresh();
		}
		if (ActionPicker.Visible)
		{
			ActionPicker.Refresh();
		}
	}

	private UIComponent AddMissionLocationPanel(MissionStopTemplate locationKey, ref Label heading, ref Label subheading)
	{
		LCDInnerPanel lCDInnerPanel = new LCDInnerPanel(Interface.gui, lcdSurface.Width, includeDecor: true, 1f);
		surfaceGrid.AddEntry(locationKey, lCDInnerPanel.Panel);
		lCDInnerPanel.ContentHeight = 144;
		lCDInnerPanel.Panel.Y = 12;
		lCDInnerPanel.VerticalContentPadding = 8;
		lCDInnerPanel.Panel.OrderByTag1 = locationKey.Number * 2;
		heading = new Label(Interface.gui);
		lCDInnerPanel.AddContent(heading, 2, -1);
		heading.Init(Label.LabelType.LCDHeadingGreen);
		heading.FitToText();
		subheading = new Label(Interface.gui);
		lCDInnerPanel.AddContent(subheading, heading.Right);
		subheading.Init(Label.LabelType.LCDNormal);
		subheading.FitToText();
		Grid grid = new Grid(Interface.gui, ListBoxType.LCD, Label.LabelType.LCDNormal);
		grid.FixedItemHeights = false;
		grid.RenderType = RenderType.CRTAndLCD;
		lCDInnerPanel.AddContent(grid);
		grid.Font = GUIManager.LCDandHUDBodyFontPath;
		grid.Width = 510;
		grid.Height = 0;
		grid.Y = heading.Bottom + 12;
		grid.CanGrowInHeight = true;
		grid.ScrollBarEnabled = false;
		grid.Selectability = Grid.SelectabilityOptions.None;
		grid.DebugTag = "contentGrid";
		grid.IsOuterGrid = false;
		grid.BeginAddingEntries();
		HorizontalList horizontalList = new HorizontalList(Interface.gui);
		horizontalList.RenderType = RenderType.CRTAndLCD;
		horizontalList.X = 2;
		grid.AddEntry(horizontalList, horizontalList);
		horizontalList.Font = GUIManager.LCDandHUDBodyFontPath;
		horizontalList.Width = grid.Width;
		horizontalList.Height = 0;
		horizontalList.HorizontalSpacing = 3;
		horizontalList.ID = UIComponent.DataControlID.CurrentOrders;
		UIComponent uIComponent = new UIComponent(Interface.gui);
		uIComponent.Height = 22;
		Label label = new Label(Interface.gui);
		uIComponent.Add(label);
		label.Init(Label.LabelType.LCDError);
		label.Y = horizontalList.Bottom + 6;
		label.ID = UIComponent.DataControlID.ErrorsAndMessages;
		grid.AddEntry(uIComponent, uIComponent);
		grid.EndAddingEntries();
		return lCDInnerPanel.Panel;
	}

	public override void Show()
	{
		base.Show();
		Populate();
	}

	private void Populate()
	{
		_ = missionTemplate;
	}

	public override void Hide()
	{
		base.Hide();
		HideOpenDialogs();
		start = null;
		destination = null;
		ClearForm();
	}

	private void HideOpenDialogs()
	{
		ResetAfterBuySellDialog();
		ResetAfterPersonnelDialog();
		ResetAfterWorldMapDialog();
		The.InGameUI.BuySellDialog.Hide();
		The.InGameUI.PersonnelDialog.Hide();
		The.InGameUI.WorldMapDialog.Hide();
	}

	protected override void OnCancel()
	{
		base.OnCancel();
		DestroyMission();
	}

	private void DestroyMission()
	{
		if (missionTemplate != null)
		{
			missionTemplate.Destroy();
			missionTemplate = null;
		}
	}

	private void ClearForm()
	{
		missionTemplate = null;
		dialogSourceLocation = null;
		dialogSourceAction = null;
		The.InGameUI.WorldMapDialog.UnCheckSiteMarkerButtons();
		lblTotalMissionCost.Text = "";
		lblAvailableCargoBulk.Text = "";
		lblTotalCargoBulk.Text = "";
		lblCost.Text = "";
		lblTotalTradingCredits.Text = "";
		iconHomeStart.Visible = false;
		iconHomeDestination.Visible = false;
		ClearErrors();
		DataTypeButton dataTypeButton = (DataTypeButton)selectionPanel.Panel.FindChildById(UIComponent.DataControlID.Name);
		if (dataTypeButton != null)
		{
			selectionPanel.Panel.Remove(dataTypeButton);
		}
		cbTransportation.Clear();
		exclamationMarkDestination.Visible = false;
		exclamationMarkStart.Visible = false;
		exclamationTransport.Visible = false;
		lblTransportNotesOrCost.Visible = false;
		lblDestination.Visible = false;
		lblStartingLocation.Visible = false;
		transportIsExpanded = false;
		tbTransportMore.Text = "MORE";
		selectionPanel.ContentHeight = 115;
		errorAndMessagePanel.Clear();
		foreach (KeyValuePair<object, UIComponent> item in surfaceGrid.EntriesByKey)
		{
			if (item.Key is MissionStopTemplate)
			{
				surfaceGrid.TryRemoveEntry(item);
			}
		}
		surfaceGrid.Clear();
	}

	private bool CanCommunicateWithExpedition(Expedition expedition)
	{
		CommunicationMethod? workingMethod;
		return Communicates.IsInCommunicationRange(expedition.Allegiance, The.InGameUI.UIAllegiance, out workingMethod);
	}

	private bool PopulateTransportation()
	{
		Mission.TransportationType = null;
		cbTransportation.BeginAddingEntries();
		cbTransportation.Clear();
		if (start.HasValue && destination.HasValue)
		{
			if (!start.Value.ResolveLocation(The.InGameUI.UIAllegiance.SharedKnowledge, out var site, out var allegiance, out var expedition, out var _))
			{
				HandleDestroyedMissionStop();
				return false;
			}
			if (!destination.Value.ResolveLocation(The.InGameUI.UIAllegiance.SharedKnowledge, out var site2, out var allegiance2, out var _, out var _))
			{
				HandleDestroyedMissionStop();
				return false;
			}
			if (allegiance != null && expedition != null && allegiance != allegiance2)
			{
				if (VehiclesAreHired(expedition) && !CanCommunicateWithExpedition(expedition))
				{
					cbTransportation.Enabled = false;
				}
				else
				{
					cbTransportation.Enabled = true;
					GetVehicleTransportationOptions(site, expedition, site2, out var vehicles, out var _);
					if (vehicles != null)
					{
						foreach (KeyValuePair<EntityType, List<Entity>> item in vehicles)
						{
							string text = item.Key.Name;
							if (expedition.Allegiance != The.InGameUI.UIAllegiance)
							{
								text += " (HIRED)";
							}
							cbTransportation.AddEntry(new TransportationType
							{
								VehicleType = item.Key
							}, text);
						}
					}
				}
			}
			OutputTransportNotes(expedition);
		}
		else
		{
			OutputTransportNotes(null);
		}
		cbTransportation.EndAddingEntries();
		return true;
	}

	private void SetDefaultStartExpedition()
	{
		List<TravelLocation> allTravelLocations = GetAllTravelLocations();
		foreach (TravelLocation item in allTravelLocations)
		{
			TravelLocation travelLocation = item;
			if (!travelLocation.ResolveLocation(The.InGameUI.UIAllegiance.SharedKnowledge, out var site, out var _, out var expedition, out var _))
			{
				continue;
			}
			foreach (TravelLocation item2 in allTravelLocations)
			{
				if (item.Equals(item2))
				{
					continue;
				}
				TravelLocation travelLocation2 = item2;
				if (travelLocation2.ResolveLocation(The.InGameUI.UIAllegiance.SharedKnowledge, out var site2, out var _, out var _, out var _))
				{
					GetVehicleTransportationOptions(site, expedition, site2, out var vehicles, out var _);
					if (vehicles != null && vehicles.Any((KeyValuePair<EntityType, List<Entity>> k) => k.Value.Count > 0))
					{
						start = item;
						destination = item2;
						return;
					}
				}
			}
		}
	}

	private static void GetVehicleTransportationOptions(Site fromSite, Expedition fromExpedition, Site toSite, out Dictionary<EntityType, List<Entity>> vehicles, out bool hasLandRoute)
	{
		List<Tuple<Route, double>> routesAndDistances = The.Sim.World.GetRoutesAndDistances(fromSite, toSite);
		vehicles = new Dictionary<EntityType, List<Entity>>();
		hasLandRoute = false;
		foreach (Tuple<Route, double> item in routesAndDistances)
		{
			bool airRoute = false;
			RouteType? routeType = null;
			if (item.Item1 == null)
			{
				airRoute = true;
			}
			else
			{
				routeType = item.Item1.RouteType;
			}
			fromExpedition.GetAvailableVehicles(routeType, airRoute, item.Item2, ref vehicles, null);
			if (routeType == RouteType.Land)
			{
				hasLandRoute = true;
			}
		}
	}

	private void OutputTransportNotes(Expedition fromExpedition)
	{
		bool flag = VehiclesAreHired(fromExpedition);
		if (flag && !CanCommunicateWithExpedition(fromExpedition))
		{
			ShowError("No communication with the Start allegiance.", "To hire transports from another allegiance, we need to establish communication first. A ground satellite station is a good option.");
		}
		else if (cbTransportation.EntriesByKey.Count == 0 && start.HasValue && destination.HasValue)
		{
			ShowError("No modes of transport are available from Start to Destination.", null, FieldError.Transport);
		}
		else if (cbTransportation.SelectedKey != null)
		{
			EntityType vehicleType = ((TransportationType)cbTransportation.SelectedKey).VehicleType;
			if (flag && vehicleType != null)
			{
				lblTransportNotesOrCost.Visible = true;
				exclamationTransport.Visible = false;
				decimal startFee;
				decimal totalDistanceCost;
				decimal costPerKilometer;
				decimal value = missionTemplate.ComputeTransportationCost(out startFee, out totalDistanceCost, out costPerKilometer);
				lblTransportNotesOrCost.Text = "COST TO HIRE: " + Common.GetPriceAsString(value);
				StringBuilder stringBuilder = new StringBuilder();
				Common.AppendLine(stringBuilder, "Hired transport cost");
				Common.AppendLine(stringBuilder, "The vehicle is not owned by us, but we can hire it for a price.");
				Common.AppendDivider(stringBuilder);
				Common.Append(stringBuilder, "Starting fee: ");
				Common.Append(stringBuilder, Common.GetPriceAsString(startFee), tintAsValue: true);
				Common.AppendLine(stringBuilder);
				Common.Append(stringBuilder, "Cost per kilometer: ");
				Common.Append(stringBuilder, Common.GetPriceAsString(costPerKilometer), tintAsValue: true);
				Common.AppendLine(stringBuilder);
				Common.Append(stringBuilder, "Distance cost: ");
				Common.Append(stringBuilder, Common.GetPriceAsString(totalDistanceCost), tintAsValue: true);
				Common.AppendLine(stringBuilder);
				Common.AppendLine(stringBuilder);
				Common.Append(stringBuilder, "Total cost: ");
				Common.Append(stringBuilder, Common.GetPriceAsString(value), tintAsValue: true);
				lblTransportNotesOrCost.ToolTip = stringBuilder.ToString();
			}
			else
			{
				lblTransportNotesOrCost.Text = "";
				lblTransportNotesOrCost.Visible = false;
			}
		}
		else
		{
			lblTransportNotesOrCost.Visible = false;
		}
	}

	private List<TravelLocation> GetAllTravelLocations()
	{
		return new List<TravelLocation>();
	}

	public bool HasTransportationIfStart(TravelLocation travelLocation)
	{
		if (worldMapDialogSource == WorldMapDialogSource.Start)
		{
			if (travelLocation.ResolveLocation(The.InGameUI.UIAllegiance.SharedKnowledge, out var _, out var _, out var expedition, out var _))
			{
				return expedition.HasAavailableVehicle();
			}
			HandleDestroyedMissionStop();
			return false;
		}
		return true;
	}

	private bool TransportCanUseTerminal(TravelLocation toTravelLocation)
	{
		if (missionTemplate.TransportationType != null)
		{
			EntityType mainTransportation = missionTemplate.TransportationType.GetMainTransportation();
			if (mainTransportation != null)
			{
				VehicleContainerType vehicle = mainTransportation.ContainerType as VehicleContainerType;
				List<string> errors = null;
				return TravelActionTemplate.CanUseTerminal(missionTemplate, toTravelLocation, vehicle, ref errors);
			}
		}
		return true;
	}

	internal bool CanSelectLocation(TravelLocation travelLocation, out bool isAlreadySelected, out bool wrongTerminalType)
	{
		isAlreadySelected = false;
		wrongTerminalType = false;
		if (worldMapDialogSource == WorldMapDialogSource.End)
		{
			if (destination.HasValue)
			{
				if (destination.Value.AllegianceID == travelLocation.AllegianceID)
				{
					long? expeditionID = destination.Value.ExpeditionID;
					long? expeditionID2 = travelLocation.ExpeditionID;
					if (expeditionID.GetValueOrDefault() == expeditionID2.GetValueOrDefault() && expeditionID.HasValue == expeditionID2.HasValue && destination.Value.TerminalEntityID == travelLocation.TerminalEntityID)
					{
						isAlreadySelected = true;
						return false;
					}
				}
				if (start.HasValue)
				{
					long? expeditionID = travelLocation.AllegianceID;
					long? expeditionID2 = start.Value.AllegianceID;
					if (expeditionID.GetValueOrDefault() == expeditionID2.GetValueOrDefault() && expeditionID.HasValue == expeditionID2.HasValue && travelLocation.ExpeditionID == start.Value.ExpeditionID)
					{
						isAlreadySelected = true;
						return false;
					}
				}
				if (!TransportCanUseTerminal(travelLocation))
				{
					wrongTerminalType = true;
					return false;
				}
			}
			else
			{
				long? expeditionID = travelLocation.AllegianceID;
				long? expeditionID2 = start.Value.AllegianceID;
				if (expeditionID.GetValueOrDefault() == expeditionID2.GetValueOrDefault() && expeditionID.HasValue == expeditionID2.HasValue && travelLocation.ExpeditionID == start.Value.ExpeditionID)
				{
					isAlreadySelected = true;
					return false;
				}
			}
		}
		else if (start.HasValue)
		{
			long? expeditionID = start.Value.AllegianceID;
			long? expeditionID2 = travelLocation.AllegianceID;
			if (expeditionID.GetValueOrDefault() == expeditionID2.GetValueOrDefault() && expeditionID.HasValue == expeditionID2.HasValue && start.Value.ExpeditionID == travelLocation.ExpeditionID)
			{
				isAlreadySelected = true;
				return false;
			}
		}
		else if (destination.HasValue)
		{
			long? expeditionID = travelLocation.AllegianceID;
			long? expeditionID2 = destination.Value.AllegianceID;
			if ((expeditionID.GetValueOrDefault() == expeditionID2.GetValueOrDefault() && expeditionID.HasValue == expeditionID2.HasValue && travelLocation.ExpeditionID == destination.Value.ExpeditionID) || travelLocation.TerminalEntityID == destination.Value.TerminalEntityID)
			{
				isAlreadySelected = true;
				return false;
			}
		}
		return true;
	}
}
