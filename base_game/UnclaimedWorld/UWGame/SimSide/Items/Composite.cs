using System;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Items;

public class Composite
{
	private static CompositeID IDCounter;

	public static CompositeID GetUniqueID()
	{
		IDCounter++;
		if (IDCounter >= CompositeID.Invalid)
		{
			throw new Exception("Astounding, CompositeID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public static void DoSnapshot(Snapshotter sn)
	{
		IDCounter = sn.DoEnum(IDCounter);
	}

	public static void ResetIDCounterNoInvoke()
	{
		IDCounter = CompositeID.First;
	}
}
