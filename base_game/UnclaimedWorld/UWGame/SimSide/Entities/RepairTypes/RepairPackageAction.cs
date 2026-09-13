using UWGame.SimSide.Processes;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities.RepairTypes;

public class RepairPackageAction : ISnapshot
{
	public RepairAction RepairAction;

	public Entity Part;

	private EntityID? snapshotPart;

	public ProcessType RepairProcess;

	private Snapshotter.Version version;

	public bool IsSnapshotted { get; set; }

	public RepairPackageAction()
	{
	}

	public RepairPackageAction(RepairPackageAction original)
	{
		RepairAction = original.RepairAction;
		Part = original.Part;
		RepairProcess = original.RepairProcess;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		RepairAction = sn.DoEnum(RepairAction);
		RepairProcess = sn.DoGameData(RepairProcess);
		snapshotPart = sn.SnapshotID<Entity, EntityID>(Part);
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
