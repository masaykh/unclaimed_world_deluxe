using System.Collections.Generic;
using System.Xml.Serialization;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Processes;

namespace UWGame.SimSide.Entities;

public class RequiresReplenishType
{
	public string ReplenishProcess;

	[XmlIgnore]
	private ProcessType ReplenishProcessType;

	[XmlIgnore]
	public Dictionary<EntityType, ProcessType> ReplenishProcesses;

	public RequiresFuelType RequiresFuelType;

	public void PostLoadContentInitialize(EntityType parent)
	{
		if (RequiresFuelType != null)
		{
			RequiresFuelType.PostLoadContentInitialize();
		}
		if (string.IsNullOrEmpty(ReplenishProcess))
		{
			return;
		}
		ReplenishProcessType = GameData.Instance.AllProcessTypes[ReplenishProcess];
		ReplenishProcessType.IsReplenishProcess = true;
		ReplenishProcessType.ReplenishAction = GoalReplenish.ReplenishAction.Refuel;
		foreach (EntityType fuelEntityType in RequiresFuelType.FuelEntityTypes)
		{
			InitReplenishProcess(parent, ReplenishProcessType, fuelEntityType, ref ReplenishProcesses);
		}
	}

	public static void InitReplenishProcess(EntityType parent, ProcessType baseProcess, EntityType item, ref Dictionary<EntityType, ProcessType> ReplenishProcesses)
	{
		ProcessType processType = new ProcessType();
		processType.KeyName = parent.KeyName + "_replenish_" + item.KeyName;
		processType.Inputs = new Input[1]
		{
			new Input
			{
				Entity = item.KeyName,
				IsConsumed = false,
				Amount = new InputAmount
				{
					NoOfItems = 1
				}
			}
		};
		processType.Name = baseProcess.Name;
		processType.WorkOrTimeNeeded = baseProcess.WorkOrTimeNeeded;
		processType.AgentActionState = baseProcess.AgentActionState;
		processType.ReplenishAction = baseProcess.ReplenishAction;
		processType.IsReplenishProcess = true;
		processType.AgentAnimationStates = baseProcess.AgentAnimationStates;
		processType.UseWorkerEnergyAsProductionFactor = false;
		processType.Stances = baseProcess.Stances;
		processType.JobTypeKey = baseProcess.JobTypeKey;
		GameData.Instance.AllProcessTypes.Add(processType.KeyName, processType);
		GameData.InitializeComputerGeneratedData(processType);
		processType.PostLoadContentInitialize();
		Common.AddToDictionary(ref ReplenishProcesses, item, processType);
	}
}
