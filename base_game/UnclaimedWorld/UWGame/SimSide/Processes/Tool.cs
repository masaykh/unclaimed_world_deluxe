using System.Collections.Generic;
using System.Xml.Serialization;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Processes;

public class Tool
{
	public string UsesToolKeyName;

	public string UsesToolTag;

	public float? ProductivityFactor;

	public float? DegradePerSecondOfUse;

	[XmlIgnore]
	public List<EntityType> ToolEntityTypes = new List<EntityType>();

	public bool ShouldSerializeProductivityFactor()
	{
		return ProductivityFactor.HasValue;
	}

	public void PreDataCompleteValidate(ref List<string> listOfErrors)
	{
		if (!string.IsNullOrEmpty(UsesToolKeyName))
		{
			EntityType.ValidateGameDataTypeExists(ref listOfErrors, UsesToolKeyName, GameData.Instance.AllEntityTypes, out var _);
		}
	}

	public void PostDataCompleteInitialize()
	{
		if (!string.IsNullOrEmpty(UsesToolTag))
		{
			ToolEntityTypes.AddRange(GameData.Instance.ToolsByTag[UsesToolTag]);
		}
		if (!string.IsNullOrEmpty(UsesToolKeyName))
		{
			EntityType item = GameData.Instance.AllEntityTypes[UsesToolKeyName];
			if (!ToolEntityTypes.Contains(item))
			{
				ToolEntityTypes.Add(item);
			}
		}
	}

	public void PreInitValidate(ref List<string> errors)
	{
		EntityType.ValidateRequiredValue(ref errors, "Degrade", DegradePerSecondOfUse.HasValue);
		EntityType.ValidateRequiredValue(ref errors, "Productivity", ProductivityFactor.HasValue);
	}

	public void PostInitValidate(ref List<string> errors)
	{
		if (ToolEntityTypes == null)
		{
			return;
		}
		foreach (EntityType toolEntityType in ToolEntityTypes)
		{
			if (toolEntityType.ToolType == null)
			{
				EntityType.CreateValidationError(ref errors, "The entity '" + toolEntityType.KeyName + "' cannot be assigned as a tool when its ToolType is null.");
			}
		}
	}
}
