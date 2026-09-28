using UWGame.Mods;
using UWGame.SimSide.Entities;

namespace UWGame.SimSide.AI.Goals;

/// <summary>
/// Whether a predator should go and break into a building (HomeRaidMod, GoalBreakIn).
///
/// Given only to the raider species (GoalThink, HomeRaidMod.IsRaider), and 0 while the mod is off.
/// Which building is the mod's rule (HomeRaidMod.FindTarget): the nearest of the player's buildings
/// within the predator's aggro range that has colonists asleep inside at night, or food in store.
///
/// The score is the one EvaluateReturnHome gives a colonist with nothing better to do: just enough
/// to beat idling (IdleGoalDesirability) plus the brain's inertia towards what it is already doing.
/// So a raid is what a predator does instead of standing about - never instead of eating, sleeping
/// or answering an attack, which all score higher and take the brain away from it again.
/// </summary>
public class EvaluateBreakIn : GoalEvaluator
{
	private EntityID? target;

	public EvaluateBreakIn(Entity entity)
		: base(entity)
	{
	}

	public override CalculateResult CalculateDesirability(double minimumRatingToConsider, ref double result)
	{
		target = null;
		bestScore = 0.0;
		if (HomeRaidMod.Enabled)
		{
			Entity building = HomeRaidMod.FindTarget(entity);
			if (building != null)
			{
				target = building.EntityID;
				bestScore = 1.1 * (GameData.Instance.AIConstants.IdleGoalDesirability + (double)GameData.Instance.AIConstants.CurrentGoalInertia);
			}
		}
		result = bestScore;
		return CalculateResult.Done;
	}

	public override bool CancelCurrentTakers()
	{
		return true;
	}

	public override bool CanTakeGoal()
	{
		return target.HasValue;
	}

	public override bool SetGoal()
	{
		if (!target.HasValue)
		{
			return false;
		}
		base.SetGoal();
		entityIntelligence.SetTopLevelGoal(new GoalBreakIn(entity, target.Value), bestScore);
		return true;
	}
}
