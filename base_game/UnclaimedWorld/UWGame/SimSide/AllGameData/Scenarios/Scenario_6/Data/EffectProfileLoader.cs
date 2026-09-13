using System.Collections.Generic;
using UWGame.SimSide.SimEffects;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_6.Data;

public class EffectProfileLoader
{
	public static List<EffectProfileType> Init()
	{
		List<EffectProfileType> list = new List<EffectProfileType>();
		list.Add(new EffectProfileType
		{
			KeyName = "contract",
			Name = "Contract",
			Description = "The character will not leave the site while under contract.",
			Effects = new string[1] { "contract" }
		});
		return list;
	}
}
