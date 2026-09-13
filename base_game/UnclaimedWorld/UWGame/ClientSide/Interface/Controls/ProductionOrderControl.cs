using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface.Inventory;
using UWGame.SimSide;
using UWGame.SimSide.Commands;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Policies;
using UWGame.SimSide.Processes;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Controls;

public class ProductionOrderControl : UIComponent
{
	public enum UILayout
	{
		HUD,
		LCD
	}

	public const string btPadlockTooltip = "Switch to standing order mode.";

	public const string btPadlockEnabledTooltip = "Standing order mode. In this mode, production will start and continue whenever the inventory is below the slider value. \nClick to switch back to direct order mode.";

	private const string tbBuildToolTip = "Build: Click the button, then place the structure on the terrain";

	private const string lblMaxOrderToolTip = "Maximum number of structures we can build";

	private const string lblImmovableToolTip = "{0} must first be selected, then the item can be built from the action menu";

	public const string orderSpamWarning = "Ordering many single items can take a while to produce. Try to look for ways to produce in larger batches, as this cuts down on the production time.";

	private EntityType entityType;

	private StockButton btStockAvailable;

	private StockButton btStockUnavailable;

	private ImageButton btStandingOrder;

	private Label lblMaxOrder;

	private ImageButton btBuild;

	private HorizontalList hzAttainable;

	private HorizontalList hzNotAttainable;

	private FillableBar fillableBar;

	private Icon icWarning;

	private bool isFirstUpdate = true;

	private bool isSliderBeingDragged;

	public ProductionOrderControl(EntityType entityType, ProductionTargetEventArgs eventArgs, GUIManager gui, UILayout uiLayout, int productionColumnX, Action<UIComponent, EventArgs> tbItems_Click, Action<UIComponent, EventArgs> btPadlock_Click)
		: base(gui)
	{
		this.entityType = entityType;
		base.Width = 224;
		base.Height = 25;
		btStockAvailable = new StockButton(guiManager, entityType);
		Add(btStockAvailable);
		btStockAvailable.Position = new Point(0, 0);
		btStockAvailable.Click += tbItems_Click.Invoke;
		CenterChildVertically(btStockAvailable);
		btStockUnavailable = new StockButton(guiManager, entityType);
		Add(btStockUnavailable);
		btStockUnavailable.Position = new Point(39, 0);
		btStockUnavailable.Click += tbItems_Click.Invoke;
		CenterChildVertically(btStockUnavailable);
		hzNotAttainable = new HorizontalList(guiManager);
		Add(hzNotAttainable);
		hzNotAttainable.X = productionColumnX;
		hzNotAttainable.Height = 21;
		CenterChildVertically(hzNotAttainable);
		hzAttainable = new HorizontalList(guiManager);
		Add(hzAttainable);
		hzAttainable.X = productionColumnX;
		hzAttainable.Height = 21;
		CenterChildVertically(hzAttainable);
		if (entityType.StructureType == null)
		{
			fillableBar = new FillableBar(guiManager, (uiLayout == UILayout.LCD) ? FillableBar.FillableBarType.LCDSliderWhite : FillableBar.FillableBarType.HUDSliderWhite, canGrow: false, includeButtons: true, GameData.Instance.GUIConstants.TimeBetweenSliderButtonIncrements, GameData.Instance.GUIConstants.SliderButtonDelay);
			Add(fillableBar);
			fillableBar.Width = 130;
			fillableBar.X = productionColumnX;
			fillableBar.Y = 5;
			fillableBar.SliderTooltip = "Drag slider to specify amount to produce.";
			fillableBar.ButtonTooltip = "Click or hold the mouse button to change the amount to produce.";
			fillableBar.EventArgs = eventArgs;
			fillableBar.SliderMouseDown += fillableBar_SliderMouseDown;
			fillableBar.SliderMouseUp += fillableBar_SliderMouseUp;
			fillableBar.ShowNotches = true;
			icWarning = new Icon(guiManager);
			Add(icWarning);
			icWarning.ToolTip = "Ordering many single items can take a while to produce. Try to look for ways to produce in larger batches, as this cuts down on the production time.";
			icWarning.SetSkinLocation(SkinState.Normal, guiManager.GUISpriteSheet.GetSourceRectangle("HUD_icon_status_exclamation"), Color.Red, Color.Red);
			icWarning.ResizeControlToFitImage();
			icWarning.X = fillableBar.Right - 7;
			icWarning.Y = 2;
		}
		else
		{
			fillableBar = null;
			btBuild = new ImageButton(guiManager);
			Add(btBuild);
			btBuild.CheckedMode = CheckedModes.CannotBeChecked;
			if (uiLayout == UILayout.LCD)
			{
				btBuild.InitWithIcon(ImageButtonType.LCD, "basic_icon_hammer", hasCheckedState: false);
			}
			else
			{
				btBuild.InitWithIcon(ImageButtonType.HUD, "basic_icon_hammer_white", hasCheckedState: false);
			}
			btBuild.Position = new Point(productionColumnX + 10, 0);
			btBuild.EventArgs = eventArgs;
			btBuild.Click += build_Click;
			btBuild.ToolTip = "Build: Click the button, then place the structure on the terrain";
			btBuild.DebugTag = "Build Debug";
			CenterChildVertically(btBuild);
			lblMaxOrder = new Label(guiManager);
			Add(lblMaxOrder);
			if (uiLayout == UILayout.LCD)
			{
				lblMaxOrder.Init(Label.LabelType.LCDNormal);
			}
			else
			{
				lblMaxOrder.Init(Label.LabelType.HUDWindow);
			}
			lblMaxOrder.X = btBuild.Right + 16;
			CenterChildVertically(lblMaxOrder);
			lblMaxOrder.ToolTip = "Maximum number of structures we can build";
		}
		if (GameData.Instance.GUIConstants.EnableStandingOrders)
		{
			btStandingOrder = new ImageButton(guiManager);
			Add(btStandingOrder);
			btStandingOrder.Init(ImageButtonType.LCDPadlockWhite);
			btStandingOrder.Position = new Point(3, 0);
			btStandingOrder.EventArgs = eventArgs;
			btStandingOrder.Click += btPadlock_Click.Invoke;
			btStandingOrder.Visible = true;
			btStandingOrder.ToolTip = "Switch to standing order mode.";
			CenterChildVertically(btStandingOrder);
			btStandingOrder.X = productionColumnX - 16;
		}
	}

	private void fillableBar_SliderMouseUp(object sender, EventArgs e)
	{
		FillableBar fillableBar = sender as FillableBar;
		EntityType item = ((ProductionTargetEventArgs)e).Item;
		ExpeditionID? uIExpedition = The.InGameUI.UIExpedition;
		if (uIExpedition.HasValue)
		{
			if (btStandingOrder != null && btStandingOrder.IsChecked)
			{
				SetStandingOrder command = new SetStandingOrder(newCount: (fillableBar.Value != GameData.Instance.GUIConstants.UnlimitedStandingOrderValue) ? fillableBar.Value : (-1), expeditionID: uIExpedition.Value, entityTypeKey: item.KeyName, giveClientFeedback: true);
				The.Client.Controller.StoreAndExecuteCommand(command);
				return;
			}
			int value = fillableBar.Value;
			int noOfJobsFromOutputAmount = GetNoOfJobsFromOutputAmount(((ProductionTargetEventArgs)e).OutputBatchAmount, value);
			SetProduction command2 = new SetProduction(uIExpedition.Value, item.KeyName, noOfJobsFromOutputAmount, giveClientFeedback: true);
			The.Client.Controller.StoreAndExecuteCommand(command2);
		}
	}

	private int GetNoOfJobsFromOutputAmount(int? outputBatchAmount, int outputAmount)
	{
		return outputAmount / (outputBatchAmount ?? 1);
	}

	public static void SetPadlockButtonState(bool hasOrder, ImageButton btStandingOrder)
	{
		if (hasOrder)
		{
			btStandingOrder.IsChecked = true;
			btStandingOrder.ToolTip = "Standing order mode. In this mode, production will start and continue whenever the inventory is below the slider value. \nClick to switch back to direct order mode.";
		}
		else
		{
			btStandingOrder.IsChecked = false;
			btStandingOrder.ToolTip = "Switch to standing order mode.";
		}
	}

	public static void Padlock_Click(UIComponent sender, EventArgs e)
	{
		Expedition expedition = The.InGameUI.GetExpedition();
		if (expedition != null)
		{
			ProductionTargetEventArgs e2 = e as ProductionTargetEventArgs;
			ImageButton imageButton = sender as ImageButton;
			if (imageButton.IsChecked)
			{
				imageButton.ToolTip = "Standing order mode. In this mode, production will start and continue whenever the inventory is below the slider value. \nClick to switch back to direct order mode.";
				SetStandingOrder command = new SetStandingOrder(expedition.ID, e2.Item.KeyName, 0, giveClientFeedback: true);
				The.Client.Controller.StoreAndExecuteCommand(command);
			}
			else
			{
				imageButton.ToolTip = "Switch to standing order mode.";
				SetProduction command2 = new SetProduction(expedition.ID, e2.Item.KeyName, 0, giveClientFeedback: true);
				The.Client.Controller.StoreAndExecuteCommand(command2);
			}
		}
	}

	private static void build_Click(UIComponent sender, EventArgs e)
	{
		EntityType item = ((ProductionTargetEventArgs)e).Item;
		The.InGameUI.InterfaceMode = InGameInterface.InterfaceState.Build;
		Entity entity = new Entity(item, isStructureBeingPlaced: true);
		entity.NonLivingEntity.Progress = 0f;
		entity.Initialize(The.Sim.PlaySite, The.Sim.PlaySite.PlayerAllegiance);
		entity.InitializeModelAndOnScreenFunctionality();
		entity.Renderable.SetOverlayRendering(value: true);
		The.InGameUI.EntitiesBeingPlaced.Add(new InGameInterface.EntityPosition
		{
			Entity = entity,
			Position = Vector2.Zero
		});
	}

	public void UpdateOrders(EntityGroup owner, Dictionary<EntityType, InventoryPanel.Availability> allAvailableItems, out int noOfAvailableItems)
	{
		noOfAvailableItems = InventoryPanel.GetNoOfAvailableEntities(owner.AllEntities, owner, entityType, out var noOfIncompleteEntities, out var noOfEntitiesUsedAsParts, out var noOfItemsOnOtherSite, out var noOfItemsOwnedByOthers, out var _, out var _, out var listOfAvailableEntities, out var listOfUnavailableEntities, allAvailableItems);
		UpdateItemRowAvailableStockButton(listOfAvailableEntities, noOfAvailableItems);
		UpdateItemRowUnavailableStockButton(listOfUnavailableEntities, noOfIncompleteEntities, noOfEntitiesUsedAsParts, noOfItemsOwnedByOthers, noOfItemsOnOtherSite);
		bool hasInputs;
		bool hasTools;
		int maxAmountThatCanBeProduced;
		int? noOfMissingInputTypes;
		int? noOfAvailableInputTypes;
		bool hasSkills;
		bool hasResources;
		bool hasSpecialSite;
		bool hasPolicy;
		EntityType needsImmovableInput;
		int? outputBatchAmount;
		ProcessType processType;
		bool bestProcessForDisplay = InventoryPanel.GetBestProcessForDisplay(entityType, owner, out hasInputs, out hasTools, out maxAmountThatCanBeProduced, out noOfMissingInputTypes, out noOfAvailableInputTypes, out hasSkills, out hasResources, out hasSpecialSite, out hasPolicy, out needsImmovableInput, out outputBatchAmount, out processType, The.InGameUI.InventorySettings.IncludeSalvageProcesses, (ProcessType p) => !p.IsSalvageProcess && !p.IsPartOfProductionChainButCannotOrderFromInventory(), allAvailableItems);
		Dictionary<ProcessType, AttainableInfo> dictionary = null;
		if (!bestProcessForDisplay)
		{
			dictionary = The.InGameUI.InventorySettings.GetAttainableInfo(entityType);
		}
		ProcessType.ProductionUI productionUI = processType?.GetProductionUI() ?? ProcessType.ProductionUI.None;
		if (productionUI == ProcessType.ProductionUI.Build)
		{
			UpdateItemRowStructureType(processType, allAvailableItems, dictionary, noOfIncompleteEntities, needsImmovableInput, maxAmountThatCanBeProduced, hasTools, hasInputs, hasSkills, hasResources);
		}
		else
		{
			UpdateItemRowNormalProduction(owner, allAvailableItems, dictionary, entityType, processType, hasTools, hasInputs, hasSkills, hasResources, maxAmountThatCanBeProduced, outputBatchAmount, productionUI);
		}
		switch (The.InGameUI.InventorySettings.SortingSettings.SortedBy)
		{
		case InventorySettings.SortColumns.Name:
			SetOrderBy(entityType.PluralName);
			break;
		case InventorySettings.SortColumns.CanProduce:
			if (bestProcessForDisplay)
			{
				if (productionUI == ProcessType.ProductionUI.Slider || productionUI == ProcessType.ProductionUI.Build)
				{
					SetOrderBy(10000 * (outputBatchAmount ?? 1) * maxAmountThatCanBeProduced);
				}
				else
				{
					Parent.OrderByTag1 = 100;
				}
			}
			else if (dictionary != null)
			{
				if (dictionary.Any((KeyValuePair<ProcessType, AttainableInfo> a) => a.Value.IsProducable))
				{
					SetOrderBy(2);
				}
				else
				{
					SetOrderBy(1);
				}
			}
			else
			{
				SetOrderBy(0);
			}
			break;
		case InventorySettings.SortColumns.InStock:
		{
			int sumToOrderBy = GetSumToOrderBy(noOfIncompleteEntities, noOfEntitiesUsedAsParts, noOfAvailableItems, noOfItemsOnOtherSite);
			SetOrderBy(sumToOrderBy);
			break;
		}
		}
		isFirstUpdate = false;
	}

	private int GetSumToOrderBy(int noOfIncompleteItems, int noOfEntitiesUsedAsParts, int noOfAvailableItems, int noOfItemsOffSite)
	{
		return 100 * noOfAvailableItems + 10 * (noOfEntitiesUsedAsParts + noOfItemsOffSite) + noOfIncompleteItems;
	}

	private void SetOrderBy(object order)
	{
		Parent.OrderByTag1 = order;
	}

	private void UpdateItemRowAvailableStockButton(List<EntityID> listOfEntities, int noOfAvailableItems)
	{
		btStockAvailable.UpdateStockButton(noOfAvailableItems, null, null, null, null, listOfEntities, hideIfZero: false);
	}

	private void UpdateItemRowUnavailableStockButton(List<EntityID> listOfEntities, int noOfIncompleteItems, int noOfEntitiesUsedAsParts, int noOfItemsOwnedByOthers, int noOfOffSiteItems)
	{
		btStockUnavailable.UpdateStockButton(null, noOfIncompleteItems, noOfEntitiesUsedAsParts, noOfOffSiteItems, noOfItemsOwnedByOthers, listOfEntities, hideIfZero: true);
	}

	private void UpdateItemRowStructureType(ProcessType process, Dictionary<EntityType, InventoryPanel.Availability> allAvailableItems, Dictionary<ProcessType, AttainableInfo> attainableInfo, int noOfIncompleteItems, EntityType immovableInput, int productionLimit, bool hasTools, bool hasInputs, bool hasSkills, bool hasResources)
	{
		if (GameData.Instance.GUIConstants.EnableStandingOrders)
		{
			btStandingOrder.Visible = false;
		}
		bool canProduceNow = productionLimit > 0;
		int num = Common.ClampBottom(productionLimit - noOfIncompleteItems, 0);
		lblMaxOrder.Text = num.ToString();
		if (num == 0)
		{
			lblMaxOrder.Visible = false;
		}
		else
		{
			lblMaxOrder.Visible = true;
		}
		if (num > 0)
		{
			btBuild.Visible = true;
		}
		else
		{
			btBuild.Visible = false;
		}
		if (!btBuild.Visible)
		{
			UpdateAttainable(attainableInfo, hzAttainable, hzNotAttainable, hasTools, hasInputs, hasSkills, hasResources, canProduceNow, process);
			return;
		}
		hzAttainable.Visible = false;
		hzNotAttainable.Visible = false;
	}

	public static void UpdateAttainable(Dictionary<ProcessType, AttainableInfo> attainableInfos, HorizontalList hzAttainable, HorizontalList hzNotAttainable, bool hasTools, bool hasInputs, bool hasSkills, bool hasResources, bool canProduceNow, ProcessType anyProcess)
	{
		KeyValuePair<ProcessType, AttainableInfo> keyValuePair = default(KeyValuePair<ProcessType, AttainableInfo>);
		if (attainableInfos != null)
		{
			keyValuePair = attainableInfos.FirstOrDefault((KeyValuePair<ProcessType, AttainableInfo> a) => a.Value.IsProducable && (anyProcess == null || a.Key == anyProcess));
			if (keyValuePair.Key == null)
			{
				keyValuePair = attainableInfos.First();
			}
		}
		ProcessType processType = null;
		AttainableInfo producableProcessInfo = null;
		if (keyValuePair.Key != null)
		{
			processType = keyValuePair.Key;
			producableProcessInfo = keyValuePair.Value;
		}
		if (processType != null)
		{
			hzAttainable.Visible = true;
			hzNotAttainable.Visible = false;
			PopulateAttainable(processType, producableProcessInfo, hzAttainable, hasTools, hasInputs, hasSkills, hasResources, anyProcess, attainableInfos.Count);
			return;
		}
		hzAttainable.Visible = false;
		hzNotAttainable.Visible = true;
		AttainableInfo info = null;
		int noOfProcesses = 0;
		if (attainableInfos != null)
		{
			noOfProcesses = attainableInfos.Count;
			KeyValuePair<ProcessType, AttainableInfo> keyValuePair2 = attainableInfos.FirstOrDefault((KeyValuePair<ProcessType, AttainableInfo> a) => true);
			if (keyValuePair2.Value != null)
			{
				info = keyValuePair2.Value;
			}
		}
		PopulateNotAttainableIcons(hzNotAttainable, info, noOfProcesses);
	}

	private static void PopulateAttainable(ProcessType producableProcess, AttainableInfo producableProcessInfo, HorizontalList list, bool hasTools, bool hasInputs, bool hasSkills, bool hasResources, ProcessType processType, int totalProcesses)
	{
		bool flag = false;
		if (processType != null && processType == producableProcess)
		{
			flag = true;
		}
		list.BeginAddingEntries();
		string text = "";
		Color attainableColor = GameData.Instance.GUIConstants.AttainableColor;
		if (producableProcess.IsSalvageProcess)
		{
			UIComponent uIComponent = AddOrGetIcon(IconKeys.Salvage, list, "lcd_icon_recycleArrows", attainableColor);
			text = "Attainable from salvaging items or structures";
			uIComponent.ToolTip = text;
			list.TryRemoveEntry(IconKeys.NoProcess);
			list.TryRemoveEntry(IconKeys.NoSkill);
			list.TryRemoveEntry(IconKeys.NoResource);
			list.TryRemoveEntry(IconKeys.NoInput);
			list.TryRemoveEntry(IconKeys.NoTool);
			list.TryRemoveEntry(IconKeys.NoPolicy);
			list.EndAddingEntries();
			return;
		}
		list.TryRemoveEntry(IconKeys.Salvage);
		if (!flag)
		{
			list.TryRemoveEntry(IconKeys.NoProcess);
			list.TryRemoveEntry(IconKeys.NoSkill);
			list.TryRemoveEntry(IconKeys.NoResource);
			list.TryRemoveEntry(IconKeys.NoInput);
			list.TryRemoveEntry(IconKeys.NoTool);
			list.TryRemoveEntry(IconKeys.NoPolicy);
			list.TryRemoveEntry(IconKeys.Salvage);
			list.EndAddingEntries();
			return;
		}
		if (!hasInputs)
		{
			UIComponent uIComponent2 = AddOrGetIcon(IconKeys.NoInput, list, "lcd_icon_stockpile", attainableColor);
			text = "Attainable, but inputs are needed. Examine the tooltip to determine what is missing.";
			uIComponent2.ToolTip = text;
		}
		else
		{
			list.TryRemoveEntry(IconKeys.NoInput);
		}
		if (!hasTools)
		{
			UIComponent uIComponent3 = AddOrGetIcon(IconKeys.NoTool, list, "lcd_icon_tool", attainableColor);
			text = "Attainable, but some tools are needed. Examine the tooltip to determine what is missing.";
			uIComponent3.ToolTip = text;
		}
		else
		{
			list.TryRemoveEntry(IconKeys.NoTool);
		}
		list.EndAddingEntries();
	}

	public static UIComponent AddOrGetIcon(IconKeys iconKey, HorizontalList list, string sprite, Color color)
	{
		if (!list.TryGetEntry(iconKey, out var entry))
		{
			entry = new Icon(list.guiManager);
			Icon obj = entry as Icon;
			list.AddEntry(iconKey, entry);
			Rectangle sourceRectangle = list.guiManager.GUISpriteSheet.GetSourceRectangle(sprite);
			obj.SetSkinLocation(SkinState.Normal, sourceRectangle, color, color);
			obj.ResizeControlToFitImage();
		}
		return entry;
	}

	public static UIComponent AddOrGetIcon(TierOrAreaType areaTier, HorizontalList list, Color color)
	{
		if (!list.TryGetEntry(IconKeys.NoPolicy, out var entry))
		{
			entry = new Icon(list.guiManager);
			Icon obj = entry as Icon;
			list.AddEntry(IconKeys.NoPolicy, entry);
			Rectangle sourceRectangle = list.guiManager.GUISpriteSheet.GetSourceRectangle(areaTier.Icon);
			obj.SetSkinLocation(SkinState.Normal, sourceRectangle, color, color);
			obj.ResizeControlToFitImage();
		}
		return entry;
	}

	private static void PopulateNotAttainableIcons(HorizontalList list, AttainableInfo info, int noOfProcesses)
	{
		list.BeginAddingEntries();
		string text = "";
		string text2 = "";
		Color unattainableColor = GameData.Instance.GUIConstants.UnattainableColor;
		if (noOfProcesses > 1)
		{
			UIComponent uIComponent = AddOrGetIcon(IconKeys.MultipleProcesses, list, "lcd_icon_asterisk", unattainableColor);
			uIComponent.ToolTip = "There is more than one way of producing this item, but none of them are attainable.";
			list.TryRemoveEntry(IconKeys.NoProcess);
			list.TryRemoveEntry(IconKeys.NoSkill);
			list.TryRemoveEntry(IconKeys.NoResource);
			list.TryRemoveEntry(IconKeys.NoInput);
			list.TryRemoveEntry(IconKeys.NoTool);
			list.EndAddingEntries();
			return;
		}
		list.TryRemoveEntry(IconKeys.MultipleProcesses);
		if (info == null)
		{
			UIComponent uIComponent = AddOrGetIcon(IconKeys.NoProcess, list, "lcd_icon_noEntry", unattainableColor);
			text = ((!The.InGameUI.InventorySettings.IncludeSalvageProcesses) ? "Not attainable. We have no way of producing this (HOWEVER: There may/may not be salvage options available!)" : "Not attainable. We have no way of producing this.");
			uIComponent.ToolTip = text;
			list.TryRemoveEntry(IconKeys.NoSkill);
			list.TryRemoveEntry(IconKeys.NoResource);
			list.TryRemoveEntry(IconKeys.NoInput);
			list.TryRemoveEntry(IconKeys.NoTool);
			list.EndAddingEntries();
			return;
		}
		list.TryRemoveEntry(IconKeys.NoProcess);
		if (info.UnavailableSkill != null)
		{
			string sprite = "lcd_icon_person";
			UIComponent uIComponent = AddOrGetIcon(IconKeys.NoSkill, list, sprite, unattainableColor);
			text = "Not attainable. No one has the needed skill: " + info.UnavailableSkill.Name;
			uIComponent.ToolTip = text;
		}
		else
		{
			list.TryRemoveEntry(IconKeys.NoSkill);
		}
		if (info.UnavailableResource != null)
		{
			UIComponent uIComponent = AddOrGetIcon(IconKeys.NoResource, list, "lcd_icon_gather", unattainableColor);
			text = "Not attainable. The following resource is needed, but has not been discovered (exploration may help): " + info.UnavailableResource.Name;
			uIComponent.ToolTip = text;
		}
		else
		{
			list.TryRemoveEntry(IconKeys.NoResource);
		}
		if (info.UnavailablePolicy != null)
		{
			UIComponent uIComponent = AddOrGetIcon(info.UnavailablePolicy, list, unattainableColor);
			text = "Not attainable. " + info.UnavailablePolicy.GetNotAvailableTooltip();
			uIComponent.ToolTip = text;
		}
		else
		{
			list.TryRemoveEntry(IconKeys.NoPolicy);
		}
		if (info.UnavailableSpecialSite != null)
		{
			UIComponent uIComponent = AddOrGetIcon(IconKeys.NoSpecialEntity, list, "lcd_icon_star", unattainableColor);
			text = "Not attainable. The following special site is needed, but has not been discovered (exploration may help): " + info.UnavailableSpecialSite.Name;
			uIComponent.ToolTip = text;
		}
		else
		{
			list.TryRemoveEntry(IconKeys.NoSpecialEntity);
		}
		if (info.UnavailableInputs != null)
		{
			UIComponent uIComponent = AddOrGetIcon(IconKeys.NoInput, list, "lcd_icon_stockpile", unattainableColor);
			text = "Not attainable. The following inputs (materials/ingredients) are needed, but are also not attainable: \n";
			text2 = "";
			foreach (EntityType unavailableInput in info.UnavailableInputs)
			{
				text = text + text2 + unavailableInput.PluralName;
				text2 = " \n";
			}
			uIComponent.ToolTip = text;
		}
		else
		{
			list.TryRemoveEntry(IconKeys.NoInput);
		}
		if (info.UnavailableTools != null)
		{
			UIComponent uIComponent = AddOrGetIcon(IconKeys.NoTool, list, "lcd_icon_tool", unattainableColor);
			int num = 0;
			text = "Not attainable. One of these tools is needed, but they are not attainable either: \n";
			string text3 = "";
			foreach (EntityType unavailableTool in info.UnavailableTools)
			{
				if (num == 3)
				{
					text += "...";
					break;
				}
				text = text + text3 + unavailableTool.Name;
				text3 = ", ";
				num++;
			}
			uIComponent.ToolTip = text;
		}
		else
		{
			list.TryRemoveEntry(IconKeys.NoTool);
		}
		list.EndAddingEntries();
	}

	private void UpdateItemRowNormalProduction(EntityGroup owner, Dictionary<EntityType, InventoryPanel.Availability> allAvailableItems, Dictionary<ProcessType, AttainableInfo> attainableInfo, EntityType entityType, ProcessType processType, bool hasTools, bool hasInputs, bool hasSkills, bool hasResources, int productionLimit, int? outputBatchAmount, ProcessType.ProductionUI processProductionMethod)
	{
		bool flag = productionLimit > 0;
		if (btBuild != null)
		{
			btBuild.Visible = false;
		}
		if (owner == null)
		{
			return;
		}
		bool showWarning = false;
		if (processProductionMethod == ProcessType.ProductionUI.Build || processProductionMethod == ProcessType.ProductionUI.Slider)
		{
			ProductionOrder value;
			if (GameData.Instance.GUIConstants.EnableStandingOrders)
			{
				btStandingOrder.Visible = true;
				if (owner.ProductionOrders.Orders.TryGetValue(entityType, out value))
				{
					SetPadlockButtonState(value.AmountToKeepInStore.HasValue, btStandingOrder);
					if (btStandingOrder.IsChecked)
					{
						UpdateStandingOrderProduction(productionLimit, outputBatchAmount, flag, value, fillableBar, btStandingOrder);
					}
					else
					{
						UpdateDirectProduction(owner, productionLimit, outputBatchAmount, flag, value, ref showWarning, fillableBar, btStandingOrder);
					}
				}
				else
				{
					fillableBar.Visible = false;
					btStandingOrder.Visible = false;
				}
			}
			else if (owner.ProductionOrders.Orders.TryGetValue(entityType, out value))
			{
				UpdateDirectProduction(owner, productionLimit, outputBatchAmount, flag, value, ref showWarning, fillableBar, null);
			}
		}
		else
		{
			if (fillableBar != null)
			{
				fillableBar.Visible = false;
			}
			if (GameData.Instance.GUIConstants.EnableStandingOrders && btStandingOrder != null)
			{
				btStandingOrder.Visible = false;
			}
		}
		if (fillableBar == null || !fillableBar.Visible)
		{
			if (processProductionMethod == ProcessType.ProductionUI.Other && flag)
			{
				UpdateNonInventoryOrders(allAvailableItems, processType, hzAttainable, hzNotAttainable);
			}
			else
			{
				UpdateAttainable(attainableInfo, hzAttainable, hzNotAttainable, hasTools, hasInputs, hasSkills, hasResources, flag, processType);
			}
		}
		else
		{
			hzAttainable.Visible = false;
			hzNotAttainable.Visible = false;
		}
		if (icWarning != null)
		{
			if (showWarning)
			{
				icWarning.Visible = true;
			}
			else
			{
				icWarning.Visible = false;
			}
		}
	}

	private void UpdateStandingOrderProduction(int productionLimit, int? outputBatchAmount, bool canProduceNow, ProductionOrder stockTarget, FillableBar fillableBar, ImageButton btStandingOrder)
	{
		fillableBar.Visible = true;
		int num = stockTarget.AmountToKeepInStore ?? 0;
		fillableBar.ColorAllControls = GameData.Instance.GUIConstants.StandingOrderTint;
		btStandingOrder.NormalColor = GameData.Instance.GUIConstants.StandingOrderTint;
		bool flag = false;
		fillableBar.StepSize = 1;
		fillableBar.MaxSliderValueSymbol = "...";
		fillableBar.MaxSliderValueTooltip = "No limit";
		fillableBar.ShowMaxValueLabelAtEnd = false;
		if (fillableBar.MaxValue != GameData.Instance.GUIConstants.UnlimitedStandingOrderValue)
		{
			fillableBar.MaxValue = GameData.Instance.GUIConstants.UnlimitedStandingOrderValue;
			flag = true;
		}
		if (!isSliderBeingDragged && fillableBar.Value != num)
		{
			int value = ((num != -1) ? num : GameData.Instance.GUIConstants.UnlimitedStockpileValue);
			fillableBar.Value = value;
			flag = true;
		}
		if (flag)
		{
			fillableBar.UpdateSliderPosition();
		}
	}

	private void UpdateDirectProduction(EntityGroup owner, int productionLimit, int? outputBatchAmount, bool canProduceNow, ProductionOrder stockTarget, ref bool showWarning, FillableBar fillableBar, ImageButton btStandingOrder)
	{
		int num = stockTarget.ProductionJobsToComplete ?? 0;
		if (OrderedItemsRequireWarning(owner, num))
		{
			showWarning = true;
		}
		fillableBar.ColorAllControls = UIComponent.LCDTint;
		if (btStandingOrder != null)
		{
			btStandingOrder.NormalColor = UIComponent.LCDTint;
		}
		fillableBar.MaxSliderValueSymbol = null;
		fillableBar.MaxSliderValueTooltip = null;
		fillableBar.ShowMaxValueLabelAtEnd = true;
		bool flag = false;
		((ProductionTargetEventArgs)fillableBar.EventArgs).OutputBatchAmount = outputBatchAmount;
		int num2 = productionLimit;
		int num3 = num;
		if (outputBatchAmount.HasValue)
		{
			num2 *= outputBatchAmount.Value;
			num3 *= outputBatchAmount.Value;
			fillableBar.StepSize = outputBatchAmount.Value;
		}
		else
		{
			fillableBar.StepSize = 1;
		}
		if (!isSliderBeingDragged)
		{
			if (!canProduceNow)
			{
				fillableBar.Visible = false;
			}
			else
			{
				fillableBar.Visible = true;
				if (fillableBar.MaxValue != num2)
				{
					fillableBar.MaxValue = num2;
					flag = true;
				}
				if (fillableBar.Value != num3)
				{
					fillableBar.Value = num3;
					flag = true;
				}
			}
		}
		if (flag)
		{
			fillableBar.UpdateSliderPosition();
		}
	}

	private void fillableBar_SliderMouseDown(object sender, EventArgs e)
	{
		isSliderBeingDragged = true;
	}

	public void ResetSliderBeingDragged()
	{
		isSliderBeingDragged = false;
	}

	private void UpdateNonInventoryOrders(Dictionary<EntityType, InventoryPanel.Availability> allAvailableItems, ProcessType processType, HorizontalList hzAttainable, HorizontalList hzNotAttainable)
	{
		hzAttainable.Visible = true;
		hzNotAttainable.Visible = false;
		hzAttainable.BeginAddingEntries();
		hzAttainable.TryRemoveEntry(IconKeys.NoProcess);
		hzAttainable.TryRemoveEntry(IconKeys.NoSkill);
		hzAttainable.TryRemoveEntry(IconKeys.NoResource);
		hzAttainable.TryRemoveEntry(IconKeys.NoPolicy);
		hzAttainable.TryRemoveEntry(IconKeys.NoInput);
		hzAttainable.TryRemoveEntry(IconKeys.NoTool);
		hzAttainable.TryRemoveEntry(IconKeys.Upgrade);
		hzAttainable.TryRemoveEntry(IconKeys.Pseudo);
		Color lCDNormal = UIComponent.LCDNormal;
		string text = "";
		if (processType.IsSalvageProcess)
		{
			UIComponent uIComponent = AddOrGetIcon(IconKeys.Salvage, hzAttainable, "lcd_icon_recycleArrows", lCDNormal);
			string text2 = ".";
			if (processType.InputsByType != null && processType.InputsByType.Count > 0)
			{
				EntityType key = processType.InputsByType.First().Key;
				if (allAvailableItems.TryGetValue(key, out var value) && value.NoOfAvailableItems > 0)
				{
					text2 = ", for example: " + key.Name + ".";
				}
			}
			text = "Attainable from salvaging items or structures" + text2;
			uIComponent.ToolTip = text;
		}
		else
		{
			hzAttainable.TryRemoveEntry(IconKeys.Salvage);
		}
		if (processType.IsGathering)
		{
			UIComponent uIComponent2 = AddOrGetIcon(IconKeys.NoResource, hzAttainable, "lcd_icon_gather", lCDNormal);
			text = "Attainable, but needs to be harvested from a resource with the GATHER action. Examine the tooltip to see the resource.";
			uIComponent2.ToolTip = text;
		}
		else
		{
			hzAttainable.TryRemoveEntry(IconKeys.NoResource);
		}
		if (processType.IsUpgrade)
		{
			UIComponent uIComponent3 = AddOrGetIcon(IconKeys.Upgrade, hzAttainable, "lcd_icon_uparrow", lCDNormal);
			text = "This is an upgrade and it is constructed with the UPGRADE action. Examine the tooltip to see the objects that can be upgraded.";
			uIComponent3.ToolTip = text;
		}
		else
		{
			hzAttainable.TryRemoveEntry(IconKeys.Upgrade);
		}
		if (processType.IsPseudoProcess)
		{
			UIComponent uIComponent4 = AddOrGetIcon(IconKeys.Pseudo, hzAttainable, "lcd_icon_pseudo", lCDNormal);
			text = "This is a pseudo process. Each pseudo process yields its product in a special way. Examine the tooltip to find out how to produce the item";
			uIComponent4.ToolTip = text;
		}
		else
		{
			hzAttainable.TryRemoveEntry(IconKeys.Pseudo);
		}
		if (processType.IsSpecialActionType)
		{
			UIComponent uIComponent5 = AddOrGetIcon(IconKeys.NoSpecialEntity, hzAttainable, "lcd_icon_special", lCDNormal);
			text = $"{processType.ActingOnType.Name} must first be selected, then the item can be built from the action menu";
			uIComponent5.ToolTip = text;
		}
		else
		{
			hzAttainable.TryRemoveEntry(IconKeys.NoSpecialEntity);
		}
		hzAttainable.EndAddingEntries();
	}

	public static bool OrderedItemsRequireWarning(EntityGroup entityGroup, int orderedJobs)
	{
		if (orderedJobs > GameData.Instance.GUIConstants.OrderedJobsWithSameOutputToTriggerWarning && TotalJobsRequireWarning(entityGroup))
		{
			return true;
		}
		return false;
	}

	public static bool TotalJobsRequireWarning(EntityGroup entityGroup)
	{
		if (entityGroup.ProductionOrders.TotalDirectOrders > EntityGroup.GetMaximumJobsBeforeWarning(entityGroup.Parent))
		{
			return true;
		}
		return false;
	}
}
