using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Log;
using UWGame.SimSide.Entities;
using WindowSystem;

namespace UWGame.ClientSide.Interface.HUD_Windows;

public class HUDTalkPanel : HUDWindow
{
	private const int minHeight = 26;

	private const int defaultHeight = 260;

	private Grid grid;

	private List<TalkEvent> logdata;

	private static Color[] conversationColors = new Color[7]
	{
		Color.White,
		Color.Turquoise,
		Color.LightBlue,
		Color.LightSkyBlue,
		Color.LightCyan,
		Color.MediumAquamarine,
		Color.LightSeaGreen
	};

	private const int maxRowsToStore = 1000;

	private const int noOfPageButtonsToEachSide = 2;

	private const int pagerXPos = 210;

	private int xPosOfCurrentPageButton;

	private const int pageButtonSpacing = 4;

	private const int spacingToEndButtons = 12;

	private const int entriesPerMessage = 2;

	private int maxEntries;

	public HUDTalkPanel(int xPos, int yPos, int width, int height)
		: base(width, height)
	{
		HideOnRightClick = false;
		DisplayWindow.X = xPos;
		DisplayWindow.Y = yPos;
		DisplayWindow.Resizable = false;
		DisplayWindow.Level = Level.Bottom;
		DisplayWindow.Show();
		grid = new Grid(gui, ListBoxType.HUDAndLCD, Label.LabelType.HUDWindow);
		DisplayWindow.Add(grid);
		grid.Font = GUIManager.LCDandHUDBodyFontPath;
		grid.Color = Color.White;
		grid.FixedItemHeights = false;
		SetListWidth();
		SetListHeight();
		grid.Position = new Point(10, 10);
		grid.ZOrder = 1f;
		grid.InsertNewRows = Grid.NewRowsInsertion.First;
		maxEntries = 2 * The.Sim.Controller.Options.TalkLogMessagesToShow;
	}

	private void SetListHeight()
	{
		grid.Height = DisplayWindow.Height - 2 * grid.Y - 20;
	}

	private void SetListWidth()
	{
		grid.Width = DisplayWindow.Width - 2 * grid.X - 20;
	}

	private void DisplayWindow_Resize(UIComponent sender)
	{
		SetListHeight();
		SetListWidth();
	}

	private void AddMessage(TalkEvent talkEvent)
	{
		string line = talkEvent.Line;
		Entity entity = Entity.FindByID(talkEvent.SpokenBy);
		if (entity != null)
		{
			Color? conversationColor = GetConversationColor(talkEvent);
			grid.AddEntry(talkEvent.ID, line, useLineBreaks: true, conversationColor).Tag1 = talkEvent.ID;
			grid.AddEntry(string.Concat(entity.EntityID, " ", talkEvent.ID), entity.ToLink(useUpperCase: true), useLineBreaks: false, conversationColor).Tag1 = talkEvent.ID;
		}
	}

	private static Color? GetConversationColor(TalkEvent talkEvent)
	{
		ulong num = 0uL;
		if (talkEvent.MessageGroupNo.HasValue)
		{
			num = talkEvent.MessageGroupNo.Value;
		}
		return conversationColors[num % (ulong)conversationColors.Length];
	}

	private void RemoveOldestMessage()
	{
		grid.RemoveEntry(grid.GetKeyFromIndex(grid.Entries.Count - 1));
		if (grid.Entries.Count > 0)
		{
			grid.RemoveEntry(grid.GetKeyFromIndex(grid.Entries.Count - 1));
		}
	}

	private void GetAllData()
	{
		logdata = The.Client.Log.TalkEvents;
	}

	public override void Refresh()
	{
		GetAllData();
		bool flag = false;
		if (grid.Entries.Count == 0 || grid.ScrollBarIsAtTop())
		{
			flag = true;
		}
		int firstIndexOfNewMessagesToShow = GetFirstIndexOfNewMessagesToShow();
		TalkEvent talkEvent = null;
		if (firstIndexOfNewMessagesToShow != -1)
		{
			grid.BeginAddingEntries();
			for (int i = firstIndexOfNewMessagesToShow; i < logdata.Count; i++)
			{
				talkEvent = logdata[i];
				AddMessage(talkEvent);
			}
			while (grid.Entries.Count > maxEntries)
			{
				RemoveOldestMessage();
			}
			grid.EndAddingEntries();
			if (flag && grid.Entries.Count > 0)
			{
				grid.ScrollToIndex(0);
			}
		}
		if (talkEvent != null)
		{
			The.InGameUI.TalkPanel.ShowSpeaker(talkEvent);
		}
	}

	private int GetFirstIndexOfNewMessagesToShow()
	{
		int result = -1;
		if (grid.Count > 0)
		{
			uint num = (uint)grid.Entries[0].Tag1;
			for (int num2 = logdata.Count - 1; num2 >= 0; num2--)
			{
				TalkEvent talkEvent = logdata[num2];
				if (talkEvent.ID <= num)
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
}
