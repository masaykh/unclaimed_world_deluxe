using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface.LCD;
using UWGame.SimSide.AI;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Overland.Missions.Templates;
using UWGame.SimSide.Snapshots;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Missions;

public class ActionPicker : UIComponent
{
	private LCDInnerPanel panel;

	private HorizontalList list;

	private int maxWidth = 409;

	private ImageButton btSell;

	private ImageButton btBuy;
	private ImageButton btEmbark;
	private MissionStopTemplate missionStopTemplate;

	private MissionTemplate missionTemplate;

	public event Action<ActionTypes> ActionSelected;

	public event Action InvalidLocation;

	public ActionPicker(GUIManager gui)
		: base(gui)
	{
		DebugTag = "ActionPicker";
		panel = new LCDInnerPanel(gui, maxWidth, includeDecor: true, 1f);
		Add(panel.Panel);
		panel.ContentHeight = 215;
		panel.Panel.Y = 0;
		panel.VerticalContentPadding = 6;
		panel.Panel.X = 0;
		Color value = "#ccffcc".ColorFromHex();
		panel.Panel.SetSkinLocation(SkinState.Normal, gui.GUISpriteSheet.GetSourceRectangle("lcd_panel_background"), value, value);
		Width = panel.Panel.Width;
		Height = panel.Panel.Height;
		base.ZOrder = 1f;
		list = new HorizontalList(gui);
		list.RenderType = RenderType.CRTAndLCD;
		panel.AddContentSetFullWidth(list);
		list.Font = GUIManager.LCDandHUDBodyFontPath;
		list.MaxWidth = panel.ContentWidth;
		list.Height = 0;
		list.Y = 15;
		list.X = 15;
		PopulateAll();
	}

	public void Fill(MissionTemplate missionTemplate, MissionStopTemplate missionstopTemplate)
	{
		missionStopTemplate = missionstopTemplate;
		this.missionTemplate = missionTemplate;
	}

	public void Show()
	{
		Populate();
	}

	public new void Refresh()
	{
		Populate();
	}

	private void PopulateAll()
	{
		list.BeginAddingEntries();
		btBuy = AddAction(ActionTypes.Buy);
		btEmbark = AddAction(ActionTypes.Embark);
		btSell = AddAction(ActionTypes.Sell);
		list.EndAddingEntries();
	}

	private ImageButton AddAction(ActionTypes action)
	{
		CreateMissionPanel.AddAction(list, action, action, action, null, enabled: true, null, guiManager, out var btAction);
		btAction.Click += bt_Click;
		return btAction;
	}

	private void Populate()
	{
		Allegiance ourAllegiance = LookUp<Allegiance, AllegianceID>.FindByID((AllegianceID)missionTemplate.Allegiance);
		if (!missionStopTemplate.TravelLocation.ResolveLocation(The.InGameUI.UIAllegiance.SharedKnowledge, out var _, out var allegiance, out var _, out var terminalData))
		{
			if (this.InvalidLocation != null)
			{
				this.InvalidLocation();
			}
		}
		else
		{
			UpdateSelling(terminalData);
			UpdateBuying(terminalData);
			UpdateEmbark(ourAllegiance, terminalData, allegiance);
		}
	}

	private void UpdateEmbark(Allegiance ourAllegiance, IKnownEntityData terminalData, Allegiance siteAllegiance)
	{
		List<string> errors = null;
		if (!ActionExists(ActionTypes.Embark, ref errors) && EmbarkActionTemplate.ValidateEmbark(ourAllegiance, terminalData, siteAllegiance, ref errors))
		{
			btEmbark.Enabled = true;
			btEmbark.ToolTip = UWGame.Locale.Text("Click to add an Embark action");
		}
		else
		{
			btEmbark.Enabled = false;
			btEmbark.ToolTip = UWGame.Locale.Text("Embark is unavailable here:") + " " + string.Join(" \n", errors);
		}
	}

	private void UpdateSelling(IKnownEntityData terminalData)
	{
		List<string> errors = null;
		if (!ActionExists(ActionTypes.Sell, ref errors) && BuySellActionTemplate.ValidateWorkingTerminalCanSell(missionTemplate, terminalData, ref errors))
		{
			btSell.Enabled = true;
			btSell.ToolTip = UWGame.Locale.Text("Click to add a Sell action");
		}
		else
		{
			btSell.Enabled = false;
			btSell.ToolTip = UWGame.Locale.Text("Sell is unavailable here:") + " " + string.Join(" \n", errors);
		}
	}

	private bool ActionExists(ActionTypes action, ref List<string> errors)
	{
		if (missionStopTemplate.Actions.Any((MissionActionTemplate a) => a.ActionType == action))
		{
			Common.AddToList(ref errors, UWGame.Locale.Text("Action already exists."));
			return true;
		}
		return false;
	}

	private void UpdateBuying(IKnownEntityData terminalData)
	{
		List<string> errors = null;
		if (!ActionExists(ActionTypes.Buy, ref errors) && BuySellActionTemplate.ValidateWorkingTerminalCanBuy(missionTemplate, terminalData, ref errors))
		{
			btBuy.Enabled = true;
			btBuy.ToolTip = UWGame.Locale.Text("Click to add a Buy action");
		}
		else
		{
			btBuy.Enabled = false;
			btBuy.ToolTip = UWGame.Locale.Text("Buy is unavailable here:") + " " + string.Join(" \n", errors);
		}
	}

	private void bt_Click(UIComponent sender, EventArgs e)
	{
		if (this.ActionSelected != null)
		{
			this.ActionSelected((ActionTypes)sender.Tag1);
		}
	}
}
