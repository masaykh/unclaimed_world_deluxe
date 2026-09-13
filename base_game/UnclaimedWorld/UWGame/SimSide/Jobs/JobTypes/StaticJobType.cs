using System;
using System.Collections.Generic;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.Jobs.JobTypes;

public class StaticJobType : JobType
{
	public StaticJobTypes StaticJobTypeSetting;

	public override void IterateJobs(EntityGroup entityGroup, Action<Job> iterateFunction)
	{
		switch (StaticJobTypeSetting)
		{
		case StaticJobTypes.Patrol:
		{
			foreach (Job patrolJob in entityGroup.PatrolJobs)
			{
				iterateFunction(patrolJob);
			}
			break;
		}
		case StaticJobTypes.AttackArea:
		{
			foreach (Job attackAreaJob in entityGroup.AttackAreaJobs)
			{
				iterateFunction(attackAreaJob);
			}
			break;
		}
		case StaticJobTypes.Examine:
		case StaticJobTypes.Scout:
		{
			foreach (Job scoutingJob in entityGroup.ScoutingJobs)
			{
				if (IsType(scoutingJob))
				{
					iterateFunction(scoutingJob);
				}
			}
			break;
		}
		case StaticJobTypes.Hunt:
			foreach (Job findPreyJob in entityGroup.FindPreyJobs)
			{
				iterateFunction(findPreyJob);
			}
			{
				foreach (Job otherJob in entityGroup.OtherJobs)
				{
					if (IsType(otherJob))
					{
						iterateFunction(otherJob);
					}
				}
				break;
			}
		case StaticJobTypes.Upgrading:
		{
			foreach (Job otherJob2 in entityGroup.OtherJobs)
			{
				if (IsType(otherJob2))
				{
					iterateFunction(otherJob2);
				}
			}
			break;
		}
		case StaticJobTypes.Repairing:
		{
			foreach (KeyValuePair<EntityID, List<ProcessJob>> repairJob in entityGroup.RepairJobs)
			{
				foreach (ProcessJob item in repairJob.Value)
				{
					if (IsType(item))
					{
						iterateFunction(item);
					}
				}
			}
			break;
		}
		case StaticJobTypes.HaulToStorage:
		{
			foreach (Job haulingJob in entityGroup.HaulingJobs)
			{
				if (IsType(haulingJob))
				{
					iterateFunction(haulingJob);
				}
			}
			break;
		}
		case StaticJobTypes.Salvaging:
		{
			foreach (Job otherJob3 in entityGroup.OtherJobs)
			{
				if (IsType(otherJob3))
				{
					iterateFunction(otherJob3);
				}
			}
			break;
		}
		}
	}

	public override bool IsType(Job job)
	{
		switch (StaticJobTypeSetting)
		{
		case StaticJobTypes.Scout:
			if (job is ScoutingJob scoutingJob2)
			{
				return !scoutingJob2.Examine;
			}
			return false;
		case StaticJobTypes.Examine:
			if (job is ScoutingJob scoutingJob)
			{
				return scoutingJob.Examine;
			}
			return false;
		case StaticJobTypes.Patrol:
			return job is PatrolJob;
		case StaticJobTypes.AttackArea:
			return job is AttackAreaJob;
		case StaticJobTypes.HaulToStorage:
			if (job is HaulingJobSpecificItem { IsHaulJobToStorage: not false })
			{
				return true;
			}
			break;
		case StaticJobTypes.Hunt:
			if (!(job is FindPreyJob))
			{
				return job is HuntingJob;
			}
			return true;
		case StaticJobTypes.Repairing:
		case StaticJobTypes.Salvaging:
		case StaticJobTypes.Upgrading:
			if (job is ProcessJob processJob)
			{
				if (StaticJobTypeSetting == StaticJobTypes.Salvaging && processJob.SalvageJob != null)
				{
					return true;
				}
				if (StaticJobTypeSetting == StaticJobTypes.Repairing && processJob.RepairJob != null)
				{
					return true;
				}
				if (StaticJobTypeSetting == StaticJobTypes.Upgrading && processJob.GetIsUpgradeJob())
				{
					return true;
				}
			}
			return false;
		}
		return false;
	}

	public override string GetDefaultDisplayName()
	{
		return StaticJobTypeSetting switch
		{
			StaticJobTypes.Patrol => "Patrolling", 
			StaticJobTypes.AttackArea => "Attacking", 
			StaticJobTypes.Hunt => "Hunting", 
			StaticJobTypes.Examine => "Examining", 
			StaticJobTypes.Scout => "Scouting", 
			StaticJobTypes.HaulToStorage => "Hauling to storage", 
			StaticJobTypes.Repairing => "Doing maintenance", 
			StaticJobTypes.Salvaging => "Salvaging", 
			StaticJobTypes.Upgrading => "Upgrading", 
			_ => "No display string for:" + StaticJobTypeSetting, 
		};
	}
}
