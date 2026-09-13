using Microsoft.Xna.Framework;
using WindowSystem;

namespace UWGame.ClientSide.Interface.LCD;

public class LCDInnerPanel
{
	public Box Panel;

	private Image upperRightDecor;

	private Image lowerRightDecor;

	private int horizontalPadding = 5;

	private int verticalPadding = 4;

	public int HorizontalContentPadding
	{
		get
		{
			return horizontalPadding;
		}
		set
		{
			horizontalPadding = value;
		}
	}

	public int VerticalContentPadding
	{
		get
		{
			return verticalPadding;
		}
		set
		{
			verticalPadding = value;
		}
	}

	public int ContentWidth
	{
		get
		{
			return Panel.Width - 2 * horizontalPadding;
		}
		set
		{
			Panel.Width = value + 2 * horizontalPadding;
		}
	}

	public int Height => Panel.Height;

	public int ContentHeight
	{
		get
		{
			return Panel.Height - verticalPadding;
		}
		set
		{
			Panel.Height = value + verticalPadding;
			if (upperRightDecor != null)
			{
				upperRightDecor.X = Panel.Width - upperRightDecor.Width;
				upperRightDecor.Y = 0;
				lowerRightDecor.X = Panel.Width - lowerRightDecor.Width;
				lowerRightDecor.Y = Panel.Height - lowerRightDecor.Height;
				if (lowerRightDecor.Y < upperRightDecor.Bottom)
				{
					Panel.Remove(lowerRightDecor);
				}
				else
				{
					Panel.Add(lowerRightDecor);
				}
			}
		}
	}

	public LCDInnerPanel(GUIManager gui, int width, bool includeDecor, float? alpha = null, Color? color = null)
	{
		if (alpha.HasValue)
		{
			color = ((!color.HasValue) ? new Color?(new Color(alpha.Value, alpha.Value, alpha.Value, alpha.Value)) : new Color?(new Color(color.Value.R, color.Value.G, color.Value.B, (byte)(alpha.Value * 255f))));
		}
		Panel = new Box(gui);
		Panel.CornerSize = 20;
		Panel.SetSkinLocation(SkinState.Normal, gui.GUISpriteSheet.GetSourceRectangle("lcd_panel_background"), color, color);
		Panel.Width = width;
		Panel.Height = 100;
		if (includeDecor)
		{
			upperRightDecor = new Image(gui);
			upperRightDecor.SetSkinLocation(SkinState.Normal, gui.GUISpriteSheet.GetSourceRectangle("lcd_panel_decor_uppercorner"));
			upperRightDecor.ResizeControlToFitImage();
			upperRightDecor.CanHaveFocus = false;
			Panel.Add(upperRightDecor);
			lowerRightDecor = new Image(gui);
			lowerRightDecor.SetSkinLocation(SkinState.Normal, gui.GUISpriteSheet.GetSourceRectangle("lcd_panel_decor_lowercorner"));
			lowerRightDecor.ResizeControlToFitImage();
			lowerRightDecor.CanHaveFocus = false;
			Panel.Add(lowerRightDecor);
		}
	}

	public void TintPanel(Color color)
	{
		Panel.SetSkinLocation(SkinState.Normal, Panel.guiManager.GUISpriteSheet.GetSourceRectangle("lcd_panel_background"), color, color);
	}

	public void AddContentSetFullWidth(UIComponent content)
	{
		content.X = horizontalPadding;
		content.Width = Panel.Width - 2 * horizontalPadding;
		Panel.Add(content);
	}

	public void RemoveContent(UIComponent content)
	{
		Panel.Remove(content);
	}

	public void AddContent(UIComponent content, int contentX = 0, int contentY = 0)
	{
		Panel.Add(content);
		content.X = horizontalPadding + contentX;
		content.Y = verticalPadding + contentY;
	}
}
