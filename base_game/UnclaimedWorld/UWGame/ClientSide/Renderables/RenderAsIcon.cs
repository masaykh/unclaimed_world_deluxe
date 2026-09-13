using Microsoft.Xna.Framework;
using UWGame.SimSide.Resources;

namespace UWGame.ClientSide.Renderables;

public class RenderAsIcon
{
	private Color color = Color.White;

	public IconToRender IconToRender = IconToRender.Hook;

	public IconToRender GetIconToRender()
	{
		return IconToRender;
	}
}
