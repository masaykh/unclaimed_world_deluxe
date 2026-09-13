namespace UWGame.SimSide.AI.Needs;

public class PhysicalEffects
{
	public float LimitForIncreasedSickness;

	public float SicknessFactor = 1f;

	public float LimitForReducedGrowth;

	public float GrowthReductionFactor = 1f;

	public float? LimitForWeightReduction;

	public float? LimitForWeightIncrease;

	public float? DaysAtZeroCausingDeath;

	public float? DaysAtZeroCausingCollapse;

	public float? DaysAtZeroDecreaseFactor = 1f;

	public bool UseExertionFactorToDecrease;

	public bool ShouldSerializeLimitForWeightReduction()
	{
		return LimitForWeightReduction.HasValue;
	}

	public bool ShouldSerializeLimitForWeightIncrease()
	{
		return LimitForWeightIncrease.HasValue;
	}

	public bool ShouldSerializeDaysAtZeroCausingDeath()
	{
		return DaysAtZeroCausingDeath.HasValue;
	}

	public bool ShouldSerializeDaysAtZeroCausingCollapse()
	{
		return DaysAtZeroCausingCollapse.HasValue;
	}

	public bool ShouldSerializeDaysAtZeroDecreaseFactor()
	{
		return DaysAtZeroDecreaseFactor.HasValue;
	}
}
