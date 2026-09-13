using System;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities.Owners;

public class Owner
{
	private static OwnerID IDCounter = OwnerID.First;

	public static OwnerID GetUniqueID()
	{
		IDCounter++;
		if (IDCounter >= OwnerID.Invalid)
		{
			throw new Exception("Astounding, OwnerID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public static void DoSnapshot(Snapshotter sn)
	{
		IDCounter = sn.DoEnum(IDCounter);
	}

	public static void ResetIDCounterNoInvoke()
	{
		IDCounter = OwnerID.First;
	}
}
