using System;
using UWGame.SimSide.Entities;

namespace UWGame.ClientSide.Log;

public class Event
{
	public EventType EventType;

	public EntityID? Entity;

	public string EntityName;

	public string EntityLink;

	public Priority Priority;

	public DateTime Time;

	public string Text;

	public uint ID;

	private static uint idCounter;

	public Event(Entity entity)
	{
		if (entity != null)
		{
			Entity = entity.EntityID;
			EntityName = entity.ToString();
			EntityLink = entity.ToLink();
		}
		ID = idCounter;
		idCounter++;
	}

	public override string ToString()
	{
		if (Entity.HasValue)
		{
			return $"{Time}: [{Entity}] {Text}";
		}
		return $"{Time}: {Text}";
	}

	public static void ResetOtherIDCounter()
	{
		idCounter = 0u;
	}
}
