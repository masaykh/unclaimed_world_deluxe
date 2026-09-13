using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.Buildings;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

internal class GoalDoSleep : CompositeGoal
{
	private Regulator testTimeToWakeupRegulator;

	private float maximumSleepGainFromThisLocation = 1f;

	private float increaseAmountPerDay;

	private bool isSleeping;

	private Regulator conditionRegulator;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public GoalDoSleep(Entity owner, EvaluateSleep evaluator)
		: base(owner)
	{
		GoalEvaluator = evaluator;
	}

	protected override void CreateRegulators()
	{
		conditionRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 0.5, "GoalDoSleepCondition");
		testTimeToWakeupRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 1.0, "GoalDoSleep");
	}

	public GoalDoSleep()
	{
	}

	protected override void Activate()
	{
		if (AreThereNonMovingEntitiesInThisSpot())
		{
			base.Status = Status.Failed;
			return;
		}
		Entity container = null;
		if (!entity.GetContainedBy(out container))
		{
			base.Status = Status.Failed;
			return;
		}
		ComputeSleepConditions(entity, container, out maximumSleepGainFromThisLocation, out increaseAmountPerDay);
		base.Status = Status.Active;
		if (entity.EntityType.Person != null)
		{
			The.Client.AddLogEvent(entityIntelligence.Allegiance, The.Client.Log.GeneralEvent, entity, "goes to sleep");
		}
		entityIntelligence.IsAwakeAndActive = false;
		DropAllCarriedItems();
		EnableCollisions(enable: false);
		if (entity.HasStance())
		{
			ChangeStance(entity.Locomotor.Stance.PickRandomStance(entity.EntityType.LocomotorType.StancesType.SleepStances, entity.EntityType.LocomotorType.StancesType.DefaultStanceType));
		}
		entity.EntityType.IntelligenceType.EventActions.TryGetValue(AgentActionHooks.GoingToSleep, out var value);
		Goal.FireEventActions(entity, null, value);
	}

	public override bool CanReactToInterest()
	{
		return false;
	}

	public static void ComputeSleepConditions(Entity entity, IKnownEntityData containerData, out float maximumSleepGainFromThisLocation, out float increaseAmountPerDay)
	{
		float num = 1f;
		maximumSleepGainFromThisLocation = 1f;
		if (entity.EntityType.Person != null)
		{
			if (containerData == null)
			{
				maximumSleepGainFromThisLocation = GameData.Instance.Constants.SleepNeed.MaxLimitForPeopleSleepingInOpen;
				num = GameData.Instance.Constants.SleepNeed.GainFactorForPeopleSleepingInOpen;
			}
			else if (containerData.EntityType.ContainerType != null && containerData.EntityType.ContainerType.ResidenceType != null)
			{
				num = Residence.GetSleepNeedGainFactor(containerData.Condition.Value, containerData.EntityType.ContainerType.ResidenceType.ComfortLevel);
			}
			else if (containerData.EntityType.GatheringSiteType != null)
			{
				if (containerData.IsPrepared == true)
				{
					maximumSleepGainFromThisLocation = GameData.Instance.Constants.SleepNeed.MaxLimitForPeopleSleepingNearCampfire;
					num = GameData.Instance.Constants.SleepNeed.GainFactorForPeopleSleepingNearCampfire;
				}
				else
				{
					maximumSleepGainFromThisLocation = GameData.Instance.Constants.SleepNeed.MaxLimitForPeopleSleepingInOpen;
					num = GameData.Instance.Constants.SleepNeed.GainFactorForPeopleSleepingInOpen;
				}
			}
			increaseAmountPerDay = num * GameData.Instance.Constants.SleepNeed.GainPerDayWhenSleeping;
		}
		else
		{
			increaseAmountPerDay = GameData.Instance.Constants.SleepNeed.GainPerDayWhenSleeping;
		}
	}

	public override float GetExertionLevel()
	{
		return GameData.Instance.Constants.PhysicalWork.Sleeping;
	}

	public override DetectionFactor GetDetectAgentsFactor(EntityType typeOfAgent, bool requiresExamineAction)
	{
		return DetectionFactor.CannotDetect;
	}

	public override DetectionFactor GetDetectResourcesFactor(ResourceType resourceType, bool requiresExamineAction)
	{
		return DetectionFactor.CannotDetect;
	}

	public override string GetStatus()
	{
		return "Sleeping";
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		base.Status = ProcessSubgoals(elapsed);
		if (base.Status == Status.Completed && !isSleeping)
		{
			isSleeping = true;
			entity.Renderable.SetAnimationActionStateFlag(AnimAction.Sleeping);
			entity.DisableCollisions();
			base.Status = Status.Active;
		}
		if (base.Status == Status.Failed)
		{
			return;
		}
		base.Status = Status.Active;
		if (!isSleeping)
		{
			return;
		}
		Need need = entity.BiologicalEntity.Needs.NeedsList.Values.FirstOrDefault((Need n) => n.NeedType.SleepNeedType != null);
		if (conditionRegulator.IsReady())
		{
			if (!entity.GetContainedBy(out Entity container))
			{
				base.Status = Status.Failed;
				return;
			}
			ComputeSleepConditions(entity, container, out maximumSleepGainFromThisLocation, out increaseAmountPerDay);
		}
		need.CurrentLevel = Common.IncreaseValueBetweenZeroAndTopLimit(need.CurrentLevel, increaseAmountPerDay, elapsed.ElapsedGameTime.TotalSeconds, maximumSleepGainFromThisLocation);
		if (testTimeToWakeupRegulator.IsReady() && IsItTimeToWakeup())
		{
			base.Status = Status.Completed;
			if (entity.PersonEntity != null)
			{
				The.Client.AddLogEvent(entityIntelligence.Allegiance, The.Client.Log.GeneralEvent, entity, "wakes up");
			}
		}
	}

	private bool IsItTimeToWakeup()
	{
		if (((EvaluateSleep)GoalEvaluator).ScoreSleepNeed() < 0.5 && GoalEvaluator.ScoreTimeOfDay() < 0.05000000074505806)
		{
			return true;
		}
		return false;
	}

	public override void OnExit()
	{
		base.OnExit();
		entity.Renderable.ClearAnimationActionStateFlag(AnimAction.Sleeping);
		entityIntelligence.IsAwakeAndActive = true;
		EnableCollisions(enable: true);
	}

	public override bool HandleMessage(Message message)
	{
		switch (message.MessageType)
		{
		case Message.MessageTypes.WakeUpCombatAlert:
			if (entityIntelligence.Memory.RecentlyGotCombatAlert())
			{
				return false;
			}
			entityIntelligence.Memory.SetTimepointForCombatAlert();
			base.Status = Status.Completed;
			return true;
		case Message.MessageTypes.AlertToPresence:
			base.Status = Status.Completed;
			return false;
		case Message.MessageTypes.Hit:
			base.Status = Status.Completed;
			return false;
		case Message.MessageTypes.CancelJobForAIReset:
			return true;
		default:
			return base.HandleMessage(message);
		}
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
		maximumSleepGainFromThisLocation = sn.DoFloat(maximumSleepGainFromThisLocation);
		increaseAmountPerDay = sn.DoFloat(increaseAmountPerDay);
		isSleeping = sn.DoBool(isSleeping);
		sn.Ignore(testTimeToWakeupRegulator);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		base.LoadPostProcess(sn);
	}
}
