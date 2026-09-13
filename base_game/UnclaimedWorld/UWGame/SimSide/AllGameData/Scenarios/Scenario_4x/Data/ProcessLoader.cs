using System.Collections.Generic;
using UWGame.SimSide.Processes;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_4x.Data;

public class ProcessLoader
{
	public static List<ProcessType> Init()
	{
		return new List<ProcessType>
		{
			new ProcessType
			{
				KeyName = "makeSulfurSmokeBomb",
				DeleteRecord = true
			}
		};
	}
}
