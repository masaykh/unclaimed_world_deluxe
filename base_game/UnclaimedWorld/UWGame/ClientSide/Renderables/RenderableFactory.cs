using UWGame.SimSide.AI;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Systems;

namespace UWGame.ClientSide.Renderables;

public class RenderableFactory
{
	public static void Remove(Renderable e)
	{
		The.Client.RemoveRenderable(e);
	}

	public static Renderable Produce(Entity entity, RenderableType type, Renderable.SnapshotRenderable snapshotRenderable = null, double? lifetimeInSeconds = null)
	{
		Renderable renderable = new Renderable(entity, type);
		InitRenderable(lifetimeInSeconds, renderable, snapshotRenderable);
		return renderable;
	}

	public static Renderable Produce(Renderable original, Entity entity)
	{
		Renderable renderable = new Renderable(original, entity);
		InitRenderable(null, renderable);
		return renderable;
	}

	public static Renderable Produce(Renderable original, MemoryFact memoryFact)
	{
		Renderable renderable = new Renderable(original, memoryFact);
		InitRenderable(null, renderable);
		return renderable;
	}

	public static Renderable Produce(Renderable.SnapshotRenderable snapshotRenderable, MemoryFact memoryFact)
	{
		Renderable renderable = new Renderable(memoryFact);
		InitRenderable(null, renderable, snapshotRenderable);
		return renderable;
	}

	private static void InitRenderable(double? lifetimeInSeconds, Renderable renderable, Renderable.SnapshotRenderable snapshotRenderable = null)
	{
		snapshotRenderable?.LoadRenderableWithSnapshotData(renderable);
		if (lifetimeInSeconds.HasValue)
		{
			renderable.ExpiryTimePointInSeconds = UpdateTimePoints.ComputeTimePointFromInterval(lifetimeInSeconds.Value);
		}
		renderable.RecomputeUpdateInterval(out var _);
		The.Client.AddRenderable(renderable);
	}
}
