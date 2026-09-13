using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Processes;

public class Productivity : ISnapshot
{
	private Snapshotter.Version version = Snapshotter.Version.Original;

	public float SkillProductivity { get; private set; }

	public float ToolProductivity { get; private set; }

	public float EnergyProductivity { get; private set; }

	public float TotalProductivity { get; private set; }

	public bool IsSnapshotted { get; set; }

	public Productivity()
	{
	}

	public Productivity(Productivity original)
	{
		SkillProductivity = original.SkillProductivity;
		ToolProductivity = original.ToolProductivity;
		EnergyProductivity = original.EnergyProductivity;
		TotalProductivity = original.TotalProductivity;
	}

	private float GetMean(float val1, float val2)
	{
		return (val1 + val2) / 2f;
	}

	public void Merge(Productivity mergeWith)
	{
		TotalProductivity = GetMean(TotalProductivity, mergeWith.TotalProductivity);
		SkillProductivity = GetMean(SkillProductivity, mergeWith.SkillProductivity);
		ToolProductivity = GetMean(ToolProductivity, mergeWith.ToolProductivity);
		EnergyProductivity = GetMean(EnergyProductivity, mergeWith.EnergyProductivity);
	}

	public void SaveProductivityStats(float progressDelta, float toolProductivity, float skillProductivity, float energyProductivity, float updateInterval)
	{
		ToolProductivity += progressDelta * toolProductivity;
		SkillProductivity += progressDelta * skillProductivity;
		EnergyProductivity += progressDelta * energyProductivity;
		TotalProductivity += progressDelta / updateInterval;
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		ToolProductivity = sn.DoFloat(ToolProductivity);
		EnergyProductivity = sn.DoFloat(EnergyProductivity);
		SkillProductivity = sn.DoFloat(SkillProductivity);
		TotalProductivity = sn.DoFloat(TotalProductivity);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}
}
