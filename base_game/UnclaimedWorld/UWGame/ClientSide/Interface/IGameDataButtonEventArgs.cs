using System;

namespace UWGame.ClientSide.Interface;

public class IGameDataButtonEventArgs : EventArgs
{
	public object Item;

	public IGameDataButtonEventArgs(object item)
	{
		Item = item;
	}
}
