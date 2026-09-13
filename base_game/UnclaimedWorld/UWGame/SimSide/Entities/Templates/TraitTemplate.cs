using System.Collections.Generic;
using System.Linq;

namespace UWGame.SimSide.Entities.Templates;

public class TraitTemplate : IGameData
{
	public string[] ExpertSkills;

	public string[] HighSkills;

	public string[] MediumSkills;

	public string[] LowSkills;

	public string[] ZeroSkills;

	public string KeyName { get; set; }

	public string Name { get; set; }

	public bool DeleteRecord { get; set; }

	private void SetSkills(string[] group, Entity entity, NormalDistribution distribution)
	{
		foreach (string skillKey in group)
		{
			entity.Intelligence.SetSkill(skillKey, (float)distribution.GetRandomValue(The.Sim.GameplayRandomGenerator, clampBetweenZeroAndOne: true));
		}
	}

	public void FillEntity(Entity entity)
	{
		SetSkills(ExpertSkills, entity, GameData.Instance.Constants.ExpertSkillDistribution);
		SetSkills(HighSkills, entity, GameData.Instance.Constants.HighSkillDistribution);
		SetSkills(MediumSkills, entity, GameData.Instance.Constants.MediumSkillDistribution);
		SetSkills(LowSkills, entity, GameData.Instance.Constants.LowSkillDistribution);
		SetSkills(ZeroSkills, entity, GameData.Instance.Constants.ZeroSkillDistribution);
	}

	private bool HasSkill(string[] group, SkillType skill)
	{
		return group?.Any((string s) => s == skill.KeyName) ?? false;
	}

	public void PreInitValidate(ref List<string> errors)
	{
	}

	public void Initialize()
	{
	}

	public void PostInitValidate(ref List<string> errors)
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
		foreach (KeyValuePair<string, SkillType> allSkillType in GameData.Instance.AllSkillTypes)
		{
			if (!HasSkill(ExpertSkills, allSkillType.Value) && !HasSkill(HighSkills, allSkillType.Value) && !HasSkill(MediumSkills, allSkillType.Value) && !HasSkill(LowSkills, allSkillType.Value) && !HasSkill(ZeroSkills, allSkillType.Value))
			{
				EntityType.CreateValidationError(ref listOfErrors, "Skill not represented: " + allSkillType.Key);
			}
		}
	}
}
