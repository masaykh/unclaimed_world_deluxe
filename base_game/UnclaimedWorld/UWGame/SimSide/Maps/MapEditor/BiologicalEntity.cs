using System;
using System.Collections.Generic;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Maps.MapEditor;

public class BiologicalEntity
{
	public NormalDistribution AgeInYears;

	public AIAgeGroup? AgeGroup;

	public string CasteKey;

	public string RaceKey;

	public string ModelTextureName;

	public SerializableDictionary<string, float> Skills;

	public NormalDistribution StomachContent;

	public StringChance[] TraitTemplates;

	public StringChance[] CultureTemplates;

	public void FillEntity(Entity entity, float? age = null, string casteKey = null)
	{
		if (TraitTemplates != null)
		{
			int stairstep;
			StringChance stairStepIndex = Common.GetStairStepIndex(TraitTemplates, out stairstep, The.Sim.GameplayRandomGenerator);
			GameData.Instance.AllTraitTemplates[stairStepIndex.String].FillEntity(entity);
		}
		else if (Skills != null)
		{
			foreach (KeyValuePair<string, float> skill in Skills)
			{
				entity.Intelligence.SetSkill(skill.Key, skill.Value);
			}
		}
		entity.Find<UWGame.SimSide.Entities.Biological.BiologicalEntity>(out var c);
		if (CultureTemplates != null)
		{
			int stairstep2;
			StringChance stairStepIndex2 = Common.GetStairStepIndex(CultureTemplates, out stairstep2, The.Sim.GameplayRandomGenerator);
			GameData.Instance.AllCultureTemplates[stairStepIndex2.String].FillEntity(entity);
		}
		else
		{
			SetCaste(CasteKey ?? casteKey, c);
			if (!string.IsNullOrEmpty(RaceKey))
			{
				_ = RaceKey == "ManOchreClothesBlackHairTexture";
				c.SetRaceOnNewEntity(RaceKey);
			}
			float? num = null;
			if (AgeInYears != null)
			{
				num = (float)AgeInYears.GetRandomValue(The.Sim.GameplayRandomGenerator);
				c.SetAgePreInit(num);
			}
			else if (AgeGroup.HasValue)
			{
				c.SetAgePreInit(null, AgeGroup.Value);
			}
			else if (age.HasValue)
			{
				c.SetAgePreInit(age);
			}
			else
			{
				c.SetAgePreInit();
			}
		}
		if (!string.IsNullOrEmpty(ModelTextureName))
		{
			c.ModelTextureName = ModelTextureName;
		}
	}

	public static void SetCaste(string casteKey, UWGame.SimSide.Entities.Biological.BiologicalEntity bioComponent)
	{
		if (!string.IsNullOrEmpty(casteKey))
		{
			bioComponent.SetCasteOnNewEntity(casteKey);
			if (bioComponent.CasteType == null)
			{
				throw new Exception("Unknown caste in saved map entity: " + casteKey + " (Set Caste key to null to pick a random caste)");
			}
		}
		else
		{
			bioComponent.SetRandomCaste();
		}
	}
}
