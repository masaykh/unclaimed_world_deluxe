namespace UWGame.SimSide.Entities.Containers;

public interface IHoldsProductionOutput
{
	float? TotalOutputCapacity { get; }

	float? TotalStoredOutput { get; }

	void UncontainAllProductionOutput();

	bool HasCapacityForOutput(Entity item);
}
