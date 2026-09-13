using System.Collections.Generic;
using System.Linq;
using UWGame.SimSide.Processes;

namespace UWGame.SimSide.Entities.RepairTypes;

public class RepairType
{
	public RepairProcess Repair;

	public string ProcessType;

	public float? ProductionTimeFactor;

	public const string integrityProcessName = "Repairing integrity";

	public const string integrityProcessSummary = "Fixing the integrity of the structure so it does not fall apart";

	public const string partsConditionProcessName = "Reconditioning";

	public const string partsConditionProcessSummary = "Repairing a '{0}' part to improve its condition.";

	public const string partsConditionProcessSummaryNoPlaceholder = "Repairing a part to improve its condition.";

	public void PreDataCompleteValidate(ref List<string> listOfErrors)
	{
		if (Repair == RepairProcess.Custom)
		{
			if (ProcessType == null)
			{
				EntityType.CreateValidationError(ref listOfErrors, "Process is required when Custom is defined");
			}
		}
		else if (ProcessType != null)
		{
			EntityType.CreateValidationError(ref listOfErrors, "Process should not be defined unless Custom is defined");
		}
	}

	public void PostDataCompleteInitialize()
	{
	}

	public ProcessType GetProcessType(RepairAction repairAction, EntityType parent, EntityType part)
	{
		if (Repair == RepairProcess.Production)
		{
			if (GameData.Instance.ProcessYieldsThisOutput.TryGetValue(parent, out var value))
			{
				return CreateRepairProcess(repairAction, parent, value[0], part);
			}
			return null;
		}
		if (Repair == RepairProcess.Custom && ProcessType != null)
		{
			return GameData.Instance.AllProcessTypes[ProcessType];
		}
		return null;
	}

	private ProcessType CreateRepairProcess(RepairAction repairAction, EntityType parent, ProcessType productionProcess, EntityType part)
	{
		string text = "";
		string summaryDescription = "";
		string name = "Doing maintenance";
		string newKeyname = "";
		switch (repairAction)
		{
		case RepairAction.Integrity:
			text = "integrity";
			name = "Repairing integrity";
			summaryDescription = "Fixing the integrity of the structure so it does not fall apart";
			newKeyname = parent.KeyName + "_" + text;
			break;
		case RepairAction.RemovePart:
			text = "removePart";
			newKeyname = parent.KeyName + "_" + text + "_" + part.KeyName;
			break;
		case RepairAction.AddPart:
			text = "addPart";
			newKeyname = parent.KeyName + "_" + text + "_" + part.KeyName;
			break;
		case RepairAction.ReplacePart:
			text = "replacePart";
			newKeyname = parent.KeyName + "_" + text + "_" + part.KeyName;
			break;
		case RepairAction.PartsCondition:
			text = "reconditionPart";
			name = "Reconditioning";
			summaryDescription = $"Repairing a '{part.Name.ToLower(Config.Culture)}' part to improve its condition.";
			newKeyname = parent.KeyName + "_" + text + "_" + part.KeyName;
			break;
		case RepairAction.Condition:
			text = "recondition";
			newKeyname = parent.KeyName + "_" + text;
			break;
		}
		ProcessType processType = new ProcessType(productionProcess, newKeyname, null);
		float value;
		if (ProductionTimeFactor.HasValue)
		{
			value = productionProcess.GetTimeNeeded() * ProductionTimeFactor.Value;
		}
		else
		{
			value = productionProcess.GetTimeNeeded();
			if (repairAction == RepairAction.Integrity)
			{
				value /= 3f;
			}
			else
			{
				int num = 2;
				if (productionProcess.Inputs != null)
				{
					num += productionProcess.Inputs.Sum((Input i) => i.Amount.NoOfItems ?? 0);
				}
				value /= (float)num;
			}
		}
		processType.WorkOrTimeNeeded = new WorkOrTime
		{
			DaysNeeded = value
		};
		processType.Name = name;
		processType.SummaryDescription = summaryDescription;
		processType.RepairAction = repairAction;
		processType.JobTypeKey = null;
		if (IsReplaceAction(repairAction))
		{
			processType.Inputs = new Input[1]
			{
				new Input
				{
					IsConsumed = true,
					Amount = new InputAmount
					{
						NoOfItems = 1
					},
					Entity = part.KeyName
				}
			};
		}
		else
		{
			processType.Inputs = null;
			processType.Outputs = null;
		}
		processType.UseOriginalProcessEvents = false;
		processType.InitDynamicProcess();
		return processType;
	}

	public static bool IsReplaceAction(RepairAction action)
	{
		if (action == RepairAction.ReplacePart || action == RepairAction.RemovePart || action == RepairAction.AddPart)
		{
			return true;
		}
		return false;
	}
}
