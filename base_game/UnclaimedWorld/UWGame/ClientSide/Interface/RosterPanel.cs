using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface.Controls;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

public class RosterPanel : Panel
{
	protected InGameInterface intface = The.InGameUI;

	public bool HasStatusCRT;

	public ICanBeChecked MainControlButton;

	protected const int statusTextX = 8;

	protected const int statusTextWidth = 200;

	protected UIComponent statusContent;

	protected Bar crtUnderline;

	protected Label lblStatusHeading;

	protected Image imStatusBackground;

	protected UIComponent pnBillboards;

	protected Vector2 statusBillboardPanelCenter;

	protected const int hyperLinkMargin = 6;

	public const int SlimGridItemHeight = 18;

	public const int ItemHeight = 20;

	protected Box display;

	protected LCDScreen lcdScreen;

	protected UIComponent lcdSurface;

	protected bool hasBeenDrawn;

	public ImageButton AccessButton;

	private Icon tintedBackground;

	protected const int columnHeaderHeight = 20;

	protected const int headingsY = 0;

	protected int titleBottom;

	private const int sidePanelInterfaceWidth = 278;

	private const int sidePanelAccessPanelOverlap = 13;

	private const int eventArchiveAccessPanelOverlap = 17;

	private const int rosterPanelAccessPanelOverlap = 19;

	public const int IrregularCornerSize = 242;

	private const Level level = Level.BelowBelowMiddle;

	private ModalOverlay modalOverlay;

	protected Color? BackgroundTint
	{
		set
		{
			SetBackgroundTint(value, tintedBackground);
		}
	}

	public RosterPanel(string topTitle, int width, bool needBottomMarginForButtons)
		: this(topTitle, width, The.InGameUI.rosterPanelHeight, needBottomMarginForButtons)
	{
		modalOverlay = new ModalOverlay(Window);
	}

	public RosterPanel(int height, bool isInfoPanel, int bottomMargin = 28)
		: base(The.InGameUI, "", new Point(The.MapUI.mapWindowWidth - 278 - 51 + 13, 213), new Vector2(278f, height), Level.BelowBelowMiddle, PanelType.InfoPanel)
	{
		modalOverlay = new ModalOverlay(Window);
		Window.HasCloseButton = false;
		CreateInfoLCD(bottomMargin);
		HasStatusCRT = true;
		AddDirtOnSidePanel();
	}

	public RosterPanel(string topTitle, int width, int height, bool needBottomMarginForButton, int yPos = 18, PanelType panelType = PanelType.RosterPanel)
		: base(The.InGameUI, topTitle, new Point(The.MapUI.mapWindowWidth - width - 51 + 19, yPos), new Vector2(width, height), Level.BelowBelowMiddle, panelType)
	{
		Window.HasCloseButton = false;
		modalOverlay = new ModalOverlay(Window);
		if (needBottomMarginForButton)
		{
			CreateRosterStyleLCDPanel(The.InGameUI, Window, out display, out lcdSurface, ref lcdScreen, 52, 10);
		}
		else
		{
			CreateRosterStyleLCDPanel(The.InGameUI, Window, out display, out lcdSurface, ref lcdScreen, 20, 10);
		}
		AddTintedBackground(ref tintedBackground, display);
	}

	public RosterPanel(string topTitle, int width, int height)
		: base(The.InGameUI, topTitle, new Point(The.MapUI.mapWindowWidth - width - 51 + 17, 201), new Vector2(width, height), Level.BelowBelowMiddle, PanelType.EventArchive)
	{
		Window.HasCloseButton = false;
		modalOverlay = new ModalOverlay(Window);
		CreateRosterStyleLCDPanel(The.InGameUI, Window, out display, out lcdSurface, ref lcdScreen, 52, 7, addTopEdgeDirt: false);
	}

	public static void AddTintedBackground(ref Icon tintedBackground, Box display)
	{
		tintedBackground = new Icon(display.guiManager);
		tintedBackground.SetSkinLocation(SkinState.Normal, display.guiManager.GUISpriteSheet.GetSourceRectangle("whiteSquare"));
		tintedBackground.ScaleImageToSizeOfControl = true;
		tintedBackground.X = 8;
		tintedBackground.Y = 8;
		tintedBackground.Width = display.Width - 16;
		tintedBackground.Height = display.Height - 16;
		tintedBackground.Visible = false;
		display.Add(tintedBackground);
	}

	public static void SetBackgroundTint(Color? color, Icon tintedBackground)
	{
		if (color.HasValue)
		{
			tintedBackground.Visible = true;
			tintedBackground.Color = color;
		}
		else
		{
			tintedBackground.Visible = false;
		}
	}

	public static void CreateRosterStyleLCDPanel(CommonInterface intf, Window form, out Box display, out UIComponent surface, ref LCDScreen lcdScreen, int bottomMargin = 52, int? rightMargin = null, bool addTopEdgeDirt = true, bool addBottomEdgeDirt = true)
	{
		FullLCDPanel.AddLCDPanelFitWindowWithBottomMargin(intf, form, bottomMargin, Panel.RosterMargin, out display, out surface, ref lcdScreen, null, rightMargin);
		AddWatermark(intf.gui, surface);
		Panel.AddDirtOnIrregularEdges(intf.gui, form, addTopEdgeDirt, addBottomEdgeDirt);
	}

	public void ShowModalOverlay()
	{
		modalOverlay.Show(Window, lcdSurface, display);
	}

	public void RemoveModalOverlay()
	{
		modalOverlay.Remove(Window, lcdSurface, display);
	}

	protected void CreateInfoLCD(int bottomMargin = 28)
	{
		FullLCDPanel.AddLCDPanelFitWindowWithBottomMargin(The.InGameUI, Window, bottomMargin, new Point(15, 33), out display, out lcdSurface, ref lcdScreen, 260);
	}

	protected void AddDirtOnSidePanel()
	{
		GUIManager gui = The.InGameUI.gui;
		Rectangle sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle("Fingerprint");
		Image image = Panel.AddImage(Interface.gui, Window, sourceRectangle, new Point(display.Right - sourceRectangle.Width + 8, display.Y));
		image.RenderType = RenderType.Overlay;
		image.ResizeControlToFitImage();
		image.Alpha = 0.5f;
		AddDefaultDirt();
		sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_smallsplotch");
		Panel.AddImage(Interface.gui, Window, sourceRectangle, new Point(Window.Width - sourceRectangle.Width, Window.Height - sourceRectangle.Height)).RenderType = RenderType.Overlay;
		Panel.AddImage(Interface.gui, Window, "basic_dirt_bigsplotch", new Point(228, display.Bottom - 180)).RenderType = RenderType.Overlay;
	}

	public override void Hide()
	{
		if (AccessButton != null)
		{
			AccessButton.IsChecked = false;
		}
		_ = HasStatusCRT;
		base.Hide();
	}

	protected void SetHeaderText(string text)
	{
		lblStatusHeading.Text = text;
		crtUnderline.Width = lblStatusHeading.Width;
	}

	protected void InitStatusImage(int fixedWidth, int fixedHeight)
	{
		InitStatusImage();
		imStatusBackground.Width = fixedWidth;
		imStatusBackground.Height = fixedHeight;
		imStatusBackground.ScaleImageToSizeOfControl = true;
	}

	protected void InitStatusImage()
	{
		imStatusBackground = new Image(The.InGameUI.gui);
		statusContent.Add(imStatusBackground);
		imStatusBackground.Position = new Point(0, 2);
		imStatusBackground.ResizeControlToFitImage();
		imStatusBackground.RenderType = RenderType.CRTAndLCD;
		imStatusBackground.Alpha = 0.7f;
	}

	protected void ShowBillboardPanel()
	{
		statusContent.Insert(pnBillboards, 1);
	}

	public static Grid CreateOuterGridForCollapsableLists(GUIManager gui, UIComponent lcdSurface, int bottomMargin = 0)
	{
		int num = 4;
		Grid grid = new Grid(gui, ListBoxType.LCD, Label.LabelType.CRTBigGlow);
		grid.FixedItemHeights = false;
		grid.RenderType = RenderType.CRTAndLCD;
		lcdSurface.Add(grid);
		grid.Font = GUIManager.LCDandHUDBodyFontPath;
		grid.Width = lcdSurface.Width;
		grid.Height = lcdSurface.Height - num - bottomMargin;
		grid.ItemHeight = 26;
		grid.Position = new Point(0, num);
		return grid;
	}

	public static void CreateAndPlaceBillboards(GUIManager gui, UIComponent pnBillboards, EntityType entityType, Vector2 imageCenter, float? targetImageHeight, bool doScaling)
	{
		new List<Image>();
		if (entityType.RenderableTypeMode.DefaultClientState.RenderAsBillboardType[0].AssetName != null)
		{
			float num = GameData.Instance.BillboardSpriteSheet.GetSourceRectangle(entityType.RenderableTypeMode.DefaultClientState.RenderAsBillboardType[0].AssetName).Height;
			pnBillboards.Controls.Clear();
			bool flag = doScaling && Math.Abs(num - targetImageHeight.Value) > 18f;
			float num2 = Common.ClampTop(targetImageHeight.Value / num, 1.5f);
			RenderAsBillboardType[] renderAsBillboardType = entityType.RenderableTypeMode.DefaultClientState.RenderAsBillboardType;
			foreach (RenderAsBillboardType renderAsBillboardType2 in renderAsBillboardType)
			{
				Rectangle sourceRectangle = GameData.Instance.BillboardSpriteSheet.GetSourceRectangle(renderAsBillboardType2.AssetName);
				Vector2 vector = ((!(renderAsBillboardType2.BaseCenter == Vector2.Zero)) ? renderAsBillboardType2.BaseCenter : new Vector2((float)sourceRectangle.Width / 2f, (float)sourceRectangle.Height / 2f));
				Vector2 offset = renderAsBillboardType2.Offset;
				Image image = new Image(gui);
				pnBillboards.Add(image);
				image.ResizeControlToFitImage();
				image.RenderType = RenderType.CRTAndLCD;
				image.Alpha = 0.7f;
				image.SetSkinLocation(SkinState.Normal, sourceRectangle);
				image.Texture = GameData.Instance.BillboardSpriteSheet.Texture;
				if (flag)
				{
					image.Height = (int)(num2 * (float)sourceRectangle.Height);
					image.Width = (int)(num2 * (float)sourceRectangle.Width);
					image.ScaleImageToSizeOfControl = true;
					Vector2 vector2 = imageCenter + num2 * (-vector + offset);
					image.Position = new Point((int)vector2.X, (int)vector2.Y);
				}
				else
				{
					Vector2 vector2 = imageCenter - vector + offset;
					image.Position = new Point((int)vector2.X, (int)vector2.Y);
					image.ScaleImageToSizeOfControl = false;
					image.ResizeControlToFitImage();
				}
			}
		}
		else
		{
			pnBillboards.Controls.Clear();
		}
	}

	protected static bool AddPanelIfNotPresent(bool addPanel, Grid outerGrid, CollapsablePanel cp)
	{
		if (addPanel)
		{
			if (!outerGrid.EntriesByKey.ContainsKey(cp))
			{
				outerGrid.AddEntry(cp, cp);
			}
		}
		else
		{
			outerGrid.TryRemoveEntry(cp);
		}
		return addPanel;
	}

	protected void InitBillboardPanel()
	{
		pnBillboards = new UIComponent(The.InGameUI.gui);
		pnBillboards.Width = statusContent.Width;
		pnBillboards.Height = statusContent.Height;
		statusBillboardPanelCenter = new Vector2((float)statusContent.Width * 2f / 3f, (float)statusContent.Height / 2f);
	}

	protected static void InitStatusCRTHeader(GUIManager gui, UIComponent content, int leftMargin, out Label lblStatusHeading, out Bar underline)
	{
		lblStatusHeading = new Label(gui);
		content.Add(lblStatusHeading);
		lblStatusHeading.Position = new Point(8, 14);
		lblStatusHeading.Init(Label.LabelType.CRTNormal);
		lblStatusHeading.Width = 200;
		lblStatusHeading.Height = 24;
		underline = new Bar(gui);
		content.Add(underline);
		underline.Position = new Point(leftMargin, 36);
		underline.EdgeSize = 6;
		Rectangle sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle("CRT_LayoutLine");
		underline.SetSkinLocation(SkinState.Normal, sourceRectangle);
		underline.Width = 290;
		underline.Height = sourceRectangle.Height;
		underline.RenderType = RenderType.CRTAndLCD;
		underline.DebugTag = "underline";
	}

	private static void AddWatermark(GUIManager gui, UIComponent lcdSurface, int y = 58)
	{
		Image image = new Image(gui);
		image.SetSkinLocation(SkinState.Normal, gui.GUISpriteSheet.GetSourceRectangle("Watermark_UWLogo"));
		image.ResizeControlToFitImage();
		lcdSurface.Add(image);
		image.Y = y;
		lcdSurface.CenterChildHorizontally(image);
	}

	public static void AddCollapsablePanelAndGrid(GUIManager gui, Grid outerGrid, string title, int? itemHeight, out CollapsablePanel panel, out Grid grid, Grid.SelectabilityOptions selectability = Grid.SelectabilityOptions.Single, int sortOrder = 0)
	{
		if (!itemHeight.HasValue)
		{
			itemHeight = 18;
		}
		panel = new CollapsablePanel(gui, CollapsablePanel.PanelType.DropDownSmall);
		panel.CollapsedHeight = outerGrid.ItemHeight;
		panel.Init();
		outerGrid.AddEntry(panel, panel);
		panel.Title = title;
		panel.OrderByTag1 = sortOrder;
		grid = new Grid(gui, ListBoxType.LCD, Label.LabelType.LCDNormal);
		grid.FixedItemHeights = true;
		grid.Width = panel.Width;
		panel.AddContent(grid);
		grid.ScrollBarEnabled = false;
		grid.ItemHeight = itemHeight.Value;
		grid.CanGrowInHeight = true;
		grid.Font = GUIManager.LCDandHUDBodyFontPath;
		grid.Selectability = selectability;
		grid.IsOuterGrid = false;
		grid.CanReceiveMouseWheelEvents = false;
	}

	public static void AddCollapsablePanelAndGridNotFixed(GUIManager gui, Grid outerGrid, string title, out CollapsablePanel panel, out Grid grid, Grid.SelectabilityOptions selectability = Grid.SelectabilityOptions.Single)
	{
		panel = new CollapsablePanel(gui, CollapsablePanel.PanelType.DropDownSmall);
		panel.CollapsedHeight = outerGrid.ItemHeight;
		panel.Init();
		outerGrid.AddEntry(panel, panel);
		panel.Title = title;
		grid = new Grid(gui, ListBoxType.LCD, Label.LabelType.LCDNormal);
		grid.FixedItemHeights = false;
		grid.Width = panel.Width;
		panel.AddContent(grid);
		grid.ScrollBarEnabled = false;
		grid.CanGrowInHeight = true;
		grid.Font = GUIManager.LCDandHUDBodyFontPath;
		grid.Selectability = selectability;
		grid.IsOuterGrid = false;
		grid.CanReceiveMouseWheelEvents = false;
	}

	protected TextButton AddCloseButton(Window Form, Action<object, EventArgs> closeFunction)
	{
		TextButton textButton = AddLowerButton("CLOSE", "Closes the panel", Align.Right);
		textButton.Click += closeFunction.Invoke;
		return textButton;
	}

	protected virtual void OnCancel()
	{
	}

	public override void Show()
	{
		base.Show();
		if (statusContent != null)
		{
			The.InGameUI.StatusScreen.ChangeContent(statusContent);
		}
	}
}
