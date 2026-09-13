using System;

namespace UWGame.ClientSide.Interface;

public class ItemButtonEventArgs : EventArgs
{
	public object Item;

	public ItemButtonEventArgs(object item)
	{
		Item = item;
	}
}
