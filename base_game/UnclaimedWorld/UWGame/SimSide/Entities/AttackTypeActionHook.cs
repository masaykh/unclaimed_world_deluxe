using System.Collections.Generic;
using System.Xml.Serialization;

namespace UWGame.SimSide.Entities;

public class AttackTypeActionHook : IGameData, IHook
{
	public AgentActionHooks Hook;

	public string ActionSetsKey;

	[XmlElement(ElementName = "AttackTypeKey")]
	public string TypeKey { get; set; }

	public int ExecutionOrder { get; set; }

	public string KeyName { get; set; }

	public string Name { get; set; }

	public bool DeleteRecord { get; set; }

	public void PreInitValidate(ref List<string> errors)
	{
		AgentActionHooks hook = Hook;
		if ((uint)(hook - 6) > 1u && (uint)(hook - 11) > 3u)
		{
			EntityType.CreateValidationError(ref errors, Hook.ToString() + " is not a valid value for an attack type hook event");
		}
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
