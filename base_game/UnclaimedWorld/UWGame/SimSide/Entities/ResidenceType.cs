using System;

namespace UWGame.SimSide.Entities;

public class ResidenceType
{
	public float ComfortLevel;

	public int LivingCapacity;

	public int PeopleCapacity;

	public void Initialize()
	{
		PeopleCapacity = Math.Max(PeopleCapacity, LivingCapacity);
	}
}
