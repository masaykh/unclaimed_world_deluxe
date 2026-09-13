using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Serialization;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.Actions;

namespace UWGame.SimSide.SimEffects;

[DebuggerDisplay("{KeyName}")]
public class EffectProfileType : IGameData
{
	public string Description;

	public string Comments;

	public AIDesirability AIDesirability;

	public string[] Effects;

	public int SortOrder;

	[XmlIgnore]
	public List<EffectType> EffectTypes;

	[XmlIgnore]
	public Dictionary<AgentActionHooks, List<ActionSets>> EventActions = new Dictionary<AgentActionHooks, List<ActionSets>>();

	public string KeyName { get; set; }

	public string Name { get; set; }

	public bool DeleteRecord { get; set; }

	public bool Affects(AffectsNumbers affects)
	{
		if (EffectTypes != null && EffectTypes.Any((EffectType e) => e is NumberEffectType && ((NumberEffectType)e).Affects == affects))
		{
			return true;
		}
		return false;
	}

	public bool Affects(AffectsFlags affects)
	{
		if (EffectTypes != null && EffectTypes.Any((EffectType e) => e is FlagEffectType && ((FlagEffectType)e).Affects == affects))
		{
			return true;
		}
		return false;
	}

	public void Initialize()
	{
		EffectTypes = new List<EffectType>();
		string[] effects = Effects;
		foreach (string key in effects)
		{
			EffectTypes.Add(GameData.Instance.AllEffectTypes[key]);
		}
	}

	public void PostLoadContentInitialize()
	{
		if (!GameData.Instance.EventHooksByEffectType.TryGetValue(this, out var value))
		{
			return;
		}
		foreach (EffectTypeActionHook item in value)
		{
			List<ActionSets> value2 = null;
			if (!EventActions.TryGetValue(item.Hook, out value2))
			{
				value2 = new List<ActionSets>();
				EventActions.Add(item.Hook, value2);
			}
			value2.Add(GameData.Instance.AllActionSets[item.ActionSetsKey]);
		}
	}

	public void PreInitValidate(ref List<string> listOfErrors)
	{
	}

	public void PostInitValidate(ref List<string> listOfErrors)
	{
	}

	public void PreDataCompleteValidate(ref List<string> listOfErrors)
	{
	}

	public void PostDataCompleteInitialize()
	{
	}

	public void PostDataCompleteValidate(ref List<string> listOfErrors)
	{
	}
}
