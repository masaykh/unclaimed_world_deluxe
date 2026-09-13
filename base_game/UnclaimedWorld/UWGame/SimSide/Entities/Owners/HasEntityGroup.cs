using System;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities.Owners;

public class HasEntityGroup
{
	private static HasEntityGroupID IDCounter = HasEntityGroupID.First;

	public static HasEntityGroupID GetUniqueID()
	{
		IDCounter++;
		if (IDCounter >= HasEntityGroupID.Invalid)
		{
			throw new Exception("Astounding, HasEntityGroupID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public static void DoSnapshot(Snapshotter sn)
	{
		IDCounter = sn.DoEnum(IDCounter);
	}

	public static void ResetIDCounterNoInvoke()
	{
		IDCounter = HasEntityGroupID.First;
	}
}
