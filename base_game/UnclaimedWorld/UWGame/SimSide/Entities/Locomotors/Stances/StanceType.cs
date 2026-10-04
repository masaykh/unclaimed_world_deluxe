using System.Collections.Generic;
using UWGame.ClientSide.Renderables;

namespace UWGame.SimSide.Entities.Locomotors.Stances;

public class StanceType : IGameData
{
	/// <summary>
	/// Changing from a higher-numbered stance to a lower-numbered stance sets the Reverse animation flag.
	/// </summary>
	public int Number;

	public AnimModifier? AnimModifier;

	/// <summary>
	/// Whether a character in this stance may take part in an idle conversation.
	/// </summary>
	public bool? CanStartIdleConversation;

	/// <summary>
	/// Used when reacting to interest.
	/// </summary>
	public bool? CanTurnBody;

	/// <summary>
	/// Used when reacting to interest.
	/// </summary>
	public bool? CanTurnHead;

	/// <summary>
	/// Prone stances take more damage from attacks.
	/// </summary>
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
