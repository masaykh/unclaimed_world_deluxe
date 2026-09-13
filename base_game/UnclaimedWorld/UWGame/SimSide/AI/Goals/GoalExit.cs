using Microsoft.Xna.Framework;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

internal class GoalExit : Goal
{
	private bool isDisembarking;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public GoalExit(Entity owner, bool isDisembarking = false)
		: base(owner)
	{
		this.isDisembarking = isDisembarking;
	}

	public GoalExit()
	{
	}

	protected override void Activate()
	{
		if (!entity.GetContainedBy(out Entity container))
		{
			base.Status = Status.Failed;
		}
		else if (container != null)
		{
			base.Status = Status.Active;
		}
		else
		{
			base.Status = Status.Completed;
		}
	}

	public override bool IsSame(Job job)
	{
		return false;
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		if (!entity.GetContainedBy(out Entity container))
		{
			base.Status = Status.Failed;
			return;
		}
		Vector3 position = Vector3.Zero;
		((IExit)container.Contains).GetDoorPosition(ref position, exiting: true, out var _);
		container.Contains.Uncontain(entity, destroy: false, shouldQueue: true, null, null, null, null, null, position);
		if (((IGarrison)container.Contains).GetNoOfAgentsInside() == 0)
		{
			container.TurnOffTheLight();
		}
		if (isDisembarking && entityIntelligence.Allegiance.AllegianceType == AllegianceType.Player)
		{
			entity.EntityType.IntelligenceType.EventActions.TryGetValue(AgentActionHooks.DisembarkedNewPlayerAllegianceMember, out var value);
			Goal.FireEventActions(entity, null, value);
		}
		base.Status = Status.Completed;
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
		isDisembarking = sn.DoBool(isDisembarking);
		return this;
	}
}
