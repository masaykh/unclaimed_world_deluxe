using System;
using System.Collections.Generic;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.InGameEvents.Actions;

public class Conversation : ILookUp<Conversation, ConversationID>, ISnapshot
{
	public Dictionary<TalkAction.SpeakerInConversation, EntityID> AssignedSpeakers = new Dictionary<TalkAction.SpeakerInConversation, EntityID>();

	public bool ConversationShouldEnd;

	public TalkAction.TalkActionPriority? TalkPriority;

	public int TotalNoOfTalkActions;

	private int noOfSpokenLines;

	private ConversationID id = ConversationID.Invalid;

	private static ConversationID IDCounter = ConversationID.First;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool HasStarted => noOfSpokenLines > 0;

	public ConversationID ID
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

	public bool IsSnapshotted { get; set; }

	public Conversation(int totalTalkActions)
	{
		AddToLookup();
		TotalNoOfTalkActions = totalTalkActions;
	}

	public Conversation()
	{
	}

	public bool IsTalkActionNextInLine(TalkAction talkAction)
	{
		return noOfSpokenLines == talkAction.LineNo;
	}

	public void SpeakLine(TalkAction talkAction, Entity entity)
	{
		if (talkAction.SpeakerDenomination.HasValue && !AssignedSpeakers.ContainsKey(talkAction.SpeakerDenomination.Value))
		{
			AssignedSpeakers.Add(talkAction.SpeakerDenomination.Value, entity.EntityID);
		}
		TalkPriority = talkAction.TalkPriority;
		noOfSpokenLines++;
	}

	private void Intelligence_TalkActionEnded()
	{
		TalkActionEnded();
	}

	public void TalkActionEnded()
	{
		if (noOfSpokenLines == TotalNoOfTalkActions)
		{
			EndConversation();
		}
	}

	public void EndConversation()
	{
		ConversationShouldEnd = true;
		if (AssignedSpeakers != null)
		{
			foreach (KeyValuePair<TalkAction.SpeakerInConversation, EntityID> assignedSpeaker in AssignedSpeakers)
			{
				Entity entity = Entity.FindByID(assignedSpeaker.Value);
				if (entity != null && entity.Intelligence.CurrentConversationID == id)
				{
					entity.Intelligence.CurrentConversationID = null;
				}
			}
		}
		RemoveIDEntry();
	}

	public ConversationID GetUniqueID()
	{
		IDCounter++;
		if ((ulong)IDCounter >= ulong.MaxValue)
		{
			throw new Exception("Astounding, ConversationID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public ConversationID SnapshotID(Snapshotter sn, ConversationID id)
	{
		return sn.DoEnum(id);
	}

	public void AddToLookup()
	{
		ID = GetUniqueID();
		if (ID != ConversationID.Invalid)
		{
			LookUp<Conversation, ConversationID>.Add(ID, this);
		}
	}

	public void SetInvalid()
	{
		id = ConversationID.Invalid;
	}

	public void RemoveIDEntry()
	{
		LookUp<Conversation, ConversationID>.Remove(this);
	}

	void ILookUp<Conversation, ConversationID>.ResetIDCounter()
	{
	}

	public static void ResetIDCounter()
	{
		IDCounter = ConversationID.First;
	}

	void ILookUp<Conversation, ConversationID>.CreateLookupCollection()
	{
	}

	public static void CreateLookupCollection()
	{
		LookUp<Conversation, ConversationID>.Create();
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		id = SnapshotID(sn, id);
		IDCounter = sn.DoEnum(IDCounter);
		AssignedSpeakers = sn.DoDictionary(AssignedSpeakers);
		ConversationShouldEnd = sn.DoBool(ConversationShouldEnd);
		noOfSpokenLines = sn.DoInt32(noOfSpokenLines);
		TalkPriority = sn.DoEnumNullable(TalkPriority);
		TotalNoOfTalkActions = sn.DoInt32(TotalNoOfTalkActions);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}
}
