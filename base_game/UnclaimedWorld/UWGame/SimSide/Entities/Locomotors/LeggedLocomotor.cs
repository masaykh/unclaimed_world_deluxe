using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities.Locomotors;

public class LeggedLocomotor : ISnapshot
{
	public float Strength;

	public float Agility;

	public Locomotor Parent;

	private Goal.MovementSpeeds currentMovementSpeedType = Goal.MovementSpeeds.Normal;

	public bool TestWander;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public Goal.MovementSpeeds TargetSpeed
	{
		get
		{
			return currentMovementSpeedType;
		}
		set
		{
			if (currentMovementSpeedType != value)
			{
				currentMovementSpeedType = value;
				Parent.CurrentMaximumSpeedIsDirty = true;
				Parent.CurrentMaximumSpeedNotAffectedByTerrainIsDirty = true;
			}
		}
	}

	public bool IsSnapshotted { get; set; }

	public LeggedLocomotor()
	{
	}

	public LeggedLocomotor(Locomotor parent)
	{
		Parent = parent;
		Strength = (float)The.Sim.GameplayRandomGenerator.RandomNormalDistribution(0.5, 0.10000000149011612);
		Agility = (float)The.Sim.GameplayRandomGenerator.RandomNormalDistribution(0.5, 0.10000000149011612);
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		Agility = sn.DoFloat(Agility);
		currentMovementSpeedType = sn.DoEnum(currentMovementSpeedType);
		Strength = sn.DoFloat(Strength);
		TestWander = sn.DoBool(TestWander);
		sn.Ignore(Parent);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}
}
