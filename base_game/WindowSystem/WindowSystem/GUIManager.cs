using System;
using System.Collections.Generic;
using InputEventSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpriteSheetRuntime;

namespace WindowSystem;

public class GUIManager
{
	public delegate void ShowTooltipHandler(UIComponent sender);

	public delegate void WindowClosedHandler(Window sender);

	public delegate void HyperlinkClickedHandler(uint? entityID, uint? containerID, uint? zoneID, Point? mapPos, MouseButtonClicked button);

	public delegate void SetMouseCursorHandler(MouseSprites mouseSprite);

	public enum MouseButtonClicked
	{
		Left,
		Right
	}

	private static string defaultSkinTexture = "Textures/DefaultStyle";

	private static ContentManager contentManager;

	public InputData InputData;

	private Dictionary<Level, List<UIComponent>> controls;

	private HashSet<Window> allWindows;

	private UIComponent focusedControl;

	private UIComponent mouseWheelTarget;

	private UIComponent modalControl;

	public MouseSprites MouseSprite;

	public Dictionary<string, Color> CustomColors = new Dictionary<string, Color>();

	public static string LCDandHUDSubheadingFontPath = "Fonts/LCDandHUDSubHeading";

	public static SpriteFont LCDandHUDSubheadingFont;

	public static string LCDInterfaceBoldFontPath = "Fonts/LCD_bold";

	public static SpriteFont LCDInterfaceBoldFont;

	public static string MediumButtonFaceFontPath = "Fonts/newtown_8pt";

	public static SpriteFont MediumButtonInterfaceFont;

	public static string CRTGlowFontPath = "Fonts/CRTGlow";

	public static SpriteFont CRTBigGlowFont;

	public static string LCDandHUDBodyFontPath = "Fonts/LCDandHUDBody";

	public static SpriteFont LCDandHUDFont;

	public static SpriteFont VeryLargeInterfaceFont;

	public static string CRTBasicFontPath = "Fonts/CRT_18pt";

	public static SpriteFont CRTBasicFont;

	public static SoundEffect Click1;

	public static SoundEffect BeepBasicPanel;

	public static SoundEffect BeepLCD;

	public static SoundEffect BeepMainPanel;

	public static SoundEffect BeepMetalPanel;

	public static SoundEffect WhiteNoise;

	public static SoundEffect CRTTurnOn;

	public static SoundEffect PlaceBuildingBeep;

	public SpriteSheet GUISpriteSheet;

	public SpriteSheet GUI_CRT_SpriteSheet;

	private SpriteBatch spriteBatch;

	public Level LCDLevel = Level.RockBottom;

	public bool MouseIsInLCDInterface;

	public Game Game;

	public ContentManager ContentManager
	{
		get
		{
			return contentManager;
		}
		set
		{
			contentManager = value;
		}
	}

	internal Texture2D SkinTexture => GUISpriteSheet.Texture;

	public int ScreenHeight { get; private set; }

	public int ScreenWidth { get; private set; }

	public event ShowTooltipHandler ShowTooltip;

	public event ShowTooltipHandler HideTooltip;

	public event HyperlinkClickedHandler HyperlinkClicked;

	public event SetMouseCursorHandler SetMouseCursorEvent;

	public event WindowClosedHandler WindowClosed;

	public event Action<Window> MouseOverWindow;

	public event Action<Window> MouseOutOfWindow;

	public GUIManager(Game game, int width, int height, InputData inputData, ContentManager content, bool receiveInputEvents = true)
	{
		Game = game;
		ScreenWidth = width;
		ScreenHeight = height;
		ContentManager = content;
		InputData = inputData;
		controls = new Dictionary<Level, List<UIComponent>>();
		controls.Add(Level.Min, new List<UIComponent>());
		controls.Add(Level.RockBottom, new List<UIComponent>());
		controls.Add(Level.Bottom, new List<UIComponent>());
		controls.Add(Level.BelowBelowBelowMiddle, new List<UIComponent>());
		controls.Add(Level.BelowBelowMiddle, new List<UIComponent>());
		controls.Add(Level.BelowMiddle, new List<UIComponent>());
		controls.Add(Level.Middle, new List<UIComponent>());
		controls.Add(Level.Dialogs, new List<UIComponent>());
		controls.Add(Level.StackedDialogs, new List<UIComponent>());
		controls.Add(Level.EntityTypeInfo, new List<UIComponent>());
		controls.Add(Level.EventDialog, new List<UIComponent>());
		controls.Add(Level.Menu, new List<UIComponent>());
		controls.Add(Level.MessageBox, new List<UIComponent>());
		controls.Add(Level.Tooltip, new List<UIComponent>());
		controls.Add(Level.ComboBoxList, new List<UIComponent>());
		allWindows = new HashSet<Window>();
		focusedControl = null;
		modalControl = null;
		if (receiveInputEvents)
		{
			InputData.RequestingFocus += RequestingFocus;
			InputData.MouseWheelMove += InputData_MouseWheelMove;
			InputData.KeyDown += InputData_KeyDown;
			InputData.KeyUp += InputData_KeyUp;
			InputData.MouseMove += InputData_MouseMove;
			InputData.MouseDown += InputData_MouseDown;
			InputData.MouseUp += InputData_MouseUp;
		}
		LoadContent();
	}

	private void InputData_KeyDown(KeyEventArgs args)
	{
		if (focusedControl != null)
		{
			focusedControl.KeyDownIntercept(args);
		}
	}

	private void InputData_KeyUp(KeyEventArgs args)
	{
		if (focusedControl != null)
		{
			focusedControl.KeyUpIntercept(args);
		}
	}

	private void InputData_MouseDown(MouseEventArgs args)
	{
		if (focusedControl != null)
		{
			focusedControl.MouseDownIntercept(args);
		}
	}

	private void InputData_MouseUp(MouseEventArgs args)
	{
		if (focusedControl != null)
		{
			focusedControl.MouseUpIntercept(args);
		}
	}

	private void InputData_MouseMove(MouseEventArgs args)
	{
		if (focusedControl != null)
		{
			focusedControl.MouseMoveIntercept(args);
		}
		CheckMouseStatus(args);
	}

	private void InputData_MouseWheelMove(int wheelChange)
	{
		if (mouseWheelTarget != null)
		{
			mouseWheelTarget.MouseWheelIntercept(wheelChange);
		}
	}

	public void SetMousePosition(int x, int y)
	{
		if (InputData != null)
		{
			InputData.SetMousePosition(x, y);
		}
	}

	public void Destroy()
	{
		if (InputData != null)
		{
			InputData.RequestingFocus -= RequestingFocus;
			InputData.MouseMove -= InputData_MouseMove;
			InputData.MouseWheelMove -= InputData_MouseWheelMove;
			InputData.KeyDown -= InputData_KeyDown;
			InputData.KeyUp -= InputData_KeyUp;
			InputData.MouseDown -= InputData_MouseDown;
			InputData.MouseUp -= InputData_MouseUp;
		}
		foreach (KeyValuePair<Level, List<UIComponent>> control in controls)
		{
			foreach (UIComponent item in control.Value)
			{
				item.Destroy();
			}
		}
		allWindows.Clear();
		controls.Clear();
		focusedControl = null;
		modalControl = null;
		this.HideTooltip = null;
		this.HyperlinkClicked = null;
		this.SetMouseCursorEvent = null;
		this.ShowTooltip = null;
		this.WindowClosed = null;
	}

	protected void LoadContent()
	{
		GUISpriteSheet = ContentManager.Load<SpriteSheet>("GUI\\GUISprites");
		GUI_CRT_SpriteSheet = ContentManager.Load<SpriteSheet>("GUI\\GUI_CRT_Sprites");
		CRTBigGlowFont = ContentManager.Load<SpriteFont>(CRTGlowFontPath);
		CRTBigGlowFont.Spacing = -3f;
		CRTBasicFont = ContentManager.Load<SpriteFont>(CRTBasicFontPath);
		CRTBasicFont.Spacing = -4f;
		LCDandHUDFont = ContentManager.Load<SpriteFont>(LCDandHUDBodyFontPath);
		LCDandHUDFont.LineSpacing = 17;
		LCDandHUDSubheadingFont = ContentManager.Load<SpriteFont>(LCDandHUDSubheadingFontPath);
		LCDInterfaceBoldFont = ContentManager.Load<SpriteFont>(LCDInterfaceBoldFontPath);
		MediumButtonInterfaceFont = ContentManager.Load<SpriteFont>(MediumButtonFaceFontPath);
		MediumButtonInterfaceFont.Spacing = -2f;
		Click1 = ContentManager.Load<SoundEffect>("Sounds/CLICK14A_lowVolume");
		BeepBasicPanel = ContentManager.Load<SoundEffect>("Sounds/button beeps/Click3b");
		BeepLCD = ContentManager.Load<SoundEffect>("Sounds/button beeps/lcd_buttonclick_lowVolume");
		BeepMainPanel = ContentManager.Load<SoundEffect>("Sounds/button beeps/menuBeep");
		BeepMetalPanel = ContentManager.Load<SoundEffect>("Sounds/button beeps/metalpanel button_lowVolume");
		CRTTurnOn = ContentManager.Load<SoundEffect>("Sounds/tv/tvStaticTurnOn");
		WhiteNoise = ContentManager.Load<SoundEffect>("Sounds/tv/whitenoise_lowVolume");
		PlaceBuildingBeep = ContentManager.Load<SoundEffect>("Sounds/BIP2_lowVolume");
	}

	public void PlaySound(SoundEffect sound)
	{
		sound.Play();
	}

	public void ShowToolTip(UIComponent control)
	{
		if (this.ShowTooltip != null)
		{
			this.ShowTooltip(control);
		}
	}

	public void HideToolTip(UIComponent control)
	{
		if (this.HideTooltip != null)
		{
			this.HideTooltip(control);
		}
	}

	public void WindowWasClosed(Window window)
	{
		if (this.WindowClosed != null)
		{
			this.WindowClosed(window);
		}
	}

	public void MouseIsOverWindow(Window window)
	{
		if (this.MouseOverWindow != null)
		{
			this.MouseOverWindow(window);
		}
	}

	public void MouseIsOutOfWindow(Window window)
	{
		if (this.MouseOutOfWindow != null)
		{
			this.MouseOutOfWindow(window);
		}
	}

	public void HyperLinkClicked(uint? entityID, uint? resourceContainerID, uint? zoneID, Point? mapPos, MouseButtonClicked button)
	{
		if (this.HyperlinkClicked != null)
		{
			this.HyperlinkClicked(entityID, resourceContainerID, zoneID, mapPos, button);
		}
	}

	protected void UnloadContent()
	{
		foreach (KeyValuePair<Level, List<UIComponent>> control in controls)
		{
			foreach (UIComponent item in control.Value)
			{
				item.UnloadGraphicsContent(unloadAllContent: true);
			}
		}
	}

	public void Update(GameTime gameTime)
	{
		List<Window> list = null;
		foreach (KeyValuePair<Level, List<UIComponent>> control in controls)
		{
			foreach (UIComponent item in control.Value)
			{
				item.Update(gameTime);
				if (item is Window { HideThisNow: not false } window)
				{
					if (list == null)
					{
						list = new List<Window>();
					}
					list.Add(window);
					window.HideThisNow = false;
				}
			}
		}
		if (list == null)
		{
			return;
		}
		foreach (Window item2 in list)
		{
			item2.CloseWindow();
		}
	}

	public void Add(UIComponent control)
	{
		List<UIComponent> list = ((!(control is Window window)) ? controls[control.Level] : controls[window.Level]);
		if (!list.Contains(control))
		{
			control.Parent = null;
			control.Initialize();
			list.Add(control);
		}
	}

	public void EndSpriteBatch()
	{
		if (spriteBatch != null)
		{
			spriteBatch.End();
		}
	}

	public void BeginSpriteBatch()
	{
		if (spriteBatch != null)
		{
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
		}
	}

	public void RegisterWindow(Window window)
	{
		allWindows.Add(window);
	}

	public void Remove(UIComponent control)
	{
		List<UIComponent> list = ((!(control is Window window)) ? controls[control.Level] : controls[window.Level]);
		if (list.Remove(control))
		{
			control.CleanUp();
		}
	}

	internal void BringToTop(UIComponent control)
	{
		List<UIComponent> list = ((!(control is Window window)) ? controls[Level.Bottom] : controls[window.Level]);
		if (list.Remove(control))
		{
			list.Add(control);
		}
	}

	public void BringToBottom(Window window)
	{
		List<UIComponent> list = controls[window.Level];
		if (list.Remove(window))
		{
			list.Insert(0, window);
		}
	}

	public UIComponent GetFocus()
	{
		return focusedControl;
	}

	public UIComponent MouseWheelReceiver()
	{
		return mouseWheelTarget;
	}

	public void SetFocus(UIComponent control)
	{
		if (control != focusedControl && (control == null || control.CanHaveFocus) && (control == null || control.Visible))
		{
			UIComponent uIComponent = focusedControl;
			focusedControl = control;
			uIComponent?.TakeFocus();
			if (focusedControl != null)
			{
				focusedControl.GiveFocus();
			}
		}
	}

	public void SetMouseCursor(MouseSprites state)
	{
		MouseSprite = state;
		if (this.SetMouseCursorEvent != null)
		{
			this.SetMouseCursorEvent(state);
		}
	}

	public UIComponent GetModal()
	{
		return modalControl;
	}

	public void SetModal(UIComponent modalControl)
	{
		this.modalControl = modalControl;
		SetFocus(null);
	}

	public void Draw(GameTime gameTime, RenderType typesToRender, Level levelToDraw)
	{
		if (spriteBatch == null)
		{
			spriteBatch = new SpriteBatch(Game.GraphicsDevice);
		}
		Rectangle parentScissor = new Rectangle(0, 0, ScreenWidth, ScreenHeight);
		spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
		foreach (UIComponent item in controls[levelToDraw])
		{
			if (item is Window window)
			{
				if (!window.Visible)
				{
					continue;
				}
				switch (typesToRender)
				{
				case RenderType.CRTAndLCD:
					if (!window.HasCRTOrLCDComponents)
					{
						continue;
					}
					break;
				case RenderType.Overlay:
					if (!window.HasOverlayComponents)
					{
						continue;
					}
					break;
				}
				item.Draw(spriteBatch, parentScissor, typesToRender, 1f);
			}
			else
			{
				item.Draw(spriteBatch, parentScissor, typesToRender, 1f);
			}
		}
		spriteBatch.End();
	}

	private void RequestingFocus(MouseEventArgs args, TestMode mode)
	{
		UIComponent uIComponent = null;
		foreach (KeyValuePair<Level, List<UIComponent>> control in controls)
		{
			foreach (UIComponent item in control.Value)
			{
				if ((mode == TestMode.Focus && item.CanHaveFocus) || (mode == TestMode.MouseWheel && item.CanReceiveMouseWheelEvents))
				{
					UIComponent uIComponent2 = item.CheckFocus(args.Position.X, args.Position.Y, mode);
					if (uIComponent2 != null)
					{
						_ = uIComponent2.Visible;
						uIComponent = uIComponent2;
						_ = uIComponent is ScrollBar;
					}
				}
			}
		}
		if (modalControl == null || uIComponent == null || modalControl.IsChild(uIComponent))
		{
			if (mode == TestMode.Focus)
			{
				SetFocus(uIComponent);
				CheckMouseStatus(args);
			}
			else
			{
				mouseWheelTarget = uIComponent;
			}
		}
	}

	private void CheckMouseStatus(MouseEventArgs args)
	{
		UIComponent uIComponent = null;
		MouseIsInLCDInterface = false;
		LCDLevel = Level.RockBottom;
		Rectangle parentScissor = new Rectangle(0, 0, ScreenWidth, ScreenHeight);
		foreach (KeyValuePair<Level, List<UIComponent>> control in controls)
		{
			foreach (UIComponent item in control.Value)
			{
				if (item.IsAnimating)
				{
					return;
				}
				UIComponent uIComponent2 = item.CheckMouseStatus(args, parentScissor);
				if (uIComponent2 != null && (modalControl == null || modalControl.IsChild(uIComponent2)))
				{
					if (uIComponent != null && uIComponent.IsMouseOver)
					{
						uIComponent.InvokeMouseOut(args);
					}
					uIComponent = uIComponent2;
				}
			}
		}
		if (uIComponent != null && !uIComponent.IsMouseOver)
		{
			uIComponent.InvokeMouseOver(args);
		}
	}

	public bool IsMouseInInterface(int mouseX, int mouseY)
	{
		foreach (KeyValuePair<Level, List<UIComponent>> control in controls)
		{
			foreach (UIComponent item in control.Value)
			{
				if ((!(item is Window) || !((Window)item).IsBackgroundGraphics) && item.CheckCoordinates(mouseX, mouseY))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static string NumberKeyToString(KeyEventArgs args, bool allowDecimals)
	{
		switch (args.Key)
		{
		case Keys.NumPad0:
			return "0";
		case Keys.NumPad1:
			return "1";
		case Keys.NumPad2:
			return "2";
		case Keys.NumPad3:
			return "3";
		case Keys.NumPad4:
			return "4";
		case Keys.NumPad5:
			return "5";
		case Keys.NumPad6:
			return "6";
		case Keys.NumPad7:
			return "7";
		case Keys.NumPad8:
			return "8";
		case Keys.NumPad9:
			return "9";
		case Keys.D0:
			if (!args.Shift)
			{
				return "0";
			}
			return "";
		case Keys.D1:
			if (!args.Shift)
			{
				return "1";
			}
			return "";
		case Keys.D2:
			if (!args.Shift)
			{
				return "2";
			}
			return "";
		case Keys.D3:
			if (!args.Shift)
			{
				return "3";
			}
			return "";
		case Keys.D4:
			if (!args.Shift)
			{
				return "4";
			}
			return "";
		case Keys.D5:
			if (!args.Shift)
			{
				return "5";
			}
			return "";
		case Keys.D6:
			if (!args.Shift)
			{
				return "6";
			}
			return "";
		case Keys.D7:
			if (!args.Shift)
			{
				return "7";
			}
			return "";
		case Keys.D8:
			if (!args.Shift)
			{
				return "8";
			}
			return "";
		case Keys.D9:
			if (!args.Shift)
			{
				return "9";
			}
			return "";
		case Keys.Decimal:
			if (!allowDecimals)
			{
				return "";
			}
			return ".";
		case Keys.OemComma:
			if (!allowDecimals)
			{
				return "";
			}
			return ",";
		default:
			return "";
		}
	}

	public static string KeyToString(KeyEventArgs args)
	{
		switch (args.Key)
		{
		case Keys.A:
			if (!args.Shift)
			{
				return "a";
			}
			return "A";
		case Keys.B:
			if (!args.Shift)
			{
				return "b";
			}
			return "B";
		case Keys.C:
			if (!args.Shift)
			{
				return "c";
			}
			return "C";
		case Keys.D:
			if (!args.Shift)
			{
				return "d";
			}
			return "D";
		case Keys.E:
			if (!args.Shift)
			{
				return "e";
			}
			return "E";
		case Keys.F:
			if (!args.Shift)
			{
				return "f";
			}
			return "F";
		case Keys.G:
			if (!args.Shift)
			{
				return "g";
			}
			return "G";
		case Keys.H:
			if (!args.Shift)
			{
				return "h";
			}
			return "H";
		case Keys.I:
			if (!args.Shift)
			{
				return "i";
			}
			return "I";
		case Keys.J:
			if (!args.Shift)
			{
				return "j";
			}
			return "J";
		case Keys.K:
			if (!args.Shift)
			{
				return "k";
			}
			return "K";
		case Keys.L:
			if (!args.Shift)
			{
				return "l";
			}
			return "L";
		case Keys.M:
			if (!args.Shift)
			{
				return "m";
			}
			return "M";
		case Keys.N:
			if (!args.Shift)
			{
				return "n";
			}
			return "N";
		case Keys.O:
			if (!args.Shift)
			{
				return "o";
			}
			return "O";
		case Keys.P:
			if (!args.Shift)
			{
				return "p";
			}
			return "P";
		case Keys.Q:
			if (!args.Shift)
			{
				return "q";
			}
			return "Q";
		case Keys.R:
			if (!args.Shift)
			{
				return "r";
			}
			return "R";
		case Keys.S:
			if (!args.Shift)
			{
				return "s";
			}
			return "S";
		case Keys.T:
			if (!args.Shift)
			{
				return "t";
			}
			return "T";
		case Keys.U:
			if (!args.Shift)
			{
				return "u";
			}
			return "U";
		case Keys.V:
			if (!args.Shift)
			{
				return "v";
			}
			return "V";
		case Keys.W:
			if (!args.Shift)
			{
				return "w";
			}
			return "W";
		case Keys.X:
			if (!args.Shift)
			{
				return "x";
			}
			return "X";
		case Keys.Y:
			if (!args.Shift)
			{
				return "y";
			}
			return "Y";
		case Keys.Z:
			if (!args.Shift)
			{
				return "z";
			}
			return "Z";
		case Keys.NumPad0:
			return "0";
		case Keys.NumPad1:
			return "1";
		case Keys.NumPad2:
			return "2";
		case Keys.NumPad3:
			return "3";
		case Keys.NumPad4:
			return "4";
		case Keys.NumPad5:
			return "5";
		case Keys.NumPad6:
			return "6";
		case Keys.NumPad7:
			return "7";
		case Keys.NumPad8:
			return "8";
		case Keys.NumPad9:
			return "9";
		case Keys.D0:
			if (!args.Shift)
			{
				return "0";
			}
			return ")";
		case Keys.D1:
			if (!args.Shift)
			{
				return "1";
			}
			return "!";
		case Keys.D2:
			if (!args.Shift)
			{
				return "2";
			}
			return "@";
		case Keys.D3:
			if (!args.Shift)
			{
				return "3";
			}
			return "#";
		case Keys.D4:
			if (!args.Shift)
			{
				return "4";
			}
			return "$";
		case Keys.D5:
			if (!args.Shift)
			{
				return "5";
			}
			return "%";
		case Keys.D6:
			if (!args.Shift)
			{
				return "6";
			}
			return "^";
		case Keys.D7:
			if (!args.Shift)
			{
				return "7";
			}
			return "&";
		case Keys.D8:
			if (!args.Shift)
			{
				return "8";
			}
			return "*";
		case Keys.D9:
			if (!args.Shift)
			{
				return "9";
			}
			return "(";
		case Keys.OemPlus:
			if (!args.Shift)
			{
				return "=";
			}
			return "+";
		case Keys.OemMinus:
			if (!args.Shift)
			{
				return "-";
			}
			return "_";
		case Keys.OemOpenBrackets:
			if (!args.Shift)
			{
				return "[";
			}
			return "{";
		case Keys.OemCloseBrackets:
			if (!args.Shift)
			{
				return "]";
			}
			return "}";
		case Keys.OemQuestion:
			if (!args.Shift)
			{
				return "/";
			}
			return "?";
		case Keys.OemPeriod:
			if (!args.Shift)
			{
				return ".";
			}
			return ">";
		case Keys.OemComma:
			if (!args.Shift)
			{
				return ",";
			}
			return "<";
		case Keys.OemPipe:
			if (!args.Shift)
			{
				return "\\";
			}
			return "|";
		case Keys.Space:
			return " ";
		case Keys.OemSemicolon:
			if (!args.Shift)
			{
				return ";";
			}
			return ":";
		case Keys.OemQuotes:
			if (!args.Shift)
			{
				return "'";
			}
			return "\"";
		case Keys.OemTilde:
			if (!args.Shift)
			{
				return "`";
			}
			return "~";
		default:
			return "";
		}
	}
}
