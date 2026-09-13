using System;
using System.Collections.Generic;
using System.Linq;
using UWGame.SimSide.SimEffects;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems;

namespace UWGame.SimSide.Entities;

public class SimEffectsComponent : Component
{
	public List<SimEffectProfile> EffectProfiles;

	public Dictionary<AffectsNumbers, List<SimEffect>> NumberEffects;

	private Dictionary<AffectsNumbers, List<SimEffectID>> snapshotNumberEffects;

	public Dictionary<AffectsFlags, List<SimEffect>> FlagEffects;

	private Dictionary<AffectsFlags, List<SimEffectID>> snapshotFlagEffects;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public SimEffectsComponent()
	{
	}

	public SimEffectsComponent(Entity parent)
		: base(parent, 1.0 / (double)GameData.Instance.Constants.UpdateIntervalForEntityComponents)
	{
		NumberEffects = new Dictionary<AffectsNumbers, List<SimEffect>>();
		FlagEffects = new Dictionary<AffectsFlags, List<SimEffect>>();
		EffectProfiles = new List<SimEffectProfile>();
	}

	public void Start(EffectProfileType effect)
	{
		Remove(effect);
		SimEffectProfile simEffectProfile = new SimEffectProfile(effect, Parent);
		EffectProfiles.Add(simEffectProfile);
		foreach (SimEffect effect2 in simEffectProfile.Effects)
		{
			if (effect2.EffectType is NumberEffectType numberEffectType)
			{
				Common.AddToMultiList(NumberEffects, numberEffectType.Affects, effect2);
			}
			if (effect2.EffectType is FlagEffectType flagEffectType)
			{
				Common.AddToMultiList(FlagEffects, flagEffectType.Affects, effect2);
			}
		}
		Parent.RecomputeUpdateInterval();
	}

	public void Remove(EffectProfileType effect)
	{
		SimEffectProfile simEffectProfile = EffectProfiles.FirstOrDefault((SimEffectProfile e) => e.EffectProfileType == effect);
		if (simEffectProfile != null)
		{
			DestroyProfile(simEffectProfile, wasReplaced: true);
		}
	}

	private void DestroyProfile(SimEffectProfile existingProfile, bool wasReplaced)
	{
		existingProfile.Destroy(Parent, wasReplaced);
		EffectProfiles.Remove(existingProfile);
		foreach (SimEffect effect in existingProfile.Effects)
		{
			RemoveEffect(effect);
		}
	}

	private void RemoveEffect(SimEffect item)
	{
		if (item.EffectType is NumberEffectType numberEffectType)
		{
			Common.RemoveFromMultiList(NumberEffects, numberEffectType.Affects, item);
		}
		if (item.EffectType is FlagEffectType flagEffectType)
		{
			Common.RemoveFromMultiList(FlagEffects, flagEffectType.Affects, item);
		}
	}

	public bool GetEffect(AffectsFlags affects, bool baseValue, string typeKey = null, string typeTag = null, List<Tuple<string, bool>> effectComponents = null)
	{
		bool flag = baseValue;
		if (FlagEffects.TryGetValue(affects, out var value))
		{
			IEnumerable<SimEffect> effectsToIterate = GetEffectsToIterate(typeKey, typeTag, value);
			if (effectsToIterate != null)
			{
				foreach (SimEffect item in effectsToIterate)
				{
					FlagEffectType flagEffectType = item.EffectType as FlagEffectType;
					flag = flag && flagEffectType.Value;
					GatherComponent(effectComponents, item, flagEffectType);
				}
			}
		}
		return flag;
	}

	private static void GatherComponent(List<Tuple<string, bool>> effectComponents, SimEffect item, FlagEffectType flagEffect)
	{
		effectComponents?.Add(new Tuple<string, bool>(item.EffectType.Name, flagEffect.Value));
	}

	private static void GatherComponent(List<Tuple<string, NumberEffectOperator, float>> effectComponents, SimEffect item, NumberEffectType effect, float componentValue)
	{
		effectComponents?.Add(new Tuple<string, NumberEffectOperator, float>(item.EffectType.Name, effect.Operator, componentValue));
	}

	public float GetEffect(AffectsNumbers affects, float baseValue, string typeKey = null, string typeTag = null, List<Tuple<string, NumberEffectOperator, float>> effectComponents = null)
	{
		float num = baseValue;
		if (NumberEffects.TryGetValue(affects, out var value))
		{
			IEnumerable<SimEffect> effectsToIterate = GetEffectsToIterate(typeKey, typeTag, value);
			if (effectsToIterate != null)
			{
				foreach (SimEffect item in effectsToIterate)
				{
					NumberEffectType numberEffectType = item.EffectType as NumberEffectType;
					if (numberEffectType.Operator == NumberEffectOperator.Multiply)
					{
						float value2 = item.GetValue();
						num *= value2;
						GatherComponent(effectComponents, item, numberEffectType, value2);
					}
				}
				foreach (SimEffect item2 in effectsToIterate)
				{
					NumberEffectType numberEffectType2 = item2.EffectType as NumberEffectType;
					if (numberEffectType2.Operator == NumberEffectOperator.Add)
					{
						float value3 = item2.GetValue();
						num += value3;
						GatherComponent(effectComponents, item2, numberEffectType2, value3);
					}
				}
			}
		}
		return num;
	}

	private static IEnumerable<SimEffect> GetEffectsToIterate(string typeKey, string typeTag, List<SimEffect> effects)
	{
		IEnumerable<SimEffect> enumerable = null;
		if (typeKey != null || typeTag != null)
		{
			HashSet<SimEffect> list = null;
			if (typeKey != null)
			{
				foreach (SimEffect effect in effects)
				{
					if (effect.EffectType.AffectsTypeKey != null && effect.EffectType.AffectsTypeKey.Contains(typeKey))
					{
						Common.AddToList(ref list, effect);
					}
				}
			}
			if (typeTag != null)
			{
				foreach (SimEffect effect2 in effects)
				{
					if (effect2.EffectType.AffectsTypeTag != null && effect2.EffectType.AffectsTypeTag.Contains(typeTag))
					{
						Common.AddToList(ref list, effect2);
					}
				}
			}
			return list;
		}
		return effects;
	}

	public override double? GetUpdateInterval()
	{
		double? currentInterval = null;
		foreach (KeyValuePair<AffectsNumbers, List<SimEffect>> numberEffect in NumberEffects)
		{
			foreach (SimEffect item in numberEffect.Value)
			{
				if (item.ExpiresOn.HasValue)
				{
					UpdateTimePoints.GetSoonestInterval(UpdateTimePoints.ComputeIntervalFromTimepoint(item.ExpiresOn), ref currentInterval);
				}
			}
		}
		return currentInterval;
	}

	protected override void UpdatePlaySiteRegulated(double? timeSinceLastUpdate)
	{
		base.UpdatePlaySiteRegulated(timeSinceLastUpdate);
		List<SimEffect> list = null;
		foreach (SimEffectProfile effectProfile in EffectProfiles)
		{
			foreach (SimEffect effect in effectProfile.Effects)
			{
				effect.UpdateExpiry(out var wasDestroyed);
				if (wasDestroyed)
				{
					Common.AddToList(ref list, effect);
				}
			}
		}
		if (list == null)
		{
			return;
		}
		foreach (SimEffect item in list)
		{
			RemoveEffect(item);
			foreach (SimEffectProfile effectProfile2 in EffectProfiles)
			{
				effectProfile2.Effects.Remove(item);
			}
		}
		for (int num = EffectProfiles.Count - 1; num >= 0; num--)
		{
			SimEffectProfile simEffectProfile = EffectProfiles[num];
			if (simEffectProfile.Effects.Count == 0)
			{
				DestroyProfile(simEffectProfile, wasReplaced: false);
			}
		}
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		EffectProfiles = sn.DoList(EffectProfiles);
		if (NumberEffects != null)
		{
			snapshotNumberEffects = new Dictionary<AffectsNumbers, List<SimEffectID>>();
			foreach (KeyValuePair<AffectsNumbers, List<SimEffect>> numberEffect in NumberEffects)
			{
				snapshotNumberEffects[numberEffect.Key] = numberEffect.Value.Select((SimEffect s) => s.ID).ToList();
			}
		}
		if (FlagEffects != null)
		{
			snapshotFlagEffects = new Dictionary<AffectsFlags, List<SimEffectID>>();
			foreach (KeyValuePair<AffectsFlags, List<SimEffect>> flagEffect in FlagEffects)
			{
				snapshotFlagEffects[flagEffect.Key] = flagEffect.Value.Select((SimEffect s) => s.ID).ToList();
			}
		}
		snapshotNumberEffects = sn.DoMultiMap(snapshotNumberEffects);
		snapshotFlagEffects = sn.DoMultiMap(snapshotFlagEffects);
		sn.Ignore(NumberEffects);
		sn.Ignore(FlagEffects);
		return this;
	}

	public override Snapshotter.Version DoVersion(Snapshotter sn)
	{
		base.DoVersion(sn);
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		base.LoadPostProcess(sn);
		if (snapshotNumberEffects != null)
		{
			NumberEffects = new Dictionary<AffectsNumbers, List<SimEffect>>();
			foreach (KeyValuePair<AffectsNumbers, List<SimEffectID>> snapshotNumberEffect in snapshotNumberEffects)
			{
				NumberEffects.Add(snapshotNumberEffect.Key, snapshotNumberEffect.Value.Select((SimEffectID s) => LookUp<SimEffect, SimEffectID>.FindByID(s)).ToList());
			}
			snapshotNumberEffects.Clear();
		}
		if (snapshotFlagEffects != null)
		{
			FlagEffects = new Dictionary<AffectsFlags, List<SimEffect>>();
			foreach (KeyValuePair<AffectsFlags, List<SimEffectID>> snapshotFlagEffect in snapshotFlagEffects)
			{
				FlagEffects.Add(snapshotFlagEffect.Key, snapshotFlagEffect.Value.Select((SimEffectID s) => LookUp<SimEffect, SimEffectID>.FindByID(s)).ToList());
			}
			snapshotFlagEffects.Clear();
		}
		if (EffectProfiles == null)
		{
			return;
		}
		foreach (SimEffectProfile effectProfile in EffectProfiles)
		{
			effectProfile.LoadPostProcess(sn);
		}
	}
}
