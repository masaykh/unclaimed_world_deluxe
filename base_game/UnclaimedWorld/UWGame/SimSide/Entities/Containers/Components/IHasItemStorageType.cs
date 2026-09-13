namespace UWGame.SimSide.Entities.Containers.Components;

internal interface IHasItemStorageType
{
	ItemStorageType ItemStorageType { get; }

	bool AllowsStockpiling { get; }
}
