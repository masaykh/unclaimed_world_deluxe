namespace UWGame.SimSide.Entities.Containers;

/// <summary>
/// Implemented by fuel, magazine, and similar containers.
/// Used to access the entity that is currently being replenished.
/// </summary>
internal interface IReplenishes
{
	bool IsReplenishing(EntityID entityID);
}
