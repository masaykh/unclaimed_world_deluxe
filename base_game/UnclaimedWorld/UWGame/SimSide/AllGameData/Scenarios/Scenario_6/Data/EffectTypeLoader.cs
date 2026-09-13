using System.Collections.Generic;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.SimEffects;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_6.Data;

public class EffectTypeLoader
{
	public static List<EffectType> Init()
	{
		return new List<EffectType>
		{
			new FlagEffectType
			{
				KeyName = "contract",
				Name = "Has permission to leave",
				Affects = AffectsFlags.CanEmigrate,
				DynamicDurationInDays = new UnaryFunctionNode
				{
					Operator = UnaryExpressionOperator.DateToRelativeDays,
					Operand = new ValueNode
					{
						PropertyKey = "endDate"
					}
				},
				Value = false
			}
		};
	}
}
