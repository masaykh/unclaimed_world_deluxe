using System;
using System.Collections.Generic;
using InputEventSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WindowSystem;

public class ListBox : UIComponent, IHasText
{
	private ListBoxType type;

	protected Label.LabelType labelType;

	private static int defaultWidth = 200;

	private static int defaultHeight = 150;

	private static int defaultHMargin = 0;

	private static int defaultVMargin = 2;

	private static SpriteFont defaultFont = GUIManager.LCDandHUDFont;

	private static Rectangle defaultSkin = new Rectangle(84, 41, 25, 25);

	private bool isAddingEntries;

	public int scrollBarXOffset;

	public Label.AnimationMode AnimateOnCRTScreen;

	private UIComponent surface;

	private Box viewPort;

	private ScrollBar scrollBar;

	private List<Label> entries;

	private SpriteFont font;

	private Bar highlightBar;

	private Label selectedLabel;

	private int selectedIndex;

	private int hMargin;

	private int vMargin;

	private int entryPaddingHorizontal;

	private int entryPaddingVertical;

	private int highlightPadding;

	private bool canGrowInHeight;

	private bool scrollBarEnabled = true;

	private Color color;

	public const int GapAndScrollbarMain = 25;

	public const int GapAndScrollbarComm = 13;

	public const int GapAndScrollbarLCDHUD = 13;

	public int GapAndScrollBar;

	public static int DefaultWidth
	{
		set
		{
			defaultWidth = value;
		}
	}

	public static int DefaultHeight
	{
		set
		{
			defaultHeight = value;
		}
	}

	public static int DefaultHMargin
	{
		set
		{
			defaultHMargin = value;
		}
	}

	public static int DefaultVMargin
	{
		set
		{
			defaultVMargin = value;
		}
	}

	public static Rectangle DefaultSkin
	{
		set
		{
			defaultSkin = value;
		}
	}

	public ScrollBar ScrollBar => scrollBar;

	public bool ScrollBarEnabled
	{
		get
		{
			return scrollBarEnabled;
		}
		set
		{
			scrollBarEnabled = value;
			if (!value && scrollBar != null)
			{
				Remove(scrollBar);
			}
		}
	}

	public bool CanGrowInHeight
	{
		get
		{
			return canGrowInHeight;
		}
		set
		{
			if (canGrowInHeight != value)
			{
				canGrowInHeight = value;
				scrollBar.Visible = !canGrowInHeight;
				SetGapForScrollbar();
				RefreshMargins();
			}
		}
	}

	public int Count => entries.Count;

	public int SelectedIndex
	{
		get
		{
			return selectedIndex;
		}
		set
		{
			if (value >= 0 && value < entries.Count)
			{
				Select(entries[value], value);
			}
			else if (value == -1)
			{
				Select(null, -1);
			}
		}
	}

	public Label SelectedText
	{
		get
		{
			if (selectedIndex == -1)
			{
				return null;
			}
			return entries[selectedIndex];
		}
		set
		{
			Select(value, FindIndex(value));
		}
	}

	public SpriteFont Font
	{
		set
		{
			font = value;
			scrollBar.ScrollStep = font.LineSpacing;
			RefreshEntries();
		}
	}

	public Color Color
	{
		get
		{
			return color;
		}
		set
		{
			color = value;
			RefreshEntries();
		}
	}

	public Color NormalColor
	{
		set
		{
			Color = value;
		}
	}

	public int HMargin
	{
		get
		{
			return hMargin;
		}
		set
		{
			hMargin = value;
			RefreshMargins();
		}
	}

	public int VMargin
	{
		get
		{
			return vMargin;
		}
		set
		{
			vMargin = value;
			RefreshMargins();
		}
	}

	protected SpriteFont SpriteFont => font;

	public event SelectionChangedHandler SelectedChanged;

	public event SelectionChangedHandler SelectedSame;

	public event Action<UIComponent> MouseSelected;

	public void Init(Label.LabelType type)
	{
		ListBoxType listBoxType = this.type;
		if (listBoxType == ListBoxType.LCDCombo)
		{
			Rectangle sourceRectangle = guiManager.GUISpriteSheet.GetSourceRectangle("basic_dropdown_BG_small");
			viewPort.SetSkinLocation(SkinState.Normal, sourceRectangle);
			entryPaddingHorizontal = 9;
			entryPaddingVertical = 4;
			highlightPadding = 6;
			InitHoverBackground("basic_dropdown_highlight", 8);
		}
		if (labelType != type)
		{
			labelType = type;
			Label.ApplyTextFormat(this, type);
			RefreshEntries();
		}
	}

	public ListBox(GUIManager guiManager, ListBoxType type)
		: base(guiManager)
	{
		this.type = type;
		entries = new List<Label>();
		selectedIndex = -1;
		canGrowInHeight = false;
		base.CanReceiveMouseWheelEvents = true;
		surface = new UIComponent(guiManager);
		viewPort = new Box(guiManager);
		SetGapForScrollbar();
		switch (type)
		{
		case ListBoxType.Comm:
			scrollBar = new ScrollBar(guiManager, ScrollBar.ScrollBarType.CommRoller);
			break;
		case ListBoxType.Main:
			scrollBar = new ScrollBar(guiManager, ScrollBar.ScrollBarType.MainRoller);
			break;
		case ListBoxType.HUDAndLCD:
			scrollBar = new ScrollBar(guiManager, ScrollBar.ScrollBarType.HUD);
			break;
		case ListBoxType.LCD:
			scrollBar = new ScrollBar(guiManager, ScrollBar.ScrollBarType.LCD);
			break;
		default:
			scrollBar = new ScrollBar(guiManager);
			break;
		}
		viewPort.Add(surface);
		Add(viewPort);
		Add(scrollBar);
		surface.CanHaveFocus = false;
		viewPort.CanHaveFocus = false;
		Width = defaultWidth;
		Height = defaultHeight;
		HMargin = defaultHMargin;
		VMargin = defaultVMargin;
		scrollBar.Scroll += OnScroll;
	}

	public void SetDebugTagOnScrollbar(string tag)
	{
		scrollBar.DebugTag = tag;
	}

	private void SetGapForScrollbar()
	{
		if (!scrollBarEnabled || canGrowInHeight)
		{
			GapAndScrollBar = 0;
		}
		else if (type == ListBoxType.Comm)
		{
			GapAndScrollBar = 13;
		}
		else if (type == ListBoxType.Main)
		{
			GapAndScrollBar = 25;
		}
		else if (type == ListBoxType.LCD || type == ListBoxType.HUDAndLCD)
		{
			GapAndScrollBar = 13;
		}
		else
		{
			GapAndScrollBar = 0;
		}
	}

	public override void CleanUp()
	{
		scrollBar.CleanUp();
		base.CleanUp();
	}

	protected override void LoadGraphicsContent(bool loadAllContent)
	{
		if (loadAllContent)
		{
			Font = defaultFont;
		}
		base.LoadGraphicsContent(loadAllContent);
	}

	protected void RefreshMargins()
	{
		viewPort.X = hMargin;
		if (scrollBarEnabled)
		{
			viewPort.Width = Width - hMargin * 2 - GapAndScrollBar;
			SetSurfaceWidthForScrollbar(type, viewPort, surface);
		}
		else
		{
			viewPort.Width = Width - hMargin * 2;
			surface.Width = viewPort.Width;
		}
		SetHighlightWidth();
		viewPort.Y = vMargin;
		viewPort.Height = Height - vMargin * 2;
		if (scrollBarEnabled)
		{
			scrollBar.Viewable = viewPort.Height;
		}
	}

	public void FitToLongestEntry()
	{
		foreach (Label entry in entries)
		{
			_ = entry;
		}
	}

	public static void SetSurfaceWidthForScrollbar(ListBoxType type, UIComponent viewPort, UIComponent surface)
	{
		switch (type)
		{
		case ListBoxType.Main:
			surface.Width = viewPort.Width - 28;
			break;
		case ListBoxType.Comm:
			surface.Width = viewPort.Width - 10;
			break;
		default:
			surface.Width = viewPort.Width;
			break;
		}
	}

	public void RefreshEntries()
	{
		int num = entryPaddingVertical;
		foreach (Label entry in entries)
		{
			entry.Y = num;
			entry.X = entryPaddingHorizontal;
			if (font != null)
			{
				entry.Font = font;
				entry.Height = font.LineSpacing;
			}
			else
			{
				entry.Height = 15;
			}
			entry.NormalColor = color;
			num += entry.Height;
			entry.Width = surface.Width - 2 * entryPaddingHorizontal;
		}
		surface.Height = num + entryPaddingVertical;
		if (scrollBarEnabled)
		{
			scrollBar.MaximumValue = surface.Height;
		}
		if (canGrowInHeight)
		{
			int num2 = surface.Height + vMargin * 2;
			if (num2 != Height)
			{
				Height = num2;
			}
		}
		else if (num > viewPort.Height)
		{
			if (scrollBarEnabled)
			{
				scrollBar.ShowKnob = true;
				scrollBar.Visible = true;
			}
		}
		else if (scrollBarEnabled)
		{
			scrollBar.ShowKnob = false;
			scrollBar.Visible = false;
		}
		surface.Redraw();
	}

	public void BeginAddingEntries()
	{
		isAddingEntries = true;
	}

	public void EndAddingEntries()
	{
		isAddingEntries = false;
		RefreshEntries();
	}

	public Label AddEntry(string text)
	{
		Label label = new Label(base.GUIManager);
		if (AnimateOnCRTScreen == Label.AnimationMode.Line)
		{
			label.SetLineAnimationMode(entries.Count);
		}
		else
		{
			label.AnimateOnCRTScreen = AnimateOnCRTScreen;
		}
		label.Init(labelType);
		label.Text = text;
		label.DebugTag = "comboEntry";
		entries.Add(label);
		surface.Add(label);
		if (!isAddingEntries)
		{
			RefreshEntries();
		}
		return label;
	}

	public void Clear()
	{
		foreach (Label entry in entries)
		{
			surface.Remove(entry);
		}
		entries.Clear();
		RefreshEntries();
	}

	public string GetSelectedText()
	{
		int num = SelectedIndex;
		if (num == -1)
		{
			return null;
		}
		return entries[num].Text;
	}

	protected void Select(Label label, int index)
	{
		if (index == -1)
		{
			if (selectedLabel != null)
			{
				selectedLabel.Color = Color.Black;
				selectedLabel = null;
			}
			selectedIndex = index;
			return;
		}
		if (label != selectedLabel)
		{
			if (selectedLabel != null)
			{
				selectedLabel.Color = Color.Black;
			}
			selectedLabel = label;
			selectedLabel.Color = Color.Gray;
			selectedIndex = index;
			if (this.SelectedChanged != null)
			{
				this.SelectedChanged(this);
			}
		}
		else if (this.SelectedSame != null)
		{
			this.SelectedSame(this);
		}
		if (-(selectedLabel.Y + selectedLabel.Height) < surface.Y - viewPort.Height)
		{
			surface.Y = viewPort.Height - (selectedLabel.Y + selectedLabel.Height);
			scrollBar.Value = -surface.Y;
			viewPort.Redraw();
		}
		else if (-selectedLabel.Y > surface.Y)
		{
			surface.Y = -selectedLabel.Y;
			scrollBar.Value = -surface.Y;
			viewPort.Redraw();
		}
	}

	protected int FindIndex(Label label)
	{
		for (int i = 0; i < entries.Count; i++)
		{
			if (entries[i] == label)
			{
				return i;
			}
		}
		return -1;
	}

	protected void CheckMouseSelect(MouseEventArgs args)
	{
		if (args.Button == MouseButtons.Left && GetMousedEntry(args, out var index, out var mousedLabel))
		{
			Select(mousedLabel, index);
			if (this.MouseSelected != null)
			{
				this.MouseSelected(this);
			}
		}
	}

	protected bool GetMousedEntry(MouseEventArgs args, out int index, out Label mousedLabel)
	{
		mousedLabel = null;
		index = -1;
		foreach (Label entry in entries)
		{
			index++;
			if (entry.CheckCoordinates(args.Position.X, args.Position.Y))
			{
				mousedLabel = entry;
				return true;
			}
		}
		return false;
	}

	private void InitHoverBackground(string backgroundSprite, int spriteEdgeSize)
	{
		highlightBar = new Bar(guiManager);
		highlightBar.EdgeSize = spriteEdgeSize;
		Rectangle sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle(backgroundSprite);
		highlightBar.SetSkinLocation(SkinState.Normal, sourceRectangle);
		highlightBar.Height = sourceRectangle.Height;
		SetHighlightWidth();
		highlightBar.X = entryPaddingHorizontal - highlightPadding;
	}

	private void SetHighlightWidth()
	{
		if (highlightBar != null)
		{
			highlightBar.Width = surface.Width - 2 * entryPaddingHorizontal + 2 * highlightPadding;
		}
	}

	protected void OnScroll(int position)
	{
		surface.Y = -position;
		viewPort.Redraw();
	}

	protected override void OnMouseOut(UIComponent sender, MouseEventArgs args)
	{
		base.OnMouseOut(sender, args);
		if (highlightBar != null)
		{
			surface.Remove(highlightBar);
		}
	}

	protected override void OnMouseMove(MouseEventArgs args)
	{
		base.OnMouseMove(args);
		if (highlightBar != null && GetMousedEntry(args, out var _, out var mousedLabel))
		{
			ShowHighlight(mousedLabel);
		}
	}

	protected override void OnMouseWheelChanged(int wheelChange)
	{
		base.OnMouseWheelChanged(wheelChange);
		scrollBar.Value -= wheelChange;
	}

	private void ShowHighlight(Label mousedEntry)
	{
		surface.Remove(highlightBar);
		surface.Remove(mousedEntry);
		surface.Add(highlightBar);
		surface.Add(mousedEntry);
		ListBoxType listBoxType = type;
		if (listBoxType == ListBoxType.LCDCombo)
		{
			highlightBar.Y = mousedEntry.Y - 4;
		}
		else
		{
			highlightBar.Y = mousedEntry.Y;
		}
	}

	protected override void OnMouseDown(MouseEventArgs args)
	{
		base.OnMouseDown(args);
		if (!canGrowInHeight)
		{
			CheckMouseSelect(args);
		}
	}

	protected override void OnMouseUp(MouseEventArgs args)
	{
		base.OnMouseUp(args);
		if (canGrowInHeight)
		{
			CheckMouseSelect(args);
		}
	}

	public void RemoveEntry(Label item)
	{
		entries.Remove(item);
	}

	protected override void OnResize(UIComponent sender)
	{
		base.OnResize(sender);
		if (scrollBarEnabled)
		{
			scrollBar.X = Width - scrollBar.Width + scrollBarXOffset;
			scrollBar.Height = Height;
		}
		RefreshMargins();
		RefreshEntries();
	}
}
