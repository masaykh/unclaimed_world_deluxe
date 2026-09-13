using System.Collections.Generic;
using UWGame.SimSide.InGameEvents;
using UWGame.SimSide.InGameEvents.Conditions;
using UWGame.SimSide.InGameEvents.Expressions;
using UWGame.SimSide.InGameEvents.PropertyObjects;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_4y.Data;

public class PolledEventsLoader
{
	public static List<PolledEventType> Init()
	{
		return new List<PolledEventType>
		{
			new PolledEventType
			{
				KeyName = "SANDBOXNOMADMAP_migrationLesserWhipjawEast",
				PollInterval = new ValueNode
				{
					PropertyKey = "lesserWhipjawMigrationInterval"
				},
				StartTimePoint = new TimePoint
				{
					RelativeTimeInSeconds = new ValueNode
					{
						PropertyKey = "lesserWhipjawMigrationInterval"
					}
				},
				AllowRandomTimeOffset = false,
				Condition = new CustomCondition
				{
					TargetObject = new TargetObject
					{
						GetList = new GetList
						{
							HasPropertiesListKey = "allegiances",
							FilterCondition = new PropertyCondition
							{
								PropertyKey = "keyName",
								ConstantStringEqual = "lesserWhipjawAllegianceWest"
							},
							NextList = new GetList
							{
								HasPropertiesListKey = "members"
							}
						}
					},
					ListCondition = new ListCondition
					{
						CountMaximum = new ValueNode
						{
							PropertyKey = "maxLesserWhipjaw"
						}
					}
				},
				ActionSetsKey = "migrationLesserWhipjawEast"
			},
			new PolledEventType
			{
				KeyName = "SANDBOXNOMADMAP_migrationBajinganNorth",
				PollInterval = new ValueNode
				{
					PropertyKey = "bajinganMigrationInterval"
				},
				StartTimePoint = new TimePoint
				{
					RelativeTimeInSeconds = new ValueNode
					{
						PropertyKey = "bajinganMigrationInterval"
					}
				},
				AllowRandomTimeOffset = false,
				Condition = new CustomCondition
				{
					TargetObject = new TargetObject
					{
						GetList = new GetList
						{
							HasPropertiesListKey = "allegiances",
							FilterCondition = new PropertyCondition
							{
								PropertyKey = "keyName",
								ConstantStringEqual = "bajinganAllegianceWest"
							},
							NextList = new GetList
							{
								HasPropertiesListKey = "members"
							}
						}
					},
					ListCondition = new ListCondition
					{
						CountMaximum = new ValueNode
						{
							PropertyKey = "maxBajingan"
						}
					}
				},
				ActionSetsKey = "migrationBajinganNorth"
			}
		};
	}
}
