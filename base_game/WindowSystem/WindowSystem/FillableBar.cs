using System;
using System.Collections.Generic;
using InputEventSystem;
using Microsoft.Xna.Framework;

namespace WindowSystem;

public class FillableBar : UIComponent
{
	public enum FillableBarType
	{
		Default,
		ProgressBar,
		HUDSlider,
		HUDSliderWhite,
		LCDSlider,
		LCDSliderWhite,
		LCDIndicator,
		StockOrders
	}

	private static int defaultEdgeSize = 4;

	private Bar underBar;

	private Bar valueBar;

	private int valueBarIndex;

	private Label lblMaxValue;

	private FillableBarSlider slider;

	private FillableBarSlider slider2;

	private FillableBarSlider slider3;

	private TextButton tbDecrease;

	private TextButton tbIncrease;

	private List<Icon> notchesPool;

	private List<Icon> notchesInUse;

	private Rectangle underBarRect;

	private Rectangle valueBarRect;

	private int maxValue;

	private int barValue;

	private Rectangle skin;

	private FillableBarType type;

	private Color color;

	private float sliderButtonDelay;

	private float timeBetweenSliderButtonIncrements;

	public Func<int, string> DisplayValueFunction;

	private bool showNotches;

	private int stepSize = 1;

	public int GrowScaleFromThisValue;

	public int GrowToMaximum;

	private bool isDecreasing;

	private bool isIncreasing;

	private float buttonDownStarted;

	private bool hoverEnabled;

	private bool showMaxValueLabelAtEnd = true;

	private const int buttonMargin = 0;

	private const int distanceToMaxValueLabel = 24;

	private int barLeftMargin = 8;

	private int barRightMargin = 10;

	public bool ShowNotches
	{
		set
		{
			if (showNotches != value)
			{
				showNotches = value;
				UpdateNotches();
			}
		}
	}

	public int MaxValue
	{
		get
		{
			return maxValue;
		}
		set
		{
			if (value < 0)
			{
				throw new Exception();
			}
			SetValues(barValue, SliderValue, value);
			UpdateNotches();
			if (MaxValue < 0)
			{
				throw new Exception();
			}
		}
	}

	public int StepSize
	{
		get
		{
			return stepSize;
		}
		set
		{
			if (stepSize != value)
			{
				stepSize = value;
				UpdateNotches();
			}
		}
	}

	public int BarWidth => underBar.Width;

	public string MaxSliderValueSymbol
	{
		get
		{
			if (slider != null)
			{
				return slider.MaxSliderValueSymbol;
			}
			return null;
		}
		set
		{
			if (slider != null)
			{
				slider.MaxSliderValueSymbol = value;
			}
		}
	}

	public string MaxSliderValueTooltip
	{
		set
		{
			if (slider != null)
			{
				slider.MaxSliderValueTooltip = value;
			}
		}
	}

	public int Value
	{
		get
		{
			return barValue;
		}
		set
		{
			SetValues(value, SliderValue, maxValue);
		}
	}

	public int SliderValue
	{
		get
		{
			if (slider != null)
			{
				return slider.Value;
			}
			return barValue;
		}
		set
		{
			SetValues(barValue, value, maxValue);
		}
	}

	private bool ValueBarAndSliderAreLocked => type != FillableBarType.LCDIndicator;

	public Rectangle Skin
	{
		get
		{
			return skin;
		}
		set
		{
			skin = value;
		}
	}

	public FillableBarType Type
	{
		get
		{
			return type;
		}
		set
		{
			type = value;
		}
	}

	public Color KnobTextColor
	{
		set
		{
			if (slider != null)
			{
				slider.TextColor = value;
			}
			if (slider2 != null)
			{
				slider2.TextColor = value;
			}
			if (slider3 != null)
			{
				slider3.TextColor = value;
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
			if (base.Enabled != value)
			{
				base.Enabled = value;
				underBar.Enabled = value;
				lblMaxValue.Enabled = value;
				if (slider != null)
				{
					slider.Enabled = value;
				}
				if (tbIncrease != null)
				{
					tbIncrease.Enabled = value;
				}
				if (tbDecrease != null)
				{
					tbDecrease.Enabled = value;
				}
				if (base.Enabled)
				{
					valueBar.CurrentSkinState = SkinState.Normal;
				}
				else
				{
					valueBar.CurrentSkinState = SkinState.Disabled;
				}
			}
		}
	}

	public Color? ButtonColor
	{
		set
		{
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
			if (color != value)
			{
				color = value;
				underBar.SetSkinLocation(SkinState.Normal, null, color, color);
				valueBar.SetSkinLocation(SkinState.Normal, null, color, color);
			}
		}
	}

	public Color ColorAllControls
	{
		get
		{
			return color;
		}
		set
		{
			if (color != value)
			{
				color = value;
				underBar.SetSkinLocation(SkinState.Normal, null, color, color);
				valueBar.SetSkinLocation(SkinState.Normal, null, color, color);
				if (tbIncrease != null)
				{
					tbIncrease.NormalColor = value;
				}
				if (tbDecrease != null)
				{
					tbDecrease.NormalColor = value;
				}
				if (slider != null)
				{
					slider.NormalColor = value;
				}
			}
		}
	}

	public Color BarColor
	{
		get
		{
			return color;
		}
		set
		{
			color = value;
			valueBar.SetSkinLocation(SkinState.Normal, valueBarRect, color, color);
		}
	}

	public Color UnderBarColor
	{
		set
		{
			underBar.SetSkinLocation(SkinState.Normal, valueBarRect, value, value);
		}
	}

	private string Text
	{
		get
		{
			return lblMaxValue.Text;
		}
		set
		{
			lblMaxValue.Text = value;
			lblMaxValue.FitToText();
		}
	}

	public string SliderTooltip
	{
		set
		{
			if (slider != null)
			{
				slider.ToolTip = value;
			}
		}
	}

	public string ButtonTooltip
	{
		set
		{
			if (tbDecrease != null)
			{
				tbDecrease.ToolTip = value;
				tbIncrease.ToolTip = value;
			}
		}
	}

	public int KnobWidth
	{
		set
		{
			slider.KnobWidth = value;
		}
	}

	public FillableBarSlider.ShowValueLabelModes ShowValueLabel
	{
		get
		{
			return slider.ShowValueLabel;
		}
		set
		{
			slider.ShowValueLabel = value;
		}
	}

	public int SliderScaleStartX => GetBarLeftPos();

	public bool ShowMaxValueLabelAtEnd
	{
		get
		{
			return showMaxValueLabelAtEnd;
		}
		set
		{
			if (value != showMaxValueLabelAtEnd)
			{
				showMaxValueLabelAtEnd = value;
				if (!showMaxValueLabelAtEnd)
				{
					Remove(lblMaxValue);
				}
				else
				{
					Add(lblMaxValue);
				}
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
			if (base.Width != value)
			{
				base.Width = value;
				int num = Width;
				if (type == FillableBarType.HUDSlider || type == FillableBarType.HUDSliderWhite)
				{
					num = Width - 58;
				}
				else if (type == FillableBarType.LCDSlider || type == FillableBarType.LCDSliderWhite)
				{
					num = Width - 45;
				}
				if (tbDecrease != null)
				{
					num = num - tbDecrease.Width - tbIncrease.Width;
				}
				underBar.Width = num;
				underBar.MaxWidth = num;
				underBarRect.Width = num;
				UpdateSizesAndPositionsWithNewWidth();
			}
		}
	}

	public event EventHandler SliderMouseUp;

	public event EventHandler SliderMouseDown;

	public FillableBar(GUIManager guiManager, FillableBarType type, bool canGrow = true, bool includeButtons = false, float? timeBetweenButtonIncrements = null, float? sliderButtonDelay = null)
		: base(guiManager)
	{
		Type = type;
		switch (type)
		{
		case FillableBarType.Default:
		case FillableBarType.ProgressBar:
			underBar = new Bar(guiManager);
			valueBar = new Bar(guiManager);
			lblMaxValue = new Label(guiManager);
			valueBarRect = guiManager.GUISpriteSheet.GetSourceRectangle("lcd_barfill_white");
			valueBar.SetSkinLocation(SkinState.Normal, valueBarRect);
			valueBar.SetSkinLocation(SkinState.Disabled, valueBarRect, Color.Gray, Color.Gray);
			valueBar.EdgeSize = 2;
			valueBar.Height = valueBarRect.Height;
			Add(underBar);
			valueBarIndex = Add(valueBar);
			underBarRect = guiManager.GUISpriteSheet.GetSourceRectangle("lcd_barfill_grey");
			underBar.SetSkinLocation(SkinState.Normal, underBarRect);
			underBar.EdgeSize = 2;
			underBar.Height = underBarRect.Height;
			barLeftMargin = 0;
			Height = underBarRect.Height;
			EnableHover();
			break;
		case FillableBarType.LCDSlider:
			InitBarWithSlider(guiManager, Label.LabelType.LCDNormal, FillableBarSlider.SliderType.LCD, canGrow, "lcd_bar_empty", "lcd_bar_filled", 2, FillableBarSlider.ShowValueLabelModes.Always, canMoveSlider: true, includeButtons, sliderButtonDelay, timeBetweenButtonIncrements);
			break;
		case FillableBarType.LCDSliderWhite:
			InitBarWithSlider(guiManager, Label.LabelType.LCDNormal, FillableBarSlider.SliderType.LCDWhite, canGrow, "lcd_bar_empty", "lcd_bar_filled_white", 2, FillableBarSlider.ShowValueLabelModes.Always, canMoveSlider: true, includeButtons, sliderButtonDelay, timeBetweenButtonIncrements);
			break;
		case FillableBarType.HUDSlider:
			InitBarWithSlider(guiManager, Label.LabelType.HUDWindow, FillableBarSlider.SliderType.HUD, canGrow, "HUD_slider_base", "HUD_slider_fill", 3, FillableBarSlider.ShowValueLabelModes.Always, canMoveSlider: true, includeButtons, sliderButtonDelay, timeBetweenButtonIncrements);
			break;
		case FillableBarType.HUDSliderWhite:
			InitBarWithSlider(guiManager, Label.LabelType.HUDWindow, FillableBarSlider.SliderType.HUD, canGrow, "HUD_slider_base_white", "HUD_slider_fill_white", 3, FillableBarSlider.ShowValueLabelModes.Always, canMoveSlider: true, includeButtons, sliderButtonDelay, timeBetweenButtonIncrements);
			break;
		case FillableBarType.LCDIndicator:
			InitBarWithSlider(guiManager, Label.LabelType.LCDNormal, FillableBarSlider.SliderType.Indicator, canGrow: false, "lcd_bar_empty", "lcd_bar_filled_white", 2, FillableBarSlider.ShowValueLabelModes.Never, canMoveSlider: false, includeButtons: false, null, null);
			DebugTag = "indicatorBar";
			EnableHover();
			break;
		case FillableBarType.StockOrders:
			underBar = new Bar(guiManager);
			valueBar = new Bar(guiManager);
			lblMaxValue = new Label(guiManager);
			lblMaxValue.ZOrder = 1f;
			slider = new FillableBarSlider(guiManager, FillableBarSlider.SliderType.StockExportSlider, Label.LabelType.LCDNormal, FillableBarSlider.ShowValueLabelModes.WhenDragging);
			slider2 = new FillableBarSlider(guiManager, FillableBarSlider.SliderType.StockProductionSlider, Label.LabelType.LCDNormal, FillableBarSlider.ShowValueLabelModes.WhenDragging);
			slider3 = new FillableBarSlider(guiManager, FillableBarSlider.SliderType.StockImportSlider, Label.LabelType.LCDNormal, FillableBarSlider.ShowValueLabelModes.WhenDragging);
			Add(underBar);
			valueBarIndex = Add(valueBar);
			Add(slider);
			Add(slider2);
			Add(slider3);
			Add(lblMaxValue);
			slider.CanGrow = canGrow;
			slider2.CanGrow = canGrow;
			slider3.CanGrow = canGrow;
			underBarRect = guiManager.GUISpriteSheet.GetSourceRectangle("lcd_barempty_green");
			underBar.SetSkinLocation(SkinState.Normal, underBarRect);
			underBar.EdgeSize = defaultEdgeSize;
			underBar.Height = underBarRect.Height;
			valueBarRect = guiManager.GUISpriteSheet.GetSourceRectangle("lcd_barfill_green");
			valueBar.SetSkinLocation(SkinState.Normal, valueBarRect);
			valueBar.EdgeSize = 0;
			valueBar.Height = valueBarRect.Height - 1;
			lblMaxValue.Init(Label.LabelType.LCDNormal);
			lblMaxValue.Text = "";
			lblMaxValue.CanHaveFocus = false;
			break;
		}
		Value = 0;
		MaxValue = 0;
	}

	private void UpdateNotches()
	{
		if (showNotches && maxValue > 1)
		{
			float? num = null;
			num = (float)StepSize / (float)maxValue * (float)BarWidth;
			int num2 = 0;
			int num3 = ((num > 3f) ? (maxValue / StepSize - 1) : 0);
			num2 = ((notchesInUse == null) ? num3 : (num3 - notchesInUse.Count));
			if (num2 > 0)
			{
				int num4 = 0;
				if (notchesPool != null)
				{
					for (int num5 = Math.Min(num2, notchesPool.Count) - 1; num5 >= 0; num5--)
					{
						Icon icon = notchesPool[num5];
						Insert(icon, valueBarIndex);
						Util.AddToList(ref notchesInUse, icon);
						notchesPool.RemoveAt(num5);
						num4++;
					}
				}
				for (int i = 0; i < num2 - num4; i++)
				{
					Icon icon2 = new Icon(guiManager);
					icon2.SetSkinLocation(SkinState.Normal, guiManager.GUISpriteSheet.GetSourceRectangle("lcd_notch"));
					icon2.ResizeControlToFitImage();
					icon2.Y = underBar.Y;
					Insert(icon2, valueBarIndex);
					Util.AddToList(ref notchesInUse, icon2);
				}
			}
			else if (num2 < 0 && notchesInUse != null)
			{
				for (int num6 = -num2 - 1; num6 >= 0; num6--)
				{
					RemoveNotch(num6);
				}
			}
			if (notchesInUse != null && num.HasValue)
			{
				for (int j = 0; j < notchesInUse.Count; j++)
				{
					notchesInUse[j].X = (int)Math.Round((float)underBar.X + ((float)j + 1f) * num.Value);
				}
			}
		}
		else if (notchesInUse != null)
		{
			for (int num7 = notchesInUse.Count - 1; num7 >= 0; num7--)
			{
				RemoveNotch(num7);
			}
		}
	}

	private void RemoveNotch(int i)
	{
		Icon icon = notchesInUse[i];
		Remove(icon);
		Util.AddToList(ref notchesPool, icon);
		notchesInUse.RemoveAt(i);
	}

	private void InitBarWithSlider(GUIManager guiManager, Label.LabelType labelType, FillableBarSlider.SliderType sliderKnobType, bool canGrow, string emptyBarSprite, string filledBarSprite, int underbarYPos, FillableBarSlider.ShowValueLabelModes showLabel, bool canMoveSlider, bool includeButtons, float? sliderButtonDelay, float? timeBetweenSliderButtonIncrements)
	{
		underBar = new Bar(guiManager);
		valueBar = new Bar(guiManager);
		lblMaxValue = new Label(guiManager);
		lblMaxValue.ZOrder = 1f;
		slider = new FillableBarSlider(guiManager, sliderKnobType, labelType, showLabel);
		slider.CanGrow = canGrow;
		slider.ShowValueLabel = showLabel;
		slider.DebugTag = "slider";
		if (canMoveSlider)
		{
			slider.SliderMouseUp += slider1_SliderMouseUp;
			slider.SliderMouseDown += slider1_SliderMouseDown;
			underBar.CanHaveFocus = true;
			underBar.MouseDown += underBar_MouseDown;
		}
		if (includeButtons)
		{
			tbDecrease = new TextButton(guiManager);
			tbIncrease = new TextButton(guiManager);
			InitButton(tbDecrease);
			InitButton(tbIncrease);
			tbDecrease.Text = "-";
			tbIncrease.Text = "+";
			tbDecrease.MouseDown += tbDecrease_MouseDown;
			tbIncrease.MouseDown += tbIncrease_MouseDown;
			tbDecrease.MouseUp += tbDecrease_MouseUp;
			tbIncrease.MouseUp += tbIncrease_MouseUp;
			tbDecrease.LoseFocus += tbDecrease_LoseFocus;
			tbIncrease.LoseFocus += tbIncrease_LoseFocus;
			this.sliderButtonDelay = sliderButtonDelay.Value;
			this.timeBetweenSliderButtonIncrements = timeBetweenSliderButtonIncrements.Value;
		}
		Add(underBar);
		valueBarIndex = Add(valueBar);
		Add(slider);
		Add(lblMaxValue);
		underBarRect = guiManager.GUISpriteSheet.GetSourceRectangle(emptyBarSprite);
		underBar.SetSkinLocation(SkinState.Normal, underBarRect);
		underBar.EdgeSize = defaultEdgeSize;
		underBar.Height = underBarRect.Height;
		valueBarRect = guiManager.GUISpriteSheet.GetSourceRectangle(filledBarSprite);
		valueBar.SetSkinLocation(SkinState.Normal, valueBarRect);
		valueBar.EdgeSize = 0;
		valueBar.Height = underBar.Height;
		valueBar.SetSkinLocation(SkinState.Disabled, valueBarRect, Color.Gray, Color.Gray);
		underBar.Y = underbarYPos;
		valueBar.Y = underBar.Y;
		lblMaxValue.Init(labelType);
		lblMaxValue.Text = "";
		lblMaxValue.CanHaveFocus = false;
		if (type == FillableBarType.HUDSlider)
		{
			lblMaxValue.Y = 0;
		}
		else
		{
			lblMaxValue.Y = -1;
		}
		base.CanReceiveMouseWheelEvents = false;
		underBar.X = GetBarLeftPos();
		valueBar.X = underBar.X;
		Height = underBarRect.Height + 2 * underbarYPos;
	}

	private void tbIncrease_LoseFocus()
	{
		OnLoseFocusReset();
	}

	private void tbDecrease_LoseFocus()
	{
		OnLoseFocusReset();
	}

	private void tbIncrease_MouseUp(MouseEventArgs args)
	{
		if (this.SliderMouseUp != null)
		{
			this.SliderMouseUp(this, EventArgs);
		}
		isIncreasing = false;
	}

	private void tbDecrease_MouseUp(MouseEventArgs args)
	{
		if (this.SliderMouseUp != null)
		{
			this.SliderMouseUp(this, EventArgs);
		}
		isDecreasing = false;
	}

	private void tbIncrease_MouseDown(MouseEventArgs args)
	{
		if (this.SliderMouseDown != null)
		{
			this.SliderMouseDown(this, EventArgs);
		}
		isIncreasing = true;
		buttonDownStarted = 0f;
		Increase();
	}

	private void tbDecrease_MouseDown(MouseEventArgs args)
	{
		if (this.SliderMouseDown != null)
		{
			this.SliderMouseDown(this, EventArgs);
		}
		isDecreasing = true;
		buttonDownStarted = 0f;
		Decrease();
	}

	private void Increase()
	{
		int sliderValue = slider.Value + StepSize;
		SetValues(sliderValue, sliderValue, MaxValue);
		UpdateSliderPosition();
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);
		if (!isDecreasing && !isIncreasing)
		{
			return;
		}
		buttonDownStarted += (float)gameTime.ElapsedGameTime.TotalSeconds;
		if (!(buttonDownStarted >= sliderButtonDelay))
		{
			return;
		}
		float num = (buttonDownStarted - sliderButtonDelay) / timeBetweenSliderButtonIncrements;
		if (num > 1f)
		{
			buttonDownStarted = sliderButtonDelay + num;
			if (isDecreasing)
			{
				Decrease();
			}
			else if (isIncreasing)
			{
				Increase();
			}
		}
	}

	private void Decrease()
	{
		int sliderValue = slider.Value - StepSize;
		SetValues(sliderValue, sliderValue, MaxValue);
		UpdateSliderPosition();
	}

	private void InitButton(TextButton tb)
	{
		Add(tb);
		if (type == FillableBarType.LCDSlider)
		{
			tb.Init(TextButton.TextButtonType.LCDSliderButton);
			tb.Y = -2;
		}
		else if (type == FillableBarType.LCDSliderWhite)
		{
			tb.Init(TextButton.TextButtonType.LCDSliderButtonWhite);
			tb.Y = -2;
		}
		else if (type == FillableBarType.HUDSlider)
		{
			tb.Init(TextButton.TextButtonType.HUDSliderButton);
		}
		else if (type == FillableBarType.HUDSliderWhite)
		{
			tb.Init(TextButton.TextButtonType.HUDSliderButtonWhite);
			tb.DebugTag = "HUDSliderButton";
		}
		tb.Width = 16;
	}

	private void slider1_SliderMouseUp(object sender, EventArgs e)
	{
		if (this.SliderMouseUp != null)
		{
			this.SliderMouseUp(this, EventArgs);
		}
		UpdateValueBarWidth();
	}

	private void slider1_SliderMouseDown(object sender, EventArgs e)
	{
		if (this.SliderMouseDown != null)
		{
			this.SliderMouseDown(this, EventArgs);
		}
	}

	public static int RoundToIncrements(int value, int roundTo)
	{
		int num = value % roundTo;
		if (num >= roundTo - num)
		{
			return value + (roundTo - num);
		}
		return value - num;
	}

	private void underBar_MouseDown(MouseEventArgs args)
	{
		if (args.Button == MouseButtons.Left)
		{
			int positionOnSlider = args.Position.X - underBar.AbsolutePosition.X;
			int valueFromPosition = slider.GetValueFromPosition(positionOnSlider, roundToNearestStep: true);
			SetValues(valueFromPosition, valueFromPosition, MaxValue);
			UpdateSliderPosition();
			if (this.SliderMouseDown != null)
			{
				this.SliderMouseDown(this, EventArgs);
			}
			if (this.SliderMouseUp != null)
			{
				this.SliderMouseUp(this, EventArgs);
			}
		}
	}

	private void EnableHover()
	{
		valueBar.SetSkinLocation(SkinState.Hover, valueBarRect, Color.DarkGray, Color.DarkGray);
		underBar.SetSkinLocation(SkinState.Hover, underBarRect, Color.DarkGray, Color.DarkGray);
		valueBar.SetSkinLocation(SkinState.HoverDisabled, valueBarRect, Color.DarkGray, Color.DarkGray);
		underBar.SetSkinLocation(SkinState.HoverDisabled, underBarRect, Color.DarkGray, Color.DarkGray);
		hoverEnabled = true;
	}

	public string GetDisplayValue(int value)
	{
		if (DisplayValueFunction != null)
		{
			return DisplayValueFunction(value);
		}
		return value.ToString();
	}

	protected override void OnMouseOver(UIComponent sender, MouseEventArgs args)
	{
		base.OnMouseOver(sender, args);
		if (hoverEnabled)
		{
			if (Enabled)
			{
				valueBar.CurrentSkinState = SkinState.Hover;
				underBar.CurrentSkinState = SkinState.Hover;
			}
			else
			{
				valueBar.CurrentSkinState = SkinState.HoverDisabled;
				underBar.CurrentSkinState = SkinState.HoverDisabled;
			}
		}
	}

	protected override void OnMouseOut(UIComponent sender, MouseEventArgs args)
	{
		base.OnMouseOut(sender, args);
		if (hoverEnabled)
		{
			if (Enabled)
			{
				valueBar.CurrentSkinState = SkinState.Normal;
				underBar.CurrentSkinState = SkinState.Normal;
			}
			else
			{
				valueBar.CurrentSkinState = SkinState.Disabled;
				underBar.CurrentSkinState = SkinState.Disabled;
			}
		}
	}

	private void OnLoseFocusReset()
	{
		isIncreasing = false;
		isDecreasing = false;
	}

	protected override void OnLoseFocus()
	{
		base.OnLoseFocus();
		OnLoseFocusReset();
	}

	private void SetValues(int barValue, int sliderValue, int maxValue)
	{
		switch (type)
		{
		case FillableBarType.Default:
			this.barValue = barValue;
			if (barValue > maxValue)
			{
				this.barValue = maxValue;
			}
			this.maxValue = maxValue;
			Text = Value + "/" + MaxValue;
			break;
		case FillableBarType.ProgressBar:
			this.barValue = barValue;
			if (barValue > maxValue)
			{
				this.barValue = maxValue;
				Text = this.barValue + "%";
			}
			this.maxValue = maxValue;
			break;
		case FillableBarType.StockOrders:
			if (slider.Value >= slider2.Value && slider.Value >= slider3.Value)
			{
				this.maxValue = slider.Value;
			}
			else if (slider2.Value >= slider.Value && slider2.Value >= slider3.Value)
			{
				this.maxValue = slider2.Value;
			}
			else if (slider3.Value >= slider.Value && slider3.Value >= slider2.Value)
			{
				this.maxValue = slider3.Value;
			}
			this.barValue = slider3.Value;
			Text = string.Concat(barValue);
			break;
		case FillableBarType.HUDSlider:
		case FillableBarType.HUDSliderWhite:
		case FillableBarType.LCDSlider:
		case FillableBarType.LCDSliderWhite:
			barValue = Util.Clamp(barValue, 0, maxValue);
			this.barValue = barValue;
			this.maxValue = maxValue;
			slider.Value = barValue;
			Text = GetDisplayValue(MaxValue);
			break;
		case FillableBarType.LCDIndicator:
			barValue = Util.Clamp(barValue, 0, maxValue);
			sliderValue = Util.Clamp(sliderValue, 0, maxValue);
			this.barValue = barValue;
			this.maxValue = maxValue;
			slider.Value = sliderValue;
			Text = MaxValue.ToString();
			break;
		}
		UpdateSizesAndPositionsWithNewWidth();
	}

	public void UpdateSliderPosition()
	{
		slider.UpdateSliderPositionAndSize(null);
		UpdateValueBarWidth();
	}

	public void SetTags()
	{
		underBar.DebugTag = "underBar";
		valueBar.DebugTag = "valueBar";
	}

	protected override void OnMouseWheelChanged(int wheelChange)
	{
		base.OnMouseWheelChanged(wheelChange);
	}

	private int GetDistanceToMaxLabel()
	{
		if (tbIncrease != null)
		{
			return 24 + tbIncrease.Width;
		}
		return 24;
	}

	private int GetBarLeftPos()
	{
		if (tbDecrease != null)
		{
			return barLeftMargin + tbDecrease.Width;
		}
		return barLeftMargin;
	}

	protected override void OnResize(UIComponent sender)
	{
		base.OnResize(sender);
		UpdateSizesAndPositionsWithNewWidth();
	}

	private void UpdateSizesAndPositionsWithNewWidth()
	{
		if (type == FillableBarType.StockOrders)
		{
			int num = 0;
			if (slider.X >= slider2.X && slider.X >= slider3.X)
			{
				num = slider.X;
			}
			else if (slider2.X >= slider.X && slider2.X >= slider3.X)
			{
				num = slider2.X;
			}
			else if (slider3.X >= slider.X && slider3.X >= slider2.X)
			{
				num = slider3.X;
			}
			num += 30;
			if (Width != num && num > base.MinWidth && num <= 230)
			{
				Width = num;
			}
			underBar.Position = new Point(0, 20);
		}
		int num2 = Width;
		if (ShowMaxValueLabelAtEnd)
		{
			num2 -= 24;
		}
		else if (slider != null)
		{
			num2 -= GetBarLeftPos();
		}
		underBar.Width = num2;
		if (valueBar != null)
		{
			UpdateValueBarWidth();
		}
		if (type == FillableBarType.ProgressBar)
		{
			valueBar.Height = Height;
			underBar.Height = Height;
		}
		if (type == FillableBarType.StockOrders)
		{
			lblMaxValue.Position = new Point(valueBar.Position.X + valueBar.Width - lblMaxValue.TextWidth / 2, underBar.Y + (underBar.Height / 2 - lblMaxValue.TextHeight / 2));
			if (lblMaxValue.X < 10 || maxValue == 0)
			{
				lblMaxValue.Position = new Point(10, underBar.Y + (underBar.Height / 2 - lblMaxValue.TextHeight / 2));
			}
		}
		else if (tbIncrease != null)
		{
			tbIncrease.X = underBar.Right + barRightMargin;
			lblMaxValue.X = tbIncrease.Right;
		}
		else
		{
			lblMaxValue.X = underBar.Right + barRightMargin;
		}
	}

	private void UpdateValueBarWidth()
	{
		valueBar.Position = underBar.Position;
		int num = ((maxValue == 0) ? underBar.Width : ((slider == null || !ValueBarAndSliderAreLocked) ? ((int)((float)barValue / (float)maxValue * (float)underBar.Width)) : (slider.X - valueBar.X + 4)));
		valueBar.Width = num;
		if (num <= 1)
		{
			valueBar.Visible = false;
		}
		else
		{
			valueBar.Visible = true;
		}
	}
}
