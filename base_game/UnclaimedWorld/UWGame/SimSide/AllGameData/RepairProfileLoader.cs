using System.Collections.Generic;
using UWGame.SimSide.Entities.RepairTypes;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.AllGameData;

public class RepairProfileLoader
{
	public static List<RepairProfile> Init()
	{
		return new List<RepairProfile>
		{
			new RepairProfile
			{
				Comments = "not used yet",
				KeyName = "improvisedEquipment",
				Integrity = new RepairType
				{
					Repair = RepairProcess.Production
				},
				DefaultPartsReplacement = new RepairType
				{
					Repair = RepairProcess.Production
				},
				DefaultPartsCondition = new RepairType
				{
					Repair = RepairProcess.Production
				}
			},
			new RepairProfile
			{
				Comments = "Survival tier building parts can be reconditioned without tools",
				KeyName = "buildingRepair",
				Integrity = new RepairType
				{
					Repair = RepairProcess.Production
				},
				DefaultPartsReplacement = new RepairType
				{
					Repair = RepairProcess.Production
				},
				DefaultPartsCondition = new RepairType
				{
					Repair = RepairProcess.Production
				},
				PartsCondition = new SerializableDictionary<string, RepairType>
				{
					{
						"item:spoakBranchesTrimmed",
						new RepairType
						{
							Repair = RepairProcess.Custom,
							ProcessType = "reconditionSpokBranchesTrimmedPart"
						}
					},
					{
						"item:spoakLeaves",
						new RepairType
						{
							Repair = RepairProcess.Custom,
							ProcessType = "reconditionSpoakLeaves"
						}
					},
					{
						"item:daysheenLeaves",
						new RepairType
						{
							Repair = RepairProcess.Custom,
							ProcessType = "reconditionDaysheenLeaves"
						}
					},
					{
						"item:sticks",
						new RepairType
						{
							Repair = RepairProcess.Custom,
							ProcessType = "reconditionSticks"
						}
					}
				}
			},
			new RepairProfile
			{
				Comments = "For mining pits and the like (NOT peat bank). Used for entities that have no production process. Uses digging tools to repair integrity. Survival tier building parts can be reconditioned without tools",
				KeyName = "diggingRepairCustomProcess",
				Integrity = new RepairType
				{
					Repair = RepairProcess.Custom,
					ProcessType = "repairPrimitiveIntegrityWithDigging"
				},
				DefaultPartsReplacement = new RepairType
				{
					Repair = RepairProcess.None
				},
				DefaultPartsCondition = new RepairType
				{
					Repair = RepairProcess.Custom,
					ProcessType = "repairPrimitiveCondition"
				},
				PartsCondition = new SerializableDictionary<string, RepairType>
				{
					{
						"item:spoakBranchesTrimmed",
						new RepairType
						{
							Repair = RepairProcess.Custom,
							ProcessType = "reconditionSpokBranchesTrimmedPart"
						}
					},
					{
						"item:spoakLeaves",
						new RepairType
						{
							Repair = RepairProcess.Custom,
							ProcessType = "reconditionSpoakLeaves"
						}
					},
					{
						"item:daysheenLeaves",
						new RepairType
						{
							Repair = RepairProcess.Custom,
							ProcessType = "reconditionDaysheenLeaves"
						}
					},
					{
						"item:sticks",
						new RepairType
						{
							Repair = RepairProcess.Custom,
							ProcessType = "reconditionSticks"
						}
					}
				}
			},
			new RepairProfile
			{
				Comments = "For peat bank. Needs only plowing tools (like farm plot) Used for entities that have no production process. Uses digging tools to repair integrity. Survival tier building parts can be reconditioned without tools",
				KeyName = "plowingRepairCustomProcess",
				Integrity = new RepairType
				{
					Repair = RepairProcess.Custom,
					ProcessType = "repairPrimitiveIntegrityWithPlowing"
				},
				DefaultPartsReplacement = new RepairType
				{
					Repair = RepairProcess.None
				},
				DefaultPartsCondition = new RepairType
				{
					Repair = RepairProcess.Custom,
					ProcessType = "repairPrimitiveCondition"
				},
				PartsCondition = new SerializableDictionary<string, RepairType>
				{
					{
						"item:spoakBranchesTrimmed",
						new RepairType
						{
							Repair = RepairProcess.Custom,
							ProcessType = "reconditionSpokBranchesTrimmedPart"
						}
					},
					{
						"item:spoakLeaves",
						new RepairType
						{
							Repair = RepairProcess.Custom,
							ProcessType = "reconditionSpoakLeaves"
						}
					},
					{
						"item:daysheenLeaves",
						new RepairType
						{
							Repair = RepairProcess.Custom,
							ProcessType = "reconditionDaysheenLeaves"
						}
					},
					{
						"item:sticks",
						new RepairType
						{
							Repair = RepairProcess.Custom,
							ProcessType = "reconditionSticks"
						}
					}
				}
			},
			new RepairProfile
			{
				Comments = "Used for the entities that have no production process. Survival tier building parts can be reconditioned without tools",
				KeyName = "buildingRepairCustomProcess",
				Integrity = new RepairType
				{
					Repair = RepairProcess.Custom,
					ProcessType = "repairPrimitiveIntegrity"
				},
				DefaultPartsReplacement = new RepairType
				{
					Repair = RepairProcess.None
				},
				DefaultPartsCondition = new RepairType
				{
					Repair = RepairProcess.Custom,
					ProcessType = "repairPrimitiveCondition"
				},
				PartsCondition = new SerializableDictionary<string, RepairType>
				{
					{
						"item:spoakBranchesTrimmed",
						new RepairType
						{
							Repair = RepairProcess.Custom,
							ProcessType = "reconditionSpokBranchesTrimmedPart"
						}
					},
					{
						"item:spoakLeaves",
						new RepairType
						{
							Repair = RepairProcess.Custom,
							ProcessType = "reconditionSpoakLeaves"
						}
					},
					{
						"item:daysheenLeaves",
						new RepairType
						{
							Repair = RepairProcess.Custom,
							ProcessType = "reconditionDaysheenLeaves"
						}
					},
					{
						"item:sticks",
						new RepairType
						{
							Repair = RepairProcess.Custom,
							ProcessType = "reconditionSticks"
						}
					}
				}
			},
			new RepairProfile
			{
				Comments = "for repairing poup tents",
				KeyName = "tentRepair",
				Integrity = new RepairType
				{
					Repair = RepairProcess.Production,
					ProductionTimeFactor = 1f
				},
				DefaultPartsReplacement = new RepairType
				{
					Repair = RepairProcess.Production
				},
				DefaultPartsCondition = new RepairType
				{
					Repair = RepairProcess.Production,
					ProductionTimeFactor = 1.5f
				}
			}
		};
	}
}
