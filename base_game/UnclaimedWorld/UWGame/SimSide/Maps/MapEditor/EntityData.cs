using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.Entities;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Maps.MapEditor;

public class EntityData : IGameData
{
	public string EntityKey;

	public Vector3? Location;

	public float? Rotation;

	public bool FlipHorizontally;

	public float? Bulk;

	public Resource[] Resources;

	public Tree Tree;

	public BiologicalEntity BioEntity;

	public Rock Rock;

	public Person Person;

	public Threat Threat;

	public SerializableDictionary<string, NeedData> NeedLevels;

	public AllegianceAndExpedition OwnedBy;

	public AllegianceAndExpedition MemberOf;

	public SerializableDictionary<string, PropertyResult> Properties;

	public string[] EffectProfiles;

	public string KeyName { get; set; }

	public string Name { get; set; }

	public bool DeleteRecord { get; set; }

	public bool ShouldSerializeRotation()
	{
		return Rotation.HasValue;
	}

	public bool ShouldSerializeResources()
	{
		return Resources != null;
	}

	public bool ShouldSerializeBulk()
	{
		return Bulk.HasValue;
	}

	public void SetRandomStats()
	{
	}

	public void PostInitValidate(ref List<string> listOfErrors)
	{
	}

	public void PostLoadContentValidate(List<string> listOfErrors)
	{
		if (!GameData.Instance.AllEntityTypes.TryGetValue(EntityKey, out var value))
		{
			return;
		}
		if (value != null && value.BiologicalType != null && Bulk < value.BiologicalType.MinimumBulk)
		{
			EntityType.CreateValidationError(ref listOfErrors, $"Bulk [{Bulk}] is lower than the minimum [{value.BiologicalType.MinimumBulk}] for {GetName()}.");
		}
		if (BioEntity == null || BioEntity.TraitTemplates != null || Person == null)
		{
			return;
		}
		foreach (KeyValuePair<string, SkillType> allSkillType in GameData.Instance.AllSkillTypes)
		{
			if (BioEntity.Skills == null || !BioEntity.Skills.ContainsKey(allSkillType.Key))
			{
				EntityType.CreateValidationError(ref listOfErrors, $"Skill {allSkillType.Key} not defined for Person: {GetName()}");
			}
		}
	}

	public void PostDataCompleteValidate(ref List<string> listOfErrors)
	{
		if (EffectProfiles != null)
		{
			string[] effectProfiles = EffectProfiles;
			foreach (string key in effectProfiles)
			{
				EntityType.ValidateGameDataTypeExists(ref listOfErrors, key, GameData.Instance.AllEffectProfileTypes, out var _);
			}
		}
	}

	private string GetName()
	{
		if (Person != null)
		{
			return (Person.FirstName + " " + Person.LastName).Trim();
		}
		return Name ?? EntityKey;
	}

	public void Initialize()
	{
	}

	public void PostDataCompleteInitialize()
	{
		if (Resources != null)
		{
			Resource[] resources = Resources;
			for (int i = 0; i < resources.Length; i++)
			{
				resources[i].PostDataCompleteInitialize();
			}
		}
	}

	public void PreDataCompleteValidate(ref List<string> listOfErrors)
	{
	}

	public void PreInitValidate(ref List<string> errors)
	{
	}
}
