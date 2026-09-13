using System;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Maps;

public static class IMapCounter
{
	private static IMapID IDCounter = IMapID.First;

	public static IMapID GetUniqueID()
	{
		IDCounter++;
		if ((ulong)IDCounter >= ulong.MaxValue)
		{
			throw new Exception("Astounding, IMapID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public static void DoSnapshot(Snapshotter sn)
	{
		IDCounter = sn.DoEnum(IDCounter);
	}

	public static void ResetIDCounterNoInvoke()
	{
		IDCounter = IMapID.First;
	}
}
