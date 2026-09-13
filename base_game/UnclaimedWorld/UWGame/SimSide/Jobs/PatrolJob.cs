using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Jobs;

public class PatrolJob : CombatAreaJob
{
	public bool AttackVermin;

	public bool AttackTargetsOutsideZone;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public override bool CanAttackVermin => AttackVermin;

	public override bool CanAttackTargetsOutsideZone => AttackTargetsOutsideZone;

	public PatrolJob()
	{
	}

	public PatrolJob(Zone zone, EntityGroup entityGroup, bool attackVermin, bool attackTargetsOutsideZone, int noOfPatrollers)
		: base(zone, entityGroup, noOfPatrollers)
	{
		AttackVermin = attackVermin;
		AttackTargetsOutsideZone = attackTargetsOutsideZone;
	}

	public override string GetName()
	{
		return "Patrolling";
	}

	public override void Destroy(bool removeTakers, Entity entityToExcludeFromCancel = null)
	{
		if (Zone != null && Zone.PatrolJob != null)
		{
			Zone.PatrolJob = null;
		}
		base.Destroy(removeTakers, entityToExcludeFromCancel);
	}

	public override Snapshotter.Version DoVersion(Snapshotter sn)
	{
		base.DoVersion(sn);
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		AttackVermin = sn.DoBool(AttackVermin);
		AttackTargetsOutsideZone = sn.DoBool(AttackTargetsOutsideZone);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		base.LoadPostProcess(sn);
	}
}
