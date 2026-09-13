using UWGame.SimSide.Entities.Containers;

namespace UWGame.SimSide.Items;

public class StorageDuration
{
	public StorageCondition StorageCondition;

	public DegradeType DegradeType;

	public StorageDurationToDisplay StorageDurationToDisplay;

	public float Duration;

	public float SortOrder;

	public void ComputeSortOrder()
	{
		if (StorageDurationToDisplay.DisplayAlways)
		{
			SortOrder = float.MaxValue;
		}
		else if (StorageDurationToDisplay.SortAtTop)
		{
			SortOrder = 10000f * Duration;
		}
		else
		{
			SortOrder = Duration;
		}
	}
}
