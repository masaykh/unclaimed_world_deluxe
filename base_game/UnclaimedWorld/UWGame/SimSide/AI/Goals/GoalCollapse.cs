using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

public class GoalCollapse : CompositeGoal
{
	private bool isDone;

	private OwnerID? ownerOfCarcass;

	private EntityID? killer;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public GoalCollapse(Entity entity, OwnerID? ownerOfCarcass, EntityID? killer = null)
		: base(entity)
	{
		this.ownerOfCarcass = ownerOfCarcass;
		this.killer = killer;
	}

	public GoalCollapse()
	{
	}

	protected override void Activate()
	{
		base.Status = Status.Active;
		if (entity.AgentStorage != null)
		{
			entity.AgentStorage.IterateContained(delegate(Entity e)
			{
				entity.AgentStorage.Uncontain(e);
			});
		}
		EnableCollisions(enable: false);
		if (entity.HasStance())
		{
			entity.Locomotor.Stance.CurrentStance = entity.EntityType.LocomotorType.StancesType.IncapacitatedStanceType;
		}
		entityIntelligence.IsAwakeAndActive = false;
		if (entity.Intelligence.ThreatStance == ThreatStance.Bold)
		{
			AddSubgoal(new GoalWait(entity, 2.0, AnimAction.Dying, AnimModifier.Pre, AnimModifier.Bold, scaleAnimationToFillWaitPeriod: true));
		}
		else
		{
			AddSubgoal(new GoalWait(entity, 2.0, AnimAction.Dying, AnimModifier.Pre, scaleAnimationToFillWaitPeriod: true));
		}
	}

	public override bool CanReactToInterest()
	{
		return false;
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		if (ProcessSubgoals(elapsed) == Status.Completed)
		{
			entity.GetStatus(out var isDead, out var _, out var causeOfDeath, out var _);
			if (isDead)
			{
				entity.Kill(ownerOfCarcass, causeOfDeath, killer);
			}
			else
			{
				base.Status = Status.Completed;
			}
		}
	}

	public override DetectionFactor GetDetectAgentsFactor(EntityType typeOfAgent, bool requiresExamineAction)
	{
		return DetectionFactor.CannotDetect;
	}

	public override DetectionFactor GetDetectResourcesFactor(ResourceType resourceType, bool requiresExamineAction)
	{
		return DetectionFactor.CannotDetect;
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override bool HandleMessage(Message message)
	{
		return true;
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
		isDone = sn.DoBool(isDone);
		ownerOfCarcass = sn.DoEnumNullable(ownerOfCarcass);
		killer = sn.DoEntityIDNullable(killer);
		return this;
	}
}
