using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.InGameEvents.Conditions;

namespace UWGame.SimSide.Items;

public class FoodNutrientAmount : IXmlSerializable, IHasExposedProperties
{
	public FoodNutrientType Nutrient;

	public float Amount;

	private static Dictionary<string, GetPropertyValue> exposedPropertyValueFunctions;

	private Dictionary<string, PropertyResult> customFields;

	public static readonly CustomXmlSerializer.XmlProxyData _proxyData;

	public string KeyName => Nutrient.KeyName;

	static FoodNutrientAmount()
	{
		exposedPropertyValueFunctions = new Dictionary<string, GetPropertyValue>();
		_proxyData = new CustomXmlSerializer.XmlProxyData(typeof(FoodNutrientAmount))
		{
			TypeMappings = DataLoader.GetListOfTypeMappings()
		};
		exposedPropertyValueFunctions.Add("nutrientLevel", GetNutrientLevel);
		exposedPropertyValueFunctions.Add("satisfiedDailyIntake", GetSatisfiedDailyIntake);
	}

	public void GetChildren(string keyToList, ref List<IHasExposedProperties> listToFillWithProperties, FilterCondition filter, EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget, SharedKnowledge getterKnowledge = null)
	{
	}

	public string GetDefaultCaption(string propertyKey)
	{
		return Nutrient.Name;
	}

	public void GetDefaultKey(out string PropertyKey)
	{
		PropertyKey = null;
	}

	public EntityID? GetEntityID()
	{
		return null;
	}

	public bool GetIsSeenDirectly()
	{
		return true;
	}

	public string GetCaption(string captionKey)
	{
		return null;
	}

	public PropertyResult? GetPropertyValue(string propertyKey, SharedKnowledge getterKnowledge, IHasExposedProperties parent)
	{
		PropertyResult? result = null;
		if (exposedPropertyValueFunctions.ContainsKey(propertyKey))
		{
			return exposedPropertyValueFunctions[propertyKey](this, getterKnowledge, parent);
		}
		if (customFields != null && customFields.TryGetValue(propertyKey, out var value))
		{
			result = value;
		}
		return result;
	}

	public void SetPropertyValue(string propertyKey, PropertyResult? value)
	{
		Entity.SetPropertyValue(ref customFields, propertyKey, value);
	}

	public static PropertyResult? GetNutrientLevel(IHasExposedProperties anoObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent)
	{
		return ((FoodNutrientAmount)anoObjectToGetValueFrom).GetNutrientLevel();
	}

	public static PropertyResult? GetSatisfiedDailyIntake(IHasExposedProperties anoObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent)
	{
		return ((FoodNutrientAmount)anoObjectToGetValueFrom).GetSatisfiedDailyIntake(getterKnowledge, parent);
	}

	public PropertyResult? GetNutrientLevel()
	{
		return new PropertyResult
		{
			NumberResult = Amount
		};
	}

	public PropertyResult? GetSatisfiedDailyIntake(SharedKnowledge sharedKnowledge, IHasExposedProperties parent)
	{
		float bulk = ((IKnownEntityData)parent).Bulk;
		float weight;
		NeedType[] adultNeedsAndWeight = sharedKnowledge.Allegiance.RepresentativeEntityType.BiologicalType.GetAdultNeedsAndWeight(out weight);
		if (adultNeedsAndWeight != null)
		{
			string satisfiedDailyIntake = GetSatisfiedDailyIntake(bulk, adultNeedsAndWeight, weight);
			return new PropertyResult
			{
				StringResult = satisfiedDailyIntake
			};
		}
		return null;
	}

	public string GetSatisfiedDailyIntake(float itemBulk, NeedType[] adultNeeds, float consumerWeight)
	{
		float bulkFromWeight = BiologicalEntity.GetBulkFromWeight(consumerWeight);
		NeedType needType = adultNeeds.FirstOrDefault((NeedType n) => n.KeyName == Nutrient.KeyName);
		if (needType != null)
		{
			float num = Amount * itemBulk;
			float num2 = (float)((double)(needType.FoodNeedType.RequiredNutrientsAsFractionOfEntityBulk * bulkFromWeight) * needType.DecreasePerDay.Mean).Value;
			float num3 = ((!(num2 > 0f)) ? 1f : (num / num2));
			return Common.PercentageToString(num3);
		}
		return null;
	}

	public XmlSchema GetSchema()
	{
		return null;
	}

	public void ReadXml(XmlReader reader)
	{
		CustomXmlSerializer.ReadXmlDeserialize(this, reader, _proxyData);
	}

	public void WriteXml(XmlWriter writer)
	{
		CustomXmlSerializer.WriteXmlSerialize(this, writer, _proxyData);
	}
}
