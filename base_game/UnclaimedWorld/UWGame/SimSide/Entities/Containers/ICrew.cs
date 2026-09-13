namespace UWGame.SimSide.Entities.Containers;

internal interface ICrew
{
	EntityID? Driver { get; set; }

	bool IsDriver(Entity entity);
}
