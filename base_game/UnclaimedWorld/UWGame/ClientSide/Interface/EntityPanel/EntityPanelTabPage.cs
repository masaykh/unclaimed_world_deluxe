using Microsoft.Xna.Framework;
using WindowSystem;

namespace UWGame.ClientSide.Interface.EntityPanel;

public class EntityPanelTabPage
{
	public bool HasCRT;

	protected GUIManager gui;

	public UIComponent lcdContent;

	protected Grid lcdContentGrid;

	public Panel tabPanel;

	public string Title = "";

	protected EntityPanel entityPanel;

	protected const int captionWidth = 100;

	protected const int lineHeight = 18;

	protected const int leftMargin = 10;

	public EntityPanelTabPage(GUIManager gui, EntityPanel entityPanel, bool hasCRT, UIComponent fullLCD, UIComponent halfLCD)
	{
		HasCRT = hasCRT;
		this.gui = gui;
		this.entityPanel = entityPanel;
		if (fullLCD != null)
		{
			lcdContent = new UIComponent(gui);
			lcdContent.RenderType = RenderType.CRTAndLCD;
			if (HasCRT)
			{
				lcdContent.Width = halfLCD.Width;
				lcdContent.Height = halfLCD.Height;
			}
			else
			{
				lcdContent.Width = fullLCD.Width;
				lcdContent.Height = fullLCD.Height;
			}
			lcdContentGrid = CreateOuterGridForContent(gui, lcdContent);
		}
	}

	public virtual void Refresh()
	{
	}

	public virtual void Clear()
	{
		lcdContentGrid.Clear();
	}

	public static UIComponent CreatePanelWithMargins(GUIManager gui, int horizMargin, UIComponent lcdSurface)
	{
		return new UIComponent(gui)
		{
			Width = FullLCDPanel.GetContentWidthFromLCDSurface(lcdSurface) - 2 * horizMargin,
			X = horizMargin,
			RenderType = RenderType.CRTAndLCD
		};
	}

	public void AddCaptionAndLabel(UIComponent pnPanel, string caption, ref Label lblValue, int xPos, ref int yPos)
	{
		Label label = new Label(gui);
		pnPanel.Add(label);
		label.Text = caption;
		label.Init(Label.LabelType.LCDNormal);
		label.Position = new Point(xPos, yPos);
		lblValue = new Label(gui);
		pnPanel.Add(lblValue);
		lblValue.Init(Label.LabelType.LCDNormal);
		lblValue.Position = new Point(xPos + 100, yPos);
		lblValue.Width = 280;
		yPos += 18;
	}

	public static Grid CreateOuterGridForContent(GUIManager gui, UIComponent lcdSurface)
	{
		int num = 4;
		Grid grid = new Grid(gui, ListBoxType.Main, Label.LabelType.LCDNormal);
		grid.FixedItemHeights = false;
		grid.RenderType = RenderType.CRTAndLCD;
		lcdSurface.Add(grid);
		grid.HMargin = 0;
		grid.VMargin = 0;
		grid.Font = GUIManager.LCDandHUDBodyFontPath;
		grid.Width = lcdSurface.Width;
		grid.Height = lcdSurface.Height - num;
		grid.ItemHeight = 26;
		grid.Position = new Point(0, num);
		return grid;
	}
}
