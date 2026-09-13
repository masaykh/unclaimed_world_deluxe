using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Content;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.Conditions;

namespace UWGame.SimSide.InGameEvents.Actions;

[DebuggerDisplay("{KeyName}")]
public class ActionSetType : IGameData
{
	public string Comments;

	public float? ChanceToFire;

	public int? MaxFirings;

	public Condition Condition;

	public EventActionType[] Actions;

	[XmlIgnore]
	public int NoOfTalkActions { get; private set; }

	public string KeyName { get; set; }

	public string Name { get; set; }

	public bool DeleteRecord { get; set; }

	public ActionSetType(string keyName)
	{
		KeyName = keyName;
	}

	public ActionSetType()
	{
	}

	public void Initialize()
	{
		if (Actions == null)
		{
			return;
		}
		for (int i = 0; i < Actions.Length; i++)
		{
			EventActionType obj = Actions[i];
			obj.Initialize();
			if (obj is TalkAction talkAction)
			{
				talkAction.SetLineNo(NoOfTalkActions);
				NoOfTalkActions++;
			}
		}
	}

	public void PostInitValidate(ref List<string> errors)
	{
		if (Actions == null)
		{
			return;
		}
		TalkAction.TalkActionPriority? talkActionPriority = null;
		EventActionType[] actions = Actions;
		foreach (EventActionType obj in actions)
		{
			obj.PostInitValidate(ref errors);
			if (!(obj is TalkAction talkAction))
			{
				continue;
			}
			if (talkActionPriority.HasValue)
			{
				if (talkActionPriority != talkAction.TalkPriority)
				{
					EntityType.CreateValidationError(ref errors, "All talk actions in an ActionSet must have the same priority.");
				}
			}
			else
			{
				talkActionPriority = talkAction.TalkPriority;
			}
		}
	}

	public void PostDataCompleteInitialize()
	{
	}

	public void PostDataCompleteValidate(ref List<string> listOfErrors)
	{
		if (Actions != null)
		{
			EventActionType[] actions = Actions;
			for (int i = 0; i < actions.Length; i++)
			{
				actions[i].PostDataCompleteValidate(ref listOfErrors);
			}
		}
	}

	public void PreDataCompleteValidate(ref List<string> listOfErrors)
	{
	}

	public void ExtractNestedGameData(ref List<string> duplicateKeyErrors)
	{
		if (GameData.Instance.AllActionSetTypes.ContainsKey(KeyName))
		{
			Common.AddToList(ref duplicateKeyErrors, "Duplicate key: " + KeyName);
		}
		else
		{
			GameData.Instance.AllActionSetTypes.Add(KeyName, this);
		}
		EventActionType[] actions = Actions;
		for (int i = 0; i < actions.Length; i++)
		{
			actions[i].ExtractNestedActionTypes(ref duplicateKeyErrors);
		}
	}

	public void LoadContent(ContentManager content)
	{
		if (Actions != null)
		{
			EventActionType[] actions = Actions;
			for (int i = 0; i < actions.Length; i++)
			{
				actions[i].LoadContent(content);
			}
		}
	}

	public void PostLoadContentValidate(ref List<string> listOfErrors)
	{
		EventActionType[] actions = Actions;
		for (int i = 0; i < actions.Length; i++)
		{
			actions[i].PostLoadContentValidate(ref listOfErrors);
		}
	}

	public bool ConditionsAreFulfilled(Entity triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
	{
		if (ChanceToFire.HasValue && The.Sim.GameplayRandomGenerator.NextDouble("") > (double)ChanceToFire.Value)
		{
			return false;
		}
		if (Condition != null)
		{
			return Condition.IsFulfilled(ref triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
		}
		return true;
	}

	public void PreInitValidate(ref List<string> errors)
	{
		EntityType.ValidateRequiredValue(ref errors, "KeyName", !string.IsNullOrEmpty(KeyName));
		if (Condition != null)
		{
			Condition.PreInitValidate(ref errors);
		}
		EventActionType[] actions = Actions;
		for (int i = 0; i < actions.Length; i++)
		{
			actions[i].PreInitValidate(ref errors);
		}
	}
}
