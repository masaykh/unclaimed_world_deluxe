using System.Collections.Generic;
using System.Xml.Serialization;
using UWGame.SimSide.SimEffects;

namespace UWGame.SimSide.Items;

public class FoodType
{
	public FoodNutrientProfile FoodNutrientProfile;

	public bool IsMeal;

	public string[] FoodTags;

	public bool IsDrunk;

	public string[] Effects;

	[XmlIgnore]
	public List<EffectProfileType> EffectTypes;

	public void Initialize()
	{
	}

	public void PostDataCompleteInitialize()
	{
		if (Effects == null)
		{
			return;
		}
		EffectTypes = new List<EffectProfileType>();
		string[] effects = Effects;
		foreach (string key in effects)
		{
			EffectTypes.Add(GameData.Instance.AllEffectProfileTypes[key]);
		}
		foreach (EffectProfileType effectType in EffectTypes)
		{
			if (effectType.Affects(AffectsNumbers.AgentComfort))
			{
				FoodNutrientAmount[] foodNutrientTypes = FoodNutrientProfile.FoodNutrientTypes;
				for (int i = 0; i < foodNutrientTypes.Length; i++)
				{
					foodNutrientTypes[i].Nutrient.SatisfiesComfort = true;
				}
			}
		}
	}
}
