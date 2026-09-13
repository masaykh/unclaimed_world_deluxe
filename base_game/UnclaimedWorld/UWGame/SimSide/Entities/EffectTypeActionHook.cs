using System.Collections.Generic;
using System.Xml.Serialization;

namespace UWGame.SimSide.Entities;

public class EffectTypeActionHook : IGameData, IHook
{
	public AgentActionHooks Hook;

	public string ActionSetsKey;

	[XmlElement(ElementName = "EffectTypeKey")]
	public string TypeKey { get; set; }

	public int ExecutionOrder { get; set; }

	public string KeyName { get; set; }

	public string Name { get; set; }

	public bool DeleteRecord { get; set; }

	public void PreInitValidate(ref List<string> errors)
	{
		AgentActionHooks hook = Hook;
		if ((uint)(hook - 8) > 1u)
		{
			EntityType.CreateValidationError(ref errors, Hook.ToString() + " is not a valid value for an effect type hook event");
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
