using System;
using UWGame.SimSide.Entities;

namespace UWGame.ClientSide.Interface;

public class EntityButtonEventArgs : EventArgs
{
	public EntityID Entity;

	public EntityButtonEventArgs(EntityID entityID)
	{
		Entity = entityID;
	}
}
