using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Locomotors;
using UWGame.SimSide.SimEffects;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Items;

[XmlRoot("Item")]
public class ItemType : IXmlSerializable
{
	public enum TaskType
	{
		LongerJourneys,
		UnspecifiedHunting,
		Scouting,
		Hauling,
		PatrolOrAttack,
		NightActivities,
		Examining
	}

	public enum AppropriateLevel
	{
		None,
		Minor,
		Normal,
		Best
	}

	public enum HauledItemValues
	{
		LowValue,
		Normal,
		MostValuable
	}

	public string Abbreviation;

	public string[] Tags;

	public string[] RequiredStorageTags;

	public string[] RequiredStorageTypes;

	[XmlIgnore]
	public HashSet<EntityType> RequiredStorageTypesFinal;

	public bool HasNoMaximumBulk;

	public float? MaximumBulk;

	public string[] EffectsWhenEquipped;

	[XmlIgnore]
	public List<EffectProfileType> FinalEffectsWhenEquipped;

	public bool UseGearAtAnyDistanceFromExpedition;

	public SerializableDictionary<TaskType, AppropriateLevel> TaskAppropriateLevels = new SerializableDictionary<TaskType, AppropriateLevel>();

	public FoodType FoodType;

	public WeaponType WeaponType;

	public LocomotorType LocomotorType;

	public CarcassType CarcassType;

	public FuelType FuelType;

	public AmmunitionType AmmunitionType;

	public string AttachedObjectRenderableType;

	public string AttachorTagToMountOn;

	public string AttachesToBodyPart;

	public AnimModifier[] AnimStatesWhenAttached;

	private bool canBeAPart;

	public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(ItemType))
	{
		TypeMappings = DataLoader.GetListOfTypeMappings(useEntityTypePlaceholders: true)
	};

	public HauledItemValues HauledItemValue = HauledItemValues.Normal;

	public string KeyName { get; set; }

	public string Name { get; set; }

	public bool CanBeAPart
	{
		get
		{
			return canBeAPart;
		}
		set
		{
			canBeAPart = true;
		}
	}

	public bool ShouldSerializeMaximumBulk()
	{
		return MaximumBulk.HasValue;
	}

	public float GetTaskAppropriateLevel(TaskType type)
	{
		if (TaskAppropriateLevels.TryGetValue(type, out var value))
		{
			switch (value)
			{
			case AppropriateLevel.None:
				return 0f;
			case AppropriateLevel.Minor:
				return 0.25f;
			case AppropriateLevel.Normal:
				return 0.5f;
			case AppropriateLevel.Best:
				return 1f;
			}
		}
		return 0f;
	}

	public float GetTaskAppropriateLevel(List<TaskType> taskTypes)
	{
		float num = 0f;
		float num2 = 0f;
		foreach (TaskType taskType in taskTypes)
		{
			num2 = GetTaskAppropriateLevel(taskType);
			if (num2 > num)
			{
				num = num2;
			}
		}
		return num;
	}

	public ItemType()
	{
	}

	public ItemType(string keyName)
	{
		KeyName = keyName;
	}

	public string GetAbbreviation()
	{
		if (!string.IsNullOrEmpty(Abbreviation))
		{
			return Abbreviation;
		}
		return Name.Substring(0, Common.Min(3, Name.Length));
	}

	public void Initialize()
	{
		if (WeaponType != null && !TaskAppropriateLevels.ContainsKey(TaskType.LongerJourneys))
		{
			TaskAppropriateLevels[TaskType.LongerJourneys] = AppropriateLevel.Normal;
		}
		if (FoodType != null)
		{
			FoodType.Initialize();
		}
	}

	public void PostDataCompleteInitialize()
	{
		if (FoodType != null)
		{
			FoodType.PostDataCompleteInitialize();
		}
		if (EffectsWhenEquipped != null)
		{
			FinalEffectsWhenEquipped = new List<EffectProfileType>();
			string[] effectsWhenEquipped = EffectsWhenEquipped;
			foreach (string key in effectsWhenEquipped)
			{
				FinalEffectsWhenEquipped.Add(GameData.Instance.AllEffectProfileTypes[key]);
			}
		}
	}

	public void PostLoadContentInitialize()
	{
		if (WeaponType != null)
		{
			WeaponType.PostLoadContentInitialize();
		}
		GameData.ResolveEntityTypeTags(ref RequiredStorageTypesFinal, RequiredStorageTags, RequiredStorageTypes, GameData.Instance.ContainersByTag);
	}

	public void Validate(ref List<string> errors)
	{
		if (HasNoMaximumBulk && MaximumBulk.HasValue)
		{
			EntityType.CreateValidationError(ref errors, "Bulk must not be defined for the item type when using unique instance bulk.");
		}
	}

	public override string ToString()
	{
		return Name;
	}

	public XmlSchema GetSchema()
	{
		return null;
	}

	public void ReadXml(XmlReader reader)
	{
		CustomXmlSerializer.ReadXmlDeserialize(this, reader, _proxyData);
	}

	public void WriteXml(XmlWriter writer)
	{
		CustomXmlSerializer.WriteXmlSerialize(this, writer, _proxyData);
	}

	public float GetHauledItemValueModifier()
	{
		return HauledItemValue switch
		{
			HauledItemValues.LowValue => 0.25f, 
			HauledItemValues.Normal => 0.5f, 
			HauledItemValues.MostValuable => 1f, 
			_ => throw new Exception("No priority set"), 
		};
	}
}
