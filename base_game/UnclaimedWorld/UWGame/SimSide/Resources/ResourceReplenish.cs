using System;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems;

namespace UWGame.SimSide.Resources;

public class ResourceReplenish : ISnapshot
{
	private ushort maxResourceItemsEverSet;

	private double? replenishTimePointInSeconds;

	private ushort? indexOfLastReplenishPoint;

	private Snapshotter.Version version;

	public bool IsSnapshotted { get; set; }

	public void SetNextReplenishTimepoint(ResourceContainer parent)
	{
		double num = parent.ResourceType.ComputeReplenishDaysFromNow(ref indexOfLastReplenishPoint);
		replenishTimePointInSeconds = The.Sim.TotalUnPausedGameTimeInSeconds + num * DateAndTime.secondsPerDay;
		Common.IsZero(num);
	}

	public void UpdateMaxItemsEverSet(int noOfItems)
	{
		maxResourceItemsEverSet = (ushort)Common.Max(maxResourceItemsEverSet, noOfItems);
	}

	public void Update(GameTime gameTime, ResourceContainer parent)
	{
		if (The.Sim.TimepointReached(replenishTimePointInSeconds))
		{
			ReplenishResourceItems(parent);
		}
	}

	public float GetCurrentReplenishRate(ResourceContainer parent, out bool maximumReached)
	{
		_ = parent.ResourceType;
		float maximumReplenishRate = GetMaximumReplenishRate(parent);
		if (maxResourceItemsEverSet <= parent.NoOfHarvestableItems)
		{
			maximumReached = true;
			return 0f;
		}
		maximumReached = false;
		return Common.ClampBottom(maximumReplenishRate, 0f);
	}

	public float GetMaximumReplenishRate(ResourceContainer parent)
	{
		ResourceType resourceType = parent.ResourceType;
		float result = 0f;
		if (resourceType.DaysOfYearToReplenish != null)
		{
			result = (float)resourceType.OrderedDaysOfYearToReplenish.Count * resourceType.FractionOfMaximumToReplenishEachTime * (float)(int)maxResourceItemsEverSet;
		}
		return result;
	}

	private void ReplenishResourceItems(ResourceContainer parent)
	{
		if (parent.NoOfHarvestableItems < maxResourceItemsEverSet)
		{
			int num = (int)Math.Round(parent.ResourceType.FractionOfMaximumToReplenishEachTime * (float)(int)maxResourceItemsEverSet);
			// The studio's line was `Common.Clamp(num, 1, maxResourceItemsEverSet - parent.NoOfHarvestableItems);`
			// - a pure function called as a statement, its result dropped. So `num` is not the
			// amount needed to reach the maximum; it is the fraction of the maximum, added on top of
			// what is already there. With a fraction of 1.0 the tile roughly DOUBLES, and
			// UpdateMaxItemsEverSet then ratchets the maximum up to the new count, so the next
			// harvest starts the next doubling from higher up.
			//
			// UNHIDDEN MOD: use the clamp the studio computed. Under the switch because it changes
			// the food economy of a running game, and because the port's own patches are meant to
			// leave gameplay alone.
			int clamped = Common.Clamp(num, 1, maxResourceItemsEverSet - parent.NoOfHarvestableItems);
			if (UWGame.Mods.UnhiddenMod.Enabled && UWGame.Mods.UnhiddenMod.CapResourceRespawn.On)
			{
				num = clamped;
			}
			parent.AddResourceItems(num);
		}
		SetNextReplenishTimepoint(parent);
	}

	public double? GetUpdateInterval()
	{
		double? currentInterval = null;
		UpdateTimePoints.GetSoonestInterval(GetIntervalForReplenish(), ref currentInterval);
		return currentInterval;
	}

	private double? GetIntervalForReplenish()
	{
		if (replenishTimePointInSeconds.HasValue)
		{
			return UpdateTimePoints.ComputeIntervalFromTimepoint(replenishTimePointInSeconds);
		}
		return null;
	}

	public virtual ISnapshot DoSnapshot(Snapshotter sn)
	{
		maxResourceItemsEverSet = sn.DoUInt16(maxResourceItemsEverSet);
		replenishTimePointInSeconds = sn.DoDoubleNullable(replenishTimePointInSeconds);
		indexOfLastReplenishPoint = sn.DoUInt16Nullable(indexOfLastReplenishPoint);
		return this;
	}

	public virtual void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}

	public virtual Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}
}
