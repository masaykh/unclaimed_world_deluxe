using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Buildings;

public class RenderAsConnectedGroundSprite : RenderAsBase
{
	public bool IsRenderedAsConnection;

	public RenderAsConnectedGroundSprite(Entity parent, Renderable renderable)
		: base(parent, renderable)
	{
	}

	public RenderAsConnectedGroundSprite(RenderAsConnectedGroundSprite original, Renderable renderable)
		: base(null, renderable)
	{
	}
}
