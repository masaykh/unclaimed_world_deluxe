using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UWGame.SimSide;
using UWGame.SimSide.AI;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Owners;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Systems;

namespace UWGame.ClientSide.Feedback;

public class FeedbackManager
{
	private Dictionary<JobID, AccessibilityFeedback> jobAccessibility = new Dictionary<JobID, AccessibilityFeedback>();

	private Dictionary<EntityID, AccessibilityFeedback> entityAccessibility = new Dictionary<EntityID, AccessibilityFeedback>();

	private SleepyUpdater<AccessibilityFeedback> accessibilityUpdater = new SleepyUpdater<AccessibilityFeedback>(Module.Client);

	private SleepyUpdater<ReplenishFeedback> replenishUpdater = new SleepyUpdater<ReplenishFeedback>(Module.Client);

	private Dictionary<ExpeditionID, Dictionary<EntityType, ReplenishFeedback>> toolReplenish = new Dictionary<ExpeditionID, Dictionary<EntityType, ReplenishFeedback>>();

	private Regulator replenishAlertRegulator;

	public FeedbackManager()
	{
		replenishAlertRegulator = new Regulator(The.Client.ClientRandomGenerator, 1.0 / GameData.Instance.GUIConstants.TimeBetweenReplenishAlerts, "FeedbackManager");
	}

	public void Update(GameTime gameTime)
	{
		accessibilityUpdater.Update(gameTime);
		replenishUpdater.Update(gameTime);
	}

	public void SetToolReplenishStatusOwnsItem(Expedition expedition, EntityType toolType, bool ownsItem, float? requiredAmount)
	{
		ReplenishFeedback existingReplenishFeedbackOrCreateNew = GetExistingReplenishFeedbackOrCreateNew(expedition, toolType);
		existingReplenishFeedbackOrCreateNew.SetOwnsItem(ownsItem, requiredAmount);
		if (!ownsItem && replenishAlertRegulator.IsReady())
		{
			The.Client.LogOutOfFuel(toolType, existingReplenishFeedbackOrCreateNew.MinimumAmountRequired);
		}
	}

	public void SetToolReplenishStatusItemIsAvailable(Expedition expedition, EntityType toolType, bool itemIsAvailable, float? requiredAmount)
	{
		if (itemIsAvailable)
		{
			ReplenishFeedback toolReplenishFeedback = GetToolReplenishFeedback(expedition, toolType);
			if (toolReplenishFeedback != null)
			{
				DestroyReplenishFeedback(toolReplenishFeedback);
			}
			return;
		}
		ReplenishFeedback existingReplenishFeedbackOrCreateNew = GetExistingReplenishFeedbackOrCreateNew(expedition, toolType);
		existingReplenishFeedbackOrCreateNew.SetItemIsAvailable(itemIsAvailable, requiredAmount);
		if (replenishAlertRegulator.IsReady())
		{
			The.Client.LogOutOfFuel(toolType, existingReplenishFeedbackOrCreateNew.MinimumAmountRequired);
		}
	}

	public bool GetToolReplenishStatus(Expedition expedition, EntityType toolType, out float? minimumRequiredAmount)
	{
		ReplenishFeedback toolReplenishFeedback = GetToolReplenishFeedback(expedition, toolType);
		if (toolReplenishFeedback != null && (!toolReplenishFeedback.OwnsItem || !toolReplenishFeedback.ItemIsAvailable))
		{
			minimumRequiredAmount = toolReplenishFeedback.MinimumAmountRequired;
			return false;
		}
		minimumRequiredAmount = null;
		return true;
	}

	private ReplenishFeedback GetToolReplenishFeedback(Expedition expedition, EntityType toolType)
	{
		if (toolReplenish.TryGetValue(expedition.ID, out var value) && value.TryGetValue(toolType, out var value2))
		{
			return value2;
		}
		return null;
	}

	private ReplenishFeedback GetExistingReplenishFeedbackOrCreateNew(Expedition expedition, EntityType toolType)
	{
		ReplenishFeedback replenishFeedback = GetToolReplenishFeedback(expedition, toolType);
		if (replenishFeedback == null)
		{
			replenishFeedback = new ReplenishFeedback(expedition, toolType);
			replenishUpdater.Add(replenishFeedback);
			Common.AddToNestedDictionary(toolReplenish, expedition.ID, toolType, replenishFeedback);
		}
		return replenishFeedback;
	}

	public void GetFeedback(JobID jobID, out bool isInAccessible, out bool isBlockedByThreat, out bool isBlockedDueToBoldStanceRequired, out bool tooFarFromExpedition, out bool huntingNotFeasible, out bool areaNotCleared)
	{
		if (jobAccessibility.TryGetValue(jobID, out var value))
		{
			isInAccessible = value.IsInaccessible;
			isBlockedByThreat = value.IsBlockedByThreat;
			isBlockedDueToBoldStanceRequired = value.IsBlockedDueToBoldStanceRequired;
			tooFarFromExpedition = value.TooFarFromExpedition;
			huntingNotFeasible = value.HuntingJobNotFeasible;
			areaNotCleared = value.AreaNotCleared;
		}
		else
		{
			isBlockedDueToBoldStanceRequired = false;
			isInAccessible = false;
			isBlockedByThreat = false;
			tooFarFromExpedition = false;
			huntingNotFeasible = false;
			areaNotCleared = false;
		}
	}

	public void GetFeedback(EntityID entityID, out bool isInAccessible, out bool isBlockedByThreat, out bool isBlockedByBoldStance)
	{
		if (entityAccessibility.TryGetValue(entityID, out var value))
		{
			isInAccessible = value.IsInaccessible;
			isBlockedByThreat = value.IsBlockedByThreat;
			isBlockedByBoldStance = value.IsBlockedDueToBoldStanceRequired;
		}
		else
		{
			isInAccessible = false;
			isBlockedByThreat = false;
			isBlockedByBoldStance = false;
		}
	}

	public bool GetIsInaccessible(JobID jobID)
	{
		if (jobAccessibility.TryGetValue(jobID, out var value))
		{
			return value.IsInaccessible;
		}
		return false;
	}

	public bool GetIsBlockedByThreat(JobID jobID)
	{
		if (jobAccessibility.TryGetValue(jobID, out var value))
		{
			return value.IsBlockedByThreat;
		}
		return false;
	}

	public void SetJobInaccessible(Job job, IHasEntityGroup ownerOfJob, bool isInaccessible)
	{
		if (!isInaccessible || job.TakenBy.Count <= 0)
		{
			GetExistingJobAccessibilityOrCreateNew(job, ownerOfJob, isInaccessible)?.SetIsInaccessible(ownerOfJob, isInaccessible);
		}
	}

	public void SetHuntingJobNotFeasible(Job job, IHasEntityGroup ownerOfJob, bool value)
	{
		if (!value || job.TakenBy.Count <= 0)
		{
			GetExistingJobAccessibilityOrCreateNew(job, ownerOfJob, value)?.SetHuntingJobNotFeasible(ownerOfJob, value);
		}
	}

	public void SetAreaNotCleared(Job job, IHasEntityGroup ownerOfJob, bool value)
	{
		GetExistingJobAccessibilityOrCreateNew(job, ownerOfJob, value)?.SetAreaNotCleared(value);
	}

	public void SetJobTooFarFromExpedition(Job job, IHasEntityGroup ownerOfJob, bool value)
	{
		if (!value || job.TakenBy.Count <= 0)
		{
			GetExistingJobAccessibilityOrCreateNew(job, ownerOfJob, value)?.SetTooFarFromExpedition(value);
		}
	}

	public void SetJobBlockedByBoldStance(Job job, IHasEntityGroup ownerOfJob, bool isBlocked)
	{
		if (!isBlocked || job.TakenBy.Count <= 0)
		{
			GetExistingJobAccessibilityOrCreateNew(job, ownerOfJob, isBlocked)?.SetBlockedByBoldStance(ownerOfJob, isBlocked);
		}
	}

	public void SetJobBlockedByThreat(Job job, IHasEntityGroup ownerOfJob, bool isBlocked)
	{
		if (!isBlocked || job.TakenBy.Count <= 0)
		{
			GetExistingJobAccessibilityOrCreateNew(job, ownerOfJob, isBlocked)?.SetBlockedByThreat(ownerOfJob, isBlocked);
		}
	}

	public void DestroyReplenishFeedback(ReplenishFeedback feedback)
	{
		replenishUpdater.Remove(feedback);
		Common.RemoveFromNestedDictionary(toolReplenish, feedback.Expedition, feedback.EntityType);
	}

	public void DestroyAccessibility(AccessibilityFeedback accessibility)
	{
		accessibilityUpdater.Remove(accessibility);
		if (accessibility.JobID.HasValue)
		{
			jobAccessibility.Remove(accessibility.JobID.Value);
		}
		else if (accessibility.EntityID.HasValue)
		{
			entityAccessibility.Remove(accessibility.EntityID.Value);
		}
	}

	public void DestroyAccessibility(JobID jobID)
	{
		if (jobAccessibility.TryGetValue(jobID, out var value))
		{
			jobAccessibility.Remove(jobID);
			accessibilityUpdater.Remove(value);
		}
	}

	public void DestroyAccessibility(EntityID entityID)
	{
		if (entityAccessibility.TryGetValue(entityID, out var value))
		{
			entityAccessibility.Remove(entityID);
			accessibilityUpdater.Remove(value);
		}
	}

	public void HandleDestroyedEntity(EntityID entityID)
	{
		if (The.InGameUI.UIAllegiance == null || !The.InGameUI.UIAllegiance.SharedKnowledge.MemoryFacts.ContainsKey(entityID))
		{
			DestroyAccessibility(entityID);
		}
	}

	private AccessibilityFeedback GetExistingJobAccessibilityOrCreateNew(Job job, IHasEntityGroup ownerOfJob, bool createNew)
	{
		if (ownerOfJob.Allegiance == The.InGameUI.UIAllegiance)
		{
			AccessibilityFeedback value = null;
			if (!jobAccessibility.TryGetValue(job.ID, out value) && createNew)
			{
				value = new AccessibilityFeedback(job.ID);
				accessibilityUpdater.Add(value);
				jobAccessibility.Add(job.ID, value);
			}
			return value;
		}
		return null;
	}

	private AccessibilityFeedback GetExistingEntityAccessibilityOrCreateNew(IKnownEntityData entityData, bool createNew)
	{
		if (!entityAccessibility.TryGetValue(entityData.EntityID, out var value) && createNew)
		{
			value = new AccessibilityFeedback(entityData.EntityID);
			accessibilityUpdater.Add(value);
			entityAccessibility.Add(entityData.EntityID, value);
		}
		return value;
	}

	public void SetEntityInaccessible(Allegiance agentAllegiance, IKnownEntityData entityData, bool isInaccessible)
	{
		if (agentAllegiance != The.InGameUI.UIAllegiance || entityData.EntityType.IntelligenceType != null)
		{
			return;
		}
		AccessibilityFeedback existingEntityAccessibilityOrCreateNew = GetExistingEntityAccessibilityOrCreateNew(entityData, isInaccessible);
		if (existingEntityAccessibilityOrCreateNew != null && LookUpOwners.ResolveEntityOwner(entityData, out IOwner owner) && owner != null && owner is IHasEntityGroup owner2)
		{
			existingEntityAccessibilityOrCreateNew.SetIsInaccessible(owner2, isInaccessible);
			if (!isInaccessible)
			{
				existingEntityAccessibilityOrCreateNew.SetBlockedByThreat(owner2, isInaccessible);
			}
		}
	}

	public void SetEntityBlockedByThreat(Allegiance agentAllegiance, IKnownEntityData entityData, bool blockedByThreat)
	{
		if (agentAllegiance == The.InGameUI.UIAllegiance && entityData.EntityType.BiologicalType == null && entityData.EntityType.IntelligenceType == null)
		{
			AccessibilityFeedback existingEntityAccessibilityOrCreateNew = GetExistingEntityAccessibilityOrCreateNew(entityData, blockedByThreat);
			if (existingEntityAccessibilityOrCreateNew != null && LookUpOwners.ResolveEntityOwner(entityData, out EntityGroup ownedEntities) && ownedEntities != null)
			{
				existingEntityAccessibilityOrCreateNew.SetBlockedByThreat(ownedEntities.Parent, blockedByThreat);
			}
		}
	}
}
