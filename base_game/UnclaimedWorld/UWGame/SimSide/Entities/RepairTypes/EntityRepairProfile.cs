using System.Collections.Generic;
using UWGame.SimSide.Processes;

namespace UWGame.SimSide.Entities.RepairTypes;

public class EntityRepairProfile
{
	public ProcessType Integrity;

	public Dictionary<EntityType, ProcessType> PartsReplacement;

	public Dictionary<EntityType, ProcessType> PartsCondition;

	public ProcessType Condition;

	public void GenerateProcesses(EntityType parent, List<string> errors)
	{
		RepairProfile repairProfile = parent.NonLivingType.RepairProfile;
		if (repairProfile.Integrity != null)
		{
			Integrity = repairProfile.Integrity.GetProcessType(RepairAction.Integrity, parent, null);
			if (repairProfile.Integrity.Repair == RepairProcess.Production && Integrity == null)
			{
				EntityType.CreateValidationError(ref errors, "Integrity repair is set to use the production process, but no production process exists.");
			}
		}
		if (repairProfile.Condition != null)
		{
			Condition = repairProfile.Condition.GetProcessType(RepairAction.Condition, parent, null);
			if (repairProfile.Condition.Repair == RepairProcess.Production && Condition == null)
			{
				EntityType.CreateValidationError(ref errors, "Condition repair is set to use the production process, but no production process exists.");
			}
		}
		if (parent.Parts == null)
		{
			return;
		}
		PartsReplacement = new Dictionary<EntityType, ProcessType>();
		PartsCondition = new Dictionary<EntityType, ProcessType>();
		foreach (KeyValuePair<EntityType, int> part in parent.Parts)
		{
			if (repairProfile.PartsReplacementFinal != null && repairProfile.PartsReplacementFinal.TryGetValue(part.Key, out var value))
			{
				PartsCondition.Add(part.Key, value.GetProcessType(RepairAction.PartsCondition, parent, part.Key));
			}
			else if (repairProfile.DefaultPartsReplacement != null)
			{
				PartsReplacement.Add(part.Key, repairProfile.DefaultPartsReplacement.GetProcessType(RepairAction.ReplacePart, parent, part.Key));
			}
			if (repairProfile.PartsConditionFinal != null && repairProfile.PartsConditionFinal.TryGetValue(part.Key, out value))
			{
				PartsCondition.Add(part.Key, value.GetProcessType(RepairAction.PartsCondition, parent, part.Key));
			}
			else if (repairProfile.DefaultPartsCondition != null)
			{
				PartsCondition.Add(part.Key, repairProfile.DefaultPartsCondition.GetProcessType(RepairAction.PartsCondition, parent, part.Key));
			}
		}
	}
}
