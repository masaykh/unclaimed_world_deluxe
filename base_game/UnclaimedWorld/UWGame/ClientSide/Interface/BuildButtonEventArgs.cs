using System;
using UWGame.SimSide.Buildings;

namespace UWGame.ClientSide.Interface;

public class BuildButtonEventArgs : EventArgs
{
	public StructureType StructureType;

	public BuildButtonEventArgs(StructureType buildingType)
	{
		StructureType = buildingType;
	}
}
