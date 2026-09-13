using UWGame.SimSide.Entities;

namespace UWGame.ClientSide.Log;

public class TalkEvent
{
	public uint ID;

	public EntityID SpokenBy;

	public string Name;

	public string Line;

	public ulong? MessageGroupNo;

	private static uint idCounter;

	public TalkEvent()
	{
		ID = idCounter;
		idCounter++;
	}

	public static void ResetOtherIDCounter()
	{
		idCounter = 0u;
	}
}
