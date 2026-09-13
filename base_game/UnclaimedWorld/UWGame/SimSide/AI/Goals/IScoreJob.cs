using UWGame.SimSide.Entities;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Maps;

namespace UWGame.SimSide.AI.Goals;

internal interface IScoreJob
{
	GoalEvaluator.CalculateResult ScoreThisJob(RegionMap regionMap, ThreatStance threatStanceToUse, Entity entity, Job job, int proposedNumberOfWorkers, double? ageContribution, double? timeContribution, out double rating, ToolParams? toolParams = null, AttackParams? attackParams = null, HaulingParams? haulingParams = null);
}
