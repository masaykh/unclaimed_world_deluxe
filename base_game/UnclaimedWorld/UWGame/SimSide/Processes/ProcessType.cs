using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Renderables;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Locomotors.Stances;
using UWGame.SimSide.Entities.Substances;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Jobs.JobTypes;
using UWGame.SimSide.Policies;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Systems.Triggers;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.Processes;

public class ProcessType : IGameData, IXmlSerializable
{
	public enum ProductionUI
	{
		None,
		Build,
		Slider,
		Other
	}

	public string SummaryDescription;

	public string Description;

	public string SpecialActionCaption;

	public string JobTypeKey;

	[XmlIgnore]
	public ProcessJobType JobType;

	public bool IsPseudoProcess;

	public int SortOrder;

	public string Tooltip;

	public string ProcessToolSetKey;

	public TierOrArea TierOrArea;

	[XmlIgnore]
	public TierOrAreaType TierOrAreaType;

	public string UserCannotCancelReason;

	public bool? UserCanCancel;

	public bool IsSalvageProcess;

	[XmlIgnore]
	public bool IsSalvageWithoutWaste;

	public bool IsConsumeProcess;

	public bool IsGathering;

	public RepairAction? RepairAction;

	[XmlIgnore]
	public bool IsUpgrade;

	public bool MoveOutputToWorkerWhenCompleted;

	public bool UseWorkerEnergyAsProductionFactor = true;

	public float? PhysicalWorkFactor;

	public bool RequiresBoldStance;

	public float? UpdateInterval;

	public bool IsMetaAction;

	public WorkerNeededOptions WorkNeeded = WorkerNeededOptions.WorkerNeeded;

	public int MaxWorkers = 1;

	public string RequiredSkill;

	public string[] RequiredKnowledge;

	public string[] ProducesKnowledge;

	public List<ProgressFactorProperty> ProgressFactorProperties;

	public SerializableDictionary<string, float> TotalContinuousSubstanceInput;

	[XmlIgnore]
	public SerializableDictionary<SubstanceType, float> TotalContinuousSubstanceInputTypes;

	[XmlIgnore]
	public EntityType ActingOnType;

	[XmlIgnore]
	public bool IsOriginalSpecialAction;

	public float? ProgressPerSecondCap;

	public Input[] Inputs;

	public Output[] Outputs;

	[XmlIgnore]
	public ResourceType ResourceTypeInput;

	public NeedSatisfaction[] SatisfiesWorkerNeeds;

	public bool HasNoEndpoint;

	public WorkOrTime WorkOrTimeNeeded;

	public SerializableDictionary<string, ChanceToTakeStance[]> Stances;

	[XmlIgnore]
	public Dictionary<StancesType, List<ChanceToTakeStance>> StanceTypes;

	public AnimAction AgentActionState = AnimAction.Mending;

	public AnimModifier[] AgentAnimationStates;

	[XmlIgnore]
	public List<AnimModifier> AgentAnimationStatesList;

	public TriggerType[] Triggers;

	[XmlIgnore]
	public Dictionary<AgentActionHooks, List<ActionSets>> EventActions = new Dictionary<AgentActionHooks, List<ActionSets>>();

	[XmlIgnore]
	public bool IsInnateExtractionProcess;

	[XmlIgnore]
	public bool IsReplenishProcess;

	public GoalReplenish.ReplenishAction? ReplenishAction;

	[XmlIgnore]
	public bool? SetPreparedProperty;

	public StateModifier? PreparedToolModifier;

	public bool? AttacksVerminValueToSet;

	public string[] EnablesSpecialActionLockProcesses;

	public string[] DisablesSpecialActionLockProcesses;

	[XmlIgnore]
	public List<ProcessType> DisablesSpecialActionLockProcessTypes;

	[XmlIgnore]
	public List<ProcessType> EnablesSpecialActionLockProcessTypes;

	public string[] EnablesSharedActionProcesses;

	public string[] DisablesSharedActionProcesses;

	[XmlIgnore]
	public List<ProcessType> EnablesSharedActionProcessTypes;

	[XmlIgnore]
	public List<ProcessType> DisablesSharedActionProcessTypes;

	public bool ShowDisabledSpecialAction = true;

	[XmlIgnore]
	public SkillType RequiredSkillType;

	[XmlIgnore]
	public ProcessToolSet ProcessToolSet;

	[XmlIgnore]
	public Dictionary<EntityType, Input> InputsByType;

	[XmlIgnore]
	public bool IsKilling;

	[XmlIgnore]
	private ProcessType originalProcess;

	[XmlIgnore]
	public bool UseOriginalProcessEvents = true;

	private static float? worstCaseSkillAndEnergyFactorsRoot;

	public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(ProcessType))
	{
		TypeMappings = DataLoader.GetListOfTypeMappings()
	};

	public string KeyName { get; set; }

	public string Name { get; set; }

	public bool DeleteRecord { get; set; }

	[XmlIgnore]
	public bool? SpecialActionEnabledAtStart { get; private set; }

	[XmlIgnore]
	public float FinalUpdateInterval { get; private set; }

	public ProcessType OriginalProcess => originalProcess ?? this;

	public bool UsesWorkerEnergyAsProductionFactor
	{
		get
		{
			if (WorkNeeded == WorkerNeededOptions.WorkerNeeded)
			{
				return UseWorkerEnergyAsProductionFactor;
			}
			return false;
		}
	}

	public bool IsSpecialActionType
	{
		get
		{
			if (!IsOriginalSpecialAction)
			{
				return ActingOnType != null;
			}
			return true;
		}
	}

	public bool HasOutput
	{
		get
		{
			if (Outputs != null)
			{
				return Outputs.Length != 0;
			}
			return false;
		}
	}

	public bool CreatesNewEntity
	{
		get
		{
			if (Outputs != null)
			{
				return Array.Exists(Outputs, (Output o) => o.FinalEntityTypeToCreate != null);
			}
			return false;
		}
	}

	public ProcessType()
	{
	}

	public ProcessType(ProcessType original, string newKeyname, bool? specialActionEnabledAtStart)
	{
		KeyName = newKeyname;
		Name = original.Name;
		SummaryDescription = original.SummaryDescription;
		Tooltip = original.Tooltip;
		Description = original.Description;
		SortOrder = original.SortOrder;
		SpecialActionEnabledAtStart = specialActionEnabledAtStart;
		SpecialActionCaption = original.SpecialActionCaption;
		UserCanCancel = original.UserCanCancel;
		DisablesSpecialActionLockProcesses = original.DisablesSpecialActionLockProcesses;
		EnablesSpecialActionLockProcesses = original.EnablesSpecialActionLockProcesses;
		DisablesSharedActionProcesses = original.DisablesSharedActionProcesses;
		EnablesSharedActionProcesses = original.EnablesSharedActionProcesses;
		ShowDisabledSpecialAction = original.ShowDisabledSpecialAction;
		originalProcess = original;
		ActingOnType = original.ActingOnType;
		AttacksVerminValueToSet = original.AttacksVerminValueToSet;
		Inputs = original.Inputs;
		Outputs = original.Outputs;
		AgentActionState = original.AgentActionState;
		AgentAnimationStates = original.AgentAnimationStates;
		UseWorkerEnergyAsProductionFactor = original.UseWorkerEnergyAsProductionFactor;
		Stances = original.Stances;
		RequiredSkill = original.RequiredSkill;
		PhysicalWorkFactor = original.PhysicalWorkFactor;
		RequiresBoldStance = original.RequiresBoldStance;
		ProcessToolSetKey = original.ProcessToolSetKey;
		WorkNeeded = original.WorkNeeded;
		IsMetaAction = original.IsMetaAction;
		HasNoEndpoint = original.HasNoEndpoint;
		WorkOrTimeNeeded = original.WorkOrTimeNeeded;
		Triggers = original.Triggers;
		PreparedToolModifier = original.PreparedToolModifier;
		RepairAction = original.RepairAction;
		IsGathering = original.IsGathering;
		IsConsumeProcess = original.IsConsumeProcess;
		MoveOutputToWorkerWhenCompleted = original.MoveOutputToWorkerWhenCompleted;
		TierOrArea = original.TierOrArea;
		JobTypeKey = original.JobTypeKey;
		IsSalvageProcess = original.IsSalvageProcess;
		ReplenishAction = original.ReplenishAction;
		SatisfiesWorkerNeeds = original.SatisfiesWorkerNeeds;
	}

	public void InitDynamicProcess()
	{
		GameData.Instance.AllProcessTypes.Add(KeyName, this);
		GameData.InitializeComputerGeneratedData(this);
		Initialize();
		PostLoadContentInitialize();
		GameData.Instance.AddProcessToProductionGraph(this);
	}

	public override string ToString()
	{
		return Name + "(" + KeyName + ")";
	}

	private float GetUpdateInterval()
	{
		if (UpdateInterval.HasValue)
		{
			return UpdateInterval.Value;
		}
		if (WorkNeeded == WorkerNeededOptions.WorkerNeeded)
		{
			return 2f;
		}
		if (CreatesNewEntity)
		{
			return 2f;
		}
		return 8f;
	}

	public void Initialize()
	{
	}

	public bool AllowProcessOnBrokenTarget()
	{
		if (!IsMetaAction)
		{
			return RepairAction.HasValue;
		}
		return true;
	}

	public bool NeedsImmovableInput()
	{
		if (Inputs != null && Inputs.FirstOrDefault((Input i) => i.InputIsImmovable()) != null)
		{
			return true;
		}
		return false;
	}

	public EntityType ImmovableInput()
	{
		if (Inputs != null)
		{
			Input[] inputs = Inputs;
			foreach (Input input in inputs)
			{
				if (input.EntityType.IsImmovable())
				{
					return input.EntityType;
				}
			}
		}
		return null;
	}

	public static void InitializeStances(SerializableDictionary<string, ChanceToTakeStance[]> stances, out Dictionary<StancesType, List<ChanceToTakeStance>> stanceTypes)
	{
		stanceTypes = null;
		if (stances == null)
		{
			return;
		}
		stanceTypes = new Dictionary<StancesType, List<ChanceToTakeStance>>();
		foreach (KeyValuePair<string, ChanceToTakeStance[]> stance in stances)
		{
			StancesType key = GameData.Instance.AllStancesTypes[stance.Key];
			ChanceToTakeStance[] value = stance.Value;
			foreach (ChanceToTakeStance chanceToTakeStance in value)
			{
				chanceToTakeStance.Initialize();
				Common.AddToMultiList(stanceTypes, key, chanceToTakeStance);
			}
		}
	}

	public bool UsesAnchor()
	{
		if (IsSpecialActionType && IsConstruction())
		{
			return true;
		}
		return false;
	}

	public float GetTimeNeeded(float totalBulkOfInput = 1f)
	{
		float value = WorkOrTimeNeeded.DaysNeeded.Value;
		return 1f * value;
	}

	public float CalculateProgressDelta(double seconds, float skillProductivity, float energyProductivity, float toolProductivity, float totalInputBulk = 1f, float averageLaborEfficiency = 1f)
	{
		float timeNeeded = GetTimeNeeded(totalInputBulk);
		float num = EstimateTotalDurationInDays(timeNeeded, skillProductivity, energyProductivity, toolProductivity, averageLaborEfficiency);
		if (Common.IsZero(num))
		{
			return 1f;
		}
		return (float)(seconds * The.Sim.DateAndTime.DaysPerSecond / (double)num);
	}

	public bool IsConstruction()
	{
		if (Outputs != null)
		{
			return Outputs[0].FinalEntityTypeToCreate.StructureType != null;
		}
		return false;
	}

	public AgentActionHooks GetStartHook()
	{
		if (IsConstruction())
		{
			return AgentActionHooks.StartConstructing;
		}
		if (IsGathering)
		{
			return AgentActionHooks.StartHarvesting;
		}
		return AgentActionHooks.StartProducing;
	}

	public bool IsPartOfAttainableCalculation()
	{
		if (Outputs != null)
		{
			if (!IsAccessibleFromInventoryPanel())
			{
				return IsSalvageProcess;
			}
			return true;
		}
		return false;
	}

	public bool IsAccessibleFromInventoryPanel()
	{
		if (Outputs != null)
		{
			return IsPartOfProductionChain();
		}
		return false;
	}

	public bool IsPartOfProductionChainButCannotOrderFromInventory()
	{
		if (!IsGathering && !IsKilling && ActingOnType == null && !IsUpgrade)
		{
			return IsPseudoProcess;
		}
		return true;
	}

	public ProductionUI GetProductionUI()
	{
		if (!IsSalvageProcess && !IsPartOfProductionChainButCannotOrderFromInventory())
		{
			if (IsConstruction())
			{
				return ProductionUI.Build;
			}
			return ProductionUI.Slider;
		}
		return ProductionUI.Other;
	}

	public bool IsPartOfProductionChain()
	{
		if (!IsSalvageProcess && !IsConsumeProcess && !IsInnateExtractionProcess && !IsReplenishProcess)
		{
			return !RepairAction.HasValue;
		}
		return false;
	}

	public float EstimateTotalDurationInDays(Entity worker, float timeNeeded, float toolProductivityFactor = 1f, float averageLaborEfficiency = 1f)
	{
		float skillProductivity = 1f;
		float workerEnergyProductivity = 1f;
		if (worker != null)
		{
			skillProductivity = worker.Intelligence.GetSkillProductionFactor(RequiredSkillType);
			workerEnergyProductivity = GetEnergyProductivity(worker);
		}
		return EstimateTotalDurationInDays(timeNeeded, skillProductivity, workerEnergyProductivity, toolProductivityFactor, averageLaborEfficiency);
	}

	public float EstimateTotalDurationInDays(float timeNeeded, float skillProductivity = 1f, float workerEnergyProductivity = 1f, float toolProductivity = 1f, float averageLaborEfficiency = 1f)
	{
		float totalFactorProductivity = ProcessJob.GetTotalFactorProductivity(workerEnergyProductivity, toolProductivity, skillProductivity, averageLaborEfficiency);
		return timeNeeded / totalFactorProductivity;
	}

	public float GetEnergyProductivity(Entity worker)
	{
		float result = 1f;
		if (worker.BiologicalEntity != null)
		{
			float energyLevel = worker.BiologicalEntity.EnergyLevel;
			if (WorkNeeded == WorkerNeededOptions.WorkerNeeded && UsesWorkerEnergyAsProductionFactor)
			{
				result = MathHelper.Lerp(GameData.Instance.Constants.ZeroEnergyProductionFactor, 1f, energyLevel);
			}
		}
		return result;
	}

	public List<Tuple<EntityType, float>> TestProcess(IKnownEntityData inputEntity)
	{
		if (Common.IsZero(inputEntity.Bulk))
		{
			return null;
		}
		if (InputsByType.Count != 1)
		{
			return null;
		}
		if (!InputsByType.TryGetValue(inputEntity.EntityType, out var value))
		{
			return null;
		}
		List<Tuple<EntityType, float>> products = null;
		Dictionary<string, float> list = null;
		if (value.Amount.NoOfItems == 1)
		{
			if (value.Amount.SubstanceTypes == null)
			{
				CreateTestOutputs(ref products);
				return products;
			}
			if (inputEntity.SubstanceBulkAmounts == null)
			{
				CreateTestOutputs(ref products);
				return products;
			}
			foreach (SubstanceType substanceType in value.Amount.SubstanceTypes)
			{
				if (inputEntity.SubstanceBulkAmounts.TryGetValue(substanceType, out var value2) && Common.IsGreaterThan(value2.Amount, 0f))
				{
					Common.AddToDictionary(ref list, substanceType.KeyName, value2.Amount);
				}
			}
			if (list != null)
			{
				Output[] outputs = Outputs;
				foreach (Output output in outputs)
				{
					int noOfItemsToCreate = 1;
					if (!output.GetOutputAmountsToCreate(inputEntity.Bulk, list, out noOfItemsToCreate, out var bulkOfEachOutputItem))
					{
						return null;
					}
					Common.AddToList(ref products, new Tuple<EntityType, float>(output.FinalEntityTypeToCreate, (float)noOfItemsToCreate * (bulkOfEachOutputItem ?? 0f)));
				}
				return products;
			}
		}
		return null;
	}

	private void CreateTestOutputs(ref List<Tuple<EntityType, float>> products)
	{
		products = new List<Tuple<EntityType, float>>();
		Output[] outputs = Outputs;
		foreach (Output output in outputs)
		{
			products.Add(new Tuple<EntityType, float>(output.FinalEntityTypeToCreate, output.FinalEntityTypeToCreate.ItemType.MaximumBulk ?? 0f));
		}
	}

	public void PostLoadContentInitialize()
	{
		if (UseOriginalProcessEvents && GameData.Instance.EventHooksByProcessType.TryGetValue(OriginalProcess, out var value))
		{
			foreach (ProcessTypeActionHook item in value)
			{
				List<ActionSets> value2 = null;
				if (!EventActions.TryGetValue(item.Hook, out value2))
				{
					value2 = new List<ActionSets>();
					EventActions.Add(item.Hook, value2);
				}
				value2.Add(GameData.Instance.AllActionSets[item.ActionSetsKey]);
			}
		}
		FinalUpdateInterval = GetUpdateInterval();
		if (!IsGathering || Outputs == null)
		{
			return;
		}
		Output[] outputs = Outputs;
		foreach (Output output in outputs)
		{
			if (GameData.Instance.ItemHarvestSource.TryGetValue(output.FinalEntityTypeToCreate, out var value3))
			{
				ResourceTypeInput = value3;
				break;
			}
		}
	}

	public void SetIsSalvageProcess()
	{
		IsSalvageProcess = true;
	}

	private bool GetIsKilling()
	{
		if (Outputs != null)
		{
			Output[] outputs = Outputs;
			foreach (Output output in outputs)
			{
				if (output.FinalEntityTypeToCreate.ItemType != null && output.FinalEntityTypeToCreate.ItemType.CarcassType != null)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool ConsumeInputsAtBeginning()
	{
		return !IsReplenishProcess;
	}

	public bool UsesItemsAsInput()
	{
		return true;
	}

	public void PreInitValidate(ref List<string> listOfErrors)
	{
	}

	public void PostInitValidate(ref List<string> listOfErrors)
	{
	}

	public void PostDataCompleteInitialize()
	{
		if (Inputs != null)
		{
			InputsByType = new Dictionary<EntityType, Input>();
			Input[] inputs = Inputs;
			foreach (Input input in inputs)
			{
				input.PostDataCompleteInitialize();
				InputsByType.Add(input.EntityType, input);
			}
		}
		if (RequiredSkill != null)
		{
			RequiredSkillType = GameData.Instance.AllSkillTypes[RequiredSkill];
		}
		if (!string.IsNullOrEmpty(ProcessToolSetKey))
		{
			ProcessToolSet = GameData.Instance.AllProcessToolSets[ProcessToolSetKey];
		}
		if (Outputs != null)
		{
			Output[] outputs = Outputs;
			foreach (Output obj in outputs)
			{
				obj.PostDataCompleteInitialize();
				if (obj.FinalEntityTypeToCreate.Upgrader != null)
				{
					IsUpgrade = true;
				}
			}
		}
		if (InputsByType != null && !IsReplenishProcess)
		{
			foreach (KeyValuePair<EntityType, Input> item in InputsByType)
			{
				if (item.Value.EntityType.ItemType == null || item.Value.IsConsumed)
				{
					continue;
				}
				if (item.Value.BecomesPartOfProductType == null)
				{
					item.Value.BecomesPartOfProductType = Outputs[0].FinalEntityTypeToCreate;
					continue;
				}
				Output[] outputs = Outputs;
				foreach (Output output in outputs)
				{
					if (output.FinalEntityTypeToCreate.KeyName == item.Value.BecomesPartOfProduct)
					{
						item.Value.BecomesPartOfProductType = output.FinalEntityTypeToCreate;
						break;
					}
				}
			}
		}
		if (AgentAnimationStates != null)
		{
			AgentAnimationStatesList = AgentAnimationStates.ToList();
		}
		if (WorkOrTimeNeeded != null)
		{
			WorkOrTimeNeeded.Initialize();
		}
		if (DisablesSpecialActionLockProcesses != null)
		{
			DisablesSpecialActionLockProcessTypes = new List<ProcessType>();
			string[] disablesSpecialActionLockProcesses = DisablesSpecialActionLockProcesses;
			foreach (string key in disablesSpecialActionLockProcesses)
			{
				DisablesSpecialActionLockProcessTypes.Add(GameData.Instance.AllProcessTypes[key]);
			}
		}
		if (EnablesSpecialActionLockProcesses != null)
		{
			EnablesSpecialActionLockProcessTypes = new List<ProcessType>();
			string[] disablesSpecialActionLockProcesses = EnablesSpecialActionLockProcesses;
			foreach (string key2 in disablesSpecialActionLockProcesses)
			{
				EnablesSpecialActionLockProcessTypes.Add(GameData.Instance.AllProcessTypes[key2]);
			}
		}
		if (DisablesSharedActionProcesses != null)
		{
			DisablesSharedActionProcessTypes = new List<ProcessType>();
			string[] disablesSpecialActionLockProcesses = DisablesSharedActionProcesses;
			foreach (string key3 in disablesSpecialActionLockProcesses)
			{
				DisablesSharedActionProcessTypes.Add(GameData.Instance.AllProcessTypes[key3]);
			}
		}
		if (EnablesSharedActionProcesses != null)
		{
			EnablesSharedActionProcessTypes = new List<ProcessType>();
			string[] disablesSpecialActionLockProcesses = EnablesSharedActionProcesses;
			foreach (string key4 in disablesSpecialActionLockProcesses)
			{
				EnablesSharedActionProcessTypes.Add(GameData.Instance.AllProcessTypes[key4]);
			}
		}
		if (JobTypeKey != null)
		{
			JobType = (ProcessJobType)GameData.Instance.AllJobTypes[JobTypeKey];
		}
		if (TierOrArea != null)
		{
			TierOrAreaType = new TierOrAreaType(TierOrArea);
		}
		InitializeStances(Stances, out StanceTypes);
		IsKilling = GetIsKilling();
		IsSalvageWithoutWaste = GetIsSalvageWithoutWaste();
	}

	private bool GetIsSalvageWithoutWaste()
	{
		if (IsSalvageProcess)
		{
			Dictionary<EntityType, int> parts = Inputs[0].EntityType.NonLivingType.Parts;
			if (parts == null && Outputs == null)
			{
				return true;
			}
			if (parts == null)
			{
				// PORT FIX. An object with outputs but no declared Parts fell into the loop below
				// and dereferenced a null dictionary. Unreachable in the stock tables - every one
				// of the 33 items the studio gave a disassembly also declares its PartKeys - and
				// reached immediately by any mod that adds a salvage recipe to something that does
				// not, which is what UWGame.Mods.DisassemblyMod does for every assembled tool.
				//
				// False rather than true: "without losing any parts" is a claim checked against
				// the parts list, and an object that declares none gives nothing to check it
				// against. Callers only choose a caption by it - HUDEntityContextMenu offers
				// PACKING DOWN when true and BEGIN (salvage) when false - so the conservative
				// answer costs a word and promises nothing that might not be kept.
				return false;
			}
			if (Outputs != null)
			{
				Output[] outputs = Outputs;
				foreach (Output output in outputs)
				{
					if (!output.IsWasteProduct && (!parts.TryGetValue(output.FinalEntityTypeToCreate, out var value) || value != (output.Amount.NoOfItems ?? 1)))
					{
						return false;
					}
				}
				if (parts != null)
				{
					foreach (KeyValuePair<EntityType, int> item in parts)
					{
						Output output2 = Outputs.FirstOrDefault((Output o) => o.FinalEntityTypeToCreate == item.Key);
						if (output2 == null || (output2.Amount.NoOfItems ?? 1) != item.Value)
						{
							return false;
						}
					}
					return true;
				}
				return false;
			}
			return false;
		}
		return false;
	}

	/// <summary>
	/// Whether this salvage process's single input type declares a parts list.
	///
	/// It matters because a salvage process does not MAKE its non-waste outputs, it hands back
	/// the input's own part entities - <c>SimProcess.ConsumeInputsAndGatherParts</c> destroys the
	/// input with <c>destroyParts: false</c> and <c>SimProcess.CreateOutputsFromInputs</c> re-uses
	/// what it collected. An input type that declares no parts has none to hand back, so a recipe
	/// over one has to create its outputs instead. The studio's validator made that case
	/// impossible; the port relaxed the rule for <c>UWGame.Mods.DisassemblyMod</c>, so it is real
	/// now and both the sim and tools/DataExport ask this question rather than each guessing.
	/// </summary>
	public bool SalvageInputDeclaresParts()
	{
		if (Inputs == null || Inputs.Length != 1)
		{
			return false;
		}
		EntityType entityType = Inputs[0].EntityType;
		return entityType?.NonLivingType?.Parts != null;
	}

	/// <summary>
	/// Whether salvaging will actually produce <paramref name="output" />, as opposed to
	/// destroying the input and quietly yielding nothing. Mirrors the three branches of
	/// <c>SimProcess.CreateOutputsFromInputs</c> for a salvage process:
	///
	///   waste          created fresh - it never was a part of anything
	///   a declared part handed back from the destroyed input, if the instance still has it
	///   neither        created fresh, since there was no parts list it could have come from
	///
	/// The middle case is the studio's, and is why this asks about the TYPE's parts rather than
	/// an instance's: a broken-off part that is gone at salvage time is a loss on purpose.
	/// Offline callers use this to check a table without running the sim over it.
	/// </summary>
	public bool SalvageOutputWillBeCreated(Output output)
	{
		if (!IsSalvageProcess || output == null)
		{
			return true;
		}
		if (SalvageOutputIsCreatedFresh(output))
		{
			return true;
		}
		if (!SalvageInputDeclaresParts())
		{
			// Not created fresh and no parts to come from: the sim destroys the input and yields
			// nothing. Unreachable while SalvageOutputIsCreatedFresh says what it says - and left
			// here answering FALSE rather than throwing, because this is the shape the bug had and
			// a report that cannot describe it is no use for catching it coming back.
			return false;
		}
		Dictionary<EntityType, int> parts = Inputs[0].EntityType.NonLivingType.Parts;
		return parts.ContainsKey(output.FinalEntityTypeToCreate);
	}

	/// <summary>
	/// Whether salvaging has to MAKE <paramref name="output" /> rather than hand back one of the
	/// destroyed input's part entities. True for waste, which never was a part of anything, and for
	/// an input type that declares no parts, which has nothing to hand back.
	///
	/// <c>SimProcess.CreateOutputsFromInputs</c> asks this at the point where it has already failed
	/// to find the output among the parts it gathered: false there means the part existed in the
	/// type and is gone from this instance, and losing it is the studio's intent.
	/// </summary>
	public bool SalvageOutputIsCreatedFresh(Output output)
	{
		return output.IsWasteProduct || !SalvageInputDeclaresParts();
	}

	public void PreDataCompleteValidate(ref List<string> listOfErrors)
	{
		if (Inputs != null)
		{
			Input[] inputs = Inputs;
			for (int i = 0; i < inputs.Length; i++)
			{
				inputs[i].PreDataCompleteValidate(ref listOfErrors);
			}
		}
		if (Outputs != null)
		{
			Output[] outputs = Outputs;
			for (int i = 0; i < outputs.Length; i++)
			{
				outputs[i].PreDataCompleteValidate(ref listOfErrors);
			}
		}
		if (!string.IsNullOrEmpty(ProcessToolSetKey))
		{
			EntityType.ValidateGameDataTypeExists(ref listOfErrors, ProcessToolSetKey, GameData.Instance.AllProcessToolSets, out var _);
		}
	}

	public void PostDataCompleteValidate(ref List<string> listOfErrors)
	{
		if (InputsByType != null)
		{
			foreach (KeyValuePair<EntityType, Input> item in InputsByType)
			{
				item.Value.Validate(ref listOfErrors);
				if (item.Value.EntityType.ItemType != null && !item.Value.IsConsumed && item.Value.BecomesPartOfProductType == null)
				{
					EntityType.CreateValidationError(ref listOfErrors, "The process input does not have a correct output specified to become part of.");
				}
				if (!IsSalvageProcess && item.Value.EntityType.ItemType != null && !item.Value.IsConsumed && (item.Value.BecomesPartOfProductType.Parts == null || !item.Value.BecomesPartOfProductType.Parts.ContainsKey(item.Key)))
				{
					EntityType.CreateValidationError(ref listOfErrors, $"The process input {item.Key.KeyName} is specified to become a part of the output, but the output entity type does not have such a part defined.");
				}
				if (item.Value.EntityType.ItemType != null && !item.Value.IsConsumed && Outputs != null && Outputs.Count() == 1 && !IsSalvageProcess)
				{
					EntityType entityType = GameData.Instance.AllEntityTypes[Outputs[0].EntityTypeToCreate];
					if (entityType.Parts != null)
					{
						if (entityType.Parts.ContainsKey(item.Value.EntityType) && entityType.Parts[item.Value.EntityType] != item.Value.Amount.NoOfItems)
						{
							EntityType.CreateValidationError(ref listOfErrors, "The amounts of inputs did not match the number of parts of the same type in the output.");
						}
					}
					else
					{
						EntityType.CreateValidationError(ref listOfErrors, "The amounts of inputs did not match the number of parts of the same type in the output, because the parts have not been filled out.");
					}
				}
				if (ProcessToolSet == null)
				{
					continue;
				}
				ToolAlternatives[] tools = ProcessToolSet.Tools;
				for (int i = 0; i < tools.Length; i++)
				{
					Tool[] tools2 = tools[i].Tools;
					for (int j = 0; j < tools2.Length; j++)
					{
						if (tools2[j].ToolEntityTypes.Contains(item.Key))
						{
							EntityType.CreateValidationError(ref listOfErrors, $"The same entity type ({item.Key}) is used as both tool and input. This is currently not allowed...");
						}
					}
				}
			}
			int num = Inputs.Count((Input input) => input.InputIsImmovable());
			if (num > 1)
			{
				EntityType.CreateValidationError(ref listOfErrors, "Only one input may have HasNoMaximumBulk or a maximum bulk over 1.");
			}
			if (ProcessToolSet != null)
			{
				ToolAlternatives[] tools = ProcessToolSet.Tools;
				for (int i = 0; i < tools.Length; i++)
				{
					if (tools[i].NeedsImmobileTool() && num > 0)
					{
						EntityType.CreateValidationError(ref listOfErrors, "It is not permitted to specify both an immobile input (HasNoMaximumBulk = true or maximum bulk over 1) and a required immobile tool.");
					}
				}
			}
			if (IsSalvageProcess)
			{
				if (Inputs.Length != 1)
				{
					EntityType.CreateValidationError(ref listOfErrors, "Salvage processes should always have exactly one input.");
				}
				// PORT FIX. `Parts` is null for any item that does not declare PartKeys, and the
				// loop below walks it. Unreachable in the stock tables for the same reason as the
				// sibling fix in GetIsSalvageWithoutWaste - all 33 items the studio gave a
				// disassembly also declare their parts - and it took the game down on the next
				// screen for anyone running UWGame.Mods.DisassemblyMod, which gives a salvage
				// recipe to tools that declare none.
				//
				// Skipping the check rather than reporting an error is the honest answer: these
				// rules ask whether the outputs match the item's declared parts, and an item with
				// no parts list is not failing them, it is outside them.
				//
				// The rule still governs every item that DOES declare its parts, which is the whole
				// stock game: this guard narrows it, it does not remove it. On the other side,
				// UWGame.Mods.DisassemblyMod refuses to generate a recipe for a parts-declaring
				// item unless the recovery is a subset of those parts, and enforces the invariant
				// this rule was protecting - never hand back more of a material than making one
				// consumed - on the items it does cover.
				if (Outputs != null && Inputs[0].EntityType.Parts != null)
				{
					Output[] outputs = Outputs;
					foreach (Output output in outputs)
					{
						bool flag = false;
						foreach (KeyValuePair<EntityType, int> part in Inputs[0].EntityType.Parts)
						{
							if (!(output.EntityTypeToCreate == part.Key.KeyName))
							{
								continue;
							}
							if (!output.IsWasteProduct)
							{
								if (output.Amount.NoOfItems.Value > part.Value)
								{
									EntityType.CreateValidationError(ref listOfErrors, "Process output had a valid item type but a higher amount than the salvaged item.");
								}
								flag = true;
								break;
							}
							EntityType.CreateValidationError(ref listOfErrors, $"An output {output.EntityTypeToCreate} was found in the parts list of the salvagable entity, even though it is marked as a waste product.");
						}
						if (!output.IsWasteProduct && !flag)
						{
							EntityType.CreateValidationError(ref listOfErrors, "An output item did not match any of the salvagable entity's parts.");
						}
					}
				}
			}
		}
		if (Outputs != null)
		{
			Output[] outputs = Outputs;
			for (int i = 0; i < outputs.Length; i++)
			{
				outputs[i].PostDataCompleteValidate(ref listOfErrors);
			}
		}
		_ = KeyName == "makeClayPotUnglazed";
	}

	public TierOrAreaType GetTierArea()
	{
		if (TierOrAreaType != null)
		{
			return TierOrAreaType;
		}
		if (Outputs != null)
		{
			Output[] outputs = Outputs;
			foreach (Output output in outputs)
			{
				if (!output.IsWasteProduct && output.FinalEntityTypeToCreate.TierOrAreaType != null)
				{
					return output.FinalEntityTypeToCreate.TierOrAreaType;
				}
			}
		}
		return null;
	}

	private void ValidateToolCanBeReplenishedInOneGo(ref List<string> listOfErrors)
	{
		if (ProcessToolSet == null)
		{
			return;
		}
		foreach (ToolTypeCombination toolTypeCombination in ProcessToolSet.ToolTypeCombinations)
		{
			foreach (Tuple<EntityType, float> tool in toolTypeCombination.Tools)
			{
				if (tool.Item1.ContainerType == null)
				{
					continue;
				}
				RequiresReplenishType requiresReplenishType = tool.Item1.ContainerType.GetRequiresReplenishType();
				if (requiresReplenishType != null)
				{
					if (!worstCaseSkillAndEnergyFactorsRoot.HasValue)
					{
						worstCaseSkillAndEnergyFactorsRoot = (float)Math.Sqrt(GameData.Instance.Constants.LowestCombinedSkillAndEnergyProductionFactors);
					}
					float durationInDays = EstimateTotalDurationInDays(GetTimeNeeded(), worstCaseSkillAndEnergyFactorsRoot.Value, worstCaseSkillAndEnergyFactorsRoot.Value, toolTypeCombination.Productivity);
					float neededFuel = requiresReplenishType.RequiresFuelType.GetNeededFuel(durationInDays);
					if (neededFuel > 1f)
					{
						EntityType.CreateValidationError(ref listOfErrors, $"Fuel required for {tool.Item1}: {neededFuel} is too high. Each tool has to be able to be replenished in one go, so the required fuel bulk must not be higher than 1");
					}
				}
			}
		}
	}

	private void ValidatePartsInProductCorrespondsWithInput(ref List<string> listOfErrors)
	{
		if (Outputs == null)
		{
			return;
		}
		Output[] outputs = Outputs;
		foreach (Output output in outputs)
		{
			if (output.FinalEntityTypeToCreate.Parts == null)
			{
				continue;
			}
			foreach (KeyValuePair<EntityType, int> part in output.FinalEntityTypeToCreate.Parts)
			{
				if (InputsByType.TryGetValue(part.Key, out var value))
				{
					if (value.BecomesPartOfProductType == output.FinalEntityTypeToCreate)
					{
						if (value.IsConsumed)
						{
							EntityType.CreateValidationError(ref listOfErrors, $"The part type {part.Key} does not have a matching input #1.");
						}
						else if (value.Amount.NoOfItems != part.Value)
						{
							EntityType.CreateValidationError(ref listOfErrors, $"The part type {part.Key} does not have enough matching inputs.");
						}
					}
					else
					{
						EntityType.CreateValidationError(ref listOfErrors, $"The part type {part.Key} does not have a matching input #2.");
					}
				}
				else
				{
					EntityType.CreateValidationError(ref listOfErrors, $"The part type {part.Key} does not have a matching input #3.");
				}
			}
		}
	}

	public void PostProcessGraphValidate(List<string> listOfErrors)
	{
	}

	public int NeededMaterials(float currentProgress, float progressChange, float materialStageLength)
	{
		int stage = GetStage(currentProgress, materialStageLength);
		return GetStage(currentProgress + progressChange, materialStageLength) - stage;
	}

	public int? GetOutputAmount(EntityType entityType)
	{
		return Outputs.FirstOrDefault((Output o) => o.FinalEntityTypeToCreate == entityType)?.Amount.NoOfItems;
	}

	public static int GetStage(float currentProgress, float materialStageLength)
	{
		if (Common.IsEqual(currentProgress, 0f))
		{
			return 0;
		}
		if (currentProgress >= 1f)
		{
			return (int)(1f / materialStageLength);
		}
		return (int)(currentProgress / materialStageLength) + 1;
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
