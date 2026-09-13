using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WindowSystem;

public class ComponentSkin
{
	public Texture2D Skin;

	public List<GUIRect> Rects = new List<GUIRect>();

	public bool UseCustomSkin;

	public bool ModulateColor;

	public Color? EdgeColor;

	public Color? CenterColor;

	public bool FlipHorizontally;
}
