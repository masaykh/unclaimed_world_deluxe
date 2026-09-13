using System;
using UWGame.SimSide.Entities;

namespace UWGame.ClientSide.Interface;

public class ProductionTargetEventArgs : EventArgs
{
	public EntityType Item;

	public int? OutputBatchAmount;

	public ProductionTargetEventArgs(EntityType item, int? outputBatchAmount)
	{
		Item = item;
		OutputBatchAmount = outputBatchAmount;
	}
}
