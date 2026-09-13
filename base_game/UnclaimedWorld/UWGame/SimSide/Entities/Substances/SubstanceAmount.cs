using System.Collections.Generic;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.AI;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities.Substances;

public class SubstanceAmount : ISnapshot, IHasExposedProperties
{
	public SubstanceType SubstanceType;

	public float Amount;

	private static Dictionary<string, GetPropertyValue> exposedPropertyValueFunctions;

	private Dictionary<string, PropertyResult> customFields;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public string KeyName => SubstanceType.KeyName;

	public bool IsSnapshotted { get; set; }

	public SubstanceAmount()
	{
	}

	public SubstanceAmount(float amount, SubstanceType type)
	{
		SubstanceType = type;
		Amount = amount;
	}

	public SubstanceAmount(SubstanceAmount original)
	{
		Amount = original.Amount;
		SubstanceType = original.SubstanceType;
	}

	static SubstanceAmount()
	{
		exposedPropertyValueFunctions = new Dictionary<string, GetPropertyValue>();
		exposedPropertyValueFunctions.Add("substanceLevel", GetSubstanceAmount);
	}

	public void GetChildren(string keyToList, ref List<IHasExposedProperties> listToFillWithProperties, FilterCondition filter, EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget, SharedKnowledge getterKnowledge = null)
	{
	}

	public string GetDefaultCaption(string propertyKey)
	{
		return SubstanceType.Name;
	}

	public void GetDefaultKey(out string PropertyKey)
	{
		PropertyKey = null;
	}

	public EntityID? GetEntityID()
	{
		return null;
	}

	public string GetCaption(string captionKey)
	{
		return null;
	}

	public bool GetIsSeenDirectly()
	{
		return true;
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

	public static PropertyResult? GetSubstanceAmount(IHasExposedProperties anoObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((SubstanceAmount)anoObjectToGetValueFrom).GetSubstanceLevel();
	}

	public PropertyResult? GetSubstanceLevel()
	{
		return new PropertyResult
		{
			NumberResult = Amount * 100f
		};
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		Amount = sn.DoFloat(Amount);
		customFields = sn.DoDictionary(customFields);
		SubstanceType = sn.DoGameData(SubstanceType);
		sn.Ignore(exposedPropertyValueFunctions);
		return this;
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		if (customFields == null)
		{
			return;
		}
		foreach (KeyValuePair<string, PropertyResult> customField in customFields)
		{
			customField.Value.LoadPostProcess(sn);
		}
	}
}
