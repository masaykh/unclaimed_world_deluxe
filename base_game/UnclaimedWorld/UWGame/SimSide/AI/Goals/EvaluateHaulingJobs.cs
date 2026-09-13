using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using UWGame.SimSide.AI.Constants;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Biological;
using UWGame.SimSide.Entities.Containers.Components;
using UWGame.SimSide.Items;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Goals;

public class EvaluateHaulingJobs : GoalEvaluator, IScoreJob
{
	private enum Progress
	{
		NotStarted,
		GetCombos,
		ScoreCombos
	}

	private HaulingJob mostDesirableJob;

	private EntityID? bestItem;

	private EntityID? bestVehicle;

	private List<Entity> needsToBeCancelled = new List<Entity>();

	private List<Entity> itemsToBeDropped = new List<Entity>();

	private List<HaulingCombo> haulingCombos = new List<HaulingCombo>();

	private static double oneOverMaxHaulingTime;

	private EntityGroup ownerOfJobs;

	private Progress progress;

	private double ageContribution = 1.0;

	private double timeOfDayContribution;

	private int haulingComboProgress;

	private bool scoringWasInterrupted;

	public List<EntityGroup> OwnersOfVehicles;

	private float priority;

	public override float Priority => priority;

	static EvaluateHaulingJobs()
	{
		oneOverMaxHaulingTime = 1f / (48f * GoalEvaluator.maxMapOctileDistance / 8f);
	}

	public EvaluateHaulingJobs(Entity entity, EntityGroup ownerOfJobs, EntityGroup newOwner, List<EntityGroup> ownersOfVehicles)
		: base(entity)
	{
		OwnersOfVehicles = ownersOfVehicles;
		this.ownerOfJobs = ownerOfJobs;
		priority = GameData.Instance.AIConstants.PriorityOfHauling;
	}

	private static double ScoreNeededForTrade(IKnownEntityData item, HaulingJob haulingJob)
	{
		if (haulingJob.IsToTradeOfferStorage)
		{
			return 1.0;
		}
		return 0.0;
	}

	public static double ScoreJobMaterialUrgency(HaulingJob haulingJob, EntityGroup owner)
	{
		if (haulingJob.RequiredByProcessJob != null)
		{
			return haulingJob.RequiredByProcessJob.GetImportance(owner);
		}
		return 0.0;
	}

	public CalculateResult ScoreThisJob(RegionMap regionMap, ThreatStance threatStance, Entity entity, Job job, int proposedNumberOfWorkers, double? ageContribution, double? timeContribution, out double rating, ToolParams? toolParams, AttackParams? attackParams, HaulingParams? haulingParams = null)
	{
		IKnownEntityData itemToHaul = null;
		_ = haulingParams.Value.Item;
		_ = 38;
		_ = haulingParams.Value.Item;
		_ = 39;
		if (!CanTakeStanceForJob(job, regionMap, threatStance, out var regionMapToUse, out var _))
		{
			rating = 0.0;
			return CalculateResult.Done;
		}
		HaulingJob haulingJob = job as HaulingJob;
		if (haulingJob.IsCompleted)
		{
			rating = 0.0;
			return CalculateResult.Done;
		}
		if (!ageContribution.HasValue)
		{
			ageContribution = GetAgeContribution();
		}
		if (!timeContribution.HasValue)
		{
			timeContribution = ScoreTimeOfDay();
		}
		double haulingTime = 0.0;
		rating = 0.0;
		if (haulingParams.Value.Vehicle != null)
		{
			if (EstimateHaulingTimeByVehicle(entity, haulingParams.Value.Item, haulingParams.Value.Vehicle, haulingJob.GetToStorageEntity, haulingJob.ToLocation, regionMapToUse, ref haulingTime) == CalculateResult.Processing)
			{
				return CalculateResult.Processing;
			}
		}
		else if (EstimateHaulingTimeByFoot(entity, haulingParams.Value.Item, haulingJob, regionMapToUse, ref haulingTime, out itemToHaul) == CalculateResult.Processing)
		{
			return CalculateResult.Processing;
		}
		if (haulingTime < 0.0)
		{
			rating = 0.0;
		}
		else
		{
			double num = 0.0;
			double num2 = ScoreJobMaterialUrgency(haulingJob, ownerOfJobs);
			num = ((!Common.IsZero(num2)) ? num2 : ScoreNeededForTrade(itemToHaul, haulingJob));
			double num3 = 0.0;
			double num4 = itemToHaul.EntityType.ItemType.GetHauledItemValueModifier();
			double num5 = GoalEvaluator.ScoreTravelTime(haulingTime, oneOverMaxHaulingTime);
			double num6 = ScoreRecentlyHauledItem(itemToHaul);
			double num7 = ScoreTimePassed(haulingJob);
			EvaluatorWeights evaluatorWeights = GameData.Instance.AIConstants.EvaluatorWeights;
			if (haulingParams.Value.Vehicle == null)
			{
				rating = num5 * evaluatorWeights.HaulJobTravelWeight + num * evaluatorWeights.HaulJobUrgencyWeight + num6 * evaluatorWeights.HaulJobMemoryWeight + 0.1 * num3 + num7 * evaluatorWeights.HaulJobStarvationWeight + evaluatorWeights.HaulJobAddend + 0.05000000074505806 * num4;
			}
			else
			{
				double num8 = GoalEvaluator.ScoreIsEntityFunctional(haulingParams.Value.Vehicle);
				rating = num5 * evaluatorWeights.HaulJobTravelWeight + num * evaluatorWeights.HaulJobUrgencyWeight + num6 * evaluatorWeights.HaulJobMemoryWeight + num7 * evaluatorWeights.HaulJobStarvationWeight + 0.15 * num8 + 0.1 * num3 + 0.05000000074505806 * num4;
			}
			double recentlyHuntedCarcassScore = entity.Intelligence.Memory.GetRecentlyHuntedCarcassScore(haulingParams.Value.Item);
			if (recentlyHuntedCarcassScore != 0.0)
			{
				rating = rating * (double)(1f - GameData.Instance.AIConstants.RecentlyKilledCarcassScoreFraction) + recentlyHuntedCarcassScore * (double)GameData.Instance.AIConstants.RecentlyKilledCarcassScoreFraction;
			}
			rating = GoalEvaluator.AddTimeAgeAndPriority(rating, timeContribution.Value, ageContribution.Value, Priority);
			if (haulingJob.RequiredByProcessJob != null)
			{
				EvaluateJob.ApplyJobPriorityModifier(haulingJob.RequiredByProcessJob.Priority, ref rating);
			}
			else if (haulingJob is HaulingJobSpecificItem { IsHaulJobToStorage: not false } haulingJobSpecificItem)
			{
				EvaluateJob.ApplyJobPriorityModifier(haulingJobSpecificItem.Priority, ref rating);
			}
		}
		EvaluateJob.SetDebugScore(entity, haulingJob, rating);
		return CalculateResult.Done;
	}

	private double ScoreRecentlyHauledItem(IKnownEntityData entityData)
	{
		return entityIntelligence.Memory.GetRecentlyHauledItemScore(entityData.EntityID);
	}

	private double ScoreTimePassed(HaulingJob job)
	{
		return job.ScoreTimePassed();
	}

	private bool IsCurrentlyHaulingItem(IKnownEntityData entityData)
	{
		if (entityData.AssignedToJob.HasValue)
		{
			Job job = LookUp<Job, JobID>.FindByID(entityData.AssignedToJob.Value);
			if (job != null && job is HaulingJob haulingJob && haulingJob.TakenBy.Contains(entity))
			{
				return true;
			}
		}
		return false;
	}

	public override CalculateResult CalculateDesirability(double minimumRatingToConsider, ref double desirability)
	{
		desirability = 0.0;
		if (progress == Progress.NotStarted)
		{
			if (entity.Find<BiologicalEntity>(out var c))
			{
				ageContribution = ScoreAge(c);
				if (ageContribution == 0.0)
				{
					return CalculateResult.Done;
				}
			}
			timeOfDayContribution = ScoreTimeOfDay();
			progress = Progress.GetCombos;
		}
		if (progress == Progress.GetCombos)
		{
			mostDesirableJob = null;
			bestScore = 0.0;
			bestItem = null;
			if (ownerOfJobs.HaulingJobs.Count > 0)
			{
				needsToBeCancelled.Clear();
				haulingCombos.Clear();
				haulingComboProgress = 0;
				scoringWasInterrupted = false;
				GoalEvaluator.GetRegionMapAndStanceForEvaluator(entity, null, out var threatStanceToUse);
				for (int num = ownerOfJobs.HaulingJobs.Count - 1; num >= 0; num--)
				{
					Job job = ownerOfJobs.HaulingJobs[num];
					if (!entityIntelligence.Brain.IsSame(job) && job is HaulingJob && CanTakeStanceForJob(job, threatStanceToUse, out var threatStanceToUse2))
					{
						if (job is HaulingJobAnyItemOfType haulingJobAnyItemOfType)
						{
							EntityGroup entityGroup = LookUp<EntityGroup, EntityGroupID>.FindByID(haulingJobAnyItemOfType.ItemsToHaulGroup);
							if (entityGroup == null)
							{
								haulingJobAnyItemOfType.Destroy(cancelTakers: true);
							}
							else
							{
								GetAllHaulingCombosForItemType(haulingJobAnyItemOfType, haulingJobAnyItemOfType.RequiredItemType, entityGroup, threatStanceToUse2);
							}
						}
						else
						{
							HaulingJobSpecificItem haulingJobSpecificItem = (HaulingJobSpecificItem)job;
							IKnownEntityData data;
							EntityResult knownData = entityIntelligence.GetKnownData(haulingJobSpecificItem.Item.Value, out data);
							if (knownData == EntityResult.EntityStatusIsNowUnknown || knownData == EntityResult.Destroyed)
							{
								haulingJobSpecificItem.Destroy(cancelTakers: true);
							}
							else if (data.IsUnassignedToAnythingButThisJob(haulingJobSpecificItem, entityIntelligence.Allegiance.SharedKnowledge))
							{
								GetAllHaulingCombosForItem(data, haulingJobSpecificItem, threatStanceToUse2);
							}
						}
					}
				}
				progress = Progress.ScoreCombos;
			}
			else
			{
				progress = Progress.NotStarted;
			}
		}
		if (progress == Progress.ScoreCombos)
		{
			HaulingCombo? bestCombo = null;
			if (ScoreAllCombosAndReturnBest(minimumRatingToConsider, ref bestCombo) != CalculateResult.Done)
			{
				return CalculateResult.Processing;
			}
			progress = Progress.NotStarted;
			if (bestCombo.HasValue)
			{
				mostDesirableJob = bestCombo.Value.Job;
				bestItem = bestCombo.Value.Item;
				bestVehicle = bestCombo.Value.Vehicle;
				desirability = GoalEvaluator.AddTimeAgeAndPriority(bestCombo.Value.Score, timeOfDayContribution, ageContribution, Priority);
				bestScore = desirability;
				entityIntelligence.TopScoringJobs.Add(new GoalAndScore
				{
					Score = bestScore,
					Goal = mostDesirableJob.ToString()
				});
				return CalculateResult.Done;
			}
		}
		return CalculateResult.Done;
	}

	private void GetAllHaulingCombosForItemType(HaulingJobAnyItemOfType job, EntityType itemType, EntityGroup entityGroup, ThreatStance threatStance)
	{
		if (!entityGroup.Items.TryGetValue(itemType, out var value))
		{
			return;
		}
		for (int num = value.Count - 1; num >= 0; num--)
		{
			EntityID entityID = value[num];
			if (GoalEvaluator.HandleOwnerDataResult(entityIntelligence.Allegiance.SharedKnowledge, entityID, entityGroup, out var entityData) && entityData.IsUnassignedToAnythingButThisJob(job, entityIntelligence.Allegiance.SharedKnowledge))
			{
				GetAllHaulingCombosForItem(entityData, job, threatStance);
			}
		}
	}

	private CalculateResult EstimateHaulingTimeByFoot(Entity entity, EntityID item, HaulingJob haulingJob, RegionMap regionMap, ref double haulingTime, out IKnownEntityData itemToHaul)
	{
		EntityID? getToStorageEntity = haulingJob.GetToStorageEntity;
		Vector3? toLocation = haulingJob.ToLocation;
		float distance = 0f;
		float distance2 = 0f;
		SharedKnowledge sharedKnowledge = entity.Intelligence.Allegiance.SharedKnowledge;
		IKnownEntityData data = null;
		IKnownEntityData data2;
		EntityResult knownData = sharedKnowledge.GetKnownData(item, out data2);
		if (knownData == EntityResult.Destroyed || knownData == EntityResult.EntityStatusIsNowUnknown)
		{
			itemToHaul = null;
			haulingTime = -1.0;
			return CalculateResult.Done;
		}
		itemToHaul = data2;
		Point? endingSubtile = null;
		if (getToStorageEntity.HasValue)
		{
			EntityResult knownData2 = sharedKnowledge.GetKnownData(getToStorageEntity.Value, out data);
			if (knownData2 == EntityResult.Destroyed || knownData2 == EntityResult.EntityStatusIsNowUnknown)
			{
				haulingTime = -1.0;
				return CalculateResult.Done;
			}
		}
		else
		{
			endingSubtile = MapManager.WorldPosToSubtile(toLocation.Value);
		}
		switch (regionMap.GetDistanceToEntity(entity, entity, data2, ref distance))
		{
		case RegionMap.Result.Wait:
			return CalculateResult.Processing;
		case RegionMap.Result.NoAccess:
			haulingTime = -1.0;
			return CalculateResult.Done;
		default:
		{
			bool flag = false;
			if (data2 is Entity { ContainedBy: var containedBy } && containedBy == entity.EntityID)
			{
				flag = true;
			}
			distance = ((!flag) ? Common.ClampBottom(distance, 15f) : 0f);
			Point? fromSubtile;
			Point? toSubtile;
			RegionMap.Result distanceToEntity = regionMap.GetDistanceToEntity(entity, data2, data, ref distance2, out fromSubtile, out toSubtile, null, endingSubtile);
			if (fromSubtile.HasValue && toSubtile.HasValue)
			{
				GoalEvaluator.UpdateJobAccessibility(entity, fromSubtile.Value, toSubtile.Value, haulingJob.RequiredByProcessJob, ownerOfJobs.Parent, distanceToEntity);
			}
			switch (distanceToEntity)
			{
			case RegionMap.Result.Wait:
				return CalculateResult.Processing;
			case RegionMap.Result.NoAccess:
				haulingTime = -1.0;
				return CalculateResult.Done;
			default:
			{
				double num = distance / entity.Locomotor.CalculateSpeed(0f, calculateWithTerrain: false) + distance2 / entity.Locomotor.CalculateSpeed(data2.Bulk, calculateWithTerrain: false);
				haulingTime = num * (double)PlainsType.Instance.MovementFactor(SurfaceType.TransportType.Foot, SurfaceType.TerrainFeatures.None);
				if (flag)
				{
					haulingTime *= 0.8;
				}
				return CalculateResult.Done;
			}
			}
		}
		}
	}

	private static CalculateResult EstimateHaulingTimeByVehicle(Entity entity, EntityID item, IKnownEntityData vehicle, EntityID? storageEntity, Vector3? groundLocation, RegionMap footRegionMap, ref double haulingTime)
	{
		float distance = 0f;
		float distance2 = 0f;
		float distance3 = 0f;
		SharedKnowledge sharedKnowledge = entity.Intelligence.Allegiance.SharedKnowledge;
		IKnownEntityData data = null;
		IKnownEntityData data2;
		EntityResult knownData = sharedKnowledge.GetKnownData(vehicle.EntityID, out data2);
		if (knownData == EntityResult.Destroyed || knownData == EntityResult.EntityStatusIsNowUnknown)
		{
			haulingTime = -1.0;
			return CalculateResult.Done;
		}
		IKnownEntityData data3;
		EntityResult knownData2 = sharedKnowledge.GetKnownData(item, out data3);
		if (knownData2 == EntityResult.Destroyed || knownData2 == EntityResult.EntityStatusIsNowUnknown)
		{
			haulingTime = -1.0;
			return CalculateResult.Done;
		}
		if (storageEntity.HasValue)
		{
			switch (sharedKnowledge.GetKnownData(storageEntity.Value, out data))
			{
			case EntityResult.EntityStatusIsNowUnknown:
			case EntityResult.Destroyed:
				haulingTime = -1.0;
				return CalculateResult.Done;
			}
		}
		switch (footRegionMap.GetDistanceToEntity(entity, entity, data2, ref distance))
		{
		case RegionMap.Result.Wait:
			return CalculateResult.Processing;
		case RegionMap.Result.NoAccess:
			haulingTime = -1.0;
			return CalculateResult.Done;
		default:
		{
			float num = vehicle.CalculateSpeed(data3.Bulk);
			double num2;
			if (((VehicleContainerType)vehicle.EntityType.ContainerType).Transport == SurfaceType.TransportType.Air)
			{
				num2 = EstimateAirTime(vehicle, groundLocation.Value, data2.PlaySiteLocation, num, data3.PlaySiteLocation);
			}
			else
			{
				RegionMap vehicleRegionMap = sharedKnowledge.GetVehicleRegionMap(entity);
				switch (vehicleRegionMap.GetDistanceToEntity(entity, data2, data3, ref distance2))
				{
				case RegionMap.Result.Wait:
					return CalculateResult.Processing;
				case RegionMap.Result.NoAccess:
					haulingTime = -1.0;
					return CalculateResult.Done;
				}
				switch ((data == null) ? vehicleRegionMap.GetDistanceToEntity(entity, data3, null, ref distance3, null, groundLocation.HasValue ? new Point?(MapManager.WorldPosToSubtile(groundLocation.Value)) : ((Point?)null)) : vehicleRegionMap.GetDistanceToEntity(entity, data3, data, ref distance3))
				{
				case RegionMap.Result.Wait:
					return CalculateResult.Processing;
				case RegionMap.Result.NoAccess:
					haulingTime = -1.0;
					return CalculateResult.Done;
				}
				num2 = (distance2 / vehicle.CurrentMaximumSpeed.Value + distance3 / num) * PlainsType.Instance.MovementFactor(((VehicleContainerType)vehicle.EntityType.ContainerType).Transport, SurfaceType.TerrainFeatures.None);
			}
			double num3 = distance / entity.Locomotor.CurrentMaximumSpeedForEvaluator * PlainsType.Instance.MovementFactor(SurfaceType.TransportType.Foot, SurfaceType.TerrainFeatures.None);
			haulingTime = num3 + num2 + GameData.Instance.AIConstants.EvaluatorTimePenaltyForUsingVehicles;
			return CalculateResult.Done;
		}
		}
	}

	public static double EstimateAirTime(IKnownEntityData vehicle, Vector3 destination, Vector3 knownVehicleLocation, float estimatedLoadedVehicleSpeed, Vector3 knownItemLocation)
	{
		return (double)(Common.DistanceOctile(knownVehicleLocation, knownItemLocation) / vehicle.CurrentMaximumSpeed.Value + Common.DistanceOctile(knownItemLocation, destination) / estimatedLoadedVehicleSpeed) + (double)(2f * ((VehicleContainerType)vehicle.EntityType.ContainerType).Aircraft.EstimatedTakeOffLandingTime);
	}

	private void GetAllHaulingCombosForItem(IKnownEntityData itemData, HaulingJob job, ThreatStance threatStance)
	{
		if (!CanTakeStanceForJob(job, threatStance, out var threatStanceToUse) || !itemData.IsItemValidForHauling(entity, entityIntelligence, job) || !GoalEvaluator.WorkSiteIsSafe(entity, itemData, threatStanceToUse))
		{
			return;
		}
		haulingCombos.Add(new HaulingCombo
		{
			Vehicle = null,
			Item = itemData.EntityID,
			Job = job
		});
		if (personEntity == null || !personEntity.CanDrive())
		{
			return;
		}
		foreach (EntityGroup ownersOfVehicle in OwnersOfVehicles)
		{
			for (int num = ownersOfVehicle.Vehicles.Count - 1; num >= 0; num--)
			{
				EntityID entityID = ownersOfVehicle.Vehicles[num];
				if (GoalEvaluator.HandleOwnerDataResult(entityIntelligence.Allegiance.SharedKnowledge, entityID, ownersOfVehicle, out var entityData) && entityData.IsVehicleValidForHauling(entity, itemData))
				{
					haulingCombos.Add(new HaulingCombo
					{
						Vehicle = entityID,
						Item = itemData.EntityID,
						Job = job
					});
				}
			}
		}
	}

	public override bool CancelCurrentTakers()
	{
		return CancelEntities(needsToBeCancelled, itemsToBeDropped);
	}

	public override bool CanTakeGoal()
	{
		if (mostDesirableJob != null)
		{
			if (!EvaluateJob.IsJobValid(mostDesirableJob))
			{
				return false;
			}
			needsToBeCancelled.Clear();
			itemsToBeDropped.Clear();
			IKnownEntityData data = null;
			if (!GoalEvaluator.EntityDataResultCausesSkip(entityIntelligence.Allegiance.SharedKnowledge.GetKnownData(bestItem.Value, out var data2)))
			{
				if (bestVehicle.HasValue && GoalEvaluator.EntityDataResultCausesSkip(entityIntelligence.Allegiance.SharedKnowledge.GetKnownData(bestVehicle.Value, out data)))
				{
					return false;
				}
				if (GetJobsToCancel(mostDesirableJob, data2, data) && GoalEvaluator.IsScoreBetterThanAllInvolveds(bestScore, needsToBeCancelled))
				{
					return true;
				}
			}
		}
		return false;
	}

	public override bool SetGoal()
	{
		base.SetGoal();
		if (mostDesirableJob != null)
		{
			EvaluateJob.SetDebugScore(entity, mostDesirableJob, bestScore);
			GoalHaul g = new GoalHaul(entity, mostDesirableJob, bestItem.Value, bestVehicle, mostDesirableJob.NewOwner, GetOwnerIDs(OwnersOfVehicles), ownerOfJobs.ID)
			{
				GoalEvaluator = this
			};
			entityIntelligence.SetTopLevelGoal(g, bestScore);
			return true;
		}
		return false;
	}

	private bool IsComboValidForHauling(Entity entity, Intelligence entityIntelligence, ThreatStance threatStance, HaulingJob comboJob, IKnownEntityData comboItemData, IKnownEntityData comboVehicleData)
	{
		if (comboItemData.IsItemValidForHauling(entity, entityIntelligence, comboJob) && GoalEvaluator.WorkSiteIsSafe(entity, comboItemData, threatStance))
		{
			return comboVehicleData?.IsVehicleValidForHauling(entity, comboItemData) ?? true;
		}
		return false;
	}

	private CalculateResult ScoreAllCombosAndReturnBest(double minimumRatingToConsider, ref HaulingCombo? bestCombo)
	{
		double rating = -1.0;
		ThreatStance threatStanceToUse;
		RegionMap regionMapAndStanceForEvaluator = GoalEvaluator.GetRegionMapAndStanceForEvaluator(entity, null, out threatStanceToUse);
		double value = GetAgeContribution();
		double value2 = ScoreTimeOfDay();
		IKnownEntityData data;
		while (haulingComboProgress < haulingCombos.Count)
		{
			HaulingCombo value3 = haulingCombos[haulingComboProgress];
			if (GoalEvaluator.EntityDataResultCausesSkip(entityIntelligence.GetKnownData(value3.Item, out data)))
			{
				value3.Score = 0.0;
				haulingCombos[haulingComboProgress] = value3;
			}
			else
			{
				IKnownEntityData data2 = null;
				if (value3.Vehicle.HasValue && GoalEvaluator.EntityDataResultCausesSkip(entityIntelligence.GetKnownData(value3.Vehicle.Value, out data2)))
				{
					value3.Score = 0.0;
					haulingCombos[haulingComboProgress] = value3;
				}
				else if (!scoringWasInterrupted || IsComboValidForHauling(entity, entityIntelligence, threatStanceToUse, value3.Job, data, data2))
				{
					if (ScoreThisJob(regionMapAndStanceForEvaluator, threatStanceToUse, entity, value3.Job, 1, value, value2, out rating, null, null, new HaulingParams
					{
						Item = value3.Item,
						Vehicle = data2
					}) != CalculateResult.Done)
					{
						scoringWasInterrupted = true;
						return CalculateResult.Processing;
					}
					value3.Score = rating;
					haulingCombos[haulingComboProgress] = value3;
					EvaluateJob.SetDebugScore(entity, value3.Job, rating);
				}
				else
				{
					value3.Score = 0.0;
					haulingCombos[haulingComboProgress] = value3;
					EvaluateJob.SetDebugScore(entity, value3.Job, 0.0);
				}
			}
			haulingComboProgress++;
		}
		entity.AgentStorage.IterateContained(SetItemOKToHaulByOthers);
		haulingCombos.RemoveAll((HaulingCombo c) => c.Score == 0.0);
		haulingCombos.Sort((HaulingCombo a, HaulingCombo b) => b.Score.CompareTo(a.Score));
		bestCombo = null;
		for (int num = 0; num < haulingCombos.Count; num++)
		{
			HaulingCombo value3 = haulingCombos[num];
			if (GoalEvaluator.EntityDataResultCausesSkip(entityIntelligence.GetKnownData(value3.Item, out data)))
			{
				continue;
			}
			IKnownEntityData data2 = null;
			if ((value3.Vehicle.HasValue && GoalEvaluator.EntityDataResultCausesSkip(entityIntelligence.GetKnownData(value3.Vehicle.Value, out data2))) || (scoringWasInterrupted && !IsComboValidForHauling(entity, entityIntelligence, threatStanceToUse, value3.Job, data, data2)))
			{
				continue;
			}
			if (value3.Score < minimumRatingToConsider)
			{
				break;
			}
			if (GetJobsToCancel(value3.Job, data, data2))
			{
				if (needsToBeCancelled.Count == 0)
				{
					bestCombo = value3;
					break;
				}
				if (GoalEvaluator.IsScoreBetterThanAllInvolveds(value3.Score, needsToBeCancelled))
				{
					bestCombo = value3;
					break;
				}
			}
		}
		return CalculateResult.Done;
	}

	public void SetItemOKToHaulByOthers(Entity item)
	{
		Item item2 = item.Item;
		if (item2 != null)
		{
			item2.OKToTakeThisItemFromCarrier = true;
		}
	}

	private bool GetJobsToCancel(Job job, IKnownEntityData itemData, IKnownEntityData vehicleData)
	{
		needsToBeCancelled.Clear();
		itemsToBeDropped.Clear();
		List<Entity> list = new List<Entity>();
		if (job.TakenBy.Count > 0)
		{
			job.TakenBy.GetLowestScorer();
			for (int i = 0; i < job.TakenBy.Count; i++)
			{
				list.Add(job.TakenBy.Get(i));
			}
			while (list.Count >= job.MaxJobPositions)
			{
				if (list[0] == base.entity)
				{
					entityIntelligence.Brain.IsSame(job);
				}
				needsToBeCancelled.Add(list[0]);
				list.RemoveAt(0);
			}
		}
		GetItemUser(itemData, needsToBeCancelled);
		Job job2 = EvaluateJob.ResolveAssignedToJob(itemData);
		if (job2 != null && job2.TakenBy.Count > 0)
		{
			job2.TakenBy.Get(0);
			_ = base.entity;
			needsToBeCancelled.Add(job2.TakenBy.Get(0));
		}
		if (itemData is Entity entity)
		{
			if (!entity.CarriedByAgent(out var carrier))
			{
				return false;
			}
			if (carrier != null && carrier != base.entity)
			{
				itemsToBeDropped.Add(entity);
			}
		}
		if (vehicleData != null)
		{
			GetItemUser(vehicleData, needsToBeCancelled);
		}
		needsToBeCancelled = needsToBeCancelled.Distinct().ToList();
		itemsToBeDropped = itemsToBeDropped.Distinct().ToList();
		return true;
	}
}
