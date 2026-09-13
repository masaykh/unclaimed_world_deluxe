using System.Collections.Generic;
using System.Xml.Serialization;

namespace UWGame.SimSide.Entities;

public class DetectResourceTypeHook : IGameData, IHook
{
	public string DetectedResourceKey;

	public string ActionSetsKey;

	[XmlElement(ElementName = "ResourceTypeKey")]
	public string TypeKey { get; set; }

	public int ExecutionOrder { get; set; }

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
