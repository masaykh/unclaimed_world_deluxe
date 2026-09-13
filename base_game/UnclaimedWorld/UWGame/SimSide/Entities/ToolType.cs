using System.Collections.Generic;
using System.Xml.Serialization;
using UWGame.SimSide.Processes;

namespace UWGame.SimSide.Entities;

public class ToolType
{
	public string[] ToolTag;

	public float? Durability;

	public ToolHandlingType? ToolHandling;

	public string PrepareProcess;

	[XmlIgnore]
	public ProcessType PrepareProcessType;

	public bool IsPseudoTool;

	public static bool IsImmovable(EntityType entityType)
	{
		if (entityType.StructureType != null || entityType.Upgrader != null)
		{
			return true;
		}
		return false;
	}

	public void PostLoadContentInitialize()
	{
		if (PrepareProcess != null)
		{
			PrepareProcessType = GameData.Instance.AllProcessTypes[PrepareProcess];
			PrepareProcessType.SetPreparedProperty = true;
		}
	}

	public void PreInitValidate(ref List<string> listOfErrors)
	{
		if (!IsPseudoTool)
		{
			EntityType.ValidateRequiredValue(ref listOfErrors, "Durability", Durability.HasValue);
			EntityType.ValidateRequiredValue(ref listOfErrors, "ToolHandling", ToolHandling.HasValue);
		}
	}

	public override string ToString()
	{
		return string.Join(",", ToolTag);
	}
}
