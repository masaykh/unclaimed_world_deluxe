using System.Collections.Generic;
using InputEventSystem;
using Microsoft.Xna.Framework;

namespace WindowSystem;

public class Bar : SkinnedComponent
{
	private static int defaultEdgeSize = 5;

	private int edgeSize;

	private bool isVertical;

	public Rectangle? UnderSprite;

	public int UnderSpriteY;

	public static int DefaultEdgeSize
	{
		set
		{
			defaultEdgeSize = value;
		}
	}

	public bool IsVertical
	{
		get
		{
			return isVertical;
		}
		set
		{
			isVertical = value;
			RefreshSkins();
		}
	}

	public int EdgeSize
	{
		get
		{
			return edgeSize;
		}
		set
		{
			edgeSize = value;
			RefreshSkins();
		}
	}

	public Bar(GUIManager guiManager)
		: base(guiManager)
	{
		isVertical = false;
		base.CanHaveFocus = false;
		EdgeSize = defaultEdgeSize;
	}

	protected override void RefreshSkins()
	{
		foreach (KeyValuePair<int, ComponentSkin> skin in base.Skins)
		{
			skin.Value.Rects = CreateBar(GetSkinLocation(skin.Key), new Rectangle(0, 0, Width, Height), edgeSize, isVertical, UnderSprite, UnderSpriteY, skin.Value.EdgeColor);
		}
		Redraw();
	}

	protected override void OnMouseOut(UIComponent sender, MouseEventArgs args)
	{
		base.OnMouseOut(sender, args);
		if (Enabled)
		{
			base.CurrentSkinState = SkinState.Normal;
		}
	}

	protected override void OnMouseOver(UIComponent sender, MouseEventArgs args)
	{
		base.OnMouseOver(sender, args);
		if (Enabled)
		{
			if (base.IsPressed)
			{
				base.CurrentSkinState = SkinState.CheckedPressed;
			}
			else
			{
				base.CurrentSkinState = SkinState.Hover;
			}
		}
	}

	protected static List<GUIRect> CreateBar(Rectangle source, Rectangle dimensions, int edgeSize, bool isVertical, Rectangle? underSprite, int underSpriteY, Color? color = null)
	{
		List<GUIRect> list = new List<GUIRect>();
		int num = 0;
		GUIRect[] array = ((!underSprite.HasValue) ? new GUIRect[3] : new GUIRect[4]);
		if (isVertical)
		{
			if (underSprite.HasValue)
			{
				array[num] = new GUIRect();
				array[num].Source = new Rectangle(underSprite.Value.X, underSprite.Value.Y, underSprite.Value.Width, dimensions.Height - edgeSize);
				array[num].Destination = new Rectangle(dimensions.X, dimensions.Y + underSpriteY, underSprite.Value.Width, dimensions.Height - edgeSize);
				num++;
			}
			GUIRect gUIRect = new GUIRect();
			gUIRect.Source = new Rectangle(source.X, source.Y, source.Width, edgeSize);
			gUIRect.Destination = new Rectangle(dimensions.X, dimensions.Y, dimensions.Width, edgeSize);
			if (color.HasValue)
			{
				gUIRect.Color = color.Value;
			}
			array[num] = gUIRect;
			gUIRect = new GUIRect();
			gUIRect.Source = new Rectangle(source.X, source.Y + edgeSize, source.Width, source.Height - 2 * edgeSize);
			gUIRect.Destination = new Rectangle(dimensions.X, dimensions.Y + edgeSize, dimensions.Width, dimensions.Height - 2 * edgeSize);
			if (color.HasValue)
			{
				gUIRect.Color = color.Value;
			}
			array[num + 1] = gUIRect;
			gUIRect = new GUIRect();
			gUIRect.Source = new Rectangle(source.X, source.Bottom - edgeSize, source.Width, edgeSize);
			gUIRect.Destination = new Rectangle(dimensions.X, dimensions.Y + dimensions.Height - edgeSize, dimensions.Width, edgeSize);
			if (color.HasValue)
			{
				gUIRect.Color = color.Value;
			}
			array[num + 2] = gUIRect;
		}
		else
		{
			GUIRect gUIRect = new GUIRect();
			gUIRect.Source = new Rectangle(source.X, source.Y, edgeSize, source.Height);
			gUIRect.Destination = new Rectangle(dimensions.X, dimensions.Y, edgeSize, dimensions.Height);
			if (color.HasValue)
			{
				gUIRect.Color = color.Value;
			}
			array[num] = gUIRect;
			int width = source.Width - 2 * edgeSize;
			gUIRect = new GUIRect();
			gUIRect.Source = new Rectangle(source.X + edgeSize, source.Y, width, source.Height);
			gUIRect.Destination = new Rectangle(dimensions.X + edgeSize, dimensions.Y, dimensions.Width - 2 * edgeSize, dimensions.Height);
			if (color.HasValue)
			{
				gUIRect.Color = color.Value;
			}
			array[num + 1] = gUIRect;
			gUIRect = new GUIRect();
			gUIRect.Source = new Rectangle(source.Right - edgeSize, source.Y, edgeSize, source.Height);
			gUIRect.Destination = new Rectangle(dimensions.Right - edgeSize, dimensions.Y, edgeSize, dimensions.Height);
			if (color.HasValue)
			{
				gUIRect.Color = color.Value;
			}
			array[num + 2] = gUIRect;
		}
		for (int i = 0; i < array.Length; i++)
		{
			list.Add(array[i]);
		}
		return list;
	}
}
