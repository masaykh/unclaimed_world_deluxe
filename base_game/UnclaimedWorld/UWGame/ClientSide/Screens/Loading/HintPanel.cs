using System;
using System.Collections.Generic;
using System.Linq;
using UWGame.ClientSide.Hints;
using UWGame.ClientSide.Interface;
using UWGame.ClientSide.Interface.HUD_Windows;
using UWGame.Control;
using UWGame.SimSide;
using WindowSystem;

namespace UWGame.ClientSide.Screens.Loading;

public class HintPanel : HUDWindow
{
	private TextArea taHint;

	public HintPanel(CommonInterface intf)
		: base(intf, 600, 80)
	{
		GUIManager gUIManager = intf.gui;
		taHint = new TextArea(gUIManager, ListBoxType.HUDAndLCD);
		taHint.Init(Label.LabelType.HUDWindow);
		Add(taHint);
		taHint.X = 9;
		taHint.Y = 9;
		taHint.CanGrowInHeight = false;
		taHint.ScrollBarEnabled = false;
		taHint.Width = DisplayWindow.Width - 2 * taHint.X;
		taHint.Height = DisplayWindow.Height - 9 - 9;
		taHint.DebugTag = "taHint";
	}

	public void ShowHint(Controller controller)
	{
		Hint hint = null;
		if (GameData.Instance.AllHints.Count > 0)
		{
			List<Hint> list = new List<Hint>();
			foreach (Hint allHint in GameData.Instance.AllHints)
			{
				if (!controller.Progress.DisplayedHints.Contains(allHint.KeyName))
				{
					list.Add(allHint);
				}
			}
			if (list.Count > 0)
			{
				hint = list.OrderByDescending((Hint h) => h.Priority).ToList()[0];
				controller.Progress.DisplayedHints.Add(hint.KeyName);
				try
				{
					controller.Progress.Write();
				}
				catch (Exception)
				{
				}
			}
			else
			{
				hint = Common.GetRandomListMember(GameData.Instance.AllHints, controller.RandomGenerator);
			}
		}
		if (hint != null)
		{
			taHint.Text = hint.Text;
		}
	}
}
