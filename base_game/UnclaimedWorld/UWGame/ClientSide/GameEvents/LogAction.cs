using System.Collections.Generic;
using UWGame.ClientSide.Log;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.Actions;

namespace UWGame.ClientSide.GameEvents;

public class LogAction : EventActionType, IGameData
{
	public Priority Priority = Priority.Normal;

	public string Text;

	public bool ReferToTriggeringEntity = true;

	public LogAction(string keyName)
		: base(keyName)
	{
	}

	public LogAction()
	{
	}

	public override bool Execute(EventAction action, ref string failReason)
	{
		Entity concernedEntity = null;
		if (ReferToTriggeringEntity && action.TriggeringEntity.HasValue)
		{
			concernedEntity = Entity.FindByID(action.TriggeringEntity.Value);
		}
		The.Client.Log.AddLogEvent(The.Client.Log.GeneralEvent, concernedEntity, Text, Priority);
		return true;
	}

	public override void PostInitValidate(ref List<string> listOfErrors)
	{
		base.PostInitValidate(ref listOfErrors);
		if (Text == null)
		{
			EntityType.CreateValidationError(ref listOfErrors, string.Format("Text was not filled out!", base.KeyName));
		}
	}
}
