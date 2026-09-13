using System.Collections.Generic;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.AI.Activities;

public class Activity
{
	public List<Entity> Members = new List<Entity>();

	public bool HasStarted;

	public bool HasEnded;

	public int MinimumMembers = 2;

	private List<Activity> listOfJobs;

	public Activity(List<Activity> BelongsTo)
	{
		if (BelongsTo != null)
		{
			listOfJobs = BelongsTo;
			listOfJobs.Add(this);
		}
	}

	public void Remove()
	{
		if (listOfJobs != null)
		{
			listOfJobs.Remove(this);
		}
	}

	public virtual void AddMember(Entity member)
	{
		Members.Add(member);
	}

	public virtual void LeaveActivity(Entity entity)
	{
		if (Members.Contains(entity))
		{
			Members.Remove(entity);
		}
		if (Members.Count < MinimumMembers)
		{
			Remove();
		}
	}

	public void SendMessageToEveryoneElse(Entity self, Message message)
	{
		foreach (Entity member in Members)
		{
			if (member != self)
			{
				member.SendMessage(message);
			}
		}
	}
}
