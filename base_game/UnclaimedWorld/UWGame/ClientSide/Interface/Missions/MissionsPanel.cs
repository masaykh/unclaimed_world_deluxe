using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface.LCD;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Overland.Missions;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Missions;

public class MissionsPanel : RosterPanel
{
	private Grid grdMissions;

	private Grid outerGrid;
	private List<Mission> allMissionsToShow = new List<Mission>();
	private LCDInnerPanel addPanel;

	private Label lblNewRun;

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
		lblNewRun = new Label(Interface.gui);
		uIComponent.Add(lblNewRun);
		lblNewRun.Init(Label.LabelType.LCDNormal);
		lblNewRun.Text = "NEW RUN";
		lblNewRun.X = 16;
		lblNewRun.Y = 14;
		return uIComponent;
	}

	private void tbNew_Click(UIComponent sender, EventArgs e)
	{
		The.InGameUI.ChangeRosterPanel(The.InGameUI.CreateMissionPanel);
	}

	private void Populate()
	{
		// PORT: the run being planned now survives the planning panel being hidden (see
		// CreateMissionPanel.Hide), so the button that leads back to it says so.
		lblNewRun.Text = The.InGameUI.CreateMissionPanel.HasDraft ? "CONTINUE RUN" : "NEW RUN";
		lblNewRun.FitToText();
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
		// PORT: the next stop and its ETA are filled in whether or not the mission can be reached.
		//
		// The studio filled them only in communication range. Out of range it coloured the ETA as an
		// error and said "the information is out of date" - of a label it had never written to,
		// unless this row had once been drawn while in range. A barge between sites has no radio,
		// so a run that had already left showed an empty ETA, and Kastuk ("Trading") asked for
		// "approximate time of their coming". The arrival is now worked out from the run's own
		// schedule (TravelAction.GetETA: the leg's remaining distance at the transport's speed)
		// in both cases, and out of range it is still coloured and labelled as an estimate.
		//
		// The column shows the time left, in the studio's own interval form
		// (TimeDateYear.ToIntervalString, "1.25 days" - its commented-out first GetETA returned
		// exactly that), and the tooltip the arrival date as the clock at the top shows dates,
		// which is what the studio's later choice of a date was for.
		label3.NormalColor = inCommRange ? label3.GetNormalColorForType() : Label.LCDErrorColor;
		label2.Text = "";
		label3.Text = "";
		label3.ToolTip = inCommRange ? null : "We are not in communication with the mission.";
		if (mission.GetNextStopAndETA(out var nextStop, out var eta))
		{
			nextStop.Value.ResolveLocation(The.InGameUI.UIAllegiance.SharedKnowledge, out var site, out var _, out var _, out var _);
			double daysLeft = eta.Value.TotalDays - The.Sim.DateAndTime.CurrentTimeDateYear.TotalDays;
			// GetETA divides by the transport's speed, so a transport that cannot move has no
			// arrival: the date it produces is not a date.
			if (site != null && daysLeft >= 0.0 && daysLeft < MaxDaysToShow)
			{
				label2.Text = site.Name;
				label3.Text = new DateAndTime.TimeDateYear(daysLeft).ToIntervalString();
				label3.ToolTip = (inCommRange ? "Estimated time of arrival at " : "We are not in communication with the mission. Estimated from its route and speed: arrival at ")
					+ site.Name + " on " + eta.Value.ToString() + ", " + DateAndTime.GetTimeOfDayAsString(eta.Value.TimeOfDay) + ".";
			}
		}
	}

	/// <summary>Further off than this, an ETA is the result of a transport that is not moving.</summary>
	private const double MaxDaysToShow = 10000.0;
}
