namespace UWGame.SimSide.Processes;

public class OutputAmount
{
	public int? NoOfItems;

	public Bulk Bulk;

	public string AmountToString()
	{
		if (NoOfItems.HasValue)
		{
			return NoOfItems.Value.ToString();
		}
		return "1";
	}

	public bool ShouldSerializeNoOfItems()
	{
		return NoOfItems.HasValue;
	}
}
