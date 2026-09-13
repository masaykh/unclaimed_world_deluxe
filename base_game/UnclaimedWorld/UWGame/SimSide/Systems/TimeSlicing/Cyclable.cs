using System;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Systems.TimeSlicing;

public static class Cyclable
{
	private static CyclableID IDCounter;

	public static CyclableID GetUniqueID()
	{
		IDCounter++;
		if ((ulong)IDCounter >= ulong.MaxValue)
		{
			throw new Exception("Astounding, CyclableID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public static void DoSnapshot(Snapshotter sn)
	{
		IDCounter = sn.DoEnum(IDCounter);
	}

	public static void ResetIDCounterNoInvoke()
	{
		IDCounter = CyclableID.First;
	}
}
