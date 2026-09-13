using System;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide;

public class MethodCounter
{
	private static MethodID IDCounter;

	public static MethodID GetUniqueID()
	{
		IDCounter++;
		if (IDCounter >= MethodID.Invalid)
		{
			throw new Exception("Astounding, MethodID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public static void DoSnapshot(Snapshotter sn)
	{
		IDCounter = sn.DoEnum(IDCounter);
	}

	public static void ResetIDCounterNoInvoke()
	{
		IDCounter = MethodID.First;
	}
}
