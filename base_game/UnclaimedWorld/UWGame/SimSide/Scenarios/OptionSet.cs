using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace UWGame.SimSide.Scenarios;

public class OptionSet
{
	public string KeyName;

	public string Name;

	public string Description;

	public int DisplayGroup;

	public Option[] Options;

	[XmlIgnore]
	public List<IGrouping<string, Option>> OptionsGroupedByDifficulty;

	public void Initialize()
	{
		IEnumerable<IGrouping<string, Option>> source = from o in Options
			group o by o.Difficulty.KeyName;
		OptionsGroupedByDifficulty = source.ToList();
		Option[] options = Options;
		for (int num = 0; num < options.Length; num++)
		{
			options[num].Initialize();
		}
	}
}
