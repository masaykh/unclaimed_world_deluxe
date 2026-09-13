using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Entities.Substances;

public class SubstancesType
{
	public SerializableDictionary<string, float> SubstanceFractions;

	[XmlIgnore]
	public Dictionary<SubstanceType, float> FinalSubstanceFractions;

	public void Initialize()
	{
		FinalSubstanceFractions = new Dictionary<SubstanceType, float>();
		float num = SubstanceFractions.Sum((KeyValuePair<string, float> s) => s.Value);
		float num2 = 1f;
		if (num > 1f)
		{
			num2 = 1f / num;
		}
		foreach (KeyValuePair<string, float> substanceFraction in SubstanceFractions)
		{
			float value = num2 * substanceFraction.Value;
			FinalSubstanceFractions.Add(GameData.Instance.AllSubstanceTypes[substanceFraction.Key], value);
		}
	}
}
