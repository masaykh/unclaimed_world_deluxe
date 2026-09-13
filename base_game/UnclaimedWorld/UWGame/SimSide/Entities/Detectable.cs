using System;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities;

public class Detectable
{
	private static DetectableID IDCounter;

	public static DetectableID GetUniqueID()
	{
		IDCounter++;
		if (IDCounter >= DetectableID.Invalid)
		{
			throw new Exception("Astounding, DetectableID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public static void DoSnapshot(Snapshotter sn)
	{
		IDCounter = sn.DoEnum(IDCounter);
	}

	public static void ResetIDCounterNoInvoke()
	{
		IDCounter = DetectableID.First;
	}
}
