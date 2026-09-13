namespace UWGame.SimSide.Entities;

public class RequiresEnergy
{
	public RequiresFuel RequiresFuel;

	public RequiresPower RequiresPower;

	public bool Start()
	{
		if (RequiresFuel != null)
		{
			return RequiresFuel.LightFire();
		}
		return true;
	}

	public bool HasEnergyForDuration(float durationInDays)
	{
		bool flag = true;
		if (RequiresFuel != null)
		{
			flag = RequiresFuel.HasFuelForDuration(durationInDays);
		}
		bool flag2 = true;
		if (RequiresPower != null)
		{
			flag2 = RequiresPower.HasPowerForDuration(durationInDays);
		}
		return flag && flag2;
	}
}
