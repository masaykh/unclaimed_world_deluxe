namespace UWGame.SimSide.Entities.Containers;

public interface ITerminal
{
	float? TotalOutputCapacity { get; }

	float? TotalStoredOutput { get; }

	void UncontainAllProductionOutput();

	bool HasCapacityForOutput(Entity item);
}
