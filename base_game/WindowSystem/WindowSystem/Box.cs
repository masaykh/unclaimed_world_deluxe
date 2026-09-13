using System.Collections.Generic;
using InputEventSystem;
using Microsoft.Xna.Framework;

namespace WindowSystem;

public class Box : SkinnedComponent
{
	protected static int defaultCornerSize = 5;

	private int cornerSize;

	public static int DefaultCornerSize
	{
		set
		{
			defaultCornerSize = value;
		}
	}

	public int CornerSize
	{
		get
		{
			return cornerSize;
		}
		set
		{
			cornerSize = value;
			RefreshSkins();
		}
	}

	public Color NormalColor
	{
		set
		{
			SetSkinLocation(SkinState.Normal, null, value, value);
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
			base.Enabled = value;
			if (!value)
			{
				base.CurrentSkinState = SkinState.Disabled;
			}
			else
			{
				base.CurrentSkinState = SkinState.Normal;
			}
		}
	}

	public Box(GUIManager guiManager)
		: base(guiManager)
	{
		base.CanHaveFocus = false;
		CornerSize = defaultCornerSize;
	}

	protected override void RefreshSkins()
	{
		foreach (KeyValuePair<int, ComponentSkin> skin in base.Skins)
		{
			CreateBox(skin.Value.Rects, GetSkinLocation(skin.Key), new Rectangle(0, 0, Width, Height), cornerSize, skin.Value.EdgeColor, skin.Value.CenterColor);
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

	public static void CreateBox(List<GUIRect> rects, Rectangle source, Rectangle dimensions, int cornerSize, Color? edgeColor = null, Color? centerColor = null)
	{
		rects.Clear();
		GUIRect gUIRect = new GUIRect();
		gUIRect.Source = new Rectangle(source.X, source.Y, cornerSize, cornerSize);
		gUIRect.Destination = new Rectangle(dimensions.X, dimensions.Y, cornerSize, cornerSize);
		if (edgeColor.HasValue)
		{
			gUIRect.Color = edgeColor.Value;
		}
		rects.Add(gUIRect);
		gUIRect = new GUIRect();
		gUIRect.Source = new Rectangle(source.X, source.Bottom - cornerSize, cornerSize, cornerSize);
		gUIRect.Destination = new Rectangle(dimensions.X, dimensions.Bottom - cornerSize, cornerSize, cornerSize);
		if (edgeColor.HasValue)
		{
			gUIRect.Color = edgeColor.Value;
		}
		rects.Add(gUIRect);
		gUIRect = new GUIRect();
		gUIRect.Source = new Rectangle(source.Right - cornerSize, source.Bottom - cornerSize, cornerSize, cornerSize);
		gUIRect.Destination = new Rectangle(dimensions.Right - cornerSize, dimensions.Bottom - cornerSize, cornerSize, cornerSize);
		if (edgeColor.HasValue)
		{
			gUIRect.Color = edgeColor.Value;
		}
		rects.Add(gUIRect);
		gUIRect = new GUIRect();
		gUIRect.Source = new Rectangle(source.Right - cornerSize, source.Y, cornerSize, cornerSize);
		gUIRect.Destination = new Rectangle(dimensions.Right - cornerSize, dimensions.Y, cornerSize, cornerSize);
		if (edgeColor.HasValue)
		{
			gUIRect.Color = edgeColor.Value;
		}
		rects.Add(gUIRect);
		gUIRect = new GUIRect();
		gUIRect.Source = new Rectangle(source.X + cornerSize, source.Y, source.Width - 2 * cornerSize, cornerSize);
		gUIRect.Destination = new Rectangle(dimensions.X + cornerSize, dimensions.Y, dimensions.Width - 2 * cornerSize, cornerSize);
		if (edgeColor.HasValue)
		{
			gUIRect.Color = edgeColor.Value;
		}
		rects.Add(gUIRect);
		gUIRect = new GUIRect();
		gUIRect.Source = new Rectangle(source.X + cornerSize, source.Bottom - cornerSize, source.Width - 2 * cornerSize, cornerSize);
		gUIRect.Destination = new Rectangle(dimensions.X + cornerSize, dimensions.Bottom - cornerSize, dimensions.Width - 2 * cornerSize, cornerSize);
		if (edgeColor.HasValue)
		{
			gUIRect.Color = edgeColor.Value;
		}
		rects.Add(gUIRect);
		gUIRect = new GUIRect();
		gUIRect.Source = new Rectangle(source.X, source.Y + cornerSize + 1, cornerSize, source.Height - 2 * cornerSize - 1);
		gUIRect.Destination = new Rectangle(dimensions.X, dimensions.Y + cornerSize, cornerSize, dimensions.Height - 2 * cornerSize);
		if (edgeColor.HasValue)
		{
			gUIRect.Color = edgeColor.Value;
		}
		rects.Add(gUIRect);
		gUIRect = new GUIRect();
		gUIRect.Source = new Rectangle(source.Right - cornerSize, source.Y + cornerSize + 1, cornerSize, source.Height - 2 * cornerSize - 1);
		gUIRect.Destination = new Rectangle(dimensions.Right - cornerSize, dimensions.Y + cornerSize, cornerSize, dimensions.Height - 2 * cornerSize);
		if (edgeColor.HasValue)
		{
			gUIRect.Color = edgeColor.Value;
		}
		rects.Add(gUIRect);
		gUIRect = new GUIRect();
		gUIRect.Source = new Rectangle(source.X + cornerSize + 1, source.Y + cornerSize + 1, source.Width - 2 * (cornerSize + 1), source.Height - 2 * (cornerSize + 1));
		gUIRect.Destination = new Rectangle(dimensions.X + cornerSize, dimensions.Y + cornerSize, dimensions.Width - 2 * cornerSize, dimensions.Height - 2 * cornerSize);
		if (centerColor.HasValue)
		{
			gUIRect.Color = centerColor.Value;
		}
		rects.Add(gUIRect);
	}
}
