using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Overland.Missions;

public abstract class MissionAction : ISnapshot
{
	public Mission parent;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public MissionAction(Mission parent)
	{
		this.parent = parent;
	}

	public MissionAction()
	{
	}

	public virtual void Destroy()
	{
	}

	public virtual bool Update(GameTime elapsed)
	{
		return false;
	}

	public virtual void StartMission()
	{
	}

	protected void HandleFailedAction()
	{
		parent.Abort();
	}

	public virtual ISnapshot DoSnapshot(Snapshotter sn)
	{
		sn.Ignore(parent);
		return this;
	}

	public virtual Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public virtual void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}
}
