using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UWGame.SimSide;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

public class Panel
{
	public enum PanelType
	{
		RosterPanel,
		IrregularEdges,
		RegularEdges,
		InfoPanel,
		MainMenu,
		EventDialog,
		Ratings,
		Counters,
		EventArchive
	}

	public enum PanelOptions
	{
		None,
		SteelAndDust
	}

	protected enum Align
	{
		Left,
		Right
	}

	public Window Window;

	private Regulator updateRegulator;

	protected Label lblTitle;

	public CommonInterface Interface;

	public const int MarginX = 16;

	public const int MarginY = 8;

	public int MarginTop = 8;

	public int MarginBottom = 8;

	public const int BottomMarginForButtons = 52;

	public const int BottomMarginWithoutButtons = 20;

	public static readonly Point RosterMargin = new Point(15, 48);

	public const int SingleSpacing = 6;

	public const int DoubleSpacing = 12;

	protected const int BottomButtonYDistance = 48;

	protected const int bottomButtonXMargin = 20;

	protected const int lcdSideMargin = 6;

	public PanelType panelType;

	public Panel(CommonInterface intf, string title, Point position, Vector2 dimension, Level level, PanelType panelType = PanelType.IrregularEdges)
	{
		Interface = intf;
		this.panelType = panelType;
		updateRegulator = new Regulator(intf.Game.Controller.RandomGenerator, 1.0, "Panel");
		Window = new Window(intf.gui);
		Window.Position = position;
		Window.WindowSize = dimension;
		Window.Level = level;
		Window.IsMovable = false;
		Window.Resizable = false;
		Window.Margin = 0;
		Window.HasCloseButton = false;
		int x = 40;
		int y = 0;
		Label.LabelType type = Label.LabelType.RosterTitle;
		bool flag = true;
		string spriteName;
		int cornerSize;
		switch (panelType)
		{
		case PanelType.RosterPanel:
			spriteName = "rosterpanel_base";
			cornerSize = 242;
			MarginTop = 48;
			MarginBottom = 12;
			break;
		case PanelType.EventArchive:
			spriteName = "rosterpanel_eventarchive";
			cornerSize = 242;
			MarginTop = 48;
			MarginBottom = 12;
			break;
		case PanelType.InfoPanel:
			spriteName = "infopanel";
			cornerSize = 116;
			flag = false;
			break;
		case PanelType.RegularEdges:
			spriteName = "optionspanel_base";
			cornerSize = 111;
			MarginTop = 64;
			MarginBottom = 19;
			break;
		case PanelType.MainMenu:
			spriteName = "mainmenu_panel";
			cornerSize = 88;
			type = Label.LabelType.PlainPanelNormal;
			x = 62;
			y = 16;
			break;
		case PanelType.EventDialog:
			spriteName = "eventpanel";
			cornerSize = 1;
			y = -1;
			MarginTop = 50;
			MarginBottom = 8;
			break;
		case PanelType.Ratings:
			spriteName = "ratings_panel";
			cornerSize = 68;
			flag = false;
			break;
		case PanelType.Counters:
			spriteName = "counter_panel";
			cornerSize = 1;
			flag = false;
			break;
		default:
			spriteName = "rosterpanelUnattached_base";
			cornerSize = 242;
			MarginTop = 48;
			MarginBottom = 12;
			break;
		}
		Window.Skin = intf.gui.GUISpriteSheet.GetSourceRectangle(spriteName);
		Window.CornerSize = cornerSize;
		Window.Destroyed += Window_Destroyed;
		if (flag)
		{
			lblTitle = new Label(intf.gui);
			Window.Add(lblTitle);
			if (title != null)
			{
				lblTitle.Text = title;
			}
			lblTitle.X = x;
			lblTitle.Y = y;
			lblTitle.Init(type);
		}
		Window.Hide();
		Window.DrawContentEvent += DrawContent;
	}

	private void Window_Destroyed()
	{
		Destroy();
	}

	protected virtual void Destroy()
	{
	}

	protected void AddBottomButtonInSequence(TextButton lastButton, out TextButton newButton, string button1Text, string button1Tooltip)
	{
		newButton = new TextButton(Interface.gui);
		Window.Add(newButton);
		newButton.Init(TextButton.TextButtonType.White);
		newButton.Text = button1Text;
		newButton.ScaleWidthToFitText();
		newButton.ToolTip = button1Tooltip;
		if (lastButton == null)
		{
			PlaceLeftButtonUnderLCD(newButton);
		}
		else
		{
			PlaceButtonUnderLCD(newButton, lastButton.Right + 5);
		}
	}

	protected void PlaceLeftButtonUnderLCD(UIComponent button)
	{
		button.X = 20;
		button.Y = Window.Height - 48;
	}

	protected void PlaceRightButtonUnderLCD(UIComponent button)
	{
		button.X = Window.Width - 20 - button.Width;
		button.Y = Window.Height - 48;
	}

	protected void PlaceButtonUnderLCD(UIComponent button, int xPos = 20)
	{
		button.X = xPos;
		button.Y = Window.Height - 48;
	}

	protected void AddDefaultDirt()
	{
		switch (panelType)
		{
		case PanelType.RosterPanel:
		case PanelType.EventArchive:
			AddDirtOnIrregularEdges(Interface.gui, Window);
			break;
		case PanelType.IrregularEdges:
			AddDirtOnIrregularEdges(Interface.gui, Window);
			break;
		case PanelType.RegularEdges:
			AddDirtOnStraightEdges();
			break;
		case PanelType.InfoPanel:
			AddDirtOnInfoPanel();
			break;
		case PanelType.MainMenu:
		case PanelType.EventDialog:
		case PanelType.Ratings:
		case PanelType.Counters:
			break;
		}
	}

	public static void AddDirtOnIrregularEdges(GUIManager gui, Window Form, bool addTopEdgeDirt = true, bool addBottomEdgeDirt = true)
	{
		Rectangle sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_left");
		AddImage(gui, Form, sourceRectangle, new Point(0, 0)).RenderType = RenderType.Overlay;
		if (addBottomEdgeDirt)
		{
			sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_bottom");
			AddImage(gui, Form, sourceRectangle, new Point(Form.Width - sourceRectangle.Width, Form.Height - sourceRectangle.Height)).RenderType = RenderType.Overlay;
		}
		if (addTopEdgeDirt)
		{
			AddDirtOnIrregularTopEdge(gui, Form);
		}
		sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_right");
		AddImage(gui, Form, sourceRectangle, new Point(Form.Width - sourceRectangle.Width + 26, 30)).RenderType = RenderType.Overlay;
	}

	public static void AddDirtOnIrregularTopEdge(GUIManager gui, Window Form)
	{
		int num = 360;
		Rectangle sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_top");
		if (num + sourceRectangle.Width < Form.Width - 14)
		{
			AddImage(gui, Form, sourceRectangle, new Point(num, 21)).RenderType = RenderType.Overlay;
		}
	}

	protected void AddDirtOnStraightEdges(bool excludeBottomDirt = false)
	{
		int topLeftCornerToExcludeX = 130;
		int topLeftCornerToExcludeY = 43;
		AddDirtOnStraightEdges(Interface.gui, Window, topLeftCornerToExcludeX, topLeftCornerToExcludeY, excludeBottomDirt);
	}

	protected void AddDirtOnInfoPanel()
	{
		Rectangle sourceRectangle = Interface.gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_left");
		AddImage(Interface.gui, Window, sourceRectangle, new Point(0, 43)).RenderType = RenderType.Overlay;
		AddDirtOnBottomEdge(Interface.gui, Window);
		Window.HasOverlayComponents = true;
	}

	public static void AddDirtOnStraightEdges(GUIManager gui, Window Form, int topLeftCornerToExcludeX = 0, int topLeftCornerToExcludeY = 0, bool excludeBottomDirt = false)
	{
		Rectangle sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_left");
		AddImage(gui, Form, sourceRectangle, new Point(0, topLeftCornerToExcludeY)).RenderType = RenderType.Overlay;
		if (!excludeBottomDirt)
		{
			AddDirtOnBottomEdge(gui, Form);
		}
		AddDirtOnTopEdge(gui, Form, topLeftCornerToExcludeX);
		sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_right");
		AddImage(gui, Form, sourceRectangle, new Point(Form.Width - sourceRectangle.Width + 26, 0)).RenderType = RenderType.Overlay;
		Form.HasOverlayComponents = true;
	}

	public static void AddDirtOnTopEdge(GUIManager gui, Window Form, int topLeftCornerToExcludeX, bool renderAsOverlay = true)
	{
		Rectangle sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_top");
		Image image = AddImage(gui, Form, sourceRectangle, new Point(topLeftCornerToExcludeX, -9));
		if (renderAsOverlay)
		{
			image.RenderType = RenderType.Overlay;
		}
	}

	public static Image AddDirtOnBottomEdge(GUIManager gui, Window Form, bool renderAsOverlay = true)
	{
		Rectangle sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle("basic_dirt_bottom");
		Image image = AddImage(gui, Form, sourceRectangle, new Point(0, Form.Height - sourceRectangle.Height));
		if (renderAsOverlay)
		{
			image.RenderType = RenderType.Overlay;
		}
		return image;
	}

	public static void CreateGridWithColumnHeadingsWithFixedLength(UIComponent lcdSurface, int yPos, int itemHeight, out Grid grid, int bottomMargin = 0, params Tuple<string, int, int>[] columnHeadings)
	{
		CreateColumnHeadingsWithFixedLength(lcdSurface, yPos, columnHeadings);
		grid = FullLCDPanel.AddGridWithFixedItemHeights(lcdSurface.guiManager, lcdSurface, yPos + 20, bottomMargin);
		grid.ItemHeight = itemHeight;
		grid.Selectability = Grid.SelectabilityOptions.None;
	}

	public static void CreateGridWithColumnHeadings(UIComponent lcdSurface, int yPos, int itemHeight, out Grid grid, int bottomMargin = 0, params Tuple<string, int>[] columnHeadings)
	{
		CreateColumnHeadings(lcdSurface, yPos, columnHeadings);
		grid = FullLCDPanel.AddGridWithFixedItemHeights(lcdSurface.guiManager, lcdSurface, yPos + 20, bottomMargin);
		grid.ItemHeight = itemHeight;
		grid.Selectability = Grid.SelectabilityOptions.None;
	}

	public static int GetItemColumnFromHeader(int headerXPos)
	{
		return headerXPos - 6;
	}

	protected static void CreateColumnHeadings(UIComponent lcdSurface, int yPos, params Tuple<string, int>[] columnHeadings)
	{
		foreach (Tuple<string, int> tuple in columnHeadings)
		{
			Label label = new Label(lcdSurface.guiManager);
			label.Init(Label.LabelType.LCDSmallHeadingBanner);
			label.Text = tuple.Item1;
			lcdSurface.Add(label);
			label.Y = yPos;
			label.X = tuple.Item2;
			label.FitToText();
		}
	}

	protected static void CreateColumnHeadingsWithFixedLength(UIComponent lcdSurface, int yPos, params Tuple<string, int, int>[] columnHeadings)
	{
		foreach (Tuple<string, int, int> tuple in columnHeadings)
		{
			Label label = new Label(lcdSurface.guiManager);
			label.Init(Label.LabelType.LCDSmallHeadingBanner);
			label.Text = tuple.Item1;
			lcdSurface.Add(label);
			label.Y = yPos;
			label.X = tuple.Item2;
			label.Width = tuple.Item3;
		}
	}

	public void SetScreenPosition(Point newPos)
	{
		Window.Position = newPos;
	}

	protected TextButton AddLowerButton(string text, string tooltip, Align align)
	{
		TextButton textButton = new TextButton(Window.guiManager);
		Window.Add(textButton);
		textButton.Init(TextButton.TextButtonType.White);
		textButton.Text = text;
		textButton.ToolTip = tooltip;
		textButton.Height = 28;
		textButton.ScaleWidthToFitText();
		switch (align)
		{
		case Align.Left:
			PlaceLeftButtonUnderLCD(textButton);
			break;
		case Align.Right:
			PlaceRightButtonUnderLCD(textButton);
			break;
		}
		return textButton;
	}

	public static TextButton AddTextButton(GUIManager gui, Window window, Point position, TextButton.TextButtonType type, string text, string tooltip)
	{
		TextButton textButton = new TextButton(gui);
		window.Add(textButton);
		textButton.Init(type);
		textButton.Position = position;
		textButton.Text = text;
		textButton.ToolTip = tooltip;
		return textButton;
	}

	protected ActionButtonPanel CreateActionButtons(int width)
	{
		ActionButtonPanel actionButtonPanel = new ActionButtonPanel(Interface.gui);
		Window.Add(actionButtonPanel);
		actionButtonPanel.Init();
		actionButtonPanel.Width = width;
		actionButtonPanel.X = 16;
		actionButtonPanel.Y = Window.Height - actionButtonPanel.Height - 8;
		return actionButtonPanel;
	}

	public static Box AddIndentation(GUIManager gui, Window window, Point position, int width, int height)
	{
		Box box = new Box(gui);
		window.Add(box);
		Rectangle sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle("indent");
		box.SetSkinLocation(SkinState.Normal, sourceRectangle);
		box.CornerSize = 9;
		box.Width = width;
		box.Height = height;
		box.Position = position;
		return box;
	}

	public static Box AddMetalPlate(GUIManager gui, Window window, Point position, Point dimension)
	{
		Box box = new Box(gui);
		window.Add(box);
		Rectangle sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle("metal_plate");
		box.SetSkinLocation(SkinState.Normal, sourceRectangle);
		box.CornerSize = 4;
		box.Position = position;
		box.Width = dimension.X;
		box.Height = dimension.Y;
		return box;
	}

	public virtual void Hide()
	{
		if (Window.guiManager.GetModal() == Window && The.Client != null)
		{
			The.Client.SetModal(value: false);
		}
		Window.Hide();
	}

	public virtual void ShowInScreenSpace(int screenPosX, int screenPosY, bool modal = false)
	{
		if (modal)
		{
			Window.ShowModal();
		}
		else
		{
			Window.Show();
		}
		screenPosX = Common.ClampTop(screenPosX, Interface.gui.ScreenWidth - Window.Width);
		screenPosY = Common.ClampTop(screenPosY, Interface.gui.ScreenHeight - Window.Height);
		SetScreenPosition(new Point(screenPosX, screenPosY));
		Refresh();
	}

	public virtual void Show()
	{
		Window.Show();
		Refresh();
	}

	public virtual void ShowDialog(bool modal)
	{
		int x = (Interface.gui.ScreenWidth - Window.Width) / 2;
		int y = (Interface.gui.ScreenHeight - Window.Height) / 2;
		Window.Position = new Point(x, y);
		Window.Show();
		if (modal)
		{
			Interface.gui.SetModal(Window);
			if (The.Client != null)
			{
				The.Client.SetModal(value: true);
			}
		}
	}

	protected void CreateSurfaceWithScrollbar(out Grid grid, UIComponent surface, bool canHaveFocus = true, int topMargin = 0, int bottomMargin = 0)
	{
		grid = new Grid(Interface.gui, ListBoxType.LCD, Label.LabelType.LCDNormal);
		grid.FixedItemHeights = false;
		grid.RenderType = RenderType.Normal;
		surface.Add(grid);
		grid.Font = GUIManager.LCDandHUDBodyFontPath;
		grid.Width = surface.Width;
		grid.Height = surface.Height - topMargin - bottomMargin;
		grid.Position = new Point(0, topMargin);
		grid.CanHaveFocus = canHaveFocus;
	}

	public static void CreateTextArea(CommonInterface intf, ref TextArea area, Grid surfaceGrid)
	{
		area = new TextArea(intf.gui, ListBoxType.LCD);
		area.RenderType = RenderType.CRTAndLCD;
		area.Init(Label.LabelType.LCDNormal);
		area.CanGrowInHeight = true;
		surfaceGrid.AddEntry(area, area);
		area.X = 6;
		area.Y = 45;
		area.Width = surfaceGrid.SurfaceWidth - 12;
	}

	public int GetBottom()
	{
		return Window.Height - 8;
	}

	public int GetContentWidth()
	{
		return Window.Width - 32;
	}

	protected void InitSmallPanel()
	{
	}

	public static Image AddImage(GUIManager gui, Window window, string spriteName, Point position)
	{
		Image image = new Image(gui);
		Rectangle sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle(spriteName);
		image.SetSkinLocation(SkinState.Normal, sourceRectangle);
		window.Add(image);
		image.Position = position;
		image.ResizeControlToFitImage();
		image.CanHaveFocus = false;
		return image;
	}

	public static Image AddImage(GUIManager gui, UIComponent addToUIComponent, string spriteName, Point position)
	{
		Image image = new Image(gui);
		Rectangle sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle(spriteName);
		image.SetSkinLocation(SkinState.Normal, sourceRectangle);
		addToUIComponent.Add(image);
		image.Position = position;
		image.ResizeControlToFitImage();
		image.CanHaveFocus = false;
		return image;
	}

	public static Image AddImage(GUIManager gui, Window window, Rectangle rect, Point position)
	{
		Image image = new Image(gui);
		image.SetSkinLocation(SkinState.Normal, rect);
		window.Add(image);
		image.Position = position;
		image.ResizeControlToFitImage();
		image.CanHaveFocus = false;
		return image;
	}

	public static Image AddDust(GUIManager gui, Window window)
	{
		Image image = new Image(gui);
		Rectangle sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle("event_dust");
		image.SetSkinLocation(SkinState.Normal, sourceRectangle);
		image.Alpha = 0.04f;
		window.Add(image);
		image.Position = new Point(0, 0);
		image.ResizeControlToFitImage();
		image.CanHaveFocus = false;
		return image;
	}

	public static Image AddDust(GUIManager gui, Window window, Rectangle rect, Point position)
	{
		Image image = new Image(gui);
		image.SetSkinLocation(SkinState.Normal, rect);
		image.Alpha = 0.04f;
		window.Add(image);
		image.Position = position;
		image.ResizeControlToFitImage();
		image.CanHaveFocus = false;
		return image;
	}

	public static void AddDustOnFrame(GUIManager gui, Rectangle frame, int frameEdgeWidth, Window window, RenderType renderType)
	{
		Rectangle sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle("event_dust");
		sourceRectangle.Width = frame.Width;
		sourceRectangle.Height = frameEdgeWidth;
		AddDust(gui, window, sourceRectangle, new Point(frame.X, frame.Y)).RenderType = renderType;
		sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle("event_dust");
		sourceRectangle.Y = sourceRectangle.Y + frame.Height - frameEdgeWidth;
		sourceRectangle.Width = frame.Width;
		sourceRectangle.Height = frameEdgeWidth;
		AddDust(gui, window, sourceRectangle, new Point(frame.X, frame.Y + frame.Height - frameEdgeWidth)).RenderType = renderType;
		sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle("event_dust");
		sourceRectangle.Y += frameEdgeWidth;
		sourceRectangle.Width = frameEdgeWidth;
		sourceRectangle.Height = frame.Height - 2 * frameEdgeWidth;
		AddDust(gui, window, sourceRectangle, new Point(frame.X, frame.Y + frameEdgeWidth)).RenderType = renderType;
		sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle("event_dust");
		sourceRectangle.X = sourceRectangle.X + frame.Width - frameEdgeWidth;
		sourceRectangle.Y += frameEdgeWidth;
		sourceRectangle.Width = frameEdgeWidth;
		sourceRectangle.Height = frame.Height - 2 * frameEdgeWidth;
		AddDust(gui, window, sourceRectangle, new Point(frame.X + frame.Width - frameEdgeWidth, frame.Y + frameEdgeWidth)).RenderType = renderType;
	}

	public static void AddSteelTexture(GUIManager gui, Window window)
	{
		int i = 0;
		int num = 0;
		for (Rectangle sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle("main_panel_texture"); i < window.Width; i += sourceRectangle.Width)
		{
			for (num = 0; num < window.Height; num += sourceRectangle.Height)
			{
				Image image = new Image(gui);
				image.DebugTag = "main_panel_texture";
				image.SetSkinLocation(SkinState.Normal, sourceRectangle);
				window.Add(image);
				image.Position = new Point(i, num);
				image.ResizeControlToFitImage();
				image.CanHaveFocus = false;
			}
		}
	}

	public virtual void DrawContent(Window sender, SpriteBatch formSpriteBatch)
	{
	}

	public virtual void Update(GameTime elapsed)
	{
		if (The.Sim != null && updateRegulator.IsReady())
		{
			Refresh();
		}
	}

	public virtual void Refresh()
	{
	}
}
