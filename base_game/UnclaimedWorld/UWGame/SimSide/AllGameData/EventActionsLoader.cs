using System.Collections.Generic;
using UWGame.SimSide.InGameEvents.Actions;
using UWGame.SimSide.InGameEvents.Expressions;

namespace UWGame.SimSide.AllGameData;

public class EventActionsLoader
{
	public static List<EventActionType> Init()
	{
		List<EventActionType> obj = new List<EventActionType>
		{
			new SetPropertyAction
			{
				KeyName = "initEnableGroupMeetings",
				PropertyKey = "enableGroupMeetings",
				Value = new ValueNode
				{
					Bool = true
				}
			},
			new SetPropertyAction
			{
				KeyName = "initTimeBeforeGroupMeeting",
				PropertyKey = "timeInGameSecondsBeforeGroupMeeting",
				Value = new ValueNode
				{
					Decimal = 400f
				}
			}
		};
		string text = "#FOR: I'm glad that most of us agree to ramp up our food production. Using better methods to produce and to store food will make our life safer! \n \n#AGAINST: I know I'm being outvoted today. I just don't agree that we should change the way we produce food. The methods we've used until now are more than adequate, they just have to be used the right way! \n \n";
		string text2 = "#FOR: I'm glad that most of us agree to make this place more comfortable. I know it'll take some hard work in the beginning but we deserve better houses and wellbeing!  \n \n#AGAINST: I still think that it's irresponsible to use so many resources on our personal comfort. But I respect the outcome of this vote. \n \n";
		string text3 = "#FOR: I'm happy that most of you agree to improve security here. Getting better weapons is vital and I assure you it will make you all sleep better at night! \n \n#AGAINST: I think you are all overreacting. Spending more resources on weapons is wasteful, we would be safe if people here used our existing equipment the right way. But I respect your decision. \n \n";
		obj.Add(new SetPropertyAction
		{
			KeyName = "comfortBasicPolicyAdopted",
			PropertyKey = "comfortBasicPolicyAdopted",
			Value = new ValueNode
			{
				String = text2
			}
		});
		obj.Add(new SetPropertyAction
		{
			KeyName = "foodBasicPolicyAdopted",
			PropertyKey = "foodBasicPolicyAdopted",
			Value = new ValueNode
			{
				String = text
			}
		});
		obj.Add(new SetPropertyAction
		{
			KeyName = "securityBasicPolicyAdopted",
			PropertyKey = "securityBasicPolicyAdopted",
			Value = new ValueNode
			{
				String = text3
			}
		});
		obj.Add(new SetPropertyAction
		{
			KeyName = "comfortMediumPolicyAdopted",
			PropertyKey = "comfortMediumPolicyAdopted",
			Value = new ValueNode
			{
				String = text2
			}
		});
		obj.Add(new SetPropertyAction
		{
			KeyName = "foodMediumPolicyAdopted",
			PropertyKey = "foodMediumPolicyAdopted",
			Value = new ValueNode
			{
				String = text
			}
		});
		obj.Add(new SetPropertyAction
		{
			KeyName = "securityMediumPolicyAdopted",
			PropertyKey = "securityMediumPolicyAdopted",
			Value = new ValueNode
			{
				String = text3
			}
		});
		obj.Add(new SetPropertyAction
		{
			KeyName = "comfortAdvancedPolicyAdopted",
			PropertyKey = "comfortAdvancedPolicyAdopted",
			Value = new ValueNode
			{
				String = text2
			}
		});
		obj.Add(new SetPropertyAction
		{
			KeyName = "foodAdvancedPolicyAdopted",
			PropertyKey = "foodAdvancedPolicyAdopted",
			Value = new ValueNode
			{
				String = text
			}
		});
		obj.Add(new SetPropertyAction
		{
			KeyName = "securityAdvancedPolicyAdopted",
			PropertyKey = "securityAdvancedPolicyAdopted",
			Value = new ValueNode
			{
				String = text3
			}
		});
		string text4 = "#FOR: I'm glad that we can all agree to improve our food supply! From now on, we'll use better methods to produce and to store food. It'll take some hard work getting it going, but it will be worth it! \n \n";
		string text5 = "#FOR: I'm glad that we can all agree to make this place more comfortable! From now on, we'll make an effort to improve our houses and well-being. It'll take some hard work getting it going, but it will be worth it! \n \n";
		string text6 = "#FOR: I'm glad that we can all agree to improve security here! From now on, we'll focus on getting better weapons - it'll take some hard work, but it will be worth it! \n \n";
		obj.Add(new SetPropertyAction
		{
			KeyName = "comfortBasicPolicyAdoptedAllAgree",
			PropertyKey = "comfortBasicPolicyAdoptedAllAgree",
			Value = new ValueNode
			{
				String = text5
			}
		});
		obj.Add(new SetPropertyAction
		{
			KeyName = "foodBasicPolicyAdoptedAllAgree",
			PropertyKey = "foodBasicPolicyAdoptedAllAgree",
			Value = new ValueNode
			{
				String = text4
			}
		});
		obj.Add(new SetPropertyAction
		{
			KeyName = "securityBasicPolicyAdoptedAllAgree",
			PropertyKey = "securityBasicPolicyAdoptedAllAgree",
			Value = new ValueNode
			{
				String = text6
			}
		});
		obj.Add(new SetPropertyAction
		{
			KeyName = "comfortMediumPolicyAdoptedAllAgree",
			PropertyKey = "comfortMediumPolicyAdoptedAllAgree",
			Value = new ValueNode
			{
				String = text5
			}
		});
		obj.Add(new SetPropertyAction
		{
			KeyName = "foodMediumPolicyAdoptedAllAgree",
			PropertyKey = "foodMediumPolicyAdoptedAllAgree",
			Value = new ValueNode
			{
				String = text4
			}
		});
		obj.Add(new SetPropertyAction
		{
			KeyName = "securityMediumPolicyAdoptedAllAgree",
			PropertyKey = "securityMediumPolicyAdoptedAllAgree",
			Value = new ValueNode
			{
				String = text6
			}
		});
		obj.Add(new SetPropertyAction
		{
			KeyName = "comfortAdvancedPolicyAdoptedAllAgree",
			PropertyKey = "comfortAdvancedPolicyAdoptedAllAgree",
			Value = new ValueNode
			{
				String = text5
			}
		});
		obj.Add(new SetPropertyAction
		{
			KeyName = "foodAdvancedPolicyAdoptedAllAgree",
			PropertyKey = "foodAdvancedPolicyAdoptedAllAgree",
			Value = new ValueNode
			{
				String = text4
			}
		});
		obj.Add(new SetPropertyAction
		{
			KeyName = "securityAdvancedPolicyAdoptedAllAgree",
			PropertyKey = "securityAdvancedPolicyAdoptedAllAgree",
			Value = new ValueNode
			{
				String = text6
			}
		});
		return obj;
	}
}
