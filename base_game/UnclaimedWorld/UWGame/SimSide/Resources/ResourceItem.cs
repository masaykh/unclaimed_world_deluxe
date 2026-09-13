using System;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Resources;

public static class ResourceItem
{
	private static ResourceItemID IDCounter;

	public static ResourceItemID GetUniqueID()
	{
		IDCounter++;
		if ((ulong)IDCounter >= ulong.MaxValue)
		{
			throw new Exception("Astounding, ResourceItemID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public static void DoSnapshot(Snapshotter sn)
	{
		IDCounter = sn.DoEnum(IDCounter);
	}

	public static void ResetIDCounterNoInvoke()
	{
		IDCounter = ResourceItemID.First;
	}
}
