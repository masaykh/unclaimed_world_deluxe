using Microsoft.Xna.Framework;
using UWGame.SimSide.InGameEvents.Actions;

namespace UWGame.ClientSide.GameEvents;

public class SetViewAction : EventActionType
{
	public Vector2 CenterOnLocation;

	public DynamicLocation OffsetToLocation;

	public SetViewAction(string keyName)
		: base(keyName)
	{
	}

	public SetViewAction()
	{
	}

	public override bool Execute(EventAction action, ref string failReason)
	{
		Vector2 centerOnLocation = CenterOnLocation;
		if (OffsetToLocation != null)
		{
			Vector2? location = OffsetToLocation.GetLocation(action);
			if (!location.HasValue)
			{
				failReason = OffsetToLocation.PropertyKey + " dynamic offset location was null.";
				return false;
			}
			centerOnLocation += location.Value;
		}
		The.MapUI.ZoomToMapPosition(centerOnLocation.ToVector3());
		return true;
	}

	public override string ToString()
	{
		return "SetView " + CenterOnLocation.ToString();
	}
}
