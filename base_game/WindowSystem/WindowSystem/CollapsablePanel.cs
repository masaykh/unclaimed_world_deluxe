using System;
using Microsoft.Xna.Framework;

namespace WindowSystem;

public class CollapsablePanel : UIComponent
{
	public enum PanelType
	{
		Node,
		DropDownBig,
		HUD,
		StockpileHUD,
		Panel,
		DropDownSmall,
		HUDSmall
	}

	public delegate void ExpandHandler();

	public enum SubType
	{
		Normal,
		Blue,
		Green,
		Red
	}

	public const int HUD_textbox_CornerSize = 8;

	public const int HUD_button_CornerSize = 9;

	public bool isExpanded;

	protected PanelType type;

	private ImageButton expandButton;

	private Icon expandIcon;

	private Label lblTitle;

	public Label lblTitleSummary;

	private TextButton headerbox;

	public Box ExpandedPanel;

	private Image icon;

	private const int iconX = 12;

	public int CollapsedHeight = 14;

	public int ExpandedPanelYPos = 20;

	private int expandedPanelTopPadding;

	public int CollapsablePanelRightPadding;

	private int expandedPanelHorizMargin;

	private const int headingXPosDropDown = 30;

	private const int headingXPosNode = 36;

	private const int headingSummaryRightPaddingDropDown = 30;

	private const int headingSummaryRightPaddingNode = 14;

	private const string hudCollapsedArrowSprite = "HUD_rightarrow";

	private const string lcdCollapsedArrowSprite = "lcd_rightarrow";

	private const string lcdExpandedArrowSprite = "lcd_downarrow";

	public int HeadingYPos = 3;

	public int? TitleSummaryRightAlignXPos;

	public string TitleTooltip
	{
		set
		{
			lblTitle.ToolTip = value;
		}
	}

	public bool IsExpanded
	{
		get
		{
			return isExpanded;
		}
		set
		{
			if (isExpanded != value)
			{
				if (value)
				{
					Expand();
				}
				else
				{
					Collapse();
				}
				isExpanded = value;
			}
		}
	}

	public int TitlePositionX
	{
		set
		{
			lblTitle.X = value;
		}
	}

	public string Title
	{
		get
		{
			return lblTitle.Text;
		}
		set
		{
			lblTitle.Text = value;
			lblTitle.Width = lblTitle.TextWidth;
			lblTitle.Height = lblTitle.TextHeight;
		}
	}

	public string Summary
	{
		get
		{
			return lblTitleSummary.Text;
		}
		set
		{
			lblTitleSummary.Text = value;
			lblTitleSummary.Width = lblTitleSummary.TextWidth;
			lblTitleSummary.Height = lblTitleSummary.TextHeight;
			RightAlignTitleSummary();
		}
	}

	public override int Width
	{
		get
		{
			return base.Width;
		}
		set
		{
			base.Width = value;
			ExpandedPanel.Width = value - CollapsablePanelRightPadding - 2 * expandedPanelHorizMargin;
			if (type == PanelType.DropDownBig || type == PanelType.DropDownSmall || type == PanelType.HUDSmall)
			{
				RightAlignTitleSummary();
				SetExpandButtonPosition();
			}
			if (headerbox != null)
			{
				headerbox.Width = value - CollapsablePanelRightPadding;
			}
		}
	}

	public event ExpandHandler ExpandEvent;

	public CollapsablePanel(GUIManager guiManager, PanelType type)
		: base(guiManager)
	{
		this.type = type;
		base.CanHaveFocus = false;
		switch (type)
		{
		case PanelType.DropDownBig:
		case PanelType.StockpileHUD:
		case PanelType.DropDownSmall:
		case PanelType.HUDSmall:
			expandIcon = new Icon(guiManager);
			expandIcon.ScaleImageToSizeOfControl = false;
			expandIcon.CanHaveFocus = false;
			break;
		default:
			expandButton = new ImageButton(guiManager);
			expandButton.DebugTag = "expandButton";
			expandButton.ZOrder = 1f;
			expandButton.Click += expandButton_Click;
			break;
		case PanelType.Panel:
			break;
		}
		ExpandedPanel = new Box(guiManager);
		lblTitle = new Label(guiManager);
		lblTitle.DebugTag = "collapseTitle";
		if (type == PanelType.DropDownBig || type == PanelType.StockpileHUD || type == PanelType.DropDownSmall || type == PanelType.HUDSmall)
		{
			headerbox = new TextButton(guiManager);
			Add(headerbox);
		}
		switch (type)
		{
		case PanelType.DropDownBig:
		case PanelType.StockpileHUD:
		case PanelType.DropDownSmall:
		case PanelType.HUDSmall:
			Add(expandIcon);
			break;
		default:
			Add(expandButton);
			break;
		case PanelType.Panel:
			break;
		}
		Add(lblTitle);
		ExpandedPanel.CanHaveFocus = false;
	}

	private void expandButton_Click(UIComponent sender, EventArgs e)
	{
		if (!isExpanded)
		{
			IsExpanded = true;
			if (this.ExpandEvent != null)
			{
				this.ExpandEvent();
			}
		}
		else
		{
			IsExpanded = false;
		}
	}

	public void AddContent(UIComponent content, bool hookToResize = true)
	{
		ExpandedPanel.Add(content);
		content.Y = expandedPanelTopPadding;
		if (hookToResize)
		{
			content.Resize += content_Resize;
		}
	}

	private void content_Resize(UIComponent sender)
	{
		ExpandedPanel.Height = sender.Bottom;
		if (isExpanded)
		{
			Height = ExpandedPanelYPos + ExpandedPanel.Height;
		}
		else
		{
			Height = CollapsedHeight;
		}
	}

	private void Collapse()
	{
		Height = CollapsedHeight;
		Remove(ExpandedPanel);
		if (type == PanelType.Node)
		{
			expandButton.Init(ImageButtonType.LCDExpand);
		}
		else if (type == PanelType.DropDownSmall)
		{
			SetIconCollapsed("lcd_rightarrow");
		}
		else if (type == PanelType.DropDownBig)
		{
			SetIconCollapsed("lcd_rightarrow");
		}
		else if (type == PanelType.HUD)
		{
			expandButton.Init(ImageButtonType.HUDArrowRight);
		}
		else if (type == PanelType.HUDSmall)
		{
			SetIconCollapsed("HUD_rightarrow");
		}
	}

	private void SetIconCollapsed(string sprite)
	{
		expandIcon.SetSkinLocation(SkinState.Normal, guiManager.GUISpriteSheet.GetSourceRectangle(sprite));
		expandIcon.ResizeControlToFitImage();
		if (type == PanelType.HUDSmall)
		{
			expandIcon.Y = 4;
		}
	}

	private void SetIconExpanded(string sprite)
	{
		expandIcon.SetSkinLocation(SkinState.Normal, guiManager.GUISpriteSheet.GetSourceRectangle(sprite));
		expandIcon.ResizeControlToFitImage();
		if (type == PanelType.HUDSmall)
		{
			expandIcon.Y = 6;
		}
	}

	private void Expand()
	{
		Height = ExpandedPanelYPos + ExpandedPanel.Height;
		Add(ExpandedPanel);
		ExpandedPanel.Y = ExpandedPanelYPos;
		if (type == PanelType.Node)
		{
			expandButton.Init(ImageButtonType.LCDCollapse);
		}
		else if (type == PanelType.DropDownSmall)
		{
			SetIconExpanded("lcd_downarrow");
		}
		else if (type == PanelType.DropDownBig)
		{
			SetIconExpanded("lcd_downarrow");
		}
		else if (type == PanelType.HUD)
		{
			expandButton.Init(ImageButtonType.HUDArrowDown);
		}
		else if (type == PanelType.HUDSmall)
		{
			SetIconExpanded("HUD_downarrow");
		}
	}

	private void SetExpandButtonPosition()
	{
		UIComponent uIComponent = expandButton ?? expandIcon;
		if (uIComponent != null)
		{
			if (type == PanelType.HUDSmall)
			{
				uIComponent.X = Width - 70;
			}
			else
			{
				uIComponent.X = Width - 24;
			}
		}
	}

	public void SetIcon(string spriteName)
	{
		if (icon == null)
		{
			icon = new Image(guiManager);
			Add(icon);
			icon.X = 12;
		}
		Rectangle sourceRectangle = guiManager.GUISpriteSheet.GetSourceRectangle(spriteName);
		icon.SetSkinLocation(SkinState.Normal, sourceRectangle);
		icon.ResizeControlToFitImage();
		headerbox.CenterChildVertically(icon);
	}

	private void RightAlignTitleSummary()
	{
		if (TitleSummaryRightAlignXPos.HasValue)
		{
			lblTitleSummary.AlignRight(TitleSummaryRightAlignXPos.Value);
		}
		else if (type == PanelType.DropDownBig || type == PanelType.DropDownSmall || type == PanelType.Node)
		{
			lblTitleSummary.X = Width - 30 - lblTitleSummary.TextWidth;
		}
	}

	public int GetPaddingRight()
	{
		if (type == PanelType.DropDownBig)
		{
			return 30;
		}
		return 14;
	}

	public void RightJustifyLabel(Label lblTitleSummary)
	{
		if (type == PanelType.DropDownBig || type == PanelType.DropDownSmall)
		{
			lblTitleSummary.X = Width - 30 - lblTitleSummary.TextWidth;
		}
		else
		{
			lblTitleSummary.X = Width - 14 - lblTitleSummary.TextWidth;
		}
	}

	public void CenterOnHeader(UIComponent control)
	{
		headerbox.CenterChildVertically(control);
	}

	public void Init(SubType subtype = SubType.Normal)
	{
		lblTitleSummary = new Label(guiManager);
		Add(lblTitleSummary);
		switch (type)
		{
		case PanelType.HUD:
			expandButton.Init(ImageButtonType.HUDArrowRight);
			expandButton.Position = new Point(0, HeadingYPos);
			lblTitle.Init(Label.LabelType.HUDWindow);
			lblTitle.Position = new Point(30, HeadingYPos);
			lblTitleSummary.Init(Label.LabelType.HUDWindow);
			break;
		case PanelType.Node:
			expandButton.Init(ImageButtonType.LCDExpand);
			expandButton.Position = new Point(0, HeadingYPos);
			lblTitle.Init(Label.LabelType.LCDNormal);
			lblTitle.Position = new Point(30, HeadingYPos);
			lblTitleSummary.Init(Label.LabelType.LCDNormal);
			break;
		case PanelType.DropDownSmall:
		{
			Label.LabelType labelType2 = Label.LabelType.LCDNormalLight;
			HeadingYPos = 5;
			lblTitle.Init(labelType2);
			lblTitle.Position = new Point(13, HeadingYPos);
			headerbox.Init(TextButton.TextButtonType.LCDCollapsableHeaderSmall);
			SetIconCollapsed("lcd_rightarrow");
			headerbox.CenterChildVertically(expandIcon);
			expandIcon.Y++;
			SetExpandButtonPosition();
			headerbox.Click += expandButton_Click;
			lblTitleSummary.Init(labelType2);
			ExpandedPanelYPos = headerbox.Height - 2;
			break;
		}
		case PanelType.DropDownBig:
		{
			Label.LabelType labelType = Label.LabelType.LCDNormalLight;
			HeadingYPos = 8;
			lblTitle.Init(labelType);
			lblTitle.Position = new Point(30, HeadingYPos);
			headerbox.Init(subtype switch
			{
				SubType.Normal => TextButton.TextButtonType.LCDCollapsableHeaderBig, 
				SubType.Green => TextButton.TextButtonType.LCDCollapsableHeaderBigGreen, 
				SubType.Blue => TextButton.TextButtonType.LCDCollapsableHeaderBigBlue, 
				SubType.Red => TextButton.TextButtonType.LCDCollapsableHeaderBigRed, 
				_ => TextButton.TextButtonType.LCDCollapsableHeaderBig, 
			});
			SetIconCollapsed("lcd_rightarrow");
			headerbox.CenterChildVertically(expandIcon);
			expandIcon.Y++;
			SetExpandButtonPosition();
			headerbox.Click += expandButton_Click;
			lblTitleSummary.Init(labelType);
			ExpandedPanelYPos = headerbox.Bottom - 5;
			expandedPanelTopPadding = 6;
			float num = 0.5f;
			Color value = new Color(num, num, num, num);
			Rectangle sourceRectangle3 = guiManager.GUISpriteSheet.GetSourceRectangle("lcd_panel_background");
			ExpandedPanel.SetSkinLocation(SkinState.Normal, sourceRectangle3, value, value);
			ExpandedPanel.CornerSize = 6;
			expandedPanelHorizMargin = 6;
			break;
		}
		case PanelType.StockpileHUD:
		{
			Remove(expandButton);
			headerbox.Init(TextButton.TextButtonType.HUD);
			headerbox.Height = 31;
			headerbox.Click += expandButton_Click;
			ExpandedPanelYPos = headerbox.Height - 3;
			CollapsablePanelRightPadding = 6;
			lblTitle.Init(Label.LabelType.HUDWindow);
			lblTitle.Position = new Point(20, HeadingYPos);
			headerbox.CenterChildVertically(lblTitle);
			lblTitleSummary.Init(Label.LabelType.HUDWindow);
			headerbox.CenterChildVertically(lblTitleSummary);
			HeadingYPos = lblTitle.Y;
			Rectangle sourceRectangle2 = guiManager.GUISpriteSheet.GetSourceRectangle("HUD_gradient");
			ExpandedPanel.SetSkinLocation(SkinState.Normal, sourceRectangle2);
			ExpandedPanel.CornerSize = 6;
			expandedPanelHorizMargin = 3;
			break;
		}
		case PanelType.HUDSmall:
		{
			headerbox.Init(TextButton.TextButtonType.HUD);
			headerbox.Height = 27;
			headerbox.Click += expandButton_Click;
			ExpandedPanelYPos = headerbox.Height - 3;
			CollapsablePanelRightPadding = 6;
			lblTitle.Init(Label.LabelType.HUDWindow);
			lblTitle.Position = new Point(20, HeadingYPos);
			headerbox.CenterChildVertically(lblTitle);
			lblTitleSummary.Init(Label.LabelType.HUDWindow);
			headerbox.CenterChildVertically(lblTitleSummary);
			HeadingYPos = lblTitle.Y;
			Rectangle sourceRectangle = guiManager.GUISpriteSheet.GetSourceRectangle("HUD_gradient");
			ExpandedPanel.SetSkinLocation(SkinState.Normal, sourceRectangle);
			ExpandedPanel.CornerSize = 6;
			expandedPanelHorizMargin = 3;
			SetIconCollapsed("HUD_rightarrow");
			headerbox.CenterChildVertically(expandIcon);
			SetExpandButtonPosition();
			break;
		}
		case PanelType.Panel:
			lblTitle.Init(Label.LabelType.LCDNormal);
			lblTitle.Position = new Point(30, HeadingYPos);
			break;
		}
		lblTitleSummary.Y = HeadingYPos;
		ExpandedPanel.Width = Width;
		ExpandedPanel.X = expandedPanelHorizMargin;
		ExpandedPanel.Y = ExpandedPanelYPos;
		Collapse();
	}

	private void headerbox_Click(UIComponent sender, EventArgs e)
	{
	}
}
