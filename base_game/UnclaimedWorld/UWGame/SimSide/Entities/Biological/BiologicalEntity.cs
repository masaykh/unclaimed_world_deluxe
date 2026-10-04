using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.Client.Interface;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Processes;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities.Biological;

public class BiologicalEntity : Component
{
	public Entity Mate;

	public Entity BiologicalMother;

	public Entity BiologicalFather;

	public List<Entity> BiologicalChildren = new List<Entity>();

	/// <summary>
	/// is transferred from EntityData for use as fixed uniform...
	/// will override race, age and caste textures, if specified
	/// </summary>
	public string ModelTextureName;

	/// <summary>
	/// Never null.
	/// </summary>
	public CasteType CasteType;

	private string casteKey;
	// for snapshot

	/// <summary>
	/// can be null!
	/// </summary>
	public RaceType RaceType;

	private string raceKey;
	// for snapshot

	public AgeGroup AgeGroup;

	/// <summary>
	/// physical attractiveness
	/// </summary>
	public float Appearance;

	/// <summary>
	/// physical endurance
	/// </summary>
	public float Endurance;

	/// <summary>
	/// hitpoints modifier. gets multiplied with Bulk to determine max hitpoints.
	/// </summary>
	public float Resilience = 1f;

	/// <summary>
	/// the stealth or detection factor when the creature is trying to be stealthy
	/// </summary>
	public float ActiveStealthRating;

	/// <summary>
	/// the stealth or detection factor when the creature is not trying to be stealthy
	/// </summary>
	public float PassiveStealthRating;

	/// <summary>
	/// the rate as percentage per day we replenish the oxygen/muscle energy
	/// </summary>
	public float OxygenAndMuscleEnergyIncreaseRatePerDay;

	/// <summary>
	/// how much room does the stomach have
	/// humans: 1/70
	/// snakes: 2/3
	/// </summary>
	public float StomachSizeFractionOfEntityBulk;

	public float TimeToConsumeFullMealInDays;

	public float FractionOfMaxHitpointsGainedPerDay;

	public float MaxRegainLimit;

	/// <summary>
	/// will trigger attacks by entities that hunt vermin...
	/// </summary>
	public bool IsVermin;

	/// <summary>
	/// what can be consumed by the entity? - depends on its caste, age etc.
	/// Any item that can be consumed, can also be converted to a smaller bulk if needed to fit in the stomach
	/// </summary>
	public Dictionary<EntityType, ProcessType> ConsumeProcesses;

	/// <summary>
	/// extraction processes for the individual - depends on its caste, age etc.
	/// keyed by input entity: a carcass will have separate processes for extracting the organs, meat, etc...
	/// 
	/// Note: This collection of processes may not result in a consumable item for the entity!
	/// For instance, a worker may extract food with these processes that it would then give to the infants, but could not consume on its own.
	/// </summary>
	public Dictionary<EntityType, HashSet<ProcessType>> FoodExtractionProcesses;

	/// <summary>
	/// indicates if we can consume the product that comes from extracting from the key type
	/// 
	/// compute this by running extraction processes on the chain of products, and testing each if it is consumable
	/// </summary>
	public Dictionary<EntityType, HashSet<ProcessType>> ExtractionResultsInConsumable;

	public float Height;

	public float Weight;

	public float AdultTargetHeight;

	public float AdultTargetWeight;

	public Vector3? DwellingSpot;

	/// <summary>
	/// non-physical needs only?
	/// </summary>
	public Needs Needs;

	/// <summary>
	/// TODO: blood left in the body
	/// </summary>
	public float Blood;

	private float oxygenAndMuscleEnergy = 1f;

	private float energyLevel;

	private bool energyLevelIsDirty = true;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	/// <summary>
	/// 0 - 1
	/// oxygen + sugar? Represents oxygen and energy for muscles, as well as acid buildup. 
	/// Will fairly quickly grow back to 1.       
	/// </summary>
	public float OxygenAndMuscleEnergy
	{
		get
		{
			return oxygenAndMuscleEnergy;
		}
		set
		{
			if (value != oxygenAndMuscleEnergy)
			{
				oxygenAndMuscleEnergy = value;
				if (oxygenAndMuscleEnergy < GameData.Instance.Constants.OxygenEnergyRequiredToStartRunning)
				{
					Parent.Renderable.SetAnimationStateFlag(AnimModifier.Fatigued);
				}
				else if (oxygenAndMuscleEnergy > 1.1f * GameData.Instance.Constants.OxygenEnergyRequiredToStartRunning)
				{
					Parent.Renderable.ClearAnimationStateFlag(AnimModifier.Fatigued);
				}
			}
		}
	}

	/// <summary>
	/// 0 - 1
	/// a factor that simulates slowly digesting the stomach contents, even though the food items in it have already been destroyed
	/// </summary>
	public float StomachContents { get; private set; }

	/// <summary>
	/// the energy level is a weighted function of all needs that determine work efficiency (concentration, physical ability) and many other things
	/// if a need is below the specified limit, it will reduce energy level by a specified weight.
	/// </summary>
	public float EnergyLevel
	{
		get
		{
			if (energyLevelIsDirty)
			{
				energyLevel = ComputeEnergyLevel();
				energyLevelIsDirty = false;
			}
			return energyLevel;
		}
	}

	public BiologicalEntity(Entity parent)
		: base(parent)
	{
		AgeGroup = new AgeGroup(this);
		Needs = new Needs(this);
	}

	public BiologicalEntity()
	{
	}

	public bool IsEatable(IKnownEntityData food)
	{
		if (food.EntityType.ItemType == null || food.EntityType.ItemType.FoodType == null || !ConsumeProcesses.ContainsKey(food.EntityType))
		{
			return CanExtractFoodFromItem(food);
		}
		return true;
	}

	public bool CanExtractFoodFromItem(IKnownEntityData food)
	{
		if (ExtractionResultsInConsumable.ContainsKey(food.EntityType))
		{
			List<Tuple<ProcessType, EntityType, float>> listOfFoodTypes = null;
			return GetConsumableFoodExtractionResults(food, returnListOfFoodTypes: false, ref listOfFoodTypes);
		}
		return false;
	}

	public bool IsEatable(EntityType food)
	{
		return food.IsEatable(ExtractionResultsInConsumable, ConsumeProcesses);
	}

	public bool GetConsumableFoodExtractionResult(IKnownEntityData foodSource, ProcessType process, bool returnListOfFoodTypes, ref List<Tuple<ProcessType, EntityType, float>> listOfFoodTypes)
	{
		List<Tuple<EntityType, float>> list = process.TestProcess(foodSource);
		if (list != null && list.Count > 0)
		{
			foreach (Tuple<EntityType, float> item in list)
			{
				if (ConsumeProcesses.ContainsKey(item.Item1))
				{
					if (!returnListOfFoodTypes)
					{
						return true;
					}
					Common.AddToList(ref listOfFoodTypes, new Tuple<ProcessType, EntityType, float>(process, item.Item1, item.Item2));
				}
			}
		}
		if (listOfFoodTypes != null && listOfFoodTypes.Count > 0)
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// returns the types and amounts of consumable items that extracting from this food source would result in
	/// </summary>
	public bool GetConsumableFoodExtractionResults(IKnownEntityData foodSource, bool returnListOfFoodTypes, ref List<Tuple<ProcessType, EntityType, float>> listOfFoodTypes)
	{
		foreach (ProcessType item in ExtractionResultsInConsumable[foodSource.EntityType])
		{
			bool consumableFoodExtractionResult = GetConsumableFoodExtractionResult(foodSource, item, returnListOfFoodTypes, ref listOfFoodTypes);
			if (!returnListOfFoodTypes && consumableFoodExtractionResult)
			{
				return true;
			}
		}
		if (listOfFoodTypes != null && listOfFoodTypes.Count > 0)
		{
			return true;
		}
		return false;
	}

	private float GetNormalDistributedBioProperty(string meanPropertyKey, string stdDevPropertyKey, float meanDefaultValue, float stdDevDefaultValue)
	{
		BioProperty bioProperty = GetBioProperty(meanPropertyKey, meanDefaultValue);
		float? num = bioProperty.NumberValue.Value;
		bioProperty = GetBioProperty(stdDevPropertyKey, stdDevDefaultValue);
		float? num2 = bioProperty.NumberValue.Value;
		return (float)The.Sim.GameplayRandomGenerator.RandomNormalDistribution(num.Value, num2.Value);
	}

	private bool GetBioPropertyBoolean(string propertyKey, bool defaultValue)
	{
		return GetBioProperty(propertyKey, Parent.EntityType, CasteType, RaceType, AgeGroup.Age, Parent.EntityType.BiologicalType.OrderType, null, defaultValue).BoolValue.Value;
	}

	private float GetBioPropertyNumber(string propertyKey, float defaultValue)
	{
		return GetBioProperty(propertyKey, Parent.EntityType, CasteType, RaceType, AgeGroup.Age, Parent.EntityType.BiologicalType.OrderType, defaultValue).NumberValue.Value;
	}

	/// <summary>
	/// we can either cache this value if it will never change over the lifetime of an entity,
	/// or we can look it up each time (when Age changes?)
	/// </summary>
	public BioProperty GetBioProperty(string propertyKey, float? defaultValue = null)
	{
		return GetBioProperty(propertyKey, Parent.EntityType, CasteType, RaceType, AgeGroup.Age, Parent.EntityType.BiologicalType.OrderType, defaultValue);
	}

	public static BioProperty GetBioProperty(string propertyKey, EntityType entityType, CasteType caste, RaceType race, float age, BioOrderType order, float? defaultFloatValue = null, bool? defaultBoolValue = null)
	{
		BioProperty bioProperty = null;
		if (defaultFloatValue.HasValue)
		{
			bioProperty = new BioProperty
			{
				NumberValue = defaultFloatValue.Value
			};
		}
		else if (defaultBoolValue.HasValue)
		{
			bioProperty = new BioProperty
			{
				BoolValue = defaultBoolValue.Value
			};
		}
		if (!entityType.BiologicalType.GetBioPropertyType(propertyKey, out var bioPropertyType))
		{
			return bioProperty;
		}
		int stairstep;
		AgeGroupType stairStepIndex = Common.GetStairStepIndex(age, caste.AgeGroupTypes, out stairstep);
		BioProperty property = null;
		BioProperty property2 = null;
		BioProperty property3 = null;
		BioProperty property4 = null;
		stairStepIndex?.GetBioPropertyValue(bioPropertyType, out property);
		caste?.GetBioPropertyValue(bioPropertyType, out property2);
		race?.GetBioPropertyValue(bioPropertyType, out property3);
		order?.GetBioPropertyValue(bioPropertyType, out property4);
		if (bioPropertyType.InterpolateSetting == BioPropertyType.Interpolate.DefaultPriority)
		{
			return property ?? property3 ?? property2 ?? property4 ?? bioProperty;
		}
		return null;
	}

	public static bool GetBioPropertyValue(BioPropertyType propertyKey, Dictionary<string, BioProperty> bioProperties, out BioProperty property)
	{
		property = null;
		return bioProperties?.TryGetValue(propertyKey.KeyName, out property) ?? false;
	}

	/// <summary>
	/// random age is weighted by the age spans
	/// </summary>
	public void SetAgePreInit(float? age = null, AIAgeGroup? ageGroup = null)
	{
		if (age.HasValue)
		{
			AgeGroup.SetAge(age.Value);
			return;
		}
		if (!ageGroup.HasValue)
		{
			if (CasteType != null)
			{
				float edge = CasteType.AgeGroupTypes[CasteType.AgeGroupTypes.Count - 1].Edge;
				age = (float)The.Sim.GameplayRandomGenerator.NextDouble("BiologicalEntity") * edge;
				AgeGroup.SetAge(age.Value);
			}
			else
			{
				ageGroup = Common.GetRandomEnumValue(AIAgeGroup.Adult, The.Sim.GameplayRandomGenerator);
			}
		}
		AgeGroup.SetRandomAge(ageGroup.Value);
	}

	public void SetCasteOnNewEntity(string key)
	{
		CasteType = Parent.EntityType.BiologicalType.Castes.Find((CasteType r) => r.KeyName == key);
	}

	public void SetRaceOnNewEntity(string key)
	{
		RaceType = Parent.EntityType.BiologicalType.RaceTypes.FirstOrDefault((RaceType r) => r.KeyName == key);
	}

	/// <summary>
	/// sets random caste and race if not already set.
	/// computes weight and height and bulk from age if not already set (if bulk is zero!)
	/// </summary>
	public void Initialize()
	{
		if (CasteType == null)
		{
			SetRandomCaste();
		}
		if (RaceType == null && Parent.EntityType.BiologicalType.RaceTypes != null)
		{
			Common.GetStairStepIndex(Parent.EntityType.BiologicalType.RaceTypes, out var stairstep, The.Sim.GameplayRandomGenerator);
			RaceType = Parent.EntityType.BiologicalType.RaceTypes[stairstep];
		}
		Endurance = (float)The.Sim.GameplayRandomGenerator.RandomNormalDistribution(0.6000000238418579, 0.10000000149011612);
		Appearance = (float)The.Sim.GameplayRandomGenerator.RandomNormalDistribution(0.5, 0.10000000149011612);
		AdultTargetHeight = (float)The.Sim.GameplayRandomGenerator.RandomNormalDistribution(CasteType.HeightMean, CasteType.HeightStandardDeviation);
		AdultTargetWeight = (float)The.Sim.GameplayRandomGenerator.RandomNormalDistribution(CasteType.WeightMean, CasteType.WeightStandardDeviation);
		InitializeBioProperties();
		if (Common.IsZero(Parent.Bulk))
		{
			SetRandomWeight();
		}
		if (Common.IsZero(Height))
		{
			SetRandomHeight();
		}
		Needs.Initialize();
	}

	/// <summary>
	/// here we compute and cache all bio properties - properties that can be overridden and perhaps interpolated in the caste/age/race/order system
	/// 
	/// this could be triggered when age changes??
	/// </summary>
	private void InitializeBioProperties()
	{
		Resilience = GetNormalDistributedBioProperty("ResilienceMean", "ResilienceStandardDeviation", Parent.EntityType.BiologicalType.ResilienceMean, Parent.EntityType.BiologicalType.ResilienceStandardDeviation);
		ActiveStealthRating = GetBioPropertyNumber("ActiveStealthRating", Parent.EntityType.BiologicalType.ActiveStealthRating.Value);
		PassiveStealthRating = GetBioPropertyNumber("PassiveStealthRating", Parent.EntityType.BiologicalType.PassiveStealthRating ?? GameData.Instance.Constants.DefaultPassiveStealthFactor);
		OxygenAndMuscleEnergyIncreaseRatePerDay = GetBioPropertyNumber("OxygenAndMuscleEnergyIncreaseRatePerDay", Parent.EntityType.BiologicalType.OxygenAndMuscleEnergyIncreaseRatePerDay.Value);
		StomachSizeFractionOfEntityBulk = GetBioPropertyNumber("StomachSizeFractionOfEntityBulk", Parent.EntityType.BiologicalType.StomachSizeFractionOfEntityBulk.Value);
		TimeToConsumeFullMealInDays = GetBioPropertyNumber("TimeToConsumeFullMealInDays", Parent.EntityType.BiologicalType.TimeToConsumeFullMealInDays.Value);
		MaxRegainLimit = GetBioPropertyNumber("MaxRegainLimit", Parent.EntityType.BiologicalType.MaxRegainLimit.Value);
		FractionOfMaxHitpointsGainedPerDay = GetBioPropertyNumber("FractionOfMaxHitpointsGainedPerDay", Parent.EntityType.BiologicalType.FractionOfMaxHitpointsGainedPerDay.Value);
		IsVermin = GetBioPropertyBoolean("IsVermin", Parent.EntityType.BiologicalType.IsVermin);
		if (Parent.AgentStorage != null)
		{
			// added to ensure that stomach capacity is set correctly in case bulk already has a value - otherwise this happens in bulk change event
			Parent.AgentStorage.NotifyStomachFractionChanged();
		}
		InitializeConsumeAndExtractionProcesses();
	}

	private void InitializeConsumeAndExtractionProcesses()
	{
		Dictionary<EntityType, HashSet<ProcessType>> oldFoodExtractionProcesses = ((FoodExtractionProcesses == null) ? new Dictionary<EntityType, HashSet<ProcessType>>() : new Dictionary<EntityType, HashSet<ProcessType>>(FoodExtractionProcesses));
		Dictionary<EntityType, ProcessType> oldConsumeProcesses = ((ConsumeProcesses == null) ? new Dictionary<EntityType, ProcessType>() : new Dictionary<EntityType, ProcessType>(ConsumeProcesses));
		ConsumeProcesses = Parent.EntityType.BiologicalType.ConsumeProcesses;
		FoodExtractionProcesses = Parent.EntityType.BiologicalType.ExtractionProcesses;
		BiologicalType.ComputeExtractionResultsInConsumables(FoodExtractionProcesses, ConsumeProcesses, ref ExtractionResultsInConsumable);
		if (FoodExtraction.FoodProcessesHaveChanged(FoodExtractionProcesses, oldFoodExtractionProcesses, ConsumeProcesses, oldConsumeProcesses))
		{
			if (Parent.EntityType.Person != null)
			{
				Parent.PersonEntity.NotifyFoodProcessesChanged();
			}
			if (Parent.EntityType.IntelligenceType != null)
			{
				Parent.Intelligence.NotifyFoodProcessesChanged();
			}
		}
	}

	public void SetRandomCaste()
	{
		Common.GetStairStepIndex(Parent.EntityType.BiologicalType.Castes, out var stairstep, The.Sim.GameplayRandomGenerator);
		CasteType = Parent.EntityType.BiologicalType.Castes[stairstep];
	}

	private void SetRandomWeight()
	{
		float num = AgeGroup.Age / AgeGroup.GetAdultAge(CasteType.AgeGroupTypes);
		Weight = (float)(1.0 / (1.0 + Math.Pow(15.0, 0f - num) - 0.5) * 0.550000011920929 * (double)AdultTargetWeight);
		Parent.Bulk = GetBulkFromWeight(Weight);
	}

	public static float GetBulkFromWeight(float weight)
	{
		return 0.01f * weight;
	}

	public float GetTotalStomachCapacity()
	{
		return StomachSizeFractionOfEntityBulk * Parent.Bulk;
	}

	private void SetRandomHeight()
	{
		float num = AgeGroup.Age / AgeGroup.GetAdultAge(CasteType.AgeGroupTypes);
		Height = (float)(1.0 / (1.0 + Math.Pow(100.0, 0f - num) - 0.5) * 0.5 * (double)AdultTargetHeight);
	}

	public void SatisfyNeeds(double elapsedSeconds, float progressDelta, NeedSatisfaction[] needSatisfaction)
	{
		if (Needs != null && needSatisfaction != null)
		{
			Needs.SatisfyNeeds(elapsedSeconds, progressDelta, needSatisfaction);
		}
	}

	public void UpdateSimulation(double deltaTimeInSeconds)
	{
		Needs.Update(deltaTimeInSeconds);
		if (OxygenAndMuscleEnergy < 1f)
		{
			OxygenAndMuscleEnergy = Common.IncreaseValueBetweenZeroAndOne(OxygenAndMuscleEnergy, OxygenAndMuscleEnergyIncreaseRatePerDay * EnergyLevel, deltaTimeInSeconds);
		}
		AgeGroup.UpdateAge(deltaTimeInSeconds);
		if (StomachContents > 0f)
		{
			StomachContents = Common.DecreaseValueBetweenZeroAndOne(StomachContents, Parent.EntityType.BiologicalType.StomachContentsDecreaseRatePerDay.Value, deltaTimeInSeconds);
		}
		energyLevelIsDirty = true;
	}

	/// <summary>
	/// 0 - 1
	/// </summary>
	private float ComputeEnergyLevel()
	{
		if (Needs.NeedsList.Count > 0)
		{
			float num = 0f;
			{
				foreach (KeyValuePair<string, Need> needs in Needs.NeedsList)
				{
					float? energyFactor = needs.Value.GetEnergyFactor();
					if (energyFactor.HasValue)
					{
						num += energyFactor.Value;
					}
				}
				return num;
			}
		}
		return 1f;
	}

	public void GetStatus(out bool isDead, out bool isUnconscious, ref CauseOfDeath? causeOfDeath, ref CauseOfUnconsciousness? causeOfUnconsciousness)
	{
		isDead = false;
		isUnconscious = false;
		foreach (KeyValuePair<string, Need> needs in Needs.NeedsList)
		{
			if (needs.Value.PhysicalNeed != null)
			{
				if (needs.Value.PhysicalNeed.IsStarvedToDeath())
				{
					isDead = true;
					causeOfDeath = CauseOfDeath.Starvation;
				}
				else if (needs.Value.PhysicalNeed.IsCollapsedFromStarvation())
				{
					isUnconscious = true;
					causeOfUnconsciousness = CauseOfUnconsciousness.Starvation;
				}
			}
		}
	}

	public void GenerateBiologicalEntityData(EntityTypeTooltipInstanceData generatedData)
	{
		string text = null;
		string text2 = null;
		if (Parent.PersonEntity == null)
		{
			text2 = "";
			if (Parent.EntityType.BiologicalType.OrderType != null)
			{
				text2 = text2 + "ORDER: " + Parent.EntityType.BiologicalType.OrderType.Name + " \n";
			}
			if (Parent.EntityType.Name != null)
			{
				text = "SPECIES: " + Parent.EntityType.Name + " \n";
			}
		}
		string text3 = "";
		if (CasteType.Reproduction == Reproduction.Female)
		{
			text3 = "SEX: Female \n";
		}
		else if (CasteType.Reproduction == Reproduction.Male)
		{
			text3 = "SEX: Male \n";
		}
		string text4 = "AGE: " + AgeGroup.AgeGroupType.Name;
		generatedData.Description = text2 + text + text3 + text4;
		if (RaceType != null && RaceType.Description != null)
		{
			generatedData.RaceTypeDescription = RaceType.Description;
		}
	}

	public void AddToStomachContents(float foodBulk)
	{
		StomachContents += foodBulk / GetTotalStomachCapacity();
		StomachContents = Common.Clamp(StomachContents, 0f, 1f);
	}

	public void SetStomachContents(float contents)
	{
		StomachContents = Common.Clamp(contents, 0f, 1f);
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		ActiveStealthRating = sn.DoFloat(ActiveStealthRating);
		AdultTargetHeight = sn.DoFloat(AdultTargetHeight);
		AdultTargetWeight = sn.DoFloat(AdultTargetWeight);
		AgeGroup = (AgeGroup)sn.DoISnapshot(AgeGroup);
		Appearance = sn.DoFloat(Appearance);
		Blood = sn.DoFloat(Blood);
		casteKey = sn.DoString((CasteType != null) ? CasteType.KeyName : null);
		raceKey = sn.DoString((RaceType != null) ? RaceType.KeyName : null);
		DwellingSpot = sn.DoVector3Nullable(DwellingSpot);
		Endurance = sn.DoFloat(Endurance);
		energyLevel = sn.DoFloat(energyLevel);
		energyLevelIsDirty = sn.DoBool(energyLevelIsDirty);
		FractionOfMaxHitpointsGainedPerDay = sn.DoFloat(FractionOfMaxHitpointsGainedPerDay);
		Height = sn.DoFloat(Height);
		MaxRegainLimit = sn.DoFloat(MaxRegainLimit);
		Needs = (Needs)sn.DoISnapshot(Needs);
		oxygenAndMuscleEnergy = sn.DoFloat(oxygenAndMuscleEnergy);
		OxygenAndMuscleEnergyIncreaseRatePerDay = sn.DoFloat(OxygenAndMuscleEnergyIncreaseRatePerDay);
		PassiveStealthRating = sn.DoFloat(PassiveStealthRating);
		Resilience = sn.DoFloat(Resilience);
		StomachContents = sn.DoFloat(StomachContents);
		StomachSizeFractionOfEntityBulk = sn.DoFloat(StomachSizeFractionOfEntityBulk);
		TimeToConsumeFullMealInDays = sn.DoFloat(TimeToConsumeFullMealInDays);
		Weight = sn.DoFloat(Weight);
		IsVermin = sn.DoBool(IsVermin);
		ModelTextureName = sn.DoString(ModelTextureName);
		ConsumeProcesses = sn.DoDictionary(ConsumeProcesses);
		ExtractionResultsInConsumable = sn.DoMultiMapHashSet(ExtractionResultsInConsumable);
		FoodExtractionProcesses = sn.DoMultiMapHashSet(FoodExtractionProcesses);
		sn.Ignore(CasteType);
		sn.Ignore(RaceType);
		sn.Postpone(BiologicalChildren);
		sn.Postpone(Mate);
		sn.Postpone(BiologicalFather);
		sn.Postpone(BiologicalMother);
		return this;
	}

	/// <summary>
	/// When the snapshotted fields change, increase this version number and add repair logic for older snapshots.
	/// </summary>
	public override Snapshotter.Version DoVersion(Snapshotter sn)
	{
		base.DoVersion(sn);
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		base.LoadPostProcess(sn);
		CasteType = Parent.EntityType.BiologicalType.Castes.FirstOrDefault((CasteType c) => c.KeyName.Equals(casteKey));
		if (raceKey != null)
		{
			RaceType = Parent.EntityType.BiologicalType.RaceTypes.FirstOrDefault((RaceType c) => c.KeyName.Equals(raceKey));
		}
		AgeGroup.LoadPostProcess(sn);
		Needs.Parent = this;
		Needs.LoadPostProcess(sn);
	}
}
