using System.Xml.Serialization;

namespace UWGame.SimSide.Entities;

public class DefaultCrops
{
	public string KeyName;

	public int? MinItemsForFullGrownPlant;

	public int? MaxItemsForFullGrownPlant;

	[XmlIgnore]
	public float? AbsoluteMeanItems;

	[XmlIgnore]
	public float? AbsoluteStandardDeviationItems;

	public void Initialize()
	{
		if (MinItemsForFullGrownPlant.HasValue && MaxItemsForFullGrownPlant.HasValue)
		{
			Common.GetNormalDistributionFromMinMaxValues(MinItemsForFullGrownPlant.Value, MaxItemsForFullGrownPlant.Value, out AbsoluteMeanItems, out AbsoluteStandardDeviationItems);
		}
	}
}
