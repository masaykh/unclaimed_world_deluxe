using System.Collections.Generic;

namespace UWGame.SimSide.Entities;

public class RequiresPower
{
	public bool IsOn;

	public List<Entity> PowerCells;

	public bool HasPowerForDuration(float duration)
	{
		return true;
	}

	public void UpdateSimulationInParallel(double deltaTimeInSeconds)
	{
	}
}
