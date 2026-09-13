using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.AI.Pathfinding;
using UWGame.SimSide.AI.StrategicDecisions;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Combat;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Entities.Skills;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Processes;
using UWGame.SimSide.SimEffects;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems;
using UWGame.SimSide.Systems.TimeSlicing;
using UWGame.SimSide.Systems.Triggers;

namespace UWGame.SimSide.Entities;

public class Intelligence : Component
{
	public Allegiance Allegiance;

	private AllegianceID snapshotAllegianceID;

	public Expedition CurrentExpedition;

	private ExpeditionID? snapshotExpedition;

	public Memory Memory = new Memory();

	public CombatInfo CombatInfo;

	public ConversationID? CurrentConversationID;

	public Dictionary<EntityType, EntityID> IntrinsicTools;

	public Dictionary<EntityType, EntityID> IntrinsicWeapons;

	public float interestLevel;

	private EntityID entityIDToLookAt = EntityID.Invalid;

	private Vector3? locationToLookAt;

	private float lookAngle;

	private float maxAngleToTurnHead;

	public PathPlanner PathPlanner;

	private CyclableID? pathPlannerID;

	public ResourceMapForAgent CropsMapForAgent;

	private CyclableID? snapshotCropsMap;

	public GroupStatistics Statistics;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	private ThreatStance threatStance = ThreatStance.Normal;

	public bool AttacksVermin;

	private bool isStealthy;

	public bool IsAwakeAndActive = true;

	public ProtectionLevel ProtectionLevel;

	private float morale = 1f;

	public GoalThink Brain;

	private GoalID? snapshotBrain;

	public EmigrateDecider EmigrateDecider;

	public bool? IsAtGroupMoveDestination;

	public FollowerStatus FollowerStatus = FollowerStatus.Normal;

	public Regulator CurrentActionScoreRegulator;

	private Dictionary<TriggerID, long> triggersOnCooldown = new Dictionary<TriggerID, long>();

	public bool DisableAI;

	public Dictionary<SkillType, Skill> Skills = new Dictionary<SkillType, Skill>();

	private bool professionIsDirty = true;

	private ProfessionType profession;

	public string SpokenLine;

	private double spokenLineElapsedTime;

	private double spokenLineDuration;

	private bool panicLevelIsDirty = true;

	private int panicLevel;

	private Waypoint groupMoveAssignedWaypoint;

	public Vector3? GroupMoveAssignedWaypointRay;

	private double? currentGoalUtility;

	public List<GoalAndScore> TopScoringJobs = new List<GoalAndScore>();

	private ushort[][] evaluatorInfluenceMap;

	private Queue<Tuple<TimeSpan, float>> hitsTakenLastFewSeconds = new Queue<Tuple<TimeSpan, float>>();

	private Regulator combatStatRegulator;

	private float DPSmoraleDamageFactor;

	private Regulator triggerCleanupRegulator;

	private List<TriggerID> triggersToActivate = new List<TriggerID>();

	public string FirstName { get; private set; }

	public string LastName { get; private set; }

	public EntityID EntityIDToLookAt => entityIDToLookAt;

	public Vector3? LocationToLookAt => locationToLookAt;

	public float Comfort => Parent.GetEffect(AffectsNumbers.AgentComfort, 0f);

	public ThreatStance ThreatStance => threatStance;

	public bool IsStealthy
	{
		get
		{
			return isStealthy;
		}
		set
		{
			if (value != isStealthy)
			{
				isStealthy = value;
				if (isStealthy)
				{
					Parent.Renderable.SetAnimationStateFlag(AnimModifier.Stealthy);
				}
				else
				{
					Parent.Renderable.ClearAnimationStateFlag(AnimModifier.Stealthy);
				}
			}
		}
	}

	public float Morale
	{
		get
		{
			return morale;
		}
		set
		{
			if (value != morale)
			{
				morale = value;
				panicLevelIsDirty = true;
			}
		}
	}

	public ProfessionType Profession
	{
		get
		{
			if (professionIsDirty)
			{
				profession = null;
				foreach (KeyValuePair<SkillType, Skill> item in Skills.OrderByDescending((KeyValuePair<SkillType, Skill> kvp) => kvp.Value.Value))
				{
					if (item.Value.Value < GameData.Instance.Constants.ExpertSkillLevel)
					{
						break;
					}
					if (item.Key.ProfessionType != null)
					{
						profession = item.Key.ProfessionType;
						break;
					}
				}
				professionIsDirty = false;
			}
			return profession;
		}
	}

	public int PanicLevel
	{
		get
		{
			if (panicLevelIsDirty)
			{
				ComputePanicLevel();
				panicLevelIsDirty = false;
			}
			return panicLevel;
		}
	}

	public ushort[][] EvaluatorInfluenceMap
	{
		get
		{
			if (evaluatorInfluenceMap == null)
			{
				Common.InitJaggedArray(ref evaluatorInfluenceMap, The.Map.mapTileWidth, The.Map.mapTileHeight);
			}
			return evaluatorInfluenceMap;
		}
	}

	public Waypoint GroupMoveAssignedWaypoint
	{
		get
		{
			return groupMoveAssignedWaypoint;
		}
		set
		{
			groupMoveAssignedWaypoint = value;
			if (groupMoveAssignedWaypoint != null)
			{
				GroupMoveAssignedWaypointRay = GoalTraverseEdgeBetweenWaypoints.ComputeWaypointRay(groupMoveAssignedWaypoint.Location, Parent);
			}
			else
			{
				GroupMoveAssignedWaypointRay = null;
			}
		}
	}

	private bool GatherPolledStatistics
	{
		get
		{
			if (IsIndependent())
			{
				return Parent.IsOnPlaySite();
			}
			return false;
		}
	}

	public event Action TalkActionEnded;

	public Intelligence()
	{
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
		FirstName = sn.DoString(FirstName);
		LastName = sn.DoString(LastName);
		DisableAI = sn.DoBool(DisableAI);
		DPSmoraleDamageFactor = sn.DoFloat(DPSmoraleDamageFactor);
		entityIDToLookAt = sn.DoEntityID(entityIDToLookAt);
		evaluatorInfluenceMap = sn.DoJaggedArray(evaluatorInfluenceMap);
		GroupMoveAssignedWaypointRay = sn.DoVector3Nullable(GroupMoveAssignedWaypointRay);
		interestLevel = sn.DoFloat(interestLevel);
		IsAtGroupMoveDestination = sn.DoBoolNullable(IsAtGroupMoveDestination);
		IsAwakeAndActive = sn.DoBool(IsAwakeAndActive);
		isStealthy = sn.DoBool(isStealthy);
		locationToLookAt = sn.DoVector3Nullable(locationToLookAt);
		lookAngle = sn.DoFloat(lookAngle);
		maxAngleToTurnHead = sn.DoFloat(maxAngleToTurnHead);
		morale = sn.DoFloat(morale);
		panicLevel = sn.DoInt32(panicLevel);
		panicLevelIsDirty = sn.DoBool(panicLevelIsDirty);
		Skills = sn.DoDictionary(Skills);
		SpokenLine = sn.DoString(SpokenLine);
		spokenLineDuration = sn.DoDouble(spokenLineDuration);
		spokenLineElapsedTime = sn.DoDouble(spokenLineElapsedTime);
		AttacksVermin = sn.DoBool(AttacksVermin);
		triggersOnCooldown = sn.DoDictionary(triggersOnCooldown);
		ProtectionLevel = sn.DoEnum(ProtectionLevel);
		threatStance = sn.DoEnum(threatStance);
		FollowerStatus = sn.DoEnum(FollowerStatus);
		hitsTakenLastFewSeconds = sn.DoQueue(hitsTakenLastFewSeconds);
		snapshotBrain = sn.SnapshotID<Goal, GoalID>(Brain);
		CombatInfo = (CombatInfo)sn.DoISnapshot(CombatInfo);
		currentGoalUtility = sn.DoDoubleNullable(currentGoalUtility);
		Memory = (Memory)sn.DoISnapshot(Memory);
		CurrentConversationID = sn.DoEnumNullable(CurrentConversationID);
		snapshotAllegianceID = sn.SnapshotID<Allegiance, AllegianceID>(Allegiance).Value;
		snapshotCropsMap = sn.SnapshotID<ICyclable, CyclableID>(CropsMapForAgent);
		pathPlannerID = sn.SnapshotID<ICyclable, CyclableID>(PathPlanner);
		EmigrateDecider = (EmigrateDecider)sn.DoISnapshot(EmigrateDecider);
		Statistics = (GroupStatistics)sn.DoISnapshot(Statistics);
		snapshotExpedition = sn.SnapshotID<Expedition, ExpeditionID>(CurrentExpedition);
		IntrinsicTools = sn.DoDictionary(IntrinsicTools);
		IntrinsicWeapons = sn.DoDictionary(IntrinsicWeapons);
		profession = sn.DoGameData(profession);
		professionIsDirty = sn.DoBool(professionIsDirty);
		sn.Ignore(CropsMapForAgent);
		sn.Ignore(PathPlanner);
		sn.Ignore(triggersToActivate);
		sn.Ignore(TopScoringJobs);
		sn.Ignore(triggerCleanupRegulator);
		sn.Ignore(combatStatRegulator);
		sn.Ignore(CurrentActionScoreRegulator);
		sn.Ignore(Brain);
		sn.Ignore(this.TalkActionEnded);
		sn.Postpone(groupMoveAssignedWaypoint);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		base.LoadPostProcess(sn);
		CombatInfo.LoadPostProcess(sn);
		Memory.LoadPostProcess(sn);
		if (snapshotExpedition.HasValue)
		{
			CurrentExpedition = Expedition.FindByID(snapshotExpedition.Value);
		}
		if (pathPlannerID.HasValue)
		{
			PathPlanner = (PathPlanner)LookUp<ICyclable, CyclableID>.FindByID(pathPlannerID.Value);
		}
		if (snapshotBrain.HasValue)
		{
			Brain = (GoalThink)LookUpGoals.FindByID(snapshotBrain.Value);
		}
		Allegiance = LookUp<Allegiance, AllegianceID>.FindByID(snapshotAllegianceID);
		if (snapshotCropsMap.HasValue)
		{
			CropsMapForAgent = (ResourceMapForAgent)LookUp<ICyclable, CyclableID>.FindByID(snapshotCropsMap.Value);
		}
		if (EmigrateDecider != null)
		{
			EmigrateDecider.LoadPostProcess(sn);
		}
		foreach (KeyValuePair<SkillType, Skill> skill in Skills)
		{
			skill.Value.LoadPostProcess(sn);
		}
		Statistics.LoadPostProcess(sn);
		CreateRegulators();
	}

	public bool CanAttackVermin(out CombatAreaJob patrolJob)
	{
		patrolJob = null;
		if (Parent.EntityType.IntelligenceType.HuntsVermin)
		{
			return true;
		}
		patrolJob = Memory.GetLastCombatAreaJob();
		if (patrolJob != null && patrolJob.CanAttackVermin)
		{
			return true;
		}
		return AttacksVermin;
	}

	public float GetSkillValue(SkillType skillType)
	{
		if (skillType == null)
		{
			return 1f;
		}
		if (!Skills.TryGetValue(skillType, out var value))
		{
			return 0f;
		}
		float num = value.Value;
		if (skillType.GiveExpertSkillBonus)
		{
			float num2 = 1f - GameData.Instance.Constants.ExpertSkillBonusThreshold;
			float num3 = (num - GameData.Instance.Constants.ExpertSkillBonusThreshold) / num2;
			if (num3 > 0f)
			{
				num = MathHelper.Lerp(GameData.Instance.Constants.ExpertSkillBonusThreshold, 1f + GameData.Instance.Constants.ExpertSkillBonus, num3);
			}
		}
		return num;
	}

	public void SetSkillValue(SkillType skillType, float value)
	{
		if (Skills.TryGetValue(skillType, out var value2))
		{
			value2.Value = value;
		}
		professionIsDirty = true;
	}

	public float GetSkillProductionFactor(SkillType skillType)
	{
		if (skillType == null)
		{
			return 1f;
		}
		if (!Skills.TryGetValue(skillType, out var value))
		{
			return 0f;
		}
		return value.ProductionFactor;
	}

	public double? GetCurrentGoalUtility()
	{
		return currentGoalUtility;
	}

	public bool SetTopLevelGoal(ITopLevelGoal g, double score)
	{
		Goal goal = g as Goal;
		if (Brain.NotPresent(g.GetType()))
		{
			Brain.AddSubgoal(goal);
			goal.ActivateIfInactive();
			currentGoalUtility = score;
			Parent.DebugLog.Add($"SetTopLevelGoal: {goal.ToString()}");
			return true;
		}
		goal.RemoveIDEntry();
		return false;
	}

	public EntityResult GetKnownData(EntityID entityID, out IKnownEntityData data)
	{
		return Allegiance.SharedKnowledge.GetKnownData(entityID, out data);
	}

	public ProcessResult GetKnownProcessData(SimProcessID processID, out IKnownProcess data)
	{
		return Allegiance.SharedKnowledge.PlaySiteKnowledge.GetKnownProcessData(processID, out data);
	}

	public bool GetEntitySeenDirectly(EntityID entityID, out Entity entity)
	{
		if (GetKnownData(entityID, out var data) != EntityResult.SeenDirectly)
		{
			entity = null;
			return false;
		}
		entity = (Entity)data;
		return true;
	}

	private void ComputePanicLevel()
	{
		if (!Parent.EntityType.IntelligenceType.CanPanic || !Parent.EntityType.IntelligenceType.IsMobile)
		{
			panicLevel = 100;
		}
		else if (!CanDefend() && !CanAttack())
		{
			panicLevel = 4;
		}
		else
		{
			Parent.Find<BodyComponent>(out var c);
			float hitpointsFractionUntilUnconsciousness = c.Body.GetHitpointsFractionUntilUnconsciousness();
			float hurtVitalBodyPartFactorForMorale = c.Body.GetHurtVitalBodyPartFactorForMorale();
			float amount = hitpointsFractionUntilUnconsciousness * Morale * hurtVitalBodyPartFactorForMorale;
			panicLevel = (int)MathHelper.Lerp(4f, 100f, amount);
		}
		panicLevelIsDirty = false;
		if (threatStance != ThreatStance.Bold)
		{
			SetNormalOrCautiousThreatStance();
		}
	}

	public void SetSkill(string skillKey, float value)
	{
		SkillType skillType = GameData.Instance.AllSkillTypes[skillKey];
		if (!Skills.TryGetValue(skillType, out var value2))
		{
			value2 = new Skill(value, skillType);
			Skills.Add(skillType, value2);
		}
		value2.Value = value;
	}

	private void SetNormalOrCautiousThreatStance()
	{
		if (StanceShouldBeCautious())
		{
			threatStance = ThreatStance.Cautious;
			panicLevel = Common.ClampBottom(panicLevel, 4);
		}
		else
		{
			threatStance = ThreatStance.Normal;
		}
	}

	public void SetBoldStance()
	{
		threatStance = ThreatStance.Bold;
	}

	public void SetLastAgentOnPost(CombatAreaJob job)
	{
		Memory.SetRecentlyOnPatrol(job.ID);
		job.SetLastAgentOnPost(Parent.ID);
	}

	public void ResetLastAgentOnPost(CombatAreaJob job)
	{
		Memory.SetRecentlyOnPatrol(null);
		job.RemoveLastAgentOnPost(Parent.ID);
	}

	public bool IsSleeping()
	{
		if (Brain != null && Brain.Subgoals.Count > 0 && Brain.Subgoals.Peek() is GoalSleep && (Brain.Subgoals.Peek() as GoalSleep).Status != Status.Completed)
		{
			return true;
		}
		return false;
	}

	public bool IsIdle()
	{
		if (Brain.Subgoals.Count > 0)
		{
			return Brain.Subgoals.Peek() is GoalTakeFive;
		}
		return false;
	}

	public bool IsEmigrating()
	{
		if (Brain.Subgoals.Count > 0)
		{
			return Brain.Subgoals.Peek() is GoalEmigrate;
		}
		return false;
	}

	public bool IsAttacking()
	{
		return CombatInfo.Target.HasValue;
	}

	public bool IsFleeing()
	{
		return Parent.Locomotor.LeggedLocomotor.TargetSpeed == Goal.MovementSpeeds.Run;
	}

	public Intelligence(Entity parent)
		: base(parent)
	{
		CombatInfo = new CombatInfo();
		if (parent.EntityType.IntelligenceType.IntrinsicToolTypes != null)
		{
			IntrinsicTools = new Dictionary<EntityType, EntityID>();
		}
		if (parent.EntityType.IntelligenceType.IntrinsicWeaponTypes != null)
		{
			IntrinsicWeapons = new Dictionary<EntityType, EntityID>();
		}
		CreateRegulators();
	}

	public void Initialize(Allegiance allegiance)
	{
		if (allegiance == null)
		{
			if (The.Sim.Mode == Sim.EngineMode.Game)
			{
				throw new Exception("Allegiance has not been set on the Entity");
			}
		}
		else
		{
			Allegiance = allegiance;
			Allegiance.AddMember(Parent);
		}
		Statistics = new GroupStatistics(Parent.CanIterateEntitiesID, allegiance);
	}

	public void NotifyFoodProcessesChanged()
	{
		if (Allegiance != null)
		{
			Allegiance.FoodExtraction.SetIsDirty();
		}
	}

	public void SpeakLine(string lineKey, string defaultText, float durationInSeconds, bool turnTowardsListeners, Conversation conversation)
	{
		string text = null;
		text = ((string.IsNullOrEmpty(lineKey) || !Parent.Find<Person>(out var c)) ? defaultText : c.Personality.PersonalityType.GetSpokenLine(lineKey, defaultText));
		The.Client.SpeakLine(Parent, durationInSeconds, conversation, text);
		SpokenLine = text;
		spokenLineDuration = durationInSeconds;
		spokenLineElapsedTime = 0.0;
		CurrentConversationID = conversation.ID;
		if (turnTowardsListeners && Brain != null)
		{
			Brain.SendMessage(new Message(Message.MessageTypes.SpeakLine));
		}
	}

	private void CalculateDamagePerSecondTaken()
	{
		DPSmoraleDamageFactor = hitsTakenLastFewSeconds.Sum((Tuple<TimeSpan, float> t) => t.Item2);
		DPSmoraleDamageFactor *= 0.5f;
	}

	public void DamageMorale(float physicalDamage, BodyPart hitBodyPart)
	{
		float num = 0f;
		float num2 = 0f;
		if (Parent.Find<BodyComponent>(out var c) && c.Body.GlobalHitpoints > 0f)
		{
			num2 = physicalDamage / c.Body.GlobalHitpoints;
		}
		if (hitBodyPart.IsVital())
		{
			float num3 = GameData.Instance.Constants.vitalBodyPartDamageEvaluationBoost * hitBodyPart.GetMoraleDecreaseFactor(physicalDamage);
			if (num3 != 0f)
			{
				num2 *= num3;
			}
		}
		num2 = Common.ClampBottom(num2, GameData.Instance.Constants.MoraleDamageForZeroDamageAttacks);
		hitsTakenLastFewSeconds.Enqueue(new Tuple<TimeSpan, float>(The.Sim.TotalUnPausedGameTime.Add(new TimeSpan(0, 0, GameData.Instance.Constants.TimeIntervalForDPSMoraleFactor)), num2));
		CalculateDamagePerSecondTaken();
		num = DPSmoraleDamageFactor;
		num *= 1f - Parent.EntityType.IntelligenceType.Courage;
		Morale = Common.ClampBottom(Morale - num, 0f);
	}

	public void SetPanicLevelDirty()
	{
		panicLevelIsDirty = true;
	}

	public void SetTriggerCooldown(Trigger trigger, long? periodInTicks)
	{
		if (periodInTicks.HasValue && !triggersOnCooldown.ContainsKey(trigger.ID))
		{
			long value = The.Sim.TotalUnPausedGameTime.Ticks + periodInTicks.Value;
			triggersOnCooldown.Add(trigger.ID, value);
		}
	}

	public bool IsReadyToHandleTrigger(Trigger triggerToCheck)
	{
		if (!triggersOnCooldown.TryGetValue(triggerToCheck.ID, out var value))
		{
			return true;
		}
		if (The.Sim.TotalUnPausedGameTime.Ticks > value)
		{
			triggersOnCooldown.Remove(triggerToCheck.ID);
			return true;
		}
		return false;
	}

	public bool HasSkill(SkillType skillType)
	{
		if (skillType == null)
		{
			return true;
		}
		if (Skills.TryGetValue(skillType, out var value))
		{
			return value.Value > GameData.Instance.Constants.MinimumSkillValueToUse;
		}
		return false;
	}

	public Skill GetSkill(SkillType skillType)
	{
		if (Skills.TryGetValue(skillType, out var value))
		{
			return value;
		}
		return null;
	}

	public void ComeOnline()
	{
		if (Parent.IsOnPlaySite())
		{
			if (Parent.EntityType.IntelligenceType.IsMobile)
			{
				if (PathPlanner == null)
				{
					PathPlanner = new PathPlanner(Parent);
				}
				if (CropsMapForAgent == null)
				{
					CropsMapForAgent = new ResourceMapForAgent(Parent);
				}
			}
			if (Brain == null)
			{
				Brain = new GoalThink(Parent);
			}
		}
		if (CanEmigrate() && EmigrateDecider == null)
		{
			EmigrateDecider = new EmigrateDecider(Parent);
		}
		if (!GatherPolledStatistics && IsIndependent())
		{
			Statistics.UpdateOnce();
		}
	}

	public bool CanEmigrate()
	{
		if (IsIndependent())
		{
			return Parent.EntityType.IntelligenceType.CanEmigrate == true;
		}
		return false;
	}

	public AllegianceRatings GetRatingsForAllegiance(AllegianceID allegiance)
	{
		AllegianceRatings result = null;
		if (EmigrateDecider != null)
		{
			return EmigrateDecider.GetRatings(allegiance);
		}
		return result;
	}

	public bool HasDesireToEmigrate(Allegiance toAllegiance)
	{
		if (EmigrateDecider != null)
		{
			return EmigrateDecider.HasDesireToEmigrate(toAllegiance);
		}
		return false;
	}

	public void ResetThreatStance()
	{
		SetNormalOrCautiousThreatStance();
	}

	private bool CanDefend()
	{
		if (Parent.EntityType.IntelligenceType.DefendActionTypes != null)
		{
			Parent.Find<BodyComponent>(out var c);
			DefendActionType[] defendActionTypes = Parent.EntityType.IntelligenceType.DefendActionTypes;
			foreach (DefendActionType defendType in defendActionTypes)
			{
				if (IntelligenceType.DefendActionTypeIsFunctional(c.Body, defendType))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool CanAttack()
	{
		if (Parent.EntityType.IntelligenceType.AttackTypes != null)
		{
			Parent.Find<BodyComponent>(out var c);
			foreach (AttackType attackType in Parent.EntityType.IntelligenceType.AttackTypes)
			{
				if (IntelligenceType.AttackTypeIsFunctional(c.Body, attackType))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool StanceCanBeBold()
	{
		return PanicLevel >= 12;
	}

	public bool StanceShouldBeCautious()
	{
		return PanicLevel < 12;
	}

	public void SetName(string firstName, string lastName)
	{
		FirstName = firstName;
		LastName = lastName;
		string text = ((lastName == null) ? FirstName : (FirstName + " " + LastName));
		text = text.Trim();
		Parent.Name = text;
	}

	public double? GetScore()
	{
		if (CurrentActionScoreRegulator.IsReady())
		{
			currentGoalUtility = Brain.ScoreTopLevelGoal();
		}
		return currentGoalUtility;
	}

	public void UpdateOtherSite(GameTime time)
	{
	}

	public void PerformWorkAndAffectHandTools(SimProcess process, float workedTimeInSeconds, float progressDelta)
	{
		Brain.PerformWork(process, workedTimeInSeconds, progressDelta);
	}

	private void UpdateCommonSystems(GameTime time)
	{
		if (Parent.Name != null)
		{
			Parent.Name.Contains("Tamara");
		}
		if (GatherPolledStatistics)
		{
			Statistics.Update(time);
		}
		if (Parent.PersonEntity != null)
		{
			EmigrateDecider.Update(time);
		}
	}

	private void UpdatePlaySiteSystemsOnly(GameTime time)
	{
		Brain.ProcessThink(time);
		if (Parent.EntityID != EntityID.Invalid)
		{
			if (Allegiance != null)
			{
				Allegiance.SharedKnowledge.PlaySiteKnowledge.KnownEntityDataTree.UpdateObject(Parent.EntityID, Parent.PlaySiteLocation.ToVector2());
			}
			if (CropsMapForAgent != null)
			{
				CropsMapForAgent.Update(time);
			}
			UpdateCombatStats();
			UpdateInterest(time);
			UpdateSpokenLine(time);
			if (triggerCleanupRegulator.IsReady())
			{
				CleanupTriggersOnCooldown();
			}
		}
	}

	private void UpdateCombatStats()
	{
		if (!combatStatRegulator.IsReadyGetTimeElapsedInSeconds(out var secondsSinceLastReady))
		{
			return;
		}
		if (hitsTakenLastFewSeconds.Count > 0)
		{
			int count = hitsTakenLastFewSeconds.Count;
			while (hitsTakenLastFewSeconds.Count > 0 && hitsTakenLastFewSeconds.Peek().Item1 < The.Sim.TotalUnPausedGameTime)
			{
				hitsTakenLastFewSeconds.Dequeue();
			}
			if (count != hitsTakenLastFewSeconds.Count)
			{
				CalculateDamagePerSecondTaken();
			}
		}
		Morale = Common.IncreaseValueBetweenZeroAndOne(Morale, Parent.EntityType.IntelligenceType.MoraleIncreasePerDay, secondsSinceLastReady);
	}

	public override void Update(GameTime time)
	{
		UpdateCommonSystems(time);
		if (Parent.IsOnPlaySite())
		{
			UpdatePlaySiteSystemsOnly(time);
		}
		else
		{
			UpdateOtherSite(time);
		}
	}

	private void UpdateSpokenLine(GameTime time)
	{
		if (string.IsNullOrEmpty(SpokenLine))
		{
			return;
		}
		spokenLineElapsedTime += time.ElapsedGameTime.TotalSeconds;
		if (spokenLineElapsedTime >= spokenLineDuration)
		{
			SpokenLine = null;
			spokenLineElapsedTime = 0.0;
			if (this.TalkActionEnded != null)
			{
				this.TalkActionEnded();
			}
			if (CurrentConversationID.HasValue)
			{
				LookUp<Conversation, ConversationID>.FindByID(CurrentConversationID.Value)?.TalkActionEnded();
			}
		}
	}

	private void UpdateInterest(GameTime time)
	{
		if (interestLevel <= 0f)
		{
			interestLevel = 0f;
			HandleNoInterest(time);
			return;
		}
		interestLevel -= (float)time.ElapsedGameTime.TotalSeconds * GameData.Instance.Constants.InterestLevelDropOffPerSecond;
		Vector2? lookAt = GetPointToLookAt();
		if (!lookAt.HasValue || Common.DistanceOctile(lookAt.Value, GetLocationForHeadAndBodyTurn()) < 2f)
		{
			interestLevel = 0f;
			lookAt = null;
		}
		if (lookAt.HasValue)
		{
			HandleTurnToInterest(lookAt);
		}
	}

	public bool HasHappiness()
	{
		if (IsIndependent())
		{
			return Parent.PersonEntity != null;
		}
		return false;
	}

	public bool IsIndependent()
	{
		if (Parent.EntityType.IntelligenceType.ServantForEntityTypeTag != null && Allegiance.RepresentativeEntityType.IntelligenceType.HasServants != null && Allegiance.RepresentativeEntityType.IntelligenceType.HasServants.Contains(Parent.EntityType))
		{
			return false;
		}
		return true;
	}

	public void ClearCenterOfAttention()
	{
		interestLevel = 0f;
	}

	public void HandleNoInterest(GameTime gameTime)
	{
		entityIDToLookAt = EntityID.Invalid;
		locationToLookAt = null;
		if (!Common.IsZero(lookAngle))
		{
			Vector2 lookTarget = (Parent.FacingNormal + Parent.PlaySiteLocation).ToVector2();
			UpdateHeadTurnAngles(lookTarget, interestLevel, out var lookTargetAngle, out var isLookingAtTarget, out var _);
			Parent.Renderable.TurnToLook(lookTarget, lookAngle, lookTargetAngle);
			if (isLookingAtTarget)
			{
				lookAngle = 0f;
				Parent.Renderable.StartLerpingBackSpine();
			}
		}
		else
		{
			Parent.Renderable.UpdateLerpingBackSpine(gameTime);
		}
	}

	public bool InterestIsHighEnough(EntityID? id, Vector3? location, float interestLevel)
	{
		if (id == Parent.EntityID)
		{
			return false;
		}
		if (location.HasValue)
		{
			Vector3 value = location.Value;
			Vector3? location2 = Parent.Location;
			if (value == location2)
			{
				return false;
			}
		}
		if (interestLevel > 0f)
		{
			bool flag = GameData.Instance.Constants.InterestInertiaFactor * this.interestLevel > interestLevel;
			bool num = entityIDToLookAt == id;
			bool flag2 = entityIDToLookAt != EntityID.Invalid;
			bool hasValue = locationToLookAt.HasValue;
			if (num || ((flag2 || hasValue) && flag))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public bool SetNewCenterOfAttention(EntityID? id, Vector3? location, float interestLevel)
	{
		if (!InterestIsHighEnough(id, location, interestLevel))
		{
			return false;
		}
		maxAngleToTurnHead = GetMaxAngleFromInterest(interestLevel);
		if (id.HasValue)
		{
			entityIDToLookAt = id.Value;
		}
		else
		{
			entityIDToLookAt = EntityID.Invalid;
		}
		locationToLookAt = location;
		this.interestLevel = interestLevel;
		return true;
	}

	private float GetMaxAngleFromInterest(float interestLevel)
	{
		return MathHelper.Lerp(GameData.Instance.Constants.MaxHeadTurnAngleForZeroInterest, GameData.Instance.Constants.MaxHeadTurnAngleForMaxInterest, Common.Clamp(interestLevel, 0f, 1f));
	}

	private void HandleTurnToInterest(Vector2? lookAt)
	{
		if (Parent.EntityType.RenderableTypeMode.AnimatedHeadType != null)
		{
			UpdateHeadTurnAngles(lookAt.Value, interestLevel, out var lookTargetAngle, out var _, out var isOutOfView);
			if (isOutOfView)
			{
				interestLevel = 0f;
			}
			else
			{
				Parent.Renderable.TurnToLook(lookAt.Value, lookAngle, lookTargetAngle);
			}
		}
	}

	private void UpdateHeadTurnAngles(Vector2 lookTarget, float? interestLevel, out float lookTargetAngle, out bool isLookingAtTarget, out bool isOutOfView)
	{
		isLookingAtTarget = false;
		isOutOfView = false;
		float num = Common.VectorToAngle(Parent.FacingNormal.ToVector2());
		float num2 = num + lookAngle;
		Vector2 vector = lookTarget - GetLocationForHeadAndBodyTurn();
		vector.Normalize();
		lookTargetAngle = Common.VectorToAngle(vector);
		float num3 = Math.Abs(MathHelper.WrapAngle(num2 - lookTargetAngle));
		float num4 = Math.Abs(MathHelper.WrapAngle(num - lookTargetAngle));
		if (num3 < 0.05f)
		{
			isLookingAtTarget = true;
			return;
		}
		if (num4 > maxAngleToTurnHead)
		{
			isOutOfView = true;
			return;
		}
		float targetValue = MathHelper.WrapAngle(lookTargetAngle - num);
		float turnToLookLerpFactor = Parent.EntityType.RenderableTypeMode.AnimatedHeadType.TurnToLookLerpFactor;
		lookAngle = Common.EaseInValueTowardsTarget(lookAngle, targetValue, turnToLookLerpFactor);
	}

	private Vector2 GetLocationForHeadAndBodyTurn()
	{
		return Parent.PlaySiteLocation.ToVector2();
	}

	private Vector2? GetPointToLookAt()
	{
		if (entityIDToLookAt != EntityID.Invalid)
		{
			GetKnownData(entityIDToLookAt, out var data);
			if (data != null && data.Location.HasValue)
			{
				return data.PlaySiteLocation.ToVector2();
			}
			entityIDToLookAt = EntityID.Invalid;
			interestLevel = 0f;
			return null;
		}
		if (locationToLookAt.HasValue)
		{
			return locationToLookAt.Value.ToVector2();
		}
		return null;
	}

	private void CreateRegulators()
	{
		triggerCleanupRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 0.5, "IntelligenceTriggerCleanup");
		CurrentActionScoreRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 3.0, "IntelligenceCurrentActionsScoreRegulator");
		combatStatRegulator = new Regulator(The.Sim.GameplayRandomGenerator, 1.0, "IntelligenceDamagePerSecond");
	}

	public override double? GetUpdateInterval()
	{
		if (!Parent.IsDead && Parent.IsCompleted())
		{
			if (Parent.IsOnPlaySite())
			{
				return 0.0;
			}
			double? currentInterval = null;
			if (EmigrateDecider != null)
			{
				UpdateTimePoints.GetSoonestInterval(EmigrateDecider.GetUpdateInterval(), ref currentInterval);
			}
			return currentInterval;
		}
		return null;
	}

	private void CleanupTriggersOnCooldown()
	{
		if (triggersOnCooldown.Count <= 0)
		{
			return;
		}
		long ticks = The.Sim.TotalUnPausedGameTime.Ticks;
		triggersToActivate.Clear();
		foreach (KeyValuePair<TriggerID, long> item in triggersOnCooldown)
		{
			if (item.Value < ticks)
			{
				triggersToActivate.Add(item.Key);
			}
		}
		foreach (TriggerID item2 in triggersToActivate)
		{
			triggersOnCooldown.Remove(item2);
		}
	}

	public void Destroy()
	{
		if (Brain != null)
		{
			Brain.RemoveAllSubgoals();
			Brain.RemoveIDEntry();
			Brain.Terminate();
		}
		if (Allegiance != null)
		{
			Allegiance.RemoveMember(Parent, isDestroyed: true);
			Allegiance.LetOtherAllegiancesSeeEntity(Parent, seeEntity: false);
			if (Allegiance.Members.Count == 0 && Allegiance.AllegianceType != AllegianceType.Player && !Allegiance.Expeditions.Any((Expedition e) => e.Population != null && (e.Population.GrowthInMembersPerDay.HasValue || e.Population.GrowthInPercentagePerDay.HasValue)))
			{
				Allegiance.Destroy();
			}
			Allegiance = null;
		}
		if (PathPlanner != null)
		{
			PathPlanner.Destroy();
		}
		if (CropsMapForAgent != null)
		{
			CropsMapForAgent.Destroy();
		}
		if (CurrentExpedition != null)
		{
			CurrentExpedition.RemoveMember(Parent);
		}
	}

	public bool IsReadyForEmbark(Allegiance toAllegiance)
	{
		if (Allegiance == toAllegiance)
		{
			return false;
		}
		if (EmigrateDecider.CanEmigrateToAnyTarget())
		{
			AllegianceRatings ratingsForAllegiance = GetRatingsForAllegiance(toAllegiance.ID);
			if (ratingsForAllegiance != null && ratingsForAllegiance.Desirability > 0f)
			{
				return true;
			}
		}
		return false;
	}

	public bool CanDropRequestedItem(Entity item)
	{
		if (Brain != null)
		{
			return Brain.CanDropRequestedItem(item);
		}
		return false;
	}
}
