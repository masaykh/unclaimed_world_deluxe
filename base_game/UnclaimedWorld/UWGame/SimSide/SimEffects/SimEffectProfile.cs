using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.SimEffects;

public class SimEffectProfile : ISnapshot, IHasExposedProperties
{
	public EffectProfileType EffectProfileType;

	public List<SimEffect> Effects;

	private List<SimEffectID> snapshotEffects;

	private static Dictionary<string, GetPropertyValue> exposedPropertyValueFunctions;

	private Dictionary<string, PropertyResult> customFields;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public string KeyName => EffectProfileType.KeyName;

	public bool IsSnapshotted { get; set; }

	public SimEffectProfile()
	{
	}

	public SimEffectProfile(EffectProfileType effectType, Entity onEntity)
	{
		EffectProfileType = effectType;
		Effects = new List<SimEffect>();
		foreach (EffectType effectType2 in EffectProfileType.EffectTypes)
		{
			SimEffect item = new SimEffect(effectType2);
			Effects.Add(item);
		}
		if (onEntity != null)
		{
			EffectProfileType.EventActions.TryGetValue(AgentActionHooks.StartedEffect, out var value);
			Goal.FireEventActions(onEntity, null, value);
		}
	}

	static SimEffectProfile()
	{
		exposedPropertyValueFunctions = new Dictionary<string, GetPropertyValue>();
		exposedPropertyValueFunctions.Add("noOfEffects", GetNoOfEffects);
		exposedPropertyValueFunctions.Add("description", GetDescription);
		exposedPropertyValueFunctions.Add("name", GetName);
	}

	public void Destroy(Entity onEntity, bool wasReplaced)
	{
		foreach (SimEffect effect in Effects)
		{
			effect.Destroy();
		}
		if (!wasReplaced && onEntity != null)
		{
			EffectProfileType.EventActions.TryGetValue(AgentActionHooks.EndedEffect, out var value);
			Goal.FireEventActions(onEntity, null, value);
		}
	}

	public static PropertyResult? GetNoOfEffects(IHasExposedProperties anoObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((SimEffectProfile)anoObjectToGetValueFrom).GetNoOfEffects();
	}

	public PropertyResult? GetNoOfEffects()
	{
		return new PropertyResult
		{
			NumberResult = EffectProfileType.EffectTypes.Count
		};
	}

	public static PropertyResult? GetName(IHasExposedProperties anoObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((SimEffectProfile)anoObjectToGetValueFrom).GetName();
	}

	public PropertyResult? GetName()
	{
		return new PropertyResult
		{
			StringResult = EffectProfileType.Name
		};
	}

	public static PropertyResult? GetDescription(IHasExposedProperties anoObjectToGetValueFrom, SharedKnowledge getterKnowledge, IHasExposedProperties parent = null)
	{
		return ((SimEffectProfile)anoObjectToGetValueFrom).GetDescription();
	}

	public PropertyResult? GetDescription()
	{
		PropertyResult value = default(PropertyResult);
		StringBuilder stringBuilder = new StringBuilder();
		Common.AppendHeaderOnLightBG(stringBuilder, EffectProfileType.Name);
		Common.Append(stringBuilder, EffectProfileType.Description);
		Common.AppendDividerOnOwnLine(stringBuilder);
		foreach (SimEffect effect in Effects)
		{
			effect.EffectType.AppendAsString(stringBuilder, EffectType.Background.White);
			Common.AppendLine(stringBuilder);
		}
		value.StringResult = stringBuilder.ToString();
		return value;
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
		if (customFields != null && customFields.TryGetValue(propertyKey, out var value))
		{
			result = value;
		}
		return result;
	}

	public string GetDefaultCaption(string propertyKey)
	{
		return EffectProfileType.Name;
	}

	public void GetDefaultKey(out string key)
	{
		key = KeyName;
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

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		EffectProfileType = sn.DoGameData(EffectProfileType);
		if (Effects != null)
		{
			snapshotEffects = Effects.Select((SimEffect e) => e.ID).ToList();
		}
		snapshotEffects = sn.DoList(snapshotEffects);
		customFields = sn.DoDictionary(customFields);
		sn.Ignore(Effects);
		sn.Ignore(exposedPropertyValueFunctions);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		if (snapshotEffects != null)
		{
			Effects = snapshotEffects.Select((SimEffectID e) => LookUp<SimEffect, SimEffectID>.FindByID(e)).ToList();
		}
	}
}
