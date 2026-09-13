using Microsoft.Xna.Framework;

namespace UWGame.SimSide.Systems;

public interface ISleepingUpdatable
{
	double? TimePointInSeconds { get; }

	double? UpdateInterval { get; }

	SleepyUpdaterID SleepyUpdater { get; set; }

	void SetNextTimepoint(double? timepoint);

	void Update(GameTime gameTime, out bool wasDestroyed);

	void RecomputeUpdateInterval(out bool intervalChanged);

	void CreateSleepyLookupCollection();
}
