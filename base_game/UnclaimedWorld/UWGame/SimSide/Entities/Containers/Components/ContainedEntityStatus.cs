using System;

namespace UWGame.SimSide.Entities.Containers.Components;

[Flags]
public enum ContainedEntityStatus
{
	None = 0,
	Entering = 1,
	Exiting = 2,
	Contained = 4,
	Passenger = 8,
	GettingHealed = 0x16,
	EatingBirthdayCake = 0x32
}
