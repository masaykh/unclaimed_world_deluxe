using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Items;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Resources;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Systems.Triggers;

namespace UWGame.SimSide.AI.Goals;

internal class GoalFindPrey : CompositeGoal, ITopLevelGoal
{
	private Vector3 location;

	private FindPreyJob job;

	private JobID? snapshotJob;

	private EntityAndRoot? weapon;

	private Dictionary<EntityAndRoot, List<ReplenishItemsForAction>> replenishItemsForTools;

	private bool hasFoundPrey;

	private Snapshotter.Version version = Snapshotter.Version.Original;

	public double TimeSpentInTopLevelGoal { get; set; }

	public GoalFindPrey(Entity owner, FindPreyJob job, List<EntityGroupID> ownersVehicles, EntityAndRoot? weaponToUse, Dictionary<EntityAndRoot, List<ReplenishItemsForAction>> replenishItemsForTools, Vector3 locationInSearchArea)
		: base(owner)
	{
		this.job = job;
		ownersOfVehicles = ownersVehicles;
		location = locationInSearchArea;
		this.replenishItemsForTools = replenishItemsForTools;
		weapon = weaponToUse;
	}

	public GoalFindPrey()
	{
	}

	protected override void Activate()
	{
		base.Status = Status.Active;
		RemoveAllSubgoals();
		AddSubgoal(new GoalWait(entity, GameData.Instance.AIConstants.TimeToWaitBeforeStartingGoal));
		job.TakeJob(entity);
		SetLocksOnReplenishItems(job, replenishItemsForTools);
		if (!GatherToolsOrWeapons(weapon, ownersOfVehicles, job, StorageCompartment.Haul))
		{
			base.Status = Status.Failed;
			return;
		}
		ReplenishToolsOrWeapons(replenishItemsForTools, ownersOfVehicles, job);
		List<ItemType.TaskType> gearTasks = new List<ItemType.TaskType> { ItemType.TaskType.UnspecifiedHunting };
		CompositeGoal.AddNightActivityGear(ref gearTasks);
		FindOptionalEquipmentIfNeeded(location, job, equipWeapon: false, equipFood: true, mountWeapon: true, gearTasks);
		AddSubgoal(new GoalMoveToPosition(entity, location, ownersOfVehicles));
		AddSubgoal(new GoalSearchArea(entity, job.Zone, ownersOfVehicles, isStealthy: true, examine: false));
	}

	protected override bool ArePreconditionsOK()
	{
		if (!IsToolOrWeaponOK(weapon, out var _))
		{
			return false;
		}
		return true;
	}

	public override string GetStatus()
	{
		return "Locating prey";
	}

	public override DetectionFactor GetDetectAgentsFactor(EntityType typeOfAgent, bool requiresExamineAction)
	{
		if (!requiresExamineAction)
		{
			return DetectionFactor.DetectVeryGood;
		}
		return DetectionFactor.CannotDetect;
	}

	public override DetectionFactor GetDetectResourcesFactor(ResourceType resourceType, bool requiresExamineAction)
	{
		if (!requiresExamineAction)
		{
			return DetectionFactor.DetectSome;
		}
		return DetectionFactor.CannotDetect;
	}

	public override bool IsSame(Job job)
	{
		return job == this.job;
	}

	public override bool CanDropRequestedItem(Entity item)
	{
		if (job != null && item.AssignedToJob == job.ID)
		{
			return false;
		}
		return true;
	}

	protected override void ProcessWhileActive(GameTime elapsed)
	{
		if (!preconditionsRegulator.IsReady() || ArePreconditionsOK())
		{
			base.Status = ProcessSubgoals(elapsed);
		}
		else
		{
			base.Status = Status.Failed;
		}
		if (base.Status == Status.Completed && job != null)
		{
			if (!hasFoundPrey && job.ResolveOwner(out var owner))
			{
				job.Zone.ZoneHunt.RegisterUnsuccessfulHunt(owner);
			}
			DestroyJobAndRemoveLocks(ref job, null, replenishItemsForTools, null, weapon);
		}
	}

	public override bool HandleMessage(Message message)
	{
		if (!ForwardMessageToFrontMostSubgoal(message))
		{
			switch (message.MessageType)
			{
			case Message.MessageTypes.CancelJobOrItemInUse:
			case Message.MessageTypes.CancelJobForAIReset:
				base.Status = Status.Failed;
				job.Abandon(entity);
				return true;
			case Message.MessageTypes.PreyIsNear:
				if (!hasFoundPrey)
				{
					Entity targetEntity = message.Sender;
					if (job.ResolveOwner(out var owner) && targetEntity.CanBeHunted(entityIntelligence.Allegiance) && job.Zone.ZoneHunt.HasUnfulfilledHuntOrders(targetEntity.EntityType, owner) && !owner.OtherJobs.Any((Job j) => j is HuntingJob && ((HuntingJob)j).Target == targetEntity.ID))
					{
						ThreatStance threatStance = ThreatStance.Bold;
						RegionMap regionMap = entityIntelligence.Allegiance.SharedKnowledge.GetMovementMap(entityIntelligence.ProtectionLevel, entity.EntityType, threatStance).Layers[SurfaceType.TransportType.Foot].RegionMap;
						double travelTimeScore = 0.0;
						if (GoalEvaluator.ScoreTravelTime(regionMap, threatStance, entity.AccessPoint.Value, targetEntity.Location.Value, entity, ref travelTimeScore, job) == RegionMap.Result.NoAccess)
						{
							return false;
						}
						if (message.OtherInfo != null)
						{
							Trigger trigger = (Trigger)message.OtherInfo;
							entityIntelligence.SetTriggerCooldown(trigger, trigger.TriggerType.CooldownInTicks);
						}
						entityIntelligence.Memory.SetRecentlyFoundPrey(targetEntity.EntityID);
						base.Status = Status.Completed;
						RemoveAllSubgoals();
						Zone zone = job.Zone;
						job.Zone.ZoneHunt.NotifyHasFoundPrey(targetEntity.EntityType);
						DestroyJobAndRemoveLocks(ref job, null, replenishItemsForTools, null, weapon);
						hasFoundPrey = true;
						Expedition currentExpedition = entityIntelligence.CurrentExpedition;
						new HuntingJob(targetEntity.EntityID, targetEntity.EntityType, currentExpedition.OwnedEntities, zone.ID);
					}
				}
				return true;
			default:
				return false;
			}
		}
		return true;
	}

	public override void Deactivate()
	{
		if (job != null)
		{
			RemoveLocksFromJob(job, null, replenishItemsForTools, null, weapon);
		}
		if (!hasFoundPrey)
		{
			entity.SetSneaking(value: false);
		}
	}

	public double ScoreGoal()
	{
		return ScoreJobGoal(job);
	}

	private bool EntityCanBeMarkedAsHuntTarget(Entity entity)
	{
		if (entity != null && entity.EntityType.Person == null && entity.Intelligence != null && entity.Intelligence.Allegiance != null && entity.Intelligence.Allegiance.AllegianceType != AllegianceType.Player)
		{
			return true;
		}
		return false;
	}

	public override Snapshotter.Version DoVersion(Snapshotter sn)
	{
		base.DoVersion(sn);
		version = sn.DoVersion(Snapshotter.Version.Original);
		return version;
	}

	public override ISnapshot DoSnapshot(Snapshotter sn)
	{
		base.DoSnapshot(sn);
		location = sn.DoVector3(location);
		snapshotJob = sn.SnapshotID<Job, JobID>(job);
		hasFoundPrey = sn.DoBool(hasFoundPrey);
		weapon = sn.DoEntityAndRootNullable(weapon);
		replenishItemsForTools = sn.DoMultiMap(replenishItemsForTools);
		TimeSpentInTopLevelGoal = sn.DoDouble(TimeSpentInTopLevelGoal);
		sn.Ignore(job);
		return this;
	}

	public override void LoadPostProcess(Snapshotter sn)
	{
		sn.RegisterLoadPostProcessCall(this);
		base.LoadPostProcess(sn);
		if (snapshotJob.HasValue)
		{
			job = (FindPreyJob)LookUp<Job, JobID>.FindByID(snapshotJob);
		}
	}
}
