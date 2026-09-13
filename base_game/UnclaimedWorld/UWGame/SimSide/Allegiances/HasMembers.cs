using System;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Allegiances;

public class HasMembers
{
	private static CanIterateEntitiesID IDCounter;

	public static CanIterateEntitiesID GetUniqueID()
	{
		IDCounter++;
		if (IDCounter >= CanIterateEntitiesID.Invalid)
		{
			throw new Exception("Astounding, HasMembersID just exceeded 64 bits. Something seriously wrong has happened.");
		}
		return IDCounter;
	}

	public static void DoSnapshot(Snapshotter sn)
	{
		IDCounter = sn.DoEnum(IDCounter);
	}

	public static void ResetIDCounterNoInvoke()
	{
		IDCounter = CanIterateEntitiesID.First;
	}
}
