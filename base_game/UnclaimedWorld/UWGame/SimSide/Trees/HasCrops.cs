using System;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Trees;

public class HasCrops
{
	private static HasCropsID IDCounter = HasCropsID.First;

	public static HasCropsID GetUniqueID()
	{
		IDCounter++;
		if (IDCounter >= HasCropsID.Invalid)
		{
			throw new Exception("Astounding, HasCropsID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public static void DoSnapshot(Snapshotter sn)
	{
		IDCounter = sn.DoEnum(IDCounter);
	}

	public static void ResetIDCounterNoInvoke()
	{
		IDCounter = HasCropsID.First;
	}
}
