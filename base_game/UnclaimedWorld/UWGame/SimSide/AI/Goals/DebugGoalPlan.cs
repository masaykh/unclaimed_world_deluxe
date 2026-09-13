namespace UWGame.SimSide.AI.Goals;

public class DebugGoalPlan
{
	public GoalPlanner GoalPlanner { get; set; }

	public DebugGoalPlan(GoalPlanner plan)
	{
		GoalPlanner = plan;
	}
}
