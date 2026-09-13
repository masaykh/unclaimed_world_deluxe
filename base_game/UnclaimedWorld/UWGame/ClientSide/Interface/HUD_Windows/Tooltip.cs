using InputEventSystem;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Interface.Controls;
using WindowSystem;

namespace UWGame.ClientSide.Interface.HUD_Windows;

public class Tooltip : HUDWindow
{
	private TextArea area;

	public Window SpawningWindow;

	private const int width = 200;

	private const int height = 80;

	private double timePassed;

	private const double timeBeforeAppearing = 0.4;

	private const double timeBeforeDisappearing = 7.0;

	private int screenPosX;

	private int screenPosY;

	private UIComponent tooltipAnchor;

	public string Text
	{
		set
		{
			area.Text = value;
			DisplayWindow.Width = area.Width + 18;
			DisplayWindow.Height = area.Height + 9;
			DisplayWindow.CenterChildVertically(area);
		}
	}

	public int Width
	{
		get
		{
			return DisplayWindow.Width;
		}
		set
		{
			DisplayWindow.Width = value;
			SetAreaWidth();
		}
	}

	public Tooltip(CommonInterface intf)
		: base(200, 80, hasSurface: true, hasCloseButton: false, isMovable: false, "HUD_windowHint_base", hideWhenMouseExits: false, Level.RockBottom, intf)
	{
		UpdateWhileHidden = true;
		DisplayWindow.CanHaveFocus = false;
		area = new TextArea(gui, ListBoxType.HUDAndLCD);
		area.Font = GUIManager.LCDandHUDFont;
		area.Color = Color.Black;
		Add(area);
		area.X = 9;
		area.Y = 9;
		SetAreaWidth();
		area.Height = DisplayWindow.Height - 12;
		area.ZOrder = 1f;
		DisplayWindow.Level = Level.Tooltip;
		area.CanHaveFocus = false;
		gui.ShowTooltip += gui_ShowTooltip;
		gui.HideTooltip += gui_HideTooltip;
		gui.WindowClosed += gui_WindowClosed;
	}

	private void SetAreaWidth()
	{
		area.Width = DisplayWindow.Width - 12;
	}

	private void gui_ShowTooltip(UIComponent sender)
	{
		StartCountdownToShow(sender);
	}

	private void gui_WindowClosed(Window sender)
	{
		if (SpawningWindow == sender)
		{
			Hide();
		}
	}

	private void gui_HideTooltip(UIComponent sender)
	{
		if (sender.ToolTip != null)
		{
			Hide();
		}
	}

	public void Destroy()
	{
		gui.ShowTooltip -= gui_ShowTooltip;
		gui.HideTooltip -= gui_HideTooltip;
	}

	public override void Update(GameTime elapsed)
	{
		base.Update(elapsed);
		HandleUpdate(elapsed, ref timePassed, tooltipAnchor, this);
	}

	public override void Refresh()
	{
		base.Refresh();
		if (DisplayWindow.IsVisibleAndActive)
		{
			Text = tooltipAnchor.ToolTip;
		}
	}

	public static void HandleUpdate(GameTime elapsed, ref double timePassed, UIComponent tooltipAnchor, HUDWindow tooltip)
	{
		if (tooltipAnchor == null)
		{
			return;
		}
		timePassed += elapsed.ElapsedGameTime.TotalSeconds;
		InputData inputData = tooltipAnchor.guiManager.InputData;
		if (!tooltip.DisplayWindow.IsVisibleAndActive && timePassed > 0.4)
		{
			if (tooltipAnchor == null)
			{
				return;
			}
			if (tooltipAnchor.CheckCoordinates(inputData.mouseX, inputData.mouseY))
			{
				if (tooltip is Tooltip standardTooltip)
				{
					ShowTooltip(inputData, tooltipAnchor, standardTooltip);
					return;
				}
				DataSheet entityTypeTooltip = tooltip as DataSheet;
				ShowDataTypeTooltip(tooltipAnchor, entityTypeTooltip);
			}
			else
			{
				tooltip.Hide();
			}
		}
		else if (tooltip is Tooltip tooltip2 && tooltip2.tooltipAnchor.TooltipExpires && tooltip.DisplayWindow.IsVisibleAndActive && timePassed > 7.0)
		{
			tooltip.Hide();
		}
	}

	private static void ShowTooltip(InputData inputData, UIComponent tooltipAnchor, Tooltip standardTooltip)
	{
		tooltipAnchor.NotifyTooltipShown();
		standardTooltip.Width = tooltipAnchor.TooltipWidth;
		standardTooltip.Text = tooltipAnchor.ToolTip;
		int x = inputData.mouseX + 24;
		int y = inputData.mouseY + 24;
		Rectangle value = new Rectangle(x, y, standardTooltip.DisplayWindow.Width, standardTooltip.DisplayWindow.Height);
		if (!new Rectangle(0, 0, standardTooltip.gui.ScreenWidth, standardTooltip.gui.ScreenHeight).Contains(value))
		{
			standardTooltip.Interface.SelectAnchorPoint(tooltipAnchor, standardTooltip.DisplayWindow, null, standardTooltip.DisplayWindow.Height, doOverlap: false, 0, out x, out y, -4);
		}
		standardTooltip.ShowInScreenSpace(x, y);
	}

	private static void ShowDataTypeTooltip(UIComponent tooltipAnchor, DataSheet entityTypeTooltip)
	{
		DataTypeButton dataTypeButton = tooltipAnchor as DataTypeButton;
		if (The.InGameUI.GetAnySpawnedTooltip(dataTypeButton) != null)
		{
			entityTypeTooltip.Hide();
			return;
		}
		if (dataTypeButton.IsRoot)
		{
			The.InGameUI.CloseAllEntityTooltips();
		}
		DataTypeButtonEventArgs e = tooltipAnchor.EventArgs as DataTypeButtonEventArgs;
		The.InGameUI.SelectAnchorPoint(tooltipAnchor, entityTypeTooltip.DisplayWindow, dataTypeButton.SideToAnchorOn, 320, doOverlap: true, DataSheet.GetAnchorPointYOffset(), out var x, out var y);
		if (e != null)
		{
			entityTypeTooltip.InitAndShow(tooltipAnchor, e.Owner, e.UseUIOwner, x, y);
		}
	}

	public bool IsShowingTooltipForComponent(UIComponent component)
	{
		if (tooltipAnchor == component)
		{
			return true;
		}
		return false;
	}

	public void StartCountdownToShow(UIComponent sender)
	{
		tooltipAnchor = sender;
		UIComponent uIComponent = sender.FindParentOfType(typeof(Window));
		if (uIComponent != null)
		{
			SpawningWindow = (Window)uIComponent;
		}
		timePassed = 0.0;
	}

	public override void Hide()
	{
		base.Hide();
		if (tooltipAnchor != null)
		{
			tooltipAnchor.NotifyTooltipHidden();
			tooltipAnchor = null;
		}
		timePassed = 0.0;
	}
}
