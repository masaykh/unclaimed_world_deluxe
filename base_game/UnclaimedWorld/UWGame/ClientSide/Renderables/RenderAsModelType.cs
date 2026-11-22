using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Content;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using Xclna.Xna.Animation;

namespace UWGame.ClientSide.Renderables;

public class RenderAsModelType
{
	public float ModelScale = 1f;

	public string ModelBasicTextureName;

	public XmlDictionary<string, GaitAnimationBracket[]> GaitAnimations;

	public AnimConditionInfo[] AnimConditions;

	// Definitions selected independently of the main animation, one match per parallel condition set.
	public AnimConditionInfo[] ParallelAnimations;

	[XmlIgnore]
	public Dictionary<AnimAction, List<AnimConditionInfo>> AnimConditionsByAction;

	public string AssetName { get; set; }

	public AnimConditionInfo DefaultInfo { get; set; }

	public AnimConditionInfo[] DefaultStances { get; set; }

	public void LoadContent(ContentManager content)
	{
	}

	public void Initialize()
	{
		if (ParallelAnimations != null)
		{
			foreach (AnimConditionInfo animation in ParallelAnimations)
			{
				animation.Initialize();
			}
		}
		if (AnimConditions != null)
		{
			AnimConditionsByAction = new Dictionary<AnimAction, List<AnimConditionInfo>>();
			AnimConditionInfo[] animConditions = AnimConditions;
			foreach (AnimConditionInfo animConditionInfo in animConditions)
			{
				animConditionInfo.Initialize();
				if (animConditionInfo.ConditionSet.Action.HasValue)
				{
					if (!AnimConditionsByAction.TryGetValue(animConditionInfo.ConditionSet.Action.Value, out var value))
					{
						value = new List<AnimConditionInfo>();
						AnimConditionsByAction.Add(animConditionInfo.ConditionSet.Action.Value, value);
					}
					value.Add(animConditionInfo);
				}
			}
		}
		if (DefaultInfo == null)
		{
			AnimConditions condition = new AnimConditions
			{
				Action = AnimAction.Idle,
				Modifiers = new BitMask64()
			};
			AnimConditionInfo bestMatch = new AnimConditionInfo();
			FindBestAnimInfo(condition, out bestMatch);
			DefaultInfo = bestMatch;
		}
		if (GaitAnimations == null)
		{
			return;
		}
		foreach (KeyValuePair<string, GaitAnimationBracket[]> gaitAnimation in GaitAnimations)
		{
			gaitAnimation.Value.Initialize();
		}
	}

	public void PostLoadContentValidate(ref List<string> listOfErrors, EntityType parent)
	{
		HashSet<string> hashSet = new HashSet<string>();
		if (AssetName != null)
		{
			hashSet.Add(AssetName);
		}
		if (parent != null && parent.BiologicalType != null)
		{
			foreach (CasteType caste in parent.BiologicalType.Castes)
			{
				foreach (AgeGroupType ageGroupType in caste.AgeGroupTypes)
				{
					if (parent.BiologicalType.RaceTypes != null)
					{
						RaceType[] raceTypes = parent.BiologicalType.RaceTypes;
						foreach (RaceType race in raceTypes)
						{
							string modelName = parent.BiologicalType.GetModelName(caste, race, ageGroupType.Edge);
							if (modelName != null)
							{
								hashSet.Add(modelName);
							}
						}
					}
					else
					{
						string modelName2 = parent.BiologicalType.GetModelName(caste, null, ageGroupType.Edge);
						if (modelName2 != null)
						{
							hashSet.Add(modelName2);
						}
					}
				}
			}
		}
		if (AnimConditions != null)
		{
			ValidateConditionSet(AnimConditions, "AnimConditions", ref listOfErrors);
		}
		if (DefaultStances != null)
		{
			ValidateConditionSet(DefaultStances, "DefaultStances", ref listOfErrors);
		}
		foreach (string item in hashSet)
		{
			AnimationInfoCollection animations = AnimationInfoCollection.FromModel(GameData.Instance.AllModels[item].Model);
			if (ParallelAnimations != null)
			{
				foreach (AnimConditionInfo animation in ParallelAnimations)
				{
					animation.PostLoadContentValidate(listOfErrors, item, animations);
				}
			}
			if (GaitAnimations != null)
			{
				foreach (KeyValuePair<string, GaitAnimationBracket[]> gaitAnimation in GaitAnimations)
				{
					GaitAnimationBracket[] value = gaitAnimation.Value;
					foreach (GaitAnimationBracket gaitAnimationBracket in value)
					{
						listOfErrors = ValidateThatAnimExists(listOfErrors, gaitAnimationBracket.AnimationKey, item, animations);
					}
				}
			}
			if (DefaultInfo != null)
			{
				DefaultInfo.PostLoadContentValidate(listOfErrors, item, animations);
			}
			if (DefaultStances != null)
			{
				AnimConditionInfo[] defaultStances = DefaultStances;
				for (int i = 0; i < defaultStances.Length; i++)
				{
					defaultStances[i].PostLoadContentValidate(listOfErrors, item, animations);
				}
			}
			if (AnimConditions != null)
			{
				AnimConditionInfo[] defaultStances = AnimConditions;
				for (int i = 0; i < defaultStances.Length; i++)
				{
					defaultStances[i].PostLoadContentValidate(listOfErrors, item, animations);
				}
			}
		}
	}

	private static void ValidateConditionSet(AnimConditionInfo[] conditions, string fieldName, ref List<string> listOfErrors)
	{
		int num = conditions.Distinct().Count();
		if (conditions.Length != num)
		{
			EntityType.CreateValidationError(ref listOfErrors, "Duplicates found in " + fieldName + ".");
		}
	}

	public static List<string> ValidateThatAnimExists(List<string> listOfErrors, string key, string modelName, AnimationInfoCollection animations)
	{
		if (!animations.ContainsKey(key))
		{
			EntityType.CreateValidationError(ref listOfErrors, key + " animation not found on model: " + modelName);
		}
		return listOfErrors;
	}

	// Unlike the main animation lookup, parallel matching also accepts actionless condition sets.
	// Actions must match exactly; required modifiers are a subset, forbidden modifiers must be absent,
	// and the first most-specific definition wins.
	public AnimConditionInfo FindBestParallelAnimInfo(AnimConditions condition)
	{
		AnimConditionInfo best = null;
		uint bestSpecificity = 0;
		if (condition == null || ParallelAnimations == null)
			return null;
		ulong modifiers = condition.Modifiers?.Bits ?? 0;
		foreach (AnimConditionInfo candidate in ParallelAnimations)
		{
			if (candidate?.ConditionSet == null || candidate.ConditionSet.Action != condition.Action)
				continue;
			ulong required = candidate.ConditionSet.Modifiers?.Bits ?? 0;
			if ((modifiers & required) != required || (modifiers & (candidate.Forbiddens?.Bits ?? 0)) != 0)
				continue;
			uint specificity = candidate.ConditionSet.Modifiers?.CountBits() ?? 0;
			if (best == null || specificity > bestSpecificity)
			{
				best = candidate;
				bestSpecificity = specificity;
			}
		}
		return best;
	}

	public void FindBestAnimInfo(AnimConditions condition, out AnimConditionInfo bestMatch)
	{
		bestMatch = DefaultInfo;
		if (AnimConditions == null)
		{
			return;
		}
		if (!condition.Action.HasValue)
		{
			if (DefaultStances != null)
			{
				bestMatch = SelectDefaultStance(condition) ?? DefaultInfo;
			}
			return;
		}
		int num = 0;
		if (!AnimConditionsByAction.TryGetValue(condition.Action.Value, out var value))
		{
			return;
		}
		foreach (AnimConditionInfo item in value)
		{
			if (item.ConditionSet == null)
			{
				continue;
			}
			if (IsSame(condition, item))
			{
				bestMatch = item;
				break;
			}
			bool forbidden;
			int num2 = ScoreModifiers(condition, item, out forbidden);
			if (!forbidden)
			{
				int num3 = num2 + 3;
				if (num3 > num)
				{
					num = num3;
					bestMatch = item;
				}
			}
		}
	}

	private bool IsSame(AnimConditions condition, AnimConditionInfo matchCandidate)
	{
		AnimAction? action = condition.Action;
		AnimAction? action2 = matchCandidate.GetAction();
		if (action.GetValueOrDefault() == action2.GetValueOrDefault() && action.HasValue == action2.HasValue && (((condition.Modifiers == null || condition.Modifiers.Bits == 0L) && matchCandidate.GetModifiers() == null) || (condition.Modifiers.Bits != 0L && matchCandidate.GetModifiers() != null && condition.Modifiers.Equals(matchCandidate.GetModifiers()))))
		{
			return true;
		}
		return false;
	}

	private AnimConditionInfo SelectDefaultStance(AnimConditions condition)
	{
		int num = 0;
		AnimConditionInfo result = null;
		DefaultStances.FirstOrDefault((AnimConditionInfo s) => IsSame(condition, s));
		AnimConditionInfo[] defaultStances = DefaultStances;
		foreach (AnimConditionInfo animConditionInfo in defaultStances)
		{
			if (IsSame(condition, animConditionInfo))
			{
				return animConditionInfo;
			}
			bool forbidden;
			int num3 = ScoreModifiers(condition, animConditionInfo, out forbidden);
			if (!forbidden && num3 > num)
			{
				num = num3;
				result = animConditionInfo;
			}
		}
		return result;
	}

	private int ScoreModifiers(AnimConditions condition, AnimConditionInfo matchCandidate, out bool forbidden)
	{
		forbidden = false;
		int num = 0;
		int num2 = 0;
		BitMask64 modifiers = matchCandidate.GetModifiers();
		if (condition.Modifiers.Bits != 0L)
		{
			if (matchCandidate.Forbiddens != null && (int)condition.Modifiers.CountIntersection(matchCandidate.Forbiddens) > 0)
			{
				forbidden = true;
				return 0;
			}
			if (modifiers != null)
			{
				num = (int)condition.Modifiers.CountIntersection(modifiers);
				num2 = (int)condition.Modifiers.CountInverseIntersection(modifiers);
			}
			else
			{
				num2 = (int)condition.Modifiers.CountBits();
			}
		}
		else if (modifiers != null)
		{
			num2 = (int)modifiers.CountBits();
		}
		return num - num2;
	}
}
