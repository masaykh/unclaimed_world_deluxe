using UWGame.ClientSide.PropertyPresentation;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.InGameEvents.Expressions;

namespace UWGame.SimSide.Allegiances;

public class StatsData
{
	public float? Security = 1f;

	public float? Comfort = 1f;

	public float? FoodSupply = 1f;

	public NormalDistribution RandomSecurity;

	public NormalDistribution RandomComfort;

	public NormalDistribution RandomFood;

	public EvalNode DynamicFood;

	public EvalNode DynamicComfort;

	public EvalNode DynamicSecurity;

	public float GetRating(RatingTypes rating)
	{
		return rating switch
		{
			RatingTypes.Comfort => GetValue(Comfort, RandomComfort, DynamicComfort), 
			RatingTypes.Food => GetValue(FoodSupply, RandomFood, DynamicFood), 
			RatingTypes.Security => GetValue(Security, RandomSecurity, DynamicSecurity), 
			_ => 0f, 
		};
	}

	private static float GetValue(float? staticValue, NormalDistribution randomValue, EvalNode dynamicValue)
	{
		if (dynamicValue != null)
		{
			PropertyResult? propertyResult = dynamicValue.Evaluate(null, null, null, null);
			if (propertyResult.HasValue)
			{
				return propertyResult.Value.NumberResult.Value;
			}
			return 0f;
		}
		if (randomValue != null)
		{
			return (float)randomValue.GetRandomValue(The.Sim.GameplayRandomGenerator, clampBetweenZeroAndOne: true);
		}
		return staticValue ?? 0f;
	}
}
