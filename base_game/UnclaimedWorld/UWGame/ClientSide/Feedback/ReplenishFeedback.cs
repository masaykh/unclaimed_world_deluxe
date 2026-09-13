using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Systems;

namespace UWGame.ClientSide.Feedback;

public class ReplenishFeedback : ISleepingUpdatable
{
	private const double timeInSecondsToRevert = 10.0;

	public ExpeditionID Expedition;

	public EntityType EntityType;

	public float? MinimumAmountRequired { get; private set; }

	public bool OwnsItem { get; private set; }

	public bool ItemIsAvailable { get; private set; }

	public double? TimePointInSeconds { get; private set; }

	public double? UpdateInterval => 10.0;

	public SleepyUpdaterID SleepyUpdater { get; set; }

	public ReplenishFeedback(Expedition expedition, EntityType entityType)
	{
		Expedition = expedition.ID;
		EntityType = entityType;
		OwnsItem = true;
		ItemIsAvailable = true;
	}

	public void SetOwnsItem(bool value, float? requiredAmount)
	{
		RefreshExpiry();
		if (value)
		{
			OwnsItem = true;
		}
		else
		{
			OwnsItem = false;
		}
		SetRequiredAmount(value, requiredAmount);
	}

	public void SetItemIsAvailable(bool value, float? requiredAmount)
	{
		RefreshExpiry();
		ItemIsAvailable = value;
		SetRequiredAmount(value, requiredAmount);
	}

	private void SetRequiredAmount(bool value, float? requiredAmount)
	{
		if (value)
		{
			MinimumAmountRequired = null;
		}
		else if (MinimumAmountRequired.HasValue)
		{
			if (requiredAmount < MinimumAmountRequired.Value)
			{
				MinimumAmountRequired = requiredAmount;
			}
		}
		else
		{
			MinimumAmountRequired = requiredAmount;
		}
	}

	public void RefreshExpiry()
	{
		LookUpSleepyUpdater<ReplenishFeedback>.FindByID(SleepyUpdater)?.NotifyUpdateIntervalChanged(this);
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
		LookUpSleepyUpdater<ReplenishFeedback>.Create();
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
		The.Client.Feedback.DestroyReplenishFeedback(this);
	}

	public void RecomputeUpdateInterval(out bool intervalChanged)
	{
		intervalChanged = false;
	}
}
