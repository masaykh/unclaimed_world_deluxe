using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Log;
using WindowSystem;

namespace UWGame.ClientSide.Interface.HUD_Windows;

public class HUDLog : HUDWindow
{
	private const int minHeight = 26;

	private const int defaultHeight = 80;

	private const int pagerHeight = 32;

	private Grid grid;

	private List<TextButton> pagerButtons = new List<TextButton>();

	private List<Event> logdata;

	private int totalPages;

	private int currentPage = 1;

	private int thisPageButtonIndex;

	private int messageGridYPos;

	public const int DefaultWidth = 404;

	private int logMessagesPerPage;

	private Dictionary<uint, UIComponent> messageRowCache = new Dictionary<uint, UIComponent>();

	private const int maxRowsToStore = 1000;

	private const int noOfPageButtonsToEachSide = 2;

	private const int pagerXPos = 210;

	private int xPosOfCurrentPageButton;

	private const int pageButtonSpacing = 4;

	private const int spacingToEndButtons = 12;

	public HUDLog(int xPos, int width)
		: base(width, 80)
	{
		HideOnRightClick = false;
		messageGridYPos = 12;
		DisplayWindow.X = xPos - 2;
		DisplayWindow.Y = The.Sim.Controller.DrawArea.Height - 80 - 10;
		DisplayWindow.Resizable = false;
		DisplayWindow.SetResizableArea(ResizeAreas.Top, isResizable: true);
		DisplayWindow.SetResizableArea(ResizeAreas.Right, isResizable: true);
		DisplayWindow.SetResizableArea(ResizeAreas.TopRight, isResizable: true);
		DisplayWindow.ResizableBorderSize = messageGridYPos;
		DisplayWindow.Level = Level.Bottom;
		DisplayWindow.MinHeight = 26;
		DisplayWindow.MinWidth = 60;
		DisplayWindow.Resize += DisplayWindow_Resize;
		DisplayWindow.Show();
		PlacePageButtons();
		grid = new Grid(gui, ListBoxType.HUDAndLCD, Label.LabelType.HUDWindow);
		DisplayWindow.Add(grid);
		grid.Font = GUIManager.LCDandHUDBodyFontPath;
		grid.Color = Color.White;
		grid.ItemHeight = 20;
		SetListWidth();
		SetListHeight();
		grid.Position = new Point(8, messageGridYPos);
		grid.ZOrder = 1f;
		grid.Parent.Y -= 6;
		grid.ScrollBar.Position = new Point(grid.Width - 28, 0);
	}

	private void SetListHeight()
	{
		grid.Height = DisplayWindow.Height - 32 - messageGridYPos;
		foreach (TextButton pagerButton in pagerButtons)
		{
			pagerButton.Y = grid.Bottom + 6;
		}
		int num = Math.Max(The.Sim.Controller.Options.MinimumLogMessagesPerPage, (grid.Height - 20) / grid.ItemHeight);
		if (num != logMessagesPerPage)
		{
			logMessagesPerPage = num;
			GetAllData();
			if (currentPage > totalPages)
			{
				currentPage = Common.ClampBottom(totalPages, 1);
			}
			GetMessageIndicesToShow(currentPage, out var startIndex, out var endIndex);
			Repopulate(startIndex, endIndex);
		}
	}

	private void SetListWidth()
	{
		grid.Width = DisplayWindow.Width - 2 * grid.X;
	}

	private void DisplayWindow_Resize(UIComponent sender)
	{
		SetListHeight();
		SetListWidth();
	}

	private void AddRow(Event newEvent)
	{
		if (!messageRowCache.TryGetValue(newEvent.ID, out var value))
		{
			string entry = FormatLogEntryText(newEvent);
			value = grid.AddEntry(newEvent.ID, entry);
			if (messageRowCache.Count < 1000)
			{
				messageRowCache.Add(newEvent.ID, value);
			}
		}
		else
		{
			grid.AddEntry(newEvent.ID, value);
		}
	}

	public static string FormatLogEntryText(Event newEvent, bool includeTime = true)
	{
		string arg = "";
		if (includeTime)
		{
			arg = newEvent.Time.ToShortTimeString() + " ";
		}
		if (newEvent.Entity.HasValue)
		{
			return $"{arg}{newEvent.EntityLink} {newEvent.Text}";
		}
		return $"{arg}{newEvent.Text}";
	}

	private void GetAllData()
	{
		logdata = The.Client.Log.Events;
		totalPages = (int)Math.Ceiling((float)logdata.Count / (float)logMessagesPerPage);
	}

	private void PlacePageButtons()
	{
		xPosOfCurrentPageButton = DisplayWindow.Width / 2;
		thisPageButtonIndex = 3;
		int num = 7;
		for (int i = 0; i < num; i++)
		{
			TextButton textButton = new TextButton(The.InGameUI.gui);
			if (i == thisPageButtonIndex)
			{
				textButton.Init(TextButton.TextButtonType.HUDHasState);
			}
			else
			{
				textButton.Init(TextButton.TextButtonType.HUD);
			}
			pagerButtons.Add(textButton);
			textButton.Visible = false;
			Add(textButton);
			textButton.Click += btPager_Click;
		}
	}

	private void btPager_Click(UIComponent sender, EventArgs e)
	{
		int num = (int)sender.Tag1;
		if (num != currentPage)
		{
			((TextButton)sender).IsChecked = false;
			currentPage = num;
			GetMessageIndicesToShow(currentPage, out var startIndex, out var endIndex);
			Repopulate(startIndex, endIndex);
		}
	}

	private void RefreshPagerButtons()
	{
		foreach (TextButton pagerButton in pagerButtons)
		{
			pagerButton.Visible = false;
			pagerButton.IsChecked = false;
		}
		if (totalPages == 1)
		{
			return;
		}
		int f = currentPage - 2;
		f = Common.ClampBottom(f, 1);
		_ = currentPage;
		int d = currentPage + 2;
		d = Common.ClampTop(d, totalPages);
		_ = totalPages;
		TextButton textButton = pagerButtons[thisPageButtonIndex];
		textButton.Text = currentPage.ToString();
		textButton.IsChecked = true;
		textButton.ScaleWidthToFitText();
		textButton.X = xPosOfCurrentPageButton - textButton.Width / 2;
		textButton.Visible = true;
		textButton.Tag1 = currentPage;
		int num = currentPage - 1;
		int x = textButton.X;
		for (int num2 = thisPageButtonIndex - 1; num2 >= 0; num2--)
		{
			TextButton textButton2 = pagerButtons[num2];
			if (num >= f)
			{
				textButton2.Visible = true;
				textButton2.Text = num.ToString();
				textButton2.ScaleWidthToFitText();
				textButton2.X = x - 4 - textButton2.Width;
				textButton2.Tag1 = num;
				x = textButton2.X;
			}
			else
			{
				textButton2.Visible = false;
			}
			num--;
		}
		if (f > 1)
		{
			TextButton textButton2 = pagerButtons[0];
			textButton2.Visible = true;
			textButton2.Text = "1";
			textButton2.ScaleWidthToFitText();
			textButton2.Tag1 = 1;
			textButton2.X = x - 12 - textButton2.Width;
		}
		num = currentPage + 1;
		x = textButton.Right;
		for (int i = thisPageButtonIndex + 1; i < pagerButtons.Count; i++)
		{
			TextButton textButton2 = pagerButtons[i];
			if (num <= d)
			{
				textButton2.Visible = true;
				textButton2.Text = num.ToString();
				textButton2.ScaleWidthToFitText();
				textButton2.X = x + 4;
				textButton2.Tag1 = num;
				x = textButton2.Right;
			}
			else
			{
				textButton2.Visible = false;
			}
			num++;
		}
		if (d < totalPages)
		{
			TextButton textButton2 = pagerButtons[pagerButtons.Count - 1];
			textButton2.Visible = true;
			textButton2.Text = totalPages.ToString();
			textButton2.ScaleWidthToFitText();
			textButton2.Tag1 = totalPages;
			textButton2.X = x + 12;
		}
	}

	private void Repopulate(int startIndex, int endIndex)
	{
		grid.BeginAddingEntries();
		grid.Clear();
		if (endIndex - startIndex >= 0 && logdata.Count > 0)
		{
			for (int i = startIndex; i <= endIndex; i++)
			{
				Event newEvent = logdata[i];
				AddRow(newEvent);
			}
			grid.EndAddingEntries();
			if (currentPage == 1)
			{
				grid.ScrollToIndex(grid.Entries.Count - 1);
			}
			else
			{
				grid.ScrollToIndex(0);
			}
		}
		else
		{
			grid.EndAddingEntries();
		}
		RefreshPagerButtons();
	}

	private void GetMessageIndicesToShow(int pageToShow, out int startIndex, out int endIndex)
	{
		if (totalPages > 0)
		{
			startIndex = (totalPages - pageToShow) * logMessagesPerPage;
			endIndex = Common.Min(logdata.Count - 1, startIndex + logMessagesPerPage - 1);
		}
		else
		{
			startIndex = 0;
			endIndex = 0;
		}
	}

	private int GetFirstIndexOfNewMessagesToShow()
	{
		int result = -1;
		if (grid.Count > 0)
		{
			uint num = (uint)grid.Entries[grid.Count - 1].Tag1;
			for (int num2 = logdata.Count - 1; num2 >= 0; num2--)
			{
				Event obj = logdata[num2];
				if (obj.ID <= num)
				{
					result = num2 + 1;
					if (result < logdata.Count)
					{
						return result;
					}
					return -1;
				}
			}
		}
		else
		{
			result = 0;
		}
		return result;
	}

	public override void Refresh()
	{
		if (currentPage == 1)
		{
			int count = grid.Entries.Count;
			GetAllData();
			GetMessageIndicesToShow(currentPage, out var startIndex, out var endIndex);
			int num = endIndex - startIndex + 1;
			if (count != num)
			{
				Repopulate(startIndex, endIndex);
			}
		}
	}
}
