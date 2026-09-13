using System.Collections.Generic;
using UWGame.ClientSide.Renderables;

namespace UWGame.SimSide.Entities.Locomotors.Stances;

public class StanceType : IGameData
{
	public int Number;

	public AnimModifier? AnimModifier;

	public bool? CanStartIdleConversation;

	public bool? CanTurnBody;

	public bool? CanTurnHead;

	public bool? IsProne;

	public float IdleExertionLevel;

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

	public override string ToString()
	{
		return Name ?? KeyName;
	}

	public void PreDataCompleteValidate(ref List<string> listOfErrors)
	{
	}

	public void PostDataCompleteInitialize()
	{
	}

	public void PostDataCompleteValidate(ref List<string> listOfErrors)
	{
	}
}
