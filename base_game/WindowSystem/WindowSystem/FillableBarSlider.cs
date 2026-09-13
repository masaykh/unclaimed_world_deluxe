using System;
using InputEventSystem;
using Microsoft.Xna.Framework;

namespace WindowSystem;

public class FillableBarSlider : UIComponent
{
	public enum SliderType
	{
		HUD,
		LCD,
		LCDWhite,
		Indicator,
		StockExportSlider,
		StockProductionSlider,
		StockImportSlider
	}

	public enum ShowValueLabelModes
	{
		Always,
		WhenDragging,
		Never
	}

	private Box knob;

	private Label label;

	private const float ShrinkScaleWhenSliderIsBelowThisPercentage = 0.8f;

	public int MinimumSliderWidth = 14;

	private int labelHorizontalPadding = 2;

	private bool isDragging;

	private int value;

	private string maxSliderValueTooltip;

	private SliderType type;

	private Point mouseClickPosition;

	private Point mouseOffsetPosition;

	public bool CanGrow = true;

	private ShowValueLabelModes showValueLabel = ShowValueLabelModes.WhenDragging;

	private const int digitTooltipWidth = 30;

	private bool mouseWasHidden;

	public int Value
	{
		get
		{
			if (type == SliderType.HUD || type == SliderType.LCD || type == SliderType.LCDWhite)
			{
				return parentBar.Value;
			}
			return value;
		}
		set
		{
			if (type != SliderType.HUD && type != SliderType.LCD && type != SliderType.LCDWhite)
			{
				this.value = value;
			}
			if (label != null)
			{
				SetNumberLabel(value);
			}
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
			label.Enabled = value;
			knob.Enabled = value;
			base.Enabled = value;
		}
	}

	public string MaxSliderValueSymbol { get; set; }

	public string MaxSliderValueTooltip
	{
		get
		{
			return maxSliderValueTooltip;
		}
		set
		{
			maxSliderValueTooltip = value;
			TooltipWidth = 80;
		}
	}

	private Game game => guiManager.Game;

	public ShowValueLabelModes ShowValueLabel
	{
		get
		{
			return showValueLabel;
		}
		set
		{
			if (showValueLabel != value)
			{
				showValueLabel = value;
				if (showValueLabel == ShowValueLabelModes.Always)
				{
					Add(label);
				}
				else
				{
					Remove(label);
				}
			}
		}
	}

	public override string ToolTip
	{
		get
		{
			if (IsShowingMaximumSymbol() && MaxSliderValueTooltip != null)
			{
				return MaxSliderValueTooltip;
			}
			return parentBar.GetDisplayValue(parentBar.Value);
		}
	}

	public Color NormalColor
	{
		set
		{
			knob.NormalColor = value;
		}
	}

	public int KnobWidth
	{
		set
		{
			knob.Width = value;
		}
	}

	private FillableBar parentBar => (FillableBar)Parent;

	public Color TextColor
	{
		set
		{
			label.NormalColor = value;
		}
	}

	public override bool Visible
	{
		get
		{
			return base.Visible;
		}
		set
		{
			if (base.Visible != value && (!value || parentBar.MaxValue != 0))
			{
				base.Visible = value;
			}
		}
	}

	public new event MouseUpHandler MouseUp;

	public event EventHandler SliderMouseUp;

	public event EventHandler SliderMouseDown;

	private void SetNumberLabel(int value)
	{
		int num = value / parentBar.StepSize * parentBar.StepSize;
		string text = null;
		if (MaxSliderValueSymbol != null && num == parentBar.MaxValue)
		{
			text = MaxSliderValueSymbol;
		}
		if (text == null)
		{
			text = ((parentBar.DisplayValueFunction != null) ? parentBar.DisplayValueFunction(num) : ((num <= 99) ? num.ToString() : "**"));
		}
		label.Text = text;
	}

	private bool IsShowingMaximumSymbol()
	{
		if (MaxSliderValueSymbol != null)
		{
			return parentBar.Value == parentBar.MaxValue;
		}
		return false;
	}

	public FillableBarSlider(GUIManager guiManager, SliderType type, Label.LabelType labelType, ShowValueLabelModes showValueLabel)
		: base(guiManager)
	{
		this.type = type;
		knob = new Box(guiManager);
		label = new Label(guiManager);
		TooltipWidth = 30;
		switch (type)
		{
		case SliderType.LCD:
			InitSliderKnobSingleLine(labelType, "lcd_bar_knob", "lcd_bar_knob_disabled", 6, 4, 1, -2, isMovable: true, showValueLabel);
			break;
		case SliderType.LCDWhite:
			InitSliderKnobSingleLine(labelType, "lcd_bar_knob_white", "lcd_bar_knob_disabled", 6, 4, 1, -2, isMovable: true, showValueLabel);
			break;
		case SliderType.HUD:
			InitSliderKnobSingleLine(labelType, "HUD_slider_knob", "HUD_slider_knob", 4, 3, 0, 0, isMovable: true, showValueLabel);
			break;
		case SliderType.Indicator:
			InitSliderKnobSingleLine(labelType, "lcd_bar_indicatorLine", "lcd_bar_indicatorLine", 1, 0, 1, -2, isMovable: false, showValueLabel);
			break;
		case SliderType.StockExportSlider:
		{
			Rectangle sourceRectangle = guiManager.GUISpriteSheet.GetSourceRectangle("ordercontrol_handle_E");
			base.ZOrder = 0.1f;
			InitSliderKnobMultipleLines(labelType, sourceRectangle);
			break;
		}
		case SliderType.StockProductionSlider:
		{
			Rectangle sourceRectangle = guiManager.GUISpriteSheet.GetSourceRectangle("ordercontrol_handle_E");
			base.ZOrder = 0.2f;
			InitSliderKnobMultipleLines(labelType, sourceRectangle);
			break;
		}
		case SliderType.StockImportSlider:
		{
			Rectangle sourceRectangle = guiManager.GUISpriteSheet.GetSourceRectangle("ordercontrol_handle_E");
			base.ZOrder = 0.3f;
			InitSliderKnobMultipleLines(labelType, sourceRectangle);
			break;
		}
		}
		Width = knob.Width;
		Height = knob.Height + knob.Y;
		value = 0;
	}

	private void InitSliderKnobSingleLine(Label.LabelType labelType, string sprite, string disabledSprite, int cornerSize, int labelHorizontalPadding, int labelYPosition, int sliderYPosition, bool isMovable, ShowValueLabelModes showValueLabel)
	{
		Add(knob);
		Rectangle sourceRectangle = guiManager.GUISpriteSheet.GetSourceRectangle(sprite);
		knob.SetSkinLocation(SkinState.Normal, sourceRectangle);
		knob.SetSkinLocation(SkinState.Hover, sourceRectangle, UIComponent.lcdTooltipHoverTint, UIComponent.lcdTooltipHoverTint);
		sourceRectangle = guiManager.GUISpriteSheet.GetSourceRectangle(disabledSprite);
		knob.SetSkinLocation(SkinState.Disabled, sourceRectangle);
		knob.CornerSize = cornerSize;
		knob.Width = sourceRectangle.Width;
		knob.Height = sourceRectangle.Height;
		ShowValueLabel = showValueLabel;
		if (isMovable)
		{
			base.CanHaveFocus = true;
			ToolTip = "-";
		}
		else
		{
			base.CanHaveFocus = false;
		}
		base.Position = new Point(-knob.X, sliderYPosition);
		this.labelHorizontalPadding = labelHorizontalPadding;
		if (ShowValueLabel != ShowValueLabelModes.Never)
		{
			label.Y = labelYPosition;
			label.Init(labelType);
			label.Text = "0";
			SetKnobAndLabelSize();
		}
	}

	private void InitSliderKnobMultipleLines(Label.LabelType labelType, Rectangle rect)
	{
		Add(knob);
		knob.SetSkinLocation(SkinState.Normal, rect);
		base.MouseMove += Icon_MouseMove;
		base.MouseDown += Icon_MouseDown;
		MouseUp += Icon_MouseUp;
		base.Position = new Point(-knob.X, 0);
		label.Position = new Point(knob.X, 0);
		label.Init(labelType);
		label.Text = "";
		label.Width = knob.Width;
	}

	private void Icon_MouseUp(MouseEventArgs args)
	{
		if (Enabled)
		{
			isDragging = false;
			int sliderValue = Value / parentBar.StepSize * parentBar.StepSize;
			SetSliderValue(sliderValue);
			if (ShowValueLabel == ShowValueLabelModes.WhenDragging)
			{
				Remove(label);
			}
			game.IsMouseVisible = true;
			mouseWasHidden = false;
			SetMouseCursorPosition();
			UpdateSliderPositionAndSize(null);
			if (this.SliderMouseUp != null)
			{
				this.SliderMouseUp(this, EventArgs);
			}
			if (this.MouseUp != null)
			{
				this.MouseUp(args);
			}
		}
	}

	private void SetMouseCursorPosition()
	{
		guiManager.SetMousePosition(base.AbsolutePosition.X + mouseOffsetPosition.X, base.AbsolutePosition.Y + mouseOffsetPosition.Y);
	}

	private void Icon_MouseDown(MouseEventArgs args)
	{
		if (Enabled)
		{
			isDragging = true;
			if (ShowValueLabel == ShowValueLabelModes.WhenDragging)
			{
				Add(label);
			}
			mouseClickPosition = args.Position;
			mouseOffsetPosition = new Point(args.Position.X - base.AbsolutePosition.X, args.Position.Y - base.AbsolutePosition.Y);
			if (this.SliderMouseDown != null)
			{
				this.SliderMouseDown(this, EventArgs);
			}
			mouseWasHidden = true;
			game.IsMouseVisible = false;
		}
	}

	protected override void OnMouseMove(MouseEventArgs args)
	{
		base.OnMouseMove(args);
		Icon_MouseMove(args);
	}

	protected override void OnMouseOver(UIComponent sender, MouseEventArgs args)
	{
		base.OnMouseOver(sender, args);
		if (Enabled && !isDragging)
		{
			knob.CurrentSkinState = SkinState.Hover;
		}
	}

	protected override void OnMouseOut(UIComponent sender, MouseEventArgs args)
	{
		base.OnMouseOut(sender, args);
		if (Enabled)
		{
			knob.CurrentSkinState = SkinState.Normal;
		}
	}

	protected override void OnMouseUp(MouseEventArgs args)
	{
		base.OnMouseUp(args);
		Icon_MouseUp(args);
	}

	protected override void OnMouseDown(MouseEventArgs args)
	{
		base.OnMouseDown(args);
		Icon_MouseDown(args);
	}

	protected override void OnLoseFocus()
	{
		base.OnLoseFocus();
		isDragging = false;
	}

	private void Icon_MouseMove(MouseEventArgs args)
	{
		if (Enabled && isDragging)
		{
			int num = base.X + knob.Width / 2 - parentBar.SliderScaleStartX;
			int num2 = args.Position.X - mouseClickPosition.X;
			_ = 0;
			mouseClickPosition = base.AbsolutePosition;
			guiManager.SetMousePosition(mouseClickPosition.X, mouseClickPosition.Y + Height / 2);
			num += num2;
			num = Math.Max(num, 0);
			if (!CanGrow)
			{
				num = Math.Min(num, parentBar.BarWidth);
				int valueFromPosition = GetValueFromPosition(num);
				SetSliderValue(valueFromPosition);
			}
			else
			{
				int sliderValue = ((num <= parentBar.BarWidth || parentBar.MaxValue >= parentBar.GrowToMaximum) ? GetValueFromPosition(num) : (parentBar.MaxValue + 1));
				SetSliderValue(sliderValue);
				num = Math.Min(num, parentBar.BarWidth);
			}
			UpdateSliderPositionAndSize(num);
		}
	}

	public override void CleanUp()
	{
		if (mouseWasHidden && !game.IsMouseVisible)
		{
			game.IsMouseVisible = true;
			mouseWasHidden = false;
		}
		base.CleanUp();
	}

	public void UpdateSliderPositionAndSize(int? positionOnSlider)
	{
		if (!positionOnSlider.HasValue)
		{
			positionOnSlider = GetPositionFromValue();
		}
		if (parentBar.MaxValue == 0)
		{
			Visible = false;
		}
		else
		{
			Visible = true;
			if (ShowValueLabel != ShowValueLabelModes.Never)
			{
				SetNumberLabel(Value);
				SetKnobAndLabelSize();
			}
		}
		int num = 0;
		if (Value == parentBar.MaxValue && knob.Width < 4)
		{
			num = -1;
		}
		base.X = positionOnSlider.Value - knob.Width / 2 + parentBar.SliderScaleStartX + num;
	}

	private void SetKnobAndLabelSize()
	{
		label.FitToText();
		int width = Math.Max(MinimumSliderWidth, label.Width + 2 * labelHorizontalPadding);
		knob.Width = width;
		Width = knob.Width;
		knob.CenterChildHorizontally(label);
	}

	public int GetValueFromPosition(int positionOnSlider, bool roundToNearestStep = false)
	{
		int result = (int)Math.Round((float)positionOnSlider / (float)parentBar.BarWidth * (float)parentBar.MaxValue);
		if (roundToNearestStep)
		{
			return FillableBar.RoundToIncrements(result, parentBar.StepSize);
		}
		return result;
	}

	private int GetPositionFromValue()
	{
		int f = parentBar.MaxValue / parentBar.StepSize;
		f = Util.Clamp(f, 1, 10000000);
		return (int)((float)(Value / parentBar.StepSize) * ((float)parentBar.BarWidth / (float)f));
	}

	private void SetSliderValue(int newValue)
	{
		if (newValue < 0)
		{
			newValue = 0;
		}
		if (type == SliderType.HUD || type == SliderType.LCD || type == SliderType.LCDWhite)
		{
			parentBar.Value = newValue;
		}
		else
		{
			value = newValue;
		}
		if (CanGrow)
		{
			if (newValue > parentBar.MaxValue && newValue <= parentBar.GrowToMaximum)
			{
				parentBar.MaxValue = newValue;
			}
			else if (parentBar.MaxValue > parentBar.GrowScaleFromThisValue && (float)newValue < 0.8f * (float)parentBar.MaxValue)
			{
				parentBar.MaxValue -= 1;
			}
		}
	}

	private void MoveSliderToPosition(Point pos)
	{
	}
}
