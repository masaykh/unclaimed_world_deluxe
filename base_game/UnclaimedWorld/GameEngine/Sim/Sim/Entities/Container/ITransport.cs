using UWGame.SimSide.Entities;

namespace GameEngine.Sim.Sim.Entities.Container;

internal interface ITransport
{
	bool IsPassenger(Entity entity);
}
