using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Resources;

public class TileResourceItem : IResourceItem, ILookUp<IResourceItem, ResourceItemID>, ISnapshot
{
	private JobID? snapshotJob;

	private TileResourceContainer container;

	private ResourceID snapshotResourceContainer;

	private ResourceItemID id = ResourceItemID.Invalid;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public ProcessJob AssignedToJob { get; set; }

	public ResourceContainer Container => container;

	public ResourceItemID ID
	{
		get
		{
			return id;
		}
		private set
		{
			id = value;
		}
	}

	public int LoadPostProcessOrder => 0;

	public bool IsSnapshotted { get; set; }

	public TileResourceItem()
	{
	}

	public TileResourceItem(TileResourceContainer container)
	{
		AddToLookup();
		this.container = container;
	}

	public void Destroy()
	{
		RemoveIDEntry();
	}

	public ResourceItemID GetUniqueID()
	{
		return ResourceItem.GetUniqueID();
	}

	public ResourceItemID SnapshotID(Snapshotter sn, ResourceItemID id)
	{
		return sn.DoEnum(id);
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != ResourceItemID.Invalid)
		{
			LookUp<IResourceItem, ResourceItemID>.Add(ID, this);
		}
	}

	public void RemoveIDEntry()
	{
		LookUp<IResourceItem, ResourceItemID>.Remove(this);
	}

	public void SetInvalid()
	{
		id = ResourceItemID.Invalid;
	}

	public void ResetIDCounter()
	{
	}

	void ILookUp<IResourceItem, ResourceItemID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUp<IResourceItem, ResourceItemID>.Create();
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		id = sn.DoEnum(id);
		snapshotResourceContainer = sn.SnapshotID<ResourceContainer, ResourceID>(container).Value;
		snapshotJob = sn.SnapshotID<Job, JobID>(AssignedToJob);
		sn.Ignore(container);
		sn.Ignore(AssignedToJob);
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
		container = (TileResourceContainer)LookUp<ResourceContainer, ResourceID>.FindByID(snapshotResourceContainer);
		if (snapshotJob.HasValue)
		{
			AssignedToJob = (ProcessJob)LookUp<Job, JobID>.FindByID(snapshotJob);
		}
	}
}
