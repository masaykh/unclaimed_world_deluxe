using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Jobs;

public class StokeFireJob : Job
{
	public float ManSecondsOfWorkNeeded;

	public Entity FireSite;

	public float Progress;

	public StokeFireJob()
	{
	}

	public StokeFireJob(Entity fireSite, EntityGroup entityGroup)
		: base(entityGroup)
	{
		FireSite = fireSite;
		ComputeJobType();
		SetDefaultPriority(entityGroup);
	}

	public bool IsInProgress()
	{
		return Progress > 0f;
	}

	public override Vector3? GetCircaLocation()
	{
		return null;
	}
}
