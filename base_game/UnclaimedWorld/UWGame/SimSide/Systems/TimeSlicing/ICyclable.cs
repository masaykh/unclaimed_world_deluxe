using System.Text;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Systems.TimeSlicing;

public interface ICyclable : ILookUp<ICyclable, CyclableID>, ISnapshot
{
	bool IsPaused { get; }

	bool UnregisterBeforeSnapshot { get; }

	double StartedOnTimeInSeconds { get; set; }

	double ComputationTimeSpentInSeconds { get; set; }

	double TotalComputationAllInstancesInSeconds { get; set; }

	double? UpdateInterval { get; }

	bool CycleOnce();

	void PrintInfo(StringBuilder text);
}
