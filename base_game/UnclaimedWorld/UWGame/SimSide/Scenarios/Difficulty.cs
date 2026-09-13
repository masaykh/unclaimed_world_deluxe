using System.Collections.Generic;
using System.Linq;
using UWGame.SimSide.Entities;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Scenarios;

public class Difficulty
{
	public string KeyName;

	public string Name;

	public string Description;

	public bool IsDefault;

	public SerializableDictionary<string, string[]> OptionsToUse;

	public void PostInitValidate(ScenarioData parent, List<string> listOfErrors)
	{
		if (OptionsToUse == null)
		{
			return;
		}
		foreach (KeyValuePair<string, string[]> item in OptionsToUse)
		{
			OptionSet optionSet = parent.OptionSets.FirstOrDefault((OptionSet o) => o.KeyName == item.Key);
			if (optionSet == null)
			{
				EntityType.CreateValidationError(ref listOfErrors, "OptionsToUse: The option set key: " + item.Key + " was not found in OptionSets.");
				continue;
			}
			string[] value = item.Value;
			foreach (string key in value)
			{
				if (optionSet.Options.FirstOrDefault((Option o) => o.KeyName == key) == null)
				{
					EntityType.CreateValidationError(ref listOfErrors, "The option key: " + key + " was not found in the Options list for OptionSet: " + optionSet.KeyName);
				}
			}
		}
	}
}
