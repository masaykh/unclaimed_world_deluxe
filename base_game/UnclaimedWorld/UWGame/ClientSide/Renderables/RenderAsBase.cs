using UWGame.SimSide.AI;
using UWGame.SimSide.Entities;

namespace UWGame.ClientSide.Renderables;

public class RenderAsBase
{
	public Renderable Renderable;

	public IKnownEntityData Parent { get; set; }

	public Entity ParentEntity { get; set; }

	public RenderAsBase(Entity parent, Renderable renderable)
	{
		Parent = parent;
		ParentEntity = parent;
		Renderable = renderable;
	}
}
