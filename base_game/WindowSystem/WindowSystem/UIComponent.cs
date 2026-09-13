using System;
using System.Collections.Generic;
using System.Linq;
using InputEventSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WindowSystem;

public class UIComponent
{
	public enum DataControlID
	{
		None,
		Caption,
		Stock,
		Available,
		Unavailable,
		CurrentOrders,
		MaxOrders,
		Build,
		Up,
		Down,
		Amount,
		Condition,
		Status,
		StatusIconBackground,
		StatusIcon,
		Background,
		Location,
		Immigration,
		Communication,
		Transport,
		SalvageAction,
		PackingDownAction,
		DiscardClaim,
		Filter,
		UpgradeAllow,
		UpgradeProhibit,
		UpgradeAllowItem,
		UpgradeProhibitIem,
		HasOverridingItemIcon,
		PriorityNormal,
		PriorityHigh,
		PriorityLow,
		Randomize,
		Option,
		Difficulty,
		Score,
		LeftGrid,
		Tools,
		ToolsError,
		JobType,
		JobsPanelAssignedWorker,
		JobStatus,
		Progress,
		Cancel,
		Action,
		Price,
		GoodsForSale,
		WillingToBuy,
		Connector,
		HorizontalConnector,
		ETA,
		NextStop,
		ErrorsAndMessages,
		Profession,
		Name,
		Selector,
		Age,
		Sex,
		Rating,
		DistanceCaption,
		Distance,
		CapacityCaption,
		Capacity,
		FuelConsumptionCaption,
		FuelConsumption,
		TravelTimeCaption,
		TravelTime,
		SmallArrow,
		BigArrow,
		Track,
		Expand,
		Attainable,
		Goods,
		MapTexture,
		PanelBox,
		GridInItemRow,
		RatingIcon,
		Bulk,
		Personnel,
		NotAttainableIcons,
		Warning,
		AttainableIcons,
		Priority,
		ApplyPriority,
		Skill,
		Passengers,
		CannotEmigrate,
		Icon,
		Actions,
		VotersFor,
		VotersAgainst,
		Prompt,
		VotersForIcon,
		VotersAgainstIcon,
		HasSelection,
		StandingOrderModeHotspot,
		StandingOrderModePadlock,
		TravelStatus,
		InputHeading,
		OfferDemand,
		BiggestConcernIcon,
		Orders,
		CurrentCategoryOrders,
		Habitat,
		Produced,
		Consumed,
		Degraded,
		CritterEaten,
		Disappeared,
		Overconsumed,
		Stored,
		DaysLeft,
		Value,
		Regrowth,
		Productivity,
		Killed
	}

	public GUIManager guiManager;

	private Rectangle location;

	private Point absolutePosition;

	private List<UIComponent> controls;

	private UIComponent parent;

	private int minWidth;

	private int minHeight;

	private int? maxWidth;

	private int? maxHeight;

	private float zOrder;

	private bool canHaveFocus;

	private bool isRedrawRequired;

	private bool isInitialized;

	private bool isAnimating;

	private bool isMouseOver;

	private bool isPressed;

	private bool rightIsPressed;

	private RenderType renderType;

	private EventArgs eventArgs;

	protected string toolTip;

	private bool visible = true;

	public static Color lcdButtonHoverTint = Color.Azure;

	public static Color lcdHoverTint = Color.LightGray;

	public static Color lcdTooltipHoverTint = Color.LightGray;

	public static Color lcdTooltipPressedTint = Color.DarkGray;

	public static Color lcdPressedTint = Color.DarkGray;

	public static Color lcdTooltipCheckedTint = Color.LightGray;

	public static Color lcdTooltipCheckedPressedTint = Color.DarkGray;

	public static Color lcdTooltipCheckedHoverTint = Color.LightGray;

	public static Color panelHoverTint = Color.LightGray;

	public static Color hudHoverTint = Color.Yellow;

	public static Color hudHoverTintNonEnabled = Color.Gray;

	public static Color hudPressedTint = new Color(205, 216, 52);

	public static Color hudCheckedTint = Color.Turquoise;

	public static Color hudCheckedPressedTint = Color.DarkTurquoise;

	public static Color hudCheckedHoverTint = Color.YellowGreen;

	public static Color lcdDisabledColor = Color.Gray;

	public static Color lcdHoverDisabledColor = Color.LightGray;

	public static Color HUDDisabledColor = Color.Gray;

	public static Color errorColor = new Color(149, 53, 64);

	public static Color lcdLight = new Color(223, 238, 240);

	public static Color lcdYellow = new Color(255, 235, 40);

	public static Color HUDLightTint = "#9FB5B2".ColorFromHex().Value;

	public const string HUDTintHex = "#859997";

	public static Color HUDTint = "#859997".ColorFromHex().Value;

	public const string LCDTintHex = "#DFEDEE";

	public static Color LCDTint = "#DFEDEE".ColorFromHex().Value;

	public const string LCDNormalHex = "#1E525C";

	public static Color LCDNormal = "#1E525C".ColorFromHex().Value;

	public static Color LCDDark = new Color(45, 64, 70);

	public static Color OffWhiteColor = new Color(226, 226, 226);

	private bool tooltipExpires = true;

	private int tooltipWidth = 200;

	public DataControlID ID;

	public object Tag1;

	public object Tag2;

	public object OrderByTag1;

	public object OrderByTag2;

	public string Name = "";

	public string DebugTag = "";

	private bool enabled = true;

	public bool ClipThis = true;

	private Level level = Level.RockBottom;

	protected bool drawChildrenFirst;

	public virtual bool Visible
	{
		get
		{
			return visible;
		}
		set
		{
			bool wasVisible = visible;
			visible = value;
			// A component that is hidden while the pointer is on it never gets its MouseOut.
			// CheckMouseStatus - the only thing that sends one - returns immediately for anything
			// invisible, so the hover is left switched on: isMouseOver stays true, and whatever the
			// component did on MouseOver stays done. For a ResizableArea that means the resize
			// cursor stays a double arrow after the window under it closes, which is the "cursor
			// gets stuck in that mode for a while" a modder reported; it also means the next time
			// the component is shown under the pointer it will not raise MouseOver, because as far
			// as it knows the pointer never left.
			//
			// Sent here rather than checked per frame, which is what the report asked for: this
			// costs nothing except at the moment something is hidden.
			if (wasVisible && !value && isMouseOver)
			{
				ReleaseHover();
			}
			if (controls == null)
			{
				return;
			}
			foreach (UIComponent control in controls)
			{
				control.Visible = value;
			}
		}
	}

	public virtual string ToolTip
	{
		get
		{
			if (this.TooltipRequested != null)
			{
				this.TooltipRequested(this);
			}
			return toolTip;
		}
		set
		{
			toolTip = value;
			if (!string.IsNullOrEmpty(toolTip))
			{
				CanHaveFocus = true;
			}
		}
	}

	public virtual bool TooltipExpires
	{
		get
		{
			return tooltipExpires;
		}
		set
		{
			tooltipExpires = value;
		}
	}

	public virtual int TooltipWidth
	{
		get
		{
			return tooltipWidth;
		}
		set
		{
			tooltipWidth = Math.Max(value, 50);
		}
	}

	public virtual bool Enabled
	{
		get
		{
			return enabled;
		}
		set
		{
			enabled = value;
		}
	}

	public virtual RenderType RenderType
	{
		get
		{
			return renderType;
		}
		set
		{
			renderType = value;
			foreach (UIComponent control in controls)
			{
				control.RenderType = value;
			}
		}
	}

	public Level Level
	{
		get
		{
			return level;
		}
		set
		{
			level = value;
			foreach (UIComponent control in controls)
			{
				control.Level = value;
			}
		}
	}

	protected GUIManager GUIManager => guiManager;

	public Rectangle Location => location;

	public virtual EventArgs EventArgs
	{
		get
		{
			return eventArgs;
		}
		set
		{
			eventArgs = value;
		}
	}

	public int X
	{
		get
		{
			return location.X;
		}
		set
		{
			if (location.X != value)
			{
				_ = 1581;
				location.X = value;
				if (this.Move != null)
				{
					this.Move(this);
				}
			}
		}
	}

	public int Right => location.X + Width;

	public int MiddleVertical => location.Y + Height / 2;

	public int Y
	{
		get
		{
			return location.Y;
		}
		set
		{
			if (location.Y != value)
			{
				location.Y = value;
				if (this.Move != null)
				{
					this.Move(this);
				}
			}
		}
	}

	public Point Position
	{
		get
		{
			return location.Location;
		}
		set
		{
			X = value.X;
			Y = value.Y;
		}
	}

	public virtual int Width
	{
		get
		{
			return location.Width;
		}
		set
		{
			if (value != location.Width)
			{
				if (value < minWidth)
				{
					value = minWidth;
				}
				if (maxWidth.HasValue && value > maxWidth.Value)
				{
					value = maxWidth.Value;
				}
				location.Width = value;
				if (this.Resize != null)
				{
					this.Resize(this);
				}
			}
		}
	}

	public int Bottom => Y + Height;

	public virtual int Height
	{
		get
		{
			return location.Height;
		}
		set
		{
			if (location.Height != value)
			{
				if (value < minHeight)
				{
					value = minHeight;
				}
				if (maxHeight.HasValue && value > maxHeight.Value)
				{
					value = maxHeight.Value;
				}
				location.Height = value;
				if (this.Resize != null)
				{
					this.Resize(this);
				}
				if (this.HeightResize != null)
				{
					this.HeightResize(this);
				}
			}
		}
	}

	public Point AbsolutePosition => absolutePosition;

	public List<UIComponent> Controls => controls;

	public virtual UIComponent Parent
	{
		get
		{
			return parent;
		}
		internal set
		{
			if (parent != null)
			{
				parent.Move -= OnParentMoved;
				parent.Resize -= OnParentResized;
			}
			parent = value;
			if (parent != null)
			{
				parent.Move += OnParentMoved;
				parent.Resize += OnParentResized;
			}
			Refresh();
		}
	}

	public int MinWidth
	{
		get
		{
			return minWidth;
		}
		set
		{
			minWidth = value;
			if (Width < minWidth)
			{
				Width = minWidth;
			}
		}
	}

	public int? MaxWidth
	{
		get
		{
			return maxWidth;
		}
		set
		{
			maxWidth = value;
			if (Width > maxWidth.Value)
			{
				Width = maxWidth.Value;
			}
		}
	}

	public virtual int? MaxHeight
	{
		get
		{
			return maxHeight;
		}
		set
		{
			maxHeight = value;
			if (Height > maxHeight.Value)
			{
				Height = maxHeight.Value;
			}
		}
	}

	public virtual int MinHeight
	{
		get
		{
			return minHeight;
		}
		set
		{
			minHeight = value;
			if (Height < minHeight)
			{
				Height = minHeight;
			}
		}
	}

	public float ZOrder
	{
		get
		{
			return zOrder;
		}
		set
		{
			zOrder = value;
		}
	}

	public bool CanHaveFocus
	{
		get
		{
			return canHaveFocus;
		}
		set
		{
			canHaveFocus = value;
		}
	}

	public bool CanReceiveMouseWheelEvents { get; set; }

	protected bool IsRedrawRequired
	{
		get
		{
			return isRedrawRequired;
		}
		set
		{
			isRedrawRequired = value;
		}
	}

	protected internal bool IsInitialized
	{
		get
		{
			return isInitialized;
		}
		set
		{
			isInitialized = value;
		}
	}

	protected internal bool IsAnimating
	{
		get
		{
			return isAnimating;
		}
		set
		{
			isAnimating = value;
		}
	}

	protected internal bool IsMouseOver => isMouseOver;

	protected bool IsPressed
	{
		get
		{
			return isPressed;
		}
		set
		{
			isPressed = value;
		}
	}

	public event MouseDownHandler MouseDown;

	public event MouseUpHandler MouseUp;

	public event MouseMoveHandler MouseMove;

	public event MouseWheelHandler MouseWheelChanged;

	public event KeyDownHandler KeyDown;

	public event KeyUpHandler KeyUp;

	public event ClickHandler Click;

	public event ClickHandler RightClick;

	public event MoveHandler Move;

	public event ResizeHandler Resize;

	public event ResizeHandler HeightResize;

	public event MouseOverHandler MouseOver;

	public event MouseOutHandler MouseOut;

	public event RequiresRedrawHandler RequiresRedraw;

	public event GetFocusHandler GetFocus;

	public event LoseFocusHandler LoseFocus;

	public event DrawContentHandler DrawContentEvent;

	public event UpdateHandler UpdateEvent;

	public event Action<UIComponent> TooltipRequested;

	public event Action<UIComponent, bool> TooltipDisplayChange;

	public event Action Destroyed;

	public void CenterThisVertically(int YPosToCenterTo)
	{
		Y = YPosToCenterTo - Height / 2;
	}

	public void CenterChildVertically(UIComponent childControl, int? childCenterYPos = null)
	{
		if (childCenterYPos.HasValue)
		{
			childControl.Y = (Height - 2 * childCenterYPos.Value) / 2;
		}
		else
		{
			childControl.Y = (Height - childControl.Height) / 2;
		}
	}

	public void AlignVertically(UIComponent controlToAlign)
	{
		controlToAlign.Y = Y + (Height - controlToAlign.Height) / 2;
	}

	public void AlignRight(int rightXPos)
	{
		X = rightXPos - Width;
	}

	public void CenterChildHorizontally(UIComponent childControl)
	{
		childControl.X = (Width - childControl.Width) / 2;
	}

	public void CenterHorizontally(int xPosToCenterAbout, UIComponent childControl)
	{
		childControl.X = xPosToCenterAbout - childControl.Width / 2;
	}

	public void CenterThisHorizontally(int xPosToCenterAbout)
	{
		X = xPosToCenterAbout - Width / 2;
	}

	public UIComponent(GUIManager guiManager)
	{
		this.guiManager = guiManager;
		absolutePosition = Point.Zero;
		controls = new List<UIComponent>();
		parent = null;
		location = new Rectangle(0, 0, 1, 1);
		minWidth = 1;
		minHeight = 1;
		zOrder = 0f;
		canHaveFocus = true;
		isRedrawRequired = true;
		isInitialized = false;
		isAnimating = false;
		isMouseOver = false;
		isPressed = false;
		rightIsPressed = false;
		MouseDown += OnMouseDown;
		MouseUp += OnMouseUp;
		MouseMove += OnMouseMove;
		MouseOver += OnMouseOver;
		MouseOut += OnMouseOut;
		MouseWheelChanged += OnMouseWheelChanged;
		KeyDown += OnKeyDown;
		KeyUp += OnKeyUp;
		Move += OnMove;
		Resize += OnResize;
		RequiresRedraw += OnRequiresRedraw;
		GetFocus += OnGetFocus;
		LoseFocus += OnLoseFocus;
	}

	public void NotifyTooltipShown()
	{
		if (this.TooltipDisplayChange != null)
		{
			this.TooltipDisplayChange(this, arg2: true);
		}
	}

	public void NotifyTooltipHidden()
	{
		if (this.TooltipDisplayChange != null)
		{
			this.TooltipDisplayChange(this, arg2: false);
		}
	}

	public virtual void Initialize()
	{
		foreach (UIComponent control in controls)
		{
			control.Initialize();
		}
		if (!isInitialized)
		{
			Refresh();
			isInitialized = true;
			LoadGraphicsContent(loadAllContent: true);
		}
	}

	public virtual void UnloadGraphicsContent(bool unloadAllContent)
	{
	}

	protected virtual void LoadGraphicsContent(bool loadAllContent)
	{
	}

	public void SortControls(Func<UIComponent, int> orderBy)
	{
		controls = controls.OrderBy(orderBy).ToList();
	}

	public virtual void Destroy()
	{
		if (controls != null)
		{
			foreach (UIComponent control in controls)
			{
				control.Destroy();
			}
		}
		if (this.Destroyed != null)
		{
			this.Destroyed();
		}
	}

	public virtual void CleanUp()
	{
		foreach (UIComponent control in controls)
		{
			control.CleanUp();
		}
		if (isInitialized)
		{
			if (guiManager.GetFocus() == this)
			{
				guiManager.SetFocus(null);
			}
			if (guiManager.GetModal() == this)
			{
				guiManager.SetModal(null);
			}
			isInitialized = false;
		}
	}

	public virtual void Update(GameTime gameTime)
	{
		foreach (UIComponent control in controls)
		{
			control.Update(gameTime);
		}
		if (this.UpdateEvent != null)
		{
			this.UpdateEvent(gameTime);
		}
	}

	public virtual void BringToTop()
	{
		guiManager.BringToTop(this);
	}

	protected internal bool IsChild(UIComponent key)
	{
		if (key == this)
		{
			return true;
		}
		foreach (UIComponent control in controls)
		{
			if (control.IsChild(key))
			{
				return true;
			}
		}
		return false;
	}

	public virtual int Add(UIComponent control)
	{
		Insert(control, controls.Count);
		return controls.Count;
	}

	public bool Contains(UIComponent control)
	{
		return controls.Contains(control);
	}

	public void Insert(UIComponent control, int indexPosition)
	{
		if (!controls.Contains(control))
		{
			control.Parent = this;
			if (renderType != RenderType.Normal)
			{
				control.RenderType = renderType;
			}
			control.ClipThis = ClipThis;
			control.Level = Level;
			control.Initialize();
			controls.Insert(indexPosition, control);
		}
	}

	public virtual bool Remove(UIComponent control)
	{
		bool result = false;
		if (controls.Remove(control))
		{
			// Before it is orphaned: a control removed while the pointer is on it never hears
			// about the pointer leaving, and whatever it set on MouseOver - a cursor, a tint -
			// stays set with nothing left to unset it.
			control.ReleaseHoverTree();
			control.Parent = null;
			control.CleanUp();
			result = true;
		}
		return result;
	}

	internal static bool CheckSkinLocation(Rectangle location)
	{
		bool result = false;
		if (location.X >= 0 && location.Y >= 0 && location.Width > 0 && location.Height > 0)
		{
			result = true;
		}
		return result;
	}

	public void Redraw()
	{
		if (this.RequiresRedraw != null)
		{
			this.RequiresRedraw(this);
		}
	}

	protected void Refresh()
	{
		if (parent != null)
		{
			absolutePosition.X = parent.AbsolutePosition.X + X;
			absolutePosition.Y = parent.AbsolutePosition.Y + Y;
		}
		else
		{
			absolutePosition.X = X;
			absolutePosition.Y = Y;
		}
		foreach (UIComponent control in controls)
		{
			control.Refresh();
		}
	}

	public Window GetParentWindow()
	{
		if (this is Window)
		{
			return (Window)this;
		}
		if (parent != null)
		{
			return parent.GetParentWindow();
		}
		return null;
	}

	internal virtual void Draw(SpriteBatch spriteBatch, Rectangle parentScissor, RenderType typesToRender, float alpha)
	{
		if (!Visible)
		{
			return;
		}
		Rectangle value = location;
		value.X = absolutePosition.X;
		value.Y = absolutePosition.Y;
		bool result;
		if (!ClipThis)
		{
			result = true;
		}
		else
		{
			parentScissor.Intersects(ref value, out result);
		}
		_ = DebugTag == "rbDisabled";
		if (!result)
		{
			return;
		}
		if (ClipThis)
		{
			ClipToParent(ref parentScissor, ref value);
		}
		if (RenderType == typesToRender)
		{
			if (drawChildrenFirst)
			{
				DrawChildren(spriteBatch, typesToRender, alpha, value);
			}
			DrawControl(spriteBatch, value, alpha);
			if (this.DrawContentEvent != null)
			{
				this.DrawContentEvent();
			}
		}
		if (!drawChildrenFirst)
		{
			DrawChildren(spriteBatch, typesToRender, alpha, value);
		}
	}

	private void DrawChildren(SpriteBatch spriteBatch, RenderType typesToRender, float alpha, Rectangle thisScissor)
	{
		foreach (UIComponent control in controls)
		{
			control.Draw(spriteBatch, thisScissor, typesToRender, alpha);
		}
	}

	private static void ClipToParent(ref Rectangle parentScissor, ref Rectangle thisScissor)
	{
		if (thisScissor.X < parentScissor.X)
		{
			thisScissor.Width -= parentScissor.X - thisScissor.X;
			thisScissor.X = parentScissor.X;
		}
		if (thisScissor.Right > parentScissor.Right)
		{
			thisScissor.Width -= thisScissor.Right - parentScissor.Right;
		}
		if (thisScissor.Y < parentScissor.Y)
		{
			thisScissor.Height -= parentScissor.Y - thisScissor.Y;
			thisScissor.Y = parentScissor.Y;
		}
		if (thisScissor.Bottom > parentScissor.Bottom)
		{
			thisScissor.Height -= thisScissor.Bottom - parentScissor.Bottom;
		}
	}

	protected virtual void DrawControl(SpriteBatch spriteBatch, Rectangle parentScissor, float alpha)
	{
	}

	internal UIComponent CheckFocus(int x, int y, TestMode mode)
	{
		UIComponent uIComponent = null;
		if (!isAnimating && Visible && CheckCoordinates(x, y))
		{
			float num = 0f;
			foreach (UIComponent control in controls)
			{
				if (control.ZOrder >= num)
				{
					UIComponent uIComponent2 = control.CheckFocus(x, y, mode);
					if (uIComponent2 != null)
					{
						uIComponent = uIComponent2;
						num = control.ZOrder;
					}
				}
			}
			if (uIComponent == null)
			{
				if (mode == TestMode.Focus && canHaveFocus)
				{
					uIComponent = this;
				}
				else if (mode == TestMode.MouseWheel && CanReceiveMouseWheelEvents)
				{
					uIComponent = this;
				}
			}
		}
		return uIComponent;
	}

	internal UIComponent CheckMouseStatus(MouseEventArgs args, Rectangle parentScissor)
	{
		UIComponent uIComponent = null;
		if (!isAnimating && Visible)
		{
			Rectangle thisScissor = location;
			thisScissor.X = absolutePosition.X;
			thisScissor.Y = absolutePosition.Y;
			ClipToParent(ref parentScissor, ref thisScissor);
			bool flag = thisScissor.Contains(args.Position);
			if (isMouseOver && !flag)
			{
				InvokeMouseOut(args);
			}
			if (flag)
			{
				float num = 0f;
				foreach (UIComponent control in controls)
				{
					if (!control.Visible || !(control.ZOrder >= num))
					{
						continue;
					}
					UIComponent uIComponent2 = control.CheckMouseStatus(args, thisScissor);
					if (uIComponent2 != null)
					{
						if (uIComponent != null && uIComponent.IsMouseOver)
						{
							uIComponent.InvokeMouseOut(args);
						}
						uIComponent = uIComponent2;
						num = control.ZOrder;
					}
				}
				if (uIComponent == null && canHaveFocus)
				{
					uIComponent = this;
				}
			}
			else
			{
				foreach (UIComponent control2 in controls)
				{
					control2.CheckMouseStatus(args, thisScissor);
				}
			}
		}
		return uIComponent;
	}

	internal void InvokeMouseOver(MouseEventArgs args)
	{
		isMouseOver = true;
		this.MouseOver(this, args);
	}

	internal void InvokeMouseOut(MouseEventArgs args)
	{
		isMouseOver = false;
		this.MouseOut(this, args);
	}

	internal void GiveFocus()
	{
		if (this.GetFocus != null)
		{
			this.GetFocus();
		}
	}

	internal void TakeFocus()
	{
		if (this.LoseFocus != null)
		{
			this.LoseFocus();
		}
	}

	public bool CheckCoordinates(int x, int y)
	{
		bool result = false;
		if (x >= absolutePosition.X && x < absolutePosition.X + location.Width && y >= absolutePosition.Y && y < absolutePosition.Y + location.Height)
		{
			result = true;
		}
		return result;
	}

	public virtual void KeyDownIntercept(KeyEventArgs args)
	{
		if (enabled)
		{
			this.KeyDown(args);
		}
	}

	public virtual void KeyUpIntercept(KeyEventArgs args)
	{
		if (enabled)
		{
			this.KeyUp(args);
		}
	}

	public virtual void MouseDownIntercept(MouseEventArgs args)
	{
		if (enabled && CheckCoordinates(args.Position.X, args.Position.Y))
		{
			this.MouseDown(args);
		}
	}

	public virtual void MouseUpIntercept(MouseEventArgs args)
	{
		if (enabled)
		{
			this.MouseUp(args);
		}
	}

	public virtual void MouseMoveIntercept(MouseEventArgs args)
	{
		if (enabled)
		{
			this.MouseMove(args);
		}
	}

	public virtual void MouseWheelIntercept(int wheelChange)
	{
		if (enabled)
		{
			this.MouseWheelChanged(wheelChange);
		}
	}

	protected virtual void OnKeyDown(KeyEventArgs args)
	{
	}

	protected virtual void OnKeyUp(KeyEventArgs args)
	{
	}

	protected virtual void OnMouseDown(MouseEventArgs args)
	{
		if (args.Button == MouseButtons.Left)
		{
			isPressed = true;
		}
		else if (args.Button == MouseButtons.Right)
		{
			rightIsPressed = true;
		}
	}

	protected virtual void OnMouseUp(MouseEventArgs args)
	{
		if (!Enabled || !Visible)
		{
			return;
		}
		if (isPressed)
		{
			if (args.Button == MouseButtons.Left)
			{
				isPressed = false;
				if (this.Click != null && CheckCoordinates(args.Position.X, args.Position.Y))
				{
					this.Click(this, eventArgs);
				}
			}
		}
		else if (rightIsPressed && args.Button == MouseButtons.Right)
		{
			rightIsPressed = false;
			if (this.RightClick != null && CheckCoordinates(args.Position.X, args.Position.Y))
			{
				this.RightClick(this, eventArgs);
			}
		}
	}

	protected virtual void OnMouseMove(MouseEventArgs args)
	{
	}

	protected virtual void OnMouseOver(UIComponent sender, MouseEventArgs args)
	{
		if (toolTip != null && Visible)
		{
			guiManager.ShowToolTip(this);
		}
	}

	protected virtual void OnMouseOut(UIComponent sender, MouseEventArgs args)
	{
		guiManager.HideToolTip(this);
	}

	protected virtual void OnMouseWheelChanged(int wheelChange)
	{
	}

	protected virtual void OnMove(UIComponent sender)
	{
		Refresh();
		ReleaseHoverIfPointerLeft();
	}

	protected virtual void OnResize(UIComponent sender)
	{
		Refresh();
		ReleaseHoverIfPointerLeft();
	}

	/// <summary>
	/// The pointer can leave a component without moving: the component can move out from under it.
	///
	/// MouseOut is only ever sent from CheckMouseStatus, which runs on mouse MOVEMENT - so a
	/// stationary pointer over something that then scrolls, moves or is resized away keeps its
	/// hover forever. That is the second half of the stuck resize cursor: hiding the window was
	/// one route (handled in the Visible setter), and scrolling a data sheet under a still pointer
	/// is the other. Reported both times by the same modder, the second time after the first fix.
	///
	/// Only on an actual move or resize of a component that currently holds the hover, so a
	/// per-frame position check - which the report specifically asked to avoid - is not needed.
	/// </summary>
	private void ReleaseHoverIfPointerLeft()
	{
		if (!isMouseOver || guiManager == null || guiManager.InputData == null)
		{
			return;
		}
		if (!CheckCoordinates(guiManager.InputData.mouseX, guiManager.InputData.mouseY))
		{
			ReleaseHover();
		}
	}

	/// <summary>
	/// Gives up this component's hover, and its children's, wherever they are holding one.
	///
	/// For removal, which - unlike hiding - does not cascade through the Visible setter: a control
	/// taken out of its parent while the pointer is on it would otherwise keep isMouseOver true
	/// for good, along with whatever it did on the way in.
	/// </summary>
	internal void ReleaseHoverTree()
	{
		ReleaseHover();
		if (controls == null)
		{
			return;
		}
		foreach (UIComponent control in controls)
		{
			control.ReleaseHoverTree();
		}
	}

	/// <summary>
	/// Gives up the hover WITHOUT raising MouseOut.
	///
	/// The first version of this raised a synthesized MouseOut, and that was wrong in a way that
	/// took a crash report to see. MouseOut handlers are written for "the pointer moved off me",
	/// and some of them clear state that the code hiding the component is about to use. The game's
	/// own action picker does exactly that: <c>ActionPicker_ActionSelected</c> calls
	/// <c>HideActionPicker()</c> and then reads <c>actionPickerSourceButton</c>, which
	/// <c>ActionPicker_MouseOut</c> sets to null - so hiding it mid-click nulled the field a line
	/// before it was dereferenced, and picking a trade action threw a NullReferenceException.
	///
	/// So the flag is cleared and nothing else runs. That is all the hover state needs: the
	/// component will raise MouseOver again next time the pointer is genuinely over it, because it
	/// no longer believes the pointer is already there. What a MouseOut handler would have done to
	/// the world - a tint, a cursor - either does not matter on something invisible, or belongs in
	/// <see cref="OnHoverReleased"/>, which is for the handful of components that change something
	/// outside themselves.
	/// </summary>
	internal void ReleaseHover()
	{
		if (!isMouseOver)
		{
			return;
		}
		isMouseOver = false;
		OnHoverReleased();
	}

	/// <summary>
	/// Called when the hover is given up without the pointer moving - the component was hidden,
	/// moved, resized or removed out from under it.
	///
	/// Empty here on purpose: the point of the silent release is that arbitrary handlers do NOT
	/// run. Override it only to undo something the component did to the world outside itself, of
	/// which the mouse cursor is the whole list.
	/// </summary>
	protected virtual void OnHoverReleased()
	{
	}

	protected virtual void OnParentMoved(UIComponent sender)
	{
		this.Move(this);
	}

	protected virtual void OnParentResized(UIComponent sender)
	{
	}

	protected virtual void OnRequiresRedraw(UIComponent sender)
	{
		isRedrawRequired = true;
		if (parent != null)
		{
			parent.OnRequiresRedraw(sender);
		}
	}

	public UIComponent FindParentOfType(Type componentType)
	{
		if (Parent == null)
		{
			return null;
		}
		if (Parent != null && Parent.GetType() == componentType)
		{
			return Parent;
		}
		return Parent.FindParentOfType(componentType);
	}

	public void FindChildById<T>(DataControlID id, out T child, bool firstLevelOnly = false) where T : UIComponent
	{
		UIComponent uIComponent = FindChildById(id, firstLevelOnly);
		if (uIComponent != null)
		{
			child = (T)uIComponent;
		}
		else
		{
			child = null;
		}
	}

	public UIComponent FindChildById(DataControlID id, bool firstLevelOnly = false)
	{
		foreach (UIComponent control in Controls)
		{
			if (control.ID == id)
			{
				return control;
			}
		}
		if (!firstLevelOnly)
		{
			UIComponent uIComponent = null;
			foreach (UIComponent control2 in Controls)
			{
				uIComponent = control2.FindChildById(id);
				if (uIComponent != null)
				{
					return uIComponent;
				}
			}
		}
		return null;
	}

	public void FindChildrenById(DataControlID id, ref List<UIComponent> foundChildren, bool firstLevelOnly = false)
	{
		foreach (UIComponent control in Controls)
		{
			if (control.ID == id)
			{
				Util.AddToList(ref foundChildren, control);
			}
		}
		if (firstLevelOnly)
		{
			return;
		}
		foreach (UIComponent control2 in Controls)
		{
			control2.FindChildrenById(id, ref foundChildren);
		}
	}

	public UIComponent FindChildById(string id)
	{
		foreach (UIComponent control in Controls)
		{
			if (control.Name == id)
			{
				return control;
			}
		}
		UIComponent uIComponent = null;
		foreach (UIComponent control2 in Controls)
		{
			uIComponent = control2.FindChildById(id);
			if (uIComponent != null)
			{
				return uIComponent;
			}
		}
		return null;
	}

	public void ResetMouseOver()
	{
		isMouseOver = false;
	}

	public void FindChildOfType<T>(UIComponent ignoreThis, ref List<T> foundChildren) where T : UIComponent
	{
		foreach (UIComponent control in Controls)
		{
			if (control != ignoreThis && control is T)
			{
				if (foundChildren == null)
				{
					foundChildren = new List<T>();
				}
				foundChildren.Add((T)control);
			}
		}
		if (this is CollapsablePanel collapsablePanel)
		{
			collapsablePanel.ExpandedPanel.FindChildOfType(ignoreThis, ref foundChildren);
			return;
		}
		foreach (UIComponent control2 in Controls)
		{
			control2.FindChildOfType(ignoreThis, ref foundChildren);
		}
	}

	protected virtual void OnGetFocus()
	{
	}

	protected virtual void OnLoseFocus()
	{
		isPressed = false;
		rightIsPressed = false;
	}
}
