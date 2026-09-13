using System;
using System.Collections.Generic;
using InputEventSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WindowSystem;

public class Window : UIComponent
{
	private enum Transition
	{
		In,
		Out,
		None
	}

	public delegate void DrawContentDelegate(Window sender, SpriteBatch spriteBatch);

	private static bool defaultHasCloseButton = true;

	private static bool defaultHasTitleBar = true;

	private static bool defaultFullWindowMovableArea = true;

	private static int defaultTitleBarHeight = 24;

	private static int defaultButtonSize = 20;

	private static int defaultMargin = 0;

	private static float defaultAnimationTransparency = 0.75f;

	private static string defaultTitleFont = "Content/Fonts/DefaultHeading";

	private static Rectangle defaultSkin = new Rectangle(15, 1, 15, 15);

	private static Rectangle defaultTitleBarSkin = new Rectangle(1, 1, 13, 25);

	private static Rectangle defaultCloseButtonSkin = new Rectangle(1, 168, 20, 20);

	private static Rectangle defaultCloseButtonHoverSkin = new Rectangle(22, 168, 20, 20);

	private static Rectangle defaultCloseButtonPressedSkin = new Rectangle(43, 168, 20, 20);

	private static float timeToTransitionIn = 0.2f;

	private static float timeToTransitionOut = 0.2f;

	private Level level = Level.Bottom;

	public bool HasCRTOrLCDComponents;

	public bool HasOverlayComponents;

	public bool IsBackgroundGraphics;

	private Box box;

	public UIComponent ViewPort;

	private MovableArea movableArea;

	private MovableArea backgroundMovableArea;

	private Label label;

	private ImageButton closeButton;

	private ResizableArea[] resizableAreas;

	private bool hasCloseButton;

	private bool fullWindowMovableArea;

	private int margin;

	private int resizableBorder;

	private bool isResizable;

	private float transparency;

	private Transition transitioning = Transition.None;

	public bool HideThisNow;

	private float transitionValue;

	public float Opacity = 1f;

	private float alpha = 1f;

	public new string ID;

	public new bool Visible;

	public new Level Level
	{
		get
		{
			return level;
		}
		set
		{
			if (level != value)
			{
				guiManager.Remove(this);
				level = value;
				guiManager.Add(this);
			}
			else
			{
				guiManager.Add(this);
			}
			foreach (UIComponent control in base.Controls)
			{
				control.Level = value;
			}
		}
	}

	[Skin]
	public static bool DefaultHasCloseButton
	{
		set
		{
			defaultHasCloseButton = value;
		}
	}

	[Skin]
	public static bool DefaultHasFullWindowMovableArea
	{
		set
		{
			defaultFullWindowMovableArea = value;
		}
	}

	[Skin]
	public static int DefaultTitleBarHeight
	{
		set
		{
			defaultTitleBarHeight = value;
		}
	}

	[Skin]
	public static int DefaultButtonSize
	{
		set
		{
			defaultButtonSize = value;
		}
	}

	[Skin]
	public static int DefaultMargin
	{
		set
		{
			defaultMargin = value;
		}
	}

	[Skin]
	public static string DefaultTitleFont
	{
		set
		{
			defaultTitleFont = value;
		}
	}

	[Skin]
	public static Rectangle DefaultSkin
	{
		set
		{
			defaultSkin = value;
		}
	}

	[Skin]
	public static Rectangle DefaultTitleBarSkin
	{
		set
		{
			defaultTitleBarSkin = value;
		}
	}

	[Skin]
	public static Rectangle DefaultCloseButtonSkin
	{
		set
		{
			defaultCloseButtonSkin = value;
		}
	}

	[Skin]
	public static Rectangle DefaultCloseButtonHoverSkin
	{
		set
		{
			defaultCloseButtonHoverSkin = value;
		}
	}

	[Skin]
	public static Rectangle DefaultCloseButtonPressedSkin
	{
		set
		{
			defaultCloseButtonPressedSkin = value;
		}
	}

	public float TransitionValue
	{
		get
		{
			return transitionValue;
		}
		set
		{
			transitionValue = value;
			if (this.Transitioning != null)
			{
				this.Transitioning(this);
			}
		}
	}

	public bool IsVisibleAndActive
	{
		get
		{
			if (transitioning == Transition.Out || !Visible)
			{
				return false;
			}
			return true;
		}
	}

	[Skin]
	public bool Resizable
	{
		get
		{
			return isResizable;
		}
		set
		{
			isResizable = value;
			if (isResizable)
			{
				for (int i = 0; i < 8; i++)
				{
					resizableAreas[i].CanHaveFocus = true;
				}
			}
			else
			{
				for (int j = 0; j < 8; j++)
				{
					resizableAreas[j].CanHaveFocus = false;
				}
			}
		}
	}

	public Vector2 WindowSize
	{
		get
		{
			return new Vector2(ClientWidth, ClientHeight);
		}
		set
		{
			ClientWidth = (int)value.X;
			ClientHeight = (int)value.Y;
		}
	}

	public int ClientWidth
	{
		get
		{
			return ViewPort.Width;
		}
		set
		{
			Width = value + margin * 2;
		}
	}

	public int ClientHeight
	{
		get
		{
			return ViewPort.Height;
		}
		set
		{
			Height = value + margin;
		}
	}

	[Skin]
	public bool HasCloseButton
	{
		get
		{
			return hasCloseButton;
		}
		set
		{
			if (hasCloseButton && !value)
			{
				base.Remove(closeButton);
			}
			else if (!hasCloseButton && value)
			{
				base.Add(closeButton);
			}
			hasCloseButton = value;
		}
	}

	[Skin]
	public bool IsMovable
	{
		get
		{
			return fullWindowMovableArea;
		}
		set
		{
			fullWindowMovableArea = value;
			if (fullWindowMovableArea)
			{
				ViewPort.Add(backgroundMovableArea);
				backgroundMovableArea.Parent = this;
			}
			else
			{
				ViewPort.Remove(backgroundMovableArea);
			}
		}
	}

	[Skin]
	public int ButtonSize
	{
		get
		{
			return closeButton.Width;
		}
		set
		{
			closeButton.Width = value;
			closeButton.Height = value;
			closeButton.X = Width - value - closeButton.Y;
		}
	}

	public int CornerSize
	{
		set
		{
			box.CornerSize = value;
		}
	}

	public int ResizableBorderSize
	{
		get
		{
			return resizableBorder;
		}
		set
		{
			resizableBorder = value;
			RefreshResizableAreas();
		}
	}

	public bool ShowPanel
	{
		set
		{
			if (!value)
			{
				box.DebugTag = "Removed";
				if (base.Controls.Remove(box))
				{
					box.CleanUp();
				}
			}
			else
			{
				Add(box);
			}
		}
	}

	public Image Background
	{
		set
		{
			base.Add(value);
		}
	}

	[Skin]
	public int Margin
	{
		get
		{
			return margin;
		}
		set
		{
			margin = value;
			label.X = value;
			ViewPort.X = value;
			ViewPort.Y = value;
			ViewPort.Width = Width - value * 2;
			ViewPort.Height = Height - value;
			int clientHeight = ClientHeight;
			ClientWidth = ClientWidth;
			ClientHeight = clientHeight;
		}
	}

	public string TitleText
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

	[Skin]
	public Rectangle Skin
	{
		set
		{
			box.SetSkinLocation(SkinState.Normal, value);
		}
	}

	[Skin]
	public Rectangle CloseButtonSkin
	{
		set
		{
			closeButton.SetSkinLocation(SkinState.Normal, value);
		}
	}

	[Skin]
	public Rectangle CloseButtonHoverSkin
	{
		set
		{
			closeButton.SetSkinLocation(1, value);
		}
	}

	[Skin]
	public Rectangle CloseButtonPressedSkin
	{
		set
		{
			closeButton.SetSkinLocation(2, value);
		}
	}

	public event TransitioningHandler Transitioning;

	public new event DrawContentDelegate DrawContentEvent;

	public event CloseHandler Close;

	public void SetSkin(Rectangle source, Color? edgeColor = null, Color? centerColor = null)
	{
		box.SetSkinLocation(SkinState.Normal, source, edgeColor, centerColor);
	}

	public void SetResizableArea(ResizeAreas area, bool isResizable)
	{
		ResizableArea[] array = resizableAreas;
		foreach (ResizableArea resizableArea in array)
		{
			if (resizableArea.ResizeArea == area)
			{
				resizableArea.CanHaveFocus = isResizable;
				break;
			}
		}
	}

	public Window(GUIManager guiManager)
		: base(guiManager)
	{
		isResizable = true;
		transparency = -1f;
		box = new Box(guiManager);
		box.DebugTag = "windowBox";
		ViewPort = new UIComponent(guiManager);
		movableArea = new MovableArea(guiManager);
		backgroundMovableArea = new MovableArea(guiManager);
		label = new Label(guiManager);
		closeButton = new ImageButton(guiManager);
		base.Add(box);
		base.Add(ViewPort);
		base.Add(label);
		base.Add(movableArea);
		resizableAreas = new ResizableArea[8];
		for (int i = 0; i < 8; i++)
		{
			resizableAreas[i] = new ResizableArea(guiManager);
			resizableAreas[i].ZOrder = 0.3f;
			resizableAreas[i].StartResizing += OnStartAnimating;
			resizableAreas[i].EndResizing += Window_EndResizing;
			base.Add(resizableAreas[i]);
		}
		resizableAreas[0].ResizeArea = ResizeAreas.TopLeft;
		resizableAreas[1].ResizeArea = ResizeAreas.Top;
		resizableAreas[1].DebugTag = "resizeTop";
		resizableAreas[2].ResizeArea = ResizeAreas.TopRight;
		resizableAreas[3].ResizeArea = ResizeAreas.Left;
		resizableAreas[4].ResizeArea = ResizeAreas.Right;
		resizableAreas[5].ResizeArea = ResizeAreas.BottomLeft;
		resizableAreas[6].ResizeArea = ResizeAreas.Bottom;
		resizableAreas[7].ResizeArea = ResizeAreas.BottomRight;
		movableArea.ZOrder = 0.1f;
		closeButton.ZOrder = 0.4f;
		ViewPort.ZOrder = 0.2f;
		ViewPort.CanHaveFocus = true;
		ViewPort.CanReceiveMouseWheelEvents = true;
		base.CanReceiveMouseWheelEvents = true;
		base.MinWidth = 16;
		Margin = defaultMargin;
		HasCloseButton = defaultHasCloseButton;
		IsMovable = defaultFullWindowMovableArea;
		Width = base.MinWidth;
		Height = MinHeight;
		ButtonSize = defaultButtonSize;
		Skin = defaultSkin;
		CloseButtonSkin = defaultCloseButtonSkin;
		CloseButtonHoverSkin = defaultCloseButtonHoverSkin;
		CloseButtonPressedSkin = defaultCloseButtonPressedSkin;
		closeButton.Click += OnClose;
		movableArea.StartMoving += OnStartAnimating;
		movableArea.EndMoving += OnEndAnimating;
		backgroundMovableArea.StartMoving += OnStartAnimating;
		backgroundMovableArea.EndMoving += OnEndAnimating;
		guiManager.RegisterWindow(this);
	}

	private void Window_EndResizing(UIComponent sender)
	{
		OnEndAnimating(sender);
	}

	public override void CleanUp()
	{
		closeButton.CleanUp();
		ViewPort.CleanUp();
		backgroundMovableArea.CleanUp();
		base.CleanUp();
	}

	protected override void LoadGraphicsContent(bool loadAllContent)
	{
		base.LoadGraphicsContent(loadAllContent);
	}

	public override int Add(UIComponent control)
	{
		return ViewPort.Add(control);
	}

	public override bool Remove(UIComponent control)
	{
		return ViewPort.Remove(control);
	}

	public void CenterWindow()
	{
		base.X = guiManager.ScreenWidth / 2 - Width / 2;
		base.Y = guiManager.ScreenHeight / 2 - Height / 2;
	}

	public void ShowModal()
	{
		Show();
		base.GUIManager.SetModal(this);
	}

	public void Show()
	{
		base.GUIManager.Add(this);
		transitioning = Transition.In;
		ResetAllScrollBars();
		Visible = true;
	}

	internal void CloseWindow()
	{
		Visible = false;
		guiManager.WindowWasClosed(this);
		if (Parent == null)
		{
			base.GUIManager.Remove(this);
		}
		else
		{
			base.GUIManager.Remove(this);
		}
		if (this.Close != null)
		{
			this.Close(this);
		}
	}

	private void ResetAllScrollBars()
	{
		List<ScrollBar> foundChildren = null;
		FindChildOfType(null, ref foundChildren);
		if (foundChildren == null)
		{
			return;
		}
		foreach (ScrollBar item in foundChildren)
		{
			item.Value = 0;
		}
	}

	private void RefreshResizableAreas()
	{
		resizableAreas[0].X = 0;
		resizableAreas[0].Y = 0;
		resizableAreas[0].Width = resizableBorder;
		resizableAreas[0].Height = resizableBorder;
		resizableAreas[1].X = resizableBorder;
		resizableAreas[1].Y = 0;
		resizableAreas[1].Width = Width - 2 * resizableBorder;
		resizableAreas[1].Height = resizableBorder;
		resizableAreas[2].X = Width - resizableBorder;
		resizableAreas[2].Y = 0;
		resizableAreas[2].Width = resizableBorder;
		resizableAreas[2].Height = resizableBorder;
		resizableAreas[3].X = 0;
		resizableAreas[3].Y = resizableBorder;
		resizableAreas[3].Width = resizableBorder;
		resizableAreas[3].Height = Height - 2 * resizableBorder;
		resizableAreas[4].X = Width - resizableBorder;
		resizableAreas[4].Y = resizableBorder;
		resizableAreas[4].Width = resizableBorder;
		resizableAreas[4].Height = Height - 2 * resizableBorder;
		resizableAreas[5].X = 0;
		resizableAreas[5].Y = Height - resizableBorder;
		resizableAreas[5].Width = resizableBorder;
		resizableAreas[5].Height = resizableBorder;
		resizableAreas[6].X = resizableBorder;
		resizableAreas[6].Y = Height - resizableBorder;
		resizableAreas[6].Width = Width - 2 * resizableBorder;
		resizableAreas[6].Height = resizableBorder;
		resizableAreas[7].X = Width - resizableBorder;
		resizableAreas[7].Y = Height - resizableBorder;
		resizableAreas[7].Width = resizableBorder;
		resizableAreas[7].Height = resizableBorder;
	}

	private void OnStartAnimating(UIComponent sender)
	{
		base.IsAnimating = true;
	}

	public void Hide()
	{
		ResetMouseOver();
		foreach (UIComponent control in base.Controls)
		{
			control.ResetMouseOver();
		}
		transitioning = Transition.Out;
	}

	public override void Update(GameTime gameTime)
	{
		if (transitioning == Transition.In)
		{
			TransitionValue += (float)(gameTime.ElapsedGameTime.TotalSeconds / (double)timeToTransitionIn);
			if (TransitionValue >= 1f)
			{
				TransitionValue = 1f;
				transitioning = Transition.None;
			}
			ComputeAlpha();
		}
		else if (transitioning == Transition.Out)
		{
			TransitionValue -= (float)(gameTime.ElapsedGameTime.TotalSeconds / (double)timeToTransitionOut);
			if (TransitionValue <= 0f)
			{
				TransitionValue = 0f;
				transitioning = Transition.None;
				HideThisNow = true;
			}
			ComputeAlpha();
		}
		base.Update(gameTime);
	}

	private void ComputeAlpha()
	{
		alpha = transitionValue * Opacity;
	}

	protected override void OnMouseOver(UIComponent sender, MouseEventArgs args)
	{
		base.OnMouseOver(sender, args);
		if (Visible)
		{
			guiManager.MouseIsOverWindow(this);
		}
	}

	protected override void OnMove(UIComponent sender)
	{
		base.OnMove(sender);
		if (base.IsAnimating)
		{
			_ = transparency;
			_ = -1f;
		}
	}

	private void OnEndAnimating(UIComponent sender)
	{
		base.IsAnimating = false;
		_ = transparency;
		_ = -1f;
	}

	protected void OnClose(UIComponent sender, EventArgs e)
	{
		Hide();
	}

	protected override void OnResize(UIComponent sender)
	{
		base.OnResize(sender);
		box.Width = Width;
		movableArea.Width = Width;
		closeButton.X = Width - closeButton.Width - closeButton.Y;
		label.Width = Width - closeButton.Width - closeButton.Y - margin * 2;
		ViewPort.Width = Width - margin * 2;
		backgroundMovableArea.Width = ViewPort.Width;
		box.Height = Height;
		ViewPort.Height = Height - margin;
		backgroundMovableArea.Height = ViewPort.Height;
		RefreshResizableAreas();
		if (base.IsAnimating)
		{
			_ = transparency;
			_ = -1f;
		}
	}

	internal override void Draw(SpriteBatch spriteBatch, Rectangle parentScissor, RenderType typesToRender, float a)
	{
		base.Draw(spriteBatch, parentScissor, typesToRender, alpha);
	}

	protected override void DrawControl(SpriteBatch spriteBatch, Rectangle parentScissor, float a)
	{
		base.DrawControl(spriteBatch, parentScissor, a);
		if (this.DrawContentEvent != null)
		{
			this.DrawContentEvent(this, spriteBatch);
		}
	}
}
