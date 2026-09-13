using System;
using UWGame.SimSide.Entities;

namespace UWGame.ClientSide.Interface;

public class ItemTypeButtonEventArgs : EventArgs
{
	public EntityType Item;

	public ItemTypeButtonEventArgs(EntityType item)
	{
		Item = item;
	}
}
