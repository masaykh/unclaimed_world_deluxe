using System.Collections.Generic;
using System.Linq;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities.RepairTypes;

public class RepairPackage : ISnapshot
{
	public List<RepairPackageAction> Actions;

	private Snapshotter.Version version;

	public bool IsSnapshotted { get; set; }

	public RepairPackage()
	{
	}

	public RepairPackage(RepairPackage original)
	{
		Actions = new List<RepairPackageAction>();
		foreach (RepairPackageAction action in original.Actions)
		{
			Actions.Add(new RepairPackageAction(action));
		}
	}

	public bool IncludesPartRepair(Entity part)
	{
		return false;
	}

	public bool MatchesJob(ProcessJob repairJob)
	{
		return Actions.Any((RepairPackageAction p) => p.RepairAction == repairJob.RepairJob.RepairActionToUse && ((p.Part == null && !repairJob.RepairJob.PartToFix.HasValue) || (p.Part != null && repairJob.RepairJob.PartToFix.HasValue && p.Part.ID == repairJob.RepairJob.PartToFix.Value.Entity)));
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		Actions = sn.DoList(Actions);
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
		if (Actions == null)
		{
			return;
		}
		foreach (RepairPackageAction action in Actions)
		{
			action.LoadPostProcess(sn);
		}
	}
}
