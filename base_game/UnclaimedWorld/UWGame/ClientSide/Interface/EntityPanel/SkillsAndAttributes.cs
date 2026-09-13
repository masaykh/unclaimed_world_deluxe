using Microsoft.Xna.Framework;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using WindowSystem;

namespace UWGame.ClientSide.Interface.EntityPanel;

public class SkillsAndAttributes : EntityPanelTabPage
{
	private FullLCDPanel.TypeIsRepresented EntityHasSkillTypeDelegate;

	private FullLCDPanel.SetCollapsedSummary SetSummaryDelegate;

	protected UIComponent pnAttributes;

	protected Label lblStrength;

	protected Label lblEndurance;

	protected Label lblAppearance;

	protected Label lblAgility;

	protected Label lblPerception;

	protected UIComponent pnSkills;

	protected Grid grdSkillsLeft;

	protected Grid grdSkillsRight;

	protected Regulator displayRegulator;

	public SkillsAndAttributes(GUIManager gui, EntityPanel entityPanel, UIComponent fullLCD, UIComponent halfLCD)
		: base(gui, entityPanel, hasCRT: true, fullLCD, halfLCD)
	{
		Title = "MAIN";
		InitAttributes();
		InitSkills();
	}

	private void InitAttributes()
	{
		pnAttributes = EntityPanelTabPage.CreatePanelWithMargins(gui, 10, lcdContent);
		pnAttributes.RenderType = RenderType.CRTAndLCD;
		Label label = new Label(gui);
		pnAttributes.Add(label);
		label.Y = 5;
		label.Text = "ATTRIBUTES";
		label.Init(Label.LabelType.LCDNormal);
		FullLCDPanel.AddLCDLineThin(gui, new Point(0, 23), pnAttributes.Width, pnAttributes);
		int yPos;
		int num = (yPos = 35);
		AddCaptionAndLabel(pnAttributes, "Strength:", ref lblStrength, 0, ref yPos);
		AddCaptionAndLabel(pnAttributes, "Agility:", ref lblAgility, 0, ref yPos);
		int xPos = 200;
		yPos = num;
		AddCaptionAndLabel(pnAttributes, "Endurance:", ref lblEndurance, xPos, ref yPos);
		AddCaptionAndLabel(pnAttributes, "Appearance:", ref lblAppearance, xPos, ref yPos);
		pnAttributes.Height = yPos + 10;
	}

	private void InitSkills()
	{
		EntityHasSkillTypeDelegate = EntityHasSkillType;
		SetSummaryDelegate = SetSummary;
		pnSkills = EntityPanelTabPage.CreatePanelWithMargins(gui, 10, lcdContent);
		int num = 15;
		int width = (pnSkills.Width - num) / 2;
		Label label = new Label(gui);
		pnSkills.Add(label);
		label.Text = "SKILLS";
		label.Init(Label.LabelType.LCDNormal);
		FullLCDPanel.AddLCDLineThin(gui, new Point(0, 18), pnSkills.Width, pnSkills).DebugTag = "SkillsLine";
		int y = 22;
		grdSkillsLeft = FullLCDPanel.AddTreeGrid(gui, CollapsablePanel.PanelType.DropDownBig);
		grdSkillsLeft.IsOuterGrid = false;
		grdSkillsLeft.Width = width;
		pnSkills.Add(grdSkillsLeft);
		grdSkillsLeft.Y = y;
		grdSkillsLeft.HMargin = 0;
		grdSkillsLeft.HeightResize += grdSkills_HeightResize;
		grdSkillsRight = FullLCDPanel.AddTreeGrid(gui, CollapsablePanel.PanelType.DropDownBig);
		grdSkillsRight.Width = width;
		grdSkillsRight.IsOuterGrid = false;
		pnSkills.Add(grdSkillsRight);
		grdSkillsRight.Y = y;
		grdSkillsRight.HMargin = 0;
		grdSkillsRight.X = grdSkillsLeft.X + grdSkillsLeft.Width + num;
		grdSkillsRight.HeightResize += grdSkills_HeightResize;
	}

	private void grdSkills_HeightResize(UIComponent sender)
	{
		pnSkills.Height = (int)MathHelper.Max(grdSkillsLeft.Y + grdSkillsLeft.Height + 10, grdSkillsRight.Y + grdSkillsRight.Height + 10);
	}

	public void SetSummary(CollapsablePanel cpCategory, object o)
	{
		string text = o.ToString();
		string summary = cpCategory.Summary;
		if (summary == "" || int.Parse(summary) < int.Parse(text))
		{
			cpCategory.Summary = text;
		}
	}

	public bool EntityHasSkillType(object type)
	{
		return entityPanel.SelectedEntity.Intelligence.Skills.ContainsKey((SkillType)type);
	}

	private void PopulateSkills()
	{
		FullLCDPanel.PopulateCategoryGrid<SkillType, Skill, SkillCategory>(gui, grdSkillsLeft, grdSkillsRight, grdSkillsLeft.Width - 30, EntityHasSkillTypeDelegate, SetSummaryDelegate, entityPanel.SelectedEntity.Intelligence.Skills);
	}

	private string AttributeToString(float attribute)
	{
		return ((int)(100f * attribute)).ToString();
	}

	private void PopulateAttributes()
	{
		lblAgility.Text = AttributeToString(entityPanel.SelectedEntity.Locomotor.LeggedLocomotor.Agility);
		lblStrength.Text = AttributeToString(entityPanel.SelectedEntity.Locomotor.LeggedLocomotor.Strength);
		if (entityPanel.SelectedEntity.BiologicalEntity != null)
		{
			lblEndurance.Text = AttributeToString(entityPanel.SelectedEntity.BiologicalEntity.Endurance);
			lblAppearance.Text = AttributeToString(entityPanel.SelectedEntity.BiologicalEntity.Appearance);
		}
		else
		{
			lblEndurance.Text = "n/a";
			lblAppearance.Text = "n/a";
		}
	}

	public override void Refresh()
	{
		lcdContentGrid.BeginAddingEntries();
		if (entityPanel.SelectedEntity.Locomotor.LeggedLocomotor != null)
		{
			if (!lcdContentGrid.EntriesByKey.ContainsKey(pnAttributes))
			{
				lcdContentGrid.AddEntry(pnAttributes, pnAttributes);
			}
			PopulateAttributes();
		}
		else
		{
			lcdContentGrid.TryRemoveEntry(pnAttributes);
		}
		if (entityPanel.SelectedEntity.PersonEntity != null)
		{
			if (!lcdContentGrid.EntriesByKey.ContainsKey(pnSkills))
			{
				lcdContentGrid.AddEntry(pnSkills, pnSkills);
			}
			PopulateSkills();
		}
		else
		{
			lcdContentGrid.TryRemoveEntry(pnSkills);
		}
		lcdContentGrid.EndAddingEntries();
	}
}
