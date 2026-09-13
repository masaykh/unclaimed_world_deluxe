using System.Collections.Generic;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

public class ReplenishItemsForAction : ISnapshot
{
	public ProcessType Action;

	public List<EntityID> Items;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public ReplenishItemsForAction(ProcessType action, List<EntityID> items)
	{
		Action = action;
		Items = items;
	}

	public ReplenishItemsForAction()
	{
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		Action = sn.DoGameData(Action);
		Items = sn.DoList(Items);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}
}
