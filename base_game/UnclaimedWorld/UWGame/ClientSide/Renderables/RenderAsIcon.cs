using Microsoft.Xna.Framework;
using UWGame.SimSide.Resources;

namespace UWGame.ClientSide.Renderables;

public class RenderAsIcon
{
	public IconToRender IconToRender = IconToRender.Hook;

	public IconToRender GetIconToRender()
	{
		return IconToRender;
	}
}
