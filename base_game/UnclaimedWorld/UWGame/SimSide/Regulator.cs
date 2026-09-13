using System;
using Microsoft.Xna.Framework;
using UWGame.Control.Replays;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide;

public class Regulator : ISnapshot
{
	public enum Modes
	{
		Sim,
		Client
	}

	public Modes Mode;

	private long updatePeriod;

	private TimeSpan elapsedTime = new TimeSpan(0L);

	private TimeSpan? lastUpdate;

	private RandomGenerator randomGenerator;

	private Snapshotter.Version version;

	public bool IsSnapshotted { get; set; }

	public Regulator()
	{
	}

	public Regulator(RandomGenerator random, double numUpdatesPerSecond, string belongsTo, Modes mode = Modes.Sim)
	{
		Mode = mode;
		randomGenerator = random;
		if (numUpdatesPerSecond > 0.0)
		{
			updatePeriod = (long)(1000.0 / numUpdatesPerSecond);
			if (randomGenerator != null)
			{
				if (updatePeriod < int.MaxValue)
				{
					elapsedTime = new TimeSpan(0, 0, 0, 0, (int)(updatePeriod - randomGenerator.Next(10, "Regulator", saveMessage: false)));
				}
				else
				{
					elapsedTime = new TimeSpan(0, 0, 0, 0, (int)(updatePeriod - randomGenerator.Next(10, "Regulator", saveMessage: false)));
				}
			}
		}
		else if (Common.IsEqual(0.0, numUpdatesPerSecond))
		{
			updatePeriod = 0L;
		}
		else if (numUpdatesPerSecond < 0.0)
		{
			updatePeriod = -1L;
		}
	}

	public bool IsReadyGetTimeElapsedInSeconds(out double secondsSinceLastReady)
	{
		double millisecondsSinceLastReady = 0.0;
		bool result = IsReady(ref millisecondsSinceLastReady);
		secondsSinceLastReady = millisecondsSinceLastReady / 1000.0;
		return result;
	}

	public bool IsReady(GameTime gameTime, ref double millisecondsSinceLastReady)
	{
		TimeSpan totalGameTime = gameTime.TotalGameTime;
		return IsReady(totalGameTime, ref millisecondsSinceLastReady);
	}

	public bool IsReady(ref double millisecondsSinceLastReady)
	{
		TimeSpan timeToUse = GetTimeToUse();
		return IsReady(timeToUse, ref millisecondsSinceLastReady);
	}

	private bool IsReady(TimeSpan timeToUse, ref double millisecondsSinceLastReady)
	{
		if (!lastUpdate.HasValue)
		{
			lastUpdate = timeToUse;
		}
		TimeSpan ts = timeToUse.Subtract(lastUpdate.Value);
		lastUpdate = timeToUse;
		elapsedTime = elapsedTime.Add(ts);
		if (elapsedTime.TotalMilliseconds >= (double)updatePeriod)
		{
			millisecondsSinceLastReady = elapsedTime.TotalMilliseconds;
			if (randomGenerator != null)
			{
				elapsedTime = new TimeSpan(0, 0, 0, 0, randomGenerator.Next(10, "Regulator", saveMessage: false));
			}
			else
			{
				elapsedTime = new TimeSpan(0, 0, 0, 0, 0);
			}
			return true;
		}
		return false;
	}

	private TimeSpan GetTimeToUse()
	{
		if (Mode == Modes.Sim)
		{
			return The.Sim.TotalUnPausedGameTime;
		}
		return The.Sim.GameTime.TotalGameTime;
	}

	public bool IsReady()
	{
		double millisecondsSinceLastReady = 0.0;
		return IsReady(ref millisecondsSinceLastReady);
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		Mode = sn.DoEnum(Mode);
		lastUpdate = sn.DoTimeSpanNullable(lastUpdate);
		elapsedTime = sn.DoTimeSpan(elapsedTime);
		updatePeriod = sn.DoInt64(updatePeriod);
		sn.Ignore(randomGenerator);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		randomGenerator = The.Sim.GameplayRandomGenerator;
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}
}
