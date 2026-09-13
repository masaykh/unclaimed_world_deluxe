using System;
using System.Collections.Generic;
using System.Linq;
using InputEventSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WindowSystem;

public class Grid : UIComponent, IKeyedEntryComponent
{
	public enum SelectabilityOptions
	{
		None,
		Single,
		Multiple
	}

	public enum NewRowsInsertion
	{
		First,
		Last
	}

	public enum Sorting
	{
		Ascending,
		Descending
	}

	public delegate void RefreshFunction();

	public NewRowsInsertion InsertNewRows = NewRowsInsertion.Last;

	private bool isOuterGrid = true;

	private ListBoxType type;

	private Label.LabelType labelType;

	private const int itemContentJustifyX = 2;

	private const int itemContentJustifyY = 2;

	private static int defaultWidth = 200;

	private static int defaultHeight = 150;

	private static int defaultHMargin = 0;

	private static int defaultVMargin = 0;

	private static string defaultFont = "Content/Fonts/DefaultFont";

	private static Rectangle defaultSkin = new Rectangle(84, 41, 25, 25);

	public const int ScrollBarAndGapMain = 25;

	public const int GapAndScrollbarComm = 20;

	public const int GapAndScrollbarLCD = 18;

	public const int GapAndScrollbarHUD = 14;

	private bool isAddingEntries;

	public int GapAndScrollBar;

	private Box background;

	private int? backgroundRightMargin;

	public UIComponent surface;

	private UIComponent viewPort;

	private ScrollBar scrollBar;

	private List<UIComponent> entries;

	private Dictionary<object, UIComponent> entriesByKey;

	private SpriteFont font;

	private string fontFileName;

	private UIComponent selectedItem;

	private int selectedIndex;

	private int hMargin;

	private int topMargin;

	private int bottomMargin;

	private int itemHeight = 15;

	private bool canGrowInHeight;

	private Box selectionBox;

	private SelectabilityOptions selectability;

	public bool FixedItemHeights = true;

	private bool scrollBarEnabled = true;

	private Color? color;

	public bool IsOuterGrid
	{
		get
		{
			return isOuterGrid;
		}
		set
		{
			isOuterGrid = value;
			if (!isOuterGrid)
			{
				base.CanReceiveMouseWheelEvents = false;
			}
			RefreshMargins();
		}
	}

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

	public static string DefaultFont
	{
		set
		{
			defaultFont = value;
		}
	}

	public static Rectangle DefaultSkin
	{
		set
		{
			defaultSkin = value;
		}
	}

	public SelectabilityOptions Selectability
	{
		get
		{
			return selectability;
		}
		set
		{
			selectability = value;
			if (selectability == SelectabilityOptions.None)
			{
				base.CanHaveFocus = false;
			}
			else
			{
				base.CanHaveFocus = true;
			}
		}
	}

	public UIComponent SelectedItem => selectedItem;

	public int SurfaceWidth => surface.Width;

	public override int Width
	{
		get
		{
			return base.Width;
		}
		set
		{
			if (base.Width != value)
			{
				base.Width = value;
				RefreshMargins();
			}
		}
	}

	public int ItemHeight
	{
		get
		{
			return itemHeight;
		}
		set
		{
			if (value != itemHeight)
			{
				itemHeight = value;
				RefreshEntries();
			}
		}
	}

	public int RowSpacing { get; set; }

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
			RefreshMargins();
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
			canGrowInHeight = value;
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

	public string Font
	{
		get
		{
			return fontFileName;
		}
		set
		{
			fontFileName = value;
			font = base.GUIManager.ContentManager.Load<SpriteFont>(value);
			scrollBar.ScrollStep = font.LineSpacing;
			RefreshEntries();
		}
	}

	public Color? Color
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
			return topMargin;
		}
		set
		{
			topMargin = value;
			bottomMargin = value;
			RefreshMargins();
		}
	}

	public int BottomMargin
	{
		get
		{
			return bottomMargin;
		}
		set
		{
			bottomMargin = value;
			RefreshMargins();
		}
	}

	public int TopMargin
	{
		get
		{
			return topMargin;
		}
		set
		{
			topMargin = value;
			RefreshMargins();
		}
	}

	public Rectangle Skin
	{
		set
		{
			background.SetSkinLocation(SkinState.Normal, value);
		}
	}

	protected SpriteFont SpriteFont => font;

	public List<UIComponent> Entries
	{
		get
		{
			return entries;
		}
		set
		{
			entries = value;
		}
	}

	public Dictionary<object, UIComponent> EntriesByKey => entriesByKey;

	public event ResizeHandler SurfaceHeightResize;

	public event SelectionChangedHandler SelectedChanged;

	private void ResizeBackground()
	{
		if (background != null)
		{
			background.Height = Height;
			background.Width = SurfaceWidth - (backgroundRightMargin ?? 0);
		}
	}

	public override int Add(UIComponent control)
	{
		return base.Add(control);
	}

	public Grid(GUIManager guiManager, ListBoxType type, Label.LabelType labelType, string backgroundSprite = null, int? backgroundCornerSize = null, int? backgroundRightMargin = null)
		: base(guiManager)
	{
		this.type = type;
		this.labelType = labelType;
		entries = new List<UIComponent>();
		entriesByKey = new Dictionary<object, UIComponent>();
		selectedIndex = -1;
		canGrowInHeight = false;
		Selectability = SelectabilityOptions.None;
		base.CanReceiveMouseWheelEvents = true;
		surface = new UIComponent(guiManager);
		viewPort = new UIComponent(guiManager);
		Rectangle sourceRectangle;
		if (backgroundSprite != null)
		{
			background = new Box(guiManager);
			background.CornerSize = backgroundCornerSize.Value;
			this.backgroundRightMargin = backgroundRightMargin;
			sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle(backgroundSprite);
			background.SetSkinLocation(SkinState.Normal, sourceRectangle);
			Add(background);
		}
		selectionBox = new Box(guiManager);
		sourceRectangle = guiManager.GUISpriteSheet.GetSourceRectangle("lcd_selection");
		selectionBox.SetSkinLocation(SkinState.Normal, sourceRectangle);
		selectionBox.CornerSize = 3;
		selectionBox.CanHaveFocus = false;
		switch (type)
		{
		case ListBoxType.Comm:
			GapAndScrollBar = 20;
			scrollBar = new ScrollBar(guiManager, ScrollBar.ScrollBarType.CommRoller);
			break;
		case ListBoxType.Main:
			GapAndScrollBar = 25;
			scrollBar = new ScrollBar(guiManager, ScrollBar.ScrollBarType.MainRoller);
			break;
		case ListBoxType.HUDAndLCD:
			GapAndScrollBar = 14;
			scrollBar = new ScrollBar(guiManager, ScrollBar.ScrollBarType.HUD);
			break;
		case ListBoxType.LCD:
			GapAndScrollBar = 18;
			scrollBar = new ScrollBar(guiManager, ScrollBar.ScrollBarType.LCD);
			break;
		default:
			scrollBar = new ScrollBar(guiManager);
			break;
		}
		scrollBar.DebugTag = "ScrollBarDebug";
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

	public override void CleanUp()
	{
		scrollBar.CleanUp();
		base.CleanUp();
	}

	protected override void LoadGraphicsContent(bool loadAllContent)
	{
		base.LoadGraphicsContent(loadAllContent);
	}

	public bool GetSelectedKey(out object key)
	{
		return GetKey(SelectedItem, out key);
	}

	public bool GetKey(UIComponent item, out object key)
	{
		foreach (KeyValuePair<object, UIComponent> item2 in entriesByKey)
		{
			if (item2.Value == item)
			{
				key = item2.Key;
				return true;
			}
		}
		key = null;
		return false;
	}

	public void Sort(Func<UIComponent, object> keySelector, Sorting sorting = Sorting.Descending)
	{
		Sort(keySelector, sorting, ref entries, RefreshEntries);
	}

	public static void Sort(Func<UIComponent, object> keySelector, Sorting sorting, ref List<UIComponent> entriesToSort, RefreshFunction refreshFunction)
	{
		if (sorting == Sorting.Descending)
		{
			entriesToSort = entriesToSort.OrderByDescending(keySelector).ToList();
		}
		else
		{
			entriesToSort = entriesToSort.OrderBy(keySelector).ToList();
		}
		refreshFunction?.Invoke();
	}

	public void Sort(Func<UIComponent, object> keySelector1, Sorting sorting1, Func<UIComponent, object> keySelector2, Sorting sorting2)
	{
		Sort(keySelector1, sorting1, keySelector2, sorting2, ref entries, RefreshEntries);
	}

	public static void Sort(Func<UIComponent, object> keySelector1, Sorting sorting1, Func<UIComponent, object> keySelector2, Sorting sorting2, ref List<UIComponent> entriesToSort, RefreshFunction refreshFunction)
	{
		IOrderedEnumerable<UIComponent> source = ((sorting1 != Sorting.Descending) ? entriesToSort.OrderBy(keySelector1) : entriesToSort.OrderByDescending(keySelector1));
		IOrderedEnumerable<UIComponent> source2 = ((sorting2 != Sorting.Descending) ? source.ThenBy(keySelector2) : source.ThenByDescending(keySelector2));
		entriesToSort = source2.ToList();
		refreshFunction?.Invoke();
	}

	public void Sort(Sorting sortType, bool useFirstTag, Func<float, float> transformation = null)
	{
		if (useFirstTag)
		{
			if (transformation != null)
			{
				Sort((UIComponent i) => transformation((float)i.OrderByTag1), sortType);
			}
			else
			{
				Sort((UIComponent i) => (float)i.OrderByTag1, sortType);
			}
		}
		else
		{
			Sort((UIComponent i) => (int)i.OrderByTag2, sortType);
		}
	}

	protected void RefreshMargins()
	{
		viewPort.X = hMargin;
		viewPort.Y = topMargin;
		viewPort.Height = Height - (topMargin + bottomMargin);
		if (IsOuterGrid && scrollBarEnabled)
		{
			viewPort.Width = Width - hMargin * 2 - GapAndScrollBar;
			ListBox.SetSurfaceWidthForScrollbar(type, viewPort, surface);
		}
		else
		{
			viewPort.Width = Width - hMargin * 2;
			surface.Width = viewPort.Width;
		}
		if (scrollBarEnabled)
		{
			scrollBar.Viewable = viewPort.Height;
		}
	}

	public void SetContentWidth(int contentWidth)
	{
		Width = contentWidth + GapAndScrollBar;
	}

	public void RefreshEntries()
	{
		if (isAddingEntries)
		{
			return;
		}
		int num = 0;
		foreach (UIComponent entry in entries)
		{
			entry.Y = num;
			if (FixedItemHeights)
			{
				entry.Height = ItemHeight;
			}
			num += entry.Height;
			num += RowSpacing;
			if (entry.X == 0)
			{
				entry.Width = surface.Width;
			}
		}
		bool flag = false;
		if (surface.Height != num)
		{
			surface.Height = num;
			flag = true;
		}
		if (scrollBarEnabled)
		{
			scrollBar.MaximumValue = surface.Height;
		}
		if (canGrowInHeight)
		{
			int num2 = surface.Height + (topMargin + bottomMargin);
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
		if (flag && this.SurfaceHeightResize != null)
		{
			this.SurfaceHeightResize(this);
		}
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

	public bool TryRemoveEntry(object key)
	{
		if (entriesByKey.ContainsKey(key))
		{
			RemoveEntry(key);
			return true;
		}
		return false;
	}

	public override bool Remove(UIComponent control)
	{
		return base.Remove(control);
	}

	public void RemoveEntry(object key)
	{
		UIComponent item = entriesByKey[key];
		RemoveEntry(key, item);
	}

	public void RemoveEntry(object key, UIComponent item)
	{
		entries.Remove(item);
		surface.Remove(item);
		if (key != null)
		{
			entriesByKey.Remove(key);
		}
		if (!FixedItemHeights)
		{
			item.HeightResize -= item_Resize;
		}
		RefreshEntries();
	}

	public UIComponent AddEntry(object key, string entry, bool useLineBreaks = false, Color? entryColor = null, int? leftMargin = null)
	{
		UIComponent uIComponent = new UIComponent(guiManager);
		Color? color = entryColor ?? this.color;
		if (useLineBreaks)
		{
			TextArea textArea = new TextArea(guiManager, type);
			textArea.HMargin = 0;
			textArea.VMargin = 0;
			textArea.CanGrowInHeight = true;
			textArea.Font = font;
			if (color.HasValue)
			{
				textArea.Color = color.Value;
			}
			textArea.Text = entry;
			uIComponent.Add(textArea);
		}
		else
		{
			Label label = new Label(guiManager);
			label.Text = entry;
			InitLabel(label);
			uIComponent.Add(label);
			JustifyItemContent(label);
			label.X = leftMargin ?? 0;
			if (color.HasValue)
			{
				label.NormalColor = color.Value;
			}
		}
		AddEntry(key, uIComponent);
		return uIComponent;
	}

	private void InitLabel(Label label)
	{
		label.Init(labelType);
		label.ID = DataControlID.Status;
	}

	public Hyperlink AddHyperLinkEntry(object key, string entry, uint entityID, int marginX = 0)
	{
		UIComponent newEntry = new UIComponent(guiManager);
		return AddHyperLinkEntry(key, entry, entityID, newEntry);
	}

	public Hyperlink AddHyperLinkEntry(object key, string entry, uint entityID, UIComponent newEntry, int marginX = 0)
	{
		Hyperlink hyperlink = new Hyperlink(guiManager);
		hyperlink.Text = entry;
		hyperlink.TargetEntityID = entityID;
		hyperlink.RenderType = base.RenderType;
		newEntry.Add(hyperlink);
		JustifyItemContent(hyperlink);
		hyperlink.X = marginX;
		AddEntry(key, newEntry);
		return hyperlink;
	}

	public void AddEntry(object key, string entry1, int xPositionColumn2, string entry2)
	{
		AddEntry(key, null, entry1, xPositionColumn2, entry2);
	}

	public UIComponent AddEntryRightJustifyValue(object key, Rectangle? iconRect, int? iconXPos, int? leadingTextXPos, string entry1, int paddingRight, string entry2, string valueTooltip = null, string captionTooltip = null)
	{
		UIComponent uIComponent = new UIComponent(guiManager);
		if (iconRect.HasValue)
		{
			Image image = new Image(guiManager);
			image.SetSkinLocation(SkinState.Normal, iconRect.Value);
			image.Position = new Point(0, 0);
			image.ResizeControlToFitImage();
			uIComponent.Add(image);
			if (iconXPos.HasValue)
			{
				image.X += iconXPos.Value;
			}
		}
		Label label = new Label(guiManager);
		label.Text = entry1;
		label.ToolTip = captionTooltip;
		label.Init(labelType);
		uIComponent.Add(label);
		if (leadingTextXPos.HasValue)
		{
			label.X += leadingTextXPos.Value;
		}
		JustifyItemContent(label);
		label = new Label(guiManager);
		label.Text = entry2;
		label.Init(labelType);
		label.ID = DataControlID.Value;
		int num = (label.X = Width - paddingRight - label.TextWidth);
		int x = num;
		label.X = x;
		label.Y += 2;
		label.Name = "value";
		label.ToolTip = valueTooltip;
		uIComponent.Add(label);
		AddEntry(key, uIComponent);
		return uIComponent;
	}

	public void AddEntryWithCaptionTwoValuesAndButton(object key, int? leadingTextXPos, string caption, int paddingRight, string value1, string value2, ImageButtonType imageButtonType, ClickHandler clickHandler, EventArgs eventArgs)
	{
		UIComponent uIComponent = new UIComponent(guiManager);
		Label label = new Label(guiManager);
		label.Text = caption;
		label.Init(labelType);
		uIComponent.Add(label);
		if (leadingTextXPos.HasValue)
		{
			label.X += leadingTextXPos.Value;
		}
		JustifyItemContent(label);
		int textWidth = label.TextWidth;
		label = new Label(guiManager);
		label.Text = value1;
		label.Init(labelType);
		label.Name = "value1";
		uIComponent.Add(label);
		label.X = textWidth + 6;
		_ = label.X;
		label = new Label(guiManager);
		label.Text = value2;
		label.Init(labelType);
		label.Name = "value2";
		uIComponent.Add(label);
		label.X = 100;
		int x = label.X + label.TextWidth + 4;
		ImageButton imageButton = new ImageButton(guiManager);
		imageButton.X = x;
		imageButton.Click += clickHandler;
		imageButton.Name = "button";
		uIComponent.Add(imageButton);
		imageButton.EventArgs = eventArgs;
		imageButton.Init(imageButtonType);
		AddEntry(key, uIComponent);
	}

	public void AddEntryAndButton(object key, int? leadingTextXPos, string entry1, int paddingRight, string entry2, ClickHandler clickHandler, EventArgs eventArgs)
	{
		UIComponent uIComponent = new UIComponent(guiManager);
		Label label = new Label(guiManager);
		label.Text = entry1 + " " + entry2;
		label.Init(labelType);
		label.Name = "captionAndValue";
		uIComponent.Add(label);
		if (leadingTextXPos.HasValue)
		{
			label.X += leadingTextXPos.Value;
		}
		JustifyItemContent(label);
		int x = Width - paddingRight - label.TextWidth;
		ImageButton imageButton = new ImageButton(guiManager);
		imageButton.X = x;
		imageButton.Click += clickHandler;
		uIComponent.Add(imageButton);
		imageButton.EventArgs = eventArgs;
		imageButton.Init(ImageButtonType.HUDArrowRight);
		AddEntry(key, uIComponent);
	}

	public void AddEntry(object key, int? xPosColumn1, string entry1, int xPositionColumn2, string entry2)
	{
		UIComponent uIComponent = new UIComponent(guiManager);
		Label label = new Label(guiManager);
		label.Text = entry1;
		label.Init(labelType);
		uIComponent.Add(label);
		if (xPosColumn1.HasValue)
		{
			label.X += xPosColumn1.Value;
		}
		JustifyItemContent(label);
		label = new Label(guiManager);
		label.Text = entry2;
		label.Init(labelType);
		label.X = xPositionColumn2;
		label.Y += 2;
		label.Name = "value";
		uIComponent.Add(label);
		AddEntry(key, uIComponent);
	}

	private void JustifyItemContent(UIComponent content)
	{
	}

	public void AddEntryWithIcon(object key, Rectangle iconRect, int margin1, string entry1, int margin2, string entry2)
	{
		UIComponent uIComponent = new UIComponent(guiManager);
		Image image = new Image(guiManager);
		image.SetSkinLocation(SkinState.Normal, iconRect);
		image.Position = new Point(0, 0);
		image.ResizeControlToFitImage();
		uIComponent.Add(image);
		Label label = new Label(guiManager);
		label.Text = entry1;
		label.Init(labelType);
		label.X = margin1;
		uIComponent.Add(label);
		label.Y += 2;
		label = new Label(guiManager);
		label.Text = entry2;
		label.Init(labelType);
		label.X = margin2;
		label.Y += 2;
		uIComponent.Add(label);
		AddEntry(key, uIComponent);
	}

	public void AddEntry(object key, UIComponent item, int? index = null)
	{
		if (selectability != SelectabilityOptions.None)
		{
			item.CanHaveFocus = false;
		}
		else
		{
			item.CanHaveFocus = base.CanHaveFocus;
		}
		InitNestedGrids(item);
		item.Tag1 = key;
		if (!index.HasValue)
		{
			if (InsertNewRows == NewRowsInsertion.Last)
			{
				entries.Add(item);
			}
			else
			{
				entries.Insert(0, item);
			}
			surface.Add(item);
		}
		else
		{
			entries.Insert(index.Value, item);
			surface.Insert(item, index.Value);
		}
		if (key != null)
		{
			entriesByKey.Add(key, item);
		}
		item.RenderType = RenderType;
		if (FixedItemHeights)
		{
			item.Height = ItemHeight;
		}
		int num = item.Height;
		foreach (UIComponent control in item.Controls)
		{
			if (FixedItemHeights)
			{
				control.Height = ItemHeight;
			}
			else
			{
				num = Math.Max(num, control.Height);
			}
			if (control is Label label)
			{
				if (font != null)
				{
					label.Font = font;
				}
				if (color.HasValue)
				{
					label.NormalColor = color.Value;
				}
			}
			else if (control is Hyperlink hyperlink)
			{
				if (font != null)
				{
					hyperlink.Font = font;
				}
				if (color.HasValue)
				{
					hyperlink.LabelColor = color.Value;
				}
			}
		}
		if (!FixedItemHeights)
		{
			item.Height = num;
			item.HeightResize += item_Resize;
		}
		RefreshEntries();
	}

	private static void InitNestedGrids(UIComponent item)
	{
		item.CanReceiveMouseWheelEvents = false;
		if (item is Grid grid)
		{
			grid.IsOuterGrid = false;
		}
		List<Grid> foundChildren = null;
		item.FindChildOfType(null, ref foundChildren);
		if (foundChildren != null)
		{
			foreach (Grid item2 in foundChildren)
			{
				item2.CanReceiveMouseWheelEvents = false;
				item2.IsOuterGrid = false;
			}
		}
		List<ListBox> foundChildren2 = null;
		item.FindChildOfType(null, ref foundChildren2);
		if (foundChildren2 == null)
		{
			return;
		}
		foreach (ListBox item3 in foundChildren2)
		{
			item3.CanReceiveMouseWheelEvents = false;
		}
	}

	public void DeleteEntries<T>(Predicate<T> exists)
	{
		DeleteEntries(this, exists);
	}

	public static void DeleteEntries<T>(IKeyedEntryComponent entryComponent, Predicate<T> exists)
	{
		for (int num = entryComponent.Entries.Count - 1; num >= 0; num--)
		{
			UIComponent uIComponent = entryComponent.Entries[num];
			T obj = (T)uIComponent.Tag1;
			if (!exists(obj))
			{
				entryComponent.RemoveEntry(uIComponent.Tag1, uIComponent);
			}
		}
	}

	public static void DeleteEntriesWithMixedKeyTypes(IKeyedEntryComponent entryComponent, Predicate<object> exists)
	{
		for (int num = entryComponent.Entries.Count - 1; num >= 0; num--)
		{
			UIComponent uIComponent = entryComponent.Entries[num];
			object tag = uIComponent.Tag1;
			if (!exists(tag))
			{
				entryComponent.RemoveEntry(uIComponent.Tag1, uIComponent);
			}
		}
	}

	public void DeleteEntriesWithNullableKey<T>(Predicate<T> exists) where T : class
	{
		for (int num = Entries.Count - 1; num >= 0; num--)
		{
			UIComponent uIComponent = Entries[num];
			T obj = uIComponent.Tag1 as T;
			if (!exists(obj))
			{
				RemoveEntry(uIComponent.Tag1, uIComponent);
			}
		}
	}

	private void item_Resize(UIComponent sender)
	{
		RefreshEntries();
	}

	public int GetIndex(UIComponent entry)
	{
		return Entries.IndexOf(entry);
	}

	public int GetIndexByKey(object key)
	{
		return Entries.IndexOf(entriesByKey[key]);
	}

	public void Clear()
	{
		foreach (UIComponent entry in entries)
		{
			surface.Remove(entry);
		}
		entries.Clear();
		entriesByKey.Clear();
		RefreshEntries();
	}

	public void Deselect()
	{
	}

	protected void Select(UIComponent item, int index)
	{
		SelectInternalNoEvent(item, index);
		if (this.SelectedChanged != null)
		{
			this.SelectedChanged(this);
		}
	}

	public void PublicSetSelectionNoEvent(string itemString)
	{
		foreach (KeyValuePair<object, UIComponent> item in entriesByKey)
		{
			if (item.Key is string text && text == itemString)
			{
				selectedItem = item.Value;
				SelectInternalNoEvent(item.Value, 0);
				break;
			}
		}
	}

	private void SelectInternalNoEvent(UIComponent item, int index)
	{
		if (index == -1)
		{
			if (selectedItem != null)
			{
				selectedItem.Remove(selectionBox);
			}
			selectedItem = item;
			selectedIndex = index;
			List<Grid> foundChildren = null;
			FindChildOfType(this, ref foundChildren);
			if (foundChildren == null)
			{
				return;
			}
			{
				foreach (Grid item2 in foundChildren)
				{
					item2.SelectedIndex = -1;
				}
				return;
			}
		}
		if (item != selectedItem)
		{
			if (selectedItem != null)
			{
				selectedItem.Remove(selectionBox);
			}
			if (item != null)
			{
				DeselectItemsInSiblingGrids();
			}
			selectedItem = item;
			selectedIndex = index;
		}
		selectedItem.Insert(selectionBox, 0);
		selectionBox.Width = selectedItem.Width - hMargin;
		selectionBox.Height = selectedItem.Height;
		ScrollToItem(selectedItem);
	}

	private void DeselectItemsInSiblingGrids()
	{
		UIComponent uIComponent = FindParentOfType(typeof(Grid));
		if (uIComponent == null)
		{
			return;
		}
		List<Grid> foundChildren = null;
		uIComponent.FindChildOfType(this, ref foundChildren);
		if (foundChildren == null)
		{
			return;
		}
		foreach (Grid item in foundChildren)
		{
			item.SelectedIndex = -1;
		}
	}

	public bool TryGetEntry(object key, out UIComponent item)
	{
		return entriesByKey.TryGetValue(key, out item);
	}

	protected void CheckMouseSelect(MouseEventArgs args)
	{
		if (args.Button != MouseButtons.Left)
		{
			return;
		}
		int num = -1;
		foreach (UIComponent entry in entries)
		{
			num++;
			if (entry.CheckCoordinates(args.Position.X, args.Position.Y))
			{
				Select(entry, num);
				break;
			}
		}
	}

	public void CenterChildHorizontallyInViewport(UIComponent childControl)
	{
		childControl.X = (viewPort.Width - childControl.Width) / 2;
	}

	public void ScrollToIndex(int index)
	{
		if (index != -1 && entries.Count > 0)
		{
			UIComponent itemToScrollTo = entries[index];
			ScrollToItem(itemToScrollTo);
		}
	}

	public void ScrollToItem(UIComponent itemToScrollTo)
	{
		if (-(itemToScrollTo.Y + itemToScrollTo.Height) < surface.Y - viewPort.Height)
		{
			surface.Y = viewPort.Height - (itemToScrollTo.Y + itemToScrollTo.Height);
			scrollBar.Value = -surface.Y;
			viewPort.Redraw();
		}
		else if (-itemToScrollTo.Y > surface.Y)
		{
			surface.Y = -itemToScrollTo.Y;
			scrollBar.Value = -surface.Y;
			viewPort.Redraw();
		}
	}

	public bool ScrollBarIsAtEnd()
	{
		return scrollBar.IsAtEnd();
	}

	public bool ScrollBarIsAtTop()
	{
		return scrollBar.IsAtTop();
	}

	protected void OnScroll(int position)
	{
		surface.Y = -position;
		viewPort.Redraw();
	}

	protected override void OnMouseDown(MouseEventArgs args)
	{
		base.OnMouseDown(args);
		if (Selectability != SelectabilityOptions.None)
		{
			CheckMouseSelect(args);
		}
	}

	protected override void OnMouseWheelChanged(int wheelChange)
	{
		base.OnMouseWheelChanged(wheelChange);
		scrollBar.Value -= wheelChange;
	}

	protected override void OnResize(UIComponent sender)
	{
		base.OnResize(sender);
		if (scrollBarEnabled)
		{
			scrollBar.X = Width - scrollBar.Width;
			scrollBar.Height = Height;
		}
		RefreshMargins();
		RefreshEntries();
		ResizeBackground();
	}

	public bool CanProcessEntryData(string entryTerm, string entryIconName, float? normalizedValue)
	{
		if (entryTerm != null || normalizedValue.HasValue)
		{
			return true;
		}
		return false;
	}

	public UIComponent AddEntry(object key, string term, string caption, Action<UIComponent, bool> tooltipDisplayedCallback, string tooltipActivationProperty, bool? disableExpiry, int? tooltipWidth, string entryIconName, Color? iconColor, Color? termColor, int? orderingNumber, float? entryValue, bool showBar = false, float? normalizedValue1 = null, float? normalizedValue2 = null, int? barWidth = null, bool clickable = false, uint? entityID = null, UIComponent customComponent = null, int? height = null, int? paddingLeft = null, bool useIconBackground = false)
	{
		UIComponent uIComponent = null;
		if (CanProcessEntryData(term ?? caption, entryIconName, normalizedValue1))
		{
			uIComponent = new UIComponent(guiManager);
			uIComponent.OrderByTag2 = orderingNumber ?? 0;
			Hyperlink hyperlink = null;
			Label label = null;
			if (caption != null)
			{
				if (!clickable)
				{
					label = new Label(guiManager);
					label.Init(labelType);
					label.ID = DataControlID.Caption;
					uIComponent.Add(label);
				}
				else
				{
					hyperlink = new Hyperlink(guiManager);
					hyperlink.Text = caption;
					hyperlink.TargetEntityID = entityID;
					hyperlink.RenderType = base.RenderType;
					hyperlink.NormalColor = UIComponent.LCDNormal;
					uIComponent.Add(hyperlink);
					hyperlink.ID = DataControlID.Caption;
				}
			}
			Icon icon = null;
			if (useIconBackground && entryIconName != null)
			{
				icon = new Icon(base.GUIManager);
				icon.ID = DataControlID.StatusIconBackground;
				Rectangle sourceRectangle = guiManager.GUISpriteSheet.GetSourceRectangle("lcd_icon_circleBG");
				icon.SetSkinLocation(SkinState.Normal, sourceRectangle);
				icon.ResizeControlToFitImage();
				uIComponent.Add(icon);
			}
			Icon icon2 = null;
			if (entryIconName != null)
			{
				icon2 = new Icon(base.GUIManager);
				icon2.ID = DataControlID.StatusIcon;
				uIComponent.Add(icon2);
			}
			Label label2 = null;
			Hyperlink hyperlink2 = null;
			if (term != null || normalizedValue1.HasValue || showBar)
			{
				if (showBar || normalizedValue1.HasValue)
				{
					FillableBar fillableBar = (normalizedValue2.HasValue ? new FillableBar(base.GUIManager, FillableBar.FillableBarType.LCDIndicator)
					{
						ShowMaxValueLabelAtEnd = false
					} : new FillableBar(base.GUIManager, FillableBar.FillableBarType.ProgressBar)
					{
						ShowMaxValueLabelAtEnd = false
					});
					fillableBar.ID = DataControlID.Status;
					fillableBar.Width = barWidth ?? 40;
					fillableBar.MaxValue = 40;
					uIComponent.Add(fillableBar);
					SetTermTooltipSettings(tooltipActivationProperty, disableExpiry, tooltipWidth, entityID, tooltipDisplayedCallback, fillableBar);
				}
				else if (customComponent != null)
				{
					uIComponent.Add(customComponent);
				}
				else if (!clickable || hyperlink != null)
				{
					label2 = new Label(guiManager);
					label2.Init(labelType);
					label2.ID = DataControlID.Status;
					uIComponent.Add(label2);
					SetTermTooltipSettings(tooltipActivationProperty, disableExpiry, tooltipWidth, entityID, tooltipDisplayedCallback, label2);
				}
				else
				{
					hyperlink2 = new Hyperlink(guiManager);
					hyperlink2.Text = term;
					hyperlink2.TargetEntityID = entityID;
					hyperlink2.RenderType = base.RenderType;
					uIComponent.Add(hyperlink2);
					hyperlink2.ID = DataControlID.Status;
				}
			}
			AddEntry(key, uIComponent);
			if (height.HasValue)
			{
				uIComponent.Height = height.Value;
			}
			if (icon != null)
			{
				uIComponent.CenterChildVertically(icon);
			}
		}
		return uIComponent;
	}

	private static void SetTermTooltipSettings(string tooltipActivationProperty, bool? disableExpiry, int? tooltipWidth, uint? entityID, Action<UIComponent, bool> tooltipDisplayedCallback, UIComponent control)
	{
		control.TooltipDisplayChange += tooltipDisplayedCallback;
		if (disableExpiry.HasValue)
		{
			control.TooltipExpires = !disableExpiry.Value;
		}
		if (tooltipWidth.HasValue)
		{
			control.TooltipWidth = tooltipWidth.Value;
		}
		if (entityID.HasValue && tooltipActivationProperty != null)
		{
			control.Tag2 = new Tuple<uint, string>(entityID.Value, tooltipActivationProperty);
		}
	}

	public UIComponent AddGroup(object key, bool hasBorder, int? marginLeft = null)
	{
		Grid grid = ((!hasBorder) ? new Grid(guiManager, type, labelType) : new Grid(guiManager, type, labelType, "basic_dropdown_BG_tiny", 5, 0));
		grid.FixedItemHeights = false;
		grid.RenderType = RenderType.CRTAndLCD;
		grid.Font = GUIManager.LCDandHUDBodyFontPath;
		grid.Width = Width - (marginLeft ?? 0);
		grid.Height = 0;
		grid.X = marginLeft ?? 0;
		grid.IsOuterGrid = false;
		grid.CanGrowInHeight = true;
		grid.ScrollBarEnabled = false;
		grid.Selectability = SelectabilityOptions.None;
		AddEntry(key, grid);
		return grid;
	}

	public void UpdateEntry(UIComponent entry, string term, string caption, string termTooltip, string captionTooltip, string iconName, Color? iconColor, Color? termColor, float? entryValue, float? normalizedValue1, float? normalizedValue2, int? paddingLeft, int? paddingRight = null, bool showIconBackground = false, bool showFaded = false, bool rightAdjustTerm = false)
	{
		if (entryValue.HasValue)
		{
			entry.OrderByTag1 = entryValue.Value;
		}
		else
		{
			entry.OrderByTag1 = -1f;
		}
		UIComponent uIComponent = entry.FindChildById(DataControlID.StatusIcon);
		UIComponent uIComponent2 = entry.FindChildById(DataControlID.StatusIconBackground);
		if (uIComponent != null)
		{
			Icon icon = uIComponent as Icon;
			if (iconName != null)
			{
				Rectangle sourceRectangle = guiManager.GUISpriteSheet.GetSourceRectangle(iconName);
				icon.SetSkinLocation(SkinState.Normal, sourceRectangle);
				icon.ResizeControlToFitImage();
				icon.Visible = true;
				icon.Color = iconColor;
				if (showIconBackground && uIComponent2 != null)
				{
					uIComponent2.Visible = true;
				}
			}
			else
			{
				icon.Visible = false;
				if (uIComponent2 != null)
				{
					uIComponent2.Visible = false;
				}
			}
		}
		UIComponent uIComponent3 = entry.FindChildById(DataControlID.Caption);
		if (uIComponent3 != null)
		{
			if (uIComponent3 is Label label)
			{
				label.Text = caption ?? "";
			}
			if (uIComponent3 is Hyperlink hyperlink)
			{
				hyperlink.Text = caption;
			}
			uIComponent3.ToolTip = captionTooltip;
		}
		UIComponent uIComponent4 = entry.FindChildById(DataControlID.Status);
		if (uIComponent4 != null)
		{
			if (uIComponent4 is Label label2)
			{
				label2.Text = term;
				if (termColor.HasValue)
				{
					label2.NormalColor = termColor.Value;
				}
			}
			if (uIComponent4 is Hyperlink hyperlink2)
			{
				hyperlink2.Text = term;
			}
			if (uIComponent4 is FillableBar fillableBar)
			{
				if (!normalizedValue1.HasValue)
				{
					fillableBar.Visible = false;
				}
				else
				{
					fillableBar.Visible = true;
					fillableBar.Value = (int)(normalizedValue1 * (float)fillableBar.MaxValue).Value;
					if (normalizedValue2.HasValue)
					{
						fillableBar.SliderValue = (int)(normalizedValue2 * (float)fillableBar.MaxValue).Value;
						fillableBar.UpdateSliderPosition();
					}
					if (termColor.HasValue)
					{
						fillableBar.Color = termColor.Value;
					}
				}
			}
			termTooltip?.Contains("Well rested");
			uIComponent4.ToolTip = termTooltip;
		}
		int num = paddingLeft ?? 0;
		if (uIComponent != null && uIComponent.Visible)
		{
			if (uIComponent2 != null && showIconBackground)
			{
				uIComponent2.X = num + 1;
				uIComponent2.CenterChildHorizontally(uIComponent);
				uIComponent.X += uIComponent2.X;
				num = Math.Max(uIComponent.Right, uIComponent2.Right);
			}
			else
			{
				uIComponent.X = num;
				num = uIComponent.Right;
			}
		}
		if (uIComponent3 != null)
		{
			uIComponent3.X = num;
			num = uIComponent3.Right;
		}
		if (uIComponent4 != null)
		{
			if (rightAdjustTerm)
			{
				uIComponent4.AlignRight((SurfaceWidth - paddingRight) ?? 0);
				if (uIComponent3 != null)
				{
					uIComponent3.MaxWidth = uIComponent4.X - uIComponent3.X - 4;
				}
			}
			else
			{
				uIComponent4.X = num;
			}
		}
		if (uIComponent3 != null)
		{
			entry.CenterChildVertically(uIComponent3);
		}
		if (uIComponent != null)
		{
			entry.CenterChildVertically(uIComponent);
		}
		if (uIComponent4 != null)
		{
			entry.CenterChildVertically(uIComponent4);
		}
	}

	public bool AddSeparator(object key, int? orderingNumber, float? entryValue)
	{
		Label label = new Label(guiManager);
		AddEntry(key, label);
		label.OrderByTag2 = orderingNumber.Value;
		label.OrderByTag1 = entryValue.Value;
		return true;
	}

	public bool AddSubHeader(string subHeaderName, int? orderingNumber, float? entryValue)
	{
		Label label = new Label(guiManager);
		label.Text = subHeaderName;
		label.OrderByTag2 = orderingNumber.Value;
		label.OrderByTag1 = entryValue.Value;
		_ = (float)label.OrderByTag1;
		InitLabel(label);
		AddEntry(subHeaderName, label);
		return true;
	}

	public object GetKeyFromIndex(int index)
	{
		GetKey(entries[index], out var key);
		return key;
	}

	public bool CapNoOfEntries(int maxItems)
	{
		int num = Entries.Count - maxItems;
		bool result = false;
		while (num > 0)
		{
			RemoveEntry(entries[entries.Count - 1].Tag1);
			num--;
			result = true;
		}
		return result;
	}

	public void CapNoOfEntries()
	{
	}
}
