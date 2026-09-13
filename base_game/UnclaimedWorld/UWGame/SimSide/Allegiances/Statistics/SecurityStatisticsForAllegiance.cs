using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Constants.Rating;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Combat;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Allegiances.Statistics;

public class SecurityStatisticsForAllegiance : SecurityStatistics
{
	public List<DataPoint<float>> SharedRating = new List<DataPoint<float>>();

	private Snapshotter.Version version;

	public SecurityStatisticsForAllegiance()
	{
	}

	public SecurityStatisticsForAllegiance(GroupStatistics parent)
		: base(parent)
	{
	}

	public float GetSharedRatings()
	{
		if (SharedRating.Count > 0)
		{
			return SharedRating.Last().Value;
		}
		return 0f;
	}

	private Dictionary<EntityType, int> GatherAmmoData()
	{
		ICanIterateEntities canIterateEntities = LookUpICanIterateEntities.FindByID(Parent.CanIterateEntitiesID);
		Dictionary<EntityType, int> ammoRounds = new Dictionary<EntityType, int>();
		canIterateEntities.IterateOwnedItems(delegate(EntityGroup e)
		{
			GatherAmmoData(e, ref ammoRounds);
		});
		return ammoRounds;
	}

	private void ScoreDefensiveAgents(float assets, Dictionary<EntityType, int> ammoRounds, out int noOfDefensiveAgents, out float averageRating, out float totalScore, out float finalAgentsRating)
	{
		totalScore = 0f;
		noOfDefensiveAgents = 0;
		averageRating = 0f;
		ICanIterateEntities canIterateEntities = LookUpICanIterateEntities.FindByID(Parent.CanIterateEntitiesID);
		_ = canIterateEntities.GetAllegiance;
		Security security = GameData.Instance.AIConstants.Ratings.Security;
		int agents = 0;
		float score = 0f;
		float defenseRating;
		canIterateEntities.IterateMembers(delegate(Entity e)
		{
			defenseRating = GetAgentDefenseRating(e, security, ammoRounds);
			if (!Common.IsZero(defenseRating))
			{
				agents++;
				score += defenseRating;
			}
		});
		totalScore = score;
		noOfDefensiveAgents = agents;
		averageRating = score;
		if (noOfDefensiveAgents > 0)
		{
			averageRating /= noOfDefensiveAgents;
		}
		if (!Common.IsZero(assets))
		{
			finalAgentsRating = totalScore / assets;
		}
		else
		{
			finalAgentsRating = totalScore;
		}
	}

	public override void AddSharedRating(float ratingValue)
	{
		base.AddSharedRating(ratingValue);
		DateAndTime.TimeDateYear currentTimeDateYear = The.Sim.DateAndTime.CurrentTimeDateYear;
		SharedRating.Add(new DataPoint<float>
		{
			Time = currentTimeDateYear,
			Value = ratingValue
		});
	}

	private void ScoreHandWeapons(float assets, Dictionary<EntityType, int> ammoRounds, out int noOfWeaponCarriers, out int noOfWeapons, out float averageRating, out float totalHandWeaponsScore, out float finalHandWeaponsScore)
	{
		totalHandWeaponsScore = 0f;
		noOfWeapons = 0;
		noOfWeaponCarriers = 0;
		averageRating = 0f;
		finalHandWeaponsScore = 0f;
		if (Parent.RepresentativeEntityType.IntelligenceType.CanUseWeapons != true)
		{
			return;
		}
		Security security = GameData.Instance.AIConstants.Ratings.Security;
		int weaponCarriers = 0;
		ICanIterateEntities canIterateEntities = LookUpICanIterateEntities.FindByID(Parent.CanIterateEntitiesID);
		Allegiance getAllegiance = canIterateEntities.GetAllegiance;
		canIterateEntities.IterateMembers(delegate(Entity e)
		{
			if (e.EntityType.IntelligenceType.CanUseWeapons == true)
			{
				weaponCarriers++;
			}
		});
		int num = (int)((float)weaponCarriers * security.MaxHandWeaponsToScorePerMember);
		List<EntityID> weapons = null;
		canIterateEntities.IterateOwnedItems(delegate(EntityGroup e)
		{
			GatherHandWeaponsData(weaponCarriers, e, ref weapons);
		});
		noOfWeaponCarriers = weaponCarriers;
		List<Tuple<IKnownEntityData, float>> list = new List<Tuple<IKnownEntityData, float>>();
		if (weapons != null)
		{
			foreach (EntityID item in weapons)
			{
				if (!GoalEvaluator.EntityDataResultCausesSkip(getAllegiance.SharedKnowledge.GetKnownData(item, out var data)) && IsUsableWeapon(data, security, ammoRounds, out var defenseRating))
				{
					list.Add(new Tuple<IKnownEntityData, float>(data, defenseRating));
				}
			}
		}
		noOfWeapons = list.Count;
		int num2 = 0;
		if (list.Count > num)
		{
			foreach (Tuple<IKnownEntityData, float> item2 in list.OrderByDescending((Tuple<IKnownEntityData, float> t) => t.Item2))
			{
				totalHandWeaponsScore += item2.Item2;
				num2++;
				if (num2 >= num)
				{
					break;
				}
			}
		}
		else
		{
			num2 = list.Count;
			totalHandWeaponsScore = list.Sum((Tuple<IKnownEntityData, float> t) => t.Item2);
		}
		if (num2 > 0)
		{
			averageRating = totalHandWeaponsScore / (float)num2;
		}
		if (!Common.IsZero(assets))
		{
			finalHandWeaponsScore = totalHandWeaponsScore / assets;
		}
		else
		{
			finalHandWeaponsScore = totalHandWeaponsScore;
		}
	}

	private void GatherHandWeaponsData(int noOfMembers, EntityGroup ownedItems, ref List<EntityID> weapons)
	{
		foreach (KeyValuePair<EntityType, List<EntityID>> item in ownedItems.Items)
		{
			if (AffectsHandWeaponsRating(item.Key))
			{
				Common.AddToList(ref weapons, item.Value);
			}
		}
	}

	public static bool AffectsHandWeaponsRating(EntityType entityType)
	{
		if (entityType.ItemType != null && entityType.ItemType.WeaponType != null && !entityType.IsIntrinsic() && entityType.IsMountable() && !Common.IsZero(entityType.ItemType.WeaponType.GetHighestDefenseRating()))
		{
			return true;
		}
		return false;
	}

	public static bool AffectsDefenderRating(EntityType entityType)
	{
		if (entityType.IntelligenceType != null)
		{
			if (entityType.IntelligenceType.IntrinsicWeaponTypes != null)
			{
				foreach (EntityType intrinsicWeaponType in entityType.IntelligenceType.IntrinsicWeaponTypes)
				{
					AttackType[] attackTypes = intrinsicWeaponType.ItemType.WeaponType.AttackTypes;
					for (int i = 0; i < attackTypes.Length; i++)
					{
						if (attackTypes[i].DefenseRating.HasValue)
						{
							return true;
						}
					}
				}
			}
			if (entityType.IntelligenceType.AttackTypes != null)
			{
				foreach (AttackType attackType in entityType.IntelligenceType.AttackTypes)
				{
					if (attackType.DefenseRating.HasValue)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public static bool AffectsSecurityRating(EntityType entityType)
	{
		if (!AffectsHandWeaponsRating(entityType))
		{
			return AffectsDefenderRating(entityType);
		}
		return true;
	}

	private void GatherAmmoData(EntityGroup ownedItems, ref Dictionary<EntityType, int> ammoRounds)
	{
		foreach (KeyValuePair<EntityType, OwnerAmmoOfType> ammoItem in ownedItems.AmmoItems)
		{
			int num = ammoItem.Value.TotalRounds;
			if (ammoRounds.TryGetValue(ammoItem.Key, out var value))
			{
				num += value;
			}
			ammoRounds[ammoItem.Key] = num;
		}
	}

	private bool IsUsableWeapon(IKnownEntityData weapon, Security security, Dictionary<EntityType, int> ammoRounds, out float defenseRating)
	{
		if (!AffectsHandWeaponsRating(weapon.EntityType) || !weapon.IsCompleted() || !Entity.IsFunctional(weapon))
		{
			defenseRating = 0f;
			return false;
		}
		defenseRating = GetWeaponDefenseRating(weapon, security, ammoRounds);
		return !Common.IsZero(defenseRating);
	}

	private static float GetAgentDefenseRating(Entity agent, Security security, Dictionary<EntityType, int> ammoRounds)
	{
		float bestDefenseRating = 0f;
		AttackType bestAttackType = null;
		Intelligence intelligence = agent.Intelligence;
		if (intelligence.IntrinsicWeapons != null)
		{
			foreach (KeyValuePair<EntityType, EntityID> intrinsicWeapon in intelligence.IntrinsicWeapons)
			{
				AttackType[] attackTypes = intrinsicWeapon.Key.ItemType.WeaponType.AttackTypes;
				foreach (AttackType attackType in attackTypes)
				{
					GetAttackTypeDefenseRating(security, ammoRounds, ref bestDefenseRating, ref bestAttackType, attackType);
				}
			}
		}
		if (agent.EntityType.IntelligenceType.AttackTypes != null)
		{
			foreach (AttackType attackType2 in agent.EntityType.IntelligenceType.AttackTypes)
			{
				GetAttackTypeDefenseRating(security, ammoRounds, ref bestDefenseRating, ref bestAttackType, attackType2);
			}
		}
		if (bestAttackType != null)
		{
			ConsumeAmmo(security, ammoRounds, bestAttackType);
		}
		return bestDefenseRating;
	}

	private static float GetWeaponDefenseRating(IKnownEntityData weapon, Security security, Dictionary<EntityType, int> ammoRounds)
	{
		float bestDefenseRating = 0f;
		AttackType bestAttackType = null;
		AttackType[] attackTypes = weapon.EntityType.ItemType.WeaponType.AttackTypes;
		foreach (AttackType attackType in attackTypes)
		{
			GetAttackTypeDefenseRating(security, ammoRounds, ref bestDefenseRating, ref bestAttackType, attackType);
		}
		if (bestAttackType != null)
		{
			ConsumeAmmo(security, ammoRounds, bestAttackType);
		}
		return bestDefenseRating;
	}

	private static void ConsumeAmmo(Security security, Dictionary<EntityType, int> ammoRounds, AttackType bestAttackType)
	{
		if (bestAttackType.UsesAmmoType != null && ammoRounds.TryGetValue(bestAttackType.UsesAmmoType, out var value))
		{
			int num = bestAttackType.RoundsToSpend ?? 0;
			int num2 = (int)(security.MinimumNoOfAttacksForWeaponToCount * (float)num);
			if (value >= num2)
			{
				value -= num2;
				ammoRounds[bestAttackType.UsesAmmoType] = value;
			}
		}
	}

	private static void GetAttackTypeDefenseRating(Security security, Dictionary<EntityType, int> ammoRounds, ref float bestDefenseRating, ref AttackType bestAttackType, AttackType attackType)
	{
		if (!attackType.DefenseRating.HasValue || !(attackType.DefenseRating.Value > bestDefenseRating))
		{
			return;
		}
		if (attackType.UsesAmmoType != null)
		{
			if (ammoRounds.TryGetValue(attackType.UsesAmmoType, out var value))
			{
				int num = attackType.RoundsToSpend ?? 0;
				int num2 = (int)(security.MinimumNoOfAttacksForWeaponToCount * (float)num);
				if (value >= num2)
				{
					bestDefenseRating = attackType.DefenseRating.Value;
					bestAttackType = attackType;
				}
			}
		}
		else
		{
			bestDefenseRating = attackType.DefenseRating.Value;
			bestAttackType = attackType;
		}
	}

	public void ComposeRatingBreakdown(float rating, float assets, int noOfMembers, int injuries, int deaths, float injuryContribution, float deathContribution, int noOfWeaponCarriers, int noOfWeapons, float totalHandWeaponsScore, float handWeaponsAverageRating, float finalHandWeaponsRating, int noOfDefensiveAgents, float totalAgentsRating, float agentsAverageRating, float finalAgentsRating)
	{
		StringBuilder stringBuilder = new StringBuilder();
		Security security = GameData.Instance.AIConstants.Ratings.Security;
		int num = (int)(security.MaxHandWeaponsToScorePerMember * (float)noOfWeaponCarriers);
		Common.AppendLine(stringBuilder, "How well the colony provides security:");
		AppendComponent(stringBuilder, "COLONY SECURITY CONDITIONS", rating, null, null, indent: false, omitIfZero: false, formatAsPercentage: true);
		Common.AppendDivider(stringBuilder);
		Common.AppendLine(stringBuilder, "Based on:");
		Common.AppendLine(stringBuilder);
		Common.AppendLine(stringBuilder, "ASSETS (what needs protection)");
		Common.Append(stringBuilder, "No. of colony members ");
		Common.Append(stringBuilder, noOfMembers.ToString(), tintAsValue: true);
		Common.AppendLine(stringBuilder);
		Common.Append(stringBuilder, "Total: ");
		Common.AppendFormat(stringBuilder, "{0:N2}", true, assets);
		Common.AppendLine(stringBuilder);
		Common.AppendLine(stringBuilder);
		if (noOfDefensiveAgents > 0)
		{
			Common.AppendLine(stringBuilder, "DEFENDERS");
			Common.Append(stringBuilder, "No. of defenders ");
			Common.Append(stringBuilder, noOfDefensiveAgents.ToString(), tintAsValue: true);
			Common.Append(stringBuilder, ", \ncombined Security rating ");
			Common.AppendFormat(stringBuilder, "{0:N2}", true, totalAgentsRating);
			Common.AppendLine(stringBuilder);
			AppendAssets(assets, finalAgentsRating, stringBuilder);
			stringBuilder.Append("Subscore: +");
			Common.AppendLine(stringBuilder, Common.PercentageToString(finalAgentsRating, includePlusPrefix: false, useColoring: true));
			Common.AppendLine(stringBuilder);
		}
		Common.AppendLine(stringBuilder, "HAND WEAPONS");
		Common.Append(stringBuilder, "No. of usable hand weapons ");
		Common.Append(stringBuilder, noOfWeapons.ToString(), tintAsValue: true);
		Common.Append(stringBuilder, " (max ");
		Common.Append(stringBuilder, num.ToString(), tintAsValue: true);
		Common.Append(stringBuilder, "), \ncombined Security rating ");
		Common.AppendFormat(stringBuilder, "{0:N2}", true, totalHandWeaponsScore);
		Common.AppendLine(stringBuilder);
		AppendAssets(assets, finalHandWeaponsRating, stringBuilder);
		stringBuilder.Append("Subscore: +");
		Common.AppendLine(stringBuilder, Common.PercentageToString(finalHandWeaponsRating, includePlusPrefix: false, useColoring: true));
		ComposeDeathsBreakdown(noOfMembers, injuries, deaths, injuryContribution, deathContribution, stringBuilder, security);
		ratingsBreakdown = stringBuilder.ToString();
	}

	private static void AppendAssets(float assets, float finalAgentsRating, StringBuilder text)
	{
		Common.Append(text, "Divided by assets (");
		Common.AppendFormat(text, "{0:N2}", true, assets);
		Common.Append(text, "): ");
		Common.AppendFormat(text, "{0:N2}", true, finalAgentsRating);
		Common.AppendLine(text);
	}

	protected override float ScoreRating()
	{
		int members = GetMembers();
		float assets = GetAssets();
		Dictionary<EntityType, int> ammoRounds = GatherAmmoData();
		ScoreDefensiveAgents(assets, ammoRounds, out var noOfDefensiveAgents, out var averageRating, out var totalScore, out var finalAgentsRating);
		ScoreHandWeapons(assets, ammoRounds, out var noOfWeaponCarriers, out var noOfWeapons, out var averageRating2, out var totalHandWeaponsScore, out var finalHandWeaponsScore);
		ComputeInjuriesAndDeaths(assets, out var relevantInjuries, out var relevantDeaths, out var totalInjuryContribution, out var totalDeathsContribution, out var finalInjuryContribution, out var finalDeathsContribution);
		float f = finalHandWeaponsScore + finalAgentsRating - finalDeathsContribution - finalInjuryContribution;
		f = Common.Clamp(f, 0f, 1f);
		AddSharedRating(f);
		if (composeBreakdown)
		{
			ComposeRatingBreakdown(f, assets, members, relevantInjuries, relevantDeaths, totalInjuryContribution, totalDeathsContribution, noOfWeaponCarriers, noOfWeapons, totalHandWeaponsScore, averageRating2, finalHandWeaponsScore, noOfDefensiveAgents, totalScore, averageRating, finalAgentsRating);
		}
		return f;
	}

	public override Snapshotter.Version DoVersion(Snapshotter sn)
	{
		base.DoVersion(sn);
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		SharedRating = sn.DoList(SharedRating);
		return this;
	}
}
