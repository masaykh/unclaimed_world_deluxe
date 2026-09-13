using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

public class GoalIsDying : Goal
{
	private bool isDone;

	private OwnerID? ownerOfCarcass;

	private bool takeBleedDamage;

	private Regulator reduceHitpoints;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public GoalIsDying(Entity entity, OwnerID? ownerOfCarcass, bool takeBleedDamage)
		: base(entity)
	{
		this.takeBleedDamage = takeBleedDamage;
		this.ownerOfCarcass = ownerOfCarcass;
	}

	public GoalIsDying()
	{
	}

	protected override void Activate()
	{
		base.Status = Status.Active;
		entityIntelligence.IsAwakeAndActive = false;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		entity.Renderable.SetAnimationActionStateFlag(AnimAction.Dying);
		entity.Renderable.SetAnimationStateFlag(AnimModifier.Post);
	}

	public override void OnExit()
	{
		base.OnExit();
		entity.Renderable.ClearAnimationActionStateFlag(AnimAction.Dying);
		entity.Renderable.ClearAnimationStateFlag(AnimModifier.Post);
	}

	public override bool IsSame(Job job)
	{
		return false;
	}

	public override bool CanReactToInterest()
	{
		return false;
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		double millisecondsSinceLastReady = 0.0;
		if (takeBleedDamage && reduceHitpoints.IsReady(ref millisecondsSinceLastReady))
		{
			float num = (float)millisecondsSinceLastReady / 1000f;
			entity.Body.GlobalHitpoints -= num * GameData.Instance.Constants.FractionOfMaxHitpointsLostPerSecondWhenDying * entity.Body.MaxHitpoints;
		}
		entity.GetStatus(out var isDead, out var _, out var causeOfDeath, out var _);
		if (isDead)
		{
			entity.Kill(ownerOfCarcass, causeOfDeath);
			base.Status = Status.Completed;
		}
	}

	public override string GetStatus()
	{
		return "Dying";
	}

	protected override void CreateRegulators()
	{
		base.CreateRegulators();
		reduceHitpoints = new Regulator(The.Sim.GameplayRandomGenerator, 1.0, "GoalIsDyingReduceHitpoints");
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
		takeBleedDamage = sn.DoBool(takeBleedDamage);
		return this;
	}
}
