namespace UWGame.SimSide.AI.Goals;

public interface ITopLevelGoal
{
	double TimeSpentInTopLevelGoal { get; set; }

	double ScoreGoal();
}
