using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Items;

public class StorageDurationToDisplay
{
	public string DisplayName;

	public string Tooltip;

	public string StorageCondition;

	public bool IsStorageOfWeatherProofPart;

	public bool DisplayAlways;

	public bool DisplayForStructure;

	public bool DisplayForFoodOnly;

	public bool SortAtTop;

	public bool DisplayThis(EntityType entityType)
	{
		if (DisplayAlways)
		{
			return true;
		}
		if (entityType.StructureType != null && !DisplayForStructure)
		{
			return false;
		}
		if ((entityType.ItemType == null || entityType.ItemType.FoodType == null) && DisplayForFoodOnly)
		{
			return false;
		}
		if (!entityType.NonLivingType.CanBeAPart && IsStorageOfWeatherProofPart)
		{
			return false;
		}
		return true;
	}
}
