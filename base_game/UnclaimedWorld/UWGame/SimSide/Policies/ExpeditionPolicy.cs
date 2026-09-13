using System;
using System.Collections.Generic;
using System.Linq;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Combat;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Tiers;

namespace UWGame.SimSide.Policies;

public class ExpeditionPolicy : ISnapshot
{
	public float? FractionIndependentsAllowedToSleep;

	public int? IndependentsAllowedToSleep;

	public Dictionary<RatingTypes, TierType> CurrentTiers;

	public Dictionary<EntityType, bool> AllowAmmoForVermin;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public ExpeditionPolicy()
	{
		if (Snapshotter.IsSnapshotting)
		{
			return;
		}
		CurrentTiers = new Dictionary<RatingTypes, TierType>();
		foreach (object value in Enum.GetValues(typeof(RatingTypes)))
		{
			CurrentTiers.Add((RatingTypes)value, GameData.Instance.Tiers[0]);
		}
		AllowAmmoForVermin = new Dictionary<EntityType, bool>();
	}

	public float GetWeaponPolicyScore(bool attackVermin, EntityType weapon, AttackType attackType)
	{
		if (!attackVermin)
		{
			return 1f;
		}
		if (attackType != null)
		{
			if (attackType.UsesAmmoType != null)
			{
				if (GetAllowAmmoForVermin(attackType.UsesAmmoType))
				{
					return 1f;
				}
				return 0f;
			}
			return 1f;
		}
		bool flag = false;
		AttackType[] attackTypes = weapon.ItemType.WeaponType.AttackTypes;
		foreach (AttackType attackType2 in attackTypes)
		{
			if (attackType2.UsesAmmoType != null)
			{
				if (GetAllowAmmoForVermin(attackType2.UsesAmmoType))
				{
					return 1f;
				}
				flag = true;
			}
		}
		if (flag)
		{
			return 0f;
		}
		return 1f;
	}

	public bool GetAllowAmmoForVermin(EntityType ammo)
	{
		if (AllowAmmoForVermin.TryGetValue(ammo, out var value))
		{
			return value;
		}
		return true;
	}

	public void SetAllowAmmoForVermin(EntityType ammo, bool value)
	{
		Common.AddOrUpdateDictionary(ref AllowAmmoForVermin, ammo, value);
	}

	public static ExpeditionPolicy CreateFromPolicyData(ExpeditionPolicyData policyData)
	{
		ExpeditionPolicy expeditionPolicy = new ExpeditionPolicy();
		if (policyData != null)
		{
			expeditionPolicy.IndependentsAllowedToSleep = policyData.IndependentsAllowedToSleep;
			expeditionPolicy.FractionIndependentsAllowedToSleep = policyData.FractionIndependentsAllowedToSleep;
			if (policyData.CurrentTiers != null)
			{
				expeditionPolicy.CurrentTiers = new Dictionary<RatingTypes, TierType>();
				foreach (KeyValuePair<RatingTypes, string> currentTier in policyData.CurrentTiers)
				{
					expeditionPolicy.CurrentTiers.Add(currentTier.Key, GameData.Instance.AllTierTypes[currentTier.Value]);
				}
			}
			if (policyData.AllowAmmoUseAgainstVermin != null)
			{
				foreach (KeyValuePair<string, bool> item in policyData.AllowAmmoUseAgainstVermin)
				{
					expeditionPolicy.SetAllowAmmoForVermin(GameData.Instance.AllEntityTypes[item.Key], item.Value);
				}
			}
		}
		return expeditionPolicy;
	}

	public bool SleepInShiftsIsActive()
	{
		if (!FractionIndependentsAllowedToSleep.HasValue)
		{
			return IndependentsAllowedToSleep.HasValue;
		}
		return true;
	}

	public bool TierIsUnlocked(RatingTypes rating, TierType tier)
	{
		TierType tierType = CurrentTiers[rating];
		if (tier.Index <= tierType.Index)
		{
			return true;
		}
		return false;
	}

	public bool IsLaterTier(RatingTypes ratingType, TierType tier)
	{
		TierType tierType = CurrentTiers[ratingType];
		if (tier.Index > tierType.Index + 1)
		{
			return true;
		}
		return false;
	}

	public bool RatingIsInsideOrAbovePreviousTier(RatingTypes ratingType, float rating, TierType tier, out float requiredRating)
	{
		TierType tierType = CurrentTiers[ratingType];
		TierType.GetTierBelow(tierType.Index, out var _, out var _);
		TierType.GetTierBelow(tierType.Index, out var _, out var lowerTierEdge2);
		requiredRating = lowerTierEdge2;
		if (Common.IsGreaterThanOrEqual(rating, lowerTierEdge2))
		{
			return true;
		}
		return false;
	}

	public bool CanUseProcess(ProcessType process, out TierOrAreaType policy)
	{
		TierOrAreaType tierArea = process.GetTierArea();
		if (tierArea != null && !CanProduceOrTrade(tierArea))
		{
			policy = tierArea;
			return false;
		}
		policy = null;
		return true;
	}

	public bool CanProduceOrTrade(TierOrAreaType tierOrAreaType)
	{
		if (tierOrAreaType == null)
		{
			return true;
		}
		if (tierOrAreaType.TierArea != null)
		{
			return CurrentTiers[tierOrAreaType.TierArea.Area].Index >= tierOrAreaType.TierArea.TierType.Index;
		}
		return CurrentTiers.Any((KeyValuePair<RatingTypes, TierType> t) => t.Value.Index >= tierOrAreaType.TierType.Index);
	}

	public void AdoptTierPolicy(TierType tier, RatingTypes rating)
	{
		CurrentTiers[rating] = tier;
	}

	public float GetPolicyMinimum(RatingTypes rating)
	{
		int index = CurrentTiers[rating].Index;
		if (index > 0)
		{
			return GameData.Instance.Tiers[index - 1].UpperEdge;
		}
		return 0f;
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		IndependentsAllowedToSleep = sn.DoInt32Nullable(IndependentsAllowedToSleep);
		FractionIndependentsAllowedToSleep = sn.DoFloatNullable(FractionIndependentsAllowedToSleep);
		CurrentTiers = sn.DoDictionary(CurrentTiers);
		AllowAmmoForVermin = sn.DoDictionary(AllowAmmoForVermin);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
	}
}
