using System.Collections.Generic;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Policies;

public class AllegiancePolicy : ISnapshot
{
	public enum CreaturePolicy
	{
		NeverAttack,
		Defend,
		HuntForProducts,
		HuntToDestroy
	}

	public Dictionary<EntityType, CreaturePolicy> PolicyTowardsCreatures = new Dictionary<EntityType, CreaturePolicy>();

	public Dictionary<EntityType, List<EntityType>> DoNotUseInProduct = new Dictionary<EntityType, List<EntityType>>();

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		sn.Postpone(PolicyTowardsCreatures);
		sn.Postpone(DoNotUseInProduct);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}
}
