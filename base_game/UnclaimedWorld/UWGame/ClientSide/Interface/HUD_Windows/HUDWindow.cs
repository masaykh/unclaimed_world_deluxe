using System;
using InputEventSystem;
using Microsoft.Xna.Framework;
using UWGame.SimSide;
using WindowSystem;

namespace UWGame.ClientSide.Interface.HUD_Windows;

public abstract class HUDWindow
{
	protected const int windowEdgeX = 3;

	protected const int windowEdgeY = 3;

	protected const int sideMargin = 6;

	protected const int bottomMargin = 6;

	protected const int topMargin = 6;

	protected const int correctedSideMargin = 9;

	protected const int correctedTopMargin = 9;

	protected const int correctedBottomMargin = 9;

	protected const int singleSpacing = 6;

	protected const int doubleSpacing = 12;

	protected const int tripleSpacing = 18;

	public Window DisplayWindow;

	private Regulator updateRegulator;

	public Vector2? WorldPosition;

	protected GUIManager gui;

	public CommonInterface Interface;

	public bool UpdateWhileHidden;

	public bool HideOnRightClick = true;

	protected ImageButton btClose;

	private string surface;

	public const int buttonWidth = 48;

	private const int captionX = 0;

	private const int buildX = 150;

	private const int beginX = 100;

	protected int CloseButtonYPos
	{
		set
		{
			if (btClose != null)
			{
				btClose.Y = value;
			}
		}
	}

	protected int TitleBarHeight
	{
		get
		{
			if (btClose != null)
			{
				return btClose.Bottom;
			}
			return 0;
		}
	}

	public HUDWindow(int width, int height, bool hasSurface = true, bool hasCloseButton = false, bool isMovable = false, string spriteName = "HUD_window_base", bool hideWhenMouseExits = false, Level level = Level.RockBottom, CommonInterface intf = null)
		: this(intf ?? The.InGameUI, width, height, hasSurface, hasCloseButton, isMovable, spriteName, hideWhenMouseExits, level)
	{
	}

	public HUDWindow(CommonInterface intf, int width, int height, bool hasSurface = true, bool hasCloseButton = false, bool isMovable = false, string spriteName = "HUD_window_base", bool hideWhenMouseExits = false, Level level = Level.RockBottom)
	{
		gui = intf.gui;
		Interface = intf;
		updateRegulator = new Regulator(intf.Game.Controller.RandomGenerator, 2.0, "HUDWindow");
		DisplayWindow = new Window(intf.gui);
		if (hasSurface)
		{
			ChangeSurface(spriteName);
			DisplayWindow.CornerSize = 14;
		}
		DisplayWindow.Opacity = 0.83f;
		DisplayWindow.Margin = 0;
		DisplayWindow.Level = level;
		DisplayWindow.Resizable = false;
		DisplayWindow.IsMovable = isMovable;
		DisplayWindow.WindowSize = new Vector2(width, height);
		DisplayWindow.HasCloseButton = false;
		DisplayWindow.HasCRTOrLCDComponents = false;
		DisplayWindow.HasOverlayComponents = false;
		DisplayWindow.Resize += DisplayWindow_Resize;
		DisplayWindow.Hide();
		if (hasCloseButton)
		{
			AddCloseButton();
		}
		if (The.InGameUI != null)
		{
			The.InGameUI.AddHudWindow(this);
		}
		if (hideWhenMouseExits)
		{
			DisplayWindow.ViewPort.MouseOut += ViewPort_MouseOut;
		}
	}

	private void DisplayWindow_Click(UIComponent sender, EventArgs e)
	{
		DisplayWindow.BringToTop();
	}

	public void ChangeSurface(string surfaceSpriteName)
	{
		if (surfaceSpriteName != surface)
		{
			surface = surfaceSpriteName;
			DisplayWindow.SetSkin(gui.GUISpriteSheet.GetSourceRectangle(surfaceSpriteName));
		}
	}

	private void DisplayWindow_Resize(UIComponent sender)
	{
		if (btClose != null)
		{
			SetCloseButtonPosition();
		}
	}

	private void SetCloseButtonPosition()
	{
		btClose.X = DisplayWindow.Width - 6 - btClose.Width;
	}

	private void ViewPort_MouseOut(UIComponent sender, MouseEventArgs args)
	{
		Hide();
	}

	protected void RightJustify(Label lbl, int sideMarginToUse)
	{
		lbl.FitToText();
		lbl.X = DisplayWindow.Width - lbl.TextWidth - sideMarginToUse;
	}

	protected void RightJustify(UIComponent lbl, int sideMarginToUse)
	{
		lbl.X = DisplayWindow.Width - lbl.Width - sideMarginToUse;
	}

	public virtual void Hide()
	{
		DisplayWindow.Hide();
	}

	protected void AddCloseButton()
	{
		btClose = new ImageButton(gui);
		Add(btClose);
		btClose.Init(ImageButtonType.HUDClose);
		btClose.Y = 6;
		SetCloseButtonPosition();
		btClose.ToolTip = "Close";
		btClose.Click += btClose_Click;
		btClose.DebugTag = "hudClose";
	}

	private void btClose_Click(UIComponent sender, EventArgs e)
	{
		Hide();
	}

	protected void SetDisplayName(string zoneName, Label lblName, Label lblHeader, Image icon)
	{
		lblName.Text = zoneName;
		lblName.FitToText();
		if (string.IsNullOrEmpty(zoneName))
		{
			icon.X = lblName.X;
		}
		else
		{
			icon.X = lblName.Right + 6;
		}
		lblHeader.X = icon.Right + 6;
	}

	protected void AddZoneNameAndHeader(string zoneName, string header, string icon, int margin, out Label lblName, out Label lblHeader, out Image headerIcon)
	{
		lblName = new Label(gui);
		Add(lblName);
		lblName.Init(Label.LabelType.HUDWindowHeader);
		lblName.Text = zoneName;
		lblName.FitToText();
		lblName.X = margin;
		lblName.Y = margin;
		lblHeader = new Label(gui);
		Add(lblHeader);
		lblHeader.Init(Label.LabelType.HUDWindow);
		lblHeader.Text = header;
		lblHeader.FitToText();
		lblHeader.X = lblName.Right + 12;
		lblHeader.Y = margin;
		headerIcon = new Image(gui);
		Add(headerIcon);
		headerIcon.Texture = gui.GUISpriteSheet.Texture;
		Rectangle sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle(icon);
		headerIcon.SetSkinLocation(SkinState.Normal, sourceRectangle);
		headerIcon.ResizeControlToFitImage();
		headerIcon.Y = margin + 4;
	}

	protected void Add(UIComponent control)
	{
		DisplayWindow.Add(control);
	}

	protected void Remove(UIComponent control)
	{
		DisplayWindow.Remove(control);
	}

	public void SetScreenPosition(Point newPos)
	{
		DisplayWindow.Position = newPos;
	}

	public Point GetScreenPosition()
	{
		return DisplayWindow.Position;
	}

	public void SetWorldPosition(Point newPos)
	{
		Vector2 vector = The.MapUI.ScreenToWorldPos(newPos.X, newPos.Y);
		Vector2 pos = vector + DisplayWindow.WindowSize;
		pos = The.Map.ClampWorldPosition(pos);
		vector = pos - DisplayWindow.WindowSize;
		WorldPosition = vector;
	}

	public virtual void ShowInScreenSpace(int screenPosX, int screenPosY, bool modal = false)
	{
		if (modal)
		{
			DisplayWindow.ShowModal();
		}
		else
		{
			DisplayWindow.Show();
		}
		SetScreenPosition(new Point(screenPosX, screenPosY));
	}

	public virtual void ShowOnPlayfield(int screenPosX, int screenPosY, bool avoidRightInterfaceArea = true)
	{
		DisplayWindow.Show();
		SetScreenPosition(new Point(screenPosX, screenPosY));
		PlaceWindowInsideViewableArea(avoidRightInterfaceArea);
		SetWorldPosition(GetScreenPosition());
	}

	protected void PlaceWindowInsideViewableArea(bool avoidRightInterfaceArea)
	{
		int num = The.Sim.Controller.DrawArea.Width;
		if (avoidRightInterfaceArea)
		{
			num -= 309;
		}
		if (DisplayWindow.Right > num)
		{
			DisplayWindow.X = num - DisplayWindow.Width;
		}
		int num2 = The.Sim.Controller.DrawArea.Height - 160;
		if (DisplayWindow.Bottom > num2)
		{
			DisplayWindow.Y = num2 - DisplayWindow.Height;
		}
	}

	public static Grid CreateOuterGridForCollapsableLists(GUIManager gui, UIComponent surface, bool addToSurface = true)
	{
		Grid grid = new Grid(gui, ListBoxType.HUDAndLCD, Label.LabelType.HUDWindow);
		grid.FixedItemHeights = false;
		grid.RenderType = RenderType.Normal;
		if (addToSurface)
		{
			surface.Add(grid);
		}
		grid.HMargin = 0;
		grid.VMargin = 0;
		grid.Font = GUIManager.LCDandHUDBodyFontPath;
		grid.Width = surface.Width;
		grid.Height = surface.Height;
		grid.ItemHeight = 26;
		grid.Position = new Point(0, 0);
		return grid;
	}

	public static void AddCollapsablePanelAndGrid(GUIManager gui, Grid outerGrid, string title, int? itemHeight, out CollapsablePanel panel, out Grid grid)
	{
		if (!itemHeight.HasValue)
		{
			itemHeight = 18;
		}
		panel = new CollapsablePanel(gui, CollapsablePanel.PanelType.HUD);
		panel.CollapsedHeight = outerGrid.ItemHeight;
		outerGrid.AddEntry(panel, panel);
		panel.Init();
		panel.Title = title;
		panel.Width = outerGrid.Width;
		grid = new Grid(gui, ListBoxType.HUDAndLCD, Label.LabelType.HUDWindow);
		grid.FixedItemHeights = true;
		grid.Width = panel.Width;
		panel.AddContent(grid);
		grid.ScrollBarEnabled = false;
		grid.ItemHeight = itemHeight.Value;
		grid.CanGrowInHeight = true;
		grid.Font = GUIManager.LCDandHUDBodyFontPath;
		grid.Selectability = Grid.SelectabilityOptions.Single;
	}

	public static void AddCollapsablePanelAndTreeGrid(GUIManager gui, Grid outerGrid, string title, CollapsablePanel.PanelType innerPanelType, out CollapsablePanel panel, out Grid grid)
	{
		panel = new CollapsablePanel(gui, CollapsablePanel.PanelType.HUD);
		panel.CollapsedHeight = outerGrid.ItemHeight;
		outerGrid.AddEntry(panel, panel);
		panel.Init();
		panel.Title = title;
		panel.Width = outerGrid.Width;
		grid = new Grid(gui, ListBoxType.HUDAndLCD, Label.LabelType.HUDWindow);
		grid.FixedItemHeights = false;
		grid.RenderType = RenderType.Normal;
		grid.Font = GUIManager.LCDandHUDBodyFontPath;
		int y = 0;
		grid.Width = panel.Width;
		panel.AddContent(grid);
		grid.ItemHeight = ((innerPanelType == CollapsablePanel.PanelType.DropDownBig) ? 26 : 22);
		grid.Position = new Point(0, y);
		grid.CanGrowInHeight = true;
	}

	protected void CreateMenuGrid(out Grid grid, int? yPos = null, bool hasScrollBar = true)
	{
		UIComponent uIComponent = new UIComponent(gui);
		Add(uIComponent);
		uIComponent.Y = (yPos.HasValue ? yPos.Value : 12);
		uIComponent.Width = DisplayWindow.ViewPort.Width - 12;
		uIComponent.Height = DisplayWindow.ViewPort.Height - uIComponent.Y;
		grid = new Grid(gui, ListBoxType.HUDAndLCD, Label.LabelType.HUDWindow);
		grid.FixedItemHeights = true;
		grid.RenderType = RenderType.Normal;
		uIComponent.Add(grid);
		grid.HMargin = 5;
		grid.VMargin = 5;
		grid.Font = GUIManager.LCDandHUDBodyFontPath;
		grid.Width = uIComponent.Width;
		grid.Height = uIComponent.Height;
		grid.ItemHeight = 26;
		grid.Position = new Point(0, 0);
		grid.ScrollBarEnabled = hasScrollBar;
	}

	protected void CreateSurfaceWithScrollbar(out Grid grid, out UIComponent listSurface, int? surfaceHeight = null, int topMarginToUse = 6, int sideMarginToUse = 6, bool canHaveFocus = true)
	{
		listSurface = new UIComponent(gui);
		Add(listSurface);
		listSurface.X = sideMarginToUse;
		listSurface.Y = topMarginToUse;
		listSurface.Width = DisplayWindow.ViewPort.Width - 2 * sideMarginToUse;
		listSurface.Height = (surfaceHeight.HasValue ? surfaceHeight.Value : (DisplayWindow.ViewPort.Height - listSurface.Y));
		listSurface.CanHaveFocus = canHaveFocus;
		grid = new Grid(gui, ListBoxType.HUDAndLCD, Label.LabelType.HUDWindow);
		grid.FixedItemHeights = false;
		grid.RenderType = RenderType.Normal;
		listSurface.Add(grid);
		grid.Font = GUIManager.LCDandHUDBodyFontPath;
		grid.Width = listSurface.Width;
		grid.Height = listSurface.Height;
		grid.Position = new Point(0, 0);
		grid.CanHaveFocus = canHaveFocus;
	}

	public virtual void UpdateContent(GameTime elapsed)
	{
		if (updateRegulator.IsReady())
		{
			Refresh();
		}
	}

	public virtual void Update(GameTime elapsed)
	{
	}

	public virtual void Refresh()
	{
	}
}
