using System;
using System.Collections.Generic;
using System.Linq;
using UWGame.ClientSide.Interface.LCD;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Communication;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

public class DiplomacyPanel : RosterPanel
{
	private Grid grid;

	private const int itemHeight = 36;

	private const int horizPadding = 6;

	private const int vertPadding = 4;

	private List<AllegianceRelation> allAllegiancesToShow = new List<AllegianceRelation>();

	private const int headingYPos = 30;

	private const int locationColumnX = 140;

	private const int immigrationColumnX = 260;

	private const int communicationColumnX = 380;

	public DiplomacyPanel()
		: base("CONTACTS", 600, needBottomMarginForButtons: false)
	{
		Panel.CreateGridWithColumnHeadings(lcdSurface, 0, 36, out grid, 0, new Tuple<string, int>("NAME", 0), new Tuple<string, int>("LOCATION", 140), new Tuple<string, int>("MIGRTE FACTOR", 260), new Tuple<string, int>("COMMUNICATION", 380));
	}

	private void Populate()
	{
		GetAllAllegiancesToShow();
		double overallRating = The.InGameUI.UIAllegiance.Statistics.GetOverallRating();
		grid.BeginAddingEntries();
		foreach (AllegianceRelation item2 in allAllegiancesToShow)
		{
			if (!grid.TryGetEntry(item2, out var item))
			{
				item = AddItemRow(item2, item2, overallRating);
			}
			UpdateRow(item2, item, overallRating);
		}
		grid.DeleteEntries((AllegianceRelation j) => allAllegiancesToShow.Contains(j));
		grid.EndAddingEntries();
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

	private void GetAllAllegiancesToShow()
	{
		allAllegiancesToShow.Clear();
		if (The.Sim.World.Relations.TryGetValue(The.InGameUI.UIAllegiance.ID, out var value))
		{
			allAllegiancesToShow.AddRange(value);
		}
		allAllegiancesToShow = allAllegiancesToShow.OrderBy((AllegianceRelation j) => j.AllegianceB.Site.Name).ToList();
	}

	private UIComponent AddItemRow(object key, AllegianceRelation relation, double currentAllegianceRating)
	{
		UIComponent uIComponent = new UIComponent(Interface.gui);
		uIComponent.Height = 36;
		LCDInnerPanel lCDInnerPanel = new LCDInnerPanel(Interface.gui, grid.Width, includeDecor: false);
		uIComponent.Add(lCDInnerPanel.Panel);
		lCDInnerPanel.HorizontalContentPadding = 6;
		lCDInnerPanel.VerticalContentPadding = 4;
		Label label = new Label(Interface.gui);
		lCDInnerPanel.AddContent(label);
		label.Width = 200;
		label.Init(Label.LabelType.LCDNormal);
		label.ID = UIComponent.DataControlID.Caption;
		uIComponent.CenterChildVertically(label);
		Label label2 = new Label(Interface.gui);
		lCDInnerPanel.AddContent(label2, 133);
		label2.Text = relation.AllegianceB.Site.Name;
		label2.Width = 200;
		label2.Init(Label.LabelType.LCDNormal);
		label2.ID = UIComponent.DataControlID.Location;
		uIComponent.CenterChildVertically(label2);
		Label label3 = new Label(Interface.gui);
		lCDInnerPanel.AddContent(label3, 253);
		label3.Width = 20;
		label3.Init(Label.LabelType.LCDNormal);
		label3.ID = UIComponent.DataControlID.Immigration;
		uIComponent.CenterChildVertically(label3);
		Label label4 = new Label(Interface.gui);
		lCDInnerPanel.AddContent(label4, 373);
		label4.Width = 120;
		label4.Init(Label.LabelType.LCDNormal);
		label4.ID = UIComponent.DataControlID.Communication;
		uIComponent.CenterChildVertically(label4);
		grid.AddEntry(relation, uIComponent);
		return uIComponent;
	}

	private static void SetMigrationFactor(AllegianceRelation relation, Label lblMigration, double thisAllegianceRating)
	{
		double overallRating = relation.AllegianceB.Statistics.GetOverallRating();
		lblMigration.Text = (thisAllegianceRating - overallRating).ToString("F2");
		lblMigration.ToolTip = "Shows the overall migration pull factor between this allegiance and our own. If the number is positive, we could receive more migrants.";
	}

	public static void DisplayCommunication(ICommunicates commA, ICommunicates commB, Label lblCommunication, string inCommTooltip, string noCommTooltip, out bool inCommRange)
	{
		if (Communicates.IsInCommunicationRange(commA, commB, out var workingMethod))
		{
			lblCommunication.Text = "IN COMM RANGE";
			lblCommunication.ToolTip = string.Format(inCommTooltip, CommunicatorType.GetName(workingMethod.Value));
			lblCommunication.NormalColor = lblCommunication.GetNormalColorForType();
			inCommRange = true;
		}
		else
		{
			lblCommunication.Text = "NO COMM";
			lblCommunication.ToolTip = noCommTooltip;
			lblCommunication.NormalColor = Label.LCDErrorColor;
			inCommRange = false;
		}
	}

	private void UpdateRow(AllegianceRelation relation, UIComponent itemRow, double currentAllegianceRating)
	{
		(itemRow.FindChildById(UIComponent.DataControlID.Caption) as Label).Text = relation.AllegianceB.Name;
		(itemRow.FindChildById(UIComponent.DataControlID.Location) as Label).Text = relation.AllegianceB.Site.Name;
		Label lblCommunication = itemRow.FindChildById(UIComponent.DataControlID.Communication) as Label;
		DisplayCommunication(The.InGameUI.UIAllegiance, relation.AllegianceB, lblCommunication, "The allegiance can be reached with the communication equipment ({0}) that is currently deployed", "We currently have no way to communicate with the allegiance.", out var _);
		Label lblMigration = itemRow.FindChildById(UIComponent.DataControlID.Immigration) as Label;
		SetMigrationFactor(relation, lblMigration, currentAllegianceRating);
	}
}
