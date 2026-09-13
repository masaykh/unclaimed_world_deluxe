using UWGame.SimSide.Entities;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Jobs;

public class RepairJob : ISnapshot
{
	public EntityID EntityToRepair;

	public EntityAndRoot? PartToFix;

	public RepairAction RepairActionToUse;

	private Snapshotter.Version version;

	public bool IsSnapshotted { get; set; }

	public RepairJob(EntityID entityToRepair, RepairAction repairAction, EntityAndRoot? partToFix)
	{
		EntityToRepair = entityToRepair;
		PartToFix = partToFix;
		RepairActionToUse = repairAction;
	}

	public RepairJob()
	{
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		EntityToRepair = sn.DoEnum(EntityToRepair);
		RepairActionToUse = sn.DoEnum(RepairActionToUse);
		PartToFix = sn.DoEntityAndRootNullable(PartToFix);
		return this;
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}
}
