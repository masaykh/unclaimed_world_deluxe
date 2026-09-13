using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WindowSystem;

public class SkinnedComponent : UIComponent
{
	private Dictionary<int, Rectangle> locations;

	private Dictionary<int, ComponentSkin> skins;

	private int currentSkin;

	private bool rotate90Degrees;

	public Vector2 Origin = Vector2.Zero;

	protected Dictionary<int, ComponentSkin> Skins => skins;

	public int CurrentSkin
	{
		get
		{
			return currentSkin;
		}
		set
		{
			SetActiveSkin(value);
		}
	}

	public bool Rotate90Degrees
	{
		get
		{
			return rotate90Degrees;
		}
		set
		{
			rotate90Degrees = value;
			RefreshSkins();
		}
	}

	public SkinState CurrentSkinState
	{
		get
		{
			return (SkinState)currentSkin;
		}
		set
		{
			_ = DebugTag == "cbStockpile";
			SetActiveSkin((int)value);
		}
	}

	public SkinnedComponent(GUIManager guiManager)
		: base(guiManager)
	{
		locations = new Dictionary<int, Rectangle>();
		skins = new Dictionary<int, ComponentSkin>();
		currentSkin = -1;
	}

	protected Rectangle GetSkinLocation(int index)
	{
		if (locations.ContainsKey(index))
		{
			return locations[index];
		}
		return Rectangle.Empty;
	}

	public virtual void SetSkinLocation(SkinState skinState, Rectangle? location, Color? edgeColor = null, Color? centerColor = null, bool flipHorizontally = false, bool modulateColor = false)
	{
		SetSkinLocation((int)skinState, location, edgeColor, centerColor, flipHorizontally, modulateColor);
	}

	public virtual void SetSkinLocation(int index, Rectangle? location, Color? edgeColor = null, Color? centerColor = null, bool flipHorizontally = false, bool modulateColor = false)
	{
		_ = location.HasValue;
		if (locations.ContainsKey(index))
		{
			if (location.HasValue)
			{
				locations[index] = location.Value;
			}
			if (skins.TryGetValue(index, out var value))
			{
				value.CenterColor = centerColor;
				value.EdgeColor = edgeColor;
				value.FlipHorizontally = flipHorizontally;
				value.ModulateColor = modulateColor;
			}
			RefreshSkins();
		}
		else if (location.HasValue)
		{
			locations.Add(index, location.Value);
			skins.Add(index, new ComponentSkin
			{
				EdgeColor = edgeColor,
				CenterColor = centerColor,
				FlipHorizontally = flipHorizontally,
				ModulateColor = modulateColor
			});
			RefreshSkins();
			if (currentSkin == -1)
			{
				SetActiveSkin(index);
			}
		}
	}

	protected ComponentSkin GetSkin(int index)
	{
		if (skins.ContainsKey(index))
		{
			return skins[index];
		}
		return null;
	}

	private void SetActiveSkin(int index)
	{
		if (index != currentSkin)
		{
			if (DebugTag == "cbAttackVermin")
			{
				_ = 7;
			}
			ComponentSkin componentSkin = null;
			if (index != -1)
			{
				componentSkin = GetSkin(index);
			}
			if (componentSkin != null || index == -1)
			{
				currentSkin = index;
				Redraw();
			}
		}
	}

	protected virtual void RefreshSkins()
	{
	}

	protected override void DrawControl(SpriteBatch spriteBatch, Rectangle parentScissor, float alpha)
	{
		ComponentSkin skin = GetSkin(currentSkin);
		if (skin == null)
		{
			return;
		}
		Texture2D texture = ((!skin.UseCustomSkin) ? base.GUIManager.SkinTexture : skin.Skin);
		foreach (GUIRect rect in skin.Rects)
		{
			bool flag = true;
			Rectangle source = rect.Source;
			Rectangle destination = rect.Destination;
			destination.X += base.AbsolutePosition.X;
			destination.Y += base.AbsolutePosition.Y;
			if (ClipThis && !parentScissor.Contains(destination))
			{
				if (parentScissor.Intersects(destination))
				{
					if (destination.X < parentScissor.X)
					{
						int num = parentScissor.X - destination.X;
						if (destination.Width == source.Width)
						{
							source.Width -= num;
							source.X += num;
							destination.Width -= num;
							destination.X += num;
						}
						else
						{
							destination.Width -= num;
							destination.X += num;
						}
					}
					else if (destination.Right > parentScissor.Right)
					{
						int num = destination.Right - parentScissor.Right;
						if (destination.Width == source.Width)
						{
							source.Width -= num;
							destination.Width -= num;
						}
						else
						{
							destination.Width -= num;
						}
					}
					if (destination.Y < parentScissor.Y)
					{
						int num = parentScissor.Y - destination.Y;
						if (destination.Height == source.Height)
						{
							source.Height -= num;
							source.Y += num;
							destination.Height -= num;
							destination.Y += num;
						}
						else
						{
							int num2 = (int)((float)num / (float)destination.Height * (float)source.Height);
							source.Y += num2;
							source.Height -= num2;
							destination.Y += num;
							destination.Height -= num;
						}
					}
					if (destination.Bottom > parentScissor.Bottom)
					{
						int num = destination.Bottom - parentScissor.Bottom;
						if (destination.Height == source.Height)
						{
							source.Height -= num;
							destination.Height -= num;
						}
						else
						{
							float num3 = (float)num / (float)destination.Height;
							source.Height = (int)((float)source.Height - num3 * (float)source.Height);
							destination.Height -= num;
						}
					}
				}
				else
				{
					flag = false;
				}
			}
			if (flag)
			{
				if (rect.Alpha == 1f && alpha == 1f)
				{
					spriteBatch.Draw(texture, destination, source, rect.Color, Rotate90Degrees ? ((float)Math.PI / 2f) : 0f, Origin, rect.FlipHorizontally ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
					continue;
				}
				Color color = rect.Color;
				color *= alpha * rect.Alpha;
				spriteBatch.Draw(texture, destination, source, color, Rotate90Degrees ? ((float)Math.PI / 2f) : 0f, Origin, rect.FlipHorizontally ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
			}
		}
	}

	protected override void OnResize(UIComponent sender)
	{
		base.OnResize(sender);
		RefreshSkins();
	}
}
