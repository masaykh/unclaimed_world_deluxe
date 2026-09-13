using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

public class GoalThink : CompositeGoal
{
	private enum ArbitrateMode
	{
		NoCurrentGoal,
		HasCurrentGoal
	}

	protected List<GoalEvaluator> evaluators = new List<GoalEvaluator>();

	private Regulator arbitrateRegulator;

	private Regulator arbitrateRegulatorWhileBusy;

	private bool waitingForEvaluator;

	private int evaluatorBeingProcessed;

	private GoalEvaluator previousMostDesirable;

	private double bestScore;

	private GoalEvaluator bestEvaluator;

	private bool hasArbitratedWhileBusy;

	private long lastUnpausedTimePoint;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public GoalThink(Entity entity)
		: base(entity)
	{
		ResetEvaluators();
	}

	public GoalThink()
	{
	}

	protected override void CreateRegulators()
	{
		base.CreateRegulators();
		arbitrateRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 2.0, "GoalDoThinkArbitrate");
		arbitrateRegulatorWhileBusy = new Regulator(The.Sim.GameplayRandomGenerator, GameData.Instance.AIConstants.NoOfTimesPerSecondToArbitrateWhileBusy, "GoalDoThinkArbitrateBusy");
	}

	public void ResetEvaluators()
	{
		waitingForEvaluator = false;
		evaluatorBeingProcessed = 0;
		The.Sim.WaitingAgents.Remove(entity.ID);
		List<AIAgeGroup> agegroups = new List<AIAgeGroup>
		{
			AIAgeGroup.Child,
			AIAgeGroup.YoungAdult,
			AIAgeGroup.Adult,
			AIAgeGroup.Old
		};
		List<AIAgeGroup> agegroups2 = new List<AIAgeGroup>
		{
			AIAgeGroup.YoungAdult,
			AIAgeGroup.Adult,
			AIAgeGroup.Old
		};
		List<AIAgeGroup> agegroups3 = new List<AIAgeGroup>
		{
			AIAgeGroup.Adult,
			AIAgeGroup.Old
		};
		List<AIAgeGroup> agegroups4 = new List<AIAgeGroup> { AIAgeGroup.Adult };
		Allegiance allegiance = entity.Intelligence.Allegiance;
		bool respectsOwnership = allegiance.RepresentativeEntityType.IntelligenceType.RespectsOwnership;
		List<EntityGroup> list = null;
		List<EntityGroup> list2 = null;
		List<EntityGroup> list3 = null;
		List<EntityGroup> list4 = null;
		List<EntityGroup> list5 = null;
		OwnerID? ownerOfCarcass = null;
		OwnerID? ownerID = null;
		EntityGroup entityGroup;
		EntityGroup itemGroup;
		if (respectsOwnership)
		{
			list = new List<EntityGroup>();
			list2 = new List<EntityGroup>();
			list4 = new List<EntityGroup>();
			list5 = new List<EntityGroup>();
			if (personEntity != null)
			{
				list3 = new List<EntityGroup>();
				list3.Add(personEntity.OwnedEntities);
				list3.Add(personEntity.Household.OwnedEntities);
				list4.Add(personEntity.Household.OwnedEntities);
				list.Add(personEntity.OwnedEntities);
				list.Add(personEntity.Household.OwnedEntities);
				list2 = new List<EntityGroup>();
				list2.Add(personEntity.OwnedEntities);
				list2.Add(personEntity.Household.OwnedEntities);
			}
			Expedition currentExpedition = entityIntelligence.CurrentExpedition;
			list.Add(currentExpedition.OwnedEntities);
			list2.Add(currentExpedition.OwnedEntities);
			list4.Add(currentExpedition.OwnedEntities);
			list5.Add(currentExpedition.OwnedEntities);
			ownerOfCarcass = ((ILookUp<IOwner, OwnerID>)currentExpedition).ID;
			ownerID = ((ILookUp<IOwner, OwnerID>)currentExpedition).ID;
			entityGroup = currentExpedition.OwnedEntities;
			itemGroup = currentExpedition.OwnedEntities;
		}
		else
		{
			entityGroup = allegiance.SharedKnowledge.AllKnownEntities;
			itemGroup = allegiance.SharedKnowledge.AllKnownEntities;
		}
		if (personEntity != null)
		{
			AddEvaluator(new EvaluateHaulingJobs(entity, personEntity.Household.OwnedEntities, personEntity.Household.OwnedEntities, list3), Sim.DayPhases.Leisure, agegroups);
			AddEvaluator(new EvaluateHaulingJobs(entity, personEntity.OwnedEntities, personEntity.OwnedEntities, list3), Sim.DayPhases.Leisure, agegroups);
			AddEvaluator(new EvaluateJob(entity, personEntity.Household.OwnedEntities, personEntity.Household.OwnedEntities, list3, ((ILookUp<IOwner, OwnerID>)personEntity.Household).ID), Sim.DayPhases.Leisure, agegroups2);
			AddEvaluator(new EvaluateLeisureWalk(entity, list4), Sim.DayPhases.Leisure, agegroups2);
			AddEvaluator(new EvaluateFindHome(entity), Sim.DayPhases.Leisure, agegroups3);
			AddEvaluator(new EvaluateEat(entity, list, list4));
			Sim.DayPhases sleepPhase = GetSleepPhase();
			if (entity.BiologicalEntity.Needs.NeedsList.ContainsKey("sleep"))
			{
				AddEvaluator(new EvaluateSleep(entity, list, list4), sleepPhase);
			}
		}
		EntityGroup ownedEntities = entityIntelligence.CurrentExpedition.OwnedEntities;
		if (entity.EntityType.IntelligenceType.CanHaul != false)
		{
			AddEvaluator(new EvaluateHaulingJobs(entity, entityGroup, ownedEntities, list5), Sim.DayPhases.Work, agegroups4);
		}
		AddEvaluator(new EvaluateTakeFive(entity));
		if (entity.EntityType.IntelligenceType.IsMobile)
		{
			AddEvaluator(new EvaluateReturnHome(entity, list));
		}
		if (entity.EntityType.IntelligenceType.CanAttack != false)
		{
			AddEvaluator(new EvaluateAttackJobs(entity, EvaluateAttackJobs.JobTypes.Threat, entity.Intelligence.Allegiance, null, ownerOfCarcass, list2, list), null, agegroups2);
			AddEvaluator(new EvaluateAttackJobs(entity, EvaluateAttackJobs.JobTypes.AssetThreat, entity.Intelligence.Allegiance, null, ownerOfCarcass, list2, list), null, agegroups2);
		}
		if (entityIntelligence.CanEmigrate())
		{
			AddEvaluator(new EvaluateEmigrate(entity), null, agegroups3);
		}
		if (entity.EntityType.IntelligenceType.CanDoJobs != false)
		{
			AddEvaluator(new EvaluateJob(entity, entityGroup, itemGroup, list, ownerID), Sim.DayPhases.Work, agegroups4);
		}
		bool? canScout = entity.EntityType.IntelligenceType.CanScout;
		bool flag = false;
		if (canScout == true != flag || !canScout.HasValue || entity.EntityType.IntelligenceType.CanExamine != false)
		{
			AddEvaluator(new EvaluateScoutingJobs(entity, entityGroup, list), Sim.DayPhases.Work, agegroups2);
		}
		if (entity.EntityType.IntelligenceType.CanHunt != false)
		{
			AddEvaluator(new EvaluateAttackJobs(entity, EvaluateAttackJobs.JobTypes.Hunt, entity.Intelligence.Allegiance, entityGroup, ownerID, list5, list5), Sim.DayPhases.Work, agegroups4);
		}
		if (entity.EntityType.Person == null && entity.BiologicalEntity != null && entity.BiologicalEntity.Needs != null)
		{
			if (entity.BiologicalEntity.Needs.NeedsList.Any((KeyValuePair<string, Need> n) => n.Value.NeedType.SleepNeedType != null))
			{
				Sim.DayPhases sleepPhase2 = GetSleepPhase();
				AddEvaluator(new EvaluateSleep(entity, list, null), sleepPhase2);
			}
			if (entity.BiologicalEntity.Needs.NeedsList.Any((KeyValuePair<string, Need> n) => n.Value.NeedType.FoodNeedType != null))
			{
				AddEvaluator(new EvaluateEat(entity, null, null));
			}
		}
		AddEvaluator(new EvaluateChangeThreatStance(entity));
	}

	private Sim.DayPhases GetSleepPhase()
	{
		if (entity.EntityType.BiologicalType.IsNocturnal)
		{
			return Sim.DayPhases.Work;
		}
		return Sim.DayPhases.Sleep;
	}

	public int GetIndexOfEvaluator(GoalEvaluator evaluator)
	{
		return evaluators.FindIndex((GoalEvaluator e) => e == evaluator);
	}

	public GoalEvaluator GetEvaluatorFromIndex(int index)
	{
		return evaluators[index];
	}

	protected GoalEvaluator AddEvaluator(GoalEvaluator evaluator, Sim.DayPhases? phase, List<AIAgeGroup> agegroups = null)
	{
		evaluators.Add(evaluator);
		if (phase.HasValue)
		{
			Sim.DayPhases value = phase.Value;
			if (value <= Sim.DayPhases.Sleep)
			{
				switch (value)
				{
				case Sim.DayPhases.Leisure:
					evaluator.ActiveInTime = new Tuple<double, double>(0.615, 0.923);
					break;
				case Sim.DayPhases.Work:
					evaluator.ActiveInTime = new Tuple<double, double>(0.23, 0.615);
					break;
				case Sim.DayPhases.Sleep:
					evaluator.ActiveInTime = new Tuple<double, double>(0.923, 0.23);
					break;
				}
			}
		}
		if (agegroups != null && entity.BiologicalEntity != null)
		{
			double? num = null;
			double? num2 = null;
			float num3 = 0f;
			int num4 = 0;
			foreach (AgeGroupType ageGroupType in entity.BiologicalEntity.CasteType.AgeGroupTypes)
			{
				if (num4 < agegroups.Count && ageGroupType.AIAgeGroup == agegroups[num4])
				{
					if (!num.HasValue)
					{
						num = num3;
					}
					num2 = ageGroupType.Edge;
					num4++;
				}
				num3 = ageGroupType.Edge;
			}
			if (num.HasValue && num2.HasValue)
			{
				evaluator.ActiveInAgeInterval = new Tuple<double, double>(num.Value, num2.Value);
			}
		}
		return evaluator;
	}

	protected GoalEvaluator AddEvaluator(GoalEvaluator evaluator)
	{
		evaluators.Add(evaluator);
		return evaluator;
	}

	protected override void Activate()
	{
		bool hasFailedSubgoal = false;
		CleanupSubgoals(base.Status, ref hasFailedSubgoal);
		if (arbitrateRegulator.IsReady())
		{
			entity.ToString().Contains("Lewis");
			Arbitrate(ArbitrateMode.NoCurrentGoal);
			base.Status = Status.Active;
		}
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
	}

	public Status ProcessThink(GameTime elapsed)
	{
		entity.ToString().Contains("onlan");
		if (!hasArbitratedWhileBusy)
		{
			ActivateIfInactive();
		}
		else
		{
			hasArbitratedWhileBusy = false;
		}
		entity.ToString().Contains("Millet");
		Status status = ProcessSubgoals(elapsed);
		if (!hasArbitratedWhileBusy && (status == Status.Completed || status == Status.Failed || IsIdle()))
		{
			entity.ToString().Contains("onlan");
			base.Status = Status.Inactive;
		}
		else
		{
			ITopLevelGoal topLevelGoal = GetTopLevelGoal();
			if (topLevelGoal != null)
			{
				topLevelGoal.TimeSpentInTopLevelGoal += elapsed.ElapsedGameTime.TotalSeconds;
			}
		}
		bool hasFailedSubgoal = false;
		CleanupSubgoals(base.Status, ref hasFailedSubgoal);
		return base.Status;
	}

	public bool ArbitrateWhileBusy(out ITopLevelGoal newGoal, bool useHighFrequency = false)
	{
		newGoal = null;
		if (Subgoals.Count == 0)
		{
			return false;
		}
		if (((useHighFrequency && arbitrateRegulator.IsReady()) || (!useHighFrequency && arbitrateRegulatorWhileBusy.IsReady())) && entityIntelligence.Brain.Arbitrate(ArbitrateMode.HasCurrentGoal))
		{
			if (entity.Name != null)
			{
				entity.Name.Contains("eboah");
			}
			newGoal = GetTopLevelGoal();
			hasArbitratedWhileBusy = true;
			return true;
		}
		return false;
	}

	protected ITopLevelGoal GetTopLevelGoal()
	{
		if (Subgoals.Count > 0)
		{
			return Subgoals.Peek() as ITopLevelGoal;
		}
		return null;
	}

	private bool Arbitrate(ArbitrateMode mode)
	{
		if (waitingForEvaluator)
		{
			return false;
		}
		entityIntelligence.TopScoringJobs.Clear();
		double num;
		if (mode == ArbitrateMode.NoCurrentGoal)
		{
			num = 0.0;
		}
		else
		{
			if (entity.Name != null)
			{
				entity.Name.Contains("eboah");
			}
			double? score = entityIntelligence.GetScore();
			num = (score.HasValue ? score.Value : 0.0);
		}
		while (evaluatorBeingProcessed < evaluators.Count)
		{
			GoalEvaluator goalEvaluator = evaluators[evaluatorBeingProcessed];
			if (!(num >= (double)(2f * goalEvaluator.Priority)) && goalEvaluator.IsActive)
			{
				double result = -1.0;
				if (goalEvaluator.CalculateDesirability(num, ref result) != GoalEvaluator.CalculateResult.Done)
				{
					waitingForEvaluator = true;
					The.Sim.AddWaitingAgent(entity, Sim.WaitingFor.Regions);
					return false;
				}
				if (result >= bestScore)
				{
					if (result > num && (entity.ID == (EntityID)5043L || entity.ID == (EntityID)4814L) && mode == ArbitrateMode.NoCurrentGoal)
					{
						_ = goalEvaluator is EvaluateHaulingJobs;
					}
					bestScore = result;
					bestEvaluator = goalEvaluator;
				}
				entityIntelligence.GetScore();
			}
			evaluatorBeingProcessed++;
		}
		if (entity.Name != null && !entity.Name.Contains("onlan"))
		{
			entity.Name.Contains("eboah");
		}
		if (bestEvaluator != null)
		{
			UpdateTimeSpentIdling(bestEvaluator);
			if (bestScore >= num)
			{
				switch (mode)
				{
				case ArbitrateMode.NoCurrentGoal:
				{
					string entityAIState2 = entity.Name + bestScore + bestEvaluator.ToString();
					if (bestEvaluator.CanTakeGoal() && bestEvaluator.CancelCurrentTakers())
					{
						The.Sim.Controller.SaveOrVerifyEntityAIState(entityAIState2);
						bestEvaluator.PreSetGoal();
						if (bestEvaluator.GetType() != typeof(EvaluateTakeFive))
						{
							RemoveAllSubgoals();
						}
						bestEvaluator.SetGoal();
						ResetScoreAndCounter();
						return true;
					}
					break;
				}
				case ArbitrateMode.HasCurrentGoal:
				{
					if (entity.Name != null)
					{
						_ = entity.PersonEntity;
					}
					entityIntelligence.GetScore();
					string entityAIState = entity.Name + bestScore + bestEvaluator.ToString();
					if (bestEvaluator.CanTakeGoal() && bestEvaluator.CancelCurrentTakers())
					{
						The.Sim.Controller.SaveOrVerifyEntityAIState(entityAIState);
						bestEvaluator.PreSetGoal();
						RemoveAllSubgoals();
						bestEvaluator.SetGoal();
						ResetScoreAndCounter();
						return true;
					}
					break;
				}
				}
			}
		}
		ResetScoreAndCounter();
		return false;
	}

	public override void RemoveAllSubgoals()
	{
		int count = Subgoals.Count;
		string arg = "";
		if (count > 0)
		{
			arg = Subgoals.Peek().ToString();
		}
		while (Subgoals.Count > 0)
		{
			RemoveFirstSubgoal().Terminate();
		}
		if (count > 0)
		{
			entity.DebugLog.Add($"GoalThink.RemoveAllSubgoals, count: {count}, first goal: {arg}");
		}
	}

	public double ScoreTopLevelGoal()
	{
		ITopLevelGoal topLevelGoal = GetTopLevelGoal();
		if (topLevelGoal != null)
		{
			Goal goal = topLevelGoal as Goal;
			if (base.Status == Status.Failed || base.Status == Status.Completed || goal.Status == Status.Failed || goal.Status == Status.Completed)
			{
				return 0.0;
			}
			double num = topLevelGoal.ScoreGoal();
			double num2 = ScoreInertia();
			return num + num2;
		}
		return 0.0;
	}

	private double ScoreInertia()
	{
		ITopLevelGoal topLevelGoal = GetTopLevelGoal();
		if (topLevelGoal != null)
		{
			double timeSpentInTopLevelGoal = topLevelGoal.TimeSpentInTopLevelGoal;
			if (timeSpentInTopLevelGoal < (double)GameData.Instance.AIConstants.TimeToReachFullGoalSwitchInertia)
			{
				float num = Common.Clamp((float)timeSpentInTopLevelGoal / GameData.Instance.AIConstants.TimeToReachFullGoalSwitchInertia, 0f, 1f);
				num *= num;
				return MathHelper.Lerp(0f, GameData.Instance.AIConstants.CurrentGoalInertia, num);
			}
			return GameData.Instance.AIConstants.CurrentGoalInertia;
		}
		return 0.0;
	}

	private void SortTopScoringJobsForDebugging()
	{
		entityIntelligence.TopScoringJobs.Sort((GoalAndScore j1, GoalAndScore j2) => j2.Score.CompareTo(j1.Score));
	}

	private void ResetScoreAndCounter()
	{
		bestEvaluator = null;
		bestScore = 0.0;
		evaluatorBeingProcessed = 0;
	}

	private void UpdateTimeSpentIdling(GoalEvaluator mostDesirable)
	{
		if (previousMostDesirable != null)
		{
			if (previousMostDesirable == mostDesirable)
			{
				long value = The.Sim.TotalUnPausedGameTime.Ticks - lastUnpausedTimePoint;
				lastUnpausedTimePoint = The.Sim.TotalUnPausedGameTime.Ticks;
				if (mostDesirable.IsIdleActivity())
				{
					entityIntelligence.Memory.TimeSpentIdling = entityIntelligence.Memory.TimeSpentIdling.Add(TimeSpan.FromTicks(value));
				}
				else
				{
					entityIntelligence.Memory.TimeSpentIdling = new TimeSpan(0L);
				}
			}
			else
			{
				entityIntelligence.Memory.TimeSpentIdling = new TimeSpan(0L);
			}
		}
		previousMostDesirable = mostDesirable;
	}

	public bool IsSame(Type type)
	{
		if (Subgoals.Count > 0)
		{
			return Subgoals.Peek().GetType() == type;
		}
		return false;
	}

	public new bool CanDropRequestedItem(Entity item)
	{
		if (Subgoals.Count > 0)
		{
			return Subgoals.Peek().CanDropRequestedItem(item);
		}
		return false;
	}

	private bool IsIdle()
	{
		if (Subgoals.Count > 0)
		{
			if (Subgoals.Peek() is GoalDoTakeFive)
			{
				return true;
			}
			return false;
		}
		return false;
	}

	public bool NotPresent(Type type)
	{
		if (Subgoals.Count > 0)
		{
			return Subgoals.Peek().GetType() != type;
		}
		return true;
	}

	public float GetExertionLevelOfActivity()
	{
		if (Subgoals.Count > 0)
		{
			return Subgoals.Peek().GetExertionLevel();
		}
		return 1f;
	}

	public float GetStealthFactorOfActivity()
	{
		float num = ((entity.EntityType.BiologicalType == null) ? GameData.Instance.Constants.DefaultPassiveStealthFactor : entity.BiologicalEntity.PassiveStealthRating);
		if (Subgoals.Count > 0)
		{
			switch (Subgoals.Peek().GetStealthFactor())
			{
			case StealthFactor.None:
				return num;
			case StealthFactor.NotGood:
				return num * 0.5f;
			case StealthFactor.Bad:
				return num * 0.25f;
			case StealthFactor.ExtremelyBad:
				return 0f;
			}
		}
		return num;
	}

	public float GetDetectionFactorOfActivity(IDetectable detectable, bool requiresExamineAction)
	{
		float bestDetectionFactorOfActivityWhenNotLooking = GameData.Instance.Constants.BestDetectionFactorOfActivityWhenNotLooking;
		if (Subgoals.Count > 0)
		{
			float bestDetectionFactorOfActivityWhenSearching = GameData.Instance.Constants.BestDetectionFactorOfActivityWhenSearching;
			DetectionFactor detectionFactor = ((!(detectable is Entity entity)) ? Subgoals.Peek().GetDetectResourcesFactor(detectable.ResourceType, requiresExamineAction) : Subgoals.Peek().GetDetectAgentsFactor(entity.EntityType, requiresExamineAction));
			float result = 0f;
			switch (detectionFactor)
			{
			case DetectionFactor.CannotDetect:
				result = 0f;
				break;
			case DetectionFactor.DetectSome:
				result = MathHelper.Lerp(bestDetectionFactorOfActivityWhenNotLooking, bestDetectionFactorOfActivityWhenSearching, 0f);
				break;
			case DetectionFactor.DetectGood:
				result = MathHelper.Lerp(bestDetectionFactorOfActivityWhenNotLooking, bestDetectionFactorOfActivityWhenSearching, 0.5f);
				break;
			case DetectionFactor.DetectVeryGood:
				result = MathHelper.Lerp(bestDetectionFactorOfActivityWhenNotLooking, bestDetectionFactorOfActivityWhenSearching, 1f);
				break;
			}
			return result;
		}
		return 0f;
	}

	public override void Terminate()
	{
		RemoveAllSubgoals();
		base.Terminate();
	}

	public bool SendMessage(Message msg)
	{
		bool num = HandleMessage(msg);
		if (num)
		{
			bool hasFailedSubgoal = false;
			CleanupSubgoals(base.Status, ref hasFailedSubgoal);
		}
		return num;
	}

	public override bool HandleMessage(Message message)
	{
		bool flag = ForwardMessageToFrontMostSubgoal(message);
		Message.MessageTypes messageType = message.MessageType;
		if ((uint)(messageType - 2) <= 2u)
		{
			waitingForEvaluator = false;
			The.Sim.WaitingAgents.Remove(base.entity.ID);
			return true;
		}
		if (!flag)
		{
			switch (message.MessageType)
			{
			case Message.MessageTypes.OtherAgentRequestsDropItem:
			{
				Entity entity = (Entity)message.OtherInfo;
				base.entity.AgentStorage.Uncontain(entity);
				base.entity.EntityType.IntelligenceType.EventActions.TryGetValue(AgentActionHooks.ForceDropsItem, out var value2);
				Goal.FireEventActions(base.entity, null, value2);
				return true;
			}
			case Message.MessageTypes.Hit:
			{
				float num = (float)message.OtherInfo;
				if (message.Sender != null && message.Sender.EntityType.IntelligenceType != null)
				{
					base.entity.Intelligence.Allegiance.ThreatAndCombatJobManager.CreateThreatJobsFromAttackOutOfBand(message.Sender, base.entity);
					base.entity.Intelligence.Memory.RememberAttacker(message.Sender.EntityID);
				}
				base.entity.Find<BodyComponent>(out var c);
				if (num > GameData.Instance.Constants.DamageAmountFractionCausingHitReaction * c.Body.MaxHitpoints)
				{
					if (The.Sim.TotalUnPausedGameTimeInSeconds > 17.0)
					{
						_ = base.entity.ID;
						_ = 19;
					}
					RemoveAllSubgoals();
					AddSubgoal(new GoalBeingHit(base.entity));
				}
				if (entityIntelligence.Statistics != null && Common.IsGreaterThan(num, 0.0))
				{
					base.entity.LogInjuryStatistics(message.Sender);
				}
				base.entity.EntityType.IntelligenceType.EventActions.TryGetValue(AgentActionHooks.TakingAHit, out var value);
				Goal.FireEventActions(base.entity, null, value);
				return true;
			}
			case Message.MessageTypes.HitAndCollapse:
			{
				RemoveAllSubgoals();
				OwnerID? ownerOfCarcass = (OwnerID?)message.OtherInfo;
				Entity sender = message.Sender;
				AddSubgoal(new GoalCollapse(base.entity, ownerOfCarcass, sender.EntityID));
				AddSubgoal(new GoalIsDying(base.entity, ownerOfCarcass, takeBleedDamage: true));
				return true;
			}
			case Message.MessageTypes.AlertToPresence:
				if (message.Sender != null)
				{
					entityIntelligence.Allegiance.SharedKnowledge.SeeDetectableIfRelevant(message.Sender, testForUsesMemory: true, suppressClientFeedback: false, null, base.entity);
				}
				return true;
			case Message.MessageTypes.Interest:
				if (GetFrontMostGoal().CanReactToInterest())
				{
					EntityID? entityID;
					Vector3? location;
					float? interestAndHandleTrigger = GetInterestAndHandleTrigger(message, out entityID, out location);
					if (interestAndHandleTrigger.HasValue)
					{
						entityIntelligence.SetNewCenterOfAttention(entityID, location, interestAndHandleTrigger.Value);
					}
				}
				return true;
			case Message.MessageTypes.Disembark:
				RemoveAllSubgoals();
				AddSubgoal(new GoalExit(base.entity, isDisembarking: true));
				return true;
			}
		}
		return flag;
	}

	public override void AddSubgoal(Goal g)
	{
		if (entity.PersonEntity != null)
		{
			_ = g is GoalWait;
		}
		bool flag = Subgoals.Count == 0;
		if (base.ID != GoalID.Invalid)
		{
			Subgoals.Enqueue(g);
			if (flag)
			{
				base.Status = Status.Active;
				g.EnterIfNew();
			}
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
		hasArbitratedWhileBusy = sn.DoBool(hasArbitratedWhileBusy);
		lastUnpausedTimePoint = sn.DoInt64(lastUnpausedTimePoint);
		sn.Ignore(previousMostDesirable);
		sn.Ignore(evaluators);
		sn.Ignore(waitingForEvaluator);
		sn.Ignore(evaluatorBeingProcessed);
		sn.Ignore(bestScore);
		sn.Ignore(bestEvaluator);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		base.LoadPostProcess(sn);
		if (evaluators.Count == 0)
		{
			ResetEvaluators();
		}
	}
}
