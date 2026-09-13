using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.InGameEvents.Actions;

public class TalkAction : EventActionType
{
	public enum TalkActionPriority
	{
		Low,
		Normal,
		High
	}

	public enum SpeakerInConversation
	{
		First,
		Second,
		Third
	}

	public bool TurnTowardsListeners;

	public TalkActionPriority TalkPriority = TalkActionPriority.Normal;

	public SpeakerInConversation? SpeakerDenomination;

	public float? Duration;

	public ActionByAgent ActionByAgent;

	public string NameOfSpeaker;

	public string Allegiance;

	public string LineKey;

	public string DefaultText;

	public bool CanTalkWhileSleeping;

	public bool CanTalkWhileFighting;

	public bool CanTalkWhileThreatened;

	public bool CanTalkWhileEmigrating;

	public bool CanTalkWhileTravelling;

	public Condition CanTalk;

	[XmlIgnore]
	public int LineNo { get; private set; }

	public override bool UsesTriggeringEntity
	{
		get
		{
			if (ActionByAgent != ActionByAgent.OnlyTriggeringEntity)
			{
				return ActionByAgent == ActionByAgent.PreferTriggeringEntity;
			}
			return true;
		}
	}

	public TalkAction(string keyName)
		: base(keyName)
	{
	}

	public TalkAction()
	{
	}

	public void SetLineNo(int lineNo)
	{
		LineNo = lineNo;
	}

	public override bool Execute(EventAction eventAction, ref string failReason)
	{
		ActionSetData actionSetData = LookUp<ActionSetData, ActionSetDataID>.FindByID(eventAction.ParentID.Value);
		if (!actionSetData.IsConversationValid(this))
		{
			failReason = "Conversation not valid";
			actionSetData.Conversation.EndConversation();
			return false;
		}
		if (!actionSetData.ConversationHasStarted)
		{
			if (LineNo > 0)
			{
				return false;
			}
			if (IsMoreImportantConversationActive())
			{
				failReason = "A conversation of same or higher prio is still active";
				return false;
			}
		}
		else if (!actionSetData.Conversation.IsTalkActionNextInLine(this))
		{
			actionSetData.Conversation.EndConversation();
			return false;
		}
		EntityID? entityID = FindSpeaker(eventAction, actionSetData);
		if (entityID.HasValue)
		{
			Entity entity = Entity.FindByID(entityID.Value);
			if (entity != null)
			{
				actionSetData.Conversation.SpeakLine(this, entity);
				entity.Intelligence.SpeakLine(LineKey, DefaultText, Duration.Value, TurnTowardsListeners, actionSetData.Conversation);
				return true;
			}
		}
		else
		{
			failReason = "No speaker found";
			actionSetData.Conversation.EndConversation();
		}
		return false;
	}

	private bool IsMoreImportantConversationActive()
	{
		return GetAllegianceIfItCanSpeak()?.Members.Any((Entity e) => EntityTypeCanSpeak(e.EntityType) && IsHavingSameOrMoreImportantConversation(e.Intelligence)) ?? false;
	}

	private EntityID? FindSpeaker(EventAction eventAction, ActionSetData actionData)
	{
		EntityID? triggeringEntity = eventAction.TriggeringEntity;
		if (SpeakerDenomination.HasValue && actionData.ConversationHasStarted && actionData.Conversation.AssignedSpeakers.TryGetValue(SpeakerDenomination.Value, out var value))
		{
			return value;
		}
		EntityID? result = null;
		Entity entity = null;
		if (triggeringEntity.HasValue)
		{
			entity = Entity.FindByID(triggeringEntity.Value);
		}
		switch (ActionByAgent)
		{
		case ActionByAgent.OnlySpecific:
		{
			Entity entityByName = GetEntityByName(NameOfSpeaker);
			if (entityByName != null)
			{
				result = entityByName.EntityID;
			}
			break;
		}
		case ActionByAgent.PreferSpecific:
		{
			Entity entityByName = GetEntityByName(NameOfSpeaker);
			if (entityByName == null)
			{
				return GetRandomInAllegianceWhoCanSpeak(GetAllegianceIfItCanSpeak(), actionData.Conversation, eventAction);
			}
			if (EntityCanSpeak(entityByName, eventAction))
			{
				result = entityByName.EntityID;
				break;
			}
			return GetRandomInAllegianceWhoCanSpeak(entityByName.Intelligence.Allegiance, actionData.Conversation, eventAction);
		}
		case ActionByAgent.RandomInAllegiance:
			return GetRandomInAllegianceWhoCanSpeak(GetAllegianceIfItCanSpeak(), actionData.Conversation, eventAction);
		case ActionByAgent.OnlyTriggeringEntity:
			if (EntityCanSpeak(entity, eventAction))
			{
				result = entity.EntityID;
			}
			break;
		case ActionByAgent.PreferTriggeringEntity:
			if (EntityCanSpeak(entity, eventAction))
			{
				result = entity.EntityID;
				break;
			}
			return GetRandomInAllegianceWhoCanSpeak(entity.Intelligence.Allegiance, actionData.Conversation, eventAction);
		default:
			result = null;
			break;
		}
		return result;
	}

	private bool AllegianceCanSpeak(Allegiance allegiance)
	{
		if (!CanTalkWhileThreatened && allegiance.IsUnderThreat())
		{
			return false;
		}
		return true;
	}

	private bool IsHavingSameOrMoreImportantConversation(Intelligence intelligence)
	{
		if (intelligence.CurrentConversationID.HasValue)
		{
			Conversation conversation = LookUp<Conversation, ConversationID>.FindByID(intelligence.CurrentConversationID.Value);
			if (conversation != null && conversation.TalkPriority.HasValue && conversation.TalkPriority.Value >= TalkPriority)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsCurrentlySpeakingWithoutAConversation(Intelligence intelligence)
	{
		if (!intelligence.CurrentConversationID.HasValue && !string.IsNullOrEmpty(intelligence.SpokenLine))
		{
			return true;
		}
		return false;
	}

	private bool EntityTypeCanSpeak(EntityType entityType)
	{
		if (entityType.Person == null)
		{
			if (entityType.IntelligenceType != null)
			{
				return entityType.IntelligenceType.CanSpeak == true;
			}
			return false;
		}
		return true;
	}

	public static bool TestAgentProperties(Entity entity, bool allowSleeping, bool allowUnconscious, bool allowFighting, bool allowThreatened, bool allowEmigrating, bool allowTravelling)
	{
		if (!entity.Find<Intelligence>(out var c))
		{
			return false;
		}
		bool flag = c.IsSleeping();
		if (!allowUnconscious && !c.IsAwakeAndActive && !flag)
		{
			return false;
		}
		if (!allowSleeping && flag)
		{
			return false;
		}
		if (!allowFighting && c.IsAttacking())
		{
			return false;
		}
		if (!allowThreatened && c.Allegiance.IsUnderThreat())
		{
			return false;
		}
		if (!allowTravelling && !entity.Location.HasValue)
		{
			return false;
		}
		if (!allowEmigrating && c.IsEmigrating())
		{
			return false;
		}
		return true;
	}

	private bool EntityCanSpeak(Entity entity, EventAction eventAction)
	{
		if (entity == null)
		{
			return false;
		}
		if (!entity.IsOnPlaySite())
		{
			return false;
		}
		if (!EntityTypeCanSpeak(entity.EntityType))
		{
			return false;
		}
		if (!entity.Find<Intelligence>(out var c))
		{
			return false;
		}
		if (!TestAgentProperties(entity, CanTalkWhileSleeping, allowUnconscious: false, CanTalkWhileFighting, CanTalkWhileThreatened, CanTalkWhileEmigrating, CanTalkWhileTravelling))
		{
			return false;
		}
		if (IsHavingSameOrMoreImportantConversation(c))
		{
			return false;
		}
		if (IsCurrentlySpeakingWithoutAConversation(c))
		{
			return false;
		}
		if (CanTalk != null && !CanTalk.IsFulfilled(ref entity, eventAction.TargetEntity, eventAction.PolledEventSource, eventAction.DynamicTarget))
		{
			return false;
		}
		return true;
	}

	private Allegiance GetAllegianceIfItCanSpeak()
	{
		Allegiance allegiance = ((Allegiance == null) ? The.Sim.PlaySite.PlayerAllegiance : The.Sim.PlaySite.Allegiances.First((Allegiance a) => a.Name == Allegiance));
		if (allegiance != null && allegiance.Members.Count > 0 && AllegianceCanSpeak(allegiance))
		{
			return allegiance;
		}
		return null;
	}

	private EntityID? GetRandomInAllegianceWhoCanSpeak(Allegiance allegiance, Conversation conversation, EventAction eventAction)
	{
		if (allegiance != null)
		{
			Entity entity = Common.Randomize(allegiance.Members.ToList(), The.Sim.GameplayRandomGenerator).FirstOrDefault((Entity e) => EntityCanSpeak(e, eventAction) && (conversation == null || !conversation.AssignedSpeakers.Values.Contains(e.EntityID)));
			if (entity != null)
			{
				return entity.EntityID;
			}
		}
		return null;
	}

	public static Entity GetEntityByName(string name)
	{
		if (The.Sim.PlaySite.EntitiesByName.TryGetValue(name, out var value))
		{
			Entity entity = Entity.FindByID(value);
			if (entity != null)
			{
				return entity;
			}
		}
		return null;
	}

	public override void PreInitValidate(ref List<string> errors)
	{
		base.PreInitValidate(ref errors);
		if (DefaultText == null)
		{
			EntityType.CreateValidationError(ref errors, "Text was not filled out!");
		}
	}

	public override void Initialize()
	{
		if (!Duration.HasValue)
		{
			Duration = DefaultText.Length / GameData.Instance.Constants.TalkSpeedInCharactersPerSecond;
			Duration = Common.Clamp(Duration.Value, GameData.Instance.Constants.MinimumTalkDurationInSeconds, GameData.Instance.Constants.MaximumTalkDurationInSeconds);
		}
	}

	public override string ToString()
	{
		return DefaultText;
	}
}
