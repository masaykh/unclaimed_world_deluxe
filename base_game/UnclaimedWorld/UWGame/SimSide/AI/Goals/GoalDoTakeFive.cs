using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.Collisions;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.SimSide.Entities.Locomotors.Stances;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

internal class GoalDoTakeFive : CompositeGoal
{
	private double? duration;

	private double timeAlreadyRested;

	public bool TestForDanger = true;

	private StanceType stanceToTake;

	private List<AnimModifier> statesToTake = new List<AnimModifier>();

	private List<AnimModifier> statesToClear = new List<AnimModifier>();

	public bool HasTriedToStartConversation;

	public bool IsListening;

	private const float idleDuration = 5f;

	public static float TimeToWaitBeforeTurningBodyToListen = 0.85f;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public GoalDoTakeFive(Entity owner, double? duration = null)
		: base(owner)
	{
		this.duration = duration;
	}

	public GoalDoTakeFive()
	{
	}

	protected override void Activate()
	{
		entityIntelligence.IsAwakeAndActive = true;
		if (entity.Find<Locomotor>(out var c))
		{
			c.CurrentMoveTarget = entity.PlaySiteLocation;
			if (entity.HasStance())
			{
				StanceType currentStance = c.Stance.CurrentStance;
				SelectIdleStance(currentStance);
			}
		}
		if (!duration.HasValue)
		{
			duration = 5.0;
		}
		base.Status = Status.Active;
	}

	private void SelectIdleStance(StanceType currentStance)
	{
		StancesType stancesType = entity.EntityType.LocomotorType.StancesType;
		if (entityIntelligence.ThreatStance == ThreatStance.Bold)
		{
			statesToTake.Add(AnimModifier.Bold);
			stanceToTake = stancesType.DefaultStanceType;
			return;
		}
		ChanceToTakeStance[] stancesToSelectFrom;
		if (The.Map.GetTile(entity.MapPosition.Value).OperatingAreaOf == null || Common.DistanceOctile(entity.PlaySiteLocation, entityIntelligence.CurrentExpedition.Center.Value) > 400f)
		{
			statesToTake.Add(AnimModifier.Trouble);
			stancesToSelectFrom = stancesType.IdleStancesAwayFromHome;
		}
		else
		{
			stancesToSelectFrom = stancesType.IdleStancesNearHome;
		}
		StanceType stanceType = entity.Locomotor.Stance.PickRandomStance(stancesToSelectFrom, stancesType.DefaultStanceType);
		GetStatesToClear(stanceType, stancesType);
		stanceToTake = stanceType;
		if (stanceType.AnimModifier.HasValue)
		{
			statesToTake.Add(stanceType.AnimModifier.Value);
		}
		if (stanceToTake.CanStartIdleConversation == true)
		{
			bool? canSpeak = entity.EntityType.IntelligenceType.CanSpeak;
			bool flag = true;
			if (canSpeak == true == flag && canSpeak.HasValue && !HasTriedToStartConversation)
			{
				TryToStartConversation(entity);
			}
		}
		ChangeStance(stanceToTake);
	}

	private void GetStatesToClear(StanceType stanceToTake, StancesType stancesType)
	{
		foreach (StanceType stanceType in stancesType.StanceTypes)
		{
			if (stanceType != stanceToTake && stanceType.AnimModifier.HasValue)
			{
				statesToClear.Add(stanceType.AnimModifier.Value);
			}
		}
	}

	private void TryToStartConversation(Entity entity)
	{
		if (!(The.Sim.GameplayRandomGenerator.NextDouble("GoalDoTakeFive") < (double)entity.EntityType.IntelligenceType.IdleChanceToTalk))
		{
			return;
		}
		List<Collidable<Entity>> resultsList = null;
		The.CollisionManager.GetEntitiesInRange(entity.PlaySiteLocation.ToVector2(), 80f, (Entity e) => e != entity && e.EntityType.Person != null && e.Intelligence.IsIdle() && !e.Locomotor.IsMovingOrRotating, ref resultsList);
		if (resultsList != null && resultsList.Count > 0)
		{
			Entity entity2 = ((resultsList.Count <= 1) ? resultsList[0].Parent : Common.GetMinimum(resultsList, (Collidable<Entity> e) => Common.DistanceOctile(e.Parent.PlaySiteLocation, entity.PlaySiteLocation)).Parent);
			HasTriedToStartConversation = true;
			entity2.Intelligence.Brain.SendMessage(new Message(entity, Message.MessageTypes.StartConversation, null));
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
	}

	public override void OnExit()
	{
		base.OnExit();
		entity.Renderable.ClearAnimationActionStateFlag(AnimAction.Idle);
		entity.Renderable.ClearAnimationStateFlag(AnimModifier.Bold);
		entity.Renderable.ClearAnimationStateFlag(AnimModifier.Kneeling);
		entity.Renderable.ClearAnimationStateFlag(AnimModifier.Sitting);
		entity.Renderable.ClearAnimationStateFlag(AnimModifier.Lying);
		entity.Renderable.ClearAnimationStateFlag(AnimModifier.Talk);
		entity.Renderable.ClearAnimationStateFlag(AnimModifier.Trouble);
		entity.Renderable.ClearAnimationActionStateFlag(AnimAction.Smoking);
	}

	public override void Deactivate()
	{
		RemoveAllSubgoals();
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		Status status = ProcessSubgoals(elapsed);
		if (status == Status.Completed)
		{
			if (entityIntelligence.Brain.ArbitrateWhileBusy(out var _, useHighFrequency: true))
			{
				HandleSubstitutedGoalByArbitrator();
				return;
			}
			if (entity.Name != null)
			{
				entity.Name.Contains("Pezal");
			}
			if (TestForDanger && !ValidateSafetyAndTakeAction(GameData.Instance.AIConstants.HighestDiscomfortLevelForLeisureActivityToContinue))
			{
				return;
			}
			if (Subgoals.Count > 0 && Subgoals.Peek() is GoalDoTakeFiveAtomic)
			{
				GoalDoTakeFiveAtomic goalDoTakeFiveAtomic = (GoalDoTakeFiveAtomic)Subgoals.Peek();
				timeAlreadyRested = goalDoTakeFiveAtomic.TimeAlreadyRested;
			}
			if (timeAlreadyRested < duration)
			{
				if (ContainerIsTooCrowded())
				{
					AddSubgoal(new GoalExit(entity));
				}
				else
				{
					if (entity.HasStance())
					{
						entity.Locomotor.Stance.CurrentStance = stanceToTake;
					}
					foreach (AnimModifier item in statesToTake)
					{
						entity.Renderable.SetAnimationStateFlag(item);
					}
					foreach (AnimModifier item2 in statesToClear)
					{
						entity.Renderable.ClearAnimationStateFlag(item2);
					}
					entity.Renderable.SetAnimationActionStateFlag(AnimAction.Idle);
					AddSubgoal(GoalDoTakeFiveAtomic.GetGoal(entity, timeAlreadyRested, duration.Value));
				}
				base.Status = Status.Active;
			}
			else
			{
				base.Status = Status.Completed;
			}
		}
		else
		{
			base.Status = status;
		}
	}

	private bool ContainerIsTooCrowded()
	{
		if (base.entity.ContainedBy.HasValue && The.Sim.GameplayRandomGenerator.NextDouble("") < 0.08)
		{
			Entity entity = Entity.FindByID(base.entity.ContainedBy.Value);
			if (entity != null)
			{
				int num = 0;
				if (entity.Contains is IGarrison garrison)
				{
					num = garrison.GetNoOfAgentsInside();
				}
				int capacityForIdlingPeople = entity.EntityType.ContainerType.GetCapacityForIdlingPeople();
				if (num > capacityForIdlingPeople)
				{
					return true;
				}
			}
		}
		return false;
	}

	public override bool HandleMessage(Message message)
	{
		switch (message.MessageType)
		{
		case Message.MessageTypes.StartConversation:
			if (!HasTriedToStartConversation && !IsListening)
			{
				IsListening = true;
				AddSubgoal(new GoalWait(entity, TimeToWaitBeforeTurningBodyToListen));
				AddSubgoal(new GoalTurnToFace(entity, null, message.Sender.EntityID, turnCompletely: false, setCenterOfAttentionToTurnTarget: true, GameData.Instance.Constants.InterestLevelForConversation));
				message.Sender.Intelligence.Brain.SendMessage(new Message(entity, Message.MessageTypes.ListenToConversation, null));
				return true;
			}
			break;
		case Message.MessageTypes.ListenToConversation:
			if (HasTriedToStartConversation)
			{
				RemoveAllSubgoals();
				AddSubgoal(new GoalTurnToFace(entity, null, message.Sender.EntityID, turnCompletely: false, setCenterOfAttentionToTurnTarget: true, GameData.Instance.Constants.InterestLevelForConversation));
				entity.Renderable.SetAnimationStateFlag(AnimModifier.Talk);
				return true;
			}
			break;
		case Message.MessageTypes.Interest:
			HandleInterestMessage(message);
			return true;
		case Message.MessageTypes.SpeakLine:
			HandleSpeakLineMessage();
			return true;
		case Message.MessageTypes.CancelJobOrItemInUse:
			return true;
		case Message.MessageTypes.CancelJobForAIReset:
			return true;
		}
		return base.HandleMessage(message);
	}

	private void HandleSpeakLineMessage()
	{
		List<Pair<Entity, Vector2>> resultsList = null;
		The.AgentQuadTree.GetEntitiesInRange(entity.PlaySiteLocation.ToVector2(), 100f, (Entity e) => e != entity && e.Intelligence.Allegiance == entityIntelligence.Allegiance, ref resultsList);
		if (resultsList != null && resultsList.Count > 0)
		{
			AddSubgoal(new GoalTurnToFace(entity, null, resultsList[0].First.EntityID, turnCompletely: false, setCenterOfAttentionToTurnTarget: true, GameData.Instance.Constants.InterestLevelForConversation));
			entity.Renderable.SetAnimationStateFlag(AnimModifier.Talk);
		}
	}

	private void HandleInterestMessage(Message message)
	{
		if (entity.EntityType.Person == null || HasTriedToStartConversation || IsListening)
		{
			return;
		}
		EntityID? entityToFace;
		Vector3? location;
		float? interestAndHandleTrigger = GetInterestAndHandleTrigger(message, out entityToFace, out location);
		if (!interestAndHandleTrigger.HasValue || !entityIntelligence.InterestIsHighEnough(entityToFace, location, interestAndHandleTrigger.Value))
		{
			return;
		}
		bool? flag = null;
		bool? flag2 = null;
		if (entity.HasStance())
		{
			Locomotor locomotor = entity.Locomotor;
			flag = locomotor.Stance.CurrentStance.CanTurnBody;
			flag2 = locomotor.Stance.CurrentStance.CanTurnHead;
		}
		bool? flag3 = flag;
		bool flag4 = true;
		if (flag3 == true == flag4 && flag3.HasValue && interestAndHandleTrigger.Value >= GameData.Instance.Constants.InterestLevelToCauseBodyTurn)
		{
			bool turnCompletely = The.Sim.GameplayRandomGenerator.NextDouble(null) > 0.3;
			AddSubgoal(new GoalWait(entity, The.Sim.GameplayRandomGenerator.RandomBetween(0f, 0.9f)));
			if (entity.Name != null)
			{
				entity.Name.Contains("Ward");
			}
			RemoveAllSubgoals();
			AddSubgoal(new GoalTurnToFace(entity, location.HasValue ? new Vector2?(location.Value.ToVector2()) : ((Vector2?)null), entityToFace, turnCompletely, setCenterOfAttentionToTurnTarget: true, interestAndHandleTrigger));
		}
		else if (flag2 == true)
		{
			entityIntelligence.SetNewCenterOfAttention(entityToFace, location, interestAndHandleTrigger.Value);
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
		duration = sn.DoDoubleNullable(duration);
		timeAlreadyRested = sn.DoDouble(timeAlreadyRested);
		TestForDanger = sn.DoBool(TestForDanger);
		stanceToTake = sn.DoGameData(stanceToTake);
		statesToTake = sn.DoList(statesToTake);
		statesToClear = sn.DoList(statesToClear);
		HasTriedToStartConversation = sn.DoBool(HasTriedToStartConversation);
		IsListening = sn.DoBool(IsListening);
		sn.Ignore(5f);
		return this;
	}
}
