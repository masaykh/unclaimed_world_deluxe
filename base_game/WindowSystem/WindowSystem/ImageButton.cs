using InputEventSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace WindowSystem;

public class ImageButton : Icon, ICanBeChecked
{
	protected bool isChecked;

	protected SoundEffect clickedSound;

	protected Icon icon;

	private int? iconOffSetYPos;

	private int? iconOffSetXPos;

	public bool RightClickEnabled;

	private CheckedModes checkedMode = CheckedModes.SwitchCheckedStateOnClick;

	public Color DisabledColor = Microsoft.Xna.Framework.Color.LightGray;

	private Color? enabledColor;

	private string enabledNormalSkin;

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

	public Color NormalColor
	{
		set
		{
			if (base.Color != value)
			{
				base.Color = value;
				SetSkinLocation(SkinState.Normal, null, value, value, flipHorizontally: false, modulateColor: true);
			}
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
			RecalculateIconPosition();
		}
	}

	public Rectangle Skin
	{
		set
		{
			SetSkinLocation(SkinState.Normal, value);
		}
	}

	public Rectangle HoverSkin
	{
		set
		{
			SetSkinLocation(SkinState.Hover, value);
		}
	}

	public Rectangle PressedSkin
	{
		set
		{
			SetSkinLocation(SkinState.Pressed, value);
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
			if (base.Enabled == value)
			{
				return;
			}
			base.Enabled = value;
			if (base.Enabled)
			{
				if (enabledNormalSkin != null && enabledColor.HasValue)
				{
					SetSkinLocation(SkinState.Normal, null, enabledColor.Value, enabledColor.Value);
				}
				if (IsChecked)
				{
					base.CurrentSkinState = SkinState.Checked;
				}
				else if (base.IsPressed)
				{
					base.CurrentSkinState = SkinState.Pressed;
				}
				else
				{
					base.CurrentSkinState = SkinState.Normal;
				}
			}
			else
			{
				if (enabledNormalSkin != null && enabledColor.HasValue)
				{
					SetSkinLocation(SkinState.Normal, null, DisabledColor, DisabledColor);
				}
				if (IsChecked)
				{
					base.CurrentSkinState = SkinState.CheckedDisabled;
				}
				else
				{
					base.CurrentSkinState = SkinState.Disabled;
				}
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
			if (isChecked == value)
			{
				return;
			}
			isChecked = value;
			if (IsChecked)
			{
				if (!Enabled)
				{
					base.CurrentSkinState = SkinState.CheckedDisabled;
				}
				else if (base.IsPressed)
				{
					base.CurrentSkinState = SkinState.CheckedPressed;
				}
				else
				{
					base.CurrentSkinState = SkinState.Checked;
				}
			}
			else if (!Enabled)
			{
				base.CurrentSkinState = SkinState.Disabled;
			}
			else if (base.IsPressed)
			{
				base.CurrentSkinState = SkinState.Pressed;
			}
			else
			{
				base.CurrentSkinState = SkinState.Normal;
			}
		}
	}

	private void UpdateSkinState(bool hover = false)
	{
		if (base.Enabled)
		{
			if (hover)
			{
				if (IsChecked)
				{
					base.CurrentSkinState = SkinState.CheckedHover;
				}
				else if (base.IsPressed)
				{
					base.CurrentSkinState = SkinState.Pressed;
				}
				else
				{
					base.CurrentSkinState = SkinState.Hover;
				}
			}
			else if (IsChecked)
			{
				base.CurrentSkinState = SkinState.Checked;
			}
			else if (base.IsPressed)
			{
				base.CurrentSkinState = SkinState.Pressed;
			}
			else
			{
				base.CurrentSkinState = SkinState.Normal;
			}
		}
		else if (IsChecked)
		{
			if (hover)
			{
				base.CurrentSkinState = SkinState.CheckedDisabledHover;
			}
			else
			{
				base.CurrentSkinState = SkinState.CheckedDisabled;
			}
		}
		else if (hover)
		{
			base.CurrentSkinState = SkinState.HoverDisabled;
		}
		else
		{
			base.CurrentSkinState = SkinState.Disabled;
		}
	}

	public ImageButton(GUIManager guiManager)
		: base(guiManager)
	{
		base.ScaleImageToSizeOfControl = true;
	}

	public void Init(ImageButtonType type)
	{
		Init(type, -1);
	}

	public void Init(ImageButtonType type, int flavour)
	{
		switch (type)
		{
		case ImageButtonType.Comm:
		{
			Rectangle sourceRectangle = (Skin = guiManager.GUISpriteSheet.GetSourceRectangle("event_button_out"));
			Width = sourceRectangle.Width;
			Height = sourceRectangle.Height;
			sourceRectangle = guiManager.GUISpriteSheet.GetSourceRectangle("event_button_hover");
			SetSkinLocation(SkinState.Hover, sourceRectangle);
			sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle("event_button_down");
			PressedSkin = sourceRectangle;
			clickedSound = GUIManager.Click1;
			break;
		}
		case ImageButtonType.EventDialogsButton:
			InitButton("dialog_event_button_out", "dialog_event_button_in", "dialog_event_button_hoover", "dialog_event_button_in_hover");
			clickedSound = GUIManager.Click1;
			break;
		case ImageButtonType.DiplomacyButton:
			InitButton("diplomacy_button_out", "diplomacy_button_in", "diplomacy_button_hover", "diplomacy_button_in_hover");
			clickedSound = GUIManager.Click1;
			break;
		case ImageButtonType.PolicyButton:
			InitButton("policy_button_out", "policy_button_in", "policy_button_hover", "policy_button_in_hover");
			clickedSound = GUIManager.Click1;
			break;
		case ImageButtonType.GraphButton:
			InitButton("graph_button_out", "graph_button_in", "graph_button_hoover", "graph_button_in_hover");
			clickedSound = GUIManager.Click1;
			break;
		case ImageButtonType.LedgerButton:
			InitButton("ledger_button_out", "ledger_button_in", "ledger_button_hoover", "ledger_button_in_hover");
			clickedSound = GUIManager.Click1;
			break;
		case ImageButtonType.TasksButton:
			InitButton("tasks_button_out", "tasks_button_in", "tasks_button_hoover", "tasks_button_in_hover");
			clickedSound = GUIManager.Click1;
			break;
		case ImageButtonType.InventoryButton:
			InitButton("inventory_button_out", "inventory_button_in", "inventory_button_hoover", "inventory_button_in_hover");
			clickedSound = GUIManager.Click1;
			break;
		case ImageButtonType.MissionsButton:
			InitButton("comm_button_out", "comm_button_in", "comm_button_hover", "comm_button_in_hover");
			clickedSound = GUIManager.Click1;
			break;
		case ImageButtonType.PersonnelButton:
			InitButton("personnel_button_out", "personnel_button_in", "personnel_button_hover", "personnel_button_in_hover");
			clickedSound = GUIManager.Click1;
			break;
		case ImageButtonType.EntityInfo:
		{
			InitButton("infoswitch_entitybutton_out", "infoswitch_entitybutton_in", UIComponent.lcdHoverTint, UIComponent.lcdPressedTint);
			clickedSound = GUIManager.Click1;
			Rectangle sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle("infoswitch_entitybutton_disabled");
			SetSkinLocation(SkinState.Disabled, sourceRectangle);
			break;
		}
		case ImageButtonType.MapAreaInfo:
		{
			InitButton("infoswitch_zonebutton_out", "infoswitch_zonebutton_in", UIComponent.lcdHoverTint, UIComponent.lcdPressedTint);
			clickedSound = GUIManager.Click1;
			Rectangle sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle("infoswitch_zonebutton_disabled");
			SetSkinLocation(SkinState.Disabled, sourceRectangle);
			break;
		}
		case ImageButtonType.CenterOnEntity:
			InitButton("centerentity_button_out", "centerentity_button_in", "centerentity_button_hover");
			clickedSound = GUIManager.Click1;
			break;
		case ImageButtonType.Counter:
			InitButton("counter_button_out", "counter_button_in", UIComponent.panelHoverTint);
			break;
		case ImageButtonType.FoodRating:
			InitButton("nutritionRating_button_out", "nutritionRating_button_in", UIComponent.panelHoverTint);
			checkedMode = CheckedModes.CannotBeChecked;
			break;
		case ImageButtonType.SecurityRating:
			InitButton("securityRating_button_out", "securityRating_button_in", UIComponent.panelHoverTint);
			checkedMode = CheckedModes.CannotBeChecked;
			break;
		case ImageButtonType.ComfortRating:
			InitButton("comfortRating_button_out", "comfortRating_button_in", UIComponent.panelHoverTint);
			checkedMode = CheckedModes.CannotBeChecked;
			break;
		case ImageButtonType.BuyAction:
			InitButton("mission_button_crate_green", UIComponent.lcdHoverTint, UIComponent.lcdPressedTint);
			break;
		case ImageButtonType.SellAction:
			InitButton("mission_button_crate_red", UIComponent.lcdHoverTint, UIComponent.lcdPressedTint);
			break;
		case ImageButtonType.LoadAction:
			InitButton("mission_button_crate_green", UIComponent.lcdHoverTint, UIComponent.lcdPressedTint);
			break;
		case ImageButtonType.UnloadAction:
			InitButton("mission_button_crate_red", UIComponent.lcdHoverTint, UIComponent.lcdPressedTint);
			break;
		case ImageButtonType.EmbarkAction:
			InitButton("mission_button_people_green", UIComponent.lcdHoverTint, UIComponent.lcdPressedTint);
			break;
		case ImageButtonType.DisembarkAction:
			InitButton("mission_button_people_red", UIComponent.lcdHoverTint, UIComponent.lcdPressedTint);
			break;
		case ImageButtonType.AddAction:
			InitButton("mission_button_plus", UIComponent.lcdHoverTint, UIComponent.lcdPressedTint);
			break;
		case ImageButtonType.ComfortPolicy:
			InitButton("policy_button_comfort", UIComponent.lcdHoverTint, UIComponent.lcdPressedTint);
			break;
		case ImageButtonType.SecurityPolicy:
			InitButton("policy_button_security", UIComponent.lcdHoverTint, UIComponent.lcdPressedTint);
			break;
		case ImageButtonType.FoodPolicy:
			InitButton("policy_button_food", UIComponent.lcdHoverTint, UIComponent.lcdPressedTint);
			break;
		case ImageButtonType.BuildButton:
		{
			Rectangle sourceRectangle = (Skin = guiManager.GUISpriteSheet.GetSourceRectangle("build_button_out"));
			Width = sourceRectangle.Width;
			Height = sourceRectangle.Height;
			sourceRectangle = guiManager.GUISpriteSheet.GetSourceRectangle("build_button_in");
			SetSkinLocation(SkinState.Checked, sourceRectangle);
			PressedSkin = sourceRectangle;
			sourceRectangle = guiManager.GUISpriteSheet.GetSourceRectangle("build_button_in_hover");
			SetSkinLocation(SkinState.CheckedHover, sourceRectangle);
			sourceRectangle = guiManager.GUISpriteSheet.GetSourceRectangle("build_button_hoover");
			SetSkinLocation(SkinState.Hover, sourceRectangle);
			clickedSound = GUIManager.Click1;
			break;
		}
		case ImageButtonType.ResourceSelectionArrow:
		{
			Rectangle sourceRectangle = (Skin = guiManager.GUISpriteSheet.GetSourceRectangle("arrow_button_out"));
			Width = sourceRectangle.Width;
			Height = sourceRectangle.Height;
			sourceRectangle = guiManager.GUISpriteSheet.GetSourceRectangle("arrow_button_in");
			SetSkinLocation(SkinState.Checked, sourceRectangle);
			PressedSkin = sourceRectangle;
			sourceRectangle = guiManager.GUISpriteSheet.GetSourceRectangle("arrow_button_in_hover");
			SetSkinLocation(SkinState.CheckedHover, sourceRectangle);
			sourceRectangle = guiManager.GUISpriteSheet.GetSourceRectangle("arrow_button_hover");
			SetSkinLocation(SkinState.Hover, sourceRectangle);
			clickedSound = GUIManager.Click1;
			break;
		}
		case ImageButtonType.ScanButton:
		{
			Rectangle sourceRectangle = (Skin = guiManager.GUISpriteSheet.GetSourceRectangle("scan_button_out"));
			Width = sourceRectangle.Width;
			Height = sourceRectangle.Height;
			sourceRectangle = guiManager.GUISpriteSheet.GetSourceRectangle("scan_button_in");
			SetSkinLocation(SkinState.Checked, sourceRectangle);
			PressedSkin = sourceRectangle;
			sourceRectangle = guiManager.GUISpriteSheet.GetSourceRectangle("scan_button_in_hover");
			SetSkinLocation(SkinState.CheckedHover, sourceRectangle);
			sourceRectangle = guiManager.GUISpriteSheet.GetSourceRectangle("scan_button_hover");
			SetSkinLocation(SkinState.Hover, sourceRectangle);
			clickedSound = GUIManager.Click1;
			break;
		}
		case ImageButtonType.Minimap:
			InitButton("minimap_button_out", "minimap_button_in", "minimap_button_hover", "minimap_button_in_hover");
			break;
		case ImageButtonType.CommSlim:
		{
			Rectangle sourceRectangle = (Skin = guiManager.GUISpriteSheet.GetSourceRectangle("event_slim_button_out"));
			SetSkinLocation(SkinState.Checked, sourceRectangle);
			SetSkinLocation(SkinState.CheckedHover, sourceRectangle);
			sourceRectangle = guiManager.GUISpriteSheet.GetSourceRectangle("event_slim_button_hover");
			SetSkinLocation(SkinState.Hover, sourceRectangle);
			Width = sourceRectangle.Width;
			Height = sourceRectangle.Height;
			sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle("event_slim_button_down");
			PressedSkin = sourceRectangle;
			clickedSound = GUIManager.Click1;
			break;
		}
		case ImageButtonType.MinimapRubber:
		{
			Rectangle sourceRectangle = (Skin = guiManager.GUISpriteSheet.GetSourceRectangle("minimap_button_out"));
			SetSkinLocation(SkinState.Checked, sourceRectangle);
			sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle("minimap_button_hover");
			SetSkinLocation(SkinState.Hover, sourceRectangle);
			SetSkinLocation(SkinState.CheckedHover, sourceRectangle);
			Width = sourceRectangle.Width;
			Height = sourceRectangle.Height;
			sourceRectangle = (PressedSkin = base.GUIManager.GUISpriteSheet.GetSourceRectangle("minimap_button_in"));
			SetSkinLocation(SkinState.CheckedPressed, sourceRectangle);
			clickedSound = GUIManager.Click1;
			break;
		}
		case ImageButtonType.Help:
		{
			Rectangle sourceRectangle = (Skin = guiManager.GUISpriteSheet.GetSourceRectangle("help_button_out"));
			SetSkinLocation(SkinState.Checked, sourceRectangle);
			SetSkinLocation(SkinState.CheckedHover, sourceRectangle);
			SetSkinLocation(SkinState.Hover, sourceRectangle);
			Width = sourceRectangle.Width;
			Height = sourceRectangle.Height;
			sourceRectangle = (PressedSkin = base.GUIManager.GUISpriteSheet.GetSourceRectangle("help_button_in"));
			SetSkinLocation(SkinState.CheckedPressed, sourceRectangle);
			clickedSound = GUIManager.Click1;
			break;
		}
		case ImageButtonType.LCDIncrease:
			InitButton("spinner_increase", "spinner_increase_in", "spinner_increase_hover");
			clickedSound = GUIManager.BeepLCD;
			break;
		case ImageButtonType.LCDDecrease:
			InitButton("spinner_decrease", "spinner_decrease_in", "spinner_decrease_hover");
			clickedSound = GUIManager.BeepLCD;
			break;
		case ImageButtonType.LCDCancel:
			InitButton("spinner_decrease", "spinner_decrease_in", "spinner_decrease_hover");
			clickedSound = GUIManager.BeepLCD;
			break;
		case ImageButtonType.LCDExpand:
			InitButton("lcd_expandbutton", "lcd_expandbutton_in", "lcd_expandbutton_hover");
			clickedSound = GUIManager.BeepLCD;
			break;
		case ImageButtonType.LCDCollapse:
			InitButton("lcd_collapsebutton", "lcd_collapsebutton_in", "lcd_collapsebutton_hover");
			clickedSound = GUIManager.BeepLCD;
			break;
		case ImageButtonType.LCDArrowRight:
			InitButton("lcd_rightarrow", "lcd_rightarrow_in", "lcd_rightarrow_hover");
			clickedSound = GUIManager.BeepLCD;
			break;
		case ImageButtonType.LCDPadlock:
			InitButton("basic_padlock", UIComponent.lcdHoverTint, UIComponent.lcdPressedTint, UIComponent.lcdPressedTint, UIComponent.lcdHoverTint, UIComponent.lcdPressedTint);
			CheckedMode = CheckedModes.SwitchCheckedStateOnClick;
			clickedSound = GUIManager.BeepLCD;
			break;
		case ImageButtonType.LCDPadlockWhite:
			InitButton("basic_padlock_white", UIComponent.lcdHoverTint, UIComponent.lcdPressedTint, UIComponent.lcdPressedTint, UIComponent.lcdHoverTint, UIComponent.lcdPressedTint);
			CheckedMode = CheckedModes.SwitchCheckedStateOnClick;
			clickedSound = GUIManager.BeepLCD;
			break;
		case ImageButtonType.LCDArrowDown:
			InitButton("lcd_downarrow", "lcd_downarrow_in", "lcd_downarrow_hover");
			clickedSound = GUIManager.BeepLCD;
			break;
		case ImageButtonType.SiteMarker:
			InitButton("map_button_roundSmall", "map_button_roundSmall", UIComponent.lcdHoverTint, UIComponent.lcdPressedTint);
			break;
		case ImageButtonType.SiteMarkerTallPin:
			InitButton("map_button_roundSmall_pin", "map_button_roundSmall_pin", UIComponent.lcdHoverTint, UIComponent.lcdPressedTint);
			break;
		case ImageButtonType.SiteMarkerShortPin:
			InitButton("map_button_roundSmall_pinShort", "map_button_roundSmall_pinShort", UIComponent.lcdHoverTint, UIComponent.lcdPressedTint);
			break;
		case ImageButtonType.LCDCheckbox:
			InitButton("basic_checkbox_unselected", "basic_checkbox_selected", UIComponent.lcdHoverTint, UIComponent.lcdPressedTint, null, modulateHoverColor: false, "basic_checkbox_disabled");
			break;
		case ImageButtonType.LCDArrowDownNew:
			InitButton("basic_dropdown_arrow_down", UIComponent.lcdHoverTint, UIComponent.lcdPressedTint);
			break;
		case ImageButtonType.LCDTracking:
		{
			Rectangle sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle("basic_button_light");
			Width = sourceRectangle.Width;
			Height = sourceRectangle.Height;
			Skin = sourceRectangle;
			SetSkinLocation(1, sourceRectangle, UIComponent.lcdTooltipHoverTint, UIComponent.lcdTooltipHoverTint);
			sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle("basic_button_light_in");
			PressedSkin = sourceRectangle;
			Rectangle sourceRectangle3 = guiManager.GUISpriteSheet.GetSourceRectangle("basic_icon_crosshairs");
			icon = new Icon(guiManager);
			Color value = new Color(58, 113, 119);
			icon.SetSkinLocation(SkinState.Normal, sourceRectangle3, value, value);
			icon.CurrentSkin = 0;
			icon.Visible = false;
			Add(icon);
			icon.X = (Width - icon.Width) / 2;
			icon.Y = (Height - icon.Height) / 2;
			icon.CanHaveFocus = false;
			clickedSound = GUIManager.BeepLCD;
			break;
		}
		case ImageButtonType.LCDList:
			InitButton("basic_icon_list", UIComponent.hudHoverTint, UIComponent.hudPressedTint);
			clickedSound = GUIManager.BeepLCD;
			break;
		case ImageButtonType.HUDIncrease:
			InitButton("HUD_spinner_increase", UIComponent.hudHoverTint, UIComponent.hudPressedTint);
			clickedSound = GUIManager.BeepLCD;
			break;
		case ImageButtonType.HUDDecrease:
			InitButton("HUD_spinner_decrease", UIComponent.hudHoverTint, UIComponent.hudPressedTint);
			clickedSound = GUIManager.BeepLCD;
			break;
		case ImageButtonType.HUDCycleEntity:
			InitButton("HUD_button_cycle", UIComponent.hudHoverTint, UIComponent.hudPressedTint);
			clickedSound = GUIManager.BeepLCD;
			break;
		case ImageButtonType.HUDModifyZone:
			InitButton("HUD_button_changeLayout", UIComponent.hudHoverTint, UIComponent.hudPressedTint);
			clickedSound = GUIManager.BeepLCD;
			break;
		case ImageButtonType.HUDDelete:
			InitButton("HUD_button_trash", UIComponent.hudHoverTint, UIComponent.hudPressedTint);
			clickedSound = GUIManager.BeepLCD;
			break;
		case ImageButtonType.HUDBuild:
			InitButton("HUD_button_hammer", UIComponent.hudHoverTint, UIComponent.hudPressedTint);
			clickedSound = GUIManager.BeepLCD;
			break;
		case ImageButtonType.HUDSalvage:
			InitButton("HUD_button_recycleArrows", UIComponent.hudHoverTint, UIComponent.hudPressedTint);
			clickedSound = GUIManager.BeepLCD;
			break;
		case ImageButtonType.HUDPackDown:
			InitButton("HUD_button_closedBox", UIComponent.hudHoverTint, UIComponent.hudPressedTint);
			clickedSound = GUIManager.BeepLCD;
			break;
		case ImageButtonType.HUDDiscard:
			InitButton("HUD_button_palmSlanted", UIComponent.hudHoverTint, UIComponent.hudPressedTint);
			clickedSound = GUIManager.BeepLCD;
			break;
		case ImageButtonType.HUDClaim:
			InitButton("HUD_button_handGrabbing", UIComponent.hudHoverTint, UIComponent.hudPressedTint);
			clickedSound = GUIManager.BeepLCD;
			break;
		case ImageButtonType.HUDPrices:
			InitButton("HUD_button_coins", UIComponent.hudHoverTint, UIComponent.hudPressedTint);
			break;
		case ImageButtonType.HUDPeople:
			InitButton("HUD_button_people", UIComponent.hudHoverTint, UIComponent.hudPressedTint);
			break;
		case ImageButtonType.HUDShowProductionInfo:
			InitButton("HUD_info_button_production", "HUD_info_button_production_in", "HUD_info_button_production_hover", null, UIComponent.hudHoverTint);
			clickedSound = null;
			break;
		case ImageButtonType.HUDShowGeneralInfo:
			InitButton("HUD_info_button_data", "HUD_info_button_data_hover", UIComponent.hudHoverTint, UIComponent.hudCheckedTint);
			clickedSound = null;
			break;
		case ImageButtonType.HUDClose:
			InitButton("HUD_button_close", UIComponent.hudHoverTint, UIComponent.hudPressedTint, UIComponent.hudCheckedTint, UIComponent.hudCheckedHoverTint, UIComponent.hudCheckedPressedTint);
			clickedSound = null;
			checkedMode = CheckedModes.CannotBeChecked;
			break;
		case ImageButtonType.HUDCrosshair:
			InitButton("HUD_button_crosshairs", UIComponent.hudHoverTint, UIComponent.hudPressedTint, UIComponent.hudCheckedTint, UIComponent.hudCheckedHoverTint, UIComponent.hudCheckedPressedTint);
			clickedSound = null;
			break;
		case ImageButtonType.HUDPin:
			InitButton("HUD_button_pushPin", UIComponent.hudHoverTint, UIComponent.hudPressedTint, UIComponent.hudCheckedTint, UIComponent.hudCheckedHoverTint, UIComponent.hudCheckedPressedTint);
			clickedSound = null;
			break;
		case ImageButtonType.HUDPadlock:
			InitButton("HUD_padlock", UIComponent.hudHoverTint, UIComponent.hudPressedTint, UIComponent.hudCheckedTint, UIComponent.hudCheckedHoverTint, UIComponent.hudCheckedPressedTint);
			CheckedMode = CheckedModes.SwitchCheckedStateOnClick;
			clickedSound = null;
			break;
		case ImageButtonType.HUDInfoScrollUp:
			InitButton("HUD_lineScroller_up", UIComponent.hudHoverTint, UIComponent.hudPressedTint, UIComponent.hudCheckedTint, UIComponent.hudCheckedHoverTint, UIComponent.hudCheckedPressedTint);
			clickedSound = null;
			break;
		case ImageButtonType.HUDInfoScrollDown:
			InitButton("HUD_lineScroller_down", UIComponent.hudHoverTint, UIComponent.hudPressedTint, UIComponent.hudCheckedTint, UIComponent.hudCheckedHoverTint, UIComponent.hudCheckedPressedTint);
			clickedSound = null;
			break;
		case ImageButtonType.HUDCheckbox:
			InitButton("HUD_checkbox_empty", "HUD_checkbox_filled", UIComponent.hudHoverTint, UIComponent.hudPressedTint, null, modulateHoverColor: false, null, hasDisabledState: true);
			base.ScaleImageToSizeOfControl = false;
			break;
		case ImageButtonType.HUDRadioButton:
			InitButton("HUD_radio_empty", "HUD_radio_filled", UIComponent.hudHoverTint, UIComponent.hudPressedTint, null, modulateHoverColor: false, null, hasDisabledState: true);
			base.ScaleImageToSizeOfControl = false;
			break;
		case ImageButtonType.HUDExpandCollapseTinted:
			InitButton("HUD_rightarrow", "HUD_downarrow", Microsoft.Xna.Framework.Color.Gray, UIComponent.hudPressedTint, null, modulateHoverColor: true);
			base.ScaleImageToSizeOfControl = false;
			clickedSound = GUIManager.BeepLCD;
			DebugTag = "HUD_rightarrow";
			break;
		case ImageButtonType.HUDExpandArrowDown:
			InitButton("HUD_downarrow", UIComponent.hudHoverTint, UIComponent.hudPressedTint);
			clickedSound = GUIManager.BeepLCD;
			break;
		case ImageButtonType.HUDArrowRight:
			InitButton("HUD_rightarrow", UIComponent.hudHoverTint, UIComponent.hudPressedTint);
			clickedSound = GUIManager.BeepLCD;
			DebugTag = "HUD_rightarrow";
			break;
		case ImageButtonType.HUDArrowDown:
			InitButton("HUD_downarrow", UIComponent.hudHoverTint, UIComponent.hudPressedTint);
			break;
		case ImageButtonType.HUDArrowUp:
			InitButton("HUD_uparrow", UIComponent.hudHoverTint, UIComponent.hudPressedTint);
			break;
		case ImageButtonType.MetalPanel:
		{
			Rectangle sourceRectangle = guiManager.GUISpriteSheet.GetSourceRectangle($"metal_button_{flavour}_out");
			Skin = sourceRectangle;
			sourceRectangle = guiManager.GUISpriteSheet.GetSourceRectangle($"metal_button_{flavour}_hover");
			SetSkinLocation(SkinState.Hover, sourceRectangle);
			Width = sourceRectangle.Width;
			Height = sourceRectangle.Height;
			sourceRectangle = (PressedSkin = base.GUIManager.GUISpriteSheet.GetSourceRectangle($"metal_button_{flavour}_in"));
			SetSkinLocation(SkinState.Checked, sourceRectangle);
			SetSkinLocation(SkinState.CheckedHover, sourceRectangle);
			SetSkinLocation(SkinState.CheckedPressed, sourceRectangle);
			clickedSound = GUIManager.BeepMetalPanel;
			break;
		}
		case ImageButtonType.MetalPanelHorizontal:
		{
			Rectangle sourceRectangle = guiManager.GUISpriteSheet.GetSourceRectangle($"metal_button_{flavour}horiz_out");
			Skin = sourceRectangle;
			sourceRectangle = guiManager.GUISpriteSheet.GetSourceRectangle($"metal_button_{flavour}horiz_hover");
			SetSkinLocation(SkinState.Hover, sourceRectangle);
			Width = sourceRectangle.Width;
			Height = sourceRectangle.Height;
			sourceRectangle = (PressedSkin = base.GUIManager.GUISpriteSheet.GetSourceRectangle($"metal_button_{flavour}horiz_in"));
			SetSkinLocation(SkinState.Checked, sourceRectangle);
			SetSkinLocation(SkinState.CheckedHover, sourceRectangle);
			SetSkinLocation(SkinState.CheckedPressed, sourceRectangle);
			clickedSound = GUIManager.BeepMetalPanel;
			break;
		}
		case ImageButtonType.LCDExpandWithUpAndDownArrows:
		{
			Rectangle sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle("basic_button_light");
			Width = sourceRectangle.Width;
			Height = sourceRectangle.Height;
			Skin = sourceRectangle;
			SetSkinLocation(1, sourceRectangle, UIComponent.lcdTooltipHoverTint, UIComponent.lcdTooltipHoverTint);
			sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle("basic_button_light_in");
			SetSkinLocation(SkinState.CheckedPressed, sourceRectangle, UIComponent.lcdTooltipHoverTint, UIComponent.lcdTooltipHoverTint);
			PressedSkin = sourceRectangle;
			Rectangle sourceRectangle3 = guiManager.GUISpriteSheet.GetSourceRectangle("arrowblue_down");
			icon = new Icon(guiManager);
			Color value = new Color(58, 113, 119);
			icon.SetSkinLocation(SkinState.Normal, sourceRectangle3, value, value);
			sourceRectangle3 = guiManager.GUISpriteSheet.GetSourceRectangle("arrowblue_up");
			icon.SetSkinLocation(1, sourceRectangle3, value, value);
			icon.ResizeControlToFitImage();
			icon.CurrentSkin = 0;
			Add(icon);
			icon.X = (Width - icon.Width) / 2;
			icon.Y = (Height - icon.Height) / 2;
			icon.CanHaveFocus = false;
			clickedSound = GUIManager.BeepBasicPanel;
			break;
		}
		case ImageButtonType.LCDSortingArrows:
		{
			Rectangle sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle("basic_button_light");
			Width = sourceRectangle.Width;
			Height = sourceRectangle.Height;
			Skin = sourceRectangle;
			SetSkinLocation(1, sourceRectangle, UIComponent.lcdTooltipHoverTint, UIComponent.lcdTooltipHoverTint);
			sourceRectangle = (PressedSkin = base.GUIManager.GUISpriteSheet.GetSourceRectangle("basic_button_light_in"));
			SetSkinLocation(SkinState.Pressed, sourceRectangle);
			SetSkinLocation(SkinState.CheckedPressed, sourceRectangle, UIComponent.lcdTooltipHoverTint, UIComponent.lcdTooltipHoverTint);
			Rectangle sourceRectangle3 = guiManager.GUISpriteSheet.GetSourceRectangle("basic_icon_sortingArrow_down");
			icon = new Icon(guiManager);
			icon.SetSkinLocation(SkinState.Normal, sourceRectangle3);
			sourceRectangle3 = guiManager.GUISpriteSheet.GetSourceRectangle("basic_icon_sortingArrow_up");
			icon.SetSkinLocation(1, sourceRectangle3);
			icon.ResizeControlToFitImage();
			icon.CurrentSkin = 0;
			icon.Visible = false;
			Add(icon);
			icon.X = (Width - icon.Width) / 2;
			icon.Y = (Height - icon.Height) / 2;
			icon.CanHaveFocus = false;
			clickedSound = GUIManager.BeepBasicPanel;
			break;
		}
		case ImageButtonType.LCD:
		case ImageButtonType.White:
		case ImageButtonType.Black:
			break;
		}
	}

	public void InitWithIcon(ImageButtonType type, string iconSprite, bool hasCheckedState, Color? iconTint = null)
	{
		if (!hasCheckedState)
		{
			CheckedMode = CheckedModes.CannotBeChecked;
		}
		Rectangle sourceRectangle;
		switch (type)
		{
		case ImageButtonType.Counter:
			Init(ImageButtonType.Counter);
			break;
		case ImageButtonType.White:
			sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle("basic_buttonwhite_out");
			Width = sourceRectangle.Width;
			Height = sourceRectangle.Height;
			Skin = sourceRectangle;
			sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle("basic_buttonwhite_hover");
			HoverSkin = sourceRectangle;
			sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle("basic_buttonwhite_in");
			PressedSkin = sourceRectangle;
			clickedSound = GUIManager.BeepBasicPanel;
			break;
		case ImageButtonType.Black:
			sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle("smallblackbutton_out");
			Width = sourceRectangle.Width;
			Height = sourceRectangle.Height;
			Skin = sourceRectangle;
			sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle("smallblackbutton_hover");
			HoverSkin = sourceRectangle;
			sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle("smallblackbutton_in");
			PressedSkin = sourceRectangle;
			clickedSound = GUIManager.BeepBasicPanel;
			break;
		case ImageButtonType.SiteMarker:
			InitButton("map_button_roundSmall", "map_button_roundSmall", UIComponent.lcdHoverTint, UIComponent.lcdPressedTint);
			break;
		case ImageButtonType.SiteMarkerTallPin:
			InitButton("map_button_roundSmall_pin", "map_button_roundSmall_pin", UIComponent.lcdHoverTint, UIComponent.lcdPressedTint);
			iconOffSetYPos = -1;
			break;
		case ImageButtonType.SiteMarkerShortPin:
			InitButton("map_button_roundSmall_pinShort", "map_button_roundSmall_pinShort", UIComponent.lcdHoverTint, UIComponent.lcdPressedTint);
			iconOffSetYPos = -1;
			break;
		case ImageButtonType.LCD:
			_ = iconSprite == "basic_icon_crosshairs";
			sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle("basic_button_light");
			Width = sourceRectangle.Width;
			Height = sourceRectangle.Height;
			Skin = sourceRectangle;
			SetSkinLocation(1, sourceRectangle, UIComponent.lcdTooltipHoverTint, UIComponent.lcdTooltipHoverTint);
			if (!hasCheckedState)
			{
				SetSkinLocation(SkinState.Checked, sourceRectangle);
				SetSkinLocation(SkinState.CheckedPressed, sourceRectangle, UIComponent.lcdTooltipHoverTint, UIComponent.lcdTooltipHoverTint);
				SetSkinLocation(SkinState.CheckedHover, sourceRectangle, UIComponent.lcdTooltipCheckedHoverTint, UIComponent.lcdTooltipCheckedHoverTint);
			}
			sourceRectangle = (PressedSkin = base.GUIManager.GUISpriteSheet.GetSourceRectangle("basic_button_light_in"));
			SetSkinLocation(SkinState.Pressed, sourceRectangle);
			if (hasCheckedState)
			{
				SetSkinLocation(SkinState.Checked, sourceRectangle);
				SetSkinLocation(SkinState.CheckedPressed, sourceRectangle, UIComponent.lcdTooltipHoverTint, UIComponent.lcdTooltipHoverTint);
				SetSkinLocation(SkinState.CheckedHover, sourceRectangle, UIComponent.lcdTooltipCheckedHoverTint, UIComponent.lcdTooltipCheckedHoverTint);
			}
			sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle("basic_button_light_disabled");
			SetSkinLocation(SkinState.Disabled, sourceRectangle);
			clickedSound = GUIManager.BeepBasicPanel;
			break;
		case ImageButtonType.HUD:
			sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle("HUD_button_wide");
			Width = sourceRectangle.Width;
			Height = sourceRectangle.Height;
			Skin = sourceRectangle;
			SetSkinLocation(SkinState.Hover, sourceRectangle, UIComponent.hudHoverTint, UIComponent.hudHoverTint);
			SetSkinLocation(SkinState.Pressed, sourceRectangle, UIComponent.hudPressedTint, UIComponent.hudPressedTint);
			SetSkinLocation(SkinState.Checked, sourceRectangle, UIComponent.hudCheckedTint, UIComponent.hudCheckedTint);
			SetSkinLocation(SkinState.CheckedPressed, sourceRectangle, UIComponent.hudCheckedPressedTint, UIComponent.hudCheckedPressedTint);
			SetSkinLocation(SkinState.CheckedHover, sourceRectangle, UIComponent.hudHoverTint, UIComponent.hudHoverTint);
			SetSkinLocation(SkinState.HoverDisabled, sourceRectangle, UIComponent.hudHoverTintNonEnabled, UIComponent.hudHoverTintNonEnabled);
			sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle("HUD_button_wide_disabled");
			SetSkinLocation(SkinState.Disabled, sourceRectangle);
			clickedSound = GUIManager.BeepBasicPanel;
			break;
		}
		icon = new Icon(guiManager);
		Add(icon);
		icon.CanHaveFocus = false;
		sourceRectangle = guiManager.GUISpriteSheet.GetSourceRectangle(iconSprite);
		icon.SetSkinLocation(SkinState.Normal, sourceRectangle, iconTint, iconTint);
		icon.ResizeControlToFitImage();
		RecalculateIconPosition();
	}

	public void RecalculateIconPosition()
	{
		if (icon != null)
		{
			if (iconOffSetXPos.HasValue)
			{
				icon.X = iconOffSetXPos.Value;
			}
			else
			{
				icon.X = (Width - icon.Width) / 2;
			}
			if (iconOffSetYPos.HasValue)
			{
				icon.Y = iconOffSetYPos.Value;
			}
			else
			{
				icon.Y = (Height - icon.Height) / 2;
			}
		}
	}

	public void SetIconSkinState(int stateIndex)
	{
		icon.CurrentSkin = stateIndex;
	}

	public void SetIconTint(Color color)
	{
		icon.Color = color;
		icon.Visible = true;
	}

	public void SetIconTooltip(string toolTip)
	{
		icon.ToolTip = toolTip;
		icon.Visible = true;
	}

	public void SetIconClick(ClickHandler clickEvent)
	{
		icon.Click += clickEvent;
		icon.Tag1 = Tag1;
	}

	private void InitButton(string normalSprite, string inSprite, string hoverSprite, string checkedHoverSprite = null, Color? checkedHoverTint = null)
	{
		enabledNormalSkin = normalSprite;
		Rectangle value = (Skin = guiManager.GUISpriteSheet.GetSourceRectangle(normalSprite));
		Width = value.Width;
		Height = value.Height;
		if (hoverSprite != null)
		{
			value = guiManager.GUISpriteSheet.GetSourceRectangle(hoverSprite);
		}
		SetSkinLocation(SkinState.Hover, value);
		if (checkedHoverSprite != null)
		{
			value = guiManager.GUISpriteSheet.GetSourceRectangle(checkedHoverSprite);
		}
		SetSkinLocation(SkinState.CheckedHover, value, checkedHoverTint, checkedHoverTint);
		value = (PressedSkin = base.GUIManager.GUISpriteSheet.GetSourceRectangle(inSprite));
		SetSkinLocation(SkinState.Checked, value);
		SetSkinLocation(SkinState.CheckedPressed, value);
	}

	public void InitButton(string normalSprite, string checkedSprite, Color hoverTint, Color pressedTint, Color? normalTint = null, bool modulateHoverColor = false, string disabledSprite = null, bool hasDisabledState = false)
	{
		enabledNormalSkin = normalSprite;
		enabledColor = normalTint;
		Rectangle sourceRectangle = guiManager.GUISpriteSheet.GetSourceRectangle(normalSprite);
		Rectangle sourceRectangle2 = guiManager.GUISpriteSheet.GetSourceRectangle(checkedSprite);
		if (!normalTint.HasValue)
		{
			Skin = sourceRectangle;
			SetSkinLocation(SkinState.Checked, sourceRectangle2);
		}
		else
		{
			SetSkinLocation(SkinState.Normal, sourceRectangle, normalTint.Value, normalTint.Value);
			SetSkinLocation(SkinState.Checked, sourceRectangle2, normalTint.Value, normalTint.Value);
		}
		SetSkinLocation(SkinState.Hover, sourceRectangle, hoverTint, hoverTint, flipHorizontally: false, modulateHoverColor);
		SetSkinLocation(SkinState.Pressed, sourceRectangle, pressedTint, pressedTint);
		if (disabledSprite != null)
		{
			Rectangle sourceRectangle3 = guiManager.GUISpriteSheet.GetSourceRectangle(disabledSprite);
			SetSkinLocation(SkinState.Disabled, sourceRectangle3);
			SetSkinLocation(SkinState.HoverDisabled, sourceRectangle3, hoverTint, hoverTint, flipHorizontally: false, modulateHoverColor);
			SetSkinLocation(SkinState.CheckedDisabled, sourceRectangle3);
			SetSkinLocation(SkinState.CheckedDisabledHover, sourceRectangle3, hoverTint, hoverTint, flipHorizontally: false, modulateHoverColor);
		}
		else if (hasDisabledState)
		{
			SetSkinLocation(SkinState.Disabled, sourceRectangle, UIComponent.lcdDisabledColor, UIComponent.lcdDisabledColor);
			SetSkinLocation(SkinState.HoverDisabled, sourceRectangle, UIComponent.lcdHoverDisabledColor, UIComponent.lcdHoverDisabledColor);
			SetSkinLocation(SkinState.CheckedDisabled, sourceRectangle2, UIComponent.lcdDisabledColor, UIComponent.lcdDisabledColor);
			SetSkinLocation(SkinState.CheckedDisabledHover, sourceRectangle2, UIComponent.lcdHoverDisabledColor, UIComponent.lcdHoverDisabledColor);
		}
		SetSkinLocation(SkinState.CheckedHover, sourceRectangle2, hoverTint, hoverTint, flipHorizontally: false, modulateHoverColor);
		SetSkinLocation(SkinState.CheckedPressed, sourceRectangle2, pressedTint, pressedTint);
		Width = sourceRectangle.Width;
		Height = sourceRectangle.Height;
	}

	public void InitButton(string normalSprite, string checkedSprite, Color hoverTint, Color? normalTint = null)
	{
		enabledNormalSkin = normalSprite;
		enabledColor = normalTint;
		Rectangle sourceRectangle = guiManager.GUISpriteSheet.GetSourceRectangle(normalSprite);
		Rectangle sourceRectangle2 = guiManager.GUISpriteSheet.GetSourceRectangle(checkedSprite);
		if (!normalTint.HasValue)
		{
			Skin = sourceRectangle;
			SetSkinLocation(SkinState.Checked, sourceRectangle2);
		}
		else
		{
			SetSkinLocation(SkinState.Normal, sourceRectangle, normalTint.Value, normalTint.Value);
			SetSkinLocation(SkinState.Checked, sourceRectangle2, normalTint.Value, normalTint.Value);
		}
		SetSkinLocation(SkinState.Hover, sourceRectangle, hoverTint, hoverTint);
		SetSkinLocation(SkinState.Pressed, sourceRectangle2);
		SetSkinLocation(SkinState.CheckedHover, sourceRectangle2, hoverTint, hoverTint);
		SetSkinLocation(SkinState.CheckedPressed, sourceRectangle2);
		Width = sourceRectangle.Width;
		Height = sourceRectangle.Height;
	}

	public void InitButton(string normalSprite, Color hoverTint, Color pressedTint, Color? normalTint = null, bool hasDisabledState = true)
	{
		enabledNormalSkin = normalSprite;
		enabledColor = normalTint;
		Rectangle sourceRectangle = guiManager.GUISpriteSheet.GetSourceRectangle(normalSprite);
		if (!normalTint.HasValue)
		{
			SetSkinLocation(SkinState.Normal, sourceRectangle);
			SetSkinLocation(SkinState.Checked, sourceRectangle);
		}
		else
		{
			SetSkinLocation(SkinState.Normal, sourceRectangle, normalTint.Value, normalTint.Value);
			SetSkinLocation(SkinState.Checked, sourceRectangle, normalTint.Value, normalTint.Value);
		}
		SetSkinLocation(SkinState.CheckedHover, sourceRectangle, hoverTint, hoverTint);
		SetSkinLocation(SkinState.Hover, sourceRectangle, hoverTint, hoverTint);
		SetSkinLocation(SkinState.Pressed, sourceRectangle, pressedTint, pressedTint);
		SetSkinLocation(SkinState.CheckedPressed, sourceRectangle, pressedTint, pressedTint);
		if (hasDisabledState)
		{
			SetSkinLocation(SkinState.Disabled, sourceRectangle, UIComponent.lcdDisabledColor, UIComponent.lcdDisabledColor);
			SetSkinLocation(SkinState.HoverDisabled, sourceRectangle, UIComponent.lcdHoverDisabledColor, UIComponent.lcdHoverDisabledColor);
		}
		Width = sourceRectangle.Width;
		Height = sourceRectangle.Height;
	}

	public void InitButton(string normalSprite, Color hoverTint, Color pressedTint, Color checkedTint, Color checkedHoverTint, Color checkedPressedTint)
	{
		enabledNormalSkin = normalSprite;
		Rectangle value = (Skin = guiManager.GUISpriteSheet.GetSourceRectangle(normalSprite));
		SetSkinLocation(SkinState.Hover, value, hoverTint, hoverTint);
		SetSkinLocation(SkinState.Pressed, value, pressedTint, pressedTint);
		SetSkinLocation(SkinState.Checked, value, checkedTint);
		SetSkinLocation(SkinState.CheckedHover, value, checkedHoverTint, checkedHoverTint);
		SetSkinLocation(SkinState.CheckedPressed, value, checkedPressedTint, checkedPressedTint);
		Width = value.Width;
		Height = value.Height;
	}

	protected override void OnMouseOver(UIComponent sender, MouseEventArgs args)
	{
		base.OnMouseOver(sender, args);
		_ = DebugTag == "cbStockpile";
		if (!Enabled)
		{
			if (string.IsNullOrEmpty(ToolTip))
			{
				return;
			}
			if (CheckedMode != CheckedModes.CannotBeChecked)
			{
				if (IsChecked)
				{
					base.CurrentSkinState = SkinState.CheckedDisabledHover;
				}
				else
				{
					base.CurrentSkinState = SkinState.HoverDisabled;
				}
			}
			else
			{
				base.CurrentSkinState = SkinState.HoverDisabled;
			}
			base.OnMouseOver(sender, args);
		}
		else if (base.IsPressed)
		{
			base.CurrentSkinState = SkinState.CheckedPressed;
		}
		else
		{
			base.CurrentSkinState = SkinState.Hover;
			if (isChecked)
			{
				base.CurrentSkinState = SkinState.CheckedHover;
			}
		}
	}

	protected override void OnMouseOut(UIComponent sender, MouseEventArgs args)
	{
		guiManager.HideToolTip(this);
		if (Enabled)
		{
			if (base.CurrentSkinState == SkinState.Hover || base.CurrentSkinState == SkinState.CheckedHover)
			{
				if (base.IsPressed)
				{
					base.CurrentSkinState = SkinState.Pressed;
				}
				else if (IsChecked)
				{
					base.CurrentSkinState = SkinState.Checked;
				}
				else
				{
					base.CurrentSkinState = SkinState.Normal;
				}
			}
		}
		else if (IsChecked)
		{
			base.CurrentSkinState = SkinState.CheckedDisabled;
		}
		else
		{
			base.CurrentSkinState = SkinState.Disabled;
		}
	}

	protected override void OnMouseDown(MouseEventArgs args)
	{
		base.OnMouseDown(args);
		if (args.Button == MouseButtons.Left)
		{
			base.CurrentSkinState = SkinState.Pressed;
			if (isChecked)
			{
				base.CurrentSkinState = SkinState.CheckedPressed;
			}
			if (clickedSound != null)
			{
				base.GUIManager.PlaySound(clickedSound);
			}
		}
		if (RightClickEnabled && args.Button == MouseButtons.Right)
		{
			base.CurrentSkinState = SkinState.Pressed;
			if (isChecked)
			{
				base.CurrentSkinState = SkinState.CheckedPressed;
			}
			if (clickedSound != null)
			{
				base.GUIManager.PlaySound(clickedSound);
			}
		}
	}

	protected override void OnLoseFocus()
	{
		base.OnLoseFocus();
		UpdateSkinState();
	}

	protected override void OnMouseUp(MouseEventArgs args)
	{
		if (args.Button == MouseButtons.Left && CheckCoordinates(args.Position.X, args.Position.Y))
		{
			if (checkedMode == CheckedModes.CanBeChecked && !isChecked)
			{
				isChecked = true;
			}
			else if (checkedMode == CheckedModes.SwitchCheckedStateOnClick)
			{
				isChecked = !isChecked;
			}
		}
		base.OnMouseUp(args);
		if (args.Button != MouseButtons.Left)
		{
			return;
		}
		if (CheckCoordinates(args.Position.X, args.Position.Y))
		{
			base.CurrentSkinState = SkinState.Hover;
			if (base.IsPressed)
			{
				base.CurrentSkinState = SkinState.Pressed;
			}
			if (isChecked)
			{
				base.CurrentSkinState = SkinState.CheckedHover;
			}
		}
		else
		{
			base.CurrentSkinState = SkinState.Normal;
			if (base.IsPressed)
			{
				base.CurrentSkinState = SkinState.Pressed;
			}
			if (isChecked)
			{
				base.CurrentSkinState = SkinState.Checked;
			}
		}
	}
}
