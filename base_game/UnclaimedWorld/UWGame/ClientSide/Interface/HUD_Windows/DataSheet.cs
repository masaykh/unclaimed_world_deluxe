using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InputEventSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using UWGame.Client.Interface;
using UWGame.ClientSide.Interface.Controls;
using UWGame.ClientSide.Interface.Inventory;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Policies;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Snapshots;
using WindowSystem;

namespace UWGame.ClientSide.Interface.HUD_Windows;

public abstract class DataSheet : HUDWindow
{
	public enum State
	{
		Collapsed,
		ExpandedProduction,
		ExpandedData
	}

	private class ToolScrollEventArgs : EventArgs
	{
		public int Index;
	}

	public enum InfoToShow
	{
		Production,
		Data
	}

	public UIComponent SpawningControl;

	protected ImageButton btPin;

	private ImageButton btShowProductionInfo;

	private ImageButton btShowGeneralInfo;

	private Image noProduction;

	protected TextArea summaryDescription;

	protected Label lblName;

	private State state;

	private double timePassed;

	protected const int rightMargin = 15;

	private const int collapsedWidth = 256;

	private const int collapsedHeight = 104;

	public const int ExpandedHeight = 320;

	private const int collapsedContentLeft = 42;

	private const int summaryDescripitonX = 34;

	private int expandedContentStart;

	private int maxExpandedContentHeightBeforeScrollAreasAppear = 180;

	protected const int itemHeight = 22;

	public bool CanExpand = true;

	protected Label lblExpandedHeading;

	protected Grid grdProductionOuter;

	protected Grid grdGeneralOuter;

	private Grid currentlyShownExpandedGrid;

	private UIComponent expandedContentViewPort;

	private ImageButton btScrollDown;

	private ImageButton btScrollUp;

	private TextArea taDescription;

	private const float policyIndex = 5f;

	private const float skillIndex = 10f;

	private const float actingOnIndex = 15f;

	private const float gatheredFromIndex = 18f;

	private const float inputIndex = 20f;

	private const float toolsIndex = 50f;

	private Grid grdInputs;

	private Label lblMadeFrom;

	private Icon icMadeFrom;

	private UIComponent madeFromHeader;

	private UIComponent skillHeader;

	private UIComponent actingOnHeader;

	private UIComponent gatheredFromHeader;

	private List<ToolGrid> allToolGrids = new List<ToolGrid>();

	private Dictionary<ToolAlternatives, ToolGrid> currentToolGrids = new Dictionary<ToolAlternatives, ToolGrid>();

	private bool showAllTools;

	private Label lblSkill;

	private Label lblGatheredFrom;

	protected UIComponent policyHeader;

	private Icon policyIcon;

	private UIComponent actingOn;

	private EntityTypeTooltipInstanceData instanceData;

	protected EntityGroupID? owner;

	protected bool useCurrentUIOwner;

	protected Image icon;

	public const int expandButtonY = 44;

	public const int expandButtonHeight = 18;

	private const int scrollAreaHeight = 15;

	public Color entityToolTipHeaderColor;

	protected int expandedPanelHeadingY = 110;

	protected static Color productionColor = "FFA83E".ColorFromHex();

	private bool mouseOverScrollDownArea;

	private bool mouseOverScrollUpArea;

	public static float scrollSpeedPerSecond = 200f;

	protected ProcessType processTypeToShowProductionFor;

	protected const int extraSideMargin = 5;

	private const string madeFromTooltip = "We need all of the below materials, in the given amounts";

	private const string gatheredTooltip = "This item is gathered from a resource";

	private static Color cyan = "0FF8FD".ColorFromHex();

	private UIComponent tooltipAnchor;

	private List<EntityType> toolsInGrid = new List<EntityType>();

	public State CurrentState => state;

	public ProcessType ProcessType => processTypeToShowProductionFor;

	protected bool ResolveOwner(out EntityGroup resolvedOwner)
	{
		if (useCurrentUIOwner)
		{
			resolvedOwner = LookUp<EntityGroup, EntityGroupID>.FindByID(The.InGameUI.UIOwner);
		}
		else
		{
			resolvedOwner = LookUp<EntityGroup, EntityGroupID>.FindByID(owner);
		}
		return resolvedOwner != null;
	}

	public DataSheet()
		: base(256, 104, hasSurface: true, hasCloseButton: false, isMovable: true, "HUD_windowInfo_base", hideWhenMouseExits: false, Level.EntityTypeInfo)
	{
		DisplayWindow.SetResizableArea(ResizeAreas.Top, isResizable: true);
		DisplayWindow.SetResizableArea(ResizeAreas.Bottom, isResizable: true);
		DisplayWindow.MinHeight = 104;
		DisplayWindow.ResizableBorderSize = 6;
		DisplayWindow.Resize += DisplayWindow_Resize;
		DisplayWindow.ViewPort.MouseOut += ViewPort_MouseOut;
		DisplayWindow.DebugTag = "DataTypeTooltip";
		showAllTools = The.Sim.Controller.Options.ShowAllTools;
		UpdateWhileHidden = true;
		icon = new Image(gui);
		icon.Texture = gui.GUISpriteSheet.Texture;
		Add(icon);
		icon.X = 6;
		icon.Y = 6;
		lblName = new Label(gui);
		lblName.X = 42;
		lblName.Y = 6;
		Add(lblName);
		lblName.Width = DisplayWindow.Width - 42 - 6;
		lblName.Init(Label.LabelType.EntityTypeTooltipHeader);
		lblName.ID = UIComponent.DataControlID.Caption;
		summaryDescription = new TextArea(gui, ListBoxType.HUDAndLCD);
		Add(summaryDescription);
		summaryDescription.Init(Label.LabelType.HUDWindow);
		summaryDescription.X = 42;
		summaryDescription.Y = lblName.Bottom + 6;
		summaryDescription.Width = DisplayWindow.Width - 42 - 6;
		summaryDescription.Height = DisplayWindow.Height - 12 - 10;
		summaryDescription.HMargin = 0;
		summaryDescription.ZOrder = 1f;
		noProduction = new Image(gui);
		noProduction.Texture = gui.GUISpriteSheet.Texture;
		noProduction.SetSkinLocation(SkinState.Normal, gui.GUISpriteSheet.GetSourceRectangle("HUD_info_button_productionUnavailable"));
		noProduction.ResizeControlToFitImage();
		noProduction.X = 8;
		noProduction.Y = 44;
		noProduction.ToolTip = "No production info available for this";
		btShowProductionInfo = new ImageButton(gui);
		Add(btShowProductionInfo);
		btShowProductionInfo.Init(ImageButtonType.HUDShowProductionInfo);
		btShowProductionInfo.X = noProduction.X;
		btShowProductionInfo.Y = 44;
		btShowProductionInfo.Click += btShowProductionInfo_Click;
		btShowProductionInfo.ToolTip = "See production/process information about this";
		btShowProductionInfo.CheckedMode = CheckedModes.CanBeChecked;
		btShowGeneralInfo = new ImageButton(gui);
		Add(btShowGeneralInfo);
		btShowGeneralInfo.Init(ImageButtonType.HUDShowGeneralInfo);
		btShowGeneralInfo.X = noProduction.X;
		btShowGeneralInfo.Y = 68;
		btShowGeneralInfo.Click += btShowGeneralInfo_Click;
		btShowGeneralInfo.ToolTip = "See general data/info about this";
		btShowGeneralInfo.CheckedMode = CheckedModes.CanBeChecked;
		btPin = new ImageButton(gui);
		Add(btPin);
		btPin.Init(ImageButtonType.HUDPin);
		btPin.CheckedMode = CheckedModes.SwitchCheckedStateOnClick;
		btPin.X = DisplayWindow.Width - 6 - btPin.Width;
		btPin.Y = 58;
		btPin.Click += btPin_Click;
		UpdatePinButton();
		lblExpandedHeading = new Label(gui);
		Add(lblExpandedHeading);
		lblExpandedHeading.Init(Label.LabelType.EntityTypeTooltipSubHeading);
		lblExpandedHeading.X = SideMarginOutsideGrid();
		lblExpandedHeading.Y = expandedPanelHeadingY;
		btClose = new ImageButton(gui);
		Add(btClose);
		btClose.Init(ImageButtonType.HUDClose);
		btClose.X = DisplayWindow.Width - 6 - btClose.Width;
		btClose.Y = 5;
		btClose.Click += btClose_Click;
		lblName.MaxWidth = btClose.X - 25 - lblName.X;
		summaryDescription.MaxWidth = DisplayWindow.Width - 2 * summaryDescription.X;
		expandedContentStart = 136;
		expandedContentViewPort = new UIComponent(gui);
		expandedContentViewPort.Y = expandedContentStart;
		expandedContentViewPort.Width = DisplayWindow.Width;
		expandedContentViewPort.Height = maxExpandedContentHeightBeforeScrollAreasAppear;
		Add(expandedContentViewPort);
		btScrollDown = new ImageButton(gui);
		btScrollDown.Init(ImageButtonType.HUDInfoScrollDown);
		btScrollDown.X = (DisplayWindow.Width - btScrollDown.Width) / 2;
		btScrollDown.Y = expandedContentViewPort.Bottom + 6;
		btScrollDown.Height = 15;
		btScrollDown.MouseOver += btScrollDown_MouseOver;
		btScrollDown.MouseOut += btScrollDown_MouseOut;
		btScrollUp = new ImageButton(gui);
		btScrollUp.Init(ImageButtonType.HUDInfoScrollUp);
		btScrollUp.X = (DisplayWindow.Width - btScrollUp.Width) / 2;
		btScrollUp.Y = expandedContentStart - 18 + 5;
		btScrollUp.Height = 15;
		btScrollUp.MouseOver += btScrollUp_MouseOver;
		btScrollUp.MouseOut += btScrollUp_MouseOut;
		CreateProductionPanel();
		CreateGeneralPanel();
	}

	private void SetVerticalPositions()
	{
		AdaptContentToWindow();
	}

	private void DisplayWindow_Resize(UIComponent sender)
	{
		SetVerticalPositions();
	}

	private void btPin_Click(UIComponent sender, EventArgs e)
	{
		UpdatePinButton();
	}

	private void UpdatePinButton()
	{
		if (btPin.IsChecked)
		{
			btPin.ToolTip = "Unpin this window so it will be hidden as normal";
			The.InGameUI.PinnedDataTypeTooltips.Add(this);
			The.InGameUI.UnpinnedDataTypeTooltipsOutsideStack.Remove(this);
			SpawningControl = null;
			return;
		}
		btPin.ToolTip = "Pin this window so it stays open";
		The.InGameUI.PinnedDataTypeTooltips.Remove(this);
		if (DisplayWindow.IsVisibleAndActive)
		{
			The.InGameUI.UnpinnedDataTypeTooltipsOutsideStack.Add(this);
		}
	}

	private void btEncyclopedia_Click(UIComponent sender, EventArgs e)
	{
	}

	private void btClose_Click(UIComponent sender, EventArgs e)
	{
		btPin.IsChecked = false;
		Hide();
	}

	public virtual void OnSetProduction()
	{
	}

	public static int GetAnchorPointYOffset()
	{
		return -53;
	}

	private void btScrollUp_MouseOut(UIComponent sender, MouseEventArgs args)
	{
		mouseOverScrollUpArea = false;
	}

	private void btScrollUp_MouseOver(UIComponent sender, MouseEventArgs args)
	{
		mouseOverScrollUpArea = true;
	}

	private void btScrollDown_MouseOut(UIComponent sender, MouseEventArgs args)
	{
		mouseOverScrollDownArea = false;
	}

	private void btScrollDown_MouseOver(UIComponent sender, MouseEventArgs args)
	{
		mouseOverScrollDownArea = true;
	}

	private void UpdateScrolling(GameTime elapsed)
	{
		if (mouseOverScrollDownArea)
		{
			ScrollDownGrid(elapsed, currentlyShownExpandedGrid);
		}
		else if (mouseOverScrollUpArea)
		{
			ScrollUpGrid(elapsed, currentlyShownExpandedGrid);
		}
		else if (UWGame.Mods.UnhiddenMod.Enabled)
		{
			UpdateWheelScrolling();
		}
	}

	/// <summary>
	/// UNHIDDEN MOD: mouse wheel scrolls the expanded grid, in addition to the studio's
	/// hover-over-the-arrow scrolling.
	///
	/// Polled rather than driven off InputData.MouseWheelMove. That is an event GUIManager
	/// already subscribes to, and adding a second subscriber from a window that comes and goes
	/// means owning an unsubscribe; polling a wheel value here is self-contained and cannot
	/// leak. The delta baseline is per-instance for the same reason - the original patch kept it
	/// in a static that Client.HandleInput also wrote to, which coupled two unrelated classes.
	///
	/// Only runs in the else branch, so a wheel turn cannot fight an arrow hover.
	///
	/// ONLY THE SHEET UNDER THE POINTER SCROLLS. Polling is per-instance, so without this every
	/// open data sheet saw the same wheel movement and they all scrolled together - reported by
	/// Kastuk, and the obvious consequence of polling that nobody notices with one window open.
	/// GUIManager already tracks which component a wheel event belongs to (mouseWheelTarget, from
	/// its own hit test), and MouseWheelReceiver() hands it over, so the sheet can ask rather than
	/// guess. Comparing parent windows rather than the component itself is what makes it work
	/// wherever the pointer is inside the sheet - over a row, a header or the empty space below.
	///
	/// The baseline is updated whether or not this sheet is the target. It has to be: a baseline
	/// frozen while the pointer is elsewhere would store up every notch turned over other windows
	/// and spend them the moment the pointer came back.
	/// </summary>
	private void UpdateWheelScrolling()
	{
		int wheel = Mouse.GetState().ScrollWheelValue;
		if (!wheelBaselineValid)
		{
			wheelBaseline = wheel;
			wheelBaselineValid = true;
			return;
		}
		int delta = wheel - wheelBaseline;
		wheelBaseline = wheel;
		if (delta == 0 || currentlyShownExpandedGrid == null || !PointerIsOverThisSheet())
		{
			return;
		}

		// A fixed step per notch, rather than the time-scaled speed the hover path uses - a
		// wheel notch is a discrete event, not a duration.
		if (delta < 0)
		{
			currentlyShownExpandedGrid.Y -= wheelScrollStep;
			if (IsAtBottom(currentlyShownExpandedGrid))
			{
				currentlyShownExpandedGrid.Y = expandedContentViewPort.Height - currentlyShownExpandedGrid.Height;
				HideScrollDownArea();
			}
			ShowScrollUpArea();
		}
		else
		{
			currentlyShownExpandedGrid.Y += wheelScrollStep;
			if (IsAtTop(currentlyShownExpandedGrid))
			{
				currentlyShownExpandedGrid.Y = 0;
				HideScrollUpArea();
			}
			ShowScrollDownArea();
		}
	}

	/// <summary>
	/// Whether the mouse wheel currently belongs to this data sheet, according to the GUI's own
	/// hit test rather than a geometry comparison of our own.
	/// </summary>
	private bool PointerIsOverThisSheet()
	{
		if (gui == null || DisplayWindow == null)
		{
			return false;
		}
		UIComponent wheelTarget = gui.MouseWheelReceiver();
		return wheelTarget != null && wheelTarget.GetParentWindow() == DisplayWindow;
	}

	private const int wheelScrollStep = 30;

	private int wheelBaseline;

	private bool wheelBaselineValid;

	private void AdaptContentToWindow()
	{
		if (currentlyShownExpandedGrid == null)
		{
			return;
		}
		int num = DisplayWindow.Height - expandedContentViewPort.Y - 24;
		expandedContentViewPort.Height = num;
		if (currentlyShownExpandedGrid.Height > num)
		{
			if (!mouseOverScrollDownArea && !mouseOverScrollUpArea && currentlyShownExpandedGrid.Y < 0 && currentlyShownExpandedGrid.Bottom < expandedContentViewPort.Height)
			{
				int d = expandedContentViewPort.Height - currentlyShownExpandedGrid.Height;
				d = Common.ClampTop(d, 0);
				currentlyShownExpandedGrid.Y = d;
			}
			if (!IsAtBottom(currentlyShownExpandedGrid))
			{
				ShowScrollDownArea();
				btScrollDown.Y = expandedContentViewPort.Bottom + 6;
			}
			else
			{
				HideScrollDownArea();
			}
			if (!IsAtTop(currentlyShownExpandedGrid))
			{
				ShowScrollUpArea();
			}
			else
			{
				HideScrollUpArea();
			}
		}
		else
		{
			HideScrollUpArea();
			HideScrollDownArea();
		}
	}

	private void ShowScrollUpArea()
	{
		if (!DisplayWindow.Contains(btScrollUp))
		{
			Add(btScrollUp);
		}
	}

	private void ShowScrollDownArea()
	{
		if (!DisplayWindow.Contains(btScrollDown))
		{
			Add(btScrollDown);
		}
	}

	private void HideScrollUpArea()
	{
		Remove(btScrollUp);
		mouseOverScrollUpArea = false;
	}

	private void HideScrollDownArea()
	{
		Remove(btScrollDown);
		mouseOverScrollDownArea = false;
	}

	private void ScrollUpGrid(GameTime elapsed, Grid grid)
	{
		grid.Y += (int)(elapsed.ElapsedGameTime.TotalSeconds * (double)scrollSpeedPerSecond);
		if (IsAtTop(grid))
		{
			grid.Y = 0;
			HideScrollUpArea();
		}
		ShowScrollDownArea();
	}

	private bool IsAtTop(Grid grid)
	{
		return grid.Y >= 0;
	}

	private bool IsAtBottom(Grid grid)
	{
		return grid.Bottom <= expandedContentViewPort.Height;
	}

	private void ScrollDownGrid(GameTime elapsed, Grid grid)
	{
		grid.Y -= (int)(elapsed.ElapsedGameTime.TotalSeconds * (double)scrollSpeedPerSecond);
		if (IsAtBottom(grid))
		{
			grid.Y = expandedContentViewPort.Height - grid.Height;
			HideScrollDownArea();
		}
		ShowScrollUpArea();
	}

	private void CreateProductionPanel()
	{
		grdProductionOuter = HUDWindow.CreateOuterGridForCollapsableLists(The.InGameUI.gui, DisplayWindow.ViewPort, addToSurface: false);
		grdProductionOuter.ScrollBarEnabled = false;
		grdProductionOuter.CanGrowInHeight = true;
		grdProductionOuter.DebugTag = "outerProdGrid";
		grdProductionOuter.BeginAddingEntries();
		madeFromHeader = AddSubHeader(grdProductionOuter, "MADE FROM:", "HUD_icon_stockpile", 1, 5, out lblMadeFrom, out icMadeFrom, addToGrid: true, 0, 9);
		madeFromHeader.OrderByTag1 = 20f;
		grdInputs = CreateFixedItemHeightGrid();
		grdInputs.OrderByTag1 = 21f;
		grdProductionOuter.AddEntry(grdInputs, grdInputs);
		policyHeader = AddSubHeader(grdProductionOuter, "POLICY:", "lcd_icon_section", -2, 0, out var lbl, out var icon, addToGrid: true, 0, 18);
		policyHeader.OrderByTag1 = 5f;
		lbl.ToolTip = "Requires a policy to be enacted";
		policyIcon = new Icon(gui);
		policyHeader.Add(policyIcon);
		skillHeader = AddSubHeader(grdProductionOuter, "REQUIRED SKILL:", "HUD_icon_person", -7, -4, out lbl, out icon, addToGrid: true, 0, 18);
		skillHeader.OrderByTag1 = 10f;
		lbl.ToolTip = "Requires a character with sufficient level in this skill (more than 0.1)";
		lblSkill = new Label(gui);
		lblSkill.Init(Label.LabelType.EntityTypeTooltip);
		lblSkill.X = 48;
		grdProductionOuter.AddEntry(lblSkill, lblSkill);
		lblSkill.OrderByTag1 = 11f;
		actingOnHeader = AddSubHeader(grdProductionOuter, "SPECIAL LOCATION:", "HUD_icon_star", -7, 1, out lbl, out icon, addToGrid: false, 6, 25);
		actingOnHeader.OrderByTag1 = 15f;
		lbl.ToolTip = "Can only be built at a special location, by using its action menu";
		gatheredFromHeader = AddSubHeader(grdProductionOuter, "GATHERED FROM:", "HUD_icon_gather", -1, 11, out lbl, out icon, addToGrid: false, 6, 25);
		gatheredFromHeader.OrderByTag1 = 18f;
		lbl.ToolTip = "Needs to be gathered from a resource";
		lblGatheredFrom = new Label(gui);
		lblGatheredFrom.Init(Label.LabelType.EntityTypeTooltip);
		lblGatheredFrom.X = lblSkill.X;
		lblGatheredFrom.OrderByTag1 = 19f;
		for (int i = 0; i < 3; i++)
		{
			CreateToolGrid(i);
		}
		CreateProductionPanelContents();
		grdProductionOuter.EndAddingEntries();
	}

	protected int SideMarginOutsideGrid()
	{
		return 11;
	}

	protected Grid CreateFixedItemHeightGrid(int? itemHeight = null)
	{
		return new Grid(gui, ListBoxType.HUDAndLCD, Label.LabelType.EntityTypeTooltip)
		{
			IsOuterGrid = false,
			X = 6,
			FixedItemHeights = true,
			Width = grdProductionOuter.Width,
			ScrollBarEnabled = false,
			ItemHeight = (itemHeight ?? 22),
			CanGrowInHeight = true,
			Font = GUIManager.LCDandHUDBodyFontPath,
			Height = 160
		};
	}

	protected virtual void CreateProductionPanelContents()
	{
	}

	protected UIComponent AddSubHeader(Grid grid, string text, out Label lbl, bool addToGrid = true, Color? labelColor = null)
	{
		Icon icon;
		return AddSubHeader(grid, text, null, 0, 0, out lbl, out icon, addToGrid, 6, null, labelColor);
	}

	protected UIComponent AddSubHeader(Grid grid, string text, string sprite, int iconXPos, int iconYPos, out Label lbl, out Icon icon, bool addToGrid = true, int topPadding = 6, int? itemHeight = null, Color? labelColor = null)
	{
		icon = null;
		int num = 11;
		lbl = new Label(gui);
		lbl.Init(Label.LabelType.EntityTypeTooltip);
		lbl.Text = text;
		lbl.FitToText();
		lbl.X = num;
		UIComponent uIComponent = new UIComponent(gui);
		uIComponent.Height = itemHeight ?? (lbl.TextHeight + topPadding);
		uIComponent.Add(lbl);
		lbl.Y = topPadding;
		if (labelColor.HasValue)
		{
			lbl.NormalColor = labelColor.Value;
		}
		if (sprite != null)
		{
			icon = new Icon(gui);
			icon.SetSkinLocation(SkinState.Normal, gui.GUISpriteSheet.GetSourceRectangle(sprite));
			icon.ScaleImageToSizeOfControl = false;
			icon.ResizeControlToFitImage();
			if (itemHeight.HasValue && icon.Height > itemHeight.Value)
			{
				icon.Height = itemHeight.Value;
			}
			uIComponent.Add(icon);
			icon.Y = iconYPos;
			icon.X = num + iconXPos;
			lbl.X = num + 14;
		}
		if (addToGrid)
		{
			grid.AddEntry(uIComponent, uIComponent);
		}
		return uIComponent;
	}

	private void CreateToolGrid(int index)
	{
		UIComponent uIComponent;
		Label lbl;
		Icon icon;
		switch (index)
		{
		case 0:
			uIComponent = AddSubHeader(grdProductionOuter, GameData.Instance.GUIConstants.FirstToolOption, "HUD_icon_tool", 0, 8, out lbl, out icon, addToGrid: false);
			break;
		case 1:
			uIComponent = AddSubHeader(grdProductionOuter, GameData.Instance.GUIConstants.SecondToolOption, "HUD_icon_tool", 0, 8, out lbl, out icon, addToGrid: false);
			break;
		case 2:
			uIComponent = AddSubHeader(grdProductionOuter, GameData.Instance.GUIConstants.ThirdToolOption, "HUD_icon_tool", 0, 8, out lbl, out icon, addToGrid: false);
			break;
		default:
			uIComponent = AddSubHeader(grdProductionOuter, "More than 3 tool options." + (index + 1), out lbl, addToGrid: false);
			icon = null;
			break;
		}
		if (icon != null)
		{
			icon.Color = cyan;
		}
		lbl.NormalColor = cyan;
		lbl.ToolTip = "Needs one of the tools from the group below. Expand the list to see more tool options. NOTE: There can be even more options than the ones shown";
		ImageButton imageButton = new ImageButton(The.InGameUI.gui);
		uIComponent.Add(imageButton);
		imageButton.Init(ImageButtonType.HUDExpandCollapseTinted);
		imageButton.ID = UIComponent.DataControlID.Expand;
		imageButton.X = 220;
		imageButton.Y = lbl.Y;
		imageButton.Click += btExpandTools_Click;
		imageButton.ToolTip = "Expand the list of tools";
		imageButton.CheckedMode = CheckedModes.SwitchCheckedStateOnClick;
		imageButton.NormalColor = cyan;
		imageButton.DebugTag = "btExpand";
		ImageButton imageButton2 = null;
		ImageButton imageButton3 = null;
		if (showAllTools)
		{
			imageButton2 = new ImageButton(The.InGameUI.gui);
			uIComponent.Add(imageButton2);
			imageButton2.Init(ImageButtonType.HUDArrowUp);
			imageButton2.ID = UIComponent.DataControlID.Up;
			imageButton2.X = 116;
			imageButton2.Click += btScrollToolsUp_Click;
			imageButton2.ToolTip = "Scroll up in the list of tools";
			imageButton3 = new ImageButton(The.InGameUI.gui);
			uIComponent.Add(imageButton3);
			imageButton3.Init(ImageButtonType.HUDArrowDown);
			imageButton3.ID = UIComponent.DataControlID.Down;
			imageButton3.X = imageButton2.Right + 12;
			imageButton3.Click += btDown_Click;
			imageButton3.ToolTip = "Scroll down in the list of tools";
		}
		Grid grid = new Grid(gui, ListBoxType.HUDAndLCD, Label.LabelType.EntityTypeTooltip);
		grid.IsOuterGrid = false;
		grid.DebugTag = "grdTools";
		grid.X = 6;
		grid.FixedItemHeights = true;
		grid.Width = grdProductionOuter.Width;
		grid.ScrollBarEnabled = false;
		grid.ItemHeight = 22;
		grid.CanGrowInHeight = true;
		grid.Font = GUIManager.LCDandHUDBodyFontPath;
		grid.Height = 160;
		if (showAllTools)
		{
			ToolScrollEventArgs eventArgs = (ToolScrollEventArgs)(imageButton3.EventArgs = new ToolScrollEventArgs
			{
				Index = index
			});
			imageButton2.EventArgs = eventArgs;
		}
		allToolGrids.Add(new ToolGrid
		{
			Grid = grid,
			ScrollPosition = 0,
			btScrollDown = imageButton3,
			btScrollUp = imageButton2,
			HeaderRow = uIComponent
		});
	}

	private void btExpandTools_Click(UIComponent sender, EventArgs e)
	{
		if (((ImageButton)sender).IsChecked)
		{
			sender.ToolTip = "Collapse the list of tools";
		}
		else
		{
			sender.ToolTip = "Expand the list of tools";
		}
		PopulateProductionDataRefresh();
	}

	private void btDown_Click(UIComponent sender, EventArgs e)
	{
		ScrollTools(e, 1);
	}

	private void btScrollToolsUp_Click(UIComponent sender, EventArgs e)
	{
		ScrollTools(e, -1);
	}

	private void ScrollTools(EventArgs e, int increase)
	{
		ToolGrid toolGrid = allToolGrids[((ToolScrollEventArgs)e).Index];
		int scrollPosition = toolGrid.ScrollPosition + increase;
		toolGrid.ScrollPosition = scrollPosition;
	}

	protected virtual void CreateGeneralPanelContents()
	{
	}

	private void CreateGeneralPanel()
	{
		grdGeneralOuter = HUDWindow.CreateOuterGridForCollapsableLists(The.InGameUI.gui, DisplayWindow.ViewPort, addToSurface: false);
		grdGeneralOuter.ScrollBarEnabled = false;
		grdGeneralOuter.CanGrowInHeight = true;
		grdGeneralOuter.BeginAddingEntries();
		taDescription = new TextArea(gui, ListBoxType.HUDAndLCD);
		grdGeneralOuter.AddEntry(taDescription, taDescription);
		taDescription.Init(Label.LabelType.HUDWindow);
		taDescription.CanGrowInHeight = true;
		taDescription.ZOrder = 1f;
		taDescription.X = SideMarginOutsideGrid();
		taDescription.Width = 256 - 2 * taDescription.X;
		taDescription.OrderByTag1 = 0f;
		CreateGeneralPanelContents();
		grdGeneralOuter.EndAddingEntries();
	}

	private void btShowGeneralInfo_Click(UIComponent sender, EventArgs e)
	{
		if (state != State.ExpandedData)
		{
			Expand(InfoToShow.Data);
		}
	}

	private void btShowProductionInfo_Click(UIComponent sender, EventArgs e)
	{
		if (state != State.ExpandedProduction)
		{
			Expand(InfoToShow.Production);
		}
	}

	public void InitAndShow(UIComponent spawningControl, EntityGroupID? owner, bool useUIOwner, int screenPosX, int screenPosY)
	{
		useCurrentUIOwner = useUIOwner;
		if (!useUIOwner)
		{
			this.owner = owner;
		}
		else
		{
			this.owner = null;
		}
		SpawningControl = spawningControl;
		if (spawningControl is TextButton textButton)
		{
			textButton.IsChecked = true;
		}
		if (!The.InGameUI.EntityTypeTooltipsStack.Contains(this))
		{
			The.InGameUI.EntityTypeTooltipsStack.Add(this);
		}
		ShowInScreenSpace(screenPosX, screenPosY, modal: false);
	}

	public override void ShowInScreenSpace(int screenPosX, int screenPosY, bool modal)
	{
		base.ShowInScreenSpace(screenPosX, screenPosY, modal);
		DisplayWindow.BringToTop();
		timePassed = 0.0;
		The.InGameUI.InventorySettings.TrackTargetsChanged += InventorySettings_TrackTargetsChanged;
	}

	protected virtual void RefreshTrackTargets()
	{
	}

	private void InventorySettings_TrackTargetsChanged()
	{
		RefreshTrackTargets();
		if (state == State.ExpandedProduction)
		{
			PopulateProductionDataRefresh();
		}
	}

	private void ViewPort_MouseOut(UIComponent sender, MouseEventArgs args)
	{
		if (state == State.Collapsed && The.InGameUI.GetChildTooltip(this) == null)
		{
			Hide();
		}
	}

	protected void Fill()
	{
		processTypeToShowProductionFor = GetProcessToShow();
		PopulateCollapsedFields();
		UpdateToolGrids();
	}

	private void UpdateToolGrids()
	{
		grdProductionOuter.BeginAddingEntries();
		currentToolGrids.Clear();
		if (processTypeToShowProductionFor != null && processTypeToShowProductionFor.ProcessToolSet != null && processTypeToShowProductionFor.ProcessToolSet.Tools != null)
		{
			int num = 0;
			ToolAlternatives[] tools = processTypeToShowProductionFor.ProcessToolSet.Tools;
			foreach (ToolAlternatives key in tools)
			{
				ToolGrid value = allToolGrids[num];
				currentToolGrids.Add(key, value);
				num++;
			}
		}
		foreach (ToolGrid allToolGrid in allToolGrids)
		{
			allToolGrid.Grid.Clear();
			grdProductionOuter.RemoveEntry(allToolGrid.HeaderRow, allToolGrid.HeaderRow);
			grdProductionOuter.RemoveEntry(allToolGrid.Grid, allToolGrid.Grid);
		}
		if (currentToolGrids.Count > 0)
		{
			float num2 = 0f;
			foreach (KeyValuePair<ToolAlternatives, ToolGrid> currentToolGrid in currentToolGrids)
			{
				grdProductionOuter.AddEntry(currentToolGrid.Value.HeaderRow, currentToolGrid.Value.HeaderRow);
				currentToolGrid.Value.HeaderRow.OrderByTag1 = 50f + num2;
				num2 += 1f;
				grdProductionOuter.AddEntry(currentToolGrid.Value.Grid, currentToolGrid.Value.Grid);
				currentToolGrid.Value.Grid.OrderByTag1 = 50f + num2;
				num2 += 1f;
			}
		}
		grdProductionOuter.Sort(Grid.Sorting.Ascending, useFirstTag: true);
		grdProductionOuter.EndAddingEntries();
	}

	public override void Update(GameTime elapsed)
	{
		base.Update(elapsed);
		if (!DisplayWindow.Visible)
		{
			Tooltip.HandleUpdate(elapsed, ref timePassed, tooltipAnchor, this);
		}
		if (DisplayWindow.IsVisibleAndActive && currentlyShownExpandedGrid != null)
		{
			UpdateScrolling(elapsed);
		}
	}

	public override void Refresh()
	{
		base.Refresh();
		ProcessType processType = processTypeToShowProductionFor;
		processTypeToShowProductionFor = GetProcessToShow();
		if (processTypeToShowProductionFor != processType)
		{
			UpdateToolGrids();
		}
		RefreshCollapsedFields();
		if (state == State.ExpandedProduction)
		{
			PopulateProductionDataRefresh();
		}
		else if (state == State.ExpandedData)
		{
			PopulateGeneralDataRefresh();
		}
	}

	public bool MouseIsOverTooltipOrChildTooltips(Point mousePos, int callNo)
	{
		if (DisplayWindow.CheckCoordinates(mousePos.X, mousePos.Y))
		{
			return true;
		}
		return The.InGameUI.GetChildTooltip(this)?.MouseIsOverTooltipOrChildTooltips(mousePos, callNo + 1) ?? false;
	}

	public void StartCountdownToShow(UIComponent sender)
	{
		tooltipAnchor = sender;
		timePassed = 0.0;
	}

	public void UpdateProductionAndGeneralInfoButtons(InfoToShow infoToShow)
	{
		if (infoToShow == InfoToShow.Production && processTypeToShowProductionFor != null)
		{
			btShowProductionInfo.IsChecked = true;
			btShowGeneralInfo.IsChecked = false;
		}
		else
		{
			btShowGeneralInfo.IsChecked = true;
			btShowProductionInfo.IsChecked = false;
		}
	}

	public void Expand(InfoToShow infoToShow)
	{
		DisplayWindow.Height = 320;
		UpdateProductionAndGeneralInfoButtons(infoToShow);
		if (infoToShow == InfoToShow.Production)
		{
			btShowProductionInfo.IsChecked = true;
			btShowGeneralInfo.IsChecked = false;
			state = State.ExpandedProduction;
			expandedContentViewPort.Remove(grdGeneralOuter);
			expandedContentViewPort.Add(grdProductionOuter);
			currentlyShownExpandedGrid = grdProductionOuter;
			SetProductionHeading(lblExpandedHeading);
			lblExpandedHeading.FitToText();
			lblExpandedHeading.NormalColor = productionColor;
			grdProductionOuter.Y = 0;
			PopulateProductionDataRefresh();
		}
		else
		{
			state = State.ExpandedData;
			expandedContentViewPort.Remove(grdProductionOuter);
			expandedContentViewPort.Add(grdGeneralOuter);
			currentlyShownExpandedGrid = grdGeneralOuter;
			lblExpandedHeading.Text = "DATA";
			lblExpandedHeading.FitToText();
			PadHeader(lblExpandedHeading);
			lblExpandedHeading.NormalColor = Color.White;
			grdGeneralOuter.Y = 0;
			PopulateGeneralFields();
		}
		AdaptContentToWindow();
	}

	protected void PadHeader(Label lbl)
	{
		int textWidth = lbl.GetTextWidth("/");
		int count = (grdProductionOuter.Width - lbl.Right - 60) / textWidth;
		string text = new string('/', count);
		lbl.Text += text;
		lbl.FitToText();
	}

	protected abstract void SetProductionHeading(Label label);

	protected abstract string GetInputHeading();

	private void PopulateCollapsedFields()
	{
		btShowProductionInfo.IsChecked = false;
		btShowGeneralInfo.IsChecked = false;
		btShowProductionInfo.Visible = true;
		Remove(noProduction);
		PopulateCollapsedFieldsContents();
	}

	protected virtual void RefreshCollapsedFields()
	{
	}

	protected virtual void PopulateCollapsedFieldsContents()
	{
	}

	private void PopulateProductionDataRefresh()
	{
		ResolveOwner(out var resolvedOwner);
		grdProductionOuter.BeginAddingEntries();
		PopulatePolicy();
		PopulateGatheredAt();
		PopulateInputList(resolvedOwner);
		PopulateSkill();
		PopulateActingOn();
		PopulateToolsList(resolvedOwner);
		PopulateProductionContentRefresh();
		grdProductionOuter.Sort(Grid.Sorting.Ascending, useFirstTag: true);
		CollapseTopRowPadding();
		grdProductionOuter.EndAddingEntries();
		AdaptContentToWindow();
	}

	private void CollapseTopRowPadding()
	{
	}

	private void PopulateGeneralDataRefresh()
	{
		grdGeneralOuter.BeginAddingEntries();
		PopulateGeneralDataContentRefresh();
		grdGeneralOuter.Sort(Grid.Sorting.Ascending, useFirstTag: true);
		grdGeneralOuter.EndAddingEntries();
		AdaptContentToWindow();
	}

	private void PopulateGeneralFields()
	{
		grdGeneralOuter.BeginAddingEntries();
		PopulateDescription();
		PopulateGeneralDataContent();
		grdGeneralOuter.Sort(Grid.Sorting.Ascending, useFirstTag: true);
		grdGeneralOuter.EndAddingEntries();
	}

	protected virtual string GetDescription()
	{
		return "";
	}

	private void PopulateDescription()
	{
		grdGeneralOuter.TryRemoveEntry(taDescription);
		taDescription.Text = GetDescription();
		if (!string.IsNullOrEmpty(taDescription.Text))
		{
			grdGeneralOuter.AddEntry(taDescription, taDescription);
		}
	}

	protected abstract ProcessType GetProcessToShow();

	private void PopulateSkill()
	{
		grdProductionOuter.TryRemoveEntry(skillHeader);
		grdProductionOuter.TryRemoveEntry(lblSkill);
		if (processTypeToShowProductionFor != null && processTypeToShowProductionFor.RequiredSkillType != null)
		{
			SkillType requiredSkillType = processTypeToShowProductionFor.RequiredSkillType;
			lblSkill.Text = requiredSkillType.Name;
			if (!The.InGameUI.GetExpedition().HasSkill(requiredSkillType))
			{
				lblSkill.NormalColor = DataTypeButton.notInStockColorLight;
				lblSkill.ToolTip = "No one has sufficent level in this skill (more than 0.1)";
			}
			else
			{
				lblSkill.NormalColor = lblSkill.GetNormalColorForType();
				lblSkill.ToolTip = "At least one character has the sufficent level in this skill (more than 0.1)";
			}
			grdProductionOuter.AddEntry(skillHeader, skillHeader);
			grdProductionOuter.AddEntry(lblSkill, lblSkill);
		}
		else
		{
			lblSkill.Text = "";
		}
	}

	protected virtual void PopulatePolicy()
	{
		grdProductionOuter.TryRemoveEntry(policyHeader);
		if (processTypeToShowProductionFor != null)
		{
			TierOrAreaType tierArea = processTypeToShowProductionFor.GetTierArea();
			ShowPolicyArea(tierArea);
		}
	}

	protected void ShowPolicyArea(TierOrAreaType tierArea)
	{
		if (tierArea != null)
		{
			policyIcon.SetSkinLocation(SkinState.Normal, gui.GUISpriteSheet.GetSourceRectangle(tierArea.Icon));
			policyIcon.SetSkinLocation(SkinState.Hover, gui.GUISpriteSheet.GetSourceRectangle(tierArea.Icon), Color.Gray, Color.Gray, flipHorizontally: false, modulateColor: true);
			policyIcon.ResizeControlToFitImage();
			policyIcon.ToolTip = Common.ComposeHeadingAndBlobText(tierArea.ToString(), tierArea.Description);
			policyIcon.AlignRight(policyHeader.Width - 15);
			policyIcon.TooltipExpires = false;
			if (!The.InGameUI.GetExpedition().Policy.CanProduceOrTrade(tierArea))
			{
				policyIcon.Color = DataTypeButton.notInStockColorLight;
				policyIcon.ToolTip = tierArea.GetNotAvailableTooltip();
			}
			else
			{
				policyIcon.Color = Color.White;
				policyIcon.ToolTip = "We have the needed policy: " + tierArea.ToString();
			}
			grdProductionOuter.AddEntry(policyHeader, policyHeader);
		}
	}

	private void PopulateGatheredAt()
	{
		grdProductionOuter.TryRemoveEntry(gatheredFromHeader);
		grdProductionOuter.TryRemoveEntry(lblGatheredFrom);
		if (processTypeToShowProductionFor != null && processTypeToShowProductionFor.IsGathering)
		{
			ResourceType resourceTypeInput = processTypeToShowProductionFor.ResourceTypeInput;
			if (resourceTypeInput != null)
			{
				lblGatheredFrom.Text = resourceTypeInput.Name;
			}
			else
			{
				lblGatheredFrom.Text = "CONSULT DATASHEET";
			}
			grdProductionOuter.AddEntry(gatheredFromHeader, gatheredFromHeader);
			grdProductionOuter.AddEntry(lblGatheredFrom, lblGatheredFrom);
		}
	}

	private void PopulateInputList(EntityGroup resolvedOwner)
	{
		grdInputs.BeginAddingEntries();
		grdProductionOuter.TryRemoveEntry(madeFromHeader);
		if (processTypeToShowProductionFor != null)
		{
			if (processTypeToShowProductionFor.InputsByType != null && processTypeToShowProductionFor.InputsByType.Count > 0 && !processTypeToShowProductionFor.IsGathering)
			{
				if (processTypeToShowProductionFor.IsKilling)
				{
					lblMadeFrom.Text = "YIELDED FROM:";
				}
				else
				{
					lblMadeFrom.Text = GetInputHeading();
				}
				lblMadeFrom.Visible = true;
				icMadeFrom.Visible = true;
			}
			else
			{
				lblMadeFrom.Text = "";
				lblMadeFrom.Visible = false;
				icMadeFrom.Visible = false;
			}
			if (lblMadeFrom.Visible)
			{
				grdProductionOuter.AddEntry(madeFromHeader, madeFromHeader);
				if (icMadeFrom.Visible)
				{
					lblMadeFrom.ToolTip = "We need all of the below materials, in the given amounts";
					lblMadeFrom.X = 25;
				}
				else
				{
					lblMadeFrom.ToolTip = "This item is gathered from a resource";
					lblMadeFrom.X = 10;
				}
			}
			if (processTypeToShowProductionFor.InputsByType != null && processTypeToShowProductionFor.InputsByType.Count > 0)
			{
				if (processTypeToShowProductionFor.InputsByType != null)
				{
					foreach (KeyValuePair<EntityType, Input> item2 in processTypeToShowProductionFor.InputsByType)
					{
						EntityType key = item2.Key;
						InventoryPanel.HasInputForProcess(processTypeToShowProductionFor, resolvedOwner, out var hasInputs, out var _, out var _, out var _, out var noOfAvailableItems, item2.Key);
						bool flag = hasInputs;
						bool value = InventoryPanel.HasToolsForProcess(processTypeToShowProductionFor, resolvedOwner);
						if (!grdInputs.TryGetEntry(key, out var item))
						{
							item = AddEntityAmountRow(grdInputs, key, flag, "The amount of materials that are needed");
						}
						UpdateRequiredItemRow(item, key, 0f, value, hasInputs, flag, item2.Value.Amount.NoOfItems, noOfAvailableItems);
					}
				}
				grdInputs.DeleteEntries((EntityType e) => processTypeToShowProductionFor.InputsByType.ContainsKey(e));
			}
			else
			{
				grdInputs.Clear();
			}
		}
		else
		{
			grdInputs.Clear();
		}
		grdInputs.EndAddingEntries();
	}

	private void PopulateActingOn()
	{
		if (actingOn != null)
		{
			grdProductionOuter.TryRemoveEntry(actingOnHeader);
			grdProductionOuter.TryRemoveEntry(actingOn);
		}
		if (processTypeToShowProductionFor != null && processTypeToShowProductionFor.ActingOnType != null)
		{
			EntityType actingOnType = processTypeToShowProductionFor.ActingOnType;
			if (actingOn == null)
			{
				actingOn = new UIComponent(gui);
				StockpileWindow.CreateItemGridRow(actingOnType, owner, useCurrentUIOwner, actingOn, InfoToShow.Data, usePluralName: false, isRoot: false, out var entityTypeButton);
				entityTypeButton.X = 42;
				entityTypeButton.Width = 154;
				actingOn.OrderByTag1 = 16f;
				actingOn.Height = 23;
			}
			grdProductionOuter.AddEntry(actingOnHeader, actingOnHeader);
			grdProductionOuter.AddEntry(actingOn, actingOn);
			Icon icon = (Icon)actingOn.FindChildById(UIComponent.DataControlID.Icon, firstLevelOnly: true);
			IconInfo iconInfo;
			Rectangle iconSprite = actingOnType.GetIconSprite(out iconInfo);
			icon.SetSkinLocation(SkinState.Normal, iconSprite);
			icon.ResizeControlToFitImage();
			actingOn.CenterChildVertically(icon, iconInfo?.CenterYPos);
			actingOn.CenterHorizontally(24, icon);
			((DataTypeButton)actingOn.FindChildById(UIComponent.DataControlID.Caption, firstLevelOnly: true)).FillEntityType(InfoToShow.Data, actingOnType, null, useUIOwner: true, actingOnType.Name);
		}
	}

	protected UIComponent AddEntityAmountRow(Grid grid, EntityType inputEntityType, bool ownsItem, string amountTooltip)
	{
		UIComponent uIComponent = new UIComponent(gui);
		grid.AddEntry(inputEntityType, uIComponent);
		CreateItemGridRow(inputEntityType, uIComponent, out var entityTypeButton);
		entityTypeButton.SetAvailableStatusColor(ownsItem);
		uIComponent.CenterChildVertically(entityTypeButton);
		Label label = new Label(gui);
		label.Init(Label.LabelType.HUDWindow);
		label.X = entityTypeButton.Right + 12;
		uIComponent.Add(label);
		label.ToolTip = amountTooltip;
		label.ID = UIComponent.DataControlID.Amount;
		RightJustify(label);
		uIComponent.CenterChildVertically(label);
		label.Y++;
		return uIComponent;
	}

	private void RightJustify(Label lbl)
	{
		lbl.FitToText();
		lbl.X = 256 - lbl.TextWidth - 6 - 5 - 5;
	}

	protected void UpdateEntityAmountRow(UIComponent itemRow, EntityType entityType, bool isAvailable, int amount)
	{
		((DataTypeButton)itemRow.FindChildById(UIComponent.DataControlID.Caption)).SetAvailableStatusColor(isAvailable);
		Bar bar = (Bar)itemRow.FindChildById(UIComponent.DataControlID.Background);
		RefreshTrackedColorBar(entityType, bar);
		Label label = (Label)itemRow.FindChildById(UIComponent.DataControlID.Amount);
		if (label != null)
		{
			label.Text = amount.ToString();
			label.FitToText();
			RightJustify(label);
		}
	}

	protected void UpdateRequiredItemRow(UIComponent itemRow, EntityType entityType, float score, bool? hasTools = null, bool? hasInputs = null, bool? isAvailable = null, int? neededAmount = null, int? availableAmount = null)
	{
		((DataTypeButton)itemRow.FindChildById(UIComponent.DataControlID.Caption)).SetAvailableStatusColor(isAvailable == true);
		itemRow.OrderByTag1 = score;
		Bar bar = (Bar)itemRow.FindChildById(UIComponent.DataControlID.Background);
		RefreshTrackedColorBar(entityType, bar);
		Label label = (Label)itemRow.FindChildById(UIComponent.DataControlID.Amount);
		if (label == null)
		{
			return;
		}
		if (neededAmount.HasValue)
		{
			if (availableAmount.HasValue)
			{
				label.Text = availableAmount.Value + "/" + neededAmount.Value.ToString();
				StringBuilder stringBuilder = new StringBuilder();
				if (availableAmount.Value >= neededAmount.Value)
				{
					Common.AppendLine(stringBuilder, "We have the needed amount of this input.");
					Common.Append(stringBuilder, "Available: ");
					Common.Append(stringBuilder, availableAmount.Value.ToString(), tintAsValue: true);
					label.NormalColor = Color.White;
				}
				else
				{
					Common.AppendLine(stringBuilder, "We do not have the needed amount of this input.");
					Common.Append(stringBuilder, "Available: ");
					Common.Append(stringBuilder, availableAmount.Value.ToString(), Common.ValueTint.Negative);
					label.NormalColor = DataTypeButton.notInStockColorLight;
				}
				Common.Append(stringBuilder, " / Needed: ");
				Common.Append(stringBuilder, neededAmount.Value.ToString(), tintAsValue: true);
				label.ToolTip = stringBuilder.ToString();
			}
			else
			{
				label.Text = neededAmount.Value.ToString();
				label.NormalColor = Color.White;
				label.ToolTip = "Needed amount";
			}
			label.FitToText();
			RightJustify(label);
		}
		else
		{
			label.Text = "0";
			label.NormalColor = Color.White;
		}
	}

	protected static void RefreshTrackedColorBar(EntityType entityType, Bar bar)
	{
		string toolTip = null;
		if (The.InGameUI.InventorySettings.GetTrackedColorAndTooltip(entityType, out var color, out toolTip))
		{
			bar.Visible = true;
			bar.SetSkinLocation(SkinState.Normal, null, color, color);
			Color value = new Color(color.Value.R - 40, color.Value.G - 40, color.Value.B - 40);
			bar.SetSkinLocation(SkinState.Hover, null, value, value);
			bar.ToolTip = toolTip;
		}
		else
		{
			bar.Visible = false;
		}
	}

	private UIComponent AddToolRow(Grid grid, EntityType entityType, float productivity, float score, bool hasInputs, bool hasTools, bool ownsItem)
	{
		UIComponent uIComponent = new UIComponent(gui);
		grid.AddEntry(entityType, uIComponent);
		CreateItemGridRow(entityType, uIComponent, out var entityTypeButton);
		uIComponent.OrderByTag1 = score;
		uIComponent.CenterChildVertically(entityTypeButton);
		Label label = new Label(gui);
		label.Init(Label.LabelType.HUDWindow);
		label.Text = productivity.ToString("N", Config.Culture);
		label.X = entityTypeButton.Right + 6;
		uIComponent.Add(label);
		RightJustify(label);
		label.DebugTag = "toolProd";
		label.ToolTip = "Productivity rating";
		uIComponent.CenterChildVertically(label);
		label.Y++;
		return uIComponent;
	}

	protected void CreateItemGridRow(EntityType entityType, UIComponent itemRow, out DataTypeButton entityTypeButton)
	{
		StockpileWindow.CreateItemGridRow(entityType, owner, useCurrentUIOwner, itemRow, InfoToShow.Production, usePluralName: false, isRoot: false, out entityTypeButton);
		entityTypeButton.X = 36;
		entityTypeButton.Width = 154;
		Bar control = CreateTrackedColorBar(entityTypeButton.Right);
		itemRow.Add(control);
	}

	protected Bar CreateTrackedColorBar(int x)
	{
		Bar bar = new Bar(Interface.gui);
		bar.ID = UIComponent.DataControlID.Background;
		bar.X = x;
		bar.Height = 23;
		bar.Width = 15;
		Rectangle sourceRectangle = Interface.gui.GUISpriteSheet.GetSourceRectangle("basic_highlightBar_white");
		bar.SetSkinLocation(SkinState.Normal, sourceRectangle);
		bar.SetSkinLocation(SkinState.Hover, sourceRectangle);
		bar.Visible = false;
		return bar;
	}

	private void PopulateToolsList(EntityGroup resolvedOwner)
	{
		EntityType entityType = null;
		foreach (KeyValuePair<ToolAlternatives, ToolGrid> currentToolGrid in currentToolGrids)
		{
			ToolGrid value = currentToolGrid.Value;
			Grid grid = value.Grid;
			ToolAlternatives key = currentToolGrid.Key;
			int num = value.ScrollPosition;
			value.HeaderRow.FindChildById<ImageButton>(UIComponent.DataControlID.Expand, out var child, firstLevelOnly: false);
			int num2 = ((!child.IsChecked) ? GameData.Instance.GUIConstants.NoOfToolsToDisplayWhenCollapsed : GameData.Instance.GUIConstants.NoOfToolsToDisplayWhenExpanded);
			toolsInGrid.Clear();
			List<Tuple<EntityType, float, float, bool, bool, bool>> list = ScoreTools(resolvedOwner, key);
			int num3 = Common.Clamp(num, 0, list.Count - num2);
			if (num3 != num)
			{
				currentToolGrid.Value.ScrollPosition = num3;
				num = num3;
			}
			int num4 = Common.ClampTop(num + num2, list.Count);
			if (!child.IsChecked)
			{
				if (num4 < list.Count)
				{
					child.Visible = true;
				}
				else
				{
					child.Visible = false;
				}
			}
			else
			{
				child.Visible = true;
			}
			grid.BeginAddingEntries();
			int num5 = -1;
			try
			{
				for (int i = num; i < num4; i++)
				{
					num5 = i;
					Tuple<EntityType, float, float, bool, bool, bool> tuple = list[i];
					entityType = tuple.Item1;
					float item = tuple.Item2;
					toolsInGrid.Add(entityType);
					if (!grid.TryGetEntry(entityType, out var item2))
					{
						item2 = AddToolRow(grid, entityType, item, tuple.Item3, tuple.Item4, tuple.Item5, tuple.Item6);
					}
					UpdateRequiredItemRow(item2, entityType, tuple.Item3, tuple.Item4, tuple.Item5, tuple.Item6);
				}
			}
			catch (Exception ex)
			{
				string message = ex.Message;
				message = message + " \n lblName.Text: " + lblName.Text;
				message += ((" \n entityType: " + entityType != null) ? entityType.KeyName : "null");
				message = message + " \n thisIndex: " + num5;
				message = message + " \n maxToolIndex: " + num4;
				message = message + " \n firstToolIndexToShow: " + num;
				throw new Exception("#1 " + message);
			}
			grid.DeleteEntries((EntityType entityType2) => toolsInGrid.Exists((EntityType t) => entityType2 == t));
			grid.Sort((UIComponent uIComponent) => (float)uIComponent.OrderByTag1);
			if (showAllTools)
			{
				ShowOrHideToolGridScrollButtons(key, value, list.Count);
			}
			grid.EndAddingEntries();
		}
	}

	protected void CreateGridAndHeader(Grid outerGrid, out UIComponent header, string title, string tooltip, float sortIndex, out Grid grid, out Label lblHeader)
	{
		header = AddSubHeader(outerGrid, title, out lblHeader);
		header.OrderByTag1 = sortIndex;
		lblHeader.ToolTip = tooltip;
		grid = new Grid(gui, ListBoxType.HUDAndLCD, Label.LabelType.EntityTypeTooltip);
		grid.IsOuterGrid = false;
		grid.X = 6;
		grid.FixedItemHeights = true;
		grid.Width = outerGrid.Width;
		grid.ScrollBarEnabled = false;
		grid.ItemHeight = 22;
		grid.CanGrowInHeight = true;
		grid.Font = GUIManager.LCDandHUDBodyFontPath;
		grid.Height = 160;
		grid.OrderByTag1 = sortIndex + 1f;
		outerGrid.AddEntry(grid, grid);
	}

	protected virtual void PopulateProductionContentRefresh()
	{
	}

	protected virtual void PopulateGeneralDataContentRefresh()
	{
	}

	protected virtual void PopulateGeneralDataContent()
	{
	}

	private bool ProcessHasEntityTypeAsOutput(ProcessType processType, EntityType entityType)
	{
		return processType.Outputs.FirstOrDefault((Output o) => o.FinalEntityTypeToCreate == entityType) != null;
	}

	private void ShowOrHideToolGridScrollButtons(ToolAlternatives tools, ToolGrid toolGrid, int toolScoresCount)
	{
		UIComponent item;
		if (toolScoresCount > GameData.Instance.GUIConstants.NoOfToolsToDisplayWhenCollapsed && !toolGrid.ScrollButtonsAreShown)
		{
			if (grdProductionOuter.TryGetEntry(tools, out item) && item.FindChildById(UIComponent.DataControlID.Up) == null)
			{
				item.Add(toolGrid.btScrollUp);
				item.Add(toolGrid.btScrollDown);
				toolGrid.ScrollButtonsAreShown = true;
			}
		}
		else if (toolScoresCount <= GameData.Instance.GUIConstants.NoOfToolsToDisplayWhenCollapsed && toolGrid.ScrollButtonsAreShown && grdProductionOuter.TryGetEntry(tools, out item))
		{
			item.Remove(toolGrid.btScrollDown);
			item.Remove(toolGrid.btScrollUp);
			toolGrid.ScrollButtonsAreShown = false;
		}
	}

	private List<Tuple<EntityType, float, float, bool, bool, bool>> ScoreTools(EntityGroup resolvedOwner, ToolAlternatives tools)
	{
		List<Tuple<EntityType, float, float, bool, bool, bool>> list = new List<Tuple<EntityType, float, float, bool, bool, bool>>();
		foreach (Tuple<EntityType, float> item2 in tools.ToolsAndProductivity)
		{
			bool hasInputs;
			bool hasTools;
			bool isAvailable;
			float item = ScoreItem(resolvedOwner, item2.Item1, item2.Item2, out hasInputs, out hasTools, out isAvailable, isToolContext: true);
			list.Add(new Tuple<EntityType, float, float, bool, bool, bool>(item2.Item1, item2.Item2, item, hasInputs, hasTools, isAvailable));
		}
		return list.OrderByDescending((Tuple<EntityType, float, float, bool, bool, bool> s) => s.Item3).ToList();
	}

	protected float ScoreItem(EntityGroup resolvedOwner, EntityType item, float productivity, out bool hasInputs, out bool hasTools, out bool isAvailable, bool isToolContext)
	{
		InventoryPanel.OwnsProductOrHasProcessInputsAndTools(item, resolvedOwner, out var ownsItem, out var ownsItemIncludingIntrinsicPart, out hasInputs, out hasTools, null, countPartsOfEntities: true);
		float num = 0f;
		if (isToolContext)
		{
			num = ScoreItem(item, ownsItemIncludingIntrinsicPart);
			isAvailable = ownsItemIncludingIntrinsicPart;
		}
		else
		{
			num = ScoreItem(item, ownsItem);
			isAvailable = ownsItem;
		}
		return num + MathHelper.Lerp(0f, 0.05f, productivity);
	}

	private static float ScoreItem(EntityType item, bool isAvailable)
	{
		Dictionary<ProcessType, AttainableInfo> dictionary = null;
		float result = 0f;
		if (!isAvailable)
		{
			dictionary = The.InGameUI.InventorySettings.GetAttainableInfo(item);
		}
		if (isAvailable)
		{
			result = 100f;
		}
		else if (dictionary != null && dictionary.Any((KeyValuePair<ProcessType, AttainableInfo> a) => a.Value.IsProducable))
		{
			result = 10f;
		}
		return result;
	}

	private void Collapse()
	{
		state = State.Collapsed;
		currentlyShownExpandedGrid = null;
		DisplayWindow.Width = 256;
		DisplayWindow.Height = 104;
	}

	protected virtual void Retire()
	{
	}

	public override void Hide()
	{
		if (btPin.IsChecked)
		{
			The.InGameUI.EntityTypeTooltipsStack.Remove(this);
			return;
		}
		DisplayWindow.Hide();
		Collapse();
		Retire();
		if (SpawningControl != null)
		{
			if (SpawningControl is TextButton textButton)
			{
				textButton.IsChecked = false;
			}
			SpawningControl = null;
		}
		DataSheet childTooltip = The.InGameUI.GetChildTooltip(this);
		if (childTooltip != null)
		{
			childTooltip.Hide();
			childTooltip = null;
		}
		The.InGameUI.EntityTypeTooltipsStack.Remove(this);
		The.InGameUI.PinnedDataTypeTooltips.Remove(this);
		tooltipAnchor = null;
		timePassed = 0.0;
		The.InGameUI.InventorySettings.TrackTargetsChanged -= InventorySettings_TrackTargetsChanged;
	}
}
