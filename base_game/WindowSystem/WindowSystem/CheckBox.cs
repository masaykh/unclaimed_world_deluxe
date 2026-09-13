using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WindowSystem;

public class CheckBox : UIComponent, ICanBeChecked
{
	private static int defaultHeight = 15;

	private static int defaultHMargin = 5;

	private static string defaultFont = "Content/Fonts/DefaultFont";

	private static Rectangle defaultSkin = new Rectangle(1, 27, 15, 15);

	private static Rectangle defaultHoverSkin = new Rectangle(17, 27, 15, 15);

	private static Rectangle defaultPressedSkin = new Rectangle(33, 27, 15, 15);

	private static Rectangle defaultCheckedSkin = new Rectangle(1, 43, 15, 15);

	private static Rectangle defaultCheckedHoverSkin = new Rectangle(17, 43, 15, 15);

	private static Rectangle defaultCheckedPressedSkin = new Rectangle(33, 43, 15, 15);

	public ImageButton button;

	private Label label;

	private int hMargin;

	private int vMargin;

	private CheckedModes checkedMode = CheckedModes.SwitchCheckedStateOnClick;

	private CheckBoxType type;

	public static int DefaultHeight
	{
		set
		{
			defaultHeight = value;
		}
	}

	public static int DefaultHMargin
	{
		set
		{
			defaultHMargin = value;
		}
	}

	public static string DefaultFont
	{
		set
		{
			defaultFont = value;
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

	public static Rectangle DefaultCheckedSkin
	{
		set
		{
			defaultCheckedSkin = value;
		}
	}

	public static Rectangle DefaultCheckedHoverSkin
	{
		set
		{
			defaultCheckedHoverSkin = value;
		}
	}

	public static Rectangle DefaultCheckedPressedSkin
	{
		set
		{
			defaultCheckedPressedSkin = value;
		}
	}

	public int HMargin
	{
		get
		{
			return hMargin;
		}
		set
		{
			hMargin = value;
			RefreshMargins();
		}
	}

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

	public override EventArgs EventArgs
	{
		get
		{
			return base.EventArgs;
		}
		set
		{
			base.EventArgs = value;
			button.EventArgs = value;
		}
	}

	public bool SwitchStateOnClick
	{
		get
		{
			return button.CheckedMode == CheckedModes.SwitchCheckedStateOnClick;
		}
		set
		{
			if (value)
			{
				button.CheckedMode = CheckedModes.SwitchCheckedStateOnClick;
			}
			else
			{
				button.CheckedMode = CheckedModes.CanBeChecked;
			}
		}
	}

	public int VMargin
	{
		get
		{
			return vMargin;
		}
		set
		{
			vMargin = value;
			RefreshMargins();
		}
	}

	public Color BackColor
	{
		set
		{
			label.BackColor = value;
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
				button.Enabled = value;
				label.Enabled = value;
			}
		}
	}

	public override string ToolTip
	{
		get
		{
			return button.ToolTip;
		}
		set
		{
			button.ToolTip = value;
		}
	}

	public SpriteFont Font
	{
		set
		{
			label.Font = value;
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
			SetWidth();
		}
	}

	public Label Label => label;

	public Rectangle Skin
	{
		set
		{
			button.SetSkinLocation(SkinState.Normal, value);
		}
	}

	public Rectangle HoverSkin
	{
		set
		{
			button.SetSkinLocation(SkinState.Hover, value);
		}
	}

	public Rectangle PressedSkin
	{
		set
		{
			button.SetSkinLocation(2, value);
		}
	}

	public Rectangle CheckedSkin
	{
		set
		{
			button.SetSkinLocation(3, value);
		}
	}

	public Rectangle CheckedHoverSkin
	{
		set
		{
			button.SetSkinLocation(SkinState.CheckedHover, value);
		}
	}

	public Rectangle CheckedPressedSkin
	{
		set
		{
			button.SetSkinLocation(5, value);
		}
	}

	public bool IsChecked
	{
		get
		{
			return button.IsChecked;
		}
		set
		{
			button.IsChecked = value;
		}
	}

	protected ImageButton Button => button;

	public new event ClickHandler Click;

	private void SetWidth()
	{
		Width = label.X + label.Width;
	}

	public void FitToText()
	{
		label.FitToText();
		SetWidth();
	}

	public void SetNormalColor(Color? color)
	{
		button.SetSkinLocation(SkinState.Normal, null, color, color);
		button.SetSkinLocation(SkinState.Checked, null, color, color);
	}

	public CheckBox(GUIManager guiManager)
		: base(guiManager)
	{
		button = new ImageButton(guiManager);
		label = new Label(guiManager);
		Add(button);
		Add(label);
		base.CanHaveFocus = false;
		Width = defaultHeight;
		Height = defaultHeight;
		HMargin = defaultHMargin;
		Skin = defaultSkin;
		HoverSkin = defaultHoverSkin;
		PressedSkin = defaultPressedSkin;
		CheckedSkin = defaultCheckedSkin;
		CheckedHoverSkin = defaultCheckedHoverSkin;
		CheckedPressedSkin = defaultCheckedPressedSkin;
		button.Click += OnClick;
	}

	protected override void LoadGraphicsContent(bool loadAllContent)
	{
		base.LoadGraphicsContent(loadAllContent);
	}

	public void Init(CheckBoxType type)
	{
		Init(type, CheckBoxFlavor.NA);
	}

	public void Init(CheckBoxType type, CheckBoxFlavor flavor)
	{
		this.type = type;
		switch (type)
		{
		case CheckBoxType.LCD:
			button.Init(ImageButtonType.LCDCheckbox);
			label.Init(Label.LabelType.LCDCheckbox);
			Height = (int)MathHelper.Max(button.Height, label.Height);
			CenterChildVertically(label);
			HMargin = 0;
			VMargin = 1;
			break;
		case CheckBoxType.LCDNoLabel:
			button.Init(ImageButtonType.LCDCheckbox);
			Height = button.Height;
			HMargin = 0;
			VMargin = 1;
			break;
		case CheckBoxType.LCDTinting:
			button.Init(ImageButtonType.LCDCheckbox);
			label.Init(Label.LabelType.LCDCheckboxWhite);
			Height = (int)MathHelper.Max(button.Height, label.Height);
			CenterChildVertically(label);
			HMargin = 0;
			VMargin = 1;
			break;
		case CheckBoxType.HUDCheckBox:
			button.Init(ImageButtonType.HUDCheckbox);
			label.Init(Label.LabelType.HUDWindow);
			Height = (int)MathHelper.Max(button.Height, label.Height);
			CenterChildVertically(label);
			HMargin = 0;
			VMargin = 1;
			break;
		case CheckBoxType.LCDRadioBanner:
			button.InitButton("basic_radio_unselected", "basic_radio_selected", UIComponent.lcdHoverTint, UIComponent.lcdPressedTint, null, modulateHoverColor: false, "basic_radio_disabled");
			label.Init(Label.LabelType.LCDRadioBanner);
			Height = button.Height;
			Width = button.Width;
			VMargin = 2;
			HMargin = -6;
			break;
		case CheckBoxType.LCDRadio:
			button.InitButton("basic_radio_unselected", "basic_radio_selected", UIComponent.lcdHoverTint, UIComponent.lcdPressedTint);
			label.Init(Label.LabelType.LCDRadio);
			Height = button.Height;
			Width = button.Width;
			VMargin = 2;
			HMargin = 0;
			break;
		case CheckBoxType.HUDRadio:
			button.InitButton("HUD_radio_empty", "HUD_radio_filled", UIComponent.hudHoverTint, UIComponent.hudPressedTint);
			button.ScaleImageToSizeOfControl = false;
			label.Font = GUIManager.LCDandHUDFont;
			label.NormalColor = Color.White;
			break;
		case CheckBoxType.LED:
		{
			Rectangle rectangle = (PressedSkin = (HoverSkin = (Skin = flavor switch
			{
				CheckBoxFlavor.Blue => guiManager.GUISpriteSheet.GetSourceRectangle("minimap_light_blue_off"), 
				CheckBoxFlavor.Purple => guiManager.GUISpriteSheet.GetSourceRectangle("minimap_light_purple_off"), 
				CheckBoxFlavor.Green => guiManager.GUISpriteSheet.GetSourceRectangle("minimap_light_green_off"), 
				_ => guiManager.GUISpriteSheet.GetSourceRectangle("minimap_light_red_off"), 
			})));
			button.Width = rectangle.Width;
			button.Height = rectangle.Height;
			Height = rectangle.Height;
			rectangle = (CheckedHoverSkin = (CheckedSkin = flavor switch
			{
				CheckBoxFlavor.Blue => guiManager.GUISpriteSheet.GetSourceRectangle("minimap_light_blue_on"), 
				CheckBoxFlavor.Purple => guiManager.GUISpriteSheet.GetSourceRectangle("minimap_light_purple_on"), 
				CheckBoxFlavor.Green => guiManager.GUISpriteSheet.GetSourceRectangle("minimap_light_green_on"), 
				_ => guiManager.GUISpriteSheet.GetSourceRectangle("minimap_light_red_on"), 
			}));
			CheckedPressedSkin = rectangle;
			label.Font = GUIManager.MediumButtonInterfaceFont;
			label.NormalColor = Color.White;
			VMargin = 12;
			HMargin = -4;
			break;
		}
		}
		SetWidth();
	}

	public void InitHUD(Rectangle rect)
	{
		Skin = rect;
		button.SetSkinLocation(SkinState.Hover, rect, UIComponent.hudHoverTint, UIComponent.hudHoverTint);
		button.SetSkinLocation(SkinState.Pressed, rect, UIComponent.hudPressedTint, UIComponent.hudPressedTint);
		button.SetSkinLocation(SkinState.Checked, rect, UIComponent.hudCheckedTint, UIComponent.hudCheckedTint);
		button.SetSkinLocation(SkinState.CheckedPressed, rect, UIComponent.hudCheckedPressedTint, UIComponent.hudCheckedPressedTint);
		button.SetSkinLocation(SkinState.CheckedHover, rect, UIComponent.hudHoverTint, UIComponent.hudHoverTint);
		Font = GUIManager.LCDandHUDFont;
	}

	protected void RefreshMargins()
	{
		label.X = HMargin + button.Width;
		label.Width = Width - button.Width - HMargin;
		if (type != CheckBoxType.LCD && type != CheckBoxType.LCDTinting)
		{
			label.Y = VMargin;
		}
	}

	protected override void OnResize(UIComponent sender)
	{
		base.OnResize(sender);
		if (type != CheckBoxType.LCD && type != CheckBoxType.LCDRadioBanner && type != CheckBoxType.LCDTinting)
		{
			button.Height = Height;
			label.Height = Height;
		}
		RefreshMargins();
	}

	public void OnClick(UIComponent sender, EventArgs e)
	{
		if (this.Click != null)
		{
			this.Click(this, e);
		}
	}
}
