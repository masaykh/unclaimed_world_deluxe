using System.Collections.Generic;
using System.Linq;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Maps;

public class ZoneHunt : ISnapshot
{
	public bool RemoveAfterFirstSuccessfulHunt;

	public Dictionary<EntityType, int> CreaturesToHunt = new Dictionary<EntityType, int>();

	public List<FindPreyJob> FindPreyJobs = new List<FindPreyJob>();

	private List<JobID> snapshotFindPreyJobs;

	public Dictionary<EntityType, double> TimePointForNextHunt = new Dictionary<EntityType, double>();

	public Dictionary<EntityType, double> NextHuntInterval = new Dictionary<EntityType, double>();

	public int MaxHuntJobs = 1;

	public HashSet<EntityType> AllowStandingOrderHunt = new HashSet<EntityType>();

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public bool IsSnapshotted { get; set; }

	public bool HasFindPreyJobs()
	{
		if (FindPreyJobs != null && FindPreyJobs.Count > 0)
		{
			return true;
		}
		return false;
	}

	public bool HasDirectOrders()
	{
		return CreaturesToHunt.Any((KeyValuePair<EntityType, int> kvp) => kvp.Value > 0);
	}

	public bool AllowsStandingOrders()
	{
		return AllowStandingOrderHunt.Count > 0;
	}

	public bool HasHuntOrders()
	{
		if (!HasDirectOrders())
		{
			return AllowsStandingOrders();
		}
		return true;
	}

	public bool HasAnyHuntOrders(EntityType creatureType)
	{
		if (CreaturesToHunt.TryGetValue(creatureType, out var value) && value > 0)
		{
			return true;
		}
		if (AllowStandingOrderHunt.Contains(creatureType))
		{
			return true;
		}
		return false;
	}

	public bool HasUnfulfilledHuntOrders(EntityType creatureType, EntityGroup owner)
	{
		if (CreaturesToHunt.TryGetValue(creatureType, out var value) && value > 0)
		{
			return true;
		}
		if (owner.ProductionOrders != null && AllowStandingOrderHunt.Contains(creatureType) && owner.StandingOrderJobIsNeeded(creatureType.BiologicalType.CarcassType))
		{
			return true;
		}
		return false;
	}

	public void CancelJobs()
	{
		for (int num = FindPreyJobs.Count - 1; num >= 0; num--)
		{
			FindPreyJobs[num].Destroy(removeTakers: true);
		}
	}

	public void RegisterUnsuccessfulHunt(EntityGroup owner)
	{
		HashSet<EntityType> creaturesWithHuntOrders = GetCreaturesWithHuntOrders(owner);
		if (creaturesWithHuntOrders == null)
		{
			return;
		}
		foreach (EntityType item in creaturesWithHuntOrders)
		{
			double value;
			double num = ((!NextHuntInterval.TryGetValue(item, out value)) ? GameData.Instance.AIConstants.CooldownTimeAfterUnsuccessfulHunt : Common.ClampTop(GameData.Instance.AIConstants.IncreaseCooldownTimeFactorAfterUnsuccessfulHunt * value, GameData.Instance.AIConstants.MaxCooldownTimeAfterUnsuccessfulHunt));
			SetTimePointForNextHunt(item, num);
			NextHuntInterval[item] = num;
		}
	}

	private void SetTimePointForNextHunt(EntityType item, double interval)
	{
		double value = The.Sim.TotalUnPausedGameTimeInSeconds + interval;
		TimePointForNextHunt[item] = value;
	}

	public void NotifyHasFoundPrey(EntityType creature)
	{
		SetTimePointForNextHunt(creature, GameData.Instance.AIConstants.CooldownTimeAfterSpottingPrey);
	}

	public void NotifySuccessfulHunt(EntityType creature, Zone zone)
	{
		NextHuntInterval.Remove(creature);
		ResetTimepoint(creature);
		Common.RemoveFromDictWithSums(CreaturesToHunt, creature);
		if (RemoveAfterFirstSuccessfulHunt)
		{
			zone.Destroy();
		}
		else
		{
			zone.RemoveZoneOrFireOrdersChangedEvent();
		}
	}

	private HashSet<EntityType> GetCreaturesWithHuntOrders(EntityGroup owner)
	{
		HashSet<EntityType> set = null;
		foreach (EntityType preyType in owner.GetAllegiance().RepresentativeEntityType.IntelligenceType.PreyTypes)
		{
			if (HasUnfulfilledHuntOrders(preyType, owner))
			{
				Common.AddToSet(ref set, preyType);
			}
		}
		return set;
	}

	public bool HuntIsOnCooldown(EntityType creatureType)
	{
		return TimePointForNextHunt.ContainsKey(creatureType);
	}

	public bool HasOrdersNotOnCooldown()
	{
		if (HasHuntOrders())
		{
			foreach (KeyValuePair<EntityType, int> item in CreaturesToHunt)
			{
				if (item.Value > 0 && !HuntIsOnCooldown(item.Key))
				{
					return true;
				}
			}
			foreach (EntityType item2 in AllowStandingOrderHunt)
			{
				if (!HuntIsOnCooldown(item2))
				{
					return true;
				}
			}
			return false;
		}
		return false;
	}

	public void ResetTimepoint(EntityType creatureType)
	{
		TimePointForNextHunt.Remove(creatureType);
	}

	public void Destroy()
	{
		if (FindPreyJobs != null)
		{
			for (int num = FindPreyJobs.Count - 1; num >= 0; num--)
			{
				FindPreyJobs[num].Destroy(removeTakers: true);
			}
		}
	}

	public Snapshotter.Version DoVersion(Snapshotter sn)
	{
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public ISnapshot DoSnapshot(Snapshotter sn)
	{
		AllowStandingOrderHunt = sn.DoHashSet(AllowStandingOrderHunt);
		CreaturesToHunt = sn.DoDictionary(CreaturesToHunt);
		MaxHuntJobs = sn.DoInt32(MaxHuntJobs);
		RemoveAfterFirstSuccessfulHunt = sn.DoBool(RemoveAfterFirstSuccessfulHunt);
		TimePointForNextHunt = sn.DoDictionary(TimePointForNextHunt);
		NextHuntInterval = sn.DoDictionary(NextHuntInterval);
		if (sn.mode != Snapshotter.Mode.Load && FindPreyJobs != null)
		{
			snapshotFindPreyJobs = new List<JobID>();
			foreach (FindPreyJob findPreyJob in FindPreyJobs)
			{
				snapshotFindPreyJobs.Add(findPreyJob.ID);
			}
		}
		snapshotFindPreyJobs = sn.DoList(snapshotFindPreyJobs);
		sn.Ignore(FindPreyJobs);
		return this;
	}

	public void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		if (snapshotFindPreyJobs == null)
		{
			return;
		}
		FindPreyJobs = new List<FindPreyJob>();
		foreach (JobID snapshotFindPreyJob in snapshotFindPreyJobs)
		{
			FindPreyJobs.Add((FindPreyJob)LookUp<Job, JobID>.FindByID(snapshotFindPreyJob));
		}
	}
}
