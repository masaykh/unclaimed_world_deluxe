using System.Collections.Generic;
using System.Linq;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Allegiances.Statistics;

public class PopulationStatistics : ISnapshot
{
	public List<DataPoint<float>> Population = new List<DataPoint<float>>();

	public List<DataPoint<float>> Emigration = new List<DataPoint<float>>();

	public List<DataPoint<float>> IndependentDeaths = new List<DataPoint<float>>();

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public void RecordDeath()
	{
	}

	public void RecordEmigration()
	{
	}

	public void SetPopulation(int members)
	{
		DateAndTime.TimeDateYear currentTimeDateYear = The.Sim.DateAndTime.CurrentTimeDateYear;
		if (Population.Count > 0 && Common.IsEqual(Population.Last().Time.TotalDays, currentTimeDateYear.TotalDays))
		{
			Population.RemoveAt(Population.Count - 1);
		}
		Population.Add(new DataPoint<float>(members, currentTimeDateYear));
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		Population = sn.DoList(Population);
		Emigration = sn.DoList(Emigration);
		IndependentDeaths = sn.DoList(IndependentDeaths);
		return this;
	}

	public virtual void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}
}
