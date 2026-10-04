namespace UWGame.SimSide.Entities.Containers;

/// <summary>
/// Crew for a vehicle or similar container.
/// </summary>
internal interface ICrew
{
	EntityID? Driver { get; set; }

	bool IsDriver(Entity entity);
}
