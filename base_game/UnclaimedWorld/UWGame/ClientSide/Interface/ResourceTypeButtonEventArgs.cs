using System;
using UWGame.SimSide.Resources;

namespace UWGame.ClientSide.Interface;

public class ResourceTypeButtonEventArgs : EventArgs
{
	public ResourceType Item;

	public ResourceTypeButtonEventArgs(ResourceType item)
	{
		Item = item;
	}
}
