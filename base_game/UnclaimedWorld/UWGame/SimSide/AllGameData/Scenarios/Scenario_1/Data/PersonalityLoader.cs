using System.Collections.Generic;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide.Entities;
using UWGame.SimSide.XmlCollections;

namespace UWGame.SimSide.AllGameData.Scenarios.Scenario_1.Data;

public class PersonalityLoader
{
	public static List<PersonalityType> Init()
	{
		return new List<PersonalityType>
		{
			new PersonalityType("Conlan")
			{
				Adaptability = new NormalDistribution
				{
					Mean = 0.5,
					StandardDeviation = 0.05000000074505806
				},
				Stability = new NormalDistribution
				{
					Mean = 0.75,
					StandardDeviation = 0.029999999329447746
				},
				Attraction = new SerializableDictionary<string, NormalDistribution> { 
				{
					"playerAllegiance",
					new NormalDistribution
					{
						Mean = 0.30000001192092896,
						StandardDeviation = 0.07999999821186066
					}
				} },
				Principles = new SerializableDictionary<RatingTypes, NormalDistribution>
				{
					{
						RatingTypes.Food,
						new NormalDistribution
						{
							Mean = 0.25,
							StandardDeviation = 0.009999999776482582
						}
					},
					{
						RatingTypes.Security,
						new NormalDistribution
						{
							Mean = 0.44999998807907104,
							StandardDeviation = 0.009999999776482582
						}
					},
					{
						RatingTypes.Comfort,
						new NormalDistribution
						{
							Mean = 0.0,
							StandardDeviation = 0.009999999776482582
						}
					}
				},
				SpokenLines = new SerializableDictionary<string, string>
				{
					{ "crashQuestion", "Rough landing. Everyone OK?" },
					{ "crashAnswer", "No injuries it seems!" },
					{ "crashConclusion", "We might just have a chance, then." },
					{ "crashSuggestion", "Now, I want to see what we managed to bring." },
					{ "branchesQuestion", "So we got spoak branches. Remind me what they were for?" },
					{ "branchesAnswer", "Fences. Arrange them tightly, and they should keep out any quadites." },
					{ "branchesAnswerComeback", "Alright." },
					{ "onlyThreeMembersLeftComment", "We need to stick together now." },
					{ "onlyThreeMembersLeftAnswer", "Agreed. This must not happen again." }
				}
			},
			new PersonalityType("Khan")
			{
				Adaptability = new NormalDistribution
				{
					Mean = 0.5,
					StandardDeviation = 0.05000000074505806
				},
				Stability = new NormalDistribution
				{
					Mean = 0.5,
					StandardDeviation = 0.10000000149011612
				},
				Attraction = new SerializableDictionary<string, NormalDistribution> { 
				{
					"playerAllegiance",
					new NormalDistribution
					{
						Mean = 0.30000001192092896,
						StandardDeviation = 0.07999999821186066
					}
				} },
				Principles = new SerializableDictionary<RatingTypes, NormalDistribution>
				{
					{
						RatingTypes.Food,
						new NormalDistribution
						{
							Mean = 0.44999998807907104,
							StandardDeviation = 0.009999999776482582
						}
					},
					{
						RatingTypes.Security,
						new NormalDistribution
						{
							Mean = 0.30000001192092896,
							StandardDeviation = 0.009999999776482582
						}
					},
					{
						RatingTypes.Comfort,
						new NormalDistribution
						{
							Mean = 0.15000000596046448,
							StandardDeviation = 0.009999999776482582
						}
					}
				},
				SpokenLines = new SerializableDictionary<string, string>
				{
					{ "crashQuestion", "Is everyone alright?" },
					{ "crashAnswer", "Looks like we're all in one piece." },
					{ "crashConclusion", "Good. I'm sure we'll pull through." },
					{ "crashSuggestion", "Now, I want to see what we managed to bring." },
					{ "branchesQuestion", "So we got spoak branches. What was it you wanted them for?" },
					{ "branchesAnswer", "Fences. Arrange them tightly, and they should keep out any quadites." },
					{ "branchesAnswerComeback", "OK." },
					{ "onlyThreeMembersLeftComment", "We need to stick together now." },
					{ "onlyThreeMembersLeftAnswer", "Agreed. This must not happen again." }
				}
			},
			new PersonalityType("Yeboah")
			{
				Adaptability = new NormalDistribution
				{
					Mean = 0.5,
					StandardDeviation = 0.05000000074505806
				},
				Stability = new NormalDistribution
				{
					Mean = 0.800000011920929,
					StandardDeviation = 0.05000000074505806
				},
				Attraction = new SerializableDictionary<string, NormalDistribution> { 
				{
					"playerAllegiance",
					new NormalDistribution
					{
						Mean = 0.30000001192092896,
						StandardDeviation = 0.07999999821186066
					}
				} },
				Principles = new SerializableDictionary<RatingTypes, NormalDistribution>
				{
					{
						RatingTypes.Food,
						new NormalDistribution
						{
							Mean = 0.3499999940395355,
							StandardDeviation = 0.029999999329447746
						}
					},
					{
						RatingTypes.Security,
						new NormalDistribution
						{
							Mean = 0.25,
							StandardDeviation = 0.029999999329447746
						}
					},
					{
						RatingTypes.Comfort,
						new NormalDistribution
						{
							Mean = 0.4000000059604645,
							StandardDeviation = 0.03999999910593033
						}
					}
				},
				SpokenLines = new SerializableDictionary<string, string>
				{
					{ "crashQuestion", "Anyone got hurt in the crash?" },
					{ "crashAnswer", "Nothing serious here." },
					{ "crashConclusion", "Good. Let's keep it that way." },
					{ "crashSuggestion", "Now, we should look through our supplies, see what things survived the crash." },
					{ "branchesQuestion", "We got spoak branches. What was it you wanted them for?" },
					{ "branchesAnswer", "Fences. Arrange them tightly, and they should keep out any quadites." },
					{ "branchesAnswerComeback", "'Should'." },
					{ "onlyThreeMembersLeftComment", "From now on, we have to be extra careful." },
					{ "onlyThreeMembersLeftAnswer", "Yes. We can make it, but only if we don't take unnecessary chances." }
				}
			},
			new PersonalityType("Lehner")
			{
				Adaptability = new NormalDistribution
				{
					Mean = 0.5,
					StandardDeviation = 0.05000000074505806
				},
				Stability = new NormalDistribution
				{
					Mean = 0.800000011920929,
					StandardDeviation = 0.05000000074505806
				},
				Attraction = new SerializableDictionary<string, NormalDistribution> { 
				{
					"playerAllegiance",
					new NormalDistribution
					{
						Mean = 0.30000001192092896,
						StandardDeviation = 0.07999999821186066
					}
				} },
				Principles = new SerializableDictionary<RatingTypes, NormalDistribution>
				{
					{
						RatingTypes.Food,
						new NormalDistribution
						{
							Mean = 0.25,
							StandardDeviation = 0.019999999552965164
						}
					},
					{
						RatingTypes.Security,
						new NormalDistribution
						{
							Mean = 0.3499999940395355,
							StandardDeviation = 0.019999999552965164
						}
					},
					{
						RatingTypes.Comfort,
						new NormalDistribution
						{
							Mean = 0.30000001192092896,
							StandardDeviation = 0.05000000074505806
						}
					}
				},
				SpokenLines = new SerializableDictionary<string, string>
				{
					{ "crashQuestion", "The skimmer took a beating. What about you - are you all ok?" },
					{ "crashAnswer", "Some minor bruising, but I'll be fine." },
					{ "crashConclusion", "Let's hope things stay that way." },
					{ "crashSuggestion", "Now, we should look through our supplies, see what things survived the crash." },
					{ "branchesQuestion", "Here, I have the branches. What was it you wanted them for?" },
					{ "branchesAnswer", "Fences. Arrange them tightly, and they should keep out any quadites." },
					{ "branchesAnswerComeback", "'Should'." },
					{ "onlyThreeMembersLeftComment", "From now on, we have to be extra careful." },
					{ "onlyThreeMembersLeftAnswer", "Yes. We can make it, but only if we don't take unnecessary chances." }
				}
			}
		};
	}
}
