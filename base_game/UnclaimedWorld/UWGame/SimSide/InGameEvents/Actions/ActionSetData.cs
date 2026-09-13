using System;
using System.Linq;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.InGameEvents.Actions;

public class ActionSetData : ISnapshot, ILookUp<ActionSetData, ActionSetDataID>
{
	private ActionSetType actionSetType;

	private ConversationID? conversationID;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	private ActionSetDataID id = ActionSetDataID.Invalid;

	private static ActionSetDataID IDCounter = ActionSetDataID.First;

	public Conversation Conversation
	{
		get
		{
			Conversation conversation;
			if (!conversationID.HasValue)
			{
				conversation = new Conversation(actionSetType.NoOfTalkActions);
				conversationID = conversation.ID;
			}
			else
			{
				conversation = LookUp<Conversation, ConversationID>.FindByID(conversationID.Value);
			}
			return conversation;
		}
	}

	public bool ConversationHasStarted
	{
		get
		{
			if (conversationID.HasValue)
			{
				Conversation conversation = LookUp<Conversation, ConversationID>.FindByID(conversationID.Value);
				if (conversation != null)
				{
					return conversation.HasStarted;
				}
			}
			return false;
		}
	}

	public bool IsSnapshotted { get; set; }

	public ActionSetDataID ID
	{
		get
		{
			return id;
		}
		private set
		{
			id = value;
		}
	}

	public int LoadPostProcessOrder => 0;

	public ActionSetData(ActionSetType actionSetType)
	{
		this.actionSetType = actionSetType;
		AddToLookup();
	}

	public ActionSetData()
	{
	}

	public bool IsConversationValid(TalkAction talkAction)
	{
		if (talkAction.SpeakerDenomination.HasValue && ConversationHasStarted)
		{
			if (Conversation.ConversationShouldEnd)
			{
				return false;
			}
			if (Conversation.AssignedSpeakers.Values.Any((EntityID e) => Entity.FindByID(e) == null))
			{
				return false;
			}
		}
		return true;
	}

	public void Fire(Entity triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
	{
		EventActionType[] actions = actionSetType.Actions;
		for (int i = 0; i < actions.Length; i++)
		{
			new EventAction(actions[i], this).ExecuteNowOrLater(triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
		}
		if (actionSetType.MaxFirings.HasValue)
		{
			The.Sim.PlaySite.EventManager.IncreaseFirings(actionSetType);
		}
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		id = sn.DoEnum(id);
		IDCounter = sn.DoEnum(IDCounter);
		actionSetType = sn.DoGameData(actionSetType);
		conversationID = sn.DoEnumNullable(conversationID);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}

	public ActionSetDataID GetUniqueID()
	{
		IDCounter++;
		if (IDCounter >= ActionSetDataID.Invalid)
		{
			throw new Exception("Astounding, ActionSetDataID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public ActionSetDataID SnapshotID(Snapshotter sn, ActionSetDataID id)
	{
		return sn.DoEnum(id);
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != ActionSetDataID.Invalid)
		{
			LookUp<ActionSetData, ActionSetDataID>.Add(ID, this);
		}
	}

	public void RemoveIDEntry()
	{
		LookUp<ActionSetData, ActionSetDataID>.Remove(this);
	}

	void ILookUp<ActionSetData, ActionSetDataID>.ResetIDCounter()
	{
	}

	public static void ResetIDCounter()
	{
		IDCounter = ActionSetDataID.First;
	}

	void ILookUp<ActionSetData, ActionSetDataID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUp<ActionSetData, ActionSetDataID>.Create();
	}

	public void SetInvalid()
	{
		id = ActionSetDataID.Invalid;
	}
}
