using System;
using UWGame.SimSide.Entities;

namespace UWGame.ClientSide.Interface;

public class ItemCategoryButtonEventArgs : EventArgs
{
	public EntityCategory Category;

	public ItemCategoryButtonEventArgs(EntityCategory item)
	{
		Category = item;
	}
}
