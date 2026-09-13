using System.Collections.Generic;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.AI;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities;

public class Skill : IHasExposedProperties, ISnapshot
{
	private float value;

	private float alltimeMaxValue;

	private float potential;

	public SkillType SkillType;

	private float productionFactor;

	private static Dictionary<string, GetPropertyValue> exposedPropertyValueFunctions;

	private Dictionary<string, PropertyResult> customFields;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public float Value
	{
		get
		{
			return value;
		}
		set
		{
			this.value = value;
			productionFactor = GetSkillProductionFactor(value);
		}
	}

	public float ProductionFactor => productionFactor;

	public string KeyName => SkillType.KeyName;

	public bool IsSnapshotted { get; set; }

	public static float GetSkillProductionFactor(float value)
	{
		return Common.Clamp(-0.65f * (value * value) + 1.3f * value + 0.35f, 0f, 1f);
	}

	public Skill(float value, SkillType skillType)
	{
		SkillType = skillType;
		Value = value;
	}

	public Skill()
	{
	}

	public override string ToString()
	{
		return ((int)(100f * value)).ToString();
	}

	public float CalculateProgress(double secondsElapsed, float manSecondsOfWorkNeeded, float? upperSkillBound)
	{
		return (float)(secondsElapsed / (double)manSecondsOfWorkNeeded * (double)((!upperSkillBound.HasValue) ? ProductionFactor : Common.ClampTop(upperSkillBound.Value, ProductionFactor)));
	}

	static Skill()
	{
		exposedPropertyValueFunctions = new Dictionary<string, GetPropertyValue>();
		exposedPropertyValueFunctions.Add("skillLevel", GetSkillLevel);
		exposedPropertyValueFunctions.Add("skillDescription", GetSkillDescription);
	}

	public void GetChildren(string keyToList, ref List<IHasExposedProperties> listToFillWithProperties, FilterCondition filter, EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget, SharedKnowledge getterKnowledge = null)
	{
	}

	public string GetCaption(string captionKey)
	{
		return null;
	}

	public PropertyResult? GetPropertyValue(string propertyKey, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		PropertyResult? result = null;
		if (exposedPropertyValueFunctions.ContainsKey(propertyKey))
		{
			return exposedPropertyValueFunctions[propertyKey](this, getterKnowledge, parent);
		}
		if (customFields != null && customFields.TryGetValue(propertyKey, out var propertyResult))
		{
			result = propertyResult;
		}
		return result;
	}

	public string GetDefaultCaption(string propertyKey)
	{
		return SkillType.Name;
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

	public void SetPropertyValue(string propertyKey, PropertyResult? value)
	{
		Entity.SetPropertyValue(ref customFields, propertyKey, value);
	}

	public static PropertyResult? GetSkillLevel(IHasExposedProperties anoObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Skill)anoObjectToGetValueFrom).GetSkillLevel();
	}

	public PropertyResult? GetSkillLevel()
	{
		return new PropertyResult
		{
			NumberResult = value
		};
	}

	public static PropertyResult? GetSkillDescription(IHasExposedProperties anoObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((Skill)anoObjectToGetValueFrom).GetSkillDescription();
	}

	public PropertyResult? GetSkillDescription()
	{
		return new PropertyResult
		{
			StringResult = SkillType.Description
		};
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		alltimeMaxValue = sn.DoFloat(alltimeMaxValue);
		customFields = sn.DoDictionary(customFields);
		potential = sn.DoFloat(potential);
		productionFactor = sn.DoFloat(productionFactor);
		SkillType = sn.DoGameData(SkillType);
		value = sn.DoFloat(value);
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
	}
}
