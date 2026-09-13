using System.Collections.Generic;
using System.Xml.Serialization;
using UWGame.SimSide.Entities.Substances;

namespace UWGame.SimSide.Processes;

public class InputAmount
{
	public int? NoOfItems;

	public string[] Substances;

	[XmlIgnore]
	public List<SubstanceType> SubstanceTypes;

	public void Initialize()
	{
		if (Substances != null)
		{
			SubstanceTypes = new List<SubstanceType>();
			string[] substances = Substances;
			foreach (string key in substances)
			{
				SubstanceTypes.Add(GameData.Instance.AllSubstanceTypes[key]);
			}
		}
	}

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
