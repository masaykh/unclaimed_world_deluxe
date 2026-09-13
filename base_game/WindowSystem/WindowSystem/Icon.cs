using System.Collections.Generic;
using System.Text;
using InputEventSystem;
using Microsoft.Xna.Framework;

namespace WindowSystem;

public class Icon : SkinnedComponent
{
	public enum UIType
	{
		HUD,
		LCD
	}

	private bool scaleImageToSizeOfControl;

	private float alpha = 1f;

	private Color? color;

	public bool ScaleImageToSizeOfControl
	{
		get
		{
			return scaleImageToSizeOfControl;
		}
		set
		{
			scaleImageToSizeOfControl = value;
			RefreshSkins();
		}
	}

	public float Alpha
	{
		get
		{
			return alpha;
		}
		set
		{
			alpha = value;
			RefreshSkins();
		}
	}

	public Color? Color
	{
		get
		{
			return color;
		}
		set
		{
			color = value;
			RefreshSkins();
		}
	}

	public Icon(GUIManager guiManager)
		: base(guiManager)
	{
		scaleImageToSizeOfControl = false;
	}

	protected override void OnMouseOver(UIComponent sender, MouseEventArgs args)
	{
		if (SetMouseOverState())
		{
			base.OnMouseOver(sender, args);
		}
	}

	public bool SetMouseOverState()
	{
		if (!Enabled && string.IsNullOrEmpty(ToolTip))
		{
			return false;
		}
		if (string.IsNullOrEmpty(ToolTip))
		{
			return false;
		}
		if (!Enabled)
		{
			base.CurrentSkinState = SkinState.HoverDisabled;
		}
		else
		{
			base.CurrentSkinState = SkinState.Hover;
		}
		return true;
	}

	protected override void OnMouseOut(UIComponent sender, MouseEventArgs args)
	{
		if (SetMouseOutState())
		{
			base.OnMouseOut(sender, args);
		}
	}

	public bool SetMouseOutState()
	{
		if (!Enabled && string.IsNullOrEmpty(ToolTip))
		{
			return false;
		}
		if (!Enabled)
		{
			base.CurrentSkinState = SkinState.Disabled;
		}
		else
		{
			base.CurrentSkinState = SkinState.Normal;
		}
		return true;
	}

	public void SetSkinLocations(Rectangle location, UIType uiType)
	{
		Color value = ((uiType != UIType.HUD) ? UIComponent.lcdHoverTint : Microsoft.Xna.Framework.Color.Gray);
		SetSkinLocation(SkinState.Normal, location);
		SetSkinLocation(SkinState.Hover, location, value, value, flipHorizontally: false, modulateColor: true);
	}

	public void SetSkinLocations(Rectangle location, Color? normalColor = null, Color? hoverColor = null)
	{
		SetSkinLocation(SkinState.Normal, location, normalColor, normalColor);
		SetSkinLocation(SkinState.Hover, location, hoverColor, hoverColor);
	}

	public override void SetSkinLocation(int index, Rectangle? location, Color? edgeColor = null, Color? centerColor = null, bool flipHorizontally = false, bool modulateColor = false)
	{
		base.SetSkinLocation(index, location, edgeColor, centerColor, flipHorizontally, modulateColor);
		if (index == 0 && !base.Skins.ContainsKey(1))
		{
			SetSkinLocation(SkinState.Hover, location, Microsoft.Xna.Framework.Color.Gray, Microsoft.Xna.Framework.Color.Gray, flipHorizontally, modulateColor: true);
		}
	}

	public void ResizeControlToFitImage()
	{
		if (scaleImageToSizeOfControl)
		{
			return;
		}
		int num = base.CurrentSkin;
		if (num != -1)
		{
			Rectangle skinLocation = GetSkinLocation(num);
			if (skinLocation.Width > 0 && skinLocation.Height > 0)
			{
				Width = skinLocation.Width;
				Height = skinLocation.Height;
			}
		}
	}

	protected override void RefreshSkins()
	{
		foreach (KeyValuePair<int, ComponentSkin> skin in base.Skins)
		{
			GUIRect gUIRect = new GUIRect();
			gUIRect.Source = GetSkinLocation(skin.Key);
			gUIRect.Alpha = Alpha;
			gUIRect.FlipHorizontally = skin.Value.FlipHorizontally;
			if (skin.Value.ModulateColor)
			{
				Color? color = skin.Value.CenterColor ?? skin.Value.EdgeColor;
				if (color.HasValue && Color.HasValue)
				{
					gUIRect.Color = new Color(color.Value.ToVector4() * Color.Value.ToVector4());
				}
				else
				{
					gUIRect.Color = Color ?? skin.Value.CenterColor ?? skin.Value.EdgeColor ?? Microsoft.Xna.Framework.Color.White;
				}
			}
			else
			{
				gUIRect.Color = Color ?? skin.Value.CenterColor ?? skin.Value.EdgeColor ?? Microsoft.Xna.Framework.Color.White;
			}
			if (scaleImageToSizeOfControl)
			{
				if (base.Rotate90Degrees)
				{
					gUIRect.Destination = new Rectangle(0, 0, Height, Width);
				}
				else
				{
					gUIRect.Destination = new Rectangle(0, 0, Width, Height);
				}
			}
			else if (base.Rotate90Degrees)
			{
				gUIRect.Destination = new Rectangle(0, 0, gUIRect.Source.Height, gUIRect.Source.Width);
			}
			else
			{
				gUIRect.Destination = new Rectangle(0, 0, gUIRect.Source.Width, gUIRect.Source.Height);
			}
			skin.Value.Rects.Clear();
			skin.Value.Rects.Add(gUIRect);
		}
		Redraw();
	}

	public static string ToIcon(string sprite, string color)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("§I");
		stringBuilder.Append(color);
		stringBuilder.Append("¤");
		stringBuilder.Append(sprite);
		stringBuilder.Append("§");
		return stringBuilder.ToString();
	}
}
