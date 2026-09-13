using System;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI.Activities;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

public class GoalMoveAsGroup : CompositeGoal
{
	private GroupMoveActivity activity;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public GoalMoveAsGroup(Entity owner, GroupMoveActivity activity)
		: base(owner)
	{
		this.activity = activity;
	}

	protected override void Activate()
	{
		base.Status = Status.Active;
		RemoveAllSubgoals();
	}

	public override Snapshotter.Version DoVersion(Snapshotter sn)
	{
		base.DoVersion(sn);
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		throw new Exception("THIS CLASS IS OBSOLETE");
	}

	public GoalMoveAsGroup()
	{
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		base.Status = ProcessSubgoals(elapsed);
		_ = base.Status;
		_ = 2;
	}
}
