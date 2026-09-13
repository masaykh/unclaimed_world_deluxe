using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface.LCD;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Overland.Missions;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Missions;

public class MissionsPanel : RosterPanel
{
	private Grid grdMissions;

	private Grid outerGrid;

	private const int itemHeight = 36;

	private const int horizPadding = 6;

	private const int vertPadding = 4;

	private List<Mission> allMissionsToShow = new List<Mission>();

	private const int headingYPos = 45;

	private LCDInnerPanel addPanel;

	private const int vehicleColumnX = 7;

	private const int communicationColumnX = 140;

	private const int locationColumnX = 270;

	private const int etaColumnX = 390;

	private const string addPanelKey = "Add Panel";

	public MissionsPanel()
		: base("MISSIONS", 600, needBottomMarginForButtons: false)
	{
		Panel.CreateColumnHeadingsWithFixedLength(lcdSurface, 0, new Tuple<string, int, int>("TRANSPORT", 0, 136), new Tuple<string, int, int>("COMMUNICATION", 140, 128), new Tuple<string, int, int>("NEXT STOP", 270, 118), new Tuple<string, int, int>("ETA", 390, 143));
		int num = 21;
		outerGrid = new Grid(Interface.gui, ListBoxType.LCD, Label.LabelType.CRTBigGlow);
		outerGrid.FixedItemHeights = false;
		outerGrid.RenderType = RenderType.CRTAndLCD;
		lcdSurface.Add(outerGrid);
		outerGrid.HMargin = 0;
		outerGrid.VMargin = 0;
		outerGrid.Font = GUIManager.LCDandHUDBodyFontPath;
		outerGrid.Width = lcdSurface.Width;
		outerGrid.Height = lcdSurface.Height - num;
		outerGrid.CanGrowInHeight = false;
		outerGrid.ScrollBarEnabled = true;
		outerGrid.Position = new Point(0, num);
		outerGrid.RowSpacing = -1;
		grdMissions = new Grid(Interface.gui, ListBoxType.LCD, Label.LabelType.LCDNormal);
		grdMissions.FixedItemHeights = true;
		grdMissions.HMargin = 0;
		grdMissions.VMargin = 0;
		grdMissions.RenderType = RenderType.CRTAndLCD;
		outerGrid.AddEntry(grdMissions, grdMissions);
		grdMissions.Font = GUIManager.LCDandHUDBodyFontPath;
		grdMissions.ItemHeight = 36;
		grdMissions.Position = new Point(0, 0);
		grdMissions.CanGrowInHeight = true;
		grdMissions.ScrollBarEnabled = false;
		grdMissions.Selectability = Grid.SelectabilityOptions.None;
		addPanel = new LCDInnerPanel(Interface.gui, lcdSurface.Width - 4, includeDecor: true, 1f);
		outerGrid.AddEntry("Add Panel", addPanel.Panel);
		addPanel.ContentHeight = 102;
		addPanel.Panel.Y = 0;
		addPanel.Panel.X = 0;
		addPanel.Panel.Add(AddCreateActionButton());
	}

	private UIComponent AddCreateActionButton()
	{
		UIComponent uIComponent = new UIComponent(Interface.gui);
		ImageButton imageButton = new ImageButton(Interface.gui);
		uIComponent.Add(imageButton);
		imageButton.Init(ImageButtonType.AddAction);
		imageButton.ScaleImageToSizeOfControl = false;
		imageButton.Click += tbNew_Click;
		imageButton.X = 5;
		imageButton.Y = 5;
		imageButton.DebugTag = "newRun";
		uIComponent.Width = imageButton.Width;
		uIComponent.Height = imageButton.Height;
		Label label = new Label(Interface.gui);
		uIComponent.Add(label);
		label.Init(Label.LabelType.LCDNormal);
		label.Text = "NEW RUN";
		label.X = 16;
		label.Y = 14;
		return uIComponent;
	}

	private void tbNew_Click(UIComponent sender, EventArgs e)
	{
		The.InGameUI.ChangeRosterPanel(The.InGameUI.CreateMissionPanel);
	}

	private void Populate()
	{
		GetAllMissionsToShow();
		outerGrid.BeginAddingEntries();
		grdMissions.BeginAddingEntries();
		foreach (Mission item2 in allMissionsToShow)
		{
			if (!grdMissions.TryGetEntry(item2, out var item))
			{
				item = AddItemRow(item2, item2);
			}
			UpdateRow(item2, item);
		}
		grdMissions.DeleteEntries((Mission j) => allMissionsToShow.Contains(j));
		grdMissions.EndAddingEntries();
		outerGrid.EndAddingEntries();
	}

	public override void Refresh()
	{
		Populate();
		base.Refresh();
	}

	public override void Show()
	{
		base.Show();
	}

	private void CreateColumnHeadings()
	{
		Label label = new Label(Interface.gui);
		label.Init(Label.LabelType.LCDSmallHeadingBanner);
		label.Text = "NAME";
		lcdSurface.Add(label);
		label.Y = 45;
		label.FitToText();
		label = new Label(Interface.gui);
		label.Init(Label.LabelType.LCDSmallHeadingBanner);
		label.Text = "LOCATION";
		lcdSurface.Add(label);
		label.Y = 45;
		label.X = 270;
		label.FitToText();
		label = new Label(Interface.gui);
		label.Init(Label.LabelType.LCDSmallHeadingBanner);
		label.Text = "COMMUNICATION";
		lcdSurface.Add(label);
		label.Y = 45;
		label.X = 140;
		label.FitToText();
	}

	private void GetAllMissionsToShow()
	{
		allMissionsToShow.Clear();
		if (The.InGameUI.UIAllegiance.Missions != null)
		{
			allMissionsToShow.AddRange(The.InGameUI.UIAllegiance.Missions);
		}
	}

	private UIComponent AddItemRow(object key, Mission transport)
	{
		UIComponent uIComponent = new UIComponent(Interface.gui);
		uIComponent.Height = 36;
		LCDInnerPanel lCDInnerPanel = new LCDInnerPanel(Interface.gui, grdMissions.SurfaceWidth, includeDecor: false);
		uIComponent.Add(lCDInnerPanel.Panel);
		lCDInnerPanel.HorizontalContentPadding = 6;
		lCDInnerPanel.VerticalContentPadding = 4;
		Label label = new Label(Interface.gui);
		lCDInnerPanel.AddContent(label, Panel.GetItemColumnFromHeader(7));
		label.Width = 120;
		label.Init(Label.LabelType.LCDNormal);
		label.ID = UIComponent.DataControlID.Transport;
		uIComponent.CenterChildVertically(label);
		Label label2 = new Label(Interface.gui);
		lCDInnerPanel.AddContent(label2, Panel.GetItemColumnFromHeader(140));
		label2.Width = 120;
		label2.Init(Label.LabelType.LCDNormal);
		label2.ID = UIComponent.DataControlID.Communication;
		uIComponent.CenterChildVertically(label2);
		Label label3 = new Label(Interface.gui);
		lCDInnerPanel.AddContent(label3, Panel.GetItemColumnFromHeader(270));
		label3.Width = 120;
		label3.Init(Label.LabelType.LCDNormal);
		label3.ID = UIComponent.DataControlID.NextStop;
		uIComponent.CenterChildVertically(label3);
		Label label4 = new Label(Interface.gui);
		lCDInnerPanel.AddContent(label4, Panel.GetItemColumnFromHeader(390));
		label4.Width = 120;
		label4.Init(Label.LabelType.LCDNormal);
		label4.ID = UIComponent.DataControlID.ETA;
		uIComponent.CenterChildVertically(label4);
		grdMissions.AddEntry(transport, uIComponent);
		return uIComponent;
	}

	private void UpdateRow(Mission mission, UIComponent itemRow)
	{
		itemRow.Height = 36;
		Label label = itemRow.FindChildById(UIComponent.DataControlID.Transport) as Label;
		EntityType mainTransportation = mission.GetMainTransportation();
		if (mainTransportation != null)
		{
			label.Text = mainTransportation.Name;
		}
		else
		{
			label.Text = "On foot";
		}
		Label lblCommunication = itemRow.FindChildById(UIComponent.DataControlID.Communication) as Label;
		DiplomacyPanel.DisplayCommunication(The.InGameUI.UIAllegiance, mission, lblCommunication, "The mission can be reached with the communication equipment ({0}) that is currently deployed", "We currently have no way to communicate with the mission. ETA and location are not up to date.", out var inCommRange);
		Label label2 = itemRow.FindChildById(UIComponent.DataControlID.NextStop) as Label;
		Label label3 = itemRow.FindChildById(UIComponent.DataControlID.ETA) as Label;
		if (inCommRange)
		{
			label3.NormalColor = label3.GetNormalColorForType();
			if (mission.GetNextStopAndETA(out var nextStop, out var eta))
			{
				nextStop.Value.ResolveLocation(The.InGameUI.UIAllegiance.SharedKnowledge, out var site, out var _, out var _, out var _);
				if (site != null)
				{
					label2.Text = site.Name;
					label3.Text = eta.ToString();
					label3.ToolTip = "The estimated time of arrival";
				}
				else
				{
					label2.Text = "";
					label3.Text = "";
				}
			}
			else
			{
				label2.Text = "";
				label3.Text = "";
			}
		}
		else
		{
			label3.NormalColor = Label.LCDErrorColor;
			label3.ToolTip = "We are not in communication with the mission and the information is out of date.";
		}
	}
}
