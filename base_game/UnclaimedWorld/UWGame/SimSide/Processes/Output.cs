using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Processes;

public class Output
{
	public OutputAmount Amount;

	public string EntityTypeToCreate;

	[XmlIgnore]
	public EntityType FinalEntityTypeToCreate;

	public string[] ToolContainerTypesToPlaceIn;

	public string[] ToolContainerTagsToPlaceIn;

	public Vector2? RelativePlacement;

	[XmlIgnore]
	public List<EntityType> ToolContainerToPlaceIn = new List<EntityType>();

	public bool IsWasteProduct;

	public void PostDataCompleteInitialize()
	{
		FinalEntityTypeToCreate = GameData.Instance.AllEntityTypes[EntityTypeToCreate];
		GameData.ResolveEntityTypeTags(ref ToolContainerToPlaceIn, ToolContainerTagsToPlaceIn, ToolContainerTypesToPlaceIn, GameData.Instance.ToolsByTag);
	}

	public void PreDataCompleteValidate(ref List<string> listOfErrors)
	{
		if (EntityTypeToCreate != null)
		{
			EntityType.ValidateEntityTypeKeyExists(ref listOfErrors, EntityTypeToCreate);
		}
	}

	public void PostDataCompleteValidate(ref List<string> listOfErrors)
	{
		if (ToolContainerToPlaceIn == null)
		{
			return;
		}
		foreach (EntityType item in ToolContainerToPlaceIn)
		{
			if (item.ContainerType == null || !item.ContainerType.HasOutputStorage)
			{
				EntityType.CreateValidationError(ref listOfErrors, "The output is specified to be placed inside the tool " + item.KeyName + ", but this tool does not define a production output storage.");
			}
		}
	}

	public bool GetOutputAmountsToCreate(float totalBulkOfInput, Dictionary<string, float> extractedSubstances, out int noOfItemsToCreate, out float? bulkOfEachOutputItem)
	{
		bulkOfEachOutputItem = null;
		int? num = null;
		if (Amount.NoOfItems.HasValue)
		{
			num = Amount.NoOfItems.Value;
		}
		if (Amount.Bulk != null)
		{
			float num2 = ((!Amount.Bulk.FractionOfInputBulk.HasValue) ? extractedSubstances[Amount.Bulk.InputSubstance] : (totalBulkOfInput * Amount.Bulk.FractionOfInputBulk.Value));
			if (Common.IsZero(totalBulkOfInput))
			{
				noOfItemsToCreate = 0;
				return true;
			}
			if (FinalEntityTypeToCreate.ItemType != null)
			{
				if (FinalEntityTypeToCreate.ItemType.HasNoMaximumBulk)
				{
					noOfItemsToCreate = num ?? 1;
					bulkOfEachOutputItem = num2 / (float)noOfItemsToCreate;
				}
				else
				{
					if (num.HasValue)
					{
						noOfItemsToCreate = num.Value;
					}
					else
					{
						noOfItemsToCreate = (int)Math.Ceiling(num2 / FinalEntityTypeToCreate.ItemType.MaximumBulk.Value);
					}
					if (noOfItemsToCreate > 0)
					{
						bulkOfEachOutputItem = num2 / (float)noOfItemsToCreate;
						bulkOfEachOutputItem = Math.Min(bulkOfEachOutputItem.Value, FinalEntityTypeToCreate.ItemType.MaximumBulk.Value);
					}
				}
				return true;
			}
		}
		if (num.HasValue)
		{
			noOfItemsToCreate = num.Value;
			return true;
		}
		noOfItemsToCreate = 0;
		return false;
	}

	public Entity GetToolContainerToPlaceOutputIn(List<EntityID> tools)
	{
		if (ToolContainerToPlaceIn != null && tools != null)
		{
			foreach (EntityID tool in tools)
			{
				Entity entity = Entity.FindByID(tool);
				if (entity != null && ToolContainerToPlaceIn.Contains(entity.EntityType))
				{
					return entity;
				}
			}
		}
		return null;
	}
}
