using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WindowSystem;

public class HorizontalList : UIComponent, IKeyedEntryComponent
{
	private static int defaultWidth = 200;

	private static int defaultHeight = 150;

	private static int defaultHMargin = 5;

	private static int defaultVMargin = 0;

	private static string defaultFont = GUIManager.LCDandHUDBodyFontPath;

	private static Rectangle defaultSkin = new Rectangle(84, 41, 25, 25);

	private bool isAddingEntries;

	private int? MaxNumberOfEntries;

	public Label.AnimationMode AnimateOnCRTScreen;

	private UIComponent surface;

	private UIComponent viewPort;

	private List<UIComponent> entries;

	private Dictionary<object, UIComponent> entriesByKey;

	private SpriteFont font;

	private string fontFileName;

	private List<Icon> poolOfIcons;

	private int hMargin;

	private int vMargin;

	private Color color;

	public int HorizontalSpacing;

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

	public int Count => Entries.Count;

	public string Font
	{
		set
		{
			fontFileName = value;
			font = base.GUIManager.ContentManager.Load<SpriteFont>(value);
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

	public bool CenterItemsVertically { get; set; }

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

	public override int? MaxHeight
	{
		get
		{
			return base.MaxHeight;
		}
		set
		{
			base.MaxHeight = value;
			surface.MaxHeight = value;
		}
	}

	public override int MinHeight
	{
		get
		{
			return base.MinHeight;
		}
		set
		{
			base.MinHeight = value;
			surface.MinHeight = value;
		}
	}

	public HorizontalList(GUIManager guiManager, int? maxNumberOfEntries = null)
		: base(guiManager)
	{
		MaxNumberOfEntries = maxNumberOfEntries;
		Entries = new List<UIComponent>();
		entriesByKey = new Dictionary<object, UIComponent>();
		poolOfIcons = new List<Icon>();
		surface = new UIComponent(guiManager);
		viewPort = new UIComponent(guiManager);
		viewPort.Add(surface);
		Add(viewPort);
		surface.CanHaveFocus = false;
		viewPort.CanHaveFocus = false;
		Width = defaultWidth;
		Height = defaultHeight;
		HMargin = defaultHMargin;
		VMargin = defaultVMargin;
		surface.Height = 0;
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
		viewPort.X = 0;
		viewPort.Width = Width;
		viewPort.Y = vMargin;
		viewPort.Height = Height - vMargin * 2;
		SetSurfaceWidth(viewPort, surface);
	}

	public static void SetSurfaceWidth(UIComponent viewPort, UIComponent surface)
	{
		surface.Width = viewPort.Width;
	}

	public void RefreshEntries()
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 1;
		bool flag = false;
		int num6 = 0;
		foreach (UIComponent entry in Entries)
		{
			num6++;
			if (num6 == Entries.Count)
			{
				flag = true;
			}
			if (entry is Label label)
			{
				if (font != null)
				{
					label.Font = font;
					label.Height = font.LineSpacing;
				}
				else
				{
					label.Height = 15;
				}
				label.NormalColor = color;
				label.Width = surface.Width;
			}
			entry.X = num;
			num = ComputeItemXPosition(num, entry, !flag);
			if (base.MaxWidth.HasValue && num > base.MaxWidth)
			{
				num = 0;
				num2 += entry.Height;
				num4 = base.MaxWidth.Value;
				num5++;
			}
			if (num > num4)
			{
				num4 = num;
			}
			if (entry.Height > num3)
			{
				num3 = entry.Height;
			}
		}
		Width = num4;
		num3 += num2;
		surface.Height = num3;
		int num7 = surface.Height + vMargin * 2;
		if (num7 != Height)
		{
			Height = num7;
		}
		num = 0;
		num2 = 0;
		int num8 = 0;
		int num9 = Height / num5;
		num6 = 0;
		flag = false;
		foreach (UIComponent entry2 in Entries)
		{
			num6++;
			if (num6 == Entries.Count)
			{
				flag = true;
			}
			if (CenterItemsVertically)
			{
				entry2.CenterThisVertically((int)(((float)num8 + 0.5f) * (float)num9));
			}
			else
			{
				entry2.Y = num8 * num9;
			}
			entry2.X = num;
			num = ComputeItemXPosition(num, entry2, !flag);
			if (base.MaxWidth.HasValue && num > base.MaxWidth)
			{
				num = 0;
				num8++;
				entry2.X = num;
				entry2.Y = num8 * entry2.Height;
				num = entry2.X + entry2.Width;
				Height = entry2.Bottom;
				surface.Height = entry2.Bottom;
			}
		}
		surface.Redraw();
	}

	private int ComputeItemXPosition(int x, UIComponent component, bool addHorizontalSpacing)
	{
		int num = HorizontalSpacing;
		if (component is FillableBar)
		{
			num += 4;
		}
		x += component.Width;
		if (addHorizontalSpacing)
		{
			x += num;
		}
		return x;
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

	public void Sort(Grid.Sorting sortType, bool useFirstTag, Func<float, float> transformation = null)
	{
		if (useFirstTag)
		{
			if (transformation != null)
			{
				Grid.Sort((UIComponent i) => transformation((float)i.OrderByTag1), sortType, ref entries, null);
			}
			else
			{
				Grid.Sort((UIComponent i) => (float)i.OrderByTag1, sortType, ref entries, null);
			}
		}
		else
		{
			Grid.Sort((UIComponent i) => (int)i.OrderByTag2, sortType, ref entries, null);
		}
	}

	public void CapNoOfEntries()
	{
		if (MaxNumberOfEntries.HasValue)
		{
			while (Entries.Count > MaxNumberOfEntries.Value)
			{
				int index = Entries.Count - 1;
				TryRemoveEntry((string)Entries[index].Tag1);
			}
		}
	}

	public void Clear()
	{
		foreach (UIComponent entry2 in Entries)
		{
			surface.Remove(entry2);
			if (entry2 is Icon entry)
			{
				ReturnEntryToPool(entry);
			}
		}
		Entries.Clear();
		entriesByKey.Clear();
		RefreshEntries();
	}

	protected int FindIndex(UIComponent component)
	{
		for (int i = 0; i < Entries.Count; i++)
		{
			if (Entries[i] == component)
			{
				return i;
			}
		}
		return -1;
	}

	protected override void OnResize(UIComponent sender)
	{
		base.OnResize(sender);
		RefreshMargins();
	}

	public void StartAddingData()
	{
		BeginAddingEntries();
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

	public void DeleteEntries<T>(Predicate<T> exists)
	{
		Grid.DeleteEntries(this, exists);
	}

	public void RemoveEntry(object key)
	{
		UIComponent entry = entriesByKey[key];
		RemoveEntry(key, entry);
	}

	public void RemoveEntry(object key, UIComponent entry)
	{
		UIComponent uIComponent = entriesByKey[key];
		surface.Remove(uIComponent);
		entriesByKey.Remove(key);
		Entries.Remove(entry);
		if (uIComponent is Icon entry2)
		{
			ReturnEntryToPool(entry2);
		}
	}

	private void ReturnEntryToPool(Icon entry)
	{
		poolOfIcons.Add(entry);
	}

	public bool TryGetEntry(object key, out UIComponent entry)
	{
		return entriesByKey.TryGetValue(key, out entry);
	}

	public void AddEntry(object key, UIComponent newEntry)
	{
		newEntry.CanHaveFocus = base.CanHaveFocus;
		Entries.Add(newEntry);
		surface.Add(newEntry);
		entriesByKey.Add(key, newEntry);
		newEntry.Tag1 = key;
		if (!isAddingEntries)
		{
			RefreshEntries();
		}
	}

	public UIComponent AddGroup(object key, bool hasBorder, int? marginLeft = null)
	{
		throw new Exception("Groups are not supported by horizontal list.");
	}

	public bool CanProcessEntryData(string entryTerm, string entryIconName, float? normalizedValue)
	{
		return CanProcessEntryDataStatic(entryTerm, entryIconName, normalizedValue);
	}

	public static bool CanProcessEntryDataStatic(string entryTerm, string entryIconName, float? normalizedValue)
	{
		if (entryIconName == null && !normalizedValue.HasValue)
		{
			return false;
		}
		return true;
	}

	public UIComponent AddEntry(object key, string entryTerm, string caption, Action<UIComponent, bool> tooltipDisplayedCallback, string tooltipActivationProperty, bool? disableExpiry, int? tooltipWidth, string entryIconName, Color? iconColor, Color? termColor, int? orderingNumber, float? entryValue, bool showBar = false, float? normalizedValue1 = null, float? normalizedValue2 = null, int? barWidth = null, bool clickable = false, uint? entityID = null, UIComponent cutsomComponent = null, int? height = null, int? paddingLeft = null, bool showIconBackground = false)
	{
		UIComponent uIComponent = null;
		if (!CanProcessEntryData(entryTerm, entryIconName, normalizedValue1))
		{
			return null;
		}
		if (entryIconName != null)
		{
			if (poolOfIcons.Count > 0)
			{
				uIComponent = poolOfIcons[poolOfIcons.Count - 1];
				poolOfIcons.RemoveAt(poolOfIcons.Count - 1);
			}
			else
			{
				uIComponent = new Icon(base.GUIManager);
			}
		}
		else if (showBar || normalizedValue1.HasValue)
		{
			uIComponent = new FillableBar(base.GUIManager, FillableBar.FillableBarType.ProgressBar)
			{
				ShowMaxValueLabelAtEnd = false
			};
			uIComponent.ID = DataControlID.HorizontalConnector;
			uIComponent.Width = 20;
			uIComponent.Height = 12;
		}
		AddEntry(key, uIComponent);
		uIComponent.Tag2 = entityID;
		if (orderingNumber.HasValue)
		{
			uIComponent.OrderByTag2 = orderingNumber.Value;
		}
		else
		{
			uIComponent.OrderByTag2 = -1;
		}
		return uIComponent;
	}

	public bool AddSeparator(object key, int? orderingNumber, float? entryValue)
	{
		return false;
	}

	public bool AddSubHeader(string subHeaderName, int? orderingNumber, float? entryValue)
	{
		return false;
	}

	public void UpdateEntry(UIComponent existingEntry, string entryTerm, string caption, string entryTermTooltip, string captionTooltip, string entryIconName, Color? iconColor, Color? termColor, float? entryValue, float? normalizedValue1, float? normalizedValue2 = null, int? paddingLeft = null, int? paddingRight = null, bool showIconBackground = false, bool showFaded = false, bool rightAdjustTerm = false)
	{
		if (existingEntry is Icon icon)
		{
			if (entryIconName == null)
			{
				return;
			}
			Rectangle sourceRectangle = guiManager.GUISpriteSheet.GetSourceRectangle(entryIconName);
			icon.SetSkinLocations(sourceRectangle, Icon.UIType.HUD);
			icon.ResizeControlToFitImage();
			if (iconColor.HasValue)
			{
				if (showFaded)
				{
					icon.Color = Color.Gray;
				}
				else
				{
					icon.Color = iconColor.Value;
				}
			}
			else
			{
				icon.Color = null;
			}
			icon.ToolTip = entryTermTooltip;
			switch (entryTermTooltip)
			{
			case null:
			case "":
			case "No tooltip available.":
				icon.ToolTip = entryTerm;
				break;
			}
		}
		if (existingEntry is FillableBar fillableBar)
		{
			if (!normalizedValue1.HasValue)
			{
				return;
			}
			fillableBar.MaxValue = 20;
			fillableBar.Value = (int)(normalizedValue1 * (float)fillableBar.MaxValue).Value;
			fillableBar.ToolTip = entryTermTooltip;
			if (iconColor.HasValue)
			{
				fillableBar.BarColor = iconColor.Value;
			}
			else if (termColor.HasValue)
			{
				fillableBar.BarColor = termColor.Value;
			}
			if (showFaded)
			{
				fillableBar.Enabled = false;
			}
			else
			{
				fillableBar.Enabled = true;
			}
		}
		if (entryValue.HasValue)
		{
			existingEntry.OrderByTag1 = entryValue.Value;
		}
		if (surface.Height < existingEntry.Height)
		{
			surface.Height = existingEntry.Height;
		}
	}

	public object GetKeyFromIndex(int index)
	{
		return Entries[index].Tag1;
	}
}
