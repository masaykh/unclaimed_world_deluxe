using System.Collections.Generic;
using UWGame.SimSide.InGameEvents;
using UWGame.SimSide.InGameEvents.Actions;

namespace UWGame.SimSide.Scenarios;

public class Option
{
	public enum LoadingDialogTextModes
	{
		Override,
		Append
	}

	public string ShortDescription;

	public CustomDifficulty Difficulty;

	public LoadingDialogTextModes LoadingDialogTextMode;

	public string LoadingDialogText;

	public int? LoadingDialogTextOrder;

	public string[] ActionKeys;

	public string[] ConditionalEvents;

	public string KeyName { get; set; }

	public string Name { get; set; }

	public void Initialize()
	{
	}

	public List<EventActionType> GetStartActions()
	{
		List<EventActionType> list = new List<EventActionType>();
		if (ActionKeys != null)
		{
			string[] actionKeys = ActionKeys;
			foreach (string key in actionKeys)
			{
				EventActionType item = GameData.Instance.AllEventActionTypes[key];
				list.Add(item);
			}
		}
		return list;
	}

	public List<PolledEventType> GetEvents()
	{
		List<PolledEventType> list = new List<PolledEventType>();
		if (ConditionalEvents != null)
		{
			string[] conditionalEvents = ConditionalEvents;
			foreach (string key in conditionalEvents)
			{
				PolledEventType item = GameData.Instance.AllPolledEvents[key];
				list.Add(item);
			}
		}
		return list;
	}
}
