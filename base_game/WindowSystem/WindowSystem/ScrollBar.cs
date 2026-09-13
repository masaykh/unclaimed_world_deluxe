using System;
using InputEventSystem;
using Microsoft.Xna.Framework;

namespace WindowSystem;

public class ScrollBar : Bar
{
	public enum ScrollBarType
	{
		Default,
		CommRoller,
		MainRoller,
		HUD,
		LCD
	}

	private static int defaultButtonSize = 17;

	private static Rectangle defaultBackgroundSkin = new Rectangle(66, 5, 17, 17);

	private static Rectangle defaultTopButtonSkin = new Rectangle(84, 23, 17, 17);

	private static Rectangle defaultTopButtonHoverSkin = new Rectangle(102, 23, 17, 17);

	private static Rectangle defaultTopButtonPressedSkin = new Rectangle(120, 23, 17, 17);

	private static Rectangle defaultBottomButtonSkin = new Rectangle(84, 5, 17, 17);

	private static Rectangle defaultBottomButtonHoverSkin = new Rectangle(102, 5, 17, 17);

	private static Rectangle defaultBottomButtonPressedSkin = new Rectangle(120, 5, 17, 17);

	private static Rectangle defaultThumbSkin = new Rectangle(66, 23, 17, 10);

	private static Rectangle defaultThumbHoverSkin = new Rectangle(66, 34, 17, 10);

	private static Rectangle defaultThumbPressedSkin = new Rectangle(66, 45, 17, 10);

	public ScrollBarType ScrollType;

	private const int RepeatDelay = 500;

	private const int RepeatRate = 50;

	private ImageButton topButton;

	private ImageButton bottomButton;

	private Bar thumb;

	private int value;

	private int viewable;

	private int maximumValue;

	private int scrollStep;

	private int countdown;

	private bool firstRepeat;

	private bool isTopPressed;

	private bool isBottomPressed;

	private bool isTopOver;

	private bool isBottomOver;

	private bool draggingThumb;

	private int dragPoint;

	private Point lastLocation;

	public static int DefaultButtonSize
	{
		set
		{
			defaultButtonSize = value;
		}
	}

	public static Rectangle DefaultBackgroundSkin
	{
		set
		{
			defaultBackgroundSkin = value;
		}
	}

	public static Rectangle DefaultTopButtonSkin
	{
		set
		{
			defaultTopButtonSkin = value;
		}
	}

	public static Rectangle DefaultTopButtonHoverSkin
	{
		set
		{
			defaultTopButtonHoverSkin = value;
		}
	}

	public static Rectangle DefaultTopButtonPressedSkin
	{
		set
		{
			defaultTopButtonPressedSkin = value;
		}
	}

	public static Rectangle DefaultBottomButtonSkin
	{
		set
		{
			defaultBottomButtonSkin = value;
		}
	}

	public static Rectangle DefaultBottomButtonHoverSkin
	{
		set
		{
			defaultBottomButtonHoverSkin = value;
		}
	}

	public static Rectangle DefaultBottomButtonPressedSkin
	{
		set
		{
			defaultBottomButtonPressedSkin = value;
		}
	}

	public static Rectangle DefaultThumbSkin
	{
		set
		{
			defaultThumbSkin = value;
		}
	}

	public static Rectangle DefaultThumbHoverSkin
	{
		set
		{
			defaultThumbHoverSkin = value;
		}
	}

	public static Rectangle DefaultThumbPressedSkin
	{
		set
		{
			defaultThumbPressedSkin = value;
		}
	}

	public int ButtonSize
	{
		get
		{
			return topButton.Width;
		}
		set
		{
			base.MinWidth = value;
			MinHeight = 2 * value;
			thumb.Y = value;
			Width = value;
		}
	}

	public int Viewable
	{
		get
		{
			return viewable;
		}
		set
		{
			viewable = value;
			CalculateThumbSize();
			if (viewable > maximumValue)
			{
				ScrollTo(0);
			}
		}
	}

	public override int Height
	{
		get
		{
			return base.Height;
		}
		set
		{
			base.Height = value;
		}
	}

	public int ScrollStep
	{
		get
		{
			return scrollStep;
		}
		set
		{
			scrollStep = value;
		}
	}

	public int Value
	{
		get
		{
			return value;
		}
		set
		{
			ScrollTo(value);
		}
	}

	public int MaximumValue
	{
		get
		{
			return maximumValue;
		}
		set
		{
			maximumValue = value;
			CalculateThumbSize();
			if (viewable > maximumValue)
			{
				ScrollTo(0);
			}
		}
	}

	public Rectangle BackgroundSkin
	{
		set
		{
			SetSkinLocation(0, value);
		}
	}

	public Rectangle TopButtonSkin
	{
		set
		{
			topButton.SetSkinLocation(SkinState.Normal, value);
		}
	}

	public Rectangle TopButtonHoverSkin
	{
		set
		{
			topButton.SetSkinLocation(1, value);
		}
	}

	public Rectangle TopButtonPressedSkin
	{
		set
		{
			topButton.SetSkinLocation(2, value);
		}
	}

	public Rectangle BottomButtonSkin
	{
		set
		{
			bottomButton.SetSkinLocation(SkinState.Normal, value);
		}
	}

	public Rectangle BottomButtonHoverSkin
	{
		set
		{
			bottomButton.SetSkinLocation(1, value);
		}
	}

	public Rectangle BottomButtonPressedSkin
	{
		set
		{
			bottomButton.SetSkinLocation(2, value);
		}
	}

	public Rectangle ThumbSkin
	{
		set
		{
			thumb.SetSkinLocation(SkinState.Normal, value);
		}
	}

	public Rectangle ThumbHoverSkin
	{
		set
		{
			thumb.SetSkinLocation(1, value);
		}
	}

	public Rectangle ThumbPressedSkin
	{
		set
		{
			thumb.SetSkinLocation(2, value);
		}
	}

	private float Range => maximumValue - viewable;

	private float ShaftHeight
	{
		get
		{
			if (ScrollType == ScrollBarType.Default || ScrollType == ScrollBarType.HUD || ScrollType == ScrollBarType.LCD)
			{
				return Height - 2 * topButton.Height;
			}
			return Height;
		}
	}

	public override RenderType RenderType
	{
		get
		{
			return base.RenderType;
		}
		set
		{
			base.RenderType = value;
		}
	}

	public bool ShowKnob
	{
		set
		{
			if (value)
			{
				Add(thumb);
			}
			else
			{
				Remove(thumb);
			}
		}
	}

	private float Gap => ShaftHeight - (float)thumb.Height;

	private float Ratio => Range / Gap;

	public event ScrollHandler Scroll;

	public ScrollBar(GUIManager guiManager)
		: this(guiManager, ScrollBarType.Default)
	{
	}

	public ScrollBar(GUIManager guiManager, ScrollBarType type)
		: base(guiManager)
	{
		ScrollType = type;
		value = 0;
		viewable = 1;
		maximumValue = 1;
		scrollStep = 1;
		countdown = 0;
		firstRepeat = true;
		isTopPressed = false;
		isBottomPressed = false;
		isTopOver = false;
		isBottomOver = false;
		draggingThumb = false;
		lastLocation = Point.Zero;
		switch (type)
		{
		case ScrollBarType.CommRoller:
		{
			thumb = new Bar(guiManager);
			Add(thumb);
			MinHeight = 1;
			Rectangle rectangle = (BackgroundSkin = guiManager.GUISpriteSheet.GetSourceRectangle("event_scroller_groove"));
			base.MinWidth = rectangle.Width;
			Width = rectangle.Width;
			base.EdgeSize = 10;
			thumb.UnderSprite = guiManager.GUISpriteSheet.GetSourceRectangle("event_scroller_knob_base");
			thumb.UnderSpriteY = 7;
			rectangle = (ThumbPressedSkin = (ThumbHoverSkin = (ThumbSkin = guiManager.GUISpriteSheet.GetSourceRectangle("event_scroller_knobwithshading"))));
			thumb.EdgeSize = 12;
			thumb.Width = rectangle.Width;
			RenderType = RenderType.CRTAndLCD;
			break;
		}
		case ScrollBarType.MainRoller:
		{
			thumb = new Bar(guiManager);
			Add(thumb);
			MinHeight = 1;
			Rectangle rectangle = (BackgroundSkin = guiManager.GUISpriteSheet.GetSourceRectangle("basic_scroller_groove"));
			base.MinWidth = rectangle.Width;
			Width = rectangle.Width;
			base.EdgeSize = 11;
			thumb.UnderSprite = guiManager.GUISpriteSheet.GetSourceRectangle("basic_scroller_knob_base");
			thumb.UnderSpriteY = 5;
			rectangle = (ThumbPressedSkin = (ThumbHoverSkin = (ThumbSkin = guiManager.GUISpriteSheet.GetSourceRectangle("basic_scroller_knobwithshading"))));
			thumb.EdgeSize = 13;
			thumb.Width = rectangle.Width;
			thumb.DebugTag = "scrollerKnob";
			RenderType = RenderType.Normal;
			break;
		}
		case ScrollBarType.Default:
		{
			topButton = new ImageButton(guiManager);
			bottomButton = new ImageButton(guiManager);
			thumb = new Bar(guiManager);
			Add(thumb);
			Add(topButton);
			Add(bottomButton);
			base.MinWidth = defaultButtonSize;
			MinHeight = 2 * defaultButtonSize;
			base.IsVertical = true;
			thumb.CanHaveFocus = true;
			thumb.IsVertical = true;
			thumb.Y = defaultButtonSize;
			Width = defaultButtonSize;
			Rectangle rectangle = defaultTopButtonSkin;
			BackgroundSkin = defaultBackgroundSkin;
			TopButtonSkin = rectangle;
			TopButtonHoverSkin = rectangle;
			TopButtonPressedSkin = rectangle;
			BottomButtonSkin = rectangle;
			BottomButtonHoverSkin = rectangle;
			BottomButtonPressedSkin = rectangle;
			ThumbSkin = rectangle;
			ThumbHoverSkin = rectangle;
			ThumbPressedSkin = rectangle;
			break;
		}
		case ScrollBarType.HUD:
		{
			int num = 12;
			topButton = new ImageButton(guiManager);
			bottomButton = new ImageButton(guiManager);
			thumb = new Bar(guiManager);
			Add(thumb);
			Add(topButton);
			Add(bottomButton);
			base.MinWidth = num;
			MinHeight = 2 * num;
			base.IsVertical = true;
			thumb.CanHaveFocus = true;
			thumb.IsVertical = true;
			thumb.Y = num;
			base.CanHaveFocus = true;
			Width = num;
			base.EdgeSize = 4;
			BackgroundSkin = guiManager.GUISpriteSheet.GetSourceRectangle("HUD_scrollbar_base");
			topButton.InitButton("HUD_uparrow", UIComponent.hudHoverTint, UIComponent.hudPressedTint);
			bottomButton.InitButton("HUD_downarrow", UIComponent.hudHoverTint, UIComponent.hudPressedTint);
			thumb.SetSkinLocation(SkinState.Normal, guiManager.GUISpriteSheet.GetSourceRectangle("HUD_scrollbar_knob"));
			thumb.SetSkinLocation(SkinState.Hover, guiManager.GUISpriteSheet.GetSourceRectangle("HUD_scrollbar_knob"), UIComponent.hudHoverTint, UIComponent.hudHoverTint);
			thumb.SetSkinLocation(SkinState.Pressed, guiManager.GUISpriteSheet.GetSourceRectangle("HUD_scrollbar_knob"), UIComponent.hudPressedTint, UIComponent.hudPressedTint);
			break;
		}
		case ScrollBarType.LCD:
		{
			int num = 15;
			topButton = new ImageButton(guiManager);
			bottomButton = new ImageButton(guiManager);
			thumb = new Bar(guiManager);
			Add(thumb);
			Add(topButton);
			Add(bottomButton);
			base.MinWidth = num;
			MinHeight = 2 * num;
			base.IsVertical = true;
			thumb.CanHaveFocus = true;
			thumb.IsVertical = true;
			thumb.Y = num;
			base.CanHaveFocus = true;
			Width = num;
			base.EdgeSize = 10;
			BackgroundSkin = guiManager.GUISpriteSheet.GetSourceRectangle("basic_scrollbar_groove");
			topButton.InitButton("basic_scrollbar_arrow_up", UIComponent.lcdHoverTint, UIComponent.lcdPressedTint);
			bottomButton.InitButton("basic_scrollbar_arrow_down", UIComponent.lcdHoverTint, UIComponent.lcdPressedTint);
			thumb.SetSkinLocation(SkinState.Normal, guiManager.GUISpriteSheet.GetSourceRectangle("basic_scrollbar_knob"));
			thumb.SetSkinLocation(SkinState.Hover, guiManager.GUISpriteSheet.GetSourceRectangle("basic_scrollbar_knob"), UIComponent.lcdHoverTint, UIComponent.lcdHoverTint);
			thumb.SetSkinLocation(SkinState.Pressed, guiManager.GUISpriteSheet.GetSourceRectangle("basic_scrollbar_knob"), UIComponent.lcdPressedTint, UIComponent.lcdPressedTint);
			thumb.EdgeSize = 4;
			RenderType = RenderType.CRTAndLCD;
			break;
		}
		}
		base.IsVertical = true;
		thumb.CanHaveFocus = true;
		thumb.IsVertical = true;
		thumb.Y = defaultButtonSize;
		thumb.ZOrder = 1f;
		if (topButton != null)
		{
			topButton.MouseDown += OnTopButtonDown;
			topButton.MouseUp += OnButtonUp;
			topButton.MouseOut += OnTopButtonOut;
			topButton.MouseOver += OnTopButtonOver;
		}
		if (bottomButton != null)
		{
			bottomButton.MouseDown += OnBottomButtonDown;
			bottomButton.MouseUp += OnButtonUp;
			bottomButton.MouseOver += OnBottomButtonOver;
			bottomButton.MouseOut += OnBottomButtonOut;
		}
		thumb.MouseOver += OnThumbMouseOver;
		thumb.MouseOut += OnThumbMouseOut;
		thumb.MouseDown += OnThumbDown;
		thumb.MouseUp += OnThumbUp;
		thumb.MouseMove += OnThumbMove;
	}

	public override void Update(GameTime gameTime)
	{
		if (countdown <= 0)
		{
			if (isTopPressed && isTopOver)
			{
				dragPoint = thumb.Height / 2;
				ScrollTo(value - scrollStep);
			}
			else if (isBottomPressed && isBottomOver)
			{
				dragPoint = thumb.Height / 2;
				ScrollTo(value + scrollStep);
			}
			if (firstRepeat)
			{
				countdown += 500;
				firstRepeat = false;
			}
			else
			{
				countdown = 50;
			}
		}
		if ((isTopPressed && isTopOver) || (isBottomPressed && isBottomOver))
		{
			countdown -= gameTime.ElapsedGameTime.Milliseconds;
		}
		else
		{
			countdown = 0;
		}
		base.Update(gameTime);
	}

	public bool IsAtEnd()
	{
		return (float)value == Range;
	}

	public bool IsAtTop()
	{
		return value == 0;
	}

	private void CalculateThumbSize()
	{
		int height = thumb.Height;
		try
		{
			float num = ShaftHeight / ((float)maximumValue / (float)viewable);
			thumb.Height = Convert.ToInt32(num);
		}
		catch
		{
			thumb.Height = (int)ShaftHeight;
		}
		if (thumb.Height < 10)
		{
			thumb.Height = 10;
		}
		else if ((float)thumb.Height > ShaftHeight)
		{
			thumb.Height = (int)ShaftHeight;
		}
		if (thumb.Height != height)
		{
			Redraw();
		}
	}

	private void ScrollTo(int position)
	{
		if ((float)position > Range)
		{
			position = (int)Range;
		}
		if (position < 0)
		{
			position = 0;
		}
		value = position;
		if (ScrollType == ScrollBarType.Default || ScrollType == ScrollBarType.HUD || ScrollType == ScrollBarType.LCD)
		{
			float gap = Gap;
			if (Range > 0f)
			{
				float num = (float)value / Range;
				float num2 = (float)topButton.Height + gap * num;
				if (num2 < (float)topButton.Height)
				{
					num2 = topButton.Height;
				}
				else
				{
					float num3 = Gap + (float)topButton.Height;
					if (num2 > num3)
					{
						num2 = num3;
					}
				}
				thumb.Y = Convert.ToInt32(num2);
			}
			else
			{
				thumb.Y = topButton.Height;
			}
		}
		if (this.Scroll != null)
		{
			this.Scroll(value);
		}
		Redraw();
	}

	private int MouseToScrollPosition(int mousePosition)
	{
		float num = mousePosition;
		if (Gap > 0f)
		{
			return Convert.ToInt32(Ratio * num);
		}
		return 0;
	}

	protected void OnThumbMouseOver(UIComponent sender, MouseEventArgs args)
	{
		if (!draggingThumb)
		{
			thumb.CurrentSkinState = SkinState.Hover;
		}
	}

	protected void OnThumbMouseOut(UIComponent sender, MouseEventArgs args)
	{
		if (!draggingThumb)
		{
			thumb.CurrentSkinState = SkinState.Normal;
		}
	}

	protected void OnThumbDown(MouseEventArgs args)
	{
		if (args.Button == MouseButtons.Left)
		{
			draggingThumb = true;
			lastLocation = args.Position;
			dragPoint = args.Position.Y - thumb.AbsolutePosition.Y;
			thumb.CurrentSkinState = SkinState.Pressed;
		}
	}

	protected void OnThumbUp(MouseEventArgs args)
	{
		if (args.Button == MouseButtons.Left)
		{
			draggingThumb = false;
			if (thumb.CheckCoordinates(args.Position.X, args.Position.Y))
			{
				thumb.CurrentSkinState = SkinState.Hover;
			}
			else
			{
				thumb.CurrentSkinState = SkinState.Normal;
			}
			base.GUIManager.SetFocus(this);
		}
	}

	protected void OnThumbMove(MouseEventArgs args)
	{
		if (draggingThumb)
		{
			int num = args.Position.Y - base.AbsolutePosition.Y - topButton.Height;
			num -= dragPoint;
			if (ScrollType == ScrollBarType.Default || ScrollType == ScrollBarType.HUD || ScrollType == ScrollBarType.LCD)
			{
				ScrollTo(MouseToScrollPosition(num));
			}
		}
	}

	protected override void OnMouseDown(MouseEventArgs args)
	{
		base.OnMouseDown(args);
		if (args.Button == MouseButtons.Left)
		{
			dragPoint = thumb.Height / 2;
			int num = args.Position.Y - base.AbsolutePosition.Y - topButton.Height;
			num -= dragPoint;
			if (ScrollType == ScrollBarType.Default || ScrollType == ScrollBarType.HUD || ScrollType == ScrollBarType.LCD)
			{
				ScrollTo(MouseToScrollPosition(num));
			}
		}
	}

	protected void OnTopButtonDown(MouseEventArgs args)
	{
		if (args.Button == MouseButtons.Left)
		{
			isTopPressed = true;
			isTopOver = true;
			firstRepeat = true;
		}
	}

	protected void OnBottomButtonDown(MouseEventArgs args)
	{
		if (args.Button == MouseButtons.Left)
		{
			isBottomPressed = true;
			isBottomOver = true;
			firstRepeat = true;
		}
	}

	protected void OnButtonUp(MouseEventArgs args)
	{
		if (args.Button == MouseButtons.Left)
		{
			isTopPressed = false;
			isBottomPressed = false;
			isTopOver = false;
			isBottomOver = false;
			base.GUIManager.SetFocus(this);
		}
	}

	protected void OnTopButtonOver(UIComponent sender, MouseEventArgs args)
	{
		isTopOver = true;
	}

	protected void OnBottomButtonOver(UIComponent sender, MouseEventArgs args)
	{
		isBottomOver = true;
	}

	protected void OnTopButtonOut(UIComponent sender, MouseEventArgs args)
	{
		isTopOver = false;
	}

	protected void OnBottomButtonOut(UIComponent sender, MouseEventArgs args)
	{
		isBottomOver = false;
	}

	protected override void OnResize(UIComponent sender)
	{
		base.OnResize(sender);
		if (topButton != null)
		{
			topButton.Width = Width;
			topButton.Height = Width;
		}
		if (bottomButton != null)
		{
			bottomButton.Width = Width;
			bottomButton.Height = Width;
			bottomButton.Y = Height - bottomButton.Height;
		}
		if (ScrollType == ScrollBarType.Default || ScrollType == ScrollBarType.HUD || ScrollType == ScrollBarType.LCD)
		{
			thumb.Width = Width;
		}
		CalculateThumbSize();
		ScrollTo(value);
	}
}
