using System;
using System.Collections.Generic;
using System.Text;
using InputEventSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WindowSystem;

public class Label : UIComponent, IHasText
{
	public enum LabelType
	{
		None,
		PlainPanelNormal,
		PlainPanelMedium,
		CRTBigGlow,
		CRTNormal,
		CRTSmall,
		LCDError,
		LCDBigHeaderBanner,
		LCDNormal,
		LCDNormalLight,
		LCDNormalDark,
		LCDWhite,
		LCDDate,
		LCDWeather,
		LCDCheckbox,
		LCDCheckboxWhite,
		LCDRadioBanner,
		LCDRadio,
		LCDComboBoxItem,
		LCDSmallHeadingBanner,
		LCDSmallHeadingBannerLight,
		RosterTitle,
		HUDWindow,
		HUDWindowHeader,
		EntityTypeTooltip,
		EntityTypeTooltipHeader,
		EntityTypeTooltipSubHeading,
		LCDHeadingBlue,
		LCDHeadingGreen,
		LCDHeadingRed,
		LCDHeadingBrown,
		LCDHeadingGrey,
		LCDHeadingSteelGrey,
		LCDBarfillBlue,
		LCDRadioBannerTinted
	}

	public enum AnimationMode
	{
		None,
		Character,
		Line
	}

	private static string defaultFont;

	private static Color defaultColor;

	private LabelType type;

	private Bar background;

	private Bar hoverBackground;

	public bool EnableCursor;

	private Color? normalColor;

	private Color labelHoverColor = new Color(125, 125, 125, 255);

	private Color? DisabledColor;

	private Color? enabledColor;

	private int leftPadding;

	private int rightPadding;

	private int topPadding;

	private string text;

	private string cursor;

	private bool isCursorShown;

	private SpriteFont font;

	private Color color;

	private bool isRedrawRequired;

	private Viewport viewPort;

	private static RasterizerState scissorTestRasterizerState;

	private AnimationMode animationMode;

	public int? AnimateOnCRTScreenLineNo;

	private static Color digitalGreen;

	public static Color CRTLightBlue;

	private const int lcdCornerHeadingPadding = 6;

	private const int lcdMediumHeadingTopPadding = 1;

	public static Color LCDErrorColor;

	private bool hoverEnabled;

	private int cursorWidth = 15;

	public Color NormalColor
	{
		get
		{
			return normalColor ?? Color;
		}
		set
		{
			if (!(normalColor != value))
			{
				return;
			}
			Color value2 = Color;
			Color? color = normalColor;
			if (value2 == color || Color == defaultColor)
			{
				Color? color2 = normalColor;
				normalColor = value;
				this.color = value;
				Redraw();
				foreach (UIComponent control in base.Controls)
				{
					if (control is Label { normalColor: var color3 } label)
					{
						if (color3 == color2)
						{
							label.Color = this.color;
						}
					}
					else if (control is Hyperlink hyperlink)
					{
						hyperlink.NormalColor = this.color;
					}
				}
			}
			normalColor = value;
		}
	}

	public static string DefaultFont
	{
		set
		{
			defaultFont = value;
		}
	}

	public static Color DefaultColor
	{
		set
		{
			defaultColor = value;
		}
	}

	public AnimationMode AnimateOnCRTScreen
	{
		get
		{
			return animationMode;
		}
		set
		{
			if (animationMode == AnimationMode.Line)
			{
				throw new Exception("Must set line no as well!");
			}
			animationMode = value;
		}
	}

	public string Text
	{
		get
		{
			return text;
		}
		set
		{
			if (background == null && hoverBackground == null)
			{
				base.Controls.Clear();
			}
			else
			{
				base.Controls.RemoveAll((UIComponent c) => !(c is Bar));
			}
			if (value.Contains("§"))
			{
				StringBuilder stringBuilder = new StringBuilder();
				int currentXPos = 0;
				string[] array = value.Split('§');
				for (int num = 0; num < array.Length; num++)
				{
					if (num % 2 == 0)
					{
						if (array[num] != "")
						{
							Label label = new Label(guiManager);
							label.Init(type);
							label.RenderType = base.RenderType;
							Add(label);
							if (normalColor.HasValue)
							{
								label.NormalColor = normalColor.Value;
							}
							label.Text = array[num];
							label.Width = label.TextWidth;
							label.Height = label.TextHeight;
							label.X = currentXPos;
							currentXPos += label.TextWidth;
							stringBuilder.Append(array[num]);
						}
					}
					else
					{
						string text = array[num];
						if (text.StartsWith("E") || text.StartsWith("C") || text.StartsWith("P"))
						{
							ParseHyperlink(stringBuilder, ref currentXPos, text);
						}
						else if (text.StartsWith("L"))
						{
							ParseLabel(stringBuilder, ref currentXPos, text);
						}
						else if (text.StartsWith("I"))
						{
							ParseIcon(stringBuilder, ref currentXPos, text);
						}
					}
				}
				this.text = stringBuilder.ToString();
			}
			else
			{
				this.text = value;
			}
			FitToText();
			Redraw();
			isRedrawRequired = true;
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
				if (enabledColor.HasValue)
				{
					Color = enabledColor.Value;
				}
				if (background != null)
				{
					background.CurrentSkinState = SkinState.Normal;
				}
			}
			else
			{
				if (DisabledColor.HasValue)
				{
					Color = DisabledColor.Value;
				}
				if (background != null)
				{
					background.CurrentSkinState = SkinState.Disabled;
				}
			}
		}
	}

	public bool IsCursorShown
	{
		get
		{
			return isCursorShown;
		}
		set
		{
			if (value != isCursorShown)
			{
				isCursorShown = value;
				Redraw();
				isRedrawRequired = true;
			}
		}
	}

	public SpriteFont Font
	{
		set
		{
			font = value;
			if (base.Controls.Count > 0)
			{
				int num = 0;
				int num2 = 0;
				bool flag = false;
				foreach (UIComponent control in base.Controls)
				{
					if (control is Label label)
					{
						label.Font = value;
						label.X = num;
						label.Width = label.TextWidth;
						num += label.TextWidth;
						num2 += label.Width;
						flag = true;
					}
					else if (control is Hyperlink hyperlink)
					{
						hyperlink.Font = value;
						hyperlink.X = num;
						num += hyperlink.Width;
						num2 += hyperlink.Width;
						flag = true;
					}
					else if (control is Icon icon)
					{
						icon.X = num;
						num += icon.Width;
						num2 += icon.Width;
						flag = true;
					}
				}
				if (flag)
				{
					Width = num2;
				}
				ResetHeight();
			}
			else
			{
				Width = TextWidth;
				cursorWidth = GetTextWidth(cursor);
				ResetHeight();
			}
			Redraw();
			isRedrawRequired = true;
		}
	}

	public Color BackColor
	{
		set
		{
			if (background != null)
			{
				background.SetSkinLocation(SkinState.Normal, null, value, value);
			}
		}
	}

	public Color Color
	{
		get
		{
			return color;
		}
		set
		{
			this.color = value;
			Redraw();
			isRedrawRequired = true;
			foreach (UIComponent control in base.Controls)
			{
				if (control is Label { normalColor: var color } label)
				{
					if (color == normalColor)
					{
						label.Color = this.color;
					}
				}
				else if (control is Hyperlink hyperlink)
				{
					hyperlink.NormalColor = this.color;
				}
			}
		}
	}

	public int TextHeight
	{
		get
		{
			int result = 0;
			if (font != null)
			{
				result = font.LineSpacing;
			}
			return result;
		}
	}

	public int TextWidth => GetTextWidth(Text);

	public override string ToolTip
	{
		get
		{
			return base.ToolTip;
		}
		set
		{
			if (toolTip != value)
			{
				base.ToolTip = value;
				if (!string.IsNullOrEmpty(value))
				{
					EnableHover();
				}
				else
				{
					DisableHover();
				}
			}
		}
	}

	public void SetLineAnimationMode(int lineNo)
	{
		animationMode = AnimationMode.Line;
		AnimateOnCRTScreenLineNo = lineNo;
	}

	public static string ToLabel(string labelText, string color)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("§L");
		stringBuilder.Append(color);
		stringBuilder.Append("¤");
		stringBuilder.Append(labelText);
		stringBuilder.Append("§");
		return stringBuilder.ToString();
	}

	public static string ToLabel(string labelText, Color color)
	{
		return ToLabel(labelText, color.ToHex());
	}

	private void ParseLabel(StringBuilder plainTextBuilder, ref int currentXPos, string token)
	{
		string[] array = token.Split('¤');
		string value = array[1];
		CreateNestedLabel(ref currentXPos, value, array[0], null);
		plainTextBuilder.Append(value);
	}

	private void ParseHyperlink(StringBuilder plainTextBuilder, ref int currentXPos, string token)
	{
		string[] array = token.Split('¤');
		if (LinkIsValid(array[0], out var entityID, out var tilePos, out var containerID))
		{
			Hyperlink hyperlink = new Hyperlink(guiManager)
			{
				Text = array[1],
				X = currentXPos
			};
			hyperlink.RenderType = base.RenderType;
			hyperlink.DebugTag = "hyperlink";
			hyperlink.NormalColor = Color;
			if (containerID.HasValue)
			{
				hyperlink.TargetResourceContainerID = containerID.Value;
			}
			else if (entityID.HasValue)
			{
				hyperlink.TargetEntityID = entityID.Value;
			}
			else
			{
				hyperlink.TargetMapPosition = tilePos.Value;
			}
			Add(hyperlink);
			currentXPos += hyperlink.Width;
		}
		else
		{
			CreateNestedLabel(ref currentXPos, array[1], null, normalColor);
		}
		plainTextBuilder.Append(array[1]);
	}

	private void ParseIcon(StringBuilder plainTextBuilder, ref int currentXPos, string token)
	{
		string[] array = token.Split('¤');
		string text = array[0];
		string spriteName = array[1];
		Icon icon = new Icon(guiManager);
		icon.ScaleImageToSizeOfControl = false;
		icon.X = currentXPos;
		icon.RenderType = base.RenderType;
		Color? color = null;
		if (text != null)
		{
			string colorToken = text.Substring(1, text.Length - 1);
			color = GetColorFromToken(colorToken);
		}
		icon.SetSkinLocation(SkinState.Normal, guiManager.GUISpriteSheet.GetSourceRectangle(spriteName), color, color);
		icon.ResizeControlToFitImage();
		Add(icon);
		currentXPos += icon.Width;
	}

	private void CreateNestedLabel(ref int currentXPos, string text, string colorToken, Color? normalColor)
	{
		Label label = new Label(guiManager);
		label.Text = text;
		label.X = currentXPos;
		label.Init(type);
		label.RenderType = base.RenderType;
		if (colorToken != null)
		{
			string colorToken2 = colorToken.Substring(1, colorToken.Length - 1);
			Color colorFromToken = GetColorFromToken(colorToken2);
			label.NormalColor = colorFromToken;
		}
		else
		{
			label.normalColor = normalColor;
		}
		Add(label);
		currentXPos += label.TextWidth;
	}

	private Color GetColorFromToken(string colorToken)
	{
		if (guiManager.CustomColors.TryGetValue(colorToken, out var value))
		{
			return value;
		}
		return colorToken.ColorFromHex() ?? UIComponent.LCDNormal;
	}

	private void ContainedControlHeightResize(UIComponent sender)
	{
		if (base.Controls.Count <= 0)
		{
			return;
		}
		foreach (UIComponent control in base.Controls)
		{
			control.Height = sender.Height;
		}
	}

	public void SetText(string text)
	{
		Text = text;
		FitToText();
	}

	public static int GetParsedLineWidth(string textWithMetaLinks, GUIManager gui, SpriteFont font)
	{
		int num = 0;
		List<string> iconSprites;
		int textWidth = GetTextWidth(GetPlainText(textWithMetaLinks, collectIconSprites: true, out iconSprites), font);
		if (iconSprites != null)
		{
			foreach (string item in iconSprites)
			{
				if (gui.GUISpriteSheet.TryGetSourceRectangle(item, out var spriteRect))
				{
					num += spriteRect.Value.Width;
				}
			}
		}
		return num + textWidth;
	}

	public static string GetPlainText(string textWithMetaLinks)
	{
		List<string> iconSprites;
		return GetPlainText(textWithMetaLinks, collectIconSprites: false, out iconSprites);
	}

	public static string GetPlainText(string textWithMetaLinks, bool collectIconSprites, out List<string> iconSprites)
	{
		iconSprites = null;
		if (textWithMetaLinks.Contains("§"))
		{
			StringBuilder stringBuilder = new StringBuilder();
			string[] array = textWithMetaLinks.Split('§');
			for (int i = 0; i < array.Length; i++)
			{
				string text = array[i];
				if (i % 2 == 0)
				{
					stringBuilder.Append(text);
				}
				else if (text.StartsWith("I"))
				{
					if (collectIconSprites)
					{
						string[] array2 = text.Split('¤');
						Util.AddToList(ref iconSprites, array2[1]);
					}
				}
				else
				{
					string[] array2 = text.Split('¤');
					stringBuilder.Append(array2[1]);
				}
			}
			return stringBuilder.ToString();
		}
		return textWithMetaLinks;
	}

	private bool LinkIsValid(string link, out uint? entityID, out Point? tilePos, out uint? containerID)
	{
		entityID = null;
		containerID = null;
		tilePos = null;
		if (link.StartsWith("C"))
		{
			if (uint.TryParse(link.Substring(1, link.Length - 1), out var result))
			{
				containerID = result;
				return true;
			}
		}
		else
		{
			if (link.StartsWith("E"))
			{
				if (uint.TryParse(link.Substring(1, link.Length - 1), out var result2))
				{
					entityID = result2;
					return true;
				}
				return false;
			}
			if (link.StartsWith("P"))
			{
				link = link.Replace("P", "");
				string[] array = link.Split(',');
				tilePos = new Point(int.Parse(array[0]), int.Parse(array[1]));
			}
		}
		return true;
	}

	private void ResetHeight()
	{
		if (background == null)
		{
			Height = TextHeight;
		}
		else
		{
			Height = background.Height;
		}
	}

	public Label(GUIManager guiManager)
		: base(guiManager)
	{
		text = string.Empty;
		cursor = "|";
		isCursorShown = false;
		base.CanHaveFocus = false;
		Color = defaultColor;
		base.HeightResize += ContainedControlHeightResize;
		viewPort = default(Viewport);
	}

	static Label()
	{
		defaultFont = "Content/Fonts/DefaultFont";
		defaultColor = Color.Black;
		digitalGreen = new Color(98, 251, 187);
		CRTLightBlue = new Color(188, 216, 242);
		LCDErrorColor = Color.Red;
		scissorTestRasterizerState = new RasterizerState();
		scissorTestRasterizerState.ScissorTestEnable = true;
	}

	public int GetTextWidth(string text)
	{
		int result = 0;
		if (font != null)
		{
			result = GetTextWidth(text, font);
		}
		return result;
	}

	private static int GetTextWidth(string txt, SpriteFont font)
	{
		return (int)font.MeasureString(txt).X;
	}

	public void TintLabelBackground(Color color)
	{
		background.SetSkinLocation(SkinState.Normal, null, color, color);
	}

	public void Init(LabelType type)
	{
		if (this.type != type)
		{
			this.type = type;
			switch (type)
			{
			case LabelType.LCDCheckbox:
			{
				InitNormalBackground("basic_header_small_square", 8);
				ApplyTextFormat(this, type);
				Rectangle sourceRectangle2 = base.GUIManager.GUISpriteSheet.GetSourceRectangle("basic_header_small_square_disabled");
				background.SetSkinLocation(SkinState.Disabled, sourceRectangle2);
				InitDisabledColor(UIComponent.lcdDisabledColor, Color);
				leftPadding = 2;
				rightPadding = 6;
				break;
			}
			case LabelType.LCDCheckboxWhite:
			{
				InitNormalBackground("basic_header_small_square_white", 8);
				ApplyTextFormat(this, type);
				Rectangle sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle("basic_header_small_square_disabled");
				background.SetSkinLocation(SkinState.Disabled, sourceRectangle);
				InitDisabledColor(UIComponent.lcdDisabledColor, Color);
				leftPadding = 2;
				rightPadding = 6;
				break;
			}
			case LabelType.LCDRadioBanner:
				InitNormalBackground("basic_header_small_round", 8);
				ApplyTextFormat(this, type);
				InitDisabledColor(UIComponent.lcdDisabledColor, Color);
				leftPadding = 8;
				rightPadding = 6;
				break;
			case LabelType.LCDRadioBannerTinted:
				InitNormalBackground("basic_header_small_round_white", 8);
				ApplyTextFormat(this, type);
				InitDisabledColor(UIComponent.lcdDisabledColor, Color);
				leftPadding = 8;
				rightPadding = 6;
				break;
			case LabelType.LCDRadio:
				ApplyTextFormat(this, type);
				break;
			case LabelType.LCDSmallHeadingBannerLight:
				InitNormalBackground("basic_header_small_lightgrey", 8);
				ApplyTextFormat(this, type);
				leftPadding = 6;
				rightPadding = 6;
				break;
			case LabelType.LCDSmallHeadingBanner:
				InitNormalBackground("basic_header_small", 8);
				ApplyTextFormat(this, type);
				leftPadding = 6;
				rightPadding = 6;
				break;
			case LabelType.RosterTitle:
				InitNormalBackground("rosterpanel_titleBlack", 46);
				ApplyTextFormat(this, type);
				topPadding = 17;
				leftPadding = 32;
				rightPadding = 38;
				break;
			case LabelType.LCDComboBoxItem:
				ApplyTextFormat(this, type);
				break;
			case LabelType.LCDBarfillBlue:
				InitNormalBackground("lcd_barfill_blue", 10);
				leftPadding = 10;
				topPadding = 3;
				ApplyTextFormat(this, type);
				break;
			case LabelType.LCDBigHeaderBanner:
				InitNormalBackground("basic_header_big", 10);
				leftPadding = 10;
				topPadding = 3;
				ApplyTextFormat(this, type);
				break;
			case LabelType.LCDHeadingBlue:
				InitMediumColoredBanner("basic_header_medium_blue", type);
				break;
			case LabelType.LCDHeadingGreen:
				InitMediumColoredBanner("basic_header_medium_green", type);
				break;
			case LabelType.LCDHeadingRed:
				InitMediumColoredBanner("basic_header_medium_red", type);
				break;
			case LabelType.LCDHeadingBrown:
				InitMediumColoredBanner("basic_header_medium_brown", type);
				break;
			case LabelType.LCDHeadingGrey:
				InitMediumColoredBanner("basic_header_medium_grey", type);
				break;
			case LabelType.LCDHeadingSteelGrey:
				InitMediumColoredBanner("basic_header_medium", type);
				break;
			default:
				ApplyTextFormat(this, type);
				break;
			}
			FitToText();
		}
	}

	private void InitMediumColoredBanner(string sprite, LabelType type)
	{
		InitNormalBackground(sprite, 20);
		leftPadding = 6;
		topPadding = 1;
		rightPadding = 6;
		ApplyTextFormat(this, type);
	}

	public void InitDisabledColor(Color disabledColor, Color enabledColor)
	{
		DisabledColor = disabledColor;
		this.enabledColor = enabledColor;
	}

	private void InitNormalBackground(string backgroundSprite, int edgeSize)
	{
		InitBackground(backgroundSprite, edgeSize, ref background, addNow: true);
	}

	private void InitHoverBackground(string backgroundSprite, int edgeSize)
	{
		InitBackground(backgroundSprite, edgeSize, ref hoverBackground, addNow: false);
	}

	private void InitBackground(string backgroundSprite, int edgeSize, ref Bar backgroundToUse, bool addNow)
	{
		backgroundToUse = new Bar(guiManager);
		if (addNow)
		{
			Add(backgroundToUse);
		}
		backgroundToUse.EdgeSize = edgeSize;
		Rectangle sourceRectangle = base.GUIManager.GUISpriteSheet.GetSourceRectangle(backgroundSprite);
		backgroundToUse.SetSkinLocation(SkinState.Normal, sourceRectangle);
		backgroundToUse.Height = sourceRectangle.Height;
		drawChildrenFirst = true;
	}

	public static void ApplyTextFormat(IHasText component, LabelType type)
	{
		switch (type)
		{
		case LabelType.CRTBigGlow:
			component.NormalColor = Color.White;
			component.Font = GUIManager.LCDandHUDSubheadingFont;
			component.RenderType = RenderType.CRTAndLCD;
			break;
		case LabelType.CRTSmall:
			component.NormalColor = Color.White;
			component.Font = GUIManager.LCDandHUDFont;
			component.RenderType = RenderType.CRTAndLCD;
			break;
		case LabelType.CRTNormal:
			component.NormalColor = Color.White;
			component.Font = GUIManager.LCDandHUDFont;
			component.RenderType = RenderType.CRTAndLCD;
			break;
		case LabelType.PlainPanelNormal:
			component.Font = GUIManager.LCDandHUDFont;
			component.NormalColor = GetNormalColorForType(type);
			break;
		case LabelType.LCDBigHeaderBanner:
		case LabelType.LCDNormal:
		case LabelType.LCDNormalLight:
		case LabelType.LCDNormalDark:
		case LabelType.LCDWhite:
		case LabelType.LCDCheckbox:
		case LabelType.LCDCheckboxWhite:
		case LabelType.LCDRadioBanner:
		case LabelType.LCDRadio:
		case LabelType.LCDSmallHeadingBanner:
		case LabelType.LCDSmallHeadingBannerLight:
		case LabelType.LCDHeadingBlue:
		case LabelType.LCDHeadingGreen:
		case LabelType.LCDHeadingRed:
		case LabelType.LCDHeadingBrown:
		case LabelType.LCDHeadingGrey:
		case LabelType.LCDHeadingSteelGrey:
		case LabelType.LCDBarfillBlue:
		case LabelType.LCDRadioBannerTinted:
			component.Font = GUIManager.LCDandHUDFont;
			component.NormalColor = GetNormalColorForType(type);
			component.RenderType = RenderType.CRTAndLCD;
			if (component is Label label)
			{
				label.labelHoverColor = UIComponent.lcdYellow;
			}
			break;
		case LabelType.LCDComboBoxItem:
			component.Font = GUIManager.LCDandHUDFont;
			component.NormalColor = GetNormalColorForType(type);
			component.RenderType = RenderType.Normal;
			break;
		case LabelType.RosterTitle:
			component.Font = GUIManager.LCDandHUDFont;
			component.NormalColor = GetNormalColorForType(type);
			component.RenderType = RenderType.Normal;
			break;
		case LabelType.LCDDate:
			component.Font = GUIManager.LCDandHUDFont;
			component.NormalColor = GetNormalColorForType(type);
			component.RenderType = RenderType.CRTAndLCD;
			break;
		case LabelType.LCDError:
			component.Font = GUIManager.LCDandHUDFont;
			component.NormalColor = GetNormalColorForType(type);
			component.RenderType = RenderType.CRTAndLCD;
			break;
		case LabelType.LCDWeather:
			component.Font = GUIManager.LCDandHUDFont;
			component.NormalColor = GetNormalColorForType(type);
			component.RenderType = RenderType.CRTAndLCD;
			break;
		case LabelType.EntityTypeTooltip:
			component.Font = GUIManager.LCDandHUDFont;
			component.NormalColor = GetNormalColorForType(type);
			component.RenderType = RenderType.Normal;
			break;
		case LabelType.EntityTypeTooltipHeader:
			component.Font = GUIManager.LCDandHUDFont;
			component.NormalColor = GetNormalColorForType(type);
			component.RenderType = RenderType.Normal;
			break;
		case LabelType.EntityTypeTooltipSubHeading:
			component.Font = GUIManager.LCDandHUDSubheadingFont;
			component.NormalColor = GetNormalColorForType(type);
			component.RenderType = RenderType.Normal;
			break;
		case LabelType.HUDWindow:
			component.Font = GUIManager.LCDandHUDFont;
			component.NormalColor = GetNormalColorForType(type);
			component.RenderType = RenderType.Normal;
			break;
		case LabelType.HUDWindowHeader:
			component.Font = GUIManager.LCDandHUDFont;
			component.NormalColor = GetNormalColorForType(type);
			component.RenderType = RenderType.Normal;
			break;
		case LabelType.PlainPanelMedium:
			break;
		}
	}

	private void EnableHover()
	{
		if (!hoverEnabled)
		{
			base.MouseOver += SetHover;
			base.MouseOut += ResetHover;
			hoverEnabled = true;
			base.CanHaveFocus = true;
		}
	}

	private void DisableHover()
	{
		if (hoverEnabled)
		{
			base.MouseOver -= SetHover;
			base.MouseOut -= ResetHover;
			hoverEnabled = false;
			base.CanHaveFocus = false;
		}
	}

	protected void SetHover(UIComponent sender, MouseEventArgs args)
	{
		if (!normalColor.HasValue)
		{
			normalColor = color;
		}
		color = labelHoverColor;
		if (hoverBackground != null)
		{
			if (background != null)
			{
				Remove(background);
			}
			Add(hoverBackground);
		}
	}

	protected void ResetHover(UIComponent sender, MouseEventArgs args)
	{
		color = normalColor.Value;
		if (hoverBackground != null)
		{
			Remove(hoverBackground);
			if (background != null)
			{
				Add(background);
			}
		}
	}

	public Color GetNormalColorForType()
	{
		return GetNormalColorForType(type);
	}

	private static Color GetNormalColorForType(LabelType type)
	{
		switch (type)
		{
		case LabelType.LCDNormalDark:
		case LabelType.RosterTitle:
			return UIComponent.LCDDark;
		case LabelType.LCDNormal:
		case LabelType.LCDComboBoxItem:
		case LabelType.LCDSmallHeadingBannerLight:
		case LabelType.LCDBarfillBlue:
			return UIComponent.LCDNormal;
		case LabelType.LCDNormalLight:
		case LabelType.LCDCheckbox:
		case LabelType.LCDCheckboxWhite:
		case LabelType.LCDRadioBanner:
		case LabelType.LCDRadio:
		case LabelType.LCDSmallHeadingBanner:
		case LabelType.LCDHeadingBlue:
		case LabelType.LCDHeadingGreen:
		case LabelType.LCDHeadingRed:
		case LabelType.LCDHeadingBrown:
		case LabelType.LCDHeadingGrey:
		case LabelType.LCDHeadingSteelGrey:
			return UIComponent.lcdLight;
		case LabelType.LCDWhite:
			return Color.White;
		case LabelType.LCDDate:
			return Color.LightCyan;
		case LabelType.LCDError:
			return UIComponent.errorColor;
		case LabelType.LCDWeather:
			return Color.Gold;
		case LabelType.LCDBigHeaderBanner:
			return UIComponent.lcdYellow;
		case LabelType.HUDWindow:
			return Color.White;
		case LabelType.HUDWindowHeader:
			return Color.Yellow;
		case LabelType.EntityTypeTooltip:
			return Color.White;
		case LabelType.EntityTypeTooltipHeader:
			return Color.Yellow;
		case LabelType.PlainPanelNormal:
			return Color.Black;
		default:
			return Color.White;
		}
	}

	public void FitToText()
	{
		int num = TextWidth + leftPadding + rightPadding;
		if (background != null)
		{
			num = (int)MathHelper.Max(background.EdgeSize * 2, num);
		}
		if (EnableCursor)
		{
			num += cursorWidth;
		}
		Width = num;
		ResetHeight();
	}

	protected override void LoadGraphicsContent(bool loadAllContent)
	{
		base.LoadGraphicsContent(loadAllContent);
	}

	public override void Initialize()
	{
		isRedrawRequired = true;
		base.Initialize();
	}

	public override void CleanUp()
	{
		_ = base.IsInitialized;
		base.CleanUp();
	}

	public new void UnloadGraphicsContent(bool unloadAllContent)
	{
		isRedrawRequired = true;
		base.UnloadGraphicsContent(unloadAllContent);
	}

	protected override void DrawControl(SpriteBatch spriteBatch, Rectangle parentScissor, float alpha)
	{
		if ((background == null && hoverBackground == null && base.Controls.Count != 0) || font == null)
		{
			return;
		}
		bool flag = true;
		int width = Width;
		Rectangle rectangle = new Rectangle(0, 0, width, Height);
		Rectangle rectangle2 = new Rectangle(base.AbsolutePosition.X + leftPadding, base.AbsolutePosition.Y + topPadding, width, Height);
		if (!parentScissor.Contains(rectangle2))
		{
			if (parentScissor.Intersects(rectangle2))
			{
				if (rectangle2.X < parentScissor.X)
				{
					int num = parentScissor.X - rectangle2.X;
					if (rectangle2.Width == rectangle.Width)
					{
						rectangle.Width -= num;
						rectangle.X += num;
						rectangle2.Width -= num;
						rectangle2.X += num;
					}
					else
					{
						rectangle2.Width -= num;
						rectangle2.X += num;
					}
				}
				else if (rectangle2.Right > parentScissor.Right)
				{
					int num = rectangle2.Right - parentScissor.Right;
					if (rectangle2.Width == rectangle.Width)
					{
						rectangle.Width -= num;
						rectangle2.Width -= num;
					}
					else
					{
						rectangle2.Width -= num;
					}
				}
				if (rectangle2.Y < parentScissor.Y)
				{
					int num = parentScissor.Y - rectangle2.Y;
					if (rectangle2.Height == rectangle.Height)
					{
						rectangle.Height -= num;
						rectangle.Y += num;
						rectangle2.Height -= num;
						rectangle2.Y += num;
					}
					else
					{
						rectangle2.Height -= num;
						rectangle2.Y += num;
					}
				}
				if (rectangle2.Bottom > parentScissor.Bottom)
				{
					int num = rectangle2.Bottom - parentScissor.Bottom;
					if (rectangle2.Height == rectangle.Height)
					{
						rectangle.Height -= num;
						rectangle2.Height -= num;
					}
					else
					{
						rectangle2.Height -= num;
					}
				}
			}
			else
			{
				flag = false;
			}
		}
		if (flag)
		{
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
			spriteBatch.GraphicsDevice.RasterizerState = scissorTestRasterizerState;
			spriteBatch.GraphicsDevice.ScissorRectangle = rectangle2;
			Color color = this.color;
			if (alpha != 1f)
			{
				Vector4 vector = color.ToVector4();
				vector.W *= alpha;
				color = new Color(vector);
			}
			string text = this.text;
			if (isCursorShown)
			{
				text += cursor;
			}
			spriteBatch.DrawString(font, text, new Vector2(base.AbsolutePosition.X + leftPadding, base.AbsolutePosition.Y + topPadding), this.color);
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
			spriteBatch.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
		}
	}

	protected override void OnResize(UIComponent sender)
	{
		viewPort.X = base.X;
		viewPort.Y = base.Y;
		viewPort.Width = Width;
		viewPort.Height = Height;
		if (background != null)
		{
			background.Width = Width;
			background.Height = Height;
		}
		if (hoverBackground != null)
		{
			hoverBackground.Width = Width;
			hoverBackground.Height = Height;
		}
		base.OnResize(sender);
	}
}
