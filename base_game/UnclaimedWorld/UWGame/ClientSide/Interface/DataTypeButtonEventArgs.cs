using System;
using UWGame.SimSide.Entities;

namespace UWGame.ClientSide.Interface;

public class DataTypeButtonEventArgs : EventArgs
{
	public EntityGroupID? Owner;

	public bool UseUIOwner;

	public DataTypeButtonEventArgs(EntityGroupID? owner, bool useUIOwner)
	{
		Owner = owner;
		UseUIOwner = useUIOwner;
	}
}
