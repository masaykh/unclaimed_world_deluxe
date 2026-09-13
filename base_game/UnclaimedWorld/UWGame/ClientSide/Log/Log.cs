using System.Collections.Generic;
using UWGame.SimSide;
using UWGame.SimSide.AI;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.Actions;

namespace UWGame.ClientSide.Log;

public class Log
{
	public EventType DebugEvent = new EventType
	{
		Name = "Debug",
		DefaultPriority = Priority.Normal
	};

	public EventType EconomicEvent = new EventType
	{
		Name = "Economic",
		DefaultPriority = Priority.Normal
	};

	public EventType GeneralEvent = new EventType
	{
		Name = "General",
		DefaultPriority = Priority.Normal
	};

	public EventType CombatEvent = new EventType
	{
		Name = "Combat",
		DefaultPriority = Priority.Normal
	};

	public List<Event> Events = new List<Event>();

	public List<TalkEvent> TalkEvents = new List<TalkEvent>();

	public event NewEventHandler NewEventAlert;

	public void AddTalk(string line, EntityID speaker, Conversation conversation)
	{
		TalkEvent talkEvent = new TalkEvent
		{
			Line = line,
			SpokenBy = speaker
		};
		if (conversation != null)
		{
			talkEvent.MessageGroupNo = (ulong)conversation.ID;
		}
		TalkEvents.Add(talkEvent);
	}

	public void AddLogEvent(EventType eventType, Entity concernedEntity, string eventText, Priority? priority = null)
	{
		if (eventType != DebugEvent && (concernedEntity == null || The.InGameUI.UIAllegiance.SharedKnowledge.GetKnownData(concernedEntity.EntityID, out var _) == EntityResult.SeenDirectly))
		{
			if (!priority.HasValue)
			{
				priority = eventType.DefaultPriority;
			}
			Event obj = new Event(concernedEntity)
			{
				EventType = eventType,
				Text = eventText,
				Priority = priority.Value,
				Time = The.Sim.DateAndTime.GetTime()
			};
			Events.Add(obj);
			if (Events.Count > GameData.Instance.GUIConstants.MaxLogEventsToKeep)
			{
				Events.RemoveAt(Events.Count - 1);
			}
			if (obj.Priority == Priority.High && this.NewEventAlert != null)
			{
				this.NewEventAlert(obj);
			}
		}
	}
}
