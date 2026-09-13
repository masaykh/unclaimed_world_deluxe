using System.Collections.Generic;
using System.Xml.Serialization;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Processes;

public class Input
{
	public string Entity;

	public string Tag;

	public InputAmount Amount;

	public bool IsConsumed;

	public string BecomesPartOfProduct;

	[XmlIgnore]
	public EntityType BecomesPartOfProductType;

	[XmlIgnore]
	public EntityType EntityType;

	[XmlIgnore]
	public float StageLength;

	public void PostDataCompleteInitialize()
	{
		if (Amount.NoOfItems.HasValue)
		{
			StageLength = 1f / (float)Amount.NoOfItems.Value;
		}
		else
		{
			StageLength = 1f;
		}
		EntityType = GameData.Instance.AllEntityTypes[Entity];
		Amount.Initialize();
		if (BecomesPartOfProduct != null)
		{
			BecomesPartOfProductType = GameData.Instance.AllEntityTypes[BecomesPartOfProduct];
		}
	}

	public void PreDataCompleteValidate(ref List<string> listOfErrors)
	{
		if (Entity != null)
		{
			EntityType.ValidateEntityTypeKeyExists(ref listOfErrors, Entity);
		}
		if (BecomesPartOfProduct != null)
		{
			EntityType.ValidateEntityTypeKeyExists(ref listOfErrors, BecomesPartOfProduct);
		}
	}

	public bool InputIsImmovable()
	{
		return EntityType.IsImmovable();
	}

	public void Validate(ref List<string> errors)
	{
	}
}
