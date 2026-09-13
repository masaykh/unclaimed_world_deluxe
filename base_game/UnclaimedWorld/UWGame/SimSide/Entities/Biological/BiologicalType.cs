using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Entities.Body;
using UWGame.SimSide.Entities.Locomotors.Stances;
using UWGame.SimSide.Processes;

namespace UWGame.SimSide.Entities.Biological;

public class BiologicalType : IXmlSerializable
{
	public string ModelBasicTextureName;

	public Vector3? PrimaryColor;

	public Vector3? SecondaryColor;

	public Vector3? TertiaryColor;

	public Vector3? QuaternaryColor;

	public string OrderKey;

	[XmlIgnore]
	public BioOrderType OrderType;

	public string SpeciesPlural;

	[XmlIgnore]
	public float? MinimumBulk;

	[XmlIgnore]
	public float MeanBulkOfAdultMember;

	[XmlIgnore]
	public float MaxBulkOfAdultMember;

	[XmlIgnore]
	public float MeanHitpointsOfAdultMember;

	[XmlIgnore]
	public CasteType AdultMemberCaste;

	[XmlIgnore]
	public AgeGroupType AdultMemberAgeGroup;

	public List<CasteType> Castes;

	public RaceType[] RaceTypes;

	public float ResilienceMean = 1f;

	public float ResilienceStandardDeviation;

	public bool IsNocturnal;

	public double TimeOfDayToGoToSleep;

	public bool IsTerritorial;

	public float? ActiveStealthRating;

	public float? PassiveStealthRating;

	public float? OxygenAndMuscleEnergyIncreaseRatePerDay;

	public bool HoldsFoodWhenEating;

	public ChanceToTakeStance[] EatingStances;

	[XmlIgnore]
	public List<ChanceToTakeStance> EatingStanceTypes;

	public float? StomachSizeFractionOfEntityBulk;

	public float? StomachContentsDecreaseRatePerDay;

	public float? TimeToConsumeFullMealInDays;

	public float? MaxRegainLimit;

	public float? FractionOfMaxHitpointsGainedPerDay;

	public bool IsVermin;

	public string[] ExtractionProcessTypes;

	public string[] ConsumeProcessTypes;

	public string[] FoodItemTagsThatCanBeConsumed;

	[XmlIgnore]
	public Dictionary<EntityType, HashSet<ProcessType>> ExtractionProcesses;

	[XmlIgnore]
	public Dictionary<EntityType, ProcessType> ConsumeProcesses;

	[XmlIgnore]
	public Dictionary<EntityType, HashSet<ProcessType>> ExtractionResultsInConsumable;

	public PalatableFood[] PalatableFood;

	public string Carcass;

	[XmlIgnore]
	public EntityType CarcassType;

	public BioPropertyType[] BioPropertyTypes;

	[XmlIgnore]
	public Dictionary<string, BioPropertyType> bioPropertyTypes;

	[XmlIgnore]
	public float WorstCaseEnergyFactor;

	public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(BiologicalType))
	{
		TypeMappings = DataLoader.GetListOfTypeMappings(useEntityTypePlaceholders: true)
	};

	public void Validate(ref List<string> errors)
	{
		foreach (CasteType caste in Castes)
		{
			caste.Validate(ref errors);
		}
		EntityType.ValidateRequiredValue(ref errors, "Oxygen recharge rate", OxygenAndMuscleEnergyIncreaseRatePerDay.HasValue);
		EntityType.ValidateRequiredValue(ref errors, "Stealth rating", ActiveStealthRating.HasValue);
		EntityType.ValidateRequiredValue(ref errors, "Stomach contents recharge rate", StomachContentsDecreaseRatePerDay.HasValue);
		EntityType.ValidateRequiredValue(ref errors, "Stomach size", StomachSizeFractionOfEntityBulk.HasValue);
		EntityType.ValidateRequiredValue(ref errors, "Time to consume full meal in days", TimeToConsumeFullMealInDays.HasValue);
		EntityType.ValidateRequiredValue(ref errors, "MaxRegainLimit", MaxRegainLimit.HasValue);
		EntityType.ValidateRequiredValue(ref errors, "FractionOfMaxHitpointsGainedPerDay", FractionOfMaxHitpointsGainedPerDay.HasValue);
		if (MinimumBulk.HasValue && MinimumBulk.Value < GameData.Instance.Constants.MinimumAgentBulk)
		{
			EntityType.CreateValidationError(ref errors, $"The minimum bulk has been calculated to a value {MinimumBulk.Value} that is less than the minimum allowed bulk for an agent {GameData.Instance.Constants.MinimumAgentBulk}.");
		}
	}

	public void PostDataCompleteInitialize()
	{
		foreach (CasteType caste in Castes)
		{
			caste.PostDataCompleteInitialize();
		}
	}

	public float GetMaxBulk()
	{
		return MaxBulkOfAdultMember;
	}

	public void Initialize(EntityType parent)
	{
		if (!string.IsNullOrEmpty(OrderKey))
		{
			OrderType = GameData.Instance.AllBioOrderTypes[OrderKey];
		}
		int num = 1;
		foreach (CasteType caste in Castes)
		{
			caste.Initialize(num);
			num++;
		}
		if (RaceTypes != null)
		{
			int num2 = 1;
			RaceType[] raceTypes = RaceTypes;
			for (int i = 0; i < raceTypes.Length; i++)
			{
				raceTypes[i].Initialize(num2);
				num2++;
			}
		}
		if (BioPropertyTypes != null)
		{
			bioPropertyTypes = new Dictionary<string, BioPropertyType>();
			BioPropertyType[] array = BioPropertyTypes;
			foreach (BioPropertyType bioPropertyType in array)
			{
				bioPropertyTypes.Add(bioPropertyType.KeyName, bioPropertyType);
			}
		}
		if (EatingStances != null)
		{
			EatingStanceTypes = new List<ChanceToTakeStance>();
			ChanceToTakeStance[] eatingStances = EatingStances;
			foreach (ChanceToTakeStance chanceToTakeStance in eatingStances)
			{
				chanceToTakeStance.Initialize();
				EatingStanceTypes.Add(chanceToTakeStance);
			}
		}
		GetDefaultAdultMember();
		GetMinimumBulk(out MinimumBulk);
		GetAdultMeanBulk(parent);
	}

	public void PostLoadContentInitialize(EntityType parent)
	{
		if (Carcass != null)
		{
			CarcassType = GameData.Instance.AllEntityTypes[Carcass];
		}
		InitConsumeProcesses(parent);
		InitExtractionProcesses(parent);
		InitExtractionResultsInConsumables(parent);
	}

	private void InitExtractionProcesses(EntityType parent)
	{
		ExtractionProcesses = new Dictionary<EntityType, HashSet<ProcessType>>();
		if (ExtractionProcessTypes != null)
		{
			string[] extractionProcessTypes = ExtractionProcessTypes;
			foreach (string key in extractionProcessTypes)
			{
				ProcessType processType = GameData.Instance.AllProcessTypes[key];
				processType.IsInnateExtractionProcess = true;
				EntityType key2 = processType.InputsByType.First().Key;
				Common.AddToMultiList(ExtractionProcesses, key2, processType);
			}
		}
	}

	private void InitExtractionResultsInConsumables(EntityType parent)
	{
		ComputeExtractionResultsInConsumables(ExtractionProcesses, ConsumeProcesses, ref ExtractionResultsInConsumable);
	}

	public static void ComputeExtractionResultsInConsumables(Dictionary<EntityType, HashSet<ProcessType>> extractionProcesses, Dictionary<EntityType, ProcessType> consumeProcesses, ref Dictionary<EntityType, HashSet<ProcessType>> extractionResultsInConsumable)
	{
		if (extractionResultsInConsumable == null)
		{
			extractionResultsInConsumable = new Dictionary<EntityType, HashSet<ProcessType>>();
		}
		extractionResultsInConsumable.Clear();
		foreach (KeyValuePair<EntityType, HashSet<ProcessType>> extractionProcess in extractionProcesses)
		{
			foreach (ProcessType item in extractionProcess.Value)
			{
				Output[] outputs = item.Outputs;
				foreach (Output output in outputs)
				{
					if (consumeProcesses.ContainsKey(output.FinalEntityTypeToCreate))
					{
						Common.AddToMultiList(extractionResultsInConsumable, extractionProcess.Key, item);
					}
				}
			}
		}
	}

	private void InitConsumeProcesses(EntityType parent)
	{
		ConsumeProcesses = new Dictionary<EntityType, ProcessType>();
		List<EntityType> list = new List<EntityType>();
		if (FoodItemTagsThatCanBeConsumed != null)
		{
			string[] foodItemTagsThatCanBeConsumed = FoodItemTagsThatCanBeConsumed;
			foreach (string key in foodItemTagsThatCanBeConsumed)
			{
				list.AddRange(GameData.Instance.FoodByTag[key]);
			}
		}
		list = list.Distinct().ToList();
		foreach (EntityType item in list)
		{
			InitConsumeProcess(parent, item);
		}
	}

	private void InitConsumeProcess(EntityType parent, EntityType item)
	{
		ProcessType processType = new ProcessType();
		processType.KeyName = parent.KeyName + "_defaultConsume_" + item.KeyName;
		processType.WorkOrTimeNeeded = new WorkOrTime
		{
			DaysNeeded = TimeToConsumeFullMealInDays
		};
		processType.Inputs = new Input[1]
		{
			new Input
			{
				Entity = item.KeyName,
				IsConsumed = true,
				Amount = new InputAmount
				{
					NoOfItems = 1
				}
			}
		};
		processType.Outputs = new Output[1]
		{
			new Output
			{
				EntityTypeToCreate = item.KeyName,
				Amount = new OutputAmount
				{
					Bulk = new Bulk
					{
						FractionOfInputBulk = 1f
					}
				}
			}
		};
		processType.AgentActionState = AnimAction.Eating;
		processType.IsConsumeProcess = true;
		processType.UseWorkerEnergyAsProductionFactor = false;
		GameData.Instance.AllProcessTypes.Add(processType.KeyName, processType);
		GameData.InitializeComputerGeneratedData(processType);
		if (parent.HasStance() && EatingStanceTypes != null)
		{
			processType.StanceTypes = new Dictionary<StancesType, List<ChanceToTakeStance>> { 
			{
				parent.LocomotorType.StancesType,
				EatingStanceTypes
			} };
		}
		processType.PostLoadContentInitialize();
		ConsumeProcesses.Add(item, processType);
	}

	public bool GetBioPropertyType(string propertyKey, out BioPropertyType bioPropertyType)
	{
		bioPropertyType = null;
		if (bioPropertyTypes != null)
		{
			return bioPropertyTypes.TryGetValue(propertyKey, out bioPropertyType);
		}
		return false;
	}

	private void GetMinimumBulk(out float? minimumBulk)
	{
		if (Castes.Count > 0)
		{
			float num = BiologicalEntity.GetBulkFromWeight(Common.GetMinimumValue(Castes[0].WeightMean, Castes[0].WeightStandardDeviation));
			for (int i = 1; i < Castes.Count; i++)
			{
				float bulkFromWeight = BiologicalEntity.GetBulkFromWeight(Common.GetMinimumValue(Castes[i].WeightMean, Castes[i].WeightStandardDeviation));
				if (bulkFromWeight < num)
				{
					num = bulkFromWeight;
				}
			}
			minimumBulk = num;
		}
		else
		{
			minimumBulk = null;
		}
	}

	public NeedType[] GetAdultNeedsAndWeight(out float weight)
	{
		weight = -1f;
		CasteType casteType = Castes.FirstOrDefault((CasteType c) => c.Reproduction == Reproduction.Male);
		if (casteType == null)
		{
			casteType = Castes[0];
		}
		if (casteType != null)
		{
			AgeGroupType ageGroupType = casteType.AgeGroupTypes.FirstOrDefault((AgeGroupType a) => a.AIAgeGroup == AIAgeGroup.Adult);
			if (ageGroupType != null)
			{
				weight = casteType.WeightMean;
				return ageGroupType.NeedTypes;
			}
		}
		return null;
	}

	public bool IsEatable(EntityType food)
	{
		if (food.ItemType == null || food.ItemType.FoodType == null || !ConsumeProcesses.ContainsKey(food))
		{
			return ExtractionResultsInConsumable.ContainsKey(food);
		}
		return true;
	}

	private void GetDefaultAdultMember()
	{
		AdultMemberCaste = Castes.FirstOrDefault((CasteType c) => c.Reproduction == Reproduction.Male);
		if (AdultMemberCaste == null)
		{
			AdultMemberCaste = Castes[0];
		}
		if (AdultMemberCaste != null)
		{
			AdultMemberAgeGroup = AdultMemberCaste.AgeGroupTypes.FirstOrDefault((AgeGroupType a) => a.AIAgeGroup == AIAgeGroup.Adult);
		}
		else
		{
			AdultMemberAgeGroup = AdultMemberCaste.AgeGroupTypes[0];
		}
	}

	private void GetAdultMeanBulk(EntityType parent)
	{
		MeanBulkOfAdultMember = BiologicalEntity.GetBulkFromWeight(AdultMemberCaste.WeightMean);
		MaxBulkOfAdultMember = BiologicalEntity.GetBulkFromWeight(AdultMemberCaste.WeightMean + 3.5f * AdultMemberCaste.WeightStandardDeviation);
		float value = BiologicalEntity.GetBioProperty("ResilienceMean", parent, AdultMemberCaste, null, AdultMemberAgeGroup.Edge, OrderType, ResilienceMean).NumberValue.Value;
		MeanHitpointsOfAdultMember = UWGame.SimSide.Entities.Body.Body.ComputeHitpoints(parent, MeanBulkOfAdultMember, value);
	}

	public string GetModelBasicTexture(CasteType caste, RaceType race, float age)
	{
		int stairstep;
		string modelBasicTextureName = Common.GetStairStepIndex(age, caste.AgeGroupTypes, out stairstep).ModelBasicTextureName;
		if (!string.IsNullOrEmpty(modelBasicTextureName))
		{
			return modelBasicTextureName;
		}
		if (race != null && (!string.IsNullOrEmpty(race.ModelBasicTextureName) || race.ModelBasicTextureNames != null))
		{
			if (race.ModelBasicTextureNames != null)
			{
				List<string> list = race.ModelBasicTextureNames.ToList();
				if (!string.IsNullOrEmpty(race.ModelBasicTextureName))
				{
					list.Add(race.ModelBasicTextureName);
				}
				return Common.GetRandomListMember(list, The.Sim.GameplayRandomGenerator);
			}
			return race.ModelBasicTextureName;
		}
		string modelBasicTextureName2 = caste.ModelBasicTextureName;
		if (!string.IsNullOrEmpty(modelBasicTextureName2))
		{
			return modelBasicTextureName2;
		}
		return ModelBasicTextureName;
	}

	public string GetModelName(CasteType caste, RaceType race, float age)
	{
		int stairstep;
		string modelName = Common.GetStairStepIndex(age, caste.AgeGroupTypes, out stairstep).ModelName;
		if (!string.IsNullOrEmpty(modelName))
		{
			return modelName;
		}
		if (race != null && !string.IsNullOrEmpty(race.ModelName))
		{
			return race.ModelName;
		}
		string modelName2 = caste.ModelName;
		if (!string.IsNullOrEmpty(modelName2))
		{
			return modelName2;
		}
		return null;
	}

	public float GetModelScale(CasteType caste, RaceType race, float age, float defaultScale)
	{
		int stairstep;
		float? modelScaleFraction = Common.GetStairStepIndex(age, caste.AgeGroupTypes, out stairstep).ModelScaleFraction;
		float num = ((!modelScaleFraction.HasValue) ? 1f : modelScaleFraction.Value);
		if (race != null && race.ModelScale.HasValue)
		{
			return race.ModelScale.Value * num;
		}
		float? modelScale = caste.ModelScale;
		if (modelScale.HasValue)
		{
			return modelScale.Value * num;
		}
		return defaultScale * num;
	}

	public Vector3 GetPrimaryColor(CasteType caste, RaceType race, float age)
	{
		int stairstep;
		Vector3? primaryColor = Common.GetStairStepIndex(age, caste.AgeGroupTypes, out stairstep).PrimaryColor;
		if (primaryColor.HasValue)
		{
			return primaryColor.Value;
		}
		if (race != null && race.PrimaryColor.HasValue)
		{
			return race.PrimaryColor.Value;
		}
		Vector3? primaryColor2 = caste.PrimaryColor;
		if (primaryColor2.HasValue)
		{
			return primaryColor2.Value;
		}
		if (PrimaryColor.HasValue)
		{
			return PrimaryColor.Value;
		}
		return Vector3.One;
	}

	public Vector3 GetSecondaryColor(CasteType caste, RaceType race, float age)
	{
		int stairstep;
		Vector3? secondaryColor = Common.GetStairStepIndex(age, caste.AgeGroupTypes, out stairstep).SecondaryColor;
		if (secondaryColor.HasValue)
		{
			return secondaryColor.Value;
		}
		if (race != null && race.SecondaryColorProbabilityEdges != null && race.SecondaryColorProbabilityEdges.Count > 0)
		{
			return Common.GetStairStepIndex((float)The.Sim.GameplayRandomGenerator.NextDouble("BiologicalType"), race.SecondaryColorProbabilityEdges, out stairstep).Color;
		}
		Vector3? secondaryColor2 = caste.SecondaryColor;
		if (secondaryColor2.HasValue)
		{
			return secondaryColor2.Value;
		}
		if (SecondaryColor.HasValue)
		{
			return SecondaryColor.Value;
		}
		return Vector3.One;
	}

	public Vector3 GetTertiaryColor(CasteType caste, RaceType race, float age)
	{
		int stairstep;
		Vector3? tertiaryColor = Common.GetStairStepIndex(age, caste.AgeGroupTypes, out stairstep).TertiaryColor;
		if (tertiaryColor.HasValue)
		{
			return tertiaryColor.Value;
		}
		if (race != null && race.TertiaryColor.HasValue)
		{
			return race.TertiaryColor.Value;
		}
		Vector3? tertiaryColor2 = caste.TertiaryColor;
		if (tertiaryColor2.HasValue)
		{
			return tertiaryColor2.Value;
		}
		if (TertiaryColor.HasValue)
		{
			return TertiaryColor.Value;
		}
		return Vector3.One;
	}

	public Vector3 GetQuaternaryColor(CasteType caste, RaceType race, float age)
	{
		int stairstep;
		Vector3? quaternaryColor = Common.GetStairStepIndex(age, caste.AgeGroupTypes, out stairstep).QuaternaryColor;
		if (quaternaryColor.HasValue)
		{
			return quaternaryColor.Value;
		}
		if (race != null && race.QuaternaryColor.HasValue)
		{
			return race.QuaternaryColor.Value;
		}
		Vector3? quaternaryColor2 = caste.QuaternaryColor;
		if (quaternaryColor2.HasValue)
		{
			return quaternaryColor2.Value;
		}
		if (QuaternaryColor.HasValue)
		{
			return QuaternaryColor.Value;
		}
		return Vector3.One;
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
}
