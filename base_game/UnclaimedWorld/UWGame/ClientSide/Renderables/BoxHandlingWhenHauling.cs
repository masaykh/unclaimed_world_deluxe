using Xclna.Xna.Animation;

namespace UWGame.ClientSide.Renderables;

public class BoxHandlingWhenHauling
{
	public enum BoxHandlingType
	{
		AlwaysInHand,
		AlwaysOnBack,
		OnlyOnBackWhenHeavyAndHaulingFar
	}

	public bool UseHeavyBackpack;

	public AttacheePoint? ShowBoxInHand;

	public string AttachorWhenBoxIsInHand;

	public BoxHandlingType BoxHandling;
}
