using Microsoft.Xna.Framework;
using WindowSystem;

namespace UWGame.ClientSide.Interface;

public abstract class DisplayScreen
{
	public Rectangle sourceRectangle;

	public Rectangle destinationRectangle;

	public Window ParentWindow;

	public UIComponent DisplayBox;

	public bool IsDirty;

	protected int contentTextureWidth;

	protected int contentTextureHeight;

	protected int effectTextureWidth;

	protected int effectTextureHeight;

	public DisplayScreen(UIComponent displayBox, Rectangle src, Rectangle dest, int contentTextureWidth, int contentTextureHeight, int effectTextureWidth, int effectTextureHeight, Window window)
	{
		ParentWindow = window;
		if (window != null)
		{
			window.Transitioning += WindowTransitioning;
			window.Move += window_Move;
		}
		DisplayBox = displayBox;
		sourceRectangle = src;
		destinationRectangle = dest;
		this.contentTextureWidth = contentTextureWidth;
		this.contentTextureHeight = contentTextureHeight;
		this.effectTextureWidth = effectTextureWidth;
		this.effectTextureHeight = effectTextureHeight;
	}

	public void SetDimensions(Rectangle dimensions)
	{
		sourceRectangle = dimensions;
		destinationRectangle = dimensions;
		SetupQuadVertices();
	}

	protected virtual void SetupQuadVertices()
	{
		if (DisplayBox != null)
		{
			Rectangle rectangle = sourceRectangle;
			rectangle.Location = DisplayBox.AbsolutePosition;
			rectangle.Height = DisplayBox.Height;
			rectangle.Width = DisplayBox.Width;
			sourceRectangle = rectangle;
			destinationRectangle = sourceRectangle;
		}
		IsDirty = false;
	}

	private void window_Move(UIComponent sender)
	{
		IsDirty = true;
	}

	public void WindowTransitioning(Window sender)
	{
		IsDirty = true;
	}
}
