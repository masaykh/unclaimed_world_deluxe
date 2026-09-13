using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UWGame.SimSide.Entities.RepairTypes;
using UWGame.SimSide.Items;
using UWGame.SimSide.Processes;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Entities;

public class NonLivingType
{
	public string DegradeType;

	public DegradeType FinalDegradeType;

	public string DegradesTo;

	[XmlIgnore]
	public EntityType DegradesToType;

	[XmlIgnore]
	public bool CanBeAPart;

	public float IntegrityWeightInCondition = 0.3f;

	public string SalvageProcess;

	[XmlIgnore]
	public ProcessType SalvageProcessType;

	public float Repairability;

	public string Repair;

	[XmlIgnore]
	public RepairProfile RepairProfile;

	[XmlIgnore]
	public EntityRepairProfile EntityRepairProfile;

	[XmlIgnore]
	public Dictionary<EntityType, int> Parts;

	[XmlElement("Parts")]
	public SerializableDictionary<string, int> PartKeys;

	[XmlIgnore]
	public int PartID;

	public bool PartsAreWeatherProof { get; set; }

	public void Initialize()
	{
	}

	public void PreDataCompleteValidate(ref List<string> listOfErrors)
	{
		if (PartKeys != null)
		{
			foreach (KeyValuePair<string, int> partKey in PartKeys)
			{
				EntityType.ValidateEntityTypeKeyExists(ref listOfErrors, partKey.Key);
			}
		}
		if (DegradesTo != null)
		{
			EntityType.ValidateEntityTypeKeyExists(ref listOfErrors, DegradesTo);
		}
	}

	public void PostDataCompleteInitialize(EntityType parent)
	{
		if (DegradesTo != null)
		{
			DegradesToType = GameData.Instance.AllEntityTypes[DegradesTo];
		}
		if (DegradeType != null)
		{
			FinalDegradeType = GameData.Instance.AllDegradeTypes[DegradeType];
		}
		if (Repair != null)
		{
			RepairProfile = GameData.Instance.AllRepairProfiles[Repair];
			EntityRepairProfile = new EntityRepairProfile();
		}
		if (PartKeys == null)
		{
			return;
		}
		Parts = new Dictionary<EntityType, int>();
		foreach (KeyValuePair<string, int> partKey in PartKeys)
		{
			EntityType entityType = GameData.Instance.AllEntityTypes[partKey.Key];
			Parts.Add(entityType, partKey.Value);
			if (entityType.NonLivingType != null)
			{
				entityType.NonLivingType.CanBeAPart = true;
			}
		}
	}

	public List<StorageDuration> GetRepresentativeStorageConditions(EntityType parent)
	{
		if (FinalDegradeType != null)
		{
			List<StorageDuration> list = null;
			float num = 0f;
			foreach (StorageDuration storageDuration in FinalDegradeType.StorageDurations)
			{
				if (storageDuration.Duration > num)
				{
					num = storageDuration.Duration;
				}
				if (storageDuration.StorageDurationToDisplay.DisplayThis(parent))
				{
					Common.AddToList(ref list, storageDuration);
				}
			}
			List<StorageDuration> list2 = list.OrderByDescending((StorageDuration d) => d.SortOrder).ToList();
			while (list2.Count > 4)
			{
				list2.RemoveAt(list2.Count - 1);
			}
			return list2;
		}
		return null;
	}

	public ProcessType SelectRepairProcess(RepairAction action, EntityType part)
	{
		switch (action)
		{
		case RepairAction.Integrity:
			return EntityRepairProfile.Integrity;
		case RepairAction.Condition:
			return EntityRepairProfile.Condition;
		case RepairAction.PartsCondition:
			return EntityRepairProfile.PartsCondition[part];
		default:
			if (RepairType.IsReplaceAction(action))
			{
				return EntityRepairProfile.PartsReplacement[part];
			}
			return null;
		}
	}

	public void PostLoadContentInitialize()
	{
		if (!string.IsNullOrEmpty(SalvageProcess))
		{
			SalvageProcessType = GameData.Instance.AllProcessTypes[SalvageProcess];
			SalvageProcessType.SetIsSalvageProcess();
		}
	}
}
