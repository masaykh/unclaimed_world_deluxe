using Microsoft.Xna.Framework;
using UWGame.SimSide;
using WindowSystem;

namespace UWGame.ClientSide.Interface.Ledger;

public class LedgerPanel : RosterPanel
{
	private ComboBox cbSource;

	private ComboBox cbRange;

	private Label lblRange;

	private LedgerSheet currentSheet;

	private TextArea taHelp;

	private const int headerHeight = 80;

	private const int windowWidth = 740;

	private string sheetKey = "sheetKey";

	private int sheetHeight = 600;

	public LedgerPanel()
		: base("LEDGER", 740, The.InGameUI.rosterPanelHeight, needBottomMarginForButton: false)
	{
		int yPosToCenterTo = 12;
		Label label = new Label(Interface.gui);
		lcdSurface.Add(label);
		label.Init(Label.LabelType.LCDHeadingBlue);
		label.Text = "DATA:";
		label.Position = new Point(0, 0);
		label.FitToText();
		label.CenterThisVertically(yPosToCenterTo);
		label.Y++;
		cbSource = new ComboBox(Interface.gui, ListBoxType.LCDCombo, isEditable: false);
		lcdSurface.Add(cbSource);
		cbSource.Init(ComboBoxTypes.LCD);
		cbSource.X = label.Right + 6;
		cbSource.Width = 180;
		cbSource.CenterThisVertically(yPosToCenterTo);
		cbSource.SelectionChanged += cbSource_SelectionChanged;
		lblRange = new Label(Interface.gui);
		lcdSurface.Add(lblRange);
		lblRange.Init(Label.LabelType.LCDHeadingRed);
		lblRange.Text = "RANGE:";
		lblRange.Position = new Point(280, 0);
		lblRange.FitToText();
		lblRange.CenterThisVertically(yPosToCenterTo);
		lblRange.Y++;
		cbRange = new ComboBox(Interface.gui, ListBoxType.LCDCombo, isEditable: false);
		lcdSurface.Add(cbRange);
		cbRange.Init(ComboBoxTypes.LCD);
		cbRange.X = lblRange.Right + 6;
		cbRange.Width = 105;
		cbRange.CenterThisVertically(yPosToCenterTo);
		taHelp = new TextArea(Interface.gui, ListBoxType.LCD);
		taHelp.RenderType = RenderType.CRTAndLCD;
		taHelp.Init(Label.LabelType.LCDNormal);
		taHelp.CanGrowInHeight = true;
		lcdSurface.Add(taHelp);
		taHelp.Y = label.Bottom + 2;
		taHelp.Width = lcdSurface.Width;
		CreateSheets();
		PopulateRangesCombo();
		ShowSheet();
	}

	private void cbSource_SelectionChanged(UIComponent sender)
	{
		ShowSheet();
		Refresh();
	}

	private void ShowSheet()
	{
		LedgerSheet ledgerSheet = cbSource.SelectedKey as LedgerSheet;
		taHelp.Text = ledgerSheet.Tooltip;
		if (currentSheet != null)
		{
			lcdSurface.Remove(currentSheet);
		}
		currentSheet = ledgerSheet;
		lcdSurface.Add(currentSheet);
		currentSheet.Y = 80;
		if (ledgerSheet.ShowRangeSelector)
		{
			cbRange.Visible = true;
			lblRange.Visible = true;
			SetFromDateOnSheet();
		}
		else
		{
			cbRange.Visible = false;
			lblRange.Visible = false;
		}
	}

	private void CreateSheets()
	{
		FoodProduction foodProduction = new FoodProduction(Interface.gui, lcdSurface.Width, lcdSurface.Height - 80);
		cbSource.AddEntry(foodProduction, foodProduction.DisplayName);
		foodProduction.LoadUserSettings(The.InGameUI.FoodProductionSettings.SortingSettings);
		NutrientsSheet nutrientsSheet = new NutrientsSheet(Interface.gui, lcdSurface.Width, lcdSurface.Height - 80);
		cbSource.AddEntry(nutrientsSheet, nutrientsSheet.DisplayName);
		nutrientsSheet.LoadUserSettings(The.InGameUI.NutrientSheetSettings.SortingSettings);
		Production production = new Production(Interface.gui, lcdSurface.Width, lcdSurface.Height - 80);
		cbSource.AddEntry(production, production.DisplayName);
		production.LoadUserSettings(The.InGameUI.ProductionSettings.SortingSettings);
		Kills kills = new Kills(Interface.gui, lcdSurface.Width, lcdSurface.Height - 80);
		cbSource.AddEntry(kills, kills.DisplayName);
		kills.LoadUserSettings(The.InGameUI.KillsSettings.SortingSettings);
		cbSource.SelectionChanged -= cbSource_SelectionChanged;
		cbSource.SelectedIndex = 0;
		cbSource.SelectionChanged += cbSource_SelectionChanged;
	}

	private void PopulateRangesCombo()
	{
		cbRange.AddEntry(GraphPanel.Ranges.OneDay, "One day");
		cbRange.AddEntry(GraphPanel.Ranges.OneSeason, "One season");
		cbRange.AddEntry(GraphPanel.Ranges.OneYear, "One year");
		cbRange.AddEntry(GraphPanel.Ranges.TenYears, "Ten years");
		cbRange.SelectionChanged -= cbRange_SelectionChanged;
		cbRange.SelectedKey = GraphPanel.Ranges.OneYear;
		cbRange.SelectionChanged += cbRange_SelectionChanged;
	}

	private void cbRange_SelectionChanged(UIComponent sender)
	{
		SetFromDateOnSheet();
		Refresh();
	}

	private void SetFromDateOnSheet()
	{
		string fromLabelText;
		DateAndTime.TimeDateYear startPoint = GraphPanel.GetStartPoint((GraphPanel.Ranges)cbRange.SelectedKey, out fromLabelText);
		currentSheet.FromDate = startPoint;
	}

	public override void Refresh()
	{
		base.Refresh();
		currentSheet.RefreshData();
	}

	public override void Show()
	{
		base.Show();
	}
}
