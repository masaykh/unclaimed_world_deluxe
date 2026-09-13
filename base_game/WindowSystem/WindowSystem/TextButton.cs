using System;
using InputEventSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace WindowSystem;

public class TextButton : UIComponent, ICanBeChecked
{
	public enum TextButtonType
	{
		LCD,
		LCDSliderButton,
		LCDSliderButtonWhite,
		LCDToolTipBlack,
		HUDSliderButton,
		HUDToolTipWhite,
		LCDCombo,
		LCDAmount,
		White,
		Black,
		BlackSlim,
		Hyperlink,
		HyperlinkBlack,
		HUD,
		HUDHasState,
		HUDGather,
		HUDStockpile,
		HUDItemQuantity,
		HUDScout,
		HUDHunt,
		HUDPatrol,
		HUDForage,
		LCDCollapsableHeaderBig,
		LCDCollapsableHeaderBigGreen,
		LCDCollapsableHeaderBigBlue,
		LCDCollapsableHeaderBigRed,
		LCDCollapsableHeaderSmall,
		LCDSortingArrows,
		HUDDiscard,
		HUDClaim,
		HUDSalvage,
		HUDPackingDown,
		HUDSliderButtonWhite,
		HUDAttack,
		HUDUpgrade
	}

	public enum TextAlign
	{
		Center,
		Left
	}

	private static int defaultWidth = 65;

	private static int defaultHeight = 12;

	private static int defaultEdgeSize = 12;

	private static Rectangle defaultSkin = new Rectangle(1, 142, 25, 25);

	private static Rectangle defaultHoverSkin = new Rectangle(27, 142, 25, 25);

	private static Rectangle defaultPressedSkin = new Rectangle(52, 142, 25, 25);

	private static Rectangle defaultHoverSkinNonEnabled = new Rectangle(1, 142, 25, 25);

	private const int hudButtonHeight = 25;

	protected Box buttonBox;

	protected Label label;

	protected int yLabelOffset;

	private SoundEffect clickedSound;

	private Icon icon;

	protected bool isChecked;

	private CheckedModes checkedMode;

	private int rightButtonPadding = 2;

	private int leftButtonPadding = 2;

	private TextAlign textAlignment;

	public static int DefaultWidth
	{
		set
		{
			defaultWidth = value;
		}
	}

	public static int DefaultHeight
	{
		set
		{
			defaultHeight = value;
		}
	}

	public static int DefaultEdgeSize
	{
		set
		{
			defaultEdgeSize = value;
		}
	}

	public static Rectangle DefaultSkin
	{
		set
		{
			defaultSkin = value;
		}
	}

	public static Rectangle DefaultHoverSkin
	{
		set
		{
			defaultHoverSkin = value;
		}
	}

	public static Rectangle DefaultPressedSkin
	{
		set
		{
			defaultPressedSkin = value;
		}
	}

	public TextButtonType Type { get; protected set; }

	public bool HasState => checkedMode != CheckedModes.CannotBeChecked;

	public CheckedModes CheckedMode
	{
		get
		{
			return checkedMode;
		}
		set
		{
			checkedMode = value;
		}
	}

	public TextAlign TextAlignment
	{
		get
		{
			return textAlignment;
		}
		set
		{
			if (textAlignment != value)
			{
				textAlignment = value;
				RefreshLabelPosition();
			}
		}
	}

	public Rectangle Skin
	{
		set
		{
			buttonBox.SetSkinLocation(SkinState.Normal, value);
		}
	}

	public Rectangle HoverSkin
	{
		set
		{
			buttonBox.SetSkinLocation(1, value);
		}
	}

	public Rectangle HoverSkinNonEnabled
	{
		set
		{
			buttonBox.SetSkinLocation(1, value);
		}
	}

	public Rectangle PressedSkin
	{
		set
		{
			buttonBox.SetSkinLocation(SkinState.Pressed, value);
		}
	}

	public Rectangle DisabledSkin
	{
		set
		{
			buttonBox.SetSkinLocation(SkinState.Disabled, value, UIComponent.lcdDisabledColor, UIComponent.lcdDisabledColor);
		}
	}

	public int CornerSize
	{
		get
		{
			return buttonBox.CornerSize;
		}
		set
		{
			buttonBox.CornerSize = value;
		}
	}

	public string Text
	{
		get
		{
			return label.Text;
		}
		set
		{
			label.Text = value;
			if (Type == TextButtonType.Hyperlink)
			{
				Width = label.TextWidth;
				Height = label.TextHeight;
			}
			else
			{
				RefreshLabelPosition();
			}
		}
	}

	public SpriteFont Font
	{
		set
		{
			label.Font = value;
			RefreshLabelPosition();
		}
	}

	public Color LabelColor
	{
		get
		{
			return label.NormalColor;
		}
		set
		{
			label.NormalColor = value;
		}
	}

	public Color NormalColor
	{
		set
		{
			buttonBox.SetSkinLocation(SkinState.Normal, null, value, value);
		}
	}

	public override bool Enabled
	{
		get
		{
			return base.Enabled;
		}
		set
		{
			if (base.Enabled != value)
			{
				base.Enabled = value;
				label.Enabled = value;
				if (base.Enabled)
				{
					buttonBox.CurrentSkinState = SkinState.Normal;
				}
				else
				{
					buttonBox.CurrentSkinState = SkinState.Disabled;
				}
			}
		}
	}

	public int TextWidth => label.TextWidth;

	public int TextHeight => label.TextHeight;

	public bool Pressed
	{
		get
		{
			return base.IsPressed;
		}
		set
		{
			base.IsPressed = value;
			if (value)
			{
				buttonBox.CurrentSkinState = SkinState.Pressed;
			}
			else
			{
				buttonBox.CurrentSkinState = SkinState.Normal;
			}
		}
	}

	public bool IsChecked
	{
		get
		{
			return isChecked;
		}
		set
		{
			if (isChecked != value)
			{
				isChecked = value;
				buttonBox.CurrentSkinState = SkinState.Normal;
				if (isChecked)
				{
					buttonBox.CurrentSkinState = SkinState.Checked;
				}
			}
		}
	}

	public TextButton(GUIManager guiManager)
		: base(guiManager)
	{
		buttonBox = new Box(guiManager);
		label = new Label(guiManager);
		Add(buttonBox);
		Add(label);
		base.MinWidth = defaultHeight;
		MinHeight = defaultHeight;
		base.CanHaveFocus = true;
	}

	public void Init(TextButtonType type)
	{
		Init(type, -1);
	}

	public void Init(TextButtonType type, int flavour)
	{
		Type = type;
		switch (type)
		{
		case TextButtonType.White:
			InitMainButton("basic_buttonwhite_out", "basic_buttonwhite_hover", "basic_buttonwhite_in", Color.Black);
			break;
		case TextButtonType.Black:
			InitMainButton("smallblackbutton_out", "smallblackbutton_hover", "smallblackbutton_in", Color.WhiteSmoke);
			break;
		case TextButtonType.BlackSlim:
			InitMainButton("smallblackbutton_out", "smallblackbutton_hover", "smallblackbutton_in", Color.WhiteSmoke, slim: true);
			break;
		case TextButtonType.Hyperlink:
			throw new Exception("Not Used!");
		case TextButtonType.LCD:
		{
			label.Init(Label.LabelType.LCDNormal);
			leftButtonPadding = 6;
			rightButtonPadding = 8;
			Rectangle sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle("basic_button_light");
			Width = sourceRectangle.Width;
			Height = sourceRectangle.Height;
			CornerSize = 11;
			Skin = sourceRectangle;
			buttonBox.SetSkinLocation(SkinState.Hover, sourceRectangle, UIComponent.lcdTooltipHoverTint, UIComponent.lcdTooltipHoverTint);
			sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle("basic_button_light_in");
			PressedSkin = sourceRectangle;
			label.InitDisabledColor(UIComponent.lcdDisabledColor, LabelColor);
			break;
		}
		case TextButtonType.LCDSliderButton:
		{
			Rectangle sourceRectangle = InitLCDSliderButton("lcd_bar_knob");
			break;
		}
		case TextButtonType.LCDSliderButtonWhite:
		{
			Rectangle sourceRectangle = InitLCDSliderButton("lcd_bar_knob_white");
			break;
		}
		case TextButtonType.LCDSortingArrows:
		{
			Rectangle sourceRectangle4 = guiManager.GUISpriteSheet.GetSourceRectangle("basic_icon_sortingArrow_down");
			icon = new Icon(guiManager);
			icon.SetSkinLocation(SkinState.Normal, sourceRectangle4);
			sourceRectangle4 = guiManager.GUISpriteSheet.GetSourceRectangle("basic_icon_sortingArrow_up");
			icon.SetSkinLocation(1, sourceRectangle4);
			icon.ResizeControlToFitImage();
			icon.CurrentSkin = 0;
			icon.Visible = false;
			Add(icon);
			icon.X = Width - icon.Width - 12;
			icon.Y = 8;
			icon.CanHaveFocus = false;
			InitInventory();
			break;
		}
		case TextButtonType.LCDAmount:
		{
			label.Init(Label.LabelType.LCDWhite);
			leftButtonPadding = 6;
			rightButtonPadding = 8;
			Rectangle sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle("lcd_amountbutton");
			Width = sourceRectangle.Width;
			Height = sourceRectangle.Height;
			CornerSize = 9;
			Skin = sourceRectangle;
			buttonBox.SetSkinLocation(SkinState.Hover, sourceRectangle, UIComponent.lcdTooltipHoverTint, UIComponent.lcdTooltipHoverTint);
			sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle("lcd_zeroamount_bg");
			buttonBox.SetSkinLocation(SkinState.Disabled, sourceRectangle);
			break;
		}
		case TextButtonType.LCDToolTipBlack:
		{
			Rectangle sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle("lcd_infobutton");
			Width = sourceRectangle.Width;
			Height = sourceRectangle.Height;
			CornerSize = 5;
			leftButtonPadding = 6;
			rightButtonPadding = 6;
			Skin = sourceRectangle;
			buttonBox.SetSkinLocation(SkinState.Hover, sourceRectangle, UIComponent.lcdTooltipHoverTint, UIComponent.lcdTooltipHoverTint);
			buttonBox.SetSkinLocation(SkinState.Pressed, sourceRectangle, UIComponent.lcdTooltipPressedTint, UIComponent.lcdTooltipPressedTint);
			buttonBox.SetSkinLocation(SkinState.Checked, sourceRectangle, UIComponent.lcdTooltipCheckedTint, UIComponent.lcdTooltipCheckedTint);
			buttonBox.SetSkinLocation(SkinState.CheckedPressed, sourceRectangle, UIComponent.lcdTooltipCheckedPressedTint, UIComponent.lcdTooltipCheckedPressedTint);
			buttonBox.SetSkinLocation(SkinState.CheckedHover, sourceRectangle, UIComponent.lcdTooltipHoverTint, UIComponent.lcdTooltipHoverTint);
			buttonBox.SetSkinLocation(SkinState.HoverDisabled, sourceRectangle, UIComponent.hudHoverTintNonEnabled, UIComponent.hudHoverTintNonEnabled);
			CheckedMode = CheckedModes.CanBeChecked;
			Font = GUIManager.LCDandHUDFont;
			LabelColor = UIComponent.LCDDark;
			label.InitDisabledColor(UIComponent.lcdDisabledColor, LabelColor);
			break;
		}
		case TextButtonType.HUDToolTipWhite:
		{
			Rectangle sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle("HUD_info_button_lessTransparent");
			Width = sourceRectangle.Width;
			Height = 25;
			CornerSize = 5;
			leftButtonPadding = 6;
			rightButtonPadding = 6;
			Skin = sourceRectangle;
			buttonBox.SetSkinLocation(SkinState.Hover, sourceRectangle, UIComponent.lcdTooltipHoverTint, UIComponent.lcdTooltipHoverTint);
			buttonBox.SetSkinLocation(SkinState.Pressed, sourceRectangle, UIComponent.lcdTooltipPressedTint, UIComponent.lcdTooltipPressedTint);
			buttonBox.SetSkinLocation(SkinState.Checked, sourceRectangle, UIComponent.lcdTooltipCheckedTint, UIComponent.lcdTooltipCheckedTint);
			buttonBox.SetSkinLocation(SkinState.CheckedPressed, sourceRectangle, UIComponent.lcdTooltipCheckedPressedTint, UIComponent.lcdTooltipCheckedPressedTint);
			buttonBox.SetSkinLocation(SkinState.CheckedHover, sourceRectangle, UIComponent.lcdTooltipHoverTint, UIComponent.lcdTooltipHoverTint);
			buttonBox.SetSkinLocation(SkinState.HoverDisabled, sourceRectangle, UIComponent.hudHoverTintNonEnabled, UIComponent.hudHoverTintNonEnabled);
			buttonBox.SetSkinLocation(SkinState.Disabled, sourceRectangle, UIComponent.lcdTooltipPressedTint, UIComponent.lcdTooltipPressedTint);
			CheckedMode = CheckedModes.CanBeChecked;
			Font = GUIManager.LCDandHUDFont;
			LabelColor = Color.White;
			label.InitDisabledColor(UIComponent.lcdDisabledColor, LabelColor);
			break;
		}
		case TextButtonType.LCDCombo:
		{
			label.Init(Label.LabelType.LCDNormal);
			label.InitDisabledColor(UIComponent.lcdDisabledColor, label.NormalColor);
			Rectangle sourceRectangle = (Skin = guiManager.GUISpriteSheet.GetSourceRectangle("basic_dropdown_light"));
			buttonBox.SetSkinLocation(SkinState.Hover, sourceRectangle, UIComponent.lcdHoverTint, UIComponent.lcdHoverTint);
			sourceRectangle = guiManager.GUISpriteSheet.GetSourceRectangle("basic_dropdown_light_in");
			buttonBox.SetSkinLocation(SkinState.Pressed, sourceRectangle);
			buttonBox.SetSkinLocation(SkinState.Checked, sourceRectangle);
			buttonBox.SetSkinLocation(SkinState.CheckedPressed, sourceRectangle, UIComponent.lcdPressedTint, UIComponent.lcdPressedTint);
			buttonBox.SetSkinLocation(SkinState.CheckedHover, sourceRectangle, UIComponent.lcdHoverTint, UIComponent.lcdHoverTint);
			sourceRectangle = guiManager.GUISpriteSheet.GetSourceRectangle("basic_dropdown_light_disabled");
			buttonBox.SetSkinLocation(SkinState.Disabled, sourceRectangle);
			leftButtonPadding = 17;
			rightButtonPadding = 25;
			TextAlignment = TextAlign.Left;
			CheckedMode = CheckedModes.CanBeChecked;
			buttonBox.CornerSize = 26;
			Height = sourceRectangle.Height;
			RenderType = RenderType.CRTAndLCD;
			break;
		}
		case TextButtonType.LCDCollapsableHeaderBig:
			InitLCDCollapsableHeaderBig("basic_collapsable_header_medium");
			break;
		case TextButtonType.LCDCollapsableHeaderBigBlue:
			InitLCDCollapsableHeaderBig("basic_collapsable_header_medium_blue");
			break;
		case TextButtonType.LCDCollapsableHeaderBigGreen:
			InitLCDCollapsableHeaderBig("basic_collapsable_header_medium_green");
			break;
		case TextButtonType.LCDCollapsableHeaderBigRed:
			InitLCDCollapsableHeaderBig("basic_collapsable_header_medium_red");
			break;
		case TextButtonType.LCDCollapsableHeaderSmall:
		{
			Rectangle sourceRectangle = (Skin = guiManager.GUISpriteSheet.GetSourceRectangle("basic_collapsable_header_small"));
			buttonBox.SetSkinLocation(SkinState.Hover, sourceRectangle, UIComponent.lcdHoverTint, UIComponent.lcdHoverTint);
			buttonBox.SetSkinLocation(SkinState.Pressed, sourceRectangle);
			buttonBox.SetSkinLocation(SkinState.Checked, sourceRectangle);
			buttonBox.SetSkinLocation(SkinState.CheckedPressed, sourceRectangle, UIComponent.lcdPressedTint, UIComponent.lcdPressedTint);
			buttonBox.SetSkinLocation(SkinState.CheckedHover, sourceRectangle, UIComponent.lcdHoverTint, UIComponent.lcdHoverTint);
			buttonBox.CornerSize = 18;
			Height = sourceRectangle.Height;
			break;
		}
		case TextButtonType.HUDItemQuantity:
		{
			Rectangle sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle("HUD_button");
			Width = sourceRectangle.Width;
			Height = sourceRectangle.Height;
			CornerSize = 8;
			Skin = sourceRectangle;
			buttonBox.SetSkinLocation(SkinState.Hover, sourceRectangle, UIComponent.lcdTooltipHoverTint, UIComponent.lcdTooltipHoverTint);
			buttonBox.SetSkinLocation(SkinState.Pressed, sourceRectangle, UIComponent.lcdTooltipPressedTint, UIComponent.lcdTooltipPressedTint);
			buttonBox.SetSkinLocation(SkinState.Checked, sourceRectangle, UIComponent.lcdTooltipCheckedTint, UIComponent.lcdTooltipCheckedTint);
			buttonBox.SetSkinLocation(SkinState.CheckedPressed, sourceRectangle, UIComponent.lcdTooltipCheckedPressedTint, UIComponent.lcdTooltipCheckedPressedTint);
			buttonBox.SetSkinLocation(SkinState.CheckedHover, sourceRectangle, UIComponent.lcdTooltipHoverTint, UIComponent.lcdTooltipHoverTint);
			buttonBox.SetSkinLocation(SkinState.HoverDisabled, sourceRectangle, UIComponent.hudHoverTintNonEnabled, UIComponent.hudHoverTintNonEnabled);
			CheckedMode = CheckedModes.CanBeChecked;
			Font = GUIManager.LCDandHUDFont;
			LabelColor = Color.White;
			break;
		}
		case TextButtonType.HUDGather:
		{
			Rectangle sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle("HUD_icon_gather");
			InitZoneHUD(sourceRectangle);
			break;
		}
		case TextButtonType.HUDStockpile:
		{
			Rectangle sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle("HUD_icon_stockpile");
			InitZoneHUD(sourceRectangle);
			break;
		}
		case TextButtonType.HUDUpgrade:
		{
			Rectangle sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle("HUD_icon_uparrow");
			InitZoneHUD(sourceRectangle);
			break;
		}
		case TextButtonType.HUDPatrol:
		{
			Rectangle sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle("HUD_icon_patrol");
			InitZoneHUD(sourceRectangle);
			break;
		}
		case TextButtonType.HUDAttack:
		{
			Rectangle sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle("HUD_icon_sword");
			InitZoneHUD(sourceRectangle);
			break;
		}
		case TextButtonType.HUDClaim:
		{
			Rectangle sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle("HUD_icon_handGrabbing");
			InitZoneHUD(sourceRectangle);
			break;
		}
		case TextButtonType.HUDDiscard:
		{
			Rectangle sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle("HUD_icon_palmSlanted");
			InitZoneHUD(sourceRectangle);
			break;
		}
		case TextButtonType.HUDSalvage:
		{
			Rectangle sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle("HUD_icon_recycleArrows");
			InitZoneHUD(sourceRectangle);
			break;
		}
		case TextButtonType.HUDPackingDown:
		{
			Rectangle sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle("HUD_icon_closedBox");
			InitZoneHUD(sourceRectangle);
			break;
		}
		case TextButtonType.HUDHunt:
		{
			Rectangle sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle("HUD_icon_hunt");
			InitZoneHUD(sourceRectangle);
			break;
		}
		case TextButtonType.HUDScout:
		{
			Rectangle sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle("HUD_icon_scout");
			InitZoneHUD(sourceRectangle);
			break;
		}
		case TextButtonType.HUDForage:
		{
			Rectangle sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle("HUD_icon_forage");
			InitZoneHUD(sourceRectangle);
			break;
		}
		case TextButtonType.HUD:
		case TextButtonType.HUDHasState:
		{
			Rectangle sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle("HUD_button");
			Width = sourceRectangle.Width;
			Height = 25;
			leftButtonPadding = 6;
			rightButtonPadding = 6;
			CornerSize = 9;
			InitHUD(sourceRectangle);
			if (type == TextButtonType.HUDHasState)
			{
				CheckedMode = CheckedModes.CanBeChecked;
			}
			break;
		}
		case TextButtonType.HUDSliderButton:
			InitHUDSliderButton("HUD_slider_knob");
			break;
		case TextButtonType.HUDSliderButtonWhite:
			InitHUDSliderButton("HUD_slider_knob_white");
			break;
		case TextButtonType.HyperlinkBlack:
			break;
		}
	}

	private void InitLCDCollapsableHeaderBig(string sprite)
	{
		Rectangle value = (Skin = guiManager.GUISpriteSheet.GetSourceRectangle(sprite));
		buttonBox.SetSkinLocation(SkinState.Hover, value, UIComponent.lcdHoverTint, UIComponent.lcdHoverTint);
		buttonBox.SetSkinLocation(SkinState.Pressed, value);
		buttonBox.SetSkinLocation(SkinState.Checked, value);
		buttonBox.SetSkinLocation(SkinState.CheckedPressed, value, UIComponent.lcdPressedTint, UIComponent.lcdPressedTint);
		buttonBox.SetSkinLocation(SkinState.CheckedHover, value, UIComponent.lcdHoverTint, UIComponent.lcdHoverTint);
		buttonBox.CornerSize = 15;
		Height = value.Height;
	}

	private Rectangle InitHUDSliderButton(string sprite)
	{
		Rectangle sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle(sprite);
		Width = sourceRectangle.Width;
		Height = sourceRectangle.Height;
		leftButtonPadding = 3;
		rightButtonPadding = 2;
		CornerSize = 4;
		InitHUD(sourceRectangle);
		return sourceRectangle;
	}

	private Rectangle InitLCDSliderButton(string sprite)
	{
		DebugTag = "LCDSliderButton";
		label.Init(Label.LabelType.LCDNormal);
		leftButtonPadding = 4;
		rightButtonPadding = 4;
		Rectangle sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle(sprite);
		Width = sourceRectangle.Width;
		Height = sourceRectangle.Height;
		CornerSize = 6;
		Skin = sourceRectangle;
		buttonBox.SetSkinLocation(SkinState.Hover, sourceRectangle, UIComponent.lcdTooltipHoverTint, UIComponent.lcdTooltipHoverTint);
		sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle(sprite);
		buttonBox.SetSkinLocation(SkinState.Pressed, sourceRectangle, UIComponent.lcdTooltipPressedTint, UIComponent.lcdTooltipPressedTint);
		sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle("lcd_bar_knob_disabled");
		buttonBox.SetSkinLocation(SkinState.Disabled, sourceRectangle, UIComponent.lcdTooltipHoverTint, UIComponent.lcdTooltipHoverTint);
		label.InitDisabledColor(UIComponent.lcdDisabledColor, LabelColor);
		return sourceRectangle;
	}

	private Rectangle InitMainButton(string outSprite, string hoverSprite, string inSprite, Color color, bool slim = false)
	{
		Rectangle sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle(outSprite);
		Width = sourceRectangle.Width;
		Height = sourceRectangle.Height;
		CornerSize = 10;
		Skin = sourceRectangle;
		sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle(hoverSprite);
		HoverSkin = sourceRectangle;
		sourceRectangle = (PressedSkin = base.GUIManager.GUISpriteSheet.GetSourceRectangle(inSprite));
		buttonBox.SetSkinLocation(SkinState.Checked, sourceRectangle, UIComponent.lcdTooltipCheckedTint, UIComponent.lcdTooltipCheckedTint);
		buttonBox.SetSkinLocation(SkinState.CheckedPressed, sourceRectangle, UIComponent.lcdTooltipCheckedPressedTint, UIComponent.lcdTooltipCheckedPressedTint);
		buttonBox.SetSkinLocation(SkinState.CheckedHover, sourceRectangle, UIComponent.lcdTooltipHoverTint, UIComponent.lcdTooltipHoverTint);
		Font = GUIManager.LCDandHUDFont;
		LabelColor = color;
		if (slim)
		{
			leftButtonPadding = 8;
			rightButtonPadding = 8;
		}
		else
		{
			leftButtonPadding = 14;
			rightButtonPadding = 14;
		}
		clickedSound = GUIManager.BeepBasicPanel;
		return sourceRectangle;
	}

	public Color GetNormalColor()
	{
		return Type switch
		{
			TextButtonType.HUDToolTipWhite => Color.White, 
			TextButtonType.LCDToolTipBlack => Color.Black, 
			TextButtonType.LCDAmount => Color.White, 
			_ => Color.White, 
		};
	}

	private void InitInventory()
	{
		Height = 25;
		leftButtonPadding = 26;
		rightButtonPadding = 4;
		CornerSize = 8;
		label.Init(Label.LabelType.LCDNormal);
		Height = 25;
		leftButtonPadding = 6;
		rightButtonPadding = 8;
		Rectangle sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle("basic_button_light");
		Width += icon.Width;
		Height = sourceRectangle.Height;
		CornerSize = 11;
		Skin = sourceRectangle;
		buttonBox.SetSkinLocation(SkinState.Hover, sourceRectangle, UIComponent.lcdTooltipHoverTint, UIComponent.lcdTooltipHoverTint);
		sourceRectangle = (PressedSkin = base.GUIManager.GUISpriteSheet.GetSourceRectangle("basic_button_light_in"));
		buttonBox.SetSkinLocation(SkinState.Pressed, sourceRectangle);
		buttonBox.SetSkinLocation(SkinState.CheckedPressed, sourceRectangle, UIComponent.lcdTooltipHoverTint, UIComponent.lcdTooltipHoverTint);
		label.InitDisabledColor(UIComponent.lcdDisabledColor, LabelColor);
	}

	private void InitZoneHUD(Rectangle iconRect)
	{
		Height = 25;
		Icon icon = new Icon(guiManager);
		icon.SetSkinLocation(SkinState.Normal, iconRect);
		icon.ResizeControlToFitImage();
		Add(icon);
		icon.X = 9;
		CenterChildVertically(icon);
		icon.CanHaveFocus = false;
		Width = iconRect.Width;
		leftButtonPadding = 26;
		rightButtonPadding = 4;
		CornerSize = 8;
		CheckedMode = CheckedModes.CannotBeChecked;
		Rectangle sourceRectangle = guiManager.GUISpriteSheet.GetSourceRectangle("HUD_button");
		InitHUD(sourceRectangle);
	}

	private void InitHUD(Rectangle rect)
	{
		Skin = rect;
		buttonBox.SetSkinLocation(SkinState.Hover, rect, UIComponent.hudHoverTint, UIComponent.hudHoverTint);
		buttonBox.SetSkinLocation(SkinState.Pressed, rect, UIComponent.hudPressedTint, UIComponent.hudPressedTint);
		buttonBox.SetSkinLocation(SkinState.Checked, rect, UIComponent.hudCheckedTint, UIComponent.hudCheckedTint);
		buttonBox.SetSkinLocation(SkinState.CheckedPressed, rect, UIComponent.hudCheckedPressedTint, UIComponent.hudCheckedPressedTint);
		buttonBox.SetSkinLocation(SkinState.CheckedHover, rect, UIComponent.hudHoverTint, UIComponent.hudHoverTint);
		buttonBox.SetSkinLocation(SkinState.HoverDisabled, rect, UIComponent.hudHoverTintNonEnabled, UIComponent.hudHoverTintNonEnabled);
		Font = GUIManager.LCDandHUDFont;
		LabelColor = Color.White;
		label.InitDisabledColor(UIComponent.HUDDisabledColor, LabelColor);
	}

	protected override void LoadGraphicsContent(bool loadAllContent)
	{
		base.LoadGraphicsContent(loadAllContent);
	}

	public void ScaleToFitText()
	{
		ScaleWidthToFitText();
		Height = label.TextHeight;
	}

	public void ScaleWidthToFitText()
	{
		Width = label.TextWidth + leftButtonPadding + rightButtonPadding;
	}

	private void RefreshLabelPosition()
	{
		if (Type != TextButtonType.Hyperlink)
		{
			label.Width = Math.Min(Width - leftButtonPadding - rightButtonPadding, label.TextWidth);
			label.Height = label.TextHeight;
			if (TextAlignment == TextAlign.Center)
			{
				label.X = (Width - rightButtonPadding - leftButtonPadding - label.Width) / 2 + leftButtonPadding;
			}
			else
			{
				label.X = leftButtonPadding;
			}
			label.Y = (Height - label.Height) / 2 + yLabelOffset;
			return;
		}
		label.Width = label.TextWidth;
		label.Height = label.TextHeight;
		if (Width != label.Width)
		{
			Width = label.Width;
		}
		label.X = 0;
		label.Y = 0;
	}

	public void ChangeSkinState(SkinState skinState)
	{
		buttonBox.CurrentSkinState = skinState;
	}

	protected override void OnMouseOver(UIComponent sender, MouseEventArgs args)
	{
		if (!Enabled && string.IsNullOrEmpty(ToolTip))
		{
			return;
		}
		if (!Enabled)
		{
			ChangeSkinState(SkinState.HoverDisabled);
			base.OnMouseOver(sender, args);
			return;
		}
		if (Type != TextButtonType.Hyperlink)
		{
			if (base.IsPressed)
			{
				ChangeSkinState(SkinState.CheckedPressed);
			}
			else
			{
				ChangeSkinState(SkinState.Hover);
				if (HasState && isChecked)
				{
					ChangeSkinState(SkinState.CheckedHover);
				}
			}
		}
		base.OnMouseOver(sender, args);
	}

	protected override void OnMouseOut(UIComponent sender, MouseEventArgs args)
	{
		if (Type != TextButtonType.Hyperlink)
		{
			if (!Enabled && string.IsNullOrEmpty(ToolTip))
			{
				return;
			}
			buttonBox.CurrentSkinState = SkinState.Normal;
			if (HasState && isChecked)
			{
				buttonBox.CurrentSkinState = SkinState.Checked;
			}
			if (!Enabled)
			{
				buttonBox.CurrentSkinState = SkinState.Disabled;
			}
			if (base.IsPressed)
			{
				buttonBox.CurrentSkinState = SkinState.Pressed;
			}
		}
		base.OnMouseOut(sender, args);
	}

	protected override void OnMouseDown(MouseEventArgs args)
	{
		if (!Enabled)
		{
			return;
		}
		if (Type != TextButtonType.Hyperlink && args.Button == MouseButtons.Left)
		{
			buttonBox.CurrentSkinState = SkinState.Pressed;
			if (clickedSound != null)
			{
				base.GUIManager.PlaySound(clickedSound);
			}
		}
		base.OnMouseDown(args);
	}

	protected override void OnLoseFocus()
	{
		base.OnLoseFocus();
		if (Type == TextButtonType.Hyperlink)
		{
			return;
		}
		if (Enabled)
		{
			if (isChecked)
			{
				buttonBox.CurrentSkinState = SkinState.Checked;
			}
			else
			{
				buttonBox.CurrentSkinState = SkinState.Normal;
			}
		}
		else
		{
			buttonBox.CurrentSkinState = SkinState.Disabled;
		}
	}

	protected override void OnMouseUp(MouseEventArgs args)
	{
		if (!Enabled)
		{
			return;
		}
		if (Type != TextButtonType.Hyperlink && args.Button == MouseButtons.Left)
		{
			if (CheckCoordinates(args.Position.X, args.Position.Y))
			{
				buttonBox.CurrentSkinState = SkinState.Hover;
				if (checkedMode == CheckedModes.CanBeChecked && !isChecked)
				{
					isChecked = true;
				}
				else if (checkedMode == CheckedModes.SwitchCheckedStateOnClick)
				{
					isChecked = !isChecked;
				}
				if (isChecked)
				{
					buttonBox.CurrentSkinState = SkinState.CheckedHover;
				}
			}
			else
			{
				buttonBox.CurrentSkinState = SkinState.Normal;
				if (HasState && isChecked)
				{
					buttonBox.CurrentSkinState = SkinState.Checked;
				}
			}
		}
		base.OnMouseUp(args);
	}

	protected override void OnResize(UIComponent sender)
	{
		base.OnResize(sender);
		if (buttonBox != null)
		{
			buttonBox.Width = Width;
			if (Type != TextButtonType.Hyperlink)
			{
				buttonBox.Height = Height;
			}
			else
			{
				buttonBox.Height = label.TextHeight;
			}
		}
		if (label != null && Type != TextButtonType.Hyperlink)
		{
			RefreshLabelPosition();
		}
	}
}
