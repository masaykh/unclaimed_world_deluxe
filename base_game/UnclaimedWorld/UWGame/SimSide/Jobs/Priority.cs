namespace UWGame.SimSide.Jobs;

public enum Priority
{
	Normal,
	High,
	Low,
	// MOD: GatherOnDemandMod's STOP in a construction's priority list. Appended, so the values a
	// save already holds keep their meaning. Rated 0 (EvaluateJob.ApplyJobPriorityModifier), so
	// neither the job nor the hauling for it is taken.
	Stopped
}
