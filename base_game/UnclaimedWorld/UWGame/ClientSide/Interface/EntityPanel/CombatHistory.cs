using Microsoft.Xna.Framework;
using UWGame.SimSide;
using WindowSystem;

namespace UWGame.ClientSide.Interface.EntityPanel;

public class CombatHistory : EntityPanelTabPage
{
	private FullLCDPanel.TypeIsRepresented EntityHasSkillTypeDelegate;

	private FullLCDPanel.SetCollapsedSummary SetSummaryDelegate;

	protected UIComponent pnSummary;

	protected Label lblKills;

	protected Label lblEndurance;

	protected Label lblAppearance;

	protected Label lblAccuracy;

	protected Label lblPerception;

	protected UIComponent pnKills;

	protected Grid grdKills;

	protected Regulator displayRegulator;

	public CombatHistory(GUIManager gui, EntityPanel entityPanel, UIComponent fullLCD, UIComponent halfLCD)
		: base(gui, entityPanel, hasCRT: true, fullLCD, halfLCD)
	{
		Title = "COMBAT";
		InitAttributes();
	}

	private void InitAttributes()
	{
		pnSummary = EntityPanelTabPage.CreatePanelWithMargins(gui, 10, lcdContent);
		pnSummary.RenderType = RenderType.CRTAndLCD;
		Label label = new Label(gui);
		pnSummary.Add(label);
		label.Y = 5;
		label.Text = "COMBAT STATISTICS";
		label.Init(Label.LabelType.LCDNormal);
		FullLCDPanel.AddLCDLineThin(gui, new Point(0, 23), pnSummary.Width, pnSummary);
		int yPos = 35;
		AddCaptionAndLabel(pnSummary, "Kills:", ref lblKills, 0, ref yPos);
		AddCaptionAndLabel(pnSummary, "Accuracy:", ref lblAccuracy, 0, ref yPos);
		pnSummary.Height = yPos + 10;
	}

	private void InitKills()
	{
		pnKills = EntityPanelTabPage.CreatePanelWithMargins(gui, 10, lcdContent);
		int num = 15;
		_ = (pnKills.Width - num) / 2;
		Label label = new Label(gui);
		pnKills.Add(label);
		label.Text = "KILLS";
		label.Init(Label.LabelType.LCDNormal);
		FullLCDPanel.AddLCDLineThin(gui, new Point(0, 18), pnKills.Width, pnKills).DebugTag = "SkillsLine";
		int y = 22;
		grdKills = FullLCDPanel.AddGridWithFixedItemHeights(gui, lcdContent, 30);
		grdKills.Width = 220;
		pnKills.Add(grdKills);
		grdKills.Y = y;
		grdKills.HMargin = 0;
		grdKills.HeightResize += grdSkills_HeightResize;
	}

	private void grdSkills_HeightResize(UIComponent sender)
	{
		pnKills.Height = grdKills.Y + grdKills.Height + 10;
	}

	private void PopulateKills()
	{
		grdKills.BeginAddingEntries();
		grdKills.Clear();
		grdKills.AddEntryRightJustifyValue(null, null, null, null, "Night piper", 0, "2");
		grdKills.AddEntryRightJustifyValue(null, null, null, null, "Tree dragon", 0, "1");
		grdKills.AddEntryRightJustifyValue(null, null, null, null, "Hecatonth", 0, "1");
		grdKills.EndAddingEntries();
	}

	private string AttributeToString(float attribute)
	{
		return ((int)(100f * attribute)).ToString();
	}

	private void PopulateSummary()
	{
		lblAccuracy.Text = "89 %";
		lblKills.Text = "4      Night piper (2), Tree dragon (1), Hecatonth (1)";
	}

	public override void Refresh()
	{
		lcdContentGrid.BeginAddingEntries();
		if (entityPanel.SelectedEntity.Intelligence != null)
		{
			if (!lcdContentGrid.EntriesByKey.ContainsKey(pnSummary))
			{
				lcdContentGrid.AddEntry(pnSummary, pnSummary);
			}
			PopulateSummary();
		}
		else
		{
			lcdContentGrid.TryRemoveEntry(pnSummary);
			lcdContentGrid.TryRemoveEntry(pnKills);
		}
		lcdContentGrid.EndAddingEntries();
	}
}
