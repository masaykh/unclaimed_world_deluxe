using System.Collections.Generic;
using UWGame.SimSide.Overland;

namespace UWGame.SimSide.InGameEvents.Actions;

public class SpawnWorldAction : EventActionType
{
	public WorldData WorldData;

	public SpawnWorldAction(string keyName)
		: base(keyName)
	{
	}

	public SpawnWorldAction()
	{
	}

	public override bool Execute(EventAction action, ref string failReason)
	{
		if (WorldData != null)
		{
			The.Sim.World = World.CreateFromWorldData(WorldData);
			return true;
		}
		return false;
	}

	public void PreInitValidate(List<string> listOfErrors)
	{
	}

	public new void Initialize()
	{
	}

	public void PostInitValidate(List<string> listOfErrors)
	{
		_ = WorldData;
	}

	public void PostLoadContentValidate(List<string> listOfErrors)
	{
	}

	public override string ToString()
	{
		return "Spawned World";
	}
}
