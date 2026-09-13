using System.Collections.Generic;
using System.Xml.Serialization;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Entities.RepairTypes;

public class RepairProfile : IGameData
{
	public string Comments;

	public RepairType Integrity;

	public RepairType Condition;

	public RepairType DefaultPartsReplacement;

	public RepairType DefaultPartsCondition;

	public SerializableDictionary<string, RepairType> PartsReplacement;

	[XmlIgnore]
	public Dictionary<EntityType, RepairType> PartsReplacementFinal;

	public SerializableDictionary<string, RepairType> PartsCondition;

	[XmlIgnore]
	public Dictionary<EntityType, RepairType> PartsConditionFinal;

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

	public void PreDataCompleteValidate(ref List<string> listOfErrors)
	{
		if (Integrity != null)
		{
			Integrity.PreDataCompleteValidate(ref listOfErrors);
		}
		if (DefaultPartsCondition != null)
		{
			DefaultPartsCondition.PreDataCompleteValidate(ref listOfErrors);
		}
		if (DefaultPartsReplacement != null)
		{
			DefaultPartsReplacement.PreDataCompleteValidate(ref listOfErrors);
		}
	}

	public void PostDataCompleteInitialize()
	{
		if (Integrity != null)
		{
			Integrity.PostDataCompleteInitialize();
		}
		if (DefaultPartsCondition != null)
		{
			DefaultPartsCondition.PostDataCompleteInitialize();
		}
		if (DefaultPartsReplacement != null)
		{
			DefaultPartsReplacement.PostDataCompleteInitialize();
		}
		if (PartsReplacement != null)
		{
			PartsReplacementFinal = new Dictionary<EntityType, RepairType>();
			foreach (KeyValuePair<string, RepairType> item in PartsReplacement)
			{
				PartsReplacementFinal.Add(GameData.Instance.AllEntityTypes[item.Key], item.Value);
			}
		}
		if (PartsCondition != null)
		{
			PartsConditionFinal = new Dictionary<EntityType, RepairType>();
			foreach (KeyValuePair<string, RepairType> item2 in PartsCondition)
			{
				PartsConditionFinal.Add(GameData.Instance.AllEntityTypes[item2.Key], item2.Value);
			}
		}
		if (PartsConditionFinal != null)
		{
			foreach (KeyValuePair<EntityType, RepairType> item3 in PartsConditionFinal)
			{
				item3.Value.PostDataCompleteInitialize();
			}
		}
		if (PartsReplacementFinal == null)
		{
			return;
		}
		foreach (KeyValuePair<EntityType, RepairType> item4 in PartsReplacementFinal)
		{
			item4.Value.PostDataCompleteInitialize();
		}
	}

	public void PostDataCompleteValidate(ref List<string> listOfErrors)
	{
	}
}
