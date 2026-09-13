using Microsoft.Xna.Framework;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Systems;

namespace UWGame.ClientSide.Feedback;

public class AccessibilityFeedback : ISleepingUpdatable
{
	public EntityID? EntityID;

	public JobID? JobID;

	private const double timeInSecondsToRevert = 5.0;

	private int noOfTimesWasInaccessible;

	private int noOfTimesWasBlockedByThreat;

	private int noOfTimesWasBlockedByBoldStance;

	private int noOfTimesHuntingJobNotFeasible;

	public double? TimePointInSeconds { get; private set; }

	public bool IsBlockedDueToBoldStanceRequired { get; private set; }

	public bool IsBlockedByThreat { get; private set; }

	public bool IsInaccessible { get; private set; }

	public bool TooFarFromExpedition { get; private set; }

	public bool HuntingJobNotFeasible { get; private set; }

	public bool AreaNotCleared { get; private set; }

	public double? UpdateInterval => 5.0;

	public SleepyUpdaterID SleepyUpdater { get; set; }

	public AccessibilityFeedback(JobID jobID)
	{
		JobID = jobID;
	}

	public AccessibilityFeedback(EntityID entityID)
	{
		EntityID = entityID;
	}

	public void SetTooFarFromExpedition(bool value)
	{
		TooFarFromExpedition = value;
	}

	public void SetAreaNotCleared(bool value)
	{
		AreaNotCleared = value;
	}

	public void SetHuntingJobNotFeasible(IHasEntityGroup owner, bool value)
	{
		if (value)
		{
			noOfTimesHuntingJobNotFeasible++;
			if (SetClientFeedbackProperty(noOfTimesHuntingJobNotFeasible, owner))
			{
				noOfTimesHuntingJobNotFeasible = 0;
				HuntingJobNotFeasible = true;
			}
		}
		else
		{
			HuntingJobNotFeasible = false;
		}
	}

	public void SetBlockedByBoldStance(IHasEntityGroup owner, bool value)
	{
		if (value)
		{
			noOfTimesWasBlockedByBoldStance++;
			if (SetClientFeedbackProperty(noOfTimesWasBlockedByBoldStance, owner))
			{
				noOfTimesWasBlockedByBoldStance = 0;
				IsBlockedDueToBoldStanceRequired = true;
			}
		}
		else
		{
			IsBlockedDueToBoldStanceRequired = false;
		}
	}

	public void SetBlockedByThreat(IHasEntityGroup owner, bool value)
	{
		if (value)
		{
			noOfTimesWasBlockedByThreat++;
			if (SetClientFeedbackProperty(noOfTimesWasBlockedByThreat, owner))
			{
				noOfTimesWasBlockedByThreat = 0;
				IsBlockedByThreat = true;
			}
		}
		else
		{
			IsBlockedByThreat = false;
		}
	}

	public void SetIsInaccessible(IHasEntityGroup owner, bool value)
	{
		if (value)
		{
			noOfTimesWasInaccessible++;
			if (SetClientFeedbackProperty(noOfTimesWasInaccessible, owner))
			{
				noOfTimesWasInaccessible = 0;
				IsInaccessible = true;
			}
		}
		else
		{
			IsInaccessible = false;
		}
	}

	private bool SetClientFeedbackProperty(int timesSetTrue, IHasEntityGroup owner)
	{
		LookUpSleepyUpdater<AccessibilityFeedback>.FindByID(SleepyUpdater)?.NotifyUpdateIntervalChanged(this);
		if (timesSetTrue >= owner.NoOfWorkers * 2)
		{
			return true;
		}
		return false;
	}

	public void SetNextTimepoint(double? timepoint)
	{
		TimePointInSeconds = timepoint;
	}

	void ISleepingUpdatable.CreateSleepyLookupCollection()
	{
	}

	public static void CreateSleepyLookupCollection()
	{
		LookUpSleepyUpdater<AccessibilityFeedback>.Create();
	}

	public void Update(GameTime gameTime, out bool wasDestroyed)
	{
		UpdateExpiry(out wasDestroyed);
	}

	private void UpdateExpiry(out bool wasDestroyed)
	{
		wasDestroyed = false;
		if (The.Sim.TimepointReached(TimePointInSeconds.Value))
		{
			Destroy();
			wasDestroyed = true;
		}
	}

	private void Destroy()
	{
		The.Client.Feedback.DestroyAccessibility(this);
	}

	public void RecomputeUpdateInterval(out bool intervalChanged)
	{
		intervalChanged = false;
	}
}
