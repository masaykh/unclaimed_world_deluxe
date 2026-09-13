using System.Collections.Generic;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities.Containers.Components;

public class AmmoOfType : ISnapshot
{
	public List<EntityID> Items;

	private int totalRounds;

	public bool TotalIsDirty = true;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public int TotalRounds
	{
		get
		{
			if (TotalIsDirty)
			{
				RecomputeTotalRounds();
			}
			return totalRounds;
		}
	}

	public bool IsSnapshotted { get; set; }

	public void RecomputeTotalRounds()
	{
		int num = 0;
		for (int num2 = Items.Count - 1; num2 >= 0; num2--)
		{
			Entity entity = Entity.FindByID(Items[num2]);
			if (entity != null)
			{
				num += entity.Item.Ammunition.NoOfRounds;
			}
			else
			{
				Items.RemoveAt(num2);
			}
		}
		totalRounds = num;
		TotalIsDirty = false;
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		Items = sn.DoList(Items);
		totalRounds = sn.DoInt32(totalRounds);
		TotalIsDirty = sn.DoBool(TotalIsDirty);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
	}
}
