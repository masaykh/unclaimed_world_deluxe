using System.Collections.Generic;
using UWGame.SimSide.InGameEvents.Actions;

namespace UWGame.SimSide.Allegiances;

public class AllegianceEventType : IGameData
{
	public AllegianceEvents Event;

	public ActionSets ActionSets;

	public string KeyName { get; set; }

	public string Name { get; set; }

	public bool DeleteRecord { get; set; }

	public void PreInitValidate(ref List<string> errors)
	{
	}

	public void Initialize()
	{
	}

	public void PostInitValidate(ref List<string> errors)
	{
	}

	public void PostDataCompleteInitialize()
	{
	}

	public void PreDataCompleteValidate(ref List<string> listOfErrors)
	{
	}

	public void PostDataCompleteValidate(ref List<string> listOfErrors)
	{
	}
}
