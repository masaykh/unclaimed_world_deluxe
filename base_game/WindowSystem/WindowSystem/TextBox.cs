using InputEventSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace WindowSystem;

public class TextBox : UIComponent
{
	public enum TextBoxType
	{
		HUD,
		LCD,
		LCDCombo
	}

	private static int defaultWidth = 260;

	private static int defaultHeight = 25;

	private static int defaultHMargin = 5;

	private static int defaultVMargin = 3;

	private static string defaultFont = "Content/Fonts/DefaultFont";

	private static Rectangle defaultSkin = new Rectangle(84, 41, 25, 25);

	private Box box;

	private Label label;

	private double seconds;

	private int hMargin;

	private int vMargin;

	public bool IsNumericBox;

	public int NoOfDecimals;

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

	public static int DefaultHMargin
	{
		set
		{
			defaultHMargin = value;
		}
	}

	public static int DefaultVMargin
	{
		set
		{
			defaultVMargin = value;
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

	public bool IsEditable
	{
		get
		{
			return base.CanHaveFocus;
		}
		set
		{
			base.CanHaveFocus = value;
			label.EnableCursor = value;
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

	public Rectangle Skin
	{
		set
		{
			box.SetSkinLocation(SkinState.Normal, value);
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
				box.Enabled = value;
			}
		}
	}

	public int GetTextWidth(string text)
	{
		string text2 = label.Text;
		label.Text = text;
		int textWidth = label.TextWidth;
		label.Text = text2;
		return textWidth;
	}

	public TextBox(GUIManager guiManager)
		: base(guiManager)
	{
		seconds = 0.0;
		box = new Box(guiManager);
		label = new Label(guiManager);
		label.DebugTag = "textboxLabel";
		Add(box);
		Add(label);
		Width = defaultWidth;
		Height = defaultHeight;
		HMargin = defaultHMargin;
		VMargin = defaultVMargin;
		Rectangle sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle("lcd_textbox");
		Skin = sourceRectangle;
		Font = GUIManager.LCDandHUDFont;
		base.CanHaveFocus = false;
	}

	protected override void LoadGraphicsContent(bool loadAllContent)
	{
		base.LoadGraphicsContent(loadAllContent);
	}

	public override void Update(GameTime gameTime)
	{
		if (Enabled && IsEditable)
		{
			if (base.GUIManager.GetFocus() != this)
			{
				if (label.IsCursorShown)
				{
					label.IsCursorShown = false;
					seconds = 0.0;
				}
			}
			else
			{
				seconds += gameTime.ElapsedGameTime.TotalSeconds;
				if (seconds > 0.5)
				{
					label.IsCursorShown = false;
				}
				else
				{
					label.IsCursorShown = true;
				}
				if (seconds > 1.0)
				{
					seconds = 0.0;
				}
			}
		}
		else
		{
			label.IsCursorShown = false;
		}
		base.Update(gameTime);
	}

	public float? GetNumber()
	{
		if (float.TryParse(Text, out var result))
		{
			return result;
		}
		return null;
	}

	public int? GetNumberAsInt()
	{
		if (int.TryParse(Text, out var result))
		{
			return result;
		}
		return null;
	}

	protected void RefreshMargins()
	{
		label.X = hMargin;
		label.Width = Width - hMargin * 2;
		label.Y = vMargin;
		label.Height = Height - vMargin * 2;
	}

	public void Init(TextBoxType type)
	{
		string text = null;
		switch (type)
		{
		case TextBoxType.HUD:
			label.Init(Label.LabelType.HUDWindow);
			text = "HUD_textbox";
			box.SetSkinLocation(SkinState.Normal, guiManager.GUISpriteSheet.GetSourceRectangle(text));
			Height = 27;
			break;
		case TextBoxType.LCD:
		{
			label.Init(Label.LabelType.LCDNormal);
			text = "basic_colorfield_blue";
			Rectangle sourceRectangle2 = guiManager.GUISpriteSheet.GetSourceRectangle(text);
			box.SetSkinLocation(SkinState.Normal, sourceRectangle2);
			box.SetSkinLocation(SkinState.Hover, sourceRectangle2, UIComponent.lcdHoverTint, UIComponent.lcdHoverTint);
			box.SetSkinLocation(SkinState.Disabled, sourceRectangle2, UIComponent.lcdDisabledColor, UIComponent.lcdDisabledColor);
			box.DebugTag = "TextBox";
			Height = sourceRectangle2.Height;
			Font = GUIManager.LCDandHUDFont;
			RenderType = RenderType.CRTAndLCD;
			break;
		}
		case TextBoxType.LCDCombo:
		{
			label.Init(Label.LabelType.LCDNormal);
			text = "basic_dropdown_light";
			Rectangle sourceRectangle = guiManager.GUISpriteSheet.GetSourceRectangle(text);
			box.SetSkinLocation(SkinState.Normal, sourceRectangle);
			box.SetSkinLocation(SkinState.Disabled, sourceRectangle, UIComponent.lcdDisabledColor, UIComponent.lcdDisabledColor);
			box.CornerSize = 15;
			Height = sourceRectangle.Height;
			Font = GUIManager.LCDandHUDFont;
			RenderType = RenderType.CRTAndLCD;
			break;
		}
		}
	}

	protected override void OnKeyDown(KeyEventArgs args)
	{
		base.OnKeyDown(args);
		if (!IsEditable)
		{
			return;
		}
		if (args.Key == Keys.Back)
		{
			if (label.Text.Length > 0)
			{
				label.Text = label.Text.Substring(0, label.Text.Length - 1);
			}
		}
		else if (!args.Alt && !args.Control)
		{
			if (!IsNumericBox)
			{
				label.Text += GUIManager.KeyToString(args);
				return;
			}
			label.Text += GUIManager.NumberKeyToString(args, NoOfDecimals > 0);
			label.X = Width - 4 - label.TextWidth;
		}
	}

	protected override void OnMouseOut(UIComponent sender, MouseEventArgs args)
	{
		base.OnMouseOut(sender, args);
		if (IsEditable)
		{
			if (Enabled)
			{
				box.CurrentSkinState = SkinState.Normal;
			}
			else
			{
				box.CurrentSkinState = SkinState.Disabled;
			}
		}
	}

	protected override void OnMouseOver(UIComponent sender, MouseEventArgs args)
	{
		base.OnMouseOver(sender, args);
		if (IsEditable)
		{
			if (Enabled)
			{
				box.CurrentSkinState = SkinState.Hover;
			}
			else
			{
				box.CurrentSkinState = SkinState.HoverDisabled;
			}
		}
	}

	protected override void OnResize(UIComponent sender)
	{
		base.OnResize(sender);
		box.Width = Width;
		box.Height = Height;
		RefreshMargins();
	}
}
