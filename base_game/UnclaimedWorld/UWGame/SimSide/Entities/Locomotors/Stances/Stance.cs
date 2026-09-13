using System.Collections.Generic;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities.Locomotors.Stances;

public class Stance : ISnapshot
{
	public Locomotor Parent;

	private StanceType currentStance;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public StanceType CurrentStance
	{
		get
		{
			return currentStance;
		}
		set
		{
			currentStance = value;
		}
	}

	public bool IsSnapshotted { get; set; }

	public Stance()
	{
	}

	public Stance(Locomotor parent)
	{
		Parent = parent;
		currentStance = parent.Parent.EntityType.LocomotorType.StancesType.DefaultStanceType;
	}

	public StanceType PickRandomStance(IList<ChanceToTakeStance> stancesToSelectFrom, StanceType defaultStance)
	{
		StanceType result = null;
		if (stancesToSelectFrom != null)
		{
			double num = -1.0;
			double num2 = 0.0;
			foreach (ChanceToTakeStance item in stancesToSelectFrom)
			{
				num2 = ScoreStance(item, stancesToSelectFrom.Count);
				if (num2 >= num)
				{
					num = num2;
					result = item.StanceType;
				}
			}
		}
		else
		{
			result = defaultStance;
		}
		return result;
	}

	public StanceType PickRandomProcessStance(Dictionary<StancesType, List<ChanceToTakeStance>> stanceTypes)
	{
		StanceType defaultStanceTypeWhenWorking = Parent.Parent.EntityType.LocomotorType.StancesType.DefaultStanceTypeWhenWorking;
		List<ChanceToTakeStance> value = null;
		stanceTypes?.TryGetValue(Parent.Parent.EntityType.LocomotorType.StancesType, out value);
		return PickRandomStance(value, defaultStanceTypeWhenWorking);
	}

	public double ScoreStance(ChanceToTakeStance chanceToTakeStance, int totalStances)
	{
		double num = (double)(chanceToTakeStance.Chance ?? (1f / (float)totalStances)) * The.Sim.GameplayRandomGenerator.NextDouble("Goal");
		double num2 = 0.0;
		if (currentStance == chanceToTakeStance.StanceType)
		{
			num2 = chanceToTakeStance.AddedChanceToRemainInStance ?? 0f;
		}
		return num + num2;
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		currentStance = sn.DoGameData(currentStance);
		sn.Ignore(Parent);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}
}
