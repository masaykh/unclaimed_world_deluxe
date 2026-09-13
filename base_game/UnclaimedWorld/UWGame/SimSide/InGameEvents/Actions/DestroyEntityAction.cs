using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.PropertyObjects;

namespace UWGame.SimSide.InGameEvents.Actions;

public class DestroyEntityAction : EventActionType
{
	public TargetObject TargetObject;

	public string EntityName;

	public DestroyEntityAction(string keyName)
		: base(keyName)
	{
	}

	public DestroyEntityAction()
	{
	}

	public override bool Execute(EventAction action, ref string failReason)
	{
		Entity entity = null;
		if (!EventActionType.GetEntity(EntityName, TargetObject, action, out entity, ref failReason))
		{
			return false;
		}
		if (entity != null)
		{
			entity.Destroy();
			return true;
		}
		return false;
	}

	public override string ToString()
	{
		return ("Destroy entity " + EntityName) ?? "";
	}
}
