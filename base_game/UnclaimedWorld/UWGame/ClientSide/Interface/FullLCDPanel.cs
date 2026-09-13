using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

public class FullLCDPanel
{
	public delegate bool TypeIsRepresented(object type);

	public delegate void SetCollapsedSummary(CollapsablePanel cp, object type);

	private const int lcdPaddingLeft = 12;

	private const int lcdPaddingRight = 12;

	private const int lcdPaddingTop = 12;

	private const int lcdPaddingBottom = 12;

	public static Bar AddLCDDividerLine(GUIManager gui, int yPos, int xMargin, UIComponent lcdSurface)
	{
		return AddLCDLine(gui, new Point(xMargin, yPos), lcdSurface.Width - 2 * xMargin - 25, lcdSurface);
	}

	public static int GetContentWidthFromLCDSurface(UIComponent lcdSurface)
	{
		return lcdSurface.Width - 12 - 25;
	}

	public static Bar AddLCDLine(GUIManager gui, Point pos, int width, UIComponent lcdSurface)
	{
		Bar bar = new Bar(gui);
		lcdSurface.Add(bar);
		bar.Position = pos;
		bar.EdgeSize = 6;
		Rectangle sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle("lcd_line");
		bar.SetSkinLocation(SkinState.Normal, sourceRectangle);
		bar.Width = width;
		bar.Height = sourceRectangle.Height;
		bar.RenderType = RenderType.CRTAndLCD;
		return bar;
	}

	public static Bar AddLCDLineThin(GUIManager gui, Point pos, int width, UIComponent lcdSurface)
	{
		Bar bar = new Bar(gui);
		lcdSurface.Add(bar);
		bar.Position = pos;
		bar.EdgeSize = 1;
		Rectangle sourceRectangle = gui.GUISpriteSheet.GetSourceRectangle("lcd_thinline");
		bar.SetSkinLocation(SkinState.Normal, sourceRectangle);
		bar.Width = width;
		bar.Height = sourceRectangle.Height;
		bar.RenderType = RenderType.CRTAndLCD;
		return bar;
	}

	public static void AddLCDPanelFitWindowWithBottomMargin(CommonInterface intf, Window window, int bottomMargin, Point? margin, out Box display, out UIComponent surface, ref LCDScreen lcdScreen, int? width = null, int? rightMargin = null)
	{
		int num;
		int num2;
		Point position;
		if (margin.HasValue)
		{
			num = margin.Value.X;
			num2 = margin.Value.Y;
			position = new Point(margin.Value.X, margin.Value.Y);
		}
		else
		{
			num = window.Margin;
			num2 = 0;
			position = new Point(0, 0);
		}
		int width2 = (width.HasValue ? width.Value : ((!rightMargin.HasValue) ? (window.Width - 2 * num) : (window.Width - num - rightMargin.Value)));
		int height = window.Height - bottomMargin - num2;
		AddLCDPanel(intf, window, position, width2, height, out display, out surface, ref lcdScreen);
	}

	public static void SetFullsizeLCDScreenDimensionsFromContent(Window window, Box display, LCDScreen lcdScreen, int margin, int surfaceWidth, int? surfaceHeight = null)
	{
		display.Width = surfaceWidth + 12 + 12;
		if (surfaceHeight.HasValue)
		{
			display.Height = surfaceHeight.Value + 12 + 12;
		}
		Rectangle dimensions = new Rectangle(display.X, display.Y, display.Width, display.Height);
		lcdScreen.SetDimensions(dimensions);
		window.Width = display.Width + 2 * margin;
	}

	public static void AddLCDPanel(CommonInterface intf, Window window, Point position, int width, int height, out Box display, out UIComponent surface, ref LCDScreen lcdScreen)
	{
		window.HasCRTOrLCDComponents = true;
		window.HasOverlayComponents = true;
		display = new Box(intf.gui);
		window.Add(display);
		display.DebugTag = "lcdBackground";
		display.RenderType = RenderType.CRTAndLCD;
		Rectangle sourceRectangle = intf.gui.GUISpriteSheet.GetSourceRectangle("basic_display_panel");
		display.SetSkinLocation(SkinState.Normal, sourceRectangle);
		display.CornerSize = 12;
		display.Position = new Point(position.X, position.Y);
		display.Width = width;
		display.Height = height;
		surface = new UIComponent(intf.gui);
		surface.Position = new Point(display.X + 12, display.Y + 12);
		surface.Width = width - 12 - 12;
		surface.Height = display.Height - 12 - 12;
		surface.RenderType = RenderType.CRTAndLCD;
		window.Add(surface);
		lcdScreen = intf.DisplayPanelRenderer.AddLCD(display, window.Level, window, drawDust: true);
	}

	public static Grid AddGridWithFixedItemHeights(GUIManager gui, UIComponent lcdSurface, int gridTopMargin, int gridBottomMargin = 0)
	{
		Grid grid = new Grid(gui, ListBoxType.LCD, Label.LabelType.LCDNormal);
		grid.FixedItemHeights = true;
		grid.RenderType = RenderType.CRTAndLCD;
		lcdSurface.Add(grid);
		grid.Font = GUIManager.LCDandHUDBodyFontPath;
		grid.Width = lcdSurface.Width;
		grid.Height = lcdSurface.Height - gridTopMargin - gridBottomMargin;
		grid.ItemHeight = 22;
		grid.Position = new Point(0, gridTopMargin);
		grid.CanGrowInHeight = false;
		grid.ScrollBarEnabled = true;
		grid.Selectability = Grid.SelectabilityOptions.Single;
		return grid;
	}

	public static Grid AddTreeGrid(GUIManager gui, CollapsablePanel.PanelType panelType)
	{
		Grid obj = new Grid(gui, ListBoxType.LCD, Label.LabelType.LCDNormal)
		{
			FixedItemHeights = false,
			RenderType = RenderType.CRTAndLCD,
			Font = GUIManager.LCDandHUDBodyFontPath
		};
		int y = 10;
		obj.ItemHeight = ((panelType == CollapsablePanel.PanelType.DropDownBig) ? 26 : 22);
		obj.Position = new Point(0, y);
		obj.CanGrowInHeight = true;
		return obj;
	}

	public static void PopulateCategoryGrid<T, TE, CATTYPE>(GUIManager gui, Grid outerGrid, CollapsablePanel.PanelType panelType, Label.LabelType labelType, SetCollapsedSummary setCollapsedSummary, ClickHandler clickHandler, Dictionary<T, TE> dictionary) where T : IHasCategory<CATTYPE> where CATTYPE : ICategoryType
	{
		outerGrid.BeginAddingEntries();
		Grid grid = null;
		UIComponent item = null;
		List<Grid> foundChildren = new List<Grid>();
		List<CollapsablePanel> list = new List<CollapsablePanel>();
		foreach (KeyValuePair<T, TE> item2 in dictionary)
		{
			CollapsablePanel collapsablePanel;
			if (outerGrid.TryGetEntry(item2.Key.Category, out item))
			{
				collapsablePanel = (CollapsablePanel)item;
				if (!list.Contains(collapsablePanel))
				{
					collapsablePanel.Summary = "";
					list.Add(collapsablePanel);
				}
				foundChildren.Clear();
				collapsablePanel.ExpandedPanel.FindChildOfType(null, ref foundChildren);
				grid = foundChildren[0];
			}
			else
			{
				if (panelType == CollapsablePanel.PanelType.DropDownBig)
				{
					collapsablePanel = new CollapsablePanel(gui, CollapsablePanel.PanelType.DropDownBig);
					collapsablePanel.HeadingYPos = 4;
					collapsablePanel.CollapsedHeight = outerGrid.ItemHeight;
					outerGrid.AddEntry(item2.Key.Category, collapsablePanel);
					collapsablePanel.Init();
				}
				else
				{
					collapsablePanel = new CollapsablePanel(gui, panelType);
					collapsablePanel.HeadingYPos = 4;
					collapsablePanel.CollapsedHeight = outerGrid.ItemHeight;
					outerGrid.AddEntry(item2.Key.Category, collapsablePanel);
					collapsablePanel.Init();
					if (item2.Key.Category is IHasIcon { IconSpriteName: not null } hasIcon)
					{
						collapsablePanel.SetIcon(hasIcon.IconSpriteName);
					}
				}
				collapsablePanel.Title = item2.Key.Category.Name;
				collapsablePanel.Width = outerGrid.Width;
				grid = new Grid(gui, ListBoxType.LCD, labelType);
				grid.IsOuterGrid = false;
				grid.FixedItemHeights = true;
				grid.Width = collapsablePanel.Width;
				collapsablePanel.AddContent(grid);
				grid.ScrollBarEnabled = false;
				grid.ItemHeight = 22;
				grid.CanGrowInHeight = true;
				grid.Font = GUIManager.LCDandHUDBodyFontPath;
			}
			grid.BeginAddingEntries();
			string text = item2.Value.ToString();
			if (grid.TryGetEntry(item2.Key, out item))
			{
				UIComponent uIComponent = item.FindChildById("value");
				if (uIComponent != null)
				{
					Label label = (Label)uIComponent;
					label.Text = text;
					collapsablePanel.RightJustifyLabel(label);
				}
				else
				{
					uIComponent = item.FindChildById("captionAndValue");
					((Label)uIComponent).Text = item2.Key.Name + " " + text;
				}
			}
			else if (item2.Key is IHasIcon hasIcon2)
			{
				grid.AddEntryRightJustifyValue(item2.Key, gui.GUISpriteSheet.GetSourceRectangle(hasIcon2.IconSpriteName), 34, 60, item2.Key.Name, collapsablePanel.GetPaddingRight(), text);
			}
			else if (clickHandler != null)
			{
				grid.AddEntryAndButton(item2.Key, null, item2.Key.Name, collapsablePanel.GetPaddingRight(), text, clickHandler, new IGameDataButtonEventArgs(item2.Key));
			}
			else
			{
				grid.AddEntryRightJustifyValue(item2.Key, null, 0, 34, item2.Key.Name, collapsablePanel.GetPaddingRight(), text);
			}
			setCollapsedSummary?.Invoke(collapsablePanel, text);
		}
		Cleanup<T, TE, CATTYPE>(outerGrid, dictionary, null, null);
		outerGrid.EndAddingEntries();
	}

	public static void Cleanup<T, TE, CATTYPE>(Grid outerGrid, Dictionary<T, TE> amountDictionary, Dictionary<T, T> typeDictionary, Predicate<T> itemExists, Predicate<CATTYPE> categoryExists = null) where T : IHasCategory<CATTYPE> where CATTYPE : ICategoryType
	{
		List<Grid> foundChildren = new List<Grid>();
		List<T> list = new List<T>();
		List<ICategoryType> list2 = new List<ICategoryType>();
		ICategoryType categoryType = null;
		foreach (UIComponent entry in outerGrid.Entries)
		{
			foundChildren.Clear();
			((CollapsablePanel)entry).ExpandedPanel.FindChildOfType(null, ref foundChildren);
			Grid grid = foundChildren[0];
			list.Clear();
			categoryType = null;
			foreach (KeyValuePair<object, UIComponent> item in grid.EntriesByKey)
			{
				T val = (T)item.Key;
				if ((itemExists != null && !itemExists(val)) || (amountDictionary != null && !amountDictionary.ContainsKey(val)) || (typeDictionary != null && !typeDictionary.ContainsKey(val)))
				{
					list.Add(val);
				}
			}
			foreach (T item2 in list)
			{
				categoryType = item2.Category;
				grid.RemoveEntry(item2);
			}
			if (categoryType != null && categoryExists != null && !categoryExists((CATTYPE)entry.Tag1) && grid.EntriesByKey.Count == 0)
			{
				list2.Add(categoryType);
			}
		}
		foreach (ICategoryType item3 in list2)
		{
			outerGrid.RemoveEntry(item3);
		}
		foundChildren.Clear();
		outerGrid.FindChildOfType(null, ref foundChildren);
		foreach (Grid item4 in foundChildren)
		{
			item4.EndAddingEntries();
		}
	}

	public static void PopulateCategoryGrid<T, TE, CATTYPE>(GUIManager gui, Grid outerGrid1, Grid outerGrid2, int xValueColumn, TypeIsRepresented TypeIsRepresented, SetCollapsedSummary setCollapsedSummary, Dictionary<T, TE> dictionary) where T : IHasCategory<CATTYPE> where CATTYPE : ICategoryType
	{
		outerGrid1.BeginAddingEntries();
		outerGrid2.BeginAddingEntries();
		Grid grid = null;
		UIComponent item = null;
		List<Grid> foundChildren = new List<Grid>();
		List<CollapsablePanel> list = new List<CollapsablePanel>();
		Grid grid2 = outerGrid1;
		foreach (KeyValuePair<T, TE> item2 in dictionary)
		{
			CollapsablePanel collapsablePanel;
			if (outerGrid1.TryGetEntry(item2.Key.Category, out item))
			{
				grid2 = outerGrid1;
				collapsablePanel = (CollapsablePanel)item;
				if (!list.Contains(collapsablePanel))
				{
					collapsablePanel.Summary = "";
					list.Add(collapsablePanel);
				}
				foundChildren.Clear();
				collapsablePanel.ExpandedPanel.FindChildOfType(null, ref foundChildren);
				grid = foundChildren[0];
			}
			else if (outerGrid2.TryGetEntry(item2.Key.Category, out item))
			{
				grid2 = outerGrid2;
				collapsablePanel = (CollapsablePanel)item;
				if (!list.Contains(collapsablePanel))
				{
					collapsablePanel.Summary = "";
					list.Add(collapsablePanel);
				}
				foundChildren.Clear();
				collapsablePanel.ExpandedPanel.FindChildOfType(null, ref foundChildren);
				grid = foundChildren[0];
			}
			else
			{
				collapsablePanel = new CollapsablePanel(gui, CollapsablePanel.PanelType.DropDownBig);
				collapsablePanel.HeadingYPos = 4;
				collapsablePanel.CollapsedHeight = grid2.ItemHeight;
				grid2.AddEntry(item2.Key.Category, collapsablePanel);
				collapsablePanel.Init();
				collapsablePanel.Title = item2.Key.Category.Name;
				collapsablePanel.Width = grid2.Width;
				grid = new Grid(gui, ListBoxType.LCD, Label.LabelType.LCDNormal);
				grid.IsOuterGrid = false;
				grid.FixedItemHeights = true;
				grid.Width = collapsablePanel.Width;
				collapsablePanel.AddContent(grid);
				grid.ScrollBarEnabled = false;
				grid.ItemHeight = 22;
				grid.CanGrowInHeight = true;
				grid.Font = GUIManager.LCDandHUDBodyFontPath;
			}
			grid.BeginAddingEntries();
			string text = item2.Value.ToString();
			if (grid.TryGetEntry(item2.Key, out item))
			{
				((Label)item.FindChildById("value")).Text = text;
			}
			else
			{
				grid.AddEntry(item2.Key, item2.Key.Name, xValueColumn, text);
			}
			setCollapsedSummary(collapsablePanel, text);
			grid2 = ((grid2 != outerGrid1) ? outerGrid1 : outerGrid2);
		}
		List<T> list2 = new List<T>();
		List<ICategoryType> list3 = new List<ICategoryType>();
		ICategoryType categoryType = null;
		grid2 = outerGrid1;
		for (int i = 0; i < 2; i++)
		{
			foreach (UIComponent entry in grid2.Entries)
			{
				foundChildren.Clear();
				((CollapsablePanel)entry).ExpandedPanel.FindChildOfType(null, ref foundChildren);
				Grid grid3 = foundChildren[0];
				list2.Clear();
				categoryType = null;
				foreach (KeyValuePair<object, UIComponent> item3 in grid3.EntriesByKey)
				{
					T val = (T)item3.Key;
					if (!TypeIsRepresented(val))
					{
						list2.Add(val);
					}
				}
				foreach (T item4 in list2)
				{
					categoryType = item4.Category;
					grid3.RemoveEntry(item4);
				}
				if (categoryType != null && grid3.EntriesByKey.Count == 0 && categoryType != null)
				{
					list3.Add(categoryType);
				}
			}
			foreach (ICategoryType item5 in list3)
			{
				grid2.RemoveEntry(item5);
			}
			foundChildren.Clear();
			grid2.FindChildOfType(null, ref foundChildren);
			foreach (Grid item6 in foundChildren)
			{
				item6.EndAddingEntries();
			}
			list3.Clear();
			grid2 = outerGrid2;
		}
		int num = Math.Abs(outerGrid1.Entries.Count - outerGrid2.Entries.Count);
		if (num > 1)
		{
			list3.Clear();
			if (outerGrid1.Entries.Count > outerGrid2.Entries.Count)
			{
				foreach (KeyValuePair<object, UIComponent> item7 in outerGrid1.EntriesByKey)
				{
					list3.Add((ICategoryType)item7.Key);
					num--;
					if (num < 2)
					{
						break;
					}
				}
				foreach (ICategoryType item8 in list3)
				{
					_ = item8;
				}
			}
			else
			{
				foreach (KeyValuePair<object, UIComponent> item9 in outerGrid2.EntriesByKey)
				{
					list3.Add((ICategoryType)item9.Key);
					num--;
					if (num < 2)
					{
						break;
					}
				}
				foreach (ICategoryType item10 in list3)
				{
					_ = item10;
				}
			}
		}
		outerGrid1.EndAddingEntries();
		outerGrid2.EndAddingEntries();
	}
}
